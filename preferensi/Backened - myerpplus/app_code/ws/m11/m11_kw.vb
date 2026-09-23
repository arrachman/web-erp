Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_kw
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""


    <WebMethod()>
    Public Function M11_KwSimpan(ByVal param As String) As String
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
        If (dataUtama.Length <> 68) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'icid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kwid required numeric." : GoTo selesai
        End If
        'icautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "kwautonotransaksi required numeric." : GoTo selesai
        End If
        'ictgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "kwtgl required date." : GoTo selesai
        End If
        'ickodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "kwkodepa required numeric." : GoTo selesai
        End If
        'iccustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "kwcustomer required numeric." : GoTo selesai
        End If
        'If (dataUtama(9) < 1) Then
        '    result(2) = "kwcustomer can't be empty." : GoTo selesai
        'End If
        'icbagianpenjualan(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "kwbagianpenjualan required numeric." : GoTo selesai
        End If
        'icbagianpenagihan(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "kwbagianpenagihan required numeric." : GoTo selesai
        End If
        'ictglnoref(22) As Date
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "kwtglnoref required date." : GoTo selesai
        End If
        'iccarabayar(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "kwcarabayar required numeric." : GoTo selesai
        End If
        'ictglbayar(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "kwtglbayar required date." : GoTo selesai
        End If
        'ickurs(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "kwkurs required numeric." : GoTo selesai
        End If
        'ictotalap(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "kwtotalap required numeric." : GoTo selesai
        End If
        'ictotalapvalas(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "kwtotalapvalas required numeric." : GoTo selesai
        End If
        'ictotalar(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "kwtotalar required numeric." : GoTo selesai
        End If
        'ictotalarvalas(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "kwtotalarvalas required numeric." : GoTo selesai
        End If
        'icjmltagih(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "kwjmltagih required numeric." : GoTo selesai
        End If
        'icjmltagihvalas(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "kwjmltagihvalas required numeric." : GoTo selesai
        End If
        'icbayar(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "kwbayar required numeric." : GoTo selesai
        End If
        'icbayarvalas(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "kwbayarvalas required numeric." : GoTo selesai
        End If
        'icselisihkurs(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "kwselisihkurs required numeric." : GoTo selesai
        End If
        'icdiskontermin(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "kwdiskontermin required numeric." : GoTo selesai
        End If
        'icdiskonterminvalas(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "kwdiskonterminvalas required numeric." : GoTo selesai
        End If
        'icstatuspv(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "kwstatuspb required numeric." : GoTo selesai
        End If
        'icstatus(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "kwstatus required numeric." : GoTo selesai
        End If
        'icstatussebelumnya(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "kwstatussebelumnya required numeric." : GoTo selesai
        End If
        'icjmlrevisi(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "kwjmlrevisi required numeric." : GoTo selesai
        End If
        'iccetakanke(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "kwcetakanke required numeric." : GoTo selesai
        End If
        'icinputuser(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "kwinputuser required numeric." : GoTo selesai
        End If
        'icinputtgl(46) As DateTime
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "kwinputtgl required date." : GoTo selesai
        End If
        'icmodifikasiuser(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "kwmodifikasiuser required numeric." : GoTo selesai
        End If
        'icmodifikasitgl(48) As DateTime
        If (IsDate(dataUtama(48)) = False) Then
            result(2) = "kwmodifikasitgl required date." : GoTo selesai
        End If
        'icisclose(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "kwisclose required numeric." : GoTo selesai
        End If
        'iccustomint1(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "kwcustomint1 required numeric." : GoTo selesai
        End If
        'iccustomint2(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "kwcustomint2 required numeric." : GoTo selesai
        End If
        'iccustomint3(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "kwcustomint3 required numeric." : GoTo selesai
        End If
        'iccustomdbl1(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "kwcustomdbl1 required numeric." : GoTo selesai
        End If
        'iccustomdbl2(59) As Double
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "kwcustomdbl2 required numeric." : GoTo selesai
        End If
        'iccustomdbl3(60) As Double
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "kwcustomdbl3 required numeric." : GoTo selesai
        End If
        'iccustomdate1(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "kwcustomdate1 required date." : GoTo selesai
        End If
        'iccustomdate2(62) As Date
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "kwcustomdate2 required date." : GoTo selesai
        End If
        'iccustomdate3(63) As Date
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "kwcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'iccabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kwcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "kwcabang should not be more than 25 character." : GoTo selesai
        End If

        'iclokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "kwlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "kwlokasi should not be more than 25 character." : GoTo selesai
        End If

        'kwsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "kwsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "kwsumber should not be more than 10 character." : GoTo selesai
        End If

        'kwnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "kwnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "kwnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kwtgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "kwtgl can't be empty" : GoTo selesai
        End If

        'kwtglnoref(22) As Date
        If Len(dataUtama(22)) = 0 Then
            result(2) = "kwtglnoref can't be empty" : GoTo selesai
        End If

        'kwtglbayar(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "kwtglbayar can't be empty" : GoTo selesai
        End If

        'kwkurs(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "kwkurs can't be empty" : GoTo selesai
        End If

        'kwtotalap(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "kwtotalap can't be empty" : GoTo selesai
        End If

        'kwtotalar(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "kwtotalar can't be empty" : GoTo selesai
        End If

        'kwinputtgl(46) As DateTime
        If Len(dataUtama(46)) = 0 Then
            result(2) = "kwinputtgl can't be empty" : GoTo selesai
        End If

        'kwmodifikasitgl(48) As DateTime
        If Len(dataUtama(48)) = 0 Then
            result(2) = "kwmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kwcustomdbl1(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "kwcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kwcustomdbl2(59) As Double
        If Len(dataUtama(59)) = 0 Then
            result(2) = "kwcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kwcustomdbl3(60) As Double
        If Len(dataUtama(60)) = 0 Then
            result(2) = "kwcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kwcustomdate1(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "kwcustomdate1 can't be empty" : GoTo selesai
        End If

        'kwcustomdate2(62) As Date
        If Len(dataUtama(62)) = 0 Then
            result(2) = "kwcustomdate2 can't be empty" : GoTo selesai
        End If

        'kwcustomdate3(63) As Date
        If Len(dataUtama(63)) = 0 Then
            result(2) = "kwcustomdate3 can't be empty" : GoTo selesai
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
        AsDataTableTambahField(dtutama, "kwid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kw1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kw1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kw1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kw2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kw2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kw2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwbagianpenagihan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwtglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwtotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwtotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwtotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwtotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwjmltagih", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwjmltagihvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwrekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwrekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwstatuspb", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwjenistransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kwtglkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kwdokter", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "kwid~kwcabang~kwlokasi~kwgudang~kwsumber~kwautonotransaksi~kwnotransaksi~kwtgl~kwkodepa~kwcustomer~kwcustomerkontak~kw1alamat1~kw1alamat2~kw1alamat3~kw2alamat1~kw2alamat2~kw2alamat3~kwbagianpenjualan~kwbagianpenagihan~kwuraian~kwcatatan~kwnoref~kwtglnoref~kwcarabayar~kwtglbayar~kwmatauang~kwkurs~kwtotalap~kwtotalapvalas~kwtotalar~kwtotalarvalas~kwjmltagih~kwjmltagihvalas~kwbayar~kwbayarvalas~kwselisihkurs~kwrekselisihkurs~kwdiskontermin~kwdiskonterminvalas~kwrekdiskontermin~kwstatuspb~kwstatus~kwstatussebelumnya~kwjmlrevisi~kwcetakanke~kwinputuser~kwinputtgl~kwmodifikasiuser~kwmodifikasitgl~kwisclose~kwcustomtext1~kwcustomtext2~kwcustomtext3~kwcustomtext4~kwcustomtext5~kwcustomint1~kwcustomint2~kwcustomint3~kwcustomdbl1~kwcustomdbl2~kwcustomdbl3~kwcustomdate1~kwcustomdate2~kwcustomdate3~kwjenistransaksi~kwpetugas~kwtglkeluar~kwdokter", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idkwdetail(0) As Integer, idkw(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, nogiro(14) As String, 
        'rekhutangpiutang(15) As String, catatan(16) As String, costcenter(17) As String, divisi(18) As String, subdivisi(19) As String, 
        'proyek(20) As String, jmlpv(21) As Double, jmlpvvalas(22) As Double, statuspv(23) As Double, urutan(24) As Integer, 
        'isclose(25) As Integer, customtext1(26) As String, customtext2(27) As String, customtext3(28) As String, customdbl1(29) As Double, 
        'customdbl2(30) As Double, customdbl3(31) As Double, customdate1(32) As Date, customdate2(33) As Date, customdate3(34) As Date, rencana(35) As String


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idkwdetail, idkw, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
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
        AsDataTableTambahField(dtdetail, "idkwdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idkw", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "jmlpb", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpbvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspb", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "uraian", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 37) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idkwdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idkwdetail required numeric." : GoTo selesai
            End If
            'idkw(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idkw required numeric." : GoTo selesai
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
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - rencana required numeric." : GoTo selesai
            End If
            'sisa(8) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - sisa required numeric." : GoTo selesai
            End If
            'jmlbayar(9) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbayar required numeric." : GoTo selesai
            End If
            'jmlbayarvalas(10) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - jmlbayarvalas required numeric." : GoTo selesai
            End If
            'jmldiskontermin(12) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmldiskontermin required numeric." : GoTo selesai
            End If
            'jmldiskonterminvalas(13) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas required numeric." : GoTo selesai
            End If
            'jmlpv(21) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlpb required numeric." : GoTo selesai
            End If
            'jmlpvvalas(22) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - jmlpbvalas required numeric." : GoTo selesai
            End If
            'statuspv(23) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - statuspb required numeric." : GoTo selesai
            End If
            'urutan(24) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(25) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(29) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(30) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(31) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(32) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(33) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(34) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
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
            If (dataRowDetail(2) <> "KJ" And dataRowDetail(2) <> "LU" And dataRowDetail(2) <> "AK" And dataRowDetail(2) <> "LB" And dataRowDetail(2) <> "CA") Then
                result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
            End If

            'matauang(4) As String
            'If Len(dataRowDetail(4)) = 0 Then
            '    result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(4)) > 25 Then
            '    result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            'End If

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
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - rencana can't be empty" : GoTo selesai
            End If

            'sisa(8) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - sisa can't be empty" : GoTo selesai
            End If

            'jmlbayar(9) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayar can't be empty" : GoTo selesai
            End If

            'jmlbayarvalas(10) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayarvalas can't be empty" : GoTo selesai
            End If

            'diskontermin(11) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - diskontermin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - diskontermin should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskontermin(12) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskontermin can't be empty" : GoTo selesai
            End If

            'jmldiskonterminvalas(13) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas can't be empty" : GoTo selesai
            End If

            'rekhutangpiutang(15) As String
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(16)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'jmlpv(21) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlpb can't be empty" : GoTo selesai
            End If

            'jmlpvvalas(22) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - jmlpbvalas can't be empty" : GoTo selesai
            End If

            'statuspv(23) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - statuspb can't be empty" : GoTo selesai
            End If

            'customdbl1(29) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(30) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(31) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(32) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(33) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(34) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idkwdetail~idkw~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~rencana~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~nogiro~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~jmlpb~jmlpbvalas~statuspb~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~uraian", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36)) = False Then
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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 50
                Select Case drutama("kwstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("kwtgl")), AsFormatTanggal(drutama("kwtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "kwmatauang", "kwrekselisihkurs~kwrekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================

                If isUpdate Then
                    result(4) = drutama("kwid")
                    notransaksi = drutama("kwnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(kwid), kwnotransaksi FROM m_11_kw WHERE kwid='" & result(4) & "' AND kwstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kwid) FROM m_11_kw WHERE kwnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m5_ic_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M5_Ic_HistorySimpan("" & paramSplit(0) & "★M5_Ic_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("icsumber")) & "▼" & FixQuotes(drutama("icid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update m_11_kw set kwcabang  = '" & FixQuotes(drutama("kwcabang")) & "', kwlokasi  = '" & FixQuotes(drutama("kwlokasi")) & "', kwgudang  = '" & FixQuotes(drutama("kwgudang")) & "', kwsumber  = '" & FixQuotes(drutama("kwsumber")) & "', kwautonotransaksi  = " & drutama("kwautonotransaksi") & ", kwnotransaksi  = '" & FixQuotes(notransaksi) & "', kwtgl  = '" & FixQuotes(AsFormatTanggal(drutama("kwtgl"))) & "', kwkodepa  = " & drutama("kwkodepa") & ", kwcustomer  = " & drutama("kwcustomer") & ", kwcustomerkontak  = '" & FixQuotes(drutama("kwcustomerkontak")) & "', kw1alamat1  = '" & FixQuotes(drutama("kw1alamat1")) & "', kw1alamat2  = '" & FixQuotes(drutama("kw1alamat2")) & "', kw1alamat3  = '" & FixQuotes(drutama("kw1alamat3")) & "', kw2alamat1  = '" & FixQuotes(drutama("kw2alamat1")) & "', kw2alamat2  = '" & FixQuotes(drutama("kw2alamat2")) & "', kw2alamat3  = '" & FixQuotes(drutama("kw2alamat3")) & "', kwbagianpenjualan  = " & drutama("kwbagianpenjualan") & ", kwbagianpenagihan  = " & drutama("kwbagianpenagihan") & ", kwuraian  = '" & FixQuotes(drutama("kwuraian")) & "', kwcatatan  = '" & FixQuotes(drutama("kwcatatan")) & "', kwnoref  = '" & FixQuotes(drutama("kwnoref")) & "', kwtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("kwtglnoref"))) & "', kwcarabayar  = " & drutama("kwcarabayar") & ", kwtglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("kwtglbayar"))) & "', kwmatauang  = '" & FixQuotes(drutama("kwmatauang")) & "', kwkurs  = '" & FixDouble(drutama("kwkurs")) & "', kwtotalap  = '" & FixDouble(drutama("kwtotalap")) & "', kwtotalapvalas  = '" & FixDouble(drutama("kwtotalapvalas")) & "', kwtotalar  = '" & FixDouble(drutama("kwtotalar")) & "', kwtotalarvalas  = '" & FixDouble(drutama("kwtotalarvalas")) & "', kwjmltagih  = '" & FixDouble(drutama("kwjmltagih")) & "', kwjmltagihvalas  = '" & FixDouble(drutama("kwjmltagihvalas")) & "', kwbayar  = '" & FixDouble(drutama("kwbayar")) & "', kwbayarvalas  = '" & FixDouble(drutama("kwbayarvalas")) & "', kwselisihkurs  = '" & FixDouble(drutama("kwselisihkurs")) & "', kwrekselisihkurs  = '" & FixQuotes(drutama("kwrekselisihkurs")) & "', kwdiskontermin  = '" & FixDouble(drutama("kwdiskontermin")) & "', kwdiskonterminvalas  = '" & FixDouble(drutama("kwdiskonterminvalas")) & "', kwrekdiskontermin  = '" & FixQuotes(drutama("kwrekdiskontermin")) & "', kwstatuspb  = " & drutama("kwstatuspb") & ", kwstatus  = " & drutama("kwstatus") & ", kwstatussebelumnya  = " & drutama("kwstatussebelumnya") & ", kwjmlrevisi  = kwjmlrevisi+1, kwcetakanke  = " & drutama("kwcetakanke") & ", kwmodifikasiuser  = " & drutama("kwmodifikasiuser") & ", kwmodifikasitgl  = NOW(), kwcustomtext1  = '" & FixQuotes(drutama("kwcustomtext1")) & "', kwcustomtext2  = '" & FixQuotes(drutama("kwcustomtext2")) & "', kwcustomtext3  = '" & FixQuotes(drutama("kwcustomtext3")) & "', kwcustomtext4  = '" & FixQuotes(drutama("kwcustomtext4")) & "', kwcustomtext5  = '" & FixQuotes(drutama("kwcustomtext5")) & "', kwcustomint1  = " & drutama("kwcustomint1") & ", kwcustomint2  = " & drutama("kwcustomint2") & ", kwcustomint3  = " & drutama("kwcustomint3") & ", kwcustomdbl1  = '" & FixDouble(drutama("kwcustomdbl1")) & "', kwcustomdbl2  = '" & FixDouble(drutama("kwcustomdbl2")) & "', kwcustomdbl3  = '" & FixDouble(drutama("kwcustomdbl3")) & "', kwcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("kwcustomdate1"))) & "', kwcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("kwcustomdate2"))) & "', kwcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("kwcustomdate3"))) & "', kwjenistransaksi = " & drutama("kwjenistransaksi") & ", kwpetugas = " & drutama("kwpetugas") & ", kwtglkeluar = '" & FixQuotes(AsFormatTanggal(drutama("kwtglkeluar"))) & "', kwdokter = '" & FixQuotes(drutama("kwdokter")) & "' where kwid = '" & drutama("kwid") & "'"
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

                    If drutama("kwautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("kwcabang"), drutama("kwlokasi"), drutama("kwsumber"), drutama("kwtgl"))
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
                        notransaksi = drutama("kwnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kwid) FROM m_11_kw WHERE kwnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into m_11_kw (kwcabang, kwlokasi, kwgudang, kwsumber, kwautonotransaksi, kwnotransaksi, kwtgl, kwkodepa, kwcustomer, kwcustomerkontak, kw1alamat1, kw1alamat2, kw1alamat3, kw2alamat1, kw2alamat2, kw2alamat3, kwbagianpenjualan, kwbagianpenagihan, kwuraian, kwcatatan, kwnoref, kwtglnoref, kwcarabayar, kwtglbayar, kwmatauang, kwkurs, kwtotalap, kwtotalapvalas, kwtotalar, kwtotalarvalas, kwjmltagih, kwjmltagihvalas, kwbayar, kwbayarvalas, kwselisihkurs, kwrekselisihkurs, kwdiskontermin, kwdiskonterminvalas, kwrekdiskontermin, kwstatuspb, kwstatus, kwstatussebelumnya, kwjmlrevisi, kwcetakanke, kwinputuser, kwinputtgl, kwmodifikasiuser, kwmodifikasitgl, kwisclose, kwcustomtext1, kwcustomtext2, kwcustomtext3, kwcustomtext4, kwcustomtext5, kwcustomint1, kwcustomint2, kwcustomint3, kwcustomdbl1, kwcustomdbl2, kwcustomdbl3, kwcustomdate1, kwcustomdate2, kwcustomdate3, kwjenistransaksi, kwpetugas, kwtglkeluar, kwdokter) values('" & FixQuotes(drutama("kwcabang")) & "', '" & FixQuotes(drutama("kwlokasi")) & "', '" & FixQuotes(drutama("kwgudang")) & "', '" & FixQuotes(drutama("kwsumber")) & "', " & drutama("kwautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("kwtgl"))) & "', " & drutama("kwkodepa") & ", " & drutama("kwcustomer") & ", '" & FixQuotes(drutama("kwcustomerkontak")) & "', '" & FixQuotes(drutama("kw1alamat1")) & "', '" & FixQuotes(drutama("kw1alamat2")) & "', '" & FixQuotes(drutama("kw1alamat3")) & "', '" & FixQuotes(drutama("kw2alamat1")) & "', '" & FixQuotes(drutama("kw2alamat2")) & "', '" & FixQuotes(drutama("kw2alamat3")) & "', " & drutama("kwbagianpenjualan") & ", " & drutama("kwbagianpenagihan") & ", '" & FixQuotes(drutama("kwuraian")) & "', '" & FixQuotes(drutama("kwcatatan")) & "', '" & FixQuotes(drutama("kwnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kwtglnoref"))) & "', " & drutama("kwcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("kwtglbayar"))) & "', '" & FixQuotes(drutama("kwmatauang")) & "', '" & FixDouble(drutama("kwkurs")) & "', '" & FixDouble(drutama("kwtotalap")) & "', '" & FixDouble(drutama("kwtotalapvalas")) & "', '" & FixDouble(drutama("kwtotalar")) & "', '" & FixDouble(drutama("kwtotalarvalas")) & "', '" & FixDouble(drutama("kwjmltagih")) & "', '" & FixDouble(drutama("kwjmltagihvalas")) & "', '" & FixDouble(drutama("kwbayar")) & "', '" & FixDouble(drutama("kwbayarvalas")) & "', '" & FixDouble(drutama("kwselisihkurs")) & "', '" & FixQuotes(drutama("kwrekselisihkurs")) & "', '" & FixDouble(drutama("kwdiskontermin")) & "', '" & FixDouble(drutama("kwdiskonterminvalas")) & "', '" & FixQuotes(drutama("kwrekdiskontermin")) & "', " & drutama("kwstatuspb") & ", " & drutama("kwstatus") & ", " & drutama("kwstatussebelumnya") & ", " & drutama("kwjmlrevisi") & ", " & drutama("kwcetakanke") & ", " & drutama("kwinputuser") & ", NOW(), " & drutama("kwmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("kwisclose") & ", '" & FixQuotes(drutama("kwcustomtext1")) & "', '" & FixQuotes(drutama("kwcustomtext2")) & "', '" & FixQuotes(drutama("kwcustomtext3")) & "', '" & FixQuotes(drutama("kwcustomtext4")) & "', '" & FixQuotes(drutama("kwcustomtext5")) & "', " & drutama("kwcustomint1") & ", " & drutama("kwcustomint2") & ", " & drutama("kwcustomint3") & ", '" & FixDouble(drutama("kwcustomdbl1")) & "', '" & FixDouble(drutama("kwcustomdbl2")) & "', '" & FixDouble(drutama("kwcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kwcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kwcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kwcustomdate3"))) & "', " & drutama("kwjenistransaksi") & ", " & drutama("kwpetugas") & ", '" & FixQuotes(AsFormatTanggal(drutama("kwtglkeluar"))) & "', '" & FixQuotes(drutama("kwdokter")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select kwid from m_11_kw where kwnotransaksi='" & notransaksi & "' AND kwinputuser= '" & userid & "' order by kwmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from m_11_kw_detail where idkw = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idkwdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(dr1("jmlpb")) & "', '" & FixDouble(dr1("jmlpbvalas")) & "', '" & FixDouble(dr1("statuspb")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "','" & FixQuotes(dr1("uraian")) & "')")
                    Next
                    sql = "Insert into m_11_kw_detail(idkwdetail, idkw, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpb, jmlpbvalas, statuspb, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, uraian) values" & strValue2.ToString & ""
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

                If drutama("kwstatus") = 2 Then
                    If drutama("kwjenistransaksi") = 0 Then 'dengan kunjungan
                        If (dtdetail.Rows.Count > 0) Then
                            For Each dr2 As DataRow In dtdetail.Rows
                                sql = "UPDATE m_11_kj SET kjtglkeluar = '" & FixQuotes(AsFormatTanggal(drutama("kwtglkeluar"))) & "', kjstatus = 4, kjdokter = '" & FixQuotes(drutama("kwdokter")) & "' WHERE kjid = '" & (dr2("idtransaksi")) & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                sql = "UPDATE m_11_lu SET lustatus = 4 WHERE luidkj = '" & (dr2("idtransaksi")) & "' AND lustatus IN (2,3)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                sql = "UPDATE m_11_km SET kmstatus = 4 WHERE kmidkj = '" & (dr2("idtransaksi")) & "' AND kmstatus IN (2,3)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                Dim dtCekDataAK As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(akid) FROM m_11_ak WHERE akidkj = '" & (dr2("idtransaksi")) & "' AND akidkj <> 0 AND akpenjualanlangsung = 0", myConn)
                                Dim CekDataAK As Double = Val(dtCekDataAK.Rows(0)(0))
                                If CekDataAK > 0 Then
                                    sql = "UPDATE m_11_ak SET akstatus = 4, aktglbayar = '" & FixQuotes(AsFormatTanggal(drutama("kwtgl"))) & "' WHERE akidkj = '" & (dr2("idtransaksi")) & "' AND akidkj <> 0 AND akstatus IN (2,3) AND akpenjualanlangsung = 0"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                                Dim dtCekDataLB As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid) FROM m_11_lb WHERE lbidkj = '" & (dr2("idtransaksi")) & "' AND lbidkj <> 0 AND lbpenjualanlangsung = 0", myConn)
                                Dim CekDataLB As Double = Val(dtCekDataLB.Rows(0)(0))
                                If CekDataLB > 0 Then
                                    sql = "UPDATE m_11_lb SET lbstatus = 4 WHERE lbidkj = '" & (dr2("idtransaksi")) & "' AND lbidkj <> 0 AND lbstatus IN (2,3) AND lbpenjualanlangsung = 0"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                                sql = "UPDATE m_11_rk SET rkstatus = 4 WHERE rkidkj = '" & (dr2("idtransaksi")) & "' AND rkstatus IN (2,3)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                sql = "UPDATE m_11_ro SET rostatus = 4 WHERE roidkj = '" & (dr2("idtransaksi")) & "' AND rostatus IN (2,3)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            Next
                        End If
                    ElseIf drutama("kwjenistransaksi") = 1 Then 'tanpa kunjungan
                        If (dtdetail.Rows.Count > 0) Then
                            For Each dr3 As DataRow In dtdetail.Rows
                                If dr3("sumber") = "AK" Then
                                    Dim dtCekDataAKK As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(akid) FROM m_11_ak WHERE akid = '" & (dr3("idtransaksi")) & "' AND akidkj = 0 AND akpenjualanlangsung = 1", myConn)
                                    Dim CekDataAKK As Double = Val(dtCekDataAKK.Rows(0)(0))
                                    If CekDataAKK > 0 Then
                                        sql = "UPDATE m_11_ak SET akstatus = 4, aktglbayar = '" & FixQuotes(AsFormatTanggal(drutama("kwtgl"))) & "' WHERE akid = '" & (dr3("idtransaksi")) & "' AND akidkj = 0 AND akstatus = 2 AND akpenjualanlangsung = 1"
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

                                If dr3("sumber") = "LU" Then
                                    sql = "UPDATE m_11_lu SET lustatus = 4 WHERE luid = '" & (dr3("idtransaksi")) & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                                If dr3("sumber") = "LB" Then
                                    Dim dtCekDataLBB As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid) FROM m_11_lb WHERE lbid = '" & (dr3("idtransaksi")) & "' AND lbidkj = 0 AND lbpenjualanlangsung = 1", myConn)
                                    Dim CekDataLBB As Double = Val(dtCekDataLBB.Rows(0)(0))
                                    If CekDataLBB > 0 Then
                                        sql = "UPDATE m_11_lb SET lbstatus = 4 WHERE lbid = '" & (dr3("idtransaksi")) & "' AND lbidkj = 0 AND lbstatus = 2 AND lbpenjualanlangsung = 1"
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

                                If dr3("sumber") = "KM" Then
                                    sql = "UPDATE m_11_km SET kmstatus = 4 WHERE kmid = '" & (dr3("idtransaksi")) & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                                If dr3("sumber") = "RK" Then
                                    sql = "UPDATE m_11_rk SET rkstatus = 4 WHERE rkid = '" & (dr3("idtransaksi")) & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                                If dr3("sumber") = "RO" Then
                                    sql = "UPDATE m_11_ro SET rostatus = 4 WHERE roid = '" & (dr3("idtransaksi")) & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                            Next
                        End If
                    End If
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "KW", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("kwstatus") = 2 Then
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
    Public Function M11_KwUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Kw", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT kwtgl, kwnotransaksi, kwstatus FROM m_11_kw WHERE kwid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "kwstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m5_ic_history
            'Dim rsSimpanHistory As String = SimpanHistory.M5_Ic_HistorySimpan("" & paramSplit(0) & "★M5_Ic_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                'sql = query.m11_kw_terkait("kwid = '" & idtransaksi & "'")

                sql = query.PanggilQuery("m11_kw_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)

                myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                myConn.Open()

                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE TRANSAKSI DETAIL ========================================================
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT kwd.sumber, kwd.idtransaksi, kwd.urutan, kw.kwjenistransaksi FROM m_11_kw_detail kwd JOIN m_11_kw kw ON kwd.idkw = kw.kwid WHERE kw.kwid = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then

                    'AMBIL JENIS TRANSAKSI
                    '0 : DENGAN KUNJUNGAN, 1: TANPA KUNJUNGAN
                    Dim kwjenistransaksi As Integer = 0
                    If Len(FxDB(dtdetail.Rows(0)("kwjenistransaksi"), "")) > 0 Then
                        kwjenistransaksi = Integer.Parse(FxDB(dtdetail.Rows(0)("kwjenistransaksi"), 0))
                    End If

                    For Each dr1 As DataRow In dtdetail.Rows
                        If kwjenistransaksi = 0 Then
                            'DENGAN KUNJUNGAN
                            sql = "UPDATE m_11_kj SET kjtglkeluar = '1900-01-01', kjstatus = kjstatussebelumnya WHERE kjid = '" & (dr1("idtransaksi")) & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "UPDATE m_11_lu SET lustatus = lustatussebelumnya WHERE luidkj = '" & (dr1("idtransaksi")) & "' AND lustatus IN (4)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "UPDATE m_11_km SET kmstatus = kmstatussebelumnya WHERE kmidkj = '" & (dr1("idtransaksi")) & "' AND kmstatus IN (4)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "UPDATE m_11_ak SET akstatus = akstatussebelumnya WHERE akidkj = '" & (dr1("idtransaksi")) & "' AND akpenjualanlangsung = 0 AND akstatus IN (4)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "UPDATE m_11_lb SET lbstatus = lbstatussebelumnya WHERE lbidkj = '" & (dr1("idtransaksi")) & "' AND lbpenjualanlangsung = 0 AND lbstatus IN (4)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "UPDATE m_11_rk SET rkstatus = rkstatussebelumnya WHERE rkidkj = '" & (dr1("idtransaksi")) & "' AND rkstatus IN (4)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "UPDATE m_11_ro SET rostatus = rostatussebelumnya WHERE roidkj = '" & (dr1("idtransaksi")) & "' AND rostatus IN (4)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        Else
                            'TANPA KUNJUNGAN
                            If dr1("sumber") = "AK" Then
                                sql = "UPDATE m_11_ak SET akstatus = akstatussebelumnya WHERE akid = '" & (dr1("idtransaksi")) & "' AND akpenjualanlangsung = 1 AND akstatus = 4"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If

                            If dr1("sumber") = "LU" Then
                                sql = "UPDATE m_11_lu SET lustatus = lustatussebelumnya WHERE luid = '" & (dr1("idtransaksi")) & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If

                            If dr1("sumber") = "LB" Then
                                sql = "UPDATE m_11_lb SET lbstatus = lbstatussebelumnya WHERE lbid = '" & (dr1("idtransaksi")) & "' AND lbpenjualanlangsung = 1 AND lbstatus = 4"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If

                            If dr1("sumber") = "KM" Then
                                sql = "UPDATE m_11_km SET kmstatus = kmstatussebelumnya WHERE kmid = '" & (dr1("idtransaksi")) & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If

                            If dr1("sumber") = "RK" Then
                                sql = "UPDATE m_11_rk SET rkstatus = rkstatussebelumnya WHERE rkid = '" & (dr1("idtransaksi")) & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If

                            If dr1("sumber") = "RO" Then
                                sql = "UPDATE m_11_ro SET rostatus = rostatussebelumnya WHERE roid = '" & (dr1("idtransaksi")) & "'"
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
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF UPDATE TRANSAKSI DETAIL =================================================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'KW' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE m_11_kw SET kwstatus = " & nilaiStatus & ", kwmodifikasiuser='" & userid & "', kwmodifikasitgl = NOW(), kwposting = 0, kwpostingtgl = '1971-01-01 00:00:00', kwjmlrevisi = kwjmlrevisi + 1 WHERE kwid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_KwSearch(PostWsSearch(paramSplit(0), "M11_KwSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_KwDelete(ByVal param As String) As String

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
            Dim sumber As String = "Kw", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Kwid, Kwnotransaksi FROM m_11_kw WHERE kwid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT kwcabang, kwlokasi, kwsumber, kwautonotransaksi, kwnotransaksi, kwtgl"
            sql &= " FROM m_11_kw"
            sql &= " WHERE kwid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("kwcabang")
                lokasi = dtNomorNext.Rows(0)("kwlokasi")
                sumber = dtNomorNext.Rows(0)("kwsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("kwautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("kwnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("kwtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM m_11_kw_detail WHERE idkw='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM m_11_kw WHERE kwid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_KwSearch(PostWsSearch(paramSplit(0), "M11_KwSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M11_KwGetdataById(ByVal param As String) As String
        'M11_KwGetdataById Utama --------------------------------------------------------
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
        'icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama

        'M11_Kw_GetdataById Detail --------------------------------------------------------
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

        Dim NmMemcached As String = "aplikasi1-M11_Kw~M11_Kw_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "kwid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "kwid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        icl = query.PanggilQuery("m11_kw_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , icl) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kwid"), 0), sptField,
                     FxDB(drutama("kwcabang"), ""), sptField,
                     FxDB(drutama("kwlokasi"), ""), sptField,
                     FxDB(drutama("kwgudang"), ""), sptField,
                     FxDB(drutama("kwsumber"), ""), sptField,
                     FxDB(drutama("kwautonotransaksi"), 0), sptField,
                     FxDB(drutama("kwnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kwtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("kwkodepa"), 0), sptField,
                     FxDB(drutama("kwcustomer"), 0), sptField,
                     FxDB(drutama("kwcustomerkontak"), ""), sptField,
                     FxDB(drutama("kw1alamat1"), ""), sptField,
                     FxDB(drutama("kw1alamat2"), ""), sptField,
                     FxDB(drutama("kw1alamat3"), ""), sptField,
                     FxDB(drutama("kw2alamat1"), ""), sptField,
                     FxDB(drutama("kw2alamat2"), ""), sptField,
                     FxDB(drutama("kw2alamat3"), ""), sptField,
                     FxDB(drutama("kwbagianpenjualan"), 0), sptField,
                     FxDB(drutama("kwbagianpenagihan"), 0), sptField,
                     FxDB(drutama("kwuraian"), ""), sptField,
                     FxDB(drutama("kwcatatan"), ""), sptField,
                     FxDB(drutama("kwnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kwtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("kwcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kwtglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("kwmatauang"), ""), sptField,
                     FxDB(drutama("kwkurs"), 0), sptField,
                     FxDB(drutama("kwtotalap"), 0), sptField,
                     FxDB(drutama("kwtotalapvalas"), 0), sptField,
                     FxDB(drutama("kwtotalar"), 0), sptField,
                     FxDB(drutama("kwtotalarvalas"), 0), sptField,
                     FxDB(drutama("kwjmltagih"), 0), sptField,
                     FxDB(drutama("kwjmltagihvalas"), 0), sptField,
                     FxDB(drutama("kwbayar"), 0), sptField,
                     FxDB(drutama("kwbayarvalas"), 0), sptField,
                     FxDB(drutama("kwselisihkurs"), 0), sptField,
                     FxDB(drutama("kwrekselisihkurs"), ""), sptField,
                     FxDB(drutama("kwdiskontermin"), 0), sptField,
                     FxDB(drutama("kwdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("kwrekdiskontermin"), ""), sptField,
                     FxDB(drutama("kwstatuspb"), 0), sptField,
                     FxDB(drutama("kwstatus"), 0), sptField,
                     FxDB(drutama("kwstatussebelumnya"), 0), sptField,
                     FxDB(drutama("kwjmlrevisi"), 0), sptField,
                     FxDB(drutama("kwcetakanke"), 0), sptField,
                     FxDB(drutama("kwinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kwinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kwmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kwmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kwposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kwpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kwisclose"), 0), sptField,
                     FxDB(drutama("kwcustomtext1"), ""), sptField,
                     FxDB(drutama("kwcustomtext2"), ""), sptField,
                     FxDB(drutama("kwcustomtext3"), ""), sptField,
                     FxDB(drutama("kwcustomtext4"), ""), sptField,
                     FxDB(drutama("kwcustomtext5"), ""), sptField,
                     FxDB(drutama("kwcustomint1"), 0), sptField,
                     FxDB(drutama("kwcustomint2"), 0), sptField,
                     FxDB(drutama("kwcustomint3"), 0), sptField,
                     FxDB(drutama("kwcustomdbl1"), 0), sptField,
                     FxDB(drutama("kwcustomdbl2"), 0), sptField,
                     FxDB(drutama("kwcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kwcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kwcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kwcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("kwcabangnama"), ""), sptField,
                     FxDB(drutama("kwlokasinama"), ""), sptField,
                     FxDB(drutama("kwgudangnama"), ""), sptField,
                     FxDB(drutama("kwcustomerkode"), ""), sptField,
                     FxDB(drutama("kwcustomernama"), ""), sptField,
                     FxDB(drutama("kwbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("kwbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("kwbagianpenagihankode"), ""), sptField,
                     FxDB(drutama("kwbagianpenagihannama"), ""), sptField,
                     FxDB(drutama("kwcarabayarnama"), ""), sptField,
                     FxDB(drutama("kwrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("kwrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("kwstatusnama"), ""), sptField,
                     FxDB(drutama("kwstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("kwinputusernama"), ""), sptField,
                     FxDB(drutama("kwmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kwjenistransaksi"), 0), sptField,
                     FxDB(drutama("kwpetugas"), 0), sptField,
                     FxDB(drutama("kwpetugaskode"), ""), sptField,
                     FxDB(drutama("kwpetugasnama"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kwtglkeluar"), ""), formatTgl), sptField,
                     FxDB(drutama("kwdokter"), ""), sptField,
                     FxDB(drutama("kwdokternama"), ""))

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

                detail = String.Concat(detail, FxDB(dr("idkwdetail"), 0), sptField,
                     FxDB(dr("idkw"), 0), sptField,
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
                     FxDB(dr("jmlpb"), 0), sptField,
                     FxDB(dr("jmlpbvalas"), 0), sptField,
                     FxDB(dr("statuspb"), 0), sptField,
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
                     FxDB(dr("notransaksikw"), ""), sptField,
                     inputtgl, sptField,
                     FxDB(dr("uraian"), ""), sptField,
                     FxDB(dr("norm"), 0), sptField,
                     FxDB(dr("nama"), ""), sptField,
                     FxDB(dr("kategoripasien"), ""), sptField,
                     FxDB(dr("ditanggungoleh"), ""), sptField,
                     FxDB(dr("kamar"), ""), sptField,
                     FxDB(dr("alamat"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kwid, kwcabang, kwlokasi, kwgudang, kwsumber, kwautonotransaksi, kwnotransaksi, kwtgl, kwkodepa, kwcustomer, kwcustomerkontak, kw1alamat1, kw1alamat2, kw1alamat3, kw2alamat1, kw2alamat2, kw2alamat3, kwbagianpenjualan, kwbagianpenagihan, kwuraian, kwcatatan, kwnoref, kwtglnoref, kwcarabayar, kwtglbayar, kwmatauang, kwkurs, kwtotalap, kwtotalapvalas, kwtotalar, kwtotalarvalas, kwjmltagih, kwjmltagihvalas, kwbayar, kwbayarvalas, kwselisihkurs, kwrekselisihkurs, kwdiskontermin, kwdiskonterminvalas, kwrekdiskontermin, kwstatuspv, kwstatus, kwstatussebelumnya, kwjmlrevisi, kwcetakanke, kwinputuser, kwinputtgl, kwmodifikasiuser, kwmodifikasitgl, kwposting, kwpostingtgl, kwisclose, kwcustomtext1, kwcustomtext2, kwcustomtext3, kwcustomtext4, kwcustomtext5, kwcustomint1, kwcustomint2, kwcustomint3, kwcustomdbl1, kwcustomdbl2, kwcustomdbl3, kwcustomdate1, kwcustomdate2, kwcustomdate3, kwcabangnama, kwlokasinama, kwgudangnama, kwcustomerkode, kwcustomernama, kwbagianpenjualankode, kwbagianpenjualannama, kwbagianpenagihankode, kwbagianpenagihannama, kwcarabayarnama, kwrekselisihkursnama, kwrekdiskonterminnama, kwstatusnama, kwstatussebelumnyanama, kwinputusernama, kwmodifikasiusernama, kwjenistransaksi, kwpetugas, kwpetugaskode, kwpetugasnama, kwtglkeluar, kwdokter, kwdokternama" & sptSubParam & "idkwdetail, idkw, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpb, jmlpbvalas, statuspb, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksikw, inputtgl, uraian, norm, nama, kategoripasien, ditanggungoleh, kamar, alamat"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KwSearch(ByVal param As String) As String
        'M11_KwSearch --------------------------------------------------------
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
            Filter = Filter.Replace("kwnorm", "p.pkode")
            Filter = Filter.Replace("kwnama", "p.pnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_kw_v")

        dt = AmbilData("aplikasi1-m11_kw_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kwid"), 0), sptField,
                     FxDB(dr("kwcabang"), ""), sptField,
                     FxDB(dr("kwlokasi"), ""), sptField,
                     FxDB(dr("kwgudang"), ""), sptField,
                     FxDB(dr("kwsumber"), ""), sptField,
                     FxDB(dr("kwautonotransaksi"), 0), sptField,
                     FxDB(dr("kwnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kwtgl"), ""), formatTgl), sptField,
                     FxDB(dr("kwkodepa"), 0), sptField,
                     FxDB(dr("kwcustomer"), 0), sptField,
                     FxDB(dr("kwcustomerkontak"), ""), sptField,
                     FxDB(dr("kw1alamat1"), ""), sptField,
                     FxDB(dr("kw1alamat2"), ""), sptField,
                     FxDB(dr("kw1alamat3"), ""), sptField,
                     FxDB(dr("kw2alamat1"), ""), sptField,
                     FxDB(dr("kw2alamat2"), ""), sptField,
                     FxDB(dr("kw2alamat3"), ""), sptField,
                     FxDB(dr("kwbagianpenjualan"), 0), sptField,
                     FxDB(dr("kwbagianpenagihan"), 0), sptField,
                     FxDB(dr("kwuraian"), ""), sptField,
                     FxDB(dr("kwcatatan"), ""), sptField,
                     FxDB(dr("kwnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kwtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("kwcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kwtglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("kwmatauang"), ""), sptField,
                     FxDB(dr("kwkurs"), 0), sptField,
                     FxDB(dr("kwtotalap"), 0), sptField,
                     FxDB(dr("kwtotalapvalas"), 0), sptField,
                     FxDB(dr("kwtotalar"), 0), sptField,
                     FxDB(dr("kwtotalarvalas"), 0), sptField,
                     FxDB(dr("kwjmltagih"), 0), sptField,
                     FxDB(dr("kwjmltagihvalas"), 0), sptField,
                     FxDB(dr("kwbayar"), 0), sptField,
                     FxDB(dr("kwbayarvalas"), 0), sptField,
                     FxDB(dr("kwselisihkurs"), 0), sptField,
                     FxDB(dr("kwrekselisihkurs"), ""), sptField,
                     FxDB(dr("kwdiskontermin"), 0), sptField,
                     FxDB(dr("kwdiskonterminvalas"), 0), sptField,
                     FxDB(dr("kwrekdiskontermin"), ""), sptField,
                     FxDB(dr("kwstatuspb"), 0), sptField,
                     FxDB(dr("kwstatus"), 0), sptField,
                     FxDB(dr("kwstatussebelumnya"), 0), sptField,
                     FxDB(dr("kwjmlrevisi"), 0), sptField,
                     FxDB(dr("kwcetakanke"), 0), sptField,
                     FxDB(dr("kwinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kwinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kwmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kwmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kwposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kwpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kwisclose"), 0), sptField,
                     FxDB(dr("kwcabangnama"), ""), sptField,
                     FxDB(dr("kwlokasinama"), ""), sptField,
                     FxDB(dr("kwgudangnama"), ""), sptField,
                     FxDB(dr("kwcustomerkode"), ""), sptField,
                     FxDB(dr("kwcustomernama"), ""), sptField,
                     FxDB(dr("kwbagianpenjualankode"), ""), sptField,
                     FxDB(dr("kwbagianpenjualannama"), ""), sptField,
                     FxDB(dr("kwbagianpenagihankode"), ""), sptField,
                     FxDB(dr("kwbagianpenagihannama"), ""), sptField,
                     FxDB(dr("kwcarabayarnama"), ""), sptField,
                     FxDB(dr("kwrekselisihkursnama"), ""), sptField,
                     FxDB(dr("kwrekdiskonterminnama"), ""), sptField,
                     FxDB(dr("kwstatusnama"), ""), sptField,
                     FxDB(dr("kwstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("kwinputusernama"), ""), sptField,
                     FxDB(dr("kwmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kwpetugasnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kwid, kwcabang, kwlokasi, kwgudang, kwsumber, kwautonotransaksi, kwnotransaksi, kwtgl, kwkodepa, kwcustomer, kwcustomerkontak, kw1alamat1, kw1alamat2, kw1alamat3, kw2alamat1, kw2alamat2, kw2alamat3, kwbagianpenjualan, kwbagianpenagihan, kwuraian, kwcatatan, kwnoref, kwtglnoref, kwcarabayar, kwtglbayar, kwmatauang, kwkurs, kwtotalap, kwtotalapvalas, kwtotalar, kwtotalarvalas, kwjmltagih, kwjmltagihvalas, kwbayar, kwbayarvalas, kwselisihkurs, kwrekselisihkurs, kwdiskontermin, kwdiskonterminvalas, kwrekdiskontermin, kwstatuspb, kwstatus, kwstatussebelumnya, kwjmlrevisi, kwcetakanke, kwinputuser, kwinputtgl, kwmodifikasiuser, kwmodifikasitgl, kwposting, kwpostingtgl, kwisclose, kwcabangnama, kwlokasinama, kwgudangnama, kwcustomerkode, kwcustomernama, kwbagianpenjualankode, kwbagianpenjualannama, kwbagianpenagihankode, kwbagianpenagihannama, kwcarabayarnama, kwrekselisihkursnama, kwrekdiskonterminnama, kwstatusnama, kwstatussebelumnyanama, kwinputusernama, kwmodifikasiusernama, kwpetugasnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KwTakedataSearch(ByVal param As String) As String
        'M11_KwTakedataSearch --------------------------------------------------------
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
        Dim query As New m0_query
        sql = query.m11_kw_takedata(Filter)

        dt = AmbilData("aplikasi1-m11_kw_takedata", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("norm"), ""), sptField,
                     FxDB(dr("nama"), ""), sptField,
                     FxDB(dr("kategoripasien"), ""), sptField,
                     FxDB(dr("ditanggungoleh"), ""), sptField,
                     FxDB(dr("kamar"), ""), sptField,
                     FxDB(dr("alamat"), ""), sptField,
                     FxDB(dr("uraian"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)
            'result(2) = sql : GoTo selesai
            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            'result(2) = "Transaction data not found."
            result(2) = sql
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, sumber, notransaksi, tgl, norm, nama, kategoripasien, ditanggungoleh, kamar, alamat, uraian, kontak, catatan, totaltransaksi, rekhutangpiutang, inputtgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KwTakedata1Search(ByVal param As String) As String
        'M11_KwTakedataSearch --------------------------------------------------------
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
        Dim query As New m0_query
        sql = query.m11_kw_takedata1(Filter)

        dt = AmbilData("aplikasi1-m11_kw_takedata1", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("norm"), ""), sptField,
                     FxDB(dr("nama"), ""), sptField,
                     FxDB(dr("kategoripasien"), ""), sptField,
                     FxDB(dr("ditanggungoleh"), ""), sptField,
                     FxDB(dr("kamar"), ""), sptField,
                     FxDB(dr("alamat"), ""), sptField,
                     FxDB(dr("uraian"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)
            'result(2) = sql : GoTo selesai
            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            'result(2) = "Transaction data not found."
            result(2) = sql
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, sumber, notransaksi, tgl, norm, nama, kategoripasien, ditanggungoleh, kamar, alamat, uraian, kontak, catatan, totaltransaksi, rekhutangpiutang, inputtgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KwTerkait(ByVal param As String) As String
        'M11_KwTerkait --------------------------------------------------------
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
            result(2) = "kwid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND kwid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "kwid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m11_kw_terkait(Filter)
        'result(2) = sql : GoTo selesai

        sql = query.PanggilQuery("m11_kw_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_kw_terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each ic As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(ic("kwid"), 0), sptField,
                     FxDB(ic("kwnotransaksi"), ""), sptField,
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
            result(2) = "Related KW data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kwid, kwnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

End Class