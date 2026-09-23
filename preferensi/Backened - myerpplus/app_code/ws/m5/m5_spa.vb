Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_spa
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""


    <WebMethod()>
    Public Function M5_SpaSimpan(ByVal param As String) As String
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
        'spaid(0) As , spacabang(1) As String, spalokasi(2) As String, spasumber(3) As String, spaautonotransaksi(4) As Integer, 
        'spanotransaksi(5) As String, spatgl(6) As Date, spakodepa(7) As , spakontak(8) As , spakontakperson(9) As String, 
        'spauraian(10) As String, spacatatan(11) As String, spastatus(12) As Integer, spastatussebelumnya(13) As Integer, spajmlrevisi(14) As Integer, 
        'spacetakanke(15) As Integer, spaisclose(16) As Integer, spainputuser(17) As , spainputtgl(18) As DateTime, spamodifikasiuser(19) As , 
        'spamodifikasitgl(20) As DateTime, spaposting(21) As Integer, spapostingtgl(22) As DateTime, spacustomtext1(23) As String, spacustomtext2(24) As String, 
        'spacustomtext3(25) As String, spacustomtext4(26) As String, spacustomtext5(27) As String, spacustomint1(28) As Integer, spacustomint2(29) As Integer, 
        'spacustomint3(30) As Integer, spacustomdbl1(31) As Double, spacustomdbl2(32) As Double, spacustomdbl3(33) As Double, spacustomdate1(34) As Date, 
        'spacustomdate2(35) As Date, spacustomdate3(36) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, 
        'spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, 
        'spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, 
        'spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, 
        'spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, 
        'spacustomdate2, spacustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 37) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================



        'VALIDASI TIPE DATA UTAMA ==========================================================
        'spaautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "spaautonotransaksi required numeric." : GoTo selesai
        End If
        'spatgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "spatgl required date." : GoTo selesai
        End If
        'spastatus(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "spastatus required numeric." : GoTo selesai
        End If
        'spastatussebelumnya(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "spastatussebelumnya required numeric." : GoTo selesai
        End If
        'spajmlrevisi(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "spajmlrevisi required numeric." : GoTo selesai
        End If
        'spacetakanke(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "spacetakanke required numeric." : GoTo selesai
        End If
        'spaisclose(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "spaisclose required numeric." : GoTo selesai
        End If
        'spainputtgl(18) As DateTime
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "spainputtgl required date." : GoTo selesai
        End If
        'spamodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "spamodifikasitgl required date." : GoTo selesai
        End If
        'spaposting(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "spaposting required numeric." : GoTo selesai
        End If
        'spapostingtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "spapostingtgl required date." : GoTo selesai
        End If
        'spacustomint1(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "spacustomint1 required numeric." : GoTo selesai
        End If
        'spacustomint2(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "spacustomint2 required numeric." : GoTo selesai
        End If
        'spacustomint3(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "spacustomint3 required numeric." : GoTo selesai
        End If
        'spacustomdbl1(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "spacustomdbl1 required numeric." : GoTo selesai
        End If
        'spacustomdbl2(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "spacustomdbl2 required numeric." : GoTo selesai
        End If
        'spacustomdbl3(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "spacustomdbl3 required numeric." : GoTo selesai
        End If
        'spacustomdate1(34) As Date
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "spacustomdate1 required date." : GoTo selesai
        End If
        'spacustomdate2(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "spacustomdate2 required date." : GoTo selesai
        End If
        'spacustomdate3(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "spacustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'spaid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "spaid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "spaid should not be more than 20 character." : GoTo selesai
        End If

        'spacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "spacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "spacabang should not be more than 25 character." : GoTo selesai
        End If

        'spalokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "spalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "spalokasi should not be more than 25 character." : GoTo selesai
        End If

        'spasumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "spasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "spasumber should not be more than 10 character." : GoTo selesai
        End If

        'spanotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "spanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "spanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'spatgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "spatgl can't be empty" : GoTo selesai
        End If

        'spakodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "spakodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "spakodepa should not be more than 20 character." : GoTo selesai
        End If

        'spakontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "spakontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "spakontak should not be more than 20 character." : GoTo selesai
        End If

        'spainputtgl(18) As DateTime
        If Len(dataUtama(18)) = 0 Then
            result(2) = "spainputtgl can't be empty" : GoTo selesai
        End If

        'spamodifikasitgl(20) As DateTime
        If Len(dataUtama(20)) = 0 Then
            result(2) = "spamodifikasitgl can't be empty" : GoTo selesai
        End If

        'spapostingtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "spapostingtgl can't be empty" : GoTo selesai
        End If

        'spacustomdbl1(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "spacustomdbl1 can't be empty" : GoTo selesai
        End If

        'spacustomdbl2(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "spacustomdbl2 can't be empty" : GoTo selesai
        End If

        'spacustomdbl3(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "spacustomdbl3 can't be empty" : GoTo selesai
        End If

        'spacustomdate1(34) As Date
        If Len(dataUtama(34)) = 0 Then
            result(2) = "spacustomdate1 can't be empty" : GoTo selesai
        End If

        'spacustomdate2(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "spacustomdate2 can't be empty" : GoTo selesai
        End If

        'spacustomdate3(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "spacustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "spaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spaautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spatgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spakontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spakontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spaisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spaposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spapostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "spaid~spacabang~spalokasi~spasumber~spaautonotransaksi~spanotransaksi~spatgl~spakodepa~spakontak~spakontakperson~spauraian~spacatatan~spastatus~spastatussebelumnya~spajmlrevisi~spacetakanke~spaisclose~spainputuser~spainputtgl~spamodifikasiuser~spamodifikasitgl~spaposting~spapostingtgl~spacustomtext1~spacustomtext2~spacustomtext3~spacustomtext4~spacustomtext5~spacustomint1~spacustomint2~spacustomint3~spacustomdbl1~spacustomdbl2~spacustomdbl3~spacustomdate1~spacustomdate2~spacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idspadetail(0) As , idspa(1) As , kontak(2) As , poinlama(3) As Double, poinmasuk(4) As Double, 
        'poinkeluar(5) As Double, poinbaru(6) As Double, catatan(7) As String, urutan(8) As Integer, isclose(9) As Integer, 
        'customtext1(10) As String, customtext2(11) As String, customtext3(12) As String, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idspadetail, idspa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idspadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idspa", AsEnumTypeData.AsInt64)
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
            'idspadetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idspadetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idspadetail should not be more than 20 character." : GoTo selesai
            End If

            'idspa(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idspa can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idspa should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idspadetail~idspa~kontak~poinlama~poinmasuk~poinkeluar~poinbaru~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 54
                Select Case drutama("spastatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("spatgl")), AsFormatTanggal(drutama("spatgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                If isUpdate Then
                    result(4) = drutama("spaid")
                    notransaksi = drutama("spanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(spaid), spanotransaksi FROM M5_Spa WHERE spaid='" & result(4) & "' AND spastatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("spaautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("spacabang"), drutama("spalokasi"), drutama("spasumber"), drutama("spatgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(spaid) FROM M5_Spa WHERE spanotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_spa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Spa_HistorySimpan("" & paramSplit(0) & "★M5_Spa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("spasumber")) & "▼" & FixQuotes(drutama("spaid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Spa set spacabang  = '" & FixQuotes(drutama("spacabang")) & "', spalokasi  = '" & FixQuotes(drutama("spalokasi")) & "', spasumber  = '" & FixQuotes(drutama("spasumber")) & "', spaautonotransaksi  = " & drutama("spaautonotransaksi") & ", spanotransaksi  = '" & FixQuotes(notransaksi) & "', spatgl  = '" & FixQuotes(AsFormatTanggal(drutama("spatgl"))) & "', spakodepa  = '" & FixQuotes(drutama("spakodepa")) & "', spakontak  = '" & FixQuotes(drutama("spakontak")) & "', spakontakperson  = '" & FixQuotes(drutama("spakontakperson")) & "', spauraian  = '" & FixQuotes(drutama("spauraian")) & "', spacatatan  = '" & FixQuotes(drutama("spacatatan")) & "', spastatus  = " & drutama("spastatus") & ", spastatussebelumnya  = " & drutama("spastatussebelumnya") & ", spajmlrevisi  = spajmlrevisi+1, spacetakanke  = " & drutama("spacetakanke") & ", spaisclose  = " & drutama("spaisclose") & ", spamodifikasiuser  = '" & FixQuotes(drutama("spamodifikasiuser")) & "', spamodifikasitgl  = NOW(), spacustomtext1  = '" & FixQuotes(drutama("spacustomtext1")) & "', spacustomtext2  = '" & FixQuotes(drutama("spacustomtext2")) & "', spacustomtext3  = '" & FixQuotes(drutama("spacustomtext3")) & "', spacustomtext4  = '" & FixQuotes(drutama("spacustomtext4")) & "', spacustomtext5  = '" & FixQuotes(drutama("spacustomtext5")) & "', spacustomint1  = " & drutama("spacustomint1") & ", spacustomint2  = " & drutama("spacustomint2") & ", spacustomint3  = " & drutama("spacustomint3") & ", spacustomdbl1  = '" & FixDouble(drutama("spacustomdbl1")) & "', spacustomdbl2  = '" & FixDouble(drutama("spacustomdbl2")) & "', spacustomdbl3  = '" & FixDouble(drutama("spacustomdbl3")) & "', spacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate1"))) & "', spacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate2"))) & "', spacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate3"))) & "' where spaid = '" & drutama("spaid") & "'"
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

                    If drutama("spaautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("spacabang"), drutama("spalokasi"), drutama("spasumber"), drutama("spatgl"))
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
                        notransaksi = drutama("spanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(spaid) FROM M5_Spa WHERE spanotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Spa (spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, spacustomdate2, spacustomdate3) values('" & FixQuotes(drutama("spacabang")) & "', '" & FixQuotes(drutama("spalokasi")) & "', '" & FixQuotes(drutama("spasumber")) & "', " & drutama("spaautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("spatgl"))) & "', '" & FixQuotes(drutama("spakodepa")) & "', '" & FixQuotes(drutama("spakontak")) & "', '" & FixQuotes(drutama("spakontakperson")) & "', '" & FixQuotes(drutama("spauraian")) & "', '" & FixQuotes(drutama("spacatatan")) & "', " & drutama("spastatus") & ", " & drutama("spastatussebelumnya") & ", " & drutama("spajmlrevisi") & ", " & drutama("spacetakanke") & ", " & drutama("spaisclose") & ", '" & FixQuotes(drutama("spainputuser")) & "', NOW(), 0, '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("spacustomtext1")) & "', '" & FixQuotes(drutama("spacustomtext2")) & "', '" & FixQuotes(drutama("spacustomtext3")) & "', '" & FixQuotes(drutama("spacustomtext4")) & "', '" & FixQuotes(drutama("spacustomtext5")) & "', " & drutama("spacustomint1") & ", " & drutama("spacustomint2") & ", " & drutama("spacustomint3") & ", '" & FixDouble(drutama("spacustomdbl1")) & "', '" & FixDouble(drutama("spacustomdbl2")) & "', '" & FixDouble(drutama("spacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select spaid from M5_Spa where spanotransaksi='" & notransaksi & "' AND spainputuser= '" & userid & "' order by spamodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Spa_Detail where idspa = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idspadetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("kontak")) & "', '" & FixDouble(dr1("poinlama")) & "', '" & FixDouble(dr1("poinmasuk")) & "', '" & FixDouble(dr1("poinkeluar")) & "', '" & FixDouble(dr1("poinbaru")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Spa_Detail(idspadetail, idspa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'If drutama("spastatus") = 2 Then
                '    'UPDATE POIN PENJUALAN ==========================================================
                '    sql = "UPDATE m1_contact c JOIN m5_spa_detail spad ON c.kid = spad.kontak SET c.kkomisipenjualan = c.kkomisipenjualan + spad.poinmasuk - spad.poinkeluar WHERE spad.idspa = '" & result(4) & "'"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'UPDATE POIN PENJUALAN ==========================================================
                'End If


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "SPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SpaUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "SPA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT spatgl, spanotransaksi, spastatus FROM M5_Spa WHERE spaid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "spastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_spa_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Spa_HistorySimpan("" & paramSplit(0) & "★M5_Spa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'If isDelete Then
            '    'UPDATE POIN PENJUALAN ==========================================================
            '    sql = "UPDATE m1_contact c JOIN m5_spa_detail spad ON c.kid = spad.kontak SET c.kkomisipenjualan = c.kkomisipenjualan - spad.poinmasuk + spad.poinkeluar WHERE spad.idspa = '" & idtransaksi & "'"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myconn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()
            '    'UPDATE POIN PENJUALAN ==========================================================
            'End If

            'update status utama
            sql = "UPDATE M5_Spa SET spastatus = " & nilaiStatus & ", spamodifikasiuser='" & userid & "', spamodifikasitgl = NOW(), spaposting = 0, spapostingtgl = '1971-01-01 00:00:00', spajmlrevisi = spajmlrevisi + 1 WHERE spaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SpaSearch(PostWsSearch(paramSplit(0), "M5_SpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SpaDelete(ByVal param As String) As String

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
            Dim sumber As String = "SPA", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT spaid, spanotransaksi FROM M5_Spa WHERE spaid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl"
            sql &= " FROM M5_Spa"
            sql &= " WHERE spaid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("spacabang")
                lokasi = dtNomorNext.Rows(0)("spalokasi")
                sumber = dtNomorNext.Rows(0)("spasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("spaautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("spanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("spatgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Spa_Detail WHERE idspa='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Spa WHERE spaid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SpaSearch(PostWsSearch(paramSplit(0), "M5_SpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SpaGetdataById(ByVal param As String) As String
        'M5_SpaGetdataById Utama --------------------------------------------------------
        'spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, 
        'spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, 
        'spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, 
        'spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, 
        'spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, 
        'spacustomdate2, spacustomdate3, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, 
        'spastatussebelumnyanama, spainputusernama, spamodifikasiusernama

        'M5_SpaGetdataById Detail --------------------------------------------------------
        'idspadetail, idspa, kontak, poinlama, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Spa~M5_Spa_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "spaid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "spaid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `spa`.`spaid` AS `spaid`,`spa`.`spacabang` AS `spacabang`,`spa`.`spalokasi` AS `spalokasi`,`spa`.`spasumber` AS `spasumber`,`spa`.`spaautonotransaksi` AS `spaautonotransaksi`,`spa`.`spanotransaksi` AS `spanotransaksi`,`spa`.`spatgl` AS `spatgl`,`spa`.`spakodepa` AS `spakodepa`,`spa`.`spakontak` AS `spakontak`,`spa`.`spakontakperson` AS `spakontakperson`,`spa`.`spauraian` AS `spauraian`,`spa`.`spacatatan` AS `spacatatan`,`spa`.`spastatus` AS `spastatus`,`spa`.`spastatussebelumnya` AS `spastatussebelumnya`,`spa`.`spajmlrevisi` AS `spajmlrevisi`,`spa`.`spacetakanke` AS `spacetakanke`,`spa`.`spaisclose` AS `spaisclose`,`spa`.`spainputuser` AS `spainputuser`,`spa`.`spainputtgl` AS `spainputtgl`,`spa`.`spamodifikasiuser` AS `spamodifikasiuser`,`spa`.`spamodifikasitgl` AS `spamodifikasitgl`,`spa`.`spaposting` AS `spaposting`,`spa`.`spapostingtgl` AS `spapostingtgl`,`spa`.`spacustomtext1` AS `spacustomtext1`,`spa`.`spacustomtext2` AS `spacustomtext2`,`spa`.`spacustomtext3` AS `spacustomtext3`,`spa`.`spacustomtext4` AS `spacustomtext4`,`spa`.`spacustomtext5` AS `spacustomtext5`,`spa`.`spacustomint1` AS `spacustomint1`,`spa`.`spacustomint2` AS `spacustomint2`,`spa`.`spacustomint3` AS `spacustomint3`,`spa`.`spacustomdbl1` AS `spacustomdbl1`,`spa`.`spacustomdbl2` AS `spacustomdbl2`,`spa`.`spacustomdbl3` AS `spacustomdbl3`,`spa`.`spacustomdate1` AS `spacustomdate1`,`spa`.`spacustomdate2` AS `spacustomdate2`,`spa`.`spacustomdate3` AS `spacustomdate3`,`br`.`bnama` AS `spacabangnama`,`lc`.`lnama` AS `spalokasinama`,`c1`.`kkode` AS `spakontakkode`,`c1`.`knama` AS `spakontaknama`,`st1`.`nama` AS `spastatusnama`,`st2`.`nama` AS `spastatussebelumnyanama`,`u1`.`unama` AS `spainputusernama`,`u2`.`unama` AS `spamodifikasiusernama`,`spad`.`idspadetail` AS `idspadetail`,`spad`.`idspa` AS `idspa`,`spad`.`kontak` AS `kontak`,`spad`.`poinlama` AS `poinlama`,`spad`.`poinmasuk` AS `poinmasuk`,`spad`.`poinkeluar` AS `poinkeluar`,`spad`.`poinbaru` AS `poinbaru`,`spad`.`catatan` AS `catatan`,`spad`.`urutan` AS `urutan`,`spad`.`isclose` AS `isclose`,`spad`.`customtext1` AS `customtext1`,`spad`.`customtext2` AS `customtext2`,`spad`.`customtext3` AS `customtext3`,`spad`.`customdbl1` AS `customdbl1`,`spad`.`customdbl2` AS `customdbl2`,`spad`.`customdbl3` AS `customdbl3`,`spad`.`customdate1` AS `customdate1`,`spad`.`customdate2` AS `customdate2`,`spad`.`customdate3` AS `customdate3`,`c2`.`kkode` AS `kontakkode`,`c2`.`knama` AS `kontaknama`,`c2`.`kkategori` AS `kontakkategori`,`cc`.`ccnama` AS `kontakkategorinama`,`c2`.`kkategorisalesman` AS `kontakkategorisalesman`,`sc`.`scnama` AS `kontakkategorisalesmannama`,`c2`.`karea` AS `kontakarea`,`a`.`anama` AS `kontakareanama` from ((((((((((((`m5_spa` `spa` join `m0_status` `st1` on((`spa`.`spastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`spa`.`spastatussebelumnya` = `st2`.`kode`))) join `m5_spa_detail` `spad` on((`spa`.`spaid` = `spad`.`idspa`))) left join `m1_branch` `br` on((`spa`.`spacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`spa`.`spalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`spa`.`spakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`spa`.`spainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`spa`.`spamodifikasiuser` = `u2`.`userid`))) left join `m1_contact` `c2` on((`spad`.`kontak` = `c2`.`kid`))) left join `m1_contact_category` `cc` on((`c2`.`kkategori` = `cc`.`cckode`))) left join `m1_salesman_category` `sc` on((`c2`.`kkategorisalesman` = `sc`.`sckode`))) left join `m1_area` `a` on((`c2`.`karea` = `a`.`akode`)))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("spaid"), ""), sptField,
                     FxDB(drutama("spacabang"), ""), sptField,
                     FxDB(drutama("spalokasi"), ""), sptField,
                     FxDB(drutama("spasumber"), ""), sptField,
                     FxDB(drutama("spaautonotransaksi"), 0), sptField,
                     FxDB(drutama("spanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("spatgl"), ""), formatTgl), sptField,
                     FxDB(drutama("spakodepa"), ""), sptField,
                     FxDB(drutama("spakontak"), ""), sptField,
                     FxDB(drutama("spakontakperson"), ""), sptField,
                     FxDB(drutama("spauraian"), ""), sptField,
                     FxDB(drutama("spacatatan"), ""), sptField,
                     FxDB(drutama("spastatus"), 0), sptField,
                     FxDB(drutama("spastatussebelumnya"), 0), sptField,
                     FxDB(drutama("spajmlrevisi"), 0), sptField,
                     FxDB(drutama("spacetakanke"), 0), sptField,
                     FxDB(drutama("spaisclose"), 0), sptField,
                     FxDB(drutama("spainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("spainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("spamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("spacustomtext1"), ""), sptField,
                     FxDB(drutama("spacustomtext2"), ""), sptField,
                     FxDB(drutama("spacustomtext3"), ""), sptField,
                     FxDB(drutama("spacustomtext4"), ""), sptField,
                     FxDB(drutama("spacustomtext5"), ""), sptField,
                     FxDB(drutama("spacustomint1"), 0), sptField,
                     FxDB(drutama("spacustomint2"), 0), sptField,
                     FxDB(drutama("spacustomint3"), 0), sptField,
                     FxDB(drutama("spacustomdbl1"), 0), sptField,
                     FxDB(drutama("spacustomdbl2"), 0), sptField,
                     FxDB(drutama("spacustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("spacustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spacustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("spacustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("spacabangnama"), ""), sptField,
                     FxDB(drutama("spalokasinama"), ""), sptField,
                     FxDB(drutama("spakontakkode"), ""), sptField,
                     FxDB(drutama("spakontaknama"), ""), sptField,
                     FxDB(drutama("spastatusnama"), ""), sptField,
                     FxDB(drutama("spastatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("spainputusernama"), ""), sptField,
                     FxDB(drutama("spamodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("idspadetail"), ""), sptField,
                     FxDB(dr("idspa"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, spacustomdate2, spacustomdate3, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, spastatussebelumnyanama, spainputusernama, spamodifikasiusernama" & sptSubParam & "idspadetail, idspa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, kontakkategori, kontakkategorinama, kontakkategorisalesman, kontakkategorisalesmannama, kontakarea, kontakareanama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SpaSearch(ByVal param As String) As String
        'M5_SpaSearch --------------------------------------------------------
        'spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, 
        'spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, 
        'spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, 
        'spaposting, spapostingtgl, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, 
        'spastatussebelumnyanama, spainputusernama, spamodifikasiusernama

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
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `spa`.`spaid` AS `spaid`,`spa`.`spacabang` AS `spacabang`,`spa`.`spalokasi` AS `spalokasi`,`spa`.`spasumber` AS `spasumber`,`spa`.`spaautonotransaksi` AS `spaautonotransaksi`,`spa`.`spanotransaksi` AS `spanotransaksi`,`spa`.`spatgl` AS `spatgl`,`spa`.`spakodepa` AS `spakodepa`,`spa`.`spakontak` AS `spakontak`,`spa`.`spakontakperson` AS `spakontakperson`,`spa`.`spauraian` AS `spauraian`,`spa`.`spacatatan` AS `spacatatan`,`spa`.`spastatus` AS `spastatus`,`spa`.`spastatussebelumnya` AS `spastatussebelumnya`,`spa`.`spajmlrevisi` AS `spajmlrevisi`,`spa`.`spacetakanke` AS `spacetakanke`,`spa`.`spaisclose` AS `spaisclose`,`spa`.`spainputuser` AS `spainputuser`,`spa`.`spainputtgl` AS `spainputtgl`,`spa`.`spamodifikasiuser` AS `spamodifikasiuser`,`spa`.`spamodifikasitgl` AS `spamodifikasitgl`,`spa`.`spaposting` AS `spaposting`,`spa`.`spapostingtgl` AS `spapostingtgl`,`br`.`bnama` AS `spacabangnama`,`lc`.`lnama` AS `spalokasinama`,`c1`.`kkode` AS `spakontakkode`,`c1`.`knama` AS `spakontaknama`,`st1`.`nama` AS `spastatusnama`,`st2`.`nama` AS `spastatussebelumnyanama`,`u1`.`unama` AS `spainputusernama`,`u2`.`unama` AS `spamodifikasiusernama` from (((((((`m5_spa` `spa` join `m0_status` `st1` on((`spa`.`spastatus` = `st1`.`kode`))) join `m0_status` `st2` on((`spa`.`spastatussebelumnya` = `st2`.`kode`))) left join `m1_branch` `br` on((`spa`.`spacabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`spa`.`spalokasi` = `lc`.`lkode`))) left join `m1_contact` `c1` on((`spa`.`spakontak` = `c1`.`kid`))) left join `m0_user` `u1` on((`spa`.`spainputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`spa`.`spamodifikasiuser` = `u2`.`userid`)))"

        dt = AmbilData("aplikasi1-M5_Spa_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("spaid"), ""), sptField,
                     FxDB(dr("spacabang"), ""), sptField,
                     FxDB(dr("spalokasi"), ""), sptField,
                     FxDB(dr("spasumber"), ""), sptField,
                     FxDB(dr("spaautonotransaksi"), 0), sptField,
                     FxDB(dr("spanotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("spatgl"), ""), formatTgl), sptField,
                     FxDB(dr("spakodepa"), ""), sptField,
                     FxDB(dr("spakontak"), ""), sptField,
                     FxDB(dr("spakontakperson"), ""), sptField,
                     FxDB(dr("spauraian"), ""), sptField,
                     FxDB(dr("spacatatan"), ""), sptField,
                     FxDB(dr("spastatus"), 0), sptField,
                     FxDB(dr("spastatussebelumnya"), 0), sptField,
                     FxDB(dr("spajmlrevisi"), 0), sptField,
                     FxDB(dr("spacetakanke"), 0), sptField,
                     FxDB(dr("spaisclose"), 0), sptField,
                     FxDB(dr("spainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("spainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("spamodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spaposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("spapostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("spacabangnama"), ""), sptField,
                     FxDB(dr("spalokasinama"), ""), sptField,
                     FxDB(dr("spakontakkode"), ""), sptField,
                     FxDB(dr("spakontaknama"), ""), sptField,
                     FxDB(dr("spastatusnama"), ""), sptField,
                     FxDB(dr("spastatussebelumnyanama"), ""), sptField,
                     FxDB(dr("spainputusernama"), ""), sptField,
                     FxDB(dr("spamodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, spaposting, spapostingtgl, spacabangnama, spalokasinama, spakontakkode, spakontaknama, spastatusnama, spastatussebelumnyanama, spainputusernama, spamodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SpaTerkait(ByVal param As String) As String
        'M5_SpaTerkait --------------------------------------------------------
        'spaid, spanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        'sql = query.PanggilQuery("M5_Spa_terkait")
        'sql = sql.Replace("validtransaksi", idtransaksi)

        ''BUKA KONEKSI
        'Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'Con1.Open()

        'dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("spaid"), 0), sptField,
                     FxDB(dr("spanotransaksi"), ""), sptField,
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
            result(2) = "Related spa data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("spaid, spanotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SpaSimpanOld(ByVal param As String) As String
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
        'spaid(0) As , spacabang(1) As String, spalokasi(2) As String, spasumber(3) As String, spaautonotransaksi(4) As Integer, 
        'spanotransaksi(5) As String, spatgl(6) As Date, spakodepa(7) As , spakontak(8) As , spakontakperson(9) As String, 
        'spauraian(10) As String, spacatatan(11) As String, spastatus(12) As Integer, spastatussebelumnya(13) As Integer, spajmlrevisi(14) As Integer, 
        'spacetakanke(15) As Integer, spaisclose(16) As Integer, spainputuser(17) As , spainputtgl(18) As DateTime, spamodifikasiuser(19) As , 
        'spamodifikasitgl(20) As DateTime, spaposting(21) As Integer, spapostingtgl(22) As DateTime, spacustomtext1(23) As String, spacustomtext2(24) As String, 
        'spacustomtext3(25) As String, spacustomtext4(26) As String, spacustomtext5(27) As String, spacustomint1(28) As Integer, spacustomint2(29) As Integer, 
        'spacustomint3(30) As Integer, spacustomdbl1(31) As Double, spacustomdbl2(32) As Double, spacustomdbl3(33) As Double, spacustomdate1(34) As Date, 
        'spacustomdate2(35) As Date, spacustomdate3(36) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'spaid, spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, 
        'spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, 
        'spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, 
        'spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, 
        'spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, 
        'spacustomdate2, spacustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 37) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================



        'VALIDASI TIPE DATA UTAMA ==========================================================
        'spaautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "spaautonotransaksi required numeric." : GoTo selesai
        End If
        'spatgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "spatgl required date." : GoTo selesai
        End If
        'spastatus(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "spastatus required numeric." : GoTo selesai
        End If
        'spastatussebelumnya(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "spastatussebelumnya required numeric." : GoTo selesai
        End If
        'spajmlrevisi(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "spajmlrevisi required numeric." : GoTo selesai
        End If
        'spacetakanke(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "spacetakanke required numeric." : GoTo selesai
        End If
        'spaisclose(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "spaisclose required numeric." : GoTo selesai
        End If
        'spainputtgl(18) As DateTime
        If (IsDate(dataUtama(18)) = False) Then
            result(2) = "spainputtgl required date." : GoTo selesai
        End If
        'spamodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "spamodifikasitgl required date." : GoTo selesai
        End If
        'spaposting(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "spaposting required numeric." : GoTo selesai
        End If
        'spapostingtgl(22) As DateTime
        If (IsDate(dataUtama(22)) = False) Then
            result(2) = "spapostingtgl required date." : GoTo selesai
        End If
        'spacustomint1(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "spacustomint1 required numeric." : GoTo selesai
        End If
        'spacustomint2(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "spacustomint2 required numeric." : GoTo selesai
        End If
        'spacustomint3(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "spacustomint3 required numeric." : GoTo selesai
        End If
        'spacustomdbl1(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "spacustomdbl1 required numeric." : GoTo selesai
        End If
        'spacustomdbl2(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "spacustomdbl2 required numeric." : GoTo selesai
        End If
        'spacustomdbl3(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "spacustomdbl3 required numeric." : GoTo selesai
        End If
        'spacustomdate1(34) As Date
        If (IsDate(dataUtama(34)) = False) Then
            result(2) = "spacustomdate1 required date." : GoTo selesai
        End If
        'spacustomdate2(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "spacustomdate2 required date." : GoTo selesai
        End If
        'spacustomdate3(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "spacustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'spaid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "spaid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "spaid should not be more than 20 character." : GoTo selesai
        End If

        'spacabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "spacabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "spacabang should not be more than 25 character." : GoTo selesai
        End If

        'spalokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "spalokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "spalokasi should not be more than 25 character." : GoTo selesai
        End If

        'spasumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "spasumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "spasumber should not be more than 10 character." : GoTo selesai
        End If

        'spanotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "spanotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "spanotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'spatgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "spatgl can't be empty" : GoTo selesai
        End If

        'spakodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "spakodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "spakodepa should not be more than 20 character." : GoTo selesai
        End If

        'spakontak(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "spakontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "spakontak should not be more than 20 character." : GoTo selesai
        End If

        'spainputtgl(18) As DateTime
        If Len(dataUtama(18)) = 0 Then
            result(2) = "spainputtgl can't be empty" : GoTo selesai
        End If

        'spamodifikasitgl(20) As DateTime
        If Len(dataUtama(20)) = 0 Then
            result(2) = "spamodifikasitgl can't be empty" : GoTo selesai
        End If

        'spapostingtgl(22) As DateTime
        If Len(dataUtama(22)) = 0 Then
            result(2) = "spapostingtgl can't be empty" : GoTo selesai
        End If

        'spacustomdbl1(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "spacustomdbl1 can't be empty" : GoTo selesai
        End If

        'spacustomdbl2(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "spacustomdbl2 can't be empty" : GoTo selesai
        End If

        'spacustomdbl3(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "spacustomdbl3 can't be empty" : GoTo selesai
        End If

        'spacustomdate1(34) As Date
        If Len(dataUtama(34)) = 0 Then
            result(2) = "spacustomdate1 can't be empty" : GoTo selesai
        End If

        'spacustomdate2(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "spacustomdate2 can't be empty" : GoTo selesai
        End If

        'spacustomdate3(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "spacustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "spaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spasumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spaautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spanotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spatgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spakodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spakontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spakontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spauraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spastatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spastatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spajmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spaisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spamodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spaposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spapostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "spacustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "spacustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "spaid~spacabang~spalokasi~spasumber~spaautonotransaksi~spanotransaksi~spatgl~spakodepa~spakontak~spakontakperson~spauraian~spacatatan~spastatus~spastatussebelumnya~spajmlrevisi~spacetakanke~spaisclose~spainputuser~spainputtgl~spamodifikasiuser~spamodifikasitgl~spaposting~spapostingtgl~spacustomtext1~spacustomtext2~spacustomtext3~spacustomtext4~spacustomtext5~spacustomint1~spacustomint2~spacustomint3~spacustomdbl1~spacustomdbl2~spacustomdbl3~spacustomdate1~spacustomdate2~spacustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idspadetail(0) As , idspa(1) As , kontak(2) As , poinlama(3) As Double, poinmasuk(4) As Double, 
        'poinkeluar(5) As Double, poinbaru(6) As Double, catatan(7) As String, urutan(8) As Integer, isclose(9) As Integer, 
        'customtext1(10) As String, customtext2(11) As String, customtext3(12) As String, customdbl1(13) As Double, customdbl2(14) As Double, 
        'customdbl3(15) As Double, customdate1(16) As Date, customdate2(17) As Date, customdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idspadetail, idspa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, 
        'catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idspadetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idspa", AsEnumTypeData.AsInt64)
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
            'idspadetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idspadetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - idspadetail should not be more than 20 character." : GoTo selesai
            End If

            'idspa(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - idspa can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - idspa should not be more than 20 character." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idspadetail~idspa~kontak~poinlama~poinmasuk~poinkeluar~poinbaru~catatan~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("spatgl")), AsFormatTanggal(drutama("spatgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                If isUpdate Then
                    result(4) = drutama("spaid")
                    notransaksi = drutama("spanotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(spaid), spanotransaksi FROM M5_Spa WHERE spaid='" & result(4) & "' AND spastatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(spaid) FROM M5_Spa WHERE spanotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_spa_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Spa_HistorySimpan("" & paramSplit(0) & "★M5_Spa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("spasumber")) & "▼" & FixQuotes(drutama("spaid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Spa set spacabang  = '" & FixQuotes(drutama("spacabang")) & "', spalokasi  = '" & FixQuotes(drutama("spalokasi")) & "', spasumber  = '" & FixQuotes(drutama("spasumber")) & "', spaautonotransaksi  = " & drutama("spaautonotransaksi") & ", spanotransaksi  = '" & FixQuotes(notransaksi) & "', spatgl  = '" & FixQuotes(AsFormatTanggal(drutama("spatgl"))) & "', spakodepa  = '" & FixQuotes(drutama("spakodepa")) & "', spakontak  = '" & FixQuotes(drutama("spakontak")) & "', spakontakperson  = '" & FixQuotes(drutama("spakontakperson")) & "', spauraian  = '" & FixQuotes(drutama("spauraian")) & "', spacatatan  = '" & FixQuotes(drutama("spacatatan")) & "', spastatus  = " & drutama("spastatus") & ", spastatussebelumnya  = " & drutama("spastatussebelumnya") & ", spajmlrevisi  = spajmlrevisi+1, spacetakanke  = " & drutama("spacetakanke") & ", spaisclose  = " & drutama("spaisclose") & ", spamodifikasiuser  = '" & FixQuotes(drutama("spamodifikasiuser")) & "', spamodifikasitgl  = NOW(), spacustomtext1  = '" & FixQuotes(drutama("spacustomtext1")) & "', spacustomtext2  = '" & FixQuotes(drutama("spacustomtext2")) & "', spacustomtext3  = '" & FixQuotes(drutama("spacustomtext3")) & "', spacustomtext4  = '" & FixQuotes(drutama("spacustomtext4")) & "', spacustomtext5  = '" & FixQuotes(drutama("spacustomtext5")) & "', spacustomint1  = " & drutama("spacustomint1") & ", spacustomint2  = " & drutama("spacustomint2") & ", spacustomint3  = " & drutama("spacustomint3") & ", spacustomdbl1  = '" & FixDouble(drutama("spacustomdbl1")) & "', spacustomdbl2  = '" & FixDouble(drutama("spacustomdbl2")) & "', spacustomdbl3  = '" & FixDouble(drutama("spacustomdbl3")) & "', spacustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate1"))) & "', spacustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate2"))) & "', spacustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate3"))) & "' where spaid = '" & drutama("spaid") & "'"
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

                    If drutama("spaautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("spacabang"), drutama("spalokasi"), drutama("spasumber"), drutama("spatgl"))
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
                        notransaksi = drutama("spanotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(spaid) FROM M5_Spa WHERE spanotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Spa (spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl, spakodepa, spakontak, spakontakperson, spauraian, spacatatan, spastatus, spastatussebelumnya, spajmlrevisi, spacetakanke, spaisclose, spainputuser, spainputtgl, spamodifikasiuser, spamodifikasitgl, spaposting, spapostingtgl, spacustomtext1, spacustomtext2, spacustomtext3, spacustomtext4, spacustomtext5, spacustomint1, spacustomint2, spacustomint3, spacustomdbl1, spacustomdbl2, spacustomdbl3, spacustomdate1, spacustomdate2, spacustomdate3) values('" & FixQuotes(drutama("spacabang")) & "', '" & FixQuotes(drutama("spalokasi")) & "', '" & FixQuotes(drutama("spasumber")) & "', " & drutama("spaautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("spatgl"))) & "', '" & FixQuotes(drutama("spakodepa")) & "', '" & FixQuotes(drutama("spakontak")) & "', '" & FixQuotes(drutama("spakontakperson")) & "', '" & FixQuotes(drutama("spauraian")) & "', '" & FixQuotes(drutama("spacatatan")) & "', " & drutama("spastatus") & ", " & drutama("spastatussebelumnya") & ", " & drutama("spajmlrevisi") & ", " & drutama("spacetakanke") & ", " & drutama("spaisclose") & ", '" & FixQuotes(drutama("spainputuser")) & "', NOW(), 0, '1971-01-01 00:00:00', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("spacustomtext1")) & "', '" & FixQuotes(drutama("spacustomtext2")) & "', '" & FixQuotes(drutama("spacustomtext3")) & "', '" & FixQuotes(drutama("spacustomtext4")) & "', '" & FixQuotes(drutama("spacustomtext5")) & "', " & drutama("spacustomint1") & ", " & drutama("spacustomint2") & ", " & drutama("spacustomint3") & ", '" & FixDouble(drutama("spacustomdbl1")) & "', '" & FixDouble(drutama("spacustomdbl2")) & "', '" & FixDouble(drutama("spacustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("spacustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select spaid from M5_Spa where spanotransaksi='" & notransaksi & "' AND spainputuser= '" & userid & "' order by spamodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Spa_Detail where idspa = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idspadetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("kontak")) & "', '" & FixDouble(dr1("poinlama")) & "', '" & FixDouble(dr1("poinmasuk")) & "', '" & FixDouble(dr1("poinkeluar")) & "', '" & FixDouble(dr1("poinbaru")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Spa_Detail(idspadetail, idspa, kontak, poinlama, poinmasuk, poinkeluar, poinbaru, catatan, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'If drutama("spastatus") = 2 Then
                '    'UPDATE POIN PENJUALAN ==========================================================
                '    sql = "UPDATE m1_contact c JOIN m5_spa_detail spad ON c.kid = spad.kontak SET c.kkomisipenjualan = c.kkomisipenjualan + spad.poinmasuk - spad.poinkeluar WHERE spad.idspa = '" & result(4) & "'"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = Con1
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'UPDATE POIN PENJUALAN ==========================================================
                'End If


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "SPA", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SpaUpdateStatusOld(ByVal param As String) As String
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
            Dim sumber As String = "SPA", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT spatgl, spanotransaksi, spastatus FROM M5_Spa WHERE spaid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "spastatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_spa_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Spa_HistorySimpan("" & paramSplit(0) & "★M5_Spa_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'If isDelete Then
            '    'UPDATE POIN PENJUALAN ==========================================================
            '    sql = "UPDATE m1_contact c JOIN m5_spa_detail spad ON c.kid = spad.kontak SET c.kkomisipenjualan = c.kkomisipenjualan - spad.poinmasuk + spad.poinkeluar WHERE spad.idspa = '" & idtransaksi & "'"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = Con1
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()
            '    'UPDATE POIN PENJUALAN ==========================================================
            'End If

            'update status utama
            sql = "UPDATE M5_Spa SET spastatus = " & nilaiStatus & ", spamodifikasiuser='" & userid & "', spamodifikasitgl = NOW(), spaposting = 0, spapostingtgl = '1971-01-01 00:00:00', spajmlrevisi = spajmlrevisi + 1 WHERE spaid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SpaSearch(PostWsSearch(paramSplit(0), "M5_SpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SpaDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "SPA", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT spaid, spanotransaksi FROM M5_Spa WHERE spaid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT spacabang, spalokasi, spasumber, spaautonotransaksi, spanotransaksi, spatgl"
            sql &= " FROM M5_Spa"
            sql &= " WHERE spaid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("spacabang")
                lokasi = dtNomorNext.Rows(0)("spalokasi")
                sumber = dtNomorNext.Rows(0)("spasumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("spaautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("spanotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("spatgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Spa_Detail WHERE idspa='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Spa WHERE spaid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SpaSearch(PostWsSearch(paramSplit(0), "M5_SpaSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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