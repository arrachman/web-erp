Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_pb
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_PvSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================



        'MAPPING BUAT WS ----------------------------------------------------------
        'pvid(0) As Integer, pvcabang(1) As String, pvlokasi(2) As String, pvgudang(3) As String, pvsumber(4) As String, 
        'pvautonotransaksi(5) As Integer, pvnotransaksi(6) As String, pvtgl(7) As Date, pvkodepa(8) As Integer, pvcustomer(9) As Integer, 
        'pvcustomerkontak(10) As String, pv1alamat1(11) As String, pv1alamat2(12) As String, pv1alamat3(13) As String, pv2alamat1(14) As String, 
        'pv2alamat2(15) As String, pv2alamat3(16) As String, pvbagianpenjualan(17) As Integer, pvbagianterima(18) As Integer, pvuraian(19) As String, 
        'pvcatatan(20) As String, pvnoref(21) As String, pvtglnoref(22) As Date, pvcarabayar(23) As Integer, pvtglbayar(24) As Date, 
        'pvmatauang(25) As String, pvkurs(26) As Double, pvtotalap(27) As Double, pvtotalapvalas(28) As Double, pvtotalar(29) As Double, 
        'pvtotalarvalas(30) As Double, pvbayar(31) As Double, pvbayarvalas(32) As Double, pvselisihkurs(33) As Double, pvrekselisihkurs(34) As String, 
        'pvdiskontermin(35) As Double, pvdiskonterminvalas(36) As Double, pvrekdiskontermin(37) As String, pvidic(38) As Integer, pvstatus(39) As Integer, 
        'pvstatussebelumnya(40) As Integer, pvjmlrevisi(41) As Integer, pvcetakanke(42) As Integer, pvinputuser(43) As Integer, pvinputtgl(44) As DateTime, 
        'pvmodifikasiuser(45) As Integer, pvmodifikasitgl(46) As DateTime, pvisclose(47) As Integer, pvcustomtext1(48) As String, pvcustomtext2(49) As String, 
        'pvcustomtext3(50) As String, pvcustomtext4(51) As String, pvcustomtext5(52) As String, pvcustomint1(53) As Integer, pvcustomint2(54) As Integer, 
        'pvcustomint3(55) As Integer, pvcustomdbl1(56) As Double, pvcustomdbl2(57) As Double, pvcustomdbl3(58) As Double, pvcustomdate1(59) As Date, 
        'pvcustomdate2(60) As Date, pvcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, 
        'pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, 
        'pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, 
        'pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, 
        'pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, 
        'pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, 
        'pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvisclose, pvcustomtext1, 
        'pvcustomtext2, pvcustomtext3, pvcustomtext4, pvcustomtext5, pvcustomint1, pvcustomint2, pvcustomint3, 
        'pvcustomdbl1, pvcustomdbl2, pvcustomdbl3, pvcustomdate1, pvcustomdate2, pvcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 63) Then
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
        'pvdiskontermin(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pvdiskontermin required numeric." : GoTo selesai
        End If
        'pvdiskonterminvalas(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pvdiskonterminvalas required numeric." : GoTo selesai
        End If
        'pvidic(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pvidic required numeric." : GoTo selesai
        End If
        'pvstatus(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pvstatus required numeric." : GoTo selesai
        End If
        'pvstatussebelumnya(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pvstatussebelumnya required numeric." : GoTo selesai
        End If
        'pvjmlrevisi(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pvjmlrevisi required numeric." : GoTo selesai
        End If
        'pvcetakanke(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pvcetakanke required numeric." : GoTo selesai
        End If
        'pvinputuser(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pvinputuser required numeric." : GoTo selesai
        End If
        'pvinputtgl(44) As DateTime
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "pvinputtgl required date." : GoTo selesai
        End If
        'pvmodifikasiuser(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "pvmodifikasiuser required numeric." : GoTo selesai
        End If
        'pvmodifikasitgl(46) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "pvmodifikasitgl required date." : GoTo selesai
        End If
        'pvisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "pvisclose required numeric." : GoTo selesai
        End If
        'pvcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pvcustomint1 required numeric." : GoTo selesai
        End If
        'pvcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pvcustomint2 required numeric." : GoTo selesai
        End If
        'pvcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pvcustomint3 required numeric." : GoTo selesai
        End If
        'pvcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "pvcustomdbl1 required numeric." : GoTo selesai
        End If
        'pvcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "pvcustomdbl2 required numeric." : GoTo selesai
        End If
        'pvcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pvcustomdbl3 required numeric." : GoTo selesai
        End If
        'pvcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "pvcustomdate1 required date." : GoTo selesai
        End If
        'pvcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "pvcustomdate2 required date." : GoTo selesai
        End If
        'pvcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "pvcustomdate3 required date." : GoTo selesai
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
        'If Len(dataUtama(25)) = 0 Then
        '    result(2) = "pvmatauang can't be empty" : GoTo selesai
        'End If
        'If Len(dataUtama(25)) > 25 Then
        '    result(2) = "pvmatauang should not be more than 25 character." : GoTo selesai
        'End If

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

        'pvdiskontermin(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "pvdiskontermin can't be empty" : GoTo selesai
        End If

        'pvdiskonterminvalas(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pvdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'pvinputtgl(44) As DateTime
        If Len(dataUtama(44)) = 0 Then
            result(2) = "pvinputtgl can't be empty" : GoTo selesai
        End If

        'pvmodifikasitgl(46) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "pvmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pvcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "pvcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pvcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "pvcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pvcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "pvcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pvcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "pvcustomdate1 can't be empty" : GoTo selesai
        End If

        'pvcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "pvcustomdate2 can't be empty" : GoTo selesai
        End If

        'pvcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "pvcustomdate3 can't be empty" : GoTo selesai
        End If

        ''VALIDASI JUMLAH BAYAR
        ''JIKA TOTAL AR - DISKON TERMIN - TOTAL AP + SELISIH KURS <> 0 MAKA MUNCUL PERINGATAN
        ''               pvtotalar(29),           pvdiskontermin(35),                pvtotalap(27),            pvselisihkurs(33)
        'If Double.Parse(dataUtama(29)) - Double.Parse(dataUtama(35)) - Double.Parse(dataUtama(27)) + Double.Parse(dataUtama(33)) <> 0 Then
        '    Dim selisih(2) As String
        '    selisih = F_Nominal((Double.Parse(dataUtama(29)) - Double.Parse(dataUtama(35)) - Double.Parse(dataUtama(27)) + Double.Parse(dataUtama(33))), False).Split(sptSubParam)
        '    result(2) = "Total AR - Total AP must be balance : " & selisih(1) & "" : GoTo selesai
        'End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pvid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pv1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pv1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pv1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pv2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pv2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pv2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvtglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvtotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvtotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvtotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvtotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvrekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvrekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvidic", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pvcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pvpetugas", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "pvid~pvcabang~pvlokasi~pvgudang~pvsumber~pvautonotransaksi~pvnotransaksi~pvtgl~pvkodepa~pvcustomer~pvcustomerkontak~pv1alamat1~pv1alamat2~pv1alamat3~pv2alamat1~pv2alamat2~pv2alamat3~pvbagianpenjualan~pvbagianterima~pvuraian~pvcatatan~pvnoref~pvtglnoref~pvcarabayar~pvtglbayar~pvmatauang~pvkurs~pvtotalap~pvtotalapvalas~pvtotalar~pvtotalarvalas~pvbayar~pvbayarvalas~pvselisihkurs~pvrekselisihkurs~pvdiskontermin~pvdiskonterminvalas~pvrekdiskontermin~pvidic~pvstatus~pvstatussebelumnya~pvjmlrevisi~pvcetakanke~pvinputuser~pvinputtgl~pvmodifikasiuser~pvmodifikasitgl~pvisclose~pvcustomtext1~pvcustomtext2~pvcustomtext3~pvcustomtext4~pvcustomtext5~pvcustomint1~pvcustomint2~pvcustomint3~pvcustomdbl1~pvcustomdbl2~pvcustomdbl3~pvcustomdate1~pvcustomdate2~pvcustomdate3~pvpetugas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpvdetail(0) As Integer, idpv(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, nogiro(14) As String, 
        'rekhutangpiutang(15) As String, catatan(16) As String, costcenter(17) As String, divisi(18) As String, subdivisi(19) As String, 
        'proyek(20) As String, idicdetail(21) As Integer, urutan(22) As Integer, isclose(23) As Integer, customtext1(24) As String, 
        'customtext2(25) As String, customtext3(26) As String, customdbl1(27) As Double, customdbl2(28) As Double, customdbl3(29) As Double, 
        'customdate1(30) As Date, customdate2(31) As Date, customdate3(32) As Date, rencana(33) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpvdetail, idpv, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, 
        'idicdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, rencana


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpvdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpv", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idicdetail", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstandingSI As String = "", ftOutstandingSI As String = "", updNilaiSI As String = "", updFilterSI As String = "", updTglLunasSI As String = ""
        'AS
        Dim ftExistOutstandingAS As String = "", ftOutstandingAS As String = "", updNilaiAS As String = "", updNilaiValasAS As String = "", updFilterAS As String = "", updTglLunasAS As String = ""
        'SR
        Dim ftExistOutstandingSR As String = "", ftOutstandingSR As String = "", updNilaiSR As String = "", updFilterSR As String = "", updTglLunasSR As String = ""
        'IP
        Dim ftExistOutstandingIP As String = "", ftOutstandingIP As String = "", updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = "", updTglLunasIP As String = ""
        'RP
        Dim ftExistOutstandingRP As String = "", ftOutstandingRP As String = "", updNilaiRP As String = "", updNilaiValasRP As String = "", updFilterRP As String = "", updTglLunasRP As String = ""


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 34) Then
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
            'rencana(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
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
            'idicdetail(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - idicdetail required numeric." : GoTo selesai
            End If
            'urutan(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
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
            If (dataRowDetail(2) <> "KW" And dataRowDetail(2) <> "CA") Then
                result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
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

            'rencana(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
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

            'rekhutangpiutang(15) As String
            'If Len(dataRowDetail(15)) = 0 Then
            '    result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(15)) > 25 Then
            '    result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            'End If

            'customdbl1(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(30) As Date
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpvdetail~idpv~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~nogiro~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~idicdetail~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'sumber(2) As String            , idtransaksi(3) As Integer            , jmlbayar(9) As Double
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3) : jmlbayar = dataRowDetail(9)
            'jmlbayarvalas(10) As Double      , rekhutangpiutang(15) As String, idicdetail(21) As Integer
            jmlbayarvalas = dataRowDetail(10) : norek = dataRowDetail(14) : idicdetail = dataRowDetail(21)
            'matauang(4) As String
            matauangDetail = dataRowDetail(4)

            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------


            'VALIDASI OUTSTANDING -------------------------
            If idicdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                Select Case sumberDetail
                    Case "KW"
                        ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m_11_kw_detail JOIN m_11_kw ON idkw = kwid WHERE idkwdetail = '" & idicdetail & "' AND (kwstatus = 2 OR kwstatus = 3 OR kwstatus = 4 OR kwstatus = 7) LIMIT 1) as rowExists, '" & idicdetail & "' as idkwdetail, '" & sumberDetail & "' as sumber, kwnotransaksi as notransaksi FROM m_11_kw WHERE kwid = '" & idtransaksiDetail & "'")
                        'Case "AS"
                        '    ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_ic_detail JOIN m5_ic ON idic = icid WHERE idicdetail = '" & idicdetail & "' AND (icstatus = 2 OR icstatus = 3 OR icstatus = 4 OR icstatus = 7) LIMIT 1) as rowExists, '" & idicdetail & "' as idicdetail, '" & sumberDetail & "' as sumber, asnotransaksi as notransaksi FROM m5_as WHERE asid = '" & idtransaksiDetail & "'")
                        'Case "SR"
                        '    ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_ic_detail JOIN m5_ic ON idic = icid WHERE idicdetail = '" & idicdetail & "' AND (icstatus = 2 OR icstatus = 3 OR icstatus = 4 OR icstatus = 7) LIMIT 1) as rowExists, '" & idicdetail & "' as idicdetail, '" & sumberDetail & "' as sumber, srnotransaksi as notransaksi FROM m5_sr WHERE srid = '" & idtransaksiDetail & "'")
                        'Case "RP"
                        '    ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_ic_detail JOIN m5_ic ON idic = icid WHERE idicdetail = '" & idicdetail & "' AND (icstatus = 2 OR icstatus = 3 OR icstatus = 4 OR icstatus = 7) LIMIT 1) as rowExists, '" & idicdetail & "' as idicdetail, '" & sumberDetail & "' as sumber, rpnotransaksi as notransaksi FROM m5_rp WHERE rpid = '" & idtransaksiDetail & "'")
                        'Case "IP"
                        '    ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_ic_detail JOIN m5_ic ON idic = icid WHERE idicdetail = '" & idicdetail & "' AND (icstatus = 2 OR icstatus = 3 OR icstatus = 4 OR icstatus = 7) LIMIT 1) as rowExists, '" & idicdetail & "' as idicdetail, '" & sumberDetail & "' as sumber, ipnotransaksi as notransaksi FROM m5_ip WHERE ipid = '" & idtransaksiDetail & "'")
                        'Case "CA"
                        '    ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_ic_detail JOIN m5_ic ON idic = icid WHERE idicdetail = '" & idicdetail & "' AND (icstatus = 2 OR icstatus = 3 OR icstatus = 4 OR icstatus = 7) LIMIT 1) as rowExists, '" & idicdetail & "' as idicdetail, '" & sumberDetail & "' as sumber, '" & norek & "' as notransaksi")
                    Case Else
                        result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
                End Select

                '2. CEK JML OUTSTANDING
                Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "idicdetail=" & idicdetail)
                OutstandingValas = AsDataTableDSum(dtdetail, "jmlbayarvalas", "idicdetail=" & idicdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (icd.idicdetail = " & idicdetail & " AND " & Outstanding & " > (icd.jmlbayar - icd.jmlpv)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idicdetail & "' THEN jmlpv + '" & Outstanding & "' ", updNilai)
                updNilaiValas = String.Concat("WHEN '" & idicdetail & "' THEN jmlpvvalas + '" & OutstandingValas & "' ", updNilaiValas)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idkwdetail = '" & idicdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 22
                Select Case drutama("pvstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pvtgl")), AsFormatTanggal(drutama("pvtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "pvmatauang", "pvrekselisihkurs~pvrekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("pvstatus") = 2 Or drutama("pvstatus") = 1 Or drutama("pvstatus") = 8 Or drutama("pvstatus") = 9 Or drutama("pvstatus") = 10 Or drutama("pvstatus") = 11 Then

                    ''CEK JMLBAYAR TRANSAKSI ---------------------
                    Dim JmlCoa As Double = 0, JmlKW As Double = 0

                    JmlCoa = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = 'CA'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'CA'")
                    JmlKW = AsDataTableDSum(dtdetail, "totaltransaksi", "sumber = 'KW'") - AsDataTableDSum(dtdetail, "jmldiskontermin", "sumber = 'KW'")
                    'result(2) = "Total CoA " & JmlCoa & " - " & JmlKW : Trans.Rollback() : GoTo selesai
                    'JIKA SELISIH TOTAL COA DAN TOTAL KW >= 0.1 MAKA ALERT TIDAK BISA DISIMPAN
                    If Math.Abs(JmlKW - JmlCoa) >= 0.1 Then
                        Dim selisih(2) As String
                        selisih = F_Nominal(F_Round(Math.Abs(JmlKW - JmlCoa)), False).Split(sptSubParam)
                        result(2) = "Total CoA and Total KW are not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                    End If
                    ''END OF CEK JMLBAYAR TRANSAKSI --------------

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, MUFungsional, ftExistOutstandingSI, ftOutstandingSI, ftExistOutstandingAS, ftOutstandingAS, ftExistOutstandingSR, ftOutstandingSR, ftExistOutstandingRP, ftOutstandingRP, ftExistOutstandingIP, ftOutstandingIP, updFilterSI, updFilterAS, updFilterSR, updFilterRP, updFilterIP, formatTgl, drutama("pvtgl"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("pvid")
                    notransaksi = drutama("pvnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(pvid), pvnotransaksi FROM m_11_pb WHERE pvid='" & result(4) & "' AND pvstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pvid) FROM m_11_pb WHERE pvnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m5_pv_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M5_Pv_HistorySimpan("" & paramSplit(0) & "★M5_Pv_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pvsumber")) & "▼" & FixQuotes(drutama("pvid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update m_11_pb set pvcabang  = '" & FixQuotes(drutama("pvcabang")) & "', pvlokasi  = '" & FixQuotes(drutama("pvlokasi")) & "', pvgudang  = '" & FixQuotes(drutama("pvgudang")) & "', pvsumber  = '" & FixQuotes(drutama("pvsumber")) & "', pvautonotransaksi  = " & drutama("pvautonotransaksi") & ", pvnotransaksi  = '" & FixQuotes(notransaksi) & "', pvtgl  = '" & FixQuotes(AsFormatTanggal(drutama("pvtgl"))) & "', pvkodepa  = " & drutama("pvkodepa") & ", pvcustomer  = " & drutama("pvcustomer") & ", pvcustomerkontak  = '" & FixQuotes(drutama("pvcustomerkontak")) & "', pv1alamat1  = '" & FixQuotes(drutama("pv1alamat1")) & "', pv1alamat2  = '" & FixQuotes(drutama("pv1alamat2")) & "', pv1alamat3  = '" & FixQuotes(drutama("pv1alamat3")) & "', pv2alamat1  = '" & FixQuotes(drutama("pv2alamat1")) & "', pv2alamat2  = '" & FixQuotes(drutama("pv2alamat2")) & "', pv2alamat3  = '" & FixQuotes(drutama("pv2alamat3")) & "', pvbagianpenjualan  = " & drutama("pvbagianpenjualan") & ", pvbagianterima  = " & drutama("pvbagianterima") & ", pvuraian  = '" & FixQuotes(drutama("pvuraian")) & "', pvcatatan  = '" & FixQuotes(drutama("pvcatatan")) & "', pvnoref  = '" & FixQuotes(drutama("pvnoref")) & "', pvtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pvtglnoref"))) & "', pvcarabayar  = " & drutama("pvcarabayar") & ", pvtglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("pvtglbayar"))) & "', pvmatauang  = '" & FixQuotes(drutama("pvmatauang")) & "', pvkurs  = '" & FixDouble(drutama("pvkurs")) & "', pvtotalap  = '" & FixDouble(drutama("pvtotalap")) & "', pvtotalapvalas  = '" & FixDouble(drutama("pvtotalapvalas")) & "', pvtotalar  = '" & FixDouble(drutama("pvtotalar")) & "', pvtotalarvalas  = '" & FixDouble(drutama("pvtotalarvalas")) & "', pvbayar  = '" & FixDouble(drutama("pvbayar")) & "', pvbayarvalas  = '" & FixDouble(drutama("pvbayarvalas")) & "', pvselisihkurs  = '" & FixDouble(drutama("pvselisihkurs")) & "', pvrekselisihkurs  = '" & FixQuotes(drutama("pvrekselisihkurs")) & "', pvdiskontermin  = '" & FixDouble(drutama("pvdiskontermin")) & "', pvdiskonterminvalas  = '" & FixDouble(drutama("pvdiskonterminvalas")) & "', pvrekdiskontermin  = '" & FixQuotes(drutama("pvrekdiskontermin")) & "', pvidic  = " & drutama("pvidic") & ", pvstatus  = " & drutama("pvstatus") & ", pvstatussebelumnya  = " & drutama("pvstatussebelumnya") & ", pvjmlrevisi  = pvjmlrevisi+1, pvcetakanke  = " & drutama("pvcetakanke") & ", pvmodifikasiuser  = " & drutama("pvmodifikasiuser") & ", pvmodifikasitgl  = NOW(), pvcustomtext1  = '" & FixQuotes(drutama("pvcustomtext1")) & "', pvcustomtext2  = '" & FixQuotes(drutama("pvcustomtext2")) & "', pvcustomtext3  = '" & FixQuotes(drutama("pvcustomtext3")) & "', pvcustomtext4  = '" & FixQuotes(drutama("pvcustomtext4")) & "', pvcustomtext5  = '" & FixQuotes(drutama("pvcustomtext5")) & "', pvcustomint1  = " & drutama("pvcustomint1") & ", pvcustomint2  = " & drutama("pvcustomint2") & ", pvcustomint3  = " & drutama("pvcustomint3") & ", pvcustomdbl1  = '" & FixDouble(drutama("pvcustomdbl1")) & "', pvcustomdbl2  = '" & FixDouble(drutama("pvcustomdbl2")) & "', pvcustomdbl3  = '" & FixDouble(drutama("pvcustomdbl3")) & "', pvcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pvcustomdate1"))) & "', pvcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pvcustomdate2"))) & "', pvcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pvcustomdate3"))) & "', pvpetugas = " & drutama("pvpetugas") & " where pvid = '" & drutama("pvid") & "'"
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

                    If drutama("pvautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pvcabang"), drutama("pvlokasi"), drutama("pvsumber"), drutama("pvtgl"))
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
                        notransaksi = drutama("pvnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pvid) FROM m_11_pb WHERE pvnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into m_11_pb (pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvisclose, pvcustomtext1, pvcustomtext2, pvcustomtext3, pvcustomtext4, pvcustomtext5, pvcustomint1, pvcustomint2, pvcustomint3, pvcustomdbl1, pvcustomdbl2, pvcustomdbl3, pvcustomdate1, pvcustomdate2, pvcustomdate3, pvpetugas) values('" & FixQuotes(drutama("pvcabang")) & "', '" & FixQuotes(drutama("pvlokasi")) & "', '" & FixQuotes(drutama("pvgudang")) & "', '" & FixQuotes(drutama("pvsumber")) & "', " & drutama("pvautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pvtgl"))) & "', " & drutama("pvkodepa") & ", " & drutama("pvcustomer") & ", '" & FixQuotes(drutama("pvcustomerkontak")) & "', '" & FixQuotes(drutama("pv1alamat1")) & "', '" & FixQuotes(drutama("pv1alamat2")) & "', '" & FixQuotes(drutama("pv1alamat3")) & "', '" & FixQuotes(drutama("pv2alamat1")) & "', '" & FixQuotes(drutama("pv2alamat2")) & "', '" & FixQuotes(drutama("pv2alamat3")) & "', " & drutama("pvbagianpenjualan") & ", " & drutama("pvbagianterima") & ", '" & FixQuotes(drutama("pvuraian")) & "', '" & FixQuotes(drutama("pvcatatan")) & "', '" & FixQuotes(drutama("pvnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pvtglnoref"))) & "', " & drutama("pvcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("pvtglbayar"))) & "', '" & FixQuotes(drutama("pvmatauang")) & "', '" & FixDouble(drutama("pvkurs")) & "', '" & FixDouble(drutama("pvtotalap")) & "', '" & FixDouble(drutama("pvtotalapvalas")) & "', '" & FixDouble(drutama("pvtotalar")) & "', '" & FixDouble(drutama("pvtotalarvalas")) & "', '" & FixDouble(drutama("pvbayar")) & "', '" & FixDouble(drutama("pvbayarvalas")) & "', '" & FixDouble(drutama("pvselisihkurs")) & "', '" & FixQuotes(drutama("pvrekselisihkurs")) & "', '" & FixDouble(drutama("pvdiskontermin")) & "', '" & FixDouble(drutama("pvdiskonterminvalas")) & "', '" & FixQuotes(drutama("pvrekdiskontermin")) & "', " & drutama("pvidic") & ", " & drutama("pvstatus") & ", " & drutama("pvstatussebelumnya") & ", " & drutama("pvjmlrevisi") & ", " & drutama("pvcetakanke") & ", " & drutama("pvinputuser") & ", NOW(), " & drutama("pvmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("pvisclose") & ", '" & FixQuotes(drutama("pvcustomtext1")) & "', '" & FixQuotes(drutama("pvcustomtext2")) & "', '" & FixQuotes(drutama("pvcustomtext3")) & "', '" & FixQuotes(drutama("pvcustomtext4")) & "', '" & FixQuotes(drutama("pvcustomtext5")) & "', " & drutama("pvcustomint1") & ", " & drutama("pvcustomint2") & ", " & drutama("pvcustomint3") & ", '" & FixDouble(drutama("pvcustomdbl1")) & "', '" & FixDouble(drutama("pvcustomdbl2")) & "', '" & FixDouble(drutama("pvcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pvcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pvcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pvcustomdate3"))) & "'," & FixDouble(drutama("pvpetugas")) & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select pvid from m_11_pb where pvnotransaksi='" & notransaksi & "' AND pvinputuser= '" & userid & "' order by pvmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from m_11_pb_detail where idpv = '" & result(4) & "'"
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
                Dim updFilterKW As String = ""

                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows

                        If FixQuotes(dr1("sumber")).ToUpper.Equals("KW") Then
                            'SET FILTER UPDATE OUTSTANDING ---------
                            updFilterKW = IIf(Len(updFilterKW.ToString) = 0, "", updFilterKW & " OR ")
                            updFilterKW = String.Concat(updFilterKW, "(kwid = '" & FixDouble(dr1("idtransaksi")) & "')")
                        End If

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idpvdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("idicdetail") & ", " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into m_11_pb_detail(idpvdetail, idpv, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idicdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("pvstatus") = 2 Then

                    If Len(updFilterKW) > 0 Then
                        sql = "UPDATE m_11_kw SET kwstatus = 4 WHERE " & updFilterKW
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    ''UPDATE OUTSTANDING TRANSAKSI ===================================================
                    ''If Len(updNilai) > 0 Then
                    ''UPDATE DETAIL
                    'sql = "UPDATE m_11_kw_detail SET jmlpb = '" & FixDouble(drutama("pvtotalap")) & "', statuspb = 2 WHERE idkw = '" & (drutama("pvidic")) & "'"
                    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    'With objCmd
                    '    .Connection = myconn
                    '    .Transaction = Trans
                    '    .CommandType = CommandType.Text
                    '    .CommandText = sql
                    'End With
                    'objCmd.ExecuteNonQuery()

                    ''UPDATE UTAMA
                    ''Dim ftDetail As String = "", statusOut As Integer = 0
                    ''Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idkw FROM m_11_kw_detail WHERE " & updFilter & " GROUP BY idkw")
                    ''If dtOut.Rows.Count > 0 Then
                    ''    For Each dr1 As DataRow In dtOut.Rows
                    ''        ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                    ''        ftDetail = String.Concat(ftDetail, "(idkw = '" & dr1("idkw") & "')")
                    ''    Next
                    ''End If
                    ''dtOut = AsDataTableAmbilDariDBCon("SELECT idkw, SUM(jmlbayar) as jmlbayar, SUM(jmlpb) as jmlpb FROM m_11_kw_detail WHERE " & ftDetail & " GROUP BY idkw")
                    ''If dtOut.Rows.Count > 0 Then
                    ''    'KOSONGKAN VARIABEL NILAI DAN FILTER
                    ''    updNilai = "" : updFilter = ""
                    ''    For Each dr1 As DataRow In dtOut.Rows
                    ''        '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                    ''        If dr1("jmlpb") >= 0 Then
                    ''            statusOut = 2
                    ''        Else
                    ''            statusOut = 1
                    ''        End If
                    ''        '2. SET NILAI UPDATE OUTSTANDING
                    ''        updNilai = String.Concat(updNilai, "WHEN '" & dr1("idkw") & "' THEN '" & statusOut & "' ")
                    ''        '3. SET FILTERUPDATE OUTSTANDING
                    ''        updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                    ''        updFilter = String.Concat(updFilter, "(kwid = '" & dr1("idkw") & "')")
                    ''    Next

                    'sql = "UPDATE m_11_kw SET kwstatuspb = 2, kwstatus = 4 WHERE kwid = '" & (drutama("pvidic")) & "'"
                    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    'With objCmd
                    '    .Connection = myconn
                    '    .Transaction = Trans
                    '    .CommandType = CommandType.Text
                    '    .CommandText = sql
                    'End With
                    'objCmd.ExecuteNonQuery()
                    ''End If
                    ''End If
                    ''END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    ''UPDATE TRANSAKSI PEMBAYARAN ====================================================
                    ''SI
                    ''If Len(updNilaiSI) > 0 Then
                    ''    sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid =  t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET si.sijmlbayar = (CASE si.siid " & updNilaiSI & " ELSE si.sijmlbayar END), si.sitgllunas = (CASE si.siid " & updTglLunasSI & " ELSE si.sitgllunas END), t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE " & updFilterSI
                    ''    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    ''    With objCmd
                    ''        .Connection = myconn
                    ''        .Transaction = Trans
                    ''        .CommandType = CommandType.Text
                    ''        .CommandText = sql
                    ''    End With
                    ''    objCmd.ExecuteNonQuery()
                    ''End If

                    ' ''AS
                    ''If Len(updNilaiAS) > 0 Then
                    ''    'UPDATE UTAMA
                    ''    sql = "UPDATE m5_as m5as LEFT JOIN m2_transaction_journal t ON m5as.assumber = t.tsumber AND m5as.asid =  t.tidtransaksi AND m5as.asnotransaksi = t.tnotransaksi SET m5as.asjumlahbayar = (CASE m5as.asid " & updNilaiAS & " ELSE m5as.asjumlahbayar END), m5as.asjumlahbayarvalas = (CASE m5as.asid " & updNilaiValasAS & " ELSE m5as.asjumlahbayarvalas END), m5as.astgllunas = (CASE m5as.asid " & updTglLunasAS & " ELSE m5as.astgllunas END), t.tstatuslunas = m5as.asstatusbayar, t.ttgllunas = m5as.astgllunas WHERE " & updFilterAS
                    ''    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    ''    With objCmd
                    ''        .Connection = myconn
                    ''        .Transaction = Trans
                    ''        .CommandType = CommandType.Text
                    ''        .CommandText = sql
                    ''    End With
                    ''    objCmd.ExecuteNonQuery()
                    ''End If

                    ' ''SR
                    ''If Len(updNilaiSR) > 0 Then
                    ''    sql = "UPDATE m5_sr sr LEFT JOIN m2_transaction_journal t ON sr.srsumber = t.tsumber AND sr.srid =  t.tidtransaksi AND sr.srnotransaksi = t.tnotransaksi SET sr.srjmlbayar = (CASE sr.srid " & updNilaiSR & " ELSE sr.srjmlbayar END), sr.srtgllunas = (CASE sr.srid " & updTglLunasSR & " ELSE sr.srtgllunas END), t.tstatuslunas = sr.srstatuslunas, t.ttgllunas = sr.srtgllunas WHERE " & updFilterSR
                    ''    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    ''    With objCmd
                    ''        .Connection = myconn
                    ''        .Transaction = Trans
                    ''        .CommandType = CommandType.Text
                    ''        .CommandText = sql
                    ''    End With
                    ''    objCmd.ExecuteNonQuery()
                    ''End If

                    ' ''RP
                    ''If Len(updNilaiRP) > 0 Then
                    ''    'UPDATE UTAMA
                    ''    sql = "UPDATE m5_rp rp LEFT JOIN m2_transaction_journal t ON rp.rpsumber = t.tsumber AND rp.rpid =  t.tidtransaksi AND rp.rpnotransaksi = t.tnotransaksi SET rp.rpjumlahbayar = (CASE rp.rpid " & updNilaiRP & " ELSE rp.rpjumlahbayar END), rp.rpjumlahbayarvalas = (CASE rp.rpid " & updNilaiValasRP & " ELSE rp.rpjumlahbayarvalas END), rp.rptgllunas = (CASE rp.rpid " & updTglLunasRP & " ELSE rp.rptgllunas END), t.tstatuslunas = rp.rpstatusbayar, t.ttgllunas = rp.rptgllunas WHERE " & updFilterRP
                    ''    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    ''    With objCmd
                    ''        .Connection = myconn
                    ''        .Transaction = Trans
                    ''        .CommandType = CommandType.Text
                    ''        .CommandText = sql
                    ''    End With
                    ''    objCmd.ExecuteNonQuery()
                    ''End If

                    ' ''IP
                    ''If Len(updNilaiIP) > 0 Then
                    ''    'UPDATE UTAMA
                    ''    sql = "UPDATE m5_ip ip LEFT JOIN m2_transaction_journal t ON ip.ipsumber = t.tsumber AND ip.ipid =  t.tidtransaksi AND ip.ipnotransaksi = t.tnotransaksi SET ip.ipjumlahbayar = (CASE ip.ipid " & updNilaiIP & " ELSE ip.ipjumlahbayar END), ip.ipjumlahbayarvalas = (CASE ip.ipid " & updNilaiValasIP & " ELSE ip.ipjumlahbayarvalas END), ip.iptgllunas = (CASE ip.ipid " & updTglLunasIP & " ELSE ip.iptgllunas END), t.tstatuslunas = ip.ipstatusbayar, t.ttgllunas = ip.iptgllunas WHERE " & updFilterIP
                    ''    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    ''    With objCmd
                    ''        .Connection = myconn
                    ''        .Transaction = Trans
                    ''        .CommandType = CommandType.Text
                    ''        .CommandText = sql
                    ''    End With
                    ''    objCmd.ExecuteNonQuery()
                    ''End If
                    ''UPDATE TRANSAKSI PEMBAYARAN ====================================================
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PB", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("pvstatus") = 2 Then
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
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
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
    Public Function M11_PvUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Pb", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pvtgl, Pvnotransaksi, Pvstatus FROM M_11_Pb WHERE Pvid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pvstatussebelumnya" : jnsaktivitas = 17
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

                'VARIABEL CEK TRANSAKSI PEMBAYARAN --> SI, AS, SR, RP, IP, CA
                'SI
                Dim updNilaiSI As String = "", updFilterSI As String = ""
                'AS
                Dim updNilaiAS As String = "", updNilaiValasAS As String = "", updFilterAS As String = ""
                'SR
                Dim updNilaiSR As String = "", updFilterSR As String = ""
                'IP
                Dim updNilaiIP As String = "", updNilaiValasIP As String = "", updFilterIP As String = ""
                'RP
                Dim updNilaiRP As String = "", updNilaiValasRP As String = "", updFilterRP As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT pvd.sumber as sumber, pvd.idtransaksi as idtransaksi, pvd.totaltransaksi as totaltransaksi, pvd.rekhutangpiutang as rekhutangpiutang, pv.pvidic as idic, pvd.urutan as urutan FROM m_11_pb_detail pvd join m_11_pb pv on (pvd.idpv = pv.pvid) and pvd.sumber = 'KW' WHERE pvd.idpv = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    Dim MUFungsional As String = ""

                    'AMBIL MATA UANG FUNGSIONAL DARI SETTING
                    'Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
                    'If dtSetting.Rows.Count > 0 Then
                    '    MUFungsional = dtSetting.Rows(0)(0)
                    'Else
                    '    result(2) = "Can't found 'Functional Currency' in Setting." : Trans.Rollback() : GoTo selesai
                    'End If

                    For Each dr1 As DataRow In dtdetail.Rows
                        sumberDetail = dr1("sumber") : idtransaksiDetail = dr1("idtransaksi") : jmlbayar = dr1("totaltransaksi")
                        norek = dr1("rekhutangpiutang") : idicdetail = dr1("idic")

                        '2. SET FILTER UPDATE OUTSTANDING ---------
                        updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                        updFilter = String.Concat(updFilter, "(kwid = '" & idtransaksiDetail & "')")
                        'End If

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    sql = "UPDATE m_11_kw SET kwstatus = kwstatussebelumnya WHERE " & updFilter
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                ''UPDATE OUTSTANDING TRANSAKSI =======================================================
                ''If Len(updNilai) > 0 Then
                ' ''UPDATE DETAIL
                ''sql = "UPDATE m_11_kw_detail SET jmlpb = 0, statuspb = 0 WHERE " & updFilter
                ''objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                ''With objCmd
                ''    .Connection = myconn
                ''    .Transaction = Trans
                ''    .CommandType = CommandType.Text
                ''    .CommandText = sql
                ''End With
                ''objCmd.ExecuteNonQuery()

                ' ''UPDATE UTAMA
                ''Dim ftDetail As String = "", statusOut As Integer = 0
                'Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idkw FROM m_11_kw_detail WHERE " & updFilter & " GROUP BY idkw")
                ''If dtOut.Rows.Count > 0 Then
                ''    For Each dr1 As DataRow In dtOut.Rows
                ''        ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                ''        ftDetail = String.Concat(ftDetail, "(idic = '" & dr1("idic") & "')")
                ''    Next
                ''End If
                ''dtOut = AsDataTableAmbilDariDBCon("SELECT idic, SUM(jmlbayar) as jmlbayar, SUM(jmlpv) as jmlpv FROM m5_ic_detail WHERE " & ftDetail & " GROUP BY idic")
                'If dtOut.Rows.Count > 0 Then
                '    '    'KOSONGKAN VARIABEL NILAI DAN FILTER
                '    updNilai = "" : updFilter = ""
                '    For Each dr1 As DataRow In dtOut.Rows
                '        '        '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                '        '        If dr1("jmlpv") >= dr1("jmlbayar") Then
                '        '            statusOut = 2
                '        '        ElseIf dr1("jmlpv") < 1 Then
                '        '            statusOut = 0
                '        '        Else
                '        '            statusOut = 1
                '        '        End If
                '        '        '2. SET NILAI UPDATE OUTSTANDING
                '        '        updNilai = String.Concat(updNilai, "WHEN '" & dr1("idic") & "' THEN '" & statusOut & "' ")
                '        '        '3. SET FILTERUPDATE OUTSTANDING
                '        updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                '        updFilter = String.Concat(updFilter, "(kwid = '" & dr1("idkw") & "')")
                '    Next

                '    sql = "UPDATE m_11_kw SET kwstatuspb = 0 WHERE " & updFilter
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                ''End If
                ''END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PB' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M_11_Pb SET Pvstatus = " & nilaiStatus & ", Pvmodifikasiuser='" & userid & "', Pvmodifikasitgl = NOW(), Pvposting = 0, Pvpostingtgl = '1971-01-01 00:00:00', Pvjmlrevisi = Pvjmlrevisi + 1 WHERE Pvid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_PvSearch(PostWsSearch(paramSplit(0), "M11_PvSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_PvDelete(ByVal param As String) As String

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
            Dim sumber As String = "Pv", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pvid, Pvnotransaksi FROM M5_Pv WHERE Pvid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pvcabang, pvlokasi, pvsumber, pvautonotransaksi, pvnotransaksi, pvtgl"
            sql &= " FROM M5_pv"
            sql &= " WHERE pvid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pvcabang")
                lokasi = dtNomorNext.Rows(0)("pvlokasi")
                sumber = dtNomorNext.Rows(0)("pvsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pvautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pvnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pvtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Pv_Detail WHERE idpv='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Pv WHERE pvid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_PvSearch(PostWsSearch(paramSplit(0), "M11_PvSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M11_PvGetdataById(ByVal param As String) As String
        'M11_PvGetdataById Utama --------------------------------------------------------
        'pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, 
        'pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, 
        'pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, 
        'pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, 
        'pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, 
        'pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, 
        'pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, 
        'pvisclose, pvcustomtext1, pvcustomtext2, pvcustomtext3, pvcustomtext4, pvcustomtext5, pvcustomint1, 
        'pvcustomint2, pvcustomint3, pvcustomdbl1, pvcustomdbl2, pvcustomdbl3, pvcustomdate1, pvcustomdate2, 
        'pvcustomdate3, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, 
        'pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, pvnotransaksiic, 
        'pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama

        'M11_PvGetdataById Detail --------------------------------------------------------
        'idpvdetail, idpv, sumber, idtransaksi, matauang, 
        'kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, 
        'jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, 
        'subdivisi, proyek, idicdetail, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, 
        'diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl

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

        Dim NmMemcached As String = "aplikasi1-M11_Pv~M11_Pv_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pvid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pvid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("M11_Pb_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pvid"), 0), sptField,
                     FxDB(drutama("pvcabang"), ""), sptField,
                     FxDB(drutama("pvlokasi"), ""), sptField,
                     FxDB(drutama("pvgudang"), ""), sptField,
                     FxDB(drutama("pvsumber"), ""), sptField,
                     FxDB(drutama("pvautonotransaksi"), 0), sptField,
                     FxDB(drutama("pvnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pvtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pvkodepa"), 0), sptField,
                     FxDB(drutama("pvcustomer"), 0), sptField,
                     FxDB(drutama("pvcustomerkontak"), ""), sptField,
                     FxDB(drutama("pv1alamat1"), ""), sptField,
                     FxDB(drutama("pv1alamat2"), ""), sptField,
                     FxDB(drutama("pv1alamat3"), ""), sptField,
                     FxDB(drutama("pv2alamat1"), ""), sptField,
                     FxDB(drutama("pv2alamat2"), ""), sptField,
                     FxDB(drutama("pv2alamat3"), ""), sptField,
                     FxDB(drutama("pvbagianpenjualan"), 0), sptField,
                     FxDB(drutama("pvbagianterima"), 0), sptField,
                     FxDB(drutama("pvuraian"), ""), sptField,
                     FxDB(drutama("pvcatatan"), ""), sptField,
                     FxDB(drutama("pvnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pvtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pvcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvtglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("pvmatauang"), ""), sptField,
                     FxDB(drutama("pvkurs"), 0), sptField,
                     FxDB(drutama("pvtotalap"), 0), sptField,
                     FxDB(drutama("pvtotalapvalas"), 0), sptField,
                     FxDB(drutama("pvtotalar"), 0), sptField,
                     FxDB(drutama("pvtotalarvalas"), 0), sptField,
                     FxDB(drutama("pvbayar"), 0), sptField,
                     FxDB(drutama("pvbayarvalas"), 0), sptField,
                     FxDB(drutama("pvselisihkurs"), 0), sptField,
                     FxDB(drutama("pvrekselisihkurs"), ""), sptField,
                     FxDB(drutama("pvdiskontermin"), 0), sptField,
                     FxDB(drutama("pvdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("pvrekdiskontermin"), ""), sptField,
                     FxDB(drutama("pvidic"), 0), sptField,
                     FxDB(drutama("pvstatus"), 0), sptField,
                     FxDB(drutama("pvstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pvjmlrevisi"), 0), sptField,
                     FxDB(drutama("pvcetakanke"), 0), sptField,
                     FxDB(drutama("pvinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pvmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pvposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pvisclose"), 0), sptField,
                     FxDB(drutama("pvcustomtext1"), ""), sptField,
                     FxDB(drutama("pvcustomtext2"), ""), sptField,
                     FxDB(drutama("pvcustomtext3"), ""), sptField,
                     FxDB(drutama("pvcustomtext4"), ""), sptField,
                     FxDB(drutama("pvcustomtext5"), ""), sptField,
                     FxDB(drutama("pvcustomint1"), 0), sptField,
                     FxDB(drutama("pvcustomint2"), 0), sptField,
                     FxDB(drutama("pvcustomint3"), 0), sptField,
                     FxDB(drutama("pvcustomdbl1"), 0), sptField,
                     FxDB(drutama("pvcustomdbl2"), 0), sptField,
                     FxDB(drutama("pvcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pvcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pvcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pvcabangnama"), ""), sptField,
                     FxDB(drutama("pvlokasinama"), ""), sptField,
                     FxDB(drutama("pvgudangnama"), ""), sptField,
                     FxDB(drutama("pvcustomerkode"), ""), sptField,
                     FxDB(drutama("pvcustomernama"), ""), sptField,
                     FxDB(drutama("pvbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("pvbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("pvbagianterimakode"), ""), sptField,
                     FxDB(drutama("pvbagianterimanama"), ""), sptField,
                     FxDB(drutama("pvcarabayarnama"), ""), sptField,
                     FxDB(drutama("pvrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("pvrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("pvnotransaksiic"), ""), sptField,
                     FxDB(drutama("pvstatusnama"), ""), sptField,
                     FxDB(drutama("pvstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pvinputusernama"), ""), sptField,
                     FxDB(drutama("pvmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("pvpetugas"), 0), sptField,
                     FxDB(drutama("pvpetugaskode"), ""), sptField,
                     FxDB(drutama("pvpetugasnama"), ""))

            For Each dr As DataRow In dt.Rows
                Dim tglgiro As String = FxDB(dr("tgljtgiro"), "")
                If Len(tglgiro) > 0 Then tglgiro = AsFormatTanggal(FxDB(dr("tgljtgiro"), ""), formatTgl) Else tglgiro = tglgiro

                detail = String.Concat(detail, FxDB(dr("idpvdetail"), 0), sptField,
                     FxDB(dr("idpv"), 0), sptField,
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
                     FxDB(dr("nogiro"), ""), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("idicdetail"), 0), sptField,
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
                     tglgiro, sptField,
                     FxDB(dr("notransaksiic"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("uraian"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, pvisclose, pvcustomtext1, pvcustomtext2, pvcustomtext3, pvcustomtext4, pvcustomtext5, pvcustomint1, pvcustomint2, pvcustomint3, pvcustomdbl1, pvcustomdbl2, pvcustomdbl3, pvcustomdate1, pvcustomdate2, pvcustomdate3, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, pvnotransaksiic, pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama, pvpetugas, pvpetugaskode, pvpetugasnama" & sptSubParam & "idpvdetail, idpv, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idicdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl, uraian"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_PvSearch(ByVal param As String) As String
        'M11_PvSearch --------------------------------------------------------
        'pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, 
        'pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, 
        'pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, 
        'pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, 
        'pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, 
        'pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, 
        'pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, 
        'pvisclose, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, 
        'pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, icnotransaksi, 
        'pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama

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
            Filter = Filter.Replace("pvnorm", "p.pkode")
            Filter = Filter.Replace("pvnama", "p.pnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("M11_Pb_v")

        dt = AmbilData("aplikasi1-M11_Pb_V", Filter, "pvinputtgl desc", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pvid"), 0), sptField,
                     FxDB(dr("pvcabang"), ""), sptField,
                     FxDB(dr("pvlokasi"), ""), sptField,
                     FxDB(dr("pvgudang"), ""), sptField,
                     FxDB(dr("pvsumber"), ""), sptField,
                     FxDB(dr("pvautonotransaksi"), 0), sptField,
                     FxDB(dr("pvnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pvtgl"), ""), formatTgl), sptField,
                     FxDB(dr("pvkodepa"), 0), sptField,
                     FxDB(dr("pvcustomer"), 0), sptField,
                     FxDB(dr("pvcustomerkontak"), ""), sptField,
                     FxDB(dr("pv1alamat1"), ""), sptField,
                     FxDB(dr("pv1alamat2"), ""), sptField,
                     FxDB(dr("pv1alamat3"), ""), sptField,
                     FxDB(dr("pv2alamat1"), ""), sptField,
                     FxDB(dr("pv2alamat2"), ""), sptField,
                     FxDB(dr("pv2alamat3"), ""), sptField,
                     FxDB(dr("pvbagianpenjualan"), 0), sptField,
                     FxDB(dr("pvbagianterima"), 0), sptField,
                     FxDB(dr("pvuraian"), ""), sptField,
                     FxDB(dr("pvcatatan"), ""), sptField,
                     FxDB(dr("pvnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pvtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pvcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvtglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("pvmatauang"), ""), sptField,
                     FxDB(dr("pvkurs"), 0), sptField,
                     FxDB(dr("pvtotalap"), 0), sptField,
                     FxDB(dr("pvtotalapvalas"), 0), sptField,
                     FxDB(dr("pvtotalar"), 0), sptField,
                     FxDB(dr("pvtotalarvalas"), 0), sptField,
                     FxDB(dr("pvbayar"), 0), sptField,
                     FxDB(dr("pvbayarvalas"), 0), sptField,
                     FxDB(dr("pvselisihkurs"), 0), sptField,
                     FxDB(dr("pvrekselisihkurs"), ""), sptField,
                     FxDB(dr("pvdiskontermin"), 0), sptField,
                     FxDB(dr("pvdiskonterminvalas"), 0), sptField,
                     FxDB(dr("pvrekdiskontermin"), ""), sptField,
                     FxDB(dr("pvidic"), 0), sptField,
                     FxDB(dr("pvstatus"), 0), sptField,
                     FxDB(dr("pvstatussebelumnya"), 0), sptField,
                     FxDB(dr("pvjmlrevisi"), 0), sptField,
                     FxDB(dr("pvcetakanke"), 0), sptField,
                     FxDB(dr("pvinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pvmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pvposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pvisclose"), 0), sptField,
                     FxDB(dr("pvcabangnama"), ""), sptField,
                     FxDB(dr("pvlokasinama"), ""), sptField,
                     FxDB(dr("pvgudangnama"), ""), sptField,
                     FxDB(dr("pvcustomerkode"), ""), sptField,
                     FxDB(dr("pvcustomernama"), ""), sptField,
                     FxDB(dr("pvbagianpenjualankode"), ""), sptField,
                     FxDB(dr("pvbagianpenjualannama"), ""), sptField,
                     FxDB(dr("pvbagianterimakode"), ""), sptField,
                     FxDB(dr("pvbagianterimanama"), ""), sptField,
                     FxDB(dr("pvcarabayarnama"), ""), sptField,
                     FxDB(dr("pvrekselisihkursnama"), ""), sptField,
                     FxDB(dr("pvrekdiskonterminnama"), ""), sptField,
                     FxDB(dr("icnotransaksi"), ""), sptField,
                     FxDB(dr("pvstatusnama"), ""), sptField,
                     FxDB(dr("pvstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pvinputusernama"), ""), sptField,
                     FxDB(dr("pvmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, pvisclose, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, icnotransaksi, pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_PvTerkait(ByVal param As String) As String
        'M11_PvTerkait --------------------------------------------------------
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "pbid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND pvid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "pvid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("M11_Pv_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pbid"), 0), sptField,
                     FxDB(dr("pbnotransaksi"), ""), sptField,
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
            result(2) = "Related PB data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pbid, pbnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal MUFungsional As String, _
                                    ByVal ftExistOutstandingSI As String, ByVal ftOutstandingSI As String, _
                                    ByVal ftExistOutstandingAS As String, ByVal ftOutstandingAS As String, _
                                    ByVal ftExistOutstandingSR As String, ByVal ftOutstandingSR As String, _
                                    ByVal ftExistOutstandingRP As String, ByVal ftOutstandingRP As String, _
                                    ByVal ftExistOutstandingIP As String, ByVal ftOutstandingIP As String, _
                                    ByVal updFilterSI As String, ByVal updFilterAS As String, ByVal updFilterSR As String, _
                                    ByVal updFilterRP As String, ByVal updFilterIP As String, _
                                    ByVal formatTgl As String, ByVal tglPembayaran As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, sumber As String = "", notransaksi As String = "", matauang As String = "", tgl As String = ""
        Dim filterLookup As String = "", urutan As String = "", sisa As Double = 0

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idicdetail, sumber, notransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("sumber")
                notransaksi = dtval.Rows(0)("notransaksi")

                filterLookup = "idkwdetail=" & dtval.Rows(0)("idkwdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in KW" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBAYAR YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        'sql = "SELECT kwd.idkwdetail, (kwd.jmlbayar - kwd.jmlpv) as sisapv, (icd.jmlbayarvalas - icd.jmlpvvalas) as sisapvvalas, icd.matauang, icd.sumber, (CASE icd.sumber WHEN 'AS' THEN `as`.asnotransaksi WHEN 'SI' THEN si.sinotransaksi WHEN 'SR' THEN sr.srnotransaksi ELSE icd.rekhutangpiutang END) as notransaksi FROM m5_ic_detail AS icd LEFT JOIN m5_as `as` ON icd.sumber = 'AS' AND icd.idtransaksi = `as`.asid LEFT JOIN m5_si si ON icd.sumber = 'SI' AND icd.idtransaksi = si.siid LEFT JOIN m5_sr sr ON icd.sumber = 'SR' AND icd.idtransaksi = sr.srid WHERE " & ftOutstanding
        'dtval = AsDataTableAmbilDariDB(sql)
        'If dtval.Rows.Count > 0 Then
        '    'Ambil informasi utk errmessage
        '    sumber = dtval.Rows(0)("sumber")
        '    notransaksi = dtval.Rows(0)("notransaksi")
        '    matauang = dtval.Rows(0)("matauang")
        '    If matauang = MUFungsional Then sisa = dtval.Rows(0)("sisapv") Else sisa = dtval.Rows(0)("sisapvvalas")

        '    filterLookup = "idicdetail=" & dtval.Rows(0)("idicdetail")
        '    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '    If dtLookup.Rows.Count > 0 Then
        '        urutan = dtLookup.Rows(0)("urutan")
        '    End If
        '    errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in IC, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        'End If
        'END OF VALIDASI OUTSTANDING --------------------------------


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'SI
        'If Len(ftExistOutstandingSI) > 0 Then 'ftExistOutstanding = rowExists, siid, sisumber, sinotransaksi
        '    'CEK DATA EXIST/TIDAK
        '    dtval = AsDataTableAmbilDariDB(ftExistOutstandingSI)
        '    filterLookup = "rowExists = 0"
        '    dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("sinotransaksi")
        '        sumber = dtval.Rows(0)("sisumber")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("sisumber") & "' AND idtransaksi = '" & dtval.Rows(0)("siid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in SI" : GoTo selesai
        '    End If
        'End If

        ''CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        'If Len(updFilterSI) > 0 Then
        '    sql = "SELECT si.siid, si.sisumber, si.sitgl, si.sinotransaksi FROM m5_si si WHERE si.sitgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterSI & ")"
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        sumber = dtval.Rows(0)("sisumber")
        '        notransaksi = dtval.Rows(0)("sinotransaksi")
        '        tgl = dtval.Rows(0)("sitgl")

        '        filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("siid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
        '    End If
        'End If

        ''PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        'If Len(ftOutstandingSI) > 0 Then
        '    sql = "SELECT si.siid, si.sisumber, si.sinotransaksi, si.simatauang, si.sitotaltransaksi - si.sijmlbayar as sisisatransaksi FROM m5_si si LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingSI
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("sinotransaksi")
        '        sumber = dtval.Rows(0)("sisumber")
        '        sisa = dtval.Rows(0)("sisisatransaksi")
        '        matauang = dtval.Rows(0)("simatauang")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("sisumber") & "' AND idtransaksi = '" & dtval.Rows(0)("siid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        If dtLookup.Rows.Count > 0 Then
        '            urutan = dtLookup.Rows(0)("urutan")
        '        End If
        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in SI, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        '    End If
        'End If
        ''END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        ''VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        ''AS
        'If Len(ftExistOutstandingAS) > 0 Then 'ftExistOutstanding = rowExists, asid, assumber, asnotransaksi
        '    'CEK DATA EXIST/TIDAK
        '    dtval = AsDataTableAmbilDariDB(ftExistOutstandingAS)
        '    filterLookup = "rowExists = 0"
        '    dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("asnotransaksi")
        '        sumber = dtval.Rows(0)("assumber")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("assumber") & "' AND idtransaksi = '" & dtval.Rows(0)("asid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in AS" : GoTo selesai
        '    End If
        'End If

        ''CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        'If Len(updFilterAS) > 0 Then
        '    sql = "SELECT m5as.asid, m5as.assumber, m5as.astgl, m5as.asnotransaksi FROM m5_as m5as WHERE m5as.astgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterAS & ")"
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        sumber = dtval.Rows(0)("assumber")
        '        notransaksi = dtval.Rows(0)("asnotransaksi")
        '        tgl = dtval.Rows(0)("astgl")

        '        filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("asid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
        '    End If
        'End If

        ''PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        'If Len(ftOutstandingAS) > 0 Then
        '    sql = "SELECT m5as.asid, m5as.assumber, m5as.asnotransaksi, m5as.asmatauang, (CASE m5as.asmatauang WHEN s.snilai THEN m5as.asjumlah - m5as.asjumlahbayar ELSE m5as.asjumlahvalas - m5as.asjumlahbayarvalas END) assisatransaksi FROM m5_as as LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingAS
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("asnotransaksi")
        '        sumber = dtval.Rows(0)("assumber")
        '        sisa = dtval.Rows(0)("assisatransaksi")
        '        matauang = dtval.Rows(0)("asmatauang")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("assumber") & "' AND idtransaksi = '" & dtval.Rows(0)("asid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        If dtLookup.Rows.Count > 0 Then
        '            urutan = dtLookup.Rows(0)("urutan")
        '        End If
        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in AS, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        '    End If
        'End If
        ''END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        ''VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        ''SR
        'If Len(ftExistOutstandingSR) > 0 Then 'ftExistOutstanding = rowExists, srid, srsumber, srnotransaksi
        '    'CEK DATA EXIST/TIDAK
        '    dtval = AsDataTableAmbilDariDB(ftExistOutstandingSR)
        '    filterLookup = "rowExists = 0"
        '    dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("srnotransaksi")
        '        sumber = dtval.Rows(0)("srsumber")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("srsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("srid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in SR" : GoTo selesai
        '    End If
        'End If

        ''CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        'If Len(updFilterSR) > 0 Then
        '    sql = "SELECT sr.srid, sr.srsumber, sr.srtgl, sr.srnotransaksi FROM m5_sr sr WHERE sr.srtgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterSR & ")"
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        sumber = dtval.Rows(0)("srsumber")
        '        notransaksi = dtval.Rows(0)("srnotransaksi")
        '        tgl = dtval.Rows(0)("srtgl")

        '        filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("srid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
        '    End If
        'End If

        ''PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        'If Len(ftOutstandingSR) > 0 Then
        '    sql = "SELECT sr.srid, sr.srsumber, sr.srnotransaksi, sr.srmatauang, sr.srtotaltransaksi - sr.srjmlbayar as srsisatransaksi FROM m5_sr sr LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingSR
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("srnotransaksi")
        '        sumber = dtval.Rows(0)("srsumber")
        '        sisa = dtval.Rows(0)("srsisatransaksi")
        '        matauang = dtval.Rows(0)("srmatauang")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("srsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("srid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        If dtLookup.Rows.Count > 0 Then
        '            urutan = dtLookup.Rows(0)("urutan")
        '        End If
        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in SR, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        '    End If
        'End If
        ''END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        ''VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        ''RP
        'If Len(ftExistOutstandingRP) > 0 Then 'ftExistOutstanding = rowExists, rpid, rpsumber, rpnotransaksi
        '    'CEK DATA EXIST/TIDAK
        '    dtval = AsDataTableAmbilDariDB(ftExistOutstandingRP)
        '    filterLookup = "rowExists = 0"
        '    dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("rpnotransaksi")
        '        sumber = dtval.Rows(0)("rpsumber")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("rpsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("rpid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in RP" : GoTo selesai
        '    End If
        'End If

        ''CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        'If Len(updFilterRP) > 0 Then
        '    sql = "SELECT rp.rpid, rp.rpsumber, rp.rptgl, rp.rpnotransaksi FROM m5_rp rp WHERE rp.rptgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterRP & ")"
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        sumber = dtval.Rows(0)("rpsumber")
        '        notransaksi = dtval.Rows(0)("rpnotransaksi")
        '        tgl = dtval.Rows(0)("rptgl")

        '        filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("rpid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
        '    End If
        'End If

        ''PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        'If Len(ftOutstandingRP) > 0 Then
        '    sql = "SELECT rp.rpid, rp.rpsumber, rp.rpnotransaksi, rp.rpmatauang, (CASE rp.rpmatauang WHEN s.snilai THEN rp.rpjumlah - rp.rpjumlahbayar ELSE rp.rpjumlahvalas - rp.rpjumlahbayarvalas END) rpsisatransaksi FROM m5_rp rp LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingRP
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("rpnotransaksi")
        '        sumber = dtval.Rows(0)("rpsumber")
        '        sisa = dtval.Rows(0)("rpsisatransaksi")
        '        matauang = dtval.Rows(0)("rpmatauang")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("rpsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("rpid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        If dtLookup.Rows.Count > 0 Then
        '            urutan = dtLookup.Rows(0)("urutan")
        '        End If
        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in RP, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        '    End If
        'End If
        ''END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        ''VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        ''IP
        'If Len(ftExistOutstandingIP) > 0 Then 'ftExistOutstanding = rowExists, ipid, ipsumber, ipnotransaksi
        '    'CEK DATA EXIST/TIDAK
        '    dtval = AsDataTableAmbilDariDB(ftExistOutstandingIP)
        '    filterLookup = "rowExists = 0"
        '    dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("ipnotransaksi")
        '        sumber = dtval.Rows(0)("ipsumber")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("ipsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("ipid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in IP" : GoTo selesai
        '    End If
        'End If

        ''CEK TGL TRANSAKSI DETAIL TIDAK BOLEH LEBIH DARI TANGGAL PEMBAYARAN
        'If Len(updFilterIP) > 0 Then
        '    sql = "SELECT ip.ipid, ip.ipsumber, ip.iptgl, ip.ipnotransaksi FROM m5_ip ip WHERE ip.iptgl > '" & AsFormatTanggal(tglPembayaran) & "' AND (" & updFilterIP & ")"
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        sumber = dtval.Rows(0)("ipsumber")
        '        notransaksi = dtval.Rows(0)("ipnotransaksi")
        '        tgl = dtval.Rows(0)("iptgl")

        '        filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("ipid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        urutan = dtLookup.Rows(0)("urutan")

        '        errmessage = "Row : " & urutan & " - " & sumber & " : Date of " & notransaksi & " (" & AsFormatTanggal(tgl, formatTgl) & ") is more than date of payment (" & AsFormatTanggal(tglPembayaran, formatTgl) & ")" : GoTo selesai
        '    End If
        'End If

        ''PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        'If Len(ftOutstandingIP) > 0 Then
        '    sql = "SELECT ip.ipid, ip.ipsumber, ip.ipnotransaksi, ip.ipmatauang, (CASE ip.ipmatauang WHEN s.snilai THEN ip.ipjumlah - ip.ipjumlahbayar ELSE ip.ipjumlahvalas - ip.ipjumlahbayarvalas END) ipsisatransaksi FROM m5_ip ip LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingIP
        '    dtval = AsDataTableAmbilDariDB(sql)
        '    If dtval.Rows.Count > 0 Then
        '        'Ambil informasi utk errmessage
        '        notransaksi = dtval.Rows(0)("ipnotransaksi")
        '        sumber = dtval.Rows(0)("ipsumber")
        '        sisa = dtval.Rows(0)("ipsisatransaksi")
        '        matauang = dtval.Rows(0)("ipmatauang")

        '        filterLookup = "sumber = '" & dtval.Rows(0)("ipsumber") & "' AND idtransaksi = '" & dtval.Rows(0)("ipid") & "'"
        '        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
        '        If dtLookup.Rows.Count > 0 Then
        '            urutan = dtLookup.Rows(0)("urutan")
        '        End If
        '        errmessage = "Row : " & urutan & " - " & sumber & " : " & notransaksi & " exceeds the amount of payment in IP, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
        '    End If
        'End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------

selesai:
        Return errmessage
    End Function

End Class