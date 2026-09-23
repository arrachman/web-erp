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
Public Class m6_bom_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_Bom_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m6_bom_history(SELECT 0, bom.* FROM m6_bom bom WHERE bom.bomid = '" & idtransaksi & "')"
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
            sql = "SELECT bomidhistory FROM m6_bom_history WHERE bomid = '" & idtransaksi & "' ORDER BY bommodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY IN --------------------------------------
            sql = "INSERT INTO m6_bom_in_history (SELECT 0, '" & result(4) & "', bom.* FROM m6_bom_in bom WHERE bom.idbom = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY IN -------------------------------


            'PROSES INSERT HISTORY IN --------------------------------------
            sql = "INSERT INTO m6_bom_out_history (SELECT 0, '" & result(4) & "', bom.* FROM m6_bom_out bom WHERE bom.idbom = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY IN -------------------------------


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
    Public Function M6_Bom_HistorySearch(ByVal param As String) As String
        'M6_Bom_HistorySearch --------------------------------------------------------
        'bomidhistory, bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, 
        'bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, 
        'bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, 
        'bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, 
        'bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, 
        'bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, 
        'bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""
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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_bom_v_history")

        dt = AmbilData("aplikasi1-M6_bom_v_history", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("bomid"), 0), sptField,
                     FxDB(dr("bomidhistory"), 0), sptField,
                     FxDB(dr("bomcabang"), ""), sptField,
                     FxDB(dr("bomlokasi"), ""), sptField,
                     FxDB(dr("bomgudangasal"), ""), sptField,
                     FxDB(dr("bomgudangproduksi"), ""), sptField,
                     FxDB(dr("bomgudangtujuan"), ""), sptField,
                     FxDB(dr("bomsumber"), ""), sptField,
                     FxDB(dr("bomjenis"), ""), sptField,
                     FxDB(dr("bomautonotransaksi"), 0), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bomtgl"), ""), formatTgl), sptField,
                     FxDB(dr("bomkodepa"), 0), sptField,
                     FxDB(dr("bompembuat"), 0), sptField,
                     FxDB(dr("bompembuatkontak"), ""), sptField,
                     FxDB(dr("bomestimasikerja"), ""), sptField,
                     FxDB(dr("bommatauang"), ""), sptField,
                     FxDB(dr("bomkurs"), 0), sptField,
                     FxDB(dr("bomtotalhargain"), 0), sptField,
                     FxDB(dr("bomtotalhargaout"), 0), sptField,
                     FxDB(dr("bomtotalhppin"), 0), sptField,
                     FxDB(dr("bomtotalhppout"), 0), sptField,
                     FxDB(dr("bomuraian"), ""), sptField,
                     FxDB(dr("bomcatatan"), ""), sptField,
                     FxDB(dr("bomnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bomtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("bomstatus"), 0), sptField,
                     FxDB(dr("bomstatussebelumnya"), 0), sptField,
                     FxDB(dr("bomjmlrevisi"), 0), sptField,
                     FxDB(dr("bomcetakanke"), 0), sptField,
                     FxDB(dr("bominputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bominputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bommodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bommodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bomposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bompostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bomcabangnama"), ""), sptField,
                     FxDB(dr("bomlokasinama"), ""), sptField,
                     FxDB(dr("bomgudangasalnama"), ""), sptField,
                     FxDB(dr("bomgudangproduksinama"), ""), sptField,
                     FxDB(dr("bomgudangtujuannama"), ""), sptField,
                     FxDB(dr("bomjenisnama"), ""), sptField,
                     FxDB(dr("bompembuatkode"), ""), sptField,
                     FxDB(dr("bompembuatnama"), ""), sptField,
                     FxDB(dr("bomestimasikerjanama"), ""), sptField,
                     FxDB(dr("bomstatusnama"), ""), sptField,
                     FxDB(dr("bomstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("bominputusernama"), ""), sptField,
                     FxDB(dr("bommodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bomidhistory, bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_BomHistoryGetdataById(ByVal param As String) As String
        'M6_BomHistoryGetdataById Utama --------------------------------------------------------
        'bomidhistory, bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, 
        'bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, 
        'bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, 
        'bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, 
        'bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, 
        'bomcustomtext1, bomcustomtext2, bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, 
        'bomcustomint3, bomcustomdbl1, bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3, 
        'bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, 
        'bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama

        'M6_BomHistoryGetdataById In --------------------------------------------------------
        'idhistoryin, idhistory, idbomin, idbom, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, 
        'bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi

        'M6_BomHistoryGetdataById Out --------------------------------------------------------
        'idhistoryout, idhistory, idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'costcenternama, divisinama, subdivisinama, proyeknama, notransaksi

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

        Dim utama As String = "", detail As String = "", detailout As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M6_bom_history~M6_bom_in_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "bomidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "bomidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_bom_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("bomidhistory"), 0), sptField, FxDB(drutama("bomid"), 0), sptField,
                     FxDB(drutama("bomcabang"), ""), sptField,
                     FxDB(drutama("bomlokasi"), ""), sptField,
                     FxDB(drutama("bomgudangasal"), ""), sptField,
                     FxDB(drutama("bomgudangproduksi"), ""), sptField,
                     FxDB(drutama("bomgudangtujuan"), ""), sptField,
                     FxDB(drutama("bomsumber"), ""), sptField,
                     FxDB(drutama("bomjenis"), ""), sptField,
                     FxDB(drutama("bomautonotransaksi"), 0), sptField,
                     FxDB(drutama("bomnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bomtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bomkodepa"), 0), sptField,
                     FxDB(drutama("bompembuat"), 0), sptField,
                     FxDB(drutama("bompembuatkontak"), ""), sptField,
                     FxDB(drutama("bomestimasikerja"), ""), sptField,
                     FxDB(drutama("bommatauang"), ""), sptField,
                     FxDB(drutama("bomkurs"), 0), sptField,
                     FxDB(drutama("bomtotalhargain"), 0), sptField,
                     FxDB(drutama("bomtotalhargaout"), 0), sptField,
                     FxDB(drutama("bomtotalhppin"), 0), sptField,
                     FxDB(drutama("bomtotalhppout"), 0), sptField,
                     FxDB(drutama("bomuraian"), ""), sptField,
                     FxDB(drutama("bomcatatan"), ""), sptField,
                     FxDB(drutama("bomnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bomtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("bomstatus"), 0), sptField,
                     FxDB(drutama("bomstatussebelumnya"), 0), sptField,
                     FxDB(drutama("bomjmlrevisi"), 0), sptField,
                     FxDB(drutama("bomcetakanke"), 0), sptField,
                     FxDB(drutama("bominputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bominputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bommodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bommodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bomposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bompostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bomcustomtext1"), ""), sptField,
                     FxDB(drutama("bomcustomtext2"), ""), sptField,
                     FxDB(drutama("bomcustomtext3"), ""), sptField,
                     FxDB(drutama("bomcustomtext4"), ""), sptField,
                     FxDB(drutama("bomcustomtext5"), ""), sptField,
                     FxDB(drutama("bomcustomint1"), 0), sptField,
                     FxDB(drutama("bomcustomint2"), 0), sptField,
                     FxDB(drutama("bomcustomint3"), 0), sptField,
                     FxDB(drutama("bomcustomdbl1"), 0), sptField,
                     FxDB(drutama("bomcustomdbl2"), 0), sptField,
                     FxDB(drutama("bomcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bomcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bomcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bomcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bomcabangnama"), ""), sptField,
                     FxDB(drutama("bomlokasinama"), ""), sptField,
                     FxDB(drutama("bomgudangasalnama"), ""), sptField,
                     FxDB(drutama("bomgudangproduksinama"), ""), sptField,
                     FxDB(drutama("bomgudangtujuannama"), ""), sptField,
                     FxDB(drutama("bomjenisnama"), ""), sptField,
                     FxDB(drutama("bompembuatkode"), ""), sptField,
                     FxDB(drutama("bompembuatnama"), ""), sptField,
                     FxDB(drutama("bomestimasikerjanama"), ""), sptField,
                     FxDB(drutama("bomstatusnama"), ""), sptField,
                     FxDB(drutama("bomstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("bominputusernama"), ""), sptField,
                     FxDB(drutama("bommodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistoryin"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idbomin"), 0), sptField,
                     FxDB(dr("idbom"), 0), sptField,
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
                     FxDB(dr("hpppersen"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
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
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m6_bom_getdata_out_history")

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Bom_Pack", "idhistory='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailout = String.Concat(detailout, FxDB(dr("idhistoryout"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idbomout"), 0), sptField,
                     FxDB(dr("idbom"), 0), sptField,
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
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
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
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptRow)
            Next
            detailout = detailout.Substring(0, detailout.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, detailout)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bomidhistory, bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, bomcustomtext1, bomcustomtext2, bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, bomcustomint3, bomcustomdbl1, bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3, bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama" & sptSubParam & "idhistoryin, idhistory, idbomin, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi" & sptSubParam & "idhistoryout, idhistory, idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi"))

        Return wsResult
    End Function

End Class
