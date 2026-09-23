Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_bom
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_BomSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataDetail2(), dataRowDetail2() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, idbaranghasil As Double = 0

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'bomid(0) As Integer, bomcabang(1) As String, bomlokasi(2) As String, bomgudangasal(3) As String, bomgudangproduksi(4) As String, 
        'bomgudangtujuan(5) As String, bomsumber(6) As String, bomjenis(7) As String, bomautonotransaksi(8) As Integer, bomnotransaksi(9) As String, 
        'bomtgl(10) As Date, bomkodepa(11) As Integer, bompembuat(12) As Integer, bompembuatkontak(13) As String, bomestimasikerja(14) As String, 
        'bommatauang(15) As String, bomkurs(16) As Double, bomtotalhargain(17) As Double, bomtotalhargaout(18) As Double, bomtotalhppin(19) As Double, 
        'bomtotalhppout(20) As Double, bomuraian(21) As String, bomcatatan(22) As String, bomnoref(23) As String, bomtglnoref(24) As Date, 
        'bomstatus(25) As Integer, bomstatussebelumnya(26) As Integer, bomjmlrevisi(27) As Integer, bomcetakanke(28) As Integer, bominputuser(29) As Integer, 
        'bominputtgl(30) As DateTime, bommodifikasiuser(31) As Integer, bommodifikasitgl(32) As DateTime, bomcustomtext1(33) As String, bomcustomtext2(34) As String, 
        'bomcustomtext3(35) As String, bomcustomtext4(36) As String, bomcustomtext5(37) As String, bomcustomint1(38) As Integer, bomcustomint2(39) As Integer, 
        'bomcustomint3(40) As Integer, bomcustomdbl1(41) As Double, bomcustomdbl2(42) As Double, bomcustomdbl3(43) As Double, bomcustomdate1(44) As Date, 
        'bomcustomdate2(45) As Date, bomcustomdate3(46) As Date, bomaktivitas(47) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, 
        'bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, 
        'bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, 
        'bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, 
        'bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomcustomtext1, bomcustomtext2, 
        'bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, bomcustomint3, bomcustomdbl1, 
        'bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3, bomaktivitas

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 47 And dataUtama.Length <> 48) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'bomid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bomid required numeric." : GoTo selesai
        End If
        'bomautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "bomautonotransaksi required numeric." : GoTo selesai
        End If
        'bomtgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "bomtgl required date." : GoTo selesai
        End If
        'bomkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "bomkodepa required numeric." : GoTo selesai
        End If
        'bompembuat(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "bompembuat required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "bompembuat can't be empty." : GoTo selesai
        End If
        'bomkurs(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "bomkurs required numeric." : GoTo selesai
        End If
        'bomtotalhargain(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "bomtotalhargain required numeric." : GoTo selesai
        End If
        'bomtotalhargaout(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "bomtotalhargaout required numeric." : GoTo selesai
        End If
        'bomtotalhppin(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "bomtotalhppin required numeric." : GoTo selesai
        End If
        'bomtotalhppout(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "bomtotalhppout required numeric." : GoTo selesai
        End If
        'bomtglnoref(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "bomtglnoref required date." : GoTo selesai
        End If
        'bomstatus(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bomstatus required numeric." : GoTo selesai
        End If
        'bomstatussebelumnya(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "bomstatussebelumnya required numeric." : GoTo selesai
        End If
        'bomjmlrevisi(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bomjmlrevisi required numeric." : GoTo selesai
        End If
        'bomcetakanke(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "bomcetakanke required numeric." : GoTo selesai
        End If
        'bominputuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bominputuser required numeric." : GoTo selesai
        End If
        'bominputtgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "bominputtgl required date." : GoTo selesai
        End If
        'bommodifikasiuser(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bommodifikasiuser required numeric." : GoTo selesai
        End If
        'bommodifikasitgl(32) As DateTime
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "bommodifikasitgl required date." : GoTo selesai
        End If
        'bomcustomint1(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bomcustomint1 required numeric." : GoTo selesai
        End If
        'bomcustomint2(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bomcustomint2 required numeric." : GoTo selesai
        End If
        'bomcustomint3(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "bomcustomint3 required numeric." : GoTo selesai
        End If
        'bomcustomdbl1(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "bomcustomdbl1 required numeric." : GoTo selesai
        End If
        'bomcustomdbl2(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "bomcustomdbl2 required numeric." : GoTo selesai
        End If
        'bomcustomdbl3(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "bomcustomdbl3 required numeric." : GoTo selesai
        End If
        'bomcustomdate1(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "bomcustomdate1 required date." : GoTo selesai
        End If
        'bomcustomdate2(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "bomcustomdate2 required date." : GoTo selesai
        End If
        'bomcustomdate3(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "bomcustomdate3 required date." : GoTo selesai
        End If
        If dataUtama.Length > 47 Then
            'bomaktivitas(47) As Integer
            If (IsNumeric(dataUtama(47)) = False) Then
                result(2) = "bomaktivitas required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'bomcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bomcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bomcabang should not be more than 25 character." : GoTo selesai
        End If

        'bomlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bomlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bomlokasi should not be more than 25 character." : GoTo selesai
        End If

        'bomgudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "bomgudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "bomgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'bomgudangproduksi(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "bomgudangproduksi can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "bomgudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'bomgudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "bomgudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "bomgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'bomsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "bomsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "bomsumber should not be more than 10 character." : GoTo selesai
        End If

        'bomjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "bomjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "bomjenis should not be more than 25 character." : GoTo selesai
        End If

        'bomnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bomnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "bomnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bomtgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "bomtgl can't be empty" : GoTo selesai
        End If

        'bommatauang(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "bommatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "bommatauang should not be more than 25 character." : GoTo selesai
        End If

        'bomkurs(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "bomkurs can't be empty" : GoTo selesai
        End If

        'bomtotalhargain(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "bomtotalhargain can't be empty" : GoTo selesai
        End If

        'bomtotalhargaout(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "bomtotalhargaout can't be empty" : GoTo selesai
        End If

        'bomtotalhppin(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "bomtotalhppin can't be empty" : GoTo selesai
        End If

        'bomtotalhppout(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "bomtotalhppout can't be empty" : GoTo selesai
        End If

        'bomtglnoref(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "bomtglnoref can't be empty" : GoTo selesai
        End If

        'bominputtgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "bominputtgl can't be empty" : GoTo selesai
        End If

        'bommodifikasitgl(32) As DateTime
        If Len(dataUtama(32)) = 0 Then
            result(2) = "bommodifikasitgl can't be empty" : GoTo selesai
        End If

        'bomcustomdbl1(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "bomcustomdbl1 can't be empty" : GoTo selesai
        End If

        'bomcustomdbl2(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "bomcustomdbl2 can't be empty" : GoTo selesai
        End If

        'bomcustomdbl3(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "bomcustomdbl3 can't be empty" : GoTo selesai
        End If

        'bomcustomdate1(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "bomcustomdate1 can't be empty" : GoTo selesai
        End If

        'bomcustomdate2(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "bomcustomdate2 can't be empty" : GoTo selesai
        End If

        'bomcustomdate3(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "bomcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bomid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomgudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bompembuat", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bompembuatkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bommatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bominputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bominputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bommodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bommodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomaktivitas", AsEnumTypeData.AsInt64)
        If dataUtama.Length > 47 Then
            If AsDataTableTambahData(dtutama, "bomid~bomcabang~bomlokasi~bomgudangasal~bomgudangproduksi~bomgudangtujuan~bomsumber~bomjenis~bomautonotransaksi~bomnotransaksi~bomtgl~bomkodepa~bompembuat~bompembuatkontak~bomestimasikerja~bommatauang~bomkurs~bomtotalhargain~bomtotalhargaout~bomtotalhppin~bomtotalhppout~bomuraian~bomcatatan~bomnoref~bomtglnoref~bomstatus~bomstatussebelumnya~bomjmlrevisi~bomcetakanke~bominputuser~bominputtgl~bommodifikasiuser~bommodifikasitgl~bomcustomtext1~bomcustomtext2~bomcustomtext3~bomcustomtext4~bomcustomtext5~bomcustomint1~bomcustomint2~bomcustomint3~bomcustomdbl1~bomcustomdbl2~bomcustomdbl3~bomcustomdate1~bomcustomdate2~bomcustomdate3~bomaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "bomid~bomcabang~bomlokasi~bomgudangasal~bomgudangproduksi~bomgudangtujuan~bomsumber~bomjenis~bomautonotransaksi~bomnotransaksi~bomtgl~bomkodepa~bompembuat~bompembuatkontak~bomestimasikerja~bommatauang~bomkurs~bomtotalhargain~bomtotalhargaout~bomtotalhppin~bomtotalhppout~bomuraian~bomcatatan~bomnoref~bomtglnoref~bomstatus~bomstatussebelumnya~bomjmlrevisi~bomcetakanke~bominputuser~bominputtgl~bommodifikasiuser~bommodifikasitgl~bomcustomtext1~bomcustomtext2~bomcustomtext3~bomcustomtext4~bomcustomtext5~bomcustomint1~bomcustomint2~bomcustomint3~bomcustomdbl1~bomcustomdbl2~bomcustomdbl3~bomcustomdate1~bomcustomdate2~bomcustomdate3~bomaktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If


        'MAPPING BUAT WS DATA DETAIL1 -------------------------------------------------------
        'idbomin(0) As Integer, idbom(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, customtext1(27) As String, customtext2(28) As String, customtext3(29) As String, 
        'customdbl1(30) As Double, customdbl2(31) As Double, customdbl3(32) As Double, customdate1(33) As Date, customdate2(34) As Date, 
        'customdate3(35) As Date

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idbomin, idbom, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)

        'BARANG HASIL HARUS 1 ITEM SAJA
        If dataDetail.Length > 1 Then
            result(2) = "Detail 1 : Item result should not be more then one item." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable DETAIL1
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpppersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL1 ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL1 -----------------------------------
            'CEK ARRAY DATA DETAIL1
            If (dataRowDetail.Length <> 36) Then
                result(2) = "Detail 1 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL1 ----------------------------

            'VALIDASI TIPE DATA DETAIL1 ------------------------------------------
            'idbomin(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbomin required numeric." : GoTo selesai
            End If

            'idbom(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbom required numeric." : GoTo selesai
            End If

            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'SET IDBARANG HASIL
            idbaranghasil = Double.Parse(dataRowDetail(2))

            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpppersen(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'customdbl1(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(35) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL1 -----------------------------------

            'VALIDASI DATA DETAIL1 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Detail 1 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpppersen(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(15) As String
            'If Len(dataRowDetail(15)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(18) As String
            'If Len(dataRowDetail(18)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(18)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(19) As String
            'If Len(dataRowDetail(19)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(19)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(20) As String
            'If Len(dataRowDetail(20)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(35) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL1 --------------------------------

            If AsDataTableTambahData(dtdetail, "idbomin~idbom~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35)) = False Then
                result(2) = "Detail 1 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL1 ===========================================


        'MAPPING BUAT WS DATA DETAIL2 -------------------------------------------------------
        'idbomout(0) As Integer, idbom(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, customtext1(28) As String, customtext2(29) As String, 
        'customtext3(30) As String, customdbl1(31) As Double, customdbl2(32) As Double, customdbl3(33) As Double, customdate1(34) As Date, 
        'customdate2(35) As Date, customdate3(36) As Date

        'MAPPING BUAT FLEX DATA DETAIL2 -----------------------------------------------------
        'idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL2 ======================================================
        'SPLIT PARAMETER DATA DETAIL2
        dataDetail2 = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL2 ===============================================

        'Buat datatable DETAIL2
        Dim dtdetail2 As New DataTable
        AsDataTableTambahField(dtdetail2, "idbomout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jmlbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL2 ==================================================
        Dim JmlDtDetail2 As Integer = dataDetail2.Length
        For i = 1 To JmlDtDetail2
            'SPLIT DATA DETAIL
            dataRowDetail2 = dataDetail2(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL2 -----------------------------------
            'CEK ARRAY DATA DETAIL2
            If (dataRowDetail2.Length <> 37) Then
                result(2) = "Detail 2 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL2 ----------------------------

            'VALIDASI TIPE DATA DETAIL2 ------------------------------------------
            'idbomout(0) As Integer
            If (IsNumeric(dataRowDetail2(0)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbomout required numeric." : GoTo selesai
            End If
            'idbom(1) As Integer
            If (IsNumeric(dataRowDetail2(1)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbom required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail2(2)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail2(5)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail2(7)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail2(8) = Double.Parse(dataRowDetail2(5)) * Double.Parse(dataRowDetail2(7))
            If (IsNumeric(dataRowDetail2(8)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail2(11)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail2(12)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(13) As Double
            If (IsNumeric(dataRowDetail2(13)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(14) As Integer
            If (IsNumeric(dataRowDetail2(14)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(15) As Integer
            If (IsNumeric(dataRowDetail2(15)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail2(27)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'customdbl1(31) As Double
            If (IsNumeric(dataRowDetail2(31)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(32) As Double
            If (IsNumeric(dataRowDetail2(32)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(33) As Double
            If (IsNumeric(dataRowDetail2(33)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(34) As Date
            If (IsDate(dataRowDetail2(34)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(35) As Date
            If (IsDate(dataRowDetail2(35)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(36) As Date
            If (IsDate(dataRowDetail2(36)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL2 -----------------------------------

            'VALIDASI DATA DETAIL2 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail2(3)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail2(3)) > 100 Then
            '    result(2) = "Detail 2 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail2(5)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail2(5) <= 0 Then
                result(2) = "Detail 2 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail2(6)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(6)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail2(7)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail2(8)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            'If dataRowDetail2(8) <= 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            'End If

            'satuanbarang(9) As String
            If Len(dataRowDetail2(9)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(9)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail2(11)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail2(12)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(13) As Double
            If Len(dataRowDetail2(13)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(16) As String
            'If Len(dataRowDetail2(16)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(16)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(19) As String
            'If Len(dataRowDetail2(19)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(19)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(20) As String
            'If Len(dataRowDetail2(20)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(20)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(21) As String
            'If Len(dataRowDetail2(21)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(21)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(31) As Double
            If Len(dataRowDetail2(31)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(32) As Double
            If Len(dataRowDetail2(32)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(33) As Double
            If Len(dataRowDetail2(33)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(34) As Date
            If Len(dataRowDetail2(34)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(35) As Date
            If Len(dataRowDetail2(35)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(36) As Date
            If Len(dataRowDetail2(36)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL2 --------------------------------

            If AsDataTableTambahData(dtdetail2, "idbomout~idbom~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususmasuk~idhppfifomasuk~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail2(0) & "~" & dataRowDetail2(1) & "~" & dataRowDetail2(2) & "~" & dataRowDetail2(3) & "~" & dataRowDetail2(4) & "~" & dataRowDetail2(5) & "~" & dataRowDetail2(6) & "~" & dataRowDetail2(7) & "~" & dataRowDetail2(8) & "~" & dataRowDetail2(9) & "~" & dataRowDetail2(10) & "~" & dataRowDetail2(11) & "~" & dataRowDetail2(12) & "~" & dataRowDetail2(13) & "~" & dataRowDetail2(14) & "~" & dataRowDetail2(15) & "~" & dataRowDetail2(16) & "~" & dataRowDetail2(17) & "~" & dataRowDetail2(18) & "~" & dataRowDetail2(19) & "~" & dataRowDetail2(20) & "~" & dataRowDetail2(21) & "~" & dataRowDetail2(22) & "~" & dataRowDetail2(23) & "~" & dataRowDetail2(24) & "~" & dataRowDetail2(25) & "~" & dataRowDetail2(26) & "~" & dataRowDetail2(27) & "~" & dataRowDetail2(28) & "~" & dataRowDetail2(29) & "~" & dataRowDetail2(30) & "~" & dataRowDetail2(31) & "~" & dataRowDetail2(32) & "~" & dataRowDetail2(33) & "~" & dataRowDetail2(34) & "~" & dataRowDetail2(35) & "~" & dataRowDetail2(36)) = False Then
                result(2) = "Detail 2 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL2 ===========================================



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
                Dim vModuleId As Integer = 6, vMenuId As Integer = 3
                Select Case drutama("bomstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("bomtgl")), AsFormatTanggal(drutama("bomtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("bomid")
                    notransaksi = drutama("bomnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(bomid), bomnotransaksi FROM M6_bom WHERE bomid='" & result(4) & "' AND bomstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("bomautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bomcabang"), drutama("bomlokasi"), drutama("bomsumber"), drutama("bomtgl"), drutama("bomsumber"), 6)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bomid) FROM M6_bom WHERE bomnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_bom_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Bom_HistorySimpan("" & paramSplit(0) & "★M6_Bom_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bomsumber")) & "▼" & FixQuotes(drutama("bomid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Bom set bomcabang  = '" & FixQuotes(drutama("bomcabang")) & "', bomlokasi  = '" & FixQuotes(drutama("bomlokasi")) & "', bomgudangasal  = '" & FixQuotes(drutama("bomgudangasal")) & "', bomgudangproduksi  = '" & FixQuotes(drutama("bomgudangproduksi")) & "', bomgudangtujuan  = '" & FixQuotes(drutama("bomgudangtujuan")) & "', bomsumber  = '" & FixQuotes(drutama("bomsumber")) & "', bomjenis  = '" & FixQuotes(drutama("bomjenis")) & "', bomautonotransaksi  = " & drutama("bomautonotransaksi") & ", bomnotransaksi  = '" & FixQuotes(notransaksi) & "', bomtgl  = '" & FixQuotes(AsFormatTanggal(drutama("bomtgl"))) & "', bomkodepa  = " & drutama("bomkodepa") & ", bompembuat  = " & drutama("bompembuat") & ", bompembuatkontak  = '" & FixQuotes(drutama("bompembuatkontak")) & "', bomestimasikerja  = '" & FixQuotes(drutama("bomestimasikerja")) & "', bommatauang  = '" & FixQuotes(drutama("bommatauang")) & "', bomkurs  = '" & FixDouble(drutama("bomkurs")) & "', bomtotalhargain  = '" & FixDouble(drutama("bomtotalhargain")) & "', bomtotalhargaout  = '" & FixDouble(drutama("bomtotalhargaout")) & "', bomtotalhppin  = '" & FixDouble(drutama("bomtotalhppin")) & "', bomtotalhppout  = '" & FixDouble(drutama("bomtotalhppout")) & "', bomuraian  = '" & FixQuotes(drutama("bomuraian")) & "', bomcatatan  = '" & FixQuotes(drutama("bomcatatan")) & "', bomnoref  = '" & FixQuotes(drutama("bomnoref")) & "', bomtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("bomtglnoref"))) & "', bomstatus  = " & drutama("bomstatus") & ", bomstatussebelumnya  = " & drutama("bomstatussebelumnya") & ", bomjmlrevisi  = bomjmlrevisi+1, bomcetakanke  = " & drutama("bomcetakanke") & ", bommodifikasiuser  = " & drutama("bommodifikasiuser") & ", bommodifikasitgl  = NOW(), bomcustomtext1  = '" & FixQuotes(drutama("bomcustomtext1")) & "', bomcustomtext2  = '" & FixQuotes(drutama("bomcustomtext2")) & "', bomcustomtext3  = '" & FixQuotes(drutama("bomcustomtext3")) & "', bomcustomtext4  = '" & FixQuotes(drutama("bomcustomtext4")) & "', bomcustomtext5  = '" & FixQuotes(drutama("bomcustomtext5")) & "', bomcustomint1  = " & drutama("bomcustomint1") & ", bomcustomint2  = " & drutama("bomcustomint2") & ", bomcustomint3  = " & drutama("bomcustomint3") & ", bomcustomdbl1  = '" & FixDouble(drutama("bomcustomdbl1")) & "', bomcustomdbl2  = '" & FixDouble(drutama("bomcustomdbl2")) & "', bomcustomdbl3  = '" & FixDouble(drutama("bomcustomdbl3")) & "', bomcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate1"))) & "', bomcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate2"))) & "', bomcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate3"))) & "', bomaktivitas = '" & FixDouble(drutama("bomaktivitas")) & "' where bomid = '" & drutama("bomid") & "'"
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

                    If drutama("bomautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bomcabang"), drutama("bomlokasi"), drutama("bomsumber"), drutama("bomtgl"), drutama("bomsumber"), 6)
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
                        notransaksi = drutama("bomnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bomid) FROM m6_bom WHERE bomnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Bom (bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomcustomtext1, bomcustomtext2, bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, bomcustomint3, bomcustomdbl1, bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3, bomaktivitas) values('" & FixQuotes(drutama("bomcabang")) & "', '" & FixQuotes(drutama("bomlokasi")) & "', '" & FixQuotes(drutama("bomgudangasal")) & "', '" & FixQuotes(drutama("bomgudangproduksi")) & "', '" & FixQuotes(drutama("bomgudangtujuan")) & "', '" & FixQuotes(drutama("bomsumber")) & "', '" & FixQuotes(drutama("bomjenis")) & "', " & drutama("bomautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomtgl"))) & "', " & drutama("bomkodepa") & ", " & drutama("bompembuat") & ", '" & FixQuotes(drutama("bompembuatkontak")) & "', '" & FixQuotes(drutama("bomestimasikerja")) & "', '" & FixQuotes(drutama("bommatauang")) & "', '" & FixDouble(drutama("bomkurs")) & "', '" & FixDouble(drutama("bomtotalhargain")) & "', '" & FixDouble(drutama("bomtotalhargaout")) & "', '" & FixDouble(drutama("bomtotalhppin")) & "', '" & FixDouble(drutama("bomtotalhppout")) & "', '" & FixQuotes(drutama("bomuraian")) & "', '" & FixQuotes(drutama("bomcatatan")) & "', '" & FixQuotes(drutama("bomnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomtglnoref"))) & "', " & drutama("bomstatus") & ", " & drutama("bomstatussebelumnya") & ", " & drutama("bomjmlrevisi") & ", " & drutama("bomcetakanke") & ", " & drutama("bominputuser") & ", NOW(), " & drutama("bommodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(drutama("bomcustomtext1")) & "', '" & FixQuotes(drutama("bomcustomtext2")) & "', '" & FixQuotes(drutama("bomcustomtext3")) & "', '" & FixQuotes(drutama("bomcustomtext4")) & "', '" & FixQuotes(drutama("bomcustomtext5")) & "', " & drutama("bomcustomint1") & ", " & drutama("bomcustomint2") & ", " & drutama("bomcustomint3") & ", '" & FixDouble(drutama("bomcustomdbl1")) & "', '" & FixDouble(drutama("bomcustomdbl2")) & "', '" & FixDouble(drutama("bomcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate3"))) & "', '" & FixDouble(drutama("bomaktivitas")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select bomid from M6_bom where bomnotransaksi='" & notransaksi & "' AND bominputuser= '" & userid & "' order by bommodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Bom_In where idbom = '" & result(4) & "'"
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
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idbomin") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Bom_In(idbomin, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail In Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail2 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Bom_Out where idbom = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail2
                If (dtdetail2.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail2.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idbomout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Bom_Out(idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Out Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'PROSES POSTING KE M6_ITEMBOM --------------------------------
                If drutama("bomstatus") = 2 Then

                    'DELETE M6_ITEMBOM_IN
                    sql = "DELETE FROM m6_itembom_in WHERE idbarang = '" & FixDouble(idbaranghasil) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE M6_ITEMBOM_OUT
                    sql = "DELETE FROM m6_itembom_out WHERE idbaranghasil = '" & FixDouble(idbaranghasil) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT M6_ITEMBOM_IN
                    sql = "INSERT INTO m6_itembom_in (SELECT  idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_in bomin WHERE bomin.idbom = '" & result(4) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT M6_ITEMBOM_OUT
                    sql = "INSERT INTO m6_itembom_out (SELECT '" & FixDouble(idbaranghasil) & "', idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_out bomout WHERE bomout.idbom = '" & result(4) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                End If
                'END OF PROSES POSTING KE M6_ITEMBOM -------------------------


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
            result(2) = ex.Message & sql
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
    Public Function M6_BomUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "PF", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bomtgl, Bomnotransaksi, Bomstatus FROM M6_Bom WHERE Bomid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bomstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_bom_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Bom_HistorySimpan("" & paramSplit(0) & "★M6_Bom_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'If isDelete Then
            '    'CEK TERKAIT ====================================================================
            '    'PANGGIL QUERY TERKAIT
            '    Dim query As New m0_query
            '    sql = query.m6_bom_terkait("bomid = '" & idtransaksi & "'")
            '    Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            '    dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
            '    If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
            '    'END OF CEK TERKAIT =============================================================
            'End If

            'update status utama
            sql = "UPDATE M6_Bom SET Bomstatus = " & nilaiStatus & ", Bommodifikasiuser='" & userid & "', Bommodifikasitgl = NOW(), Bomposting = 0, Bompostingtgl = '1971-01-01 00:00:00', Bomjmlrevisi = Bomjmlrevisi + 1 WHERE Bomid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'UNPOSTING M6_ITEM BOM ==========================================================
            'AMBIL IDBARANG HASIL DARI M6_BOM_IN
            Dim dtBarangHasil As DataTable = AsDataTableAmbilDariDBCon("SELECT idbarang FROM m6_bom_in WHERE idbom = '" & FixDouble(idtransaksi) & "'", myConn)
            If dtBarangHasil.Rows.Count > 0 Then
                Dim idBarangHasil As Double = Double.Parse(dtBarangHasil.Rows(0)(0))

                'DELETE M6_ITEMBOM_IN
                sql = "DELETE FROM m6_itembom_in WHERE idbarang = '" & FixDouble(idBarangHasil) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE M6_ITEMBOM_OUT
                sql = "DELETE FROM m6_itembom_out WHERE idbaranghasil = '" & FixDouble(idBarangHasil) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'AMBIL BOM SEBELUMNYA YANG AKTIF
                Dim dtBom As DataTable = AsDataTableAmbilDariDBCon("SELECT bom.bomid FROM m6_bom_in bomin JOIN m6_bom bom ON bomin.idbom = bom.bomid WHERE bomin.idbarang = '" & idBarangHasil & "' AND bom.bomstatus IN(2,3,4,7) ORDER BY bominputtgl DESC LIMIT 1", myConn)
                If dtBom.Rows.Count > 0 Then
                    Dim idbom As Double = Double.Parse(dtBom.Rows(0)(0))

                    'INSERT M6_ITEMBOM_IN
                    sql = "INSERT INTO m6_itembom_in (SELECT  idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_in bomin WHERE bomin.idbom = '" & idbom & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT M6_ITEMBOM_OUT
                    sql = "INSERT INTO m6_itembom_out (SELECT '" & FixDouble(idBarangHasil) & "', idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_out bomout WHERE bomout.idbom = '" & idbom & "')"
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
            'END OF UNPOSTING M6_ITEM BOM ===================================================

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
            Dim paramSearch As String = M6_BomSearch(PostWsSearch(paramSplit(0), "M6_BomSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_BomDelete(ByVal param As String) As String

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
            Dim sumber As String = "PF", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Bomid, Bomnotransaksi FROM M6_Bom WHERE Bomid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bomcabang, bomlokasi, bomsumber, bomautonotransaksi, bomnotransaksi, bomtgl"
            sql &= " FROM M6_bom"
            sql &= " WHERE bomid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bomcabang")
                lokasi = dtNomorNext.Rows(0)("bomlokasi")
                sumber = dtNomorNext.Rows(0)("bomsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("bomautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("bomnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bomtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_Bom_In WHERE idBom ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL2
            sql = "DELETE FROM M6_Bom_Out WHERE idBom ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M6_Bom WHERE Bomid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_BomSearch(PostWsSearch(paramSplit(0), "M6_BomSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M6_BomGetdataById(ByVal param As String) As String
        'M6_BomGetdataById Utama --------------------------------------------------------
        'bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, 
        'bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, 
        'bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, 
        'bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, 
        'bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, 
        'bomcustomtext1, bomcustomtext2, bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, 
        'bomcustomint3, bomcustomdbl1, bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3, 
        'bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, 
        'bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama, 
        'bomaktivitas, bomaktivitaskode, bomaktivitasnama, bomjeniswajibwo

        'M6_BomGetdataById In --------------------------------------------------------
        'idbomin, idbom, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, 
        'bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi

        'M6_BomGetdataById Out --------------------------------------------------------
        'idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'costcenternama, divisinama, subdivisinama, proyeknama, notransaksi

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

        Dim utama As String = "", detail As String = "", detailout As String = "", idtransaksi As String = ""

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
            formatTglWaktu = "yyy-MM-dd H:mm:ss"
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

        Dim NmMemcached As String = "aplikasi1-M5_pl~M5_pl_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "bomid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "bomid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_bom_getdata")
        sql = "select bom.bomid AS bomid, bom.bomcabang AS bomcabang, bom.bomlokasi AS bomlokasi, bom.bomgudangasal AS bomgudangasal, bom.bomgudangproduksi AS bomgudangproduksi, bom.bomgudangtujuan AS bomgudangtujuan, bom.bomsumber AS bomsumber, bom.bomjenis AS bomjenis, bom.bomautonotransaksi AS bomautonotransaksi, bom.bomnotransaksi AS bomnotransaksi, bom.bomtgl AS bomtgl, bom.bomkodepa AS bomkodepa, bom.bompembuat AS bompembuat, bom.bompembuatkontak AS bompembuatkontak, bom.bomestimasikerja AS bomestimasikerja, bom.bommatauang AS bommatauang, bom.bomkurs AS bomkurs, bom.bomtotalhargain AS bomtotalhargain, bom.bomtotalhargaout AS bomtotalhargaout, bom.bomtotalhppin AS bomtotalhppin, bom.bomtotalhppout AS bomtotalhppout, bom.bomuraian AS bomuraian, bom.bomcatatan AS bomcatatan, bom.bomnoref AS bomnoref, bom.bomtglnoref AS bomtglnoref, bom.bomstatus AS bomstatus, bom.bomstatussebelumnya AS bomstatussebelumnya, bom.bomjmlrevisi AS bomjmlrevisi, bom.bomcetakanke AS bomcetakanke, bom.bominputuser AS bominputuser, bom.bominputtgl AS bominputtgl, bom.bommodifikasiuser AS bommodifikasiuser, bom.bommodifikasitgl AS bommodifikasitgl, bom.bomposting AS bomposting, bom.bompostingtgl AS bompostingtgl, bom.bomcustomtext1 AS bomcustomtext1, bom.bomcustomtext2 AS bomcustomtext2, bom.bomcustomtext3 AS bomcustomtext3, bom.bomcustomtext4 AS bomcustomtext4, bom.bomcustomtext5 AS bomcustomtext5, bom.bomcustomint1 AS bomcustomint1, bom.bomcustomint2 AS bomcustomint2, bom.bomcustomint3 AS bomcustomint3, bom.bomcustomdbl1 AS bomcustomdbl1, bom.bomcustomdbl2 AS bomcustomdbl2, bom.bomcustomdbl3 AS bomcustomdbl3, bom.bomcustomdate1 AS bomcustomdate1, bom.bomcustomdate2 AS bomcustomdate2, bom.bomcustomdate3 AS bomcustomdate3, br.bnama AS bomcabangnama, lc.lnama AS bomlokasinama, wh1.wnama AS bomgudangasalnama, wh2.wnama AS bomgudangproduksinama, wh3.wnama AS bomgudangtujuannama, pc.pcnama AS bomjenisnama, c1.kkode AS bompembuatkode, c1.knama AS bompembuatnama, we.wenama AS bomestimasikerjanama, st1.nama AS bomstatusnama, st2.nama AS bomstatussebelumnyanama, u1.unama AS bominputusernama, u2.unama AS bommodifikasiusernama, bom.bomaktivitas, pa.pakode as bomaktivitaskode, pa.panama as bomaktivitasnama, pc.pcwajibwo AS bomjeniswajibwo, bomi.idbomin AS idbomin, bomi.idbom AS idbom, bomi.idbarang AS idbarang, bomi.namabarang AS namabarang, bomi.tipebarang AS tipebarang, bomi.jml AS jml, bomi.satuan AS satuan, bomi.nilaisatuan AS nilaisatuan, bomi.jmlbarang AS jmlbarang, bomi.satuanbarang AS satuanbarang, bomi.matauang AS matauang, bomi.kurs AS kurs, bomi.harga AS harga, bomi.hpppersen AS hpppersen, bomi.hpp AS hpp, i.brekpersediaan AS rekpersediaan, bomi.cabang AS cabang, bomi.lokasi AS lokasi, bomi.gudangasal AS gudangasal, bomi.gudangproduksi AS gudangproduksi, bomi.gudangtujuan AS gudangtujuan, bomi.costcenter AS costcenter, bomi.divisi AS divisi, bomi.subdivisi AS subdivisi, bomi.proyek AS proyek, bomi.catatan AS catatan, bomi.urutan AS urutan, bomi.customtext1 AS customtext1, bomi.customtext2 AS customtext2, bomi.customtext3 AS customtext3, bomi.customdbl1 AS customdbl1, bomi.customdbl2 AS customdbl2, bomi.customdbl3 AS customdbl3, bomi.customdate1 AS customdate1, bomi.customdate2 AS customdate2, bomi.customdate3 AS customdate3, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, cc.ccnama AS costcenternama, d.dnama AS divisinama,sd.sdnama AS subdivisinama, p.pnama AS proyeknama, bom.bomnotransaksi AS notransaksi from m6_bom bom join m6_bom_in bomi on bom.bomid = bomi.idbom left join m1_branch br on bom.bomcabang = br.bkode left join m1_location lc on bom.bomlokasi = lc.lkode left join m1_warehouse wh1 on bom.bomgudangasal = wh1.wkode left join m1_warehouse wh2 on bom.bomgudangproduksi = wh2.wkode left join m1_warehouse wh3 on bom.bomgudangtujuan = wh3.wkode left join m1_production_category pc on bom.bomjenis = pc.pckode left join m1_contact c1 on bom.bompembuat = c1.kid left join m1_working_estimate we on bom.bomestimasikerja = we.wekode left join m0_status st1 on bom.bomstatus = st1.kode left join m0_status st2 on bom.bomstatussebelumnya = st2.kode left join m0_user u1 on bom.bominputuser = u1.userid left join m0_user u2 on bom.bommodifikasiuser = u2.userid left join m1_production_activity pa on bom.bomaktivitas = pa.paid left join m1_item i on bomi.idbarang = i.bid left join m1_cost_center cc on bomi.costcenter = cc.cckode left join m1_division d on bomi.divisi = d.dkode left join m1_subdivision sd on bomi.subdivisi = sd.sdkode left join m1_project p on bomi.proyek = p.pkode"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("bomid"), 0), sptField,
                     FxDB(drutama("bomcabang"), ""), sptField,
                     FxDB(drutama("bomlokasi"), ""), sptField,
                     FxDB(drutama("bomgudangasal"), ""), sptField,
                     FxDB(drutama("bomgudangproduksi"), ""), sptField,
                     FxDB(drutama("bomgudangtujuan"), ""), sptField,
                     FxDB(drutama("bomsumber"), ""), sptField,
                     FxDB(drutama("bomjenis"), ""), sptField,
                     FxDB(drutama("bomautonotransaksi"), 0), sptField,
                     FxDB(drutama("bomnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bomtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bomkodepa"), 0), sptField,
                     FxDB(drutama("bompembuat"), 0), sptField,
                     FxDB(drutama("bompembuatkontak"), ""), sptField,
                     FxDB(drutama("bomestimasikerja"), ""), sptField,
                     FxDB(drutama("bommatauang"), ""), sptField,
                     FxDB(drutama("bomkurs"), 0), sptField,
                     FxDB(drutama("bomtotalhargain"), 0), sptField,
                     FxDB(drutama("bomtotalhargaout"), 0), sptField,
                     FxDB(drutama("bomtotalhppin"), 0), sptField,
                     FxDB(drutama("bomtotalhppout"), 0), sptField,
                     FxDB(drutama("bomuraian"), ""), sptField,
                     FxDB(drutama("bomcatatan"), ""), sptField,
                     FxDB(drutama("bomnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bomtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("bomstatus"), 0), sptField,
                     FxDB(drutama("bomstatussebelumnya"), 0), sptField,
                     FxDB(drutama("bomjmlrevisi"), 0), sptField,
                     FxDB(drutama("bomcetakanke"), 0), sptField,
                     FxDB(drutama("bominputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bominputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bommodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bommodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bomposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bompostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bomcustomtext1"), ""), sptField,
                     FxDB(drutama("bomcustomtext2"), ""), sptField,
                     FxDB(drutama("bomcustomtext3"), ""), sptField,
                     FxDB(drutama("bomcustomtext4"), ""), sptField,
                     FxDB(drutama("bomcustomtext5"), ""), sptField,
                     FxDB(drutama("bomcustomint1"), 0), sptField,
                     FxDB(drutama("bomcustomint2"), 0), sptField,
                     FxDB(drutama("bomcustomint3"), 0), sptField,
                     FxDB(drutama("bomcustomdbl1"), 0), sptField,
                     FxDB(drutama("bomcustomdbl2"), 0), sptField,
                     FxDB(drutama("bomcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bomcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bomcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bomcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bomcabangnama"), ""), sptField,
                     FxDB(drutama("bomlokasinama"), ""), sptField,
                     FxDB(drutama("bomgudangasalnama"), ""), sptField,
                     FxDB(drutama("bomgudangproduksinama"), ""), sptField,
                     FxDB(drutama("bomgudangtujuannama"), ""), sptField,
                     FxDB(drutama("bomjenisnama"), ""), sptField,
                     FxDB(drutama("bompembuatkode"), ""), sptField,
                     FxDB(drutama("bompembuatnama"), ""), sptField,
                     FxDB(drutama("bomestimasikerjanama"), ""), sptField,
                     FxDB(drutama("bomstatusnama"), ""), sptField,
                     FxDB(drutama("bomstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("bominputusernama"), ""), sptField,
                     FxDB(drutama("bommodifikasiusernama"), ""), sptField,
                     FxDB(drutama("bomaktivitas"), 0), sptField,
                     FxDB(drutama("bomaktivitaskode"), ""), sptField,
                     FxDB(drutama("bomaktivitasnama"), ""), sptField,
                     FxDB(drutama("bomjeniswajibwo"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idbomin"), 0), sptField,
                     FxDB(dr("idbom"), 0), sptField,
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
                     FxDB(dr("hpppersen"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            Dim querygiro As New m0_query
            'sql = querygiro.PanggilQuery("m6_bom_getdata_out")
            sql = "select `bomo`.`idbomout` AS `idbomout`,`bomo`.`idbom` AS `idbom`,`bomo`.`idbarang` AS `idbarang`,`bomo`.`namabarang` AS `namabarang`,`bomo`.`tipebarang` AS `tipebarang`,`bomo`.`jml` AS `jml`,`bomo`.`satuan` AS `satuan`,`bomo`.`nilaisatuan` AS `nilaisatuan`,`bomo`.`jmlbarang` AS `jmlbarang`,`bomo`.`satuanbarang` AS `satuanbarang`,`bomo`.`matauang` AS `matauang`,`bomo`.`kurs` AS `kurs`,`bomo`.`harga` AS `harga`,`bomo`.`hpp` AS `hpp`,`bomo`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`bomo`.`idhppfifomasuk` AS `idhppfifomasuk`,`i`.`brekpersediaan` AS `rekpersediaan`,`bomo`.`cabang` AS `cabang`,`bomo`.`lokasi` AS `lokasi`,`bomo`.`gudangasal` AS `gudangasal`,`bomo`.`gudangproduksi` AS `gudangproduksi`,`bomo`.`gudangtujuan` AS `gudangtujuan`,`bomo`.`costcenter` AS `costcenter`,`bomo`.`divisi` AS `divisi`,`bomo`.`subdivisi` AS `subdivisi`,`bomo`.`proyek` AS `proyek`,`bomo`.`catatan` AS `catatan`,`bomo`.`urutan` AS `urutan`,`bomo`.`customtext1` AS `customtext1`,`bomo`.`customtext2` AS `customtext2`,`bomo`.`customtext3` AS `customtext3`,`bomo`.`customdbl1` AS `customdbl1`,`bomo`.`customdbl2` AS `customdbl2`,`bomo`.`customdbl3` AS `customdbl3`,`bomo`.`customdate1` AS `customdate1`,`bomo`.`customdate2` AS `customdate2`,`bomo`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`bom`.`bomnotransaksi` AS `notransaksi`, i.bstok AS bstok, IFNULL(SUM(ib.jmlbooking),0) AS booking, IFNULL((i.bstok-SUM(ib.jmlbooking)),0) AS stokakhir from (((((((`m6_bom_out` `bomo` join `m6_bom` `bom` on((`bomo`.`idbom` = `bom`.`bomid`))) left join `m1_item` `i` on((`bomo`.`idbarang` = `i`.`bid`))) left join `m1_cost_center` `cc` on((`bomo`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`bomo`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`bomo`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`bomo`.`proyek` = `p`.`pkode`))) left join `m1_item_booking` `ib` on((`ib`.`idbarang` = bomo.idbarang)))"

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Bom_Pack", "idbom='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "bomo.idbomout", sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailout = String.Concat(detailout,
                     FxDB(dr("idbomout"), 0), sptField,
                     FxDB(dr("idbom"), 0), sptField,
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
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("stokakhir"), 0), sptRow)
            Next
            detailout = detailout.Substring(0, detailout.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, detailout)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, bomcustomtext1, bomcustomtext2, bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, bomcustomint3, bomcustomdbl1, bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3, bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama, bomaktivitas, bomaktivitaskode, bomaktivitasnama, bomjeniswajibwo" & sptSubParam & "idbomin, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi" & sptSubParam & "idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, stokakhir"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_BomSearch(ByVal param As String) As String
        'M6_BomSearch --------------------------------------------------------
        'bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, 
        'bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, 
        'bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, 
        'bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, 
        'bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, 
        'bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, 
        'bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama, 
        'bomaktivitas, bomaktivitaskode, bomaktivitasnama

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_bom_v")
        'sql = "select bom.bomid AS bomid, bom.bomcabang AS bomcabang, bom.bomlokasi AS bomlokasi, bom.bomgudangasal AS bomgudangasal, bom.bomgudangproduksi AS bomgudangproduksi, bom.bomgudangtujuan AS bomgudangtujuan, bom.bomsumber AS bomsumber, bom.bomjenis AS bomjenis, bom.bomautonotransaksi AS bomautonotransaksi, bom.bomnotransaksi AS bomnotransaksi, bom.bomtgl AS bomtgl, bom.bomkodepa AS bomkodepa, bom.bompembuat AS bompembuat, bom.bompembuatkontak AS bompembuatkontak, bom.bomestimasikerja AS bomestimasikerja, bom.bommatauang AS bommatauang, bom.bomkurs AS bomkurs, bom.bomtotalhargain AS bomtotalhargain, bom.bomtotalhargaout AS bomtotalhargaout, bom.bomtotalhppin AS bomtotalhppin, bom.bomtotalhppout AS bomtotalhppout, bom.bomuraian AS bomuraian, bom.bomcatatan AS bomcatatan, bom.bomnoref AS bomnoref, bom.bomtglnoref AS bomtglnoref, bom.bomstatus AS bomstatus, bom.bomstatussebelumnya AS bomstatussebelumnya, bom.bomjmlrevisi AS bomjmlrevisi, bom.bomcetakanke AS bomcetakanke, bom.bominputuser AS bominputuser, bom.bominputtgl AS bominputtgl, bom.bommodifikasiuser AS bommodifikasiuser, bom.bommodifikasitgl AS bommodifikasitgl, bom.bomposting AS bomposting, bom.bompostingtgl AS bompostingtgl, br.bnama AS bomcabangnama, lc.lnama AS bomlokasinama, wh1.wnama AS bomgudangasalnama, wh2.wnama AS bomgudangproduksinama, wh3.wnama AS bomgudangtujuannama, pc.pcnama AS bomjenisnama, c1.kkode AS bompembuatkode, c1.knama AS bompembuatnama, we.wenama AS bomestimasikerjanama, st1.nama AS bomstatusnama, st2.nama AS bomstatussebelumnyanama, u1.unama AS bominputusernama, u2.unama AS bommodifikasiusernama, bom.bomaktivitas, pa.pakode as bomaktivitaskode, pa.panama as bomaktivitasnama from m6_bom bom left join m1_branch br on bom.bomcabang = br.bkode left join m1_location lc on bom.bomlokasi = lc.lkode left join m1_warehouse wh1 on bom.bomgudangasal = wh1.wkode left join m1_warehouse wh2 on bom.bomgudangproduksi = wh2.wkode left join m1_warehouse wh3 on bom.bomgudangtujuan = wh3.wkode left join m1_production_category pc on bom.bomjenis = pc.pckode left join m1_contact c1 on bom.bompembuat = c1.kid left join m1_working_estimate we on bom.bomestimasikerja = we.wekode left join m0_status st1 on bom.bomstatus = st1.kode left join m0_status st2 on bom.bomstatussebelumnya = st2.kode left join m0_user u1 on bom.bominputuser = u1.userid left join m0_user u2 on bom.bommodifikasiuser = u2.userid left join m1_production_activity pa on bom.bomaktivitas = pa.paid"
        sql = "select bom.bomid AS bomid, bom.bomcabang AS bomcabang, bom.bomlokasi AS bomlokasi, bom.bomgudangasal AS bomgudangasal, bom.bomgudangproduksi AS bomgudangproduksi, bom.bomgudangtujuan AS bomgudangtujuan, bom.bomsumber AS bomsumber, bom.bomjenis AS bomjenis, bom.bomautonotransaksi AS bomautonotransaksi, bom.bomnotransaksi AS bomnotransaksi, bom.bomtgl AS bomtgl, bom.bomkodepa AS bomkodepa, bom.bompembuat AS bompembuat, bom.bompembuatkontak AS bompembuatkontak, bom.bomestimasikerja AS bomestimasikerja, bom.bommatauang AS bommatauang, bom.bomkurs AS bomkurs, bom.bomtotalhargain AS bomtotalhargain, bom.bomtotalhargaout AS bomtotalhargaout, bom.bomtotalhppin AS bomtotalhppin, bom.bomtotalhppout AS bomtotalhppout, bom.bomuraian AS bomuraian, bom.bomcatatan AS bomcatatan, bom.bomnoref AS bomnoref, bom.bomtglnoref AS bomtglnoref, bom.bomstatus AS bomstatus, bom.bomstatussebelumnya AS bomstatussebelumnya, bom.bomjmlrevisi AS bomjmlrevisi, bom.bomcetakanke AS bomcetakanke, bom.bominputuser AS bominputuser, bom.bominputtgl AS bominputtgl, bom.bommodifikasiuser AS bommodifikasiuser, bom.bommodifikasitgl AS bommodifikasitgl, bom.bomposting AS bomposting, bom.bompostingtgl AS bompostingtgl, br.bnama AS bomcabangnama, lc.lnama AS bomlokasinama, wh1.wnama AS bomgudangasalnama, wh2.wnama AS bomgudangproduksinama, wh3.wnama AS bomgudangtujuannama, pc.pcnama AS bomjenisnama, c1.kkode AS bompembuatkode, c1.knama AS bompembuatnama, we.wenama AS bomestimasikerjanama, st1.nama AS bomstatusnama, st2.nama AS bomstatussebelumnyanama, u1.unama AS bominputusernama, u2.unama AS bommodifikasiusernama, bom.bomaktivitas, pa.pakode as bomaktivitaskode, pa.panama as bomaktivitasnama, i.bid, i.bkode, i.bnama from m6_bom bom join m6_bom_in bomi on bom.bomid = bomi.idbom join m1_item i on bomi.idbarang = i.bid left join m1_branch br on bom.bomcabang = br.bkode left join m1_location lc on bom.bomlokasi = lc.lkode left join m1_warehouse wh1 on bom.bomgudangasal = wh1.wkode left join m1_warehouse wh2 on bom.bomgudangproduksi = wh2.wkode left join m1_warehouse wh3 on bom.bomgudangtujuan = wh3.wkode left join m1_production_category pc on bom.bomjenis = pc.pckode left join m1_contact c1 on bom.bompembuat = c1.kid left join m1_working_estimate we on bom.bomestimasikerja = we.wekode left join m0_status st1 on bom.bomstatus = st1.kode left join m0_status st2 on bom.bomstatussebelumnya = st2.kode left join m0_user u1 on bom.bominputuser = u1.userid left join m0_user u2 on bom.bommodifikasiuser = u2.userid left join m1_production_activity pa on bom.bomaktivitas = pa.paid"

        dt = AmbilData("aplikasi1-M5_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "bom.bomid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bomid"), 0), sptField,
                     FxDB(dr("bomcabang"), ""), sptField,
                     FxDB(dr("bomlokasi"), ""), sptField,
                     FxDB(dr("bomgudangasal"), ""), sptField,
                     FxDB(dr("bomgudangproduksi"), ""), sptField,
                     FxDB(dr("bomgudangtujuan"), ""), sptField,
                     FxDB(dr("bomsumber"), ""), sptField,
                     FxDB(dr("bomjenis"), ""), sptField,
                     FxDB(dr("bomautonotransaksi"), 0), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bomtgl"), ""), formatTgl), sptField,
                     FxDB(dr("bomkodepa"), 0), sptField,
                     FxDB(dr("bompembuat"), 0), sptField,
                     FxDB(dr("bompembuatkontak"), ""), sptField,
                     FxDB(dr("bomestimasikerja"), ""), sptField,
                     FxDB(dr("bommatauang"), ""), sptField,
                     FxDB(dr("bomkurs"), 0), sptField,
                     FxDB(dr("bomtotalhargain"), 0), sptField,
                     FxDB(dr("bomtotalhargaout"), 0), sptField,
                     FxDB(dr("bomtotalhppin"), 0), sptField,
                     FxDB(dr("bomtotalhppout"), 0), sptField,
                     FxDB(dr("bomuraian"), ""), sptField,
                     FxDB(dr("bomcatatan"), ""), sptField,
                     FxDB(dr("bomnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bomtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("bomstatus"), 0), sptField,
                     FxDB(dr("bomstatussebelumnya"), 0), sptField,
                     FxDB(dr("bomjmlrevisi"), 0), sptField,
                     FxDB(dr("bomcetakanke"), 0), sptField,
                     FxDB(dr("bominputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bominputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bommodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bommodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bomposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bompostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bomcabangnama"), ""), sptField,
                     FxDB(dr("bomlokasinama"), ""), sptField,
                     FxDB(dr("bomgudangasalnama"), ""), sptField,
                     FxDB(dr("bomgudangproduksinama"), ""), sptField,
                     FxDB(dr("bomgudangtujuannama"), ""), sptField,
                     FxDB(dr("bomjenisnama"), ""), sptField,
                     FxDB(dr("bompembuatkode"), ""), sptField,
                     FxDB(dr("bompembuatnama"), ""), sptField,
                     FxDB(dr("bomestimasikerjanama"), ""), sptField,
                     FxDB(dr("bomstatusnama"), ""), sptField,
                     FxDB(dr("bomstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("bominputusernama"), ""), sptField,
                     FxDB(dr("bommodifikasiusernama"), ""), sptField,
                     FxDB(dr("bomaktivitas"), 0), sptField,
                     FxDB(dr("bomaktivitaskode"), ""), sptField,
                     FxDB(dr("bomaktivitasnama"), ""), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomposting, bompostingtgl, bomcabangnama, bomlokasinama, bomgudangasalnama, bomgudangproduksinama, bomgudangtujuannama, bomjenisnama, bompembuatkode, bompembuatnama, bomestimasikerjanama, bomstatusnama, bomstatussebelumnyanama, bominputusernama, bommodifikasiusernama, bomaktivitas, bomaktivitaskode, bomaktivitasnama, bid, bkode, bnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_BomTerkait(ByVal param As String) As String
        'M6_BomTerkait --------------------------------------------------------
        'bomid, bomnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "bomid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND bomid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "bomid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m6_bom_terkait(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m5_bom_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bomid"), 0), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
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
            result(2) = "Related BOM data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bomid, bomnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_BomTerkait_S(ByVal param As String) As String
        'M6_BomTerkait --------------------------------------------------------
        'bomid, bomnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "bomid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND bomid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "bomid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.m6_bom_terkait(Filter)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m5_bom_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bomid"), 0), sptField,
                     FxDB(dr("bomnotransaksi"), ""), sptField,
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
            result(2) = "Related BOM data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bomid, bomnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_ItemBomSearch(ByVal param As String) As String
        'M6_ItemBomSearch --------------------------------------------------------
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, 
        'rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bjmllapangan, 
        'bsatuanlapangan, prosentase, stokakhir, hargabeli, stokreal

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataDetail(), dataRowDetail() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = ""
        Dim strJml As String = "", strJmlbarang As String = "", ftBarang As String = "", idbarang As Double = 0, jmlbarang As Double = 0
        Dim dt As New DataTable, ftBarangIn As String = ""

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
            sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'MAPPING BUAT WS ----------------------------------------------------------
        'idbarang(0) As Integer, jmlbarang(1) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'idbarang, jmlbarang

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)

        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail

            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 2) Then
                result(2) = "Row : " & i & " - Invalid filter data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idbarang(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            Else
                idbarang = Double.Parse(dataRowDetail(0))
            End If
            'jmlbarang(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            Else
                jmlbarang = Double.Parse(dataRowDetail(1))
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------


            'BUAT CASE UNTUK QUERY ----------------------------------------------
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, " (ibomout.idbaranghasil = '" & FixDouble(idbarang) & "') ")

            ftBarangIn = IIf(Len(ftBarangIn.ToString) = 0, "", ftBarangIn & " OR ")
            ftBarangIn = String.Concat(ftBarangIn, " (bomin.idbarang = '" & FixDouble(idbarang) & "') ")

            strJml += " WHEN ibomout.idbaranghasil = '" & FixDouble(idbarang) & "' THEN (((ibomout.jmlbarang / ibomin.jmlbarang) * " & FixDouble(jmlbarang) & ") / ibomout.nilaisatuan) "
            strJmlbarang += " WHEN ibomout.idbaranghasil = '" & FixDouble(idbarang) & "' THEN (((ibomout.jmlbarang / ibomin.jmlbarang) * " & FixDouble(jmlbarang) & ")) "
            'END OF BUAT CASE UNTUK QUERY ---------------------------------------

        Next
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'AMBIL PROSENTASE
        Dim vTotalHarga As Double = 0
        Dim dtPersen As DataTable = AsDataTableAmbilDariDB("SELECT IFNULL(SUM(jml*harga),0) as total FROM m6_itembom_in bomin WHERE " & ftBarangIn)
        If dtPersen.Rows.Count > 0 Then
            vTotalHarga = FxDB(FixDouble(dtPersen.Rows(0)(0)), 0)
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = "SELECT bjmllapangan, bsatuanlapangan, idbarang, namabarang, tipebarang, SUM(jml) as jml, satuan, nilaisatuan, SUM(nilai) as nilai, SUM(jmlbarang) as jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi,  gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, IFNULL((stokreal-stokbooking),0) AS stokakhir, IFNULL((stokreal),0) AS stokreal, hargabeli FROM ( SELECT i.bjmllapangan, i.bsatuanlapangan, ibomout.idbaranghasil, ibomout.idbarang, ibomout.namabarang, ibomout.tipebarang, (CASE " & strJml & " END) as jml, ibomout.satuan, ibomout.nilaisatuan, ibomout.jml * ibomout.harga as nilai, (CASE " & strJmlbarang & " END) as jmlbarang, ibomout.satuanbarang, ibomout.matauang, ibomout.kurs, ibomout.harga, ibomout.hpp, ibomout.idhppkhususmasuk, ibomout.idhppfifomasuk, ibomout.rekpersediaan, ibomout.cabang, ibomout.lokasi, ibomout.gudangasal, ibomout.gudangproduksi, ibomout.gudangtujuan, ibomout.costcenter, ibomout.divisi, ibomout.subdivisi, ibomout.proyek, ibomout.catatan, ibomout.urutan, ibomout.idbom, ibomout.idbomout, ibomout.customtext1, ibomout.customtext2, ibomout.customtext3, ibomout.customdbl1, ibomout.customdbl2, ibomout.customdbl3, ibomout.customdate1, ibomout.customdate2, ibomout.customdate3, i.bkode as kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama,  bom.bomnotransaksi AS notransaksi, i.bstok AS stokreal, SUM(ib.jmlbooking) AS stokbooking, i.bhargabeli AS hargabeli FROM m6_itembom_out ibomout JOIN m6_itembom_in ibomin ON ibomout.idbaranghasil = ibomin.idbarang LEFT JOIN m1_item i ON ibomout.idbarang = i.bid LEFT JOIN m1_cost_center cc ON ibomout.costcenter = cc.cckode LEFT JOIN m1_division d ON ibomout.divisi = d.dkode LEFT JOIN m1_subdivision sd ON ibomout.subdivisi = sd.sdkode LEFT JOIN m1_project p ON ibomout.proyek = p.pkode LEFT JOIN m6_bom bom ON ibomout.idbom = bom.bomid LEFT JOIN m1_item_booking ib ON ibomout.idbarang = ib.idbarang WHERE " & ftBarang & " GROUP BY ibomout.idbarang) as bom GROUP BY idbarang, satuan"
        sql = "SELECT bjmllapangan, bsatuanlapangan, idbarang, namabarang, tipebarang, SUM(jml) as jml, satuan, nilaisatuan, SUM(nilai) as nilai, SUM(jmlbarang) as jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi,  gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, IFNULL((stokreal-stokbooking),0) AS stokakhir, IFNULL((stokreal),0) AS stokreal, hargabeli FROM ( SELECT i.bjmllapangan, i.bsatuanlapangan, ibomout.idbaranghasil, ibomout.idbarang, ibomout.namabarang, ibomout.tipebarang, (CASE " & strJml & " END) as jml, ibomout.satuan, ibomout.nilaisatuan, ibomout.jml * ibomout.harga as nilai, (CASE " & strJmlbarang & " END) as jmlbarang, ibomout.satuanbarang, ibomout.matauang, ibomout.kurs, ibomout.harga, ibomout.hpp, ibomout.idhppkhususmasuk, ibomout.idhppfifomasuk, ibomout.rekpersediaan, ibomout.cabang, ibomout.lokasi, ibomout.gudangasal, ibomout.gudangproduksi, ibomout.gudangtujuan, ibomout.costcenter, ibomout.divisi, ibomout.subdivisi, ibomout.proyek, ibomout.catatan, ibomout.urutan, ibomout.idbom, ibomout.idbomout, ibomout.customtext1, ibomout.customtext2, ibomout.customtext3, ibomout.customdbl1, ibomout.customdbl2, ibomout.customdbl3, ibomout.customdate1, ibomout.customdate2, ibomout.customdate3, i.bkode as kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama,  bom.bomnotransaksi AS notransaksi, i.bstok AS stokreal, SUM(ib.jmlbooking) AS stokbooking, i.bhargabeli AS hargabeli FROM m6_itembom_out ibomout JOIN m6_bom bom ON ibomout.idbom = bom.bomid JOIN m6_itembom_in ibomin ON ibomout.idbaranghasil = ibomin.idbarang JOIN m1_item i ON ibomout.idbarang = i.bid LEFT JOIN m1_cost_center cc ON ibomout.costcenter = cc.cckode LEFT JOIN m1_division d ON ibomout.divisi = d.dkode LEFT JOIN m1_subdivision sd ON ibomout.subdivisi = sd.sdkode LEFT JOIN m1_project p ON ibomout.proyek = p.pkode LEFT JOIN m1_item_booking ib ON ibomout.idbarang = ib.idbarang WHERE " & ftBarang & " GROUP BY ibomout.idbomout) as bom GROUP BY idbomout, idbarang, satuan"

        dt = AmbilData("aplikasi1-M5_pl_v", "", "bom.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim prosentase As Double = 0
            For Each dr As DataRow In dt.Rows
                prosentase = 0
                If vTotalHarga <> 0 Then
                    prosentase = (FxDB(FixDouble(dr("nilai")), 0) / vTotalHarga) * 100
                End If

                search = String.Concat(search,
                     FxDB(dr("idbarang"), ""), sptField,
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
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), ""), sptField,
                     FxDB(dr("idhppfifomasuk"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangproduksi"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idbom"), ""), sptField,
                     FxDB(dr("idbomout"), ""), sptField,
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
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(prosentase, 0), sptField,
                     FxDB(dr("stokakhir"), 0), sptField,
                     FxDB(dr("hargabeli"), 0), sptField,
                     FxDB(dr("stokreal"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            'result(2) = sql
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bjmllapangan, bsatuanlapangan, prosentase, stokakhir, hargabeli, stokreal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_BomSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataDetail2(), dataRowDetail2() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, idbaranghasil As Double = 0

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'bomid(0) As Integer, bomcabang(1) As String, bomlokasi(2) As String, bomgudangasal(3) As String, bomgudangproduksi(4) As String, 
        'bomgudangtujuan(5) As String, bomsumber(6) As String, bomjenis(7) As String, bomautonotransaksi(8) As Integer, bomnotransaksi(9) As String, 
        'bomtgl(10) As Date, bomkodepa(11) As Integer, bompembuat(12) As Integer, bompembuatkontak(13) As String, bomestimasikerja(14) As String, 
        'bommatauang(15) As String, bomkurs(16) As Double, bomtotalhargain(17) As Double, bomtotalhargaout(18) As Double, bomtotalhppin(19) As Double, 
        'bomtotalhppout(20) As Double, bomuraian(21) As String, bomcatatan(22) As String, bomnoref(23) As String, bomtglnoref(24) As Date, 
        'bomstatus(25) As Integer, bomstatussebelumnya(26) As Integer, bomjmlrevisi(27) As Integer, bomcetakanke(28) As Integer, bominputuser(29) As Integer, 
        'bominputtgl(30) As DateTime, bommodifikasiuser(31) As Integer, bommodifikasitgl(32) As DateTime, bomcustomtext1(33) As String, bomcustomtext2(34) As String, 
        'bomcustomtext3(35) As String, bomcustomtext4(36) As String, bomcustomtext5(37) As String, bomcustomint1(38) As Integer, bomcustomint2(39) As Integer, 
        'bomcustomint3(40) As Integer, bomcustomdbl1(41) As Double, bomcustomdbl2(42) As Double, bomcustomdbl3(43) As Double, bomcustomdate1(44) As Date, 
        'bomcustomdate2(45) As Date, bomcustomdate3(46) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bomid, bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, 
        'bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, 
        'bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, 
        'bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, 
        'bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomcustomtext1, bomcustomtext2, 
        'bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, bomcustomint3, bomcustomdbl1, 
        'bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 47) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'bomid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bomid required numeric." : GoTo selesai
        End If
        'bomautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "bomautonotransaksi required numeric." : GoTo selesai
        End If
        'bomtgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "bomtgl required date." : GoTo selesai
        End If
        'bomkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "bomkodepa required numeric." : GoTo selesai
        End If
        'bompembuat(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "bompembuat required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "bompembuat can't be empty." : GoTo selesai
        End If
        'bomkurs(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "bomkurs required numeric." : GoTo selesai
        End If
        'bomtotalhargain(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "bomtotalhargain required numeric." : GoTo selesai
        End If
        'bomtotalhargaout(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "bomtotalhargaout required numeric." : GoTo selesai
        End If
        'bomtotalhppin(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "bomtotalhppin required numeric." : GoTo selesai
        End If
        'bomtotalhppout(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "bomtotalhppout required numeric." : GoTo selesai
        End If
        'bomtglnoref(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "bomtglnoref required date." : GoTo selesai
        End If
        'bomstatus(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bomstatus required numeric." : GoTo selesai
        End If
        'bomstatussebelumnya(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "bomstatussebelumnya required numeric." : GoTo selesai
        End If
        'bomjmlrevisi(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bomjmlrevisi required numeric." : GoTo selesai
        End If
        'bomcetakanke(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "bomcetakanke required numeric." : GoTo selesai
        End If
        'bominputuser(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "bominputuser required numeric." : GoTo selesai
        End If
        'bominputtgl(30) As DateTime
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "bominputtgl required date." : GoTo selesai
        End If
        'bommodifikasiuser(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bommodifikasiuser required numeric." : GoTo selesai
        End If
        'bommodifikasitgl(32) As DateTime
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "bommodifikasitgl required date." : GoTo selesai
        End If
        'bomcustomint1(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bomcustomint1 required numeric." : GoTo selesai
        End If
        'bomcustomint2(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "bomcustomint2 required numeric." : GoTo selesai
        End If
        'bomcustomint3(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "bomcustomint3 required numeric." : GoTo selesai
        End If
        'bomcustomdbl1(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "bomcustomdbl1 required numeric." : GoTo selesai
        End If
        'bomcustomdbl2(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "bomcustomdbl2 required numeric." : GoTo selesai
        End If
        'bomcustomdbl3(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "bomcustomdbl3 required numeric." : GoTo selesai
        End If
        'bomcustomdate1(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "bomcustomdate1 required date." : GoTo selesai
        End If
        'bomcustomdate2(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "bomcustomdate2 required date." : GoTo selesai
        End If
        'bomcustomdate3(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "bomcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'bomcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bomcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bomcabang should not be more than 25 character." : GoTo selesai
        End If

        'bomlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bomlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "bomlokasi should not be more than 25 character." : GoTo selesai
        End If

        'bomgudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "bomgudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "bomgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'bomgudangproduksi(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "bomgudangproduksi can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "bomgudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'bomgudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "bomgudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "bomgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'bomsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "bomsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "bomsumber should not be more than 10 character." : GoTo selesai
        End If

        'bomjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "bomjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "bomjenis should not be more than 25 character." : GoTo selesai
        End If

        'bomnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bomnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "bomnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'bomtgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "bomtgl can't be empty" : GoTo selesai
        End If

        'bommatauang(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "bommatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "bommatauang should not be more than 25 character." : GoTo selesai
        End If

        'bomkurs(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "bomkurs can't be empty" : GoTo selesai
        End If

        'bomtotalhargain(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "bomtotalhargain can't be empty" : GoTo selesai
        End If

        'bomtotalhargaout(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "bomtotalhargaout can't be empty" : GoTo selesai
        End If

        'bomtotalhppin(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "bomtotalhppin can't be empty" : GoTo selesai
        End If

        'bomtotalhppout(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "bomtotalhppout can't be empty" : GoTo selesai
        End If

        'bomtglnoref(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "bomtglnoref can't be empty" : GoTo selesai
        End If

        'bominputtgl(30) As DateTime
        If Len(dataUtama(30)) = 0 Then
            result(2) = "bominputtgl can't be empty" : GoTo selesai
        End If

        'bommodifikasitgl(32) As DateTime
        If Len(dataUtama(32)) = 0 Then
            result(2) = "bommodifikasitgl can't be empty" : GoTo selesai
        End If

        'bomcustomdbl1(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "bomcustomdbl1 can't be empty" : GoTo selesai
        End If

        'bomcustomdbl2(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "bomcustomdbl2 can't be empty" : GoTo selesai
        End If

        'bomcustomdbl3(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "bomcustomdbl3 can't be empty" : GoTo selesai
        End If

        'bomcustomdate1(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "bomcustomdate1 can't be empty" : GoTo selesai
        End If

        'bomcustomdate2(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "bomcustomdate2 can't be empty" : GoTo selesai
        End If

        'bomcustomdate3(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "bomcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bomid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomgudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bompembuat", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bompembuatkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bommatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bominputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bominputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bommodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bommodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bomcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bomcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "bomid~bomcabang~bomlokasi~bomgudangasal~bomgudangproduksi~bomgudangtujuan~bomsumber~bomjenis~bomautonotransaksi~bomnotransaksi~bomtgl~bomkodepa~bompembuat~bompembuatkontak~bomestimasikerja~bommatauang~bomkurs~bomtotalhargain~bomtotalhargaout~bomtotalhppin~bomtotalhppout~bomuraian~bomcatatan~bomnoref~bomtglnoref~bomstatus~bomstatussebelumnya~bomjmlrevisi~bomcetakanke~bominputuser~bominputtgl~bommodifikasiuser~bommodifikasitgl~bomcustomtext1~bomcustomtext2~bomcustomtext3~bomcustomtext4~bomcustomtext5~bomcustomint1~bomcustomint2~bomcustomint3~bomcustomdbl1~bomcustomdbl2~bomcustomdbl3~bomcustomdate1~bomcustomdate2~bomcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL1 -------------------------------------------------------
        'idbomin(0) As Integer, idbom(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, customtext1(27) As String, customtext2(28) As String, customtext3(29) As String, 
        'customdbl1(30) As Double, customdbl2(31) As Double, customdbl3(32) As Double, customdate1(33) As Date, customdate2(34) As Date, 
        'customdate3(35) As Date

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idbomin, idbom, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)

        'BARANG HASIL HARUS 1 ITEM SAJA
        If dataDetail.Length > 1 Then
            result(2) = "Detail 1 : Item result should not be more then one item." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable DETAIL1
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpppersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL1 ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL1 -----------------------------------
            'CEK ARRAY DATA DETAIL1
            If (dataRowDetail.Length <> 36) Then
                result(2) = "Detail 1 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL1 ----------------------------

            'VALIDASI TIPE DATA DETAIL1 ------------------------------------------
            'idbomin(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbomin required numeric." : GoTo selesai
            End If

            'idbom(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbom required numeric." : GoTo selesai
            End If

            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'SET IDBARANG HASIL
            idbaranghasil = Double.Parse(dataRowDetail(2))

            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpppersen(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen required numeric." : GoTo selesai
            End If
            'hpp(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'customdbl1(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(35) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL1 -----------------------------------

            'VALIDASI DATA DETAIL1 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Detail 1 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpppersen(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpppersen can't be empty" : GoTo selesai
            End If

            'hpp(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(15) As String
            'If Len(dataRowDetail(15)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(15)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(18) As String
            'If Len(dataRowDetail(18)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(18)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(19) As String
            'If Len(dataRowDetail(19)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(19)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(20) As String
            'If Len(dataRowDetail(20)) = 0 Then
            '    result(2) = "Detail 1 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(20)) > 25 Then
                result(2) = "Detail 1 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(35) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL1 --------------------------------

            If AsDataTableTambahData(dtdetail, "idbomin~idbom~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35)) = False Then
                result(2) = "Detail 1 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL1 ===========================================


        'MAPPING BUAT WS DATA DETAIL2 -------------------------------------------------------
        'idbomout(0) As Integer, idbom(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, customtext1(28) As String, customtext2(29) As String, 
        'customtext3(30) As String, customdbl1(31) As Double, customdbl2(32) As Double, customdbl3(33) As Double, customdate1(34) As Date, 
        'customdate2(35) As Date, customdate3(36) As Date

        'MAPPING BUAT FLEX DATA DETAIL2 -----------------------------------------------------
        'idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL2 ======================================================
        'SPLIT PARAMETER DATA DETAIL2
        dataDetail2 = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL2 ===============================================

        'Buat datatable DETAIL2
        Dim dtdetail2 As New DataTable
        AsDataTableTambahField(dtdetail2, "idbomout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jmlbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "harga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "customdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL2 ==================================================
        Dim JmlDtDetail2 As Integer = dataDetail2.Length
        For i = 1 To JmlDtDetail2
            'SPLIT DATA DETAIL
            dataRowDetail2 = dataDetail2(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL2 -----------------------------------
            'CEK ARRAY DATA DETAIL2
            If (dataRowDetail2.Length <> 37) Then
                result(2) = "Detail 2 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL2 ----------------------------

            'VALIDASI TIPE DATA DETAIL2 ------------------------------------------
            'idbomout(0) As Integer
            If (IsNumeric(dataRowDetail2(0)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbomout required numeric." : GoTo selesai
            End If
            'idbom(1) As Integer
            If (IsNumeric(dataRowDetail2(1)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbom required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail2(2)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail2(5)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail2(7)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail2(8) = Double.Parse(dataRowDetail2(5)) * Double.Parse(dataRowDetail2(7))
            If (IsNumeric(dataRowDetail2(8)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail2(11)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail2(12)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(13) As Double
            If (IsNumeric(dataRowDetail2(13)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'idhppkhususmasuk(14) As Integer
            If (IsNumeric(dataRowDetail2(14)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(15) As Integer
            If (IsNumeric(dataRowDetail2(15)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail2(27)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'customdbl1(31) As Double
            If (IsNumeric(dataRowDetail2(31)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(32) As Double
            If (IsNumeric(dataRowDetail2(32)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(33) As Double
            If (IsNumeric(dataRowDetail2(33)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(34) As Date
            If (IsDate(dataRowDetail2(34)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(35) As Date
            If (IsDate(dataRowDetail2(35)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(36) As Date
            If (IsDate(dataRowDetail2(36)) = False) Then
                result(2) = "Detail 2 Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL2 -----------------------------------

            'VALIDASI DATA DETAIL2 ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail2(3)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(3)) > 100 Then
                result(2) = "Detail 2 Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowDetail2(5)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail2(5) <= 0 Then
                result(2) = "Detail 2 Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail2(6)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(6)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail2(7)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail2(8)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail2(8) <= 0 Then
                result(2) = "Detail 2 Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail2(9)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail2(9)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail2(11)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail2(12)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(13) As Double
            If Len(dataRowDetail2(13)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'rekpersediaan(16) As String
            'If Len(dataRowDetail2(16)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(16)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - rekpersediaan should not be more than 25 character." : GoTo selesai
            End If

            'gudangasal(19) As String
            'If Len(dataRowDetail2(19)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(19)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangproduksi(20) As String
            'If Len(dataRowDetail2(20)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangproduksi can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(20)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangproduksi should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(21) As String
            'If Len(dataRowDetail2(21)) = 0 Then
            '    result(2) = "Detail 2 Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail2(21)) > 25 Then
                result(2) = "Detail 2 Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(31) As Double
            If Len(dataRowDetail2(31)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(32) As Double
            If Len(dataRowDetail2(32)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(33) As Double
            If Len(dataRowDetail2(33)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(34) As Date
            If Len(dataRowDetail2(34)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(35) As Date
            If Len(dataRowDetail2(35)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(36) As Date
            If Len(dataRowDetail2(36)) = 0 Then
                result(2) = "Detail 2 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL2 --------------------------------

            If AsDataTableTambahData(dtdetail2, "idbomout~idbom~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususmasuk~idhppfifomasuk~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail2(0) & "~" & dataRowDetail2(1) & "~" & dataRowDetail2(2) & "~" & dataRowDetail2(3) & "~" & dataRowDetail2(4) & "~" & dataRowDetail2(5) & "~" & dataRowDetail2(6) & "~" & dataRowDetail2(7) & "~" & dataRowDetail2(8) & "~" & dataRowDetail2(9) & "~" & dataRowDetail2(10) & "~" & dataRowDetail2(11) & "~" & dataRowDetail2(12) & "~" & dataRowDetail2(13) & "~" & dataRowDetail2(14) & "~" & dataRowDetail2(15) & "~" & dataRowDetail2(16) & "~" & dataRowDetail2(17) & "~" & dataRowDetail2(18) & "~" & dataRowDetail2(19) & "~" & dataRowDetail2(20) & "~" & dataRowDetail2(21) & "~" & dataRowDetail2(22) & "~" & dataRowDetail2(23) & "~" & dataRowDetail2(24) & "~" & dataRowDetail2(25) & "~" & dataRowDetail2(26) & "~" & dataRowDetail2(27) & "~" & dataRowDetail2(28) & "~" & dataRowDetail2(29) & "~" & dataRowDetail2(30) & "~" & dataRowDetail2(31) & "~" & dataRowDetail2(32) & "~" & dataRowDetail2(33) & "~" & dataRowDetail2(34) & "~" & dataRowDetail2(35) & "~" & dataRowDetail2(36)) = False Then
                result(2) = "Detail 2 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL2 ===========================================



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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("bomtgl")), AsFormatTanggal(drutama("bomtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                If isUpdate Then
                    result(4) = drutama("bomid")
                    notransaksi = drutama("bomnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bomid), bomnotransaksi FROM M6_bom WHERE bomid='" & result(4) & "' AND bomstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bomid) FROM M6_bom WHERE bomnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_bom_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Bom_HistorySimpan("" & paramSplit(0) & "★M6_Bom_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("bomsumber")) & "▼" & FixQuotes(drutama("bomid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Bom set bomcabang  = '" & FixQuotes(drutama("bomcabang")) & "', bomlokasi  = '" & FixQuotes(drutama("bomlokasi")) & "', bomgudangasal  = '" & FixQuotes(drutama("bomgudangasal")) & "', bomgudangproduksi  = '" & FixQuotes(drutama("bomgudangproduksi")) & "', bomgudangtujuan  = '" & FixQuotes(drutama("bomgudangtujuan")) & "', bomsumber  = '" & FixQuotes(drutama("bomsumber")) & "', bomjenis  = '" & FixQuotes(drutama("bomjenis")) & "', bomautonotransaksi  = " & drutama("bomautonotransaksi") & ", bomnotransaksi  = '" & FixQuotes(notransaksi) & "', bomtgl  = '" & FixQuotes(AsFormatTanggal(drutama("bomtgl"))) & "', bomkodepa  = " & drutama("bomkodepa") & ", bompembuat  = " & drutama("bompembuat") & ", bompembuatkontak  = '" & FixQuotes(drutama("bompembuatkontak")) & "', bomestimasikerja  = '" & FixQuotes(drutama("bomestimasikerja")) & "', bommatauang  = '" & FixQuotes(drutama("bommatauang")) & "', bomkurs  = '" & FixDouble(drutama("bomkurs")) & "', bomtotalhargain  = '" & FixDouble(drutama("bomtotalhargain")) & "', bomtotalhargaout  = '" & FixDouble(drutama("bomtotalhargaout")) & "', bomtotalhppin  = '" & FixDouble(drutama("bomtotalhppin")) & "', bomtotalhppout  = '" & FixDouble(drutama("bomtotalhppout")) & "', bomuraian  = '" & FixQuotes(drutama("bomuraian")) & "', bomcatatan  = '" & FixQuotes(drutama("bomcatatan")) & "', bomnoref  = '" & FixQuotes(drutama("bomnoref")) & "', bomtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("bomtglnoref"))) & "', bomstatus  = " & drutama("bomstatus") & ", bomstatussebelumnya  = " & drutama("bomstatussebelumnya") & ", bomjmlrevisi  = bomjmlrevisi+1, bomcetakanke  = " & drutama("bomcetakanke") & ", bommodifikasiuser  = " & drutama("bommodifikasiuser") & ", bommodifikasitgl  = NOW(), bomcustomtext1  = '" & FixQuotes(drutama("bomcustomtext1")) & "', bomcustomtext2  = '" & FixQuotes(drutama("bomcustomtext2")) & "', bomcustomtext3  = '" & FixQuotes(drutama("bomcustomtext3")) & "', bomcustomtext4  = '" & FixQuotes(drutama("bomcustomtext4")) & "', bomcustomtext5  = '" & FixQuotes(drutama("bomcustomtext5")) & "', bomcustomint1  = " & drutama("bomcustomint1") & ", bomcustomint2  = " & drutama("bomcustomint2") & ", bomcustomint3  = " & drutama("bomcustomint3") & ", bomcustomdbl1  = '" & FixDouble(drutama("bomcustomdbl1")) & "', bomcustomdbl2  = '" & FixDouble(drutama("bomcustomdbl2")) & "', bomcustomdbl3  = '" & FixDouble(drutama("bomcustomdbl3")) & "', bomcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate1"))) & "', bomcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate2"))) & "', bomcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate3"))) & "' where bomid = '" & drutama("bomid") & "'"
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

                    If drutama("bomautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("bomcabang"), drutama("bomlokasi"), drutama("bomsumber"), drutama("bomtgl"))
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
                        notransaksi = drutama("bomnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bomid) FROM m6_bom WHERE bomnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Bom (bomcabang, bomlokasi, bomgudangasal, bomgudangproduksi, bomgudangtujuan, bomsumber, bomjenis, bomautonotransaksi, bomnotransaksi, bomtgl, bomkodepa, bompembuat, bompembuatkontak, bomestimasikerja, bommatauang, bomkurs, bomtotalhargain, bomtotalhargaout, bomtotalhppin, bomtotalhppout, bomuraian, bomcatatan, bomnoref, bomtglnoref, bomstatus, bomstatussebelumnya, bomjmlrevisi, bomcetakanke, bominputuser, bominputtgl, bommodifikasiuser, bommodifikasitgl, bomcustomtext1, bomcustomtext2, bomcustomtext3, bomcustomtext4, bomcustomtext5, bomcustomint1, bomcustomint2, bomcustomint3, bomcustomdbl1, bomcustomdbl2, bomcustomdbl3, bomcustomdate1, bomcustomdate2, bomcustomdate3) values('" & FixQuotes(drutama("bomcabang")) & "', '" & FixQuotes(drutama("bomlokasi")) & "', '" & FixQuotes(drutama("bomgudangasal")) & "', '" & FixQuotes(drutama("bomgudangproduksi")) & "', '" & FixQuotes(drutama("bomgudangtujuan")) & "', '" & FixQuotes(drutama("bomsumber")) & "', '" & FixQuotes(drutama("bomjenis")) & "', " & drutama("bomautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomtgl"))) & "', " & drutama("bomkodepa") & ", " & drutama("bompembuat") & ", '" & FixQuotes(drutama("bompembuatkontak")) & "', '" & FixQuotes(drutama("bomestimasikerja")) & "', '" & FixQuotes(drutama("bommatauang")) & "', '" & FixDouble(drutama("bomkurs")) & "', '" & FixDouble(drutama("bomtotalhargain")) & "', '" & FixDouble(drutama("bomtotalhargaout")) & "', '" & FixDouble(drutama("bomtotalhppin")) & "', '" & FixDouble(drutama("bomtotalhppout")) & "', '" & FixQuotes(drutama("bomuraian")) & "', '" & FixQuotes(drutama("bomcatatan")) & "', '" & FixQuotes(drutama("bomnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomtglnoref"))) & "', " & drutama("bomstatus") & ", " & drutama("bomstatussebelumnya") & ", " & drutama("bomjmlrevisi") & ", " & drutama("bomcetakanke") & ", " & drutama("bominputuser") & ", NOW(), " & drutama("bommodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(drutama("bomcustomtext1")) & "', '" & FixQuotes(drutama("bomcustomtext2")) & "', '" & FixQuotes(drutama("bomcustomtext3")) & "', '" & FixQuotes(drutama("bomcustomtext4")) & "', '" & FixQuotes(drutama("bomcustomtext5")) & "', " & drutama("bomcustomint1") & ", " & drutama("bomcustomint2") & ", " & drutama("bomcustomint3") & ", '" & FixDouble(drutama("bomcustomdbl1")) & "', '" & FixDouble(drutama("bomcustomdbl2")) & "', '" & FixDouble(drutama("bomcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("bomcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select bomid from M6_bom where bomnotransaksi='" & notransaksi & "' AND bominputuser= '" & userid & "' order by bommodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Bom_In where idbom = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail1
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idbomin") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Bom_In(idbomin, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail In Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail2 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Bom_Out where idbom = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail2
                If (dtdetail2.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail2.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idbomout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Bom_Out(idbomout, idbom, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Out Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'PROSES POSTING KE M6_ITEMBOM --------------------------------
                If drutama("bomstatus") = 2 Then

                    'DELETE M6_ITEMBOM_IN
                    sql = "DELETE FROM m6_itembom_in WHERE idbarang = '" & FixDouble(idbaranghasil) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'DELETE M6_ITEMBOM_OUT
                    sql = "DELETE FROM m6_itembom_out WHERE idbaranghasil = '" & FixDouble(idbaranghasil) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT M6_ITEMBOM_IN
                    sql = "INSERT INTO m6_itembom_in (SELECT  idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_in bomin WHERE bomin.idbom = '" & result(4) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT M6_ITEMBOM_OUT
                    sql = "INSERT INTO m6_itembom_out (SELECT '" & FixDouble(idbaranghasil) & "', idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_out bomout WHERE bomout.idbom = '" & result(4) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                End If
                'END OF PROSES POSTING KE M6_ITEMBOM -------------------------


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M6_BomUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Bom", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Bomtgl, Bomnotransaksi, Bomstatus FROM M6_Bom WHERE Bomid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Bomstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_bom_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Bom_HistorySimpan("" & paramSplit(0) & "★M6_Bom_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m6_bom_terkait("bomid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M6_Bom SET Bomstatus = " & nilaiStatus & ", Bommodifikasiuser='" & userid & "', Bommodifikasitgl = NOW(), Bomposting = 0, Bompostingtgl = '1971-01-01 00:00:00', Bomjmlrevisi = Bomjmlrevisi + 1 WHERE Bomid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'UNPOSTING M6_ITEM BOM ==========================================================
            'AMBIL IDBARANG HASIL DARI M6_BOM_IN
            Dim dtBarangHasil As DataTable = AsDataTableAmbilDariDB("SELECT idbarang FROM m6_bom_in WHERE idbom = '" & FixDouble(idtransaksi) & "'")
            If dtBarangHasil.Rows.Count > 0 Then
                Dim idBarangHasil As Double = Double.Parse(dtBarangHasil.Rows(0)(0))

                'DELETE M6_ITEMBOM_IN
                sql = "DELETE FROM m6_itembom_in WHERE idbarang = '" & FixDouble(idBarangHasil) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE M6_ITEMBOM_OUT
                sql = "DELETE FROM m6_itembom_out WHERE idbaranghasil = '" & FixDouble(idBarangHasil) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'AMBIL BOM SEBELUMNYA YANG AKTIF
                Dim dtBom As DataTable = AsDataTableAmbilDariDB("SELECT bom.bomid FROM m6_bom_in bomin JOIN m6_bom bom ON bomin.idbom = bom.bomid WHERE bomin.idbarang = '" & idBarangHasil & "' AND bom.bomstatus IN(2,3,4,7) ORDER BY bominputtgl DESC LIMIT 1")
                If dtBom.Rows.Count > 0 Then
                    Dim idbom As Double = Double.Parse(dtBom.Rows(0)(0))

                    'INSERT M6_ITEMBOM_IN
                    sql = "INSERT INTO m6_itembom_in (SELECT  idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomin, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_in bomin WHERE bomin.idbom = '" & idbom & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT M6_ITEMBOM_OUT
                    sql = "INSERT INTO m6_itembom_out (SELECT '" & FixDouble(idBarangHasil) & "', idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbom, idbomout, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3 FROM m6_bom_out bomout WHERE bomout.idbom = '" & idbom & "')"
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
            'END OF UNPOSTING M6_ITEM BOM ===================================================

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
            Dim paramSearch As String = M6_BomSearch(PostWsSearch(paramSplit(0), "M6_BomSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_BomDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Bom", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Bomid, Bomnotransaksi FROM M6_Bom WHERE Bomid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT bomcabang, bomlokasi, bomsumber, bomautonotransaksi, bomnotransaksi, bomtgl"
            sql &= " FROM M6_bom"
            sql &= " WHERE bomid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("bomcabang")
                lokasi = dtNomorNext.Rows(0)("bomlokasi")
                sumber = dtNomorNext.Rows(0)("bomsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("bomautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("bomnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("bomtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_Bom_In WHERE idBom ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL2
            sql = "DELETE FROM M6_Bom_Out WHERE idBom ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M6_Bom WHERE Bomid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_BomSearch(PostWsSearch(paramSplit(0), "M6_BomSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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