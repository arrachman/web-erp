Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_ppv
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_PpvSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String, dataPay() As String, dataRowPay() As String

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
        'ppvid(0) As Integer, ppvcabang(1) As String, ppvlokasi(2) As String, ppvgudang(3) As String, ppvsumber(4) As String, 
        'ppvautonotransaksi(5) As Integer, ppvnotransaksi(6) As String, ppvtgl(7) As Date, ppvkodepa(8) As Integer, ppvcustomer(9) As Integer, 
        'ppvcustomerkontak(10) As String, ppv1alamat1(11) As String, ppv1alamat2(12) As String, ppv1alamat3(13) As String, ppv2alamat1(14) As String, 
        'ppv2alamat2(15) As String, ppv2alamat3(16) As String, ppvbagianpenjualan(17) As Integer, ppvbagianterima(18) As Integer, ppvuraian(19) As String, 
        'ppvcatatan(20) As String, ppvnoref(21) As String, ppvtglnoref(22) As Date, ppvcarabayar(23) As Integer, ppvtglbayar(24) As Date, 
        'ppvmatauang(25) As String, ppvkurs(26) As Double, ppvtotalap(27) As Double, ppvtotalapvalas(28) As Double, ppvtotalar(29) As Double, 
        'ppvtotalarvalas(30) As Double, ppvbayar(31) As Double, ppvbayarvalas(32) As Double, ppvselisihkurs(33) As Double, ppvrekselisihkurs(34) As String, 
        'ppvdiskon(35) As Double, ppvdiskonvalas(36) As Double, ppvrekdiskon(37) As String, ppvstatus(38) As Integer, 
        'ppvstatussebelumnya(39) As Integer, ppvjmlrevisi(40) As Integer, ppvcetakanke(41) As Integer, ppvinputuser(42) As Integer, ppvinputtgl(43) As DateTime, 
        'ppvmodifikasiuser(44) As Integer, ppvmodifikasitgl(45) As DateTime, ppvisclose(46) As Integer, ppvcustomtext1(47) As String, ppvcustomtext2(48) As String, 
        'ppvcustomtext3(49) As String, ppvcustomtext4(50) As String, ppvcustomtext5(51) As String, ppvcustomint1(52) As Integer, ppvcustomint2(53) As Integer, 
        'ppvcustomint3(54) As Integer, ppvcustomdbl1(55) As Double, ppvcustomdbl2(56) As Double, ppvcustomdbl3(57) As Double, ppvcustomdate1(58) As Date, 
        'ppvcustomdate2(59) As Date, ppvcustomdate3(60) As Date, ppvdenda(61) As Double, ppvdendavalas(62) As Double, ppvrekdenda(63) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ppvid, ppvcabang, ppvlokasi, ppvgudang, ppvsumber, ppvautonotransaksi, ppvnotransaksi, ppvtgl
        'ppvkodepa, ppvcustomer, ppvcustomerkontak, ppv1alamat1, ppv1alamat2, ppv1alamat3, ppv2alamat1
        'ppv2alamat2, ppv2alamat3, ppvbagianpenjualan, ppvbagianterima, ppvuraian, ppvcatatan, ppvnoref
        'ppvtglnoref, ppvcarabayar, ppvtglbayar, ppvmatauang, ppvkurs, ppvtotalap, ppvtotalapvalas
        'ppvtotalar, ppvtotalarvalas, ppvbayar, ppvbayarvalas, ppvselisihkurs, ppvrekselisihkurs
        'ppvdiskon, ppvdiskonvalas, ppvrekdiskon, ppvstatus, ppvstatussebelumnya, ppvjmlrevisi, ppvcetakanke
        'ppvinputuser, ppvinputtgl, ppvmodifikasiuser, ppvmodifikasitgl, ppvisclose
        'ppvcustomtext1, ppvcustomtext2, ppvcustomtext3, ppvcustomtext4, ppvcustomtext5
        'ppvcustomint1, ppvcustomint2, ppvcustomint3, ppvcustomdbl1, ppvcustomdbl2, ppvcustomdbl3
        'ppvcustomdate1, ppvcustomdate2, ppvcustomdate3, ppvdenda, ppvdendavalas, ppvrekdenda


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 64) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================



        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pvid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pvid required numeric." : GoTo selesai
        End If
        'pvautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "pvautonotransaksi required numeric." : GoTo selesai
        End If
        'pvtgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "pvtgl required date." : GoTo selesai
        End If
        'pvkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pvkodepa required numeric." : GoTo selesai
        End If
        'pvcustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "pvcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "pvcustomer can't be empty." : GoTo selesai
        End If
        'pvbagianpenjualan(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "pvbagianpenjualan required numeric." : GoTo selesai
        End If
        'pvbagianterima(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pvbagianterima required numeric." : GoTo selesai
        End If
        'pvtglnoref(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "pvtglnoref required date." : GoTo selesai
        End If
        'pvcarabayar(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "pvcarabayar required numeric." : GoTo selesai
        End If
        'pvtglbayar(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "pvtglbayar required date." : GoTo selesai
        End If
        'pvkurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "pvkurs required numeric." : GoTo selesai
        End If
        'pvtotalap(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pvtotalap required numeric." : GoTo selesai
        End If
        'pvtotalapvalas(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pvtotalapvalas required numeric." : GoTo selesai
        End If
        'pvtotalar(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "pvtotalar required numeric." : GoTo selesai
        End If
        'pvtotalarvalas(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pvtotalarvalas required numeric." : GoTo selesai
        End If
        'pvbayar(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "pvbayar required numeric." : GoTo selesai
        End If
        'pvbayarvalas(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pvbayarvalas required numeric." : GoTo selesai
        End If
        'pvselisihkurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pvselisihkurs required numeric." : GoTo selesai
        End If
        'pvdiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pvdiskon required numeric." : GoTo selesai
        End If
        'pvdiskonvalas(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pvdiskonvalas required numeric." : GoTo selesai
        End If
        'pvstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pvstatus required numeric." : GoTo selesai
        End If
        'pvstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pvstatussebelumnya required numeric." : GoTo selesai
        End If
        'pvjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pvjmlrevisi required numeric." : GoTo selesai
        End If
        'pvcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pvcetakanke required numeric." : GoTo selesai
        End If
        'pvinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pvinputuser required numeric." : GoTo selesai
        End If
        'pvinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "pvinputtgl required date." : GoTo selesai
        End If
        'pvmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "pvmodifikasiuser required numeric." : GoTo selesai
        End If
        'pvmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "pvmodifikasitgl required date." : GoTo selesai
        End If
        'pvisclose(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "pvisclose required numeric." : GoTo selesai
        End If
        'pvcustomint1(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pvcustomint1 required numeric." : GoTo selesai
        End If
        'pvcustomint2(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pvcustomint2 required numeric." : GoTo selesai
        End If
        'pvcustomint3(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pvcustomint3 required numeric." : GoTo selesai
        End If
        'pvcustomdbl1(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pvcustomdbl1 required numeric." : GoTo selesai
        End If
        'pvcustomdbl2(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "pvcustomdbl2 required numeric." : GoTo selesai
        End If
        'pvcustomdbl3(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "pvcustomdbl3 required numeric." : GoTo selesai
        End If
        'pvcustomdate1(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "pvcustomdate1 required date." : GoTo selesai
        End If
        'pvcustomdate2(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "pvcustomdate2 required date." : GoTo selesai
        End If
        'pvcustomdate3(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "pvcustomdate3 required date." : GoTo selesai
        End If

        'ppvdenda(57) As Double
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "ppvdenda required numeric." : GoTo selesai
        End If
        'ppvdendavalas(57) As Double
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "ppvdendavalas required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'pvcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pvcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pvcabang should not be more than 25 character." : GoTo selesai
        End If

        'pvlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pvlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pvlokasi should not be more than 25 character." : GoTo selesai
        End If

        'pvsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "pvsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "pvsumber should not be more than 10 character." : GoTo selesai
        End If

        'pvnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pvnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "pvnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pvtgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pvtgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS TRANSAKSI
        tglLunas = AsFormatTanggal(dataUtama(7))

        'pvtglnoref(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "pvtglnoref can't be empty" : GoTo selesai
        End If

        'pvtglbayar(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "pvtglbayar can't be empty" : GoTo selesai
        End If

        'pvmatauang(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pvmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "pvmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pvkurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "pvkurs can't be empty" : GoTo selesai
        End If

        'pvtotalap(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "pvtotalap can't be empty" : GoTo selesai
        End If

        'pvtotalapvalas(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "pvtotalapvalas can't be empty" : GoTo selesai
        End If

        'pvtotalar(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "pvtotalar can't be empty" : GoTo selesai
        End If

        'pvtotalarvalas(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "pvtotalarvalas can't be empty" : GoTo selesai
        End If

        'pvbayar(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pvbayar can't be empty" : GoTo selesai
        End If

        'pvbayarvalas(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pvbayarvalas can't be empty" : GoTo selesai
        End If

        'pvselisihkurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "pvselisihkurs can't be empty" : GoTo selesai
        End If

        'pvdiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "pvdiskon can't be empty" : GoTo selesai
        End If

        'pvdiskonvalas(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pvdiskonvalas can't be empty" : GoTo selesai
        End If

        'pvinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pvinputtgl can't be empty" : GoTo selesai
        End If

        'pvmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "pvmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pvcustomdbl1(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "pvcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pvcustomdbl2(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "pvcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pvcustomdbl3(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "pvcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pvcustomdate1(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "pvcustomdate1 can't be empty" : GoTo selesai
        End If

        'pvcustomdate2(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "pvcustomdate2 can't be empty" : GoTo selesai
        End If

        'pvcustomdate3(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "pvcustomdate3 can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(61)) = 0 Then
            result(2) = "ppvdenda can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(62)) = 0 Then
            result(2) = "ppvdendavalas can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(63)) = 0 Then
            result(2) = "ppvrekdenda can't be empty" : GoTo selesai
        End If

        ''VALIDASI JUMLAH BAYAR
        ''JIKA TOTAL AR - DISKON TERMIN - TOTAL AP + SELISIH KURS <> 0 MAKA MUNCUL PERINGATAN
        ''               pvtotalar(29),           pvdiskon(35),                pvtotalap(27),            pvselisihkurs(33)
        'If Double.Parse(dataUtama(29)) - Double.Parse(dataUtama(35)) - Double.Parse(dataUtama(27)) + Double.Parse(dataUtama(33)) <> 0 Then
        '    Dim selisih(2) As String
        '    selisih = F_Nominal((Double.Parse(dataUtama(29)) - Double.Parse(dataUtama(35)) - Double.Parse(dataUtama(27)) + Double.Parse(dataUtama(33))), False).Split(sptSubParam)
        '    result(2) = "Total AR - Total AP must be balance : " & selisih(1) & "" : GoTo selesai
        'End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ppvid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppv1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppv1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppv1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppv2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppv2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppv2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvtglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvtotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvtotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvtotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvtotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvrekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvdiskonvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppvcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvdenda", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvdendavalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppvrekdenda", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "ppvid~ppvcabang~ppvlokasi~ppvgudang~ppvsumber~ppvautonotransaksi~ppvnotransaksi~ppvtgl~ppvkodepa~ppvcustomer~ppvcustomerkontak~ppv1alamat1~ppv1alamat2~ppv1alamat3~ppv2alamat1~ppv2alamat2~ppv2alamat3~ppvbagianpenjualan~ppvbagianterima~ppvuraian~ppvcatatan~ppvnoref~ppvtglnoref~ppvcarabayar~ppvtglbayar~ppvmatauang~ppvkurs~ppvtotalap~ppvtotalapvalas~ppvtotalar~ppvtotalarvalas~ppvbayar~ppvbayarvalas~ppvselisihkurs~ppvrekselisihkurs~ppvdiskon~ppvdiskonvalas~ppvrekdiskon~ppvstatus~ppvstatussebelumnya~ppvjmlrevisi~ppvcetakanke~ppvinputuser~ppvinputtgl~ppvmodifikasiuser~ppvmodifikasitgl~ppvisclose~ppvcustomtext1~ppvcustomtext2~ppvcustomtext3~ppvcustomtext4~ppvcustomtext5~ppvcustomint1~ppvcustomint2~ppvcustomint3~ppvcustomdbl1~ppvcustomdbl2~ppvcustomdbl3~ppvcustomdate1~ppvcustomdate2~ppvcustomdate3~ppvdenda~ppvdendavalas~ppvrekdenda", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idppvdetail(0) As Integer, idppv(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskon(11) As String, jmldiskon(12) As Double, jmldiskonvalas(13) As Double, nogiro(14) As String, 
        'rekhutangpiutang(15) As String, catatan(16) As String, costcenter(17) As String, divisi(18) As String, subdivisi(19) As String, 
        'proyek(20) As String, idicdetail(21) As Integer, urutan(22) As Integer, isclose(23) As Integer, customtext1(24) As String, 
        'customtext2(25) As String, customtext3(26) As String, customdbl1(27) As Double, customdbl2(28) As Double, customdbl3(29) As Double, 
        'customdate1(30) As Date, customdate2(31) As Date, customdate3(32) As Date, rencana(33) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idppvdetail, idppv, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskon, jmldiskon, jmldiskonvalas, 
        'nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, 
        'idicdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, rencana


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idppvdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idppv", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskonvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
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
        Dim idtransaksiDetail As Integer = 0, idicdetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0
        Dim Outstanding As Double = 0, OutstandingValas As Double = 0

        'VARIABEL CEK TRANSAKSI PEMBAYARAN --> SI, AS, SR, RP, IP, CA
        'SI
        Dim ftExistOutstandingSI As String = "", ftOutstandingSI As String = "", updNilaiSI As String = "", updNilaiSIValas As String = "", updFilterSI As String = "", updTglLunasSI As String = ""
        'IP
        Dim ftExistOutstandingIP As String = "", ftOutstandingIP As String = "", updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", updTglLunasIP As String = ""

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
            'idpvdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpvdetail required numeric." : GoTo selesai
            End If
            'idpv(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpv required numeric." : GoTo selesai
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
            'jmldiskon(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmldiskonvalas(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmldiskonvalas required numeric." : GoTo selesai
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
            If (dataRowDetail(2) <> "SI" And _
                dataRowDetail(2) <> "IP") Then
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

            'diskon(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            End If

            'jmldiskonvalas(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonvalas can't be empty" : GoTo selesai
            End If

            'rekhutangpiutang(15) As String
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
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

            If AsDataTableTambahData(dtdetail, "idppvdetail~idppv~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskon~jmldiskon~jmldiskonvalas~nogiro~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'sumber(2) As String            , idtransaksi(3) As Integer            , jmlbayar(9) As Double
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3) : jmlbayar = dataRowDetail(9)
            'jmlbayarvalas(10) As Double      , rekhutangpiutang(15) As String, idicdetail(21) As Integer
            jmlbayarvalas = dataRowDetail(10) : norek = dataRowDetail(15) ': idicdetail = dataRowDetail(21)
            'matauang(4) As String
            matauangDetail = dataRowDetail(4)


            'VALIDASI TRANSAKSI PEMBAYARAN ----------------
            Select Case sumberDetail
                Case "SI"
                    '1. CEK DATA EXIST
                    ftExistOutstandingSI = IIf(Len(ftExistOutstandingSI.ToString) = 0, "", ftExistOutstandingSI & " UNION ")
                    ftExistOutstandingSI = String.Concat(ftExistOutstandingSI, "SELECT EXISTS(SELECT 1 FROM m5_si_installment WHERE idsiinstallment = '" & idtransaksiDetail & "' AND (sistatus = 2 OR sistatus = 3 OR sistatus = 4 OR sistatus = 7) LIMIT 1) as rowExists, siid, sisumber, sinotransaksi, idsiinstallment, angsuranke FROM m5_si_installment sii join m5_si si on sii.idsi = si.siid WHERE sii.idsiinstallment = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    'If matauangDetail = MUFungsional Then
                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    'Else
                    OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    'End If

                    ftOutstandingSI = IIf(Len(ftOutstandingSI.ToString) = 0, "", ftOutstandingSI & " OR ")
                    If matauangDetail = MUFungsional Then
                        ftOutstandingSI = String.Concat(ftOutstandingSI, " (sii.idsiinstallment = '" & idtransaksiDetail & "' AND " & Math.Round(Outstanding, 2) & " > ROUND(sii.jumlah - sii.jumlahbayar,2)) ")
                    Else
                        ftOutstandingSI = String.Concat(ftOutstandingSI, " (sii.idsiinstallment = '" & idtransaksiDetail & "' AND " & Math.Round(OutstandingValas, 2) & " > ROUND(sii.jumlahvalas - sii.jumlahbayarvalas,2)) ")
                    End If


                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiSI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(sii.jumlahbayar + '" & Outstanding & "', 5) ", updNilaiSI)
                    updNilaiSIValas = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(sii.jumlahbayarvalas + '" & OutstandingValas & "', 5) ", updNilaiSIValas)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                    updFilterSI = String.Concat(updFilterSI, "(sii.idsiinstallment = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    If matauangDetail = MUFungsional Then
                        updTglLunasSI = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(sii.jumlahbayar + '" & Outstanding & "', 5) >= sii.jumlah THEN '" & FixQuotes(tglLunas) & "' ELSE sii.tgllunas END) ", updTglLunasSI)
                    Else
                        updTglLunasSI = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(sii.jumlahbayarvalas + '" & OutstandingValas & "', 5) >= sii.jumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE sii.tgllunas END) ", updTglLunasSI)
                    End If

                    'Dim rsValidasi2 As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, MUFungsional, ftExistOutstandingSI, ftOutstandingSI, updFilterSI, formatTgl, drutama("ppvtgl"))
                Case "IP"
                    '1. CEK DATA EXIST
                    ftExistOutstandingIP = IIf(Len(ftExistOutstandingIP.ToString) = 0, "", ftExistOutstandingIP & " UNION ")
                    ftExistOutstandingIP = String.Concat(ftExistOutstandingIP, "SELECT EXISTS(SELECT 1 FROM m5_ip WHERE ipid = '" & idtransaksiDetail & "' AND (ipstatus = 2 OR ipstatus = 3 OR ipstatus = 4 OR ipstatus = 7) LIMIT 1) as rowExists, ipid, ipsumber, ipnotransaksi FROM m5_ip WHERE ipid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    ftOutstandingIP = IIf(Len(ftOutstandingIP.ToString) = 0, "", ftOutstandingIP & " OR ")
                    ftOutstandingIP = String.Concat(ftOutstandingIP, " (ip.ipid = '" & idtransaksiDetail & "' AND (CASE ip.ipmatauang WHEN s.snilai THEN " & Math.Round(Outstanding, 2) & " > ROUND(ip.ipjumlah - ip.ipjumlahbayar,2) ELSE " & Math.Round(OutstandingValas, 2) & " > ROUND(ip.ipjumlahvalas - ip.ipjumlahbayarvalas,2) END)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiIP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ip.ipjumlahbayar + '" & Outstanding & "', 5) ", updNilaiIP)
                    updNilaiValasIP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) ", updNilaiValasIP)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                    updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    If matauangDetail = MUFungsional Then
                        updTglLunasIP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayar + '" & Outstanding & "', 5) >= ip.ipjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                    Else
                        updTglLunasIP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) >= ip.ipjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                    End If
            End Select
            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idppvcarabayar(0) As Integer, idppv(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idppvcarabayar, idppv, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idppvcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idppv", AsEnumTypeData.AsInt64)
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

                If (dataRowPay.Length <> 16) Then
                    result(2) = "Pay Row : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idvpcarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Pay Row : " & i & " - idppvcarabayar required numeric." : GoTo selesai
                End If

                'idvp(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Pay Row : " & i & " - idppv required numeric." : GoTo selesai
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
                'isclose(16) As Integer
                If (IsNumeric(dataRowPay(15)) = False) Then
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


                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idppvcarabayar~idppv~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15)) = False Then
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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 78
                Select Case drutama("ppvstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ppvtgl")), AsFormatTanggal(drutama("ppvtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================



                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("ppvstatus") = 2 Or drutama("ppvstatus") = 1 Or drutama("ppvstatus") = 8 Or drutama("ppvstatus") = 9 Or drutama("ppvstatus") = 10 Or drutama("ppvstatus") = 11 Then

                    'CEK JMLBAYAR TRANSAKSI ---------------------
                    Dim JmlSI As Double = 0
                    Dim JmlTabBayar As Double = 0
                    Dim JmlIP As Double = 0
                    Dim TotalAP As Double = 0, TotalAR As Double = 0

                    'TOTAL AR = RI + RP + COA
                    JmlSI = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'SI'") + Double.Parse(drutama("ppvdenda")) - Double.Parse(drutama("ppvdiskon"))
                    TotalAR = JmlSI

                    'TOTAL AP = IP + AS + SR
                    JmlTabBayar = AsDataTableDSum(dtpay, "jumlah")
                    JmlIP = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'IP'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'IP'")
                    TotalAP = JmlTabBayar + JmlIP - Double.Parse(drutama("ppvselisihkurs"))

                    'JIKA SELISIH TOTAL AP DAN TOTAL AP >= 0.1 MAKA ALERT TIDAK BISA DISIMPAN
                    If Math.Abs(TotalAR - TotalAP) >= 0.1 Then
                        Dim selisih(2) As String
                        selisih = F_Nominal(F_Round(Math.Abs(TotalAR - TotalAP)), False).Split(sptSubParam)
                        result(2) = "Total AR and Total AP are not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK JMLBAYAR TRANSAKSI --------------

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, MUFungsional, ftExistOutstandingSI, ftOutstandingSI, updFilterSI, formatTgl, drutama("ppvtgl"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("ppvid")
                    notransaksi = drutama("ppvnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(ppvid), ppvnotransaksi FROM M_12_Ppv WHERE ppvid='" & result(4) & "' AND ppvstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ppvid) FROM M_12_Ppv WHERE ppvnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============


                        sql = "Update M_12_Ppv set ppvcabang  = '" & FixQuotes(drutama("ppvcabang")) & "', ppvlokasi  = '" & FixQuotes(drutama("ppvlokasi")) & "', ppvgudang  = '" & FixQuotes(drutama("ppvgudang")) & "', ppvsumber  = '" & FixQuotes(drutama("ppvsumber")) & "', ppvautonotransaksi  = " & drutama("ppvautonotransaksi") & ", ppvnotransaksi  = '" & FixQuotes(notransaksi) & "', ppvtgl  = '" & FixQuotes(AsFormatTanggal(drutama("ppvtgl"))) & "', ppvkodepa  = " & drutama("ppvkodepa") & ", ppvcustomer  = " & drutama("ppvcustomer") & ", ppvcustomerkontak  = '" & FixQuotes(drutama("ppvcustomerkontak")) & "', ppv1alamat1  = '" & FixQuotes(drutama("ppv1alamat1")) & "', ppv1alamat2  = '" & FixQuotes(drutama("ppv1alamat2")) & "', ppv1alamat3  = '" & FixQuotes(drutama("ppv1alamat3")) & "', ppv2alamat1  = '" & FixQuotes(drutama("ppv2alamat1")) & "', ppv2alamat2  = '" & FixQuotes(drutama("ppv2alamat2")) & "', ppv2alamat3  = '" & FixQuotes(drutama("ppv2alamat3")) & "', ppvbagianpenjualan  = " & drutama("ppvbagianpenjualan") & ", ppvbagianterima  = " & drutama("ppvbagianterima") & ", ppvuraian  = '" & FixQuotes(drutama("ppvuraian")) & "', ppvcatatan  = '" & FixQuotes(drutama("ppvcatatan")) & "', ppvnoref  = '" & FixQuotes(drutama("ppvnoref")) & "', ppvtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ppvtglnoref"))) & "', ppvcarabayar  = " & drutama("ppvcarabayar") & ", ppvtglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("ppvtglbayar"))) & "', ppvmatauang  = '" & FixQuotes(drutama("ppvmatauang")) & "', ppvkurs  = '" & FixDouble(drutama("ppvkurs")) & "', ppvtotalap  = '" & FixDouble(drutama("ppvtotalap")) & "', ppvtotalapvalas  = '" & FixDouble(drutama("ppvtotalapvalas")) & "', ppvtotalar  = '" & FixDouble(drutama("ppvtotalar")) & "', ppvtotalarvalas  = '" & FixDouble(drutama("ppvtotalarvalas")) & "', ppvbayar  = '" & FixDouble(drutama("ppvbayar")) & "', ppvbayarvalas  = '" & FixDouble(drutama("ppvbayarvalas")) & "', ppvselisihkurs  = '" & FixDouble(drutama("ppvselisihkurs")) & "', ppvrekselisihkurs  = '" & FixQuotes(drutama("ppvrekselisihkurs")) & "', ppvdiskon  = '" & FixDouble(drutama("ppvdiskon")) & "', ppvdiskonvalas  = '" & FixDouble(drutama("ppvdiskonvalas")) & "', ppvrekdiskon  = '" & FixQuotes(drutama("ppvrekdiskon")) & "', ppvstatus  = " & drutama("ppvstatus") & ", ppvstatussebelumnya  = " & drutama("ppvstatussebelumnya") & ", ppvjmlrevisi  = ppvjmlrevisi+1, ppvcetakanke  = " & drutama("ppvcetakanke") & ", ppvmodifikasiuser  = " & drutama("ppvmodifikasiuser") & ", ppvmodifikasitgl  = NOW(), ppvcustomtext1  = '" & FixQuotes(drutama("ppvcustomtext1")) & "', ppvcustomtext2  = '" & FixQuotes(drutama("ppvcustomtext2")) & "', ppvcustomtext3  = '" & FixQuotes(drutama("ppvcustomtext3")) & "', ppvcustomtext4  = '" & FixQuotes(drutama("ppvcustomtext4")) & "', ppvcustomtext5  = '" & FixQuotes(drutama("ppvcustomtext5")) & "', ppvcustomint1  = " & drutama("ppvcustomint1") & ", ppvcustomint2  = " & drutama("ppvcustomint2") & ", ppvcustomint3  = " & drutama("ppvcustomint3") & ", ppvcustomdbl1  = '" & FixDouble(drutama("ppvcustomdbl1")) & "', ppvcustomdbl2  = '" & FixDouble(drutama("ppvcustomdbl2")) & "', ppvcustomdbl3  = '" & FixDouble(drutama("ppvcustomdbl3")) & "', ppvcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ppvcustomdate1"))) & "', ppvcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ppvcustomdate2"))) & "', ppvcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ppvcustomdate3"))) & "', ppvdenda  = '" & FixQuotes(drutama("ppvdenda")) & "', ppvdendavalas  = '" & FixQuotes(drutama("ppvdendavalas")) & "', ppvrekdenda  = '" & FixQuotes(drutama("ppvrekdenda")) & "' where ppvid = '" & drutama("ppvid") & "'"
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

                    If drutama("ppvautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ppvcabang"), drutama("ppvlokasi"), drutama("ppvsumber"), drutama("ppvtgl"))
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
                        notransaksi = drutama("ppvnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ppvid) FROM m_12_ppv WHERE ppvnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Ppv (ppvcabang, ppvlokasi, ppvgudang, ppvsumber, ppvautonotransaksi, ppvnotransaksi, ppvtgl, ppvkodepa, ppvcustomer, ppvcustomerkontak, ppv1alamat1, ppv1alamat2, ppv1alamat3, ppv2alamat1, ppv2alamat2, ppv2alamat3, ppvbagianpenjualan, ppvbagianterima, ppvuraian, ppvcatatan, ppvnoref, ppvtglnoref, ppvcarabayar, ppvtglbayar, ppvmatauang, ppvkurs, ppvtotalap, ppvtotalapvalas, ppvtotalar, ppvtotalarvalas, ppvbayar, ppvbayarvalas, ppvselisihkurs, ppvrekselisihkurs, ppvdiskon, ppvdiskonvalas, ppvrekdiskon, ppvstatus, ppvstatussebelumnya, ppvjmlrevisi, ppvcetakanke, ppvinputuser, ppvinputtgl, ppvmodifikasiuser, ppvmodifikasitgl, ppvisclose, ppvcustomtext1, ppvcustomtext2, ppvcustomtext3, ppvcustomtext4, ppvcustomtext5, ppvcustomint1, ppvcustomint2, ppvcustomint3, ppvcustomdbl1, ppvcustomdbl2, ppvcustomdbl3, ppvcustomdate1, ppvcustomdate2, ppvcustomdate3, ppvdenda, ppvdendavalas, ppvrekdenda) values('" & FixQuotes(drutama("ppvcabang")) & "', '" & FixQuotes(drutama("ppvlokasi")) & "', '" & FixQuotes(drutama("ppvgudang")) & "', '" & FixQuotes(drutama("ppvsumber")) & "', " & drutama("ppvautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppvtgl"))) & "', " & drutama("ppvkodepa") & ", " & drutama("ppvcustomer") & ", '" & FixQuotes(drutama("ppvcustomerkontak")) & "', '" & FixQuotes(drutama("ppv1alamat1")) & "', '" & FixQuotes(drutama("ppv1alamat2")) & "', '" & FixQuotes(drutama("ppv1alamat3")) & "', '" & FixQuotes(drutama("ppv2alamat1")) & "', '" & FixQuotes(drutama("ppv2alamat2")) & "', '" & FixQuotes(drutama("ppv2alamat3")) & "', " & drutama("ppvbagianpenjualan") & ", " & drutama("ppvbagianterima") & ", '" & FixQuotes(drutama("ppvuraian")) & "', '" & FixQuotes(drutama("ppvcatatan")) & "', '" & FixQuotes(drutama("ppvnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppvtglnoref"))) & "', " & drutama("ppvcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("ppvtglbayar"))) & "', '" & FixQuotes(drutama("ppvmatauang")) & "', '" & FixDouble(drutama("ppvkurs")) & "', '" & FixDouble(drutama("ppvtotalap")) & "', '" & FixDouble(drutama("ppvtotalapvalas")) & "', '" & FixDouble(drutama("ppvtotalar")) & "', '" & FixDouble(drutama("ppvtotalarvalas")) & "', '" & FixDouble(drutama("ppvbayar")) & "', '" & FixDouble(drutama("ppvbayarvalas")) & "', '" & FixDouble(drutama("ppvselisihkurs")) & "', '" & FixQuotes(drutama("ppvrekselisihkurs")) & "', '" & FixDouble(drutama("ppvdiskon")) & "', '" & FixDouble(drutama("ppvdiskonvalas")) & "', '" & FixQuotes(drutama("ppvrekdiskon")) & "', " & drutama("ppvstatus") & ", " & drutama("ppvstatussebelumnya") & ", " & drutama("ppvjmlrevisi") & ", " & drutama("ppvcetakanke") & ", " & drutama("ppvinputuser") & ", NOW(), " & drutama("ppvmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("ppvisclose") & ", '" & FixQuotes(drutama("ppvcustomtext1")) & "', '" & FixQuotes(drutama("ppvcustomtext2")) & "', '" & FixQuotes(drutama("ppvcustomtext3")) & "', '" & FixQuotes(drutama("ppvcustomtext4")) & "', '" & FixQuotes(drutama("ppvcustomtext5")) & "', " & drutama("ppvcustomint1") & ", " & drutama("ppvcustomint2") & ", " & drutama("ppvcustomint3") & ", '" & FixDouble(drutama("ppvcustomdbl1")) & "', '" & FixDouble(drutama("ppvcustomdbl2")) & "', '" & FixDouble(drutama("ppvcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppvcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppvcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppvcustomdate3"))) & "', '" & FixDouble(drutama("ppvdenda")) & "', '" & FixDouble(drutama("ppvdendavalas")) & "', '" & FixQuotes(drutama("ppvrekdenda")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select ppvid from M_12_ppv where ppvnotransaksi='" & notransaksi & "' AND ppvinputuser= '" & userid & "' order by ppvmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ppv_Detail where idppv = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idppvdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixDouble(dr1("jmldiskonvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_Ppv_Detail(idppvdetail, idppv, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskon, jmldiskon, jmldiskonvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M_12_PPv_Pay where idppv = '" & result(4) & "'"
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

                    For Each dr1 As DataRow In dtpay.Rows

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idppvcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                    Next
                    sql = "Insert into M_12_Ppv_Pay(idppvcarabayar, idppv, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                If drutama("ppvstatus") = 2 Then

                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                    'SI
                    If Len(updNilaiSI) > 0 Then
                        'TRANSAKSI INSTALLMENT
                        sql = "UPDATE m5_si_installment sii SET sii.jumlahbayar = (CASE sii.idsiinstallment " & updNilaiSI & " ELSE sii.jumlahbayar END), sii.jumlahbayarvalas = (CASE sii.idsiinstallment " & updNilaiSIValas & " ELSE sii.jumlahbayarvalas END), sii.tgllunas = (CASE sii.idsiinstallment " & updTglLunasSI & " ELSE sii.tgllunas END) WHERE " & updFilterSI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'TRANSAKSI UTAMA
                        Dim updSiUtama As String = "", updSiUtamaTgl As String = "", ftSiUtama As String = ""
                        Dim dtSiUtama As DataTable = AsDataTableAmbilDariDBCon("SELECT si.siid, sum(ppvd.jmlbayar) as bayar, sum(ppvd.jmlbayarvalas) as bayarvalas FROM m5_si si JOIN m5_si_installment sii ON si.siid = sii.idsi JOIN m_12_ppv_detail ppvd ON si.sisumber = ppvd.sumber AND sii.idsiinstallment = ppvd.idtransaksi WHERE ppvd.idppv = '" & result(4) & "' GROUP BY si.siid", myConn)
                        If dtSiUtama.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtSiUtama.Rows
                                'FILTER SI UTAMA
                                ftSiUtama = IIf(Len(ftSiUtama.ToString) = 0, "", ftSiUtama & " OR ")
                                ftSiUtama = String.Concat(ftSiUtama, "(siid = '" & FxDB(dr1("siid"), 0) & "')")

                                'NILAI UPDATE JMLBAYAR SI UTAMA
                                updSiUtama = String.Concat(updSiUtama, "WHEN siid = '" & FxDB(dr1("siid"), 0) & "' AND simatauang = '" & MUFungsional & "' THEN ROUND(sijmlbayar + " & FxDB(FixDouble(dr1("bayar")), 0) & ", 5) ")
                                updSiUtama = String.Concat(updSiUtama, "WHEN siid = '" & FxDB(dr1("siid"), 0) & "' AND simatauang <> '" & MUFungsional & "' THEN ROUND(sijmlbayar + " & FxDB(FixDouble(dr1("bayarvalas")), 0) & ", 5) ")

                                'NILAI UPDATE TGLLUNAS SI UTAMA
                                updSiUtamaTgl = String.Concat(updSiUtamaTgl, " WHEN siid = '" & FxDB(dr1("siid"), 0) & "' AND simatauang = '" & MUFungsional & "' THEN (CASE WHEN ROUND(sijmlbayar + " & FxDB(FixDouble(dr1("bayar")), 0) & ", 5) >= sitotaltransaksi + sicustomdbl7 THEN '" & FixQuotes(tglLunas) & "' ELSE sitgllunas END) ")
                                updSiUtamaTgl = String.Concat(updSiUtamaTgl, " WHEN siid = '" & FxDB(dr1("siid"), 0) & "' AND simatauang <> '" & MUFungsional & "' THEN (CASE WHEN ROUND(sijmlbayar + " & FxDB(FixDouble(dr1("bayarvalas")), 0) & ", 5) >= sitotaltransaksi + sicustomdbl7 THEN '" & FixQuotes(tglLunas) & "' ELSE sitgllunas END) ")
                            Next
                        End If

                        If Len(ftSiUtama) > 0 Then
                            'UPDATE SI UTAMA
                            sql = " UPDATE m5_si SET sijmlbayar = (CASE " & updSiUtama & " ELSE sijmlbayar END), sitgllunas = (CASE " & updSiUtamaTgl & " ELSE sitgllunas END) WHERE " & ftSiUtama
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'JURNAL
                            sql = "UPDATE m5_si si JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid =  t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE " & ftSiUtama
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

                    'IP
                    If Len(updNilaiIP) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m5_ip ip SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = (CASE ip.ipid " & updTglLunasIP & " ELSE ip.iptgllunas END) WHERE " & updFilterIP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_ip ip JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid =  t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
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


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PPV", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ppvstatus") = 2 Then
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
    Public Function M_12_PpvUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Ppv", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ppvtgl, Ppvnotransaksi, Ppvstatus FROM M_12_Ppv WHERE Ppvid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ppvstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m5_pv_history
            'Dim rsSimpanHistory As String = SimpanHistory.M5_Pv_HistorySimpan("" & paramSplit(0) & "★M5_Pv_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'Variabel ValidasiSimpan
                Dim ftOutstanding As String = "", updNilai As String = "", updNilaiValas As String = "", updFilter As String = "", sumberDetail As String = "", norek As String = ""
                Dim idtransaksiDetail As Integer = 0, idicdetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0, matauangDetail As String = ""

                Dim Outstanding As Double = 0, OutstandingValas As Double = 0, tglLunas = "1900-01-01"

                'VARIABEL CEK TRANSAKSI PEMBAYARAN --> SI
                'SI
                Dim updNilaiSI As String = "", updNilaiSIValas As String = "", updFilterSI As String = ""

                'AMBIL MATA UANG FUNGSIONAL DARI SETTING
                Dim MUFungsional As String = ""
                Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : Trans.Rollback() : GoTo selesai
                End If

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT sumber, idtransaksi, matauang, jmlbayar, jmlbayarvalas, rekhutangpiutang, urutan FROM m_12_ppv_detail WHERE idppv = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then

                    For Each dr1 As DataRow In dtdetail.Rows
                        sumberDetail = dr1("sumber") : idtransaksiDetail = dr1("idtransaksi") : jmlbayar = dr1("jmlbayar")
                        jmlbayarvalas = dr1("jmlbayarvalas") : norek = dr1("rekhutangpiutang")
                        matauangDetail = dr1("matauang")

                        'VALIDASI TRANSAKSI PEMBAYARAN ----------------
                        Select Case sumberDetail
                            Case "SI"
                                '1. CEK JML OUTSTANDING
                                'If matauangDetail = MUFungsional Then
                                Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                'Else
                                OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                'End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiSI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(sii.jumlahbayar - '" & Outstanding & "', 5) ", updNilaiSI)
                                updNilaiSIValas = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(sii.jumlahbayarvalas - '" & OutstandingValas & "', 5) ", updNilaiSIValas)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                                updFilterSI = String.Concat(updFilterSI, "(sii.idsiinstallment = '" & idtransaksiDetail & "')")
                            Case "IP"
                                '1. CEK DATA EXIST
                                'ftExistOutstandingIP = IIf(Len(ftExistOutstandingIP.ToString) = 0, "", ftExistOutstandingIP & " UNION ")
                                'ftExistOutstandingIP = String.Concat(ftExistOutstandingIP, "SELECT EXISTS(SELECT 1 FROM m5_ip WHERE ipid = '" & idtransaksiDetail & "' AND (ipstatus = 2 OR ipstatus = 3 OR ipstatus = 4 OR ipstatus = 7) LIMIT 1) as rowExists, ipid, ipsumber, ipnotransaksi FROM m5_ip WHERE ipid = '" & idtransaksiDetail & "'")

                                ''2. CEK JML OUTSTANDING
                                'Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                'OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                'ftOutstandingIP = IIf(Len(ftOutstandingIP.ToString) = 0, "", ftOutstandingIP & " OR ")
                                'ftOutstandingIP = String.Concat(ftOutstandingIP, " (ip.ipid = '" & idtransaksiDetail & "' AND (CASE ip.ipmatauang WHEN s.snilai THEN " & Math.Round(Outstanding, 2) & " > ROUND(ip.ipjumlah - ip.ipjumlahbayar,2) ELSE " & Math.Round(OutstandingValas, 2) & " > ROUND(ip.ipjumlahvalas - ip.ipjumlahbayarvalas,2) END)) ")

                                ''3. SET NILAI UPDATE OUTSTANDING
                                'updNilaiIP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ip.ipjumlahbayar + '" & Outstanding & "', 5) ", updNilaiIP)
                                'updNilaiValasIP = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) ", updNilaiValasIP)

                                ''4. SET FILTER UPDATE OUTSTANDING
                                'updFilterIP = IIf(Len(updFilterIP.ToString) = 0, "", updFilterIP & " OR ")
                                'updFilterIP = String.Concat(updFilterIP, "(ip.ipid = '" & idtransaksiDetail & "')")

                                ''5. SET NILAI TGLLUNAS TRANSAKSI
                                'If matauangDetail = MUFungsional Then
                                '    updTglLunasIP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayar + '" & Outstanding & "', 5) >= ip.ipjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                                'Else
                                '    updTglLunasIP = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ROUND(ip.ipjumlahbayarvalas + '" & OutstandingValas & "', 5) >= ip.ipjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE ip.iptgllunas END) ", updTglLunasIP)
                                'End If

                        End Select
                        'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'UPDATE TRANSAKSI PEMBAYARAN ========================================================
                'SI
                If Len(updNilaiSI) > 0 Then
                    'TRANSAKSI DETAIL INSTALLMENT
                    sql = "UPDATE m5_si_installment sii SET sii.jumlahbayar = (CASE sii.idsiinstallment " & updNilaiSI & " ELSE sii.jumlahbayar END), sii.jumlahbayarvalas = (CASE sii.idsiinstallment " & updNilaiSIValas & " ELSE sii.jumlahbayarvalas END), sii.tgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterSI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'TRANSAKSI UTAMA
                    Dim updSiUtama As String = "", ftSiUtama As String = ""
                    Dim dtSiUtama As DataTable = AsDataTableAmbilDariDBCon("SELECT si.siid, sum(ppvd.jmlbayar) as bayar, sum(ppvd.jmlbayarvalas) as bayarvalas FROM m5_si si JOIN m5_si_installment sii ON si.siid = sii.idsi JOIN m_12_ppv_detail ppvd ON si.sisumber = ppvd.sumber AND sii.idsiinstallment = ppvd.idtransaksi WHERE ppvd.idppv = '" & idtransaksi & "' GROUP BY si.siid", myConn)
                    If dtSiUtama.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtSiUtama.Rows
                            'FILTER SI UTAMA
                            ftSiUtama = IIf(Len(ftSiUtama.ToString) = 0, "", ftSiUtama & " OR ")
                            ftSiUtama = String.Concat(ftSiUtama, "(siid = '" & FxDB(dr1("siid"), 0) & "')")

                            'NILAI UPDATE JMLBAYAR SI UTAMA
                            updSiUtama = String.Concat(updSiUtama, "WHEN siid = '" & FxDB(dr1("siid"), 0) & "' AND simatauang = '" & MUFungsional & "' THEN ROUND(sijmlbayar - " & FxDB(FixDouble(dr1("bayar")), 0) & ", 5) ")
                            updSiUtama = String.Concat(updSiUtama, "WHEN siid = '" & FxDB(dr1("siid"), 0) & "' AND simatauang <> '" & MUFungsional & "' THEN ROUND(sijmlbayar - " & FxDB(FixDouble(dr1("bayarvalas")), 0) & ", 5) ")
                        Next
                    End If

                    If Len(ftSiUtama) > 0 Then
                        'UPDATE SI UTAMA
                        sql = " UPDATE m5_si SET sijmlbayar = (CASE " & updSiUtama & " ELSE sijmlbayar END) WHERE " & ftSiUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_si si JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid =  t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE " & ftSiUtama
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
                'UPDATE TRANSAKSI PEMBAYARAN ========================================================


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PPV' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M_12_Ppv SET Ppvstatus = " & nilaiStatus & ", Ppvmodifikasiuser='" & userid & "', Ppvmodifikasitgl = NOW(), Ppvposting = 0, Ppvpostingtgl = '1971-01-01 00:00:00', Ppvjmlrevisi = Ppvjmlrevisi + 1 WHERE Ppvid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_PpvSearch(PostWsSearch(paramSplit(0), "M12_PpvSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M12_PpvDelete(ByVal param As String) As String

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
            Dim sumber As String = "Ppv", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pvid, Pvnotransaksi FROM M5_Pv WHERE Pvid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ppvcabang, ppvlokasi, ppvsumber, ppvautonotransaksi, ppvnotransaksi, ppvtgl"
            sql &= " FROM M_12_ppv"
            sql &= " WHERE ppvid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ppvcabang")
                lokasi = dtNomorNext.Rows(0)("ppvlokasi")
                sumber = dtNomorNext.Rows(0)("ppvsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ppvautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ppvnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ppvtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Ppv_Detail WHERE idppv='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Ppv WHERE ppvid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_PpvSearch(PostWsSearch(paramSplit(0), "M12_PpvSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_PpvSearch(ByVal param As String) As String
        'M12_PpvSearch --------------------------------------------------------
        'ppvid, ppvcabang, ppvlokasi, ppvgudang, ppvsumber, ppvautonotransaksi, ppvnotransaksi, 
        'ppvtgl, ppvkodepa, ppvcustomer, ppvcustomerkontak, ppv1alamat1, ppv1alamat2, ppv1alamat3, 
        'ppv2alamat1, ppv2alamat2, ppv2alamat3, ppvbagianpenjualan, ppvbagianterima, ppvuraian, ppvcatatan, 
        'ppvnoref, ppvtglnoref, ppvcarabayar, ppvtglbayar, ppvmatauang, ppvkurs, ppvtotalap, 
        'ppvtotalapvalas, ppvtotalar, ppvtotalarvalas, ppvbayar, ppvbayarvalas, ppvselisihkurs, ppvrekselisihkurs, 
        'ppvdiskon, ppvdiskonvalas, ppvrekdiskon, ppvstatus, ppvstatussebelumnya, ppvjmlrevisi, 
        'ppvcetakanke, ppvinputuser, ppvinputtgl, ppvmodifikasiuser, ppvmodifikasitgl, ppvposting, ppvpostingtgl, 
        'ppvisclose, ppvcabangnama, ppvlokasinama, ppvgudangnama, ppvcustomerkode, ppvcustomernama, ppvbagianpenjualankode, 
        'ppvbagianpenjualannama, ppvbagianterimakode, ppvbagianterimanama, ppvcarabayarnama, ppvrekselisihkursnama, ppvrekdiskonnama, icnotransaksi, 
        'ppvstatusnama, ppvstatussebelumnyanama, ppvinputusernama, ppvmodifikasiusernama

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `ppv`.`ppvid` AS `ppvid`,`ppv`.`ppvcabang` AS `ppvcabang`,`ppv`.`ppvlokasi` AS `ppvlokasi`,`ppv`.`ppvgudang` AS `ppvgudang`,`ppv`.`ppvsumber` AS `ppvsumber`,`ppv`.`ppvautonotransaksi` AS `ppvautonotransaksi`,`ppv`.`ppvnotransaksi` AS `ppvnotransaksi`,`ppv`.`ppvtgl` AS `ppvtgl`,`ppv`.`ppvkodepa` AS `ppvkodepa`,`ppv`.`ppvcustomer` AS `ppvcustomer`,`ppv`.`ppvcustomerkontak` AS `ppvcustomerkontak`,`ppv`.`ppv1alamat1` AS `ppv1alamat1`,`ppv`.`ppv1alamat2` AS `ppv1alamat2`,`ppv`.`ppv1alamat3` AS `ppv1alamat3`,`ppv`.`ppv2alamat1` AS `ppv2alamat1`,`ppv`.`ppv2alamat2` AS `ppv2alamat2`,`ppv`.`ppv2alamat3` AS `ppv2alamat3`,`ppv`.`ppvbagianpenjualan` AS `ppvbagianpenjualan`,`ppv`.`ppvbagianterima` AS `ppvbagianterima`,`ppv`.`ppvuraian` AS `ppvuraian`,`ppv`.`ppvcatatan` AS `ppvcatatan`,`ppv`.`ppvnoref` AS `ppvnoref`,`ppv`.`ppvtglnoref` AS `ppvtglnoref`,`ppv`.`ppvcarabayar` AS `ppvcarabayar`,`ppv`.`ppvtglbayar` AS `ppvtglbayar`,`ppv`.`ppvmatauang` AS `ppvmatauang`,`ppv`.`ppvkurs` AS `ppvkurs`,`ppv`.`ppvtotalap` AS `ppvtotalap`,`ppv`.`ppvtotalapvalas` AS `ppvtotalapvalas`,`ppv`.`ppvtotalar` AS `ppvtotalar`,`ppv`.`ppvtotalarvalas` AS `ppvtotalarvalas`,`ppv`.`ppvbayar` AS `ppvbayar`,`ppv`.`ppvbayarvalas` AS `ppvbayarvalas`,`ppv`.`ppvselisihkurs` AS `ppvselisihkurs`,`ppv`.`ppvrekselisihkurs` AS `ppvrekselisihkurs`,`ppv`.`ppvdiskon` AS `ppvdiskon`,`ppv`.`ppvdiskonvalas` AS `ppvdiskonvalas`,`ppv`.`ppvrekdiskon` AS `ppvrekdiskon`,`ppv`.`ppvstatus` AS `ppvstatus`,`ppv`.`ppvstatussebelumnya` AS `ppvstatussebelumnya`,`ppv`.`ppvjmlrevisi` AS `ppvjmlrevisi`,`ppv`.`ppvcetakanke` AS `ppvcetakanke`,`ppv`.`ppvinputuser` AS `ppvinputuser`,`ppv`.`ppvinputtgl` AS `ppvinputtgl`,`ppv`.`ppvmodifikasiuser` AS `ppvmodifikasiuser`,`ppv`.`ppvmodifikasitgl` AS `ppvmodifikasitgl`,`ppv`.`ppvposting` AS `ppvposting`,`ppv`.`ppvpostingtgl` AS `ppvpostingtgl`,`ppv`.`ppvisclose` AS `ppvisclose`,`br`.`bnama` AS `ppvcabangnama`,`lc`.`lnama` AS `ppvlokasinama`,`wh`.`wnama` AS `ppvgudangnama`,`c1`.`kkode` AS `ppvcustomerkode`,`c1`.`knama` AS `ppvcustomernama`,`c2`.`kkode` AS `ppvbagianpenjualankode`,`c2`.`knama` AS `ppvbagianpenjualannama`,`c3`.`kkode` AS `ppvbagianterimakode`,`c3`.`knama` AS `ppvbagianterimanama`,`pm`.`nama` AS `ppvcarabayarnama`,`coa1`.`cnama` AS `ppvrekselisihkursnama`,`coa2`.`cnama` AS `ppvrekdiskonnama`,`st1`.`nama` AS `ppvstatusnama`,`st2`.`nama` AS `ppvstatussebelumnyanama`,`u1`.`unama` AS `ppvinputusernama`,`u2`.`unama` AS `ppvmodifikasiusernama`,`ppv`.`ppvdenda` AS `ppvdenda`,`ppv`.`ppvdendavalas` AS `ppvdendavalas`,`ppv`.`ppvrekdenda` AS `ppvrekdenda`from ((((((((((((((`m_12_ppv` `ppv` left join `m1_branch` `br` on((`br`.`bkode` = `ppv`.`ppvcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `ppv`.`ppvlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `ppv`.`ppvgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `ppv`.`ppvcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `ppv`.`ppvbagianpenjualan`))) left join `m1_contact` `c3` on((`c3`.`kid` = `ppv`.`ppvbagianterima`))) left join `m0_payment_method` `pm` on((`ppv`.`ppvcarabayar` = `pm`.`kode`))) left join `m1_coa` `coa1` on((`ppv`.`ppvrekselisihkurs` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`ppv`.`ppvrekdiskon` = `coa2`.`cnomor`)))) left join `m0_status` `st1` on((`st1`.`kode` = `ppv`.`ppvstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `ppv`.`ppvstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `ppv`.`ppvinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `ppv`.`ppvmodifikasiuser`)))"

        dt = AmbilData("aplikasi1-M12_ppv_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ppvid"), 0), sptField,
                     FxDB(dr("ppvcabang"), ""), sptField,
                     FxDB(dr("ppvlokasi"), ""), sptField,
                     FxDB(dr("ppvgudang"), ""), sptField,
                     FxDB(dr("ppvsumber"), ""), sptField,
                     FxDB(dr("ppvautonotransaksi"), 0), sptField,
                     FxDB(dr("ppvnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ppvtgl"), ""), formatTgl), sptField,
                     FxDB(dr("ppvkodepa"), 0), sptField,
                     FxDB(dr("ppvcustomer"), 0), sptField,
                     FxDB(dr("ppvcustomerkontak"), ""), sptField,
                     FxDB(dr("ppv1alamat1"), ""), sptField,
                     FxDB(dr("ppv1alamat2"), ""), sptField,
                     FxDB(dr("ppv1alamat3"), ""), sptField,
                     FxDB(dr("ppv2alamat1"), ""), sptField,
                     FxDB(dr("ppv2alamat2"), ""), sptField,
                     FxDB(dr("ppv2alamat3"), ""), sptField,
                     FxDB(dr("ppvbagianpenjualan"), 0), sptField,
                     FxDB(dr("ppvbagianterima"), 0), sptField,
                     FxDB(dr("ppvuraian"), ""), sptField,
                     FxDB(dr("ppvcatatan"), ""), sptField,
                     FxDB(dr("ppvnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ppvtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("ppvcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppvtglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("ppvmatauang"), ""), sptField,
                     FxDB(dr("ppvkurs"), 0), sptField,
                     FxDB(dr("ppvtotalap"), 0), sptField,
                     FxDB(dr("ppvtotalapvalas"), 0), sptField,
                     FxDB(dr("ppvtotalar"), 0), sptField,
                     FxDB(dr("ppvtotalarvalas"), 0), sptField,
                     FxDB(dr("ppvbayar"), 0), sptField,
                     FxDB(dr("ppvbayarvalas"), 0), sptField,
                     FxDB(dr("ppvselisihkurs"), 0), sptField,
                     FxDB(dr("ppvrekselisihkurs"), ""), sptField,
                     FxDB(dr("ppvdiskon"), 0), sptField,
                     FxDB(dr("ppvdiskonvalas"), 0), sptField,
                     FxDB(dr("ppvrekdiskon"), ""), sptField,
                     FxDB(dr("ppvstatus"), 0), sptField,
                     FxDB(dr("ppvstatussebelumnya"), 0), sptField,
                     FxDB(dr("ppvjmlrevisi"), 0), sptField,
                     FxDB(dr("ppvcetakanke"), 0), sptField,
                     FxDB(dr("ppvinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppvinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppvmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppvmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppvposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppvpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppvisclose"), 0), sptField,
                     FxDB(dr("ppvcabangnama"), ""), sptField,
                     FxDB(dr("ppvlokasinama"), ""), sptField,
                     FxDB(dr("ppvgudangnama"), ""), sptField,
                     FxDB(dr("ppvcustomerkode"), ""), sptField,
                     FxDB(dr("ppvcustomernama"), ""), sptField,
                     FxDB(dr("ppvbagianpenjualankode"), ""), sptField,
                     FxDB(dr("ppvbagianpenjualannama"), ""), sptField,
                     FxDB(dr("ppvbagianterimakode"), ""), sptField,
                     FxDB(dr("ppvbagianterimanama"), ""), sptField,
                     FxDB(dr("ppvcarabayarnama"), ""), sptField,
                     FxDB(dr("ppvrekselisihkursnama"), ""), sptField,
                     FxDB(dr("ppvrekdiskonnama"), ""), sptField,
                     FxDB(dr("ppvstatusnama"), ""), sptField,
                     FxDB(dr("ppvstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ppvinputusernama"), ""), sptField,
                     FxDB(dr("ppvmodifikasiusernama"), ""), sptField,
                     FxDB(dr("ppvdenda"), 0), sptField,
                     FxDB(dr("ppvdendavalas"), 0), sptField,
                     FxDB(dr("ppvrekdenda"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppvid, ppvcabang, ppvlokasi, ppvgudang, ppvsumber, ppvautonotransaksi, ppvnotransaksi, ppvtgl, ppvkodepa, ppvcustomer, ppvcustomerkontak, ppv1alamat1, ppv1alamat2, ppv1alamat3, ppv2alamat1, ppv2alamat2, ppv2alamat3, ppvbagianpenjualan, ppvbagianterima, ppvuraian, ppvcatatan, ppvnoref, ppvtglnoref, ppvcarabayar, ppvtglbayar, ppvmatauang, ppvkurs, ppvtotalap, ppvtotalapvalas, ppvtotalar, ppvtotalarvalas, ppvbayar, ppvbayarvalas, ppvselisihkurs, ppvrekselisihkurs, ppvdiskon, ppvdiskonvalas, ppvrekdiskon, ppvstatus, ppvstatussebelumnya, ppvjmlrevisi, ppvcetakanke, ppvinputuser, ppvinputtgl, ppvmodifikasiuser, ppvmodifikasitgl, ppvposting, ppvpostingtgl, ppvisclose, ppvcabangnama, ppvlokasinama, ppvgudangnama, ppvcustomerkode, ppvcustomernama, ppvbagianpenjualankode, ppvbagianpenjualannama, ppvbagianterimakode, ppvbagianterimanama, ppvcarabayarnama, ppvrekselisihkursnama, ppvrekdiskonnama, ppvstatusnama, ppvstatussebelumnyanama, ppvinputusernama, ppvmodifikasiusernama, ppvdenda, ppvdendavalas, ppvrekdenda"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PvTerkait(ByVal param As String) As String
        'M5_PvTerkait --------------------------------------------------------
        'pvid, pvnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        sql = query.PanggilQuery("m5_pv_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pvid"), 0), sptField,
                     FxDB(dr("pvnotransaksi"), ""), sptField,
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
            result(2) = "Related PV data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pvid, pvnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal MUFungsional As String, _
                                    ByVal ftExistOutstandingSI As String, ByVal ftOutstandingSI As String, _
                                    ByVal updFilterSI As String, _
                                    ByVal formatTgl As String, ByVal tglPembayaran As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, sumber As String = "", notransaksi As String = "", matauang As String = "", tgl As String = "", angsuranke As String = ""
        Dim filterLookup As String = "", urutan As String = "", sisa As Double = 0



        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'SI
        If Len(ftExistOutstandingSI) > 0 Then 'ftExistOutstanding = rowExists, siid, sisumber, sinotransaksi, idsiinstallment, angsuranke
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingSI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("sinotransaksi")
                sumber = dtval.Rows(0)("sisumber")

                filterLookup = "sumber = '" & dtval.Rows(0)("sumber") & "' AND idtransaksi = '" & dtval.Rows(0)("idsiinstallment") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " - " & dtval.Rows(0)("angsuranke") & " doesn't exists/yet approved in SI" : GoTo selesai
            End If
        End If

        'CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        If Len(updFilterSI) > 0 Then
            sql = "SELECT si.siid, si.sisumber, si.sitgl, si.sinotransaksi, sii.idsiinstallment FROM m5_si si join m5_si_installment sii on sii.idsi = si.siid WHERE si.sitgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterSI & ")"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("sisumber")
                notransaksi = dtval.Rows(0)("sinotransaksi")
                tgl = dtval.Rows(0)("sitgl")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("idsiinstallment") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingSI) > 0 Then
            sql = "SELECT sii.idsiinstallment, si.sisumber, si.sinotransaksi, si.simatauang, sii.angsuranke, sii.jumlah - sii.jumlahbayar as sisisatransaksi FROM m5_si_installment sii join m5_si si on si.siid = sii.idsi LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingSI
            'errmessage = sql : GoTo selesai
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("sinotransaksi")
                sumber = dtval.Rows(0)("sisumber")
                sisa = dtval.Rows(0)("sisisatransaksi")
                matauang = dtval.Rows(0)("simatauang")
                angsuranke = dtval.Rows(0)("angsuranke")

                filterLookup = "sumber = '" & dtval.Rows(0)("sisumber") & "' AND idtransaksi = '" & dtval.Rows(0)("idsiinstallment") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "" & sumber & " : " & notransaksi & " installment '" & angsuranke & "' exceeds the amount of payment in SI, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------



selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M12_PpvTakedataSearch(ByVal param As String) As String
        'M12_PpvTakedataSearch --------------------------------------------------------
        'idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, 
        'termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, rencana, 
        'sisa, sisavalas, statuslunas, rekhutangpiutang, inputtgl, sinoref

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

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m5_ic_takedata(Filter)
        Dim filter1 As String, filter2 As String

        filter1 = Filter
        filter1 = filter1.Replace("idtransaksi", "SELECT sii.idsiinstallment")
        filter1 = filter1.Replace("sumber", "si.sisumber")
        filter1 = filter1.Replace("notransaksi", "si.sinotransaksi")
        filter1 = filter1.Replace("kontak", "si.sicustomer")
        filter1 = filter1.Replace("tgl", "si.sitgl")
        filter1 = filter1.Replace("matauang", "si.simatauang")
        filter1 = filter1.Replace("statuslunas", "sii.statuslunas")
        filter1 = filter1.Replace("tgljatuhtempo", "sii.tgljt")
        filter1 = filter1.Replace("uraian", "si.siuraian")

        filter2 = Filter
        filter2 = filter2.Replace("idtransaksi", "ip.ipid")
        filter2 = filter2.Replace("sumber", "ip.ipsumber")
        filter2 = filter2.Replace("notransaksi", "ip.ipnotransaksi")
        filter2 = filter2.Replace("kontak", "ip.ipkontak")
        filter2 = filter2.Replace("tgl", "ip.iptgl")
        filter2 = filter2.Replace("matauang", "ip.ipmatauang")
        filter2 = filter2.Replace("statuslunas", "ip.ipstatusbayar")
        filter2 = filter2.Replace("tanggaljatuhtempo", "ip.iptgljatuhtempo")
        filter2 = filter2.Replace("uraian", "ip.ipuraian")

        sql = "(SELECT sii.idsiinstallment AS idtransaksi, si.sisumber AS sumber, si.sinotransaksi AS notransaksi, si.sitgl AS tgl, si.sicustomer AS kontak, sii.catatan AS catatan, sii.tgljt AS tgljatuhtempo, sii.matauang AS matauang, sii.kurs AS kurs,sii.jumlah AS totaltransaksi, sii.jumlahbayar AS terbayar, sii.jumlah-sii.jumlahbayar AS rencana, sii.jumlah-sii.jumlahbayar AS sisa, sii.jumlahvalas - sii.jumlahbayarvalas AS sisavalas, sii.statuslunas AS statuslunas, sii.rekpiutang AS rekhutangpiutang, si.siinputtgl AS inputtgl, sii.angsuranke as angsuranke, si.sinoref as noref FROM m5_si_installment sii JOIN m5_si si ON si.siid = sii.idsi  where" & filter1 & ")"
        'IP
        sql &= " UNION ALL"
        sql &= "(select `ip`.`ipid` AS `idtransaksi`,`ip`.`ipsumber` AS `sumber`,`ip`.`ipnotransaksi` AS `notransaksi`,`ip`.`iptgl` AS `tgl`,`ip`.`ipkontak` AS `kontak`,`ip`.`ipuraian` AS `catatan`,`ip`.`iptgljatuhtempo` AS `tgljatuhtempo`,`ip`.`ipmatauang` AS `matauang`,`ip`.`ipkurs` AS `kurs`,(case `ip`.`ipmatauang` when `s2`.`snilai` then `ip`.`ipjumlah` else `ip`.`ipjumlahvalas` end) AS `totaltransaksi`,(case `ip`.`ipmatauang` when `s2`.`snilai` then `ip`.`ipjumlahbayar` else `ip`.`ipjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `ip`.`ipkurs`) AS `rencana`,(`ip`.`ipjumlah` - `ip`.`ipjumlahbayar`) AS `sisa`,(case `ip`.`ipmatauang` when `s2`.`snilai` then 0 else (`ip`.`ipjumlahvalas` - `ip`.`ipjumlahbayarvalas`) end) AS `sisavalas`,`ip`.`ipstatusbayar` AS `statuslunas`,`ip`.`ipnorek` AS `rekhutangpiutang`,`ip`.`ipinputtgl` AS `inputtgl`, 0 as angsuranke, '' as noref from (((`m5_ip` `ip` left join `m1_terms` `tr` on((`ip`.`iptermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'ip') and (`icd`.`idtransaksi` = `ip`.`ipid`) and (`icd`.`statuspv` <> 2)))) Where " & filter2 & " group by `ip`.`ipid` order by `ip`.`iptgl`, `ip.ipid` desc)"
        'result(2) = sql : GoTo selesai
        dt = AmbilData("aplikasi1-M5_Ic_Takedata", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("sisavalas"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("angsuranke"), ""), sptField,
                     FxDB(dr("noref"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, sumber, notransaksi, tgl, kontak, catatan, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, sisavalas, statuslunas, rekhutangpiutang, inputtgl, angsuranke, noref"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M12_PpvGetdataById(ByVal param As String) As String
        'M5_PvGetdataById Utama --------------------------------------------------------
        'ppvid, ppvcabang, ppvlokasi, ppvgudang, ppvsumber, ppvautonotransaksi, ppvnotransaksi, 
        'ppvtgl, ppvkodepa, ppvcustomer, ppvcustomerkontak, ppv1alamat1, ppv1alamat2, ppv1alamat3, 
        'ppv2alamat1, ppv2alamat2, ppv2alamat3, ppvbagianpenjualan, ppvbagianterima, ppvuraian, ppvcatatan, 
        'ppvnoref, ppvtglnoref, ppvcarabayar, ppvtglbayar, ppvmatauang, ppvkurs, ppvtotalap, 
        'ppvtotalapvalas, ppvtotalar, ppvtotalarvalas, ppvbayar, ppvbayarvalas, ppvselisihkurs, ppvrekselisihkurs, 
        'ppvdiskon, ppvdiskonvalas, ppvrekdiskon, ppvstatus, ppvstatussebelumnya, ppvjmlrevisi, 
        'ppvcetakanke, ppvinputuser, ppvinputtgl, ppvmodifikasiuser, ppvmodifikasitgl, ppvposting, ppvpostingtgl, 
        'ppvisclose, ppvcustomtext1, ppvcustomtext2, ppvcustomtext3, ppvcustomtext4, ppvcustomtext5, ppvcustomint1, 
        'ppvcustomint2, ppvcustomint3, ppvcustomdbl1, ppvcustomdbl2, ppvcustomdbl3, ppvcustomdate1, ppvcustomdate2, 
        'ppvcustomdate3, ppvcabangnama, ppvlokasinama, ppvgudangnama, ppvcustomerkode, ppvcustomernama, ppvbagianpenjualankode, 
        'ppvbagianpenjualannama, ppvbagianterimakode, ppvbagianterimanama, ppvcarabayarnama, ppvrekselisihkursnama, ppvrekdiskonnama, 
        'ppvstatusnama, ppvstatussebelumnyanama, ppvinputusernama, ppvmodifikasiusernama, kpkp, ppvdenda, ppvdendavalas, ppvrekdenda, ppvrekdendanama

        'M5_PvGetdataById Detail --------------------------------------------------------
        'idppvdetail, idppv, sumber, idtransaksi, matauang, 
        'kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskon, 
        'jmldiskon, jmldiskonvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, 
        'subdivisi, proyek, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, 
        'rekhutangpiutangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, tgljtgiro, inputtgl

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

        Dim NmMemcached As String = "aplikasi1-M5_pv~M5_pv_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ppvid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ppvid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `ppv`.`ppvid` AS `ppvid`,`ppv`.`ppvcabang` AS `ppvcabang`,`ppv`.`ppvlokasi` AS `ppvlokasi`,`ppv`.`ppvgudang` AS `ppvgudang`,`ppv`.`ppvsumber` AS `ppvsumber`,`ppv`.`ppvautonotransaksi` AS `ppvautonotransaksi`,`ppv`.`ppvnotransaksi` AS `ppvnotransaksi`,`ppv`.`ppvtgl` AS `ppvtgl`,`ppv`.`ppvkodepa` AS `ppvkodepa`,`ppv`.`ppvcustomer` AS `ppvcustomer`,`ppv`.`ppvcustomerkontak` AS `ppvcustomerkontak`,`ppv`.`ppv1alamat1` AS `ppv1alamat1`,`ppv`.`ppv1alamat2` AS `ppv1alamat2`,`ppv`.`ppv1alamat3` AS `ppv1alamat3`,`ppv`.`ppv2alamat1` AS `ppv2alamat1`,`ppv`.`ppv2alamat2` AS `ppv2alamat2`,`ppv`.`ppv2alamat3` AS `ppv2alamat3`,`ppv`.`ppvbagianpenjualan` AS `ppvbagianpenjualan`,`ppv`.`ppvbagianterima` AS `ppvbagianterima`,`ppv`.`ppvuraian` AS `ppvuraian`,`ppv`.`ppvcatatan` AS `ppvcatatan`,`ppv`.`ppvnoref` AS `ppvnoref`,`ppv`.`ppvtglnoref` AS `ppvtglnoref`,`ppv`.`ppvcarabayar` AS `ppvcarabayar`,`ppv`.`ppvtglbayar` AS `ppvtglbayar`,`ppv`.`ppvmatauang` AS `ppvmatauang`,`ppv`.`ppvkurs` AS `ppvkurs`,`ppv`.`ppvtotalap` AS `ppvtotalap`,`ppv`.`ppvtotalapvalas` AS `ppvtotalapvalas`,`ppv`.`ppvtotalar` AS `ppvtotalar`,`ppv`.`ppvtotalarvalas` AS `ppvtotalarvalas`,`ppv`.`ppvbayar` AS `ppvbayar`,`ppv`.`ppvbayarvalas` AS `ppvbayarvalas`,`ppv`.`ppvselisihkurs` AS `ppvselisihkurs`,`ppv`.`ppvrekselisihkurs` AS `ppvrekselisihkurs`,`ppv`.`ppvdiskon` AS `ppvdiskon`,`ppv`.`ppvdiskonvalas` AS `ppvdiskonvalas`,`ppv`.`ppvrekdiskon` AS `ppvrekdiskon`,`ppv`.`ppvdenda` AS `ppvdenda`,`ppv`.`ppvdendavalas` AS `ppvdendavalas`,`ppv`.`ppvrekdenda` AS `ppvrekdenda`,`ppv`.`ppvstatus` AS `ppvstatus`,`ppv`.`ppvstatussebelumnya` AS `ppvstatussebelumnya`,`ppv`.`ppvjmlrevisi` AS `ppvjmlrevisi`,`ppv`.`ppvcetakanke` AS `ppvcetakanke`,`ppv`.`ppvinputuser` AS `ppvinputuser`,`ppv`.`ppvinputtgl` AS `ppvinputtgl`,`ppv`.`ppvmodifikasiuser` AS `ppvmodifikasiuser`,`ppv`.`ppvmodifikasitgl` AS `ppvmodifikasitgl`,`ppv`.`ppvposting` AS `ppvposting`,`ppv`.`ppvpostingtgl` AS `ppvpostingtgl`,`ppv`.`ppvisclose` AS `ppvisclose`,`ppv`.`ppvcustomtext1` AS `ppvcustomtext1`,`ppv`.`ppvcustomtext2` AS `ppvcustomtext2`,`ppv`.`ppvcustomtext3` AS `ppvcustomtext3`,`ppv`.`ppvcustomtext4` AS `ppvcustomtext4`,`ppv`.`ppvcustomtext5` AS `ppvcustomtext5`,`ppv`.`ppvcustomint1` AS `ppvcustomint1`,`ppv`.`ppvcustomint2` AS `ppvcustomint2`,`ppv`.`ppvcustomint3` AS `ppvcustomint3`,`ppv`.`ppvcustomdbl1` AS `ppvcustomdbl1`,`ppv`.`ppvcustomdbl2` AS `ppvcustomdbl2`,`ppv`.`ppvcustomdbl3` AS `ppvcustomdbl3`,`ppv`.`ppvcustomdate1` AS `ppvcustomdate1`,`ppv`.`ppvcustomdate2` AS `ppvcustomdate2`,`ppv`.`ppvcustomdate3` AS `ppvcustomdate3`,`br`.`bnama` AS `ppvcabangnama`,`lc`.`lnama` AS `ppvlokasinama`,`wh`.`wnama` AS `ppvgudangnama`,`c1`.`kkode` AS `ppvcustomerkode`,`c1`.`knama` AS `ppvcustomernama`,`c2`.`kkode` AS `ppvbagianpenjualankode`,`c2`.`knama` AS `ppvbagianpenjualannama`,`c3`.`kkode` AS `ppvbagianterimakode`,`c3`.`knama` AS `ppvbagianterimanama`,`pm`.`nama` AS `ppvcarabayarnama`,`coa1`.`cnama` AS `ppvrekselisihkursnama`,`coa2`.`cnama` AS `ppvrekdiskonnama`,`coa4`.`cnama` AS `ppvrekdendanama`,`st1`.`nama` AS `ppvstatusnama`,`st2`.`nama` AS `ppvstatussebelumnyanama`,`u1`.`unama` AS `ppvinputusernama`,`u2`.`unama` AS `ppvmodifikasiusernama`,`ppvd`.`idppvdetail` AS `idppvdetail`,`ppvd`.`idppv` AS `idppv`,`ppvd`.`sumber` AS `sumber`,`ppvd`.`idtransaksi` AS `idtransaksi`,`ppvd`.`matauang` AS `matauang`,`ppvd`.`kurs` AS `kurs`,`ppvd`.`totaltransaksi` AS `totaltransaksi`,`ppvd`.`terbayar` AS `terbayar`,`ppvd`.`sisa` AS `sisa`,`ppvd`.`jmlbayar` AS `jmlbayar`,`ppvd`.`jmlbayarvalas` AS `jmlbayarvalas`,`ppvd`.`diskon` AS `diskon`,`ppvd`.`jmldiskon` AS `jmldiskon`,`ppvd`.`jmldiskonvalas` AS `jmldiskonvalas`,`ppvd`.`nogiro` AS `nogiro`,`ppvd`.`rekhutangpiutang` AS `rekhutangpiutang`,`ppvd`.`catatan` AS `catatan`,`ppvd`.`costcenter` AS `costcenter`,`ppvd`.`divisi` AS `divisi`,`ppvd`.`subdivisi` AS `subdivisi`,`ppvd`.`proyek` AS `proyek`,`ppvd`.`urutan` AS `urutan`,`ppvd`.`isclose` AS `isclose`,`ppvd`.`customtext1` AS `customtext1`,`ppvd`.`customtext2` AS `customtext2`,`ppvd`.`customtext3` AS `customtext3`,`ppvd`.`customdbl1` AS `customdbl1`,`ppvd`.`customdbl2` AS `customdbl2`,`ppvd`.`customdbl3` AS `customdbl3`,`ppvd`.`customdate1` AS `customdate1`,`ppvd`.`customdate2` AS `customdate2`,`ppvd`.`customdate3` AS `customdate3`,(case `ppvd`.`sumber` when 'SI' then sinotransaksi when 'IP' then `ip`.`ipnotransaksi` else '' end) AS `notransaksi`,(case `ppvd`.`sumber` when 'SI' then angsuranke when 'IP' then angsuranke else '' end) AS `angsuranke`,(case `ppvd`.`sumber` when 'SI' then `si`.`sitgl` when 'IP' then `ip`.`iptgl` else `ppv`.`ppvtgl` end) AS `tgl`,(case `ppvd`.`sumber` when 'SI' then `si`.`sicarabayar` else `ppv`.`ppvcarabayar` end) AS `carabayar`,(case `ppvd`.`sumber` when 'SI' then `si`.`sitgljatuhtempo` when 'IP' then `ip`.`iptgljatuhtempo` else `ppv`.`ppvtgl` end) AS `tgljatuhtempo`, `ppvd`.`rencana` AS `rencana`,(case `ppvd`.`sumber` when 'SI' then `si`.`sistatuslunas` when 'IP' then `ip`.`ipstatusbayar` else 0 end) AS `statuslunas`,`coa3`.`cnama` AS `rekhutangpiutangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,(case `ppvd`.`sumber` when 'SI' then `si`.`siinputtgl` when 'IP' then `ip`.`ipinputtgl` else `ppv`.`ppvinputtgl` end) AS `inputtgl`, c1.kpkp from (((((((((((((((((((((`m_12_ppv` `ppv` join `m_12_ppv_detail` `ppvd` on((`ppv`.`ppvid` = `ppvd`.`idppv`))) left join `m1_branch` `br` on((`br`.`bkode` = `ppv`.`ppvcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `ppv`.`ppvlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `ppv`.`ppvgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `ppv`.`ppvcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `ppv`.`ppvbagianpenjualan`))) left join `m1_contact` `c3` on((`c3`.`kid` = `ppv`.`ppvbagianterima`))) left join `m0_payment_method` `pm` on((`ppv`.`ppvcarabayar` = `pm`.`kode`))) left join `m1_coa` `coa1` on((`ppv`.`ppvrekselisihkurs` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`ppv`.`ppvrekdiskon` = `coa2`.`cnomor`))) left join `m1_coa` `coa4` on((`ppv`.`ppvrekdenda` = `coa4`.`cnomor`)))left join `m0_status` `st1` on((`st1`.`kode` = `ppv`.`ppvstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `ppv`.`ppvstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `ppv`.`ppvinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `ppv`.`ppvmodifikasiuser`))) left join `m5_si_installment` `sii` on(((`ppvd`.`sumber` = 'SI') and (`ppvd`.`idtransaksi` = `sii`.`idsiinstallment`)))) LEFT JOIN `m5_si` `si` on(((`ppvd`.`sumber` = 'SI') and (`sii`.`idsi` = `si`.`siid`)))) left join `m5_ip` `ip` on(((`ppvd`.`sumber` = 'IP') and (`ppvd`.`idtransaksi` = `ip`.`ipid`))) left join `m1_coa` `coa3` on((`ppvd`.`rekhutangpiutang` = `coa3`.`cnomor`))) left join `m1_project` `p` on((`ppvd`.`proyek` = `p`.`pkode`))) left join `m1_cost_center` `cc` on((`ppvd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`ppvd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`ppvd`.`subdivisi` = `sd`.`sdkode`))"
        'result(2) = sql : GoTo selesai
        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ppvid"), 0), sptField,
                     FxDB(drutama("ppvcabang"), ""), sptField,
                     FxDB(drutama("ppvlokasi"), ""), sptField,
                     FxDB(drutama("ppvgudang"), ""), sptField,
                     FxDB(drutama("ppvsumber"), ""), sptField,
                     FxDB(drutama("ppvautonotransaksi"), 0), sptField,
                     FxDB(drutama("ppvnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ppvkodepa"), 0), sptField,
                     FxDB(drutama("ppvcustomer"), 0), sptField,
                     FxDB(drutama("ppvcustomerkontak"), ""), sptField,
                     FxDB(drutama("ppv1alamat1"), ""), sptField,
                     FxDB(drutama("ppv1alamat2"), ""), sptField,
                     FxDB(drutama("ppv1alamat3"), ""), sptField,
                     FxDB(drutama("ppv2alamat1"), ""), sptField,
                     FxDB(drutama("ppv2alamat2"), ""), sptField,
                     FxDB(drutama("ppv2alamat3"), ""), sptField,
                     FxDB(drutama("ppvbagianpenjualan"), 0), sptField,
                     FxDB(drutama("ppvbagianterima"), 0), sptField,
                     FxDB(drutama("ppvuraian"), ""), sptField,
                     FxDB(drutama("ppvcatatan"), ""), sptField,
                     FxDB(drutama("ppvnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("ppvcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvtglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("ppvmatauang"), ""), sptField,
                     FxDB(drutama("ppvkurs"), 0), sptField,
                     FxDB(drutama("ppvtotalap"), 0), sptField,
                     FxDB(drutama("ppvtotalapvalas"), 0), sptField,
                     FxDB(drutama("ppvtotalar"), 0), sptField,
                     FxDB(drutama("ppvtotalarvalas"), 0), sptField,
                     FxDB(drutama("ppvbayar"), 0), sptField,
                     FxDB(drutama("ppvbayarvalas"), 0), sptField,
                     FxDB(drutama("ppvselisihkurs"), 0), sptField,
                     FxDB(drutama("ppvrekselisihkurs"), ""), sptField,
                     FxDB(drutama("ppvdiskon"), 0), sptField,
                     FxDB(drutama("ppvdiskonvalas"), 0), sptField,
                     FxDB(drutama("ppvrekdiskon"), ""), sptField,
                     FxDB(drutama("ppvdenda"), 0), sptField,
                     FxDB(drutama("ppvdendavalas"), 0), sptField,
                     FxDB(drutama("ppvrekdenda"), ""), sptField,
                     FxDB(drutama("ppvstatus"), 0), sptField,
                     FxDB(drutama("ppvstatussebelumnya"), 0), sptField,
                     FxDB(drutama("ppvjmlrevisi"), 0), sptField,
                     FxDB(drutama("ppvcetakanke"), 0), sptField,
                     FxDB(drutama("ppvinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppvmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppvposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppvisclose"), 0), sptField,
                     FxDB(drutama("ppvcustomtext1"), ""), sptField,
                     FxDB(drutama("ppvcustomtext2"), ""), sptField,
                     FxDB(drutama("ppvcustomtext3"), ""), sptField,
                     FxDB(drutama("ppvcustomtext4"), ""), sptField,
                     FxDB(drutama("ppvcustomtext5"), ""), sptField,
                     FxDB(drutama("ppvcustomint1"), 0), sptField,
                     FxDB(drutama("ppvcustomint2"), 0), sptField,
                     FxDB(drutama("ppvcustomint3"), 0), sptField,
                     FxDB(drutama("ppvcustomdbl1"), 0), sptField,
                     FxDB(drutama("ppvcustomdbl2"), 0), sptField,
                     FxDB(drutama("ppvcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppvcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ppvcabangnama"), ""), sptField,
                     FxDB(drutama("ppvlokasinama"), ""), sptField,
                     FxDB(drutama("ppvgudangnama"), ""), sptField,
                     FxDB(drutama("ppvcustomerkode"), ""), sptField,
                     FxDB(drutama("ppvcustomernama"), ""), sptField,
                     FxDB(drutama("ppvbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("ppvbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("ppvbagianterimakode"), ""), sptField,
                     FxDB(drutama("ppvbagianterimanama"), ""), sptField,
                     FxDB(drutama("ppvcarabayarnama"), ""), sptField,
                     FxDB(drutama("ppvrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("ppvrekdiskonnama"), ""), sptField,
                     FxDB(drutama("ppvstatusnama"), ""), sptField,
                     FxDB(drutama("ppvstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("ppvinputusernama"), ""), sptField,
                     FxDB(drutama("ppvmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                'Dim tglgiro As String = FxDB(dr("tgljtgiro"), "")
                'If Len(tglgiro) > 0 Then tglgiro = AsFormatTanggal(FxDB(dr("tgljtgiro"), ""), formatTgl) Else tglgiro = tglgiro

                detail = String.Concat(detail, FxDB(dr("idppvdetail"), 0), sptField,
                     FxDB(dr("idppv"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptField,
                     FxDB(dr("jmlbayarvalas"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("jmldiskonvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
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
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("angsuranke"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'PANGGIL QUERY
            sql = "select `ppv`.`idppvcarabayar` AS `idppvcarabayar`, `ppv`.`idppv` AS `idppv`,`ppv`.`carabayar` AS `carabayar`, `ppv`.`matauang` AS `matauang`, `ppv`.`kurs` AS `kurs`,`ppv`.`jumlah` AS `jumlah`, `ppv`.`jumlahvalas` AS `jumlahvalas`,`ppv`.`nogiro` AS `nogiro`, `ppv`.`tgljt` AS `tgljt`,`ppv`.`bank` AS `bank`,`ppv`.`noacbank` AS `noacbank`, `ppv`.`rekbank` AS `rekbank`,`ppv`.`rekgiro` AS `rekgiro`,`ppv`.`catatan` AS `catatan`, `ppv`.`urutan` AS `urutan`,`ppv`.`isclose` AS `isclose`, `pm`.`nama` AS `carabayarnama`,`b`.`bnama` AS `banknama`,`coa1`.`cnama` AS `rekbanknama`, `coa2`.`cnama` AS `rekgironama` from ((((`m_12_ppv_pay` `ppv` left join `m0_payment_method` `pm` on((`ppv`.`carabayar` = `pm`.`kode`))) left join `m1_bank` `b` on((`ppv`.`bank` = `b`.`bkode`))) left join `m1_coa` `coa1` on((`ppv`.`rekbank` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`ppv`.`rekgiro` = `coa2`.`cnomor`)))"

            'AMBIL DATA PAY
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M_12_Ppv_Pay", "idppv=" & idtransaksi, "idppv ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idppvcarabayar"), 0), sptField,
                     FxDB(dr("idppv"), 0), sptField,
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
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppvid, ppvcabang, ppvlokasi, ppvgudang, ppvsumber, ppvautonotransaksi, ppvnotransaksi, ppvtgl, ppvkodepa, ppvcustomer, ppvcustomerkontak, ppv1alamat1, ppv1alamat2, ppv1alamat3, ppv2alamat1, ppv2alamat2, ppv2alamat3, ppvbagianpenjualan, ppvbagianterima, ppvuraian, ppvcatatan, ppvnoref, ppvtglnoref, ppvcarabayar, ppvtglbayar, ppvmatauang, ppvkurs, ppvtotalap, ppvtotalapvalas, ppvtotalar, ppvtotalarvalas, ppvbayar, ppvbayarvalas, ppvselisihkurs, ppvrekselisihkurs, ppvdiskon, ppvdiskonvalas, ppvrekdiskon, ppvdenda, ppvdendavalas, ppvrekdenda, ppvstatus, ppvstatussebelumnya, ppvjmlrevisi, ppvcetakanke, ppvinputuser, ppvinputtgl, ppvmodifikasiuser, ppvmodifikasitgl, ppvposting, ppvpostingtgl, ppvisclose, ppvcustomtext1, ppvcustomtext2, ppvcustomtext3, ppvcustomtext4, ppvcustomtext5, ppvcustomint1, ppvcustomint2, ppvcustomint3, ppvcustomdbl1, ppvcustomdbl2, ppvcustomdbl3, ppvcustomdate1, ppvcustomdate2, ppvcustomdate3, ppvcabangnama, ppvlokasinama, ppvgudangnama, ppvcustomerkode, ppvcustomernama, ppvbagianpenjualankode, ppvbagianpenjualannama, ppvbagianterimakode, ppvbagianterimanama, ppvcarabayarnama, ppvrekselisihkursnama, ppvrekdiskonnama, ppvrekdendanama, ppvstatusnama, ppvstatussebelumnyanama, ppvinputusernama, ppvmodifikasiusernama, kpkp" & sptSubParam & "idppvdetail, idppv, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskon, jmldiskon, jmldiskonvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, tgljatuhtempo, rencana, statuslunas, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, angsuranke, inputtgl" & sptSubParam & "idppvcarabayar, idppv, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function
End Class