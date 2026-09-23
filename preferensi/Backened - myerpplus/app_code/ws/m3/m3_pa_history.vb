Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_pa_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_Pa_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m3_pa_history(SELECT 0, pa.* FROM m3_pa pa WHERE pa.paid = '" & idtransaksi & "')"
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
            sql = "SELECT paidhistory FROM m3_pa_history WHERE paid = '" & idtransaksi & "' ORDER BY pamodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m3_pa_detail_history (SELECT 0, '" & result(4) & "', pa.* FROM m3_pa_detail pa WHERE pa.idpa = '" & idtransaksi & "' )"
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
    Public Function M3_Pa_HistorySearch(ByVal param As String) As String
        'M3_Pa_HistorySearch --------------------------------------------------------
        'paidhistory, paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, 
        'patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, 
        'pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, 
        'pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, 
        'patutupperiode, paisclose, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, 
        'pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama,
        'pakategori, pakategorinama, pakategoriharga, pakategoriharganama

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
            Filter = Filter.Replace("pabagianpakode", "c1.kkode")
            Filter = Filter.Replace("pabagianpanama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_pa_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Pa_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("paid"), 0), sptField,
                     FxDB(dr("paidhistory"), 0), sptField,
                     FxDB(dr("pacabang"), ""), sptField,
                     FxDB(dr("palokasi"), ""), sptField,
                     FxDB(dr("pagudang"), ""), sptField,
                     FxDB(dr("pasumber"), ""), sptField,
                     FxDB(dr("paautonotransaksi"), 0), sptField,
                     FxDB(dr("panotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("patgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("patglberlakusampai"), ""), formatTgl), sptField,
                     FxDB(dr("pakodepa"), 0), sptField,
                     FxDB(dr("pabagianpa"), 0), sptField,
                     FxDB(dr("pabagianpakontak"), ""), sptField,
                     FxDB(dr("pamatauang"), ""), sptField,
                     FxDB(dr("pakurs"), 0), sptField,
                     FxDB(dr("pauraian"), ""), sptField,
                     FxDB(dr("pacatatan"), ""), sptField,
                     FxDB(dr("panoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("patglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pastatus"), 0), sptField,
                     FxDB(dr("pastatussebelumnya"), 0), sptField,
                     FxDB(dr("pajmlrevisi"), 0), sptField,
                     FxDB(dr("pacetakanke"), 0), sptField,
                     FxDB(dr("painputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("painputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pamodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("paposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("papostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("patutupperiode"), 0), sptField,
                     FxDB(dr("paisclose"), 0), sptField,
                     FxDB(dr("pacabangnama"), ""), sptField,
                     FxDB(dr("palokasinama"), ""), sptField,
                     FxDB(dr("pagudangnama"), ""), sptField,
                     FxDB(dr("pabagianpakode"), ""), sptField,
                     FxDB(dr("pabagianpanama"), ""), sptField,
                     FxDB(dr("pastatusnama"), ""), sptField,
                     FxDB(dr("pastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("painputusernama"), ""), sptField,
                     FxDB(dr("pamodifikasiusernama"), ""), sptField,
                     FxDB(dr("pakategori"), 0), sptField,
                     FxDB(dr("pakategorinama"), ""), sptField,
                     FxDB(dr("pakategoriharga"), ""), sptField,
                     FxDB(dr("pakategoriharganama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paidhistory, paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, patutupperiode, paisclose, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama, pakategori, pakategorinama, pakategoriharga, pakategoriharganama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_PaHistoryGetdataById(ByVal param As String) As String

        'M3_PaHistoryGetdataById Utama --------------------------------------------------------
        'paidhistory, paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, 
        'patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, 
        'pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, 
        'pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, 
        'patutupperiode, paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, 
        'pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama,
        'pakategori, pakategorinama, pakategoriharga, pakategoriharganama

        'M3_PaHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idpadetail, idpa, idbarang, 
        'satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, 
        'hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, 
        'hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, 
        'diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, bhargabeli,
        'kontak, kontakkode, kontaknama


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

        Dim NmMemcached As String = "aplikasi1-M3_Pa_history~M3_Pa_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "paidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "paidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_pa_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("paidhistory"), 0), sptField,
                     FxDB(drutama("paid"), 0), sptField,
                     FxDB(drutama("pacabang"), ""), sptField,
                     FxDB(drutama("palokasi"), ""), sptField,
                     FxDB(drutama("pagudang"), ""), sptField,
                     FxDB(drutama("pasumber"), ""), sptField,
                     FxDB(drutama("paautonotransaksi"), 0), sptField,
                     FxDB(drutama("panotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("patgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("patglberlakusampai"), ""), formatTgl), sptField,
                     FxDB(drutama("pakodepa"), 0), sptField,
                     FxDB(drutama("pabagianpa"), 0), sptField,
                     FxDB(drutama("pabagianpakontak"), ""), sptField,
                     FxDB(drutama("pamatauang"), ""), sptField,
                     FxDB(drutama("pakurs"), 0), sptField,
                     FxDB(drutama("pauraian"), ""), sptField,
                     FxDB(drutama("pacatatan"), ""), sptField,
                     FxDB(drutama("panoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("patglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pastatus"), 0), sptField,
                     FxDB(drutama("pastatussebelumnya"), 0), sptField,
                     FxDB(drutama("pajmlrevisi"), 0), sptField,
                     FxDB(drutama("pacetakanke"), 0), sptField,
                     FxDB(drutama("painputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("painputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pamodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("paposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("papostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("patutupperiode"), 0), sptField,
                     FxDB(drutama("paisclose"), 0), sptField,
                     FxDB(drutama("pacustomtext1"), ""), sptField,
                     FxDB(drutama("pacustomtext2"), ""), sptField,
                     FxDB(drutama("pacustomtext3"), ""), sptField,
                     FxDB(drutama("pacustomtext4"), ""), sptField,
                     FxDB(drutama("pacustomtext5"), ""), sptField,
                     FxDB(drutama("pacustomint1"), 0), sptField,
                     FxDB(drutama("pacustomint2"), 0), sptField,
                     FxDB(drutama("pacustomint3"), 0), sptField,
                     FxDB(drutama("pacustomdbl1"), 0), sptField,
                     FxDB(drutama("pacustomdbl2"), 0), sptField,
                     FxDB(drutama("pacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pacabangnama"), ""), sptField,
                     FxDB(drutama("palokasinama"), ""), sptField,
                     FxDB(drutama("pagudangnama"), ""), sptField,
                     FxDB(drutama("pabagianpakode"), ""), sptField,
                     FxDB(drutama("pabagianpanama"), ""), sptField,
                     FxDB(drutama("pastatusnama"), ""), sptField,
                     FxDB(drutama("pastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("painputusernama"), ""), sptField,
                     FxDB(drutama("pamodifikasiusernama"), ""), sptField,
                     FxDB(drutama("pakategori"), 0), sptField,
                     FxDB(drutama("pakategorinama"), ""), sptField,
                     FxDB(drutama("pakategoriharga"), ""), sptField,
                     FxDB(drutama("pakategoriharganama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpadetail"), 0), sptField,
                     FxDB(dr("idpa"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargajual1lama"), 0), sptField,
                     FxDB(dr("hargajual2lama"), 0), sptField,
                     FxDB(dr("hargajual3lama"), 0), sptField,
                     FxDB(dr("hargajual4lama"), 0), sptField,
                     FxDB(dr("hargajual5lama"), 0), sptField,
                     FxDB(dr("hargajual1"), 0), sptField,
                     FxDB(dr("hargajual2"), 0), sptField,
                     FxDB(dr("hargajual3"), 0), sptField,
                     FxDB(dr("hargajual4"), 0), sptField,
                     FxDB(dr("hargajual5"), 0), sptField,
                     FxDB(dr("diskonjual1lama"), 0), sptField,
                     FxDB(dr("diskonjual2lama"), 0), sptField,
                     FxDB(dr("diskonjual3lama"), 0), sptField,
                     FxDB(dr("diskonjual4lama"), 0), sptField,
                     FxDB(dr("diskonjual5lama"), 0), sptField,
                     FxDB(dr("diskonjual1"), 0), sptField,
                     FxDB(dr("diskonjual2"), 0), sptField,
                     FxDB(dr("diskonjual3"), 0), sptField,
                     FxDB(dr("diskonjual4"), 0), sptField,
                     FxDB(dr("diskonjual5"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("statusberlaku"), 0), sptField,
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
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paidhistory, paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, patutupperiode, paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama, pakategori, pakategorinama, pakategoriharga, pakategoriharganama" & sptSubParam & "idhistorydetail, idhistory, idpadetail, idpa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, bhargabeli, kontak, kontakkode, kontaknama"))

        Return wsResult
    End Function

End Class
