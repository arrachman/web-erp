Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pf_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Pf_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_pf_history(SELECT 0, pf.* FROM m4_pf pf WHERE pf.pfid = '" & idtransaksi & "')"
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
            sql = "SELECT pfidhistory FROM m4_pf_history WHERE pfid = '" & idtransaksi & "' ORDER BY pfmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_pf_detail_history (SELECT 0, '" & result(4) & "', pf.* FROM m4_pf_detail pf WHERE pf.idpf = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY COST --------------------------------------
            sql = "INSERT INTO m4_pf_cost_history (SELECT 0, '" & result(4) & "', pf.* FROM m4_pf_cost pf WHERE pf.idpf = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY COST -------------------------------

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
    Public Function M4_Pf_HistorySearch(ByVal param As String) As String
        'M4_Pf_HistorySearch --------------------------------------------------------
        'pfidhistory, pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempf, pfuraian, pfcatatan, 
        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
        'pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, 
        'pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfpfsting, pfpfstingtgl, pfisclose, pfcabangnama, 
        'pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, prnotransaksi, 
        'csnotransaksi, rqnotransaksi, bsnotransaksi, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama

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
            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pf_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pf_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("pfidhistory"), 0), sptField,
                     FxDB(dr("pfid"), 0), sptField,
                     FxDB(dr("pfcabang"), ""), sptField,
                     FxDB(dr("pflokasi"), ""), sptField,
                     FxDB(dr("pfgudang"), ""), sptField,
                     FxDB(dr("pfasalbarang"), ""), sptField,
                     FxDB(dr("pfasalbarangkategori"), 0), sptField,
                     FxDB(dr("pfjenispembelian"), ""), sptField,
                     FxDB(dr("pfjenispembeliankategori"), 0), sptField,
                     FxDB(dr("pfcarabayar"), 0), sptField,
                     FxDB(dr("pfsumber"), ""), sptField,
                     FxDB(dr("pfautonotransaksi"), 0), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pftgl"), ""), formatTgl), sptField,
                     FxDB(dr("pfkodepa"), 0), sptField,
                     FxDB(dr("pfsupplier"), 0), sptField,
                     FxDB(dr("pfsupplierkontak"), ""), sptField,
                     FxDB(dr("pf1alamat1"), ""), sptField,
                     FxDB(dr("pf1alamat2"), ""), sptField,
                     FxDB(dr("pf1alamat3"), ""), sptField,
                     FxDB(dr("pf2alamat1"), ""), sptField,
                     FxDB(dr("pf2alamat2"), ""), sptField,
                     FxDB(dr("pf2alamat3"), ""), sptField,
                     FxDB(dr("pfbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pftgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("pftermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pftgljatuhtempf"), ""), formatTgl), sptField,
                     FxDB(dr("pfuraian"), ""), sptField,
                     FxDB(dr("pfcatatan"), ""), sptField,
                     FxDB(dr("pfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("pfmatauang"), ""), sptField,
                     FxDB(dr("pfkurs"), 0), sptField,
                     FxDB(dr("pfhargatermasukpajak"), 0), sptField,
                     FxDB(dr("pftotal"), 0), sptField,
                     FxDB(dr("pfdiskonpersen"), ""), sptField,
                     FxDB(dr("pfjmldiskon"), 0), sptField,
                     FxDB(dr("pftotalpajak1detail"), 0), sptField,
                     FxDB(dr("pftotalpajak2detail"), 0), sptField,
                     FxDB(dr("pfbiayalainpersen"), ""), sptField,
                     FxDB(dr("pfbiayalain"), 0), sptField,
                     FxDB(dr("pftotaltransaksi"), 0), sptField,
                     FxDB(dr("pfjmlbayar"), 0), sptField,
                     FxDB(dr("pfrekdiskon"), ""), sptField,
                     FxDB(dr("pfrekpajak1"), ""), sptField,
                     FxDB(dr("pfrekpajak2"), ""), sptField,
                     FxDB(dr("pfrekbiayalain"), ""), sptField,
                     FxDB(dr("pfrekbayar"), ""), sptField,
                     FxDB(dr("pfidpr"), 0), sptField,
                     FxDB(dr("pfidcs"), 0), sptField,
                     FxDB(dr("pfidrq"), 0), sptField,
                     FxDB(dr("pfidbs"), 0), sptField,
                     FxDB(dr("pfstatusipc"), 0), sptField,
                     FxDB(dr("pfstatusgrn"), 0), sptField,
                     FxDB(dr("pfstatusri"), 0), sptField,
                     FxDB(dr("pfstatusdnr"), 0), sptField,
                     FxDB(dr("pfstatusprt"), 0), sptField,
                     FxDB(dr("pfstatusrealisasi"), 0), sptField,
                     FxDB(dr("pfstatus"), 0), sptField,
                     FxDB(dr("pfstatussebelumnya"), 0), sptField,
                     FxDB(dr("pfjmlrevisi"), 0), sptField,
                     FxDB(dr("pfcetakanke"), 0), sptField,
                     FxDB(dr("pfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pfpfsting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pfpfstingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pfisclose"), 0), sptField,
                     FxDB(dr("pfcabangnama"), ""), sptField,
                     FxDB(dr("pflokasinama"), ""), sptField,
                     FxDB(dr("pfgudangnama"), ""), sptField,
                     FxDB(dr("pfsupplierkode"), ""), sptField,
                     FxDB(dr("pfsuppliernama"), ""), sptField,
                     FxDB(dr("pfbagianpembeliankode"), ""), sptField,
                     FxDB(dr("pfbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("pfstatusnama"), ""), sptField,
                     FxDB(dr("pfstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pfinputusernama"), ""), sptField,
                     FxDB(dr("pfmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pfidhistory, pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempf, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfpfsting, pfpfstingtgl, pfisclose, pfcabangnama, pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PfHistoryGetdataById(ByVal param As String) As String

        'M4_PfHistoryGetdataById Utama --------------------------------------------------------
        'pfidhistory, pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempf, pfuraian, pfcatatan, 
        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
        'pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, 
        'pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfpfsting, pfpfstingtgl, pfisclose, pfcustomtext1, 
        'pfcustomtext2, pfcustomtext3, pfcustomtext4, pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, 
        'pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, pfcustomdate1, pfcustomdate2, pfcustomdate3, pfcabangnama, 
        'pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, pfterminnama, 
        'pfterminharijatuhtempf, pfrekdiskonnama, pfrekpajak1nama, pfrekpajak2nama, pfrekbiayalainnama, pfrekbayarnama, pfnotransaksipr, 
        'pfnotransaksics, pfnotransaksirq, pfnotransaksibs, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama 

        'M4_PfHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, rqnotransaksi, 
        'bsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_PfHistoryGetdataById Cost -------------------------------------------------------
        'idhistorycost, idhistory, idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, 
        'kontaknama, costcenternama, divisinama, subdivisinama

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

        Dim utama As String = "", detail As String = "", cost As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Pf_history~M4_Pf_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pfidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pfidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pf_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pfidhistory"), 0), sptField,
                     FxDB(drutama("pfid"), 0), sptField,
                     FxDB(drutama("pfcabang"), ""), sptField,
                     FxDB(drutama("pflokasi"), ""), sptField,
                     FxDB(drutama("pfgudang"), ""), sptField,
                     FxDB(drutama("pfasalbarang"), ""), sptField,
                     FxDB(drutama("pfasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pfjenispembelian"), ""), sptField,
                     FxDB(drutama("pfjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("pfcarabayar"), 0), sptField,
                     FxDB(drutama("pfsumber"), ""), sptField,
                     FxDB(drutama("pfautonotransaksi"), 0), sptField,
                     FxDB(drutama("pfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pfkodepa"), 0), sptField,
                     FxDB(drutama("pfsupplier"), 0), sptField,
                     FxDB(drutama("pfsupplierkontak"), ""), sptField,
                     FxDB(drutama("pf1alamat1"), ""), sptField,
                     FxDB(drutama("pf1alamat2"), ""), sptField,
                     FxDB(drutama("pf1alamat3"), ""), sptField,
                     FxDB(drutama("pf2alamat1"), ""), sptField,
                     FxDB(drutama("pf2alamat2"), ""), sptField,
                     FxDB(drutama("pf2alamat3"), ""), sptField,
                     FxDB(drutama("pfbagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pftgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("pftermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pftgljatuhtempf"), ""), formatTgl), sptField,
                     FxDB(drutama("pfuraian"), ""), sptField,
                     FxDB(drutama("pfcatatan"), ""), sptField,
                     FxDB(drutama("pfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("pfmatauang"), ""), sptField,
                     FxDB(drutama("pfkurs"), 0), sptField,
                     FxDB(drutama("pfhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pftotal"), 0), sptField,
                     FxDB(drutama("pfdiskonpersen"), ""), sptField,
                     FxDB(drutama("pfjmldiskon"), 0), sptField,
                     FxDB(drutama("pftotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pftotalpajak2detail"), 0), sptField,
                     FxDB(drutama("pfbiayalainpersen"), ""), sptField,
                     FxDB(drutama("pfbiayalain"), 0), sptField,
                     FxDB(drutama("pftotaltransaksi"), 0), sptField,
                     FxDB(drutama("pfjmlbayar"), 0), sptField,
                     FxDB(drutama("pfrekdiskon"), ""), sptField,
                     FxDB(drutama("pfrekpajak1"), ""), sptField,
                     FxDB(drutama("pfrekpajak2"), ""), sptField,
                     FxDB(drutama("pfrekbiayalain"), ""), sptField,
                     FxDB(drutama("pfrekbayar"), ""), sptField,
                     FxDB(drutama("pfidpr"), 0), sptField,
                     FxDB(drutama("pfidcs"), 0), sptField,
                     FxDB(drutama("pfidrq"), 0), sptField,
                     FxDB(drutama("pfidbs"), 0), sptField,
                     FxDB(drutama("pfstatusipc"), 0), sptField,
                     FxDB(drutama("pfstatusgrn"), 0), sptField,
                     FxDB(drutama("pfstatusri"), 0), sptField,
                     FxDB(drutama("pfstatusdnr"), 0), sptField,
                     FxDB(drutama("pfstatusprt"), 0), sptField,
                     FxDB(drutama("pfstatusrealisasi"), 0), sptField,
                     FxDB(drutama("pfstatus"), 0), sptField,
                     FxDB(drutama("pfstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pfjmlrevisi"), 0), sptField,
                     FxDB(drutama("pfcetakanke"), 0), sptField,
                     FxDB(drutama("pfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pfpfsting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pfpfstingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pfisclose"), 0), sptField,
                     FxDB(drutama("pfcustomtext1"), ""), sptField,
                     FxDB(drutama("pfcustomtext2"), ""), sptField,
                     FxDB(drutama("pfcustomtext3"), ""), sptField,
                     FxDB(drutama("pfcustomtext4"), ""), sptField,
                     FxDB(drutama("pfcustomtext5"), ""), sptField,
                     FxDB(drutama("pfcustomint1"), 0), sptField,
                     FxDB(drutama("pfcustomint2"), 0), sptField,
                     FxDB(drutama("pfcustomint3"), 0), sptField,
                     FxDB(drutama("pfcustomdbl1"), 0), sptField,
                     FxDB(drutama("pfcustomdbl2"), 0), sptField,
                     FxDB(drutama("pfcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pfcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pfcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pfcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pfcabangnama"), ""), sptField,
                     FxDB(drutama("pflokasinama"), ""), sptField,
                     FxDB(drutama("pfgudangnama"), ""), sptField,
                     FxDB(drutama("pfsupplierkode"), ""), sptField,
                     FxDB(drutama("pfsuppliernama"), ""), sptField,
                     FxDB(drutama("pfbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("pfbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("pfterminnama"), ""), sptField,
                     FxDB(drutama("pfterminharijatuhtempf"), 0), sptField,
                     FxDB(drutama("pfrekdiskonnama"), ""), sptField,
                     FxDB(drutama("pfrekpajak1nama"), ""), sptField,
                     FxDB(drutama("pfrekpajak2nama"), ""), sptField,
                     FxDB(drutama("pfrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("pfrekbayarnama"), ""), sptField,
                     FxDB(drutama("pfnotransaksipr"), ""), sptField,
                     FxDB(drutama("pfnotransaksics"), ""), sptField,
                     FxDB(drutama("pfnotransaksirq"), ""), sptField,
                     FxDB(drutama("pfnotransaksibs"), ""), sptField,
                     FxDB(drutama("pfstatusnama"), ""), sptField,
                     FxDB(drutama("pfstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pfinputusernama"), ""), sptField,
                     FxDB(drutama("pfmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpfdetail"), 0), sptField,
                     FxDB(dr("idpf"), 0), sptField,
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
                     FxDB(dr("hargafix"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
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
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA COST
            sql = "SELECT pfc.idhistorycost, pfc.idhistory, pfc.idpfcost, pfc.idpf, pfc.kodecost, pfc.matauang, pfc.kurs, pfc.jumlah, pfc.rekdebit, pfc.rekkredit, pfc.kontak, pfc.termasukhpp, pfc.catatan, pfc.costcenter, pfc.divisi, pfc.subdivisi, pfc.proyek, pfc.urutan, pfc.idprcost, pfc.idcscost, pfc.idrqcost, pfc.idbscost, pfc.jumlahipc, pfc.statusipc, pfc.jumlahgrn, pfc.statusgrn, pfc.jumlahri, pfc.statusri, pfc.jumlahbayar, pfc.statusbayar, pfc.isclose, pfc.customtext1, pfc.customtext2, pfc.customtext3, pfc.customdbl1, pfc.customdbl2, pfc.customdbl3, pfc.customdate1, pfc.customdate2, pfc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama,  c.kkode as kontakkode, c.knama as kontaknama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sddivisi as subdivisinama FROM m4_pf_cost_history pfc JOIN m4_pf_history pf ON pfc.idhistory = pf.pfidhistory LEFT JOIN m1_other_cost oc ON pfc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON pfc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON pfc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON pfc.kontak = c.kid LEFT JOIN m1_cost_center cc ON pfc.costcenter = cc.cckode LEFT JOIN m1_division d ON pfc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON pfc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_pf_cost", Filter, "pfc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idhistorycost"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idricost"), 0), sptField,
                     FxDB(dr("idpfcost"), ""), sptField,
                     FxDB(dr("idpf"), ""), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("termasukhpp"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), ""), sptField,
                     FxDB(dr("idcscost"), ""), sptField,
                     FxDB(dr("idrqcost"), ""), sptField,
                     FxDB(dr("idbscost"), ""), sptField,
                     FxDB(dr("jumlahipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jumlahgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jumlahri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jumlahbayar"), 0), sptField,
                     FxDB(dr("statusbayar"), 0), sptField,
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
                     FxDB(dr("kodecostnama"), ""), sptField,
                     FxDB(dr("rekdebitnama"), ""), sptField,
                     FxDB(dr("rekkreditnama"), ""), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, cost)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pfidhistory, pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempf, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfpfsting, pfpfstingtgl, pfisclose, pfcustomtext1, pfcustomtext2, pfcustomtext3, pfcustomtext4, pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, pfcustomdate1, pfcustomdate2, pfcustomdate3, pfcabangnama, pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, pfterminnama, pfterminharijatuhtempf, pfrekdiskonnama, pfrekpajak1nama, pfrekpajak2nama, pfrekbiayalainnama, pfrekbayarnama, pfnotransaksipr, pfnotransaksics, pfnotransaksirq, pfnotransaksibs, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "idhistorycost, idhistory, idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama"))

        Return wsResult
    End Function

End Class
