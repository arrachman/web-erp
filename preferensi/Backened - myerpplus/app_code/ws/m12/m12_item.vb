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
Public Class m12_item
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M12_Item_InformationSearch(ByVal param As String) As String
        'M12_Item_InformationSearch --------------------------------------------------------
        'bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan
        'bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2,
        'bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4,
        'bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekreturpenjualan, brekdiskonpenjualan,
        'brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan,
        'bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bstokminimal, bstokmaksimal, bstatusmoving,
        'binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, fnamafile, lkategoripos, pcnama, pistokreorder, icnama, baktif, baktiftgl

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2) & " AND (CASE LENGTH(IFNULL(uw.gudang,'')) WHEN 0 THEN isw.kgudang LIKE '%' OR isw.kgudang IS NULL ELSE isw.kgudang = uw.gudang END)"
            '#Taruh fungsi replace disini...
        Else
            Filter = " (CASE LENGTH(IFNULL(uw.gudang,'')) WHEN 0 THEN isw.kgudang LIKE '%' OR isw.kgudang IS NULL ELSE isw.kgudang = uw.gudang END)"
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
        'sql = "select `i`.`bid` AS `bid`, `i`.`bkode` AS `bkode`, `i`.`bnama` AS `bnama`, `i`.`btipe` AS `btipe`, `i`.`bjenis` AS `bjenis`, `i`.`bkategori` AS `bkategori`, `i`.`bsatuan` AS `bsatuan`, `i`.`bsatuandefault` AS `bsatuandefault`, `i`.`bhpp` AS `bhpp`, `i`.`bbarcode` AS `bbarcode`, `i`.`bhargabeli` AS `bhargabeli`, `i`.`bhppaverage` AS `bhppaverage`, `pi`.`pihargajual1` AS `bhargajual1`, `pi`.`pihargajual2` AS `bhargajual2`, `pi`.`pihargajual3` AS `bhargajual3`, `pi`.`pihargajual4` AS `bhargajual4`, `pi`.`pihargajual5` AS `bhargajual5`, `pi`.`pidiskonjual1` AS `bdiskonjual1`, `pi`.`pidiskonjual2` AS `bdiskonjual2`, `pi`.`pidiskonjual3` AS `bdiskonjual3`, `pi`.`pidiskonjual4` AS `bdiskonjual4`, `pi`.`pidiskonjual5` AS `bdiskonjual5`, `i`.`bstok` AS `bstok`, ifnull(sum(`ib`.`jmlbooking`), 0) AS `bstokbooking`, `i`.`bmarginminimal` AS `bmarginminimal`, `i`.`brekpersediaan` AS `brekpersediaan`, `i`.`brekpenjualan` AS `brekpenjualan`, `i`.`brekreturpenjualan` AS `brekreturpenjualan`, `i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`, `i`.`brekhargapokok` AS `brekhargapokok`, `i`.`brekreturpembelian` AS `brekreturpembelian`, `i`.`brekdiskonpembelian` AS `brekdiskonpembelian`, `i`.`brekkonsinyasi` AS `brekkonsinyasi`, `i`.`bserial` AS `bserial`, `i`.`bbatch` AS `bbatch`, `i`.`bnilaisatuan` AS `bnilaisatuan`, `i`.`bnilaisatuandefault` AS `bnilaisatuandefault`, `i`.`bsuplier` AS `bsuplier`, `c`.`kkode` AS `bsuplierkode`, `c`.`knama` AS `bsupliernama`, `pi`.`pistokminimal` AS `bstokminimal`, `pi`.`pistokmaksimal` AS `bstokmaksimal`, `i`.`bstatusmoving` AS `bstatusmoving`, `i`.`binputuser` AS `binputuser`, `i`.`binputtgl` AS `binputtgl`, `i`.`bmodifikasiuser` AS `bmodifikasiuser`, `i`.`bmodifikasitgl` AS `bmodifikasitgl`, `f`.`fnamafile` AS `fnamafile`, l.lkategoripos as lkategoripos, pc.pcnama, pi.pistokreorder, ic.icnama from `m1_item` `i` join `m0_user` `u` on `u`.`userid` = 'valuserid' join `m1_location` `l` on `u`.`ulokasi` = `l`.`lkode` join `m_12_pos_item` `pi` on `l`.`lkategoripos` = `pi`.`pikategori` and `i`.`bid` = `pi`.`piidbarang` join m_12_pos_category pc on l.lkategoripos = pc.pckode left join m1_item_category ic on i.bkategori = ic.ickode left join `m1_item_booking` `ib` on `i`.`bid` = `ib`.`idbarang` left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_files` `f` on `i`.`bid` = `f`.`fidtransaksi` and `f`.`fdefault` = 1 and `f`.`fsumber` = 'Item'"
        sql = "select `i`.`bid` AS `bid`, `i`.`bkode` AS `bkode`, `i`.`bnama` AS `bnama`, `i`.`btipe` AS `btipe`, `i`.`bjenis` AS `bjenis`, `i`.`bkategori` AS `bkategori`, `i`.`bsatuan` AS `bsatuan`, `i`.`bsatuandefault` AS `bsatuandefault`, `i`.`bhpp` AS `bhpp`, `i`.`bbarcode` AS `bbarcode`, `i`.`bhargabeli` AS `bhargabeli`, `i`.`bhppaverage` AS `bhppaverage`, `pi`.`pihargajual1` AS `bhargajual1`, `pi`.`pihargajual2` AS `bhargajual2`, `pi`.`pihargajual3` AS `bhargajual3`, `pi`.`pihargajual4` AS `bhargajual4`, `pi`.`pihargajual5` AS `bhargajual5`, `pi`.`pidiskonjual1` AS `bdiskonjual1`, `pi`.`pidiskonjual2` AS `bdiskonjual2`, `pi`.`pidiskonjual3` AS `bdiskonjual3`, `pi`.`pidiskonjual4` AS `bdiskonjual4`, `pi`.`pidiskonjual5` AS `bdiskonjual5`, (CASE i.bjenis WHEN 'J' THEN 0 ELSE IFNULL(SUM(isw.stok),0) END) AS `bstok`, (CASE i.bjenis WHEN 'J' THEN 0 ELSE ifnull(sum(`ib`.`jmlbooking`), 0) END) AS `bstokbooking`, `i`.`bmarginminimal` AS `bmarginminimal`, `i`.`brekpersediaan` AS `brekpersediaan`, `i`.`brekpenjualan` AS `brekpenjualan`, `i`.`brekreturpenjualan` AS `brekreturpenjualan`, `i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`, `i`.`brekhargapokok` AS `brekhargapokok`, `i`.`brekreturpembelian` AS `brekreturpembelian`, `i`.`brekdiskonpembelian` AS `brekdiskonpembelian`, `i`.`brekkonsinyasi` AS `brekkonsinyasi`, `i`.`bserial` AS `bserial`, `i`.`bbatch` AS `bbatch`, `i`.`bnilaisatuan` AS `bnilaisatuan`, `i`.`bnilaisatuandefault` AS `bnilaisatuandefault`, `i`.`bsuplier` AS `bsuplier`, `c`.`kkode` AS `bsuplierkode`, `c`.`knama` AS `bsupliernama`, `pi`.`pistokminimal` AS `bstokminimal`, `pi`.`pistokmaksimal` AS `bstokmaksimal`, `i`.`bstatusmoving` AS `bstatusmoving`, `i`.`binputuser` AS `binputuser`, `i`.`binputtgl` AS `binputtgl`, `i`.`bmodifikasiuser` AS `bmodifikasiuser`, `i`.`bmodifikasitgl` AS `bmodifikasitgl`, `f`.`fnamafile` AS `fnamafile`, l.lkategoripos as lkategoripos, pc.pcnama, pi.pistokreorder, ic.icnama, i.baktif, i.baktiftgl from `m1_item` `i` join `m0_user` `u` on `u`.`userid` = 'valuserid' join `m1_location` `l` on `u`.`ulokasi` = `l`.`lkode` join `m_12_pos_item` `pi` on `l`.`lkategoripos` = `pi`.`pikategori` and `i`.`bid` = `pi`.`piidbarang` join m_12_pos_category pc on l.lkategoripos = pc.pckode left join m0_user_warehouse uw on u.userid = uw.userid left join m1_item_stock_warehouse isw on i.bid = isw.idbarang AND (CASE LENGTH(IFNULL(uw.gudang,'')) WHEN 0 THEN isw.kgudang LIKE '%' OR isw.kgudang IS NULL ELSE isw.kgudang = uw.gudang END) left join m1_item_category ic on i.bkategori = ic.ickode left join `m1_item_booking` `ib` on `i`.`bid` = `ib`.`idbarang` and uw.gudang = ib.gudang left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_files` `f` on `i`.`bid` = `f`.`fidtransaksi` and `f`.`fdefault` = 1 and `f`.`fsumber` = 'Item'"
        sql = sql.Replace("valuserid", userid)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "i.bid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("bid"), 0), sptField,
                    FxDB(dr("bkode"), ""), sptField,
                    FxDB(dr("bnama"), ""), sptField,
                    FxDB(dr("btipe"), ""), sptField,
                    FxDB(dr("bjenis"), ""), sptField,
                    FxDB(dr("bkategori"), ""), sptField,
                    FxDB(dr("bsatuan"), ""), sptField,
                    FxDB(dr("bsatuandefault"), ""), sptField,
                    FxDB(dr("bhpp"), ""), sptField,
                    FxDB(dr("bbarcode"), ""), sptField,
                    FxDB(dr("bhargabeli"), 0), sptField,
                    FxDB(dr("bhppaverage"), 0), sptField,
                    FxDB(dr("bhargajual1"), 0), sptField,
                    FxDB(dr("bhargajual2"), 0), sptField,
                    FxDB(dr("bhargajual3"), 0), sptField,
                    FxDB(dr("bhargajual4"), 0), sptField,
                    FxDB(dr("bhargajual5"), 0), sptField,
                    FxDB(dr("bdiskonjual1"), 0), sptField,
                    FxDB(dr("bdiskonjual2"), 0), sptField,
                    FxDB(dr("bdiskonjual3"), 0), sptField,
                    FxDB(dr("bdiskonjual4"), 0), sptField,
                    FxDB(dr("bdiskonjual5"), 0), sptField,
                    FxDB(dr("bstok"), 0), sptField,
                    FxDB(dr("bstokbooking"), 0), sptField,
                    FxDB(dr("bmarginminimal"), 0), sptField,
                    FxDB(dr("brekpersediaan"), ""), sptField,
                    FxDB(dr("brekreturpenjualan"), ""), sptField,
                    FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                    FxDB(dr("brekhargapokok"), ""), sptField,
                    FxDB(dr("brekreturpembelian"), ""), sptField,
                    FxDB(dr("brekdiskonpembelian"), ""), sptField,
                    FxDB(dr("brekkonsinyasi"), ""), sptField,
                    FxDB(dr("bserial"), ""), sptField,
                    FxDB(dr("bbatch"), ""), sptField,
                    FxDB(dr("bnilaisatuan"), 0), sptField,
                    FxDB(dr("bnilaisatuandefault"), 0), sptField,
                    FxDB(dr("bsuplier"), 0), sptField,
                    FxDB(dr("bsuplierkode"), ""), sptField,
                    FxDB(dr("bsupliernama"), ""), sptField,
                    FxDB(dr("bstokminimal"), 0), sptField,
                    FxDB(dr("bstokmaksimal"), 0), sptField,
                    FxDB(dr("bstatusmoving"), ""), sptField,
                    FxDB(dr("binputuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("bmodifikasiuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("fnamafile"), ""), sptField,
                    FxDB(dr("lkategoripos"), ""), sptField,
                    FxDB(dr("pcnama"), ""), sptField,
                    FxDB(dr("pistokreorder"), 0), sptField,
                    FxDB(dr("icnama"), ""), sptField,
                    FxDB(dr("baktif"), 0), sptField,
                    AsFormatTanggal(FxDB(dr("baktiftgl"), ""), formatTgl), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bstokminimal, bstokmaksimal, bstatusmoving, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, fnamafile, lkategoripos, pcnama, pistokreorder, icnama, baktif, baktiftgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Item_InformationSearchOld(ByVal param As String) As String
        'M12_Item_InformationSearch --------------------------------------------------------
        'bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan
        'bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2,
        'bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4,
        'bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekreturpenjualan, brekdiskonpenjualan,
        'brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan,
        'bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bstokminimal, bstokmaksimal, bstatusmoving,
        'binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, fnamafile, lkategoripos, pcnama, pistokreorder, icnama

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
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

        'PANGGIL QUERY
        'sql = "select `i`.`bid` AS `bid`, `i`.`bkode` AS `bkode`, `i`.`bnama` AS `bnama`, `i`.`btipe` AS `btipe`, `i`.`bjenis` AS `bjenis`, `i`.`bkategori` AS `bkategori`, `i`.`bsatuan` AS `bsatuan`, `i`.`bsatuandefault` AS `bsatuandefault`, `i`.`bhpp` AS `bhpp`, `i`.`bbarcode` AS `bbarcode`, `i`.`bhargabeli` AS `bhargabeli`, `i`.`bhppaverage` AS `bhppaverage`, `pi`.`pihargajual1` AS `bhargajual1`, `pi`.`pihargajual2` AS `bhargajual2`, `pi`.`pihargajual3` AS `bhargajual3`, `pi`.`pihargajual4` AS `bhargajual4`, `pi`.`pihargajual5` AS `bhargajual5`, `pi`.`pidiskonjual1` AS `bdiskonjual1`, `pi`.`pidiskonjual2` AS `bdiskonjual2`, `pi`.`pidiskonjual3` AS `bdiskonjual3`, `pi`.`pidiskonjual4` AS `bdiskonjual4`, `pi`.`pidiskonjual5` AS `bdiskonjual5`, `i`.`bstok` AS `bstok`, ifnull(sum(`ib`.`jmlbooking`), 0) AS `bstokbooking`, `i`.`bmarginminimal` AS `bmarginminimal`, `i`.`brekpersediaan` AS `brekpersediaan`, `i`.`brekpenjualan` AS `brekpenjualan`, `i`.`brekreturpenjualan` AS `brekreturpenjualan`, `i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`, `i`.`brekhargapokok` AS `brekhargapokok`, `i`.`brekreturpembelian` AS `brekreturpembelian`, `i`.`brekdiskonpembelian` AS `brekdiskonpembelian`, `i`.`brekkonsinyasi` AS `brekkonsinyasi`, `i`.`bserial` AS `bserial`, `i`.`bbatch` AS `bbatch`, `i`.`bnilaisatuan` AS `bnilaisatuan`, `i`.`bnilaisatuandefault` AS `bnilaisatuandefault`, `i`.`bsuplier` AS `bsuplier`, `c`.`kkode` AS `bsuplierkode`, `c`.`knama` AS `bsupliernama`, `pi`.`pistokminimal` AS `bstokminimal`, `pi`.`pistokmaksimal` AS `bstokmaksimal`, `i`.`bstatusmoving` AS `bstatusmoving`, `i`.`binputuser` AS `binputuser`, `i`.`binputtgl` AS `binputtgl`, `i`.`bmodifikasiuser` AS `bmodifikasiuser`, `i`.`bmodifikasitgl` AS `bmodifikasitgl`, `f`.`fnamafile` AS `fnamafile`, l.lkategoripos as lkategoripos, pc.pcnama, pi.pistokreorder, ic.icnama from `m1_item` `i` join `m0_user` `u` on `u`.`userid` = 'valuserid' join `m1_location` `l` on `u`.`ulokasi` = `l`.`lkode` join `m_12_pos_item` `pi` on `l`.`lkategoripos` = `pi`.`pikategori` and `i`.`bid` = `pi`.`piidbarang` join m_12_pos_category pc on l.lkategoripos = pc.pckode left join m1_item_category ic on i.bkategori = ic.ickode left join `m1_item_booking` `ib` on `i`.`bid` = `ib`.`idbarang` left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_files` `f` on `i`.`bid` = `f`.`fidtransaksi` and `f`.`fdefault` = 1 and `f`.`fsumber` = 'Item'"
        sql = "select `i`.`bid` AS `bid`, `i`.`bkode` AS `bkode`, `i`.`bnama` AS `bnama`, `i`.`btipe` AS `btipe`, `i`.`bjenis` AS `bjenis`, `i`.`bkategori` AS `bkategori`, `i`.`bsatuan` AS `bsatuan`, `i`.`bsatuandefault` AS `bsatuandefault`, `i`.`bhpp` AS `bhpp`, `i`.`bbarcode` AS `bbarcode`, `i`.`bhargabeli` AS `bhargabeli`, `i`.`bhppaverage` AS `bhppaverage`, `pi`.`pihargajual1` AS `bhargajual1`, `pi`.`pihargajual2` AS `bhargajual2`, `pi`.`pihargajual3` AS `bhargajual3`, `pi`.`pihargajual4` AS `bhargajual4`, `pi`.`pihargajual5` AS `bhargajual5`, `pi`.`pidiskonjual1` AS `bdiskonjual1`, `pi`.`pidiskonjual2` AS `bdiskonjual2`, `pi`.`pidiskonjual3` AS `bdiskonjual3`, `pi`.`pidiskonjual4` AS `bdiskonjual4`, `pi`.`pidiskonjual5` AS `bdiskonjual5`, (CASE i.bjenis WHEN 'J' THEN 0 ELSE IFNULL(SUM(isw.stok),0) END) AS `bstok`, (CASE i.bjenis WHEN 'J' THEN 0 ELSE ifnull(sum(`ib`.`jmlbooking`), 0) END) AS `bstokbooking`, `i`.`bmarginminimal` AS `bmarginminimal`, `i`.`brekpersediaan` AS `brekpersediaan`, `i`.`brekpenjualan` AS `brekpenjualan`, `i`.`brekreturpenjualan` AS `brekreturpenjualan`, `i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`, `i`.`brekhargapokok` AS `brekhargapokok`, `i`.`brekreturpembelian` AS `brekreturpembelian`, `i`.`brekdiskonpembelian` AS `brekdiskonpembelian`, `i`.`brekkonsinyasi` AS `brekkonsinyasi`, `i`.`bserial` AS `bserial`, `i`.`bbatch` AS `bbatch`, `i`.`bnilaisatuan` AS `bnilaisatuan`, `i`.`bnilaisatuandefault` AS `bnilaisatuandefault`, `i`.`bsuplier` AS `bsuplier`, `c`.`kkode` AS `bsuplierkode`, `c`.`knama` AS `bsupliernama`, `pi`.`pistokminimal` AS `bstokminimal`, `pi`.`pistokmaksimal` AS `bstokmaksimal`, `i`.`bstatusmoving` AS `bstatusmoving`, `i`.`binputuser` AS `binputuser`, `i`.`binputtgl` AS `binputtgl`, `i`.`bmodifikasiuser` AS `bmodifikasiuser`, `i`.`bmodifikasitgl` AS `bmodifikasitgl`, `f`.`fnamafile` AS `fnamafile`, l.lkategoripos as lkategoripos, pc.pcnama, pi.pistokreorder, ic.icnama from `m1_item` `i` join `m0_user` `u` on `u`.`userid` = 'valuserid' join `m1_location` `l` on `u`.`ulokasi` = `l`.`lkode` join `m_12_pos_item` `pi` on `l`.`lkategoripos` = `pi`.`pikategori` and `i`.`bid` = `pi`.`piidbarang` join m_12_pos_category pc on l.lkategoripos = pc.pckode join m0_user_warehouse uw on u.userid = uw.userid left join m1_item_stock_warehouse isw on uw.gudang = isw.kgudang and i.bid = isw.idbarang left join m1_item_category ic on i.bkategori = ic.ickode left join `m1_item_booking` `ib` on `i`.`bid` = `ib`.`idbarang` and uw.gudang = ib.gudang left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_files` `f` on `i`.`bid` = `f`.`fidtransaksi` and `f`.`fdefault` = 1 and `f`.`fsumber` = 'Item'"
        sql = sql.Replace("valuserid", userid)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "i.bid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("bid"), 0), sptField,
                    FxDB(dr("bkode"), ""), sptField,
                    FxDB(dr("bnama"), ""), sptField,
                    FxDB(dr("btipe"), ""), sptField,
                    FxDB(dr("bjenis"), ""), sptField,
                    FxDB(dr("bkategori"), ""), sptField,
                    FxDB(dr("bsatuan"), ""), sptField,
                    FxDB(dr("bsatuandefault"), ""), sptField,
                    FxDB(dr("bhpp"), ""), sptField,
                    FxDB(dr("bbarcode"), ""), sptField,
                    FxDB(dr("bhargabeli"), 0), sptField,
                    FxDB(dr("bhppaverage"), 0), sptField,
                    FxDB(dr("bhargajual1"), 0), sptField,
                    FxDB(dr("bhargajual2"), 0), sptField,
                    FxDB(dr("bhargajual3"), 0), sptField,
                    FxDB(dr("bhargajual4"), 0), sptField,
                    FxDB(dr("bhargajual5"), 0), sptField,
                    FxDB(dr("bdiskonjual1"), 0), sptField,
                    FxDB(dr("bdiskonjual2"), 0), sptField,
                    FxDB(dr("bdiskonjual3"), 0), sptField,
                    FxDB(dr("bdiskonjual4"), 0), sptField,
                    FxDB(dr("bdiskonjual5"), 0), sptField,
                    FxDB(dr("bstok"), 0), sptField,
                    FxDB(dr("bstokbooking"), 0), sptField,
                    FxDB(dr("bmarginminimal"), 0), sptField,
                    FxDB(dr("brekpersediaan"), ""), sptField,
                    FxDB(dr("brekreturpenjualan"), ""), sptField,
                    FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                    FxDB(dr("brekhargapokok"), ""), sptField,
                    FxDB(dr("brekreturpembelian"), ""), sptField,
                    FxDB(dr("brekdiskonpembelian"), ""), sptField,
                    FxDB(dr("brekkonsinyasi"), ""), sptField,
                    FxDB(dr("bserial"), ""), sptField,
                    FxDB(dr("bbatch"), ""), sptField,
                    FxDB(dr("bnilaisatuan"), 0), sptField,
                    FxDB(dr("bnilaisatuandefault"), 0), sptField,
                    FxDB(dr("bsuplier"), 0), sptField,
                    FxDB(dr("bsuplierkode"), ""), sptField,
                    FxDB(dr("bsupliernama"), ""), sptField,
                    FxDB(dr("bstokminimal"), 0), sptField,
                    FxDB(dr("bstokmaksimal"), 0), sptField,
                    FxDB(dr("bstatusmoving"), ""), sptField,
                    FxDB(dr("binputuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("bmodifikasiuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("fnamafile"), ""), sptField,
                    FxDB(dr("lkategoripos"), ""), sptField,
                    FxDB(dr("pcnama"), ""), sptField,
                    FxDB(dr("pistokreorder"), 0), sptField,
                    FxDB(dr("icnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bstokminimal, bstokmaksimal, bstatusmoving, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, fnamafile, lkategoripos, pcnama, pistokreorder, icnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Item_Stock_WarehouseSearch(ByVal param As String) As String
        'M12_Item_Stock_WarehouseSearch --------------------------------------------------------
        'idbarang, kodebarang, gudang, namagudang, idlokasi, kodelokasi, namalokasi, 
        'stok

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
            Filter = pagingSplit(2) & " AND (CASE LENGTH(IFNULL(uw.gudang,'')) WHEN 0 THEN isw.kgudang LIKE '%' ELSE isw.kgudang = uw.gudang END)"
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("kodebarang", "blgkodebarang")
        Else
            Filter = " (CASE LENGTH(IFNULL(uw.gudang,'')) WHEN 0 THEN isw.kgudang LIKE '%' ELSE isw.kgudang = uw.gudang END)"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_item_stock_warehouse_v")
        'sql = "select `isw`.`idbarang` AS `idbarang`, `ilw`.`blgkodebarang` AS `kodebarang`, `isw`.`kgudang` AS `gudang`, `wh`.`wnama` AS `namagudang`, `ilw`.`blgidlokasi` AS `idlokasi`, `ilw`.`blgkodelokasi` AS `kodelokasi`, `ilw`.`blgnamalokasi` AS `namalokasi`, `isw`.`stok` AS `stok` from `m1_item_stock_warehouse` `isw` join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' left join m0_user_warehouse uw on ul.uluser = uw.userid and isw.kgudang = uw.gudang left join `m1_item_location_warehouse` `ilw` on `isw`.`idbarang` = `ilw`.`blgidbarang` and `isw`.`kgudang` = `ilw`.`blggudang` left join `m1_warehouse` `wh` on `isw`.`kgudang` = `wh`.`wkode`"
        sql = "select `isw`.`idbarang` AS `idbarang`, `ilw`.`blgkodebarang` AS `kodebarang`, `isw`.`kgudang` AS `gudang`, `wh`.`wnama` AS `namagudang`, `ilw`.`blgidlokasi` AS `idlokasi`, `ilw`.`blgkodelokasi` AS `kodelokasi`, `ilw`.`blgnamalokasi` AS `namalokasi`, `isw`.`stok` AS `stok` from `m1_item_stock_warehouse` `isw` join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' left join m0_user_warehouse uw on ul.uluser = uw.userid left join `m1_item_location_warehouse` `ilw` on `isw`.`idbarang` = `ilw`.`blgidbarang` and `isw`.`kgudang` = `ilw`.`blggudang` left join `m1_warehouse` `wh` on `isw`.`kgudang` = `wh`.`wkode`"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Location_Warehouse", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("namagudang"), ""), sptField,
                     FxDB(dr("idlokasi"), 0), sptField,
                     FxDB(dr("kodelokasi"), ""), sptField,
                     FxDB(dr("namalokasi"), ""), sptField,
                     FxDB(dr("stok"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Stock Warehouse data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idbarang, kodebarang, gudang, namagudang, idlokasi, kodelokasi, namalokasi, stok"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Item_Stock_WarehouseSearchOld(ByVal param As String) As String
        'M12_Item_Stock_WarehouseSearch --------------------------------------------------------
        'idbarang, kodebarang, gudang, namagudang, idlokasi, kodelokasi, namalokasi, 
        'stok

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
            Filter = Filter.Replace("kodebarang", "blgkodebarang")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_item_stock_warehouse_v")
        sql = "select `isw`.`idbarang` AS `idbarang`, `ilw`.`blgkodebarang` AS `kodebarang`, `isw`.`kgudang` AS `gudang`, `wh`.`wnama` AS `namagudang`, `ilw`.`blgidlokasi` AS `idlokasi`, `ilw`.`blgkodelokasi` AS `kodelokasi`, `ilw`.`blgnamalokasi` AS `namalokasi`, `isw`.`stok` AS `stok` from `m1_item_stock_warehouse` `isw` join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_warehouse uw on ul.uluser = uw.userid and isw.kgudang = uw.gudang left join `m1_item_location_warehouse` `ilw` on `isw`.`idbarang` = `ilw`.`blgidbarang` and `isw`.`kgudang` = `ilw`.`blggudang` left join `m1_warehouse` `wh` on `isw`.`kgudang` = `wh`.`wkode`"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Location_Warehouse", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("namagudang"), ""), sptField,
                     FxDB(dr("idlokasi"), 0), sptField,
                     FxDB(dr("kodelokasi"), ""), sptField,
                     FxDB(dr("namalokasi"), ""), sptField,
                     FxDB(dr("stok"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Stock Warehouse data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idbarang, kodebarang, gudang, namagudang, idlokasi, kodelokasi, namalokasi, stok"))

        Return wsResult
    End Function

End Class
