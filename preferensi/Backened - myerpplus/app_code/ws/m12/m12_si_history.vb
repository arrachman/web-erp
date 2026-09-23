Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_si_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m12_Si_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_si_history(SELECT 0, si.* FROM m5_si si WHERE si.siid = '" & idtransaksi & "')"
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
            sql = "SELECT siidhistory FROM m5_si_history WHERE siid = '" & idtransaksi & "' ORDER BY simodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_si_detail_history (SELECT 0, '" & result(4) & "', si.* FROM m5_si_detail si WHERE si.idsi = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------


            'PROSES INSERT HISTORY PAY --------------------------------------
            sql = "INSERT INTO m5_si_pay_history (SELECT 0, '" & result(4) & "', si.* FROM m5_si_pay si WHERE si.idsi = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY PAY -------------------------------


            'PROSES INSERT HISTORY MATERIAL ---------------------------------
            sql = "INSERT INTO m5_si_material_history (SELECT 0, '" & result(4) & "', si.* FROM m5_si_material si WHERE si.idsi = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY MATERIAL --------------------------


            'PROSES INSERT HISTORY BATCH ---------------------------------------
            sql = "INSERT INTO m1_no_batch_transaction_history(SELECT 0, '" & result(4) & "', nb.* FROM m1_no_batch_transaction nb WHERE nb.nbtidtransaksi = '" & idtransaksi & "' and nb.nbtsumber = 'SI')"
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
            sql = "INSERT INTO m1_no_serial_transaction_history(SELECT 0, '" & result(4) & "', ns.* FROM m1_no_serial_transaction ns WHERE ns.nstidtransaksi = '" & idtransaksi & "' and ns.nstsumber = 'SI')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY SERIAL --------------------------------

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
    Public Function M12_Si_HistorySearch(ByVal param As String) As String
        'M5_Si_HistorySearch --------------------------------------------------------
        'siidhistory, siid, sicabang, silokasi, sigudang, siasalbarang, siasalbarangkategori, 
        'sijenispenjualan, sijenispenjualankategori, sicarabayar, sisumber, siautonotransaksi, sinotransaksi, sitgl, 
        'sikodepa, sicustomer, sicustomerkontak, si1alamat1, si1alamat2, si1alamat3, si2alamat1, 
        'si2alamat2, si2alamat3, sibagianpenjualan, siekspedisi, sitglkirim, sitermin, sitgljatuhtempo, 
        'siuraian, sicatatan, sinoref, sitglnoref, sitglpenutupan, simatauang, sikurs, 
        'sihargatermasukpajak, sitotal, sidiskonpersen, sijmldiskon, sitotalpajak1detail, sitotalpajak2detail, sibiayalainpersen, 
        'sibiayalain, sitotaltransaksi, sijmlbayar, sistatuslunas, sitgllunas, sinofakturpajak, sisdhbayarpajak, 
        'sitglbayarpajak, sirekdiskon, sirekpajak1, sirekpajak2, sirekbiayalain, sirekbayar, siidsq, 
        'siidso, siidpl, siiddo, siiddr, siidpi, sistatusrnr, sistatussr, 
        'sistatusrealisasi, sistatus, sistatussebelumnya, sijmlrevisi, sicetakanke, siinputuser, siinputtgl, 
        'simodifikasiuser, simodifikasitgl, siposting, sipostingtgl, situtupperiode, siisclose, sicabangnama, 
        'silokasinama, sigudangnama, sicustomerkode, sicustomernama, sibagianpenjualankode, sibagianpenjualannama, siekspedisinama, 
        'sqnotransaksi, sonotransaksi, plnotransaksi, donotransaksi, drnotransaksi, pinotransaksi, sistatusnama, 
        'sistatussebelumnyanama, siinputusernama, simodifikasiusernama, sijmluangmuka, sirekuangmuka, siidas, asnotransaksi, 
        'sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, 
        'sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, 
        'sisaldoawal, sibayartunai, sibayarkkredit, sibayarkdebit, sibayarvoucher, sibayarpoin, sibayarjmlpoin, 
        'sichargepersen, sicharge, sipoinsebelumnya, sipoindidapat, sicustomarea, sicustomareanama, sirekcharge,
        'sijmlkembali, sirekkembali

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
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
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
        sql = query.PanggilQuery("m5_si_v_history")

        dt = AmbilData("aplikasi1-M5_si_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("siidhistory"), ""), sptField,
                     FxDB(dr("siid"), ""), sptField,
                     FxDB(dr("sicabang"), ""), sptField,
                     FxDB(dr("silokasi"), ""), sptField,
                     FxDB(dr("sigudang"), ""), sptField,
                     FxDB(dr("siasalbarang"), ""), sptField,
                     FxDB(dr("siasalbarangkategori"), 0), sptField,
                     FxDB(dr("sijenispenjualan"), ""), sptField,
                     FxDB(dr("sijenispenjualankategori"), 0), sptField,
                     FxDB(dr("sicarabayar"), 0), sptField,
                     FxDB(dr("sisumber"), ""), sptField,
                     FxDB(dr("siautonotransaksi"), 0), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sitgl"), ""), formatTgl), sptField,
                     FxDB(dr("sikodepa"), ""), sptField,
                     FxDB(dr("sicustomer"), ""), sptField,
                     FxDB(dr("sicustomerkontak"), ""), sptField,
                     FxDB(dr("si1alamat1"), ""), sptField,
                     FxDB(dr("si1alamat2"), ""), sptField,
                     FxDB(dr("si1alamat3"), ""), sptField,
                     FxDB(dr("si2alamat1"), ""), sptField,
                     FxDB(dr("si2alamat2"), ""), sptField,
                     FxDB(dr("si2alamat3"), ""), sptField,
                     FxDB(dr("sibagianpenjualan"), ""), sptField,
                     FxDB(dr("siekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sitglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sitermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sitgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("siuraian"), ""), sptField,
                     FxDB(dr("sicatatan"), ""), sptField,
                     FxDB(dr("sinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sitglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("simatauang"), ""), sptField,
                     FxDB(dr("sikurs"), 0), sptField,
                     FxDB(dr("sihargatermasukpajak"), 0), sptField,
                     FxDB(dr("sitotal"), 0), sptField,
                     FxDB(dr("sidiskonpersen"), ""), sptField,
                     FxDB(dr("sijmldiskon"), 0), sptField,
                     FxDB(dr("sitotalpajak1detail"), 0), sptField,
                     FxDB(dr("sitotalpajak2detail"), 0), sptField,
                     FxDB(dr("sibiayalainpersen"), ""), sptField,
                     FxDB(dr("sibiayalain"), 0), sptField,
                     FxDB(dr("sitotaltransaksi"), 0), sptField,
                     FxDB(dr("sijmlbayar"), 0), sptField,
                     FxDB(dr("sistatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sitgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("sinofakturpajak"), ""), sptField,
                     FxDB(dr("sisdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sitglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("sirekdiskon"), ""), sptField,
                     FxDB(dr("sirekpajak1"), ""), sptField,
                     FxDB(dr("sirekpajak2"), ""), sptField,
                     FxDB(dr("sirekbiayalain"), ""), sptField,
                     FxDB(dr("sirekbayar"), ""), sptField,
                     FxDB(dr("siidsq"), ""), sptField,
                     FxDB(dr("siidso"), ""), sptField,
                     FxDB(dr("siidpl"), ""), sptField,
                     FxDB(dr("siiddo"), ""), sptField,
                     FxDB(dr("siiddr"), ""), sptField,
                     FxDB(dr("siidpi"), ""), sptField,
                     FxDB(dr("sistatusrnr"), 0), sptField,
                     FxDB(dr("sistatussr"), 0), sptField,
                     FxDB(dr("sistatusrealisasi"), 0), sptField,
                     FxDB(dr("sistatus"), 0), sptField,
                     FxDB(dr("sistatussebelumnya"), 0), sptField,
                     FxDB(dr("sijmlrevisi"), 0), sptField,
                     FxDB(dr("sicetakanke"), 0), sptField,
                     FxDB(dr("siinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("siinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("simodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("simodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("siposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("situtupperiode"), 0), sptField,
                     FxDB(dr("siisclose"), 0), sptField,
                     FxDB(dr("sicabangnama"), ""), sptField,
                     FxDB(dr("silokasinama"), ""), sptField,
                     FxDB(dr("sigudangnama"), ""), sptField,
                     FxDB(dr("sicustomerkode"), ""), sptField,
                     FxDB(dr("sicustomernama"), ""), sptField,
                     FxDB(dr("sibagianpenjualankode"), ""), sptField,
                     FxDB(dr("sibagianpenjualannama"), ""), sptField,
                     FxDB(dr("siekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("plnotransaksi"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("sistatusnama"), ""), sptField,
                     FxDB(dr("sistatussebelumnyanama"), ""), sptField,
                     FxDB(dr("siinputusernama"), ""), sptField,
                     FxDB(dr("simodifikasiusernama"), ""), sptField,
                     FxDB(dr("sijmluangmuka"), 0), sptField,
                     FxDB(dr("sirekuangmuka"), ""), sptField,
                     FxDB(dr("siidas"), ""), sptField,
                     FxDB(dr("asnotransaksi"), ""), sptField,
                     FxDB(dr("sicustomtext1"), ""), sptField,
                     FxDB(dr("sicustomtext2"), ""), sptField,
                     FxDB(dr("sicustomtext3"), ""), sptField,
                     FxDB(dr("sicustomtext4"), ""), sptField,
                     FxDB(dr("sicustomtext5"), ""), sptField,
                     FxDB(dr("sicustomint1"), 0), sptField,
                     FxDB(dr("sicustomint2"), 0), sptField,
                     FxDB(dr("sicustomint3"), 0), sptField,
                     FxDB(dr("sicustomdbl1"), 0), sptField,
                     FxDB(dr("sicustomdbl2"), 0), sptField,
                     FxDB(dr("sicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("sisaldoawal"), 0), sptField,
                     FxDB(dr("sibayartunai"), 0), sptField,
                     FxDB(dr("sibayarkkredit"), 0), sptField,
                     FxDB(dr("sibayarkdebit"), 0), sptField,
                     FxDB(dr("sibayarvoucher"), 0), sptField,
                     FxDB(dr("sibayarpoin"), 0), sptField,
                     FxDB(dr("sibayarjmlpoin"), 0), sptField,
                     FxDB(dr("sichargepersen"), ""), sptField,
                     FxDB(dr("sicharge"), 0), sptField,
                     FxDB(dr("sipoinsebelumnya"), 0), sptField,
                     FxDB(dr("sipoindidapat"), 0), sptField,
                     FxDB(dr("sicustomarea"), ""), sptField,
                     FxDB(dr("sicustomareanama"), ""), sptField,
                     FxDB(dr("sirekcharge"), ""), sptField,
                     FxDB(dr("sijmlkembali"), 0), sptField,
                     FxDB(dr("sirekkembali"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siidhistory, siid, sicabang, silokasi, sigudang, siasalbarang, siasalbarangkategori, sijenispenjualan, sijenispenjualankategori, sicarabayar, sisumber, siautonotransaksi, sinotransaksi, sitgl, sikodepa, sicustomer, sicustomerkontak, si1alamat1, si1alamat2, si1alamat3, si2alamat1, si2alamat2, si2alamat3, sibagianpenjualan, siekspedisi, sitglkirim, sitermin, sitgljatuhtempo, siuraian, sicatatan, sinoref, sitglnoref, sitglpenutupan, simatauang, sikurs, sihargatermasukpajak, sitotal, sidiskonpersen, sijmldiskon, sitotalpajak1detail, sitotalpajak2detail, sibiayalainpersen, sibiayalain, sitotaltransaksi, sijmlbayar, sistatuslunas, sitgllunas, sinofakturpajak, sisdhbayarpajak, sitglbayarpajak, sirekdiskon, sirekpajak1, sirekpajak2, sirekbiayalain, sirekbayar, siidsq, siidso, siidpl, siiddo, siiddr, siidpi, sistatusrnr, sistatussr, sistatusrealisasi, sistatus, sistatussebelumnya, sijmlrevisi, sicetakanke, siinputuser, siinputtgl, simodifikasiuser, simodifikasitgl, siposting, sipostingtgl, situtupperiode, siisclose, sicabangnama, silokasinama, sigudangnama, sicustomerkode, sicustomernama, sibagianpenjualankode, sibagianpenjualannama, siekspedisinama, sqnotransaksi, sonotransaksi, plnotransaksi, donotransaksi, drnotransaksi, pinotransaksi, sistatusnama, sistatussebelumnyanama, siinputusernama, simodifikasiusernama, sijmluangmuka, sirekuangmuka, siidas, asnotransaksi, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sisaldoawal, sibayartunai, sibayarkkredit, sibayarkdebit, sibayarvoucher, sibayarpoin, sibayarjmlpoin, sichargepersen, sicharge, sipoinsebelumnya, sipoindidapat, sicustomarea, sicustomareanama, sirekcharge, sijmlkembali, sirekkembali"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_SiHistoryGetdataById(ByVal param As String) As String
        'M5_SiHistoryGetdataById Utama --------------------------------------------------------
        'siidhistory, siid, sicabang, silokasi, sigudang, siasalbarang, siasalbarangkategori, sijenispenjualan, 
        'sijenispenjualankategori, sicarabayar, sisumber, siautonotransaksi, sinotransaksi, sitgl, sikodepa, 
        'sicustomer, sicustomerkontak, si1alamat1, si1alamat2, si1alamat3, si2alamat1, si2alamat2, 
        'si2alamat3, sibagianpenjualan, siekspedisi, sitglkirim, sitermin, sitgljatuhtempo, siuraian, 
        'sicatatan, sinoref, sitglnoref, sitglpenutupan, simatauang, sikurs, sihargatermasukpajak, 
        'sitotal, sidiskonpersen, sijmldiskon, sitotalpajak1detail, sitotalpajak2detail, sibiayalainpersen, sibiayalain, 
        'sitotaltransaksi, sijmlbayar, sistatuslunas, sitgllunas, sinofakturpajak, sisdhbayarpajak, sitglbayarpajak, 
        'sirekdiskon, sirekpajak1, sirekpajak2, sirekbiayalain, sirekbayar, siidsq, siidso, 
        'siidpl, siiddo, siiddr, siidpi, sistatusrnr, sistatussr, sistatusrealisasi, 
        'sistatus, sistatussebelumnya, sijmlrevisi, sicetakanke, siinputuser, siinputtgl, simodifikasiuser, 
        'simodifikasitgl, siposting, sipostingtgl, situtupperiode, siisclose, sicustomtext1, sicustomtext2, 
        'sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, 
        'sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sicabangnama, silokasinama, 
        'sigudangnama, ktingkatjual, sicustomerkode, sicustomernama, sibagianpenjualankode, sibagianpenjualannama, siekspedisinama, 
        'siterminnama, siterminharijatuhtempo, sirekdiskonnama, sirekpajak1nama, sirekpajak2nama, sirekbiayalainnama, sirekbayarnama, 
        'sinotransaksiso, sinotransaksipl, sinotransaksido, sinotransaksidr, sinotransaksipi, sistatusnama, sistatussebelumnyanama, 
        'siinputusernama, simodifikasiusernama, sijmluangmuka, sirekuangmuka, siidas, sirekuangmukanama, asnotransaksi, 
        'sisaldoawal, sibayartunai, sibayarkkredit, sibayarkdebit, sibayarvoucher, sibayarpoin, sibayarjmlpoin, 
        'sichargepersen, sicharge, sipoinsebelumnya, sipoindidapat, sicustomtext6, sicustomtext7, sicustomtext8, 
        'sicustomtext9, sicustomtext10, sicustomint4, sicustomint5, sicustomint6, sicustomint7, sicustomint8, 
        'sicustomint9, sicustomint10, sicustomdbl4, sicustomdbl5, sicustomdbl6, sicustomdbl7, sicustomdbl8, 
        'sicustomdbl9, sicustomdbl10, sicustomdate4, sicustomdate5, sicustomdate6, sicustomdate7, sicustomdate8, 
        'sicustomdate9, sicustomdate10, sicustomarea, sicustomareanama, sirekcharge, sirekchargenama,
        'sijmlkembali, sirekkembali, sirekkembalinama

        'M5_SiHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idsidetail, idsi, idbarang, namabarang, tipebarang, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, 
        'idhppfifomasuk, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, 
        'gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, 
        'iddodetail, iddrdetail, idpidetail, jmlrnr, statusrnr, jmlsr, statussr, 
        'jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, 
        'bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, sonotransaksi, donotransaksi, drnotransaksi, pinotransaksi, isbonus, isbonusfrom

        'M5_SiHistoryGetdataById Batch --------------------------------------------------------
        'nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_SiHistoryGetdataById Serial --------------------------------------------------------
        'nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M5_SiHistoryGetdataById Pay -------------------------------------------------------
        'idhistorycarabayar, idhistory, idsicarabayar, idsi, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = "", pay As String = ""
        Dim sumber As String = "SI"

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

        Dim NmMemcached As String = "aplikasi1-M5_si~M5_si_Detail-" & idtransaksi

        'Resiace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "siidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "siidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_si_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("siidhistory"), 0), sptField,
                     FxDB(drutama("siid"), ""), sptField,
                     FxDB(drutama("sicabang"), ""), sptField,
                     FxDB(drutama("silokasi"), ""), sptField,
                     FxDB(drutama("sigudang"), ""), sptField,
                     FxDB(drutama("siasalbarang"), ""), sptField,
                     FxDB(drutama("siasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sijenispenjualan"), ""), sptField,
                     FxDB(drutama("sijenispenjualankategori"), 0), sptField,
                     FxDB(drutama("sicarabayar"), 0), sptField,
                     FxDB(drutama("sisumber"), ""), sptField,
                     FxDB(drutama("siautonotransaksi"), 0), sptField,
                     FxDB(drutama("sinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sikodepa"), ""), sptField,
                     FxDB(drutama("sicustomer"), ""), sptField,
                     FxDB(drutama("sicustomerkontak"), ""), sptField,
                     FxDB(drutama("si1alamat1"), ""), sptField,
                     FxDB(drutama("si1alamat2"), ""), sptField,
                     FxDB(drutama("si1alamat3"), ""), sptField,
                     FxDB(drutama("si2alamat1"), ""), sptField,
                     FxDB(drutama("si2alamat2"), ""), sptField,
                     FxDB(drutama("si2alamat3"), ""), sptField,
                     FxDB(drutama("sibagianpenjualan"), ""), sptField,
                     FxDB(drutama("siekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sitglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sitermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sitgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("siuraian"), ""), sptField,
                     FxDB(drutama("sicatatan"), ""), sptField,
                     FxDB(drutama("sinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sitglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("simatauang"), ""), sptField,
                     FxDB(drutama("sikurs"), 0), sptField,
                     FxDB(drutama("sihargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sitotal"), 0), sptField,
                     FxDB(drutama("sidiskonpersen"), ""), sptField,
                     FxDB(drutama("sijmldiskon"), 0), sptField,
                     FxDB(drutama("sitotalpajak1detail"), 0), sptField,
                     FxDB(drutama("sitotalpajak2detail"), 0), sptField,
                     FxDB(drutama("sibiayalainpersen"), ""), sptField,
                     FxDB(drutama("sibiayalain"), 0), sptField,
                     FxDB(drutama("sitotaltransaksi"), 0), sptField,
                     FxDB(drutama("sijmlbayar"), 0), sptField,
                     FxDB(drutama("sistatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sitgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("sinofakturpajak"), ""), sptField,
                     FxDB(drutama("sisdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sitglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("sirekdiskon"), ""), sptField,
                     FxDB(drutama("sirekpajak1"), ""), sptField,
                     FxDB(drutama("sirekpajak2"), ""), sptField,
                     FxDB(drutama("sirekbiayalain"), ""), sptField,
                     FxDB(drutama("sirekbayar"), ""), sptField,
                     FxDB(drutama("siidsq"), ""), sptField,
                     FxDB(drutama("siidso"), ""), sptField,
                     FxDB(drutama("siidpl"), ""), sptField,
                     FxDB(drutama("siiddo"), ""), sptField,
                     FxDB(drutama("siiddr"), ""), sptField,
                     FxDB(drutama("siidpi"), ""), sptField,
                     FxDB(drutama("sistatusrnr"), 0), sptField,
                     FxDB(drutama("sistatussr"), 0), sptField,
                     FxDB(drutama("sistatusrealisasi"), 0), sptField,
                     FxDB(drutama("sistatus"), 0), sptField,
                     FxDB(drutama("sistatussebelumnya"), 0), sptField,
                     FxDB(drutama("sijmlrevisi"), 0), sptField,
                     FxDB(drutama("sicetakanke"), 0), sptField,
                     FxDB(drutama("siinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("siinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("simodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("simodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("siposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("situtupperiode"), 0), sptField,
                     FxDB(drutama("siisclose"), 0), sptField,
                     FxDB(drutama("sicustomtext1"), ""), sptField,
                     FxDB(drutama("sicustomtext2"), ""), sptField,
                     FxDB(drutama("sicustomtext3"), ""), sptField,
                     FxDB(drutama("sicustomtext4"), ""), sptField,
                     FxDB(drutama("sicustomtext5"), ""), sptField,
                     FxDB(drutama("sicustomint1"), 0), sptField,
                     FxDB(drutama("sicustomint2"), 0), sptField,
                     FxDB(drutama("sicustomint3"), 0), sptField,
                     FxDB(drutama("sicustomdbl1"), 0), sptField,
                     FxDB(drutama("sicustomdbl2"), 0), sptField,
                     FxDB(drutama("sicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sicabangnama"), ""), sptField,
                     FxDB(drutama("silokasinama"), ""), sptField,
                     FxDB(drutama("sigudangnama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("sicustomerkode"), ""), sptField,
                     FxDB(drutama("sicustomernama"), ""), sptField,
                     FxDB(drutama("sibagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sibagianpenjualannama"), ""), sptField,
                     FxDB(drutama("siekspedisinama"), ""), sptField,
                     FxDB(drutama("siterminnama"), ""), sptField,
                     FxDB(drutama("siterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sirekdiskonnama"), ""), sptField,
                     FxDB(drutama("sirekpajak1nama"), ""), sptField,
                     FxDB(drutama("sirekpajak2nama"), ""), sptField,
                     FxDB(drutama("sirekbiayalainnama"), ""), sptField,
                     FxDB(drutama("sirekbayarnama"), ""), sptField,
                     FxDB(drutama("sinotransaksiso"), ""), sptField,
                     FxDB(drutama("sinotransaksipl"), ""), sptField,
                     FxDB(drutama("sinotransaksido"), ""), sptField,
                     FxDB(drutama("sinotransaksidr"), ""), sptField,
                     FxDB(drutama("sinotransaksipi"), ""), sptField,
                     FxDB(drutama("sistatusnama"), ""), sptField,
                     FxDB(drutama("sistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("siinputusernama"), ""), sptField,
                     FxDB(drutama("simodifikasiusernama"), ""), sptField,
                     FxDB(drutama("sijmluangmuka"), 0), sptField,
                     FxDB(drutama("sirekuangmuka"), ""), sptField,
                     FxDB(drutama("siidas"), ""), sptField,
                     FxDB(drutama("sirekuangmukanama"), ""), sptField,
                     FxDB(drutama("asnotransaksi"), ""), sptField,
                     FxDB(drutama("sisaldoawal"), 0), sptField,
                     FxDB(drutama("sibayartunai"), 0), sptField,
                     FxDB(drutama("sibayarkkredit"), 0), sptField,
                     FxDB(drutama("sibayarkdebit"), 0), sptField,
                     FxDB(drutama("sibayarvoucher"), 0), sptField,
                     FxDB(drutama("sibayarpoin"), 0), sptField,
                     FxDB(drutama("sibayarjmlpoin"), 0), sptField,
                     FxDB(drutama("sichargepersen"), ""), sptField,
                     FxDB(drutama("sicharge"), 0), sptField,
                     FxDB(drutama("sipoinsebelumnya"), 0), sptField,
                     FxDB(drutama("sipoindidapat"), 0), sptField,
                     FxDB(drutama("sicustomtext6"), ""), sptField,
                     FxDB(drutama("sicustomtext7"), ""), sptField,
                     FxDB(drutama("sicustomtext8"), ""), sptField,
                     FxDB(drutama("sicustomtext9"), ""), sptField,
                     FxDB(drutama("sicustomtext10"), ""), sptField,
                     FxDB(drutama("sicustomint4"), 0), sptField,
                     FxDB(drutama("sicustomint5"), 0), sptField,
                     FxDB(drutama("sicustomint6"), 0), sptField,
                     FxDB(drutama("sicustomint7"), 0), sptField,
                     FxDB(drutama("sicustomint8"), 0), sptField,
                     FxDB(drutama("sicustomint9"), 0), sptField,
                     FxDB(drutama("sicustomint10"), 0), sptField,
                     FxDB(drutama("sicustomdbl4"), 0), sptField,
                     FxDB(drutama("sicustomdbl5"), 0), sptField,
                     FxDB(drutama("sicustomdbl6"), 0), sptField,
                     FxDB(drutama("sicustomdbl7"), 0), sptField,
                     FxDB(drutama("sicustomdbl8"), 0), sptField,
                     FxDB(drutama("sicustomdbl9"), 0), sptField,
                     FxDB(drutama("sicustomdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sicustomdate10"), ""), formatTgl), sptField,
                     FxDB(drutama("sicustomarea"), ""), sptField,
                     FxDB(drutama("sicustomareanama"), ""), sptField,
                     FxDB(drutama("sirekcharge"), ""), sptField,
                     FxDB(drutama("sirekchargenama"), ""), sptField,
                     FxDB(drutama("sijmlkembali"), 0), sptField,
                     FxDB(drutama("sirekkembali"), ""), sptField,
                     FxDB(drutama("sirekkembalinama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsidetail"), 0), sptField,
                     FxDB(dr("idsi"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hargapricelist"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("rekpenjualan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("iddodetail"), 0), sptField,
                     FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
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
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("isbonus"), 0), sptField,
                     FxDB(dr("isbonusfrom"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtidhistory` AS `nbtidhistory`, `nbt`.`nbtidtransaksihistory` AS `nbtidtransaksihistory`,`nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction_history` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksihistory = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch,
                     FxDB(dr("nbtidhistory"), 0), sptField,
                     FxDB(dr("nbtidtransaksihistory"), 0), sptField,
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
                serial = String.Concat(serial,
                     FxDB(dr("nstidhistory"), 0), sptField,
                     FxDB(dr("nstidtransaksihistory"), 0), sptField,
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


            'AMBIL DATA PAY
            sql = "SELECT sip.idhistorycarabayar, sip.idhistory, sip.idsicarabayar AS idsicarabayar, sip.idsi AS idsi, sip.carabayar AS carabayar, sip.matauang AS matauang, sip.kurs AS kurs, sip.jumlah AS jumlah, sip.jumlahvalas AS jumlahvalas, sip.nogiro AS nogiro, sip.tgljt AS tgljt, sip.bank AS bank, sip.noacbank AS noacbank, sip.rekbank AS rekbank, sip.rekgiro AS rekgiro, sip.catatan AS catatan, sip.urutan AS urutan, sip.isclose AS isclose, pm.nama AS carabayarnama, b.bnama AS banknama, coa1.cnama AS rekbanknama, coa2.cnama AS rekgironama FROM m5_si_pay_history AS sip LEFT JOIN m0_payment_method AS pm ON sip.carabayar = pm.kode LEFT JOIN m1_bank AS b ON sip.bank = b.bkode LEFT JOIN m1_coa AS coa1 ON sip.rekbank = coa1.cnomor LEFT JOIN m1_coa AS coa2 ON sip.rekgiro = coa2.cnomor"
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M5_Si_Pay", "idhistory=" & idtransaksi, "idhistory ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idhistorycarabayar"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsicarabayar"), 0), sptField,
                     FxDB(dr("idsi"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siidhistory, siid, sicabang, silokasi, sigudang, siasalbarang, siasalbarangkategori, sijenispenjualan, sijenispenjualankategori, sicarabayar, sisumber, siautonotransaksi, sinotransaksi, sitgl, sikodepa, sicustomer, sicustomerkontak, si1alamat1, si1alamat2, si1alamat3, si2alamat1, si2alamat2, si2alamat3, sibagianpenjualan, siekspedisi, sitglkirim, sitermin, sitgljatuhtempo, siuraian, sicatatan, sinoref, sitglnoref, sitglpenutupan, simatauang, sikurs, sihargatermasukpajak, sitotal, sidiskonpersen, sijmldiskon, sitotalpajak1detail, sitotalpajak2detail, sibiayalainpersen, sibiayalain, sitotaltransaksi, sijmlbayar, sistatuslunas, sitgllunas, sinofakturpajak, sisdhbayarpajak, sitglbayarpajak, sirekdiskon, sirekpajak1, sirekpajak2, sirekbiayalain, sirekbayar, siidsq, siidso, siidpl, siiddo, siiddr, siidpi, sistatusrnr, sistatussr, sistatusrealisasi, sistatus, sistatussebelumnya, sijmlrevisi, sicetakanke, siinputuser, siinputtgl, simodifikasiuser, simodifikasitgl, siposting, sipostingtgl, situtupperiode, siisclose, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, sicustomdate1, sicustomdate2, sicustomdate3, sicabangnama, silokasinama, sigudangnama, ktingkatjual, sicustomerkode, sicustomernama, sibagianpenjualankode, sibagianpenjualannama, siekspedisinama, siterminnama, siterminharijatuhtempo, sirekdiskonnama, sirekpajak1nama, sirekpajak2nama, sirekbiayalainnama, sirekbayarnama, sinotransaksiso, sinotransaksipl, sinotransaksido, sinotransaksidr, sinotransaksipi, sistatusnama, sistatussebelumnyanama, siinputusernama, simodifikasiusernama, sijmluangmuka, sirekuangmuka, siidas, sirekuangmukanama, asnotransaksi, sisaldoawal, sibayartunai, sibayarkkredit, sibayarkdebit, sibayarvoucher, sibayarpoin, sibayarjmlpoin, sichargepersen, sicharge, sipoinsebelumnya, sipoindidapat, sicustomtext6, sicustomtext7, sicustomtext8, sicustomtext9, sicustomtext10, sicustomint4, sicustomint5, sicustomint6, sicustomint7, sicustomint8, sicustomint9, sicustomint10, sicustomdbl4, sicustomdbl5, sicustomdbl6, sicustomdbl7, sicustomdbl8, sicustomdbl9, sicustomdbl10, sicustomdate4, sicustomdate5, sicustomdate6, sicustomdate7, sicustomdate8, sicustomdate9, sicustomdate10, sicustomarea, sicustomareanama, sirekcharge, sirekchargenama, sijmlkembali, sirekkembali, sirekkembalinama" & sptSubParam & "idhistorydetail, idhistory, idsidetail, idsi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususmasuk, idhppfifomasuk, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi, donotransaksi, drnotransaksi, pinotransaksi, isbonus, isbonusfrom" & sptSubParam & "nbtidhistory, nbtidtransaksihistory, nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstidhistory, nstidtransaksihistory, nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "idhistorycarabayar, idhistory, idsicarabayar, idsi, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
