Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class m8_content_detail
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

#Region "M3"

    <WebMethod()>
    Public Function M8_Sa_DetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "select sa.said AS said,sa.sacabang AS sacabang,sa.salokasi AS salokasi,sa.sagudang AS sagudang,sa.sasumber AS sasumber,sa.sajenis AS sajenis,sa.saautonotransaksi AS saautonotransaksi,sa.sanotransaksi AS sanotransaksi,sa.satgl AS satgl,sa.sakodepa AS sakodepa,sa.sabagiansa AS sabagiansa,sa.sabagiansakontak AS sabagiansakontak,sa.sauraian AS sauraian,sa.sacatatan AS sacatatan,sa.sanoref AS sanoref,sa.satglnoref AS satglnoref,sa.saidsp AS saidsp,sa.sastatus AS sastatus,sa.sastatussebelumnya AS sastatussebelumnya,sa.sajmlrevisi AS sajmlrevisi,sa.sacetakanke AS sacetakanke,sa.sainputuser AS sainputuser,sa.sainputtgl AS sainputtgl,sa.samodifikasiuser AS samodifikasiuser,sa.samodifikasitgl AS samodifikasitgl,sa.saposting AS saposting,sa.sapostingtgl AS sapostingtgl,sa.satutupperiode AS satutupperiode,sa.saisclose AS saisclose,br.bnama AS sacabangnama,lc.lnama AS salokasinama,wh.wnama AS sagudangnama,tsa.tsanama AS sajenisnama,c1.kkode AS sabagiansakode,c1.knama AS sabagiansanama,sp.spnotransaksi AS sanotransaksisp,st1.nama AS sastatusnama,st2.nama AS sastatussebelumnyanama,u1.unama AS sainputusernama,u2.unama AS samodifikasiusernama, i.bkode as kodebarang, sad.namabarang, sad.jmlmasuk, sad.jmlkeluar, sad.satuan, sad.hpp, (sad.jmlmasuk-sad.jmlkeluar)*sad.hpp AS total from ((((((((((m3_sa sa left join m1_branch br on((br.bkode = sa.sacabang))) left join m1_location lc on((lc.lkode = sa.salokasi))) left join m1_warehouse wh on((wh.wkode = sa.sagudang))) left join m1_type_sa tsa on((tsa.tsakode = sa.sajenis))) left join m1_contact c1 on((c1.kid = sa.sabagiansa))) left join m3_sp sp on((sa.saidsp = sp.spid))) left join m0_status st1 on((st1.kode = sa.sastatus))) left join m0_status st2 on((st2.kode = sa.sastatussebelumnya))) left join m0_user u1 on((u1.userid = sa.sainputuser))) left join m0_user u2 on((u2.userid = sa.samodifikasiuser))) JOIN m3_sa_detail sad ON sad.idsa = sa.said JOIN m1_item i ON i.bid = sad.idbarang "

        dt = AmbilData("aplikasi1-m3_sa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("said"), 0), sptField,
                     FxDB(dr("sacabang"), ""), sptField,
                     FxDB(dr("salokasi"), ""), sptField,
                     FxDB(dr("sagudang"), ""), sptField,
                     FxDB(dr("sasumber"), ""), sptField,
                     FxDB(dr("sajenis"), ""), sptField,
                     FxDB(dr("saautonotransaksi"), 0), sptField,
                     FxDB(dr("sanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("satgl"), ""), formatTgl), sptField,
                     FxDB(dr("sakodepa"), 0), sptField,
                     FxDB(dr("sabagiansa"), 0), sptField,
                     FxDB(dr("sabagiansakontak"), ""), sptField,
                     FxDB(dr("sauraian"), ""), sptField,
                     FxDB(dr("sacatatan"), ""), sptField,
                     FxDB(dr("sanoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("satglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("saidsp"), 0), sptField,
                     FxDB(dr("sastatus"), 0), sptField,
                     FxDB(dr("sastatussebelumnya"), 0), sptField,
                     FxDB(dr("sajmlrevisi"), 0), sptField,
                     FxDB(dr("sacetakanke"), 0), sptField,
                     FxDB(dr("sainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("samodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("samodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("saposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("satutupperiode"), 0), sptField,
                     FxDB(dr("saisclose"), 0), sptField,
                     FxDB(dr("sacabangnama"), ""), sptField,
                     FxDB(dr("salokasinama"), ""), sptField,
                     FxDB(dr("sagudangnama"), ""), sptField,
                     FxDB(dr("sajenisnama"), ""), sptField,
                     FxDB(dr("sabagiansakode"), ""), sptField,
                     FxDB(dr("sabagiansanama"), ""), sptField,
                     FxDB(dr("sanotransaksisp"), ""), sptField,
                     FxDB(dr("sastatusnama"), ""), sptField,
                     FxDB(dr("sastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sainputusernama"), ""), sptField,
                     FxDB(dr("samodifikasiusernama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("jmlmasuk"), 0), sptField,
                     FxDB(dr("jmlkeluar"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("total"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, sapostingtgl, satutupperiode, saisclose, sacabangnama, salokasinama, sagudangnama, sajenisnama, sabagiansakode, sabagiansanama, sanotransaksisp, sastatusnama, sastatussebelumnyanama, sainputusernama, samodifikasiusernama, kodebarang, namabarang, jmlmasuk, jmlkeluar, satuan, hpp, total"))

        Return wsResult
    End Function

#End Region

#Region "M4"

    <WebMethod()>
    Public Function M8_PemenuhanPrDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(prid) AS jml FROM m4_pr"
        filterr = "prstatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m4_pr", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(prid) AS jml FROM m4_pr"
        filtera = "prstatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_pr", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim pemenuhanPR As Double = 0
        If (jml <> 0) Then
            pemenuhanPR = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(pemenuhanPR, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_PemenuhanPoDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(poid) AS jml FROM m4_po"
        filterr = "postatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m4_po", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(poid) AS jml FROM m4_po"
        filtera = "postatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_po", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim pemenuhanPR As Double = 0
        If (jml <> 0) Then
            pemenuhanPR = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(pemenuhanPR, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_KecepatanPoDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(po.poid) AS jml FROM m4_po po JOIN m4_po_detail pod ON po.poid = pod.idpo JOIN m4_pr_detail prd ON prd.idprdetail = pod.idprdetail JOIN m4_pr pr ON pr.prid = prd.idpr"
        filterr = "po.postatus IN (2,3,4,7) AND po.potgl <= pr.prtgldipakai AND " + Filter
        dtr = AmbilData("aplikasi1-m4_po", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml po yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(poid) AS jml FROM m4_po po JOIN m4_po_detail pod ON po.poid = pod.idpo JOIN m4_pr_detail prd ON prd.idprdetail = pod.idprdetail JOIN m4_pr pr ON pr.prid = prd.idpr"
        filtera = "postatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_po", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_PemenuhanGrnDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(grnid) AS jml FROM m4_grn"
        filterr = "grnstatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m4_grn", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(grnid) AS jml FROM m4_grn"
        filtera = "grnstatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_grn", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_KecepatanGrnDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(grnid) AS jml FROM m4_grn grn JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn JOIN m4_po_detail pod ON pod.idpodetail = grnd.idpodetail JOIN m4_po po ON po.poid = pod.idpo"
        filterr = "grn.grnstatus (2,3,4,7) And grntgl <= potgldipenuhi AND " + Filter
        dtr = AmbilData("aplikasi1-m4_po", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml po yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(grnid) AS jml FROM m4_grn grn JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn JOIN m4_po_detail pod ON pod.idpodetail = grnd.idpodetail JOIN m4_po po ON po.poid = pod.idpo"
        filtera = "grn.grnstatus In (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_po", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_PemenuhanRiDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(riid) AS jml FROM m4_ri"
        filterr = "ristatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m4_ri", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(riid) AS jml FROM m4_ri"
        filtera = "ristatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_ri", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_KecepatanVpDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(vpid) AS jml FROM m4_vp vp JOIN m4_vp_detail vpd ON vp.vpid = vpd.idvp JOIN m4_ri ri ON ri.riid = vpd.idtransaksi AND vpd.sumber = 'RI'"
        filterr = "vp.vpstatus IN (2,3,4,7) And vptgl <= ritgljatuhtempo AND " + Filter
        dtr = AmbilData("aplikasi1-m4_vp", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml VP yg menggunakan RI
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(vpid) AS jml FROM m4_vp vp JOIN m4_vp_detail vpd ON vp.vpid = vpd.idvp JOIN m4_ri ri ON ri.riid = vpd.idtransaksi AND vpd.sumber = 'RI'"
        filtera = "vp.vpstatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m4_vp", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function


#End Region

#Region "M5"

    <WebMethod()>
    Public Function M8_PemenuhanSoDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(soid) AS jml FROM m5_so"
        filterr = "sostatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m5_so", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(soid) AS jml FROM m5_so"
        filtera = "sostatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m5_so", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_PemenuhanDoDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(doid) AS jml FROM m5_do"
        filterr = "dostatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m5_do", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(doid) AS jml FROM m5_do"
        filtera = "dostatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m5_do", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_KecepatanDoDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(do.doid) AS jml FROM m5_do do JOIN m5_do_detail dod ON do.doid = dod.iddo JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail JOIN m5_so so ON so.soid = sod.idso"
        filterr = "do.dostatus IN (2,3,4,7) AND do.dotgl <= so.sotglkirim AND " + Filter
        dtr = AmbilData("aplikasi1-m5_do", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml do yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(doid) AS jml FROM m5_do do JOIN m5_do_detail dod ON do.doid = dod.iddo JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail JOIN m5_so so ON so.soid = sod.idso"
        filtera = "dostatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m5_do", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_PemenuhanSiDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(siid) AS jml FROM m5_si"
        filterr = "sistatus IN (4) AND " + Filter
        dtr = AmbilData("aplikasi1-m5_si", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(siid) AS jml FROM m5_si"
        filtera = "sistatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m5_si", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_KecepatanSiDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(si.siid) AS jml FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi JOIN m5_do_detail dod ON dod.iddodetail = sid.iddodetail JOIN m5_do do ON do.doid = dod.iddo"
        filterr = "si.sistatus IN (2,3,4,7) AND si.sitgl <= do.dotglkirim AND " + Filter
        dtr = AmbilData("aplikasi1-m5_si", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml si yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(siid) AS jml FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi JOIN m5_do_detail dod ON dod.iddodetail = sid.iddodetail JOIN m5_do do ON do.doid = dod.iddo"
        filtera = "sistatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m5_si", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M8_KecepatanPvDetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(pvid) AS jml FROM m5_pv pv JOIN m5_pv_detail pvd ON pv.pvid = pvd.idpv JOIN m5_si si ON si.siid = pvd.idtransaksi AND pvd.sumber = 'SI'"
        filterr = "pv.pvstatus IN (2,3,4,7) And pvtgl <= sitgljatuhtempo AND " + Filter
        dtr = AmbilData("aplikasi1-m5_pv", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml PV yg menggunakan SI
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(pvid) AS jml FROM m5_pv pv JOIN m5_pv_detail pvd ON pv.pvid = pvd.idpv JOIN m5_si si ON si.siid = pvd.idtransaksi AND pvd.sumber = 'SI'"
        filtera = "pv.pvstatus IN (2,3,4,7) AND " + Filter
        dta = AmbilData("aplikasi1-m5_pv", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        Dim nilai As Double = 0
        If (jml <> 0) Then
            nilai = (jmlrealisasi / jml) * 100
        End If

        search = String.Concat(search,
                     FxDB("", ""), sptField,
                     FxDB(jmlrealisasi, 0), sptField,
                     FxDB(jml, 0), sptField,
                     FxDB(nilai, 0), sptRow)
        search = search.Substring(0, search.Length - sptRow.Length)

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("periode, jmlrealisasi, jml, jmlpemenuhan"))
        Return wsResult
    End Function


#End Region

#Region "M6"

    <WebMethod()>
    Public Function M8_Pd_DetailSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

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

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT pd.pdnotransaksi, pd.pdtgl, pi.namabarang, pi.jml, s.nama AS pdstatusnama FROM m6_pd pd JOIN m6_pd_in pi ON pd.pdid = pi.idpd JOIN m0_status s ON s.kode = pd.pdstatus "

        dt = AmbilData("aplikasi1-m6_pd", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdtgl"), ""), formatTgl), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("pdstatusnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, sapostingtgl, satutupperiode, saisclose, sacabangnama, salokasinama, sagudangnama, sajenisnama, sabagiansakode, sabagiansanama, sanotransaksisp, sastatusnama, sastatussebelumnyanama, sainputusernama, samodifikasiusernama, kodebarang, namabarang, jmlmasuk, jmlkeluar, satuan, hpp, total"))

        Return wsResult
    End Function

#End Region

End Class