Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_item_History
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Item_HistorySimpan(ByVal param As String) As String
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
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
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
            sql = "INSERT INTO m1_item_history(SELECT 0, item.* FROM m1_item item WHERE item.bid = '" & idtransaksi & "')"
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
            sql = "SELECT bidhistory FROM m1_item_history WHERE bid = '" & idtransaksi & "' ORDER BY bmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY LOCATION WAREHOUSE --------------------------
            sql = "INSERT INTO m1_item_location_warehouse_history(SELECT '" & FixDouble(result(4)) & "', 0, item.* FROM m1_item_location_warehouse item WHERE item.blgidbarang = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY LOCATION WAREHOUSE -------------------


            'PROSES INSERT HISTORY ITEM ASSEMBLY -------------------------------
            sql = "INSERT INTO m1_item_assembly_history(SELECT '" & FixDouble(result(4)) & "', 0, item.* FROM m1_item_assembly item WHERE item.iaidbarang = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY ITEM ASSEMBLY -----------------------


            'PROSES INSERT HISTORY ITEM SUPPLIER ------------------------------
            sql = "INSERT INTO m1_item_supplier_history(SELECT '" & FixDouble(result(4)) & "', 0, item.* FROM m1_item_supplier item WHERE item.isidbarang = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY ITEM SUPPLIER -----------------------


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
    Public Function M1_Item_HistorySearch(ByVal param As String) As String
        'M1_Item_HistorySearch --------------------------------------------------------
        'bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
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
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, 
        'bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, 
        'bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, 
        'brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama, binputusernama, bmodifikasiusernama,
        'bkelasproduk, bretur, btag, bminorder, bmobile, bassembly, bdownloaded, bkelasproduknama, btagnama, bkp, bkl

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
            Filter = Filter.Replace("bid", "i.bid")
            Filter = Filter.Replace("bkode", "i.bkode")
            Filter = Filter.Replace("btipe", "i.btipe")
            Filter = Filter.Replace("bstok", "i.bstok")
            Filter = Filter.Replace("bsatuan", "i.bsatuan")
            Filter = Filter.Replace("bnama", "i.bnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bidhistory"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
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
                     FxDB(dr("bsubitemdari"), 0), sptField,
                     FxDB(dr("bbarcode"), ""), sptField,
                     FxDB(dr("bsuplier"), 0), sptField,
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
                     FxDB(dr("bdiskonjual1"), 0), sptField,
                     FxDB(dr("bdiskonjual2"), 0), sptField,
                     FxDB(dr("bdiskonjual3"), 0), sptField,
                     FxDB(dr("bdiskonjual4"), 0), sptField,
                     FxDB(dr("bdiskonjual5"), 0), sptField,
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
                     FxDB(dr("bpengganti"), 0), sptField,
                     FxDB(dr("bgambar"), ""), sptField,
                     FxDB(dr("burutan"), 0), sptField,
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
                     FxDB(dr("binputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bedithpp"), 0), sptField,
                     FxDB(dr("btipenama"), ""), sptField,
                     FxDB(dr("bkategorinama"), ""), sptField,
                     FxDB(dr("bsatuannama"), ""), sptField,
                     FxDB(dr("bsatuandefaultnama"), ""), sptField,
                     FxDB(dr("bcabangnama"), ""), sptField,
                     FxDB(dr("blokasinama"), ""), sptField,
                     FxDB(dr("bdivisinama"), ""), sptField,
                     FxDB(dr("bsubdivisinama"), ""), sptField,
                     FxDB(dr("bgudangnama"), ""), sptField,
                     FxDB(dr("bproyeknama"), ""), sptField,
                     FxDB(dr("bsubitemdarikode"), ""), sptField,
                     FxDB(dr("bsuplierkode"), ""), sptField,
                     FxDB(dr("bsupliernama"), ""), sptField,
                     FxDB(dr("bpajakbelinama"), ""), sptField,
                     FxDB(dr("bpajakjualnama"), ""), sptField,
                     FxDB(dr("brekpersediaannama"), ""), sptField,
                     FxDB(dr("brekpenjualannama"), ""), sptField,
                     FxDB(dr("brekreturpenjualannama"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualannama"), ""), sptField,
                     FxDB(dr("brekhargapokoknama"), ""), sptField,
                     FxDB(dr("brekreturpembeliannama"), ""), sptField,
                     FxDB(dr("brekdiskonpembeliannama"), ""), sptField,
                     FxDB(dr("brekkonsinyasinama"), ""), sptField,
                     FxDB(dr("binputusernama"), ""), sptField,
                     FxDB(dr("bmodifikasiusernama"), ""), sptField,
                     FxDB(dr("bkelasproduk"), ""), sptField,
                     FxDB(dr("bretur"), 0), sptField,
                     FxDB(dr("btag"), ""), sptField,
                     FxDB(dr("bminorder"), 0), sptField,
                     FxDB(dr("bmobile"), 0), sptField,
                     FxDB(dr("bassembly"), 0), sptField,
                     FxDB(dr("bdownloaded"), 0), sptField,
                     FxDB(dr("bkelasproduknama"), ""), sptField,
                     FxDB(dr("btagnama"), ""), sptField,
                     FxDB(dr("bkp"), 0), sptField,
                     FxDB(dr("bkl"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama, binputusernama, bmodifikasiusernama, bkelasproduk, bretur, btag, bminorder, bmobile, bassembly, bdownloaded, bkelasproduknama, btagnama, bkp, bkl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HistoryGetdataById(ByVal param As String) As String

        'M1_Item_HistoryGetdataById Utama --------------------------------------------------------
        'bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
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
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, 
        'bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, 
        'bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, 
        'brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, 
        'brekkonsinyasinama, bjmlterkait, bkomisikode, bkomisinama, bmobile, bassembly,
        'bkelasproduk, bretur, btag, bminorder, bdownloaded, bkelasproduknama, btagnama, 
        'bdepartemen, bsubdepartemen, bdepartemennama, bsubdepartemennama, bkp, bkl, bjmllapangan, bsatuanlapangan,
        'bakelasnama, bsubkelasnama, bawarnanama, bdesignernama, bamodelnama, bamerknama, bmaterialnama, baoemnama, 
        'bsectionnama, baukurannama, bvendornama

        'M1_ItemGetdataById Item Location Warehouse ---------------------------------------
        'blgidhistorybarang, blgidhistory, blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, 
        'blginputtgl, blgmodifikasiuser, blgmodifikasitgl

        'M1_ItemGetdataById Item Assembly -------------------------------------------------
        'iaidhistorybarang, iaidhistory, iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, 
        'iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl

        'M1_ItemGetdataById Item Supplier -------------------------------------------------
        'isidhistorybarang, isidhistory, isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, 
        'iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, 
        'iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3, kkode, knama

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

        Dim utama As String = "", lokasigudang As String = "", assembly As String = "", supplier As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Vp~M4_Vp_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "i.bidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "i.bidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("bidhistory"), 0), sptField,
                     FxDB(drutama("bid"), 0), sptField,
                     FxDB(drutama("bkode"), ""), sptField,
                     FxDB(drutama("bnama"), ""), sptField,
                     FxDB(drutama("bnamaalias1"), ""), sptField,
                     FxDB(drutama("bnamaalias2"), ""), sptField,
                     FxDB(drutama("bnamaalias3"), ""), sptField,
                     FxDB(drutama("bnamaalias4"), ""), sptField,
                     FxDB(drutama("bnamaalias5"), ""), sptField,
                     FxDB(drutama("btipe"), ""), sptField,
                     FxDB(drutama("bjenis"), ""), sptField,
                     FxDB(drutama("bjenisdetail"), 0), sptField,
                     FxDB(drutama("bkategori"), ""), sptField,
                     FxDB(drutama("bketerangan"), ""), sptField,
                     FxDB(drutama("bsatuan"), ""), sptField,
                     FxDB(drutama("bnilaisatuan"), 0), sptField,
                     FxDB(drutama("bsatuandefault"), ""), sptField,
                     FxDB(drutama("bnilaisatuandefault"), 0), sptField,
                     FxDB(drutama("bhpp"), ""), sptField,
                     FxDB(drutama("bcabang"), ""), sptField,
                     FxDB(drutama("blokasi"), ""), sptField,
                     FxDB(drutama("bdivisi"), ""), sptField,
                     FxDB(drutama("bsubdivisi"), ""), sptField,
                     FxDB(drutama("bgudang"), ""), sptField,
                     FxDB(drutama("bproyek"), ""), sptField,
                     FxDB(drutama("bsubitem"), 0), sptField,
                     FxDB(drutama("bsubitemdari"), 0), sptField,
                     FxDB(drutama("bbarcode"), ""), sptField,
                     FxDB(drutama("bsuplier"), 0), sptField,
                     FxDB(drutama("baktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("baktiftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bstokminimal"), 0), sptField,
                     FxDB(drutama("bstokmaksimal"), 0), sptField,
                     FxDB(drutama("breorder"), 0), sptField,
                     FxDB(drutama("bjmlorderbeli"), 0), sptField,
                     FxDB(drutama("bjmlorderjual"), 0), sptField,
                     FxDB(drutama("bkategoriumur"), ""), sptField,
                     FxDB(drutama("bstatusmoving"), ""), sptField,
                     FxDB(drutama("bsifatharga"), ""), sptField,
                     FxDB(drutama("bpromo"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bpromoberlaku"), ""), formatTgl), sptField,
                     FxDB(drutama("bpajakbeli"), ""), sptField,
                     FxDB(drutama("bpajakjual"), ""), sptField,
                     FxDB(drutama("bhargabeli"), 0), sptField,
                     FxDB(drutama("bhppaverage"), 0), sptField,
                     FxDB(drutama("bhargajual1"), 0), sptField,
                     FxDB(drutama("bhargajual2"), 0), sptField,
                     FxDB(drutama("bhargajual3"), 0), sptField,
                     FxDB(drutama("bhargajual4"), 0), sptField,
                     FxDB(drutama("bhargajual5"), 0), sptField,
                     FxDB(drutama("bdiskonjual1"), 0), sptField,
                     FxDB(drutama("bdiskonjual2"), 0), sptField,
                     FxDB(drutama("bdiskonjual3"), 0), sptField,
                     FxDB(drutama("bdiskonjual4"), 0), sptField,
                     FxDB(drutama("bdiskonjual5"), 0), sptField,
                     FxDB(drutama("bstok"), 0), sptField,
                     FxDB(drutama("bkomisi"), 0), sptField,
                     FxDB(drutama("bmarginminimal"), 0), sptField,
                     FxDB(drutama("brekpersediaan"), ""), sptField,
                     FxDB(drutama("brekpenjualan"), ""), sptField,
                     FxDB(drutama("brekreturpenjualan"), ""), sptField,
                     FxDB(drutama("brekdiskonpenjualan"), ""), sptField,
                     FxDB(drutama("brekhargapokok"), ""), sptField,
                     FxDB(drutama("brekreturpembelian"), ""), sptField,
                     FxDB(drutama("brekdiskonpembelian"), ""), sptField,
                     FxDB(drutama("brekkonsinyasi"), ""), sptField,
                     FxDB(drutama("bapanjang"), 0), sptField,
                     FxDB(drutama("balebar"), 0), sptField,
                     FxDB(drutama("batinggi"), 0), sptField,
                     FxDB(drutama("bavolume"), 0), sptField,
                     FxDB(drutama("baberat"), 0), sptField,
                     FxDB(drutama("bawarna"), ""), sptField,
                     FxDB(drutama("baoem"), ""), sptField,
                     FxDB(drutama("bamerk"), ""), sptField,
                     FxDB(drutama("baukuran"), ""), sptField,
                     FxDB(drutama("bamodel"), ""), sptField,
                     FxDB(drutama("bakelas"), ""), sptField,
                     FxDB(drutama("bserial"), 0), sptField,
                     FxDB(drutama("bbatch"), 0), sptField,
                     FxDB(drutama("bpengganti"), 0), sptField,
                     FxDB(drutama("bgambar"), ""), sptField,
                     FxDB(drutama("burutan"), 0), sptField,
                     FxDB(drutama("bcustom1"), ""), sptField,
                     FxDB(drutama("bcustom2"), ""), sptField,
                     FxDB(drutama("bcustom3"), ""), sptField,
                     FxDB(drutama("bcustom4"), ""), sptField,
                     FxDB(drutama("bcustom5"), ""), sptField,
                     FxDB(drutama("bcustom6"), ""), sptField,
                     FxDB(drutama("bcustom7"), ""), sptField,
                     FxDB(drutama("bcustom8"), ""), sptField,
                     FxDB(drutama("bcustom9"), ""), sptField,
                     FxDB(drutama("bcustom10"), ""), sptField,
                     FxDB(drutama("bcustom11"), 0), sptField,
                     FxDB(drutama("bcustom12"), 0), sptField,
                     FxDB(drutama("bcustom13"), 0), sptField,
                     FxDB(drutama("bcustom14"), 0), sptField,
                     FxDB(drutama("bcustom15"), 0), sptField,
                     FxDB(drutama("bcatatan"), ""), sptField,
                     FxDB(drutama("binputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bedithpp"), 0), sptField,
                     FxDB(drutama("btipenama"), ""), sptField,
                     FxDB(drutama("bkategorinama"), ""), sptField,
                     FxDB(drutama("bsatuannama"), ""), sptField,
                     FxDB(drutama("bsatuandefaultnama"), ""), sptField,
                     FxDB(drutama("bcabangnama"), ""), sptField,
                     FxDB(drutama("blokasinama"), ""), sptField,
                     FxDB(drutama("bdivisinama"), ""), sptField,
                     FxDB(drutama("bsubdivisinama"), ""), sptField,
                     FxDB(drutama("bgudangnama"), ""), sptField,
                     FxDB(drutama("bproyeknama"), ""), sptField,
                     FxDB(drutama("bsubitemdarikode"), ""), sptField,
                     FxDB(drutama("bsuplierkode"), ""), sptField,
                     FxDB(drutama("bsupliernama"), ""), sptField,
                     FxDB(drutama("bpajakbelinama"), ""), sptField,
                     FxDB(drutama("bpajakjualnama"), ""), sptField,
                     FxDB(drutama("brekpersediaannama"), ""), sptField,
                     FxDB(drutama("brekpenjualannama"), ""), sptField,
                     FxDB(drutama("brekreturpenjualannama"), ""), sptField,
                     FxDB(drutama("brekdiskonpenjualannama"), ""), sptField,
                     FxDB(drutama("brekhargapokoknama"), ""), sptField,
                     FxDB(drutama("brekreturpembeliannama"), ""), sptField,
                     FxDB(drutama("brekdiskonpembeliannama"), ""), sptField,
                     FxDB(drutama("brekkonsinyasinama"), ""), sptField,
                     0, sptField,
                     FxDB(drutama("bkomisikode"), ""), sptField,
                     FxDB(drutama("bkomisinama"), ""), sptField,
                     FxDB(drutama("bmobile"), 0), sptField,
                     FxDB(drutama("bassembly"), 0), sptField,
                     FxDB(drutama("bkelasproduk"), ""), sptField,
                     FxDB(drutama("bretur"), 0), sptField,
                     FxDB(drutama("btag"), ""), sptField,
                     FxDB(drutama("bminorder"), 0), sptField,
                     FxDB(drutama("bdownloaded"), 0), sptField,
                     FxDB(drutama("bkelasproduknama"), ""), sptField,
                     FxDB(drutama("btagnama"), ""), sptField,
                     FxDB(drutama("bdepartemen"), ""), sptField,
                     FxDB(drutama("bsubdepartemen"), ""), sptField,
                     FxDB(drutama("bdepartemennama"), ""), sptField,
                     FxDB(drutama("bsubdepartemennama"), ""), sptField,
                     FxDB(drutama("bkp"), 0), sptField,
                     FxDB(drutama("bkl"), 0), sptField,
                     FxDB(drutama("bjmllapangan"), 0), sptField,
                     FxDB(drutama("bsatuanlapangan"), ""), sptField,
                     FxDB(drutama("bakelasnama"), ""), sptField,
                     FxDB(drutama("bsubkelasnama"), ""), sptField,
                     FxDB(drutama("bawarnanama"), ""), sptField,
                     FxDB(drutama("bdesignernama"), ""), sptField,
                     FxDB(drutama("bamodelnama"), ""), sptField,
                     FxDB(drutama("bamerknama"), ""), sptField,
                     FxDB(drutama("bmaterialnama"), ""), sptField,
                     FxDB(drutama("baoemnama"), ""), sptField,
                     FxDB(drutama("bsectionnama"), ""), sptField,
                     FxDB(drutama("baukurannama"), ""), sptField,
                     FxDB(drutama("bvendornama"), ""), sptField,
                     FxDB(drutama("bsubkelas"), ""), sptField,
                     FxDB(drutama("bdesigner"), ""), sptField,
                     FxDB(drutama("bmaterial"), ""), sptField,
                     FxDB(drutama("bsection"), ""), sptField,
                     FxDB(drutama("bvendor"), ""))

            Dim inputtgl As String = "", modiftgl As String = ""
            For Each dr As DataRow In dt.Rows
                inputtgl = FxDB(dr("blginputtgl"), "")
                modiftgl = FxDB(dr("blgmodifikasitgl"), "")

                If Len(inputtgl) > 0 Then inputtgl = AsFormatTanggal(inputtgl, formatTglWaktu)
                If Len(modiftgl) > 0 Then modiftgl = AsFormatTanggal(modiftgl, formatTglWaktu)

                lokasigudang = String.Concat(lokasigudang,
                     FxDB(dr("blgidhistorybarang"), 0), sptField,
                     FxDB(dr("blgidhistory"), 0), sptField,
                     FxDB(dr("blgidbarang"), 0), sptField,
                     FxDB(dr("blgkodebarang"), ""), sptField,
                     FxDB(dr("blggudang"), ""), sptField,
                     FxDB(dr("blgidlokasi"), 0), sptField,
                     FxDB(dr("blgkodelokasi"), ""), sptField,
                     FxDB(dr("blgnamalokasi"), ""), sptField,
                     FxDB(dr("blginputuser"), 0), sptField,
                     inputtgl, sptField,
                     FxDB(dr("blgmodifikasiuser"), 0), sptField,
                     modiftgl, sptRow)
            Next
            If lokasigudang.Length > 0 Then lokasigudang = lokasigudang.Substring(0, lokasigudang.Length - sptRow.Length) Else lokasigudang = lokasigudang

            'AMBIL DATA ITEM ASSEMBLY
            Dim dtassembly As New DataTable
            dtassembly = AmbilData("aplikasi1-M1_Item_Assembly_History", "iaidhistorybarang = " & idtransaksi, "iaidbarang ASC, iaurutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
            For Each dr As DataRow In dtassembly.Rows
                assembly = String.Concat(assembly,
                             FxDB(dr("iaidhistorybarang"), 0), sptField,
                             FxDB(dr("iaidhistory"), 0), sptField,
                             FxDB(dr("iaidbarang"), 0), sptField,
                             FxDB(dr("iakodebarang"), ""), sptField,
                             FxDB(dr("iaidbarangpenyusun"), 0), sptField,
                             FxDB(dr("iakodebarangpenyusun"), ""), sptField,
                             FxDB(dr("iaurutan"), 0), sptField,
                             FxDB(dr("iajml"), 0), sptField,
                             FxDB(dr("iasatuan"), ""), sptField,
                             FxDB(dr("iainputuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("iainputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("iamodifikasiuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("iamodifikasitgl"), ""), formatTglWaktu), sptRow)
            Next
            If assembly.Length > 0 Then assembly = assembly.Substring(0, assembly.Length - sptRow.Length) Else assembly = assembly

            'AMBIL DATA ITEM Supplier
            Dim dtSupplier As New DataTable
            sql = "SELECT its.isidhistorybarang, its.isidhistory, its.isidbarang, its.isidkontak, its.iscatatan, its.isurutan, its.iscustomtext1, its.iscustomtext2, its.iscustomtext3, its.iscustomtext4, its.iscustomtext5, its.iscustomint1, its.iscustomint2, its.iscustomint3, its.iscustomdbl1, its.iscustomdbl2, its.iscustomdbl3, its.iscustomdate1, its.iscustomdate2, its.iscustomdate3, c.kkode, c.knama FROM m1_item_supplier_history its JOIN m1_contact c ON its.isidkontak = c.kid"
            dtSupplier = AmbilData("aplikasi1-M1_Item_Supplier", "isidhistorybarang = " & idtransaksi, "isidbarang ASC, isurutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtSupplier.Rows
                supplier = String.Concat(supplier,
                     FxDB(dr("isidbarang"), 0), sptField,
                     FxDB(dr("isidkontak"), 0), sptField,
                     FxDB(dr("iscatatan"), ""), sptField,
                     FxDB(dr("isurutan"), 0), sptField,
                     FxDB(dr("iscustomtext1"), ""), sptField,
                     FxDB(dr("iscustomtext2"), ""), sptField,
                     FxDB(dr("iscustomtext3"), ""), sptField,
                     FxDB(dr("iscustomtext4"), ""), sptField,
                     FxDB(dr("iscustomtext5"), ""), sptField,
                     FxDB(dr("iscustomint1"), 0), sptField,
                     FxDB(dr("iscustomint2"), 0), sptField,
                     FxDB(dr("iscustomint3"), 0), sptField,
                     FxDB(dr("iscustomdbl1"), 0), sptField,
                     FxDB(dr("iscustomdbl2"), 0), sptField,
                     FxDB(dr("iscustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("iscustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("iscustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("iscustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptRow)
            Next
            If supplier.Length > 0 Then supplier = supplier.Substring(0, supplier.Length - sptRow.Length) Else supplier = supplier

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item history data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, lokasigudang, sptSubParam, assembly, sptSubParam, supplier)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bidhistory, bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama, bjmlterkait, bkomisikode, bkomisinama, bmobile, bassembly, bkelasproduk, bretur, btag, bminorder, bdownloaded, bkelasproduknama, btagnama, bdepartemen, bsubdepartemen, bdepartemennama, bsubdepartemennama, bkp, bkl, bjmllapangan, bsatuanlapangan, bakelasnama, bsubkelasnama, bawarnanama, bdesignernama, bamodelnama, bamerknama, bmaterialnama, baoemnama, bsectionnama, baukurannama, bvendornama, bsubkelas, bdesigner, bmaterial, bsection, bvendor" & sptSubParam & "blgidhistorybarang, blgidhistory, blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, blginputtgl, blgmodifikasiuser, blgmodifikasitgl" & sptSubParam & "iaidhistorybarang, iaidhistory, iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl" & sptSubParam & "isidhistorybarang, isidhistory, isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3, kkode, knama"))

        Return wsResult
    End Function

End Class
