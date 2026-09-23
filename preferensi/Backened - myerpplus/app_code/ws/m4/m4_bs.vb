Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_bs
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_BsSimpan(ByVal param As String) As String
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
        'bsid(0) As Integer, bscabang(1) As String, bslokasi(2) As String, bsgudang(3) As String, bsasalbarang(4) As String, 
        'bsasalbarangkategori(5) As Integer, bsjenispembelian(6) As String, bsjenispembeliankategori(7) As Integer, bscarabayar(8) As Integer, bssumber(9) As String, 
        'bsnogrup(10) As String, bsautonotransaksi(11) As Integer, bsnotransaksi(12) As String, bstgl(13) As Date, bskodepa(14) As Integer, 
        'bsbagianperbandingan(15) As Integer, bsbagianperbandingankontak(16) As String, bsuraian(17) As String, bscatatan(18) As String, bsnoref(19) As String, 
        'bstglnoref(20) As Date, bstglpenutupan(21) As Date, bsmatauang(22) As String, bsidrq1(23) As Integer, bsidrq2(24) As Integer, 
        'bsidrq3(25) As Integer, bsidrq4(26) As Integer, bsidrq5(27) As Integer, bsidrq1statuspo(28) As Integer, bsidrq2statuspo(29) As Integer, 
        'bsidrq3statuspo(30) As Integer, bsidrq4statuspo(31) As Integer, bsidrq5statuspo(32) As Integer, bsstatus(33) As Integer, bsstatussebelumnya(34) As Integer, 
        'bsjmlrevisi(35) As Integer, bscetakanke(36) As Integer, bsinputuser(37) As Integer, bsinputtgl(38) As DateTime, bsmodifikasiuser(39) As Integer, 
        'bsmodifikasitgl(40) As DateTime, bsisclose(41) As Integer, bscustomtext1(42) As String, bscustomtext2(43) As String, bscustomtext3(44) As String, 
        'bscustomtext4(45) As String, bscustomtext5(46) As String, bscustomint1(47) As Integer, bscustomint2(48) As Integer, bscustomint3(49) As Integer, 
        'bscustomdbl1(50) As Double, bscustomdbl2(51) As Double, bscustomdbl3(52) As Double, bscustomdate1(53) As Date, bscustomdate2(54) As Date, 
        'bscustomdate3(55) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, 
        'bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, 
        'bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, 
        'bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, 
        'bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, 
        'bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, 
        'bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, 
        'bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 56) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'bsid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bsid required numeric." : GoTo selesai
        End If
        'bsasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "bsasalbarangkategori required numeric." : GoTo selesai
        End If
        'bsjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "bsjenispembeliankategori required numeric." : GoTo selesai
        End If
        'bscarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "bscarabayar required numeric." : GoTo selesai
        End If
        'bsautonotransaksi(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "bsautonotransaksi required numeric." : GoTo selesai
        End If
        'bstgl(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "bstgl required date." : GoTo selesai
        End If
        'bskodepa(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "bskodepa required numeric." : GoTo selesai
        End If
        'bsbagianperbandingan(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "bsbagianperbandingan required numeric." : GoTo selesai
        End If
        If (dataUtama(15) < 1) Then
            result(2) = "bsbagianperbandingan can't be empty." : GoTo selesai
        End If
        'bstglnoref(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "bstglnoref required date." : GoTo selesai
        End If
        'bstglpenutupan(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "bstglpenutupan required date." : GoTo selesai
        End If
        'bsidrq1(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "bsidrq1 required numeric." : GoTo selesai
        End If
        'bsidrq2(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "bsidrq2 required numeric." : GoTo selesai
        End If
        'bsidrq3(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bsidrq3 required numeric." : GoTo selesai
        End If
        'bsidrq4(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "bsidrq4 required numeric." : GoTo selesai
        End If
        'bsidrq5(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bsidrq5 required numeric." : GoTo selesai
        End If
        'bsidrq1statuspo(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "bsidrq1statuspo required numeric." : GoTo selesai
        End If
        'bsidrq2statuspo(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bsidrq2statuspo required numeric." : GoTo selesai
        End If
        'bsidrq3statuspo(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "bsidrq3statuspo required numeric." : GoTo selesai
        End If
        'bsidrq4statuspo(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bsidrq4statuspo required numeric." : GoTo selesai
        End If
        'bsidrq5statuspo(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "bsidrq5statuspo required numeric." : GoTo selesai
        End If
        'bsstatus(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "bsstatus required numeric." : GoTo selesai
        End If
        'bsstatussebelumnya(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "bsstatussebelumnya required numeric." : GoTo selesai
        End If
        'bsjmlrevisi(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "bsjmlrevisi required numeric." : GoTo selesai
        End If
        'bscetakanke(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "bscetakanke required numeric." : GoTo selesai
        End If
        'bsinputuser(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "bsinputuser required numeric." : GoTo selesai
        End If
        'bsinputtgl(38) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "bsinputtgl required date." : GoTo selesai
        End If
        'bsmodifikasiuser(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bsmodifikasiuser required numeric." : GoTo selesai
        End If
        'bsmodifikasitgl(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "bsmodifikasitgl required date." : GoTo selesai
        End If
        'bsisclose(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "bsisclose required numeric." : GoTo selesai
        End If
        'bscustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "bscustomint1 required numeric." : GoTo selesai
        End If
        'bscustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "bscustomint2 required numeric." : GoTo selesai
        End If
        'bscustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "bscustomint3 required numeric." : GoTo selesai
        End If
        'bscustomdbl1(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "bscustomdbl1 required numeric." : GoTo selesai
        End If
        'bscustomdbl2(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "bscustomdbl2 required numeric." : GoTo selesai
        End If
        'bscustomdbl3(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "bscustomdbl3 required numeric." : GoTo selesai
        End If
        'bscustomdate1(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "bscustomdate1 required date." : GoTo selesai
        End If
        'bscustomdate2(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "bscustomdate2 required date." : GoTo selesai
        End If
        'bscustomdate3(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "bscustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'bscabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bscabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bscabang should not be more than 25 character." : GoTo selesai
        End If

        'bslokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bslokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bslokasi should not be more than 25 character." : GoTo selesai
        End If

        'bsgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "bsgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "bsgudang should not be more than 25 character." : GoTo selesai
        End If

        'bssumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bssumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "bssumber should not be more than 10 character." : GoTo selesai
        End If

        'bsnotransaksi(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "bsnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 50 Then
            result(2) = "bsnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bstgl(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "bstgl can't be empty" : GoTo selesai
        End If

        'bstglnoref(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "bstglnoref can't be empty" : GoTo selesai
        End If

        'bstglpenutupan(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "bstglpenutupan can't be empty" : GoTo selesai
        End If

        'bsmatauang(22) As String
        If Len(dataUtama(22)) = 0 Then
            result(2) = "bsmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(22)) > 25 Then
            result(2) = "bsmatauang should not be more than 25 character." : GoTo selesai
        End If

        'bsinputtgl(38) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "bsinputtgl can't be empty" : GoTo selesai
        End If

        'bsmodifikasitgl(40) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "bsmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bscustomdbl1(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "bscustomdbl1 can't be empty" : GoTo selesai
        End If

        'bscustomdbl2(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "bscustomdbl2 can't be empty" : GoTo selesai
        End If

        'bscustomdbl3(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "bscustomdbl3 can't be empty" : GoTo selesai
        End If

        'bscustomdate1(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "bscustomdate1 can't be empty" : GoTo selesai
        End If

        'bscustomdate2(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "bscustomdate2 can't be empty" : GoTo selesai
        End If

        'bscustomdate3(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "bscustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bsid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bslokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bssumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsnogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bskodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsbagianperbandingan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsbagianperbandingankontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsidrq1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq1statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq2statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq3statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq4statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq5statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "bsid~bscabang~bslokasi~bsgudang~bsasalbarang~bsasalbarangkategori~bsjenispembelian~bsjenispembeliankategori~bscarabayar~bssumber~bsnogrup~bsautonotransaksi~bsnotransaksi~bstgl~bskodepa~bsbagianperbandingan~bsbagianperbandingankontak~bsuraian~bscatatan~bsnoref~bstglnoref~bstglpenutupan~bsmatauang~bsidrq1~bsidrq2~bsidrq3~bsidrq4~bsidrq5~bsidrq1statuspo~bsidrq2statuspo~bsidrq3statuspo~bsidrq4statuspo~bsidrq5statuspo~bsstatus~bsstatussebelumnya~bsjmlrevisi~bscetakanke~bsinputuser~bsinputtgl~bsmodifikasiuser~bsmodifikasitgl~bsisclose~bscustomtext1~bscustomtext2~bscustomtext3~bscustomtext4~bscustomtext5~bscustomint1~bscustomint2~bscustomint3~bscustomdbl1~bscustomdbl2~bscustomdbl3~bscustomdate1~bscustomdate2~bscustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idbsdetail(0) As Integer, idbs(1) As Integer, idrqdetail(2) As Integer, terpilih(3) As Integer, hargake(4) As Integer, 
        'catatan(5) As String, urutan(6) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idbsdetail, idbs, idrqdetail, terpilih, hargake, catatan, urutan


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "terpilih", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hargake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 7) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idbsdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idbs(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idbs required numeric." : GoTo selesai
            End If
            'idrqdetail(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'terpilih(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - terpilih required numeric." : GoTo selesai
            End If
            'hargake(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - hargake required numeric." : GoTo selesai
            End If
            'urutan(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idbsdetail~idbs~idrqdetail~terpilih~hargake~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6)) = False Then
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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 6
                Select Case drutama("bsstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("bstgl")), AsFormatTanggal(drutama("bstgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("bsid")
                    notransaksi = drutama("bsnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(bsid), bsnotransaksi FROM M4_bs WHERE bsid='" & result(4) & "' AND bsstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("bsautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bscabang"), drutama("bslokasi"), drutama("bssumber"), drutama("bstgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bsid) FROM m4_bs WHERE bsnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_bs_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Bs_HistorySimpan("" & paramSplit(0) & "★M4_Bs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bssumber")) & "▼" & FixQuotes(drutama("bsid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Bs set bscabang  = '" & FixQuotes(drutama("bscabang")) & "', bslokasi  = '" & FixQuotes(drutama("bslokasi")) & "', bsgudang  = '" & FixQuotes(drutama("bsgudang")) & "', bsasalbarang  = '" & FixQuotes(drutama("bsasalbarang")) & "', bsasalbarangkategori  = " & drutama("bsasalbarangkategori") & ", bsjenispembelian  = '" & FixQuotes(drutama("bsjenispembelian")) & "', bsjenispembeliankategori  = " & drutama("bsjenispembeliankategori") & ", bscarabayar  = " & drutama("bscarabayar") & ", bssumber  = '" & FixQuotes(drutama("bssumber")) & "', bsnogrup  = '" & FixQuotes(drutama("bsnogrup")) & "', bsautonotransaksi  = " & drutama("bsautonotransaksi") & ", bsnotransaksi  = '" & notransaksi & "', bstgl  = '" & FixQuotes(AsFormatTanggal(drutama("bstgl"))) & "', bskodepa  = " & drutama("bskodepa") & ", bsbagianperbandingan  = " & drutama("bsbagianperbandingan") & ", bsbagianperbandingankontak  = '" & FixQuotes(drutama("bsbagianperbandingankontak")) & "', bsuraian  = '" & FixQuotes(drutama("bsuraian")) & "', bscatatan  = '" & FixQuotes(drutama("bscatatan")) & "', bsnoref  = '" & FixQuotes(drutama("bsnoref")) & "', bstglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("bstglnoref"))) & "', bstglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("bstglpenutupan"))) & "', bsmatauang  = '" & FixQuotes(drutama("bsmatauang")) & "', bsidrq1  = " & drutama("bsidrq1") & ", bsidrq2  = " & drutama("bsidrq2") & ", bsidrq3  = " & drutama("bsidrq3") & ", bsidrq4  = " & drutama("bsidrq4") & ", bsidrq5  = " & drutama("bsidrq5") & ", bsidrq1statuspo  = " & drutama("bsidrq1statuspo") & ", bsidrq2statuspo  = " & drutama("bsidrq2statuspo") & ", bsidrq3statuspo  = " & drutama("bsidrq3statuspo") & ", bsidrq4statuspo  = " & drutama("bsidrq4statuspo") & ", bsidrq5statuspo  = " & drutama("bsidrq5statuspo") & ", bsstatus  = " & drutama("bsstatus") & ", bsstatussebelumnya  = " & drutama("bsstatussebelumnya") & ", bsjmlrevisi  = bsjmlrevisi+1, bscetakanke  = " & drutama("bscetakanke") & ", bsmodifikasiuser  = " & drutama("bsmodifikasiuser") & ", bsmodifikasitgl  = NOW(), bscustomtext1  = '" & FixQuotes(drutama("bscustomtext1")) & "', bscustomtext2  = '" & FixQuotes(drutama("bscustomtext2")) & "', bscustomtext3  = '" & FixQuotes(drutama("bscustomtext3")) & "', bscustomtext4  = '" & FixQuotes(drutama("bscustomtext4")) & "', bscustomtext5  = '" & FixQuotes(drutama("bscustomtext5")) & "', bscustomint1  = " & drutama("bscustomint1") & ", bscustomint2  = " & drutama("bscustomint2") & ", bscustomint3  = " & drutama("bscustomint3") & ", bscustomdbl1  = '" & FixDouble(drutama("bscustomdbl1")) & "', bscustomdbl2  = '" & FixDouble(drutama("bscustomdbl2")) & "', bscustomdbl3  = '" & FixDouble(drutama("bscustomdbl3")) & "', bscustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate1"))) & "', bscustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate2"))) & "', bscustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate3"))) & "' where bsid = '" & drutama("bsid") & "'"
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

                    If drutama("bsautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bscabang"), drutama("bslokasi"), drutama("bssumber"), drutama("bstgl"))
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
                        notransaksi = drutama("bsnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bsid) FROM m4_bs WHERE bsnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Bs (bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3) values('" & FixQuotes(drutama("bscabang")) & "', '" & FixQuotes(drutama("bslokasi")) & "', '" & FixQuotes(drutama("bsgudang")) & "', '" & FixQuotes(drutama("bsasalbarang")) & "', " & drutama("bsasalbarangkategori") & ", '" & FixQuotes(drutama("bsjenispembelian")) & "', " & drutama("bsjenispembeliankategori") & ", " & drutama("bscarabayar") & ", '" & FixQuotes(drutama("bssumber")) & "', '" & FixQuotes(drutama("bsnogrup")) & "', " & drutama("bsautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("bstgl"))) & "', " & drutama("bskodepa") & ", " & drutama("bsbagianperbandingan") & ", '" & FixQuotes(drutama("bsbagianperbandingankontak")) & "', '" & FixQuotes(drutama("bsuraian")) & "', '" & FixQuotes(drutama("bscatatan")) & "', '" & FixQuotes(drutama("bsnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bstglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bstglpenutupan"))) & "', '" & FixQuotes(drutama("bsmatauang")) & "', " & drutama("bsidrq1") & ", " & drutama("bsidrq2") & ", " & drutama("bsidrq3") & ", " & drutama("bsidrq4") & ", " & drutama("bsidrq5") & ", " & drutama("bsidrq1statuspo") & ", " & drutama("bsidrq2statuspo") & ", " & drutama("bsidrq3statuspo") & ", " & drutama("bsidrq4statuspo") & ", " & drutama("bsidrq5statuspo") & ", " & drutama("bsstatus") & ", " & drutama("bsstatussebelumnya") & ", " & drutama("bsjmlrevisi") & ", " & drutama("bscetakanke") & ", " & drutama("bsinputuser") & ", NOW(), " & drutama("bsmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("bsisclose") & ", '" & FixQuotes(drutama("bscustomtext1")) & "', '" & FixQuotes(drutama("bscustomtext2")) & "', '" & FixQuotes(drutama("bscustomtext3")) & "', '" & FixQuotes(drutama("bscustomtext4")) & "', '" & FixQuotes(drutama("bscustomtext5")) & "', " & drutama("bscustomint1") & ", " & drutama("bscustomint2") & ", " & drutama("bscustomint3") & ", '" & FixDouble(drutama("bscustomdbl1")) & "', '" & FixDouble(drutama("bscustomdbl2")) & "', '" & FixDouble(drutama("bscustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select bsid from M4_bs where bsnotransaksi='" & notransaksi & "' AND bsinputuser= '" & userid & "' order by bsmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Bs_Detail where idbs = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idbsdetail") & ", " & result(4) & ", " & dr1("idrqdetail") & ", " & dr1("terpilih") & ", " & dr1("hargake") & ", '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                    Next
                    sql = "Insert into M4_Bs_Detail(idbsdetail, idbs, idrqdetail, terpilih, hargake, catatan, urutan) values" & strValue2.ToString & ""
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
                Dim sumber As String = "BS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_BsUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("bsbagianperbandingankode", "c1.kkode")
            Filter = Filter.Replace("bsbagianperbandingannama", "c1.knama")
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
            Dim sumber As String = "Bs", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bstgl, Bsnotransaksi, Bsstatus FROM M4_Bs WHERE Bsid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bsstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_bs_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Bs_HistorySimpan("" & paramSplit(0) & "★M4_Bs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_bs_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M4_Bs SET Bsstatus = " & nilaiStatus & ", Bsmodifikasiuser='" & userid & "', Bsmodifikasitgl = NOW(), Bsjmlrevisi = Bsjmlrevisi + 1 WHERE Bsid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_BsSearch(PostWsSearch(paramSplit(0), "M4_BsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_BsDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("bsbagianperbandingankode", "c1.kkode")
            Filter = Filter.Replace("bsbagianperbandingannama", "c1.knama")
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
            Dim sumber As String = "Bs", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Bsid, Bsnotransaksi FROM M4_Bs WHERE Bsid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bscabang, bslokasi, bssumber, bsautonotransaksi, bsnotransaksi, bstgl"
            sql &= " FROM M4_bs"
            sql &= " WHERE bsid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bscabang")
                lokasi = dtNomorNext.Rows(0)("bslokasi")
                sumber = dtNomorNext.Rows(0)("bssumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("bsautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("bsnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bstgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Bs_Detail WHERE idbs ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Bs WHERE bsid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_BsSearch(PostWsSearch(paramSplit(0), "M4_BsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_BsGetdataById(ByVal param As String) As String

        'M4_BsGetdataById Utama --------------------------------------------------------
        'bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, 
        'bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, 
        'bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, 
        'bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, 
        'bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, 
        'bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, 
        'bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, 
        'bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3, 
        'bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bssupplierrq1, 
        'bssupplierkoderq1, bssuppliernamarq1, bsterminrq1, bsterminnamarq1, bsterminharijatuhtemporq1, bsnotransaksirq2, bssupplierrq2, 
        'bssupplierkoderq2, bssuppliernamarq2, bsterminrq2, bsterminnamarq2, bsterminharijatuhtemporq2, bsnotransaksirq3, bssupplierrq3, 
        'bssupplierkoderq3, bssuppliernamarq3, bsterminrq3, bsterminnamarq3, bsterminharijatuhtemporq3, bsnotransaksirq4, bssupplierrq4, 
        'bssupplierkoderq4, bssuppliernamarq4, bsterminrq4, bsterminnamarq4, bsterminharijatuhtemporq4, bsnotransaksirq5, bssupplierrq5, 
        'bssupplierkoderq5, bssuppliernamarq5, bsterminrq5, bsterminnamarq5, bsterminharijatuhtemporq5, bsstatusnama, bsstatussebelumnyanama, 
        'bsinputusernama, bsmodifikasiusernama

        'M4_BsGetdataById Detail -------------------------------------------------------
        'idbsdetail, idbs, idrqdetail, terpilih, hargake, 
        'catatan, urutan, idrq, idbarang, namabarang, tipebarang, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai

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

        Dim NmMemcached As String = "aplikasi1-M4_Bs~M4_Bs_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "bsid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "bsid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_bs_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("bsid"), 0), sptField,
                     FxDB(drutama("bscabang"), ""), sptField,
                     FxDB(drutama("bslokasi"), ""), sptField,
                     FxDB(drutama("bsgudang"), ""), sptField,
                     FxDB(drutama("bsasalbarang"), ""), sptField,
                     FxDB(drutama("bsasalbarangkategori"), 0), sptField,
                     FxDB(drutama("bsjenispembelian"), ""), sptField,
                     FxDB(drutama("bsjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("bscarabayar"), 0), sptField,
                     FxDB(drutama("bssumber"), ""), sptField,
                     FxDB(drutama("bsnogrup"), ""), sptField,
                     FxDB(drutama("bsautonotransaksi"), 0), sptField,
                     FxDB(drutama("bsnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bstgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bskodepa"), 0), sptField,
                     FxDB(drutama("bsbagianperbandingan"), 0), sptField,
                     FxDB(drutama("bsbagianperbandingankontak"), ""), sptField,
                     FxDB(drutama("bsuraian"), ""), sptField,
                     FxDB(drutama("bscatatan"), ""), sptField,
                     FxDB(drutama("bsnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bstglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bstglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("bsmatauang"), ""), sptField,
                     FxDB(drutama("bsidrq1"), 0), sptField,
                     FxDB(drutama("bsidrq2"), 0), sptField,
                     FxDB(drutama("bsidrq3"), 0), sptField,
                     FxDB(drutama("bsidrq4"), 0), sptField,
                     FxDB(drutama("bsidrq5"), 0), sptField,
                     FxDB(drutama("bsidrq1statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq2statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq3statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq4statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq5statuspo"), 0), sptField,
                     FxDB(drutama("bsstatus"), 0), sptField,
                     FxDB(drutama("bsstatussebelumnya"), 0), sptField,
                     FxDB(drutama("bsjmlrevisi"), 0), sptField,
                     FxDB(drutama("bscetakanke"), 0), sptField,
                     FxDB(drutama("bsinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bsinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bsmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bsmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bsisclose"), 0), sptField,
                     FxDB(drutama("bscustomtext1"), ""), sptField,
                     FxDB(drutama("bscustomtext2"), ""), sptField,
                     FxDB(drutama("bscustomtext3"), ""), sptField,
                     FxDB(drutama("bscustomtext4"), ""), sptField,
                     FxDB(drutama("bscustomtext5"), ""), sptField,
                     FxDB(drutama("bscustomint1"), 0), sptField,
                     FxDB(drutama("bscustomint2"), 0), sptField,
                     FxDB(drutama("bscustomint3"), 0), sptField,
                     FxDB(drutama("bscustomdbl1"), 0), sptField,
                     FxDB(drutama("bscustomdbl2"), 0), sptField,
                     FxDB(drutama("bscustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bscustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bscustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bscustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bscabangnama"), ""), sptField,
                     FxDB(drutama("bslokasinama"), ""), sptField,
                     FxDB(drutama("bsgudangnama"), ""), sptField,
                     FxDB(drutama("bsbagianperbandingankode"), ""), sptField,
                     FxDB(drutama("bsbagianperbandingannama"), ""), sptField,
                     FxDB(drutama("bsnotransaksirq1"), ""), sptField,
                     FxDB(drutama("bssupplierrq1"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq1"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq1"), ""), sptField,
                     FxDB(drutama("bsterminrq1"), ""), sptField,
                     FxDB(drutama("bsterminnamarq1"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq1"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq2"), ""), sptField,
                     FxDB(drutama("bssupplierrq2"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq2"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq2"), ""), sptField,
                     FxDB(drutama("bsterminrq2"), ""), sptField,
                     FxDB(drutama("bsterminnamarq2"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq2"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq3"), ""), sptField,
                     FxDB(drutama("bssupplierrq3"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq3"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq3"), ""), sptField,
                     FxDB(drutama("bsterminrq3"), ""), sptField,
                     FxDB(drutama("bsterminnamarq3"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq3"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq4"), ""), sptField,
                     FxDB(drutama("bssupplierrq4"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq4"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq4"), ""), sptField,
                     FxDB(drutama("bsterminrq4"), ""), sptField,
                     FxDB(drutama("bsterminnamarq4"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq4"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq5"), ""), sptField,
                     FxDB(drutama("bssupplierrq5"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq5"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq5"), ""), sptField,
                     FxDB(drutama("bsterminrq5"), ""), sptField,
                     FxDB(drutama("bsterminnamarq5"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq5"), 0), sptField,
                     FxDB(drutama("bsstatusnama"), ""), sptField,
                     FxDB(drutama("bsstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("bsinputusernama"), ""), sptField,
                     FxDB(drutama("bsmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("idbs"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("terpilih"), 0), sptField,
                     FxDB(dr("hargake"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idrq"), 0), sptField,
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
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3, bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bssupplierrq1, bssupplierkoderq1, bssuppliernamarq1, bsterminrq1, bsterminnamarq1, bsterminharijatuhtemporq1, bsnotransaksirq2, bssupplierrq2, bssupplierkoderq2, bssuppliernamarq2, bsterminrq2, bsterminnamarq2, bsterminharijatuhtemporq2, bsnotransaksirq3, bssupplierrq3, bssupplierkoderq3, bssuppliernamarq3, bsterminrq3, bsterminnamarq3, bsterminharijatuhtemporq3, bsnotransaksirq4, bssupplierrq4, bssupplierkoderq4, bssuppliernamarq4, bsterminrq4, bsterminnamarq4, bsterminharijatuhtemporq4, bsnotransaksirq5, bssupplierrq5, bssupplierkoderq5, bssuppliernamarq5, bsterminrq5, bsterminnamarq5, bsterminharijatuhtemporq5, bsstatusnama, bsstatussebelumnyanama, bsinputusernama, bsmodifikasiusernama" & sptSubParam & "idbsdetail, idbs, idrqdetail, terpilih, hargake, catatan, urutan, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_BsSearch(ByVal param As String) As String
        'M4_BsSearch --------------------------------------------------------
        'bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, 
        'bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, 
        'bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, 
        'bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, 
        'bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, 
        'bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, 
        'bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bsnotransaksirq2, 
        'bsnotransaksirq3, bsnotransaksirq4, bsnotransaksirq5, bsstatusnama, bsstatussebelumnyanama, bsinputusernama, bsmodifikasiusernama

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
            Filter = Filter.Replace("bsbagianperbandingankode", "c1.kkode")
            Filter = Filter.Replace("bsbagianperbandingannama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_bs_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Bs", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bsid"), 0), sptField,
                     FxDB(dr("bscabang"), ""), sptField,
                     FxDB(dr("bslokasi"), ""), sptField,
                     FxDB(dr("bsgudang"), ""), sptField,
                     FxDB(dr("bsasalbarang"), ""), sptField,
                     FxDB(dr("bsasalbarangkategori"), 0), sptField,
                     FxDB(dr("bsjenispembelian"), ""), sptField,
                     FxDB(dr("bsjenispembeliankategori"), 0), sptField,
                     FxDB(dr("bscarabayar"), 0), sptField,
                     FxDB(dr("bssumber"), ""), sptField,
                     FxDB(dr("bsnogrup"), ""), sptField,
                     FxDB(dr("bsautonotransaksi"), 0), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bstgl"), ""), formatTgl), sptField,
                     FxDB(dr("bskodepa"), 0), sptField,
                     FxDB(dr("bsbagianperbandingan"), 0), sptField,
                     FxDB(dr("bsbagianperbandingankontak"), ""), sptField,
                     FxDB(dr("bsuraian"), ""), sptField,
                     FxDB(dr("bscatatan"), ""), sptField,
                     FxDB(dr("bsnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bstglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bstglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("bsmatauang"), ""), sptField,
                     FxDB(dr("bsidrq1"), 0), sptField,
                     FxDB(dr("bsidrq2"), 0), sptField,
                     FxDB(dr("bsidrq3"), 0), sptField,
                     FxDB(dr("bsidrq4"), 0), sptField,
                     FxDB(dr("bsidrq5"), 0), sptField,
                     FxDB(dr("bsidrq1statuspo"), 0), sptField,
                     FxDB(dr("bsidrq2statuspo"), 0), sptField,
                     FxDB(dr("bsidrq3statuspo"), 0), sptField,
                     FxDB(dr("bsidrq4statuspo"), 0), sptField,
                     FxDB(dr("bsidrq5statuspo"), 0), sptField,
                     FxDB(dr("bsstatus"), 0), sptField,
                     FxDB(dr("bsstatussebelumnya"), 0), sptField,
                     FxDB(dr("bsjmlrevisi"), 0), sptField,
                     FxDB(dr("bscetakanke"), 0), sptField,
                     FxDB(dr("bsinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bsinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bsmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bsmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bsisclose"), 0), sptField,
                     FxDB(dr("bscabangnama"), ""), sptField,
                     FxDB(dr("bslokasinama"), ""), sptField,
                     FxDB(dr("bsgudangnama"), ""), sptField,
                     FxDB(dr("bsbagianperbandingankode"), ""), sptField,
                     FxDB(dr("bsbagianperbandingannama"), ""), sptField,
                     FxDB(dr("bsnotransaksirq1"), ""), sptField,
                     FxDB(dr("bsnotransaksirq2"), ""), sptField,
                     FxDB(dr("bsnotransaksirq3"), ""), sptField,
                     FxDB(dr("bsnotransaksirq4"), ""), sptField,
                     FxDB(dr("bsnotransaksirq5"), ""), sptField,
                     FxDB(dr("bsstatusnama"), ""), sptField,
                     FxDB(dr("bsstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("bsinputusernama"), ""), sptField,
                     FxDB(dr("bsmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bsnotransaksirq2, bsnotransaksirq3, bsnotransaksirq4, bsnotransaksirq5, bsstatusnama, bsstatussebelumnyanama, bsinputusernama, bsmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_BsTerkait(ByVal param As String) As String
        'M4_BsTerkait --------------------------------------------------------
        'bsid, bsnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rqid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_bs_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bsid"), 0), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
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
            result(2) = "Related BS data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bsid, bsnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_BsSimpanOld(ByVal param As String) As String
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
        'bsid(0) As Integer, bscabang(1) As String, bslokasi(2) As String, bsgudang(3) As String, bsasalbarang(4) As String, 
        'bsasalbarangkategori(5) As Integer, bsjenispembelian(6) As String, bsjenispembeliankategori(7) As Integer, bscarabayar(8) As Integer, bssumber(9) As String, 
        'bsnogrup(10) As String, bsautonotransaksi(11) As Integer, bsnotransaksi(12) As String, bstgl(13) As Date, bskodepa(14) As Integer, 
        'bsbagianperbandingan(15) As Integer, bsbagianperbandingankontak(16) As String, bsuraian(17) As String, bscatatan(18) As String, bsnoref(19) As String, 
        'bstglnoref(20) As Date, bstglpenutupan(21) As Date, bsmatauang(22) As String, bsidrq1(23) As Integer, bsidrq2(24) As Integer, 
        'bsidrq3(25) As Integer, bsidrq4(26) As Integer, bsidrq5(27) As Integer, bsidrq1statuspo(28) As Integer, bsidrq2statuspo(29) As Integer, 
        'bsidrq3statuspo(30) As Integer, bsidrq4statuspo(31) As Integer, bsidrq5statuspo(32) As Integer, bsstatus(33) As Integer, bsstatussebelumnya(34) As Integer, 
        'bsjmlrevisi(35) As Integer, bscetakanke(36) As Integer, bsinputuser(37) As Integer, bsinputtgl(38) As DateTime, bsmodifikasiuser(39) As Integer, 
        'bsmodifikasitgl(40) As DateTime, bsisclose(41) As Integer, bscustomtext1(42) As String, bscustomtext2(43) As String, bscustomtext3(44) As String, 
        'bscustomtext4(45) As String, bscustomtext5(46) As String, bscustomint1(47) As Integer, bscustomint2(48) As Integer, bscustomint3(49) As Integer, 
        'bscustomdbl1(50) As Double, bscustomdbl2(51) As Double, bscustomdbl3(52) As Double, bscustomdate1(53) As Date, bscustomdate2(54) As Date, 
        'bscustomdate3(55) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, 
        'bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, 
        'bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, 
        'bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, 
        'bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, 
        'bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, 
        'bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, 
        'bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 56) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'bsid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bsid required numeric." : GoTo selesai
        End If
        'bsasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "bsasalbarangkategori required numeric." : GoTo selesai
        End If
        'bsjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "bsjenispembeliankategori required numeric." : GoTo selesai
        End If
        'bscarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "bscarabayar required numeric." : GoTo selesai
        End If
        'bsautonotransaksi(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "bsautonotransaksi required numeric." : GoTo selesai
        End If
        'bstgl(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "bstgl required date." : GoTo selesai
        End If
        'bskodepa(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "bskodepa required numeric." : GoTo selesai
        End If
        'bsbagianperbandingan(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "bsbagianperbandingan required numeric." : GoTo selesai
        End If
        If (dataUtama(15) < 1) Then
            result(2) = "bsbagianperbandingan can't be empty." : GoTo selesai
        End If
        'bstglnoref(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "bstglnoref required date." : GoTo selesai
        End If
        'bstglpenutupan(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "bstglpenutupan required date." : GoTo selesai
        End If
        'bsidrq1(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "bsidrq1 required numeric." : GoTo selesai
        End If
        'bsidrq2(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "bsidrq2 required numeric." : GoTo selesai
        End If
        'bsidrq3(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bsidrq3 required numeric." : GoTo selesai
        End If
        'bsidrq4(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "bsidrq4 required numeric." : GoTo selesai
        End If
        'bsidrq5(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bsidrq5 required numeric." : GoTo selesai
        End If
        'bsidrq1statuspo(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "bsidrq1statuspo required numeric." : GoTo selesai
        End If
        'bsidrq2statuspo(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bsidrq2statuspo required numeric." : GoTo selesai
        End If
        'bsidrq3statuspo(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "bsidrq3statuspo required numeric." : GoTo selesai
        End If
        'bsidrq4statuspo(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bsidrq4statuspo required numeric." : GoTo selesai
        End If
        'bsidrq5statuspo(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "bsidrq5statuspo required numeric." : GoTo selesai
        End If
        'bsstatus(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "bsstatus required numeric." : GoTo selesai
        End If
        'bsstatussebelumnya(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "bsstatussebelumnya required numeric." : GoTo selesai
        End If
        'bsjmlrevisi(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "bsjmlrevisi required numeric." : GoTo selesai
        End If
        'bscetakanke(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "bscetakanke required numeric." : GoTo selesai
        End If
        'bsinputuser(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "bsinputuser required numeric." : GoTo selesai
        End If
        'bsinputtgl(38) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "bsinputtgl required date." : GoTo selesai
        End If
        'bsmodifikasiuser(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bsmodifikasiuser required numeric." : GoTo selesai
        End If
        'bsmodifikasitgl(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "bsmodifikasitgl required date." : GoTo selesai
        End If
        'bsisclose(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "bsisclose required numeric." : GoTo selesai
        End If
        'bscustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "bscustomint1 required numeric." : GoTo selesai
        End If
        'bscustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "bscustomint2 required numeric." : GoTo selesai
        End If
        'bscustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "bscustomint3 required numeric." : GoTo selesai
        End If
        'bscustomdbl1(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "bscustomdbl1 required numeric." : GoTo selesai
        End If
        'bscustomdbl2(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "bscustomdbl2 required numeric." : GoTo selesai
        End If
        'bscustomdbl3(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "bscustomdbl3 required numeric." : GoTo selesai
        End If
        'bscustomdate1(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "bscustomdate1 required date." : GoTo selesai
        End If
        'bscustomdate2(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "bscustomdate2 required date." : GoTo selesai
        End If
        'bscustomdate3(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "bscustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'bscabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bscabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bscabang should not be more than 25 character." : GoTo selesai
        End If

        'bslokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bslokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bslokasi should not be more than 25 character." : GoTo selesai
        End If

        'bsgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "bsgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "bsgudang should not be more than 25 character." : GoTo selesai
        End If

        'bssumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bssumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "bssumber should not be more than 10 character." : GoTo selesai
        End If

        'bsnotransaksi(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "bsnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 50 Then
            result(2) = "bsnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bstgl(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "bstgl can't be empty" : GoTo selesai
        End If

        'bstglnoref(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "bstglnoref can't be empty" : GoTo selesai
        End If

        'bstglpenutupan(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "bstglpenutupan can't be empty" : GoTo selesai
        End If

        'bsmatauang(22) As String
        If Len(dataUtama(22)) = 0 Then
            result(2) = "bsmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(22)) > 25 Then
            result(2) = "bsmatauang should not be more than 25 character." : GoTo selesai
        End If

        'bsinputtgl(38) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "bsinputtgl can't be empty" : GoTo selesai
        End If

        'bsmodifikasitgl(40) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "bsmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bscustomdbl1(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "bscustomdbl1 can't be empty" : GoTo selesai
        End If

        'bscustomdbl2(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "bscustomdbl2 can't be empty" : GoTo selesai
        End If

        'bscustomdbl3(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "bscustomdbl3 can't be empty" : GoTo selesai
        End If

        'bscustomdate1(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "bscustomdate1 can't be empty" : GoTo selesai
        End If

        'bscustomdate2(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "bscustomdate2 can't be empty" : GoTo selesai
        End If

        'bscustomdate3(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "bscustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bsid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bslokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bssumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsnogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bskodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsbagianperbandingan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsbagianperbandingankontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsidrq1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq1statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq2statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq3statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq4statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsidrq5statuspo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bscustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bscustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "bsid~bscabang~bslokasi~bsgudang~bsasalbarang~bsasalbarangkategori~bsjenispembelian~bsjenispembeliankategori~bscarabayar~bssumber~bsnogrup~bsautonotransaksi~bsnotransaksi~bstgl~bskodepa~bsbagianperbandingan~bsbagianperbandingankontak~bsuraian~bscatatan~bsnoref~bstglnoref~bstglpenutupan~bsmatauang~bsidrq1~bsidrq2~bsidrq3~bsidrq4~bsidrq5~bsidrq1statuspo~bsidrq2statuspo~bsidrq3statuspo~bsidrq4statuspo~bsidrq5statuspo~bsstatus~bsstatussebelumnya~bsjmlrevisi~bscetakanke~bsinputuser~bsinputtgl~bsmodifikasiuser~bsmodifikasitgl~bsisclose~bscustomtext1~bscustomtext2~bscustomtext3~bscustomtext4~bscustomtext5~bscustomint1~bscustomint2~bscustomint3~bscustomdbl1~bscustomdbl2~bscustomdbl3~bscustomdate1~bscustomdate2~bscustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idbsdetail(0) As Integer, idbs(1) As Integer, idrqdetail(2) As Integer, terpilih(3) As Integer, hargake(4) As Integer, 
        'catatan(5) As String, urutan(6) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idbsdetail, idbs, idrqdetail, terpilih, hargake, catatan, urutan


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "terpilih", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hargake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 7) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idbsdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idbs(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idbs required numeric." : GoTo selesai
            End If
            'idrqdetail(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'terpilih(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - terpilih required numeric." : GoTo selesai
            End If
            'hargake(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - hargake required numeric." : GoTo selesai
            End If
            'urutan(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idbsdetail~idbs~idrqdetail~terpilih~hargake~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("bstgl")), AsFormatTanggal(drutama("bstgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("bsid")
                    notransaksi = drutama("bsnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bsid), bsnotransaksi FROM M4_bs WHERE bsid='" & result(4) & "' AND bsstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bsid) FROM m4_bs WHERE bsnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_bs_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Bs_HistorySimpan("" & paramSplit(0) & "★M4_Bs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bssumber")) & "▼" & FixQuotes(drutama("bsid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Bs set bscabang  = '" & FixQuotes(drutama("bscabang")) & "', bslokasi  = '" & FixQuotes(drutama("bslokasi")) & "', bsgudang  = '" & FixQuotes(drutama("bsgudang")) & "', bsasalbarang  = '" & FixQuotes(drutama("bsasalbarang")) & "', bsasalbarangkategori  = " & drutama("bsasalbarangkategori") & ", bsjenispembelian  = '" & FixQuotes(drutama("bsjenispembelian")) & "', bsjenispembeliankategori  = " & drutama("bsjenispembeliankategori") & ", bscarabayar  = " & drutama("bscarabayar") & ", bssumber  = '" & FixQuotes(drutama("bssumber")) & "', bsnogrup  = '" & FixQuotes(drutama("bsnogrup")) & "', bsautonotransaksi  = " & drutama("bsautonotransaksi") & ", bsnotransaksi  = '" & notransaksi & "', bstgl  = '" & FixQuotes(AsFormatTanggal(drutama("bstgl"))) & "', bskodepa  = " & drutama("bskodepa") & ", bsbagianperbandingan  = " & drutama("bsbagianperbandingan") & ", bsbagianperbandingankontak  = '" & FixQuotes(drutama("bsbagianperbandingankontak")) & "', bsuraian  = '" & FixQuotes(drutama("bsuraian")) & "', bscatatan  = '" & FixQuotes(drutama("bscatatan")) & "', bsnoref  = '" & FixQuotes(drutama("bsnoref")) & "', bstglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("bstglnoref"))) & "', bstglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("bstglpenutupan"))) & "', bsmatauang  = '" & FixQuotes(drutama("bsmatauang")) & "', bsidrq1  = " & drutama("bsidrq1") & ", bsidrq2  = " & drutama("bsidrq2") & ", bsidrq3  = " & drutama("bsidrq3") & ", bsidrq4  = " & drutama("bsidrq4") & ", bsidrq5  = " & drutama("bsidrq5") & ", bsidrq1statuspo  = " & drutama("bsidrq1statuspo") & ", bsidrq2statuspo  = " & drutama("bsidrq2statuspo") & ", bsidrq3statuspo  = " & drutama("bsidrq3statuspo") & ", bsidrq4statuspo  = " & drutama("bsidrq4statuspo") & ", bsidrq5statuspo  = " & drutama("bsidrq5statuspo") & ", bsstatus  = " & drutama("bsstatus") & ", bsstatussebelumnya  = " & drutama("bsstatussebelumnya") & ", bsjmlrevisi  = bsjmlrevisi+1, bscetakanke  = " & drutama("bscetakanke") & ", bsmodifikasiuser  = " & drutama("bsmodifikasiuser") & ", bsmodifikasitgl  = NOW(), bscustomtext1  = '" & FixQuotes(drutama("bscustomtext1")) & "', bscustomtext2  = '" & FixQuotes(drutama("bscustomtext2")) & "', bscustomtext3  = '" & FixQuotes(drutama("bscustomtext3")) & "', bscustomtext4  = '" & FixQuotes(drutama("bscustomtext4")) & "', bscustomtext5  = '" & FixQuotes(drutama("bscustomtext5")) & "', bscustomint1  = " & drutama("bscustomint1") & ", bscustomint2  = " & drutama("bscustomint2") & ", bscustomint3  = " & drutama("bscustomint3") & ", bscustomdbl1  = '" & FixDouble(drutama("bscustomdbl1")) & "', bscustomdbl2  = '" & FixDouble(drutama("bscustomdbl2")) & "', bscustomdbl3  = '" & FixDouble(drutama("bscustomdbl3")) & "', bscustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate1"))) & "', bscustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate2"))) & "', bscustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate3"))) & "' where bsid = '" & drutama("bsid") & "'"
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

                    If drutama("bsautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bscabang"), drutama("bslokasi"), drutama("bssumber"), drutama("bstgl"))
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
                        notransaksi = drutama("bsnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bsid) FROM m4_bs WHERE bsnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Bs (bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3) values('" & FixQuotes(drutama("bscabang")) & "', '" & FixQuotes(drutama("bslokasi")) & "', '" & FixQuotes(drutama("bsgudang")) & "', '" & FixQuotes(drutama("bsasalbarang")) & "', " & drutama("bsasalbarangkategori") & ", '" & FixQuotes(drutama("bsjenispembelian")) & "', " & drutama("bsjenispembeliankategori") & ", " & drutama("bscarabayar") & ", '" & FixQuotes(drutama("bssumber")) & "', '" & FixQuotes(drutama("bsnogrup")) & "', " & drutama("bsautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("bstgl"))) & "', " & drutama("bskodepa") & ", " & drutama("bsbagianperbandingan") & ", '" & FixQuotes(drutama("bsbagianperbandingankontak")) & "', '" & FixQuotes(drutama("bsuraian")) & "', '" & FixQuotes(drutama("bscatatan")) & "', '" & FixQuotes(drutama("bsnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bstglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bstglpenutupan"))) & "', '" & FixQuotes(drutama("bsmatauang")) & "', " & drutama("bsidrq1") & ", " & drutama("bsidrq2") & ", " & drutama("bsidrq3") & ", " & drutama("bsidrq4") & ", " & drutama("bsidrq5") & ", " & drutama("bsidrq1statuspo") & ", " & drutama("bsidrq2statuspo") & ", " & drutama("bsidrq3statuspo") & ", " & drutama("bsidrq4statuspo") & ", " & drutama("bsidrq5statuspo") & ", " & drutama("bsstatus") & ", " & drutama("bsstatussebelumnya") & ", " & drutama("bsjmlrevisi") & ", " & drutama("bscetakanke") & ", " & drutama("bsinputuser") & ", NOW(), " & drutama("bsmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("bsisclose") & ", '" & FixQuotes(drutama("bscustomtext1")) & "', '" & FixQuotes(drutama("bscustomtext2")) & "', '" & FixQuotes(drutama("bscustomtext3")) & "', '" & FixQuotes(drutama("bscustomtext4")) & "', '" & FixQuotes(drutama("bscustomtext5")) & "', " & drutama("bscustomint1") & ", " & drutama("bscustomint2") & ", " & drutama("bscustomint3") & ", '" & FixDouble(drutama("bscustomdbl1")) & "', '" & FixDouble(drutama("bscustomdbl2")) & "', '" & FixDouble(drutama("bscustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bscustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select bsid from M4_bs where bsnotransaksi='" & notransaksi & "' AND bsinputuser= '" & userid & "' order by bsmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Bs_Detail where idbs = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idbsdetail") & ", " & result(4) & ", " & dr1("idrqdetail") & ", " & dr1("terpilih") & ", " & dr1("hargake") & ", '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                    Next
                    sql = "Insert into M4_Bs_Detail(idbsdetail, idbs, idrqdetail, terpilih, hargake, catatan, urutan) values" & strValue2.ToString & ""
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
                Dim sumber As String = "BS", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_BsUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("bsbagianperbandingankode", "c1.kkode")
            Filter = Filter.Replace("bsbagianperbandingannama", "c1.knama")
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
            Dim sumber As String = "Bs", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bstgl, Bsnotransaksi, Bsstatus FROM M4_Bs WHERE Bsid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bsstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_bs_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Bs_HistorySimpan("" & paramSplit(0) & "★M4_Bs_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_bs_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M4_Bs SET Bsstatus = " & nilaiStatus & ", Bsmodifikasiuser='" & userid & "', Bsmodifikasitgl = NOW(), Bsjmlrevisi = Bsjmlrevisi + 1 WHERE Bsid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_BsSearch(PostWsSearch(paramSplit(0), "M4_BsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_BsDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("bsbagianperbandingankode", "c1.kkode")
            Filter = Filter.Replace("bsbagianperbandingannama", "c1.knama")
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
            Dim sumber As String = "Bs", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Bsid, Bsnotransaksi FROM M4_Bs WHERE Bsid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bscabang, bslokasi, bssumber, bsautonotransaksi, bsnotransaksi, bstgl"
            sql &= " FROM M4_bs"
            sql &= " WHERE bsid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bscabang")
                lokasi = dtNomorNext.Rows(0)("bslokasi")
                sumber = dtNomorNext.Rows(0)("bssumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("bsautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("bsnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bstgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M4_Bs_Detail WHERE idbs ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Bs WHERE bsid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_BsSearch(PostWsSearch(paramSplit(0), "M4_BsSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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