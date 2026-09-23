Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_rm
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_RmSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama() As String

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
        If (dataSplit.Length <> 1) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'kjid(0) As Integer, kjcabang(1) As String, kjlokasi(2) As String, kjsumber(3) As String, kjautonotransaksi(4) As Integer, kjnotransaksi(5) As String, kjtgl(6) As Date, kjkodepa(7) As Integer, kjnopasien(8) As String, 
        'kjnama(9) As String, kjprefix(10) As String, kjtgllahir(11) As Date, kjumur(12) As Integer, kjjeniskelamin(13) As String, kjstatusperkawinan(14) As Integer,
        'kjagama(15) As Integer, kjayah(16) As String, kjibu(17) As String, kjsuamiistri(18) As String, kjnotelepon(19) As String, kjnofax(20) As String,
        'kjnohp(21) As String, kjemail(22) As String, kjalamat(23) As String, kjkota(24) As String, kjprovinsi(25) As String, kjnegara(26) As String, 
        'kjkodepos(27) As String, kjkeluargalain(28) As String, kjnoteleponlain(29) As String, kjcatatan(30) As String,
        'kjtglkeluar(31) As Date, kjtglmeninggal(32) As Date, kjcarakunjungan(33) As Integer, kjdirujukoleh(34) As Integer, kjditanggungoleh(35) As Integer, 
        'kjstatusrealisasi(36) As Interger, kjstatus(37) As Integer, kjstatussebelumnya(38) As Integer, kjjmlrevisi(39) As Integer, kjcetakanke(40) As Integer, 
        'kjinputuser(41) As Integer, kjinputtgl(42) As DateTime, kjmodifikasiuser(43) As Integer, kjmodifikasitgl(44) As DateTime, kjisclose(45) As Integer, 
        'kjcustomtext1(46) As String, kjcustomtext2(47) As String, kjcustomtext3(48) As String, kjcustomtext4(49) As String, kjcustomtext5(50) As String, 
        'kjcustomtext6(51) As String, kjcustomtext7(52) As String, kjcustomtext8(53) As String, kjcustomtext9(54) As String, kjcustomtext10(55) As String,
        'kjcustomtext11(56) As String, kjcustomtext12(57) As String, kjcustomtext13(58) As String, kjcustomtext14(59) As String, kjcustomtext15(60) As String, 
        'kjcustomtext16(61) As String, kjcustomtext17(62) As String, kjcustomtext18(63) As String, kjcustomtext19(64) As String, kjcustomtext20(65) As String, 
        'kjcustomint1(66) As Integer, kjcustomint2(67) As Integer, kjcustomint3(68) As Integer, kjcustomint4(69) As Integer, kjcustomint5(70) As Integer, 
        'kjcustomint6(71) As Integer, kjcustomint7(72) As Integer, kjcustomint8(73) As Integer, kjcustomint9(74) As Integer, kjcustomint10(75) As Integer, 
        'kjcustomint11(76) As Integer, kjcustomint12(77) As Integer, kjcustomint13(78) As Integer, kjcustomint14(79) As Integer, kjcustomint15(80) As Integer, 
        'kjcustomint16(81) As Integer, kjcustomint17(82) As Integer, kjcustomint18(83) As Integer, kjcustomint19(84) As Integer, kjcustomint20(85) As Integer,
        'kjcustomdbl1(86) As Double, kjcustomdbl2(87) As Double, kjcustomdbl3(88) As Double, kjcustomdbl4(89) As Double, kjcustomdbl5(90) As Double, 
        'kjcustomdbl6(91) As Double, kjcustomdbl7(92) As Double, kjcustomdbl8(93) As Double, kjcustomdbl9(94) As Double, kjcustomdbl10(95) As Double, 
        'kjcustomdbl11(96) As Double, kjcustomdbl12(97) As Double, kjcustomdbl13(98) As Double, kjcustomdbl14(99) As Double, kjcustomdbl15(100) As Double, 
        'kjcustomdbl16(101) As Double, kjcustomdbl17(102) As Double, kjcustomdbl18(103) As Double, kjcustomdbl19(104) As Double, kjcustomdbl20(105) As Double, 
        'kjcustomdate1(106) As Date, kjcustomdate2(107) As Date, kjcustomdate3(108) As Date, kjcustomdate4(109) As Date, kjcustomdate5(110) As Date,
        'kjcustomdate6(111) As Date, kjcustomdate7(112) As Date, kjcustomdate8(113) As Date, kjcustomdate9(114) As Date, kjcustomdate10(115) As Date,
        'kjcustomdate11(116) As Date, kjcustomdate12(117) As Date, kjcustomdate13(118) As Date, kjcustomdate14(119) As Date, kjcustomdate15(120) As Date,
        'kjcustomdate16(121) As Date, kjcustomdate17(122) As Date, kjcustomdate18(123) As Date, kjcustomdate19(124) As Date, kjcustomdate20(125) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 85) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kjid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rmid required numeric." : GoTo selesai
        End If
        ''kjautonotransaksi(2) As Integer
        'If (IsNumeric(dataUtama(4)) = False) Then
        '    result(2) = "kjautonotransaksi required numeric." : GoTo selesai
        'End If
        'kjtgl(4) As Date
        If (IsDate(dataUtama(8)) = False) Then
            result(2) = "rmtgl required date." : GoTo selesai
        End If
        'kjkodepa(5) As Integer
        'If (IsNumeric(dataUtama(7)) = False) Then
        '    result(2) = "kjkodepa required numeric." : GoTo selesai
        'End If

        'If (IsNumeric(dataUtama(12)) = False) Then
        '    result(2) = "kjumur required numeric." : GoTo selesai
        'End If
        ''statusperkawinan(7) As Integer
        'If (IsNumeric(dataUtama(14)) = False) Then
        '    result(2) = "kjstatusperkawinan required numeric." : GoTo selesai
        'End If
        ''agama(8) As Integer
        'If (IsNumeric(dataUtama(15)) = False) Then
        '    result(2) = "kjagama required numeric." : GoTo selesai
        'End If

        'kjstatusrealisasi(12) As Interger
        'If (IsNumeric(dataUtama(36)) = False) Then
        '    result(2) = "kjstatusrealisasi required numeric." : GoTo selesai
        'End If
        'kjstatus(13) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rmstatus required numeric." : GoTo selesai
        End If
        'kjstatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rmstatussebelumnya required numeric." : GoTo selesai
        End If
        'kjjmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rmjmlrevisi required numeric." : GoTo selesai
        End If
        'kjcetakanke(16) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rmcetakanke required numeric." : GoTo selesai
        End If
        'kjinputuser(17) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rminputuser required numeric." : GoTo selesai
        End If
        'kjinputtgl(18) As DateTime
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "rminputtgl required date." : GoTo selesai
        End If
        'kjmodifikasiuser(19) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rmmodifikasiuser required numeric." : GoTo selesai
        End If
        'kjmodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "rmmodifikasitgl required date." : GoTo selesai
        End If
        'kjisclose(21) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rmisclose required numeric." : GoTo selesai
        End If
        'kjcustomint1(42) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rmjmlrawat required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================

        'kjnotransaksi(3) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "rmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kjtgl(4) As Date
        If Len(dataUtama(8)) = 0 Then
            result(2) = "rmtgl can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmnorm", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmlayanan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmdokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkecelakaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmkasus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmicd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmtindaklanjut", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmkrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcarakrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmjmlrawat", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmstatusimunisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmumur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmketumur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmrujukan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrujukandetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrehabmedik", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmhamilke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmpersalinan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmkeadaanbayi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmjeniskelamin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmpanjang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rmberat", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rmketerangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmicd10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmdokumen", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtpip11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtpip12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtpip13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtpip4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmtpip5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd21", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd22", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd18a", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd31", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd32", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd33", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd34", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd35", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmigd7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmvk10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmvk10b", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmvk22bayi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat36", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat37", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat38", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat21a", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat21b", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat22", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmfp16oral", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmgizi17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmoklapanastesi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmok19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmalasan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmrawat18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmok18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rmdiagnosa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmcatatandiagnosa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmlokasidokumen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rmicd10nama", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rmid~rmidkj~rmnorm~rmperawatan~rmkategoripasien~rmlayanan~rmdokter~rmkecelakaan~rmtgl~rmnotransaksi~rmkasus~rmicd~rmtindaklanjut~rmkrs~rmcarakrs~rmstatus~rmstatussebelumnya~rmjmlrevisi~rmcetakanke~rminputuser~rminputtgl~rmmodifikasiuser~rmmodifikasitgl~rmisclose~rmjmlrawat~rmsumber~rmawalankatpasien~rmstatusimunisasi~rmtgllahir~rmumur~rmketumur~rmrujukan~rmrujukandetail~rmrehabmedik~rmhamilke~rmpersalinan~rmkeadaanbayi~rmjeniskelamin~rmpanjang~rmberat~rmketerangan~rmicd10~rmdokumen~rmtpip11~rmtpip12~rmtpip13~rmtpip4~rmtpip5~rmigd21~rmigd22~rmigd18a~rmigd31~rmigd32~rmigd33~rmigd34~rmigd35~rmigd6~rmigd7~rmvk10~rmvk10b~rmvk22bayi~rmrawat36~rmrawat37~rmrawat38~rmrawat9~rmrawat10~rmrawat14~rmrawat15~rmrawat16~rmrawat20~rmrawat21a~rmrawat21b~rmrawat22~rmfp16oral~rmgizi17~rmoklapanastesi~rmok19~rmpetugas~rmalasan~rmrawat18~rmok18~rmdiagnosa~rmcatatandiagnosa~rmlokasidokumen~rmicd10nama", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 45
                Select Case drutama("rmstatus")
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


                If isUpdate Then
                    result(4) = drutama("rmid")
                    notransaksi = drutama("rmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rmid), rmnotransaksi FROM M_11_rm WHERE rmid='" & result(4) & "' AND rmstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rmid) FROM M_11_rm WHERE rmnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New M11_Rm_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M11_Rm_HistorySimpan("" & paramSplit(0) & "★M11_Rm_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("kjsumber")) & "▼" & FixQuotes(drutama("kjid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update m_11_rm set rmidkj  = " & drutama("rmidkj") & ", rmnorm  = '" & FixQuotes(drutama("rmnorm")) & "', rmperawatan  = '" & FixQuotes(drutama("rmperawatan")) & "', rmkategoripasien  = '" & FixQuotes(drutama("rmkategoripasien")) & "', rmlayanan  = '" & FixQuotes(drutama("rmlayanan")) & "', rmdokter  = '" & FixQuotes(drutama("rmdokter")) & "', rmkecelakaan = '" & FixQuotes(drutama("rmkecelakaan")) & "', rmtgl = '" & FixQuotes(AsFormatTanggal(drutama("rmtgl"))) & "', rmnotransaksi = '" & FixQuotes(drutama("rmnotransaksi")) & "', rmkasus = " & drutama("rmkasus") & ", rmicd = '" & FixQuotes(drutama("rmicd")) & "', rmtindaklanjut = " & drutama("rmtindaklanjut") & ", rmkrs = " & drutama("rmkrs") & ", rmcarakrs = " & drutama("rmcarakrs") & ", rmstatus  = " & drutama("rmstatus") & ", rmstatussebelumnya  = " & drutama("rmstatussebelumnya") & ", rmjmlrevisi = rmjmlrevisi+1, rmcetakanke  = " & drutama("rmcetakanke") & ", rmmodifikasiuser  = " & drutama("rmmodifikasiuser") & ", rmmodifikasitgl  = NOW(), rmjmlrawat  = " & drutama("rmjmlrawat") & ", rmstatusimunisasi  = " & drutama("rmstatusimunisasi") & ", rmtgllahir = '" & FixQuotes(AsFormatTanggal(drutama("rmtgllahir"))) & "', rmumur = " & drutama("rmumur") & ", rmketumur = '" & FixQuotes(drutama("rmketumur")) & "', rmrujukan = " & drutama("rmrujukan") & ", rmrujukandetail = " & drutama("rmrujukandetail") & ", rmrehabmedik = '" & FixQuotes(drutama("rmrehabmedik")) & "', rmhamilke = " & drutama("rmhamilke") & ", rmpersalinan = " & drutama("rmpersalinan") & ", rmkeadaanbayi = " & drutama("rmkeadaanbayi") & ", rmjeniskelamin = '" & FixQuotes(drutama("rmjeniskelamin")) & "', rmpanjang = " & drutama("rmpanjang") & ", rmberat = " & drutama("rmberat") & ", rmketerangan = '" & FixQuotes(drutama("rmketerangan")) & "', rmicd10 = '" & FixQuotes(drutama("rmicd10")) & "', rmdokumen = " & drutama("rmdokumen") & ", rmtpip11 = " & drutama("rmtpip11") & ", rmtpip12 = " & drutama("rmtpip12") & ", rmtpip13 = " & drutama("rmtpip13") & ", rmtpip4 = " & drutama("rmtpip4") & ", rmtpip5 = " & drutama("rmtpip5") & ", rmigd21 = " & drutama("rmigd21") & ", rmigd22 = " & drutama("rmigd22") & ", rmigd18a = " & drutama("rmigd18a") & ", rmigd31 = " & drutama("rmigd31") & ", rmigd32 = " & drutama("rmigd32") & ", rmigd33 = " & drutama("rmigd33") & ", rmigd34 = " & drutama("rmigd34") & ", rmigd35 = " & drutama("rmigd35") & ", rmigd6 = " & drutama("rmigd6") & ", rmigd7 = " & drutama("rmigd7") & ", rmvk10 = " & drutama("rmvk10") & ", rmvk10b = " & drutama("rmvk10b") & ", rmvk22bayi = " & drutama("rmvk22bayi") & ", rmrawat36 = " & drutama("rmrawat36") & ", rmrawat37 = " & drutama("rmrawat37") & ", rmrawat38 = " & drutama("rmrawat38") & ", rmrawat9 = " & drutama("rmrawat9") & ", rmrawat10 = " & drutama("rmrawat10") & ", rmrawat14 = " & drutama("rmrawat14") & ", rmrawat15 = " & drutama("rmrawat15") & ", rmrawat16 = " & drutama("rmrawat16") & ", rmrawat20 = " & drutama("rmrawat20") & ", rmrawat21a = " & drutama("rmrawat21a") & ", rmrawat21b = " & drutama("rmrawat21b") & ", rmrawat22 = " & drutama("rmrawat22") & ", rmfp16oral = " & drutama("rmfp16oral") & ", rmgizi17 = " & drutama("rmgizi17") & ", rmoklapanastesi = " & drutama("rmoklapanastesi") & ", rmok19 = " & drutama("rmok19") & ", rmpetugas = " & drutama("rmpetugas") & ", rmalasan = " & drutama("rmalasan") & ", rmrawat18 = " & drutama("rmrawat18") & ", rmok18 = " & drutama("rmok18") & ", rmdiagnosa = '" & FixQuotes(drutama("rmdiagnosa")) & "', rmcatatandiagnosa = '" & FixQuotes(drutama("rmcatatandiagnosa")) & "', rmlokasidokumen = '" & FixQuotes(drutama("rmlokasidokumen")) & "', rmicd10nama = '" & FixQuotes(drutama("rmicd10nama")) & "' where rmid = '" & drutama("rmid") & "'"
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

                    'If drutama("kjautonotransaksi") = 1 Then

                    'GENERATE NOTRANSAKSI =========================================
                    Dim wsM0_Nomor As New m0_nomor
                    Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("rmperawatan"), drutama("rmawalankatpasien"), drutama("rmsumber"), drutama("rmtgl"))
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

                    '    Else
                    '    notransaksi = drutama("kjnotransaksi")
                    'End If
                    'result(2) = notransaksi + " " + userid + " Dtdetail : " + dtdetail.Rows.Count.ToString : GoTo selesai
                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rmid) FROM m_11_rm WHERE rmnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into m_11_rm (rmidkj, rmnorm, rmperawatan, rmkategoripasien, rmlayanan, rmdokter, rmkecelakaan, rmtgl, rmnotransaksi, rmkasus, rmicd, rmtindaklanjut, rmkrs, rmcarakrs, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmisclose, rmjmlrawat, rmstatusimunisasi, rmtgllahir,rmumur,rmketumur,rmrujukan,rmrujukandetail,rmrehabmedik,rmhamilke,rmpersalinan,rmkeadaanbayi,rmjeniskelamin,rmpanjang,rmberat,rmketerangan,rmicd10,rmdokumen,rmtpip11,rmtpip12,rmtpip13,rmtpip4,rmtpip5,rmigd21,rmigd22,rmigd18a,rmigd31,rmigd32,rmigd33,rmigd34,rmigd35,rmigd6,rmigd7,rmvk10,rmvk10b,rmvk22bayi,rmrawat36,rmrawat37,rmrawat38,rmrawat9,rmrawat10,rmrawat14,rmrawat15,rmrawat16,rmrawat20,rmrawat21a,rmrawat21b,rmrawat22,rmfp16oral,rmgizi17,rmoklapanastesi,rmok19,rmpetugas,rmalasan,rmrawat18,rmok18,rmdiagnosa,rmcatatandiagnosa,rmlokasidokumen,rmicd10nama) values(" & drutama("rmidkj") & ",'" & FixQuotes(drutama("rmnorm")) & "','" & FixQuotes(drutama("rmperawatan")) & "', '" & FixQuotes(drutama("rmkategoripasien")) & "', '" & FixQuotes(drutama("rmlayanan")) & "', '" & FixQuotes(drutama("rmdokter")) & "', '" & FixQuotes(drutama("rmkecelakaan")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rmtgl"))) & "', '" & FixQuotes(notransaksi) & "', " & drutama("rmkasus") & ", '" & FixQuotes(drutama("rmicd")) & "', " & drutama("rmtindaklanjut") & ", " & drutama("rmkrs") & ", " & drutama("rmcarakrs") & ", " & drutama("rmstatus") & ", " & drutama("rmstatussebelumnya") & ", " & drutama("rmjmlrevisi") & ", " & drutama("rmcetakanke") & ", " & drutama("rminputuser") & ", NOW(), " & drutama("rmmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("rmisclose") & ", " & drutama("rmjmlrawat") & ", " & drutama("rmstatusimunisasi") & ", '" & FixQuotes(AsFormatTanggal(drutama("rmtgllahir"))) & "'," & drutama("rmumur") & ",'" & FixQuotes(drutama("rmketumur")) & "'," & drutama("rmrujukan") & "," & drutama("rmrujukandetail") & ",'" & FixQuotes(drutama("rmrehabmedik")) & "'," & drutama("rmhamilke") & "," & drutama("rmpersalinan") & "," & drutama("rmkeadaanbayi") & ",'" & FixQuotes(drutama("rmjeniskelamin")) & "'," & FixDouble(drutama("rmpanjang")) & "," & FixDouble(drutama("rmberat")) & ",'" & FixQuotes(drutama("rmketerangan")) & "','" & FixQuotes(drutama("rmicd10")) & "'," & drutama("rmdokumen") & "," & drutama("rmtpip11") & "," & drutama("rmtpip12") & "," & drutama("rmtpip13") & "," & drutama("rmtpip4") & "," & drutama("rmtpip5") & "," & drutama("rmigd21") & "," & drutama("rmigd22") & "," & drutama("rmigd18a") & "," & drutama("rmigd31") & "," & drutama("rmigd32") & "," & drutama("rmigd33") & "," & drutama("rmigd34") & "," & drutama("rmigd35") & "," & drutama("rmigd6") & "," & drutama("rmigd7") & "," & drutama("rmvk10") & "," & drutama("rmvk10b") & "," & drutama("rmvk22bayi") & "," & drutama("rmrawat36") & "," & drutama("rmrawat37") & "," & drutama("rmrawat38") & "," & drutama("rmrawat9") & "," & drutama("rmrawat10") & "," & drutama("rmrawat14") & "," & drutama("rmrawat15") & "," & drutama("rmrawat16") & "," & drutama("rmrawat20") & "," & drutama("rmrawat21a") & "," & drutama("rmrawat21b") & "," & drutama("rmrawat22") & "," & drutama("rmfp16oral") & "," & drutama("rmgizi17") & "," & drutama("rmoklapanastesi") & "," & drutama("rmok19") & "," & drutama("rmpetugas") & "," & drutama("rmalasan") & "," & drutama("rmrawat18") & "," & drutama("rmok18") & ",'" & FixQuotes(drutama("rmdiagnosa")) & "','" & FixQuotes(drutama("rmcatatandiagnosa")) & "','" & FixQuotes(drutama("rmlokasidokumen")) & "','" & FixQuotes(drutama("rmicd10nama")) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'result(2) = "nananana" : Trans.Rollback() : GoTo selesai

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDBCon("select rmid from m_11_rm where rmnotransaksi='" & notransaksi & "' AND rminputuser= '" & userid & "' order by rmmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "RM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M11_RmUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "RM", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT rmtgl, rmnotransaksi, rmstatus, rmidkj FROM m_11_rm WHERE rmid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'idkj
                idkj = Integer.Parse(FxDB(dtdetail(1)(3), 0))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "rmstatussebelumnya" : jnsaktivitas = 17
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

            ' ''SIMPAN HISTORY ========================
            ''Dim SimpanHistory As New m5_so_history
            ''Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            ''Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            ''Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ' ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            ''If (rsSplitResult(1) = 0) Then
            ''    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            ''End If
            ' ''END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                'sql = query.m11_kj_terkait("rmid = '" & idtransaksi & "'")

                sql = query.PanggilQuery("m11_rm_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)

                'BUKA KONEKSI
                myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                myConn.Open()

                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'UPDATE STATUS KJ ===============================================================
                'CEK TRANSAKSI TERKAIT KJ
                sql = "  SELECT * FROM ( "
                sql &= " SELECT a.akid as idterkait, a.aksumber as sumber FROM m_11_ak a JOIN m_11_kj kj ON a.akidkj = kj.kjid AND a.akstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kmid as idterkait, a.kmsumber as sumber FROM m_11_km a JOIN m_11_kj kj ON a.kmidkj = kj.kjid AND a.kmstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kwid as idterkait, a.kwsumber as sumber FROM m_11_kw a JOIN m_11_kw_detail b ON a.kwid = b.idkw AND a.kwstatus IN(2,3,4,7) JOIN m_11_kj kj ON b.sumber = 'KJ' AND b.idtransaksi = kj.kjid AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.lbid as idterkait, a.lbsumber as sumber FROM m_11_lb a JOIN m_11_kj kj ON a.lbidkj = kj.kjid AND a.lbstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.luid as idterkait, a.lusumber as sumber FROM m_11_lu a JOIN m_11_kj kj ON a.luidkj = kj.kjid AND a.lustatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rkid as idterkait, a.rksumber as sumber FROM m_11_rk a JOIN m_11_kj kj ON a.rkidkj = kj.kjid AND a.rkstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rmid as idterkait, 'RM' as sumber FROM m_11_rm a JOIN m_11_kj kj ON a.rmidkj = kj.kjid AND a.rmstatus IN(2,3,4,7) AND a.rmid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.roid as idterkait, a.rosumber as sumber FROM m_11_ro a JOIN m_11_kj kj ON a.roidkj = kj.kjid AND a.rostatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " ) as terkait "
                sql &= " ORDER BY terkait.sumber = 'KW' DESC, terkait.sumber ASC "
                dtdetail = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtdetail.Rows.Count > 0 Then
                    'JIKA KJ MEMILIKI TRANSAKSI TERKAIT
                    If FxDB(dtdetail.Rows(0)("sumber"), "").ToUpper.Equals("KW") Then
                        'JIKA ADA KJ TERKAIT KW MAKA STATUS KJ = 4 (COMPLETE)
                        sql = "UPDATE M_11_Kj SET kjstatus = 4 WHERE kjid = '" & FixDouble(idkj) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        'JIKA ADA KJ TERKAIT SELAIN KW MAKA STATUS KJ = 3 (INPROGRESS)
                        sql = "UPDATE M_11_Kj SET kjstatus = 3 WHERE kjid = '" & FixDouble(idkj) & "'"
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
                    'JIKA KJ TIDAK MEMILIKI TRANSAKSI TERKAIT, STATUS KJ = 2 (APPROVED)
                    sql = "UPDATE M_11_Kj SET kjstatus = 2 WHERE kjid = '" & FixDouble(idkj) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                End If
                'END OF UPDATE STATUS KJ ========================================================

                'Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
                'Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

                ''AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idsqdetail, urutan FROM m5_so_detail WHERE idso = '" & idtransaksi & "'")
                'If dtdetail.Rows.Count > 0 Then
                '    For Each dr1 As DataRow In dtdetail.Rows
                '        'BUAT FILTER UNTUK UPDATE ---------------------------------
                '        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idsqdetail = dr1("idsqdetail")

                '        'UPDATE OUTSTANDING ---------------------------
                '        If idsqdetail <> 0 Then
                '            '1. SET NILAI UPDATE OUTSTANDING
                '            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                '            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN jmlrealisasi - '" & Outstanding & "' ", updNilai)

                '            '2. SET FILTERUPDATE OUTSTANDING
                '            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                '            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
                '        End If

                '        ''3. SET NILAI UPDATE STOK KELUAR -------------
                '        'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                '        'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                '        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                '    Next
                'Else
                '    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                'End If

                'If Len(updFilter) > 0 Then
                '    'UPDATE OUTSTANDING DETAIL ----------------------
                '    sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'END OF UPDATE OUTSTANDING DETAIL ---------------

                '    'UPDATE OUTSTANDING UTAMA -----------------------
                '    Dim ftDetail As String = "", statusOut As Integer = 0
                '    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq")
                '    If dtOut.Rows.Count > 0 Then
                '        For Each dr1 As DataRow In dtOut.Rows
                '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                '            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                '        Next
                '    End If
                '    dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
                '    If dtOut.Rows.Count > 0 Then
                '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                '        updNilai = "" : updFilter = ""
                '        For Each dr1 As DataRow In dtOut.Rows
                '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                '                statusOut = 2
                '            ElseIf dr1("jmlrealisasi") < 1 Then
                '                statusOut = 0
                '            Else
                '                statusOut = 1
                '            End If
                '            '2. SET NILAI UPDATE OUTSTANDING
                '            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                '            '3. SET FILTERUPDATE OUTSTANDING
                '            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                '            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                '        Next

                '        sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()
                '    End If
                '    'END OF UPDATE OUTSTANDING UTAMA ----------------
                'End If

                'UPDATE STOK BOOKING ================================
                'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                'sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang * -1 FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()

                'If Len(updStokBooking) > 0 Then
                '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                'END OF UPDATE STOK BOOKING =========================

            End If


            'JIKA CLOSE MAKA KURANGI STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            'If jnsaktivitas = 7 Then
            '    'KURANGI STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
            '    sql = "  UPDATE m1_item_booking ib"
            '    sql &= " JOIN"
            '    sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
            '    sql &= " FROM ( "
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_do `do` "
            '    sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " UNION ALL"
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_si si "
            '    sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " ) as detail"
            '    sql &= " GROUP BY idsodetail"
            '    sql &= " ) sod  ON ib.idbarang = sod.idbarang"
            '    sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
            '    sql &= " SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.realisasi)"
            '    sql &= " WHERE sod.jmlbarang <> sod.realisasi"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myconn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()

            '    'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
            '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    'With objCmd
            '    '    .Connection = myconn
            '    '    .Transaction = Trans
            '    '    .CommandType = CommandType.Text
            '    '    .CommandText = sql
            '    'End With
            '    'objCmd.ExecuteNonQuery()
            'End If

            'JIKA UNCLOSE MAKA TAMBAH STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            'If jnsaktivitas = 17 Then
            '    'TAMBAH STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
            '    sql = "  UPDATE m1_item_booking ib"
            '    sql &= " JOIN"
            '    sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
            '    sql &= " FROM ( "
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_do `do` "
            '    sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " UNION ALL"
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_si si "
            '    sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " ) as detail"
            '    sql &= " GROUP BY idsodetail"
            '    sql &= " ) sod  ON ib.idbarang = sod.idbarang"
            '    sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
            '    sql &= " SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.realisasi)"
            '    sql &= " WHERE sod.jmlbarang <> sod.realisasi"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myconn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()

            '    'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
            '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    'With objCmd
            '    '    .Connection = myconn
            '    '    .Transaction = Trans
            '    '    .CommandType = CommandType.Text
            '    '    .CommandText = sql
            '    'End With
            '    'objCmd.ExecuteNonQuery()
            'End If

            'update status utama
            sql = "UPDATE M_11_rm SET rmstatus = " & nilaiStatus & ", rmmodifikasiuser='" & userid & "', rmmodifikasitgl = NOW(), rmjmlrevisi = rmjmlrevisi + 1 WHERE rmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_RmSearch(PostWsSearch(paramSplit(0), "M11_RmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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
    Public Function M11_RmDelete(ByVal param As String) As String

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
            Dim sumber As String = "RM", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT rmid, rmnotransaksi FROM m_11_rm WHERE rmid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rmnotransaksi, rmtgl"
            sql &= " FROM m_11_rm"
            sql &= " WHERE rmid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                notransaksi = dtNomorNext.Rows(0)("rmnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rmtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE UTAMA
            sql = "DELETE FROM m_11_rm WHERE rmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_RmSearch(PostWsSearch(paramSplit(0), "M11_RmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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
    Public Function M11_RmGetdataById(ByVal param As String) As String
        'M11_Rm_GetdataById Utama --------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20


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

        Dim utama As String = "", detail As String = "", akdetail As String = "", ludetail As String = "", kmutama As String = "", lbutama As String = "", rkutama As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M11_Rm-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rmid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rmid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_rm_getdata")

        dt = AmbilData("aplikasi1-M11_rm_getdata", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'result(2) = idtransaksi & "  " & Filter & " jml dt: " & dt.Rows.Count.ToString : GoTo selesai

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rmid"), 0), sptField,
                     FxDB(drutama("rmidkj"), 0), sptField,
                     FxDB(drutama("rmnorm"), ""), sptField,
                     FxDB(drutama("rmperawatan"), ""), sptField,
                     FxDB(drutama("rmkategoripasien"), ""), sptField,
                     FxDB(drutama("rmlayanan"), ""), sptField,
                     FxDB(drutama("rmdokter"), ""), sptField,
                     FxDB(drutama("rmkecelakaan"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rmnotransaksi"), ""), sptField,
                     FxDB(drutama("rmkasus"), 0), sptField,
                     FxDB(drutama("rmicd"), ""), sptField,
                     FxDB(drutama("rmtindaklanjut"), 0), sptField,
                     FxDB(drutama("rmkrs"), 0), sptField,
                     FxDB(drutama("rmcarakrs"), 0), sptField,
                     FxDB(drutama("rmstatus"), 0), sptField,
                     FxDB(drutama("rmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rmjmlrevisi"), 0), sptField,
                     FxDB(drutama("rmcetakanke"), 0), sptField,
                     FxDB(drutama("rminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmisclose"), 0), sptField,
                     FxDB(drutama("rmjmlrawat"), 0), sptField,
                     FxDB(drutama("rmnotransaksikj"), ""), sptField,
                     FxDB(drutama("rmnama"), ""), sptField,
                     FxDB(drutama("rmawalannotran"), ""), sptField,
                     FxDB(drutama("rmkategoripasiennama"), ""), sptField,
                     FxDB(drutama("rmlayanannama"), ""), sptField,
                     FxDB(drutama("rmdokternama"), ""), sptField,
                     FxDB(drutama("rmstatusnama"), ""), sptField,
                     FxDB(drutama("rmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rminputusernama"), ""), sptField,
                     FxDB(drutama("rmmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("rmkecelakaannama"), ""), sptField,
                     FxDB(drutama("rmicdnama"), ""), sptField,
                     FxDB(drutama("rmstatusimunisasi"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmtgllahir"), ""), formatTgl), sptField,
                     FxDB(drutama("rmumur"), 0), sptField,
                     FxDB(drutama("rmketumur"), ""), sptField,
                     FxDB(drutama("rmrujukan"), 0), sptField,
                     FxDB(drutama("rmrujukandetail"), 0), sptField,
                     FxDB(drutama("rmrehabmedik"), ""), sptField,
                     FxDB(drutama("rmhamilke"), 0), sptField,
                     FxDB(drutama("rmpersalinan"), 0), sptField,
                     FxDB(drutama("rmkeadaanbayi"), 0), sptField,
                     FxDB(drutama("rmjeniskelamin"), ""), sptField,
                     FxDB(drutama("rmpanjang"), 0), sptField,
                     FxDB(drutama("rmberat"), 0), sptField,
                     FxDB(drutama("rmketerangan"), ""), sptField,
                     FxDB(drutama("rmicd10"), ""), sptField,
                     FxDB(drutama("rmicd10nama"), ""), sptField,
                     FxDB(drutama("rmdokumen"), 0), sptField,
                     FxDB(drutama("rmtpip11"), 0), sptField,
                     FxDB(drutama("rmtpip12"), 0), sptField,
                     FxDB(drutama("rmtpip13"), 0), sptField,
                     FxDB(drutama("rmtpip4"), 0), sptField,
                     FxDB(drutama("rmtpip5"), 0), sptField,
                     FxDB(drutama("rmigd21"), 0), sptField,
                     FxDB(drutama("rmigd22"), 0), sptField,
                     FxDB(drutama("rmigd18a"), 0), sptField,
                     FxDB(drutama("rmigd31"), 0), sptField,
                     FxDB(drutama("rmigd32"), 0), sptField,
                     FxDB(drutama("rmigd33"), 0), sptField,
                     FxDB(drutama("rmigd34"), 0), sptField,
                     FxDB(drutama("rmigd35"), 0), sptField,
                     FxDB(drutama("rmigd6"), 0), sptField,
                     FxDB(drutama("rmigd7"), 0), sptField,
                     FxDB(drutama("rmvk10"), 0), sptField,
                     FxDB(drutama("rmvk10b"), 0), sptField,
                     FxDB(drutama("rmvk22bayi"), 0), sptField,
                     FxDB(drutama("rmrawat36"), 0), sptField,
                     FxDB(drutama("rmrawat37"), 0), sptField,
                     FxDB(drutama("rmrawat38"), 0), sptField,
                     FxDB(drutama("rmrawat9"), 0), sptField,
                     FxDB(drutama("rmrawat10"), 0), sptField,
                     FxDB(drutama("rmrawat14"), 0), sptField,
                     FxDB(drutama("rmrawat15"), 0), sptField,
                     FxDB(drutama("rmrawat16"), 0), sptField,
                     FxDB(drutama("rmrawat20"), 0), sptField,
                     FxDB(drutama("rmrawat21a"), 0), sptField,
                     FxDB(drutama("rmrawat21b"), 0), sptField,
                     FxDB(drutama("rmrawat22"), 0), sptField,
                     FxDB(drutama("rmfp16oral"), 0), sptField,
                     FxDB(drutama("rmgizi17"), 0), sptField,
                     FxDB(drutama("rmoklapanastesi"), 0), sptField,
                     FxDB(drutama("rmok19"), 0), sptField,
                     FxDB(drutama("rmpetugas"), 0), sptField,
                     FxDB(drutama("rmpetugaskode"), ""), sptField,
                     FxDB(drutama("rmpetugasnama"), ""), sptField,
                     FxDB(drutama("rmalasan"), 0), sptField,
                     FxDB(drutama("rmrawat18"), 0), sptField,
                     FxDB(drutama("rmok18"), 0), sptField,
                     FxDB(drutama("rmdiagnosa"), ""), sptField,
                     FxDB(drutama("rmdiagnosanama"), ""), sptField,
                     FxDB(drutama("rmrujukankode"), ""), sptField,
                     FxDB(drutama("rmrujukannama"), ""), sptField,
                     FxDB(drutama("rmcatatandiagnosa"), ""), sptField,
                     FxDB(drutama("rmlokasidokumen"), ""), sptRow)

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
        'strResultData = String.Concat(utama, sptSubParam, luutama, sptSubParam, ludetail)
        strResultData = String.Concat(utama)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        'wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien, kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5,kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20,kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5,kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15,kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5,kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20,kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20, kjstatusnama, kjstatussebelumnyanama, kjinputusernama, kjmodifikasiusernama, kjcabangnama, kjlokasinama" & sptSubParam & "luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8, lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13, lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18, lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3, lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8, lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13, lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18, lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8, lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13, lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18, lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8, lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13, lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18, lucustomdate19, lucustomdate20, lucabangnama, lulokasinama, lugudangnama,  lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama" & sptSubParam & "idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter"))
        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmid, rmidkj, rmnorm, rmperawatan, rmkategoripasien, rmlayanan, rmdokter, rmkecelakaan, rmtgl, rmnotransaksi, rmkasus, rmicd, rmtindaklanjut, rmkrs, rmcarakrs, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmisclose, rmjmlrawat, rmnotransaksikj, rmnama, rmawalannotran, rmkategoripasiennama, rmlayanannama, rmdokternama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, rmmodifikasiusernama, rmkecelakaannama, rmicdnama, rmstatusimunisasi, rmtgllahir, rmumur, rmketumur, rmrujukan, rmrujukandetail, rmrehabmedik, rmhamilke, rmpersalinan, rmkeadaanbayi, rmjeniskelamin, rmpanjang, rmberat, rmketerangan, rmicd10, rmicd10nama, rmdokumen, rmtpip11, rmtpip12, rmtpip13, rmtpip4, rmtpip5, rmigd21, rmigd22, rmigd18a, rmigd31, rmigd32, rmigd33, rmigd34, rmigd35, rmigd6, rmigd7, rmvk10, rmvk10b, rmvk22bayi, rmrawat36, rmrawat37, rmrawat38, rmrawat9, rmrawat10, rmrawat14, rmrawat15, rmrawat16, rmrawat20, rmrawat21a, rmrawat21b, rmrawat22, rmfp16oral, rmgizi17, rmoklapanastesi, rmok19, rmpetugas, rmpetugaskode, rmpetugasnama, rmalasan, rmrawat18, rmok18, rmdiagnosa, rmdiagnosanama, rmrujukankode, rmrujukannama, rmcatatandiagnosa, rmlokasidokumen"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RmSearch(ByVal param As String) As String
        'M11_RmSearch --------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20

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
            formatTglWaktu = "yyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            Filter = Filter.Replace("rmnorm", "pat.pkode")
            Filter = Filter.Replace("rmnama", "pat.pnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_rm_v")

        dt = AmbilData("aplikasi1-m11_rm_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "rmid", sql) ' Ambil data ke databases
        pg1 = pg1
        'Dim hitung As Int16 = 0
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'hitung = hitung + 1
                search = String.Concat(search,
                     FxDB(dr("rmid"), 0), sptField,
                     FxDB(dr("rmidkj"), 0), sptField,
                     FxDB(dr("rmnorm"), ""), sptField,
                     FxDB(dr("rmperawatan"), ""), sptField,
                     FxDB(dr("rmkategoripasien"), ""), sptField,
                     FxDB(dr("rmlayanan"), ""), sptField,
                     FxDB(dr("rmdokter"), ""), sptField,
                     FxDB(dr("rmkecelakaan"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rmnotransaksi"), ""), sptField,
                     FxDB(dr("rmkasus"), 0), sptField,
                     FxDB(dr("rmicd"), ""), sptField,
                     FxDB(dr("rmtindaklanjut"), 0), sptField,
                     FxDB(dr("rmkrs"), 0), sptField,
                     FxDB(dr("rmcarakrs"), 0), sptField,
                     FxDB(dr("rmstatus"), 0), sptField,
                     FxDB(dr("rmstatussebelumnya"), 0), sptField,
                     FxDB(dr("rmjmlrevisi"), 0), sptField,
                     FxDB(dr("rmcetakanke"), 0), sptField,
                     FxDB(dr("rminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmisclose"), 0), sptField,
                     FxDB(dr("rmjmlrawat"), 0), sptField,
                     FxDB(dr("rmnama"), ""), sptField,
                     FxDB(dr("rmawalannotran"), ""), sptField,
                     FxDB(dr("rmkategoripasiennama"), ""), sptField,
                     FxDB(dr("rmlayanannama"), ""), sptField,
                     FxDB(dr("rmdokternama"), ""), sptField,
                     FxDB(dr("rmstatusnama"), ""), sptField,
                     FxDB(dr("rmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rminputusernama"), ""), sptField,
                     FxDB(dr("rmmodifikasiusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rmtglkeluar"), ""), formatTgl), sptField,
      AsFormatTanggal(FxDB(dr("rmtglmasuk"), ""), formatTgl), sptField,
                     FxDB(dr("rmkamar"), ""), sptField,
      FxDB(dr("rmicd10nama"), ""), sptField,
                     FxDB(dr("rmlokasidokumen"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            'result(2) = search : GoTo selesai
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmid, rmidkj, rmnorm, rmperawatan, rmkategoripasien, rmlayanan, rmdokter, rmkecelakaan, rmtgl, rmnotransaksi, rmkasus, rmicd, rmtindaklanjut, rmkrs, rmcarakrs, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmisclose, rmjmlrawat, rmnama, rmawalannotran, rmkategoripasiennama, rmlayanannama, rmdokternama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, rmmodifikasiusernama, rmtglkeluar, rmtglmasuk, rmkamar,rmicd10nama, rmlokasidokumen"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RmTerkait(ByVal param As String) As String
        'M5_SoTerkait --------------------------------------------------------
        'soid, sonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rmid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            'Else
            '    Filter = "rmid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_rm_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_rm_terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rmid"), 0), sptField,
                     FxDB(dr("rmnotransaksi"), ""), sptField,
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
            result(2) = "Related RM data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmid, rmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idsqdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SQ" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT sqd.idsqdetail, (sqd.jmlbarang - sqd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid WHERE " & ftOutstanding
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SQ, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

End Class