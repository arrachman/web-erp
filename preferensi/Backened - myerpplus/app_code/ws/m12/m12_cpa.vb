Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_cpa
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_CpaSimpan(ByVal param As String) As String
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

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, tglLunas As String = ""

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

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
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
        'cpaid(0) As , cpacabang(1) As String, cpalokasi(2) As String, cpasumber(3) As String, cpaautonotransaksi(4) As Integer, 
        'cpanotransaksi(5) As String, cpatgl(6) As Date, cpakodepa(7) As , cpakontak(8) As , cpakontakperson(9) As String, 
        'cpauraian(10) As String, cpacatatan(11) As String, cpastatus(12) As Integer, cpastatussebelumnya(13) As Integer, cpajmlrevisi(14) As Integer, 
        'cpacetakanke(15) As Integer, cpaisclose(16) As Integer, cpainputuser(17) As , cpainputtgl(18) As DateTime, cpamodifikasiuser(19) As , 
        'cpamodifikasitgl(20) As DateTime, cpaposting(21) As Integer, cpapostingtgl(22) As DateTime, cpacustomtext1(23) As String, cpacustomtext2(24) As String, 
        'cpacustomtext3(25) As String, cpacustomtext4(26) As String, cpacustomtext5(27) As String, cpacustomint1(28) As Integer, cpacustomint2(29) As Integer, 
        'cpacustomint3(30) As Integer, cpacustomdbl1(31) As Double, cpacustomdbl2(32) As Double, cpacustomdbl3(33) As Double, cpacustomdate1(34) As Date, 
        'cpacustomdate2(35) As Date, cpacustomdate3(36) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, 
        'cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, 
        'cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, 
        'cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, 
        'cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, 
        'cpacustomdate2, cpacustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 37) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================



        'VALIDASI TIPE DATA UTAMA ==========================================================
        'cpaautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "cpaautonotransaksi required numeric." : GoTo selesai
        End If
        'cpatgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "cpatgl required date." : GoTo selesai
        End If
        'cpastatus(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "cpastatus required numeric." : GoTo selesai
        End If
        'cpastatussebelumnya(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "cpastatussebelumnya required numeric." : GoTo selesai
        End If
        'cpajmlrevisi(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "cpajmlrevisi required numeric." : GoTo selesai
        End If
        'cpacetakanke(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "cpacetakanke required numeric." : GoTo selesai
        End If
        'cpaisclose(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "cpaisclose required numeric." : GoTo selesai
        End If
        'cpainputtgl(18) As DateTime
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "cpainputtgl required date." : GoTo selesai
        End If
        'cpamodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "cpamodifikasitgl required date." : GoTo selesai
        End If
        'cpaposting(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "cpaposting required numeric." : GoTo selesai
        End If
        'cpapostingtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "cpapostingtgl required date." : GoTo selesai
        End If
        'cpacustomint1(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "cpacustomint1 required numeric." : GoTo selesai
        End If
        'cpacustomint2(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "cpacustomint2 required numeric." : GoTo selesai
        End If
        'cpacustomint3(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "cpacustomint3 required numeric." : GoTo selesai
        End If
        'cpacustomdbl1(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "cpacustomdbl1 required numeric." : GoTo selesai
        End If
        'cpacustomdbl2(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "cpacustomdbl2 required numeric." : GoTo selesai
        End If
        'cpacustomdbl3(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "cpacustomdbl3 required numeric." : GoTo selesai
        End If
        'cpacustomdate1(34) As Date
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "cpacustomdate1 required date." : GoTo selesai
        End If
        'cpacustomdate2(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "cpacustomdate2 required date." : GoTo selesai
        End If
        'cpacustomdate3(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "cpacustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'cpaid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "cpaid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "cpaid should not be more than 20 character." : GoTo selesai
        End If

        'cpacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cpacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cpacabang should not be more than 25 character." : GoTo selesai
        End If

        'cpalokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cpalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cpalokasi should not be more than 25 character." : GoTo selesai
        End If

        'cpasumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cpasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "cpasumber should not be more than 10 character." : GoTo selesai
        End If

        'cpanotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "cpanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "cpanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cpatgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cpatgl can't be empty" : GoTo selesai
        End If

        'cpakodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "cpakodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "cpakodepa should not be more than 20 character." : GoTo selesai
        End If

        'cpakontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "cpakontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "cpakontak should not be more than 20 character." : GoTo selesai
        End If

        'cpainputtgl(18) As DateTime
        If Len(dataUtama(18)) = 0 Then
            result(2) = "cpainputtgl can't be empty" : GoTo selesai
        End If

        'cpamodifikasitgl(20) As DateTime
        If Len(dataUtama(20)) = 0 Then
            result(2) = "cpamodifikasitgl can't be empty" : GoTo selesai
        End If

        'cpapostingtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "cpapostingtgl can't be empty" : GoTo selesai
        End If

        'cpacustomdbl1(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "cpacustomdbl1 can't be empty" : GoTo selesai
        End If

        'cpacustomdbl2(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "cpacustomdbl2 can't be empty" : GoTo selesai
        End If

        'cpacustomdbl3(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "cpacustomdbl3 can't be empty" : GoTo selesai
        End If

        'cpacustomdate1(34) As Date
        If Len(dataUtama(34)) = 0 Then
            result(2) = "cpacustomdate1 can't be empty" : GoTo selesai
        End If

        'cpacustomdate2(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "cpacustomdate2 can't be empty" : GoTo selesai
        End If

        'cpacustomdate3(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "cpacustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "cpaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpaautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpatgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpakontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpakontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpaisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpaposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpapostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "cpaid~cpacabang~cpalokasi~cpasumber~cpaautonotransaksi~cpanotransaksi~cpatgl~cpakodepa~cpakontak~cpakontakperson~cpauraian~cpacatatan~cpastatus~cpastatussebelumnya~cpajmlrevisi~cpacetakanke~cpaisclose~cpainputuser~cpainputtgl~cpamodifikasiuser~cpamodifikasitgl~cpaposting~cpapostingtgl~cpacustomtext1~cpacustomtext2~cpacustomtext3~cpacustomtext4~cpacustomtext5~cpacustomint1~cpacustomint2~cpacustomint3~cpacustomdbl1~cpacustomdbl2~cpacustomdbl3~cpacustomdate1~cpacustomdate2~cpacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcpadetail(0) As , idcpa(1) As , kontak(2) As , poinlama(3) As Double, poinmasuk(4) As Double, 
        'poinkeluar(5) As Double, poinbaru(6) As Double, catatan(7) As String, urutan(8) As Integer, isclose(9) As Integer, 
        'customtext1(10) As String, customtext2(11) As String, customtext3(12) As String, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcpadetail, idcpa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcpadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "poinlama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "poinmasuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "poinkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "poinbaru", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'poinlama(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "poinlama required numeric." : GoTo selesai
            End If
            'poinmasuk(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "poinmasuk required numeric." : GoTo selesai
            End If
            'poinkeluar(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "poinkeluar required numeric." : GoTo selesai
            End If
            'poinbaru(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "poinbaru required numeric." : GoTo selesai
            End If
            'urutan(8) As Integer
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(9) As Integer
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idcpadetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idcpadetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idcpadetail should not be more than 20 character." : GoTo selesai
            End If

            'idcpa(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idcpa can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idcpa should not be more than 20 character." : GoTo selesai
            End If

            'kontak(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kontak can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - kontak should not be more than 20 character." : GoTo selesai
            End If

            'poinlama(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - poinlama can't be empty" : GoTo selesai
            End If

            'poinmasuk(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - poinmasuk can't be empty" : GoTo selesai
            End If

            'poinkeluar(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - poinkeluar can't be empty" : GoTo selesai
            End If

            'poinbaru(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - poinbaru can't be empty" : GoTo selesai
            End If

            'customdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idcpadetail~idcpa~kontak~poinlama~poinmasuk~poinkeluar~poinbaru~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 25
                Select Case drutama("cpastatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("cpatgl")), AsFormatTanggal(drutama("cpatgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                If isUpdate Then
                    result(4) = drutama("cpaid")
                    notransaksi = drutama("cpanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(cpaid), cpanotransaksi FROM M_12_Cpa WHERE cpaid='" & result(4) & "' AND cpastatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(cpaid) FROM M_12_Cpa WHERE cpanotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_cpa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M12_Cpa_HistorySimpan("" & paramSplit(0) & "★M12_Cpa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("cpasumber")) & "▼" & FixQuotes(drutama("cpaid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Cpa set cpacabang  = '" & FixQuotes(drutama("cpacabang")) & "', cpalokasi  = '" & FixQuotes(drutama("cpalokasi")) & "', cpasumber  = '" & FixQuotes(drutama("cpasumber")) & "', cpaautonotransaksi  = " & drutama("cpaautonotransaksi") & ", cpanotransaksi  = '" & FixQuotes(notransaksi) & "', cpatgl  = '" & FixQuotes(AsFormatTanggal(drutama("cpatgl"))) & "', cpakodepa  = '" & FixQuotes(drutama("cpakodepa")) & "', cpakontak  = '" & FixQuotes(drutama("cpakontak")) & "', cpakontakperson  = '" & FixQuotes(drutama("cpakontakperson")) & "', cpauraian  = '" & FixQuotes(drutama("cpauraian")) & "', cpacatatan  = '" & FixQuotes(drutama("cpacatatan")) & "', cpastatus  = " & drutama("cpastatus") & ", cpastatussebelumnya  = " & drutama("cpastatussebelumnya") & ", cpajmlrevisi  = cpajmlrevisi+1, cpacetakanke  = " & drutama("cpacetakanke") & ", cpaisclose  = " & drutama("cpaisclose") & ", cpamodifikasiuser  = '" & FixQuotes(drutama("cpamodifikasiuser")) & "', cpamodifikasitgl  = NOW(), cpacustomtext1  = '" & FixQuotes(drutama("cpacustomtext1")) & "', cpacustomtext2  = '" & FixQuotes(drutama("cpacustomtext2")) & "', cpacustomtext3  = '" & FixQuotes(drutama("cpacustomtext3")) & "', cpacustomtext4  = '" & FixQuotes(drutama("cpacustomtext4")) & "', cpacustomtext5  = '" & FixQuotes(drutama("cpacustomtext5")) & "', cpacustomint1  = " & drutama("cpacustomint1") & ", cpacustomint2  = " & drutama("cpacustomint2") & ", cpacustomint3  = " & drutama("cpacustomint3") & ", cpacustomdbl1  = '" & FixDouble(drutama("cpacustomdbl1")) & "', cpacustomdbl2  = '" & FixDouble(drutama("cpacustomdbl2")) & "', cpacustomdbl3  = '" & FixDouble(drutama("cpacustomdbl3")) & "', cpacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate1"))) & "', cpacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate2"))) & "', cpacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate3"))) & "' where cpaid = '" & drutama("cpaid") & "'"
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

                    If drutama("cpaautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cpacabang"), drutama("cpalokasi"), drutama("cpasumber"), drutama("cpatgl"))
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
                        notransaksi = drutama("cpanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(cpaid) FROM M_12_cpa WHERE cpanotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_cpa (cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, cpacustomdate2, cpacustomdate3) values('" & FixQuotes(drutama("cpacabang")) & "', '" & FixQuotes(drutama("cpalokasi")) & "', '" & FixQuotes(drutama("cpasumber")) & "', " & drutama("cpaautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpatgl"))) & "', '" & FixQuotes(drutama("cpakodepa")) & "', '" & FixQuotes(drutama("cpakontak")) & "', '" & FixQuotes(drutama("cpakontakperson")) & "', '" & FixQuotes(drutama("cpauraian")) & "', '" & FixQuotes(drutama("cpacatatan")) & "', " & drutama("cpastatus") & ", " & drutama("cpastatussebelumnya") & ", " & drutama("cpajmlrevisi") & ", " & drutama("cpacetakanke") & ", " & drutama("cpaisclose") & ", '" & FixQuotes(drutama("cpainputuser")) & "', NOW(), 0, '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("cpacustomtext1")) & "', '" & FixQuotes(drutama("cpacustomtext2")) & "', '" & FixQuotes(drutama("cpacustomtext3")) & "', '" & FixQuotes(drutama("cpacustomtext4")) & "', '" & FixQuotes(drutama("cpacustomtext5")) & "', " & drutama("cpacustomint1") & ", " & drutama("cpacustomint2") & ", " & drutama("cpacustomint3") & ", '" & FixDouble(drutama("cpacustomdbl1")) & "', '" & FixDouble(drutama("cpacustomdbl2")) & "', '" & FixDouble(drutama("cpacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select cpaid from M_12_cpa where cpanotransaksi='" & notransaksi & "' AND cpainputuser= '" & userid & "' order by cpamodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_cpa_Detail where idcpa = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idcpadetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("kontak")) & "', '" & FixDouble(dr1("poinlama")) & "', '" & FixDouble(dr1("poinmasuk")) & "', '" & FixDouble(dr1("poinkeluar")) & "', '" & FixDouble(dr1("poinbaru")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_cpa_Detail(idcpadetail, idcpa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("cpastatus") = 2 Then
                    'UPDATE POIN PELANGGAN ==========================================================
                    'sql = "UPDATE m1_contact_point cp JOIN m_12_cpa_detail cpad ON cp.cpidkontak = cpad.kontak SET cp.cppoin = cp.cppoin + cpad.poinmasuk - cpad.poinkeluar WHERE cpad.idcpa = '" & result(4) & "'"
                    sql = "INSERT INTO m1_contact_point(SELECT cpad.kontak as cpidkontak, cpad.poinmasuk - cpad.poinkeluar as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, 0 as cpcustomint1, 0 as cpcustomint2, 0 as cpcustomint3, 0 as cpcustomdbl1, 0 as cpcustomdbl2, 0 as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m_12_cpa_detail cpad WHERE cpad.idcpa = '" & result(4) & "') ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'UPDATE POIN PELANGGAN ==========================================================
                End If


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "CPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M12_CpaUpdateStatus(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'icpaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

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
            Dim sumber As String = "CPA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT cpatgl, cpanotransaksi, cpastatus FROM M_12_Cpa WHERE cpaid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "cpastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m12_cpa_history
            Dim rsSimpanHistory As String = SimpanHistory.M12_Cpa_HistorySimpan("" & paramSplit(0) & "★M12_Cpa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'UPDATE POIN PELANGGAN ==========================================================
                'sql = "UPDATE m1_contact_point cp JOIN m_12_cpa_detail cpad ON cp.cpidkontak = cpad.kontak SET cp.cppoin = cp.cppoin - cpad.poinmasuk + cpad.poinkeluar WHERE cpad.idcpa = '" & idtransaksi & "'"
                sql = "INSERT INTO m1_contact_point(SELECT cpad.kontak as cpidkontak, cpad.poinkeluar - cpad.poinmasuk as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, 0 as cpcustomint1, 0 as cpcustomint2, 0 as cpcustomint3, 0 as cpcustomdbl1, 0 as cpcustomdbl2, 0 as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m_12_cpa_detail cpad WHERE cpad.idcpa = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'UPDATE POIN PELANGGAN ==========================================================
            End If

            'update status utama
            sql = "UPDATE M_12_Cpa SET cpastatus = " & nilaiStatus & ", cpamodifikasiuser='" & userid & "', cpamodifikasitgl = NOW(), cpaposting = 0, cpapostingtgl = '1971-01-01 00:00:00', cpajmlrevisi = cpajmlrevisi + 1 WHERE cpaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_CpaSearch(PostWsSearch(paramSplit(0), "M12_CpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M12_CpaDelete(ByVal param As String) As String

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
            Dim sumber As String = "CPA", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT cpaid, cpanotransaksi FROM M_12_Cpa WHERE cpaid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl"
            sql &= " FROM M_12_Cpa"
            sql &= " WHERE cpaid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("cpacabang")
                lokasi = dtNomorNext.Rows(0)("cpalokasi")
                sumber = dtNomorNext.Rows(0)("cpasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("cpaautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("cpanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("cpatgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Cpa_Detail WHERE idcpa='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Cpa WHERE cpaid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_CpaSearch(PostWsSearch(paramSplit(0), "M12_CpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_CpaSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean, tglLunas As String = ""

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

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
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
        'cpaid(0) As , cpacabang(1) As String, cpalokasi(2) As String, cpasumber(3) As String, cpaautonotransaksi(4) As Integer, 
        'cpanotransaksi(5) As String, cpatgl(6) As Date, cpakodepa(7) As , cpakontak(8) As , cpakontakperson(9) As String, 
        'cpauraian(10) As String, cpacatatan(11) As String, cpastatus(12) As Integer, cpastatussebelumnya(13) As Integer, cpajmlrevisi(14) As Integer, 
        'cpacetakanke(15) As Integer, cpaisclose(16) As Integer, cpainputuser(17) As , cpainputtgl(18) As DateTime, cpamodifikasiuser(19) As , 
        'cpamodifikasitgl(20) As DateTime, cpaposting(21) As Integer, cpapostingtgl(22) As DateTime, cpacustomtext1(23) As String, cpacustomtext2(24) As String, 
        'cpacustomtext3(25) As String, cpacustomtext4(26) As String, cpacustomtext5(27) As String, cpacustomint1(28) As Integer, cpacustomint2(29) As Integer, 
        'cpacustomint3(30) As Integer, cpacustomdbl1(31) As Double, cpacustomdbl2(32) As Double, cpacustomdbl3(33) As Double, cpacustomdate1(34) As Date, 
        'cpacustomdate2(35) As Date, cpacustomdate3(36) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, 
        'cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, 
        'cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, 
        'cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, 
        'cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, 
        'cpacustomdate2, cpacustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 37) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================



        'VALIDASI TIPE DATA UTAMA ==========================================================
        'cpaautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "cpaautonotransaksi required numeric." : GoTo selesai
        End If
        'cpatgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "cpatgl required date." : GoTo selesai
        End If
        'cpastatus(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "cpastatus required numeric." : GoTo selesai
        End If
        'cpastatussebelumnya(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "cpastatussebelumnya required numeric." : GoTo selesai
        End If
        'cpajmlrevisi(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "cpajmlrevisi required numeric." : GoTo selesai
        End If
        'cpacetakanke(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "cpacetakanke required numeric." : GoTo selesai
        End If
        'cpaisclose(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "cpaisclose required numeric." : GoTo selesai
        End If
        'cpainputtgl(18) As DateTime
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "cpainputtgl required date." : GoTo selesai
        End If
        'cpamodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "cpamodifikasitgl required date." : GoTo selesai
        End If
        'cpaposting(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "cpaposting required numeric." : GoTo selesai
        End If
        'cpapostingtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "cpapostingtgl required date." : GoTo selesai
        End If
        'cpacustomint1(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "cpacustomint1 required numeric." : GoTo selesai
        End If
        'cpacustomint2(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "cpacustomint2 required numeric." : GoTo selesai
        End If
        'cpacustomint3(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "cpacustomint3 required numeric." : GoTo selesai
        End If
        'cpacustomdbl1(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "cpacustomdbl1 required numeric." : GoTo selesai
        End If
        'cpacustomdbl2(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "cpacustomdbl2 required numeric." : GoTo selesai
        End If
        'cpacustomdbl3(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "cpacustomdbl3 required numeric." : GoTo selesai
        End If
        'cpacustomdate1(34) As Date
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "cpacustomdate1 required date." : GoTo selesai
        End If
        'cpacustomdate2(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "cpacustomdate2 required date." : GoTo selesai
        End If
        'cpacustomdate3(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "cpacustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'cpaid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "cpaid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "cpaid should not be more than 20 character." : GoTo selesai
        End If

        'cpacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "cpacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "cpacabang should not be more than 25 character." : GoTo selesai
        End If

        'cpalokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cpalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cpalokasi should not be more than 25 character." : GoTo selesai
        End If

        'cpasumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "cpasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "cpasumber should not be more than 10 character." : GoTo selesai
        End If

        'cpanotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "cpanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "cpanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cpatgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "cpatgl can't be empty" : GoTo selesai
        End If

        'cpakodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "cpakodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "cpakodepa should not be more than 20 character." : GoTo selesai
        End If

        'cpakontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "cpakontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "cpakontak should not be more than 20 character." : GoTo selesai
        End If

        'cpainputtgl(18) As DateTime
        If Len(dataUtama(18)) = 0 Then
            result(2) = "cpainputtgl can't be empty" : GoTo selesai
        End If

        'cpamodifikasitgl(20) As DateTime
        If Len(dataUtama(20)) = 0 Then
            result(2) = "cpamodifikasitgl can't be empty" : GoTo selesai
        End If

        'cpapostingtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "cpapostingtgl can't be empty" : GoTo selesai
        End If

        'cpacustomdbl1(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "cpacustomdbl1 can't be empty" : GoTo selesai
        End If

        'cpacustomdbl2(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "cpacustomdbl2 can't be empty" : GoTo selesai
        End If

        'cpacustomdbl3(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "cpacustomdbl3 can't be empty" : GoTo selesai
        End If

        'cpacustomdate1(34) As Date
        If Len(dataUtama(34)) = 0 Then
            result(2) = "cpacustomdate1 can't be empty" : GoTo selesai
        End If

        'cpacustomdate2(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "cpacustomdate2 can't be empty" : GoTo selesai
        End If

        'cpacustomdate3(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "cpacustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "cpaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpaautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpatgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpakontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpakontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpaisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpaposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpapostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cpacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cpacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "cpaid~cpacabang~cpalokasi~cpasumber~cpaautonotransaksi~cpanotransaksi~cpatgl~cpakodepa~cpakontak~cpakontakperson~cpauraian~cpacatatan~cpastatus~cpastatussebelumnya~cpajmlrevisi~cpacetakanke~cpaisclose~cpainputuser~cpainputtgl~cpamodifikasiuser~cpamodifikasitgl~cpaposting~cpapostingtgl~cpacustomtext1~cpacustomtext2~cpacustomtext3~cpacustomtext4~cpacustomtext5~cpacustomint1~cpacustomint2~cpacustomint3~cpacustomdbl1~cpacustomdbl2~cpacustomdbl3~cpacustomdate1~cpacustomdate2~cpacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idcpadetail(0) As , idcpa(1) As , kontak(2) As , poinlama(3) As Double, poinmasuk(4) As Double, 
        'poinkeluar(5) As Double, poinbaru(6) As Double, catatan(7) As String, urutan(8) As Integer, isclose(9) As Integer, 
        'customtext1(10) As String, customtext2(11) As String, customtext3(12) As String, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idcpadetail, idcpa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idcpadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idcpa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "poinlama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "poinmasuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "poinkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "poinbaru", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'poinlama(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "poinlama required numeric." : GoTo selesai
            End If
            'poinmasuk(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "poinmasuk required numeric." : GoTo selesai
            End If
            'poinkeluar(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "poinkeluar required numeric." : GoTo selesai
            End If
            'poinbaru(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "poinbaru required numeric." : GoTo selesai
            End If
            'urutan(8) As Integer
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "urutan required numeric." : GoTo selesai
            End If
            'isclose(9) As Integer
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "isclose required numeric." : GoTo selesai
            End If
            'customdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'idcpadetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idcpadetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idcpadetail should not be more than 20 character." : GoTo selesai
            End If

            'idcpa(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idcpa can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idcpa should not be more than 20 character." : GoTo selesai
            End If

            'kontak(2) As 
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kontak can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 20 Then
                result(2) = "Row : " & i & " - kontak should not be more than 20 character." : GoTo selesai
            End If

            'poinlama(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - poinlama can't be empty" : GoTo selesai
            End If

            'poinmasuk(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - poinmasuk can't be empty" : GoTo selesai
            End If

            'poinkeluar(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - poinkeluar can't be empty" : GoTo selesai
            End If

            'poinbaru(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - poinbaru can't be empty" : GoTo selesai
            End If

            'customdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idcpadetail~idcpa~kontak~poinlama~poinmasuk~poinkeluar~poinbaru~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("cpatgl")), AsFormatTanggal(drutama("cpatgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                If isUpdate Then
                    result(4) = drutama("cpaid")
                    notransaksi = drutama("cpanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(cpaid), cpanotransaksi FROM M_12_Cpa WHERE cpaid='" & result(4) & "' AND cpastatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(cpaid) FROM M_12_Cpa WHERE cpanotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_cpa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M12_Cpa_HistorySimpan("" & paramSplit(0) & "★M12_Cpa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("cpasumber")) & "▼" & FixQuotes(drutama("cpaid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Cpa set cpacabang  = '" & FixQuotes(drutama("cpacabang")) & "', cpalokasi  = '" & FixQuotes(drutama("cpalokasi")) & "', cpasumber  = '" & FixQuotes(drutama("cpasumber")) & "', cpaautonotransaksi  = " & drutama("cpaautonotransaksi") & ", cpanotransaksi  = '" & FixQuotes(notransaksi) & "', cpatgl  = '" & FixQuotes(AsFormatTanggal(drutama("cpatgl"))) & "', cpakodepa  = '" & FixQuotes(drutama("cpakodepa")) & "', cpakontak  = '" & FixQuotes(drutama("cpakontak")) & "', cpakontakperson  = '" & FixQuotes(drutama("cpakontakperson")) & "', cpauraian  = '" & FixQuotes(drutama("cpauraian")) & "', cpacatatan  = '" & FixQuotes(drutama("cpacatatan")) & "', cpastatus  = " & drutama("cpastatus") & ", cpastatussebelumnya  = " & drutama("cpastatussebelumnya") & ", cpajmlrevisi  = cpajmlrevisi+1, cpacetakanke  = " & drutama("cpacetakanke") & ", cpaisclose  = " & drutama("cpaisclose") & ", cpamodifikasiuser  = '" & FixQuotes(drutama("cpamodifikasiuser")) & "', cpamodifikasitgl  = NOW(), cpacustomtext1  = '" & FixQuotes(drutama("cpacustomtext1")) & "', cpacustomtext2  = '" & FixQuotes(drutama("cpacustomtext2")) & "', cpacustomtext3  = '" & FixQuotes(drutama("cpacustomtext3")) & "', cpacustomtext4  = '" & FixQuotes(drutama("cpacustomtext4")) & "', cpacustomtext5  = '" & FixQuotes(drutama("cpacustomtext5")) & "', cpacustomint1  = " & drutama("cpacustomint1") & ", cpacustomint2  = " & drutama("cpacustomint2") & ", cpacustomint3  = " & drutama("cpacustomint3") & ", cpacustomdbl1  = '" & FixDouble(drutama("cpacustomdbl1")) & "', cpacustomdbl2  = '" & FixDouble(drutama("cpacustomdbl2")) & "', cpacustomdbl3  = '" & FixDouble(drutama("cpacustomdbl3")) & "', cpacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate1"))) & "', cpacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate2"))) & "', cpacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate3"))) & "' where cpaid = '" & drutama("cpaid") & "'"
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

                    If drutama("cpaautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("cpacabang"), drutama("cpalokasi"), drutama("cpasumber"), drutama("cpatgl"))
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
                        notransaksi = drutama("cpanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(cpaid) FROM M_12_cpa WHERE cpanotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_cpa (cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, cpacustomdate2, cpacustomdate3) values('" & FixQuotes(drutama("cpacabang")) & "', '" & FixQuotes(drutama("cpalokasi")) & "', '" & FixQuotes(drutama("cpasumber")) & "', " & drutama("cpaautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpatgl"))) & "', '" & FixQuotes(drutama("cpakodepa")) & "', '" & FixQuotes(drutama("cpakontak")) & "', '" & FixQuotes(drutama("cpakontakperson")) & "', '" & FixQuotes(drutama("cpauraian")) & "', '" & FixQuotes(drutama("cpacatatan")) & "', " & drutama("cpastatus") & ", " & drutama("cpastatussebelumnya") & ", " & drutama("cpajmlrevisi") & ", " & drutama("cpacetakanke") & ", " & drutama("cpaisclose") & ", '" & FixQuotes(drutama("cpainputuser")) & "', NOW(), 0, '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("cpacustomtext1")) & "', '" & FixQuotes(drutama("cpacustomtext2")) & "', '" & FixQuotes(drutama("cpacustomtext3")) & "', '" & FixQuotes(drutama("cpacustomtext4")) & "', '" & FixQuotes(drutama("cpacustomtext5")) & "', " & drutama("cpacustomint1") & ", " & drutama("cpacustomint2") & ", " & drutama("cpacustomint3") & ", '" & FixDouble(drutama("cpacustomdbl1")) & "', '" & FixDouble(drutama("cpacustomdbl2")) & "', '" & FixDouble(drutama("cpacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cpacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select cpaid from M_12_cpa where cpanotransaksi='" & notransaksi & "' AND cpainputuser= '" & userid & "' order by cpamodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_cpa_Detail where idcpa = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idcpadetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("kontak")) & "', '" & FixDouble(dr1("poinlama")) & "', '" & FixDouble(dr1("poinmasuk")) & "', '" & FixDouble(dr1("poinkeluar")) & "', '" & FixDouble(dr1("poinbaru")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M_12_cpa_Detail(idcpadetail, idcpa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("cpastatus") = 2 Then
                    'UPDATE POIN PELANGGAN ==========================================================
                    'sql = "UPDATE m1_contact_point cp JOIN m_12_cpa_detail cpad ON cp.cpidkontak = cpad.kontak SET cp.cppoin = cp.cppoin + cpad.poinmasuk - cpad.poinkeluar WHERE cpad.idcpa = '" & result(4) & "'"
                    sql = "INSERT INTO m1_contact_point(SELECT cpad.kontak as cpidkontak, cpad.poinmasuk - cpad.poinkeluar as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, 0 as cpcustomint1, 0 as cpcustomint2, 0 as cpcustomint3, 0 as cpcustomdbl1, 0 as cpcustomdbl2, 0 as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m_12_cpa_detail cpad WHERE cpad.idcpa = '" & result(4) & "') ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'UPDATE POIN PELANGGAN ==========================================================
                End If


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "CPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M12_CpaUpdateStatusOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'icpaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

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
            Dim sumber As String = "CPA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT cpatgl, cpanotransaksi, cpastatus FROM M_12_Cpa WHERE cpaid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "cpastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m12_cpa_history
            Dim rsSimpanHistory As String = SimpanHistory.M12_Cpa_HistorySimpan("" & paramSplit(0) & "★M12_Cpa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'UPDATE POIN PELANGGAN ==========================================================
                'sql = "UPDATE m1_contact_point cp JOIN m_12_cpa_detail cpad ON cp.cpidkontak = cpad.kontak SET cp.cppoin = cp.cppoin - cpad.poinmasuk + cpad.poinkeluar WHERE cpad.idcpa = '" & idtransaksi & "'"
                sql = "INSERT INTO m1_contact_point(SELECT cpad.kontak as cpidkontak, cpad.poinkeluar - cpad.poinmasuk as cppoin, '' as cpcustomtext1, '' as cpcustomtext2, '' as cpcustomtext3, '' as cpcustomtext4, '' as cpcustomtext5, 0 as cpcustomint1, 0 as cpcustomint2, 0 as cpcustomint3, 0 as cpcustomdbl1, 0 as cpcustomdbl2, 0 as cpcustomdbl3, '1900-01-01' as cpcustomdate1, '1900-01-01' as cpcustomdate2, '1900-01-01' as cpcustomdate3 FROM m_12_cpa_detail cpad WHERE cpad.idcpa = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE cppoin = cppoin + VALUES(cppoin)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'UPDATE POIN PELANGGAN ==========================================================
            End If

            'update status utama
            sql = "UPDATE M_12_Cpa SET cpastatus = " & nilaiStatus & ", cpamodifikasiuser='" & userid & "', cpamodifikasitgl = NOW(), cpaposting = 0, cpapostingtgl = '1971-01-01 00:00:00', cpajmlrevisi = cpajmlrevisi + 1 WHERE cpaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_CpaSearch(PostWsSearch(paramSplit(0), "M12_CpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M12_CpaDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "CPA", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT cpaid, cpanotransaksi FROM M_12_Cpa WHERE cpaid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl"
            sql &= " FROM M_12_Cpa"
            sql &= " WHERE cpaid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("cpacabang")
                lokasi = dtNomorNext.Rows(0)("cpalokasi")
                sumber = dtNomorNext.Rows(0)("cpasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("cpaautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("cpanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("cpatgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Cpa_Detail WHERE idcpa='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Cpa WHERE cpaid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_CpaSearch(PostWsSearch(paramSplit(0), "M12_CpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_CpaGetdataById(ByVal param As String) As String
        'M12_CpaGetdataById Utama --------------------------------------------------------
        'cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, 
        'cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, 
        'cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, 
        'cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, 
        'cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, 
        'cpacustomdate2, cpacustomdate3, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, 
        'cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama

        'M12_CpaGetdataById Detail --------------------------------------------------------
        'idcpadetail, idcpa, kontak, poinlama, 
        'poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, 
        'kontakarea, kontakareanama

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

        Dim NmMemcached As String = "aplikasi1-M_12_Cpa~M_12_Cpa_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "cpaid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "cpaid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `cpa`.`cpaid` AS `cpaid`,`cpa`.`cpacabang` AS `cpacabang`,`cpa`.`cpalokasi` AS `cpalokasi`,`cpa`.`cpasumber` AS `cpasumber`,`cpa`.`cpaautonotransaksi` AS `cpaautonotransaksi`,`cpa`.`cpanotransaksi` AS `cpanotransaksi`,`cpa`.`cpatgl` AS `cpatgl`,`cpa`.`cpakodepa` AS `cpakodepa`,`cpa`.`cpakontak` AS `cpakontak`,`cpa`.`cpakontakperson` AS `cpakontakperson`,`cpa`.`cpauraian` AS `cpauraian`,`cpa`.`cpacatatan` AS `cpacatatan`,`cpa`.`cpastatus` AS `cpastatus`,`cpa`.`cpastatussebelumnya` AS `cpastatussebelumnya`,`cpa`.`cpajmlrevisi` AS `cpajmlrevisi`,`cpa`.`cpacetakanke` AS `cpacetakanke`,`cpa`.`cpaisclose` AS `cpaisclose`,`cpa`.`cpainputuser` AS `cpainputuser`,`cpa`.`cpainputtgl` AS `cpainputtgl`,`cpa`.`cpamodifikasiuser` AS `cpamodifikasiuser`,`cpa`.`cpamodifikasitgl` AS `cpamodifikasitgl`,`cpa`.`cpaposting` AS `cpaposting`,`cpa`.`cpapostingtgl` AS `cpapostingtgl`,`cpa`.`cpacustomtext1` AS `cpacustomtext1`,`cpa`.`cpacustomtext2` AS `cpacustomtext2`,`cpa`.`cpacustomtext3` AS `cpacustomtext3`,`cpa`.`cpacustomtext4` AS `cpacustomtext4`,`cpa`.`cpacustomtext5` AS `cpacustomtext5`,`cpa`.`cpacustomint1` AS `cpacustomint1`,`cpa`.`cpacustomint2` AS `cpacustomint2`,`cpa`.`cpacustomint3` AS `cpacustomint3`,`cpa`.`cpacustomdbl1` AS `cpacustomdbl1`,`cpa`.`cpacustomdbl2` AS `cpacustomdbl2`,`cpa`.`cpacustomdbl3` AS `cpacustomdbl3`,`cpa`.`cpacustomdate1` AS `cpacustomdate1`,`cpa`.`cpacustomdate2` AS `cpacustomdate2`,`cpa`.`cpacustomdate3` AS `cpacustomdate3`,`br`.`bnama` AS `cpacabangnama`,`lc`.`lnama` AS `cpalokasinama`,`c1`.`kkode` AS `cpakontakkode`,`c1`.`knama` AS `cpakontaknama`,`st1`.`nama` AS `cpastatusnama`,`st2`.`nama` AS `cpastatussebelumnyanama`,`u1`.`unama` AS `cpainputusernama`,`u2`.`unama` AS `cpamodifikasiusernama`,`cpad`.`idcpadetail` AS `idcpadetail`,`cpad`.`idcpa` AS `idcpa`,`cpad`.`kontak` AS `kontak`,`cpad`.`poinlama` AS `poinlama`,`cpad`.`poinmasuk` AS `poinmasuk`,`cpad`.`poinkeluar` AS `poinkeluar`,`cpad`.`poinbaru` AS `poinbaru`,`cpad`.`catatan` AS `catatan`,`cpad`.`urutan` AS `urutan`,`cpad`.`isclose` AS `isclose`,`cpad`.`customtext1` AS `customtext1`,`cpad`.`customtext2` AS `customtext2`,`cpad`.`customtext3` AS `customtext3`,`cpad`.`customdbl1` AS `customdbl1`,`cpad`.`customdbl2` AS `customdbl2`,`cpad`.`customdbl3` AS `customdbl3`,`cpad`.`customdate1` AS `customdate1`,`cpad`.`customdate2` AS `customdate2`,`cpad`.`customdate3` AS `customdate3`,`c2`.`kkode` AS `kontakkode`,`c2`.`knama` AS `kontaknama`,`c2`.`kkategori` AS `kontakkategori`,`cc`.`ccnama` AS `kontakkategorinama`,`c2`.`kkategorisalesman` AS `kontakkategorisalesman`,`sc`.`scnama` AS `kontakkategorisalesmannama`,`c2`.`karea` AS `kontakarea`,`a`.`anama` AS `kontakareanama` from ((((((((((((`m_12_cpa` `cpa` join `m0_status` `st1` on((`cpa`.`cpastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`cpa`.`cpastatussebelumnya` = `st2`.`kode`))) join `m_12_cpa_detail` `cpad` on((`cpa`.`cpaid` = `cpad`.`idcpa`))) left join `m1_branch` `br` on((`cpa`.`cpacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`cpa`.`cpalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`cpa`.`cpakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`cpa`.`cpainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`cpa`.`cpamodifikasiuser` = `u2`.`userid`))) left join `m1_contact` `c2` on((`cpad`.`kontak` = `c2`.`kid`))) left join `m1_contact_category` `cc` on((`c2`.`kkategori` = `cc`.`cckode`))) left join `m1_salesman_category` `sc` on((`c2`.`kkategorisalesman` = `sc`.`sckode`))) left join `m1_area` `a` on((`c2`.`karea` = `a`.`akode`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("cpaid"), ""), sptField,
                     FxDB(drutama("cpacabang"), ""), sptField,
                     FxDB(drutama("cpalokasi"), ""), sptField,
                     FxDB(drutama("cpasumber"), ""), sptField,
                     FxDB(drutama("cpaautonotransaksi"), 0), sptField,
                     FxDB(drutama("cpanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cpatgl"), ""), formatTgl), sptField,
                     FxDB(drutama("cpakodepa"), ""), sptField,
                     FxDB(drutama("cpakontak"), ""), sptField,
                     FxDB(drutama("cpakontakperson"), ""), sptField,
                     FxDB(drutama("cpauraian"), ""), sptField,
                     FxDB(drutama("cpacatatan"), ""), sptField,
                     FxDB(drutama("cpastatus"), 0), sptField,
                     FxDB(drutama("cpastatussebelumnya"), 0), sptField,
                     FxDB(drutama("cpajmlrevisi"), 0), sptField,
                     FxDB(drutama("cpacetakanke"), 0), sptField,
                     FxDB(drutama("cpaisclose"), 0), sptField,
                     FxDB(drutama("cpainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cpainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cpamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cpamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cpaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cpapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cpacustomtext1"), ""), sptField,
                     FxDB(drutama("cpacustomtext2"), ""), sptField,
                     FxDB(drutama("cpacustomtext3"), ""), sptField,
                     FxDB(drutama("cpacustomtext4"), ""), sptField,
                     FxDB(drutama("cpacustomtext5"), ""), sptField,
                     FxDB(drutama("cpacustomint1"), 0), sptField,
                     FxDB(drutama("cpacustomint2"), 0), sptField,
                     FxDB(drutama("cpacustomint3"), 0), sptField,
                     FxDB(drutama("cpacustomdbl1"), 0), sptField,
                     FxDB(drutama("cpacustomdbl2"), 0), sptField,
                     FxDB(drutama("cpacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cpacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cpacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cpacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("cpacabangnama"), ""), sptField,
                     FxDB(drutama("cpalokasinama"), ""), sptField,
                     FxDB(drutama("cpakontakkode"), ""), sptField,
                     FxDB(drutama("cpakontaknama"), ""), sptField,
                     FxDB(drutama("cpastatusnama"), ""), sptField,
                     FxDB(drutama("cpastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("cpainputusernama"), ""), sptField,
                     FxDB(drutama("cpamodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idcpadetail"), ""), sptField,
                     FxDB(dr("idcpa"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("poinlama"), 0), sptField,
                     FxDB(dr("poinmasuk"), 0), sptField,
                     FxDB(dr("poinkeluar"), 0), sptField,
                     FxDB(dr("poinbaru"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
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
                     FxDB(dr("kontakkategori"), ""), sptField,
                     FxDB(dr("kontakkategorinama"), ""), sptField,
                     FxDB(dr("kontakkategorisalesman"), ""), sptField,
                     FxDB(dr("kontakkategorisalesmannama"), ""), sptField,
                     FxDB(dr("kontakarea"), ""), sptField,
                     FxDB(dr("kontakareanama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, cpaposting, cpapostingtgl, cpacustomtext1, cpacustomtext2, cpacustomtext3, cpacustomtext4, cpacustomtext5, cpacustomint1, cpacustomint2, cpacustomint3, cpacustomdbl1, cpacustomdbl2, cpacustomdbl3, cpacustomdate1, cpacustomdate2, cpacustomdate3, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama" & sptSubParam & "idcpadetail, idcpa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, kontakarea, kontakareanama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_CpaSearch(ByVal param As String) As String
        'M12_CpaSearch --------------------------------------------------------
        'cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, 
        'cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, 
        'cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, 
        'cpaposting, cpapostingtgl, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, 
        'cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama

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
            Filter &= " AND (CASE LENGTH(IFNULL(ub.cabang,'')) WHEN 0 THEN cpa.cpacabang LIKE '%' ELSE cpa.cpacabang = ub.cabang END)"
            Filter &= " AND (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN cpa.cpalokasi LIKE '%' ELSE cpa.cpalokasi = uloc.lokasi END)"
            '#Taruh fungsi replace disini...
        Else
            Filter = " (CASE LENGTH(IFNULL(ub.cabang,'')) WHEN 0 THEN cpa.cpacabang LIKE '%' ELSE cpa.cpacabang = ub.cabang END)"
            Filter &= " AND (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN cpa.cpalokasi LIKE '%' ELSE cpa.cpalokasi = uloc.lokasi END)"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = "select `cpa`.`cpaid` AS `cpaid`,`cpa`.`cpacabang` AS `cpacabang`,`cpa`.`cpalokasi` AS `cpalokasi`,`cpa`.`cpasumber` AS `cpasumber`,`cpa`.`cpaautonotransaksi` AS `cpaautonotransaksi`,`cpa`.`cpanotransaksi` AS `cpanotransaksi`,`cpa`.`cpatgl` AS `cpatgl`,`cpa`.`cpakodepa` AS `cpakodepa`,`cpa`.`cpakontak` AS `cpakontak`,`cpa`.`cpakontakperson` AS `cpakontakperson`,`cpa`.`cpauraian` AS `cpauraian`,`cpa`.`cpacatatan` AS `cpacatatan`,`cpa`.`cpastatus` AS `cpastatus`,`cpa`.`cpastatussebelumnya` AS `cpastatussebelumnya`,`cpa`.`cpajmlrevisi` AS `cpajmlrevisi`,`cpa`.`cpacetakanke` AS `cpacetakanke`,`cpa`.`cpaisclose` AS `cpaisclose`,`cpa`.`cpainputuser` AS `cpainputuser`,`cpa`.`cpainputtgl` AS `cpainputtgl`,`cpa`.`cpamodifikasiuser` AS `cpamodifikasiuser`,`cpa`.`cpamodifikasitgl` AS `cpamodifikasitgl`,`cpa`.`cpaposting` AS `cpaposting`,`cpa`.`cpapostingtgl` AS `cpapostingtgl`,`br`.`bnama` AS `cpacabangnama`,`lc`.`lnama` AS `cpalokasinama`,`c1`.`kkode` AS `cpakontakkode`,`c1`.`knama` AS `cpakontaknama`,`st1`.`nama` AS `cpastatusnama`,`st2`.`nama` AS `cpastatussebelumnyanama`,`u1`.`unama` AS `cpainputusernama`,`u2`.`unama` AS `cpamodifikasiusernama` from (((((((`m_12_cpa` `cpa` join `m0_status` `st1` on((`cpa`.`cpastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`cpa`.`cpastatussebelumnya` = `st2`.`kode`))) left join `m1_branch` `br` on((`cpa`.`cpacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`cpa`.`cpalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`cpa`.`cpakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`cpa`.`cpainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`cpa`.`cpamodifikasiuser` = `u2`.`userid`)))"
        'sql = "select `cpa`.`cpaid` AS `cpaid`, `cpa`.`cpacabang` AS `cpacabang`, `cpa`.`cpalokasi` AS `cpalokasi`, `cpa`.`cpasumber` AS `cpasumber`, `cpa`.`cpaautonotransaksi` AS `cpaautonotransaksi`, `cpa`.`cpanotransaksi` AS `cpanotransaksi`, `cpa`.`cpatgl` AS `cpatgl`, `cpa`.`cpakodepa` AS `cpakodepa`, `cpa`.`cpakontak` AS `cpakontak`, `cpa`.`cpakontakperson` AS `cpakontakperson`, `cpa`.`cpauraian` AS `cpauraian`, `cpa`.`cpacatatan` AS `cpacatatan`, `cpa`.`cpastatus` AS `cpastatus`, `cpa`.`cpastatussebelumnya` AS `cpastatussebelumnya`, `cpa`.`cpajmlrevisi` AS `cpajmlrevisi`, `cpa`.`cpacetakanke` AS `cpacetakanke`, `cpa`.`cpaisclose` AS `cpaisclose`, `cpa`.`cpainputuser` AS `cpainputuser`, `cpa`.`cpainputtgl` AS `cpainputtgl`, `cpa`.`cpamodifikasiuser` AS `cpamodifikasiuser`, `cpa`.`cpamodifikasitgl` AS `cpamodifikasitgl`, `cpa`.`cpaposting` AS `cpaposting`, `cpa`.`cpapostingtgl` AS `cpapostingtgl`, `br`.`bnama` AS `cpacabangnama`, `lc`.`lnama` AS `cpalokasinama`, `c1`.`kkode` AS `cpakontakkode`, `c1`.`knama` AS `cpakontaknama`, `st1`.`nama` AS `cpastatusnama`, `st2`.`nama` AS `cpastatussebelumnyanama`, `u1`.`unama` AS `cpainputusernama`, `u2`.`unama` AS `cpamodifikasiusernama` from `m_12_cpa` `cpa` join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_branch ub on ul.uluser = ub.userid and cpa.cpacabang = ub.cabang join m0_user_location uloc on ul.uluser = uloc.userid and cpa.cpalokasi = uloc.lokasi join `m0_status` `st1` on `cpa`.`cpastatus` = `st1`.`kode` join `m0_status` `st2` on `cpa`.`cpastatussebelumnya` = `st2`.`kode` left join `m1_branch` `br` on `cpa`.`cpacabang` = `br`.`bkode` left join `m1_location` `lc` on `cpa`.`cpalokasi` = `lc`.`lkode` left join `m1_contact` `c1` on `cpa`.`cpakontak` = `c1`.`kid` left join `m0_user` `u1` on `cpa`.`cpainputuser` = `u1`.`userid` left join `m0_user` `u2` on `cpa`.`cpamodifikasiuser` = `u2`.`userid`"
        sql = "select `cpa`.`cpaid` AS `cpaid`, `cpa`.`cpacabang` AS `cpacabang`, `cpa`.`cpalokasi` AS `cpalokasi`, `cpa`.`cpasumber` AS `cpasumber`, `cpa`.`cpaautonotransaksi` AS `cpaautonotransaksi`, `cpa`.`cpanotransaksi` AS `cpanotransaksi`, `cpa`.`cpatgl` AS `cpatgl`, `cpa`.`cpakodepa` AS `cpakodepa`, `cpa`.`cpakontak` AS `cpakontak`, `cpa`.`cpakontakperson` AS `cpakontakperson`, `cpa`.`cpauraian` AS `cpauraian`, `cpa`.`cpacatatan` AS `cpacatatan`, `cpa`.`cpastatus` AS `cpastatus`, `cpa`.`cpastatussebelumnya` AS `cpastatussebelumnya`, `cpa`.`cpajmlrevisi` AS `cpajmlrevisi`, `cpa`.`cpacetakanke` AS `cpacetakanke`, `cpa`.`cpaisclose` AS `cpaisclose`, `cpa`.`cpainputuser` AS `cpainputuser`, `cpa`.`cpainputtgl` AS `cpainputtgl`, `cpa`.`cpamodifikasiuser` AS `cpamodifikasiuser`, `cpa`.`cpamodifikasitgl` AS `cpamodifikasitgl`, `cpa`.`cpaposting` AS `cpaposting`, `cpa`.`cpapostingtgl` AS `cpapostingtgl`, `br`.`bnama` AS `cpacabangnama`, `lc`.`lnama` AS `cpalokasinama`, `c1`.`kkode` AS `cpakontakkode`, `c1`.`knama` AS `cpakontaknama`, `st1`.`nama` AS `cpastatusnama`, `st2`.`nama` AS `cpastatussebelumnyanama`, `u1`.`unama` AS `cpainputusernama`, `u2`.`unama` AS `cpamodifikasiusernama` from `m_12_cpa` `cpa` join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join `m0_status` `st1` on `cpa`.`cpastatus` = `st1`.`kode` join `m0_status` `st2` on `cpa`.`cpastatussebelumnya` = `st2`.`kode` left join m0_user_branch ub on ul.uluser = ub.userid left join m0_user_location uloc on ul.uluser = uloc.userid left join `m1_branch` `br` on `cpa`.`cpacabang` = `br`.`bkode` left join `m1_location` `lc` on `cpa`.`cpalokasi` = `lc`.`lkode` left join `m1_contact` `c1` on `cpa`.`cpakontak` = `c1`.`kid` left join `m0_user` `u1` on `cpa`.`cpainputuser` = `u1`.`userid` left join `m0_user` `u2` on `cpa`.`cpamodifikasiuser` = `u2`.`userid`"

        dt = AmbilData("aplikasi1-M_12_Cpa_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cpaid"), ""), sptField,
                     FxDB(dr("cpacabang"), ""), sptField,
                     FxDB(dr("cpalokasi"), ""), sptField,
                     FxDB(dr("cpasumber"), ""), sptField,
                     FxDB(dr("cpaautonotransaksi"), 0), sptField,
                     FxDB(dr("cpanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cpatgl"), ""), formatTgl), sptField,
                     FxDB(dr("cpakodepa"), ""), sptField,
                     FxDB(dr("cpakontak"), ""), sptField,
                     FxDB(dr("cpakontakperson"), ""), sptField,
                     FxDB(dr("cpauraian"), ""), sptField,
                     FxDB(dr("cpacatatan"), ""), sptField,
                     FxDB(dr("cpastatus"), 0), sptField,
                     FxDB(dr("cpastatussebelumnya"), 0), sptField,
                     FxDB(dr("cpajmlrevisi"), 0), sptField,
                     FxDB(dr("cpacetakanke"), 0), sptField,
                     FxDB(dr("cpaisclose"), 0), sptField,
                     FxDB(dr("cpainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cpainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cpamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cpamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cpaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cpapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cpacabangnama"), ""), sptField,
                     FxDB(dr("cpalokasinama"), ""), sptField,
                     FxDB(dr("cpakontakkode"), ""), sptField,
                     FxDB(dr("cpakontaknama"), ""), sptField,
                     FxDB(dr("cpastatusnama"), ""), sptField,
                     FxDB(dr("cpastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("cpainputusernama"), ""), sptField,
                     FxDB(dr("cpamodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cpaid, cpacabang, cpalokasi, cpasumber, cpaautonotransaksi, cpanotransaksi, cpatgl, cpakodepa, cpakontak, cpakontakperson, cpauraian, cpacatatan, cpastatus, cpastatussebelumnya, cpajmlrevisi, cpacetakanke, cpaisclose, cpainputuser, cpainputtgl, cpamodifikasiuser, cpamodifikasitgl, cpaposting, cpapostingtgl, cpacabangnama, cpalokasinama, cpakontakkode, cpakontaknama, cpastatusnama, cpastatussebelumnyanama, cpainputusernama, cpamodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_CpaTerkait(ByVal param As String) As String
        'M12_CpaTerkait --------------------------------------------------------
        'cpaid, cpanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rmid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("M_12_Cpa_terkait")
        'sql = sql.Replace("validtransaksi", idtransaksi)

        ''BUKA KONEKSI
        'Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'Con1.Open()

        'dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cpaid"), 0), sptField,
                     FxDB(dr("cpanotransaksi"), ""), sptField,
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
            result(2) = "Related cpa data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cpaid, cpanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

End Class
