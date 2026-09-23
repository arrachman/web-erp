Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_asset_category_tax_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_Asset_Category_Tax_HistorySimpan(ByVal param As String) As String
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
        idtransaksi = dataUtama(0)
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m7_asset_category_tax_history(SELECT 0, asset_category_tax.* FROM m7_asset_category_tax asset_category_tax WHERE asset_category_tax.actkode = '" & idtransaksi & "')"
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
    Public Function M7_Asset_Category_Tax_HistorySearch(ByVal param As String) As String
        'M7_Asset_Category_Tax_HistorySearch --------------------------------------------------------
        'actidhistory, actkode, actnama, actmetode, actumur, actpenyusutan, actinputuser, actinputtgl, 
        'actmodifikasiuser, actmodifikasitgl, actcustomtext1, actcustomtext2, actcustomtext3, actcustomtext4, actcustomtext5, 
        'actcustomint1, actcustomint2, actcustomint3, actcustomdbl1, actcustomdbl2, actcustomdbl3, actcustomdate1, 
        'actcustomdate2, actcustomdate3, actmetodenama, actinputusernama, actmodifikasiusernama

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
        sql = "SELECT `act`.`actidhistory` AS `actidhistory`,`act`.`actkode` AS `actkode`,`act`.`actnama` AS `actnama`,`act`.`actmetode` AS `actmetode`,`act`.`actumur` AS `actumur`,`act`.`actpenyusutan` AS `actpenyusutan`,`act`.`actinputuser` AS `actinputuser`,`act`.`actinputtgl` AS `actinputtgl`,`act`.`actmodifikasiuser` AS `actmodifikasiuser`,`act`.`actmodifikasitgl` AS `actmodifikasitgl`,`act`.`actcustomtext1` AS `actcustomtext1`,`act`.`actcustomtext2` AS `actcustomtext2`,`act`.`actcustomtext3` AS `actcustomtext3`,`act`.`actcustomtext4` AS `actcustomtext4`,`act`.`actcustomtext5` AS `actcustomtext5`,`act`.`actcustomint1` AS `actcustomint1`,`act`.`actcustomint2` AS `actcustomint2`,`act`.`actcustomint3` AS `actcustomint3`,`act`.`actcustomdbl1` AS `actcustomdbl1`,`act`.`actcustomdbl2` AS `actcustomdbl2`,`act`.`actcustomdbl3` AS `actcustomdbl3`,`act`.`actcustomdate1` AS `actcustomdate1`,`act`.`actcustomdate2` AS `actcustomdate2`,`act`.`actcustomdate3` AS `actcustomdate3`,`dc`.`nama` AS `actmetodenama`,`u1`.`unama` AS `actinputusernama`,`u2`.`unama` AS `actmodifikasiusernama` from (((`m7_asset_category_tax_history` `act` left join `m7_depreciation_category` `dc` on((`act`.`actmetode` = `dc`.`kode`))) left join `m0_user` `u1` on((`act`.`actinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`act`.`actmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Coa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("actidhistory"), ""), sptField,
                     FxDB(dr("actkode"), ""), sptField,
                     FxDB(dr("actnama"), ""), sptField,
                     FxDB(dr("actmetode"), ""), sptField,
                     FxDB(dr("actumur"), 0), sptField,
                     FxDB(dr("actpenyusutan"), 0), sptField,
                     FxDB(dr("actinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("actinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("actmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("actmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("actcustomtext1"), ""), sptField,
                     FxDB(dr("actcustomtext2"), ""), sptField,
                     FxDB(dr("actcustomtext3"), ""), sptField,
                     FxDB(dr("actcustomtext4"), ""), sptField,
                     FxDB(dr("actcustomtext5"), ""), sptField,
                     FxDB(dr("actcustomint1"), 0), sptField,
                     FxDB(dr("actcustomint2"), 0), sptField,
                     FxDB(dr("actcustomint3"), 0), sptField,
                     FxDB(dr("actcustomdbl1"), 0), sptField,
                     FxDB(dr("actcustomdbl2"), 0), sptField,
                     FxDB(dr("actcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("actcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("actcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("actcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("actmetodenama"), ""), sptField,
                     FxDB(dr("actinputusernama"), ""), sptField,
                     FxDB(dr("actmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Asset Category Tax data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("actidhistory, actkode, actnama, actmetode, actumur, actpenyusutan, actinputuser, actinputtgl, actmodifikasiuser, actmodifikasitgl, actcustomtext1, actcustomtext2, actcustomtext3, actcustomtext4, actcustomtext5, actcustomint1, actcustomint2, actcustomint3, actcustomdbl1, actcustomdbl2, actcustomdbl3, actcustomdate1, actcustomdate2, actcustomdate3, actmetodenama, actinputusernama, actmodifikasiusernama"))

        Return wsResult
    End Function
End Class
