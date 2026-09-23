Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_subdepartment_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_SubdepartmentHistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M1_Subdepartment_history(SELECT 0, Subdepartment.* FROM M1_Subdepartment Subdepartment WHERE Subdepartment.sdpkode = '" & idtransaksi & "')"
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
    Public Function M1_SubdepartmentHistorySearch(ByVal param As String) As String
        'M1_SubdepartmentHistorySearch --------------------------------------------------------
        'sdpidhistory, sdpkode, sdpnama, sdpdepartemen, sdpdivisi, sdpsubdivisi, sdpcatatan, sdpaktif, 
        'sdpinputuser, sdpinputtgl, sdpmodifikasiuser, sdpmodifikasitgl, sdpcustomtext1, sdpcustomtext2, sdpcustomtext3, 
        'sdpcustomtext4, sdpcustomtext5, sdpcustomint1, sdpcustomint2, sdpcustomint3, sdpcustomdbl1, sdpcustomdbl2, 
        'sdpcustomdbl3, sdpcustomdate1, sdpcustomdate2, sdpcustomdate3, sdpdepartemennama, sdpdivisinama, sdpsubdivisinama, 
        'sdpinputusernama, sdpmodifikasiusernama, sdpindexbarcode

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
        sql = "SELECT sdp.sdpidhistory, sdp.sdpkode, sdp.sdpnama, sdp.sdpdepartemen, sdp.sdpdivisi, sdp.sdpsubdivisi, sdp.sdpcatatan, sdp.sdpaktif, sdp.sdpinputuser, sdp.sdpinputtgl, sdp.sdpmodifikasiuser, sdp.sdpmodifikasitgl, sdp.sdpcustomtext1, sdp.sdpcustomtext2, sdp.sdpcustomtext3, sdp.sdpcustomtext4, sdp.sdpcustomtext5, sdp.sdpcustomint1, sdp.sdpcustomint2, sdp.sdpcustomint3, sdp.sdpcustomdbl1, sdp.sdpcustomdbl2, sdp.sdpcustomdbl3, sdp.sdpcustomdate1, sdp.sdpcustomdate2, sdp.sdpcustomdate3, dp.dpnama as sdpdepartemennama, d.dnama as sdpdivisinama, sd.sdnama as sdpsubdivisinama, u1.unama as sdpinputusernama, u2.unama as sdpmodifikasiusernama, sdp.sdpindexbarcode FROM m1_subdepartment_history sdp LEFT JOIN m1_department dp ON sdp.sdpdepartemen = dp.dpkode LEFT JOIN m1_subdivision sd ON sdp.sdpsubdivisi = sd.sdkode LEFT JOIN m1_division d ON sdp.sdpdivisi = d.dkode LEFT JOIN m0_user u1 ON sdp.sdpinputuser = u1.userid LEFT JOIN m0_user u2 ON sdp.sdpmodifikasiuser = u2.userid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Subdepartment_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sdpidhistory"), ""), sptField,
                     FxDB(dr("sdpkode"), ""), sptField,
                     FxDB(dr("sdpnama"), ""), sptField,
                     FxDB(dr("sdpdepartemen"), ""), sptField,
                     FxDB(dr("sdpdivisi"), ""), sptField,
                     FxDB(dr("sdpsubdivisi"), ""), sptField,
                     FxDB(dr("sdpcatatan"), ""), sptField,
                     FxDB(dr("sdpaktif"), 0), sptField,
                     FxDB(dr("sdpinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sdpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sdpmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sdpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sdpcustomtext1"), ""), sptField,
                     FxDB(dr("sdpcustomtext2"), ""), sptField,
                     FxDB(dr("sdpcustomtext3"), ""), sptField,
                     FxDB(dr("sdpcustomtext4"), ""), sptField,
                     FxDB(dr("sdpcustomtext5"), ""), sptField,
                     FxDB(dr("sdpcustomint1"), 0), sptField,
                     FxDB(dr("sdpcustomint2"), 0), sptField,
                     FxDB(dr("sdpcustomint3"), 0), sptField,
                     FxDB(dr("sdpcustomdbl1"), 0), sptField,
                     FxDB(dr("sdpcustomdbl2"), 0), sptField,
                     FxDB(dr("sdpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sdpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sdpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sdpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("sdpdepartemennama"), ""), sptField,
                     FxDB(dr("sdpdivisinama"), ""), sptField,
                     FxDB(dr("sdpsubdivisinama"), ""), sptField,
                     FxDB(dr("sdpinputusernama"), ""), sptField,
                     FxDB(dr("sdpmodifikasiusernama"), ""), sptField,
                     FxDB(dr("sdpindexbarcode"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Subdepartment History data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sdpidhistory, sdpkode, sdpnama, sdpdepartemen, sdpdivisi, sdpsubdivisi, sdpcatatan, sdpaktif, sdpinputuser, sdpinputtgl, sdpmodifikasiuser, sdpmodifikasitgl, sdpcustomtext1, sdpcustomtext2, sdpcustomtext3, sdpcustomtext4, sdpcustomtext5, sdpcustomint1, sdpcustomint2, sdpcustomint3, sdpcustomdbl1, sdpcustomdbl2, sdpcustomdbl3, sdpcustomdate1, sdpcustomdate2, sdpcustomdate3, sdpdepartemennama, sdpdivisinama, sdpsubdivisinama, sdpinputusernama, sdpmodifikasiusernama, sdpindexbarcode"))

        Return wsResult
    End Function

End Class
