Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_type_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_TypeHistorySimpan(ByVal param As String) As String
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

        Dim idtransaksi As String = ""

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
        'idbarang(1) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'idbarang


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 1) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================

        'idbarang(0) As Integer
        If (Len(dataUtama(0)) = 0) Then
            result(2) = "ptkode can't be empty." : GoTo selesai
        Else
            idtransaksi = dataUtama(0)
        End If
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO M_12_Pos_Type_History(SELECT 0, pt.* FROM M_12_Pos_Type pt WHERE pt.ptkode = '" & idtransaksi & "')"
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
            sql = "SELECT ptidhistory FROM M_12_Pos_Type_History WHERE ptkode = '" & idtransaksi & "' ORDER BY ptmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY ATTENTION -----------------------------------
            sql = "INSERT INTO M_12_Pos_Type_Class_Product_History(SELECT '" & FixQuotes(result(4)) & "', pt.* FROM M_12_Pos_Type_Class_Product pt WHERE pt.tipepos = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY ATTENTION ----------------------------


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
    Public Function M12_Pos_TypeHistorySearch(ByVal param As String) As String
        'M12_Pos_TypeHistorySearch --------------------------------------------------------
        'ptidhistory, ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, 
        'ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, 
        'ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, 
        'ptcustomdate3, ptinputusernama, ptmodifikasiusernama

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
        Dim query As New m0_query
        sql = "select pt.ptidhistory, `pt`.`ptkode` AS `ptkode`,`pt`.`ptnama` AS `ptnama`,`pt`.`ptcatatan` AS `ptcatatan`,`pt`.`ptaktif` AS `ptaktif`,`pt`.`ptinputuser` AS `ptinputuser`,`pt`.`ptinputtgl` AS `ptinputtgl`,`pt`.`ptmodifikasiuser` AS `ptmodifikasiuser`,`pt`.`ptmodifikasitgl` AS `ptmodifikasitgl`,`pt`.`ptcustomtext1` AS `ptcustomtext1`,`pt`.`ptcustomtext2` AS `ptcustomtext2`,`pt`.`ptcustomtext3` AS `ptcustomtext3`,`pt`.`ptcustomtext4` AS `ptcustomtext4`,`pt`.`ptcustomtext5` AS `ptcustomtext5`,`pt`.`ptcustomint1` AS `ptcustomint1`,`pt`.`ptcustomint2` AS `ptcustomint2`,`pt`.`ptcustomint3` AS `ptcustomint3`,`pt`.`ptcustomdbl1` AS `ptcustomdbl1`,`pt`.`ptcustomdbl2` AS `ptcustomdbl2`,`pt`.`ptcustomdbl3` AS `ptcustomdbl3`,`pt`.`ptcustomdate1` AS `ptcustomdate1`,`pt`.`ptcustomdate2` AS `ptcustomdate2`,`pt`.`ptcustomdate3` AS `ptcustomdate3`,`u1`.`unama` AS `ptinputusernama`,`u2`.`unama` AS `ptmodifikasiusernama` from ((`m_12_pos_type_history` `pt` left join `m0_user` `u1` on((`pt`.`ptinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`pt`.`ptmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Type", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ptidhistory"), 0), sptField,
                     FxDB(dr("ptkode"), ""), sptField,
                     FxDB(dr("ptnama"), ""), sptField,
                     FxDB(dr("ptcatatan"), ""), sptField,
                     FxDB(dr("ptaktif"), 0), sptField,
                     FxDB(dr("ptinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ptinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ptmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ptmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ptcustomtext1"), ""), sptField,
                     FxDB(dr("ptcustomtext2"), ""), sptField,
                     FxDB(dr("ptcustomtext3"), ""), sptField,
                     FxDB(dr("ptcustomtext4"), ""), sptField,
                     FxDB(dr("ptcustomtext5"), ""), sptField,
                     FxDB(dr("ptcustomint1"), 0), sptField,
                     FxDB(dr("ptcustomint2"), 0), sptField,
                     FxDB(dr("ptcustomint3"), 0), sptField,
                     FxDB(dr("ptcustomdbl1"), 0), sptField,
                     FxDB(dr("ptcustomdbl2"), 0), sptField,
                     FxDB(dr("ptcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ptinputusernama"), ""), sptField,
                     FxDB(dr("ptmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Type data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptidhistory, ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, ptinputusernama, ptmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_TypeHistoryGetdataById(ByVal param As String) As String

        'M12_Pos_TypeHistoryGetdataById Utama --------------------------------------------------------
        'ptidhistory, ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, 
        'ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, 
        'ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, 
        'ptcustomdate3, ptinputusernama, ptmodifikasiusernama

        'M12_Pos_TypeHistoryGetdataById Detail -------------------------------------------------------
        'idhistory, tipepos, kelasproduk, kelasproduknama

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
        If (IsNumeric(paramSplit(3)) = 0) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M2_Aj~M2_Aj_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ptidhistory = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter = "ptidhistory = '" & idtransaksi & "' and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select pt.ptidhistory, `pt`.`ptkode` AS `ptkode`,`pt`.`ptnama` AS `ptnama`,`pt`.`ptcatatan` AS `ptcatatan`,`pt`.`ptaktif` AS `ptaktif`,`pt`.`ptinputuser` AS `ptinputuser`,`pt`.`ptinputtgl` AS `ptinputtgl`,`pt`.`ptmodifikasiuser` AS `ptmodifikasiuser`,`pt`.`ptmodifikasitgl` AS `ptmodifikasitgl`,`pt`.`ptcustomtext1` AS `ptcustomtext1`,`pt`.`ptcustomtext2` AS `ptcustomtext2`,`pt`.`ptcustomtext3` AS `ptcustomtext3`,`pt`.`ptcustomtext4` AS `ptcustomtext4`,`pt`.`ptcustomtext5` AS `ptcustomtext5`,`pt`.`ptcustomint1` AS `ptcustomint1`,`pt`.`ptcustomint2` AS `ptcustomint2`,`pt`.`ptcustomint3` AS `ptcustomint3`,`pt`.`ptcustomdbl1` AS `ptcustomdbl1`,`pt`.`ptcustomdbl2` AS `ptcustomdbl2`,`pt`.`ptcustomdbl3` AS `ptcustomdbl3`,`pt`.`ptcustomdate1` AS `ptcustomdate1`,`pt`.`ptcustomdate2` AS `ptcustomdate2`,`pt`.`ptcustomdate3` AS `ptcustomdate3`,`u1`.`unama` AS `ptinputusernama`,`u2`.`unama` AS `ptmodifikasiusernama` from `m_12_pos_type_history` `pt` left join `m0_user` `u1` on `pt`.`ptinputuser` = `u1`.`userid` left join `m0_user` `u2` on `pt`.`ptmodifikasiuser` = `u2`.`userid`"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)

            Dim tglinput As String = "", tglmodifikasi As String = "", tglcustom1 As String = "", tglcustom2 As String = "", tglcustom3 As String = ""
            'SET FORMAT TGL
            If Len(FxDB(drutama("ptinputtgl"), "")) > 0 Then tglinput = AsFormatTanggal(FxDB(drutama("ptinputtgl"), ""), formatTglWaktu)
            If Len(FxDB(drutama("ptmodifikasitgl"), "")) > 0 Then tglmodifikasi = AsFormatTanggal(FxDB(drutama("ptmodifikasitgl"), ""), formatTglWaktu)
            If Len(FxDB(drutama("ptcustomdate1"), "")) > 0 Then tglcustom1 = AsFormatTanggal(FxDB(drutama("ptcustomdate1"), ""), formatTgl)
            If Len(FxDB(drutama("ptcustomdate2"), "")) > 0 Then tglcustom2 = AsFormatTanggal(FxDB(drutama("ptcustomdate2"), ""), formatTgl)
            If Len(FxDB(drutama("ptcustomdate3"), "")) > 0 Then tglcustom3 = AsFormatTanggal(FxDB(drutama("ptcustomdate3"), ""), formatTgl)

            utama = String.Concat(
                     FxDB(drutama("ptidhistory"), 0), sptField,
                     FxDB(drutama("ptkode"), ""), sptField,
                     FxDB(drutama("ptnama"), ""), sptField,
                     FxDB(drutama("ptcatatan"), ""), sptField,
                     FxDB(drutama("ptaktif"), 0), sptField,
                     FxDB(drutama("ptinputuser"), ""), sptField,
                     tglinput, sptField,
                     FxDB(drutama("ptmodifikasiuser"), ""), sptField,
                     tglmodifikasi, sptField,
                     FxDB(drutama("ptcustomtext1"), ""), sptField,
                     FxDB(drutama("ptcustomtext2"), ""), sptField,
                     FxDB(drutama("ptcustomtext3"), ""), sptField,
                     FxDB(drutama("ptcustomtext4"), ""), sptField,
                     FxDB(drutama("ptcustomtext5"), ""), sptField,
                     FxDB(drutama("ptcustomint1"), 0), sptField,
                     FxDB(drutama("ptcustomint2"), 0), sptField,
                     FxDB(drutama("ptcustomint3"), 0), sptField,
                     FxDB(drutama("ptcustomdbl1"), 0), sptField,
                     FxDB(drutama("ptcustomdbl2"), 0), sptField,
                     FxDB(drutama("ptcustomdbl3"), 0), sptField,
                     tglcustom1, sptField,
                     tglcustom2, sptField,
                     tglcustom3, sptField,
                     FxDB(drutama("ptinputusernama"), ""), sptField,
                     FxDB(drutama("ptmodifikasiusernama"), ""))

            sql = "SELECT ptcp.idhistory, ptcp.tipepos as tipepos, cp.cpkode as kelasproduk, cp.cpnama as kelasproduknama FROM m1_class_product cp LEFT JOIN m_12_pos_type_class_product_history ptcp ON cp.cpkode = ptcp.kelasproduk AND ptcp.idhistory = 'valkode'"
            sql = sql.Replace("valkode", idtransaksi)
            dt = AmbilData(NmMemcached, "", "ptcp.tipepos DESC, cp.cpkode", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("tipepos"), ""), sptField,
                     FxDB(dr("kelasproduk"), ""), sptField,
                     FxDB(dr("kelasproduknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptidhistory, ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, ptinputusernama, ptmodifikasiusernama" & sptSubParam & "idhistory, tipepos, kelasproduk, kelasproduknama"))

        Return wsResult
    End Function

End Class
