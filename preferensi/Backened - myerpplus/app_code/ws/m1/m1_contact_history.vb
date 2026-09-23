Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_Contact_History
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Contact_HistorySimpan(ByVal param As String) As String
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

        Dim idtransaksi As String = ""

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

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

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
        'idbarang(1) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'idbarang


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 1) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================

        'idbarang(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "id required numeric." : GoTo selesai
        Else
            idtransaksi = dataUtama(0)
        End If
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m1_contact_history(SELECT 0, contact.* FROM m1_contact contact WHERE contact.kid = '" & idtransaksi & "')"
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
            sql = "SELECT kidhistory FROM m1_contact_history WHERE kid = '" & idtransaksi & "' ORDER BY kmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY ATTENTION -----------------------------------
            sql = "INSERT INTO m1_contact_attention_history(SELECT '" & FixDouble(result(4)) & "', 0, contact.* FROM m1_contact_attention contact WHERE contact.kaidkontak = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY ATTENTION ----------------------------


            'PROSES INSERT HISTORY PRICE -----------------------------------
            sql = "INSERT INTO m1_contact_price_history(SELECT '" & FixDouble(result(4)) & "', 0, contact.* FROM m1_contact_price contact WHERE contact.khidkontak = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY PRICE ----------------------------


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
    Public Function M1_Contact_HistorySearch(ByVal param As String) As String
        'M1_Contact_HistorySearch --------------------------------------------------------
        'kidhistory, kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, 
        'kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, 
        'kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, 
        'kterminjual, ktingkatjual, ksalesmankode, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kinputusernama, kmodifikasiusernama

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
            Filter = Filter.Replace("kid", "c.kid")
            Filter = Filter.Replace("kaktif", "c.kaktif")
            Filter = Filter.Replace("knama", "c.knama")
            Filter = Filter.Replace("kkode", "c.kkode")
            Filter = Filter.Replace("kkategori", "c.kkategori")
            Filter = Filter.Replace("k1kota", "c.k1kota")
            Filter = Filter.Replace("karea", "c.karea")
            Filter = Filter.Replace("ksalesmannama", "c.ksalesmannama")
            Filter = Filter.Replace("ksalesmankode", "cs.kkode")
            Filter = Filter.Replace("ksalesman", "c.ksalesman")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_contact_v_history")
        'Return sql
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("kidhistory"), 0), sptField,
                     FxDB(dr("kid"), 0), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptField,
                     FxDB(dr("kkategori"), ""), sptField,
                     FxDB(dr("kkategorinama"), ""), sptField,
                     FxDB(dr("kcabang"), ""), sptField,
                     FxDB(dr("klokasi"), ""), sptField,
                     FxDB(dr("kgudang"), ""), sptField,
                     FxDB(dr("kkategorisalesman"), ""), sptField,
                     FxDB(dr("kkategorisalesmannama"), ""), sptField,
                     FxDB(dr("karea"), ""), sptField,
                     FxDB(dr("kareanama"), ""), sptField,
                     FxDB(dr("kkategoricustomer"), ""), sptField,
                     FxDB(dr("kkategoricustomernama"), ""), sptField,
                     FxDB(dr("kkategorisupplier"), ""), sptField,
                     FxDB(dr("kkategorisuppliernama"), ""), sptField,
                     FxDB(dr("ksalesman"), 0), sptField,
                     FxDB(dr("ksalesmannama"), ""), sptField,
                     FxDB(dr("kkontakperson"), ""), sptField,
                     FxDB(dr("kaktif"), 0), sptField,
                     FxDB(dr("k1alamat1"), ""), sptField,
                     FxDB(dr("k1alamat2"), ""), sptField,
                     FxDB(dr("k1kota"), ""), sptField,
                     FxDB(dr("k1propinsi"), ""), sptField,
                     FxDB(dr("k1kodepos"), ""), sptField,
                     FxDB(dr("k1negara"), ""), sptField,
                     FxDB(dr("k1kontakperson"), ""), sptField,
                     FxDB(dr("k1notelp1"), ""), sptField,
                     FxDB(dr("k2alamat1"), ""), sptField,
                     FxDB(dr("k2alamat2"), ""), sptField,
                     FxDB(dr("k2propinsi"), ""), sptField,
                     FxDB(dr("k2kota"), ""), sptField,
                     FxDB(dr("k2kodepos"), ""), sptField,
                     FxDB(dr("k2negara"), ""), sptField,
                     FxDB(dr("k2kontakperson"), ""), sptField,
                     FxDB(dr("k2notelp1"), ""), sptField,
                     FxDB(dr("kterminbeli"), ""), sptField,
                     FxDB(dr("kterminjual"), ""), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("ksalesmankode"), ""), sptField,
                     FxDB(dr("kinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kinputusernama"), ""), sptField,
                     FxDB(dr("kmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Contact data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kidhistory, kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, kterminjual, ktingkatjual, ksalesmankode, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kinputusernama, kmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Contact_HistoryGetdataById(ByVal param As String) As String

        'M1_Contact_HistoryGetdataById Utama --------------------------------------------------------
        'kidhistory, kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, 
        'ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, 
        'k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, 
        'k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, 
        'k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, 
        'k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, 
        'k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, 
        'k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, 
        'k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, 
        'k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, 
        'k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, 
        'kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, 
        'krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, 
        'knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, 
        'kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, 
        'kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, 
        'kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, 
        'kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, 
        'kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama, kkomisikode, kkomisinama, khargacustom

        'M1_ContactGetdataById Detail -------------------------------------------------------
        'kaidhistorykontak, kaidhistory, kaid, kaidkontak, kakodekontak, kanama, 
        'kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, 
        'kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, 
        'kamodifikasiuser, kamodifikasitgl

        'M1_ContactGetdataById Price -------------------------------------------------------
        'khidhistorykontak, khidhistory, khidkontak, khidbarang, bnama, khsatuan, khkomisi, khhargabeli, khhargajual, 
        'khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, 
        'khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, 
        'khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, 
        'khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5

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

        Dim utama As String = "", detail As String = "", price As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Aj~M2_Aj_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "c1.kidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "c1.kidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_contact_getdata_history")
        sql = "select `c1`.`kidhistory` AS `kidhistory`, `c1`.`kid` AS `kid`, `c1`.`kkode` AS `kkode`, `c1`.`knama` AS `knama`, `c1`.`kkategori` AS `kkategori`, `cc`.`ccnama` AS `kkategorinama`, `c1`.`kcabang` AS `kcabang`, `br`.`bnama` AS `kcabangnama`, `c1`.`klokasi` AS `klokasi`, `l`.`lnama` AS `klokasinama`, `c1`.`kgudang` AS `kgudang`, `w`.`wnama` AS `kgudangnama`, `c1`.`kkategorisalesman` AS `kkategorisalesman`, `sc`.`scnama` AS `kkategorisalesmannama`, `c1`.`karea` AS `karea`, `a`.`anama` AS `kareanama`, `c1`.`kkategoricustomer` AS `kkategoricustomer`, `cusc`.`ccnama` AS `kkategoricustomernama`, `c1`.`kkategorisupplier` AS `kkategorisupplier`, `suppc`.`scnama` AS `kkategorisuppliernama`, `c1`.`kdivisi` AS `kdivisi`, `d`.`dnama` AS `kdivisinama`, `c1`.`ksubdivisi` AS `ksubdivisi`, `sd`.`sdnama` AS `ksubdivisinama`, `c1`.`ksalesman` AS `ksalesman`, `c2`.`knama` AS `ksalesmannama`, `c1`.`kkontakperson` AS `kkontakperson`, `c1`.`kterminglobal` AS `kterminglobal`, `c1`.`kaktif` AS `kaktif`, `c1`.`kaktiftgl` AS `kaktiftgl`, `c1`.`k1alamat1` AS `k1alamat1`, `c1`.`k1alamat2` AS `k1alamat2`, `c1`.`k1alamat3` AS `k1alamat3`, `c1`.`k1alamat4` AS `k1alamat4`, `c1`.`k1alamat5` AS `k1alamat5`, `c1`.`k1kota` AS `k1kota`, `c1`.`k1propinsi` AS `k1propinsi`, `c1`.`k1kodepos` AS `k1kodepos`, `c1`.`k1negara` AS `k1negara`, `c1`.`k1kontakperson` AS `k1kontakperson`, `c1`.`k1kontaknohp` AS `k1kontaknohp`, `c1`.`k1kontakemail` AS `k1kontakemail`, `c1`.`k1notelp1` AS `k1notelp1`, `c1`.`k1notelp2` AS `k1notelp2`, `c1`.`k1nofax` AS `k1nofax`, `c1`.`k1email` AS `k1email`, `c1`.`k1website` AS `k1website`, `c1`.`k2alamat1` AS `k2alamat1`, `c1`.`k2alamat2` AS `k2alamat2`, `c1`.`k2alamat3` AS `k2alamat3`, `c1`.`k2alamat4` AS `k2alamat4`, `c1`.`k2alamat5` AS `k2alamat5`, `c1`.`k2propinsi` AS `k2propinsi`, `c1`.`k2kota` AS `k2kota`, `c1`.`k2kodepos` AS `k2kodepos`, `c1`.`k2negara` AS `k2negara`, `c1`.`k2kontakperson` AS `k2kontakperson`, `c1`.`k2kontaknohp` AS `k2kontaknohp`, `c1`.`k2kontakemail` AS `k2kontakemail`, `c1`.`k2notelp1` AS `k2notelp1`, `c1`.`k2notelp2` AS `k2notelp2`, `c1`.`k2nofax` AS `k2nofax`, `c1`.`k2email` AS `k2email`, `c1`.`k2website` AS `k2website`, `c1`.`k3alamat1` AS `k3alamat1`, `c1`.`k3alamat2` AS `k3alamat2`, `c1`.`k3alamat3` AS `k3alamat3`, `c1`.`k3alamat4` AS `k3alamat4`, `c1`.`k3alamat5` AS `k3alamat5`, `c1`.`k3kota` AS `k3kota`, `c1`.`k3propinsi` AS `k3propinsi`, `c1`.`k3kodepos` AS `k3kodepos`, `c1`.`k3negara` AS `k3negara`, `c1`.`k3kontakperson` AS `k3kontakperson`, `c1`.`k3kontaknohp` AS `k3kontaknohp`, `c1`.`k3kontakemail` AS `k3kontakemail`, `c1`.`k3notelp1` AS `k3notelp1`, `c1`.`k3notelp2` AS `k3notelp2`, `c1`.`k3nofax` AS `k3nofax`, `c1`.`k3email` AS `k3email`, `c1`.`k3website` AS `k3website`, `c1`.`k4alamat1` AS `k4alamat1`, `c1`.`k4alamat2` AS `k4alamat2`, `c1`.`k4alamat3` AS `k4alamat3`, `c1`.`k4alamat4` AS `k4alamat4`, `c1`.`k4alamat5` AS `k4alamat5`, `c1`.`k4kota` AS `k4kota`, `c1`.`k4propinsi` AS `k4propinsi`, `c1`.`k4kodepos` AS `k4kodepos`, `c1`.`k4negara` AS `k4negara`, `c1`.`k4kontakperson` AS `k4kontakperson`, `c1`.`k4kontaknohp` AS `k4kontaknohp`, `c1`.`k4kontakemail` AS `k4kontakemail`, `c1`.`k4notelp1` AS `k4notelp1`, `c1`.`k4notelp2` AS `k4notelp2`, `c1`.`k4nofax` AS `k4nofax`, `c1`.`k4email` AS `k4email`, `c1`.`k4website` AS `k4website`, `c1`.`knpwp` AS `knpwp`, `c1`.`kpkp` AS `kpkp`, `c1`.`kbatashutang` AS `kbatashutang`, `c1`.`kterminbeli` AS `kterminbeli`, `c1`.`krekhutang` AS `krekhutang`, `c1`.`kbagpembelian` AS `kbagpembelian`, `c1`.`kfobbeli` AS `kfobbeli`, `c1`.`kviabeli` AS `kviabeli`, `c1`.`kbataspiutang` AS `kbataspiutang`, `c1`.`kterminjual` AS `kterminjual`, `c1`.`krekpiutang` AS `krekpiutang`, `c1`.`kbagpenjualan` AS `kbagpenjualan`, `c1`.`ktingkatjual` AS `ktingkatjual`, `c1`.`kfobjual` AS `kfobjual`, `c1`.`kviajual` AS `kviajual`, `c1`.`ktglkontrak` AS `ktglkontrak`, `c1`.`kbank` AS `kbank`, `c1`.`knorekening` AS `knorekening`, `c1`.`kjeniskelamin` AS `kjeniskelamin`, `c1`.`kmatauang` AS `kmatauang`, `c1`.`ktgllahir` AS `ktgllahir`, `c1`.`ktglnikah` AS `ktglnikah`, `c1`.`kkomisipenjualan` AS `kkomisipenjualan`, `c1`.`kcatatan` AS `kcatatan`, `c1`.`kinputuser` AS `kinputuser`, `c1`.`kinputtgl` AS `kinputtgl`, `c1`.`kcustomtext1` AS `kcustomtext1`, `c1`.`kcustomtext2` AS `kcustomtext2`, `c1`.`kcustomtext3` AS `kcustomtext3`, `c1`.`kcustomtext4` AS `kcustomtext4`, `c1`.`kcustomtext5` AS `kcustomtext5`, `c1`.`kcustomtext6` AS `kcustomtext6`, `c1`.`kcustomtext7` AS `kcustomtext7`, `c1`.`kcustomtext8` AS `kcustomtext8`, `c1`.`kcustomtext9` AS `kcustomtext9`, `c1`.`kmodifikasiuser` AS `kmodifikasiuser`, `c1`.`kmodifikasitgl` AS `kmodifikasitgl`, `c1`.`kcustomtext10` AS `kcustomtext10`, `c1`.`kcustomint1` AS `kcustomint1`, `c1`.`kcustomint2` AS `kcustomint2`, `c1`.`kcustomint3` AS `kcustomint3`, `c1`.`kcustomdbl1` AS `kcustomdbl1`, `c1`.`kcustomdbl2` AS `kcustomdbl2`, `c1`.`kcustomdbl3` AS `kcustomdbl3`, `c1`.`kcustomdate1` AS `kcustomdate1`, `c1`.`kcustomdate2` AS `kcustomdate2`, `c1`.`kcustomdate3` AS `kcustomdate3`, `c2`.`kkode` AS `ksalesmankode`, `coa1`.`cnama` AS `krekhutangnama`, `c3`.`kkode` AS `kbagpembeliankode`, `c3`.`knama` AS `kbagpembeliannama`, `coa2`.`cnama` AS `krekpiutangnama`, `c4`.`kkode` AS `kbagpenjualankode`, `c4`.`knama` AS `kbagpenjualannama`, `b`.`bnama` AS `kbanknama`, `sr`.`nama` AS `ktingkatjualnama`, c1.kkomisikode, comm.kmnama as kkomisinama, `ca`.`kaidhistorykontak` AS `kaidhistorykontak`, `ca`.`kaidhistory` AS `kaidhistory`, `ca`.`kaid` AS `kaid`, `ca`.`kaidkontak` AS `kaidkontak`, `ca`.`kakodekontak` AS `kakodekontak`, `ca`.`kanama` AS `kanama`, `ca`.`kajabatan` AS `kajabatan`, `ca`.`kanotelp` AS `kanotelp`, `ca`.`kanofax` AS `kanofax`, `ca`.`kanohp` AS `kanohp`, `ca`.`kaemail` AS `kaemail`, `ca`.`kawebsite` AS `kawebsite`, `ca`.`kamessenger` AS `kamessenger`, `ca`.`kaalamat` AS `kaalamat`, `ca`.`katgllahir` AS `katgllahir`, `ca`.`katglnikah` AS `katglnikah`, `ca`.`kacatatan` AS `kacatatan`, `ca`.`kadefault` AS `kadefault`, `ca`.`kainputuser` AS `kainputuser`,  `ca`.`kainputtgl` AS `kainputtgl`,  `ca`.`kamodifikasiuser` AS `kamodifikasiuser`,  `ca`.`kamodifikasitgl` AS `kamodifikasitgl`, c1.khargacustom from `m1_contact_history` `c1`  left join `m1_contact` `c2` on `c1`.`ksalesman` = `c2`.`kid` left join `m1_coa` `coa1` on `c1`.`krekhutang` = `coa1`.`cnomor` left join `m1_contact` `c3` on `c1`.`kbagpembelian` = `c3`.`kid` left join `m1_coa` `coa2` on `c1`.`krekpiutang` = `coa2`.`cnomor` left join `m1_contact` `c4` on `c1`.`kbagpenjualan` = `c4`.`kid` left join `m1_bank` `b` on `c1`.`kbank` = `b`.`bkode` left join `m1_contact_attention_history` `ca` on `c1`.`kidhistory` = `ca`.`kaidhistorykontak` left join `m1_contact_category` `cc` on `c1`.`kkategori` = `cc`.`cckode` left join `m1_branch` `br` on `c1`.`kcabang` = `br`.`bkode` left join `m1_location` `l` on `c1`.`klokasi` = `l`.`lkode` left join `m1_warehouse` `w` on `c1`.`kgudang` = `w`.`wkode` left join `m1_salesman_category` `sc` on `c1`.`kkategorisalesman` = `sc`.`sckode` left join `m1_area` `a` on `c1`.`karea` = `a`.`akode` left join `m1_customer_category` `cusc` on `c1`.`kkategoricustomer` = `cusc`.`cckode` left join `m1_supplier_category` `suppc` on `c1`.`kkategorisupplier` = `suppc`.`sckode` left join `m1_division` `d` on `c1`.`kdivisi` = `d`.`dkode` left join `m1_subdivision` `sd` on `c1`.`ksubdivisi` = `sd`.`sdkode` left join `m0_selling_rate` `sr` on `c1`.`ktingkatjual` = `sr`.`kode` left join m1_commission comm on c1.kkomisikode = comm.kmkode"

        'result(2) = sql & " where " & Filter : GoTo selesai

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kidhistory"), 0), sptField,
                     FxDB(drutama("kid"), 0), sptField,
                     FxDB(drutama("kkode"), ""), sptField,
                     FxDB(drutama("knama"), ""), sptField,
                     FxDB(drutama("kkategori"), ""), sptField,
                     FxDB(drutama("kkategorinama"), ""), sptField,
                     FxDB(drutama("kcabang"), ""), sptField,
                     FxDB(drutama("kcabangnama"), ""), sptField,
                     FxDB(drutama("klokasi"), ""), sptField,
                     FxDB(drutama("klokasinama"), ""), sptField,
                     FxDB(drutama("kgudang"), ""), sptField,
                     FxDB(drutama("kgudangnama"), ""), sptField,
                     FxDB(drutama("kkategorisalesman"), ""), sptField,
                     FxDB(drutama("kkategorisalesmannama"), ""), sptField,
                     FxDB(drutama("karea"), ""), sptField,
                     FxDB(drutama("kareanama"), ""), sptField,
                     FxDB(drutama("kkategoricustomer"), ""), sptField,
                     FxDB(drutama("kkategoricustomernama"), ""), sptField,
                     FxDB(drutama("kkategorisupplier"), ""), sptField,
                     FxDB(drutama("kkategorisuppliernama"), ""), sptField,
                     FxDB(drutama("kdivisi"), ""), sptField,
                     FxDB(drutama("kdivisinama"), ""), sptField,
                     FxDB(drutama("ksubdivisi"), ""), sptField,
                     FxDB(drutama("ksubdivisinama"), ""), sptField,
                     FxDB(drutama("ksalesman"), 0), sptField,
                     FxDB(drutama("ksalesmannama"), ""), sptField,
                     FxDB(drutama("kkontakperson"), ""), sptField,
                     FxDB(drutama("kterminglobal"), 0), sptField,
                     FxDB(drutama("kaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kaktiftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("k1alamat1"), ""), sptField,
                     FxDB(drutama("k1alamat2"), ""), sptField,
                     FxDB(drutama("k1alamat3"), ""), sptField,
                     FxDB(drutama("k1alamat4"), ""), sptField,
                     FxDB(drutama("k1alamat5"), ""), sptField,
                     FxDB(drutama("k1kota"), ""), sptField,
                     FxDB(drutama("k1propinsi"), ""), sptField,
                     FxDB(drutama("k1kodepos"), ""), sptField,
                     FxDB(drutama("k1negara"), ""), sptField,
                     FxDB(drutama("k1kontakperson"), ""), sptField,
                     FxDB(drutama("k1kontaknohp"), ""), sptField,
                     FxDB(drutama("k1kontakemail"), ""), sptField,
                     FxDB(drutama("k1notelp1"), ""), sptField,
                     FxDB(drutama("k1notelp2"), ""), sptField,
                     FxDB(drutama("k1nofax"), ""), sptField,
                     FxDB(drutama("k1email"), ""), sptField,
                     FxDB(drutama("k1website"), ""), sptField,
                     FxDB(drutama("k2alamat1"), ""), sptField,
                     FxDB(drutama("k2alamat2"), ""), sptField,
                     FxDB(drutama("k2alamat3"), ""), sptField,
                     FxDB(drutama("k2alamat4"), ""), sptField,
                     FxDB(drutama("k2alamat5"), ""), sptField,
                     FxDB(drutama("k2propinsi"), ""), sptField,
                     FxDB(drutama("k2kota"), ""), sptField,
                     FxDB(drutama("k2kodepos"), ""), sptField,
                     FxDB(drutama("k2negara"), ""), sptField,
                     FxDB(drutama("k2kontakperson"), ""), sptField,
                     FxDB(drutama("k2kontaknohp"), ""), sptField,
                     FxDB(drutama("k2kontakemail"), ""), sptField,
                     FxDB(drutama("k2notelp1"), ""), sptField,
                     FxDB(drutama("k2notelp2"), ""), sptField,
                     FxDB(drutama("k2nofax"), ""), sptField,
                     FxDB(drutama("k2email"), ""), sptField,
                     FxDB(drutama("k2website"), ""), sptField,
                     FxDB(drutama("k3alamat1"), ""), sptField,
                     FxDB(drutama("k3alamat2"), ""), sptField,
                     FxDB(drutama("k3alamat3"), ""), sptField,
                     FxDB(drutama("k3alamat4"), ""), sptField,
                     FxDB(drutama("k3alamat5"), ""), sptField,
                     FxDB(drutama("k3kota"), ""), sptField,
                     FxDB(drutama("k3propinsi"), ""), sptField,
                     FxDB(drutama("k3kodepos"), ""), sptField,
                     FxDB(drutama("k3negara"), ""), sptField,
                     FxDB(drutama("k3kontakperson"), ""), sptField,
                     FxDB(drutama("k3kontaknohp"), ""), sptField,
                     FxDB(drutama("k3kontakemail"), ""), sptField,
                     FxDB(drutama("k3notelp1"), ""), sptField,
                     FxDB(drutama("k3notelp2"), ""), sptField,
                     FxDB(drutama("k3nofax"), ""), sptField,
                     FxDB(drutama("k3email"), ""), sptField,
                     FxDB(drutama("k3website"), ""), sptField,
                     FxDB(drutama("k4alamat1"), ""), sptField,
                     FxDB(drutama("k4alamat2"), ""), sptField,
                     FxDB(drutama("k4alamat3"), ""), sptField,
                     FxDB(drutama("k4alamat4"), ""), sptField,
                     FxDB(drutama("k4alamat5"), ""), sptField,
                     FxDB(drutama("k4kota"), ""), sptField,
                     FxDB(drutama("k4propinsi"), ""), sptField,
                     FxDB(drutama("k4kodepos"), ""), sptField,
                     FxDB(drutama("k4negara"), ""), sptField,
                     FxDB(drutama("k4kontakperson"), ""), sptField,
                     FxDB(drutama("k4kontaknohp"), ""), sptField,
                     FxDB(drutama("k4kontakemail"), ""), sptField,
                     FxDB(drutama("k4notelp1"), ""), sptField,
                     FxDB(drutama("k4notelp2"), ""), sptField,
                     FxDB(drutama("k4nofax"), ""), sptField,
                     FxDB(drutama("k4email"), ""), sptField,
                     FxDB(drutama("k4website"), ""), sptField,
                     FxDB(drutama("knpwp"), ""), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("kbatashutang"), 0), sptField,
                     FxDB(drutama("kterminbeli"), ""), sptField,
                     FxDB(drutama("krekhutang"), ""), sptField,
                     FxDB(drutama("kbagpembelian"), 0), sptField,
                     FxDB(drutama("kfobbeli"), ""), sptField,
                     FxDB(drutama("kviabeli"), ""), sptField,
                     FxDB(drutama("kbataspiutang"), 0), sptField,
                     FxDB(drutama("kterminjual"), ""), sptField,
                     FxDB(drutama("krekpiutang"), ""), sptField,
                     FxDB(drutama("kbagpenjualan"), 0), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kfobjual"), ""), sptField,
                     FxDB(drutama("kviajual"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ktglkontrak"), ""), formatTgl), sptField,
                     FxDB(drutama("kbank"), ""), sptField,
                     FxDB(drutama("knorekening"), ""), sptField,
                     FxDB(drutama("kjeniskelamin"), 0), sptField,
                     FxDB(drutama("kmatauang"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ktgllahir"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ktglnikah"), ""), formatTgl), sptField,
                     FxDB(drutama("kkomisipenjualan"), 0), sptField,
                     FxDB(drutama("kcatatan"), ""), sptField,
                     FxDB(drutama("kinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kcustomtext1"), ""), sptField,
                     FxDB(drutama("kcustomtext2"), ""), sptField,
                     FxDB(drutama("kcustomtext3"), ""), sptField,
                     FxDB(drutama("kcustomtext4"), ""), sptField,
                     FxDB(drutama("kcustomtext5"), ""), sptField,
                     FxDB(drutama("kcustomtext6"), ""), sptField,
                     FxDB(drutama("kcustomtext7"), ""), sptField,
                     FxDB(drutama("kcustomtext8"), ""), sptField,
                     FxDB(drutama("kcustomtext9"), ""), sptField,
                     FxDB(drutama("kmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kcustomtext10"), ""), sptField,
                     FxDB(drutama("kcustomint1"), 0), sptField,
                     FxDB(drutama("kcustomint2"), 0), sptField,
                     FxDB(drutama("kcustomint3"), 0), sptField,
                     FxDB(drutama("kcustomdbl1"), 0), sptField,
                     FxDB(drutama("kcustomdbl2"), 0), sptField,
                     FxDB(drutama("kcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ksalesmankode"), ""), sptField,
                     FxDB(drutama("krekhutangnama"), ""), sptField,
                     FxDB(drutama("kbagpembeliankode"), ""), sptField,
                     FxDB(drutama("kbagpembeliannama"), ""), sptField,
                     FxDB(drutama("krekpiutangnama"), ""), sptField,
                     FxDB(drutama("kbagpenjualankode"), ""), sptField,
                     FxDB(drutama("kbagpenjualannama"), ""), sptField,
                     FxDB(drutama("kbanknama"), ""), sptField,
                     FxDB(drutama("ktingkatjualnama"), ""), sptField,
                     FxDB(drutama("kkomisikode"), ""), sptField,
                     FxDB(drutama("kkomisinama"), ""), sptField,
                     FxDB(drutama("khargacustom"), "0"))

            Dim tgllahir As String = "", tglnikah As String = "", tglinput As String = "", tglmodif As String = ""

            For Each dr As DataRow In dt.Rows

                'SET FORMAT TGL
                If Len(FxDB(dr("katgllahir"), "")) > 0 Then tgllahir = AsFormatTanggal(FxDB(dr("katgllahir"), ""), formatTgl)
                If Len(FxDB(dr("katglnikah"), "")) > 0 Then tglnikah = AsFormatTanggal(FxDB(dr("katglnikah"), ""), formatTgl)
                If Len(FxDB(dr("kainputtgl"), "")) > 0 Then tglinput = AsFormatTanggal(FxDB(dr("kainputtgl"), ""), formatTglWaktu)
                If Len(FxDB(dr("kamodifikasitgl"), "")) > 0 Then tglmodif = AsFormatTanggal(FxDB(dr("kamodifikasitgl"), ""), formatTglWaktu)

                detail = String.Concat(detail,
                     FxDB(dr("kaidhistorykontak"), 0), sptField,
                     FxDB(dr("kaidhistory"), 0), sptField,
                     FxDB(dr("kaid"), 0), sptField,
                     FxDB(dr("kaidkontak"), 0), sptField,
                     FxDB(dr("kakodekontak"), ""), sptField,
                     FxDB(dr("kanama"), ""), sptField,
                     FxDB(dr("kajabatan"), ""), sptField,
                     FxDB(dr("kanotelp"), ""), sptField,
                     FxDB(dr("kanofax"), ""), sptField,
                     FxDB(dr("kanohp"), ""), sptField,
                     FxDB(dr("kaemail"), ""), sptField,
                     FxDB(dr("kawebsite"), ""), sptField,
                     FxDB(dr("kamessenger"), ""), sptField,
                     FxDB(dr("kaalamat"), ""), sptField,
                     tgllahir, sptField,
                     tglnikah, sptField,
                     FxDB(dr("kacatatan"), ""), sptField,
                     FxDB(dr("kadefault"), 0), sptField,
                     FxDB(dr("kainputuser"), 0), sptField,
                     tglinput, sptField,
                     FxDB(dr("kamodifikasiuser"), 0), sptField,
                     tglmodif, sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA ITEM Price
            Dim dtPrice As New DataTable
            sql = "SELECT cp.khidhistorykontak, cp.khidhistory, cp.khidkontak, cp.khidbarang, i.bkode, i.bnama, cp.khsatuan, cp.khkomisi, cp.khhargabeli, cp.khhargajual, cp.khberlakudari, cp.khberlakusampai, cp.khcatatan, cp.khinputuser, cp.khinputtgl, cp.khmodifikasiuser, cp.khmodifikasitgl, cp.khcustomtext1, cp.khcustomtext2, cp.khcustomtext3, cp.khcustomtext4, cp.khcustomtext5, cp.khcustomint1, cp.khcustomint2, cp.khcustomint3, cp.khcustomint4, cp.khcustomint5, cp.khcustomdbl1, cp.khcustomdbl2, cp.khcustomdbl3, cp.khcustomdbl4, cp.khcustomdbl5, cp.khcustomdate1, cp.khcustomdate2, cp.khcustomdate3, cp.khcustomdate4, cp.khcustomdate5 FROM m1_contact_price_history cp JOIN m1_contact c ON cp.khidkontak = c.kid AND cp.khidkontak = '" & FixDouble(idtransaksi) & "' JOIN m1_item i ON cp.khidbarang = i.bid"
            dtPrice = AmbilData("aplikasi1-M1_Item_Price", "", "cp.khidkontak, i.bkode", True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtPrice.Rows
                price = String.Concat(price,
                     FxDB(dr("khidhistorykontak"), ""), sptField,
                     FxDB(dr("khidhistory"), ""), sptField,
                     FxDB(dr("khidkontak"), ""), sptField,
                     FxDB(dr("khidbarang"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("khsatuan"), ""), sptField,
                     FxDB(dr("khkomisi"), 0), sptField,
                     FxDB(dr("khhargabeli"), 0), sptField,
                     FxDB(dr("khhargajual"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("khberlakudari"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khberlakusampai"), ""), formatTgl), sptField,
                     FxDB(dr("khcatatan"), ""), sptField,
                     FxDB(dr("khinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("khinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("khmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("khmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("khcustomtext1"), ""), sptField,
                     FxDB(dr("khcustomtext2"), ""), sptField,
                     FxDB(dr("khcustomtext3"), ""), sptField,
                     FxDB(dr("khcustomtext4"), ""), sptField,
                     FxDB(dr("khcustomtext5"), ""), sptField,
                     FxDB(dr("khcustomint1"), 0), sptField,
                     FxDB(dr("khcustomint2"), 0), sptField,
                     FxDB(dr("khcustomint3"), 0), sptField,
                     FxDB(dr("khcustomint4"), 0), sptField,
                     FxDB(dr("khcustomint5"), 0), sptField,
                     FxDB(dr("khcustomdbl1"), 0), sptField,
                     FxDB(dr("khcustomdbl2"), 0), sptField,
                     FxDB(dr("khcustomdbl3"), 0), sptField,
                     FxDB(dr("khcustomdbl4"), 0), sptField,
                     FxDB(dr("khcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate5"), ""), formatTgl), sptRow)
            Next
            If price.Length > 0 Then price = price.Substring(0, price.Length - sptRow.Length) Else price = price

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, price)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kidhistory, kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama, kkomisikode, kkomisinama, khargacustom" & sptSubParam & "kaidhistorykontak, kaidhistory, kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl" & sptSubParam & "khidhistorykontak, khidhistory, khidkontak, khidbarang, bkode, bnama, khsatuan, khkomisi, khhargabeli, khhargajual, khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5"))

        Return wsResult
    End Function

End Class
