Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sq_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Sq_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_sq_history(SELECT 0, sq.* FROM m5_sq sq WHERE sq.sqid = '" & idtransaksi & "')"
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
            sql = "SELECT sqidhistory FROM m5_sq_history WHERE sqid = '" & idtransaksi & "' ORDER BY sqmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_sq_detail_history (SELECT 0, '" & result(4) & "', sq.* FROM m5_sq_detail sq WHERE sq.idsq = '" & idtransaksi & "' )"
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
    Public Function M5_Sq_HistorySearch(ByVal param As String) As String
        'M5_Sq_HistorySearch --------------------------------------------------------
        'sqidhistory, sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, 
        'sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, 
        'sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, 
        'sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, 
        'sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, 
        'sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, 
        'sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, 
        'sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, 
        'sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, 
        'sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama


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
        sql = query.PanggilQuery("m5_sq_v_history")

        dt = AmbilData("aplikasi1-M5_Sq_V_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("sqid"), 0), sptField,
                     FxDB(dr("sqidhistory"), 0), sptField,
                     FxDB(dr("sqcabang"), ""), sptField,
                     FxDB(dr("sqlokasi"), ""), sptField,
                     FxDB(dr("sqgudang"), ""), sptField,
                     FxDB(dr("sqasalbarang"), ""), sptField,
                     FxDB(dr("sqasalbarangkategori"), 0), sptField,
                     FxDB(dr("sqjenispenjualan"), ""), sptField,
                     FxDB(dr("sqjenispenjualankategori"), 0), sptField,
                     FxDB(dr("sqcarabayar"), 0), sptField,
                     FxDB(dr("sqsumber"), ""), sptField,
                     FxDB(dr("sqautonotransaksi"), 0), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtgl"), ""), formatTgl), sptField,
                     FxDB(dr("sqkodepa"), 0), sptField,
                     FxDB(dr("sqcustomer"), 0), sptField,
                     FxDB(dr("sqcustomerkontak"), ""), sptField,
                     FxDB(dr("sq1alamat1"), ""), sptField,
                     FxDB(dr("sq1alamat2"), ""), sptField,
                     FxDB(dr("sq1alamat3"), ""), sptField,
                     FxDB(dr("sq2alamat1"), ""), sptField,
                     FxDB(dr("sq2alamat2"), ""), sptField,
                     FxDB(dr("sq2alamat3"), ""), sptField,
                     FxDB(dr("sqbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("squraian"), ""), sptField,
                     FxDB(dr("sqcatatan"), ""), sptField,
                     FxDB(dr("sqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("sqmatauang"), ""), sptField,
                     FxDB(dr("sqkurs"), 0), sptField,
                     FxDB(dr("sqhargatermasukpajak"), 0), sptField,
                     FxDB(dr("sqtotal"), 0), sptField,
                     FxDB(dr("sqdiskonpersen"), ""), sptField,
                     FxDB(dr("sqjmldiskon"), 0), sptField,
                     FxDB(dr("sqtotalpajak1detail"), 0), sptField,
                     FxDB(dr("sqtotalpajak2detail"), 0), sptField,
                     FxDB(dr("sqbiayalainpersen"), 0), sptField,
                     FxDB(dr("sqbiayalain"), 0), sptField,
                     FxDB(dr("sqtotaltransaksi"), 0), sptField,
                     FxDB(dr("sqstatuspr"), 0), sptField,
                     FxDB(dr("sqstatusso"), 0), sptField,
                     FxDB(dr("sqstatuspl"), 0), sptField,
                     FxDB(dr("sqstatusdo"), 0), sptField,
                     FxDB(dr("sqstatusdr"), 0), sptField,
                     FxDB(dr("sqstatuspi"), 0), sptField,
                     FxDB(dr("sqstatussi"), 0), sptField,
                     FxDB(dr("sqstatusrnr"), 0), sptField,
                     FxDB(dr("sqstatussr"), 0), sptField,
                     FxDB(dr("sqstatusrealisasi"), 0), sptField,
                     FxDB(dr("sqstatus"), 0), sptField,
                     FxDB(dr("sqstatussebelumnya"), 0), sptField,
                     FxDB(dr("sqjmlrevisi"), 0), sptField,
                     FxDB(dr("sqcetakanke"), 0), sptField,
                     FxDB(dr("sqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sqisclose"), 0), sptField,
                     FxDB(dr("sqcabangnama"), ""), sptField,
                     FxDB(dr("sqlokasinama"), ""), sptField,
                     FxDB(dr("sqgudangnama"), ""), sptField,
                     FxDB(dr("sqcustomerkode"), ""), sptField,
                     FxDB(dr("sqcustomernama"), ""), sptField,
                     FxDB(dr("sqbagianpenjualankode"), ""), sptField,
                     FxDB(dr("sqbagianpenjualannama"), ""), sptField,
                     FxDB(dr("sqstatusnama"), ""), sptField,
                     FxDB(dr("sqstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sqinputusernama"), ""), sptField,
                     FxDB(dr("sqmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sqidhistory, sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SqHistoryGetdataById(ByVal param As String) As String
        'M5_SqHistoryGetdataById Utama --------------------------------------------------------
        'sqidhistory, sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, 
        'sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, 
        'sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, 
        'sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, 
        'sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, 
        'sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, 
        'sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, 
        'sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, 
        'sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, 
        'sqcustomtext1, sqcustomtext2, sqcustomtext3, sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, 
        'sqcustomint3, sqcustomdbl1, sqcustomdbl2, sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3, 
        'sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, 
        'sqterminnama, sqterminharijatuhtempo, sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama

        'M5_SqHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, 
        'statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, 
        'jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, 
        'statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama

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

        Dim NmMemcached As String = "aplikasi1-M5_Sq_history~M5_Sq_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sqidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sqidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_sq_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sqidhistory"), 0), sptField, FxDB(drutama("sqid"), 0), sptField,
                     FxDB(drutama("sqcabang"), ""), sptField,
                     FxDB(drutama("sqlokasi"), ""), sptField,
                     FxDB(drutama("sqgudang"), ""), sptField,
                     FxDB(drutama("sqasalbarang"), ""), sptField,
                     FxDB(drutama("sqasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sqjenispenjualan"), ""), sptField,
                     FxDB(drutama("sqjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("sqcarabayar"), 0), sptField,
                     FxDB(drutama("sqsumber"), ""), sptField,
                     FxDB(drutama("sqautonotransaksi"), 0), sptField,
                     FxDB(drutama("sqnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sqkodepa"), 0), sptField,
                     FxDB(drutama("sqcustomer"), 0), sptField,
                     FxDB(drutama("sqcustomerkontak"), ""), sptField,
                     FxDB(drutama("sq1alamat1"), ""), sptField,
                     FxDB(drutama("sq1alamat2"), ""), sptField,
                     FxDB(drutama("sq1alamat3"), ""), sptField,
                     FxDB(drutama("sq2alamat1"), ""), sptField,
                     FxDB(drutama("sq2alamat2"), ""), sptField,
                     FxDB(drutama("sq2alamat3"), ""), sptField,
                     FxDB(drutama("sqbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sqtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("squraian"), ""), sptField,
                     FxDB(drutama("sqcatatan"), ""), sptField,
                     FxDB(drutama("sqnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sqtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("sqmatauang"), ""), sptField,
                     FxDB(drutama("sqkurs"), 0), sptField,
                     FxDB(drutama("sqhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sqtotal"), 0), sptField,
                     FxDB(drutama("sqdiskonpersen"), ""), sptField,
                     FxDB(drutama("sqjmldiskon"), 0), sptField,
                     FxDB(drutama("sqtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("sqtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("sqbiayalainpersen"), 0), sptField,
                     FxDB(drutama("sqbiayalain"), 0), sptField,
                     FxDB(drutama("sqtotaltransaksi"), 0), sptField,
                     FxDB(drutama("sqstatuspr"), 0), sptField,
                     FxDB(drutama("sqstatusso"), 0), sptField,
                     FxDB(drutama("sqstatuspl"), 0), sptField,
                     FxDB(drutama("sqstatusdo"), 0), sptField,
                     FxDB(drutama("sqstatusdr"), 0), sptField,
                     FxDB(drutama("sqstatuspi"), 0), sptField,
                     FxDB(drutama("sqstatussi"), 0), sptField,
                     FxDB(drutama("sqstatusrnr"), 0), sptField,
                     FxDB(drutama("sqstatussr"), 0), sptField,
                     FxDB(drutama("sqstatusrealisasi"), 0), sptField,
                     FxDB(drutama("sqstatus"), 0), sptField,
                     FxDB(drutama("sqstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sqjmlrevisi"), 0), sptField,
                     FxDB(drutama("sqcetakanke"), 0), sptField,
                     FxDB(drutama("sqinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sqmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sqposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sqisclose"), 0), sptField,
                     FxDB(drutama("sqcustomtext1"), ""), sptField,
                     FxDB(drutama("sqcustomtext2"), ""), sptField,
                     FxDB(drutama("sqcustomtext3"), ""), sptField,
                     FxDB(drutama("sqcustomtext4"), ""), sptField,
                     FxDB(drutama("sqcustomtext5"), ""), sptField,
                     FxDB(drutama("sqcustomint1"), 0), sptField,
                     FxDB(drutama("sqcustomint2"), 0), sptField,
                     FxDB(drutama("sqcustomint3"), 0), sptField,
                     FxDB(drutama("sqcustomdbl1"), 0), sptField,
                     FxDB(drutama("sqcustomdbl2"), 0), sptField,
                     FxDB(drutama("sqcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sqcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sqcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sqcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sqcabangnama"), ""), sptField,
                     FxDB(drutama("sqlokasinama"), ""), sptField,
                     FxDB(drutama("sqgudangnama"), ""), sptField,
                     FxDB(drutama("sqcustomerkode"), ""), sptField,
                     FxDB(drutama("sqcustomernama"), ""), sptField,
                     FxDB(drutama("sqbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sqbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("sqterminnama"), ""), sptField,
                     FxDB(drutama("sqterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sqstatusnama"), ""), sptField,
                     FxDB(drutama("sqstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sqinputusernama"), ""), sptField,
                     FxDB(drutama("sqmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsq"), 0), sptField,
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
                     FxDB(dr("jmlpr"), 0), sptField,
                     FxDB(dr("statuspr"), 0), sptField,
                     FxDB(dr("jmlso"), 0), sptField,
                     FxDB(dr("statusso"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
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
                     FxDB(dr("proyeknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sqidhistory, sqid, sqcabang, sqlokasi, sqgudang, sqasalbarang, sqasalbarangkategori, sqjenispenjualan, sqjenispenjualankategori, sqcarabayar, sqsumber, sqautonotransaksi, sqnotransaksi, sqtgl, sqkodepa, sqcustomer, sqcustomerkontak, sq1alamat1, sq1alamat2, sq1alamat3, sq2alamat1, sq2alamat2, sq2alamat3, sqbagianpenjualan, sqtglkirim, sqtermin, sqtgljatuhtempo, squraian, sqcatatan, sqnoref, sqtglnoref, sqtglpenutupan, sqmatauang, sqkurs, sqhargatermasukpajak, sqtotal, sqdiskonpersen, sqjmldiskon, sqtotalpajak1detail, sqtotalpajak2detail, sqbiayalainpersen, sqbiayalain, sqtotaltransaksi, sqstatuspr, sqstatusso, sqstatuspl, sqstatusdo, sqstatusdr, sqstatuspi, sqstatussi, sqstatusrnr, sqstatussr, sqstatusrealisasi, sqstatus, sqstatussebelumnya, sqjmlrevisi, sqcetakanke, sqinputuser, sqinputtgl, sqmodifikasiuser, sqmodifikasitgl, sqposting, sqpostingtgl, sqisclose, sqcustomtext1, sqcustomtext2, sqcustomtext3, sqcustomtext4, sqcustomtext5, sqcustomint1, sqcustomint2, sqcustomint3, sqcustomdbl1, sqcustomdbl2, sqcustomdbl3, sqcustomdate1, sqcustomdate2, sqcustomdate3, sqcabangnama, sqlokasinama, sqgudangnama, sqcustomerkode, sqcustomernama, sqbagianpenjualankode, sqbagianpenjualannama, sqterminnama, sqterminharijatuhtempo, sqstatusnama, sqstatussebelumnyanama, sqinputusernama, sqmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idsqdetail, idsq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

End Class
