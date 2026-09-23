Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_at
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_AtSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPay(), dataRowPay() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean, tglLunas As String = ""

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'atid(0) As , atcabang(1) As String, atlokasi(2) As String, atgudang(3) As String, atsumber(4) As String, 
        'atautonotransaksi(5) As Integer, atnotransaksi(6) As String, attgl(7) As Date, atkodepa(8) As , atsupplier(9) As , 
        'atsupplierkontak(10) As String, at1alamat1(11) As String, at1alamat2(12) As String, at1alamat3(13) As String, at2alamat1(14) As String, 
        'at2alamat2(15) As String, at2alamat3(16) As String, atbagianpembayaran(17) As , aturaian(18) As String, atcatatan(19) As String, 
        'atnoref(20) As String, attglnoref(21) As Date, atcarabayar(22) As Integer, attglbayar(23) As Date, atmatauang(24) As String, 
        'atkurs(25) As Double, attotalap(26) As Double, attotalapvalas(27) As Double, atbayar(28) As Double, atbayarvalas(29) As Double, 
        'atdiskontermin(30) As Double, atdiskonterminvalas(31) As Double, atrekdiskontermin(32) As String, atstatus(33) As Integer, atstatussebelumnya(34) As Integer, 
        'atjmlrevisi(35) As Integer, atcetakanke(36) As Integer, atinputuser(37) As , atinputtgl(38) As DateTime, atmodifikasiuser(39) As , 
        'atmodifikasitgl(40) As DateTime, atposting(41) As Integer, atpostingtgl(42) As DateTime, atisclose(43) As Integer, atcustomtext1(44) As String, 
        'atcustomtext2(45) As String, atcustomtext3(46) As String, atcustomtext4(47) As String, atcustomtext5(48) As String, atcustomint1(49) As Integer, 
        'atcustomint2(50) As Integer, atcustomint3(51) As Integer, atcustomdbl1(52) As Double, atcustomdbl2(53) As Double, atcustomdbl3(54) As Double, 
        'atcustomdate1(55) As Date, atcustomdate2(56) As Date, atcustomdate3(57) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'atid, atcabang, atlokasi, atgudang, atsumber, atautonotransaksi, atnotransaksi, 
        'attgl, atkodepa, atsupplier, atsupplierkontak, at1alamat1, at1alamat2, at1alamat3, 
        'at2alamat1, at2alamat2, at2alamat3, atbagianpembayaran, aturaian, atcatatan, atnoref, 
        'attglnoref, atcarabayar, attglbayar, atmatauang, atkurs, attotalap, attotalapvalas, 
        'atbayar, atbayarvalas, atdiskontermin, atdiskonterminvalas, atrekdiskontermin, atstatus, atstatussebelumnya, 
        'atjmlrevisi, atcetakanke, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atposting, 
        'atpostingtgl, atisclose, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, 
        'atcustomint1, atcustomint2, atcustomint3, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdate1, 
        'atcustomdate2, atcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 58) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'atautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "atautonotransaksi required numeric." : GoTo selesai
        End If
        'attgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "attgl required date." : GoTo selesai
        End If
        'attglnoref(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "attglnoref required date." : GoTo selesai
        End If
        'atcarabayar(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "atcarabayar required numeric." : GoTo selesai
        End If
        'attglbayar(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "attglbayar required date." : GoTo selesai
        End If
        'atkurs(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "atkurs required numeric." : GoTo selesai
        End If
        'attotalap(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "attotalap required numeric." : GoTo selesai
        End If
        'attotalapvalas(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "attotalapvalas required numeric." : GoTo selesai
        End If
        'atbayar(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "atbayar required numeric." : GoTo selesai
        End If
        'atbayarvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "atbayarvalas required numeric." : GoTo selesai
        End If
        'atdiskontermin(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "atdiskontermin required numeric." : GoTo selesai
        End If
        'atdiskonterminvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "atdiskonterminvalas required numeric." : GoTo selesai
        End If
        'atstatus(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "atstatus required numeric." : GoTo selesai
        End If
        'atstatussebelumnya(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "atstatussebelumnya required numeric." : GoTo selesai
        End If
        'atjmlrevisi(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "atjmlrevisi required numeric." : GoTo selesai
        End If
        'atcetakanke(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "atcetakanke required numeric." : GoTo selesai
        End If
        'atinputtgl(38) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "atinputtgl required date." : GoTo selesai
        End If
        'atmodifikasitgl(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "atmodifikasitgl required date." : GoTo selesai
        End If
        'atposting(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "atposting required numeric." : GoTo selesai
        End If
        'atpostingtgl(42) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "atpostingtgl required date." : GoTo selesai
        End If
        'atisclose(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "atisclose required numeric." : GoTo selesai
        End If
        'atcustomint1(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "atcustomint1 required numeric." : GoTo selesai
        End If
        'atcustomint2(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "atcustomint2 required numeric." : GoTo selesai
        End If
        'atcustomint3(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "atcustomint3 required numeric." : GoTo selesai
        End If
        'atcustomdbl1(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "atcustomdbl1 required numeric." : GoTo selesai
        End If
        'atcustomdbl2(53) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "atcustomdbl2 required numeric." : GoTo selesai
        End If
        'atcustomdbl3(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "atcustomdbl3 required numeric." : GoTo selesai
        End If
        'atcustomdate1(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "atcustomdate1 required date." : GoTo selesai
        End If
        'atcustomdate2(56) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "atcustomdate2 required date." : GoTo selesai
        End If
        'atcustomdate3(57) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "atcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'atid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "atid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "atid should not be more than 20 character." : GoTo selesai
        End If

        'atcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "atcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "atcabang should not be more than 25 character." : GoTo selesai
        End If

        'atlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "atlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "atlokasi should not be more than 25 character." : GoTo selesai
        End If

        'atsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "atsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "atsumber should not be more than 10 character." : GoTo selesai
        End If

        'atnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "atnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "atnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'attgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "attgl can't be empty" : GoTo selesai
        End If

        'atkodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "atkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "atkodepa should not be more than 20 character." : GoTo selesai
        End If

        'atsupplier(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "atsupplier can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "atsupplier should not be more than 20 character." : GoTo selesai
        End If

        'atbagianpembayaran(17) As 
        If Len(dataUtama(17)) = 0 Then
            result(2) = "atbagianpembayaran can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(17)) > 20 Then
            result(2) = "atbagianpembayaran should not be more than 20 character." : GoTo selesai
        End If

        'attglnoref(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "attglnoref can't be empty" : GoTo selesai
        End If

        'attglbayar(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "attglbayar can't be empty" : GoTo selesai
        End If

        'atmatauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "atmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "atmatauang should not be more than 25 character." : GoTo selesai
        End If

        'atkurs(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "atkurs can't be empty" : GoTo selesai
        End If

        'attotalap(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "attotalap can't be empty" : GoTo selesai
        End If

        'attotalapvalas(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "attotalapvalas can't be empty" : GoTo selesai
        End If

        'atbayar(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "atbayar can't be empty" : GoTo selesai
        End If

        'atbayarvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "atbayarvalas can't be empty" : GoTo selesai
        End If

        'atdiskontermin(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "atdiskontermin can't be empty" : GoTo selesai
        End If

        'atdiskonterminvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "atdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'atinputuser(37) As 
        If Len(dataUtama(37)) = 0 Then
            result(2) = "atinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 20 Then
            result(2) = "atinputuser should not be more than 20 character." : GoTo selesai
        End If

        'atinputtgl(38) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "atinputtgl can't be empty" : GoTo selesai
        End If

        'atmodifikasiuser(39) As 
        If Len(dataUtama(39)) = 0 Then
            result(2) = "atmodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 20 Then
            result(2) = "atmodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'atmodifikasitgl(40) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "atmodifikasitgl can't be empty" : GoTo selesai
        End If

        'atpostingtgl(42) As DateTime
        If Len(dataUtama(42)) = 0 Then
            result(2) = "atpostingtgl can't be empty" : GoTo selesai
        End If

        'atcustomdbl1(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "atcustomdbl1 can't be empty" : GoTo selesai
        End If

        'atcustomdbl2(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "atcustomdbl2 can't be empty" : GoTo selesai
        End If

        'atcustomdbl3(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "atcustomdbl3 can't be empty" : GoTo selesai
        End If

        'atcustomdate1(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "atcustomdate1 can't be empty" : GoTo selesai
        End If

        'atcustomdate2(56) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "atcustomdate2 can't be empty" : GoTo selesai
        End If

        'atcustomdate3(57) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "atcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "atid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atautonotransaksi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "attgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atkodepa", )
        AsDataTableTambahField(dtutama, "atsupplier", )
        AsDataTableTambahField(dtutama, "atsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "at1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "at1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "at1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "at2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "at2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "at2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atbagianpembayaran", )
        AsDataTableTambahField(dtutama, "aturaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "attglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcarabayar", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "attglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "attotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "attotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atrekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atstatus", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atstatussebelumnya", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atjmlrevisi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atcetakanke", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atinputuser", )
        AsDataTableTambahField(dtutama, "atinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atmodifikasiuser", )
        AsDataTableTambahField(dtutama, "atmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atposting", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atisclose", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atcustomint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atcustomint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "atcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "atcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "atid~atcabang~atlokasi~atgudang~atsumber~atautonotransaksi~atnotransaksi~attgl~atkodepa~atsupplier~atsupplierkontak~at1alamat1~at1alamat2~at1alamat3~at2alamat1~at2alamat2~at2alamat3~atbagianpembayaran~aturaian~atcatatan~atnoref~attglnoref~atcarabayar~attglbayar~atmatauang~atkurs~attotalap~attotalapvalas~atbayar~atbayarvalas~atdiskontermin~atdiskonterminvalas~atrekdiskontermin~atstatus~atstatussebelumnya~atjmlrevisi~atcetakanke~atinputuser~atinputtgl~atmodifikasiuser~atmodifikasitgl~atposting~atpostingtgl~atisclose~atcustomtext1~atcustomtext2~atcustomtext3~atcustomtext4~atcustomtext5~atcustomint1~atcustomint2~atcustomint3~atcustomdbl1~atcustomdbl2~atcustomdbl3~atcustomdate1~atcustomdate2~atcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idatdetail(0) As , idat(1) As , sumber(2) As String, idtransaksi(3) As , matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, rencana(8) As Double, sisa(9) As Double, 
        'jmlbayar(10) As Double, jmlbayarvalas(11) As Double, diskontermin(12) As String, jmldiskontermin(13) As Double, jmldiskonterminvalas(14) As Double, 
        'rekhutangpiutang(15) As String, catatan(16) As String, costcenter(17) As String, divisi(18) As String, subdivisi(19) As String, 
        'proyek(20) As String, urutan(21) As Integer, isclose(22) As Integer, customtext1(23) As String, customtext2(24) As String, 
        'customtext3(25) As String, customdbl1(26) As Double, customdbl2(27) As Double, customdbl3(28) As Double, customdate1(29) As Date, 
        'customdate2(30) As Date, customdate3(31) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idatdetail, idat, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, 
        'jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idatdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idat", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt16)
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
        Dim idtransaksiDetail As Integer = 0, jmlbayar As Double = 0, jmlbayarvalas As Double = 0
        Dim Outstanding As Double = 0, OutstandingValas As Double = 0

        'VARIABEL CEK TRANSAKSI PEMBAYARAN --> AE
        'RI
        Dim ftExistOutstandingAE As String = "", ftOutstandingAE As String = "", updNilaiAE As String = "", updFilterAE As String = "", updTglLunasAE As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 32) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'totaltransaksi(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "totaltransaksi required numeric." : GoTo selesai
            End If
            'terbayar(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "terbayar required numeric." : GoTo selesai
            End If
            'rencana(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "rencana required numeric." : GoTo selesai
            End If
            'sisa(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "sisa required numeric." : GoTo selesai
            End If
            'jmlbayar(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "jmlbayar required numeric." : GoTo selesai
            End If
            'jmlbayarvalas(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "jmlbayarvalas required numeric." : GoTo selesai
            End If
            'jmldiskontermin(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "jmldiskontermin required numeric." : GoTo selesai
            End If
            'jmldiskonterminvalas(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "jmldiskonterminvalas required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(26) As Double
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(27) As Double
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(29) As Date
            If (IsDate(dataRowDetail(29)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idatdetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idatdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idatdetail should not be more than 20 character." : GoTo selesai
            End If

            'idat(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idat can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idat should not be more than 20 character." : GoTo selesai
            End If

            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If

            'idtransaksi(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idtransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idtransaksi should not be more than 20 character." : GoTo selesai
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

            'rencana(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - rencana can't be empty" : GoTo selesai
            End If

            'sisa(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - sisa can't be empty" : GoTo selesai
            End If

            'jmlbayar(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayar can't be empty" : GoTo selesai
            End If

            'jmlbayarvalas(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayarvalas can't be empty" : GoTo selesai
            End If

            'diskontermin(12) As String
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - diskontermin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(12)) > 25 Then
                result(2) = "Row : " & i & " - diskontermin should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskontermin(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskontermin can't be empty" : GoTo selesai
            End If

            'jmldiskonterminvalas(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idatdetail~idat~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~rencana~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'sumber(2) As String            , idtransaksi(3) As Integer            , jmlbayar(10) As Double
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3) : jmlbayar = dataRowDetail(10)
            'jmlbayarvalas(10) As Double      , rekhutangpiutang(14) As String, 
            jmlbayarvalas = dataRowDetail(11) : norek = dataRowDetail(15)
            'matauang(4) As String
            matauangDetail = dataRowDetail(4)

            'VALIDASI TRANSAKSI PEMBAYARAN ----------------
            Select Case sumberDetail
                Case "AE"
                    '1. CEK DATA EXIST
                    ftExistOutstandingAE = IIf(Len(ftExistOutstandingAE.ToString) = 0, "", ftExistOutstandingAE & " UNION ")
                    ftExistOutstandingAE = String.Concat(ftExistOutstandingAE, "SELECT EXISTS(SELECT 1 FROM m7_ae WHERE aeid = '" & idtransaksiDetail & "' AND (aestatus = 2 OR aestatus = 3 OR aestatus = 4 OR aestatus = 7) LIMIT 1) as rowExists, aeid, aesumber, aenotransaksi FROM m7_ae WHERE aeid = '" & idtransaksiDetail & "'")

                    '2. CEK JML OUTSTANDING
                    If matauangDetail = MUFungsional Then
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    Else
                        Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                    End If
                    ftOutstandingAE = IIf(Len(ftOutstandingAE.ToString) = 0, "", ftOutstandingAE & " OR ")
                    ftOutstandingAE = String.Concat(ftOutstandingAE, " (ae.aeid = '" & idtransaksiDetail & "' AND " & Outstanding & " > ae.aetotaltransaksi - ae.aejmlbayar) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiAE = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ae.aejmlbayar + '" & Outstanding & "', 5) ", updNilaiAE)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterAE = IIf(Len(updFilterAE.ToString) = 0, "", updFilterAE & " OR ")
                    updFilterAE = String.Concat(updFilterAE, "(ae.aeid = '" & idtransaksiDetail & "')")

                    '5. SET NILAI TGLLUNAS TRANSAKSI
                    updTglLunasAE = String.Concat(" WHEN '" & idtransaksiDetail & "' THEN (CASE WHEN ae.aejmlbayar >= ae.aetotaltransaksi THEN '" & FixQuotes(tglLunas) & "' ELSE ae.aetgllunas END) ", updTglLunasAE)
            End Select
            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------
        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idatcarabayar(0) As , idat(1) As , carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idatcarabayar, idat, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idatcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idat", AsEnumTypeData.AsInt64)
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
                'idatcarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Pay Row : " & i & " - idatcarabayar required numeric." : GoTo selesai
                End If
                'idat(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Pay Row : " & i & " - idat required numeric." : GoTo selesai
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

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Row : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Row : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekbank(11) As String
                    If Len(dataRowPay(11)) = 0 Then
                        result(2) = "Row : " & i & " - rekbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(11)) > 25 Then
                        result(2) = "Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idatcarabayar~idat~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15)) = False Then
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
                'CEK TOTAL UTAMA DAN BAYAR ==============================
                Dim jumlah As Double = AsDataTableDSum(dtpay, "jumlah")
                Dim jumlahvalas As Double = AsDataTableDSum(dtpay, "jumlahvalas")
                If Double.Parse(drutama("atbayar")) <> jumlah Then
                    Dim selisih(2) As String
                    selisih = F_Nominal(Double.Parse(drutama("atbayar")) - jumlah, False).Split(sptSubParam)
                    result(2) = "Total amount of pay is not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                    'ElseIf drutama("atpbayarvalas") <> jumlahvalas Then
                    '    result(2) = "Total amount of foreign pay is not balanced" : Trans.Rollback() : GoTo selesai
                End If
                'END OF CEK TOTAL UTAMA DAN BAYAR =======================


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("attgl")), AsFormatTanggal(drutama("attgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "atmatauang", "atrekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================

                'CEK MATAUANG COA =======================================
                'PAY
                rsCekCoa = ValidasiMatauangCOA(dtutama, "atmatauang", "", dtpay, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================

                If isUpdate Then
                    result(4) = drutama("atid")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(atid) FROM M7_At WHERE atid=" & result(4) & "' AND atstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M7_At set atcabang  = '" & FixQuotes(drutama("atcabang")) & "', atlokasi  = '" & FixQuotes(drutama("atlokasi")) & "', atgudang  = '" & FixQuotes(drutama("atgudang")) & "', atsumber  = '" & FixQuotes(drutama("atsumber")) & "', atautonotransaksi  = " & drutama("atautonotransaksi") & ", atnotransaksi  = '" & FixQuotes(drutama("atnotransaksi")) & "', attgl  = '" & FixQuotes(AsFormatTanggal(drutama("attgl"))) & "', atkodepa  = '" & FixQuotes(drutama("atkodepa")) & "', atsupplier  = '" & FixQuotes(drutama("atsupplier")) & "', atsupplierkontak  = '" & FixQuotes(drutama("atsupplierkontak")) & "', at1alamat1  = '" & FixQuotes(drutama("at1alamat1")) & "', at1alamat2  = '" & FixQuotes(drutama("at1alamat2")) & "', at1alamat3  = '" & FixQuotes(drutama("at1alamat3")) & "', at2alamat1  = '" & FixQuotes(drutama("at2alamat1")) & "', at2alamat2  = '" & FixQuotes(drutama("at2alamat2")) & "', at2alamat3  = '" & FixQuotes(drutama("at2alamat3")) & "', atbagianpembayaran  = '" & FixQuotes(drutama("atbagianpembayaran")) & "', aturaian  = '" & FixQuotes(drutama("aturaian")) & "', atcatatan  = '" & FixQuotes(drutama("atcatatan")) & "', atnoref  = '" & FixQuotes(drutama("atnoref")) & "', attglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("attglnoref"))) & "', atcarabayar  = " & drutama("atcarabayar") & ", attglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("attglbayar"))) & "', atmatauang  = '" & FixQuotes(drutama("atmatauang")) & "', atkurs  = '" & FixDouble(drutama("atkurs")) & "', attotalap  = '" & FixDouble(drutama("attotalap")) & "', attotalapvalas  = '" & FixDouble(drutama("attotalapvalas")) & "', atbayar  = '" & FixDouble(drutama("atbayar")) & "', atbayarvalas  = '" & FixDouble(drutama("atbayarvalas")) & "', atdiskontermin  = '" & FixDouble(drutama("atdiskontermin")) & "', atdiskonterminvalas  = '" & FixDouble(drutama("atdiskonterminvalas")) & "', atrekdiskontermin  = '" & FixQuotes(drutama("atrekdiskontermin")) & "', atstatus  = " & drutama("atstatus") & ", atstatussebelumnya  = " & drutama("atstatussebelumnya") & ", atjmlrevisi  = atjmlrevisi+1, atcetakanke  = " & drutama("atcetakanke") & ", atinputuser  = '" & FixQuotes(drutama("atinputuser")) & "', atinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', atmodifikasiuser  = '" & FixQuotes(drutama("atmodifikasiuser")) & "', atmodifikasitgl  = NOW(), atposting  = " & drutama("atposting") & ", atpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("atpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', atcustomtext1  = '" & FixQuotes(drutama("atcustomtext1")) & "', atcustomtext2  = '" & FixQuotes(drutama("atcustomtext2")) & "', atcustomtext3  = '" & FixQuotes(drutama("atcustomtext3")) & "', atcustomtext4  = '" & FixQuotes(drutama("atcustomtext4")) & "', atcustomtext5  = '" & FixQuotes(drutama("atcustomtext5")) & "', atcustomint1  = " & drutama("atcustomint1") & ", atcustomint2  = " & drutama("atcustomint2") & ", atcustomint3  = " & drutama("atcustomint3") & ", atcustomdbl1  = '" & FixDouble(drutama("atcustomdbl1")) & "', atcustomdbl2  = '" & FixDouble(drutama("atcustomdbl2")) & "', atcustomdbl3  = '" & FixDouble(drutama("atcustomdbl3")) & "', atcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("atcustomdate1"))) & "', atcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("atcustomdate2"))) & "', atcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("atcustomdate3"))) & "' where atid = " & drutama("atid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else
                    If drutama("atautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("atcabang"), drutama("atlokasi"), drutama("atsumber"), drutama("attgl"))
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
                        notransaksi = drutama("atnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(atid) FROM m7_at WHERE atnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M7_At (atcabang, atlokasi, atgudang, atsumber, atautonotransaksi, atnotransaksi, attgl, atkodepa, atsupplier, atsupplierkontak, at1alamat1, at1alamat2, at1alamat3, at2alamat1, at2alamat2, at2alamat3, atbagianpembayaran, aturaian, atcatatan, atnoref, attglnoref, atcarabayar, attglbayar, atmatauang, atkurs, attotalap, attotalapvalas, atbayar, atbayarvalas, atdiskontermin, atdiskonterminvalas, atrekdiskontermin, atstatus, atstatussebelumnya, atjmlrevisi, atcetakanke, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atposting, atpostingtgl, atisclose, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdate1, atcustomdate2, atcustomdate3) values('" & FixQuotes(drutama("atcabang")) & "', '" & FixQuotes(drutama("atlokasi")) & "', '" & FixQuotes(drutama("atgudang")) & "', '" & FixQuotes(drutama("atsumber")) & "', " & drutama("atautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("attgl"))) & "', '" & FixQuotes(drutama("atkodepa")) & "', '" & FixQuotes(drutama("atsupplier")) & "', '" & FixQuotes(drutama("atsupplierkontak")) & "', '" & FixQuotes(drutama("at1alamat1")) & "', '" & FixQuotes(drutama("at1alamat2")) & "', '" & FixQuotes(drutama("at1alamat3")) & "', '" & FixQuotes(drutama("at2alamat1")) & "', '" & FixQuotes(drutama("at2alamat2")) & "', '" & FixQuotes(drutama("at2alamat3")) & "', '" & FixQuotes(drutama("atbagianpembayaran")) & "', '" & FixQuotes(drutama("aturaian")) & "', '" & FixQuotes(drutama("atcatatan")) & "', '" & FixQuotes(drutama("atnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("attglnoref"))) & "', " & drutama("atcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("attglbayar"))) & "', '" & FixQuotes(drutama("atmatauang")) & "', '" & FixDouble(drutama("atkurs")) & "', '" & FixDouble(drutama("attotalap")) & "', '" & FixDouble(drutama("attotalapvalas")) & "', '" & FixDouble(drutama("atbayar")) & "', '" & FixDouble(drutama("atbayarvalas")) & "', '" & FixDouble(drutama("atdiskontermin")) & "', '" & FixDouble(drutama("atdiskonterminvalas")) & "', '" & FixQuotes(drutama("atrekdiskontermin")) & "', " & drutama("atstatus") & ", " & drutama("atstatussebelumnya") & ", " & drutama("atjmlrevisi") & ", " & drutama("atcetakanke") & ", '" & FixQuotes(drutama("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(drutama("atmodifikasiuser")) & "', '1971-01-01 00:00:00', " & drutama("atposting") & ", '1971-01-01 00:00:00', " & drutama("atisclose") & ", '" & FixQuotes(drutama("atcustomtext1")) & "', '" & FixQuotes(drutama("atcustomtext2")) & "', '" & FixQuotes(drutama("atcustomtext3")) & "', '" & FixQuotes(drutama("atcustomtext4")) & "', '" & FixQuotes(drutama("atcustomtext5")) & "', " & drutama("atcustomint1") & ", " & drutama("atcustomint2") & ", " & drutama("atcustomint3") & ", '" & FixDouble(drutama("atcustomdbl1")) & "', '" & FixDouble(drutama("atcustomdbl2")) & "', '" & FixDouble(drutama("atcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("atcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select atid from M7_at where atnotransaksi='" & notransaksi & "' AND atinputuser= '" & userid & "' order by atmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then

                    sql = "Delete from M7_At_Detail where idat = " & result(4)
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
                        strValue2.Append("('" & FixQuotes(dr1("idatdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M7_At_Detail(idatdetail, idat, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M7_at_Pay where idat = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idatcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("atstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(4, 15, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("atsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("atsupplier") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M7_at_Pay(idatcarabayar, idat, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("atstatus") = 2 And Len(strGiro.ToString) > 0 Then
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

                'If drutama("atstatus") = 2 Then
                '    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                '    'RI
                '    If Len(updNilaiAE) > 0 Then
                '        sql = "UPDATE m7_ae ae LEFT JOIN m2_transaction_journal t ON ae.aesumber = t.tsumber AND ae.aeid =  t.tidtransaksi AND ae.aenotransaksi = t.tnotransaksi SET ae.aejmlbayar = (CASE ae.aeid " & updNilaiAE & " ELSE ae.aejmlbayar END), ae.aetgllunas = (CASE ae.aeid " & updTglLunasAE & " ELSE ae.aetgllunas END), t.tstatuslunas = ae.aestatuslunas, t.ttgllunas = ae.aetgllunas WHERE " & updFilterAE
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()
                '    End If
                '    'UPDATE TRANSAKSI PEMBAYARAN ====================================================

                'End If

                Dim sumber As String = "AT", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'INSERT USER LOG ====================================================================
                Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'")
                'ambil moduleid dan menuid dari m0_nomor
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
        Con1.Close()
        Con1 = Nothing
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
    Public Function M7_AtTakedataSearch(ByVal param As String) As String
        'M4_atpTakedataSearch --------------------------------------------------------
        'idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, 
        'termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, 
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
        Dim query As New m0_query
        sql = m7_at_takedata(Filter)

        dt = AmbilData("aplikasi1-M5_Ic_Takedata", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
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
            result(2) = sql
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, sisa, sisavalas, statuslunas, rekhutangpiutang, diskon1, haridiskon1, diskon2, haridiskon2, inputtgl"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function m7_at_takedata(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter As String = ""

        'Replace Filter
        If (strFilter.Length > 0) Then
            filter = strFilter
            filter = filter.Replace("idtransaksi", "ae.aeid")
            filter = filter.Replace("sumber", "ae.aesumber")
            filter = filter.Replace("notransaksi", "ae.aenotransaksi")
            filter = filter.Replace("kontak", "ae.aesupplier")
            filter = filter.Replace("tgl", "ae.aetgl")
            filter = filter.Replace("matauang", "ae.aematauang")
            filter = filter.Replace("statuslunas", "ae.aestatuslunas")
            filter = filter.Replace("tanggaljatuhtempo", "ae.aetgljatuhtempo")


        End If

        filter = " WHERE ae.aestatus IN(2,3,4,7) AND " & filter & ""
        '
        'AE
        sql = "select `ae`.`aeid` AS `idtransaksi`,`ae`.`aesumber` AS `sumber`,`ae`.`aenotransaksi` AS `notransaksi`,`ae`.`aetgl` AS `tgl`,`ae`.`aesupplier` AS `kontak`,`ae`.`aecatatan` AS `catatan`,`ae`.`aecarabayar` AS `carabayar`,`ae`.`aetermin` AS `termin`,`ae`.`aetgljatuhtempo` AS `tgljatuhtempo`,`ae`.`aematauang` AS `matauang`,`ae`.`aekurs` AS `kurs`,`ae`.`aetotaltransaksi` AS `totaltransaksi`,`ae`.`aejmlbayar` AS `terbayar`,((`ae`.`aetotaltransaksi` - `ae`.`aejmlbayar`) * `ae`.`aekurs`) AS `sisa`,(case `ae`.`aematauang` when `s2`.`snilai` then 0 else (`ae`.`aetotaltransaksi` - `ae`.`aejmlbayar`) end) AS `sisavalas`,`ae`.`aestatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`ae`.`aeinputtgl` AS `inputtgl` from ((((`m7_ae` `ae` left join `m1_terms` `tr` on((`ae`.`aetermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'HutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m7_at_detail` `atd` on(((`atd`.`sumber` = 'AE') and (`atd`.`idtransaksi` = `ae`.`aeid`) and (`atd`.`sisa` <> 0)))) " & filter & " group by `ae`.`aeid`"
        Return sql
    End Function

    <WebMethod()>
    Public Function M7_AtGetdataById(ByVal param As String) As String

        'M4_AtGetdataById Utama --------------------------------------------------------
        'atid, atcabang, atlokasi, atgudang, atsumber, atautonotransaksi, atnotransaksi, 
        'attgl, atkodepa, atsupplier, atsupplierkontak, at1alamat1, at1alamat2, at1alamat3, 
        'at2alamat1, at2alamat2, at2alamat3, atbagianpembayaran, aturaian, atcatatan, atnoref, 
        'attglnoref, atcarabayar, attglbayar, atmatauang, atkurs, attotalap, attotalapvalas, 
        'atbayar, atbayarvalas, atdiskontermin, 
        'atdiskonterminvalas, atrekdiskontermin, atstatus, atstatussebelumnya, atjmlrevisi, atcetakanke, 
        'atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atposting, atpostingtgl, atisclose, 
        'atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, 
        'atcustomint3, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdate1, atcustomdate2, atcustomdate3, 
        'atcabangnama, atlokasinama, atgudangnama, atsupplierkode, atsuppliernama, atbagianpembayarankode, atbagianpembayarannama, 
        'atcarabayarnama, atrekdiskonterminnama, atstatusnama, atstatussebelumnyanama, atinputusernama, 
        'atmodifikasiusernama

        'M4_AtGetdataById Detail -------------------------------------------------------
        'idatdetail, idat, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, 
        'sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, inputtgl

        'M4_atGetdataById Pay ----------------------------------------------------------
        'idatcarabayar, idat, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M4_at~M4_at_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "atid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "atid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m7_at_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("atid"), 0), sptField,
                     FxDB(drutama("atcabang"), ""), sptField,
                     FxDB(drutama("atlokasi"), ""), sptField,
                     FxDB(drutama("atgudang"), ""), sptField,
                     FxDB(drutama("atsumber"), ""), sptField,
                     FxDB(drutama("atautonotransaksi"), 0), sptField,
                     FxDB(drutama("atnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("attgl"), ""), formatTgl), sptField,
                     FxDB(drutama("atkodepa"), 0), sptField,
                     FxDB(drutama("atsupplier"), 0), sptField,
                     FxDB(drutama("atsupplierkontak"), ""), sptField,
                     FxDB(drutama("at1alamat1"), ""), sptField,
                     FxDB(drutama("at1alamat2"), ""), sptField,
                     FxDB(drutama("at1alamat3"), ""), sptField,
                     FxDB(drutama("at2alamat1"), ""), sptField,
                     FxDB(drutama("at2alamat2"), ""), sptField,
                     FxDB(drutama("at2alamat3"), ""), sptField,
                     FxDB(drutama("atbagianpembayaran"), 0), sptField,
                     FxDB(drutama("aturaian"), ""), sptField,
                     FxDB(drutama("atcatatan"), ""), sptField,
                     FxDB(drutama("atnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("attglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("atcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("attglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("atmatauang"), ""), sptField,
                     FxDB(drutama("atkurs"), 0), sptField,
                     FxDB(drutama("attotalap"), 0), sptField,
                     FxDB(drutama("attotalapvalas"), 0), sptField,
                     FxDB(drutama("atbayar"), 0), sptField,
                     FxDB(drutama("atbayarvalas"), 0), sptField,
                     FxDB(drutama("atdiskontermin"), 0), sptField,
                     FxDB(drutama("atdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("atrekdiskontermin"), ""), sptField,
                     FxDB(drutama("atstatus"), 0), sptField,
                     FxDB(drutama("atstatussebelumnya"), 0), sptField,
                     FxDB(drutama("atjmlrevisi"), 0), sptField,
                     FxDB(drutama("atcetakanke"), 0), sptField,
                     FxDB(drutama("atinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("atinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("atmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("atmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("atposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("atpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("atisclose"), 0), sptField,
                     FxDB(drutama("atcustomtext1"), ""), sptField,
                     FxDB(drutama("atcustomtext2"), ""), sptField,
                     FxDB(drutama("atcustomtext3"), ""), sptField,
                     FxDB(drutama("atcustomtext4"), ""), sptField,
                     FxDB(drutama("atcustomtext5"), ""), sptField,
                     FxDB(drutama("atcustomint1"), 0), sptField,
                     FxDB(drutama("atcustomint2"), 0), sptField,
                     FxDB(drutama("atcustomint3"), 0), sptField,
                     FxDB(drutama("atcustomdbl1"), 0), sptField,
                     FxDB(drutama("atcustomdbl2"), 0), sptField,
                     FxDB(drutama("atcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("atcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("atcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("atcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("atcabangnama"), ""), sptField,
                     FxDB(drutama("atlokasinama"), ""), sptField,
                     FxDB(drutama("atgudangnama"), ""), sptField,
                     FxDB(drutama("atsupplierkode"), ""), sptField,
                     FxDB(drutama("atsuppliernama"), ""), sptField,
                     FxDB(drutama("atbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("atbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("atcarabayarnama"), ""), sptField,
                     FxDB(drutama("atrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("atstatusnama"), ""), sptField,
                     FxDB(drutama("atstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("atinputusernama"), ""), sptField,
                     FxDB(drutama("atmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idatdetail"), 0), sptField,
                     FxDB(dr("idat"), 0), sptField,
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
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'PANGGIL QUERY
            sql = query.PanggilQuery("m7_at_getdata_pay")

            'AMBIL DATA PAY
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M7_at_Pay", "idat=" & idtransaksi, "idat ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idatcarabayar"), 0), sptField,
                     FxDB(dr("idat"), 0), sptField,
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
            result(2) = "at transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("atid, atcabang, atlokasi, atgudang, atsumber, atautonotransaksi, atnotransaksi, attgl, atkodepa, atsupplier, atsupplierkontak, at1alamat1, at1alamat2, at1alamat3, at2alamat1, at2alamat2, at2alamat3, atbagianpembayaran, aturaian, atcatatan, atnoref, attglnoref, atcarabayar, attglbayar, atmatauang, atkurs, attotalap, attotalapvalas, atbayar, atbayarvalas, atdiskontermin, atdiskonterminvalas, atrekdiskontermin, atstatus, atstatussebelumnya, atjmlrevisi, atcetakanke, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atposting, atpostingtgl, atisclose, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdate1, atcustomdate2, atcustomdate3, atcabangnama, atlokasinama, atgudangnama, atsupplierkode, atsuppliernama, atbagianpembayarankode, atbagianpembayarannama, atcarabayarnama, atrekdiskonterminnama, atnotransaksiatp, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama" & sptSubParam & "idatdetail, idat, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, atpnotransaksi, inputtgl" & sptSubParam & "idatcarabayar, idat, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M7_AtSearch(ByVal param As String) As String
        'M7_AtSearch --------------------------------------------------------
        'atid, atcabang, atlokasi, atgudang, atsumber, atautonotransaksi, atnotransaksi, 
        'attgl, atkodepa, atsupplier, atsupplierkontak, at1alamat1, at1alamat2, at1alamat3, 
        'at2alamat1, at2alamat2, at2alamat3, atbagianpembayaran, aturaian, atcatatan, atnoref, 
        'attglnoref, atcarabayar, attglbayar, atmatauang, atkurs, attotalap, attotalapvalas, 
        'atbayar, atbayarvalas, atdiskontermin, atdiskonterminvalas, atrekdiskontermin, atstatus, atstatussebelumnya, 
        'atjmlrevisi, atcetakanke, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atposting, 
        'atpostingtgl, atisclose, atcabangnama, atlokasinama, atgudangnama, atsupplierkode, atsuppliernama, 
        'atbagianpembayarankode, atbagianpembayarannama, atcarabayarnama, atrekdiskonterminnama, atstatusnama, atstatussebelumnyanama, atinputusernama, 
        'atmodifikasiusernama

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
            Filter = Filter.Replace("atsupplierkode", "c1.kkode")
            Filter = Filter.Replace("atsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim query As New m0_query
        sql = query.PanggilQuery("m7_at_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Rq", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("atid"), ""), sptField,
                     FxDB(dr("atcabang"), ""), sptField,
                     FxDB(dr("atlokasi"), ""), sptField,
                     FxDB(dr("atgudang"), ""), sptField,
                     FxDB(dr("atsumber"), ""), sptField,
                     FxDB(dr("atautonotransaksi"), 0), sptField,
                     FxDB(dr("atnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attgl"), ""), formatTgl), sptField,
                     FxDB(dr("atkodepa"), ""), sptField,
                     FxDB(dr("atsupplier"), ""), sptField,
                     FxDB(dr("atsupplierkontak"), ""), sptField,
                     FxDB(dr("at1alamat1"), ""), sptField,
                     FxDB(dr("at1alamat2"), ""), sptField,
                     FxDB(dr("at1alamat3"), ""), sptField,
                     FxDB(dr("at2alamat1"), ""), sptField,
                     FxDB(dr("at2alamat2"), ""), sptField,
                     FxDB(dr("at2alamat3"), ""), sptField,
                     FxDB(dr("atbagianpembayaran"), ""), sptField,
                     FxDB(dr("aturaian"), ""), sptField,
                     FxDB(dr("atcatatan"), ""), sptField,
                     FxDB(dr("atnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("atcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("attglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("atmatauang"), ""), sptField,
                     FxDB(dr("atkurs"), 0), sptField,
                     FxDB(dr("attotalap"), 0), sptField,
                     FxDB(dr("attotalapvalas"), 0), sptField,
                     FxDB(dr("atbayar"), 0), sptField,
                     FxDB(dr("atbayarvalas"), 0), sptField,
                     FxDB(dr("atdiskontermin"), 0), sptField,
                     FxDB(dr("atdiskonterminvalas"), 0), sptField,
                     FxDB(dr("atrekdiskontermin"), ""), sptField,
                     FxDB(dr("atstatus"), 0), sptField,
                     FxDB(dr("atstatussebelumnya"), 0), sptField,
                     FxDB(dr("atjmlrevisi"), 0), sptField,
                     FxDB(dr("atcetakanke"), 0), sptField,
                     FxDB(dr("atinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("atpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atisclose"), 0), sptField,
                     FxDB(dr("atcabangnama"), ""), sptField,
                     FxDB(dr("atlokasinama"), ""), sptField,
                     FxDB(dr("atgudangnama"), ""), sptField,
                     FxDB(dr("atsupplierkode"), ""), sptField,
                     FxDB(dr("atsuppliernama"), ""), sptField,
                     FxDB(dr("atbagianpembayarankode"), ""), sptField,
                     FxDB(dr("atbagianpembayarannama"), ""), sptField,
                     FxDB(dr("atcarabayarnama"), ""), sptField,
                     FxDB(dr("atrekdiskonterminnama"), ""), sptField,
                     FxDB(dr("atstatusnama"), ""), sptField,
                     FxDB(dr("atstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("atinputusernama"), ""), sptField,
                     FxDB(dr("atmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("atid, atcabang, atlokasi, atgudang, atsumber, atautonotransaksi, atnotransaksi, attgl, atkodepa, atsupplier, atsupplierkontak, at1alamat1, at1alamat2, at1alamat3, at2alamat1, at2alamat2, at2alamat3, atbagianpembayaran, aturaian, atcatatan, atnoref, attglnoref, atcarabayar, attglbayar, atmatauang, atkurs, attotalap, attotalapvalas, atbayar, atbayarvalas, atdiskontermin, atdiskonterminvalas, atrekdiskontermin, atstatus, atstatussebelumnya, atjmlrevisi, atcetakanke, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atposting, atpostingtgl, atisclose, atcabangnama, atlokasinama, atgudangnama, atsupplierkode, atsuppliernama, atbagianpembayarankode, atbagianpembayarannama, atcarabayarnama, atrekdiskonterminnama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M7_AtUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("atsupplierkode", "c1.kkode")
            Filter = Filter.Replace("atsuppliernama", "c1.knama")
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
            Dim sumber As String = "At", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Attgl, Atnotransaksi, Atstatus FROM M7_At WHERE Atid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Atstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m4_vp_history
            'Dim rsSimpanHistory As String = SimpanHistory.M4_Vp_HistorySimpan("" & paramSplit(0) & "★M4_Vp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m7_at_terkait")
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
                dtdetail = AsDataTableAmbilDariDB("SELECT sumber, idtransaksi, matauang, jmlbayar, jmlbayarvalas, rekhutangpiutang, urutan FROM M7_at_detail WHERE idat = '" & idtransaksi & "'")
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
                        jmlbayarvalas = dr1("jmlbayarvalas") : norek = dr1("rekhutangpiutang")
                        matauangDetail = dr1("matauang")


                        'VALIDASI TRANSAKSI PEMBAYARAN ----------------
                        Select Case sumberDetail
                            Case "AE"
                                '1. CEK JML OUTSTANDING
                                If matauangDetail = MUFungsional Then
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayar", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                Else
                                    Outstanding = AsDataTableDSum(dtdetail, "jmlbayarvalas", "sumber = '" & sumberDetail & "' AND idtransaksi = '" & idtransaksiDetail & "'")
                                End If

                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiRI = String.Concat("WHEN '" & idtransaksiDetail & "' THEN ROUND(ae.aejmlbayar - '" & Outstanding & "', 5) ", updNilaiRI)

                                '3. SET FILTER UPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(ae.aeid = '" & idtransaksiDetail & "')")


                        End Select
                        'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE TRANSAKSI PEMBAYARAN ========================================================
                'AE
                'If Len(updNilaiRI) > 0 Then
                '    sql = "UPDATE m7_ae ae LEFT JOIN m2_transaction_journal t ON ae.aesumber = t.tsumber AND ae.aeid = t.tidtransaksi AND ae.aenotransaksi = t.tnotransaksi SET ae.aejmlbayar = (CASE ae.aeid " & updNilaiRI & " ELSE ae.aejmlbayar END), ae.aetgllunas = '" & FixQuotes(tglLunas) & "', t.tstatuslunas = ae.aestatuslunas, t.ttgllunas = ae.aetgllunas WHERE " & updFilterRI
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = Con1
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If

                'UPDATE TRANSAKSI PEMBAYARAN ========================================================


                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'AT' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE JURNAL
                'sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'VP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = Con1
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M7_At SET Atstatus = " & nilaiStatus & ", Atmodifikasiuser='" & userid & "', Atmodifikasitgl = NOW(), Atposting = 0, Atpostingtgl = '1971-01-01 00:00:00', Atjmlrevisi = Atjmlrevisi + 1 WHERE Atid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AtSearch(PostWsSearch(paramSplit(0), "M7_AtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M7_AtDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("atsupplierkode", "c1.kkode")
            Filter = Filter.Replace("atsuppliernama", "c1.knama")
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
            Dim sumber As String = "At", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Atid, Atnotransaksi FROM M7_At WHERE Atid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT atcabang, atlokasi, atsumber, atautonotransaksi, atnotransaksi, attgl"
            sql &= " FROM M7_at"
            sql &= " WHERE atid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("atcabang")
                lokasi = dtNomorNext.Rows(0)("atlokasi")
                sumber = dtNomorNext.Rows(0)("atsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("atautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("atnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("attgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PAY
            sql = "DELETE FROM M7_At_Pay WHERE idat='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M7_At_Detail WHERE idat='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M7_At WHERE atid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M7_AtSearch(PostWsSearch(paramSplit(0), "M7_AtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
