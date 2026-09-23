Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_cd_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Cd_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_cd_history(SELECT 0, cd.* FROM m2_cd cd WHERE cd.cdid = '" & idtransaksi & "')"
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
            sql = "SELECT cdidhistory FROM m2_cd_history WHERE cdid = '" & idtransaksi & "' ORDER BY cdmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_cd_detail_history (SELECT 0, '" & result(4) & "', cd.* FROM m2_cd_detail cd WHERE cd.idcd = '" & idtransaksi & "' )"
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
        'myConn.Close()
        'myConn = Nothing
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
    Public Function M2_Cd_HistorySearch(ByVal param As String) As String
        'M2_Cd_HistorySearch --------------------------------------------------------
        'cdidhistory, cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, 
        'cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, 
        'cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, 
        'cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, 
        'cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcabangnama, cdlokasinama, cdkontakkode, 
        'cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama

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
            Filter = Filter.Replace("cdkontakkode", "c.kkode")
            Filter = Filter.Replace("cdkontaknama", "c.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cd_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cd_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("cdid"), 0), sptField,
                    FxDB(dr("cdidhistory"), ""), sptField,
                    FxDB(dr("cdcabang"), ""), sptField,
                    FxDB(dr("cdlokasi"), ""), sptField,
                    FxDB(dr("cdsumber"), ""), sptField,
                    FxDB(dr("cdautonotransaksi"), 0), sptField,
                    FxDB(dr("cdnotransaksi"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("cdtgl"), ""), formatTgl), sptField,
                    FxDB(dr("cdkodepa"), ""), sptField,
                    FxDB(dr("cdkontak"), ""), sptField,
                    FxDB(dr("cdkontakperson"), ""), sptField,
                    FxDB(dr("cdnorek"), ""), sptField,
                    FxDB(dr("cduraian"), ""), sptField,
                    FxDB(dr("cdcatatan"), ""), sptField,
                    FxDB(dr("cdmatauang"), ""), sptField,
                    FxDB(dr("cdkurs"), 0), sptField,
                    FxDB(dr("cdjumlah"), 0), sptField,
                    FxDB(dr("cdjumlahvalas"), 0), sptField,
                    FxDB(dr("cdjumlahbayar"), 0), sptField,
                    FxDB(dr("cdjumlahbayarvalas"), 0), sptField,
                    FxDB(dr("cdstatusbayar"), 0), sptField,
                    AsFormatTanggal(FxDB(dr("cdtgllunas"), ""), formatTgl), sptField,
                    FxDB(dr("cdstatus"), 0), sptField,
                    FxDB(dr("cdstatussebelumnya"), 0), sptField,
                    FxDB(dr("cdjmlrevisi"), 0), sptField,
                    FxDB(dr("cdcetakanke"), 0), sptField,
                    FxDB(dr("cdisclose"), 0), sptField,
                    FxDB(dr("cdinputuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("cdinputtgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("cdmodifikasiuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("cdmodifikasitgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("cdposting"), 0), sptField,
                    AsFormatTanggal(FxDB(dr("cdpostingtgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("cdcabangnama"), ""), sptField,
                    FxDB(dr("cdlokasinama"), ""), sptField,
                    FxDB(dr("cdkontakkode"), ""), sptField,
                    FxDB(dr("cdkontaknama"), ""), sptField,
                    FxDB(dr("cdnoreknama"), ""), sptField,
                    FxDB(dr("cdstatusnama"), ""), sptField,
                    FxDB(dr("cdstatussebelumnyanama"), ""), sptField,
                    FxDB(dr("cdinputusernama"), ""), sptField,
                    FxDB(dr("cdmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cdidhistory ,cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcabangnama, cdlokasinama, cdkontakkode, cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CdHistoryGetdataById(ByVal param As String) As String

        'M2_CdGetdataById Utama --------------------------------------------------------
        'cdidhistory, cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, 
        'cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, 
        'cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, 
        'cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, 
        'cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcustomtext1, cdcustomtext2, 
        'cdcustomtext3, cdcustomtext4, cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, 
        'cdcustomdbl2, cdcustomdbl3, cdcustomdate1, cdcustomdate2, cdcustomdate3

        'M2_CdGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idcddetail, idcd, norek, matauang, kurs, jumlah, 
        'jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, 
        'noreknama, costcenternama, divisinama, subdivisinama, proyeknama

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

        Dim NmMemcached As String = "aplikasi1-M2_Cd_History~M2_Cd_Detail_History-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "cdidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "cdidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cd_h_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("cdidhistory"), 0), sptField,
                     FxDB(drutama("cdid"), ""), sptField,
                     FxDB(drutama("cdcabang"), ""), sptField,
                     FxDB(drutama("cdlokasi"), ""), sptField,
                     FxDB(drutama("cdsumber"), ""), sptField,
                     FxDB(drutama("cdautonotransaksi"), 0), sptField,
                     FxDB(drutama("cdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cdtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("cdkodepa"), ""), sptField,
                     FxDB(drutama("cdkontak"), ""), sptField,
                     FxDB(drutama("cdkontakperson"), ""), sptField,
                     FxDB(drutama("cdnorek"), ""), sptField,
                     FxDB(drutama("cduraian"), ""), sptField,
                     FxDB(drutama("cdcatatan"), ""), sptField,
                     FxDB(drutama("cdmatauang"), ""), sptField,
                     FxDB(drutama("cdkurs"), 0), sptField,
                     FxDB(drutama("cdjumlah"), 0), sptField,
                     FxDB(drutama("cdjumlahvalas"), 0), sptField,
                     FxDB(drutama("cdjumlahbayar"), 0), sptField,
                     FxDB(drutama("cdjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("cdstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("cdstatus"), 0), sptField,
                     FxDB(drutama("cdstatussebelumnya"), 0), sptField,
                     FxDB(drutama("cdjmlrevisi"), 0), sptField,
                     FxDB(drutama("cdcetakanke"), 0), sptField,
                     FxDB(drutama("cdisclose"), 0), sptField,
                     FxDB(drutama("cdinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cdmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cdcustomtext1"), ""), sptField,
                     FxDB(drutama("cdcustomtext2"), ""), sptField,
                     FxDB(drutama("cdcustomtext3"), ""), sptField,
                     FxDB(drutama("cdcustomtext4"), ""), sptField,
                     FxDB(drutama("cdcustomtext5"), ""), sptField,
                     FxDB(drutama("cdcustomint1"), 0), sptField,
                     FxDB(drutama("cdcustomint2"), 0), sptField,
                     FxDB(drutama("cdcustomint3"), 0), sptField,
                     FxDB(drutama("cdcustomdbl1"), 0), sptField,
                     FxDB(drutama("cdcustomdbl2"), 0), sptField,
                     FxDB(drutama("cdcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cdcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cdcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cdcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("cdcabangnama"), ""), sptField,
                     FxDB(drutama("cdlokasinama"), ""), sptField,
                     FxDB(drutama("cdkontakkode"), ""), sptField,
                     FxDB(drutama("cdkontaknama"), ""), sptField,
                     FxDB(drutama("cdnoreknama"), ""), sptField,
                     FxDB(drutama("cdstatusnama"), ""), sptField,
                     FxDB(drutama("cdstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("cdinputusernama"), ""), sptField,
                     FxDB(drutama("cdmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idcddetail"), 0), sptField,
                     FxDB(dr("idcd"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
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
                     FxDB(dr("noreknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cdidhistory, cdid, cdcabang, cdlokasi, cdsumber, cdautonotransaksi, cdnotransaksi, cdtgl, cdkodepa, cdkontak, cdkontakperson, cdnorek, cduraian, cdcatatan, cdmatauang, cdkurs, cdjumlah, cdjumlahvalas, cdjumlahbayar, cdjumlahbayarvalas, cdstatusbayar, cdtgllunas, cdstatus, cdstatussebelumnya, cdjmlrevisi, cdcetakanke, cdisclose, cdinputuser, cdinputtgl, cdmodifikasiuser, cdmodifikasitgl, cdposting, cdpostingtgl, cdcustomtext1, cdcustomtext2, cdcustomtext3, cdcustomtext4, cdcustomtext5, cdcustomint1, cdcustomint2, cdcustomint3, cdcustomdbl1, cdcustomdbl2, cdcustomdbl3, cdcustomdate1, cdcustomdate2, cdcustomdate3, cdcabangnama, cdlokasinama, cdkontakkode, cdkontaknama, cdnoreknama, cdstatusnama, cdstatussebelumnyanama, cdinputusernama, cdmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idcddetail, idcd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function


End Class