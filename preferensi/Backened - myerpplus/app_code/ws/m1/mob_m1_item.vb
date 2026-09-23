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
Public Class mob_m1_item
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function MobM1_ItemSearch(ByVal param As String) As String
        'MobM1_ItemSearch --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, 
        'bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, 
        'bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, 
        'bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, 
        'bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, 
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile

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
        Dim Filter As String = "", Sorting As String = "", deviceUUID As String = ""
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

        'Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
        '    result(2) = "Access denied for get data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        Else
            userid = paramSplit(3)
        End If

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

        'SET DAN VALIDASI VARIABEL USER ====================================================
        'Device UUID
        If (Len(paramSplit(4)) = 0) Then
            result(2) = "Device UUID can't be empty" : GoTo selesai
        Else
            deviceUUID = paramSplit(4).ToString
        End If
        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m1_item_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'Query (cek userid apa sudah ada di tabel)
        sql = "SELECT * FROM m0_nomor_mobile WHERE macaddress = '" + deviceUUID + "' AND userid = " + userid
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count = 0 Then
            result(2) = "User has logged on other device." : GoTo selesai
        End If

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("bnamaalias1"), ""), sptField,
                     FxDB(dr("bnamaalias2"), ""), sptField,
                     FxDB(dr("bnamaalias3"), ""), sptField,
                     FxDB(dr("bnamaalias4"), ""), sptField,
                     FxDB(dr("bnamaalias5"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bjenisdetail"), 0), sptField,
                     FxDB(dr("bkategori"), ""), sptField,
                     FxDB(dr("bketerangan"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("bnilaisatuan"), 0), sptField,
                     FxDB(dr("bsatuandefault"), ""), sptField,
                     FxDB(dr("bnilaisatuandefault"), 0), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bcabang"), ""), sptField,
                     FxDB(dr("blokasi"), ""), sptField,
                     FxDB(dr("bdivisi"), ""), sptField,
                     FxDB(dr("bsubdivisi"), ""), sptField,
                     FxDB(dr("bgudang"), ""), sptField,
                     FxDB(dr("bproyek"), ""), sptField,
                     FxDB(dr("bsubitem"), 0), sptField,
                     FxDB(dr("bsubitemdari"), ""), sptField,
                     FxDB(dr("bbarcode"), ""), sptField,
                     FxDB(dr("bsuplier"), ""), sptField,
                     FxDB(dr("baktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("baktiftgl"), ""), formatTgl), sptField,
                     FxDB(dr("bstokminimal"), 0), sptField,
                     FxDB(dr("bstokmaksimal"), 0), sptField,
                     FxDB(dr("breorder"), 0), sptField,
                     FxDB(dr("bjmlorderbeli"), 0), sptField,
                     FxDB(dr("bjmlorderjual"), 0), sptField,
                     FxDB(dr("bkategoriumur"), ""), sptField,
                     FxDB(dr("bstatusmoving"), ""), sptField,
                     FxDB(dr("bsifatharga"), ""), sptField,
                     FxDB(dr("bpromo"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bpromoberlaku"), ""), formatTgl), sptField,
                     FxDB(dr("bpajakbeli"), ""), sptField,
                     FxDB(dr("bpajakjual"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bhargajual2"), 0), sptField,
                     FxDB(dr("bhargajual3"), 0), sptField,
                     FxDB(dr("bhargajual4"), 0), sptField,
                     FxDB(dr("bhargajual5"), 0), sptField,
                     FxDB(dr("bdiskonjual1"), ""), sptField,
                     FxDB(dr("bdiskonjual2"), ""), sptField,
                     FxDB(dr("bdiskonjual3"), ""), sptField,
                     FxDB(dr("bdiskonjual4"), ""), sptField,
                     FxDB(dr("bdiskonjual5"), ""), sptField,
                     FxDB(dr("bstok"), 0), sptField,
                     FxDB(dr("bkomisi"), 0), sptField,
                     FxDB(dr("bmarginminimal"), 0), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("brekreturpenjualan"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("brekhargapokok"), ""), sptField,
                     FxDB(dr("brekreturpembelian"), ""), sptField,
                     FxDB(dr("brekdiskonpembelian"), ""), sptField,
                     FxDB(dr("brekkonsinyasi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bavolume"), 0), sptField,
                     FxDB(dr("baberat"), 0), sptField,
                     FxDB(dr("bawarna"), ""), sptField,
                     FxDB(dr("baoem"), ""), sptField,
                     FxDB(dr("bamerk"), ""), sptField,
                     FxDB(dr("baukuran"), ""), sptField,
                     FxDB(dr("bamodel"), ""), sptField,
                     FxDB(dr("bakelas"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("bpengganti"), ""), sptField,
                     FxDB(dr("bgambar"), ""), sptField,
                     FxDB(dr("burutan"), ""), sptField,
                     FxDB(dr("bcustom1"), ""), sptField,
                     FxDB(dr("bcustom2"), ""), sptField,
                     FxDB(dr("bcustom3"), ""), sptField,
                     FxDB(dr("bcustom4"), ""), sptField,
                     FxDB(dr("bcustom5"), ""), sptField,
                     FxDB(dr("bcustom6"), ""), sptField,
                     FxDB(dr("bcustom7"), ""), sptField,
                     FxDB(dr("bcustom8"), ""), sptField,
                     FxDB(dr("bcustom9"), ""), sptField,
                     FxDB(dr("bcustom10"), ""), sptField,
                     FxDB(dr("bcustom11"), 0), sptField,
                     FxDB(dr("bcustom12"), 0), sptField,
                     FxDB(dr("bcustom13"), 0), sptField,
                     FxDB(dr("bcustom14"), 0), sptField,
                     FxDB(dr("bcustom15"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("binputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bedithpp"), 0), sptField,
                     FxDB(dr("bmobile"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile"))

        Return wsResult
    End Function

End Class