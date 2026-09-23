Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
Imports System.Messaging
Imports System.Data.OleDb
Imports System.IO
Imports System
Imports System.Diagnostics
Imports System.ComponentModel
Imports System.Management
Imports MySql.Data.MySqlClient
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_upload
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_SiCreateFile(ByVal param As String) As String


ProsesAwal:

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sqlUtama As String = "" : Dim sql As String = "" : Dim sqlNotransaksi As String = ""
        Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & ""
        Dim fileName As String = "", folderGlobal As String = ""
        Dim SqlName As String = ""

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
        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
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



        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'CREATE SQL FILE =====================================

        myPath = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan/"
        'myPath = "D:UploadPenjualan/"
        'result(2) = SqlName : GoTo selesai
        SqlName = dataSplit(0)
        contents = dataSplit(1)
        'CEK FILE EXISTS
        Try
            'File.Delete(myPath & SqlName)
            File.WriteAllText(myPath & SqlName, contents)
            result(1) = 1
            result(2) = SqlName
        Catch ex As Exception
            result(2) = ex.Message
            contents = "" : GoTo selesai
        End Try
        'END OF CREATE SQL FILE ==============================




        'myconn.Close()
        'myconn = Nothing
        'END OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then
                result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
            End If



        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_ExecuteDbFileAuto(ByVal param As String) As String
        'M0_ExecuteDbFile --------------------------------------------------------
        'namaFile

        'On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan\"

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




        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        filename = paramSplit(5)
        If Len(filename) < 1 Then
            result(2) = "Filename can't be empty." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'EXECUTE SQL FILE
        Dim arrExecute() As String = F_ExecuteSQL2(paramSplit(0), userid, myPath + filename, Application("As_ConStr1"))
        System.Threading.Thread.Sleep(20000)
        If arrExecute(0) = 0 Then
            result(2) = arrExecute(1) : GoTo selesai
        Else
            result(1) = 1
        End If
selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_DeleteSIPenampungNew(ByVal param As String) As String

        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim Filter As String = ""

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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

        'Filter
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""

        Try
           
            Sql = "delete sid from `m0_si_detail` sid join m0_si si on si.siid = sid.idsi AND si.silokasi = '" & Filter & "'"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()

            Sql = "delete sid from `m0_si_pay` sid join m0_si si on si.siid = sid.idsi AND si.silokasi = '" & Filter & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()

            Sql = "delete si from `m0_si` si where si.silokasi = '" & Filter & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()

        Catch ex As Exception
            result(1) = 0
            Trans.Rollback()
            result(2) = ex.Message : GoTo selesai
        End Try



        Trans.Commit()  '*** Commit Transaction ***'
        result(1) = 1
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function
    'upload penjualan 1 fungsi
    <WebMethod()>
    Public Function M12_SiUploadData(ByVal param As String) As String

        'M5_SiGetUpload --------------------------------------------------------
        'siid
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""
        Dim sqlFile As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", IdUpload As String = ""
        Dim dt As New DataTable, dtdetail As New DataTable, dtpay As New DataTable, dtdate As New DataTable
        Dim siid As String = ""

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

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
            IdUpload = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)
        Dim stepke As Integer = 0
        Try
            'insert si utama
            stepke = 1
            sql = "INSERT INTO m5_si(SELECT 0 as siid,si.sicabang,si.silokasi,si.sigudang,si.siasalbarang,si.siasalbarangkategori,si.sijenispenjualan,si.sijenispenjualankategori,si.sisaldoawal,si.sicarabayar,si.sisumber,si.siautonotransaksi,CONCAT(si.sinotransaksi,'-T') as sinotransaksi,si.sitgl,si.sikodepa,si.sicustomer,si.sicustomerkontak,si.si1alamat1,si.si1alamat2,si.si1alamat3,si.si2alamat1,si.si2alamat2,si.si2alamat3,si.sibagianpenjualan,si.siekspedisi,si.sitglkirim,si.sitermin,si.sitgljatuhtempo,si.siuraian,si.sicatatan,CONCAT('(',si.sinotransaksi,')') as sinoref,si.sitglnoref,si.sitglpenutupan,si.simatauang,si.sikurs,si.sihargatermasukpajak,si.sitotal,si.sidiskonpersen,si.sijmldiskon,si.sitotalpajak1detail,si.sitotalpajak2detail,si.sibiayalainpersen,si.sibiayalain,si.sitotaltransaksi,si.sijmluangmuka,si.sijmlbayar,si.sibayartunai,si.sibayarkkredit,si.sibayarkdebit,si.sibayarvoucher,si.sibayarpoin,si.sibayarjmlpoin,si.sichargepersen,si.sicharge,si.sijmlkembali,si.sipoinsebelumnya,si.sipoindidapat,si.sistatuslunas,si.sitgllunas,si.sinofakturpajak,si.sisdhbayarpajak,si.sitglbayarpajak,si.sirekdiskon,si.sirekpajak1,si.sirekpajak2,si.sirekbiayalain,si.sirekuangmuka,si.sirekbayar,si.sirekcharge,si.sirekkembali,si.siidsq,si.siidso,si.siidas,si.siidpi,si.siidpl,si.siiddo,si.siiddr,si.sistatusrnr,si.sistatussr,si.sistatusrealisasi,si.sistatussie,si.sitglsie,si.sistatus,si.sistatussebelumnya,si.sijmlrevisi,si.sicetakanke,si.siinputuser,si.siinputtgl,si.simodifikasiuser,si.simodifikasitgl,si.siposting,si.sipostingtgl,si.situtupperiode,si.siisclose,si.siuploaded,si.sicustomarea,si.sicustomtext1,si.sicustomtext2,si.sicustomtext3,si.sicustomtext4,si.sicustomtext5,si.sicustomtext6,si.sicustomtext7,si.sicustomtext8,si.sicustomtext9,si.sicustomtext10,si.sicustomint1,si.sicustomint2,si.sicustomint3,si.sicustomint4,si.sicustomint5,si.sicustomint6,si.siid as sicustomint7,si.sicustomint8,si.sicustomint9,si.sicustomint10,si.sicustomdbl1,si.sicustomdbl2,si.sicustomdbl3,si.sicustomdbl4,si.sicustomdbl5,si.sicustomdbl6,si.sicustomdbl7,si.sicustomdbl8,si.sicustomdbl9,si.sicustomdbl10,si.sicustomdate1,si.sicustomdate2,si.sicustomdate3,si.sicustomdate4,si.sicustomdate5,si.sicustomdate6,si.sicustomdate7,si.sicustomdate8,si.sicustomdate9,si.sicustomdate10 FROM `m0_si` si where si.siid in " & Filter & " ORDER BY si.sitgl, si.siid)"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            System.Threading.Thread.Sleep(10000)

            'update poin
            stepke = 2
            sql = "INSERT INTO m1_contact_point (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m0_si si WHERE si.siid in " & Filter & " GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'insert tabel si detail
            System.Threading.Thread.Sleep(10000)
            stepke = 3
            'Sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.customtext3 = si.sinotransaksi AND sid.idupload = si.siidupload)"
            'sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON si.sicustomint7 = sid.idsi AND si.sinotransaksi = sid.customtext3)"
            sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON si.sicustomint7 = sid.idsi where sid.idsi in " & Filter & " AND sid.idupload = '" & IdUpload & "')"
            'result(2) = sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'insert tabel si pay
            System.Threading.Thread.Sleep(10000)
            stepke = 4
            'Sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi AND sid.idupload = si.siidupload)"
            'sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi)"
            sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 join m0_si si0 on si0.siid = sid.idsi where sid.idsi in " & Filter & ")"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'update Voucher
            System.Threading.Thread.Sleep(10000)
            stepke = 5
            sql = "update m_12_pos_voucher_in vi join m0_si_pay sid on vi.vikode = sid.noacbank AND sid.carabayar = 6 SET vi.vijmlbayar = sid.jumlah, vijmlbayarvalas = sid.jumlahvalas"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            ''insert voucher out
            'sql = "INSERT INTO m_12_pos_voucher_out (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m0_si si WHERE si.siid in " & Filter & " GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'hapus tabel si utama penampung
            System.Threading.Thread.Sleep(10000)
            
            stepke = 9
            sql = "INSERT into m1_item_transaction(SELECT 0 as id,si.sicabang as cabang,si.silokasi as lokasi,si.sigudang as gudang,si.sikodepa as kodepa,0 as jenismutasi,si.sisumber as sumber,si.siid as idutama,sid.idsidetail as iddetail,si.sinotransaksi as notransaksi,si.sitgl as tgl,si.sicustomer as kontak,sid.idbarang as idbarang,sid.namabarang as namabarang,sid.tipebarang as tipebarang,i.bhpp as tipehpp,sid.jml as jml,sid.satuan as satuan,sid.jmlbarang as jmlbarang,sid.satuanbarang as satuanbarang,si.simatauang as matauang,si.sikurs as kurs,sid.harga as harga,sid.diskon as diskon,sid.jmldiskon as jmldiskon,0 as idhppikm,0 as idhppikk,0 as sidhppfifo,(CASE si.sihargatermasukpajak WHEN 0 THEN ((sid.jml * sid.harga) - sid.jmldiskon) / sid.jml ELSE ((sid.jml * sid.harga) - sid.jmldiskon - sid.jmlpajak1) / sid.jml END) as hpp,si.siuraian as uraian,si.sicatatan as catatan,sid.catatan as catatandetail,sid.costcenter as costcenter,sid.divisi as divisi,sid.subdivisi as subdivisi,sid.proyek as proyek,0 as saldojml,0 as saldohpp,0 as saldonilai,si.siinputtgl as inputtgl,si.siinputuser as inputuser,si.sipostingtgl as postingtgl,0 as updatehpp,1 as postinghpp,0 as hppfix,0 as postingjurnal,0 as jurnalfix,0 as tutupperiode,0 as isclose,'' as customtext1,'' as customtext2,'' as customtext3,'' as customtext4,'' as customtext5,'' as customtext6,'' as customtext7,'' as customtext8,'' as customtext9,'' as customtext10,'0' as customint1,'0' as customint2,'0' as customint3,'0' as customint4,'0' as customint5,'0' as customint6,'0' as customint7,'0' as customint8,'0' as customint9,'0' as customint10,'0' as customdbl1,'0' as customdbl2,'0' as customdbl3,'0' as customdbl4,'0' as customdbl5,'0' as customdbl6,'0' as customdbl7,'0' as customdbl8,'0' as customdbl9,'0' as customdbl10,'1900-01-01' as customdate1,'1900-01-01' as customdate2,'1900-01-01' as customdate3,'1900-01-01' as customdate4,'1900-01-01' as customdate5,'1900-01-01' as customdate6,'1900-01-01' as customdate7,'1900-01-01' as customdate8,'1900-01-01' as customdate9,'1900-01-01' as customdate10 FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi AND si.sicustomint7 in " & Filter & " join m1_item i ON i.bid = sid.idbarang)"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'hitung stok per gudang
            System.Threading.Thread.Sleep(10000)
            stepke = 10
            sql = "INSERT INTO m1_item_stock_warehouse(select sid.idbarang, si.sigudang, SUM(sid.jmlbarang * -1) as stok from m5_si_detail sid JOIN m5_si si on si.siid = sid.idsi AND si.sicustomint7 in " & Filter & " GROUP BY si.sigudang, sid.idbarang) ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'update stok global
            System.Threading.Thread.Sleep(10000)
            stepke = 11
            sql = "UPDATE m1_item SET bstok = 0; UPDATE m1_item i JOIN (SELECT isw.idbarang, ROUND(SUM(isw.stok),5) as totalstok FROM m1_item_stock_warehouse isw GROUP BY isw.idbarang ) as sp ON i.bid = sp.idbarang SET i.bstok = sp.totalstok WHERE i.bstok <> sp.totalstok;"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT MSMQ JURNAL =================================================================
            stepke = 12
            Dim sumber As String = "SI_POS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
            'BUAT ID UNIQUE
            mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

            'MSMQ TABEL
            sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                & mjid & "', '" & sumber & "', '0', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '0')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'MSMQ ANTRIAN
            stepke = 13
            Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
            If PostingJurnal.Equals("0") = False Then
                hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                If Len(hasilMsmq) > 0 Then
                    result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF INSERT MSMQ JURNAL ==========================================================

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1

            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***' 

            result(1) = 0
            result(2) = ex.Message & "" & stepke.ToString : GoTo selesai
            result(3) = 0
            result(4) = Filter
        End Try
        objCmd = Nothing

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, sptParam)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_SITakeData(ByVal param As String) As String

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", SFilterSplit() As String = {}, SFilter As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable


        Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
        Dim utama As String = "", detail As String = "", pay As String = "", siid As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
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

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT si.siid, si.sicabang, si.silokasi, si.sigudang, si.siasalbarang, si.siasalbarangkategori, si.sijenispenjualan, si.sijenispenjualankategori, si.sisaldoawal, si.sicarabayar, si.sisumber, si.siautonotransaksi, si.sinotransaksi, si.sitgl, si.sikodepa, si.sicustomer, si.sicustomerkontak, si.si1alamat1, si.si1alamat2, si.si1alamat3, si.si2alamat1, si.si2alamat2, si.si2alamat3, si.sibagianpenjualan, si.siekspedisi, si.sitglkirim, si.sitermin, si.sitgljatuhtempo, si.siuraian, si.sicatatan, CONCAT('(',si.sinotransaksi,')')sinoref, si.sitglnoref, si.sitglpenutupan, si.simatauang, si.sikurs, si.sihargatermasukpajak, si.sitotal, si.sidiskonpersen, si.sijmldiskon, si.sitotalpajak1detail, si.sitotalpajak2detail, si.sibiayalainpersen, si.sibiayalain, si.sitotaltransaksi, si.sijmluangmuka, si.sijmlbayar, si.sibayartunai, si.sibayarkkredit, si.sibayarkdebit, si.sibayarvoucher, si.sibayarpoin, si.sibayarjmlpoin, si.sichargepersen, si.sicharge, si.sijmlkembali, si.sipoinsebelumnya, si.sipoindidapat, si.sistatuslunas, si.sitgllunas, si.sinofakturpajak, si.sisdhbayarpajak, si.sitglbayarpajak, si.sirekdiskon, si.sirekpajak1, si.sirekpajak2, si.sirekbiayalain, si.sirekuangmuka, si.sirekbayar, si.sirekcharge, si.sirekkembali, si.siidsq, si.siidso, si.siidas, si.siidpi, si.siidpl, si.siiddo, si.siiddr, si.sistatusrnr, si.sistatussr, si.sistatusrealisasi, 0 as sistatussie, '1900-01-01' as sitglsie, si.sistatus, si.sistatussebelumnya, si.sijmlrevisi, si.sicetakanke, si.siinputuser, si.siinputtgl, si.simodifikasiuser, si.simodifikasitgl, si.siposting, si.sipostingtgl, si.situtupperiode, si.siisclose, si.siuploaded, si.sicustomarea, si.sicustomtext1, si.sicustomtext2, si.sicustomtext3, si.sicustomtext4, si.sicustomtext5, si.sicustomtext6, si.sicustomtext7, si.sicustomtext8, si.sicustomtext9, si.sicustomtext10, si.sicustomint1, si.sicustomint2, si.sicustomint3, si.sicustomint4, si.sicustomint5, si.sicustomint6, si.sicustomint7, si.sicustomint8, si.sicustomint9, si.sicustomint10, si.sicustomdbl1, si.sicustomdbl2, si.sicustomdbl3, si.sicustomdbl4, si.sicustomdbl5, si.sicustomdbl6, si.sicustomdbl7, si.sicustomdbl8, si.sicustomdbl9, si.sicustomdbl10, si.sicustomdate1, si.sicustomdate2, si.sicustomdate3, si.sicustomdate4, si.sicustomdate5, si.sicustomdate6, si.sicustomdate7, si.sicustomdate8, si.sicustomdate9, si.sicustomdate10 FROM m5_si si"

        dtUtama = AmbilData("aplikasi1-M_12_Pos_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        'errMessage = dtUtama.Rows.Count & "" : GoTo selesai
        'result(2) = dtUtama.Rows.Count.ToString & " - " & sql & " where " & Filter : GoTo selesai
        If dtUtama.Rows.Count > 0 Then
            For Each dr As DataRow In dtUtama.Rows
                siid = String.Concat(siid, FxDB(dr("siid"), ""), ",")
                If utama.Length = 0 Then
                    utama = String.Concat(utama,
                                      "INSERT INTO `m0_si` VALUES(" &
                         FxDB(dr("siid"), ""), ",",
                        "'" & FxDB(dr("sicabang"), "") & "'", ",",
                        "'" & FxDB(dr("silokasi"), "") & "'", ",",
                        "'" & FxDB(dr("sigudang"), "") & "'", ",",
                        "'" & FxDB(dr("siasalbarang") & "'", ""), ",",
                        "'" & FxDB(dr("siasalbarangkategori") & "'", ""), ",",
                        "'" & FxDB(dr("sijenispenjualan"), "") & "'", ",",
                        "'" & FxDB(dr("sijenispenjualankategori") & "'", ""), ",",
                        "'" & FxDB(dr("sisaldoawal"), "") & "'", ",",
                        "'" & FxDB(dr("sicarabayar"), "") & "'", ",",
                        "'" & FxDB(dr("sisumber"), "") & "'", ",",
                        "'" & FxDB(dr("siautonotransaksi") & "'", ""), ",",
                        "'" & FxDB(dr("sinotransaksi"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitgl"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sikodepa"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomer"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomerkontak"), "") & "'", ",",
                        "'" & FxDB(dr("si1alamat1"), "") & "'", ",",
                        "'" & FxDB(dr("si1alamat2"), "") & "'", ",",
                        "'" & FxDB(dr("si1alamat3"), "") & "'", ",",
                        "'" & FxDB(dr("si2alamat1"), "") & "'", ",",
                        "'" & FxDB(dr("si2alamat2"), "") & "'", ",",
                        "'" & FxDB(dr("si2alamat3"), "") & "'", ",",
                        "'" & FxDB(dr("sibagianpenjualan"), "") & "'", ",",
                        "'" & FxDB(dr("siekspedisi"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglkirim"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sitermin"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitgljatuhtempo"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("siuraian"), "") & "'", ",",
                        "'" & FxDB(dr("sicatatan"), "") & "'", ",",
                        "'" & FxDB(dr("sinoref"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglnoref"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglpenutupan"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("simatauang"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sikurs")), "") & "'", ",",
                        "'" & FxDB(dr("sihargatermasukpajak"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotal")), "") & "'", ",",
                        "'" & FxDB(dr("sidiskonpersen"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmldiskon")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotalpajak1detail")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotalpajak2detail")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibiayalainpersen")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibiayalain")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotaltransaksi")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmluangmuka")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmlbayar")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayartunai")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarkkredit")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarkdebit")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarvoucher")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarpoin")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarjmlpoin")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sichargepersen")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicharge")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmlkembali")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sipoinsebelumnya")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sipoindidapat")), "") & "'", ",",
                        "'" & FxDB(dr("sistatuslunas"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitgllunas"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sinofakturpajak"), "") & "'", ",",
                        "'" & FxDB(dr("sisdhbayarpajak"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglbayarpajak"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sirekdiskon"), "") & "'", ",",
                        "'" & FxDB(dr("sirekpajak1"), "") & "'", ",",
                        "'" & FxDB(dr("sirekpajak2"), "") & "'", ",",
                        "'" & FxDB(dr("sirekbiayalain"), "") & "'", ",",
                        "'" & FxDB(dr("sirekuangmuka"), "") & "'", ",",
                        "'" & FxDB(dr("sirekbayar"), "") & "'", ",",
                        "'" & FxDB(dr("sirekcharge"), "") & "'", ",",
                        "'" & FxDB(dr("sirekkembali"), "") & "'", ",",
                        "'" & FxDB(dr("siidsq"), "") & "'", ",",
                        "'" & FxDB(dr("siidso"), "") & "'", ",",
                        "'" & FxDB(dr("siidas"), "") & "'", ",",
                        "'" & FxDB(dr("siidpi"), "") & "'", ",",
                        "'" & FxDB(dr("siidpl"), "") & "'", ",",
                        "'" & FxDB(dr("siiddo"), "") & "'", ",",
                        "'" & FxDB(dr("siiddr"), "") & "'", ",",
                        "'" & FxDB(dr("sistatusrnr"), "") & "'", ",",
                        "'" & FxDB(dr("sistatussr"), "") & "'", ",",
                        "'" & FxDB(dr("sistatusrealisasi"), "") & "'", ",",
                        "'" & FxDB(dr("sistatussie"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglsie"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sistatus"), "") & "'", ",",
                        "'" & FxDB(dr("sistatussebelumnya"), "") & "'", ",",
                        "'" & FxDB(dr("sijmlrevisi"), "") & "'", ",",
                        "'" & FxDB(dr("sicetakanke"), "") & "'", ",",
                        "'" & FxDB(dr("siinputuser"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("siinputtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                        "'" & FxDB(dr("simodifikasiuser"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("simodifikasitgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                        "'" & FxDB(dr("siposting"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sipostingtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                        "'" & FxDB(dr("situtupperiode"), "") & "'", ",",
                        "'" & FxDB(dr("siisclose"), "") & "'", ",",
                        "'" & FxDB(dr("siuploaded"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomarea"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext1"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext2"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext3"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext4"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext5"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext6"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext7"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext8"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext9"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext10"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint1"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint2"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint3"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint4"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint5"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint6"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint7"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint8"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint9"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint10"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl1")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl2")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl3")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl4")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl5")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl6")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl7")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl8")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl9")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl10")), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate4"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate5"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate6"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate7"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate8"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate9"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate10"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB("", "") & "'", ")" & sptRow)
                Else
                    utama = String.Concat(utama,
                                      ",(" &
                         FxDB(dr("siid"), ""), ",",
                        "'" & FxDB(dr("sicabang"), "") & "'", ",",
                        "'" & FxDB(dr("silokasi"), "") & "'", ",",
                        "'" & FxDB(dr("sigudang"), "") & "'", ",",
                        "'" & FxDB(dr("siasalbarang") & "'", ""), ",",
                        "'" & FxDB(dr("siasalbarangkategori") & "'", ""), ",",
                        "'" & FxDB(dr("sijenispenjualan"), "") & "'", ",",
                        "'" & FxDB(dr("sijenispenjualankategori") & "'", ""), ",",
                        "'" & FxDB(dr("sisaldoawal"), "") & "'", ",",
                        "'" & FxDB(dr("sicarabayar"), "") & "'", ",",
                        "'" & FxDB(dr("sisumber"), "") & "'", ",",
                        "'" & FxDB(dr("siautonotransaksi") & "'", ""), ",",
                        "'" & FxDB(dr("sinotransaksi"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitgl"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sikodepa"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomer"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomerkontak"), "") & "'", ",",
                        "'" & FxDB(dr("si1alamat1"), "") & "'", ",",
                        "'" & FxDB(dr("si1alamat2"), "") & "'", ",",
                        "'" & FxDB(dr("si1alamat3"), "") & "'", ",",
                        "'" & FxDB(dr("si2alamat1"), "") & "'", ",",
                        "'" & FxDB(dr("si2alamat2"), "") & "'", ",",
                        "'" & FxDB(dr("si2alamat3"), "") & "'", ",",
                        "'" & FxDB(dr("sibagianpenjualan"), "") & "'", ",",
                        "'" & FxDB(dr("siekspedisi"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglkirim"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sitermin"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitgljatuhtempo"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("siuraian"), "") & "'", ",",
                        "'" & FxDB(dr("sicatatan"), "") & "'", ",",
                        "'" & FxDB(dr("sinoref"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglnoref"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglpenutupan"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("simatauang"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sikurs")), "") & "'", ",",
                        "'" & FxDB(dr("sihargatermasukpajak"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotal")), "") & "'", ",",
                        "'" & FxDB(dr("sidiskonpersen"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmldiskon")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotalpajak1detail")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotalpajak2detail")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibiayalainpersen")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibiayalain")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sitotaltransaksi")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmluangmuka")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmlbayar")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayartunai")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarkkredit")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarkdebit")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarvoucher")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarpoin")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sibayarjmlpoin")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sichargepersen")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicharge")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sijmlkembali")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sipoinsebelumnya")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sipoindidapat")), "") & "'", ",",
                        "'" & FxDB(dr("sistatuslunas"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitgllunas"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sinofakturpajak"), "") & "'", ",",
                        "'" & FxDB(dr("sisdhbayarpajak"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglbayarpajak"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sirekdiskon"), "") & "'", ",",
                        "'" & FxDB(dr("sirekpajak1"), "") & "'", ",",
                        "'" & FxDB(dr("sirekpajak2"), "") & "'", ",",
                        "'" & FxDB(dr("sirekbiayalain"), "") & "'", ",",
                        "'" & FxDB(dr("sirekuangmuka"), "") & "'", ",",
                        "'" & FxDB(dr("sirekbayar"), "") & "'", ",",
                        "'" & FxDB(dr("sirekcharge"), "") & "'", ",",
                        "'" & FxDB(dr("sirekkembali"), "") & "'", ",",
                        "'" & FxDB(dr("siidsq"), "") & "'", ",",
                        "'" & FxDB(dr("siidso"), "") & "'", ",",
                        "'" & FxDB(dr("siidas"), "") & "'", ",",
                        "'" & FxDB(dr("siidpi"), "") & "'", ",",
                        "'" & FxDB(dr("siidpl"), "") & "'", ",",
                        "'" & FxDB(dr("siiddo"), "") & "'", ",",
                        "'" & FxDB(dr("siiddr"), "") & "'", ",",
                        "'" & FxDB(dr("sistatusrnr"), "") & "'", ",",
                        "'" & FxDB(dr("sistatussr"), "") & "'", ",",
                        "'" & FxDB(dr("sistatusrealisasi"), "") & "'", ",",
                        "'" & FxDB(dr("sistatussie"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sitglsie"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB(dr("sistatus"), "") & "'", ",",
                        "'" & FxDB(dr("sistatussebelumnya"), "") & "'", ",",
                        "'" & FxDB(dr("sijmlrevisi"), "") & "'", ",",
                        "'" & FxDB(dr("sicetakanke"), "") & "'", ",",
                        "'" & FxDB(dr("siinputuser"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("siinputtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                        "'" & FxDB(dr("simodifikasiuser"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("simodifikasitgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                        "'" & FxDB(dr("siposting"), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sipostingtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                        "'" & FxDB(dr("situtupperiode"), "") & "'", ",",
                        "'" & FxDB(dr("siisclose"), "") & "'", ",",
                        "'" & FxDB(dr("siuploaded"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomarea"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext1"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext2"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext3"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext4"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext5"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext6"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext7"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext8"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext9"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomtext10"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint1"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint2"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint3"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint4"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint5"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint6"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint7"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint8"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint9"), "") & "'", ",",
                        "'" & FxDB(dr("sicustomint10"), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl1")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl2")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl3")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl4")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl5")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl6")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl7")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl8")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl9")), "") & "'", ",",
                        "'" & FxDB(FixDouble(dr("sicustomdbl10")), "") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate4"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate5"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate6"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate7"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate8"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate9"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & AsFormatTanggal(FxDB(dr("sicustomdate10"), ""), "yyyy-MM-dd") & "'", ",",
                        "'" & FxDB("", "") & "'", ")" & sptRow)
                End If
            Next
            If utama.Length > 0 Then
                utama = utama + ";"
            End If
            utama = utama.Substring(0, utama.Length - sptRow.Length)
            utama = utama.Replace(sptRow, vbCrLf)
            utama = String.Concat(utama, ";")
            siid = "(" & siid & ")"
            siid = siid.Replace(",)", ")")

            'Detail
            sql = "SELECT sid.idsidetail, sid.idsi, sid.idbarang, sid.namabarang, sid.tipebarang, sid.jml, sid.satuan, sid.nilaisatuan, sid.jmlbarang, sid.satuanbarang, sid.matauang, sid.kurs, sid.idhppkhususmasuk, sid.idhppfifomasuk, sid.harga, sid.hargapricelist, sid.hpp, sid.diskon, sid.jmldiskon, sid.pajak1, sid.jmlpajak1, sid.pajak2, sid.jmlpajak2, sid.cabang, sid.lokasi, sid.gudangasal, sid.gudangtransit, sid.gudangtujuan, sid.rekpersediaan, sid.rekhargapokok, sid.rekdiskonpenjualan, sid.rekpenjualan, sid.costcenter, sid.divisi, sid.subdivisi, sid.proyek, sid.catatan, sid.urutan, sid.idsqdetail, sid.idsodetail, sid.idpidetail, sid.idpldetail, sid.iddodetail, sid.iddrdetail, sid.jmlrnr, sid.statusrnr, sid.jmlsr, sid.statussr, sid.jmlrealisasi, sid.statusrealisasi, sid.isbonus, sid.isbonusfrom, sid.isclose, sid.customtext1, sid.customtext2, CONCAT(si.sinotransaksi,'-T') as customtext3, sid.customdbl1, sid.customdbl2, sid.customdbl3, sid.customdate1, sid.customdate2, sid.customdate3 FROM m5_si_detail sid join m5_si si on sid.idsi = si.siid"
            dtDetail = AmbilData("aplikasi1-M_12_Pos_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            If dtDetail.Rows.Count > 0 Then
                For Each dr As DataRow In dtDetail.Rows
                    If detail.Length = 0 Then
                        detail = String.Concat(detail,
                                           "INSERT INTO `m0_si_detail` VALUES(" &
                            "'" & FxDB(dr("idsidetail"), "") & "'", ",",
                            "'" & FxDB(dr("idsi"), "") & "'", ",",
                            "'" & FxDB(dr("idbarang"), "") & "'", ",",
                            "'" & FxDB(Server.HtmlDecode(dr("namabarang")), "") & "'", ",",
                            "'" & FxDB(dr("tipebarang"), "").ToString & "'", ",",
                            "'" & FxDB(dr("jml"), "") & "'", ",",
                            "'" & FxDB(dr("satuan"), "").ToString & "'", ",",
                            "'" & FxDB(FixDouble(dr("nilaisatuan")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmlbarang")), "") & "'", ",",
                            "'" & FxDB(dr("satuanbarang"), "").ToString & "'", ",",
                            "'" & FxDB(dr("matauang"), "").ToString & "'", ",",
                            "'" & FxDB(dr("kurs"), "") & "'", ",",
                            "'" & FxDB(dr("idhppkhususmasuk"), "") & "'", ",",
                            "'" & FxDB(dr("idhppfifomasuk"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("harga")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("hargapricelist")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("hpp")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("diskon")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmldiskon")), "") & "'", ",",
                            "'" & FxDB(dr("pajak1"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmlpajak1")), "") & "'", ",",
                            "'" & FxDB(dr("pajak2"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmlpajak2")), "") & "'", ",",
                            "'" & FxDB(dr("cabang"), "").ToString & "'", ",",
                            "'" & FxDB(dr("lokasi"), "").ToString & "'", ",",
                            "'" & FxDB(dr("gudangasal"), "").ToString & "'", ",",
                            "'" & FxDB(dr("gudangtransit"), "").ToString & "'", ",",
                            "'" & FxDB(dr("gudangtujuan"), "").ToString & "'", ",",
                            "'" & FxDB(dr("rekpersediaan"), "").ToString & "'", ",",
                            "'" & FxDB(dr("rekhargapokok"), "").ToString & "'", ",",
                            "'" & FxDB(dr("rekdiskonpenjualan"), "").ToString & "'", ",",
                            "'" & FxDB(dr("rekpenjualan"), "").ToString & "'", ",",
                            "'" & FxDB(dr("costcenter"), "").ToString & "'", ",",
                            "'" & FxDB(dr("divisi"), "").ToString & "'", ",",
                            "'" & FxDB(dr("subdivisi"), "").ToString & "'", ",",
                            "'" & FxDB(dr("proyek"), "").ToString & "'", ",",
                            "'" & FxDB(dr("catatan"), "").ToString & "'", ",",
                            "'" & FxDB(dr("urutan"), "") & "'", ",",
                            "'" & FxDB(dr("idsqdetail"), "") & "'", ",",
                            "'" & FxDB(dr("idsodetail"), "") & "'", ",",
                            "'" & FxDB(dr("idpidetail"), "") & "'", ",",
                            "'" & FxDB(dr("idpldetail"), "") & "'", ",",
                            "'" & FxDB(dr("iddodetail"), "") & "'", ",",
                            "'" & FxDB(dr("iddrdetail"), "") & "'", ",",
                            "'" & FxDB(dr("jmlrnr"), "") & "'", ",",
                            "'" & FxDB(dr("statusrnr"), "") & "'", ",",
                            "'" & FxDB(dr("jmlsr"), "") & "'", ",",
                            "'" & FxDB(dr("statussr"), "") & "'", ",",
                            "'" & FxDB(dr("jmlrealisasi"), "") & "'", ",",
                            "'" & FxDB(dr("statusrealisasi"), "") & "'", ",",
                            "'" & FxDB(dr("isbonus"), "") & "'", ",",
                            "'" & FxDB(dr("isbonusfrom"), "") & "'", ",",
                            "'" & FxDB(dr("isclose"), "") & "'", ",",
                            "'" & FxDB(dr("customtext1"), "").ToString & "'", ",",
                            "'" & FxDB(dr("customtext2"), "").ToString & "'", ",",
                            "'" & FxDB(dr("customtext3"), "").ToString & "'", ",",
                            "'" & FxDB(FixDouble(dr("customdbl1")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("customdbl2")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("customdbl3")), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("customdate1"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("customdate2"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("customdate3"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB("", "") & "'", ")" & sptRow)
                    Else
                        detail = String.Concat(detail,
                                           ",(" &
                            "'" & FxDB(dr("idsidetail"), "") & "'", ",",
                            "'" & FxDB(dr("idsi"), "") & "'", ",",
                            "'" & FxDB(dr("idbarang"), "") & "'", ",",
                            "'" & FxDB(Server.HtmlDecode(dr("namabarang")), "") & "'", ",",
                            "'" & FxDB(dr("tipebarang"), "") & "'", ",",
                            "'" & FxDB(dr("jml"), "") & "'", ",",
                            "'" & FxDB(dr("satuan"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("nilaisatuan")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmlbarang")), "") & "'", ",",
                            "'" & FxDB(dr("satuanbarang"), "") & "'", ",",
                            "'" & FxDB(dr("matauang"), "") & "'", ",",
                            "'" & FxDB(dr("kurs"), "") & "'", ",",
                            "'" & FxDB(dr("idhppkhususmasuk"), "") & "'", ",",
                            "'" & FxDB(dr("idhppfifomasuk"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("harga")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("hargapricelist")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("hpp")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("diskon")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmldiskon")), "") & "'", ",",
                            "'" & FxDB(dr("pajak1"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmlpajak1")), "") & "'", ",",
                            "'" & FxDB(dr("pajak2"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("jmlpajak2")), "") & "'", ",",
                            "'" & FxDB(dr("cabang"), "") & "'", ",",
                            "'" & FxDB(dr("lokasi"), "") & "'", ",",
                            "'" & FxDB(dr("gudangasal"), "") & "'", ",",
                            "'" & FxDB(dr("gudangtransit"), "") & "'", ",",
                            "'" & FxDB(dr("gudangtujuan"), "") & "'", ",",
                            "'" & FxDB(dr("rekpersediaan"), "") & "'", ",",
                            "'" & FxDB(dr("rekhargapokok"), "") & "'", ",",
                            "'" & FxDB(dr("rekdiskonpenjualan"), "") & "'", ",",
                            "'" & FxDB(dr("rekpenjualan"), "") & "'", ",",
                            "'" & FxDB(dr("costcenter"), "") & "'", ",",
                            "'" & FxDB(dr("divisi"), "") & "'", ",",
                            "'" & FxDB(dr("subdivisi"), "") & "'", ",",
                            "'" & FxDB(dr("proyek"), "") & "'", ",",
                            "'" & FxDB(dr("catatan"), "") & "'", ",",
                            "'" & FxDB(dr("urutan"), "") & "'", ",",
                            "'" & FxDB(dr("idsqdetail"), "") & "'", ",",
                            "'" & FxDB(dr("idsodetail"), "") & "'", ",",
                            "'" & FxDB(dr("idpidetail"), "") & "'", ",",
                            "'" & FxDB(dr("idpldetail"), "") & "'", ",",
                            "'" & FxDB(dr("iddodetail"), "") & "'", ",",
                            "'" & FxDB(dr("iddrdetail"), "") & "'", ",",
                            "'" & FxDB(dr("jmlrnr"), "") & "'", ",",
                            "'" & FxDB(dr("statusrnr"), "") & "'", ",",
                            "'" & FxDB(dr("jmlsr"), "") & "'", ",",
                            "'" & FxDB(dr("statussr"), "") & "'", ",",
                            "'" & FxDB(dr("jmlrealisasi"), "") & "'", ",",
                            "'" & FxDB(dr("statusrealisasi"), "") & "'", ",",
                            "'" & FxDB(dr("isbonus"), "") & "'", ",",
                            "'" & FxDB(dr("isbonusfrom"), "") & "'", ",",
                            "'" & FxDB(dr("isclose"), "") & "'", ",",
                            "'" & FxDB(dr("customtext1"), "") & "'", ",",
                            "'" & FxDB(dr("customtext2"), "") & "'", ",",
                            "'" & FxDB(dr("customtext3"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("customdbl1")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("customdbl2")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("customdbl3")), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("customdate1"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("customdate2"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("customdate3"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB("", "") & "'", ")" & sptRow)
                    End If


                Next
                detail = detail.Substring(0, detail.Length - sptRow.Length)
                detail = detail.Replace(sptRow, vbCrLf)
                detail = detail.Replace("'", "\'")
                detail = detail.Replace(",\'", ",'")
                detail = detail.Replace("\',", "',")
                detail = detail.Replace("(\'", "('")
                detail = detail.Replace("\')", "')")
                detail = String.Concat(detail, ";")
                HttpUtility.HtmlDecode(detail)
            End If

            'pay
            sql = "SELECT sid.idsicarabayar as idsicarabayar,sid.idsi as idsi,sid.carabayar as carabayar,sid.matauang as matauang,sid.kurs as kurs,sid.jumlah as jumlah,sid.jumlahvalas as jumlahvalas,sid.nogiro as nogiro,sid.tgljt as tgljt,sid.bank as bank,sid.noacbank as noacbank,sid.rekbank as rekbank,CONCAT(si.sinotransaksi,'-T') as rekgiro,CONCAT(si.sicabang,' - ',si.silokasi) as catatan, sid.urutan as urutan, sid.isclose as isclose FROM m5_si si join m5_si_pay sid ON sid.idsi = si.siid"
            dtPay = AmbilData("aplikasi1-M_12_Pos_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            Dim dtServerPay As New DataTable
            If dtPay.Rows.Count > 0 Then
                For Each dr As DataRow In dtPay.Rows
                    If pay.Length = 0 Then
                        pay = String.Concat(pay,
                                        "INSERT INTO `m0_si_pay` VALUES(" &
                            "|" & FxDB(dr("idsicarabayar"), "") & "|", ",",
                            "|" & FxDB(dr("idsi"), "") & "|", ",",
                            "|" & FxDB(dr("carabayar"), "") & "|", ",",
                            "|" & FxDB(dr("matauang"), "") & "|", ",",
                            "|" & FxDB(FixDouble(dr("kurs")), "") & "|", ",",
                            "|" & FxDB(FixDouble(dr("jumlah")), "") & "|", ",",
                            "|" & FxDB(FixDouble(dr("jumlahvalas")), "") & "|", ",",
                            "|" & FxDB(dr("nogiro"), "") & "|", ",",
                            "|" & AsFormatTanggal(FxDB(dr("tgljt"), ""), "yyyy-MM-dd") & "|", ",",
                            "|" & FxDB(dr("bank"), "") & "|", ",",
                            "|" & FxDB(dr("noacbank"), "") & "|", ",",
                            "|" & FxDB(dr("rekbank"), "") & "|", ",",
                            "|" & FxDB(dr("rekgiro"), "") & "|", ",",
                            "|" & FxDB(dr("catatan"), "") & "|", ",",
                            "|" & FxDB(dr("urutan"), "") & "|", ",",
                            "|" & FxDB(dr("isclose"), "") & "|", ",",
                            "|" & FxDB("", "") & "|", ")" & sptRow)
                    Else
                        pay = String.Concat(pay,
                                        ",(" &
                            "|" & FxDB(dr("idsicarabayar"), "") & "|", ",",
                            "|" & FxDB(dr("idsi"), "") & "|", ",",
                            "|" & FxDB(dr("carabayar"), "") & "|", ",",
                            "|" & FxDB(dr("matauang"), "") & "|", ",",
                            "|" & FxDB(FixDouble(dr("kurs")), "") & "|", ",",
                            "|" & FxDB(FixDouble(dr("jumlah")), "") & "|", ",",
                            "|" & FxDB(FixDouble(dr("jumlahvalas")), "") & "|", ",",
                            "|" & FxDB(dr("nogiro"), "") & "|", ",",
                            "|" & AsFormatTanggal(FxDB(dr("tgljt"), ""), "yyyy-MM-dd") & "|", ",",
                            "|" & FxDB(dr("bank"), "") & "|", ",",
                            "|" & FxDB(dr("noacbank"), "") & "|", ",",
                            "|" & FxDB(dr("rekbank"), "") & "|", ",",
                            "|" & FxDB(dr("rekgiro"), "") & "|", ",",
                            "|" & FxDB(dr("catatan"), "") & "|", ",",
                            "|" & FxDB(dr("urutan"), "") & "|", ",",
                            "|" & FxDB(dr("isclose"), "") & "|", ",",
                            "|" & FxDB("", "") & "|", ")" & sptRow)
                    End If

                Next

                pay = pay.Substring(0, pay.Length - sptRow.Length)
                pay = pay.Replace(sptRow, vbCrLf)
                pay = pay.Replace("|", """")
                pay = String.Concat(pay, ";")
            End If
            result(1) = 1
            'result(2) = "0415 Jacket Mom&amp;Me 0196-011215010196 XS" : GoTo selesai
            result(2) = detail : GoTo selesai
        Else
            result(1) = 0
            result(2) = "0415 Jacket Mom&amp;Me 0196-011215010196 XS" : GoTo selesai
            'result(2) = "Transaction data not found" : GoTo selesai
        End If

selesai:
        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(siid, sptSubParam, utama, vbCrLf, detail, vbCrLf, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid" & sptSubParam & "data"))

        Return wsResult
    End Function

    'upload penjualan 1 fungsi
    <WebMethod()>
    Public Function M12_SiUploadDataNew(ByVal param As String) As String

        'M5_SiGetUpload --------------------------------------------------------
        'siid
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""
        Dim CreateFile As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", FilterTglSI As String = "", FilterTglIT As String = "", FilterTglJurnal As String = "", Lokasi As String = ""
        Dim dt As New DataTable, dtdetail As New DataTable, dtpay As New DataTable, dtdate As New DataTable
        Dim siid As String = ""

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

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
            FilterTglSI = pagingSplit(3).Replace("tanggal", "si.sitgl")
            FilterTglIT = pagingSplit(3).Replace("tanggal", "tgl")
            FilterTglJurnal = pagingSplit(3).Replace("tanggal", "ttgl")
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET DATA =============================================================
        Dim dataSplit() As String
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        Lokasi = dataSplit(0)
        CreateFile = dataSplit(1)
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)
        Dim stepke As Integer = 0
        Try
            'end of hapus penjualan dari tabel utama
            stepke = 1
            If Len(FilterTglSI) > 0 Then
                sql = "delete sid from m5_si_detail sid join m5_si si on sid.idsi = si.siid where si.silokasi = '" & Lokasi & "' AND " & FilterTglSI & ""
            Else
                sql = "delete sid from m5_si_detail sid join m5_si si on sid.idsi = si.siid where si.silokasi = '" & Lokasi & "'"
            End If

            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            stepke = 2
            If Len(FilterTglSI) > 0 Then
                sql = "delete sid from m5_si_pay sid join m5_si si on sid.idsi = si.siid where si.silokasi = '" & Lokasi & "' AND " & FilterTglSI & ""
            Else
                sql = "delete sid from m5_si_pay sid join m5_si si on sid.idsi = si.siid where si.silokasi = '" & Lokasi & "'"
            End If

            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            stepke = 3
            If Len(FilterTglSI) > 0 Then
                sql = "delete si from m5_si si where si.silokasi = '" & Lokasi & "' AND " & FilterTglSI & ""
            Else
                sql = "delete si from m5_si si where si.silokasi = '" & Lokasi & "'"
            End If

            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            stepke = 4
            If Len(FilterTglIT) > 0 Then
                sql = "delete from m1_item_transaction where sumber = 'SI' and lokasi = '" & Lokasi & "' AND " & FilterTglIT & ""
            Else
                sql = "delete from m1_item_transaction where sumber = 'SI' and lokasi = '" & Lokasi & "'"
            End If

            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            stepke = 5
            If Len(FilterTglIT) > 0 Then
                sql = "delete from m2_transaction_journal where tsumber = 'POS' and tlokasi = '" & Lokasi & "' AND " & FilterTglJurnal & ""
            Else
                sql = "delete from m2_transaction_journal where tsumber = 'POS' and tlokasi = '" & Lokasi & "'"
            End If

            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'end of hapus penjualan dari tabel utama


            'hapus tabel penampung
            stepke = 6
            sql = "delete sid from `m0_si_detail` sid join m0_si si on si.siid = sid.idsi AND si.silokasi = '" & Lokasi & "'"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            stepke = 7
            sql = "delete sid from `m0_si_pay` sid join m0_si si on si.siid = sid.idsi AND si.silokasi = '" & Lokasi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            stepke = 8
            sql = "delete si from `m0_si` si where silokasi = '" & Lokasi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'end of hapus tabel penampung

            'insert data baru ke tabel penampung
            stepke = 9
            sql = CreateFile
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'insert si utama
            stepke = 10
            sql = "INSERT INTO m5_si(SELECT 0 as siid,si.sicabang,si.silokasi,si.sigudang,si.siasalbarang,si.siasalbarangkategori,si.sijenispenjualan,si.sijenispenjualankategori,si.sisaldoawal,si.sicarabayar,si.sisumber,si.siautonotransaksi,CONCAT(si.sinotransaksi,'-T') as sinotransaksi,si.sitgl,si.sikodepa,si.sicustomer,si.sicustomerkontak,si.si1alamat1,si.si1alamat2,si.si1alamat3,si.si2alamat1,si.si2alamat2,si.si2alamat3,si.sibagianpenjualan,si.siekspedisi,si.sitglkirim,si.sitermin,si.sitgljatuhtempo,si.siuraian,si.sicatatan,CONCAT('(',si.sinotransaksi,')') as sinoref,si.sitglnoref,si.sitglpenutupan,si.simatauang,si.sikurs,si.sihargatermasukpajak,si.sitotal,si.sidiskonpersen,si.sijmldiskon,si.sitotalpajak1detail,si.sitotalpajak2detail,si.sibiayalainpersen,si.sibiayalain,si.sitotaltransaksi,si.sijmluangmuka,si.sijmlbayar,si.sibayartunai,si.sibayarkkredit,si.sibayarkdebit,si.sibayarvoucher,si.sibayarpoin,si.sibayarjmlpoin,si.sichargepersen,si.sicharge,si.sijmlkembali,si.sipoinsebelumnya,si.sipoindidapat,si.sistatuslunas,si.sitgllunas,si.sinofakturpajak,si.sisdhbayarpajak,si.sitglbayarpajak,si.sirekdiskon,si.sirekpajak1,si.sirekpajak2,si.sirekbiayalain,si.sirekuangmuka,si.sirekbayar,si.sirekcharge,si.sirekkembali,si.siidsq,si.siidso,si.siidas,si.siidpi,si.siidpl,si.siiddo,si.siiddr,si.sistatusrnr,si.sistatussr,si.sistatusrealisasi,si.sistatussie,si.sitglsie,si.sistatus,si.sistatussebelumnya,si.sijmlrevisi,si.sicetakanke,si.siinputuser,si.siinputtgl,si.simodifikasiuser,si.simodifikasitgl,si.siposting,si.sipostingtgl,si.situtupperiode,si.siisclose,si.siuploaded,si.sicustomarea,si.sicustomtext1,si.sicustomtext2,si.sicustomtext3,si.sicustomtext4,si.sicustomtext5,si.sicustomtext6,si.sicustomtext7,si.sicustomtext8,si.sicustomtext9,si.sicustomtext10,si.sicustomint1,si.sicustomint2,si.sicustomint3,si.sicustomint4,si.sicustomint5,si.sicustomint6,si.siid as sicustomint7,si.sicustomint8,si.sicustomint9,si.sicustomint10,si.sicustomdbl1,si.sicustomdbl2,si.sicustomdbl3,si.sicustomdbl4,si.sicustomdbl5,si.sicustomdbl6,si.sicustomdbl7,si.sicustomdbl8,si.sicustomdbl9,si.sicustomdbl10,si.sicustomdate1,si.sicustomdate2,si.sicustomdate3,si.sicustomdate4,si.sicustomdate5,si.sicustomdate6,si.sicustomdate7,si.sicustomdate8,si.sicustomdate9,si.sicustomdate10 FROM `m0_si` si where si.siid in " & Filter & " ORDER BY si.sitgl, si.siid)"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'System.Threading.Thread.Sleep(5000)

            'update poin
            stepke = 11
            'sql = "INSERT INTO m1_contact_point (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m0_si si WHERE si.siid in " & Filter & " GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
            sql = "UPDATE m1_contact_point SET cppoin = 0; INSERT INTO m1_contact_point(SELECT cpad.kontak as cpidkontak, SUM(cpad.poinmasuk - cpad.poinkeluar) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m_12_cpa cpa JOIN m_12_cpa_detail cpad ON cpa.cpaid = cpad.idcpa AND cpa.cpastatus IN(2,3,4,7) GROUP BY cpad.kontak) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin); INSERT INTO m1_contact_point (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m5_si si WHERE si.sistatus IN(2,3,4,7) GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin);"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'insert tabel si detail
            'System.Threading.Thread.Sleep(5000)
            stepke = 12
            sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON si.sicustomint7 = sid.idsi and si.sinotransaksi = sid.customtext3 where sid.idsi in " & Filter & " AND si.silokasi = '" & Lokasi & "')"
            'result(2) = sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'insert tabel si pay
            'System.Threading.Thread.Sleep(5000)
            stepke = 13
            'Sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi AND sid.idupload = si.siidupload)"
            'sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi)"
            sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi where sid.idsi in " & Filter & ")"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'update Voucher
            'System.Threading.Thread.Sleep(5000)
            stepke = 14
            sql = "update m_12_pos_voucher_in vi join m0_si_pay sid on vi.vikode = sid.noacbank AND sid.carabayar = 6 SET vi.vijmlbayar = sid.jumlah, vijmlbayarvalas = sid.jumlahvalas"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            ''insert voucher out
            'sql = "INSERT INTO m_12_pos_voucher_out (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m0_si si WHERE si.siid in " & Filter & " GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'hapus tabel si utama penampung

            stepke = 15
            sql = "INSERT into m1_item_transaction(SELECT 0 as id,si.sicabang as cabang,si.silokasi as lokasi,si.sigudang as gudang,si.sikodepa as kodepa,0 as jenismutasi,si.sisumber as sumber,si.siid as idutama,sid.idsidetail as iddetail,si.sinotransaksi as notransaksi,si.sitgl as tgl,si.sicustomer as kontak,sid.idbarang as idbarang,sid.namabarang as namabarang,sid.tipebarang as tipebarang,i.bhpp as tipehpp,sid.jml as jml,sid.satuan as satuan,sid.jmlbarang as jmlbarang,sid.satuanbarang as satuanbarang,si.simatauang as matauang,si.sikurs as kurs,sid.harga as harga,sid.diskon as diskon,sid.jmldiskon as jmldiskon,0 as idhppikm,0 as idhppikk,0 as sidhppfifo,(CASE si.sihargatermasukpajak WHEN 0 THEN ((sid.jml * sid.harga) - sid.jmldiskon) / sid.jml ELSE ((sid.jml * sid.harga) - sid.jmldiskon - sid.jmlpajak1) / sid.jml END) as hpp,si.siuraian as uraian,si.sicatatan as catatan,sid.catatan as catatandetail,sid.costcenter as costcenter,sid.divisi as divisi,sid.subdivisi as subdivisi,sid.proyek as proyek,0 as saldojml,0 as saldohpp,0 as saldonilai,si.siinputtgl as inputtgl,si.siinputuser as inputuser,si.sipostingtgl as postingtgl,0 as updatehpp,1 as postinghpp,0 as hppfix,0 as postingjurnal,0 as jurnalfix,0 as tutupperiode,0 as isclose,'' as customtext1,'' as customtext2,'' as customtext3,'' as customtext4,'' as customtext5,'' as customtext6,'' as customtext7,'' as customtext8,'' as customtext9,'' as customtext10,'0' as customint1,'0' as customint2,'0' as customint3,'0' as customint4,'0' as customint5,'0' as customint6,'0' as customint7,'0' as customint8,'0' as customint9,'0' as customint10,'0' as customdbl1,'0' as customdbl2,'0' as customdbl3,'0' as customdbl4,'0' as customdbl5,'0' as customdbl6,'0' as customdbl7,'0' as customdbl8,'0' as customdbl9,'0' as customdbl10,'1900-01-01' as customdate1,'1900-01-01' as customdate2,'1900-01-01' as customdate3,'1900-01-01' as customdate4,'1900-01-01' as customdate5,'1900-01-01' as customdate6,'1900-01-01' as customdate7,'1900-01-01' as customdate8,'1900-01-01' as customdate9,'1900-01-01' as customdate10 FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi AND si.sicustomint7 in " & Filter & " join m1_item i ON i.bid = sid.idbarang)"
            'sql = "INSERT into m1_item_transaction(SELECT 0 as id,si.sicabang as cabang,si.silokasi as lokasi,si.sigudang as gudang,si.sikodepa as kodepa,0 as jenismutasi,si.sisumber as sumber,si.siid as idutama,sid.idsidetail as iddetail,si.sinotransaksi as notransaksi,si.sitgl as tgl,si.sicustomer as kontak,sid.idbarang as idbarang,sid.namabarang as namabarang,sid.tipebarang as tipebarang,i.bhpp as tipehpp,sid.jml as jml,sid.satuan as satuan,sid.jmlbarang as jmlbarang,sid.satuanbarang as satuanbarang,si.simatauang as matauang,si.sikurs as kurs,sid.harga as harga,sid.diskon as diskon,sid.jmldiskon as jmldiskon,0 as idhppikm,0 as idhppikk,0 as sidhppfifo,(CASE si.sihargatermasukpajak WHEN 0 THEN ((sid.jml * sid.harga) - sid.jmldiskon) / sid.jml ELSE ((sid.jml * sid.harga) - sid.jmldiskon - sid.jmlpajak1) / sid.jml END) as hpp,si.siuraian as uraian,si.sicatatan as catatan,sid.catatan as catatandetail,sid.costcenter as costcenter,sid.divisi as divisi,sid.subdivisi as subdivisi,sid.proyek as proyek,0 as saldojml,0 as saldohpp,0 as saldonilai,si.siinputtgl as inputtgl,si.siinputuser as inputuser,si.sipostingtgl as postingtgl,0 as updatehpp,1 as postinghpp,0 as hppfix,0 as postingjurnal,0 as jurnalfix,0 as tutupperiode,0 as isclose,'' as customtext1,'' as customtext2,'' as customtext3,'' as customtext4,'' as customtext5,'' as customtext6,'' as customtext7,'' as customtext8,'' as customtext9,'' as customtext10,'0' as customint1,'0' as customint2,'0' as customint3,'0' as customint4,'0' as customint5,'0' as customint6,'0' as customint7,'0' as customint8,'0' as customint9,'0' as customint10,'0' as customdbl1,'0' as customdbl2,'0' as customdbl3,'0' as customdbl4,'0' as customdbl5,'0' as customdbl6,'0' as customdbl7,'0' as customdbl8,'0' as customdbl9,'0' as customdbl10,'1900-01-01' as customdate1,'1900-01-01' as customdate2,'1900-01-01' as customdate3,'1900-01-01' as customdate4,'1900-01-01' as customdate5,'1900-01-01' as customdate6,'1900-01-01' as customdate7,'1900-01-01' as customdate8,'1900-01-01' as customdate9,'1900-01-01' as customdate10 FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi AND si.sicustomint7 in " & Filter & ")"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'hitung stok per gudang
            'System.Threading.Thread.Sleep(5000)
            stepke = 16
            'sql = "INSERT INTO m1_item_stock_warehouse(select sid.idbarang, si.sigudang, SUM(sid.jmlbarang * -1) as stok from m5_si_detail sid JOIN m5_si si on si.siid = sid.idsi AND si.sicustomint7 in " & Filter & " GROUP BY si.sigudang, sid.idbarang) ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
            sql = "UPDATE m1_item_stock_warehouse SET stok = 0; INSERT INTO m1_item_stock_warehouse(SELECT b.bid, tb.gudang, sum((CASE tb.jenismutasi WHEN 1 THEN tb.jmlbarang ELSE tb.jmlbarang * -1 END)) as stokfix FROM m1_item_transaction tb JOIN m1_item b ON tb.idbarang = b.bid WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang) ON DUPLICATE KEY UPDATE stok=VALUES(stok)"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'update stok global
            System.Threading.Thread.Sleep(5000)
            stepke = 17
            sql = "UPDATE m1_item SET bstok = 0; UPDATE m1_item i JOIN (SELECT isw.idbarang, ROUND(SUM(isw.stok),5) as totalstok FROM m1_item_stock_warehouse isw GROUP BY isw.idbarang ) as sp ON i.bid = sp.idbarang SET i.bstok = sp.totalstok WHERE i.bstok <> sp.totalstok;"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT MSMQ JURNAL =================================================================
            stepke = 18
            Dim sumber As String = "SI_POS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
            'BUAT ID UNIQUE
            mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

            'MSMQ TABEL
            sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                & mjid & "', '" & sumber & "', '0', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '0')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'MSMQ ANTRIAN
            stepke = 19
            Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
            If PostingJurnal.Equals("0") = False Then
                hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                If Len(hasilMsmq) > 0 Then
                    result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF INSERT MSMQ JURNAL ==========================================================

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1

            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***' 

            result(1) = 0
            result(2) = ex.Message & "" & stepke.ToString : GoTo selesai
            result(3) = 0
            result(4) = Filter
        End Try
        objCmd = Nothing

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        result(2) = result(2) & " - stepke : " & stepke

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, sptParam)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid"))
        Return wsResult
    End Function


    '//FUNGSI UNTUK EXECUTE DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_ExecuteSQL2(ByVal websiteAccessKey As String, ByVal userid As String, ByVal fileName As String, ByVal strCon As String) As String()
        ' Uses the mysqlimport.exe program to execute a backup of the database.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        'Dim filePath As String = HttpContext.Current.Server.MapPath("~/") & "files\db\"

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If

            'CEK FILE EXISTS
            If (Not File.Exists(fileName)) Then
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai
            End If

            'PROSES EXECUTE SQL
            Dim myProcess As New Process()
            myProcess.StartInfo.FileName = "cmd.exe"
            myProcess.StartInfo.UseShellExecute = False
            myProcess.StartInfo.CreateNoWindow = True
            myProcess.StartInfo.WorkingDirectory = pathServiceDB
            myProcess.StartInfo.RedirectStandardInput = True
            myProcess.StartInfo.RedirectStandardOutput = True
            myProcess.StartInfo.RedirectStandardError = True
            myProcess.Start()

            Dim myStreamWriter As StreamWriter = myProcess.StandardInput
            Dim mystreamreader As StreamReader = myProcess.StandardOutput
            myStreamWriter.WriteLine("mysql -u " & DBUser & " -p" & DBPassword & " " & DBDatabase & " < " & fileName & " ")
            myStreamWriter.Close()
            myProcess.WaitForExit()
            myProcess.Close()

            hasil(0) = 1
            hasil(1) = ""

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Execute database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    <WebMethod()>
    Public Function M12_InsertSIUtama(ByVal param As String) As String
        'M5_SiGetUpload --------------------------------------------------------
        'siid
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""
        Dim sqlFile As String = ""
        Dim lokasi As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable, dtdetail As New DataTable, dtpay As New DataTable, dtdate As New DataTable
        Dim siid As String = ""

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


        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'insert si utama
            sql = "INSERT INTO m5_si(SELECT 0 as siid,si.sicabang,si.silokasi,si.sigudang,si.siasalbarang,si.siasalbarangkategori,si.sijenispenjualan,si.sijenispenjualankategori,si.sisaldoawal,si.sicarabayar,si.sisumber,si.siautonotransaksi,CONCAT(si.sinotransaksi,'-T') as sinotransaksi,si.sitgl,si.sikodepa,si.sicustomer,si.sicustomerkontak,si.si1alamat1,si.si1alamat2,si.si1alamat3,si.si2alamat1,si.si2alamat2,si.si2alamat3,si.sibagianpenjualan,si.siekspedisi,si.sitglkirim,si.sitermin,si.sitgljatuhtempo,si.siuraian,si.sicatatan,CONCAT('(',si.sinotransaksi,')') as sinoref,si.sitglnoref,si.sitglpenutupan,si.simatauang,si.sikurs,si.sihargatermasukpajak,si.sitotal,si.sidiskonpersen,si.sijmldiskon,si.sitotalpajak1detail,si.sitotalpajak2detail,si.sibiayalainpersen,si.sibiayalain,si.sitotaltransaksi,si.sijmluangmuka,si.sijmlbayar,si.sibayartunai,si.sibayarkkredit,si.sibayarkdebit,si.sibayarvoucher,si.sibayarpoin,si.sibayarjmlpoin,si.sichargepersen,si.sicharge,si.sijmlkembali,si.sipoinsebelumnya,si.sipoindidapat,si.sistatuslunas,si.sitgllunas,si.sinofakturpajak,si.sisdhbayarpajak,si.sitglbayarpajak,si.sirekdiskon,si.sirekpajak1,si.sirekpajak2,si.sirekbiayalain,si.sirekuangmuka,si.sirekbayar,si.sirekcharge,si.sirekkembali,si.siidsq,si.siidso,si.siidas,si.siidpi,si.siidpl,si.siiddo,si.siiddr,si.sistatusrnr,si.sistatussr,si.sistatusrealisasi,si.sistatussie,si.sitglsie,si.sistatus,si.sistatussebelumnya,si.sijmlrevisi,si.sicetakanke,si.siinputuser,si.siinputtgl,si.simodifikasiuser,si.simodifikasitgl,si.siposting,si.sipostingtgl,si.situtupperiode,si.siisclose,si.siuploaded,si.sicustomarea,si.sicustomtext1,si.sicustomtext2,si.sicustomtext3,si.sicustomtext4,si.sicustomtext5,si.sicustomtext6,si.sicustomtext7,si.sicustomtext8,si.sicustomtext9,si.sicustomtext10,si.sicustomint1,si.sicustomint2,si.sicustomint3,si.sicustomint4,si.sicustomint5,si.sicustomint6,si.siid as sicustomint7,si.sicustomint8,si.sicustomint9,si.sicustomint10,si.sicustomdbl1,si.sicustomdbl2,si.sicustomdbl3,si.sicustomdbl4,si.sicustomdbl5,si.sicustomdbl6,si.sicustomdbl7,si.sicustomdbl8,si.sicustomdbl9,si.sicustomdbl10,si.sicustomdate1,si.sicustomdate2,si.sicustomdate3,si.sicustomdate4,si.sicustomdate5,si.sicustomdate6,si.sicustomdate7,si.sicustomdate8,si.sicustomdate9,si.sicustomdate10 FROM `m0_si` si where si.siid in " & Filter & " ORDER BY si.sitgl, si.siid)"
            'result(2) = sql : GoTo selesai

            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            System.Threading.Thread.Sleep(5000)
            'update poin
            'sql = "INSERT INTO m1_contact_point (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m0_si si WHERE si.siid in " & Filter & " GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
            sql = "UPDATE m1_contact_point SET cppoin = 0; INSERT INTO m1_contact_point(SELECT cpad.kontak as cpidkontak, SUM(cpad.poinmasuk - cpad.poinkeluar) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m_12_cpa cpa JOIN m_12_cpa_detail cpad ON cpa.cpaid = cpad.idcpa AND cpa.cpastatus IN(2,3,4,7) GROUP BY cpad.kontak) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin); INSERT INTO m1_contact_point (SELECT si.sicustomer as cpidkontak, SUM(si.sipoindidapat - si.sibayarpoin) as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, '0' as cpcustomint1, '0' as cpcustomint2, '0' as cpcustomint3, '0' as cpcustomdbl1, '0' as cpcustomdbl2, '0' as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m5_si si WHERE si.sistatus IN(2,3,4,7) GROUP BY si.sicustomer) ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin);"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            System.Threading.Thread.Sleep(10000)
            ''Sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.customtext3 = si.sinotransaksi AND sid.idupload = si.siidupload)"
            'sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON si.sicustomint7 = sid.idsi AND si.sinotransaksi = sid.customtext3)"
            ''result(2) = Sql : GoTo selesai
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()
            'System.Threading.Thread.Sleep(10000)
            ''Sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi AND sid.idupload = si.siidupload)"
            'sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi)"
            ''result(2) = Sql : GoTo selesai
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'System.Threading.Thread.Sleep(10000)
            'sql = "delete sid from `m0_si_detail` sid join m0_si si on si.siid = sid.idsi where sid.idsi in" & Filter
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'System.Threading.Thread.Sleep(10000)
            'sql = "delete sid from `m0_si_pay` sid join m0_si si on si.siid = sid.idsi where sid.idsi in" & Filter
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'System.Threading.Thread.Sleep(10000)
            'sql = "delete sid from `m0_si_pay` sid join m0_si si on si.siid = sid.idsi where sid.idsi in" & Filter
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'System.Threading.Thread.Sleep(10000)
            'sql = "delete si from `m0_si` si where si.siid in" & Filter
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()

            'System.Threading.Thread.Sleep(10000)


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1

            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***' 

            result(1) = 0
            result(2) = ex.Message : GoTo selesai
            result(3) = 0
            result(4) = Filter
        End Try
        objCmd = Nothing

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, sptParam)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("siid"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_InsertSIDetail(ByVal param As String) As String
        'M0_ExecuteDbFile --------------------------------------------------------
        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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
            result(2) = "Invalid parameter." & paramSplit.Length : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If



        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================
        'Filter
        Dim Filter As String = ""
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        If Len(Filter) > 0 Then
            Filter = " where " & Filter
        End If
        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""
        'truncate m0_si
        Try
            'Sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.customtext3 = si.sinotransaksi AND sid.idupload = si.siidupload)"
            Sql = "INSERT INTO m5_si_detail(SELECT 0 as idsidetail,si.siid as idsi,sid.idbarang,sid.namabarang,sid.tipebarang,sid.jml,sid.satuan,sid.nilaisatuan,sid.jmlbarang,sid.satuanbarang,sid.matauang,sid.kurs,sid.idhppkhususmasuk,sid.idhppfifomasuk,sid.harga,sid.hargapricelist,sid.hpp,sid.diskon,sid.jmldiskon,sid.pajak1,sid.jmlpajak1,sid.pajak2,sid.jmlpajak2,sid.cabang,sid.lokasi,sid.gudangasal,sid.gudangtransit,sid.gudangtujuan,sid.rekpersediaan,sid.rekhargapokok,sid.rekdiskonpenjualan,sid.rekpenjualan,sid.costcenter,sid.divisi,sid.subdivisi,sid.proyek,sid.catatan,sid.urutan,sid.idsqdetail,sid.idsodetail,sid.idpidetail,sid.idpldetail,sid.iddodetail,sid.iddrdetail,sid.jmlrnr,sid.statusrnr,sid.jmlsr,sid.statussr,sid.jmlrealisasi,sid.statusrealisasi,sid.isbonus,sid.isbonusfrom,sid.isclose,sid.customtext1,sid.customtext2,sid.customtext3,sid.customdbl1,sid.customdbl2,sid.customdbl3,sid.customdate1,sid.customdate2,sid.customdate3 FROM `m0_si_detail` sid JOIN m5_si si ON si.sicustomint7 = sid.idsi AND si.sinotransaksi = sid.customtext3" & Filter & ")"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
        Catch ex As Exception
            result(1) = 0
            Trans.Rollback()
            result(2) = ex.Message : GoTo selesai
        End Try
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_InsertSIPay(ByVal param As String) As String
        'M0_ExecuteDbFile --------------------------------------------------------
        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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



        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'Filter
        Dim Filter As String = ""
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        If Len(Filter) > 0 Then
            Filter = " where " & Filter
        End If
        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""
        'truncate m0_si
        Try
            'Sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi AND sid.idupload = si.siidupload)"
            Sql = "INSERT INTO m5_si_pay(SELECT 0 as idsicarabayar,si.siid as idsi,sid.carabayar,sid.matauang,sid.kurs,sid.jumlah,sid.jumlahvalas,sid.nogiro,sid.tgljt,sid.bank,sid.noacbank,sid.rekbank,sid.rekgiro,sid.catatan,sid.urutan,sid.isclose FROM `m0_si_pay` sid JOIN m5_si si ON sid.idsi = si.sicustomint7 AND sid.rekgiro = si.sinotransaksi " & Filter & ")"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
        Catch ex As Exception
            result(1) = 0
            Trans.Rollback()
            result(2) = ex.Message : GoTo selesai
        End Try
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_DeleteSIPenampung(ByVal param As String) As String

        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim Filter As String = ""

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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

        'Filter
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""

        Try
            Sql = "delete sid from `m0_si_detail` sid join m0_si si on si.siid = sid.idsi where sid.idsi in" & Filter
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()
        Catch ex As Exception
            result(1) = 0
            Trans.Rollback()
            result(2) = ex.Message : GoTo selesai
        End Try



        Try
            Sql = "delete sid from `m0_si_pay` sid join m0_si si on si.siid = sid.idsi where sid.idsi in" & Filter
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()

        Catch ex As Exception
            result(1) = 0
            Trans.Rollback()
            result(2) = ex.Message : GoTo selesai
        End Try

        Try
            Sql = "delete si from `m0_si` si where si.siid in" & Filter
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()
        Catch ex As Exception
            result(1) = 0
            Trans.Rollback()
            result(2) = ex.Message : GoTo selesai
        End Try


        Trans.Commit()  '*** Commit Transaction ***'
        result(1) = 1
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M12_InsertItemTransaction(ByVal param As String) As String

        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim Filter As String = ""

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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

        'Filter
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""
        Try
            Sql = "INSERT into m1_item_transaction(SELECT 0 as id,si.sicabang as cabang,si.silokasi as lokasi,si.sigudang as gudang,si.sikodepa as kodepa,0 as jenismutasi,si.sisumber as sumber,si.siid as idutama,sid.idsidetail as iddetail,si.sinotransaksi as notransaksi,si.sitgl as tgl,si.sicustomer as kontak,sid.idbarang as idbarang,sid.namabarang as namabarang,sid.tipebarang as tipebarang,i.bhpp as tipehpp,sid.jml as jml,sid.satuan as satuan,sid.jmlbarang as jmlbarang,sid.satuanbarang as satuanbarang,si.simatauang as matauang,si.sikurs as kurs,sid.harga as harga,sid.diskon as diskon,sid.jmldiskon as jmldiskon,0 as idhppikm,0 as idhppikk,0 as sidhppfifo,(CASE si.sihargatermasukpajak WHEN 0 THEN ((sid.jml * sid.harga) - sid.jmldiskon) / sid.jml ELSE ((sid.jml * sid.harga) - sid.jmldiskon - sid.jmlpajak1) / sid.jml END) as hpp,si.siuraian as uraian,si.sicatatan as catatan,sid.catatan as catatandetail,sid.costcenter as costcenter,sid.divisi as divisi,sid.subdivisi as subdivisi,sid.proyek as proyek,0 as saldojml,0 as saldohpp,0 as saldonilai,si.siinputtgl as inputtgl,si.siinputuser as inputuser,si.sipostingtgl as postingtgl,0 as updatehpp,1 as postinghpp,0 as hppfix,0 as postingjurnal,0 as jurnalfix,0 as tutupperiode,0 as isclose,'' as customtext1,'' as customtext2,'' as customtext3,'' as customtext4,'' as customtext5,'' as customtext6,'' as customtext7,'' as customtext8,'' as customtext9,'' as customtext10,'0' as customint1,'0' as customint2,'0' as customint3,'0' as customint4,'0' as customint5,'0' as customint6,'0' as customint7,'0' as customint8,'0' as customint9,'0' as customint10,'0' as customdbl1,'0' as customdbl2,'0' as customdbl3,'0' as customdbl4,'0' as customdbl5,'0' as customdbl6,'0' as customdbl7,'0' as customdbl8,'0' as customdbl9,'0' as customdbl10,'1900-01-01' as customdate1,'1900-01-01' as customdate2,'1900-01-01' as customdate3,'1900-01-01' as customdate4,'1900-01-01' as customdate5,'1900-01-01' as customdate6,'1900-01-01' as customdate7,'1900-01-01' as customdate8,'1900-01-01' as customdate9,'1900-01-01' as customdate10 FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi AND si.sicustomint7 in " & Filter & " join m1_item i ON i.bid = sid.idbarang)"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()
        Catch ex As Exception
            result(2) = ex.Message : GoTo selesai
        End Try


        Trans.Commit()  '*** Commit Transaction ***'
        result(1) = 1
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_CalculatingStock(ByVal param As String) As String

        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim Filter As String = ""

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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

        'Filter
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""
        Try
            Sql = "INSERT INTO m1_item_stock_warehouse(select sid.idbarang, si.sigudang, SUM(sid.jmlbarang * -1) as stok from m5_si_detail sid JOIN m5_si si on si.siid = sid.idsi AND si.sicustomint7 in " & Filter & " GROUP BY si.sigudang, sid.idbarang) ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()
        Catch ex As Exception
            result(2) = ex.Message : GoTo selesai
        End Try


        Trans.Commit()  '*** Commit Transaction ***'
        result(1) = 1
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M12_UpdateGlobalStock(ByVal param As String) As String

        'namaFile

        'On Error GoTo selesai
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim Filter As String = ""

        Dim filename As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan"

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

        'Filter
        pagingSplit = paramSplit(2).Split(sptSubParam)
        Filter = pagingSplit(2)

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET FILENAME
        'filename = paramSplit(5)
        'If Len(filename) < 1 Then
        '    result(2) = "Filename can't be empty." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        'Truncate Table
        Dim Sql As String = ""
        Try
            'Sql = "UPDATE m1_item i JOIN (SELECT isw.idbarang, SUM(isw.stok) as totalstok FROM m1_item_stock_warehouse isw GROUP BY isw.idbarang) as sp ON i.bid = sp.idbarang SET i.bstok = sp.totalstok WHERE i.bstok <> sp.totalstok"
            Sql = "UPDATE m1_item SET bstok = 0; UPDATE m1_item i JOIN (SELECT isw.idbarang, ROUND(SUM(isw.stok),5) as totalstok FROM m1_item_stock_warehouse isw GROUP BY isw.idbarang ) as sp ON i.bid = sp.idbarang SET i.bstok = sp.totalstok WHERE i.bstok <> sp.totalstok;"
            'result(2) = Sql : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = Sql
            End With
            objCmd.ExecuteNonQuery()
        Catch ex As Exception
            result(2) = ex.Message : GoTo selesai
        End Try

        'INSERT MSMQ JURNAL =================================================================
        Dim sumber As String = "SI_POS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
        Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
        'BUAT ID UNIQUE
        mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

        'MSMQ TABEL
        Sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
            & mjid & "', '" & sumber & "', '0', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '0')"
        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
        With objCmd
            .Connection = myConn
            .Transaction = Trans
            .CommandType = CommandType.Text
            .CommandText = Sql
        End With
        objCmd.ExecuteNonQuery()

        'MSMQ ANTRIAN
        Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
        If PostingJurnal.Equals("0") = False Then
            hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
            If Len(hasilMsmq) > 0 Then
                result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
            End If
        End If
        'END OF INSERT MSMQ JURNAL ==========================================================

        Trans.Commit()  '*** Commit Transaction ***'
        result(1) = 1
        objCmd = Nothing


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

        Return wsResult
    End Function
    '
    
End Class