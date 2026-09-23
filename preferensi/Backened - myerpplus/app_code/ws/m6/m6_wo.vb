Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_wo
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_WoSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataDetail2(), dataRowDetail2(), dataDetail3(), dataDetail4() As String

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
        If (dataSplit.Length <> 5) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'woid(0) As Integer, wocabang(1) As String, wolokasi(2) As String, wogudangasal(3) As String, wogudangproduksi(4) As String, 
        'wogudangtujuan(5) As String, wosumber(6) As String, wojenis(7) As String, woautonotransaksi(8) As Integer, wonotransaksi(9) As String, 
        'wotgl(10) As Date, wokodepa(11) As Integer, wodimintaoleh(12) As Integer, wodimintaolehkontak(13) As String, womintake(14) As Integer, 
        'wotgldipakai(15) As Date, woestimasikerja(16) As String, womatauang(17) As String, wokurs(18) As Double, wototalhargain(19) As Double, 
        'wototalhargaout(20) As Double, wototalhppin(21) As Double, wototalhppout(22) As Double, wouraian(23) As String, wocatatan(24) As String, 
        'wonoref(25) As String, wotglnoref(26) As Date, woidbom(27) As Integer, woidpdr(28) As Integer, wostatusmrsin(29) As Integer, 
        'wostatusmrsout(30) As Integer, wostatusmrnin(31) As Integer, wostatusmrnout(32) As Integer, wostatuspdin(33) As Integer, wostatuspdout(34) As Integer, 
        'wostatus(35) As Integer, wostatussebelumnya(36) As Integer, wojmlrevisi(37) As Integer, wocetakanke(38) As Integer, woinputuser(39) As Integer, 
        'woinputtgl(40) As DateTime, womodifikasiuser(41) As Integer, womodifikasitgl(42) As DateTime, woisclose(43) As Integer, wocustomtext1(44) As String, 
        'wocustomtext2(45) As String, wocustomtext3(46) As String, wocustomtext4(47) As String, wocustomtext5(48) As String, wocustomint1(49) As Integer, 
        'wocustomint2(50) As Integer, wocustomint3(51) As Integer, wocustomdbl1(52) As Double, wocustomdbl2(53) As Double, wocustomdbl3(54) As Double, 
        'wocustomdate1(55) As Date, wocustomdate2(56) As Date, wocustomdate3(57) As Date, woaktivitas(58) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, 
        'wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, 
        'womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, 
        'wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, 
        'woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, 
        'wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, 
        'womodifikasitgl, woisclose, wocustomtext1, wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, 
        'wocustomint1, wocustomint2, wocustomint3, wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, 
        'wocustomdate2, wocustomdate3, woaktivitas

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 58 And dataUtama.Length <> 59) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'woid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "woid required numeric." : GoTo selesai
        End If
        'woautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "woautonotransaksi required numeric." : GoTo selesai
        End If
        'wotgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "wotgl required date." : GoTo selesai
        End If
        'wokodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "wokodepa required numeric." : GoTo selesai
        End If
        'wodimintaoleh(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "wodimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "wodimintaoleh can't be empty." : GoTo selesai
        End If
        'womintake(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "womintake required numeric." : GoTo selesai
        End If
        'wotgldipakai(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "wotgldipakai required date." : GoTo selesai
        End If
        'wokurs(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "wokurs required numeric." : GoTo selesai
        End If
        'wototalhargain(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "wototalhargain required numeric." : GoTo selesai
        End If
        'wototalhargaout(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "wototalhargaout required numeric." : GoTo selesai
        End If
        'wototalhppin(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "wototalhppin required numeric." : GoTo selesai
        End If
        'wototalhppout(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "wototalhppout required numeric." : GoTo selesai
        End If
        'wotglnoref(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "wotglnoref required date." : GoTo selesai
        End If
        'woidbom(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "woidbom required numeric." : GoTo selesai
        End If
        'woidpdr(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "woidpdr required numeric." : GoTo selesai
        End If
        'wostatusmrsin(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "wostatusmrsin required numeric." : GoTo selesai
        End If
        'wostatusmrsout(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "wostatusmrsout required numeric." : GoTo selesai
        End If
        'wostatusmrnin(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "wostatusmrnin required numeric." : GoTo selesai
        End If
        'wostatusmrnout(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "wostatusmrnout required numeric." : GoTo selesai
        End If
        'wostatuspdin(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "wostatuspdin required numeric." : GoTo selesai
        End If
        'wostatuspdout(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "wostatuspdout required numeric." : GoTo selesai
        End If
        'wostatus(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "wostatus required numeric." : GoTo selesai
        End If
        'wostatussebelumnya(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "wostatussebelumnya required numeric." : GoTo selesai
        End If
        'wojmlrevisi(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "wojmlrevisi required numeric." : GoTo selesai
        End If
        'wocetakanke(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "wocetakanke required numeric." : GoTo selesai
        End If
        'woinputuser(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "woinputuser required numeric." : GoTo selesai
        End If
        'woinputtgl(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "woinputtgl required date." : GoTo selesai
        End If
        'womodifikasiuser(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "womodifikasiuser required numeric." : GoTo selesai
        End If
        'womodifikasitgl(42) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "womodifikasitgl required date." : GoTo selesai
        End If
        'woisclose(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "woisclose required numeric." : GoTo selesai
        End If
        'wocustomint1(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "wocustomint1 required numeric." : GoTo selesai
        End If
        'wocustomint2(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "wocustomint2 required numeric." : GoTo selesai
        End If
        'wocustomint3(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "wocustomint3 required numeric." : GoTo selesai
        End If
        'wocustomdbl1(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "wocustomdbl1 required numeric." : GoTo selesai
        End If
        'wocustomdbl2(53) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "wocustomdbl2 required numeric." : GoTo selesai
        End If
        'wocustomdbl3(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "wocustomdbl3 required numeric." : GoTo selesai
        End If
        'wocustomdate1(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "wocustomdate1 required date." : GoTo selesai
        End If
        'wocustomdate2(56) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "wocustomdate2 required date." : GoTo selesai
        End If
        'wocustomdate3(57) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "wocustomdate3 required date." : GoTo selesai
        End If
        If dataUtama.Length > 58 Then
            'woaktivitas(58) As Integer
            If (IsNumeric(dataUtama(58)) = False) Then
                result(2) = "woaktivitas required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'wocabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "wocabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "wocabang should not be more than 25 character." : GoTo selesai
        End If

        'wolokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "wolokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "wolokasi should not be more than 25 character." : GoTo selesai
        End If

        'wogudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "wogudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "wogudangasal should not be more than 25 character." : GoTo selesai
        End If

        'wogudangproduksi(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "wogudangproduksi can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "wogudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'wogudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "wogudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "wogudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'wosumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "wosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "wosumber should not be more than 10 character." : GoTo selesai
        End If

        'wojenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "wojenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "wojenis should not be more than 25 character." : GoTo selesai
        End If

        'wonotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "wonotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "wonotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'wotgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "wotgl can't be empty" : GoTo selesai
        End If

        'wotgldipakai(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "wotgldipakai can't be empty" : GoTo selesai
        End If

        'womatauang(17) As String
        If Len(dataUtama(17)) = 0 Then
            result(2) = "womatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(17)) > 25 Then
            result(2) = "womatauang should not be more than 25 character." : GoTo selesai
        End If

        'wokurs(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "wokurs can't be empty" : GoTo selesai
        End If

        'wototalhargain(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "wototalhargain can't be empty" : GoTo selesai
        End If

        'wototalhargaout(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "wototalhargaout can't be empty" : GoTo selesai
        End If

        'wototalhppin(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "wototalhppin can't be empty" : GoTo selesai
        End If

        'wototalhppout(22) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = "wototalhppout can't be empty" : GoTo selesai
        End If

        'wotglnoref(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "wotglnoref can't be empty" : GoTo selesai
        End If

        'woinputtgl(40) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "woinputtgl can't be empty" : GoTo selesai
        End If

        'womodifikasitgl(42) As DateTime
        If Len(dataUtama(42)) = 0 Then
            result(2) = "womodifikasitgl can't be empty" : GoTo selesai
        End If

        'wocustomdbl1(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "wocustomdbl1 can't be empty" : GoTo selesai
        End If

        'wocustomdbl2(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "wocustomdbl2 can't be empty" : GoTo selesai
        End If

        'wocustomdbl3(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "wocustomdbl3 can't be empty" : GoTo selesai
        End If

        'wocustomdate1(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "wocustomdate1 can't be empty" : GoTo selesai
        End If

        'wocustomdate2(56) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "wocustomdate2 can't be empty" : GoTo selesai
        End If

        'wocustomdate3(57) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "wocustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "woid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wolokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wogudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wogudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wogudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wojenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wodimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wodimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "womintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wotgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "womatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wouraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woidbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "woidpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrsin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrsout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrnin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrnout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatuspdin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatuspdout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "woinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "woinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "womodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "womodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woaktivitas", AsEnumTypeData.AsInt64)
        If dataUtama.Length > 58 Then
            If AsDataTableTambahData(dtutama, "woid~wocabang~wolokasi~wogudangasal~wogudangproduksi~wogudangtujuan~wosumber~wojenis~woautonotransaksi~wonotransaksi~wotgl~wokodepa~wodimintaoleh~wodimintaolehkontak~womintake~wotgldipakai~woestimasikerja~womatauang~wokurs~wototalhargain~wototalhargaout~wototalhppin~wototalhppout~wouraian~wocatatan~wonoref~wotglnoref~woidbom~woidpdr~wostatusmrsin~wostatusmrsout~wostatusmrnin~wostatusmrnout~wostatuspdin~wostatuspdout~wostatus~wostatussebelumnya~wojmlrevisi~wocetakanke~woinputuser~woinputtgl~womodifikasiuser~womodifikasitgl~woisclose~wocustomtext1~wocustomtext2~wocustomtext3~wocustomtext4~wocustomtext5~wocustomint1~wocustomint2~wocustomint3~wocustomdbl1~wocustomdbl2~wocustomdbl3~wocustomdate1~wocustomdate2~wocustomdate3~woaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "woid~wocabang~wolokasi~wogudangasal~wogudangproduksi~wogudangtujuan~wosumber~wojenis~woautonotransaksi~wonotransaksi~wotgl~wokodepa~wodimintaoleh~wodimintaolehkontak~womintake~wotgldipakai~woestimasikerja~womatauang~wokurs~wototalhargain~wototalhargaout~wototalhppin~wototalhppout~wouraian~wocatatan~wonoref~wotglnoref~woidbom~woidpdr~wostatusmrsin~wostatusmrsout~wostatusmrnin~wostatusmrnout~wostatuspdin~wostatuspdout~wostatus~wostatussebelumnya~wojmlrevisi~wocetakanke~woinputuser~woinputtgl~womodifikasiuser~womodifikasitgl~woisclose~wocustomtext1~wocustomtext2~wocustomtext3~wocustomtext4~wocustomtext5~wocustomint1~wocustomint2~wocustomint3~wocustomdbl1~wocustomdbl2~wocustomdbl3~wocustomdate1~wocustomdate2~wocustomdate3~woaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If


        'MAPPING BUAT WS DATA DETAIL1 -------------------------------------------------------
        'idwoin(0) As Integer, idwo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, idbomin(27) As Integer, idpdrin(28) As Integer, jmlmrs(29) As Double, 
        'statusmrs(30) As Integer, jmlmrn(31) As Double, statusmrn(32) As Integer, jmlpd(33) As Double, statuspd(34) As Integer, 
        'isclose(35) As Integer, customtext1(36) As String, customtext2(37) As String, customtext3(38) As String, customdbl1(39) As Double, 
        'customdbl2(40) As Double, customdbl3(41) As Double, customdate1(42) As Date, customdate2(43) As Date, customdate3(44) As Date

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, 
        'idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable DETAIL1
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idwoin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idwo", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "hpppersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpdrin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrn", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, idbomin As Integer = 0, idbomout As Integer = 0, idpdrin As Integer = 0, idpdrout As Integer = 0

        Dim ftExistOutstandingBomIn As String = "", ftOutstandingBomIn As String = ""
        Dim ftExistOutstandingBomOut As String = "", ftOutstandingBomOut As String = ""

        Dim ftExistOutstandingPdrIn As String = "", ftOutstandingPdrIn As String = ""
        Dim updNilaiPdrIn As String = "", updFilterPdrIn As String = ""

        Dim ftExistOutstandingPdrOut As String = "", ftOutstandingPdrOut As String = ""
        Dim updNilaiPdrOut As String = "", updFilterPdrOut As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL1 ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL1 -----------------------------------
            'CEK ARRAY DATA DETAIL1
            If (dataRowDetail.Length <> 45) Then
                result(2) = "Detail 1 Row : " & i & " - Invalid detail hasil transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL1 ----------------------------

            'VALIDASI TIPE DATA DETAIL1 ------------------------------------------
            'idwoin(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idwoin required numeric." : GoTo selesai
            End If
            'idwo(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idwo required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpppersen(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idbomin(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbomin required numeric." : GoTo selesai
            End If
            'idpdrin(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idpdrin required numeric." : GoTo selesai
            End If
            'jmlmrs(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
            End If
            'statusmrs(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrs required numeric." : GoTo selesai
            End If
            'jmlmrn(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
            End If
            'statusmrn(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrn required numeric." : GoTo selesai
            End If
            'jmlpd(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(42) As Date
            If (IsDate(dataRowDetail(42)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(43) As Date
            If (IsDate(dataRowDetail(43)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(44) As Date
            If (IsDate(dataRowDetail(44)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL1 -----------------------------------

            'VALIDASI DATA DETAIL1 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Detail 1 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpppersen(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(15) As String
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(18) As String
            'If Len(dataRowDetail(18)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(18)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(19) As String
            'If Len(dataRowDetail(19)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(19)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(20) As String
            'If Len(dataRowDetail(20)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmlmrs(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
            End If

            'jmlmrn(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
            End If

            'jmlpd(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(42) As Date
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(43) As Date
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(44) As Date
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL1 --------------------------------

            If AsDataTableTambahData(dtdetail, "idwoin~idwo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomin~idpdrin~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44)) = False Then
                result(2) = "Detail 1 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , idbomin(27) As Integer      , idpdrin(28) As Integer
            idbarang = dataRowDetail(2) : idbomin = dataRowDetail(27) : idpdrin = dataRowDetail(28)

            'VALIDASI OUTSTANDING -------------------------
            ''BOM
            'If idbomin <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingBomIn = IIf(Len(ftExistOutstandingBomIn.ToString) = 0, "", ftExistOutstandingBomIn & " UNION ")
            '    ftExistOutstandingBomIn = String.Concat(ftExistOutstandingBomIn, "SELECT EXISTS(SELECT 1 FROM m6_bom_in JOIN m6_bom ON idbom = bomid WHERE idbomin = '" & idbomin & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomin & "' as idbomin, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
            '    '2. CEK JML OUTSTANDING
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbomin=" & idbomin)
            '    ftOutstandingBomIn = IIf(Len(ftOutstandingBomIn.ToString) = 0, "", ftOutstandingBomIn & " OR ")
            '    ftOutstandingBomIn = String.Concat(ftOutstandingBomIn, " (bomin.idbomin = " & idbomin & " AND " & Outstanding & " > bomin.jmlbarang) ")
            'End If

            'PDR
            If idpdrin <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstandingPdrIn = IIf(Len(ftExistOutstandingPdrIn.ToString) = 0, "", ftExistOutstandingPdrIn & " UNION ")
                ftExistOutstandingPdrIn = String.Concat(ftExistOutstandingPdrIn, "SELECT EXISTS(SELECT 1 FROM m6_pdr_in JOIN m6_pdr ON idpdr = pdrid WHERE idpdrin = '" & idpdrin & "' AND (pdrstatus = 2 OR pdrstatus = 3 OR pdrstatus = 4 OR pdrstatus = 7) LIMIT 1) as rowExists, '" & idpdrin & "' as idpdrin, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpdrin=" & idpdrin)
                ftOutstandingPdrIn = IIf(Len(ftOutstandingPdrIn.ToString) = 0, "", ftOutstandingPdrIn & " OR ")
                ftOutstandingPdrIn = String.Concat(ftOutstandingPdrIn, " (pdrin.idpdrin = " & idpdrin & " AND " & Outstanding & " > (pdrin.jmlbarang - pdrin.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiPdrIn = String.Concat("WHEN '" & idpdrin & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPdrIn)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterPdrIn = IIf(Len(updFilterPdrIn.ToString) = 0, "", updFilterPdrIn & " OR ")
                updFilterPdrIn = String.Concat(updFilterPdrIn, "(idpdrin = '" & idpdrin & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL1 ===========================================


        'MAPPING BUAT WS DATA DETAIL2 -------------------------------------------------------
        'idwoout(0) As Integer, idwo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idbomout(28) As Integer, idpdrout(29) As Integer, 
        'jmlmrs(30) As Double, statusmrs(31) As Integer, jmlmrn(32) As Double, statusmrn(33) As Integer, jmlpd(34) As Double, 
        'statuspd(35) As Integer, isclose(36) As Integer, customtext1(37) As String, customtext2(38) As String, customtext3(39) As String, 
        'customdbl1(40) As Double, customdbl2(41) As Double, customdbl3(42) As Double, customdate1(43) As Date, customdate2(44) As Date, 
        'customdate3(45) As Date

        'MAPPING BUAT FLEX DATA DETAIL2 -----------------------------------------------------
        'idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL2 ======================================================
        'SPLIT PARAMETER DATA DETAIL2
        dataDetail2 = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL2 ===============================================

        'Buat datatable DETAIL2
        Dim dtdetail2 As New DataTable
        AsDataTableTambahField(dtdetail2, "idwoout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail2, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbomout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idpdrout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statuspd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL2 ==================================================
        Dim JmlDtDetail2 As Integer = dataDetail2.Length
        For i = 1 To JmlDtDetail2
            If dataDetail2(i - 1).Length > 0 Then
                'SPLIT DATA DETAIL
                dataRowDetail2 = dataDetail2(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL2 -----------------------------------
                'CEK ARRAY DATA DETAIL2
                If (dataRowDetail2.Length <> 46) Then
                    result(2) = "Detail 2 Row : " & i & " - Invalid detail bahan transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL2 ----------------------------

                'VALIDASI TIPE DATA DETAIL2 ------------------------------------------
                'idwoout(0) As Integer
                If (IsNumeric(dataRowDetail2(0)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idwoout required numeric." : GoTo selesai
                End If
                'idwo(1) As Integer
                If (IsNumeric(dataRowDetail2(1)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idwo required numeric." : GoTo selesai
                End If
                'idbarang(2) As Integer
                If (IsNumeric(dataRowDetail2(2)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idbarang required numeric." : GoTo selesai
                End If
                'jml(5) As Double
                If (IsNumeric(dataRowDetail2(5)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jml required numeric." : GoTo selesai
                End If
                'nilaisatuan(7) As Double
                If (IsNumeric(dataRowDetail2(7)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
                End If
                'jmlbarang(8) As Double
                'jmlbarang = jml * nilaisatuan
                dataRowDetail2(8) = Double.Parse(dataRowDetail2(5)) * Double.Parse(dataRowDetail2(7))
                If (IsNumeric(dataRowDetail2(8)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
                End If
                'kurs(11) As Double
                If (IsNumeric(dataRowDetail2(11)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'harga(12) As Double
                If (IsNumeric(dataRowDetail2(12)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - harga required numeric." : GoTo selesai
                End If
                'hpp(13) As Double
                If (IsNumeric(dataRowDetail2(13)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - hpp required numeric." : GoTo selesai
                End If
                'idhppkhususmasuk(14) As Integer
                If (IsNumeric(dataRowDetail2(14)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
                End If
                'idhppfifomasuk(15) As Integer
                If (IsNumeric(dataRowDetail2(15)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
                End If
                'urutan(27) As Integer
                If (IsNumeric(dataRowDetail2(27)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idbomout(28) As Integer
                If (IsNumeric(dataRowDetail2(28)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idbomout required numeric." : GoTo selesai
                End If
                'idpdrout(29) As Integer
                If (IsNumeric(dataRowDetail2(29)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idpdrout required numeric." : GoTo selesai
                End If
                'jmlmrs(30) As Double
                If (IsNumeric(dataRowDetail2(30)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
                End If
                'statusmrs(31) As Integer
                If (IsNumeric(dataRowDetail2(31)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statusmrs required numeric." : GoTo selesai
                End If
                'jmlmrn(32) As Double
                If (IsNumeric(dataRowDetail2(32)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
                End If
                'statusmrn(33) As Integer
                If (IsNumeric(dataRowDetail2(33)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statusmrn required numeric." : GoTo selesai
                End If
                'jmlpd(34) As Double
                If (IsNumeric(dataRowDetail2(34)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlpd required numeric." : GoTo selesai
                End If
                'statuspd(35) As Integer
                If (IsNumeric(dataRowDetail2(35)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statuspd required numeric." : GoTo selesai
                End If
                'isclose(36) As Integer
                If (IsNumeric(dataRowDetail2(36)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(40) As Double
                If (IsNumeric(dataRowDetail2(40)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(41) As Double
                If (IsNumeric(dataRowDetail2(41)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(42) As Double
                If (IsNumeric(dataRowDetail2(42)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(43) As Date
                If (IsDate(dataRowDetail2(43)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(44) As Date
                If (IsDate(dataRowDetail2(44)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(45) As Date
                If (IsDate(dataRowDetail2(45)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL2 -----------------------------------

                'VALIDASI DATA DETAIL2 ---------------------------------------
                'namabarang(3) As String
                If Len(dataRowDetail2(3)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - namabarang can't be empty" : GoTo selesai
                End If
                'If Len(dataRowDetail2(3)) > 100 Then
                '    result(2) = "Detail 2 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
                'End If

                'jml(5) As Double
                If Len(dataRowDetail2(5)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jml can't be empty" : GoTo selesai
                End If
                If dataRowDetail2(5) <= 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
                End If

                'satuan(6) As String
                If Len(dataRowDetail2(6)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - satuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail2(6)) > 25 Then
                    result(2) = "Detail 2 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
                End If

                'nilaisatuan(7) As Double
                If Len(dataRowDetail2(7)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
                End If

                'jmlbarang(8) As Double
                If Len(dataRowDetail2(8)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
                End If
                If dataRowDetail2(8) <= 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
                End If

                'satuanbarang(9) As String
                If Len(dataRowDetail2(9)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail2(9)) > 25 Then
                    result(2) = "Detail 2 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(11) As Double
                If Len(dataRowDetail2(11)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'harga(12) As Double
                If Len(dataRowDetail2(12)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - harga can't be empty" : GoTo selesai
                End If

                'hpp(13) As Double
                If Len(dataRowDetail2(13)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - hpp can't be empty" : GoTo selesai
                End If

                'rekpersediaan(16) As String
                If Len(dataRowDetail2(16)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail2(16)) > 25 Then
                    result(2) = "Detail 2 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
                End If

                'gudangasal(19) As String
                'If Len(dataRowDetail2(19)) = 0 Then
                '    result(2) = "Detail 2 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
                'End If
                If Len(dataRowDetail2(19)) > 25 Then
                    result(2) = "Detail 2 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
                End If

                'gudangproduksi(20) As String
                'If Len(dataRowDetail2(20)) = 0 Then
                '    result(2) = "Detail 2 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
                'End If
                If Len(dataRowDetail2(20)) > 25 Then
                    result(2) = "Detail 2 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
                End If

                'gudangtujuan(21) As String
                'If Len(dataRowDetail2(21)) = 0 Then
                '    result(2) = "Detail 2 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
                'End If
                If Len(dataRowDetail2(21)) > 25 Then
                    result(2) = "Detail 2 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
                End If

                'jmlmrs(30) As Double
                If Len(dataRowDetail2(30)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
                End If

                'jmlmrn(32) As Double
                If Len(dataRowDetail2(32)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
                End If

                'jmlpd(34) As Double
                If Len(dataRowDetail2(34)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
                End If

                'customdbl1(40) As Double
                If Len(dataRowDetail2(40)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(41) As Double
                If Len(dataRowDetail2(41)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(42) As Double
                If Len(dataRowDetail2(42)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(43) As Date
                If Len(dataRowDetail2(43)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(44) As Date
                If Len(dataRowDetail2(44)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(45) As Date
                If Len(dataRowDetail2(45)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA DETAIL2 --------------------------------

                If AsDataTableTambahData(dtdetail2, "idwoout~idwo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususmasuk~idhppfifomasuk~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomout~idpdrout~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail2(0) & "~" & dataRowDetail2(1) & "~" & dataRowDetail2(2) & "~" & dataRowDetail2(3) & "~" & dataRowDetail2(4) & "~" & dataRowDetail2(5) & "~" & dataRowDetail2(6) & "~" & dataRowDetail2(7) & "~" & dataRowDetail2(8) & "~" & dataRowDetail2(9) & "~" & dataRowDetail2(10) & "~" & dataRowDetail2(11) & "~" & dataRowDetail2(12) & "~" & dataRowDetail2(13) & "~" & dataRowDetail2(14) & "~" & dataRowDetail2(15) & "~" & dataRowDetail2(16) & "~" & dataRowDetail2(17) & "~" & dataRowDetail2(18) & "~" & dataRowDetail2(19) & "~" & dataRowDetail2(20) & "~" & dataRowDetail2(21) & "~" & dataRowDetail2(22) & "~" & dataRowDetail2(23) & "~" & dataRowDetail2(24) & "~" & dataRowDetail2(25) & "~" & dataRowDetail2(26) & "~" & dataRowDetail2(27) & "~" & dataRowDetail2(28) & "~" & dataRowDetail2(29) & "~" & dataRowDetail2(30) & "~" & dataRowDetail2(31) & "~" & dataRowDetail2(32) & "~" & dataRowDetail2(33) & "~" & dataRowDetail2(34) & "~" & dataRowDetail2(35) & "~" & dataRowDetail2(36) & "~" & dataRowDetail2(37) & "~" & dataRowDetail2(38) & "~" & dataRowDetail2(39) & "~" & dataRowDetail2(40) & "~" & dataRowDetail2(41) & "~" & dataRowDetail2(42) & "~" & dataRowDetail2(43) & "~" & dataRowDetail2(44) & "~" & dataRowDetail2(45)) = False Then
                    result(2) = "Detail 2 Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'ValidasiSimpan
                'idbarang(2) As Integer      , idbomout(28) As Integer       , idpdrout(29) As Integer
                idbarang = dataRowDetail2(2) : idbomout = dataRowDetail2(28) : idpdrout = dataRowDetail2(29)

                'VALIDASI OUTSTANDING -------------------------
                ''BOM
                'If idbomout <> 0 Then
                '    '1. CEK DATA EXIST
                '    ftExistOutstandingBomOut = IIf(Len(ftExistOutstandingBomOut.ToString) = 0, "", ftExistOutstandingBomOut & " UNION ")
                '    ftExistOutstandingBomOut = String.Concat(ftExistOutstandingBomOut, "SELECT EXISTS(SELECT 1 FROM m6_bom_out JOIN m6_bom ON idbom = bomid WHERE idbomout = '" & idbomout & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomout & "' as idbomout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                '    '2. CEK JML OUTSTANDING
                '    Dim Outstanding As Double = AsDataTableDSum(dtdetail2, "jmlbarang", "idbomout=" & idbomout)
                '    ftOutstandingBomOut = IIf(Len(ftOutstandingBomOut.ToString) = 0, "", ftOutstandingBomOut & " OR ")
                '    ftOutstandingBomOut = String.Concat(ftOutstandingBomOut, " (bomout.idbomout = " & idbomout & " AND " & Outstanding & " > bomout.jmlbarang) ")
                'End If

                'PDR
                If idpdrout <> 0 Then
                    '1. CEK DATA EXIST
                    ftExistOutstandingPdrOut = IIf(Len(ftExistOutstandingPdrOut.ToString) = 0, "", ftExistOutstandingPdrOut & " UNION ")
                    ftExistOutstandingPdrOut = String.Concat(ftExistOutstandingPdrOut, "SELECT EXISTS(SELECT 1 FROM m6_pdr_out JOIN m6_pdr ON idpdr = pdrid WHERE idpdrout = '" & idpdrout & "' AND (pdrstatus = 2 OR pdrstatus = 3 OR pdrstatus = 4 OR pdrstatus = 7) LIMIT 1) as rowExists, '" & idpdrout & "' as idpdrout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                    '2. CEK JML OUTSTANDING
                    Dim Outstanding As Double = AsDataTableDSum(dtdetail2, "jmlbarang", "idpdrout=" & idpdrout)
                    ftOutstandingPdrOut = IIf(Len(ftOutstandingPdrOut.ToString) = 0, "", ftOutstandingPdrOut & " OR ")
                    ftOutstandingPdrOut = String.Concat(ftOutstandingPdrOut, " (pdrout.idpdrout = " & idpdrout & " AND " & Outstanding & " > (pdrout.jmlbarang - pdrout.jmlrealisasi)) ")

                    '3. SET NILAI UPDATE OUTSTANDING
                    updNilaiPdrOut = String.Concat("WHEN '" & idpdrout & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPdrOut)

                    '4. SET FILTER UPDATE OUTSTANDING
                    updFilterPdrOut = IIf(Len(updFilterPdrOut.ToString) = 0, "", updFilterPdrOut & " OR ")
                    updFilterPdrOut = String.Concat(updFilterPdrOut, "(idpdrout = '" & idpdrout & "')")
                End If
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------
            End If

        Next

        Dim dtdetail3 As New DataTable
        'Buat datatable detail
        AsDataTableTambahField(dtdetail3, "idwoactivity", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "idwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail3, "idpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail3, "namaaktivitas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "kodemesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail3, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail3, "customdate3", AsEnumTypeData.AsString)

        If dataSplit(3).Length > 0 Then
            'VALIDASI DAN SET DATA ROW DETAIL ==================================================
            dataDetail3 = dataSplit(3).Split(sptRow)
            Dim JmlDtDetail3 As Integer = dataDetail3.Length
            For i = 1 To JmlDtDetail3
                'SPLIT DATA DETAIL
                dataRowDetail = dataDetail3(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowDetail.Length <> 20) Then
                    result(2) = "Row : " & i & " - Invalid detail activity transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------

                'urutan(26) As Integer
                If (IsNumeric(dataRowDetail(10)) = False) Then
                    result(2) = "urutan required numeric." : GoTo selesai
                End If
                'customdbl1(32) As Double
                If (IsNumeric(dataRowDetail(14)) = False) Then
                    result(2) = "customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(33) As Double
                If (IsNumeric(dataRowDetail(15)) = False) Then
                    result(2) = "customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(34) As Double
                If (IsNumeric(dataRowDetail(16)) = False) Then
                    result(2) = "customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(35) As Date
                If (IsDate(dataRowDetail(17)) = False) Then
                    result(2) = "customdate1 required date." : GoTo selesai
                End If
                'customdate2(36) As Date
                If (IsDate(dataRowDetail(18)) = False) Then
                    result(2) = "customdate2 required date." : GoTo selesai
                End If
                'customdate3(37) As Date
                If (IsDate(dataRowDetail(19)) = False) Then
                    result(2) = "customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'idpadetail(0) As Integer
                If Len(dataRowDetail(0)) = 0 Then
                    result(2) = "Row : " & i & " - idwoactivity can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(0)) > 20 Then
                    result(2) = "Row : " & i & " - idwoactivity should not be more than 20 character." : GoTo selesai
                End If

                'idpa(1) As Integer 
                If Len(dataRowDetail(1)) = 0 Then
                    result(2) = "Row : " & i & " - idwo can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(1)) > 20 Then
                    result(2) = "Row : " & i & " - idwo should not be more than 20 character." : GoTo selesai
                End If

                'idbarang(2) As Integer 
                If Len(dataRowDetail(2)) = 0 Then
                    result(2) = "Row : " & i & " - idpa can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(2)) > 20 Then
                    result(2) = "Row : " & i & " - idpa should not be more than 20 character." : GoTo selesai
                End If

                'namabarang(3) As String
                If Len(dataRowDetail(3)) = 0 Then
                    result(2) = "Row : " & i & " - namaaktivitas can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(3)) > 100 Then
                    result(2) = "Row : " & i & " - namaaktivitas should not be more than 100 character." : GoTo selesai
                End If

                'customdbl1(32) As Double
                If Len(dataRowDetail(14)) = 0 Then
                    result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(33) As Double
                If Len(dataRowDetail(15)) = 0 Then
                    result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(34) As Double
                If Len(dataRowDetail(16)) = 0 Then
                    result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(35) As Date
                If Len(dataRowDetail(17)) = 0 Then
                    result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(36) As Date
                If Len(dataRowDetail(18)) = 0 Then
                    result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(37) As Date
                If Len(dataRowDetail(19)) = 0 Then
                    result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA DETAIL --------------------------------

                AsDataTableTambahData(dtdetail3, "idwoactivity~idwo~idpa~namaaktivitas~kodemesin~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19))

            Next
        End If

        Dim dtdetail4 As New DataTable
        'Buat datatable detail
        AsDataTableTambahField(dtdetail4, "idworoutecard", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "idwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail4, "notransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail4, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail4, "customdate3", AsEnumTypeData.AsString)

        If dataSplit(4).Length > 0 Then
            dataDetail4 = dataSplit(4).Split(sptRow)
            Dim JmlDtDetail4 As Integer = dataDetail4.Length
            For i = 1 To JmlDtDetail4
                'SPLIT DATA DETAIL
                dataRowDetail = dataDetail4(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowDetail.Length <> 20) Then
                    result(2) = "Row : " & i & " - Invalid detail route card transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------

                If (IsNumeric(dataRowDetail(3)) = False) Then
                    result(2) = "jml required numeric." : GoTo selesai
                End If
                'urutan(26) As Integer
                If (IsNumeric(dataRowDetail(10)) = False) Then
                    result(2) = "urutan required numeric." : GoTo selesai
                End If
                'customdbl1(32) As Double
                If (IsNumeric(dataRowDetail(14)) = False) Then
                    result(2) = "customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(33) As Double
                If (IsNumeric(dataRowDetail(15)) = False) Then
                    result(2) = "customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(34) As Double
                If (IsNumeric(dataRowDetail(16)) = False) Then
                    result(2) = "customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(35) As Date
                If (IsDate(dataRowDetail(17)) = False) Then
                    result(2) = "customdate1 required date." : GoTo selesai
                End If
                'customdate2(36) As Date
                If (IsDate(dataRowDetail(18)) = False) Then
                    result(2) = "customdate2 required date." : GoTo selesai
                End If
                'customdate3(37) As Date
                If (IsDate(dataRowDetail(19)) = False) Then
                    result(2) = "customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'idpadetail(0) As Integer
                If Len(dataRowDetail(0)) = 0 Then
                    result(2) = "Row : " & i & " - idworoutecard can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(0)) > 20 Then
                    result(2) = "Row : " & i & " - idworoutecard should not be more than 20 character." : GoTo selesai
                End If

                'idpa(1) As Integer 
                If Len(dataRowDetail(1)) = 0 Then
                    result(2) = "Row : " & i & " - idwo can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(1)) > 20 Then
                    result(2) = "Row : " & i & " - idwo should not be more than 20 character." : GoTo selesai
                End If

                'customdbl1(32) As Double
                If Len(dataRowDetail(14)) = 0 Then
                    result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(33) As Double
                If Len(dataRowDetail(15)) = 0 Then
                    result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(34) As Double
                If Len(dataRowDetail(16)) = 0 Then
                    result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(35) As Date
                If Len(dataRowDetail(17)) = 0 Then
                    result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(36) As Date
                If Len(dataRowDetail(18)) = 0 Then
                    result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(37) As Date
                If Len(dataRowDetail(19)) = 0 Then
                    result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA DETAIL --------------------------------

                AsDataTableTambahData(dtdetail4, "idworoutecard~idwo~notransaksi~jml~satuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19))

            Next
        End If

        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================
        'END OF VALIDASI DAN SET ROW DATA DETAIL2 ===========================================


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
                Dim vModuleId As Integer = 6, vMenuId As Integer = 5
                Select Case drutama("wostatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("wotgl")), AsFormatTanggal(drutama("wotgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("wostatus") = 2 Or drutama("wostatus") = 1 Or drutama("wostatus") = 8 Or drutama("wostatus") = 9 Or drutama("wostatus") = 10 Or drutama("wostatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingBomIn, ftOutstandingBomIn, ftExistOutstandingPdrIn, ftOutstandingPdrIn, dtdetail2, ftExistOutstandingBomOut, ftOutstandingBomOut, ftExistOutstandingPdrOut, ftOutstandingPdrOut)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("woid")
                    notransaksi = drutama("wonotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(woid), wonotransaksi FROM M6_wo WHERE woid='" & result(4) & "' AND wostatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("woautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("wocabang"), drutama("wolokasi"), drutama("wosumber"), drutama("wotgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(woid) FROM M6_wo WHERE wonotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_wo_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Wo_HistorySimpan("" & paramSplit(0) & "★M6_Wo_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("wosumber")) & "▼" & FixQuotes(drutama("woid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Wo set wocabang  = '" & FixQuotes(drutama("wocabang")) & "', wolokasi  = '" & FixQuotes(drutama("wolokasi")) & "', wogudangasal  = '" & FixQuotes(drutama("wogudangasal")) & "', wogudangproduksi  = '" & FixQuotes(drutama("wogudangproduksi")) & "', wogudangtujuan  = '" & FixQuotes(drutama("wogudangtujuan")) & "', wosumber  = '" & FixQuotes(drutama("wosumber")) & "', wojenis  = '" & FixQuotes(drutama("wojenis")) & "', woautonotransaksi  = " & drutama("woautonotransaksi") & ", wonotransaksi  = '" & FixQuotes(notransaksi) & "', wotgl  = '" & FixQuotes(AsFormatTanggal(drutama("wotgl"))) & "', wokodepa  = " & drutama("wokodepa") & ", wodimintaoleh  = " & drutama("wodimintaoleh") & ", wodimintaolehkontak  = '" & FixQuotes(drutama("wodimintaolehkontak")) & "', womintake  = " & drutama("womintake") & ", wotgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("wotgldipakai"))) & "', woestimasikerja  = '" & FixQuotes(drutama("woestimasikerja")) & "', womatauang  = '" & FixQuotes(drutama("womatauang")) & "', wokurs  = '" & FixDouble(drutama("wokurs")) & "', wototalhargain  = '" & FixDouble(drutama("wototalhargain")) & "', wototalhargaout  = '" & FixDouble(drutama("wototalhargaout")) & "', wototalhppin  = '" & FixDouble(drutama("wototalhppin")) & "', wototalhppout  = '" & FixDouble(drutama("wototalhppout")) & "', wouraian  = '" & FixQuotes(drutama("wouraian")) & "', wocatatan  = '" & FixQuotes(drutama("wocatatan")) & "', wonoref  = '" & FixQuotes(drutama("wonoref")) & "', wotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("wotglnoref"))) & "', woidbom  = " & drutama("woidbom") & ", woidpdr  = " & drutama("woidpdr") & ", wostatusmrsin  = " & drutama("wostatusmrsin") & ", wostatusmrsout  = " & drutama("wostatusmrsout") & ", wostatusmrnin  = " & drutama("wostatusmrnin") & ", wostatusmrnout  = " & drutama("wostatusmrnout") & ", wostatuspdin  = " & drutama("wostatuspdin") & ", wostatuspdout  = " & drutama("wostatuspdout") & ", wostatus  = " & drutama("wostatus") & ", wostatussebelumnya  = " & drutama("wostatussebelumnya") & ", wojmlrevisi  = wojmlrevisi+1, wocetakanke  = " & drutama("wocetakanke") & ", womodifikasiuser  = " & drutama("womodifikasiuser") & ", womodifikasitgl  = NOW(), wocustomtext1  = '" & FixQuotes(drutama("wocustomtext1")) & "', wocustomtext2  = '" & FixQuotes(drutama("wocustomtext2")) & "', wocustomtext3  = '" & FixQuotes(drutama("wocustomtext3")) & "', wocustomtext4  = '" & FixQuotes(drutama("wocustomtext4")) & "', wocustomtext5  = '" & FixQuotes(drutama("wocustomtext5")) & "', wocustomint1  = " & drutama("wocustomint1") & ", wocustomint2  = " & drutama("wocustomint2") & ", wocustomint3  = " & drutama("wocustomint3") & ", wocustomdbl1  = '" & FixDouble(drutama("wocustomdbl1")) & "', wocustomdbl2  = '" & FixDouble(drutama("wocustomdbl2")) & "', wocustomdbl3  = '" & FixDouble(drutama("wocustomdbl3")) & "', wocustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate1"))) & "', wocustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate2"))) & "', wocustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate3"))) & "', woaktivitas = '" & FixDouble(drutama("woaktivitas")) & "' where woid = '" & drutama("woid") & "'"
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

                    If drutama("woautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("wocabang"), drutama("wolokasi"), drutama("wosumber"), drutama("wotgl"))
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
                        notransaksi = drutama("wonotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(woid) FROM m6_wo WHERE wonotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Wo (wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, womodifikasitgl, woisclose, wocustomtext1, wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, wocustomint1, wocustomint2, wocustomint3, wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, wocustomdate2, wocustomdate3, woaktivitas) values('" & FixQuotes(drutama("wocabang")) & "', '" & FixQuotes(drutama("wolokasi")) & "', '" & FixQuotes(drutama("wogudangasal")) & "', '" & FixQuotes(drutama("wogudangproduksi")) & "', '" & FixQuotes(drutama("wogudangtujuan")) & "', '" & FixQuotes(drutama("wosumber")) & "', '" & FixQuotes(drutama("wojenis")) & "', " & drutama("woautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("wotgl"))) & "', " & drutama("wokodepa") & ", " & drutama("wodimintaoleh") & ", '" & FixQuotes(drutama("wodimintaolehkontak")) & "', " & drutama("womintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("wotgldipakai"))) & "', '" & FixQuotes(drutama("woestimasikerja")) & "', '" & FixQuotes(drutama("womatauang")) & "', '" & FixDouble(drutama("wokurs")) & "', '" & FixDouble(drutama("wototalhargain")) & "', '" & FixDouble(drutama("wototalhargaout")) & "', '" & FixDouble(drutama("wototalhppin")) & "', '" & FixDouble(drutama("wototalhppout")) & "', '" & FixQuotes(drutama("wouraian")) & "', '" & FixQuotes(drutama("wocatatan")) & "', '" & FixQuotes(drutama("wonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("wotglnoref"))) & "', " & drutama("woidbom") & ", " & drutama("woidpdr") & ", " & drutama("wostatusmrsin") & ", " & drutama("wostatusmrsout") & ", " & drutama("wostatusmrnin") & ", " & drutama("wostatusmrnout") & ", " & drutama("wostatuspdin") & ", " & drutama("wostatuspdout") & ", " & drutama("wostatus") & ", " & drutama("wostatussebelumnya") & ", " & drutama("wojmlrevisi") & ", " & drutama("wocetakanke") & ", " & drutama("woinputuser") & ", NOW(), " & drutama("womodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("woisclose") & ", '" & FixQuotes(drutama("wocustomtext1")) & "', '" & FixQuotes(drutama("wocustomtext2")) & "', '" & FixQuotes(drutama("wocustomtext3")) & "', '" & FixQuotes(drutama("wocustomtext4")) & "', '" & FixQuotes(drutama("wocustomtext5")) & "', " & drutama("wocustomint1") & ", " & drutama("wocustomint2") & ", " & drutama("wocustomint3") & ", '" & FixDouble(drutama("wocustomdbl1")) & "', '" & FixDouble(drutama("wocustomdbl2")) & "', '" & FixDouble(drutama("wocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate3"))) & "', '" & FixDouble(drutama("woaktivitas")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select woid from M6_wo where wonotransaksi='" & notransaksi & "' AND woinputuser= '" & userid & "' order by womodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Wo_In where idwo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                Dim vCostCenter As String = ""

                'Proses detail1
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        vCostCenter = FixQuotes(dr1("costcenter"))
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idwoin") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomin") & ", " & dr1("idpdrin") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Wo_In(idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail In Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail2 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Wo_Out where idwo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail2
                If (dtdetail2.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    Dim strValueBooking As New StringBuilder
                    For Each dr1 As DataRow In dtdetail2.Rows
                        strValueBooking.Clear()
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idwoout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomout") & ", " & dr1("idpdrout") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(vCostCenter) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")

                        dtupdate = AsDataTableAmbilDariDBCon("SELECT idbarang, gudang, jmlbooking FROM M1_item_booking WHERE idbarang = " & dr1("idbarang") & " AND gudang = '" & FixQuotes(drutama("wogudangasal")) & "'", myConn)
                        If dtupdate.Rows.Count > 0 Then
                            Dim jmlbooking As Double = dtupdate.Rows(0)(2)
                            jmlbooking = jmlbooking + FixDouble(dr1("jml"))
                            sql = "Update M1_item_booking set jmlbooking  = '" & FixDouble(jmlbooking) & "' where idbarang = " & dtupdate.Rows(0)(0) & " AND gudang = '" & FixQuotes(dtupdate.Rows(0)(1)) & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            strValueBooking.Append(IIf(Len(strValueBooking.ToString) = 0, "", ", "))
                            strValueBooking.Append("(" & dr1("idbarang") & ", '" & FixQuotes(drutama("wogudangasal")) & "', '" & FixDouble(dr1("jml")) & "')")
                            sql = "Insert into M1_item_booking(idbarang, gudang, jmlbooking) values" & strValueBooking.ToString & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking+VALUES(jmlbooking)"
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
                    sql = "Insert into M6_Wo_Out(idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'Else
                    '    result(2) = "Detail Out Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail3 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_wo_activity where idwo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail3
                If (dtdetail3.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail3.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idwoactivity")) & "', " & result(4) & ", '" & FixQuotes(dr1("idpa")) & "', '" & FixQuotes(dr1("namaaktivitas")) & "', '" & FixQuotes(dr1("kodemesin")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_wo_activity(idwoactivity, idwo, idpa, namaaktivitas, kodemesin, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'Else
                    '    result(2) = "Detail Out Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail4 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_wo_route_card where idwo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail4
                If (dtdetail4.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail4.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idworoutecard")) & "', " & result(4) & ", '" & FixQuotes(notransaksi & "-" & dr1("urutan")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_wo_route_card(idworoutecard, idwo, notransaksi, jml, satuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'Else
                    '    result(2) = "Detail Out Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                If drutama("wostatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ===================================================
                    Dim updNilaiPdrUtamaIn = "", updNilaiPdrUtamaOut = "", updFilterPdrUtama = ""

                    'PDR IN
                    If Len(updNilaiPdrIn) > 0 Then
                        'UPDATE DETAIL IN
                        sql = "UPDATE m6_pdr_in SET jmlrealisasi = (CASE idpdrin " & updNilaiPdrIn & " ELSE jmlrealisasi END) WHERE " & updFilterPdrIn
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'AMBIL ID UTAMA IN
                        Dim ftDetail As String = ""
                        Dim dtIn As DataTable = AsDataTableAmbilDariDBCon("SELECT idpdr FROM m6_pdr_in WHERE " & updFilterPdrIn & " GROUP BY idpdr", myConn)
                        If dtIn.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtIn.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                            Next
                        End If

                        'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                        If Len(ftDetail) > 0 Then
                            dtIn = AsDataTableAmbilDariDBCon("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_in WHERE " & ftDetail & " GROUP BY idpdr", myConn)
                            If dtIn.Rows.Count > 0 Then
                                For Each dr1 As DataRow In dtIn.Rows
                                    '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                    Dim statusIn As Integer = 0
                                    If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                        statusIn = 2
                                    ElseIf dr1("jmlrealisasi") < 1 Then
                                        statusIn = 0
                                    Else
                                        statusIn = 1
                                    End If
                                    '2. SET NILAI UPDATE OUTSTANDING
                                    updNilaiPdrUtamaIn = String.Concat(updNilaiPdrUtamaIn, "WHEN '" & dr1("idpdr") & "' THEN '" & statusIn & "' ")
                                    '3. SET FILTERUPDATE OUTSTANDING
                                    updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                    updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                                Next
                            End If
                        End If

                    End If

                    'PDR OUT
                    If Len(updNilaiPdrOut) > 0 Then
                        'UPDATE DETAIL OUT
                        sql = "UPDATE m6_pdr_out SET jmlrealisasi = (CASE idpdrout " & updNilaiPdrOut & " ELSE jmlrealisasi END) WHERE " & updFilterPdrOut
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpdr FROM m6_pdr_out WHERE " & updFilterPdrOut & " GROUP BY idpdr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                            Next
                        End If

                        'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                        If Len(ftDetail) > 0 Then
                            dtOut = AsDataTableAmbilDariDBCon("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_out WHERE " & ftDetail & " GROUP BY idpdr", myConn)
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
                                    updNilaiPdrUtamaOut = String.Concat(updNilaiPdrUtamaOut, "WHEN '" & dr1("idpdr") & "' THEN '" & statusOut & "' ")
                                    '3. SET FILTERUPDATE OUTSTANDING
                                    updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                    updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                                Next
                            End If
                        End If

                    End If

                    'PDR UTAMA
                    'UPDATE STATUS IN DAN OUT
                    If Len(updNilaiPdrUtamaIn) > 0 And Len(updNilaiPdrUtamaOut) > 0 Then
                        sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END), pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE STATUS IN
                    ElseIf Len(updNilaiPdrUtamaIn) > 0 Then
                        sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END) WHERE " & updFilterPdrUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE STATUS OUT
                    ElseIf Len(updNilaiPdrUtamaOut) > 0 Then
                        sql = "UPDATE m6_pdr SET pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
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
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "WO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M6_WoUpdateStatus(ByVal param As String) As String

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
        Dim dtdetail As DataTable, dtdetailIn As DataTable, dtdetailOut As DataTable
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
            Dim sumber As String = "Wo", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Wotgl, Wonotransaksi, Wostatus FROM M6_Wo WHERE Woid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Wostatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_wo_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Wo_HistorySimpan("" & paramSplit(0) & "★M6_Wo_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m6_wo_terkait("woid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                Dim updNilaiPdrUtamaIn = "", updNilaiPdrUtamaOut = "", updFilterPdrUtama = ""
                Dim idbarang As Integer = 0
                Dim idpdrin As Integer = 0, idpdrout As Integer = 0
                Dim updNilaiPdrIn As String = "", updFilterPdrIn As String = ""
                Dim updNilaiPdrOut As String = "", updFilterPdrOut As String = ""

                'AMBIL DATA DETAIL IN
                dtdetailIn = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpdrin, urutan FROM m6_wo_in WHERE idwo = '" & idtransaksi & "'", myConn)
                If dtdetailIn.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetailIn.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : idpdrin = dr1("idpdrin")

                        'UPDATE OUTSTANDING ---------------------------
                        If idpdrin <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING 
                            Dim Outstanding As Double = AsDataTableDSum(dtdetailIn, "jmlbarang", "idpdrin=" & idpdrin)
                            updNilaiPdrIn = String.Concat("WHEN '" & idpdrin & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPdrIn)
                            '2. SET FILTERUPDATE OUTSTANDING 
                            updFilterPdrIn = IIf(Len(updFilterPdrIn.ToString) = 0, "", updFilterPdrIn & " OR ")
                            updFilterPdrIn = String.Concat(updFilterPdrIn, "(idpdrin = '" & idpdrin & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found. (Result)" : Trans.Rollback() : GoTo selesai
                End If

                'PDR IN
                If Len(updNilaiPdrIn) > 0 Then
                    'UPDATE DETAIL IN
                    sql = "UPDATE m6_pdr_in SET jmlrealisasi = (CASE idpdrin " & updNilaiPdrIn & " ELSE jmlrealisasi END) WHERE " & updFilterPdrIn
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'AMBIL ID UTAMA IN
                    Dim ftDetail As String = ""
                    Dim dtIn As DataTable = AsDataTableAmbilDariDBCon("SELECT idpdr FROM m6_pdr_in WHERE " & updFilterPdrIn & " GROUP BY idpdr", myConn)
                    If dtIn.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtIn.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                        Next
                    End If

                    'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                    If Len(ftDetail) > 0 Then
                        dtIn = AsDataTableAmbilDariDBCon("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_in WHERE " & ftDetail & " GROUP BY idpdr", myConn)
                        If dtIn.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtIn.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                Dim statusIn As Integer = 0
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusIn = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusIn = 0
                                Else
                                    statusIn = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPdrUtamaIn = String.Concat(updNilaiPdrUtamaIn, "WHEN '" & dr1("idpdr") & "' THEN '" & statusIn & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                            Next
                        End If
                    End If

                End If

                'AMBIL DATA DETAIL OUT
                dtdetailOut = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpdrout, urutan FROM m6_wo_out WHERE idwo = '" & idtransaksi & "'", myConn)
                If dtdetailOut.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetailOut.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : idpdrout = dr1("idpdrout")

                        'UPDATE OUTSTANDING ---------------------------
                        If idpdrout <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING 
                            Dim Outstanding As Double = AsDataTableDSum(dtdetailOut, "jmlbarang", "idpdrout=" & idpdrout)
                            updNilaiPdrOut = String.Concat("WHEN '" & idpdrout & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPdrOut)
                            '2. SET FILTERUPDATE OUTSTANDING 
                            updFilterPdrOut = IIf(Len(updFilterPdrOut.ToString) = 0, "", updFilterPdrOut & " OR ")
                            updFilterPdrOut = String.Concat(updFilterPdrOut, "(idpdrout = '" & idpdrout & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                    'Else
                    '    result(2) = "Detail transaction not found. (Material)" : Trans.Rollback() : GoTo selesai
                End If

                'PDR OUT
                If Len(updNilaiPdrOut) > 0 Then
                    'UPDATE DETAIL OUT
                    sql = "UPDATE m6_pdr_out SET jmlrealisasi = (CASE idpdrout " & updNilaiPdrOut & " ELSE jmlrealisasi END) WHERE " & updFilterPdrOut
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpdr FROM m6_pdr_out WHERE " & updFilterPdrOut & " GROUP BY idpdr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                        Next
                    End If

                    'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                    If Len(ftDetail) > 0 Then
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_out WHERE " & ftDetail & " GROUP BY idpdr", myConn)
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
                                updNilaiPdrUtamaOut = String.Concat(updNilaiPdrUtamaOut, "WHEN '" & dr1("idpdr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                            Next
                        End If
                    End If

                End If

                'PDR UTAMA
                'UPDATE STATUS IN DAN OUT
                If Len(updNilaiPdrUtamaIn) > 0 And Len(updNilaiPdrUtamaOut) > 0 Then
                    sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END), pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE STATUS IN
                ElseIf Len(updNilaiPdrUtamaIn) > 0 Then
                    sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END) WHERE " & updFilterPdrUtama
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE STATUS OUT
                ElseIf Len(updNilaiPdrUtamaOut) > 0 Then
                    sql = "UPDATE m6_pdr SET pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
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

            End If

            'update status utama
            sql = "UPDATE M6_Wo SET Wostatus = " & nilaiStatus & ", Womodifikasiuser='" & userid & "', Womodifikasitgl = NOW(), Woposting = 0, Wopostingtgl = '1971-01-01 00:00:00', Wojmlrevisi = Wojmlrevisi + 1 WHERE Woid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_WoSearch(PostWsSearch(paramSplit(0), "M6_WoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_WoDelete(ByVal param As String) As String

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
            Dim sumber As String = "Wo", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Woid, Wonotransaksi FROM M6_Wo WHERE Woid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT wocabang, wolokasi, wosumber, woautonotransaksi, wonotransaksi, wotgl"
            sql &= " FROM M6_wo"
            sql &= " WHERE woid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("wocabang")
                lokasi = dtNomorNext.Rows(0)("wolokasi")
                sumber = dtNomorNext.Rows(0)("wosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("woautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("wonotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("wotgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_Wo_In WHERE idwo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL2
            sql = "DELETE FROM M6_Wo_Out WHERE idwo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M6_Wo WHERE woid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_WoSearch(PostWsSearch(paramSplit(0), "M6_WoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M6_WoGetdataById(ByVal param As String) As String
        'M6_WoGetdataById Utama --------------------------------------------------------
        'woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, 
        'wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, 
        'womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, 
        'wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, 
        'woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, 
        'wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, 
        'woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocustomtext1, 
        'wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, wocustomint1, wocustomint2, wocustomint3, 
        'wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, wocustomdate2, wocustomdate3, wocabangnama, 
        'wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, 
        'womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, 
        'woinputusernama, womodifikasiusernama, woaktivitas, woaktivitaskode, woaktivitasnama, wojeniswajibwo

        'M6_WoGetdataById In --------------------------------------------------------
        'idwoin, idwo, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, 
        'subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, 
        'jmlsisapd, jmlsisarealisasi

        'M6_WoGetdataById Out --------------------------------------------------------
        'idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, 
        'proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, 
        'jmlsisarealisasi

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

        Dim utama As String = "", detail As String = "", detailout As String = "", detailactivity As String = "", detailroutecard As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_pl~M5_pl_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "woi.statusrealisasi")

            Filter2 = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter2 = Filter2.Replace("statusrealisasi", "woo.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "woid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "woid = " & idtransaksi & " and " & Filter
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idwo='" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idwo='" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_wo_getdata")
        sql = "select wo.woid AS woid, wo.wocabang AS wocabang, wo.wolokasi AS wolokasi, wo.wogudangasal AS wogudangasal, wo.wogudangproduksi AS wogudangproduksi, wo.wogudangtujuan AS wogudangtujuan, wo.wosumber AS wosumber, wo.wojenis AS wojenis, wo.woautonotransaksi AS woautonotransaksi, wo.wonotransaksi AS wonotransaksi, wo.wotgl AS wotgl, wo.wokodepa AS wokodepa, wo.wodimintaoleh AS wodimintaoleh, wo.wodimintaolehkontak AS wodimintaolehkontak, wo.womintake AS womintake, wo.wotgldipakai AS wotgldipakai, wo.woestimasikerja AS woestimasikerja, wo.womatauang AS womatauang, wo.wokurs AS wokurs, wo.wototalhargain AS wototalhargain, wo.wototalhargaout AS wototalhargaout, wo.wototalhppin AS wototalhppin, wo.wototalhppout AS wototalhppout, wo.wouraian AS wouraian, wo.wocatatan AS wocatatan, wo.wonoref AS wonoref, wo.wotglnoref AS wotglnoref, wo.woidbom AS woidbom, wo.woidpdr AS woidpdr, wo.wostatusmrsin AS wostatusmrsin, wo.wostatusmrsout AS wostatusmrsout, wo.wostatusmrnin AS wostatusmrnin, wo.wostatusmrnout AS wostatusmrnout, wo.wostatuspdin AS wostatuspdin, wo.wostatuspdout AS wostatuspdout, wo.wostatusrealisasiin AS wostatusrealisasiin, wo.wostatusrealisasiout AS wostatusrealisasiout, wo.wostatus AS wostatus, wo.wostatussebelumnya AS wostatussebelumnya, wo.wojmlrevisi AS wojmlrevisi, wo.wocetakanke AS wocetakanke, wo.woinputuser AS woinputuser, wo.woinputtgl AS woinputtgl, wo.womodifikasiuser AS womodifikasiuser, wo.womodifikasitgl AS womodifikasitgl, wo.woposting AS woposting, wo.wopostingtgl AS wopostingtgl, wo.woisclose AS woisclose, wo.wocustomtext1 AS wocustomtext1, wo.wocustomtext2 AS wocustomtext2, wo.wocustomtext3 AS wocustomtext3, wo.wocustomtext4 AS wocustomtext4, wo.wocustomtext5 AS wocustomtext5, wo.wocustomint1 AS wocustomint1, wo.wocustomint2 AS wocustomint2, wo.wocustomint3 AS wocustomint3, wo.wocustomdbl1 AS wocustomdbl1, wo.wocustomdbl2 AS wocustomdbl2, wo.wocustomdbl3 AS wocustomdbl3, wo.wocustomdate1 AS wocustomdate1, wo.wocustomdate2 AS wocustomdate2, wo.wocustomdate3 AS wocustomdate3, br.bnama AS wocabangnama, lc.lnama AS wolokasinama, wh1.wnama AS wogudangasalnama, wh2.wnama AS wogudangproduksinama, wh3.wnama AS wogudangtujuannama, pc.pcnama AS wojenisnama, c1.kkode AS wodimintaolehkode, c1.knama AS wodimintaolehnama, c2.kkode AS womintakekode, c2.knama AS womintakenama, we.wenama AS woestimasikerjanama, bom.bomnotransaksi AS wonotransaksibom, pdr.pdrnotransaksi AS wonotransaksipdr, st1.nama AS wostatusnama, st2.nama AS wostatussebelumnyanama, u1.unama AS woinputusernama, u2.unama AS womodifikasiusernama, wo.woaktivitas, pa.pakode as woaktivitaskode, pa.panama as woaktivitasnama, pc.pcwajibwo AS wojeniswajibwo, woi.idwoin AS idwoin, woi.idwo AS idwo, woi.idbarang AS idbarang, woi.namabarang AS namabarang, woi.tipebarang AS tipebarang, woi.jml AS jml, woi.satuan AS satuan, woi.nilaisatuan AS nilaisatuan, woi.jmlbarang AS jmlbarang, woi.satuanbarang AS satuanbarang, woi.matauang AS matauang, woi.kurs AS kurs, woi.harga AS harga, woi.hpppersen AS hpppersen, woi.hpp AS hpp, i.brekpersediaan AS rekpersediaan, woi.cabang AS cabang, woi.lokasi AS lokasi, woi.gudangasal AS gudangasal, woi.gudangproduksi AS gudangproduksi, woi.gudangtujuan AS gudangtujuan, woi.costcenter AS costcenter, woi.divisi AS divisi, woi.subdivisi AS subdivisi, woi.proyek AS proyek, woi.catatan AS catatan, woi.urutan AS urutan, woi.idbomin AS idbomin, woi.idpdrin AS idpdrin, woi.jmlmrs AS jmlmrs, woi.statusmrs AS statusmrs, woi.jmlmrn AS jmlmrn, woi.statusmrn AS statusmrn, woi.jmlpd AS jmlpd, woi.statuspd AS statuspd, woi.jmlrealisasi AS jmlrealisasi, woi.statusrealisasi AS statusrealisasi, woi.isclose AS isclose, woi.customtext1 AS customtext1, woi.customtext2 AS customtext2, woi.customtext3 AS customtext3, woi.customdbl1 AS customdbl1, woi.customdbl2 AS customdbl2, woi.customdbl3 AS customdbl3, woi.customdate1 AS customdate1, woi.customdate2 AS customdate2, woi.customdate3 AS customdate3, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama, wo.wonotransaksi AS notransaksi, bom2.bomnotransaksi AS bomnotransaksi, pdr2.pdrnotransaksi AS pdrnotransaksi, ((woi.jmlbarang - woi.jmlmrs) / woi.nilaisatuan) AS jmlsisamrs, ((woi.jmlbarang - woi.jmlmrn) / woi.nilaisatuan) AS jmlsisamrn, ((woi.jmlbarang - woi.jmlpd) / woi.nilaisatuan) AS jmlsisapd, ((woi.jmlbarang - woi.jmlrealisasi) / woi.nilaisatuan) AS jmlsisarealisasi from m6_wo wo join m6_wo_in woi on wo.woid = woi.idwo left join m1_branch br on wo.wocabang = br.bkode left join m1_location lc on wo.wolokasi = lc.lkode left join m1_warehouse wh1 on wo.wogudangasal = wh1.wkode left join m1_warehouse wh2 on wo.wogudangproduksi = wh2.wkode left join m1_warehouse wh3 on wo.wogudangtujuan = wh3.wkode left join m1_production_category pc on wo.wojenis = pc.pckode left join m1_contact c1 on wo.wodimintaoleh = c1.kid left join m1_contact c2 on wo.womintake = c2.kid left join m1_working_estimate we on wo.woestimasikerja = we.wekode left join m6_bom bom on wo.woidbom = bom.bomid left join m6_pdr pdr on wo.woidpdr = pdr.pdrid left join m0_status st1 on wo.wostatus = st1.kode left join m0_status st2 on wo.wostatussebelumnya = st2.kode left join m0_user u1 on wo.woinputuser = u1.userid left join m0_user u2 on wo.womodifikasiuser = u2.userid left join m1_production_activity pa on wo.woaktivitas = pa.paid left join m1_item i on woi.idbarang = i.bid left join m1_cost_center cc on woi.costcenter = cc.cckode left join m1_division d on woi.divisi = d.dkode left join m1_subdivision sd on woi.subdivisi = sd.sdkode left join m1_project p on woi.proyek = p.pkode left join m6_bom_in bomi on woi.idbomin = bomi.idbomin left join m6_bom bom2 on bomi.idbom = bom2.bomid left join m6_pdr_in pdri on woi.idpdrin = pdri.idpdrin left join m6_pdr pdr2 on pdri.idpdr = pdr2.pdrid"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("woid"), 0), sptField,
                     FxDB(drutama("wocabang"), ""), sptField,
                     FxDB(drutama("wolokasi"), ""), sptField,
                     FxDB(drutama("wogudangasal"), ""), sptField,
                     FxDB(drutama("wogudangproduksi"), ""), sptField,
                     FxDB(drutama("wogudangtujuan"), ""), sptField,
                     FxDB(drutama("wosumber"), ""), sptField,
                     FxDB(drutama("wojenis"), ""), sptField,
                     FxDB(drutama("woautonotransaksi"), 0), sptField,
                     FxDB(drutama("wonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("wotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("wokodepa"), 0), sptField,
                     FxDB(drutama("wodimintaoleh"), 0), sptField,
                     FxDB(drutama("wodimintaolehkontak"), ""), sptField,
                     FxDB(drutama("womintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("wotgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("woestimasikerja"), ""), sptField,
                     FxDB(drutama("womatauang"), ""), sptField,
                     FxDB(drutama("wokurs"), 0), sptField,
                     FxDB(drutama("wototalhargain"), 0), sptField,
                     FxDB(drutama("wototalhargaout"), 0), sptField,
                     FxDB(drutama("wototalhppin"), 0), sptField,
                     FxDB(drutama("wototalhppout"), 0), sptField,
                     FxDB(drutama("wouraian"), ""), sptField,
                     FxDB(drutama("wocatatan"), ""), sptField,
                     FxDB(drutama("wonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("wotglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("woidbom"), 0), sptField,
                     FxDB(drutama("woidpdr"), 0), sptField,
                     FxDB(drutama("wostatusmrsin"), 0), sptField,
                     FxDB(drutama("wostatusmrsout"), 0), sptField,
                     FxDB(drutama("wostatusmrnin"), 0), sptField,
                     FxDB(drutama("wostatusmrnout"), 0), sptField,
                     FxDB(drutama("wostatuspdin"), 0), sptField,
                     FxDB(drutama("wostatuspdout"), 0), sptField,
                     FxDB(drutama("wostatusrealisasiin"), 0), sptField,
                     FxDB(drutama("wostatusrealisasiout"), 0), sptField,
                     FxDB(drutama("wostatus"), 0), sptField,
                     FxDB(drutama("wostatussebelumnya"), 0), sptField,
                     FxDB(drutama("wojmlrevisi"), 0), sptField,
                     FxDB(drutama("wocetakanke"), 0), sptField,
                     FxDB(drutama("woinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("woinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("womodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("womodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("woposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("wopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("woisclose"), 0), sptField,
                     FxDB(drutama("wocustomtext1"), ""), sptField,
                     FxDB(drutama("wocustomtext2"), ""), sptField,
                     FxDB(drutama("wocustomtext3"), ""), sptField,
                     FxDB(drutama("wocustomtext4"), ""), sptField,
                     FxDB(drutama("wocustomtext5"), ""), sptField,
                     FxDB(drutama("wocustomint1"), 0), sptField,
                     FxDB(drutama("wocustomint2"), 0), sptField,
                     FxDB(drutama("wocustomint3"), 0), sptField,
                     FxDB(drutama("wocustomdbl1"), 0), sptField,
                     FxDB(drutama("wocustomdbl2"), 0), sptField,
                     FxDB(drutama("wocustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("wocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("wocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("wocustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("wocabangnama"), ""), sptField,
                     FxDB(drutama("wolokasinama"), ""), sptField,
                     FxDB(drutama("wogudangasalnama"), ""), sptField,
                     FxDB(drutama("wogudangproduksinama"), ""), sptField,
                     FxDB(drutama("wogudangtujuannama"), ""), sptField,
                     FxDB(drutama("wojenisnama"), ""), sptField,
                     FxDB(drutama("wodimintaolehkode"), ""), sptField,
                     FxDB(drutama("wodimintaolehnama"), ""), sptField,
                     FxDB(drutama("womintakekode"), ""), sptField,
                     FxDB(drutama("womintakenama"), ""), sptField,
                     FxDB(drutama("woestimasikerjanama"), ""), sptField,
                     FxDB(drutama("wonotransaksibom"), ""), sptField,
                     FxDB(drutama("wonotransaksipdr"), ""), sptField,
                     FxDB(drutama("wostatusnama"), ""), sptField,
                     FxDB(drutama("wostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("woinputusernama"), ""), sptField,
                     FxDB(drutama("womodifikasiusernama"), ""), sptField,
                     FxDB(drutama("woaktivitas"), 0), sptField,
                     FxDB(drutama("woaktivitaskode"), ""), sptField,
                     FxDB(drutama("woaktivitasnama"), ""), sptField,
                     FxDB(drutama("wojeniswajibwo"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idwoin"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
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
                     FxDB(dr("hpppersen"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
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
                     FxDB(dr("idbomin"), 0), sptField,
                     FxDB(dr("idpdrin"), 0), sptField,
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
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
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m6_wo_getdata_out")

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Wo_Pack", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailout = String.Concat(detailout,
                     FxDB(dr("idwoout"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
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
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
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
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
            Next
            'detailout = detailout.Substring(0, detailout.Length - sptRow.Length)
            If detailout.Length > 0 Then detailout = detailout.Substring(0, detailout.Length - sptRow.Length)

            'AMBIL DATA OUT
            'Dim queryactivity As New m0_query
            sql = "SELECT woa.*, pa.pakode AS kodeaktivitas, m.mnama AS namamesin FROM m6_wo_activity woa JOIN m6_wo wo ON woa.idwo = wo.woid JOIN m1_production_activity pa ON woa.idpa = pa.paid LEFT JOIN m1_machine m ON woa.kodemesin = m.mkode"

            Dim dtactivity As New DataTable
            dtactivity = AmbilData("aplikasi1-M6_Wo_Activity", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtactivity.Rows
                detailactivity = String.Concat(detailactivity,
                     FxDB(dr("idwoactivity"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
                     FxDB(dr("idpa"), 0), sptField,
                     FxDB(dr("namaaktivitas"), ""), sptField,
                     FxDB(dr("kodemesin"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodeaktivitas"), ""), sptField,
                     FxDB(dr("namamesin"), ""), sptRow)
            Next

            If detailactivity.Length > 0 Then detailactivity = detailactivity.Substring(0, detailactivity.Length - sptRow.Length)

            sql = "SELECT wrc.* FROM m6_wo_route_card wrc JOIN m6_wo wo ON wrc.idwo = wo.woid"

            Dim dtroutecard As New DataTable
            dtroutecard = AmbilData("aplikasi1-M6_Wo_Route_Card", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

            For Each dr As DataRow In dtroutecard.Rows
                detailroutecard = String.Concat(detailroutecard,
                     FxDB(dr("idworoutecard"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
            Next

            If detailroutecard.Length > 0 Then detailroutecard = detailroutecard.Substring(0, detailroutecard.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, detailout, sptSubParam, detailactivity, sptSubParam, detailroutecard)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocustomtext1, wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, wocustomint1, wocustomint2, wocustomint3, wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, wocustomdate2, wocustomdate3, wocabangnama, wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, woinputusernama, womodifikasiusernama, woaktivitas, woaktivitaskode, woaktivitasnama, wojeniswajibwo" & sptSubParam & "idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi" & sptSubParam & "idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi" & sptSubParam & "idwoactivity, idwo, idpa, namaaktivitas, kodemesin, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodeaktivitas, namamesin" & sptSubParam & "idworoutecard, idwo, notransaksi, jml, satuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_WoSearch(ByVal param As String) As String
        'M6_WoSearch --------------------------------------------------------
        'woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, 
        'wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, 
        'womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, 
        'wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, 
        'woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, 
        'wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, 
        'woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocabangnama, 
        'wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, 
        'womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, 
        'woinputusernama, womodifikasiusernama, woaktivitas, woaktivitaskode, woaktivitasnama

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
        'sql = query.PanggilQuery("m6_wo_v")
        sql = "select wo.woid AS woid, wo.wocabang AS wocabang, wo.wolokasi AS wolokasi, wo.wogudangasal AS wogudangasal, wo.wogudangproduksi AS wogudangproduksi, wo.wogudangtujuan AS wogudangtujuan, wo.wosumber AS wosumber, wo.wojenis AS wojenis, wo.woautonotransaksi AS woautonotransaksi, wo.wonotransaksi AS wonotransaksi, wo.wotgl AS wotgl, wo.wokodepa AS wokodepa, wo.wodimintaoleh AS wodimintaoleh, wo.wodimintaolehkontak AS wodimintaolehkontak, wo.womintake AS womintake, wo.wotgldipakai AS wotgldipakai, wo.woestimasikerja AS woestimasikerja, wo.womatauang AS womatauang, wo.wokurs AS wokurs, wo.wototalhargain AS wototalhargain, wo.wototalhargaout AS wototalhargaout, wo.wototalhppin AS wototalhppin, wo.wototalhppout AS wototalhppout, wo.wouraian AS wouraian, wo.wocatatan AS wocatatan, wo.wonoref AS wonoref, wo.wotglnoref AS wotglnoref, wo.woidbom AS woidbom, wo.woidpdr AS woidpdr, wo.wostatusmrsin AS wostatusmrsin, wo.wostatusmrsout AS wostatusmrsout, wo.wostatusmrnin AS wostatusmrnin, wo.wostatusmrnout AS wostatusmrnout, wo.wostatuspdin AS wostatuspdin, wo.wostatuspdout AS wostatuspdout, wo.wostatusrealisasiin AS wostatusrealisasiin, wo.wostatusrealisasiout AS wostatusrealisasiout, wo.wostatus AS wostatus, wo.wostatussebelumnya AS wostatussebelumnya, wo.wojmlrevisi AS wojmlrevisi, wo.wocetakanke AS wocetakanke, wo.woinputuser AS woinputuser, wo.woinputtgl AS woinputtgl, wo.womodifikasiuser AS womodifikasiuser, wo.womodifikasitgl AS womodifikasitgl, wo.woposting AS woposting, wo.wopostingtgl AS wopostingtgl, wo.woisclose AS woisclose, br.bnama AS wocabangnama, lc.lnama AS wolokasinama, wh1.wnama AS wogudangasalnama, wh2.wnama AS wogudangproduksinama, wh3.wnama AS wogudangtujuannama, pc.pcnama AS wojenisnama, c1.kkode AS wodimintaolehkode, c1.knama AS wodimintaolehnama, c2.kkode AS womintakekode, c2.knama AS womintakenama, we.wenama AS woestimasikerjanama, bom.bomnotransaksi AS wonotransaksibom, pdr.pdrnotransaksi AS wonotransaksipdr, st1.nama AS wostatusnama, st2.nama AS wostatussebelumnyanama, u1.unama AS woinputusernama, u2.unama AS womodifikasiusernama, wo.woaktivitas, pa.pakode as woaktivitaskode, pa.panama as woaktivitasnama from m6_wo wo left join m1_branch br on wo.wocabang = br.bkode left join m1_location lc on wo.wolokasi = lc.lkode left join m1_warehouse wh1 on wo.wogudangasal = wh1.wkode left join m1_warehouse wh2 on wo.wogudangproduksi = wh2.wkode left join m1_warehouse wh3 on wo.wogudangtujuan = wh3.wkode left join m1_production_category pc on wo.wojenis = pc.pckode left join m1_contact c1 on wo.wodimintaoleh = c1.kid left join m1_contact c2 on wo.womintake = c2.kid left join m1_working_estimate we on wo.woestimasikerja = we.wekode left join m6_bom bom on wo.woidbom = bom.bomid left join m6_pdr pdr on wo.woidpdr = pdr.pdrid left join m0_status st1 on wo.wostatus = st1.kode left join m0_status st2 on wo.wostatussebelumnya = st2.kode left join m0_user u1 on wo.woinputuser = u1.userid left join m0_user u2 on wo.womodifikasiuser = u2.userid left join m1_production_activity pa on wo.woaktivitas = pa.paid"

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("woid"), 0), sptField,
                     FxDB(dr("wocabang"), ""), sptField,
                     FxDB(dr("wolokasi"), ""), sptField,
                     FxDB(dr("wogudangasal"), ""), sptField,
                     FxDB(dr("wogudangproduksi"), ""), sptField,
                     FxDB(dr("wogudangtujuan"), ""), sptField,
                     FxDB(dr("wosumber"), ""), sptField,
                     FxDB(dr("wojenis"), ""), sptField,
                     FxDB(dr("woautonotransaksi"), 0), sptField,
                     FxDB(dr("wonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("wotgl"), ""), formatTgl), sptField,
                     FxDB(dr("wokodepa"), 0), sptField,
                     FxDB(dr("wodimintaoleh"), 0), sptField,
                     FxDB(dr("wodimintaolehkontak"), ""), sptField,
                     FxDB(dr("womintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("wotgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("woestimasikerja"), ""), sptField,
                     FxDB(dr("womatauang"), ""), sptField,
                     FxDB(dr("wokurs"), 0), sptField,
                     FxDB(dr("wototalhargain"), 0), sptField,
                     FxDB(dr("wototalhargaout"), 0), sptField,
                     FxDB(dr("wototalhppin"), 0), sptField,
                     FxDB(dr("wototalhppout"), 0), sptField,
                     FxDB(dr("wouraian"), ""), sptField,
                     FxDB(dr("wocatatan"), ""), sptField,
                     FxDB(dr("wonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("wotglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("woidbom"), 0), sptField,
                     FxDB(dr("woidpdr"), 0), sptField,
                     FxDB(dr("wostatusmrsin"), 0), sptField,
                     FxDB(dr("wostatusmrsout"), 0), sptField,
                     FxDB(dr("wostatusmrnin"), 0), sptField,
                     FxDB(dr("wostatusmrnout"), 0), sptField,
                     FxDB(dr("wostatuspdin"), 0), sptField,
                     FxDB(dr("wostatuspdout"), 0), sptField,
                     FxDB(dr("wostatusrealisasiin"), 0), sptField,
                     FxDB(dr("wostatusrealisasiout"), 0), sptField,
                     FxDB(dr("wostatus"), 0), sptField,
                     FxDB(dr("wostatussebelumnya"), 0), sptField,
                     FxDB(dr("wojmlrevisi"), 0), sptField,
                     FxDB(dr("wocetakanke"), 0), sptField,
                     FxDB(dr("woinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("woinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("womodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("womodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("woposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("wopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("woisclose"), 0), sptField,
                     FxDB(dr("wocabangnama"), ""), sptField,
                     FxDB(dr("wolokasinama"), ""), sptField,
                     FxDB(dr("wogudangasalnama"), ""), sptField,
                     FxDB(dr("wogudangproduksinama"), ""), sptField,
                     FxDB(dr("wogudangtujuannama"), ""), sptField,
                     FxDB(dr("wojenisnama"), ""), sptField,
                     FxDB(dr("wodimintaolehkode"), ""), sptField,
                     FxDB(dr("wodimintaolehnama"), ""), sptField,
                     FxDB(dr("womintakekode"), ""), sptField,
                     FxDB(dr("womintakenama"), ""), sptField,
                     FxDB(dr("woestimasikerjanama"), ""), sptField,
                     FxDB(dr("wonotransaksibom"), ""), sptField,
                     FxDB(dr("wonotransaksipdr"), ""), sptField,
                     FxDB(dr("wostatusnama"), ""), sptField,
                     FxDB(dr("wostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("woinputusernama"), ""), sptField,
                     FxDB(dr("womodifikasiusernama"), ""), sptField,
                     FxDB(dr("woaktivitas"), 0), sptField,
                     FxDB(dr("woaktivitaskode"), ""), sptField,
                     FxDB(dr("woaktivitasnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, wostatusrealisasiin, wostatusrealisasiout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, womodifikasitgl, woposting, wopostingtgl, woisclose, wocabangnama, wolokasinama, wogudangasalnama, wogudangproduksinama, wogudangtujuannama, wojenisnama, wodimintaolehkode, wodimintaolehnama, womintakekode, womintakenama, woestimasikerjanama, wonotransaksibom, wonotransaksipdr, wostatusnama, wostatussebelumnyanama, woinputusernama, womodifikasiusernama, woaktivitas, woaktivitaskode, woaktivitasnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_Wo_InSearch(ByVal param As String) As String
        'M6_Wo_InSearch --------------------------------------------------------
        'idwoin, idwo, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, 
        'subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, 
        'jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan

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
            Filter = Filter.Replace("idwo", "woi.idwo")
            Filter = Filter.Replace("statusrealisasi", "woi.statusrealisasi")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_wo_getdata_in")

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idwoin"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
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
                     FxDB(dr("hpppersen"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
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
                     FxDB(dr("idbomin"), 0), sptField,
                     FxDB(dr("idpdrin"), 0), sptField,
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
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
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_Wo_OutSearch(ByVal param As String) As String
        'M6_Wo_OutSearch --------------------------------------------------------
        'idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, 
        'proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, 
        'jmlsisarealisasi

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
            Filter = Filter.Replace("idwo", "woo.idwo")
            Filter = Filter.Replace("statusrealisasi", "woo.statusrealisasi")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m6_wo_getdata_out")

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idwoout"), 0), sptField,
                     FxDB(dr("idwo"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
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
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
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
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_WoTerkait(ByVal param As String) As String
        'M6_WoTerkait --------------------------------------------------------
        'woid, wonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "woid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND woid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "woid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m6_wo_terkait(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m5_bom_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each pl As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(pl("woid"), 0), sptField,
                     FxDB(pl("wonotransaksi"), ""), sptField,
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
            result(2) = "Related WO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("woid, wonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetailIn As DataTable, ByVal ftExistOutstandingBomIn As String, ByVal ftOutstandingBomIn As String, ByVal ftExistOutstandingPdrIn As String, ByVal ftOutstandingPdrIn As String, ByVal dtdetailOut As DataTable, ByVal ftExistOutstandingBomOut As String, ByVal ftOutstandingBomOut As String, ByVal ftExistOutstandingPdrOut As String, ByVal ftOutstandingPdrOut As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING BOM IN --------------------------------
        If Len(ftExistOutstandingBomIn) > 0 Then 'ftExistOutstanding = rowExists, idbomin, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingBomIn)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idbomin=" & dtval.Rows(0)("idbomin")
                dtLookup = AsDataTableFilterLimit(dtdetailIn, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail 1 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in BOM(result)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT bomin.idbomin, (bomin.jmlbarang) as sisarealisasi, i.bid, i.bkode FROM m6_bom_in AS bomin INNER JOIN m1_item AS i ON bomin.idbarang = i.bid WHERE " & ftOutstandingBomIn
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idbomin=" & dtval.Rows(0)("idbomin")
                dtLookup = AsDataTableFilterLimit(dtdetailIn, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail 1 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in BOM(result), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING BOM IN -------------------------


        'VALIDASI OUTSTANDING BOM OUT -------------------------------
        If Len(ftExistOutstandingBomOut) > 0 Then 'ftExistOutstanding = rowExists, idbomout, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingBomOut)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idbomout=" & dtval.Rows(0)("idbomout")
                dtLookup = AsDataTableFilterLimit(dtdetailOut, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail 2 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in BOM(material)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT bomout.idbomout, (bomout.jmlbarang) as sisarealisasi, i.bid, i.bkode FROM m6_bom_out AS bomout INNER JOIN m1_item AS i ON bomout.idbarang = i.bid WHERE " & ftOutstandingBomOut
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idbomout=" & dtval.Rows(0)("idbomout")
                dtLookup = AsDataTableFilterLimit(dtdetailOut, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail 2 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in BOM(material), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING BOM OUT ------------------------


        'VALIDASI OUTSTANDING PDR IN --------------------------------
        If Len(ftExistOutstandingPdrIn) > 0 Then 'ftExistOutstanding = rowExists, idpdrin, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPdrIn)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpdrin=" & dtval.Rows(0)("idpdrin")
                dtLookup = AsDataTableFilterLimit(dtdetailIn, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail 1 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PDR(result)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT pdrin.idpdrin, (pdrin.jmlbarang - pdrin.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m6_pdr_in AS pdrin INNER JOIN m1_item AS i ON pdrin.idbarang = i.bid WHERE " & ftOutstandingPdrIn
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idpdrin=" & dtval.Rows(0)("idpdrin")
                dtLookup = AsDataTableFilterLimit(dtdetailIn, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail 1 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PDR(result), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING PDR IN -------------------------


        'VALIDASI OUTSTANDING PDR OUT -------------------------------
        If Len(ftExistOutstandingPdrOut) > 0 Then 'ftExistOutstanding = rowExists, idpdrout, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPdrOut)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpdrout=" & dtval.Rows(0)("idpdrout")
                dtLookup = AsDataTableFilterLimit(dtdetailOut, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail 2 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PDR(material)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT pdrout.idpdrout, (pdrout.jmlbarang - pdrout.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m6_pdr_out AS pdrout INNER JOIN m1_item AS i ON pdrout.idbarang = i.bid WHERE " & ftOutstandingPdrOut
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idpdrout=" & dtval.Rows(0)("idpdrout")
                dtLookup = AsDataTableFilterLimit(dtdetailOut, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail 2 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PDR(material), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING PDR OUT ------------------------


selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M6_WoSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataDetail2(), dataRowDetail2() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'woid(0) As Integer, wocabang(1) As String, wolokasi(2) As String, wogudangasal(3) As String, wogudangproduksi(4) As String, 
        'wogudangtujuan(5) As String, wosumber(6) As String, wojenis(7) As String, woautonotransaksi(8) As Integer, wonotransaksi(9) As String, 
        'wotgl(10) As Date, wokodepa(11) As Integer, wodimintaoleh(12) As Integer, wodimintaolehkontak(13) As String, womintake(14) As Integer, 
        'wotgldipakai(15) As Date, woestimasikerja(16) As String, womatauang(17) As String, wokurs(18) As Double, wototalhargain(19) As Double, 
        'wototalhargaout(20) As Double, wototalhppin(21) As Double, wototalhppout(22) As Double, wouraian(23) As String, wocatatan(24) As String, 
        'wonoref(25) As String, wotglnoref(26) As Date, woidbom(27) As Integer, woidpdr(28) As Integer, wostatusmrsin(29) As Integer, 
        'wostatusmrsout(30) As Integer, wostatusmrnin(31) As Integer, wostatusmrnout(32) As Integer, wostatuspdin(33) As Integer, wostatuspdout(34) As Integer, 
        'wostatus(35) As Integer, wostatussebelumnya(36) As Integer, wojmlrevisi(37) As Integer, wocetakanke(38) As Integer, woinputuser(39) As Integer, 
        'woinputtgl(40) As DateTime, womodifikasiuser(41) As Integer, womodifikasitgl(42) As DateTime, woisclose(43) As Integer, wocustomtext1(44) As String, 
        'wocustomtext2(45) As String, wocustomtext3(46) As String, wocustomtext4(47) As String, wocustomtext5(48) As String, wocustomint1(49) As Integer, 
        'wocustomint2(50) As Integer, wocustomint3(51) As Integer, wocustomdbl1(52) As Double, wocustomdbl2(53) As Double, wocustomdbl3(54) As Double, 
        'wocustomdate1(55) As Date, wocustomdate2(56) As Date, wocustomdate3(57) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, 
        'wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, 
        'womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, 
        'wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, 
        'woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, 
        'wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, 
        'womodifikasitgl, woisclose, wocustomtext1, wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, 
        'wocustomint1, wocustomint2, wocustomint3, wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, 
        'wocustomdate2, wocustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 58) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'woid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "woid required numeric." : GoTo selesai
        End If
        'woautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "woautonotransaksi required numeric." : GoTo selesai
        End If
        'wotgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "wotgl required date." : GoTo selesai
        End If
        'wokodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "wokodepa required numeric." : GoTo selesai
        End If
        'wodimintaoleh(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "wodimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "wodimintaoleh can't be empty." : GoTo selesai
        End If
        'womintake(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "womintake required numeric." : GoTo selesai
        End If
        'wotgldipakai(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "wotgldipakai required date." : GoTo selesai
        End If
        'wokurs(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "wokurs required numeric." : GoTo selesai
        End If
        'wototalhargain(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "wototalhargain required numeric." : GoTo selesai
        End If
        'wototalhargaout(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "wototalhargaout required numeric." : GoTo selesai
        End If
        'wototalhppin(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "wototalhppin required numeric." : GoTo selesai
        End If
        'wototalhppout(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "wototalhppout required numeric." : GoTo selesai
        End If
        'wotglnoref(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "wotglnoref required date." : GoTo selesai
        End If
        'woidbom(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "woidbom required numeric." : GoTo selesai
        End If
        'woidpdr(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "woidpdr required numeric." : GoTo selesai
        End If
        'wostatusmrsin(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "wostatusmrsin required numeric." : GoTo selesai
        End If
        'wostatusmrsout(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "wostatusmrsout required numeric." : GoTo selesai
        End If
        'wostatusmrnin(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "wostatusmrnin required numeric." : GoTo selesai
        End If
        'wostatusmrnout(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "wostatusmrnout required numeric." : GoTo selesai
        End If
        'wostatuspdin(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "wostatuspdin required numeric." : GoTo selesai
        End If
        'wostatuspdout(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "wostatuspdout required numeric." : GoTo selesai
        End If
        'wostatus(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "wostatus required numeric." : GoTo selesai
        End If
        'wostatussebelumnya(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "wostatussebelumnya required numeric." : GoTo selesai
        End If
        'wojmlrevisi(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "wojmlrevisi required numeric." : GoTo selesai
        End If
        'wocetakanke(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "wocetakanke required numeric." : GoTo selesai
        End If
        'woinputuser(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "woinputuser required numeric." : GoTo selesai
        End If
        'woinputtgl(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "woinputtgl required date." : GoTo selesai
        End If
        'womodifikasiuser(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "womodifikasiuser required numeric." : GoTo selesai
        End If
        'womodifikasitgl(42) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "womodifikasitgl required date." : GoTo selesai
        End If
        'woisclose(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "woisclose required numeric." : GoTo selesai
        End If
        'wocustomint1(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "wocustomint1 required numeric." : GoTo selesai
        End If
        'wocustomint2(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "wocustomint2 required numeric." : GoTo selesai
        End If
        'wocustomint3(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "wocustomint3 required numeric." : GoTo selesai
        End If
        'wocustomdbl1(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "wocustomdbl1 required numeric." : GoTo selesai
        End If
        'wocustomdbl2(53) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "wocustomdbl2 required numeric." : GoTo selesai
        End If
        'wocustomdbl3(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "wocustomdbl3 required numeric." : GoTo selesai
        End If
        'wocustomdate1(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "wocustomdate1 required date." : GoTo selesai
        End If
        'wocustomdate2(56) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "wocustomdate2 required date." : GoTo selesai
        End If
        'wocustomdate3(57) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "wocustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'wocabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "wocabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "wocabang should not be more than 25 character." : GoTo selesai
        End If

        'wolokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "wolokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "wolokasi should not be more than 25 character." : GoTo selesai
        End If

        'wogudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "wogudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "wogudangasal should not be more than 25 character." : GoTo selesai
        End If

        'wogudangproduksi(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "wogudangproduksi can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "wogudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'wogudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "wogudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "wogudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'wosumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "wosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "wosumber should not be more than 10 character." : GoTo selesai
        End If

        'wojenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "wojenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "wojenis should not be more than 25 character." : GoTo selesai
        End If

        'wonotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "wonotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "wonotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'wotgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "wotgl can't be empty" : GoTo selesai
        End If

        'wotgldipakai(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "wotgldipakai can't be empty" : GoTo selesai
        End If

        'womatauang(17) As String
        If Len(dataUtama(17)) = 0 Then
            result(2) = "womatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(17)) > 25 Then
            result(2) = "womatauang should not be more than 25 character." : GoTo selesai
        End If

        'wokurs(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "wokurs can't be empty" : GoTo selesai
        End If

        'wototalhargain(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "wototalhargain can't be empty" : GoTo selesai
        End If

        'wototalhargaout(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "wototalhargaout can't be empty" : GoTo selesai
        End If

        'wototalhppin(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "wototalhppin can't be empty" : GoTo selesai
        End If

        'wototalhppout(22) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = "wototalhppout can't be empty" : GoTo selesai
        End If

        'wotglnoref(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "wotglnoref can't be empty" : GoTo selesai
        End If

        'woinputtgl(40) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "woinputtgl can't be empty" : GoTo selesai
        End If

        'womodifikasitgl(42) As DateTime
        If Len(dataUtama(42)) = 0 Then
            result(2) = "womodifikasitgl can't be empty" : GoTo selesai
        End If

        'wocustomdbl1(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "wocustomdbl1 can't be empty" : GoTo selesai
        End If

        'wocustomdbl2(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "wocustomdbl2 can't be empty" : GoTo selesai
        End If

        'wocustomdbl3(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "wocustomdbl3 can't be empty" : GoTo selesai
        End If

        'wocustomdate1(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "wocustomdate1 can't be empty" : GoTo selesai
        End If

        'wocustomdate2(56) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "wocustomdate2 can't be empty" : GoTo selesai
        End If

        'wocustomdate3(57) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "wocustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "woid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wolokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wogudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wogudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wogudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wojenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wodimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wodimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "womintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wotgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "womatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wototalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wouraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woidbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "woidpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrsin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrsout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrnin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatusmrnout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatuspdin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatuspdout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "woinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "woinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "womodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "womodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "woisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "wocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "wocustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "woid~wocabang~wolokasi~wogudangasal~wogudangproduksi~wogudangtujuan~wosumber~wojenis~woautonotransaksi~wonotransaksi~wotgl~wokodepa~wodimintaoleh~wodimintaolehkontak~womintake~wotgldipakai~woestimasikerja~womatauang~wokurs~wototalhargain~wototalhargaout~wototalhppin~wototalhppout~wouraian~wocatatan~wonoref~wotglnoref~woidbom~woidpdr~wostatusmrsin~wostatusmrsout~wostatusmrnin~wostatusmrnout~wostatuspdin~wostatuspdout~wostatus~wostatussebelumnya~wojmlrevisi~wocetakanke~woinputuser~woinputtgl~womodifikasiuser~womodifikasitgl~woisclose~wocustomtext1~wocustomtext2~wocustomtext3~wocustomtext4~wocustomtext5~wocustomint1~wocustomint2~wocustomint3~wocustomdbl1~wocustomdbl2~wocustomdbl3~wocustomdate1~wocustomdate2~wocustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL1 -------------------------------------------------------
        'idwoin(0) As Integer, idwo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, idbomin(27) As Integer, idpdrin(28) As Integer, jmlmrs(29) As Double, 
        'statusmrs(30) As Integer, jmlmrn(31) As Double, statusmrn(32) As Integer, jmlpd(33) As Double, statuspd(34) As Integer, 
        'isclose(35) As Integer, customtext1(36) As String, customtext2(37) As String, customtext3(38) As String, customdbl1(39) As Double, 
        'customdbl2(40) As Double, customdbl3(41) As Double, customdate1(42) As Date, customdate2(43) As Date, customdate3(44) As Date

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, 
        'idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable DETAIL1
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idwoin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idwo", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "hpppersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpdrin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrn", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, idbomin As Integer = 0, idbomout As Integer = 0, idpdrin As Integer = 0, idpdrout As Integer = 0

        Dim ftExistOutstandingBomIn As String = "", ftOutstandingBomIn As String = ""
        Dim ftExistOutstandingBomOut As String = "", ftOutstandingBomOut As String = ""

        Dim ftExistOutstandingPdrIn As String = "", ftOutstandingPdrIn As String = ""
        Dim updNilaiPdrIn As String = "", updFilterPdrIn As String = ""

        Dim ftExistOutstandingPdrOut As String = "", ftOutstandingPdrOut As String = ""
        Dim updNilaiPdrOut As String = "", updFilterPdrOut As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL1 ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL1 -----------------------------------
            'CEK ARRAY DATA DETAIL1
            If (dataRowDetail.Length <> 45) Then
                result(2) = "Detail 1 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL1 ----------------------------

            'VALIDASI TIPE DATA DETAIL1 ------------------------------------------
            'idwoin(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idwoin required numeric." : GoTo selesai
            End If
            'idwo(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idwo required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpppersen(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idbomin(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbomin required numeric." : GoTo selesai
            End If
            'idpdrin(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idpdrin required numeric." : GoTo selesai
            End If
            'jmlmrs(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
            End If
            'statusmrs(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrs required numeric." : GoTo selesai
            End If
            'jmlmrn(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
            End If
            'statusmrn(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrn required numeric." : GoTo selesai
            End If
            'jmlpd(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(42) As Date
            If (IsDate(dataRowDetail(42)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(43) As Date
            If (IsDate(dataRowDetail(43)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(44) As Date
            If (IsDate(dataRowDetail(44)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL1 -----------------------------------

            'VALIDASI DATA DETAIL1 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Detail 1 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpppersen(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(15) As String
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(18) As String
            'If Len(dataRowDetail(18)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(18)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(19) As String
            'If Len(dataRowDetail(19)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(19)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(20) As String
            'If Len(dataRowDetail(20)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmlmrs(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
            End If

            'jmlmrn(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
            End If

            'jmlpd(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(42) As Date
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(43) As Date
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(44) As Date
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL1 --------------------------------

            If AsDataTableTambahData(dtdetail, "idwoin~idwo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomin~idpdrin~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44)) = False Then
                result(2) = "Detail 1 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , idbomin(27) As Integer      , idpdrin(28) As Integer
            idbarang = dataRowDetail(2) : idbomin = dataRowDetail(27) : idpdrin = dataRowDetail(28)

            'VALIDASI OUTSTANDING -------------------------
            ''BOM
            'If idbomin <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingBomIn = IIf(Len(ftExistOutstandingBomIn.ToString) = 0, "", ftExistOutstandingBomIn & " UNION ")
            '    ftExistOutstandingBomIn = String.Concat(ftExistOutstandingBomIn, "SELECT EXISTS(SELECT 1 FROM m6_bom_in JOIN m6_bom ON idbom = bomid WHERE idbomin = '" & idbomin & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomin & "' as idbomin, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
            '    '2. CEK JML OUTSTANDING
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbomin=" & idbomin)
            '    ftOutstandingBomIn = IIf(Len(ftOutstandingBomIn.ToString) = 0, "", ftOutstandingBomIn & " OR ")
            '    ftOutstandingBomIn = String.Concat(ftOutstandingBomIn, " (bomin.idbomin = " & idbomin & " AND " & Outstanding & " > bomin.jmlbarang) ")
            'End If

            'PDR
            If idpdrin <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstandingPdrIn = IIf(Len(ftExistOutstandingPdrIn.ToString) = 0, "", ftExistOutstandingPdrIn & " UNION ")
                ftExistOutstandingPdrIn = String.Concat(ftExistOutstandingPdrIn, "SELECT EXISTS(SELECT 1 FROM m6_pdr_in JOIN m6_pdr ON idpdr = pdrid WHERE idpdrin = '" & idpdrin & "' AND (pdrstatus = 2 OR pdrstatus = 3 OR pdrstatus = 4 OR pdrstatus = 7) LIMIT 1) as rowExists, '" & idpdrin & "' as idpdrin, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpdrin=" & idpdrin)
                ftOutstandingPdrIn = IIf(Len(ftOutstandingPdrIn.ToString) = 0, "", ftOutstandingPdrIn & " OR ")
                ftOutstandingPdrIn = String.Concat(ftOutstandingPdrIn, " (pdrin.idpdrin = " & idpdrin & " AND " & Outstanding & " > (pdrin.jmlbarang - pdrin.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiPdrIn = String.Concat("WHEN '" & idpdrin & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPdrIn)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterPdrIn = IIf(Len(updFilterPdrIn.ToString) = 0, "", updFilterPdrIn & " OR ")
                updFilterPdrIn = String.Concat(updFilterPdrIn, "(idpdrin = '" & idpdrin & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL1 ===========================================


        'MAPPING BUAT WS DATA DETAIL2 -------------------------------------------------------
        'idwoout(0) As Integer, idwo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idbomout(28) As Integer, idpdrout(29) As Integer, 
        'jmlmrs(30) As Double, statusmrs(31) As Integer, jmlmrn(32) As Double, statusmrn(33) As Integer, jmlpd(34) As Double, 
        'statuspd(35) As Integer, isclose(36) As Integer, customtext1(37) As String, customtext2(38) As String, customtext3(39) As String, 
        'customdbl1(40) As Double, customdbl2(41) As Double, customdbl3(42) As Double, customdate1(43) As Date, customdate2(44) As Date, 
        'customdate3(45) As Date

        'MAPPING BUAT FLEX DATA DETAIL2 -----------------------------------------------------
        'idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL2 ======================================================
        'SPLIT PARAMETER DATA DETAIL2
        dataDetail2 = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL2 ===============================================

        'Buat datatable DETAIL2
        Dim dtdetail2 As New DataTable
        AsDataTableTambahField(dtdetail2, "idwoout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail2, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbomout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idpdrout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statuspd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL2 ==================================================
        Dim JmlDtDetail2 As Integer = dataDetail2.Length
        For i = 1 To JmlDtDetail2
            'SPLIT DATA DETAIL
            dataRowDetail2 = dataDetail2(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL2 -----------------------------------
            'CEK ARRAY DATA DETAIL2
            If (dataRowDetail2.Length <> 46) Then
                result(2) = "Detail 2 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL2 ----------------------------

            'VALIDASI TIPE DATA DETAIL2 ------------------------------------------
            'idwoout(0) As Integer
            If (IsNumeric(dataRowDetail2(0)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idwoout required numeric." : GoTo selesai
            End If
            'idwo(1) As Integer
            If (IsNumeric(dataRowDetail2(1)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idwo required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail2(2)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail2(5)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail2(7)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail2(8) = Double.Parse(dataRowDetail2(5)) * Double.Parse(dataRowDetail2(7))
            If (IsNumeric(dataRowDetail2(8)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail2(11)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail2(12)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(13) As Double
            If (IsNumeric(dataRowDetail2(13)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(14) As Integer
            If (IsNumeric(dataRowDetail2(14)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(15) As Integer
            If (IsNumeric(dataRowDetail2(15)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail2(27)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idbomout(28) As Integer
            If (IsNumeric(dataRowDetail2(28)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbomout required numeric." : GoTo selesai
            End If
            'idpdrout(29) As Integer
            If (IsNumeric(dataRowDetail2(29)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idpdrout required numeric." : GoTo selesai
            End If
            'jmlmrs(30) As Double
            If (IsNumeric(dataRowDetail2(30)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
            End If
            'statusmrs(31) As Integer
            If (IsNumeric(dataRowDetail2(31)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - statusmrs required numeric." : GoTo selesai
            End If
            'jmlmrn(32) As Double
            If (IsNumeric(dataRowDetail2(32)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
            End If
            'statusmrn(33) As Integer
            If (IsNumeric(dataRowDetail2(33)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - statusmrn required numeric." : GoTo selesai
            End If
            'jmlpd(34) As Double
            If (IsNumeric(dataRowDetail2(34)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(35) As Integer
            If (IsNumeric(dataRowDetail2(35)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(36) As Integer
            If (IsNumeric(dataRowDetail2(36)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(40) As Double
            If (IsNumeric(dataRowDetail2(40)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(41) As Double
            If (IsNumeric(dataRowDetail2(41)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(42) As Double
            If (IsNumeric(dataRowDetail2(42)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(43) As Date
            If (IsDate(dataRowDetail2(43)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(44) As Date
            If (IsDate(dataRowDetail2(44)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(45) As Date
            If (IsDate(dataRowDetail2(45)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL2 -----------------------------------

            'VALIDASI DATA DETAIL2 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail2(3)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(3)) > 100 Then
                result(2) = "Detail 2 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowDetail2(5)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail2(5) <= 0 Then
                result(2) = "Detail 2 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail2(6)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(6)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail2(7)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail2(8)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail2(8) <= 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail2(9)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(9)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail2(11)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail2(12)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(13) As Double
            If Len(dataRowDetail2(13)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(16) As String
            If Len(dataRowDetail2(16)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(16)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(19) As String
            'If Len(dataRowDetail2(19)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(19)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(20) As String
            'If Len(dataRowDetail2(20)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(20)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(21) As String
            'If Len(dataRowDetail2(21)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(21)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmlmrs(30) As Double
            If Len(dataRowDetail2(30)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
            End If

            'jmlmrn(32) As Double
            If Len(dataRowDetail2(32)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
            End If

            'jmlpd(34) As Double
            If Len(dataRowDetail2(34)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(40) As Double
            If Len(dataRowDetail2(40)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(41) As Double
            If Len(dataRowDetail2(41)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(42) As Double
            If Len(dataRowDetail2(42)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(43) As Date
            If Len(dataRowDetail2(43)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(44) As Date
            If Len(dataRowDetail2(44)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(45) As Date
            If Len(dataRowDetail2(45)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL2 --------------------------------

            If AsDataTableTambahData(dtdetail2, "idwoout~idwo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususmasuk~idhppfifomasuk~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomout~idpdrout~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail2(0) & "~" & dataRowDetail2(1) & "~" & dataRowDetail2(2) & "~" & dataRowDetail2(3) & "~" & dataRowDetail2(4) & "~" & dataRowDetail2(5) & "~" & dataRowDetail2(6) & "~" & dataRowDetail2(7) & "~" & dataRowDetail2(8) & "~" & dataRowDetail2(9) & "~" & dataRowDetail2(10) & "~" & dataRowDetail2(11) & "~" & dataRowDetail2(12) & "~" & dataRowDetail2(13) & "~" & dataRowDetail2(14) & "~" & dataRowDetail2(15) & "~" & dataRowDetail2(16) & "~" & dataRowDetail2(17) & "~" & dataRowDetail2(18) & "~" & dataRowDetail2(19) & "~" & dataRowDetail2(20) & "~" & dataRowDetail2(21) & "~" & dataRowDetail2(22) & "~" & dataRowDetail2(23) & "~" & dataRowDetail2(24) & "~" & dataRowDetail2(25) & "~" & dataRowDetail2(26) & "~" & dataRowDetail2(27) & "~" & dataRowDetail2(28) & "~" & dataRowDetail2(29) & "~" & dataRowDetail2(30) & "~" & dataRowDetail2(31) & "~" & dataRowDetail2(32) & "~" & dataRowDetail2(33) & "~" & dataRowDetail2(34) & "~" & dataRowDetail2(35) & "~" & dataRowDetail2(36) & "~" & dataRowDetail2(37) & "~" & dataRowDetail2(38) & "~" & dataRowDetail2(39) & "~" & dataRowDetail2(40) & "~" & dataRowDetail2(41) & "~" & dataRowDetail2(42) & "~" & dataRowDetail2(43) & "~" & dataRowDetail2(44) & "~" & dataRowDetail2(45)) = False Then
                result(2) = "Detail 2 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer      , idbomout(28) As Integer       , idpdrout(29) As Integer
            idbarang = dataRowDetail2(2) : idbomout = dataRowDetail2(28) : idpdrout = dataRowDetail2(29)

            'VALIDASI OUTSTANDING -------------------------
            ''BOM
            'If idbomout <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingBomOut = IIf(Len(ftExistOutstandingBomOut.ToString) = 0, "", ftExistOutstandingBomOut & " UNION ")
            '    ftExistOutstandingBomOut = String.Concat(ftExistOutstandingBomOut, "SELECT EXISTS(SELECT 1 FROM m6_bom_out JOIN m6_bom ON idbom = bomid WHERE idbomout = '" & idbomout & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomout & "' as idbomout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
            '    '2. CEK JML OUTSTANDING
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail2, "jmlbarang", "idbomout=" & idbomout)
            '    ftOutstandingBomOut = IIf(Len(ftOutstandingBomOut.ToString) = 0, "", ftOutstandingBomOut & " OR ")
            '    ftOutstandingBomOut = String.Concat(ftOutstandingBomOut, " (bomout.idbomout = " & idbomout & " AND " & Outstanding & " > bomout.jmlbarang) ")
            'End If

            'PDR
            If idpdrout <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstandingPdrOut = IIf(Len(ftExistOutstandingPdrOut.ToString) = 0, "", ftExistOutstandingPdrOut & " UNION ")
                ftExistOutstandingPdrOut = String.Concat(ftExistOutstandingPdrOut, "SELECT EXISTS(SELECT 1 FROM m6_pdr_out JOIN m6_pdr ON idpdr = pdrid WHERE idpdrout = '" & idpdrout & "' AND (pdrstatus = 2 OR pdrstatus = 3 OR pdrstatus = 4 OR pdrstatus = 7) LIMIT 1) as rowExists, '" & idpdrout & "' as idpdrout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail2, "jmlbarang", "idpdrout=" & idpdrout)
                ftOutstandingPdrOut = IIf(Len(ftOutstandingPdrOut.ToString) = 0, "", ftOutstandingPdrOut & " OR ")
                ftOutstandingPdrOut = String.Concat(ftOutstandingPdrOut, " (pdrout.idpdrout = " & idpdrout & " AND " & Outstanding & " > (pdrout.jmlbarang - pdrout.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiPdrOut = String.Concat("WHEN '" & idpdrout & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPdrOut)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterPdrOut = IIf(Len(updFilterPdrOut.ToString) = 0, "", updFilterPdrOut & " OR ")
                updFilterPdrOut = String.Concat(updFilterPdrOut, "(idpdrout = '" & idpdrout & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL2 ===========================================


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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("wotgl")), AsFormatTanggal(drutama("wotgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("wostatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingBomIn, ftOutstandingBomIn, ftExistOutstandingPdrIn, ftOutstandingPdrIn, dtdetail2, ftExistOutstandingBomOut, ftOutstandingBomOut, ftExistOutstandingPdrOut, ftOutstandingPdrOut)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("woid")
                    notransaksi = drutama("wonotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(woid), wonotransaksi FROM M6_wo WHERE woid='" & result(4) & "' AND wostatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(woid) FROM M6_wo WHERE wonotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_wo_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Wo_HistorySimpan("" & paramSplit(0) & "★M6_Wo_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("wosumber")) & "▼" & FixQuotes(drutama("woid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Wo set wocabang  = '" & FixQuotes(drutama("wocabang")) & "', wolokasi  = '" & FixQuotes(drutama("wolokasi")) & "', wogudangasal  = '" & FixQuotes(drutama("wogudangasal")) & "', wogudangproduksi  = '" & FixQuotes(drutama("wogudangproduksi")) & "', wogudangtujuan  = '" & FixQuotes(drutama("wogudangtujuan")) & "', wosumber  = '" & FixQuotes(drutama("wosumber")) & "', wojenis  = '" & FixQuotes(drutama("wojenis")) & "', woautonotransaksi  = " & drutama("woautonotransaksi") & ", wonotransaksi  = '" & FixQuotes(notransaksi) & "', wotgl  = '" & FixQuotes(AsFormatTanggal(drutama("wotgl"))) & "', wokodepa  = " & drutama("wokodepa") & ", wodimintaoleh  = " & drutama("wodimintaoleh") & ", wodimintaolehkontak  = '" & FixQuotes(drutama("wodimintaolehkontak")) & "', womintake  = " & drutama("womintake") & ", wotgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("wotgldipakai"))) & "', woestimasikerja  = '" & FixQuotes(drutama("woestimasikerja")) & "', womatauang  = '" & FixQuotes(drutama("womatauang")) & "', wokurs  = '" & FixDouble(drutama("wokurs")) & "', wototalhargain  = '" & FixDouble(drutama("wototalhargain")) & "', wototalhargaout  = '" & FixDouble(drutama("wototalhargaout")) & "', wototalhppin  = '" & FixDouble(drutama("wototalhppin")) & "', wototalhppout  = '" & FixDouble(drutama("wototalhppout")) & "', wouraian  = '" & FixQuotes(drutama("wouraian")) & "', wocatatan  = '" & FixQuotes(drutama("wocatatan")) & "', wonoref  = '" & FixQuotes(drutama("wonoref")) & "', wotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("wotglnoref"))) & "', woidbom  = " & drutama("woidbom") & ", woidpdr  = " & drutama("woidpdr") & ", wostatusmrsin  = " & drutama("wostatusmrsin") & ", wostatusmrsout  = " & drutama("wostatusmrsout") & ", wostatusmrnin  = " & drutama("wostatusmrnin") & ", wostatusmrnout  = " & drutama("wostatusmrnout") & ", wostatuspdin  = " & drutama("wostatuspdin") & ", wostatuspdout  = " & drutama("wostatuspdout") & ", wostatus  = " & drutama("wostatus") & ", wostatussebelumnya  = " & drutama("wostatussebelumnya") & ", wojmlrevisi  = wojmlrevisi+1, wocetakanke  = " & drutama("wocetakanke") & ", womodifikasiuser  = " & drutama("womodifikasiuser") & ", womodifikasitgl  = NOW(), wocustomtext1  = '" & FixQuotes(drutama("wocustomtext1")) & "', wocustomtext2  = '" & FixQuotes(drutama("wocustomtext2")) & "', wocustomtext3  = '" & FixQuotes(drutama("wocustomtext3")) & "', wocustomtext4  = '" & FixQuotes(drutama("wocustomtext4")) & "', wocustomtext5  = '" & FixQuotes(drutama("wocustomtext5")) & "', wocustomint1  = " & drutama("wocustomint1") & ", wocustomint2  = " & drutama("wocustomint2") & ", wocustomint3  = " & drutama("wocustomint3") & ", wocustomdbl1  = '" & FixDouble(drutama("wocustomdbl1")) & "', wocustomdbl2  = '" & FixDouble(drutama("wocustomdbl2")) & "', wocustomdbl3  = '" & FixDouble(drutama("wocustomdbl3")) & "', wocustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate1"))) & "', wocustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate2"))) & "', wocustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate3"))) & "' where woid = '" & drutama("woid") & "'"
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

                    If drutama("woautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("wocabang"), drutama("wolokasi"), drutama("wosumber"), drutama("wotgl"))
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
                        notransaksi = drutama("wonotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(woid) FROM m6_wo WHERE wonotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Wo (wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, wokodepa, wodimintaoleh, wodimintaolehkontak, womintake, wotgldipakai, woestimasikerja, womatauang, wokurs, wototalhargain, wototalhargaout, wototalhppin, wototalhppout, wouraian, wocatatan, wonoref, wotglnoref, woidbom, woidpdr, wostatusmrsin, wostatusmrsout, wostatusmrnin, wostatusmrnout, wostatuspdin, wostatuspdout, wostatus, wostatussebelumnya, wojmlrevisi, wocetakanke, woinputuser, woinputtgl, womodifikasiuser, womodifikasitgl, woisclose, wocustomtext1, wocustomtext2, wocustomtext3, wocustomtext4, wocustomtext5, wocustomint1, wocustomint2, wocustomint3, wocustomdbl1, wocustomdbl2, wocustomdbl3, wocustomdate1, wocustomdate2, wocustomdate3) values('" & FixQuotes(drutama("wocabang")) & "', '" & FixQuotes(drutama("wolokasi")) & "', '" & FixQuotes(drutama("wogudangasal")) & "', '" & FixQuotes(drutama("wogudangproduksi")) & "', '" & FixQuotes(drutama("wogudangtujuan")) & "', '" & FixQuotes(drutama("wosumber")) & "', '" & FixQuotes(drutama("wojenis")) & "', " & drutama("woautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("wotgl"))) & "', " & drutama("wokodepa") & ", " & drutama("wodimintaoleh") & ", '" & FixQuotes(drutama("wodimintaolehkontak")) & "', " & drutama("womintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("wotgldipakai"))) & "', '" & FixQuotes(drutama("woestimasikerja")) & "', '" & FixQuotes(drutama("womatauang")) & "', '" & FixDouble(drutama("wokurs")) & "', '" & FixDouble(drutama("wototalhargain")) & "', '" & FixDouble(drutama("wototalhargaout")) & "', '" & FixDouble(drutama("wototalhppin")) & "', '" & FixDouble(drutama("wototalhppout")) & "', '" & FixQuotes(drutama("wouraian")) & "', '" & FixQuotes(drutama("wocatatan")) & "', '" & FixQuotes(drutama("wonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("wotglnoref"))) & "', " & drutama("woidbom") & ", " & drutama("woidpdr") & ", " & drutama("wostatusmrsin") & ", " & drutama("wostatusmrsout") & ", " & drutama("wostatusmrnin") & ", " & drutama("wostatusmrnout") & ", " & drutama("wostatuspdin") & ", " & drutama("wostatuspdout") & ", " & drutama("wostatus") & ", " & drutama("wostatussebelumnya") & ", " & drutama("wojmlrevisi") & ", " & drutama("wocetakanke") & ", " & drutama("woinputuser") & ", NOW(), " & drutama("womodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("woisclose") & ", '" & FixQuotes(drutama("wocustomtext1")) & "', '" & FixQuotes(drutama("wocustomtext2")) & "', '" & FixQuotes(drutama("wocustomtext3")) & "', '" & FixQuotes(drutama("wocustomtext4")) & "', '" & FixQuotes(drutama("wocustomtext5")) & "', " & drutama("wocustomint1") & ", " & drutama("wocustomint2") & ", " & drutama("wocustomint3") & ", '" & FixDouble(drutama("wocustomdbl1")) & "', '" & FixDouble(drutama("wocustomdbl2")) & "', '" & FixDouble(drutama("wocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("wocustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select woid from M6_wo where wonotransaksi='" & notransaksi & "' AND woinputuser= '" & userid & "' order by womodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Wo_In where idwo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail1
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idwoin") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomin") & ", " & dr1("idpdrin") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Wo_In(idwoin, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail In Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail2 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Wo_Out where idwo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail2
                If (dtdetail2.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail2.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idwoout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomout") & ", " & dr1("idpdrout") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Wo_Out(idwoout, idwo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, idpdrout, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Out Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                If drutama("wostatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ===================================================
                    Dim updNilaiPdrUtamaIn = "", updNilaiPdrUtamaOut = "", updFilterPdrUtama = ""

                    'PDR IN
                    If Len(updNilaiPdrIn) > 0 Then
                        'UPDATE DETAIL IN
                        sql = "UPDATE m6_pdr_in SET jmlrealisasi = (CASE idpdrin " & updNilaiPdrIn & " ELSE jmlrealisasi END) WHERE " & updFilterPdrIn
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'AMBIL ID UTAMA IN
                        Dim ftDetail As String = ""
                        Dim dtIn As DataTable = AsDataTableAmbilDariDB("SELECT idpdr FROM m6_pdr_in WHERE " & updFilterPdrIn & " GROUP BY idpdr")
                        If dtIn.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtIn.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                            Next
                        End If

                        'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                        If Len(ftDetail) > 0 Then
                            dtIn = AsDataTableAmbilDariDB("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_in WHERE " & ftDetail & " GROUP BY idpdr")
                            If dtIn.Rows.Count > 0 Then
                                For Each dr1 As DataRow In dtIn.Rows
                                    '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                    Dim statusIn As Integer = 0
                                    If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                        statusIn = 2
                                    ElseIf dr1("jmlrealisasi") < 1 Then
                                        statusIn = 0
                                    Else
                                        statusIn = 1
                                    End If
                                    '2. SET NILAI UPDATE OUTSTANDING
                                    updNilaiPdrUtamaIn = String.Concat(updNilaiPdrUtamaIn, "WHEN '" & dr1("idpdr") & "' THEN '" & statusIn & "' ")
                                    '3. SET FILTERUPDATE OUTSTANDING
                                    updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                    updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                                Next
                            End If
                        End If

                    End If

                    'PDR OUT
                    If Len(updNilaiPdrOut) > 0 Then
                        'UPDATE DETAIL OUT
                        sql = "UPDATE m6_pdr_out SET jmlrealisasi = (CASE idpdrout " & updNilaiPdrOut & " ELSE jmlrealisasi END) WHERE " & updFilterPdrOut
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpdr FROM m6_pdr_out WHERE " & updFilterPdrOut & " GROUP BY idpdr")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                            Next
                        End If

                        'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                        If Len(ftDetail) > 0 Then
                            dtOut = AsDataTableAmbilDariDB("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_out WHERE " & ftDetail & " GROUP BY idpdr")
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
                                    updNilaiPdrUtamaOut = String.Concat(updNilaiPdrUtamaOut, "WHEN '" & dr1("idpdr") & "' THEN '" & statusOut & "' ")
                                    '3. SET FILTERUPDATE OUTSTANDING
                                    updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                    updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                                Next
                            End If
                        End If

                    End If

                    'PDR UTAMA
                    'UPDATE STATUS IN DAN OUT
                    If Len(updNilaiPdrUtamaIn) > 0 And Len(updNilaiPdrUtamaOut) > 0 Then
                        sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END), pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE STATUS IN
                    ElseIf Len(updNilaiPdrUtamaIn) > 0 Then
                        sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END) WHERE " & updFilterPdrUtama
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE STATUS OUT
                    ElseIf Len(updNilaiPdrUtamaOut) > 0 Then
                        sql = "UPDATE m6_pdr SET pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
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
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "WO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M6_WoUpdateStatusOld(ByVal param As String) As String

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
        Dim dtdetail As DataTable, dtdetailIn As DataTable, dtdetailOut As DataTable
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
            Dim sumber As String = "Wo", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Wotgl, Wonotransaksi, Wostatus FROM M6_Wo WHERE Woid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Wostatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_wo_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Wo_HistorySimpan("" & paramSplit(0) & "★M6_Wo_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m6_wo_terkait("woid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ===================================================
                Dim updNilaiPdrUtamaIn = "", updNilaiPdrUtamaOut = "", updFilterPdrUtama = ""
                Dim idbarang As Integer = 0
                Dim idpdrin As Integer = 0, idpdrout As Integer = 0
                Dim updNilaiPdrIn As String = "", updFilterPdrIn As String = ""
                Dim updNilaiPdrOut As String = "", updFilterPdrOut As String = ""

                'AMBIL DATA DETAIL IN
                dtdetailIn = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpdrin, urutan FROM m6_wo_in WHERE idwo = '" & idtransaksi & "'")
                If dtdetailIn.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetailIn.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : idpdrin = dr1("idpdrin")

                        'UPDATE OUTSTANDING ---------------------------
                        If idpdrin <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING 
                            Dim Outstanding As Double = AsDataTableDSum(dtdetailIn, "jmlbarang", "idpdrin=" & idpdrin)
                            updNilaiPdrIn = String.Concat("WHEN '" & idpdrin & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPdrIn)
                            '2. SET FILTERUPDATE OUTSTANDING 
                            updFilterPdrIn = IIf(Len(updFilterPdrIn.ToString) = 0, "", updFilterPdrIn & " OR ")
                            updFilterPdrIn = String.Concat(updFilterPdrIn, "(idpdrin = '" & idpdrin & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found. (Result)" : Trans.Rollback() : GoTo selesai
                End If

                'PDR IN
                If Len(updNilaiPdrIn) > 0 Then
                    'UPDATE DETAIL IN
                    sql = "UPDATE m6_pdr_in SET jmlrealisasi = (CASE idpdrin " & updNilaiPdrIn & " ELSE jmlrealisasi END) WHERE " & updFilterPdrIn
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'AMBIL ID UTAMA IN
                    Dim ftDetail As String = ""
                    Dim dtIn As DataTable = AsDataTableAmbilDariDB("SELECT idpdr FROM m6_pdr_in WHERE " & updFilterPdrIn & " GROUP BY idpdr")
                    If dtIn.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtIn.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                        Next
                    End If

                    'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                    If Len(ftDetail) > 0 Then
                        dtIn = AsDataTableAmbilDariDB("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_in WHERE " & ftDetail & " GROUP BY idpdr")
                        If dtIn.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtIn.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                Dim statusIn As Integer = 0
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusIn = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusIn = 0
                                Else
                                    statusIn = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPdrUtamaIn = String.Concat(updNilaiPdrUtamaIn, "WHEN '" & dr1("idpdr") & "' THEN '" & statusIn & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                            Next
                        End If
                    End If

                End If

                'AMBIL DATA DETAIL OUT
                dtdetailOut = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpdrout, urutan FROM m6_wo_out WHERE idwo = '" & idtransaksi & "'")
                If dtdetailOut.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetailOut.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : idpdrout = dr1("idpdrout")

                        'UPDATE OUTSTANDING ---------------------------
                        If idpdrout <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING 
                            Dim Outstanding As Double = AsDataTableDSum(dtdetailOut, "jmlbarang", "idpdrout=" & idpdrout)
                            updNilaiPdrOut = String.Concat("WHEN '" & idpdrout & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPdrOut)
                            '2. SET FILTERUPDATE OUTSTANDING 
                            updFilterPdrOut = IIf(Len(updFilterPdrOut.ToString) = 0, "", updFilterPdrOut & " OR ")
                            updFilterPdrOut = String.Concat(updFilterPdrOut, "(idpdrout = '" & idpdrout & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found. (Material)" : Trans.Rollback() : GoTo selesai
                End If

                'PDR OUT
                If Len(updNilaiPdrOut) > 0 Then
                    'UPDATE DETAIL OUT
                    sql = "UPDATE m6_pdr_out SET jmlrealisasi = (CASE idpdrout " & updNilaiPdrOut & " ELSE jmlrealisasi END) WHERE " & updFilterPdrOut
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpdr FROM m6_pdr_out WHERE " & updFilterPdrOut & " GROUP BY idpdr")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpdr = '" & dr1("idpdr") & "')")
                        Next
                    End If

                    'SET NILAI STATUS DAN FILTER UPDATE UTAMA
                    If Len(ftDetail) > 0 Then
                        dtOut = AsDataTableAmbilDariDB("SELECT idpdr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m6_pdr_out WHERE " & ftDetail & " GROUP BY idpdr")
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
                                updNilaiPdrUtamaOut = String.Concat(updNilaiPdrUtamaOut, "WHEN '" & dr1("idpdr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPdrUtama = IIf(Len(updFilterPdrUtama.ToString) = 0, "", updFilterPdrUtama & " OR ")
                                updFilterPdrUtama = String.Concat(updFilterPdrUtama, "(pdrid = '" & dr1("idpdr") & "')")
                            Next
                        End If
                    End If

                End If

                'PDR UTAMA
                'UPDATE STATUS IN DAN OUT
                If Len(updNilaiPdrUtamaIn) > 0 And Len(updNilaiPdrUtamaOut) > 0 Then
                    sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END), pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE STATUS IN
                ElseIf Len(updNilaiPdrUtamaIn) > 0 Then
                    sql = "UPDATE m6_pdr SET pdrstatusrealisasiin = (CASE pdrid " & updNilaiPdrUtamaIn & " ELSE pdrstatusrealisasiin END) WHERE " & updFilterPdrUtama
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE STATUS OUT
                ElseIf Len(updNilaiPdrUtamaOut) > 0 Then
                    sql = "UPDATE m6_pdr SET pdrstatusrealisasiout = (CASE pdrid " & updNilaiPdrUtamaOut & " ELSE pdrstatusrealisasiout END) WHERE " & updFilterPdrUtama
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

            End If

            'update status utama
            sql = "UPDATE M6_Wo SET Wostatus = " & nilaiStatus & ", Womodifikasiuser='" & userid & "', Womodifikasitgl = NOW(), Woposting = 0, Wopostingtgl = '1971-01-01 00:00:00', Wojmlrevisi = Wojmlrevisi + 1 WHERE Woid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_WoSearch(PostWsSearch(paramSplit(0), "M6_WoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_WoDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Wo", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Woid, Wonotransaksi FROM M6_Wo WHERE Woid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT wocabang, wolokasi, wosumber, woautonotransaksi, wonotransaksi, wotgl"
            sql &= " FROM M6_wo"
            sql &= " WHERE woid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("wocabang")
                lokasi = dtNomorNext.Rows(0)("wolokasi")
                sumber = dtNomorNext.Rows(0)("wosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("woautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("wonotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("wotgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_Wo_In WHERE idwo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL2
            sql = "DELETE FROM M6_Wo_Out WHERE idwo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M6_Wo WHERE woid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_WoSearch(PostWsSearch(paramSplit(0), "M6_WoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

    <WebMethod()>
    Public Function M6_Wo_Detail_VSearch(ByVal param As String) As String
        'M5_So_Detail_VSearch --------------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sonotransaksi, 
        'souraian, socatatan, sonoref, sotgl, sotglnoref, sotglkirim, socustomerkontak, so1alamat1, 
        'so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, sobagianpenjualankode, 
        'sobagianpenjualannama, soekspedisi, soekspedisinama, sotermin, soterminnama, soterminharijatuhtempo, kodebarang, 
        'bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, brekpenjualan, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, 
        'jmlsisado, jmlsisadr, jmlsisapi, jmlsisasi, jmlsisarealisasi, socustomer, socustomerkode, socustomernama, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisarealisasips, bhargabeli, basset, ktingkatjual,
        'somatauang, sokurs, sotgljatuhtempo, sohargatermasukpajak, kpkp,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sol As String = ""

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
            Filter = Filter.Replace("idpr", "prd.idpr")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sol = query.PanggilQuery("m5_so_detail_v")
        sol = "SELECT prd.*, pa.pakode AS kodeaktivitas, m.mnama AS namamesin FROM m1_production_route_detail prd JOIN m1_production_route pr ON prd.idpr = pr.prid JOIN m1_machine m ON prd.kodemesin = m.mkode JOIN m1_production_activity pa ON prd.idpa = pa.paid"

        dt = AmbilData("aplikasi1-M6_wo_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idprdetail"), ""), sptField,
                     FxDB(dr("idpr"), ""), sptField,
                     FxDB(dr("idpa"), ""), sptField,
                     FxDB(dr("kodeaktivitas"), ""), sptField,
                     FxDB(dr("namaaktivitas"), ""), sptField,
                     FxDB(dr("kodemesin"), ""), sptField,
                     FxDB(dr("namamesin"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idprdetail, idpr, idpa, kodeaktivitas, namaaktivitas, kodemesin, namamesin, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_Wo_RcSearch(ByVal param As String) As String
        'M6_Wo_InSearch --------------------------------------------------------
        'idwoin, idwo, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idbomin, idpdrin, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, 
        'subdivisinama, proyeknama, notransaksi, bomnotransaksi, pdrnotransaksi, jmlsisamrs, jmlsisamrn, 
        'jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan

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
            Filter = Filter.Replace("notransaksi", "wrc.notransaksi")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_wo_getdata_in")
        sql = "select wo.woid AS woid, wo.wocabang AS wocabang, wo.wolokasi AS wolokasi, wo.wogudangasal AS wogudangasal, wo.wogudangproduksi AS wogudangproduksi, wo.wogudangtujuan AS wogudangtujuan, wo.wosumber AS wosumber, wo.wojenis AS wojenis, wo.woautonotransaksi AS woautonotransaksi, wo.wonotransaksi AS wonotransaksi, wo.wotgl AS wotgl, woin.idbarang AS idbarang, woin.namabarang, woin.tipebarang, wrc.jml, i.bkode, woin.satuan, woin.nilaisatuan, woin.jmlbarang, woin.satuanbarang, woin.idwoin, woin.satuanbarang, woin.harga, woin.hpppersen, woin.hpp, woin.rekpersediaan, woin.gudangasal, woin.gudangproduksi, woin.gudangtujuan, woin.costcenter, woin.divisi, woin.subdivisi, woin.proyek, woin.catatan, woin.urutan, i.bsatuanlapangan, i.bserial, i.bbatch, i.bjmllapangan FROM m6_wo wo LEFT JOIN m6_wo_in woin ON wo.woid = woin.idwo LEFT JOIN m6_wo_route_card wrc ON wo.woid = wrc.idwo LEFT JOIN m1_item i ON i.bid = woin.idbarang"

        dt = AmbilData("aplikasi1-M6_wo_rc_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("woid"), 0), sptField,
                     FxDB(dr("wocabang"), ""), sptField,
                     FxDB(dr("wolokasi"), ""), sptField,
                     FxDB(dr("wogudangasal"), ""), sptField,
                     FxDB(dr("wogudangproduksi"), ""), sptField,
                     FxDB(dr("wogudangtujuan"), ""), sptField,
                     FxDB(dr("wosumber"), ""), sptField,
                     FxDB(dr("wojenis"), ""), sptField,
                     FxDB(dr("woautonotransaksi"), 0), sptField,
                     FxDB(dr("wonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("wotgl"), ""), formatTgl), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("idwoin"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpppersen"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("woid, wocabang, wolokasi, wogudangasal, wogudangproduksi, wogudangtujuan, wosumber, wojenis, woautonotransaksi, wonotransaksi, wotgl, idbarang, namabarang, tipebarang, jml, bkode, satuan, nilaisatuan, jmlbarang, satuanbarang, idwoin, harga, hpppersen, hpp, rekpersediaan, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, bsatuanlapangan, bserial, bbatch, bjmllapangan "))

        Return wsResult
    End Function

End Class