Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_mr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_Mr_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m3_mr_history(SELECT 0, mr.* FROM m3_mr mr WHERE mr.mrid = '" & idtransaksi & "')"
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
            sql = "SELECT mridhistory FROM m3_mr_history WHERE mrid = '" & idtransaksi & "' ORDER BY mrmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m3_mr_detail_history (SELECT 0, '" & result(4) & "', mr.* FROM m3_mr_detail mr WHERE mr.idmr = '" & idtransaksi & "' )"
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
    Public Function M3_Mr_HistorySearch(ByVal param As String) As String
        'M3_Mr_HistorySearch --------------------------------------------------------
        'mridhistory, mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, 
        'mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, 
        'mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, 
        'mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, 
        'mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcabangnama, mrlokasinama, mrgudangasalnama, 
        'mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, 
        'mrinputusernama, mrmodifikasiusernama

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
            Filter = Filter.Replace("mrdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("mrdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("mrmintakekode", "c2.kkode")
            Filter = Filter.Replace("mrmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_mr_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Mr_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("mrid"), 0), sptField,
                     FxDB(dr("mridhistory"), 0), sptField,
                     FxDB(dr("mrcabang"), ""), sptField,
                     FxDB(dr("mrlokasi"), ""), sptField,
                     FxDB(dr("mrgudangasal"), ""), sptField,
                     FxDB(dr("mrgudangtujuan"), ""), sptField,
                     FxDB(dr("mrsumber"), ""), sptField,
                     FxDB(dr("mrautonotransaksi"), 0), sptField,
                     FxDB(dr("mrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("mrkodepa"), 0), sptField,
                     FxDB(dr("mrdimintaoleh"), 0), sptField,
                     FxDB(dr("mrdimintaolehkontak"), ""), sptField,
                     FxDB(dr("mrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("mruraian"), ""), sptField,
                     FxDB(dr("mrcatatan"), ""), sptField,
                     FxDB(dr("mrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("mrstatusts"), 0), sptField,
                     FxDB(dr("mrstatusrs"), 0), sptField,
                     FxDB(dr("mrstatusrealisasi"), 0), sptField,
                     FxDB(dr("mrstatus"), 0), sptField,
                     FxDB(dr("mrstatussebelumnya"), 0), sptField,
                     FxDB(dr("mrjmlrevisi"), 0), sptField,
                     FxDB(dr("mrcetakanke"), 0), sptField,
                     FxDB(dr("mrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrisclose"), 0), sptField,
                     FxDB(dr("mrcabangnama"), ""), sptField,
                     FxDB(dr("mrlokasinama"), ""), sptField,
                     FxDB(dr("mrgudangasalnama"), ""), sptField,
                     FxDB(dr("mrgudangtujuannama"), ""), sptField,
                     FxDB(dr("mrdimintaolehkode"), ""), sptField,
                     FxDB(dr("mrdimintaolehnama"), ""), sptField,
                     FxDB(dr("mrmintakekode"), ""), sptField,
                     FxDB(dr("mrmintakenama"), ""), sptField,
                     FxDB(dr("mrstatusnama"), ""), sptField,
                     FxDB(dr("mrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("mrinputusernama"), ""), sptField,
                     FxDB(dr("mrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mridhistory, mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcabangnama, mrlokasinama, mrgudangasalnama, mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, mrinputusernama, mrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_MrHistoryGetdataById(ByVal param As String) As String

        'M3_MrHistoryGetdataById Utama --------------------------------------------------------
        'mridhistory, mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, 
        'mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, 
        'mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, 
        'mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, 
        'mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, 
        'mrcustomtext4, mrcustomtext5, mrcustomint1, mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, 
        'mrcustomdbl3, mrcustomdate1, mrcustomdate2, mrcustomdate3, mrcabangnama, mrlokasinama, mrgudangasalnama, 
        'mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, 
        'mrinputusernama, mrmodifikasiusernama

        'M3_MrHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, 
        'stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, 
        'statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, 
        'proyeknama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

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

        Dim NmMemcached As String = "aplikasi1-M3_Mr_history~M3_Mr_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "mridhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "mridhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_mr_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("mridhistory"), 0), sptField,
                     FxDB(drutama("mrid"), 0), sptField,
                     FxDB(drutama("mrcabang"), ""), sptField,
                     FxDB(drutama("mrlokasi"), ""), sptField,
                     FxDB(drutama("mrgudangasal"), ""), sptField,
                     FxDB(drutama("mrgudangtujuan"), ""), sptField,
                     FxDB(drutama("mrsumber"), ""), sptField,
                     FxDB(drutama("mrautonotransaksi"), 0), sptField,
                     FxDB(drutama("mrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("mrkodepa"), 0), sptField,
                     FxDB(drutama("mrdimintaoleh"), 0), sptField,
                     FxDB(drutama("mrdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("mrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("mruraian"), ""), sptField,
                     FxDB(drutama("mrcatatan"), ""), sptField,
                     FxDB(drutama("mrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("mrstatusts"), 0), sptField,
                     FxDB(drutama("mrstatusrs"), 0), sptField,
                     FxDB(drutama("mrstatusrealisasi"), 0), sptField,
                     FxDB(drutama("mrstatus"), 0), sptField,
                     FxDB(drutama("mrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("mrjmlrevisi"), 0), sptField,
                     FxDB(drutama("mrcetakanke"), 0), sptField,
                     FxDB(drutama("mrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrisclose"), 0), sptField,
                     FxDB(drutama("mrcustomtext1"), ""), sptField,
                     FxDB(drutama("mrcustomtext2"), ""), sptField,
                     FxDB(drutama("mrcustomtext3"), ""), sptField,
                     FxDB(drutama("mrcustomtext4"), ""), sptField,
                     FxDB(drutama("mrcustomtext5"), ""), sptField,
                     FxDB(drutama("mrcustomint1"), 0), sptField,
                     FxDB(drutama("mrcustomint2"), 0), sptField,
                     FxDB(drutama("mrcustomint3"), 0), sptField,
                     FxDB(drutama("mrcustomdbl1"), 0), sptField,
                     FxDB(drutama("mrcustomdbl2"), 0), sptField,
                     FxDB(drutama("mrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("mrcabangnama"), ""), sptField,
                     FxDB(drutama("mrlokasinama"), ""), sptField,
                     FxDB(drutama("mrgudangasalnama"), ""), sptField,
                     FxDB(drutama("mrgudangtujuannama"), ""), sptField,
                     FxDB(drutama("mrdimintaolehkode"), ""), sptField,
                     FxDB(drutama("mrdimintaolehnama"), ""), sptField,
                     FxDB(drutama("mrmintakekode"), ""), sptField,
                     FxDB(drutama("mrmintakenama"), ""), sptField,
                     FxDB(drutama("mrstatusnama"), ""), sptField,
                     FxDB(drutama("mrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("mrinputusernama"), ""), sptField,
                     FxDB(drutama("mrmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idmrdetail"), 0), sptField,
                     FxDB(dr("idmr"), 0), sptField,
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
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlts"), 0), sptField,
                     FxDB(dr("statusts"), 0), sptField,
                     FxDB(dr("jmlrs"), 0), sptField,
                     FxDB(dr("statusrs"), 0), sptField,
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
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), 0), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mridhistory, mrid, mrcabang, mrlokasi, mrgudangasal, mrgudangtujuan, mrsumber, mrautonotransaksi, mrnotransaksi, mrtgl, mrkodepa, mrdimintaoleh, mrdimintaolehkontak, mrmintake, mrtgldipakai, mruraian, mrcatatan, mrnoref, mrtglnoref, mrstatusts, mrstatusrs, mrstatusrealisasi, mrstatus, mrstatussebelumnya, mrjmlrevisi, mrcetakanke, mrinputuser, mrinputtgl, mrmodifikasiuser, mrmodifikasitgl, mrposting, mrpostingtgl, mrisclose, mrcustomtext1, mrcustomtext2, mrcustomtext3, mrcustomtext4, mrcustomtext5, mrcustomint1, mrcustomint2, mrcustomint3, mrcustomdbl1, mrcustomdbl2, mrcustomdbl3, mrcustomdate1, mrcustomdate2, mrcustomdate3, mrcabangnama, mrlokasinama, mrgudangasalnama, mrgudangtujuannama, mrdimintaolehkode, mrdimintaolehnama, mrmintakekode, mrmintakenama, mrstatusnama, mrstatussebelumnyanama, mrinputusernama, mrmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, cabangnama, lokasinama, gudangasalnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan"))

        Return wsResult
    End Function


End Class