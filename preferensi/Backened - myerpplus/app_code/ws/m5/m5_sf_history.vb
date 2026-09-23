Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sf_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Sf_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_sf_history(SELECT 0, sf.* FROM m5_sf sf WHERE sf.sfid = '" & idtransaksi & "')"
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
            sql = "SELECT sfidhistory FROM m5_sf_history WHERE sfid = '" & idtransaksi & "' ORDER BY sfmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_sf_detail_history (SELECT 0, '" & result(4) & "', sf.* FROM m5_sf_detail sf WHERE sf.idsf = '" & idtransaksi & "' )"
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
    Public Function M5_Sf_HistorySearch(ByVal param As String) As String
        'M5_Sf_HistorySearch --------------------------------------------------------
        'sfidhistory, sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, 
        'sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, 
        'sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, 
        'sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, 
        'sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, 
        'sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, 
        'sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, 
        'sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, 
        'sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, 
        'sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, 
        'sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama


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
        sql = query.PanggilQuery("m5_sf_v_history")

        dt = AmbilData("aplikasi1-M5_Sf_V_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("sfidhistory"), 0), sptField,
                     FxDB(dr("sfid"), 0), sptField,
                     FxDB(dr("sfcabang"), ""), sptField,
                     FxDB(dr("sflokasi"), ""), sptField,
                     FxDB(dr("sfgudang"), ""), sptField,
                     FxDB(dr("sfasalbarang"), ""), sptField,
                     FxDB(dr("sfasalbarangkategori"), 0), sptField,
                     FxDB(dr("sfjenispenjualan"), ""), sptField,
                     FxDB(dr("sfjenispenjualankategori"), 0), sptField,
                     FxDB(dr("sfcarabayar"), 0), sptField,
                     FxDB(dr("sfsumber"), ""), sptField,
                     FxDB(dr("sfautonotransaksi"), 0), sptField,
                     FxDB(dr("sfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftgl"), ""), formatTgl), sptField,
                     FxDB(dr("sfkodepa"), 0), sptField,
                     FxDB(dr("sfcustomer"), 0), sptField,
                     FxDB(dr("sfcustomerkontak"), ""), sptField,
                     FxDB(dr("sf1alamat1"), ""), sptField,
                     FxDB(dr("sf1alamat2"), ""), sptField,
                     FxDB(dr("sf1alamat3"), ""), sptField,
                     FxDB(dr("sf2alamat1"), ""), sptField,
                     FxDB(dr("sf2alamat2"), ""), sptField,
                     FxDB(dr("sf2alamat3"), ""), sptField,
                     FxDB(dr("sfbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sftglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sftermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sfuraian"), ""), sptField,
                     FxDB(dr("sfcatatan"), ""), sptField,
                     FxDB(dr("sfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("sfmatauang"), ""), sptField,
                     FxDB(dr("sfkurs"), 0), sptField,
                     FxDB(dr("sfhargatermasukpajak"), 0), sptField,
                     FxDB(dr("sftotal"), 0), sptField,
                     FxDB(dr("sfdiskonpersen"), ""), sptField,
                     FxDB(dr("sfjmldiskon"), 0), sptField,
                     FxDB(dr("sftotalpajak1detail"), 0), sptField,
                     FxDB(dr("sftotalpajak2detail"), 0), sptField,
                     FxDB(dr("sfbiayalainpersen"), 0), sptField,
                     FxDB(dr("sfbiayalain"), 0), sptField,
                     FxDB(dr("sftotaltransaksi"), 0), sptField,
                     FxDB(dr("sfstatuspr"), 0), sptField,
                     FxDB(dr("sfstatusso"), 0), sptField,
                     FxDB(dr("sfstatuspl"), 0), sptField,
                     FxDB(dr("sfstatusdo"), 0), sptField,
                     FxDB(dr("sfstatusdr"), 0), sptField,
                     FxDB(dr("sfstatuspi"), 0), sptField,
                     FxDB(dr("sfstatussi"), 0), sptField,
                     FxDB(dr("sfstatusrnr"), 0), sptField,
                     FxDB(dr("sfstatussr"), 0), sptField,
                     FxDB(dr("sfstatusrealisasi"), 0), sptField,
                     FxDB(dr("sfstatus"), 0), sptField,
                     FxDB(dr("sfstatussebelumnya"), 0), sptField,
                     FxDB(dr("sfjmlrevisi"), 0), sptField,
                     FxDB(dr("sfcetakanke"), 0), sptField,
                     FxDB(dr("sfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sfisclose"), 0), sptField,
                     FxDB(dr("sfcabangnama"), ""), sptField,
                     FxDB(dr("sflokasinama"), ""), sptField,
                     FxDB(dr("sfgudangnama"), ""), sptField,
                     FxDB(dr("sfcustomerkode"), ""), sptField,
                     FxDB(dr("sfcustomernama"), ""), sptField,
                     FxDB(dr("sfbagianpenjualankode"), ""), sptField,
                     FxDB(dr("sfbagianpenjualannama"), ""), sptField,
                     FxDB(dr("sfstatusnama"), ""), sptField,
                     FxDB(dr("sfstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sfinputusernama"), ""), sptField,
                     FxDB(dr("sfmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfidhistory, sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SfHistoryGetdataById(ByVal param As String) As String
        'M5_SfHistoryGetdataById Utama --------------------------------------------------------
        'sfidhistory, sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, 
        'sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, 
        'sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, 
        'sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, 
        'sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, 
        'sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, 
        'sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, 
        'sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, 
        'sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, 
        'sfcustomtext1, sfcustomtext2, sfcustomtext3, sfcustomtext4, sfcustomtext5, sfcustomint1, sfcustomint2, 
        'sfcustomint3, sfcustomdbl1, sfcustomdbl2, sfcustomdbl3, sfcustomdate1, sfcustomdate2, sfcustomdate3, 
        'sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, 
        'sfterminnama, sfterminharijatuhtempo, sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama

        'M5_SfHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Sf_history~M5_Sf_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sfidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sfidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_sf_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sfidhistory"), 0), sptField, FxDB(drutama("sfid"), 0), sptField,
                     FxDB(drutama("sfcabang"), ""), sptField,
                     FxDB(drutama("sflokasi"), ""), sptField,
                     FxDB(drutama("sfgudang"), ""), sptField,
                     FxDB(drutama("sfasalbarang"), ""), sptField,
                     FxDB(drutama("sfasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sfjenispenjualan"), ""), sptField,
                     FxDB(drutama("sfjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("sfcarabayar"), 0), sptField,
                     FxDB(drutama("sfsumber"), ""), sptField,
                     FxDB(drutama("sfautonotransaksi"), 0), sptField,
                     FxDB(drutama("sfnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sfkodepa"), 0), sptField,
                     FxDB(drutama("sfcustomer"), 0), sptField,
                     FxDB(drutama("sfcustomerkontak"), ""), sptField,
                     FxDB(drutama("sf1alamat1"), ""), sptField,
                     FxDB(drutama("sf1alamat2"), ""), sptField,
                     FxDB(drutama("sf1alamat3"), ""), sptField,
                     FxDB(drutama("sf2alamat1"), ""), sptField,
                     FxDB(drutama("sf2alamat2"), ""), sptField,
                     FxDB(drutama("sf2alamat3"), ""), sptField,
                     FxDB(drutama("sfbagianpenjualan"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sftglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sftermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sftgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("sfuraian"), ""), sptField,
                     FxDB(drutama("sfcatatan"), ""), sptField,
                     FxDB(drutama("sfnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sftglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("sfmatauang"), ""), sptField,
                     FxDB(drutama("sfkurs"), 0), sptField,
                     FxDB(drutama("sfhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sftotal"), 0), sptField,
                     FxDB(drutama("sfdiskonpersen"), ""), sptField,
                     FxDB(drutama("sfjmldiskon"), 0), sptField,
                     FxDB(drutama("sftotalpajak1detail"), 0), sptField,
                     FxDB(drutama("sftotalpajak2detail"), 0), sptField,
                     FxDB(drutama("sfbiayalainpersen"), 0), sptField,
                     FxDB(drutama("sfbiayalain"), 0), sptField,
                     FxDB(drutama("sftotaltransaksi"), 0), sptField,
                     FxDB(drutama("sfstatuspr"), 0), sptField,
                     FxDB(drutama("sfstatusso"), 0), sptField,
                     FxDB(drutama("sfstatuspl"), 0), sptField,
                     FxDB(drutama("sfstatusdo"), 0), sptField,
                     FxDB(drutama("sfstatusdr"), 0), sptField,
                     FxDB(drutama("sfstatuspi"), 0), sptField,
                     FxDB(drutama("sfstatussi"), 0), sptField,
                     FxDB(drutama("sfstatusrnr"), 0), sptField,
                     FxDB(drutama("sfstatussr"), 0), sptField,
                     FxDB(drutama("sfstatusrealisasi"), 0), sptField,
                     FxDB(drutama("sfstatus"), 0), sptField,
                     FxDB(drutama("sfstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sfjmlrevisi"), 0), sptField,
                     FxDB(drutama("sfcetakanke"), 0), sptField,
                     FxDB(drutama("sfinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sfmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sfposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sfisclose"), 0), sptField,
                     FxDB(drutama("sfcustomtext1"), ""), sptField,
                     FxDB(drutama("sfcustomtext2"), ""), sptField,
                     FxDB(drutama("sfcustomtext3"), ""), sptField,
                     FxDB(drutama("sfcustomtext4"), ""), sptField,
                     FxDB(drutama("sfcustomtext5"), ""), sptField,
                     FxDB(drutama("sfcustomint1"), 0), sptField,
                     FxDB(drutama("sfcustomint2"), 0), sptField,
                     FxDB(drutama("sfcustomint3"), 0), sptField,
                     FxDB(drutama("sfcustomdbl1"), 0), sptField,
                     FxDB(drutama("sfcustomdbl2"), 0), sptField,
                     FxDB(drutama("sfcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sfcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sfcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sfcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sfcabangnama"), ""), sptField,
                     FxDB(drutama("sflokasinama"), ""), sptField,
                     FxDB(drutama("sfgudangnama"), ""), sptField,
                     FxDB(drutama("sfcustomerkode"), ""), sptField,
                     FxDB(drutama("sfcustomernama"), ""), sptField,
                     FxDB(drutama("sfbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sfbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("sfterminnama"), ""), sptField,
                     FxDB(drutama("sfterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sfstatusnama"), ""), sptField,
                     FxDB(drutama("sfstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sfinputusernama"), ""), sptField,
                     FxDB(drutama("sfmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsfdetail"), 0), sptField,
                     FxDB(dr("idsf"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfidhistory, sfid, sfcabang, sflokasi, sfgudang, sfasalbarang, sfasalbarangkategori, sfjenispenjualan, sfjenispenjualankategori, sfcarabayar, sfsumber, sfautonotransaksi, sfnotransaksi, sftgl, sfkodepa, sfcustomer, sfcustomerkontak, sf1alamat1, sf1alamat2, sf1alamat3, sf2alamat1, sf2alamat2, sf2alamat3, sfbagianpenjualan, sftglkirim, sftermin, sftgljatuhtempo, sfuraian, sfcatatan, sfnoref, sftglnoref, sftglpenutupan, sfmatauang, sfkurs, sfhargatermasukpajak, sftotal, sfdiskonpersen, sfjmldiskon, sftotalpajak1detail, sftotalpajak2detail, sfbiayalainpersen, sfbiayalain, sftotaltransaksi, sfstatuspr, sfstatusso, sfstatuspl, sfstatusdo, sfstatusdr, sfstatuspi, sfstatussi, sfstatusrnr, sfstatussr, sfstatusrealisasi, sfstatus, sfstatussebelumnya, sfjmlrevisi, sfcetakanke, sfinputuser, sfinputtgl, sfmodifikasiuser, sfmodifikasitgl, sfposting, sfpostingtgl, sfisclose, sfcustomtext1, sfcustomtext2, sfcustomtext3, sfcustomtext4, sfcustomtext5, sfcustomint1, sfcustomint2, sfcustomint3, sfcustomdbl1, sfcustomdbl2, sfcustomdbl3, sfcustomdate1, sfcustomdate2, sfcustomdate3, sfcabangnama, sflokasinama, sfgudangnama, sfcustomerkode, sfcustomernama, sfbagianpenjualankode, sfbagianpenjualannama, sfterminnama, sfterminharijatuhtempo, sfstatusnama, sfstatussebelumnyanama, sfinputusernama, sfmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idsfdetail, idsf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlpr, statuspr, jmlso, statusso, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama"))

        Return wsResult
    End Function

End Class
