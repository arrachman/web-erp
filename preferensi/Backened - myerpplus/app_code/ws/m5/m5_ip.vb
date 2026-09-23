Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_ip
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_IpSimpan(ByVal param As String) As String
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
        'ipid(0) As Integer, ipcabang(1) As String, iplokasi(2) As String, ipjenis(3) As Integer, ipsumber(4) As String, 
        'ipautonotransaksi(5) As Integer, ipnotransaksi(6) As String, iptgl(7) As Date, ipkodepa(8) As Integer, ipkontak(9) As Integer, 
        'ipkontakperson(10) As String, ip1alamat1(11) As String, ip1alamat2(12) As String, ip1alamat3(13) As String, ip2alamat1(14) As String, 
        'ip2alamat2(15) As String, ip2alamat3(16) As String, ipbagianterima(17) As Integer, iptermin(18) As String, iptgljatuhtempo(19) As Date, 
        'ipidso(20) As Integer, ipnorek(21) As String, ipuraian(22) As String, ipcatatan(23) As String, ipnoref(24) As String, 
        'iptglnoref(25) As Date, ipmatauang(26) As String, ipkurs(27) As Double, ipjumlah(28) As Double, ipjumlahvalas(29) As Double, 
        'ipjumlahbayar(30) As Double, ipjumlahbayarvalas(31) As Double, ipstatusbayar(32) As Integer, iptgllunas(33) As Date, ipcostcenter(34) As String, 
        'ipdivisi(35) As String, ipsubdivisi(36) As String, ipproyek(37) As String, ipstatus(38) As Integer, ipstatussebelumnya(39) As Integer, 
        'ipjmlrevisi(40) As Integer, ipcetakanke(41) As Integer, ipinputuser(42) As Integer, ipinputtgl(43) As DateTime, ipmodifikasiuser(44) As Integer, 
        'ipmodifikasitgl(45) As DateTime, ipposting(46) As Integer, ipisclose(47) As Integer, ipcustomtext1(48) As String, ipcustomtext2(49) As String, 
        'ipcustomtext3(50) As String, ipcustomtext4(51) As String, ipcustomtext5(52) As String, ipcustomint1(53) As Integer, ipcustomint2(54) As Integer, 
        'ipcustomint3(55) As Integer, ipcustomdbl1(56) As Double, ipcustomdbl2(57) As Double, ipcustomdbl3(58) As Double, ipcustomdate1(59) As Date, 
        'ipcustomdate2(60) As Date, ipcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, 
        'iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, 
        'ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, 
        'ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, 
        'ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, 
        'ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, 
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ipisclose, ipcustomtext1, 
        'ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, 
        'ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ipid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ipid required numeric." : GoTo selesai
        End If
        'ipjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "ipjenis required numeric." : GoTo selesai
        End If
        'ipautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "ipautonotransaksi required numeric." : GoTo selesai
        End If
        'iptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "iptgl required date." : GoTo selesai
        End If
        'ipkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ipkodepa required numeric." : GoTo selesai
        End If
        'ipkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "ipkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "ipkontak can't be empty." : GoTo selesai
        End If
        'ipbagianterima(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "ipbagianterima required numeric." : GoTo selesai
        End If
        'iptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "iptgljatuhtempo required date." : GoTo selesai
        End If
        'ipidso(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ipidso required numeric." : GoTo selesai
        End If
        'iptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "iptglnoref required date." : GoTo selesai
        End If
        'ipkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ipkurs required numeric." : GoTo selesai
        End If
        'ipjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ipjumlah required numeric." : GoTo selesai
        End If
        'ipjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ipjumlahvalas required numeric." : GoTo selesai
        End If
        'ipjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "ipjumlahbayar required numeric." : GoTo selesai
        End If
        'ipjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "ipjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'ipstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "ipstatusbayar required numeric." : GoTo selesai
        End If
        'iptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "iptgllunas required date." : GoTo selesai
        End If
        'ipstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ipstatus required numeric." : GoTo selesai
        End If
        'ipstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ipstatussebelumnya required numeric." : GoTo selesai
        End If
        'ipjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ipjmlrevisi required numeric." : GoTo selesai
        End If
        'ipcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "ipcetakanke required numeric." : GoTo selesai
        End If
        'ipinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ipinputuser required numeric." : GoTo selesai
        End If
        'ipinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ipinputtgl required date." : GoTo selesai
        End If
        'ipmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "ipmodifikasiuser required numeric." : GoTo selesai
        End If
        'ipmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "ipmodifikasitgl required date." : GoTo selesai
        End If
        'ipposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "ipposting required numeric." : GoTo selesai
        End If
        'ipisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "ipisclose required numeric." : GoTo selesai
        End If
        'ipcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "ipcustomint1 required numeric." : GoTo selesai
        End If
        'ipcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "ipcustomint2 required numeric." : GoTo selesai
        End If
        'ipcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "ipcustomint3 required numeric." : GoTo selesai
        End If
        'ipcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "ipcustomdbl1 required numeric." : GoTo selesai
        End If
        'ipcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "ipcustomdbl2 required numeric." : GoTo selesai
        End If
        'ipcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "ipcustomdbl3 required numeric." : GoTo selesai
        End If
        'ipcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "ipcustomdate1 required date." : GoTo selesai
        End If
        'ipcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "ipcustomdate2 required date." : GoTo selesai
        End If
        'ipcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "ipcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ipcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ipcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ipcabang should not be more than 25 character." : GoTo selesai
        End If

        'iplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "iplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "iplokasi should not be more than 25 character." : GoTo selesai
        End If

        'ipsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "ipsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "ipsumber should not be more than 10 character." : GoTo selesai
        End If

        'ipnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ipnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ipnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'iptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "iptgl can't be empty" : GoTo selesai
        End If

        'iptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "iptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'ipnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "ipnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "ipnorek should not be more than 25 character." : GoTo selesai
        End If

        'iptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "iptglnoref can't be empty" : GoTo selesai
        End If

        'ipmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "ipmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "ipmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ipkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "ipkurs can't be empty" : GoTo selesai
        End If

        'ipjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ipjumlah can't be empty" : GoTo selesai
        End If

        'ipjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ipjumlahvalas can't be empty" : GoTo selesai
        End If

        'ipjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ipjumlahbayar can't be empty" : GoTo selesai
        End If

        'ipjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "ipjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'iptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "iptgllunas can't be empty" : GoTo selesai
        End If

        'ipinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ipinputtgl can't be empty" : GoTo selesai
        End If

        'ipmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "ipmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ipcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "ipcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ipcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "ipcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ipcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "ipcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ipcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "ipcustomdate1 can't be empty" : GoTo selesai
        End If

        'ipcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ipcustomdate2 can't be empty" : GoTo selesai
        End If

        'ipcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "ipcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ipid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ipjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ipjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "ipid~ipcabang~iplokasi~ipjenis~ipsumber~ipautonotransaksi~ipnotransaksi~iptgl~ipkodepa~ipkontak~ipkontakperson~ip1alamat1~ip1alamat2~ip1alamat3~ip2alamat1~ip2alamat2~ip2alamat3~ipbagianterima~iptermin~iptgljatuhtempo~ipidso~ipnorek~ipuraian~ipcatatan~ipnoref~iptglnoref~ipmatauang~ipkurs~ipjumlah~ipjumlahvalas~ipjumlahbayar~ipjumlahbayarvalas~ipstatusbayar~iptgllunas~ipcostcenter~ipdivisi~ipsubdivisi~ipproyek~ipstatus~ipstatussebelumnya~ipjmlrevisi~ipcetakanke~ipinputuser~ipinputtgl~ipmodifikasiuser~ipmodifikasitgl~ipposting~ipisclose~ipcustomtext1~ipcustomtext2~ipcustomtext3~ipcustomtext4~ipcustomtext5~ipcustomint1~ipcustomint2~ipcustomint3~ipcustomdbl1~ipcustomdbl2~ipcustomdbl3~ipcustomdate1~ipcustomdate2~ipcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idipcarabayar(0) As Integer, idip(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idipcarabayar, idip, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idipcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idip", AsEnumTypeData.AsInt64)
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
            'idipcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idipcarabayar required numeric." : GoTo selesai
            End If
            'idip(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idip required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idipcarabayar~idip~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 44
                Select Case drutama("ipstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("iptgl")), AsFormatTanggal(drutama("iptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "ipmatauang", "ipnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("iptermin").ToString, AsFormatTanggal(drutama("iptgl")), "iptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("iptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("ipjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("ipjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("ipjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("ipjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================


                If isUpdate Then
                    result(4) = drutama("ipid")
                    notransaksi = drutama("ipnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(ipid), ipnotransaksi FROM M5_ip WHERE ipid='" & result(4) & "' AND ipstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("ipautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ipcabang"), drutama("iplokasi"), drutama("ipsumber"), drutama("iptgl"), drutama("ipsumber"), 5)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ipid) FROM M5_ip WHERE ipnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_ip_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Ip_HistorySimpan("" & paramSplit(0) & "★M5_Ip_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("ipsumber")) & "▼" & FixQuotes(drutama("ipid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Ip set ipcabang  = '" & FixQuotes(drutama("ipcabang")) & "', iplokasi  = '" & FixQuotes(drutama("iplokasi")) & "', ipjenis  = " & drutama("ipjenis") & ", ipsumber  = '" & FixQuotes(drutama("ipsumber")) & "', ipautonotransaksi  = " & drutama("ipautonotransaksi") & ", ipnotransaksi  = '" & notransaksi & "', iptgl  = '" & FixQuotes(AsFormatTanggal(drutama("iptgl"))) & "', ipkodepa  = " & drutama("ipkodepa") & ", ipkontak  = " & drutama("ipkontak") & ", ipkontakperson  = '" & FixQuotes(drutama("ipkontakperson")) & "', ip1alamat1  = '" & FixQuotes(drutama("ip1alamat1")) & "', ip1alamat2  = '" & FixQuotes(drutama("ip1alamat2")) & "', ip1alamat3  = '" & FixQuotes(drutama("ip1alamat3")) & "', ip2alamat1  = '" & FixQuotes(drutama("ip2alamat1")) & "', ip2alamat2  = '" & FixQuotes(drutama("ip2alamat2")) & "', ip2alamat3  = '" & FixQuotes(drutama("ip2alamat3")) & "', ipbagianterima  = " & drutama("ipbagianterima") & ", iptermin  = '" & FixQuotes(drutama("iptermin")) & "', iptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("iptgljatuhtempo"))) & "', ipidso  = " & drutama("ipidso") & ", ipnorek  = '" & FixQuotes(drutama("ipnorek")) & "', ipuraian  = '" & FixQuotes(drutama("ipuraian")) & "', ipcatatan  = '" & FixQuotes(drutama("ipcatatan")) & "', ipnoref  = '" & FixQuotes(drutama("ipnoref")) & "', iptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("iptglnoref"))) & "', ipmatauang  = '" & FixQuotes(drutama("ipmatauang")) & "', ipkurs  = '" & FixDouble(drutama("ipkurs")) & "', ipjumlah  = '" & FixDouble(drutama("ipjumlah")) & "', ipjumlahvalas  = '" & FixDouble(drutama("ipjumlahvalas")) & "', ipjumlahbayar  = '" & FixDouble(drutama("ipjumlahbayar")) & "', ipjumlahbayarvalas  = '" & FixDouble(drutama("ipjumlahbayarvalas")) & "', ipstatusbayar  = " & drutama("ipstatusbayar") & ", iptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("iptgllunas"))) & "', ipcostcenter  = '" & FixQuotes(drutama("ipcostcenter")) & "', ipdivisi  = '" & FixQuotes(drutama("ipdivisi")) & "', ipsubdivisi  = '" & FixQuotes(drutama("ipsubdivisi")) & "', ipproyek  = '" & FixQuotes(drutama("ipproyek")) & "', ipstatus  = " & drutama("ipstatus") & ", ipstatussebelumnya  = " & drutama("ipstatussebelumnya") & ", ipjmlrevisi  = ipjmlrevisi+1, ipcetakanke  = " & drutama("ipcetakanke") & ", ipmodifikasiuser  = " & drutama("ipmodifikasiuser") & ", ipmodifikasitgl  = NOW(), ipposting  = 0, ipcustomtext1  = '" & FixQuotes(drutama("ipcustomtext1")) & "', ipcustomtext2  = '" & FixQuotes(drutama("ipcustomtext2")) & "', ipcustomtext3  = '" & FixQuotes(drutama("ipcustomtext3")) & "', ipcustomtext4  = '" & FixQuotes(drutama("ipcustomtext4")) & "', ipcustomtext5  = '" & FixQuotes(drutama("ipcustomtext5")) & "', ipcustomint1  = " & drutama("ipcustomint1") & ", ipcustomint2  = " & drutama("ipcustomint2") & ", ipcustomint3  = " & drutama("ipcustomint3") & ", ipcustomdbl1  = '" & FixDouble(drutama("ipcustomdbl1")) & "', ipcustomdbl2  = '" & FixDouble(drutama("ipcustomdbl2")) & "', ipcustomdbl3  = '" & FixDouble(drutama("ipcustomdbl3")) & "', ipcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate1"))) & "', ipcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate2"))) & "', ipcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate3"))) & "' where ipid = '" & drutama("ipid") & "'"
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

                    If drutama("ipautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ipcabang"), drutama("iplokasi"), drutama("ipsumber"), drutama("iptgl"), drutama("ipsumber"), 5)
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
                        notransaksi = drutama("ipnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ipid) FROM M5_ip WHERE ipnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Ip (ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ipisclose, ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3) values('" & FixQuotes(drutama("ipcabang")) & "', '" & FixQuotes(drutama("iplokasi")) & "', " & drutama("ipjenis") & ", '" & FixQuotes(drutama("ipsumber")) & "', " & drutama("ipautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("iptgl"))) & "', " & drutama("ipkodepa") & ", " & drutama("ipkontak") & ", '" & FixQuotes(drutama("ipkontakperson")) & "', '" & FixQuotes(drutama("ip1alamat1")) & "', '" & FixQuotes(drutama("ip1alamat2")) & "', '" & FixQuotes(drutama("ip1alamat3")) & "', '" & FixQuotes(drutama("ip2alamat1")) & "', '" & FixQuotes(drutama("ip2alamat2")) & "', '" & FixQuotes(drutama("ip2alamat3")) & "', " & drutama("ipbagianterima") & ", '" & FixQuotes(drutama("iptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("iptgljatuhtempo"))) & "', " & drutama("ipidso") & ", '" & FixQuotes(drutama("ipnorek")) & "', '" & FixQuotes(drutama("ipuraian")) & "', '" & FixQuotes(drutama("ipcatatan")) & "', '" & FixQuotes(drutama("ipnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("iptglnoref"))) & "', '" & FixQuotes(drutama("ipmatauang")) & "', '" & FixDouble(drutama("ipkurs")) & "', '" & FixDouble(drutama("ipjumlah")) & "', '" & FixDouble(drutama("ipjumlahvalas")) & "', '" & FixDouble(drutama("ipjumlahbayar")) & "', '" & FixDouble(drutama("ipjumlahbayarvalas")) & "', " & drutama("ipstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("iptgllunas"))) & "', '" & FixQuotes(drutama("ipcostcenter")) & "', '" & FixQuotes(drutama("ipdivisi")) & "', '" & FixQuotes(drutama("ipsubdivisi")) & "', '" & FixQuotes(drutama("ipproyek")) & "', " & drutama("ipstatus") & ", " & drutama("ipstatussebelumnya") & ", " & drutama("ipjmlrevisi") & ", " & drutama("ipcetakanke") & ", " & drutama("ipinputuser") & ", NOW(), " & drutama("ipmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ipisclose") & ", '" & FixQuotes(drutama("ipcustomtext1")) & "', '" & FixQuotes(drutama("ipcustomtext2")) & "', '" & FixQuotes(drutama("ipcustomtext3")) & "', '" & FixQuotes(drutama("ipcustomtext4")) & "', '" & FixQuotes(drutama("ipcustomtext5")) & "', " & drutama("ipcustomint1") & ", " & drutama("ipcustomint2") & ", " & drutama("ipcustomint3") & ", '" & FixDouble(drutama("ipcustomdbl1")) & "', '" & FixDouble(drutama("ipcustomdbl2")) & "', '" & FixDouble(drutama("ipcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select ipid from M5_ip where ipnotransaksi='" & notransaksi & "' AND ipinputuser= '" & userid & "' order by ipmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Ip_Pay where idip = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idipcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("ipsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("ipkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M5_Ip_Pay(idipcarabayar, idip, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("ipstatus") = 2 And Len(strGiro.ToString) > 0 Then
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

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "IP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ipstatus") = 2 Then
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
    Public Function M5_IpUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "Ip", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Iptgl, Ipnotransaksi, Ipstatus FROM M5_Ip WHERE Ipid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ipstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_ip_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Ip_HistorySimpan("" & paramSplit(0) & "★M5_Ip_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m5_ip_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = asdatatableambildaridbcon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'IP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'IP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'IP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M5_Ip SET Ipstatus = " & nilaiStatus & ", Ipmodifikasiuser='" & userid & "', Ipmodifikasitgl = NOW(), Ipposting = 0, Ippostingtgl = '1971-01-01 00:00:00', Ipjmlrevisi = Ipjmlrevisi + 1 WHERE Ipid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IpSearch(PostWsSearch(paramSplit(0), "M5_IpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_IpDelete(ByVal param As String) As String

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
            Dim sumber As String = "Ip", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Ipid, Ipnotransaksi FROM M5_Ip WHERE Ipid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ipcabang, iplokasi, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl"
            sql &= " FROM M5_ip"
            sql &= " WHERE ipid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ipcabang")
                lokasi = dtNomorNext.Rows(0)("iplokasi")
                sumber = dtNomorNext.Rows(0)("ipsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ipautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ipnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("iptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Ip_Pay WHERE idip = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Ip WHERE ipid = '" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 5)
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
            Dim paramSearch As String = M5_IpSearch(PostWsSearch(paramSplit(0), "M5_IpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_IpGetdataById(ByVal param As String) As String
        'M5_IpGetdataById Utama --------------------------------------------------------
        'ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, 
        'iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, 
        'ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, 
        'ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, 
        'ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, 
        'ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, 
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ippostingtgl, ipisclose, 
        'ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, 
        'ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3, 
        'ipcabangnama, iplokasinama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, ipterminnama, 
        'ipterminharijatuhtempo, sonotransaksi, ipnoreknama, ipcostcenternama, ipdivisinama, ipsubdivisinama, ipproyeknama, 
        'ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama, kpkp

        'M5_IpGetdataById Pay -------------------------------------------------------
        'idipcarabayar, idip, carabayar, matauang, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Ip~M5_Ip_Pay-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ipid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ipid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_ip_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ipid"), 0), sptField,
                     FxDB(drutama("ipcabang"), ""), sptField,
                     FxDB(drutama("iplokasi"), ""), sptField,
                     FxDB(drutama("ipjenis"), 0), sptField,
                     FxDB(drutama("ipsumber"), ""), sptField,
                     FxDB(drutama("ipautonotransaksi"), 0), sptField,
                     FxDB(drutama("ipnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("iptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ipkodepa"), 0), sptField,
                     FxDB(drutama("ipkontak"), 0), sptField,
                     FxDB(drutama("ipkontakperson"), ""), sptField,
                     FxDB(drutama("ip1alamat1"), ""), sptField,
                     FxDB(drutama("ip1alamat2"), ""), sptField,
                     FxDB(drutama("ip1alamat3"), ""), sptField,
                     FxDB(drutama("ip2alamat1"), ""), sptField,
                     FxDB(drutama("ip2alamat2"), ""), sptField,
                     FxDB(drutama("ip2alamat3"), ""), sptField,
                     FxDB(drutama("ipbagianterima"), 0), sptField,
                     FxDB(drutama("iptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("iptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("ipidso"), 0), sptField,
                     FxDB(drutama("ipnorek"), ""), sptField,
                     FxDB(drutama("ipuraian"), ""), sptField,
                     FxDB(drutama("ipcatatan"), ""), sptField,
                     FxDB(drutama("ipnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("iptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("ipmatauang"), ""), sptField,
                     FxDB(drutama("ipkurs"), 0), sptField,
                     FxDB(drutama("ipjumlah"), 0), sptField,
                     FxDB(drutama("ipjumlahvalas"), 0), sptField,
                     FxDB(drutama("ipjumlahbayar"), 0), sptField,
                     FxDB(drutama("ipjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("ipstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("iptgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("ipcostcenter"), ""), sptField,
                     FxDB(drutama("ipdivisi"), ""), sptField,
                     FxDB(drutama("ipsubdivisi"), ""), sptField,
                     FxDB(drutama("ipproyek"), ""), sptField,
                     FxDB(drutama("ipstatus"), 0), sptField,
                     FxDB(drutama("ipstatussebelumnya"), 0), sptField,
                     FxDB(drutama("ipjmlrevisi"), 0), sptField,
                     FxDB(drutama("ipcetakanke"), 0), sptField,
                     FxDB(drutama("ipinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ipinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ipmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ipmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ipposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ippostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ipisclose"), 0), sptField,
                     FxDB(drutama("ipcustomtext1"), ""), sptField,
                     FxDB(drutama("ipcustomtext2"), ""), sptField,
                     FxDB(drutama("ipcustomtext3"), ""), sptField,
                     FxDB(drutama("ipcustomtext4"), ""), sptField,
                     FxDB(drutama("ipcustomtext5"), ""), sptField,
                     FxDB(drutama("ipcustomint1"), 0), sptField,
                     FxDB(drutama("ipcustomint2"), 0), sptField,
                     FxDB(drutama("ipcustomint3"), 0), sptField,
                     FxDB(drutama("ipcustomdbl1"), 0), sptField,
                     FxDB(drutama("ipcustomdbl2"), 0), sptField,
                     FxDB(drutama("ipcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ipcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ipcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ipcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ipcabangnama"), ""), sptField,
                     FxDB(drutama("iplokasinama"), ""), sptField,
                     FxDB(drutama("ipkontakkode"), ""), sptField,
                     FxDB(drutama("ipkontaknama"), ""), sptField,
                     FxDB(drutama("ipbagianterimakode"), ""), sptField,
                     FxDB(drutama("ipbagianterimanama"), ""), sptField,
                     FxDB(drutama("ipterminnama"), ""), sptField,
                     FxDB(drutama("ipterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sonotransaksi"), ""), sptField,
                     FxDB(drutama("ipnoreknama"), ""), sptField,
                     FxDB(drutama("ipcostcenternama"), ""), sptField,
                     FxDB(drutama("ipdivisinama"), ""), sptField,
                     FxDB(drutama("ipsubdivisinama"), ""), sptField,
                     FxDB(drutama("ipproyeknama"), ""), sptField,
                     FxDB(drutama("ipstatusnama"), ""), sptField,
                     FxDB(drutama("ipstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("ipinputusernama"), ""), sptField,
                     FxDB(drutama("ipmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idipcarabayar"), 0), sptField,
                     FxDB(dr("idip"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ippostingtgl, ipisclose, ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3, ipcabangnama, iplokasinama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, ipterminnama, ipterminharijatuhtempo, sonotransaksi, ipnoreknama, ipcostcenternama, ipdivisinama, ipsubdivisinama, ipproyeknama, ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama, kpkp"), sptSubParam, ReplaceMapping("idipcarabayar, idip, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IpSearch(ByVal param As String) As String
        'M5_IpSearch --------------------------------------------------------
        'ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, 
        'iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, 
        'ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, 
        'ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, 
        'ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, 
        'ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, 
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ippostingtgl, ipisclose, 
        'ipcabangnama, iplokasinama, ipjenisnama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, 
        'sonotransaksi, ipnoreknama, ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama

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
            Filter = Filter.Replace("ipkontakkode", "c1.kkode")
            Filter = Filter.Replace("ipkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_ip_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Ip", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ipid"), 0), sptField,
                     FxDB(dr("ipcabang"), ""), sptField,
                     FxDB(dr("iplokasi"), ""), sptField,
                     FxDB(dr("ipjenis"), 0), sptField,
                     FxDB(dr("ipsumber"), ""), sptField,
                     FxDB(dr("ipautonotransaksi"), 0), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("iptgl"), ""), formatTgl), sptField,
                     FxDB(dr("ipkodepa"), 0), sptField,
                     FxDB(dr("ipkontak"), 0), sptField,
                     FxDB(dr("ipkontakperson"), ""), sptField,
                     FxDB(dr("ip1alamat1"), ""), sptField,
                     FxDB(dr("ip1alamat2"), ""), sptField,
                     FxDB(dr("ip1alamat3"), ""), sptField,
                     FxDB(dr("ip2alamat1"), ""), sptField,
                     FxDB(dr("ip2alamat2"), ""), sptField,
                     FxDB(dr("ip2alamat3"), ""), sptField,
                     FxDB(dr("ipbagianterima"), 0), sptField,
                     FxDB(dr("iptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("iptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("ipidso"), 0), sptField,
                     FxDB(dr("ipnorek"), ""), sptField,
                     FxDB(dr("ipuraian"), ""), sptField,
                     FxDB(dr("ipcatatan"), ""), sptField,
                     FxDB(dr("ipnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("iptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("ipmatauang"), ""), sptField,
                     FxDB(dr("ipkurs"), 0), sptField,
                     FxDB(dr("ipjumlah"), 0), sptField,
                     FxDB(dr("ipjumlahvalas"), 0), sptField,
                     FxDB(dr("ipjumlahbayar"), 0), sptField,
                     FxDB(dr("ipjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("ipstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("iptgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("ipcostcenter"), ""), sptField,
                     FxDB(dr("ipdivisi"), ""), sptField,
                     FxDB(dr("ipsubdivisi"), ""), sptField,
                     FxDB(dr("ipproyek"), ""), sptField,
                     FxDB(dr("ipstatus"), 0), sptField,
                     FxDB(dr("ipstatussebelumnya"), 0), sptField,
                     FxDB(dr("ipjmlrevisi"), 0), sptField,
                     FxDB(dr("ipcetakanke"), 0), sptField,
                     FxDB(dr("ipinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ipinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ipmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ippostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ipisclose"), 0), sptField,
                     FxDB(dr("ipcabangnama"), ""), sptField,
                     FxDB(dr("iplokasinama"), ""), sptField,
                     FxDB(dr("ipjenisnama"), ""), sptField,
                     FxDB(dr("ipkontakkode"), ""), sptField,
                     FxDB(dr("ipkontaknama"), ""), sptField,
                     FxDB(dr("ipbagianterimakode"), ""), sptField,
                     FxDB(dr("ipbagianterimanama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("ipnoreknama"), ""), sptField,
                     FxDB(dr("ipstatusnama"), ""), sptField,
                     FxDB(dr("ipstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ipinputusernama"), ""), sptField,
                     FxDB(dr("ipmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, ipdivisi, ipsubdivisi, riproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, riposting, ripostingtgl, ipisclose, ipcabangnama, iplokasinama, ipjenisnama, ipkontakkode, ipkontaknama, ipbagianterimakode, ipbagianterimanama, sonotransaksi, ipnoreknama, ipstatusnama, ipstatussebelumnyanama, ipinputusernama, ipmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IpTerkait(ByVal param As String) As String
        'M5_IpTerkait --------------------------------------------------------
        'ipid, ipnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "ipid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_ip_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ipid"), 0), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptField,
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
            result(2) = "Related IP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ipid, ipnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IpSimpanOld(ByVal param As String) As String
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
        'ipid(0) As Integer, ipcabang(1) As String, iplokasi(2) As String, ipjenis(3) As Integer, ipsumber(4) As String, 
        'ipautonotransaksi(5) As Integer, ipnotransaksi(6) As String, iptgl(7) As Date, ipkodepa(8) As Integer, ipkontak(9) As Integer, 
        'ipkontakperson(10) As String, ip1alamat1(11) As String, ip1alamat2(12) As String, ip1alamat3(13) As String, ip2alamat1(14) As String, 
        'ip2alamat2(15) As String, ip2alamat3(16) As String, ipbagianterima(17) As Integer, iptermin(18) As String, iptgljatuhtempo(19) As Date, 
        'ipidso(20) As Integer, ipnorek(21) As String, ipuraian(22) As String, ipcatatan(23) As String, ipnoref(24) As String, 
        'iptglnoref(25) As Date, ipmatauang(26) As String, ipkurs(27) As Double, ipjumlah(28) As Double, ipjumlahvalas(29) As Double, 
        'ipjumlahbayar(30) As Double, ipjumlahbayarvalas(31) As Double, ipstatusbayar(32) As Integer, iptgllunas(33) As Date, ipcostcenter(34) As String, 
        'ipdivisi(35) As String, ipsubdivisi(36) As String, ipproyek(37) As String, ipstatus(38) As Integer, ipstatussebelumnya(39) As Integer, 
        'ipjmlrevisi(40) As Integer, ipcetakanke(41) As Integer, ipinputuser(42) As Integer, ipinputtgl(43) As DateTime, ipmodifikasiuser(44) As Integer, 
        'ipmodifikasitgl(45) As DateTime, ipposting(46) As Integer, ipisclose(47) As Integer, ipcustomtext1(48) As String, ipcustomtext2(49) As String, 
        'ipcustomtext3(50) As String, ipcustomtext4(51) As String, ipcustomtext5(52) As String, ipcustomint1(53) As Integer, ipcustomint2(54) As Integer, 
        'ipcustomint3(55) As Integer, ipcustomdbl1(56) As Double, ipcustomdbl2(57) As Double, ipcustomdbl3(58) As Double, ipcustomdate1(59) As Date, 
        'ipcustomdate2(60) As Date, ipcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ipid, ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, 
        'iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, 
        'ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, 
        'ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, 
        'ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, 
        'ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, 
        'ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ipisclose, ipcustomtext1, 
        'ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, 
        'ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ipid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ipid required numeric." : GoTo selesai
        End If
        'ipjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "ipjenis required numeric." : GoTo selesai
        End If
        'ipautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "ipautonotransaksi required numeric." : GoTo selesai
        End If
        'iptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "iptgl required date." : GoTo selesai
        End If
        'ipkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ipkodepa required numeric." : GoTo selesai
        End If
        'ipkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "ipkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "ipkontak can't be empty." : GoTo selesai
        End If
        'ipbagianterima(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "ipbagianterima required numeric." : GoTo selesai
        End If
        'iptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "iptgljatuhtempo required date." : GoTo selesai
        End If
        'ipidso(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ipidso required numeric." : GoTo selesai
        End If
        'iptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "iptglnoref required date." : GoTo selesai
        End If
        'ipkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ipkurs required numeric." : GoTo selesai
        End If
        'ipjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ipjumlah required numeric." : GoTo selesai
        End If
        'ipjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ipjumlahvalas required numeric." : GoTo selesai
        End If
        'ipjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "ipjumlahbayar required numeric." : GoTo selesai
        End If
        'ipjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "ipjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'ipstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "ipstatusbayar required numeric." : GoTo selesai
        End If
        'iptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "iptgllunas required date." : GoTo selesai
        End If
        'ipstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ipstatus required numeric." : GoTo selesai
        End If
        'ipstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ipstatussebelumnya required numeric." : GoTo selesai
        End If
        'ipjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ipjmlrevisi required numeric." : GoTo selesai
        End If
        'ipcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "ipcetakanke required numeric." : GoTo selesai
        End If
        'ipinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ipinputuser required numeric." : GoTo selesai
        End If
        'ipinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ipinputtgl required date." : GoTo selesai
        End If
        'ipmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "ipmodifikasiuser required numeric." : GoTo selesai
        End If
        'ipmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "ipmodifikasitgl required date." : GoTo selesai
        End If
        'ipposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "ipposting required numeric." : GoTo selesai
        End If
        'ipisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "ipisclose required numeric." : GoTo selesai
        End If
        'ipcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "ipcustomint1 required numeric." : GoTo selesai
        End If
        'ipcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "ipcustomint2 required numeric." : GoTo selesai
        End If
        'ipcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "ipcustomint3 required numeric." : GoTo selesai
        End If
        'ipcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "ipcustomdbl1 required numeric." : GoTo selesai
        End If
        'ipcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "ipcustomdbl2 required numeric." : GoTo selesai
        End If
        'ipcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "ipcustomdbl3 required numeric." : GoTo selesai
        End If
        'ipcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "ipcustomdate1 required date." : GoTo selesai
        End If
        'ipcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "ipcustomdate2 required date." : GoTo selesai
        End If
        'ipcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "ipcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ipcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ipcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ipcabang should not be more than 25 character." : GoTo selesai
        End If

        'iplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "iplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "iplokasi should not be more than 25 character." : GoTo selesai
        End If

        'ipsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "ipsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "ipsumber should not be more than 10 character." : GoTo selesai
        End If

        'ipnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ipnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ipnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'iptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "iptgl can't be empty" : GoTo selesai
        End If

        'iptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "iptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'ipnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "ipnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "ipnorek should not be more than 25 character." : GoTo selesai
        End If

        'iptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "iptglnoref can't be empty" : GoTo selesai
        End If

        'ipmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "ipmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "ipmatauang should not be more than 25 character." : GoTo selesai
        End If

        'ipkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "ipkurs can't be empty" : GoTo selesai
        End If

        'ipjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ipjumlah can't be empty" : GoTo selesai
        End If

        'ipjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ipjumlahvalas can't be empty" : GoTo selesai
        End If

        'ipjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "ipjumlahbayar can't be empty" : GoTo selesai
        End If

        'ipjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "ipjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'iptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "iptgllunas can't be empty" : GoTo selesai
        End If

        'ipinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ipinputtgl can't be empty" : GoTo selesai
        End If

        'ipmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "ipmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ipcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "ipcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ipcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "ipcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ipcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "ipcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ipcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "ipcustomdate1 can't be empty" : GoTo selesai
        End If

        'ipcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ipcustomdate2 can't be empty" : GoTo selesai
        End If

        'ipcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "ipcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ipid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ip2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "iptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ipjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "ipjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "iptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ipcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ipcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "ipid~ipcabang~iplokasi~ipjenis~ipsumber~ipautonotransaksi~ipnotransaksi~iptgl~ipkodepa~ipkontak~ipkontakperson~ip1alamat1~ip1alamat2~ip1alamat3~ip2alamat1~ip2alamat2~ip2alamat3~ipbagianterima~iptermin~iptgljatuhtempo~ipidso~ipnorek~ipuraian~ipcatatan~ipnoref~iptglnoref~ipmatauang~ipkurs~ipjumlah~ipjumlahvalas~ipjumlahbayar~ipjumlahbayarvalas~ipstatusbayar~iptgllunas~ipcostcenter~ipdivisi~ipsubdivisi~ipproyek~ipstatus~ipstatussebelumnya~ipjmlrevisi~ipcetakanke~ipinputuser~ipinputtgl~ipmodifikasiuser~ipmodifikasitgl~ipposting~ipisclose~ipcustomtext1~ipcustomtext2~ipcustomtext3~ipcustomtext4~ipcustomtext5~ipcustomint1~ipcustomint2~ipcustomint3~ipcustomdbl1~ipcustomdbl2~ipcustomdbl3~ipcustomdate1~ipcustomdate2~ipcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idipcarabayar(0) As Integer, idip(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idipcarabayar, idip, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idipcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idip", AsEnumTypeData.AsInt64)
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
            'idipcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idipcarabayar required numeric." : GoTo selesai
            End If
            'idip(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idip required numeric." : GoTo selesai
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
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jumlah must be more than zero" : GoTo selesai
            End If

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

            If AsDataTableTambahData(dtdetail, "idipcarabayar~idip~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
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

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("iptgl")), AsFormatTanggal(drutama("iptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "ipmatauang", "ipnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("iptermin").ToString, AsFormatTanggal(drutama("iptgl")), "iptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("iptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("ipjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("ipjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("ipjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("ipjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================


                If isUpdate Then
                    result(4) = drutama("ipid")
                    notransaksi = drutama("ipnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ipid), ipnotransaksi FROM M5_ip WHERE ipid='" & result(4) & "' AND ipstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ipid) FROM M5_ip WHERE ipnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_ip_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Ip_HistorySimpan("" & paramSplit(0) & "★M5_Ip_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("ipsumber")) & "▼" & FixQuotes(drutama("ipid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Ip set ipcabang  = '" & FixQuotes(drutama("ipcabang")) & "', iplokasi  = '" & FixQuotes(drutama("iplokasi")) & "', ipjenis  = " & drutama("ipjenis") & ", ipsumber  = '" & FixQuotes(drutama("ipsumber")) & "', ipautonotransaksi  = " & drutama("ipautonotransaksi") & ", ipnotransaksi  = '" & notransaksi & "', iptgl  = '" & FixQuotes(AsFormatTanggal(drutama("iptgl"))) & "', ipkodepa  = " & drutama("ipkodepa") & ", ipkontak  = " & drutama("ipkontak") & ", ipkontakperson  = '" & FixQuotes(drutama("ipkontakperson")) & "', ip1alamat1  = '" & FixQuotes(drutama("ip1alamat1")) & "', ip1alamat2  = '" & FixQuotes(drutama("ip1alamat2")) & "', ip1alamat3  = '" & FixQuotes(drutama("ip1alamat3")) & "', ip2alamat1  = '" & FixQuotes(drutama("ip2alamat1")) & "', ip2alamat2  = '" & FixQuotes(drutama("ip2alamat2")) & "', ip2alamat3  = '" & FixQuotes(drutama("ip2alamat3")) & "', ipbagianterima  = " & drutama("ipbagianterima") & ", iptermin  = '" & FixQuotes(drutama("iptermin")) & "', iptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("iptgljatuhtempo"))) & "', ipidso  = " & drutama("ipidso") & ", ipnorek  = '" & FixQuotes(drutama("ipnorek")) & "', ipuraian  = '" & FixQuotes(drutama("ipuraian")) & "', ipcatatan  = '" & FixQuotes(drutama("ipcatatan")) & "', ipnoref  = '" & FixQuotes(drutama("ipnoref")) & "', iptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("iptglnoref"))) & "', ipmatauang  = '" & FixQuotes(drutama("ipmatauang")) & "', ipkurs  = '" & FixDouble(drutama("ipkurs")) & "', ipjumlah  = '" & FixDouble(drutama("ipjumlah")) & "', ipjumlahvalas  = '" & FixDouble(drutama("ipjumlahvalas")) & "', ipjumlahbayar  = '" & FixDouble(drutama("ipjumlahbayar")) & "', ipjumlahbayarvalas  = '" & FixDouble(drutama("ipjumlahbayarvalas")) & "', ipstatusbayar  = " & drutama("ipstatusbayar") & ", iptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("iptgllunas"))) & "', ipcostcenter  = '" & FixQuotes(drutama("ipcostcenter")) & "', ipdivisi  = '" & FixQuotes(drutama("ipdivisi")) & "', ipsubdivisi  = '" & FixQuotes(drutama("ipsubdivisi")) & "', ipproyek  = '" & FixQuotes(drutama("ipproyek")) & "', ipstatus  = " & drutama("ipstatus") & ", ipstatussebelumnya  = " & drutama("ipstatussebelumnya") & ", ipjmlrevisi  = ipjmlrevisi+1, ipcetakanke  = " & drutama("ipcetakanke") & ", ipmodifikasiuser  = " & drutama("ipmodifikasiuser") & ", ipmodifikasitgl  = NOW(), ipposting  = 0, ipcustomtext1  = '" & FixQuotes(drutama("ipcustomtext1")) & "', ipcustomtext2  = '" & FixQuotes(drutama("ipcustomtext2")) & "', ipcustomtext3  = '" & FixQuotes(drutama("ipcustomtext3")) & "', ipcustomtext4  = '" & FixQuotes(drutama("ipcustomtext4")) & "', ipcustomtext5  = '" & FixQuotes(drutama("ipcustomtext5")) & "', ipcustomint1  = " & drutama("ipcustomint1") & ", ipcustomint2  = " & drutama("ipcustomint2") & ", ipcustomint3  = " & drutama("ipcustomint3") & ", ipcustomdbl1  = '" & FixDouble(drutama("ipcustomdbl1")) & "', ipcustomdbl2  = '" & FixDouble(drutama("ipcustomdbl2")) & "', ipcustomdbl3  = '" & FixDouble(drutama("ipcustomdbl3")) & "', ipcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate1"))) & "', ipcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate2"))) & "', ipcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate3"))) & "' where ipid = '" & drutama("ipid") & "'"
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

                    If drutama("ipautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ipcabang"), drutama("iplokasi"), drutama("ipsumber"), drutama("iptgl"))
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
                        notransaksi = drutama("ipnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ipid) FROM M5_ip WHERE ipnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Ip (ipcabang, iplokasi, ipjenis, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl, ipkodepa, ipkontak, ipkontakperson, ip1alamat1, ip1alamat2, ip1alamat3, ip2alamat1, ip2alamat2, ip2alamat3, ipbagianterima, iptermin, iptgljatuhtempo, ipidso, ipnorek, ipuraian, ipcatatan, ipnoref, iptglnoref, ipmatauang, ipkurs, ipjumlah, ipjumlahvalas, ipjumlahbayar, ipjumlahbayarvalas, ipstatusbayar, iptgllunas, ipcostcenter, ipdivisi, ipsubdivisi, ipproyek, ipstatus, ipstatussebelumnya, ipjmlrevisi, ipcetakanke, ipinputuser, ipinputtgl, ipmodifikasiuser, ipmodifikasitgl, ipposting, ipisclose, ipcustomtext1, ipcustomtext2, ipcustomtext3, ipcustomtext4, ipcustomtext5, ipcustomint1, ipcustomint2, ipcustomint3, ipcustomdbl1, ipcustomdbl2, ipcustomdbl3, ipcustomdate1, ipcustomdate2, ipcustomdate3) values('" & FixQuotes(drutama("ipcabang")) & "', '" & FixQuotes(drutama("iplokasi")) & "', " & drutama("ipjenis") & ", '" & FixQuotes(drutama("ipsumber")) & "', " & drutama("ipautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("iptgl"))) & "', " & drutama("ipkodepa") & ", " & drutama("ipkontak") & ", '" & FixQuotes(drutama("ipkontakperson")) & "', '" & FixQuotes(drutama("ip1alamat1")) & "', '" & FixQuotes(drutama("ip1alamat2")) & "', '" & FixQuotes(drutama("ip1alamat3")) & "', '" & FixQuotes(drutama("ip2alamat1")) & "', '" & FixQuotes(drutama("ip2alamat2")) & "', '" & FixQuotes(drutama("ip2alamat3")) & "', " & drutama("ipbagianterima") & ", '" & FixQuotes(drutama("iptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("iptgljatuhtempo"))) & "', " & drutama("ipidso") & ", '" & FixQuotes(drutama("ipnorek")) & "', '" & FixQuotes(drutama("ipuraian")) & "', '" & FixQuotes(drutama("ipcatatan")) & "', '" & FixQuotes(drutama("ipnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("iptglnoref"))) & "', '" & FixQuotes(drutama("ipmatauang")) & "', '" & FixDouble(drutama("ipkurs")) & "', '" & FixDouble(drutama("ipjumlah")) & "', '" & FixDouble(drutama("ipjumlahvalas")) & "', '" & FixDouble(drutama("ipjumlahbayar")) & "', '" & FixDouble(drutama("ipjumlahbayarvalas")) & "', " & drutama("ipstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("iptgllunas"))) & "', '" & FixQuotes(drutama("ipcostcenter")) & "', '" & FixQuotes(drutama("ipdivisi")) & "', '" & FixQuotes(drutama("ipsubdivisi")) & "', '" & FixQuotes(drutama("ipproyek")) & "', " & drutama("ipstatus") & ", " & drutama("ipstatussebelumnya") & ", " & drutama("ipjmlrevisi") & ", " & drutama("ipcetakanke") & ", " & drutama("ipinputuser") & ", NOW(), " & drutama("ipmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ipisclose") & ", '" & FixQuotes(drutama("ipcustomtext1")) & "', '" & FixQuotes(drutama("ipcustomtext2")) & "', '" & FixQuotes(drutama("ipcustomtext3")) & "', '" & FixQuotes(drutama("ipcustomtext4")) & "', '" & FixQuotes(drutama("ipcustomtext5")) & "', " & drutama("ipcustomint1") & ", " & drutama("ipcustomint2") & ", " & drutama("ipcustomint3") & ", '" & FixDouble(drutama("ipcustomdbl1")) & "', '" & FixDouble(drutama("ipcustomdbl2")) & "', '" & FixDouble(drutama("ipcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ipcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select ipid from M5_ip where ipnotransaksi='" & notransaksi & "' AND ipinputuser= '" & userid & "' order by ipmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Ip_Pay where idip = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idipcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("ipsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("ipkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 0 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M5_Ip_Pay(idipcarabayar, idip, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("ipstatus") = 2 And Len(strGiro.ToString) > 0 Then
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
                Dim sumber As String = "IP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ipstatus") = 2 Then
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
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
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
    Public Function M5_IpUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Ip", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Iptgl, Ipnotransaksi, Ipstatus FROM M5_Ip WHERE Ipid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ipstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_ip_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Ip_HistorySimpan("" & paramSplit(0) & "★M5_Ip_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m5_ip_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'IP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'IP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'IP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M5_Ip SET Ipstatus = " & nilaiStatus & ", Ipmodifikasiuser='" & userid & "', Ipmodifikasitgl = NOW(), Ipposting = 0, Ippostingtgl = '1971-01-01 00:00:00', Ipjmlrevisi = Ipjmlrevisi + 1 WHERE Ipid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IpSearch(PostWsSearch(paramSplit(0), "M5_IpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_IpDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Ip", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Ipid, Ipnotransaksi FROM M5_Ip WHERE Ipid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ipcabang, iplokasi, ipsumber, ipautonotransaksi, ipnotransaksi, iptgl"
            sql &= " FROM M5_ip"
            sql &= " WHERE ipid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ipcabang")
                lokasi = dtNomorNext.Rows(0)("iplokasi")
                sumber = dtNomorNext.Rows(0)("ipsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ipautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ipnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("iptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Ip_Pay WHERE idip = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Ip WHERE ipid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_IpSearch(PostWsSearch(paramSplit(0), "M5_IpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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