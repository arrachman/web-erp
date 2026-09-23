Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_vp
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_VpSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPay(), dataRowPay() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, tglLunas As String = ""


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


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        ''CEK PAGENUMBER
        'If (IsNumeric(pagingSplit(0)) = False) Then
        '    result(2) = "pageNumber required numeric." : GoTo selesai
        'End If

        ''CEK ITEMLIMIT
        'If (IsNumeric(pagingSplit(1)) = False) Then
        '    result(2) = "itemLimit required numeric." : GoTo selesai
        'End If

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'vpid(0) As Integer, vpcabang(1) As String, vplokasi(2) As String, vpgudang(3) As String, vpsumber(4) As String, 
        'vpautonotransaksi(5) As Integer, vpnotransaksi(6) As String, vptgl(7) As Date, vpkodepa(8) As Integer, vpsupplier(9) As Integer, 
        'vpsupplierkontak(10) As String, vp1alamat1(11) As String, vp1alamat2(12) As String, vp1alamat3(13) As String, vp2alamat1(14) As String, 
        'vp2alamat2(15) As String, vp2alamat3(16) As String, vpbagianpembayaran(17) As Integer, vpuraian(18) As String, vpcatatan(19) As String, 
        'vpnoref(20) As String, vptglnoref(21) As Date, vpcarabayar(22) As Integer, vptglbayar(23) As Date, vpmatauang(24) As String, 
        'vpkurs(25) As Double, vptotalap(26) As Double, vptotalapvalas(27) As Double, vptotalar(28) As Double, vptotalarvalas(29) As Double, 
        'vpbayar(30) As Double, vpbayarvalas(31) As Double, vpselisihkurs(32) As Double, vprekselisihkurs(33) As String, vpdiskontermin(34) As Double, 
        'vpdiskonterminvalas(35) As Double, vprekdiskontermin(36) As String, vpidvpp(37) As Integer, vpstatus(38) As Integer, vpstatussebelumnya(39) As Integer, 
        'vpjmlrevisi(40) As Integer, vpcetakanke(41) As Integer, vpinputuser(42) As Integer, vpinputtgl(43) As DateTime, vpmodifikasiuser(44) As Integer, 
        'vpmodifikasitgl(45) As DateTime, vpisclose(46) As Integer, vpcustomtext1(47) As String, vpcustomtext2(48) As String, vpcustomtext3(49) As String, 
        'vpcustomtext4(50) As String, vpcustomtext5(51) As String, vpcustomint1(52) As Integer, vpcustomint2(53) As Integer, vpcustomint3(54) As Integer, 
        'vpcustomdbl1(55) As Double, vpcustomdbl2(56) As Double, vpcustomdbl3(57) As Double, vpcustomdate1(58) As Date, vpcustomdate2(59) As Date, 
        'vpcustomdate3(60) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpisclose, vpcustomtext1, vpcustomtext2, 
        'vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, 
        'vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 61) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'vpid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "vpid required numeric." : GoTo selesai
        End If
        'vpautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "vpautonotransaksi required numeric." : GoTo selesai
        End If
        'vptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "vptgl required date." : GoTo selesai
        End If
        'vpkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "vpkodepa required numeric." : GoTo selesai
        End If
        'vpsupplier(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "vpsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "vpsupplier can't be empty." : GoTo selesai
        End If
        'vpbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "vpbagianpembayaran required numeric." : GoTo selesai
        End If
        'vptglnoref(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "vptglnoref required date." : GoTo selesai
        End If
        'vpcarabayar(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "vpcarabayar required numeric." : GoTo selesai
        End If
        'vptglbayar(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "vptglbayar required date." : GoTo selesai
        End If
        'vpkurs(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "vpkurs required numeric." : GoTo selesai
        End If
        'vptotalap(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "vptotalap required numeric." : GoTo selesai
        End If
        'vptotalapvalas(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "vptotalapvalas required numeric." : GoTo selesai
        End If
        'vptotalar(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "vptotalar required numeric." : GoTo selesai
        End If
        'vptotalarvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "vptotalarvalas required numeric." : GoTo selesai
        End If
        'vpbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "vpbayar required numeric." : GoTo selesai
        End If
        'vpbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "vpbayarvalas required numeric." : GoTo selesai
        End If
        'vpselisihkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "vpselisihkurs required numeric." : GoTo selesai
        End If
        'vpdiskontermin(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "vpdiskontermin required numeric." : GoTo selesai
        End If
        'vpdiskonterminvalas(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "vpdiskonterminvalas required numeric." : GoTo selesai
        End If
        'vpidvpp(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "vpidvpp required numeric." : GoTo selesai
        End If
        'vpstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "vpstatus required numeric." : GoTo selesai
        End If
        'vpstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "vpstatussebelumnya required numeric." : GoTo selesai
        End If
        'vpjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "vpjmlrevisi required numeric." : GoTo selesai
        End If
        'vpcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "vpcetakanke required numeric." : GoTo selesai
        End If
        'vpinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "vpinputuser required numeric." : GoTo selesai
        End If
        'vpinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "vpinputtgl required date." : GoTo selesai
        End If
        'vpmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "vpmodifikasiuser required numeric." : GoTo selesai
        End If
        'vpmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "vpmodifikasitgl required date." : GoTo selesai
        End If
        'vpisclose(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "vpisclose required numeric." : GoTo selesai
        End If
        'vpcustomint1(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "vpcustomint1 required numeric." : GoTo selesai
        End If
        'vpcustomint2(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "vpcustomint2 required numeric." : GoTo selesai
        End If
        'vpcustomint3(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "vpcustomint3 required numeric." : GoTo selesai
        End If
        'vpcustomdbl1(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "vpcustomdbl1 required numeric." : GoTo selesai
        End If
        'vpcustomdbl2(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "vpcustomdbl2 required numeric." : GoTo selesai
        End If
        'vpcustomdbl3(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "vpcustomdbl3 required numeric." : GoTo selesai
        End If
        'vpcustomdate1(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "vpcustomdate1 required date." : GoTo selesai
        End If
        'vpcustomdate2(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "vpcustomdate2 required date." : GoTo selesai
        End If
        'vpcustomdate3(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "vpcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'vpcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "vpcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "vpcabang should not be more than 25 character." : GoTo selesai
        End If

        'vplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "vplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "vplokasi should not be more than 25 character." : GoTo selesai
        End If

        'vpsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "vpsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "vpsumber should not be more than 10 character." : GoTo selesai
        End If

        'vpnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "vpnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "vpnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'vptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "vptgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS TRANSAKSI
        tglLunas = AsFormatTanggal(dataUtama(7))

        'vptglnoref(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "vptglnoref can't be empty" : GoTo selesai
        End If

        'vptglbayar(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "vptglbayar can't be empty" : GoTo selesai
        End If

        'vpmatauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "vpmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "vpmatauang should not be more than 25 character." : GoTo selesai
        End If

        'vpkurs(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "vpkurs can't be empty" : GoTo selesai
        End If

        'vptotalap(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "vptotalap can't be empty" : GoTo selesai
        End If

        'vptotalapvalas(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "vptotalapvalas can't be empty" : GoTo selesai
        End If

        'vptotalar(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "vptotalar can't be empty" : GoTo selesai
        End If

        'vptotalarvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "vptotalarvalas can't be empty" : GoTo selesai
        End If

        'vpbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "vpbayar can't be empty" : GoTo selesai
        End If

        'vpbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "vpbayarvalas can't be empty" : GoTo selesai
        End If

        'vpselisihkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "vpselisihkurs can't be empty" : GoTo selesai
        End If

        'vpdiskontermin(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "vpdiskontermin can't be empty" : GoTo selesai
        End If

        'vpdiskonterminvalas(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "vpdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'vpinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "vpinputtgl can't be empty" : GoTo selesai
        End If

        'vpmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "vpmodifikasitgl can't be empty" : GoTo selesai
        End If

        'vpcustomdbl1(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "vpcustomdbl1 can't be empty" : GoTo selesai
        End If

        'vpcustomdbl2(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "vpcustomdbl2 can't be empty" : GoTo selesai
        End If

        'vpcustomdbl3(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "vpcustomdbl3 can't be empty" : GoTo selesai
        End If

        'vpcustomdate1(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "vpcustomdate1 can't be empty" : GoTo selesai
        End If

        'vpcustomdate2(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "vpcustomdate2 can't be empty" : GoTo selesai
        End If

        'vpcustomdate3(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "vpcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "vpid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vptglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vpbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vpselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vprekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vprekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpidvpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "vpid~vpcabang~vplokasi~vpgudang~vpsumber~vpautonotransaksi~vpnotransaksi~vptgl~vpkodepa~vpsupplier~vpsupplierkontak~vp1alamat1~vp1alamat2~vp1alamat3~vp2alamat1~vp2alamat2~vp2alamat3~vpbagianpembayaran~vpuraian~vpcatatan~vpnoref~vptglnoref~vpcarabayar~vptglbayar~vpmatauang~vpkurs~vptotalap~vptotalapvalas~vptotalar~vptotalarvalas~vpbayar~vpbayarvalas~vpselisihkurs~vprekselisihkurs~vpdiskontermin~vpdiskonterminvalas~vprekdiskontermin~vpidvpp~vpstatus~vpstatussebelumnya~vpjmlrevisi~vpcetakanke~vpinputuser~vpinputtgl~vpmodifikasiuser~vpmodifikasitgl~vpisclose~vpcustomtext1~vpcustomtext2~vpcustomtext3~vpcustomtext4~vpcustomtext5~vpcustomint1~vpcustomint2~vpcustomint3~vpcustomdbl1~vpcustomdbl2~vpcustomdbl3~vpcustomdate1~vpcustomdate2~vpcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idvpdetail(0) As Integer, idvp(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, rekhutangpiutang(14) As String, 
        'catatan(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'idvppdetail(20) As Integer, urutan(21) As Integer, isclose(22) As Integer, customtext1(23) As String, customtext2(24) As String, 
        'customtext3(25) As String, customdbl1(26) As Double, customdbl2(27) As Double, customdbl3(28) As Double, customdate1(29) As Date, 
        'customdate2(30) As Date, customdate3(31) As Date, rencana(32) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, rencana


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idvpdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idvppdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'AMBIL MATA UANG FUNGSIONAL DARI SETTING ================
        Dim MUFungsional As String = ""
        Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
        If dtSetting.Rows.Count > 0 Then
            MUFungsional = dtSetting.Rows(0)(0)
        Else
            result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
        End If
        'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING =========


        'VARIABEL VALIDASI OUTSTANDING
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updNilaiValas As String = "", updFilter As String = "", sumberDetail As String = "", matauangDetail As String = "", norek As String = ""
        Dim idtransaksiDetail As Integer = 0, idvppdetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0
        Dim Outstanding As Double = 0, OutstandingValas As Double = 0

        'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT, 
        'RI
        Dim ftExistOutstandingRI As String = "", ftOutstandingRI As String = "", updNilaiRI As String = "", updFilterRI As String = "", updTglLunasRI As String = ""
        'AP
        Dim ftExistOutstandingAP As String = "", ftOutstandingAP As String = "", updNilaiAP As String = "", updNilaiValasAP As String = "", updFilterAP As String = "", updTglLunasAP As String = ""
        'PRT
        Dim ftExistOutstandingPRT As String = "", ftOutstandingPRT As String = "", updNilaiPRT As String = "", updFilterPRT As String = "", updTglLunasPRT As String = ""


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 33) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idvpdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idvpdetail required numeric." : GoTo selesai
            End If
            'idvp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idvp required numeric." : GoTo selesai
            End If
            'idtransaksi(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - idtransaksi required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'totaltransaksi(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - totaltransaksi required numeric." : GoTo selesai
            End If
            'terbayar(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - terbayar required numeric." : GoTo selesai
            End If
            'rencana(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - rencana required numeric." : GoTo selesai
            End If
            'sisa(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - sisa required numeric." : GoTo selesai
            End If
            'jmlbayar(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbayar required numeric." : GoTo selesai
            End If
            'jmlbayarvalas(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbayarvalas required numeric." : GoTo selesai
            End If
            'jmldiskontermin(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmldiskontermin required numeric." : GoTo selesai
            End If
            'jmldiskonterminvalas(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas required numeric." : GoTo selesai
            End If
            'idvppdetail(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - idvppdetail required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(29) As Date
            If (IsDate(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If
            If (dataRowDetail(2) <> "RI" And _
                dataRowDetail(2) <> "AP" And _
                dataRowDetail(2) <> "PRT" And _
                dataRowDetail(2) <> "CA") Then
                result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
            End If

            'matauang(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'totaltransaksi(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - totaltransaksi can't be empty" : GoTo selesai
            End If

            'terbayar(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - terbayar can't be empty" : GoTo selesai
            End If

            'rencana(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - rencana can't be empty" : GoTo selesai
            End If

            'sisa(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - sisa can't be empty" : GoTo selesai
            End If

            'jmlbayar(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayar can't be empty" : GoTo selesai
            End If

            'jmlbayarvalas(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayarvalas can't be empty" : GoTo selesai
            End If

            'diskontermin(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - diskontermin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - diskontermin should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskontermin(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskontermin can't be empty" : GoTo selesai
            End If

            'jmldiskonterminvalas(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas can't be empty" : GoTo selesai
            End If

            'rekhutangpiutang(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(26) As Double
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(29) As Date
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(30) As Date
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idvpdetail~idvp~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~idvppdetail~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'sumber(2) As String            , idtransaksi(3) As Integer            , jmlbayar(9) As Double
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3) : jmlbayar = dataRowDetail(9)
            'jmlbayarvalas(10) As Double      , rekhutangpiutang(14) As String, idvppdetail(20) As Integer
            jmlbayarvalas = dataRowDetail(10) : norek = dataRowDetail(14) : idvppdetail = dataRowDetail(20)
            'matauang(4) As String
            matauangDetail = dataRowDetail(4)


            'VALIDASI TRANSAKSI PEMBAYARAN ----------------
            Select Case sumberDetail
                Case "RI"
                    '1. CEK DATA EXIST
                    ftExistOutstandingRI = IIf(Len(ftExistOutstandingRI.ToString) = 0, "", ftExistOutstandingRI & " UNION ")
                    ftExistOutstandingRI = String.Concat(ftExistOutstandingRI, "SELECT EXISTS(SELECT 1 FROM m4_ri WHERE riid = '" & idtransaksiDetail & "' AND (ristatus = 2 OR ristatus = 3 OR ristatus = 4 OR ristatus = 7) LIMIT 1) as rowExists, riid, risumber, rinotransaksi FROM m4_ri WHERE riid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    If matauangDetail = MUFungsional Then
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    Else
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    End If
                    ftOutstandingRI = IIf(Len(ftOutstandingRI.ToString) = 0, "", ftOutstandingRI & " OR ")
                    'ftOutstandingRI = String.Concat(ftOutstandingRI, " (ri.riid = '" & idtransaksiDetail & "' AND " & Math.Round(Outstanding, 2) & " > ROUND(ri.ritotaltransaksi - ri.rijmlbayar,2)) ")
                    'ftOutstandingRI = String.Concat(ftOutstandingRI, " (ri.riid = '" & idtransaksiDetail & "' AND " & Outstanding & " > ROUND(ri.ritotaltransaksi - ri.rijmlbayar,5)) ")
					'ftOutstandingRI = String.Concat(ftOutstandingRI, " (ri.riid = '" & idtransaksiDetail & "' AND " & Math.Round(Outstanding, 2) & " > ROUND(ri.ritotaltransaksi - ri.rijmlbayar,2)) ")
					ftOutstandingRI = String.Concat(ftOutstandingRI, " (ri.riid = '" & idtransaksiDetail & "' AND " & Math.Round(Outstanding, 2) & " > CEIL(ri.ritotaltransaksi - ri.rijmlbayar)) ")
					
                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiRI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ri.rijmlbayar + '" & Outstanding & "', 5) ", updNilaiRI)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                    updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    updTglLunasRI = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ri.rijmlbayar + '" & Outstanding & "', 5) >= ri.ritotaltransaksi THEN '" & FixQuotes(tglLunas) & "' ELSE ri.ritgllunas END) ", updTglLunasRI)

                Case "AP"
                    '1. CEK DATA EXIST
                    ftExistOutstandingAP = IIf(Len(ftExistOutstandingAP.ToString) = 0, "", ftExistOutstandingAP & " UNION ")
                    ftExistOutstandingAP = String.Concat(ftExistOutstandingAP, "SELECT EXISTS(SELECT 1 FROM m4_ap WHERE apid = '" & idtransaksiDetail & "' AND (apstatus = 2 OR apstatus = 3 OR apstatus = 4 OR apstatus = 7) LIMIT 1) as rowExists, apid, apsumber, apnotransaksi FROM m4_ap WHERE apid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    ftOutstandingAP = IIf(Len(ftOutstandingAP.ToString) = 0, "", ftOutstandingAP & " OR ")
                    ftOutstandingAP = String.Concat(ftOutstandingAP, " (ap.apid = '" & idtransaksiDetail & "' AND (CASE ap.apmatauang WHEN s.snilai THEN " & Outstanding & " > ROUND(ap.apjumlah - ap.apjumlahbayar,2) ELSE " & OutstandingValas & " > ROUND(ap.apjumlahvalas - ap.apjumlahbayarvalas,2) END)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayar + '" & Math.Round(Outstanding, 2) & "', 5) ", updNilaiAP)
                    updNilaiValasAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayarvalas + '" & Math.Round(OutstandingValas, 2) & "', 5) ", updNilaiValasAP)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                    updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    If matauangDetail = MUFungsional Then
                        updTglLunasAP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ap.apjumlahbayar + '" & Outstanding & "', 5) >= ap.apjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ap.aptgllunas END) ", updTglLunasAP)
                    Else
                        updTglLunasAP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ap.apjumlahbayarvalas + '" & OutstandingValas & "', 5) >= ap.apjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ap.aptgllunas END) ", updTglLunasAP)
                    End If

                Case "PRT"
                    '1. CEK DATA EXIST
                    ftExistOutstandingPRT = IIf(Len(ftExistOutstandingPRT.ToString) = 0, "", ftExistOutstandingPRT & " UNION ")
                    ftExistOutstandingPRT = String.Concat(ftExistOutstandingPRT, "SELECT EXISTS(SELECT 1 FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "' AND (prtstatus = 2 OR prtstatus = 3 OR prtstatus = 4 OR prtstatus = 7) LIMIT 1) as rowExists, prtid, prtsumber, prtnotransaksi FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    If matauangDetail = MUFungsional Then
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    Else
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    End If
                    ftOutstandingPRT = IIf(Len(ftOutstandingPRT.ToString) = 0, "", ftOutstandingPRT & " OR ")
                    ftOutstandingPRT = String.Concat(ftOutstandingPRT, " (prt.prtid = '" & idtransaksiDetail & "' AND " & Math.Round(Outstanding, 2) & " > ROUND(prt.prttotaltransaksi - prt.prtjmlbayar,2)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiPRT = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(prt.prtjmlbayar + '" & Outstanding & "', 5) ", updNilaiPRT)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                    updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    updTglLunasPRT = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(prt.prtjmlbayar + '" & Outstanding & "', 5) >= prt.prttotaltransaksi THEN '" & FixQuotes(tglLunas) & "' ELSE prt.prttgllunas END) ", updTglLunasPRT)
            End Select
            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------


            'VALIDASI OUTSTANDING -------------------------
            If idvppdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                Select Case sumberDetail
                    Case "RI"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, rinotransaksi as notransaksi FROM m4_ri WHERE riid = '" & idtransaksiDetail & "'")
                    Case "AP"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, apnotransaksi as notransaksi FROM m4_ap WHERE apid = '" & idtransaksiDetail & "'")
                    Case "PRT"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, prtnotransaksi as notransaksi FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "'")
                    Case "CA"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, '" & norek & "' as notransaksi")
                    Case Else
                        result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
                End Select

                '2. CEK JML OUTSTANDING
                Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "idvppdetail=" & idvppdetail)
                OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "idvppdetail=" & idvppdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (vppd.idvppdetail = " & idvppdetail & " AND " & Math.Round(Outstanding, 2) & " > ROUND((vppd.jmlbayar - vppd.jmlvp),2)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvp + '" & Outstanding & "', 5) ", updNilai)
                updNilaiValas = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvpvalas + '" & OutstandingValas & "', 5) ", updNilaiValas)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idvppdetail = '" & idvppdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idvpcarabayar(0) As Integer, idvp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'idvppcarabayar(15) As Integer, isclose(16) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idvppcarabayar, isclose

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idvpcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "idvppcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "isclose", AsEnumTypeData.AsInt64)

        If (Len(dataSplit(2)) > 0) Then

            'VALIDASI DAN SET DATA PAY ======================================================
            'SPLIT PARAMETER DATA PAY
            dataPay = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA PAY ===============================================

            'VALIDASI DAN SET DATA ROW PAY ==================================================
            Dim JmlDtPay As Integer = dataPay.Length
            For i = 1 To JmlDtPay
                'SPLIT DATA PAY
                dataRowPay = dataPay(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA PAY -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowPay.Length <> 17) Then
                    result(2) = "Pay Row : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idvpcarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvpcarabayar required numeric." : GoTo selesai
                End If
                'idvp(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvp required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowPay(2)) = False) Then
                    result(2) = "Pay Row : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowPay(4)) = False) Then
                    result(2) = "Pay Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowPay(5)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowPay(6)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowPay(8)) = False) Then
                    result(2) = "Pay Row : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowPay(14)) = False) Then
                    result(2) = "Pay Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idvppcarabayar(15) As Integer
                If (IsNumeric(dataRowPay(15)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvppcarabayar required numeric." : GoTo selesai
                End If
                'isclose(16) As Integer
                If (IsNumeric(dataRowPay(16)) = False) Then
                    result(2) = "Pay Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA PAY -----------------------------------

                'VALIDASI DATA PAY ---------------------------------------
                'matauang(3) As String
                If Len(dataRowPay(3)) = 0 Then
                    result(2) = "Pay Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(3)) > 25 Then
                    result(2) = "Pay Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowPay(4)) = 0 Then
                    result(2) = "Pay Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowPay(5)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowPay(5) <= 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowPay(6)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowPay(8)) = 0 Then
                    result(2) = "Pay Row : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowPay(11)) = 0 Then
                    result(2) = "Pay Row : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(11)) > 25 Then
                    result(2) = "Pay Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Pay Row : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Pay Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Pay Row : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Pay Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Pay Row : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Pay Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Pay Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Pay Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idvpcarabayar~idvp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~idvppcarabayar~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15) & "~" & dataRowPay(16)) = False Then
                    result(2) = "Pay Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA PAY ===========================================

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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 15
                Select Case drutama("vpstatus")
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


                ''CEK TOTAL UTAMA DAN BAYAR ==============================
                'Dim jumlah As Double = AsDataTableDSum(dtpay, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtpay, "jumlahvalas")
                'If Double.Parse(drutama("vpbayar")) <> jumlah Then
                '    Dim selisih(2) As String
                '    selisih = F_Nominal(Double.Parse(drutama("vpbayar")) - jumlah, False).Split(sptSubParam)
                '    result(2) = "Total amount of pay is not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                '    'ElseIf drutama("vppbayarvalas") <> jumlahvalas Then
                '    '    result(2) = "Total amount of foreign pay is not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN BAYAR =======================


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("vptgl")), AsFormatTanggal(drutama("vptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "vpmatauang", "vprekselisihkurs~vprekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK MATAUANG COA =======================================
                'PAY
                rsCekCoa = ValidasiMatauangCOA(dtutama, "vpmatauang", "", dtpay, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("vpstatus") = 2 Or drutama("vpstatus") = 1 Or drutama("vpstatus") = 8 Or drutama("vpstatus") = 9 Or drutama("vpstatus") = 10 Or drutama("vpstatus") = 11 Then

                    'CEK JMLBAYAR TRANSAKSI ---------------------
                    Dim JmlRI As Double = 0, JmlCoa As Double = 0
                    Dim JmlAP As Double = 0, JmlPRT As Double = 0
                    Dim JmlTabBayar As Double = 0
                    Dim TotalAP As Double = 0, TotalAR As Double = 0

                    'TOTAL AP = RI + COA
                    JmlRI = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'RI'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'RI'")
                    JmlCoa = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'CA'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'CA'")
                    TotalAP = JmlRI + JmlCoa
					'SimpanLogWsToFile(4, "VP", JmlRI & " + " & JmlCoa)

                    'TOTAL AR = AP + PRT + BAYAR
                    JmlAP = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'AP'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'AP'")
                    JmlPRT = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'PRT'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'PR'")
                    JmlTabBayar = AsDataTableDSum(dtpay, "jumlah")
                    TotalAR = JmlAP + JmlPRT + JmlTabBayar + Double.Parse(drutama("vpselisihkurs"))
					'SimpanLogWsToFile(4, "VP", JmlAP & " + " & JmlPRT & " + " & JmlTabBayar & " + " & Double.Parse(drutama("vpselisihkurs")))

                    'JIKA SELISIH TOTAL AP DAN TOTAL AP >= 0.1 MAKA ALERT TIDAK BISA DISIMPAN
                    If Math.Abs(TotalAP - TotalAR) >= 0.1 Then
                        Dim selisih(2) As String
                        selisih = F_Nominal(F_Round(Math.Abs(TotalAP - TotalAR)), False).Split(sptSubParam)
                        result(2) = "Total AP and Total AR are not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK JMLBAYAR TRANSAKSI --------------

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, MUFungsional, ftExistOutstandingRI, ftOutstandingRI, ftExistOutstandingAP, ftOutstandingAP, ftExistOutstandingPRT, ftOutstandingPRT, updFilterRI, updFilterAP, updFilterPRT, formatTgl, drutama("vptgl"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("vpid")
                    notransaksi = drutama("vpnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(vpid), vpnotransaksi FROM M4_vp WHERE vpid='" & result(4) & "' AND vpstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("vpautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("vpcabang"), drutama("vplokasi"), drutama("vpsumber"), drutama("vptgl"), drutama("vpsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(vpid) FROM M4_vp WHERE vpnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_vp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Vp_HistorySimpan("" & paramSplit(0) & "★M4_Vp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("vpsumber")) & "▼" & FixQuotes(drutama("vpid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Vp set vpcabang  = '" & FixQuotes(drutama("vpcabang")) & "', vplokasi  = '" & FixQuotes(drutama("vplokasi")) & "', vpgudang  = '" & FixQuotes(drutama("vpgudang")) & "', vpsumber  = '" & FixQuotes(drutama("vpsumber")) & "', vpautonotransaksi  = " & drutama("vpautonotransaksi") & ", vpnotransaksi  = '" & FixQuotes(notransaksi) & "', vptgl  = '" & FixQuotes(AsFormatTanggal(drutama("vptgl"))) & "', vpkodepa  = " & drutama("vpkodepa") & ", vpsupplier  = " & drutama("vpsupplier") & ", vpsupplierkontak  = '" & FixQuotes(drutama("vpsupplierkontak")) & "', vp1alamat1  = '" & FixQuotes(drutama("vp1alamat1")) & "', vp1alamat2  = '" & FixQuotes(drutama("vp1alamat2")) & "', vp1alamat3  = '" & FixQuotes(drutama("vp1alamat3")) & "', vp2alamat1  = '" & FixQuotes(drutama("vp2alamat1")) & "', vp2alamat2  = '" & FixQuotes(drutama("vp2alamat2")) & "', vp2alamat3  = '" & FixQuotes(drutama("vp2alamat3")) & "', vpbagianpembayaran  = " & drutama("vpbagianpembayaran") & ", vpuraian  = '" & FixQuotes(drutama("vpuraian")) & "', vpcatatan  = '" & FixQuotes(drutama("vpcatatan")) & "', vpnoref  = '" & FixQuotes(drutama("vpnoref")) & "', vptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("vptglnoref"))) & "', vpcarabayar  = " & drutama("vpcarabayar") & ", vptglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("vptglbayar"))) & "', vpmatauang  = '" & FixQuotes(drutama("vpmatauang")) & "', vpkurs  = '" & FixDouble(drutama("vpkurs")) & "', vptotalap  = '" & FixDouble(drutama("vptotalap")) & "', vptotalapvalas  = '" & FixDouble(drutama("vptotalapvalas")) & "', vptotalar  = '" & FixDouble(drutama("vptotalar")) & "', vptotalarvalas  = '" & FixDouble(drutama("vptotalarvalas")) & "', vpbayar  = '" & FixDouble(drutama("vpbayar")) & "', vpbayarvalas  = '" & FixDouble(drutama("vpbayarvalas")) & "', vpselisihkurs  = '" & FixDouble(drutama("vpselisihkurs")) & "', vprekselisihkurs  = '" & FixQuotes(drutama("vprekselisihkurs")) & "', vpdiskontermin  = '" & FixDouble(drutama("vpdiskontermin")) & "', vpdiskonterminvalas  = '" & FixDouble(drutama("vpdiskonterminvalas")) & "', vprekdiskontermin  = '" & FixQuotes(drutama("vprekdiskontermin")) & "', vpidvpp  = " & drutama("vpidvpp") & ", vpstatus  = " & drutama("vpstatus") & ", vpstatussebelumnya  = " & drutama("vpstatussebelumnya") & ", vpjmlrevisi  = vpjmlrevisi+1, vpcetakanke  = " & drutama("vpcetakanke") & ", vpmodifikasiuser  = " & drutama("vpmodifikasiuser") & ", vpmodifikasitgl  = NOW(), vpcustomtext1  = '" & FixQuotes(drutama("vpcustomtext1")) & "', vpcustomtext2  = '" & FixQuotes(drutama("vpcustomtext2")) & "', vpcustomtext3  = '" & FixQuotes(drutama("vpcustomtext3")) & "', vpcustomtext4  = '" & FixQuotes(drutama("vpcustomtext4")) & "', vpcustomtext5  = '" & FixQuotes(drutama("vpcustomtext5")) & "', vpcustomint1  = " & drutama("vpcustomint1") & ", vpcustomint2  = " & drutama("vpcustomint2") & ", vpcustomint3  = " & drutama("vpcustomint3") & ", vpcustomdbl1  = '" & FixDouble(drutama("vpcustomdbl1")) & "', vpcustomdbl2  = '" & FixDouble(drutama("vpcustomdbl2")) & "', vpcustomdbl3  = '" & FixDouble(drutama("vpcustomdbl3")) & "', vpcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate1"))) & "', vpcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate2"))) & "', vpcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate3"))) & "' where vpid = '" & drutama("vpid") & "'"
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

                    If drutama("vpautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("vpcabang"), drutama("vplokasi"), drutama("vpsumber"), drutama("vptgl"), drutama("vpsumber"), 4)
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
                        notransaksi = drutama("vpnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(vpid) FROM m4_vp WHERE vpnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Vp (vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpisclose, vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3) values('" & FixQuotes(drutama("vpcabang")) & "', '" & FixQuotes(drutama("vplokasi")) & "', '" & FixQuotes(drutama("vpgudang")) & "', '" & FixQuotes(drutama("vpsumber")) & "', " & drutama("vpautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("vptgl"))) & "', " & drutama("vpkodepa") & ", " & drutama("vpsupplier") & ", '" & FixQuotes(drutama("vpsupplierkontak")) & "', '" & FixQuotes(drutama("vp1alamat1")) & "', '" & FixQuotes(drutama("vp1alamat2")) & "', '" & FixQuotes(drutama("vp1alamat3")) & "', '" & FixQuotes(drutama("vp2alamat1")) & "', '" & FixQuotes(drutama("vp2alamat2")) & "', '" & FixQuotes(drutama("vp2alamat3")) & "', " & drutama("vpbagianpembayaran") & ", '" & FixQuotes(drutama("vpuraian")) & "', '" & FixQuotes(drutama("vpcatatan")) & "', '" & FixQuotes(drutama("vpnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vptglnoref"))) & "', " & drutama("vpcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("vptglbayar"))) & "', '" & FixQuotes(drutama("vpmatauang")) & "', '" & FixDouble(drutama("vpkurs")) & "', '" & FixDouble(drutama("vptotalap")) & "', '" & FixDouble(drutama("vptotalapvalas")) & "', '" & FixDouble(drutama("vptotalar")) & "', '" & FixDouble(drutama("vptotalarvalas")) & "', '" & FixDouble(drutama("vpbayar")) & "', '" & FixDouble(drutama("vpbayarvalas")) & "', '" & FixDouble(drutama("vpselisihkurs")) & "', '" & FixQuotes(drutama("vprekselisihkurs")) & "', '" & FixDouble(drutama("vpdiskontermin")) & "', '" & FixDouble(drutama("vpdiskonterminvalas")) & "', '" & FixQuotes(drutama("vprekdiskontermin")) & "', " & drutama("vpidvpp") & ", " & drutama("vpstatus") & ", " & drutama("vpstatussebelumnya") & ", " & drutama("vpjmlrevisi") & ", " & drutama("vpcetakanke") & ", " & drutama("vpinputuser") & ", NOW(), " & drutama("vpmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("vpisclose") & ", '" & FixQuotes(drutama("vpcustomtext1")) & "', '" & FixQuotes(drutama("vpcustomtext2")) & "', '" & FixQuotes(drutama("vpcustomtext3")) & "', '" & FixQuotes(drutama("vpcustomtext4")) & "', '" & FixQuotes(drutama("vpcustomtext5")) & "', " & drutama("vpcustomint1") & ", " & drutama("vpcustomint2") & ", " & drutama("vpcustomint3") & ", '" & FixDouble(drutama("vpcustomdbl1")) & "', '" & FixDouble(drutama("vpcustomdbl2")) & "', '" & FixDouble(drutama("vpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select vpid from M4_vp where vpnotransaksi='" & notransaksi & "' AND vpinputuser= '" & userid & "' order by vpmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vp_Detail where idvp = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idvpdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("idvppdetail") & ", " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Vp_Detail(idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vp_Pay where idvp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses pay
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    Dim rsCekGiro As String

                    For Each dr1 As DataRow In dtpay.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idvpcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idvppcarabayar") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("vpstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(4, 15, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("vpsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("vpsupplier") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M4_Vp_Pay(idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idvppcarabayar, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("vpstatus") = 2 And Len(strGiro.ToString) > 0 Then
                        sql = "Insert into M2_Giro_List(glnogiro, glsumber, glidtransaksi, glnotransaksi, glkontak, glrekbank, glrekgiro, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, gltglcair, glstatus, glstatussebelumnya, glurutan) values" & strGiro.ToString & ""
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

                If drutama("vpstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ==================================================
                    If Len(updNilai) > 0 Then
                        'UPDATE DETAIL
                        sql = "UPDATE M4_vpp_detail SET jmlvp = (CASE idvppdetail " & updNilai & " ELSE jmlvp END), jmlvpvalas = (CASE idvppdetail " & updNilaiValas & " ELSE jmlvpvalas END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idvpp FROM M4_vpp_detail WHERE " & updFilter & " GROUP BY idvpp", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idvpp = '" & dr1("idvpp") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idvpp, SUM(jmlbayar) as jmlbayar, SUM(jmlvp) as jmlvp FROM M4_vpp_detail WHERE " & ftDetail & " GROUP BY idvpp", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlvp") >= dr1("jmlbayar") Then
                                    statusOut = 2
                                ElseIf dr1("jmlvp") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idvpp") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(vppid = '" & dr1("idvpp") & "')")
                            Next

                            sql = "UPDATE M4_vpp SET vppstatusvp = (CASE vppid " & updNilai & " ELSE vppstatusvp END) WHERE " & updFilter
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
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                    'RI
                    If Len(updNilaiRI) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri SET ri.rijmlbayar = (CASE ri.riid " & updNilaiRI & " ELSE ri.rijmlbayar END), ri.ritgllunas = (CASE ri.riid " & updTglLunasRI & " ELSE ri.ritgllunas END) WHERE " & updFilterRI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ri ri JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid =  t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE " & updFilterRI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'AP
                    If Len(updNilaiAP) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ap ap SET ap.apjumlahbayar = (CASE ap.apid " & updNilaiAP & " ELSE ap.apjumlahbayar END), ap.apjumlahbayarvalas = (CASE ap.apid " & updNilaiValasAP & " ELSE ap.apjumlahbayarvalas END), ap.aptgllunas = (CASE ap.apid " & updTglLunasAP & " ELSE ap.aptgllunas END) WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ap ap JOIN m2_transaction_journal t ON ap.apsumber = t.tsumber AND ap.apid =  t.tidtransaksi AND ap.apnotransaksi = t.tnotransaksi SET t.tstatuslunas = ap.apstatusbayar, t.ttgllunas = ap.aptgllunas WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'PRT
                    If Len(updNilaiPRT) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_prt prt SET prt.prtjmlbayar = (CASE prt.prtid " & updNilaiPRT & " ELSE prt.prtjmlbayar END), prt.prttgllunas = (CASE prt.prtid " & updTglLunasPRT & " ELSE prt.prttgllunas END) WHERE " & updFilterPRT
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_prt prt JOIN m2_transaction_journal t ON prt.prtsumber = t.tsumber AND prt.prtid =  t.tidtransaksi AND prt.prtnotransaksi = t.tnotransaksi SET t.tstatuslunas = prt.prtstatuslunas, t.ttgllunas = prt.prttgllunas WHERE " & updFilterPRT
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================

                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "VP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("vpstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
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

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG ====================================================================
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
    Public Function M4_VpUpdateStatus(ByVal param As String) As String

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
        Dim dtdetail As DataTable
        Dim isDelete As Boolean = False

        Dim pg1 As New RsPaging
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
            Filter = Filter.Replace("vpsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vpsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Vptgl, Vpnotransaksi, Vpstatus FROM M4_Vp WHERE Vpid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Vpstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_vp_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Vp_HistorySimpan("" & paramSplit(0) & "★M4_Vp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m4_vp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'VP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'Variabel ValidasiSimpan
                Dim ftOutstanding As String = "", updNilai As String = "", updNilaiValas As String = "", updFilter As String = "", sumberDetail As String = "", norek As String = ""
                Dim idtransaksiDetail As Integer = 0, idvppdetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0, matauangDetail As String = ""
                Dim Outstanding As Double = 0, OutstandingValas As Double = 0, tglLunas = "1900-01-01"

                'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT, CA
                'RI
                Dim updNilaiRI As String = "", updFilterRI As String = ""
                'AP
                Dim updNilaiAP As String = "", updNilaiValasAP As String = "", updFilterAP As String = ""
                'PRT
                Dim updNilaiPRT As String = "", updFilterPRT As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT sumber, idtransaksi, matauang, jmlbayar, jmlbayarvalas, rekhutangpiutang, idvppdetail, urutan FROM M4_vp_detail WHERE idvp = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    Dim MUFungsional As String = ""

                    'AMBIL MATA UANG FUNGSIONAL DARI SETTING
                    Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                    If dtSetting.Rows.Count > 0 Then
                        MUFungsional = dtSetting.Rows(0)(0)
                    Else
                        result(2) = "Can't found 'Functional Currency' in Setting." : Trans.Rollback() : GoTo selesai
                    End If

                    For Each dr1 As DataRow In dtdetail.Rows
                        sumberDetail = dr1("sumber") : idtransaksiDetail = dr1("idtransaksi") : jmlbayar = dr1("jmlbayar")
                        jmlbayarvalas = dr1("jmlbayarvalas") : norek = dr1("rekhutangpiutang") : idvppdetail = dr1("idvppdetail")
                        matauangDetail = dr1("matauang")

                        If idvppdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "idvppdetail=" & idvppdetail)
                            OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "idvppdetail=" & idvppdetail)
                            updNilai = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvp - '" & Outstanding & "', 5) ", updNilai)
                            updNilaiValas = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvpvalas - '" & OutstandingValas & "', 5) ", updNilaiValas)

                            '2. SET FILTER UPDATE OUTSTANDING ---------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idvppdetail = '" & idvppdetail & "')")
                        End If

                        'VALIDASI TRANSAKSI PEMBAYARAN ----------------
                        Select Case sumberDetail
                            Case "RI"
                                '1. CEK JML OUTSTANDING
                                If matauangDetail = MUFungsional Then
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                Else
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiRI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ri.rijmlbayar - '" & Outstanding & "', 5) ", updNilaiRI)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                            Case "AP"
                                '1. CEK JML OUTSTANDING
                                Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayar - '" & Outstanding & "', 5) ", updNilaiAP)
                                updNilaiValasAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayarvalas - '" & OutstandingValas & "', 5) ", updNilaiValasAP)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                                updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                            Case "PRT"
                                '1. CEK JML OUTSTANDING
                                If matauangDetail = MUFungsional Then
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                Else
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPRT = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(prt.prtjmlbayar - '" & Outstanding & "', 5) ", updNilaiPRT)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                                updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

                        End Select
                        'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI =======================================================
                If Len(updNilai) > 0 Then
                    'UPDATE DETAIL
                    sql = "UPDATE M4_vpp_detail SET jmlvp = (CASE idvppdetail " & updNilai & " ELSE jmlvp END), jmlvpvalas = (CASE idvppdetail " & updNilaiValas & " ELSE jmlvpvalas END) WHERE " & updFilter
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE UTAMA
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idvpp FROM M4_vpp_detail WHERE " & updFilter & " GROUP BY idvpp", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idvpp = '" & dr1("idvpp") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idvpp, SUM(jmlbayar) as jmlbayar, SUM(jmlvp) as jmlvp FROM M4_vpp_detail WHERE " & ftDetail & " GROUP BY idvpp", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlvp") >= dr1("jmlbayar") Then
                                statusOut = 2
                            ElseIf dr1("jmlvp") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idvpp") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(vppid = '" & dr1("idvpp") & "')")
                        Next

                        sql = "UPDATE M4_vpp SET vppstatusvp = (CASE vppid " & updNilai & " ELSE vppstatusvp END) WHERE " & updFilter
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
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================


                'UPDATE TRANSAKSI PEMBAYARAN ========================================================
                'RI
                If Len(updNilaiRI) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ri ri SET ri.rijmlbayar = (CASE ri.riid " & updNilaiRI & " ELSE ri.rijmlbayar END), ri.ritgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterRI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_ri ri JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE " & updFilterRI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'AP
                If Len(updNilaiAP) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ap ap SET ap.apjumlahbayar = (CASE ap.apid " & updNilaiAP & " ELSE ap.apjumlahbayar END), ap.apjumlahbayarvalas = (CASE ap.apid " & updNilaiValasAP & " ELSE ap.apjumlahbayarvalas END), ap.aptgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterAP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_ap ap JOIN m2_transaction_journal t ON ap.apsumber = t.tsumber AND ap.apid = t.tidtransaksi AND ap.apnotransaksi = t.tnotransaksi SET t.tstatuslunas = ap.apstatusbayar, t.ttgllunas = ap.aptgllunas WHERE " & updFilterAP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'PRT
                If Len(updNilaiPRT) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_prt prt SET prt.prtjmlbayar = (CASE prt.prtid " & updNilaiPRT & " ELSE prt.prtjmlbayar END), prt.prttgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterPRT
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_prt prt JOIN m2_transaction_journal t ON prt.prtsumber = t.tsumber AND prt.prtid = t.tidtransaksi AND prt.prtnotransaksi = t.tnotransaksi SET t.tstatuslunas = prt.prtstatuslunas, t.ttgllunas = prt.prttgllunas WHERE " & updFilterPRT
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'UPDATE TRANSAKSI PEMBAYARAN ========================================================


                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'VP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'VP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M4_Vp SET Vpstatus = " & nilaiStatus & ", Vpmodifikasiuser='" & userid & "', Vpmodifikasitgl = NOW(), Vpposting = 0, Vppostingtgl = '1971-01-01 00:00:00', Vpjmlrevisi = Vpjmlrevisi + 1 WHERE Vpid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_VpSearch(PostWsSearch(paramSplit(0), "M4_VpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_VpDelete(ByVal param As String) As String

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
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("vpsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vpsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Vpid, Vpnotransaksi FROM M4_Vp WHERE Vpid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT vpcabang, vplokasi, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl"
            sql &= " FROM M4_vp"
            sql &= " WHERE vpid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("vpcabang")
                lokasi = dtNomorNext.Rows(0)("vplokasi")
                sumber = dtNomorNext.Rows(0)("vpsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("vpautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("vpnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("vptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PAY
            sql = "DELETE FROM M4_Vp_Pay WHERE idvp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Vp_Detail WHERE idvp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Vp WHERE vpid='" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 4)
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
            Dim paramSearch As String = M4_VpSearch(PostWsSearch(paramSplit(0), "M4_VpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_VpGetdataByIdSerenity(ByVal param As String) As String

        'M4_VpGetdataById Utama --------------------------------------------------------
        'vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, 
        'vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, 
        'vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3, 
        'vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, 
        'vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vpnotransaksivpp, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, 
        'vpmodifikasiusernama, kpkp

        'M4_VpGetdataById Detail -------------------------------------------------------
        'idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, 
        'sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, 
        'catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl

        'M4_VpGetdataById Pay ----------------------------------------------------------
        'idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idvppcarabayar, isclose, carabayarnama, banknama, rekbanknama, rekgironama

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

        Dim utama As String = "", detail As String = "", detailRI As String = "", detailPRT As String = "", detailAP As String = "", detailCOA As String = "", pay As String = "", idtransaksi As String = ""

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
            Filter = "vpid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "vpid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vp_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("vpid"), 0), sptField,
                     FxDB(drutama("vpcabang"), ""), sptField,
                     FxDB(drutama("vplokasi"), ""), sptField,
                     FxDB(drutama("vpgudang"), ""), sptField,
                     FxDB(drutama("vpsumber"), ""), sptField,
                     FxDB(drutama("vpautonotransaksi"), 0), sptField,
                     FxDB(drutama("vpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("vpkodepa"), 0), sptField,
                     FxDB(drutama("vpsupplier"), 0), sptField,
                     FxDB(drutama("vpsupplierkontak"), ""), sptField,
                     FxDB(drutama("vp1alamat1"), ""), sptField,
                     FxDB(drutama("vp1alamat2"), ""), sptField,
                     FxDB(drutama("vp1alamat3"), ""), sptField,
                     FxDB(drutama("vp2alamat1"), ""), sptField,
                     FxDB(drutama("vp2alamat2"), ""), sptField,
                     FxDB(drutama("vp2alamat3"), ""), sptField,
                     FxDB(drutama("vpbagianpembayaran"), 0), sptField,
                     FxDB(drutama("vpuraian"), ""), sptField,
                     FxDB(drutama("vpcatatan"), ""), sptField,
                     FxDB(drutama("vpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("vpcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vptglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("vpmatauang"), ""), sptField,
                     FxDB(drutama("vpkurs"), 0), sptField,
                     FxDB(drutama("vptotalap"), 0), sptField,
                     FxDB(drutama("vptotalapvalas"), 0), sptField,
                     FxDB(drutama("vptotalar"), 0), sptField,
                     FxDB(drutama("vptotalarvalas"), 0), sptField,
                     FxDB(drutama("vpbayar"), 0), sptField,
                     FxDB(drutama("vpbayarvalas"), 0), sptField,
                     FxDB(drutama("vpselisihkurs"), 0), sptField,
                     FxDB(drutama("vprekselisihkurs"), ""), sptField,
                     FxDB(drutama("vpdiskontermin"), 0), sptField,
                     FxDB(drutama("vpdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("vprekdiskontermin"), ""), sptField,
                     FxDB(drutama("vpidvpp"), 0), sptField,
                     FxDB(drutama("vpstatus"), 0), sptField,
                     FxDB(drutama("vpstatussebelumnya"), 0), sptField,
                     FxDB(drutama("vpjmlrevisi"), 0), sptField,
                     FxDB(drutama("vpcetakanke"), 0), sptField,
                     FxDB(drutama("vpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpisclose"), 0), sptField,
                     FxDB(drutama("vpcustomtext1"), ""), sptField,
                     FxDB(drutama("vpcustomtext2"), ""), sptField,
                     FxDB(drutama("vpcustomtext3"), ""), sptField,
                     FxDB(drutama("vpcustomtext4"), ""), sptField,
                     FxDB(drutama("vpcustomtext5"), ""), sptField,
                     FxDB(drutama("vpcustomint1"), 0), sptField,
                     FxDB(drutama("vpcustomint2"), 0), sptField,
                     FxDB(drutama("vpcustomint3"), 0), sptField,
                     FxDB(drutama("vpcustomdbl1"), 0), sptField,
                     FxDB(drutama("vpcustomdbl2"), 0), sptField,
                     FxDB(drutama("vpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("vpcabangnama"), ""), sptField,
                     FxDB(drutama("vplokasinama"), ""), sptField,
                     FxDB(drutama("vpgudangnama"), ""), sptField,
                     FxDB(drutama("vpsupplierkode"), ""), sptField,
                     FxDB(drutama("vpsuppliernama"), ""), sptField,
                     FxDB(drutama("vpbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("vpbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("vpcarabayarnama"), ""), sptField,
                     FxDB(drutama("vprekselisihkursnama"), ""), sptField,
                     FxDB(drutama("vprekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("vpnotransaksivpp"), ""), sptField,
                     FxDB(drutama("vpstatusnama"), ""), sptField,
                     FxDB(drutama("vpstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("vpinputusernama"), ""), sptField,
                     FxDB(drutama("vpmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                Dim sumberdetail As String = FxDB(dr("sumber"), "")

                Select Case sumberdetail
                    Case "RI"
                        detailRI = String.Concat(detailRI, FxDB(dr("idvpdetail"), 0), sptField,
                             FxDB(dr("idvp"), 0), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idtransaksi"), 0), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("totaltransaksi"), 0), sptField,
                             FxDB(dr("terbayar"), 0), sptField,
                             FxDB(dr("sisa"), 0), sptField,
                             FxDB(dr("jmlbayar"), 0), sptField,
                             FxDB(dr("jmlbayarvalas"), 0), sptField,
                             FxDB(dr("diskontermin"), ""), sptField,
                             FxDB(dr("jmldiskontermin"), 0), sptField,
                             FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                             FxDB(dr("rekhutangpiutang"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("idvppdetail"), 0), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("notransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                             FxDB(dr("carabayar"), 0), sptField,
                             FxDB(dr("termin"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                             FxDB(dr("rencana"), 0), sptField,
                             FxDB(dr("statuslunas"), 0), sptField,
                             FxDB(dr("diskon1"), 0), sptField,
                             FxDB(dr("haridiskon1"), 0), sptField,
                             FxDB(dr("diskon2"), 0), sptField,
                             FxDB(dr("haridiskon2"), 0), sptField,
                             FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                             FxDB(dr("costcenternama"), ""), sptField,
                             FxDB(dr("divisinama"), ""), sptField,
                             FxDB(dr("subdivisinama"), ""), sptField,
                             FxDB(dr("proyeknama"), ""), sptField,
                             FxDB(dr("vppnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                    Case "PRT"
                        detailPRT = String.Concat(detailPRT, FxDB(dr("idvpdetail"), 0), sptField,
                             FxDB(dr("idvp"), 0), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idtransaksi"), 0), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("totaltransaksi"), 0), sptField,
                             FxDB(dr("terbayar"), 0), sptField,
                             FxDB(dr("sisa"), 0), sptField,
                             FxDB(dr("jmlbayar"), 0), sptField,
                             FxDB(dr("jmlbayarvalas"), 0), sptField,
                             FxDB(dr("diskontermin"), ""), sptField,
                             FxDB(dr("jmldiskontermin"), 0), sptField,
                             FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                             FxDB(dr("rekhutangpiutang"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("idvppdetail"), 0), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("notransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                             FxDB(dr("carabayar"), 0), sptField,
                             FxDB(dr("termin"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                             FxDB(dr("rencana"), 0), sptField,
                             FxDB(dr("statuslunas"), 0), sptField,
                             FxDB(dr("diskon1"), 0), sptField,
                             FxDB(dr("haridiskon1"), 0), sptField,
                             FxDB(dr("diskon2"), 0), sptField,
                             FxDB(dr("haridiskon2"), 0), sptField,
                             FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                             FxDB(dr("costcenternama"), ""), sptField,
                             FxDB(dr("divisinama"), ""), sptField,
                             FxDB(dr("subdivisinama"), ""), sptField,
                             FxDB(dr("proyeknama"), ""), sptField,
                             FxDB(dr("vppnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                    Case "AP"
                        detailAP = String.Concat(detailAP, FxDB(dr("idvpdetail"), 0), sptField,
                             FxDB(dr("idvp"), 0), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idtransaksi"), 0), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("totaltransaksi"), 0), sptField,
                             FxDB(dr("terbayar"), 0), sptField,
                             FxDB(dr("sisa"), 0), sptField,
                             FxDB(dr("jmlbayar"), 0), sptField,
                             FxDB(dr("jmlbayarvalas"), 0), sptField,
                             FxDB(dr("diskontermin"), ""), sptField,
                             FxDB(dr("jmldiskontermin"), 0), sptField,
                             FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                             FxDB(dr("rekhutangpiutang"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("idvppdetail"), 0), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("notransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                             FxDB(dr("carabayar"), 0), sptField,
                             FxDB(dr("termin"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                             FxDB(dr("rencana"), 0), sptField,
                             FxDB(dr("statuslunas"), 0), sptField,
                             FxDB(dr("diskon1"), 0), sptField,
                             FxDB(dr("haridiskon1"), 0), sptField,
                             FxDB(dr("diskon2"), 0), sptField,
                             FxDB(dr("haridiskon2"), 0), sptField,
                             FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                             FxDB(dr("costcenternama"), ""), sptField,
                             FxDB(dr("divisinama"), ""), sptField,
                             FxDB(dr("subdivisinama"), ""), sptField,
                             FxDB(dr("proyeknama"), ""), sptField,
                             FxDB(dr("vppnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                    Case "CA"
                        detailCOA = String.Concat(detailCOA, FxDB(dr("idvpdetail"), 0), sptField,
                             FxDB(dr("idvp"), 0), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idtransaksi"), 0), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("totaltransaksi"), 0), sptField,
                             FxDB(dr("terbayar"), 0), sptField,
                             FxDB(dr("sisa"), 0), sptField,
                             FxDB(dr("jmlbayar"), 0), sptField,
                             FxDB(dr("jmlbayarvalas"), 0), sptField,
                             FxDB(dr("diskontermin"), ""), sptField,
                             FxDB(dr("jmldiskontermin"), 0), sptField,
                             FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                             FxDB(dr("rekhutangpiutang"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("idvppdetail"), 0), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("notransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                             FxDB(dr("carabayar"), 0), sptField,
                             FxDB(dr("termin"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                             FxDB(dr("rencana"), 0), sptField,
                             FxDB(dr("statuslunas"), 0), sptField,
                             FxDB(dr("diskon1"), 0), sptField,
                             FxDB(dr("haridiskon1"), 0), sptField,
                             FxDB(dr("diskon2"), 0), sptField,
                             FxDB(dr("haridiskon2"), 0), sptField,
                             FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                             FxDB(dr("costcenternama"), ""), sptField,
                             FxDB(dr("divisinama"), ""), sptField,
                             FxDB(dr("subdivisinama"), ""), sptField,
                             FxDB(dr("proyeknama"), ""), sptField,
                             FxDB(dr("vppnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                End Select
            Next
            If detailRI.Length > 0 Then detailRI = detailRI.Substring(0, detailRI.Length - sptRow.Length) Else detailRI = detailRI
            If detailPRT.Length > 0 Then detailPRT = detailPRT.Substring(0, detailPRT.Length - sptRow.Length) Else detailPRT = detailPRT
            If detailAP.Length > 0 Then detailAP = detailAP.Substring(0, detailAP.Length - sptRow.Length) Else detailAP = detailAP
            If detailCOA.Length > 0 Then detailCOA = detailCOA.Substring(0, detailCOA.Length - sptRow.Length) Else detailCOA = detailCOA

            'PANGGIL QUERY
            sql = query.PanggilQuery("m4_vp_getdata_pay")

            'AMBIL DATA PAY
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M4_Vp_Pay", "idvp=" & idtransaksi, "idvp ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idvpcarabayar"), 0), sptField,
                     FxDB(dr("idvp"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idvppcarabayar"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "VP transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detailRI, sptSubParam, detailPRT, sptSubParam, pay, sptSubParam, detailAP, sptSubParam, detailCOA)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3, vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vpnotransaksivpp, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, vpmodifikasiusernama, kpkp" &
                                                                    sptSubParam & "idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl" &
                                                                    sptSubParam & "idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl" &
                                                                    sptSubParam & "idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idvppcarabayar, isclose, carabayarnama, banknama, rekbanknama, rekgironama" &
                                                                    sptSubParam & "idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl" &
                                                                    sptSubParam & "idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl"))

        Return wsResult
    End Function
	
    <WebMethod()>
    Public Function M4_VpGetdataById(ByVal param As String) As String

        'M4_VpGetdataById Utama --------------------------------------------------------
        'vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, 
        'vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, 
        'vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3, 
        'vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, 
        'vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vpnotransaksivpp, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, 
        'vpmodifikasiusernama, kpkp

        'M4_VpGetdataById Detail -------------------------------------------------------
        'idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, 
        'sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, 
        'catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl

        'M4_VpGetdataById Pay ----------------------------------------------------------
        'idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idvppcarabayar, isclose, carabayarnama, banknama, rekbanknama, rekgironama

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

        Dim utama As String = "", detail As String = "", pay As String = "", idtransaksi As String = ""

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
            Filter = "vpid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "vpid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vp_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("vpid"), 0), sptField,
                     FxDB(drutama("vpcabang"), ""), sptField,
                     FxDB(drutama("vplokasi"), ""), sptField,
                     FxDB(drutama("vpgudang"), ""), sptField,
                     FxDB(drutama("vpsumber"), ""), sptField,
                     FxDB(drutama("vpautonotransaksi"), 0), sptField,
                     FxDB(drutama("vpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("vpkodepa"), 0), sptField,
                     FxDB(drutama("vpsupplier"), 0), sptField,
                     FxDB(drutama("vpsupplierkontak"), ""), sptField,
                     FxDB(drutama("vp1alamat1"), ""), sptField,
                     FxDB(drutama("vp1alamat2"), ""), sptField,
                     FxDB(drutama("vp1alamat3"), ""), sptField,
                     FxDB(drutama("vp2alamat1"), ""), sptField,
                     FxDB(drutama("vp2alamat2"), ""), sptField,
                     FxDB(drutama("vp2alamat3"), ""), sptField,
                     FxDB(drutama("vpbagianpembayaran"), 0), sptField,
                     FxDB(drutama("vpuraian"), ""), sptField,
                     FxDB(drutama("vpcatatan"), ""), sptField,
                     FxDB(drutama("vpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("vpcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vptglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("vpmatauang"), ""), sptField,
                     FxDB(drutama("vpkurs"), 0), sptField,
                     FxDB(drutama("vptotalap"), 0), sptField,
                     FxDB(drutama("vptotalapvalas"), 0), sptField,
                     FxDB(drutama("vptotalar"), 0), sptField,
                     FxDB(drutama("vptotalarvalas"), 0), sptField,
                     FxDB(drutama("vpbayar"), 0), sptField,
                     FxDB(drutama("vpbayarvalas"), 0), sptField,
                     FxDB(drutama("vpselisihkurs"), 0), sptField,
                     FxDB(drutama("vprekselisihkurs"), ""), sptField,
                     FxDB(drutama("vpdiskontermin"), 0), sptField,
                     FxDB(drutama("vpdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("vprekdiskontermin"), ""), sptField,
                     FxDB(drutama("vpidvpp"), 0), sptField,
                     FxDB(drutama("vpstatus"), 0), sptField,
                     FxDB(drutama("vpstatussebelumnya"), 0), sptField,
                     FxDB(drutama("vpjmlrevisi"), 0), sptField,
                     FxDB(drutama("vpcetakanke"), 0), sptField,
                     FxDB(drutama("vpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vpisclose"), 0), sptField,
                     FxDB(drutama("vpcustomtext1"), ""), sptField,
                     FxDB(drutama("vpcustomtext2"), ""), sptField,
                     FxDB(drutama("vpcustomtext3"), ""), sptField,
                     FxDB(drutama("vpcustomtext4"), ""), sptField,
                     FxDB(drutama("vpcustomtext5"), ""), sptField,
                     FxDB(drutama("vpcustomint1"), 0), sptField,
                     FxDB(drutama("vpcustomint2"), 0), sptField,
                     FxDB(drutama("vpcustomint3"), 0), sptField,
                     FxDB(drutama("vpcustomdbl1"), 0), sptField,
                     FxDB(drutama("vpcustomdbl2"), 0), sptField,
                     FxDB(drutama("vpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("vpcabangnama"), ""), sptField,
                     FxDB(drutama("vplokasinama"), ""), sptField,
                     FxDB(drutama("vpgudangnama"), ""), sptField,
                     FxDB(drutama("vpsupplierkode"), ""), sptField,
                     FxDB(drutama("vpsuppliernama"), ""), sptField,
                     FxDB(drutama("vpbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("vpbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("vpcarabayarnama"), ""), sptField,
                     FxDB(drutama("vprekselisihkursnama"), ""), sptField,
                     FxDB(drutama("vprekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("vpnotransaksivpp"), ""), sptField,
                     FxDB(drutama("vpstatusnama"), ""), sptField,
                     FxDB(drutama("vpstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("vpinputusernama"), ""), sptField,
                     FxDB(drutama("vpmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idvpdetail"), 0), sptField,
                     FxDB(dr("idvp"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptField,
                     FxDB(dr("jmlbayarvalas"), 0), sptField,
                     FxDB(dr("diskontermin"), ""), sptField,
                     FxDB(dr("jmldiskontermin"), 0), sptField,
                     FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("idvppdetail"), 0), sptField,
                     FxDB(dr("urutan"), 0), sptField,
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
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("diskon1"), 0), sptField,
                     FxDB(dr("haridiskon1"), 0), sptField,
                     FxDB(dr("diskon2"), 0), sptField,
                     FxDB(dr("haridiskon2"), 0), sptField,
                     FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'PANGGIL QUERY
            sql = query.PanggilQuery("m4_vp_getdata_pay")

            'AMBIL DATA PAY
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M4_Vp_Pay", "idvp=" & idtransaksi, "idvp ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idvpcarabayar"), 0), sptField,
                     FxDB(dr("idvp"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idvppcarabayar"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "VP transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3, vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vpnotransaksivpp, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, vpmodifikasiusernama, kpkp" & sptSubParam & "idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, vppnotransaksi, inputtgl" & sptSubParam & "idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idvppcarabayar, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VpSearch(ByVal param As String) As String
        'M4_VpSearch --------------------------------------------------------
        'vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, 
        'vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, 
        'vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vppnotransaksi, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, 
        'vpmodifikasiusernama

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
            Filter = Filter.Replace("vpsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vpsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vp_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Vp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("vpid"), 0), sptField,
                     FxDB(dr("vpcabang"), ""), sptField,
                     FxDB(dr("vplokasi"), ""), sptField,
                     FxDB(dr("vpgudang"), ""), sptField,
                     FxDB(dr("vpsumber"), ""), sptField,
                     FxDB(dr("vpautonotransaksi"), 0), sptField,
                     FxDB(dr("vpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vptgl"), ""), formatTgl), sptField,
                     FxDB(dr("vpkodepa"), 0), sptField,
                     FxDB(dr("vpsupplier"), 0), sptField,
                     FxDB(dr("vpsupplierkontak"), ""), sptField,
                     FxDB(dr("vp1alamat1"), ""), sptField,
                     FxDB(dr("vp1alamat2"), ""), sptField,
                     FxDB(dr("vp1alamat3"), ""), sptField,
                     FxDB(dr("vp2alamat1"), ""), sptField,
                     FxDB(dr("vp2alamat2"), ""), sptField,
                     FxDB(dr("vp2alamat3"), ""), sptField,
                     FxDB(dr("vpbagianpembayaran"), 0), sptField,
                     FxDB(dr("vpuraian"), ""), sptField,
                     FxDB(dr("vpcatatan"), ""), sptField,
                     FxDB(dr("vpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("vpcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vptglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("vpmatauang"), ""), sptField,
                     FxDB(dr("vpkurs"), 0), sptField,
                     FxDB(dr("vptotalap"), 0), sptField,
                     FxDB(dr("vptotalapvalas"), 0), sptField,
                     FxDB(dr("vptotalar"), 0), sptField,
                     FxDB(dr("vptotalarvalas"), 0), sptField,
                     FxDB(dr("vpbayar"), 0), sptField,
                     FxDB(dr("vpbayarvalas"), 0), sptField,
                     FxDB(dr("vpselisihkurs"), 0), sptField,
                     FxDB(dr("vprekselisihkurs"), ""), sptField,
                     FxDB(dr("vpdiskontermin"), 0), sptField,
                     FxDB(dr("vpdiskonterminvalas"), 0), sptField,
                     FxDB(dr("vprekdiskontermin"), ""), sptField,
                     FxDB(dr("vpidvpp"), 0), sptField,
                     FxDB(dr("vpstatus"), 0), sptField,
                     FxDB(dr("vpstatussebelumnya"), 0), sptField,
                     FxDB(dr("vpjmlrevisi"), 0), sptField,
                     FxDB(dr("vpcetakanke"), 0), sptField,
                     FxDB(dr("vpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vpisclose"), 0), sptField,
                     FxDB(dr("vpcabangnama"), ""), sptField,
                     FxDB(dr("vplokasinama"), ""), sptField,
                     FxDB(dr("vpgudangnama"), ""), sptField,
                     FxDB(dr("vpsupplierkode"), ""), sptField,
                     FxDB(dr("vpsuppliernama"), ""), sptField,
                     FxDB(dr("vpbagianpembayarankode"), ""), sptField,
                     FxDB(dr("vpbagianpembayarannama"), ""), sptField,
                     FxDB(dr("vpcarabayarnama"), ""), sptField,
                     FxDB(dr("vprekselisihkursnama"), ""), sptField,
                     FxDB(dr("vprekdiskonterminnama"), ""), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
                     FxDB(dr("vpstatusnama"), ""), sptField,
                     FxDB(dr("vpstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("vpinputusernama"), ""), sptField,
                     FxDB(dr("vpmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpposting, vppostingtgl, vpisclose, vpcabangnama, vplokasinama, vpgudangnama, vpsupplierkode, vpsuppliernama, vpbagianpembayarankode, vpbagianpembayarannama, vpcarabayarnama, vprekselisihkursnama, vprekdiskonterminnama, vppnotransaksi, vpstatusnama, vpstatussebelumnyanama, vpinputusernama, vpmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VpTerkait(ByVal param As String) As String
        'M4_VpTerkait --------------------------------------------------------
        'vpid, vpnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "rmid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vp_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("vpid"), 0), sptField,
                     FxDB(dr("vpnotransaksi"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idterkait"), 0), sptField,
                     FxDB(dr("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related VP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vpid, vpnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal MUFungsional As String, _
                                    ByVal ftExistOutstandingRI As String, ByVal ftOutstandingRI As String, _
                                    ByVal ftExistOutstandingAP As String, ByVal ftOutstandingAP As String, _
                                    ByVal ftExistOutstandingPRT As String, ByVal ftOutstandingPRT As String, _
                                    ByVal updFilterRI As String, ByVal updFilterAP As String, ByVal updFilterPRT As String, ByVal formatTgl As String, ByVal tglPembayaran As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, sumber As String = "", notransaksi As String = "", matauang As String = "", tgl As String = ""
        Dim filterLookup As String = "", urutan As String = "", sisa As Double = 0

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idvppdetail, sumber, notransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("sumber")
                notransaksi = dtval.Rows(0)("notransaksi")

                filterLookup = "idvppdetail=" & dtval.Rows(0)("idvppdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in VPP" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBAYAR YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        sql = "SELECT vppd.idvppdetail, (vppd.jmlbayar - vppd.jmlvp) as sisavp, (vppd.jmlbayarvalas - vppd.jmlvpvalas) as sisavpvalas, vppd.matauang, vppd.sumber, (CASE vppd.sumber WHEN 'AP' THEN ap.apnotransaksi WHEN 'RI' THEN ri.rinotransaksi WHEN 'PRT' THEN prt.prtnotransaksi ELSE vppd.rekhutangpiutang END) as notransaksi FROM m4_vpp_detail AS vppd LEFT JOIN m4_ap ap ON vppd.sumber = 'AP' AND vppd.idtransaksi = ap.apid LEFT JOIN m4_ri ri ON vppd.sumber = 'RI' AND vppd.idtransaksi = ri.riid LEFT JOIN m4_prt prt ON vppd.sumber = 'PRT' AND vppd.idtransaksi = prt.prtid WHERE " & ftOutstanding
        dtval = AsDataTableAmbilDariDB(sql)
        If dtval.Rows.Count > 0 Then
            'Ambil informasi utk errmessage
            sumber = dtval.Rows(0)("sumber")
            notransaksi = dtval.Rows(0)("notransaksi")
            matauang = dtval.Rows(0)("matauang")
            If matauang = MUFungsional Then sisa = dtval.Rows(0)("sisavp") Else sisa = dtval.Rows(0)("sisavpvalas")

            filterLookup = "idvppdetail=" & dtval.Rows(0)("idvppdetail")
            dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
            If dtLookup.Rows.Count > 0 Then
                urutan = dtLookup.Rows(0)("urutan")
            End If
            errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in VPP, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'RI
        If Len(ftExistOutstandingRI) > 0 Then 'ftExistOutstanding = rowExists, riid, risumber, rinotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingRI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("rinotransaksi")
                sumber = dtval.Rows(0)("risumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("risumber") & "' AND idtransaksi = '" & dtval.Rows(0)("riid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in RI" : GoTo selesai
            End If
        End If

        'CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        If Len(updFilterRI) > 0 Then
            sql = "SELECT ri.riid, ri.risumber, ri.ritgl, ri.rinotransaksi FROM m4_ri ri WHERE ri.ritgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterRI & ")"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("risumber")
                notransaksi = dtval.Rows(0)("rinotransaksi")
                tgl = dtval.Rows(0)("ritgl")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("riid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingRI) > 0 Then
            sql = "SELECT ri.riid, ri.risumber, ri.rinotransaksi, ri.rimatauang, round(ri.ritotaltransaksi - ri.rijmlbayar, 5) as risisatransaksi FROM m4_ri ri LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingRI
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("rinotransaksi")
                sumber = dtval.Rows(0)("risumber")
                sisa = dtval.Rows(0)("risisatransaksi")
                matauang = dtval.Rows(0)("rimatauang")

                filterLookup = "sumber = '" & dtval.Rows(0)("risumber") & "' AND idtransaksi = '" & dtval.Rows(0)("riid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in RI, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'AP
        If Len(ftExistOutstandingAP) > 0 Then 'ftExistOutstanding = rowExists, apid, apsumber, apnotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingAP)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("apnotransaksi")
                sumber = dtval.Rows(0)("apsumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("apsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("apid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in AP" : GoTo selesai
            End If
        End If

        'CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        If Len(updFilterAP) > 0 Then
            sql = "SELECT ap.apid, ap.apsumber, ap.aptgl, ap.apnotransaksi FROM m4_ap ap WHERE ap.aptgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterAP & ")"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("apsumber")
                notransaksi = dtval.Rows(0)("apnotransaksi")
                tgl = dtval.Rows(0)("aptgl")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("apid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingAP) > 0 Then
            sql = "SELECT ap.apid, ap.apsumber, ap.apnotransaksi, ap.apmatauang, (CASE ap.apmatauang WHEN s.snilai THEN ap.apjumlah - ap.apjumlahbayar ELSE ap.apjumlahvalas - ap.apjumlahbayarvalas END) apsisatransaksi FROM m4_ap ap LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingAP
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("apnotransaksi")
                sumber = dtval.Rows(0)("apsumber")
                sisa = dtval.Rows(0)("apsisatransaksi")
                matauang = dtval.Rows(0)("apmatauang")

                filterLookup = "sumber = '" & dtval.Rows(0)("apsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("apid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in AP, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'PRT
        If Len(ftExistOutstandingPRT) > 0 Then 'ftExistOutstanding = rowExists, prtid, prtsumber, prtnotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPRT)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("prtnotransaksi")
                sumber = dtval.Rows(0)("prtsumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("prtsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("prtid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in PRT" : GoTo selesai
            End If
        End If

        'CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        If Len(updFilterPRT) > 0 Then
            sql = "SELECT prt.prtid, prt.prtsumber, prt.prttgl, prt.prtnotransaksi FROM m4_prt prt WHERE prt.prttgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterPRT & ")"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("prtsumber")
                notransaksi = dtval.Rows(0)("prtnotransaksi")
                tgl = dtval.Rows(0)("prttgl")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("prtid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingPRT) > 0 Then
            sql = "SELECT prt.prtid, prt.prtsumber, prt.prtnotransaksi, prt.prtmatauang, prt.prttotaltransaksi - prt.prtjmlbayar as prtsisatransaksi FROM m4_prt prt LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingPRT
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("prtnotransaksi")
                sumber = dtval.Rows(0)("prtsumber")
                sisa = dtval.Rows(0)("prtsisatransaksi")
                matauang = dtval.Rows(0)("prtmatauang")

                filterLookup = "sumber = '" & dtval.Rows(0)("prtsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("prtid") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in PRT, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_VpSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPay(), dataRowPay() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, tglLunas As String = ""


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


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        ''CEK PAGENUMBER
        'If (IsNumeric(pagingSplit(0)) = False) Then
        '    result(2) = "pageNumber required numeric." : GoTo selesai
        'End If

        ''CEK ITEMLIMIT
        'If (IsNumeric(pagingSplit(1)) = False) Then
        '    result(2) = "itemLimit required numeric." : GoTo selesai
        'End If

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'vpid(0) As Integer, vpcabang(1) As String, vplokasi(2) As String, vpgudang(3) As String, vpsumber(4) As String, 
        'vpautonotransaksi(5) As Integer, vpnotransaksi(6) As String, vptgl(7) As Date, vpkodepa(8) As Integer, vpsupplier(9) As Integer, 
        'vpsupplierkontak(10) As String, vp1alamat1(11) As String, vp1alamat2(12) As String, vp1alamat3(13) As String, vp2alamat1(14) As String, 
        'vp2alamat2(15) As String, vp2alamat3(16) As String, vpbagianpembayaran(17) As Integer, vpuraian(18) As String, vpcatatan(19) As String, 
        'vpnoref(20) As String, vptglnoref(21) As Date, vpcarabayar(22) As Integer, vptglbayar(23) As Date, vpmatauang(24) As String, 
        'vpkurs(25) As Double, vptotalap(26) As Double, vptotalapvalas(27) As Double, vptotalar(28) As Double, vptotalarvalas(29) As Double, 
        'vpbayar(30) As Double, vpbayarvalas(31) As Double, vpselisihkurs(32) As Double, vprekselisihkurs(33) As String, vpdiskontermin(34) As Double, 
        'vpdiskonterminvalas(35) As Double, vprekdiskontermin(36) As String, vpidvpp(37) As Integer, vpstatus(38) As Integer, vpstatussebelumnya(39) As Integer, 
        'vpjmlrevisi(40) As Integer, vpcetakanke(41) As Integer, vpinputuser(42) As Integer, vpinputtgl(43) As DateTime, vpmodifikasiuser(44) As Integer, 
        'vpmodifikasitgl(45) As DateTime, vpisclose(46) As Integer, vpcustomtext1(47) As String, vpcustomtext2(48) As String, vpcustomtext3(49) As String, 
        'vpcustomtext4(50) As String, vpcustomtext5(51) As String, vpcustomint1(52) As Integer, vpcustomint2(53) As Integer, vpcustomint3(54) As Integer, 
        'vpcustomdbl1(55) As Double, vpcustomdbl2(56) As Double, vpcustomdbl3(57) As Double, vpcustomdate1(58) As Date, vpcustomdate2(59) As Date, 
        'vpcustomdate3(60) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'vpid, vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, 
        'vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, 
        'vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, 
        'vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, 
        'vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, 
        'vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, 
        'vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpisclose, vpcustomtext1, vpcustomtext2, 
        'vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, 
        'vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 61) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'vpid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "vpid required numeric." : GoTo selesai
        End If
        'vpautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "vpautonotransaksi required numeric." : GoTo selesai
        End If
        'vptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "vptgl required date." : GoTo selesai
        End If
        'vpkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "vpkodepa required numeric." : GoTo selesai
        End If
        'vpsupplier(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "vpsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "vpsupplier can't be empty." : GoTo selesai
        End If
        'vpbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "vpbagianpembayaran required numeric." : GoTo selesai
        End If
        'vptglnoref(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "vptglnoref required date." : GoTo selesai
        End If
        'vpcarabayar(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "vpcarabayar required numeric." : GoTo selesai
        End If
        'vptglbayar(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "vptglbayar required date." : GoTo selesai
        End If
        'vpkurs(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "vpkurs required numeric." : GoTo selesai
        End If
        'vptotalap(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "vptotalap required numeric." : GoTo selesai
        End If
        'vptotalapvalas(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "vptotalapvalas required numeric." : GoTo selesai
        End If
        'vptotalar(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "vptotalar required numeric." : GoTo selesai
        End If
        'vptotalarvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "vptotalarvalas required numeric." : GoTo selesai
        End If
        'vpbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "vpbayar required numeric." : GoTo selesai
        End If
        'vpbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "vpbayarvalas required numeric." : GoTo selesai
        End If
        'vpselisihkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "vpselisihkurs required numeric." : GoTo selesai
        End If
        'vpdiskontermin(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "vpdiskontermin required numeric." : GoTo selesai
        End If
        'vpdiskonterminvalas(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "vpdiskonterminvalas required numeric." : GoTo selesai
        End If
        'vpidvpp(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "vpidvpp required numeric." : GoTo selesai
        End If
        'vpstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "vpstatus required numeric." : GoTo selesai
        End If
        'vpstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "vpstatussebelumnya required numeric." : GoTo selesai
        End If
        'vpjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "vpjmlrevisi required numeric." : GoTo selesai
        End If
        'vpcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "vpcetakanke required numeric." : GoTo selesai
        End If
        'vpinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "vpinputuser required numeric." : GoTo selesai
        End If
        'vpinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "vpinputtgl required date." : GoTo selesai
        End If
        'vpmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "vpmodifikasiuser required numeric." : GoTo selesai
        End If
        'vpmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "vpmodifikasitgl required date." : GoTo selesai
        End If
        'vpisclose(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "vpisclose required numeric." : GoTo selesai
        End If
        'vpcustomint1(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "vpcustomint1 required numeric." : GoTo selesai
        End If
        'vpcustomint2(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "vpcustomint2 required numeric." : GoTo selesai
        End If
        'vpcustomint3(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "vpcustomint3 required numeric." : GoTo selesai
        End If
        'vpcustomdbl1(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "vpcustomdbl1 required numeric." : GoTo selesai
        End If
        'vpcustomdbl2(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "vpcustomdbl2 required numeric." : GoTo selesai
        End If
        'vpcustomdbl3(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "vpcustomdbl3 required numeric." : GoTo selesai
        End If
        'vpcustomdate1(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "vpcustomdate1 required date." : GoTo selesai
        End If
        'vpcustomdate2(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "vpcustomdate2 required date." : GoTo selesai
        End If
        'vpcustomdate3(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "vpcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'vpcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "vpcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "vpcabang should not be more than 25 character." : GoTo selesai
        End If

        'vplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "vplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "vplokasi should not be more than 25 character." : GoTo selesai
        End If

        'vpsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "vpsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "vpsumber should not be more than 10 character." : GoTo selesai
        End If

        'vpnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "vpnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "vpnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'vptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "vptgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS TRANSAKSI
        tglLunas = AsFormatTanggal(dataUtama(7))

        'vptglnoref(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "vptglnoref can't be empty" : GoTo selesai
        End If

        'vptglbayar(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "vptglbayar can't be empty" : GoTo selesai
        End If

        'vpmatauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "vpmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "vpmatauang should not be more than 25 character." : GoTo selesai
        End If

        'vpkurs(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "vpkurs can't be empty" : GoTo selesai
        End If

        'vptotalap(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "vptotalap can't be empty" : GoTo selesai
        End If

        'vptotalapvalas(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "vptotalapvalas can't be empty" : GoTo selesai
        End If

        'vptotalar(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "vptotalar can't be empty" : GoTo selesai
        End If

        'vptotalarvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "vptotalarvalas can't be empty" : GoTo selesai
        End If

        'vpbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "vpbayar can't be empty" : GoTo selesai
        End If

        'vpbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "vpbayarvalas can't be empty" : GoTo selesai
        End If

        'vpselisihkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "vpselisihkurs can't be empty" : GoTo selesai
        End If

        'vpdiskontermin(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "vpdiskontermin can't be empty" : GoTo selesai
        End If

        'vpdiskonterminvalas(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "vpdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'vpinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "vpinputtgl can't be empty" : GoTo selesai
        End If

        'vpmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "vpmodifikasitgl can't be empty" : GoTo selesai
        End If

        'vpcustomdbl1(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "vpcustomdbl1 can't be empty" : GoTo selesai
        End If

        'vpcustomdbl2(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "vpcustomdbl2 can't be empty" : GoTo selesai
        End If

        'vpcustomdbl3(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "vpcustomdbl3 can't be empty" : GoTo selesai
        End If

        'vpcustomdate1(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "vpcustomdate1 can't be empty" : GoTo selesai
        End If

        'vpcustomdate2(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "vpcustomdate2 can't be empty" : GoTo selesai
        End If

        'vpcustomdate3(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "vpcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "vpid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vptglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vptotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vpbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vpselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vprekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vprekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpidvpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "vpid~vpcabang~vplokasi~vpgudang~vpsumber~vpautonotransaksi~vpnotransaksi~vptgl~vpkodepa~vpsupplier~vpsupplierkontak~vp1alamat1~vp1alamat2~vp1alamat3~vp2alamat1~vp2alamat2~vp2alamat3~vpbagianpembayaran~vpuraian~vpcatatan~vpnoref~vptglnoref~vpcarabayar~vptglbayar~vpmatauang~vpkurs~vptotalap~vptotalapvalas~vptotalar~vptotalarvalas~vpbayar~vpbayarvalas~vpselisihkurs~vprekselisihkurs~vpdiskontermin~vpdiskonterminvalas~vprekdiskontermin~vpidvpp~vpstatus~vpstatussebelumnya~vpjmlrevisi~vpcetakanke~vpinputuser~vpinputtgl~vpmodifikasiuser~vpmodifikasitgl~vpisclose~vpcustomtext1~vpcustomtext2~vpcustomtext3~vpcustomtext4~vpcustomtext5~vpcustomint1~vpcustomint2~vpcustomint3~vpcustomdbl1~vpcustomdbl2~vpcustomdbl3~vpcustomdate1~vpcustomdate2~vpcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idvpdetail(0) As Integer, idvp(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, rekhutangpiutang(14) As String, 
        'catatan(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'idvppdetail(20) As Integer, urutan(21) As Integer, isclose(22) As Integer, customtext1(23) As String, customtext2(24) As String, 
        'customtext3(25) As String, customdbl1(26) As Double, customdbl2(27) As Double, customdbl3(28) As Double, customdate1(29) As Date, 
        'customdate2(30) As Date, customdate3(31) As Date, rencana(32) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, rencana


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idvpdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idvppdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'AMBIL MATA UANG FUNGSIONAL DARI SETTING ================
        Dim MUFungsional As String = ""
        Dim dtSetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
        If dtSetting.Rows.Count > 0 Then
            MUFungsional = dtSetting.Rows(0)(0)
        Else
            result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
        End If
        'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING =========


        'VARIABEL VALIDASI OUTSTANDING
        Dim ftExistOutstanding As String = "", ftOutstanding As String = ""
        Dim updNilai As String = "", updNilaiValas As String = "", updFilter As String = "", sumberDetail As String = "", matauangDetail As String = "", norek As String = ""
        Dim idtransaksiDetail As Integer = 0, idvppdetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0
        Dim Outstanding As Double = 0, OutstandingValas As Double = 0

        'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT, 
        'RI
        Dim ftExistOutstandingRI As String = "", ftOutstandingRI As String = "", updNilaiRI As String = "", updFilterRI As String = "", updTglLunasRI As String = ""
        'AP
        Dim ftExistOutstandingAP As String = "", ftOutstandingAP As String = "", updNilaiAP As String = "", updNilaiValasAP As String = "", updFilterAP As String = "", updTglLunasAP As String = ""
        'PRT
        Dim ftExistOutstandingPRT As String = "", ftOutstandingPRT As String = "", updNilaiPRT As String = "", updFilterPRT As String = "", updTglLunasPRT As String = ""


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 33) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idvpdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idvpdetail required numeric." : GoTo selesai
            End If
            'idvp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idvp required numeric." : GoTo selesai
            End If
            'idtransaksi(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - idtransaksi required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'totaltransaksi(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - totaltransaksi required numeric." : GoTo selesai
            End If
            'terbayar(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - terbayar required numeric." : GoTo selesai
            End If
            'rencana(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - rencana required numeric." : GoTo selesai
            End If
            'sisa(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - sisa required numeric." : GoTo selesai
            End If
            'jmlbayar(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbayar required numeric." : GoTo selesai
            End If
            'jmlbayarvalas(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbayarvalas required numeric." : GoTo selesai
            End If
            'jmldiskontermin(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmldiskontermin required numeric." : GoTo selesai
            End If
            'jmldiskonterminvalas(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas required numeric." : GoTo selesai
            End If
            'idvppdetail(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - idvppdetail required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(29) As Date
            If (IsDate(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If
            If (dataRowDetail(2) <> "RI" And _
                dataRowDetail(2) <> "AP" And _
                dataRowDetail(2) <> "PRT" And _
                dataRowDetail(2) <> "CA") Then
                result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
            End If

            'matauang(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'totaltransaksi(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - totaltransaksi can't be empty" : GoTo selesai
            End If

            'terbayar(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - terbayar can't be empty" : GoTo selesai
            End If

            'rencana(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - rencana can't be empty" : GoTo selesai
            End If

            'sisa(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - sisa can't be empty" : GoTo selesai
            End If

            'jmlbayar(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayar can't be empty" : GoTo selesai
            End If

            'jmlbayarvalas(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayarvalas can't be empty" : GoTo selesai
            End If

            'diskontermin(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - diskontermin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - diskontermin should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskontermin(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskontermin can't be empty" : GoTo selesai
            End If

            'jmldiskonterminvalas(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas can't be empty" : GoTo selesai
            End If

            'rekhutangpiutang(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(26) As Double
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(29) As Date
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(30) As Date
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idvpdetail~idvp~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~idvppdetail~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'sumber(2) As String            , idtransaksi(3) As Integer            , jmlbayar(9) As Double
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3) : jmlbayar = dataRowDetail(9)
            'jmlbayarvalas(10) As Double      , rekhutangpiutang(14) As String, idvppdetail(20) As Integer
            jmlbayarvalas = dataRowDetail(10) : norek = dataRowDetail(14) : idvppdetail = dataRowDetail(20)
            'matauang(4) As String
            matauangDetail = dataRowDetail(4)


            'VALIDASI TRANSAKSI PEMBAYARAN ----------------
            Select Case sumberDetail
                Case "RI"
                    '1. CEK DATA EXIST
                    ftExistOutstandingRI = IIf(Len(ftExistOutstandingRI.ToString) = 0, "", ftExistOutstandingRI & " UNION ")
                    ftExistOutstandingRI = String.Concat(ftExistOutstandingRI, "SELECT EXISTS(SELECT 1 FROM m4_ri WHERE riid = '" & idtransaksiDetail & "' AND (ristatus = 2 OR ristatus = 3 OR ristatus = 4 OR ristatus = 7) LIMIT 1) as rowExists, riid, risumber, rinotransaksi FROM m4_ri WHERE riid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    If matauangDetail = MUFungsional Then
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    Else
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    End If
                    ftOutstandingRI = IIf(Len(ftOutstandingRI.ToString) = 0, "", ftOutstandingRI & " OR ")
                    ftOutstandingRI = String.Concat(ftOutstandingRI, " (ri.riid = '" & idtransaksiDetail & "' AND " & Outstanding & " > ROUND(ri.ritotaltransaksi - ri.rijmlbayar, 5)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiRI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ri.rijmlbayar + '" & Outstanding & "', 5) ", updNilaiRI)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                    updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    updTglLunasRI = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ri.rijmlbayar + '" & Outstanding & "', 5) >= ri.ritotaltransaksi THEN '" & FixQuotes(tglLunas) & "' ELSE ri.ritgllunas END) ", updTglLunasRI)

                Case "AP"
                    '1. CEK DATA EXIST
                    ftExistOutstandingAP = IIf(Len(ftExistOutstandingAP.ToString) = 0, "", ftExistOutstandingAP & " UNION ")
                    ftExistOutstandingAP = String.Concat(ftExistOutstandingAP, "SELECT EXISTS(SELECT 1 FROM m4_ap WHERE apid = '" & idtransaksiDetail & "' AND (apstatus = 2 OR apstatus = 3 OR apstatus = 4 OR apstatus = 7) LIMIT 1) as rowExists, apid, apsumber, apnotransaksi FROM m4_ap WHERE apid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    ftOutstandingAP = IIf(Len(ftOutstandingAP.ToString) = 0, "", ftOutstandingAP & " OR ")
                    ftOutstandingAP = String.Concat(ftOutstandingAP, " (ap.apid = '" & idtransaksiDetail & "' AND (CASE ap.apmatauang WHEN s.snilai THEN " & Outstanding & " > ROUND(ap.apjumlah - ap.apjumlahbayar,2) ELSE " & OutstandingValas & " > ROUND(ap.apjumlahvalas - ap.apjumlahbayarvalas,2) END)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayar + '" & Outstanding & "', 5) ", updNilaiAP)
                    updNilaiValasAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayarvalas + '" & OutstandingValas & "', 5) ", updNilaiValasAP)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                    updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    If matauangDetail = MUFungsional Then
                        updTglLunasAP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ap.apjumlahbayar + '" & Outstanding & "', 5) >= ap.apjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ap.aptgllunas END) ", updTglLunasAP)
                    Else
                        updTglLunasAP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ap.apjumlahbayarvalas + '" & OutstandingValas & "', 5) >= ap.apjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ap.aptgllunas END) ", updTglLunasAP)
                    End If

                Case "PRT"
                    '1. CEK DATA EXIST
                    ftExistOutstandingPRT = IIf(Len(ftExistOutstandingPRT.ToString) = 0, "", ftExistOutstandingPRT & " UNION ")
                    ftExistOutstandingPRT = String.Concat(ftExistOutstandingPRT, "SELECT EXISTS(SELECT 1 FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "' AND (prtstatus = 2 OR prtstatus = 3 OR prtstatus = 4 OR prtstatus = 7) LIMIT 1) as rowExists, prtid, prtsumber, prtnotransaksi FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    If matauangDetail = MUFungsional Then
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    Else
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    End If
                    ftOutstandingPRT = IIf(Len(ftOutstandingPRT.ToString) = 0, "", ftOutstandingPRT & " OR ")
                    ftOutstandingPRT = String.Concat(ftOutstandingPRT, " (prt.prtid = '" & idtransaksiDetail & "' AND " & Outstanding & " > ROUND(prt.prttotaltransaksi - prt.prtjmlbayar,2)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiPRT = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(prt.prtjmlbayar + '" & Outstanding & "', 5) ", updNilaiPRT)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                    updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    updTglLunasPRT = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(prt.prtjmlbayar + '" & Outstanding & "', 5) >= prt.prttotaltransaksi THEN '" & FixQuotes(tglLunas) & "' ELSE prt.prttgllunas END) ", updTglLunasPRT)
            End Select
            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------


            'VALIDASI OUTSTANDING -------------------------
            If idvppdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                Select Case sumberDetail
                    Case "RI"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, rinotransaksi as notransaksi FROM m4_ri WHERE riid = '" & idtransaksiDetail & "'")
                    Case "AP"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, apnotransaksi as notransaksi FROM m4_ap WHERE apid = '" & idtransaksiDetail & "'")
                    Case "PRT"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, prtnotransaksi as notransaksi FROM m4_prt WHERE prtid = '" & idtransaksiDetail & "'")
                    Case "CA"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM M4_vpp_detail JOIN M4_vpp ON idvpp = vppid WHERE idvppdetail = '" & idvppdetail & "' AND (vppstatus = 2 OR vppstatus = 3 OR vppstatus = 4 OR vppstatus = 7) LIMIT 1) as rowExists, '" & idvppdetail & "' as idvppdetail, '" & sumberDetail & "' as sumber, '" & norek & "' as notransaksi")
                    Case Else
                        result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
                End Select

                '2. CEK JML OUTSTANDING
                Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "idvppdetail=" & idvppdetail)
                OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "idvppdetail=" & idvppdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (vppd.idvppdetail = " & idvppdetail & " AND " & Outstanding & " > (vppd.jmlbayar - vppd.jmlvp)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvp + '" & Outstanding & "', 5) ", updNilai)
                updNilaiValas = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvpvalas + '" & OutstandingValas & "', 5) ", updNilaiValas)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idvppdetail = '" & idvppdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idvpcarabayar(0) As Integer, idvp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'idvppcarabayar(15) As Integer, isclose(16) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, idvppcarabayar, isclose

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idvpcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "idvppcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "isclose", AsEnumTypeData.AsInt64)

        If (Len(dataSplit(2)) > 0) Then

            'VALIDASI DAN SET DATA PAY ======================================================
            'SPLIT PARAMETER DATA PAY
            dataPay = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA PAY ===============================================

            'VALIDASI DAN SET DATA ROW PAY ==================================================
            Dim JmlDtPay As Integer = dataPay.Length
            For i = 1 To JmlDtPay
                'SPLIT DATA PAY
                dataRowPay = dataPay(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA PAY -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowPay.Length <> 17) Then
                    result(2) = "Pay Row : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idvpcarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvpcarabayar required numeric." : GoTo selesai
                End If
                'idvp(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvp required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowPay(2)) = False) Then
                    result(2) = "Pay Row : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowPay(4)) = False) Then
                    result(2) = "Pay Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowPay(5)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowPay(6)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowPay(8)) = False) Then
                    result(2) = "Pay Row : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowPay(14)) = False) Then
                    result(2) = "Pay Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idvppcarabayar(15) As Integer
                If (IsNumeric(dataRowPay(15)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvppcarabayar required numeric." : GoTo selesai
                End If
                'isclose(16) As Integer
                If (IsNumeric(dataRowPay(16)) = False) Then
                    result(2) = "Pay Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA PAY -----------------------------------

                'VALIDASI DATA PAY ---------------------------------------
                'matauang(3) As String
                If Len(dataRowPay(3)) = 0 Then
                    result(2) = "Pay Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(3)) > 25 Then
                    result(2) = "Pay Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowPay(4)) = 0 Then
                    result(2) = "Pay Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowPay(5)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowPay(5) <= 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowPay(6)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowPay(8)) = 0 Then
                    result(2) = "Pay Row : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowPay(11)) = 0 Then
                    result(2) = "Pay Row : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(11)) > 25 Then
                    result(2) = "Pay Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Pay Row : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Pay Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Pay Row : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Pay Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Pay Row : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Pay Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Pay Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Pay Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idvpcarabayar~idvp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~idvppcarabayar~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15) & "~" & dataRowPay(16)) = False Then
                    result(2) = "Pay Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA PAY ===========================================

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

                ''CEK TOTAL UTAMA DAN BAYAR ==============================
                'Dim jumlah As Double = AsDataTableDSum(dtpay, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtpay, "jumlahvalas")
                'If Double.Parse(drutama("vpbayar")) <> jumlah Then
                '    Dim selisih(2) As String
                '    selisih = F_Nominal(Double.Parse(drutama("vpbayar")) - jumlah, False).Split(sptSubParam)
                '    result(2) = "Total amount of pay is not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                '    'ElseIf drutama("vppbayarvalas") <> jumlahvalas Then
                '    '    result(2) = "Total amount of foreign pay is not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN BAYAR =======================


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("vptgl")), AsFormatTanggal(drutama("vptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "vpmatauang", "vprekselisihkurs~vprekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK MATAUANG COA =======================================
                'PAY
                rsCekCoa = ValidasiMatauangCOA(dtutama, "vpmatauang", "", dtpay, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("vpstatus") = 2 Then

                    'CEK JMLBAYAR TRANSAKSI ---------------------
                    Dim JmlRI As Double = 0, JmlCoa As Double = 0
                    Dim JmlAP As Double = 0, JmlPRT As Double = 0
                    Dim JmlTabBayar As Double = 0
                    Dim TotalAP As Double = 0, TotalAR As Double = 0

                    'TOTAL AP = RI + COA
                    JmlRI = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'RI'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'RI'")
                    JmlCoa = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'CA'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'CA'")
                    TotalAP = JmlRI + JmlCoa

                    'TOTAL AR = AP + PRT + BAYAR
                    JmlAP = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'AP'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'AP'")
                    JmlPRT = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'PRT'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'PR'")
                    JmlTabBayar = AsDataTableDSum(dtpay, "jumlah")
                    TotalAR = JmlAP + JmlPRT + JmlTabBayar + Double.Parse(drutama("vpselisihkurs"))

                    'JIKA SELISIH TOTAL AP DAN TOTAL AP >= 0.1 MAKA ALERT TIDAK BISA DISIMPAN
                    If Math.Abs(TotalAP - TotalAR) >= 0.1 Then
                        Dim selisih(2) As String
                        selisih = F_Nominal(F_Round(Math.Abs(TotalAP - TotalAR)), False).Split(sptSubParam)
                        result(2) = "Total AP and Total AR are not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK JMLBAYAR TRANSAKSI --------------

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, MUFungsional, ftExistOutstandingRI, ftOutstandingRI, ftExistOutstandingAP, ftOutstandingAP, ftExistOutstandingPRT, ftOutstandingPRT, updFilterRI, updFilterAP, updFilterPRT, formatTgl, drutama("vptgl"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("vpid")
                    notransaksi = drutama("vpnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(vpid), vpnotransaksi FROM M4_vp WHERE vpid='" & result(4) & "' AND vpstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(vpid) FROM M4_vp WHERE vpnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_vp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Vp_HistorySimpan("" & paramSplit(0) & "★M4_Vp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("vpsumber")) & "▼" & FixQuotes(drutama("vpid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Vp set vpcabang  = '" & FixQuotes(drutama("vpcabang")) & "', vplokasi  = '" & FixQuotes(drutama("vplokasi")) & "', vpgudang  = '" & FixQuotes(drutama("vpgudang")) & "', vpsumber  = '" & FixQuotes(drutama("vpsumber")) & "', vpautonotransaksi  = " & drutama("vpautonotransaksi") & ", vpnotransaksi  = '" & FixQuotes(notransaksi) & "', vptgl  = '" & FixQuotes(AsFormatTanggal(drutama("vptgl"))) & "', vpkodepa  = " & drutama("vpkodepa") & ", vpsupplier  = " & drutama("vpsupplier") & ", vpsupplierkontak  = '" & FixQuotes(drutama("vpsupplierkontak")) & "', vp1alamat1  = '" & FixQuotes(drutama("vp1alamat1")) & "', vp1alamat2  = '" & FixQuotes(drutama("vp1alamat2")) & "', vp1alamat3  = '" & FixQuotes(drutama("vp1alamat3")) & "', vp2alamat1  = '" & FixQuotes(drutama("vp2alamat1")) & "', vp2alamat2  = '" & FixQuotes(drutama("vp2alamat2")) & "', vp2alamat3  = '" & FixQuotes(drutama("vp2alamat3")) & "', vpbagianpembayaran  = " & drutama("vpbagianpembayaran") & ", vpuraian  = '" & FixQuotes(drutama("vpuraian")) & "', vpcatatan  = '" & FixQuotes(drutama("vpcatatan")) & "', vpnoref  = '" & FixQuotes(drutama("vpnoref")) & "', vptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("vptglnoref"))) & "', vpcarabayar  = " & drutama("vpcarabayar") & ", vptglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("vptglbayar"))) & "', vpmatauang  = '" & FixQuotes(drutama("vpmatauang")) & "', vpkurs  = '" & FixDouble(drutama("vpkurs")) & "', vptotalap  = '" & FixDouble(drutama("vptotalap")) & "', vptotalapvalas  = '" & FixDouble(drutama("vptotalapvalas")) & "', vptotalar  = '" & FixDouble(drutama("vptotalar")) & "', vptotalarvalas  = '" & FixDouble(drutama("vptotalarvalas")) & "', vpbayar  = '" & FixDouble(drutama("vpbayar")) & "', vpbayarvalas  = '" & FixDouble(drutama("vpbayarvalas")) & "', vpselisihkurs  = '" & FixDouble(drutama("vpselisihkurs")) & "', vprekselisihkurs  = '" & FixQuotes(drutama("vprekselisihkurs")) & "', vpdiskontermin  = '" & FixDouble(drutama("vpdiskontermin")) & "', vpdiskonterminvalas  = '" & FixDouble(drutama("vpdiskonterminvalas")) & "', vprekdiskontermin  = '" & FixQuotes(drutama("vprekdiskontermin")) & "', vpidvpp  = " & drutama("vpidvpp") & ", vpstatus  = " & drutama("vpstatus") & ", vpstatussebelumnya  = " & drutama("vpstatussebelumnya") & ", vpjmlrevisi  = vpjmlrevisi+1, vpcetakanke  = " & drutama("vpcetakanke") & ", vpmodifikasiuser  = " & drutama("vpmodifikasiuser") & ", vpmodifikasitgl  = NOW(), vpcustomtext1  = '" & FixQuotes(drutama("vpcustomtext1")) & "', vpcustomtext2  = '" & FixQuotes(drutama("vpcustomtext2")) & "', vpcustomtext3  = '" & FixQuotes(drutama("vpcustomtext3")) & "', vpcustomtext4  = '" & FixQuotes(drutama("vpcustomtext4")) & "', vpcustomtext5  = '" & FixQuotes(drutama("vpcustomtext5")) & "', vpcustomint1  = " & drutama("vpcustomint1") & ", vpcustomint2  = " & drutama("vpcustomint2") & ", vpcustomint3  = " & drutama("vpcustomint3") & ", vpcustomdbl1  = '" & FixDouble(drutama("vpcustomdbl1")) & "', vpcustomdbl2  = '" & FixDouble(drutama("vpcustomdbl2")) & "', vpcustomdbl3  = '" & FixDouble(drutama("vpcustomdbl3")) & "', vpcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate1"))) & "', vpcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate2"))) & "', vpcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate3"))) & "' where vpid = '" & drutama("vpid") & "'"
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

                    If drutama("vpautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("vpcabang"), drutama("vplokasi"), drutama("vpsumber"), drutama("vptgl"))
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
                        notransaksi = drutama("vpnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(vpid) FROM m4_vp WHERE vpnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Vp (vpcabang, vplokasi, vpgudang, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl, vpkodepa, vpsupplier, vpsupplierkontak, vp1alamat1, vp1alamat2, vp1alamat3, vp2alamat1, vp2alamat2, vp2alamat3, vpbagianpembayaran, vpuraian, vpcatatan, vpnoref, vptglnoref, vpcarabayar, vptglbayar, vpmatauang, vpkurs, vptotalap, vptotalapvalas, vptotalar, vptotalarvalas, vpbayar, vpbayarvalas, vpselisihkurs, vprekselisihkurs, vpdiskontermin, vpdiskonterminvalas, vprekdiskontermin, vpidvpp, vpstatus, vpstatussebelumnya, vpjmlrevisi, vpcetakanke, vpinputuser, vpinputtgl, vpmodifikasiuser, vpmodifikasitgl, vpisclose, vpcustomtext1, vpcustomtext2, vpcustomtext3, vpcustomtext4, vpcustomtext5, vpcustomint1, vpcustomint2, vpcustomint3, vpcustomdbl1, vpcustomdbl2, vpcustomdbl3, vpcustomdate1, vpcustomdate2, vpcustomdate3) values('" & FixQuotes(drutama("vpcabang")) & "', '" & FixQuotes(drutama("vplokasi")) & "', '" & FixQuotes(drutama("vpgudang")) & "', '" & FixQuotes(drutama("vpsumber")) & "', " & drutama("vpautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("vptgl"))) & "', " & drutama("vpkodepa") & ", " & drutama("vpsupplier") & ", '" & FixQuotes(drutama("vpsupplierkontak")) & "', '" & FixQuotes(drutama("vp1alamat1")) & "', '" & FixQuotes(drutama("vp1alamat2")) & "', '" & FixQuotes(drutama("vp1alamat3")) & "', '" & FixQuotes(drutama("vp2alamat1")) & "', '" & FixQuotes(drutama("vp2alamat2")) & "', '" & FixQuotes(drutama("vp2alamat3")) & "', " & drutama("vpbagianpembayaran") & ", '" & FixQuotes(drutama("vpuraian")) & "', '" & FixQuotes(drutama("vpcatatan")) & "', '" & FixQuotes(drutama("vpnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vptglnoref"))) & "', " & drutama("vpcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("vptglbayar"))) & "', '" & FixQuotes(drutama("vpmatauang")) & "', '" & FixDouble(drutama("vpkurs")) & "', '" & FixDouble(drutama("vptotalap")) & "', '" & FixDouble(drutama("vptotalapvalas")) & "', '" & FixDouble(drutama("vptotalar")) & "', '" & FixDouble(drutama("vptotalarvalas")) & "', '" & FixDouble(drutama("vpbayar")) & "', '" & FixDouble(drutama("vpbayarvalas")) & "', '" & FixDouble(drutama("vpselisihkurs")) & "', '" & FixQuotes(drutama("vprekselisihkurs")) & "', '" & FixDouble(drutama("vpdiskontermin")) & "', '" & FixDouble(drutama("vpdiskonterminvalas")) & "', '" & FixQuotes(drutama("vprekdiskontermin")) & "', " & drutama("vpidvpp") & ", " & drutama("vpstatus") & ", " & drutama("vpstatussebelumnya") & ", " & drutama("vpjmlrevisi") & ", " & drutama("vpcetakanke") & ", " & drutama("vpinputuser") & ", NOW(), " & drutama("vpmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("vpisclose") & ", '" & FixQuotes(drutama("vpcustomtext1")) & "', '" & FixQuotes(drutama("vpcustomtext2")) & "', '" & FixQuotes(drutama("vpcustomtext3")) & "', '" & FixQuotes(drutama("vpcustomtext4")) & "', '" & FixQuotes(drutama("vpcustomtext5")) & "', " & drutama("vpcustomint1") & ", " & drutama("vpcustomint2") & ", " & drutama("vpcustomint3") & ", '" & FixDouble(drutama("vpcustomdbl1")) & "', '" & FixDouble(drutama("vpcustomdbl2")) & "', '" & FixDouble(drutama("vpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select vpid from M4_vp where vpnotransaksi='" & notransaksi & "' AND vpinputuser= '" & userid & "' order by vpmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vp_Detail where idvp = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idvpdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("idvppdetail") & ", " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Vp_Detail(idvpdetail, idvp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idvppdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vp_Pay where idvp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses pay
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    Dim rsCekGiro As String

                    For Each dr1 As DataRow In dtpay.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idvpcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idvppcarabayar") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("vpstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(4, 15, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("vpsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("vpsupplier") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M4_Vp_Pay(idvpcarabayar, idvp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idvppcarabayar, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("vpstatus") = 2 And Len(strGiro.ToString) > 0 Then
                        sql = "Insert into M2_Giro_List(glnogiro, glsumber, glidtransaksi, glnotransaksi, glkontak, glrekbank, glrekgiro, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, gltglcair, glstatus, glstatussebelumnya, glurutan) values" & strGiro.ToString & ""
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

                If drutama("vpstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ==================================================
                    If Len(updNilai) > 0 Then
                        'UPDATE DETAIL
                        sql = "UPDATE M4_vpp_detail SET jmlvp = (CASE idvppdetail " & updNilai & " ELSE jmlvp END), jmlvpvalas = (CASE idvppdetail " & updNilaiValas & " ELSE jmlvpvalas END) WHERE " & updFilter
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idvpp FROM M4_vpp_detail WHERE " & updFilter & " GROUP BY idvpp")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idvpp = '" & dr1("idvpp") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idvpp, SUM(jmlbayar) as jmlbayar, SUM(jmlvp) as jmlvp FROM M4_vpp_detail WHERE " & ftDetail & " GROUP BY idvpp")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlvp") >= dr1("jmlbayar") Then
                                    statusOut = 2
                                ElseIf dr1("jmlvp") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idvpp") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(vppid = '" & dr1("idvpp") & "')")
                            Next

                            sql = "UPDATE M4_vpp SET vppstatusvp = (CASE vppid " & updNilai & " ELSE vppstatusvp END) WHERE " & updFilter
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
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                    'RI
                    If Len(updNilaiRI) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri SET ri.rijmlbayar = (CASE ri.riid " & updNilaiRI & " ELSE ri.rijmlbayar END), ri.ritgllunas = (CASE ri.riid " & updTglLunasRI & " ELSE ri.ritgllunas END) WHERE " & updFilterRI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ri ri JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid =  t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE " & updFilterRI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'AP
                    If Len(updNilaiAP) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ap ap SET ap.apjumlahbayar = (CASE ap.apid " & updNilaiAP & " ELSE ap.apjumlahbayar END), ap.apjumlahbayarvalas = (CASE ap.apid " & updNilaiValasAP & " ELSE ap.apjumlahbayarvalas END), ap.aptgllunas = (CASE ap.apid " & updTglLunasAP & " ELSE ap.aptgllunas END) WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ap ap JOIN m2_transaction_journal t ON ap.apsumber = t.tsumber AND ap.apid =  t.tidtransaksi AND ap.apnotransaksi = t.tnotransaksi SET t.tstatuslunas = ap.apstatusbayar, t.ttgllunas = ap.aptgllunas WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'PRT
                    If Len(updNilaiPRT) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_prt prt SET prt.prtjmlbayar = (CASE prt.prtid " & updNilaiPRT & " ELSE prt.prtjmlbayar END), prt.prttgllunas = (CASE prt.prtid " & updTglLunasPRT & " ELSE prt.prttgllunas END) WHERE " & updFilterPRT
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_prt prt JOIN m2_transaction_journal t ON prt.prtsumber = t.tsumber AND prt.prtid =  t.tidtransaksi AND prt.prtnotransaksi = t.tnotransaksi SET t.tstatuslunas = prt.prtstatuslunas, t.ttgllunas = prt.prttgllunas WHERE " & updFilterPRT
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================

                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "VP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("vpstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
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

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG ====================================================================
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
    Public Function M4_VpUpdateStatusOld(ByVal param As String) As String

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
        Dim dtdetail As DataTable
        Dim isDelete As Boolean = False

        Dim pg1 As New RsPaging
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
            Filter = Filter.Replace("vpsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vpsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Vptgl, Vpnotransaksi, Vpstatus FROM M4_Vp WHERE Vpid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Vpstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_vp_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Vp_HistorySimpan("" & paramSplit(0) & "★M4_Vp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m4_vp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'VP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'Variabel ValidasiSimpan
                Dim ftOutstanding As String = "", updNilai As String = "", updNilaiValas As String = "", updFilter As String = "", sumberDetail As String = "", norek As String = ""
                Dim idtransaksiDetail As Integer = 0, idvppdetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0, matauangDetail As String = ""
                Dim Outstanding As Double = 0, OutstandingValas As Double = 0, tglLunas = "1900-01-01"

                'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT, CA
                'RI
                Dim updNilaiRI As String = "", updFilterRI As String = ""
                'AP
                Dim updNilaiAP As String = "", updNilaiValasAP As String = "", updFilterAP As String = ""
                'PRT
                Dim updNilaiPRT As String = "", updFilterPRT As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT sumber, idtransaksi, matauang, jmlbayar, jmlbayarvalas, rekhutangpiutang, idvppdetail, urutan FROM M4_vp_detail WHERE idvp = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    Dim MUFungsional As String = ""

                    'AMBIL MATA UANG FUNGSIONAL DARI SETTING
                    Dim dtSetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
                    If dtSetting.Rows.Count > 0 Then
                        MUFungsional = dtSetting.Rows(0)(0)
                    Else
                        result(2) = "Can't found 'Functional Currency' in Setting." : Trans.Rollback() : GoTo selesai
                    End If

                    For Each dr1 As DataRow In dtdetail.Rows
                        sumberDetail = dr1("sumber") : idtransaksiDetail = dr1("idtransaksi") : jmlbayar = dr1("jmlbayar")
                        jmlbayarvalas = dr1("jmlbayarvalas") : norek = dr1("rekhutangpiutang") : idvppdetail = dr1("idvppdetail")
                        matauangDetail = dr1("matauang")

                        If idvppdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "idvppdetail=" & idvppdetail)
                            OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "idvppdetail=" & idvppdetail)
                            updNilai = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvp - '" & Outstanding & "', 5) ", updNilai)
                            updNilaiValas = String.Concat("WHEN '" & idvppdetail & "' THEN ROUND(jmlvpvalas - '" & OutstandingValas & "', 5) ", updNilaiValas)

                            '2. SET FILTER UPDATE OUTSTANDING ---------
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idvppdetail = '" & idvppdetail & "')")
                        End If

                        'VALIDASI TRANSAKSI PEMBAYARAN ----------------
                        Select Case sumberDetail
                            Case "RI"
                                '1. CEK JML OUTSTANDING
                                If matauangDetail = MUFungsional Then
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                Else
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiRI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ri.rijmlbayar - '" & Outstanding & "', 5) ", updNilaiRI)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                            Case "AP"
                                '1. CEK JML OUTSTANDING
                                Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayar - '" & Outstanding & "', 5) ", updNilaiAP)
                                updNilaiValasAP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ap.apjumlahbayarvalas - '" & OutstandingValas & "', 5) ", updNilaiValasAP)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                                updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                            Case "PRT"
                                '1. CEK JML OUTSTANDING
                                If matauangDetail = MUFungsional Then
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                Else
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPRT = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(prt.prtjmlbayar - '" & Outstanding & "', 5) ", updNilaiPRT)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                                updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

                        End Select
                        'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI =======================================================
                If Len(updNilai) > 0 Then
                    'UPDATE DETAIL
                    sql = "UPDATE M4_vpp_detail SET jmlvp = (CASE idvppdetail " & updNilai & " ELSE jmlvp END), jmlvpvalas = (CASE idvppdetail " & updNilaiValas & " ELSE jmlvpvalas END) WHERE " & updFilter
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE UTAMA
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idvpp FROM M4_vpp_detail WHERE " & updFilter & " GROUP BY idvpp")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idvpp = '" & dr1("idvpp") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idvpp, SUM(jmlbayar) as jmlbayar, SUM(jmlvp) as jmlvp FROM M4_vpp_detail WHERE " & ftDetail & " GROUP BY idvpp")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlvp") >= dr1("jmlbayar") Then
                                statusOut = 2
                            ElseIf dr1("jmlvp") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idvpp") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(vppid = '" & dr1("idvpp") & "')")
                        Next

                        sql = "UPDATE M4_vpp SET vppstatusvp = (CASE vppid " & updNilai & " ELSE vppstatusvp END) WHERE " & updFilter
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
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================


                'UPDATE TRANSAKSI PEMBAYARAN ========================================================
                'RI
                If Len(updNilaiRI) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ri ri SET ri.rijmlbayar = (CASE ri.riid " & updNilaiRI & " ELSE ri.rijmlbayar END), ri.ritgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterRI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_ri ri JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE " & updFilterRI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'AP
                If Len(updNilaiAP) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ap ap SET ap.apjumlahbayar = (CASE ap.apid " & updNilaiAP & " ELSE ap.apjumlahbayar END), ap.apjumlahbayarvalas = (CASE ap.apid " & updNilaiValasAP & " ELSE ap.apjumlahbayarvalas END), ap.aptgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterAP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_ap ap JOIN m2_transaction_journal t ON ap.apsumber = t.tsumber AND ap.apid = t.tidtransaksi AND ap.apnotransaksi = t.tnotransaksi SET t.tstatuslunas = ap.apstatusbayar, t.ttgllunas = ap.aptgllunas WHERE " & updFilterAP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'PRT
                If Len(updNilaiPRT) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_prt prt SET prt.prtjmlbayar = (CASE prt.prtid " & updNilaiPRT & " ELSE prt.prtjmlbayar END), prt.prttgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterPRT
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_prt prt JOIN m2_transaction_journal t ON prt.prtsumber = t.tsumber AND prt.prtid = t.tidtransaksi AND prt.prtnotransaksi = t.tnotransaksi SET t.tstatuslunas = prt.prtstatuslunas, t.ttgllunas = prt.prttgllunas WHERE " & updFilterPRT
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'UPDATE TRANSAKSI PEMBAYARAN ========================================================


                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'VP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'VP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M4_Vp SET Vpstatus = " & nilaiStatus & ", Vpmodifikasiuser='" & userid & "', Vpmodifikasitgl = NOW(), Vpposting = 0, Vppostingtgl = '1971-01-01 00:00:00', Vpjmlrevisi = Vpjmlrevisi + 1 WHERE Vpid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_VpSearch(PostWsSearch(paramSplit(0), "M4_VpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_VpDeleteOld(ByVal param As String) As String

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
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("vpsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vpsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Vpid, Vpnotransaksi FROM M4_Vp WHERE Vpid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT vpcabang, vplokasi, vpsumber, vpautonotransaksi, vpnotransaksi, vptgl"
            sql &= " FROM M4_vp"
            sql &= " WHERE vpid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("vpcabang")
                lokasi = dtNomorNext.Rows(0)("vplokasi")
                sumber = dtNomorNext.Rows(0)("vpsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("vpautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("vpnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("vptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PAY
            sql = "DELETE FROM M4_Vp_Pay WHERE idvp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Vp_Detail WHERE idvp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Vp WHERE vpid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_VpSearch(PostWsSearch(paramSplit(0), "M4_VpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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