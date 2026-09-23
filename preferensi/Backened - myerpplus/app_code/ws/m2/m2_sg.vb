Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_sg
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_SgSimpan(ByVal param As String) As String
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
        'sgid(0) As Integer, sgcabang(1) As String, sglokasi(2) As String, sgsumber(3) As String, sgautonotransaksi(4) As Integer, 
        'sgnotransaksi(5) As String, sgtgl(6) As Date, sgkodepa(7) As Integer, sgkontak(8) As Integer, sgkontakperson(9) As String, 
        'sguraian(10) As String, sgcatatan(11) As String, sgmatauang(12) As String, sgkurs(13) As Double, sgjumlah(14) As Double, 
        'sgjumlahvalas(15) As Double, sgstatussgc(16) As Integer, sgstatus(17) As Integer, sgstatussebelumnya(18) As Integer, sgjmlrevisi(19) As Integer, 
        'sgcetakanke(20) As Integer, sgisclose(21) As Integer, sginputuser(22) As Integer, sginputtgl(23) As DateTime, sgmodifikasiuser(24) As Integer, 
        'sgmodifikasitgl(25) As DateTime, sgposting(26) As Integer, sgcustomtext1(27) As String, sgcustomtext2(28) As String, sgcustomtext3(29) As String, 
        'sgcustomtext4(30) As String, sgcustomtext5(31) As String, sgcustomint1(32) As Integer, sgcustomint2(33) As Integer, sgcustomint3(34) As Integer, 
        'sgcustomdbl1(35) As Double, sgcustomdbl2(36) As Double, sgcustomdbl3(37) As Double, sgcustomdate1(38) As Date, sgcustomdate2(39) As Date, 
        'sgcustomdate3(40) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgcustomtext1, 
        'sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, sgcustomint3, 
        'sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 41) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sgid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sgid required numeric." : GoTo selesai
        End If
        'sgautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sgautonotransaksi required numeric." : GoTo selesai
        End If
        'sgtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "sgtgl required date." : GoTo selesai
        End If
        'sgkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sgkodepa required numeric." : GoTo selesai
        End If
        'sgkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sgkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "sgkontak can't be empty." : GoTo selesai
        End If
        'sgkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sgkurs required numeric." : GoTo selesai
        End If
        'sgjumlah(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sgjumlah required numeric." : GoTo selesai
        End If
        'sgjumlahvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sgjumlahvalas required numeric." : GoTo selesai
        End If
        'sgstatussgc(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sgstatussgc required numeric." : GoTo selesai
        End If
        'sgstatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sgstatus required numeric." : GoTo selesai
        End If
        'sgstatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "sgstatussebelumnya required numeric." : GoTo selesai
        End If
        'sgjmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "sgjmlrevisi required numeric." : GoTo selesai
        End If
        'sgcetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "sgcetakanke required numeric." : GoTo selesai
        End If
        'sgisclose(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "sgisclose required numeric." : GoTo selesai
        End If
        'sginputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sginputuser required numeric." : GoTo selesai
        End If
        'sginputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sginputtgl required date." : GoTo selesai
        End If
        'sgmodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "sgmodifikasiuser required numeric." : GoTo selesai
        End If
        'sgmodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sgmodifikasitgl required date." : GoTo selesai
        End If
        'sgposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "sgposting required numeric." : GoTo selesai
        End If
        'sgcustomint1(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sgcustomint1 required numeric." : GoTo selesai
        End If
        'sgcustomint2(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sgcustomint2 required numeric." : GoTo selesai
        End If
        'sgcustomint3(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sgcustomint3 required numeric." : GoTo selesai
        End If
        'sgcustomdbl1(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sgcustomdbl1 required numeric." : GoTo selesai
        End If
        'sgcustomdbl2(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sgcustomdbl2 required numeric." : GoTo selesai
        End If
        'sgcustomdbl3(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sgcustomdbl3 required numeric." : GoTo selesai
        End If
        'sgcustomdate1(38) As Date
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "sgcustomdate1 required date." : GoTo selesai
        End If
        'sgcustomdate2(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "sgcustomdate2 required date." : GoTo selesai
        End If
        'sgcustomdate3(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "sgcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sgcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sgcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sgcabang should not be more than 25 character." : GoTo selesai
        End If

        'sglokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sglokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sglokasi should not be more than 25 character." : GoTo selesai
        End If

        'sgsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sgsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "sgsumber should not be more than 10 character." : GoTo selesai
        End If

        'sgnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "sgnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "sgnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sgtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sgtgl can't be empty" : GoTo selesai
        End If

        'sgmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sgmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "sgmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sgkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "sgkurs can't be empty" : GoTo selesai
        End If

        'sgjumlah(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sgjumlah can't be empty" : GoTo selesai
        End If

        'sgjumlahvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "sgjumlahvalas can't be empty" : GoTo selesai
        End If

        'sginputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sginputtgl can't be empty" : GoTo selesai
        End If

        'sgmodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sgmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sgcustomdbl1(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sgcustomdbl1 can't be empty" : GoTo selesai
        End If

        'sgcustomdbl2(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sgcustomdbl2 can't be empty" : GoTo selesai
        End If

        'sgcustomdbl3(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sgcustomdbl3 can't be empty" : GoTo selesai
        End If

        'sgcustomdate1(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sgcustomdate1 can't be empty" : GoTo selesai
        End If

        'sgcustomdate2(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sgcustomdate2 can't be empty" : GoTo selesai
        End If

        'sgcustomdate3(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sgcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sgid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sglokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sguraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgstatussgc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sginputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sginputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "sgid~sgcabang~sglokasi~sgsumber~sgautonotransaksi~sgnotransaksi~sgtgl~sgkodepa~sgkontak~sgkontakperson~sguraian~sgcatatan~sgmatauang~sgkurs~sgjumlah~sgjumlahvalas~sgstatussgc~sgstatus~sgstatussebelumnya~sgjmlrevisi~sgcetakanke~sgisclose~sginputuser~sginputtgl~sgmodifikasiuser~sgmodifikasitgl~sgposting~sgcustomtext1~sgcustomtext2~sgcustomtext3~sgcustomtext4~sgcustomtext5~sgcustomint1~sgcustomint2~sgcustomint3~sgcustomdbl1~sgcustomdbl2~sgcustomdbl3~sgcustomdate1~sgcustomdate2~sgcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsgdetail(0) As Integer, idsg(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, statussgc(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsgdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsg", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "statussgc", AsEnumTypeData.AsInt64)
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
            'idsgdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsgdetail required numeric." : GoTo selesai
            End If
            'idsg(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsg required numeric." : GoTo selesai
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
            'statussgc(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - statussgc required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idsgdetail~idsg~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~statussgc~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
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
                Dim vModuleId As Integer = 2, vMenuId As Integer = 10
                Select Case drutama("sgstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sgtgl")), AsFormatTanggal(drutama("sgtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sgstatus") = 2 Or drutama("sgstatus") = 1 Or drutama("sgstatus") = 8 Or drutama("sgstatus") = 9 Or drutama("sgstatus") = 10 Or drutama("sgstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro, drutama("sgtgl"), formatTgl)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("sgjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("sgjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("sgid")
                    notransaksi = drutama("sgnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(sgid), sgnotransaksi FROM M2_sg WHERE sgid='" & result(4) & "' AND sgstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("sgautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sgcabang"), drutama("sglokasi"), drutama("sgsumber"), drutama("sgtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sgid) FROM m2_sg WHERE sgnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_sg_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Sg_HistorySimpan("" & paramSplit(0) & "★M2_Sg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sgsumber")) & "▼" & FixQuotes(drutama("sgid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Sg set sgcabang  = '" & FixQuotes(drutama("sgcabang")) & "', sglokasi  = '" & FixQuotes(drutama("sglokasi")) & "', sgsumber  = '" & FixQuotes(drutama("sgsumber")) & "', sgautonotransaksi  = " & drutama("sgautonotransaksi") & ", sgnotransaksi  = '" & notransaksi & "', sgtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sgtgl"))) & "', sgkodepa  = " & drutama("sgkodepa") & ", sgkontak  = " & drutama("sgkontak") & ", sgkontakperson  = '" & FixQuotes(drutama("sgkontakperson")) & "', sguraian  = '" & FixQuotes(drutama("sguraian")) & "', sgcatatan  = '" & FixQuotes(drutama("sgcatatan")) & "', sgmatauang  = '" & FixQuotes(drutama("sgmatauang")) & "', sgkurs  = '" & FixDouble(drutama("sgkurs")) & "', sgjumlah  = '" & FixDouble(drutama("sgjumlah")) & "', sgjumlahvalas  = '" & FixDouble(drutama("sgjumlahvalas")) & "', sgstatussgc  = " & drutama("sgstatussgc") & ", sgstatus  = " & drutama("sgstatus") & ", sgstatussebelumnya  = " & drutama("sgstatussebelumnya") & ", sgjmlrevisi  = sgjmlrevisi+1, sgcetakanke  = " & drutama("sgcetakanke") & ", sgisclose  = " & drutama("sgisclose") & ", sgmodifikasiuser  = " & drutama("sgmodifikasiuser") & ", sgmodifikasitgl  = NOW(), sgposting  = 0, sgcustomtext1  = '" & FixQuotes(drutama("sgcustomtext1")) & "', sgcustomtext2  = '" & FixQuotes(drutama("sgcustomtext2")) & "', sgcustomtext3  = '" & FixQuotes(drutama("sgcustomtext3")) & "', sgcustomtext4  = '" & FixQuotes(drutama("sgcustomtext4")) & "', sgcustomtext5  = '" & FixQuotes(drutama("sgcustomtext5")) & "', sgcustomint1  = " & drutama("sgcustomint1") & ", sgcustomint2  = " & drutama("sgcustomint2") & ", sgcustomint3  = " & drutama("sgcustomint3") & ", sgcustomdbl1  = '" & FixDouble(drutama("sgcustomdbl1")) & "', sgcustomdbl2  = '" & FixDouble(drutama("sgcustomdbl2")) & "', sgcustomdbl3  = '" & FixDouble(drutama("sgcustomdbl3")) & "', sgcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate1"))) & "', sgcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate2"))) & "', sgcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate3"))) & "' where sgid = '" & drutama("sgid") & "'"
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

                    If drutama("sgautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sgcabang"), drutama("sglokasi"), drutama("sgsumber"), drutama("sgtgl"))
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
                        notransaksi = drutama("sgnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(sgid) FROM m2_sg WHERE sgnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Sg (sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgcustomtext1, sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, sgcustomint3, sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3) values('" & FixQuotes(drutama("sgcabang")) & "', '" & FixQuotes(drutama("sglokasi")) & "', '" & FixQuotes(drutama("sgsumber")) & "', " & drutama("sgautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sgtgl"))) & "', " & drutama("sgkodepa") & ", " & drutama("sgkontak") & ", '" & FixQuotes(drutama("sgkontakperson")) & "', '" & FixQuotes(drutama("sguraian")) & "', '" & FixQuotes(drutama("sgcatatan")) & "', '" & FixQuotes(drutama("sgmatauang")) & "', '" & FixDouble(drutama("sgkurs")) & "', '" & FixDouble(drutama("sgjumlah")) & "', '" & FixDouble(drutama("sgjumlahvalas")) & "', " & drutama("sgstatussgc") & ", " & drutama("sgstatus") & ", " & drutama("sgstatussebelumnya") & ", " & drutama("sgjmlrevisi") & ", " & drutama("sgcetakanke") & ", " & drutama("sgisclose") & ", " & drutama("sginputuser") & ", NOW(), " & drutama("sgmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("sgcustomtext1")) & "', '" & FixQuotes(drutama("sgcustomtext2")) & "', '" & FixQuotes(drutama("sgcustomtext3")) & "', '" & FixQuotes(drutama("sgcustomtext4")) & "', '" & FixQuotes(drutama("sgcustomtext5")) & "', " & drutama("sgcustomint1") & ", " & drutama("sgcustomint2") & ", " & drutama("sgcustomint3") & ", '" & FixDouble(drutama("sgcustomdbl1")) & "', '" & FixDouble(drutama("sgcustomdbl2")) & "', '" & FixDouble(drutama("sgcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select sgid from M2_sg where sgnotransaksi='" & notransaksi & "' AND sginputuser= '" & userid & "' order by sgmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Sg_Detail where idsg = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsgdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("statussgc") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        'filter query untuk update status giro menjadi cair
                        If drutama("sgstatus") = 2 Then
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
                    sql = "Insert into M2_Sg_Detail(idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus,  gltglcair, glrekbank, glbank, glnoacbank m2_giro_list
                    If drutama("sgstatus") = 2 Then '  glstatus    , gltglcair                             , glrekbank                                                                 , glbank                                                           , glnoacbank                                                                              filter
                        sql = "UPDATE m2_giro_list SET glstatus = 1, gltglcair = '" & drutama("sgtgl") & "', glrekbank = (CASE glnogiro " & strRekbank.ToString & " ELSE glrekbank END), glbank = (CASE glnogiro " & strBank.ToString & " ELSE glbank END), glnoacbank = (CASE glnogiro " & strNoacbank.ToString & " ELSE glnoacbank END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "SG", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("sgstatus") = 2 Then
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
    Public Function M2_SgUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "Sg", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sgtgl, Sgnotransaksi, Sgstatus FROM m2_Sg WHERE Sgid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sgstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_sg_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Sg_HistorySimpan("" & paramSplit(0) & "★M2_Sg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                dtdetail = AsDataTableAmbilDariDBCon("SELECT nogiro FROM m2_sg_detail WHERE idsg = '" & idtransaksi & "'", myConn)
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
                    sql = "UPDATE m2_giro_list gl LEFT JOIN (SELECT sgcd.nogiro, sgc.sgctgl as tgl FROM m2_sgc_detail sgcd JOIN m2_sgc sgc ON sgcd.idsgc = sgc.sgcid AND sgc.sgcstatus IN(2,3,4,7) WHERE (" & strGiroBatal.ToString & ")) as gc ON gl.glnogiro = gc.nogiro SET gl.glstatus = gl.glstatussebelumnya, gl.gltglcair = (CASE gl.glstatussebelumnya WHEN 0 THEN '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "' ELSE IFNULL(gc.tgl,'1900-01-01') END) WHERE (" & strGiro.ToString & ")"
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M2_Sg SET Sgstatus = " & nilaiStatus & ", Sgmodifikasiuser='" & userid & "', Sgmodifikasitgl = NOW(), Sgposting = 0, Sgpostingtgl = '1971-01-01 00:00:00', Sgjmlrevisi = Sgjmlrevisi + 1 WHERE Sgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgSearch(PostWsSearch(paramSplit(0), "M2_SgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_SgDelete(ByVal param As String) As String

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
            Dim sumber As String = "Sg", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sgid, Sgnotransaksi FROM m2_Sg WHERE Sgid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl"
            sql &= " FROM M2_sg"
            sql &= " WHERE sgid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sgcabang")
                lokasi = dtNomorNext.Rows(0)("sglokasi")
                sumber = dtNomorNext.Rows(0)("sgsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sgautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sgnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sgtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Sg_Detail WHERE idSg = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Sg WHERE Sgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgSearch(PostWsSearch(paramSplit(0), "M2_SgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_SgGetdataById(ByVal param As String) As String

        'M2_SgGetdataById Utama --------------------------------------------------------
        'sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, 
        'sgcustomtext1, sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, 
        'sgcustomint3, sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3, 
        'sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, 
        'sgmodifikasiusernama

        'M2_SgGetdataById Detail -------------------------------------------------------
        'idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, 
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

        Dim NmMemcached As String = "aplikasi1-M2_Sg~M2_Sg_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sgid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sgid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sg_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sgid"), 0), sptField,
                     FxDB(drutama("sgcabang"), ""), sptField,
                     FxDB(drutama("sglokasi"), ""), sptField,
                     FxDB(drutama("sgsumber"), ""), sptField,
                     FxDB(drutama("sgautonotransaksi"), 0), sptField,
                     FxDB(drutama("sgnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sgtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sgkodepa"), 0), sptField,
                     FxDB(drutama("sgkontak"), 0), sptField,
                     FxDB(drutama("sgkontakperson"), ""), sptField,
                     FxDB(drutama("sguraian"), ""), sptField,
                     FxDB(drutama("sgcatatan"), ""), sptField,
                     FxDB(drutama("sgmatauang"), ""), sptField,
                     FxDB(drutama("sgkurs"), 0), sptField,
                     FxDB(drutama("sgjumlah"), 0), sptField,
                     FxDB(drutama("sgjumlahvalas"), 0), sptField,
                     FxDB(drutama("sgstatussgc"), 0), sptField,
                     FxDB(drutama("sgstatus"), 0), sptField,
                     FxDB(drutama("sgstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sgjmlrevisi"), 0), sptField,
                     FxDB(drutama("sgcetakanke"), 0), sptField,
                     FxDB(drutama("sgisclose"), 0), sptField,
                     FxDB(drutama("sginputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgcustomtext1"), ""), sptField,
                     FxDB(drutama("sgcustomtext2"), ""), sptField,
                     FxDB(drutama("sgcustomtext3"), ""), sptField,
                     FxDB(drutama("sgcustomtext4"), ""), sptField,
                     FxDB(drutama("sgcustomtext5"), ""), sptField,
                     FxDB(drutama("sgcustomint1"), 0), sptField,
                     FxDB(drutama("sgcustomint2"), 0), sptField,
                     FxDB(drutama("sgcustomint3"), 0), sptField,
                     FxDB(drutama("sgcustomdbl1"), 0), sptField,
                     FxDB(drutama("sgcustomdbl2"), 0), sptField,
                     FxDB(drutama("sgcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sgcabangnama"), ""), sptField,
                     FxDB(drutama("sglokasinama"), ""), sptField,
                     FxDB(drutama("sgkontakkode"), ""), sptField,
                     FxDB(drutama("sgkontaknama"), ""), sptField,
                     FxDB(drutama("sgstatusnama"), ""), sptField,
                     FxDB(drutama("sgstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sginputusernama"), ""), sptField,
                     FxDB(drutama("sgmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsgdetail"), 0), sptField,
                     FxDB(dr("idsg"), 0), sptField,
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
                     FxDB(dr("statussgc"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, sgcustomtext1, sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, sgcustomint3, sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3, sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, sgmodifikasiusernama" & sptSubParam & "idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_SgSearch(ByVal param As String) As String
        'M2_SgSearch --------------------------------------------------------
        'sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, 
        'sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, 
        'sgmodifikasiusernama

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
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sg_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Sg", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sgid"), 0), sptField,
                     FxDB(dr("sgcabang"), ""), sptField,
                     FxDB(dr("sglokasi"), ""), sptField,
                     FxDB(dr("sgsumber"), ""), sptField,
                     FxDB(dr("sgautonotransaksi"), 0), sptField,
                     FxDB(dr("sgnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sgtgl"), ""), formatTgl), sptField,
                     FxDB(dr("sgkodepa"), 0), sptField,
                     FxDB(dr("sgkontak"), 0), sptField,
                     FxDB(dr("sgkontakperson"), ""), sptField,
                     FxDB(dr("sguraian"), ""), sptField,
                     FxDB(dr("sgcatatan"), ""), sptField,
                     FxDB(dr("sgmatauang"), ""), sptField,
                     FxDB(dr("sgkurs"), 0), sptField,
                     FxDB(dr("sgjumlah"), 0), sptField,
                     FxDB(dr("sgjumlahvalas"), 0), sptField,
                     FxDB(dr("sgstatussgc"), 0), sptField,
                     FxDB(dr("sgstatus"), 0), sptField,
                     FxDB(dr("sgstatussebelumnya"), 0), sptField,
                     FxDB(dr("sgjmlrevisi"), 0), sptField,
                     FxDB(dr("sgcetakanke"), 0), sptField,
                     FxDB(dr("sgisclose"), 0), sptField,
                     FxDB(dr("sginputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgcabangnama"), ""), sptField,
                     FxDB(dr("sglokasinama"), ""), sptField,
                     FxDB(dr("sgkontakkode"), ""), sptField,
                     FxDB(dr("sgkontaknama"), ""), sptField,
                     FxDB(dr("sgstatusnama"), ""), sptField,
                     FxDB(dr("sgstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sginputusernama"), ""), sptField,
                     FxDB(dr("sgmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, sgmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_Sg_DetailSearch(ByVal param As String) As String
        'M2_Sg_DetailSearch --------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, glkontakkode, glkontaknama, glrekbank, glrekbanknama, 
        'glrekgiro, glrekgironama, gljenis, gljenisnama, glbank, glbanknama, glnoacbank, 
        'glurutan, glstatus, glstatusnama, gljumlah, gljumlahvalas, glmatauang, gltgljthtempo, 
        'gltglcair, idsgdetail

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
        sql = query.PanggilQuery("m2_sg_detail_v")
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
                     FxDB(dr("idsgdetail"), 0), sptRow)
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
    Public Function M2_SgTerkait(ByVal param As String) As String
        'M2_SgTerkait --------------------------------------------------------
        'sgid, sgnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "sgid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sg_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sgid"), 0), sptField,
                     FxDB(dr("sgnotransaksi"), ""), sptField,
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
            result(2) = "Related SG data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgid, sgnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

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
            sql = "SELECT glnogiro, sgnotransaksi FROM m2_giro_list JOIN m2_sg_detail ON glnogiro=nogiro JOIN m2_sg ON idsg=sgid WHERE (glstatus = 1) AND (sgstatus=2 OR sgstatus=3 OR sgstatus=4 OR sgstatus=7) AND (" & filter & ") LIMIT 1"
            dtvalidasi = AsDataTableAmbilDariDB(sql)
            If (dtvalidasi.Rows.Count > 0) Then
                hasil = "Giro : " & dtvalidasi.Rows(0)(0) & " - has disbursed in transaction : " & dtvalidasi.Rows(0)(1) : GoTo selesai
            End If

            'CEK TGL PENCAIRAN GIRO < TGL TOLAKAN GIRO
            sql = "SELECT glnogiro, sgcnotransaksi, sgctgl FROM m2_giro_list JOIN m2_sgc_detail ON glnogiro = nogiro JOIN m2_sgc ON idsgc = sgcid WHERE (glstatus = 2 OR glstatus = 3) AND (sgcstatus = 2 OR sgcstatus = 3 OR sgcstatus = 4 OR sgcstatus = 7) AND sgctgl > '" & FixQuotes(AsFormatTanggal(tgl)) & "' AND (" & filter & ") LIMIT 1"
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
    Public Function M2_SgSimpanOld(ByVal param As String) As String
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
        'sgid(0) As Integer, sgcabang(1) As String, sglokasi(2) As String, sgsumber(3) As String, sgautonotransaksi(4) As Integer, 
        'sgnotransaksi(5) As String, sgtgl(6) As Date, sgkodepa(7) As Integer, sgkontak(8) As Integer, sgkontakperson(9) As String, 
        'sguraian(10) As String, sgcatatan(11) As String, sgmatauang(12) As String, sgkurs(13) As Double, sgjumlah(14) As Double, 
        'sgjumlahvalas(15) As Double, sgstatussgc(16) As Integer, sgstatus(17) As Integer, sgstatussebelumnya(18) As Integer, sgjmlrevisi(19) As Integer, 
        'sgcetakanke(20) As Integer, sgisclose(21) As Integer, sginputuser(22) As Integer, sginputtgl(23) As DateTime, sgmodifikasiuser(24) As Integer, 
        'sgmodifikasitgl(25) As DateTime, sgposting(26) As Integer, sgcustomtext1(27) As String, sgcustomtext2(28) As String, sgcustomtext3(29) As String, 
        'sgcustomtext4(30) As String, sgcustomtext5(31) As String, sgcustomint1(32) As Integer, sgcustomint2(33) As Integer, sgcustomint3(34) As Integer, 
        'sgcustomdbl1(35) As Double, sgcustomdbl2(36) As Double, sgcustomdbl3(37) As Double, sgcustomdate1(38) As Date, sgcustomdate2(39) As Date, 
        'sgcustomdate3(40) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgcustomtext1, 
        'sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, sgcustomint3, 
        'sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 41) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'sgid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "sgid required numeric." : GoTo selesai
        End If
        'sgautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "sgautonotransaksi required numeric." : GoTo selesai
        End If
        'sgtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "sgtgl required date." : GoTo selesai
        End If
        'sgkodepa(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sgkodepa required numeric." : GoTo selesai
        End If
        'sgkontak(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "sgkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(8) < 1) Then
            result(2) = "sgkontak can't be empty." : GoTo selesai
        End If
        'sgkurs(13) As Double
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sgkurs required numeric." : GoTo selesai
        End If
        'sgjumlah(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "sgjumlah required numeric." : GoTo selesai
        End If
        'sgjumlahvalas(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "sgjumlahvalas required numeric." : GoTo selesai
        End If
        'sgstatussgc(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "sgstatussgc required numeric." : GoTo selesai
        End If
        'sgstatus(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "sgstatus required numeric." : GoTo selesai
        End If
        'sgstatussebelumnya(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "sgstatussebelumnya required numeric." : GoTo selesai
        End If
        'sgjmlrevisi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "sgjmlrevisi required numeric." : GoTo selesai
        End If
        'sgcetakanke(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "sgcetakanke required numeric." : GoTo selesai
        End If
        'sgisclose(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "sgisclose required numeric." : GoTo selesai
        End If
        'sginputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sginputuser required numeric." : GoTo selesai
        End If
        'sginputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "sginputtgl required date." : GoTo selesai
        End If
        'sgmodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "sgmodifikasiuser required numeric." : GoTo selesai
        End If
        'sgmodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "sgmodifikasitgl required date." : GoTo selesai
        End If
        'sgposting(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "sgposting required numeric." : GoTo selesai
        End If
        'sgcustomint1(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "sgcustomint1 required numeric." : GoTo selesai
        End If
        'sgcustomint2(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sgcustomint2 required numeric." : GoTo selesai
        End If
        'sgcustomint3(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sgcustomint3 required numeric." : GoTo selesai
        End If
        'sgcustomdbl1(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sgcustomdbl1 required numeric." : GoTo selesai
        End If
        'sgcustomdbl2(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "sgcustomdbl2 required numeric." : GoTo selesai
        End If
        'sgcustomdbl3(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sgcustomdbl3 required numeric." : GoTo selesai
        End If
        'sgcustomdate1(38) As Date
        If (IsDate(dataUtama(38)) = False) Then
            result(2) = "sgcustomdate1 required date." : GoTo selesai
        End If
        'sgcustomdate2(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "sgcustomdate2 required date." : GoTo selesai
        End If
        'sgcustomdate3(40) As Date
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "sgcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'sgcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "sgcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "sgcabang should not be more than 25 character." : GoTo selesai
        End If

        'sglokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "sglokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "sglokasi should not be more than 25 character." : GoTo selesai
        End If

        'sgsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sgsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "sgsumber should not be more than 10 character." : GoTo selesai
        End If

        'sgnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "sgnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "sgnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sgtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "sgtgl can't be empty" : GoTo selesai
        End If

        'sgmatauang(12) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sgmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 25 Then
            result(2) = "sgmatauang should not be more than 25 character." : GoTo selesai
        End If

        'sgkurs(13) As Double
        If Len(dataUtama(13)) = 0 Then
            result(2) = "sgkurs can't be empty" : GoTo selesai
        End If

        'sgjumlah(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "sgjumlah can't be empty" : GoTo selesai
        End If

        'sgjumlahvalas(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "sgjumlahvalas can't be empty" : GoTo selesai
        End If

        'sginputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "sginputtgl can't be empty" : GoTo selesai
        End If

        'sgmodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "sgmodifikasitgl can't be empty" : GoTo selesai
        End If

        'sgcustomdbl1(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sgcustomdbl1 can't be empty" : GoTo selesai
        End If

        'sgcustomdbl2(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sgcustomdbl2 can't be empty" : GoTo selesai
        End If

        'sgcustomdbl3(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sgcustomdbl3 can't be empty" : GoTo selesai
        End If

        'sgcustomdate1(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sgcustomdate1 can't be empty" : GoTo selesai
        End If

        'sgcustomdate2(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sgcustomdate2 can't be empty" : GoTo selesai
        End If

        'sgcustomdate3(40) As Date
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sgcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "sgid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sglokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sguraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgjumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgjumlahvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgstatussgc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sginputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sginputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sgcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sgcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "sgid~sgcabang~sglokasi~sgsumber~sgautonotransaksi~sgnotransaksi~sgtgl~sgkodepa~sgkontak~sgkontakperson~sguraian~sgcatatan~sgmatauang~sgkurs~sgjumlah~sgjumlahvalas~sgstatussgc~sgstatus~sgstatussebelumnya~sgjmlrevisi~sgcetakanke~sgisclose~sginputuser~sginputtgl~sgmodifikasiuser~sgmodifikasitgl~sgposting~sgcustomtext1~sgcustomtext2~sgcustomtext3~sgcustomtext4~sgcustomtext5~sgcustomint1~sgcustomint2~sgcustomint3~sgcustomdbl1~sgcustomdbl2~sgcustomdbl3~sgcustomdate1~sgcustomdate2~sgcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsgdetail(0) As Integer, idsg(1) As Integer, nogiro(2) As String, kontak(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, jumlah(6) As Double, jumlahvalas(7) As Double, bank(8) As String, noacbank(9) As String, 
        'rekbank(10) As String, rekgiro(11) As String, tgljatuhtempo(12) As Date, catatan(13) As String, urutan(14) As Integer, 
        'statusgiro(15) As Integer, statussgc(16) As Integer, isclose(17) As Integer, customtext1(18) As String, customtext2(19) As String, 
        'customtext3(20) As String, customdbl1(21) As Double, customdbl2(22) As Double, customdbl3(23) As Double, customdate1(24) As Date, 
        'customdate2(25) As Date, customdate3(26) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsgdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsg", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "statussgc", AsEnumTypeData.AsInt64)
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
            'idsgdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsgdetail required numeric." : GoTo selesai
            End If
            'idsg(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsg required numeric." : GoTo selesai
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
            'statussgc(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - statussgc required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idsgdetail~idsg~nogiro~kontak~matauang~kurs~jumlah~jumlahvalas~bank~noacbank~rekbank~rekgiro~tgljatuhtempo~catatan~urutan~statusgiro~statussgc~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26)) = False Then
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sgtgl")), AsFormatTanggal(drutama("sgtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sgstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(ftExistGiro, ftGiro, drutama("sgtgl"), formatTgl)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================

                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("sgjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("sgjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============

                If isUpdate Then
                    result(4) = drutama("sgid")
                    notransaksi = drutama("sgnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(sgid), sgnotransaksi FROM M2_sg WHERE sgid='" & result(4) & "' AND sgstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sgid) FROM m2_sg WHERE sgnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m2_sg_history
                        Dim rsSimpanHistory As String = SimpanHistory.M2_Sg_HistorySimpan("" & paramSplit(0) & "★M2_Sg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sgsumber")) & "▼" & FixQuotes(drutama("sgid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M2_Sg set sgcabang  = '" & FixQuotes(drutama("sgcabang")) & "', sglokasi  = '" & FixQuotes(drutama("sglokasi")) & "', sgsumber  = '" & FixQuotes(drutama("sgsumber")) & "', sgautonotransaksi  = " & drutama("sgautonotransaksi") & ", sgnotransaksi  = '" & notransaksi & "', sgtgl  = '" & FixQuotes(AsFormatTanggal(drutama("sgtgl"))) & "', sgkodepa  = " & drutama("sgkodepa") & ", sgkontak  = " & drutama("sgkontak") & ", sgkontakperson  = '" & FixQuotes(drutama("sgkontakperson")) & "', sguraian  = '" & FixQuotes(drutama("sguraian")) & "', sgcatatan  = '" & FixQuotes(drutama("sgcatatan")) & "', sgmatauang  = '" & FixQuotes(drutama("sgmatauang")) & "', sgkurs  = '" & FixDouble(drutama("sgkurs")) & "', sgjumlah  = '" & FixDouble(drutama("sgjumlah")) & "', sgjumlahvalas  = '" & FixDouble(drutama("sgjumlahvalas")) & "', sgstatussgc  = " & drutama("sgstatussgc") & ", sgstatus  = " & drutama("sgstatus") & ", sgstatussebelumnya  = " & drutama("sgstatussebelumnya") & ", sgjmlrevisi  = sgjmlrevisi+1, sgcetakanke  = " & drutama("sgcetakanke") & ", sgisclose  = " & drutama("sgisclose") & ", sgmodifikasiuser  = " & drutama("sgmodifikasiuser") & ", sgmodifikasitgl  = NOW(), sgposting  = 0, sgcustomtext1  = '" & FixQuotes(drutama("sgcustomtext1")) & "', sgcustomtext2  = '" & FixQuotes(drutama("sgcustomtext2")) & "', sgcustomtext3  = '" & FixQuotes(drutama("sgcustomtext3")) & "', sgcustomtext4  = '" & FixQuotes(drutama("sgcustomtext4")) & "', sgcustomtext5  = '" & FixQuotes(drutama("sgcustomtext5")) & "', sgcustomint1  = " & drutama("sgcustomint1") & ", sgcustomint2  = " & drutama("sgcustomint2") & ", sgcustomint3  = " & drutama("sgcustomint3") & ", sgcustomdbl1  = '" & FixDouble(drutama("sgcustomdbl1")) & "', sgcustomdbl2  = '" & FixDouble(drutama("sgcustomdbl2")) & "', sgcustomdbl3  = '" & FixDouble(drutama("sgcustomdbl3")) & "', sgcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate1"))) & "', sgcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate2"))) & "', sgcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate3"))) & "' where sgid = '" & drutama("sgid") & "'"
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

                    If drutama("sgautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("sgcabang"), drutama("sglokasi"), drutama("sgsumber"), drutama("sgtgl"))
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
                        notransaksi = drutama("sgnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(sgid) FROM m2_sg WHERE sgnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M2_Sg (sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgcustomtext1, sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, sgcustomint3, sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3) values('" & FixQuotes(drutama("sgcabang")) & "', '" & FixQuotes(drutama("sglokasi")) & "', '" & FixQuotes(drutama("sgsumber")) & "', " & drutama("sgautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("sgtgl"))) & "', " & drutama("sgkodepa") & ", " & drutama("sgkontak") & ", '" & FixQuotes(drutama("sgkontakperson")) & "', '" & FixQuotes(drutama("sguraian")) & "', '" & FixQuotes(drutama("sgcatatan")) & "', '" & FixQuotes(drutama("sgmatauang")) & "', '" & FixDouble(drutama("sgkurs")) & "', '" & FixDouble(drutama("sgjumlah")) & "', '" & FixDouble(drutama("sgjumlahvalas")) & "', " & drutama("sgstatussgc") & ", " & drutama("sgstatus") & ", " & drutama("sgstatussebelumnya") & ", " & drutama("sgjmlrevisi") & ", " & drutama("sgcetakanke") & ", " & drutama("sgisclose") & ", " & drutama("sginputuser") & ", NOW(), " & drutama("sgmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("sgcustomtext1")) & "', '" & FixQuotes(drutama("sgcustomtext2")) & "', '" & FixQuotes(drutama("sgcustomtext3")) & "', '" & FixQuotes(drutama("sgcustomtext4")) & "', '" & FixQuotes(drutama("sgcustomtext5")) & "', " & drutama("sgcustomint1") & ", " & drutama("sgcustomint2") & ", " & drutama("sgcustomint3") & ", '" & FixDouble(drutama("sgcustomdbl1")) & "', '" & FixDouble(drutama("sgcustomdbl2")) & "', '" & FixDouble(drutama("sgcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sgcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select sgid from M2_sg where sgnotransaksi='" & notransaksi & "' AND sginputuser= '" & userid & "' order by sgmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M2_Sg_Detail where idsg = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsgdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("nogiro")) & "', " & dr1("kontak") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljatuhtempo"))) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("statusgiro") & ", " & dr1("statussgc") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                        'filter query untuk update status giro menjadi cair
                        If drutama("sgstatus") = 2 Then
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
                    sql = "Insert into M2_Sg_Detail(idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update glstatus,  gltglcair, glrekbank, glbank, glnoacbank m2_giro_list
                    If drutama("sgstatus") = 2 Then '  glstatus    , gltglcair                             , glrekbank                                                                 , glbank                                                           , glnoacbank                                                                              filter
                        sql = "UPDATE m2_giro_list SET glstatus = 1, gltglcair = '" & drutama("sgtgl") & "', glrekbank = (CASE glnogiro " & strRekbank.ToString & " ELSE glrekbank END), glbank = (CASE glnogiro " & strBank.ToString & " ELSE glbank END), glnoacbank = (CASE glnogiro " & strNoacbank.ToString & " ELSE glnoacbank END) WHERE " & strGiro.ToString & ""
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
                Dim sumber As String = "SG", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("sgstatus") = 2 Then
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
    Public Function M2_SgUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Sg", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sgtgl, Sgnotransaksi, Sgstatus FROM m2_Sg WHERE Sgid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sgstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m2_sg_history
            Dim rsSimpanHistory As String = SimpanHistory.M2_Sg_HistorySimpan("" & paramSplit(0) & "★M2_Sg_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                dtdetail = AsDataTableAmbilDariDB("SELECT nogiro FROM m2_sg_detail WHERE idsg = '" & idtransaksi & "'")
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
                    sql = "UPDATE m2_giro_list gl LEFT JOIN (SELECT sgcd.nogiro, sgc.sgctgl as tgl FROM m2_sgc_detail sgcd JOIN m2_sgc sgc ON sgcd.idsgc = sgc.sgcid AND sgc.sgcstatus IN(2,3,4,7) WHERE (" & strGiroBatal.ToString & ")) as gc ON gl.glnogiro = gc.nogiro SET gl.glstatus = gl.glstatussebelumnya, gl.gltglcair = (CASE gl.glstatussebelumnya WHEN 0 THEN '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "' ELSE IFNULL(gc.tgl,'1900-01-01') END) WHERE (" & strGiro.ToString & ")"
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M2_Sg SET Sgstatus = " & nilaiStatus & ", Sgmodifikasiuser='" & userid & "', Sgmodifikasitgl = NOW(), Sgposting = 0, Sgpostingtgl = '1971-01-01 00:00:00', Sgjmlrevisi = Sgjmlrevisi + 1 WHERE Sgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgSearch(PostWsSearch(paramSplit(0), "M2_SgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M2_SgDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Sg", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Sgid, Sgnotransaksi FROM m2_Sg WHERE Sgid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl"
            sql &= " FROM M2_sg"
            sql &= " WHERE sgid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("sgcabang")
                lokasi = dtNomorNext.Rows(0)("sglokasi")
                sumber = dtNomorNext.Rows(0)("sgsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("sgautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sgnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sgtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SG' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M2_Sg_Detail WHERE idSg = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M2_Sg WHERE Sgid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M2_SgSearch(PostWsSearch(paramSplit(0), "M2_SgSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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