Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_bd
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_BdSimpan(ByVal param As String) As String

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
        'bdid(0) As , bdcabang(1) As String, bdlokasi(2) As String, bdsumber(3) As String, bdautonotransaksi(4) As Integer, 
        'bdnotransaksi(5) As String, bdtgl(6) As Date, bdtglanggaran(7) As Date, bdkodepa(8) As , bdkontak(9) As , 
        'bdkontakperson(10) As String, bdanggarankategori(11) As Integer, bdanggarancabang(12) As String, bdanggaranlokasi(13) As String, bdanggarancostcenter(14) As String, 
        'bdanggarandivisi(15) As String, bdanggaransubdivisi(16) As String, bdanggaranproyek(17) As String, bduraian(18) As String, bdcatatan(19) As String, 
        'bdmatauang(20) As String, bdkurs(21) As Double, bdstatus(22) As Integer, bdstatussebelumnya(23) As Integer, bdjmlrevisi(24) As Integer, 
        'bdcetakanke(25) As Integer, bdisclose(26) As Integer, bdinputuser(27) As , bdinputtgl(28) As DateTime, bdmodifikasiuser(29) As , 
        'bdmodifikasitgl(30) As DateTime, bdposting(31) As Integer, bdpostingtgl(32) As DateTime, bdcustomtext1(33) As String, bdcustomtext2(34) As String, 
        'bdcustomtext3(35) As String, bdcustomtext4(36) As String, bdcustomtext5(37) As String, bdcustomint1(38) As Integer, bdcustomint2(39) As Integer, 
        'bdcustomint3(40) As Integer, bdcustomdbl1(41) As Double, bdcustomdbl2(42) As Double, bdcustomdbl3(43) As Double, bdcustomdate1(44) As Date, 
        'bdcustomdate2(45) As Date, bdcustomdate3(46) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, 
        'bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, 
        'bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, 
        'bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, 
        'bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcustomtext1, bdcustomtext2, 
        'bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, 
        'bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 47) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'bdid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bdid required numeric." : GoTo selesai
        End If
        'bdautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "bdautonotransaksi required numeric." : GoTo selesai
        End If
        'bdtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "bdtgl required date." : GoTo selesai
        End If
        'bdtglanggaran(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "bdtglanggaran required date." : GoTo selesai
        End If
        'bdkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "bdkodepa required numeric." : GoTo selesai
        End If
        'bdkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "bdkontak required numeric." : GoTo selesai
        End If
        'bdanggarankategori(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "bdanggarankategori required numeric." : GoTo selesai
        End If
        'bdkurs(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "bdkurs required numeric." : GoTo selesai
        End If
        'bdstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "bdstatus required numeric." : GoTo selesai
        End If
        'bdstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "bdstatussebelumnya required numeric." : GoTo selesai
        End If
        'bdjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "bdjmlrevisi required numeric." : GoTo selesai
        End If
        'bdcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bdcetakanke required numeric." : GoTo selesai
        End If
        'bdisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "bdisclose required numeric." : GoTo selesai
        End If
        'bdinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bdinputuser required numeric." : GoTo selesai
        End If
        'bdinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "bdinputtgl required date." : GoTo selesai
        End If
        'bdmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bdmodifikasiuser required numeric." : GoTo selesai
        End If
        'bdmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "bdmodifikasitgl required date." : GoTo selesai
        End If
        'bdposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bdposting required numeric." : GoTo selesai
        End If
        'bdpostingtgl(32) As DateTime
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "bdpostingtgl required date." : GoTo selesai
        End If
        'bdcustomint1(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bdcustomint1 required numeric." : GoTo selesai
        End If
        'bdcustomint2(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bdcustomint2 required numeric." : GoTo selesai
        End If
        'bdcustomint3(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "bdcustomint3 required numeric." : GoTo selesai
        End If
        'bdcustomdbl1(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "bdcustomdbl1 required numeric." : GoTo selesai
        End If
        'bdcustomdbl2(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "bdcustomdbl2 required numeric." : GoTo selesai
        End If
        'bdcustomdbl3(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "bdcustomdbl3 required numeric." : GoTo selesai
        End If
        'bdcustomdate1(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "bdcustomdate1 required date." : GoTo selesai
        End If
        'bdcustomdate2(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "bdcustomdate2 required date." : GoTo selesai
        End If
        'bdcustomdate3(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "bdcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'bdcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bdcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bdcabang should not be more than 25 character." : GoTo selesai
        End If

        'bdlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bdlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bdlokasi should not be more than 25 character." : GoTo selesai
        End If

        'bdsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "bdsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "bdsumber should not be more than 10 character." : GoTo selesai
        End If

        'bdnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "bdnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "bdnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bdtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "bdtgl can't be empty" : GoTo selesai
        End If

        'bdtglanggaran(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "bdtglanggaran can't be empty" : GoTo selesai
        End If

        'bdanggarancabang(12) As String
        If dataUtama(11) = 1 And Len(dataUtama(12)) = 0 Then
            result(2) = "bdanggarancabang can't be empty" : GoTo selesai
        End If

        'bdanggaranlokasi(13) As String
        If dataUtama(11) = 2 And Len(dataUtama(13)) = 0 Then
            result(2) = "bdanggaranlokasi can't be empty" : GoTo selesai
        End If

        'bdanggarancostcenter(14) As String
        If dataUtama(11) = 3 And Len(dataUtama(14)) = 0 Then
            result(2) = "bdanggarancostcenter can't be empty" : GoTo selesai
        End If

        'bdanggarandivisi(15) As String
        If dataUtama(11) = 4 And Len(dataUtama(15)) = 0 Then
            result(2) = "bdanggarandivisi can't be empty" : GoTo selesai
        End If

        'bdanggaransubdivisi(16) As String
        If dataUtama(11) = 5 And Len(dataUtama(16)) = 0 Then
            result(2) = "bdanggaransubdivisi can't be empty" : GoTo selesai
        End If

        'bdanggaranproyek(17) As String
        If dataUtama(11) = 6 And Len(dataUtama(17)) = 0 Then
            result(2) = "bdanggaranproyek can't be empty" : GoTo selesai
        End If

        'bdmatauang(20) As String
        If Len(dataUtama(20)) = 0 Then
            result(2) = "bdmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(20)) > 25 Then
            result(2) = "bdmatauang should not be more than 25 character." : GoTo selesai
        End If

        'bdkurs(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "bdkurs can't be empty" : GoTo selesai
        End If

        'bdinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "bdinputtgl can't be empty" : GoTo selesai
        End If

        'bdmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "bdmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bdpostingtgl(32) As DateTime
        If Len(dataUtama(32)) = 0 Then
            result(2) = "bdpostingtgl can't be empty" : GoTo selesai
        End If

        'bdcustomdbl1(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "bdcustomdbl1 can't be empty" : GoTo selesai
        End If

        'bdcustomdbl2(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "bdcustomdbl2 can't be empty" : GoTo selesai
        End If

        'bdcustomdbl3(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "bdcustomdbl3 can't be empty" : GoTo selesai
        End If

        'bdcustomdate1(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "bdcustomdate1 can't be empty" : GoTo selesai
        End If

        'bdcustomdate2(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "bdcustomdate2 can't be empty" : GoTo selesai
        End If

        'bdcustomdate3(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "bdcustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bdid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdtglanggaran", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggarankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdanggarancabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggaranlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggarancostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggarandivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggaransubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggaranproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bduraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "bdid~bdcabang~bdlokasi~bdsumber~bdautonotransaksi~bdnotransaksi~bdtgl~bdtglanggaran~bdkodepa~bdkontak~bdkontakperson~bdanggarankategori~bdanggarancabang~bdanggaranlokasi~bdanggarancostcenter~bdanggarandivisi~bdanggaransubdivisi~bdanggaranproyek~bduraian~bdcatatan~bdmatauang~bdkurs~bdstatus~bdstatussebelumnya~bdjmlrevisi~bdcetakanke~bdisclose~bdinputuser~bdinputtgl~bdmodifikasiuser~bdmodifikasitgl~bdposting~bdpostingtgl~bdcustomtext1~bdcustomtext2~bdcustomtext3~bdcustomtext4~bdcustomtext5~bdcustomint1~bdcustomint2~bdcustomint3~bdcustomdbl1~bdcustomdbl2~bdcustomdbl3~bdcustomdate1~bdcustomdate2~bdcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idbddetail(0) As Integer, idbd(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idbddetail, idbd, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbddetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsString)
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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idbddetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idbddetail required numeric." : GoTo selesai
            End If
            'idbd(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idbd required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jumlahvalas required numeric." : GoTo selesai
            End If
            'urutan(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'norek(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
            End If

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

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'customdbl1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idbddetail~idbd~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 135
                Select Case drutama("bdstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("bdtgl")), AsFormatTanggal(drutama("bdtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("bdid")
                    notransaksi = drutama("bdnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(bdid), bdnotransaksi FROM M2_Bd WHERE bdid='" & result(4) & "' AND bdstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("bdautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bdcabang"), drutama("bdlokasi"), drutama("bdsumber"), drutama("bdtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bdid) FROM m2_bd WHERE bdnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============


                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_bd_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Bd_HistorySimpan("" & paramSplit(0) & "★M2_Bd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bdsumber")) & "▼" & FixQuotes(drutama("bdid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================


                        sql = "Update M2_Bd set bdcabang  = '" & FixQuotes(drutama("bdcabang")) & "', bdlokasi  = '" & FixQuotes(drutama("bdlokasi")) & "', bdsumber  = '" & FixQuotes(drutama("bdsumber")) & "', bdautonotransaksi  = " & drutama("bdautonotransaksi") & ", bdnotransaksi  = '" & notransaksi & "', bdtgl  = '" & FixQuotes(AsFormatTanggal(drutama("bdtgl"))) & "', bdtglanggaran  = '" & FixQuotes(AsFormatTanggal(drutama("bdtglanggaran"))) & "', bdkodepa  = " & drutama("bdkodepa") & ", bdkontak  = " & drutama("bdkontak") & ", bdkontakperson  = '" & FixQuotes(drutama("bdkontakperson")) & "', bdanggarankategori  = " & drutama("bdanggarankategori") & ", bdanggarancabang  = '" & FixQuotes(drutama("bdanggarancabang")) & "', bdanggaranlokasi  = '" & FixQuotes(drutama("bdanggaranlokasi")) & "', bdanggarancostcenter  = '" & FixQuotes(drutama("bdanggarancostcenter")) & "', bdanggarandivisi  = '" & FixQuotes(drutama("bdanggarandivisi")) & "', bdanggaransubdivisi  = '" & FixQuotes(drutama("bdanggaransubdivisi")) & "', bdanggaranproyek  = '" & FixQuotes(drutama("bdanggaranproyek")) & "', bduraian  = '" & FixQuotes(drutama("bduraian")) & "', bdcatatan  = '" & FixQuotes(drutama("bdcatatan")) & "', bdmatauang  = '" & FixQuotes(drutama("bdmatauang")) & "', bdkurs  = '" & FixDouble(drutama("bdkurs")) & "', bdstatus  = " & drutama("bdstatus") & ", bdstatussebelumnya  = " & drutama("bdstatussebelumnya") & ", bdjmlrevisi  = bdjmlrevisi+1, bdcetakanke  = " & drutama("bdcetakanke") & ", bdisclose  = " & drutama("bdisclose") & ", bdmodifikasiuser  = " & drutama("bdmodifikasiuser") & ", bdmodifikasitgl  = NOW(), bdposting  = 0, bdcustomtext1  = '" & FixQuotes(drutama("bdcustomtext1")) & "', bdcustomtext2  = '" & FixQuotes(drutama("bdcustomtext2")) & "', bdcustomtext3  = '" & FixQuotes(drutama("bdcustomtext3")) & "', bdcustomtext4  = '" & FixQuotes(drutama("bdcustomtext4")) & "', bdcustomtext5  = '" & FixQuotes(drutama("bdcustomtext5")) & "', bdcustomint1  = " & drutama("bdcustomint1") & ", bdcustomint2  = " & drutama("bdcustomint2") & ", bdcustomint3  = " & drutama("bdcustomint3") & ", bdcustomdbl1  = '" & FixDouble(drutama("bdcustomdbl1")) & "', bdcustomdbl2  = '" & FixDouble(drutama("bdcustomdbl2")) & "', bdcustomdbl3  = '" & FixDouble(drutama("bdcustomdbl3")) & "', bdcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate1"))) & "', bdcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate2"))) & "', bdcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate3"))) & "' where bdid = '" & drutama("bdid") & "'"
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

                    If drutama("bdautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bdcabang"), drutama("bdlokasi"), drutama("bdsumber"), drutama("bdtgl"))
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
                        notransaksi = drutama("bdnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bdid) FROM m2_bd WHERE bdnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Bd (bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdcustomtext1, bdcustomtext2, bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3) values('" & FixQuotes(drutama("bdcabang")) & "', '" & FixQuotes(drutama("bdlokasi")) & "', '" & FixQuotes(drutama("bdsumber")) & "', " & drutama("bdautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("bdtgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdtglanggaran"))) & "', " & drutama("bdkodepa") & ", " & drutama("bdkontak") & ", '" & FixQuotes(drutama("bdkontakperson")) & "', " & drutama("bdanggarankategori") & ", '" & FixQuotes(drutama("bdanggarancabang")) & "', '" & FixQuotes(drutama("bdanggaranlokasi")) & "', '" & FixQuotes(drutama("bdanggarancostcenter")) & "', '" & FixQuotes(drutama("bdanggarandivisi")) & "', '" & FixQuotes(drutama("bdanggaransubdivisi")) & "', '" & FixQuotes(drutama("bdanggaranproyek")) & "', '" & FixQuotes(drutama("bduraian")) & "', '" & FixQuotes(drutama("bdcatatan")) & "', '" & FixQuotes(drutama("bdmatauang")) & "', '" & FixDouble(drutama("bdkurs")) & "', " & drutama("bdstatus") & ", " & drutama("bdstatussebelumnya") & ", " & drutama("bdjmlrevisi") & ", " & drutama("bdcetakanke") & ", " & drutama("bdisclose") & ", " & drutama("bdinputuser") & ", NOW(), " & drutama("bdmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("bdcustomtext1")) & "', '" & FixQuotes(drutama("bdcustomtext2")) & "', '" & FixQuotes(drutama("bdcustomtext3")) & "', '" & FixQuotes(drutama("bdcustomtext4")) & "', '" & FixQuotes(drutama("bdcustomtext5")) & "', " & drutama("bdcustomint1") & ", " & drutama("bdcustomint2") & ", " & drutama("bdcustomint3") & ", '" & FixDouble(drutama("bdcustomdbl1")) & "', '" & FixDouble(drutama("bdcustomdbl2")) & "', '" & FixDouble(drutama("bdcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select bdid from M2_Bd where bdnotransaksi='" & notransaksi & "' AND Bdinputuser= '" & userid & "' order by Bdmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai

                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Bd_Detail where idbd = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idbddetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Bd_Detail(idbddetail, idbd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'UPDATE ANGGARAN ====================================================================
                If drutama("bdstatus") = 2 Then
                    'UPDATE ANGGARAN SESUAI KATEGORI
                    If drutama("bdanggarankategori") = 0 Then
                        'ANGGARAN GLOBAL
                        sql = "INSERT INTO m2_realization ( SELECT YEAR(bd.bdtglanggaran) as rtahun, MONTH(bd.bdtglanggaran) as rbulan, bdd.norek as rnorek, 0 as rjmldebit, 0 as rjmlkredit, bdd.jumlah as ranggaran, ap.apkode as rkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE ranggaran = VALUES(ranggaran)"

                    ElseIf drutama("bdanggarankategori") = 1 Then
                        'ANGGARAN CABANG
                        sql = "INSERT INTO m2_realization_branch ( SELECT YEAR(bd.bdtglanggaran) as rwtahun, MONTH(bd.bdtglanggaran) as rwbulan, bd.bdanggarancabang as rwcabang, bdd.norek as rwnorek, 0 as rwjmldebit, 0 as rwjmlkredit, bdd.jumlah as rwanggaran, ap.apkode as rwkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rwanggaran = VALUES(rwanggaran)"

                    ElseIf drutama("bdanggarankategori") = 2 Then
                        'ANGGARAN LOKASI
                        sql = "INSERT INTO m2_realization_location ( SELECT YEAR(bd.bdtglanggaran) as rltahun, MONTH(bd.bdtglanggaran) as rlbulan, bd.bdanggaranlokasi as rllokasi, bdd.norek as rlnorek, 0 as rljmldebit, 0 as rljmlkredit, bdd.jumlah as rlanggaran, ap.apkode as rlkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rlanggaran = VALUES(rlanggaran)"

                    ElseIf drutama("bdanggarankategori") = 3 Then
                        'ANGGARAN COSTCENTER
                        sql = "INSERT INTO m2_realization_cost_center ( SELECT YEAR(bd.bdtglanggaran) as rcctahun, MONTH(bd.bdtglanggaran) as rccbulan, bd.bdanggarancostcenter as rcccostcenter, bdd.norek as rccnorek, 0 as rccjmldebit, 0 as rccjmlkredit, bdd.jumlah as rccanggaran, ap.apkode as rcckodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rccanggaran = VALUES(rccanggaran)"

                    ElseIf drutama("bdanggarankategori") = 4 Then
                        'ANGGARAN DIVISI
                        sql = "INSERT INTO m2_realization_division ( SELECT YEAR(bd.bdtglanggaran) as rdtahun, MONTH(bd.bdtglanggaran) as rdbulan, bd.bdanggarandivisi as rddivisi, bdd.norek as rdnorek, 0 as rdjmldebit, 0 as rdjmlkredit, bdd.jumlah as rdanggaran, ap.apkode as rdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rdanggaran = VALUES(rdanggaran)"

                    ElseIf drutama("bdanggarankategori") = 5 Then
                        'ANGGARAN SUBDIVISI
                        sql = "INSERT INTO m2_realization_subdivision ( SELECT YEAR(bd.bdtglanggaran) as rsdtahun, MONTH(bd.bdtglanggaran) as rsdbulan, bd.bdanggaransubdivisi as rsdsubdivisi, bdd.norek as rsdnorek, 0 as rsdjmldebit, 0 as rsdjmlkredit, bdd.jumlah as rsdanggaran, ap.apkode as rsdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rsdanggaran = VALUES(rsdanggaran)"

                    ElseIf drutama("bdanggarankategori") = 6 Then
                        'ANGGARAN PROYEK
                        sql = "INSERT INTO m2_realization_project ( SELECT YEAR(bd.bdtglanggaran) as rptahun, MONTH(bd.bdtglanggaran) as rpbulan, bd.bdanggaranproyek as rpproyek, bdd.norek as rpnorek, 0 as rpjmldebit, 0 as rpjmlkredit, bdd.jumlah as rpanggaran, ap.apkode as rpkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rpanggaran = VALUES(rpanggaran)"

                    End If

                    'EKSEKUSI QUERY UPDATE ANGGARAN
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

                End If
                'END OF UPDATE ANGGARAN =============================================================


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "BD", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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
    Public Function M2_BdUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("bdkontakkode", "c.kkode")
            Filter = Filter.Replace("bdkontaknama", "c.knama")
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
            Dim sumber As String = "Bd", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bdtgl, Bdnotransaksi, Bdstatus FROM m2_Bd WHERE Bdid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bdstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_bd_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Bd_HistorySimpan("" & paramSplit(0) & "★M2_Bd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then

                'UPDATE ANGGARAN ==================================================
                sql = "SELECT bdanggarankategori FROM m2_bd bd WHERE bd.bdid = '" & idtransaksi & "'"
                Dim dtTrans As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtTrans.Rows.Count > 0 Then
                    Dim anggarankategori As Double = Double.Parse(dtTrans.Rows(0)("bdanggarankategori"))

                    'UPDATE ANGGARAN SESUAI KATEGORI
                    If anggarankategori = 0 Then
                        'ANGGARAN GLOBAL
                        sql = "INSERT INTO m2_realization ( SELECT YEAR(bd.bdtglanggaran) as rtahun, MONTH(bd.bdtglanggaran) as rbulan, bdd.norek as rnorek, 0 as rjmldebit, 0 as rjmlkredit, 0 as ranggaran, ap.apkode as rkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE ranggaran = VALUES(ranggaran)"

                    ElseIf anggarankategori = 1 Then
                        'ANGGARAN CABANG
                        sql = "INSERT INTO m2_realization_branch ( SELECT YEAR(bd.bdtglanggaran) as rwtahun, MONTH(bd.bdtglanggaran) as rwbulan, bd.bdanggarancabang as rwcabang, bdd.norek as rwnorek, 0 as rwjmldebit, 0 as rwjmlkredit, 0 as rwanggaran, ap.apkode as rwkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rwanggaran = VALUES(rwanggaran)"

                    ElseIf anggarankategori = 2 Then
                        'ANGGARAN LOKASI
                        sql = "INSERT INTO m2_realization_location ( SELECT YEAR(bd.bdtglanggaran) as rltahun, MONTH(bd.bdtglanggaran) as rlbulan, bd.bdanggaranlokasi as rllokasi, bdd.norek as rlnorek, 0 as rljmldebit, 0 as rljmlkredit, 0 as rlanggaran, ap.apkode as rlkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rlanggaran = VALUES(rlanggaran)"

                    ElseIf anggarankategori = 3 Then
                        'ANGGARAN COSTCENTER
                        sql = "INSERT INTO m2_realization_cost_center ( SELECT YEAR(bd.bdtglanggaran) as rcctahun, MONTH(bd.bdtglanggaran) as rccbulan, bd.bdanggarancostcenter as rcccostcenter, bdd.norek as rccnorek, 0 as rccjmldebit, 0 as rccjmlkredit, 0 as rccanggaran, ap.apkode as rcckodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rccanggaran = VALUES(rccanggaran)"

                    ElseIf anggarankategori = 4 Then
                        'ANGGARAN DIVISI
                        sql = "INSERT INTO m2_realization_division ( SELECT YEAR(bd.bdtglanggaran) as rdtahun, MONTH(bd.bdtglanggaran) as rdbulan, bd.bdanggarandivisi as rddivisi, bdd.norek as rdnorek, 0 as rdjmldebit, 0 as rdjmlkredit, 0 as rdanggaran, ap.apkode as rdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rdanggaran = VALUES(rdanggaran)"

                    ElseIf anggarankategori = 5 Then
                        'ANGGARAN SUBDIVISI
                        sql = "INSERT INTO m2_realization_subdivision ( SELECT YEAR(bd.bdtglanggaran) as rsdtahun, MONTH(bd.bdtglanggaran) as rsdbulan, bd.bdanggaransubdivisi as rsdsubdivisi, bdd.norek as rsdnorek, 0 as rsdjmldebit, 0 as rsdjmlkredit, 0 as rsdanggaran, ap.apkode as rsdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rsdanggaran = VALUES(rsdanggaran)"

                    ElseIf anggarankategori = 6 Then
                        'ANGGARAN PROYEK
                        sql = "INSERT INTO m2_realization_project ( SELECT YEAR(bd.bdtglanggaran) as rptahun, MONTH(bd.bdtglanggaran) as rpbulan, bd.bdanggaranproyek as rpproyek, bdd.norek as rpnorek, 0 as rpjmldebit, 0 as rpjmlkredit, 0 as rpanggaran, ap.apkode as rpkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rpanggaran = VALUES(rpanggaran)"

                    End If

                    'EKSEKUSI QUERY UPDATE ANGGARAN
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

                End If
                'END OF UPDATE ANGGARAN ===========================================

            End If


            'update status utama
            sql = "UPDATE M2_Bd SET Bdstatus = " & nilaiStatus & ", bdmodifikasiuser='" & userid & "', bdmodifikasitgl = NOW(), bdposting = 0, bdpostingtgl = '1971-01-01 00:00:00', Bdjmlrevisi = Bdjmlrevisi + 1 WHERE bdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_BdSearch(PostWsSearch(paramSplit(0), "M2_BdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_BdDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("bdkontakkode", "c.kkode")
            Filter = Filter.Replace("bdkontaknama", "c.knama")
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
            Dim sumber As String = "BD", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT bdid, bdnotransaksi FROM m2_bd WHERE bdid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl"
            sql &= " FROM M2_bd"
            sql &= " WHERE bdid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bdcabang")
                lokasi = dtNomorNext.Rows(0)("bdlokasi")
                sumber = dtNomorNext.Rows(0)("bdsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("bdautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("bdnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bdtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M2_Bd_Detail WHERE idbd = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Bd WHERE bdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_BdSearch(PostWsSearch(paramSplit(0), "M2_BdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_BdGetdataById(ByVal param As String) As String

        'M2_BdGetdataById Utama --------------------------------------------------------
        'bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, 
        'bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, 
        'bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, 
        'bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, 
        'bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcustomtext1, bdcustomtext2, 
        'bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, 
        'bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3, bdcabangnama, bdlokasinama, 
        'bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, 
        'bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama

        'M2_BdGetdataById Detail -------------------------------------------------------
        'idbddetail, 
        'idbd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, 
        'costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, noreknama

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


        Dim NmMemcached As String = "aplikasi1-M2_Bd~M2_Bd_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "bdid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "bdid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_bd_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "select `bd`.`bdid` AS `bdid`,`bd`.`bdcabang` AS `bdcabang`,`bd`.`bdlokasi` AS `bdlokasi`,`bd`.`bdsumber` AS `bdsumber`,`bd`.`bdautonotransaksi` AS `bdautonotransaksi`,`bd`.`bdnotransaksi` AS `bdnotransaksi`,`bd`.`bdtgl` AS `bdtgl`,`bd`.`bdtglanggaran` AS `bdtglanggaran`,`bd`.`bdkodepa` AS `bdkodepa`,`bd`.`bdkontak` AS `bdkontak`,`bd`.`bdkontakperson` AS `bdkontakperson`,`bd`.`bdanggarankategori` AS `bdanggarankategori`,`bd`.`bdanggarancabang` AS `bdanggarancabang`,`bd`.`bdanggaranlokasi` AS `bdanggaranlokasi`,`bd`.`bdanggarancostcenter` AS `bdanggarancostcenter`,`bd`.`bdanggarandivisi` AS `bdanggarandivisi`,`bd`.`bdanggaransubdivisi` AS `bdanggaransubdivisi`,`bd`.`bdanggaranproyek` AS `bdanggaranproyek`,`bd`.`bduraian` AS `bduraian`,`bd`.`bdcatatan` AS `bdcatatan`,`bd`.`bdmatauang` AS `bdmatauang`,`bd`.`bdkurs` AS `bdkurs`,`bd`.`bdstatus` AS `bdstatus`,`bd`.`bdstatussebelumnya` AS `bdstatussebelumnya`,`bd`.`bdjmlrevisi` AS `bdjmlrevisi`,`bd`.`bdcetakanke` AS `bdcetakanke`,`bd`.`bdisclose` AS `bdisclose`,`bd`.`bdinputuser` AS `bdinputuser`,`bd`.`bdinputtgl` AS `bdinputtgl`,`bd`.`bdmodifikasiuser` AS `bdmodifikasiuser`,`bd`.`bdmodifikasitgl` AS `bdmodifikasitgl`,`bd`.`bdposting` AS `bdposting`,`bd`.`bdpostingtgl` AS `bdpostingtgl`,`bd`.`bdcustomtext1` AS `bdcustomtext1`,`bd`.`bdcustomtext2` AS `bdcustomtext2`,`bd`.`bdcustomtext3` AS `bdcustomtext3`,`bd`.`bdcustomtext4` AS `bdcustomtext4`,`bd`.`bdcustomtext5` AS `bdcustomtext5`,`bd`.`bdcustomint1` AS `bdcustomint1`,`bd`.`bdcustomint2` AS `bdcustomint2`,`bd`.`bdcustomint3` AS `bdcustomint3`,`bd`.`bdcustomdbl1` AS `bdcustomdbl1`,`bd`.`bdcustomdbl2` AS `bdcustomdbl2`,`bd`.`bdcustomdbl3` AS `bdcustomdbl3`,`bd`.`bdcustomdate1` AS `bdcustomdate1`,`bd`.`bdcustomdate2` AS `bdcustomdate2`,`bd`.`bdcustomdate3` AS `bdcustomdate3`,`br`.`bnama` AS `bdcabangnama`,`lc`.`lnama` AS `bdlokasinama`,`c`.`kkode` AS `bdkontakkode`,`c`.`knama` AS `bdkontaknama`,`rc`.`nama` AS `bdanggarankategorinama`,`br2`.`bnama` AS `bdanggarancabangnama`,`lc2`.`lnama` AS `bdanggaranlokasinama`,`cc2`.`ccnama` AS `bdanggarancostcenternama`,`d2`.`dnama` AS `bdanggarandivisinama`,`sd2`.`sdnama` AS `bdanggaransubdivisinama`,`p2`.`pnama` AS `bdanggaranproyeknama`,`st1`.`nama` AS `bdstatusnama`,`st2`.`nama` AS `bdstatussebelumnyanama`,`u1`.`unama` AS `bdinputusernama`,`u2`.`unama` AS `bdmodifikasiusernama`,`bdd`.`idbddetail` AS `idbddetail`,`bdd`.`idbd` AS `idbd`,`bdd`.`norek` AS `norek`,`bdd`.`matauang` AS `matauang`,`bdd`.`kurs` AS `kurs`,`bdd`.`jumlah` AS `jumlah`,`bdd`.`jumlahvalas` AS `jumlahvalas`,`bdd`.`catatan` AS `catatan`,`bdd`.`costcenter` AS `costcenter`,`bdd`.`divisi` AS `divisi`,`bdd`.`subdivisi` AS `subdivisi`,`bdd`.`proyek` AS `proyek`,`bdd`.`urutan` AS `urutan`,`bdd`.`isclose` AS `isclose`,`bdd`.`customtext1` AS `customtext1`,`bdd`.`customtext2` AS `customtext2`,`bdd`.`customtext3` AS `customtext3`,`bdd`.`customdbl1` AS `customdbl1`,`bdd`.`customdbl2` AS `customdbl2`,`bdd`.`customdbl3` AS `customdbl3`,`bdd`.`customdate1` AS `customdate1`,`bdd`.`customdate2` AS `customdate2`,`bdd`.`customdate3` AS `customdate3`,`coa`.`cnama` AS `noreknama` from ((((((((((((((((`m2_bd` `bd` join `m0_status` `st1` on((`bd`.`bdstatus` = `st1`.`kode`))) join `m0_status` `st2` on((`bd`.`bdstatussebelumnya` = `st2`.`kode`))) join `m0_realization_category` `rc` on((`bd`.`bdanggarankategori` = `rc`.`kode`))) join `m2_bd_detail` `bdd` on((`bd`.`bdid` = `bdd`.`idbd`))) left join `m1_branch` `br` on((`bd`.`bdcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bd`.`bdlokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bd`.`bdkontak` = `c`.`kid`))) left join `m1_branch` `br2` on((`bd`.`bdanggarancabang` = `br2`.`bkode`))) left join `m1_location` `lc2` on((`bd`.`bdanggaranlokasi` = `lc2`.`lkode`))) left join `m1_cost_center` `cc2` on((`bd`.`bdanggarancostcenter` = `cc2`.`cckode`))) left join `m1_division` `d2` on((`bd`.`bdanggarandivisi` = `d2`.`dkode`))) left join `m1_subdivision` `sd2` on((`bd`.`bdanggaransubdivisi` = `sd2`.`sdkode`))) left join `m1_project` `p2` on((`bd`.`bdanggaranproyek` = `p2`.`pkode`))) left join `m0_user` `u1` on((`bd`.`bdinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bd`.`bdmodifikasiuser` = `u2`.`userid`))) left join `m1_coa` `coa` on((`bdd`.`norek` = `coa`.`cnomor`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("bdid"), ""), sptField,
                     FxDB(drutama("bdcabang"), ""), sptField,
                     FxDB(drutama("bdlokasi"), ""), sptField,
                     FxDB(drutama("bdsumber"), ""), sptField,
                     FxDB(drutama("bdautonotransaksi"), 0), sptField,
                     FxDB(drutama("bdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bdtgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bdtglanggaran"), ""), formatTgl), sptField,
                     FxDB(drutama("bdkodepa"), ""), sptField,
                     FxDB(drutama("bdkontak"), ""), sptField,
                     FxDB(drutama("bdkontakperson"), ""), sptField,
                     FxDB(drutama("bdanggarankategori"), 0), sptField,
                     FxDB(drutama("bdanggarancabang"), ""), sptField,
                     FxDB(drutama("bdanggaranlokasi"), ""), sptField,
                     FxDB(drutama("bdanggarancostcenter"), ""), sptField,
                     FxDB(drutama("bdanggarandivisi"), ""), sptField,
                     FxDB(drutama("bdanggaransubdivisi"), ""), sptField,
                     FxDB(drutama("bdanggaranproyek"), ""), sptField,
                     FxDB(drutama("bduraian"), ""), sptField,
                     FxDB(drutama("bdcatatan"), ""), sptField,
                     FxDB(drutama("bdmatauang"), ""), sptField,
                     FxDB(drutama("bdkurs"), 0), sptField,
                     FxDB(drutama("bdstatus"), 0), sptField,
                     FxDB(drutama("bdstatussebelumnya"), 0), sptField,
                     FxDB(drutama("bdjmlrevisi"), 0), sptField,
                     FxDB(drutama("bdcetakanke"), 0), sptField,
                     FxDB(drutama("bdisclose"), 0), sptField,
                     FxDB(drutama("bdinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bdmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bdcustomtext1"), ""), sptField,
                     FxDB(drutama("bdcustomtext2"), ""), sptField,
                     FxDB(drutama("bdcustomtext3"), ""), sptField,
                     FxDB(drutama("bdcustomtext4"), ""), sptField,
                     FxDB(drutama("bdcustomtext5"), ""), sptField,
                     FxDB(drutama("bdcustomint1"), 0), sptField,
                     FxDB(drutama("bdcustomint2"), 0), sptField,
                     FxDB(drutama("bdcustomint3"), 0), sptField,
                     FxDB(drutama("bdcustomdbl1"), 0), sptField,
                     FxDB(drutama("bdcustomdbl2"), 0), sptField,
                     FxDB(drutama("bdcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bdcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bdcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bdcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bdcabangnama"), ""), sptField,
                     FxDB(drutama("bdlokasinama"), ""), sptField,
                     FxDB(drutama("bdkontakkode"), ""), sptField,
                     FxDB(drutama("bdkontaknama"), ""), sptField,
                     FxDB(drutama("bdanggarankategorinama"), ""), sptField,
                     FxDB(drutama("bdanggarancabangnama"), ""), sptField,
                     FxDB(drutama("bdanggaranlokasinama"), ""), sptField,
                     FxDB(drutama("bdanggarancostcenternama"), ""), sptField,
                     FxDB(drutama("bdanggarandivisinama"), ""), sptField,
                     FxDB(drutama("bdanggaransubdivisinama"), ""), sptField,
                     FxDB(drutama("bdanggaranproyeknama"), ""), sptField,
                     FxDB(drutama("bdstatusnama"), ""), sptField,
                     FxDB(drutama("bdstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("bdinputusernama"), ""), sptField,
                     FxDB(drutama("bdmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idbddetail"), ""), sptField,
                     FxDB(dr("idbd"), ""), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
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
                     FxDB(dr("noreknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcustomtext1, bdcustomtext2, bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3, bdcabangnama, bdlokasinama, bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama" & sptSubParam & "idbddetail, idbd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_BdSearch(ByVal param As String) As String
        'M2_BdSearch --------------------------------------------------------
        'bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, 
        'bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, 
        'bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, 
        'bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, 
        'bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcabangnama, bdlokasinama, 
        'bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, 
        'bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama

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
            Filter = Filter.Replace("bdkontakkode", "c.kkode")
            Filter = Filter.Replace("bdkontaknama", "c.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m2_bd_v")

        sql = "select `bd`.`bdid` AS `bdid`,`bd`.`bdcabang` AS `bdcabang`,`bd`.`bdlokasi` AS `bdlokasi`,`bd`.`bdsumber` AS `bdsumber`,`bd`.`bdautonotransaksi` AS `bdautonotransaksi`,`bd`.`bdnotransaksi` AS `bdnotransaksi`,`bd`.`bdtgl` AS `bdtgl`,`bd`.`bdtglanggaran` AS `bdtglanggaran`,`bd`.`bdkodepa` AS `bdkodepa`,`bd`.`bdkontak` AS `bdkontak`,`bd`.`bdkontakperson` AS `bdkontakperson`,`bd`.`bdanggarankategori` AS `bdanggarankategori`,`bd`.`bdanggarancabang` AS `bdanggarancabang`,`bd`.`bdanggaranlokasi` AS `bdanggaranlokasi`,`bd`.`bdanggarancostcenter` AS `bdanggarancostcenter`,`bd`.`bdanggarandivisi` AS `bdanggarandivisi`,`bd`.`bdanggaransubdivisi` AS `bdanggaransubdivisi`,`bd`.`bdanggaranproyek` AS `bdanggaranproyek`,`bd`.`bduraian` AS `bduraian`,`bd`.`bdcatatan` AS `bdcatatan`,`bd`.`bdmatauang` AS `bdmatauang`,`bd`.`bdkurs` AS `bdkurs`,`bd`.`bdstatus` AS `bdstatus`,`bd`.`bdstatussebelumnya` AS `bdstatussebelumnya`,`bd`.`bdjmlrevisi` AS `bdjmlrevisi`,`bd`.`bdcetakanke` AS `bdcetakanke`,`bd`.`bdisclose` AS `bdisclose`,`bd`.`bdinputuser` AS `bdinputuser`,`bd`.`bdinputtgl` AS `bdinputtgl`,`bd`.`bdmodifikasiuser` AS `bdmodifikasiuser`,`bd`.`bdmodifikasitgl` AS `bdmodifikasitgl`,`bd`.`bdposting` AS `bdposting`,`bd`.`bdpostingtgl` AS `bdpostingtgl`,`br`.`bnama` AS `bdcabangnama`,`lc`.`lnama` AS `bdlokasinama`,`c`.`kkode` AS `bdkontakkode`,`c`.`knama` AS `bdkontaknama`,`rc`.`nama` AS `bdanggarankategorinama`,`br2`.`bnama` AS `bdanggarancabangnama`,`lc2`.`lnama` AS `bdanggaranlokasinama`,`cc2`.`ccnama` AS `bdanggarancostcenternama`,`d2`.`dnama` AS `bdanggarandivisinama`,`sd2`.`sdnama` AS `bdanggaransubdivisinama`,`p2`.`pnama` AS `bdanggaranproyeknama`,`st1`.`nama` AS `bdstatusnama`,`st2`.`nama` AS `bdstatussebelumnyanama`,`u1`.`unama` AS `bdinputusernama`,`u2`.`unama` AS `bdmodifikasiusernama` from ((((((((((((((`m2_bd` `bd` join `m0_status` `st1` on((`bd`.`bdstatus` = `st1`.`kode`))) join `m0_status` `st2` on((`bd`.`bdstatussebelumnya` = `st2`.`kode`))) join `m0_realization_category` `rc` on((`bd`.`bdanggarankategori` = `rc`.`kode`))) left join `m1_branch` `br` on((`bd`.`bdcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`bd`.`bdlokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`bd`.`bdkontak` = `c`.`kid`))) left join `m1_branch` `br2` on((`bd`.`bdanggarancabang` = `br2`.`bkode`))) left join `m1_location` `lc2` on((`bd`.`bdanggaranlokasi` = `lc2`.`lkode`))) left join `m1_cost_center` `cc2` on((`bd`.`bdanggarancostcenter` = `cc2`.`cckode`))) left join `m1_division` `d2` on((`bd`.`bdanggarandivisi` = `d2`.`dkode`))) left join `m1_subdivision` `sd2` on((`bd`.`bdanggaransubdivisi` = `sd2`.`sdkode`))) left join `m1_project` `p2` on((`bd`.`bdanggaranproyek` = `p2`.`pkode`))) left join `m0_user` `u1` on((`bd`.`bdinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`bd`.`bdmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Bd", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bdid"), ""), sptField,
                     FxDB(dr("bdcabang"), ""), sptField,
                     FxDB(dr("bdlokasi"), ""), sptField,
                     FxDB(dr("bdsumber"), ""), sptField,
                     FxDB(dr("bdautonotransaksi"), 0), sptField,
                     FxDB(dr("bdnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bdtgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bdtglanggaran"), ""), formatTgl), sptField,
                     FxDB(dr("bdkodepa"), ""), sptField,
                     FxDB(dr("bdkontak"), ""), sptField,
                     FxDB(dr("bdkontakperson"), ""), sptField,
                     FxDB(dr("bdanggarankategori"), 0), sptField,
                     FxDB(dr("bdanggarancabang"), ""), sptField,
                     FxDB(dr("bdanggaranlokasi"), ""), sptField,
                     FxDB(dr("bdanggarancostcenter"), ""), sptField,
                     FxDB(dr("bdanggarandivisi"), ""), sptField,
                     FxDB(dr("bdanggaransubdivisi"), ""), sptField,
                     FxDB(dr("bdanggaranproyek"), ""), sptField,
                     FxDB(dr("bduraian"), ""), sptField,
                     FxDB(dr("bdcatatan"), ""), sptField,
                     FxDB(dr("bdmatauang"), ""), sptField,
                     FxDB(dr("bdkurs"), 0), sptField,
                     FxDB(dr("bdstatus"), 0), sptField,
                     FxDB(dr("bdstatussebelumnya"), 0), sptField,
                     FxDB(dr("bdjmlrevisi"), 0), sptField,
                     FxDB(dr("bdcetakanke"), 0), sptField,
                     FxDB(dr("bdisclose"), 0), sptField,
                     FxDB(dr("bdinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bdinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bdmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bdmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bdposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bdpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bdcabangnama"), ""), sptField,
                     FxDB(dr("bdlokasinama"), ""), sptField,
                     FxDB(dr("bdkontakkode"), ""), sptField,
                     FxDB(dr("bdkontaknama"), ""), sptField,
                     FxDB(dr("bdanggarankategorinama"), ""), sptField,
                     FxDB(dr("bdanggarancabangnama"), ""), sptField,
                     FxDB(dr("bdanggaranlokasinama"), ""), sptField,
                     FxDB(dr("bdanggarancostcenternama"), ""), sptField,
                     FxDB(dr("bdanggarandivisinama"), ""), sptField,
                     FxDB(dr("bdanggaransubdivisinama"), ""), sptField,
                     FxDB(dr("bdanggaranproyeknama"), ""), sptField,
                     FxDB(dr("bdstatusnama"), ""), sptField,
                     FxDB(dr("bdstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("bdinputusernama"), ""), sptField,
                     FxDB(dr("bdmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcabangnama, bdlokasinama, bdkontakkode, bdkontaknama, bdanggarankategorinama, bdanggarancabangnama, bdanggaranlokasinama, bdanggarancostcenternama, bdanggarandivisinama, bdanggaransubdivisinama, bdanggaranproyeknama, bdstatusnama, bdstatussebelumnyanama, bdinputusernama, bdmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_BdTerkait(ByVal param As String) As String

        'M2_BdTerkait --------------------------------------------------------
        'bdid, bdnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
                     FxDB(dr("bdid"), 0), sptField,
                     FxDB(dr("bdnotransaksi"), ""), sptField,
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
            result(2) = "Related BD data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bdid, bdnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_BdSimpanOld(ByVal param As String) As String

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
        'bdid(0) As , bdcabang(1) As String, bdlokasi(2) As String, bdsumber(3) As String, bdautonotransaksi(4) As Integer, 
        'bdnotransaksi(5) As String, bdtgl(6) As Date, bdtglanggaran(7) As Date, bdkodepa(8) As , bdkontak(9) As , 
        'bdkontakperson(10) As String, bdanggarankategori(11) As Integer, bdanggarancabang(12) As String, bdanggaranlokasi(13) As String, bdanggarancostcenter(14) As String, 
        'bdanggarandivisi(15) As String, bdanggaransubdivisi(16) As String, bdanggaranproyek(17) As String, bduraian(18) As String, bdcatatan(19) As String, 
        'bdmatauang(20) As String, bdkurs(21) As Double, bdstatus(22) As Integer, bdstatussebelumnya(23) As Integer, bdjmlrevisi(24) As Integer, 
        'bdcetakanke(25) As Integer, bdisclose(26) As Integer, bdinputuser(27) As , bdinputtgl(28) As DateTime, bdmodifikasiuser(29) As , 
        'bdmodifikasitgl(30) As DateTime, bdposting(31) As Integer, bdpostingtgl(32) As DateTime, bdcustomtext1(33) As String, bdcustomtext2(34) As String, 
        'bdcustomtext3(35) As String, bdcustomtext4(36) As String, bdcustomtext5(37) As String, bdcustomint1(38) As Integer, bdcustomint2(39) As Integer, 
        'bdcustomint3(40) As Integer, bdcustomdbl1(41) As Double, bdcustomdbl2(42) As Double, bdcustomdbl3(43) As Double, bdcustomdate1(44) As Date, 
        'bdcustomdate2(45) As Date, bdcustomdate3(46) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bdid, bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, 
        'bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, 
        'bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, 
        'bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, 
        'bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdpostingtgl, bdcustomtext1, bdcustomtext2, 
        'bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, 
        'bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 47) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'bdid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bdid required numeric." : GoTo selesai
        End If
        'bdautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "bdautonotransaksi required numeric." : GoTo selesai
        End If
        'bdtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "bdtgl required date." : GoTo selesai
        End If
        'bdtglanggaran(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "bdtglanggaran required date." : GoTo selesai
        End If
        'bdkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "bdkodepa required numeric." : GoTo selesai
        End If
        'bdkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "bdkontak required numeric." : GoTo selesai
        End If
        'bdanggarankategori(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "bdanggarankategori required numeric." : GoTo selesai
        End If
        'bdkurs(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "bdkurs required numeric." : GoTo selesai
        End If
        'bdstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "bdstatus required numeric." : GoTo selesai
        End If
        'bdstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "bdstatussebelumnya required numeric." : GoTo selesai
        End If
        'bdjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "bdjmlrevisi required numeric." : GoTo selesai
        End If
        'bdcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bdcetakanke required numeric." : GoTo selesai
        End If
        'bdisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "bdisclose required numeric." : GoTo selesai
        End If
        'bdinputuser(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bdinputuser required numeric." : GoTo selesai
        End If
        'bdinputtgl(28) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "bdinputtgl required date." : GoTo selesai
        End If
        'bdmodifikasiuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bdmodifikasiuser required numeric." : GoTo selesai
        End If
        'bdmodifikasitgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "bdmodifikasitgl required date." : GoTo selesai
        End If
        'bdposting(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bdposting required numeric." : GoTo selesai
        End If
        'bdpostingtgl(32) As DateTime
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "bdpostingtgl required date." : GoTo selesai
        End If
        'bdcustomint1(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bdcustomint1 required numeric." : GoTo selesai
        End If
        'bdcustomint2(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bdcustomint2 required numeric." : GoTo selesai
        End If
        'bdcustomint3(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "bdcustomint3 required numeric." : GoTo selesai
        End If
        'bdcustomdbl1(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "bdcustomdbl1 required numeric." : GoTo selesai
        End If
        'bdcustomdbl2(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "bdcustomdbl2 required numeric." : GoTo selesai
        End If
        'bdcustomdbl3(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "bdcustomdbl3 required numeric." : GoTo selesai
        End If
        'bdcustomdate1(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "bdcustomdate1 required date." : GoTo selesai
        End If
        'bdcustomdate2(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "bdcustomdate2 required date." : GoTo selesai
        End If
        'bdcustomdate3(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "bdcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'bdcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bdcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bdcabang should not be more than 25 character." : GoTo selesai
        End If

        'bdlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bdlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bdlokasi should not be more than 25 character." : GoTo selesai
        End If

        'bdsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "bdsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "bdsumber should not be more than 10 character." : GoTo selesai
        End If

        'bdnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "bdnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "bdnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bdtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "bdtgl can't be empty" : GoTo selesai
        End If

        'bdtglanggaran(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "bdtglanggaran can't be empty" : GoTo selesai
        End If

        'bdanggarancabang(12) As String
        If dataUtama(11) = 1 And Len(dataUtama(12)) = 0 Then
            result(2) = "bdanggarancabang can't be empty" : GoTo selesai
        End If

        'bdanggaranlokasi(13) As String
        If dataUtama(11) = 2 And Len(dataUtama(13)) = 0 Then
            result(2) = "bdanggaranlokasi can't be empty" : GoTo selesai
        End If

        'bdanggarancostcenter(14) As String
        If dataUtama(11) = 3 And Len(dataUtama(14)) = 0 Then
            result(2) = "bdanggarancostcenter can't be empty" : GoTo selesai
        End If

        'bdanggarandivisi(15) As String
        If dataUtama(11) = 4 And Len(dataUtama(15)) = 0 Then
            result(2) = "bdanggarandivisi can't be empty" : GoTo selesai
        End If

        'bdanggaransubdivisi(16) As String
        If dataUtama(11) = 5 And Len(dataUtama(16)) = 0 Then
            result(2) = "bdanggaransubdivisi can't be empty" : GoTo selesai
        End If

        'bdanggaranproyek(17) As String
        If dataUtama(11) = 6 And Len(dataUtama(17)) = 0 Then
            result(2) = "bdanggaranproyek can't be empty" : GoTo selesai
        End If

        'bdmatauang(20) As String
        If Len(dataUtama(20)) = 0 Then
            result(2) = "bdmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(20)) > 25 Then
            result(2) = "bdmatauang should not be more than 25 character." : GoTo selesai
        End If

        'bdkurs(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "bdkurs can't be empty" : GoTo selesai
        End If

        'bdinputtgl(28) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "bdinputtgl can't be empty" : GoTo selesai
        End If

        'bdmodifikasitgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "bdmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bdpostingtgl(32) As DateTime
        If Len(dataUtama(32)) = 0 Then
            result(2) = "bdpostingtgl can't be empty" : GoTo selesai
        End If

        'bdcustomdbl1(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "bdcustomdbl1 can't be empty" : GoTo selesai
        End If

        'bdcustomdbl2(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "bdcustomdbl2 can't be empty" : GoTo selesai
        End If

        'bdcustomdbl3(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "bdcustomdbl3 can't be empty" : GoTo selesai
        End If

        'bdcustomdate1(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "bdcustomdate1 can't be empty" : GoTo selesai
        End If

        'bdcustomdate2(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "bdcustomdate2 can't be empty" : GoTo selesai
        End If

        'bdcustomdate3(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "bdcustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bdid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdtglanggaran", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggarankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdanggarancabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggaranlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggarancostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggarandivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggaransubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdanggaranproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bduraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bdcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "bdid~bdcabang~bdlokasi~bdsumber~bdautonotransaksi~bdnotransaksi~bdtgl~bdtglanggaran~bdkodepa~bdkontak~bdkontakperson~bdanggarankategori~bdanggarancabang~bdanggaranlokasi~bdanggarancostcenter~bdanggarandivisi~bdanggaransubdivisi~bdanggaranproyek~bduraian~bdcatatan~bdmatauang~bdkurs~bdstatus~bdstatussebelumnya~bdjmlrevisi~bdcetakanke~bdisclose~bdinputuser~bdinputtgl~bdmodifikasiuser~bdmodifikasitgl~bdposting~bdpostingtgl~bdcustomtext1~bdcustomtext2~bdcustomtext3~bdcustomtext4~bdcustomtext5~bdcustomint1~bdcustomint2~bdcustomint3~bdcustomdbl1~bdcustomdbl2~bdcustomdbl3~bdcustomdate1~bdcustomdate2~bdcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idbddetail(0) As Integer, idbd(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, catatan(7) As String, costcenter(8) As String, divisi(9) As String, 
        'subdivisi(10) As String, proyek(11) As String, urutan(12) As Integer, isclose(13) As Integer, customtext1(14) As String, 
        'customtext2(15) As String, customtext3(16) As String, customdbl1(17) As Double, customdbl2(18) As Double, customdbl3(19) As Double, 
        'customdate1(20) As Date, customdate2(21) As Date, customdate3(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idbddetail, idbd, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbddetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsString)
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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idbddetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "idbddetail required numeric." : GoTo selesai
            End If
            'idbd(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "idbd required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jumlahvalas required numeric." : GoTo selesai
            End If
            'urutan(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'norek(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
            End If

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

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'customdbl1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idbddetail~idbd~norek~matauang~kurs~jumlah~jumlahvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("bdtgl")), AsFormatTanggal(drutama("bdtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("bdid")
                    notransaksi = drutama("bdnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bdid), bdnotransaksi FROM M2_Bd WHERE bdid='" & result(4) & "' AND bdstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bdid) FROM m2_bd WHERE bdnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============


                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_bd_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Bd_HistorySimpan("" & paramSplit(0) & "★M2_Bd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bdsumber")) & "▼" & FixQuotes(drutama("bdid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================


                        sql = "Update M2_Bd set bdcabang  = '" & FixQuotes(drutama("bdcabang")) & "', bdlokasi  = '" & FixQuotes(drutama("bdlokasi")) & "', bdsumber  = '" & FixQuotes(drutama("bdsumber")) & "', bdautonotransaksi  = " & drutama("bdautonotransaksi") & ", bdnotransaksi  = '" & notransaksi & "', bdtgl  = '" & FixQuotes(AsFormatTanggal(drutama("bdtgl"))) & "', bdtglanggaran  = '" & FixQuotes(AsFormatTanggal(drutama("bdtglanggaran"))) & "', bdkodepa  = " & drutama("bdkodepa") & ", bdkontak  = " & drutama("bdkontak") & ", bdkontakperson  = '" & FixQuotes(drutama("bdkontakperson")) & "', bdanggarankategori  = " & drutama("bdanggarankategori") & ", bdanggarancabang  = '" & FixQuotes(drutama("bdanggarancabang")) & "', bdanggaranlokasi  = '" & FixQuotes(drutama("bdanggaranlokasi")) & "', bdanggarancostcenter  = '" & FixQuotes(drutama("bdanggarancostcenter")) & "', bdanggarandivisi  = '" & FixQuotes(drutama("bdanggarandivisi")) & "', bdanggaransubdivisi  = '" & FixQuotes(drutama("bdanggaransubdivisi")) & "', bdanggaranproyek  = '" & FixQuotes(drutama("bdanggaranproyek")) & "', bduraian  = '" & FixQuotes(drutama("bduraian")) & "', bdcatatan  = '" & FixQuotes(drutama("bdcatatan")) & "', bdmatauang  = '" & FixQuotes(drutama("bdmatauang")) & "', bdkurs  = '" & FixDouble(drutama("bdkurs")) & "', bdstatus  = " & drutama("bdstatus") & ", bdstatussebelumnya  = " & drutama("bdstatussebelumnya") & ", bdjmlrevisi  = bdjmlrevisi+1, bdcetakanke  = " & drutama("bdcetakanke") & ", bdisclose  = " & drutama("bdisclose") & ", bdmodifikasiuser  = " & drutama("bdmodifikasiuser") & ", bdmodifikasitgl  = NOW(), bdposting  = 0, bdcustomtext1  = '" & FixQuotes(drutama("bdcustomtext1")) & "', bdcustomtext2  = '" & FixQuotes(drutama("bdcustomtext2")) & "', bdcustomtext3  = '" & FixQuotes(drutama("bdcustomtext3")) & "', bdcustomtext4  = '" & FixQuotes(drutama("bdcustomtext4")) & "', bdcustomtext5  = '" & FixQuotes(drutama("bdcustomtext5")) & "', bdcustomint1  = " & drutama("bdcustomint1") & ", bdcustomint2  = " & drutama("bdcustomint2") & ", bdcustomint3  = " & drutama("bdcustomint3") & ", bdcustomdbl1  = '" & FixDouble(drutama("bdcustomdbl1")) & "', bdcustomdbl2  = '" & FixDouble(drutama("bdcustomdbl2")) & "', bdcustomdbl3  = '" & FixDouble(drutama("bdcustomdbl3")) & "', bdcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate1"))) & "', bdcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate2"))) & "', bdcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate3"))) & "' where bdid = '" & drutama("bdid") & "'"
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

                    If drutama("bdautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bdcabang"), drutama("bdlokasi"), drutama("bdsumber"), drutama("bdtgl"))
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
                        notransaksi = drutama("bdnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bdid) FROM m2_bd WHERE bdnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Bd (bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl, bdtglanggaran, bdkodepa, bdkontak, bdkontakperson, bdanggarankategori, bdanggarancabang, bdanggaranlokasi, bdanggarancostcenter, bdanggarandivisi, bdanggaransubdivisi, bdanggaranproyek, bduraian, bdcatatan, bdmatauang, bdkurs, bdstatus, bdstatussebelumnya, bdjmlrevisi, bdcetakanke, bdisclose, bdinputuser, bdinputtgl, bdmodifikasiuser, bdmodifikasitgl, bdposting, bdcustomtext1, bdcustomtext2, bdcustomtext3, bdcustomtext4, bdcustomtext5, bdcustomint1, bdcustomint2, bdcustomint3, bdcustomdbl1, bdcustomdbl2, bdcustomdbl3, bdcustomdate1, bdcustomdate2, bdcustomdate3) values('" & FixQuotes(drutama("bdcabang")) & "', '" & FixQuotes(drutama("bdlokasi")) & "', '" & FixQuotes(drutama("bdsumber")) & "', " & drutama("bdautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("bdtgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdtglanggaran"))) & "', " & drutama("bdkodepa") & ", " & drutama("bdkontak") & ", '" & FixQuotes(drutama("bdkontakperson")) & "', " & drutama("bdanggarankategori") & ", '" & FixQuotes(drutama("bdanggarancabang")) & "', '" & FixQuotes(drutama("bdanggaranlokasi")) & "', '" & FixQuotes(drutama("bdanggarancostcenter")) & "', '" & FixQuotes(drutama("bdanggarandivisi")) & "', '" & FixQuotes(drutama("bdanggaransubdivisi")) & "', '" & FixQuotes(drutama("bdanggaranproyek")) & "', '" & FixQuotes(drutama("bduraian")) & "', '" & FixQuotes(drutama("bdcatatan")) & "', '" & FixQuotes(drutama("bdmatauang")) & "', '" & FixDouble(drutama("bdkurs")) & "', " & drutama("bdstatus") & ", " & drutama("bdstatussebelumnya") & ", " & drutama("bdjmlrevisi") & ", " & drutama("bdcetakanke") & ", " & drutama("bdisclose") & ", " & drutama("bdinputuser") & ", NOW(), " & drutama("bdmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("bdcustomtext1")) & "', '" & FixQuotes(drutama("bdcustomtext2")) & "', '" & FixQuotes(drutama("bdcustomtext3")) & "', '" & FixQuotes(drutama("bdcustomtext4")) & "', '" & FixQuotes(drutama("bdcustomtext5")) & "', " & drutama("bdcustomint1") & ", " & drutama("bdcustomint2") & ", " & drutama("bdcustomint3") & ", '" & FixDouble(drutama("bdcustomdbl1")) & "', '" & FixDouble(drutama("bdcustomdbl2")) & "', '" & FixDouble(drutama("bdcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bdcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select bdid from M2_Bd where bdnotransaksi='" & notransaksi & "' AND Bdinputuser= '" & userid & "' order by Bdmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai

                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Bd_Detail where idbd = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idbddetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M2_Bd_Detail(idbddetail, idbd, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'UPDATE ANGGARAN ====================================================================
                If drutama("bdstatus") = 2 Then
                    'UPDATE ANGGARAN SESUAI KATEGORI
                    If drutama("bdanggarankategori") = 0 Then
                        'ANGGARAN GLOBAL
                        sql = "INSERT INTO m2_realization ( SELECT YEAR(bd.bdtglanggaran) as rtahun, MONTH(bd.bdtglanggaran) as rbulan, bdd.norek as rnorek, 0 as rjmldebit, 0 as rjmlkredit, bdd.jumlah as ranggaran, ap.apkode as rkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE ranggaran = VALUES(ranggaran)"

                    ElseIf drutama("bdanggarankategori") = 1 Then
                        'ANGGARAN CABANG
                        sql = "INSERT INTO m2_realization_branch ( SELECT YEAR(bd.bdtglanggaran) as rwtahun, MONTH(bd.bdtglanggaran) as rwbulan, bd.bdanggarancabang as rwcabang, bdd.norek as rwnorek, 0 as rwjmldebit, 0 as rwjmlkredit, bdd.jumlah as rwanggaran, ap.apkode as rwkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rwanggaran = VALUES(rwanggaran)"

                    ElseIf drutama("bdanggarankategori") = 2 Then
                        'ANGGARAN LOKASI
                        sql = "INSERT INTO m2_realization_location ( SELECT YEAR(bd.bdtglanggaran) as rltahun, MONTH(bd.bdtglanggaran) as rlbulan, bd.bdanggaranlokasi as rllokasi, bdd.norek as rlnorek, 0 as rljmldebit, 0 as rljmlkredit, bdd.jumlah as rlanggaran, ap.apkode as rlkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rlanggaran = VALUES(rlanggaran)"

                    ElseIf drutama("bdanggarankategori") = 3 Then
                        'ANGGARAN COSTCENTER
                        sql = "INSERT INTO m2_realization_cost_center ( SELECT YEAR(bd.bdtglanggaran) as rcctahun, MONTH(bd.bdtglanggaran) as rccbulan, bd.bdanggarancostcenter as rcccostcenter, bdd.norek as rccnorek, 0 as rccjmldebit, 0 as rccjmlkredit, bdd.jumlah as rccanggaran, ap.apkode as rcckodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rccanggaran = VALUES(rccanggaran)"

                    ElseIf drutama("bdanggarankategori") = 4 Then
                        'ANGGARAN DIVISI
                        sql = "INSERT INTO m2_realization_division ( SELECT YEAR(bd.bdtglanggaran) as rdtahun, MONTH(bd.bdtglanggaran) as rdbulan, bd.bdanggarandivisi as rddivisi, bdd.norek as rdnorek, 0 as rdjmldebit, 0 as rdjmlkredit, bdd.jumlah as rdanggaran, ap.apkode as rdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rdanggaran = VALUES(rdanggaran)"

                    ElseIf drutama("bdanggarankategori") = 5 Then
                        'ANGGARAN SUBDIVISI
                        sql = "INSERT INTO m2_realization_subdivision ( SELECT YEAR(bd.bdtglanggaran) as rsdtahun, MONTH(bd.bdtglanggaran) as rsdbulan, bd.bdanggaransubdivisi as rsdsubdivisi, bdd.norek as rsdnorek, 0 as rsdjmldebit, 0 as rsdjmlkredit, bdd.jumlah as rsdanggaran, ap.apkode as rsdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rsdanggaran = VALUES(rsdanggaran)"

                    ElseIf drutama("bdanggarankategori") = 6 Then
                        'ANGGARAN PROYEK
                        sql = "INSERT INTO m2_realization_project ( SELECT YEAR(bd.bdtglanggaran) as rptahun, MONTH(bd.bdtglanggaran) as rpbulan, bd.bdanggaranproyek as rpproyek, bdd.norek as rpnorek, 0 as rpjmldebit, 0 as rpjmlkredit, bdd.jumlah as rpanggaran, ap.apkode as rpkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & result(4) & "' ) ON DUPLICATE KEY UPDATE rpanggaran = VALUES(rpanggaran)"

                    End If

                    'EKSEKUSI QUERY UPDATE ANGGARAN
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

                End If
                'END OF UPDATE ANGGARAN =============================================================


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "BD", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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
    Public Function M2_BdUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("bdkontakkode", "c.kkode")
            Filter = Filter.Replace("bdkontaknama", "c.knama")
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
            Dim sumber As String = "Bd", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bdtgl, Bdnotransaksi, Bdstatus FROM m2_Bd WHERE Bdid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bdstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_bd_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Bd_HistorySimpan("" & paramSplit(0) & "★M2_Bd_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then

                'UPDATE ANGGARAN ==================================================
                sql = "SELECT bdanggarankategori FROM m2_bd bd WHERE bd.bdid = '" & idtransaksi & "'"
                Dim dtTrans As DataTable = AsDataTableAmbilDariDB(sql)
                If dtTrans.Rows.Count > 0 Then
                    Dim anggarankategori As Double = Double.Parse(dtTrans.Rows(0)("bdanggarankategori"))

                    'UPDATE ANGGARAN SESUAI KATEGORI
                    If anggarankategori = 0 Then
                        'ANGGARAN GLOBAL
                        sql = "INSERT INTO m2_realization ( SELECT YEAR(bd.bdtglanggaran) as rtahun, MONTH(bd.bdtglanggaran) as rbulan, bdd.norek as rnorek, 0 as rjmldebit, 0 as rjmlkredit, 0 as ranggaran, ap.apkode as rkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE ranggaran = VALUES(ranggaran)"

                    ElseIf anggarankategori = 1 Then
                        'ANGGARAN CABANG
                        sql = "INSERT INTO m2_realization_branch ( SELECT YEAR(bd.bdtglanggaran) as rwtahun, MONTH(bd.bdtglanggaran) as rwbulan, bd.bdanggarancabang as rwcabang, bdd.norek as rwnorek, 0 as rwjmldebit, 0 as rwjmlkredit, 0 as rwanggaran, ap.apkode as rwkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rwanggaran = VALUES(rwanggaran)"

                    ElseIf anggarankategori = 2 Then
                        'ANGGARAN LOKASI
                        sql = "INSERT INTO m2_realization_location ( SELECT YEAR(bd.bdtglanggaran) as rltahun, MONTH(bd.bdtglanggaran) as rlbulan, bd.bdanggaranlokasi as rllokasi, bdd.norek as rlnorek, 0 as rljmldebit, 0 as rljmlkredit, 0 as rlanggaran, ap.apkode as rlkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rlanggaran = VALUES(rlanggaran)"

                    ElseIf anggarankategori = 3 Then
                        'ANGGARAN COSTCENTER
                        sql = "INSERT INTO m2_realization_cost_center ( SELECT YEAR(bd.bdtglanggaran) as rcctahun, MONTH(bd.bdtglanggaran) as rccbulan, bd.bdanggarancostcenter as rcccostcenter, bdd.norek as rccnorek, 0 as rccjmldebit, 0 as rccjmlkredit, 0 as rccanggaran, ap.apkode as rcckodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rccanggaran = VALUES(rccanggaran)"

                    ElseIf anggarankategori = 4 Then
                        'ANGGARAN DIVISI
                        sql = "INSERT INTO m2_realization_division ( SELECT YEAR(bd.bdtglanggaran) as rdtahun, MONTH(bd.bdtglanggaran) as rdbulan, bd.bdanggarandivisi as rddivisi, bdd.norek as rdnorek, 0 as rdjmldebit, 0 as rdjmlkredit, 0 as rdanggaran, ap.apkode as rdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rdanggaran = VALUES(rdanggaran)"

                    ElseIf anggarankategori = 5 Then
                        'ANGGARAN SUBDIVISI
                        sql = "INSERT INTO m2_realization_subdivision ( SELECT YEAR(bd.bdtglanggaran) as rsdtahun, MONTH(bd.bdtglanggaran) as rsdbulan, bd.bdanggaransubdivisi as rsdsubdivisi, bdd.norek as rsdnorek, 0 as rsdjmldebit, 0 as rsdjmlkredit, 0 as rsdanggaran, ap.apkode as rsdkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rsdanggaran = VALUES(rsdanggaran)"

                    ElseIf anggarankategori = 6 Then
                        'ANGGARAN PROYEK
                        sql = "INSERT INTO m2_realization_project ( SELECT YEAR(bd.bdtglanggaran) as rptahun, MONTH(bd.bdtglanggaran) as rpbulan, bd.bdanggaranproyek as rpproyek, bdd.norek as rpnorek, 0 as rpjmldebit, 0 as rpjmlkredit, 0 as rpanggaran, ap.apkode as rpkodepa FROM m2_bd bd JOIN m2_bd_detail bdd ON bd.bdid = bdd.idbd JOIN m2_accounting_period ap ON YEAR(bd.bdtglanggaran) = ap.aptahun AND MONTH(bd.bdtglanggaran) = ap.apbulan WHERE bd.bdid = '" & idtransaksi & "' ) ON DUPLICATE KEY UPDATE rpanggaran = VALUES(rpanggaran)"

                    End If

                    'EKSEKUSI QUERY UPDATE ANGGARAN
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

                End If
                'END OF UPDATE ANGGARAN ===========================================

            End If


            'update status utama
            sql = "UPDATE M2_Bd SET Bdstatus = " & nilaiStatus & ", bdmodifikasiuser='" & userid & "', bdmodifikasitgl = NOW(), bdposting = 0, bdpostingtgl = '1971-01-01 00:00:00', Bdjmlrevisi = Bdjmlrevisi + 1 WHERE bdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_BdSearch(PostWsSearch(paramSplit(0), "M2_BdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_BdDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("bdkontakkode", "c.kkode")
            Filter = Filter.Replace("bdkontaknama", "c.knama")
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
            Dim sumber As String = "BD", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT bdid, bdnotransaksi FROM m2_bd WHERE bdid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bdcabang, bdlokasi, bdsumber, bdautonotransaksi, bdnotransaksi, bdtgl"
            sql &= " FROM M2_bd"
            sql &= " WHERE bdid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bdcabang")
                lokasi = dtNomorNext.Rows(0)("bdlokasi")
                sumber = dtNomorNext.Rows(0)("bdsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("bdautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("bdnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bdtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M2_Bd_Detail WHERE idbd = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Bd WHERE bdid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_BdSearch(PostWsSearch(paramSplit(0), "M2_BdSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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