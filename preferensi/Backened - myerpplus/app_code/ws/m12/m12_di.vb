Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_di
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_DiSimpan(ByVal param As String) As String
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
        'diid(0) As Integer, dicabang(1) As String, dilokasi(2) As String, disumber(3) As String, dikategoripos(4) As String, 
        'diautonotransaksi(5) As Integer, dinotransaksi(6) As String, ditgl(7) As Date, dikodepa(8) As , dikontak(9) As , 
        'dikontakperson(10) As String, diuraian(11) As String, dicatatan(12) As String, distatus(13) As Integer, distatussebelumnya(14) As Integer, 
        'dijmlrevisi(15) As Integer, dicetakanke(16) As Integer, diisclose(17) As Integer, diinputuser(18) As , diinputtgl(19) As DateTime, 
        'dimodifikasiuser(20) As , dimodifikasitgl(21) As DateTime, diposting(22) As Integer, dipostingtgl(23) As DateTime, dicustomtext1(24) As String, 
        'dicustomtext2(25) As String, dicustomtext3(26) As String, dicustomtext4(27) As String, dicustomtext5(28) As String, dicustomint1(29) As Integer, 
        'dicustomint2(30) As Integer, dicustomint3(31) As Integer, dicustomdbl1(32) As Double, dicustomdbl2(33) As Double, dicustomdbl3(34) As Double, 
        'dicustomdate1(35) As Date, dicustomdate2(36) As Date, dicustomdate3(37) As Date, dijeniskategori(38) As Int

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, 
        'ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, 
        'distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, 
        'dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, 
        'dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, 
        'dicustomdate1, dicustomdate2, dicustomdate3, dijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'diid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "diid required numeric." : GoTo selesai
        End If
        'diautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "diautonotransaksi required numeric." : GoTo selesai
        End If
        'ditgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ditgl required date." : GoTo selesai
        End If
        'distatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "distatus required numeric." : GoTo selesai
        End If
        'distatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "distatussebelumnya required numeric." : GoTo selesai
        End If
        'dijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "dijmlrevisi required numeric." : GoTo selesai
        End If
        'dicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "dicetakanke required numeric." : GoTo selesai
        End If
        'diisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "diisclose required numeric." : GoTo selesai
        End If
        'diinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "diinputtgl required date." : GoTo selesai
        End If
        'dimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "dimodifikasitgl required date." : GoTo selesai
        End If
        'diposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "diposting required numeric." : GoTo selesai
        End If
        'dipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "dipostingtgl required date." : GoTo selesai
        End If
        'dicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "dicustomint1 required numeric." : GoTo selesai
        End If
        'dicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "dicustomint2 required numeric." : GoTo selesai
        End If
        'dicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "dicustomint3 required numeric." : GoTo selesai
        End If
        'dicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "dicustomdbl1 required numeric." : GoTo selesai
        End If
        'dicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "dicustomdbl2 required numeric." : GoTo selesai
        End If
        'dicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dicustomdbl3 required numeric." : GoTo selesai
        End If
        'dicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "dicustomdate1 required date." : GoTo selesai
        End If
        'dicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "dicustomdate2 required date." : GoTo selesai
        End If
        'dicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "dicustomdate3 required date." : GoTo selesai
        End If

        'dijeniskategori(38) As Date
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dijeniskategori required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'dicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dicabang should not be more than 25 character." : GoTo selesai
        End If

        'dilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dilokasi should not be more than 25 character." : GoTo selesai
        End If

        'disumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "disumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "disumber should not be more than 10 character." : GoTo selesai
        End If

        'dikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "dikategoripos can't be empty" : GoTo selesai
        'End If
        'If Len(dataUtama(4)) > 50 Then
        '    result(2) = "dikategoripos should not be more than 50 character." : GoTo selesai
        'End If

        'dinotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "dinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "dinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ditgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ditgl can't be empty" : GoTo selesai
        End If

        'dikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "dikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "dikodepa should not be more than 20 character." : GoTo selesai
        End If

        'dikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "dikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "dikontak should not be more than 20 character." : GoTo selesai
        End If

        'diinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "diinputtgl can't be empty" : GoTo selesai
        End If

        'dimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "dimodifikasitgl can't be empty" : GoTo selesai
        End If

        'dipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "dipostingtgl can't be empty" : GoTo selesai
        End If

        'dicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "dicustomdbl1 can't be empty" : GoTo selesai
        End If

        'dicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "dicustomdbl2 can't be empty" : GoTo selesai
        End If

        'dicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "dicustomdbl3 can't be empty" : GoTo selesai
        End If

        'dicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "dicustomdate1 can't be empty" : GoTo selesai
        End If

        'dicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "dicustomdate2 can't be empty" : GoTo selesai
        End If

        'dicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dicustomdate3 can't be empty" : GoTo selesai
        End If

        'dijeniskategori(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dijeniskategori can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "diid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "disumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "diautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ditgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "diuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "distatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "distatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "diisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "diinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "diinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "diposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "diid~dicabang~dilokasi~disumber~dikategoripos~diautonotransaksi~dinotransaksi~ditgl~dikodepa~dikontak~dikontakperson~diuraian~dicatatan~distatus~distatussebelumnya~dijmlrevisi~dicetakanke~diisclose~diinputuser~diinputtgl~dimodifikasiuser~dimodifikasitgl~diposting~dipostingtgl~dicustomtext1~dicustomtext2~dicustomtext3~dicustomtext4~dicustomtext5~dicustomint1~dicustomint2~dicustomint3~dicustomdbl1~dicustomdbl2~dicustomdbl3~dicustomdate1~dicustomdate2~dicustomdate3~dijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, 
        'kriteria, nilai, tgl1, tgl2, jam1, jam2, catatan, 
        'urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, 
        'customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, nopromo

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "operator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kriteria", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "nilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nopromo", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 30) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'jml1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jml1 required numeric." : GoTo selesai
            End If
            'jml2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jml2 required numeric." : GoTo selesai
            End If
            'kriteria(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "kriteria required numeric." : GoTo selesai
            End If
            'tgl1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "tgl1 required date." : GoTo selesai
            End If
            'tgl2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "tgl2 required date." : GoTo selesai
            End If
            'jam1(11) As Date
            If (IsDate(dataRowDetail(11)) = False) Then
                result(2) = "jam1 required date." : GoTo selesai
            End If
            'jam2(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "jam2 required date." : GoTo selesai
            End If
            'customint1(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(28) As Date
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'iddidetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - iddidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - iddidetail should not be more than 20 character." : GoTo selesai
            End If

            'iddi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - iddi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - iddi should not be more than 20 character." : GoTo selesai
            End If

            'dikategori(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(2)) > 25 Then
            '    result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            'End If

            'idbarang(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'operator(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - operator can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - operator should not be more than 25 character." : GoTo selesai
            End If

            'jml1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml1 can't be empty" : GoTo selesai
            End If

            'jml2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jml2 can't be empty" : GoTo selesai
            End If

            'nilai(8) As String
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - nilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(8)) > 25 Then
                result(2) = "Row : " & i & " - nilai should not be more than 25 character." : GoTo selesai
            End If

            'tgl1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - tgl1 can't be empty" : GoTo selesai
            End If

            'tgl2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - tgl2 can't be empty" : GoTo selesai
            End If

            'jam1(11) As Date
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jam1 can't be empty" : GoTo selesai
            End If

            'jam2(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jam2 can't be empty" : GoTo selesai
            End If

            'urutan(14) As 
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 20 Then
                result(2) = "Row : " & i & " - urutan should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(28) As Date
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "iddidetail~iddi~dikategori~idbarang~operator~jml1~jml2~kriteria~nilai~tgl1~tgl2~jam1~jam2~catatan~urutan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nopromo", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29))

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
                Dim vModuleId As Integer = 12, vMenuId As Integer = 57
                Select Case drutama("distatus")
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
                    result(4) = drutama("diid")
                    notransaksi = drutama("dinotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(diid), dinotransaksi FROM M_12_Di WHERE diid=" & result(4), myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(diid) FROM M_12_Di WHERE dinotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_di_history
                        Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("disumber")) & "▼" & FixQuotes(drutama("diid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Di set dicabang  = '" & FixQuotes(drutama("dicabang")) & "', dilokasi  = '" & FixQuotes(drutama("dilokasi")) & "', disumber  = '" & FixQuotes(drutama("disumber")) & "', dikategoripos  = '" & FixQuotes(drutama("dikategoripos")) & "', diautonotransaksi  = " & drutama("diautonotransaksi") & ", dinotransaksi  = '" & FixQuotes(drutama("dinotransaksi")) & "', ditgl  = '" & FixQuotes(AsFormatTanggal(drutama("ditgl"))) & "', dikodepa  = '" & FixQuotes(drutama("dikodepa")) & "', dikontak  = '" & FixQuotes(drutama("dikontak")) & "', dikontakperson  = '" & FixQuotes(drutama("dikontakperson")) & "', diuraian  = '" & FixQuotes(drutama("diuraian")) & "', dicatatan  = '" & FixQuotes(drutama("dicatatan")) & "', distatus  = " & drutama("distatus") & ", distatussebelumnya  = " & drutama("distatussebelumnya") & ", dijmlrevisi  = " & drutama("dijmlrevisi") & ", dicetakanke  = " & drutama("dicetakanke") & ", diisclose  = " & drutama("diisclose") & ", diinputuser  = '" & FixQuotes(drutama("diinputuser")) & "', diinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("diinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', dimodifikasiuser  = '" & FixQuotes(drutama("dimodifikasiuser")) & "', dimodifikasitgl  = NOW(), diposting  = " & drutama("diposting") & ", dipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("dipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', dicustomtext1  = '" & FixQuotes(drutama("dicustomtext1")) & "', dicustomtext2  = '" & FixQuotes(drutama("dicustomtext2")) & "', dicustomtext3  = '" & FixQuotes(drutama("dicustomtext3")) & "', dicustomtext4  = '" & FixQuotes(drutama("dicustomtext4")) & "', dicustomtext5  = '" & FixQuotes(drutama("dicustomtext5")) & "', dicustomint1  = " & drutama("dicustomint1") & ", dicustomint2  = " & drutama("dicustomint2") & ", dicustomint3  = " & drutama("dicustomint3") & ", dicustomdbl1  = '" & FixDouble(drutama("dicustomdbl1")) & "', dicustomdbl2  = '" & FixDouble(drutama("dicustomdbl2")) & "', dicustomdbl3  = '" & FixDouble(drutama("dicustomdbl3")) & "', dicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate1"))) & "', dicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate2"))) & "', dicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate3"))) & "', dijeniskategori  = '" & FixQuotes(drutama("dijeniskategori")) & "' where diid = " & drutama("diid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else

                    If drutama("diautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dicabang"), drutama("dilokasi"), drutama("disumber"), drutama("ditgl"))
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
                        notransaksi = drutama("dinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(diid) FROM m_12_di WHERE dinotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_di (dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dijeniskategori) values('" & FixQuotes(drutama("dicabang")) & "', '" & FixQuotes(drutama("dilokasi")) & "', '" & FixQuotes(drutama("disumber")) & "', '" & FixQuotes(drutama("dikategoripos")) & "', " & drutama("diautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ditgl"))) & "', '" & FixQuotes(drutama("dikodepa")) & "', '" & FixQuotes(drutama("dikontak")) & "', '" & FixQuotes(drutama("dikontakperson")) & "', '" & FixQuotes(drutama("diuraian")) & "', '" & FixQuotes(drutama("dicatatan")) & "', " & drutama("distatus") & ", " & drutama("distatussebelumnya") & ", " & drutama("dijmlrevisi") & ", " & drutama("dicetakanke") & ", " & drutama("diisclose") & ", '" & FixQuotes(drutama("diinputuser")) & "', NOW(), '" & FixQuotes(drutama("dimodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dimodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("dicustomtext1")) & "', '" & FixQuotes(drutama("dicustomtext2")) & "', '" & FixQuotes(drutama("dicustomtext3")) & "', '" & FixQuotes(drutama("dicustomtext4")) & "', '" & FixQuotes(drutama("dicustomtext5")) & "', " & drutama("dicustomint1") & ", " & drutama("dicustomint2") & ", " & drutama("dicustomint3") & ", '" & FixDouble(drutama("dicustomdbl1")) & "', '" & FixDouble(drutama("dicustomdbl2")) & "', '" & FixDouble(drutama("dicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate3"))) & "', '" & FixQuotes(drutama("dijeniskategori")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select diid from M_12_di where dinotransaksi='" & notransaksi & "' AND diinputuser= '" & drutama("diinputuser") & "' order by dimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If isUpdate = True Then
                    sql = "Delete from M_12_Di_Detail where iddi = " & result(4)
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

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT did.dikategori as kategori, did.idbarang as idbarang, did.operator as operator, i.bkode, (CASE did.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_di_detail did JOIN m1_item i ON did.idbarang = i.bid WHERE did.dikategori = '" & FxDB(drutama("dikategoripos"), "") & "' AND did.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND did.iddi = '" & result(4) & "' AND did.iddidetail <> '" & FxDB(dr1("iddidetail"), "") & "' GROUP BY did.operator ORDER BY did.operator"
                        dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtOperator.Rows.Count > 0 Then
                            Dim vOperator As String = ""
                            Dim vIdBarang As Integer = 0
                            For Each drOperator As DataRow In dtOperator.Rows
                                vOperator = FxDB(drOperator("operator").ToString, "")
                                vIdBarang = FxDB(drOperator("idbarang").ToString, "")
                                If Len(vOperator) > 0 Then
                                    If vOperator = 2 Then
                                        'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                        result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    Else
                                        'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                        'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                        'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                        If dr1("idbarang") = vIdBarang And (dr1("operator") = 2 Or (vOperator = 1 And dr1("operator") = vOperator)) Then
                                            result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                        End If
                                    End If
                                End If
                            Next
                        End If

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("iddidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("dikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', " & dr1("kriteria") & ", '" & FixQuotes(dr1("nilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("jam1")) & "', '" & FixQuotes(dr1("jam2")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("urutan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & notransaksi & "')")

                        sql = "Insert into M_12_Di_Detail(iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, kriteria, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        strValue2.Clear()

                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Discount
                If drutama("distatus") = 2 Then 'JIKA STATUS APPROVED
                    If drutama("dijeniskategori") = 1 Then 'JIKA PER KATEGORI
                        'Cek apakah kategori pos sudah ada di tabel pos_bonus_item, jika sudah ada, hapus data di tabel itu
                        'HAPUS POS DISCOUNT ITEM
                        sql = "Delete From m_12_pos_discount_item where dikategori = '" & drutama("dikategoripos") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    ElseIf drutama("dijeniskategori") = 2 Then 'JIKA PER CABANG
                        'ambil kategori pos sesuai cabang
                        Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("dicabang")) & "'", myConn)
                        If dtCatPOS.Rows.Count > 0 Then
                            If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                'Cek apakah kategori pos sudah ada di tabel pos_bonus_item, jika sudah ada, hapus data di tabel itu
                                'HAPUS POS DISCOUNT ITEM
                                sql = "Delete From m_12_pos_discount_item where dikategori IN (" & dtCatPOS.Rows(0)(0) & ")"
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

                    Else 'JIKA SEMUA KATEGORI
                        'HAPUS POS DISCOUNT ITEM
                        sql = "Delete From m_12_pos_discount_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    Dim dtdtl As New DataTable
                    dtdtl = AsDataTableAmbilDariDBCon("select * from M_12_Di_Detail where iddi = '" & result(4) & "' order by iddi asc", myConn)
                    Dim strInsertDiscountItem As New StringBuilder 'untuk query simpan di tabel bonus utama
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("dijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_discount_item & tabel m_12_pos_discount_item_detail
                                strInsertDiscountItem.Append(IIf(Len(strInsertDiscountItem.ToString) = 0, "", ", "))
                                strInsertDiscountItem.Append("('" & FixQuotes(drutama("dikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', " & drdtl("kriteria") & ", '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("jam1")) & "', '" & FixQuotes(drdtl("jam2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_discount_item
                            sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dinopromo) values" & strInsertDiscountItem.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        ElseIf drutama("dijeniskategori") = 2 Then 'JIKA PER CABANG
                            'ambil kategori pos sesuai cabang
                            Dim dtCatPOS As DataTable = AsDataTableAmbilDariDBCon("SELECT GROUP_CONCAT(" & Chr(34) & "'" & Chr(34) & ",l.lkategoripos," & Chr(34) & "'" & Chr(34) & ") as kategoripos FROM m1_location l WHERE l.lkategoripos <> '' AND l.lcabang = '" & FixQuotes(drutama("dicabang")) & "'", myConn)
                            If dtCatPOS.Rows.Count > 0 Then
                                If Len(FxDB(dtCatPOS.Rows(0)(0), "")) > 0 Then
                                    Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                                    'CARI DATA KATEGORI POS
                                    dtKatPOS = AsDataTableAmbilDariDBCon("select pckode from m_12_pos_category WHERE pckode IN (" & dtCatPOS.Rows(0)(0) & ")", myConn)
                                    For Each drKatPos As DataRow In dtKatPOS.Rows
                                        For Each drdtl As DataRow In dtdtl.Rows
                                            'AMBIL DATA BARANG POS
                                            dtPosItem = AsDataTableAmbilDariDBCon("select piidbarang from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl("idbarang") & "' order by pikategori asc", myConn)
                                            If dtPosItem.Rows.Count > 0 Then
                                                For Each drPosItem As DataRow In dtPosItem.Rows
                                                    'persiapan insert ke tabel m_12_pos_discount_item 
                                                    strInsertDiscountItem.Append(IIf(Len(strInsertDiscountItem.ToString) = 0, "", ", "))
                                                    strInsertDiscountItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', " & drdtl("kriteria") & ", '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("jam1")) & "', '" & FixQuotes(drdtl("jam2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                                Next
                                            End If

                                        Next
                                    Next

                                    'insert ke tabel m_12_pos_discount_item
                                    sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dinopromo) values" & strInsertDiscountItem.ToString & ""
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

                        Else 'JIKA SEMUA KATEGORI
                            Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                            'CARI DATA KATEGORI POS
                            dtKatPOS = AsDataTableAmbilDariDBCon("select pckode from m_12_pos_category", myConn)
                            For Each drKatPos As DataRow In dtKatPOS.Rows
                                For Each drdtl As DataRow In dtdtl.Rows
                                    'AMBIL DATA BARANG POS
                                    dtPosItem = AsDataTableAmbilDariDBCon("select piidbarang from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl("idbarang") & "' order by pikategori asc", myConn)
                                    If dtPosItem.Rows.Count > 0 Then
                                        For Each drPosItem As DataRow In dtPosItem.Rows
                                            'persiapan insert ke tabel m_12_pos_discount_item 
                                            strInsertDiscountItem.Append(IIf(Len(strInsertDiscountItem.ToString) = 0, "", ", "))
                                            strInsertDiscountItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', " & drdtl("kriteria") & ", '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("jam1")) & "', '" & FixQuotes(drdtl("jam2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                        Next
                                    End If

                                Next
                            Next

                            'insert ke tabel m_12_pos_discount_item
                            sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dinopromo) values" & strInsertDiscountItem.ToString & ""
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
                        result(2) = "Main Transaction POS Discount Item data not found." : Trans.Rollback() : GoTo selesai
                    End If
                End If

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
        'myConn.Close()
        'myConn = Nothing
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
    Public Function M12_DiUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("dikontakkode", "c.kkode")
            Filter = Filter.Replace("dikontaknama", "c.knama")
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
            Dim sumber As String = "DI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ditgl, Dinotransaksi, Distatus FROM m_12_Di WHERE Diid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Distatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            'CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================


            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m12_di_history
            Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Di WHERE diid=" & idtransaksi, myConn)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows
                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDBCon("SELECT * FROM M_12_Di_Detail WHERE iddi=" & idtransaksi, myConn)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                If drutama("dijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_discount_item WHERE dikategori='" & drdetail("dikategori") & "' AND dinopromo = '" & drdetail("nopromo") & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Else 'JIKA SEMUA KATEGORI
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_discount_item WHERE dinopromo = '" & drdetail("nopromo") & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                            Next


                            'hapus data detail
                            'sql = "Delete from M_12_Bi_Detail WHERE idbidetail=" & idtransaksi
                            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            'With objCmd
                            '    .Connection = myconn
                            '    .Transaction = Trans
                            '    .CommandType = CommandType.Text
                            '    .CommandText = sql
                            'End With
                            'objCmd.ExecuteNonQuery()

                            ''jika status unclose maka nilai status ambil dari status sebelumnya
                            'If (nilaiStatus = "unclose") Then
                            '    Dim dtstatusbefore As DataTable
                            '    dtstatusbefore = asdatatableambildaridbcon("SELECT Bistatussebelumnya FROM M_12_Bi WHERE biid=" & idtransaksi)
                            '    nilaiStatus = Val(dtstatusbefore.Rows(0)(0))
                            'End If

                        End If
                    Next
                End If


            End If


            'update status utama
            sql = "UPDATE M_12_Di SET Distatus = " & nilaiStatus & ", dimodifikasiuser='" & userid & "', dimodifikasitgl = NOW(), diposting = 0, dipostingtgl = '1971-01-01 00:00:00', Dijmlrevisi = Dijmlrevisi + 1 WHERE diid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_DiSearch(PostWsSearch(paramSplit(0), "M12_DiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_DiDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("dikontakkode", "c.kkode")
            Filter = Filter.Replace("dikontaknama", "c.knama")
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
            Dim sumber As String = "DI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT diid, dinotransaksi FROM m_12_di WHERE diid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dicabang, dilokasi, disumber, diautonotransaksi, dinotransaksi, ditgl"
            sql &= " FROM M_12_di"
            sql &= " WHERE diid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dicabang")
                lokasi = dtNomorNext.Rows(0)("dilokasi")
                sumber = dtNomorNext.Rows(0)("disumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("diautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("dinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ditgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Di_Detail WHERE iddi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Di WHERE diid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_DiSearch(PostWsSearch(paramSplit(0), "M12_DiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_DiSimpanOld(ByVal param As String) As String
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
        'diid(0) As Integer, dicabang(1) As String, dilokasi(2) As String, disumber(3) As String, dikategoripos(4) As String, 
        'diautonotransaksi(5) As Integer, dinotransaksi(6) As String, ditgl(7) As Date, dikodepa(8) As , dikontak(9) As , 
        'dikontakperson(10) As String, diuraian(11) As String, dicatatan(12) As String, distatus(13) As Integer, distatussebelumnya(14) As Integer, 
        'dijmlrevisi(15) As Integer, dicetakanke(16) As Integer, diisclose(17) As Integer, diinputuser(18) As , diinputtgl(19) As DateTime, 
        'dimodifikasiuser(20) As , dimodifikasitgl(21) As DateTime, diposting(22) As Integer, dipostingtgl(23) As DateTime, dicustomtext1(24) As String, 
        'dicustomtext2(25) As String, dicustomtext3(26) As String, dicustomtext4(27) As String, dicustomtext5(28) As String, dicustomint1(29) As Integer, 
        'dicustomint2(30) As Integer, dicustomint3(31) As Integer, dicustomdbl1(32) As Double, dicustomdbl2(33) As Double, dicustomdbl3(34) As Double, 
        'dicustomdate1(35) As Date, dicustomdate2(36) As Date, dicustomdate3(37) As Date, dijeniskategori(38) As Int

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, 
        'ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, 
        'distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, 
        'dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, 
        'dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, 
        'dicustomdate1, dicustomdate2, dicustomdate3, dijeniskategori

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 39) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'diid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "diid required numeric." : GoTo selesai
        End If
        'diautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "diautonotransaksi required numeric." : GoTo selesai
        End If
        'ditgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ditgl required date." : GoTo selesai
        End If
        'distatus(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "distatus required numeric." : GoTo selesai
        End If
        'distatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "distatussebelumnya required numeric." : GoTo selesai
        End If
        'dijmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "dijmlrevisi required numeric." : GoTo selesai
        End If
        'dicetakanke(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "dicetakanke required numeric." : GoTo selesai
        End If
        'diisclose(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "diisclose required numeric." : GoTo selesai
        End If
        'diinputtgl(19) As DateTime
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "diinputtgl required date." : GoTo selesai
        End If
        'dimodifikasitgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "dimodifikasitgl required date." : GoTo selesai
        End If
        'diposting(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "diposting required numeric." : GoTo selesai
        End If
        'dipostingtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "dipostingtgl required date." : GoTo selesai
        End If
        'dicustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "dicustomint1 required numeric." : GoTo selesai
        End If
        'dicustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "dicustomint2 required numeric." : GoTo selesai
        End If
        'dicustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "dicustomint3 required numeric." : GoTo selesai
        End If
        'dicustomdbl1(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "dicustomdbl1 required numeric." : GoTo selesai
        End If
        'dicustomdbl2(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "dicustomdbl2 required numeric." : GoTo selesai
        End If
        'dicustomdbl3(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "dicustomdbl3 required numeric." : GoTo selesai
        End If
        'dicustomdate1(35) As Date
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "dicustomdate1 required date." : GoTo selesai
        End If
        'dicustomdate2(36) As Date
        If (IsDate(dataUtama(36)) = False) Then
            result(2) = "dicustomdate2 required date." : GoTo selesai
        End If
        'dicustomdate3(37) As Date
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "dicustomdate3 required date." : GoTo selesai
        End If

        'dijeniskategori(38) As Date
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "dijeniskategori required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'dicabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dicabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dicabang should not be more than 25 character." : GoTo selesai
        End If

        'dilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dilokasi should not be more than 25 character." : GoTo selesai
        End If

        'disumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "disumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "disumber should not be more than 10 character." : GoTo selesai
        End If

        'dikategoripos(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "dikategoripos can't be empty" : GoTo selesai
        'End If
        'If Len(dataUtama(4)) > 50 Then
        '    result(2) = "dikategoripos should not be more than 50 character." : GoTo selesai
        'End If

        'dinotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "dinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "dinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ditgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ditgl can't be empty" : GoTo selesai
        End If

        'dikodepa(8) As 
        If Len(dataUtama(8)) = 0 Then
            result(2) = "dikodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(8)) > 20 Then
            result(2) = "dikodepa should not be more than 20 character." : GoTo selesai
        End If

        'dikontak(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "dikontak can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "dikontak should not be more than 20 character." : GoTo selesai
        End If

        'diinputtgl(19) As DateTime
        If Len(dataUtama(19)) = 0 Then
            result(2) = "diinputtgl can't be empty" : GoTo selesai
        End If

        'dimodifikasitgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "dimodifikasitgl can't be empty" : GoTo selesai
        End If

        'dipostingtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "dipostingtgl can't be empty" : GoTo selesai
        End If

        'dicustomdbl1(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "dicustomdbl1 can't be empty" : GoTo selesai
        End If

        'dicustomdbl2(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "dicustomdbl2 can't be empty" : GoTo selesai
        End If

        'dicustomdbl3(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "dicustomdbl3 can't be empty" : GoTo selesai
        End If

        'dicustomdate1(35) As Date
        If Len(dataUtama(35)) = 0 Then
            result(2) = "dicustomdate1 can't be empty" : GoTo selesai
        End If

        'dicustomdate2(36) As Date
        If Len(dataUtama(36)) = 0 Then
            result(2) = "dicustomdate2 can't be empty" : GoTo selesai
        End If

        'dicustomdate3(37) As Date
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dicustomdate3 can't be empty" : GoTo selesai
        End If

        'dijeniskategori(38) As Date
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dijeniskategori can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "diid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "disumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dikategoripos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "diautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ditgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dikontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dikontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "diuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "distatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "distatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "diisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "diinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "diinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "diposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dipostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dijeniskategori", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "diid~dicabang~dilokasi~disumber~dikategoripos~diautonotransaksi~dinotransaksi~ditgl~dikodepa~dikontak~dikontakperson~diuraian~dicatatan~distatus~distatussebelumnya~dijmlrevisi~dicetakanke~diisclose~diinputuser~diinputtgl~dimodifikasiuser~dimodifikasitgl~diposting~dipostingtgl~dicustomtext1~dicustomtext2~dicustomtext3~dicustomtext4~dicustomtext5~dicustomint1~dicustomint2~dicustomint3~dicustomdbl1~dicustomdbl2~dicustomdbl3~dicustomdate1~dicustomdate2~dicustomdate3~dijeniskategori", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38)) = False Then
            result(2) = "insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, 
        'kriteria, nilai, tgl1, tgl2, jam1, jam2, catatan, 
        'urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, 
        'customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, nopromo

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddidetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "dikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "operator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kriteria", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtdetail, "nilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jam2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nopromo", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 30) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'jml1(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "jml1 required numeric." : GoTo selesai
            End If
            'jml2(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "jml2 required numeric." : GoTo selesai
            End If
            'kriteria(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "kriteria required numeric." : GoTo selesai
            End If
            'tgl1(9) As Date
            If (IsDate(dataRowDetail(9)) = False) Then
                result(2) = "tgl1 required date." : GoTo selesai
            End If
            'tgl2(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "tgl2 required date." : GoTo selesai
            End If
            'jam1(11) As Date
            If (IsDate(dataRowDetail(11)) = False) Then
                result(2) = "jam1 required date." : GoTo selesai
            End If
            'jam2(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "jam2 required date." : GoTo selesai
            End If
            'customint1(20) As Integer
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "customint1 required numeric." : GoTo selesai
            End If
            'customint2(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "customint2 required numeric." : GoTo selesai
            End If
            'customint3(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "customint3 required numeric." : GoTo selesai
            End If
            'customdbl1(23) As Double
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(24) As Double
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(25) As Double
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "customdate1 required date." : GoTo selesai
            End If
            'customdate2(27) As Date
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "customdate2 required date." : GoTo selesai
            End If
            'customdate3(28) As Date
            If (IsDate(dataRowDetail(28)) = False) Then
                result(2) = "customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'iddidetail(0) As 
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - iddidetail can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 20 Then
                result(2) = "Row : " & i & " - iddidetail should not be more than 20 character." : GoTo selesai
            End If

            'iddi(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - iddi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - iddi should not be more than 20 character." : GoTo selesai
            End If

            'dikategori(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - dikategori can't be empty" : GoTo selesai
            'End If
            'If Len(dataRowDetail(2)) > 25 Then
            '    result(2) = "Row : " & i & " - dikategori should not be more than 25 character." : GoTo selesai
            'End If

            'idbarang(3) As 
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 20 Then
                result(2) = "Row : " & i & " - idbarang should not be more than 20 character." : GoTo selesai
            End If

            'operator(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - operator can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - operator should not be more than 25 character." : GoTo selesai
            End If

            'jml1(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml1 can't be empty" : GoTo selesai
            End If

            'jml2(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jml2 can't be empty" : GoTo selesai
            End If

            'nilai(8) As String
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - nilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(8)) > 25 Then
                result(2) = "Row : " & i & " - nilai should not be more than 25 character." : GoTo selesai
            End If

            'tgl1(9) As Date
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - tgl1 can't be empty" : GoTo selesai
            End If

            'tgl2(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - tgl2 can't be empty" : GoTo selesai
            End If

            'jam1(11) As Date
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - jam1 can't be empty" : GoTo selesai
            End If

            'jam2(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jam2 can't be empty" : GoTo selesai
            End If

            'urutan(14) As 
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - urutan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 20 Then
                result(2) = "Row : " & i & " - urutan should not be more than 20 character." : GoTo selesai
            End If

            'customdbl1(23) As Double
            If Len(dataRowDetail(23)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(24) As Double
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(25) As Double
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(27) As Date
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(28) As Date
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "iddidetail~iddi~dikategori~idbarang~operator~jml1~jml2~kriteria~nilai~tgl1~tgl2~jam1~jam2~catatan~urutan~customtext1~customtext2~customtext3~customtext4~customtext5~customint1~customint2~customint3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~nopromo", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29))

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
                If isUpdate Then
                    result(4) = drutama("diid")
                    notransaksi = drutama("dinotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(diid), dinotransaksi FROM M_12_Di WHERE diid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(diid) FROM M_12_Di WHERE dinotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_di_history
                        Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("disumber")) & "▼" & FixQuotes(drutama("diid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Di set dicabang  = '" & FixQuotes(drutama("dicabang")) & "', dilokasi  = '" & FixQuotes(drutama("dilokasi")) & "', disumber  = '" & FixQuotes(drutama("disumber")) & "', dikategoripos  = '" & FixQuotes(drutama("dikategoripos")) & "', diautonotransaksi  = " & drutama("diautonotransaksi") & ", dinotransaksi  = '" & FixQuotes(drutama("dinotransaksi")) & "', ditgl  = '" & FixQuotes(AsFormatTanggal(drutama("ditgl"))) & "', dikodepa  = '" & FixQuotes(drutama("dikodepa")) & "', dikontak  = '" & FixQuotes(drutama("dikontak")) & "', dikontakperson  = '" & FixQuotes(drutama("dikontakperson")) & "', diuraian  = '" & FixQuotes(drutama("diuraian")) & "', dicatatan  = '" & FixQuotes(drutama("dicatatan")) & "', distatus  = " & drutama("distatus") & ", distatussebelumnya  = " & drutama("distatussebelumnya") & ", dijmlrevisi  = " & drutama("dijmlrevisi") & ", dicetakanke  = " & drutama("dicetakanke") & ", diisclose  = " & drutama("diisclose") & ", diinputuser  = '" & FixQuotes(drutama("diinputuser")) & "', diinputtgl  = '" & FixQuotes(AsFormatTanggal(drutama("diinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', dimodifikasiuser  = '" & FixQuotes(drutama("dimodifikasiuser")) & "', dimodifikasitgl  = NOW(), diposting  = " & drutama("diposting") & ", dipostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("dipostingtgl"), "yyyy-MM-dd H:mm:ss")) & "', dicustomtext1  = '" & FixQuotes(drutama("dicustomtext1")) & "', dicustomtext2  = '" & FixQuotes(drutama("dicustomtext2")) & "', dicustomtext3  = '" & FixQuotes(drutama("dicustomtext3")) & "', dicustomtext4  = '" & FixQuotes(drutama("dicustomtext4")) & "', dicustomtext5  = '" & FixQuotes(drutama("dicustomtext5")) & "', dicustomint1  = " & drutama("dicustomint1") & ", dicustomint2  = " & drutama("dicustomint2") & ", dicustomint3  = " & drutama("dicustomint3") & ", dicustomdbl1  = '" & FixDouble(drutama("dicustomdbl1")) & "', dicustomdbl2  = '" & FixDouble(drutama("dicustomdbl2")) & "', dicustomdbl3  = '" & FixDouble(drutama("dicustomdbl3")) & "', dicustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate1"))) & "', dicustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate2"))) & "', dicustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate3"))) & "', dijeniskategori  = '" & FixQuotes(drutama("dijeniskategori")) & "' where diid = " & drutama("diid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else

                    If drutama("diautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dicabang"), drutama("dilokasi"), drutama("disumber"), drutama("ditgl"))
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
                        notransaksi = drutama("dinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(diid) FROM m_12_di WHERE dinotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_12_di (dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dijeniskategori) values('" & FixQuotes(drutama("dicabang")) & "', '" & FixQuotes(drutama("dilokasi")) & "', '" & FixQuotes(drutama("disumber")) & "', '" & FixQuotes(drutama("dikategoripos")) & "', " & drutama("diautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ditgl"))) & "', '" & FixQuotes(drutama("dikodepa")) & "', '" & FixQuotes(drutama("dikontak")) & "', '" & FixQuotes(drutama("dikontakperson")) & "', '" & FixQuotes(drutama("diuraian")) & "', '" & FixQuotes(drutama("dicatatan")) & "', " & drutama("distatus") & ", " & drutama("distatussebelumnya") & ", " & drutama("dijmlrevisi") & ", " & drutama("dicetakanke") & ", " & drutama("diisclose") & ", '" & FixQuotes(drutama("diinputuser")) & "', NOW(), '" & FixQuotes(drutama("dimodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dimodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("dicustomtext1")) & "', '" & FixQuotes(drutama("dicustomtext2")) & "', '" & FixQuotes(drutama("dicustomtext3")) & "', '" & FixQuotes(drutama("dicustomtext4")) & "', '" & FixQuotes(drutama("dicustomtext5")) & "', " & drutama("dicustomint1") & ", " & drutama("dicustomint2") & ", " & drutama("dicustomint3") & ", '" & FixDouble(drutama("dicustomdbl1")) & "', '" & FixDouble(drutama("dicustomdbl2")) & "', '" & FixDouble(drutama("dicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dicustomdate3"))) & "', '" & FixQuotes(drutama("dijeniskategori")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select diid from M_12_di where dinotransaksi='" & notransaksi & "' AND diinputuser= '" & drutama("diinputuser") & "' order by dimodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If isUpdate = True Then
                    sql = "Delete from M_12_Di_Detail where iddi = " & result(4)
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

                        'CEK OPERATOR :
                        'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                        '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                        'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                        Dim dtOperator As New DataTable
                        sql = "SELECT did.dikategori as kategori, did.idbarang as idbarang, did.operator as operator, i.bkode, (CASE did.operator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_di_detail did JOIN m1_item i ON did.idbarang = i.bid WHERE did.dikategori = '" & FxDB(drutama("dikategoripos"), "") & "' AND did.idbarang = '" & FxDB(dr1("idbarang"), "") & "' AND did.iddi = '" & result(4) & "' AND did.iddidetail <> '" & FxDB(dr1("iddidetail"), "") & "' GROUP BY did.operator ORDER BY did.operator"
                        dtOperator = AsDataTableAmbilDariDB(sql)
                        If dtOperator.Rows.Count > 0 Then
                            Dim vOperator As String = ""
                            Dim vIdBarang As Integer = 0
                            For Each drOperator As DataRow In dtOperator.Rows
                                vOperator = FxDB(drOperator("operator").ToString, "")
                                vIdBarang = FxDB(drOperator("idbarang").ToString, "")
                                If Len(vOperator) > 0 Then
                                    If vOperator = 2 Then
                                        'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                        result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    Else
                                        'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                        'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                        'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                        If dr1("idbarang") = vIdBarang And (dr1("operator") = 2 Or (vOperator = 1 And dr1("operator") = vOperator)) Then
                                            result(2) = "Item : " & FxDB(drOperator("bkode"), "") & " - already has '" & FxDB(drOperator("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                        End If
                                    End If
                                End If
                            Next
                        End If

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("iddidetail")) & "', " & result(4) & ", '" & FixQuotes(dr1("dikategori")) & "', '" & FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("operator")) & "', '" & FixDouble(dr1("jml1")) & "', '" & FixDouble(dr1("jml2")) & "', " & dr1("kriteria") & ", '" & FixQuotes(dr1("nilai")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl2"))) & "', '" & FixQuotes(dr1("jam1")) & "', '" & FixQuotes(dr1("jam2")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("urutan")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & notransaksi & "')")

                        sql = "Insert into M_12_Di_Detail(iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, kriteria, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        strValue2.Clear()

                    Next

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                'Update ke tabel Barang Discount
                If drutama("distatus") = 2 Then 'JIKA STATUS APPROVED
                    If drutama("dijeniskategori") = 1 Then 'JIKA PER KATEGORI

                        'Cek apakah kategori pos sudah ada di tabel pos_bonus_item, jika sudah ada, hapus data di tabel itu
                        'HAPUS POS DISCOUNT ITEM
                        sql = "Delete From m_12_pos_discount_item where dikategori = '" & drutama("dikategoripos") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else 'JIKA SEMUA KATEGORI
                        'HAPUS POS DISCOUNT ITEM
                        sql = "Delete From m_12_pos_discount_item"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    Dim dtdtl As New DataTable
                    dtdtl = AsDataTableAmbilDariDB("select * from M_12_Di_Detail where iddi = '" & result(4) & "' order by iddi asc")
                    Dim strInsertDiscountItem As New StringBuilder 'untuk query simpan di tabel bonus utama
                    Dim dtKatPOS As New DataTable 'untuk menampung data kategori pos, jika jenis kategori 

                    If dtdtl.Rows.Count > 0 Then

                        If drutama("dijeniskategori") = 1 Then 'JIKA PER KATEGORI
                            For Each drdtl As DataRow In dtdtl.Rows
                                'persiapan insert ke tabel m_12_pos_discount_item & tabel m_12_pos_discount_item_detail
                                strInsertDiscountItem.Append(IIf(Len(strInsertDiscountItem.ToString) = 0, "", ", "))
                                strInsertDiscountItem.Append("('" & FixQuotes(drutama("dikategoripos")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', " & drdtl("kriteria") & ", '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("jam1")) & "', '" & FixQuotes(drdtl("jam2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                            Next

                            'insert ke tabel m_12_pos_discount_item
                            sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dinopromo) values" & strInsertDiscountItem.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        Else 'JIKA SEMUA KATEGORI
                            Dim dtPosItem As New DataTable 'variabel untuk cari data barang pos
                            'CARI DATA KATEGORI POS
                            dtKatPOS = AsDataTableAmbilDariDB("select * from m_12_pos_category")
                            For Each drKatPos As DataRow In dtKatPOS.Rows
                                For Each drdtl As DataRow In dtdtl.Rows
                                    'AMBIL DATA BARANG POS
                                    dtPosItem = AsDataTableAmbilDariDB("select * from M_12_Pos_Item where pikategori = '" & drKatPos("pckode") & "' AND piidbarang = '" & drdtl("idbarang") & "' order by pikategori asc")
                                    If dtPosItem.Rows.Count > 0 Then
                                        For Each drPosItem As DataRow In dtPosItem.Rows
                                            'persiapan insert ke tabel m_12_pos_discount_item 
                                            strInsertDiscountItem.Append(IIf(Len(strInsertDiscountItem.ToString) = 0, "", ", "))
                                            strInsertDiscountItem.Append("('" & FixQuotes(drKatPos("pckode")) & "', '" & FixQuotes(drdtl("idbarang")) & "', '" & FixQuotes(drdtl("operator")) & "', '" & FixDouble(drdtl("jml1")) & "', '" & FixDouble(drdtl("jml2")) & "', " & drdtl("kriteria") & ", '" & FixQuotes(drdtl("nilai")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("tgl2"))) & "', '" & FixQuotes(drdtl("jam1")) & "', '" & FixQuotes(drdtl("jam2")) & "', '" & FixQuotes(drdtl("customtext1")) & "', '" & FixQuotes(drdtl("customtext2")) & "', '" & FixQuotes(drdtl("customtext3")) & "', '" & FixQuotes(drdtl("customtext4")) & "', '" & FixQuotes(drdtl("customtext5")) & "', " & drdtl("customint1") & ", " & drdtl("customint2") & ", " & drdtl("customint3") & ", '" & FixDouble(drdtl("customdbl1")) & "', '" & FixDouble(drdtl("customdbl2")) & "', '" & FixDouble(drdtl("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drdtl("customdate3"))) & "', '" & FixQuotes(drdtl("nopromo")) & "')")
                                        Next
                                    End If

                                Next
                            Next

                            'insert ke tabel m_12_pos_discount_item
                            sql = "Insert into M_12_Pos_Discount_Item(dikategori, diidbarang, dioperator, dijml1, dijml2, dikriteria, dinilai, ditgl1, ditgl2, dijam1, dijam2, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dinopromo) values" & strInsertDiscountItem.ToString & ""
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
                        result(2) = "Main Transaction POS Discount Item data not found." : Trans.Rollback() : GoTo selesai
                    End If
                End If

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
        Con1.Close()
        Con1 = Nothing
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
    Public Function M12_DiUpdateStatusOld(ByVal param As String) As String
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
            Filter = Filter.Replace("dikontakkode", "c.kkode")
            Filter = Filter.Replace("dikontaknama", "c.knama")
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
            Dim sumber As String = "DI", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ditgl, Dinotransaksi, Distatus FROM m_12_Di WHERE Diid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Distatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            'CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================


            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m12_di_history
            Dim rsSimpanHistory As String = SimpanHistory.M12_Di_HistorySimpan("" & paramSplit(0) & "★M12_Di_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            If isDelete Then
                Dim dtutama As New DataTable
                dtutama = AsDataTableAmbilDariDB("SELECT * FROM M_12_Di WHERE diid=" & idtransaksi)
                If (dtutama.Rows.Count > 0) Then
                    For Each drutama As DataRow In dtutama.Rows
                        'AMBIL DATA DETAIL
                        dtdetail = AsDataTableAmbilDariDB("SELECT * FROM M_12_Di_Detail WHERE iddi=" & idtransaksi)
                        If (dtdetail.Rows.Count > 0) Then
                            For Each drdetail As DataRow In dtdetail.Rows
                                If drutama("dijeniskategori") = 1 Then 'JIKA PER KATEGORI
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_discount_item WHERE dikategori='" & drdetail("dikategori") & "' AND dinopromo = '" & drdetail("nopromo") & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = Con1
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Else 'JIKA SEMUA KATEGORI
                                    'hapus data detail
                                    Dim strValue2 As New StringBuilder
                                    sql = "Delete from M_12_pos_discount_item WHERE dinopromo = '" & drdetail("nopromo") & "'"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = Con1
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If

                            Next


                            'hapus data detail
                            'sql = "Delete from M_12_Bi_Detail WHERE idbidetail=" & idtransaksi
                            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            'With objCmd
                            '    .Connection = Con1
                            '    .Transaction = Trans
                            '    .CommandType = CommandType.Text
                            '    .CommandText = sql
                            'End With
                            'objCmd.ExecuteNonQuery()

                            ''jika status unclose maka nilai status ambil dari status sebelumnya
                            'If (nilaiStatus = "unclose") Then
                            '    Dim dtstatusbefore As DataTable
                            '    dtstatusbefore = AsDataTableAmbilDariDB("SELECT Bistatussebelumnya FROM M_12_Bi WHERE biid=" & idtransaksi)
                            '    nilaiStatus = Val(dtstatusbefore.Rows(0)(0))
                            'End If

                        End If
                    Next
                End If


            End If


            'update status utama
            sql = "UPDATE M_12_Di SET Distatus = " & nilaiStatus & ", dimodifikasiuser='" & userid & "', dimodifikasitgl = NOW(), diposting = 0, dipostingtgl = '1971-01-01 00:00:00', Dijmlrevisi = Dijmlrevisi + 1 WHERE diid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_DiSearch(PostWsSearch(paramSplit(0), "M12_DiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_DiDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("dikontakkode", "c.kkode")
            Filter = Filter.Replace("dikontaknama", "c.knama")
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
            Dim sumber As String = "DI", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT diid, dinotransaksi FROM m_12_di WHERE diid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dicabang, dilokasi, disumber, diautonotransaksi, dinotransaksi, ditgl"
            sql &= " FROM M_12_di"
            sql &= " WHERE diid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dicabang")
                lokasi = dtNomorNext.Rows(0)("dilokasi")
                sumber = dtNomorNext.Rows(0)("disumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("diautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("dinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ditgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_12_Di_Detail WHERE iddi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Di WHERE diid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_DiSearch(PostWsSearch(paramSplit(0), "M12_DiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_DiGetdataById(ByVal param As String) As String

        'M12_DiGetdataById Utama --------------------------------------------------------
        'diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, 
        'ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, 
        'distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, 
        'dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, 
        'dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, 
        'dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, dikontaknama
        'distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama, dikategoriposnama, dijeniskategori

        'M12_DiGetdataById Detail -------------------------------------------------------
        'iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, kriteria, 
        'nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, 
        'customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'nopromo, kodebarang, namabarang



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

        Dim utama As String = "", detail As String = "", discount As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Cr~M2_Cr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "diid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "diid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m12_di_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("diid"), 0), sptField,
                     FxDB(drutama("dicabang"), ""), sptField,
                     FxDB(drutama("dilokasi"), ""), sptField,
                     FxDB(drutama("disumber"), ""), sptField,
                     FxDB(drutama("dikategoripos"), ""), sptField,
                     FxDB(drutama("diautonotransaksi"), 0), sptField,
                     FxDB(drutama("dinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ditgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dikodepa"), ""), sptField,
                     FxDB(drutama("dikontak"), ""), sptField,
                     FxDB(drutama("dikontakperson"), ""), sptField,
                     FxDB(drutama("diuraian"), ""), sptField,
                     FxDB(drutama("dicatatan"), ""), sptField,
                     FxDB(drutama("distatus"), 0), sptField,
                     FxDB(drutama("distatussebelumnya"), 0), sptField,
                     FxDB(drutama("dijmlrevisi"), 0), sptField,
                     FxDB(drutama("dicetakanke"), 0), sptField,
                     FxDB(drutama("diisclose"), 0), sptField,
                     FxDB(drutama("diinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("diinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dimodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("diposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dipostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dicustomtext1"), ""), sptField,
                     FxDB(drutama("dicustomtext2"), ""), sptField,
                     FxDB(drutama("dicustomtext3"), ""), sptField,
                     FxDB(drutama("dicustomtext4"), ""), sptField,
                     FxDB(drutama("dicustomtext5"), ""), sptField,
                     FxDB(drutama("dicustomint1"), 0), sptField,
                     FxDB(drutama("dicustomint2"), 0), sptField,
                     FxDB(drutama("dicustomint3"), 0), sptField,
                     FxDB(drutama("dicustomdbl1"), 0), sptField,
                     FxDB(drutama("dicustomdbl2"), 0), sptField,
                     FxDB(drutama("dicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dicustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dicabangnama"), ""), sptField,
                     FxDB(drutama("dilokasinama"), ""), sptField,
                     FxDB(drutama("dikontakkode"), ""), sptField,
                     FxDB(drutama("dikontaknama"), ""), sptField,
                     FxDB(drutama("distatusnama"), ""), sptField,
                     FxDB(drutama("distatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("diinputusernama"), ""), sptField,
                     FxDB(drutama("dimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("dikategoriposnama"), ""), sptField,
                     FxDB(drutama("dijeniskategori"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("iddidetail"), ""), sptField,
                     FxDB(dr("iddi"), ""), sptField,
                     FxDB(dr("dikategori"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
                     FxDB(dr("operator"), ""), sptField,
                     FxDB(dr("jml1"), 0), sptField,
                     FxDB(dr("jml2"), 0), sptField,
                     FxDB(dr("kriteria"), 0), sptField,
                     FxDB(dr("nilai"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("tgl2"), ""), formatTgl), sptField,
                     FxDB(dr("jam1"), ""), sptField,
                     FxDB(dr("jam2"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("nopromo"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, dikontaknama, distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama, dikategoriposnama, dijeniskategori" & sptSubParam & "iddidetail, iddi, dikategori, idbarang, operator, jml1, jml2, kriteria, nilai, tgl1, tgl2, jam1, jam2, catatan, urutan, customtext1, customtext2, customtext3, customtext4, customtext5, customint1, customint2, customint3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, nopromo, kodebarang, namabarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_DiSearch(ByVal param As String) As String
        'M12_DiSearch --------------------------------------------------------
        'diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, 
        'ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, 
        'distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, 
        'dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, 
        'dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, 
        'dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, 
        'dikontaknama, distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama

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
        sql = "select `di`.`diid` AS `diid`,`di`.`dicabang` AS `dicabang`,`di`.`dilokasi` AS `dilokasi`,`di`.`disumber` AS `disumber`,`di`.`diautonotransaksi` AS `diautonotransaksi`,`di`.`dinotransaksi` AS `dinotransaksi`,`di`.`ditgl` AS `ditgl`,`di`.`dikodepa` AS `dikodepa`,`di`.`dikontak` AS `dikontak`,`di`.`dikontakperson` AS `dikontakperson`,`di`.`dikategoripos` AS `dikategoripos`,`di`.`diuraian` AS `diuraian`,`di`.`dicatatan` AS `dicatatan`,`di`.`distatus` AS `distatus`,`di`.`distatussebelumnya` AS `distatussebelumnya`,`di`.`dijmlrevisi` AS `dijmlrevisi`,`di`.`dicetakanke` AS `dicetakanke`,`di`.`diisclose` AS `diisclose`,`di`.`diinputuser` AS `diinputuser`,`di`.`diinputtgl` AS `diinputtgl`,`di`.`dimodifikasiuser` AS `dimodifikasiuser`,`di`.`dimodifikasitgl` AS `dimodifikasitgl`,`di`.`diposting` AS `diposting`,`di`.`dipostingtgl` AS `dipostingtgl`,`di`.`dicustomtext1` AS `dicustomtext1`,`di`.`dicustomtext2` AS `dicustomtext2`,`di`.`dicustomtext3` AS `dicustomtext3`,`di`.`dicustomtext4` AS `dicustomtext4`,`di`.`dicustomtext5` AS `dicustomtext5`,`di`.`dicustomint1` AS `dicustomint1`,`di`.`dicustomint2` AS `dicustomint2`,`di`.`dicustomint3` AS `dicustomint3`,`di`.`dicustomdbl1` AS `dicustomdbl1`,`di`.`dicustomdbl2` AS `dicustomdbl2`,`di`.`dicustomdbl3` AS `dicustomdbl3`,`di`.`dicustomdate1` AS `dicustomdate1`,`di`.`dicustomdate2` AS `dicustomdate2`,`di`.`dicustomdate3` AS `dicustomdate3`,`br`.`bnama` AS `dicabangnama`,`lc`.`lnama` AS `dilokasinama`,`c`.`kkode` AS `dikontakkode`,`c`.`knama` AS `dikontaknama`,`st1`.`nama` AS `distatusnama`,`st2`.`nama` AS `distatussebelumnyanama`,`u1`.`unama` AS `diinputusernama`,`u2`.`unama` AS `dimodifikasiusernama` from (((((((`m_12_di` `di` left join `m1_branch` `br` on((`di`.`dicabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`di`.`dilokasi` = `lc`.`lkode`))) left join `m1_contact` `c` on((`di`.`dikontak` = `c`.`kid`))) left join `m0_status` `st1` on((`di`.`distatus` = `st1`.`kode`))) left join `m0_status` `st2` on((`di`.`distatussebelumnya` = `st2`.`kode`))) left join `m0_user` `u1` on((`di`.`diinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`di`.`dimodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr~M2_Cr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("diid"), 0), sptField,
                             FxDB(dr("dicabang"), ""), sptField,
                             FxDB(dr("dilokasi"), ""), sptField,
                             FxDB(dr("disumber"), ""), sptField,
                             FxDB(dr("dikategoripos"), ""), sptField,
                             FxDB(dr("diautonotransaksi"), 0), sptField,
                             FxDB(dr("dinotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("ditgl"), ""), formatTgl), sptField,
                             FxDB(dr("dikodepa"), ""), sptField,
                             FxDB(dr("dikontak"), ""), sptField,
                             FxDB(dr("dikontakperson"), ""), sptField,
                             FxDB(dr("diuraian"), ""), sptField,
                             FxDB(dr("dicatatan"), ""), sptField,
                             FxDB(dr("distatus"), 0), sptField,
                             FxDB(dr("distatussebelumnya"), 0), sptField,
                             FxDB(dr("dijmlrevisi"), 0), sptField,
                             FxDB(dr("dicetakanke"), 0), sptField,
                             FxDB(dr("diisclose"), 0), sptField,
                             FxDB(dr("diinputuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("diinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("dimodifikasiuser"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("dimodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("diposting"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("dipostingtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("dicustomtext1"), ""), sptField,
                             FxDB(dr("dicustomtext2"), ""), sptField,
                             FxDB(dr("dicustomtext3"), ""), sptField,
                             FxDB(dr("dicustomtext4"), ""), sptField,
                             FxDB(dr("dicustomtext5"), ""), sptField,
                             FxDB(dr("dicustomint1"), 0), sptField,
                             FxDB(dr("dicustomint2"), 0), sptField,
                             FxDB(dr("dicustomint3"), 0), sptField,
                             FxDB(dr("dicustomdbl1"), 0), sptField,
                             FxDB(dr("dicustomdbl2"), 0), sptField,
                             FxDB(dr("dicustomdbl3"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("dicustomdate1"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("dicustomdate2"), ""), formatTgl), sptField,
                             AsFormatTanggal(FxDB(dr("dicustomdate3"), ""), formatTgl), sptField,
                             FxDB(dr("dicabangnama"), ""), sptField,
                             FxDB(dr("dilokasinama"), ""), sptField,
                             FxDB(dr("dikontakkode"), ""), sptField,
                             FxDB(dr("dikontaknama"), ""), sptField,
                             FxDB(dr("distatusnama"), ""), sptField,
                             FxDB(dr("distatussebelumnyanama"), ""), sptField,
                             FxDB(dr("diinputusernama"), ""), sptField,
                             FxDB(dr("dimodifikasiusernama"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = pg1.isPaging
            resultPaging(1) = pg1.isNext
            resultPaging(2) = pg1.isPrev
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("diid, dicabang, dilokasi, disumber, dikategoripos, diautonotransaksi, dinotransaksi, ditgl, dikodepa, dikontak, dikontakperson, diuraian, dicatatan, distatus, distatussebelumnya, dijmlrevisi, dicetakanke, diisclose, diinputuser, diinputtgl, dimodifikasiuser, dimodifikasitgl, diposting, dipostingtgl, dicustomtext1, dicustomtext2, dicustomtext3, dicustomtext4, dicustomtext5, dicustomint1, dicustomint2, dicustomint3, dicustomdbl1, dicustomdbl2, dicustomdbl3, dicustomdate1, dicustomdate2, dicustomdate3, dicabangnama, dilokasinama, dikontakkode, dikontaknama, distatusnama, distatussebelumnyanama, diinputusernama, dimodifikasiusernama"))

        Return wsResult
    End Function

End Class
