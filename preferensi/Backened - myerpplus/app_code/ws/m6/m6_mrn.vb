Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_mrn
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_MrnSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'mrnid(0) As Integer, mrncabang(1) As String, mrnlokasi(2) As String, mrngudangasal(3) As String, mrngudangproduksi(4) As String, 
        'mrngudangtujuan(5) As String, mrnsumber(6) As String, mrnjenis(7) As String, mrnautonotransaksi(8) As Integer, mrnnotransaksi(9) As String, 
        'mrntgl(10) As Date, mrnkodepa(11) As Integer, mrnbagianmrn(12) As Integer, mrnbagianmrnkontak(13) As String, mrntgldipakai(14) As Date, 
        'mrnestimasikerja(15) As String, mrnmatauang(16) As String, mrnkurs(17) As Double, mrntotalhargain(18) As Double, mrntotalhargaout(19) As Double, 
        'mrntotalhppin(20) As Double, mrntotalhppout(21) As Double, mrnuraian(22) As String, mrncatatan(23) As String, mrnnoref(24) As String, 
        'mrntglnoref(25) As Date, mrnidbom(26) As Integer, mrnidpdr(27) As Integer, mrnidwo(28) As Integer, mrnidmrs(29) As Integer, 
        'mrnstatuspdin(30) As Integer, mrnstatuspdout(31) As Integer, mrnstatus(32) As Integer, mrnstatussebelumnya(33) As Integer, mrnjmlrevisi(34) As Integer, 
        'mrncetakanke(35) As Integer, mrninputuser(36) As Integer, mrninputtgl(37) As DateTime, mrnmodifikasiuser(38) As Integer, mrnmodifikasitgl(39) As DateTime, 
        'mrnisclose(40) As Integer, mrncustomtext1(41) As String, mrncustomtext2(42) As String, mrncustomtext3(43) As String, mrncustomtext4(44) As String, 
        'mrncustomtext5(45) As String, mrncustomint1(46) As Integer, mrncustomint2(47) As Integer, mrncustomint3(48) As Integer, mrncustomdbl1(49) As Double, 
        'mrncustomdbl2(50) As Double, mrncustomdbl3(51) As Double, mrncustomdate1(52) As Date, mrncustomdate2(53) As Date, mrncustomdate3(54) As Date, mrnaktivitas(55) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, 
        'mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, 
        'mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, 
        'mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, 
        'mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, 
        'mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnisclose, mrncustomtext1, 
        'mrncustomtext2, mrncustomtext3, mrncustomtext4, mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, 
        'mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, mrncustomdate1, mrncustomdate2, mrncustomdate3, mrnaktivitas

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 55 And dataUtama.Length <> 56) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'mrnid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "mrnid required numeric." : GoTo selesai
        End If
        'mrnautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "mrnautonotransaksi required numeric." : GoTo selesai
        End If
        'mrntgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "mrntgl required date." : GoTo selesai
        End If
        'mrnkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "mrnkodepa required numeric." : GoTo selesai
        End If
        'mrnbagianmrn(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "mrnbagianmrn required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "mrnbagianmrn can't be empty." : GoTo selesai
        End If
        'mrntgldipakai(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "mrntgldipakai required date." : GoTo selesai
        End If
        'mrnkurs(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "mrnkurs required numeric." : GoTo selesai
        End If
        'mrntotalhargain(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "mrntotalhargain required numeric." : GoTo selesai
        End If
        'mrntotalhargaout(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "mrntotalhargaout required numeric." : GoTo selesai
        End If
        'mrntotalhppin(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "mrntotalhppin required numeric." : GoTo selesai
        End If
        'mrntotalhppout(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "mrntotalhppout required numeric." : GoTo selesai
        End If
        'mrntglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "mrntglnoref required date." : GoTo selesai
        End If
        'mrnidbom(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "mrnidbom required numeric." : GoTo selesai
        End If
        'mrnidpdr(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "mrnidpdr required numeric." : GoTo selesai
        End If
        'mrnidwo(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "mrnidwo required numeric." : GoTo selesai
        End If
        'mrnidmrs(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "mrnidmrs required numeric." : GoTo selesai
        End If
        'mrnstatuspdin(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "mrnstatuspdin required numeric." : GoTo selesai
        End If
        'mrnstatuspdout(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "mrnstatuspdout required numeric." : GoTo selesai
        End If
        'mrnstatus(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "mrnstatus required numeric." : GoTo selesai
        End If
        'mrnstatussebelumnya(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "mrnstatussebelumnya required numeric." : GoTo selesai
        End If
        'mrnjmlrevisi(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "mrnjmlrevisi required numeric." : GoTo selesai
        End If
        'mrncetakanke(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "mrncetakanke required numeric." : GoTo selesai
        End If
        'mrninputuser(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "mrninputuser required numeric." : GoTo selesai
        End If
        'mrninputtgl(37) As DateTime
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "mrninputtgl required date." : GoTo selesai
        End If
        'mrnmodifikasiuser(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "mrnmodifikasiuser required numeric." : GoTo selesai
        End If
        'mrnmodifikasitgl(39) As DateTime
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "mrnmodifikasitgl required date." : GoTo selesai
        End If
        'mrnisclose(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "mrnisclose required numeric." : GoTo selesai
        End If
        'mrncustomint1(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "mrncustomint1 required numeric." : GoTo selesai
        End If
        'mrncustomint2(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "mrncustomint2 required numeric." : GoTo selesai
        End If
        'mrncustomint3(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "mrncustomint3 required numeric." : GoTo selesai
        End If
        'mrncustomdbl1(49) As Double
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "mrncustomdbl1 required numeric." : GoTo selesai
        End If
        'mrncustomdbl2(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "mrncustomdbl2 required numeric." : GoTo selesai
        End If
        'mrncustomdbl3(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "mrncustomdbl3 required numeric." : GoTo selesai
        End If
        'mrncustomdate1(52) As Date
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "mrncustomdate1 required date." : GoTo selesai
        End If
        'mrncustomdate2(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "mrncustomdate2 required date." : GoTo selesai
        End If
        'mrncustomdate3(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "mrncustomdate3 required date." : GoTo selesai
        End If

        If dataUtama.Length > 55 Then
            'mrnaktivitas(55) As Integer
            If (IsNumeric(dataUtama(55)) = False) Then
                result(2) = "mrnaktivitas required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===========================================


        'VALIDASI DATA UTAMA =======================================================
        'mrncabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "mrncabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "mrncabang should not be more than 25 character." : GoTo selesai
        End If

        'mrnlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "mrnlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "mrnlokasi should not be more than 25 character." : GoTo selesai
        End If

        'mrngudangasal(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "mrngudangasal can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "mrngudangasal should not be more than 25 character." : GoTo selesai
        End If

        'mrngudangproduksi(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "mrngudangproduksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "mrngudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'mrngudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "mrngudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "mrngudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'mrnsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "mrnsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "mrnsumber should not be more than 10 character." : GoTo selesai
        End If

        'mrnjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "mrnjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "mrnjenis should not be more than 25 character." : GoTo selesai
        End If

        'mrnnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "mrnnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "mrnnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'mrntgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "mrntgl can't be empty" : GoTo selesai
        End If

        'mrntgldipakai(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "mrntgldipakai can't be empty" : GoTo selesai
        End If

        'mrnmatauang(16) As String
        If Len(dataUtama(16)) = 0 Then
            result(2) = "mrnmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(16)) > 25 Then
            result(2) = "mrnmatauang should not be more than 25 character." : GoTo selesai
        End If

        'mrnkurs(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "mrnkurs can't be empty" : GoTo selesai
        End If

        'mrntotalhargain(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "mrntotalhargain can't be empty" : GoTo selesai
        End If

        'mrntotalhargaout(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "mrntotalhargaout can't be empty" : GoTo selesai
        End If

        'mrntotalhppin(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "mrntotalhppin can't be empty" : GoTo selesai
        End If

        'mrntotalhppout(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "mrntotalhppout can't be empty" : GoTo selesai
        End If

        'mrntglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "mrntglnoref can't be empty" : GoTo selesai
        End If

        'mrninputtgl(37) As DateTime
        If Len(dataUtama(37)) = 0 Then
            result(2) = "mrninputtgl can't be empty" : GoTo selesai
        End If

        'mrnmodifikasitgl(39) As DateTime
        If Len(dataUtama(39)) = 0 Then
            result(2) = "mrnmodifikasitgl can't be empty" : GoTo selesai
        End If

        'mrncustomdbl1(49) As Double
        If Len(dataUtama(49)) = 0 Then
            result(2) = "mrncustomdbl1 can't be empty" : GoTo selesai
        End If

        'mrncustomdbl2(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "mrncustomdbl2 can't be empty" : GoTo selesai
        End If

        'mrncustomdbl3(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "mrncustomdbl3 can't be empty" : GoTo selesai
        End If

        'mrncustomdate1(52) As Date
        If Len(dataUtama(52)) = 0 Then
            result(2) = "mrncustomdate1 can't be empty" : GoTo selesai
        End If

        'mrncustomdate2(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "mrncustomdate2 can't be empty" : GoTo selesai
        End If

        'mrncustomdate3(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "mrncustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "mrnid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrngudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrngudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrngudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnbagianmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnbagianmrnkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnidbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnidpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnidwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnidmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatuspdin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatuspdout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrninputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrninputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnaktivitas", AsEnumTypeData.AsInt64)
        If dataUtama.Length > 55 Then
            If AsDataTableTambahData(dtutama, "mrnid~mrncabang~mrnlokasi~mrngudangasal~mrngudangproduksi~mrngudangtujuan~mrnsumber~mrnjenis~mrnautonotransaksi~mrnnotransaksi~mrntgl~mrnkodepa~mrnbagianmrn~mrnbagianmrnkontak~mrntgldipakai~mrnestimasikerja~mrnmatauang~mrnkurs~mrntotalhargain~mrntotalhargaout~mrntotalhppin~mrntotalhppout~mrnuraian~mrncatatan~mrnnoref~mrntglnoref~mrnidbom~mrnidpdr~mrnidwo~mrnidmrs~mrnstatuspdin~mrnstatuspdout~mrnstatus~mrnstatussebelumnya~mrnjmlrevisi~mrncetakanke~mrninputuser~mrninputtgl~mrnmodifikasiuser~mrnmodifikasitgl~mrnisclose~mrncustomtext1~mrncustomtext2~mrncustomtext3~mrncustomtext4~mrncustomtext5~mrncustomint1~mrncustomint2~mrncustomint3~mrncustomdbl1~mrncustomdbl2~mrncustomdbl3~mrncustomdate1~mrncustomdate2~mrncustomdate3~mrnaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "mrnid~mrncabang~mrnlokasi~mrngudangasal~mrngudangproduksi~mrngudangtujuan~mrnsumber~mrnjenis~mrnautonotransaksi~mrnnotransaksi~mrntgl~mrnkodepa~mrnbagianmrn~mrnbagianmrnkontak~mrntgldipakai~mrnestimasikerja~mrnmatauang~mrnkurs~mrntotalhargain~mrntotalhargaout~mrntotalhppin~mrntotalhppout~mrnuraian~mrncatatan~mrnnoref~mrntglnoref~mrnidbom~mrnidpdr~mrnidwo~mrnidmrs~mrnstatuspdin~mrnstatuspdout~mrnstatus~mrnstatussebelumnya~mrnjmlrevisi~mrncetakanke~mrninputuser~mrninputtgl~mrnmodifikasiuser~mrnmodifikasitgl~mrnisclose~mrncustomtext1~mrncustomtext2~mrncustomtext3~mrncustomtext4~mrncustomtext5~mrncustomint1~mrncustomint2~mrncustomint3~mrncustomdbl1~mrncustomdbl2~mrncustomdbl3~mrncustomdate1~mrncustomdate2~mrncustomdate3~mrnaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If


        'Variabel BatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, jmlbarang As Double = 0, idmrsout As Integer = 0

        Dim ftExistOutstandingMrsOut As String = "", ftOutstandingMrsOut As String = ""
        Dim updNilaiMrsOut As String = "", updFilterMrsOut As String = ""

        Dim ftExistStok As String = "", ftStok As String = ""
        Dim updStokOut As String = "", gudangOut As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idmrnout(0) As Integer, idmrn(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususkeluar(14) As Integer, 
        'idhppfifokeluar(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idbomout(28) As Integer, idpdrout(29) As Integer, 
        'idwoout(30) As Integer, idmrsout(31) As Integer, jmlpd(32) As Double, statuspd(33) As Integer, isclose(34) As Integer, 
        'customtext1(35) As String, customtext2(36) As String, customtext3(37) As String, customdbl1(38) As Double, customdbl2(39) As Double, 
        'customdbl3(40) As Double, customdate1(41) As Date, customdate2(42) As Date, customdate3(43) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable DETAIL
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idmrnout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususkeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifokeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbomout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpdrout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idwoout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idmrsout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim Jmldtdetail As Integer = dataDetail.Length
        For i = 1 To Jmldtdetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 44) Then
                result(2) = "Detail Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idmrnout(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail Row : " & i & " - idmrnout required numeric." : GoTo selesai
            End If
            'idmrn(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail Row : " & i & " - idmrn required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Detail Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Detail Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Detail Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idhppkhususkeluar(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Detail Row : " & i & " - idhppkhususkeluar required numeric." : GoTo selesai
            End If
            'idhppfifokeluar(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Detail Row : " & i & " - idhppfifokeluar required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Detail Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idbomout(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail Row : " & i & " - idbomout required numeric." : GoTo selesai
            End If
            'idpdrout(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail Row : " & i & " - idpdrout required numeric." : GoTo selesai
            End If
            'idwoout(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail Row : " & i & " - idwoout required numeric." : GoTo selesai
            End If
            'idmrsout(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail Row : " & i & " - idmrsout required numeric." : GoTo selesai
            End If
            'jmlpd(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(41) As Date
            If (IsDate(dataRowDetail(41)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(42) As Date
            If (IsDate(dataRowDetail(42)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(43) As Date
            If (IsDate(dataRowDetail(43)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Detail Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(16) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Detail Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Detail Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(19) As String
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Detail Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(19)) > 25 Then
                result(2) = "Detail Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(20) As String
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Detail Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Detail Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(21) As String
            'If Len(dataRowDetail(21)) = 0 Then
            '    result(2) = "Detail Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(21)) > 25 Then
                result(2) = "Detail Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmlpd(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(41) As Date
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(42) As Date
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(43) As Date
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idmrnout~idmrn~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususkeluar~idhppfifokeluar~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomout~idpdrout~idwoout~idmrsout~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idmrsout(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idmrsout = dataRowDetail(31)
            'gudangtujuan(21) As String    , gudangproduksi(20) As String
            gudangIn = dataRowDetail(21) : gudangOut = dataRowDetail(20)

            'Filter barang (serial batch)
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI OUTSTANDING -------------------------
            'MRS
            If idmrsout <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstandingMrsOut = IIf(Len(ftExistOutstandingMrsOut.ToString) = 0, "", ftExistOutstandingMrsOut & " UNION ")
                ftExistOutstandingMrsOut = String.Concat(ftExistOutstandingMrsOut, "SELECT EXISTS(SELECT 1 FROM m6_mrs_out JOIN m6_mrs ON idmrs = mrsid WHERE idmrsout = '" & idmrsout & "' AND (mrsstatus = 2 OR mrsstatus = 3 OR mrsstatus = 4 OR mrsstatus = 7) LIMIT 1) as rowExists, '" & idmrsout & "' as idmrsout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idmrsout=" & idmrsout)
                ftOutstandingMrsOut = IIf(Len(ftOutstandingMrsOut.ToString) = 0, "", ftOutstandingMrsOut & " OR ")
                ftOutstandingMrsOut = String.Concat(ftOutstandingMrsOut, " (mrsout.idmrsout = " & idmrsout & " AND " & Outstanding & " > (mrsout.jmlbarang - mrsout.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiMrsOut = String.Concat("WHEN '" & idmrsout & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiMrsOut)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterMrsOut = IIf(Len(updFilterMrsOut.ToString) = 0, "", updFilterMrsOut & " OR ")
                updFilterMrsOut = String.Concat(updFilterMrsOut, "(idmrsout = '" & idmrsout & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangproduksi='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA BATCH -------------------------------------------------------
        'nbtid(0) As Integer, nbtjenismutasi(1) As Integer, nbtidbarang(2) As Integer, nbtkode(3) As String, nbtsumber(4) As String, 
        'nbtidtransaksi(5) As Integer, nbtsatuan(6) As String, nbtjml(7) As Double, nbtcustomtext1(8) As String, nbtcustomtext2(9) As String, 
        'nbtcustomtext3(10) As String, nbtcustomdbl1(11) As Double, nbtcustomdbl2(12) As Double, nbtcustomdbl3(13) As Double, nbtcustomdate1(14) As Date, 
        'nbtcustomdate2(15) As Date, nbtcustomdate3(16) As Date, nbtgudang(17) As String, nbtidbatchin(18) As Integer

        'MAPPING BUAT FLEX DATA BATCH -----------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin

        'Buat datatable BATCH
        Dim dtbatch As New DataTable
        AsDataTableTambahField(dtbatch, "nbtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbatch, "nbtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidbatchin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

        'CEK PARAMETER DATA BATCH
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA BATCH ======================================================
            'SPLIT PARAMETER DATA BATCH
            dataBatch = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA BATCH ===============================================

            'VALIDASI DAN SET DATA ROW BATCH ==================================================
            Dim JmlDtBatch As Integer = dataBatch.Length
            For i = 1 To JmlDtBatch
                'SPLIT DATA DETAIL
                dataRowBatch = dataBatch(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA BATCH -----------------------------------
                'CEK ARRAY DATA BATCH
                If (dataRowBatch.Length <> 19) Then
                    result(2) = "Batch Row : " & i & " - Invalid batch number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowBatch(1) = 0
                If (IsNumeric(dataRowBatch(1)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjenismutasi required numeric." : GoTo selesai
                End If
                'nbtidbarang(2) As Integer
                If (IsNumeric(dataRowBatch(2)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbarang required numeric." : GoTo selesai
                End If
                'nbtidtransaksi(5) As Integer
                If (IsNumeric(dataRowBatch(5)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidtransaksi required numeric." : GoTo selesai
                End If
                'nbtjml(7) As Double
                If (IsNumeric(dataRowBatch(7)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjml required numeric." : GoTo selesai
                End If
                'nbtcustomdbl1(11) As Double
                If (IsNumeric(dataRowBatch(11)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl2(12) As Double
                If (IsNumeric(dataRowBatch(12)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl3(13) As Double
                If (IsNumeric(dataRowBatch(13)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 required numeric." : GoTo selesai
                End If
                'nbtcustomdate1(14) As Date
                If (IsDate(dataRowBatch(14)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 required date." : GoTo selesai
                End If
                'nbtcustomdate2(15) As Date
                If (IsDate(dataRowBatch(15)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 required date." : GoTo selesai
                End If
                'nbtcustomdate3(16) As Date
                If (IsDate(dataRowBatch(16)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 required date." : GoTo selesai
                End If
                'nbtidbatchin(18) As Integer
                If (IsNumeric(dataRowBatch(18)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbatchin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA BATCH -----------------------------------

                'VALIDASI DATA BATCH ---------------------------------------
                'nbtkode(3) As String
                If Len(dataRowBatch(3)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(3)) > 100 Then
                    result(2) = "Batch Row : " & i & " - nbtkode should not be more than 100 character." : GoTo selesai
                End If

                'nbtsumber(4) As String
                If Len(dataRowBatch(4)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(4)) > 10 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber should not be more than 10 character." : GoTo selesai
                End If

                'nbtsatuan(6) As String
                If Len(dataRowBatch(6)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(6)) > 25 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nbtjml(7) As Double
                If Len(dataRowBatch(7)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtjml can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl1(11) As Double
                If Len(dataRowBatch(11)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl2(12) As Double
                If Len(dataRowBatch(12)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl3(13) As Double
                If Len(dataRowBatch(13)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate1(14) As Date
                If Len(dataRowBatch(14)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate2(15) As Date
                If Len(dataRowBatch(15)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate3(16) As Date
                If Len(dataRowBatch(16)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 can't be empty" : GoTo selesai
                End If

                'nbtgudang(17) As String
                If Len(dataRowBatch(17)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA BATCH --------------------------------

                If AsDataTableTambahData(dtbatch, "nbtid~nbtjenismutasi~nbtidbarang~nbtkode~nbtsumber~nbtidtransaksi~nbtsatuan~nbtjml~nbtcustomtext1~nbtcustomtext2~nbtcustomtext3~nbtcustomdbl1~nbtcustomdbl2~nbtcustomdbl3~nbtcustomdate1~nbtcustomdate2~nbtcustomdate3~nbtgudang~nbtidbatchin", dataRowBatch(0) & "~" & dataRowBatch(1) & "~" & dataRowBatch(2) & "~" & dataRowBatch(3) & "~" & dataRowBatch(4) & "~" & dataRowBatch(5) & "~" & dataRowBatch(6) & "~" & dataRowBatch(7) & "~" & dataRowBatch(8) & "~" & dataRowBatch(9) & "~" & dataRowBatch(10) & "~" & dataRowBatch(11) & "~" & dataRowBatch(12) & "~" & dataRowBatch(13) & "~" & dataRowBatch(14) & "~" & dataRowBatch(15) & "~" & dataRowBatch(16) & "~" & dataRowBatch(17) & "~" & dataRowBatch(18)) = False Then
                    result(2) = "Batch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA BATCH ===========================================

        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'nstid(0) As Integer, nstjenismutasi(1) As Integer, nstidbarang(2) As Integer, nstkode(3) As String, nstsumber(4) As String, 
        'nstidtransaksi(5) As Integer, nstsatuan(6) As String, nstjml(7) As Double, nstcustomtext1(8) As String, nstcustomtext2(9) As String, 
        'nstcustomtext3(10) As String, nstcustomdbl1(11) As Double, nstcustomdbl2(12) As Double, nstcustomdbl3(13) As Double, nstcustomdate1(14) As Date, 
        'nstcustomdate2(15) As Date, nstcustomdate3(16) As Date, nstgudang(17) As String, nstidserialin(18) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'nstid, nstjenismutasi, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, nstgudang, nstidserialin

        'Buat datatable serial
        Dim dtserial As New DataTable
        AsDataTableTambahField(dtserial, "nstid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtserial, "nstcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidserialin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

        'CEK PARAMETER DATA SERIAL
        If dataSplit(3).Length > 0 Then
            'VALIDASI DAN SET DATA SERIAL ======================================================
            'SPLIT PARAMETER DATA SERIAL
            dataSerial = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA SERIAL ===============================================

            'VALIDASI DAN SET DATA ROW SERIAL ==================================================
            Dim JmlDtSerial As Integer = dataSerial.Length
            For i = 1 To JmlDtSerial
                'SPLIT DATA SERIAL
                dataRowSerial = dataSerial(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA SERIAL -----------------------------------
                'CEK ARRAY DATA SERIAL
                If (dataRowSerial.Length <> 19) Then
                    result(2) = "Serial Row : " & i & " - Invalid serial number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW SERIAL ----------------------------

                'VALIDASI TIPE DATA SERIAL ------------------------------------------
                'nstid(0) As Integer
                If (IsNumeric(dataRowSerial(0)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstid required numeric." : GoTo selesai
                End If
                'nstjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowSerial(1) = 0
                If (IsNumeric(dataRowSerial(1)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjenismutasi required numeric." : GoTo selesai
                End If
                'nstidbarang(2) As Integer
                If (IsNumeric(dataRowSerial(2)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidbarang required numeric." : GoTo selesai
                End If
                'nstidtransaksi(5) As Integer
                If (IsNumeric(dataRowSerial(5)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidtransaksi required numeric." : GoTo selesai
                End If
                'nstjml(7) As Double
                If (IsNumeric(dataRowSerial(7)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjml required numeric." : GoTo selesai
                End If
                'nstcustomdbl1(11) As Double
                If (IsNumeric(dataRowSerial(11)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 required numeric." : GoTo selesai
                End If
                'nstcustomdbl2(12) As Double
                If (IsNumeric(dataRowSerial(12)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 required numeric." : GoTo selesai
                End If
                'nstcustomdbl3(13) As Double
                If (IsNumeric(dataRowSerial(13)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 required numeric." : GoTo selesai
                End If
                'nstcustomdate1(14) As Date
                If (IsDate(dataRowSerial(14)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 required date." : GoTo selesai
                End If
                'nstcustomdate2(15) As Date
                If (IsDate(dataRowSerial(15)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 required date." : GoTo selesai
                End If
                'nstcustomdate3(16) As Date
                If (IsDate(dataRowSerial(16)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 required date." : GoTo selesai
                End If
                'nstidserialin(18) As Integer
                If (IsNumeric(dataRowSerial(18)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidserialin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA SERIAL -----------------------------------

                'VALIDASI DATA SERIAL ---------------------------------------
                'nstkode(3) As String
                If Len(dataRowSerial(3)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(3)) > 100 Then
                    result(2) = "Serial Row : " & i & " - nstkode should not be more than 100 character." : GoTo selesai
                End If

                'nstsumber(4) As String
                If Len(dataRowSerial(4)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(4)) > 10 Then
                    result(2) = "Serial Row : " & i & " - nstsumber should not be more than 10 character." : GoTo selesai
                End If

                'nstsatuan(6) As String
                If Len(dataRowSerial(6)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(6)) > 25 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nstjml(7) As Double
                If Len(dataRowSerial(7)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstjml can't be empty" : GoTo selesai
                End If

                'nstcustomdbl1(11) As Double
                If Len(dataRowSerial(11)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl2(12) As Double
                If Len(dataRowSerial(12)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl3(13) As Double
                If Len(dataRowSerial(13)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nstcustomdate1(14) As Date
                If Len(dataRowSerial(14)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 can't be empty" : GoTo selesai
                End If

                'nstcustomdate2(15) As Date
                If Len(dataRowSerial(15)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 can't be empty" : GoTo selesai
                End If

                'nstcustomdate3(16) As Date
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 can't be empty" : GoTo selesai
                End If

                'nstgudang(17) As String
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA SERIAL --------------------------------

                If AsDataTableTambahData(dtserial, "nstid~nstjenismutasi~nstidbarang~nstkode~nstsumber~nstidtransaksi~nstsatuan~nstjml~nstcustomtext1~nstcustomtext2~nstcustomtext3~nstcustomdbl1~nstcustomdbl2~nstcustomdbl3~nstcustomdate1~nstcustomdate2~nstcustomdate3~nstgudang~nstidserialin", dataRowSerial(0) & "~" & dataRowSerial(1) & "~" & dataRowSerial(2) & "~" & dataRowSerial(3) & "~" & dataRowSerial(4) & "~" & dataRowSerial(5) & "~" & dataRowSerial(6) & "~" & dataRowSerial(7) & "~" & dataRowSerial(8) & "~" & dataRowSerial(9) & "~" & dataRowSerial(10) & "~" & dataRowSerial(11) & "~" & dataRowSerial(12) & "~" & dataRowSerial(13) & "~" & dataRowSerial(14) & "~" & dataRowSerial(15) & "~" & dataRowSerial(16) & "~" & dataRowSerial(17) & "~" & dataRowSerial(18)) = False Then
                    result(2) = "Serial Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
        End If


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 6, vMenuId As Integer = 7
                Select Case drutama("mrnstatus")
                    Case 0 : vAkses = 0
                    Case 1 : vAkses = 0
                    Case 2 : vAkses = 8
                    Case 3 : vAkses = 0
                    Case 4 : vAkses = 0
                    Case 5 : vAkses = 0
                    Case 6 : vAkses = 0
                    Case 7 : vAkses = 0
                    Case 8 : vAkses = 4
                    Case 9 : vAkses = 5
                    Case 10 : vAkses = 6
                    Case 11 : vAkses = 7
                    Case 12 : vAkses = 0
                End Select
                msgAkses = HakAkses(vModuleId, vMenuId, vAkses, userid)
                If Len(msgAkses) > 0 Then
                    result(2) = msgAkses : Trans.Rollback() : GoTo selesai
                End If
                'END OF CEK HAK AKSES STATUS =====================


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("mrntgl")), AsFormatTanggal(drutama("mrntgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                If drutama("mrnstatus") = 2 Or drutama("mrnstatus") = 1 Or drutama("mrnstatus") = 8 Or drutama("mrnstatus") = 9 Or drutama("mrnstatus") = 10 Or drutama("mrnstatus") = 11 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingMrsOut, ftOutstandingMrsOut, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangproduksi")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("Mrnid")
                    notransaksi = drutama("Mrnnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(Mrnid), Mrnnotransaksi FROM M6_Mrn WHERE Mrnid='" & result(4) & "' AND mrnstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("mrnautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Mrncabang"), drutama("Mrnlokasi"), drutama("Mrnsumber"), drutama("Mrntgl"))
                            Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                            arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                            'cek success generate notransaksi
                            If (arrNotransaksi(0) = 1) Then
                                notransaksi = arrNotransaksi(2)
                                'tambah query update m0_nomor_next
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = arrNotransaksi(3)
                                End With
                                objCmd.ExecuteNonQuery()
                            Else
                                result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF GENERATE NOTRANSAKSI ==================================

                        End If

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(Mrnid) FROM M6_Mrn WHERE Mrnnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_mrn_history
                        Dim rsSimpanHistory As String = SimpanHistory.m6_Mrn_HistorySimpan("" & paramSplit(0) & "★M6_Mrn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("mrnsumber")) & "▼" & FixQuotes(drutama("mrnid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Mrn set mrncabang  = '" & FixQuotes(drutama("mrncabang")) & "', mrnlokasi  = '" & FixQuotes(drutama("mrnlokasi")) & "', mrngudangasal  = '" & FixQuotes(drutama("mrngudangasal")) & "', mrngudangproduksi  = '" & FixQuotes(drutama("mrngudangproduksi")) & "', mrngudangtujuan  = '" & FixQuotes(drutama("mrngudangtujuan")) & "', mrnsumber  = '" & FixQuotes(drutama("mrnsumber")) & "', mrnjenis  = '" & FixQuotes(drutama("mrnjenis")) & "', mrnautonotransaksi  = " & drutama("mrnautonotransaksi") & ", mrnnotransaksi  = '" & FixQuotes(notransaksi) & "', mrntgl  = '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', mrnkodepa  = " & drutama("mrnkodepa") & ", mrnbagianmrn  = " & drutama("mrnbagianmrn") & ", mrnbagianmrnkontak  = '" & FixQuotes(drutama("mrnbagianmrnkontak")) & "', mrntgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("mrntgldipakai"))) & "', mrnestimasikerja  = '" & FixQuotes(drutama("mrnestimasikerja")) & "', mrnmatauang  = '" & FixQuotes(drutama("mrnmatauang")) & "', mrnkurs  = '" & FixDouble(drutama("mrnkurs")) & "', mrntotalhargain  = '" & FixDouble(drutama("mrntotalhargain")) & "', mrntotalhargaout  = '" & FixDouble(drutama("mrntotalhargaout")) & "', mrntotalhppin  = '" & FixDouble(drutama("mrntotalhppin")) & "', mrntotalhppout  = '" & FixDouble(drutama("mrntotalhppout")) & "', mrnuraian  = '" & FixQuotes(drutama("mrnuraian")) & "', mrncatatan  = '" & FixQuotes(drutama("mrncatatan")) & "', mrnnoref  = '" & FixQuotes(drutama("mrnnoref")) & "', mrntglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("mrntglnoref"))) & "', mrnidbom  = " & drutama("mrnidbom") & ", mrnidpdr  = " & drutama("mrnidpdr") & ", mrnidwo  = " & drutama("mrnidwo") & ", mrnidmrs  = " & drutama("mrnidmrs") & ", mrnstatuspdin  = " & drutama("mrnstatuspdin") & ", mrnstatuspdout  = " & drutama("mrnstatuspdout") & ", mrnstatus  = " & drutama("mrnstatus") & ", mrnstatussebelumnya  = " & drutama("mrnstatussebelumnya") & ", mrnjmlrevisi  = mrnjmlrevisi+1, mrncetakanke  = " & drutama("mrncetakanke") & ", mrnmodifikasiuser  = " & drutama("mrnmodifikasiuser") & ", mrnmodifikasitgl  = NOW(), mrncustomtext1  = '" & FixQuotes(drutama("mrncustomtext1")) & "', mrncustomtext2  = '" & FixQuotes(drutama("mrncustomtext2")) & "', mrncustomtext3  = '" & FixQuotes(drutama("mrncustomtext3")) & "', mrncustomtext4  = '" & FixQuotes(drutama("mrncustomtext4")) & "', mrncustomtext5  = '" & FixQuotes(drutama("mrncustomtext5")) & "', mrncustomint1  = " & drutama("mrncustomint1") & ", mrncustomint2  = " & drutama("mrncustomint2") & ", mrncustomint3  = " & drutama("mrncustomint3") & ", mrncustomdbl1  = '" & FixDouble(drutama("mrncustomdbl1")) & "', mrncustomdbl2  = '" & FixDouble(drutama("mrncustomdbl2")) & "', mrncustomdbl3  = '" & FixDouble(drutama("mrncustomdbl3")) & "', mrncustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate1"))) & "', mrncustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate2"))) & "', mrncustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate3"))) & "', mrnaktivitas = '" & FixDouble(drutama("mrnaktivitas")) & "' where mrnid = '" & drutama("mrnid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("Mrnautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Mrncabang"), drutama("Mrnlokasi"), drutama("Mrnsumber"), drutama("Mrntgl"))
                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksi(0) = 1) Then
                            notransaksi = arrNotransaksi(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNotransaksi(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOTRANSAKSI ==================================

                    Else
                        notransaksi = drutama("Mrnnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(Mrnid) FROM m6_mrn WHERE Mrnnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Mrn (mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnisclose, mrncustomtext1, mrncustomtext2, mrncustomtext3, mrncustomtext4, mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, mrncustomdate1, mrncustomdate2, mrncustomdate3, mrnaktivitas) values('" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(drutama("mrngudangasal")) & "', '" & FixQuotes(drutama("mrngudangproduksi")) & "', '" & FixQuotes(drutama("mrngudangtujuan")) & "', '" & FixQuotes(drutama("mrnsumber")) & "', '" & FixQuotes(drutama("mrnjenis")) & "', " & drutama("mrnautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnkodepa") & ", " & drutama("mrnbagianmrn") & ", '" & FixQuotes(drutama("mrnbagianmrnkontak")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgldipakai"))) & "', '" & FixQuotes(drutama("mrnestimasikerja")) & "', '" & FixQuotes(drutama("mrnmatauang")) & "', '" & FixDouble(drutama("mrnkurs")) & "', '" & FixDouble(drutama("mrntotalhargain")) & "', '" & FixDouble(drutama("mrntotalhargaout")) & "', '" & FixDouble(drutama("mrntotalhppin")) & "', '" & FixDouble(drutama("mrntotalhppout")) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(drutama("mrnnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntglnoref"))) & "', " & drutama("mrnidbom") & ", " & drutama("mrnidpdr") & ", " & drutama("mrnidwo") & ", " & drutama("mrnidmrs") & ", " & drutama("mrnstatuspdin") & ", " & drutama("mrnstatuspdout") & ", " & drutama("mrnstatus") & ", " & drutama("mrnstatussebelumnya") & ", " & drutama("mrnjmlrevisi") & ", " & drutama("mrncetakanke") & ", " & drutama("mrninputuser") & ", NOW(), " & drutama("mrnmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("mrnisclose") & ", '" & FixQuotes(drutama("mrncustomtext1")) & "', '" & FixQuotes(drutama("mrncustomtext2")) & "', '" & FixQuotes(drutama("mrncustomtext3")) & "', '" & FixQuotes(drutama("mrncustomtext4")) & "', '" & FixQuotes(drutama("mrncustomtext5")) & "', " & drutama("mrncustomint1") & ", " & drutama("mrncustomint2") & ", " & drutama("mrncustomint3") & ", '" & FixDouble(drutama("mrncustomdbl1")) & "', '" & FixDouble(drutama("mrncustomdbl2")) & "', '" & FixDouble(drutama("mrncustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate3"))) & "', '" & FixDouble(drutama("mrnaktivitas")) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDBCon("select Mrnid from M6_Mrn where Mrnnotransaksi='" & notransaksi & "' AND Mrninputuser= '" & userid & "' order by Mrnmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Mrn_Out where idMrn = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idmrnout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususkeluar") & ", " & dr1("idhppfifokeluar") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomout") & ", " & dr1("idpdrout") & ", " & dr1("idwoout") & ", " & dr1("idmrsout") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Mrn_Out(idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'MRN'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses batch
                If (dtbatch.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nbtjenismutasi") & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_Transaction(nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus serial ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'MRN'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses serial
                If (dtserial.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nstjenismutasi") & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_Transaction(nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("mrnstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ===================================================
                    Dim updNilaiMrsUtamaOut = "", updFilterMrsUtama = ""

                    'MRS OUT
                    If Len(updNilaiMrsOut) > 0 Then
                        'UPDATE DETAIL OUT
                        sql = "UPDATE m6_mrs_out SET jmlrealisasi = (CASE idmrsout " & updNilaiMrsOut & " ELSE jmlrealisasi END) WHERE " & updFilterMrsOut
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'AMBIL ID UTAMA OUT
                        Dim ftDetail As String = ""
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idmrs FROM m6_mrs_out WHERE " & updFilterMrsOut & " GROUP BY idmrs", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idmrs = '" & dr1("idmrs") & "')")
                            Next
                        End If

                        'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                        If Len(ftDetail) > 0 Then
                            dtOut = AsDataTableAmbilDariDBCon("SELECT idmrs, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_mrs_out WHERE " & ftDetail & " GROUP BY idmrs", myConn)
                            If dtOut.Rows.Count > 0 Then
                                For Each dr1 As DataRow In dtOut.Rows
                                    '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                    Dim statusOut As Integer = 0
                                    If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                        statusOut = 2
                                    ElseIf dr1("jmlrealisasi") < 1 Then
                                        statusOut = 0
                                    Else
                                        statusOut = 1
                                    End If

                                    '2. SET NILAI UPDATE OUTSTANDING
                                    updNilaiMrsUtamaOut = String.Concat(updNilaiMrsUtamaOut, "WHEN '" & dr1("idmrs") & "' THEN '" & statusOut & "' ")

                                    '3. SET FILTERUPDATE OUTSTANDING
                                    updFilterMrsUtama = IIf(Len(updFilterMrsUtama.ToString) = 0, "", updFilterMrsUtama & " OR ")
                                    updFilterMrsUtama = String.Concat(updFilterMrsUtama, "(mrsid = '" & dr1("idmrs") & "')")
                                Next
                            End If
                        End If
                    End If

                    'MRS UTAMA, UPDATE STATUS OUT
                    If Len(updNilaiMrsUtamaOut) > 0 Then
                        sql = "UPDATE m6_mrs SET mrsstatusrealisasiout = (CASE mrsid " & updNilaiMrsUtamaOut & " ELSE mrsstatusrealisasiout END) WHERE " & updFilterMrsUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'AMBIL GUDANG TUJUAN DARI UTAMA ================================================
                    'GUDANG TUJUAN UTAMA DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG PRODUKSI 'A', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'A' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TUJUAN BERTAMBAH
                    Dim SetGudang As String = drutama("mrngudangtujuan")
                    'END OF AMBIL GUDANG TUJUAN DARI UTAMA =========================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO BATCH =========================================================

                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO SERIAL ========================================================


                    'UPDATE STOK ====================================================================
                    'STOK KELUAR
                    If Len(updStokOut) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'STOK MASUK
                    If Len(updStokIn) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT mrno.idmrnout, mrno.idbarang, mrno.namabarang, mrno.tipebarang, mrno.jml, mrno.satuan, mrno.jmlbarang, mrno.satuanbarang, mrno.matauang, mrno.kurs, mrno.harga, mrno.hpp, mrno.idhppkhususkeluar, mrno.gudangasal, mrno.gudangproduksi, mrno.gudangtujuan, mrno.catatan, mrno.costcenter, mrno.divisi, mrno.subdivisi, mrno.proyek, mrn.mrninputtgl, i.bhpp FROM m6_mrn_out mrno JOIN m6_mrn mrn ON mrno.idmrn = mrn.mrnid JOIN m1_item i ON mrno.idbarang = i.bid WHERE mrno.idmrn = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("hpp"))

                            'POSTING BARANG KELUAR (gudangproduksi)
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                              cabang,                                    lokasi,                                     gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,        idhppikm,          idhppikk,                               hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', " & drutama("mrnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrnsumber")) & "', " & result(4) & ", " & dr1("idmrnout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnbagianmrn") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangtujuan)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                              cabang,                                    lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,        idhppikm,          idhppikk,                               hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("mrnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrnsumber")) & "', " & result(4) & ", " & dr1("idmrnout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnbagianmrn") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "MRN", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'ambil moduleid dan menuid dari m0_nomor
                Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'", myConn)
                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

                sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF INSERT USER LOG =============================================================

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'myconn.Close()
        'myconn = Nothing
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
    Public Function M6_MrnUpdateStatus(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", nilaiStatus As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"
        Dim idtransaksi As String = "", idtransaksih As String = ""
        Dim dtdetail As DataTable, dtdetailOut As DataTable
        Dim isDelete As Boolean = False

        Dim Filter As String = "", Sorting As String = "", search As String = ""

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISDELETE =========================================================
        'CEK ISDELETE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isdelete required numeric." : GoTo selesai
        Else
            'SET ISDELETE
            If (Val(paramSplit(4)) = 1) Then
                isDelete = True
            Else
                isDelete = False
            End If
        End If
        'END OF VALIDASI DAN SET ISDELETE ==================================================

        'VALIDASI DAN SET NILAISTATUS ======================================================
        'SPILIT PARAMETER NILAISTATUS
        nilaiSplit = paramSplit(5).Split(sptSubParam)

        'CEK ARRAY NILAISTATUS
        If (nilaiSplit.Length <> 2) Then
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If

        'CEK IDTRANSAKSI
        If (IsNumeric(nilaiSplit(0)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = nilaiSplit(0)

        'SET NILAI STATUS
        If (Len(nilaiSplit(1)) > 0) Then
            'JIKA NUMERIC MAKA NILAISTATUS = PARAM NILAI STATUS YG DIINPUT
            'JIKA TIDAK MAKA NILAISTATUS = UNCLOSE
            If (IsNumeric(nilaiSplit(1)) = True) Then
                nilaiStatus = nilaiSplit(1)
                'JIKA NILAI STATUS < 0 ATAU NILAI STATUS > 12 MAKA NILAISTATUS TIDAK VALID
                If (nilaiStatus < 0 Or nilaiStatus > 12) Then
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            Else
                If (nilaiSplit(1).ToString.ToLower = "unclose") Then
                    nilaiStatus = "unclose"
                Else
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            End If
        Else
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET NILAISTATUS ================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "MRN", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Mrntgl, Mrnnotransaksi, Mrnstatus FROM M6_Mrn WHERE Mrnid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Mrnstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True


            'CEK PERIODE AKUNTANSI ==============================================================
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m6_mrn_history
            Dim rsSimpanHistory As String = SimpanHistory.m6_Mrn_HistorySimpan("" & paramSplit(0) & "★M6_Mrn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                ''CEK TERKAIT ====================================================================
                ''PANGGIL QUERY TERKAIT
                'Dim query As New m0_query
                'sql = query.PanggilQuery("m6_mrn_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                ''END OF CEK TERKAIT =============================================================


                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                Dim updNilaiMrsUtamaOut = "", updFilterMrsUtama = ""
                Dim idbarang As Integer = 0, jmlbarang As Double = 0
                Dim idmrsout As Integer = 0
                Dim updNilaiMrsOut As String = "", updFilterMrsOut As String = ""

                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updStokOut As String = "", gudangOut As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""

                'AMBIL DATA DETAIL OUT
                dtdetailOut = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudangasal, gudangproduksi, gudangtujuan, idmrsout, urutan FROM m6_mrn_out WHERE idmrn = '" & idtransaksi & "'", myConn)
                If dtdetailOut.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetailOut.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudangIn = dr1("gudangproduksi") : gudangOut = dr1("gudangtujuan") : idmrsout = dr1("idmrsout")

                        'UPDATE OUTSTANDING ---------------------------
                        If idmrsout <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING 
                            Dim Outstanding As Double = AsDataTableDSum(dtdetailOut, "jmlbarang", "idmrsout=" & idmrsout)
                            updNilaiMrsOut = String.Concat("WHEN '" & idmrsout & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiMrsOut)

                            '2. SET FILTERUPDATE OUTSTANDING 
                            updFilterMrsOut = IIf(Len(updFilterMrsOut.ToString) = 0, "", updFilterMrsOut & " OR ")
                            updFilterMrsOut = String.Concat(updFilterMrsOut, "(idmrsout = '" & idmrsout & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                        'VALIDASI STOK --------------------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        'END OF VALIDASI STOK -------------------------------------

                    Next
                Else
                    result(2) = "Detail transaction not found. (Material)" : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetailOut, "", "", ftExistStok, ftStok, "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'MRS OUT
                If Len(updNilaiMrsOut) > 0 Then
                    'UPDATE DETAIL OUT
                    sql = "UPDATE m6_mrs_out SET jmlrealisasi = (CASE idmrsout " & updNilaiMrsOut & " ELSE jmlrealisasi END) WHERE " & updFilterMrsOut
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'AMBIL ID UTAMA OUT
                    Dim ftDetail As String = ""
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idmrs FROM m6_mrs_out WHERE " & updFilterMrsOut & " GROUP BY idmrs", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idmrs = '" & dr1("idmrs") & "')")
                        Next
                    End If

                    'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                    If Len(ftDetail) > 0 Then
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idmrs, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_mrs_out WHERE " & ftDetail & " GROUP BY idmrs", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                Dim statusOut As Integer = 0
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiMrsUtamaOut = String.Concat(updNilaiMrsUtamaOut, "WHEN '" & dr1("idmrs") & "' THEN '" & statusOut & "' ")

                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterMrsUtama = IIf(Len(updFilterMrsUtama.ToString) = 0, "", updFilterMrsUtama & " OR ")
                                updFilterMrsUtama = String.Concat(updFilterMrsUtama, "(mrsid = '" & dr1("idmrs") & "')")
                            Next
                        End If
                    End If
                End If

                'MRS UTAMA, UPDATE STATUS OUT
                If Len(updNilaiMrsUtamaOut) > 0 Then
                    sql = "UPDATE m6_mrs SET mrsstatusrealisasiout = (CASE mrsid " & updNilaiMrsUtamaOut & " ELSE mrsstatusrealisasiout END) WHERE " & updFilterMrsUtama
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDBCon("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'", myConn)
                If dtBatch.Rows.Count > 0 Then
                    'DELETE NO BATCH IN MASUK ---------------------------
                    sql = "DELETE FROM m1_no_batch_in WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDBCon("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'", myConn)
                If dtSerial.Rows.Count > 0 Then
                    'DELETE NO SERIAL IN MASUK --------------------------
                    sql = "DELETE FROM m1_no_serial_in WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE NO SERIAL =======================================================


                'UPDATE STOK ====================================================================
                'STOK KELUAR
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK =============================================================


                'DELETE TRANSAKSI BARANG ========================================================
                'HAPUS DI M1_ITEM_TRANSACTION
                sql = "DELETE FROM m1_item_transaction WHERE sumber = '" & sumber & "' AND idutama = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG =================================================

            End If

            'update status utama
            sql = "UPDATE M6_Mrn SET Mrnstatus = " & nilaiStatus & ", Mrnmodifikasiuser='" & userid & "', Mrnmodifikasitgl = NOW(), Mrnposting = 0, Mrnpostingtgl = '1971-01-01 00:00:00', Mrnjmlrevisi = Mrnjmlrevisi + 1 WHERE Mrnid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M6_MrnSearch(PostWsSearch(paramSplit(0), "M6_MrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***' 

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi
        End Try

        objCmd = Nothing
        'myconn.Close()
        'myconn = Nothing
        'UPDATE OF SIMPAN KE DATABASE ==========================================================

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

    <WebMethod()>
    Public Function M6_MrnDelete(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "MRN", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Mrnid, Mrnnotransaksi FROM M6_Mrn WHERE Mrnid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT mrncabang, mrnlokasi, mrnsumber, mrnautonotransaksi, mrnnotransaksi, mrntgl"
            sql &= " FROM M6_mrn"
            sql &= " WHERE mrnid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("mrncabang")
                lokasi = dtNomorNext.Rows(0)("mrnlokasi")
                sumber = dtNomorNext.Rows(0)("mrnsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("mrnautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("mrnnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("mrntgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'HAPUS BATCH
            sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS SERIAL
            sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M6_Mrn_Out WHERE idMrn ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M6_Mrn WHERE Mrnid ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'UPDATE NOMOR BERIKUTNYA ============================================================
            'JIKA AUTO NO. TRANSAKSI
            If autonotransaksi = 1 Then
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi)
                Dim arrNomorNext(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                arrNomorNext = rsNomorNext.Split(sptSubParam)
                'Cek success M0_DeleteNotransaksi
                If (arrNomorNext(0) = 1) Then
                    sql = arrNomorNext(3)
                    'Tambah query update m0_nomor_next
                    If Len(sql) > 0 Then
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                Else
                    result(2) = arrNomorNext(1) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF UPDATE NOMOR BERIKUTNYA =====================================================


            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M6_MrnSearch(PostWsSearch(paramSplit(0), "M6_MrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi

        End Try

        objCmd = Nothing
        'myconn.Close()
        'myconn = Nothing
        'END OF DELETE DI DATABASE ==========================================================

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

    <WebMethod()>
    Public Function M6_MrnGetdataById(ByVal param As String) As String
        'M6_MrnGetdataById Utama --------------------------------------------------------
        'mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, 
        'mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, 
        'mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, 
        'mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, 
        'mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, 
        'mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, 
        'mrnposting, mrnpostingtgl, mrnisclose, mrncustomtext1, mrncustomtext2, mrncustomtext3, mrncustomtext4, 
        'mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, 
        'mrncustomdate1, mrncustomdate2, mrncustomdate3, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, 
        'mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, 
        'mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama, 
        'mrnaktivitas, mrnaktivitaskode, mrnaktivitasnama, mrnjeniswajibwo

        'M6_MrnGetdataById Out --------------------------------------------------------
        'idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, 
        'bomnotransaksi, pdrnotransaksi, wonotransaksi, mrsnotransaksi, jmlsisapd, jmlsisarealisasi

        'M6_MrnGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M6_MrnGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = ""
        Dim sumber As String = "MRN"

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M5_pl~M5_pl_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "mrno.statusrealisasi")

            Filter2 = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter2 = Filter2.Replace("statusrealisasi", "mrno.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "mrnid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "mrnid = " & idtransaksi & " and " & Filter
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idmrn = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idmrn = '" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_mrn_getdata")
        sql = "select mrn.mrnid AS mrnid, mrn.mrncabang AS mrncabang, mrn.mrnlokasi AS mrnlokasi, mrn.mrngudangasal AS mrngudangasal, mrn.mrngudangproduksi AS mrngudangproduksi, mrn.mrngudangtujuan AS mrngudangtujuan, mrn.mrnsumber AS mrnsumber, mrn.mrnjenis AS mrnjenis, mrn.mrnautonotransaksi AS mrnautonotransaksi, mrn.mrnnotransaksi AS mrnnotransaksi, mrn.mrntgl AS mrntgl, mrn.mrnkodepa AS mrnkodepa, mrn.mrnbagianmrn AS mrnbagianmrn, mrn.mrnbagianmrnkontak AS mrnbagianmrnkontak, mrn.mrntgldipakai AS mrntgldipakai, mrn.mrnestimasikerja AS mrnestimasikerja, mrn.mrnmatauang AS mrnmatauang, mrn.mrnkurs AS mrnkurs, mrn.mrntotalhargain AS mrntotalhargain, mrn.mrntotalhargaout AS mrntotalhargaout, mrn.mrntotalhppin AS mrntotalhppin, mrn.mrntotalhppout AS mrntotalhppout, mrn.mrnuraian AS mrnuraian, mrn.mrncatatan AS mrncatatan, mrn.mrnnoref AS mrnnoref, mrn.mrntglnoref AS mrntglnoref, mrn.mrnidbom AS mrnidbom, mrn.mrnidpdr AS mrnidpdr, mrn.mrnidwo AS mrnidwo, mrn.mrnidmrs AS mrnidmrs, mrn.mrnstatuspdin AS mrnstatuspdin, mrn.mrnstatuspdout AS mrnstatuspdout, mrn.mrnstatusrealisasiin AS mrnstatusrealisasiin, mrn.mrnstatusrealisasiout AS mrnstatusrealisasiout, mrn.mrnstatus AS mrnstatus, mrn.mrnstatussebelumnya AS mrnstatussebelumnya, mrn.mrnjmlrevisi AS mrnjmlrevisi, mrn.mrncetakanke AS mrncetakanke, mrn.mrninputuser AS mrninputuser, mrn.mrninputtgl AS mrninputtgl, mrn.mrnmodifikasiuser AS mrnmodifikasiuser, mrn.mrnmodifikasitgl AS mrnmodifikasitgl, mrn.mrnposting AS mrnposting, mrn.mrnpostingtgl AS mrnpostingtgl, mrn.mrnisclose AS mrnisclose, mrn.mrncustomtext1 AS mrncustomtext1, mrn.mrncustomtext2 AS mrncustomtext2, mrn.mrncustomtext3 AS mrncustomtext3, mrn.mrncustomtext4 AS mrncustomtext4, mrn.mrncustomtext5 AS mrncustomtext5, mrn.mrncustomint1 AS mrncustomint1, mrn.mrncustomint2 AS mrncustomint2, mrn.mrncustomint3 AS mrncustomint3, mrn.mrncustomdbl1 AS mrncustomdbl1, mrn.mrncustomdbl2 AS mrncustomdbl2, mrn.mrncustomdbl3 AS mrncustomdbl3, mrn.mrncustomdate1 AS mrncustomdate1, mrn.mrncustomdate2 AS mrncustomdate2, mrn.mrncustomdate3 AS mrncustomdate3, br.bnama AS mrncabangnama, lc.lnama AS mrnlokasinama, wh1.wnama AS mrngudangasalnama, wh2.wnama AS mrngudangproduksinama, wh3.wnama AS mrngudangtujuannama, pc.pcnama AS mrnjenisnama, c1.kkode AS mrnbagianmrnkode, c1.knama AS mrnbagianmrnnama, we.wenama AS mrnestimasikerjanama, bom.bomnotransaksi AS mrnnotransaksibom, pdr.pdrnotransaksi AS mrnnotransaksipdr, wo.wonotransaksi AS mrnnotransaksiwo, mrs.mrsnotransaksi AS mrnnotransaksimrs, st1.nama AS mrnstatusnama, st2.nama AS mrnstatussebelumnyanama, u1.unama AS mrninputusernama, u2.unama AS mrnmodifikasiusernama, mrn.mrnaktivitas, pa.pakode as mrnaktivitaskode, pa.panama as mrnaktivitasnama, pc.pcwajibwo AS mrnjeniswajibwo, mrno.idmrnout AS idmrnout, mrno.idmrn AS idmrn, mrno.idbarang AS idbarang, mrno.namabarang AS namabarang, mrno.tipebarang AS tipebarang, mrno.jml AS jml, mrno.satuan AS satuan, mrno.nilaisatuan AS nilaisatuan, mrno.jmlbarang AS jmlbarang, mrno.satuanbarang AS satuanbarang, mrno.matauang AS matauang, mrno.kurs AS kurs, mrno.harga AS harga, mrno.hpp AS hpp, mrno.idhppkhususkeluar AS idhppkhususkeluar, mrno.idhppfifokeluar AS idhppfifokeluar, i.brekpersediaan AS rekpersediaan, mrno.cabang AS cabang, mrno.lokasi AS lokasi, mrno.gudangasal AS gudangasal, mrno.gudangproduksi AS gudangproduksi, mrno.gudangtujuan AS gudangtujuan, mrno.costcenter AS costcenter, mrno.divisi AS divisi, mrno.subdivisi AS subdivisi, mrno.proyek AS proyek, mrno.catatan AS catatan, mrno.urutan AS urutan, mrno.idbomout AS idbomout, mrno.idpdrout AS idpdrout, mrno.idwoout AS idwoout, mrno.idmrsout AS idmrsout, mrno.jmlpd AS jmlpd, mrno.statuspd AS statuspd, mrno.jmlrealisasi AS jmlrealisasi, mrno.statusrealisasi AS statusrealisasi, mrno.isclose AS isclose, mrno.customtext1 AS customtext1, mrno.customtext2 AS customtext2, mrno.customtext3 AS customtext3, mrno.customdbl1 AS customdbl1, mrno.customdbl2 AS customdbl2, mrno.customdbl3 AS customdbl3, mrno.customdate1 AS customdate1, mrno.customdate2 AS customdate2, mrno.customdate3 AS customdate3, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama, mrn.mrnnotransaksi AS notransaksi, bom2.bomnotransaksi AS bomnotransaksi, pdr2.pdrnotransaksi AS pdrnotransaksi, wo2.wonotransaksi AS wonotransaksi, mrs2.mrsnotransaksi AS mrsnotransaksi, ((mrno.jmlbarang - mrno.jmlpd) / mrno.nilaisatuan) AS jmlsisapd,  ((mrno.jmlbarang - mrno.jmlrealisasi) / mrno.nilaisatuan) AS jmlsisarealisasi  from m6_mrn mrn  join m6_mrn_out mrno on mrn.mrnid = mrno.idmrn left join m1_branch br on mrn.mrncabang = br.bkode left join m1_location lc on mrn.mrnlokasi = lc.lkode left join m1_warehouse wh1 on mrn.mrngudangasal = wh1.wkode left join m1_warehouse wh2 on mrn.mrngudangproduksi = wh2.wkode left join m1_warehouse wh3 on mrn.mrngudangtujuan = wh3.wkode left join m1_production_category pc on mrn.mrnjenis = pc.pckode left join m1_contact c1 on mrn.mrnbagianmrn = c1.kid left join m1_working_estimate we on mrn.mrnestimasikerja = we.wekode left join m6_bom bom on mrn.mrnidbom = bom.bomid left join m6_pdr pdr on mrn.mrnidpdr = pdr.pdrid left join m6_wo wo on mrn.mrnidwo = wo.woid left join m6_mrs mrs on mrn.mrnidmrs = mrs.mrsid left join m0_status st1 on mrn.mrnstatus = st1.kode left join m0_status st2 on mrn.mrnstatussebelumnya = st2.kode left join m0_user u1 on mrn.mrninputuser = u1.userid left join m0_user u2 on mrn.mrnmodifikasiuser = u2.userid left join m1_production_activity pa on mrn.mrnaktivitas = pa.paid left join m1_item i on mrno.idbarang = i.bid left join m1_cost_center cc on mrno.costcenter = cc.cckode left join m1_division d on mrno.divisi = d.dkode left join m1_subdivision sd on mrno.subdivisi = sd.sdkode left join m1_project p on mrno.proyek = p.pkode left join m6_bom_out bomo on mrno.idbomout = bomo.idbomout left join m6_bom bom2 on bomo.idbom = bom2.bomid left join m6_pdr_out pdro on mrno.idpdrout = pdro.idpdrout left join m6_pdr pdr2 on pdro.idpdr = pdr2.pdrid left join m6_wo_out woo on mrno.idwoout = woo.idwoout left join m6_wo wo2 on woo.idwo = wo2.woid left join m6_mrs_out mrso on mrno.idmrsout = mrso.idmrsout left join m6_mrs mrs2 on mrso.idmrs = mrs2.mrsid"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("mrnid"), 0), sptField,
                     FxDB(drutama("mrncabang"), ""), sptField,
                     FxDB(drutama("mrnlokasi"), ""), sptField,
                     FxDB(drutama("mrngudangasal"), ""), sptField,
                     FxDB(drutama("mrngudangproduksi"), ""), sptField,
                     FxDB(drutama("mrngudangtujuan"), ""), sptField,
                     FxDB(drutama("mrnsumber"), ""), sptField,
                     FxDB(drutama("mrnjenis"), ""), sptField,
                     FxDB(drutama("mrnautonotransaksi"), 0), sptField,
                     FxDB(drutama("mrnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrntgl"), ""), formatTgl), sptField,
                     FxDB(drutama("mrnkodepa"), 0), sptField,
                     FxDB(drutama("mrnbagianmrn"), 0), sptField,
                     FxDB(drutama("mrnbagianmrnkontak"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrntgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("mrnestimasikerja"), ""), sptField,
                     FxDB(drutama("mrnmatauang"), ""), sptField,
                     FxDB(drutama("mrnkurs"), 0), sptField,
                     FxDB(drutama("mrntotalhargain"), 0), sptField,
                     FxDB(drutama("mrntotalhargaout"), 0), sptField,
                     FxDB(drutama("mrntotalhppin"), 0), sptField,
                     FxDB(drutama("mrntotalhppout"), 0), sptField,
                     FxDB(drutama("mrnuraian"), ""), sptField,
                     FxDB(drutama("mrncatatan"), ""), sptField,
                     FxDB(drutama("mrnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("mrntglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("mrnidbom"), 0), sptField,
                     FxDB(drutama("mrnidpdr"), 0), sptField,
                     FxDB(drutama("mrnidwo"), 0), sptField,
                     FxDB(drutama("mrnidmrs"), 0), sptField,
                     FxDB(drutama("mrnstatuspdin"), 0), sptField,
                     FxDB(drutama("mrnstatuspdout"), 0), sptField,
                     FxDB(drutama("mrnstatusrealisasiin"), 0), sptField,
                     FxDB(drutama("mrnstatusrealisasiout"), 0), sptField,
                     FxDB(drutama("mrnstatus"), 0), sptField,
                     FxDB(drutama("mrnstatussebelumnya"), 0), sptField,
                     FxDB(drutama("mrnjmlrevisi"), 0), sptField,
                     FxDB(drutama("mrncetakanke"), 0), sptField,
                     FxDB(drutama("mrninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("mrnisclose"), 0), sptField,
                     FxDB(drutama("mrncustomtext1"), ""), sptField,
                     FxDB(drutama("mrncustomtext2"), ""), sptField,
                     FxDB(drutama("mrncustomtext3"), ""), sptField,
                     FxDB(drutama("mrncustomtext4"), ""), sptField,
                     FxDB(drutama("mrncustomtext5"), ""), sptField,
                     FxDB(drutama("mrncustomint1"), 0), sptField,
                     FxDB(drutama("mrncustomint2"), 0), sptField,
                     FxDB(drutama("mrncustomint3"), 0), sptField,
                     FxDB(drutama("mrncustomdbl1"), 0), sptField,
                     FxDB(drutama("mrncustomdbl2"), 0), sptField,
                     FxDB(drutama("mrncustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("mrncustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrncustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("mrncustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("mrncabangnama"), ""), sptField,
                     FxDB(drutama("mrnlokasinama"), ""), sptField,
                     FxDB(drutama("mrngudangasalnama"), ""), sptField,
                     FxDB(drutama("mrngudangproduksinama"), ""), sptField,
                     FxDB(drutama("mrngudangtujuannama"), ""), sptField,
                     FxDB(drutama("mrnjenisnama"), ""), sptField,
                     FxDB(drutama("mrnbagianmrnkode"), ""), sptField,
                     FxDB(drutama("mrnbagianmrnnama"), ""), sptField,
                     FxDB(drutama("mrnestimasikerjanama"), ""), sptField,
                     FxDB(drutama("mrnnotransaksibom"), ""), sptField,
                     FxDB(drutama("mrnnotransaksipdr"), ""), sptField,
                     FxDB(drutama("mrnnotransaksiwo"), ""), sptField,
                     FxDB(drutama("mrnnotransaksimrs"), ""), sptField,
                     FxDB(drutama("mrnstatusnama"), ""), sptField,
                     FxDB(drutama("mrnstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("mrninputusernama"), ""), sptField,
                     FxDB(drutama("mrnmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("mrnaktivitas"), 0), sptField,
                     FxDB(drutama("mrnaktivitaskode"), ""), sptField,
                     FxDB(drutama("mrnaktivitasnama"), ""), sptField,
                     FxDB(drutama("mrnjeniswajibwo"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idmrnout"), 0), sptField,
                     FxDB(dr("idmrn"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("idhppkhususkeluar"), 0), sptField,
                     FxDB(dr("idhppfifokeluar"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idbomout"), 0), sptField,
                     FxDB(dr("idpdrout"), 0), sptField,
                     FxDB(dr("idwoout"), 0), sptField,
                     FxDB(dr("idmrsout"), 0), sptField,
                     FxDB(dr("jmlpd"), 0), sptField,
                     FxDB(dr("statuspd"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     FxDB(dr("pdrnotransaksi"), ""), sptField,
                     FxDB(dr("wonotransaksi"), ""), sptField,
                     FxDB(dr("mrsnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch,
                     FxDB(dr("nbtid"), 0), sptField,
                     FxDB(dr("nbtjenismutasi"), 0), sptField,
                     FxDB(dr("nbtidbatchin"), 0), sptField,
                     FxDB(dr("nbtgudang"), ""), sptField,
                     FxDB(dr("nbtidbarang"), 0), sptField,
                     FxDB(dr("nbtkode"), ""), sptField,
                     FxDB(dr("nbtsumber"), ""), sptField,
                     FxDB(dr("nbtidtransaksi"), 0), sptField,
                     FxDB(dr("nbtsatuan"), ""), sptField,
                     FxDB(dr("nbtjml"), 0), sptField,
                     FxDB(dr("nbtcustomtext1"), ""), sptField,
                     FxDB(dr("nbtcustomtext2"), ""), sptField,
                     FxDB(dr("nbtcustomtext3"), ""), sptField,
                     FxDB(dr("nbtcustomdbl1"), 0), sptField,
                     FxDB(dr("nbtcustomdbl2"), 0), sptField,
                     FxDB(dr("nbtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial,
                     FxDB(dr("nstid"), 0), sptField,
                     FxDB(dr("nstjenismutasi"), 0), sptField,
                     FxDB(dr("nstidserialin"), 0), sptField,
                     FxDB(dr("nstgudang"), ""), sptField,
                     FxDB(dr("nstidbarang"), 0), sptField,
                     FxDB(dr("nstkode"), ""), sptField,
                     FxDB(dr("nstsumber"), ""), sptField,
                     FxDB(dr("nstidtransaksi"), 0), sptField,
                     FxDB(dr("nstsatuan"), ""), sptField,
                     FxDB(dr("nstjml"), 0), sptField,
                     FxDB(dr("nstcustomtext1"), ""), sptField,
                     FxDB(dr("nstcustomtext2"), ""), sptField,
                     FxDB(dr("nstcustomtext3"), ""), sptField,
                     FxDB(dr("nstcustomdbl1"), 0), sptField,
                     FxDB(dr("nstcustomdbl2"), 0), sptField,
                     FxDB(dr("nstcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnposting, mrnpostingtgl, mrnisclose, mrncustomtext1, mrncustomtext2, mrncustomtext3, mrncustomtext4, mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, mrncustomdate1, mrncustomdate2, mrncustomdate3, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama, mrnaktivitas, mrnaktivitaskode, mrnaktivitasnama, mrnjeniswajibwo" & sptSubParam & "idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, wonotransaksi, mrsnotransaksi, jmlsisapd, jmlsisarealisasi" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_MrnSearch(ByVal param As String) As String
        'M6_MrnSearch --------------------------------------------------------
        'mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, 
        'mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, 
        'mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, 
        'mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, 
        'mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, 
        'mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, 
        'mrnposting, mrnpostingtgl, mrnisclose, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, 
        'mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, 
        'mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama, 
        'mrnaktivitas, mrnaktivitaskode, mrnaktivitasnama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""
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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_mrn_v")
        sql = "select mrn.mrnid AS mrnid, mrn.mrncabang AS mrncabang, mrn.mrnlokasi AS mrnlokasi, mrn.mrngudangasal AS mrngudangasal, mrn.mrngudangproduksi AS mrngudangproduksi, mrn.mrngudangtujuan AS mrngudangtujuan, mrn.mrnsumber AS mrnsumber, mrn.mrnjenis AS mrnjenis, mrn.mrnautonotransaksi AS mrnautonotransaksi, mrn.mrnnotransaksi AS mrnnotransaksi, mrn.mrntgl AS mrntgl, mrn.mrnkodepa AS mrnkodepa, mrn.mrnbagianmrn AS mrnbagianmrn, mrn.mrnbagianmrnkontak AS mrnbagianmrnkontak, mrn.mrntgldipakai AS mrntgldipakai, mrn.mrnestimasikerja AS mrnestimasikerja, mrn.mrnmatauang AS mrnmatauang, mrn.mrnkurs AS mrnkurs, mrn.mrntotalhargain AS mrntotalhargain, mrn.mrntotalhargaout AS mrntotalhargaout, mrn.mrntotalhppin AS mrntotalhppin, mrn.mrntotalhppout AS mrntotalhppout, mrn.mrnuraian AS mrnuraian, mrn.mrncatatan AS mrncatatan, mrn.mrnnoref AS mrnnoref, mrn.mrntglnoref AS mrntglnoref, mrn.mrnidbom AS mrnidbom, mrn.mrnidpdr AS mrnidpdr, mrn.mrnidwo AS mrnidwo, mrn.mrnidmrs AS mrnidmrs, mrn.mrnstatuspdin AS mrnstatuspdin, mrn.mrnstatuspdout AS mrnstatuspdout, mrn.mrnstatusrealisasiin AS mrnstatusrealisasiin, mrn.mrnstatusrealisasiout AS mrnstatusrealisasiout, mrn.mrnstatus AS mrnstatus, mrn.mrnstatussebelumnya AS mrnstatussebelumnya, mrn.mrnjmlrevisi AS mrnjmlrevisi, mrn.mrncetakanke AS mrncetakanke, mrn.mrninputuser AS mrninputuser, mrn.mrninputtgl AS mrninputtgl, mrn.mrnmodifikasiuser AS mrnmodifikasiuser, mrn.mrnmodifikasitgl AS mrnmodifikasitgl, mrn.mrnposting AS mrnposting, mrn.mrnpostingtgl AS mrnpostingtgl, mrn.mrnisclose AS mrnisclose, br.bnama AS mrncabangnama, lc.lnama AS mrnlokasinama, wh1.wnama AS mrngudangasalnama, wh2.wnama AS mrngudangproduksinama, wh3.wnama AS mrngudangtujuannama, pc.pcnama AS mrnjenisnama, c1.kkode AS mrnbagianmrnkode, c1.knama AS mrnbagianmrnnama, we.wenama AS mrnestimasikerjanama, bom.bomnotransaksi AS mrnnotransaksibom, pdr.pdrnotransaksi AS mrnnotransaksipdr, wo.wonotransaksi AS mrnnotransaksiwo, mrs.mrsnotransaksi AS mrnnotransaksimrs, st1.nama AS mrnstatusnama, st2.nama AS mrnstatussebelumnyanama, u1.unama AS mrninputusernama, u2.unama AS mrnmodifikasiusernama, mrn.mrnaktivitas, pa.pakode as mrnaktivitaskode, pa.panama as mrnaktivitasnama from m6_mrn mrn left join m1_branch br on mrn.mrncabang = br.bkode left join m1_location lc on mrn.mrnlokasi = lc.lkode left join m1_warehouse wh1 on mrn.mrngudangasal = wh1.wkode left join m1_warehouse wh2 on mrn.mrngudangproduksi = wh2.wkode left join m1_warehouse wh3 on mrn.mrngudangtujuan = wh3.wkode left join m1_production_category pc on mrn.mrnjenis = pc.pckode left join m1_contact c1 on mrn.mrnbagianmrn = c1.kid left join m1_working_estimate we on mrn.mrnestimasikerja = we.wekode left join m6_bom bom on mrn.mrnidbom = bom.bomid left join m6_pdr pdr on mrn.mrnidpdr = pdr.pdrid left join m6_wo wo on mrn.mrnidwo = wo.woid left join m6_mrs mrs on mrn.mrnidmrs = mrs.mrsid left join m0_status st1 on mrn.mrnstatus = st1.kode left join m0_status st2 on mrn.mrnstatussebelumnya = st2.kode left join m0_user u1 on mrn.mrninputuser = u1.userid left join m0_user u2 on mrn.mrnmodifikasiuser = u2.userid left join m1_production_activity pa on mrn.mrnaktivitas = pa.paid"

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("mrnid"), 0), sptField,
                     FxDB(dr("mrncabang"), ""), sptField,
                     FxDB(dr("mrnlokasi"), ""), sptField,
                     FxDB(dr("mrngudangasal"), ""), sptField,
                     FxDB(dr("mrngudangproduksi"), ""), sptField,
                     FxDB(dr("mrngudangtujuan"), ""), sptField,
                     FxDB(dr("mrnsumber"), ""), sptField,
                     FxDB(dr("mrnjenis"), ""), sptField,
                     FxDB(dr("mrnautonotransaksi"), 0), sptField,
                     FxDB(dr("mrnnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrntgl"), ""), formatTgl), sptField,
                     FxDB(dr("mrnkodepa"), 0), sptField,
                     FxDB(dr("mrnbagianmrn"), 0), sptField,
                     FxDB(dr("mrnbagianmrnkontak"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrntgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("mrnestimasikerja"), ""), sptField,
                     FxDB(dr("mrnmatauang"), ""), sptField,
                     FxDB(dr("mrnkurs"), 0), sptField,
                     FxDB(dr("mrntotalhargain"), 0), sptField,
                     FxDB(dr("mrntotalhargaout"), 0), sptField,
                     FxDB(dr("mrntotalhppin"), 0), sptField,
                     FxDB(dr("mrntotalhppout"), 0), sptField,
                     FxDB(dr("mrnuraian"), ""), sptField,
                     FxDB(dr("mrncatatan"), ""), sptField,
                     FxDB(dr("mrnnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("mrntglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("mrnidbom"), 0), sptField,
                     FxDB(dr("mrnidpdr"), 0), sptField,
                     FxDB(dr("mrnidwo"), 0), sptField,
                     FxDB(dr("mrnidmrs"), 0), sptField,
                     FxDB(dr("mrnstatuspdin"), 0), sptField,
                     FxDB(dr("mrnstatuspdout"), 0), sptField,
                     FxDB(dr("mrnstatusrealisasiin"), 0), sptField,
                     FxDB(dr("mrnstatusrealisasiout"), 0), sptField,
                     FxDB(dr("mrnstatus"), 0), sptField,
                     FxDB(dr("mrnstatussebelumnya"), 0), sptField,
                     FxDB(dr("mrnjmlrevisi"), 0), sptField,
                     FxDB(dr("mrncetakanke"), 0), sptField,
                     FxDB(dr("mrninputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrninputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrnmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrnmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrnposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("mrnpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("mrnisclose"), 0), sptField,
                     FxDB(dr("mrncabangnama"), ""), sptField,
                     FxDB(dr("mrnlokasinama"), ""), sptField,
                     FxDB(dr("mrngudangasalnama"), ""), sptField,
                     FxDB(dr("mrngudangproduksinama"), ""), sptField,
                     FxDB(dr("mrngudangtujuannama"), ""), sptField,
                     FxDB(dr("mrnjenisnama"), ""), sptField,
                     FxDB(dr("mrnbagianmrnkode"), ""), sptField,
                     FxDB(dr("mrnbagianmrnnama"), ""), sptField,
                     FxDB(dr("mrnestimasikerjanama"), ""), sptField,
                     FxDB(dr("mrnnotransaksibom"), ""), sptField,
                     FxDB(dr("mrnnotransaksipdr"), ""), sptField,
                     FxDB(dr("mrnnotransaksiwo"), ""), sptField,
                     FxDB(dr("mrnnotransaksimrs"), ""), sptField,
                     FxDB(dr("mrnstatusnama"), ""), sptField,
                     FxDB(dr("mrnstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("mrninputusernama"), ""), sptField,
                     FxDB(dr("mrnmodifikasiusernama"), ""), sptField,
                     FxDB(dr("mrnaktivitas"), 0), sptField,
                     FxDB(dr("mrnaktivitaskode"), ""), sptField,
                     FxDB(dr("mrnaktivitasnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatusrealisasiin, mrnstatusrealisasiout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnposting, mrnpostingtgl, mrnisclose, mrncabangnama, mrnlokasinama, mrngudangasalnama, mrngudangproduksinama, mrngudangtujuannama, mrnjenisnama, mrnbagianmrnkode, mrnbagianmrnnama, mrnestimasikerjanama, mrnnotransaksibom, mrnnotransaksipdr, mrnnotransaksiwo, mrnnotransaksimrs, mrnstatusnama, mrnstatussebelumnyanama, mrninputusernama, mrnmodifikasiusernama, mrnaktivitas, mrnaktivitaskode, mrnaktivitasnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_MrnTerkait(ByVal param As String) As String
        'M6_MrnTerkait --------------------------------------------------------
        'mrnid, mrnnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "mrnid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

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
            Filter = pagingSplit(2) & " AND mrnid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "mrnid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m6_mrn_terkait(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m5_bom_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each pl As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(pl("mrnid"), 0), sptField,
                     FxDB(pl("mrnnotransaksi"), ""), sptField,
                     FxDB(pl("sumber"), ""), sptField,
                     FxDB(pl("idterkait"), 0), sptField,
                     FxDB(pl("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(pl("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(pl("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(pl("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(pl("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related MRN data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mrnid, mrnnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingMrsOut As String, ByVal ftOutstandingMrsOut As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = "", noBatch As String = "", noSerial As String = ""

        'VALIDASI OUTSTANDING MRS OUT -------------------------------
        If Len(ftExistOutstandingMrsOut) > 0 Then 'ftExistOutstanding = rowExists, idmrsout, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingMrsOut)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idmrsout=" & dtval.Rows(0)("idmrsout")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in MRS(material)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT mrsout.idmrsout, (mrsout.jmlbarang - mrsout.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m6_mrs_out AS mrsout INNER JOIN m1_item AS i ON mrsout.idbarang = i.bid WHERE " & ftOutstandingMrsOut
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idmrsout=" & dtval.Rows(0)("idmrsout")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in MRS(material), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING MRS OUT ------------------------


        Dim ProsesValidasiStok As String = F_getSetting(0, "company", "ValidasiStok")
        If ProsesValidasiStok.Equals("0") = False Then
            'VALIDASI STOK ----------------------------------------------
            'CEK DATA EXIST/TIDAK
            If Len(ftExistStok) > 0 Then
                dtval = AsDataTableAmbilDariDB(ftExistStok) 'ftExistStok = rowExists, idbarang, bkode, gudang
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    gudang = dtval.Rows(0)("gudang")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in '" & gudang & "' warehouse" : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK PERGUDANG YG TERSEDIA
            If Len(ftStok) > 0 Then
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStok
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStok
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("stok")
                    gudang = dtval.Rows(0)("kgudang")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in '" & gudang & "' warehouse, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI STOK ---------------------------------------
        End If


        'VALIDASI BATCH ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistBatch) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistBatch) 'ftExistBatch = rowExists, idbarang, bkode, nbikode, nbigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " doesn't exists in No. Batch list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA BATCH YG TERSEDIA
        If Len(ftBatch) > 0 Then
            sql = "SELECT nbi.nbiidbarang, nbi.nbikode, nbi.nbigudang, nbi.nbijmlsisa, i.bkode FROM m1_no_batch_in nbi JOIN m1_item i ON nbi.nbiidbarang = i.bid WHERE " & ftBatch
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nbijmlsisa")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nbiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " exceeds the number of stock in No. Batch list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI BATCH --------------------------------------

        'VALIDASI SERIAL ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistSerial) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistSerial) 'ftExistSerial = rowExists, idbarang, bkode, nsikode, nsigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " doesn't exists in No. Serial list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA SERIAL YG TERSEDIA
        If Len(ftSerial) > 0 Then
            sql = "SELECT nsi.nsiidbarang, nsi.nsikode, nsi.nsigudang, nsi.nsijmlsisa, i.bkode FROM m1_no_serial_in nsi JOIN m1_item i ON nsi.nsiidbarang = i.bid WHERE " & ftSerial
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nsijmlsisa")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nsiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " exceeds the number of stock in No. Serial list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI SERIAL --------------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M6_MrnSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'mrnid(0) As Integer, mrncabang(1) As String, mrnlokasi(2) As String, mrngudangasal(3) As String, mrngudangproduksi(4) As String, 
        'mrngudangtujuan(5) As String, mrnsumber(6) As String, mrnjenis(7) As String, mrnautonotransaksi(8) As Integer, mrnnotransaksi(9) As String, 
        'mrntgl(10) As Date, mrnkodepa(11) As Integer, mrnbagianmrn(12) As Integer, mrnbagianmrnkontak(13) As String, mrntgldipakai(14) As Date, 
        'mrnestimasikerja(15) As String, mrnmatauang(16) As String, mrnkurs(17) As Double, mrntotalhargain(18) As Double, mrntotalhargaout(19) As Double, 
        'mrntotalhppin(20) As Double, mrntotalhppout(21) As Double, mrnuraian(22) As String, mrncatatan(23) As String, mrnnoref(24) As String, 
        'mrntglnoref(25) As Date, mrnidbom(26) As Integer, mrnidpdr(27) As Integer, mrnidwo(28) As Integer, mrnidmrs(29) As Integer, 
        'mrnstatuspdin(30) As Integer, mrnstatuspdout(31) As Integer, mrnstatus(32) As Integer, mrnstatussebelumnya(33) As Integer, mrnjmlrevisi(34) As Integer, 
        'mrncetakanke(35) As Integer, mrninputuser(36) As Integer, mrninputtgl(37) As DateTime, mrnmodifikasiuser(38) As Integer, mrnmodifikasitgl(39) As DateTime, 
        'mrnisclose(40) As Integer, mrncustomtext1(41) As String, mrncustomtext2(42) As String, mrncustomtext3(43) As String, mrncustomtext4(44) As String, 
        'mrncustomtext5(45) As String, mrncustomint1(46) As Integer, mrncustomint2(47) As Integer, mrncustomint3(48) As Integer, mrncustomdbl1(49) As Double, 
        'mrncustomdbl2(50) As Double, mrncustomdbl3(51) As Double, mrncustomdate1(52) As Date, mrncustomdate2(53) As Date, mrncustomdate3(54) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'mrnid, mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, 
        'mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, 
        'mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, 
        'mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, 
        'mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, 
        'mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnisclose, mrncustomtext1, 
        'mrncustomtext2, mrncustomtext3, mrncustomtext4, mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, 
        'mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, mrncustomdate1, mrncustomdate2, mrncustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 55) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'mrnid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "mrnid required numeric." : GoTo selesai
        End If
        'mrnautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "mrnautonotransaksi required numeric." : GoTo selesai
        End If
        'mrntgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "mrntgl required date." : GoTo selesai
        End If
        'mrnkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "mrnkodepa required numeric." : GoTo selesai
        End If
        'mrnbagianmrn(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "mrnbagianmrn required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "mrnbagianmrn can't be empty." : GoTo selesai
        End If
        'mrntgldipakai(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "mrntgldipakai required date." : GoTo selesai
        End If
        'mrnkurs(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "mrnkurs required numeric." : GoTo selesai
        End If
        'mrntotalhargain(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "mrntotalhargain required numeric." : GoTo selesai
        End If
        'mrntotalhargaout(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "mrntotalhargaout required numeric." : GoTo selesai
        End If
        'mrntotalhppin(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "mrntotalhppin required numeric." : GoTo selesai
        End If
        'mrntotalhppout(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "mrntotalhppout required numeric." : GoTo selesai
        End If
        'mrntglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "mrntglnoref required date." : GoTo selesai
        End If
        'mrnidbom(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "mrnidbom required numeric." : GoTo selesai
        End If
        'mrnidpdr(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "mrnidpdr required numeric." : GoTo selesai
        End If
        'mrnidwo(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "mrnidwo required numeric." : GoTo selesai
        End If
        'mrnidmrs(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "mrnidmrs required numeric." : GoTo selesai
        End If
        'mrnstatuspdin(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "mrnstatuspdin required numeric." : GoTo selesai
        End If
        'mrnstatuspdout(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "mrnstatuspdout required numeric." : GoTo selesai
        End If
        'mrnstatus(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "mrnstatus required numeric." : GoTo selesai
        End If
        'mrnstatussebelumnya(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "mrnstatussebelumnya required numeric." : GoTo selesai
        End If
        'mrnjmlrevisi(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "mrnjmlrevisi required numeric." : GoTo selesai
        End If
        'mrncetakanke(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "mrncetakanke required numeric." : GoTo selesai
        End If
        'mrninputuser(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "mrninputuser required numeric." : GoTo selesai
        End If
        'mrninputtgl(37) As DateTime
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "mrninputtgl required date." : GoTo selesai
        End If
        'mrnmodifikasiuser(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "mrnmodifikasiuser required numeric." : GoTo selesai
        End If
        'mrnmodifikasitgl(39) As DateTime
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "mrnmodifikasitgl required date." : GoTo selesai
        End If
        'mrnisclose(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "mrnisclose required numeric." : GoTo selesai
        End If
        'mrncustomint1(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "mrncustomint1 required numeric." : GoTo selesai
        End If
        'mrncustomint2(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "mrncustomint2 required numeric." : GoTo selesai
        End If
        'mrncustomint3(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "mrncustomint3 required numeric." : GoTo selesai
        End If
        'mrncustomdbl1(49) As Double
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "mrncustomdbl1 required numeric." : GoTo selesai
        End If
        'mrncustomdbl2(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "mrncustomdbl2 required numeric." : GoTo selesai
        End If
        'mrncustomdbl3(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "mrncustomdbl3 required numeric." : GoTo selesai
        End If
        'mrncustomdate1(52) As Date
        If (IsDate(dataUtama(52)) = False) Then
            result(2) = "mrncustomdate1 required date." : GoTo selesai
        End If
        'mrncustomdate2(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "mrncustomdate2 required date." : GoTo selesai
        End If
        'mrncustomdate3(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "mrncustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===========================================


        'VALIDASI DATA UTAMA =======================================================
        'mrncabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "mrncabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "mrncabang should not be more than 25 character." : GoTo selesai
        End If

        'mrnlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "mrnlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "mrnlokasi should not be more than 25 character." : GoTo selesai
        End If

        'mrngudangasal(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "mrngudangasal can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "mrngudangasal should not be more than 25 character." : GoTo selesai
        End If

        'mrngudangproduksi(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "mrngudangproduksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "mrngudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'mrngudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "mrngudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "mrngudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'mrnsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "mrnsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "mrnsumber should not be more than 10 character." : GoTo selesai
        End If

        'mrnjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "mrnjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "mrnjenis should not be more than 25 character." : GoTo selesai
        End If

        'mrnnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "mrnnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "mrnnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'mrntgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "mrntgl can't be empty" : GoTo selesai
        End If

        'mrntgldipakai(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "mrntgldipakai can't be empty" : GoTo selesai
        End If

        'mrnmatauang(16) As String
        If Len(dataUtama(16)) = 0 Then
            result(2) = "mrnmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(16)) > 25 Then
            result(2) = "mrnmatauang should not be more than 25 character." : GoTo selesai
        End If

        'mrnkurs(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "mrnkurs can't be empty" : GoTo selesai
        End If

        'mrntotalhargain(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "mrntotalhargain can't be empty" : GoTo selesai
        End If

        'mrntotalhargaout(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "mrntotalhargaout can't be empty" : GoTo selesai
        End If

        'mrntotalhppin(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "mrntotalhppin can't be empty" : GoTo selesai
        End If

        'mrntotalhppout(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "mrntotalhppout can't be empty" : GoTo selesai
        End If

        'mrntglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "mrntglnoref can't be empty" : GoTo selesai
        End If

        'mrninputtgl(37) As DateTime
        If Len(dataUtama(37)) = 0 Then
            result(2) = "mrninputtgl can't be empty" : GoTo selesai
        End If

        'mrnmodifikasitgl(39) As DateTime
        If Len(dataUtama(39)) = 0 Then
            result(2) = "mrnmodifikasitgl can't be empty" : GoTo selesai
        End If

        'mrncustomdbl1(49) As Double
        If Len(dataUtama(49)) = 0 Then
            result(2) = "mrncustomdbl1 can't be empty" : GoTo selesai
        End If

        'mrncustomdbl2(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "mrncustomdbl2 can't be empty" : GoTo selesai
        End If

        'mrncustomdbl3(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "mrncustomdbl3 can't be empty" : GoTo selesai
        End If

        'mrncustomdate1(52) As Date
        If Len(dataUtama(52)) = 0 Then
            result(2) = "mrncustomdate1 can't be empty" : GoTo selesai
        End If

        'mrncustomdate2(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "mrncustomdate2 can't be empty" : GoTo selesai
        End If

        'mrncustomdate3(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "mrncustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "mrnid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrngudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrngudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrngudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnbagianmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnbagianmrnkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrntglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnidbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnidpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnidwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnidmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatuspdin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatuspdout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrninputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrninputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrnmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrnisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "mrncustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "mrncustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "mrnid~mrncabang~mrnlokasi~mrngudangasal~mrngudangproduksi~mrngudangtujuan~mrnsumber~mrnjenis~mrnautonotransaksi~mrnnotransaksi~mrntgl~mrnkodepa~mrnbagianmrn~mrnbagianmrnkontak~mrntgldipakai~mrnestimasikerja~mrnmatauang~mrnkurs~mrntotalhargain~mrntotalhargaout~mrntotalhppin~mrntotalhppout~mrnuraian~mrncatatan~mrnnoref~mrntglnoref~mrnidbom~mrnidpdr~mrnidwo~mrnidmrs~mrnstatuspdin~mrnstatuspdout~mrnstatus~mrnstatussebelumnya~mrnjmlrevisi~mrncetakanke~mrninputuser~mrninputtgl~mrnmodifikasiuser~mrnmodifikasitgl~mrnisclose~mrncustomtext1~mrncustomtext2~mrncustomtext3~mrncustomtext4~mrncustomtext5~mrncustomint1~mrncustomint2~mrncustomint3~mrncustomdbl1~mrncustomdbl2~mrncustomdbl3~mrncustomdate1~mrncustomdate2~mrncustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'Variabel BatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, jmlbarang As Double = 0, idmrsout As Integer = 0

        Dim ftExistOutstandingMrsOut As String = "", ftOutstandingMrsOut As String = ""
        Dim updNilaiMrsOut As String = "", updFilterMrsOut As String = ""

        Dim ftExistStok As String = "", ftStok As String = ""
        Dim updStokOut As String = "", gudangOut As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idmrnout(0) As Integer, idmrn(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususkeluar(14) As Integer, 
        'idhppfifokeluar(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idbomout(28) As Integer, idpdrout(29) As Integer, 
        'idwoout(30) As Integer, idmrsout(31) As Integer, jmlpd(32) As Double, statuspd(33) As Integer, isclose(34) As Integer, 
        'customtext1(35) As String, customtext2(36) As String, customtext3(37) As String, customdbl1(38) As Double, customdbl2(39) As Double, 
        'customdbl3(40) As Double, customdate1(41) As Date, customdate2(42) As Date, customdate3(43) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable DETAIL
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idmrnout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususkeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifokeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbomout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpdrout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idwoout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idmrsout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim Jmldtdetail As Integer = dataDetail.Length
        For i = 1 To Jmldtdetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 44) Then
                result(2) = "Detail Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idmrnout(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail Row : " & i & " - idmrnout required numeric." : GoTo selesai
            End If
            'idmrn(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail Row : " & i & " - idmrn required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Detail Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Detail Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Detail Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idhppkhususkeluar(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Detail Row : " & i & " - idhppkhususkeluar required numeric." : GoTo selesai
            End If
            'idhppfifokeluar(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Detail Row : " & i & " - idhppfifokeluar required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Detail Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idbomout(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail Row : " & i & " - idbomout required numeric." : GoTo selesai
            End If
            'idpdrout(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail Row : " & i & " - idpdrout required numeric." : GoTo selesai
            End If
            'idwoout(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail Row : " & i & " - idwoout required numeric." : GoTo selesai
            End If
            'idmrsout(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail Row : " & i & " - idmrsout required numeric." : GoTo selesai
            End If
            'jmlpd(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(41) As Date
            If (IsDate(dataRowDetail(41)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(42) As Date
            If (IsDate(dataRowDetail(42)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(43) As Date
            If (IsDate(dataRowDetail(43)) = False) Then
                result(2) = "Detail Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Detail Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Detail Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Detail Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Detail Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(16) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Detail Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Detail Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(19) As String
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Detail Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(19)) > 25 Then
                result(2) = "Detail Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(20) As String
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Detail Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Detail Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(21) As String
            'If Len(dataRowDetail(21)) = 0 Then
            '    result(2) = "Detail Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(21)) > 25 Then
                result(2) = "Detail Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmlpd(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(41) As Date
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(42) As Date
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(43) As Date
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Detail Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idmrnout~idmrn~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususkeluar~idhppfifokeluar~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomout~idpdrout~idwoout~idmrsout~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43)) = False Then
                result(2) = "Detail Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , idmrsout(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : idmrsout = dataRowDetail(31)
            'gudangtujuan(21) As String    , gudangproduksi(20) As String
            gudangIn = dataRowDetail(21) : gudangOut = dataRowDetail(20)

            'Filter barang (serial batch)
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI OUTSTANDING -------------------------
            'MRS
            If idmrsout <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstandingMrsOut = IIf(Len(ftExistOutstandingMrsOut.ToString) = 0, "", ftExistOutstandingMrsOut & " UNION ")
                ftExistOutstandingMrsOut = String.Concat(ftExistOutstandingMrsOut, "SELECT EXISTS(SELECT 1 FROM m6_mrs_out JOIN m6_mrs ON idmrs = mrsid WHERE idmrsout = '" & idmrsout & "' AND (mrsstatus = 2 OR mrsstatus = 3 OR mrsstatus = 4 OR mrsstatus = 7) LIMIT 1) as rowExists, '" & idmrsout & "' as idmrsout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idmrsout=" & idmrsout)
                ftOutstandingMrsOut = IIf(Len(ftOutstandingMrsOut.ToString) = 0, "", ftOutstandingMrsOut & " OR ")
                ftOutstandingMrsOut = String.Concat(ftOutstandingMrsOut, " (mrsout.idmrsout = " & idmrsout & " AND " & Outstanding & " > (mrsout.jmlbarang - mrsout.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiMrsOut = String.Concat("WHEN '" & idmrsout & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiMrsOut)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterMrsOut = IIf(Len(updFilterMrsOut.ToString) = 0, "", updFilterMrsOut & " OR ")
                updFilterMrsOut = String.Concat(updFilterMrsOut, "(idmrsout = '" & idmrsout & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangproduksi='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA BATCH -------------------------------------------------------
        'nbtid(0) As Integer, nbtjenismutasi(1) As Integer, nbtidbarang(2) As Integer, nbtkode(3) As String, nbtsumber(4) As String, 
        'nbtidtransaksi(5) As Integer, nbtsatuan(6) As String, nbtjml(7) As Double, nbtcustomtext1(8) As String, nbtcustomtext2(9) As String, 
        'nbtcustomtext3(10) As String, nbtcustomdbl1(11) As Double, nbtcustomdbl2(12) As Double, nbtcustomdbl3(13) As Double, nbtcustomdate1(14) As Date, 
        'nbtcustomdate2(15) As Date, nbtcustomdate3(16) As Date, nbtgudang(17) As String, nbtidbatchin(18) As Integer

        'MAPPING BUAT FLEX DATA BATCH -----------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin

        'Buat datatable BATCH
        Dim dtbatch As New DataTable
        AsDataTableTambahField(dtbatch, "nbtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbatch, "nbtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidbatchin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

        'CEK PARAMETER DATA BATCH
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA BATCH ======================================================
            'SPLIT PARAMETER DATA BATCH
            dataBatch = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA BATCH ===============================================

            'VALIDASI DAN SET DATA ROW BATCH ==================================================
            Dim JmlDtBatch As Integer = dataBatch.Length
            For i = 1 To JmlDtBatch
                'SPLIT DATA DETAIL
                dataRowBatch = dataBatch(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA BATCH -----------------------------------
                'CEK ARRAY DATA BATCH
                If (dataRowBatch.Length <> 19) Then
                    result(2) = "Batch Row : " & i & " - Invalid batch number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowBatch(1) = 0
                If (IsNumeric(dataRowBatch(1)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjenismutasi required numeric." : GoTo selesai
                End If
                'nbtidbarang(2) As Integer
                If (IsNumeric(dataRowBatch(2)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbarang required numeric." : GoTo selesai
                End If
                'nbtidtransaksi(5) As Integer
                If (IsNumeric(dataRowBatch(5)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidtransaksi required numeric." : GoTo selesai
                End If
                'nbtjml(7) As Double
                If (IsNumeric(dataRowBatch(7)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjml required numeric." : GoTo selesai
                End If
                'nbtcustomdbl1(11) As Double
                If (IsNumeric(dataRowBatch(11)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl2(12) As Double
                If (IsNumeric(dataRowBatch(12)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl3(13) As Double
                If (IsNumeric(dataRowBatch(13)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 required numeric." : GoTo selesai
                End If
                'nbtcustomdate1(14) As Date
                If (IsDate(dataRowBatch(14)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 required date." : GoTo selesai
                End If
                'nbtcustomdate2(15) As Date
                If (IsDate(dataRowBatch(15)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 required date." : GoTo selesai
                End If
                'nbtcustomdate3(16) As Date
                If (IsDate(dataRowBatch(16)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 required date." : GoTo selesai
                End If
                'nbtidbatchin(18) As Integer
                If (IsNumeric(dataRowBatch(18)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbatchin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA BATCH -----------------------------------

                'VALIDASI DATA BATCH ---------------------------------------
                'nbtkode(3) As String
                If Len(dataRowBatch(3)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(3)) > 100 Then
                    result(2) = "Batch Row : " & i & " - nbtkode should not be more than 100 character." : GoTo selesai
                End If

                'nbtsumber(4) As String
                If Len(dataRowBatch(4)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(4)) > 10 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber should not be more than 10 character." : GoTo selesai
                End If

                'nbtsatuan(6) As String
                If Len(dataRowBatch(6)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(6)) > 25 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nbtjml(7) As Double
                If Len(dataRowBatch(7)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtjml can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl1(11) As Double
                If Len(dataRowBatch(11)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl2(12) As Double
                If Len(dataRowBatch(12)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl3(13) As Double
                If Len(dataRowBatch(13)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate1(14) As Date
                If Len(dataRowBatch(14)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate2(15) As Date
                If Len(dataRowBatch(15)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate3(16) As Date
                If Len(dataRowBatch(16)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 can't be empty" : GoTo selesai
                End If

                'nbtgudang(17) As String
                If Len(dataRowBatch(17)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA BATCH --------------------------------

                If AsDataTableTambahData(dtbatch, "nbtid~nbtjenismutasi~nbtidbarang~nbtkode~nbtsumber~nbtidtransaksi~nbtsatuan~nbtjml~nbtcustomtext1~nbtcustomtext2~nbtcustomtext3~nbtcustomdbl1~nbtcustomdbl2~nbtcustomdbl3~nbtcustomdate1~nbtcustomdate2~nbtcustomdate3~nbtgudang~nbtidbatchin", dataRowBatch(0) & "~" & dataRowBatch(1) & "~" & dataRowBatch(2) & "~" & dataRowBatch(3) & "~" & dataRowBatch(4) & "~" & dataRowBatch(5) & "~" & dataRowBatch(6) & "~" & dataRowBatch(7) & "~" & dataRowBatch(8) & "~" & dataRowBatch(9) & "~" & dataRowBatch(10) & "~" & dataRowBatch(11) & "~" & dataRowBatch(12) & "~" & dataRowBatch(13) & "~" & dataRowBatch(14) & "~" & dataRowBatch(15) & "~" & dataRowBatch(16) & "~" & dataRowBatch(17) & "~" & dataRowBatch(18)) = False Then
                    result(2) = "Batch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA BATCH ===========================================

        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'nstid(0) As Integer, nstjenismutasi(1) As Integer, nstidbarang(2) As Integer, nstkode(3) As String, nstsumber(4) As String, 
        'nstidtransaksi(5) As Integer, nstsatuan(6) As String, nstjml(7) As Double, nstcustomtext1(8) As String, nstcustomtext2(9) As String, 
        'nstcustomtext3(10) As String, nstcustomdbl1(11) As Double, nstcustomdbl2(12) As Double, nstcustomdbl3(13) As Double, nstcustomdate1(14) As Date, 
        'nstcustomdate2(15) As Date, nstcustomdate3(16) As Date, nstgudang(17) As String, nstidserialin(18) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'nstid, nstjenismutasi, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, nstgudang, nstidserialin

        'Buat datatable serial
        Dim dtserial As New DataTable
        AsDataTableTambahField(dtserial, "nstid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtserial, "nstcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidserialin", AsEnumTypeData.AsInt64)

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

        'CEK PARAMETER DATA SERIAL
        If dataSplit(3).Length > 0 Then
            'VALIDASI DAN SET DATA SERIAL ======================================================
            'SPLIT PARAMETER DATA SERIAL
            dataSerial = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA SERIAL ===============================================

            'VALIDASI DAN SET DATA ROW SERIAL ==================================================
            Dim JmlDtSerial As Integer = dataSerial.Length
            For i = 1 To JmlDtSerial
                'SPLIT DATA SERIAL
                dataRowSerial = dataSerial(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA SERIAL -----------------------------------
                'CEK ARRAY DATA SERIAL
                If (dataRowSerial.Length <> 19) Then
                    result(2) = "Serial Row : " & i & " - Invalid serial number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW SERIAL ----------------------------

                'VALIDASI TIPE DATA SERIAL ------------------------------------------
                'nstid(0) As Integer
                If (IsNumeric(dataRowSerial(0)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstid required numeric." : GoTo selesai
                End If
                'nstjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowSerial(1) = 0
                If (IsNumeric(dataRowSerial(1)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjenismutasi required numeric." : GoTo selesai
                End If
                'nstidbarang(2) As Integer
                If (IsNumeric(dataRowSerial(2)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidbarang required numeric." : GoTo selesai
                End If
                'nstidtransaksi(5) As Integer
                If (IsNumeric(dataRowSerial(5)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidtransaksi required numeric." : GoTo selesai
                End If
                'nstjml(7) As Double
                If (IsNumeric(dataRowSerial(7)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjml required numeric." : GoTo selesai
                End If
                'nstcustomdbl1(11) As Double
                If (IsNumeric(dataRowSerial(11)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 required numeric." : GoTo selesai
                End If
                'nstcustomdbl2(12) As Double
                If (IsNumeric(dataRowSerial(12)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 required numeric." : GoTo selesai
                End If
                'nstcustomdbl3(13) As Double
                If (IsNumeric(dataRowSerial(13)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 required numeric." : GoTo selesai
                End If
                'nstcustomdate1(14) As Date
                If (IsDate(dataRowSerial(14)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 required date." : GoTo selesai
                End If
                'nstcustomdate2(15) As Date
                If (IsDate(dataRowSerial(15)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 required date." : GoTo selesai
                End If
                'nstcustomdate3(16) As Date
                If (IsDate(dataRowSerial(16)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 required date." : GoTo selesai
                End If
                'nstidserialin(18) As Integer
                If (IsNumeric(dataRowSerial(18)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidserialin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA SERIAL -----------------------------------

                'VALIDASI DATA SERIAL ---------------------------------------
                'nstkode(3) As String
                If Len(dataRowSerial(3)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(3)) > 100 Then
                    result(2) = "Serial Row : " & i & " - nstkode should not be more than 100 character." : GoTo selesai
                End If

                'nstsumber(4) As String
                If Len(dataRowSerial(4)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(4)) > 10 Then
                    result(2) = "Serial Row : " & i & " - nstsumber should not be more than 10 character." : GoTo selesai
                End If

                'nstsatuan(6) As String
                If Len(dataRowSerial(6)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(6)) > 25 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nstjml(7) As Double
                If Len(dataRowSerial(7)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstjml can't be empty" : GoTo selesai
                End If

                'nstcustomdbl1(11) As Double
                If Len(dataRowSerial(11)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl2(12) As Double
                If Len(dataRowSerial(12)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl3(13) As Double
                If Len(dataRowSerial(13)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nstcustomdate1(14) As Date
                If Len(dataRowSerial(14)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 can't be empty" : GoTo selesai
                End If

                'nstcustomdate2(15) As Date
                If Len(dataRowSerial(15)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 can't be empty" : GoTo selesai
                End If

                'nstcustomdate3(16) As Date
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 can't be empty" : GoTo selesai
                End If

                'nstgudang(17) As String
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA SERIAL --------------------------------

                If AsDataTableTambahData(dtserial, "nstid~nstjenismutasi~nstidbarang~nstkode~nstsumber~nstidtransaksi~nstsatuan~nstjml~nstcustomtext1~nstcustomtext2~nstcustomtext3~nstcustomdbl1~nstcustomdbl2~nstcustomdbl3~nstcustomdate1~nstcustomdate2~nstcustomdate3~nstgudang~nstidserialin", dataRowSerial(0) & "~" & dataRowSerial(1) & "~" & dataRowSerial(2) & "~" & dataRowSerial(3) & "~" & dataRowSerial(4) & "~" & dataRowSerial(5) & "~" & dataRowSerial(6) & "~" & dataRowSerial(7) & "~" & dataRowSerial(8) & "~" & dataRowSerial(9) & "~" & dataRowSerial(10) & "~" & dataRowSerial(11) & "~" & dataRowSerial(12) & "~" & dataRowSerial(13) & "~" & dataRowSerial(14) & "~" & dataRowSerial(15) & "~" & dataRowSerial(16) & "~" & dataRowSerial(17) & "~" & dataRowSerial(18)) = False Then
                    result(2) = "Serial Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
        End If


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("mrntgl")), AsFormatTanggal(drutama("mrntgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                If drutama("mrnstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingMrsOut, ftOutstandingMrsOut, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangproduksi")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("Mrnid")
                    notransaksi = drutama("Mrnnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(Mrnid), Mrnnotransaksi FROM M6_Mrn WHERE Mrnid='" & result(4) & "' AND mrnstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(Mrnid) FROM M6_Mrn WHERE Mrnnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_mrn_history
                        Dim rsSimpanHistory As String = SimpanHistory.m6_Mrn_HistorySimpan("" & paramSplit(0) & "★M6_Mrn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("mrnsumber")) & "▼" & FixQuotes(drutama("mrnid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Mrn set mrncabang  = '" & FixQuotes(drutama("mrncabang")) & "', mrnlokasi  = '" & FixQuotes(drutama("mrnlokasi")) & "', mrngudangasal  = '" & FixQuotes(drutama("mrngudangasal")) & "', mrngudangproduksi  = '" & FixQuotes(drutama("mrngudangproduksi")) & "', mrngudangtujuan  = '" & FixQuotes(drutama("mrngudangtujuan")) & "', mrnsumber  = '" & FixQuotes(drutama("mrnsumber")) & "', mrnjenis  = '" & FixQuotes(drutama("mrnjenis")) & "', mrnautonotransaksi  = " & drutama("mrnautonotransaksi") & ", mrnnotransaksi  = '" & FixQuotes(notransaksi) & "', mrntgl  = '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', mrnkodepa  = " & drutama("mrnkodepa") & ", mrnbagianmrn  = " & drutama("mrnbagianmrn") & ", mrnbagianmrnkontak  = '" & FixQuotes(drutama("mrnbagianmrnkontak")) & "', mrntgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("mrntgldipakai"))) & "', mrnestimasikerja  = '" & FixQuotes(drutama("mrnestimasikerja")) & "', mrnmatauang  = '" & FixQuotes(drutama("mrnmatauang")) & "', mrnkurs  = '" & FixDouble(drutama("mrnkurs")) & "', mrntotalhargain  = '" & FixDouble(drutama("mrntotalhargain")) & "', mrntotalhargaout  = '" & FixDouble(drutama("mrntotalhargaout")) & "', mrntotalhppin  = '" & FixDouble(drutama("mrntotalhppin")) & "', mrntotalhppout  = '" & FixDouble(drutama("mrntotalhppout")) & "', mrnuraian  = '" & FixQuotes(drutama("mrnuraian")) & "', mrncatatan  = '" & FixQuotes(drutama("mrncatatan")) & "', mrnnoref  = '" & FixQuotes(drutama("mrnnoref")) & "', mrntglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("mrntglnoref"))) & "', mrnidbom  = " & drutama("mrnidbom") & ", mrnidpdr  = " & drutama("mrnidpdr") & ", mrnidwo  = " & drutama("mrnidwo") & ", mrnidmrs  = " & drutama("mrnidmrs") & ", mrnstatuspdin  = " & drutama("mrnstatuspdin") & ", mrnstatuspdout  = " & drutama("mrnstatuspdout") & ", mrnstatus  = " & drutama("mrnstatus") & ", mrnstatussebelumnya  = " & drutama("mrnstatussebelumnya") & ", mrnjmlrevisi  = mrnjmlrevisi+1, mrncetakanke  = " & drutama("mrncetakanke") & ", mrnmodifikasiuser  = " & drutama("mrnmodifikasiuser") & ", mrnmodifikasitgl  = NOW(), mrncustomtext1  = '" & FixQuotes(drutama("mrncustomtext1")) & "', mrncustomtext2  = '" & FixQuotes(drutama("mrncustomtext2")) & "', mrncustomtext3  = '" & FixQuotes(drutama("mrncustomtext3")) & "', mrncustomtext4  = '" & FixQuotes(drutama("mrncustomtext4")) & "', mrncustomtext5  = '" & FixQuotes(drutama("mrncustomtext5")) & "', mrncustomint1  = " & drutama("mrncustomint1") & ", mrncustomint2  = " & drutama("mrncustomint2") & ", mrncustomint3  = " & drutama("mrncustomint3") & ", mrncustomdbl1  = '" & FixDouble(drutama("mrncustomdbl1")) & "', mrncustomdbl2  = '" & FixDouble(drutama("mrncustomdbl2")) & "', mrncustomdbl3  = '" & FixDouble(drutama("mrncustomdbl3")) & "', mrncustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate1"))) & "', mrncustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate2"))) & "', mrncustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate3"))) & "' where mrnid = '" & drutama("mrnid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("Mrnautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Mrncabang"), drutama("Mrnlokasi"), drutama("Mrnsumber"), drutama("Mrntgl"))
                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksi(0) = 1) Then
                            notransaksi = arrNotransaksi(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNotransaksi(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOTRANSAKSI ==================================

                    Else
                        notransaksi = drutama("Mrnnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(Mrnid) FROM m6_mrn WHERE Mrnnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Mrn (mrncabang, mrnlokasi, mrngudangasal, mrngudangproduksi, mrngudangtujuan, mrnsumber, mrnjenis, mrnautonotransaksi, mrnnotransaksi, mrntgl, mrnkodepa, mrnbagianmrn, mrnbagianmrnkontak, mrntgldipakai, mrnestimasikerja, mrnmatauang, mrnkurs, mrntotalhargain, mrntotalhargaout, mrntotalhppin, mrntotalhppout, mrnuraian, mrncatatan, mrnnoref, mrntglnoref, mrnidbom, mrnidpdr, mrnidwo, mrnidmrs, mrnstatuspdin, mrnstatuspdout, mrnstatus, mrnstatussebelumnya, mrnjmlrevisi, mrncetakanke, mrninputuser, mrninputtgl, mrnmodifikasiuser, mrnmodifikasitgl, mrnisclose, mrncustomtext1, mrncustomtext2, mrncustomtext3, mrncustomtext4, mrncustomtext5, mrncustomint1, mrncustomint2, mrncustomint3, mrncustomdbl1, mrncustomdbl2, mrncustomdbl3, mrncustomdate1, mrncustomdate2, mrncustomdate3) values('" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(drutama("mrngudangasal")) & "', '" & FixQuotes(drutama("mrngudangproduksi")) & "', '" & FixQuotes(drutama("mrngudangtujuan")) & "', '" & FixQuotes(drutama("mrnsumber")) & "', '" & FixQuotes(drutama("mrnjenis")) & "', " & drutama("mrnautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnkodepa") & ", " & drutama("mrnbagianmrn") & ", '" & FixQuotes(drutama("mrnbagianmrnkontak")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgldipakai"))) & "', '" & FixQuotes(drutama("mrnestimasikerja")) & "', '" & FixQuotes(drutama("mrnmatauang")) & "', '" & FixDouble(drutama("mrnkurs")) & "', '" & FixDouble(drutama("mrntotalhargain")) & "', '" & FixDouble(drutama("mrntotalhargaout")) & "', '" & FixDouble(drutama("mrntotalhppin")) & "', '" & FixDouble(drutama("mrntotalhppout")) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(drutama("mrnnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntglnoref"))) & "', " & drutama("mrnidbom") & ", " & drutama("mrnidpdr") & ", " & drutama("mrnidwo") & ", " & drutama("mrnidmrs") & ", " & drutama("mrnstatuspdin") & ", " & drutama("mrnstatuspdout") & ", " & drutama("mrnstatus") & ", " & drutama("mrnstatussebelumnya") & ", " & drutama("mrnjmlrevisi") & ", " & drutama("mrncetakanke") & ", " & drutama("mrninputuser") & ", NOW(), " & drutama("mrnmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("mrnisclose") & ", '" & FixQuotes(drutama("mrncustomtext1")) & "', '" & FixQuotes(drutama("mrncustomtext2")) & "', '" & FixQuotes(drutama("mrncustomtext3")) & "', '" & FixQuotes(drutama("mrncustomtext4")) & "', '" & FixQuotes(drutama("mrncustomtext5")) & "', " & drutama("mrncustomint1") & ", " & drutama("mrncustomint2") & ", " & drutama("mrncustomint3") & ", '" & FixDouble(drutama("mrncustomdbl1")) & "', '" & FixDouble(drutama("mrncustomdbl2")) & "', '" & FixDouble(drutama("mrncustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrncustomdate3"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDB("select Mrnid from M6_Mrn where Mrnnotransaksi='" & notransaksi & "' AND Mrninputuser= '" & userid & "' order by Mrnmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Mrn_Out where idMrn = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idmrnout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususkeluar") & ", " & dr1("idhppfifokeluar") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomout") & ", " & dr1("idpdrout") & ", " & dr1("idwoout") & ", " & dr1("idmrsout") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Mrn_Out(idmrnout, idmrn, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususkeluar, idhppfifokeluar, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, idwoout, idmrsout, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'MRN'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses batch
                If (dtbatch.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nbtjenismutasi") & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_Transaction(nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus serial ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'MRN'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses serial
                If (dtserial.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nstjenismutasi") & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_Transaction(nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("mrnstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ===================================================
                    Dim updNilaiMrsUtamaOut = "", updFilterMrsUtama = ""

                    'MRS OUT
                    If Len(updNilaiMrsOut) > 0 Then
                        'UPDATE DETAIL OUT
                        sql = "UPDATE m6_mrs_out SET jmlrealisasi = (CASE idmrsout " & updNilaiMrsOut & " ELSE jmlrealisasi END) WHERE " & updFilterMrsOut
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'AMBIL ID UTAMA OUT
                        Dim ftDetail As String = ""
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idmrs FROM m6_mrs_out WHERE " & updFilterMrsOut & " GROUP BY idmrs")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idmrs = '" & dr1("idmrs") & "')")
                            Next
                        End If

                        'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                        If Len(ftDetail) > 0 Then
                            dtOut = AsDataTableAmbilDariDB("SELECT idmrs, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_mrs_out WHERE " & ftDetail & " GROUP BY idmrs")
                            If dtOut.Rows.Count > 0 Then
                                For Each dr1 As DataRow In dtOut.Rows
                                    '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                    Dim statusOut As Integer = 0
                                    If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                        statusOut = 2
                                    ElseIf dr1("jmlrealisasi") < 1 Then
                                        statusOut = 0
                                    Else
                                        statusOut = 1
                                    End If

                                    '2. SET NILAI UPDATE OUTSTANDING
                                    updNilaiMrsUtamaOut = String.Concat(updNilaiMrsUtamaOut, "WHEN '" & dr1("idmrs") & "' THEN '" & statusOut & "' ")

                                    '3. SET FILTERUPDATE OUTSTANDING
                                    updFilterMrsUtama = IIf(Len(updFilterMrsUtama.ToString) = 0, "", updFilterMrsUtama & " OR ")
                                    updFilterMrsUtama = String.Concat(updFilterMrsUtama, "(mrsid = '" & dr1("idmrs") & "')")
                                Next
                            End If
                        End If
                    End If

                    'MRS UTAMA, UPDATE STATUS OUT
                    If Len(updNilaiMrsUtamaOut) > 0 Then
                        sql = "UPDATE m6_mrs SET mrsstatusrealisasiout = (CASE mrsid " & updNilaiMrsUtamaOut & " ELSE mrsstatusrealisasiout END) WHERE " & updFilterMrsUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'AMBIL GUDANG TUJUAN DARI UTAMA ================================================
                    'GUDANG TUJUAN UTAMA DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG PRODUKSI 'A', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'A' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TUJUAN BERTAMBAH
                    Dim SetGudang As String = drutama("mrngudangtujuan")
                    'END OF AMBIL GUDANG TUJUAN DARI UTAMA =========================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO BATCH =========================================================

                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If
                    'END OF INSERT NO SERIAL ========================================================


                    'UPDATE STOK ====================================================================
                    'STOK KELUAR
                    If Len(updStokOut) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'STOK MASUK
                    If Len(updStokIn) > 0 Then
                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT mrno.idmrnout, mrno.idbarang, mrno.namabarang, mrno.tipebarang, mrno.jml, mrno.satuan, mrno.jmlbarang, mrno.satuanbarang, mrno.matauang, mrno.kurs, mrno.harga, mrno.hpp, mrno.idhppkhususkeluar, mrno.gudangasal, mrno.gudangproduksi, mrno.gudangtujuan, mrno.catatan, mrno.costcenter, mrno.divisi, mrno.subdivisi, mrno.proyek, mrn.mrninputtgl, i.bhpp FROM m6_mrn_out mrno JOIN m6_mrn mrn ON mrno.idmrn = mrn.mrnid JOIN m1_item i ON mrno.idbarang = i.bid WHERE mrno.idmrn = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("hpp"))

                            'POSTING BARANG KELUAR (gudangproduksi)
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                              cabang,                                    lokasi,                                     gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,        idhppikm,          idhppikk,                               hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', " & drutama("mrnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrnsumber")) & "', " & result(4) & ", " & dr1("idmrnout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnbagianmrn") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangtujuan)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                              cabang,                                    lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,        idhppikm,          idhppikk,                               hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("mrnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrnsumber")) & "', " & result(4) & ", " & dr1("idmrnout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnbagianmrn") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "MRN", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'ambil moduleid dan menuid dari m0_nomor
                Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'")
                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

                sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF INSERT USER LOG =============================================================

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_MrnUpdateStatusOld(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", nilaiStatus As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"
        Dim idtransaksi As String = "", idtransaksih As String = ""
        Dim dtdetail As DataTable, dtdetailOut As DataTable
        Dim isDelete As Boolean = False

        Dim Filter As String = "", Sorting As String = "", search As String = ""

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISDELETE =========================================================
        'CEK ISDELETE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isdelete required numeric." : GoTo selesai
        Else
            'SET ISDELETE
            If (Val(paramSplit(4)) = 1) Then
                isDelete = True
            Else
                isDelete = False
            End If
        End If
        'END OF VALIDASI DAN SET ISDELETE ==================================================

        'VALIDASI DAN SET NILAISTATUS ======================================================
        'SPILIT PARAMETER NILAISTATUS
        nilaiSplit = paramSplit(5).Split(sptSubParam)

        'CEK ARRAY NILAISTATUS
        If (nilaiSplit.Length <> 2) Then
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If

        'CEK IDTRANSAKSI
        If (IsNumeric(nilaiSplit(0)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = nilaiSplit(0)

        'SET NILAI STATUS
        If (Len(nilaiSplit(1)) > 0) Then
            'JIKA NUMERIC MAKA NILAISTATUS = PARAM NILAI STATUS YG DIINPUT
            'JIKA TIDAK MAKA NILAISTATUS = UNCLOSE
            If (IsNumeric(nilaiSplit(1)) = True) Then
                nilaiStatus = nilaiSplit(1)
                'JIKA NILAI STATUS < 0 ATAU NILAI STATUS > 12 MAKA NILAISTATUS TIDAK VALID
                If (nilaiStatus < 0 Or nilaiStatus > 12) Then
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            Else
                If (nilaiSplit(1).ToString.ToLower = "unclose") Then
                    nilaiStatus = "unclose"
                Else
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            End If
        Else
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET NILAISTATUS ================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "MRN", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Mrntgl, Mrnnotransaksi, Mrnstatus FROM M6_Mrn WHERE Mrnid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Mrnstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True


            'CEK PERIODE AKUNTANSI ==============================================================
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m6_mrn_history
            Dim rsSimpanHistory As String = SimpanHistory.m6_Mrn_HistorySimpan("" & paramSplit(0) & "★M6_Mrn_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                ''CEK TERKAIT ====================================================================
                ''PANGGIL QUERY TERKAIT
                'Dim query As New m0_query
                'sql = query.PanggilQuery("m6_mrn_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                ''END OF CEK TERKAIT =============================================================


                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                Dim updNilaiMrsUtamaOut = "", updFilterMrsUtama = ""
                Dim idbarang As Integer = 0, jmlbarang As Double = 0
                Dim idmrsout As Integer = 0
                Dim updNilaiMrsOut As String = "", updFilterMrsOut As String = ""

                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updStokOut As String = "", gudangOut As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""

                'AMBIL DATA DETAIL OUT
                dtdetailOut = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudangasal, gudangproduksi, gudangtujuan, idmrsout, urutan FROM m6_mrn_out WHERE idmrn = '" & idtransaksi & "'")
                If dtdetailOut.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetailOut.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudangIn = dr1("gudangproduksi") : gudangOut = dr1("gudangtujuan") : idmrsout = dr1("idmrsout")

                        'UPDATE OUTSTANDING ---------------------------
                        If idmrsout <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING 
                            Dim Outstanding As Double = AsDataTableDSum(dtdetailOut, "jmlbarang", "idmrsout=" & idmrsout)
                            updNilaiMrsOut = String.Concat("WHEN '" & idmrsout & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiMrsOut)

                            '2. SET FILTERUPDATE OUTSTANDING 
                            updFilterMrsOut = IIf(Len(updFilterMrsOut.ToString) = 0, "", updFilterMrsOut & " OR ")
                            updFilterMrsOut = String.Concat(updFilterMrsOut, "(idmrsout = '" & idmrsout & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                        'VALIDASI STOK --------------------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        'END OF VALIDASI STOK -------------------------------------

                    Next
                Else
                    result(2) = "Detail transaction not found. (Material)" : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetailOut, "", "", ftExistStok, ftStok, "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'MRS OUT
                If Len(updNilaiMrsOut) > 0 Then
                    'UPDATE DETAIL OUT
                    sql = "UPDATE m6_mrs_out SET jmlrealisasi = (CASE idmrsout " & updNilaiMrsOut & " ELSE jmlrealisasi END) WHERE " & updFilterMrsOut
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'AMBIL ID UTAMA OUT
                    Dim ftDetail As String = ""
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idmrs FROM m6_mrs_out WHERE " & updFilterMrsOut & " GROUP BY idmrs")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idmrs = '" & dr1("idmrs") & "')")
                        Next
                    End If

                    'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                    If Len(ftDetail) > 0 Then
                        dtOut = AsDataTableAmbilDariDB("SELECT idmrs, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_mrs_out WHERE " & ftDetail & " GROUP BY idmrs")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                Dim statusOut As Integer = 0
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiMrsUtamaOut = String.Concat(updNilaiMrsUtamaOut, "WHEN '" & dr1("idmrs") & "' THEN '" & statusOut & "' ")

                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterMrsUtama = IIf(Len(updFilterMrsUtama.ToString) = 0, "", updFilterMrsUtama & " OR ")
                                updFilterMrsUtama = String.Concat(updFilterMrsUtama, "(mrsid = '" & dr1("idmrs") & "')")
                            Next
                        End If
                    End If
                End If

                'MRS UTAMA, UPDATE STATUS OUT
                If Len(updNilaiMrsUtamaOut) > 0 Then
                    sql = "UPDATE m6_mrs SET mrsstatusrealisasiout = (CASE mrsid " & updNilaiMrsUtamaOut & " ELSE mrsstatusrealisasiout END) WHERE " & updFilterMrsUtama
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDB("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'")
                If dtBatch.Rows.Count > 0 Then
                    'DELETE NO BATCH IN MASUK ---------------------------
                    sql = "DELETE FROM m1_no_batch_in WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDB("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'")
                If dtSerial.Rows.Count > 0 Then
                    'DELETE NO SERIAL IN MASUK --------------------------
                    sql = "DELETE FROM m1_no_serial_in WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE NO SERIAL =======================================================


                'UPDATE STOK ====================================================================
                'STOK KELUAR
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK =============================================================


                'DELETE TRANSAKSI BARANG ========================================================
                'HAPUS DI M1_ITEM_TRANSACTION
                sql = "DELETE FROM m1_item_transaction WHERE sumber = '" & sumber & "' AND idutama = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG =================================================

            End If

            'update status utama
            sql = "UPDATE M6_Mrn SET Mrnstatus = " & nilaiStatus & ", Mrnmodifikasiuser='" & userid & "', Mrnmodifikasitgl = NOW(), Mrnposting = 0, Mrnpostingtgl = '1971-01-01 00:00:00', Mrnjmlrevisi = Mrnjmlrevisi + 1 WHERE Mrnid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M6_MrnSearch(PostWsSearch(paramSplit(0), "M6_MrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***' 

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi
        End Try

        objCmd = Nothing
        'Con1.Close()
        'Con1 = Nothing
        'UPDATE OF SIMPAN KE DATABASE ==========================================================

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

    <WebMethod()>
    Public Function M6_MrnDeleteOld(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "MRN", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Mrnid, Mrnnotransaksi FROM M6_Mrn WHERE Mrnid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT mrncabang, mrnlokasi, mrnsumber, mrnautonotransaksi, mrnnotransaksi, mrntgl"
            sql &= " FROM M6_mrn"
            sql &= " WHERE mrnid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("mrncabang")
                lokasi = dtNomorNext.Rows(0)("mrnlokasi")
                sumber = dtNomorNext.Rows(0)("mrnsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("mrnautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("mrnnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("mrntgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'HAPUS BATCH
            sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS SERIAL
            sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M6_Mrn_Out WHERE idMrn ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M6_Mrn WHERE Mrnid ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'UPDATE NOMOR BERIKUTNYA ============================================================
            'JIKA AUTO NO. TRANSAKSI
            If autonotransaksi = 1 Then
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi)
                Dim arrNomorNext(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                arrNomorNext = rsNomorNext.Split(sptSubParam)
                'Cek success M0_DeleteNotransaksi
                If (arrNomorNext(0) = 1) Then
                    sql = arrNomorNext(3)
                    'Tambah query update m0_nomor_next
                    If Len(sql) > 0 Then
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
                    result(2) = arrNomorNext(1) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF UPDATE NOMOR BERIKUTNYA =====================================================


            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M6_MrnSearch(PostWsSearch(paramSplit(0), "M6_MrnSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi

        End Try

        objCmd = Nothing
        'Con1.Close()
        'Con1 = Nothing
        'END OF DELETE DI DATABASE ==========================================================

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