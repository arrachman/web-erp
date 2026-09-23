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
Public Class m6_pdr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_Pdr_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m6_pdr_history(SELECT 0, pdr.* FROM m6_pdr pdr WHERE pdr.pdrid = '" & idtransaksi & "')"
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
            sql = "SELECT pdridhistory FROM m6_pdr_history WHERE pdrid = '" & idtransaksi & "' ORDER BY pdrmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY IN --------------------------------------
            sql = "INSERT INTO m6_pdr_in_history (SELECT 0, '" & result(4) & "', pdr.* FROM m6_pdr_in pdr WHERE pdr.idpdr = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m6_pdr_out_history (SELECT 0, '" & result(4) & "', pdr.* FROM m6_pdr_out pdr WHERE pdr.idpdr = '" & idtransaksi & "' )"
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
    Public Function M6_Pdr_HistorySearch(ByVal param As String) As String
        'M6_Pdr_HistorySearch --------------------------------------------------------
        'pdridhistory, pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, 
        'pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, 
        'pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, 
        'pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, 
        'pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, 
        'pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, 
        'pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, 
        'pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, 
        'pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, 
        'pdrinputusernama, pdrmodifikasiusernama

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
        sql = query.PanggilQuery("m6_pdr_v_history")

        dt = AmbilData("aplikasi1-m6_pdr_v_history", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("pdrid"), 0), sptField,
                     FxDB(dr("pdridhistory"), 0), sptField,
                     FxDB(dr("pdrcabang"), ""), sptField,
                     FxDB(dr("pdrlokasi"), ""), sptField,
                     FxDB(dr("pdrgudangasal"), ""), sptField,
                     FxDB(dr("pdrgudangproduksi"), ""), sptField,
                     FxDB(dr("pdrgudangtujuan"), ""), sptField,
                     FxDB(dr("pdrsumber"), ""), sptField,
                     FxDB(dr("pdrjenis"), ""), sptField,
                     FxDB(dr("pdrautonotransaksi"), 0), sptField,
                     FxDB(dr("pdrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("pdrkodepa"), 0), sptField,
                     FxDB(dr("pdrdimintaoleh"), 0), sptField,
                     FxDB(dr("pdrdimintaolehkontak"), ""), sptField,
                     FxDB(dr("pdrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("pdrestimasikerja"), ""), sptField,
                     FxDB(dr("pdrmatauang"), ""), sptField,
                     FxDB(dr("pdrkurs"), 0), sptField,
                     FxDB(dr("pdrtotalhargain"), 0), sptField,
                     FxDB(dr("pdrtotalhargaout"), 0), sptField,
                     FxDB(dr("pdrtotalhppin"), 0), sptField,
                     FxDB(dr("pdrtotalhppout"), 0), sptField,
                     FxDB(dr("pdruraian"), ""), sptField,
                     FxDB(dr("pdrcatatan"), ""), sptField,
                     FxDB(dr("pdrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdrtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pdridbom"), 0), sptField,
                     FxDB(dr("pdrstatuswoin"), 0), sptField,
                     FxDB(dr("pdrstatuswoout"), 0), sptField,
                     FxDB(dr("pdrstatusmrsin"), 0), sptField,
                     FxDB(dr("pdrstatusmrsout"), 0), sptField,
                     FxDB(dr("pdrstatusmrnin"), 0), sptField,
                     FxDB(dr("pdrstatusmrnout"), 0), sptField,
                     FxDB(dr("pdrstatuspdin"), 0), sptField,
                     FxDB(dr("pdrstatuspdout"), 0), sptField,
                     FxDB(dr("pdrstatusrealisasiin"), 0), sptField,
                     FxDB(dr("pdrstatusrealisasiout"), 0), sptField,
                     FxDB(dr("pdrstatus"), 0), sptField,
                     FxDB(dr("pdrstatussebelumnya"), 0), sptField,
                     FxDB(dr("pdrjmlrevisi"), 0), sptField,
                     FxDB(dr("pdrcetakanke"), 0), sptField,
                     FxDB(dr("pdrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdrisclose"), 0), sptField,
                     FxDB(dr("pdrcabangnama"), ""), sptField,
                     FxDB(dr("pdrlokasinama"), ""), sptField,
                     FxDB(dr("pdrgudangasalnama"), ""), sptField,
                     FxDB(dr("pdrgudangproduksinama"), ""), sptField,
                     FxDB(dr("pdrgudangtujuannama"), ""), sptField,
                     FxDB(dr("pdrjenisnama"), ""), sptField,
                     FxDB(dr("pdrdimintaolehkode"), ""), sptField,
                     FxDB(dr("pdrdimintaolehnama"), ""), sptField,
                     FxDB(dr("pdrmintakekode"), ""), sptField,
                     FxDB(dr("pdrmintakenama"), ""), sptField,
                     FxDB(dr("pdrestimasikerjanama"), ""), sptField,
                     FxDB(dr("pdrnotransaksibom"), ""), sptField,
                     FxDB(dr("pdrstatusnama"), ""), sptField,
                     FxDB(dr("pdrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pdrinputusernama"), ""), sptField,
                     FxDB(dr("pdrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdridhistory, pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, pdrinputusernama, pdrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdrHistoryGetdataById(ByVal param As String) As String
        'M6_PdrHistoryGetdataById Utama --------------------------------------------------------
        'pdridhistory, pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, 
        'pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, 
        'pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, 
        'pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, 
        'pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, 
        'pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, 
        'pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, 
        'pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, pdrcustomtext5, pdrcustomint1, pdrcustomint2, 
        'pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, pdrcustomdate1, pdrcustomdate2, pdrcustomdate3, 
        'pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, 
        'pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, 
        'pdrinputusernama, pdrmodifikasiusernama

        'M6_PdrHistoryGetdataById In --------------------------------------------------------
        'idhistoryin, idhistory, idpdrin, idpdr, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idbomin, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, 
        'statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, 
        'divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, 
        'jmlsisamrn, jmlsisapd, jmlsisarealisasi

        'M6_PdrHistoryGetdataById Out --------------------------------------------------------
        'idhistoryout, idhistory, idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, 
        'subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, jmlsisamrn, 
        'jmlsisapd, jmlsisarealisasi

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

        Dim NmMemcached As String = "aplikasi1-m6_pl~m6_pl_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "pdri.statusrealisasi")

            Filter2 = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter2 = Filter2.Replace("statusrealisasi", "pdro.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pdridhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pdridhistory = " & idtransaksi & " and " & Filter
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idhistory = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idhistory = '" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_pdr_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pdridhistory"), 0), sptField, FxDB(drutama("pdrid"), 0), sptField,
                     FxDB(drutama("pdrcabang"), ""), sptField,
                     FxDB(drutama("pdrlokasi"), ""), sptField,
                     FxDB(drutama("pdrgudangasal"), ""), sptField,
                     FxDB(drutama("pdrgudangproduksi"), ""), sptField,
                     FxDB(drutama("pdrgudangtujuan"), ""), sptField,
                     FxDB(drutama("pdrsumber"), ""), sptField,
                     FxDB(drutama("pdrjenis"), ""), sptField,
                     FxDB(drutama("pdrautonotransaksi"), 0), sptField,
                     FxDB(drutama("pdrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pdrkodepa"), 0), sptField,
                     FxDB(drutama("pdrdimintaoleh"), 0), sptField,
                     FxDB(drutama("pdrdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("pdrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("pdrestimasikerja"), ""), sptField,
                     FxDB(drutama("pdrmatauang"), ""), sptField,
                     FxDB(drutama("pdrkurs"), 0), sptField,
                     FxDB(drutama("pdrtotalhargain"), 0), sptField,
                     FxDB(drutama("pdrtotalhargaout"), 0), sptField,
                     FxDB(drutama("pdrtotalhppin"), 0), sptField,
                     FxDB(drutama("pdrtotalhppout"), 0), sptField,
                     FxDB(drutama("pdruraian"), ""), sptField,
                     FxDB(drutama("pdrcatatan"), ""), sptField,
                     FxDB(drutama("pdrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pdridbom"), 0), sptField,
                     FxDB(drutama("pdrstatuswoin"), 0), sptField,
                     FxDB(drutama("pdrstatuswoout"), 0), sptField,
                     FxDB(drutama("pdrstatusmrsin"), 0), sptField,
                     FxDB(drutama("pdrstatusmrsout"), 0), sptField,
                     FxDB(drutama("pdrstatusmrnin"), 0), sptField,
                     FxDB(drutama("pdrstatusmrnout"), 0), sptField,
                     FxDB(drutama("pdrstatuspdin"), 0), sptField,
                     FxDB(drutama("pdrstatuspdout"), 0), sptField,
                     FxDB(drutama("pdrstatusrealisasiin"), 0), sptField,
                     FxDB(drutama("pdrstatusrealisasiout"), 0), sptField,
                     FxDB(drutama("pdrstatus"), 0), sptField,
                     FxDB(drutama("pdrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pdrjmlrevisi"), 0), sptField,
                     FxDB(drutama("pdrcetakanke"), 0), sptField,
                     FxDB(drutama("pdrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdrisclose"), 0), sptField,
                     FxDB(drutama("pdrcustomtext1"), ""), sptField,
                     FxDB(drutama("pdrcustomtext2"), ""), sptField,
                     FxDB(drutama("pdrcustomtext3"), ""), sptField,
                     FxDB(drutama("pdrcustomtext4"), ""), sptField,
                     FxDB(drutama("pdrcustomtext5"), ""), sptField,
                     FxDB(drutama("pdrcustomint1"), 0), sptField,
                     FxDB(drutama("pdrcustomint2"), 0), sptField,
                     FxDB(drutama("pdrcustomint3"), 0), sptField,
                     FxDB(drutama("pdrcustomdbl1"), 0), sptField,
                     FxDB(drutama("pdrcustomdbl2"), 0), sptField,
                     FxDB(drutama("pdrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pdrcabangnama"), ""), sptField,
                     FxDB(drutama("pdrlokasinama"), ""), sptField,
                     FxDB(drutama("pdrgudangasalnama"), ""), sptField,
                     FxDB(drutama("pdrgudangproduksinama"), ""), sptField,
                     FxDB(drutama("pdrgudangtujuannama"), ""), sptField,
                     FxDB(drutama("pdrjenisnama"), ""), sptField,
                     FxDB(drutama("pdrdimintaolehkode"), ""), sptField,
                     FxDB(drutama("pdrdimintaolehnama"), ""), sptField,
                     FxDB(drutama("pdrmintakekode"), ""), sptField,
                     FxDB(drutama("pdrmintakenama"), ""), sptField,
                     FxDB(drutama("pdrestimasikerjanama"), ""), sptField,
                     FxDB(drutama("pdrnotransaksibom"), ""), sptField,
                     FxDB(drutama("pdrstatusnama"), ""), sptField,
                     FxDB(drutama("pdrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pdrinputusernama"), ""), sptField,
                     FxDB(drutama("pdrmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistoryin"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idpdrin"), 0), sptField,
                     FxDB(dr("idpdr"), 0), sptField,
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
                     FxDB(dr("idbomin"), 0), sptField,
                     FxDB(dr("jmlwo"), 0), sptField,
                     FxDB(dr("statuswo"), 0), sptField,
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
                     FxDB(dr("jmlpd"), 0), sptField,
                     FxDB(dr("statuspd"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisawo"), 0), sptField,
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m6_pdr_getdata_out_history")

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Pdr_Pack", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailout = String.Concat(detailout, FxDB(dr("idhistoryout"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpdrout"), 0), sptField,
                     FxDB(dr("idpdr"), 0), sptField,
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
                     FxDB(dr("idbomout"), 0), sptField,
                     FxDB(dr("jmlwo"), 0), sptField,
                     FxDB(dr("statuswo"), 0), sptField,
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
                     FxDB(dr("jmlpd"), 0), sptField,
                     FxDB(dr("statuspd"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisawo"), 0), sptField,
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            If detailout.Length > 0 Then detailout = detailout.Substring(0, detailout.Length - sptRow.Length)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdridhistory, pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, pdrcustomtext5, pdrcustomint1, pdrcustomint2, pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, pdrcustomdate1, pdrcustomdate2, pdrcustomdate3, pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, pdrinputusernama, pdrmodifikasiusernama" & sptSubParam & "idhistoryin, idhistory, idpdrin, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi" & sptSubParam & "idhistoryout, idhistory, idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi"))

        Return wsResult
    End Function

End Class
