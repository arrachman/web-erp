Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_ic
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""


    <WebMethod()>
    Public Function M5_IcSimpan(ByVal param As String) As String
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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'icid(0) As Integer, iccabang(1) As String, iclokasi(2) As String, icgudang(3) As String, icsumber(4) As String, 
        'icautonotransaksi(5) As Integer, icnotransaksi(6) As String, ictgl(7) As Date, ickodepa(8) As Integer, iccustomer(9) As Integer, 
        'iccustomerkontak(10) As String, ic1alamat1(11) As String, ic1alamat2(12) As String, ic1alamat3(13) As String, ic2alamat1(14) As String, 
        'ic2alamat2(15) As String, ic2alamat3(16) As String, icbagianpenjualan(17) As Integer, icbagianpenagihan(18) As Integer, icuraian(19) As String, 
        'iccatatan(20) As String, icnoref(21) As String, ictglnoref(22) As Date, iccarabayar(23) As Integer, ictglbayar(24) As Date, 
        'icmatauang(25) As String, ickurs(26) As Double, ictotalap(27) As Double, ictotalapvalas(28) As Double, ictotalar(29) As Double, 
        'ictotalarvalas(30) As Double, icjmltagih(31) As Double, icjmltagihvalas(32) As Double, icbayar(33) As Double, icbayarvalas(34) As Double, 
        'icselisihkurs(35) As Double, icrekselisihkurs(36) As String, icdiskontermin(37) As Double, icdiskonterminvalas(38) As Double, icrekdiskontermin(39) As String, 
        'icstatuspv(40) As Integer, icstatus(41) As Integer, icstatussebelumnya(42) As Integer, icjmlrevisi(43) As Integer, iccetakanke(44) As Integer, 
        'icinputuser(45) As Integer, icinputtgl(46) As DateTime, icmodifikasiuser(47) As Integer, icmodifikasitgl(48) As DateTime, icisclose(49) As Integer, 
        'iccustomtext1(50) As String, iccustomtext2(51) As String, iccustomtext3(52) As String, iccustomtext4(53) As String, iccustomtext5(54) As String, 
        'iccustomint1(55) As Integer, iccustomint2(56) As Integer, iccustomint3(57) As Integer, iccustomdbl1(58) As Double, iccustomdbl2(59) As Double, 
        'iccustomdbl3(60) As Double, iccustomdate1(61) As Date, iccustomdate2(62) As Date, iccustomdate3(63) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, 
        'iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, 
        'iccustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 64) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'icid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "icid required numeric." : GoTo selesai
        End If
        'icautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "icautonotransaksi required numeric." : GoTo selesai
        End If
        'ictgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ictgl required date." : GoTo selesai
        End If
        'ickodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ickodepa required numeric." : GoTo selesai
        End If
        'iccustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "iccustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "iccustomer can't be empty." : GoTo selesai
        End If
        'icbagianpenjualan(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "icbagianpenjualan required numeric." : GoTo selesai
        End If
        'icbagianpenagihan(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "icbagianpenagihan required numeric." : GoTo selesai
        End If
        'ictglnoref(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "ictglnoref required date." : GoTo selesai
        End If
        'iccarabayar(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "iccarabayar required numeric." : GoTo selesai
        End If
        'ictglbayar(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "ictglbayar required date." : GoTo selesai
        End If
        'ickurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ickurs required numeric." : GoTo selesai
        End If
        'ictotalap(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ictotalap required numeric." : GoTo selesai
        End If
        'ictotalapvalas(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ictotalapvalas required numeric." : GoTo selesai
        End If
        'ictotalar(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ictotalar required numeric." : GoTo selesai
        End If
        'ictotalarvalas(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "ictotalarvalas required numeric." : GoTo selesai
        End If
        'icjmltagih(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "icjmltagih required numeric." : GoTo selesai
        End If
        'icjmltagihvalas(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "icjmltagihvalas required numeric." : GoTo selesai
        End If
        'icbayar(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "icbayar required numeric." : GoTo selesai
        End If
        'icbayarvalas(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "icbayarvalas required numeric." : GoTo selesai
        End If
        'icselisihkurs(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "icselisihkurs required numeric." : GoTo selesai
        End If
        'icdiskontermin(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "icdiskontermin required numeric." : GoTo selesai
        End If
        'icdiskonterminvalas(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "icdiskonterminvalas required numeric." : GoTo selesai
        End If
        'icstatuspv(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "icstatuspv required numeric." : GoTo selesai
        End If
        'icstatus(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "icstatus required numeric." : GoTo selesai
        End If
        'icstatussebelumnya(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "icstatussebelumnya required numeric." : GoTo selesai
        End If
        'icjmlrevisi(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "icjmlrevisi required numeric." : GoTo selesai
        End If
        'iccetakanke(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "iccetakanke required numeric." : GoTo selesai
        End If
        'icinputuser(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "icinputuser required numeric." : GoTo selesai
        End If
        'icinputtgl(46) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "icinputtgl required date." : GoTo selesai
        End If
        'icmodifikasiuser(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "icmodifikasiuser required numeric." : GoTo selesai
        End If
        'icmodifikasitgl(48) As DateTime
        If (IsDate(dataUtama(48)) = False) Then
            result(2) = "icmodifikasitgl required date." : GoTo selesai
        End If
        'icisclose(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "icisclose required numeric." : GoTo selesai
        End If
        'iccustomint1(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "iccustomint1 required numeric." : GoTo selesai
        End If
        'iccustomint2(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "iccustomint2 required numeric." : GoTo selesai
        End If
        'iccustomint3(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "iccustomint3 required numeric." : GoTo selesai
        End If
        'iccustomdbl1(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "iccustomdbl1 required numeric." : GoTo selesai
        End If
        'iccustomdbl2(59) As Double
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "iccustomdbl2 required numeric." : GoTo selesai
        End If
        'iccustomdbl3(60) As Double
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "iccustomdbl3 required numeric." : GoTo selesai
        End If
        'iccustomdate1(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "iccustomdate1 required date." : GoTo selesai
        End If
        'iccustomdate2(62) As Date
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "iccustomdate2 required date." : GoTo selesai
        End If
        'iccustomdate3(63) As Date
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "iccustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'iccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "iccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "iccabang should not be more than 25 character." : GoTo selesai
        End If

        'iclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "iclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "iclokasi should not be more than 25 character." : GoTo selesai
        End If

        'icsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "icsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "icsumber should not be more than 10 character." : GoTo selesai
        End If

        'icnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "icnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "icnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ictgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ictgl can't be empty" : GoTo selesai
        End If

        'ictglnoref(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "ictglnoref can't be empty" : GoTo selesai
        End If

        'ictglbayar(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "ictglbayar can't be empty" : GoTo selesai
        End If

        'icmatauang(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "icmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "icmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ickurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "ickurs can't be empty" : GoTo selesai
        End If

        'ictotalap(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "ictotalap can't be empty" : GoTo selesai
        End If

        'ictotalapvalas(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ictotalapvalas can't be empty" : GoTo selesai
        End If

        'ictotalar(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ictotalar can't be empty" : GoTo selesai
        End If

        'ictotalarvalas(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ictotalarvalas can't be empty" : GoTo selesai
        End If

        'icjmltagih(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "icjmltagih can't be empty" : GoTo selesai
        End If

        'icjmltagihvalas(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "icjmltagihvalas can't be empty" : GoTo selesai
        End If

        'icbayar(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "icbayar can't be empty" : GoTo selesai
        End If

        'icbayarvalas(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "icbayarvalas can't be empty" : GoTo selesai
        End If

        'icselisihkurs(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "icselisihkurs can't be empty" : GoTo selesai
        End If

        'icdiskontermin(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "icdiskontermin can't be empty" : GoTo selesai
        End If

        'icdiskonterminvalas(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "icdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'icinputtgl(46) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "icinputtgl can't be empty" : GoTo selesai
        End If

        'icmodifikasitgl(48) As DateTime
        If Len(dataUtama(48)) = 0 Then
            result(2) = "icmodifikasitgl can't be empty" : GoTo selesai
        End If

        'iccustomdbl1(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "iccustomdbl1 can't be empty" : GoTo selesai
        End If

        'iccustomdbl2(59) As Double
        If Len(dataUtama(59)) = 0 Then
            result(2) = "iccustomdbl2 can't be empty" : GoTo selesai
        End If

        'iccustomdbl3(60) As Double
        If Len(dataUtama(60)) = 0 Then
            result(2) = "iccustomdbl3 can't be empty" : GoTo selesai
        End If

        'iccustomdate1(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "iccustomdate1 can't be empty" : GoTo selesai
        End If

        'iccustomdate2(62) As Date
        If Len(dataUtama(62)) = 0 Then
            result(2) = "iccustomdate2 can't be empty" : GoTo selesai
        End If

        'iccustomdate3(63) As Date
        If Len(dataUtama(63)) = 0 Then
            result(2) = "iccustomdate3 can't be empty" : GoTo selesai
        End If

        ''VALIDASI JUMLAH BAYAR
        ''JIKA TOTAL AP - DISKON TERMIN - TOTAL AR + SELISIH KURS <> 0 MAKA MUNCUL PERINGATAN
        ''               ictotalap(27),           icdiskontermin(37),                ictotalar(29),            icselisihkurs(35)
        'If Double.Parse(dataUtama(27)) - Double.Parse(dataUtama(37)) - Double.Parse(dataUtama(29)) + Double.Parse(dataUtama(35)) <> 0 Then
        '    Dim selisih(2) As String
        '    selisih = F_Nominal(Double.Parse(dataUtama(27)) - Double.Parse(dataUtama(37)) - Double.Parse(dataUtama(29)) + Double.Parse(dataUtama(35)), False).Split(sptSubParam)
        '    result(2) = "Total AR - Total AP must be balance : " & selisih(1) & "" : GoTo selesai
        'End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "icid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ickodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icbagianpenagihan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ictglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ickurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icjmltagih", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icjmltagihvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icrekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icrekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icstatuspv", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "icid~iccabang~iclokasi~icgudang~icsumber~icautonotransaksi~icnotransaksi~ictgl~ickodepa~iccustomer~iccustomerkontak~ic1alamat1~ic1alamat2~ic1alamat3~ic2alamat1~ic2alamat2~ic2alamat3~icbagianpenjualan~icbagianpenagihan~icuraian~iccatatan~icnoref~ictglnoref~iccarabayar~ictglbayar~icmatauang~ickurs~ictotalap~ictotalapvalas~ictotalar~ictotalarvalas~icjmltagih~icjmltagihvalas~icbayar~icbayarvalas~icselisihkurs~icrekselisihkurs~icdiskontermin~icdiskonterminvalas~icrekdiskontermin~icstatuspv~icstatus~icstatussebelumnya~icjmlrevisi~iccetakanke~icinputuser~icinputtgl~icmodifikasiuser~icmodifikasitgl~icisclose~iccustomtext1~iccustomtext2~iccustomtext3~iccustomtext4~iccustomtext5~iccustomint1~iccustomint2~iccustomint3~iccustomdbl1~iccustomdbl2~iccustomdbl3~iccustomdate1~iccustomdate2~iccustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idicdetail(0) As Integer, idic(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, nogiro(14) As String, 
        'rekhutangpiutang(15) As String, catatan(16) As String, costcenter(17) As String, divisi(18) As String, subdivisi(19) As String, 
        'proyek(20) As String, jmlpv(21) As Double, jmlpvvalas(22) As Double, statuspv(23) As Double, urutan(24) As Integer, 
        'isclose(25) As Integer, customtext1(26) As String, customtext2(27) As String, customtext3(28) As String, customdbl1(29) As Double, 
        'customdbl2(30) As Double, customdbl3(31) As Double, customdate1(32) As Date, customdate2(33) As Date, customdate3(34) As Date, rencana(35) As String


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, 
        'jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rencana


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idicdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idic", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpv", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpvvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspv", AsEnumTypeData.AsString)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 36) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idicdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idicdetail required numeric." : GoTo selesai
            End If
            'idic(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idic required numeric." : GoTo selesai
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
            'rencana(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
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
            'jmlpv(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - jmlpv required numeric." : GoTo selesai
            End If
            'jmlpvvalas(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlpvvalas required numeric." : GoTo selesai
            End If
            'statuspv(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statuspv required numeric." : GoTo selesai
            End If
            'urutan(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
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
            If (dataRowDetail(2) <> "SI" And dataRowDetail(2) <> "AS" And dataRowDetail(2) <> "SR" And dataRowDetail(2) <> "CA" And dataRowDetail(2) <> "RP" And dataRowDetail(2) <> "IP") Then
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

            'rencana(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
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
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'jmlpv(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - jmlpv can't be empty" : GoTo selesai
            End If

            'jmlpvvalas(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlpvvalas can't be empty" : GoTo selesai
            End If

            'statuspv(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - statuspv can't be empty" : GoTo selesai
            End If

            'customdbl1(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idicdetail~idic~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~nogiro~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~jmlpv~jmlpvvalas~statuspv~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 13
                Select Case drutama("icstatus")
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


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ictgl")), AsFormatTanggal(drutama("ictgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "icmatauang", "icrekselisihkurs~icrekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================

                If isUpdate Then
                    result(4) = drutama("icid")
                    notransaksi = drutama("icnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(icid), icnotransaksi FROM M5_IC WHERE icid='" & result(4) & "' AND icstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("icautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("iccabang"), drutama("iclokasi"), drutama("icsumber"), drutama("ictgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(icid) FROM M5_IC WHERE icnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_ic_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Ic_HistorySimpan("" & paramSplit(0) & "★M5_Ic_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("icsumber")) & "▼" & FixQuotes(drutama("icid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Ic set iccabang  = '" & FixQuotes(drutama("iccabang")) & "', iclokasi  = '" & FixQuotes(drutama("iclokasi")) & "', icgudang  = '" & FixQuotes(drutama("icgudang")) & "', icsumber  = '" & FixQuotes(drutama("icsumber")) & "', icautonotransaksi  = " & drutama("icautonotransaksi") & ", icnotransaksi  = '" & FixQuotes(notransaksi) & "', ictgl  = '" & FixQuotes(AsFormatTanggal(drutama("ictgl"))) & "', ickodepa  = " & drutama("ickodepa") & ", iccustomer  = " & drutama("iccustomer") & ", iccustomerkontak  = '" & FixQuotes(drutama("iccustomerkontak")) & "', ic1alamat1  = '" & FixQuotes(drutama("ic1alamat1")) & "', ic1alamat2  = '" & FixQuotes(drutama("ic1alamat2")) & "', ic1alamat3  = '" & FixQuotes(drutama("ic1alamat3")) & "', ic2alamat1  = '" & FixQuotes(drutama("ic2alamat1")) & "', ic2alamat2  = '" & FixQuotes(drutama("ic2alamat2")) & "', ic2alamat3  = '" & FixQuotes(drutama("ic2alamat3")) & "', icbagianpenjualan  = " & drutama("icbagianpenjualan") & ", icbagianpenagihan  = " & drutama("icbagianpenagihan") & ", icuraian  = '" & FixQuotes(drutama("icuraian")) & "', iccatatan  = '" & FixQuotes(drutama("iccatatan")) & "', icnoref  = '" & FixQuotes(drutama("icnoref")) & "', ictglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ictglnoref"))) & "', iccarabayar  = " & drutama("iccarabayar") & ", ictglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("ictglbayar"))) & "', icmatauang  = '" & FixQuotes(drutama("icmatauang")) & "', ickurs  = '" & FixDouble(drutama("ickurs")) & "', ictotalap  = '" & FixDouble(drutama("ictotalap")) & "', ictotalapvalas  = '" & FixDouble(drutama("ictotalapvalas")) & "', ictotalar  = '" & FixDouble(drutama("ictotalar")) & "', ictotalarvalas  = '" & FixDouble(drutama("ictotalarvalas")) & "', icjmltagih  = '" & FixDouble(drutama("icjmltagih")) & "', icjmltagihvalas  = '" & FixDouble(drutama("icjmltagihvalas")) & "', icbayar  = '" & FixDouble(drutama("icbayar")) & "', icbayarvalas  = '" & FixDouble(drutama("icbayarvalas")) & "', icselisihkurs  = '" & FixDouble(drutama("icselisihkurs")) & "', icrekselisihkurs  = '" & FixQuotes(drutama("icrekselisihkurs")) & "', icdiskontermin  = '" & FixDouble(drutama("icdiskontermin")) & "', icdiskonterminvalas  = '" & FixDouble(drutama("icdiskonterminvalas")) & "', icrekdiskontermin  = '" & FixQuotes(drutama("icrekdiskontermin")) & "', icstatuspv  = " & drutama("icstatuspv") & ", icstatus  = " & drutama("icstatus") & ", icstatussebelumnya  = " & drutama("icstatussebelumnya") & ", icjmlrevisi  = icjmlrevisi+1, iccetakanke  = " & drutama("iccetakanke") & ", icmodifikasiuser  = " & drutama("icmodifikasiuser") & ", icmodifikasitgl  = NOW(), iccustomtext1  = '" & FixQuotes(drutama("iccustomtext1")) & "', iccustomtext2  = '" & FixQuotes(drutama("iccustomtext2")) & "', iccustomtext3  = '" & FixQuotes(drutama("iccustomtext3")) & "', iccustomtext4  = '" & FixQuotes(drutama("iccustomtext4")) & "', iccustomtext5  = '" & FixQuotes(drutama("iccustomtext5")) & "', iccustomint1  = " & drutama("iccustomint1") & ", iccustomint2  = " & drutama("iccustomint2") & ", iccustomint3  = " & drutama("iccustomint3") & ", iccustomdbl1  = '" & FixDouble(drutama("iccustomdbl1")) & "', iccustomdbl2  = '" & FixDouble(drutama("iccustomdbl2")) & "', iccustomdbl3  = '" & FixDouble(drutama("iccustomdbl3")) & "', iccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate1"))) & "', iccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate2"))) & "', iccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate3"))) & "' where icid = '" & drutama("icid") & "'"
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

                    If drutama("icautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("iccabang"), drutama("iclokasi"), drutama("icsumber"), drutama("ictgl"))
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
                        notransaksi = drutama("icnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(icid) FROM m5_ic WHERE icnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Ic (iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, iccustomdate3) values('" & FixQuotes(drutama("iccabang")) & "', '" & FixQuotes(drutama("iclokasi")) & "', '" & FixQuotes(drutama("icgudang")) & "', '" & FixQuotes(drutama("icsumber")) & "', " & drutama("icautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ictgl"))) & "', " & drutama("ickodepa") & ", " & drutama("iccustomer") & ", '" & FixQuotes(drutama("iccustomerkontak")) & "', '" & FixQuotes(drutama("ic1alamat1")) & "', '" & FixQuotes(drutama("ic1alamat2")) & "', '" & FixQuotes(drutama("ic1alamat3")) & "', '" & FixQuotes(drutama("ic2alamat1")) & "', '" & FixQuotes(drutama("ic2alamat2")) & "', '" & FixQuotes(drutama("ic2alamat3")) & "', " & drutama("icbagianpenjualan") & ", " & drutama("icbagianpenagihan") & ", '" & FixQuotes(drutama("icuraian")) & "', '" & FixQuotes(drutama("iccatatan")) & "', '" & FixQuotes(drutama("icnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ictglnoref"))) & "', " & drutama("iccarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("ictglbayar"))) & "', '" & FixQuotes(drutama("icmatauang")) & "', '" & FixDouble(drutama("ickurs")) & "', '" & FixDouble(drutama("ictotalap")) & "', '" & FixDouble(drutama("ictotalapvalas")) & "', '" & FixDouble(drutama("ictotalar")) & "', '" & FixDouble(drutama("ictotalarvalas")) & "', '" & FixDouble(drutama("icjmltagih")) & "', '" & FixDouble(drutama("icjmltagihvalas")) & "', '" & FixDouble(drutama("icbayar")) & "', '" & FixDouble(drutama("icbayarvalas")) & "', '" & FixDouble(drutama("icselisihkurs")) & "', '" & FixQuotes(drutama("icrekselisihkurs")) & "', '" & FixDouble(drutama("icdiskontermin")) & "', '" & FixDouble(drutama("icdiskonterminvalas")) & "', '" & FixQuotes(drutama("icrekdiskontermin")) & "', " & drutama("icstatuspv") & ", " & drutama("icstatus") & ", " & drutama("icstatussebelumnya") & ", " & drutama("icjmlrevisi") & ", " & drutama("iccetakanke") & ", " & drutama("icinputuser") & ", NOW(), " & drutama("icmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("icisclose") & ", '" & FixQuotes(drutama("iccustomtext1")) & "', '" & FixQuotes(drutama("iccustomtext2")) & "', '" & FixQuotes(drutama("iccustomtext3")) & "', '" & FixQuotes(drutama("iccustomtext4")) & "', '" & FixQuotes(drutama("iccustomtext5")) & "', " & drutama("iccustomint1") & ", " & drutama("iccustomint2") & ", " & drutama("iccustomint3") & ", '" & FixDouble(drutama("iccustomdbl1")) & "', '" & FixDouble(drutama("iccustomdbl2")) & "', '" & FixDouble(drutama("iccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select icid from M5_ic where icnotransaksi='" & notransaksi & "' AND icinputuser= '" & userid & "' order by icmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Ic_Detail where idic = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idicdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(dr1("jmlpv")) & "', '" & FixDouble(dr1("jmlpvvalas")) & "', '" & FixDouble(dr1("statuspv")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Ic_Detail(idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "IC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_IcUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Ic", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ictgl, Icnotransaksi, Icstatus FROM M5_Ic WHERE Icid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Icstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_ic_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Ic_HistorySimpan("" & paramSplit(0) & "★M5_Ic_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_ic_terkait("icid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M5_Ic SET Icstatus = " & nilaiStatus & ", Icmodifikasiuser='" & userid & "', Icmodifikasitgl = NOW(), Icposting = 0, Icpostingtgl = '1971-01-01 00:00:00', Icjmlrevisi = Icjmlrevisi + 1 WHERE Icid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IcSearch(PostWsSearch(paramSplit(0), "M5_icSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_IcDelete(ByVal param As String) As String

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
            Dim sumber As String = "Ic", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Icid, Icnotransaksi FROM M5_Ic WHERE Icid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT iccabang, iclokasi, icsumber, icautonotransaksi, icnotransaksi, ictgl"
            sql &= " FROM M5_ic"
            sql &= " WHERE icid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("iccabang")
                lokasi = dtNomorNext.Rows(0)("iclokasi")
                sumber = dtNomorNext.Rows(0)("icsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("icautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("icnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ictgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Ic_Detail WHERE idic='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Ic WHERE icid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IcSearch(PostWsSearch(paramSplit(0), "M5_IcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_IcGetdataByIdSerenity(ByVal param As String) As String
        'M5_IcGetdataById Utama --------------------------------------------------------
        'icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icposting, icpostingtgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, 
        'iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, 
        'iccustomdate1, iccustomdate2, iccustomdate3, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, 
        'iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, 
        'icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama, kpkp

        'M5_ic_GetdataById Detail --------------------------------------------------------
        'idicdetail, idic, sumber, idtransaksi, 
        'matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, 
        'diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, 
        'divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, 
        'inputtgl

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim icl As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", idtransaksi As String = "", detailSI As String = "", detailIP As String = "", detailAS As String = "", detailSR As String = "", detailCOA As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_ic~M5_ic_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "icid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "icid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        icl = query.PanggilQuery("m5_ic_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , icl) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("icid"), 0), sptField,
                     FxDB(drutama("iccabang"), ""), sptField,
                     FxDB(drutama("iclokasi"), ""), sptField,
                     FxDB(drutama("icgudang"), ""), sptField,
                     FxDB(drutama("icsumber"), ""), sptField,
                     FxDB(drutama("icautonotransaksi"), 0), sptField,
                     FxDB(drutama("icnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ictgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ickodepa"), 0), sptField,
                     FxDB(drutama("iccustomer"), 0), sptField,
                     FxDB(drutama("iccustomerkontak"), ""), sptField,
                     FxDB(drutama("ic1alamat1"), ""), sptField,
                     FxDB(drutama("ic1alamat2"), ""), sptField,
                     FxDB(drutama("ic1alamat3"), ""), sptField,
                     FxDB(drutama("ic2alamat1"), ""), sptField,
                     FxDB(drutama("ic2alamat2"), ""), sptField,
                     FxDB(drutama("ic2alamat3"), ""), sptField,
                     FxDB(drutama("icbagianpenjualan"), 0), sptField,
                     FxDB(drutama("icbagianpenagihan"), 0), sptField,
                     FxDB(drutama("icuraian"), ""), sptField,
                     FxDB(drutama("iccatatan"), ""), sptField,
                     FxDB(drutama("icnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ictglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("iccarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ictglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("icmatauang"), ""), sptField,
                     FxDB(drutama("ickurs"), 0), sptField,
                     FxDB(drutama("ictotalap"), 0), sptField,
                     FxDB(drutama("ictotalapvalas"), 0), sptField,
                     FxDB(drutama("ictotalar"), 0), sptField,
                     FxDB(drutama("ictotalarvalas"), 0), sptField,
                     FxDB(drutama("icjmltagih"), 0), sptField,
                     FxDB(drutama("icjmltagihvalas"), 0), sptField,
                     FxDB(drutama("icbayar"), 0), sptField,
                     FxDB(drutama("icbayarvalas"), 0), sptField,
                     FxDB(drutama("icselisihkurs"), 0), sptField,
                     FxDB(drutama("icrekselisihkurs"), ""), sptField,
                     FxDB(drutama("icdiskontermin"), 0), sptField,
                     FxDB(drutama("icdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("icrekdiskontermin"), ""), sptField,
                     FxDB(drutama("icstatuspv"), 0), sptField,
                     FxDB(drutama("icstatus"), 0), sptField,
                     FxDB(drutama("icstatussebelumnya"), 0), sptField,
                     FxDB(drutama("icjmlrevisi"), 0), sptField,
                     FxDB(drutama("iccetakanke"), 0), sptField,
                     FxDB(drutama("icinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icisclose"), 0), sptField,
                     FxDB(drutama("iccustomtext1"), ""), sptField,
                     FxDB(drutama("iccustomtext2"), ""), sptField,
                     FxDB(drutama("iccustomtext3"), ""), sptField,
                     FxDB(drutama("iccustomtext4"), ""), sptField,
                     FxDB(drutama("iccustomtext5"), ""), sptField,
                     FxDB(drutama("iccustomint1"), 0), sptField,
                     FxDB(drutama("iccustomint2"), 0), sptField,
                     FxDB(drutama("iccustomint3"), 0), sptField,
                     FxDB(drutama("iccustomdbl1"), 0), sptField,
                     FxDB(drutama("iccustomdbl2"), 0), sptField,
                     FxDB(drutama("iccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("iccabangnama"), ""), sptField,
                     FxDB(drutama("iclokasinama"), ""), sptField,
                     FxDB(drutama("icgudangnama"), ""), sptField,
                     FxDB(drutama("iccustomerkode"), ""), sptField,
                     FxDB(drutama("iccustomernama"), ""), sptField,
                     FxDB(drutama("icbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("icbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("icbagianpenagihankode"), ""), sptField,
                     FxDB(drutama("icbagianpenagihannama"), ""), sptField,
                     FxDB(drutama("iccarabayarnama"), ""), sptField,
                     FxDB(drutama("icrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("icrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("icstatusnama"), ""), sptField,
                     FxDB(drutama("icstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("icinputusernama"), ""), sptField,
                     FxDB(drutama("icmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                Dim tglgiro As String = FxDB(dr("tgljtgiro"), "")
                Dim customdate1 As String = FxDB(dr("customdate1"), "")
                Dim customdate2 As String = FxDB(dr("customdate2"), "")
                Dim customdate3 As String = FxDB(dr("customdate3"), "")
                Dim tgl As String = FxDB(dr("tgl"), "")
                Dim tgljatuhtempo As String = FxDB(dr("tgljatuhtempo"), "")
                Dim inputtgl As String = FxDB(dr("inputtgl"), "")

                If Len(tglgiro) > 0 Then tglgiro = AsFormatTanggal(FxDB(dr("tgljtgiro"), ""), formatTgl) Else tglgiro = tglgiro
                If Len(customdate1) > 0 Then customdate1 = AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl) Else customdate1 = customdate1
                If Len(customdate2) > 0 Then customdate2 = AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl) Else customdate2 = customdate2
                If Len(customdate3) > 0 Then customdate3 = AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl) Else customdate3 = customdate3
                If Len(tgl) > 0 Then tgl = AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl) Else tgl = tgl
                If Len(tgljatuhtempo) > 0 Then tgljatuhtempo = AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl) Else tgljatuhtempo = tgljatuhtempo
                If Len(inputtgl) > 0 Then inputtgl = AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu) Else inputtgl = inputtgl

                Dim sumberdetail As String = FxDB(dr("sumber"), "")
                Select Case sumberdetail
                    Case "SI"
                        detailSI = String.Concat(detailSI, FxDB(dr("idicdetail"), 0), sptField,
                                             FxDB(dr("idic"), 0), sptField,
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
                                             FxDB(dr("jmlpv"), 0), sptField,
                                             FxDB(dr("jmlpvvalas"), 0), sptField,
                                             FxDB(dr("statuspv"), 0), sptField,
                                             FxDB(dr("urutan"), 0), sptField,
                                             FxDB(dr("isclose"), 0), sptField,
                                             FxDB(dr("customtext1"), ""), sptField,
                                             FxDB(dr("customtext2"), ""), sptField,
                                             FxDB(dr("customtext3"), ""), sptField,
                                             FxDB(dr("customdbl1"), 0), sptField,
                                             FxDB(dr("customdbl2"), 0), sptField,
                                             FxDB(dr("customdbl3"), 0), sptField,
                                             customdate1, sptField,
                                             customdate2, sptField,
                                             customdate3, sptField,
                                             FxDB(dr("notransaksi"), ""), sptField,
                                             tgl, sptField,
                                             FxDB(dr("carabayar"), 0), sptField,
                                             FxDB(dr("termin"), ""), sptField,
                                             tgljatuhtempo, sptField,
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
                                             inputtgl, sptRow)
                    Case "IP"
                        detailIP = String.Concat(detailIP, FxDB(dr("idicdetail"), 0), sptField,
                                             FxDB(dr("idic"), 0), sptField,
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
                                             FxDB(dr("jmlpv"), 0), sptField,
                                             FxDB(dr("jmlpvvalas"), 0), sptField,
                                             FxDB(dr("statuspv"), 0), sptField,
                                             FxDB(dr("urutan"), 0), sptField,
                                             FxDB(dr("isclose"), 0), sptField,
                                             FxDB(dr("customtext1"), ""), sptField,
                                             FxDB(dr("customtext2"), ""), sptField,
                                             FxDB(dr("customtext3"), ""), sptField,
                                             FxDB(dr("customdbl1"), 0), sptField,
                                             FxDB(dr("customdbl2"), 0), sptField,
                                             FxDB(dr("customdbl3"), 0), sptField,
                                             customdate1, sptField,
                                             customdate2, sptField,
                                             customdate3, sptField,
                                             FxDB(dr("notransaksi"), ""), sptField,
                                             tgl, sptField,
                                             FxDB(dr("carabayar"), 0), sptField,
                                             FxDB(dr("termin"), ""), sptField,
                                             tgljatuhtempo, sptField,
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
                                             inputtgl, sptRow)
                    Case "AS"
                        detailAS = String.Concat(detailAS, FxDB(dr("idicdetail"), 0), sptField,
                                             FxDB(dr("idic"), 0), sptField,
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
                                             FxDB(dr("jmlpv"), 0), sptField,
                                             FxDB(dr("jmlpvvalas"), 0), sptField,
                                             FxDB(dr("statuspv"), 0), sptField,
                                             FxDB(dr("urutan"), 0), sptField,
                                             FxDB(dr("isclose"), 0), sptField,
                                             FxDB(dr("customtext1"), ""), sptField,
                                             FxDB(dr("customtext2"), ""), sptField,
                                             FxDB(dr("customtext3"), ""), sptField,
                                             FxDB(dr("customdbl1"), 0), sptField,
                                             FxDB(dr("customdbl2"), 0), sptField,
                                             FxDB(dr("customdbl3"), 0), sptField,
                                             customdate1, sptField,
                                             customdate2, sptField,
                                             customdate3, sptField,
                                             FxDB(dr("notransaksi"), ""), sptField,
                                             tgl, sptField,
                                             FxDB(dr("carabayar"), 0), sptField,
                                             FxDB(dr("termin"), ""), sptField,
                                             tgljatuhtempo, sptField,
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
                                             inputtgl, sptRow)
                    Case "SR"
                        detailSR = String.Concat(detailSR, FxDB(dr("idicdetail"), 0), sptField,
                                             FxDB(dr("idic"), 0), sptField,
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
                                             FxDB(dr("jmlpv"), 0), sptField,
                                             FxDB(dr("jmlpvvalas"), 0), sptField,
                                             FxDB(dr("statuspv"), 0), sptField,
                                             FxDB(dr("urutan"), 0), sptField,
                                             FxDB(dr("isclose"), 0), sptField,
                                             FxDB(dr("customtext1"), ""), sptField,
                                             FxDB(dr("customtext2"), ""), sptField,
                                             FxDB(dr("customtext3"), ""), sptField,
                                             FxDB(dr("customdbl1"), 0), sptField,
                                             FxDB(dr("customdbl2"), 0), sptField,
                                             FxDB(dr("customdbl3"), 0), sptField,
                                             customdate1, sptField,
                                             customdate2, sptField,
                                             customdate3, sptField,
                                             FxDB(dr("notransaksi"), ""), sptField,
                                             tgl, sptField,
                                             FxDB(dr("carabayar"), 0), sptField,
                                             FxDB(dr("termin"), ""), sptField,
                                             tgljatuhtempo, sptField,
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
                                             inputtgl, sptRow)
                    Case "CA"
                        detailCOA = String.Concat(detailCOA, FxDB(dr("idicdetail"), 0), sptField,
                                             FxDB(dr("idic"), 0), sptField,
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
                                             FxDB(dr("jmlpv"), 0), sptField,
                                             FxDB(dr("jmlpvvalas"), 0), sptField,
                                             FxDB(dr("statuspv"), 0), sptField,
                                             FxDB(dr("urutan"), 0), sptField,
                                             FxDB(dr("isclose"), 0), sptField,
                                             FxDB(dr("customtext1"), ""), sptField,
                                             FxDB(dr("customtext2"), ""), sptField,
                                             FxDB(dr("customtext3"), ""), sptField,
                                             FxDB(dr("customdbl1"), 0), sptField,
                                             FxDB(dr("customdbl2"), 0), sptField,
                                             FxDB(dr("customdbl3"), 0), sptField,
                                             customdate1, sptField,
                                             customdate2, sptField,
                                             customdate3, sptField,
                                             FxDB(dr("notransaksi"), ""), sptField,
                                             tgl, sptField,
                                             FxDB(dr("carabayar"), 0), sptField,
                                             FxDB(dr("termin"), ""), sptField,
                                             tgljatuhtempo, sptField,
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
                                             inputtgl, sptRow)
                End Select
            Next
            If detailSI.Length > 0 Then detailSI = detailSI.Substring(0, detailSI.Length - sptRow.Length) Else detailSI = detailSI
            If detailIP.Length > 0 Then detailIP = detailIP.Substring(0, detailIP.Length - sptRow.Length) Else detailIP = detailIP
            If detailAS.Length > 0 Then detailAS = detailAS.Substring(0, detailAS.Length - sptRow.Length) Else detailAS = detailAS
            If detailSR.Length > 0 Then detailSR = detailSR.Substring(0, detailSR.Length - sptRow.Length) Else detailSR = detailSR
            If detailCOA.Length > 0 Then detailCOA = detailCOA.Substring(0, detailCOA.Length - sptRow.Length) Else detailCOA = detailCOA

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
        strResultData = String.Concat(utama, sptSubParam, detailSI, sptSubParam, detailIP, sptSubParam, detailAS, sptSubParam, detailSR, sptSubParam, detailCOA)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icposting, icpostingtgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, iccustomdate3, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama, kpkp" &
            sptSubParam & "idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl" &
            sptSubParam & "idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl" &
            sptSubParam & "idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl" &
            sptSubParam & "idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl" &
            sptSubParam & "idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IcGetdataById(ByVal param As String) As String
        'M5_IcGetdataById Utama --------------------------------------------------------
        'icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icposting, icpostingtgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, 
        'iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, 
        'iccustomdate1, iccustomdate2, iccustomdate3, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, 
        'iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, 
        'icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama, kpkp

        'M5_ic_GetdataById Detail --------------------------------------------------------
        'idicdetail, idic, sumber, idtransaksi, 
        'matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, 
        'diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, 
        'divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, 
        'inputtgl

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim icl As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_ic~M5_ic_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "icid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "icid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        icl = query.PanggilQuery("m5_ic_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , icl) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("icid"), 0), sptField,
                     FxDB(drutama("iccabang"), ""), sptField,
                     FxDB(drutama("iclokasi"), ""), sptField,
                     FxDB(drutama("icgudang"), ""), sptField,
                     FxDB(drutama("icsumber"), ""), sptField,
                     FxDB(drutama("icautonotransaksi"), 0), sptField,
                     FxDB(drutama("icnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ictgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ickodepa"), 0), sptField,
                     FxDB(drutama("iccustomer"), 0), sptField,
                     FxDB(drutama("iccustomerkontak"), ""), sptField,
                     FxDB(drutama("ic1alamat1"), ""), sptField,
                     FxDB(drutama("ic1alamat2"), ""), sptField,
                     FxDB(drutama("ic1alamat3"), ""), sptField,
                     FxDB(drutama("ic2alamat1"), ""), sptField,
                     FxDB(drutama("ic2alamat2"), ""), sptField,
                     FxDB(drutama("ic2alamat3"), ""), sptField,
                     FxDB(drutama("icbagianpenjualan"), 0), sptField,
                     FxDB(drutama("icbagianpenagihan"), 0), sptField,
                     FxDB(drutama("icuraian"), ""), sptField,
                     FxDB(drutama("iccatatan"), ""), sptField,
                     FxDB(drutama("icnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ictglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("iccarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ictglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("icmatauang"), ""), sptField,
                     FxDB(drutama("ickurs"), 0), sptField,
                     FxDB(drutama("ictotalap"), 0), sptField,
                     FxDB(drutama("ictotalapvalas"), 0), sptField,
                     FxDB(drutama("ictotalar"), 0), sptField,
                     FxDB(drutama("ictotalarvalas"), 0), sptField,
                     FxDB(drutama("icjmltagih"), 0), sptField,
                     FxDB(drutama("icjmltagihvalas"), 0), sptField,
                     FxDB(drutama("icbayar"), 0), sptField,
                     FxDB(drutama("icbayarvalas"), 0), sptField,
                     FxDB(drutama("icselisihkurs"), 0), sptField,
                     FxDB(drutama("icrekselisihkurs"), ""), sptField,
                     FxDB(drutama("icdiskontermin"), 0), sptField,
                     FxDB(drutama("icdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("icrekdiskontermin"), ""), sptField,
                     FxDB(drutama("icstatuspv"), 0), sptField,
                     FxDB(drutama("icstatus"), 0), sptField,
                     FxDB(drutama("icstatussebelumnya"), 0), sptField,
                     FxDB(drutama("icjmlrevisi"), 0), sptField,
                     FxDB(drutama("iccetakanke"), 0), sptField,
                     FxDB(drutama("icinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icisclose"), 0), sptField,
                     FxDB(drutama("iccustomtext1"), ""), sptField,
                     FxDB(drutama("iccustomtext2"), ""), sptField,
                     FxDB(drutama("iccustomtext3"), ""), sptField,
                     FxDB(drutama("iccustomtext4"), ""), sptField,
                     FxDB(drutama("iccustomtext5"), ""), sptField,
                     FxDB(drutama("iccustomint1"), 0), sptField,
                     FxDB(drutama("iccustomint2"), 0), sptField,
                     FxDB(drutama("iccustomint3"), 0), sptField,
                     FxDB(drutama("iccustomdbl1"), 0), sptField,
                     FxDB(drutama("iccustomdbl2"), 0), sptField,
                     FxDB(drutama("iccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("iccabangnama"), ""), sptField,
                     FxDB(drutama("iclokasinama"), ""), sptField,
                     FxDB(drutama("icgudangnama"), ""), sptField,
                     FxDB(drutama("iccustomerkode"), ""), sptField,
                     FxDB(drutama("iccustomernama"), ""), sptField,
                     FxDB(drutama("icbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("icbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("icbagianpenagihankode"), ""), sptField,
                     FxDB(drutama("icbagianpenagihannama"), ""), sptField,
                     FxDB(drutama("iccarabayarnama"), ""), sptField,
                     FxDB(drutama("icrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("icrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("icstatusnama"), ""), sptField,
                     FxDB(drutama("icstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("icinputusernama"), ""), sptField,
                     FxDB(drutama("icmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                Dim tglgiro As String = FxDB(dr("tgljtgiro"), "")
                Dim customdate1 As String = FxDB(dr("customdate1"), "")
                Dim customdate2 As String = FxDB(dr("customdate2"), "")
                Dim customdate3 As String = FxDB(dr("customdate3"), "")
                Dim tgl As String = FxDB(dr("tgl"), "")
                Dim tgljatuhtempo As String = FxDB(dr("tgljatuhtempo"), "")
                Dim inputtgl As String = FxDB(dr("inputtgl"), "")

                If Len(tglgiro) > 0 Then tglgiro = AsFormatTanggal(FxDB(dr("tgljtgiro"), ""), formatTgl) Else tglgiro = tglgiro
                If Len(customdate1) > 0 Then customdate1 = AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl) Else customdate1 = customdate1
                If Len(customdate2) > 0 Then customdate2 = AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl) Else customdate2 = customdate2
                If Len(customdate3) > 0 Then customdate3 = AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl) Else customdate3 = customdate3
                If Len(tgl) > 0 Then tgl = AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl) Else tgl = tgl
                If Len(tgljatuhtempo) > 0 Then tgljatuhtempo = AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl) Else tgljatuhtempo = tgljatuhtempo
                If Len(inputtgl) > 0 Then inputtgl = AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu) Else inputtgl = inputtgl

                detail = String.Concat(detail, FxDB(dr("idicdetail"), 0), sptField,
                     FxDB(dr("idic"), 0), sptField,
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
                     FxDB(dr("jmlpv"), 0), sptField,
                     FxDB(dr("jmlpvvalas"), 0), sptField,
                     FxDB(dr("statuspv"), 0), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     customdate1, sptField,
                     customdate2, sptField,
                     customdate3, sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     tgl, sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     tgljatuhtempo, sptField,
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
                     inputtgl, sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icposting, icpostingtgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, iccustomdate3, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama, kpkp" & sptSubParam & "idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IcSearch(ByVal param As String) As String
        'M5_IcSearch --------------------------------------------------------
        'icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icposting, icpostingtgl, icisclose, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, 
        'iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, 
        'icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama

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
        sql = query.PanggilQuery("m5_ic_v")

        dt = AmbilData("aplikasi1-M5_ic_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("icid"), 0), sptField,
                     FxDB(dr("iccabang"), ""), sptField,
                     FxDB(dr("iclokasi"), ""), sptField,
                     FxDB(dr("icgudang"), ""), sptField,
                     FxDB(dr("icsumber"), ""), sptField,
                     FxDB(dr("icautonotransaksi"), 0), sptField,
                     FxDB(dr("icnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ictgl"), ""), formatTgl), sptField,
                     FxDB(dr("ickodepa"), 0), sptField,
                     FxDB(dr("iccustomer"), 0), sptField,
                     FxDB(dr("iccustomerkontak"), ""), sptField,
                     FxDB(dr("ic1alamat1"), ""), sptField,
                     FxDB(dr("ic1alamat2"), ""), sptField,
                     FxDB(dr("ic1alamat3"), ""), sptField,
                     FxDB(dr("ic2alamat1"), ""), sptField,
                     FxDB(dr("ic2alamat2"), ""), sptField,
                     FxDB(dr("ic2alamat3"), ""), sptField,
                     FxDB(dr("icbagianpenjualan"), 0), sptField,
                     FxDB(dr("icbagianpenagihan"), 0), sptField,
                     FxDB(dr("icuraian"), ""), sptField,
                     FxDB(dr("iccatatan"), ""), sptField,
                     FxDB(dr("icnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ictglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("iccarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ictglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("icmatauang"), ""), sptField,
                     FxDB(dr("ickurs"), 0), sptField,
                     FxDB(dr("ictotalap"), 0), sptField,
                     FxDB(dr("ictotalapvalas"), 0), sptField,
                     FxDB(dr("ictotalar"), 0), sptField,
                     FxDB(dr("ictotalarvalas"), 0), sptField,
                     FxDB(dr("icjmltagih"), 0), sptField,
                     FxDB(dr("icjmltagihvalas"), 0), sptField,
                     FxDB(dr("icbayar"), 0), sptField,
                     FxDB(dr("icbayarvalas"), 0), sptField,
                     FxDB(dr("icselisihkurs"), 0), sptField,
                     FxDB(dr("icrekselisihkurs"), ""), sptField,
                     FxDB(dr("icdiskontermin"), 0), sptField,
                     FxDB(dr("icdiskonterminvalas"), 0), sptField,
                     FxDB(dr("icrekdiskontermin"), ""), sptField,
                     FxDB(dr("icstatuspv"), 0), sptField,
                     FxDB(dr("icstatus"), 0), sptField,
                     FxDB(dr("icstatussebelumnya"), 0), sptField,
                     FxDB(dr("icjmlrevisi"), 0), sptField,
                     FxDB(dr("iccetakanke"), 0), sptField,
                     FxDB(dr("icinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("icinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("icmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("icmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("icposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("icpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("icisclose"), 0), sptField,
                     FxDB(dr("iccabangnama"), ""), sptField,
                     FxDB(dr("iclokasinama"), ""), sptField,
                     FxDB(dr("icgudangnama"), ""), sptField,
                     FxDB(dr("iccustomerkode"), ""), sptField,
                     FxDB(dr("iccustomernama"), ""), sptField,
                     FxDB(dr("icbagianpenjualankode"), ""), sptField,
                     FxDB(dr("icbagianpenjualannama"), ""), sptField,
                     FxDB(dr("icbagianpenagihankode"), ""), sptField,
                     FxDB(dr("icbagianpenagihannama"), ""), sptField,
                     FxDB(dr("iccarabayarnama"), ""), sptField,
                     FxDB(dr("icrekselisihkursnama"), ""), sptField,
                     FxDB(dr("icrekdiskonterminnama"), ""), sptField,
                     FxDB(dr("icstatusnama"), ""), sptField,
                     FxDB(dr("icstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("icinputusernama"), ""), sptField,
                     FxDB(dr("icmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icposting, icpostingtgl, icisclose, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IcTakedataSearch(ByVal param As String) As String
        'M5_IcTakedataSearch --------------------------------------------------------
        'idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, 
        'termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, rencana, 
        'sisa, sisavalas, statuslunas, rekhutangpiutang, diskon1, haridiskon1, diskon2, 
        'haridiskon2, inputtgl

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
        'Dim query As New m0_query
        'sql = query.m5_ic_takedata(Filter)
        sql = m5_ic_takedata(Filter)
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
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
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
                     FxDB(dr("diskon1"), 0), sptField,
                     FxDB(dr("haridiskon1"), 0), sptField,
                     FxDB(dr("diskon2"), 0), sptField,
                     FxDB(dr("haridiskon2"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, sisavalas, statuslunas, rekhutangpiutang, diskon1, haridiskon1, diskon2, haridiskon2, inputtgl"))

        Return wsResult
    End Function
    <WebMethod()>
    Public Function m5_ic_takedata(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = ""

        'Replace Filter & icrt
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1.Replace("idtransaksi", "si.siid")
            filter1 = filter1.Replace("sumber", "si.sisumber")
            filter1 = filter1.Replace("notransaksi", "si.sinotransaksi")
            filter1 = filter1.Replace("kontak", "si.sicustomer")
            filter1 = filter1.Replace("tgl", "si.sitgl")
            filter1 = filter1.Replace("matauang", "si.simatauang")
            filter1 = filter1.Replace("statuslunas", "si.sistatuslunas")
            filter1 = filter1.Replace("tanggaljatuhtempo", "si.sitgljatuhtempo")
            filter1 = filter1.Replace("uraian", "si.siuraian")

            filter2 = strFilter
            filter2 = filter2.Replace("idtransaksi", "as.asid")
            filter2 = filter2.Replace("sumber", "as.assumber")
            filter2 = filter2.Replace("notransaksi", "as.asnotransaksi")
            filter2 = filter2.Replace("kontak", "as.askontak")
            filter2 = filter2.Replace("tgl", "as.astgl")
            filter2 = filter2.Replace("matauang", "as.asmatauang")
            filter2 = filter2.Replace("statuslunas", "as.asstatusbayar")
            filter2 = filter2.Replace("tanggaljatuhtempo", "as.astgljatuhtempo")
            filter2 = filter2.Replace("uraian", "as.asuraian")

            filter3 = strFilter
            filter3 = filter3.Replace("idtransaksi", "sr.srid")
            filter3 = filter3.Replace("sumber", "sr.srsumber")
            filter3 = filter3.Replace("notransaksi", "sr.srnotransaksi")
            filter3 = filter3.Replace("kontak", "sr.srcustomer")
            filter3 = filter3.Replace("tgl", "sr.srtgl")
            filter3 = filter3.Replace("matauang", "sr.srmatauang")
            filter3 = filter3.Replace("statuslunas", "sr.srstatuslunas")
            filter3 = filter3.Replace("tanggaljatuhtempo", "sr.srtgljatuhtempo")
            filter3 = filter3.Replace("uraian", "sr.sruraian")

            filter4 = strFilter
            filter4 = filter4.Replace("idtransaksi", "rp.rpid")
            filter4 = filter4.Replace("sumber", "rp.rpsumber")
            filter4 = filter4.Replace("notransaksi", "rp.rpnotransaksi")
            filter4 = filter4.Replace("kontak", "rp.rpkontak")
            filter4 = filter4.Replace("tgl", "rp.rptgl")
            filter4 = filter4.Replace("matauang", "rp.rpmatauang")
            filter4 = filter4.Replace("statuslunas", "rp.rpstatusbayar")
            filter4 = filter4.Replace("tanggaljatuhtempo", "rp.rptgljatuhtempo")
            filter4 = filter4.Replace("uraian", "rp.rpuraian")

            filter5 = strFilter
            filter5 = filter5.Replace("idtransaksi", "ip.ipid")
            filter5 = filter5.Replace("sumber", "ip.ipsumber")
            filter5 = filter5.Replace("notransaksi", "ip.ipnotransaksi")
            filter5 = filter5.Replace("kontak", "ip.ipkontak")
            filter5 = filter5.Replace("tgl", "ip.iptgl")
            filter5 = filter5.Replace("matauang", "ip.ipmatauang")
            filter5 = filter5.Replace("statuslunas", "ip.ipstatusbayar")
            filter5 = filter5.Replace("tanggaljatuhtempo", "ip.iptgljatuhtempo")
            filter5 = filter5.Replace("uraian", "ip.ipuraian")
        End If

        'If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        filter1 = " WHERE si.sistatus IN(2,3,4,7) AND si.sicarabayar = 1 AND si.sitotaltransaksi <> 0 AND " & filter1

        'If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        filter2 = " WHERE as.asstatus IN(2,3,4,7) AND as.asjumlah <> 0 AND " & filter2

        'If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        filter3 = " WHERE sr.srstatus IN(2,3,4,7) AND sr.srjenis = 0 AND sr.srtotaltransaksi <> 0 AND " & filter3

        'If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        filter4 = " WHERE rp.rpstatus IN(2,3,4,7) AND rp.rpjumlah <> 0 AND " & filter4

        'If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        filter5 = " WHERE ip.ipstatus IN(2,3,4,7) AND ip.ipjumlah <> 0 AND " & filter5

        'SI
        'sql = "select `si`.`siid` AS `idtransaksi`,`si`.`sisumber` AS `sumber`,`si`.`sinotransaksi` AS `notransaksi`,`si`.`sitgl` AS `tgl`,`si`.`sicustomer` AS `kontak`,`si`.`sicatatan` AS `catatan`,`si`.`sicarabayar` AS `carabayar`,`si`.`sitermin` AS `termin`,`si`.`sitgljatuhtempo` AS `tgljatuhtempo`,`si`.`simatauang` AS `matauang`,`si`.`sikurs` AS `kurs`,`si`.`sitotaltransaksi` AS `totaltransaksi`,`si`.`sijmlbayar` AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `si`.`sikurs`) AS `rencana`,((`si`.`sitotaltransaksi` - `si`.`sijmlbayar`) * `si`.`sikurs`) AS `sisa`,(case `si`.`simatauang` when `s2`.`snilai` then 0 else (`si`.`sitotaltransaksi` - `si`.`sijmlbayar`) end) AS `sisavalas`,`si`.`sistatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`si`.`siinputtgl` AS `inputtgl` from ((((`m5_si` `si` left join `m1_terms` `tr` on((`si`.`sitermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'PiutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'SI') and (`icd`.`idtransaksi` = `si`.`siid`) and (`icd`.`statuspv` <> 2)))) " & filter1 & " group by `si`.`siid`"
        sql = "select `si`.`siid` AS `idtransaksi`,`si`.`sisumber` AS `sumber`,`si`.`sinotransaksi` AS `notransaksi`,`si`.`sitgl` AS `tgl`,`si`.`sicustomer` AS `kontak`,`si`.`sicatatan` AS `catatan`,`si`.`sicarabayar` AS `carabayar`,`si`.`sitermin` AS `termin`,`si`.`sitgljatuhtempo` AS `tgljatuhtempo`,`si`.`simatauang` AS `matauang`,`si`.`sikurs` AS `kurs`,`si`.`sitotaltransaksi` AS `totaltransaksi`,`si`.`sijmlbayar` AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `si`.`sikurs`) AS `rencana`,((`si`.`sitotaltransaksi` - `si`.`sijmlbayar`) * `si`.`sikurs`) AS `sisa`,(case `si`.`simatauang` when `s2`.`snilai` then 0 else (`si`.`sitotaltransaksi` - `si`.`sijmlbayar`) end) AS `sisavalas`,`si`.`sistatuslunas` AS `statuslunas`,c.krekpiutang AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`si`.`siinputtgl` AS `inputtgl` from `m5_si` `si` join m1_contact c on si.sicustomer = c.kid join `m0_setting` `s` on `s`.`smodule` = 0 and `s`.`sgrup` = 'akun' and `s`.`skode` = 'PiutangUsaha' join `m0_setting` `s2` on `s2`.`smodule` = 0 and `s2`.`sgrup` = 'accounting' and `s2`.`skode` = 'MataUangFungsional' left join `m1_terms` `tr` on `si`.`sitermin` = `tr`.`trkode` left join `m5_ic_detail` `icd` on `icd`.`sumber` = 'SI' and `icd`.`idtransaksi` = `si`.`siid` and `icd`.`statuspv` <> 2 " & filter1 & " group by `si`.`siid`"
        'AS
        sql &= " UNION "
        sql &= "select `as`.`asid` AS `idtransaksi`,`as`.`assumber` AS `sumber`,`as`.`asnotransaksi` AS `notransaksi`,`as`.`astgl` AS `tgl`,`as`.`askontak` AS `kontak`,`asd`.`catatan` AS `catatan`,0 AS `carabayar`,`as`.`astermin` AS `termin`,`as`.`astgljatuhtempo` AS `tgljatuhtempo`,`as`.`asmatauang` AS `matauang`,`as`.`askurs` AS `kurs`,(case `as`.`asmatauang` when `s2`.`snilai` then `as`.`asjumlah` else `as`.`asjumlahvalas` end) AS `totaltransaksi`,(case `as`.`asmatauang` when `s2`.`snilai` then `as`.`asjumlahbayar` else `as`.`asjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `as`.`askurs`) AS `rencana`,(`as`.`asjumlah` - `as`.`asjumlahbayar`) AS `sisa`,(case `as`.`asmatauang` when `s2`.`snilai` then 0 else (`as`.`asjumlahvalas` - `as`.`asjumlahbayarvalas`) end) AS `sisavalas`,`as`.`asstatusbayar` AS `statuslunas`,`as`.`asnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`as`.`asinputtgl` AS `inputtgl` from (((`m5_as` `as` left join `m1_terms` `tr` on((`as`.`astermin` = `tr`.`trkode`))) left join `m5_as_pay` `asd` on `as`.`asid` = `asd`.`idas` join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'AS') and (`icd`.`idtransaksi` = `as`.`asid`) and (`icd`.`statuspv` <> 2)))) " & filter2 & " group by `as`.`asid`"
        'SR
        sql &= " UNION "
        'sql &= "select `sr`.`srid` AS `idtransaksi`,`sr`.`srsumber` AS `sumber`,`sr`.`srnotransaksi` AS `notransaksi`,`sr`.`srtgl` AS `tgl`,`sr`.`srcustomer` AS `kontak`,`sr`.`srcatatan` AS `catatan`,`sr`.`srcarabayar` AS `carabayar`,`sr`.`srtermin` AS `termin`,`sr`.`srtgljatuhtempo` AS `tgljatuhtempo`,`sr`.`srmatauang` AS `matauang`,`sr`.`srkurs` AS `kurs`,`sr`.`srtotaltransaksi` AS `totaltransaksi`,`sr`.`srjmlbayar` AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `sr`.`srkurs`) AS `rencana`,((`sr`.`srtotaltransaksi` - `sr`.`srjmlbayar`) * `sr`.`srkurs`) AS `sisa`,(case `sr`.`srmatauang` when `s2`.`snilai` then 0 else (`sr`.`srtotaltransaksi` - `sr`.`srjmlbayar`) end) AS `sisavalas`,`sr`.`srstatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`sr`.`srinputtgl` AS `inputtgl` from ((((`m5_sr` `sr` left join `m1_terms` `tr` on((`sr`.`srtermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'PiutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'SR') and (`icd`.`idtransaksi` = `sr`.`srid`) and (`icd`.`statuspv` <> 2)))) " & filter3 & " group by `sr`.`srid`"
        sql &= "select `sr`.`srid` AS `idtransaksi`,`sr`.`srsumber` AS `sumber`,`sr`.`srnotransaksi` AS `notransaksi`,`sr`.`srtgl` AS `tgl`,`sr`.`srcustomer` AS `kontak`,`sr`.`srcatatan` AS `catatan`,`sr`.`srcarabayar` AS `carabayar`,`sr`.`srtermin` AS `termin`,`sr`.`srtgljatuhtempo` AS `tgljatuhtempo`,`sr`.`srmatauang` AS `matauang`,`sr`.`srkurs` AS `kurs`,`sr`.`srtotaltransaksi` AS `totaltransaksi`,`sr`.`srjmlbayar` AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `sr`.`srkurs`) AS `rencana`,((`sr`.`srtotaltransaksi` - `sr`.`srjmlbayar`) * `sr`.`srkurs`) AS `sisa`,(case `sr`.`srmatauang` when `s2`.`snilai` then 0 else (`sr`.`srtotaltransaksi` - `sr`.`srjmlbayar`) end) AS `sisavalas`,`sr`.`srstatuslunas` AS `statuslunas`,c.krekpiutang AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`sr`.`srinputtgl` AS `inputtgl` from `m5_sr` `sr` join m1_contact c on sr.srcustomer = c.kid join `m0_setting` `s` on `s`.`smodule` = 0 and `s`.`sgrup` = 'akun' and `s`.`skode` = 'PiutangUsaha' join `m0_setting` `s2` on `s2`.`smodule` = 0 and `s2`.`sgrup` = 'accounting' and `s2`.`skode` = 'MataUangFungsional' left join `m1_terms` `tr` on `sr`.`srtermin` = `tr`.`trkode` left join `m5_ic_detail` `icd` on `icd`.`sumber` = 'SR' and `icd`.`idtransaksi` = `sr`.`srid` and `icd`.`statuspv` <> 2 " & filter3 & " group by `sr`.`srid`"
        'RP
        sql &= " UNION "
        sql &= "select `rp`.`rpid` AS `idtransaksi`,`rp`.`rpsumber` AS `sumber`,`rp`.`rpnotransaksi` AS `notransaksi`,`rp`.`rptgl` AS `tgl`,`rp`.`rpkontak` AS `kontak`,`rp`.`rpcatatan` AS `catatan`,0 AS `carabayar`,`rp`.`rptermin` AS `termin`,`rp`.`rptgljatuhtempo` AS `tgljatuhtempo`,`rp`.`rpmatauang` AS `matauang`,`rp`.`rpkurs` AS `kurs`,(case `rp`.`rpmatauang` when `s2`.`snilai` then `rp`.`rpjumlah` else `rp`.`rpjumlahvalas` end) AS `totaltransaksi`,(case `rp`.`rpmatauang` when `s2`.`snilai` then `rp`.`rpjumlahbayar` else `rp`.`rpjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `rp`.`rpkurs`) AS `rencana`,(`rp`.`rpjumlah` - `rp`.`rpjumlahbayar`) AS `sisa`,(case `rp`.`rpmatauang` when `s2`.`snilai` then 0 else (`rp`.`rpjumlahvalas` - `rp`.`rpjumlahbayarvalas`) end) AS `sisavalas`,`rp`.`rpstatusbayar` AS `statuslunas`,`rp`.`rpnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`rp`.`rpinputtgl` AS `inputtgl` from (((`m5_rp` `rp` left join `m1_terms` `tr` on((`rp`.`rptermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'RP') and (`icd`.`idtransaksi` = `rp`.`rpid`) and (`icd`.`statuspv` <> 2)))) " & filter4 & " group by `rp`.`rpid`"
        'IP
        sql &= " UNION "
        sql &= "select `ip`.`ipid` AS `idtransaksi`,`ip`.`ipsumber` AS `sumber`,`ip`.`ipnotransaksi` AS `notransaksi`,`ip`.`iptgl` AS `tgl`,`ip`.`ipkontak` AS `kontak`,`ip`.`ipcatatan` AS `catatan`,0 AS `carabayar`,`ip`.`iptermin` AS `termin`,`ip`.`iptgljatuhtempo` AS `tgljatuhtempo`,`ip`.`ipmatauang` AS `matauang`,`ip`.`ipkurs` AS `kurs`,(case `ip`.`ipmatauang` when `s2`.`snilai` then `ip`.`ipjumlah` else `ip`.`ipjumlahvalas` end) AS `totaltransaksi`,(case `ip`.`ipmatauang` when `s2`.`snilai` then `ip`.`ipjumlahbayar` else `ip`.`ipjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `ip`.`ipkurs`) AS `rencana`,(`ip`.`ipjumlah` - `ip`.`ipjumlahbayar`) AS `sisa`,(case `ip`.`ipmatauang` when `s2`.`snilai` then 0 else (`ip`.`ipjumlahvalas` - `ip`.`ipjumlahbayarvalas`) end) AS `sisavalas`,`ip`.`ipstatusbayar` AS `statuslunas`,`ip`.`ipnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`ip`.`ipinputtgl` AS `inputtgl` from (((`m5_ip` `ip` left join `m1_terms` `tr` on((`ip`.`iptermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'ip') and (`icd`.`idtransaksi` = `ip`.`ipid`) and (`icd`.`statuspv` <> 2)))) " & filter5 & " group by `ip`.`ipid`"

        Return sql
    End Function
    <WebMethod()>
    Public Function m5_ic_takedataold(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = ""

        'Replace Filter & icrt
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1.Replace("idtransaksi", "si.siid")
            filter1 = filter1.Replace("sumber", "si.sisumber")
            filter1 = filter1.Replace("notransaksi", "si.sinotransaksi")
            filter1 = filter1.Replace("kontak", "si.sicustomer")
            filter1 = filter1.Replace("tgl", "si.sitgl")
            filter1 = filter1.Replace("matauang", "si.simatauang")
            filter1 = filter1.Replace("statuslunas", "si.sistatuslunas")
            filter1 = filter1.Replace("tanggaljatuhtempo", "si.sitgljatuhtempo")
            filter1 = filter1.Replace("uraian", "si.siuraian")

            filter2 = strFilter
            filter2 = filter2.Replace("idtransaksi", "as.asid")
            filter2 = filter2.Replace("sumber", "as.assumber")
            filter2 = filter2.Replace("notransaksi", "as.asnotransaksi")
            filter2 = filter2.Replace("kontak", "as.askontak")
            filter2 = filter2.Replace("tgl", "as.astgl")
            filter2 = filter2.Replace("matauang", "as.asmatauang")
            filter2 = filter2.Replace("statuslunas", "as.asstatusbayar")
            filter2 = filter2.Replace("tanggaljatuhtempo", "as.astgljatuhtempo")
            filter2 = filter2.Replace("uraian", "as.asuraian")

            filter3 = strFilter
            filter3 = filter3.Replace("idtransaksi", "sr.srid")
            filter3 = filter3.Replace("sumber", "sr.srsumber")
            filter3 = filter3.Replace("notransaksi", "sr.srnotransaksi")
            filter3 = filter3.Replace("kontak", "sr.srcustomer")
            filter3 = filter3.Replace("tgl", "sr.srtgl")
            filter3 = filter3.Replace("matauang", "sr.srmatauang")
            filter3 = filter3.Replace("statuslunas", "sr.srstatuslunas")
            filter3 = filter3.Replace("tanggaljatuhtempo", "sr.srtgljatuhtempo")
            filter3 = filter3.Replace("uraian", "sr.sruraian")

            filter4 = strFilter
            filter4 = filter4.Replace("idtransaksi", "rp.rpid")
            filter4 = filter4.Replace("sumber", "rp.rpsumber")
            filter4 = filter4.Replace("notransaksi", "rp.rpnotransaksi")
            filter4 = filter4.Replace("kontak", "rp.rpkontak")
            filter4 = filter4.Replace("tgl", "rp.rptgl")
            filter4 = filter4.Replace("matauang", "rp.rpmatauang")
            filter4 = filter4.Replace("statuslunas", "rp.rpstatusbayar")
            filter4 = filter4.Replace("tanggaljatuhtempo", "rp.rptgljatuhtempo")
            filter4 = filter4.Replace("uraian", "rp.rpuraian")

            filter5 = strFilter
            filter5 = filter5.Replace("idtransaksi", "ip.ipid")
            filter5 = filter5.Replace("sumber", "ip.ipsumber")
            filter5 = filter5.Replace("notransaksi", "ip.ipnotransaksi")
            filter5 = filter5.Replace("kontak", "ip.ipkontak")
            filter5 = filter5.Replace("tgl", "ip.iptgl")
            filter5 = filter5.Replace("matauang", "ip.ipmatauang")
            filter5 = filter5.Replace("statuslunas", "ip.ipstatusbayar")
            filter5 = filter5.Replace("tanggaljatuhtempo", "ip.iptgljatuhtempo")
            filter5 = filter5.Replace("uraian", "ip.ipuraian")
        End If

        'If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        filter1 = " WHERE si.sistatus IN(2,3,4,7) AND si.sicarabayar = 1 AND si.sitotaltransaksi <> 0 AND " & filter1

        'If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        filter2 = " WHERE as.asstatus IN(2,3,4,7) AND as.asjumlah <> 0 AND " & filter2

        'If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        filter3 = " WHERE sr.srstatus IN(2,3,4,7) AND sr.srjenis = 0 AND sr.srtotaltransaksi <> 0 AND " & filter3

        'If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        filter4 = " WHERE rp.rpstatus IN(2,3,4,7) AND rp.rpjumlah <> 0 AND " & filter4

        'If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        filter5 = " WHERE ip.ipstatus IN(2,3,4,7) AND ip.ipjumlah <> 0 AND " & filter5

        'SI
        sql = "select `si`.`siid` AS `idtransaksi`,`si`.`sisumber` AS `sumber`,`si`.`sinotransaksi` AS `notransaksi`,`si`.`sitgl` AS `tgl`,`si`.`sicustomer` AS `kontak`,`si`.`sicatatan` AS `catatan`,`si`.`sicarabayar` AS `carabayar`,`si`.`sitermin` AS `termin`,`si`.`sitgljatuhtempo` AS `tgljatuhtempo`,`si`.`simatauang` AS `matauang`,`si`.`sikurs` AS `kurs`,`si`.`sitotaltransaksi` AS `totaltransaksi`,`si`.`sijmlbayar` AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `si`.`sikurs`) AS `rencana`,((`si`.`sitotaltransaksi` - `si`.`sijmlbayar`) * `si`.`sikurs`) AS `sisa`,(case `si`.`simatauang` when `s2`.`snilai` then 0 else (`si`.`sitotaltransaksi` - `si`.`sijmlbayar`) end) AS `sisavalas`,`si`.`sistatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`si`.`siinputtgl` AS `inputtgl` from ((((`m5_si` `si` left join `m1_terms` `tr` on((`si`.`sitermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'PiutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'SI') and (`icd`.`idtransaksi` = `si`.`siid`) and (`icd`.`statuspv` <> 2)))) " & filter1 & " group by `si`.`siid`"
        'AS
        sql &= " UNION "
        sql &= "select `as`.`asid` AS `idtransaksi`,`as`.`assumber` AS `sumber`,`as`.`asnotransaksi` AS `notransaksi`,`as`.`astgl` AS `tgl`,`as`.`askontak` AS `kontak`,`as`.`ascatatan` AS `catatan`,0 AS `carabayar`,`as`.`astermin` AS `termin`,`as`.`astgljatuhtempo` AS `tgljatuhtempo`,`as`.`asmatauang` AS `matauang`,`as`.`askurs` AS `kurs`,(case `as`.`asmatauang` when `s2`.`snilai` then `as`.`asjumlah` else `as`.`asjumlahvalas` end) AS `totaltransaksi`,(case `as`.`asmatauang` when `s2`.`snilai` then `as`.`asjumlahbayar` else `as`.`asjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `as`.`askurs`) AS `rencana`,(`as`.`asjumlah` - `as`.`asjumlahbayar`) AS `sisa`,(case `as`.`asmatauang` when `s2`.`snilai` then 0 else (`as`.`asjumlahvalas` - `as`.`asjumlahbayarvalas`) end) AS `sisavalas`,`as`.`asstatusbayar` AS `statuslunas`,`as`.`asnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`as`.`asinputtgl` AS `inputtgl` from (((`m5_as` `as` left join `m1_terms` `tr` on((`as`.`astermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'AS') and (`icd`.`idtransaksi` = `as`.`asid`) and (`icd`.`statuspv` <> 2)))) " & filter2 & " group by `as`.`asid`"
        'SR
        sql &= " UNION "
        sql &= "select `sr`.`srid` AS `idtransaksi`,`sr`.`srsumber` AS `sumber`,`sr`.`srnotransaksi` AS `notransaksi`,`sr`.`srtgl` AS `tgl`,`sr`.`srcustomer` AS `kontak`,`sr`.`srcatatan` AS `catatan`,`sr`.`srcarabayar` AS `carabayar`,`sr`.`srtermin` AS `termin`,`sr`.`srtgljatuhtempo` AS `tgljatuhtempo`,`sr`.`srmatauang` AS `matauang`,`sr`.`srkurs` AS `kurs`,`sr`.`srtotaltransaksi` AS `totaltransaksi`,`sr`.`srjmlbayar` AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `sr`.`srkurs`) AS `rencana`,((`sr`.`srtotaltransaksi` - `sr`.`srjmlbayar`) * `sr`.`srkurs`) AS `sisa`,(case `sr`.`srmatauang` when `s2`.`snilai` then 0 else (`sr`.`srtotaltransaksi` - `sr`.`srjmlbayar`) end) AS `sisavalas`,`sr`.`srstatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`sr`.`srinputtgl` AS `inputtgl` from ((((`m5_sr` `sr` left join `m1_terms` `tr` on((`sr`.`srtermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'PiutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'SR') and (`icd`.`idtransaksi` = `sr`.`srid`) and (`icd`.`statuspv` <> 2)))) " & filter3 & " group by `sr`.`srid`"
        'RP
        sql &= " UNION "
        sql &= "select `rp`.`rpid` AS `idtransaksi`,`rp`.`rpsumber` AS `sumber`,`rp`.`rpnotransaksi` AS `notransaksi`,`rp`.`rptgl` AS `tgl`,`rp`.`rpkontak` AS `kontak`,`rp`.`rpcatatan` AS `catatan`,0 AS `carabayar`,`rp`.`rptermin` AS `termin`,`rp`.`rptgljatuhtempo` AS `tgljatuhtempo`,`rp`.`rpmatauang` AS `matauang`,`rp`.`rpkurs` AS `kurs`,(case `rp`.`rpmatauang` when `s2`.`snilai` then `rp`.`rpjumlah` else `rp`.`rpjumlahvalas` end) AS `totaltransaksi`,(case `rp`.`rpmatauang` when `s2`.`snilai` then `rp`.`rpjumlahbayar` else `rp`.`rpjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `rp`.`rpkurs`) AS `rencana`,(`rp`.`rpjumlah` - `rp`.`rpjumlahbayar`) AS `sisa`,(case `rp`.`rpmatauang` when `s2`.`snilai` then 0 else (`rp`.`rpjumlahvalas` - `rp`.`rpjumlahbayarvalas`) end) AS `sisavalas`,`rp`.`rpstatusbayar` AS `statuslunas`,`rp`.`rpnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`rp`.`rpinputtgl` AS `inputtgl` from (((`m5_rp` `rp` left join `m1_terms` `tr` on((`rp`.`rptermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'RP') and (`icd`.`idtransaksi` = `rp`.`rpid`) and (`icd`.`statuspv` <> 2)))) " & filter4 & " group by `rp`.`rpid`"
        'IP
        sql &= " UNION "
        sql &= "select `ip`.`ipid` AS `idtransaksi`,`ip`.`ipsumber` AS `sumber`,`ip`.`ipnotransaksi` AS `notransaksi`,`ip`.`iptgl` AS `tgl`,`ip`.`ipkontak` AS `kontak`,`ip`.`ipcatatan` AS `catatan`,0 AS `carabayar`,`ip`.`iptermin` AS `termin`,`ip`.`iptgljatuhtempo` AS `tgljatuhtempo`,`ip`.`ipmatauang` AS `matauang`,`ip`.`ipkurs` AS `kurs`,(case `ip`.`ipmatauang` when `s2`.`snilai` then `ip`.`ipjumlah` else `ip`.`ipjumlahvalas` end) AS `totaltransaksi`,(case `ip`.`ipmatauang` when `s2`.`snilai` then `ip`.`ipjumlahbayar` else `ip`.`ipjumlahbayarvalas` end) AS `terbayar`,(sum((`icd`.`jmlbayar` - `icd`.`jmlpv`)) / `ip`.`ipkurs`) AS `rencana`,(`ip`.`ipjumlah` - `ip`.`ipjumlahbayar`) AS `sisa`,(case `ip`.`ipmatauang` when `s2`.`snilai` then 0 else (`ip`.`ipjumlahvalas` - `ip`.`ipjumlahbayarvalas`) end) AS `sisavalas`,`ip`.`ipstatusbayar` AS `statuslunas`,`ip`.`ipnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`ip`.`ipinputtgl` AS `inputtgl` from (((`m5_ip` `ip` left join `m1_terms` `tr` on((`ip`.`iptermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m5_ic_detail` `icd` on(((`icd`.`sumber` = 'ip') and (`icd`.`idtransaksi` = `ip`.`ipid`) and (`icd`.`statuspv` <> 2)))) " & filter5 & " group by `ip`.`ipid`"

        Return sql
    End Function

    <WebMethod()>
    Public Function M5_IcTerkait(ByVal param As String) As String
        'M5_IcTerkait --------------------------------------------------------
        'icid, icnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "icid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND icid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "icid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m5_ic_terkait(Filter)

        'result(2) = sql : GoTo selesai

        dt = AmbilData("aplikasi1-m5_ic_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each ic As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(ic("icid"), 0), sptField,
                     FxDB(ic("icnotransaksi"), ""), sptField,
                     FxDB(ic("sumber"), ""), sptField,
                     FxDB(ic("idterkait"), 0), sptField,
                     FxDB(ic("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(ic("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(ic("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(ic("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(ic("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related IC data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("icid, icnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IcSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'icid(0) As Integer, iccabang(1) As String, iclokasi(2) As String, icgudang(3) As String, icsumber(4) As String, 
        'icautonotransaksi(5) As Integer, icnotransaksi(6) As String, ictgl(7) As Date, ickodepa(8) As Integer, iccustomer(9) As Integer, 
        'iccustomerkontak(10) As String, ic1alamat1(11) As String, ic1alamat2(12) As String, ic1alamat3(13) As String, ic2alamat1(14) As String, 
        'ic2alamat2(15) As String, ic2alamat3(16) As String, icbagianpenjualan(17) As Integer, icbagianpenagihan(18) As Integer, icuraian(19) As String, 
        'iccatatan(20) As String, icnoref(21) As String, ictglnoref(22) As Date, iccarabayar(23) As Integer, ictglbayar(24) As Date, 
        'icmatauang(25) As String, ickurs(26) As Double, ictotalap(27) As Double, ictotalapvalas(28) As Double, ictotalar(29) As Double, 
        'ictotalarvalas(30) As Double, icjmltagih(31) As Double, icjmltagihvalas(32) As Double, icbayar(33) As Double, icbayarvalas(34) As Double, 
        'icselisihkurs(35) As Double, icrekselisihkurs(36) As String, icdiskontermin(37) As Double, icdiskonterminvalas(38) As Double, icrekdiskontermin(39) As String, 
        'icstatuspv(40) As Integer, icstatus(41) As Integer, icstatussebelumnya(42) As Integer, icjmlrevisi(43) As Integer, iccetakanke(44) As Integer, 
        'icinputuser(45) As Integer, icinputtgl(46) As DateTime, icmodifikasiuser(47) As Integer, icmodifikasitgl(48) As DateTime, icisclose(49) As Integer, 
        'iccustomtext1(50) As String, iccustomtext2(51) As String, iccustomtext3(52) As String, iccustomtext4(53) As String, iccustomtext5(54) As String, 
        'iccustomint1(55) As Integer, iccustomint2(56) As Integer, iccustomint3(57) As Integer, iccustomdbl1(58) As Double, iccustomdbl2(59) As Double, 
        'iccustomdbl3(60) As Double, iccustomdate1(61) As Date, iccustomdate2(62) As Date, iccustomdate3(63) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, 
        'iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, 
        'iccustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 64) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'icid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "icid required numeric." : GoTo selesai
        End If
        'icautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "icautonotransaksi required numeric." : GoTo selesai
        End If
        'ictgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ictgl required date." : GoTo selesai
        End If
        'ickodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ickodepa required numeric." : GoTo selesai
        End If
        'iccustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "iccustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "iccustomer can't be empty." : GoTo selesai
        End If
        'icbagianpenjualan(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "icbagianpenjualan required numeric." : GoTo selesai
        End If
        'icbagianpenagihan(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "icbagianpenagihan required numeric." : GoTo selesai
        End If
        'ictglnoref(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "ictglnoref required date." : GoTo selesai
        End If
        'iccarabayar(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "iccarabayar required numeric." : GoTo selesai
        End If
        'ictglbayar(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "ictglbayar required date." : GoTo selesai
        End If
        'ickurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ickurs required numeric." : GoTo selesai
        End If
        'ictotalap(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ictotalap required numeric." : GoTo selesai
        End If
        'ictotalapvalas(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ictotalapvalas required numeric." : GoTo selesai
        End If
        'ictotalar(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ictotalar required numeric." : GoTo selesai
        End If
        'ictotalarvalas(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "ictotalarvalas required numeric." : GoTo selesai
        End If
        'icjmltagih(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "icjmltagih required numeric." : GoTo selesai
        End If
        'icjmltagihvalas(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "icjmltagihvalas required numeric." : GoTo selesai
        End If
        'icbayar(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "icbayar required numeric." : GoTo selesai
        End If
        'icbayarvalas(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "icbayarvalas required numeric." : GoTo selesai
        End If
        'icselisihkurs(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "icselisihkurs required numeric." : GoTo selesai
        End If
        'icdiskontermin(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "icdiskontermin required numeric." : GoTo selesai
        End If
        'icdiskonterminvalas(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "icdiskonterminvalas required numeric." : GoTo selesai
        End If
        'icstatuspv(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "icstatuspv required numeric." : GoTo selesai
        End If
        'icstatus(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "icstatus required numeric." : GoTo selesai
        End If
        'icstatussebelumnya(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "icstatussebelumnya required numeric." : GoTo selesai
        End If
        'icjmlrevisi(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "icjmlrevisi required numeric." : GoTo selesai
        End If
        'iccetakanke(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "iccetakanke required numeric." : GoTo selesai
        End If
        'icinputuser(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "icinputuser required numeric." : GoTo selesai
        End If
        'icinputtgl(46) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "icinputtgl required date." : GoTo selesai
        End If
        'icmodifikasiuser(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "icmodifikasiuser required numeric." : GoTo selesai
        End If
        'icmodifikasitgl(48) As DateTime
        If (IsDate(dataUtama(48)) = False) Then
            result(2) = "icmodifikasitgl required date." : GoTo selesai
        End If
        'icisclose(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "icisclose required numeric." : GoTo selesai
        End If
        'iccustomint1(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "iccustomint1 required numeric." : GoTo selesai
        End If
        'iccustomint2(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "iccustomint2 required numeric." : GoTo selesai
        End If
        'iccustomint3(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "iccustomint3 required numeric." : GoTo selesai
        End If
        'iccustomdbl1(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "iccustomdbl1 required numeric." : GoTo selesai
        End If
        'iccustomdbl2(59) As Double
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "iccustomdbl2 required numeric." : GoTo selesai
        End If
        'iccustomdbl3(60) As Double
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "iccustomdbl3 required numeric." : GoTo selesai
        End If
        'iccustomdate1(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "iccustomdate1 required date." : GoTo selesai
        End If
        'iccustomdate2(62) As Date
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "iccustomdate2 required date." : GoTo selesai
        End If
        'iccustomdate3(63) As Date
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "iccustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'iccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "iccabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "iccabang should not be more than 25 character." : GoTo selesai
        End If

        'iclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "iclokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "iclokasi should not be more than 25 character." : GoTo selesai
        End If

        'icsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "icsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "icsumber should not be more than 10 character." : GoTo selesai
        End If

        'icnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "icnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "icnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ictgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ictgl can't be empty" : GoTo selesai
        End If

        'ictglnoref(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "ictglnoref can't be empty" : GoTo selesai
        End If

        'ictglbayar(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "ictglbayar can't be empty" : GoTo selesai
        End If

        'icmatauang(25) As String
        If Len(dataUtama(25)) = 0 Then
            result(2) = "icmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 25 Then
            result(2) = "icmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ickurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "ickurs can't be empty" : GoTo selesai
        End If

        'ictotalap(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "ictotalap can't be empty" : GoTo selesai
        End If

        'ictotalapvalas(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ictotalapvalas can't be empty" : GoTo selesai
        End If

        'ictotalar(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ictotalar can't be empty" : GoTo selesai
        End If

        'ictotalarvalas(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ictotalarvalas can't be empty" : GoTo selesai
        End If

        'icjmltagih(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "icjmltagih can't be empty" : GoTo selesai
        End If

        'icjmltagihvalas(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "icjmltagihvalas can't be empty" : GoTo selesai
        End If

        'icbayar(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "icbayar can't be empty" : GoTo selesai
        End If

        'icbayarvalas(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "icbayarvalas can't be empty" : GoTo selesai
        End If

        'icselisihkurs(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "icselisihkurs can't be empty" : GoTo selesai
        End If

        'icdiskontermin(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "icdiskontermin can't be empty" : GoTo selesai
        End If

        'icdiskonterminvalas(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "icdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'icinputtgl(46) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "icinputtgl can't be empty" : GoTo selesai
        End If

        'icmodifikasitgl(48) As DateTime
        If Len(dataUtama(48)) = 0 Then
            result(2) = "icmodifikasitgl can't be empty" : GoTo selesai
        End If

        'iccustomdbl1(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "iccustomdbl1 can't be empty" : GoTo selesai
        End If

        'iccustomdbl2(59) As Double
        If Len(dataUtama(59)) = 0 Then
            result(2) = "iccustomdbl2 can't be empty" : GoTo selesai
        End If

        'iccustomdbl3(60) As Double
        If Len(dataUtama(60)) = 0 Then
            result(2) = "iccustomdbl3 can't be empty" : GoTo selesai
        End If

        'iccustomdate1(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "iccustomdate1 can't be empty" : GoTo selesai
        End If

        'iccustomdate2(62) As Date
        If Len(dataUtama(62)) = 0 Then
            result(2) = "iccustomdate2 can't be empty" : GoTo selesai
        End If

        'iccustomdate3(63) As Date
        If Len(dataUtama(63)) = 0 Then
            result(2) = "iccustomdate3 can't be empty" : GoTo selesai
        End If

        ''VALIDASI JUMLAH BAYAR
        ''JIKA TOTAL AP - DISKON TERMIN - TOTAL AR + SELISIH KURS <> 0 MAKA MUNCUL PERINGATAN
        ''               ictotalap(27),           icdiskontermin(37),                ictotalar(29),            icselisihkurs(35)
        'If Double.Parse(dataUtama(27)) - Double.Parse(dataUtama(37)) - Double.Parse(dataUtama(29)) + Double.Parse(dataUtama(35)) <> 0 Then
        '    Dim selisih(2) As String
        '    selisih = F_Nominal(Double.Parse(dataUtama(27)) - Double.Parse(dataUtama(37)) - Double.Parse(dataUtama(29)) + Double.Parse(dataUtama(35)), False).Split(sptSubParam)
        '    result(2) = "Total AR - Total AP must be balance : " & selisih(1) & "" : GoTo selesai
        'End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "icid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iclokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ickodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ic2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icbagianpenagihan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ictglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ickurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ictotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icjmltagih", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icjmltagihvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icrekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icrekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icstatuspv", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "icmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "icisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iccustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "icid~iccabang~iclokasi~icgudang~icsumber~icautonotransaksi~icnotransaksi~ictgl~ickodepa~iccustomer~iccustomerkontak~ic1alamat1~ic1alamat2~ic1alamat3~ic2alamat1~ic2alamat2~ic2alamat3~icbagianpenjualan~icbagianpenagihan~icuraian~iccatatan~icnoref~ictglnoref~iccarabayar~ictglbayar~icmatauang~ickurs~ictotalap~ictotalapvalas~ictotalar~ictotalarvalas~icjmltagih~icjmltagihvalas~icbayar~icbayarvalas~icselisihkurs~icrekselisihkurs~icdiskontermin~icdiskonterminvalas~icrekdiskontermin~icstatuspv~icstatus~icstatussebelumnya~icjmlrevisi~iccetakanke~icinputuser~icinputtgl~icmodifikasiuser~icmodifikasitgl~icisclose~iccustomtext1~iccustomtext2~iccustomtext3~iccustomtext4~iccustomtext5~iccustomint1~iccustomint2~iccustomint3~iccustomdbl1~iccustomdbl2~iccustomdbl3~iccustomdate1~iccustomdate2~iccustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idicdetail(0) As Integer, idic(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, nogiro(14) As String, 
        'rekhutangpiutang(15) As String, catatan(16) As String, costcenter(17) As String, divisi(18) As String, subdivisi(19) As String, 
        'proyek(20) As String, jmlpv(21) As Double, jmlpvvalas(22) As Double, statuspv(23) As Double, urutan(24) As Integer, 
        'isclose(25) As Integer, customtext1(26) As String, customtext2(27) As String, customtext3(28) As String, customdbl1(29) As Double, 
        'customdbl2(30) As Double, customdbl3(31) As Double, customdate1(32) As Date, customdate2(33) As Date, customdate3(34) As Date, rencana(35) As String


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, 
        'jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rencana


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idicdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idic", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpv", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpvvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspv", AsEnumTypeData.AsString)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 36) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idicdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idicdetail required numeric." : GoTo selesai
            End If
            'idic(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idic required numeric." : GoTo selesai
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
            'rencana(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
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
            'jmlpv(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - jmlpv required numeric." : GoTo selesai
            End If
            'jmlpvvalas(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlpvvalas required numeric." : GoTo selesai
            End If
            'statuspv(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statuspv required numeric." : GoTo selesai
            End If
            'urutan(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
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
            If (dataRowDetail(2) <> "SI" And dataRowDetail(2) <> "AS" And dataRowDetail(2) <> "SR" And dataRowDetail(2) <> "CA" And dataRowDetail(2) <> "RP" And dataRowDetail(2) <> "IP") Then
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

            'rencana(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
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
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'jmlpv(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - jmlpv can't be empty" : GoTo selesai
            End If

            'jmlpvvalas(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlpvvalas can't be empty" : GoTo selesai
            End If

            'statuspv(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - statuspv can't be empty" : GoTo selesai
            End If

            'customdbl1(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idicdetail~idic~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~nogiro~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~jmlpv~jmlpvvalas~statuspv~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ictgl")), AsFormatTanggal(drutama("ictgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "icmatauang", "icrekselisihkurs~icrekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================

                If isUpdate Then
                    result(4) = drutama("icid")
                    notransaksi = drutama("icnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(icid), icnotransaksi FROM M5_IC WHERE icid='" & result(4) & "' AND icstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(icid) FROM M5_IC WHERE icnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_ic_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Ic_HistorySimpan("" & paramSplit(0) & "★M5_Ic_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("icsumber")) & "▼" & FixQuotes(drutama("icid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Ic set iccabang  = '" & FixQuotes(drutama("iccabang")) & "', iclokasi  = '" & FixQuotes(drutama("iclokasi")) & "', icgudang  = '" & FixQuotes(drutama("icgudang")) & "', icsumber  = '" & FixQuotes(drutama("icsumber")) & "', icautonotransaksi  = " & drutama("icautonotransaksi") & ", icnotransaksi  = '" & FixQuotes(notransaksi) & "', ictgl  = '" & FixQuotes(AsFormatTanggal(drutama("ictgl"))) & "', ickodepa  = " & drutama("ickodepa") & ", iccustomer  = " & drutama("iccustomer") & ", iccustomerkontak  = '" & FixQuotes(drutama("iccustomerkontak")) & "', ic1alamat1  = '" & FixQuotes(drutama("ic1alamat1")) & "', ic1alamat2  = '" & FixQuotes(drutama("ic1alamat2")) & "', ic1alamat3  = '" & FixQuotes(drutama("ic1alamat3")) & "', ic2alamat1  = '" & FixQuotes(drutama("ic2alamat1")) & "', ic2alamat2  = '" & FixQuotes(drutama("ic2alamat2")) & "', ic2alamat3  = '" & FixQuotes(drutama("ic2alamat3")) & "', icbagianpenjualan  = " & drutama("icbagianpenjualan") & ", icbagianpenagihan  = " & drutama("icbagianpenagihan") & ", icuraian  = '" & FixQuotes(drutama("icuraian")) & "', iccatatan  = '" & FixQuotes(drutama("iccatatan")) & "', icnoref  = '" & FixQuotes(drutama("icnoref")) & "', ictglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ictglnoref"))) & "', iccarabayar  = " & drutama("iccarabayar") & ", ictglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("ictglbayar"))) & "', icmatauang  = '" & FixQuotes(drutama("icmatauang")) & "', ickurs  = '" & FixDouble(drutama("ickurs")) & "', ictotalap  = '" & FixDouble(drutama("ictotalap")) & "', ictotalapvalas  = '" & FixDouble(drutama("ictotalapvalas")) & "', ictotalar  = '" & FixDouble(drutama("ictotalar")) & "', ictotalarvalas  = '" & FixDouble(drutama("ictotalarvalas")) & "', icjmltagih  = '" & FixDouble(drutama("icjmltagih")) & "', icjmltagihvalas  = '" & FixDouble(drutama("icjmltagihvalas")) & "', icbayar  = '" & FixDouble(drutama("icbayar")) & "', icbayarvalas  = '" & FixDouble(drutama("icbayarvalas")) & "', icselisihkurs  = '" & FixDouble(drutama("icselisihkurs")) & "', icrekselisihkurs  = '" & FixQuotes(drutama("icrekselisihkurs")) & "', icdiskontermin  = '" & FixDouble(drutama("icdiskontermin")) & "', icdiskonterminvalas  = '" & FixDouble(drutama("icdiskonterminvalas")) & "', icrekdiskontermin  = '" & FixQuotes(drutama("icrekdiskontermin")) & "', icstatuspv  = " & drutama("icstatuspv") & ", icstatus  = " & drutama("icstatus") & ", icstatussebelumnya  = " & drutama("icstatussebelumnya") & ", icjmlrevisi  = icjmlrevisi+1, iccetakanke  = " & drutama("iccetakanke") & ", icmodifikasiuser  = " & drutama("icmodifikasiuser") & ", icmodifikasitgl  = NOW(), iccustomtext1  = '" & FixQuotes(drutama("iccustomtext1")) & "', iccustomtext2  = '" & FixQuotes(drutama("iccustomtext2")) & "', iccustomtext3  = '" & FixQuotes(drutama("iccustomtext3")) & "', iccustomtext4  = '" & FixQuotes(drutama("iccustomtext4")) & "', iccustomtext5  = '" & FixQuotes(drutama("iccustomtext5")) & "', iccustomint1  = " & drutama("iccustomint1") & ", iccustomint2  = " & drutama("iccustomint2") & ", iccustomint3  = " & drutama("iccustomint3") & ", iccustomdbl1  = '" & FixDouble(drutama("iccustomdbl1")) & "', iccustomdbl2  = '" & FixDouble(drutama("iccustomdbl2")) & "', iccustomdbl3  = '" & FixDouble(drutama("iccustomdbl3")) & "', iccustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate1"))) & "', iccustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate2"))) & "', iccustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate3"))) & "' where icid = '" & drutama("icid") & "'"
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

                    If drutama("icautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("iccabang"), drutama("iclokasi"), drutama("icsumber"), drutama("ictgl"))
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
                        notransaksi = drutama("icnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(icid) FROM m5_ic WHERE icnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Ic (iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, iccustomdate3) values('" & FixQuotes(drutama("iccabang")) & "', '" & FixQuotes(drutama("iclokasi")) & "', '" & FixQuotes(drutama("icgudang")) & "', '" & FixQuotes(drutama("icsumber")) & "', " & drutama("icautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ictgl"))) & "', " & drutama("ickodepa") & ", " & drutama("iccustomer") & ", '" & FixQuotes(drutama("iccustomerkontak")) & "', '" & FixQuotes(drutama("ic1alamat1")) & "', '" & FixQuotes(drutama("ic1alamat2")) & "', '" & FixQuotes(drutama("ic1alamat3")) & "', '" & FixQuotes(drutama("ic2alamat1")) & "', '" & FixQuotes(drutama("ic2alamat2")) & "', '" & FixQuotes(drutama("ic2alamat3")) & "', " & drutama("icbagianpenjualan") & ", " & drutama("icbagianpenagihan") & ", '" & FixQuotes(drutama("icuraian")) & "', '" & FixQuotes(drutama("iccatatan")) & "', '" & FixQuotes(drutama("icnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ictglnoref"))) & "', " & drutama("iccarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("ictglbayar"))) & "', '" & FixQuotes(drutama("icmatauang")) & "', '" & FixDouble(drutama("ickurs")) & "', '" & FixDouble(drutama("ictotalap")) & "', '" & FixDouble(drutama("ictotalapvalas")) & "', '" & FixDouble(drutama("ictotalar")) & "', '" & FixDouble(drutama("ictotalarvalas")) & "', '" & FixDouble(drutama("icjmltagih")) & "', '" & FixDouble(drutama("icjmltagihvalas")) & "', '" & FixDouble(drutama("icbayar")) & "', '" & FixDouble(drutama("icbayarvalas")) & "', '" & FixDouble(drutama("icselisihkurs")) & "', '" & FixQuotes(drutama("icrekselisihkurs")) & "', '" & FixDouble(drutama("icdiskontermin")) & "', '" & FixDouble(drutama("icdiskonterminvalas")) & "', '" & FixQuotes(drutama("icrekdiskontermin")) & "', " & drutama("icstatuspv") & ", " & drutama("icstatus") & ", " & drutama("icstatussebelumnya") & ", " & drutama("icjmlrevisi") & ", " & drutama("iccetakanke") & ", " & drutama("icinputuser") & ", NOW(), " & drutama("icmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("icisclose") & ", '" & FixQuotes(drutama("iccustomtext1")) & "', '" & FixQuotes(drutama("iccustomtext2")) & "', '" & FixQuotes(drutama("iccustomtext3")) & "', '" & FixQuotes(drutama("iccustomtext4")) & "', '" & FixQuotes(drutama("iccustomtext5")) & "', " & drutama("iccustomint1") & ", " & drutama("iccustomint2") & ", " & drutama("iccustomint3") & ", '" & FixDouble(drutama("iccustomdbl1")) & "', '" & FixDouble(drutama("iccustomdbl2")) & "', '" & FixDouble(drutama("iccustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("iccustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select icid from M5_ic where icnotransaksi='" & notransaksi & "' AND icinputuser= '" & userid & "' order by icmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Ic_Detail where idic = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idicdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(dr1("jmlpv")) & "', '" & FixDouble(dr1("jmlpvvalas")) & "', '" & FixDouble(dr1("statuspv")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Ic_Detail(idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "IC", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_IcUpdateStatusOld(ByVal param As String) As String
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
            Dim sumber As String = "Ic", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ictgl, Icnotransaksi, Icstatus FROM M5_Ic WHERE Icid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Icstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_ic_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Ic_HistorySimpan("" & paramSplit(0) & "★M5_Ic_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_ic_terkait("icid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M5_Ic SET Icstatus = " & nilaiStatus & ", Icmodifikasiuser='" & userid & "', Icmodifikasitgl = NOW(), Icposting = 0, Icpostingtgl = '1971-01-01 00:00:00', Icjmlrevisi = Icjmlrevisi + 1 WHERE Icid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IcSearch(PostWsSearch(paramSplit(0), "M5_icSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_IcDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Ic", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Icid, Icnotransaksi FROM M5_Ic WHERE Icid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT iccabang, iclokasi, icsumber, icautonotransaksi, icnotransaksi, ictgl"
            sql &= " FROM M5_ic"
            sql &= " WHERE icid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("iccabang")
                lokasi = dtNomorNext.Rows(0)("iclokasi")
                sumber = dtNomorNext.Rows(0)("icsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("icautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("icnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ictgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Ic_Detail WHERE idic='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Ic WHERE icid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IcSearch(PostWsSearch(paramSplit(0), "M5_IcSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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