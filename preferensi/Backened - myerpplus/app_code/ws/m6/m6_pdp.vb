Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_pdp
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_PdpSimpan(ByVal param As String) As String
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
        Dim isUpdate As Boolean

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
        'pdpid(0) As Integer, pdpcabang(1) As String, pdplokasi(2) As String, pdpgudangasal(3) As String, pdpgudangproduksi(4) As String, 
        'pdpgudangtujuan(5) As String, pdpsumber(6) As String, pdpjenis(7) As String, pdpautonotransaksi(8) As Integer, pdpnotransaksi(9) As String, 
        'pdptgl(10) As Date, pdpkodepa(11) As Integer, pdpbagianpdp(12) As Integer, pdpbagianpdpkontak(13) As String, pdptgldipakai(14) As Date, 
        'pdpestimasikerja(15) As String, pdpmatauang(16) As String, pdpkurs(17) As Double, pdptotalhargain(18) As Double, pdptotalhargaout(19) As Double, 
        'pdptotalhppin(20) As Double, pdptotalhppout(21) As Double, pdpuraian(22) As String, pdpcatatan(23) As String, pdpnoref(24) As String, 
        'pdptglnoref(25) As Date, pdpidbom(26) As Integer, pdpidpdr(27) As Integer, pdpidwo(28) As Integer, pdpidmrs(29) As Integer, 
        'pdpidmrn(30) As Integer, pdpstatus(31) As Integer, pdpstatussebelumnya(32) As Integer, pdpjmlrevisi(33) As Integer, pdpcetakanke(34) As Integer, 
        'pdpinputuser(35) As Integer, pdpinputtgl(36) As DateTime, pdpmodifikasiuser(37) As Integer, pdpmodifikasitgl(38) As DateTime, pdpposting(39) As Integer, 
        'pdptutupperiode(40) As Integer, pdpisclose(41) As Integer, pdpcustomtext1(42) As String, pdpcustomtext2(43) As String, pdpcustomtext3(44) As String, 
        'pdpcustomtext4(45) As String, pdpcustomtext5(46) As String, pdpcustomint1(47) As Integer, pdpcustomint2(48) As Integer, pdpcustomint3(49) As Integer, 
        'pdpcustomdbl1(50) As Double, pdpcustomdbl2(51) As Double, pdpcustomdbl3(52) As Double, pdpcustomdate1(53) As Date, pdpcustomdate2(54) As Date, 
        'pdpcustomdate3(55) As Date, pdpaktivitas(56) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pdpid, pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, 
        'pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, 
        'pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, 
        'pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, 
        'pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, 
        'pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdptutupperiode, pdpisclose, 
        'pdpcustomtext1, pdpcustomtext2, pdpcustomtext3, pdpcustomtext4, pdpcustomtext5, pdpcustomint1, pdpcustomint2, 
        'pdpcustomint3, pdpcustomdbl1, pdpcustomdbl2, pdpcustomdbl3, pdpcustomdate1, pdpcustomdate2, pdpcustomdate3, pdpaktivitas


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 56 And dataUtama.Length <> 57) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pdpid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pdpid required numeric." : GoTo selesai
        End If
        'pdpautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pdpautonotransaksi required numeric." : GoTo selesai
        End If
        'pdptgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "pdptgl required date." : GoTo selesai
        End If
        'pdpkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "pdpkodepa required numeric." : GoTo selesai
        End If
        'pdpbagianpdp(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "pdpbagianpdp required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "pdpbagianpdp can't be empty." : GoTo selesai
        End If
        'pdptgldipakai(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "pdptgldipakai required date." : GoTo selesai
        End If
        'pdpkurs(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "pdpkurs required numeric." : GoTo selesai
        End If
        'pdptotalhargain(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pdptotalhargain required numeric." : GoTo selesai
        End If
        'pdptotalhargaout(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pdptotalhargaout required numeric." : GoTo selesai
        End If
        'pdptotalhppin(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "pdptotalhppin required numeric." : GoTo selesai
        End If
        'pdptotalhppout(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "pdptotalhppout required numeric." : GoTo selesai
        End If
        'pdptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pdptglnoref required date." : GoTo selesai
        End If
        'pdpidbom(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "pdpidbom required numeric." : GoTo selesai
        End If
        'pdpidpdr(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pdpidpdr required numeric." : GoTo selesai
        End If
        'pdpidwo(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pdpidwo required numeric." : GoTo selesai
        End If
        'pdpidmrs(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "pdpidmrs required numeric." : GoTo selesai
        End If
        'pdpidmrn(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pdpidmrn required numeric." : GoTo selesai
        End If
        'pdpstatus(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "pdpstatus required numeric." : GoTo selesai
        End If
        'pdpstatussebelumnya(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pdpstatussebelumnya required numeric." : GoTo selesai
        End If
        'pdpjmlrevisi(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pdpjmlrevisi required numeric." : GoTo selesai
        End If
        'pdpcetakanke(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pdpcetakanke required numeric." : GoTo selesai
        End If
        'pdpinputuser(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pdpinputuser required numeric." : GoTo selesai
        End If
        'pdpinputtgl(36) As DateTime
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "pdpinputtgl required date." : GoTo selesai
        End If
        'pdpmodifikasiuser(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pdpmodifikasiuser required numeric." : GoTo selesai
        End If
        'pdpmodifikasitgl(38) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "pdpmodifikasitgl required date." : GoTo selesai
        End If
        'pdpposting(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pdpposting required numeric." : GoTo selesai
        End If
        'pdptutupperiode(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pdptutupperiode required numeric." : GoTo selesai
        End If
        'pdpisclose(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pdpisclose required numeric." : GoTo selesai
        End If
        'pdpcustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "pdpcustomint1 required numeric." : GoTo selesai
        End If
        'pdpcustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "pdpcustomint2 required numeric." : GoTo selesai
        End If
        'pdpcustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "pdpcustomint3 required numeric." : GoTo selesai
        End If
        'pdpcustomdbl1(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "pdpcustomdbl1 required numeric." : GoTo selesai
        End If
        'pdpcustomdbl2(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pdpcustomdbl2 required numeric." : GoTo selesai
        End If
        'pdpcustomdbl3(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pdpcustomdbl3 required numeric." : GoTo selesai
        End If
        'pdpcustomdate1(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "pdpcustomdate1 required date." : GoTo selesai
        End If
        'pdpcustomdate2(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "pdpcustomdate2 required date." : GoTo selesai
        End If
        'pdpcustomdate3(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "pdpcustomdate3 required date." : GoTo selesai
        End If

        If dataUtama.Length > 56 Then
            'pdpaktivitas(56) As Integer
            If (IsNumeric(dataUtama(56)) = False) Then
                result(2) = "pdpaktivitas required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===========================================


        'VALIDASI DATA UTAMA =======================================================
        'pdpcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pdpcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pdpcabang should not be more than 25 character." : GoTo selesai
        End If

        'pdplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pdplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pdplokasi should not be more than 25 character." : GoTo selesai
        End If

        'pdpgudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pdpgudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pdpgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'pdpgudangproduksi(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "pdpgudangproduksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "pdpgudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'pdpgudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "pdpgudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "pdpgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'pdpsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pdpsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "pdpsumber should not be more than 10 character." : GoTo selesai
        End If

        'pdpjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pdpjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "pdpjenis should not be more than 25 character." : GoTo selesai
        End If

        'pdpnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pdpnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "pdpnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pdptgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "pdptgl can't be empty" : GoTo selesai
        End If

        'pdptgldipakai(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "pdptgldipakai can't be empty" : GoTo selesai
        End If

        'pdpmatauang(16) As String
        If Len(dataUtama(16)) = 0 Then
            result(2) = "pdpmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(16)) > 25 Then
            result(2) = "pdpmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pdpkurs(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "pdpkurs can't be empty" : GoTo selesai
        End If

        'pdptotalhargain(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "pdptotalhargain can't be empty" : GoTo selesai
        End If

        'pdptotalhargaout(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pdptotalhargaout can't be empty" : GoTo selesai
        End If

        'pdptotalhppin(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "pdptotalhppin can't be empty" : GoTo selesai
        End If

        'pdptotalhppout(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "pdptotalhppout can't be empty" : GoTo selesai
        End If

        'pdptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pdptglnoref can't be empty" : GoTo selesai
        End If

        'pdpinputtgl(36) As DateTime
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pdpinputtgl can't be empty" : GoTo selesai
        End If

        'pdpmodifikasitgl(38) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pdpmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pdpcustomdbl1(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "pdpcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pdpcustomdbl2(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "pdpcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pdpcustomdbl3(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "pdpcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pdpcustomdate1(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "pdpcustomdate1 can't be empty" : GoTo selesai
        End If

        'pdpcustomdate2(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "pdpcustomdate2 can't be empty" : GoTo selesai
        End If

        'pdpcustomdate3(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "pdpcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pdpid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpgudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpbagianpdp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpbagianpdpkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpidbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdptutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpaktivitas", AsEnumTypeData.AsInt64)
        If dataUtama.Length > 56 Then
            If AsDataTableTambahData(dtutama, "pdpid~pdpcabang~pdplokasi~pdpgudangasal~pdpgudangproduksi~pdpgudangtujuan~pdpsumber~pdpjenis~pdpautonotransaksi~pdpnotransaksi~pdptgl~pdpkodepa~pdpbagianpdp~pdpbagianpdpkontak~pdptgldipakai~pdpestimasikerja~pdpmatauang~pdpkurs~pdptotalhargain~pdptotalhargaout~pdptotalhppin~pdptotalhppout~pdpuraian~pdpcatatan~pdpnoref~pdptglnoref~pdpidbom~pdpidpdr~pdpidwo~pdpidmrs~pdpidmrn~pdpstatus~pdpstatussebelumnya~pdpjmlrevisi~pdpcetakanke~pdpinputuser~pdpinputtgl~pdpmodifikasiuser~pdpmodifikasitgl~pdpposting~pdptutupperiode~pdpisclose~pdpcustomtext1~pdpcustomtext2~pdpcustomtext3~pdpcustomtext4~pdpcustomtext5~pdpcustomint1~pdpcustomint2~pdpcustomint3~pdpcustomdbl1~pdpcustomdbl2~pdpcustomdbl3~pdpcustomdate1~pdpcustomdate2~pdpcustomdate3~pdpaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "pdpid~pdpcabang~pdplokasi~pdpgudangasal~pdpgudangproduksi~pdpgudangtujuan~pdpsumber~pdpjenis~pdpautonotransaksi~pdpnotransaksi~pdptgl~pdpkodepa~pdpbagianpdp~pdpbagianpdpkontak~pdptgldipakai~pdpestimasikerja~pdpmatauang~pdpkurs~pdptotalhargain~pdptotalhargaout~pdptotalhppin~pdptotalhppout~pdpuraian~pdpcatatan~pdpnoref~pdptglnoref~pdpidbom~pdpidpdr~pdpidwo~pdpidmrs~pdpidmrn~pdpstatus~pdpstatussebelumnya~pdpjmlrevisi~pdpcetakanke~pdpinputuser~pdpinputtgl~pdpmodifikasiuser~pdpmodifikasitgl~pdpposting~pdptutupperiode~pdpisclose~pdpcustomtext1~pdpcustomtext2~pdpcustomtext3~pdpcustomtext4~pdpcustomtext5~pdpcustomint1~pdpcustomint2~pdpcustomint3~pdpcustomdbl1~pdpcustomdbl2~pdpcustomdbl3~pdpcustomdate1~pdpcustomdate2~pdpcustomdate3~pdpaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'mesin(0) As String, tgl(1) As Date, kelas(2) As String, subkelas(3) As String, jml(4) As Double, 
        'satuan(5) As String, nilaisatuan(6) As Double, jmlbarang(7) As Double, satuanbarang(8) As String, matauang(9) As String, 
        'kurs(10) As Double, harga(11) As Double, hpp(12) As Double, cabang(13) As String, lokasi(14) As String, 
        'gudang(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, jmlrealisasi(22) As Double, statusrealisasi(23) As Integer, insertby(24) As Integer, 
        'isclose(25) As Integer, inputuser(26) As Bigint, inputtgl(27) As DateTime, modifikasiuser(28) As Bigint, modifikasitgl(29) As DateTime, 
        'customtext1(30) As String, customtext2(31) As String, customtext3(32) As String, customtext4(33) As String, customtext5(34) As String, 
        'customtext6(35) As String, customtext7(36) As String, customtext8(37) As String, customtext9(38) As String, customtext10(39) As String, 
        'customint1(40) As Integer, customint2(41) As Integer, customint3(42) As Integer, customint4(43) As Integer, customint5(44) As Integer, 
        'customint6(45) As Integer, customint7(46) As Integer, customint8(47) As Integer, customint9(48) As Integer, customint10(49) As Integer, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdbl4(53) As Double, customdbl5(54) As Double, 
        'customdbl6(55) As Double, customdbl7(56) As Double, customdbl8(57) As Double, customdbl9(58) As Double, customdbl10(59) As Double, 
        'customdate1(60) As Date, customdate2(61) As Date, customdate3(62) As Date, customdate4(63) As Date, customdate5(64) As Date, 
        'customdate6(65) As Date, customdate7(66) As Date, customdate8(67) As Date, customdate9(68) As Date, customdate10(69) As Date, 
        'idpdpdetail(70) As Bigint, idpdp(71) As Bigint, idsodetail(72) As Bigint, idbarang(73) As Bigint, namabarang(74) As String, 
        'tipebarang(75) As String, jmljam(76) As Double

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)

        Dim vTotalJam As Double = 0, vMaxJam As Double = 0
        Dim dtSett As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE smodule = 6 AND sgrup = 'options' AND skode = 'PdpMaxJam'", myConn)
        If dtSett.Rows.Count > 0 Then
            If IsNumeric(FxDB(dtSett.Rows(0)(0), 0)) Then
                vMaxJam = FxDB(dtSett.Rows(0)(0), 0)
            Else
                result(2) = "Setting for PdpMaxJam required numeric." : GoTo selesai
            End If
        Else
            result(2) = "Setting for PdpMaxJam not found." : GoTo selesai
        End If

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 77) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'tgl(1) As Date
            If (IsDate(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - tgl required date." : GoTo selesai
            End If
            'jml(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'jmlrealisasi(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlrealisasi required numeric." : GoTo selesai
            End If
            'statusrealisasi(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statusrealisasi required numeric." : GoTo selesai
            End If
            'insertby(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - insertby required numeric." : GoTo selesai
            End If
            'isclose(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'inputtgl(27) As DateTime
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - inputtgl required date." : GoTo selesai
            End If
            'modifikasitgl(29) As DateTime
            If (IsDate(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - modifikasitgl required date." : GoTo selesai
            End If
            'customint1(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customint4(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - customint4 required numeric." : GoTo selesai
            End If
            'customint5(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - customint5 required numeric." : GoTo selesai
            End If
            'customint6(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - customint6 required numeric." : GoTo selesai
            End If
            'customint7(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customint7 required numeric." : GoTo selesai
            End If
            'customint8(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customint8 required numeric." : GoTo selesai
            End If
            'customint9(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customint9 required numeric." : GoTo selesai
            End If
            'customint10(49) As Integer
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customint10 required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdbl4(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl4 required numeric." : GoTo selesai
            End If
            'customdbl5(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl5 required numeric." : GoTo selesai
            End If
            'customdbl6(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdbl6 required numeric." : GoTo selesai
            End If
            'customdbl7(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdbl7 required numeric." : GoTo selesai
            End If
            'customdbl8(57) As Double
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - customdbl8 required numeric." : GoTo selesai
            End If
            'customdbl9(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdbl9 required numeric." : GoTo selesai
            End If
            'customdbl10(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - customdbl10 required numeric." : GoTo selesai
            End If
            'customdate1(60) As Date
            If (IsDate(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(61) As Date
            If (IsDate(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(62) As Date
            If (IsDate(dataRowDetail(62)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'customdate4(63) As Date
            If (IsDate(dataRowDetail(63)) = False) Then
                result(2) = "Row : " & i & " - customdate4 required date." : GoTo selesai
            End If
            'customdate5(64) As Date
            If (IsDate(dataRowDetail(64)) = False) Then
                result(2) = "Row : " & i & " - customdate5 required date." : GoTo selesai
            End If
            'customdate6(65) As Date
            If (IsDate(dataRowDetail(65)) = False) Then
                result(2) = "Row : " & i & " - customdate6 required date." : GoTo selesai
            End If
            'customdate7(66) As Date
            If (IsDate(dataRowDetail(66)) = False) Then
                result(2) = "Row : " & i & " - customdate7 required date." : GoTo selesai
            End If
            'customdate8(67) As Date
            If (IsDate(dataRowDetail(67)) = False) Then
                result(2) = "Row : " & i & " - customdate8 required date." : GoTo selesai
            End If
            'customdate9(68) As Date
            If (IsDate(dataRowDetail(68)) = False) Then
                result(2) = "Row : " & i & " - customdate9 required date." : GoTo selesai
            End If
            'customdate10(69) As Date
            If (IsDate(dataRowDetail(69)) = False) Then
                result(2) = "Row : " & i & " - customdate10 required date." : GoTo selesai
            End If
            'idpdpdetail(70) As Bigint
            If (IsNumeric(dataRowDetail(70)) = False) Then
                result(2) = "Row : " & i & " - idpdpdetail required numeric." : GoTo selesai
            End If
            'idpdp(71) As Bigint
            If (IsNumeric(dataRowDetail(71)) = False) Then
                result(2) = "Row : " & i & " - idpdp required numeric." : GoTo selesai
            End If
            'idsodetail(72) As Bigint
            If (IsNumeric(dataRowDetail(72)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idbarang(73) As Bigint
            If (IsNumeric(dataRowDetail(73)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmljam(76) As Double
            If (IsNumeric(dataRowDetail(76)) = False) Then
                result(2) = "Row : " & i & " - jmljam required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'mesin(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - mesin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 500 Then
                result(2) = "Row : " & i & " - mesin should not be more than 500 character." : GoTo selesai
            End If

            'tgl(1) As Date
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - tgl can't be empty" : GoTo selesai
            End If

            'kelas(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kelas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 500 Then
                result(2) = "Row : " & i & " - kelas should not be more than 500 character." : GoTo selesai
            End If

            'subkelas(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - subkelas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 500 Then
                result(2) = "Row : " & i & " - subkelas should not be more than 500 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 500 Then
                result(2) = "Row : " & i & " - satuan should not be more than 500 character." : GoTo selesai
            End If

            'nilaisatuan(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If

            'satuanbarang(8) As String
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(8)) > 500 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 500 character." : GoTo selesai
            End If

            'matauang(9) As String
            'If Len(dataRowDetail(9)) = 0 Then
            '    result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(9)) > 500 Then
                result(2) = "Row : " & i & " - matauang should not be more than 500 character." : GoTo selesai
            End If

            'kurs(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'jmlrealisasi(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlrealisasi can't be empty" : GoTo selesai
            End If

            'inputuser(26) As 
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - inputuser can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 20 Then
                result(2) = "Row : " & i & " - inputuser should not be more than 20 character." : GoTo selesai
            End If

            'inputtgl(27) As DateTime
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - inputtgl can't be empty" : GoTo selesai
            End If

            'modifikasiuser(28) As 
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - modifikasiuser can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(28)) > 20 Then
                result(2) = "Row : " & i & " - modifikasiuser should not be more than 20 character." : GoTo selesai
            End If

            'modifikasitgl(29) As DateTime
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - modifikasitgl can't be empty" : GoTo selesai
            End If

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdbl4(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
            End If

            'customdbl5(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
            End If

            'customdbl6(55) As Double
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdbl6 can't be empty" : GoTo selesai
            End If

            'customdbl7(56) As Double
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdbl7 can't be empty" : GoTo selesai
            End If

            'customdbl8(57) As Double
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdbl8 can't be empty" : GoTo selesai
            End If

            'customdbl9(58) As Double
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdbl9 can't be empty" : GoTo selesai
            End If

            'customdbl10(59) As Double
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdbl10 can't be empty" : GoTo selesai
            End If

            'customdate1(60) As Date
            If Len(dataRowDetail(60)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(61) As Date
            If Len(dataRowDetail(61)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(62) As Date
            If Len(dataRowDetail(62)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'customdate4(63) As Date
            If Len(dataRowDetail(63)) = 0 Then
                result(2) = "Row : " & i & " - customdate4 can't be empty" : GoTo selesai
            End If

            'customdate5(64) As Date
            If Len(dataRowDetail(64)) = 0 Then
                result(2) = "Row : " & i & " - customdate5 can't be empty" : GoTo selesai
            End If

            'customdate6(65) As Date
            If Len(dataRowDetail(65)) = 0 Then
                result(2) = "Row : " & i & " - customdate6 can't be empty" : GoTo selesai
            End If

            'customdate7(66) As Date
            If Len(dataRowDetail(66)) = 0 Then
                result(2) = "Row : " & i & " - customdate7 can't be empty" : GoTo selesai
            End If

            'customdate8(67) As Date
            If Len(dataRowDetail(67)) = 0 Then
                result(2) = "Row : " & i & " - customdate8 can't be empty" : GoTo selesai
            End If

            'customdate9(68) As Date
            If Len(dataRowDetail(68)) = 0 Then
                result(2) = "Row : " & i & " - customdate9 can't be empty" : GoTo selesai
            End If

            'customdate10(69) As Date
            If Len(dataRowDetail(69)) = 0 Then
                result(2) = "Row : " & i & " - customdate10 can't be empty" : GoTo selesai
            End If

            'idpdpdetail(70) As Bigint
            If Len(dataRowDetail(70)) = 0 Then
                result(2) = "Row : " & i & " - idpdpdetail can't be empty" : GoTo selesai
            End If

            'idpdp(71) As Bigint
            If Len(dataRowDetail(71)) = 0 Then
                result(2) = "Row : " & i & " - idpdp can't be empty" : GoTo selesai
            End If

            'idsodetail(72) As Bigint
            If Len(dataRowDetail(72)) = 0 Then
                result(2) = "Row : " & i & " - idsodetail can't be empty" : GoTo selesai
            End If

            'idbarang(73) As Bigint
            If Len(dataRowDetail(73)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If

            'jmljam(76) As Double
            If Len(dataRowDetail(76)) = 0 Then
                result(2) = "Row : " & i & " - jmljam can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            'VALIDASI JML JAM PRODUKSI PER TANGGAL PER MESIN TIDAK BOLEH LEBIH DARI vMaxJam JAM
            vTotalJam = AsDataTableDSum(dtdetail, "jmljam", "mesin = '" & dataRowDetail(0) & "' AND tgl = '" & dataRowDetail(1) & "'") + dataRowDetail(76)
            If Math.Round(vTotalJam, 5) > vMaxJam Then
                result(2) = "Row : " & i & " - Total operating hours for machine : " & dataRowDetail(0) & ", date : " & dataRowDetail(1) & " = " & vTotalJam & " hours. It must be less than or equal to '" & vMaxJam & "' hours." : GoTo selesai
            Else
                If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If
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
                Dim vModuleId As Integer = 6, vMenuId As Integer = 91
                Select Case drutama("pdpstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pdptgl")), AsFormatTanggal(drutama("pdptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                ''VALIDASI SIMPAN ========================================
                'If drutama("pdpstatus") = 2 Or drutama("pdpstatus") = 1 Or drutama("pdpstatus") = 8 Or drutama("pdpstatus") = 9 Or drutama("pdpstatus") = 10 Or drutama("pdpstatus") = 11 Then
                '    Dim rsValidasi As String
                '    'ValidasiSimpan
                '    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingWoIn, ftOutstandingWoIn, dtdetail2, ftExistOutstandingMrsOut, ftOutstandingMrsOut, "", "", ftExistStok, "", ftStokAvailable, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangproduksi")
                '    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("Pdpid")
                    notransaksi = drutama("Pdpnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(Pdpid), Pdpnotransaksi FROM M6_Pdp WHERE Pdpid='" & result(4) & "' AND pdpstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("pdpautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Pdpcabang"), drutama("Pdplokasi"), drutama("Pdpsumber"), drutama("Pdptgl"), drutama("Pdpsumber"), 6)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(Pdpid) FROM M6_Pdp WHERE Pdpnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_pdp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Pdp_HistorySimpan("" & paramSplit(0) & "★M6_Pdp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pdpsumber")) & "▼" & FixQuotes(drutama("pdpid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Pdp set pdpcabang  = '" & FixQuotes(drutama("pdpcabang")) & "', pdplokasi  = '" & FixQuotes(drutama("pdplokasi")) & "', pdpgudangasal  = '" & FixQuotes(drutama("pdpgudangasal")) & "', pdpgudangproduksi  = '" & FixQuotes(drutama("pdpgudangproduksi")) & "', pdpgudangtujuan  = '" & FixQuotes(drutama("pdpgudangtujuan")) & "', pdpsumber  = '" & FixQuotes(drutama("pdpsumber")) & "', pdpjenis  = '" & FixQuotes(drutama("pdpjenis")) & "', pdpautonotransaksi  = " & drutama("pdpautonotransaksi") & ", pdpnotransaksi  = '" & FixQuotes(notransaksi) & "', pdptgl  = '" & FixQuotes(AsFormatTanggal(drutama("pdptgl"))) & "', pdpkodepa  = " & drutama("pdpkodepa") & ", pdpbagianpdp  = " & drutama("pdpbagianpdp") & ", pdpbagianpdpkontak  = '" & FixQuotes(drutama("pdpbagianpdpkontak")) & "', pdptgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("pdptgldipakai"))) & "', pdpestimasikerja  = '" & FixQuotes(drutama("pdpestimasikerja")) & "', pdpmatauang  = '" & FixQuotes(drutama("pdpmatauang")) & "', pdpkurs  = '" & FixDouble(drutama("pdpkurs")) & "', pdptotalhargain  = '" & FixDouble(drutama("pdptotalhargain")) & "', pdptotalhargaout  = '" & FixDouble(drutama("pdptotalhargaout")) & "', pdptotalhppin  = '" & FixDouble(drutama("pdptotalhppin")) & "', pdptotalhppout  = '" & FixDouble(drutama("pdptotalhppout")) & "', pdpuraian  = '" & FixQuotes(drutama("pdpuraian")) & "', pdpcatatan  = '" & FixQuotes(drutama("pdpcatatan")) & "', pdpnoref  = '" & FixQuotes(drutama("pdpnoref")) & "', pdptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pdptglnoref"))) & "', pdpidbom  = " & drutama("pdpidbom") & ", pdpidpdr  = " & drutama("pdpidpdr") & ", pdpidwo  = " & drutama("pdpidwo") & ", pdpidmrs  = " & drutama("pdpidmrs") & ", pdpidmrn  = " & drutama("pdpidmrn") & ", pdpstatus  = " & drutama("pdpstatus") & ", pdpstatussebelumnya  = " & drutama("pdpstatussebelumnya") & ", pdpjmlrevisi  = pdpjmlrevisi+1, pdpcetakanke  = " & drutama("pdpcetakanke") & ", pdpmodifikasiuser  = " & drutama("pdpmodifikasiuser") & ", pdpmodifikasitgl  = NOW(), pdpposting  = 0, pdptutupperiode  = " & drutama("pdptutupperiode") & ", pdpcustomtext1  = '" & FixQuotes(drutama("pdpcustomtext1")) & "', pdpcustomtext2  = '" & FixQuotes(drutama("pdpcustomtext2")) & "', pdpcustomtext3  = '" & FixQuotes(drutama("pdpcustomtext3")) & "', pdpcustomtext4  = '" & FixQuotes(drutama("pdpcustomtext4")) & "', pdpcustomtext5  = '" & FixQuotes(drutama("pdpcustomtext5")) & "', pdpcustomint1  = " & drutama("pdpcustomint1") & ", pdpcustomint2  = " & drutama("pdpcustomint2") & ", pdpcustomint3  = " & drutama("pdpcustomint3") & ", pdpcustomdbl1  = '" & FixDouble(drutama("pdpcustomdbl1")) & "', pdpcustomdbl2  = '" & FixDouble(drutama("pdpcustomdbl2")) & "', pdpcustomdbl3  = '" & FixDouble(drutama("pdpcustomdbl3")) & "', pdpcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate1"))) & "', pdpcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate2"))) & "', pdpcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate3"))) & "', pdpaktivitas  = '" & FixDouble(drutama("pdpaktivitas")) & "' where pdpid = '" & drutama("pdpid") & "'"
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

                    If drutama("Pdpautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Pdpcabang"), drutama("Pdplokasi"), drutama("Pdpsumber"), drutama("Pdptgl"), drutama("Pdpsumber"), 6)
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
                        notransaksi = drutama("Pdpnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(Pdpid) FROM m6_pdp WHERE Pdpnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Pdp (pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdptutupperiode, pdpisclose, pdpcustomtext1, pdpcustomtext2, pdpcustomtext3, pdpcustomtext4, pdpcustomtext5, pdpcustomint1, pdpcustomint2, pdpcustomint3, pdpcustomdbl1, pdpcustomdbl2, pdpcustomdbl3, pdpcustomdate1, pdpcustomdate2, pdpcustomdate3, pdpaktivitas) values('" & FixQuotes(drutama("pdpcabang")) & "', '" & FixQuotes(drutama("pdplokasi")) & "', '" & FixQuotes(drutama("pdpgudangasal")) & "', '" & FixQuotes(drutama("pdpgudangproduksi")) & "', '" & FixQuotes(drutama("pdpgudangtujuan")) & "', '" & FixQuotes(drutama("pdpsumber")) & "', '" & FixQuotes(drutama("pdpjenis")) & "', " & drutama("pdpautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdptgl"))) & "', " & drutama("pdpkodepa") & ", " & drutama("pdpbagianpdp") & ", '" & FixQuotes(drutama("pdpbagianpdpkontak")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdptgldipakai"))) & "', '" & FixQuotes(drutama("pdpestimasikerja")) & "', '" & FixQuotes(drutama("pdpmatauang")) & "', '" & FixDouble(drutama("pdpkurs")) & "', '" & FixDouble(drutama("pdptotalhargain")) & "', '" & FixDouble(drutama("pdptotalhargaout")) & "', '" & FixDouble(drutama("pdptotalhppin")) & "', '" & FixDouble(drutama("pdptotalhppout")) & "', '" & FixQuotes(drutama("pdpuraian")) & "', '" & FixQuotes(drutama("pdpcatatan")) & "', '" & FixQuotes(drutama("pdpnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdptglnoref"))) & "', " & drutama("pdpidbom") & ", " & drutama("pdpidpdr") & ", " & drutama("pdpidwo") & ", " & drutama("pdpidmrs") & ", " & drutama("pdpidmrn") & ", " & drutama("pdpstatus") & ", " & drutama("pdpstatussebelumnya") & ", " & drutama("pdpjmlrevisi") & ", " & drutama("pdpcetakanke") & ", " & drutama("pdpinputuser") & ", NOW(), " & drutama("pdpmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("pdptutupperiode") & ", " & drutama("pdpisclose") & ", '" & FixQuotes(drutama("pdpcustomtext1")) & "', '" & FixQuotes(drutama("pdpcustomtext2")) & "', '" & FixQuotes(drutama("pdpcustomtext3")) & "', '" & FixQuotes(drutama("pdpcustomtext4")) & "', '" & FixQuotes(drutama("pdpcustomtext5")) & "', " & drutama("pdpcustomint1") & ", " & drutama("pdpcustomint2") & ", " & drutama("pdpcustomint3") & ", '" & FixDouble(drutama("pdpcustomdbl1")) & "', '" & FixDouble(drutama("pdpcustomdbl2")) & "', '" & FixDouble(drutama("pdpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate3"))) & "', '" & FixDouble(drutama("pdpaktivitas")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select Pdpid from M6_Pdp where Pdpnotransaksi='" & notransaksi & "' AND Pdpinputuser= '" & userid & "' order by Pdpmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from m6_pdp_detail where idPdp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail1
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    Dim wsM0_NomorPDR As New m0_nomor
                    Dim rsNotransaksiPDR As String = "", notransaksiPDR As String = ""
                    Dim arrNotransaksiPDR(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                    For Each dr1 As DataRow In dtdetail.Rows
                        If drutama("pdpstatus") = 2 And Len(dr1("customtext10")) = 0 Then
                            rsNotransaksiPDR = wsM0_NomorPDR.M0_Notransaksi(drutama("pdpcabang"), drutama("pdplokasi"), "RC", AsFormatTanggal(dr1("tgl")), "RC", 6)
                            arrNotransaksiPDR = rsNotransaksiPDR.Split(sptSubParam)
                            'cek success generate notransaksi
                            If (arrNotransaksiPDR(0) = 1) Then
                                notransaksiPDR = arrNotransaksiPDR(2)
                                'tambah query update m0_nomor_next
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = arrNotransaksiPDR(3)
                                End With
                                objCmd.ExecuteNonQuery()
                                dr1("customtext10") = notransaksiPDR
                            Else
                                result(2) = arrNotransaksiPDR(1) : Trans.Rollback() : GoTo selesai
                            End If

                        End If
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("mesin")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl"))) & "', '" & FixQuotes(dr1("kelas")) & "', '" & FixQuotes(dr1("subkelas")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("insertby") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("inputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("inputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("modifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("modifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", " & dr1("customint4") & ", " & dr1("customint5") & ", " & dr1("customint6") & ", " & dr1("customint7") & ", " & dr1("customint8") & ", " & dr1("customint9") & ", " & dr1("customint10") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "', '" & FixQuotes(dr1("idpdpdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idsodetail")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jmljam")) & "')")
                    Next
                    sql = "Insert into M6_Pdp_Detail(mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam) values" & strValue2.ToString & ""
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


                'INSERT KE TABEL PLAN
                If drutama("pdpstatus") = 2 Then
                    sql = "INSERT INTO m6_production_planning(SELECT pdpd.mesin, pdpd.tgl, pdpd.kelas, pdpd.subkelas, pdpd.jml, pdpd.satuan, pdpd.nilaisatuan, pdpd.jmlbarang, pdpd.satuanbarang, pdpd.matauang, pdpd.kurs, pdpd.harga, pdpd.hpp, pdpd.cabang, pdpd.lokasi, pdpd.gudang, pdpd.costcenter, pdpd.divisi, pdpd.subdivisi, pdpd.proyek, pdpd.catatan, pdpd.urutan, pdpd.jmlrealisasi, pdpd.statusrealisasi, pdpd.insertby, pdpd.isclose, pdpd.inputuser, pdpd.inputtgl, pdpd.modifikasiuser, pdpd.modifikasitgl, pdpd.customtext1, pdpd.customtext2, pdpd.customtext3, pdpd.customtext4, pdpd.customtext5, pdpd.customtext6, pdpd.customtext7, pdpd.customtext8, pdpd.customtext9, pdpd.customtext10, pdpd.customint1, pdpd.customint2, pdpd.customint3, pdpd.customint4, pdpd.customint5, pdpd.customint6, pdpd.customint7, pdpd.customint8, pdpd.customint9, pdpd.customint10, pdpd.customdbl1, pdpd.customdbl2, pdpd.customdbl3, pdpd.customdbl4, pdpd.customdbl5, pdpd.customdbl6, pdpd.customdbl7, pdpd.customdbl8, pdpd.customdbl9, pdpd.customdbl10, pdpd.customdate1, pdpd.customdate2, pdpd.customdate3, pdpd.customdate4, pdpd.customdate5, pdpd.customdate6, pdpd.customdate7, pdpd.customdate8, pdpd.customdate9, pdpd.customdate10, 0 as idplan, pdpd.idsodetail, pdpd.idbarang, pdpd.namabarang, pdpd.tipebarang, pdpd.jmljam, pdpd.idpdpdetail FROM m6_pdp_detail pdpd WHERE pdpd.idpdp = '" & result(4) & "') "
                    sql &= " ON DUPLICATE KEY UPDATE mesin= VALUES(mesin), tgl= VALUES(tgl), kelas= VALUES(kelas), subkelas= VALUES(subkelas), jml= VALUES(jml), satuan= VALUES(satuan), nilaisatuan= VALUES(nilaisatuan), jmlbarang= VALUES(jmlbarang), satuanbarang= VALUES(satuanbarang), matauang= VALUES(matauang), kurs= VALUES(kurs), harga= VALUES(harga), hpp= VALUES(hpp), cabang= VALUES(cabang), lokasi= VALUES(lokasi), gudang= VALUES(gudang), costcenter= VALUES(costcenter), divisi= VALUES(divisi), subdivisi= VALUES(subdivisi), proyek= VALUES(proyek), catatan= VALUES(catatan), urutan= VALUES(urutan), jmlrealisasi= VALUES(jmlrealisasi), statusrealisasi= VALUES(statusrealisasi), insertby= VALUES(insertby), isclose= VALUES(isclose), inputuser= VALUES(inputuser), inputtgl= VALUES(inputtgl), modifikasiuser= VALUES(modifikasiuser), modifikasitgl= VALUES(modifikasitgl), customtext1= VALUES(customtext1), customtext2= VALUES(customtext2), customtext3= VALUES(customtext3), customtext4= VALUES(customtext4), customtext5= VALUES(customtext5), customtext6= VALUES(customtext6), customtext7= VALUES(customtext7), customtext8= VALUES(customtext8), customtext9= VALUES(customtext9), customtext10= VALUES(customtext10), customint1= VALUES(customint1), customint2= VALUES(customint2), customint3= VALUES(customint3), customint4= VALUES(customint4), customint5= VALUES(customint5), customint6= VALUES(customint6), customint7= VALUES(customint7), customint8= VALUES(customint8), customint9= VALUES(customint9), customint10= VALUES(customint10), customdbl1= VALUES(customdbl1), customdbl2= VALUES(customdbl2), customdbl3= VALUES(customdbl3), customdbl4= VALUES(customdbl4), customdbl5= VALUES(customdbl5), customdbl6= VALUES(customdbl6), customdbl7= VALUES(customdbl7), customdbl8= VALUES(customdbl8), customdbl9= VALUES(customdbl9), customdbl10= VALUES(customdbl10), customdate1= VALUES(customdate1), customdate2= VALUES(customdate2), customdate3= VALUES(customdate3), customdate4= VALUES(customdate4), customdate5= VALUES(customdate5), customdate6= VALUES(customdate6), customdate7= VALUES(customdate7), customdate8= VALUES(customdate8), customdate9= VALUES(customdate9), customdate10= VALUES(customdate10), idplan= VALUES(idplan), idsodetail= VALUES(idsodetail), idbarang= VALUES(idbarang), namabarang= VALUES(namabarang), tipebarang= VALUES(tipebarang), jmljam= VALUES(jmljam), idpdpdetail= VALUES(idpdpdetail)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE JML REALISASI PLANNING PADA SO PRODUCTION
                    sql = "  UPDATE m5_so_production sop "
                    sql &= " JOIN (SELECT SUM(pdpd.jml) as jmltotal, pdpd.idsodetail, pdpd.idbarang FROM m6_pdp_detail pdpd WHERE pdpd.idpdp = '" & result(4) & "' GROUP BY pdpd.idsodetail, pdpd.idbarang) as pdp "
                    sql &= " ON sop.idso = pdp.idsodetail AND sop.idbarang = pdp.idbarang "
                    sql &= " SET sop.jmlpl = sop.jmlpl + pdp.jmltotal; "
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                Dim sumber As String = "PDP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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
    Public Function M6_PdpAllowanceSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

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


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idso(0) As Bigint, idbarang(1) As Bigint, jmlallowance(2) As Double, prosentaseallowance(3) As Double

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idso, idbarang, jmlallowance, prosentaseallowance

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlallowance", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "prosentaseallowance", AsEnumTypeData.AsDouble)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 4) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idso(0) As Bigint
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idso required numeric." : GoTo selesai
            End If
            'idbarang(1) As Bigint
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmlallowance(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - jmlallowance required numeric." : GoTo selesai
            End If
            'prosentaseallowance(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - prosentaseallowance required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idso(0) As Bigint
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idso can't be empty" : GoTo selesai
            End If

            'idbarang(1) As Bigint
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If

            'jmlallowance(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - jmlallowance can't be empty" : GoTo selesai
            End If

            'prosentaseallowance(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - prosentaseallowance can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idso~idbarang~jmlallowance~prosentaseallowance", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                sql = ""
                For Each dr1 As DataRow In dtdetail.Rows
                    sql &= " UPDATE m5_so_production SET jmlpi = '" & FixDouble(FxDB(dr1("jmlallowance"), 0)) & "', statuspi = '" & FixDouble(FxDB(dr1("prosentaseallowance"), 0)) & "' WHERE idso = '" & FixDouble(FxDB(dr1("idso"), 0)) & "' AND idbarang = '" & FixDouble(FxDB(dr1("idbarang"), 0)) & "'; "
                Next
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
    Public Function M6_PdpSimpan20200609(ByVal param As String) As String
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
        Dim isUpdate As Boolean

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
        'pdpid(0) As Integer, pdpcabang(1) As String, pdplokasi(2) As String, pdpgudangasal(3) As String, pdpgudangproduksi(4) As String, 
        'pdpgudangtujuan(5) As String, pdpsumber(6) As String, pdpjenis(7) As String, pdpautonotransaksi(8) As Integer, pdpnotransaksi(9) As String, 
        'pdptgl(10) As Date, pdpkodepa(11) As Integer, pdpbagianpdp(12) As Integer, pdpbagianpdpkontak(13) As String, pdptgldipakai(14) As Date, 
        'pdpestimasikerja(15) As String, pdpmatauang(16) As String, pdpkurs(17) As Double, pdptotalhargain(18) As Double, pdptotalhargaout(19) As Double, 
        'pdptotalhppin(20) As Double, pdptotalhppout(21) As Double, pdpuraian(22) As String, pdpcatatan(23) As String, pdpnoref(24) As String, 
        'pdptglnoref(25) As Date, pdpidbom(26) As Integer, pdpidpdr(27) As Integer, pdpidwo(28) As Integer, pdpidmrs(29) As Integer, 
        'pdpidmrn(30) As Integer, pdpstatus(31) As Integer, pdpstatussebelumnya(32) As Integer, pdpjmlrevisi(33) As Integer, pdpcetakanke(34) As Integer, 
        'pdpinputuser(35) As Integer, pdpinputtgl(36) As DateTime, pdpmodifikasiuser(37) As Integer, pdpmodifikasitgl(38) As DateTime, pdpposting(39) As Integer, 
        'pdptutupperiode(40) As Integer, pdpisclose(41) As Integer, pdpcustomtext1(42) As String, pdpcustomtext2(43) As String, pdpcustomtext3(44) As String, 
        'pdpcustomtext4(45) As String, pdpcustomtext5(46) As String, pdpcustomint1(47) As Integer, pdpcustomint2(48) As Integer, pdpcustomint3(49) As Integer, 
        'pdpcustomdbl1(50) As Double, pdpcustomdbl2(51) As Double, pdpcustomdbl3(52) As Double, pdpcustomdate1(53) As Date, pdpcustomdate2(54) As Date, 
        'pdpcustomdate3(55) As Date, pdpaktivitas(56) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pdpid, pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, 
        'pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, 
        'pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, 
        'pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, 
        'pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, 
        'pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdptutupperiode, pdpisclose, 
        'pdpcustomtext1, pdpcustomtext2, pdpcustomtext3, pdpcustomtext4, pdpcustomtext5, pdpcustomint1, pdpcustomint2, 
        'pdpcustomint3, pdpcustomdbl1, pdpcustomdbl2, pdpcustomdbl3, pdpcustomdate1, pdpcustomdate2, pdpcustomdate3, pdpaktivitas


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 56 And dataUtama.Length <> 57) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pdpid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pdpid required numeric." : GoTo selesai
        End If
        'pdpautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pdpautonotransaksi required numeric." : GoTo selesai
        End If
        'pdptgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "pdptgl required date." : GoTo selesai
        End If
        'pdpkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "pdpkodepa required numeric." : GoTo selesai
        End If
        'pdpbagianpdp(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "pdpbagianpdp required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "pdpbagianpdp can't be empty." : GoTo selesai
        End If
        'pdptgldipakai(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "pdptgldipakai required date." : GoTo selesai
        End If
        'pdpkurs(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "pdpkurs required numeric." : GoTo selesai
        End If
        'pdptotalhargain(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pdptotalhargain required numeric." : GoTo selesai
        End If
        'pdptotalhargaout(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pdptotalhargaout required numeric." : GoTo selesai
        End If
        'pdptotalhppin(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "pdptotalhppin required numeric." : GoTo selesai
        End If
        'pdptotalhppout(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "pdptotalhppout required numeric." : GoTo selesai
        End If
        'pdptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pdptglnoref required date." : GoTo selesai
        End If
        'pdpidbom(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "pdpidbom required numeric." : GoTo selesai
        End If
        'pdpidpdr(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pdpidpdr required numeric." : GoTo selesai
        End If
        'pdpidwo(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pdpidwo required numeric." : GoTo selesai
        End If
        'pdpidmrs(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "pdpidmrs required numeric." : GoTo selesai
        End If
        'pdpidmrn(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pdpidmrn required numeric." : GoTo selesai
        End If
        'pdpstatus(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "pdpstatus required numeric." : GoTo selesai
        End If
        'pdpstatussebelumnya(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pdpstatussebelumnya required numeric." : GoTo selesai
        End If
        'pdpjmlrevisi(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pdpjmlrevisi required numeric." : GoTo selesai
        End If
        'pdpcetakanke(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pdpcetakanke required numeric." : GoTo selesai
        End If
        'pdpinputuser(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pdpinputuser required numeric." : GoTo selesai
        End If
        'pdpinputtgl(36) As DateTime
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "pdpinputtgl required date." : GoTo selesai
        End If
        'pdpmodifikasiuser(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pdpmodifikasiuser required numeric." : GoTo selesai
        End If
        'pdpmodifikasitgl(38) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "pdpmodifikasitgl required date." : GoTo selesai
        End If
        'pdpposting(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pdpposting required numeric." : GoTo selesai
        End If
        'pdptutupperiode(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pdptutupperiode required numeric." : GoTo selesai
        End If
        'pdpisclose(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pdpisclose required numeric." : GoTo selesai
        End If
        'pdpcustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "pdpcustomint1 required numeric." : GoTo selesai
        End If
        'pdpcustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "pdpcustomint2 required numeric." : GoTo selesai
        End If
        'pdpcustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "pdpcustomint3 required numeric." : GoTo selesai
        End If
        'pdpcustomdbl1(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "pdpcustomdbl1 required numeric." : GoTo selesai
        End If
        'pdpcustomdbl2(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pdpcustomdbl2 required numeric." : GoTo selesai
        End If
        'pdpcustomdbl3(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pdpcustomdbl3 required numeric." : GoTo selesai
        End If
        'pdpcustomdate1(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "pdpcustomdate1 required date." : GoTo selesai
        End If
        'pdpcustomdate2(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "pdpcustomdate2 required date." : GoTo selesai
        End If
        'pdpcustomdate3(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "pdpcustomdate3 required date." : GoTo selesai
        End If

        If dataUtama.Length > 56 Then
            'pdpaktivitas(56) As Integer
            If (IsNumeric(dataUtama(56)) = False) Then
                result(2) = "pdpaktivitas required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===========================================


        'VALIDASI DATA UTAMA =======================================================
        'pdpcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pdpcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pdpcabang should not be more than 25 character." : GoTo selesai
        End If

        'pdplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pdplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pdplokasi should not be more than 25 character." : GoTo selesai
        End If

        'pdpgudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pdpgudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pdpgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'pdpgudangproduksi(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "pdpgudangproduksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "pdpgudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'pdpgudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "pdpgudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "pdpgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'pdpsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pdpsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "pdpsumber should not be more than 10 character." : GoTo selesai
        End If

        'pdpjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pdpjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "pdpjenis should not be more than 25 character." : GoTo selesai
        End If

        'pdpnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pdpnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "pdpnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pdptgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "pdptgl can't be empty" : GoTo selesai
        End If

        'pdptgldipakai(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "pdptgldipakai can't be empty" : GoTo selesai
        End If

        'pdpmatauang(16) As String
        If Len(dataUtama(16)) = 0 Then
            result(2) = "pdpmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(16)) > 25 Then
            result(2) = "pdpmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pdpkurs(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "pdpkurs can't be empty" : GoTo selesai
        End If

        'pdptotalhargain(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "pdptotalhargain can't be empty" : GoTo selesai
        End If

        'pdptotalhargaout(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pdptotalhargaout can't be empty" : GoTo selesai
        End If

        'pdptotalhppin(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "pdptotalhppin can't be empty" : GoTo selesai
        End If

        'pdptotalhppout(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "pdptotalhppout can't be empty" : GoTo selesai
        End If

        'pdptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pdptglnoref can't be empty" : GoTo selesai
        End If

        'pdpinputtgl(36) As DateTime
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pdpinputtgl can't be empty" : GoTo selesai
        End If

        'pdpmodifikasitgl(38) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pdpmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pdpcustomdbl1(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "pdpcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pdpcustomdbl2(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "pdpcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pdpcustomdbl3(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "pdpcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pdpcustomdate1(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "pdpcustomdate1 can't be empty" : GoTo selesai
        End If

        'pdpcustomdate2(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "pdpcustomdate2 can't be empty" : GoTo selesai
        End If

        'pdpcustomdate3(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "pdpcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pdpid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpgudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpbagianpdp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpbagianpdpkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpidbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidwo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpidmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdptutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdpaktivitas", AsEnumTypeData.AsInt64)
        If dataUtama.Length > 56 Then
            If AsDataTableTambahData(dtutama, "pdpid~pdpcabang~pdplokasi~pdpgudangasal~pdpgudangproduksi~pdpgudangtujuan~pdpsumber~pdpjenis~pdpautonotransaksi~pdpnotransaksi~pdptgl~pdpkodepa~pdpbagianpdp~pdpbagianpdpkontak~pdptgldipakai~pdpestimasikerja~pdpmatauang~pdpkurs~pdptotalhargain~pdptotalhargaout~pdptotalhppin~pdptotalhppout~pdpuraian~pdpcatatan~pdpnoref~pdptglnoref~pdpidbom~pdpidpdr~pdpidwo~pdpidmrs~pdpidmrn~pdpstatus~pdpstatussebelumnya~pdpjmlrevisi~pdpcetakanke~pdpinputuser~pdpinputtgl~pdpmodifikasiuser~pdpmodifikasitgl~pdpposting~pdptutupperiode~pdpisclose~pdpcustomtext1~pdpcustomtext2~pdpcustomtext3~pdpcustomtext4~pdpcustomtext5~pdpcustomint1~pdpcustomint2~pdpcustomint3~pdpcustomdbl1~pdpcustomdbl2~pdpcustomdbl3~pdpcustomdate1~pdpcustomdate2~pdpcustomdate3~pdpaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "pdpid~pdpcabang~pdplokasi~pdpgudangasal~pdpgudangproduksi~pdpgudangtujuan~pdpsumber~pdpjenis~pdpautonotransaksi~pdpnotransaksi~pdptgl~pdpkodepa~pdpbagianpdp~pdpbagianpdpkontak~pdptgldipakai~pdpestimasikerja~pdpmatauang~pdpkurs~pdptotalhargain~pdptotalhargaout~pdptotalhppin~pdptotalhppout~pdpuraian~pdpcatatan~pdpnoref~pdptglnoref~pdpidbom~pdpidpdr~pdpidwo~pdpidmrs~pdpidmrn~pdpstatus~pdpstatussebelumnya~pdpjmlrevisi~pdpcetakanke~pdpinputuser~pdpinputtgl~pdpmodifikasiuser~pdpmodifikasitgl~pdpposting~pdptutupperiode~pdpisclose~pdpcustomtext1~pdpcustomtext2~pdpcustomtext3~pdpcustomtext4~pdpcustomtext5~pdpcustomint1~pdpcustomint2~pdpcustomint3~pdpcustomdbl1~pdpcustomdbl2~pdpcustomdbl3~pdpcustomdate1~pdpcustomdate2~pdpcustomdate3~pdpaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'mesin(0) As String, tgl(1) As Date, kelas(2) As String, subkelas(3) As String, jml(4) As Double, 
        'satuan(5) As String, nilaisatuan(6) As Double, jmlbarang(7) As Double, satuanbarang(8) As String, matauang(9) As String, 
        'kurs(10) As Double, harga(11) As Double, hpp(12) As Double, cabang(13) As String, lokasi(14) As String, 
        'gudang(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, jmlrealisasi(22) As Double, statusrealisasi(23) As Integer, insertby(24) As Integer, 
        'isclose(25) As Integer, inputuser(26) As Bigint, inputtgl(27) As DateTime, modifikasiuser(28) As Bigint, modifikasitgl(29) As DateTime, 
        'customtext1(30) As String, customtext2(31) As String, customtext3(32) As String, customtext4(33) As String, customtext5(34) As String, 
        'customtext6(35) As String, customtext7(36) As String, customtext8(37) As String, customtext9(38) As String, customtext10(39) As String, 
        'customint1(40) As Integer, customint2(41) As Integer, customint3(42) As Integer, customint4(43) As Integer, customint5(44) As Integer, 
        'customint6(45) As Integer, customint7(46) As Integer, customint8(47) As Integer, customint9(48) As Integer, customint10(49) As Integer, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdbl4(53) As Double, customdbl5(54) As Double, 
        'customdbl6(55) As Double, customdbl7(56) As Double, customdbl8(57) As Double, customdbl9(58) As Double, customdbl10(59) As Double, 
        'customdate1(60) As Date, customdate2(61) As Date, customdate3(62) As Date, customdate4(63) As Date, customdate5(64) As Date, 
        'customdate6(65) As Date, customdate7(66) As Date, customdate8(67) As Date, customdate9(68) As Date, customdate10(69) As Date, 
        'idpdpdetail(70) As Bigint, idpdp(71) As Bigint, idsodetail(72) As Bigint, idbarang(73) As Bigint, namabarang(74) As String, 
        'tipebarang(75) As String, jmljam(76) As Double

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)

        Dim vTotalJam As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 77) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'tgl(1) As Date
            If (IsDate(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - tgl required date." : GoTo selesai
            End If
            'jml(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'jmlrealisasi(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlrealisasi required numeric." : GoTo selesai
            End If
            'statusrealisasi(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statusrealisasi required numeric." : GoTo selesai
            End If
            'insertby(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - insertby required numeric." : GoTo selesai
            End If
            'isclose(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'inputtgl(27) As DateTime
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - inputtgl required date." : GoTo selesai
            End If
            'modifikasitgl(29) As DateTime
            If (IsDate(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - modifikasitgl required date." : GoTo selesai
            End If
            'customint1(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customint4(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - customint4 required numeric." : GoTo selesai
            End If
            'customint5(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - customint5 required numeric." : GoTo selesai
            End If
            'customint6(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - customint6 required numeric." : GoTo selesai
            End If
            'customint7(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customint7 required numeric." : GoTo selesai
            End If
            'customint8(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customint8 required numeric." : GoTo selesai
            End If
            'customint9(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customint9 required numeric." : GoTo selesai
            End If
            'customint10(49) As Integer
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customint10 required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdbl4(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl4 required numeric." : GoTo selesai
            End If
            'customdbl5(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl5 required numeric." : GoTo selesai
            End If
            'customdbl6(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdbl6 required numeric." : GoTo selesai
            End If
            'customdbl7(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdbl7 required numeric." : GoTo selesai
            End If
            'customdbl8(57) As Double
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - customdbl8 required numeric." : GoTo selesai
            End If
            'customdbl9(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdbl9 required numeric." : GoTo selesai
            End If
            'customdbl10(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - customdbl10 required numeric." : GoTo selesai
            End If
            'customdate1(60) As Date
            If (IsDate(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(61) As Date
            If (IsDate(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(62) As Date
            If (IsDate(dataRowDetail(62)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'customdate4(63) As Date
            If (IsDate(dataRowDetail(63)) = False) Then
                result(2) = "Row : " & i & " - customdate4 required date." : GoTo selesai
            End If
            'customdate5(64) As Date
            If (IsDate(dataRowDetail(64)) = False) Then
                result(2) = "Row : " & i & " - customdate5 required date." : GoTo selesai
            End If
            'customdate6(65) As Date
            If (IsDate(dataRowDetail(65)) = False) Then
                result(2) = "Row : " & i & " - customdate6 required date." : GoTo selesai
            End If
            'customdate7(66) As Date
            If (IsDate(dataRowDetail(66)) = False) Then
                result(2) = "Row : " & i & " - customdate7 required date." : GoTo selesai
            End If
            'customdate8(67) As Date
            If (IsDate(dataRowDetail(67)) = False) Then
                result(2) = "Row : " & i & " - customdate8 required date." : GoTo selesai
            End If
            'customdate9(68) As Date
            If (IsDate(dataRowDetail(68)) = False) Then
                result(2) = "Row : " & i & " - customdate9 required date." : GoTo selesai
            End If
            'customdate10(69) As Date
            If (IsDate(dataRowDetail(69)) = False) Then
                result(2) = "Row : " & i & " - customdate10 required date." : GoTo selesai
            End If
            'idpdpdetail(70) As Bigint
            If (IsNumeric(dataRowDetail(70)) = False) Then
                result(2) = "Row : " & i & " - idpdpdetail required numeric." : GoTo selesai
            End If
            'idpdp(71) As Bigint
            If (IsNumeric(dataRowDetail(71)) = False) Then
                result(2) = "Row : " & i & " - idpdp required numeric." : GoTo selesai
            End If
            'idsodetail(72) As Bigint
            If (IsNumeric(dataRowDetail(72)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idbarang(73) As Bigint
            If (IsNumeric(dataRowDetail(73)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jmljam(76) As Double
            If (IsNumeric(dataRowDetail(76)) = False) Then
                result(2) = "Row : " & i & " - jmljam required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'mesin(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - mesin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 500 Then
                result(2) = "Row : " & i & " - mesin should not be more than 500 character." : GoTo selesai
            End If

            'tgl(1) As Date
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - tgl can't be empty" : GoTo selesai
            End If

            'kelas(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kelas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 500 Then
                result(2) = "Row : " & i & " - kelas should not be more than 500 character." : GoTo selesai
            End If

            'subkelas(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - subkelas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 500 Then
                result(2) = "Row : " & i & " - subkelas should not be more than 500 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 500 Then
                result(2) = "Row : " & i & " - satuan should not be more than 500 character." : GoTo selesai
            End If

            'nilaisatuan(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If

            'satuanbarang(8) As String
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(8)) > 500 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 500 character." : GoTo selesai
            End If

            'matauang(9) As String
            'If Len(dataRowDetail(9)) = 0 Then
            '    result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(9)) > 500 Then
                result(2) = "Row : " & i & " - matauang should not be more than 500 character." : GoTo selesai
            End If

            'kurs(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'jmlrealisasi(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlrealisasi can't be empty" : GoTo selesai
            End If

            'inputuser(26) As 
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - inputuser can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 20 Then
                result(2) = "Row : " & i & " - inputuser should not be more than 20 character." : GoTo selesai
            End If

            'inputtgl(27) As DateTime
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - inputtgl can't be empty" : GoTo selesai
            End If

            'modifikasiuser(28) As 
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - modifikasiuser can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(28)) > 20 Then
                result(2) = "Row : " & i & " - modifikasiuser should not be more than 20 character." : GoTo selesai
            End If

            'modifikasitgl(29) As DateTime
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - modifikasitgl can't be empty" : GoTo selesai
            End If

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdbl4(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
            End If

            'customdbl5(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
            End If

            'customdbl6(55) As Double
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdbl6 can't be empty" : GoTo selesai
            End If

            'customdbl7(56) As Double
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdbl7 can't be empty" : GoTo selesai
            End If

            'customdbl8(57) As Double
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdbl8 can't be empty" : GoTo selesai
            End If

            'customdbl9(58) As Double
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdbl9 can't be empty" : GoTo selesai
            End If

            'customdbl10(59) As Double
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdbl10 can't be empty" : GoTo selesai
            End If

            'customdate1(60) As Date
            If Len(dataRowDetail(60)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(61) As Date
            If Len(dataRowDetail(61)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(62) As Date
            If Len(dataRowDetail(62)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'customdate4(63) As Date
            If Len(dataRowDetail(63)) = 0 Then
                result(2) = "Row : " & i & " - customdate4 can't be empty" : GoTo selesai
            End If

            'customdate5(64) As Date
            If Len(dataRowDetail(64)) = 0 Then
                result(2) = "Row : " & i & " - customdate5 can't be empty" : GoTo selesai
            End If

            'customdate6(65) As Date
            If Len(dataRowDetail(65)) = 0 Then
                result(2) = "Row : " & i & " - customdate6 can't be empty" : GoTo selesai
            End If

            'customdate7(66) As Date
            If Len(dataRowDetail(66)) = 0 Then
                result(2) = "Row : " & i & " - customdate7 can't be empty" : GoTo selesai
            End If

            'customdate8(67) As Date
            If Len(dataRowDetail(67)) = 0 Then
                result(2) = "Row : " & i & " - customdate8 can't be empty" : GoTo selesai
            End If

            'customdate9(68) As Date
            If Len(dataRowDetail(68)) = 0 Then
                result(2) = "Row : " & i & " - customdate9 can't be empty" : GoTo selesai
            End If

            'customdate10(69) As Date
            If Len(dataRowDetail(69)) = 0 Then
                result(2) = "Row : " & i & " - customdate10 can't be empty" : GoTo selesai
            End If

            'idpdpdetail(70) As Bigint
            If Len(dataRowDetail(70)) = 0 Then
                result(2) = "Row : " & i & " - idpdpdetail can't be empty" : GoTo selesai
            End If

            'idpdp(71) As Bigint
            If Len(dataRowDetail(71)) = 0 Then
                result(2) = "Row : " & i & " - idpdp can't be empty" : GoTo selesai
            End If

            'idsodetail(72) As Bigint
            If Len(dataRowDetail(72)) = 0 Then
                result(2) = "Row : " & i & " - idsodetail can't be empty" : GoTo selesai
            End If

            'idbarang(73) As Bigint
            If Len(dataRowDetail(73)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If

            'jmljam(76) As Double
            If Len(dataRowDetail(76)) = 0 Then
                result(2) = "Row : " & i & " - jmljam can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            'VALIDASI JML JAM PRODUKSI PER TANGGAL PER MESIN TIDAK BOLEH LEBIH DARI 24 JAM
            vTotalJam = AsDataTableDSum(dtdetail, "jmljam", "mesin = '" & dataRowDetail(0) & "' AND tgl = '" & dataRowDetail(1) & "'")
            If vTotalJam > 24 Then
                result(2) = "Row : " & i & " - Total operating hours for machine : " & dataRowDetail(0) & ", date : " & dataRowDetail(1) & " = " & vTotalJam & " hours. It must be less than or equal to 24 hours." : GoTo selesai
            Else
                If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If
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
                Dim vModuleId As Integer = 6, vMenuId As Integer = 91
                Select Case drutama("pdpstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pdptgl")), AsFormatTanggal(drutama("pdptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                ''VALIDASI SIMPAN ========================================
                'If drutama("pdpstatus") = 2 Or drutama("pdpstatus") = 1 Or drutama("pdpstatus") = 8 Or drutama("pdpstatus") = 9 Or drutama("pdpstatus") = 10 Or drutama("pdpstatus") = 11 Then
                '    Dim rsValidasi As String
                '    'ValidasiSimpan
                '    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingWoIn, ftOutstandingWoIn, dtdetail2, ftExistOutstandingMrsOut, ftOutstandingMrsOut, "", "", ftExistStok, "", ftStokAvailable, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangproduksi")
                '    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("Pdpid")
                    notransaksi = drutama("Pdpnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(Pdpid), Pdpnotransaksi FROM M6_Pdp WHERE Pdpid='" & result(4) & "' AND pdpstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("pdpautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Pdpcabang"), drutama("Pdplokasi"), drutama("Pdpsumber"), drutama("Pdptgl"), drutama("Pdpsumber"), 6)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(Pdpid) FROM M6_Pdp WHERE Pdpnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_pdp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Pdp_HistorySimpan("" & paramSplit(0) & "★M6_Pdp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pdpsumber")) & "▼" & FixQuotes(drutama("pdpid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Pdp set pdpcabang  = '" & FixQuotes(drutama("pdpcabang")) & "', pdplokasi  = '" & FixQuotes(drutama("pdplokasi")) & "', pdpgudangasal  = '" & FixQuotes(drutama("pdpgudangasal")) & "', pdpgudangproduksi  = '" & FixQuotes(drutama("pdpgudangproduksi")) & "', pdpgudangtujuan  = '" & FixQuotes(drutama("pdpgudangtujuan")) & "', pdpsumber  = '" & FixQuotes(drutama("pdpsumber")) & "', pdpjenis  = '" & FixQuotes(drutama("pdpjenis")) & "', pdpautonotransaksi  = " & drutama("pdpautonotransaksi") & ", pdpnotransaksi  = '" & FixQuotes(notransaksi) & "', pdptgl  = '" & FixQuotes(AsFormatTanggal(drutama("pdptgl"))) & "', pdpkodepa  = " & drutama("pdpkodepa") & ", pdpbagianpdp  = " & drutama("pdpbagianpdp") & ", pdpbagianpdpkontak  = '" & FixQuotes(drutama("pdpbagianpdpkontak")) & "', pdptgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("pdptgldipakai"))) & "', pdpestimasikerja  = '" & FixQuotes(drutama("pdpestimasikerja")) & "', pdpmatauang  = '" & FixQuotes(drutama("pdpmatauang")) & "', pdpkurs  = '" & FixDouble(drutama("pdpkurs")) & "', pdptotalhargain  = '" & FixDouble(drutama("pdptotalhargain")) & "', pdptotalhargaout  = '" & FixDouble(drutama("pdptotalhargaout")) & "', pdptotalhppin  = '" & FixDouble(drutama("pdptotalhppin")) & "', pdptotalhppout  = '" & FixDouble(drutama("pdptotalhppout")) & "', pdpuraian  = '" & FixQuotes(drutama("pdpuraian")) & "', pdpcatatan  = '" & FixQuotes(drutama("pdpcatatan")) & "', pdpnoref  = '" & FixQuotes(drutama("pdpnoref")) & "', pdptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pdptglnoref"))) & "', pdpidbom  = " & drutama("pdpidbom") & ", pdpidpdr  = " & drutama("pdpidpdr") & ", pdpidwo  = " & drutama("pdpidwo") & ", pdpidmrs  = " & drutama("pdpidmrs") & ", pdpidmrn  = " & drutama("pdpidmrn") & ", pdpstatus  = " & drutama("pdpstatus") & ", pdpstatussebelumnya  = " & drutama("pdpstatussebelumnya") & ", pdpjmlrevisi  = pdpjmlrevisi+1, pdpcetakanke  = " & drutama("pdpcetakanke") & ", pdpmodifikasiuser  = " & drutama("pdpmodifikasiuser") & ", pdpmodifikasitgl  = NOW(), pdpposting  = 0, pdptutupperiode  = " & drutama("pdptutupperiode") & ", pdpcustomtext1  = '" & FixQuotes(drutama("pdpcustomtext1")) & "', pdpcustomtext2  = '" & FixQuotes(drutama("pdpcustomtext2")) & "', pdpcustomtext3  = '" & FixQuotes(drutama("pdpcustomtext3")) & "', pdpcustomtext4  = '" & FixQuotes(drutama("pdpcustomtext4")) & "', pdpcustomtext5  = '" & FixQuotes(drutama("pdpcustomtext5")) & "', pdpcustomint1  = " & drutama("pdpcustomint1") & ", pdpcustomint2  = " & drutama("pdpcustomint2") & ", pdpcustomint3  = " & drutama("pdpcustomint3") & ", pdpcustomdbl1  = '" & FixDouble(drutama("pdpcustomdbl1")) & "', pdpcustomdbl2  = '" & FixDouble(drutama("pdpcustomdbl2")) & "', pdpcustomdbl3  = '" & FixDouble(drutama("pdpcustomdbl3")) & "', pdpcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate1"))) & "', pdpcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate2"))) & "', pdpcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate3"))) & "', pdpaktivitas  = '" & FixDouble(drutama("pdpaktivitas")) & "' where pdpid = '" & drutama("pdpid") & "'"
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

                    If drutama("Pdpautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("Pdpcabang"), drutama("Pdplokasi"), drutama("Pdpsumber"), drutama("Pdptgl"), drutama("Pdpsumber"), 6)
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
                        notransaksi = drutama("Pdpnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(Pdpid) FROM m6_pdp WHERE Pdpnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Pdp (pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdptutupperiode, pdpisclose, pdpcustomtext1, pdpcustomtext2, pdpcustomtext3, pdpcustomtext4, pdpcustomtext5, pdpcustomint1, pdpcustomint2, pdpcustomint3, pdpcustomdbl1, pdpcustomdbl2, pdpcustomdbl3, pdpcustomdate1, pdpcustomdate2, pdpcustomdate3, pdpaktivitas) values('" & FixQuotes(drutama("pdpcabang")) & "', '" & FixQuotes(drutama("pdplokasi")) & "', '" & FixQuotes(drutama("pdpgudangasal")) & "', '" & FixQuotes(drutama("pdpgudangproduksi")) & "', '" & FixQuotes(drutama("pdpgudangtujuan")) & "', '" & FixQuotes(drutama("pdpsumber")) & "', '" & FixQuotes(drutama("pdpjenis")) & "', " & drutama("pdpautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdptgl"))) & "', " & drutama("pdpkodepa") & ", " & drutama("pdpbagianpdp") & ", '" & FixQuotes(drutama("pdpbagianpdpkontak")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdptgldipakai"))) & "', '" & FixQuotes(drutama("pdpestimasikerja")) & "', '" & FixQuotes(drutama("pdpmatauang")) & "', '" & FixDouble(drutama("pdpkurs")) & "', '" & FixDouble(drutama("pdptotalhargain")) & "', '" & FixDouble(drutama("pdptotalhargaout")) & "', '" & FixDouble(drutama("pdptotalhppin")) & "', '" & FixDouble(drutama("pdptotalhppout")) & "', '" & FixQuotes(drutama("pdpuraian")) & "', '" & FixQuotes(drutama("pdpcatatan")) & "', '" & FixQuotes(drutama("pdpnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdptglnoref"))) & "', " & drutama("pdpidbom") & ", " & drutama("pdpidpdr") & ", " & drutama("pdpidwo") & ", " & drutama("pdpidmrs") & ", " & drutama("pdpidmrn") & ", " & drutama("pdpstatus") & ", " & drutama("pdpstatussebelumnya") & ", " & drutama("pdpjmlrevisi") & ", " & drutama("pdpcetakanke") & ", " & drutama("pdpinputuser") & ", NOW(), " & drutama("pdpmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("pdptutupperiode") & ", " & drutama("pdpisclose") & ", '" & FixQuotes(drutama("pdpcustomtext1")) & "', '" & FixQuotes(drutama("pdpcustomtext2")) & "', '" & FixQuotes(drutama("pdpcustomtext3")) & "', '" & FixQuotes(drutama("pdpcustomtext4")) & "', '" & FixQuotes(drutama("pdpcustomtext5")) & "', " & drutama("pdpcustomint1") & ", " & drutama("pdpcustomint2") & ", " & drutama("pdpcustomint3") & ", '" & FixDouble(drutama("pdpcustomdbl1")) & "', '" & FixDouble(drutama("pdpcustomdbl2")) & "', '" & FixDouble(drutama("pdpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdpcustomdate3"))) & "', '" & FixDouble(drutama("pdpaktivitas")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select Pdpid from M6_Pdp where Pdpnotransaksi='" & notransaksi & "' AND Pdpinputuser= '" & userid & "' order by Pdpmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from m6_pdp_detail where idPdp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail1
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    Dim wsM0_NomorPDR As New m0_nomor
                    Dim rsNotransaksiPDR As String = "", notransaksiPDR As String = ""
                    Dim arrNotransaksiPDR(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                    For Each dr1 As DataRow In dtdetail.Rows
                        If drutama("pdpstatus") = 2 And Len(dr1("customtext10")) = 0 Then
                            rsNotransaksiPDR = wsM0_NomorPDR.M0_Notransaksi(drutama("pdpcabang"), drutama("pdplokasi"), "RC", AsFormatTanggal(dr1("tgl")), "RC", 6)
                            arrNotransaksiPDR = rsNotransaksiPDR.Split(sptSubParam)
                            'cek success generate notransaksi
                            If (arrNotransaksiPDR(0) = 1) Then
                                notransaksiPDR = arrNotransaksiPDR(2)
                                'tambah query update m0_nomor_next
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = arrNotransaksiPDR(3)
                                End With
                                objCmd.ExecuteNonQuery()
                                dr1("customtext10") = notransaksiPDR
                            Else
                                result(2) = arrNotransaksiPDR(1) : Trans.Rollback() : GoTo selesai
                            End If

                        End If
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("mesin")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl"))) & "', '" & FixQuotes(dr1("kelas")) & "', '" & FixQuotes(dr1("subkelas")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("insertby") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("inputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("inputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("modifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("modifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", " & dr1("customint4") & ", " & dr1("customint5") & ", " & dr1("customint6") & ", " & dr1("customint7") & ", " & dr1("customint8") & ", " & dr1("customint9") & ", " & dr1("customint10") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "', '" & FixQuotes(dr1("idpdpdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idsodetail")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jmljam")) & "')")
                    Next
                    sql = "Insert into M6_Pdp_Detail(mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam) values" & strValue2.ToString & ""
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


                'INSERT KE TABEL PLAN
                If drutama("pdpstatus") = 2 Then
                    sql = "INSERT INTO m6_production_planning(SELECT pdpd.mesin, pdpd.tgl, pdpd.kelas, pdpd.subkelas, pdpd.jml, pdpd.satuan, pdpd.nilaisatuan, pdpd.jmlbarang, pdpd.satuanbarang, pdpd.matauang, pdpd.kurs, pdpd.harga, pdpd.hpp, pdpd.cabang, pdpd.lokasi, pdpd.gudang, pdpd.costcenter, pdpd.divisi, pdpd.subdivisi, pdpd.proyek, pdpd.catatan, pdpd.urutan, pdpd.jmlrealisasi, pdpd.statusrealisasi, pdpd.insertby, pdpd.isclose, pdpd.inputuser, pdpd.inputtgl, pdpd.modifikasiuser, pdpd.modifikasitgl, pdpd.customtext1, pdpd.customtext2, pdpd.customtext3, pdpd.customtext4, pdpd.customtext5, pdpd.customtext6, pdpd.customtext7, pdpd.customtext8, pdpd.customtext9, pdpd.customtext10, pdpd.customint1, pdpd.customint2, pdpd.customint3, pdpd.customint4, pdpd.customint5, pdpd.customint6, pdpd.customint7, pdpd.customint8, pdpd.customint9, pdpd.customint10, pdpd.customdbl1, pdpd.customdbl2, pdpd.customdbl3, pdpd.customdbl4, pdpd.customdbl5, pdpd.customdbl6, pdpd.customdbl7, pdpd.customdbl8, pdpd.customdbl9, pdpd.customdbl10, pdpd.customdate1, pdpd.customdate2, pdpd.customdate3, pdpd.customdate4, pdpd.customdate5, pdpd.customdate6, pdpd.customdate7, pdpd.customdate8, pdpd.customdate9, pdpd.customdate10, 0 as idplan, pdpd.idsodetail, pdpd.idbarang, pdpd.namabarang, pdpd.tipebarang, pdpd.jmljam, pdpd.idpdpdetail FROM m6_pdp_detail pdpd WHERE pdpd.idpdp = '" & result(4) & "') "
                    sql &= " ON DUPLICATE KEY UPDATE mesin= VALUES(mesin), tgl= VALUES(tgl), kelas= VALUES(kelas), subkelas= VALUES(subkelas), jml= VALUES(jml), satuan= VALUES(satuan), nilaisatuan= VALUES(nilaisatuan), jmlbarang= VALUES(jmlbarang), satuanbarang= VALUES(satuanbarang), matauang= VALUES(matauang), kurs= VALUES(kurs), harga= VALUES(harga), hpp= VALUES(hpp), cabang= VALUES(cabang), lokasi= VALUES(lokasi), gudang= VALUES(gudang), costcenter= VALUES(costcenter), divisi= VALUES(divisi), subdivisi= VALUES(subdivisi), proyek= VALUES(proyek), catatan= VALUES(catatan), urutan= VALUES(urutan), jmlrealisasi= VALUES(jmlrealisasi), statusrealisasi= VALUES(statusrealisasi), insertby= VALUES(insertby), isclose= VALUES(isclose), inputuser= VALUES(inputuser), inputtgl= VALUES(inputtgl), modifikasiuser= VALUES(modifikasiuser), modifikasitgl= VALUES(modifikasitgl), customtext1= VALUES(customtext1), customtext2= VALUES(customtext2), customtext3= VALUES(customtext3), customtext4= VALUES(customtext4), customtext5= VALUES(customtext5), customtext6= VALUES(customtext6), customtext7= VALUES(customtext7), customtext8= VALUES(customtext8), customtext9= VALUES(customtext9), customtext10= VALUES(customtext10), customint1= VALUES(customint1), customint2= VALUES(customint2), customint3= VALUES(customint3), customint4= VALUES(customint4), customint5= VALUES(customint5), customint6= VALUES(customint6), customint7= VALUES(customint7), customint8= VALUES(customint8), customint9= VALUES(customint9), customint10= VALUES(customint10), customdbl1= VALUES(customdbl1), customdbl2= VALUES(customdbl2), customdbl3= VALUES(customdbl3), customdbl4= VALUES(customdbl4), customdbl5= VALUES(customdbl5), customdbl6= VALUES(customdbl6), customdbl7= VALUES(customdbl7), customdbl8= VALUES(customdbl8), customdbl9= VALUES(customdbl9), customdbl10= VALUES(customdbl10), customdate1= VALUES(customdate1), customdate2= VALUES(customdate2), customdate3= VALUES(customdate3), customdate4= VALUES(customdate4), customdate5= VALUES(customdate5), customdate6= VALUES(customdate6), customdate7= VALUES(customdate7), customdate8= VALUES(customdate8), customdate9= VALUES(customdate9), customdate10= VALUES(customdate10), idplan= VALUES(idplan), idsodetail= VALUES(idsodetail), idbarang= VALUES(idbarang), namabarang= VALUES(namabarang), tipebarang= VALUES(tipebarang), jmljam= VALUES(jmljam), idpdpdetail= VALUES(idpdpdetail)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                Dim sumber As String = "PDP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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
    Public Function M6_PdpUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "PDP", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pdptgl, Pdpnotransaksi, Pdpstatus FROM M6_Pdp WHERE Pdpid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pdpstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_pdp_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Pdp_HistorySimpan("" & paramSplit(0) & "★M6_Pdp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE PLAN
                sql = "DELETE p FROM m6_production_planning p JOIN m6_pdp_detail pdpd ON p.idpdpdetail = pdpd.idpdpdetail JOIN m6_pdp pdp ON pdpd.idpdp = pdp.pdpid AND pdp.pdpid = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'UPDATE JML REALISASI PLANNING PADA SO PRODUCTION
                sql = "  UPDATE m5_so_production sop "
                sql &= " JOIN (SELECT SUM(pdpd.jmlbarang) as jmltotal, pdpd.idsodetail, pdpd.idbarang FROM m6_pdp_detail pdpd WHERE pdpd.idpdp = '" & idtransaksi & "' GROUP BY pdpd.idsodetail, pdpd.idbarang) as pdp "
                sql &= " ON sop.idso = pdp.idsodetail AND sop.idbarang = pdp.idbarang "
                sql &= " SET sop.jmlpl = sop.jmlpl - pdp.jmltotal; "
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
            sql = "UPDATE M6_Pdp SET Pdpstatus = " & nilaiStatus & ", Pdpmodifikasiuser='" & userid & "', Pdpmodifikasitgl = NOW(), Pdpposting = 0, Pdppostingtgl = '1971-01-01 00:00:00', Pdpjmlrevisi = Pdpjmlrevisi + 1 WHERE Pdpid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_PdpSearch(PostWsSearch(paramSplit(0), "M6_PdpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_PdpUpdateStatus20200609(ByVal param As String) As String

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
            Dim sumber As String = "PDP", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pdptgl, Pdpnotransaksi, Pdpstatus FROM M6_Pdp WHERE Pdpid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pdpstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_pdp_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Pdp_HistorySimpan("" & paramSplit(0) & "★M6_Pdp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE PLAN
                sql = "DELETE p FROM m6_production_planning p JOIN m6_pdp_detail pdpd ON p.idpdpdetail = pdpd.idpdpdetail JOIN m6_pdp pdp ON pdpd.idpdp = pdp.pdpid AND pdp.pdpid = '" & idtransaksi & "'"
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
            sql = "UPDATE M6_Pdp SET Pdpstatus = " & nilaiStatus & ", Pdpmodifikasiuser='" & userid & "', Pdpmodifikasitgl = NOW(), Pdpposting = 0, Pdppostingtgl = '1971-01-01 00:00:00', Pdpjmlrevisi = Pdpjmlrevisi + 1 WHERE Pdpid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_PdpSearch(PostWsSearch(paramSplit(0), "M6_PdpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_PdpDelete(ByVal param As String) As String

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
            Dim sumber As String = "PDP", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pdpid, Pdpnotransaksi FROM M6_Pdp WHERE Pdpid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pdpcabang, pdplokasi, pdpsumber, pdpautonotransaksi, pdpnotransaksi, pdptgl"
            sql &= " FROM M6_pdp"
            sql &= " WHERE pdpid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pdpcabang")
                lokasi = dtNomorNext.Rows(0)("pdplokasi")
                sumber = dtNomorNext.Rows(0)("pdpsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pdpautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pdpnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pdptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_pdp_detail WHERE idPdp ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M6_Pdp WHERE Pdpid ='" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 6)
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
            Dim paramSearch As String = M6_PdpSearch(PostWsSearch(paramSplit(0), "M6_PdpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M6_PdpGetdataById(ByVal param As String) As String

        'M6_PdpGetdataById Utama --------------------------------------------------------
        'pdpid, pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, 
        'pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, 
        'pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, 
        'pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, 
        'pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, 
        'pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdppostingtgl, pdptutupperiode, 
        'pdpisclose, pdpcustomtext1, pdpcustomtext2, pdpcustomtext3, pdpcustomtext4, pdpcustomtext5, pdpcustomint1, 
        'pdpcustomint2, pdpcustomint3, pdpcustomdbl1, pdpcustomdbl2, pdpcustomdbl3, pdpcustomdate1, pdpcustomdate2, 
        'pdpcustomdate3, pdpcabangnama, pdplokasinama, pdpgudangasalnama, pdpgudangproduksinama, pdpgudangtujuannama, pdpjenisnama, pdpjeniswajibwo,
        'pdpbagianpdpkode, pdpbagianpdpnama, pdpestimasikerjanama, pdpnotransaksibom, pdpnotransaksipdr, pdpnotransaksiwo, pdpnotransaksimrs, 
        'pdpnotransaksimrn, pdpstatusnama, pdpstatussebelumnyanama, pdpinputusernama, pdpmodifikasiusernama, pdpaktivitas, pdpaktivitaskode, pdpaktivitasnama

        'M6_PdpGetdataById Detail --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        'M6_PdpGetdataById Allowance --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal


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
        Dim sumber As String = "PDP", strAllowance As String = "", ftAllowance As String = ""

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

        Dim NmMemcached As String = "aplikasi1-m6_pl~m6_pl_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "pdpi.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pdpid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pdpid = " & idtransaksi & " and " & Filter
        End If


        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_getdata")
        'sql = "select pdp.pdpid AS pdpid, pdp.pdpcabang AS pdpcabang, pdp.pdplokasi AS pdplokasi, pdp.pdpgudangasal AS pdpgudangasal,pdp.pdpgudangproduksi AS pdpgudangproduksi, pdp.pdpgudangtujuan AS pdpgudangtujuan, pdp.pdpsumber AS pdpsumber, pdp.pdpjenis AS pdpjenis, pdp.pdpautonotransaksi AS pdpautonotransaksi, pdp.pdpnotransaksi AS pdpnotransaksi, pdp.pdptgl AS pdptgl, pdp.pdpkodepa AS pdpkodepa, pdp.pdpbagianpdp AS pdpbagianpdp, pdp.pdpbagianpdpkontak AS pdpbagianpdpkontak, pdp.pdptgldipakai AS pdptgldipakai, pdp.pdpestimasikerja AS pdpestimasikerja, pdp.pdpmatauang AS pdpmatauang, pdp.pdpkurs AS pdpkurs, pdp.pdptotalhargain AS pdptotalhargain, pdp.pdptotalhargaout AS pdptotalhargaout, pdp.pdptotalhppin AS pdptotalhppin, pdp.pdptotalhppout AS pdptotalhppout, pdp.pdpuraian AS pdpuraian, pdp.pdpcatatan AS pdpcatatan, pdp.pdpnoref AS pdpnoref, pdp.pdptglnoref AS pdptglnoref, pdp.pdpidbom AS pdpidbom, pdp.pdpidpdr AS pdpidpdr, pdp.pdpidwo AS pdpidwo, pdp.pdpidmrs AS pdpidmrs, pdp.pdpidmrn AS pdpidmrn, pdp.pdpstatus AS pdpstatus, pdp.pdpstatussebelumnya AS pdpstatussebelumnya, pdp.pdpjmlrevisi AS pdpjmlrevisi, pdp.pdpcetakanke AS pdpcetakanke, pdp.pdpinputuser AS pdpinputuser, pdp.pdpinputtgl AS pdpinputtgl, pdp.pdpmodifikasiuser AS pdpmodifikasiuser, pdp.pdpmodifikasitgl AS pdpmodifikasitgl, pdp.pdpposting AS pdpposting, pdp.pdppostingtgl AS pdppostingtgl, pdp.pdptutupperiode AS pdptutupperiode, pdp.pdpisclose AS pdpisclose, pdp.pdpcustomtext1 AS pdpcustomtext1, pdp.pdpcustomtext2 AS pdpcustomtext2, pdp.pdpcustomtext3 AS pdpcustomtext3, pdp.pdpcustomtext4 AS pdpcustomtext4, pdp.pdpcustomtext5 AS pdpcustomtext5, pdp.pdpcustomint1 AS pdpcustomint1, pdp.pdpcustomint2 AS pdpcustomint2, pdp.pdpcustomint3 AS pdpcustomint3, pdp.pdpcustomdbl1 AS pdpcustomdbl1, pdp.pdpcustomdbl2 AS pdpcustomdbl2, pdp.pdpcustomdbl3 AS pdpcustomdbl3, pdp.pdpcustomdate1 AS pdpcustomdate1, pdp.pdpcustomdate2 AS pdpcustomdate2, pdp.pdpcustomdate3 AS pdpcustomdate3, br.bnama AS pdpcabangnama, lc.lnama AS pdplokasinama, '' AS pdpgudangasalnama, '' AS pdpgudangproduksinama, '' AS pdpgudangtujuannama, '' AS pdpjenisnama, '' AS pdpjeniswajibwo, c1.kkode AS pdpbagianpdpkode, c1.knama AS pdpbagianpdpnama, '' AS pdpestimasikerjanama, '' AS pdpnotransaksibom, '' AS pdpnotransaksipdr, '' AS pdpnotransaksiwo, '' AS pdpnotransaksimrs, '' AS pdpnotransaksimrn, st1.nama AS pdpstatusnama, st2.nama AS pdpstatussebelumnyanama, u1.unama AS pdpinputusernama, u2.unama AS pdpmodifikasiusernama, pdp.pdpaktivitas, '' as pdpaktivitaskode, '' as pdpaktivitasnama, pdpd.mesin, pdpd.tgl, pdpd.kelas, pdpd.subkelas, pdpd.jml, pdpd.satuan, pdpd.nilaisatuan, pdpd.jmlbarang, pdpd.satuanbarang, pdpd.matauang, pdpd.kurs, pdpd.harga, pdpd.hpp, pdpd.cabang, pdpd.lokasi, pdpd.gudang, pdpd.costcenter, pdpd.divisi, pdpd.subdivisi, pdpd.proyek, pdpd.catatan, pdpd.urutan, pdpd.jmlrealisasi, pdpd.statusrealisasi, pdpd.insertby, pdpd.isclose, pdpd.inputuser, pdpd.inputtgl, pdpd.modifikasiuser, pdpd.modifikasitgl, pdpd.customtext1, pdpd.customtext2, pdpd.customtext3, pdpd.customtext4, pdpd.customtext5, pdpd.customtext6, pdpd.customtext7, pdpd.customtext8, pdpd.customtext9, pdpd.customtext10, pdpd.customint1, pdpd.customint2, pdpd.customint3, pdpd.customint4, pdpd.customint5, pdpd.customint6, pdpd.customint7, pdpd.customint8, pdpd.customint9, pdpd.customint10, pdpd.customdbl1, pdpd.customdbl2, pdpd.customdbl3, pdpd.customdbl4, pdpd.customdbl5, pdpd.customdbl6, pdpd.customdbl7, pdpd.customdbl8, pdpd.customdbl9, pdpd.customdbl10, pdpd.customdate1, pdpd.customdate2, pdpd.customdate3, pdpd.customdate4, pdpd.customdate5, pdpd.customdate6, pdpd.customdate7, pdpd.customdate8, pdpd.customdate9, pdpd.customdate10, m.mnama as mesinnama, cl.cnama as kelasnama, sb.scnama as subkelasnama, u1.unama as inputusernama, u2.unama as modifikasiusernama, pdpd.idpdpdetail, pdpd.idpdp, pdpd.idsodetail, pdpd.idbarang, pdpd.namabarang, pdpd.tipebarang, pdpd.jmljam, so.sonotransaksi as sonotransaksi, i.bkode as bkode, 0 as idplan, i.bcustom12 as maxso, i.bcustom13 as kpj  from m6_pdp pdp join m6_pdp_detail pdpd on pdp.pdpid = pdpd.idpdp left join m1_branch br on pdp.pdpcabang = br.bkode left join m1_location lc on pdp.pdplokasi = lc.lkode left join m1_contact c1 on pdp.pdpbagianpdp = c1.kid left join m0_status st1 on pdp.pdpstatus = st1.kode left join m0_status st2 on pdp.pdpstatussebelumnya = st2.kode left join m0_user u1 on pdp.pdpinputuser = u1.userid left join m0_user u2 on pdp.pdpmodifikasiuser = u2.userid left join m1_machine m on pdpd.mesin = m.mkode left join m1_class cl on pdpd.kelas = cl.ckode left join m1_subclass sb on pdpd.subkelas = sb.sckode left join m5_so_detail sod on pdpd.idsodetail = sod.idsodetail left join m5_so so on sod.idso = so.soid left join m1_item i on pdpd.idbarang = i.bid"
        sql = "select pdp.pdpid AS pdpid, pdp.pdpcabang AS pdpcabang, pdp.pdplokasi AS pdplokasi, pdp.pdpgudangasal AS pdpgudangasal,pdp.pdpgudangproduksi AS pdpgudangproduksi, pdp.pdpgudangtujuan AS pdpgudangtujuan, pdp.pdpsumber AS pdpsumber, pdp.pdpjenis AS pdpjenis, pdp.pdpautonotransaksi AS pdpautonotransaksi, pdp.pdpnotransaksi AS pdpnotransaksi, pdp.pdptgl AS pdptgl, pdp.pdpkodepa AS pdpkodepa, pdp.pdpbagianpdp AS pdpbagianpdp, pdp.pdpbagianpdpkontak AS pdpbagianpdpkontak, pdp.pdptgldipakai AS pdptgldipakai, pdp.pdpestimasikerja AS pdpestimasikerja, pdp.pdpmatauang AS pdpmatauang, pdp.pdpkurs AS pdpkurs, pdp.pdptotalhargain AS pdptotalhargain, pdp.pdptotalhargaout AS pdptotalhargaout, pdp.pdptotalhppin AS pdptotalhppin, pdp.pdptotalhppout AS pdptotalhppout, pdp.pdpuraian AS pdpuraian, pdp.pdpcatatan AS pdpcatatan, pdp.pdpnoref AS pdpnoref, pdp.pdptglnoref AS pdptglnoref, pdp.pdpidbom AS pdpidbom, pdp.pdpidpdr AS pdpidpdr, pdp.pdpidwo AS pdpidwo, pdp.pdpidmrs AS pdpidmrs, pdp.pdpidmrn AS pdpidmrn, pdp.pdpstatus AS pdpstatus, pdp.pdpstatussebelumnya AS pdpstatussebelumnya, pdp.pdpjmlrevisi AS pdpjmlrevisi, pdp.pdpcetakanke AS pdpcetakanke, pdp.pdpinputuser AS pdpinputuser, pdp.pdpinputtgl AS pdpinputtgl, pdp.pdpmodifikasiuser AS pdpmodifikasiuser, pdp.pdpmodifikasitgl AS pdpmodifikasitgl, pdp.pdpposting AS pdpposting, pdp.pdppostingtgl AS pdppostingtgl, pdp.pdptutupperiode AS pdptutupperiode, pdp.pdpisclose AS pdpisclose, pdp.pdpcustomtext1 AS pdpcustomtext1, pdp.pdpcustomtext2 AS pdpcustomtext2, pdp.pdpcustomtext3 AS pdpcustomtext3, pdp.pdpcustomtext4 AS pdpcustomtext4, pdp.pdpcustomtext5 AS pdpcustomtext5, pdp.pdpcustomint1 AS pdpcustomint1, pdp.pdpcustomint2 AS pdpcustomint2, pdp.pdpcustomint3 AS pdpcustomint3, pdp.pdpcustomdbl1 AS pdpcustomdbl1, pdp.pdpcustomdbl2 AS pdpcustomdbl2, pdp.pdpcustomdbl3 AS pdpcustomdbl3, pdp.pdpcustomdate1 AS pdpcustomdate1, pdp.pdpcustomdate2 AS pdpcustomdate2, pdp.pdpcustomdate3 AS pdpcustomdate3, br.bnama AS pdpcabangnama, lc.lnama AS pdplokasinama, '' AS pdpgudangasalnama, '' AS pdpgudangproduksinama, '' AS pdpgudangtujuannama, '' AS pdpjenisnama, '' AS pdpjeniswajibwo, c1.kkode AS pdpbagianpdpkode, c1.knama AS pdpbagianpdpnama, '' AS pdpestimasikerjanama, '' AS pdpnotransaksibom, '' AS pdpnotransaksipdr, '' AS pdpnotransaksiwo, '' AS pdpnotransaksimrs, '' AS pdpnotransaksimrn, st1.nama AS pdpstatusnama, st2.nama AS pdpstatussebelumnyanama, u1.unama AS pdpinputusernama, u2.unama AS pdpmodifikasiusernama, pdp.pdpaktivitas, '' as pdpaktivitaskode, '' as pdpaktivitasnama, pdpd.mesin, pdpd.tgl, pdpd.kelas, pdpd.subkelas, pdpd.jml, pdpd.satuan, pdpd.nilaisatuan, pdpd.jmlbarang, pdpd.satuanbarang, pdpd.matauang, pdpd.kurs, pdpd.harga, pdpd.hpp, pdpd.cabang, pdpd.lokasi, pdpd.gudang, pdpd.costcenter, pdpd.divisi, pdpd.subdivisi, pdpd.proyek, pdpd.catatan, pdpd.urutan, pdpd.jmlrealisasi, pdpd.statusrealisasi, pdpd.insertby, pdpd.isclose, pdpd.inputuser, pdpd.inputtgl, pdpd.modifikasiuser, pdpd.modifikasitgl, pdpd.customtext1, pdpd.customtext2, pdpd.customtext3, pdpd.customtext4, pdpd.customtext5, pdpd.customtext6, pdpd.customtext7, pdpd.customtext8, pdpd.customtext9, pdpd.customtext10, pdpd.customint1, pdpd.customint2, pdpd.customint3, pdpd.customint4, pdpd.customint5, pdpd.customint6, pdpd.customint7, pdpd.customint8, pdpd.customint9, pdpd.customint10, pdpd.customdbl1, pdpd.customdbl2, pdpd.customdbl3, pdpd.customdbl4, pdpd.customdbl5, pdpd.customdbl6, pdpd.customdbl7, pdpd.customdbl8, pdpd.customdbl9, pdpd.customdbl10, pdpd.customdate1, pdpd.customdate2, pdpd.customdate3, pdpd.customdate4, pdpd.customdate5, pdpd.customdate6, pdpd.customdate7, pdpd.customdate8, pdpd.customdate9, pdpd.customdate10, m.mnama as mesinnama, cl.cnama as kelasnama, sb.scnama as subkelasnama, u1.unama as inputusernama, u2.unama as modifikasiusernama, pdpd.idpdpdetail, pdpd.idpdp, pdpd.idsodetail, pdpd.idbarang, pdpd.namabarang, pdpd.tipebarang, pdpd.jmljam, so.sonotransaksi as sonotransaksi, i.bkode as bkode, 0 as idplan, i.bcustom12 as maxso, i.bcustom13 as kpj  from m6_pdp pdp join m6_pdp_detail pdpd on pdp.pdpid = pdpd.idpdp left join m1_branch br on pdp.pdpcabang = br.bkode left join m1_location lc on pdp.pdplokasi = lc.lkode left join m1_contact c1 on pdp.pdpbagianpdp = c1.kid left join m0_status st1 on pdp.pdpstatus = st1.kode left join m0_status st2 on pdp.pdpstatussebelumnya = st2.kode left join m0_user u1 on pdp.pdpinputuser = u1.userid left join m0_user u2 on pdp.pdpmodifikasiuser = u2.userid left join m1_machine m on pdpd.mesin = m.mkode left join m1_class cl on pdpd.kelas = cl.ckode left join m1_subclass sb on pdpd.subkelas = sb.sckode left join m5_so so on pdpd.idsodetail = so.soid left join m1_item i on pdpd.idbarang = i.bid"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pdpid"), 0), sptField,
                     FxDB(drutama("pdpcabang"), ""), sptField,
                     FxDB(drutama("pdplokasi"), ""), sptField,
                     FxDB(drutama("pdpgudangasal"), ""), sptField,
                     FxDB(drutama("pdpgudangproduksi"), ""), sptField,
                     FxDB(drutama("pdpgudangtujuan"), ""), sptField,
                     FxDB(drutama("pdpsumber"), ""), sptField,
                     FxDB(drutama("pdpjenis"), ""), sptField,
                     FxDB(drutama("pdpautonotransaksi"), 0), sptField,
                     FxDB(drutama("pdpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pdpkodepa"), 0), sptField,
                     FxDB(drutama("pdpbagianpdp"), 0), sptField,
                     FxDB(drutama("pdpbagianpdpkontak"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdptgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("pdpestimasikerja"), ""), sptField,
                     FxDB(drutama("pdpmatauang"), ""), sptField,
                     FxDB(drutama("pdpkurs"), 0), sptField,
                     FxDB(drutama("pdptotalhargain"), 0), sptField,
                     FxDB(drutama("pdptotalhargaout"), 0), sptField,
                     FxDB(drutama("pdptotalhppin"), 0), sptField,
                     FxDB(drutama("pdptotalhppout"), 0), sptField,
                     FxDB(drutama("pdpuraian"), ""), sptField,
                     FxDB(drutama("pdpcatatan"), ""), sptField,
                     FxDB(drutama("pdpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pdpidbom"), 0), sptField,
                     FxDB(drutama("pdpidpdr"), 0), sptField,
                     FxDB(drutama("pdpidwo"), 0), sptField,
                     FxDB(drutama("pdpidmrs"), 0), sptField,
                     FxDB(drutama("pdpidmrn"), 0), sptField,
                     FxDB(drutama("pdpstatus"), 0), sptField,
                     FxDB(drutama("pdpstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pdpjmlrevisi"), 0), sptField,
                     FxDB(drutama("pdpcetakanke"), 0), sptField,
                     FxDB(drutama("pdpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdptutupperiode"), 0), sptField,
                     FxDB(drutama("pdpisclose"), 0), sptField,
                     FxDB(drutama("pdpcustomtext1"), ""), sptField,
                     FxDB(drutama("pdpcustomtext2"), ""), sptField,
                     FxDB(drutama("pdpcustomtext3"), ""), sptField,
                     FxDB(drutama("pdpcustomtext4"), ""), sptField,
                     FxDB(drutama("pdpcustomtext5"), ""), sptField,
                     FxDB(drutama("pdpcustomint1"), 0), sptField,
                     FxDB(drutama("pdpcustomint2"), 0), sptField,
                     FxDB(drutama("pdpcustomint3"), 0), sptField,
                     FxDB(drutama("pdpcustomdbl1"), 0), sptField,
                     FxDB(drutama("pdpcustomdbl2"), 0), sptField,
                     FxDB(drutama("pdpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pdpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pdpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pdpcabangnama"), ""), sptField,
                     FxDB(drutama("pdplokasinama"), ""), sptField,
                     FxDB(drutama("pdpgudangasalnama"), ""), sptField,
                     FxDB(drutama("pdpgudangproduksinama"), ""), sptField,
                     FxDB(drutama("pdpgudangtujuannama"), ""), sptField,
                     FxDB(drutama("pdpjenisnama"), ""), sptField,
                     FxDB(drutama("pdpjeniswajibwo"), ""), sptField,
                     FxDB(drutama("pdpbagianpdpkode"), ""), sptField,
                     FxDB(drutama("pdpbagianpdpnama"), ""), sptField,
                     FxDB(drutama("pdpestimasikerjanama"), ""), sptField,
                     FxDB(drutama("pdpnotransaksibom"), ""), sptField,
                     FxDB(drutama("pdpnotransaksipdr"), ""), sptField,
                     FxDB(drutama("pdpnotransaksiwo"), ""), sptField,
                     FxDB(drutama("pdpnotransaksimrs"), ""), sptField,
                     FxDB(drutama("pdpnotransaksimrn"), ""), sptField,
                     FxDB(drutama("pdpstatusnama"), ""), sptField,
                     FxDB(drutama("pdpstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pdpinputusernama"), ""), sptField,
                     FxDB(drutama("pdpmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("pdpaktivitas"), 0), sptField,
                     FxDB(drutama("pdpaktivitaskode"), ""), sptField,
                     FxDB(drutama("pdpaktivitasnama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)

                'TAMBAH FILTER UNTUK DATA ALLOWANCE
                If Len(ftAllowance) > 0 Then
                    ftAllowance = ftAllowance & " OR "
                End If
                ftAllowance = ftAllowance & " (so.soid = '" & FixDouble(FxDB(dr("idsodetail"), "0")) & "' AND sop.idbarang = '" & FixDouble(FxDB(dr("idbarang"), "0")) & "') "

            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)


            'AMBIL DATA ALLOWANCE
            If Len(ftAllowance) > 0 Then
                sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal "
                sql &= " FROM m5_so_production sop "
                sql &= " JOIN m5_so so ON sop.idso = so.soid "
                sql &= " AND (" & ftAllowance & ")"
                sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
                sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode"
                Dim dtallowance As New DataTable
                dtallowance = AmbilData("aplikasi1-m1_allowance", "", "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", sql) ' Ambil data ke databases
                For Each dr As DataRow In dtallowance.Rows
                    strAllowance = String.Concat(strAllowance,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(dr("jml"), 0), sptField,
                             FxDB(dr("jmljam"), 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(dr("jmlbarang"), 0), sptField,
                             FxDB(dr("jmlsisa"), 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)
                Next
                If strAllowance.Length > 0 Then strAllowance = strAllowance.Substring(0, strAllowance.Length - sptRow.Length) Else strAllowance = strAllowance

            End If


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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, strAllowance)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdpid, pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdppostingtgl, pdptutupperiode, pdpisclose, pdpcustomtext1, pdpcustomtext2, pdpcustomtext3, pdpcustomtext4, pdpcustomtext5, pdpcustomint1, pdpcustomint2, pdpcustomint3, pdpcustomdbl1, pdpcustomdbl2, pdpcustomdbl3, pdpcustomdate1, pdpcustomdate2, pdpcustomdate3, pdpcabangnama, pdplokasinama, pdpgudangasalnama, pdpgudangproduksinama, pdpgudangtujuannama, pdpjenisnama, pdpjeniswajibwo, pdpbagianpdpkode, pdpbagianpdpnama, pdpestimasikerjanama, pdpnotransaksibom, pdpnotransaksipdr, pdpnotransaksiwo, pdpnotransaksimrs, pdpnotransaksimrn, pdpstatusnama, pdpstatussebelumnyanama, pdpinputusernama, pdpmodifikasiusernama, pdpaktivitas, pdpaktivitaskode, pdpaktivitasnama" & sptSubParam & "mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj" & sptSubParam & "bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpSearch(ByVal param As String) As String
        'M6_PdSearch --------------------------------------------------------
        'pdpid, pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, 
        'pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, 
        'pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, 
        'pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, 
        'pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, 
        'pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdppostingtgl, pdptutupperiode, 
        'pdpisclose, pdpcabangnama, pdplokasinama, pdpgudangasalnama, pdpgudangproduksinama, pdpgudangtujuannama, pdpjenisnama, 
        'pdpbagianpdpkode, pdpbagianpdpnama, pdpestimasikerjanama, pdpnotransaksibom, pdpnotransaksipdr, pdpnotransaksiwo, pdpnotransaksimrs, 
        'pdpnotransaksimrn, pdpstatusnama, pdpstatussebelumnyanama, pdpinputusernama, pdpmodifikasiusernama, pdpaktivitas, pdpaktivitaskode, pdpaktivitasnama

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        sql = "select pdp.pdpid AS pdpid, pdp.pdpcabang AS pdpcabang, pdp.pdplokasi AS pdplokasi, pdp.pdpgudangasal AS pdpgudangasal, pdp.pdpgudangproduksi AS pdpgudangproduksi, pdp.pdpgudangtujuan AS pdpgudangtujuan, pdp.pdpsumber AS pdpsumber, pdp.pdpjenis AS pdpjenis, pdp.pdpautonotransaksi AS pdpautonotransaksi, pdp.pdpnotransaksi AS pdpnotransaksi, pdp.pdptgl AS pdptgl, pdp.pdpkodepa AS pdpkodepa, pdp.pdpbagianpdp AS pdpbagianpdp, pdp.pdpbagianpdpkontak AS pdpbagianpdpkontak, pdp.pdptgldipakai AS pdptgldipakai, pdp.pdpestimasikerja AS pdpestimasikerja, pdp.pdpmatauang AS pdpmatauang, pdp.pdpkurs AS pdpkurs, pdp.pdptotalhargain AS pdptotalhargain, pdp.pdptotalhargaout AS pdptotalhargaout, pdp.pdptotalhppin AS pdptotalhppin, pdp.pdptotalhppout AS pdptotalhppout, pdp.pdpuraian AS pdpuraian, pdp.pdpcatatan AS pdpcatatan, pdp.pdpnoref AS pdpnoref, pdp.pdptglnoref AS pdptglnoref, pdp.pdpidbom AS pdpidbom, pdp.pdpidpdr AS pdpidpdr, pdp.pdpidwo AS pdpidwo, pdp.pdpidmrs AS pdpidmrs, pdp.pdpidmrn AS pdpidmrn, pdp.pdpstatus AS pdpstatus, pdp.pdpstatussebelumnya AS pdpstatussebelumnya, pdp.pdpjmlrevisi AS pdpjmlrevisi, pdp.pdpcetakanke AS pdpcetakanke, pdp.pdpinputuser AS pdpinputuser, pdp.pdpinputtgl AS pdpinputtgl, pdp.pdpmodifikasiuser AS pdpmodifikasiuser, pdp.pdpmodifikasitgl AS pdpmodifikasitgl, pdp.pdpposting AS pdpposting, pdp.pdppostingtgl AS pdppostingtgl, pdp.pdptutupperiode AS pdptutupperiode, pdp.pdpisclose AS pdpisclose, br.bnama AS pdpcabangnama, lc.lnama AS pdplokasinama, '' AS pdpgudangasalnama, '' AS pdpgudangproduksinama, '' AS pdpgudangtujuannama, '' AS pdpjenisnama, c1.kkode AS pdpbagianpdpkode, c1.knama AS pdpbagianpdpnama, '' AS pdpestimasikerjanama, '' AS pdpnotransaksibom, '' AS pdpnotransaksipdr, '' AS pdpnotransaksiwo, '' AS pdpnotransaksimrs, '' AS pdpnotransaksimrn, st1.nama AS pdpstatusnama, st2.nama AS pdpstatussebelumnyanama, u1.unama AS pdpinputusernama, u2.unama AS pdpmodifikasiusernama, pdp.pdpaktivitas, '' as pdpaktivitaskode, '' as pdpaktivitasnama from m6_pdp pdp left join m1_branch br on pdp.pdpcabang = br.bkode left join m1_location lc on pdp.pdplokasi = lc.lkode left join m1_contact c1 on pdp.pdpbagianpdp = c1.kid left join m0_status st1 on pdp.pdpstatus = st1.kode left join m0_status st2 on pdp.pdpstatussebelumnya = st2.kode left join m0_user u1 on pdp.pdpinputuser = u1.userid left join m0_user u2 on pdp.pdpmodifikasiuser = u2.userid "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pdpid"), 0), sptField,
                     FxDB(dr("pdpcabang"), ""), sptField,
                     FxDB(dr("pdplokasi"), ""), sptField,
                     FxDB(dr("pdpgudangasal"), ""), sptField,
                     FxDB(dr("pdpgudangproduksi"), ""), sptField,
                     FxDB(dr("pdpgudangtujuan"), ""), sptField,
                     FxDB(dr("pdpsumber"), ""), sptField,
                     FxDB(dr("pdpjenis"), ""), sptField,
                     FxDB(dr("pdpautonotransaksi"), 0), sptField,
                     FxDB(dr("pdpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdptgl"), ""), formatTgl), sptField,
                     FxDB(dr("pdpkodepa"), 0), sptField,
                     FxDB(dr("pdpbagianpdp"), 0), sptField,
                     FxDB(dr("pdpbagianpdpkontak"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdptgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("pdpestimasikerja"), ""), sptField,
                     FxDB(dr("pdpmatauang"), ""), sptField,
                     FxDB(dr("pdpkurs"), 0), sptField,
                     FxDB(dr("pdptotalhargain"), 0), sptField,
                     FxDB(dr("pdptotalhargaout"), 0), sptField,
                     FxDB(dr("pdptotalhppin"), 0), sptField,
                     FxDB(dr("pdptotalhppout"), 0), sptField,
                     FxDB(dr("pdpuraian"), ""), sptField,
                     FxDB(dr("pdpcatatan"), ""), sptField,
                     FxDB(dr("pdpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pdpidbom"), 0), sptField,
                     FxDB(dr("pdpidpdr"), 0), sptField,
                     FxDB(dr("pdpidwo"), 0), sptField,
                     FxDB(dr("pdpidmrs"), 0), sptField,
                     FxDB(dr("pdpidmrn"), 0), sptField,
                     FxDB(dr("pdpstatus"), 0), sptField,
                     FxDB(dr("pdpstatussebelumnya"), 0), sptField,
                     FxDB(dr("pdpjmlrevisi"), 0), sptField,
                     FxDB(dr("pdpcetakanke"), 0), sptField,
                     FxDB(dr("pdpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdptutupperiode"), 0), sptField,
                     FxDB(dr("pdpisclose"), 0), sptField,
                     FxDB(dr("pdpcabangnama"), ""), sptField,
                     FxDB(dr("pdplokasinama"), ""), sptField,
                     FxDB(dr("pdpgudangasalnama"), ""), sptField,
                     FxDB(dr("pdpgudangproduksinama"), ""), sptField,
                     FxDB(dr("pdpgudangtujuannama"), ""), sptField,
                     FxDB(dr("pdpjenisnama"), ""), sptField,
                     FxDB(dr("pdpbagianpdpkode"), ""), sptField,
                     FxDB(dr("pdpbagianpdpnama"), ""), sptField,
                     FxDB(dr("pdpestimasikerjanama"), ""), sptField,
                     FxDB(dr("pdpnotransaksibom"), ""), sptField,
                     FxDB(dr("pdpnotransaksipdr"), ""), sptField,
                     FxDB(dr("pdpnotransaksiwo"), ""), sptField,
                     FxDB(dr("pdpnotransaksimrs"), ""), sptField,
                     FxDB(dr("pdpnotransaksimrn"), ""), sptField,
                     FxDB(dr("pdpstatusnama"), ""), sptField,
                     FxDB(dr("pdpstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pdpinputusernama"), ""), sptField,
                     FxDB(dr("pdpmodifikasiusernama"), ""), sptField,
                     FxDB(dr("pdpaktivitas"), 0), sptField,
                     FxDB(dr("pdpaktivitaskode"), ""), sptField,
                     FxDB(dr("pdpaktivitasnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdpid, pdpcabang, pdplokasi, pdpgudangasal, pdpgudangproduksi, pdpgudangtujuan, pdpsumber, pdpjenis, pdpautonotransaksi, pdpnotransaksi, pdptgl, pdpkodepa, pdpbagianpdp, pdpbagianpdpkontak, pdptgldipakai, pdpestimasikerja, pdpmatauang, pdpkurs, pdptotalhargain, pdptotalhargaout, pdptotalhppin, pdptotalhppout, pdpuraian, pdpcatatan, pdpnoref, pdptglnoref, pdpidbom, pdpidpdr, pdpidwo, pdpidmrs, pdpidmrn, pdpstatus, pdpstatussebelumnya, pdpjmlrevisi, pdpcetakanke, pdpinputuser, pdpinputtgl, pdpmodifikasiuser, pdpmodifikasitgl, pdpposting, pdppostingtgl, pdptutupperiode, pdpisclose, pdpcabangnama, pdplokasinama, pdpgudangasalnama, pdpgudangproduksinama, pdpgudangtujuannama, pdpjenisnama, pdpbagianpdpkode, pdpbagianpdpnama, pdpestimasikerjanama, pdpnotransaksibom, pdpnotransaksipdr, pdpnotransaksiwo, pdpnotransaksimrs, pdpnotransaksimrn, pdpstatusnama, pdpstatussebelumnyanama, pdpinputusernama, pdpmodifikasiusernama, pdpaktivitas, pdpaktivitaskode, pdpaktivitasnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTerkait(ByVal param As String) As String
        'M6_PdpTerkait --------------------------------------------------------
        'pdpid, pdpnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "pdpid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND pdpid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "pdpid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m6_pdp_terkait(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m6_bom_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each pl As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(pl("pdpid"), 0), sptField,
                     FxDB(pl("pdpnotransaksi"), ""), sptField,
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
            result(2) = "Related PDP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdpid, pdpnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpGenerate20200324(ByVal param As String) As String
        'M6_PdpGenerate --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8 And dataUtama.Length <> 9) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String,
        'minPlan(8) As Double

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""
        Dim minPlan As Double = 0


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
            'Else
            '    result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
            'Else
            '    result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            'If kategoriAwal <> kategoriAkhir Then
            '    result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            'End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
            'Else
            '    result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            'If kelompokAwal <> kelompokAkhir Then
            '    result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            'End If
        Else
            kelompokAkhir = kelompokAwal
        End If

        'minPlan(8) As Double
        If dataUtama.Length > 8 Then
            If IsNumeric(dataUtama(8)) Then
                minPlan = dataUtama(8)
            Else
                result(2) = "Minimum Planning Qty required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "mesinnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "inputusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idplan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "maxso", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kpj", AsEnumTypeData.AsDouble)

        Dim dtSO As New DataTable, dtPlot As New DataTable, dtPlan As New DataTable, dtSOTemp As New DataTable, dtPlotTemp As New DataTable

        'AMBIL SO
        sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, sop.jml, sop.satuan, sop.nilaisatuan, sop.jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql &= " FROM m5_so_production sop "
        sql &= " JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2"
        sql &= " AND so.sotgl < '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
        sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
        sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
        sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        dtSO = AsDataTableAmbilDariDB(sql)
        If dtSO.Rows.Count > 0 Then

            'AMBIL PLOT MESIN
            sql = "  SELECT mp.mesin, mp.tgl, mp.kelas, mp.subkelas, mp.jml, m.mnama FROM m6_machine_plotting mp JOIN m1_machine m ON mp.mesin = m.mkode "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            'filter mesin
            If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                sql &= " AND mp.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin >= '" & FixQuotes(mesinAwal) & "' "
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin <= '" & FixQuotes(mesinAkhir) & "' "
            End If
            'filter kelas
            If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                sql &= " AND mp.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas >= '" & FixQuotes(kategoriAwal) & "' "
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
            End If
            'filter subkelas
            If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                sql &= " AND mp.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
            End If
            sql &= " ORDER BY mp.mesin, mp.tgl, mp.kelas, mp.subkelas "
            dtPlot = AsDataTableAmbilDariDB(sql)
            If dtPlot.Rows.Count > 0 Then

                'AMBIL PLAN
                sql = "  SELECT pl.mesin, pl.tgl, pl.kelas, pl.subkelas, pl.jml, pl.satuan, pl.nilaisatuan, pl.jmlbarang, pl.satuanbarang, pl.matauang, pl.kurs, pl.harga, pl.hpp, pl.cabang, pl.lokasi, pl.gudang, pl.costcenter, pl.divisi, pl.subdivisi, pl.proyek, pl.catatan, pl.urutan, pl.jmlrealisasi, pl.statusrealisasi, pl.insertby, pl.isclose, pl.inputuser, pl.inputtgl, pl.modifikasiuser, pl.modifikasitgl, pl.customtext1, pl.customtext2, pl.customtext3, pl.customtext4, pl.customtext5, pl.customtext6, pl.customtext7, pl.customtext8, pl.customtext9, pl.customtext10, pl.customint1, pl.customint2, pl.customint3, pl.customint4, pl.customint5, pl.customint6, pl.customint7, pl.customint8, pl.customint9, pl.customint10,  pl.customdbl1, pl.customdbl2, pl.customdbl3, pl.customdbl4, pl.customdbl5, pl.customdbl6, pl.customdbl7, pl.customdbl8, pl.customdbl9, pl.customdbl10, pl.customdate1, pl.customdate2, pl.customdate3, pl.customdate4, pl.customdate5, pl.customdate6, pl.customdate7, pl.customdate8, pl.customdate9, pl.customdate10, IFNULL(m.mnama,'') as mesinnama, IFNULL(cl.cnama,'') as kelasnama, IFNULL(sc.scnama,'') as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama, pl.idpdpdetail, IFNULL(pdpd.idpdp,0) as idpdp, pl.idsodetail, pl.idbarang, pl.namabarang, pl.tipebarang, pl.jmljam, IFNULL(so.sonotransaksi,'') as sonotransaksi, IFNULL(i.bkode,'') as bkode, pl.idplan, i.bcustom12 as maxso, i.bcustom13 as kpj "
                sql &= " FROM m6_production_planning pl "
                sql &= " LEFT JOIN m1_machine m ON pl.mesin = m.mkode "
                sql &= " LEFT JOIN m1_class cl on pl.kelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON pl.subkelas = sc.sckode "
                sql &= " LEFT JOIN m0_user u1 ON pl.inputuser = u1.userid "
                sql &= " LEFT JOIN m0_user u2 ON pl.modifikasiuser = u2.userid "
                sql &= " LEFT JOIN m6_pdp_detail pdpd ON pl.idpdpdetail = pdpd.idpdpdetail "
                sql &= " LEFT JOIN m5_so so ON pl.idsodetail = so.soid "
                sql &= " LEFT JOIN m1_item i ON pl.idbarang = i.bid "
                'sql &= " WHERE (pl.insertby = 1 OR pl.isclose = 1) "
                sql &= " WHERE pl.tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
                'filter mesin
                If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                    sql &= " AND pl.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin >= '" & FixQuotes(mesinAwal) & "' "
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin <= '" & FixQuotes(mesinAkhir) & "' "
                End If
                'filter kelas
                If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                    sql &= " AND pl.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas >= '" & FixQuotes(kategoriAwal) & "' "
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
                End If
                'filter subkelas
                If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                    sql &= " AND pl.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
                End If
                sql &= " ORDER BY pl.tgl, pl.mesin, pl.urutan, pl.kelas, pl.subkelas, so.sonotransaksi, i.bkode "
                dtPlan = AsDataTableAmbilDariDB(sql)
                If dtPlan.Rows.Count > 0 Then
                    Dim vRow As Double = 1, vJmlJam As Double = 0, vJmlBarang As Double = 0, vJmlProd As Double = 0
                    For Each dr1 As DataRow In dtPlan.Rows
                        'INSERT KE DATATABLE DETAIL
                        If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dr1("mesin") & "~" & dr1("tgl") & "~" & dr1("kelas") & "~" & dr1("subkelas") & "~" & dr1("jml") & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & dr1("jmlbarang") & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & dr1("hpp") & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & dr1("urutan") & "~" & dr1("jmlrealisasi") & "~" & dr1("statusrealisasi") & "~" & dr1("insertby") & "~" & dr1("isclose") & "~" & dr1("inputuser") & "~" & dr1("inputtgl") & "~" & dr1("modifikasiuser") & "~" & dr1("modifikasitgl") & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & dr1("customtext4") & "~" & dr1("customtext5") & "~" & dr1("customtext6") & "~" & dr1("customtext7") & "~" & dr1("customtext8") & "~" & dr1("customtext9") & "~" & dr1("customtext10") & "~" & dr1("customint1") & "~" & dr1("customint2") & "~" & dr1("customint3") & "~" & dr1("customint4") & "~" & dr1("customint5") & "~" & dr1("customint6") & "~" & dr1("customint7") & "~" & dr1("customint8") & "~" & dr1("customint9") & "~" & dr1("customint10") & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & dr1("customdbl4") & "~" & dr1("customdbl5") & "~" & dr1("customdbl6") & "~" & dr1("customdbl7") & "~" & dr1("customdbl8") & "~" & dr1("customdbl9") & "~" & dr1("customdbl10") & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & dr1("customdate4") & "~" & dr1("customdate5") & "~" & dr1("customdate6") & "~" & dr1("customdate7") & "~" & dr1("customdate8") & "~" & dr1("customdate9") & "~" & dr1("customdate10") & "~" & dr1("mesinnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & dr1("inputusernama") & "~" & dr1("modifikasiusernama") & "~" & dr1("idpdpdetail") & "~" & dr1("idpdp") & "~" & dr1("idsodetail") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & dr1("jmljam") & "~" & dr1("sonotransaksi") & "~" & dr1("bkode") & "~" & dr1("idplan") & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                            result(2) = "Plan Row : " & vRow & " - insert into generate datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE PLOT
                        'mesin, tgl, kelas, subkelas, jml
                        vJmlJam = AsDataTableDSum(dtPlot, "jml", "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'")
                        vJmlJam += Double.Parse(dr1("jmljam"))
                        If AsDataTableUpdateData(dtPlot, "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'", "jml", Math.Round(vJmlJam, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update Plotting Machine datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        'idsodetail, idbarang
                        vJmlProd = AsDataTableDSum(dtSO, "jmlrealisasi", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        vJmlProd += Double.Parse(dr1("jmlbarang"))
                        vJmlBarang = AsDataTableDSum(dtSO, "jmlbarang", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", vJmlProd & "~" & Math.Round(vJmlBarang - vJmlProd, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        vRow += 1
                    Next
                End If

                Dim cKelas As String = "", cSubKelas As String = "", cJmlBarang As Double = 0, cJmlRealisasi As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
                Dim dtCPlot As New DataTable, cKPJ As Double = 0, cSisaJam As Double = 0, cJamButuh As Double = 0, cJamPakai As Double = 0
                Dim cUrutan As Double = 1
                'PERULANGAN SET PRODUCTION PLANNING
setPlan:
                'dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0", "tglkirimso, tglso, idso, urutan, idbarang")
                dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0 AND jmlsisa >= " & minPlan & "", "tglkirimso, tglso, idso, urutan, idbarang")
                dtPlotTemp = AsDataTableFilterSortDt(dtPlot, "jml < 24", "mesin, tgl, kelas, subkelas")

                If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtSOTemp.Rows
                        cKelas = dr1("bakelas") : cSubKelas = dr1("bsubkelas")
                        cJmlBarang = dr1("jmlbarang") : cJmlRealisasi = dr1("jmlrealisasi") : cJmlSisa = dr1("jmlsisa")
                        cKPJ = dr1("kpj")

                        dtCPlot = AsDataTableFilterSortDt(dtPlotTemp, "jml < 24 AND kelas = '" & cKelas & "' AND subkelas = '" & cSubKelas & "'", "mesin, tgl, kelas, subkelas")
                        If dtCPlot.Rows.Count > 0 Then
                            cSisaJam = 24 - dtCPlot.Rows(0)("jml")
                            cJamButuh = cJmlSisa / cKPJ

                            If cJamButuh >= cSisaJam Then
                                cJamPakai = cSisaJam
                            Else
                                cJamPakai = cJamButuh
                            End If
                            cJmlProd = cKPJ * cJamPakai

                            'INSERT KE DATATABLE DETAIL
                            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dtCPlot.Rows(0)("mesin") & "~" & dtCPlot.Rows(0)("tgl") & "~" & dr1("bakelas") & "~" & dr1("bsubkelas") & "~" & cJmlProd & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & cJmlProd & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & 0 & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & cUrutan & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & userid & "~" & "1971-01-01 00:00:00" & "~" & 0 & "~" & "1971-01-01 00:00:00" & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & dtCPlot.Rows(0)("mnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & dr1("idso") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & cJamPakai & "~" & dr1("nomorso") & "~" & dr1("bkode") & "~" & 0 & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - insert into generate datatable failed." : GoTo selesai
                            End If

                            'UPDATE DATATABLE PLOT
                            'mesin, tgl, kelas, subkelas, jml
                            If AsDataTableUpdateData(dtPlot, "mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "'", "jml", Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update Plotting Machine datatable failed." : GoTo selesai
                            End If

                            'UPDATE DATATABLE SO
                            'idso, idbarang
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(cJmlBarang - (cJmlRealisasi + cJmlProd), 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If

                        Else
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlsisa", 0) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                        End If

                        cUrutan += 1

                        GoTo setPlan
                    Next
                End If

            Else
                result(2) = "Plotting Machine data not found." : GoTo selesai
            End If

        Else
            result(2) = "SO Backorder Production data not found." : GoTo selesai
        End If


        If dtdetail.Rows.Count > 0 Then
            dtdetail = AsDataTableFilterSortDt(dtdetail, "", "mesin, tgl, urutan, kelas, subkelas, sonotransaksi, bkode")
            For Each dr As DataRow In dtdetail.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = 0 'Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = 0 'Math.Abs(Val(pg1.isNext))
            resultPaging(2) = 0 'Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = 0 'pg1.countPage
            resultPaging(4) = dtdetail.Rows.Count 'pg1.countRow
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpGenerate20200518(ByVal param As String) As String
        'M6_PdpGenerate --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8 And dataUtama.Length <> 9 And dataUtama.Length <> 10) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String,
        'minPlan(8) As Double, divisiAwal(9) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""
        Dim minPlan As Double = 0, divisiAwal As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
            'Else
            '    result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
            'Else
            '    result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            'If kategoriAwal <> kategoriAkhir Then
            '    result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            'End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
            'Else
            '    result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            'If kelompokAwal <> kelompokAkhir Then
            '    result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            'End If
        Else
            kelompokAkhir = kelompokAwal
        End If

        'minPlan(8) As Double
        If dataUtama.Length > 8 Then
            If IsNumeric(dataUtama(8)) Then
                minPlan = dataUtama(8)
            Else
                result(2) = "Minimum Planning Qty required numeric." : GoTo selesai
            End If
        End If

        'divisiAwal(9) As String
        If dataUtama.Length > 9 Then
            divisiAwal = dataUtama(9)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "mesinnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "inputusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idplan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "maxso", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kpj", AsEnumTypeData.AsDouble)

        Dim dtSO As New DataTable, dtPlot As New DataTable, dtPlan As New DataTable, dtSOTemp As New DataTable, dtPlotTemp As New DataTable

        'AMBIL SO
        sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, sop.jml, sop.satuan, sop.nilaisatuan, sop.jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql &= " FROM m5_so_production sop "
        sql &= " JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2"
        sql &= " AND so.sotgl < '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
        sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
        If Len(divisiAwal) > 0 Then
            sql &= " AND i.bdivisi = '" & FixQuotes(divisiAwal) & "'"
        End If
        sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
        sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        dtSO = AsDataTableAmbilDariDB(sql)
        If dtSO.Rows.Count > 0 Then

            'AMBIL PLOT MESIN
            sql = "  SELECT mp.mesin, mp.tgl, mp.kelas, mp.subkelas, mp.jml, m.mnama FROM m6_machine_plotting mp JOIN m1_machine m ON mp.mesin = m.mkode "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            'filter mesin
            If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                sql &= " AND mp.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin >= '" & FixQuotes(mesinAwal) & "' "
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin <= '" & FixQuotes(mesinAkhir) & "' "
            End If
            'filter kelas
            If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                sql &= " AND mp.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas >= '" & FixQuotes(kategoriAwal) & "' "
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
            End If
            'filter subkelas
            If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                sql &= " AND mp.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
            End If
            sql &= " ORDER BY mp.mesin, mp.tgl, mp.kelas, mp.subkelas "
            dtPlot = AsDataTableAmbilDariDB(sql)
            If dtPlot.Rows.Count > 0 Then

                'AMBIL PLAN
                sql = "  SELECT pl.mesin, pl.tgl, pl.kelas, pl.subkelas, pl.jml, pl.satuan, pl.nilaisatuan, pl.jmlbarang, pl.satuanbarang, pl.matauang, pl.kurs, pl.harga, pl.hpp, pl.cabang, pl.lokasi, pl.gudang, pl.costcenter, pl.divisi, pl.subdivisi, pl.proyek, pl.catatan, pl.urutan, pl.jmlrealisasi, pl.statusrealisasi, pl.insertby, pl.isclose, pl.inputuser, pl.inputtgl, pl.modifikasiuser, pl.modifikasitgl, pl.customtext1, pl.customtext2, pl.customtext3, pl.customtext4, pl.customtext5, pl.customtext6, pl.customtext7, pl.customtext8, pl.customtext9, pl.customtext10, pl.customint1, pl.customint2, pl.customint3, pl.customint4, pl.customint5, pl.customint6, pl.customint7, pl.customint8, pl.customint9, pl.customint10,  pl.customdbl1, pl.customdbl2, pl.customdbl3, pl.customdbl4, pl.customdbl5, pl.customdbl6, pl.customdbl7, pl.customdbl8, pl.customdbl9, pl.customdbl10, pl.customdate1, pl.customdate2, pl.customdate3, pl.customdate4, pl.customdate5, pl.customdate6, pl.customdate7, pl.customdate8, pl.customdate9, pl.customdate10, IFNULL(m.mnama,'') as mesinnama, IFNULL(cl.cnama,'') as kelasnama, IFNULL(sc.scnama,'') as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama, pl.idpdpdetail, IFNULL(pdpd.idpdp,0) as idpdp, pl.idsodetail, pl.idbarang, pl.namabarang, pl.tipebarang, pl.jmljam, IFNULL(so.sonotransaksi,'') as sonotransaksi, IFNULL(i.bkode,'') as bkode, pl.idplan, i.bcustom12 as maxso, i.bcustom13 as kpj "
                sql &= " FROM m6_production_planning pl "
                sql &= " LEFT JOIN m1_machine m ON pl.mesin = m.mkode "
                sql &= " LEFT JOIN m1_class cl on pl.kelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON pl.subkelas = sc.sckode "
                sql &= " LEFT JOIN m0_user u1 ON pl.inputuser = u1.userid "
                sql &= " LEFT JOIN m0_user u2 ON pl.modifikasiuser = u2.userid "
                sql &= " LEFT JOIN m6_pdp_detail pdpd ON pl.idpdpdetail = pdpd.idpdpdetail "
                sql &= " LEFT JOIN m5_so so ON pl.idsodetail = so.soid "
                sql &= " LEFT JOIN m1_item i ON pl.idbarang = i.bid "
                'sql &= " WHERE (pl.insertby = 1 OR pl.isclose = 1) "
                sql &= " WHERE pl.tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
                'filter mesin
                If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                    sql &= " AND pl.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin >= '" & FixQuotes(mesinAwal) & "' "
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin <= '" & FixQuotes(mesinAkhir) & "' "
                End If
                'filter kelas
                If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                    sql &= " AND pl.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas >= '" & FixQuotes(kategoriAwal) & "' "
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
                End If
                'filter subkelas
                If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                    sql &= " AND pl.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
                End If
                sql &= " ORDER BY pl.tgl, pl.mesin, pl.urutan, pl.kelas, pl.subkelas, so.sonotransaksi, i.bkode "
                dtPlan = AsDataTableAmbilDariDB(sql)
                If dtPlan.Rows.Count > 0 Then
                    Dim vRow As Double = 1, vJmlJam As Double = 0, vJmlBarang As Double = 0, vJmlProd As Double = 0
                    For Each dr1 As DataRow In dtPlan.Rows
                        'INSERT KE DATATABLE DETAIL
                        If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dr1("mesin") & "~" & dr1("tgl") & "~" & dr1("kelas") & "~" & dr1("subkelas") & "~" & dr1("jml") & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & dr1("jmlbarang") & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & dr1("hpp") & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & dr1("urutan") & "~" & dr1("jmlrealisasi") & "~" & dr1("statusrealisasi") & "~" & dr1("insertby") & "~" & dr1("isclose") & "~" & dr1("inputuser") & "~" & dr1("inputtgl") & "~" & dr1("modifikasiuser") & "~" & dr1("modifikasitgl") & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & dr1("customtext4") & "~" & dr1("customtext5") & "~" & dr1("customtext6") & "~" & dr1("customtext7") & "~" & dr1("customtext8") & "~" & dr1("customtext9") & "~" & dr1("customtext10") & "~" & dr1("customint1") & "~" & dr1("customint2") & "~" & dr1("customint3") & "~" & dr1("customint4") & "~" & dr1("customint5") & "~" & dr1("customint6") & "~" & dr1("customint7") & "~" & dr1("customint8") & "~" & dr1("customint9") & "~" & dr1("customint10") & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & dr1("customdbl4") & "~" & dr1("customdbl5") & "~" & dr1("customdbl6") & "~" & dr1("customdbl7") & "~" & dr1("customdbl8") & "~" & dr1("customdbl9") & "~" & dr1("customdbl10") & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & dr1("customdate4") & "~" & dr1("customdate5") & "~" & dr1("customdate6") & "~" & dr1("customdate7") & "~" & dr1("customdate8") & "~" & dr1("customdate9") & "~" & dr1("customdate10") & "~" & dr1("mesinnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & dr1("inputusernama") & "~" & dr1("modifikasiusernama") & "~" & dr1("idpdpdetail") & "~" & dr1("idpdp") & "~" & dr1("idsodetail") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & dr1("jmljam") & "~" & dr1("sonotransaksi") & "~" & dr1("bkode") & "~" & dr1("idplan") & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                            result(2) = "Plan Row : " & vRow & " - insert into generate datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE PLOT
                        'mesin, tgl, kelas, subkelas, jml
                        vJmlJam = AsDataTableDSum(dtPlot, "jml", "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'")
                        vJmlJam += Double.Parse(dr1("jmljam"))
                        If AsDataTableUpdateData(dtPlot, "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'", "jml", Math.Round(vJmlJam, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update Plotting Machine datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        'idsodetail, idbarang
                        vJmlProd = AsDataTableDSum(dtSO, "jmlrealisasi", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        vJmlProd += Double.Parse(dr1("jmlbarang"))
                        vJmlBarang = AsDataTableDSum(dtSO, "jmlbarang", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", vJmlProd & "~" & Math.Round(vJmlBarang - vJmlProd, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        vRow += 1
                    Next
                End If

                Dim cKelas As String = "", cSubKelas As String = "", cJmlBarang As Double = 0, cJmlRealisasi As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
                Dim dtCPlot As New DataTable, cKPJ As Double = 0, cSisaJam As Double = 0, cJamButuh As Double = 0, cJamPakai As Double = 0
                Dim cUrutan As Double = 1, cMaxRc As Double = 0
                'PERULANGAN SET PRODUCTION PLANNING
setPlan:
                'dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0", "tglkirimso, tglso, idso, urutan, idbarang")
                dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0 AND jmlsisa >= " & minPlan & "", "tglkirimso, tglso, idso, urutan, idbarang")
                dtPlotTemp = AsDataTableFilterSortDt(dtPlot, "jml < 24", "mesin, tgl, kelas, subkelas")

                If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtSOTemp.Rows
                        cKelas = dr1("bakelas") : cSubKelas = dr1("bsubkelas")
                        cJmlBarang = dr1("jmlbarang") : cJmlRealisasi = dr1("jmlrealisasi") : cJmlSisa = dr1("jmlsisa")
                        cKPJ = dr1("kpj") : cMaxRc = dr1("customdbl3")

                        dtCPlot = AsDataTableFilterSortDt(dtPlotTemp, "jml < 24 AND kelas = '" & cKelas & "' AND subkelas = '" & cSubKelas & "'", "mesin, tgl, kelas, subkelas")
                        If dtCPlot.Rows.Count > 0 Then

                            If cKPJ <= 0 Then
                                result(2) = " Production Capacity/Hour for item " & dr1("bkode") & " can't be less than or equal to zero." : GoTo selesai
                            End If

                            If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                                cJmlSisa = cMaxRc
                            End If

                            cSisaJam = 24 - dtCPlot.Rows(0)("jml")
                            cJamButuh = cJmlSisa / cKPJ

                            If cJamButuh >= cSisaJam Then
                                cJamPakai = cSisaJam
                            Else
                                cJamPakai = cJamButuh
                            End If
                            cJmlProd = cKPJ * cJamPakai

                            'INSERT KE DATATABLE DETAIL
                            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dtCPlot.Rows(0)("mesin") & "~" & dtCPlot.Rows(0)("tgl") & "~" & dr1("bakelas") & "~" & dr1("bsubkelas") & "~" & cJmlProd & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & cJmlProd & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & 0 & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & cUrutan & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & userid & "~" & "1971-01-01 00:00:00" & "~" & 0 & "~" & "1971-01-01 00:00:00" & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & dtCPlot.Rows(0)("mnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & dr1("idso") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & cJamPakai & "~" & dr1("nomorso") & "~" & dr1("bkode") & "~" & 0 & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - insert into generate datatable failed." : GoTo selesai
                            End If

                            'UPDATE DATATABLE PLOT
                            'mesin, tgl, kelas, subkelas, jml
                            If AsDataTableUpdateData(dtPlot, "mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "'", "jml", Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update Plotting Machine datatable failed." : GoTo selesai
                            End If

                            'UPDATE DATATABLE SO
                            'idso, idbarang
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(cJmlBarang - (cJmlRealisasi + cJmlProd), 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If

                        Else
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlsisa", 0) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                        End If

                        cUrutan += 1

                        GoTo setPlan
                    Next
                End If

            Else
                result(2) = "Plotting Machine data not found." : GoTo selesai
            End If

        Else
            result(2) = "SO Backorder Production data not found." : GoTo selesai
        End If


        If dtdetail.Rows.Count > 0 Then
            dtdetail = AsDataTableFilterSortDt(dtdetail, "", "mesin, tgl, urutan, kelas, subkelas, sonotransaksi, bkode")
            For Each dr As DataRow In dtdetail.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = 0 'Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = 0 'Math.Abs(Val(pg1.isNext))
            resultPaging(2) = 0 'Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = 0 'pg1.countPage
            resultPaging(4) = dtdetail.Rows.Count 'pg1.countRow
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpGenerate20200609(ByVal param As String) As String
        'M6_PdpGenerate --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8 And dataUtama.Length <> 9 And dataUtama.Length <> 10) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String,
        'minPlan(8) As Double, divisiAwal(9) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""
        Dim minPlan As Double = 0, divisiAwal As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
            'Else
            '    result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
            'Else
            '    result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            'If kategoriAwal <> kategoriAkhir Then
            '    result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            'End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
            'Else
            '    result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            'If kelompokAwal <> kelompokAkhir Then
            '    result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            'End If
        Else
            kelompokAkhir = kelompokAwal
        End If

        'minPlan(8) As Double
        If dataUtama.Length > 8 Then
            If IsNumeric(dataUtama(8)) Then
                minPlan = dataUtama(8)
            Else
                result(2) = "Minimum Planning Qty required numeric." : GoTo selesai
            End If
        End If

        'divisiAwal(9) As String
        If dataUtama.Length > 9 Then
            divisiAwal = dataUtama(9)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "mesinnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "inputusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idplan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "maxso", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kpj", AsEnumTypeData.AsDouble)

        Dim dtSO As New DataTable, dtPlot As New DataTable, dtPlan As New DataTable, dtSOTemp As New DataTable, dtPlotTemp As New DataTable

        'AMBIL SO
        sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jml - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql &= " FROM m5_so_production sop "
        sql &= " JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2"
        sql &= " AND so.sotgl < '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
        sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
        If Len(divisiAwal) > 0 Then
            sql &= " AND i.bdivisi = '" & FixQuotes(divisiAwal) & "'"
        End If
        sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
        sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql &= " LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang AND pdpl.tgl < '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' "
        sql &= " GROUP BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        dtSO = AsDataTableAmbilDariDB(sql)
        If dtSO.Rows.Count > 0 Then

            'AMBIL PLOT MESIN
            sql = "  SELECT mp.mesin, mp.tgl, mp.kelas, mp.subkelas, mp.jml, m.mnama FROM m6_machine_plotting mp JOIN m1_machine m ON mp.mesin = m.mkode "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            'filter mesin
            If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                sql &= " AND mp.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin >= '" & FixQuotes(mesinAwal) & "' "
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin <= '" & FixQuotes(mesinAkhir) & "' "
            End If
            'filter kelas
            If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                sql &= " AND mp.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas >= '" & FixQuotes(kategoriAwal) & "' "
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
            End If
            'filter subkelas
            If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                sql &= " AND mp.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
            End If
            sql &= " ORDER BY mp.mesin, mp.tgl, mp.kelas, mp.subkelas "
            dtPlot = AsDataTableAmbilDariDB(sql)
            If dtPlot.Rows.Count > 0 Then

                'AMBIL PLAN
                sql = "  SELECT pl.mesin, pl.tgl, pl.kelas, pl.subkelas, pl.jml, pl.satuan, pl.nilaisatuan, pl.jmlbarang, pl.satuanbarang, pl.matauang, pl.kurs, pl.harga, pl.hpp, pl.cabang, pl.lokasi, pl.gudang, pl.costcenter, pl.divisi, pl.subdivisi, pl.proyek, pl.catatan, pl.urutan, pl.jmlrealisasi, pl.statusrealisasi, pl.insertby, pl.isclose, pl.inputuser, pl.inputtgl, pl.modifikasiuser, pl.modifikasitgl, pl.customtext1, pl.customtext2, pl.customtext3, pl.customtext4, pl.customtext5, pl.customtext6, pl.customtext7, pl.customtext8, pl.customtext9, pl.customtext10, pl.customint1, pl.customint2, pl.customint3, pl.customint4, pl.customint5, pl.customint6, pl.customint7, pl.customint8, pl.customint9, pl.customint10,  pl.customdbl1, pl.customdbl2, pl.customdbl3, pl.customdbl4, pl.customdbl5, pl.customdbl6, pl.customdbl7, pl.customdbl8, pl.customdbl9, pl.customdbl10, pl.customdate1, pl.customdate2, pl.customdate3, pl.customdate4, pl.customdate5, pl.customdate6, pl.customdate7, pl.customdate8, pl.customdate9, pl.customdate10, IFNULL(m.mnama,'') as mesinnama, IFNULL(cl.cnama,'') as kelasnama, IFNULL(sc.scnama,'') as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama, pl.idpdpdetail, IFNULL(pdpd.idpdp,0) as idpdp, pl.idsodetail, pl.idbarang, pl.namabarang, pl.tipebarang, pl.jmljam, IFNULL(so.sonotransaksi,'') as sonotransaksi, IFNULL(i.bkode,'') as bkode, pl.idplan, i.bcustom12 as maxso, i.bcustom13 as kpj "
                sql &= " FROM m6_production_planning pl "
                sql &= " LEFT JOIN m1_machine m ON pl.mesin = m.mkode "
                sql &= " LEFT JOIN m1_class cl on pl.kelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON pl.subkelas = sc.sckode "
                sql &= " LEFT JOIN m0_user u1 ON pl.inputuser = u1.userid "
                sql &= " LEFT JOIN m0_user u2 ON pl.modifikasiuser = u2.userid "
                sql &= " LEFT JOIN m6_pdp_detail pdpd ON pl.idpdpdetail = pdpd.idpdpdetail "
                sql &= " LEFT JOIN m5_so so ON pl.idsodetail = so.soid "
                sql &= " LEFT JOIN m1_item i ON pl.idbarang = i.bid "
                'sql &= " WHERE (pl.insertby = 1 OR pl.isclose = 1) "
                sql &= " WHERE pl.tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
                'filter mesin
                If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                    sql &= " AND pl.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin >= '" & FixQuotes(mesinAwal) & "' "
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin <= '" & FixQuotes(mesinAkhir) & "' "
                End If
                'filter kelas
                If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                    sql &= " AND pl.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas >= '" & FixQuotes(kategoriAwal) & "' "
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
                End If
                'filter subkelas
                If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                    sql &= " AND pl.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
                End If
                sql &= " ORDER BY pl.tgl, pl.mesin, pl.urutan, pl.kelas, pl.subkelas, so.sonotransaksi, i.bkode "
                dtPlan = AsDataTableAmbilDariDB(sql)
                If dtPlan.Rows.Count > 0 Then
                    Dim vRow As Double = 1, vJmlJam As Double = 0, vJmlBarang As Double = 0, vJmlProd As Double = 0
                    For Each dr1 As DataRow In dtPlan.Rows
                        'INSERT KE DATATABLE DETAIL
                        If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dr1("mesin") & "~" & dr1("tgl") & "~" & dr1("kelas") & "~" & dr1("subkelas") & "~" & dr1("jml") & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & dr1("jmlbarang") & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & dr1("hpp") & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & dr1("urutan") & "~" & dr1("jmlrealisasi") & "~" & dr1("statusrealisasi") & "~" & dr1("insertby") & "~" & dr1("isclose") & "~" & dr1("inputuser") & "~" & dr1("inputtgl") & "~" & dr1("modifikasiuser") & "~" & dr1("modifikasitgl") & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & dr1("customtext4") & "~" & dr1("customtext5") & "~" & dr1("customtext6") & "~" & dr1("customtext7") & "~" & dr1("customtext8") & "~" & dr1("customtext9") & "~" & dr1("customtext10") & "~" & dr1("customint1") & "~" & dr1("customint2") & "~" & dr1("customint3") & "~" & dr1("customint4") & "~" & dr1("customint5") & "~" & dr1("customint6") & "~" & dr1("customint7") & "~" & dr1("customint8") & "~" & dr1("customint9") & "~" & dr1("customint10") & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & dr1("customdbl4") & "~" & dr1("customdbl5") & "~" & dr1("customdbl6") & "~" & dr1("customdbl7") & "~" & dr1("customdbl8") & "~" & dr1("customdbl9") & "~" & dr1("customdbl10") & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & dr1("customdate4") & "~" & dr1("customdate5") & "~" & dr1("customdate6") & "~" & dr1("customdate7") & "~" & dr1("customdate8") & "~" & dr1("customdate9") & "~" & dr1("customdate10") & "~" & dr1("mesinnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & dr1("inputusernama") & "~" & dr1("modifikasiusernama") & "~" & dr1("idpdpdetail") & "~" & dr1("idpdp") & "~" & dr1("idsodetail") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & dr1("jmljam") & "~" & dr1("sonotransaksi") & "~" & dr1("bkode") & "~" & dr1("idplan") & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                            result(2) = "Plan Row : " & vRow & " - insert into generate datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE PLOT
                        'mesin, tgl, kelas, subkelas, jml
                        vJmlJam = AsDataTableDSum(dtPlot, "jml", "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'")
                        vJmlJam += Double.Parse(dr1("jmljam"))
                        If AsDataTableUpdateData(dtPlot, "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'", "jml", Math.Round(vJmlJam, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update Plotting Machine datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        'idsodetail, idbarang
                        vJmlProd = AsDataTableDSum(dtSO, "jmlrealisasi", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        vJmlProd += Double.Parse(dr1("jmlbarang"))
                        vJmlBarang = AsDataTableDSum(dtSO, "jmlbarang", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", vJmlProd & "~" & Math.Round(vJmlBarang - vJmlProd, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        vRow += 1
                    Next
                End If

                Dim cKelas As String = "", cSubKelas As String = "", cJmlBarang As Double = 0, cJmlRealisasi As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
                Dim dtCPlot As New DataTable, cKPJ As Double = 0, cSisaJam As Double = 0, cJamButuh As Double = 0, cJamPakai As Double = 0
                Dim cUrutan As Double = 1, cMaxRc As Double = 0
                'PERULANGAN SET PRODUCTION PLANNING
setPlan:
                'dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0", "tglkirimso, tglso, idso, urutan, idbarang")
                dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0 AND jmlsisa >= " & minPlan & "", "tglkirimso, tglso, idso, urutan, idbarang")
                dtPlotTemp = AsDataTableFilterSortDt(dtPlot, "jml < 24", "mesin, tgl, kelas, subkelas")

                If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtSOTemp.Rows
                        cKelas = dr1("bakelas") : cSubKelas = dr1("bsubkelas")
                        cJmlBarang = dr1("jmlbarang") : cJmlRealisasi = dr1("jmlrealisasi") : cJmlSisa = dr1("jmlsisa")
                        cKPJ = dr1("kpj") : cMaxRc = dr1("customdbl3")

                        dtCPlot = AsDataTableFilterSortDt(dtPlotTemp, "jml < 24 AND kelas = '" & cKelas & "' AND subkelas = '" & cSubKelas & "'", "mesin, tgl, kelas, subkelas")
                        If dtCPlot.Rows.Count > 0 Then

                            If cKPJ <= 0 Then
                                result(2) = " Production Capacity/Hour for item " & dr1("bkode") & " can't be less than or equal to zero." : GoTo selesai
                            End If

                            If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                                cJmlSisa = cMaxRc
                            End If

                            cSisaJam = 24 - dtCPlot.Rows(0)("jml")
                            cJamButuh = cJmlSisa / cKPJ

                            If cJamButuh >= cSisaJam Then
                                cJamPakai = cSisaJam
                            Else
                                cJamPakai = cJamButuh
                            End If
                            cJmlProd = cKPJ * cJamPakai

                            'INSERT KE DATATABLE DETAIL
                            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dtCPlot.Rows(0)("mesin") & "~" & dtCPlot.Rows(0)("tgl") & "~" & dr1("bakelas") & "~" & dr1("bsubkelas") & "~" & cJmlProd & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & cJmlProd & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & 0 & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & cUrutan & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & userid & "~" & "1971-01-01 00:00:00" & "~" & 0 & "~" & "1971-01-01 00:00:00" & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & dtCPlot.Rows(0)("mnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & dr1("idso") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & cJamPakai & "~" & dr1("nomorso") & "~" & dr1("bkode") & "~" & 0 & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - insert into generate datatable failed." : GoTo selesai
                            End If

                            'UPDATE DATATABLE PLOT
                            'mesin, tgl, kelas, subkelas, jml
                            If AsDataTableUpdateData(dtPlot, "mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "'", "jml", Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update Plotting Machine datatable failed." : GoTo selesai
                            End If

                            'UPDATE DATATABLE SO
                            'idso, idbarang
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(cJmlBarang - (cJmlRealisasi + cJmlProd), 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If

                        Else
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlsisa", 0) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                        End If

                        cUrutan += 1

                        GoTo setPlan
                    Next
                End If

            Else
                result(2) = "Plotting Machine data not found." : GoTo selesai
            End If

        Else
            result(2) = "SO Backorder Production data not found." : GoTo selesai
        End If


        If dtdetail.Rows.Count > 0 Then
            dtdetail = AsDataTableFilterSortDt(dtdetail, "", "mesin, tgl, urutan, kelas, subkelas, sonotransaksi, bkode")
            For Each dr As DataRow In dtdetail.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = 0 'Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = 0 'Math.Abs(Val(pg1.isNext))
            resultPaging(2) = 0 'Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = 0 'pg1.countPage
            resultPaging(4) = dtdetail.Rows.Count 'pg1.countRow
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpGenerate20210315(ByVal param As String) As String
        'M6_PdpGenerate Planning --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        'M6_PdpGenerate Allowance --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal


        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = "", strAllowance As String = "", ftAllowance As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8 And dataUtama.Length <> 9 And dataUtama.Length <> 10) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String,
        'minPlan(8) As Double, divisiAwal(9) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""
        Dim minPlan As Double = 0, divisiAwal As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
            'Else
            '    result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
            'Else
            '    result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            'If kategoriAwal <> kategoriAkhir Then
            '    result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            'End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
            'Else
            '    result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            'If kelompokAwal <> kelompokAkhir Then
            '    result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            'End If
        Else
            kelompokAkhir = kelompokAwal
        End If

        'minPlan(8) As Double
        If dataUtama.Length > 8 Then
            If IsNumeric(dataUtama(8)) Then
                minPlan = dataUtama(8)
            Else
                result(2) = "Minimum Planning Qty required numeric." : GoTo selesai
            End If
        End If

        'divisiAwal(9) As String
        If dataUtama.Length > 9 Then
            divisiAwal = dataUtama(9)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "mesinnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "inputusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idplan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "maxso", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kpj", AsEnumTypeData.AsDouble)

        Dim vMaxJam As Double = 0
        Dim dtSett As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule = 6 AND sgrup = 'options' AND skode = 'PdpMaxJam'")
        If dtSett.Rows.Count > 0 Then
            If IsNumeric(FxDB(dtSett.Rows(0)(0), 0)) Then
                vMaxJam = FxDB(dtSett.Rows(0)(0), 0)
            Else
                result(2) = "Setting for PdpMaxJam required numeric." : GoTo selesai
            End If
        Else
            result(2) = "Setting for PdpMaxJam not found." : GoTo selesai
        End If

        Dim dtSO As New DataTable, dtPlot As New DataTable, dtPlan As New DataTable, dtSOTemp As New DataTable, dtPlotTemp As New DataTable

        'AMBIL SO
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jml - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql &= " FROM m5_so_production sop "
        sql &= " JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND so.solokasi <> 'L' "
        'sql &= " AND sop.statuspl <> 2"
        sql &= " AND so.sotgl < '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
        sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
        If Len(divisiAwal) > 0 Then
            sql &= " AND i.bdivisi = '" & FixQuotes(divisiAwal) & "'"
        End If
        sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
        sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql &= " LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang AND pdpl.tgl < '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' "
        sql &= " GROUP BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        dtSO = AsDataTableAmbilDariDB(sql)
        If dtSO.Rows.Count > 0 Then

            'AMBIL PLOT MESIN
            sql = "  SELECT mp.mesin, mp.tgl, mp.kelas, mp.subkelas, mp.jml, m.mnama FROM m6_machine_plotting mp JOIN m1_machine m ON mp.mesin = m.mkode "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            'filter mesin
            If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                sql &= " AND mp.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin >= '" & FixQuotes(mesinAwal) & "' "
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin <= '" & FixQuotes(mesinAkhir) & "' "
            End If
            'filter kelas
            If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                sql &= " AND mp.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas >= '" & FixQuotes(kategoriAwal) & "' "
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
            End If
            'filter subkelas
            If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                sql &= " AND mp.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
            End If
            sql &= " ORDER BY mp.mesin, mp.tgl, mp.kelas, mp.subkelas "
            dtPlot = AsDataTableAmbilDariDB(sql)
            If dtPlot.Rows.Count > 0 Then

                'AMBIL PLAN
                sql = "  SELECT pl.mesin, pl.tgl, pl.kelas, pl.subkelas, pl.jml, pl.satuan, pl.nilaisatuan, pl.jmlbarang, pl.satuanbarang, pl.matauang, pl.kurs, pl.harga, pl.hpp, pl.cabang, pl.lokasi, pl.gudang, pl.costcenter, pl.divisi, pl.subdivisi, pl.proyek, pl.catatan, pl.urutan, pl.jmlrealisasi, pl.statusrealisasi, pl.insertby, pl.isclose, pl.inputuser, pl.inputtgl, pl.modifikasiuser, pl.modifikasitgl, pl.customtext1, pl.customtext2, pl.customtext3, pl.customtext4, pl.customtext5, pl.customtext6, pl.customtext7, pl.customtext8, pl.customtext9, pl.customtext10, pl.customint1, pl.customint2, pl.customint3, pl.customint4, pl.customint5, pl.customint6, pl.customint7, pl.customint8, pl.customint9, pl.customint10,  pl.customdbl1, pl.customdbl2, pl.customdbl3, pl.customdbl4, pl.customdbl5, pl.customdbl6, pl.customdbl7, pl.customdbl8, pl.customdbl9, pl.customdbl10, pl.customdate1, pl.customdate2, pl.customdate3, pl.customdate4, pl.customdate5, pl.customdate6, pl.customdate7, pl.customdate8, pl.customdate9, pl.customdate10, IFNULL(m.mnama,'') as mesinnama, IFNULL(cl.cnama,'') as kelasnama, IFNULL(sc.scnama,'') as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama, pl.idpdpdetail, IFNULL(pdpd.idpdp,0) as idpdp, pl.idsodetail, pl.idbarang, pl.namabarang, pl.tipebarang, pl.jmljam, IFNULL(so.sonotransaksi,'') as sonotransaksi, IFNULL(i.bkode,'') as bkode, pl.idplan, i.bcustom12 as maxso, i.bcustom13 as kpj "
                sql &= " FROM m6_production_planning pl "
                sql &= " LEFT JOIN m1_machine m ON pl.mesin = m.mkode "
                sql &= " LEFT JOIN m1_class cl on pl.kelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON pl.subkelas = sc.sckode "
                sql &= " LEFT JOIN m0_user u1 ON pl.inputuser = u1.userid "
                sql &= " LEFT JOIN m0_user u2 ON pl.modifikasiuser = u2.userid "
                sql &= " LEFT JOIN m6_pdp_detail pdpd ON pl.idpdpdetail = pdpd.idpdpdetail "
                sql &= " LEFT JOIN m5_so so ON pl.idsodetail = so.soid "
                sql &= " LEFT JOIN m1_item i ON pl.idbarang = i.bid "
                'sql &= " WHERE (pl.insertby = 1 OR pl.isclose = 1) "
                sql &= " WHERE pl.tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
                'filter mesin
                If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                    sql &= " AND pl.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin >= '" & FixQuotes(mesinAwal) & "' "
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin <= '" & FixQuotes(mesinAkhir) & "' "
                End If
                'filter kelas
                If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                    sql &= " AND pl.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas >= '" & FixQuotes(kategoriAwal) & "' "
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
                End If
                'filter subkelas
                If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                    sql &= " AND pl.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
                End If
                sql &= " ORDER BY pl.tgl, pl.mesin, pl.urutan, pl.kelas, pl.subkelas, so.sonotransaksi, i.bkode "
                dtPlan = AsDataTableAmbilDariDB(sql)
                If dtPlan.Rows.Count > 0 Then
                    Dim vRow As Double = 1, vJmlJam As Double = 0, vJmlBarang As Double = 0, vJmlProd As Double = 0
                    For Each dr1 As DataRow In dtPlan.Rows
                        'INSERT KE DATATABLE DETAIL
                        If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dr1("mesin") & "~" & dr1("tgl") & "~" & dr1("kelas") & "~" & dr1("subkelas") & "~" & dr1("jml") & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & dr1("jmlbarang") & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & dr1("hpp") & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & dr1("urutan") & "~" & dr1("jmlrealisasi") & "~" & dr1("statusrealisasi") & "~" & dr1("insertby") & "~" & dr1("isclose") & "~" & dr1("inputuser") & "~" & dr1("inputtgl") & "~" & dr1("modifikasiuser") & "~" & dr1("modifikasitgl") & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & dr1("customtext4") & "~" & dr1("customtext5") & "~" & dr1("customtext6") & "~" & dr1("customtext7") & "~" & dr1("customtext8") & "~" & dr1("customtext9") & "~" & dr1("customtext10") & "~" & dr1("customint1") & "~" & dr1("customint2") & "~" & dr1("customint3") & "~" & dr1("customint4") & "~" & dr1("customint5") & "~" & dr1("customint6") & "~" & dr1("customint7") & "~" & dr1("customint8") & "~" & dr1("customint9") & "~" & dr1("customint10") & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & dr1("customdbl4") & "~" & dr1("customdbl5") & "~" & dr1("customdbl6") & "~" & dr1("customdbl7") & "~" & dr1("customdbl8") & "~" & dr1("customdbl9") & "~" & dr1("customdbl10") & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & dr1("customdate4") & "~" & dr1("customdate5") & "~" & dr1("customdate6") & "~" & dr1("customdate7") & "~" & dr1("customdate8") & "~" & dr1("customdate9") & "~" & dr1("customdate10") & "~" & dr1("mesinnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & dr1("inputusernama") & "~" & dr1("modifikasiusernama") & "~" & dr1("idpdpdetail") & "~" & dr1("idpdp") & "~" & dr1("idsodetail") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & dr1("jmljam") & "~" & dr1("sonotransaksi") & "~" & dr1("bkode") & "~" & dr1("idplan") & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                            result(2) = "Plan Row : " & vRow & " - insert into generate datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE PLOT
                        'mesin, tgl, kelas, subkelas, jml
                        vJmlJam = AsDataTableDSum(dtPlot, "jml", "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'")
                        vJmlJam += Double.Parse(dr1("jmljam"))
                        If AsDataTableUpdateData(dtPlot, "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'", "jml", Math.Round(vJmlJam, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update Plotting Machine datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        'idsodetail, idbarang
                        vJmlProd = AsDataTableDSum(dtSO, "jmlrealisasi", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        vJmlProd += Double.Parse(dr1("jmlbarang"))
                        vJmlBarang = AsDataTableDSum(dtSO, "jmlbarang", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", vJmlProd & "~" & Math.Round(vJmlBarang - vJmlProd, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        vRow += 1
                    Next
                End If

                Dim cKelas As String = "", cSubKelas As String = "", cJmlBarang As Double = 0, cJmlRealisasi As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
                Dim dtCPlot As New DataTable, cKPJ As Double = 0, cSisaJam As Double = 0, cJamButuh As Double = 0, cJamPakai As Double = 0
                Dim cUrutan As Double = 1, cMaxRc As Double = 0

                'PERULANGAN SET PRODUCTION PLANNING
setPlan:
                'SimpanLogWsToFile(6, "Pdp", "setPlan:")
                'dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0", "tglkirimso, tglso, idso, urutan, idbarang")
                dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0 AND jmlsisa >= " & minPlan & " AND customdbl3 >= " & minPlan & "", "tglkirimso, tglso, idso, urutan, idbarang")
                dtPlotTemp = AsDataTableFilterSortDt(dtPlot, "jml < " & vMaxJam & "", "mesin, tgl, kelas, subkelas")

                If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then
                    'SimpanLogWsToFile(6, "Pdp", "If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then")

                    For Each dr1 As DataRow In dtSOTemp.Rows
                        'Dim dr1 As DataRow = dtSOTemp.Rows(0)
                        cKelas = dr1("bakelas") : cSubKelas = dr1("bsubkelas")
                        cJmlBarang = dr1("jmlbarang") : cJmlRealisasi = dr1("jmlrealisasi") : cJmlSisa = dr1("jmlsisa")
                        cKPJ = dr1("kpj") : cMaxRc = dr1("customdbl3")

                        dtCPlot = AsDataTableFilterSortDt(dtPlotTemp, "jml < " & vMaxJam & " AND kelas = '" & cKelas & "' AND subkelas = '" & cSubKelas & "'", "mesin, tgl, kelas, subkelas")
                        If dtCPlot.Rows.Count > 0 Then
                            'SimpanLogWsToFile(6, "Pdp", "If dtCPlot.Rows.Count > 0 Then")

                            If cKPJ <= 0 Then
                                result(2) = " Production Capacity/Hour for item " & dr1("bkode") & " can't be less than or equal to zero." : GoTo selesai
                            End If

                            If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                                cJmlSisa = cMaxRc
                            End If

                            cSisaJam = vMaxJam - dtCPlot.Rows(0)("jml")
                            cJamButuh = cJmlSisa / cKPJ

                            If cJamButuh >= cSisaJam Then
                                cJamPakai = cSisaJam
                            Else
                                cJamPakai = cJamButuh
                            End If
                            cJmlProd = cKPJ * cJamPakai

                            'INSERT KE DATATABLE DETAIL
                            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dtCPlot.Rows(0)("mesin") & "~" & dtCPlot.Rows(0)("tgl") & "~" & dr1("bakelas") & "~" & dr1("bsubkelas") & "~" & cJmlProd & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & cJmlProd & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & 0 & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & cUrutan & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & userid & "~" & "1971-01-01 00:00:00" & "~" & 0 & "~" & "1971-01-01 00:00:00" & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & dtCPlot.Rows(0)("mnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & dr1("idso") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & cJamPakai & "~" & dr1("nomorso") & "~" & dr1("bkode") & "~" & 0 & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - insert into generate datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "AsDataTableTambahData(dtdetail")

                            'UPDATE DATATABLE PLOT
                            'mesin, tgl, kelas, subkelas, jml
                            If AsDataTableUpdateData(dtPlot, "mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "'", "jml", Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update Plotting Machine datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtPlot mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "' jml " & Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5))

                            'UPDATE DATATABLE SO
                            'idso, idbarang
                            'If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(cJmlBarang - (cJmlRealisasi + cJmlProd), 5)) = False Then
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(dr1("jmlsisa") - (cJmlProd), 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtSO idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & " jmlrealisasi~jmlsisa " & cJmlRealisasi + cJmlProd & "~" & Math.Round(dr1("jmlsisa") - (cJmlProd), 5))

                        Else
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlsisa", 0) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtSO else idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "' jmlsisa " & 0)
                        End If

                        cUrutan += 1

                        GoTo setPlan
                    Next
                End If

            Else
                result(2) = "Plotting Machine data not found." : GoTo selesai
            End If

        Else
            result(2) = "SO Backorder Production data not found." : GoTo selesai
        End If


        If dtdetail.Rows.Count > 0 Then
            dtdetail = AsDataTableFilterSortDt(dtdetail, "", "mesin, tgl, urutan, kelas, subkelas, sonotransaksi, bkode")
            For Each dr As DataRow In dtdetail.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)

                'TAMBAH FILTER UNTUK DATA ALLOWANCE
                If Len(ftAllowance) > 0 Then
                    ftAllowance = ftAllowance & " OR "
                End If
                ftAllowance = ftAllowance & " (so.soid = '" & FixDouble(FxDB(dr("idsodetail"), "0")) & "' AND sop.idbarang = '" & FixDouble(FxDB(dr("idbarang"), "0")) & "') "

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            'AMBIL DATA ALLOWANCE
            If Len(ftAllowance) > 0 Then
                sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal "
                sql &= " FROM m5_so_production sop "
                sql &= " JOIN m5_so so ON sop.idso = so.soid "
                sql &= " AND (" & ftAllowance & ")"
                sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
                sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode"
                Dim dtallowance As New DataTable
                dtallowance = AmbilData("aplikasi1-m1_allowance", "", "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", sql) ' Ambil data ke databases
                For Each dr As DataRow In dtallowance.Rows
                    strAllowance = String.Concat(strAllowance,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(dr("jml"), 0), sptField,
                             FxDB(dr("jmljam"), 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(dr("jmlbarang"), 0), sptField,
                             FxDB(dr("jmlsisa"), 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)
                Next
                If strAllowance.Length > 0 Then strAllowance = strAllowance.Substring(0, strAllowance.Length - sptRow.Length) Else strAllowance = strAllowance

            End If


            result(1) = 1
            resultPaging(0) = 0 'Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = 0 'Math.Abs(Val(pg1.isNext))
            resultPaging(2) = 0 'Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = 0 'pg1.countPage
            resultPaging(4) = dtdetail.Rows.Count 'pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, strAllowance)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj" & sptSubParam & "bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpGenerate(ByVal param As String) As String
        'M6_PdpGenerate Planning --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        'M6_PdpGenerate Allowance --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal


        On Error GoTo selesai

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = "", strAllowance As String = "", ftAllowance As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8 And dataUtama.Length <> 9 And dataUtama.Length <> 10) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String,
        'minPlan(8) As Double, divisiAwal(9) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""
        Dim minPlan As Double = 0, divisiAwal As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
            'Else
            '    result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
            'Else
            '    result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            'If kategoriAwal <> kategoriAkhir Then
            '    result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            'End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
            'Else
            '    result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            'If kelompokAwal <> kelompokAkhir Then
            '    result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            'End If
        Else
            kelompokAkhir = kelompokAwal
        End If

        'minPlan(8) As Double
        If dataUtama.Length > 8 Then
            If IsNumeric(dataUtama(8)) Then
                minPlan = dataUtama(8)
            Else
                result(2) = "Minimum Planning Qty required numeric." : GoTo selesai
            End If
        End If

        'divisiAwal(9) As String
        If dataUtama.Length > 9 Then
            divisiAwal = dataUtama(9)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "mesinnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "inputusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idplan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "maxso", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kpj", AsEnumTypeData.AsDouble)

        Dim vMaxJam As Double = 0
        Dim dtSett As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE smodule = 6 AND sgrup = 'options' AND skode = 'PdpMaxJam'", myConn)
        If dtSett.Rows.Count > 0 Then
            If IsNumeric(FxDB(dtSett.Rows(0)(0), 0)) Then
                vMaxJam = FxDB(dtSett.Rows(0)(0), 0)
            Else
                result(2) = "Setting for PdpMaxJam required numeric." : GoTo selesai
            End If
        Else
            result(2) = "Setting for PdpMaxJam not found." : GoTo selesai
        End If

        Dim dtSO As New DataTable, dtPlot As New DataTable, dtPlan As New DataTable, dtSOTemp As New DataTable, dtPlotTemp As New DataTable

        'AMBIL SO
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jml - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql &= " FROM m5_so_production sop "
        sql &= " JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND so.solokasi <> 'L' "
        'sql &= " AND sop.statuspl <> 2"
        sql &= " AND so.sotgl < '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
        sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
        If Len(divisiAwal) > 0 Then
            sql &= " AND i.bdivisi = '" & FixQuotes(divisiAwal) & "'"
        End If
        sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
        sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql &= " LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang AND pdpl.tgl < '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' "
        sql &= " GROUP BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        dtSO = AsDataTableAmbilDariDBCon(sql, myConn)
        If dtSO.Rows.Count > 0 Then

            'AMBIL PLOT MESIN
            sql = "  SELECT mp.mesin, mp.tgl, mp.kelas, mp.subkelas, mp.jml, m.mnama FROM m6_machine_plotting mp JOIN m1_machine m ON mp.mesin = m.mkode "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            'filter mesin
            If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                sql &= " AND mp.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin >= '" & FixQuotes(mesinAwal) & "' "
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin <= '" & FixQuotes(mesinAkhir) & "' "
            End If
            'filter kelas
            If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                sql &= " AND mp.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas >= '" & FixQuotes(kategoriAwal) & "' "
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
            End If
            'filter subkelas
            If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                sql &= " AND mp.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
            End If
            sql &= " ORDER BY mp.mesin, mp.tgl, mp.kelas, mp.subkelas "
            dtPlot = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtPlot.Rows.Count > 0 Then

                'AMBIL PLAN
                sql = "  SELECT pl.mesin, pl.tgl, pl.kelas, pl.subkelas, pl.jml, pl.satuan, pl.nilaisatuan, pl.jmlbarang, pl.satuanbarang, pl.matauang, pl.kurs, pl.harga, pl.hpp, pl.cabang, pl.lokasi, pl.gudang, pl.costcenter, pl.divisi, pl.subdivisi, pl.proyek, pl.catatan, pl.urutan, pl.jmlrealisasi, pl.statusrealisasi, pl.insertby, pl.isclose, pl.inputuser, pl.inputtgl, pl.modifikasiuser, pl.modifikasitgl, pl.customtext1, pl.customtext2, pl.customtext3, pl.customtext4, pl.customtext5, pl.customtext6, pl.customtext7, pl.customtext8, pl.customtext9, pl.customtext10, pl.customint1, pl.customint2, pl.customint3, pl.customint4, pl.customint5, pl.customint6, pl.customint7, pl.customint8, pl.customint9, pl.customint10,  pl.customdbl1, pl.customdbl2, pl.customdbl3, pl.customdbl4, pl.customdbl5, pl.customdbl6, pl.customdbl7, pl.customdbl8, pl.customdbl9, pl.customdbl10, pl.customdate1, pl.customdate2, pl.customdate3, pl.customdate4, pl.customdate5, pl.customdate6, pl.customdate7, pl.customdate8, pl.customdate9, pl.customdate10, IFNULL(m.mnama,'') as mesinnama, IFNULL(cl.cnama,'') as kelasnama, IFNULL(sc.scnama,'') as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama, pl.idpdpdetail, IFNULL(pdpd.idpdp,0) as idpdp, pl.idsodetail, pl.idbarang, pl.namabarang, pl.tipebarang, pl.jmljam, IFNULL(so.sonotransaksi,'') as sonotransaksi, IFNULL(i.bkode,'') as bkode, pl.idplan, i.bcustom12 as maxso, i.bcustom13 as kpj "
                sql &= " FROM m6_production_planning pl "
                sql &= " LEFT JOIN m1_machine m ON pl.mesin = m.mkode "
                sql &= " LEFT JOIN m1_class cl on pl.kelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON pl.subkelas = sc.sckode "
                sql &= " LEFT JOIN m0_user u1 ON pl.inputuser = u1.userid "
                sql &= " LEFT JOIN m0_user u2 ON pl.modifikasiuser = u2.userid "
                sql &= " LEFT JOIN m6_pdp_detail pdpd ON pl.idpdpdetail = pdpd.idpdpdetail "
                sql &= " LEFT JOIN m5_so so ON pl.idsodetail = so.soid "
                sql &= " LEFT JOIN m1_item i ON pl.idbarang = i.bid "
                'sql &= " WHERE (pl.insertby = 1 OR pl.isclose = 1) "
                sql &= " WHERE pl.tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
                'filter mesin
                If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                    sql &= " AND pl.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin >= '" & FixQuotes(mesinAwal) & "' "
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin <= '" & FixQuotes(mesinAkhir) & "' "
                End If
                'filter kelas
                If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                    sql &= " AND pl.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas >= '" & FixQuotes(kategoriAwal) & "' "
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
                End If
                'filter subkelas
                If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                    sql &= " AND pl.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
                End If
                sql &= " ORDER BY pl.tgl, pl.mesin, pl.urutan, pl.kelas, pl.subkelas, so.sonotransaksi, i.bkode "
                dtPlan = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtPlan.Rows.Count > 0 Then
                    Dim vRow As Double = 1, vJmlJam As Double = 0, vJmlBarang As Double = 0, vJmlProd As Double = 0
                    For Each dr1 As DataRow In dtPlan.Rows
                        'INSERT KE DATATABLE DETAIL
                        If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dr1("mesin") & "~" & dr1("tgl") & "~" & dr1("kelas") & "~" & dr1("subkelas") & "~" & dr1("jml") & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & dr1("jmlbarang") & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & dr1("hpp") & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & dr1("urutan") & "~" & dr1("jmlrealisasi") & "~" & dr1("statusrealisasi") & "~" & dr1("insertby") & "~" & dr1("isclose") & "~" & dr1("inputuser") & "~" & dr1("inputtgl") & "~" & dr1("modifikasiuser") & "~" & dr1("modifikasitgl") & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & dr1("customtext4") & "~" & dr1("customtext5") & "~" & dr1("customtext6") & "~" & dr1("customtext7") & "~" & dr1("customtext8") & "~" & dr1("customtext9") & "~" & dr1("customtext10") & "~" & dr1("customint1") & "~" & dr1("customint2") & "~" & dr1("customint3") & "~" & dr1("customint4") & "~" & dr1("customint5") & "~" & dr1("customint6") & "~" & dr1("customint7") & "~" & dr1("customint8") & "~" & dr1("customint9") & "~" & dr1("customint10") & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & dr1("customdbl4") & "~" & dr1("customdbl5") & "~" & dr1("customdbl6") & "~" & dr1("customdbl7") & "~" & dr1("customdbl8") & "~" & dr1("customdbl9") & "~" & dr1("customdbl10") & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & dr1("customdate4") & "~" & dr1("customdate5") & "~" & dr1("customdate6") & "~" & dr1("customdate7") & "~" & dr1("customdate8") & "~" & dr1("customdate9") & "~" & dr1("customdate10") & "~" & dr1("mesinnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & dr1("inputusernama") & "~" & dr1("modifikasiusernama") & "~" & dr1("idpdpdetail") & "~" & dr1("idpdp") & "~" & dr1("idsodetail") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & dr1("jmljam") & "~" & dr1("sonotransaksi") & "~" & dr1("bkode") & "~" & dr1("idplan") & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                            result(2) = "Plan Row : " & vRow & " - insert into generate datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE PLOT
                        'mesin, tgl, kelas, subkelas, jml
                        vJmlJam = AsDataTableDSum(dtPlot, "jml", "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'")
                        vJmlJam += Double.Parse(dr1("jmljam"))
                        If AsDataTableUpdateData(dtPlot, "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'", "jml", Math.Round(vJmlJam, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update Plotting Machine datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        'idsodetail, idbarang
                        vJmlProd = AsDataTableDSum(dtSO, "jmlrealisasi", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        vJmlProd += Double.Parse(dr1("jmlbarang"))
                        vJmlBarang = AsDataTableDSum(dtSO, "jmlbarang", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", vJmlProd & "~" & Math.Round(vJmlBarang - vJmlProd, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        vRow += 1
                    Next
                End If

                Dim cKelas As String = "", cSubKelas As String = "", cJmlBarang As Double = 0, cJmlRealisasi As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
                Dim dtCPlot As New DataTable, cKPJ As Double = 0, cSisaJam As Double = 0, cJamButuh As Double = 0, cJamPakai As Double = 0
                Dim cUrutan As Double = 1, cMaxRc As Double = 0

                'PERULANGAN SET PRODUCTION PLANNING
setPlan:
                'SimpanLogWsToFile(6, "Pdp", "setPlan:")
                'dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0", "tglkirimso, tglso, idso, urutan, idbarang")
                dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0 AND jmlsisa >= " & minPlan & " AND customdbl3 >= " & minPlan & "", "tglkirimso, tglso, idso, urutan, idbarang")
                dtPlotTemp = AsDataTableFilterSortDt(dtPlot, "jml < " & vMaxJam & "", "mesin, tgl, kelas, subkelas")

                If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then
                    'SimpanLogWsToFile(6, "Pdp", "If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then")

                    For Each dr1 As DataRow In dtSOTemp.Rows
                        'Dim dr1 As DataRow = dtSOTemp.Rows(0)
                        cKelas = dr1("bakelas") : cSubKelas = dr1("bsubkelas")
                        cJmlBarang = dr1("jmlbarang") : cJmlRealisasi = dr1("jmlrealisasi") : cJmlSisa = dr1("jmlsisa")
                        cKPJ = dr1("kpj") : cMaxRc = dr1("customdbl3")

                        dtCPlot = AsDataTableFilterSortDt(dtPlotTemp, "jml < " & vMaxJam & " AND kelas = '" & cKelas & "' AND subkelas = '" & cSubKelas & "'", "mesin, tgl, kelas, subkelas")
                        If dtCPlot.Rows.Count > 0 Then
                            'SimpanLogWsToFile(6, "Pdp", "If dtCPlot.Rows.Count > 0 Then")

                            If cKPJ <= 0 Then
                                result(2) = " Production Capacity/Hour for item " & dr1("bkode") & " can't be less than or equal to zero." : GoTo selesai
                            End If

                            If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                                cJmlSisa = cMaxRc
                            End If

                            cSisaJam = vMaxJam - dtCPlot.Rows(0)("jml")
                            cJamButuh = cJmlSisa / cKPJ

                            If cJamButuh >= cSisaJam Then
                                cJamPakai = cSisaJam
                            Else
                                cJamPakai = cJamButuh
                            End If
                            cJmlProd = cKPJ * cJamPakai

                            'INSERT KE DATATABLE DETAIL
                            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dtCPlot.Rows(0)("mesin") & "~" & dtCPlot.Rows(0)("tgl") & "~" & dr1("bakelas") & "~" & dr1("bsubkelas") & "~" & cJmlProd & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & cJmlProd & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & 0 & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & cUrutan & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & userid & "~" & "1971-01-01 00:00:00" & "~" & 0 & "~" & "1971-01-01 00:00:00" & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & dtCPlot.Rows(0)("mnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & dr1("idso") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & cJamPakai & "~" & dr1("nomorso") & "~" & dr1("bkode") & "~" & 0 & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - insert into generate datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "AsDataTableTambahData(dtdetail")

                            'UPDATE DATATABLE PLOT
                            'mesin, tgl, kelas, subkelas, jml
                            If AsDataTableUpdateData(dtPlot, "mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "'", "jml", Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update Plotting Machine datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtPlot mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "' jml " & Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5))

                            'UPDATE DATATABLE SO
                            'idso, idbarang
                            'If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(cJmlBarang - (cJmlRealisasi + cJmlProd), 5)) = False Then
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(dr1("jmlsisa") - (cJmlProd), 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtSO idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & " jmlrealisasi~jmlsisa " & cJmlRealisasi + cJmlProd & "~" & Math.Round(dr1("jmlsisa") - (cJmlProd), 5))

                        Else
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlsisa", 0) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtSO else idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "' jmlsisa " & 0)
                        End If

                        cUrutan += 1

                        GoTo setPlan
                    Next
                End If

            Else
                result(2) = "Plotting Machine data not found." : GoTo selesai
            End If

        Else
            result(2) = "SO Backorder Production data not found." : GoTo selesai
        End If


        If dtdetail.Rows.Count > 0 Then
            dtdetail = AsDataTableFilterSortDt(dtdetail, "", "mesin, tgl, urutan, kelas, subkelas, sonotransaksi, bkode")
            For Each dr As DataRow In dtdetail.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)

                'TAMBAH FILTER UNTUK DATA ALLOWANCE
                If Len(ftAllowance) > 0 Then
                    ftAllowance = ftAllowance & " OR "
                End If
                ftAllowance = ftAllowance & " (so.soid = '" & FixDouble(FxDB(dr("idsodetail"), "0")) & "' AND sop.idbarang = '" & FixDouble(FxDB(dr("idbarang"), "0")) & "') "

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            'AMBIL DATA ALLOWANCE
            If Len(ftAllowance) > 0 Then
                sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal "
                sql &= " FROM m5_so_production sop "
                sql &= " JOIN m5_so so ON sop.idso = so.soid "
                sql &= " AND (" & ftAllowance & ")"
                sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
                sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode"
                'sql &= " GROUP BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
                'sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
                Dim dtallowance As New DataTable
                dtallowance = AmbilData("aplikasi1-m1_allowance", "", "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", sql) ' Ambil data ke databases
                'dtallowance = AsDataTableAmbilDariDBCon(sql, myConn)
                For Each dr As DataRow In dtallowance.Rows
                    strAllowance = String.Concat(strAllowance,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(dr("jml"), 0), sptField,
                             FxDB(dr("jmljam"), 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(dr("jmlbarang"), 0), sptField,
                             FxDB(dr("jmlsisa"), 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)
                Next
                If strAllowance.Length > 0 Then strAllowance = strAllowance.Substring(0, strAllowance.Length - sptRow.Length) Else strAllowance = strAllowance

            End If


            result(1) = 1
            resultPaging(0) = 0 'Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = 0 'Math.Abs(Val(pg1.isNext))
            resultPaging(2) = 0 'Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = 0 'pg1.countPage
            resultPaging(4) = dtdetail.Rows.Count 'pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, strAllowance)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj" & sptSubParam & "bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpGenerate20220113(ByVal param As String) As String
        'M6_PdpGenerate Planning --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, 
        'idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam,
        'sonotransaksi, bkode, idplan, maxso, kpj

        'M6_PdpGenerate Allowance --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal


        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = "", strAllowance As String = "", ftAllowance As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8 And dataUtama.Length <> 9 And dataUtama.Length <> 10) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String,
        'minPlan(8) As Double, divisiAwal(9) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""
        Dim minPlan As Double = 0, divisiAwal As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
            'Else
            '    result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
            'Else
            '    result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            'If kategoriAwal <> kategoriAkhir Then
            '    result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            'End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
            'Else
            '    result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            'If kelompokAwal <> kelompokAkhir Then
            '    result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            'End If
        Else
            kelompokAkhir = kelompokAwal
        End If

        'minPlan(8) As Double
        If dataUtama.Length > 8 Then
            If IsNumeric(dataUtama(8)) Then
                minPlan = dataUtama(8)
            Else
                result(2) = "Minimum Planning Qty required numeric." : GoTo selesai
            End If
        End If

        'divisiAwal(9) As String
        If dataUtama.Length > 9 Then
            divisiAwal = dataUtama(9)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "mesinnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelasnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "inputusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiusernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdpdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idpdp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmljam", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idplan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "maxso", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kpj", AsEnumTypeData.AsDouble)

        Dim vMaxJam As Double = 0
        Dim dtSett As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule = 6 AND sgrup = 'options' AND skode = 'PdpMaxJam'")
        If dtSett.Rows.Count > 0 Then
            If IsNumeric(FxDB(dtSett.Rows(0)(0), 0)) Then
                vMaxJam = FxDB(dtSett.Rows(0)(0), 0)
            Else
                result(2) = "Setting for PdpMaxJam required numeric." : GoTo selesai
            End If
        Else
            result(2) = "Setting for PdpMaxJam not found." : GoTo selesai
        End If

        Dim dtSO As New DataTable, dtPlot As New DataTable, dtPlan As New DataTable, dtSOTemp As New DataTable, dtPlotTemp As New DataTable

        'AMBIL SO
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jml - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        'sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jml, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - SUM(IFNULL(pdpl.jmlbarang,0)), 5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 "
        sql &= " FROM m5_so_production sop "
        sql &= " JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND so.solokasi <> 'L' "
        'sql &= " AND sop.statuspl <> 2"
        sql &= " AND so.sotgl < '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
        sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
        If Len(divisiAwal) > 0 Then
            sql &= " AND i.bdivisi = '" & FixQuotes(divisiAwal) & "'"
        End If
        sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
        sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql &= " LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang AND pdpl.tgl < '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' "
        sql &= " GROUP BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        sql &= " ORDER BY so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang "
        dtSO = AsDataTableAmbilDariDB(sql)
        If dtSO.Rows.Count > 0 Then

            'AMBIL PLOT MESIN
            sql = "  SELECT mp.mesin, mp.tgl, mp.kelas, mp.subkelas, mp.jml, m.mnama FROM m6_machine_plotting mp JOIN m1_machine m ON mp.mesin = m.mkode "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            'filter mesin
            If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                sql &= " AND mp.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin >= '" & FixQuotes(mesinAwal) & "' "
            ElseIf Len(mesinAwal) > 0 Then
                sql &= " AND mp.mesin <= '" & FixQuotes(mesinAkhir) & "' "
            End If
            'filter kelas
            If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                sql &= " AND mp.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas >= '" & FixQuotes(kategoriAwal) & "' "
            ElseIf Len(kategoriAwal) > 0 Then
                sql &= " AND mp.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
            End If
            'filter subkelas
            If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                sql &= " AND mp.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
            ElseIf Len(kelompokAwal) > 0 Then
                sql &= " AND mp.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
            End If
            sql &= " ORDER BY mp.mesin, mp.tgl, mp.kelas, mp.subkelas "
            dtPlot = AsDataTableAmbilDariDB(sql)
            If dtPlot.Rows.Count > 0 Then

                'AMBIL PLAN
                sql = "  SELECT pl.mesin, pl.tgl, pl.kelas, pl.subkelas, pl.jml, pl.satuan, pl.nilaisatuan, pl.jmlbarang, pl.satuanbarang, pl.matauang, pl.kurs, pl.harga, pl.hpp, pl.cabang, pl.lokasi, pl.gudang, pl.costcenter, pl.divisi, pl.subdivisi, pl.proyek, pl.catatan, pl.urutan, pl.jmlrealisasi, pl.statusrealisasi, pl.insertby, pl.isclose, pl.inputuser, pl.inputtgl, pl.modifikasiuser, pl.modifikasitgl, pl.customtext1, pl.customtext2, pl.customtext3, pl.customtext4, pl.customtext5, pl.customtext6, pl.customtext7, pl.customtext8, pl.customtext9, pl.customtext10, pl.customint1, pl.customint2, pl.customint3, pl.customint4, pl.customint5, pl.customint6, pl.customint7, pl.customint8, pl.customint9, pl.customint10,  pl.customdbl1, pl.customdbl2, pl.customdbl3, pl.customdbl4, pl.customdbl5, pl.customdbl6, pl.customdbl7, pl.customdbl8, pl.customdbl9, pl.customdbl10, pl.customdate1, pl.customdate2, pl.customdate3, pl.customdate4, pl.customdate5, pl.customdate6, pl.customdate7, pl.customdate8, pl.customdate9, pl.customdate10, IFNULL(m.mnama,'') as mesinnama, IFNULL(cl.cnama,'') as kelasnama, IFNULL(sc.scnama,'') as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama, pl.idpdpdetail, IFNULL(pdpd.idpdp,0) as idpdp, pl.idsodetail, pl.idbarang, pl.namabarang, pl.tipebarang, pl.jmljam, IFNULL(so.sonotransaksi,'') as sonotransaksi, IFNULL(i.bkode,'') as bkode, pl.idplan, i.bcustom12 as maxso, i.bcustom13 as kpj "
                sql &= " FROM m6_production_planning pl "
                sql &= " LEFT JOIN m1_machine m ON pl.mesin = m.mkode "
                sql &= " LEFT JOIN m1_class cl on pl.kelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON pl.subkelas = sc.sckode "
                sql &= " LEFT JOIN m0_user u1 ON pl.inputuser = u1.userid "
                sql &= " LEFT JOIN m0_user u2 ON pl.modifikasiuser = u2.userid "
                sql &= " LEFT JOIN m6_pdp_detail pdpd ON pl.idpdpdetail = pdpd.idpdpdetail "
                sql &= " LEFT JOIN m5_so so ON pl.idsodetail = so.soid "
                sql &= " LEFT JOIN m1_item i ON pl.idbarang = i.bid "
                'sql &= " WHERE (pl.insertby = 1 OR pl.isclose = 1) "
                sql &= " WHERE pl.tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
                'filter mesin
                If Len(mesinAwal) > 0 And Len(mesinAkhir) > 0 Then
                    sql &= " AND pl.mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "'"
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin >= '" & FixQuotes(mesinAwal) & "' "
                ElseIf Len(mesinAwal) > 0 Then
                    sql &= " AND pl.mesin <= '" & FixQuotes(mesinAkhir) & "' "
                End If
                'filter kelas
                If Len(kategoriAwal) > 0 And Len(kategoriAkhir) > 0 Then
                    sql &= " AND pl.kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "'"
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas >= '" & FixQuotes(kategoriAwal) & "' "
                ElseIf Len(kategoriAwal) > 0 Then
                    sql &= " AND pl.kelas <= '" & FixQuotes(kategoriAkhir) & "' "
                End If
                'filter subkelas
                If Len(kelompokAwal) > 0 And Len(kelompokAkhir) > 0 Then
                    sql &= " AND pl.subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "'"
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas >= '" & FixQuotes(kelompokAwal) & "' "
                ElseIf Len(kelompokAwal) > 0 Then
                    sql &= " AND pl.subkelas <= '" & FixQuotes(kelompokAkhir) & "' "
                End If
                sql &= " ORDER BY pl.tgl, pl.mesin, pl.urutan, pl.kelas, pl.subkelas, so.sonotransaksi, i.bkode "
                dtPlan = AsDataTableAmbilDariDB(sql)
                If dtPlan.Rows.Count > 0 Then
                    Dim vRow As Double = 1, vJmlJam As Double = 0, vJmlBarang As Double = 0, vJmlProd As Double = 0
                    For Each dr1 As DataRow In dtPlan.Rows
                        'INSERT KE DATATABLE DETAIL
                        If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dr1("mesin") & "~" & dr1("tgl") & "~" & dr1("kelas") & "~" & dr1("subkelas") & "~" & dr1("jml") & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & dr1("jmlbarang") & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & dr1("hpp") & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & dr1("urutan") & "~" & dr1("jmlrealisasi") & "~" & dr1("statusrealisasi") & "~" & dr1("insertby") & "~" & dr1("isclose") & "~" & dr1("inputuser") & "~" & dr1("inputtgl") & "~" & dr1("modifikasiuser") & "~" & dr1("modifikasitgl") & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & dr1("customtext4") & "~" & dr1("customtext5") & "~" & dr1("customtext6") & "~" & dr1("customtext7") & "~" & dr1("customtext8") & "~" & dr1("customtext9") & "~" & dr1("customtext10") & "~" & dr1("customint1") & "~" & dr1("customint2") & "~" & dr1("customint3") & "~" & dr1("customint4") & "~" & dr1("customint5") & "~" & dr1("customint6") & "~" & dr1("customint7") & "~" & dr1("customint8") & "~" & dr1("customint9") & "~" & dr1("customint10") & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & dr1("customdbl4") & "~" & dr1("customdbl5") & "~" & dr1("customdbl6") & "~" & dr1("customdbl7") & "~" & dr1("customdbl8") & "~" & dr1("customdbl9") & "~" & dr1("customdbl10") & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & dr1("customdate4") & "~" & dr1("customdate5") & "~" & dr1("customdate6") & "~" & dr1("customdate7") & "~" & dr1("customdate8") & "~" & dr1("customdate9") & "~" & dr1("customdate10") & "~" & dr1("mesinnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & dr1("inputusernama") & "~" & dr1("modifikasiusernama") & "~" & dr1("idpdpdetail") & "~" & dr1("idpdp") & "~" & dr1("idsodetail") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & dr1("jmljam") & "~" & dr1("sonotransaksi") & "~" & dr1("bkode") & "~" & dr1("idplan") & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                            result(2) = "Plan Row : " & vRow & " - insert into generate datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE PLOT
                        'mesin, tgl, kelas, subkelas, jml
                        vJmlJam = AsDataTableDSum(dtPlot, "jml", "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'")
                        vJmlJam += Double.Parse(dr1("jmljam"))
                        If AsDataTableUpdateData(dtPlot, "mesin = '" & dr1("mesin") & "' AND tgl = '" & dr1("tgl") & "'", "jml", Math.Round(vJmlJam, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update Plotting Machine datatable failed." : GoTo selesai
                        End If

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        'idsodetail, idbarang
                        vJmlProd = AsDataTableDSum(dtSO, "jmlrealisasi", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        vJmlProd += Double.Parse(dr1("jmlbarang"))
                        vJmlBarang = AsDataTableDSum(dtSO, "jmlbarang", "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'")
                        If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idsodetail") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", vJmlProd & "~" & Math.Round(vJmlBarang - vJmlProd, 5)) = False Then
                            result(2) = "Plan Row : " & vRow & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        vRow += 1
                    Next
                End If

                Dim cKelas As String = "", cSubKelas As String = "", cJmlBarang As Double = 0, cJmlRealisasi As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
                Dim dtCPlot As New DataTable, cKPJ As Double = 0, cSisaJam As Double = 0, cJamButuh As Double = 0, cJamPakai As Double = 0
                Dim cUrutan As Double = 1, cMaxRc As Double = 0

                'PERULANGAN SET PRODUCTION PLANNING
setPlan:
                'SimpanLogWsToFile(6, "Pdp", "setPlan:")
                'dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0", "tglkirimso, tglso, idso, urutan, idbarang")
                dtSOTemp = AsDataTableFilterSortDt(dtSO, "jmlsisa > 0 AND jmlsisa >= " & minPlan & " AND customdbl3 >= " & minPlan & "", "tglkirimso, tglso, idso, urutan, idbarang")
                dtPlotTemp = AsDataTableFilterSortDt(dtPlot, "jml < " & vMaxJam & "", "mesin, tgl, kelas, subkelas")

                If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then
                    'SimpanLogWsToFile(6, "Pdp", "If dtSOTemp.Rows.Count > 0 And dtPlotTemp.Rows.Count > 0 Then")

                    For Each dr1 As DataRow In dtSOTemp.Rows
                        'Dim dr1 As DataRow = dtSOTemp.Rows(0)
                        cKelas = dr1("bakelas") : cSubKelas = dr1("bsubkelas")
                        cJmlBarang = dr1("jmlbarang") : cJmlRealisasi = dr1("jmlrealisasi") : cJmlSisa = dr1("jmlsisa")
                        cKPJ = dr1("kpj") : cMaxRc = dr1("customdbl3")

                        dtCPlot = AsDataTableFilterSortDt(dtPlotTemp, "jml < " & vMaxJam & " AND kelas = '" & cKelas & "' AND subkelas = '" & cSubKelas & "'", "mesin, tgl, kelas, subkelas")
                        If dtCPlot.Rows.Count > 0 Then
                            'SimpanLogWsToFile(6, "Pdp", "If dtCPlot.Rows.Count > 0 Then")

                            If cKPJ <= 0 Then
                                result(2) = " Production Capacity/Hour for item " & dr1("bkode") & " can't be less than or equal to zero." : GoTo selesai
                            End If

                            If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                                cJmlSisa = cMaxRc
                            End If

                            cSisaJam = vMaxJam - dtCPlot.Rows(0)("jml")
                            cJamButuh = cJmlSisa / cKPJ

                            If cJamButuh >= cSisaJam Then
                                cJamPakai = cSisaJam
                            Else
                                cJamPakai = cJamButuh
                            End If
                            cJmlProd = cKPJ * cJamPakai

                            'INSERT KE DATATABLE DETAIL
                            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~mesinnama~kelasnama~subkelasnama~inputusernama~modifikasiusernama~idpdpdetail~idpdp~idsodetail~idbarang~namabarang~tipebarang~jmljam~sonotransaksi~bkode~idplan~maxso~kpj", dtCPlot.Rows(0)("mesin") & "~" & dtCPlot.Rows(0)("tgl") & "~" & dr1("bakelas") & "~" & dr1("bsubkelas") & "~" & cJmlProd & "~" & dr1("satuan") & "~" & dr1("nilaisatuan") & "~" & cJmlProd & "~" & dr1("satuanbarang") & "~" & dr1("matauang") & "~" & dr1("kurs") & "~" & dr1("harga") & "~" & 0 & "~" & dr1("cabang") & "~" & dr1("lokasi") & "~" & dr1("gudang") & "~" & dr1("costcenter") & "~" & dr1("divisi") & "~" & dr1("subdivisi") & "~" & dr1("proyek") & "~" & dr1("catatan") & "~" & cUrutan & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & userid & "~" & "1971-01-01 00:00:00" & "~" & 0 & "~" & "1971-01-01 00:00:00" & "~" & dr1("customtext1") & "~" & dr1("customtext2") & "~" & dr1("customtext3") & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdbl1") & "~" & dr1("customdbl2") & "~" & dr1("customdbl3") & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & dr1("customdate1") & "~" & dr1("customdate2") & "~" & dr1("customdate3") & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & "1900-01-01" & "~" & dtCPlot.Rows(0)("mnama") & "~" & dr1("kelasnama") & "~" & dr1("subkelasnama") & "~" & "" & "~" & "" & "~" & 0 & "~" & 0 & "~" & dr1("idso") & "~" & dr1("idbarang") & "~" & dr1("namabarang") & "~" & dr1("tipebarang") & "~" & cJamPakai & "~" & dr1("nomorso") & "~" & dr1("bkode") & "~" & 0 & "~" & dr1("maxso") & "~" & dr1("kpj")) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - insert into generate datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "AsDataTableTambahData(dtdetail")

                            'UPDATE DATATABLE PLOT
                            'mesin, tgl, kelas, subkelas, jml
                            If AsDataTableUpdateData(dtPlot, "mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "'", "jml", Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update Plotting Machine datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtPlot mesin = '" & dtCPlot.Rows(0)("mesin") & "' AND tgl = '" & dtCPlot.Rows(0)("tgl") & "' jml " & Math.Round(dtCPlot.Rows(0)("jml") + cJamPakai, 5))

                            'UPDATE DATATABLE SO
                            'idso, idbarang
                            'If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(cJmlBarang - (cJmlRealisasi + cJmlProd), 5)) = False Then
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlrealisasi~jmlsisa", cJmlRealisasi + cJmlProd & "~" & Math.Round(dr1("jmlsisa") - (cJmlProd), 5)) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtSO idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & " jmlrealisasi~jmlsisa " & cJmlRealisasi + cJmlProd & "~" & Math.Round(dr1("jmlsisa") - (cJmlProd), 5))

                        Else
                            If AsDataTableUpdateData(dtSO, "idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "'", "jmlsisa", 0) = False Then
                                result(2) = dr1("nomorso") & " : " & dr1("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                            End If
                            'SimpanLogWsToFile(6, "Pdp", "dtSO else idso = '" & dr1("idso") & "' AND idbarang = '" & dr1("idbarang") & "' jmlsisa " & 0)
                        End If

                        cUrutan += 1

                        GoTo setPlan
                    Next
                End If

            Else
                result(2) = "Plotting Machine data not found." : GoTo selesai
            End If

        Else
            result(2) = "SO Backorder Production data not found." : GoTo selesai
        End If


        If dtdetail.Rows.Count > 0 Then
            dtdetail = AsDataTableFilterSortDt(dtdetail, "", "mesin, tgl, urutan, kelas, subkelas, sonotransaksi, bkode")
            For Each dr As DataRow In dtdetail.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptField,
                     FxDB(dr("idpdpdetail"), "0"), sptField,
                     FxDB(dr("idpdp"), "0"), sptField,
                     FxDB(dr("idsodetail"), "0"), sptField,
                     FxDB(dr("idbarang"), "0"), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jmljam"), "0"), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("idplan"), "0"), sptField,
                     FxDB(dr("maxso"), "0"), sptField,
                     FxDB(dr("kpj"), "0"), sptRow)

                'TAMBAH FILTER UNTUK DATA ALLOWANCE
                If Len(ftAllowance) > 0 Then
                    ftAllowance = ftAllowance & " OR "
                End If
                ftAllowance = ftAllowance & " (so.soid = '" & FixDouble(FxDB(dr("idsodetail"), "0")) & "' AND sop.idbarang = '" & FixDouble(FxDB(dr("idbarang"), "0")) & "') "

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            'AMBIL DATA ALLOWANCE
            If Len(ftAllowance) > 0 Then
                sql = "  SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal "
                sql &= " FROM m5_so_production sop "
                sql &= " JOIN m5_so so ON sop.idso = so.soid "
                sql &= " AND (" & ftAllowance & ")"
                sql &= " JOIN m1_item i ON sop.idbarang = i.bid "
                sql &= " LEFT JOIN m1_class cl ON i.bakelas = cl.ckode "
                sql &= " LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode"
                Dim dtallowance As New DataTable
                dtallowance = AmbilData("aplikasi1-m1_allowance", "", "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang", sql) ' Ambil data ke databases
                For Each dr As DataRow In dtallowance.Rows
                    strAllowance = String.Concat(strAllowance,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(dr("jml"), 0), sptField,
                             FxDB(dr("jmljam"), 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(dr("jmlbarang"), 0), sptField,
                             FxDB(dr("jmlsisa"), 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)
                Next
                If strAllowance.Length > 0 Then strAllowance = strAllowance.Substring(0, strAllowance.Length - sptRow.Length) Else strAllowance = strAllowance

            End If


            result(1) = 1
            resultPaging(0) = 0 'Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = 0 'Math.Abs(Val(pg1.isNext))
            resultPaging(2) = 0 'Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = 0 'pg1.countPage
            resultPaging(4) = dtdetail.Rows.Count 'pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, strAllowance)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama, idpdpdetail, idpdp, idsodetail, idbarang, namabarang, tipebarang, jmljam, sonotransaksi, bkode, idplan, maxso, kpj" & sptSubParam & "bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTakeSo20200518(ByVal param As String) As String
        'M6_PdpTakeSo --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

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
        Dim dt As New DataTable, vSisaJam As Double = 0

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

        'AMBIL SISA JAM
        If IsNumeric(paramSplit(5)) Then
            If paramSplit(5) > 0 Then
                vSisaJam = paramSplit(5)
            End If
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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        Else
            sorting = "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then

            Dim cKPJ As Double = 0, cMaxRc As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
            Dim cJamButuh As Double = 0, cJamPakai As Double = 0, cJmlBarang As Double = 0

            If vSisaJam > 0 Then

setPlan:
                dt = AsDataTableFilterSortDt(dt, "jmlsisa > 0 ")
                If dt.Rows.Count > 0 And vSisaJam > 0 Then

                    For Each dr As DataRow In dt.Rows
                        cJmlBarang = dr("jmlsisa")
                        cJmlSisa = dr("jmlsisa")
                        cKPJ = dr("kpj") : cMaxRc = dr("customdbl3")

                        If cKPJ <= 0 Then
                            result(2) = " Production Capacity/Hour for item " & dr("bkode") & " can't be less than or equal to zero." : GoTo selesai
                        End If

                        If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                            cJmlSisa = cMaxRc
                        End If

                        cJamButuh = cJmlSisa / cKPJ

                        If cJamButuh >= vSisaJam Then
                            cJamPakai = vSisaJam
                        Else
                            cJamPakai = cJamButuh
                        End If
                        cJmlProd = cKPJ * cJamPakai

                        search = String.Concat(search,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJamPakai, 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
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
                             AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)

                        'UPDATE SISA JAM
                        vSisaJam = vSisaJam - cJamPakai

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        If AsDataTableUpdateData(dt, "idso = '" & dr("idso") & "' AND idbarang = '" & dr("idbarang") & "'", "jmlsisa", Math.Round(cJmlBarang - cJmlProd, 5)) = False Then
                            result(2) = dr("nomorso") & " : " & dr("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        GoTo setPlan

                    Next

                End If

            Else

                For Each dr As DataRow In dt.Rows

                    search = String.Concat(search,
                         FxDB(dr("bakelas"), ""), sptField,
                         FxDB(dr("kelasnama"), ""), sptField,
                         FxDB(dr("bsubkelas"), ""), sptField,
                         FxDB(dr("subkelasnama"), ""), sptField,
                         FxDB(dr("maxso"), 0), sptField,
                         FxDB(dr("kpj"), 0), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
                         FxDB(dr("bkode"), ""), sptField,
                         FxDB(dr("nomorso"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                         FxDB(dr("namabarang"), ""), sptField,
                         FxDB(dr("tipebarang"), ""), sptField,
                         FxDB(dr("jml"), 0), sptField,
                         FxDB(dr("jmljam"), 0), sptField,
                         FxDB(dr("satuan"), ""), sptField,
                         FxDB(dr("nilaisatuan"), 0), sptField,
                         FxDB(dr("jmlbarang"), 0), sptField,
                         FxDB(dr("jmlsisa"), 0), sptField,
                         FxDB(dr("satuanbarang"), ""), sptField,
                         FxDB(dr("matauang"), ""), sptField,
                         FxDB(dr("kurs"), 0), sptField,
                         FxDB(dr("harga"), 0), sptField,
                         FxDB(dr("diskon"), ""), sptField,
                         FxDB(dr("jmldiskon"), 0), sptField,
                         FxDB(dr("pajak1"), ""), sptField,
                         FxDB(dr("jmlpajak1"), 0), sptField,
                         FxDB(dr("pajak2"), ""), sptField,
                         FxDB(dr("jmlpajak2"), 0), sptField,
                         FxDB(dr("cabang"), ""), sptField,
                         FxDB(dr("lokasi"), ""), sptField,
                         FxDB(dr("gudang"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("urutan"), 0), sptField,
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
                         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
                Next

            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTakeSo20200609(ByVal param As String) As String
        'M6_PdpTakeSo --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

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
        Dim dt As New DataTable, vSisaJam As Double = 0

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

        'AMBIL SISA JAM
        If IsNumeric(paramSplit(5)) Then
            If paramSplit(5) > 0 Then
                vSisaJam = paramSplit(5)
            End If
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
            Filter = Filter.Replace("idbarang", "sop.idbarang")
            Filter = Filter.Replace("namabarang", "sop.namabarang")
            Filter = Filter.Replace("satuanbarang", "sop.satuanbarang")
            Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5)")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        Else
            sorting = "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang HAVING jmlsisa > 0", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then

            Dim cKPJ As Double = 0, cMaxRc As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
            Dim cJamButuh As Double = 0, cJamPakai As Double = 0, cJmlBarang As Double = 0

            If vSisaJam > 0 Then

setPlan:
                dt = AsDataTableFilterSortDt(dt, "jmlsisa > 0 ")
                If dt.Rows.Count > 0 And vSisaJam > 0 Then

                    For Each dr As DataRow In dt.Rows
                        cJmlBarang = dr("jmlsisa")
                        cJmlSisa = dr("jmlsisa")
                        cKPJ = dr("kpj") : cMaxRc = dr("customdbl3")

                        If cKPJ <= 0 Then
                            result(2) = " Production Capacity/Hour for item " & dr("bkode") & " can't be less than or equal to zero." : GoTo selesai
                        End If

                        If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                            cJmlSisa = cMaxRc
                        End If

                        cJamButuh = cJmlSisa / cKPJ

                        If cJamButuh >= vSisaJam Then
                            cJamPakai = vSisaJam
                        Else
                            cJamPakai = cJamButuh
                        End If
                        cJmlProd = cKPJ * cJamPakai

                        search = String.Concat(search,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJamPakai, 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
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
                             AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)

                        'UPDATE SISA JAM
                        vSisaJam = vSisaJam - cJamPakai

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        If AsDataTableUpdateData(dt, "idso = '" & dr("idso") & "' AND idbarang = '" & dr("idbarang") & "'", "jmlsisa", Math.Round(cJmlBarang - cJmlProd, 5)) = False Then
                            result(2) = dr("nomorso") & " : " & dr("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        GoTo setPlan

                    Next

                End If

            Else

                For Each dr As DataRow In dt.Rows

                    search = String.Concat(search,
                         FxDB(dr("bakelas"), ""), sptField,
                         FxDB(dr("kelasnama"), ""), sptField,
                         FxDB(dr("bsubkelas"), ""), sptField,
                         FxDB(dr("subkelasnama"), ""), sptField,
                         FxDB(dr("maxso"), 0), sptField,
                         FxDB(dr("kpj"), 0), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
                         FxDB(dr("bkode"), ""), sptField,
                         FxDB(dr("nomorso"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                         FxDB(dr("namabarang"), ""), sptField,
                         FxDB(dr("tipebarang"), ""), sptField,
                         FxDB(dr("jml"), 0), sptField,
                         FxDB(dr("jmljam"), 0), sptField,
                         FxDB(dr("satuan"), ""), sptField,
                         FxDB(dr("nilaisatuan"), 0), sptField,
                         FxDB(dr("jmlbarang"), 0), sptField,
                         FxDB(dr("jmlsisa"), 0), sptField,
                         FxDB(dr("satuanbarang"), ""), sptField,
                         FxDB(dr("matauang"), ""), sptField,
                         FxDB(dr("kurs"), 0), sptField,
                         FxDB(dr("harga"), 0), sptField,
                         FxDB(dr("diskon"), ""), sptField,
                         FxDB(dr("jmldiskon"), 0), sptField,
                         FxDB(dr("pajak1"), ""), sptField,
                         FxDB(dr("jmlpajak1"), 0), sptField,
                         FxDB(dr("pajak2"), ""), sptField,
                         FxDB(dr("jmlpajak2"), 0), sptField,
                         FxDB(dr("cabang"), ""), sptField,
                         FxDB(dr("lokasi"), ""), sptField,
                         FxDB(dr("gudang"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("urutan"), 0), sptField,
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
                         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
                Next

            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTakeSo20201026(ByVal param As String) As String
        'M6_PdpTakeSo --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal

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
        Dim dt As New DataTable, vSisaJam As Double = 0

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

        'AMBIL SISA JAM
        If IsNumeric(paramSplit(5)) Then
            If paramSplit(5) > 0 Then
                vSisaJam = paramSplit(5)
            End If
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
            Filter = Filter.Replace("idbarang", "sop.idbarang")
            Filter = Filter.Replace("namabarang", "sop.namabarang")
            Filter = Filter.Replace("satuanbarang", "sop.satuanbarang")
            'Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5)")
            Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5)")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        Else
            sorting = "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang "
        sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND sop.statuspl <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang HAVING jmlsisa > 0", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then

            Dim cKPJ As Double = 0, cMaxRc As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
            Dim cJamButuh As Double = 0, cJamPakai As Double = 0, cJmlBarang As Double = 0

            If vSisaJam > 0 Then

setPlan:
                dt = AsDataTableFilterSortDt(dt, "jmlsisa > 0 ")
                If dt.Rows.Count > 0 And vSisaJam > 0 Then

                    For Each dr As DataRow In dt.Rows
                        cJmlBarang = dr("jmlsisa")
                        cJmlSisa = dr("jmlsisa")
                        cKPJ = dr("kpj") : cMaxRc = dr("customdbl3")

                        If cKPJ <= 0 Then
                            result(2) = " Production Capacity/Hour for item " & dr("bkode") & " can't be less than or equal to zero." : GoTo selesai
                        End If

                        If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                            cJmlSisa = cMaxRc
                        End If

                        cJamButuh = cJmlSisa / cKPJ

                        If cJamButuh >= vSisaJam Then
                            cJamPakai = vSisaJam
                        Else
                            cJamPakai = cJamButuh
                        End If
                        cJmlProd = cKPJ * cJamPakai

                        search = String.Concat(search,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJamPakai, 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)

                        'UPDATE SISA JAM
                        vSisaJam = vSisaJam - cJamPakai

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        If AsDataTableUpdateData(dt, "idso = '" & dr("idso") & "' AND idbarang = '" & dr("idbarang") & "'", "jmlsisa", Math.Round(cJmlBarang - cJmlProd, 5)) = False Then
                            result(2) = dr("nomorso") & " : " & dr("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        GoTo setPlan

                    Next

                End If

            Else

                For Each dr As DataRow In dt.Rows

                    search = String.Concat(search,
                         FxDB(dr("bakelas"), ""), sptField,
                         FxDB(dr("kelasnama"), ""), sptField,
                         FxDB(dr("bsubkelas"), ""), sptField,
                         FxDB(dr("subkelasnama"), ""), sptField,
                         FxDB(dr("maxso"), 0), sptField,
                         FxDB(dr("kpj"), 0), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
                         FxDB(dr("bkode"), ""), sptField,
                         FxDB(dr("nomorso"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                         FxDB(dr("namabarang"), ""), sptField,
                         FxDB(dr("tipebarang"), ""), sptField,
                         FxDB(dr("jml"), 0), sptField,
                         FxDB(dr("jmljam"), 0), sptField,
                         FxDB(dr("satuan"), ""), sptField,
                         FxDB(dr("nilaisatuan"), 0), sptField,
                         FxDB(dr("jmlbarang"), 0), sptField,
                         FxDB(dr("jmlsisa"), 0), sptField,
                         FxDB(dr("satuanbarang"), ""), sptField,
                         FxDB(dr("matauang"), ""), sptField,
                         FxDB(dr("kurs"), 0), sptField,
                         FxDB(dr("harga"), 0), sptField,
                         FxDB(dr("diskon"), ""), sptField,
                         FxDB(dr("jmldiskon"), 0), sptField,
                         FxDB(dr("pajak1"), ""), sptField,
                         FxDB(dr("jmlpajak1"), 0), sptField,
                         FxDB(dr("pajak2"), ""), sptField,
                         FxDB(dr("jmlpajak2"), 0), sptField,
                         FxDB(dr("cabang"), ""), sptField,
                         FxDB(dr("lokasi"), ""), sptField,
                         FxDB(dr("gudang"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("urutan"), 0), sptField,
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
                         FxDB(dr("jmlso"), 0), sptField,
                         FxDB(dr("jmlallowance"), 0), sptField,
                         FxDB(dr("prosentaseallowance"), 0), sptField,
                         FxDB(dr("jmltotal"), 0), sptRow)
                Next

            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTakeSo20210205(ByVal param As String) As String
        'M6_PdpTakeSo --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal

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
        Dim dt As New DataTable, vSisaJam As Double = 0

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

        'AMBIL SISA JAM
        If IsNumeric(paramSplit(5)) Then
            If paramSplit(5) > 0 Then
                vSisaJam = paramSplit(5)
            End If
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
            Filter = Filter.Replace("idbarang", "sop.idbarang")
            Filter = Filter.Replace("namabarang", "sop.namabarang")
            Filter = Filter.Replace("satuanbarang", "sop.satuanbarang")
            'Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5)")
            Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5)")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        Else
            sorting = "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang "
        sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND sop.statuspl <> 2 AND so.solokasi <> 'L' JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang HAVING jmlsisa > 0", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then

            Dim cKPJ As Double = 0, cMaxRc As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
            Dim cJamButuh As Double = 0, cJamPakai As Double = 0, cJmlBarang As Double = 0

            If vSisaJam > 0 Then

setPlan:
                dt = AsDataTableFilterSortDt(dt, "jmlsisa > 0 ")
                If dt.Rows.Count > 0 And vSisaJam > 0 Then

                    For Each dr As DataRow In dt.Rows
                        cJmlBarang = dr("jmlsisa")
                        cJmlSisa = dr("jmlsisa")
                        cKPJ = dr("kpj") : cMaxRc = dr("customdbl3")

                        If cKPJ <= 0 Then
                            result(2) = " Production Capacity/Hour for item " & dr("bkode") & " can't be less than or equal to zero." : GoTo selesai
                        End If

                        If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                            cJmlSisa = cMaxRc
                        End If

                        cJamButuh = cJmlSisa / cKPJ

                        If cJamButuh >= vSisaJam Then
                            cJamPakai = vSisaJam
                        Else
                            cJamPakai = cJamButuh
                        End If
                        cJmlProd = cKPJ * cJamPakai

                        search = String.Concat(search,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJamPakai, 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)

                        'UPDATE SISA JAM
                        vSisaJam = vSisaJam - cJamPakai

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        If AsDataTableUpdateData(dt, "idso = '" & dr("idso") & "' AND idbarang = '" & dr("idbarang") & "'", "jmlsisa", Math.Round(cJmlBarang - cJmlProd, 5)) = False Then
                            result(2) = dr("nomorso") & " : " & dr("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        GoTo setPlan

                    Next

                End If

            Else

                For Each dr As DataRow In dt.Rows

                    search = String.Concat(search,
                         FxDB(dr("bakelas"), ""), sptField,
                         FxDB(dr("kelasnama"), ""), sptField,
                         FxDB(dr("bsubkelas"), ""), sptField,
                         FxDB(dr("subkelasnama"), ""), sptField,
                         FxDB(dr("maxso"), 0), sptField,
                         FxDB(dr("kpj"), 0), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
                         FxDB(dr("bkode"), ""), sptField,
                         FxDB(dr("nomorso"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                         FxDB(dr("namabarang"), ""), sptField,
                         FxDB(dr("tipebarang"), ""), sptField,
                         FxDB(dr("jml"), 0), sptField,
                         FxDB(dr("jmljam"), 0), sptField,
                         FxDB(dr("satuan"), ""), sptField,
                         FxDB(dr("nilaisatuan"), 0), sptField,
                         FxDB(dr("jmlbarang"), 0), sptField,
                         FxDB(dr("jmlsisa"), 0), sptField,
                         FxDB(dr("satuanbarang"), ""), sptField,
                         FxDB(dr("matauang"), ""), sptField,
                         FxDB(dr("kurs"), 0), sptField,
                         FxDB(dr("harga"), 0), sptField,
                         FxDB(dr("diskon"), ""), sptField,
                         FxDB(dr("jmldiskon"), 0), sptField,
                         FxDB(dr("pajak1"), ""), sptField,
                         FxDB(dr("jmlpajak1"), 0), sptField,
                         FxDB(dr("pajak2"), ""), sptField,
                         FxDB(dr("jmlpajak2"), 0), sptField,
                         FxDB(dr("cabang"), ""), sptField,
                         FxDB(dr("lokasi"), ""), sptField,
                         FxDB(dr("gudang"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("urutan"), 0), sptField,
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
                         FxDB(dr("jmlso"), 0), sptField,
                         FxDB(dr("jmlallowance"), 0), sptField,
                         FxDB(dr("prosentaseallowance"), 0), sptField,
                         FxDB(dr("jmltotal"), 0), sptRow)
                Next

            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTakeSo(ByVal param As String) As String
        'M6_PdpTakeSo --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal, tgl

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
        Dim dt As New DataTable, vSisaJam As Double = 0

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

        'AMBIL SISA JAM
        If IsNumeric(paramSplit(5)) Then
            If paramSplit(5) > 0 Then
                vSisaJam = paramSplit(5)
            End If
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
            Filter = Filter.Replace("idbarang", "sop.idbarang")
            Filter = Filter.Replace("namabarang", "sop.namabarang")
            Filter = Filter.Replace("satuanbarang", "sop.satuanbarang")
            'Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5)")
            Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5)")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        Else
            sorting = "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang "
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND sop.statuspl <> 2 AND so.solokasi <> 'L' JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlpl,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal, pdpl.tgl as tgl FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 AND sop.statuspl <> 2 AND so.solokasi <> 'L' JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang HAVING jmlsisa > 0", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then

            Dim cKPJ As Double = 0, cMaxRc As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
            Dim cJamButuh As Double = 0, cJamPakai As Double = 0, cJmlBarang As Double = 0

            If vSisaJam > 0 Then

setPlan:
                dt = AsDataTableFilterSortDt(dt, "jmlsisa > 0 ")
                If dt.Rows.Count > 0 And vSisaJam > 0 Then

                    For Each dr As DataRow In dt.Rows
                        cJmlBarang = dr("jmlsisa")
                        cJmlSisa = dr("jmlsisa")
                        cKPJ = dr("kpj") : cMaxRc = dr("customdbl3")

                        If cKPJ <= 0 Then
                            result(2) = " Production Capacity/Hour for item " & dr("bkode") & " can't be less than or equal to zero." : GoTo selesai
                        End If

                        If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                            cJmlSisa = cMaxRc
                        End If

                        cJamButuh = cJmlSisa / cKPJ

                        If cJamButuh >= vSisaJam Then
                            cJamPakai = vSisaJam
                        Else
                            cJamPakai = cJamButuh
                        End If
                        cJmlProd = cKPJ * cJamPakai

                        search = String.Concat(search,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJamPakai, 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptField,
                             IIf(Len(FxDB(dr("tgl"), "")) > 0, AsFormatTanggal(FxDB(dr("tgl"), "1900-01-01"), formatTgl), ""), sptRow)

                        'UPDATE SISA JAM
                        vSisaJam = vSisaJam - cJamPakai

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        If AsDataTableUpdateData(dt, "idso = '" & dr("idso") & "' AND idbarang = '" & dr("idbarang") & "'", "jmlsisa", Math.Round(cJmlBarang - cJmlProd, 5)) = False Then
                            result(2) = dr("nomorso") & " : " & dr("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        GoTo setPlan

                    Next

                End If

            Else

                For Each dr As DataRow In dt.Rows

                    search = String.Concat(search,
                         FxDB(dr("bakelas"), ""), sptField,
                         FxDB(dr("kelasnama"), ""), sptField,
                         FxDB(dr("bsubkelas"), ""), sptField,
                         FxDB(dr("subkelasnama"), ""), sptField,
                         FxDB(dr("maxso"), 0), sptField,
                         FxDB(dr("kpj"), 0), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
                         FxDB(dr("bkode"), ""), sptField,
                         FxDB(dr("nomorso"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                         FxDB(dr("namabarang"), ""), sptField,
                         FxDB(dr("tipebarang"), ""), sptField,
                         FxDB(dr("jml"), 0), sptField,
                         FxDB(dr("jmljam"), 0), sptField,
                         FxDB(dr("satuan"), ""), sptField,
                         FxDB(dr("nilaisatuan"), 0), sptField,
                         FxDB(dr("jmlbarang"), 0), sptField,
                         FxDB(dr("jmlsisa"), 0), sptField,
                         FxDB(dr("satuanbarang"), ""), sptField,
                         FxDB(dr("matauang"), ""), sptField,
                         FxDB(dr("kurs"), 0), sptField,
                         FxDB(dr("harga"), 0), sptField,
                         FxDB(dr("diskon"), ""), sptField,
                         FxDB(dr("jmldiskon"), 0), sptField,
                         FxDB(dr("pajak1"), ""), sptField,
                         FxDB(dr("jmlpajak1"), 0), sptField,
                         FxDB(dr("pajak2"), ""), sptField,
                         FxDB(dr("jmlpajak2"), 0), sptField,
                         FxDB(dr("cabang"), ""), sptField,
                         FxDB(dr("lokasi"), ""), sptField,
                         FxDB(dr("gudang"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("urutan"), 0), sptField,
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
                         FxDB(dr("jmlso"), 0), sptField,
                         FxDB(dr("jmlallowance"), 0), sptField,
                         FxDB(dr("prosentaseallowance"), 0), sptField,
                         FxDB(dr("jmltotal"), 0), sptField,
                         IIf(Len(FxDB(dr("tgl"), "")) > 0, AsFormatTanggal(FxDB(dr("tgl"), "1900-01-01"), formatTgl), ""), sptRow)
                Next

            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal, tgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdpTakeSoPd(ByVal param As String) As String
        'M6_PdpTakeSoPd --------------------------------------------------------
        'bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, 
        'idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, 
        'tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal

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
        Dim dt As New DataTable, vSisaJam As Double = 0

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

        'AMBIL SISA JAM
        If IsNumeric(paramSplit(5)) Then
            If paramSplit(5) > 0 Then
                vSisaJam = paramSplit(5)
            End If
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
            Filter = Filter.Replace("idbarang", "sop.idbarang")
            Filter = Filter.Replace("namabarang", "sop.namabarang")
            Filter = Filter.Replace("satuanbarang", "sop.satuanbarang")
            'Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5)")
            Filter = Filter.Replace("jmlsisa", "ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi, 5)")
        End If
        If (pagingSplit(3).Length > 0) Then
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        Else
            sorting = "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang"
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdp_v")
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "
        'sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jml, IFNULL(ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlbarang, ROUND(sop.jmlbarang - sop.jmlrealisasi - SUM(IFNULL(pdpl.jmlbarang,0)),5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3 FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode LEFT JOIN m6_production_planning pdpl ON sop.idso = pdpl.idsodetail AND sop.idbarang = pdpl.idbarang "
        sql = "SELECT i.bakelas, cl.cnama as kelasnama, i.bsubkelas, sc.scnama as subkelasnama, i.bcustom12 as maxso, i.bcustom13 as kpj, sop.idso, sop.idso as idsodetail, sop.idbarang, i.bkode, sop.nomorso, sop.tglso, sop.tglkirimso, sop.namabarang, sop.tipebarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi, 5) as jml, IFNULL(ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi,5) / i.bcustom13,0) as jmljam, sop.satuan, sop.nilaisatuan, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi,5) as jmlbarang, ROUND(sop.jmlbarang + sop.jmlpi - sop.jmlrealisasi,5) as jmlsisa, sop.satuanbarang, sop.matauang, sop.kurs, sop.harga, sop.diskon, sop.jmldiskon, sop.pajak1, sop.jmlpajak1, sop.pajak2, sop.jmlpajak2, sop.cabang, sop.lokasi, sop.gudang, sop.costcenter, sop.divisi, sop.subdivisi, sop.proyek, sop.catatan, sop.urutan, sop.jmlrealisasi, sop.statusrealisasi, sop.isclose, sop.customtext1, sop.customtext2, sop.customtext3, sop.customdbl1, sop.customdbl2, sop.customdbl3, sop.customdate1, sop.customdate2, sop.customdate3, sop.jmlbarang as jmlso, sop.jmlpi as jmlallowance, sop.statuspi as prosentaseallowance, sop.jmlbarang + sop.jmlpi as jmltotal FROM m5_so_production sop JOIN m5_so so ON sop.idso = so.soid AND so.sostatus IN(2,3) AND sop.statusrealisasi <> 2 JOIN m1_item i ON sop.idbarang = i.bid LEFT JOIN m1_class cl ON i.bakelas = cl.ckode LEFT JOIN m1_subclass sc ON i.bsubkelas = sc.sckode "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim, so.sotgl, so.soid, sop.urutan, sop.idbarang HAVING jmlsisa > 0", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then

            Dim cKPJ As Double = 0, cMaxRc As Double = 0, cJmlSisa As Double = 0, cJmlProd As Double = 0
            Dim cJamButuh As Double = 0, cJamPakai As Double = 0, cJmlBarang As Double = 0

            If vSisaJam > 0 Then

setPlan:
                dt = AsDataTableFilterSortDt(dt, "jmlsisa > 0 ")
                If dt.Rows.Count > 0 And vSisaJam > 0 Then

                    For Each dr As DataRow In dt.Rows
                        cJmlBarang = dr("jmlsisa")
                        cJmlSisa = dr("jmlsisa")
                        cKPJ = dr("kpj") : cMaxRc = dr("customdbl3")

                        If cKPJ <= 0 Then
                            result(2) = " Production Capacity/Hour for item " & dr("bkode") & " can't be less than or equal to zero." : GoTo selesai
                        End If

                        If cMaxRc > 0 And cJmlSisa > cMaxRc Then
                            cJmlSisa = cMaxRc
                        End If

                        cJamButuh = cJmlSisa / cKPJ

                        If cJamButuh >= vSisaJam Then
                            cJamPakai = vSisaJam
                        Else
                            cJamPakai = cJamButuh
                        End If
                        cJmlProd = cKPJ * cJamPakai

                        search = String.Concat(search,
                             FxDB(dr("bakelas"), ""), sptField,
                             FxDB(dr("kelasnama"), ""), sptField,
                             FxDB(dr("bsubkelas"), ""), sptField,
                             FxDB(dr("subkelasnama"), ""), sptField,
                             FxDB(dr("maxso"), 0), sptField,
                             FxDB(dr("kpj"), 0), sptField,
                             FxDB(dr("idso"), ""), sptField,
                             FxDB(dr("idsodetail"), ""), sptField,
                             FxDB(dr("idbarang"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("nomorso"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                             FxDB(dr("namabarang"), ""), sptField,
                             FxDB(dr("tipebarang"), ""), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJamPakai, 0), sptField,
                             FxDB(dr("satuan"), ""), sptField,
                             FxDB(dr("nilaisatuan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
                             FxDB(dr("satuanbarang"), ""), sptField,
                             FxDB(dr("matauang"), ""), sptField,
                             FxDB(dr("kurs"), 0), sptField,
                             FxDB(dr("harga"), 0), sptField,
                             FxDB(dr("diskon"), ""), sptField,
                             FxDB(dr("jmldiskon"), 0), sptField,
                             FxDB(dr("pajak1"), ""), sptField,
                             FxDB(dr("jmlpajak1"), 0), sptField,
                             FxDB(dr("pajak2"), ""), sptField,
                             FxDB(dr("jmlpajak2"), 0), sptField,
                             FxDB(dr("cabang"), ""), sptField,
                             FxDB(dr("lokasi"), ""), sptField,
                             FxDB(dr("gudang"), ""), sptField,
                             FxDB(dr("costcenter"), ""), sptField,
                             FxDB(dr("divisi"), ""), sptField,
                             FxDB(dr("subdivisi"), ""), sptField,
                             FxDB(dr("proyek"), ""), sptField,
                             FxDB(dr("catatan"), ""), sptField,
                             FxDB(dr("urutan"), 0), sptField,
                             FxDB(cJmlProd, 0), sptField,
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
                             FxDB(dr("jmlso"), 0), sptField,
                             FxDB(dr("jmlallowance"), 0), sptField,
                             FxDB(dr("prosentaseallowance"), 0), sptField,
                             FxDB(dr("jmltotal"), 0), sptRow)

                        'UPDATE SISA JAM
                        vSisaJam = vSisaJam - cJamPakai

                        'UPDATE DATATABLE SO
                        'idso, idbarang
                        If AsDataTableUpdateData(dt, "idso = '" & dr("idso") & "' AND idbarang = '" & dr("idbarang") & "'", "jmlsisa", Math.Round(cJmlBarang - cJmlProd, 5)) = False Then
                            result(2) = dr("nomorso") & " : " & dr("bkode") & " - update SO Backorder Production datatable failed." : GoTo selesai
                        End If

                        GoTo setPlan

                    Next

                End If

            Else

                For Each dr As DataRow In dt.Rows

                    search = String.Concat(search,
                         FxDB(dr("bakelas"), ""), sptField,
                         FxDB(dr("kelasnama"), ""), sptField,
                         FxDB(dr("bsubkelas"), ""), sptField,
                         FxDB(dr("subkelasnama"), ""), sptField,
                         FxDB(dr("maxso"), 0), sptField,
                         FxDB(dr("kpj"), 0), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
                         FxDB(dr("bkode"), ""), sptField,
                         FxDB(dr("nomorso"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("tglso"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(dr("tglkirimso"), ""), formatTgl), sptField,
                         FxDB(dr("namabarang"), ""), sptField,
                         FxDB(dr("tipebarang"), ""), sptField,
                         FxDB(dr("jml"), 0), sptField,
                         FxDB(dr("jmljam"), 0), sptField,
                         FxDB(dr("satuan"), ""), sptField,
                         FxDB(dr("nilaisatuan"), 0), sptField,
                         FxDB(dr("jmlbarang"), 0), sptField,
                         FxDB(dr("jmlsisa"), 0), sptField,
                         FxDB(dr("satuanbarang"), ""), sptField,
                         FxDB(dr("matauang"), ""), sptField,
                         FxDB(dr("kurs"), 0), sptField,
                         FxDB(dr("harga"), 0), sptField,
                         FxDB(dr("diskon"), ""), sptField,
                         FxDB(dr("jmldiskon"), 0), sptField,
                         FxDB(dr("pajak1"), ""), sptField,
                         FxDB(dr("jmlpajak1"), 0), sptField,
                         FxDB(dr("pajak2"), ""), sptField,
                         FxDB(dr("jmlpajak2"), 0), sptField,
                         FxDB(dr("cabang"), ""), sptField,
                         FxDB(dr("lokasi"), ""), sptField,
                         FxDB(dr("gudang"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("urutan"), 0), sptField,
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
                         FxDB(dr("jmlso"), 0), sptField,
                         FxDB(dr("jmlallowance"), 0), sptField,
                         FxDB(dr("prosentaseallowance"), 0), sptField,
                         FxDB(dr("jmltotal"), 0), sptRow)
                Next

            End If

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bakelas, kelasnama, bsubkelas, subkelasnama, maxso, kpj, idso, idsodetail, idbarang, bkode, nomorso, tglso, tglkirimso, namabarang, tipebarang, jml, jmljam, satuan, nilaisatuan, jmlbarang, jmlsisa, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, jmlso, jmlallowance, prosentaseallowance, jmltotal"))

        Return wsResult
    End Function

End Class