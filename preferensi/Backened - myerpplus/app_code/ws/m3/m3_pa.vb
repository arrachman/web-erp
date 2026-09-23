Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_pa
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_PaSimpan(ByVal param As String) As String
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
        'paid(0) As Integer, pacabang(1) As String, palokasi(2) As String, pagudang(3) As String, pasumber(4) As String, 
        'paautonotransaksi(5) As Integer, panotransaksi(6) As String, patgl(7) As Date, patglberlakusampai(8) As Date, pakodepa(9) As Integer, 
        'pabagianpa(10) As Integer, pabagianpakontak(11) As String, pamatauang(12) As String, pakurs(13) As Double, pauraian(14) As String, 
        'pacatatan(15) As String, panoref(16) As String, patglnoref(17) As Date, pastatus(18) As Integer, pastatussebelumnya(19) As Integer, 
        'pajmlrevisi(20) As Integer, pacetakanke(21) As Integer, painputuser(22) As Integer, painputtgl(23) As DateTime, pamodifikasiuser(24) As Integer, 
        'pamodifikasitgl(25) As DateTime, paposting(26) As Integer, patutupperiode(27) As Integer, paisclose(28) As Integer, pacustomtext1(29) As String, 
        'pacustomtext2(30) As String, pacustomtext3(31) As String, pacustomtext4(32) As String, pacustomtext5(33) As String, pacustomint1(34) As Integer, 
        'pacustomint2(35) As Integer, pacustomint3(36) As Integer, pacustomdbl1(37) As Double, pacustomdbl2(38) As Double, pacustomdbl3(39) As Double, 
        'pacustomdate1(40) As Date, pacustomdate2(41) As Date, pacustomdate3(42) As Date, pakategori(43) As Integer, pakategoriharga(44) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, 
        'patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, 
        'pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, 
        'pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, patutupperiode, 
        'paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, 
        'pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, 
        'pacustomdate3, pakategori, pakategoriharga

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'paid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "paid required numeric." : GoTo selesai
        End If
        'paautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "paautonotransaksi required numeric." : GoTo selesai
        End If
        'patgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "patgl required date." : GoTo selesai
        End If
        'patglberlakusampai(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "patglberlakusampai required date." : GoTo selesai
        End If
        'pakodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "pakodepa required numeric." : GoTo selesai
        End If
        'pabagianpa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "pabagianpa required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "pabagianpa can't be empty." : GoTo selesai
        End If
        'pakurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pakurs required numeric." : GoTo selesai
        End If
        'patglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "patglnoref required date." : GoTo selesai
        End If
        'pastatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pastatus required numeric." : GoTo selesai
        End If
        'pastatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pastatussebelumnya required numeric." : GoTo selesai
        End If
        'pajmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "pajmlrevisi required numeric." : GoTo selesai
        End If
        'pacetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "pacetakanke required numeric." : GoTo selesai
        End If
        'painputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "painputuser required numeric." : GoTo selesai
        End If
        'painputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "painputtgl required date." : GoTo selesai
        End If
        'pamodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "pamodifikasiuser required numeric." : GoTo selesai
        End If
        'pamodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pamodifikasitgl required date." : GoTo selesai
        End If
        'paposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "paposting required numeric." : GoTo selesai
        End If
        'patutupperiode(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "patutupperiode required numeric." : GoTo selesai
        End If
        'paisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "paisclose required numeric." : GoTo selesai
        End If
        'pacustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pacustomint1 required numeric." : GoTo selesai
        End If
        'pacustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pacustomint2 required numeric." : GoTo selesai
        End If
        'pacustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pacustomint3 required numeric." : GoTo selesai
        End If
        'pacustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pacustomdbl1 required numeric." : GoTo selesai
        End If
        'pacustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pacustomdbl2 required numeric." : GoTo selesai
        End If
        'pacustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pacustomdbl3 required numeric." : GoTo selesai
        End If
        'pacustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "pacustomdate1 required date." : GoTo selesai
        End If
        'pacustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "pacustomdate2 required date." : GoTo selesai
        End If
        'pacustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "pacustomdate3 required date." : GoTo selesai
        End If
        'pakategori(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pakategori required numeric." : GoTo selesai
        Else
            If dataUtama(43) <> 0 And dataUtama(43) <> 1 And dataUtama(43) <> 2 Then
                result(2) = "Invalid pakategori value." : GoTo selesai
            End If
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pacabang should not be more than 25 character." : GoTo selesai
        End If

        'palokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "palokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "palokasi should not be more than 25 character." : GoTo selesai
        End If

        'pasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "pasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "pasumber should not be more than 10 character." : GoTo selesai
        End If

        'panotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "panotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "panotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'patgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "patgl can't be empty" : GoTo selesai
        End If

        'patglberlakusampai(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "patglberlakusampai can't be empty" : GoTo selesai
        End If

        'pabagianpakontak(11) As String
        'If Len(dataUtama(11)) = 0 Then
        '    result(2) = "pabagianpakontak can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(11)) > 100 Then
            result(2) = "pabagianpakontak should not be more than 100 character." : GoTo selesai
        End If

        'pamatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pamatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "pamatauang should not be more than 25 character." : GoTo selesai
        End If

        'pakurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "pakurs can't be empty" : GoTo selesai
        End If

        'patglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "patglnoref can't be empty" : GoTo selesai
        End If

        'painputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "painputtgl can't be empty" : GoTo selesai
        End If

        'pamodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pamodifikasitgl can't be empty" : GoTo selesai
        End If

        'pacustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pacustomdbl1 can't be empty" : GoTo selesai
        End If

        'pacustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pacustomdbl2 can't be empty" : GoTo selesai
        End If

        'pacustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pacustomdbl3 can't be empty" : GoTo selesai
        End If

        'pacustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pacustomdate1 can't be empty" : GoTo selesai
        End If

        'pacustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pacustomdate2 can't be empty" : GoTo selesai
        End If

        'pacustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pacustomdate3 can't be empty" : GoTo selesai
        End If

        'pakategoriharga(44) As String
        If dataUtama(43) = 1 And Len(dataUtama(44)) = 0 Then
            result(2) = "pakategoriharga can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(44)) > 25 Then
            result(2) = "pakategoriharga should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "paid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "palokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "paautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "panotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "patgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "patglberlakusampai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pabagianpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pabagianpakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pamatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "panoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "patglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "painputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "painputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "paposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "patutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "paisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pakategoriharga", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "paid~pacabang~palokasi~pagudang~pasumber~paautonotransaksi~panotransaksi~patgl~patglberlakusampai~pakodepa~pabagianpa~pabagianpakontak~pamatauang~pakurs~pauraian~pacatatan~panoref~patglnoref~pastatus~pastatussebelumnya~pajmlrevisi~pacetakanke~painputuser~painputtgl~pamodifikasiuser~pamodifikasitgl~paposting~patutupperiode~paisclose~pacustomtext1~pacustomtext2~pacustomtext3~pacustomtext4~pacustomtext5~pacustomint1~pacustomint2~pacustomint3~pacustomdbl1~pacustomdbl2~pacustomdbl3~pacustomdate1~pacustomdate2~pacustomdate3~pakategori~pakategoriharga", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpadetail(0) As Integer, idpa(1) As Integer, idbarang(2) As Integer, satuan(3) As String, nilaisatuan(4) As Double, 
        'satuanbarang(5) As String, matauang(6) As String, kurs(7) As Double, hargajual1lama(8) As Double, hargajual2lama(9) As Double, 
        'hargajual3lama(10) As Double, hargajual4lama(11) As Double, hargajual5lama(12) As Double, hargajual1(13) As Double, hargajual2(14) As Double, 
        'hargajual3(15) As Double, hargajual4(16) As Double, hargajual5(17) As Double, diskonjual1lama(18) As Double, diskonjual2lama(19) As Double, 
        'diskonjual3lama(20) As Double, diskonjual4lama(21) As Double, diskonjual5lama(22) As Double, diskonjual1(23) As Double, diskonjual2(24) As Double, 
        'diskonjual3(25) As Double, diskonjual4(26) As Double, diskonjual5(27) As Double, cabang(28) As String, lokasi(29) As String, 
        'gudang(30) As String, costcenter(31) As String, divisi(32) As String, subdivisi(33) As String, proyek(34) As String, 
        'catatan(35) As String, urutan(36) As Integer, statusberlaku(37) As Integer, isclose(38) As Integer, customtext1(39) As String, 
        'customtext2(40) As String, customtext3(41) As String, customdbl1(42) As Double, customdbl2(43) As Double, customdbl3(44) As Double, 
        'customdate1(45) As Date, customdate2(46) As Date, customdate3(47) As Date, kontak(48) As Integer,
        'hargajual6lama(49) As Double, hargajual7lama(50) As Double, hargajual8lama(51) As Double, hargajual9lama(52) As Double, hargajual10lama(53) As Double, 
        'hargajual6(54) As Double, hargajual7(55) As Double, hargajual8(56) As Double, hargajual9(57) As Double, hargajual10(58) As Double, 
        'diskonjual6lama(59) As Double, diskonjual7lama(60) As Double, diskonjual8lama(61) As Double, diskonjual9lama(62) As Double, diskonjual10lama(63) As Double, 
        'diskonjual6(64) As Double, diskonjual7(65) As Double, diskonjual8(66) As Double, diskonjual9(67) As Double, diskonjual10(68) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpadetail, idpa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, 
        'kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, 
        'hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, 
        'diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, 
        'cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontak,
        'hargajual6lama, hargajual7lama, hargajual8lama, hargajual9lama, hargajual10lama,
        'hargajual6, hargajual7, hargajual8, hargajual9, hargajual10,
        'diskonjual6lama, diskonjual7lama, diskonjual8lama, diskonjual9lama, diskonjual10lama,
        'diskonjual6, diskonjual7, diskonjual8, diskonjual9, diskonjual10

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual1lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual2lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual3lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual4lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual5lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual1lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual2lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual3lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual4lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual5lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusberlaku", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hargajual6lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual7lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual8lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual9lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual10lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual6lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual7lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual8lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual9lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual10lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual10", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 49 And dataRowDetail.Length <> 69) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpadetail required numeric." : GoTo selesai
            End If
            'idpa(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpa required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'nilaisatuan(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'kurs(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargajual1lama(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - hargajual1lama required numeric." : GoTo selesai
            End If
            'hargajual2lama(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - hargajual2lama required numeric." : GoTo selesai
            End If
            'hargajual3lama(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - hargajual3lama required numeric." : GoTo selesai
            End If
            'hargajual4lama(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - hargajual4lama required numeric." : GoTo selesai
            End If
            'hargajual5lama(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargajual5lama required numeric." : GoTo selesai
            End If
            'hargajual1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - hargajual1 required numeric." : GoTo selesai
            End If
            'hargajual2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - hargajual2 required numeric." : GoTo selesai
            End If
            'hargajual3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hargajual3 required numeric." : GoTo selesai
            End If
            'hargajual4(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - hargajual4 required numeric." : GoTo selesai
            End If
            'hargajual5(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - hargajual5 required numeric." : GoTo selesai
            End If
            ''diskonjual1lama(18) As Double
            'If (IsNumeric(dataRowDetail(18)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual1lama required numeric." : GoTo selesai
            'End If
            ''diskonjual2lama(19) As Double
            'If (IsNumeric(dataRowDetail(19)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual2lama required numeric." : GoTo selesai
            'End If
            ''diskonjual3lama(20) As Double
            'If (IsNumeric(dataRowDetail(20)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual3lama required numeric." : GoTo selesai
            'End If
            ''diskonjual4lama(21) As Double
            'If (IsNumeric(dataRowDetail(21)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual4lama required numeric." : GoTo selesai
            'End If
            ''diskonjual5lama(22) As Double
            'If (IsNumeric(dataRowDetail(22)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual5lama required numeric." : GoTo selesai
            'End If
            ''diskonjual1(23) As Double
            'If (IsNumeric(dataRowDetail(23)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual1 required numeric." : GoTo selesai
            'End If
            ''diskonjual2(24) As Double
            'If (IsNumeric(dataRowDetail(24)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual2 required numeric." : GoTo selesai
            'End If
            ''diskonjual3(25) As Double
            'If (IsNumeric(dataRowDetail(25)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual3 required numeric." : GoTo selesai
            'End If
            ''diskonjual4(26) As Double
            'If (IsNumeric(dataRowDetail(26)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual4 required numeric." : GoTo selesai
            'End If
            ''diskonjual5(27) As Double
            'If (IsNumeric(dataRowDetail(27)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual5 required numeric." : GoTo selesai
            'End If
            'urutan(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'statusberlaku(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statusberlaku required numeric." : GoTo selesai
            End If
            'isclose(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(43) As Double
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(45) As Date
            If (IsDate(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(46) As Date
            If (IsDate(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(47) As Date
            If (IsDate(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'kontak(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If

            If dataRowDetail.Length > 49 Then

                'hargajual6lama(49) As Double
                If (IsNumeric(dataRowDetail(49)) = False) Then
                    result(2) = "Row : " & i & " - hargajual6lama required numeric." : GoTo selesai
                End If
                'hargajual7lama(50) As Double
                If (IsNumeric(dataRowDetail(50)) = False) Then
                    result(2) = "Row : " & i & " - hargajual7lama required numeric." : GoTo selesai
                End If
                'hargajual8lama(51) As Double
                If (IsNumeric(dataRowDetail(51)) = False) Then
                    result(2) = "Row : " & i & " - hargajual8lama required numeric." : GoTo selesai
                End If
                'hargajual9lama(52) As Double
                If (IsNumeric(dataRowDetail(52)) = False) Then
                    result(2) = "Row : " & i & " - hargajual9lama required numeric." : GoTo selesai
                End If
                'hargajual10lama(53) As Double
                If (IsNumeric(dataRowDetail(53)) = False) Then
                    result(2) = "Row : " & i & " - hargajual10lama required numeric." : GoTo selesai
                End If
                'hargajual6(54) As Double
                If (IsNumeric(dataRowDetail(54)) = False) Then
                    result(2) = "Row : " & i & " - hargajual6 required numeric." : GoTo selesai
                End If
                'hargajual7(55) As Double
                If (IsNumeric(dataRowDetail(55)) = False) Then
                    result(2) = "Row : " & i & " - hargajual7 required numeric." : GoTo selesai
                End If
                'hargajual8(56) As Double
                If (IsNumeric(dataRowDetail(56)) = False) Then
                    result(2) = "Row : " & i & " - hargajual8 required numeric." : GoTo selesai
                End If
                'hargajual9(57) As Double
                If (IsNumeric(dataRowDetail(57)) = False) Then
                    result(2) = "Row : " & i & " - hargajual9 required numeric." : GoTo selesai
                End If
                'hargajual10(58) As Double
                If (IsNumeric(dataRowDetail(58)) = False) Then
                    result(2) = "Row : " & i & " - hargajual10 required numeric." : GoTo selesai
                End If

            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'satuan(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'satuanbarang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'hargajual1lama(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - hargajual1lama can't be empty" : GoTo selesai
            End If

            'hargajual2lama(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - hargajual2lama can't be empty" : GoTo selesai
            End If

            'hargajual3lama(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - hargajual3lama can't be empty" : GoTo selesai
            End If

            'hargajual4lama(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - hargajual4lama can't be empty" : GoTo selesai
            End If

            'hargajual5lama(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - hargajual5lama can't be empty" : GoTo selesai
            End If

            'hargajual1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - hargajual1 can't be empty" : GoTo selesai
            End If

            'hargajual2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - hargajual2 can't be empty" : GoTo selesai
            End If

            'hargajual3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hargajual3 can't be empty" : GoTo selesai
            End If

            'hargajual4(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - hargajual4 can't be empty" : GoTo selesai
            End If

            'hargajual5(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - hargajual5 can't be empty" : GoTo selesai
            End If

            'diskonjual1lama(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual1lama can't be empty" : GoTo selesai
            End If

            'diskonjual2lama(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual2lama can't be empty" : GoTo selesai
            End If

            'diskonjual3lama(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual3lama can't be empty" : GoTo selesai
            End If

            'diskonjual4lama(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual4lama can't be empty" : GoTo selesai
            End If

            'diskonjual5lama(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual5lama can't be empty" : GoTo selesai
            End If

            'diskonjual1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual1 can't be empty" : GoTo selesai
            End If

            'diskonjual2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual2 can't be empty" : GoTo selesai
            End If

            'diskonjual3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual3 can't be empty" : GoTo selesai
            End If

            'diskonjual4(26) As Double
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual4 can't be empty" : GoTo selesai
            End If

            'diskonjual5(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual5 can't be empty" : GoTo selesai
            End If

            'customdbl1(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(43) As Double
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(45) As Date
            If Len(dataRowDetail(45)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(46) As Date
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(47) As Date
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'kontak(48) As Integer
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - contact can't be empty" : GoTo selesai
            End If

            If dataRowDetail.Length > 49 Then

                'hargajual6lama(49) As Double
                If Len(dataRowDetail(49)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual6lama can't be empty" : GoTo selesai
                End If

                'hargajual7lama(50) As Double
                If Len(dataRowDetail(50)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual7lama can't be empty" : GoTo selesai
                End If

                'hargajual8lama(51) As Double
                If Len(dataRowDetail(51)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual8lama can't be empty" : GoTo selesai
                End If

                'hargajual9lama(52) As Double
                If Len(dataRowDetail(52)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual9lama can't be empty" : GoTo selesai
                End If

                'hargajual10lama(53) As Double
                If Len(dataRowDetail(53)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual10lama can't be empty" : GoTo selesai
                End If

                'hargajual6(54) As Double
                If Len(dataRowDetail(54)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual6 can't be empty" : GoTo selesai
                End If

                'hargajual7(55) As Double
                If Len(dataRowDetail(55)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual7 can't be empty" : GoTo selesai
                End If

                'hargajual8(56) As Double
                If Len(dataRowDetail(56)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual8 can't be empty" : GoTo selesai
                End If

                'hargajual9(57) As Double
                If Len(dataRowDetail(57)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual9 can't be empty" : GoTo selesai
                End If

                'hargajual10(58) As Double
                If Len(dataRowDetail(58)) = 0 Then
                    result(2) = "Row : " & i & " - hargajual10 can't be empty" : GoTo selesai
                End If

                'diskonjual6lama(59) As Double
                If Len(dataRowDetail(59)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual6lama can't be empty" : GoTo selesai
                End If

                'diskonjual7lama(60) As Double
                If Len(dataRowDetail(60)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual7lama can't be empty" : GoTo selesai
                End If

                'diskonjual8lama(61) As Double
                If Len(dataRowDetail(61)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual8lama can't be empty" : GoTo selesai
                End If

                'diskonjual9lama(62) As Double
                If Len(dataRowDetail(62)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual9lama can't be empty" : GoTo selesai
                End If

                'diskonjual10lama(63) As Double
                If Len(dataRowDetail(63)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual10lama can't be empty" : GoTo selesai
                End If

                'diskonjual6(64) As Double
                If Len(dataRowDetail(64)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual6 can't be empty" : GoTo selesai
                End If

                'diskonjual7(65) As Double
                If Len(dataRowDetail(65)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual7 can't be empty" : GoTo selesai
                End If

                'diskonjual8(66) As Double
                If Len(dataRowDetail(66)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual8 can't be empty" : GoTo selesai
                End If

                'diskonjual9(67) As Double
                If Len(dataRowDetail(67)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual9 can't be empty" : GoTo selesai
                End If

                'diskonjual10(68) As Double
                If Len(dataRowDetail(68)) = 0 Then
                    result(2) = "Row : " & i & " - diskonjual10 can't be empty" : GoTo selesai
                End If

            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If dataRowDetail.Length > 49 Then
                If AsDataTableTambahData(dtdetail, "idpadetail~idpa~idbarang~satuan~nilaisatuan~satuanbarang~matauang~kurs~hargajual1lama~hargajual2lama~hargajual3lama~hargajual4lama~hargajual5lama~hargajual1~hargajual2~hargajual3~hargajual4~hargajual5~diskonjual1lama~diskonjual2lama~diskonjual3lama~diskonjual4lama~diskonjual5lama~diskonjual1~diskonjual2~diskonjual3~diskonjual4~diskonjual5~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~statusberlaku~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~kontak~hargajual6lama~hargajual7lama~hargajual8lama~hargajual9lama~hargajual10lama~hargajual6~hargajual7~hargajual8~hargajual9~hargajual10~diskonjual6lama~diskonjual7lama~diskonjual8lama~diskonjual9lama~diskonjual10lama~diskonjual6~diskonjual7~diskonjual8~diskonjual9~diskonjual10", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If
            Else
                If AsDataTableTambahData(dtdetail, "idpadetail~idpa~idbarang~satuan~nilaisatuan~satuanbarang~matauang~kurs~hargajual1lama~hargajual2lama~hargajual3lama~hargajual4lama~hargajual5lama~hargajual1~hargajual2~hargajual3~hargajual4~hargajual5~diskonjual1lama~diskonjual2lama~diskonjual3lama~diskonjual4lama~diskonjual5lama~diskonjual1~diskonjual2~diskonjual3~diskonjual4~diskonjual5~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~statusberlaku~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~kontak~hargajual6lama~hargajual7lama~hargajual8lama~hargajual9lama~hargajual10lama~hargajual6~hargajual7~hargajual8~hargajual9~hargajual10~diskonjual6lama~diskonjual7lama~diskonjual8lama~diskonjual9lama~diskonjual10lama~diskonjual6~diskonjual7~diskonjual8~diskonjual9~diskonjual10", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0) = False Then
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
                Dim vModuleId As Integer = 3, vMenuId As Integer = 8
                Select Case drutama("pastatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("patgl")), AsFormatTanggal(drutama("patgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                ''CEK HAK AKSES ==========================================
                'If drutama("pastatus") = 2 Then
                '    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                '    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                '    Dim rsCekHakAkses As String = HakAkses(3, 8, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                '    If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK HAK AKSES ===================================


                If isUpdate Then
                    result(4) = drutama("paid")
                    notransaksi = drutama("panotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(paid), panotransaksi FROM M3_pa WHERE paid='" & result(4) & "' AND pastatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("paautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pacabang"), drutama("palokasi"), drutama("pasumber"), drutama("patgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(paid) FROM m3_pa WHERE panotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_pa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pasumber")) & "▼" & FixQuotes(drutama("paid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Pa set pacabang  = '" & FixQuotes(drutama("pacabang")) & "', palokasi  = '" & FixQuotes(drutama("palokasi")) & "', pagudang  = '" & FixQuotes(drutama("pagudang")) & "', pasumber  = '" & FixQuotes(drutama("pasumber")) & "', paautonotransaksi  = " & drutama("paautonotransaksi") & ", panotransaksi  = '" & notransaksi & "', patgl  = '" & FixQuotes(AsFormatTanggal(drutama("patgl"))) & "', patglberlakusampai  = '" & FixQuotes(AsFormatTanggal(drutama("patglberlakusampai"))) & "', pakodepa  = " & drutama("pakodepa") & ", pabagianpa  = " & drutama("pabagianpa") & ", pabagianpakontak  = '" & FixQuotes(drutama("pabagianpakontak")) & "', pamatauang  = '" & FixQuotes(drutama("pamatauang")) & "', pakurs  = '" & FixDouble(drutama("pakurs")) & "', pauraian  = '" & FixQuotes(drutama("pauraian")) & "', pacatatan  = '" & FixQuotes(drutama("pacatatan")) & "', panoref  = '" & FixQuotes(drutama("panoref")) & "', patglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("patglnoref"))) & "', pastatus  = " & drutama("pastatus") & ", pastatussebelumnya  = " & drutama("pastatussebelumnya") & ", pajmlrevisi  = pajmlrevisi+1, pacetakanke  = " & drutama("pacetakanke") & ", pamodifikasiuser  = " & drutama("pamodifikasiuser") & ", pamodifikasitgl  = NOW(), paposting  = 0, patutupperiode  = " & drutama("patutupperiode") & ", pacustomtext1  = '" & FixQuotes(drutama("pacustomtext1")) & "', pacustomtext2  = '" & FixQuotes(drutama("pacustomtext2")) & "', pacustomtext3  = '" & FixQuotes(drutama("pacustomtext3")) & "', pacustomtext4  = '" & FixQuotes(drutama("pacustomtext4")) & "', pacustomtext5  = '" & FixQuotes(drutama("pacustomtext5")) & "', pacustomint1  = " & drutama("pacustomint1") & ", pacustomint2  = " & drutama("pacustomint2") & ", pacustomint3  = " & drutama("pacustomint3") & ", pacustomdbl1  = '" & FixDouble(drutama("pacustomdbl1")) & "', pacustomdbl2  = '" & FixDouble(drutama("pacustomdbl2")) & "', pacustomdbl3  = '" & FixDouble(drutama("pacustomdbl3")) & "', pacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate1"))) & "', pacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate2"))) & "', pacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate3"))) & "', pakategori = '" & FixQuotes(drutama("pakategori")) & "', pakategoriharga = '" & FixQuotes(drutama("pakategoriharga")) & "' where paid = '" & drutama("paid") & "'"
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

                    If drutama("paautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pacabang"), drutama("palokasi"), drutama("pasumber"), drutama("patgl"))
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
                        notransaksi = drutama("panotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(paid) FROM m3_pa WHERE panotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Pa (pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, patutupperiode, paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, pakategori, pakategoriharga) values('" & FixQuotes(drutama("pacabang")) & "', '" & FixQuotes(drutama("palokasi")) & "', '" & FixQuotes(drutama("pagudang")) & "', '" & FixQuotes(drutama("pasumber")) & "', " & drutama("paautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("patgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("patglberlakusampai"))) & "', " & drutama("pakodepa") & ", " & drutama("pabagianpa") & ", '" & FixQuotes(drutama("pabagianpakontak")) & "', '" & FixQuotes(drutama("pamatauang")) & "', '" & FixDouble(drutama("pakurs")) & "', '" & FixQuotes(drutama("pauraian")) & "', '" & FixQuotes(drutama("pacatatan")) & "', '" & FixQuotes(drutama("panoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("patglnoref"))) & "', " & drutama("pastatus") & ", " & drutama("pastatussebelumnya") & ", " & drutama("pajmlrevisi") & ", " & drutama("pacetakanke") & ", " & drutama("painputuser") & ", NOW(), " & drutama("pamodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("patutupperiode") & ", " & drutama("paisclose") & ", '" & FixQuotes(drutama("pacustomtext1")) & "', '" & FixQuotes(drutama("pacustomtext2")) & "', '" & FixQuotes(drutama("pacustomtext3")) & "', '" & FixQuotes(drutama("pacustomtext4")) & "', '" & FixQuotes(drutama("pacustomtext5")) & "', " & drutama("pacustomint1") & ", " & drutama("pacustomint2") & ", " & drutama("pacustomint3") & ", '" & FixDouble(drutama("pacustomdbl1")) & "', '" & FixDouble(drutama("pacustomdbl2")) & "', '" & FixDouble(drutama("pacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate3"))) & "', '" & FixQuotes(drutama("pakategori")) & "', '" & FixQuotes(drutama("pakategoriharga")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select paid from M3_pa where panotransaksi='" & notransaksi & "' AND painputuser= '" & userid & "' order by pamodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Pa_Detail where idpa = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpadetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargajual1lama")) & "', '" & FixDouble(dr1("hargajual2lama")) & "', '" & FixDouble(dr1("hargajual3lama")) & "', '" & FixDouble(dr1("hargajual4lama")) & "', '" & FixDouble(dr1("hargajual5lama")) & "', '" & FixDouble(dr1("hargajual1")) & "', '" & FixDouble(dr1("hargajual2")) & "', '" & FixDouble(dr1("hargajual3")) & "', '" & FixDouble(dr1("hargajual4")) & "', '" & FixDouble(dr1("hargajual5")) & "', '" & FixDouble(dr1("diskonjual1lama")) & "', '" & FixDouble(dr1("diskonjual2lama")) & "', '" & FixDouble(dr1("diskonjual3lama")) & "', '" & FixDouble(dr1("diskonjual4lama")) & "', '" & FixDouble(dr1("diskonjual5lama")) & "', '" & FixDouble(dr1("diskonjual1")) & "', '" & FixDouble(dr1("diskonjual2")) & "', '" & FixDouble(dr1("diskonjual3")) & "', '" & FixDouble(dr1("diskonjual4")) & "', '" & FixDouble(dr1("diskonjual5")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusberlaku") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', " & dr1("kontak") & ", '" & FixDouble(dr1("hargajual6lama")) & "', '" & FixDouble(dr1("hargajual7lama")) & "', '" & FixDouble(dr1("hargajual8lama")) & "', '" & FixDouble(dr1("hargajual9lama")) & "', '" & FixDouble(dr1("hargajual10lama")) & "', '" & FixDouble(dr1("hargajual6")) & "', '" & FixDouble(dr1("hargajual7")) & "', '" & FixDouble(dr1("hargajual8")) & "', '" & FixDouble(dr1("hargajual9")) & "', '" & FixDouble(dr1("hargajual10")) & "', '" & FixDouble(dr1("diskonjual6lama")) & "', '" & FixDouble(dr1("diskonjual7lama")) & "', '" & FixDouble(dr1("diskonjual8lama")) & "', '" & FixDouble(dr1("diskonjual9lama")) & "', '" & FixDouble(dr1("diskonjual10lama")) & "', '" & FixDouble(dr1("diskonjual6")) & "', '" & FixDouble(dr1("diskonjual7")) & "', '" & FixDouble(dr1("diskonjual8")) & "', '" & FixDouble(dr1("diskonjual9")) & "', '" & FixDouble(dr1("diskonjual10")) & "')")
                    Next
                    sql = "Insert into M3_Pa_Detail(idpadetail, idpa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontak, hargajual6lama, hargajual7lama, hargajual8lama, hargajual9lama, hargajual10lama, hargajual6, hargajual7, hargajual8, hargajual9, hargajual10, diskonjual6lama, diskonjual7lama, diskonjual8lama, diskonjual9lama, diskonjual10lama, diskonjual6, diskonjual7, diskonjual8, diskonjual9, diskonjual10) values" & strValue2.ToString & ""
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


                'UPDATE HARGA KE MASTER DATA BARANG =================================================
                If drutama("pastatus") = 2 Then
                    'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                    'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                    'JIKA PAKATEGORI = 2 (PER KONTAK) MAKA UPDATE HARGA KE M1_CONTACT_PRICE
                    If drutama("pakategori") = 0 Then
                        'UPDATE HARGA LAMA KE TABEL DETAIL (M3_PA_DETAIL)
                        sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid SET pad.hargajual1lama = i.bhargajual1, pad.hargajual2lama = i.bhargajual2, pad.hargajual3lama = i.bhargajual3, pad.hargajual4lama = i.bhargajual4, pad.hargajual5lama = i.bhargajual5, pad.diskonjual1lama = i.bdiskonjual1, pad.diskonjual2lama = i.bdiskonjual2, pad.diskonjual3lama = i.bdiskonjual3, pad.diskonjual4lama = i.bdiskonjual4, pad.diskonjual5lama = i.bdiskonjual5, pad.hargajual6lama = i.bhargajual6, pad.hargajual7lama = i.bhargajual7, pad.hargajual8lama = i.bhargajual8, pad.hargajual9lama = i.bhargajual9, pad.hargajual10lama = i.bhargajual10, pad.diskonjual6lama = i.bdiskonjual6, pad.diskonjual7lama = i.bdiskonjual7, pad.diskonjual8lama = i.bdiskonjual8, pad.diskonjual9lama = i.bdiskonjual9, pad.diskonjual10lama = i.bdiskonjual10 WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HARGA BARU KE TABEL BARANG (M1_ITEM)
                        sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid SET i.bhargajual1 = pad.hargajual1 / pad.nilaisatuan, i.bhargajual2 = pad.hargajual2 / pad.nilaisatuan, i.bhargajual3 = pad.hargajual3 / pad.nilaisatuan, i.bhargajual4 = pad.hargajual4 / pad.nilaisatuan, i.bhargajual5 = pad.hargajual5 / pad.nilaisatuan, i.bdiskonjual1 = pad.diskonjual1, i.bdiskonjual2 = pad.diskonjual2, i.bdiskonjual3 = pad.diskonjual3, i.bdiskonjual4 = pad.diskonjual4, i.bdiskonjual5 = pad.diskonjual5, i.bhargajual6 = pad.hargajual6 / pad.nilaisatuan, i.bhargajual7 = pad.hargajual7 / pad.nilaisatuan, i.bhargajual8 = pad.hargajual8 / pad.nilaisatuan, i.bhargajual9 = pad.hargajual9 / pad.nilaisatuan, i.bhargajual10 = pad.hargajual10 / pad.nilaisatuan, i.bdiskonjual6 = pad.diskonjual6, i.bdiskonjual7 = pad.diskonjual7, i.bdiskonjual8 = pad.diskonjual8, i.bdiskonjual9 = pad.diskonjual9, i.bdiskonjual10 = pad.diskonjual10, i.bhargabeli = pad.customdbl1 WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    ElseIf drutama("pastatus") = 1 Then

                        'UPDATE HARGA LAMA KE TABEL DETAIL (M3_PA_DETAIL)
                        sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid LEFT JOIN m1_price_category_detail pcd ON pad.idbarang = pcd.pcdidbarang AND pcd.pcdkategori = '" & FixQuotes(drutama("pakategoriharga")) & "' SET pad.hargajual1lama = ifnull(pcd.pcdhargajual1, i.bhargajual1), pad.hargajual2lama = ifnull(pcd.pcdhargajual2, i.bhargajual2), pad.hargajual3lama = ifnull(pcd.pcdhargajual3, i.bhargajual3), pad.hargajual4lama = ifnull(pcd.pcdhargajual4, i.bhargajual4), pad.hargajual5lama = ifnull(pcd.pcdhargajual5, i.bhargajual5), pad.diskonjual1lama = ifnull(pcd.pcddiskonjual1, i.bdiskonjual1), pad.diskonjual2lama = ifnull(pcd.pcddiskonjual2, i.bdiskonjual2), pad.diskonjual3lama = ifnull(pcd.pcddiskonjual3, i.bdiskonjual3), pad.diskonjual4lama = ifnull(pcd.pcddiskonjual4, i.bdiskonjual4), pad.diskonjual5lama = ifnull(pcd.pcddiskonjual5, i.bdiskonjual5), pad.hargajual6lama = ifnull(pcd.pcdhargajual6, i.bhargajual6), pad.hargajual7lama = ifnull(pcd.pcdhargajual7, i.bhargajual7), pad.hargajual8lama = ifnull(pcd.pcdhargajual8, i.bhargajual8), pad.hargajual9lama = ifnull(pcd.pcdhargajual9, i.bhargajual9), pad.hargajual10lama = ifnull(pcd.pcdhargajual10, i.bhargajual10), pad.diskonjual6lama = ifnull(pcd.pcddiskonjual6, i.bdiskonjual6), pad.diskonjual7lama = ifnull(pcd.pcddiskonjual7, i.bdiskonjual7), pad.diskonjual8lama = ifnull(pcd.pcddiskonjual8, i.bdiskonjual8), pad.diskonjual9lama = ifnull(pcd.pcddiskonjual9, i.bdiskonjual9), pad.diskonjual10lama = ifnull(pcd.pcddiskonjual10, i.bdiskonjual10) WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HARGA BARU KE TABEL BARANG (M1_PRICE_CATEGORY_DETAIL)
                        sql = "INSERT INTO M1_Price_Category_Detail (SELECT '" & FixQuotes(drutama("pakategoriharga")) & "' as pcdkategori, pad.idbarang as pcdidbarang, i.bstokminimal as pcdstokminimal, i.bstokmaksimal as pcdstokmaksimal, i.breorder as pcdstokreorder, i.bminorder as pcdstokminorder, pad.hargajual1 / pad.nilaisatuan as pcdhargajual1, pad.hargajual2 / pad.nilaisatuan as pcdhargajual2, pad.hargajual3 / pad.nilaisatuan as pcdhargajual3, pad.hargajual4 / pad.nilaisatuan as pcdhargajual4, pad.hargajual5 / pad.nilaisatuan as pcdhargajual5, pad.diskonjual1 as pcddiskonjual1, pad.diskonjual2 as pcddiskonjual2, pad.diskonjual3 as pcddiskonjual3, pad.diskonjual4 as pcddiskonjual4, pad.diskonjual5 as pcddiskonjual5, pad.customtext1 as pcdcustomtext1, pad.customtext2 as pcdcustomtext2, pad.customtext3 as pcdcustomtext3, '' as pcdcustomtext4, '' as pcdcustomtext5, 0 as pcdcustomint1, 0 as pcdcustomint2, 0 as pcdcustomint3, pad.customdbl1 as pcdcustomdbl1, pad.customdbl2 as pcdcustomdbl2, pad.customdbl3 as pcdcustomdbl3, pad.customdate1 as pcdcustomdate1, pad.customdate2 as pcdcustomdate2, pad.customdate3 as pcdcustomdate3, 0 as pcddownloaded, pad.hargajual6 / pad.nilaisatuan as pcdhargajual6, pad.hargajual7 / pad.nilaisatuan as pcdhargajual7, pad.hargajual8 / pad.nilaisatuan as pcdhargajual8, pad.hargajual9 / pad.nilaisatuan as pcdhargajual9, pad.hargajual10 / pad.nilaisatuan as pcdhargajual10, pad.diskonjual6 as pcddiskonjual6, pad.diskonjual7 as pcddiskonjual7, pad.diskonjual8 as pcddiskonjual8, pad.diskonjual9 as pcddiskonjual9, pad.diskonjual10 as pcddiskonjual10 FROM m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid AND pad.idpa = '" & FixDouble(result(4)) & "') ON DUPLICATE KEY UPDATE pcdhargajual1 = VALUES(pcdhargajual1), pcdhargajual2 = VALUES(pcdhargajual2), pcdhargajual3 = VALUES(pcdhargajual3), pcdhargajual4 = VALUES(pcdhargajual4), pcdhargajual5 = VALUES(pcdhargajual5), pcddiskonjual1 = VALUES(pcddiskonjual1), pcddiskonjual2 = VALUES(pcddiskonjual2), pcddiskonjual3 = VALUES(pcddiskonjual3), pcddiskonjual4 = VALUES(pcddiskonjual4), pcddiskonjual5 = VALUES(pcddiskonjual5), pcdhargajual6 = VALUES(pcdhargajual6), pcdhargajual7 = VALUES(pcdhargajual7), pcdhargajual8 = VALUES(pcdhargajual8), pcdhargajual9 = VALUES(pcdhargajual9), pcdhargajual10 = VALUES(pcdhargajual10), pcddiskonjual6 = VALUES(pcddiskonjual6), pcddiskonjual7 = VALUES(pcddiskonjual7), pcddiskonjual8 = VALUES(pcddiskonjual8), pcddiskonjual9 = VALUES(pcddiskonjual9), pcddiskonjual10 = VALUES(pcddiskonjual10)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    ElseIf drutama("pastatus") = 2 Then

                        'UPDATE HARGA LAMA KE TABEL DETAIL (M3_PA_DETAIL)
                        sql = "UPDATE m3_pa_detail pad LEFT JOIN m1_contact_price cp ON pad.idbarang = cp.khidbarang AND pad.kontak = cp.khidkontak SET pad.hargajual1lama = IFNULL(cp.khhargajual,0), pad.hargajual2lama = 0,	pad.hargajual3lama = 0,	pad.hargajual4lama = 0,	pad.hargajual5lama = 0,	pad.diskonjual1lama = 0, pad.diskonjual2lama = 0, pad.diskonjual3lama = 0, pad.diskonjual4lama = 0, pad.diskonjual5lama = 0, pad.hargajual6lama = 0, pad.hargajual7lama = 0,	pad.hargajual8lama = 0,	pad.hargajual9lama = 0,	pad.hargajual10lama = 0,	pad.diskonjual6lama = 0, pad.diskonjual7lama = 0, pad.diskonjual8lama = 0, pad.diskonjual9lama = 0, pad.diskonjual10lama = 0 WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HARGA BARU KE TABEL BARANG (M1_PRICE_CATEGORY_DETAIL)
                        sql = "INSERT INTO m1_contact_price( SELECT pad.kontak as khidkontak, pad.idbarang as khidbarang, pad.satuan as khsatuan, 0 as khkomisi, 0 as khhargabeli, pad.hargajual1 as khhargajual, pa.patgl as khberlakudari, '1900-01-01' as khberlakusampai, '' as khcatatan, pa.painputuser as khinputuser, pa.painputtgl as khinputtgl, pa.pamodifikasiuser as khmodifikasiuser, pa.pamodifikasitgl as khmodifikasitgl, '' as khcustomtext1, '' as khcustomtext2, '' as khcustomtext3, '' as khcustomtext4, '' as khcustomtext5, '0' as khcustomint1, '0' as khcustomint2, '0' as khcustomint3, '0' as khcustomint4, '0' as khcustomint5, '0' as khcustomdbl1, '0' as khcustomdbl2, '0' as khcustomdbl3, '0' as khcustomdbl4, '0' as khcustomdbl5, '1900-01-01' as khcustomdate1, '1900-01-01' as khcustomdate2, '1900-01-01' as khcustomdate3, '1900-01-01' as khcustomdate4, '1900-01-01' as khcustomdate5 FROM m3_pa_detail pad JOIN m3_pa pa  ON pad.idpa = pa.paid WHERE pad.idpa = '" & FixDouble(result(4)) & "' ) ON DUPLICATE KEY UPDATE khidkontak = VALUES(khidkontak), khidbarang = VALUES(khidbarang), khsatuan = VALUES(khsatuan), khhargajual = VALUES(khhargajual), khberlakudari = VALUES(khberlakudari), khinputuser = VALUES(khinputuser), khinputtgl = VALUES(khinputtgl), khmodifikasiuser = VALUES(khmodifikasiuser), khmodifikasitgl = VALUES(khmodifikasitgl)"
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
                'END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "Pa", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_PaUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("pabagianpakode", "c1.kkode")
            Filter = Filter.Replace("pabagianpanama", "c1.knama")
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
            Dim sumber As String = "Pa", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim pakategori As Integer = 0, pakategoriharga As String = ""

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0, '' FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Patgl, Panotransaksi, Pastatus, Pakategori, Pakategoriharga FROM m3_Pa WHERE Paid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1)
                'tgl                                 notransaksi                         status
                tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'kategori                                               kategoriharga       
                pakategori = FixDouble(FxDB(dtdetail.Rows(1)(3), 0)) : pakategoriharga = FixQuotes(FxDB(dtdetail.Rows(1)(4), ""))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_pa_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'UPDATE HARGA KE MASTER DATA BARANG =================================================
                'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                If pakategori = 0 Then
                    'UPDATE HARGA LAMA KE TABEL BARANG (M1_ITEM)
                    sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid SET i.bhargajual1 = pad.hargajual1lama, i.bhargajual2 = pad.hargajual2lama, i.bhargajual3 = pad.hargajual3lama, i.bhargajual4 = pad.hargajual4lama, i.bhargajual5 = pad.hargajual5lama, i.bdiskonjual1 = pad.diskonjual1lama, i.bdiskonjual2 = pad.diskonjual2lama, i.bdiskonjual3 = pad.diskonjual3lama, i.bdiskonjual4 = pad.diskonjual4lama, i.bdiskonjual5 = pad.diskonjual5lama, i.bhargajual6 = pad.hargajual6lama, i.bhargajual7 = pad.hargajual7lama, i.bhargajual8 = pad.hargajual8lama, i.bhargajual9 = pad.hargajual9lama, i.bhargajual10 = pad.hargajual10lama, i.bdiskonjual6 = pad.diskonjual6lama, i.bdiskonjual7 = pad.diskonjual7lama, i.bdiskonjual8 = pad.diskonjual8lama, i.bdiskonjual9 = pad.diskonjual9lama, i.bdiskonjual10 = pad.diskonjual10lama WHERE pad.idpa = '" & FixDouble(idtransaksi) & "'"
                ElseIf pakategori = 1 Then
                    'UPDATE HARGA LAMA KE TABEL HARGA BARANG PER KATEGORI (M1_PRICE_CATEGORY_DETAIL) SESUAI IDBARANG DAN KATEGORI HARGA BARANG
                    sql = "UPDATE m3_pa_detail pad JOIN m1_price_category_detail i ON i.pcdkategori = '" & pakategoriharga & "' AND pad.idbarang = i.pcdidbarang SET i.pcdhargajual1 = pad.hargajual1lama, i.pcdhargajual2 = pad.hargajual2lama, i.pcdhargajual3 = pad.hargajual3lama, i.pcdhargajual4 = pad.hargajual4lama, i.pcdhargajual5 = pad.hargajual5lama, i.pcddiskonjual1 = pad.diskonjual1lama, i.pcddiskonjual2 = pad.diskonjual2lama, i.pcddiskonjual3 = pad.diskonjual3lama, i.pcddiskonjual4 = pad.diskonjual4lama, i.pcddiskonjual5 = pad.diskonjual5lama, i.pcdhargajual6 = pad.hargajual6lama, i.pcdhargajual7 = pad.hargajual7lama, i.pcdhargajual8 = pad.hargajual8lama, i.pcdhargajual9 = pad.hargajual9lama, i.pcdhargajual10 = pad.hargajual10lama, i.pcddiskonjual6 = pad.diskonjual6lama, i.pcddiskonjual7 = pad.diskonjual7lama, i.pcddiskonjual8 = pad.diskonjual8lama, i.pcddiskonjual9 = pad.diskonjual9lama, i.pcddiskonjual10 = pad.diskonjual10lama WHERE pad.idpa = '" & FixDouble(idtransaksi) & "'"
                ElseIf pakategori = 2 Then
                    'UPDATE HARGA LAMA KE TABEL HARGA BARANG PER KONTAK (M1_CONTACT_PRICE) SESUAI IDBARANG DAN IDKONTAK
                    sql = "UPDATE m3_pa_detail pad JOIN m1_contact_price cp ON pad.idbarang = cp.khidbarang AND pad.kontak = cp.khidkontak SET cp.khhargajual = pad.pad.hargajual1lama WHERE pad.idpa = '" & FixDouble(idtransaksi) & "'"
                End If
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================
            End If

            'update status utama
            sql = "UPDATE M3_Pa SET Pastatus = " & nilaiStatus & ", Pamodifikasiuser='" & userid & "', Pamodifikasitgl = NOW(), Paposting = 0, Papostingtgl = '1971-01-01 00:00:00', Pajmlrevisi = Pajmlrevisi + 1 WHERE Paid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_PaSearch(PostWsSearch(paramSplit(0), "M3_PaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_PaDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("pabagianpakode", "c1.kkode")
            Filter = Filter.Replace("pabagianpanama", "c1.knama")
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
            Dim sumber As String = "Pa", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Paid, Panotransaksi FROM m3_Pa WHERE Paid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pacabang, palokasi, pasumber, paautonotransaksi, panotransaksi, patgl"
            sql &= " FROM M3_pa"
            sql &= " WHERE paid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pacabang")
                lokasi = dtNomorNext.Rows(0)("palokasi")
                sumber = dtNomorNext.Rows(0)("pasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("paautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("panotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("patgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Pa_Detail WHERE idpa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Pa WHERE paid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_PaSearch(PostWsSearch(paramSplit(0), "M3_PaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M3_PaGetdataById(ByVal param As String) As String

        'M3_PaGetdataById Utama --------------------------------------------------------
        'paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, 
        'patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, 
        'pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, 
        'pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, 
        'patutupperiode, paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, 
        'pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, 
        'pacustomdate2, pacustomdate3, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, 
        'pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama,
        'pakategori, pakategorinama, pakategoriharga, pakategoriharganama

        'M3_PaGetdataById Detail -------------------------------------------------------
        'idpadetail, idpa, idbarang, 
        'satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, 
        'hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, 
        'hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, 
        'diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, bhargabeli,
        'kontak, kontakkode, kontaknama,
        'hargajual6lama, hargajual7lama, hargajual8lama, hargajual9lama, hargajual10lama, 
        'hargajual6, hargajual7, hargajual8, hargajual9, hargajual10,
        'diskonjual6lama, diskonjual7lama, diskonjual8lama, diskonjual9lama, diskonjual10lama,
        'diskonjual6, diskonjual7, diskonjual8, diskonjual9, diskonjual10


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

        Dim NmMemcached As String = "aplikasi1-M3_Pa~M3_Pa_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "paid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "paid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m3_pa_getdata")
        sql = "select pa.paid AS paid, pa.pacabang AS pacabang, pa.palokasi AS palokasi, pa.pagudang AS pagudang, pa.pasumber AS pasumber, pa.paautonotransaksi AS paautonotransaksi, pa.panotransaksi AS panotransaksi, pa.patgl AS patgl, pa.patglberlakusampai AS patglberlakusampai, pa.pakodepa AS pakodepa, pa.pabagianpa AS pabagianpa, pa.pabagianpakontak AS pabagianpakontak, pa.pamatauang AS pamatauang, pa.pakurs AS pakurs, pa.pauraian AS pauraian, pa.pacatatan AS pacatatan, pa.panoref AS panoref, pa.patglnoref AS patglnoref, pa.pastatus AS pastatus, pa.pastatussebelumnya AS pastatussebelumnya, pa.pajmlrevisi AS pajmlrevisi, pa.pacetakanke AS pacetakanke, pa.painputuser AS painputuser, pa.painputtgl AS painputtgl, pa.pamodifikasiuser AS pamodifikasiuser, pa.pamodifikasitgl AS pamodifikasitgl, pa.paposting AS paposting, pa.papostingtgl AS papostingtgl, pa.patutupperiode AS patutupperiode, pa.paisclose AS paisclose, pa.pacustomtext1 AS pacustomtext1, pa.pacustomtext2 AS pacustomtext2, pa.pacustomtext3 AS pacustomtext3, pa.pacustomtext4 AS pacustomtext4, pa.pacustomtext5 AS pacustomtext5, pa.pacustomint1 AS pacustomint1, pa.pacustomint2 AS pacustomint2, pa.pacustomint3 AS pacustomint3, pa.pacustomdbl1 AS pacustomdbl1, pa.pacustomdbl2 AS pacustomdbl2, pa.pacustomdbl3 AS pacustomdbl3, pa.pacustomdate1 AS pacustomdate1, pa.pacustomdate2 AS pacustomdate2, pa.pacustomdate3 AS pacustomdate3, br.bnama AS pacabangnama, lc.lnama AS palokasinama, wh.wnama AS pagudangnama, c1.kkode AS pabagianpakode, c1.knama AS pabagianpanama, st1.nama AS pastatusnama, st2.nama AS pastatussebelumnyanama, u1.unama AS painputusernama, u2.unama AS pamodifikasiusernama, pa.pakategori, (CASE pa.pakategori WHEN 0 THEN 'Global' ELSE 'Category' END) as pakategorinama, pa.pakategoriharga, pc.pcnama as pakategoriharganama, pad.idpadetail AS idpadetail, pad.idpa AS idpa, pad.idbarang AS idbarang, pad.satuan AS satuan, pad.nilaisatuan AS nilaisatuan, pad.satuanbarang AS satuanbarang, pad.matauang AS matauang, pad.kurs AS kurs, pad.hargajual1lama AS hargajual1lama, pad.hargajual2lama AS hargajual2lama, pad.hargajual3lama AS hargajual3lama, pad.hargajual4lama AS hargajual4lama, pad.hargajual5lama AS hargajual5lama, pad.hargajual1 AS hargajual1, pad.hargajual2 AS hargajual2, pad.hargajual3 AS hargajual3, pad.hargajual4 AS hargajual4, pad.hargajual5 AS hargajual5, pad.diskonjual1lama AS diskonjual1lama, pad.diskonjual2lama AS diskonjual2lama, pad.diskonjual3lama AS diskonjual3lama, pad.diskonjual4lama AS diskonjual4lama, pad.diskonjual5lama AS diskonjual5lama, pad.diskonjual1 AS diskonjual1, pad.diskonjual2 AS diskonjual2, pad.diskonjual3 AS diskonjual3, pad.diskonjual4 AS diskonjual4, pad.diskonjual5 AS diskonjual5, pad.cabang AS cabang, pad.lokasi AS lokasi, pad.gudang AS gudang, pad.costcenter AS costcenter, pad.divisi AS divisi, pad.subdivisi AS subdivisi, pad.proyek AS proyek, pad.catatan AS catatan, pad.urutan AS urutan, pad.statusberlaku AS statusberlaku, pad.isclose AS isclose, pad.customtext1 AS customtext1, pad.customtext2 AS customtext2, pad.customtext3 AS customtext3, pad.customdbl1 AS customdbl1, pad.customdbl2 AS customdbl2, pad.customdbl3 AS customdbl3, pad.customdate1 AS customdate1, pad.customdate2 AS customdate2, pad.customdate3 AS customdate3, i.bkode AS kodebarang, i.bnama AS namabarang, i.btipe AS tipebarang, brd.bnama AS cabangnama, lcd.lnama AS lokasinama, whd.wnama AS gudangnama, cc.ccnama AS costcenternama,  d.dnama AS divisinama, sd.sdnama AS subdivisinama,  p.pnama AS proyeknama,  i.bhargabeli, pad.kontak, c2.kkode as kontakkode, c2.knama as kontaknama, pad.hargajual6lama, pad.hargajual7lama, pad.hargajual8lama, pad.hargajual9lama, pad.hargajual10lama, pad.hargajual6, pad.hargajual7, pad.hargajual8, pad.hargajual9, pad.hargajual10, pad.diskonjual6lama, pad.diskonjual7lama, pad.diskonjual8lama, pad.diskonjual9lama, pad.diskonjual10lama, pad.diskonjual6, pad.diskonjual7, pad.diskonjual8, pad.diskonjual9, pad.diskonjual10 from m3_pa pa join m3_pa_detail pad on pa.paid = pad.idpa join m0_status st1 on st1.kode = pa.pastatus join m0_status st2 on st2.kode = pa.pastatussebelumnya left join m1_branch br on br.bkode = pa.pacabang left join m1_location lc on lc.lkode = pa.palokasi left join m1_warehouse wh on wh.wkode = pa.pagudang left join m1_contact c1 on c1.kid = pa.pabagianpa left join m0_user u1 on u1.userid = pa.painputuser left join m0_user u2 on u2.userid = pa.pamodifikasiuser left join m1_price_category pc on pa.pakategoriharga = pc.pckode left join m1_item i on pad.idbarang = i.bid left join m1_branch brd on pad.cabang = brd.bkode left join m1_location lcd on pad.lokasi = lcd.lkode left join m1_warehouse whd on pad.gudang = whd.wkode left join m1_cost_center cc on pad.costcenter = cc.cckode left join m1_division d on pad.divisi = d.dkode left join m1_subdivision sd on pad.subdivisi = sd.sdkode left join m1_project p on pad.proyek = p.pkode left join m1_contact c2 on pad.kontak = c2.kid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("paid"), 0), sptField,
                     FxDB(drutama("pacabang"), ""), sptField,
                     FxDB(drutama("palokasi"), ""), sptField,
                     FxDB(drutama("pagudang"), ""), sptField,
                     FxDB(drutama("pasumber"), ""), sptField,
                     FxDB(drutama("paautonotransaksi"), 0), sptField,
                     FxDB(drutama("panotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("patgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("patglberlakusampai"), ""), formatTgl), sptField,
                     FxDB(drutama("pakodepa"), 0), sptField,
                     FxDB(drutama("pabagianpa"), 0), sptField,
                     FxDB(drutama("pabagianpakontak"), ""), sptField,
                     FxDB(drutama("pamatauang"), ""), sptField,
                     FxDB(drutama("pakurs"), 0), sptField,
                     FxDB(drutama("pauraian"), ""), sptField,
                     FxDB(drutama("pacatatan"), ""), sptField,
                     FxDB(drutama("panoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("patglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pastatus"), 0), sptField,
                     FxDB(drutama("pastatussebelumnya"), 0), sptField,
                     FxDB(drutama("pajmlrevisi"), 0), sptField,
                     FxDB(drutama("pacetakanke"), 0), sptField,
                     FxDB(drutama("painputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("painputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pamodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("paposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("papostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("patutupperiode"), 0), sptField,
                     FxDB(drutama("paisclose"), 0), sptField,
                     FxDB(drutama("pacustomtext1"), ""), sptField,
                     FxDB(drutama("pacustomtext2"), ""), sptField,
                     FxDB(drutama("pacustomtext3"), ""), sptField,
                     FxDB(drutama("pacustomtext4"), ""), sptField,
                     FxDB(drutama("pacustomtext5"), ""), sptField,
                     FxDB(drutama("pacustomint1"), 0), sptField,
                     FxDB(drutama("pacustomint2"), 0), sptField,
                     FxDB(drutama("pacustomint3"), 0), sptField,
                     FxDB(drutama("pacustomdbl1"), 0), sptField,
                     FxDB(drutama("pacustomdbl2"), 0), sptField,
                     FxDB(drutama("pacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pacabangnama"), ""), sptField,
                     FxDB(drutama("palokasinama"), ""), sptField,
                     FxDB(drutama("pagudangnama"), ""), sptField,
                     FxDB(drutama("pabagianpakode"), ""), sptField,
                     FxDB(drutama("pabagianpanama"), ""), sptField,
                     FxDB(drutama("pastatusnama"), ""), sptField,
                     FxDB(drutama("pastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("painputusernama"), ""), sptField,
                     FxDB(drutama("pamodifikasiusernama"), ""), sptField,
                     FxDB(drutama("pakategori"), 0), sptField,
                     FxDB(drutama("pakategorinama"), ""), sptField,
                     FxDB(drutama("pakategoriharga"), ""), sptField,
                     FxDB(drutama("pakategoriharganama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpadetail"), 0), sptField,
                     FxDB(dr("idpa"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargajual1lama"), 0), sptField,
                     FxDB(dr("hargajual2lama"), 0), sptField,
                     FxDB(dr("hargajual3lama"), 0), sptField,
                     FxDB(dr("hargajual4lama"), 0), sptField,
                     FxDB(dr("hargajual5lama"), 0), sptField,
                     FxDB(dr("hargajual1"), 0), sptField,
                     FxDB(dr("hargajual2"), 0), sptField,
                     FxDB(dr("hargajual3"), 0), sptField,
                     FxDB(dr("hargajual4"), 0), sptField,
                     FxDB(dr("hargajual5"), 0), sptField,
                     FxDB(dr("diskonjual1lama"), 0), sptField,
                     FxDB(dr("diskonjual2lama"), 0), sptField,
                     FxDB(dr("diskonjual3lama"), 0), sptField,
                     FxDB(dr("diskonjual4lama"), 0), sptField,
                     FxDB(dr("diskonjual5lama"), 0), sptField,
                     FxDB(dr("diskonjual1"), 0), sptField,
                     FxDB(dr("diskonjual2"), 0), sptField,
                     FxDB(dr("diskonjual3"), 0), sptField,
                     FxDB(dr("diskonjual4"), 0), sptField,
                     FxDB(dr("diskonjual5"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("statusberlaku"), 0), sptField,
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
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("hargajual6lama"), 0), sptField,
                     FxDB(dr("hargajual7lama"), 0), sptField,
                     FxDB(dr("hargajual8lama"), 0), sptField,
                     FxDB(dr("hargajual9lama"), 0), sptField,
                     FxDB(dr("hargajual10lama"), 0), sptField,
                     FxDB(dr("hargajual6"), 0), sptField,
                     FxDB(dr("hargajual7"), 0), sptField,
                     FxDB(dr("hargajual8"), 0), sptField,
                     FxDB(dr("hargajual9"), 0), sptField,
                     FxDB(dr("hargajual10"), 0), sptField,
                     FxDB(dr("diskonjual6lama"), 0), sptField,
                     FxDB(dr("diskonjual7lama"), 0), sptField,
                     FxDB(dr("diskonjual8lama"), 0), sptField,
                     FxDB(dr("diskonjual9lama"), 0), sptField,
                     FxDB(dr("diskonjual10lama"), 0), sptField,
                     FxDB(dr("diskonjual6"), 0), sptField,
                     FxDB(dr("diskonjual7"), 0), sptField,
                     FxDB(dr("diskonjual8"), 0), sptField,
                     FxDB(dr("diskonjual9"), 0), sptField,
                     FxDB(dr("diskonjual10"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, patutupperiode, paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama, pakategori, pakategorinama, pakategoriharga, pakategoriharganama" & sptSubParam & "idpadetail, idpa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, bhargabeli, kontak, kontakkode, kontaknama, hargajual6lama, hargajual7lama, hargajual8lama, hargajual9lama, hargajual10lama, hargajual6, hargajual7, hargajual8, hargajual9, hargajual10, diskonjual6lama, diskonjual7lama, diskonjual8lama, diskonjual9lama, diskonjual10lama, diskonjual6, diskonjual7, diskonjual8, diskonjual9, diskonjual10"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_PaSearch(ByVal param As String) As String
        'M3_PaSearch --------------------------------------------------------
        'paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, 
        'patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, 
        'pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, 
        'pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, 
        'patutupperiode, paisclose, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, 
        'pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama,
        'pakategori, pakategorinama, pakategoriharga, pakategoriharganama

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
            Filter = Filter.Replace("pabagianpakode", "c1.kkode")
            Filter = Filter.Replace("pabagianpanama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3_pa_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Pa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("paid"), 0), sptField,
                     FxDB(dr("pacabang"), ""), sptField,
                     FxDB(dr("palokasi"), ""), sptField,
                     FxDB(dr("pagudang"), ""), sptField,
                     FxDB(dr("pasumber"), ""), sptField,
                     FxDB(dr("paautonotransaksi"), 0), sptField,
                     FxDB(dr("panotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("patgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("patglberlakusampai"), ""), formatTgl), sptField,
                     FxDB(dr("pakodepa"), 0), sptField,
                     FxDB(dr("pabagianpa"), 0), sptField,
                     FxDB(dr("pabagianpakontak"), ""), sptField,
                     FxDB(dr("pamatauang"), ""), sptField,
                     FxDB(dr("pakurs"), 0), sptField,
                     FxDB(dr("pauraian"), ""), sptField,
                     FxDB(dr("pacatatan"), ""), sptField,
                     FxDB(dr("panoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("patglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pastatus"), 0), sptField,
                     FxDB(dr("pastatussebelumnya"), 0), sptField,
                     FxDB(dr("pajmlrevisi"), 0), sptField,
                     FxDB(dr("pacetakanke"), 0), sptField,
                     FxDB(dr("painputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("painputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pamodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("paposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("papostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("patutupperiode"), 0), sptField,
                     FxDB(dr("paisclose"), 0), sptField,
                     FxDB(dr("pacabangnama"), ""), sptField,
                     FxDB(dr("palokasinama"), ""), sptField,
                     FxDB(dr("pagudangnama"), ""), sptField,
                     FxDB(dr("pabagianpakode"), ""), sptField,
                     FxDB(dr("pabagianpanama"), ""), sptField,
                     FxDB(dr("pastatusnama"), ""), sptField,
                     FxDB(dr("pastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("painputusernama"), ""), sptField,
                     FxDB(dr("pamodifikasiusernama"), ""), sptField,
                     FxDB(dr("pakategori"), 0), sptField,
                     FxDB(dr("pakategorinama"), ""), sptField,
                     FxDB(dr("pakategoriharga"), ""), sptField,
                     FxDB(dr("pakategoriharganama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, papostingtgl, patutupperiode, paisclose, pacabangnama, palokasinama, pagudangnama, pabagianpakode, pabagianpanama, pastatusnama, pastatussebelumnyanama, painputusernama, pamodifikasiusernama, pakategori, pakategorinama, pakategoriharga, pakategoriharganama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_PaTerkait(ByVal param As String) As String
        'M3_PaTerkait --------------------------------------------------------
        'paid, panotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "said required numeric." : GoTo selesai
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
                     FxDB(dr("paid"), 0), sptField,
                     FxDB(dr("panotransaksi"), ""), sptField,
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
            result(2) = "Related PA data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("paid, panotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_PaSimpanOld(ByVal param As String) As String
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
        'paid(0) As Integer, pacabang(1) As String, palokasi(2) As String, pagudang(3) As String, pasumber(4) As String, 
        'paautonotransaksi(5) As Integer, panotransaksi(6) As String, patgl(7) As Date, patglberlakusampai(8) As Date, pakodepa(9) As Integer, 
        'pabagianpa(10) As Integer, pabagianpakontak(11) As String, pamatauang(12) As String, pakurs(13) As Double, pauraian(14) As String, 
        'pacatatan(15) As String, panoref(16) As String, patglnoref(17) As Date, pastatus(18) As Integer, pastatussebelumnya(19) As Integer, 
        'pajmlrevisi(20) As Integer, pacetakanke(21) As Integer, painputuser(22) As Integer, painputtgl(23) As DateTime, pamodifikasiuser(24) As Integer, 
        'pamodifikasitgl(25) As DateTime, paposting(26) As Integer, patutupperiode(27) As Integer, paisclose(28) As Integer, pacustomtext1(29) As String, 
        'pacustomtext2(30) As String, pacustomtext3(31) As String, pacustomtext4(32) As String, pacustomtext5(33) As String, pacustomint1(34) As Integer, 
        'pacustomint2(35) As Integer, pacustomint3(36) As Integer, pacustomdbl1(37) As Double, pacustomdbl2(38) As Double, pacustomdbl3(39) As Double, 
        'pacustomdate1(40) As Date, pacustomdate2(41) As Date, pacustomdate3(42) As Date, pakategori(43) As Integer, pakategoriharga(44) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'paid, pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, 
        'patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, 
        'pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, 
        'pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, patutupperiode, 
        'paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, 
        'pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, 
        'pacustomdate3, pakategori, pakategoriharga

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'paid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "paid required numeric." : GoTo selesai
        End If
        'paautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "paautonotransaksi required numeric." : GoTo selesai
        End If
        'patgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "patgl required date." : GoTo selesai
        End If
        'patglberlakusampai(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "patglberlakusampai required date." : GoTo selesai
        End If
        'pakodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "pakodepa required numeric." : GoTo selesai
        End If
        'pabagianpa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "pabagianpa required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "pabagianpa can't be empty." : GoTo selesai
        End If
        'pakurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pakurs required numeric." : GoTo selesai
        End If
        'patglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "patglnoref required date." : GoTo selesai
        End If
        'pastatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pastatus required numeric." : GoTo selesai
        End If
        'pastatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pastatussebelumnya required numeric." : GoTo selesai
        End If
        'pajmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "pajmlrevisi required numeric." : GoTo selesai
        End If
        'pacetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "pacetakanke required numeric." : GoTo selesai
        End If
        'painputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "painputuser required numeric." : GoTo selesai
        End If
        'painputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "painputtgl required date." : GoTo selesai
        End If
        'pamodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "pamodifikasiuser required numeric." : GoTo selesai
        End If
        'pamodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pamodifikasitgl required date." : GoTo selesai
        End If
        'paposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "paposting required numeric." : GoTo selesai
        End If
        'patutupperiode(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "patutupperiode required numeric." : GoTo selesai
        End If
        'paisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "paisclose required numeric." : GoTo selesai
        End If
        'pacustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pacustomint1 required numeric." : GoTo selesai
        End If
        'pacustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pacustomint2 required numeric." : GoTo selesai
        End If
        'pacustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pacustomint3 required numeric." : GoTo selesai
        End If
        'pacustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pacustomdbl1 required numeric." : GoTo selesai
        End If
        'pacustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pacustomdbl2 required numeric." : GoTo selesai
        End If
        'pacustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pacustomdbl3 required numeric." : GoTo selesai
        End If
        'pacustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "pacustomdate1 required date." : GoTo selesai
        End If
        'pacustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "pacustomdate2 required date." : GoTo selesai
        End If
        'pacustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "pacustomdate3 required date." : GoTo selesai
        End If
        'pakategori(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "pakategori required numeric." : GoTo selesai
        Else
            If dataUtama(43) <> 0 And dataUtama(43) <> 1 Then
                result(2) = "Invalid pakategori value." : GoTo selesai
            End If
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pacabang should not be more than 25 character." : GoTo selesai
        End If

        'palokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "palokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "palokasi should not be more than 25 character." : GoTo selesai
        End If

        'pasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "pasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "pasumber should not be more than 10 character." : GoTo selesai
        End If

        'panotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "panotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "panotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'patgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "patgl can't be empty" : GoTo selesai
        End If

        'patglberlakusampai(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "patglberlakusampai can't be empty" : GoTo selesai
        End If

        'pabagianpakontak(11) As String
        'If Len(dataUtama(11)) = 0 Then
        '    result(2) = "pabagianpakontak can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(11)) > 100 Then
            result(2) = "pabagianpakontak should not be more than 100 character." : GoTo selesai
        End If

        'pamatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pamatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "pamatauang should not be more than 25 character." : GoTo selesai
        End If

        'pakurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "pakurs can't be empty" : GoTo selesai
        End If

        'patglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "patglnoref can't be empty" : GoTo selesai
        End If

        'painputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "painputtgl can't be empty" : GoTo selesai
        End If

        'pamodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pamodifikasitgl can't be empty" : GoTo selesai
        End If

        'pacustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pacustomdbl1 can't be empty" : GoTo selesai
        End If

        'pacustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pacustomdbl2 can't be empty" : GoTo selesai
        End If

        'pacustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pacustomdbl3 can't be empty" : GoTo selesai
        End If

        'pacustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pacustomdate1 can't be empty" : GoTo selesai
        End If

        'pacustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pacustomdate2 can't be empty" : GoTo selesai
        End If

        'pacustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pacustomdate3 can't be empty" : GoTo selesai
        End If

        'pakategoriharga(44) As String
        If dataUtama(43) = 1 And Len(dataUtama(44)) = 0 Then
            result(2) = "pakategoriharga can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(44)) > 25 Then
            result(2) = "pakategoriharga should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "paid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "palokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "paautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "panotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "patgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "patglberlakusampai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pabagianpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pabagianpakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pamatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "panoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "patglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "painputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "painputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "paposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "patutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "paisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pacustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pakategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pakategoriharga", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "paid~pacabang~palokasi~pagudang~pasumber~paautonotransaksi~panotransaksi~patgl~patglberlakusampai~pakodepa~pabagianpa~pabagianpakontak~pamatauang~pakurs~pauraian~pacatatan~panoref~patglnoref~pastatus~pastatussebelumnya~pajmlrevisi~pacetakanke~painputuser~painputtgl~pamodifikasiuser~pamodifikasitgl~paposting~patutupperiode~paisclose~pacustomtext1~pacustomtext2~pacustomtext3~pacustomtext4~pacustomtext5~pacustomint1~pacustomint2~pacustomint3~pacustomdbl1~pacustomdbl2~pacustomdbl3~pacustomdate1~pacustomdate2~pacustomdate3~pakategori~pakategoriharga", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpadetail(0) As Integer, idpa(1) As Integer, idbarang(2) As Integer, satuan(3) As String, nilaisatuan(4) As Double, 
        'satuanbarang(5) As String, matauang(6) As String, kurs(7) As Double, hargajual1lama(8) As Double, hargajual2lama(9) As Double, 
        'hargajual3lama(10) As Double, hargajual4lama(11) As Double, hargajual5lama(12) As Double, hargajual1(13) As Double, hargajual2(14) As Double, 
        'hargajual3(15) As Double, hargajual4(16) As Double, hargajual5(17) As Double, diskonjual1lama(18) As Double, diskonjual2lama(19) As Double, 
        'diskonjual3lama(20) As Double, diskonjual4lama(21) As Double, diskonjual5lama(22) As Double, diskonjual1(23) As Double, diskonjual2(24) As Double, 
        'diskonjual3(25) As Double, diskonjual4(26) As Double, diskonjual5(27) As Double, cabang(28) As String, lokasi(29) As String, 
        'gudang(30) As String, costcenter(31) As String, divisi(32) As String, subdivisi(33) As String, proyek(34) As String, 
        'catatan(35) As String, urutan(36) As Integer, statusberlaku(37) As Integer, isclose(38) As Integer, customtext1(39) As String, 
        'customtext2(40) As String, customtext3(41) As String, customdbl1(42) As Double, customdbl2(43) As Double, customdbl3(44) As Double, 
        'customdate1(45) As Date, customdate2(46) As Date, customdate3(47) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpadetail, idpa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, 
        'kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, 
        'hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, 
        'diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, 
        'cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual1lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual2lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual3lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual4lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual5lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual1lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual2lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual3lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual4lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual5lama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusberlaku", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 48) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpadetail required numeric." : GoTo selesai
            End If
            'idpa(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpa required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'nilaisatuan(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'kurs(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargajual1lama(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - hargajual1lama required numeric." : GoTo selesai
            End If
            'hargajual2lama(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - hargajual2lama required numeric." : GoTo selesai
            End If
            'hargajual3lama(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - hargajual3lama required numeric." : GoTo selesai
            End If
            'hargajual4lama(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - hargajual4lama required numeric." : GoTo selesai
            End If
            'hargajual5lama(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargajual5lama required numeric." : GoTo selesai
            End If
            'hargajual1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - hargajual1 required numeric." : GoTo selesai
            End If
            'hargajual2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - hargajual2 required numeric." : GoTo selesai
            End If
            'hargajual3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hargajual3 required numeric." : GoTo selesai
            End If
            'hargajual4(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - hargajual4 required numeric." : GoTo selesai
            End If
            'hargajual5(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - hargajual5 required numeric." : GoTo selesai
            End If
            ''diskonjual1lama(18) As Double
            'If (IsNumeric(dataRowDetail(18)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual1lama required numeric." : GoTo selesai
            'End If
            ''diskonjual2lama(19) As Double
            'If (IsNumeric(dataRowDetail(19)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual2lama required numeric." : GoTo selesai
            'End If
            ''diskonjual3lama(20) As Double
            'If (IsNumeric(dataRowDetail(20)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual3lama required numeric." : GoTo selesai
            'End If
            ''diskonjual4lama(21) As Double
            'If (IsNumeric(dataRowDetail(21)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual4lama required numeric." : GoTo selesai
            'End If
            ''diskonjual5lama(22) As Double
            'If (IsNumeric(dataRowDetail(22)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual5lama required numeric." : GoTo selesai
            'End If
            ''diskonjual1(23) As Double
            'If (IsNumeric(dataRowDetail(23)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual1 required numeric." : GoTo selesai
            'End If
            ''diskonjual2(24) As Double
            'If (IsNumeric(dataRowDetail(24)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual2 required numeric." : GoTo selesai
            'End If
            ''diskonjual3(25) As Double
            'If (IsNumeric(dataRowDetail(25)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual3 required numeric." : GoTo selesai
            'End If
            ''diskonjual4(26) As Double
            'If (IsNumeric(dataRowDetail(26)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual4 required numeric." : GoTo selesai
            'End If
            ''diskonjual5(27) As Double
            'If (IsNumeric(dataRowDetail(27)) = False) Then
            '    result(2) = "Row : " & i & " - diskonjual5 required numeric." : GoTo selesai
            'End If
            'urutan(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'statusberlaku(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - statusberlaku required numeric." : GoTo selesai
            End If
            'isclose(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(43) As Double
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(44) As Double
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(45) As Date
            If (IsDate(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(46) As Date
            If (IsDate(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(47) As Date
            If (IsDate(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'satuan(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'satuanbarang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'hargajual1lama(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - hargajual1lama can't be empty" : GoTo selesai
            End If

            'hargajual2lama(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - hargajual2lama can't be empty" : GoTo selesai
            End If

            'hargajual3lama(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - hargajual3lama can't be empty" : GoTo selesai
            End If

            'hargajual4lama(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - hargajual4lama can't be empty" : GoTo selesai
            End If

            'hargajual5lama(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - hargajual5lama can't be empty" : GoTo selesai
            End If

            'hargajual1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - hargajual1 can't be empty" : GoTo selesai
            End If

            'hargajual2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - hargajual2 can't be empty" : GoTo selesai
            End If

            'hargajual3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hargajual3 can't be empty" : GoTo selesai
            End If

            'hargajual4(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - hargajual4 can't be empty" : GoTo selesai
            End If

            'hargajual5(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - hargajual5 can't be empty" : GoTo selesai
            End If

            'diskonjual1lama(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual1lama can't be empty" : GoTo selesai
            End If

            'diskonjual2lama(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual2lama can't be empty" : GoTo selesai
            End If

            'diskonjual3lama(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual3lama can't be empty" : GoTo selesai
            End If

            'diskonjual4lama(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual4lama can't be empty" : GoTo selesai
            End If

            'diskonjual5lama(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual5lama can't be empty" : GoTo selesai
            End If

            'diskonjual1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual1 can't be empty" : GoTo selesai
            End If

            'diskonjual2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual2 can't be empty" : GoTo selesai
            End If

            'diskonjual3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual3 can't be empty" : GoTo selesai
            End If

            'diskonjual4(26) As Double
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual4 can't be empty" : GoTo selesai
            End If

            'diskonjual5(27) As Double
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - diskonjual5 can't be empty" : GoTo selesai
            End If

            'customdbl1(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(43) As Double
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(44) As Double
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(45) As Date
            If Len(dataRowDetail(45)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(46) As Date
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(47) As Date
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpadetail~idpa~idbarang~satuan~nilaisatuan~satuanbarang~matauang~kurs~hargajual1lama~hargajual2lama~hargajual3lama~hargajual4lama~hargajual5lama~hargajual1~hargajual2~hargajual3~hargajual4~hargajual5~diskonjual1lama~diskonjual2lama~diskonjual3lama~diskonjual4lama~diskonjual5lama~diskonjual1~diskonjual2~diskonjual3~diskonjual4~diskonjual5~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~statusberlaku~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("patgl")), AsFormatTanggal(drutama("patgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'CEK HAK AKSES ==========================================
                If drutama("pastatus") = 2 Then
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                    Dim rsCekHakAkses As String = HakAkses(3, 8, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                    If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai
                End If
                'END OF CEK HAK AKSES ===================================


                If isUpdate Then
                    result(4) = drutama("paid")
                    notransaksi = drutama("panotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(paid), panotransaksi FROM M3_pa WHERE paid='" & result(4) & "' AND pastatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(paid) FROM m3_pa WHERE panotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m3_pa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pasumber")) & "▼" & FixQuotes(drutama("paid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M3_Pa set pacabang  = '" & FixQuotes(drutama("pacabang")) & "', palokasi  = '" & FixQuotes(drutama("palokasi")) & "', pagudang  = '" & FixQuotes(drutama("pagudang")) & "', pasumber  = '" & FixQuotes(drutama("pasumber")) & "', paautonotransaksi  = " & drutama("paautonotransaksi") & ", panotransaksi  = '" & notransaksi & "', patgl  = '" & FixQuotes(AsFormatTanggal(drutama("patgl"))) & "', patglberlakusampai  = '" & FixQuotes(AsFormatTanggal(drutama("patglberlakusampai"))) & "', pakodepa  = " & drutama("pakodepa") & ", pabagianpa  = " & drutama("pabagianpa") & ", pabagianpakontak  = '" & FixQuotes(drutama("pabagianpakontak")) & "', pamatauang  = '" & FixQuotes(drutama("pamatauang")) & "', pakurs  = '" & FixDouble(drutama("pakurs")) & "', pauraian  = '" & FixQuotes(drutama("pauraian")) & "', pacatatan  = '" & FixQuotes(drutama("pacatatan")) & "', panoref  = '" & FixQuotes(drutama("panoref")) & "', patglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("patglnoref"))) & "', pastatus  = " & drutama("pastatus") & ", pastatussebelumnya  = " & drutama("pastatussebelumnya") & ", pajmlrevisi  = pajmlrevisi+1, pacetakanke  = " & drutama("pacetakanke") & ", pamodifikasiuser  = " & drutama("pamodifikasiuser") & ", pamodifikasitgl  = NOW(), paposting  = 0, patutupperiode  = " & drutama("patutupperiode") & ", pacustomtext1  = '" & FixQuotes(drutama("pacustomtext1")) & "', pacustomtext2  = '" & FixQuotes(drutama("pacustomtext2")) & "', pacustomtext3  = '" & FixQuotes(drutama("pacustomtext3")) & "', pacustomtext4  = '" & FixQuotes(drutama("pacustomtext4")) & "', pacustomtext5  = '" & FixQuotes(drutama("pacustomtext5")) & "', pacustomint1  = " & drutama("pacustomint1") & ", pacustomint2  = " & drutama("pacustomint2") & ", pacustomint3  = " & drutama("pacustomint3") & ", pacustomdbl1  = '" & FixDouble(drutama("pacustomdbl1")) & "', pacustomdbl2  = '" & FixDouble(drutama("pacustomdbl2")) & "', pacustomdbl3  = '" & FixDouble(drutama("pacustomdbl3")) & "', pacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate1"))) & "', pacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate2"))) & "', pacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate3"))) & "', pakategori = '" & FixQuotes(drutama("pakategori")) & "', pakategoriharga = '" & FixQuotes(drutama("pakategoriharga")) & "' where paid = '" & drutama("paid") & "'"
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

                    If drutama("paautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pacabang"), drutama("palokasi"), drutama("pasumber"), drutama("patgl"))
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
                        notransaksi = drutama("panotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(paid) FROM m3_pa WHERE panotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Pa (pacabang, palokasi, pagudang, pasumber, paautonotransaksi, panotransaksi, patgl, patglberlakusampai, pakodepa, pabagianpa, pabagianpakontak, pamatauang, pakurs, pauraian, pacatatan, panoref, patglnoref, pastatus, pastatussebelumnya, pajmlrevisi, pacetakanke, painputuser, painputtgl, pamodifikasiuser, pamodifikasitgl, paposting, patutupperiode, paisclose, pacustomtext1, pacustomtext2, pacustomtext3, pacustomtext4, pacustomtext5, pacustomint1, pacustomint2, pacustomint3, pacustomdbl1, pacustomdbl2, pacustomdbl3, pacustomdate1, pacustomdate2, pacustomdate3, pakategori, pakategoriharga) values('" & FixQuotes(drutama("pacabang")) & "', '" & FixQuotes(drutama("palokasi")) & "', '" & FixQuotes(drutama("pagudang")) & "', '" & FixQuotes(drutama("pasumber")) & "', " & drutama("paautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("patgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("patglberlakusampai"))) & "', " & drutama("pakodepa") & ", " & drutama("pabagianpa") & ", '" & FixQuotes(drutama("pabagianpakontak")) & "', '" & FixQuotes(drutama("pamatauang")) & "', '" & FixDouble(drutama("pakurs")) & "', '" & FixQuotes(drutama("pauraian")) & "', '" & FixQuotes(drutama("pacatatan")) & "', '" & FixQuotes(drutama("panoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("patglnoref"))) & "', " & drutama("pastatus") & ", " & drutama("pastatussebelumnya") & ", " & drutama("pajmlrevisi") & ", " & drutama("pacetakanke") & ", " & drutama("painputuser") & ", NOW(), " & drutama("pamodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("patutupperiode") & ", " & drutama("paisclose") & ", '" & FixQuotes(drutama("pacustomtext1")) & "', '" & FixQuotes(drutama("pacustomtext2")) & "', '" & FixQuotes(drutama("pacustomtext3")) & "', '" & FixQuotes(drutama("pacustomtext4")) & "', '" & FixQuotes(drutama("pacustomtext5")) & "', " & drutama("pacustomint1") & ", " & drutama("pacustomint2") & ", " & drutama("pacustomint3") & ", '" & FixDouble(drutama("pacustomdbl1")) & "', '" & FixDouble(drutama("pacustomdbl2")) & "', '" & FixDouble(drutama("pacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pacustomdate3"))) & "', '" & FixQuotes(drutama("pakategori")) & "', '" & FixQuotes(drutama("pakategoriharga")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select paid from M3_pa where panotransaksi='" & notransaksi & "' AND painputuser= '" & userid & "' order by pamodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M3_Pa_Detail where idpa = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpadetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargajual1lama")) & "', '" & FixDouble(dr1("hargajual2lama")) & "', '" & FixDouble(dr1("hargajual3lama")) & "', '" & FixDouble(dr1("hargajual4lama")) & "', '" & FixDouble(dr1("hargajual5lama")) & "', '" & FixDouble(dr1("hargajual1")) & "', '" & FixDouble(dr1("hargajual2")) & "', '" & FixDouble(dr1("hargajual3")) & "', '" & FixDouble(dr1("hargajual4")) & "', '" & FixDouble(dr1("hargajual5")) & "', '" & FixDouble(dr1("diskonjual1lama")) & "', '" & FixDouble(dr1("diskonjual2lama")) & "', '" & FixDouble(dr1("diskonjual3lama")) & "', '" & FixDouble(dr1("diskonjual4lama")) & "', '" & FixDouble(dr1("diskonjual5lama")) & "', '" & FixDouble(dr1("diskonjual1")) & "', '" & FixDouble(dr1("diskonjual2")) & "', '" & FixDouble(dr1("diskonjual3")) & "', '" & FixDouble(dr1("diskonjual4")) & "', '" & FixDouble(dr1("diskonjual5")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusberlaku") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M3_Pa_Detail(idpadetail, idpa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'UPDATE HARGA KE MASTER DATA BARANG =================================================
                If drutama("pastatus") = 2 Then
                    'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                    'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                    If drutama("pakategori") = 0 Then
                        'UPDATE HARGA LAMA KE TABEL DETAIL (M3_PA_DETAIL)
                        sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid SET pad.hargajual1lama = i.bhargajual1, pad.hargajual2lama = i.bhargajual2, pad.hargajual3lama = i.bhargajual3, pad.hargajual4lama = i.bhargajual4, pad.hargajual5lama = i.bhargajual5, pad.diskonjual1lama = i.bdiskonjual1, pad.diskonjual2lama = i.bdiskonjual2, pad.diskonjual3lama = i.bdiskonjual3, pad.diskonjual4lama = i.bdiskonjual4, pad.diskonjual5lama = i.bdiskonjual5 WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HARGA BARU KE TABEL BARANG (M1_ITEM)
                        sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid SET i.bhargajual1 = pad.hargajual1 / pad.nilaisatuan, i.bhargajual2 = pad.hargajual2 / pad.nilaisatuan, i.bhargajual3 = pad.hargajual3 / pad.nilaisatuan, i.bhargajual4 = pad.hargajual4 / pad.nilaisatuan, i.bhargajual5 = pad.hargajual5 / pad.nilaisatuan, i.bdiskonjual1 = pad.diskonjual1, i.bdiskonjual2 = pad.diskonjual2, i.bdiskonjual3 = pad.diskonjual3, i.bdiskonjual4 = pad.diskonjual4, i.bdiskonjual5 = pad.diskonjual5, i.bhargabeli = pad.customdbl1 WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else

                        'UPDATE HARGA LAMA KE TABEL DETAIL (M3_PA_DETAIL)
                        sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid LEFT JOIN m1_price_category_detail pcd ON pad.idbarang = pcd.pcdidbarang AND pcd.pcdkategori = '" & FixQuotes(drutama("pakategoriharga")) & "' SET pad.hargajual1lama = ifnull(pcd.pcdhargajual1, i.bhargajual1), pad.hargajual2lama = ifnull(pcd.pcdhargajual2, i.bhargajual2), pad.hargajual3lama = ifnull(pcd.pcdhargajual3, i.bhargajual3), pad.hargajual4lama = ifnull(pcd.pcdhargajual4, i.bhargajual4), pad.hargajual5lama = ifnull(pcd.pcdhargajual5, i.bhargajual5), pad.diskonjual1lama = ifnull(pcd.pcddiskonjual1, i.bdiskonjual1), pad.diskonjual2lama = ifnull(pcd.pcddiskonjual2, i.bdiskonjual2), pad.diskonjual3lama = ifnull(pcd.pcddiskonjual3, i.bdiskonjual3), pad.diskonjual4lama = ifnull(pcd.pcddiskonjual4, i.bdiskonjual4), pad.diskonjual5lama = ifnull(pcd.pcddiskonjual5, i.bdiskonjual5) WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HARGA BARU KE TABEL BARANG (M1_PRICE_CATEGORY_DETAIL)
                        sql = "INSERT INTO M1_Price_Category_Detail (SELECT '" & FixQuotes(drutama("pakategoriharga")) & "' as pcdkategori, pad.idbarang as pcdidbarang, i.bstokminimal as pcdstokminimal, i.bstokmaksimal as pcdstokmaksimal, i.breorder as pcdstokreorder, i.bminorder as pcdstokminorder, pad.hargajual1 / pad.nilaisatuan as pcdhargajual1, pad.hargajual2 / pad.nilaisatuan as pcdhargajual2, pad.hargajual3 / pad.nilaisatuan as pcdhargajual3, pad.hargajual4 / pad.nilaisatuan as pcdhargajual4, pad.hargajual5 / pad.nilaisatuan as pcdhargajual5, pad.diskonjual1 as pcddiskonjual1, pad.diskonjual2 as pcddiskonjual2, pad.diskonjual3 as pcddiskonjual3, pad.diskonjual4 as pcddiskonjual4, pad.diskonjual5 as pcddiskonjual5, pad.customtext1 as pcdcustomtext1, pad.customtext2 as pcdcustomtext2, pad.customtext3 as pcdcustomtext3, '' as pcdcustomtext4, '' as pcdcustomtext5, 0 as pcdcustomint1, 0 as pcdcustomint2, 0 as pcdcustomint3, pad.customdbl1 as pcdcustomdbl1, pad.customdbl2 as pcdcustomdbl2, pad.customdbl3 as pcdcustomdbl3, pad.customdate1 as pcdcustomdate1, pad.customdate2 as pcdcustomdate2, pad.customdate3 as pcdcustomdate3, 0 as pcddownloaded FROM m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid AND pad.idpa = '" & FixDouble(result(4)) & "') ON DUPLICATE KEY UPDATE pcdhargajual1 = VALUES(pcdhargajual1), pcdhargajual2 = VALUES(pcdhargajual2), pcdhargajual3 = VALUES(pcdhargajual3), pcdhargajual4 = VALUES(pcdhargajual4), pcdhargajual5 = VALUES(pcdhargajual5), pcddiskonjual1 = VALUES(pcddiskonjual1), pcddiskonjual2 = VALUES(pcddiskonjual2), pcddiskonjual3 = VALUES(pcddiskonjual3), pcddiskonjual4 = VALUES(pcddiskonjual4), pcddiskonjual5 = VALUES(pcddiskonjual5)"
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
                'END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "Pa", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M3_PaUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("pabagianpakode", "c1.kkode")
            Filter = Filter.Replace("pabagianpanama", "c1.knama")
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
            Dim sumber As String = "Pa", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim pakategori As Integer = 0, pakategoriharga As String = ""

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0, 0, '' FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Patgl, Panotransaksi, Pastatus, Pakategori, Pakategoriharga FROM m3_Pa WHERE Paid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1)
                'tgl                                 notransaksi                         status
                tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'kategori                                               kategoriharga       
                pakategori = FixDouble(FxDB(dtdetail.Rows(1)(3), 0)) : pakategoriharga = FixQuotes(FxDB(dtdetail.Rows(1)(4), ""))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m3_pa_history
            Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'UPDATE HARGA KE MASTER DATA BARANG =================================================
                'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                If pakategori = 0 Then
                    'UPDATE HARGA LAMA KE TABEL BARANG (M1_ITEM)
                    sql = "UPDATE m3_pa_detail pad JOIN m1_item i ON pad.idbarang = i.bid SET i.bhargajual1 = pad.hargajual1lama, i.bhargajual2 = pad.hargajual2lama, i.bhargajual3 = pad.hargajual3lama, i.bhargajual4 = pad.hargajual4lama, i.bhargajual5 = pad.hargajual5lama, i.bdiskonjual1 = pad.diskonjual1lama, i.bdiskonjual2 = pad.diskonjual2lama, i.bdiskonjual3 = pad.diskonjual3lama, i.bdiskonjual4 = pad.diskonjual4lama, i.bdiskonjual5 = pad.diskonjual5lama WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                Else
                    'UPDATE HARGA LAMA KE TABEL HARGA BARANG PER KATEGORI (M1_PRICE_CATEGORY_DETAIL) SESUAI IDBARANG DAN KATEGORI HARGA BARANG
                    sql = "UPDATE m3_pa_detail pad JOIN m1_price_category_detail i ON i.pcdkategori = '" & pakategoriharga & "' AND pad.idbarang = i.pcdidbarang SET i.pcdhargajual1 = pad.hargajual1lama, i.pcdhargajual2 = pad.hargajual2lama, i.pcdhargajual3 = pad.hargajual3lama, i.pcdhargajual4 = pad.hargajual4lama, i.pcdhargajual5 = pad.hargajual5lama, i.pcddiskonjual1 = pad.diskonjual1lama, i.pcddiskonjual2 = pad.diskonjual2lama, i.pcddiskonjual3 = pad.diskonjual3lama, i.pcddiskonjual4 = pad.diskonjual4lama, i.pcddiskonjual5 = pad.diskonjual5lama WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                End If
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================
            End If

            'update status utama
            sql = "UPDATE M3_Pa SET Pastatus = " & nilaiStatus & ", Pamodifikasiuser='" & userid & "', Pamodifikasitgl = NOW(), Paposting = 0, Papostingtgl = '1971-01-01 00:00:00', Pajmlrevisi = Pajmlrevisi + 1 WHERE Paid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_PaSearch(PostWsSearch(paramSplit(0), "M3_PaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M3_PaDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("pabagianpakode", "c1.kkode")
            Filter = Filter.Replace("pabagianpanama", "c1.knama")
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
            Dim sumber As String = "Pa", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Paid, Panotransaksi FROM m2_Pa WHERE Paid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pacabang, palokasi, pasumber, paautonotransaksi, panotransaksi, patgl"
            sql &= " FROM M3_pa"
            sql &= " WHERE paid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pacabang")
                lokasi = dtNomorNext.Rows(0)("palokasi")
                sumber = dtNomorNext.Rows(0)("pasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("paautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("panotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("patgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M3_Pa_Detail WHERE idpa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M3_Pa WHERE paid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M3_PaSearch(PostWsSearch(paramSplit(0), "M3_PaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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