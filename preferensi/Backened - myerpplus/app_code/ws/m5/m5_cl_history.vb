Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_cl_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Cl_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO M5_Cl_history(SELECT 0, Cl.* FROM M5_Cl Cl WHERE Cl.Clid = '" & idtransaksi & "')"
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
    Public Function M5_Cl_HistorySearch(ByVal param As String) As String
        'M5_Cl_HistorySearch --------------------------------------------------------
        'clidhistory, clid, clcabang, clcabangnama, cllokasi, cllokasinama, clgudang, clgudangnama, 
        'clasalbarang, clasalbarangkategori, cljenispenjualan, cljenispenjualankategori, clcarabayar, clsumber, clautonotransaksi, 
        'clnotransaksi, cltgl, clkodepa, clcustomer, clcustomerkode, clcustomernama, clcustomerkontak, 
        'cl1alamat1, cl1alamat2, cl1alamat3, cl2alamat1, cl2alamat2, cl2alamat3, clbagianpenjualan, 
        'clbagianpenjualankode, clbagianpenjualannama, clekspedisi, clekspedisinama, cltglkirim, cltermin, clterminnama, 
        'clterminharijatuhtempo, cltgljatuhtempo, cluraian, clcatatan, clnoref, cltglnoref, cltglpenutupan, 
        'clmatauang, clkurs, clhargatermasukpajak, cltotal, cldiskonpersen, cljmldiskon, cltotalpajak1detail, 
        'cltotalpajak2detail, clbiayalainpersen, clbiayalain, cltotaltransaksi, cljmlbayar, clrekdiskon, clrekpajak1, 
        'clrekpajak2, clrekbiayalain, clrekbayar, clidso, sonotransaksi, clstatuspi, clstatuspl, 
        'clstatusdo, clstatusdr, clstatussi, clstatusrnr, clstatussr, clstatusrealisasi, clstatus, 
        'clstatusnama, clstatussebelumnya, cljmlrevisi, clcetakanke, clinputuser, clinputuserkode, clinputusernama, 
        'clinputtgl, clmodifikasiuser, clmodifikasiuserkode, clmodifikasiusernama, clmodifikasitgl, clposting, clpostingtgl, 
        'clisclose, clcustomtext1, clcustomtext2, clcustomtext3, clcustomtext4, clcustomtext5, clcustomint1, 
        'clcustomint2, clcustomint3, clcustomdbl1, clcustomdbl2, clcustomdbl3, clcustomdate1, clcustomdate2, 
        'clcustomdate3, cluploaded, clidsodetail, clidbarang, clkodebarang, clnamabarang, cltipebarang, 
        'cljml, clsatuan, clnilaisatuan, cljmlbarang, clsatuanbarang

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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        sql = "SELECT cl.clidhistory, cl.clid, cl.clcabang, br.bnama as clcabangnama, cl.cllokasi, lc.lnama as cllokasinama, cl.clgudang, wh.wnama as clgudangnama, cl.clasalbarang, cl.clasalbarangkategori, cl.cljenispenjualan, cl.cljenispenjualankategori, cl.clcarabayar, cl.clsumber, cl.clautonotransaksi, cl.clnotransaksi, cl.cltgl, cl.clkodepa, cl.clcustomer, c.kkode as clcustomerkode, c.knama as clcustomernama, cl.clcustomerkontak, cl.cl1alamat1, cl.cl1alamat2, cl.cl1alamat3, cl.cl2alamat1, cl.cl2alamat2, cl.cl2alamat3, cl.clbagianpenjualan, cs.kkode as clbagianpenjualankode, cs.knama as clbagianpenjualannama, cl.clekspedisi, ex.enama as clekspedisinama, cl.cltglkirim, cl.cltermin, tr.trnama as clterminnama, tr.trharijatuhtempo as clterminharijatuhtempo, cl.cltgljatuhtempo, cl.cluraian, cl.clcatatan, cl.clnoref, cl.cltglnoref, cl.cltglpenutupan, cl.clmatauang, cl.clkurs, cl.clhargatermasukpajak, cl.cltotal, cl.cldiskonpersen, cl.cljmldiskon, cl.cltotalpajak1detail, cl.cltotalpajak2detail, cl.clbiayalainpersen, cl.clbiayalain, cl.cltotaltransaksi, cl.cljmlbayar, cl.clrekdiskon, cl.clrekpajak1, cl.clrekpajak2, cl.clrekbiayalain, cl.clrekbayar, cl.clidso, so.sonotransaksi, cl.clstatuspi, cl.clstatuspl, cl.clstatusdo, cl.clstatusdr, cl.clstatussi, cl.clstatusrnr, cl.clstatussr, cl.clstatusrealisasi, cl.clstatus, st.nama as clstatusnama, cl.clstatussebelumnya, cl.cljmlrevisi, cl.clcetakanke, cl.clinputuser, u.ukode as clinputuserkode, u.unama as clinputusernama, cl.clinputtgl, cl.clmodifikasiuser, u2.ukode as clmodifikasiuserkode, u2.ukode as clmodifikasiusernama, cl.clmodifikasitgl, cl.clposting, cl.clpostingtgl, cl.clisclose, cl.clcustomtext1, cl.clcustomtext2, cl.clcustomtext3, cl.clcustomtext4, cl.clcustomtext5, cl.clcustomint1, cl.clcustomint2, cl.clcustomint3, cl.clcustomdbl1, cl.clcustomdbl2, cl.clcustomdbl3, cl.clcustomdate1, cl.clcustomdate2, cl.clcustomdate3, cl.cluploaded, cl.clidsodetail, cl.clidbarang, i.bkode as clkodebarang, cl.clnamabarang, cl.cltipebarang, cl.cljml, cl.clsatuan, cl.clnilaisatuan, cl.cljmlbarang, cl.clsatuanbarang FROM m5_cl_history cl JOIN m1_branch br ON cl.clcabang = br.bkode JOIN m1_location lc ON cl.cllokasi = lc.lkode JOIN m1_warehouse wh ON cl.clgudang = wh.wkode JOIN m1_contact c ON cl.clcustomer = c.kid JOIN m1_contact cs ON cl.clbagianpenjualan = cs.kid JOIN m5_so so ON cl.clidso = so.soid JOIN m1_item i ON cl.clidbarang = i.bid JOIN m0_user u ON cl.clinputuser = u.userid JOIN m0_status st ON cl.clstatus = st.kode LEFT JOIN m1_expedition ex ON cl.clekspedisi = ex.ekode LEFT JOIN m1_terms tr ON cl.cltermin = tr.trkode LEFT JOIN m0_user u2 ON cl.clmodifikasiuser = u2.userid"

        dt = AmbilData("aplikasi1-M5_Cl_v_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("clid"), 0), sptField,
                     FxDB(dr("clidhistory"), ""), sptField,
                     FxDB(dr("clcabang"), ""), sptField,
                     FxDB(dr("clcabangnama"), ""), sptField,
                     FxDB(dr("cllokasi"), ""), sptField,
                     FxDB(dr("cllokasinama"), ""), sptField,
                     FxDB(dr("clgudang"), ""), sptField,
                     FxDB(dr("clgudangnama"), ""), sptField,
                     FxDB(dr("clasalbarang"), ""), sptField,
                     FxDB(dr("clasalbarangkategori"), 0), sptField,
                     FxDB(dr("cljenispenjualan"), ""), sptField,
                     FxDB(dr("cljenispenjualankategori"), 0), sptField,
                     FxDB(dr("clcarabayar"), 0), sptField,
                     FxDB(dr("clsumber"), ""), sptField,
                     FxDB(dr("clautonotransaksi"), 0), sptField,
                     FxDB(dr("clnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cltgl"), ""), formatTgl), sptField,
                     FxDB(dr("clkodepa"), ""), sptField,
                     FxDB(dr("clcustomer"), ""), sptField,
                     FxDB(dr("clcustomerkode"), ""), sptField,
                     FxDB(dr("clcustomernama"), ""), sptField,
                     FxDB(dr("clcustomerkontak"), ""), sptField,
                     FxDB(dr("cl1alamat1"), ""), sptField,
                     FxDB(dr("cl1alamat2"), ""), sptField,
                     FxDB(dr("cl1alamat3"), ""), sptField,
                     FxDB(dr("cl2alamat1"), ""), sptField,
                     FxDB(dr("cl2alamat2"), ""), sptField,
                     FxDB(dr("cl2alamat3"), ""), sptField,
                     FxDB(dr("clbagianpenjualan"), ""), sptField,
                     FxDB(dr("clbagianpenjualankode"), ""), sptField,
                     FxDB(dr("clbagianpenjualannama"), ""), sptField,
                     FxDB(dr("clekspedisi"), ""), sptField,
                     FxDB(dr("clekspedisinama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cltglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("cltermin"), ""), sptField,
                     FxDB(dr("clterminnama"), ""), sptField,
                     FxDB(dr("clterminharijatuhtempo"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("cluraian"), ""), sptField,
                     FxDB(dr("clcatatan"), ""), sptField,
                     FxDB(dr("clnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("cltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("clmatauang"), ""), sptField,
                     FxDB(dr("clkurs"), 0), sptField,
                     FxDB(dr("clhargatermasukpajak"), 0), sptField,
                     FxDB(dr("cltotal"), 0), sptField,
                     FxDB(dr("cldiskonpersen"), ""), sptField,
                     FxDB(dr("cljmldiskon"), 0), sptField,
                     FxDB(dr("cltotalpajak1detail"), 0), sptField,
                     FxDB(dr("cltotalpajak2detail"), 0), sptField,
                     FxDB(dr("clbiayalainpersen"), ""), sptField,
                     FxDB(dr("clbiayalain"), 0), sptField,
                     FxDB(dr("cltotaltransaksi"), 0), sptField,
                     FxDB(dr("cljmlbayar"), 0), sptField,
                     FxDB(dr("clrekdiskon"), ""), sptField,
                     FxDB(dr("clrekpajak1"), ""), sptField,
                     FxDB(dr("clrekpajak2"), ""), sptField,
                     FxDB(dr("clrekbiayalain"), ""), sptField,
                     FxDB(dr("clrekbayar"), ""), sptField,
                     FxDB(dr("clidso"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("clstatuspi"), 0), sptField,
                     FxDB(dr("clstatuspl"), 0), sptField,
                     FxDB(dr("clstatusdo"), 0), sptField,
                     FxDB(dr("clstatusdr"), 0), sptField,
                     FxDB(dr("clstatussi"), 0), sptField,
                     FxDB(dr("clstatusrnr"), 0), sptField,
                     FxDB(dr("clstatussr"), 0), sptField,
                     FxDB(dr("clstatusrealisasi"), 0), sptField,
                     FxDB(dr("clstatus"), 0), sptField,
                     FxDB(dr("clstatusnama"), ""), sptField,
                     FxDB(dr("clstatussebelumnya"), 0), sptField,
                     FxDB(dr("cljmlrevisi"), 0), sptField,
                     FxDB(dr("clcetakanke"), 0), sptField,
                     FxDB(dr("clinputuser"), ""), sptField,
                     FxDB(dr("clinputuserkode"), ""), sptField,
                     FxDB(dr("clinputusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("clinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("clmodifikasiuser"), ""), sptField,
                     FxDB(dr("clmodifikasiuserkode"), ""), sptField,
                     FxDB(dr("clmodifikasiusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("clmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("clposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("clpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("clisclose"), 0), sptField,
                     FxDB(dr("clcustomtext1"), ""), sptField,
                     FxDB(dr("clcustomtext2"), ""), sptField,
                     FxDB(dr("clcustomtext3"), ""), sptField,
                     FxDB(dr("clcustomtext4"), ""), sptField,
                     FxDB(dr("clcustomtext5"), ""), sptField,
                     FxDB(dr("clcustomint1"), 0), sptField,
                     FxDB(dr("clcustomint2"), 0), sptField,
                     FxDB(dr("clcustomint3"), 0), sptField,
                     FxDB(dr("clcustomdbl1"), 0), sptField,
                     FxDB(dr("clcustomdbl2"), 0), sptField,
                     FxDB(dr("clcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("clcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("clcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("clcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("cluploaded"), 0), sptField,
                     FxDB(dr("clidsodetail"), ""), sptField,
                     FxDB(dr("clidbarang"), ""), sptField,
                     FxDB(dr("clkodebarang"), ""), sptField,
                     FxDB(dr("clnamabarang"), ""), sptField,
                     FxDB(dr("cltipebarang"), ""), sptField,
                     FxDB(dr("cljml"), 0), sptField,
                     FxDB(dr("clsatuan"), ""), sptField,
                     FxDB(dr("clnilaisatuan"), 0), sptField,
                     FxDB(dr("cljmlbarang"), 0), sptField,
                     FxDB(dr("clsatuanbarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("clidhistory, clid, clcabang, clcabangnama, cllokasi, cllokasinama, clgudang, clgudangnama, clasalbarang, clasalbarangkategori, cljenispenjualan, cljenispenjualankategori, clcarabayar, clsumber, clautonotransaksi, clnotransaksi, cltgl, clkodepa, clcustomer, clcustomerkode, clcustomernama, clcustomerkontak, cl1alamat1, cl1alamat2, cl1alamat3, cl2alamat1, cl2alamat2, cl2alamat3, clbagianpenjualan, clbagianpenjualankode, clbagianpenjualannama, clekspedisi, clekspedisinama, cltglkirim, cltermin, clterminnama, clterminharijatuhtempo, cltgljatuhtempo, cluraian, clcatatan, clnoref, cltglnoref, cltglpenutupan, clmatauang, clkurs, clhargatermasukpajak, cltotal, cldiskonpersen, cljmldiskon, cltotalpajak1detail, cltotalpajak2detail, clbiayalainpersen, clbiayalain, cltotaltransaksi, cljmlbayar, clrekdiskon, clrekpajak1, clrekpajak2, clrekbiayalain, clrekbayar, clidso, sonotransaksi, clstatuspi, clstatuspl, clstatusdo, clstatusdr, clstatussi, clstatusrnr, clstatussr, clstatusrealisasi, clstatus, clstatusnama, clstatussebelumnya, cljmlrevisi, clcetakanke, clinputuser, clinputuserkode, clinputusernama, clinputtgl, clmodifikasiuser, clmodifikasiuserkode, clmodifikasiusernama, clmodifikasitgl, clposting, clpostingtgl, clisclose, clcustomtext1, clcustomtext2, clcustomtext3, clcustomtext4, clcustomtext5, clcustomint1, clcustomint2, clcustomint3, clcustomdbl1, clcustomdbl2, clcustomdbl3, clcustomdate1, clcustomdate2, clcustomdate3, cluploaded, clidsodetail, clidbarang, clkodebarang, clnamabarang, cltipebarang, cljml, clsatuan, clnilaisatuan, cljmlbarang, clsatuanbarang"))

        Return wsResult
    End Function


End Class
