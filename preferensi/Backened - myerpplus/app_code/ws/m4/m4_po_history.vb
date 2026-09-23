Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_po_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Po_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_po_history(SELECT 0, po.* FROM m4_po po WHERE po.poid = '" & idtransaksi & "')"
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
            sql = "SELECT poidhistory FROM m4_po_history WHERE poid = '" & idtransaksi & "' ORDER BY pomodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_po_detail_history (SELECT 0, '" & result(4) & "', po.* FROM m4_po_detail po WHERE po.idpo = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m4_po_cost_history (SELECT 0, '" & result(4) & "', po.* FROM m4_po_cost po WHERE po.idpo = '" & idtransaksi & "' )"
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
    Public Function M4_Po_HistorySearch(ByVal param As String) As String
        'M4_Po_HistorySearch --------------------------------------------------------
        'poidhistory, poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, 
        'pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, 
        'posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, 
        'po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, 
        'ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, 
        'podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, 
        'pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, 
        'poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, 
        'postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, 
        'poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocabangnama, 
        'polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, prnotransaksi, 
        'csnotransaksi, rqnotransaksi, bsnotransaksi, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama

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
            Filter = Filter.Replace("posupplierkode", "c1.kkode")
            Filter = Filter.Replace("posuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_po_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Po_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("poid"), 0), sptField,
                     FxDB(dr("poidhistory"), 0), sptField,
                     FxDB(dr("pocabang"), ""), sptField,
                     FxDB(dr("polokasi"), ""), sptField,
                     FxDB(dr("pogudang"), ""), sptField,
                     FxDB(dr("poasalbarang"), ""), sptField,
                     FxDB(dr("poasalbarangkategori"), 0), sptField,
                     FxDB(dr("pojenispembelian"), ""), sptField,
                     FxDB(dr("pojenispembeliankategori"), 0), sptField,
                     FxDB(dr("pocarabayar"), 0), sptField,
                     FxDB(dr("posumber"), ""), sptField,
                     FxDB(dr("poautonotransaksi"), 0), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgl"), ""), formatTgl), sptField,
                     FxDB(dr("pokodepa"), 0), sptField,
                     FxDB(dr("posupplier"), 0), sptField,
                     FxDB(dr("posupplierkontak"), ""), sptField,
                     FxDB(dr("po1alamat1"), ""), sptField,
                     FxDB(dr("po1alamat2"), ""), sptField,
                     FxDB(dr("po1alamat3"), ""), sptField,
                     FxDB(dr("po2alamat1"), ""), sptField,
                     FxDB(dr("po2alamat2"), ""), sptField,
                     FxDB(dr("po2alamat3"), ""), sptField,
                     FxDB(dr("pobagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("potgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("potermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pouraian"), ""), sptField,
                     FxDB(dr("pocatatan"), ""), sptField,
                     FxDB(dr("ponoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("potglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("pomatauang"), ""), sptField,
                     FxDB(dr("pokurs"), 0), sptField,
                     FxDB(dr("pohargatermasukpajak"), 0), sptField,
                     FxDB(dr("pototal"), 0), sptField,
                     FxDB(dr("podiskonpersen"), ""), sptField,
                     FxDB(dr("pojmldiskon"), 0), sptField,
                     FxDB(dr("pototalpajak1detail"), 0), sptField,
                     FxDB(dr("pototalpajak2detail"), 0), sptField,
                     FxDB(dr("pobiayalainpersen"), ""), sptField,
                     FxDB(dr("pobiayalain"), 0), sptField,
                     FxDB(dr("pototaltransaksi"), 0), sptField,
                     FxDB(dr("pojmlbayar"), 0), sptField,
                     FxDB(dr("porekdiskon"), ""), sptField,
                     FxDB(dr("porekpajak1"), ""), sptField,
                     FxDB(dr("porekpajak2"), ""), sptField,
                     FxDB(dr("porekbiayalain"), ""), sptField,
                     FxDB(dr("porekbayar"), ""), sptField,
                     FxDB(dr("poidpr"), 0), sptField,
                     FxDB(dr("poidcs"), 0), sptField,
                     FxDB(dr("poidrq"), 0), sptField,
                     FxDB(dr("poidbs"), 0), sptField,
                     FxDB(dr("postatusipc"), 0), sptField,
                     FxDB(dr("postatusgrn"), 0), sptField,
                     FxDB(dr("postatusri"), 0), sptField,
                     FxDB(dr("postatusdnr"), 0), sptField,
                     FxDB(dr("postatusprt"), 0), sptField,
                     FxDB(dr("postatusrealisasi"), 0), sptField,
                     FxDB(dr("postatus"), 0), sptField,
                     FxDB(dr("postatussebelumnya"), 0), sptField,
                     FxDB(dr("pojmlrevisi"), 0), sptField,
                     FxDB(dr("pocetakanke"), 0), sptField,
                     FxDB(dr("poinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("poinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pomodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pomodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("poposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("popostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("poisclose"), 0), sptField,
                     FxDB(dr("pocabangnama"), ""), sptField,
                     FxDB(dr("polokasinama"), ""), sptField,
                     FxDB(dr("pogudangnama"), ""), sptField,
                     FxDB(dr("posupplierkode"), ""), sptField,
                     FxDB(dr("posuppliernama"), ""), sptField,
                     FxDB(dr("pobagianpembeliankode"), ""), sptField,
                     FxDB(dr("pobagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("postatusnama"), ""), sptField,
                     FxDB(dr("postatussebelumnyanama"), ""), sptField,
                     FxDB(dr("poinputusernama"), ""), sptField,
                     FxDB(dr("pomodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found. " & sql & " WHERE " & Filter & " ORDER BY " + Sorting
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("poidhistory, poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocabangnama, polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PoHistoryGetdataById(ByVal param As String) As String

        'M4_PoHistoryGetdataById Utama --------------------------------------------------------
        'poidhistory, poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, 
        'pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, 
        'posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, 
        'po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, 
        'ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, 
        'podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, 
        'pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, 
        'poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, 
        'postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, 
        'poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocustomtext1, 
        'pocustomtext2, pocustomtext3, pocustomtext4, pocustomtext5, pocustomint1, pocustomint2, pocustomint3, 
        'pocustomdbl1, pocustomdbl2, pocustomdbl3, pocustomdate1, pocustomdate2, pocustomdate3, pocabangnama, 
        'polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, poterminnama, 
        'poterminharijatuhtempo, porekdiskonnama, porekpajak1nama, porekpajak2nama, porekbiayalainnama, porekbayarnama, ponotransaksipr, 
        'ponotransaksics, ponotransaksirq, ponotransaksibs, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama 

        'M4_PoHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, 
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

        'M4_PoHistoryGetdataById Cost -------------------------------------------------------
        'idhistorycost, idhistory, idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, 
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

        Dim NmMemcached As String = "aplikasi1-M4_Po_history~M4_Po_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "poidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "poidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_po_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("poidhistory"), 0), sptField,
                     FxDB(drutama("poid"), 0), sptField,
                     FxDB(drutama("pocabang"), ""), sptField,
                     FxDB(drutama("polokasi"), ""), sptField,
                     FxDB(drutama("pogudang"), ""), sptField,
                     FxDB(drutama("poasalbarang"), ""), sptField,
                     FxDB(drutama("poasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pojenispembelian"), ""), sptField,
                     FxDB(drutama("pojenispembeliankategori"), 0), sptField,
                     FxDB(drutama("pocarabayar"), 0), sptField,
                     FxDB(drutama("posumber"), ""), sptField,
                     FxDB(drutama("poautonotransaksi"), 0), sptField,
                     FxDB(drutama("ponotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("potgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pokodepa"), 0), sptField,
                     FxDB(drutama("posupplier"), 0), sptField,
                     FxDB(drutama("posupplierkontak"), ""), sptField,
                     FxDB(drutama("po1alamat1"), ""), sptField,
                     FxDB(drutama("po1alamat2"), ""), sptField,
                     FxDB(drutama("po1alamat3"), ""), sptField,
                     FxDB(drutama("po2alamat1"), ""), sptField,
                     FxDB(drutama("po2alamat2"), ""), sptField,
                     FxDB(drutama("po2alamat3"), ""), sptField,
                     FxDB(drutama("pobagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("potgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("potermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("potgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pouraian"), ""), sptField,
                     FxDB(drutama("pocatatan"), ""), sptField,
                     FxDB(drutama("ponoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("potglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("potglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("pomatauang"), ""), sptField,
                     FxDB(drutama("pokurs"), 0), sptField,
                     FxDB(drutama("pohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pototal"), 0), sptField,
                     FxDB(drutama("podiskonpersen"), ""), sptField,
                     FxDB(drutama("pojmldiskon"), 0), sptField,
                     FxDB(drutama("pototalpajak1detail"), 0), sptField,
                     FxDB(drutama("pototalpajak2detail"), 0), sptField,
                     FxDB(drutama("pobiayalainpersen"), ""), sptField,
                     FxDB(drutama("pobiayalain"), 0), sptField,
                     FxDB(drutama("pototaltransaksi"), 0), sptField,
                     FxDB(drutama("pojmlbayar"), 0), sptField,
                     FxDB(drutama("porekdiskon"), ""), sptField,
                     FxDB(drutama("porekpajak1"), ""), sptField,
                     FxDB(drutama("porekpajak2"), ""), sptField,
                     FxDB(drutama("porekbiayalain"), ""), sptField,
                     FxDB(drutama("porekbayar"), ""), sptField,
                     FxDB(drutama("poidpr"), 0), sptField,
                     FxDB(drutama("poidcs"), 0), sptField,
                     FxDB(drutama("poidrq"), 0), sptField,
                     FxDB(drutama("poidbs"), 0), sptField,
                     FxDB(drutama("postatusipc"), 0), sptField,
                     FxDB(drutama("postatusgrn"), 0), sptField,
                     FxDB(drutama("postatusri"), 0), sptField,
                     FxDB(drutama("postatusdnr"), 0), sptField,
                     FxDB(drutama("postatusprt"), 0), sptField,
                     FxDB(drutama("postatusrealisasi"), 0), sptField,
                     FxDB(drutama("postatus"), 0), sptField,
                     FxDB(drutama("postatussebelumnya"), 0), sptField,
                     FxDB(drutama("pojmlrevisi"), 0), sptField,
                     FxDB(drutama("pocetakanke"), 0), sptField,
                     FxDB(drutama("poinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("poinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pomodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pomodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("poposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("popostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("poisclose"), 0), sptField,
                     FxDB(drutama("pocustomtext1"), ""), sptField,
                     FxDB(drutama("pocustomtext2"), ""), sptField,
                     FxDB(drutama("pocustomtext3"), ""), sptField,
                     FxDB(drutama("pocustomtext4"), ""), sptField,
                     FxDB(drutama("pocustomtext5"), ""), sptField,
                     FxDB(drutama("pocustomint1"), 0), sptField,
                     FxDB(drutama("pocustomint2"), 0), sptField,
                     FxDB(drutama("pocustomint3"), 0), sptField,
                     FxDB(drutama("pocustomdbl1"), 0), sptField,
                     FxDB(drutama("pocustomdbl2"), 0), sptField,
                     FxDB(drutama("pocustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pocustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pocabangnama"), ""), sptField,
                     FxDB(drutama("polokasinama"), ""), sptField,
                     FxDB(drutama("pogudangnama"), ""), sptField,
                     FxDB(drutama("posupplierkode"), ""), sptField,
                     FxDB(drutama("posuppliernama"), ""), sptField,
                     FxDB(drutama("pobagianpembeliankode"), ""), sptField,
                     FxDB(drutama("pobagianpembeliannama"), ""), sptField,
                     FxDB(drutama("poterminnama"), ""), sptField,
                     FxDB(drutama("poterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("porekdiskonnama"), ""), sptField,
                     FxDB(drutama("porekpajak1nama"), ""), sptField,
                     FxDB(drutama("porekpajak2nama"), ""), sptField,
                     FxDB(drutama("porekbiayalainnama"), ""), sptField,
                     FxDB(drutama("porekbayarnama"), ""), sptField,
                     FxDB(drutama("ponotransaksipr"), ""), sptField,
                     FxDB(drutama("ponotransaksics"), ""), sptField,
                     FxDB(drutama("ponotransaksirq"), ""), sptField,
                     FxDB(drutama("ponotransaksibs"), ""), sptField,
                     FxDB(drutama("postatusnama"), ""), sptField,
                     FxDB(drutama("postatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("poinputusernama"), ""), sptField,
                     FxDB(drutama("pomodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idpo"), 0), sptField,
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
            sql = "SELECT poc.idhistorycost, poc.idhistory, poc.idpocost, poc.idpo, poc.kodecost, poc.matauang, poc.kurs, poc.jumlah, poc.rekdebit, poc.rekkredit, poc.kontak, poc.termasukhpp, poc.catatan, poc.costcenter, poc.divisi, poc.subdivisi, poc.proyek, poc.urutan, poc.idprcost, poc.idcscost, poc.idrqcost, poc.idbscost, poc.jumlahipc, poc.statusipc, poc.jumlahgrn, poc.statusgrn, poc.jumlahri, poc.statusri, poc.jumlahbayar, poc.statusbayar, poc.isclose, poc.customtext1, poc.customtext2, poc.customtext3, poc.customdbl1, poc.customdbl2, poc.customdbl3, poc.customdate1, poc.customdate2, poc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama,  c.kkode as kontakkode, c.knama as kontaknama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sddivisi as subdivisinama FROM m4_po_cost_history poc JOIN m4_po_history po ON poc.idhistory = po.poidhistory LEFT JOIN m1_other_cost oc ON poc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON poc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON poc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON poc.kontak = c.kid LEFT JOIN m1_cost_center cc ON poc.costcenter = cc.cckode LEFT JOIN m1_division d ON poc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON poc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_po_cost", Filter, "poc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idhistorycost"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idricost"), 0), sptField,
                     FxDB(dr("idpocost"), ""), sptField,
                     FxDB(dr("idpo"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("poidhistory, poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocustomtext1, pocustomtext2, pocustomtext3, pocustomtext4, pocustomtext5, pocustomint1, pocustomint2, pocustomint3, pocustomdbl1, pocustomdbl2, pocustomdbl3, pocustomdate1, pocustomdate2, pocustomdate3, pocabangnama, polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, poterminnama, poterminharijatuhtempo, porekdiskonnama, porekpajak1nama, porekpajak2nama, porekbiayalainnama, porekbayarnama, ponotransaksipr, ponotransaksics, ponotransaksirq, ponotransaksibs, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "idhistorycost, idhistory, idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama"))

        Return wsResult
    End Function

End Class
