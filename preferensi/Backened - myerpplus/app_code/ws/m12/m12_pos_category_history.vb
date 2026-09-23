Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_category_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Category_HistorySimpan(ByVal param As String) As String
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
        'pckode(0) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pckode


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 1) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================

        'pckode(0) As String
        idtransaksi = dataUtama(0)
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO M_12_Pos_Category_history(SELECT 0, pc.* FROM M_12_Pos_Category pc WHERE pc.pckode = '" & idtransaksi & "')"
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
    Public Function M12_Pos_Category_HistorySearch(ByVal param As String) As String
        'M12_Pos_Category_HistorySearch --------------------------------------------------------
        'pcidhistory, pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, 
        'pcmodifikasiuser, pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, 
        'pccustomint1, pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, 
        'pccustomdate2, pccustomdate3, pcinputusernama, pcmodifikasiusernama, pctipepos, pcindeksharga, pctipeposnama, pcindeksharganama

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
        sql = "select `pc`.`pcidhistory` AS `pcidhistory`, `pc`.`pckode` AS `pckode`, `pc`.`pcnama` AS `pcnama`, `pc`.`pccatatan` AS `pccatatan`, `pc`.`pcaktif` AS `pcaktif`, `pc`.`pcinputuser` AS `pcinputuser`, `pc`.`pcinputtgl` AS `pcinputtgl`, `pc`.`pcmodifikasiuser` AS `pcmodifikasiuser`, `pc`.`pcmodifikasitgl` AS `pcmodifikasitgl`, `pc`.`pccustomtext1` AS `pccustomtext1`, `pc`.`pccustomtext2` AS `pccustomtext2`, `pc`.`pccustomtext3` AS `pccustomtext3`, `pc`.`pccustomtext4` AS `pccustomtext4`, `pc`.`pccustomtext5` AS `pccustomtext5`, `pc`.`pccustomint1` AS `pccustomint1`, `pc`.`pccustomint2` AS `pccustomint2`, `pc`.`pccustomint3` AS `pccustomint3`, `pc`.`pccustomdbl1` AS `pccustomdbl1`, `pc`.`pccustomdbl2` AS `pccustomdbl2`, `pc`.`pccustomdbl3` AS `pccustomdbl3`, `pc`.`pccustomdate1` AS `pccustomdate1`, `pc`.`pccustomdate2` AS `pccustomdate2`, `pc`.`pccustomdate3` AS `pccustomdate3`, `u1`.`unama` AS `pcinputusernama`, `u2`.`unama` AS `pcmodifikasiusernama`, pc.pctipepos, pc.pcindeksharga, pt.ptnama as pctipeposnama, ip.ipnama as pcindeksharganama from `m_12_pos_category_history` `pc` left join `m0_user` `u1` on `pc`.`pcinputuser` = `u1`.`userid` left join `m0_user` `u2` on `pc`.`pcmodifikasiuser` = `u2`.`userid` left join m_12_pos_type pt on pc.pctipepos = pt.ptkode left join m1_index_price ip on pc.pcindeksharga = ip.ipkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Category_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("pcidhistory"), ""), sptField,
                     FxDB(dr("pckode"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("pccatatan"), ""), sptField,
                     FxDB(dr("pcaktif"), 0), sptField,
                     FxDB(dr("pcinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pcmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pccustomtext1"), ""), sptField,
                     FxDB(dr("pccustomtext2"), ""), sptField,
                     FxDB(dr("pccustomtext3"), ""), sptField,
                     FxDB(dr("pccustomtext4"), ""), sptField,
                     FxDB(dr("pccustomtext5"), ""), sptField,
                     FxDB(dr("pccustomint1"), 0), sptField,
                     FxDB(dr("pccustomint2"), 0), sptField,
                     FxDB(dr("pccustomint3"), 0), sptField,
                     FxDB(dr("pccustomdbl1"), 0), sptField,
                     FxDB(dr("pccustomdbl2"), 0), sptField,
                     FxDB(dr("pccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pccustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcinputusernama"), ""), sptField,
                     FxDB(dr("pcmodifikasiusernama"), ""), sptField,
                     FxDB(dr("pctipepos"), ""), sptField,
                     FxDB(dr("pcindeksharga"), ""), sptField,
                     FxDB(dr("pctipeposnama"), ""), sptField,
                     FxDB(dr("pcindeksharganama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Pos Category data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcidhistory, pckode, pcnama, pccatatan, pcaktif, pcinputuser, pcinputtgl, pcmodifikasiuser, pcmodifikasitgl, pccustomtext1, pccustomtext2, pccustomtext3, pccustomtext4, pccustomtext5, pccustomint1, pccustomint2, pccustomint3, pccustomdbl1, pccustomdbl2, pccustomdbl3, pccustomdate1, pccustomdate2, pccustomdate3, pcinputusernama, pcmodifikasiusernama, pctipepos, pcindeksharga, pctipeposnama, pcindeksharganama"))

        Return wsResult
    End Function

End Class
