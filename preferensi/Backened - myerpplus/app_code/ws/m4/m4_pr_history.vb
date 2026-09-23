Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Pr_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_pr_history(SELECT 0, pr.* FROM m4_pr pr WHERE pr.prid = '" & idtransaksi & "')"
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
            sql = "SELECT pridhistory FROM m4_pr_history WHERE prid = '" & idtransaksi & "' ORDER BY prmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY TRANS --------------------------------------
            sql = "INSERT INTO m4_pr_trans_history (SELECT 0, '" & result(4) & "', pr.* FROM m4_pr_trans pr WHERE pr.idpr = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY TRANS -------------------------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_pr_detail_history (SELECT 0, '" & result(4) & "', pr.* FROM m4_pr_detail pr WHERE pr.idpr = '" & idtransaksi & "' )"
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
    Public Function M4_Pr_HistorySearch(ByVal param As String) As String
        'M4_Pr_HistorySearch --------------------------------------------------------
        'pridhistoryprid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, 
        'prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, 
        'prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, 
        'prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, 
        'prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, 
        'prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, 
        'prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, 
        'prisclose, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, 
        'prmintakenama, sqnotransaksi, prstatusnama, prstatussebelumnyanama, prinputusernama, prmodifikasiusernama

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
            Filter = Filter.Replace("prdimintaolehkode", "c1.kkode")
            Filter = Filter.Replace("prdimintaolehnama", "c1.knama")
            Filter = Filter.Replace("prmintakekode", "c2.kkode")
            Filter = Filter.Replace("prmintakenama", "c2.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pr_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pr_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("prid"), 0), sptField,
                     FxDB(dr("pridhistory"), 0), sptField,
                     FxDB(dr("prcabang"), ""), sptField,
                     FxDB(dr("prlokasi"), ""), sptField,
                     FxDB(dr("prgudang"), ""), sptField,
                     FxDB(dr("prasalbarang"), ""), sptField,
                     FxDB(dr("prasalbarangkategori"), 0), sptField,
                     FxDB(dr("prjenispembelian"), ""), sptField,
                     FxDB(dr("prjenispembeliankategori"), 0), sptField,
                     FxDB(dr("prcarabayar"), 0), sptField,
                     FxDB(dr("prsumber"), ""), sptField,
                     FxDB(dr("prautonotransaksi"), 0), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtgl"), ""), formatTgl), sptField,
                     FxDB(dr("prkodepa"), 0), sptField,
                     FxDB(dr("prdimintaoleh"), 0), sptField,
                     FxDB(dr("prdimintaolehkontak"), ""), sptField,
                     FxDB(dr("prmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("prtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pruraian"), ""), sptField,
                     FxDB(dr("prcatatan"), ""), sptField,
                     FxDB(dr("prnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("prmatauang"), ""), sptField,
                     FxDB(dr("prkurs"), 0), sptField,
                     FxDB(dr("prhargatermasukpajak"), 0), sptField,
                     FxDB(dr("prtotal"), 0), sptField,
                     FxDB(dr("prdiskonpersen"), ""), sptField,
                     FxDB(dr("prjmldiskon"), 0), sptField,
                     FxDB(dr("prtotalpajak1detail"), 0), sptField,
                     FxDB(dr("prtotalpajak2detail"), 0), sptField,
                     FxDB(dr("prbiayalainpersen"), ""), sptField,
                     FxDB(dr("prbiayalain"), 0), sptField,
                     FxDB(dr("prtotaltransaksi"), 0), sptField,
                     FxDB(dr("pridsq"), 0), sptField,
                     FxDB(dr("prstatuscs"), 0), sptField,
                     FxDB(dr("prstatusrq"), 0), sptField,
                     FxDB(dr("prstatuspo"), 0), sptField,
                     FxDB(dr("prstatusipc"), 0), sptField,
                     FxDB(dr("prstatusgrn"), 0), sptField,
                     FxDB(dr("prstatusri"), 0), sptField,
                     FxDB(dr("prstatusdnr"), 0), sptField,
                     FxDB(dr("prstatusprt"), 0), sptField,
                     FxDB(dr("prstatusrealisasi"), 0), sptField,
                     FxDB(dr("prstatus"), 0), sptField,
                     FxDB(dr("prstatussebelumnya"), 0), sptField,
                     FxDB(dr("prjmlrevisi"), 0), sptField,
                     FxDB(dr("prcetakanke"), 0), sptField,
                     FxDB(dr("prinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prisclose"), 0), sptField,
                     FxDB(dr("prcabangnama"), ""), sptField,
                     FxDB(dr("prlokasinama"), ""), sptField,
                     FxDB(dr("prgudangnama"), ""), sptField,
                     FxDB(dr("prdimintaolehkode"), ""), sptField,
                     FxDB(dr("prdimintaolehnama"), ""), sptField,
                     FxDB(dr("prmintakekode"), ""), sptField,
                     FxDB(dr("prmintakenama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("prstatusnama"), ""), sptField,
                     FxDB(dr("prstatussebelumnyanama"), ""), sptField,
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
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pridhistory,prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, prisclose, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, prmintakenama, sqnotransaksi, prstatusnama, prstatussebelumnyanama, prinputusernama, prmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PrHistoryGetdataById(ByVal param As String) As String

        'M4_PrHistoryGetdataById Utama --------------------------------------------------------
        'pridhistory, prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, 
        'prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, 
        'prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, 
        'prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, 
        'prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, 
        'prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, 
        'prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, 
        'prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, 
        'prisclose, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, 
        'prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, 
        'prcustomdate3, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, 
        'prmintakenama, prterminnama, prterminharijatuhtempo, prnotransaksisq, prstatusnama, prstatussebelumnyanama, prinputusernama, 
        'prmodifikasiusernama

        'M4_PrHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, 
        'cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, 
        'jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, 
        'statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, supplierkode, suppliernama, cabangnama, lokasinama, gudangnama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sqnotransaksi

        'M4_PrHistoryGetdataById Trans -------------------------------------------------------
        'idhistorytrans, idhistory, idprtrans, sumber, idtransaksi, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, 
        'customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5,
        'notransaksi, tgltransaksi, kontak, kontakkode, kontaknama

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

        Dim utama As String = "", detail As String = "", trans As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Pr_history~M4_Pr_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pridhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pridhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pr_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pridhistory"), 0), sptField, FxDB(drutama("prid"), 0), sptField,
                     FxDB(drutama("prcabang"), ""), sptField,
                     FxDB(drutama("prlokasi"), ""), sptField,
                     FxDB(drutama("prgudang"), ""), sptField,
                     FxDB(drutama("prasalbarang"), ""), sptField,
                     FxDB(drutama("prasalbarangkategori"), 0), sptField,
                     FxDB(drutama("prjenispembelian"), ""), sptField,
                     FxDB(drutama("prjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("prcarabayar"), 0), sptField,
                     FxDB(drutama("prsumber"), ""), sptField,
                     FxDB(drutama("prautonotransaksi"), 0), sptField,
                     FxDB(drutama("prnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("prkodepa"), 0), sptField,
                     FxDB(drutama("prdimintaoleh"), 0), sptField,
                     FxDB(drutama("prdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("prmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("prtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pruraian"), ""), sptField,
                     FxDB(drutama("prcatatan"), ""), sptField,
                     FxDB(drutama("prnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("prmatauang"), ""), sptField,
                     FxDB(drutama("prkurs"), 0), sptField,
                     FxDB(drutama("prhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("prtotal"), 0), sptField,
                     FxDB(drutama("prdiskonpersen"), ""), sptField,
                     FxDB(drutama("prjmldiskon"), 0), sptField,
                     FxDB(drutama("prtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("prtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("prbiayalainpersen"), ""), sptField,
                     FxDB(drutama("prbiayalain"), 0), sptField,
                     FxDB(drutama("prtotaltransaksi"), 0), sptField,
                     FxDB(drutama("pridsq"), 0), sptField,
                     FxDB(drutama("prstatuscs"), 0), sptField,
                     FxDB(drutama("prstatusrq"), 0), sptField,
                     FxDB(drutama("prstatuspo"), 0), sptField,
                     FxDB(drutama("prstatusipc"), 0), sptField,
                     FxDB(drutama("prstatusgrn"), 0), sptField,
                     FxDB(drutama("prstatusri"), 0), sptField,
                     FxDB(drutama("prstatusdnr"), 0), sptField,
                     FxDB(drutama("prstatusprt"), 0), sptField,
                     FxDB(drutama("prstatusrealisasi"), 0), sptField,
                     FxDB(drutama("prstatus"), 0), sptField,
                     FxDB(drutama("prstatussebelumnya"), 0), sptField,
                     FxDB(drutama("prjmlrevisi"), 0), sptField,
                     FxDB(drutama("prcetakanke"), 0), sptField,
                     FxDB(drutama("prinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prisclose"), 0), sptField,
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
                     FxDB(drutama("prcabangnama"), ""), sptField,
                     FxDB(drutama("prlokasinama"), ""), sptField,
                     FxDB(drutama("prgudangnama"), ""), sptField,
                     FxDB(drutama("prdimintaolehkode"), ""), sptField,
                     FxDB(drutama("prdimintaolehnama"), ""), sptField,
                     FxDB(drutama("prmintakekode"), ""), sptField,
                     FxDB(drutama("prmintakenama"), ""), sptField,
                     FxDB(drutama("prterminnama"), ""), sptField,
                     FxDB(drutama("prterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("prnotransaksisq"), ""), sptField,
                     FxDB(drutama("prstatusnama"), ""), sptField,
                     FxDB(drutama("prstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("prinputusernama"), ""), sptField,
                     FxDB(drutama("prmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idpr"), 0), sptField,
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
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("hargajual"), 0), sptField,
                     FxDB(dr("stokterakhir"), 0), sptField,
                     FxDB(dr("supplier"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlcs"), 0), sptField,
                     FxDB(dr("statuscs"), 0), sptField,
                     FxDB(dr("jmlrq"), 0), sptField,
                     FxDB(dr("statusrq"), 0), sptField,
                     FxDB(dr("jmlpo"), 0), sptField,
                     FxDB(dr("statuspo"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
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
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("supplierkode"), ""), sptField,
                     FxDB(dr("suppliernama"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA TRANS
            sql = "SELECT prtrans.idprtranshistory, prtrans.idhistory, prtrans.idprtrans, prtrans.idpr, prtrans.sumber, prtrans.idtransaksi, prtrans.catatan, prtrans.urutan, prtrans.isclose, prtrans.customtext1, prtrans.customtext2, prtrans.customtext3, prtrans.customtext4, prtrans.customtext5, prtrans.customdbl1, prtrans.customdbl2, prtrans.customdbl3, prtrans.customdbl4, prtrans.customdbl5, prtrans.customdate1, prtrans.customdate2, prtrans.customdate3, prtrans.customdate4, prtrans.customdate5,m5do.donotransaksi as notransaksi, m5do.dotgl as tgltransaksi, m5do.docustomer as kontak, c.kkode as kontakkode,  c.knama as kontaknama FROM m4_pr_trans_history prtrans LEFT JOIN m5_do m5do  ON prtrans.sumber = m5do.dosumber AND prtrans.idtransaksi = m5do.doid LEFT JOIN m1_contact c ON m5do.docustomer = c.kid"
            Dim dttrans As New DataTable
            dttrans = AmbilData("aplikasi1-m1_no_trans_out", "prtrans.idhistory = '" & idtransaksi & "'", "prtrans.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dttrans.Rows
                trans = String.Concat(trans,
                     FxDB(dr("idprtranshistory"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idprtrans"), 0), sptField,
                     FxDB(dr("idpr"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgltransaksi"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptRow)
            Next
            If trans.Length > 0 Then trans = trans.Substring(0, trans.Length - sptRow.Length) Else trans = trans


            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, trans)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pridhistory, prid, prcabang, prlokasi, prgudang, prasalbarang, prasalbarangkategori, prjenispembelian, prjenispembeliankategori, prcarabayar, prsumber, prautonotransaksi, prnotransaksi, prtgl, prkodepa, prdimintaoleh, prdimintaolehkontak, prmintake, prtgldipakai, prtermin, prtgljatuhtempo, pruraian, prcatatan, prnoref, prtglnoref, prtglpenutupan, prmatauang, prkurs, prhargatermasukpajak, prtotal, prdiskonpersen, prjmldiskon, prtotalpajak1detail, prtotalpajak2detail, prbiayalainpersen, prbiayalain, prtotaltransaksi, pridsq, prstatuscs, prstatusrq, prstatuspo, prstatusipc, prstatusgrn, prstatusri, prstatusdnr, prstatusprt, prstatusrealisasi, prstatus, prstatussebelumnya, prjmlrevisi, prcetakanke, prinputuser, prinputtgl, prmodifikasiuser, prmodifikasitgl, prposting, prpostingtgl, prisclose, prcustomtext1, prcustomtext2, prcustomtext3, prcustomtext4, prcustomtext5, prcustomint1, prcustomint2, prcustomint3, prcustomdbl1, prcustomdbl2, prcustomdbl3, prcustomdate1, prcustomdate2, prcustomdate3, prcabangnama, prlokasinama, prgudangnama, prdimintaolehkode, prdimintaolehnama, prmintakekode, prmintakenama, prterminnama, prterminharijatuhtempo, prnotransaksisq, prstatusnama, prstatussebelumnyanama, prinputusernama, prmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idprdetail, idpr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, hargajual, stokterakhir, supplier, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlcs, statuscs, jmlrq, statusrq, jmlpo, statuspo, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, supplierkode, suppliernama, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi" & sptSubParam & "idprtranshistory, idhistory, idprtrans, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5, notransaksi, tgltransaksi, kontak, kontakkode, kontaknama"))

        Return wsResult
    End Function


End Class
