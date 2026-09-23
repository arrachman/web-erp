Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_hardware
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_HardwareSimpan(ByVal param As String) As String
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

        'MAPPING BUAT WS ----------------------------------------------------------
        'phcomputermac(0) As String, phcomputerip(1) As String, phprinter(2) As String, phprinterport(3) As String, phidreport(4) As Integer, 
        'phcetak(5) As Integer, phcetakbarang(6) As Integer, phfeed(7) As Integer, phcashdrawer(8) As Integer, phcashdrawerprinter(9) As String, 
        'phcashdrawerport(10) As String, phpolenama(11) As String, phpoleport(12) As String, phpoledisplay(13) As Integer, phpolebaudrate(14) As Double, 
        'phpoleparity(15) As Integer, phpoledatabit(16) As Double, phpolestopbit(17) As Double, phescheader(18) As String, phescbody(19) As String, 
        'phescfooter(20) As String, phesccashdrawer(21) As String, phcustomtext1(22) As String, phcustomtext2(23) As String, phcustomtext3(24) As String, 
        'phcustomtext4(25) As String, phcustomtext5(26) As String, phcustomint1(27) As Integer, phcustomint2(28) As Integer, phcustomint3(29) As Integer, 
        'phcustomdbl1(30) As Double, phcustomdbl2(31) As Double, phcustomdbl3(32) As Double, phcustomdate1(33) As Date, phcustomdate2(34) As Date, 
        'phcustomdate3(35) As Date, phuserid(36) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'phcomputermac, phcomputerip, phprinter, phprinterport, phidreport, phcetak, phcetakbarang, 
        'phfeed, phcashdrawer, phcashdrawerprinter, phcashdrawerport, phpolenama, phpoleport, phpoledisplay, 
        'phpolebaudrate, phpoleparity, phpoledatabit, phpolestopbit, phescheader, phescbody, phescfooter, 
        'phesccashdrawer, phcustomtext1, phcustomtext2, phcustomtext3, phcustomtext4, phcustomtext5, phcustomint1, 
        'phcustomint2, phcustomint3, phcustomdbl1, phcustomdbl2, phcustomdbl3, phcustomdate1, phcustomdate2, 
        'phcustomdate3, phuserid

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "phcomputermac", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcomputerip", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phprinter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phprinterport", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phidreport", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcetak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcetakbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phfeed", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcashdrawer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcashdrawerprinter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcashdrawerport", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phpolenama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phpoleport", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phpoledisplay", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phpolebaudrate", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phpoleparity", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phpoledatabit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phpolestopbit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phescheader", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phescbody", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phescfooter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phesccashdrawer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "phcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "phuserid", AsEnumTypeData.AsString)

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
            'phuserid(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - phuserid required numeric." : GoTo selesai
            End If

            'phidreport(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - phidreport required numeric." : GoTo selesai
            End If
            'phcetak(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - phcetak required numeric." : GoTo selesai
            End If
            'phcetakbarang(6) As Integer
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - phcetakbarang required numeric." : GoTo selesai
            End If
            'phfeed(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - phfeed required numeric." : GoTo selesai
            End If
            'phcashdrawer(8) As Integer
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - phcashdrawer required numeric." : GoTo selesai
            End If
            'phpoledisplay(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - phpoledisplay required numeric." : GoTo selesai
            End If
            'phpolebaudrate(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - phpolebaudrate required numeric." : GoTo selesai
            End If
            'phpoleparity(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - phpoleparity required numeric." : GoTo selesai
            End If
            'phpoledatabit(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - phpoledatabit required numeric." : GoTo selesai
            End If
            'phpolestopbit(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - phpolestopbit required numeric." : GoTo selesai
            End If
            'phcustomint1(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - phcustomint1 required numeric." : GoTo selesai
            End If
            'phcustomint2(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - phcustomint2 required numeric." : GoTo selesai
            End If
            'phcustomint3(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - phcustomint3 required numeric." : GoTo selesai
            End If
            'phcustomdbl1(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - phcustomdbl1 required numeric." : GoTo selesai
            End If
            'phcustomdbl2(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - phcustomdbl2 required numeric." : GoTo selesai
            End If
            'phcustomdbl3(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - phcustomdbl3 required numeric." : GoTo selesai
            End If
            'phcustomdate1(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - phcustomdate1 required date." : GoTo selesai
            End If
            'phcustomdate2(34) As Date
            If (IsDate(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - phcustomdate2 required date." : GoTo selesai
            End If
            'phcustomdate3(35) As Date
            If (IsDate(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - phcustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'phcomputermac(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - phcomputermac can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 255 Then
                result(2) = "Row : " & i & " - phcomputermac should not be more than 255 character." : GoTo selesai
            End If

            'phcomputerip(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - phcomputerip can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 255 Then
                result(2) = "Row : " & i & " - phcomputerip should not be more than 255 character." : GoTo selesai
            End If

            'phpolebaudrate(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - phpolebaudrate can't be empty" : GoTo selesai
            End If

            'phpoledatabit(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - phpoledatabit can't be empty" : GoTo selesai
            End If

            'phpolestopbit(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - phpolestopbit can't be empty" : GoTo selesai
            End If

            'phcustomdbl1(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - phcustomdbl1 can't be empty" : GoTo selesai
            End If

            'phcustomdbl2(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - phcustomdbl2 can't be empty" : GoTo selesai
            End If

            'phcustomdbl3(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - phcustomdbl3 can't be empty" : GoTo selesai
            End If

            'phcustomdate1(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - phcustomdate1 can't be empty" : GoTo selesai
            End If

            'phcustomdate2(34) As Date
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - phcustomdate2 can't be empty" : GoTo selesai
            End If

            'phcustomdate3(35) As Date
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - phcustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "phcomputermac~phcomputerip~phprinter~phprinterport~phidreport~phcetak~phcetakbarang~phfeed~phcashdrawer~phcashdrawerprinter~phcashdrawerport~phpolenama~phpoleport~phpoledisplay~phpolebaudrate~phpoleparity~phpoledatabit~phpolestopbit~phescheader~phescbody~phescfooter~phesccashdrawer~phcustomtext1~phcustomtext2~phcustomtext3~phcustomtext4~phcustomtext5~phcustomint1~phcustomint2~phcustomint3~phcustomdbl1~phcustomdbl2~phcustomdbl3~phcustomdate1~phcustomdate2~phcustomdate3~phuserid", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                If dtdetail.Rows.Count > 0 Then

                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("phcomputermac")) & "', '" & FixQuotes(dr1("phcomputerip")) & "', '" & FixQuotes(dr1("phprinter")) & "', '" & FixQuotes(dr1("phprinterport")) & "', " & dr1("phidreport") & ", " & dr1("phcetak") & ", " & dr1("phcetakbarang") & ", " & dr1("phfeed") & ", " & dr1("phcashdrawer") & ", '" & FixQuotes(dr1("phcashdrawerprinter")) & "', '" & FixQuotes(dr1("phcashdrawerport")) & "', '" & FixQuotes(dr1("phpolenama")) & "', '" & FixQuotes(dr1("phpoleport")) & "', " & dr1("phpoledisplay") & ", '" & FixDouble(dr1("phpolebaudrate")) & "', " & dr1("phpoleparity") & ", '" & FixDouble(dr1("phpoledatabit")) & "', '" & FixDouble(dr1("phpolestopbit")) & "', '" & FixQuotes(dr1("phescheader")) & "', '" & FixQuotes(dr1("phescbody")) & "', '" & FixQuotes(dr1("phescfooter")) & "', '" & FixQuotes(dr1("phesccashdrawer")) & "', '" & FixQuotes(dr1("phcustomtext1")) & "', '" & FixQuotes(dr1("phcustomtext2")) & "', '" & FixQuotes(dr1("phcustomtext3")) & "', '" & FixQuotes(dr1("phcustomtext4")) & "', '" & FixQuotes(dr1("phcustomtext5")) & "', " & dr1("phcustomint1") & ", " & dr1("phcustomint2") & ", " & dr1("phcustomint3") & ", '" & FixDouble(dr1("phcustomdbl1")) & "', '" & FixDouble(dr1("phcustomdbl2")) & "', '" & FixDouble(dr1("phcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("phcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("phcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("phcustomdate3"))) & "', '" & FixQuotes(dr1("phuserid")) & "')")
                    Next

                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then
                        sql = "Insert into M_12_Pos_Hardware(phcomputermac, phcomputerip, phprinter, phprinterport, phidreport, phcetak, phcetakbarang, phfeed, phcashdrawer, phcashdrawerprinter, phcashdrawerport, phpolenama, phpoleport, phpoledisplay, phpolebaudrate, phpoleparity, phpoledatabit, phpolestopbit, phescheader, phescbody, phescfooter, phesccashdrawer, phcustomtext1, phcustomtext2, phcustomtext3, phcustomtext4, phcustomtext5, phcustomint1, phcustomint2, phcustomint3, phcustomdbl1, phcustomdbl2, phcustomdbl3, phcustomdate1, phcustomdate2, phcustomdate3, phuserid) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE phcomputermac = VALUES(phcomputermac), phcomputerip = VALUES(phcomputerip), phprinter = VALUES(phprinter), phprinterport = VALUES(phprinterport), phidreport = VALUES(phidreport), phcetak = VALUES(phcetak), phcetakbarang = VALUES(phcetakbarang), phfeed = VALUES(phfeed), phcashdrawer = VALUES(phcashdrawer), phcashdrawerprinter = VALUES(phcashdrawerprinter), phcashdrawerport = VALUES(phcashdrawerport), phpolenama = VALUES(phpolenama), phpoleport = VALUES(phpoleport), phpoledisplay = VALUES(phpoledisplay), phpolebaudrate = VALUES(phpolebaudrate), phpoleparity = VALUES(phpoleparity), phpoledatabit = VALUES(phpoledatabit), phpolestopbit = VALUES(phpolestopbit), phescheader = VALUES(phescheader), phescbody = VALUES(phescbody), phescfooter = VALUES(phescfooter), phesccashdrawer = VALUES(phesccashdrawer), phcustomtext1 = VALUES(phcustomtext1), phcustomtext2 = VALUES(phcustomtext2), phcustomtext3 = VALUES(phcustomtext3), phcustomtext4 = VALUES(phcustomtext4), phcustomtext5 = VALUES(phcustomtext5), phcustomint1 = VALUES(phcustomint1), phcustomint2 = VALUES(phcustomint2), phcustomint3 = VALUES(phcustomint3), phcustomdbl1 = VALUES(phcustomdbl1), phcustomdbl2 = VALUES(phcustomdbl2), phcustomdbl3 = VALUES(phcustomdbl3), phcustomdate1 = VALUES(phcustomdate1), phcustomdate2 = VALUES(phcustomdate2), phcustomdate3 = VALUES(phcustomdate3)"
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

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_HardwareSearch(PostWsSearch(paramSplit(0), "M12_Pos_HardwareSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

    <WebMethod()>
    Public Function M12_Pos_HardwareSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_HardwareSearch --------------------------------------------------------
        'phcomputermac, phcomputerip, phprinter, phprinterport, phidreport, phcetak, phcetakbarang, 
        'phfeed, phcashdrawer, phcashdrawerprinter, phcashdrawerport, phpolenama, phpoleport, phpoledisplay, 
        'phpolebaudrate, phpoleparity, phpoledatabit, phpolestopbit, phescheader, phescbody, phescfooter, 
        'phesccashdrawer, phcustomtext1, phcustomtext2, phcustomtext3, phcustomtext4, phcustomtext5, phcustomint1, 
        'phcustomint2, phcustomint3, phcustomdbl1, phcustomdbl2, phcustomdbl3, phcustomdate1, phcustomdate2, 
        'phcustomdate3, phuserid

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

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Hardware", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("phcomputermac"), ""), sptField,
                     FxDB(dr("phcomputerip"), ""), sptField,
                     FxDB(dr("phprinter"), ""), sptField,
                     FxDB(dr("phprinterport"), ""), sptField,
                     FxDB(dr("phidreport"), 0), sptField,
                     FxDB(dr("phcetak"), 0), sptField,
                     FxDB(dr("phcetakbarang"), 0), sptField,
                     FxDB(dr("phfeed"), 0), sptField,
                     FxDB(dr("phcashdrawer"), 0), sptField,
                     FxDB(dr("phcashdrawerprinter"), ""), sptField,
                     FxDB(dr("phcashdrawerport"), ""), sptField,
                     FxDB(dr("phpolenama"), ""), sptField,
                     FxDB(dr("phpoleport"), ""), sptField,
                     FxDB(dr("phpoledisplay"), 0), sptField,
                     FxDB(dr("phpolebaudrate"), 0), sptField,
                     FxDB(dr("phpoleparity"), 0), sptField,
                     FxDB(dr("phpoledatabit"), 0), sptField,
                     FxDB(dr("phpolestopbit"), 0), sptField,
                     FxDB(dr("phescheader"), ""), sptField,
                     FxDB(dr("phescbody"), ""), sptField,
                     FxDB(dr("phescfooter"), ""), sptField,
                     FxDB(dr("phesccashdrawer"), ""), sptField,
                     FxDB(dr("phcustomtext1"), ""), sptField,
                     FxDB(dr("phcustomtext2"), ""), sptField,
                     FxDB(dr("phcustomtext3"), ""), sptField,
                     FxDB(dr("phcustomtext4"), ""), sptField,
                     FxDB(dr("phcustomtext5"), ""), sptField,
                     FxDB(dr("phcustomint1"), 0), sptField,
                     FxDB(dr("phcustomint2"), 0), sptField,
                     FxDB(dr("phcustomint3"), 0), sptField,
                     FxDB(dr("phcustomdbl1"), 0), sptField,
                     FxDB(dr("phcustomdbl2"), 0), sptField,
                     FxDB(dr("phcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("phcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("phcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("phcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("phuserid"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Setting Hardware data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("phcomputermac, phcomputerip, phprinter, phprinterport, phidreport, phcetak, phcetakbarang, phfeed, phcashdrawer, phcashdrawerprinter, phcashdrawerport, phpolenama, phpoleport, phpoledisplay, phpolebaudrate, phpoleparity, phpoledatabit, phpolestopbit, phescheader, phescbody, phescfooter, phesccashdrawer, phcustomtext1, phcustomtext2, phcustomtext3, phcustomtext4, phcustomtext5, phcustomint1, phcustomint2, phcustomint3, phcustomdbl1, phcustomdbl2, phcustomdbl3, phcustomdate1, phcustomdate2, phcustomdate3, phuserid"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_HardwareDelete(ByVal param As String) As String

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
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "phuserid required numeric." : GoTo selesai
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

            'DELETE
            sql = "DELETE FROM M_12_Pos_Hardware WHERE phuserid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Pos_HardwareSearch(PostWsSearch(paramSplit(0), "M12_Pos_HardwareSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
