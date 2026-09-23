Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_coa
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_CoaSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim pg1 As New RsPaging

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

        'MAPPING BUAT WS ----------------------------------------------------------
        'cid(0) As Integer, cnomor(1) As String, ctipe(2) As Integer, cdc(3) As String, curutan(4) As Integer, 
        'caktif(5) As Integer, cnama(6) As String, cnamaalias1(7) As String, cnamaalias2(8) As String, cnamaalias3(9) As String, 
        'cgd(10) As String, clevel(11) As Integer, csubdari(12) As Integer, cparent(13) As String, clevel1(14) As String, 
        'clevel2(15) As String, clevel3(16) As String, clevel4(17) As String, clevel5(18) As String, cjenisaruskas(19) As String, 
        'cbukupembantu(20) As Integer, ccabang(21) As String, clokasi(22) As String, cdivisi(23) As String, cmatauang(24) As String, 
        'ckodebank(25) As String, cnorekbank(26) As String, cjenis(27) As String, csaldoawal(28) As Double, csaldoberjalan(29) As Double, 
        'ccatatan(30) As String, cinputuser(31) As , cinputtgl(32) As DateTime, cmodifikasiuser(33) As , cmodifikasitgl(34) As DateTime, 
        'ccostcenter(35) As Integer, ccustomtext1(36) As String, ccustomtext2(37) As String, ccustomtext3(38) As String, ccustomtext4(39) As String, 
        'ccustomtext5(40) As String, ccustomtext6(41) As String, ccustomtext7(42) As String, ccustomtext8(43) As String, ccustomtext9(44) As String, 
        'ccustomtext10(45) As String, ccustomint1(46) As Integer, ccustomint2(47) As Integer, ccustomint3(48) As Integer, ccustomint4(49) As Integer, 
        'ccustomint5(50) As Integer, ccustomint6(51) As Integer, ccustomint7(52) As Integer, ccustomint8(53) As Integer, ccustomint9(54) As Integer, 
        'ccustomint10(55) As Integer, ccustomdbl1(56) As Double, ccustomdbl2(57) As Double, ccustomdbl3(58) As Double, ccustomdbl4(59) As Double, 
        'ccustomdbl5(60) As Double, ccustomdbl6(61) As Double, ccustomdbl7(62) As Double, ccustomdbl8(63) As Double, ccustomdbl9(64) As Double, 
        'ccustomdbl10(65) As Double, ccustomdate1(66) As Date, ccustomdate2(67) As Date, ccustomdate3(68) As Date, ccustomdate4(69) As Date, 
        'ccustomdate5(70) As Date, ccustomdate6(71) As Date, ccustomdate7(72) As Date, ccustomdate8(73) As Date, ccustomdate9(74) As Date, 
        'ccustomdate10(75) As Date


        'MAPPING BUAT FLEX --------------------------------------------------------
        'cid, cnomor, ctipe, cdc, curutan, caktif, cnama, 
        'cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, 
        'clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, 
        'ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, 
        'csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, 
        'ccostcenter, ccustomtext1, ccustomtext2, ccustomtext3, ccustomtext4, ccustomtext5, ccustomtext6, 
        'ccustomtext7, ccustomtext8, ccustomtext9, ccustomtext10, ccustomint1, ccustomint2, ccustomint3, 
        'ccustomint4, ccustomint5, ccustomint6, ccustomint7, ccustomint8, ccustomint9, ccustomint10, 
        'ccustomdbl1, ccustomdbl2, ccustomdbl3, ccustomdbl4, ccustomdbl5, ccustomdbl6, ccustomdbl7, 
        'ccustomdbl8, ccustomdbl9, ccustomdbl10, ccustomdate1, ccustomdate2, ccustomdate3, ccustomdate4, 
        'ccustomdate5, ccustomdate6, ccustomdate7, ccustomdate8, ccustomdate9, ccustomdate10


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 76) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'cid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "cid required numeric." : GoTo selesai
        End If
        'ctipe(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "ctipe required numeric." : GoTo selesai
        End If
        'curutan(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "curutan required numeric." : GoTo selesai
        End If
        'caktif(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "caktif required numeric." : GoTo selesai
        End If
        'clevel(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "clevel required numeric." : GoTo selesai
        End If
        'csubdari(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "csubdari required numeric." : GoTo selesai
        End If
        'cbukupembantu(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "cbukupembantu required numeric." : GoTo selesai
        End If
        'csaldoawal(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "csaldoawal required numeric." : GoTo selesai
        End If
        'csaldoberjalan(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "csaldoberjalan required numeric." : GoTo selesai
        End If
        'cinputuser(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "cinputuser required numeric." : GoTo selesai
        End If
        'cinputtgl(32) As DateTime
        If (IsDate(dataUtama(32)) = False) Then
            result(2) = "cinputtgl required date." : GoTo selesai
        End If
        'cmodifikasiuser(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "cmodifikasiuser required numeric." : GoTo selesai
        End If
        'cmodifikasitgl(34) As DateTime
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "cmodifikasitgl required date." : GoTo selesai
        End If
        'ccostcenter(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "ccostcenter required numeric." : GoTo selesai
        End If

        'ccustomint1(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "ccustomint1 required numeric." : GoTo selesai
        End If
        'ccustomint2(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "ccustomint2 required numeric." : GoTo selesai
        End If
        'ccustomint3(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "ccustomint3 required numeric." : GoTo selesai
        End If
        'ccustomint4(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "ccustomint4 required numeric." : GoTo selesai
        End If
        'ccustomint5(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "ccustomint5 required numeric." : GoTo selesai
        End If
        'ccustomint6(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "ccustomint6 required numeric." : GoTo selesai
        End If
        'ccustomint7(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "ccustomint7 required numeric." : GoTo selesai
        End If
        'ccustomint8(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "ccustomint8 required numeric." : GoTo selesai
        End If
        'ccustomint9(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "ccustomint9 required numeric." : GoTo selesai
        End If
        'ccustomint10(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "ccustomint10 required numeric." : GoTo selesai
        End If
        'ccustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "ccustomdbl1 required numeric." : GoTo selesai
        End If
        'ccustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "ccustomdbl2 required numeric." : GoTo selesai
        End If
        'ccustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "ccustomdbl3 required numeric." : GoTo selesai
        End If
        'ccustomdbl4(59) As Double
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "ccustomdbl4 required numeric." : GoTo selesai
        End If
        'ccustomdbl5(60) As Double
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "ccustomdbl5 required numeric." : GoTo selesai
        End If
        'ccustomdbl6(61) As Double
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "ccustomdbl6 required numeric." : GoTo selesai
        End If
        'ccustomdbl7(62) As Double
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "ccustomdbl7 required numeric." : GoTo selesai
        End If
        'ccustomdbl8(63) As Double
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "ccustomdbl8 required numeric." : GoTo selesai
        End If
        'ccustomdbl9(64) As Double
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "ccustomdbl9 required numeric." : GoTo selesai
        End If
        'ccustomdbl10(65) As Double
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "ccustomdbl10 required numeric." : GoTo selesai
        End If
        'ccustomdate1(66) As Date
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "ccustomdate1 required date." : GoTo selesai
        End If
        'ccustomdate2(67) As Date
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "ccustomdate2 required date." : GoTo selesai
        End If
        'ccustomdate3(68) As Date
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "ccustomdate3 required date." : GoTo selesai
        End If
        'ccustomdate4(69) As Date
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "ccustomdate4 required date." : GoTo selesai
        End If
        'ccustomdate5(70) As Date
        If (IsDate(dataUtama(70)) = False) Then
            result(2) = "ccustomdate5 required date." : GoTo selesai
        End If
        'ccustomdate6(71) As Date
        If (IsDate(dataUtama(71)) = False) Then
            result(2) = "ccustomdate6 required date." : GoTo selesai
        End If
        'ccustomdate7(72) As Date
        If (IsDate(dataUtama(72)) = False) Then
            result(2) = "ccustomdate7 required date." : GoTo selesai
        End If
        'ccustomdate8(73) As Date
        If (IsDate(dataUtama(73)) = False) Then
            result(2) = "ccustomdate8 required date." : GoTo selesai
        End If
        'ccustomdate9(74) As Date
        If (IsDate(dataUtama(74)) = False) Then
            result(2) = "ccustomdate9 required date." : GoTo selesai
        End If
        'ccustomdate10(75) As Date
        If (IsDate(dataUtama(75)) = False) Then
            result(2) = "ccustomdate10 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'cnomor(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cnomor can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cnomor should not be more than 25 character." : GoTo selesai
        End If

        'cdc(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cdc can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 2 Then
            result(2) = "cdc should not be more than 2 character." : GoTo selesai
        End If

        'cnama(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 100 Then
            result(2) = "cnama should not be more than 100 character." : GoTo selesai
        End If

        'cgd(10) As String
        If Len(dataUtama(10)) = 0 Then
            result(2) = "cgd can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 2 Then
            result(2) = "cgd should not be more than 2 character." : GoTo selesai
        End If

        'JIKA LEVEL > 1 MAKA PARENT TIDAK BOLEH KOSONG
        'clevel(11) As Integer, cparent(13) As String
        If Double.Parse(dataUtama(11)) > 1 Then
            If Len(dataUtama(13)) = 0 Then
                result(2) = "cparent can't be empty" : GoTo selesai
            End If
            If Len(dataUtama(13)) > 25 Then
                result(2) = "cparent should not be more than 25 character." : GoTo selesai
            End If
        End If

        'cjenisaruskas(19) As String
        If Len(dataUtama(19)) = 0 Then
            result(2) = "cjenisaruskas can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(19)) > 2 Then
            result(2) = "cjenisaruskas should not be more than 2 character." : GoTo selesai
        End If

        'cmatauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "cmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "cmatauang should not be more than 25 character." : GoTo selesai
        End If

        'cjenis(27) As String
        If Len(dataUtama(27)) = 0 Then
            result(2) = "cjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(27)) > 2 Then
            result(2) = "cjenis should not be more than 2 character." : GoTo selesai
        End If

        'cinputtgl(32) As DateTime
        If Len(dataUtama(32)) = 0 Then
            result(2) = "cinputtgl can't be empty" : GoTo selesai
        End If

        'cmodifikasitgl(34) As DateTime
        If Len(dataUtama(34)) = 0 Then
            result(2) = "cmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ccustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "ccustomdbl1 can't be empty" : GoTo selesai
        End If

        'ccustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "ccustomdbl2 can't be empty" : GoTo selesai
        End If

        'ccustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "ccustomdbl3 can't be empty" : GoTo selesai
        End If

        'ccustomdbl4(59) As Double
        If Len(dataUtama(59)) = 0 Then
            result(2) = "ccustomdbl4 can't be empty" : GoTo selesai
        End If

        'ccustomdbl5(60) As Double
        If Len(dataUtama(60)) = 0 Then
            result(2) = "ccustomdbl5 can't be empty" : GoTo selesai
        End If

        'ccustomdbl6(61) As Double
        If Len(dataUtama(61)) = 0 Then
            result(2) = "ccustomdbl6 can't be empty" : GoTo selesai
        End If

        'ccustomdbl7(62) As Double
        If Len(dataUtama(62)) = 0 Then
            result(2) = "ccustomdbl7 can't be empty" : GoTo selesai
        End If

        'ccustomdbl8(63) As Double
        If Len(dataUtama(63)) = 0 Then
            result(2) = "ccustomdbl8 can't be empty" : GoTo selesai
        End If

        'ccustomdbl9(64) As Double
        If Len(dataUtama(64)) = 0 Then
            result(2) = "ccustomdbl9 can't be empty" : GoTo selesai
        End If

        'ccustomdbl10(65) As Double
        If Len(dataUtama(65)) = 0 Then
            result(2) = "ccustomdbl10 can't be empty" : GoTo selesai
        End If

        'ccustomdate1(66) As Date
        If Len(dataUtama(66)) = 0 Then
            result(2) = "ccustomdate1 can't be empty" : GoTo selesai
        End If

        'ccustomdate2(67) As Date
        If Len(dataUtama(67)) = 0 Then
            result(2) = "ccustomdate2 can't be empty" : GoTo selesai
        End If

        'ccustomdate3(68) As Date
        If Len(dataUtama(68)) = 0 Then
            result(2) = "ccustomdate3 can't be empty" : GoTo selesai
        End If

        'ccustomdate4(69) As Date
        If Len(dataUtama(69)) = 0 Then
            result(2) = "ccustomdate4 can't be empty" : GoTo selesai
        End If

        'ccustomdate5(70) As Date
        If Len(dataUtama(70)) = 0 Then
            result(2) = "ccustomdate5 can't be empty" : GoTo selesai
        End If

        'ccustomdate6(71) As Date
        If Len(dataUtama(71)) = 0 Then
            result(2) = "ccustomdate6 can't be empty" : GoTo selesai
        End If

        'ccustomdate7(72) As Date
        If Len(dataUtama(72)) = 0 Then
            result(2) = "ccustomdate7 can't be empty" : GoTo selesai
        End If

        'ccustomdate8(73) As Date
        If Len(dataUtama(73)) = 0 Then
            result(2) = "ccustomdate8 can't be empty" : GoTo selesai
        End If

        'ccustomdate9(74) As Date
        If Len(dataUtama(74)) = 0 Then
            result(2) = "ccustomdate9 can't be empty" : GoTo selesai
        End If

        'ccustomdate10(75) As Date
        If Len(dataUtama(75)) = 0 Then
            result(2) = "ccustomdate10 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA ========================================================

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(cid) FROM M1_Coa WHERE cid='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_coa_history
                    Dim coaSimpanHistory As String = SimpanHistory.M1_Coa_HistorySimpan("" & paramSplit(0) & "★M1_Coa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(1)) & "")
                    Dim coaSplit() As String = coaSimpanHistory.Split(sptParam)
                    Dim coaSplitResult() As String = coaSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (coaSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & coaSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Coa set cnomor  = '" & FixQuotes(dataUtama(1)) & "', ctipe  = " & dataUtama(2) & ", cdc  = '" & FixQuotes(dataUtama(3)) & "', curutan  = " & dataUtama(4) & ", caktif  = " & dataUtama(5) & ", cnama  = '" & FixQuotes(dataUtama(6)) & "', cnamaalias1  = '" & FixQuotes(dataUtama(7)) & "', cnamaalias2  = '" & FixQuotes(dataUtama(8)) & "', cnamaalias3  = '" & FixQuotes(dataUtama(9)) & "', cgd  = '" & FixQuotes(dataUtama(10)) & "', clevel  = " & dataUtama(11) & ", csubdari  = " & dataUtama(12) & ", cparent  = '" & FixQuotes(dataUtama(13)) & "', clevel1  = '" & FixQuotes(dataUtama(14)) & "', clevel2  = '" & FixQuotes(dataUtama(15)) & "', clevel3  = '" & FixQuotes(dataUtama(16)) & "', clevel4  = '" & FixQuotes(dataUtama(17)) & "', clevel5  = '" & FixQuotes(dataUtama(18)) & "', cjenisaruskas  = '" & FixQuotes(dataUtama(19)) & "', cbukupembantu  = " & dataUtama(20) & ", ccabang  = '" & FixQuotes(dataUtama(21)) & "', clokasi  = '" & FixQuotes(dataUtama(22)) & "', cdivisi  = '" & FixQuotes(dataUtama(23)) & "', cmatauang  = '" & FixQuotes(dataUtama(24)) & "', ckodebank  = '" & FixQuotes(dataUtama(25)) & "', cnorekbank  = '" & FixQuotes(dataUtama(26)) & "', cjenis  = '" & FixQuotes(dataUtama(27)) & "', csaldoawal  = '" & FixDouble(dataUtama(28)) & "', csaldoberjalan  = '" & FixDouble(dataUtama(29)) & "', ccatatan  = '" & FixQuotes(dataUtama(30)) & "', cinputuser  = " & dataUtama(31) & ", cinputtgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(32), "yyyy-MM-dd H:mm:ss")) & "', cmodifikasiuser  = " & dataUtama(33) & ", cmodifikasitgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(34), "yyyy-MM-dd H:mm:ss")) & "', ccostcenter  = " & dataUtama(35) & ", ccustomtext1  = '" & FixQuotes(dataUtama(36)) & "', ccustomtext2  = '" & FixQuotes(dataUtama(37)) & "', ccustomtext3  = '" & FixQuotes(dataUtama(38)) & "', ccustomtext4  = '" & FixQuotes(dataUtama(39)) & "', ccustomtext5  = '" & FixQuotes(dataUtama(40)) & "', ccustomtext6  = '" & FixQuotes(dataUtama(41)) & "', ccustomtext7  = '" & FixQuotes(dataUtama(42)) & "', ccustomtext8  = '" & FixQuotes(dataUtama(43)) & "', ccustomtext9  = '" & FixQuotes(dataUtama(44)) & "', ccustomtext10  = '" & FixQuotes(dataUtama(45)) & "', ccustomint1  = " & dataUtama(46) & ", ccustomint2  = " & dataUtama(47) & ", ccustomint3  = " & dataUtama(48) & ", ccustomint4  = " & dataUtama(49) & ", ccustomint5  = " & dataUtama(50) & ", ccustomint6  = " & dataUtama(51) & ", ccustomint7  = " & dataUtama(52) & ", ccustomint8  = " & dataUtama(53) & ", ccustomint9  = " & dataUtama(54) & ", ccustomint10  = " & dataUtama(55) & ", ccustomdbl1  = '" & FixDouble(dataUtama(56)) & "', ccustomdbl2  = '" & FixDouble(dataUtama(57)) & "', ccustomdbl3  = '" & FixDouble(dataUtama(58)) & "', ccustomdbl4  = '" & FixDouble(dataUtama(59)) & "', ccustomdbl5  = '" & FixDouble(dataUtama(60)) & "', ccustomdbl6  = '" & FixDouble(dataUtama(61)) & "', ccustomdbl7  = '" & FixDouble(dataUtama(62)) & "', ccustomdbl8  = '" & FixDouble(dataUtama(63)) & "', ccustomdbl9  = '" & FixDouble(dataUtama(64)) & "', ccustomdbl10  = '" & FixDouble(dataUtama(65)) & "', ccustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(66))) & "', ccustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(67))) & "', ccustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(68))) & "', ccustomdate4  = '" & FixQuotes(AsFormatTanggal(dataUtama(69))) & "', ccustomdate5  = '" & FixQuotes(AsFormatTanggal(dataUtama(70))) & "', ccustomdate6  = '" & FixQuotes(AsFormatTanggal(dataUtama(71))) & "', ccustomdate7  = '" & FixQuotes(AsFormatTanggal(dataUtama(72))) & "', ccustomdate8  = '" & FixQuotes(AsFormatTanggal(dataUtama(73))) & "', ccustomdate9  = '" & FixQuotes(AsFormatTanggal(dataUtama(74))) & "', ccustomdate10  = '" & FixQuotes(AsFormatTanggal(dataUtama(75))) & "' where cid = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else
                sql = "Insert into M1_Coa (cnomor, ctipe, cdc, curutan, caktif, cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, ccostcenter, ccustomtext1, ccustomtext2, ccustomtext3, ccustomtext4, ccustomtext5, ccustomtext6, ccustomtext7, ccustomtext8, ccustomtext9, ccustomtext10, ccustomint1, ccustomint2, ccustomint3, ccustomint4, ccustomint5, ccustomint6, ccustomint7, ccustomint8, ccustomint9, ccustomint10, ccustomdbl1, ccustomdbl2, ccustomdbl3, ccustomdbl4, ccustomdbl5, ccustomdbl6, ccustomdbl7, ccustomdbl8, ccustomdbl9, ccustomdbl10, ccustomdate1, ccustomdate2, ccustomdate3, ccustomdate4, ccustomdate5, ccustomdate6, ccustomdate7, ccustomdate8, ccustomdate9, ccustomdate10) values('" & FixQuotes(dataUtama(1)) & "', " & dataUtama(2) & ", '" & FixQuotes(dataUtama(3)) & "', " & dataUtama(4) & ", " & dataUtama(5) & ", '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(dataUtama(10)) & "', " & dataUtama(11) & ", " & dataUtama(12) & ", '" & FixQuotes(dataUtama(13)) & "', '" & FixQuotes(dataUtama(14)) & "', '" & FixQuotes(dataUtama(15)) & "', '" & FixQuotes(dataUtama(16)) & "', '" & FixQuotes(dataUtama(17)) & "', '" & FixQuotes(dataUtama(18)) & "', '" & FixQuotes(dataUtama(19)) & "', " & dataUtama(20) & ", '" & FixQuotes(dataUtama(21)) & "', '" & FixQuotes(dataUtama(22)) & "', '" & FixQuotes(dataUtama(23)) & "', '" & FixQuotes(dataUtama(24)) & "', '" & FixQuotes(dataUtama(25)) & "', '" & FixQuotes(dataUtama(26)) & "', '" & FixQuotes(dataUtama(27)) & "', '" & FixDouble(dataUtama(28)) & "', '" & FixDouble(dataUtama(29)) & "', '" & FixQuotes(dataUtama(30)) & "', " & dataUtama(31) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(32), "yyyy-MM-dd H:mm:ss")) & "', " & dataUtama(33) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(34), "yyyy-MM-dd H:mm:ss")) & "', " & dataUtama(35) & ", '" & FixQuotes(dataUtama(36)) & "', '" & FixQuotes(dataUtama(37)) & "', '" & FixQuotes(dataUtama(38)) & "', '" & FixQuotes(dataUtama(39)) & "', '" & FixQuotes(dataUtama(40)) & "', '" & FixQuotes(dataUtama(41)) & "', '" & FixQuotes(dataUtama(42)) & "', '" & FixQuotes(dataUtama(43)) & "', '" & FixQuotes(dataUtama(44)) & "', '" & FixQuotes(dataUtama(45)) & "', " & dataUtama(46) & ", " & dataUtama(47) & ", " & dataUtama(48) & ", " & dataUtama(49) & ", " & dataUtama(50) & ", " & dataUtama(51) & ", " & dataUtama(52) & ", " & dataUtama(53) & ", " & dataUtama(54) & ", " & dataUtama(55) & ", '" & FixDouble(dataUtama(56)) & "', '" & FixDouble(dataUtama(57)) & "', '" & FixDouble(dataUtama(58)) & "', '" & FixDouble(dataUtama(59)) & "', '" & FixDouble(dataUtama(60)) & "', '" & FixDouble(dataUtama(61)) & "', '" & FixDouble(dataUtama(62)) & "', '" & FixDouble(dataUtama(63)) & "', '" & FixDouble(dataUtama(64)) & "', '" & FixDouble(dataUtama(65)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(66))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(67))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(68))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(69))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(70))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(71))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(72))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(73))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(74))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(75))) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_CoaSearch(PostWsSearch(paramSplit(0), "M1_CoaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
            result(2) = "Transaction Rollback : " & ex.Message
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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_CoaDelete(ByVal param As String) As String

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
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim pg1 As New RsPaging

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "cnomor can't be empty." : GoTo selesai
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

            'CEK TERKAIT =============================================================
            Dim paramTerkait As String = M1_CoaTerkait(PostWsTerkait(paramSplit(0), "M1_CoaTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
            Dim hasilTerkait As New RsHasilWsSearch
            hasilTerkait = GetWsSearch(paramTerkait)
            If hasilTerkait.success = 1 Then
                result(2) = "It has related transactions."

                resultPaging(0) = hasilTerkait.isPaging
                resultPaging(1) = hasilTerkait.isNext
                resultPaging(2) = hasilTerkait.isPrevious
                resultPaging(3) = hasilTerkait.countPage
                resultPaging(4) = hasilTerkait.countRow

                search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
            End If
            'END OF CEK TERKAIT ======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m1_coa_history
            Dim coaSimpanHistory As String = SimpanHistory.M1_Coa_HistorySimpan("" & paramSplit(0) & "★M1_Coa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim coaSplit() As String = coaSimpanHistory.Split(sptParam)
            Dim coaSplitResult() As String = coaSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (coaSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & coaSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Coa WHERE cnomor = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_CoaSearch(PostWsSearch(paramSplit(0), "M1_CoaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_CoaSearch(ByVal param As String) As String
        'M1_CoaSearch --------------------------------------------------------
        'cid, cnomor, ctipe, cdc, curutan, caktif, cnama, 
        'cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, 
        'clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, 
        'ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, 
        'csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, 
        'csaldoakhir, cparentnama, ccabangnama, clokasinama, cdivisinama, cmatauangnama, cnamabank, 
        'ccostcenter, ccustomtext1, ccustomtext2, ccustomtext3, ccustomtext4, ccustomtext5, ccustomtext6, 
        'ccustomtext7, ccustomtext8, ccustomtext9, ccustomtext10, ccustomint1, ccustomint2, ccustomint3, 
        'ccustomint4, ccustomint5, ccustomint6, ccustomint7, ccustomint8, ccustomint9, ccustomint10, 
        'ccustomdbl1, ccustomdbl2, ccustomdbl3, ccustomdbl4, ccustomdbl5, ccustomdbl6, ccustomdbl7, 
        'ccustomdbl8, ccustomdbl9, ccustomdbl10, ccustomdate1, ccustomdate2, ccustomdate3, ccustomdate4, 
        'ccustomdate5, ccustomdate6, ccustomdate7, ccustomdate8, ccustomdate9, ccustomdate10

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
            Filter = Filter.Replace("caktif", "c.caktif")
            Filter = Filter.Replace("cnama", "c.cnama")
            Filter = Filter.Replace("cnomor", "c.cnomor")
            Filter = Filter.Replace("cnamaalias1", "c.cnamaalias1")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_coa_search")
        sql = "SELECT c.cid AS cid, c.cnomor AS cnomor, c.ctipe AS ctipe, c.cdc AS cdc, c.curutan AS curutan, c.caktif AS caktif, c.cnama AS cnama, c.cnamaalias1 AS cnamaalias1, c.cnamaalias2 AS cnamaalias2, c.cnamaalias3 AS cnamaalias3, c.cgd AS cgd, c.clevel AS clevel, c.csubdari AS csubdari, c.cparent AS cparent, c.clevel1 AS clevel1, c.clevel2 AS clevel2, c.clevel3 AS clevel3, c.clevel4 AS clevel4, c.clevel5 AS clevel5, c.cjenisaruskas AS cjenisaruskas, c.cbukupembantu AS cbukupembantu, c.ccabang AS ccabang, c.clokasi AS clokasi, c.cdivisi AS cdivisi, c.cmatauang AS cmatauang, c.ckodebank AS ckodebank, c.cnorekbank AS cnorekbank, c.cjenis AS cjenis, c.csaldoawal AS csaldoawal, c.csaldoberjalan AS csaldoberjalan, c.ccatatan AS ccatatan, c.cinputuser AS cinputuser, c.cinputtgl AS cinputtgl, c.cmodifikasiuser AS cmodifikasiuser, c.cmodifikasitgl AS cmodifikasitgl, (c.csaldoawal + c.csaldoberjalan) AS csaldoakhir, c2.cnama AS cparentnama, br.bnama AS ccabangnama, lc.lnama AS clokasinama, d.dnama AS cdivisinama, cr.cnama AS cmatauangnama, bn.bnama AS cnamabank, c.ccostcenter, c.ccustomtext1, c.ccustomtext2, c.ccustomtext3, c.ccustomtext4, c.ccustomtext5, c.ccustomtext6, c.ccustomtext7, c.ccustomtext8, c.ccustomtext9, c.ccustomtext10, c.ccustomint1, c.ccustomint2, c.ccustomint3, c.ccustomint4, c.ccustomint5, c.ccustomint6, c.ccustomint7, c.ccustomint8, c.ccustomint9, c.ccustomint10, c.ccustomdbl1, c.ccustomdbl2, c.ccustomdbl3, c.ccustomdbl4, c.ccustomdbl5, c.ccustomdbl6, c.ccustomdbl7, c.ccustomdbl8, c.ccustomdbl9, c.ccustomdbl10, c.ccustomdate1, c.ccustomdate2, c.ccustomdate3, c.ccustomdate4, c.ccustomdate5, c.ccustomdate6, c.ccustomdate7, c.ccustomdate8, c.ccustomdate9, c.ccustomdate10 from m1_coa c left join m1_coa c2 on c.cparent = c2.cnomor left join m1_branch br on c.ccabang = br.bkode left join m1_location lc on c.clokasi = lc.lkode left join m1_division d on c.cdivisi = d.dkode left join m1_bank bn on c.ckodebank = bn.bkode left join m1_currency cr on c.cmatauang = cr.ckode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Coa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cid"), ""), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("ctipe"), 0), sptField,
                     FxDB(dr("cdc"), ""), sptField,
                     FxDB(dr("curutan"), 0), sptField,
                     FxDB(dr("caktif"), 0), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cnamaalias1"), ""), sptField,
                     FxDB(dr("cnamaalias2"), ""), sptField,
                     FxDB(dr("cnamaalias3"), ""), sptField,
                     FxDB(dr("cgd"), ""), sptField,
                     FxDB(dr("clevel"), 0), sptField,
                     FxDB(dr("csubdari"), 0), sptField,
                     FxDB(dr("cparent"), ""), sptField,
                     FxDB(dr("clevel1"), ""), sptField,
                     FxDB(dr("clevel2"), ""), sptField,
                     FxDB(dr("clevel3"), ""), sptField,
                     FxDB(dr("clevel4"), ""), sptField,
                     FxDB(dr("clevel5"), ""), sptField,
                     FxDB(dr("cjenisaruskas"), ""), sptField,
                     FxDB(dr("cbukupembantu"), 0), sptField,
                     FxDB(dr("ccabang"), ""), sptField,
                     FxDB(dr("clokasi"), ""), sptField,
                     FxDB(dr("cdivisi"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("ckodebank"), ""), sptField,
                     FxDB(dr("cnorekbank"), ""), sptField,
                     FxDB(dr("cjenis"), ""), sptField,
                     FxDB(dr("csaldoawal"), 0), sptField,
                     FxDB(dr("csaldoberjalan"), 0), sptField,
                     FxDB(dr("ccatatan"), ""), sptField,
                     FxDB(dr("cinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("csaldoakhir"), 0), sptField,
                     FxDB(dr("cparentnama"), ""), sptField,
                     FxDB(dr("ccabangnama"), ""), sptField,
                     FxDB(dr("clokasinama"), ""), sptField,
                     FxDB(dr("cdivisinama"), ""), sptField,
                     FxDB(dr("cmatauangnama"), ""), sptField,
                     FxDB(dr("cnamabank"), ""), sptField,
                     FxDB(dr("ccostcenter"), 0), sptField,
                     FxDB(dr("ccustomtext1"), ""), sptField,
                     FxDB(dr("ccustomtext2"), ""), sptField,
                     FxDB(dr("ccustomtext3"), ""), sptField,
                     FxDB(dr("ccustomtext4"), ""), sptField,
                     FxDB(dr("ccustomtext5"), ""), sptField,
                     FxDB(dr("ccustomtext6"), ""), sptField,
                     FxDB(dr("ccustomtext7"), ""), sptField,
                     FxDB(dr("ccustomtext8"), ""), sptField,
                     FxDB(dr("ccustomtext9"), ""), sptField,
                     FxDB(dr("ccustomtext10"), ""), sptField,
                     FxDB(dr("ccustomint1"), 0), sptField,
                     FxDB(dr("ccustomint2"), 0), sptField,
                     FxDB(dr("ccustomint3"), 0), sptField,
                     FxDB(dr("ccustomint4"), 0), sptField,
                     FxDB(dr("ccustomint5"), 0), sptField,
                     FxDB(dr("ccustomint6"), 0), sptField,
                     FxDB(dr("ccustomint7"), 0), sptField,
                     FxDB(dr("ccustomint8"), 0), sptField,
                     FxDB(dr("ccustomint9"), 0), sptField,
                     FxDB(dr("ccustomint10"), 0), sptField,
                     FxDB(dr("ccustomdbl1"), 0), sptField,
                     FxDB(dr("ccustomdbl2"), 0), sptField,
                     FxDB(dr("ccustomdbl3"), 0), sptField,
                     FxDB(dr("ccustomdbl4"), 0), sptField,
                     FxDB(dr("ccustomdbl5"), 0), sptField,
                     FxDB(dr("ccustomdbl6"), 0), sptField,
                     FxDB(dr("ccustomdbl7"), 0), sptField,
                     FxDB(dr("ccustomdbl8"), 0), sptField,
                     FxDB(dr("ccustomdbl9"), 0), sptField,
                     FxDB(dr("ccustomdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate10"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Coa data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cid, cnomor, ctipe, cdc, curutan, caktif, cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, csaldoakhir, cparentnama, ccabangnama, clokasinama, cdivisinama, cmatauangnama, cnamabank, ccostcenter, ccustomtext1, ccustomtext2, ccustomtext3, ccustomtext4, ccustomtext5, ccustomtext6, ccustomtext7, ccustomtext8, ccustomtext9, ccustomtext10, ccustomint1, ccustomint2, ccustomint3, ccustomint4, ccustomint5, ccustomint6, ccustomint7, ccustomint8, ccustomint9, ccustomint10, ccustomdbl1, ccustomdbl2, ccustomdbl3, ccustomdbl4, ccustomdbl5, ccustomdbl6, ccustomdbl7, ccustomdbl8, ccustomdbl9, ccustomdbl10, ccustomdate1, ccustomdate2, ccustomdate3, ccustomdate4, ccustomdate5, ccustomdate6, ccustomdate7, ccustomdate8, ccustomdate9, ccustomdate10"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_CoaCekId(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "cnomor can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(cnomor) FROM m1_coa WHERE cnomor='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column cnomor." : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi
        'END OF CEK DI DATABASE ==========================================================


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
    Public Function M1_CoaTerkait(ByVal param As String) As String
        'M1_CoaTerkait --------------------------------------------------------
        'cnomor, cnama, sumber, idterkait

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
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "cnomor can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_coa_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("cnomor"), ""), sptField,
                             FxDB(dr("cnama"), ""), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idterkait"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related Coa data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cnomor, cnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_CoaDownload(ByVal param As String) As String
        'M1_CoaDownload --------------------------------------------------------
        'cid, cnomor, ctipe, ckategori, cdc, curutan, caktif, 
        'cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, 
        'cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, 
        'cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, 
        'cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, 
        'cmodifikasitgl, ccostcenter

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

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Coa", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cid"), ""), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("ctipe"), 0), sptField,
                     FxDB(dr("ckategori"), 0), sptField,
                     FxDB(dr("cdc"), ""), sptField,
                     FxDB(dr("curutan"), 0), sptField,
                     FxDB(dr("caktif"), 0), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cnamaalias1"), ""), sptField,
                     FxDB(dr("cnamaalias2"), ""), sptField,
                     FxDB(dr("cnamaalias3"), ""), sptField,
                     FxDB(dr("cgd"), ""), sptField,
                     FxDB(dr("clevel"), 0), sptField,
                     FxDB(dr("csubdari"), 0), sptField,
                     FxDB(dr("cparent"), ""), sptField,
                     FxDB(dr("clevel1"), ""), sptField,
                     FxDB(dr("clevel2"), ""), sptField,
                     FxDB(dr("clevel3"), ""), sptField,
                     FxDB(dr("clevel4"), ""), sptField,
                     FxDB(dr("clevel5"), ""), sptField,
                     FxDB(dr("cjenisaruskas"), ""), sptField,
                     FxDB(dr("cbukupembantu"), 0), sptField,
                     FxDB(dr("ccabang"), ""), sptField,
                     FxDB(dr("clokasi"), ""), sptField,
                     FxDB(dr("cdivisi"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("ckodebank"), ""), sptField,
                     FxDB(dr("cnorekbank"), ""), sptField,
                     FxDB(dr("cjenis"), ""), sptField,
                     FxDB(dr("csaldoawal"), 0), sptField,
                     FxDB(dr("csaldoberjalan"), 0), sptField,
                     FxDB(dr("ccatatan"), ""), sptField,
                     FxDB(dr("cinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ccostcenter"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cid, cnomor, ctipe, ckategori, cdc, curutan, caktif, cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, ccostcenter"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_CoaImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'cid(0) As Integer, cnomor(1) As String, ctipe(2) As Integer, ckategori(3) As Integer, cdc(4) As String, 
        'curutan(5) As Integer, caktif(6) As Integer, cnama(7) As String, cnamaalias1(8) As String, cnamaalias2(9) As String, 
        'cnamaalias3(10) As String, cgd(11) As String, clevel(12) As Integer, csubdari(13) As Integer, cparent(14) As String, 
        'clevel1(15) As String, clevel2(16) As String, clevel3(17) As String, clevel4(18) As String, clevel5(19) As String, 
        'cjenisaruskas(20) As String, cbukupembantu(21) As Integer, ccabang(22) As String, clokasi(23) As String, cdivisi(24) As String, 
        'cmatauang(25) As String, ckodebank(26) As String, cnorekbank(27) As String, cjenis(28) As String, csaldoawal(29) As Double, 
        'csaldoberjalan(30) As Double, ccatatan(31) As String, cinputuser(32) As Integer, cinputtgl(33) As DateTime, cmodifikasiuser(34) As Integer, 
        'cmodifikasitgl(35) As DateTime, ccostcenter(36) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'cid, cnomor, ctipe, ckategori, cdc, curutan, caktif, 
        'cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, 
        'cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, 
        'cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, 
        'cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, 
        'cmodifikasitgl, ccostcenter

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "cid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cnomor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ctipe", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ckategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cdc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "curutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "caktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cnamaalias1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cnamaalias2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cnamaalias3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cgd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clevel", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "csubdari", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cparent", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clevel1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clevel2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clevel3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clevel4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clevel5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cjenisaruskas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cbukupembantu", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ccabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "clokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ckodebank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cnorekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "csaldoawal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "csaldoberjalan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ccatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "cmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "cmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ccostcenter", AsEnumTypeData.AsInt64)

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

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 37) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'cid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - cid required numeric." : GoTo selesai
            End If
            'ctipe(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - ctipe required numeric." : GoTo selesai
            End If
            'ckategori(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - ckategori required numeric." : GoTo selesai
            End If
            'curutan(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - curutan required numeric." : GoTo selesai
            End If
            'caktif(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - caktif required numeric." : GoTo selesai
            End If
            'clevel(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - clevel required numeric." : GoTo selesai
            End If
            'csubdari(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - csubdari required numeric." : GoTo selesai
            End If
            'cbukupembantu(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - cbukupembantu required numeric." : GoTo selesai
            End If
            'csaldoawal(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - csaldoawal required numeric." : GoTo selesai
            End If
            'csaldoberjalan(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - csaldoberjalan required numeric." : GoTo selesai
            End If
            'cinputuser(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - cinputuser required numeric." : GoTo selesai
            End If
            'cinputtgl(33) As DateTime
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - cinputtgl required date." : GoTo selesai
            End If
            'cmodifikasiuser(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - cmodifikasiuser required numeric." : GoTo selesai
            End If
            'cmodifikasitgl(35) As DateTime
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - cmodifikasitgl required date." : GoTo selesai
            End If
            'ccostcenter(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - ccostcenter required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'cnomor(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - cnomor can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - cnomor should not be more than 25 character." : GoTo selesai
            End If

            'cdc(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - cdc can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 2 Then
                result(2) = "Row : " & i & " - cdc should not be more than 2 character." : GoTo selesai
            End If

            'cnama(7) As String
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - cnama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(7)) > 100 Then
                result(2) = "Row : " & i & " - cnama should not be more than 100 character." : GoTo selesai
            End If

            'cgd(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - cgd can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 2 Then
                result(2) = "Row : " & i & " - cgd should not be more than 2 character." : GoTo selesai
            End If

            'cmatauang(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - cmatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - cmatauang should not be more than 25 character." : GoTo selesai
            End If

            'cinputtgl(33) As DateTime
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - cinputtgl can't be empty" : GoTo selesai
            End If

            'cmodifikasitgl(35) As DateTime
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - cmodifikasitgl can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "cid~cnomor~ctipe~ckategori~cdc~curutan~caktif~cnama~cnamaalias1~cnamaalias2~cnamaalias3~cgd~clevel~csubdari~cparent~clevel1~clevel2~clevel3~clevel4~clevel5~cjenisaruskas~cbukupembantu~ccabang~clokasi~cdivisi~cmatauang~ckodebank~cnorekbank~cjenis~csaldoawal~csaldoberjalan~ccatatan~cinputuser~cinputtgl~cmodifikasiuser~cmodifikasitgl~ccostcenter", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("cid")) & "', '" & FixQuotes(dr1("cnomor")) & "', " & dr1("ctipe") & ", " & dr1("ckategori") & ", '" & FixQuotes(dr1("cdc")) & "', " & dr1("curutan") & ", " & dr1("caktif") & ", '" & FixQuotes(dr1("cnama")) & "', '" & FixQuotes(dr1("cnamaalias1")) & "', '" & FixQuotes(dr1("cnamaalias2")) & "', '" & FixQuotes(dr1("cnamaalias3")) & "', '" & FixQuotes(dr1("cgd")) & "', " & dr1("clevel") & ", " & dr1("csubdari") & ", '" & FixQuotes(dr1("cparent")) & "', '" & FixQuotes(dr1("clevel1")) & "', '" & FixQuotes(dr1("clevel2")) & "', '" & FixQuotes(dr1("clevel3")) & "', '" & FixQuotes(dr1("clevel4")) & "', '" & FixQuotes(dr1("clevel5")) & "', '" & FixQuotes(dr1("cjenisaruskas")) & "', " & dr1("cbukupembantu") & ", '" & FixQuotes(dr1("ccabang")) & "', '" & FixQuotes(dr1("clokasi")) & "', '" & FixQuotes(dr1("cdivisi")) & "', '" & FixQuotes(dr1("cmatauang")) & "', '" & FixQuotes(dr1("ckodebank")) & "', '" & FixQuotes(dr1("cnorekbank")) & "', '" & FixQuotes(dr1("cjenis")) & "', '" & FixDouble(dr1("csaldoawal")) & "', '" & FixDouble(dr1("csaldoberjalan")) & "', '" & FixQuotes(dr1("ccatatan")) & "', " & dr1("cinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("cinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("cmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("cmodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("ccostcenter") & ")")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M1_Coa"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M1_Coa(cid, cnomor, ctipe, ckategori, cdc, curutan, caktif, cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, ccostcenter) values" & strValue2.ToString & ""
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
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_CoaSearch(PostWsSearch(paramSplit(0), "M1_CoaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

End Class