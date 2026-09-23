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
Public Class m6_wo_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_Wo_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m6_wo_history(SELECT 0, wo.* FROM m6_wo wo WHERE wo.woid = '" & idtransaksi & "')"
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
            sql = "SELECT woidhistory FROM m6_wo_history WHERE woid = '" & idtransaksi & "' ORDER BY womodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY IN --------------------------------------
            sql = "INSERT INTO m6_wo_in_history (SELECT 0, '" & result(4) & "', wo.* FROM m6_wo_in wo WHERE wo.idwo = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m6_wo_out_history (SELECT 0, '" & result(4) & "', wo.* FROM m6_wo_out wo WHERE wo.idwo = '" & idtransaksi & "' )"
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
    Public Function M6_Wo_HistorySearch(ByVal param As String) As String
        'M6_Wo_HistorySearch --------------------------------------------------------
        'woidhistory, woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, 
        'wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, 
        'womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, 
        'wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, 
        'woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, 
        'wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, 
        'woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocabangnama, 
        'wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, 
        'womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, 
        'woinputusernama, womodifikasiusernama

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
        sql = query.PanggilQuery("m6_wo_v_history")

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("woid"), 0), sptField,
                     FxDB(dr("woidhistory"), 0), sptField,
                     FxDB(dr("wocabang"), ""), sptField,
                     FxDB(dr("wolokasi"), ""), sptField,
                     FxDB(dr("wogudangasal"), ""), sptField,
                     FxDB(dr("wogudangproduksi"), ""), sptField,
                     FxDB(dr("wogudangtujuan"), ""), sptField,
                     FxDB(dr("wosumber"), ""), sptField,
                     FxDB(dr("wojenis"), ""), sptField,
                     FxDB(dr("woautonotransaksi"), 0), sptField,
                     FxDB(dr("wonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("wotgl"), ""), formatTgl), sptField,
                     FxDB(dr("wokodepa"), 0), sptField,
                     FxDB(dr("wodimintaoleh"), 0), sptField,
                     FxDB(dr("wodimintaolehkontak"), ""), sptField,
                     FxDB(dr("womintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("wotgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("woestimasikerja"), ""), sptField,
                     FxDB(dr("womatauang"), ""), sptField,
                     FxDB(dr("wokurs"), 0), sptField,
                     FxDB(dr("wototalhargain"), 0), sptField,
                     FxDB(dr("wototalhargaout"), 0), sptField,
                     FxDB(dr("wototalhppin"), 0), sptField,
                     FxDB(dr("wototalhppout"), 0), sptField,
                     FxDB(dr("wouraian"), ""), sptField,
                     FxDB(dr("wocatatan"), ""), sptField,
                     FxDB(dr("wonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("wotglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("woidbom"), 0), sptField,
                     FxDB(dr("woidpdr"), 0), sptField,
                     FxDB(dr("wostatusmrsin"), 0), sptField,
                     FxDB(dr("wostatusmrsout"), 0), sptField,
                     FxDB(dr("wostatusmrnin"), 0), sptField,
                     FxDB(dr("wostatusmrnout"), 0), sptField,
                     FxDB(dr("wostatuspdin"), 0), sptField,
                     FxDB(dr("wostatuspdout"), 0), sptField,
                     FxDB(dr("wostatusrealisasiin"), 0), sptField,
                     FxDB(dr("wostatusrealisasiout"), 0), sptField,
                     FxDB(dr("wostatus"), 0), sptField,
                     FxDB(dr("wostatussebelumnya"), 0), sptField,
                     FxDB(dr("wojmlrevisi"), 0), sptField,
                     FxDB(dr("wocetakanke"), 0), sptField,
                     FxDB(dr("woinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("woinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("womodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("womodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("woposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("wopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("woisclose"), 0), sptField,
                     FxDB(dr("wocabangnama"), ""), sptField,
                     FxDB(dr("wolokasinama"), ""), sptField,
                     FxDB(dr("wogudangasalnama"), ""), sptField,
                     FxDB(dr("wogudangproduksinama"), ""), sptField,
                     FxDB(dr("wogudangtujuannama"), ""), sptField,
                     FxDB(dr("wojenisnama"), ""), sptField,
                     FxDB(dr("wodimintaolehkode"), ""), sptField,
                     FxDB(dr("wodimintaolehnama"), ""), sptField,
                     FxDB(dr("womintakekode"), ""), sptField,
                     FxDB(dr("womintakenama"), ""), sptField,
                     FxDB(dr("woestimasikerjanama"), ""), sptField,
                     FxDB(dr("wonotransaksibom"), ""), sptField,
                     FxDB(dr("wonotransaksipdr"), ""), sptField,
                     FxDB(dr("wostatusnama"), ""), sptField,
                     FxDB(dr("wostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("woinputusernama"), ""), sptField,
                     FxDB(dr("womodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("woidhistory, woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocabangnama, wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, woinputusernama, womodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_WoHistoryGetdataById(ByVal param As String) As String
        'M6_WoHistoryGetdataById Utama --------------------------------------------------------
        'woidhistory, woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, 
        'wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, 
        'womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, 
        'wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, 
        'woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, 
        'wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, 
        'woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocustomtext1, 
        'wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, wocustomint1, wocustomint2, wocustomint3, 
        'wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, wocustomdate2, wocustomdate3, wocabangnama, 
        'wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, 
        'womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, 
        'woinputusernama, womodifikasiusernama

        'M6_WoHistoryGetdataById In --------------------------------------------------------
        'idhistoryin, idhistory, idwoin, idwo, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, 
        'subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, 
        'jmlsisapd, jmlsisarealisasi

        'M6_WoGetdataById Out --------------------------------------------------------
        'idhistoryout, idhistory, idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, 
        'proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, 
        'jmlsisarealisasi

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

        Dim NmMemcached As String = "aplikasi1-M5_pl~M5_pl_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "woi.statusrealisasi")

            Filter2 = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter2 = Filter2.Replace("statusrealisasi", "woo.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "woidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "woidhistory = " & idtransaksi & " and " & Filter
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idhistory='" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idhistory='" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_wo_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("woidhistory"), 0), sptField, FxDB(drutama("woid"), 0), sptField,
                     FxDB(drutama("wocabang"), ""), sptField,
                     FxDB(drutama("wolokasi"), ""), sptField,
                     FxDB(drutama("wogudangasal"), ""), sptField,
                     FxDB(drutama("wogudangproduksi"), ""), sptField,
                     FxDB(drutama("wogudangtujuan"), ""), sptField,
                     FxDB(drutama("wosumber"), ""), sptField,
                     FxDB(drutama("wojenis"), ""), sptField,
                     FxDB(drutama("woautonotransaksi"), 0), sptField,
                     FxDB(drutama("wonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("wotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("wokodepa"), 0), sptField,
                     FxDB(drutama("wodimintaoleh"), 0), sptField,
                     FxDB(drutama("wodimintaolehkontak"), ""), sptField,
                     FxDB(drutama("womintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("wotgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("woestimasikerja"), ""), sptField,
                     FxDB(drutama("womatauang"), ""), sptField,
                     FxDB(drutama("wokurs"), 0), sptField,
                     FxDB(drutama("wototalhargain"), 0), sptField,
                     FxDB(drutama("wototalhargaout"), 0), sptField,
                     FxDB(drutama("wototalhppin"), 0), sptField,
                     FxDB(drutama("wototalhppout"), 0), sptField,
                     FxDB(drutama("wouraian"), ""), sptField,
                     FxDB(drutama("wocatatan"), ""), sptField,
                     FxDB(drutama("wonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("wotglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("woidbom"), 0), sptField,
                     FxDB(drutama("woidpdr"), 0), sptField,
                     FxDB(drutama("wostatusmrsin"), 0), sptField,
                     FxDB(drutama("wostatusmrsout"), 0), sptField,
                     FxDB(drutama("wostatusmrnin"), 0), sptField,
                     FxDB(drutama("wostatusmrnout"), 0), sptField,
                     FxDB(drutama("wostatuspdin"), 0), sptField,
                     FxDB(drutama("wostatuspdout"), 0), sptField,
                     FxDB(drutama("wostatusrealisasiin"), 0), sptField,
                     FxDB(drutama("wostatusrealisasiout"), 0), sptField,
                     FxDB(drutama("wostatus"), 0), sptField,
                     FxDB(drutama("wostatussebelumnya"), 0), sptField,
                     FxDB(drutama("wojmlrevisi"), 0), sptField,
                     FxDB(drutama("wocetakanke"), 0), sptField,
                     FxDB(drutama("woinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("woinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("womodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("womodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("woposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("wopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("woisclose"), 0), sptField,
                     FxDB(drutama("wocustomtext1"), ""), sptField,
                     FxDB(drutama("wocustomtext2"), ""), sptField,
                     FxDB(drutama("wocustomtext3"), ""), sptField,
                     FxDB(drutama("wocustomtext4"), ""), sptField,
                     FxDB(drutama("wocustomtext5"), ""), sptField,
                     FxDB(drutama("wocustomint1"), 0), sptField,
                     FxDB(drutama("wocustomint2"), 0), sptField,
                     FxDB(drutama("wocustomint3"), 0), sptField,
                     FxDB(drutama("wocustomdbl1"), 0), sptField,
                     FxDB(drutama("wocustomdbl2"), 0), sptField,
                     FxDB(drutama("wocustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("wocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("wocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("wocustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("wocabangnama"), ""), sptField,
                     FxDB(drutama("wolokasinama"), ""), sptField,
                     FxDB(drutama("wogudangasalnama"), ""), sptField,
                     FxDB(drutama("wogudangproduksinama"), ""), sptField,
                     FxDB(drutama("wogudangtujuannama"), ""), sptField,
                     FxDB(drutama("wojenisnama"), ""), sptField,
                     FxDB(drutama("wodimintaolehkode"), ""), sptField,
                     FxDB(drutama("wodimintaolehnama"), ""), sptField,
                     FxDB(drutama("womintakekode"), ""), sptField,
                     FxDB(drutama("womintakenama"), ""), sptField,
                     FxDB(drutama("woestimasikerjanama"), ""), sptField,
                     FxDB(drutama("wonotransaksibom"), ""), sptField,
                     FxDB(drutama("wonotransaksipdr"), ""), sptField,
                     FxDB(drutama("wostatusnama"), ""), sptField,
                     FxDB(drutama("wostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("woinputusernama"), ""), sptField,
                     FxDB(drutama("womodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistoryin"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idwoin"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
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
                     FxDB(dr("idpdrin"), 0), sptField,
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
                     FxDB(dr("pdrnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m6_wo_getdata_out_history")

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Wo_Pack", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailout = String.Concat(detailout, FxDB(dr("idhistoryout"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idwoout"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
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
                     FxDB(dr("idpdrout"), 0), sptField,
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
                     FxDB(dr("pdrnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("woidhistory, woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocustomtext1, wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, wocustomint1, wocustomint2, wocustomint3, wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, wocustomdate2, wocustomdate3, wocabangnama, wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, woinputusernama, womodifikasiusernama" & sptSubParam & "idhistoryin, idhistory, idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi" & sptSubParam & "idhistoryout, idhistory, idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi"))

        Return wsResult
    End Function

End Class
