Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_lu_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function m11_Lu_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m_11_lu_history(SELECT 0, lu.* FROM m_11_lu lu WHERE lu.luid = '" & idtransaksi & "')"
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
            sql = "SELECT luidhistory FROM m_11_lu_history WHERE luid = '" & idtransaksi & "' ORDER BY lumodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m_11_lu_detail_history (SELECT 0, '" & result(4) & "', lu.* FROM m_11_lu_detail lu WHERE lu.idlu = '" & idtransaksi & "' )"
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
    Public Function M11_Lu_HistorySearch(ByVal param As String) As String
        'M11_LuSearch --------------------------------------------------------
        'luidhistory, luid, lucabang, lulokasi, lugudang, luasalbarang, luasalbarangkategori, lujenispenjualan, 
        'lujenispenjualankategori, lucarabayar, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, 
        'lucustomer, lucustomerkontak, lu1alamat1, lu1alamat2, lu1alamat3, lu2alamat1, lu2alamat2, 
        'lu2alamat3, lubagianpenjualan, luekspedisi, lutglkirim, lutermin, lutgljatuhtempo, luuraian, 
        'lucatatan, lunoref, lutglnoref, lutglpenutupan, lumatauang, lukurs, luhargatermasukpajak, 
        'lutotal, ludiskonpersen, lujmldiskon, lutotalpajak1detail, lutotalpajak2detail, lubiayalainpersen, lubiayalain, 
        'lutotaltransaksi, lujmlbayar, lurekdiskon, lurekpajak1, lurekpajak2, lurekbiayalain, lurekbayar, 
        'luidsq, lustatuspl, lustatusdo, lustatusdr, lustatuspi, lustatussi, lustatusrnr, 
        'lustatussr, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, 
        'luinputtgl, lumodifikasiuser, lumodifikasitgl, luposting, lupostingtgl, luisclose, lucabangnama, 
        'lulokasinama, lugudangnama, lucustomerkode, lucustomernama, lubagianpenjualankode, lubagianpenjualannama, luekspedisinama, 
        'lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lu_v_history")

        dt = AmbilData("aplikasi1-M11_lu_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("luidhistory"), 0), sptField,
                     FxDB(dr("luid"), 0), sptField,
                     FxDB(dr("lucabang"), ""), sptField,
                     FxDB(dr("lulokasi"), ""), sptField,
                     FxDB(dr("lugudang"), ""), sptField,
                     FxDB(dr("lusumber"), ""), sptField,
                     FxDB(dr("luautonotransaksi"), 0), sptField,
                     FxDB(dr("lunotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lutgl"), ""), formatTgl), sptField,
                     FxDB(dr("lukodepa"), 0), sptField,
                     FxDB(dr("lucustomer"), 0), sptField,
                     FxDB(dr("lucustomerkontak"), ""), sptField,
                     FxDB(dr("luuraian"), ""), sptField,
                     FxDB(dr("lucatatan"), ""), sptField,
                     FxDB(dr("lunoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lutglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("lutotaltransaksi"), 0), sptField,
                     FxDB(dr("luidkj"), 0), sptField,
                     FxDB(dr("lustatusrealisasi"), 0), sptField,
                     FxDB(dr("lustatus"), 0), sptField,
                     FxDB(dr("lustatussebelumnya"), 0), sptField,
                     FxDB(dr("lujmlrevisi"), 0), sptField,
                     FxDB(dr("lucetakanke"), 0), sptField,
                     FxDB(dr("luinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("luinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lumodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("lumodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("luisclose"), 0), sptField,
                     FxDB(dr("lucabangnama"), ""), sptField,
                     FxDB(dr("lulokasinama"), ""), sptField,
                     FxDB(dr("lugudangnama"), ""), sptField,
                     FxDB(dr("lucustomerkode"), ""), sptField,
                     FxDB(dr("lucustomernama"), ""), sptField,
                     FxDB(dr("lunotransaksikj"), ""), sptField,
                     FxDB(dr("lustatusnama"), ""), sptField,
                     FxDB(dr("lustatussebelumnyanama"), ""), sptField,
                     FxDB(dr("luinputusernama"), ""), sptField,
                     FxDB(dr("lumodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("luidhistory, luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucabangnama, lulokasinama, lugudangnama, lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_LuHistoryGetdataById(ByVal param As String) As String
        'M11_Lu_GetdataById Utama --------------------------------------------------------
        'luidhistory, luid, lucabang, lulokasi, lugudang, lusumber, 
        'luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, 
        'lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, 
        'lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, 
        'lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, 
        'lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, 
        'lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8,
        'lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13,
        'lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18,
        'lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3,
        'lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8,
        'lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13,
        'lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18,
        'lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, 
        'lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8,
        'lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13,
        'lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18,
        'lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, 
        'lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8,
        'lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13,
        'lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18,
        'lucustomdate19, lucustomdate20, lucabangnama, lulokasinama, lugudangnama, 
        'lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, 
        'luinputusernama, lumodifikasiusernama, lumatauang, lukurs, luposting
        'lupostingtgl

        'M11_Lu_GetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idludetail, idlu, jenis, idlayanan, namalayanan, 
        'jml, satuan, nilaisatuan, jmltotal, satuandefault, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, 
        'iddokter, namadokter, customtext1, customtext2, customtext3, 
        'customtext4, customtext5, customtext6, customtext7, customtext8,
        'customtext9, customtext10, customtext11, customtext12, customtext13,
        'customtext14, customtext15, customtext16, customtext17, customtext18,
        'customtext19, customtext20, customdbl1, customdbl2, customdbl3, 
        'customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,
        'customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,
        'customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,
        'customdbl19, customdbl20, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8,
        'customdate9, customdate10, customdate11, customdate12, customdate13,
        'customdate14, customdate15, customdate16, customdate17, customdate18,
        'customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi,
        'kodedokter, matauang, kurs, rekpersediaan, rekhargapokok
        'rekdiskonpenjualan, rekpenjualan

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

        Dim NmMemcached As String = "aplikasi1-M11_Lu~M11_Lu_Detail-" & idtransaksi

        'Resiace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "luidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "luidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lu_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("luidhistory"), 0), sptField,
                     FxDB(drutama("luid"), 0), sptField,
                     FxDB(drutama("lucabang"), ""), sptField,
                     FxDB(drutama("lulokasi"), ""), sptField,
                     FxDB(drutama("lugudang"), ""), sptField,
                     FxDB(drutama("lusumber"), ""), sptField,
                     FxDB(drutama("luautonotransaksi"), 0), sptField,
                     FxDB(drutama("lunotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("lutgl"), ""), formatTgl), sptField,
                     FxDB(drutama("lukodepa"), 0), sptField,
                     FxDB(drutama("lucustomer"), 0), sptField,
                     FxDB(drutama("lucustomerkontak"), ""), sptField,
                     FxDB(drutama("luuraian"), ""), sptField,
                     FxDB(drutama("lucatatan"), ""), sptField,
                     FxDB(drutama("lunoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("lutglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("lutotaltransaksi"), 0), sptField,
                     FxDB(drutama("luidkj"), 0), sptField,
                     FxDB(drutama("lustatusrealisasi"), 0), sptField,
                     FxDB(drutama("lustatus"), 0), sptField,
                     FxDB(drutama("lustatussebelumnya"), 0), sptField,
                     FxDB(drutama("lujmlrevisi"), 0), sptField,
                     FxDB(drutama("lucetakanke"), 0), sptField,
                     FxDB(drutama("luinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("luinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("lumodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lumodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("luisclose"), 0), sptField,
                     FxDB(drutama("lucustomtext1"), ""), sptField,
                     FxDB(drutama("lucustomtext2"), ""), sptField,
                     FxDB(drutama("lucustomtext3"), ""), sptField,
                     FxDB(drutama("lucustomtext4"), ""), sptField,
                     FxDB(drutama("lucustomtext5"), ""), sptField,
                     FxDB(drutama("lucustomtext6"), ""), sptField,
                     FxDB(drutama("lucustomtext7"), ""), sptField,
                     FxDB(drutama("lucustomtext8"), ""), sptField,
                     FxDB(drutama("lucustomtext9"), ""), sptField,
                     FxDB(drutama("lucustomtext10"), ""), sptField,
                     FxDB(drutama("lucustomtext11"), ""), sptField,
                     FxDB(drutama("lucustomtext12"), ""), sptField,
                     FxDB(drutama("lucustomtext13"), ""), sptField,
                     FxDB(drutama("lucustomtext14"), ""), sptField,
                     FxDB(drutama("lucustomtext15"), ""), sptField,
                     FxDB(drutama("lucustomtext16"), ""), sptField,
                     FxDB(drutama("lucustomtext17"), ""), sptField,
                     FxDB(drutama("lucustomtext18"), ""), sptField,
                     FxDB(drutama("lucustomtext19"), ""), sptField,
                     FxDB(drutama("lucustomtext20"), ""), sptField,
                     FxDB(drutama("lucustomint1"), 0), sptField,
                     FxDB(drutama("lucustomint2"), 0), sptField,
                     FxDB(drutama("lucustomint3"), 0), sptField,
                     FxDB(drutama("lucustomint4"), 0), sptField,
                     FxDB(drutama("lucustomint5"), 0), sptField,
                     FxDB(drutama("lucustomint6"), 0), sptField,
                     FxDB(drutama("lucustomint7"), 0), sptField,
                     FxDB(drutama("lucustomint8"), 0), sptField,
                     FxDB(drutama("lucustomint9"), 0), sptField,
                     FxDB(drutama("lucustomint10"), 0), sptField,
                     FxDB(drutama("lucustomint11"), 0), sptField,
                     FxDB(drutama("lucustomint12"), 0), sptField,
                     FxDB(drutama("lucustomint13"), 0), sptField,
                     FxDB(drutama("lucustomint14"), 0), sptField,
                     FxDB(drutama("lucustomint15"), 0), sptField,
                     FxDB(drutama("lucustomint16"), 0), sptField,
                     FxDB(drutama("lucustomint17"), 0), sptField,
                     FxDB(drutama("lucustomint18"), 0), sptField,
                     FxDB(drutama("lucustomint19"), 0), sptField,
                     FxDB(drutama("lucustomint20"), 0), sptField,
                     FxDB(drutama("lucustomdbl1"), 0), sptField,
                     FxDB(drutama("lucustomdbl2"), 0), sptField,
                     FxDB(drutama("lucustomdbl3"), 0), sptField,
                     FxDB(drutama("lucustomdbl4"), 0), sptField,
                     FxDB(drutama("lucustomdbl5"), 0), sptField,
                     FxDB(drutama("lucustomdbl6"), 0), sptField,
                     FxDB(drutama("lucustomdbl7"), 0), sptField,
                     FxDB(drutama("lucustomdbl8"), 0), sptField,
                     FxDB(drutama("lucustomdbl9"), 0), sptField,
                     FxDB(drutama("lucustomdbl10"), 0), sptField,
                     FxDB(drutama("lucustomdbl11"), 0), sptField,
                     FxDB(drutama("lucustomdbl12"), 0), sptField,
                     FxDB(drutama("lucustomdbl13"), 0), sptField,
                     FxDB(drutama("lucustomdbl14"), 0), sptField,
                     FxDB(drutama("lucustomdbl15"), 0), sptField,
                     FxDB(drutama("lucustomdbl16"), 0), sptField,
                     FxDB(drutama("lucustomdbl17"), 0), sptField,
                     FxDB(drutama("lucustomdbl18"), 0), sptField,
                     FxDB(drutama("lucustomdbl19"), 0), sptField,
                     FxDB(drutama("lucustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("lucabangnama"), ""), sptField,
                     FxDB(drutama("lulokasinama"), ""), sptField,
                     FxDB(drutama("lugudangnama"), ""), sptField,
                     FxDB(drutama("lucustomerkode"), ""), sptField,
                     FxDB(drutama("lucustomernama"), ""), sptField,
                     FxDB(drutama("lunotransaksikj"), ""), sptField,
                     FxDB(drutama("lustatusnama"), ""), sptField,
                     FxDB(drutama("lustatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("luinputusernama"), ""), sptField,
                     FxDB(drutama("lumodifikasiusernama"), ""), sptField,
                     FxDB(drutama("lumatauang"), ""), sptField,
                     FxDB(drutama("lukurs"), 0), sptField,
                     FxDB(drutama("luposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lupostingtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("lutingkatjual"), 0), sptField,
                     FxDB(drutama("luperawatan"), ""), sptField,
                     FxDB(drutama("lukategoripasien"), ""), sptField,
                     FxDB(drutama("lukamar"), ""), sptField,
                     FxDB(drutama("lukategoripasiennama"), ""), sptField,
                     FxDB(drutama("lukamarnama"), ""), sptField,
                     FxDB(drutama("luawalankatpasien"), ""), sptField)

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idludetail"), 0), sptField,
                     FxDB(dr("idlu"), 0), sptField,
                     FxDB(dr("jenis"), ""), sptField,
                     FxDB(dr("idlayanan"), 0), sptField,
                     FxDB(dr("namalayanan"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmltotal"), 0), sptField,
                     FxDB(dr("satuandefault"), ""), sptField,
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
                     FxDB(dr("idkjdetail"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("iddokter"), 0), sptField,
                     FxDB(dr("namadokter"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customtext11"), ""), sptField,
                     FxDB(dr("customtext12"), ""), sptField,
                     FxDB(dr("customtext13"), ""), sptField,
                     FxDB(dr("customtext14"), ""), sptField,
                     FxDB(dr("customtext15"), ""), sptField,
                     FxDB(dr("customtext16"), ""), sptField,
                     FxDB(dr("customtext17"), ""), sptField,
                     FxDB(dr("customtext18"), ""), sptField,
                     FxDB(dr("customtext19"), ""), sptField,
                     FxDB(dr("customtext20"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     FxDB(dr("customdbl11"), 0), sptField,
                     FxDB(dr("customdbl12"), 0), sptField,
                     FxDB(dr("customdbl13"), 0), sptField,
                     FxDB(dr("customdbl14"), 0), sptField,
                     FxDB(dr("customdbl15"), 0), sptField,
                     FxDB(dr("customdbl16"), 0), sptField,
                     FxDB(dr("customdbl17"), 0), sptField,
                     FxDB(dr("customdbl18"), 0), sptField,
                     FxDB(dr("customdbl19"), 0), sptField,
                     FxDB(dr("customdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
                     FxDB(dr("kodelayanan"), ""), sptField,
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
                     FxDB(dr("kjnotransaksi"), ""), sptField,
                     FxDB(dr("kodedokter"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("rekpenjualan"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("luidhistory, luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8, lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13, lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18, lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3, lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8, lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13, lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18, lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8, lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13, lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18, lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8, lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13, lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18, lucustomdate19, lucustomdate20, lucabangnama, lulokasinama, lugudangnama,  lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama, lumatauang, lukurs, luposting, lupostingtgl, lutingkatjual, luperawatan, lukategoripasien, lukamar, lukategoripasiennama, lukamarnama, luawalankatpasien" & sptSubParam & "idhistorydetail, idhistory, idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan"))

        Return wsResult
    End Function

End Class
