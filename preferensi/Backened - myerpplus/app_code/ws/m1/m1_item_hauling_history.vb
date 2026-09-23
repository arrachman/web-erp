Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_item_hauling_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Item_Hauling_HistorySimpan(ByVal param As String) As String

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
        idtransaksi = dataUtama(0)

        If IsNumeric(idtransaksi) = False Then
            result(2) = "bid required numeric." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO M1_Item_Hauling_History(SELECT 0, ih.* FROM m1_item_hauling ih WHERE ih.bid = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY UTAMA --------------------------------


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
    Public Function M1_Item_Hauling_HistorySearch(ByVal param As String) As String
        'M1_Item_Hauling_HistorySearch --------------------------------------------------------
        'bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, 
        'bastatus, bahourmeter, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, 
        'binputusernama, bmodifikasiusernama

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

        'BUAT QUERY
        sql = "select `ih`.`bidhistory` AS `bidhistory`,`ih`.`bid` AS `bid`,`ih`.`bkode` AS `bkode`,`ih`.`bnama` AS `bnama`,`ih`.`bnamaalias1` AS `bnamaalias1`,`ih`.`bnamaalias2` AS `bnamaalias2`,`ih`.`bnamaalias3` AS `bnamaalias3`,`ih`.`bnamaalias4` AS `bnamaalias4`,`ih`.`bnamaalias5` AS `bnamaalias5`,`ih`.`btipe` AS `btipe`,`ih`.`bketerangan` AS `bketerangan`,`ih`.`bsatuan` AS `bsatuan`,`ih`.`bnilaisatuan` AS `bnilaisatuan`,`ih`.`bsatuandefault` AS `bsatuandefault`,`ih`.`bnilaisatuandefault` AS `bnilaisatuandefault`,`ih`.`bastatus` AS `bastatus`,`ih`.`bahourmeter` AS `bahourmeter`,`ih`.`bcatatan` AS `bcatatan`,`ih`.`binputuser` AS `binputuser`,`ih`.`binputtgl` AS `binputtgl`,`ih`.`bmodifikasiuser` AS `bmodifikasiuser`,`ih`.`bmodifikasitgl` AS `bmodifikasitgl`,`u1`.`unama` AS `binputusernama`,`u2`.`unama` AS `bmodifikasiusernama` from ((`m1_item_hauling_history` `ih` left join `m0_user` `u1` on((`ih`.`binputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`ih`.`bmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Hauling_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bidhistory"), ""), sptField,
                     FxDB(dr("bid"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("bnamaalias1"), ""), sptField,
                     FxDB(dr("bnamaalias2"), ""), sptField,
                     FxDB(dr("bnamaalias3"), ""), sptField,
                     FxDB(dr("bnamaalias4"), ""), sptField,
                     FxDB(dr("bnamaalias5"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bketerangan"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("bnilaisatuan"), 0), sptField,
                     FxDB(dr("bsatuandefault"), ""), sptField,
                     FxDB(dr("bnilaisatuandefault"), 0), sptField,
                     FxDB(dr("bastatus"), 0), sptField,
                     FxDB(dr("bahourmeter"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("binputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("binputusernama"), ""), sptField,
                     FxDB(dr("bmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Hauling History data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bastatus, bahourmeter, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, binputusernama, bmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_Hauling_HistoryGetdataAll(ByVal param As String) As String
        'M1_Item_Hauling_HistoryGetdataAll --------------------------------------------------------
        'bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bastatus, bahourmeter, bapanjang, balebar, batinggi, 
        'bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, 
        'bakelas, bserial, bbatch, bpengganti, bgambar, bedithpp, burutan, 
        'bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bcustomtext1, bcustomtext2, 
        'bcustomtext3, bcustomtext4, bcustomtext5, bcustomtext6, bcustomtext7, bcustomtext8, bcustomtext9, 
        'bcustomtext10, bcustomint1, bcustomint2, bcustomint3, bcustomint4, bcustomint5, bcustomdbl1, 
        'bcustomdbl2, bcustomdbl3, bcustomdbl4, bcustomdbl5, bcustomdate1, bcustomdate2, bcustomdate3, 
        'bcustomdate4, bcustomdate5, bcabangnama, blokasinama, bgudangnama, bdivisinama, bsubdivisinama, 
        'bproyeknama

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

        'BUAT QUERY
        sql = "select `ih`.`bidhistory` AS `bidhistory`, `ih`.`bid` AS `bid`,`ih`.`bkode` AS `bkode`,`ih`.`bnama` AS `bnama`,`ih`.`bnamaalias1` AS `bnamaalias1`,`ih`.`bnamaalias2` AS `bnamaalias2`,`ih`.`bnamaalias3` AS `bnamaalias3`,`ih`.`bnamaalias4` AS `bnamaalias4`,`ih`.`bnamaalias5` AS `bnamaalias5`,`ih`.`btipe` AS `btipe`,`ih`.`bjenis` AS `bjenis`,`ih`.`bjenisdetail` AS `bjenisdetail`,`ih`.`bkategori` AS `bkategori`,`ih`.`bketerangan` AS `bketerangan`,`ih`.`bsatuan` AS `bsatuan`,`ih`.`bnilaisatuan` AS `bnilaisatuan`,`ih`.`bsatuandefault` AS `bsatuandefault`,`ih`.`bnilaisatuandefault` AS `bnilaisatuandefault`,`ih`.`bhpp` AS `bhpp`,`ih`.`bcabang` AS `bcabang`,`ih`.`blokasi` AS `blokasi`,`ih`.`bdivisi` AS `bdivisi`,`ih`.`bsubdivisi` AS `bsubdivisi`,`ih`.`bgudang` AS `bgudang`,`ih`.`bproyek` AS `bproyek`,`ih`.`bsubitem` AS `bsubitem`,`ih`.`bsubitemdari` AS `bsubitemdari`,`ih`.`bbarcode` AS `bbarcode`,`ih`.`bsuplier` AS `bsuplier`,`ih`.`baktif` AS `baktif`,`ih`.`baktiftgl` AS `baktiftgl`,`ih`.`bstokminimal` AS `bstokminimal`,`ih`.`bstokmaksimal` AS `bstokmaksimal`,`ih`.`breorder` AS `breorder`,`ih`.`bjmlorderbeli` AS `bjmlorderbeli`,`ih`.`bjmlorderjual` AS `bjmlorderjual`,`ih`.`bkategoriumur` AS `bkategoriumur`,`ih`.`bstatusmoving` AS `bstatusmoving`,`ih`.`bsifatharga` AS `bsifatharga`,`ih`.`bpromo` AS `bpromo`,`ih`.`bpromoberlaku` AS `bpromoberlaku`,`ih`.`bpajakbeli` AS `bpajakbeli`,`ih`.`bpajakjual` AS `bpajakjual`,`ih`.`bhargabeli` AS `bhargabeli`,`ih`.`bhppaverage` AS `bhppaverage`,`ih`.`bhargajual1` AS `bhargajual1`,`ih`.`bhargajual2` AS `bhargajual2`,`ih`.`bhargajual3` AS `bhargajual3`,`ih`.`bhargajual4` AS `bhargajual4`,`ih`.`bhargajual5` AS `bhargajual5`,`ih`.`bdiskonjual1` AS `bdiskonjual1`,`ih`.`bdiskonjual2` AS `bdiskonjual2`,`ih`.`bdiskonjual3` AS `bdiskonjual3`,`ih`.`bdiskonjual4` AS `bdiskonjual4`,`ih`.`bdiskonjual5` AS `bdiskonjual5`,`ih`.`bstok` AS `bstok`,`ih`.`bkomisi` AS `bkomisi`,`ih`.`bmarginminimal` AS `bmarginminimal`,`ih`.`brekpersediaan` AS `brekpersediaan`,`ih`.`brekpenjualan` AS `brekpenjualan`,`ih`.`brekreturpenjualan` AS `brekreturpenjualan`,`ih`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`,`ih`.`brekhargapokok` AS `brekhargapokok`,`ih`.`brekreturpembelian` AS `brekreturpembelian`,`ih`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`ih`.`brekkonsinyasi` AS `brekkonsinyasi`,`ih`.`bastatus` AS `bastatus`,`ih`.`bahourmeter` AS `bahourmeter`,`ih`.`bapanjang` AS `bapanjang`,`ih`.`balebar` AS `balebar`,`ih`.`batinggi` AS `batinggi`,`ih`.`bavolume` AS `bavolume`,`ih`.`baberat` AS `baberat`,`ih`.`bawarna` AS `bawarna`,`ih`.`baoem` AS `baoem`,`ih`.`bamerk` AS `bamerk`,`ih`.`baukuran` AS `baukuran`,`ih`.`bamodel` AS `bamodel`,`ih`.`bakelas` AS `bakelas`,`ih`.`bserial` AS `bserial`,`ih`.`bbatch` AS `bbatch`,`ih`.`bpengganti` AS `bpengganti`,`ih`.`bgambar` AS `bgambar`,`ih`.`bedithpp` AS `bedithpp`,`ih`.`burutan` AS `burutan`,`ih`.`bcatatan` AS `bcatatan`,`ih`.`binputuser` AS `binputuser`,`ih`.`binputtgl` AS `binputtgl`,`ih`.`bmodifikasiuser` AS `bmodifikasiuser`,`ih`.`bmodifikasitgl` AS `bmodifikasitgl`,`ih`.`bcustomtext1` AS `bcustomtext1`,`ih`.`bcustomtext2` AS `bcustomtext2`,`ih`.`bcustomtext3` AS `bcustomtext3`,`ih`.`bcustomtext4` AS `bcustomtext4`,`ih`.`bcustomtext5` AS `bcustomtext5`,`ih`.`bcustomtext6` AS `bcustomtext6`,`ih`.`bcustomtext7` AS `bcustomtext7`,`ih`.`bcustomtext8` AS `bcustomtext8`,`ih`.`bcustomtext9` AS `bcustomtext9`,`ih`.`bcustomtext10` AS `bcustomtext10`,`ih`.`bcustomint1` AS `bcustomint1`,`ih`.`bcustomint2` AS `bcustomint2`,`ih`.`bcustomint3` AS `bcustomint3`,`ih`.`bcustomint4` AS `bcustomint4`,`ih`.`bcustomint5` AS `bcustomint5`,`ih`.`bcustomdbl1` AS `bcustomdbl1`,`ih`.`bcustomdbl2` AS `bcustomdbl2`,`ih`.`bcustomdbl3` AS `bcustomdbl3`,`ih`.`bcustomdbl4` AS `bcustomdbl4`,`ih`.`bcustomdbl5` AS `bcustomdbl5`,`ih`.`bcustomdate1` AS `bcustomdate1`,`ih`.`bcustomdate2` AS `bcustomdate2`,`ih`.`bcustomdate3` AS `bcustomdate3`,`ih`.`bcustomdate4` AS `bcustomdate4`,`ih`.`bcustomdate5` AS `bcustomdate5`,`br`.`bnama` AS `bcabangnama`,`lc`.`lnama` AS `blokasinama`,`w`.`wnama` AS `bgudangnama`,`d`.`dnama` AS `bdivisinama`,`sd`.`sdnama` AS `bsubdivisinama`,`p`.`pnama` AS `bproyeknama` from ((((((`m1_item_hauling_history` `ih` left join `m1_branch` `br` on((`ih`.`bcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`ih`.`blokasi` = `lc`.`lkode`))) left join `m1_warehouse` `w` on((`ih`.`bgudang` = `w`.`wkode`))) left join `m1_division` `d` on((`ih`.`bdivisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`ih`.`bsubdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`ih`.`bproyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Hauling", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bidhistory"), ""), sptField,
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
                     FxDB(dr("bastatus"), 0), sptField,
                     FxDB(dr("bahourmeter"), 0), sptField,
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
                     FxDB(dr("bedithpp"), 0), sptField,
                     FxDB(dr("burutan"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("binputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bcustomtext1"), ""), sptField,
                     FxDB(dr("bcustomtext2"), ""), sptField,
                     FxDB(dr("bcustomtext3"), ""), sptField,
                     FxDB(dr("bcustomtext4"), ""), sptField,
                     FxDB(dr("bcustomtext5"), ""), sptField,
                     FxDB(dr("bcustomtext6"), ""), sptField,
                     FxDB(dr("bcustomtext7"), ""), sptField,
                     FxDB(dr("bcustomtext8"), ""), sptField,
                     FxDB(dr("bcustomtext9"), ""), sptField,
                     FxDB(dr("bcustomtext10"), ""), sptField,
                     FxDB(dr("bcustomint1"), 0), sptField,
                     FxDB(dr("bcustomint2"), 0), sptField,
                     FxDB(dr("bcustomint3"), 0), sptField,
                     FxDB(dr("bcustomint4"), 0), sptField,
                     FxDB(dr("bcustomint5"), 0), sptField,
                     FxDB(dr("bcustomdbl1"), 0), sptField,
                     FxDB(dr("bcustomdbl2"), 0), sptField,
                     FxDB(dr("bcustomdbl3"), 0), sptField,
                     FxDB(dr("bcustomdbl4"), 0), sptField,
                     FxDB(dr("bcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate5"), ""), formatTgl), sptField,
                     FxDB(dr("bcabangnama"), ""), sptField,
                     FxDB(dr("blokasinama"), ""), sptField,
                     FxDB(dr("bgudangnama"), ""), sptField,
                     FxDB(dr("bdivisinama"), ""), sptField,
                     FxDB(dr("bsubdivisinama"), ""), sptField,
                     FxDB(dr("bproyeknama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Hauling History data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bastatus, bahourmeter, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, bedithpp, burutan, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bcustomtext1, bcustomtext2, bcustomtext3, bcustomtext4, bcustomtext5, bcustomtext6, bcustomtext7, bcustomtext8, bcustomtext9, bcustomtext10, bcustomint1, bcustomint2, bcustomint3, bcustomint4, bcustomint5, bcustomdbl1, bcustomdbl2, bcustomdbl3, bcustomdbl4, bcustomdbl5, bcustomdate1, bcustomdate2, bcustomdate3, bcustomdate4, bcustomdate5, bcabangnama, blokasinama, bgudangnama, bdivisinama, bsubdivisinama, bproyeknama"))

        Return wsResult
    End Function

End Class
