Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_pt
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_PtSimpan(ByVal param As String) As String
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
        If (dataUtama.Length <> 32) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kjid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ptid required numeric." : GoTo selesai
        End If
        'kjautonotransaksi(2) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ptautonotransaksi required numeric." : GoTo selesai
        End If
        'kjtgl(4) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "pttgl required date." : GoTo selesai
        End If
        'kjstatusrealisasi(12) As Interger
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "ptstatusrealisasi required numeric." : GoTo selesai
        End If
        'kjstatus(13) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ptstatus required numeric." : GoTo selesai
        End If
        'kjstatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "ptstatussebelumnya required numeric." : GoTo selesai
        End If
        'kjjmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "ptjmlrevisi required numeric." : GoTo selesai
        End If
        'kjcetakanke(16) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "ptcetakanke required numeric." : GoTo selesai
        End If
        'kjinputuser(17) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "ptinputuser required numeric." : GoTo selesai
        End If
        'kjinputtgl(18) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "ptinputtgl required date." : GoTo selesai
        End If
        'kjmodifikasiuser(19) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "ptmodifikasiuser required numeric." : GoTo selesai
        End If
        'kjmodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "ptmodifikasitgl required date." : GoTo selesai
        End If
        'kjisclose(21) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "ptisclose required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ptcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 10 Then
            result(2) = "ptcabang should not be more than 10 character." : GoTo selesai
        End If

        If Len(dataUtama(2)) = 0 Then
            result(2) = "ptlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 10 Then
            result(2) = "ptlokasi should not be more than 10 character." : GoTo selesai
        End If

        'kjsumber(1) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "ptsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "ptsumber should not be more than 10 character." : GoTo selesai
        End If

        'kjnotransaksi(3) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "ptnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "ptnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kjtgl(4) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pttgl can't be empty" : GoTo selesai
        End If

        'kjinputtgl(18) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "ptinputtgl can't be empty" : GoTo selesai
        End If

        'kjmodifikasitgl(20) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ptmodifikasitgl can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ptid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pttgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptjenispemasangan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptpemasanganharike", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptjeniscairan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptjenisjarum", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptobatobatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptsuhutubuh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptrasapanas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptbengkak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptkemerahan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptkeluarpus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptleukositosis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptkultur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptpetugas", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "ptid~ptcabang~ptlokasi~ptsumber~ptautonotransaksi~ptnotransaksi~pttgl~ptidkj~ptjenispemasangan~ptpemasanganharike~ptjeniscairan~ptjenisjarum~ptobatobatan~ptsuhutubuh~ptrasapanas~ptbengkak~ptkemerahan~ptkeluarpus~ptleukositosis~ptkultur~ptcatatan~ptstatusrealisasi~ptstatus~ptstatussebelumnya~ptjmlrevisi~ptcetakanke~ptinputuser~ptinputtgl~ptmodifikasiuser~ptmodifikasitgl~ptisclose~ptpetugas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31)) = False Then
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

                If isUpdate Then
                    result(4) = drutama("ptid")
                    notransaksi = drutama("ptnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(ptid), ptnotransaksi FROM M_11_pt WHERE ptid='" & result(4) & "' AND ptstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ptid) FROM M_11_pt WHERE ptnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m11_kj_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M11_Kj_HistorySimpan("" & paramSplit(0) & "★M11_Kj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("kjsumber")) & "▼" & FixQuotes(drutama("kjid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update m_11_pt set ptcabang  = '" & FixQuotes(drutama("ptcabang")) & "', ptlokasi  = '" & FixQuotes(drutama("ptlokasi")) & "', ptsumber  = '" & FixQuotes(drutama("ptsumber")) & "', ptautonotransaksi  = '" & FixQuotes(drutama("ptautonotransaksi")) & "', ptnotransaksi  = '" & FixQuotes(drutama("ptnotransaksi")) & "', pttgl  = '" & FixQuotes(AsFormatTanggal(drutama("pttgl"))) & "', ptidkj = " & drutama("ptidkj") & " , ptjenispemasangan = " & drutama("ptjenispemasangan") & " , ptpemasanganharike = " & drutama("ptpemasanganharike") & " , ptjeniscairan = " & drutama("ptjeniscairan") & " ,ptjenisjarum = " & ("ptjenisjarum") & ", ptobatobatan = '" & FixQuotes(drutama("ptobatobatan")) & "',ptsuhutubuh = " & drutama("ptsuhutubuh") & ", ptrasapanas = " & drutama("ptrasapanas") & ", ptbengkak = " & drutama("ptbengkak") & ", ptkemerahan = " & drutama("ptkemerahan") & ", ptkeluarpus = " & drutama("ptkeluarpus") & ", ptleukositosis = '" & FixQuotes(drutama("ptleukositosis")) & "' , ptkultur = '" & FixQuotes(drutama("ptkultur")) & "' , ptcatatan = '" & FixQuotes(drutama("ptcatatan")) & "' , ptstatusrealisasi  = " & drutama("ptstatusrealisasi") & ", ptstatus  = " & drutama("ptstatus") & ", ptstatussebelumnya  = " & drutama("ptstatussebelumnya") & ", ptjmlrevisi = ptjmlrevisi+1, ptcetakanke  = " & drutama("ptcetakanke") & ", ptmodifikasiuser  = " & drutama("ptmodifikasiuser") & ", ptmodifikasitgl  = NOW(), ptpetugas = " & drutama("ptpetugas") & " where ptid = '" & drutama("ptid") & "'"
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

                    'Dim dtCekNoRM As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kjid), kjnopasien, kjnotransaksi FROM m_11_kj WHERE kjperawatan = 'RI' AND kjnopasien = '" & FixQuotes(drutama("kjnopasien")) & "' AND kjtgl = '" & drutama("kjtgl") & "'")
                    'Dim cekNoRM As Double = Val(dtCekNoRM.Rows(0)(0))
                    'If cekNoRM > 0 Then
                    '    result(2) = "Kunjungan pasien '" & dtCekNoRM.Rows(0)(1) & "' sudah dibuat di nomor '" & dtCekNoRM.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                    'End If

                    If drutama("ptautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ptcabang"), drutama("ptlokasi"), drutama("ptsumber"), drutama("pttgl"))
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
                        notransaksi = drutama("ptnotransaksi")
                    End If
                    'result(2) = notransaksi + " " + userid + " Dtdetail : " + dtdetail.Rows.Count.ToString : GoTo selesai
                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(ptid) FROM m_11_pt WHERE ptnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============
                    sql = "Insert into m_11_pt (ptcabang, ptlokasi, ptsumber, ptautonotransaksi, ptnotransaksi, pttgl, ptidkj, ptjenispemasangan, ptpemasanganharike, ptjeniscairan, ptjenisjarum, ptobatobatan, ptsuhutubuh, ptrasapanas, ptbengkak, ptkemerahan, ptkeluarpus, ptleukositosis, ptkultur, ptcatatan, ptstatus, ptstatussebelumnya, ptjmlrevisi, ptcetakanke, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptisclose, ptpetugas) values('" & FixQuotes(drutama("ptcabang")) & "','" & FixQuotes(drutama("ptlokasi")) & "','" & FixQuotes(drutama("ptsumber")) & "', " & drutama("ptautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pttgl"))) & "', " & drutama("ptidkj") & ", " & drutama("ptjenispemasangan") & ", " & drutama("ptpemasanganharike") & ", " & drutama("ptjeniscairan") & ", " & drutama("ptjenisjarum") & ", '" & FixQuotes(drutama("ptobatobatan")) & "', " & drutama("ptsuhutubuh") & ", " & drutama("ptrasapanas") & ", " & drutama("ptbengkak") & ", " & drutama("ptkemerahan") & ", " & drutama("ptkeluarpus") & ", '" & FixQuotes(drutama("ptleukositosis")) & "', '" & FixQuotes(drutama("ptkultur")) & "', '" & FixQuotes(drutama("ptcatatan")) & "', " & drutama("ptstatus") & ", " & drutama("ptstatussebelumnya") & ", " & drutama("ptjmlrevisi") & ", " & drutama("ptcetakanke") & ", " & drutama("ptinputuser") & ", NOW(), " & drutama("ptmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("ptisclose") & ", " & drutama("ptpetugas") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select ptid from m_11_pt where ptnotransaksi='" & notransaksi & "' AND ptinputuser= '" & userid & "' order by ptmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If
                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PT", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M11_PtUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "PT", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT pttgl, ptnotransaksi, ptstatus FROM m_11_pt WHERE ptid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "ptstatussebelumnya" : jnsaktivitas = 17
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

            'If isDelete Then

            '    'CEK TERKAIT ====================================================================
            '    'PANGGIL QUERY TERKAIT
            '    Dim query As New m0_query
            '    'sql = query.m11_kj_terkait("kjid = '" & idtransaksi & "'")
            '    sql = query.PanggilQuery("m11_pt_terkait")
            '    sql = sql.Replace("validtransaksi", idtransaksi)

            '    myconn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
            '    myconn.Open()

            '    Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql)
            '    dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
            '    If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
            '    'END OF CEK TERKAIT =============================================================

            '    Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
            '    Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

            'End If

            'update status utama
            sql = "UPDATE M_11_pt SET ptstatus = " & nilaiStatus & ", ptmodifikasiuser='" & userid & "', ptmodifikasitgl = NOW(), ptjmlrevisi = ptjmlrevisi + 1 WHERE ptid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_PtSearch(PostWsSearch(paramSplit(0), "M11_PtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_PtDelete(ByVal param As String) As String

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
            Dim sumber As String = "PT", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT ptid, ptnotransaksi FROM m_11_pt WHERE ptid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ptcabang, ptlokasi, ptsumber, ptautonotransaksi, ptnotransaksi, pttgl"
            sql &= " FROM m_11_pt"
            sql &= " WHERE ptid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ptcabang")
                lokasi = dtNomorNext.Rows(0)("ptlokasi")
                sumber = dtNomorNext.Rows(0)("ptsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("ptautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ptnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pttgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE UTAMA
            sql = "DELETE FROM m_11_pt WHERE ptid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_PtSearch(PostWsSearch(paramSplit(0), "M11_PtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M11_PtGetdataById(ByVal param As String) As String
        'M11_Kj_GetdataById Utama --------------------------------------------------------
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

        Dim utama As String = "", detail As String = "", akdetail As String = "", ludetail As String = "", kmutama As String = "", lbutama As String = "", rkutama As String = "", rodetail As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M11_Pt-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ptid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "ptid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_pt_getdata")

        dt = AmbilData("aplikasi1-M11_pt_getdata", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'result(2) = idtransaksi & "  " & Filter & " jml dt: " & dt.Rows.Count.ToString : GoTo selesai

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("ptid"), 0), sptField,
                     FxDB(drutama("ptcabang"), ""), sptField,
                     FxDB(drutama("ptlokasi"), ""), sptField,
                     FxDB(drutama("ptsumber"), ""), sptField,
                     FxDB(drutama("ptautonotransaksi"), 0), sptField,
                     FxDB(drutama("ptnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pttgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ptidkj"), 0), sptField,
                     FxDB(drutama("ptjenispemasangan"), 0), sptField,
                     FxDB(drutama("ptpemasanganharike"), 0), sptField,
                     FxDB(drutama("ptjeniscairan"), 0), sptField,
                     FxDB(drutama("ptjenisjarum"), 0), sptField,
                     FxDB(drutama("ptobatobatan"), ""), sptField,
                     FxDB(drutama("ptsuhutubuh"), 0), sptField,
                     FxDB(drutama("ptrasapanas"), 0), sptField,
                     FxDB(drutama("ptbengkak"), 0), sptField,
                     FxDB(drutama("ptkemerahan"), 0), sptField,
                     FxDB(drutama("ptkeluarpus"), 0), sptField,
                     FxDB(drutama("ptleukositosis"), ""), sptField,
                     FxDB(drutama("ptkultur"), ""), sptField,
                     FxDB(drutama("ptcatatan"), ""), sptField,
                     FxDB(drutama("ptstatusrealisasi"), 0), sptField,
                     FxDB(drutama("ptstatus"), 0), sptField,
                     FxDB(drutama("ptstatussebelumnya"), 0), sptField,
                     FxDB(drutama("ptjmlrevisi"), 0), sptField,
                     FxDB(drutama("ptcetakanke"), 0), sptField,
                     FxDB(drutama("ptinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ptinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ptmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ptmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ptisclose"), 0), sptField,
                     FxDB(drutama("ptpetugas"), 0), sptField,
                     FxDB(drutama("ptpetugaskode"), ""), sptField,
                     FxDB(drutama("ptpetugasnama"), ""), sptField,
                     FxDB(drutama("ptnotransaksikj"), ""), sptField,
                     FxDB(drutama("ptnama"), ""), sptField,
                     FxDB(drutama("ptumur"), 0), sptField,
                     FxDB(drutama("ptjeniskelamin"), ""), sptField,
                     FxDB(drutama("ptalamat"), ""), sptRow)

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
        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptid, ptcabang, ptlokasi, ptsumber, ptautonotransaksi, ptnotransaksi, pttgl, ptidkj, ptjenispemasangan, ptpemasanganharike, ptjeniscairan, ptjenisjarum, ptobatobatan, ptsuhutubuh, ptrasapanas, ptbengkak, ptkemerahan, ptkeluarpus, ptleukositosis, ptkultur, ptcatatan, ptstatusrealisasi, ptstatus, ptstatussebelumnya, ptjmlrevisi, ptcetakanke, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptisclose, ptpetugas, ptpetugaskode, ptpetugasnama, ptnotransaksikj, ptnama, ptumur, ptjeniskelamin, ptalamat"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_PtSearch(ByVal param As String) As String
        'M11_KjSearch --------------------------------------------------------
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
            Filter = Filter.Replace("ptnama", "kj.kjnama")
            Filter = Filter.Replace("ptalamat", "kj.kjalamat")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_pt_v")
        'result(2) = sql & " WHERE " & Filter & " ORDER BY " & Sorting : GoTo selesai
        dt = AmbilData("aplikasi1-M11_pt_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        'Dim hitung As Int16 = 0
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'hitung = hitung + 1
                search = String.Concat(search,
                     FxDB(dr("ptid"), 0), sptField,
                     FxDB(dr("ptcabang"), ""), sptField,
                     FxDB(dr("ptlokasi"), ""), sptField,
                     FxDB(dr("ptsumber"), ""), sptField,
                     FxDB(dr("ptautonotransaksi"), 0), sptField,
                     FxDB(dr("ptnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pttgl"), ""), formatTgl), sptField,
                     FxDB(dr("ptidkj"), 0), sptField,
                     FxDB(dr("ptjenispemasangan"), 0), sptField,
                     FxDB(dr("ptpemasanganharike"), 0), sptField,
                     FxDB(dr("ptjeniscairan"), 0), sptField,
                     FxDB(dr("ptjenisjarum"), 0), sptField,
                     FxDB(dr("ptobatobatan"), ""), sptField,
                     FxDB(dr("ptsuhutubuh"), 0), sptField,
                     FxDB(dr("ptrasapanas"), 0), sptField,
                     FxDB(dr("ptbengkak"), 0), sptField,
                     FxDB(dr("ptkemerahan"), 0), sptField,
                     FxDB(dr("ptkeluarpus"), 0), sptField,
                     FxDB(dr("ptleukositosis"), ""), sptField,
                     FxDB(dr("ptkultur"), ""), sptField,
                     FxDB(dr("ptcatatan"), ""), sptField,
                     FxDB(dr("ptstatusrealisasi"), 0), sptField,
                     FxDB(dr("ptstatus"), 0), sptField,
                     FxDB(dr("ptstatussebelumnya"), 0), sptField,
                     FxDB(dr("ptjmlrevisi"), 0), sptField,
                     FxDB(dr("ptcetakanke"), 0), sptField,
                     FxDB(dr("ptinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ptmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ptisclose"), 0), sptField,
                     FxDB(dr("ptpetugas"), 0), sptField,
                     FxDB(dr("ptpetugaskode"), ""), sptField,
                     FxDB(dr("ptpetugasnama"), ""), sptField,
                     FxDB(dr("ptnotransaksikj"), ""), sptField,
                     FxDB(dr("ptnama"), ""), sptField,
                     FxDB(dr("ptumur"), 0), sptField,
                     FxDB(dr("ptjeniskelamin"), ""), sptField,
                     FxDB(dr("ptalamat"), ""), sptField,
                     FxDB(dr("ptstatusnama"), ""), sptField,
                     FxDB(dr("ptstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("ptinputusernama"), ""), sptField,
                     FxDB(dr("ptmodifikasiusernama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptid, ptcabang, ptlokasi, ptsumber, ptautonotransaksi, ptnotransaksi, pttgl, ptidkj, ptjenispemasangan, ptpemasanganharike, ptjeniscairan, ptjenisjarum, ptobatobatan, ptsuhutubuh, ptrasapanas, ptbengkak, ptkemerahan, ptkeluarpus, ptleukositosis, ptkultur, ptcatatan, ptstatusrealisasi, ptstatus, ptstatussebelumnya, ptjmlrevisi, ptcetakanke, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptisclose, ptpetugas, ptpetugaskode, ptpetugasnama, ptnotransaksikj, ptnama, ptumur, ptjeniskelamin, ptalamat, ptstatusnama, ptstatussebelumnyanama, ptinputusernama, ptmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_PtTerkait(ByVal param As String) As String
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
            result(2) = "kjid required numeric." : GoTo selesai
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
            'Filter = pagingSplit(2) & " AND kjid=" & idtransaksi
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            'Else
            '    Filter = "kjid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m11_kj_terkait(Filter)
        sql = query.PanggilQuery("m11_kj_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_kj_terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kjid"), 0), sptField,
                     FxDB(dr("kjnotransaksi"), ""), sptField,
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
            result(2) = "Related KJ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

End Class