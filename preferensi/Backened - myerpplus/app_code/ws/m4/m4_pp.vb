Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pp
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_PpSimpan(ByVal param As String) As String
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
        'ppid(0) As Integer, ppcabang(1) As String, pplokasi(2) As String, ppjenis(3) As Integer, ppsumber(4) As String, 
        'ppautonotransaksi(5) As Integer, ppnotransaksi(6) As String, pptgl(7) As Date, ppkodepa(8) As Integer, ppkontak(9) As Integer, 
        'ppkontakperson(10) As String, pp1alamat1(11) As String, pp1alamat2(12) As String, pp1alamat3(13) As String, pp2alamat1(14) As String, 
        'pp2alamat2(15) As String, pp2alamat3(16) As String, ppbagianpembayaran(17) As Integer, pptermin(18) As String, pptgljatuhtempo(19) As Date, 
        'ppidri(20) As Integer, ppnorek(21) As String, ppuraian(22) As String, ppcatatan(23) As String, ppnoref(24) As String, 
        'pptglnoref(25) As Date, ppmatauang(26) As String, ppkurs(27) As Double, ppjumlah(28) As Double, ppjumlahvalas(29) As Double, 
        'ppjumlahbayar(30) As Double, ppjumlahbayarvalas(31) As Double, ppstatusbayar(32) As Integer, pptgllunas(33) As Date, ppcostcenter(34) As String, 
        'ppdivisi(35) As String, ppsubdivisi(36) As String, ppproyek(37) As String, ppstatus(38) As Integer, ppstatussebelumnya(39) As Integer, 
        'ppjmlrevisi(40) As Integer, ppcetakanke(41) As Integer, ppinputuser(42) As Integer, ppinputtgl(43) As DateTime, ppmodifikasiuser(44) As Integer, 
        'ppmodifikasitgl(45) As DateTime, ppposting(46) As Integer, ppisclose(47) As Integer, ppcustomtext1(48) As String, ppcustomtext2(49) As String, 
        'ppcustomtext3(50) As String, ppcustomtext4(51) As String, ppcustomtext5(52) As String, ppcustomint1(53) As Integer, ppcustomint2(54) As Integer, 
        'ppcustomint3(55) As Integer, ppcustomdbl1(56) As Double, ppcustomdbl2(57) As Double, ppcustomdbl3(58) As Double, ppcustomdate1(59) As Date, 
        'ppcustomdate2(60) As Date, ppcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ppid, ppcabang, pplokasi, ppjenis, ppsumber, ppautonotransaksi, ppnotransaksi, 
        'pptgl, ppkodepa, ppkontak, ppkontakperson, pp1alamat1, pp1alamat2, pp1alamat3, 
        'pp2alamat1, pp2alamat2, pp2alamat3, ppbagianpembayaran, pptermin, pptgljatuhtempo, ppidri, 
        'ppnorek, ppuraian, ppcatatan, ppnoref, pptglnoref, ppmatauang, ppkurs, 
        'ppjumlah, ppjumlahvalas, ppjumlahbayar, ppjumlahbayarvalas, ppstatusbayar, pptgllunas, ppcostcenter, 
        'ppdivisi, ppsubdivisi, ppproyek, ppstatus, ppstatussebelumnya, ppjmlrevisi, ppcetakanke, 
        'ppinputuser, ppinputtgl, ppmodifikasiuser, ppmodifikasitgl, ppposting, ppisclose, ppcustomtext1, 
        'ppcustomtext2, ppcustomtext3, ppcustomtext4, ppcustomtext5, ppcustomint1, ppcustomint2, ppcustomint3, 
        'ppcustomdbl1, ppcustomdbl2, ppcustomdbl3, ppcustomdate1, ppcustomdate2, ppcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ppid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ppid required numeric." : GoTo selesai
        End If
        'ppjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "ppjenis required numeric." : GoTo selesai
        End If
        'ppautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "ppautonotransaksi required numeric." : GoTo selesai
        End If
        'pptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "pptgl required date." : GoTo selesai
        End If
        'ppkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ppkodepa required numeric." : GoTo selesai
        End If
        'ppkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "ppkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "ppkontak can't be empty." : GoTo selesai
        End If
        'ppbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "ppbagianpembayaran required numeric." : GoTo selesai
        End If
        'pptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "pptgljatuhtempo required date." : GoTo selesai
        End If
        'ppidri(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ppidri required numeric." : GoTo selesai
        End If
        'pptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pptglnoref required date." : GoTo selesai
        End If
        'ppkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ppkurs required numeric." : GoTo selesai
        End If
        'ppjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ppjumlah required numeric." : GoTo selesai
        End If
        'ppjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ppjumlahvalas required numeric." : GoTo selesai
        End If
        'ppjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "ppjumlahbayar required numeric." : GoTo selesai
        End If
        'ppjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "ppjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'ppstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "ppstatusbayar required numeric." : GoTo selesai
        End If
        'pptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "pptgllunas required date." : GoTo selesai
        End If
        'ppstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ppstatus required numeric." : GoTo selesai
        End If
        'ppstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ppstatussebelumnya required numeric." : GoTo selesai
        End If
        'ppjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ppjmlrevisi required numeric." : GoTo selesai
        End If
        'ppcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "ppcetakanke required numeric." : GoTo selesai
        End If
        'ppinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ppinputuser required numeric." : GoTo selesai
        End If
        'ppinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ppinputtgl required date." : GoTo selesai
        End If
        'ppmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "ppmodifikasiuser required numeric." : GoTo selesai
        End If
        'ppmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "ppmodifikasitgl required date." : GoTo selesai
        End If
        'ppposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "ppposting required numeric." : GoTo selesai
        End If
        'ppisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "ppisclose required numeric." : GoTo selesai
        End If
        'ppcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "ppcustomint1 required numeric." : GoTo selesai
        End If
        'ppcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "ppcustomint2 required numeric." : GoTo selesai
        End If
        'ppcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "ppcustomint3 required numeric." : GoTo selesai
        End If
        'ppcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "ppcustomdbl1 required numeric." : GoTo selesai
        End If
        'ppcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "ppcustomdbl2 required numeric." : GoTo selesai
        End If
        'ppcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "ppcustomdbl3 required numeric." : GoTo selesai
        End If
        'ppcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "ppcustomdate1 required date." : GoTo selesai
        End If
        'ppcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "ppcustomdate2 required date." : GoTo selesai
        End If
        'ppcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "ppcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ppcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ppcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ppcabang should not be more than 25 character." : GoTo selesai
        End If

        'pplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pplokasi should not be more than 25 character." : GoTo selesai
        End If

        'ppsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "ppsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "ppsumber should not be more than 10 character." : GoTo selesai
        End If

        'ppnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ppnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ppnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pptgl can't be empty" : GoTo selesai
        End If

        'pptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'ppnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "ppnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "ppnorek should not be more than 25 character." : GoTo selesai
        End If

        'pptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pptglnoref can't be empty" : GoTo selesai
        End If

        'ppmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "ppmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "ppmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ppkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "ppkurs can't be empty" : GoTo selesai
        End If

        'ppjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ppjumlah can't be empty" : GoTo selesai
        End If

        'ppjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ppjumlahvalas can't be empty" : GoTo selesai
        End If

        'ppjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ppjumlahbayar can't be empty" : GoTo selesai
        End If

        'ppjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "ppjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'pptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "pptgllunas can't be empty" : GoTo selesai
        End If

        'ppinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ppinputtgl can't be empty" : GoTo selesai
        End If

        'ppmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "ppmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ppcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "ppcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ppcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "ppcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ppcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "ppcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ppcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "ppcustomdate1 can't be empty" : GoTo selesai
        End If

        'ppcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ppcustomdate2 can't be empty" : GoTo selesai
        End If

        'ppcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "ppcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ppid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppidri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ppjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ppjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "ppid~ppcabang~pplokasi~ppjenis~ppsumber~ppautonotransaksi~ppnotransaksi~pptgl~ppkodepa~ppkontak~ppkontakperson~pp1alamat1~pp1alamat2~pp1alamat3~pp2alamat1~pp2alamat2~pp2alamat3~ppbagianpembayaran~pptermin~pptgljatuhtempo~ppidri~ppnorek~ppuraian~ppcatatan~ppnoref~pptglnoref~ppmatauang~ppkurs~ppjumlah~ppjumlahvalas~ppjumlahbayar~ppjumlahbayarvalas~ppstatusbayar~pptgllunas~ppcostcenter~ppdivisi~ppsubdivisi~ppproyek~ppstatus~ppstatussebelumnya~ppjmlrevisi~ppcetakanke~ppinputuser~ppinputtgl~ppmodifikasiuser~ppmodifikasitgl~ppposting~ppisclose~ppcustomtext1~ppcustomtext2~ppcustomtext3~ppcustomtext4~ppcustomtext5~ppcustomint1~ppcustomint2~ppcustomint3~ppcustomdbl1~ppcustomdbl2~ppcustomdbl3~ppcustomdate1~ppcustomdate2~ppcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idppcarabayar(0) As Integer, idpp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idppcarabayar, idpp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idppcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idppcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idppcarabayar required numeric." : GoTo selesai
            End If
            'idpp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpp required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'jumlah(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(5) <= 0 Then
            '    result(2) = "Row : " & i & " - jumlah must be more than zero" : GoTo selesai
            'End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljt(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgljt can't be empty" : GoTo selesai
            End If

            'rekbank(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - rekbank can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
            End If

            'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
            If dataRowDetail(2) = 2 Then
                'nogiro(7) As String
                If Len(dataRowDetail(7)) = 0 Then
                    result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(7)) > 25 Then
                    result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowDetail(9)) = 0 Then
                    result(2) = "Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(9)) > 25 Then
                    result(2) = "Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowDetail(10)) = 0 Then
                    result(2) = "Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(10)) > 50 Then
                    result(2) = "Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowDetail(12)) = 0 Then
                    result(2) = "Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(12)) > 25 Then
                    result(2) = "Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idppcarabayar~idpp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
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


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 44
                Select Case drutama("ppstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pptgl")), AsFormatTanggal(drutama("pptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "ppmatauang", "ppnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pptermin").ToString, AsFormatTanggal(drutama("pptgl")), "pptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("ppjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("ppjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If drutama("ppjumlah") <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf drutama("ppjumlahvalas") <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================

                If isUpdate Then
                    result(4) = drutama("ppid")
                    notransaksi = drutama("ppnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ppid), ppnotransaksi FROM M4_pp WHERE ppid='" & result(4) & "' AND ppstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("ppautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ppcabang"), drutama("pplokasi"), drutama("ppsumber"), drutama("pptgl"))
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

                        End If

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ppid) FROM m4_pp WHERE ppnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Update M4_Pp set ppcabang  = '" & FixQuotes(drutama("ppcabang")) & "', pplokasi  = '" & FixQuotes(drutama("pplokasi")) & "', ppjenis  = " & drutama("ppjenis") & ", ppsumber  = '" & FixQuotes(drutama("ppsumber")) & "', ppautonotransaksi  = " & drutama("ppautonotransaksi") & ", ppnotransaksi  = '" & notransaksi & "', pptgl  = '" & FixQuotes(AsFormatTanggal(drutama("pptgl"))) & "', ppkodepa  = " & drutama("ppkodepa") & ", ppkontak  = " & drutama("ppkontak") & ", ppkontakperson  = '" & FixQuotes(drutama("ppkontakperson")) & "', pp1alamat1  = '" & FixQuotes(drutama("pp1alamat1")) & "', pp1alamat2  = '" & FixQuotes(drutama("pp1alamat2")) & "', pp1alamat3  = '" & FixQuotes(drutama("pp1alamat3")) & "', pp2alamat1  = '" & FixQuotes(drutama("pp2alamat1")) & "', pp2alamat2  = '" & FixQuotes(drutama("pp2alamat2")) & "', pp2alamat3  = '" & FixQuotes(drutama("pp2alamat3")) & "', ppbagianpembayaran  = " & drutama("ppbagianpembayaran") & ", pptermin  = '" & FixQuotes(drutama("pptermin")) & "', pptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("pptgljatuhtempo"))) & "', ppidri  = " & drutama("ppidri") & ", ppnorek  = '" & FixQuotes(drutama("ppnorek")) & "', ppuraian  = '" & FixQuotes(drutama("ppuraian")) & "', ppcatatan  = '" & FixQuotes(drutama("ppcatatan")) & "', ppnoref  = '" & FixQuotes(drutama("ppnoref")) & "', pptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pptglnoref"))) & "', ppmatauang  = '" & FixQuotes(drutama("ppmatauang")) & "', ppkurs  = '" & FixDouble(drutama("ppkurs")) & "', ppjumlah  = '" & FixDouble(drutama("ppjumlah")) & "', ppjumlahvalas  = '" & FixDouble(drutama("ppjumlahvalas")) & "', ppjumlahbayar  = '" & FixDouble(drutama("ppjumlahbayar")) & "', ppjumlahbayarvalas  = '" & FixDouble(drutama("ppjumlahbayarvalas")) & "', ppstatusbayar  = " & drutama("ppstatusbayar") & ", pptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("pptgllunas"))) & "', ppcostcenter  = '" & FixQuotes(drutama("ppcostcenter")) & "', ppdivisi  = '" & FixQuotes(drutama("ppdivisi")) & "', ppsubdivisi  = '" & FixQuotes(drutama("ppsubdivisi")) & "', ppproyek  = '" & FixQuotes(drutama("ppproyek")) & "', ppstatus  = " & drutama("ppstatus") & ", ppstatussebelumnya  = " & drutama("ppstatussebelumnya") & ", ppjmlrevisi  = ppjmlrevisi+1, ppcetakanke  = " & drutama("ppcetakanke") & ", ppmodifikasiuser  = " & drutama("ppmodifikasiuser") & ", ppmodifikasitgl  = NOW(), ppposting  = 0, ppcustomtext1  = '" & FixQuotes(drutama("ppcustomtext1")) & "', ppcustomtext2  = '" & FixQuotes(drutama("ppcustomtext2")) & "', ppcustomtext3  = '" & FixQuotes(drutama("ppcustomtext3")) & "', ppcustomtext4  = '" & FixQuotes(drutama("ppcustomtext4")) & "', ppcustomtext5  = '" & FixQuotes(drutama("ppcustomtext5")) & "', ppcustomint1  = " & drutama("ppcustomint1") & ", ppcustomint2  = " & drutama("ppcustomint2") & ", ppcustomint3  = " & drutama("ppcustomint3") & ", ppcustomdbl1  = '" & FixDouble(drutama("ppcustomdbl1")) & "', ppcustomdbl2  = '" & FixDouble(drutama("ppcustomdbl2")) & "', ppcustomdbl3  = '" & FixDouble(drutama("ppcustomdbl3")) & "', ppcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ppcustomdate1"))) & "', ppcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ppcustomdate2"))) & "', ppcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ppcustomdate3"))) & "' where ppid = '" & drutama("ppid") & "'"
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

                    If drutama("ppautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ppcabang"), drutama("pplokasi"), drutama("ppsumber"), drutama("pptgl"))
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
                        notransaksi = drutama("ppnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ppid) FROM m4_pp WHERE ppnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pp (ppcabang, pplokasi, ppjenis, ppsumber, ppautonotransaksi, ppnotransaksi, pptgl, ppkodepa, ppkontak, ppkontakperson, pp1alamat1, pp1alamat2, pp1alamat3, pp2alamat1, pp2alamat2, pp2alamat3, ppbagianpembayaran, pptermin, pptgljatuhtempo, ppidri, ppnorek, ppuraian, ppcatatan, ppnoref, pptglnoref, ppmatauang, ppkurs, ppjumlah, ppjumlahvalas, ppjumlahbayar, ppjumlahbayarvalas, ppstatusbayar, pptgllunas, ppcostcenter, ppdivisi, ppsubdivisi, ppproyek, ppstatus, ppstatussebelumnya, ppjmlrevisi, ppcetakanke, ppinputuser, ppinputtgl, ppmodifikasiuser, ppmodifikasitgl, ppposting, ppisclose, ppcustomtext1, ppcustomtext2, ppcustomtext3, ppcustomtext4, ppcustomtext5, ppcustomint1, ppcustomint2, ppcustomint3, ppcustomdbl1, ppcustomdbl2, ppcustomdbl3, ppcustomdate1, ppcustomdate2, ppcustomdate3) values('" & FixQuotes(drutama("ppcabang")) & "', '" & FixQuotes(drutama("pplokasi")) & "', " & drutama("ppjenis") & ", '" & FixQuotes(drutama("ppsumber")) & "', " & drutama("ppautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("pptgl"))) & "', " & drutama("ppkodepa") & ", " & drutama("ppkontak") & ", '" & FixQuotes(drutama("ppkontakperson")) & "', '" & FixQuotes(drutama("pp1alamat1")) & "', '" & FixQuotes(drutama("pp1alamat2")) & "', '" & FixQuotes(drutama("pp1alamat3")) & "', '" & FixQuotes(drutama("pp2alamat1")) & "', '" & FixQuotes(drutama("pp2alamat2")) & "', '" & FixQuotes(drutama("pp2alamat3")) & "', " & drutama("ppbagianpembayaran") & ", '" & FixQuotes(drutama("pptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pptgljatuhtempo"))) & "', " & drutama("ppidri") & ", '" & FixQuotes(drutama("ppnorek")) & "', '" & FixQuotes(drutama("ppuraian")) & "', '" & FixQuotes(drutama("ppcatatan")) & "', '" & FixQuotes(drutama("ppnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pptglnoref"))) & "', '" & FixQuotes(drutama("ppmatauang")) & "', '" & FixDouble(drutama("ppkurs")) & "', '" & FixDouble(drutama("ppjumlah")) & "', '" & FixDouble(drutama("ppjumlahvalas")) & "', '" & FixDouble(drutama("ppjumlahbayar")) & "', '" & FixDouble(drutama("ppjumlahbayarvalas")) & "', " & drutama("ppstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("pptgllunas"))) & "', '" & FixQuotes(drutama("ppcostcenter")) & "', '" & FixQuotes(drutama("ppdivisi")) & "', '" & FixQuotes(drutama("ppsubdivisi")) & "', '" & FixQuotes(drutama("ppproyek")) & "', " & drutama("ppstatus") & ", " & drutama("ppstatussebelumnya") & ", " & drutama("ppjmlrevisi") & ", " & drutama("ppcetakanke") & ", " & drutama("ppinputuser") & ", NOW(), " & drutama("ppmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ppisclose") & ", '" & FixQuotes(drutama("ppcustomtext1")) & "', '" & FixQuotes(drutama("ppcustomtext2")) & "', '" & FixQuotes(drutama("ppcustomtext3")) & "', '" & FixQuotes(drutama("ppcustomtext4")) & "', '" & FixQuotes(drutama("ppcustomtext5")) & "', " & drutama("ppcustomint1") & ", " & drutama("ppcustomint2") & ", " & drutama("ppcustomint3") & ", '" & FixDouble(drutama("ppcustomdbl1")) & "', '" & FixDouble(drutama("ppcustomdbl2")) & "', '" & FixDouble(drutama("ppcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select ppid from M4_pp where ppnotransaksi='" & notransaksi & "' AND ppinputuser= '" & userid & "' order by ppmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pp_Pay where idpp = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idppcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("ppsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("ppkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M4_Pp_Pay(idppcarabayar, idpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("ppstatus") = 2 And Len(strGiro.ToString) > 0 Then
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

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ppstatus") = 2 Then
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
    Public Function M4_PpUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("ppkontakkode", "c1.kkode")
            Filter = Filter.Replace("ppkontaknama", "c1.knama")
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
            Dim sumber As String = "Pp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pptgl, Ppnotransaksi, Ppstatus FROM m4_Pp WHERE Ppid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ppstatussebelumnya" : jnsaktivitas = 17
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

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m4_pp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'CEK STATUS GIRO
                dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'PP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'PP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE m4_Pp SET Ppstatus = " & nilaiStatus & ", Ppmodifikasiuser='" & userid & "', Ppmodifikasitgl = NOW(), Ppposting = 0, Pppostingtgl = '1971-01-01 00:00:00', Ppjmlrevisi = Ppjmlrevisi + 1 WHERE Ppid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PpSearch(PostWsSearch(paramSplit(0), "M4_PpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PpDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("ppkontakkode", "c1.kkode")
            Filter = Filter.Replace("ppkontaknama", "c1.knama")
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
            Dim sumber As String = "Pp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Ppid, Ppnotransaksi FROM M4_Pp WHERE Ppid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ppcabang, pplokasi, ppsumber, ppautonotransaksi, ppnotransaksi, pptgl"
            sql &= " FROM M4_pp"
            sql &= " WHERE ppid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ppcabang")
                lokasi = dtNomorNext.Rows(0)("pplokasi")
                sumber = dtNomorNext.Rows(0)("ppsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ppautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ppnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Pp_Pay WHERE idpp = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Pp WHERE ppid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PpSearch(PostWsSearch(paramSplit(0), "M4_PpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PpGetdataById(ByVal param As String) As String
        'M4_PpGetdataById Utama --------------------------------------------------------
        'ppid, ppcabang, pplokasi, ppjenis, ppsumber, ppautonotransaksi, ppnotransaksi, 
        'pptgl, ppkodepa, ppkontak, ppkontakperson, pp1alamat1, pp1alamat2, pp1alamat3, 
        'pp2alamat1, pp2alamat2, pp2alamat3, ppbagianpembayaran, pptermin, pptgljatuhtempo, ppidri, 
        'ppnorek, ppuraian, ppcatatan, ppnoref, pptglnoref, ppmatauang, ppkurs, 
        'ppjumlah, ppjumlahvalas, ppjumlahbayar, ppjumlahbayarvalas, ppstatusbayar, pptgllunas, ppcostcenter, 
        'ppdivisi, ppsubdivisi, ppproyek, ppstatus, ppstatussebelumnya, ppjmlrevisi, ppcetakanke, 
        'ppinputuser, ppinputtgl, ppmodifikasiuser, ppmodifikasitgl, ppposting, pppostingtgl, ppisclose, 
        'ppcustomtext1, ppcustomtext2, ppcustomtext3, ppcustomtext4, ppcustomtext5, ppcustomint1, ppcustomint2, 
        'ppcustomint3, ppcustomdbl1, ppcustomdbl2, ppcustomdbl3, ppcustomdate1, ppcustomdate2, ppcustomdate3, 
        'ppcabangnama, pplokasinama, ppkontakkode, ppkontaknama, ppbagianpembayarankode, ppbagianpembayarannama, ppterminnama, 
        'ppterminharijatuhtempo, rinotransaksi, ppnoreknama, ppcostcenternama, ppdivisinama, ppsubdivisinama, ppproyeknama, 
        'ppstatusnama, ppstatussebelumnyanama, ppinputusernama, ppmodifikasiusernama, kpkp

        'M4_PpGetdataById Pay -------------------------------------------------------
        'idppcarabayar, idpp, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M4_Pp~M4_Pp_Pay-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ppid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ppid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pp_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ppid"), 0), sptField,
                     FxDB(drutama("ppcabang"), ""), sptField,
                     FxDB(drutama("pplokasi"), ""), sptField,
                     FxDB(drutama("ppjenis"), 0), sptField,
                     FxDB(drutama("ppsumber"), ""), sptField,
                     FxDB(drutama("ppautonotransaksi"), 0), sptField,
                     FxDB(drutama("ppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ppkodepa"), 0), sptField,
                     FxDB(drutama("ppkontak"), 0), sptField,
                     FxDB(drutama("ppkontakperson"), ""), sptField,
                     FxDB(drutama("pp1alamat1"), ""), sptField,
                     FxDB(drutama("pp1alamat2"), ""), sptField,
                     FxDB(drutama("pp1alamat3"), ""), sptField,
                     FxDB(drutama("pp2alamat1"), ""), sptField,
                     FxDB(drutama("pp2alamat2"), ""), sptField,
                     FxDB(drutama("pp2alamat3"), ""), sptField,
                     FxDB(drutama("ppbagianpembayaran"), 0), sptField,
                     FxDB(drutama("pptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("ppidri"), 0), sptField,
                     FxDB(drutama("ppnorek"), ""), sptField,
                     FxDB(drutama("ppuraian"), ""), sptField,
                     FxDB(drutama("ppcatatan"), ""), sptField,
                     FxDB(drutama("ppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("ppmatauang"), ""), sptField,
                     FxDB(drutama("ppkurs"), 0), sptField,
                     FxDB(drutama("ppjumlah"), 0), sptField,
                     FxDB(drutama("ppjumlahvalas"), 0), sptField,
                     FxDB(drutama("ppjumlahbayar"), 0), sptField,
                     FxDB(drutama("ppjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("ppstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pptgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("ppcostcenter"), ""), sptField,
                     FxDB(drutama("ppdivisi"), ""), sptField,
                     FxDB(drutama("ppsubdivisi"), ""), sptField,
                     FxDB(drutama("ppproyek"), ""), sptField,
                     FxDB(drutama("ppstatus"), 0), sptField,
                     FxDB(drutama("ppstatussebelumnya"), 0), sptField,
                     FxDB(drutama("ppjmlrevisi"), 0), sptField,
                     FxDB(drutama("ppcetakanke"), 0), sptField,
                     FxDB(drutama("ppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppisclose"), 0), sptField,
                     FxDB(drutama("ppcustomtext1"), ""), sptField,
                     FxDB(drutama("ppcustomtext2"), ""), sptField,
                     FxDB(drutama("ppcustomtext3"), ""), sptField,
                     FxDB(drutama("ppcustomtext4"), ""), sptField,
                     FxDB(drutama("ppcustomtext5"), ""), sptField,
                     FxDB(drutama("ppcustomint1"), 0), sptField,
                     FxDB(drutama("ppcustomint2"), 0), sptField,
                     FxDB(drutama("ppcustomint3"), 0), sptField,
                     FxDB(drutama("ppcustomdbl1"), 0), sptField,
                     FxDB(drutama("ppcustomdbl2"), 0), sptField,
                     FxDB(drutama("ppcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ppcabangnama"), ""), sptField,
                     FxDB(drutama("pplokasinama"), ""), sptField,
                     FxDB(drutama("ppkontakkode"), ""), sptField,
                     FxDB(drutama("ppkontaknama"), ""), sptField,
                     FxDB(drutama("ppbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("ppbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("ppterminnama"), ""), sptField,
                     FxDB(drutama("ppterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rinotransaksi"), ""), sptField,
                     FxDB(drutama("ppnoreknama"), ""), sptField,
                     FxDB(drutama("ppcostcenternama"), ""), sptField,
                     FxDB(drutama("ppdivisinama"), ""), sptField,
                     FxDB(drutama("ppsubdivisinama"), ""), sptField,
                     FxDB(drutama("ppproyeknama"), ""), sptField,
                     FxDB(drutama("ppstatusnama"), ""), sptField,
                     FxDB(drutama("ppstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("ppinputusernama"), ""), sptField,
                     FxDB(drutama("ppmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idppcarabayar"), 0), sptField,
                     FxDB(dr("idpp"), 0), sptField,
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
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = " transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppid, ppcabang, pplokasi, ppjenis, ppsumber, ppautonotransaksi, ppnotransaksi, pptgl, ppkodepa, ppkontak, ppkontakperson, pp1alamat1, pp1alamat2, pp1alamat3, pp2alamat1, pp2alamat2, pp2alamat3, ppbagianpembayaran, pptermin, pptgljatuhtempo, ppidri, ppnorek, ppuraian, ppcatatan, ppnoref, pptglnoref, ppmatauang, ppkurs, ppjumlah, ppjumlahvalas, ppjumlahbayar, ppjumlahbayarvalas, ppstatusbayar, pptgllunas, ppcostcenter, ppdivisi, ppsubdivisi, ppproyek, ppstatus, ppstatussebelumnya, ppjmlrevisi, ppcetakanke, ppinputuser, ppinputtgl, ppmodifikasiuser, ppmodifikasitgl, ppposting, pppostingtgl, ppisclose, ppcustomtext1, ppcustomtext2, ppcustomtext3, ppcustomtext4, ppcustomtext5, ppcustomint1, ppcustomint2, ppcustomint3, ppcustomdbl1, ppcustomdbl2, ppcustomdbl3, ppcustomdate1, ppcustomdate2, ppcustomdate3, ppcabangnama, pplokasinama, ppkontakkode, ppkontaknama, ppbagianpembayarankode, ppbagianpembayarannama, ppterminnama, ppterminharijatuhtempo, rinotransaksi, ppnoreknama, ppcostcenternama, ppdivisinama, ppsubdivisinama, ppproyeknama, ppstatusnama, ppstatussebelumnyanama, ppinputusernama, ppmodifikasiusernama, kpkp"), sptSubParam, ReplaceMapping("idppcarabayar, idpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PpSearch(ByVal param As String) As String
        'M4_PpSearch --------------------------------------------------------
        'ppid, ppcabang, pplokasi, ppjenis, ppsumber, ppautonotransaksi, ppnotransaksi, 
        'pptgl, ppkodepa, ppkontak, ppkontakperson, pp1alamat1, pp1alamat2, pp1alamat3, 
        'pp2alamat1, pp2alamat2, pp2alamat3, ppbagianpembayaran, pptermin, pptgljatuhtempo, ppidri, 
        'ppnorek, ppuraian, ppcatatan, ppnoref, pptglnoref, ppmatauang, ppkurs, 
        'ppjumlah, ppjumlahvalas, ppjumlahbayar, ppjumlahbayarvalas, ppstatusbayar, pptgllunas, ppcostcenter, 
        'ppdivisi, ppsubdivisi, ppproyek, ppstatus, ppstatussebelumnya, ppjmlrevisi, ppcetakanke, 
        'ppinputuser, ppinputtgl, ppmodifikasiuser, ppmodifikasitgl, ppposting, pppostingtgl, ppisclose, 
        'ppcabangnama, pplokasinama, ppjenisnama, ppkontakkode, ppkontaknama, ppbagianpembayarankode, ppbagianpembayarannama, 
        'rinotransaksi, ppnoreknama, ppstatusnama, ppstatussebelumnyanama, ppinputusernama, ppmodifikasiusernama

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
            Filter = Filter.Replace("ppkontakkode", "c1.kkode")
            Filter = Filter.Replace("ppkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pp_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ppid"), 0), sptField,
                     FxDB(dr("ppcabang"), ""), sptField,
                     FxDB(dr("pplokasi"), ""), sptField,
                     FxDB(dr("ppjenis"), 0), sptField,
                     FxDB(dr("ppsumber"), ""), sptField,
                     FxDB(dr("ppautonotransaksi"), 0), sptField,
                     FxDB(dr("ppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pptgl"), ""), formatTgl), sptField,
                     FxDB(dr("ppkodepa"), 0), sptField,
                     FxDB(dr("ppkontak"), 0), sptField,
                     FxDB(dr("ppkontakperson"), ""), sptField,
                     FxDB(dr("pp1alamat1"), ""), sptField,
                     FxDB(dr("pp1alamat2"), ""), sptField,
                     FxDB(dr("pp1alamat3"), ""), sptField,
                     FxDB(dr("pp2alamat1"), ""), sptField,
                     FxDB(dr("pp2alamat2"), ""), sptField,
                     FxDB(dr("pp2alamat3"), ""), sptField,
                     FxDB(dr("ppbagianpembayaran"), 0), sptField,
                     FxDB(dr("pptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("ppidri"), 0), sptField,
                     FxDB(dr("ppnorek"), ""), sptField,
                     FxDB(dr("ppuraian"), ""), sptField,
                     FxDB(dr("ppcatatan"), ""), sptField,
                     FxDB(dr("ppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("ppmatauang"), ""), sptField,
                     FxDB(dr("ppkurs"), 0), sptField,
                     FxDB(dr("ppjumlah"), 0), sptField,
                     FxDB(dr("ppjumlahvalas"), 0), sptField,
                     FxDB(dr("ppjumlahbayar"), 0), sptField,
                     FxDB(dr("ppjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("ppstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pptgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("ppcostcenter"), ""), sptField,
                     FxDB(dr("ppdivisi"), ""), sptField,
                     FxDB(dr("ppsubdivisi"), ""), sptField,
                     FxDB(dr("ppproyek"), ""), sptField,
                     FxDB(dr("ppstatus"), 0), sptField,
                     FxDB(dr("ppstatussebelumnya"), 0), sptField,
                     FxDB(dr("ppjmlrevisi"), 0), sptField,
                     FxDB(dr("ppcetakanke"), 0), sptField,
                     FxDB(dr("ppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppisclose"), 0), sptField,
                     FxDB(dr("ppcabangnama"), ""), sptField,
                     FxDB(dr("pplokasinama"), ""), sptField,
                     FxDB(dr("ppjenisnama"), ""), sptField,
                     FxDB(dr("ppkontakkode"), ""), sptField,
                     FxDB(dr("ppkontaknama"), ""), sptField,
                     FxDB(dr("ppbagianpembayarankode"), ""), sptField,
                     FxDB(dr("ppbagianpembayarannama"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("ppnoreknama"), ""), sptField,
                     FxDB(dr("ppstatusnama"), ""), sptField,
                     FxDB(dr("ppstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ppinputusernama"), ""), sptField,
                     FxDB(dr("ppmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppid, ppcabang, pplokasi, ppjenis, ppsumber, ppautonotransaksi, ppnotransaksi, pptgl, ppkodepa, ppkontak, ppkontakperson, pp1alamat1, pp1alamat2, pp1alamat3, pp2alamat1, pp2alamat2, pp2alamat3, ppbagianpembayaran, pptermin, pptgljatuhtempo, ppidri, ppnorek, ppuraian, ppcatatan, ppnoref, pptglnoref, ppmatauang, ppkurs, ppjumlah, ppjumlahvalas, ppjumlahbayar, ppjumlahbayarvalas, ppstatusbayar, pptgllunas, ppcostcenter, ppdivisi, ppsubdivisi, ppproyek, ppstatus, ppstatussebelumnya, ppjmlrevisi, ppcetakanke, ppinputuser, ppinputtgl, ppmodifikasiuser, ppmodifikasitgl, ppposting, pppostingtgl, ppisclose, ppcabangnama, pplokasinama, ppjenisnama, ppkontakkode, ppkontaknama, ppbagianpembayarankode, ppbagianpembayarannama, rinotransaksi, ppnoreknama, ppstatusnama, ppstatussebelumnyanama, ppinputusernama, ppmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PpTerkait(ByVal param As String) As String
        'M4_PpTerkait --------------------------------------------------------
        'ppid, ppnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "ppid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_pp_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ppid"), 0), sptField,
                     FxDB(dr("ppnotransaksi"), ""), sptField,
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
            result(2) = "Related PP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppid, ppnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

End Class