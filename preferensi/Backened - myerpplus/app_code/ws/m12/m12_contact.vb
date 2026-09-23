Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_contact
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M12_ContactSearch(ByVal param As String) As String
        'M12_ContactSearch --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, 
        'kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, 
        'kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, 
        'kterminjual, ktingkatjual, ksalesmankode

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'PANGGIL QUERY
        sql = "select `c`.`kid` AS `kid`,`c`.`kkode` AS `kkode`,`c`.`knama` AS `knama`,`c`.`kkategori` AS `kkategori`,`cc`.`ccnama` AS `kkategorinama`,`c`.`kcabang` AS `kcabang`,`c`.`klokasi` AS `klokasi`,`c`.`kgudang` AS `kgudang`,`c`.`kkategorisalesman` AS `kkategorisalesman`,`sc`.`scnama` AS `kkategorisalesmannama`,`c`.`karea` AS `karea`,`a`.`anama` AS `kareanama`,`c`.`kkategoricustomer` AS `kkategoricustomer`,`custc`.`ccnama` AS `kkategoricustomernama`,`c`.`kkategorisupplier` AS `kkategorisupplier`,`suppc`.`scnama` AS `kkategorisuppliernama`,`c`.`ksalesman` AS `ksalesman`,`cs`.`knama` AS `ksalesmannama`,`ca`.`kanama` AS `kkontakperson`,`c`.`kaktif` AS `kaktif`,`c`.`k1alamat1` AS `k1alamat1`,`c`.`k1alamat2` AS `k1alamat2`,`c`.`k1kota` AS `k1kota`,`c`.`k1propinsi` AS `k1propinsi`,`c`.`k1kodepos` AS `k1kodepos`,`c`.`k1negara` AS `k1negara`,`c`.`k1kontakperson` AS `k1kontakperson`,`c`.`k1notelp1` AS `k1notelp1`,`c`.`k2alamat1` AS `k2alamat1`,`c`.`k2alamat2` AS `k2alamat2`,`c`.`k2propinsi` AS `k2propinsi`,`c`.`k2kota` AS `k2kota`,`c`.`k2kodepos` AS `k2kodepos`,`c`.`k2negara` AS `k2negara`,`c`.`k2kontakperson` AS `k2kontakperson`,`c`.`k2notelp1` AS `k2notelp1`,`c`.`kterminbeli` AS `kterminbeli`,`c`.`kterminjual` AS `kterminjual`,`c`.`ktingkatjual` AS `ktingkatjual`,`c`.`kkomisipenjualan` AS `kkomisipenjualan`,`cs`.`kkode` AS `ksalesmankode` from ((((((((`m1_contact` `c` join `m0_user` `u` on(((`u`.`userid` = 'valuserid') and ((`c`.`klokasi` = '') or (`c`.`klokasi` = `u`.`ulokasi`))))) left join `m1_contact` `cs` on((`c`.`ksalesman` = `cs`.`kid`))) left join `m1_contact_attention` `ca` on(((`c`.`kid` = `ca`.`kaidkontak`) and (`ca`.`kadefault` = 1)))) left join `m1_area` `a` on((`c`.`karea` = `a`.`akode`))) left join `m1_contact_category` `cc` on((`c`.`kkategori` = `cc`.`cckode`))) left join `m1_salesman_category` `sc` on((`c`.`kkategorisalesman` = `sc`.`sckode`))) left join `m1_customer_category` `custc` on((`c`.`kkategoricustomer` = `custc`.`cckode`))) left join `m1_supplier_category` `suppc` on((`c`.`kkategorisupplier` = `suppc`.`sckode`)))"
        sql = sql.Replace("valuserid", userid)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
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
                     FxDB(dr("ksalesmankode"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, kterminjual, ktingkatjual, ksalesmankode"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_ContactGetdataById(ByVal param As String) As String

        'M12_ContactGetdataById Utama --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
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
        'kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama

        'M12_ContactGetdataById Detail -------------------------------------------------------
        'kaid, kaidkontak, kakodekontak, kanama, 
        'kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, 
        'kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, 
        'kamodifikasiuser, kamodifikasitgl

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
            Filter = "c1.kid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "c1.kid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_contact_getdata")

        'result(2) = sql & " where " & Filter : GoTo selesai

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kid"), 0), sptField,
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
                     FxDB(drutama("ktingkatjualnama"), ""))

            Dim tgllahir As String = "", tglnikah As String = "", tglinput As String = "", tglmodif As String = ""

            For Each dr As DataRow In dt.Rows

                'SET FORMAT TGL
                If Len(FxDB(dr("katgllahir"), "")) > 0 Then tgllahir = AsFormatTanggal(FxDB(dr("katgllahir"), ""), formatTgl)
                If Len(FxDB(dr("katglnikah"), "")) > 0 Then tglnikah = AsFormatTanggal(FxDB(dr("katglnikah"), ""), formatTgl)
                If Len(FxDB(dr("kainputtgl"), "")) > 0 Then tglinput = AsFormatTanggal(FxDB(dr("kainputtgl"), ""), formatTglWaktu)
                If Len(FxDB(dr("kamodifikasitgl"), "")) > 0 Then tglmodif = AsFormatTanggal(FxDB(dr("kamodifikasitgl"), ""), formatTglWaktu)

                detail = String.Concat(detail, FxDB(dr("kaid"), 0), sptField,
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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama" & sptSubParam & "kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl"))

        Return wsResult
    End Function

End Class
