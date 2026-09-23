Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_pi_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Pi_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_pi_history(SELECT 0, pi.* FROM m5_pi pi WHERE pi.piid = '" & idtransaksi & "')"
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
            sql = "SELECT piidhistory FROM m5_pi_history WHERE piid = '" & idtransaksi & "' ORDER BY pimodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_pi_detail_history (SELECT 0, '" & result(4) & "', pi.* FROM m5_pi_detail pi WHERE pi.idpi = '" & idtransaksi & "' )"
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
    Public Function M5_Pi_HistorySearch(ByVal param As String) As String
        'M5_Pi_HistorySearch --------------------------------------------------------
        'piidhistory, piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, 
        'pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, 
        'picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, 
        'pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, 
        'pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, 
        'pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, 
        'piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, 
        'pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, 
        'piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, 
        'picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, 
        'piekspedisinama, sqnotransaksi, sonotransaksi, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama

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
        sql = query.PanggilQuery("m5_pi_v_history")

        dt = AmbilData("aplikasi1-M5_pi_V_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("piid"), 0), sptField,
                     FxDB(dr("piidhistory"), 0), sptField,
                     FxDB(dr("picabang"), ""), sptField,
                     FxDB(dr("pilokasi"), ""), sptField,
                     FxDB(dr("pigudang"), ""), sptField,
                     FxDB(dr("piasalbarang"), ""), sptField,
                     FxDB(dr("piasalbarangkategori"), 0), sptField,
                     FxDB(dr("pijenispenjualan"), ""), sptField,
                     FxDB(dr("pijenispenjualankategori"), 0), sptField,
                     FxDB(dr("picarabayar"), 0), sptField,
                     FxDB(dr("pisumber"), ""), sptField,
                     FxDB(dr("piautonotransaksi"), 0), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitgl"), ""), formatTgl), sptField,
                     FxDB(dr("pikodepa"), 0), sptField,
                     FxDB(dr("picustomer"), 0), sptField,
                     FxDB(dr("picustomerkontak"), ""), sptField,
                     FxDB(dr("pi1alamat1"), ""), sptField,
                     FxDB(dr("pi1alamat2"), ""), sptField,
                     FxDB(dr("pi1alamat3"), ""), sptField,
                     FxDB(dr("pi2alamat1"), ""), sptField,
                     FxDB(dr("pi2alamat2"), ""), sptField,
                     FxDB(dr("pi2alamat3"), ""), sptField,
                     FxDB(dr("pibagianpenjualan"), 0), sptField,
                     FxDB(dr("piekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("pitermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("piuraian"), ""), sptField,
                     FxDB(dr("picatatan"), ""), sptField,
                     FxDB(dr("pinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pitglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("pimatauang"), ""), sptField,
                     FxDB(dr("pikurs"), 0), sptField,
                     FxDB(dr("pihargatermasukpajak"), 0), sptField,
                     FxDB(dr("pitotal"), 0), sptField,
                     FxDB(dr("pidiskonpersen"), ""), sptField,
                     FxDB(dr("pijmldiskon"), 0), sptField,
                     FxDB(dr("pitotalpajak1detail"), 0), sptField,
                     FxDB(dr("pitotalpajak2detail"), 0), sptField,
                     FxDB(dr("pibiayalainpersen"), 0), sptField,
                     FxDB(dr("pibiayalain"), 0), sptField,
                     FxDB(dr("pitotaltransaksi"), 0), sptField,
                     FxDB(dr("pijmlbayar"), 0), sptField,
                     FxDB(dr("pirekdiskon"), ""), sptField,
                     FxDB(dr("pirekpajak1"), ""), sptField,
                     FxDB(dr("pirekpajak2"), ""), sptField,
                     FxDB(dr("pirekbiayalain"), ""), sptField,
                     FxDB(dr("pirekbayar"), ""), sptField,
                     FxDB(dr("piidsq"), 0), sptField,
                     FxDB(dr("piidso"), 0), sptField,
                     FxDB(dr("pistatuspl"), 0), sptField,
                     FxDB(dr("pistatusdo"), 0), sptField,
                     FxDB(dr("pistatusdr"), 0), sptField,
                     FxDB(dr("pistatussi"), 0), sptField,
                     FxDB(dr("pistatusrnr"), 0), sptField,
                     FxDB(dr("pistatussr"), 0), sptField,
                     FxDB(dr("pistatusrealisasi"), 0), sptField,
                     FxDB(dr("pistatus"), 0), sptField,
                     FxDB(dr("pistatussebelumnya"), 0), sptField,
                     FxDB(dr("pijmlrevisi"), 0), sptField,
                     FxDB(dr("picetakanke"), 0), sptField,
                     FxDB(dr("piinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("piinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("piisclose"), 0), sptField,
                     FxDB(dr("pitutupperiode"), 0), sptField,
                     FxDB(dr("picabangnama"), ""), sptField,
                     FxDB(dr("pilokasinama"), ""), sptField,
                     FxDB(dr("pigudangnama"), ""), sptField,
                     FxDB(dr("picustomerkode"), ""), sptField,
                     FxDB(dr("picustomernama"), ""), sptField,
                     FxDB(dr("pibagianpenjualankode"), ""), sptField,
                     FxDB(dr("pibagianpenjualannama"), ""), sptField,
                     FxDB(dr("piekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("pistatusnama"), ""), sptField,
                     FxDB(dr("pistatussebelumnyanama"), ""), sptField,
                     FxDB(dr("piinputusernama"), ""), sptField,
                     FxDB(dr("pimodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("piidhistory, piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, piekspedisinama, sqnotransaksi, sonotransaksi, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PiHistoryGetdataById(ByVal param As String) As String
        'M5_PiHistoryGetdataById Utama --------------------------------------------------------
        'piidhistory, piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, 
        'pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, 
        'picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, 
        'pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, 
        'picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, 
        'pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, 
        'pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, 
        'piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, 
        'pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, 
        'piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, 
        'picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, 
        'picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, 
        'picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, 
        'piekspedisinama, piterminnama, piterminharijatuhtempo, pirekdiskonnama, pirekpajak1nama, pirekpajak2nama, pirekbiayalainnama, 
        'pirekbayarnama, pinotransaksiso, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama

        'M5_PiHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, 
        'idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M5_pi~M5_pi_Detail-" & idtransaksi

        'Repiace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi repiace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "piidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "piidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_pi_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("piidhistory"), 0), sptField, FxDB(drutama("piid"), 0), sptField,
                     FxDB(drutama("picabang"), ""), sptField,
                     FxDB(drutama("pilokasi"), ""), sptField,
                     FxDB(drutama("pigudang"), ""), sptField,
                     FxDB(drutama("piasalbarang"), ""), sptField,
                     FxDB(drutama("piasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pijenispenjualan"), ""), sptField,
                     FxDB(drutama("pijenispenjualankategori"), 0), sptField,
                     FxDB(drutama("picarabayar"), 0), sptField,
                     FxDB(drutama("pisumber"), ""), sptField,
                     FxDB(drutama("piautonotransaksi"), 0), sptField,
                     FxDB(drutama("pinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pikodepa"), 0), sptField,
                     FxDB(drutama("picustomer"), 0), sptField,
                     FxDB(drutama("picustomerkontak"), ""), sptField,
                     FxDB(drutama("pi1alamat1"), ""), sptField,
                     FxDB(drutama("pi1alamat2"), ""), sptField,
                     FxDB(drutama("pi1alamat3"), ""), sptField,
                     FxDB(drutama("pi2alamat1"), ""), sptField,
                     FxDB(drutama("pi2alamat2"), ""), sptField,
                     FxDB(drutama("pi2alamat3"), ""), sptField,
                     FxDB(drutama("pibagianpenjualan"), 0), sptField,
                     FxDB(drutama("piekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("pitermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("piuraian"), ""), sptField,
                     FxDB(drutama("picatatan"), ""), sptField,
                     FxDB(drutama("pinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pitglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pitglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("pimatauang"), ""), sptField,
                     FxDB(drutama("pikurs"), 0), sptField,
                     FxDB(drutama("pihargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pitotal"), 0), sptField,
                     FxDB(drutama("pidiskonpersen"), ""), sptField,
                     FxDB(drutama("pijmldiskon"), 0), sptField,
                     FxDB(drutama("pitotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pitotalpajak2detail"), 0), sptField,
                     FxDB(drutama("pibiayalainpersen"), 0), sptField,
                     FxDB(drutama("pibiayalain"), 0), sptField,
                     FxDB(drutama("pitotaltransaksi"), 0), sptField,
                     FxDB(drutama("pijmlbayar"), 0), sptField,
                     FxDB(drutama("pirekdiskon"), ""), sptField,
                     FxDB(drutama("pirekpajak1"), ""), sptField,
                     FxDB(drutama("pirekpajak2"), ""), sptField,
                     FxDB(drutama("pirekbiayalain"), ""), sptField,
                     FxDB(drutama("pirekbayar"), ""), sptField,
                     FxDB(drutama("piidsq"), 0), sptField,
                     FxDB(drutama("piidso"), 0), sptField,
                     FxDB(drutama("pistatuspl"), 0), sptField,
                     FxDB(drutama("pistatusdo"), 0), sptField,
                     FxDB(drutama("pistatusdr"), 0), sptField,
                     FxDB(drutama("pistatussi"), 0), sptField,
                     FxDB(drutama("pistatusrnr"), 0), sptField,
                     FxDB(drutama("pistatussr"), 0), sptField,
                     FxDB(drutama("pistatusrealisasi"), 0), sptField,
                     FxDB(drutama("pistatus"), 0), sptField,
                     FxDB(drutama("pistatussebelumnya"), 0), sptField,
                     FxDB(drutama("pijmlrevisi"), 0), sptField,
                     FxDB(drutama("picetakanke"), 0), sptField,
                     FxDB(drutama("piinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("piinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("piposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("piisclose"), 0), sptField,
                     FxDB(drutama("pitutupperiode"), 0), sptField,
                     FxDB(drutama("picustomtext1"), ""), sptField,
                     FxDB(drutama("picustomtext2"), ""), sptField,
                     FxDB(drutama("picustomtext3"), ""), sptField,
                     FxDB(drutama("picustomtext4"), ""), sptField,
                     FxDB(drutama("picustomtext5"), ""), sptField,
                     FxDB(drutama("picustomint1"), 0), sptField,
                     FxDB(drutama("picustomint2"), 0), sptField,
                     FxDB(drutama("picustomint3"), 0), sptField,
                     FxDB(drutama("picustomdbl1"), 0), sptField,
                     FxDB(drutama("picustomdbl2"), 0), sptField,
                     FxDB(drutama("picustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("picustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("picustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("picustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("picabangnama"), ""), sptField,
                     FxDB(drutama("pilokasinama"), ""), sptField,
                     FxDB(drutama("pigudangnama"), ""), sptField,
                     FxDB(drutama("picustomerkode"), ""), sptField,
                     FxDB(drutama("picustomernama"), ""), sptField,
                     FxDB(drutama("pibagianpenjualankode"), ""), sptField,
                     FxDB(drutama("pibagianpenjualannama"), ""), sptField,
                     FxDB(drutama("piekspedisinama"), ""), sptField,
                     FxDB(drutama("piterminnama"), ""), sptField,
                     FxDB(drutama("piterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("pirekdiskonnama"), ""), sptField,
                     FxDB(drutama("pirekpajak1nama"), ""), sptField,
                     FxDB(drutama("pirekpajak2nama"), ""), sptField,
                     FxDB(drutama("pirekbiayalainnama"), ""), sptField,
                     FxDB(drutama("pirekbayarnama"), ""), sptField,
                     FxDB(drutama("pinotransaksiso"), ""), sptField,
                     FxDB(drutama("pistatusnama"), ""), sptField,
                     FxDB(drutama("pistatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("piinputusernama"), ""), sptField,
                     FxDB(drutama("pimodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idpi"), 0), sptField,
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
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
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
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("piidhistory, piid, picabang, pilokasi, pigudang, piasalbarang, piasalbarangkategori, pijenispenjualan, pijenispenjualankategori, picarabayar, pisumber, piautonotransaksi, pinotransaksi, pitgl, pikodepa, picustomer, picustomerkontak, pi1alamat1, pi1alamat2, pi1alamat3, pi2alamat1, pi2alamat2, pi2alamat3, pibagianpenjualan, piekspedisi, pitglkirim, pitermin, pitgljatuhtempo, piuraian, picatatan, pinoref, pitglnoref, pitglpenutupan, pimatauang, pikurs, pihargatermasukpajak, pitotal, pidiskonpersen, pijmldiskon, pitotalpajak1detail, pitotalpajak2detail, pibiayalainpersen, pibiayalain, pitotaltransaksi, pijmlbayar, pirekdiskon, pirekpajak1, pirekpajak2, pirekbiayalain, pirekbayar, piidsq, piidso, pistatuspl, pistatusdo, pistatusdr, pistatussi, pistatusrnr, pistatussr, pistatusrealisasi, pistatus, pistatussebelumnya, pijmlrevisi, picetakanke, piinputuser, piinputtgl, pimodifikasiuser, pimodifikasitgl, piposting, pipostingtgl, piisclose, pitutupperiode, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, picabangnama, pilokasinama, pigudangnama, picustomerkode, picustomernama, pibagianpenjualankode, pibagianpenjualannama, piekspedisinama, piterminnama, piterminharijatuhtempo, pirekdiskonnama, pirekpajak1nama, pirekpajak2nama, pirekbiayalainnama, pirekbayarnama, pinotransaksiso, pistatusnama, pistatussebelumnyanama, piinputusernama, pimodifikasiusernama"), sptSubParam, ReplaceMapping("idhistorydetail, idhistory, idpidetail, idpi, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sonotransaksi"))

        Return wsResult
    End Function

End Class
