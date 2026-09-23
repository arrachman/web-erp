Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_ppa
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_PpaSimpan(ByVal param As String) As String
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
        'ppaid(0) As Integer, ppacabang(1) As String, ppalokasi(2) As String, ppagudang(3) As String, ppasumber(4) As String, 
        'ppaautonotransaksi(5) As Integer, ppanotransaksi(6) As String, ppatgl(7) As Date, ppatglberlakusamppai(8) As Date, ppakodeppa(9) As Integer, 
        'ppabagianppa(10) As Integer, ppabagianppakontak(11) As String, ppamatauang(12) As String, ppakurs(13) As Double, ppauraian(14) As String, 
        'ppacatatan(15) As String, ppanoref(16) As String, ppatglnoref(17) As Date, ppastatus(18) As Integer, ppastatussebelumnya(19) As Integer, 
        'ppajmlrevisi(20) As Integer, ppacetakanke(21) As Integer, ppainputuser(22) As Integer, ppainputtgl(23) As DateTime, ppamodifikasiuser(24) As Integer, 
        'ppamodifikasitgl(25) As DateTime, ppaposting(26) As Integer, ppatutupperiode(27) As Integer, ppaisclose(28) As Integer, ppacustomtext1(29) As String, 
        'ppacustomtext2(30) As String, ppacustomtext3(31) As String, ppacustomtext4(32) As String, ppacustomtext5(33) As String, ppacustomint1(34) As Integer, 
        'ppacustomint2(35) As Integer, ppacustomint3(36) As Integer, ppacustomdbl1(37) As Double, ppacustomdbl2(38) As Double, ppacustomdbl3(39) As Double, 
        'ppacustomdate1(40) As Date, ppacustomdate2(41) As Date, ppacustomdate3(42) As Date, ppakategori(43) As Integer, ppakategoripos(44) As String, ppajenis(45) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ppaid, ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, 
        'ppatgl, ppatglberlakusamppai, ppakodeppa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, 
        'ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, 
        'ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppatutupperiode, 
        'ppaisclose, ppacustomtext1, ppacustomtext2, ppacustomtext3, ppacustomtext4, ppacustomtext5, ppacustomint1, 
        'ppacustomint2, ppacustomint3, ppacustomdbl1, ppacustomdbl2, ppacustomdbl3, ppacustomdate1, ppacustomdate2, 
        'ppacustomdate3, ppakategori, ppakategoripos, ppajenis

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'paid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ppaid required numeric." : GoTo selesai
        End If
        'paautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "ppaautonotransaksi required numeric." : GoTo selesai
        End If
        'patgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ppatgl required date." : GoTo selesai
        End If
        'patglberlakusampai(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "ppatglberlakusampai required date." : GoTo selesai
        End If
        'pakodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "ppakodepa required numeric." : GoTo selesai
        End If
        'pabagianpa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "ppabagianppa required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "ppabagianppa can't be empty." : GoTo selesai
        End If
        'pakurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ppakurs required numeric." : GoTo selesai
        End If
        'patglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "ppatglnoref required date." : GoTo selesai
        End If
        'pastatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "ppastatus required numeric." : GoTo selesai
        End If
        'pastatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "ppastatussebelumnya required numeric." : GoTo selesai
        End If
        'pajmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ppajmlrevisi required numeric." : GoTo selesai
        End If
        'pacetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "ppacetakanke required numeric." : GoTo selesai
        End If
        'painputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ppainputuser required numeric." : GoTo selesai
        End If
        'painputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "ppainputtgl required date." : GoTo selesai
        End If
        'pamodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "ppamodifikasiuser required numeric." : GoTo selesai
        End If
        'pamodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "ppamodifikasitgl required date." : GoTo selesai
        End If
        'paposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ppaposting required numeric." : GoTo selesai
        End If
        'patutupperiode(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ppatutupperiode required numeric." : GoTo selesai
        End If
        'paisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ppaisclose required numeric." : GoTo selesai
        End If
        'pacustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "ppacustomint1 required numeric." : GoTo selesai
        End If
        'pacustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "ppacustomint2 required numeric." : GoTo selesai
        End If
        'pacustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "ppacustomint3 required numeric." : GoTo selesai
        End If
        'pacustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ppacustomdbl1 required numeric." : GoTo selesai
        End If
        'pacustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ppacustomdbl2 required numeric." : GoTo selesai
        End If
        'pacustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ppacustomdbl3 required numeric." : GoTo selesai
        End If
        'pacustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "ppacustomdate1 required date." : GoTo selesai
        End If
        'pacustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "ppacustomdate2 required date." : GoTo selesai
        End If
        'pacustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "ppacustomdate3 required date." : GoTo selesai
        End If
        'pakategori(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "ppakategori required numeric." : GoTo selesai
        Else
            If dataUtama(43) <> 0 And dataUtama(43) <> 1 And dataUtama(43) <> 2 Then
                result(2) = "Invalid pakategori value." : GoTo selesai
            End If
        End If
        'ppajenis(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "ppajenis required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ppacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ppacabang should not be more than 25 character." : GoTo selesai
        End If

        'palokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ppalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ppalokasi should not be more than 25 character." : GoTo selesai
        End If

        'pasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "ppasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "ppasumber should not be more than 10 character." : GoTo selesai
        End If

        'panotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ppanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ppanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'patgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ppatgl can't be empty" : GoTo selesai
        End If

        'patglberlakusampai(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "ppatglberlakusampai can't be empty" : GoTo selesai
        End If

        'pabagianpakontak(11) As String
        'If Len(dataUtama(11)) = 0 Then
        '    result(2) = "pabagianpakontak can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(11)) > 100 Then
            result(2) = "ppabagianppakontak should not be more than 100 character." : GoTo selesai
        End If

        'pamatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ppamatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "ppamatauang should not be more than 25 character." : GoTo selesai
        End If

        'pakurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "ppakurs can't be empty" : GoTo selesai
        End If

        'patglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "ppatglnoref can't be empty" : GoTo selesai
        End If

        'painputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "ppainputtgl can't be empty" : GoTo selesai
        End If

        'pamodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "ppamodifikasitgl can't be empty" : GoTo selesai
        End If

        'pacustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "ppacustomdbl1 can't be empty" : GoTo selesai
        End If

        'pacustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "ppacustomdbl2 can't be empty" : GoTo selesai
        End If

        'pacustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "ppacustomdbl3 can't be empty" : GoTo selesai
        End If

        'pacustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ppacustomdate1 can't be empty" : GoTo selesai
        End If

        'pacustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "ppacustomdate2 can't be empty" : GoTo selesai
        End If

        'pacustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "ppacustomdate3 can't be empty" : GoTo selesai
        End If

        'pakategoriharga(44) As String
        If dataUtama(43) = 1 And Len(dataUtama(44)) = 0 Then
            result(2) = "ppakategoripos can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(44)) > 25 Then
            result(2) = "ppakategoripos should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ppaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppaautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppatgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppatglberlakusampai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppabagianppa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppabagianppakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppamatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppakurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppanoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppatglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppaposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppatutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppaisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppakategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppakategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppajenis", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "ppaid~ppacabang~ppalokasi~ppagudang~ppasumber~ppaautonotransaksi~ppanotransaksi~ppatgl~ppatglberlakusampai~ppakodepa~ppabagianppa~ppabagianppakontak~ppamatauang~ppakurs~ppauraian~ppacatatan~ppanoref~ppatglnoref~ppastatus~ppastatussebelumnya~ppajmlrevisi~ppacetakanke~ppainputuser~ppainputtgl~ppamodifikasiuser~ppamodifikasitgl~ppaposting~ppatutupperiode~ppaisclose~ppacustomtext1~ppacustomtext2~ppacustomtext3~ppacustomtext4~ppacustomtext5~ppacustomint1~ppacustomint2~ppacustomint3~ppacustomdbl1~ppacustomdbl2~ppacustomdbl3~ppacustomdate1~ppacustomdate2~ppacustomdate3~ppakategori~ppakategoripos~ppajenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idppadetail(0) As Integer, idppa(1) As Integer, idbarang(2) As Integer, satuan(3) As String, nilaisatuan(4) As Double, 
        'satuanbarang(5) As String, matauang(6) As String, kurs(7) As Double, hargajual1lama(8) As Double, hargajual2lama(9) As Double, 
        'hargajual3lama(10) As Double, hargajual4lama(11) As Double, hargajual5lama(12) As Double, hargajual1(13) As Double, hargajual2(14) As Double, 
        'hargajual3(15) As Double, hargajual4(16) As Double, hargajual5(17) As Double, diskonjual1lama(18) As Double, diskonjual2lama(19) As Double, 
        'diskonjual3lama(20) As Double, diskonjual4lama(21) As Double, diskonjual5lama(22) As Double, diskonjual1(23) As Double, diskonjual2(24) As Double, 
        'diskonjual3(25) As Double, diskonjual4(26) As Double, diskonjual5(27) As Double, cabang(28) As String, lokasi(29) As String, 
        'gudang(30) As String, costcenter(31) As String, divisi(32) As String, subdivisi(33) As String, proyek(34) As String, 
        'catatan(35) As String, urutan(36) As Integer, statusberlaku(37) As Integer, isclose(38) As Integer, customtext1(39) As String, 
        'customtext2(40) As String, customtext3(41) As String, customdbl1(42) As Double, customdbl2(43) As Double, customdbl3(44) As Double, 
        'customdate1(45) As Date, customdate2(46) As Date, customdate3(47) As Date, stokminimallama(48) As Integer, stokminimal(49) As Integer
        'stokmaksimallama(50) As Integer, stokmaksimal(51) As Integer, stokreorderlama(52) As Integer, stokreorder(53) As Integer
        'stokminorderlama(54) As Integer, stokminorder(55) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idppadetail, idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, 
        'kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, 
        'hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, 
        'diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, 
        'cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, stokminimallama, stokminimal
        'stokmaksimallama, stokmaksimal, stokreorderlama, stokreorder
        'stokminorderlama, stokminorder

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idppadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idppa", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "stokminimallama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokmaksimallama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokreorderlama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokreorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokminorderlama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokminorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin5", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 62) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idppadetail required numeric." : GoTo selesai
            End If
            'idpa(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idppa required numeric." : GoTo selesai
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
            'stokminimallama(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - stokminimallama required numeric." : GoTo selesai
            End If
            'stokminimal(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - stokminimal required numeric." : GoTo selesai
            End If
            'stokmaksimallama(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - stokmaksimallama required numeric." : GoTo selesai
            End If
            'stokmaksimal(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - stokmaksimal required numeric." : GoTo selesai
            End If
            'stokreorderlama(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - stokreorderlama required numeric." : GoTo selesai
            End If
            'stokreorder(53) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - stokreorder required numeric." : GoTo selesai
            End If
            'stokminorderlama(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - stokminorderlama required numeric." : GoTo selesai
            End If
            'stokminorder(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - stokminorder required numeric." : GoTo selesai
            End If
            'hargabeli(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - hargabeli required numeric." : GoTo selesai
            End If
            'margin1(57) As Double
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - margin1 required numeric." : GoTo selesai
            End If
            'margin2(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - margin2 required numeric." : GoTo selesai
            End If
            'margin3(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - margin3 required numeric." : GoTo selesai
            End If
            'margin4(60) As Double
            If (IsNumeric(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - margin4 required numeric." : GoTo selesai
            End If
            'margin4(61) As Double
            If (IsNumeric(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - margin5 required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idppadetail~idppa~idbarang~satuan~nilaisatuan~satuanbarang~matauang~kurs~hargajual1lama~hargajual2lama~hargajual3lama~hargajual4lama~hargajual5lama~hargajual1~hargajual2~hargajual3~hargajual4~hargajual5~diskonjual1lama~diskonjual2lama~diskonjual3lama~diskonjual4lama~diskonjual5lama~diskonjual1~diskonjual2~diskonjual3~diskonjual4~diskonjual5~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~statusberlaku~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~stokminimallama~stokminimal~stokmaksimallama~stokmaksimal~stokreorderlama~stokreorder~stokminorderlama~stokminorder~hargabeli~margin1~margin2~margin3~margin4~margin5", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61)) = False Then
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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 66
                Select Case drutama("ppastatus")
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
                'If drutama("ppastatus") = 2 Then
                '    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                '    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                '    Dim rsCekHakAkses As String = HakAkses(3, 8, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                '    If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK HAK AKSES ===================================


                If isUpdate Then
                    result(4) = drutama("ppaid")
                    notransaksi = drutama("ppanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(ppaid), ppanotransaksi FROM M_12_ppa WHERE ppaid='" & result(4) & "' AND ppastatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ppaid) FROM m_12_ppa WHERE ppanotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m3_pa_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pasumber")) & "▼" & FixQuotes(drutama("paid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_PPa set ppacabang  = '" & FixQuotes(drutama("ppacabang")) & "', ppalokasi  = '" & FixQuotes(drutama("ppalokasi")) & "', ppagudang  = '" & FixQuotes(drutama("ppagudang")) & "', ppasumber  = '" & FixQuotes(drutama("ppasumber")) & "', ppaautonotransaksi  = " & drutama("ppaautonotransaksi") & ", ppanotransaksi  = '" & notransaksi & "', ppatgl  = '" & FixQuotes(AsFormatTanggal(drutama("ppatgl"))) & "', ppatglberlakusampai  = '" & FixQuotes(AsFormatTanggal(drutama("ppatglberlakusampai"))) & "', ppakodepa  = " & drutama("ppakodepa") & ", ppabagianppa  = " & drutama("ppabagianppa") & ", ppabagianppakontak  = '" & FixQuotes(drutama("ppabagianppakontak")) & "', ppamatauang  = '" & FixQuotes(drutama("ppamatauang")) & "', ppakurs  = '" & FixDouble(drutama("ppakurs")) & "', ppauraian  = '" & FixQuotes(drutama("ppauraian")) & "', ppacatatan  = '" & FixQuotes(drutama("ppacatatan")) & "', ppanoref  = '" & FixQuotes(drutama("ppanoref")) & "', ppatglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ppatglnoref"))) & "', ppastatus  = " & drutama("ppastatus") & ", ppastatussebelumnya  = " & drutama("ppastatussebelumnya") & ", ppajmlrevisi  = ppajmlrevisi+1, ppacetakanke  = " & drutama("ppacetakanke") & ", ppamodifikasiuser  = " & drutama("ppamodifikasiuser") & ", ppamodifikasitgl  = NOW(), ppaposting  = 0, ppatutupperiode  = " & drutama("ppatutupperiode") & ", ppacustomtext1  = '" & FixQuotes(drutama("ppacustomtext1")) & "', ppacustomtext2  = '" & FixQuotes(drutama("ppacustomtext2")) & "', ppacustomtext3  = '" & FixQuotes(drutama("ppacustomtext3")) & "', ppacustomtext4  = '" & FixQuotes(drutama("ppacustomtext4")) & "', ppacustomtext5  = '" & FixQuotes(drutama("ppacustomtext5")) & "', ppacustomint1  = " & drutama("ppacustomint1") & ", ppacustomint2  = " & drutama("ppacustomint2") & ", ppacustomint3  = " & drutama("ppacustomint3") & ", ppacustomdbl1  = '" & FixDouble(drutama("ppacustomdbl1")) & "', ppacustomdbl2  = '" & FixDouble(drutama("ppacustomdbl2")) & "', ppacustomdbl3  = '" & FixDouble(drutama("ppacustomdbl3")) & "', ppacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate1"))) & "', ppacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate2"))) & "', ppacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate3"))) & "', ppakategori = '" & FixQuotes(drutama("ppakategori")) & "', ppakategoripos = '" & FixQuotes(drutama("ppakategoripos")) & "', ppajenis = '" & FixQuotes(drutama("ppajenis")) & "' where ppaid = '" & drutama("ppaid") & "'"
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

                    If drutama("ppaautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ppacabang"), drutama("ppalokasi"), drutama("ppasumber"), drutama("ppatgl"))
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
                        notransaksi = drutama("ppanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ppaid) FROM m_12_ppa WHERE ppanotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Ppa (ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, ppatgl, ppatglberlakusampai, ppakodepa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppatutupperiode, ppaisclose, ppacustomtext1, ppacustomtext2, ppacustomtext3, ppacustomtext4, ppacustomtext5, ppacustomint1, ppacustomint2, ppacustomint3, ppacustomdbl1, ppacustomdbl2, ppacustomdbl3, ppacustomdate1, ppacustomdate2, ppacustomdate3, ppakategori, ppakategoripos, ppajenis) values('" & FixQuotes(drutama("ppacabang")) & "', '" & FixQuotes(drutama("ppalokasi")) & "', '" & FixQuotes(drutama("ppagudang")) & "', '" & FixQuotes(drutama("ppasumber")) & "', " & drutama("ppaautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ppatgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppatglberlakusampai"))) & "', " & drutama("ppakodepa") & ", " & drutama("ppabagianppa") & ", '" & FixQuotes(drutama("ppabagianppakontak")) & "', '" & FixQuotes(drutama("ppamatauang")) & "', '" & FixDouble(drutama("ppakurs")) & "', '" & FixQuotes(drutama("ppauraian")) & "', '" & FixQuotes(drutama("ppacatatan")) & "', '" & FixQuotes(drutama("ppanoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppatglnoref"))) & "', " & drutama("ppastatus") & ", " & drutama("ppastatussebelumnya") & ", " & drutama("ppajmlrevisi") & ", " & drutama("ppacetakanke") & ", " & drutama("ppainputuser") & ", NOW(), " & drutama("ppamodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ppatutupperiode") & ", " & drutama("ppaisclose") & ", '" & FixQuotes(drutama("ppacustomtext1")) & "', '" & FixQuotes(drutama("ppacustomtext2")) & "', '" & FixQuotes(drutama("ppacustomtext3")) & "', '" & FixQuotes(drutama("ppacustomtext4")) & "', '" & FixQuotes(drutama("ppacustomtext5")) & "', " & drutama("ppacustomint1") & ", " & drutama("ppacustomint2") & ", " & drutama("ppacustomint3") & ", '" & FixDouble(drutama("ppacustomdbl1")) & "', '" & FixDouble(drutama("ppacustomdbl2")) & "', '" & FixDouble(drutama("ppacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate3"))) & "', '" & FixQuotes(drutama("ppakategori")) & "', '" & FixQuotes(drutama("ppakategoripos")) & "', '" & FixQuotes(drutama("ppajenis")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select ppaid from M_12_ppa where ppanotransaksi='" & notransaksi & "' AND ppainputuser= '" & userid & "' order by ppamodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ppa_Detail where idppa = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idppadetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargajual1lama")) & "', '" & FixDouble(dr1("hargajual2lama")) & "', '" & FixDouble(dr1("hargajual3lama")) & "', '" & FixDouble(dr1("hargajual4lama")) & "', '" & FixDouble(dr1("hargajual5lama")) & "', '" & FixDouble(dr1("hargajual1")) & "', '" & FixDouble(dr1("hargajual2")) & "', '" & FixDouble(dr1("hargajual3")) & "', '" & FixDouble(dr1("hargajual4")) & "', '" & FixDouble(dr1("hargajual5")) & "', '" & FixDouble(dr1("diskonjual1lama")) & "', '" & FixDouble(dr1("diskonjual2lama")) & "', '" & FixDouble(dr1("diskonjual3lama")) & "', '" & FixDouble(dr1("diskonjual4lama")) & "', '" & FixDouble(dr1("diskonjual5lama")) & "', '" & FixDouble(dr1("diskonjual1")) & "', '" & FixDouble(dr1("diskonjual2")) & "', '" & FixDouble(dr1("diskonjual3")) & "', '" & FixDouble(dr1("diskonjual4")) & "', '" & FixDouble(dr1("diskonjual5")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusberlaku") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("stokminimallama")) & "', '" & FixQuotes(dr1("stokminimal")) & "', '" & FixQuotes(dr1("stokmaksimallama")) & "', '" & FixQuotes(dr1("stokmaksimal")) & "', '" & FixQuotes(dr1("stokreorderlama")) & "', '" & FixQuotes(dr1("stokreorder")) & "', '" & FixQuotes(dr1("stokminorderlama")) & "', '" & FixQuotes(dr1("stokminorder")) & "', '" & FixQuotes(dr1("hargabeli")) & "', '" & FixQuotes(dr1("margin1")) & "', '" & FixQuotes(dr1("margin2")) & "', '" & FixQuotes(dr1("margin3")) & "', '" & FixQuotes(dr1("margin4")) & "', '" & FixQuotes(dr1("margin5")) & "')")
                    Next
                    sql = "Insert into M_12_Ppa_Detail(idppadetail, idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3,stokminimallama,stokminimal,stokmaksimallama,stokmaksimal,stokreorderlama,stokreorder,stokminorderlama,stokminorder, hargabeli, margin1, margin2, margin3, margin4, margin5) values" & strValue2.ToString & ""
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

                ''UPDATE HARGA KE DATA BARANG POS =================================================
                'If drutama("ppastatus") = 2 Then
                '    'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                '    'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                '    If drutama("ppakategori") = 1 Then
                '        'UPDATE HARGA LAMA KE TABEL DETAIL (M_12_PPA_DETAIL)
                '        'sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang SET ppad.hargajual1lama = pi.pihargajual1, ppad.hargajual2lama = pi.pihargajual2, ppad.hargajual3lama = pi.pihargajual3, ppad.hargajual4lama = pi.pihargajual4, ppad.hargajual5lama = pi.pihargajual5, ppad.diskonjual1lama = pi.pidiskonjual1, ppad.diskonjual2lama = pi.pidiskonjual2, ppad.diskonjual3lama = pi.pidiskonjual3, ppad.diskonjual4lama = pi.pidiskonjual4, ppad.diskonjual5lama = pi.pidiskonjual5, ppad.stokminimallama = pi.pistokminimal, ppad.stokmaksimallama = pi.pistokmaksimal, ppad.stokreorderlama = pi.pistokreorder, ppad.stokminorderlama = pi.pistokminorder WHERE ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "'"
                '        sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "' SET ppad.hargajual1lama = pi.pihargajual1, ppad.hargajual2lama = pi.pihargajual2, ppad.hargajual3lama = pi.pihargajual3, ppad.hargajual4lama = pi.pihargajual4, ppad.hargajual5lama = pi.pihargajual5, ppad.diskonjual1lama = pi.pidiskonjual1, ppad.diskonjual2lama = pi.pidiskonjual2, ppad.diskonjual3lama = pi.pidiskonjual3, ppad.diskonjual4lama = pi.pidiskonjual4, ppad.diskonjual5lama = pi.pidiskonjual5, ppad.stokminimallama = pi.pistokminimal, ppad.stokmaksimallama = pi.pistokmaksimal, ppad.stokreorderlama = pi.pistokreorder, ppad.stokminorderlama = pi.pistokminorder "
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '        'UPDATE HARGA BARU KE TABEL BARANG (M_12_POS_ITEM)
                '        'sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang SET pi.pihargajual1 = ppad.hargajual1, pi.pihargajual2 = ppad.hargajual2, pi.pihargajual3 = ppad.hargajual3, pi.pihargajual4 = ppad.hargajual4, pi.pihargajual5 = ppad.hargajual5, pi.pidiskonjual1 = ppad.diskonjual1, pi.pidiskonjual2 = ppad.diskonjual2, pi.pidiskonjual3 = ppad.diskonjual3, pi.pidiskonjual4 = ppad.diskonjual4, pi.pidiskonjual5 = ppad.diskonjual5, pi.pistokminimal = ppad.stokminimal, pi.pistokmaksimal = ppad.stokmaksimal, pi.pistokreorder = ppad.stokreorder, pi.pistokminorder = ppad.stokminorder WHERE ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "'"
                '        sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "' SET pi.pihargajual1 = ppad.hargajual1, pi.pihargajual2 = ppad.hargajual2, pi.pihargajual3 = ppad.hargajual3, pi.pihargajual4 = ppad.hargajual4, pi.pihargajual5 = ppad.hargajual5, pi.pidiskonjual1 = ppad.diskonjual1, pi.pidiskonjual2 = ppad.diskonjual2, pi.pidiskonjual3 = ppad.diskonjual3, pi.pidiskonjual4 = ppad.diskonjual4, pi.pidiskonjual5 = ppad.diskonjual5, pi.pistokminimal = ppad.stokminimal, pi.pistokmaksimal = ppad.stokmaksimal, pi.pistokreorder = ppad.stokreorder, pi.pistokminorder = ppad.stokminorder "
                '        'result(2) = sql : Trans.Rollback() : GoTo selesai
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '    Else

                '        'UPDATE HARGA LAMA KE TABEL DETAIL (M_12_PPA_DETAIL)
                '        sql = "UPDATE m_12_ppa_detail ppad JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' SET ppad.hargajual1lama = pi.pihargajual1, ppad.hargajual2lama = pi.pihargajual2, ppad.hargajual3lama = pi.pihargajual3, ppad.hargajual4lama = pi.pihargajual4, ppad.hargajual5lama = pi.pihargajual5, ppad.diskonjual1lama = pi.pidiskonjual1, ppad.diskonjual2lama = pi.pidiskonjual2, ppad.diskonjual3lama = pi.pidiskonjual3, ppad.diskonjual4lama = pi.pidiskonjual4, ppad.diskonjual5lama = pi.pidiskonjual5, ppad.stokminimallama = pi.pistokminimal, ppad.stokmaksimallama = pi.pistokmaksimal, ppad.stokreorderlama = pi.pistokreorder, ppad.stokminorderlama = pi.pistokminorder "
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '        'UPDATE HARGA BARU KE TABEL BARANG (M_12_POS_ITEM)
                '        sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' SET pi.pihargajual1 = ppad.hargajual1, pi.pihargajual2 = ppad.hargajual2, pi.pihargajual3 = ppad.hargajual3, pi.pihargajual4 = ppad.hargajual4, pi.pihargajual5 = ppad.hargajual5, pi.pidiskonjual1 = ppad.diskonjual1, pi.pidiskonjual2 = ppad.diskonjual2, pi.pidiskonjual3 = ppad.diskonjual3, pi.pidiskonjual4 = ppad.diskonjual4, pi.pidiskonjual5 = ppad.diskonjual5, pi.pistokminimal = ppad.stokminimal, pi.pistokmaksimal = ppad.stokmaksimal, pi.pistokreorder = ppad.stokreorder, pi.pistokminorder = ppad.stokminorder "
                '        'result(2) = sql : Trans.Rollback() : GoTo selesai
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '    End If

                'End If
                ''END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ppastatus") = 2 Then
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
                    'Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                    'If PostingJurnal.Equals("0") = False Then
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                    End If
                    'End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================


                'INSERT USER LOG ====================================================================
                ' Dim sumber As String = "PPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M12_PpaUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("ppabagianppakode", "c1.kkode")
            Filter = Filter.Replace("ppabagianppanama", "c1.knama")
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
            Dim sumber As String = "PPA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim pakategori As Integer = 0, pakategoriharga As String = ""

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0, '' FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ppatgl, Ppanotransaksi, Ppastatus, Ppakategori, Ppakategoripos FROM m_12_Ppa WHERE Ppaid='" & idtransaksi & "'", myConn)
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
                nilaiStatus = "Ppastatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m3_pa_history
            'Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'UPDATE HARGA KE MASTER DATA BARANG =================================================
                'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                'If pakategori = 0 Then
                '    'UPDATE HARGA LAMA KE TABEL BARANG (M1_ITEM)
                '    sql = "UPDATE m_12_ppa_detail ppad JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang SET pi.pihargajual1 = ppad.hargajual1lama, pi.pihargajual2 = ppad.hargajual2lama, pi.pihargajual3 = ppad.hargajual3lama, pi.pihargajual4 = ppad.hargajual4lama, pi.pihargajual5 = ppad.hargajual5lama, pi.pidiskonjual1 = ppad.diskonjual1lama, pi.pidiskonjual2 = ppad.diskonjual2lama, pi.pidiskonjual3 = ppad.diskonjual3lama, pi.pidiskonjual4 = ppad.diskonjual4lama, pi.pidiskonjual5 = ppad.diskonjual5lama, pi.pistokminimal = ppad.stokminimallama, pi.pistokmaksimal = ppad.stokmaksimallama, pi.pistokreorder = ppad.stokreorderlama, pi.pistokminorder = ppad.stokminorderlama WHERE ppad.idppa = '" & FixDouble(result(4)) & "'"
                'Else
                '    'UPDATE HARGA LAMA KE TABEL HARGA BARANG PER KATEGORI (M1_PRICE_CATEGORY_DETAIL) SESUAI IDBARANG DAN KATEGORI HARGA BARANG
                '    sql = "UPDATE m3_pa_detail pad JOIN m1_price_category_detail i ON i.pcdkategori = '" & pakategoriharga & "' AND pad.idbarang = i.pcdidbarang SET i.pcdhargajual1 = pad.hargajual1lama, i.pcdhargajual2 = pad.hargajual2lama, i.pcdhargajual3 = pad.hargajual3lama, i.pcdhargajual4 = pad.hargajual4lama, i.pcdhargajual5 = pad.hargajual5lama, i.pcddiskonjual1 = pad.diskonjual1lama, i.pcddiskonjual2 = pad.diskonjual2lama, i.pcddiskonjual3 = pad.diskonjual3lama, i.pcddiskonjual4 = pad.diskonjual4lama, i.pcddiskonjual5 = pad.diskonjual5lama WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                'End If
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()

                'END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================
            End If

            'update status utama
            sql = "UPDATE M_12_Ppa SET Ppastatus = " & nilaiStatus & ", Ppamodifikasiuser='" & userid & "', Ppamodifikasitgl = NOW(), Ppaposting = 0, Ppapostingtgl = '1971-01-01 00:00:00', Ppajmlrevisi = Ppajmlrevisi + 1 WHERE Ppaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_PpaSearch(PostWsSearch(paramSplit(0), "M12_PpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
            Filter = Filter.Replace("ppabagianppakode", "c1.kkode")
            Filter = Filter.Replace("ppabagianppanama", "c1.knama")
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
            Dim sumber As String = "Ppa", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Paid, Ppanotransaksi FROM m_12_Ppa WHERE Ppaid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ppacabang, ppalokasi, ppasumber, ppaautonotransaksi, ppanotransaksi, ppatgl"
            sql &= " FROM M_12_ppa"
            sql &= " WHERE ppaid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ppacabang")
                lokasi = dtNomorNext.Rows(0)("ppalokasi")
                sumber = dtNomorNext.Rows(0)("ppasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ppaautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ppanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ppatgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Ppa_Detail WHERE idppa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Ppa WHERE ppaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_PpaSearch(PostWsSearch(paramSplit(0), "M12_PpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_PpaSimpanOld(ByVal param As String) As String
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
        'ppaid(0) As Integer, ppacabang(1) As String, ppalokasi(2) As String, ppagudang(3) As String, ppasumber(4) As String, 
        'ppaautonotransaksi(5) As Integer, ppanotransaksi(6) As String, ppatgl(7) As Date, ppatglberlakusamppai(8) As Date, ppakodeppa(9) As Integer, 
        'ppabagianppa(10) As Integer, ppabagianppakontak(11) As String, ppamatauang(12) As String, ppakurs(13) As Double, ppauraian(14) As String, 
        'ppacatatan(15) As String, ppanoref(16) As String, ppatglnoref(17) As Date, ppastatus(18) As Integer, ppastatussebelumnya(19) As Integer, 
        'ppajmlrevisi(20) As Integer, ppacetakanke(21) As Integer, ppainputuser(22) As Integer, ppainputtgl(23) As DateTime, ppamodifikasiuser(24) As Integer, 
        'ppamodifikasitgl(25) As DateTime, ppaposting(26) As Integer, ppatutupperiode(27) As Integer, ppaisclose(28) As Integer, ppacustomtext1(29) As String, 
        'ppacustomtext2(30) As String, ppacustomtext3(31) As String, ppacustomtext4(32) As String, ppacustomtext5(33) As String, ppacustomint1(34) As Integer, 
        'ppacustomint2(35) As Integer, ppacustomint3(36) As Integer, ppacustomdbl1(37) As Double, ppacustomdbl2(38) As Double, ppacustomdbl3(39) As Double, 
        'ppacustomdate1(40) As Date, ppacustomdate2(41) As Date, ppacustomdate3(42) As Date, ppakategori(43) As Integer, ppakategoripos(44) As String, ppajenis(45) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ppaid, ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, 
        'ppatgl, ppatglberlakusamppai, ppakodeppa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, 
        'ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, 
        'ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppatutupperiode, 
        'ppaisclose, ppacustomtext1, ppacustomtext2, ppacustomtext3, ppacustomtext4, ppacustomtext5, ppacustomint1, 
        'ppacustomint2, ppacustomint3, ppacustomdbl1, ppacustomdbl2, ppacustomdbl3, ppacustomdate1, ppacustomdate2, 
        'ppacustomdate3, ppakategori, ppakategoripos, ppajenis

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 46) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'paid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ppaid required numeric." : GoTo selesai
        End If
        'paautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "ppaautonotransaksi required numeric." : GoTo selesai
        End If
        'patgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ppatgl required date." : GoTo selesai
        End If
        'patglberlakusampai(8) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "ppatglberlakusampai required date." : GoTo selesai
        End If
        'pakodepa(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "ppakodepa required numeric." : GoTo selesai
        End If
        'pabagianpa(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "ppabagianppa required numeric." : GoTo selesai
        End If
        If (dataUtama(10) < 1) Then
            result(2) = "ppabagianppa can't be empty." : GoTo selesai
        End If
        'pakurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ppakurs required numeric." : GoTo selesai
        End If
        'patglnoref(17) As Date
        If (IsDate(dataUtama(17)) = False) Then
            result(2) = "ppatglnoref required date." : GoTo selesai
        End If
        'pastatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "ppastatus required numeric." : GoTo selesai
        End If
        'pastatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "ppastatussebelumnya required numeric." : GoTo selesai
        End If
        'pajmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "ppajmlrevisi required numeric." : GoTo selesai
        End If
        'pacetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "ppacetakanke required numeric." : GoTo selesai
        End If
        'painputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ppainputuser required numeric." : GoTo selesai
        End If
        'painputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "ppainputtgl required date." : GoTo selesai
        End If
        'pamodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "ppamodifikasiuser required numeric." : GoTo selesai
        End If
        'pamodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "ppamodifikasitgl required date." : GoTo selesai
        End If
        'paposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ppaposting required numeric." : GoTo selesai
        End If
        'patutupperiode(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "ppatutupperiode required numeric." : GoTo selesai
        End If
        'paisclose(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ppaisclose required numeric." : GoTo selesai
        End If
        'pacustomint1(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "ppacustomint1 required numeric." : GoTo selesai
        End If
        'pacustomint2(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "ppacustomint2 required numeric." : GoTo selesai
        End If
        'pacustomint3(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "ppacustomint3 required numeric." : GoTo selesai
        End If
        'pacustomdbl1(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ppacustomdbl1 required numeric." : GoTo selesai
        End If
        'pacustomdbl2(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "ppacustomdbl2 required numeric." : GoTo selesai
        End If
        'pacustomdbl3(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ppacustomdbl3 required numeric." : GoTo selesai
        End If
        'pacustomdate1(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "ppacustomdate1 required date." : GoTo selesai
        End If
        'pacustomdate2(41) As Date
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "ppacustomdate2 required date." : GoTo selesai
        End If
        'pacustomdate3(42) As Date
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "ppacustomdate3 required date." : GoTo selesai
        End If
        'pakategori(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "ppakategori required numeric." : GoTo selesai
        Else
            If dataUtama(43) <> 0 And dataUtama(43) <> 1 Then
                result(2) = "Invalid pakategori value." : GoTo selesai
            End If
        End If
        'ppajenis(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "ppajenis required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ppacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ppacabang should not be more than 25 character." : GoTo selesai
        End If

        'palokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "ppalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "ppalokasi should not be more than 25 character." : GoTo selesai
        End If

        'pasumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "ppasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "ppasumber should not be more than 10 character." : GoTo selesai
        End If

        'panotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ppanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ppanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'patgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ppatgl can't be empty" : GoTo selesai
        End If

        'patglberlakusampai(8) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "ppatglberlakusampai can't be empty" : GoTo selesai
        End If

        'pabagianpakontak(11) As String
        'If Len(dataUtama(11)) = 0 Then
        '    result(2) = "pabagianpakontak can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(11)) > 100 Then
            result(2) = "ppabagianppakontak should not be more than 100 character." : GoTo selesai
        End If

        'pamatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ppamatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "ppamatauang should not be more than 25 character." : GoTo selesai
        End If

        'pakurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "ppakurs can't be empty" : GoTo selesai
        End If

        'patglnoref(17) As Date
        If Len(dataUtama(17)) = 0 Then
            result(2) = "ppatglnoref can't be empty" : GoTo selesai
        End If

        'painputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "ppainputtgl can't be empty" : GoTo selesai
        End If

        'pamodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "ppamodifikasitgl can't be empty" : GoTo selesai
        End If

        'pacustomdbl1(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "ppacustomdbl1 can't be empty" : GoTo selesai
        End If

        'pacustomdbl2(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "ppacustomdbl2 can't be empty" : GoTo selesai
        End If

        'pacustomdbl3(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "ppacustomdbl3 can't be empty" : GoTo selesai
        End If

        'pacustomdate1(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ppacustomdate1 can't be empty" : GoTo selesai
        End If

        'pacustomdate2(41) As Date
        If Len(dataUtama(41)) = 0 Then
            result(2) = "ppacustomdate2 can't be empty" : GoTo selesai
        End If

        'pacustomdate3(42) As Date
        If Len(dataUtama(42)) = 0 Then
            result(2) = "ppacustomdate3 can't be empty" : GoTo selesai
        End If

        'pakategoriharga(44) As String
        If dataUtama(43) = 1 And Len(dataUtama(44)) = 0 Then
            result(2) = "ppakategoripos can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(44)) > 25 Then
            result(2) = "ppakategoripos should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ppaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppagudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppaautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppatgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppatglberlakusampai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppabagianppa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppabagianppakontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppamatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppakurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppanoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppatglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppaposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppatutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppaisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppacustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppakategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ppakategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ppajenis", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "ppaid~ppacabang~ppalokasi~ppagudang~ppasumber~ppaautonotransaksi~ppanotransaksi~ppatgl~ppatglberlakusampai~ppakodepa~ppabagianppa~ppabagianppakontak~ppamatauang~ppakurs~ppauraian~ppacatatan~ppanoref~ppatglnoref~ppastatus~ppastatussebelumnya~ppajmlrevisi~ppacetakanke~ppainputuser~ppainputtgl~ppamodifikasiuser~ppamodifikasitgl~ppaposting~ppatutupperiode~ppaisclose~ppacustomtext1~ppacustomtext2~ppacustomtext3~ppacustomtext4~ppacustomtext5~ppacustomint1~ppacustomint2~ppacustomint3~ppacustomdbl1~ppacustomdbl2~ppacustomdbl3~ppacustomdate1~ppacustomdate2~ppacustomdate3~ppakategori~ppakategoripos~ppajenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idppadetail(0) As Integer, idppa(1) As Integer, idbarang(2) As Integer, satuan(3) As String, nilaisatuan(4) As Double, 
        'satuanbarang(5) As String, matauang(6) As String, kurs(7) As Double, hargajual1lama(8) As Double, hargajual2lama(9) As Double, 
        'hargajual3lama(10) As Double, hargajual4lama(11) As Double, hargajual5lama(12) As Double, hargajual1(13) As Double, hargajual2(14) As Double, 
        'hargajual3(15) As Double, hargajual4(16) As Double, hargajual5(17) As Double, diskonjual1lama(18) As Double, diskonjual2lama(19) As Double, 
        'diskonjual3lama(20) As Double, diskonjual4lama(21) As Double, diskonjual5lama(22) As Double, diskonjual1(23) As Double, diskonjual2(24) As Double, 
        'diskonjual3(25) As Double, diskonjual4(26) As Double, diskonjual5(27) As Double, cabang(28) As String, lokasi(29) As String, 
        'gudang(30) As String, costcenter(31) As String, divisi(32) As String, subdivisi(33) As String, proyek(34) As String, 
        'catatan(35) As String, urutan(36) As Integer, statusberlaku(37) As Integer, isclose(38) As Integer, customtext1(39) As String, 
        'customtext2(40) As String, customtext3(41) As String, customdbl1(42) As Double, customdbl2(43) As Double, customdbl3(44) As Double, 
        'customdate1(45) As Date, customdate2(46) As Date, customdate3(47) As Date, stokminimallama(48) As Integer, stokminimal(49) As Integer
        'stokmaksimallama(50) As Integer, stokmaksimal(51) As Integer, stokreorderlama(52) As Integer, stokreorder(53) As Integer
        'stokminorderlama(54) As Integer, stokminorder(55) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idppadetail, idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, 
        'kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, 
        'hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, 
        'diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, 
        'cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, stokminimallama, stokminimal
        'stokmaksimallama, stokmaksimal, stokreorderlama, stokreorder
        'stokminorderlama, stokminorder

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idppadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idppa", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "stokminimallama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokmaksimallama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokreorderlama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokreorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokminorderlama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "stokminorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "margin5", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 62) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpadetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idppadetail required numeric." : GoTo selesai
            End If
            'idpa(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idppa required numeric." : GoTo selesai
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
            'stokminimallama(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - stokminimallama required numeric." : GoTo selesai
            End If
            'stokminimal(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - stokminimal required numeric." : GoTo selesai
            End If
            'stokmaksimallama(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - stokmaksimallama required numeric." : GoTo selesai
            End If
            'stokmaksimal(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - stokmaksimal required numeric." : GoTo selesai
            End If
            'stokreorderlama(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - stokreorderlama required numeric." : GoTo selesai
            End If
            'stokreorder(53) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - stokreorder required numeric." : GoTo selesai
            End If
            'stokminorderlama(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - stokminorderlama required numeric." : GoTo selesai
            End If
            'stokminorder(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - stokminorder required numeric." : GoTo selesai
            End If
            'hargabeli(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - hargabeli required numeric." : GoTo selesai
            End If
            'margin1(57) As Double
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - margin1 required numeric." : GoTo selesai
            End If
            'margin2(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - margin2 required numeric." : GoTo selesai
            End If
            'margin3(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - margin3 required numeric." : GoTo selesai
            End If
            'margin4(60) As Double
            If (IsNumeric(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - margin4 required numeric." : GoTo selesai
            End If
            'margin4(61) As Double
            If (IsNumeric(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - margin5 required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idppadetail~idppa~idbarang~satuan~nilaisatuan~satuanbarang~matauang~kurs~hargajual1lama~hargajual2lama~hargajual3lama~hargajual4lama~hargajual5lama~hargajual1~hargajual2~hargajual3~hargajual4~hargajual5~diskonjual1lama~diskonjual2lama~diskonjual3lama~diskonjual4lama~diskonjual5lama~diskonjual1~diskonjual2~diskonjual3~diskonjual4~diskonjual5~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~statusberlaku~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~stokminimallama~stokminimal~stokmaksimallama~stokmaksimal~stokreorderlama~stokreorder~stokminorderlama~stokminorder~hargabeli~margin1~margin2~margin3~margin4~margin5", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61)) = False Then
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


                ''CEK HAK AKSES ==========================================
                'If drutama("ppastatus") = 2 Then
                '    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                '    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                '    Dim rsCekHakAkses As String = HakAkses(3, 8, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                '    If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK HAK AKSES ===================================


                If isUpdate Then
                    result(4) = drutama("ppaid")
                    notransaksi = drutama("ppanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ppaid), ppanotransaksi FROM M_12_ppa WHERE ppaid='" & result(4) & "' AND ppastatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ppaid) FROM m_12_ppa WHERE ppanotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m3_pa_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pasumber")) & "▼" & FixQuotes(drutama("paid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_PPa set ppacabang  = '" & FixQuotes(drutama("ppacabang")) & "', ppalokasi  = '" & FixQuotes(drutama("ppalokasi")) & "', ppagudang  = '" & FixQuotes(drutama("ppagudang")) & "', ppasumber  = '" & FixQuotes(drutama("ppasumber")) & "', ppaautonotransaksi  = " & drutama("ppaautonotransaksi") & ", ppanotransaksi  = '" & notransaksi & "', ppatgl  = '" & FixQuotes(AsFormatTanggal(drutama("ppatgl"))) & "', ppatglberlakusampai  = '" & FixQuotes(AsFormatTanggal(drutama("ppatglberlakusampai"))) & "', ppakodepa  = " & drutama("ppakodepa") & ", ppabagianppa  = " & drutama("ppabagianppa") & ", ppabagianppakontak  = '" & FixQuotes(drutama("ppabagianppakontak")) & "', ppamatauang  = '" & FixQuotes(drutama("ppamatauang")) & "', ppakurs  = '" & FixDouble(drutama("ppakurs")) & "', ppauraian  = '" & FixQuotes(drutama("ppauraian")) & "', ppacatatan  = '" & FixQuotes(drutama("ppacatatan")) & "', ppanoref  = '" & FixQuotes(drutama("ppanoref")) & "', ppatglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ppatglnoref"))) & "', ppastatus  = " & drutama("ppastatus") & ", ppastatussebelumnya  = " & drutama("ppastatussebelumnya") & ", ppajmlrevisi  = ppajmlrevisi+1, ppacetakanke  = " & drutama("ppacetakanke") & ", ppamodifikasiuser  = " & drutama("ppamodifikasiuser") & ", ppamodifikasitgl  = NOW(), ppaposting  = 0, ppatutupperiode  = " & drutama("ppatutupperiode") & ", ppacustomtext1  = '" & FixQuotes(drutama("ppacustomtext1")) & "', ppacustomtext2  = '" & FixQuotes(drutama("ppacustomtext2")) & "', ppacustomtext3  = '" & FixQuotes(drutama("ppacustomtext3")) & "', ppacustomtext4  = '" & FixQuotes(drutama("ppacustomtext4")) & "', ppacustomtext5  = '" & FixQuotes(drutama("ppacustomtext5")) & "', ppacustomint1  = " & drutama("ppacustomint1") & ", ppacustomint2  = " & drutama("ppacustomint2") & ", ppacustomint3  = " & drutama("ppacustomint3") & ", ppacustomdbl1  = '" & FixDouble(drutama("ppacustomdbl1")) & "', ppacustomdbl2  = '" & FixDouble(drutama("ppacustomdbl2")) & "', ppacustomdbl3  = '" & FixDouble(drutama("ppacustomdbl3")) & "', ppacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate1"))) & "', ppacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate2"))) & "', ppacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate3"))) & "', ppakategori = '" & FixQuotes(drutama("ppakategori")) & "', ppakategoripos = '" & FixQuotes(drutama("ppakategoripos")) & "', ppajenis = '" & FixQuotes(drutama("ppajenis")) & "' where ppaid = '" & drutama("ppaid") & "'"
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

                    If drutama("ppaautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ppacabang"), drutama("ppalokasi"), drutama("ppasumber"), drutama("ppatgl"))
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
                        notransaksi = drutama("ppanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ppaid) FROM m_12_ppa WHERE ppanotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_Ppa (ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, ppatgl, ppatglberlakusampai, ppakodepa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppatutupperiode, ppaisclose, ppacustomtext1, ppacustomtext2, ppacustomtext3, ppacustomtext4, ppacustomtext5, ppacustomint1, ppacustomint2, ppacustomint3, ppacustomdbl1, ppacustomdbl2, ppacustomdbl3, ppacustomdate1, ppacustomdate2, ppacustomdate3, ppakategori, ppakategoripos, ppajenis) values('" & FixQuotes(drutama("ppacabang")) & "', '" & FixQuotes(drutama("ppalokasi")) & "', '" & FixQuotes(drutama("ppagudang")) & "', '" & FixQuotes(drutama("ppasumber")) & "', " & drutama("ppaautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ppatgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppatglberlakusampai"))) & "', " & drutama("ppakodepa") & ", " & drutama("ppabagianppa") & ", '" & FixQuotes(drutama("ppabagianppakontak")) & "', '" & FixQuotes(drutama("ppamatauang")) & "', '" & FixDouble(drutama("ppakurs")) & "', '" & FixQuotes(drutama("ppauraian")) & "', '" & FixQuotes(drutama("ppacatatan")) & "', '" & FixQuotes(drutama("ppanoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppatglnoref"))) & "', " & drutama("ppastatus") & ", " & drutama("ppastatussebelumnya") & ", " & drutama("ppajmlrevisi") & ", " & drutama("ppacetakanke") & ", " & drutama("ppainputuser") & ", NOW(), " & drutama("ppamodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ppatutupperiode") & ", " & drutama("ppaisclose") & ", '" & FixQuotes(drutama("ppacustomtext1")) & "', '" & FixQuotes(drutama("ppacustomtext2")) & "', '" & FixQuotes(drutama("ppacustomtext3")) & "', '" & FixQuotes(drutama("ppacustomtext4")) & "', '" & FixQuotes(drutama("ppacustomtext5")) & "', " & drutama("ppacustomint1") & ", " & drutama("ppacustomint2") & ", " & drutama("ppacustomint3") & ", '" & FixDouble(drutama("ppacustomdbl1")) & "', '" & FixDouble(drutama("ppacustomdbl2")) & "', '" & FixDouble(drutama("ppacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ppacustomdate3"))) & "', '" & FixQuotes(drutama("ppakategori")) & "', '" & FixQuotes(drutama("ppakategoripos")) & "', '" & FixQuotes(drutama("ppajenis")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select ppaid from M_12_ppa where ppanotransaksi='" & notransaksi & "' AND ppainputuser= '" & userid & "' order by ppamodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Ppa_Detail where idppa = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idppadetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("hargajual1lama")) & "', '" & FixDouble(dr1("hargajual2lama")) & "', '" & FixDouble(dr1("hargajual3lama")) & "', '" & FixDouble(dr1("hargajual4lama")) & "', '" & FixDouble(dr1("hargajual5lama")) & "', '" & FixDouble(dr1("hargajual1")) & "', '" & FixDouble(dr1("hargajual2")) & "', '" & FixDouble(dr1("hargajual3")) & "', '" & FixDouble(dr1("hargajual4")) & "', '" & FixDouble(dr1("hargajual5")) & "', '" & FixDouble(dr1("diskonjual1lama")) & "', '" & FixDouble(dr1("diskonjual2lama")) & "', '" & FixDouble(dr1("diskonjual3lama")) & "', '" & FixDouble(dr1("diskonjual4lama")) & "', '" & FixDouble(dr1("diskonjual5lama")) & "', '" & FixDouble(dr1("diskonjual1")) & "', '" & FixDouble(dr1("diskonjual2")) & "', '" & FixDouble(dr1("diskonjual3")) & "', '" & FixDouble(dr1("diskonjual4")) & "', '" & FixDouble(dr1("diskonjual5")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusberlaku") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("stokminimallama")) & "', '" & FixQuotes(dr1("stokminimal")) & "', '" & FixQuotes(dr1("stokmaksimallama")) & "', '" & FixQuotes(dr1("stokmaksimal")) & "', '" & FixQuotes(dr1("stokreorderlama")) & "', '" & FixQuotes(dr1("stokreorder")) & "', '" & FixQuotes(dr1("stokminorderlama")) & "', '" & FixQuotes(dr1("stokminorder")) & "', '" & FixQuotes(dr1("hargabeli")) & "', '" & FixQuotes(dr1("margin1")) & "', '" & FixQuotes(dr1("margin2")) & "', '" & FixQuotes(dr1("margin3")) & "', '" & FixQuotes(dr1("margin4")) & "', '" & FixQuotes(dr1("margin5")) & "')")
                    Next
                    sql = "Insert into M_12_Ppa_Detail(idppadetail, idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3,stokminimallama,stokminimal,stokmaksimallama,stokmaksimal,stokreorderlama,stokreorder,stokminorderlama,stokminorder, hargabeli, margin1, margin2, margin3, margin4, margin5) values" & strValue2.ToString & ""
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

                ''UPDATE HARGA KE DATA BARANG POS =================================================
                'If drutama("ppastatus") = 2 Then
                '    'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                '    'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                '    If drutama("ppakategori") = 1 Then
                '        'UPDATE HARGA LAMA KE TABEL DETAIL (M_12_PPA_DETAIL)
                '        'sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang SET ppad.hargajual1lama = pi.pihargajual1, ppad.hargajual2lama = pi.pihargajual2, ppad.hargajual3lama = pi.pihargajual3, ppad.hargajual4lama = pi.pihargajual4, ppad.hargajual5lama = pi.pihargajual5, ppad.diskonjual1lama = pi.pidiskonjual1, ppad.diskonjual2lama = pi.pidiskonjual2, ppad.diskonjual3lama = pi.pidiskonjual3, ppad.diskonjual4lama = pi.pidiskonjual4, ppad.diskonjual5lama = pi.pidiskonjual5, ppad.stokminimallama = pi.pistokminimal, ppad.stokmaksimallama = pi.pistokmaksimal, ppad.stokreorderlama = pi.pistokreorder, ppad.stokminorderlama = pi.pistokminorder WHERE ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "'"
                '        sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "' SET ppad.hargajual1lama = pi.pihargajual1, ppad.hargajual2lama = pi.pihargajual2, ppad.hargajual3lama = pi.pihargajual3, ppad.hargajual4lama = pi.pihargajual4, ppad.hargajual5lama = pi.pihargajual5, ppad.diskonjual1lama = pi.pidiskonjual1, ppad.diskonjual2lama = pi.pidiskonjual2, ppad.diskonjual3lama = pi.pidiskonjual3, ppad.diskonjual4lama = pi.pidiskonjual4, ppad.diskonjual5lama = pi.pidiskonjual5, ppad.stokminimallama = pi.pistokminimal, ppad.stokmaksimallama = pi.pistokmaksimal, ppad.stokreorderlama = pi.pistokreorder, ppad.stokminorderlama = pi.pistokminorder "
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '        'UPDATE HARGA BARU KE TABEL BARANG (M_12_POS_ITEM)
                '        'sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang SET pi.pihargajual1 = ppad.hargajual1, pi.pihargajual2 = ppad.hargajual2, pi.pihargajual3 = ppad.hargajual3, pi.pihargajual4 = ppad.hargajual4, pi.pihargajual5 = ppad.hargajual5, pi.pidiskonjual1 = ppad.diskonjual1, pi.pidiskonjual2 = ppad.diskonjual2, pi.pidiskonjual3 = ppad.diskonjual3, pi.pidiskonjual4 = ppad.diskonjual4, pi.pidiskonjual5 = ppad.diskonjual5, pi.pistokminimal = ppad.stokminimal, pi.pistokmaksimal = ppad.stokmaksimal, pi.pistokreorder = ppad.stokreorder, pi.pistokminorder = ppad.stokminorder WHERE ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "'"
                '        sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' AND pi.pikategori ='" & drutama("ppakategoripos") & "' SET pi.pihargajual1 = ppad.hargajual1, pi.pihargajual2 = ppad.hargajual2, pi.pihargajual3 = ppad.hargajual3, pi.pihargajual4 = ppad.hargajual4, pi.pihargajual5 = ppad.hargajual5, pi.pidiskonjual1 = ppad.diskonjual1, pi.pidiskonjual2 = ppad.diskonjual2, pi.pidiskonjual3 = ppad.diskonjual3, pi.pidiskonjual4 = ppad.diskonjual4, pi.pidiskonjual5 = ppad.diskonjual5, pi.pistokminimal = ppad.stokminimal, pi.pistokmaksimal = ppad.stokmaksimal, pi.pistokreorder = ppad.stokreorder, pi.pistokminorder = ppad.stokminorder "
                '        'result(2) = sql : Trans.Rollback() : GoTo selesai
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '    Else

                '        'UPDATE HARGA LAMA KE TABEL DETAIL (M_12_PPA_DETAIL)
                '        sql = "UPDATE m_12_ppa_detail ppad JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' SET ppad.hargajual1lama = pi.pihargajual1, ppad.hargajual2lama = pi.pihargajual2, ppad.hargajual3lama = pi.pihargajual3, ppad.hargajual4lama = pi.pihargajual4, ppad.hargajual5lama = pi.pihargajual5, ppad.diskonjual1lama = pi.pidiskonjual1, ppad.diskonjual2lama = pi.pidiskonjual2, ppad.diskonjual3lama = pi.pidiskonjual3, ppad.diskonjual4lama = pi.pidiskonjual4, ppad.diskonjual5lama = pi.pidiskonjual5, ppad.stokminimallama = pi.pistokminimal, ppad.stokmaksimallama = pi.pistokmaksimal, ppad.stokreorderlama = pi.pistokreorder, ppad.stokminorderlama = pi.pistokminorder "
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '        'UPDATE HARGA BARU KE TABEL BARANG (M_12_POS_ITEM)
                '        sql = "UPDATE m_12_ppa ppa JOIN m_12_ppa_detail ppad ON ppa.ppaid = ppad.idppa JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang AND ppad.idppa = '" & FixDouble(result(4)) & "' SET pi.pihargajual1 = ppad.hargajual1, pi.pihargajual2 = ppad.hargajual2, pi.pihargajual3 = ppad.hargajual3, pi.pihargajual4 = ppad.hargajual4, pi.pihargajual5 = ppad.hargajual5, pi.pidiskonjual1 = ppad.diskonjual1, pi.pidiskonjual2 = ppad.diskonjual2, pi.pidiskonjual3 = ppad.diskonjual3, pi.pidiskonjual4 = ppad.diskonjual4, pi.pidiskonjual5 = ppad.diskonjual5, pi.pistokminimal = ppad.stokminimal, pi.pistokmaksimal = ppad.stokmaksimal, pi.pistokreorder = ppad.stokreorder, pi.pistokminorder = ppad.stokminorder "
                '        'result(2) = sql : Trans.Rollback() : GoTo selesai
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()

                '    End If

                'End If
                ''END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("ppastatus") = 2 Then
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
                    'Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                    'If PostingJurnal.Equals("0") = False Then
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                    End If
                    'End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================


                'INSERT USER LOG ====================================================================
                ' Dim sumber As String = "PPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M12_PpaUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("ppabagianppakode", "c1.kkode")
            Filter = Filter.Replace("ppabagianppanama", "c1.knama")
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
            Dim sumber As String = "PPA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim pakategori As Integer = 0, pakategoriharga As String = ""

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0, 0, '' FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ppatgl, Ppanotransaksi, Ppastatus, Ppakategori, Ppakategoripos FROM m_12_Ppa WHERE Ppaid='" & idtransaksi & "'")
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
                nilaiStatus = "Ppastatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m3_pa_history
            'Dim rsSimpanHistory As String = SimpanHistory.M3_Pa_HistorySimpan("" & paramSplit(0) & "★M3_Pa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'UPDATE HARGA KE MASTER DATA BARANG =================================================
                'JIKA PAKATEGORI = 0 (GLOBAL) MAKA UPDATE HARGA KE M1_ITEM
                'JIKA PAKATEGORI = 1 (PER KATEGORI) MAKA UPDATE HARGA KE M1_PRICE_CATEGORY_DETAIL
                'If pakategori = 0 Then
                '    'UPDATE HARGA LAMA KE TABEL BARANG (M1_ITEM)
                '    sql = "UPDATE m_12_ppa_detail ppad JOIN m_12_pos_item pi ON ppad.idbarang = pi.piidbarang SET pi.pihargajual1 = ppad.hargajual1lama, pi.pihargajual2 = ppad.hargajual2lama, pi.pihargajual3 = ppad.hargajual3lama, pi.pihargajual4 = ppad.hargajual4lama, pi.pihargajual5 = ppad.hargajual5lama, pi.pidiskonjual1 = ppad.diskonjual1lama, pi.pidiskonjual2 = ppad.diskonjual2lama, pi.pidiskonjual3 = ppad.diskonjual3lama, pi.pidiskonjual4 = ppad.diskonjual4lama, pi.pidiskonjual5 = ppad.diskonjual5lama, pi.pistokminimal = ppad.stokminimallama, pi.pistokmaksimal = ppad.stokmaksimallama, pi.pistokreorder = ppad.stokreorderlama, pi.pistokminorder = ppad.stokminorderlama WHERE ppad.idppa = '" & FixDouble(result(4)) & "'"
                'Else
                '    'UPDATE HARGA LAMA KE TABEL HARGA BARANG PER KATEGORI (M1_PRICE_CATEGORY_DETAIL) SESUAI IDBARANG DAN KATEGORI HARGA BARANG
                '    sql = "UPDATE m3_pa_detail pad JOIN m1_price_category_detail i ON i.pcdkategori = '" & pakategoriharga & "' AND pad.idbarang = i.pcdidbarang SET i.pcdhargajual1 = pad.hargajual1lama, i.pcdhargajual2 = pad.hargajual2lama, i.pcdhargajual3 = pad.hargajual3lama, i.pcdhargajual4 = pad.hargajual4lama, i.pcdhargajual5 = pad.hargajual5lama, i.pcddiskonjual1 = pad.diskonjual1lama, i.pcddiskonjual2 = pad.diskonjual2lama, i.pcddiskonjual3 = pad.diskonjual3lama, i.pcddiskonjual4 = pad.diskonjual4lama, i.pcddiskonjual5 = pad.diskonjual5lama WHERE pad.idpa = '" & FixDouble(result(4)) & "'"
                'End If
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = Con1
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()

                'END OF UPDATE HARGA KE MASTER DATA BARANG ==========================================
            End If

            'update status utama
            sql = "UPDATE M_12_Ppa SET Ppastatus = " & nilaiStatus & ", Ppamodifikasiuser='" & userid & "', Ppamodifikasitgl = NOW(), Ppaposting = 0, Ppapostingtgl = '1971-01-01 00:00:00', Ppajmlrevisi = Ppajmlrevisi + 1 WHERE Ppaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_PpaSearch(PostWsSearch(paramSplit(0), "M12_PpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
            Filter = Filter.Replace("ppabagianppakode", "c1.kkode")
            Filter = Filter.Replace("ppabagianppanama", "c1.knama")
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
            Dim sumber As String = "Ppa", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Paid, Ppanotransaksi FROM m_12_Ppa WHERE Ppaid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ppacabang, ppalokasi, ppasumber, ppaautonotransaksi, ppanotransaksi, ppatgl"
            sql &= " FROM M_12_ppa"
            sql &= " WHERE ppaid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ppacabang")
                lokasi = dtNomorNext.Rows(0)("ppalokasi")
                sumber = dtNomorNext.Rows(0)("ppasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ppaautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ppanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ppatgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Ppa_Detail WHERE idppa = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Ppa WHERE ppaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_PpaSearch(PostWsSearch(paramSplit(0), "M12_PpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_PpaGetdataById(ByVal param As String) As String

        'M12_PpaGetdataById Utama --------------------------------------------------------
        'ppaid, ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, 
        'ppatgl, ppatglberlakusampai, ppakodepa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, 
        'ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, 
        'ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppapostingtgl, 
        'ppatutupperiode, ppaisclose, ppacustomtext1, ppacustomtext2, ppacustomtext3, ppacustomtext4, ppacustomtext5, 
        'ppacustomint1, ppacustomint2, ppacustomint3, ppacustomdbl1, ppacustomdbl2, ppacustomdbl3, ppacustomdate1, 
        'ppacustomdate2, ppacustomdate3, ppacabangnama, ppalokasinama, ppagudangnama, ppabagianppakode, ppabagianppanama, 
        'ppastatusnama, ppastatussebelumnyanama, ppainputusernama, ppamodifikasiusernama,
        'ppakategori, ppakategorinama, ppakategoripos, ppakategoriposnama, ppajenis

        'M3_PaGetdataById Detail -------------------------------------------------------
        'idppadetail, idppa, idbarang, 
        'satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, 
        'hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, 
        'hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, 
        'diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, 
        'lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, stokminimallama, stokmaksimallama, stokreorderlama, stokminorderlama,
        'stokminimal, stokmaksimal, stokreorder, stokminorder, hargabeli, margin1, margin2, margin3, margin4, margin5


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
            Filter = "ppaid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ppaid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select ppa.ppaid AS ppaid, ppa.ppacabang AS ppacabang, ppa.ppalokasi AS ppalokasi, ppa.ppagudang AS ppagudang, ppa.ppasumber AS ppasumber, ppa.ppaautonotransaksi AS ppaautonotransaksi, ppa.ppanotransaksi AS ppanotransaksi, ppa.ppatgl AS ppatgl, ppa.ppatglberlakusampai AS ppatglberlakusampai, ppa.ppakodepa AS ppakodepa, ppa.ppabagianppa AS ppabagianppa, ppa.ppabagianppakontak AS ppabagianppakontak, ppa.ppamatauang AS ppamatauang, ppa.ppakurs AS ppakurs, ppa.ppauraian AS ppauraian, ppa.ppacatatan AS ppacatatan, ppa.ppanoref AS ppanoref, ppa.ppatglnoref AS ppatglnoref, ppa.ppastatus AS ppastatus, ppa.ppastatussebelumnya AS ppastatussebelumnya, ppa.ppajmlrevisi AS ppajmlrevisi, ppa.ppacetakanke AS ppacetakanke, ppa.ppainputuser AS ppainputuser, ppa.ppainputtgl AS ppainputtgl, ppa.ppamodifikasiuser AS ppamodifikasiuser, ppa.ppamodifikasitgl AS ppamodifikasitgl, ppa.ppaposting AS ppaposting, ppa.ppapostingtgl AS ppapostingtgl, ppa.ppatutupperiode AS ppatutupperiode, ppa.ppaisclose AS ppaisclose, ppa.ppacustomtext1 AS ppacustomtext1, ppa.ppacustomtext2 AS ppacustomtext2, ppa.ppacustomtext3 AS ppacustomtext3, ppa.ppacustomtext4 AS ppacustomtext4, ppa.ppacustomtext5 AS ppacustomtext5, ppa.ppacustomint1 AS ppacustomint1, ppa.ppacustomint2 AS ppacustomint2, ppa.ppacustomint3 AS ppacustomint3, ppa.ppacustomdbl1 AS ppacustomdbl1, ppa.ppacustomdbl2 AS ppacustomdbl2, ppa.ppacustomdbl3 AS ppacustomdbl3, ppa.ppacustomdate1 AS ppacustomdate1, ppa.ppacustomdate2 AS ppacustomdate2, ppa.ppacustomdate3 AS ppacustomdate3, br.bnama AS ppacabangnama, lc.lnama AS ppalokasinama, wh.wnama AS ppagudangnama, c1.kkode AS ppabagianppakode, c1.knama AS ppabagianppanama, st1.nama AS ppastatusnama, st2.nama AS ppastatussebelumnyanama, u1.unama AS ppainputusernama, u2.unama AS ppamodifikasiusernama, ppa.ppakategori, (CASE ppa.ppakategori WHEN 0 THEN 'Global' ELSE 'Category' END) as ppakategorinama, ppa.ppakategoripos, pc.pcnama as ppakategoriposnama, ppa.ppajenis, ppad.idppadetail AS idppadetail, ppad.idppa AS idppa, ppad.idbarang AS idbarang, ppad.satuan AS satuan, ppad.nilaisatuan AS nilaisatuan, ppad.satuanbarang AS satuanbarang, ppad.matauang AS matauang, ppad.kurs AS kurs, ppad.hargajual1lama AS hargajual1lama, ppad.hargajual2lama AS hargajual2lama, ppad.hargajual3lama AS hargajual3lama, ppad.hargajual4lama AS hargajual4lama, ppad.hargajual5lama AS hargajual5lama, ppad.hargajual1 AS hargajual1, ppad.hargajual2 AS hargajual2, ppad.hargajual3 AS hargajual3, ppad.hargajual4 AS hargajual4, ppad.hargajual5 AS hargajual5, ppad.diskonjual1lama AS diskonjual1lama, ppad.diskonjual2lama AS diskonjual2lama, ppad.diskonjual3lama AS diskonjual3lama, ppad.diskonjual4lama AS diskonjual4lama, ppad.diskonjual5lama AS diskonjual5lama, ppad.diskonjual1 AS diskonjual1, ppad.diskonjual2 AS diskonjual2, ppad.diskonjual3 AS diskonjual3, ppad.diskonjual4 AS diskonjual4, ppad.diskonjual5 AS diskonjual5, ppad.cabang AS cabang, ppad.lokasi AS lokasi, ppad.gudang AS gudang, ppad.costcenter AS costcenter, ppad.divisi AS divisi, ppad.subdivisi AS subdivisi, ppad.proyek AS proyek, ppad.catatan AS catatan, ppad.urutan AS urutan, ppad.statusberlaku AS statusberlaku, ppad.isclose AS isclose, ppad.customtext1 AS customtext1, ppad.customtext2 AS customtext2, ppad.customtext3 AS customtext3, ppad.customdbl1 AS customdbl1, ppad.customdbl2 AS customdbl2, ppad.customdbl3 AS customdbl3, ppad.customdate1 AS customdate1, ppad.customdate2 AS customdate2, ppad.customdate3 AS customdate3, i.bkode AS kodebarang, i.bnama AS namabarang, i.btipe AS tipebarang, brd.bnama AS cabangnama, lcd.lnama AS lokasinama, whd.wnama AS gudangnama, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama, ppad.stokminimallama AS stokminimallama, ppad.stokmaksimallama AS stokmaksimallama, ppad.stokreorderlama AS stokreorderlama, ppad.stokminorderlama AS stokminorderlama, ppad.stokminimal AS stokminimal, ppad.stokmaksimal AS stokmaksimal, ppad.stokreorder AS stokreorder, ppad.stokminorder AS stokminorder, ppad.hargabeli AS hargabeli, ppad.margin1 AS margin1, ppad.margin2 AS margin2, ppad.margin3 AS margin3, ppad.margin4 AS margin4, ppad.margin5 AS margin5  from m_12_ppa ppa join m_12_ppa_detail ppad on ppa.ppaid = ppad.idppa join m0_status st1 on st1.kode = ppa.ppastatus join m0_status st2 on st2.kode = ppa.ppastatussebelumnya left join m1_branch br on br.bkode = ppa.ppacabang left join m1_location lc on lc.lkode = ppa.ppalokasi left join m1_warehouse wh on wh.wkode = ppa.ppagudang left join m1_contact c1 on c1.kid = ppa.ppabagianppa left join m0_user u1 on u1.userid = ppa.ppainputuser left join m0_user u2 on u2.userid = ppa.ppamodifikasiuser left join m_12_pos_category pc on ppa.ppakategoripos = pc.pckode left join m1_item i on ppad.idbarang = i.bid left join m1_branch brd on ppad.cabang = brd.bkode left join m1_location lcd on ppad.lokasi = lcd.lkode left join m1_warehouse whd on ppad.gudang = whd.wkode left join m1_cost_center cc on ppad.costcenter = cc.cckode left join m1_division d on ppad.divisi = d.dkode left join m1_subdivision sd on ppad.subdivisi = sd.sdkode left join m1_project p on ppad.proyek = p.pkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ppaid"), 0), sptField,
                     FxDB(drutama("ppacabang"), ""), sptField,
                     FxDB(drutama("ppalokasi"), ""), sptField,
                     FxDB(drutama("ppagudang"), ""), sptField,
                     FxDB(drutama("ppasumber"), ""), sptField,
                     FxDB(drutama("ppaautonotransaksi"), 0), sptField,
                     FxDB(drutama("ppanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ppatgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppatglberlakusampai"), ""), formatTgl), sptField,
                     FxDB(drutama("ppakodepa"), 0), sptField,
                     FxDB(drutama("ppabagianppa"), 0), sptField,
                     FxDB(drutama("ppabagianppakontak"), ""), sptField,
                     FxDB(drutama("ppamatauang"), ""), sptField,
                     FxDB(drutama("ppakurs"), 0), sptField,
                     FxDB(drutama("ppauraian"), ""), sptField,
                     FxDB(drutama("ppacatatan"), ""), sptField,
                     FxDB(drutama("ppanoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ppatglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("ppastatus"), 0), sptField,
                     FxDB(drutama("ppastatussebelumnya"), 0), sptField,
                     FxDB(drutama("ppajmlrevisi"), 0), sptField,
                     FxDB(drutama("ppacetakanke"), 0), sptField,
                     FxDB(drutama("ppainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppamodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ppatutupperiode"), 0), sptField,
                     FxDB(drutama("ppaisclose"), 0), sptField,
                     FxDB(drutama("ppacustomtext1"), ""), sptField,
                     FxDB(drutama("ppacustomtext2"), ""), sptField,
                     FxDB(drutama("ppacustomtext3"), ""), sptField,
                     FxDB(drutama("ppacustomtext4"), ""), sptField,
                     FxDB(drutama("ppacustomtext5"), ""), sptField,
                     FxDB(drutama("ppacustomint1"), 0), sptField,
                     FxDB(drutama("ppacustomint2"), 0), sptField,
                     FxDB(drutama("ppacustomint3"), 0), sptField,
                     FxDB(drutama("ppacustomdbl1"), 0), sptField,
                     FxDB(drutama("ppacustomdbl2"), 0), sptField,
                     FxDB(drutama("ppacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ppacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ppacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ppacabangnama"), ""), sptField,
                     FxDB(drutama("ppalokasinama"), ""), sptField,
                     FxDB(drutama("ppagudangnama"), ""), sptField,
                     FxDB(drutama("ppabagianppakode"), ""), sptField,
                     FxDB(drutama("ppabagianppanama"), ""), sptField,
                     FxDB(drutama("ppastatusnama"), ""), sptField,
                     FxDB(drutama("ppastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("ppainputusernama"), ""), sptField,
                     FxDB(drutama("ppamodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ppakategori"), 0), sptField,
                     FxDB(drutama("ppakategorinama"), ""), sptField,
                     FxDB(drutama("ppakategoripos"), ""), sptField,
                     FxDB(drutama("ppakategoriposnama"), ""), sptField,
                     FxDB(drutama("ppajenis"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idppadetail"), 0), sptField,
                     FxDB(dr("idppa"), 0), sptField,
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
                     FxDB(dr("stokminimallama"), 0), sptField,
                     FxDB(dr("stokmaksimallama"), 0), sptField,
                     FxDB(dr("stokreorderlama"), 0), sptField,
                     FxDB(dr("stokminorderlama"), 0), sptField,
                     FxDB(dr("stokminimal"), 0), sptField,
                     FxDB(dr("stokmaksimal"), 0), sptField,
                     FxDB(dr("stokreorder"), 0), sptField,
                     FxDB(dr("stokminorder"), 0), sptField,
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("margin1"), 0), sptField,
                     FxDB(dr("margin2"), 0), sptField,
                     FxDB(dr("margin3"), 0), sptField,
                     FxDB(dr("margin4"), 0), sptField,
                     FxDB(dr("margin5"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppaid, ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, ppatgl, ppatglberlakusampai, ppakodepa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppapostingtgl, ppatutupperiode, ppaisclose, ppacustomtext1, ppacustomtext2, ppacustomtext3, ppacustomtext4, ppacustomtext5, ppacustomint1, ppacustomint2, ppacustomint3, ppacustomdbl1, ppacustomdbl2, ppacustomdbl3, ppacustomdate1, ppacustomdate2, ppacustomdate3, ppacabangnama, ppalokasinama, ppagudangnama, ppabagianppakode, ppabagianppanama, ppastatusnama, ppastatussebelumnyanama, ppainputusernama, ppamodifikasiusernama,ppakategori, ppakategorinama, ppakategoripos, ppakategoriposnama, ppajenis" & sptSubParam & "idppadetail, idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, stokminimallama, stokmaksimallama, stokreorderlama, stokminorderlama,stokminimal, stokmaksimal, stokreorder, stokminorder, hargabeli, margin1, margin2, margin3, margin4, margin5"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_PpaSearch(ByVal param As String) As String
        'M12_PpaSearch --------------------------------------------------------
        'ppaid, ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, 
        'ppatgl, ppatglberlakusampai, ppakodepa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, 
        'ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, 
        'ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppapostingtgl, 
        'ppatutupperiode, ppaisclose, ppacabangnama, ppalokasinama, ppagudangnama, ppabagianppakode, ppabagianppanama, 
        'ppastatusnama, ppastatussebelumnyanama, ppainputusernama, ppamodifikasiusernama,
        'ppakategori, ppakategorinama, ppakategoripos, ppakategoriposnama

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
            Filter = Filter.Replace("ppabagianppakode", "c1.kkode")
            Filter = Filter.Replace("ppabagianppanama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        sql = "select ppa.ppaid AS ppaid, ppa.ppacabang AS ppacabang, ppa.ppalokasi AS ppalokasi, ppa.ppagudang AS ppagudang, ppa.ppasumber AS ppasumber, ppa.ppaautonotransaksi AS ppaautonotransaksi, ppa.ppanotransaksi AS ppanotransaksi, ppa.ppatgl AS ppatgl, ppa.ppatglberlakusampai AS ppatglberlakusampai, ppa.ppakodepa AS ppakodepa, ppa.ppabagianppa AS ppabagianppa, ppa.ppabagianppakontak AS ppabagianppakontak, ppa.ppamatauang AS ppamatauang, ppa.ppakurs AS ppakurs, ppa.ppauraian AS ppauraian, ppa.ppacatatan AS ppacatatan, ppa.ppanoref AS ppanoref, ppa.ppatglnoref AS ppatglnoref, ppa.ppastatus AS ppastatus, ppa.ppastatussebelumnya AS ppastatussebelumnya, ppa.ppajmlrevisi AS ppajmlrevisi, ppa.ppacetakanke AS ppacetakanke, ppa.ppainputuser AS ppainputuser, ppa.ppainputtgl AS ppainputtgl, ppa.ppamodifikasiuser AS ppamodifikasiuser, ppa.ppamodifikasitgl AS ppamodifikasitgl, ppa.ppaposting AS ppaposting, ppa.ppapostingtgl AS ppapostingtgl, ppa.ppatutupperiode AS ppatutupperiode, ppa.ppaisclose AS ppaisclose, br.bnama AS ppacabangnama, lc.lnama AS ppalokasinama, wh.wnama AS ppagudangnama, c1.kkode AS ppabagianppakode, c1.knama AS ppabagianppanama, st1.nama AS ppastatusnama, st2.nama AS ppastatussebelumnyanama, u1.unama AS ppainputusernama, u2.unama AS ppamodifikasiusernama, ppa.ppakategori, (CASE ppa.ppakategori WHEN 0 THEN 'All Category' ELSE 'Per Category' END) as ppakategorinama, ppa.ppakategoripos, pc.pcnama as ppakategoriposnama from m_12_ppa ppa join m0_status st1 on st1.kode = ppa.ppastatus join m0_status st2 on st2.kode = ppa.ppastatussebelumnya left join m1_branch br on br.bkode = ppa.ppacabang left join m1_location lc on lc.lkode = ppa.ppalokasi left join m1_warehouse wh on wh.wkode = ppa.ppagudang left join m1_contact c1 on c1.kid = ppa.ppabagianppa left join m0_user u1 on u1.userid = ppa.ppainputuser left join m0_user u2 on u2.userid = ppa.ppamodifikasiuser left join m_12_pos_category pc on ppa.ppakategoripos = pc.pckode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Pa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ppaid"), 0), sptField,
                     FxDB(dr("ppacabang"), ""), sptField,
                     FxDB(dr("ppalokasi"), ""), sptField,
                     FxDB(dr("ppagudang"), ""), sptField,
                     FxDB(dr("ppasumber"), ""), sptField,
                     FxDB(dr("ppaautonotransaksi"), 0), sptField,
                     FxDB(dr("ppanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ppatgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ppatglberlakusampai"), ""), formatTgl), sptField,
                     FxDB(dr("ppakodepa"), 0), sptField,
                     FxDB(dr("ppabagianppa"), 0), sptField,
                     FxDB(dr("ppabagianppakontak"), ""), sptField,
                     FxDB(dr("ppamatauang"), ""), sptField,
                     FxDB(dr("ppakurs"), 0), sptField,
                     FxDB(dr("ppauraian"), ""), sptField,
                     FxDB(dr("ppacatatan"), ""), sptField,
                     FxDB(dr("ppanoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ppatglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("ppastatus"), 0), sptField,
                     FxDB(dr("ppastatussebelumnya"), 0), sptField,
                     FxDB(dr("ppajmlrevisi"), 0), sptField,
                     FxDB(dr("ppacetakanke"), 0), sptField,
                     FxDB(dr("ppainputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppamodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ppapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ppatutupperiode"), 0), sptField,
                     FxDB(dr("ppaisclose"), 0), sptField,
                     FxDB(dr("ppacabangnama"), ""), sptField,
                     FxDB(dr("ppalokasinama"), ""), sptField,
                     FxDB(dr("ppagudangnama"), ""), sptField,
                     FxDB(dr("ppabagianppakode"), ""), sptField,
                     FxDB(dr("ppabagianppanama"), ""), sptField,
                     FxDB(dr("ppastatusnama"), ""), sptField,
                     FxDB(dr("ppastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ppainputusernama"), ""), sptField,
                     FxDB(dr("ppamodifikasiusernama"), ""), sptField,
                     FxDB(dr("ppakategori"), 0), sptField,
                     FxDB(dr("ppakategorinama"), ""), sptField,
                     FxDB(dr("ppakategoripos"), ""), sptField,
                     FxDB(dr("ppakategoriposnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppaid, ppacabang, ppalokasi, ppagudang, ppasumber, ppaautonotransaksi, ppanotransaksi, ppatgl, ppatglberlakusampai, ppakodepa, ppabagianppa, ppabagianppakontak, ppamatauang, ppakurs, ppauraian, ppacatatan, ppanoref, ppatglnoref, ppastatus, ppastatussebelumnya, ppajmlrevisi, ppacetakanke, ppainputuser, ppainputtgl, ppamodifikasiuser, ppamodifikasitgl, ppaposting, ppapostingtgl, ppatutupperiode, ppaisclose, ppacabangnama, ppalokasinama, ppagudangnama, ppabagianppakode, ppabagianppanama, ppastatusnama, ppastatussebelumnyanama, ppainputusernama, ppamodifikasiusernama, ppakategori, ppakategorinama, ppakategoripos, ppakategoriposnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_PpaTerkait(ByVal param As String) As String
        'M12_PpaTerkait --------------------------------------------------------
        'ppaid, ppanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "ppaid required numeric." : GoTo selesai
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
                     FxDB(dr("ppaid"), 0), sptField,
                     FxDB(dr("ppanotransaksi"), ""), sptField,
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
            result(2) = "Related PPA data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ppaid, ppanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Ppa_Detail_VSearch(ByVal param As String) As String
        'M12_Ppa_Detail_VSearch --------------------------------------------------------
        'idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, 
        'hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, 
        'hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, 
        'diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, 
        'diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, lokasinama, 
        'gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, stokminimallama, stokmaksimallama, 
        'stokreorderlama, stokminorderlama,stokminimal, stokmaksimal, stokreorder, stokminorder, hargabeli, margin1, 
        'margin2, margin3, margin4, margin5,ppaid, ppamatauang, ppakurs, ppauraian, ppacatatan, ppakategori, ppakategorinama, 
        'ppakategoripos, ppakategoriposnama

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        sql = "select ppad.idppadetail AS idppadetail, ppad.idppa AS idppa, ppad.idbarang AS idbarang, ppad.satuan AS satuan, ppad.nilaisatuan AS nilaisatuan, ppad.satuanbarang AS satuanbarang, ppad.matauang AS matauang, ppad.kurs AS kurs, ppad.hargajual1lama AS hargajual1lama, ppad.hargajual2lama AS hargajual2lama, ppad.hargajual3lama AS hargajual3lama, ppad.hargajual4lama AS hargajual4lama, ppad.hargajual5lama AS hargajual5lama, ppad.hargajual1 AS hargajual1, ppad.hargajual2 AS hargajual2, ppad.hargajual3 AS hargajual3, ppad.hargajual4 AS hargajual4, ppad.hargajual5 AS hargajual5, ppad.diskonjual1lama AS diskonjual1lama, ppad.diskonjual2lama AS diskonjual2lama, ppad.diskonjual3lama AS diskonjual3lama, ppad.diskonjual4lama AS diskonjual4lama, ppad.diskonjual5lama AS diskonjual5lama, ppad.diskonjual1 AS diskonjual1, ppad.diskonjual2 AS diskonjual2, ppad.diskonjual3 AS diskonjual3, ppad.diskonjual4 AS diskonjual4, ppad.diskonjual5 AS diskonjual5, ppad.cabang AS cabang, ppad.lokasi AS lokasi, ppad.gudang AS gudang, ppad.costcenter AS costcenter, ppad.divisi AS divisi, ppad.subdivisi AS subdivisi, ppad.proyek AS proyek, ppad.catatan AS catatan, ppad.urutan AS urutan, ppad.statusberlaku AS statusberlaku, ppad.isclose AS isclose, ppad.customtext1 AS customtext1, ppad.customtext2 AS customtext2, ppad.customtext3 AS customtext3, ppad.customdbl1 AS customdbl1, ppad.customdbl2 AS customdbl2, ppad.customdbl3 AS customdbl3, ppad.customdate1 AS customdate1, ppad.customdate2 AS customdate2, ppad.customdate3 AS customdate3, i.bkode AS kodebarang, i.bnama AS namabarang, i.btipe AS tipebarang, brd.bnama AS cabangnama, lcd.lnama AS lokasinama, whd.wnama AS gudangnama, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama, ppad.stokminimallama AS stokminimallama, ppad.stokmaksimallama AS stokmaksimallama, ppad.stokreorderlama AS stokreorderlama, ppad.stokminorderlama AS stokminorderlama, ppad.stokminimal AS stokminimal, ppad.stokmaksimal AS stokmaksimal, ppad.stokreorder AS stokreorder, ppad.stokminorder AS stokminorder, ppad.hargabeli AS hargabeli, ppad.margin1 AS margin1, ppad.margin2 AS margin2, ppad.margin3 AS margin3, ppad.margin4 AS margin4, ppad.margin5 AS margin5,  ppa.ppaid AS ppaid, ppa.ppamatauang AS ppamatauang, ppa.ppakurs AS ppakurs, ppa.ppauraian AS ppauraian, ppa.ppacatatan AS ppacatatan, ppa.ppakategori, (CASE ppa.ppakategori WHEN 0 THEN 'Global' ELSE 'Category' END) as ppakategorinama, ppa.ppakategoripos, pc.pcnama as ppakategoriposnama, ppa.ppanotransaksi from m_12_ppa ppa join m_12_ppa_detail ppad on ppa.ppaid = ppad.idppa join m0_status st1 on st1.kode = ppa.ppastatus join m0_status st2 on st2.kode = ppa.ppastatussebelumnya left join m1_branch br on br.bkode = ppa.ppacabang left join m1_location lc on lc.lkode = ppa.ppalokasi left join m1_warehouse wh on wh.wkode = ppa.ppagudang left join m1_contact c1 on c1.kid = ppa.ppabagianppa left join m0_user u1 on u1.userid = ppa.ppainputuser left join m0_user u2 on u2.userid = ppa.ppamodifikasiuser left join m_12_pos_category pc on ppa.ppakategoripos = pc.pckode left join m1_item i on ppad.idbarang = i.bid left join m1_branch brd on ppad.cabang = brd.bkode left join m1_location lcd on ppad.lokasi = lcd.lkode left join m1_warehouse whd on ppad.gudang = whd.wkode left join m1_cost_center cc on ppad.costcenter = cc.cckode left join m1_division d on ppad.divisi = d.dkode left join m1_subdivision sd on ppad.subdivisi = sd.sdkode left join m1_project p on ppad.proyek = p.pkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("idppadetail"), 0), sptField,
                     FxDB(dr("idppa"), 0), sptField,
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
                     FxDB(dr("stokminimallama"), 0), sptField,
                     FxDB(dr("stokmaksimallama"), 0), sptField,
                     FxDB(dr("stokreorderlama"), 0), sptField,
                     FxDB(dr("stokminorderlama"), 0), sptField,
                     FxDB(dr("stokminimal"), 0), sptField,
                     FxDB(dr("stokmaksimal"), 0), sptField,
                     FxDB(dr("stokreorder"), 0), sptField,
                     FxDB(dr("stokminorder"), 0), sptField,
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("margin1"), 0), sptField,
                     FxDB(dr("margin2"), 0), sptField,
                     FxDB(dr("margin3"), 0), sptField,
                     FxDB(dr("margin4"), 0), sptField,
                     FxDB(dr("margin5"), 0), sptField,
                     FxDB(dr("ppaid"), 0), sptField,
                     FxDB(dr("ppamatauang"), 0), sptField,
                     FxDB(dr("ppakurs"), 0), sptField,
                     FxDB(dr("ppauraian"), 0), sptField,
                     FxDB(dr("ppacatatan"), 0), sptField,
                     FxDB(dr("ppakategori"), 0), sptField,
                     FxDB(dr("ppakategorinama"), 0), sptField,
                     FxDB(dr("ppakategoripos"), 0), sptField,
                     FxDB(dr("ppakategoripos"), 0), sptField,
                     FxDB(dr("ppanotransaksi"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idppadetail, idppa, idbarang, satuan, nilaisatuan, satuanbarang, matauang, kurs, hargajual1lama, hargajual2lama, hargajual3lama, hargajual4lama, hargajual5lama, hargajual1, hargajual2, hargajual3, hargajual4, hargajual5, diskonjual1lama, diskonjual2lama, diskonjual3lama, diskonjual4lama, diskonjual5lama, diskonjual1, diskonjual2, diskonjual3, diskonjual4, diskonjual5, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, statusberlaku, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, namabarang, tipebarang, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, stokminimallama, stokmaksimallama, stokreorderlama, stokminorderlama,stokminimal, stokmaksimal, stokreorder, stokminorder, hargabeli, margin1, margin2, margin3, margin4, margin5,ppaid, ppamatauang, ppakurs, ppauraian, ppacatatan, ppakategori, ppakategorinama, ppakategoripos, ppakategoriposnama, ppanotransaksi"))

        Return wsResult
    End Function
End Class