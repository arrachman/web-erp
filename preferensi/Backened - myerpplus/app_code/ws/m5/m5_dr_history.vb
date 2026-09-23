Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_dr_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m5_Dr_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_dr_history(SELECT 0, dr.* FROM m5_dr dr WHERE dr.drid = '" & idtransaksi & "')"
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
            sql = "SELECT dridhistory FROM m5_dr_history WHERE drid = '" & idtransaksi & "' ORDER BY drmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_dr_detail_history (SELECT 0, '" & result(4) & "', dr.* FROM m5_dr_detail dr WHERE dr.iddr = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY BATCH ---------------------------------------
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'DR')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY BATCH --------------------------------

            'PROSES INSERT HISTORY SERIAL ---------------------------------------
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'DR')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY SERIAL --------------------------------

            'PROSES INSERT HISTORY ASSET ---------------------------------------
            sql = "INSERT INTO m7_asset_transaction_history(SELECT 0, '" & result(4) & "', atr.* FROM m7_asset_transaction atr WHERE atr.atidutama = '" & idtransaksi & "' and atr.atsumber = 'DR')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY ASSET --------------------------------

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
    Public Function M5_Dr_HistorySearch(ByVal param As String) As String
        'M5_Dr_HistorySearch --------------------------------------------------------
        'dridhistory, drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, 
        'drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, 
        'drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, 
        'dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, 
        'drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, 
        'drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, 
        'dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, 
        'drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, 
        'drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcabangnama, 
        'drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drekspedisinama, 
        'sqnotransaksi, sonotransaksi, plnotransaksi, donotransaksi, drstatusnama, drstatussebelumnyanama, drinputusernama, 
        'drmodifikasiusernama

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
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_dr_v_history")

        dt = AmbilData("aplikasi1-M5_dr_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("drid"), 0), sptField,
                     FxDB(dr("dridhistory"), 0), sptField,
                     FxDB(dr("drcabang"), ""), sptField,
                     FxDB(dr("drlokasi"), ""), sptField,
                     FxDB(dr("drgudang"), ""), sptField,
                     FxDB(dr("drasalbarang"), ""), sptField,
                     FxDB(dr("drasalbarangkategori"), 0), sptField,
                     FxDB(dr("drjenispenjualan"), ""), sptField,
                     FxDB(dr("drjenispenjualankategori"), 0), sptField,
                     FxDB(dr("drcarabayar"), 0), sptField,
                     FxDB(dr("drsumber"), ""), sptField,
                     FxDB(dr("drautonotransaksi"), 0), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtgl"), ""), formatTgl), sptField,
                     FxDB(dr("drkodepa"), 0), sptField,
                     FxDB(dr("drcustomer"), 0), sptField,
                     FxDB(dr("drcustomerkontak"), ""), sptField,
                     FxDB(dr("dr1alamat1"), ""), sptField,
                     FxDB(dr("dr1alamat2"), ""), sptField,
                     FxDB(dr("dr1alamat3"), ""), sptField,
                     FxDB(dr("dr2alamat1"), ""), sptField,
                     FxDB(dr("dr2alamat2"), ""), sptField,
                     FxDB(dr("dr2alamat3"), ""), sptField,
                     FxDB(dr("drbagianpenjualan"), 0), sptField,
                     FxDB(dr("drbagianpengiriman"), 0), sptField,
                     FxDB(dr("drekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("drtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("druraian"), ""), sptField,
                     FxDB(dr("drcatatan"), ""), sptField,
                     FxDB(dr("drnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("drtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("drtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("drmatauang"), ""), sptField,
                     FxDB(dr("drkurs"), 0), sptField,
                     FxDB(dr("drhargatermasukpajak"), 0), sptField,
                     FxDB(dr("drtotal"), 0), sptField,
                     FxDB(dr("drdiskonpersen"), ""), sptField,
                     FxDB(dr("drjmldiskon"), 0), sptField,
                     FxDB(dr("drtotalpajak1detail"), 0), sptField,
                     FxDB(dr("drtotalpajak2detail"), 0), sptField,
                     FxDB(dr("drbiayalainpersen"), 0), sptField,
                     FxDB(dr("drbiayalain"), 0), sptField,
                     FxDB(dr("drtotaltransaksi"), 0), sptField,
                     FxDB(dr("drrekdiskon"), ""), sptField,
                     FxDB(dr("drrekpajak1"), ""), sptField,
                     FxDB(dr("drrekpajak2"), ""), sptField,
                     FxDB(dr("drrekbiayalain"), ""), sptField,
                     FxDB(dr("dridsq"), 0), sptField,
                     FxDB(dr("dridso"), 0), sptField,
                     FxDB(dr("dridpi"), 0), sptField,
                     FxDB(dr("dridpl"), 0), sptField,
                     FxDB(dr("driddo"), 0), sptField,
                     FxDB(dr("drstatussi"), 0), sptField,
                     FxDB(dr("drstatusrnr"), 0), sptField,
                     FxDB(dr("drstatussr"), 0), sptField,
                     FxDB(dr("drstatusrealisasi"), 0), sptField,
                     FxDB(dr("drstatus"), 0), sptField,
                     FxDB(dr("drstatussebelumnya"), 0), sptField,
                     FxDB(dr("drjmlrevisi"), 0), sptField,
                     FxDB(dr("drcetakanke"), 0), sptField,
                     FxDB(dr("drinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("drinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("drmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("drmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("drposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("drpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("drtutupperiode"), 0), sptField,
                     FxDB(dr("drisclose"), 0), sptField,
                     FxDB(dr("drcabangnama"), ""), sptField,
                     FxDB(dr("drlokasinama"), ""), sptField,
                     FxDB(dr("drgudangnama"), ""), sptField,
                     FxDB(dr("drcustomerkode"), ""), sptField,
                     FxDB(dr("drcustomernama"), ""), sptField,
                     FxDB(dr("drbagianpenjualankode"), ""), sptField,
                     FxDB(dr("drbagianpenjualannama"), ""), sptField,
                     FxDB(dr("drekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("drstatusnama"), ""), sptField,
                     FxDB(dr("drstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("drinputusernama"), ""), sptField,
                     FxDB(dr("drmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dridhistory, drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcabangnama, drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drekspedisinama, sqnotransaksi, sonotransaksi, plnotransaksi, donotransaksi, drstatusnama, drstatussebelumnyanama, drinputusernama, drmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_DrHistoryGetdataById(ByVal param As String) As String
        'M5_DrHistoryGetdataById Utama --------------------------------------------------------
        'dridhistory, drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, 
        'drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, 
        'drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, 
        'dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, 
        'druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, 
        'drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, 
        'drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, 
        'dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, 
        'drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, 
        'drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcustomtext1, 
        'drcustomtext2, drcustomtext3, drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, 
        'drcustomdbl1, drcustomdbl2, drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3, drcabangnama, 
        'drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drbagianpengirimankode, 
        'drbagianpengirimannama, drekspedisinama, drterminnama, drterminharijatuhtempo, drrekdiskonnama, drrekpajak1nama, drrekpajak2nama, 
        'drrekbiayalainnama, drnotransaksisq, drnotransaksiso, drnotransaksipi, drnotransaksipl, drnotransaksido, drstatusnama, 
        'drstatussebelumnyanama, drinputusernama, drmodifikasiusernama

        'M5_DrHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, iddrdetail, iddr, idbarang, namabarang, 
        'tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, 
        'satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, 
        'diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, 
        'gudangtransitnama, gudangtujuannama, gudangkembalinama, costcenternama, divisinama, subdivisinama, proyeknama, 
        'sonotransaksi, pinotransaksi, plnotransaksi, donotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_DrHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_DrHistoryGetdataById Serial --------------------------------------------------------
        'nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang


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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = ""
        Dim sumber As String = "DR"

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

        Dim NmMemcached As String = "aplikasi1-M5_dr~M5_dr_Detail-" & idtransaksi

        'Redrace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi redrace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "dridhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "dridhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_dr_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("dridhistory"), 0), sptField, FxDB(drutama("drid"), 0), sptField,
                     FxDB(drutama("drcabang"), ""), sptField,
                     FxDB(drutama("drlokasi"), ""), sptField,
                     FxDB(drutama("drgudang"), ""), sptField,
                     FxDB(drutama("drasalbarang"), ""), sptField,
                     FxDB(drutama("drasalbarangkategori"), 0), sptField,
                     FxDB(drutama("drjenispenjualan"), ""), sptField,
                     FxDB(drutama("drjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("drcarabayar"), 0), sptField,
                     FxDB(drutama("drsumber"), ""), sptField,
                     FxDB(drutama("drautonotransaksi"), 0), sptField,
                     FxDB(drutama("drnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("drkodepa"), 0), sptField,
                     FxDB(drutama("drcustomer"), 0), sptField,
                     FxDB(drutama("drcustomerkontak"), ""), sptField,
                     FxDB(drutama("dr1alamat1"), ""), sptField,
                     FxDB(drutama("dr1alamat2"), ""), sptField,
                     FxDB(drutama("dr1alamat3"), ""), sptField,
                     FxDB(drutama("dr2alamat1"), ""), sptField,
                     FxDB(drutama("dr2alamat2"), ""), sptField,
                     FxDB(drutama("dr2alamat3"), ""), sptField,
                     FxDB(drutama("drbagianpenjualan"), 0), sptField,
                     FxDB(drutama("drbagianpengiriman"), 0), sptField,
                     FxDB(drutama("drekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("drtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("druraian"), ""), sptField,
                     FxDB(drutama("drcatatan"), ""), sptField,
                     FxDB(drutama("drnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("drtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("drtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("drmatauang"), ""), sptField,
                     FxDB(drutama("drkurs"), 0), sptField,
                     FxDB(drutama("drhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("drtotal"), 0), sptField,
                     FxDB(drutama("drdiskonpersen"), ""), sptField,
                     FxDB(drutama("drjmldiskon"), 0), sptField,
                     FxDB(drutama("drtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("drtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("drbiayalainpersen"), 0), sptField,
                     FxDB(drutama("drbiayalain"), 0), sptField,
                     FxDB(drutama("drtotaltransaksi"), 0), sptField,
                     FxDB(drutama("drrekdiskon"), ""), sptField,
                     FxDB(drutama("drrekpajak1"), ""), sptField,
                     FxDB(drutama("drrekpajak2"), ""), sptField,
                     FxDB(drutama("drrekbiayalain"), ""), sptField,
                     FxDB(drutama("dridsq"), 0), sptField,
                     FxDB(drutama("dridso"), 0), sptField,
                     FxDB(drutama("dridpi"), 0), sptField,
                     FxDB(drutama("dridpl"), 0), sptField,
                     FxDB(drutama("driddo"), 0), sptField,
                     FxDB(drutama("drstatussi"), 0), sptField,
                     FxDB(drutama("drstatusrnr"), 0), sptField,
                     FxDB(drutama("drstatussr"), 0), sptField,
                     FxDB(drutama("drstatusrealisasi"), 0), sptField,
                     FxDB(drutama("drstatus"), 0), sptField,
                     FxDB(drutama("drstatussebelumnya"), 0), sptField,
                     FxDB(drutama("drjmlrevisi"), 0), sptField,
                     FxDB(drutama("drcetakanke"), 0), sptField,
                     FxDB(drutama("drinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("drmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("drposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("drtutupperiode"), 0), sptField,
                     FxDB(drutama("drisclose"), 0), sptField,
                     FxDB(drutama("drcustomtext1"), ""), sptField,
                     FxDB(drutama("drcustomtext2"), ""), sptField,
                     FxDB(drutama("drcustomtext3"), ""), sptField,
                     FxDB(drutama("drcustomtext4"), ""), sptField,
                     FxDB(drutama("drcustomtext5"), ""), sptField,
                     FxDB(drutama("drcustomint1"), 0), sptField,
                     FxDB(drutama("drcustomint2"), 0), sptField,
                     FxDB(drutama("drcustomint3"), 0), sptField,
                     FxDB(drutama("drcustomdbl1"), 0), sptField,
                     FxDB(drutama("drcustomdbl2"), 0), sptField,
                     FxDB(drutama("drcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("drcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("drcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("drcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("drcabangnama"), ""), sptField,
                     FxDB(drutama("drlokasinama"), ""), sptField,
                     FxDB(drutama("drgudangnama"), ""), sptField,
                     FxDB(drutama("drcustomerkode"), ""), sptField,
                     FxDB(drutama("drcustomernama"), ""), sptField,
                     FxDB(drutama("drbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("drbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("drbagianpengirimankode"), ""), sptField,
                     FxDB(drutama("drbagianpengirimannama"), ""), sptField,
                     FxDB(drutama("drekspedisinama"), ""), sptField,
                     FxDB(drutama("drterminnama"), ""), sptField,
                     FxDB(drutama("drterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("drrekdiskonnama"), ""), sptField,
                     FxDB(drutama("drrekpajak1nama"), ""), sptField,
                     FxDB(drutama("drrekpajak2nama"), ""), sptField,
                     FxDB(drutama("drrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("drnotransaksisq"), ""), sptField,
                     FxDB(drutama("drnotransaksiso"), ""), sptField,
                     FxDB(drutama("drnotransaksipi"), ""), sptField,
                     FxDB(drutama("drnotransaksipl"), ""), sptField,
                     FxDB(drutama("drnotransaksido"), ""), sptField,
                     FxDB(drutama("drstatusnama"), ""), sptField,
                     FxDB(drutama("drstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("drinputusernama"), ""), sptField,
                     FxDB(drutama("drmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("iddr"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("jmlkembali"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("jmlbarangkembali"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("gudangkembali"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("iddodetail"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtransitnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("gudangkembalinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtidhistory` AS `nbtidhistory`, `nbt`.`nbtidtransaksihistory` AS `nbtidtransaksihistory`,`nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction_history` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksihistory = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch, FxDB(dr("nbtidhistory"), 0), sptField, FxDB(dr("nbtidtransaksihistory"), 0), sptField,
                     FxDB(dr("nbtid"), 0), sptField,
                     FxDB(dr("nbtjenismutasi"), 0), sptField,
                     FxDB(dr("nbtidbatchin"), 0), sptField,
                     FxDB(dr("nbtgudang"), ""), sptField,
                     FxDB(dr("nbtidbarang"), 0), sptField,
                     FxDB(dr("nbtkode"), ""), sptField,
                     FxDB(dr("nbtsumber"), ""), sptField,
                     FxDB(dr("nbtidtransaksi"), 0), sptField,
                     FxDB(dr("nbtsatuan"), ""), sptField,
                     FxDB(dr("nbtjml"), 0), sptField,
                     FxDB(dr("nbtcustomtext1"), ""), sptField,
                     FxDB(dr("nbtcustomtext2"), ""), sptField,
                     FxDB(dr("nbtcustomtext3"), ""), sptField,
                     FxDB(dr("nbtcustomdbl1"), 0), sptField,
                     FxDB(dr("nbtcustomdbl2"), 0), sptField,
                     FxDB(dr("nbtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstidhistory` AS `nstidhistory`,`nst`.`nstidtransaksihistory` AS `nstidtransaksihistory`,`nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_serial_transaction_history` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksihistory = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial, FxDB(dr("nstidhistory"), 0), sptField, FxDB(dr("nstidtransaksihistory"), 0), sptField,
                     FxDB(dr("nstid"), 0), sptField,
                     FxDB(dr("nstjenismutasi"), 0), sptField,
                     FxDB(dr("nstidserialin"), 0), sptField,
                     FxDB(dr("nstgudang"), ""), sptField,
                     FxDB(dr("nstidbarang"), 0), sptField,
                     FxDB(dr("nstkode"), ""), sptField,
                     FxDB(dr("nstsumber"), ""), sptField,
                     FxDB(dr("nstidtransaksi"), 0), sptField,
                     FxDB(dr("nstsatuan"), ""), sptField,
                     FxDB(dr("nstjml"), 0), sptField,
                     FxDB(dr("nstcustomtext1"), ""), sptField,
                     FxDB(dr("nstcustomtext2"), ""), sptField,
                     FxDB(dr("nstcustomtext3"), ""), sptField,
                     FxDB(dr("nstcustomdbl1"), 0), sptField,
                     FxDB(dr("nstcustomdbl2"), 0), sptField,
                     FxDB(dr("nstcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dridhistory, drid, drcabang, drlokasi, drgudang, drasalbarang, drasalbarangkategori, drjenispenjualan, drjenispenjualankategori, drcarabayar, drsumber, drautonotransaksi, drnotransaksi, drtgl, drkodepa, drcustomer, drcustomerkontak, dr1alamat1, dr1alamat2, dr1alamat3, dr2alamat1, dr2alamat2, dr2alamat3, drbagianpenjualan, drbagianpengiriman, drekspedisi, drtglkirim, drtermin, drtgljatuhtempo, druraian, drcatatan, drnoref, drtglnoref, drtglpenutupan, drmatauang, drkurs, drhargatermasukpajak, drtotal, drdiskonpersen, drjmldiskon, drtotalpajak1detail, drtotalpajak2detail, drbiayalainpersen, drbiayalain, drtotaltransaksi, drrekdiskon, drrekpajak1, drrekpajak2, drrekbiayalain, dridsq, dridso, dridpi, dridpl, driddo, drstatussi, drstatusrnr, drstatussr, drstatusrealisasi, drstatus, drstatussebelumnya, drjmlrevisi, drcetakanke, drinputuser, drinputtgl, drmodifikasiuser, drmodifikasitgl, drposting, drpostingtgl, drtutupperiode, drisclose, drcustomtext1, drcustomtext2, drcustomtext3, drcustomtext4, drcustomtext5, drcustomint1, drcustomint2, drcustomint3, drcustomdbl1, drcustomdbl2, drcustomdbl3, drcustomdate1, drcustomdate2, drcustomdate3, drcabangnama, drlokasinama, drgudangnama, drcustomerkode, drcustomernama, drbagianpenjualankode, drbagianpenjualannama, drbagianpengirimankode, drbagianpengirimannama, drekspedisinama, drterminnama, drterminharijatuhtempo, drrekdiskonnama, drrekpajak1nama, drrekpajak2nama, drrekbiayalainnama, drnotransaksisq, drnotransaksiso, drnotransaksipi, drnotransaksipl, drnotransaksido, drstatusnama, drstatussebelumnyanama, drinputusernama, drmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, iddrdetail, iddr, idbarang, namabarang, tipebarang, jml, jmlkembali, satuan, nilaisatuan, jmlbarang, jmlbarangkembali, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hpp, diskon, jmldiskon, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, gudangkembali, rekpersediaan, rekhargapokok, rekdiskonpenjualan, pajak1, jmlpajak1, pajak2, jmlpajak2, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpidetail, idpldetail, iddodetail, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, gudangkembalinama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi, pinotransaksi, plnotransaksi, donotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

End Class
