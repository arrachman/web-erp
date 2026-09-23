Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m7_ab
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M7_AbSimpan(ByVal param As String) As String
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
        'abid(0) As , abcabang(1) As String, ablokasi(2) As String, abgudang(3) As String, abasalbarang(4) As String, 
        'abasalbarangkategori(5) As Integer, abjenispembelian(6) As String, abjenispembeliankategori(7) As Integer, abcarabayar(8) As Integer, absumber(9) As String, 
        'abnogrup(10) As String, abautonotransaksi(11) As Integer, abnotransaksi(12) As String, abtgl(13) As Date, abkodepa(14) As , 
        'abbagianperbandingan(15) As , abbagianperbandingankontak(16) As String, aburaian(17) As String, abcatatan(18) As String, abnoref(19) As String, 
        'abtglnoref(20) As Date, abtglpenutupan(21) As Date, abmatauang(22) As String, abidaq1(23) As , abidaq2(24) As , 
        'abidaq3(25) As , abidaq4(26) As , abidaq5(27) As , abidaq1statusao(28) As Integer, abidaq2statusao(29) As Integer, 
        'abidaq3statusao(30) As Integer, abidaq4statusao(31) As Integer, abidaq5statusao(32) As Integer, abstatus(33) As Integer, abstatussebelumnya(34) As Integer, 
        'abjmlrevisi(35) As Integer, abcetakanke(36) As Integer, abinputuser(37) As , abinputtgl(38) As DateTime, abmodifikasiuser(39) As , 
        'abmodifikasitgl(40) As DateTime, abisclose(41) As Integer, abcustomtext1(42) As String, abcustomtext2(43) As String, abcustomtext3(44) As String, 
        'abcustomtext4(45) As String, abcustomtext5(46) As String, abcustomint1(47) As Integer, abcustomint2(48) As Integer, abcustomint3(49) As Integer, 
        'abcustomdbl1(50) As Double, abcustomdbl2(51) As Double, abcustomdbl3(52) As Double, abcustomdate1(53) As Date, abcustomdate2(54) As Date, 
        'abcustomdate3(55) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'abid, abcabang, ablokasi, abgudang, abasalbarang, abasalbarangkategori, abjenispembelian, 
        'abjenispembeliankategori, abcarabayar, absumber, abnogrup, abautonotransaksi, abnotransaksi, abtgl, 
        'abkodepa, abbagianperbandingan, abbagianperbandingankontak, aburaian, abcatatan, abnoref, abtglnoref, 
        'abtglpenutupan, abmatauang, abidaq1, abidaq2, abidaq3, abidaq4, abidaq5, 
        'abidaq1statusao, abidaq2statusao, abidaq3statusao, abidaq4statusao, abidaq5statusao, abstatus, abstatussebelumnya, 
        'abjmlrevisi, abcetakanke, abinputuser, abinputtgl, abmodifikasiuser, abmodifikasitgl, abisclose, 
        'abcustomtext1, abcustomtext2, abcustomtext3, abcustomtext4, abcustomtext5, abcustomint1, abcustomint2, 
        'abcustomint3, abcustomdbl1, abcustomdbl2, abcustomdbl3, abcustomdate1, abcustomdate2, abcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 56) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'abasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "abasalbarangkategori required numeric." : GoTo selesai
        End If
        'abjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "abjenispembeliankategori required numeric." : GoTo selesai
        End If
        'abcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "abcarabayar required numeric." : GoTo selesai
        End If
        'abautonotransaksi(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "abautonotransaksi required numeric." : GoTo selesai
        End If
        'abtgl(13) As Date
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "abtgl required date." : GoTo selesai
        End If
        'abtglnoref(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "abtglnoref required date." : GoTo selesai
        End If
        'abtglpenutupan(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "abtglpenutupan required date." : GoTo selesai
        End If
        'abidaq1statusao(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "abidaq1statusao required numeric." : GoTo selesai
        End If
        'abidaq2statusao(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "abidaq2statusao required numeric." : GoTo selesai
        End If
        'abidaq3statusao(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "abidaq3statusao required numeric." : GoTo selesai
        End If
        'abidaq4statusao(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "abidaq4statusao required numeric." : GoTo selesai
        End If
        'abidaq5statusao(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "abidaq5statusao required numeric." : GoTo selesai
        End If
        'abstatus(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "abstatus required numeric." : GoTo selesai
        End If
        'abstatussebelumnya(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "abstatussebelumnya required numeric." : GoTo selesai
        End If
        'abjmlrevisi(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "abjmlrevisi required numeric." : GoTo selesai
        End If
        'abcetakanke(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "abcetakanke required numeric." : GoTo selesai
        End If
        'abinputtgl(38) As DateTime
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "abinputtgl required date." : GoTo selesai
        End If
        'abmodifikasitgl(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "abmodifikasitgl required date." : GoTo selesai
        End If
        'abisclose(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "abisclose required numeric." : GoTo selesai
        End If
        'abcustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "abcustomint1 required numeric." : GoTo selesai
        End If
        'abcustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "abcustomint2 required numeric." : GoTo selesai
        End If
        'abcustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "abcustomint3 required numeric." : GoTo selesai
        End If
        'abcustomdbl1(50) As Double
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "abcustomdbl1 required numeric." : GoTo selesai
        End If
        'abcustomdbl2(51) As Double
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "abcustomdbl2 required numeric." : GoTo selesai
        End If
        'abcustomdbl3(52) As Double
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "abcustomdbl3 required numeric." : GoTo selesai
        End If
        'abcustomdate1(53) As Date
        If (IsDate(dataUtama(53)) = False) Then
            result(2) = "abcustomdate1 required date." : GoTo selesai
        End If
        'abcustomdate2(54) As Date
        If (IsDate(dataUtama(54)) = False) Then
            result(2) = "abcustomdate2 required date." : GoTo selesai
        End If
        'abcustomdate3(55) As Date
        If (IsDate(dataUtama(55)) = False) Then
            result(2) = "abcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'abid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "abid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "abid should not be more than 20 character." : GoTo selesai
        End If

        'abcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "abcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "abcabang should not be more than 25 character." : GoTo selesai
        End If

        'ablokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ablokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ablokasi should not be more than 25 character." : GoTo selesai
        End If

        'abgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "abgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "abgudang should not be more than 25 character." : GoTo selesai
        End If

        'absumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "absumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "absumber should not be more than 10 character." : GoTo selesai
        End If

        'abnotransaksi(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "abnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 50 Then
            result(2) = "abnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'abtgl(13) As Date
        If Len(dataUtama(13)) = 0 Then
            result(2) = "abtgl can't be empty" : GoTo selesai
        End If

        'abkodepa(14) As 
        If Len(dataUtama(14)) = 0 Then
            result(2) = "abkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(14)) > 20 Then
            result(2) = "abkodepa should not be more than 20 character." : GoTo selesai
        End If

        'abbagianperbandingan(15) As 
        If Len(dataUtama(15)) = 0 Then
            result(2) = "abbagianperbandingan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 20 Then
            result(2) = "abbagianperbandingan should not be more than 20 character." : GoTo selesai
        End If

        'abtglnoref(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "abtglnoref can't be empty" : GoTo selesai
        End If

        'abtglpenutupan(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "abtglpenutupan can't be empty" : GoTo selesai
        End If

        'abmatauang(22) As String
        If Len(dataUtama(22)) = 0 Then
            result(2) = "abmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(22)) > 25 Then
            result(2) = "abmatauang should not be more than 25 character." : GoTo selesai
        End If

        'abidaq1(23) As 
        If Len(dataUtama(23)) = 0 Then
            result(2) = "abidaq1 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(23)) > 20 Then
            result(2) = "abidaq1 should not be more than 20 character." : GoTo selesai
        End If

        'abidaq2(24) As 
        If Len(dataUtama(24)) = 0 Then
            result(2) = "abidaq2 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 20 Then
            result(2) = "abidaq2 should not be more than 20 character." : GoTo selesai
        End If

        'abidaq3(25) As 
        If Len(dataUtama(25)) = 0 Then
            result(2) = "abidaq3 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(25)) > 20 Then
            result(2) = "abidaq3 should not be more than 20 character." : GoTo selesai
        End If

        'abidaq4(26) As 
        If Len(dataUtama(26)) = 0 Then
            result(2) = "abidaq4 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 20 Then
            result(2) = "abidaq4 should not be more than 20 character." : GoTo selesai
        End If

        'abidaq5(27) As 
        If Len(dataUtama(27)) = 0 Then
            result(2) = "abidaq5 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 20 Then
            result(2) = "abidaq5 should not be more than 20 character." : GoTo selesai
        End If

        'abinputuser(37) As 
        If Len(dataUtama(37)) = 0 Then
            result(2) = "abinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(37)) > 20 Then
            result(2) = "abinputuser should not be more than 20 character." : GoTo selesai
        End If

        'abinputtgl(38) As DateTime
        If Len(dataUtama(38)) = 0 Then
            result(2) = "abinputtgl can't be empty" : GoTo selesai
        End If

        'abmodifikasiuser(39) As 
        If Len(dataUtama(39)) = 0 Then
            result(2) = "abmodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 20 Then
            result(2) = "abmodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'abmodifikasitgl(40) As DateTime
        If Len(dataUtama(40)) = 0 Then
            result(2) = "abmodifikasitgl can't be empty" : GoTo selesai
        End If

        'abcustomdbl1(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "abcustomdbl1 can't be empty" : GoTo selesai
        End If

        'abcustomdbl2(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "abcustomdbl2 can't be empty" : GoTo selesai
        End If

        'abcustomdbl3(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "abcustomdbl3 can't be empty" : GoTo selesai
        End If

        'abcustomdate1(53) As Date
        If Len(dataUtama(53)) = 0 Then
            result(2) = "abcustomdate1 can't be empty" : GoTo selesai
        End If

        'abcustomdate2(54) As Date
        If Len(dataUtama(54)) = 0 Then
            result(2) = "abcustomdate2 can't be empty" : GoTo selesai
        End If

        'abcustomdate3(55) As Date
        If Len(dataUtama(55)) = 0 Then
            result(2) = "abcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "abid", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ablokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "absumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abnogrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abkodepa", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abbagianperbandingan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abbagianperbandingankontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aburaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abidaq1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abidaq2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abidaq3", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abidaq4", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abidaq5", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abidaq1statusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abidaq2statusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abidaq3statusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abidaq4statusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abidaq5statusao", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abinputuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abmodifikasiuser", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "abmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "abcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "abcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "abid~abcabang~ablokasi~abgudang~abasalbarang~abasalbarangkategori~abjenispembelian~abjenispembeliankategori~abcarabayar~absumber~abnogrup~abautonotransaksi~abnotransaksi~abtgl~abkodepa~abbagianperbandingan~abbagianperbandingankontak~aburaian~abcatatan~abnoref~abtglnoref~abtglpenutupan~abmatauang~abidaq1~abidaq2~abidaq3~abidaq4~abidaq5~abidaq1statusao~abidaq2statusao~abidaq3statusao~abidaq4statusao~abidaq5statusao~abstatus~abstatussebelumnya~abjmlrevisi~abcetakanke~abinputuser~abinputtgl~abmodifikasiuser~abmodifikasitgl~abisclose~abcustomtext1~abcustomtext2~abcustomtext3~abcustomtext4~abcustomtext5~abcustomint1~abcustomint2~abcustomint3~abcustomdbl1~abcustomdbl2~abcustomdbl3~abcustomdate1~abcustomdate2~abcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idabdetail(0) As , idab(1) As , idaqdetail(2) As , terpilih(3) As Integer, hargake(4) As Integer, 
        'catatan(5) As String, urutan(6) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idabdetail, idab, idaqdetail, terpilih, hargake, catatan, urutan


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idabdetail", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idab", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "idaqdetail", AsEnumTypeData.AsDouble)
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
            'idabdetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idabdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idabdetail should not be more than 20 character." : GoTo selesai
            End If

            'idab(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idab can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idab should not be more than 20 character." : GoTo selesai
            End If

            'idaqdetail(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - idaqdetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - idaqdetail should not be more than 20 character." : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idabdetail~idab~idaqdetail~terpilih~hargake~catatan~urutan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6)) = False Then
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
                Dim dr1 As DataRow = dtutama.Rows(0)
                If isUpdate Then
                    result(4) = dr1("abid")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(abid) FROM M7_Ab WHERE abid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M7_Ab set abcabang  = '" & FixQuotes(dr1("abcabang")) & "', ablokasi  = '" & FixQuotes(dr1("ablokasi")) & "', abgudang  = '" & FixQuotes(dr1("abgudang")) & "', abasalbarang  = '" & FixQuotes(dr1("abasalbarang")) & "', abasalbarangkategori  = " & dr1("abasalbarangkategori") & ", abjenispembelian  = '" & FixQuotes(dr1("abjenispembelian")) & "', abjenispembeliankategori  = " & dr1("abjenispembeliankategori") & ", abcarabayar  = " & dr1("abcarabayar") & ", absumber  = '" & FixQuotes(dr1("absumber")) & "', abnogrup  = '" & FixQuotes(dr1("abnogrup")) & "', abautonotransaksi  = " & dr1("abautonotransaksi") & ", abnotransaksi  = '" & FixQuotes(dr1("abnotransaksi")) & "', abtgl  = '" & FixQuotes(AsFormatTanggal(dr1("abtgl"))) & "', abkodepa  = '" & FixQuotes(dr1("abkodepa")) & "', abbagianperbandingan  = '" & FixQuotes(dr1("abbagianperbandingan")) & "', abbagianperbandingankontak  = '" & FixQuotes(dr1("abbagianperbandingankontak")) & "', aburaian  = '" & FixQuotes(dr1("aburaian")) & "', abcatatan  = '" & FixQuotes(dr1("abcatatan")) & "', abnoref  = '" & FixQuotes(dr1("abnoref")) & "', abtglnoref  = '" & FixQuotes(AsFormatTanggal(dr1("abtglnoref"))) & "', abtglpenutupan  = '" & FixQuotes(AsFormatTanggal(dr1("abtglpenutupan"))) & "', abmatauang  = '" & FixQuotes(dr1("abmatauang")) & "', abidaq1  = '" & FixQuotes(dr1("abidaq1")) & "', abidaq2  = '" & FixQuotes(dr1("abidaq2")) & "', abidaq3  = '" & FixQuotes(dr1("abidaq3")) & "', abidaq4  = '" & FixQuotes(dr1("abidaq4")) & "', abidaq5  = '" & FixQuotes(dr1("abidaq5")) & "', abidaq1statusao  = " & dr1("abidaq1statusao") & ", abidaq2statusao  = " & dr1("abidaq2statusao") & ", abidaq3statusao  = " & dr1("abidaq3statusao") & ", abidaq4statusao  = " & dr1("abidaq4statusao") & ", abidaq5statusao  = " & dr1("abidaq5statusao") & ", abstatus  = " & dr1("abstatus") & ", abstatussebelumnya  = " & dr1("abstatussebelumnya") & ", abjmlrevisi  = " & dr1("abjmlrevisi") & ", abcetakanke  = " & dr1("abcetakanke") & ", abinputuser  = '" & FixQuotes(dr1("abinputuser")) & "', abinputtgl  = '" & FixQuotes(AsFormatTanggal(dr1("abinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', abmodifikasiuser  = '" & FixQuotes(dr1("abmodifikasiuser")) & "', abmodifikasitgl  = '" & FixQuotes(AsFormatTanggal(dr1("abmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', abcustomtext1  = '" & FixQuotes(dr1("abcustomtext1")) & "', abcustomtext2  = '" & FixQuotes(dr1("abcustomtext2")) & "', abcustomtext3  = '" & FixQuotes(dr1("abcustomtext3")) & "', abcustomtext4  = '" & FixQuotes(dr1("abcustomtext4")) & "', abcustomtext5  = '" & FixQuotes(dr1("abcustomtext5")) & "', abcustomint1  = " & dr1("abcustomint1") & ", abcustomint2  = " & dr1("abcustomint2") & ", abcustomint3  = " & dr1("abcustomint3") & ", abcustomdbl1  = '" & FixDouble(dr1("abcustomdbl1")) & "', abcustomdbl2  = '" & FixDouble(dr1("abcustomdbl2")) & "', abcustomdbl3  = '" & FixDouble(dr1("abcustomdbl3")) & "', abcustomdate1  = '" & FixQuotes(AsFormatTanggal(dr1("abcustomdate1"))) & "', abcustomdate2  = '" & FixQuotes(AsFormatTanggal(dr1("abcustomdate2"))) & "', abcustomdate3  = '" & FixQuotes(AsFormatTanggal(dr1("abcustomdate3"))) & "' where abid = " & dr1("abid") & ""
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
                    sql = "Insert into M7_Ab (abcabang, ablokasi, abgudang, abasalbarang, abasalbarangkategori, abjenispembelian, abjenispembeliankategori, abcarabayar, absumber, abnogrup, abautonotransaksi, abnotransaksi, abtgl, abkodepa, abbagianperbandingan, abbagianperbandingankontak, aburaian, abcatatan, abnoref, abtglnoref, abtglpenutupan, abmatauang, abidaq1, abidaq2, abidaq3, abidaq4, abidaq5, abidaq1statusao, abidaq2statusao, abidaq3statusao, abidaq4statusao, abidaq5statusao, abstatus, abstatussebelumnya, abjmlrevisi, abcetakanke, abinputuser, abinputtgl, abmodifikasiuser, abmodifikasitgl, abisclose, abcustomtext1, abcustomtext2, abcustomtext3, abcustomtext4, abcustomtext5, abcustomint1, abcustomint2, abcustomint3, abcustomdbl1, abcustomdbl2, abcustomdbl3, abcustomdate1, abcustomdate2, abcustomdate3) values('" & FixQuotes(dr1("abcabang")) & "', '" & FixQuotes(dr1("ablokasi")) & "', '" & FixQuotes(dr1("abgudang")) & "', '" & FixQuotes(dr1("abasalbarang")) & "', " & dr1("abasalbarangkategori") & ", '" & FixQuotes(dr1("abjenispembelian")) & "', " & dr1("abjenispembeliankategori") & ", " & dr1("abcarabayar") & ", '" & FixQuotes(dr1("absumber")) & "', '" & FixQuotes(dr1("abnogrup")) & "', " & dr1("abautonotransaksi") & ", '" & FixQuotes(dr1("abnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(dr1("abtgl"))) & "', '" & FixQuotes(dr1("abkodepa")) & "', '" & FixQuotes(dr1("abbagianperbandingan")) & "', '" & FixQuotes(dr1("abbagianperbandingankontak")) & "', '" & FixQuotes(dr1("aburaian")) & "', '" & FixQuotes(dr1("abcatatan")) & "', '" & FixQuotes(dr1("abnoref")) & "', '" & FixQuotes(AsFormatTanggal(dr1("abtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("abtglpenutupan"))) & "', '" & FixQuotes(dr1("abmatauang")) & "', '" & FixQuotes(dr1("abidaq1")) & "', '" & FixQuotes(dr1("abidaq2")) & "', '" & FixQuotes(dr1("abidaq3")) & "', '" & FixQuotes(dr1("abidaq4")) & "', '" & FixQuotes(dr1("abidaq5")) & "', " & dr1("abidaq1statusao") & ", " & dr1("abidaq2statusao") & ", " & dr1("abidaq3statusao") & ", " & dr1("abidaq4statusao") & ", " & dr1("abidaq5statusao") & ", " & dr1("abstatus") & ", " & dr1("abstatussebelumnya") & ", " & dr1("abjmlrevisi") & ", " & dr1("abcetakanke") & ", '" & FixQuotes(dr1("abinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("abinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("abmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("abmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & dr1("abisclose") & ", '" & FixQuotes(dr1("abcustomtext1")) & "', '" & FixQuotes(dr1("abcustomtext2")) & "', '" & FixQuotes(dr1("abcustomtext3")) & "', '" & FixQuotes(dr1("abcustomtext4")) & "', '" & FixQuotes(dr1("abcustomtext5")) & "', " & dr1("abcustomint1") & ", " & dr1("abcustomint2") & ", " & dr1("abcustomint3") & ", '" & FixDouble(dr1("abcustomdbl1")) & "', '" & FixDouble(dr1("abcustomdbl2")) & "', '" & FixDouble(dr1("abcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("abcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("abcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("abcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select abid from M7_Ab where Abinputuser= '" & userid & "' order by Abmodifikasitgl desc limit 1")
                    result(4) = dt2.Rows(0)(0)
                End If
            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            'Hapus detail ketika update
            If (isUpdate) Then
                sql = "Delete from M7_Ab_Detail where idab = " & result(4)
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
                If isUpdate Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idabdetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("idaqdetail")) & "', " & dr1("terpilih") & ", " & dr1("hargake") & ", '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                    Next
                    sql = "Insert into M7_Ab_Detail(idabdetail, idab, idaqdetail, terpilih, hargake, catatan, urutan) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("idaqdetail")) & "', " & dr1("terpilih") & ", " & dr1("hargake") & ", '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ")")
                    Next
                    sql = "Insert into M7_Ab_Detail(idab, idaqdetail, terpilih, hargake, catatan, urutan) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                ''Hapus memchaced
                'AsMemcached.Remove("apliksasi1-M7_Ab~M7_Ab_Detail-" & result(4))

            Else
                result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

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

End Class
