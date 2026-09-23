Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_asset_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_Asset_HistorySimpan(ByVal param As String) As String
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
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m7_asset_history(SELECT 0, asset.* FROM m7_asset asset WHERE asset.aid = '" & idtransaksi & "')"
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
    Public Function M7_Asset_HistorySearch(ByVal param As String) As String
        'M7_AssetSearch --------------------------------------------------------
        'aidhistory, aid, akode, anama, akategori, acabang, alokasi, adivisi, 
        'asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, 
        'ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, 
        'atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, 
        'arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, 
        'apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, 
        'amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, 
        'ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, 
        'astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acostcenter, aproyek, ajml, 
        'asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, 
        'ajmlpajak2, acostcenternama, aproyeknama, apajak1nama, apajak1nilai, apajak2nama, apajak2nilai

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
        'sql = "select `a`.`aidhistory` AS `aidhistory`,`a`.`aid` AS `aid`,`a`.`akode` AS `akode`,`a`.`anama` AS `anama`,`a`.`akategori` AS `akategori`,`a`.`acabang` AS `acabang`,`a`.`alokasi` AS `alokasi`,`a`.`adivisi` AS `adivisi`,`a`.`asubdivisi` AS `asubdivisi`,`a`.`acatatan` AS `acatatan`,`a`.`anomor` AS `anomor`,`a`.`atglbeli` AS `atglbeli`,`a`.`atglpakai` AS `atglpakai`,`a`.`amatauang` AS `amatauang`,`a`.`akurs` AS `akurs`,`a`.`ahargabeli` AS `ahargabeli`,`a`.`anilairesidu` AS `anilairesidu`,`a`.`aumurekonomis` AS `aumurekonomis`,`a`.`abebanperbln` AS `abebanperbln`,`a`.`aakumulasibeban` AS `aakumulasibeban`,`a`.`anilaibuku` AS `anilaibuku`,`a`.`ametode` AS `ametode`,`a`.`atabelpenyusutan` AS `atabelpenyusutan`,`a`.`aintangible` AS `aintangible`,`a`.`afiskal` AS `afiskal`,`a`.`aatastengahbulan` AS `aatastengahbulan`,`a`.`arekasset` AS `arekasset`,`a`.`arekakumdepresiasi` AS `arekakumdepresiasi`,`a`.`arekdepresiasi` AS `arekdepresiasi`,`a`.`arekpenghapusan` AS `arekpenghapusan`,`a`.`aprodusen` AS `aprodusen`,`a`.`atglpensiun` AS `atglpensiun`,`a`.`apenyusutanke` AS `apenyusutanke`,`a`.`anilaimenurun` AS `anilaimenurun`,`a`.`adispose` AS `adispose`,`a`.`apembelian` AS `apembelian`,`a`.`apenjualan` AS `apenjualan`,`a`.`alocked` AS `alocked`,`a`.`astatus` AS `astatus`,`a`.`astatussebelumnya` AS `astatussebelumnya`,`a`.`aisclose` AS `aisclose`,`a`.`ainputuser` AS `ainputuser`,`a`.`ainputtgl` AS `ainputtgl`,`a`.`amodifikasiuser` AS `amodifikasiuser`,`a`.`amodifikasitgl` AS `amodifikasitgl`,`ac`.`acnama` AS `akategorinama`,`br`.`bnama` AS `acabangnama`,`l`.`lnama` AS `alokasinama`,`d`.`dnama` AS `adivisinama`,`sd`.`sdnama` AS `asubdivisinama`,`dc`.`nama` AS `ametodenama`,`coa1`.`cnama` AS `arekassetnama`,`coa2`.`cnama` AS `arekakumdepresiasinama`,`coa3`.`cnama` AS `arekdepresiasinama`,`coa4`.`cnama` AS `arekpenghapusannama`,`c1`.`kkode` AS `aprodusenkode`,`c1`.`knama` AS `aprodusennama`,`sp1`.`nama` AS `astatusnama`,`sp2`.`nama` AS `astatussebelumnyanama`,`u1`.`unama` AS `ainputusernama`,`u2`.`unama` AS `amodifikasiusernama` from (((((((((((((((`m7_asset_history` `a` left join `m7_asset_category` `ac` on((`a`.`akategori` = `ac`.`ackode`))) left join `m1_branch` `br` on((`a`.`acabang` = `br`.`bkode`))) left join `m1_location` `l` on((`a`.`alokasi` = `l`.`lkode`))) left join `m1_division` `d` on((`a`.`adivisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`a`.`asubdivisi` = `sd`.`sdkode`))) left join `m7_depreciation_category` `dc` on((`a`.`ametode` = `dc`.`kode`))) left join `m1_coa` `coa1` on((`a`.`arekasset` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`a`.`arekakumdepresiasi` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`a`.`arekdepresiasi` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`a`.`arekpenghapusan` = `coa4`.`cnomor`))) left join `m1_contact` `c1` on((`a`.`aprodusen` = `c1`.`kid`))) left join `m0_status_progress` `sp1` on((`a`.`astatus` = `sp1`.`kode`))) left join `m0_status_progress` `sp2` on((`a`.`astatussebelumnya` = `sp2`.`kode`))) left join `m0_user` `u1` on((`a`.`ainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`a`.`amodifikasiuser` = `u2`.`userid`)))"
        sql = "select `a`.`aidhistory` AS `aidhistory`,`a`.`aid` AS `aid`,`a`.`akode` AS `akode`,`a`.`anama` AS `anama`,`a`.`akategori` AS `akategori`,`a`.`acabang` AS `acabang`,`a`.`alokasi` AS `alokasi`,`a`.`adivisi` AS `adivisi`,`a`.`asubdivisi` AS `asubdivisi`,`a`.`acatatan` AS `acatatan`,`a`.`anomor` AS `anomor`,`a`.`atglbeli` AS `atglbeli`,`a`.`atglpakai` AS `atglpakai`,`a`.`amatauang` AS `amatauang`,`a`.`akurs` AS `akurs`,`a`.`ahargabeli` AS `ahargabeli`,`a`.`anilairesidu` AS `anilairesidu`,`a`.`aumurekonomis` AS `aumurekonomis`,`a`.`abebanperbln` AS `abebanperbln`,`a`.`aakumulasibeban` AS `aakumulasibeban`,`a`.`anilaibuku` AS `anilaibuku`,`a`.`ametode` AS `ametode`,`a`.`atabelpenyusutan` AS `atabelpenyusutan`,`a`.`aintangible` AS `aintangible`,`a`.`afiskal` AS `afiskal`,`a`.`aatastengahbulan` AS `aatastengahbulan`,`a`.`arekasset` AS `arekasset`,`a`.`arekakumdepresiasi` AS `arekakumdepresiasi`,`a`.`arekdepresiasi` AS `arekdepresiasi`,`a`.`arekpenghapusan` AS `arekpenghapusan`,`a`.`aprodusen` AS `aprodusen`,`a`.`atglpensiun` AS `atglpensiun`,`a`.`apenyusutanke` AS `apenyusutanke`,`a`.`anilaimenurun` AS `anilaimenurun`,`a`.`adispose` AS `adispose`,`a`.`apembelian` AS `apembelian`,`a`.`apenjualan` AS `apenjualan`,`a`.`alocked` AS `alocked`,`a`.`astatus` AS `astatus`,`a`.`astatussebelumnya` AS `astatussebelumnya`,`a`.`aisclose` AS `aisclose`,`a`.`ainputuser` AS `ainputuser`,`a`.`ainputtgl` AS `ainputtgl`,`a`.`amodifikasiuser` AS `amodifikasiuser`,`a`.`amodifikasitgl` AS `amodifikasitgl`,`ac`.`acnama` AS `akategorinama`,`br`.`bnama` AS `acabangnama`,`l`.`lnama` AS `alokasinama`,`d`.`dnama` AS `adivisinama`,`sd`.`sdnama` AS `asubdivisinama`,`dc`.`nama` AS `ametodenama`,`coa1`.`cnama` AS `arekassetnama`,`coa2`.`cnama` AS `arekakumdepresiasinama`,`coa3`.`cnama` AS `arekdepresiasinama`,`coa4`.`cnama` AS `arekpenghapusannama`,`c1`.`kkode` AS `aprodusenkode`,`c1`.`knama` AS `aprodusennama`,`sp1`.`nama` AS `astatusnama`,`sp2`.`nama` AS `astatussebelumnyanama`,`u1`.`unama` AS `ainputusernama`,`u2`.`unama` AS `amodifikasiusernama`,`a`.`acostcenter` AS `acostcenter`,`a`.`aproyek` AS `aproyek`,`a`.`ajml` AS `ajml`,`a`.`asatuan` AS `asatuan`,`a`.`aharga` AS `aharga`,`a`.`adiskon` AS `adiskon`,`a`.`ajmldiskon` AS `ajmldiskon`,`a`.`apajak1` AS `apajak1`,`a`.`ajmlpajak1` AS `ajmlpajak1`,`a`.`apajak2` AS `apajak2`,`a`.`ajmlpajak2` AS `ajmlpajak2`,`cc`.`ccnama` AS `acostcenternama`,`p`.`pnama` AS `aproyeknama`,`t1`.`tnama` AS `apajak1nama`,ifnull(`t1`.`tnilai`,0) AS `apajak1nilai`,`t2`.`tnama` AS `apajak2nama`,ifnull(`t2`.`tnilai`,0) AS `apajak2nilai` from (((((((((((((((((((`m7_asset_history` `a` left join `m7_asset_category` `ac` on((`a`.`akategori` = `ac`.`ackode`))) left join `m1_branch` `br` on((`a`.`acabang` = `br`.`bkode`))) left join `m1_location` `l` on((`a`.`alokasi` = `l`.`lkode`))) left join `m1_division` `d` on((`a`.`adivisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`a`.`asubdivisi` = `sd`.`sdkode`))) left join `m7_depreciation_category` `dc` on((`a`.`ametode` = `dc`.`kode`))) left join `m1_coa` `coa1` on((`a`.`arekasset` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`a`.`arekakumdepresiasi` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`a`.`arekdepresiasi` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`a`.`arekpenghapusan` = `coa4`.`cnomor`))) left join `m1_contact` `c1` on((`a`.`aprodusen` = `c1`.`kid`))) left join `m0_status_progress` `sp1` on((`a`.`astatus` = `sp1`.`kode`))) left join `m0_status_progress` `sp2` on((`a`.`astatussebelumnya` = `sp2`.`kode`))) left join `m0_user` `u1` on((`a`.`ainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`a`.`amodifikasiuser` = `u2`.`userid`))) left join `m1_cost_center` `cc` on((`a`.`acostcenter` = `cc`.`cckode`))) left join `m1_project` `p` on((`a`.`aproyek` = `p`.`pkode`))) left join `m1_tax` `t1` on((`a`.`apajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`a`.`apajak2` = `t2`.`tkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M7_Asset_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("aidhistory"), ""), sptField,
                     FxDB(dr("aid"), ""), sptField,
                     FxDB(dr("akode"), ""), sptField,
                     FxDB(dr("anama"), ""), sptField,
                     FxDB(dr("akategori"), ""), sptField,
                     FxDB(dr("acabang"), ""), sptField,
                     FxDB(dr("alokasi"), ""), sptField,
                     FxDB(dr("adivisi"), ""), sptField,
                     FxDB(dr("asubdivisi"), ""), sptField,
                     FxDB(dr("acatatan"), ""), sptField,
                     FxDB(dr("anomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("amatauang"), ""), sptField,
                     FxDB(dr("akurs"), 0), sptField,
                     FxDB(dr("ahargabeli"), 0), sptField,
                     FxDB(dr("anilairesidu"), 0), sptField,
                     FxDB(dr("aumurekonomis"), 0), sptField,
                     FxDB(dr("abebanperbln"), 0), sptField,
                     FxDB(dr("aakumulasibeban"), 0), sptField,
                     FxDB(dr("anilaibuku"), 0), sptField,
                     FxDB(dr("ametode"), 0), sptField,
                     FxDB(dr("atabelpenyusutan"), ""), sptField,
                     FxDB(dr("aintangible"), 0), sptField,
                     FxDB(dr("afiskal"), 0), sptField,
                     FxDB(dr("aatastengahbulan"), 0), sptField,
                     FxDB(dr("arekasset"), ""), sptField,
                     FxDB(dr("arekakumdepresiasi"), ""), sptField,
                     FxDB(dr("arekdepresiasi"), ""), sptField,
                     FxDB(dr("arekpenghapusan"), ""), sptField,
                     FxDB(dr("aprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("apenyusutanke"), 0), sptField,
                     FxDB(dr("anilaimenurun"), 0), sptField,
                     FxDB(dr("adispose"), 0), sptField,
                     FxDB(dr("apembelian"), 0), sptField,
                     FxDB(dr("apenjualan"), 0), sptField,
                     FxDB(dr("alocked"), 0), sptField,
                     FxDB(dr("astatus"), 0), sptField,
                     FxDB(dr("astatussebelumnya"), 0), sptField,
                     FxDB(dr("aisclose"), 0), sptField,
                     FxDB(dr("ainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("amodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("amodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("akategorinama"), ""), sptField,
                     FxDB(dr("acabangnama"), ""), sptField,
                     FxDB(dr("alokasinama"), ""), sptField,
                     FxDB(dr("adivisinama"), ""), sptField,
                     FxDB(dr("asubdivisinama"), ""), sptField,
                     FxDB(dr("ametodenama"), ""), sptField,
                     FxDB(dr("arekassetnama"), ""), sptField,
                     FxDB(dr("arekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("arekdepresiasinama"), ""), sptField,
                     FxDB(dr("arekpenghapusannama"), ""), sptField,
                     FxDB(dr("aprodusenkode"), ""), sptField,
                     FxDB(dr("aprodusennama"), ""), sptField,
                     FxDB(dr("astatusnama"), ""), sptField,
                     FxDB(dr("astatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ainputusernama"), ""), sptField,
                     FxDB(dr("amodifikasiusernama"), ""), sptField,
                     FxDB(dr("acostcenter"), ""), sptField,
                     FxDB(dr("aproyek"), ""), sptField,
                     FxDB(dr("ajml"), 0), sptField,
                     FxDB(dr("asatuan"), ""), sptField,
                     FxDB(dr("aharga"), 0), sptField,
                     FxDB(dr("adiskon"), ""), sptField,
                     FxDB(dr("ajmldiskon"), 0), sptField,
                     FxDB(dr("apajak1"), ""), sptField,
                     FxDB(dr("ajmlpajak1"), 0), sptField,
                     FxDB(dr("apajak2"), ""), sptField,
                     FxDB(dr("ajmlpajak2"), 0), sptField,
                     FxDB(dr("acostcenternama"), ""), sptField,
                     FxDB(dr("aproyeknama"), ""), sptField,
                     FxDB(dr("apajak1nama"), ""), sptField,
                     FxDB(dr("apajak1nilai"), 0), sptField,
                     FxDB(dr("apajak2nama"), ""), sptField,
                     FxDB(dr("apajak2nilai"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Asset data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("aidhistory, aid, akode, anama, akategori, acabang, alokasi, adivisi, asubdivisi, acatatan, anomor, atglbeli, atglpakai, amatauang, akurs, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, akategorinama, acabangnama, alokasinama, adivisinama, asubdivisinama, ametodenama, arekassetnama, arekakumdepresiasinama, arekdepresiasinama, arekpenghapusannama, aprodusenkode, aprodusennama, astatusnama, astatussebelumnyanama, ainputusernama, amodifikasiusernama, acostcenter, aproyek, ajml, asatuan, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2, acostcenternama, aproyeknama, apajak1nama, apajak1nilai, apajak2nama, apajak2nilai"))

        Return wsResult
    End Function

End Class
