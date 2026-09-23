Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_production_route_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Production_Route_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m1_production_activity_history(SELECT 0, pr.* FROM m1_production_route pr WHERE pr.prid = '" & idtransaksi & "')"
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
            sql = "SELECT pridhistory FROM m1_production_route_history WHERE prid = '" & idtransaksi & "' ORDER BY prmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m1_production_route_detail_history (SELECT 0, '" & result(4) & "', pr.* FROM m1_production_route_detail pr WHERE pr.idpr = '" & idtransaksi & "' )"
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
    Public Function M1_Production_Route_HistorySearch(ByVal param As String) As String
        'M1_Production_ActivitySearch --------------------------------------------------------
        'paidhistory, paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, 
        'pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama

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
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
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
        sql = "SELECT pr.pridhistory, pr.prid, pr.prkode, pr.prnama, pr.prcatatan, pr.praktif, pr.prinputuser, pr.prinputtgl, pr.prmodifikasiuser, pr.prmodifikasitgl, pr.prcustomtext1, pr.prcustomtext2, pr.prcustomtext3, pr.prcustomtext4, pr.prcustomtext5, pr.prcustomint1, pr.prcustomint2, pr.prcustomint3, pr.prcustomdbl1, pr.prcustomdbl2, pr.prcustomdbl3, pr.prcustomdate1, pr.prcustomdate2, pr.prcustomdate3, u1.unama as prinputusernama, u2.unama as prmodifikasiusernama FROM m1_production_route_history pr LEFT JOIN m0_user u1 ON pr.prinputuser = u1.userid LEFT JOIN m0_user u2 ON pr.prmodifikasiuser = u2.userid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Production_Route", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pridhistory"), ""), sptField,
                     FxDB(dr("prid"), ""), sptField,
                     FxDB(dr("prkode"), ""), sptField,
                     FxDB(dr("prnama"), ""), sptField,
                     FxDB(dr("prcatatan"), ""), sptField,
                     FxDB(dr("praktif"), 0), sptField,
                     FxDB(dr("prinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prcustomtext1"), ""), sptField,
                     FxDB(dr("prcustomtext2"), ""), sptField,
                     FxDB(dr("prcustomtext3"), ""), sptField,
                     FxDB(dr("prcustomtext4"), ""), sptField,
                     FxDB(dr("prcustomtext5"), ""), sptField,
                     FxDB(dr("prcustomint1"), 0), sptField,
                     FxDB(dr("prcustomint2"), 0), sptField,
                     FxDB(dr("prcustomint3"), 0), sptField,
                     FxDB(dr("prcustomdbl1"), 0), sptField,
                     FxDB(dr("prcustomdbl2"), 0), sptField,
                     FxDB(dr("prcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("prinputusernama"), ""), sptField,
                     FxDB(dr("prmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Production Route data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pridhistory, prid, prkode, prnama, prcatatan, praktif, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prinputusernama, prmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Production_Route_HistoryGetdataById(ByVal param As String) As String

        'M1_Production_Activity_HistoryGetdataById Utama --------------------------------------------------------
        'paidhistory, paid, pakode, panama, pacatatan, paaktif, painputuser, painputtgl, 
        'pamodifikasiuser, pamodifikasitgl, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, painputusernama, pamodifikasiusernama

        'M1_Production_Activity_HistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idpadetail, idpa, idbarang, 
        'namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, 
        'lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'bkode, bhpp, bjenis, bserial, bbatch, bjmllapangan, bsatuanlapangan, 
        'basset

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

        Dim NmMemcached As String = "aplikasi1-M1_Production_Route~M1_Production_Route_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "prid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "prid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_aj_getdata")
        sql = "SELECT pr.pridhistory, pr.prid, pr.prkode, pr.prnama, pr.prcatatan, pr.praktif, pr.prinputuser, pr.prinputtgl, pr.prmodifikasiuser, pr.prmodifikasitgl, pr.prcustomtext1, pr.prcustomtext2, pr.prcustomtext3, pr.prcustomtext4, pr.prcustomtext5, pr.prcustomint1, pr.prcustomint2, pr.prcustomint3, pr.prcustomdbl1, pr.prcustomdbl2, pr.prcustomdbl3, pr.prcustomdate1, pr.prcustomdate2, pr.prcustomdate3, u1.unama AS prinputusernama, u2.unama AS prmodifikasiusernama, pad.idhistorydetail, pad.idhistory, pad.idprdetail, pad.idpr, pad.idpa, pad.namaaktivitas, pad.kodemesin, pad.costcenter, pad.divisi, pad.subdivisi, pad.proyek, pad.catatan, pad.urutan, pad.customtext1, pad.customtext2, pad.customtext3, pad.customdbl1, pad.customdbl2, pad.customdbl3, pad.customdate1, pad.customdate2, pad.customdate3 FROM m1_production_route_history pr JOIN m1_production_route_detail_history pad ON pr.pridhistory = pad.idhistory LEFT JOIN m0_user u1 ON pr.prinputuser = u1.userid LEFT JOIN m0_user u2 ON pr.prmodifikasiuser = u2.userid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pridhistory"), ""), sptField,
                     FxDB(drutama("prid"), ""), sptField,
                     FxDB(drutama("prkode"), ""), sptField,
                     FxDB(drutama("prnama"), ""), sptField,
                     FxDB(drutama("prcatatan"), ""), sptField,
                     FxDB(drutama("praktif"), 0), sptField,
                     FxDB(drutama("prinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prcustomtext1"), ""), sptField,
                     FxDB(drutama("prcustomtext2"), ""), sptField,
                     FxDB(drutama("prcustomtext3"), ""), sptField,
                     FxDB(drutama("prcustomtext4"), ""), sptField,
                     FxDB(drutama("prcustomtext5"), ""), sptField,
                     FxDB(drutama("prcustomint1"), 0), sptField,
                     FxDB(drutama("prcustomint2"), 0), sptField,
                     FxDB(drutama("prcustomint3"), 0), sptField,
                     FxDB(drutama("prcustomdbl1"), 0), sptField,
                     FxDB(drutama("prcustomdbl2"), 0), sptField,
                     FxDB(drutama("prcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("prinputusernama"), ""), sptField,
                     FxDB(drutama("prmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), ""), sptField,
                     FxDB(dr("idhistory"), ""), sptField,
                     FxDB(dr("idprdetail"), ""), sptField,
                     FxDB(dr("idpr"), ""), sptField,
                     FxDB(dr("idpa"), ""), sptField,
                     FxDB(dr("namaaktivitas"), ""), sptField,
                     FxDB(dr("kodemesin"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pridhistory, prid, prkode, prnama, prcatatan, praktif, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prinputusernama, prmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idprdetail, idpr, idpa, namaaktivitas, kodemesin, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

End Class