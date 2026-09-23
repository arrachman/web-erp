Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_rg
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_RgSimpan(ByVal param As String) As String
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


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        ''CEK PAGENUMBER
        'If (IsNumeric(pagingSplit(0)) = False) Then
        '    result(2) = "pageNumber required numeric." : GoTo selesai
        'End If

        ''CEK ITEMLIMIT
        'If (IsNumeric(pagingSplit(1)) = False) Then
        '    result(2) = "itemLimit required numeric." : GoTo selesai
        'End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        ''CEK FORMATTGLWAKTU
        'If Len(pagingSplit(5)) = 0 Then
        '    formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        'Else
        '    formatTglWaktu = pagingSplit(5)
        'End If
        'END OF VALIDASI PARAMETER PAGING ==================================================


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
        'rgid(0) As Integer, rgcabang(1) As String, rglokasi(2) As String, rgsumber(3) As String, rgautonotransaksi(4) As Integer, 
        'rgnotransaksi(5) As String, rgtgl(6) As Date, rgkodepa(7) As Integer, rgkontak(8) As Integer, rgkontakperson(9) As String, 
        'rguraian(10) As String, rgcatatan(11) As String, rgmatauang(12) As String, rgkurs(13) As Double, rgjumlah(14) As Double, 
        'rgjumlahvalas(15) As Double, rgstatusrgc(16) As Integer, rgstatus(17) As Integer, rgstatussebelumnya(18) As Integer, rgjmlrevisi(19) As Integer, 
        'rgcetakanke(20) As Integer, rgisclose(21) As Integer, rginputuser(22) As Integer, rginputtgl(23) As DateTime, rgmodifikasiuser(24) As Integer, 
        'rgmodifikasitgl(25) As DateTime, rgposting(26) As Integer, rgcustomtext1(27) As String, rgcustomtext2(28) As String, rgcustomtext3(29) As String, 
        'rgcustomtext4(30) As String, rgcustomtext5(31) As String, rgcustomint1(32) As Integer, rgcustomint2(33) As Integer, rgcustomint3(34) As Integer, 
        'rgcustomdbl1(35) As Double, rgcustomdbl2(36) As Double, rgcustomdbl3(37) As Double, rgcustomdate1(38) As Date, rgcustomdate2(39) As Date, 
        'rgcustomdate3(40) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rgid, rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, 
        'rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, 
        'rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, 
        'rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgcustomtext1, 
        'rgcustomtext2, rgcustomtext3, rgcustomtext4, rgcustomtext5, rgcustomint1, rgcustomint2, rgcustomint3, 
        'rgcustomdbl1, rgcustomdbl2, rgcustomdbl3, rgcustomdate1, rgcustomdate2, rgcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 41) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rgid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rgid required numeric." : GoTo selesai
        End If
        'rgautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rgautonotransaksi required numeric." : GoTo selesai
        End If
        'rgtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rgtgl required date." : GoTo selesai
        End If
        'rgkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rgkodepa required numeric." : GoTo selesai
        End If
        'rgkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rgkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "rgkontak can't be empty." : GoTo selesai
        End If
        'rgkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rgkurs required numeric." : GoTo selesai
        End If
        'rgjumlah(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rgjumlah required numeric." : GoTo selesai
        End If
        'rgjumlahvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rgjumlahvalas required numeric." : GoTo selesai
        End If
        'rgstatusrgc(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rgstatusrgc required numeric." : GoTo selesai
        End If
        'rgstatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rgstatus required numeric." : GoTo selesai
        End If
        'rgstatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rgstatussebelumnya required numeric." : GoTo selesai
        End If
        'rgjmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rgjmlrevisi required numeric." : GoTo selesai
        End If
        'rgcetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rgcetakanke required numeric." : GoTo selesai
        End If
        'rgisclose(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rgisclose required numeric." : GoTo selesai
        End If
        'rginputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rginputuser required numeric." : GoTo selesai
        End If
        'rginputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "rginputtgl required date." : GoTo selesai
        End If
        'rgmodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rgmodifikasiuser required numeric." : GoTo selesai
        End If
        'rgmodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rgmodifikasitgl required date." : GoTo selesai
        End If
        'rgposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "rgposting required numeric." : GoTo selesai
        End If
        'rgcustomint1(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rgcustomint1 required numeric." : GoTo selesai
        End If
        'rgcustomint2(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rgcustomint2 required numeric." : GoTo selesai
        End If
        'rgcustomint3(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rgcustomint3 required numeric." : GoTo selesai
        End If
        'rgcustomdbl1(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rgcustomdbl1 required numeric." : GoTo selesai
        End If
        'rgcustomdbl2(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rgcustomdbl2 required numeric." : GoTo selesai
        End If
        'rgcustomdbl3(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rgcustomdbl3 required numeric." : GoTo selesai
        End If
        'rgcustomdate1(38) As Date
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "rgcustomdate1 required date." : GoTo selesai
        End If
        'rgcustomdate2(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rgcustomdate2 required date." : GoTo selesai
        End If
        'rgcustomdate3(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rgcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rgcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rgcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rgcabang should not be more than 25 character." : GoTo selesai
        End If

        'rglokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rglokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rglokasi should not be more than 25 character." : GoTo selesai
        End If

        'rgsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rgsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rgsumber should not be more than 10 character." : GoTo selesai
        End If

        'rgnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rgnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rgnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rgtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rgtgl can't be empty" : GoTo selesai
        End If

        'rgmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "rgmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "rgmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rgkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rgkurs can't be empty" : GoTo selesai
        End If

        'rgjumlah(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rgjumlah can't be empty" : GoTo selesai
        End If

        'rgjumlahvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rgjumlahvalas can't be empty" : GoTo selesai
        End If

        'rginputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "rginputtgl can't be empty" : GoTo selesai
        End If

        'rgmodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rgmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rgcustomdbl1(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rgcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rgcustomdbl2(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rgcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rgcustomdbl3(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rgcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rgcustomdate1(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rgcustomdate1 can't be empty" : GoTo selesai
        End If

        'rgcustomdate2(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rgcustomdate2 can't be empty" : GoTo selesai
        End If

        'rgcustomdate3(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rgcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rgid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rglokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rguraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgstatusrgc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rginputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rginputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rgid~rgcabang~rglokasi~rgsumber~rgautonotransaksi~rgnotransaksi~rgtgl~rgkodepa~rgkontak~rgkontakperson~rguraian~rgcatatan~rgmatauang~rgkurs~rgjumlah~rgjumlahvalas~rgstatusrgc~rgstatus~rgstatussebelumnya~rgjmlrevisi~rgcetakanke~rgisclose~rginputuser~rginputtgl~rgmodifikasiuser~rgmodifikasitgl~rgposting~rgcustomtext1~rgcustomtext2~rgcustomtext3~rgcustomtext4~rgcustomtext5~rgcustomint1~rgcustomint2~rgcustomint3~rgcustomdbl1~rgcustomdbl2~rgcustomdbl3~rgcustomdate1~rgcustomdate2~rgcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrgdetail(0) As Integer, idrg(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, statusrgc(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrgdetail, idrg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statusrgc, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrgdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrg", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusgiro", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusrgc", AsEnumTypeData.AsInt64)
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


        'Variabel Validasi
        Dim ftExistGiro As String = "", ftGiro As String = "", vNogiro As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 27) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrgdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrgdetail required numeric." : GoTo selesai
            End If
            'idrg(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrg required numeric." : GoTo selesai
            End If
            'kontak(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljatuhtempo(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - tgljatuhtempo required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'statusgiro(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - statusgiro required numeric." : GoTo selesai
            End If
            'statusrgc(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - statusrgc required numeric." : GoTo selesai
            End If
            'isclose(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'nogiro(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
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

            'jumlah(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If

            'jumlahvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljatuhtempo(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - tgljatuhtempo can't be empty" : GoTo selesai
            End If

            'customdbl1(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrgdetail~idrg~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~statusrgc~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'BUAT FILTER VALIDASI STATUS GIRO ---------------------------
            'nogiro(2) As String
            vNogiro = dataRowDetail(2)

            'CEK DATA EXIST
            ftExistGiro = IIf(Len(ftExistGiro.ToString) = 0, "", ftExistGiro & " UNION ")
            ftExistGiro = String.Concat(ftExistGiro, "SELECT EXISTS(SELECT 1 FROM m2_giro_list WHERE glnogiro = '" & vNogiro & "' LIMIT 1) as rowExists, '" & vNogiro & "' as glnogiro")

            'Validasi Status Giro
            ftGiro = IIf(Len(ftGiro.ToString) = 0, "", ftGiro & " OR ")
            ftGiro = String.Concat(ftGiro, "(glnogiro = '" & vNogiro & "')")
            'END OF BUAT FILTER VALIDASI STATUS GIRO --------------------

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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 9
                Select Case drutama("rgstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rgtgl")), AsFormatTanggal(drutama("rgtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rgstatus") = 2 Or drutama("rgstatus") = 1 Or drutama("rgstatus") = 8 Or drutama("rgstatus") = 9 Or drutama("rgstatus") = 10 Or drutama("rgstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro, drutama("rgtgl"), formatTgl)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rgjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rgjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("rgid")
                    notransaksi = drutama("rgnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rgid), rgnotransaksi FROM M2_rg WHERE rgid='" & result(4) & "' AND rgstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rgautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rgcabang"), drutama("rglokasi"), drutama("rgsumber"), drutama("rgtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rgid) FROM m2_rg WHERE rgnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_rg_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Rg_HistorySimpan("" & paramSplit(0) & "★M2_Rg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rgsumber")) & "▼" & FixQuotes(drutama("rgid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Rg set rgcabang  = '" & FixQuotes(drutama("rgcabang")) & "', rglokasi  = '" & FixQuotes(drutama("rglokasi")) & "', rgsumber  = '" & FixQuotes(drutama("rgsumber")) & "', rgautonotransaksi  = " & drutama("rgautonotransaksi") & ", rgnotransaksi  = '" & notransaksi & "', rgtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rgtgl"))) & "', rgkodepa  = " & drutama("rgkodepa") & ", rgkontak  = " & drutama("rgkontak") & ", rgkontakperson  = '" & FixQuotes(drutama("rgkontakperson")) & "', rguraian  = '" & FixQuotes(drutama("rguraian")) & "', rgcatatan  = '" & FixQuotes(drutama("rgcatatan")) & "', rgmatauang  = '" & FixQuotes(drutama("rgmatauang")) & "', rgkurs  = '" & FixDouble(drutama("rgkurs")) & "', rgjumlah  = '" & FixDouble(drutama("rgjumlah")) & "', rgjumlahvalas  = '" & FixDouble(drutama("rgjumlahvalas")) & "', rgstatusrgc  = " & drutama("rgstatusrgc") & ", rgstatus  = " & drutama("rgstatus") & ", rgstatussebelumnya  = " & drutama("rgstatussebelumnya") & ", rgjmlrevisi  = rgjmlrevisi+1, rgcetakanke  = " & drutama("rgcetakanke") & ", rgisclose  = " & drutama("rgisclose") & ", rgmodifikasiuser  = " & drutama("rgmodifikasiuser") & ", rgmodifikasitgl  = NOW(), rgposting  = 0, rgcustomtext1  = '" & FixQuotes(drutama("rgcustomtext1")) & "', rgcustomtext2  = '" & FixQuotes(drutama("rgcustomtext2")) & "', rgcustomtext3  = '" & FixQuotes(drutama("rgcustomtext3")) & "', rgcustomtext4  = '" & FixQuotes(drutama("rgcustomtext4")) & "', rgcustomtext5  = '" & FixQuotes(drutama("rgcustomtext5")) & "', rgcustomint1  = " & drutama("rgcustomint1") & ", rgcustomint2  = " & drutama("rgcustomint2") & ", rgcustomint3  = " & drutama("rgcustomint3") & ", rgcustomdbl1  = '" & FixDouble(drutama("rgcustomdbl1")) & "', rgcustomdbl2  = '" & FixDouble(drutama("rgcustomdbl2")) & "', rgcustomdbl3  = '" & FixDouble(drutama("rgcustomdbl3")) & "', rgcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate1"))) & "', rgcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate2"))) & "', rgcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate3"))) & "' where rgid = '" & drutama("rgid") & "'"
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

                    If drutama("rgautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rgcabang"), drutama("rglokasi"), drutama("rgsumber"), drutama("rgtgl"))
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
                        notransaksi = drutama("rgnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rgid) FROM m2_rg WHERE rgnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Rg (rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgcustomtext1, rgcustomtext2, rgcustomtext3, rgcustomtext4, rgcustomtext5, rgcustomint1, rgcustomint2, rgcustomint3, rgcustomdbl1, rgcustomdbl2, rgcustomdbl3, rgcustomdate1, rgcustomdate2, rgcustomdate3) values('" & FixQuotes(drutama("rgcabang")) & "', '" & FixQuotes(drutama("rglokasi")) & "', '" & FixQuotes(drutama("rgsumber")) & "', " & drutama("rgautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rgtgl"))) & "', " & drutama("rgkodepa") & ", " & drutama("rgkontak") & ", '" & FixQuotes(drutama("rgkontakperson")) & "', '" & FixQuotes(drutama("rguraian")) & "', '" & FixQuotes(drutama("rgcatatan")) & "', '" & FixQuotes(drutama("rgmatauang")) & "', '" & FixDouble(drutama("rgkurs")) & "', '" & FixDouble(drutama("rgjumlah")) & "', '" & FixDouble(drutama("rgjumlahvalas")) & "', " & drutama("rgstatusrgc") & ", " & drutama("rgstatus") & ", " & drutama("rgstatussebelumnya") & ", " & drutama("rgjmlrevisi") & ", " & drutama("rgcetakanke") & ", " & drutama("rgisclose") & ", " & drutama("rginputuser") & ", NOW(), " & drutama("rgmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("rgcustomtext1")) & "', '" & FixQuotes(drutama("rgcustomtext2")) & "', '" & FixQuotes(drutama("rgcustomtext3")) & "', '" & FixQuotes(drutama("rgcustomtext4")) & "', '" & FixQuotes(drutama("rgcustomtext5")) & "', " & drutama("rgcustomint1") & ", " & drutama("rgcustomint2") & ", " & drutama("rgcustomint3") & ", '" & FixDouble(drutama("rgcustomdbl1")) & "', '" & FixDouble(drutama("rgcustomdbl2")) & "', '" & FixDouble(drutama("rgcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rgid from M2_rg where rgnotransaksi='" & notransaksi & "' AND rginputuser= '" & userid & "' order by rgmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rg_Detail where idrg = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder, strRekbank As New StringBuilder, strRekgiro As New StringBuilder, strBank As New StringBuilder, strNoacbank As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrgdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("statusrgc") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        'filter query untuk update status giro menjadi cair
                        If drutama("rgstatus") = 2 Then
                            'filter
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                            strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                            'rekbank
                            strRekbank.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekbank")) & "' ")
                            'rekgiro
                            strRekgiro.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekgiro")) & "' ")
                            'bank
                            strBank.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("bank")) & "' ")
                            'noacbank
                            strNoacbank.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("noacbank")) & "' ")
                        End If
                    Next
                    sql = "Insert into M2_Rg_Detail(idrgdetail, idrg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statusrgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus, gltglcair, glrekbank, glbank, glnoacbank m2_giro_list
                    If drutama("rgstatus") = 2 Then '  glstatus    , gltglcair                             , glrekbank                                                                 , glbank                                                           , glnoacbank                                                                              filter
                        sql = "UPDATE m2_giro_list SET glstatus = 1, gltglcair = '" & drutama("rgtgl") & "', glrekbank = (CASE glnogiro " & strRekbank.ToString & " ELSE glrekbank END), glbank = (CASE glnogiro " & strBank.ToString & " ELSE glbank END), glnoacbank = (CASE glnogiro " & strNoacbank.ToString & " ELSE glnoacbank END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "RG", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rgstatus") = 2 Then
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
    Public Function M2_RgUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("rgkontakkode", "c1.kkode")
            Filter = Filter.Replace("rgkontaknama", "c1.knama")
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
            Dim sumber As String = "Rg", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rgtgl, Rgnotransaksi, Rgstatus FROM m2_Rg WHERE Rgid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rgstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_rg_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Rg_HistorySimpan("" & paramSplit(0) & "★M2_Rg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'PROSES GIRO ====================================================================
                Dim strGiro As New StringBuilder, strGiroBatal As New StringBuilder
                'ambil giro dari detail
                dtdetail = AsDataTableAmbilDariDBCon("SELECT nogiro FROM m2_rg_detail WHERE idrg = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    'buat filter query untuk update giro m2_giro_list
                    For Each dr1 As DataRow In dtdetail.Rows
                        strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                        strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")

                        strGiroBatal.Append(IIf(Len(strGiroBatal.ToString) = 0, "", " OR "))
                        strGiroBatal.Append("(nogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                    Next
                    'UPDATE STATUS GIRO MENJADI BLM CAIR STATUS SEBELUMNYA
                    'sql = "UPDATE m2_giro_list SET glstatus = glstatussebelumnya, gltglcair = '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "' WHERE (" & strGiro.ToString & ")"
                    sql = "UPDATE m2_giro_list gl LEFT JOIN (SELECT rgcd.nogiro, rgc.rgctgl as tgl FROM m2_rgc_detail rgcd JOIN m2_rgc rgc ON rgcd.idrgc = rgc.rgcid AND rgc.rgcstatus IN(2,3,4,7) WHERE (" & strGiroBatal.ToString & ")) as gc ON gl.glnogiro = gc.nogiro SET gl.glstatus = gl.glstatussebelumnya, gl.gltglcair = (CASE gl.glstatussebelumnya WHEN 0 THEN '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "' ELSE IFNULL(gc.tgl,'1900-01-01') END) WHERE (" & strGiro.ToString & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF PROSES GIRO =============================================================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'UPDATE STATUS UTAMA
            sql = "UPDATE M2_Rg SET Rgstatus = " & nilaiStatus & ", Rgmodifikasiuser='" & userid & "', Rgmodifikasitgl = NOW(), Rgposting = 0, Rgpostingtgl = '1971-01-01 00:00:00', Rgjmlrevisi = Rgjmlrevisi + 1 WHERE Rgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgSearch(PostWsSearch(paramSplit(0), "M2_RgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RgDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("rgkontakkode", "c1.kkode")
            Filter = Filter.Replace("rgkontaknama", "c1.knama")
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
            Dim sumber As String = "Rg", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rgid, Rgnotransaksi FROM m2_Rg WHERE Rgid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl"
            sql &= " FROM M2_rg"
            sql &= " WHERE rgid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rgcabang")
                lokasi = dtNomorNext.Rows(0)("rglokasi")
                sumber = dtNomorNext.Rows(0)("rgsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rgautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rgnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rgtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Rg_Detail WHERE idRg = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Rg WHERE Rgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgSearch(PostWsSearch(paramSplit(0), "M2_RgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RgGetdataById(ByVal param As String) As String

        'M2_RgGetdataById Utama --------------------------------------------------------
        'rgid, rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, 
        'rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, 
        'rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, 
        'rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgpostingtgl, 
        'rgcustomtext1, rgcustomtext2, rgcustomtext3, rgcustomtext4, rgcustomtext5, rgcustomint1, rgcustomint2, 
        'rgcustomint3, rgcustomdbl1, rgcustomdbl2, rgcustomdbl3, rgcustomdate1, rgcustomdate2, rgcustomdate3, 
        'rgcabangnama, rglokasinama, rgkontakkode, rgkontaknama, rgstatusnama, rgstatussebelumnyanama, rginputusernama, 
        'rgmodifikasiusernama

        'M2_RgGetdataById Detail -------------------------------------------------------
        'idrgdetail, idrg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statusrgc, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, 
        'kontaknama, banknama, rekbanknama, rekgironama, statusgironama

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

        Dim NmMemcached As String = "aplikasi1-M2_Rg~M2_Rg_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rgid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rgid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rg_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rgid"), 0), sptField,
                     FxDB(drutama("rgcabang"), ""), sptField,
                     FxDB(drutama("rglokasi"), ""), sptField,
                     FxDB(drutama("rgsumber"), ""), sptField,
                     FxDB(drutama("rgautonotransaksi"), 0), sptField,
                     FxDB(drutama("rgnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rgtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rgkodepa"), 0), sptField,
                     FxDB(drutama("rgkontak"), 0), sptField,
                     FxDB(drutama("rgkontakperson"), ""), sptField,
                     FxDB(drutama("rguraian"), ""), sptField,
                     FxDB(drutama("rgcatatan"), ""), sptField,
                     FxDB(drutama("rgmatauang"), ""), sptField,
                     FxDB(drutama("rgkurs"), 0), sptField,
                     FxDB(drutama("rgjumlah"), 0), sptField,
                     FxDB(drutama("rgjumlahvalas"), 0), sptField,
                     FxDB(drutama("rgstatusrgc"), 0), sptField,
                     FxDB(drutama("rgstatus"), 0), sptField,
                     FxDB(drutama("rgstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rgjmlrevisi"), 0), sptField,
                     FxDB(drutama("rgcetakanke"), 0), sptField,
                     FxDB(drutama("rgisclose"), 0), sptField,
                     FxDB(drutama("rginputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgcustomtext1"), ""), sptField,
                     FxDB(drutama("rgcustomtext2"), ""), sptField,
                     FxDB(drutama("rgcustomtext3"), ""), sptField,
                     FxDB(drutama("rgcustomtext4"), ""), sptField,
                     FxDB(drutama("rgcustomtext5"), ""), sptField,
                     FxDB(drutama("rgcustomint1"), 0), sptField,
                     FxDB(drutama("rgcustomint2"), 0), sptField,
                     FxDB(drutama("rgcustomint3"), 0), sptField,
                     FxDB(drutama("rgcustomdbl1"), 0), sptField,
                     FxDB(drutama("rgcustomdbl2"), 0), sptField,
                     FxDB(drutama("rgcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rgcabangnama"), ""), sptField,
                     FxDB(drutama("rglokasinama"), ""), sptField,
                     FxDB(drutama("rgkontakkode"), ""), sptField,
                     FxDB(drutama("rgkontaknama"), ""), sptField,
                     FxDB(drutama("rgstatusnama"), ""), sptField,
                     FxDB(drutama("rgstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rginputusernama"), ""), sptField,
                     FxDB(drutama("rgmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrgdetail"), 0), sptField,
                     FxDB(dr("idrg"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("statusgiro"), 0), sptField,
                     FxDB(dr("statusrgc"), 0), sptField,
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
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptField,
                     FxDB(dr("statusgironama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgid, rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgpostingtgl, rgcustomtext1, rgcustomtext2, rgcustomtext3, rgcustomtext4, rgcustomtext5, rgcustomint1, rgcustomint2, rgcustomint3, rgcustomdbl1, rgcustomdbl2, rgcustomdbl3, rgcustomdate1, rgcustomdate2, rgcustomdate3, rgcabangnama, rglokasinama, rgkontakkode, rgkontaknama, rgstatusnama, rgstatussebelumnyanama, rginputusernama, rgmodifikasiusernama" & sptSubParam & "idrgdetail, idrg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statusrgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RgSearch(ByVal param As String) As String
        'M2_RgSearch --------------------------------------------------------
        'rgid, rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, 
        'rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, 
        'rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, 
        'rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgpostingtgl, 
        'rgcabangnama, rglokasinama, rgkontakkode, rgkontaknama, rgstatusnama, rgstatussebelumnyanama, rginputusernama, 
        'rgmodifikasiusernama

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
            Filter = Filter.Replace("rgkontakkode", "c1.kkode")
            Filter = Filter.Replace("rgkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rg_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Rg", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rgid"), 0), sptField,
                     FxDB(dr("rgcabang"), ""), sptField,
                     FxDB(dr("rglokasi"), ""), sptField,
                     FxDB(dr("rgsumber"), ""), sptField,
                     FxDB(dr("rgautonotransaksi"), 0), sptField,
                     FxDB(dr("rgnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rgtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rgkodepa"), 0), sptField,
                     FxDB(dr("rgkontak"), 0), sptField,
                     FxDB(dr("rgkontakperson"), ""), sptField,
                     FxDB(dr("rguraian"), ""), sptField,
                     FxDB(dr("rgcatatan"), ""), sptField,
                     FxDB(dr("rgmatauang"), ""), sptField,
                     FxDB(dr("rgkurs"), 0), sptField,
                     FxDB(dr("rgjumlah"), 0), sptField,
                     FxDB(dr("rgjumlahvalas"), 0), sptField,
                     FxDB(dr("rgstatusrgc"), 0), sptField,
                     FxDB(dr("rgstatus"), 0), sptField,
                     FxDB(dr("rgstatussebelumnya"), 0), sptField,
                     FxDB(dr("rgjmlrevisi"), 0), sptField,
                     FxDB(dr("rgcetakanke"), 0), sptField,
                     FxDB(dr("rgisclose"), 0), sptField,
                     FxDB(dr("rginputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgcabangnama"), ""), sptField,
                     FxDB(dr("rglokasinama"), ""), sptField,
                     FxDB(dr("rgkontakkode"), ""), sptField,
                     FxDB(dr("rgkontaknama"), ""), sptField,
                     FxDB(dr("rgstatusnama"), ""), sptField,
                     FxDB(dr("rgstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rginputusernama"), ""), sptField,
                     FxDB(dr("rgmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgid, rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgpostingtgl, rgcabangnama, rglokasinama, rgkontakkode, rgkontaknama, rgstatusnama, rgstatussebelumnyanama, rginputusernama, rgmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_Rg_DetailSearch(ByVal param As String) As String
        'M2_Rg_DetailSearch --------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, glkontakkode, glkontaknama, glrekbank, glrekbanknama, 
        'glrekgiro, glrekgironama, gljenis, gljenisnama, glbank, glbanknama, glnoacbank, 
        'glurutan, glstatus, glstatusnama, gljumlah, gljumlahvalas, glmatauang, gltgljthtempo, 
        'gltglcair, idrgdetail

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
            Filter = " where " & pagingSplit(2)
            Filter = Filter.Replace("glkontakkode", "k.kkode")
            Filter = Filter.Replace("glkontaknama", "k.knama")
            Filter = Filter.Replace("glrekbanknama", "coab.cnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rg_detail_v")
        sql = sql.Replace("valfilter", Filter)

        dt = AmbilData("aplikasi1-m2_giro_list_app", "", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glrekbank"), ""), sptField,
                     FxDB(dr("glrekbanknama"), ""), sptField,
                     FxDB(dr("glrekgiro"), ""), sptField,
                     FxDB(dr("glrekgironama"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("gljenisnama"), ""), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glurutan"), 0), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glstatusnama"), ""), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("gltglcair"), ""), formatTgl), sptField,
                     FxDB(dr("idrgdetail"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Giro data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RgTerkait(ByVal param As String) As String
        'M2_RgTerkait --------------------------------------------------------
        'rgid, rgnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rgid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rg_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rgid"), 0), sptField,
                     FxDB(dr("rgnotransaksi"), ""), sptField,
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
            result(2) = "Related RG data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgid, rgnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Public Function ValidasiSimpan(ByVal filterExist As String, ByVal filter As String, ByVal tgl As String, ByVal formatTgl As String) As String
        Dim hasil As String = "", sql As String = ""
        Dim dtvalidasi As New DataTable

        'VALIDASI EXIST GIRO =============================================
        If Len(filterExist) > 0 Then
            dtvalidasi = AsDataTableAmbilDariDB(filterExist) 'rowExists, glnogiro
            dtvalidasi = AsDataTableFilterLimit(dtvalidasi, "rowExists = 0", , , 1)
            If (dtvalidasi.Rows.Count > 0) Then
                hasil = "Giro : " & dtvalidasi.Rows(0)("glnogiro") & " - doesn't exist in Giro List." : GoTo selesai
            End If
        End If
        'END OF VALIDASI EXIST GIRO ======================================


        'VALIDASI STATUS GIRO ============================================
        If Len(filter) > 0 Then
            'filter giro dikurangi 2 karakter terakhir untuk menghilangkan 'or' terakhir
            'filter = filter.Substring(0, filter.Length - 2)

            'CEK STATUS GIRO SUDAH CAIR
            sql = "SELECT glnogiro, rgnotransaksi FROM m2_giro_list JOIN m2_rg_detail ON glnogiro=nogiro JOIN m2_rg ON idrg=rgid WHERE (glstatus = 1) AND (rgstatus=2 OR rgstatus=3 OR rgstatus=4 OR rgstatus=7) AND (" & filter & ") LIMIT 1"
            dtvalidasi = AsDataTableAmbilDariDB(sql)
            If (dtvalidasi.Rows.Count > 0) Then
                hasil = "Giro : " & dtvalidasi.Rows(0)(0) & " - has disbursed in transaction : " & dtvalidasi.Rows(0)(1) : GoTo selesai
            End If

            'CEK TGL PENCAIRAN GIRO < TGL TOLAKAN GIRO
            sql = "SELECT glnogiro, rgcnotransaksi, rgctgl FROM m2_giro_list JOIN m2_rgc_detail ON glnogiro = nogiro JOIN m2_rgc ON idrgc = rgcid WHERE (glstatus = 2 OR glstatus = 3) AND (rgcstatus = 2 OR rgcstatus = 3 OR rgcstatus = 4 OR rgcstatus = 7) AND rgctgl > '" & FixQuotes(AsFormatTanggal(tgl)) & "' AND (" & filter & ") LIMIT 1"
            dtvalidasi = AsDataTableAmbilDariDB(sql)
            If (dtvalidasi.Rows.Count > 0) Then
                hasil = "Giro : " & dtvalidasi.Rows(0)(0) & " - has rejected/canceled in transaction : " & dtvalidasi.Rows(0)(1) & ", the date must be more than or equal to " & AsFormatTanggal(dtvalidasi.Rows(0)(2), formatTgl) : GoTo selesai
            End If
        End If
        'END OF VALIDASI STATUS GIRO =====================================

selesai:
        Return hasil
    End Function

    <WebMethod()>
    Public Function M2_RgSimpanOld(ByVal param As String) As String
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


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        ''CEK PAGENUMBER
        'If (IsNumeric(pagingSplit(0)) = False) Then
        '    result(2) = "pageNumber required numeric." : GoTo selesai
        'End If

        ''CEK ITEMLIMIT
        'If (IsNumeric(pagingSplit(1)) = False) Then
        '    result(2) = "itemLimit required numeric." : GoTo selesai
        'End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        ''CEK FORMATTGLWAKTU
        'If Len(pagingSplit(5)) = 0 Then
        '    formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        'Else
        '    formatTglWaktu = pagingSplit(5)
        'End If
        'END OF VALIDASI PARAMETER PAGING ==================================================


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
        'rgid(0) As Integer, rgcabang(1) As String, rglokasi(2) As String, rgsumber(3) As String, rgautonotransaksi(4) As Integer, 
        'rgnotransaksi(5) As String, rgtgl(6) As Date, rgkodepa(7) As Integer, rgkontak(8) As Integer, rgkontakperson(9) As String, 
        'rguraian(10) As String, rgcatatan(11) As String, rgmatauang(12) As String, rgkurs(13) As Double, rgjumlah(14) As Double, 
        'rgjumlahvalas(15) As Double, rgstatusrgc(16) As Integer, rgstatus(17) As Integer, rgstatussebelumnya(18) As Integer, rgjmlrevisi(19) As Integer, 
        'rgcetakanke(20) As Integer, rgisclose(21) As Integer, rginputuser(22) As Integer, rginputtgl(23) As DateTime, rgmodifikasiuser(24) As Integer, 
        'rgmodifikasitgl(25) As DateTime, rgposting(26) As Integer, rgcustomtext1(27) As String, rgcustomtext2(28) As String, rgcustomtext3(29) As String, 
        'rgcustomtext4(30) As String, rgcustomtext5(31) As String, rgcustomint1(32) As Integer, rgcustomint2(33) As Integer, rgcustomint3(34) As Integer, 
        'rgcustomdbl1(35) As Double, rgcustomdbl2(36) As Double, rgcustomdbl3(37) As Double, rgcustomdate1(38) As Date, rgcustomdate2(39) As Date, 
        'rgcustomdate3(40) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rgid, rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, 
        'rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, 
        'rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, 
        'rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgcustomtext1, 
        'rgcustomtext2, rgcustomtext3, rgcustomtext4, rgcustomtext5, rgcustomint1, rgcustomint2, rgcustomint3, 
        'rgcustomdbl1, rgcustomdbl2, rgcustomdbl3, rgcustomdate1, rgcustomdate2, rgcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 41) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rgid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rgid required numeric." : GoTo selesai
        End If
        'rgautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rgautonotransaksi required numeric." : GoTo selesai
        End If
        'rgtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rgtgl required date." : GoTo selesai
        End If
        'rgkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rgkodepa required numeric." : GoTo selesai
        End If
        'rgkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rgkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "rgkontak can't be empty." : GoTo selesai
        End If
        'rgkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rgkurs required numeric." : GoTo selesai
        End If
        'rgjumlah(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rgjumlah required numeric." : GoTo selesai
        End If
        'rgjumlahvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rgjumlahvalas required numeric." : GoTo selesai
        End If
        'rgstatusrgc(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rgstatusrgc required numeric." : GoTo selesai
        End If
        'rgstatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rgstatus required numeric." : GoTo selesai
        End If
        'rgstatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rgstatussebelumnya required numeric." : GoTo selesai
        End If
        'rgjmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rgjmlrevisi required numeric." : GoTo selesai
        End If
        'rgcetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rgcetakanke required numeric." : GoTo selesai
        End If
        'rgisclose(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rgisclose required numeric." : GoTo selesai
        End If
        'rginputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rginputuser required numeric." : GoTo selesai
        End If
        'rginputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "rginputtgl required date." : GoTo selesai
        End If
        'rgmodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rgmodifikasiuser required numeric." : GoTo selesai
        End If
        'rgmodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rgmodifikasitgl required date." : GoTo selesai
        End If
        'rgposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "rgposting required numeric." : GoTo selesai
        End If
        'rgcustomint1(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rgcustomint1 required numeric." : GoTo selesai
        End If
        'rgcustomint2(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rgcustomint2 required numeric." : GoTo selesai
        End If
        'rgcustomint3(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rgcustomint3 required numeric." : GoTo selesai
        End If
        'rgcustomdbl1(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rgcustomdbl1 required numeric." : GoTo selesai
        End If
        'rgcustomdbl2(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rgcustomdbl2 required numeric." : GoTo selesai
        End If
        'rgcustomdbl3(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rgcustomdbl3 required numeric." : GoTo selesai
        End If
        'rgcustomdate1(38) As Date
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "rgcustomdate1 required date." : GoTo selesai
        End If
        'rgcustomdate2(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rgcustomdate2 required date." : GoTo selesai
        End If
        'rgcustomdate3(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rgcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rgcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rgcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rgcabang should not be more than 25 character." : GoTo selesai
        End If

        'rglokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rglokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rglokasi should not be more than 25 character." : GoTo selesai
        End If

        'rgsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rgsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rgsumber should not be more than 10 character." : GoTo selesai
        End If

        'rgnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rgnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rgnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rgtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rgtgl can't be empty" : GoTo selesai
        End If

        'rgmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "rgmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "rgmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rgkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rgkurs can't be empty" : GoTo selesai
        End If

        'rgjumlah(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rgjumlah can't be empty" : GoTo selesai
        End If

        'rgjumlahvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rgjumlahvalas can't be empty" : GoTo selesai
        End If

        'rginputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "rginputtgl can't be empty" : GoTo selesai
        End If

        'rgmodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rgmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rgcustomdbl1(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rgcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rgcustomdbl2(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rgcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rgcustomdbl3(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rgcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rgcustomdate1(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rgcustomdate1 can't be empty" : GoTo selesai
        End If

        'rgcustomdate2(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rgcustomdate2 can't be empty" : GoTo selesai
        End If

        'rgcustomdate3(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rgcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rgid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rglokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rguraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgstatusrgc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rginputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rginputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rgcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rgcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rgid~rgcabang~rglokasi~rgsumber~rgautonotransaksi~rgnotransaksi~rgtgl~rgkodepa~rgkontak~rgkontakperson~rguraian~rgcatatan~rgmatauang~rgkurs~rgjumlah~rgjumlahvalas~rgstatusrgc~rgstatus~rgstatussebelumnya~rgjmlrevisi~rgcetakanke~rgisclose~rginputuser~rginputtgl~rgmodifikasiuser~rgmodifikasitgl~rgposting~rgcustomtext1~rgcustomtext2~rgcustomtext3~rgcustomtext4~rgcustomtext5~rgcustomint1~rgcustomint2~rgcustomint3~rgcustomdbl1~rgcustomdbl2~rgcustomdbl3~rgcustomdate1~rgcustomdate2~rgcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrgdetail(0) As Integer, idrg(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, statusrgc(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrgdetail, idrg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statusrgc, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrgdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrg", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusgiro", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "statusrgc", AsEnumTypeData.AsInt64)
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


        'Variabel Validasi
        Dim ftExistGiro As String = "", ftGiro As String = "", vNogiro As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 27) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrgdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrgdetail required numeric." : GoTo selesai
            End If
            'idrg(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrg required numeric." : GoTo selesai
            End If
            'kontak(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - kontak required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljatuhtempo(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - tgljatuhtempo required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'statusgiro(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - statusgiro required numeric." : GoTo selesai
            End If
            'statusrgc(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - statusrgc required numeric." : GoTo selesai
            End If
            'isclose(17) As Integer
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(25) As Date
            If (IsDate(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'nogiro(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
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

            'jumlah(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If

            'jumlahvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljatuhtempo(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - tgljatuhtempo can't be empty" : GoTo selesai
            End If

            'customdbl1(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(25) As Date
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrgdetail~idrg~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~statusrgc~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If


            'BUAT FILTER VALIDASI STATUS GIRO ---------------------------
            'nogiro(2) As String
            vNogiro = dataRowDetail(2)

            'CEK DATA EXIST
            ftExistGiro = IIf(Len(ftExistGiro.ToString) = 0, "", ftExistGiro & " UNION ")
            ftExistGiro = String.Concat(ftExistGiro, "SELECT EXISTS(SELECT 1 FROM m2_giro_list WHERE glnogiro = '" & vNogiro & "' LIMIT 1) as rowExists, '" & vNogiro & "' as glnogiro")

            'Validasi Status Giro
            ftGiro = IIf(Len(ftGiro.ToString) = 0, "", ftGiro & " OR ")
            ftGiro = String.Concat(ftGiro, "(glnogiro = '" & vNogiro & "')")
            'END OF BUAT FILTER VALIDASI STATUS GIRO --------------------

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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rgtgl")), AsFormatTanggal(drutama("rgtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rgstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro, drutama("rgtgl"), formatTgl)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rgjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rgjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("rgid")
                    notransaksi = drutama("rgnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rgid), rgnotransaksi FROM M2_rg WHERE rgid='" & result(4) & "' AND rgstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rgid) FROM m2_rg WHERE rgnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_rg_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Rg_HistorySimpan("" & paramSplit(0) & "★M2_Rg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rgsumber")) & "▼" & FixQuotes(drutama("rgid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Rg set rgcabang  = '" & FixQuotes(drutama("rgcabang")) & "', rglokasi  = '" & FixQuotes(drutama("rglokasi")) & "', rgsumber  = '" & FixQuotes(drutama("rgsumber")) & "', rgautonotransaksi  = " & drutama("rgautonotransaksi") & ", rgnotransaksi  = '" & notransaksi & "', rgtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rgtgl"))) & "', rgkodepa  = " & drutama("rgkodepa") & ", rgkontak  = " & drutama("rgkontak") & ", rgkontakperson  = '" & FixQuotes(drutama("rgkontakperson")) & "', rguraian  = '" & FixQuotes(drutama("rguraian")) & "', rgcatatan  = '" & FixQuotes(drutama("rgcatatan")) & "', rgmatauang  = '" & FixQuotes(drutama("rgmatauang")) & "', rgkurs  = '" & FixDouble(drutama("rgkurs")) & "', rgjumlah  = '" & FixDouble(drutama("rgjumlah")) & "', rgjumlahvalas  = '" & FixDouble(drutama("rgjumlahvalas")) & "', rgstatusrgc  = " & drutama("rgstatusrgc") & ", rgstatus  = " & drutama("rgstatus") & ", rgstatussebelumnya  = " & drutama("rgstatussebelumnya") & ", rgjmlrevisi  = rgjmlrevisi+1, rgcetakanke  = " & drutama("rgcetakanke") & ", rgisclose  = " & drutama("rgisclose") & ", rgmodifikasiuser  = " & drutama("rgmodifikasiuser") & ", rgmodifikasitgl  = NOW(), rgposting  = 0, rgcustomtext1  = '" & FixQuotes(drutama("rgcustomtext1")) & "', rgcustomtext2  = '" & FixQuotes(drutama("rgcustomtext2")) & "', rgcustomtext3  = '" & FixQuotes(drutama("rgcustomtext3")) & "', rgcustomtext4  = '" & FixQuotes(drutama("rgcustomtext4")) & "', rgcustomtext5  = '" & FixQuotes(drutama("rgcustomtext5")) & "', rgcustomint1  = " & drutama("rgcustomint1") & ", rgcustomint2  = " & drutama("rgcustomint2") & ", rgcustomint3  = " & drutama("rgcustomint3") & ", rgcustomdbl1  = '" & FixDouble(drutama("rgcustomdbl1")) & "', rgcustomdbl2  = '" & FixDouble(drutama("rgcustomdbl2")) & "', rgcustomdbl3  = '" & FixDouble(drutama("rgcustomdbl3")) & "', rgcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate1"))) & "', rgcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate2"))) & "', rgcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate3"))) & "' where rgid = '" & drutama("rgid") & "'"
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

                    If drutama("rgautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rgcabang"), drutama("rglokasi"), drutama("rgsumber"), drutama("rgtgl"))
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
                        notransaksi = drutama("rgnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rgid) FROM m2_rg WHERE rgnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Rg (rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl, rgkodepa, rgkontak, rgkontakperson, rguraian, rgcatatan, rgmatauang, rgkurs, rgjumlah, rgjumlahvalas, rgstatusrgc, rgstatus, rgstatussebelumnya, rgjmlrevisi, rgcetakanke, rgisclose, rginputuser, rginputtgl, rgmodifikasiuser, rgmodifikasitgl, rgposting, rgcustomtext1, rgcustomtext2, rgcustomtext3, rgcustomtext4, rgcustomtext5, rgcustomint1, rgcustomint2, rgcustomint3, rgcustomdbl1, rgcustomdbl2, rgcustomdbl3, rgcustomdate1, rgcustomdate2, rgcustomdate3) values('" & FixQuotes(drutama("rgcabang")) & "', '" & FixQuotes(drutama("rglokasi")) & "', '" & FixQuotes(drutama("rgsumber")) & "', " & drutama("rgautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rgtgl"))) & "', " & drutama("rgkodepa") & ", " & drutama("rgkontak") & ", '" & FixQuotes(drutama("rgkontakperson")) & "', '" & FixQuotes(drutama("rguraian")) & "', '" & FixQuotes(drutama("rgcatatan")) & "', '" & FixQuotes(drutama("rgmatauang")) & "', '" & FixDouble(drutama("rgkurs")) & "', '" & FixDouble(drutama("rgjumlah")) & "', '" & FixDouble(drutama("rgjumlahvalas")) & "', " & drutama("rgstatusrgc") & ", " & drutama("rgstatus") & ", " & drutama("rgstatussebelumnya") & ", " & drutama("rgjmlrevisi") & ", " & drutama("rgcetakanke") & ", " & drutama("rgisclose") & ", " & drutama("rginputuser") & ", NOW(), " & drutama("rgmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("rgcustomtext1")) & "', '" & FixQuotes(drutama("rgcustomtext2")) & "', '" & FixQuotes(drutama("rgcustomtext3")) & "', '" & FixQuotes(drutama("rgcustomtext4")) & "', '" & FixQuotes(drutama("rgcustomtext5")) & "', " & drutama("rgcustomint1") & ", " & drutama("rgcustomint2") & ", " & drutama("rgcustomint3") & ", '" & FixDouble(drutama("rgcustomdbl1")) & "', '" & FixDouble(drutama("rgcustomdbl2")) & "', '" & FixDouble(drutama("rgcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rgcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rgid from M2_rg where rgnotransaksi='" & notransaksi & "' AND rginputuser= '" & userid & "' order by rgmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Rg_Detail where idrg = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder, strRekbank As New StringBuilder, strRekgiro As New StringBuilder, strBank As New StringBuilder, strNoacbank As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrgdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("statusrgc") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        'filter query untuk update status giro menjadi cair
                        If drutama("rgstatus") = 2 Then
                            'filter
                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                            strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                            'rekbank
                            strRekbank.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekbank")) & "' ")
                            'rekgiro
                            strRekgiro.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("rekgiro")) & "' ")
                            'bank
                            strBank.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("bank")) & "' ")
                            'noacbank
                            strNoacbank.Append(" WHEN '" & FixQuotes(dr1("nogiro")) & "' THEN '" & FixQuotes(dr1("noacbank")) & "' ")
                        End If
                    Next
                    sql = "Insert into M2_Rg_Detail(idrgdetail, idrg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statusrgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus, gltglcair, glrekbank, glbank, glnoacbank m2_giro_list
                    If drutama("rgstatus") = 2 Then '  glstatus    , gltglcair                             , glrekbank                                                                 , glbank                                                           , glnoacbank                                                                              filter
                        sql = "UPDATE m2_giro_list SET glstatus = 1, gltglcair = '" & drutama("rgtgl") & "', glrekbank = (CASE glnogiro " & strRekbank.ToString & " ELSE glrekbank END), glbank = (CASE glnogiro " & strBank.ToString & " ELSE glbank END), glnoacbank = (CASE glnogiro " & strNoacbank.ToString & " ELSE glnoacbank END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "RG", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rgstatus") = 2 Then
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
    Public Function M2_RgUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rgkontakkode", "c1.kkode")
            Filter = Filter.Replace("rgkontaknama", "c1.knama")
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
            Dim sumber As String = "Rg", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rgtgl, Rgnotransaksi, Rgstatus FROM m2_Rg WHERE Rgid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rgstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_rg_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Rg_HistorySimpan("" & paramSplit(0) & "★M2_Rg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'PROSES GIRO ====================================================================
                Dim strGiro As New StringBuilder, strGiroBatal As New StringBuilder
                'ambil giro dari detail
                dtdetail = AsDataTableAmbilDariDB("SELECT nogiro FROM m2_rg_detail WHERE idrg = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    'buat filter query untuk update giro m2_giro_list
                    For Each dr1 As DataRow In dtdetail.Rows
                        strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", " OR "))
                        strGiro.Append("(glnogiro = '" & FixQuotes(dr1("nogiro")) & "')")

                        strGiroBatal.Append(IIf(Len(strGiroBatal.ToString) = 0, "", " OR "))
                        strGiroBatal.Append("(nogiro = '" & FixQuotes(dr1("nogiro")) & "')")
                    Next
                    'UPDATE STATUS GIRO MENJADI BLM CAIR STATUS SEBELUMNYA
                    'sql = "UPDATE m2_giro_list SET glstatus = glstatussebelumnya, gltglcair = '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "' WHERE (" & strGiro.ToString & ")"
                    sql = "UPDATE m2_giro_list gl LEFT JOIN (SELECT rgcd.nogiro, rgc.rgctgl as tgl FROM m2_rgc_detail rgcd JOIN m2_rgc rgc ON rgcd.idrgc = rgc.rgcid AND rgc.rgcstatus IN(2,3,4,7) WHERE (" & strGiroBatal.ToString & ")) as gc ON gl.glnogiro = gc.nogiro SET gl.glstatus = gl.glstatussebelumnya, gl.gltglcair = (CASE gl.glstatussebelumnya WHEN 0 THEN '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "' ELSE IFNULL(gc.tgl,'1900-01-01') END) WHERE (" & strGiro.ToString & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If
                'END OF PROSES GIRO =============================================================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'UPDATE STATUS UTAMA
            sql = "UPDATE M2_Rg SET Rgstatus = " & nilaiStatus & ", Rgmodifikasiuser='" & userid & "', Rgmodifikasitgl = NOW(), Rgposting = 0, Rgpostingtgl = '1971-01-01 00:00:00', Rgjmlrevisi = Rgjmlrevisi + 1 WHERE Rgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgSearch(PostWsSearch(paramSplit(0), "M2_RgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_RgDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("rgkontakkode", "c1.kkode")
            Filter = Filter.Replace("rgkontaknama", "c1.knama")
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
            Dim sumber As String = "Rg", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rgid, Rgnotransaksi FROM m2_Rg WHERE Rgid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rgcabang, rglokasi, rgsumber, rgautonotransaksi, rgnotransaksi, rgtgl"
            sql &= " FROM M2_rg"
            sql &= " WHERE rgid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rgcabang")
                lokasi = dtNomorNext.Rows(0)("rglokasi")
                sumber = dtNomorNext.Rows(0)("rgsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rgautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rgnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rgtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Rg_Detail WHERE idRg = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Rg WHERE Rgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_RgSearch(PostWsSearch(paramSplit(0), "M2_RgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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