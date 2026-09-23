Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_pl_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Pl_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_pl_history(SELECT 0, pl.* FROM m5_pl pl WHERE pl.plid = '" & idtransaksi & "')"
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
            sql = "SELECT plidhistory FROM m5_pl_history WHERE plid = '" & idtransaksi & "' ORDER BY plmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_pl_detail_history (SELECT 0, '" & result(4) & "', pl.* FROM m5_pl_detail pl WHERE pl.idpl = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY PACK --------------------------------------
            sql = "INSERT INTO m5_pl_pack_history (SELECT 0, '" & result(4) & "', pl.* FROM m5_pl_pack pl WHERE pl.idpl = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY PACK -------------------------------


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
    Public Function M5_Pl_HistorySearch(ByVal param As String) As String
        'M5_Pl_HistorySearch --------------------------------------------------------
        'plidhistory, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, 
        'plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcabangnama, pllokasinama, 
        'plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plekspedisinama, sqnotransaksi, 
        'sonotransaksi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama

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
        sql = query.PanggilQuery("m5_pl_v_history")

        dt = AmbilData("aplikasi1-M5_pl_v_history", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("plid"), 0), sptField,
                     FxDB(dr("plidhistory"), 0), sptField,
                     FxDB(dr("plcabang"), ""), sptField,
                     FxDB(dr("pllokasi"), ""), sptField,
                     FxDB(dr("plgudang"), ""), sptField,
                     FxDB(dr("plasalbarang"), ""), sptField,
                     FxDB(dr("plasalbarangkategori"), 0), sptField,
                     FxDB(dr("pljenispenjualan"), ""), sptField,
                     FxDB(dr("pljenispenjualankategori"), 0), sptField,
                     FxDB(dr("plcarabayar"), 0), sptField,
                     FxDB(dr("plsumber"), ""), sptField,
                     FxDB(dr("plautonotransaksi"), 0), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltgl"), ""), formatTgl), sptField,
                     FxDB(dr("plkodepa"), 0), sptField,
                     FxDB(dr("plcustomer"), 0), sptField,
                     FxDB(dr("plcustomerkontak"), ""), sptField,
                     FxDB(dr("pl1alamat1"), ""), sptField,
                     FxDB(dr("pl1alamat2"), ""), sptField,
                     FxDB(dr("pl1alamat3"), ""), sptField,
                     FxDB(dr("pl2alamat1"), ""), sptField,
                     FxDB(dr("pl2alamat2"), ""), sptField,
                     FxDB(dr("pl2alamat3"), ""), sptField,
                     FxDB(dr("plbagianpenjualan"), 0), sptField,
                     FxDB(dr("plbagianpengepakan"), 0), sptField,
                     FxDB(dr("plekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("pltermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pluraian"), ""), sptField,
                     FxDB(dr("plcatatan"), ""), sptField,
                     FxDB(dr("plnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("plmatauang"), ""), sptField,
                     FxDB(dr("plkurs"), 0), sptField,
                     FxDB(dr("plhargatermasukpajak"), 0), sptField,
                     FxDB(dr("pltotal"), 0), sptField,
                     FxDB(dr("pldiskonpersen"), ""), sptField,
                     FxDB(dr("pljmldiskon"), 0), sptField,
                     FxDB(dr("pltotalpajak1detail"), 0), sptField,
                     FxDB(dr("pltotalpajak2detail"), 0), sptField,
                     FxDB(dr("plbiayalainpersen"), 0), sptField,
                     FxDB(dr("plbiayalain"), 0), sptField,
                     FxDB(dr("pltotaltransaksi"), 0), sptField,
                     FxDB(dr("plrekdiskon"), ""), sptField,
                     FxDB(dr("plrekpajak1"), ""), sptField,
                     FxDB(dr("plrekpajak2"), ""), sptField,
                     FxDB(dr("plrekbiayalain"), ""), sptField,
                     FxDB(dr("plidsq"), 0), sptField,
                     FxDB(dr("plidso"), 0), sptField,
                     FxDB(dr("plidpi"), 0), sptField,
                     FxDB(dr("plstatusdo"), 0), sptField,
                     FxDB(dr("plstatusdr"), 0), sptField,
                     FxDB(dr("plstatussi"), 0), sptField,
                     FxDB(dr("plstatusrnr"), 0), sptField,
                     FxDB(dr("plstatussr"), 0), sptField,
                     FxDB(dr("plstatusrealisasi"), 0), sptField,
                     FxDB(dr("plstatus"), 0), sptField,
                     FxDB(dr("plstatussebelumnya"), 0), sptField,
                     FxDB(dr("pljmlrevisi"), 0), sptField,
                     FxDB(dr("plcetakanke"), 0), sptField,
                     FxDB(dr("plinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("plinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("plmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("plmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("plposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("plpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("plisclose"), 0), sptField,
                     FxDB(dr("plcabangnama"), ""), sptField,
                     FxDB(dr("pllokasinama"), ""), sptField,
                     FxDB(dr("plgudangnama"), ""), sptField,
                     FxDB(dr("plcustomerkode"), ""), sptField,
                     FxDB(dr("plcustomernama"), ""), sptField,
                     FxDB(dr("plbagianpenjualankode"), ""), sptField,
                     FxDB(dr("plbagianpenjualannama"), ""), sptField,
                     FxDB(dr("plekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("plstatusnama"), ""), sptField,
                     FxDB(dr("plstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("plinputusernama"), ""), sptField,
                     FxDB(dr("plmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("plidhistory, plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcabangnama, pllokasinama, plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plekspedisinama, sqnotransaksi, sonotransaksi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PlHistoryGetdataById(ByVal param As String) As String
        'M5_PlGetdataById Utama --------------------------------------------------------
        'plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, 
        'plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcustomtext1, plcustomtext2, 
        'plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, 
        'plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3, plcabangnama, pllokasinama, 
        'plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, 
        'plekspedisinama, plterminnama, plterminharijatuhtempo, plrekdiskonnama, plrekpajak1nama, plrekpajak2nama, plrekbiayalainnama, 
        'plnotransaksisq, plnotransaksiso, plnotransaksipi, plstatusnama, plstatussebelumnyanama, 
        'plinputusernama, plmodifikasiusernama, ktingkatjual, kpkp

        'M5_PlGetdataById Detail --------------------------------------------------------
        'idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi

        'M5_PlGetdataById Pack --------------------------------------------------------
        'idplpack, idpl, nopack, catatan, bentuk, berat, 
        'urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

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

        Dim utama As String = "", detail As String = "", pack As String = "", idtransaksi As String = ""

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

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "plidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "plidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pl`.`plid` AS `plid`,`pl`.`plcabang` AS `plcabang`,`pl`.`pllokasi` AS `pllokasi`,`pl`.`plgudang` AS `plgudang`,`pl`.`plasalbarang` AS `plasalbarang`,`pl`.`plasalbarangkategori` AS `plasalbarangkategori`,`pl`.`pljenispenjualan` AS `pljenispenjualan`,`pl`.`pljenispenjualankategori` AS `pljenispenjualankategori`,`pl`.`plcarabayar` AS `plcarabayar`,`pl`.`plsumber` AS `plsumber`,`pl`.`plautonotransaksi` AS `plautonotransaksi`,`pl`.`plnotransaksi` AS `plnotransaksi`,`pl`.`pltgl` AS `pltgl`,`pl`.`plkodepa` AS `plkodepa`,`pl`.`plcustomer` AS `plcustomer`,`pl`.`plcustomerkontak` AS `plcustomerkontak`,`pl`.`pl1alamat1` AS `pl1alamat1`,`pl`.`pl1alamat2` AS `pl1alamat2`,`pl`.`pl1alamat3` AS `pl1alamat3`,`pl`.`pl2alamat1` AS `pl2alamat1`,`pl`.`pl2alamat2` AS `pl2alamat2`,`pl`.`pl2alamat3` AS `pl2alamat3`,`pl`.`plbagianpenjualan` AS `plbagianpenjualan`,`pl`.`plbagianpengepakan` AS `plbagianpengepakan`,`pl`.`plekspedisi` AS `plekspedisi`,`pl`.`pltglkirim` AS `pltglkirim`,`pl`.`pltermin` AS `pltermin`,`pl`.`pltgljatuhtempo` AS `pltgljatuhtempo`,`pl`.`pluraian` AS `pluraian`,`pl`.`plcatatan` AS `plcatatan`,`pl`.`plnoref` AS `plnoref`,`pl`.`pltglnoref` AS `pltglnoref`,`pl`.`pltglpenutupan` AS `pltglpenutupan`,`pl`.`plmatauang` AS `plmatauang`,`pl`.`plkurs` AS `plkurs`,`pl`.`plhargatermasukpajak` AS `plhargatermasukpajak`,`pl`.`pltotal` AS `pltotal`,`pl`.`pldiskonpersen` AS `pldiskonpersen`,`pl`.`pljmldiskon` AS `pljmldiskon`,`pl`.`pltotalpajak1detail` AS `pltotalpajak1detail`,`pl`.`pltotalpajak2detail` AS `pltotalpajak2detail`,`pl`.`plbiayalainpersen` AS `plbiayalainpersen`,`pl`.`plbiayalain` AS `plbiayalain`,`pl`.`pltotaltransaksi` AS `pltotaltransaksi`,`pl`.`plrekdiskon` AS `plrekdiskon`,`pl`.`plrekpajak1` AS `plrekpajak1`,`pl`.`plrekpajak2` AS `plrekpajak2`,`pl`.`plrekbiayalain` AS `plrekbiayalain`,`pl`.`plidsq` AS `plidsq`,`pl`.`plidso` AS `plidso`,`pl`.`plidpi` AS `plidpi`,`pl`.`plstatusdo` AS `plstatusdo`,`pl`.`plstatusdr` AS `plstatusdr`,`pl`.`plstatussi` AS `plstatussi`,`pl`.`plstatusrnr` AS `plstatusrnr`,`pl`.`plstatussr` AS `plstatussr`,`pl`.`plstatusrealisasi` AS `plstatusrealisasi`,`pl`.`plstatus` AS `plstatus`,`pl`.`plstatussebelumnya` AS `plstatussebelumnya`,`pl`.`pljmlrevisi` AS `pljmlrevisi`,`pl`.`plcetakanke` AS `plcetakanke`,`pl`.`plinputuser` AS `plinputuser`,`pl`.`plinputtgl` AS `plinputtgl`,`pl`.`plmodifikasiuser` AS `plmodifikasiuser`,`pl`.`plmodifikasitgl` AS `plmodifikasitgl`,`pl`.`plposting` AS `plposting`,`pl`.`plpostingtgl` AS `plpostingtgl`,`pl`.`plisclose` AS `plisclose`,`pl`.`plcustomtext1` AS `plcustomtext1`,`pl`.`plcustomtext2` AS `plcustomtext2`,`pl`.`plcustomtext3` AS `plcustomtext3`,`pl`.`plcustomtext4` AS `plcustomtext4`,`pl`.`plcustomtext5` AS `plcustomtext5`,`pl`.`plcustomint1` AS `plcustomint1`,`pl`.`plcustomint2` AS `plcustomint2`,`pl`.`plcustomint3` AS `plcustomint3`,`pl`.`plcustomdbl1` AS `plcustomdbl1`,`pl`.`plcustomdbl2` AS `plcustomdbl2`,`pl`.`plcustomdbl3` AS `plcustomdbl3`,`pl`.`plcustomdate1` AS `plcustomdate1`,`pl`.`plcustomdate2` AS `plcustomdate2`,`pl`.`plcustomdate3` AS `plcustomdate3`,`br`.`bnama` AS `plcabangnama`,`lc`.`lnama` AS `pllokasinama`,`wh`.`wnama` AS `plgudangnama`,`c1`.`ktingkatjual`,`c1`.`kkode` AS `plcustomerkode`,`c1`.`knama` AS `plcustomernama`,`c2`.`kkode` AS `plbagianpenjualankode`,`c2`.`knama` AS `plbagianpenjualannama`,`c3`.`kkode` AS `plbagianpengepakankode`,`c3`.`knama` AS `plbagianpengepakannama`,`e`.`enama` AS `plekspedisinama`,`tr`.`trnama` AS `plterminnama`,`tr`.`trharijatuhtempo` AS `plterminharijatuhtempo`,`coa1`.`cnama` AS `plrekdiskonnama`,`coa2`.`cnama` AS `plrekpajak1nama`,`coa3`.`cnama` AS `plrekpajak2nama`,`coa4`.`cnama` AS `plrekbiayalainnama`,`sq`.`sqnotransaksi` AS `plnotransaksisq`,`so`.`sonotransaksi` AS `plnotransaksiso`,`pi`.`pinotransaksi` AS `plnotransaksipi`,`st1`.`nama` AS `plstatusnama`,`st2`.`nama` AS `plstatussebelumnyanama`,`u1`.`unama` AS `plinputusernama`,`u2`.`unama` AS `plmodifikasiusernama`,`pld`.`idpldetail` AS `idpldetail`,`pld`.`idpl` AS `idpl`,`pld`.`idbarang` AS `idbarang`,`pld`.`namabarang` AS `namabarang`,`pld`.`tipebarang` AS `tipebarang`,`pld`.`nopack` AS `nopack`,`pld`.`jml` AS `jml`,`pld`.`satuan` AS `satuan`,`pld`.`nilaisatuan` AS `nilaisatuan`,`pld`.`jmlbarang` AS `jmlbarang`,`pld`.`satuanbarang` AS `satuanbarang`,`pld`.`matauang` AS `matauang`,`pld`.`kurs` AS `kurs`,`pld`.`harga` AS `harga`,`pld`.`diskon` AS `diskon`,`pld`.`jmldiskon` AS `jmldiskon`,`pld`.`pajak1` AS `pajak1`,`pld`.`jmlpajak1` AS `jmlpajak1`,`pld`.`pajak2` AS `pajak2`,`pld`.`jmlpajak2` AS `jmlpajak2`,`pld`.`cabang` AS `cabang`,`pld`.`lokasi` AS `lokasi`,`pld`.`gudang` AS `gudang`,`pld`.`costcenter` AS `costcenter`,`pld`.`divisi` AS `divisi`,`pld`.`subdivisi` AS `subdivisi`,`pld`.`proyek` AS `proyek`,`pld`.`catatan` AS `catatan`,`pld`.`urutan` AS `urutan`,`pld`.`idsqdetail` AS `idsqdetail`,`pld`.`idsodetail` AS `idsodetail`,`pld`.`idpidetail` AS `idpidetail`,`pld`.`jmldo` AS `jmldo`,`pld`.`statusdo` AS `statusdo`,`pld`.`jmldr` AS `jmldr`,`pld`.`statusdr` AS `statusdr`,`pld`.`jmlsi` AS `jmlsi`,`pld`.`statussi` AS `statussi`,`pld`.`jmlrnr` AS `jmlrnr`,`pld`.`statusrnr` AS `statusrnr`,`pld`.`jmlsr` AS `jmlsr`,`pld`.`statussr` AS `statussr`,`pld`.`jmlrealisasi` AS `jmlrealisasi`,`pld`.`statusrealisasi` AS `statusrealisasi`,`pld`.`isclose` AS `isclose`,`pld`.`customtext1` AS `customtext1`,`pld`.`customtext2` AS `customtext2`,`pld`.`customtext3` AS `customtext3`,`pld`.`customdbl1` AS `customdbl1`,`pld`.`customdbl2` AS `customdbl2`,`pld`.`customdbl3` AS `customdbl3`,`pld`.`customdate1` AS `customdate1`,`pld`.`customdate2` AS `customdate2`,`pld`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`sq2`.`sqnotransaksi` AS `sqnotransaksi`,`so2`.`sonotransaksi` AS `sonotransaksi`,`pi2`.`pinotransaksi` AS `pinotransaksi`, c1.kpkp from ((((((((((((((((((((((((((((((((((((`m5_pl_history` `pl` join `m5_pl_detail_history` `pld` on((`pl`.`plid` = `pld`.`idpl`))) left join `m1_branch` `br` on((`br`.`bkode` = `pl`.`plcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `pl`.`pllokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `pl`.`plgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `pl`.`plcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `pl`.`plbagianpenjualan`))) left join `m1_contact` `c3` on((`c3`.`kid` = `pl`.`plbagianpengepakan`))) left join `m1_expedition` `e` on((`pl`.`plekspedisi` = `e`.`ekode`))) left join `m1_terms` `tr` on((`pl`.`pltermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`pl`.`plrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`pl`.`plrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`pl`.`plrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`pl`.`plrekbiayalain` = `coa4`.`cnomor`))) left join `m5_sq` `sq` on((`pl`.`plidsq` = `sq`.`sqid`))) left join `m5_so` `so` on((`pl`.`plidso` = `so`.`soid`))) left join `m5_pi` `pi` on((`pl`.`plidpi` = `pi`.`piid`))) left join `m0_status` `st1` on((`st1`.`kode` = `pl`.`plstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `pl`.`plstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `pl`.`plinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `pl`.`plmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `pld`.`idbarang`))) left join `m1_tax` `t1` on((`pld`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pld`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`pld`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`pld`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`pld`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`pld`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`pld`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`pld`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`pld`.`proyek` = `p`.`pkode`))) left join `m5_sq_detail` `sqd` on((`pld`.`idsqdetail` = `sqd`.`idsqdetail`))) left join `m5_sq` `sq2` on((`sqd`.`idsq` = `sq2`.`sqid`))) left join `m5_so_detail` `sod` on((`pld`.`idsodetail` = `sod`.`idsodetail`))) left join `m5_so` `so2` on((`sod`.`idso` = `so2`.`soid`))) left join `m5_pi_detail` `pid` on((`pld`.`idpidetail` = `pid`.`idpidetail`))) left join `m5_pi` `pi2` on((`pid`.`idpi` = `pi2`.`piid`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("plid"), 0), sptField,
                     FxDB(drutama("plcabang"), ""), sptField,
                     FxDB(drutama("pllokasi"), ""), sptField,
                     FxDB(drutama("plgudang"), ""), sptField,
                     FxDB(drutama("plasalbarang"), ""), sptField,
                     FxDB(drutama("plasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pljenispenjualan"), ""), sptField,
                     FxDB(drutama("pljenispenjualankategori"), 0), sptField,
                     FxDB(drutama("plcarabayar"), 0), sptField,
                     FxDB(drutama("plsumber"), ""), sptField,
                     FxDB(drutama("plautonotransaksi"), 0), sptField,
                     FxDB(drutama("plnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltgl"), ""), formatTgl), sptField,
                     FxDB(drutama("plkodepa"), 0), sptField,
                     FxDB(drutama("plcustomer"), 0), sptField,
                     FxDB(drutama("plcustomerkontak"), ""), sptField,
                     FxDB(drutama("pl1alamat1"), ""), sptField,
                     FxDB(drutama("pl1alamat2"), ""), sptField,
                     FxDB(drutama("pl1alamat3"), ""), sptField,
                     FxDB(drutama("pl2alamat1"), ""), sptField,
                     FxDB(drutama("pl2alamat2"), ""), sptField,
                     FxDB(drutama("pl2alamat3"), ""), sptField,
                     FxDB(drutama("plbagianpenjualan"), 0), sptField,
                     FxDB(drutama("plbagianpengepakan"), 0), sptField,
                     FxDB(drutama("plekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("pltermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pluraian"), ""), sptField,
                     FxDB(drutama("plcatatan"), ""), sptField,
                     FxDB(drutama("plnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("plmatauang"), ""), sptField,
                     FxDB(drutama("plkurs"), 0), sptField,
                     FxDB(drutama("plhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pltotal"), 0), sptField,
                     FxDB(drutama("pldiskonpersen"), ""), sptField,
                     FxDB(drutama("pljmldiskon"), 0), sptField,
                     FxDB(drutama("pltotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pltotalpajak2detail"), 0), sptField,
                     FxDB(drutama("plbiayalainpersen"), 0), sptField,
                     FxDB(drutama("plbiayalain"), 0), sptField,
                     FxDB(drutama("pltotaltransaksi"), 0), sptField,
                     FxDB(drutama("plrekdiskon"), ""), sptField,
                     FxDB(drutama("plrekpajak1"), ""), sptField,
                     FxDB(drutama("plrekpajak2"), ""), sptField,
                     FxDB(drutama("plrekbiayalain"), ""), sptField,
                     FxDB(drutama("plidsq"), 0), sptField,
                     FxDB(drutama("plidso"), 0), sptField,
                     FxDB(drutama("plidpi"), 0), sptField,
                     FxDB(drutama("plstatusdo"), 0), sptField,
                     FxDB(drutama("plstatusdr"), 0), sptField,
                     FxDB(drutama("plstatussi"), 0), sptField,
                     FxDB(drutama("plstatusrnr"), 0), sptField,
                     FxDB(drutama("plstatussr"), 0), sptField,
                     FxDB(drutama("plstatusrealisasi"), 0), sptField,
                     FxDB(drutama("plstatus"), 0), sptField,
                     FxDB(drutama("plstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pljmlrevisi"), 0), sptField,
                     FxDB(drutama("plcetakanke"), 0), sptField,
                     FxDB(drutama("plinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plisclose"), 0), sptField,
                     FxDB(drutama("plcustomtext1"), ""), sptField,
                     FxDB(drutama("plcustomtext2"), ""), sptField,
                     FxDB(drutama("plcustomtext3"), ""), sptField,
                     FxDB(drutama("plcustomtext4"), ""), sptField,
                     FxDB(drutama("plcustomtext5"), ""), sptField,
                     FxDB(drutama("plcustomint1"), 0), sptField,
                     FxDB(drutama("plcustomint2"), 0), sptField,
                     FxDB(drutama("plcustomint3"), 0), sptField,
                     FxDB(drutama("plcustomdbl1"), 0), sptField,
                     FxDB(drutama("plcustomdbl2"), 0), sptField,
                     FxDB(drutama("plcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("plcabangnama"), ""), sptField,
                     FxDB(drutama("pllokasinama"), ""), sptField,
                     FxDB(drutama("plgudangnama"), ""), sptField,
                     FxDB(drutama("plcustomerkode"), ""), sptField,
                     FxDB(drutama("plcustomernama"), ""), sptField,
                     FxDB(drutama("plbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("plbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("plbagianpengepakankode"), ""), sptField,
                     FxDB(drutama("plbagianpengepakannama"), ""), sptField,
                     FxDB(drutama("plekspedisinama"), ""), sptField,
                     FxDB(drutama("plterminnama"), ""), sptField,
                     FxDB(drutama("plterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("plrekdiskonnama"), ""), sptField,
                     FxDB(drutama("plrekpajak1nama"), ""), sptField,
                     FxDB(drutama("plrekpajak2nama"), ""), sptField,
                     FxDB(drutama("plrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("plnotransaksisq"), ""), sptField,
                     FxDB(drutama("plnotransaksiso"), ""), sptField,
                     FxDB(drutama("plnotransaksipi"), ""), sptField,
                     FxDB(drutama("plstatusnama"), ""), sptField,
                     FxDB(drutama("plstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("plinputusernama"), ""), sptField,
                     FxDB(drutama("plmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("nopack"), 0), sptField,
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
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
                     FxDB(dr("jmlsr"), 0), sptField,
                     FxDB(dr("statussr"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            Dim dtpack As New DataTable
            dtpack = AmbilData("aplikasi1-M5_Pl_Pack", "idpl='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases

            For Each dr As DataRow In dtpack.Rows
                pack = String.Concat(pack,
                     FxDB(dr("idplpack"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("nopack"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("bentuk"), ""), sptField,
                     FxDB(dr("berat"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
            Next
            pack = pack.Substring(0, pack.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pack)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3, plcabangnama, pllokasinama, plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, plekspedisinama, plterminnama, plterminharijatuhtempo, plrekdiskonnama, plrekpajak1nama, plrekpajak2nama, plrekbiayalainnama, plnotransaksisq, plnotransaksiso, plnotransaksipi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama, ktingkatjual, kpkp" & sptSubParam & "idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi" & sptSubParam & "idplpack, idpl, nopack, catatan, bentuk, berat, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M5_PlHistoryGetdataById_lama(ByVal param As String) As String
        'M5_PlHistoryGetdataById Utama --------------------------------------------------------
        'plidhistory, plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, 
        'pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, 
        'plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, 
        'pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, 
        'pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, 
        'plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, 
        'plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, 
        'plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, 
        'plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, 
        'plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcustomtext1, plcustomtext2, 
        'plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, 
        'plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3, plcabangnama, pllokasinama, 
        'plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, 
        'plekspedisinama, plterminnama, plterminharijatuhtempo, plrekdiskonnama, plrekpajak1nama, plrekpajak2nama, plrekbiayalainnama, 
        'plnotransaksisq, plnotransaksiso, plnotransaksipi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama

        'M5_PlHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, 
        'statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi

        'M5_PlHistoryGetdataById Pack --------------------------------------------------------
        'idhistorypack, idhistory, idplpack, idpl, nopack, catatan, bentuk, berat, 
        'urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

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

        Dim utama As String = "", detail As String = "", pack As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_pl_history~M5_pl_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "plidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "plidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_pl_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("plidhistory"), 0), sptField,
                     FxDB(drutama("plid"), 0), sptField,
                     FxDB(drutama("plcabang"), ""), sptField,
                     FxDB(drutama("pllokasi"), ""), sptField,
                     FxDB(drutama("plgudang"), ""), sptField,
                     FxDB(drutama("plasalbarang"), ""), sptField,
                     FxDB(drutama("plasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pljenispenjualan"), ""), sptField,
                     FxDB(drutama("pljenispenjualankategori"), 0), sptField,
                     FxDB(drutama("plcarabayar"), 0), sptField,
                     FxDB(drutama("plsumber"), ""), sptField,
                     FxDB(drutama("plautonotransaksi"), 0), sptField,
                     FxDB(drutama("plnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltgl"), ""), formatTgl), sptField,
                     FxDB(drutama("plkodepa"), 0), sptField,
                     FxDB(drutama("plcustomer"), 0), sptField,
                     FxDB(drutama("plcustomerkontak"), ""), sptField,
                     FxDB(drutama("pl1alamat1"), ""), sptField,
                     FxDB(drutama("pl1alamat2"), ""), sptField,
                     FxDB(drutama("pl1alamat3"), ""), sptField,
                     FxDB(drutama("pl2alamat1"), ""), sptField,
                     FxDB(drutama("pl2alamat2"), ""), sptField,
                     FxDB(drutama("pl2alamat3"), ""), sptField,
                     FxDB(drutama("plbagianpenjualan"), 0), sptField,
                     FxDB(drutama("plbagianpengepakan"), 0), sptField,
                     FxDB(drutama("plekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("pltermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pluraian"), ""), sptField,
                     FxDB(drutama("plcatatan"), ""), sptField,
                     FxDB(drutama("plnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("plmatauang"), ""), sptField,
                     FxDB(drutama("plkurs"), 0), sptField,
                     FxDB(drutama("plhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pltotal"), 0), sptField,
                     FxDB(drutama("pldiskonpersen"), ""), sptField,
                     FxDB(drutama("pljmldiskon"), 0), sptField,
                     FxDB(drutama("pltotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pltotalpajak2detail"), 0), sptField,
                     FxDB(drutama("plbiayalainpersen"), 0), sptField,
                     FxDB(drutama("plbiayalain"), 0), sptField,
                     FxDB(drutama("pltotaltransaksi"), 0), sptField,
                     FxDB(drutama("plrekdiskon"), ""), sptField,
                     FxDB(drutama("plrekpajak1"), ""), sptField,
                     FxDB(drutama("plrekpajak2"), ""), sptField,
                     FxDB(drutama("plrekbiayalain"), ""), sptField,
                     FxDB(drutama("plidsq"), 0), sptField,
                     FxDB(drutama("plidso"), 0), sptField,
                     FxDB(drutama("plidpi"), 0), sptField,
                     FxDB(drutama("plstatusdo"), 0), sptField,
                     FxDB(drutama("plstatusdr"), 0), sptField,
                     FxDB(drutama("plstatussi"), 0), sptField,
                     FxDB(drutama("plstatusrnr"), 0), sptField,
                     FxDB(drutama("plstatussr"), 0), sptField,
                     FxDB(drutama("plstatusrealisasi"), 0), sptField,
                     FxDB(drutama("plstatus"), 0), sptField,
                     FxDB(drutama("plstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pljmlrevisi"), 0), sptField,
                     FxDB(drutama("plcetakanke"), 0), sptField,
                     FxDB(drutama("plinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("plisclose"), 0), sptField,
                     FxDB(drutama("plcustomtext1"), ""), sptField,
                     FxDB(drutama("plcustomtext2"), ""), sptField,
                     FxDB(drutama("plcustomtext3"), ""), sptField,
                     FxDB(drutama("plcustomtext4"), ""), sptField,
                     FxDB(drutama("plcustomtext5"), ""), sptField,
                     FxDB(drutama("plcustomint1"), 0), sptField,
                     FxDB(drutama("plcustomint2"), 0), sptField,
                     FxDB(drutama("plcustomint3"), 0), sptField,
                     FxDB(drutama("plcustomdbl1"), 0), sptField,
                     FxDB(drutama("plcustomdbl2"), 0), sptField,
                     FxDB(drutama("plcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("plcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("plcabangnama"), ""), sptField,
                     FxDB(drutama("pllokasinama"), ""), sptField,
                     FxDB(drutama("plgudangnama"), ""), sptField,
                     FxDB(drutama("plcustomerkode"), ""), sptField,
                     FxDB(drutama("plcustomernama"), ""), sptField,
                     FxDB(drutama("plbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("plbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("plbagianpengepakankode"), ""), sptField,
                     FxDB(drutama("plbagianpengepakannama"), ""), sptField,
                     FxDB(drutama("plekspedisinama"), ""), sptField,
                     FxDB(drutama("plterminnama"), ""), sptField,
                     FxDB(drutama("plterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("plrekdiskonnama"), ""), sptField,
                     FxDB(drutama("plrekpajak1nama"), ""), sptField,
                     FxDB(drutama("plrekpajak2nama"), ""), sptField,
                     FxDB(drutama("plrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("plnotransaksisq"), ""), sptField,
                     FxDB(drutama("plnotransaksiso"), ""), sptField,
                     FxDB(drutama("plnotransaksipi"), ""), sptField,
                     FxDB(drutama("plstatusnama"), ""), sptField,
                     FxDB(drutama("plstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("plinputusernama"), ""), sptField,
                     FxDB(drutama("plmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("nopack"), 0), sptField,
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
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
                     FxDB(dr("jmlsr"), 0), sptField,
                     FxDB(dr("statussr"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            Dim dtpack As New DataTable
            dtpack = AmbilData("aplikasi1-M5_Pl_Pack_history", "idpl='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases

            For Each dr As DataRow In dtpack.Rows
                pack = String.Concat(pack,
                     FxDB(dr("idplpack"), 0), sptField,
                     FxDB(dr("idpl"), 0), sptField,
                     FxDB(dr("nopack"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("bentuk"), ""), sptField,
                     FxDB(dr("berat"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
            Next
            pack = pack.Substring(0, pack.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pack)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("plidhistory, plid, plcabang, pllokasi, plgudang, plasalbarang, plasalbarangkategori, pljenispenjualan, pljenispenjualankategori, plcarabayar, plsumber, plautonotransaksi, plnotransaksi, pltgl, plkodepa, plcustomer, plcustomerkontak, pl1alamat1, pl1alamat2, pl1alamat3, pl2alamat1, pl2alamat2, pl2alamat3, plbagianpenjualan, plbagianpengepakan, plekspedisi, pltglkirim, pltermin, pltgljatuhtempo, pluraian, plcatatan, plnoref, pltglnoref, pltglpenutupan, plmatauang, plkurs, plhargatermasukpajak, pltotal, pldiskonpersen, pljmldiskon, pltotalpajak1detail, pltotalpajak2detail, plbiayalainpersen, plbiayalain, pltotaltransaksi, plrekdiskon, plrekpajak1, plrekpajak2, plrekbiayalain, plidsq, plidso, plidpi, plstatusdo, plstatusdr, plstatussi, plstatusrnr, plstatussr, plstatusrealisasi, plstatus, plstatussebelumnya, pljmlrevisi, plcetakanke, plinputuser, plinputtgl, plmodifikasiuser, plmodifikasitgl, plposting, plpostingtgl, plisclose, plcustomtext1, plcustomtext2, plcustomtext3, plcustomtext4, plcustomtext5, plcustomint1, plcustomint2, plcustomint3, plcustomdbl1, plcustomdbl2, plcustomdbl3, plcustomdate1, plcustomdate2, plcustomdate3, plcabangnama, pllokasinama, plgudangnama, plcustomerkode, plcustomernama, plbagianpenjualankode, plbagianpenjualannama, plbagianpengepakankode, plbagianpengepakannama, plekspedisinama, plterminnama, plterminharijatuhtempo, plrekdiskonnama, plrekpajak1nama, plrekpajak2nama, plrekbiayalainnama, plnotransaksisq, plnotransaksiso, plnotransaksipi, plstatusnama, plstatussebelumnyanama, plinputusernama, plmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idpldetail, idpl, idbarang, namabarang, tipebarang, nopack, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi, sonotransaksi, pinotransaksi" & sptSubParam & "idhistorypack, idhistory, idplpack, idpl, nopack, catatan, bentuk, berat, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

End Class
