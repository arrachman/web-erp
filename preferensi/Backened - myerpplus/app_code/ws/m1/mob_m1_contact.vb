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
Public Class mob_m1_contact
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function MobM1_ContactSearch(ByVal param As String) As String
        'MobM1_ContactSearch --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, 
        'ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, 
        'kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, 
        'k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, 
        'k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, 
        'k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, 
        'k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, 
        'k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, 
        'k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, 
        'k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, 
        'k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, 
        'kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, 
        'kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, 
        'ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, 
        'kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kcustomtext1, 
        'kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, 
        'kcustomtext9, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, 
        'kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron

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
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
        '    result(2) = "Access denied for get data"
        'End If
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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m1_contact_cd")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kid"), ""), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptField,
                     FxDB(dr("kkategori"), ""), sptField,
                     FxDB(dr("kkategorinama"), ""), sptField,
                     FxDB(dr("kcabang"), ""), sptField,
                     FxDB(dr("kcabangnama"), ""), sptField,
                     FxDB(dr("klokasi"), ""), sptField,
                     FxDB(dr("klokasinama"), ""), sptField,
                     FxDB(dr("kgudang"), ""), sptField,
                     FxDB(dr("kgudangnama"), ""), sptField,
                     FxDB(dr("kkategorisalesman"), ""), sptField,
                     FxDB(dr("kkategorisalesmannama"), ""), sptField,
                     FxDB(dr("karea"), ""), sptField,
                     FxDB(dr("kareanama"), ""), sptField,
                     FxDB(dr("kkategoricustomer"), ""), sptField,
                     FxDB(dr("kkategoricustomernama"), ""), sptField,
                     FxDB(dr("kkategorisupplier"), ""), sptField,
                     FxDB(dr("kkategorisuppliernama"), ""), sptField,
                     FxDB(dr("kdivisi"), ""), sptField,
                     FxDB(dr("kdivisinama"), ""), sptField,
                     FxDB(dr("ksubdivisi"), ""), sptField,
                     FxDB(dr("ksubdivisinama"), ""), sptField,
                     FxDB(dr("ksalesman"), ""), sptField,
                     FxDB(dr("ksalesmannama"), ""), sptField,
                     FxDB(dr("kkontakperson"), ""), sptField,
                     FxDB(dr("kterminglobal"), 0), sptField,
                     FxDB(dr("kaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kaktiftgl"), ""), formatTgl), sptField,
                     FxDB(dr("k1alamat1"), ""), sptField,
                     FxDB(dr("k1alamat2"), ""), sptField,
                     FxDB(dr("k1alamat3"), ""), sptField,
                     FxDB(dr("k1alamat4"), ""), sptField,
                     FxDB(dr("k1alamat5"), ""), sptField,
                     FxDB(dr("k1kota"), ""), sptField,
                     FxDB(dr("k1propinsi"), ""), sptField,
                     FxDB(dr("k1kodepos"), ""), sptField,
                     FxDB(dr("k1negara"), ""), sptField,
                     FxDB(dr("k1kontakperson"), ""), sptField,
                     FxDB(dr("k1kontaknohp"), ""), sptField,
                     FxDB(dr("k1kontakemail"), ""), sptField,
                     FxDB(dr("k1notelp1"), ""), sptField,
                     FxDB(dr("k1notelp2"), ""), sptField,
                     FxDB(dr("k1nofax"), ""), sptField,
                     FxDB(dr("k1email"), ""), sptField,
                     FxDB(dr("k1website"), ""), sptField,
                     FxDB(dr("k2alamat1"), ""), sptField,
                     FxDB(dr("k2alamat2"), ""), sptField,
                     FxDB(dr("k2alamat3"), ""), sptField,
                     FxDB(dr("k2alamat4"), ""), sptField,
                     FxDB(dr("k2alamat5"), ""), sptField,
                     FxDB(dr("k2propinsi"), ""), sptField,
                     FxDB(dr("k2kota"), ""), sptField,
                     FxDB(dr("k2kodepos"), ""), sptField,
                     FxDB(dr("k2negara"), ""), sptField,
                     FxDB(dr("k2kontakperson"), ""), sptField,
                     FxDB(dr("k2kontaknohp"), ""), sptField,
                     FxDB(dr("k2kontakemail"), ""), sptField,
                     FxDB(dr("k2notelp1"), ""), sptField,
                     FxDB(dr("k2notelp2"), ""), sptField,
                     FxDB(dr("k2nofax"), ""), sptField,
                     FxDB(dr("k2email"), ""), sptField,
                     FxDB(dr("k2website"), ""), sptField,
                     FxDB(dr("k3alamat1"), ""), sptField,
                     FxDB(dr("k3alamat2"), ""), sptField,
                     FxDB(dr("k3alamat3"), ""), sptField,
                     FxDB(dr("k3alamat4"), ""), sptField,
                     FxDB(dr("k3alamat5"), ""), sptField,
                     FxDB(dr("k3kota"), ""), sptField,
                     FxDB(dr("k3propinsi"), ""), sptField,
                     FxDB(dr("k3kodepos"), ""), sptField,
                     FxDB(dr("k3negara"), ""), sptField,
                     FxDB(dr("k3kontakperson"), ""), sptField,
                     FxDB(dr("k3kontaknohp"), ""), sptField,
                     FxDB(dr("k3kontakemail"), ""), sptField,
                     FxDB(dr("k3notelp1"), ""), sptField,
                     FxDB(dr("k3notelp2"), ""), sptField,
                     FxDB(dr("k3nofax"), ""), sptField,
                     FxDB(dr("k3email"), ""), sptField,
                     FxDB(dr("k3website"), ""), sptField,
                     FxDB(dr("k4alamat1"), ""), sptField,
                     FxDB(dr("k4alamat2"), ""), sptField,
                     FxDB(dr("k4alamat3"), ""), sptField,
                     FxDB(dr("k4alamat4"), ""), sptField,
                     FxDB(dr("k4alamat5"), ""), sptField,
                     FxDB(dr("k4kota"), ""), sptField,
                     FxDB(dr("k4propinsi"), ""), sptField,
                     FxDB(dr("k4kodepos"), ""), sptField,
                     FxDB(dr("k4negara"), ""), sptField,
                     FxDB(dr("k4kontakperson"), ""), sptField,
                     FxDB(dr("k4kontaknohp"), ""), sptField,
                     FxDB(dr("k4kontakemail"), ""), sptField,
                     FxDB(dr("k4notelp1"), ""), sptField,
                     FxDB(dr("k4notelp2"), ""), sptField,
                     FxDB(dr("k4nofax"), ""), sptField,
                     FxDB(dr("k4email"), ""), sptField,
                     FxDB(dr("k4website"), ""), sptField,
                     FxDB(dr("knpwp"), ""), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("kbatashutang"), 0), sptField,
                     FxDB(dr("kterminbeli"), ""), sptField,
                     FxDB(dr("krekhutang"), ""), sptField,
                     FxDB(dr("kbagpembelian"), ""), sptField,
                     FxDB(dr("kfobbeli"), ""), sptField,
                     FxDB(dr("kviabeli"), ""), sptField,
                     FxDB(dr("kbataspiutang"), 0), sptField,
                     FxDB(dr("kterminjual"), ""), sptField,
                     FxDB(dr("krekpiutang"), ""), sptField,
                     FxDB(dr("kbagpenjualan"), ""), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("kfobjual"), ""), sptField,
                     FxDB(dr("kviajual"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ktglkontrak"), ""), formatTgl), sptField,
                     FxDB(dr("kbank"), ""), sptField,
                     FxDB(dr("knorekening"), ""), sptField,
                     FxDB(dr("kjeniskelamin"), 0), sptField,
                     FxDB(dr("kmatauang"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ktgllahir"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ktglnikah"), ""), formatTgl), sptField,
                     FxDB(dr("kkomisipenjualan"), 0), sptField,
                     FxDB(dr("kcatatan"), ""), sptField,
                     FxDB(dr("kinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kcustomtext1"), ""), sptField,
                     FxDB(dr("kcustomtext2"), ""), sptField,
                     FxDB(dr("kcustomtext3"), ""), sptField,
                     FxDB(dr("kcustomtext4"), ""), sptField,
                     FxDB(dr("kcustomtext5"), ""), sptField,
                     FxDB(dr("kcustomtext6"), ""), sptField,
                     FxDB(dr("kcustomtext7"), ""), sptField,
                     FxDB(dr("kcustomtext8"), ""), sptField,
                     FxDB(dr("kcustomtext9"), ""), sptField,
                     FxDB(dr("kcustomtext10"), ""), sptField,
                     FxDB(dr("kcustomint1"), 0), sptField,
                     FxDB(dr("kcustomint2"), 0), sptField,
                     FxDB(dr("kcustomint3"), 0), sptField,
                     FxDB(dr("kcustomdbl1"), 0), sptField,
                     FxDB(dr("kcustomdbl2"), 0), sptField,
                     FxDB(dr("kcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ksinkron"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function MobM1_ContactSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama(), dataRowUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData, deviceUUID As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        'Dim dt As DataTable
        Dim pg1 As New RsPaging

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
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
        isUpdate = False
        'END OF VALIDASI DAN SET USERID ====================================================

        'Device UUID
        If (Len(paramSplit(4)) = 0) Then
            result(2) = "Device UUID can't be empty" : GoTo selesai
        Else
            deviceUUID = paramSplit(4).ToString
        End If

        'Query (cek userid apa sudah ada di tabel)
        sql = "SELECT * FROM m0_nomor_mobile WHERE macaddress = '" + deviceUUID + "' AND userid = " + userid
        Dim dt As DataTable = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count = 0 Then
            result(2) = "User has logged on other device." : GoTo selesai
        End If

        'MAPPING BUAT WS ----------------------------------------------------------
        'kid(0) As Integer, kkode(1) As String, knama(2) As String, kkategori(3) As String, kkategorinama(4) As String, 
        'kcabang(5) As String, kcabangnama(6) As String, klokasi(7) As String, klokasinama(8) As String, kgudang(9) As String, 
        'kgudangnama(10) As String, kkategorisalesman(11) As String, kkategorisalesmannama(12) As String, karea(13) As String, kareanama(14) As String, 
        'kkategoricustomer(15) As String, kkategoricustomernama(16) As String, kdivisi(17) As String, kdivisinama(18) As String, ksubdivisi(19) As String, 
        'ksubdivisinama(20) As String, ksalesman(21) As Integer, ksalesmannama(22) As String, kkontakperson(23) As String, kterminglobal(24) As Integer, 
        'kaktif(25) As Integer, kaktiftgl(26) As Date, k1alamat1(27) As String, k1alamat2(28) As String, k1alamat3(29) As String, 
        'k1alamat4(30) As String, k1alamat5(31) As String, k1kota(32) As String, k1propinsi(33) As String, k1kodepos(34) As String, 
        'k1negara(35) As String, k1kontakperson(36) As String, k1kontaknohp(37) As String, k1kontakemail(38) As String, k1notelp1(39) As String, 
        'k1notelp2(40) As String, k1nofax(41) As String, k1email(42) As String, k1website(43) As String, k2alamat1(44) As String, 
        'k2alamat2(45) As String, k2alamat3(46) As String, k2alamat4(47) As String, k2alamat5(48) As String, k2propinsi(49) As String, 
        'k2kota(50) As String, k2kodepos(51) As String, k2negara(52) As String, k2kontakperson(53) As String, k2kontaknohp(54) As String, 
        'k2kontakemail(55) As String, k2notelp1(56) As String, k2notelp2(57) As String, k2nofax(58) As String, k2email(59) As String, 
        'k2website(60) As String, k3alamat1(61) As String, k3alamat2(62) As String, k3alamat3(63) As String, k3alamat4(64) As String, 
        'k3alamat5(65) As String, k3kota(66) As String, k3propinsi(67) As String, k3kodepos(68) As String, k3negara(69) As String, 
        'k3kontakperson(70) As String, k3kontaknohp(71) As String, k3kontakemail(72) As String, k3notelp1(73) As String, k3notelp2(74) As String, 
        'k3nofax(75) As String, k3email(76) As String, k3website(77) As String, k4alamat1(78) As String, k4alamat2(79) As String, 
        'k4alamat3(80) As String, k4alamat4(81) As String, k4alamat5(82) As String, k4kota(83) As String, k4propinsi(84) As String, 
        'k4kodepos(85) As String, k4negara(86) As String, k4kontakperson(87) As String, k4kontaknohp(88) As String, k4kontakemail(89) As String, 
        'k4notelp1(90) As String, k4notelp2(91) As String, k4nofax(92) As String, k4email(93) As String, k4website(94) As String, 
        'knpwp(95) As String, kpkp(96) As Integer, kbatashutang(97) As Double, kterminbeli(98) As String, krekhutang(99) As String, 
        'kbagpembelian(100) As Integer, kfobbeli(101) As String, kviabeli(102) As String, kbataspiutang(103) As Double, kterminjual(104) As String, 
        'krekpiutang(105) As String, kbagpenjualan(106) As Integer, ktingkatjual(107) As Integer, kfobjual(108) As String, kviajual(109) As String, 
        'ktglkontrak(110) As Date, kbank(111) As String, knorekening(112) As String, kjeniskelamin(113) As Integer, kmatauang(114) As String, 
        'ktgllahir(115) As Date, ktglnikah(116) As Date, kkomisipenjualan(117) As Double, kcatatan(118) As String, kinputuser(119) As Integer, 
        'kinputtgl(120) As DateTime, kcustomtext1(121) As String, kcustomtext2(122) As String, kcustomtext3(123) As String, kcustomtext4(124) As String, 
        'kcustomtext5(125) As String, kcustomtext6(126) As String, kcustomtext7(127) As String, kcustomtext8(128) As String, kcustomtext9(129) As String, 
        'kmodifikasiuser(130) As Integer, kmodifikasitgl(131) As DateTime, kcustomtext10(132) As String, kcustomint1(133) As Integer, kcustomint2(134) As Integer, 
        'kcustomint3(135) As Integer, kcustomdbl1(136) As Double, kcustomdbl2(137) As Double, kcustomdbl3(138) As Double, kcustomdate1(139) As Date, 
        'kcustomdate2(140) As Date, kcustomdate3(141) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang,
        'kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kdivisi, kdivisinama, ksubdivisi, 
        'ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, 
        'k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, 
        'k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, 
        'k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, 
        'k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, 
        'k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, 
        'k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, 
        'k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, 
        'kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, 
        'ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, 
        'kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, 
        'kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, 
        'kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, 
        'kcustomdate2, kcustomdate3

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptRow)



        'deklarasi letakkan disini
        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcabangnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "klokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "klokasinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kgudangnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisalesman", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisalesmannama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "karea", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kareanama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategoricustomer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategoricustomernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kdivisinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksubdivisinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksalesman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ksalesmannama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminglobal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kaktiftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knpwp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kpkp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kbatashutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "krekhutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbagpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kfobbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kviabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbataspiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "krekpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbagpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ktingkatjual", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kfobjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kviajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktglkontrak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knorekening", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjeniskelamin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktglnikah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkomisipenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate3", AsEnumTypeData.AsString)

        Dim JmlDt As Integer = dataUtama.Length
        Dim DataParam As String = paramSplit(5)
        If DataParam.Length > 0 Then

            For i = 1 To JmlDt
                dataRowUtama = dataUtama(i - 1).Split(sptField)
                'CEK ARRAY DATA
                If (dataRowUtama.Length <> 142) Then
                    result(2) = "Row :" & i & " - Invalid Data Parameter" : GoTo selesai
                    'result(2) = "Row :" & i & " - Invalid Data Parameter" : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ================================================

                'VALIDASI TIPE DATA ==========================================================
                'kid(0) As Integer
                If (IsNumeric(dataRowUtama(0)) = False) Then
                    result(2) = "Row : " & i & " - kid required numeric." : GoTo selesai
                End If
                'ksalesman(21) As Integer
                If (IsNumeric(dataRowUtama(21)) = False) Then
                    result(2) = "Row : " & i & " - ksalesman required numeric." : GoTo selesai
                End If
                'kterminglobal(24) As Integer
                If (IsNumeric(dataRowUtama(24)) = False) Then
                    result(2) = "Row : " & i & " - kterminglobal required numeric." : GoTo selesai
                End If
                'kaktif(25) As Integer
                If (IsNumeric(dataRowUtama(25)) = False) Then
                    result(2) = "Row : " & i & " - kaktif required numeric." : GoTo selesai
                End If
                'kaktiftgl(26) As Date
                If (IsDate(dataRowUtama(26)) = False) Then
                    result(2) = "Row : " & i & " - kaktiftgl required date." : GoTo selesai
                End If
                'kpkp(96) As Integer
                If (IsNumeric(dataRowUtama(96)) = False) Then
                    result(2) = "Row : " & i & " -  kpkp required numeric." : GoTo selesai
                End If
                'kbatashutang(97) As Double
                If (IsNumeric(dataRowUtama(97)) = False) Then
                    result(2) = "Row : " & i & " - kbatashutang required numeric." : GoTo selesai
                End If
                'kbagpembelian(100) As Integer
                If (IsNumeric(dataRowUtama(100)) = False) Then
                    result(2) = "Row : " & i & " - kbagpembelian required numeric." : GoTo selesai
                End If
                'kbataspiutang(103) As Double
                If (IsNumeric(dataRowUtama(103)) = False) Then
                    result(2) = "Row : " & i & " - kbataspiutang required numeric." : GoTo selesai
                End If
                'kbagpenjualan(106) As Integer
                If (IsNumeric(dataRowUtama(106)) = False) Then
                    result(2) = "Row : " & i & " - kbagpenjualan required numeric." : GoTo selesai
                End If
                'ktingkatjual(107) As Integer
                If (IsNumeric(dataRowUtama(107)) = False) Then
                    result(2) = "Row : " & i & " - ktingkatjual required numeric." : GoTo selesai
                End If
                'ktglkontrak(110) As Date
                If (IsDate(dataRowUtama(110)) = False) Then
                    result(2) = "Row : " & i & " - ktglkontrak required date." : GoTo selesai
                End If
                'kjeniskelamin(113) As Integer
                If (IsNumeric(dataRowUtama(113)) = False) Then
                    result(2) = "Row : " & i & " - kjeniskelamin required numeric." : GoTo selesai
                End If
                'ktgllahir(115) As Date
                If (IsDate(dataRowUtama(115)) = False) Then
                    result(2) = "Row : " & i & " - ktgllahir required date." : GoTo selesai
                End If
                'ktglnikah(116) As Date
                If (IsDate(dataRowUtama(116)) = False) Then
                    result(2) = "Row : " & i & " - ktglnikah required date." : GoTo selesai
                End If
                'kkomisipenjualan(117) As Double
                If (IsNumeric(dataRowUtama(117)) = False) Then
                    result(2) = "Row : " & i & " - kkomisipenjualan required numeric." : GoTo selesai
                End If
                'kinputuser(119) As Integer
                If (IsNumeric(dataRowUtama(119)) = False) Then
                    result(2) = "Row : " & i & " - kinputuser required numeric." : GoTo selesai
                End If
                'kinputtgl(120) As DateTime
                If (IsDate(dataRowUtama(120)) = False) Then
                    result(2) = "Row : " & i & " - kinputtgl required date." : GoTo selesai
                End If
                'kmodifikasiuser(130) As Integer
                If (IsNumeric(dataRowUtama(130)) = False) Then
                    result(2) = "Row : " & i & " - kmodifikasiuser required numeric." : GoTo selesai
                End If
                'kmodifikasitgl(131) As DateTime
                If (IsDate(dataRowUtama(131)) = False) Then
                    result(2) = "Row : " & i & " - kmodifikasitgl required date." : GoTo selesai
                End If
                'kcustomint1(133) As Integer
                If (IsNumeric(dataRowUtama(133)) = False) Then
                    result(2) = "Row : " & i & " - kcustomint1 required numeric." : GoTo selesai
                End If
                'kcustomint2(134) As Integer
                If (IsNumeric(dataRowUtama(134)) = False) Then
                    result(2) = "Row : " & i & " - kcustomint2 required numeric." : GoTo selesai
                End If
                'kcustomint3(135) As Integer
                If (IsNumeric(dataRowUtama(135)) = False) Then
                    result(2) = "Row : " & i & " - kcustomint3 required numeric." : GoTo selesai
                End If
                'kcustomdbl1(136) As Double
                If (IsNumeric(dataRowUtama(136)) = False) Then
                    result(2) = "Row : " & i & " - kcustomdbl1 required numeric." : GoTo selesai
                End If
                'kcustomdbl2(137) As Double
                If (IsNumeric(dataRowUtama(137)) = False) Then
                    result(2) = "Row : " & i & " - kcustomdbl2 required numeric." : GoTo selesai
                End If
                'kcustomdbl3(138) As Double
                If (IsNumeric(dataRowUtama(138)) = False) Then
                    result(2) = "Row : " & i & " - kcustomdbl3 required numeric." : GoTo selesai
                End If
                'kcustomdate1(139) As Date
                If (IsDate(dataRowUtama(139)) = False) Then
                    result(2) = "Row : " & i & " - kcustomdate1 required date." : GoTo selesai
                End If
                'kcustomdate2(140) As Date
                If (IsDate(dataRowUtama(140)) = False) Then
                    result(2) = "Row : " & i & " - kcustomdate2 required date." : GoTo selesai
                End If
                'kcustomdate3(141) As Date
                If (IsDate(dataRowUtama(141)) = False) Then
                    result(2) = "Row : " & i & " - kcustomdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA ===================================================

                'VALIDASI DATA ===============================================================
                'kkode(1) As String
                If Len(dataRowUtama(1)) = 0 Then
                    result(2) = "Row : " & i & " - kkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowUtama(1)) > 25 Then
                    result(2) = "Row : " & i & " - kkode should not be more than 25 character." : GoTo selesai
                End If
                If Len(dataRowUtama(2)) = 0 Then
                    result(2) = "Row : " & i & " - knama can't be empty" : GoTo selesai
                End If
                If Len(dataRowUtama(2)) > 100 Then
                    result(2) = "Row : " & i & " - knama should not be more than 100 character." : GoTo selesai
                End If


                'kkategori(3) As String
                If Len(dataRowUtama(3)) = 0 Then
                    result(2) = "Row : " & i & " - kkategori can't be empty" : GoTo selesai
                End If
                If Len(dataRowUtama(3)) > 3 Then
                    result(2) = "Row : " & i & " - kkategori should not be more than 3 character." : GoTo selesai
                End If

                'kaktiftgl(26) As Date
                If Len(dataRowUtama(26)) = 0 Then
                    result(2) = "Row : " & i & " - kaktiftgl can't be empty" : GoTo selesai
                End If

                'kinputtgl(120) As DateTime
                If Len(dataRowUtama(120)) = 0 Then
                    result(2) = "Row : " & i & " - kinputtgl can't be empty" : GoTo selesai
                End If

                'kmodifikasitgl(131) As DateTime
                If Len(dataRowUtama(131)) = 0 Then
                    result(2) = "Row : " & i & " - kmodifikasitgl can't be empty" : GoTo selesai
                End If

                'kcustomdbl1(136) As Double
                If Len(dataRowUtama(136)) = 0 Then
                    result(2) = "Row : " & i & " - kcustomdbl1 can't be empty" : GoTo selesai
                End If

                'kcustomdbl2(137) As Double
                If Len(dataRowUtama(137)) = 0 Then
                    result(2) = "Row : " & i & " - kcustomdbl2 can't be empty" : GoTo selesai
                End If

                'kcustomdbl3(138) As Double
                If Len(dataRowUtama(138)) = 0 Then
                    result(2) = "Row : " & i & " - kcustomdbl3 can't be empty" : GoTo selesai
                End If

                'kcustomdate1(139) As Date
                If Len(dataRowUtama(139)) = 0 Then
                    result(2) = "Row : " & i & " - kcustomdate1 can't be empty" : GoTo selesai
                End If

                'kcustomdate2(140) As Date
                If Len(dataRowUtama(140)) = 0 Then
                    result(2) = "Row : " & i & " - kcustomdate2 can't be empty" : GoTo selesai
                End If

                'kcustomdate3(141) As Date
                If Len(dataRowUtama(141)) = 0 Then
                    result(2) = "Row : " & i & " - kcustomdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA ========================================================

                If AsDataTableTambahData(dtutama, "kid~kkode~knama~kkategori~kkategorinama~kcabang~kcabangnama~klokasi~klokasinama~kgudang~kgudangnama~kkategorisalesman~kkategorisalesmannama~karea~kareanama~kkategoricustomer~kkategoricustomernama~kdivisi~kdivisinama~ksubdivisi~ksubdivisinama~ksalesman~ksalesmannama~kkontakperson~kterminglobal~kaktif~kaktiftgl~k1alamat1~k1alamat2~k1alamat3~k1alamat4~k1alamat5~k1kota~k1propinsi~k1kodepos~k1negara~k1kontakperson~k1kontaknohp~k1kontakemail~k1notelp1~k1notelp2~k1nofax~k1email~k1website~k2alamat1~k2alamat2~k2alamat3~k2alamat4~k2alamat5~k2propinsi~k2kota~k2kodepos~k2negara~k2kontakperson~k2kontaknohp~k2kontakemail~k2notelp1~k2notelp2~k2nofax~k2email~k2website~k3alamat1~k3alamat2~k3alamat3~k3alamat4~k3alamat5~k3kota~k3propinsi~k3kodepos~k3negara~k3kontakperson~k3kontaknohp~k3kontakemail~k3notelp1~k3notelp2~k3nofax~k3email~k3website~k4alamat1~k4alamat2~k4alamat3~k4alamat4~k4alamat5~k4kota~k4propinsi~k4kodepos~k4negara~k4kontakperson~k4kontaknohp~k4kontakemail~k4notelp1~k4notelp2~k4nofax~k4email~k4website~knpwp~kpkp~kbatashutang~kterminbeli~krekhutang~kbagpembelian~kfobbeli~kviabeli~kbataspiutang~kterminjual~krekpiutang~kbagpenjualan~ktingkatjual~kfobjual~kviajual~ktglkontrak~kbank~knorekening~kjeniskelamin~kmatauang~ktgllahir~ktglnikah~kkomisipenjualan~kcatatan~kinputuser~kinputtgl~kcustomtext1~kcustomtext2~kcustomtext3~kcustomtext4~kcustomtext5~kcustomtext6~kcustomtext7~kcustomtext8~kcustomtext9~kmodifikasiuser~kmodifikasitgl~kcustomtext10~kcustomint1~kcustomint2~kcustomint3~kcustomdbl1~kcustomdbl2~kcustomdbl3~kcustomdate1~kcustomdate2~kcustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19) & "~" & dataRowUtama(20) & "~" & dataRowUtama(21) & "~" & dataRowUtama(22) & "~" & dataRowUtama(23) & "~" & dataRowUtama(24) & "~" & dataRowUtama(25) & "~" & dataRowUtama(26) & "~" & dataRowUtama(27) & "~" & dataRowUtama(28) & "~" & dataRowUtama(29) & "~" & dataRowUtama(30) & "~" & dataRowUtama(31) & "~" & dataRowUtama(32) & "~" & dataRowUtama(33) & "~" & dataRowUtama(34) & "~" & dataRowUtama(35) & "~" & dataRowUtama(36) & "~" & dataRowUtama(37) & "~" & dataRowUtama(38) & "~" & dataRowUtama(39) & "~" & dataRowUtama(40) & "~" & dataRowUtama(41) & "~" & dataRowUtama(42) & "~" & dataRowUtama(43) & "~" & dataRowUtama(44) & "~" & dataRowUtama(45) & "~" & dataRowUtama(46) & "~" & dataRowUtama(47) & "~" & dataRowUtama(48) & "~" & dataRowUtama(49) & "~" & dataRowUtama(50) & "~" & dataRowUtama(51) & "~" & dataRowUtama(52) & "~" & dataRowUtama(53) & "~" & dataRowUtama(54) & "~" & dataRowUtama(55) & "~" & dataRowUtama(56) & "~" & dataRowUtama(57) & "~" & dataRowUtama(58) & "~" & dataRowUtama(59) & "~" & dataRowUtama(60) & "~" & dataRowUtama(61) & "~" & dataRowUtama(62) & "~" & dataRowUtama(63) & "~" & dataRowUtama(64) & "~" & dataRowUtama(65) & "~" & dataRowUtama(66) & "~" & dataRowUtama(67) & "~" & dataRowUtama(68) & "~" & dataRowUtama(69) & "~" & dataRowUtama(70) & "~" & dataRowUtama(71) & "~" & dataRowUtama(72) & "~" & dataRowUtama(73) & "~" & dataRowUtama(74) & "~" & dataRowUtama(75) & "~" & dataRowUtama(76) & "~" & dataRowUtama(77) & "~" & dataRowUtama(78) & "~" & dataRowUtama(79) & "~" & dataRowUtama(80) & "~" & dataRowUtama(81) & "~" & dataRowUtama(82) & "~" & dataRowUtama(83) & "~" & dataRowUtama(84) & "~" & dataRowUtama(85) & "~" & dataRowUtama(86) & "~" & dataRowUtama(87) & "~" & dataRowUtama(88) & "~" & dataRowUtama(89) & "~" & dataRowUtama(90) & "~" & dataRowUtama(91) & "~" & dataRowUtama(92) & "~" & dataRowUtama(93) & "~" & dataRowUtama(94) & "~" & dataRowUtama(95) & "~" & dataRowUtama(96) & "~" & dataRowUtama(97) & "~" & dataRowUtama(98) & "~" & dataRowUtama(99) & "~" & dataRowUtama(100) & "~" & dataRowUtama(101) & "~" & dataRowUtama(102) & "~" & dataRowUtama(103) & "~" & dataRowUtama(104) & "~" & dataRowUtama(105) & "~" & dataRowUtama(106) & "~" & dataRowUtama(107) & "~" & dataRowUtama(108) & "~" & dataRowUtama(109) & "~" & dataRowUtama(110) & "~" & dataRowUtama(111) & "~" & dataRowUtama(112) & "~" & dataRowUtama(113) & "~" & dataRowUtama(114) & "~" & dataRowUtama(115) & "~" & dataRowUtama(116) & "~" & dataRowUtama(117) & "~" & dataRowUtama(118) & "~" & dataRowUtama(119) & "~" & dataRowUtama(120) & "~" & dataRowUtama(121) & "~" & dataRowUtama(122) & "~" & dataRowUtama(123) & "~" & dataRowUtama(124) & "~" & dataRowUtama(125) & "~" & dataRowUtama(126) & "~" & dataRowUtama(127) & "~" & dataRowUtama(128) & "~" & dataRowUtama(129) & "~" & dataRowUtama(130) & "~" & dataRowUtama(131) & "~" & dataRowUtama(132) & "~" & dataRowUtama(133) & "~" & dataRowUtama(134) & "~" & dataRowUtama(135) & "~" & dataRowUtama(136) & "~" & dataRowUtama(137) & "~" & dataRowUtama(138) & "~" & dataRowUtama(139) & "~" & dataRowUtama(140) & "~" & dataRowUtama(141)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If
            Next
        End If

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0
        'Proses detail
        If (dtutama.Rows.Count > 0) Then
            Dim strValue2 As New StringBuilder
            For Each dr1 As DataRow In dtutama.Rows
                If dr1(1) <> "CASH" Then
                    'CEK KODE DAN KATEGORI KONTAK
                    Dim dtCek As New DataTable
                    sql = "SELECT kid FROM m1_contact WHERE kkode = '" & FixQuotes(dr1(1)) & "' AND kkategori = '" & FixQuotes(dr1(3)) & "'"
                    dtCek = AsDataTableAmbilDariDB(sql)
                    If dtCek.Rows.Count > 0 Then
                        result(2) = "Kode : '" & FixQuotes(dr1(1)) & "' sudah digunakan." : Trans.Rollback() : GoTo selesai
                    Else                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1(1)) & "', '" & FixQuotes(dr1(2)) & "', '" & FixQuotes(dr1(3)) & "', '" & FixQuotes(dr1(4)) & "', '" & FixQuotes(dr1(5)) & "', '" & FixQuotes(dr1(6)) & "', '" & FixQuotes(dr1(7)) & "', '" & FixQuotes(dr1(8)) & "', '" & FixQuotes(dr1(9)) & "', '" & FixQuotes(dr1(10)) & "', '" & FixQuotes(dr1(11)) & "', '" & FixQuotes(dr1(12)) & "', '" & FixQuotes(dr1(13)) & "', '" & FixQuotes(dr1(14)) & "', '" & FixQuotes(dr1(15)) & "', '" & FixQuotes(dr1(16)) & "', '" & FixQuotes(dr1(17)) & "', '" & FixQuotes(dr1(18)) & "', '" & FixQuotes(dr1(19)) & "', '" & FixQuotes(dr1(20)) & "', " & dr1(21) & ", '" & FixQuotes(dr1(22)) & "', '" & FixQuotes(dr1(23)) & "', " & dr1(24) & ", " & dr1(25) & ", '" & FixQuotes(AsFormatTanggal(dr1(26))) & "', '" & FixQuotes(dr1(27)) & "', '" & FixQuotes(dr1(28)) & "', '" & FixQuotes(dr1(29)) & "', '" & FixQuotes(dr1(30)) & "', '" & FixQuotes(dr1(31)) & "', '" & FixQuotes(dr1(32)) & "', '" & FixQuotes(dr1(33)) & "', '" & FixQuotes(dr1(34)) & "', '" & FixQuotes(dr1(35)) & "', '" & FixQuotes(dr1(36)) & "', '" & FixQuotes(dr1(37)) & "', '" & FixQuotes(dr1(38)) & "', '" & FixQuotes(dr1(39)) & "', '" & FixQuotes(dr1(40)) & "', '" & FixQuotes(dr1(41)) & "', '" & FixQuotes(dr1(42)) & "', '" & FixQuotes(dr1(43)) & "', '" & FixQuotes(dr1(44)) & "', '" & FixQuotes(dr1(45)) & "', '" & FixQuotes(dr1(46)) & "', '" & FixQuotes(dr1(47)) & "', '" & FixQuotes(dr1(48)) & "', '" & FixQuotes(dr1(49)) & "', '" & FixQuotes(dr1(50)) & "', '" & FixQuotes(dr1(51)) & "', '" & FixQuotes(dr1(52)) & "', '" & FixQuotes(dr1(53)) & "', '" & FixQuotes(dr1(54)) & "', '" & FixQuotes(dr1(55)) & "', '" & FixQuotes(dr1(56)) & "', '" & FixQuotes(dr1(57)) & "', '" & FixQuotes(dr1(58)) & "', '" & FixQuotes(dr1(59)) & "', '" & FixQuotes(dr1(60)) & "', '" & FixQuotes(dr1(61)) & "', '" & FixQuotes(dr1(62)) & "', '" & FixQuotes(dr1(63)) & "', '" & FixQuotes(dr1(64)) & "', '" & FixQuotes(dr1(65)) & "', '" & FixQuotes(dr1(66)) & "', '" & FixQuotes(dr1(67)) & "', '" & FixQuotes(dr1(68)) & "', '" & FixQuotes(dr1(69)) & "', '" & FixQuotes(dr1(70)) & "', '" & FixQuotes(dr1(71)) & "', '" & FixQuotes(dr1(72)) & "', '" & FixQuotes(dr1(73)) & "', '" & FixQuotes(dr1(74)) & "', '" & FixQuotes(dr1(75)) & "', '" & FixQuotes(dr1(76)) & "', '" & FixQuotes(dr1(77)) & "', '" & FixQuotes(dr1(78)) & "', '" & FixQuotes(dr1(79)) & "', '" & FixQuotes(dr1(80)) & "', '" & FixQuotes(dr1(81)) & "', '" & FixQuotes(dr1(82)) & "', '" & FixQuotes(dr1(83)) & "', '" & FixQuotes(dr1(84)) & "', '" & FixQuotes(dr1(85)) & "', '" & FixQuotes(dr1(86)) & "', '" & FixQuotes(dr1(87)) & "', '" & FixQuotes(dr1(88)) & "', '" & FixQuotes(dr1(89)) & "', '" & FixQuotes(dr1(90)) & "', '" & FixQuotes(dr1(91)) & "', '" & FixQuotes(dr1(92)) & "', '" & FixQuotes(dr1(93)) & "', '" & FixQuotes(dr1(94)) & "', '" & FixQuotes(dr1(95)) & "', " & dr1(96) & ", '" & FixDouble(dr1(97)) & "', '" & FixQuotes(dr1(98)) & "', '" & FixQuotes(dr1(99)) & "', " & dr1(100) & ", '" & FixQuotes(dr1(101)) & "', '" & FixQuotes(dr1(102)) & "', '" & FixDouble(dr1(103)) & "', '" & FixQuotes(dr1(104)) & "', '" & FixQuotes(dr1(105)) & "', " & dr1(106) & ", " & dr1(107) & ", '" & FixQuotes(dr1(108)) & "', '" & FixQuotes(dr1(109)) & "', '" & FixQuotes(AsFormatTanggal(dr1(110))) & "', '" & FixQuotes(dr1(111)) & "', '" & FixQuotes(dr1(112)) & "', " & dr1(113) & ", '" & FixQuotes(dr1(114)) & "', '" & FixQuotes(AsFormatTanggal(dr1(115))) & "', '" & FixQuotes(AsFormatTanggal(dr1(116))) & "', '" & FixDouble(dr1(117)) & "', '" & FixQuotes(dr1(118)) & "', " & dr1(119) & ", '" & FixQuotes(AsFormatTanggal(dr1(120), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1(121)) & "', '" & FixQuotes(dr1(122)) & "', '" & FixQuotes(dr1(123)) & "', '" & FixQuotes(dr1(124)) & "', '" & FixQuotes(dr1(125)) & "', '" & FixQuotes(dr1(126)) & "', '" & FixQuotes(dr1(127)) & "', '" & FixQuotes(dr1(128)) & "', '" & FixQuotes(dr1(129)) & "', " & dr1(130) & ", '" & FixQuotes(AsFormatTanggal(dr1(131), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1(132)) & "', " & dr1(133) & ", " & dr1(134) & ", " & dr1(135) & ", '" & FixDouble(dr1(136)) & "', '" & FixDouble(dr1(137)) & "', '" & FixDouble(dr1(138)) & "', '" & FixQuotes(AsFormatTanggal(dr1(139))) & "', '" & FixQuotes(AsFormatTanggal(dr1(140))) & "', '" & FixQuotes(AsFormatTanggal(dr1(141))) & "', 1)")

                    End If
                End If
            Next
            If strValue2.ToString.Length > 0 Then
                'sql = "Insert into M3_Mr_Detail(idmrdetail, idmr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargabeli, hargajual, stokterakhir, cabang, lokasi, gudangasal, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlts, statusts, jmlrs, statusrs, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                sql = "Insert into M1_Contact (kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE kkode = values(kkode), knama = values(knama), kkategori = values(kkategori), kkategorinama = values(kkategorinama), kcabang = values(kcabang), kcabangnama = values(kcabangnama), klokasi = values(klokasi), klokasinama = values(klokasinama), kgudang = values(kgudang), kgudangnama = values(kgudangnama), kkategorisalesman = values(kkategorisalesman), kkategorisalesmannama = values(kkategorisalesmannama), karea = values(karea), kareanama = values(kareanama), kkategoricustomer = values(kkategoricustomer), kkategoricustomernama = values(kkategoricustomernama), kdivisi = values(kdivisi), kdivisinama = values(kdivisinama), ksubdivisi = values(ksubdivisi), ksubdivisinama = values(ksubdivisinama), ksalesman = values(ksalesman), ksalesmannama = values(ksalesmannama), kkontakperson = values(kkontakperson), kterminglobal = values(kterminglobal), kaktif = values(kaktif), kaktiftgl = values(kaktiftgl), k1alamat1 = values(k1alamat1), k1alamat2 = values(k1alamat2), k1alamat3 = values(k1alamat3), k1alamat4 = values(k1alamat4), k1alamat5 = values(k1alamat5), k1kota = values(k1kota), k1propinsi = values(k1propinsi), k1kodepos = values(k1kodepos), k1negara = values(k1negara), k1kontakperson = values(k1kontakperson), k1kontaknohp = values(k1kontaknohp), k1kontakemail = values(k1kontakemail), k1notelp1 = values(k1notelp1), k1notelp2 = values(k1notelp2), k1nofax = values(k1nofax), k1email = values(k1email), k1website = values(k1website), k2alamat1 = values(k2alamat1), k2alamat2 = values(k2alamat2), k2alamat3 = values(k2alamat3), k2alamat4 = values(k2alamat4), k2alamat5 = values(k2alamat5), k2propinsi = values(k2propinsi), k2kota = values(k2kota), k2kodepos = values(k2kodepos), k2negara = values(k2negara), k2kontakperson = values(k2kontakperson), k2kontaknohp = values(k2kontaknohp), k2kontakemail = values(k2kontakemail), k2notelp1 = values(k2notelp1), k2notelp2 = values(k2notelp2), k2nofax = values(k2nofax), k2email = values(k2email), k2website = values(k2website), k3alamat1 = values(k3alamat1), k3alamat2 = values(k3alamat2), k3alamat3 = values(k3alamat3), k3alamat4 = values(k3alamat4), k3alamat5 = values(k3alamat5), k3kota = values(k3kota), k3propinsi = values(k3propinsi), k3kodepos = values(k3kodepos), k3negara = values(k3negara), k3kontakperson = values(k3kontakperson), k3kontaknohp = values(k3kontaknohp), k3kontakemail = values(k3kontakemail), k3notelp1 = values(k3notelp1), k3notelp2 = values(k3notelp2), k3nofax = values(k3nofax), k3email = values(k3email), k3website = values(k3website), k4alamat1 = values(k4alamat1), k4alamat2 = values(k4alamat2), k4alamat3 = values(k4alamat3), k4alamat4 = values(k4alamat4), k4alamat5 = values(k4alamat5), k4kota = values(k4kota), k4propinsi = values(k4propinsi), k4kodepos = values(k4kodepos), k4negara = values(k4negara), k4kontakperson = values(k4kontakperson), k4kontaknohp = values(k4kontaknohp), k4kontakemail = values(k4kontakemail), k4notelp1 = values(k4notelp1), k4notelp2 = values(k4notelp2), k4nofax = values(k4nofax), k4email = values(k4email), k4website = values(k4website), knpwp = values(knpwp), kpkp = values(kpkp), kbatashutang = values(kbatashutang), kterminbeli = values(kterminbeli), krekhutang = values(krekhutang), kbagpembelian = values(kbagpembelian), kfobbeli = values(kfobbeli), kviabeli = values(kviabeli), kbataspiutang = values(kbataspiutang), kterminjual = values(kterminjual), krekpiutang = values(krekpiutang), kbagpenjualan = values(kbagpenjualan), ktingkatjual = values(ktingkatjual), kfobjual = values(kfobjual), kviajual = values(kviajual), ktglkontrak = values(ktglkontrak), kbank = values(kbank), knorekening = values(knorekening), kjeniskelamin = values(kjeniskelamin), kmatauang = values(kmatauang), ktgllahir = values(ktgllahir), ktglnikah = values(ktglnikah), kkomisipenjualan = values(kkomisipenjualan), kcatatan = values(kcatatan), kinputuser = values(kinputuser), kinputtgl = values(kinputtgl), kcustomtext1 = values(kcustomtext1), kcustomtext2 = values(kcustomtext2), kcustomtext3 = values(kcustomtext3), kcustomtext4 = values(kcustomtext4), kcustomtext5 = values(kcustomtext5), kcustomtext6 = values(kcustomtext6), kcustomtext7 = values(kcustomtext7), kcustomtext8 = values(kcustomtext8), kcustomtext9 = values(kcustomtext9), kmodifikasiuser = values(kmodifikasiuser), kmodifikasitgl = values(kmodifikasitgl), kcustomtext10 = values(kcustomtext10), kcustomint1 = values(kcustomint1), kcustomint2 = values(kcustomint2), kcustomint3 = values(kcustomint3), kcustomdbl1 = values(kcustomdbl1), kcustomdbl2 = values(kcustomdbl2), kcustomdbl3 = values(kcustomdbl3), kcustomdate1 = values(kcustomdate1), kcustomdate2 = values(kcustomdate2), kcustomdate3 = values(kcustomdate3), ksinkron = values(ksinkron)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


        Else
            'result(2) = "Main Transaction data not found." : Trans.Rollback() : GoTo selesai
        End If

        Trans.Commit()  '*** Commit Transaction ***'
        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)


        'AMBIL DATA =============================================================
        Dim paramSearch As String = MobM1_ContactSearch(PostWsSearch(paramSplit(0), "MobM1_ContactSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
        'result(2) = paramSplit(0) & "  " & "MobM1_ContactSearch" & "  " & pagingSplit(0) & "  " & pagingSplit(1) & "  " & Filter & "  " & Sorting & "  " & formatTgl & "  " & formatTglWaktu : GoTo selesai
        Dim hasilSearch As New RsHasilWsSearch
        hasilSearch = GetWsSearch(paramSearch)

        'result(1) = hasilSearch.success
        'result(2) = hasilSearch.errmessage

        resultPaging(0) = hasilSearch.isPaging
        resultPaging(1) = hasilSearch.isNext
        resultPaging(2) = hasilSearch.isPrevious
        resultPaging(3) = hasilSearch.countPage
        resultPaging(4) = hasilSearch.countRow

        search = hasilSearch.data
        'END OF AMBIL DATA ======================================================


        objCmd = Nothing
        'Con1.Close()
        'Con1 = Nothing
        'END OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function
End Class