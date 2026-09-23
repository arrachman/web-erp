Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_jm
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_JmSimpan(ByVal param As String) As String
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
        Dim strRekCostCenter As String = ""

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
        'jmid(0) As Integer, jmcabang(1) As String, jmlokasi(2) As String, jmsumber(3) As String, jmautonotransaksi(4) As Integer, 
        'jmnotransaksi(5) As String, jmtgl(6) As Date, jmkodepa(7) As Integer, jmkontakperson(8) As String, jmuraian(9) As String, 
        'jmcatatan(10) As String, jmmatauang(11) As String, jmkurs(12) As Double, jmdebit(13) As Double, jmdebitvalas(14) As Double, 
        'jmkredit(15) As Double, jmkreditvalas(16) As Double, jmjumlahbayar(17) As Double, jmjumlahbayarvalas(18) As Double, jmstatusbayar(19) As Integer, 
        'jmtgllunas(20) As Date, jmstatus(21) As Integer, jmstatussebelumnya(22) As Integer, jmjmlrevisi(23) As Integer, jmcetakanke(24) As Integer, 
        'jmisclose(25) As Integer, jminputuser(26) As Integer, jminputtgl(27) As DateTime, jmmodifikasiuser(28) As Integer, jmmodifikasitgl(29) As DateTime, 
        'jmposting(30) As Integer, jmcustomtext1(31) As String, jmcustomtext2(32) As String, jmcustomtext3(33) As String, jmcustomtext4(34) As String, 
        'jmcustomtext5(35) As String, jmcustomint1(36) As Integer, jmcustomint2(37) As Integer, jmcustomint3(38) As Integer, jmcustomdbl1(39) As Double, 
        'jmcustomdbl2(40) As Double, jmcustomdbl3(41) As Double, jmcustomdate1(42) As Date, jmcustomdate2(43) As Date, jmcustomdate3(44) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, 
        'jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, 
        'jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, 
        'jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, 
        'jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmcustomtext1, jmcustomtext2, jmcustomtext3, 
        'jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, jmcustomdbl2, 
        'jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'jmid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "jmid required numeric." : GoTo selesai
        End If
        'jmautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "jmautonotransaksi required numeric." : GoTo selesai
        End If
        'jmtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "jmtgl required date." : GoTo selesai
        End If
        'jmkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "jmkodepa required numeric." : GoTo selesai
        End If
        'jmkurs(12) As Double
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "jmkurs required numeric." : GoTo selesai
        End If
        'jmdebit(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "jmdebit required numeric." : GoTo selesai
        End If
        'jmdebitvalas(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "jmdebitvalas required numeric." : GoTo selesai
        End If
        'jmkredit(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "jmkredit required numeric." : GoTo selesai
        End If
        'jmkreditvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "jmkreditvalas required numeric." : GoTo selesai
        End If
        'jmjumlahbayar(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "jmjumlahbayar required numeric." : GoTo selesai
        End If
        'jmjumlahbayarvalas(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "jmjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'jmstatusbayar(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "jmstatusbayar required numeric." : GoTo selesai
        End If
        'jmtgllunas(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "jmtgllunas required date." : GoTo selesai
        End If
        'jmstatus(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "jmstatus required numeric." : GoTo selesai
        End If
        'jmstatussebelumnya(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "jmstatussebelumnya required numeric." : GoTo selesai
        End If
        'jmjmlrevisi(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "jmjmlrevisi required numeric." : GoTo selesai
        End If
        'jmcetakanke(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "jmcetakanke required numeric." : GoTo selesai
        End If
        'jmisclose(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "jmisclose required numeric." : GoTo selesai
        End If
        'jminputuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "jminputuser required numeric." : GoTo selesai
        End If
        'jminputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "jminputtgl required date." : GoTo selesai
        End If
        'jmmodifikasiuser(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "jmmodifikasiuser required numeric." : GoTo selesai
        End If
        'jmmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "jmmodifikasitgl required date." : GoTo selesai
        End If
        'jmposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "jmposting required numeric." : GoTo selesai
        End If
        'jmcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "jmcustomint1 required numeric." : GoTo selesai
        End If
        'jmcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "jmcustomint2 required numeric." : GoTo selesai
        End If
        'jmcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "jmcustomint3 required numeric." : GoTo selesai
        End If
        'jmcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "jmcustomdbl1 required numeric." : GoTo selesai
        End If
        'jmcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "jmcustomdbl2 required numeric." : GoTo selesai
        End If
        'jmcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "jmcustomdbl3 required numeric." : GoTo selesai
        End If
        'jmcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "jmcustomdate1 required date." : GoTo selesai
        End If
        'jmcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "jmcustomdate2 required date." : GoTo selesai
        End If
        'jmcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "jmcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'jmcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "jmcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "jmcabang should not be more than 25 character." : GoTo selesai
        End If

        'jmlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "jmlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "jmlokasi should not be more than 25 character." : GoTo selesai
        End If

        'jmsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "jmsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "jmsumber should not be more than 10 character." : GoTo selesai
        End If

        'jmnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "jmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "jmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'jmtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "jmtgl can't be empty" : GoTo selesai
        End If

        'jmmatauang(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "jmmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 25 Then
            result(2) = "jmmatauang should not be more than 25 character." : GoTo selesai
        End If

        'jmkurs(12) As Double
        If Len(dataUtama(12)) = 0 Then
            result(2) = "jmkurs can't be empty" : GoTo selesai
        End If

        'jmdebit(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "jmdebit can't be empty" : GoTo selesai
        End If

        'jmdebitvalas(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "jmdebitvalas can't be empty" : GoTo selesai
        End If

        'jmkredit(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "jmkredit can't be empty" : GoTo selesai
        End If

        'jmkreditvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "jmkreditvalas can't be empty" : GoTo selesai
        End If

        'jmjumlahbayar(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "jmjumlahbayar can't be empty" : GoTo selesai
        End If

        'jmjumlahbayarvalas(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "jmjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'jminputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "jminputtgl can't be empty" : GoTo selesai
        End If

        'jmmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "jmmodifikasitgl can't be empty" : GoTo selesai
        End If

        'jmcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "jmcustomdbl1 can't be empty" : GoTo selesai
        End If

        'jmcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "jmcustomdbl2 can't be empty" : GoTo selesai
        End If

        'jmcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "jmcustomdbl3 can't be empty" : GoTo selesai
        End If

        'jmcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "jmcustomdate1 can't be empty" : GoTo selesai
        End If

        'jmcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "jmcustomdate2 can't be empty" : GoTo selesai
        End If

        'jmcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "jmcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "jmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmdebit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmdebitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmkredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmkreditvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "jmid~jmcabang~jmlokasi~jmsumber~jmautonotransaksi~jmnotransaksi~jmtgl~jmkodepa~jmkontakperson~jmuraian~jmcatatan~jmmatauang~jmkurs~jmdebit~jmdebitvalas~jmkredit~jmkreditvalas~jmjumlahbayar~jmjumlahbayarvalas~jmstatusbayar~jmtgllunas~jmstatus~jmstatussebelumnya~jmjmlrevisi~jmcetakanke~jmisclose~jminputuser~jminputtgl~jmmodifikasiuser~jmmodifikasitgl~jmposting~jmcustomtext1~jmcustomtext2~jmcustomtext3~jmcustomtext4~jmcustomtext5~jmcustomint1~jmcustomint2~jmcustomint3~jmcustomdbl1~jmcustomdbl2~jmcustomdbl3~jmcustomdate1~jmcustomdate2~jmcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idjmdetail(0) As Integer, idjm(1) As Integer, kontak(2) As Integer, norek(3) As String, matauang(4) As String, kurs(5) As Double, 
        'debit(6) As Double, debitvalas(7) As Double, kredit(8) As Double, kreditvalas(9) As Double, catatan(10) As String, 
        'costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, urutan(15) As Integer, 
        'isclose(16) As Integer, customtext1(17) As String, customtext2(18) As String, customtext3(19) As String, customdbl1(20) As Double, 
        'customdbl2(21) As Double, customdbl3(22) As Double, customdate1(23) As Date, customdate2(24) As Date, customdate3(25) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idjmdetail, idjm, kontak, norek, matauang, kurs, debit, debitvalas, 
        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idjmdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idjm", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "debit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "debitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kreditvalas", AsEnumTypeData.AsDouble)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 26) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idjmdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idjmdetail required numeric." : GoTo selesai
            End If
            'idjm(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idjm required numeric." : GoTo selesai
            End If
            'kontak(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If
            If (dataRowDetail(2) < 1) Then
                result(2) = "Row : " & i & " - kontak can't be empty." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'debit(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - debit required numeric." : GoTo selesai
            End If
            'debitvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - debitvalas required numeric." : GoTo selesai
            End If
            'kredit(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - kredit required numeric." : GoTo selesai
            End If
            'kreditvalas(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - kreditvalas required numeric." : GoTo selesai
            End If
            'urutan(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'norek(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
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

            'debit(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - debit can't be empty" : GoTo selesai
            End If

            'debitvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - debitvalas can't be empty" : GoTo selesai
            End If

            'kredit(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - kredit can't be empty" : GoTo selesai
            End If

            'kreditvalas(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - kreditvalas can't be empty" : GoTo selesai
            End If

            'validasi jumlah debit dan kredit tidak boleh diisi keduanya
            If dataRowDetail(6) = 0 And dataRowDetail(8) = 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be zero" : GoTo selesai
            End If
            If dataRowDetail(6) <> 0 And dataRowDetail(8) <> 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be filled in both" : GoTo selesai
            End If
            If dataRowDetail(7) <> 0 And dataRowDetail(9) <> 0 Then
                result(2) = "Row : " & i & " - foreign debits and credits can't be filled in both" : GoTo selesai
            End If

            'customdbl1(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idjmdetail~idjm~kontak~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(3) & "')")

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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 138
                Select Case drutama("jmstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("jmtgl")), AsFormatTanggal(drutama("jmtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "jmmatauang", "", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("jmstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'VALIDASI NOMINAL HARUS SEIMBANG ========================
                Dim debit As Double = 0, kredit As Double = 0, debitvalas As Double = 0, kreditvalas As Double = 0
                debit = AsDataTableDSum(dtdetail, "debit")
                debitvalas = AsDataTableDSum(dtdetail, "debitvalas")
                kredit = AsDataTableDSum(dtdetail, "kredit")
                kreditvalas = AsDataTableDSum(dtdetail, "kreditvalas")

                ''AMBIL SETTING FORMAT NOMINAL
                'Dim digitGroup As String = "", pemisahDesimal As String = "", digitDesimal As Integer = 0
                'Dim FNominal As String = GetSettingNominal(digitGroup, pemisahDesimal, digitDesimal)
                'If len(FNominal) <> 0 Then result(2) = FNominal : Trans.Rollback() : GoTo selesai

                ''BULATKAN NOMINAL DETAIL SESUAI SETTING FORMAT NOMINAL
                'debit = math.round(debit, digitDesimal)
                'debitvalas = math.round(debitvalas, digitDesimal)
                'kredit = math.round(kredit, digitDesimal)
                'kreditvalas = math.round(kreditvalas, digitDesimal)

                'VALIDASI NOMINAL HARUS SEIMBANG
                If Math.Round(debit, 2) <> Math.Round(kredit, 2) Then
                    result(2) = "Total debits and credits in detail are not balanced." : GoTo selesai
                End If
                If Math.Round(debitvalas, 2) <> Math.Round(kreditvalas, 2) Then
                    result(2) = "Total foreign debits and credits in detail are not balanced." : GoTo selesai
                End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("jmdebit") = debit
                drutama("jmdebitvalas") = debitvalas
                drutama("jmkredit") = kredit
                drutama("jmkreditvalas") = kreditvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("jmid")
                    notransaksi = drutama("jmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(jmid), jmnotransaksi FROM M2_jm WHERE jmid='" & result(4) & "' AND jmstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("jmautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("jmcabang"), drutama("jmlokasi"), drutama("jmsumber"), drutama("jmtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(jmid) FROM m2_jm WHERE jmnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_jm_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Jm_HistorySimpan("" & paramSplit(0) & "★M2_Jm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("jmsumber")) & "▼" & FixQuotes(drutama("jmid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Jm set jmcabang  = '" & FixQuotes(drutama("jmcabang")) & "', jmlokasi  = '" & FixQuotes(drutama("jmlokasi")) & "', jmsumber  = '" & FixQuotes(drutama("jmsumber")) & "', jmautonotransaksi  = " & drutama("jmautonotransaksi") & ", jmnotransaksi  = '" & notransaksi & "', jmtgl  = '" & FixQuotes(AsFormatTanggal(drutama("jmtgl"))) & "', jmkodepa  = " & drutama("jmkodepa") & ", jmkontakperson  = '" & FixQuotes(drutama("jmkontakperson")) & "', jmuraian  = '" & FixQuotes(drutama("jmuraian")) & "', jmcatatan  = '" & FixQuotes(drutama("jmcatatan")) & "', jmmatauang  = '" & FixQuotes(drutama("jmmatauang")) & "', jmkurs  = '" & FixDouble(drutama("jmkurs")) & "', jmdebit  = '" & FixDouble(drutama("jmdebit")) & "', jmdebitvalas  = '" & FixDouble(drutama("jmdebitvalas")) & "', jmkredit  = '" & FixDouble(drutama("jmkredit")) & "', jmkreditvalas  = '" & FixDouble(drutama("jmkreditvalas")) & "', jmjumlahbayar  = '" & FixDouble(drutama("jmjumlahbayar")) & "', jmjumlahbayarvalas  = '" & FixDouble(drutama("jmjumlahbayarvalas")) & "', jmstatusbayar  = " & drutama("jmstatusbayar") & ", jmtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("jmtgllunas"))) & "', jmstatus  = " & drutama("jmstatus") & ", jmstatussebelumnya  = " & drutama("jmstatussebelumnya") & ", jmjmlrevisi  = jmjmlrevisi+1, jmcetakanke  = " & drutama("jmcetakanke") & ", jmisclose  = " & drutama("jmisclose") & ", jmmodifikasiuser  = " & drutama("jmmodifikasiuser") & ", jmmodifikasitgl  = NOW(), jmposting  = 0, jmcustomtext1  = '" & FixQuotes(drutama("jmcustomtext1")) & "', jmcustomtext2  = '" & FixQuotes(drutama("jmcustomtext2")) & "', jmcustomtext3  = '" & FixQuotes(drutama("jmcustomtext3")) & "', jmcustomtext4  = '" & FixQuotes(drutama("jmcustomtext4")) & "', jmcustomtext5  = '" & FixQuotes(drutama("jmcustomtext5")) & "', jmcustomint1  = " & drutama("jmcustomint1") & ", jmcustomint2  = " & drutama("jmcustomint2") & ", jmcustomint3  = " & drutama("jmcustomint3") & ", jmcustomdbl1  = '" & FixDouble(drutama("jmcustomdbl1")) & "', jmcustomdbl2  = '" & FixDouble(drutama("jmcustomdbl2")) & "', jmcustomdbl3  = '" & FixDouble(drutama("jmcustomdbl3")) & "', jmcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate1"))) & "', jmcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate2"))) & "', jmcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate3"))) & "' where jmid = '" & drutama("jmid") & "'"
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

                    If drutama("jmautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("jmcabang"), drutama("jmlokasi"), drutama("jmsumber"), drutama("jmtgl"))
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
                        notransaksi = drutama("jmnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(jmid) FROM m2_jm WHERE jmnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Jm (jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmcustomtext1, jmcustomtext2, jmcustomtext3, jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, jmcustomdbl2, jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3) values('" & FixQuotes(drutama("jmcabang")) & "', '" & FixQuotes(drutama("jmlokasi")) & "', '" & FixQuotes(drutama("jmsumber")) & "', " & drutama("jmautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("jmtgl"))) & "', " & drutama("jmkodepa") & ", '" & FixQuotes(drutama("jmkontakperson")) & "', '" & FixQuotes(drutama("jmuraian")) & "', '" & FixQuotes(drutama("jmcatatan")) & "', '" & FixQuotes(drutama("jmmatauang")) & "', '" & FixDouble(drutama("jmkurs")) & "', '" & FixDouble(drutama("jmdebit")) & "', '" & FixDouble(drutama("jmdebitvalas")) & "', '" & FixDouble(drutama("jmkredit")) & "', '" & FixDouble(drutama("jmkreditvalas")) & "', '" & FixDouble(drutama("jmjumlahbayar")) & "', '" & FixDouble(drutama("jmjumlahbayarvalas")) & "', " & drutama("jmstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("jmtgllunas"))) & "', " & drutama("jmstatus") & ", " & drutama("jmstatussebelumnya") & ", " & drutama("jmjmlrevisi") & ", " & drutama("jmcetakanke") & ", " & drutama("jmisclose") & ", " & drutama("jminputuser") & ", NOW(), " & drutama("jmmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("jmcustomtext1")) & "', '" & FixQuotes(drutama("jmcustomtext2")) & "', '" & FixQuotes(drutama("jmcustomtext3")) & "', '" & FixQuotes(drutama("jmcustomtext4")) & "', '" & FixQuotes(drutama("jmcustomtext5")) & "', " & drutama("jmcustomint1") & ", " & drutama("jmcustomint2") & ", " & drutama("jmcustomint3") & ", '" & FixDouble(drutama("jmcustomdbl1")) & "', '" & FixDouble(drutama("jmcustomdbl2")) & "', '" & FixDouble(drutama("jmcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select jmid from M2_jm where jmnotransaksi='" & notransaksi & "' AND jminputuser= '" & userid & "' order by jmmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Jm_Detail where idjm = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idjmdetail") & ", " & result(4) & ", " & FixQuotes(dr1("kontak")) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Jm_Detail(idjmdetail, idjm, kontak, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "JM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("jmstatus") = 2 Then
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
    Public Function M2_JmUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("jmkontakkode", "c1.kkode")
            Filter = Filter.Replace("jmkontaknama", "c1.knama")
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
            Dim sumber As String = "Jm", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Jmtgl, Jmnotransaksi, Jmstatus FROM m2_Jm WHERE Jmid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Jmstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_jm_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Jm_HistorySimpan("" & paramSplit(0) & "★M2_Jm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'JM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Jm SET Jmstatus = " & nilaiStatus & ", Jmmodifikasiuser='" & userid & "', Jmmodifikasitgl = NOW(), Jmposting = 0, Jmpostingtgl = '1971-01-01 00:00:00', Jmjmlrevisi = Jmjmlrevisi + 1 WHERE Jmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_JmSearch(PostWsSearch(paramSplit(0), "M2_JmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_JmDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("jmkontakkode", "c1.kkode")
            Filter = Filter.Replace("jmkontaknama", "c1.knama")
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
            Dim sumber As String = "Jm", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Jmid, Jmnotransaksi FROM m2_Jm WHERE Jmid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl"
            sql &= " FROM M2_jm"
            sql &= " WHERE jmid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("jmcabang")
                lokasi = dtNomorNext.Rows(0)("jmlokasi")
                sumber = dtNomorNext.Rows(0)("jmsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("jmautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("jmnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("jmtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'JM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Jm_Detail WHERE idJm = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Jm WHERE Jmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_JmSearch(PostWsSearch(paramSplit(0), "M2_JmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_JmGetdataById(ByVal param As String) As String

        'M2_JmGetdataById Utama --------------------------------------------------------
        'jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, 
        'jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, 
        'jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, 
        'jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, 
        'jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcustomtext1, jmcustomtext2, 
        'jmcustomtext3, jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, 
        'jmcustomdbl2, jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3, jmcabangnama, jmlokasinama, 
        'jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama

        'M2_JmGetdataById Detail -------------------------------------------------------
        'idjmdetail, idjm, kontak
        'norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama
        'kontakkode, kontaknama


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

        Dim NmMemcached As String = "aplikasi1-M2_Jm~M2_Jm_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "jmid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "jmid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_jm_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("jmid"), 0), sptField,
                     FxDB(drutama("jmcabang"), ""), sptField,
                     FxDB(drutama("jmlokasi"), ""), sptField,
                     FxDB(drutama("jmsumber"), ""), sptField,
                     FxDB(drutama("jmautonotransaksi"), 0), sptField,
                     FxDB(drutama("jmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("jmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("jmkodepa"), 0), sptField,
                     FxDB(drutama("jmkontakperson"), ""), sptField,
                     FxDB(drutama("jmuraian"), ""), sptField,
                     FxDB(drutama("jmcatatan"), ""), sptField,
                     FxDB(drutama("jmmatauang"), ""), sptField,
                     FxDB(drutama("jmkurs"), 0), sptField,
                     FxDB(drutama("jmdebit"), 0), sptField,
                     FxDB(drutama("jmdebitvalas"), 0), sptField,
                     FxDB(drutama("jmkredit"), 0), sptField,
                     FxDB(drutama("jmkreditvalas"), 0), sptField,
                     FxDB(drutama("jmjumlahbayar"), 0), sptField,
                     FxDB(drutama("jmjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("jmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("jmstatus"), 0), sptField,
                     FxDB(drutama("jmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("jmjmlrevisi"), 0), sptField,
                     FxDB(drutama("jmcetakanke"), 0), sptField,
                     FxDB(drutama("jmisclose"), 0), sptField,
                     FxDB(drutama("jminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("jmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("jmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("jmcustomtext1"), ""), sptField,
                     FxDB(drutama("jmcustomtext2"), ""), sptField,
                     FxDB(drutama("jmcustomtext3"), ""), sptField,
                     FxDB(drutama("jmcustomtext4"), ""), sptField,
                     FxDB(drutama("jmcustomtext5"), ""), sptField,
                     FxDB(drutama("jmcustomint1"), 0), sptField,
                     FxDB(drutama("jmcustomint2"), 0), sptField,
                     FxDB(drutama("jmcustomint3"), 0), sptField,
                     FxDB(drutama("jmcustomdbl1"), 0), sptField,
                     FxDB(drutama("jmcustomdbl2"), 0), sptField,
                     FxDB(drutama("jmcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("jmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("jmcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("jmcabangnama"), ""), sptField,
                     FxDB(drutama("jmlokasinama"), ""), sptField,
                     FxDB(drutama("jmstatusnama"), ""), sptField,
                     FxDB(drutama("jmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("jminputusernama"), ""), sptField,
                     FxDB(drutama("jmmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idjmdetail"), 0), sptField,
                     FxDB(dr("idjm"), 0), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("debit"), 0), sptField,
                     FxDB(dr("debitvalas"), 0), sptField,
                     FxDB(dr("kredit"), 0), sptField,
                     FxDB(dr("kreditvalas"), 0), sptField,
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
                     FxDB(dr("noreknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcustomtext1, jmcustomtext2, jmcustomtext3, jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, jmcustomdbl2, jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3, jmcabangnama, jmlokasinama, jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama" & sptSubParam & "idjmdetail, idjm, kontak, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama, kontakkode, kontaknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_JmSearch(ByVal param As String) As String
        'M2_JmSearch --------------------------------------------------------
        'jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, 
        'jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, 
        'jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, 
        'jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, 
        'jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcabangnama, jmlokasinama, 
        'jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama

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
            Filter = Filter.Replace("jmkontakkode", "c1.kkode")
            Filter = Filter.Replace("jmkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_jm_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Jm", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("jmid"), 0), sptField,
                     FxDB(dr("jmcabang"), ""), sptField,
                     FxDB(dr("jmlokasi"), ""), sptField,
                     FxDB(dr("jmsumber"), ""), sptField,
                     FxDB(dr("jmautonotransaksi"), 0), sptField,
                     FxDB(dr("jmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("jmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("jmkodepa"), 0), sptField,
                     FxDB(dr("jmkontakperson"), ""), sptField,
                     FxDB(dr("jmuraian"), ""), sptField,
                     FxDB(dr("jmcatatan"), ""), sptField,
                     FxDB(dr("jmmatauang"), ""), sptField,
                     FxDB(dr("jmkurs"), 0), sptField,
                     FxDB(dr("jmdebit"), 0), sptField,
                     FxDB(dr("jmdebitvalas"), 0), sptField,
                     FxDB(dr("jmkredit"), 0), sptField,
                     FxDB(dr("jmkreditvalas"), 0), sptField,
                     FxDB(dr("jmjumlahbayar"), 0), sptField,
                     FxDB(dr("jmjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("jmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jmtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("jmstatus"), 0), sptField,
                     FxDB(dr("jmstatussebelumnya"), 0), sptField,
                     FxDB(dr("jmjmlrevisi"), 0), sptField,
                     FxDB(dr("jmcetakanke"), 0), sptField,
                     FxDB(dr("jmisclose"), 0), sptField,
                     FxDB(dr("jminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jmcabangnama"), ""), sptField,
                     FxDB(dr("jmlokasinama"), ""), sptField,
                     FxDB(dr("jmstatusnama"), ""), sptField,
                     FxDB(dr("jmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("jminputusernama"), ""), sptField,
                     FxDB(dr("jmmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcabangnama, jmlokasinama, jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_JmTerkait(ByVal param As String) As String
        'M2_JmTerkait --------------------------------------------------------
        'jmid, jmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "jmid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_rm_terkait")
        'sql = sql.Replace("validtransaksi", idtransaksi)

        ''BUKA KONEKSI
        'Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'Con1.Open()

        'dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("jmid"), 0), sptField,
                     FxDB(dr("jmnotransaksi"), ""), sptField,
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
            result(2) = "Related JM data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmid, jmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_JmSimpanOld(ByVal param As String) As String
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
        Dim strRekCostCenter As String = ""

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
        'jmid(0) As Integer, jmcabang(1) As String, jmlokasi(2) As String, jmsumber(3) As String, jmautonotransaksi(4) As Integer, 
        'jmnotransaksi(5) As String, jmtgl(6) As Date, jmkodepa(7) As Integer, jmkontakperson(8) As String, jmuraian(9) As String, 
        'jmcatatan(10) As String, jmmatauang(11) As String, jmkurs(12) As Double, jmdebit(13) As Double, jmdebitvalas(14) As Double, 
        'jmkredit(15) As Double, jmkreditvalas(16) As Double, jmjumlahbayar(17) As Double, jmjumlahbayarvalas(18) As Double, jmstatusbayar(19) As Integer, 
        'jmtgllunas(20) As Date, jmstatus(21) As Integer, jmstatussebelumnya(22) As Integer, jmjmlrevisi(23) As Integer, jmcetakanke(24) As Integer, 
        'jmisclose(25) As Integer, jminputuser(26) As Integer, jminputtgl(27) As DateTime, jmmodifikasiuser(28) As Integer, jmmodifikasitgl(29) As DateTime, 
        'jmposting(30) As Integer, jmcustomtext1(31) As String, jmcustomtext2(32) As String, jmcustomtext3(33) As String, jmcustomtext4(34) As String, 
        'jmcustomtext5(35) As String, jmcustomint1(36) As Integer, jmcustomint2(37) As Integer, jmcustomint3(38) As Integer, jmcustomdbl1(39) As Double, 
        'jmcustomdbl2(40) As Double, jmcustomdbl3(41) As Double, jmcustomdate1(42) As Date, jmcustomdate2(43) As Date, jmcustomdate3(44) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, 
        'jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, 
        'jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, 
        'jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, 
        'jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmcustomtext1, jmcustomtext2, jmcustomtext3, 
        'jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, jmcustomdbl2, 
        'jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'jmid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "jmid required numeric." : GoTo selesai
        End If
        'jmautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "jmautonotransaksi required numeric." : GoTo selesai
        End If
        'jmtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "jmtgl required date." : GoTo selesai
        End If
        'jmkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "jmkodepa required numeric." : GoTo selesai
        End If
        'jmkurs(12) As Double
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "jmkurs required numeric." : GoTo selesai
        End If
        'jmdebit(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "jmdebit required numeric." : GoTo selesai
        End If
        'jmdebitvalas(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "jmdebitvalas required numeric." : GoTo selesai
        End If
        'jmkredit(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "jmkredit required numeric." : GoTo selesai
        End If
        'jmkreditvalas(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "jmkreditvalas required numeric." : GoTo selesai
        End If
        'jmjumlahbayar(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "jmjumlahbayar required numeric." : GoTo selesai
        End If
        'jmjumlahbayarvalas(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "jmjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'jmstatusbayar(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "jmstatusbayar required numeric." : GoTo selesai
        End If
        'jmtgllunas(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "jmtgllunas required date." : GoTo selesai
        End If
        'jmstatus(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "jmstatus required numeric." : GoTo selesai
        End If
        'jmstatussebelumnya(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "jmstatussebelumnya required numeric." : GoTo selesai
        End If
        'jmjmlrevisi(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "jmjmlrevisi required numeric." : GoTo selesai
        End If
        'jmcetakanke(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "jmcetakanke required numeric." : GoTo selesai
        End If
        'jmisclose(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "jmisclose required numeric." : GoTo selesai
        End If
        'jminputuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "jminputuser required numeric." : GoTo selesai
        End If
        'jminputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "jminputtgl required date." : GoTo selesai
        End If
        'jmmodifikasiuser(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "jmmodifikasiuser required numeric." : GoTo selesai
        End If
        'jmmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "jmmodifikasitgl required date." : GoTo selesai
        End If
        'jmposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "jmposting required numeric." : GoTo selesai
        End If
        'jmcustomint1(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "jmcustomint1 required numeric." : GoTo selesai
        End If
        'jmcustomint2(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "jmcustomint2 required numeric." : GoTo selesai
        End If
        'jmcustomint3(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "jmcustomint3 required numeric." : GoTo selesai
        End If
        'jmcustomdbl1(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "jmcustomdbl1 required numeric." : GoTo selesai
        End If
        'jmcustomdbl2(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "jmcustomdbl2 required numeric." : GoTo selesai
        End If
        'jmcustomdbl3(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "jmcustomdbl3 required numeric." : GoTo selesai
        End If
        'jmcustomdate1(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "jmcustomdate1 required date." : GoTo selesai
        End If
        'jmcustomdate2(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "jmcustomdate2 required date." : GoTo selesai
        End If
        'jmcustomdate3(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "jmcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'jmcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "jmcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "jmcabang should not be more than 25 character." : GoTo selesai
        End If

        'jmlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "jmlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "jmlokasi should not be more than 25 character." : GoTo selesai
        End If

        'jmsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "jmsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "jmsumber should not be more than 10 character." : GoTo selesai
        End If

        'jmnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "jmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "jmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'jmtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "jmtgl can't be empty" : GoTo selesai
        End If

        'jmmatauang(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "jmmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 25 Then
            result(2) = "jmmatauang should not be more than 25 character." : GoTo selesai
        End If

        'jmkurs(12) As Double
        If Len(dataUtama(12)) = 0 Then
            result(2) = "jmkurs can't be empty" : GoTo selesai
        End If

        'jmdebit(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "jmdebit can't be empty" : GoTo selesai
        End If

        'jmdebitvalas(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "jmdebitvalas can't be empty" : GoTo selesai
        End If

        'jmkredit(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "jmkredit can't be empty" : GoTo selesai
        End If

        'jmkreditvalas(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "jmkreditvalas can't be empty" : GoTo selesai
        End If

        'jmjumlahbayar(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "jmjumlahbayar can't be empty" : GoTo selesai
        End If

        'jmjumlahbayarvalas(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "jmjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'jminputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "jminputtgl can't be empty" : GoTo selesai
        End If

        'jmmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "jmmodifikasitgl can't be empty" : GoTo selesai
        End If

        'jmcustomdbl1(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "jmcustomdbl1 can't be empty" : GoTo selesai
        End If

        'jmcustomdbl2(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "jmcustomdbl2 can't be empty" : GoTo selesai
        End If

        'jmcustomdbl3(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "jmcustomdbl3 can't be empty" : GoTo selesai
        End If

        'jmcustomdate1(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "jmcustomdate1 can't be empty" : GoTo selesai
        End If

        'jmcustomdate2(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "jmcustomdate2 can't be empty" : GoTo selesai
        End If

        'jmcustomdate3(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "jmcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "jmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmdebit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmdebitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmkredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmkreditvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "jmjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "jmcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "jmcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "jmid~jmcabang~jmlokasi~jmsumber~jmautonotransaksi~jmnotransaksi~jmtgl~jmkodepa~jmkontakperson~jmuraian~jmcatatan~jmmatauang~jmkurs~jmdebit~jmdebitvalas~jmkredit~jmkreditvalas~jmjumlahbayar~jmjumlahbayarvalas~jmstatusbayar~jmtgllunas~jmstatus~jmstatussebelumnya~jmjmlrevisi~jmcetakanke~jmisclose~jminputuser~jminputtgl~jmmodifikasiuser~jmmodifikasitgl~jmposting~jmcustomtext1~jmcustomtext2~jmcustomtext3~jmcustomtext4~jmcustomtext5~jmcustomint1~jmcustomint2~jmcustomint3~jmcustomdbl1~jmcustomdbl2~jmcustomdbl3~jmcustomdate1~jmcustomdate2~jmcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idjmdetail(0) As Integer, idjm(1) As Integer, kontak(2) As Integer, norek(3) As String, matauang(4) As String, kurs(5) As Double, 
        'debit(6) As Double, debitvalas(7) As Double, kredit(8) As Double, kreditvalas(9) As Double, catatan(10) As String, 
        'costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, urutan(15) As Integer, 
        'isclose(16) As Integer, customtext1(17) As String, customtext2(18) As String, customtext3(19) As String, customdbl1(20) As Double, 
        'customdbl2(21) As Double, customdbl3(22) As Double, customdate1(23) As Date, customdate2(24) As Date, customdate3(25) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idjmdetail, idjm, kontak, norek, matauang, kurs, debit, debitvalas, 
        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idjmdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idjm", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "debit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "debitvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kredit", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "kreditvalas", AsEnumTypeData.AsDouble)
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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 26) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idjmdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idjmdetail required numeric." : GoTo selesai
            End If
            'idjm(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idjm required numeric." : GoTo selesai
            End If
            'kontak(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If
            If (dataRowDetail(2) < 1) Then
                result(2) = "Row : " & i & " - kontak can't be empty." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'debit(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - debit required numeric." : GoTo selesai
            End If
            'debitvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - debitvalas required numeric." : GoTo selesai
            End If
            'kredit(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - kredit required numeric." : GoTo selesai
            End If
            'kreditvalas(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - kreditvalas required numeric." : GoTo selesai
            End If
            'urutan(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(23) As Date
            If (IsDate(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'norek(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
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

            'debit(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - debit can't be empty" : GoTo selesai
            End If

            'debitvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - debitvalas can't be empty" : GoTo selesai
            End If

            'kredit(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - kredit can't be empty" : GoTo selesai
            End If

            'kreditvalas(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - kreditvalas can't be empty" : GoTo selesai
            End If

            'validasi jumlah debit dan kredit tidak boleh diisi keduanya
            If dataRowDetail(6) = 0 And dataRowDetail(8) = 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be zero" : GoTo selesai
            End If
            If dataRowDetail(6) <> 0 And dataRowDetail(8) <> 0 Then
                result(2) = "Row : " & i & " - debits and credits can't be filled in both" : GoTo selesai
            End If
            If dataRowDetail(7) <> 0 And dataRowDetail(9) <> 0 Then
                result(2) = "Row : " & i & " - foreign debits and credits can't be filled in both" : GoTo selesai
            End If

            'customdbl1(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(23) As Date
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idjmdetail~idjm~kontak~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(cnomor = '" & dataRowDetail(3) & "')")

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("jmtgl")), AsFormatTanggal(drutama("jmtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "jmmatauang", "", dtdetail, "norek")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK COA WAJIB COST CENTER ==============================
                If drutama("jmstatus") = 2 Then
                    Dim cekCoaCostCenter As String = ValidasiCoaRequiredCostCenter(strRekCostCenter, dtdetail)
                    If Len(cekCoaCostCenter) > 0 Then
                        result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF CEK COA WAJIB COST CENTER =======================


                'VALIDASI NOMINAL HARUS SEIMBANG ========================
                Dim debit As Double = 0, kredit As Double = 0, debitvalas As Double = 0, kreditvalas As Double = 0
                debit = AsDataTableDSum(dtdetail, "debit")
                debitvalas = AsDataTableDSum(dtdetail, "debitvalas")
                kredit = AsDataTableDSum(dtdetail, "kredit")
                kreditvalas = AsDataTableDSum(dtdetail, "kreditvalas")

                ''AMBIL SETTING FORMAT NOMINAL
                'Dim digitGroup As String = "", pemisahDesimal As String = "", digitDesimal As Integer = 0
                'Dim FNominal As String = GetSettingNominal(digitGroup, pemisahDesimal, digitDesimal)
                'If len(FNominal) <> 0 Then result(2) = FNominal : Trans.Rollback() : GoTo selesai

                ''BULATKAN NOMINAL DETAIL SESUAI SETTING FORMAT NOMINAL
                'debit = math.round(debit, digitDesimal)
                'debitvalas = math.round(debitvalas, digitDesimal)
                'kredit = math.round(kredit, digitDesimal)
                'kreditvalas = math.round(kreditvalas, digitDesimal)

                'VALIDASI NOMINAL HARUS SEIMBANG
                If debit <> kredit Then
                    result(2) = "Total debits and credits in detail are not balanced." : GoTo selesai
                End If
                If debitvalas <> kreditvalas Then
                    result(2) = "Total foreign debits and credits in detail are not balanced." : GoTo selesai
                End If

                'HITUNG TOTAL TOTAL BERDASARKAN DATA DETAIL
                drutama("jmdebit") = debit
                drutama("jmdebitvalas") = debitvalas
                drutama("jmkredit") = kredit
                drutama("jmkreditvalas") = kreditvalas
                'END OF VALIDASI NOMINAL HARUS SEIMBANG =================


                If isUpdate Then
                    result(4) = drutama("jmid")
                    notransaksi = drutama("jmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(jmid), jmnotransaksi FROM M2_jm WHERE jmid='" & result(4) & "' AND jmstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(jmid) FROM m2_jm WHERE jmnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_jm_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Jm_HistorySimpan("" & paramSplit(0) & "★M2_Jm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("jmsumber")) & "▼" & FixQuotes(drutama("jmid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Jm set jmcabang  = '" & FixQuotes(drutama("jmcabang")) & "', jmlokasi  = '" & FixQuotes(drutama("jmlokasi")) & "', jmsumber  = '" & FixQuotes(drutama("jmsumber")) & "', jmautonotransaksi  = " & drutama("jmautonotransaksi") & ", jmnotransaksi  = '" & notransaksi & "', jmtgl  = '" & FixQuotes(AsFormatTanggal(drutama("jmtgl"))) & "', jmkodepa  = " & drutama("jmkodepa") & ", jmkontakperson  = '" & FixQuotes(drutama("jmkontakperson")) & "', jmuraian  = '" & FixQuotes(drutama("jmuraian")) & "', jmcatatan  = '" & FixQuotes(drutama("jmcatatan")) & "', jmmatauang  = '" & FixQuotes(drutama("jmmatauang")) & "', jmkurs  = '" & FixDouble(drutama("jmkurs")) & "', jmdebit  = '" & FixDouble(drutama("jmdebit")) & "', jmdebitvalas  = '" & FixDouble(drutama("jmdebitvalas")) & "', jmkredit  = '" & FixDouble(drutama("jmkredit")) & "', jmkreditvalas  = '" & FixDouble(drutama("jmkreditvalas")) & "', jmjumlahbayar  = '" & FixDouble(drutama("jmjumlahbayar")) & "', jmjumlahbayarvalas  = '" & FixDouble(drutama("jmjumlahbayarvalas")) & "', jmstatusbayar  = " & drutama("jmstatusbayar") & ", jmtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("jmtgllunas"))) & "', jmstatus  = " & drutama("jmstatus") & ", jmstatussebelumnya  = " & drutama("jmstatussebelumnya") & ", jmjmlrevisi  = jmjmlrevisi+1, jmcetakanke  = " & drutama("jmcetakanke") & ", jmisclose  = " & drutama("jmisclose") & ", jmmodifikasiuser  = " & drutama("jmmodifikasiuser") & ", jmmodifikasitgl  = NOW(), jmposting  = 0, jmcustomtext1  = '" & FixQuotes(drutama("jmcustomtext1")) & "', jmcustomtext2  = '" & FixQuotes(drutama("jmcustomtext2")) & "', jmcustomtext3  = '" & FixQuotes(drutama("jmcustomtext3")) & "', jmcustomtext4  = '" & FixQuotes(drutama("jmcustomtext4")) & "', jmcustomtext5  = '" & FixQuotes(drutama("jmcustomtext5")) & "', jmcustomint1  = " & drutama("jmcustomint1") & ", jmcustomint2  = " & drutama("jmcustomint2") & ", jmcustomint3  = " & drutama("jmcustomint3") & ", jmcustomdbl1  = '" & FixDouble(drutama("jmcustomdbl1")) & "', jmcustomdbl2  = '" & FixDouble(drutama("jmcustomdbl2")) & "', jmcustomdbl3  = '" & FixDouble(drutama("jmcustomdbl3")) & "', jmcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate1"))) & "', jmcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate2"))) & "', jmcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate3"))) & "' where jmid = '" & drutama("jmid") & "'"
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

                    If drutama("jmautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("jmcabang"), drutama("jmlokasi"), drutama("jmsumber"), drutama("jmtgl"))
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
                        notransaksi = drutama("jmnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(jmid) FROM m2_jm WHERE jmnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Jm (jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmcustomtext1, jmcustomtext2, jmcustomtext3, jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, jmcustomdbl2, jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3) values('" & FixQuotes(drutama("jmcabang")) & "', '" & FixQuotes(drutama("jmlokasi")) & "', '" & FixQuotes(drutama("jmsumber")) & "', " & drutama("jmautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("jmtgl"))) & "', " & drutama("jmkodepa") & ", '" & FixQuotes(drutama("jmkontakperson")) & "', '" & FixQuotes(drutama("jmuraian")) & "', '" & FixQuotes(drutama("jmcatatan")) & "', '" & FixQuotes(drutama("jmmatauang")) & "', '" & FixDouble(drutama("jmkurs")) & "', '" & FixDouble(drutama("jmdebit")) & "', '" & FixDouble(drutama("jmdebitvalas")) & "', '" & FixDouble(drutama("jmkredit")) & "', '" & FixDouble(drutama("jmkreditvalas")) & "', '" & FixDouble(drutama("jmjumlahbayar")) & "', '" & FixDouble(drutama("jmjumlahbayarvalas")) & "', " & drutama("jmstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("jmtgllunas"))) & "', " & drutama("jmstatus") & ", " & drutama("jmstatussebelumnya") & ", " & drutama("jmjmlrevisi") & ", " & drutama("jmcetakanke") & ", " & drutama("jmisclose") & ", " & drutama("jminputuser") & ", NOW(), " & drutama("jmmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("jmcustomtext1")) & "', '" & FixQuotes(drutama("jmcustomtext2")) & "', '" & FixQuotes(drutama("jmcustomtext3")) & "', '" & FixQuotes(drutama("jmcustomtext4")) & "', '" & FixQuotes(drutama("jmcustomtext5")) & "', " & drutama("jmcustomint1") & ", " & drutama("jmcustomint2") & ", " & drutama("jmcustomint3") & ", '" & FixDouble(drutama("jmcustomdbl1")) & "', '" & FixDouble(drutama("jmcustomdbl2")) & "', '" & FixDouble(drutama("jmcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("jmcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select jmid from M2_jm where jmnotransaksi='" & notransaksi & "' AND jminputuser= '" & userid & "' order by jmmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Jm_Detail where idjm = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idjmdetail") & ", " & result(4) & ", " & FixQuotes(dr1("kontak")) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Jm_Detail(idjmdetail, idjm, kontak, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "JM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("jmstatus") = 2 Then
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
    Public Function M2_JmUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("jmkontakkode", "c1.kkode")
            Filter = Filter.Replace("jmkontaknama", "c1.knama")
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
            Dim sumber As String = "Jm", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Jmtgl, Jmnotransaksi, Jmstatus FROM m2_Jm WHERE Jmid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Jmstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_jm_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Jm_HistorySimpan("" & paramSplit(0) & "★M2_Jm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'JM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M2_Jm SET Jmstatus = " & nilaiStatus & ", Jmmodifikasiuser='" & userid & "', Jmmodifikasitgl = NOW(), Jmposting = 0, Jmpostingtgl = '1971-01-01 00:00:00', Jmjmlrevisi = Jmjmlrevisi + 1 WHERE Jmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_JmSearch(PostWsSearch(paramSplit(0), "M2_JmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_JmDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("jmkontakkode", "c1.kkode")
            Filter = Filter.Replace("jmkontaknama", "c1.knama")
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
            Dim sumber As String = "Jm", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Jmid, Jmnotransaksi FROM m2_Jm WHERE Jmid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl"
            sql &= " FROM M2_jm"
            sql &= " WHERE jmid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("jmcabang")
                lokasi = dtNomorNext.Rows(0)("jmlokasi")
                sumber = dtNomorNext.Rows(0)("jmsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("jmautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("jmnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("jmtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'JM' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Jm_Detail WHERE idJm = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Jm WHERE Jmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_JmSearch(PostWsSearch(paramSplit(0), "M2_JmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

    'Protected Overrides Sub Finalize()
    '    MyBase.Finalize()
    'End Sub

End Class