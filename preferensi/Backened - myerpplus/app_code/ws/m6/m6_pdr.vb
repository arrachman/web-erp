Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_pdr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_PdrSimpan(ByVal param As String) As String
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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'pdrid(0) As Integer, pdrcabang(1) As String, pdrlokasi(2) As String, pdrgudangasal(3) As String, pdrgudangproduksi(4) As String, 
        'pdrgudangtujuan(5) As String, pdrsumber(6) As String, pdrjenis(7) As String, pdrautonotransaksi(8) As Integer, pdrnotransaksi(9) As String, 
        'pdrtgl(10) As Date, pdrkodepa(11) As Integer, pdrdimintaoleh(12) As Integer, pdrdimintaolehkontak(13) As String, pdrmintake(14) As Integer, 
        'pdrtgldipakai(15) As Date, pdrestimasikerja(16) As String, pdrmatauang(17) As String, pdrkurs(18) As Double, pdrtotalhargain(19) As Double, 
        'pdrtotalhargaout(20) As Double, pdrtotalhppin(21) As Double, pdrtotalhppout(22) As Double, pdruraian(23) As String, pdrcatatan(24) As String, 
        'pdrnoref(25) As String, pdrtglnoref(26) As Date, pdridbom(27) As Integer, pdrstatuswoin(28) As Integer, pdrstatuswoout(29) As Integer, 
        'pdrstatusmrsin(30) As Integer, pdrstatusmrsout(31) As Integer, pdrstatusmrnin(32) As Integer, pdrstatusmrnout(33) As Integer, pdrstatuspdin(34) As Integer, 
        'pdrstatuspdout(35) As Integer, pdrstatus(36) As Integer, pdrstatussebelumnya(37) As Integer, pdrjmlrevisi(38) As Integer, pdrcetakanke(39) As Integer, 
        'pdrinputuser(40) As Integer, pdrinputtgl(41) As DateTime, pdrmodifikasiuser(42) As Integer, pdrmodifikasitgl(43) As DateTime, pdrisclose(44) As Integer, 
        'pdrcustomtext1(45) As String, pdrcustomtext2(46) As String, pdrcustomtext3(47) As String, pdrcustomtext4(48) As String, pdrcustomtext5(49) As String, 
        'pdrcustomint1(50) As Integer, pdrcustomint2(51) As Integer, pdrcustomint3(52) As Integer, pdrcustomdbl1(53) As Double, pdrcustomdbl2(54) As Double, 
        'pdrcustomdbl3(55) As Double, pdrcustomdate1(56) As Date, pdrcustomdate2(57) As Date, pdrcustomdate3(58) As Date, pdraktivitas(59) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, 
        'pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, 
        'pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, 
        'pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, 
        'pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, 
        'pdrstatuspdout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, 
        'pdrmodifikasiuser, pdrmodifikasitgl, pdrisclose, pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, 
        'pdrcustomtext5, pdrcustomint1, pdrcustomint2, pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, 
        'pdrcustomdate1, pdrcustomdate2, pdrcustomdate3, pdraktivitas

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 59 And dataUtama.Length <> 60) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pdrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pdrid required numeric." : GoTo selesai
        End If
        'pdrautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pdrautonotransaksi required numeric." : GoTo selesai
        End If
        'pdrtgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "pdrtgl required date." : GoTo selesai
        End If
        'pdrkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "pdrkodepa required numeric." : GoTo selesai
        End If
        'pdrdimintaoleh(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "pdrdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "pdrdimintaoleh can't be empty." : GoTo selesai
        End If
        'pdrmintake(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "pdrmintake required numeric." : GoTo selesai
        End If
        'pdrtgldipakai(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "pdrtgldipakai required date." : GoTo selesai
        End If
        'pdrkurs(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pdrkurs required numeric." : GoTo selesai
        End If
        'pdrtotalhargain(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pdrtotalhargain required numeric." : GoTo selesai
        End If
        'pdrtotalhargaout(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "pdrtotalhargaout required numeric." : GoTo selesai
        End If
        'pdrtotalhppin(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "pdrtotalhppin required numeric." : GoTo selesai
        End If
        'pdrtotalhppout(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pdrtotalhppout required numeric." : GoTo selesai
        End If
        'pdrtglnoref(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "pdrtglnoref required date." : GoTo selesai
        End If
        'pdridbom(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pdridbom required numeric." : GoTo selesai
        End If
        'pdrstatuswoin(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pdrstatuswoin required numeric." : GoTo selesai
        End If
        'pdrstatuswoout(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "pdrstatuswoout required numeric." : GoTo selesai
        End If
        'pdrstatusmrsin(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pdrstatusmrsin required numeric." : GoTo selesai
        End If
        'pdrstatusmrsout(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "pdrstatusmrsout required numeric." : GoTo selesai
        End If
        'pdrstatusmrnin(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pdrstatusmrnin required numeric." : GoTo selesai
        End If
        'pdrstatusmrnout(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pdrstatusmrnout required numeric." : GoTo selesai
        End If
        'pdrstatuspdin(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pdrstatuspdin required numeric." : GoTo selesai
        End If
        'pdrstatuspdout(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pdrstatuspdout required numeric." : GoTo selesai
        End If
        'pdrstatus(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pdrstatus required numeric." : GoTo selesai
        End If
        'pdrstatussebelumnya(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pdrstatussebelumnya required numeric." : GoTo selesai
        End If
        'pdrjmlrevisi(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pdrjmlrevisi required numeric." : GoTo selesai
        End If
        'pdrcetakanke(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pdrcetakanke required numeric." : GoTo selesai
        End If
        'pdrinputuser(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pdrinputuser required numeric." : GoTo selesai
        End If
        'pdrinputtgl(41) As DateTime
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "pdrinputtgl required date." : GoTo selesai
        End If
        'pdrmodifikasiuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pdrmodifikasiuser required numeric." : GoTo selesai
        End If
        'pdrmodifikasitgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "pdrmodifikasitgl required date." : GoTo selesai
        End If
        'pdrisclose(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "pdrisclose required numeric." : GoTo selesai
        End If
        'pdrcustomint1(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "pdrcustomint1 required numeric." : GoTo selesai
        End If
        'pdrcustomint2(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pdrcustomint2 required numeric." : GoTo selesai
        End If
        'pdrcustomint3(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pdrcustomint3 required numeric." : GoTo selesai
        End If
        'pdrcustomdbl1(53) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pdrcustomdbl1 required numeric." : GoTo selesai
        End If
        'pdrcustomdbl2(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pdrcustomdbl2 required numeric." : GoTo selesai
        End If
        'pdrcustomdbl3(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pdrcustomdbl3 required numeric." : GoTo selesai
        End If
        'pdrcustomdate1(56) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "pdrcustomdate1 required date." : GoTo selesai
        End If
        'pdrcustomdate2(57) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "pdrcustomdate2 required date." : GoTo selesai
        End If
        'pdrcustomdate3(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "pdrcustomdate3 required date." : GoTo selesai
        End If

        If dataUtama.Length > 59 Then
            'pdraktivitas(59) As Integer
            If (IsNumeric(dataUtama(59)) = False) Then
                result(2) = "pdraktivitas required numeric." : GoTo selesai
            End If
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pdrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pdrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pdrcabang should not be more than 25 character." : GoTo selesai
        End If

        'pdrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pdrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pdrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'pdrgudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pdrgudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pdrgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'pdrgudangproduksi(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "pdrgudangproduksi can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "pdrgudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'pdrgudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "pdrgudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "pdrgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'pdrsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pdrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "pdrsumber should not be more than 10 character." : GoTo selesai
        End If

        'pdrjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pdrjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "pdrjenis should not be more than 25 character." : GoTo selesai
        End If

        'pdrnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pdrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "pdrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pdrtgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "pdrtgl can't be empty" : GoTo selesai
        End If

        'pdrtgldipakai(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "pdrtgldipakai can't be empty" : GoTo selesai
        End If

        'pdrmatauang(17) As String
        If Len(dataUtama(17)) = 0 Then
            result(2) = "pdrmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(17)) > 25 Then
            result(2) = "pdrmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pdrkurs(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "pdrkurs can't be empty" : GoTo selesai
        End If

        'pdrtotalhargain(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pdrtotalhargain can't be empty" : GoTo selesai
        End If

        'pdrtotalhargaout(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "pdrtotalhargaout can't be empty" : GoTo selesai
        End If

        'pdrtotalhppin(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "pdrtotalhppin can't be empty" : GoTo selesai
        End If

        'pdrtotalhppout(22) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = "pdrtotalhppout can't be empty" : GoTo selesai
        End If

        'pdrtglnoref(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "pdrtglnoref can't be empty" : GoTo selesai
        End If

        'pdrinputtgl(41) As DateTime
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pdrinputtgl can't be empty" : GoTo selesai
        End If

        'pdrmodifikasitgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pdrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pdrcustomdbl1(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "pdrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pdrcustomdbl2(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "pdrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pdrcustomdbl3(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "pdrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pdrcustomdate1(56) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "pdrcustomdate1 can't be empty" : GoTo selesai
        End If

        'pdrcustomdate2(57) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "pdrcustomdate2 can't be empty" : GoTo selesai
        End If

        'pdrcustomdate3(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "pdrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pdrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrgudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrtgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdridbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuswoin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuswoout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrsin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrsout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrnin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrnout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuspdin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuspdout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdraktivitas", AsEnumTypeData.AsInt64)
        If dataUtama.Length > 59 Then
            If AsDataTableTambahData(dtutama, "pdrid~pdrcabang~pdrlokasi~pdrgudangasal~pdrgudangproduksi~pdrgudangtujuan~pdrsumber~pdrjenis~pdrautonotransaksi~pdrnotransaksi~pdrtgl~pdrkodepa~pdrdimintaoleh~pdrdimintaolehkontak~pdrmintake~pdrtgldipakai~pdrestimasikerja~pdrmatauang~pdrkurs~pdrtotalhargain~pdrtotalhargaout~pdrtotalhppin~pdrtotalhppout~pdruraian~pdrcatatan~pdrnoref~pdrtglnoref~pdridbom~pdrstatuswoin~pdrstatuswoout~pdrstatusmrsin~pdrstatusmrsout~pdrstatusmrnin~pdrstatusmrnout~pdrstatuspdin~pdrstatuspdout~pdrstatus~pdrstatussebelumnya~pdrjmlrevisi~pdrcetakanke~pdrinputuser~pdrinputtgl~pdrmodifikasiuser~pdrmodifikasitgl~pdrisclose~pdrcustomtext1~pdrcustomtext2~pdrcustomtext3~pdrcustomtext4~pdrcustomtext5~pdrcustomint1~pdrcustomint2~pdrcustomint3~pdrcustomdbl1~pdrcustomdbl2~pdrcustomdbl3~pdrcustomdate1~pdrcustomdate2~pdrcustomdate3~pdraktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "pdrid~pdrcabang~pdrlokasi~pdrgudangasal~pdrgudangproduksi~pdrgudangtujuan~pdrsumber~pdrjenis~pdrautonotransaksi~pdrnotransaksi~pdrtgl~pdrkodepa~pdrdimintaoleh~pdrdimintaolehkontak~pdrmintake~pdrtgldipakai~pdrestimasikerja~pdrmatauang~pdrkurs~pdrtotalhargain~pdrtotalhargaout~pdrtotalhppin~pdrtotalhppout~pdruraian~pdrcatatan~pdrnoref~pdrtglnoref~pdridbom~pdrstatuswoin~pdrstatuswoout~pdrstatusmrsin~pdrstatusmrsout~pdrstatusmrnin~pdrstatusmrnout~pdrstatuspdin~pdrstatuspdout~pdrstatus~pdrstatussebelumnya~pdrjmlrevisi~pdrcetakanke~pdrinputuser~pdrinputtgl~pdrmodifikasiuser~pdrmodifikasitgl~pdrisclose~pdrcustomtext1~pdrcustomtext2~pdrcustomtext3~pdrcustomtext4~pdrcustomtext5~pdrcustomint1~pdrcustomint2~pdrcustomint3~pdrcustomdbl1~pdrcustomdbl2~pdrcustomdbl3~pdrcustomdate1~pdrcustomdate2~pdrcustomdate3~pdraktivitas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If


        'MAPPING BUAT WS DATA DETAIL1 -------------------------------------------------------
        'idpdrin(0) As Integer, idpdr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, idbomin(27) As Integer, jmlwo(28) As Double, statuswo(29) As Integer, 
        'jmlmrs(30) As Double, statusmrs(31) As Integer, jmlmrn(32) As Double, statusmrn(33) As Integer, jmlpd(34) As Double, 
        'statuspd(35) As Integer, isclose(36) As Integer, customtext1(37) As String, customtext2(38) As String, customtext3(39) As String, 
        'customdbl1(40) As Double, customdbl2(41) As Double, customdbl3(42) As Double, customdate1(43) As Date, customdate2(44) As Date, 
        'customdate3(45) As Date

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idpdrin, idpdr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, 
        'jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable DETAIL1
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpdrin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlwo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuswo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspd", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, idbomin As Integer = 0, idbomout As Integer = 0

        Dim ftExistOutstandingBomIn As String = "", ftOutstandingBomIn As String = ""
        Dim ftExistOutstandingBomOut As String = "", ftOutstandingBomOut As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL1 ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL1 -----------------------------------
            'CEK ARRAY DATA DETAIL1
            If (dataRowDetail.Length <> 46) Then
                result(2) = "Detail 1 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL1 ----------------------------

            'VALIDASI TIPE DATA DETAIL1 ------------------------------------------
            'idpdrin(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idpdrin required numeric." : GoTo selesai
            End If
            'idpdr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idpdr required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
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
            'idbomin(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbomin required numeric." : GoTo selesai
            End If
            'jmlwo(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlwo required numeric." : GoTo selesai
            End If
            'statuswo(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statuswo required numeric." : GoTo selesai
            End If
            'jmlmrs(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
            End If
            'statusmrs(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrs required numeric." : GoTo selesai
            End If
            'jmlmrn(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
            End If
            'statusmrn(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrn required numeric." : GoTo selesai
            End If
            'jmlpd(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(43) As Date
            If (IsDate(dataRowDetail(43)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(44) As Date
            If (IsDate(dataRowDetail(44)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(45) As Date
            If (IsDate(dataRowDetail(45)) = False) Then
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
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
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

            'jmlwo(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlwo can't be empty" : GoTo selesai
            End If

            'jmlmrs(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
            End If

            'jmlmrn(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
            End If

            'jmlpd(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(43) As Date
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(44) As Date
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(45) As Date
            If Len(dataRowDetail(45)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL1 --------------------------------

            If AsDataTableTambahData(dtdetail, "idpdrin~idpdr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomin~jmlwo~statuswo~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45)) = False Then
                result(2) = "Detail 1 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , idbomin(27) As Integer
            idbarang = dataRowDetail(2) : idbomin = dataRowDetail(27)

            'VALIDASI OUTSTANDING -------------------------
            'If idbomin <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingBomIn = IIf(Len(ftExistOutstandingBomIn.ToString) = 0, "", ftExistOutstandingBomIn & " UNION ")
            '    ftExistOutstandingBomIn = String.Concat(ftExistOutstandingBomIn, "SELECT EXISTS(SELECT 1 FROM m6_bom_in JOIN m6_bom ON idbom = bomid WHERE idbomin = '" & idbomin & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomin & "' as idbomin, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
            '    '2. CEK JML OUTSTANDING
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbomin=" & idbomin)
            '    ftOutstandingBomIn = IIf(Len(ftOutstandingBomIn.ToString) = 0, "", ftOutstandingBomIn & " OR ")
            '    ftOutstandingBomIn = String.Concat(ftOutstandingBomIn, " (bomin.idbomin = " & idbomin & " AND " & Outstanding & " > bomin.jmlbarang) ")
            'End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL1 ===========================================


        'MAPPING BUAT WS DATA DETAIL2 -------------------------------------------------------
        'idpdrout(0) As Integer, idpdr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idbomout(28) As Integer, jmlwo(29) As Double, 
        'statuswo(30) As Integer, jmlmrs(31) As Double, statusmrs(32) As Integer, jmlmrn(33) As Double, statusmrn(34) As Integer, 
        'jmlpd(35) As Double, statuspd(36) As Integer, isclose(37) As Integer, customtext1(38) As String, customtext2(39) As String, 
        'customtext3(40) As String, customdbl1(41) As Double, customdbl2(42) As Double, customdbl3(43) As Double, customdate1(44) As Date, 
        'customdate2(45) As Date, customdate3(46) As Date

        'MAPPING BUAT FLEX DATA DETAIL2 -----------------------------------------------------
        'idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL2 ======================================================
        'SPLIT PARAMETER DATA DETAIL2
        dataDetail2 = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL2 ===============================================

        'Buat datatable DETAIL2
        Dim dtdetail2 As New DataTable
        AsDataTableTambahField(dtdetail2, "idpdrout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jmlbarang", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail2, "idbomout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlwo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statuswo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statuspd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "isclose", AsEnumTypeData.AsInt64)
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
            If dataDetail2(i - 1).Length > 0 Then
                'SPLIT DATA DETAIL
                dataRowDetail2 = dataDetail2(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL2 -----------------------------------
                'CEK ARRAY DATA DETAIL2
                If (dataRowDetail2.Length <> 47) Then
                    result(2) = "Detail 2 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL2 ----------------------------

                'VALIDASI TIPE DATA DETAIL2 ------------------------------------------
                'idpdrout(0) As Integer
                If (IsNumeric(dataRowDetail2(0)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idpdrout required numeric." : GoTo selesai
                End If
                'idpdr(1) As Integer
                If (IsNumeric(dataRowDetail2(1)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idpdr required numeric." : GoTo selesai
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
                'idbomout(28) As Integer
                If (IsNumeric(dataRowDetail2(28)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idbomout required numeric." : GoTo selesai
                End If
                'jmlwo(29) As Double
                If (IsNumeric(dataRowDetail2(29)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlwo required numeric." : GoTo selesai
                End If
                'statuswo(30) As Integer
                If (IsNumeric(dataRowDetail2(30)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statuswo required numeric." : GoTo selesai
                End If
                'jmlmrs(31) As Double
                If (IsNumeric(dataRowDetail2(31)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
                End If
                'statusmrs(32) As Integer
                If (IsNumeric(dataRowDetail2(32)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statusmrs required numeric." : GoTo selesai
                End If
                'jmlmrn(33) As Double
                If (IsNumeric(dataRowDetail2(33)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
                End If
                'statusmrn(34) As Integer
                If (IsNumeric(dataRowDetail2(34)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statusmrn required numeric." : GoTo selesai
                End If
                'jmlpd(35) As Double
                If (IsNumeric(dataRowDetail2(35)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlpd required numeric." : GoTo selesai
                End If
                'statuspd(36) As Integer
                If (IsNumeric(dataRowDetail2(36)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statuspd required numeric." : GoTo selesai
                End If
                'isclose(37) As Integer
                If (IsNumeric(dataRowDetail2(37)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(41) As Double
                If (IsNumeric(dataRowDetail2(41)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(42) As Double
                If (IsNumeric(dataRowDetail2(42)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(43) As Double
                If (IsNumeric(dataRowDetail2(43)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(44) As Date
                If (IsDate(dataRowDetail2(44)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(45) As Date
                If (IsDate(dataRowDetail2(45)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(46) As Date
                If (IsDate(dataRowDetail2(46)) = False) Then
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
                If Len(dataRowDetail2(16)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
                End If
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

                'jmlwo(29) As Double
                If Len(dataRowDetail2(29)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlwo can't be empty" : GoTo selesai
                End If

                'jmlmrs(31) As Double
                If Len(dataRowDetail2(31)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
                End If

                'jmlmrn(33) As Double
                If Len(dataRowDetail2(33)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
                End If

                'jmlpd(35) As Double
                If Len(dataRowDetail2(35)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
                End If

                'customdbl1(41) As Double
                If Len(dataRowDetail2(41)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(42) As Double
                If Len(dataRowDetail2(42)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(43) As Double
                If Len(dataRowDetail2(43)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(44) As Date
                If Len(dataRowDetail2(44)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(45) As Date
                If Len(dataRowDetail2(45)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(46) As Date
                If Len(dataRowDetail2(46)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA DETAIL2 --------------------------------

                If AsDataTableTambahData(dtdetail2, "idpdrout~idpdr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususmasuk~idhppfifomasuk~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomout~jmlwo~statuswo~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail2(0) & "~" & dataRowDetail2(1) & "~" & dataRowDetail2(2) & "~" & dataRowDetail2(3) & "~" & dataRowDetail2(4) & "~" & dataRowDetail2(5) & "~" & dataRowDetail2(6) & "~" & dataRowDetail2(7) & "~" & dataRowDetail2(8) & "~" & dataRowDetail2(9) & "~" & dataRowDetail2(10) & "~" & dataRowDetail2(11) & "~" & dataRowDetail2(12) & "~" & dataRowDetail2(13) & "~" & dataRowDetail2(14) & "~" & dataRowDetail2(15) & "~" & dataRowDetail2(16) & "~" & dataRowDetail2(17) & "~" & dataRowDetail2(18) & "~" & dataRowDetail2(19) & "~" & dataRowDetail2(20) & "~" & dataRowDetail2(21) & "~" & dataRowDetail2(22) & "~" & dataRowDetail2(23) & "~" & dataRowDetail2(24) & "~" & dataRowDetail2(25) & "~" & dataRowDetail2(26) & "~" & dataRowDetail2(27) & "~" & dataRowDetail2(28) & "~" & dataRowDetail2(29) & "~" & dataRowDetail2(30) & "~" & dataRowDetail2(31) & "~" & dataRowDetail2(32) & "~" & dataRowDetail2(33) & "~" & dataRowDetail2(34) & "~" & dataRowDetail2(35) & "~" & dataRowDetail2(36) & "~" & dataRowDetail2(37) & "~" & dataRowDetail2(38) & "~" & dataRowDetail2(39) & "~" & dataRowDetail2(40) & "~" & dataRowDetail2(41) & "~" & dataRowDetail2(42) & "~" & dataRowDetail2(43) & "~" & dataRowDetail2(44) & "~" & dataRowDetail2(45) & "~" & dataRowDetail2(46)) = False Then
                    result(2) = "Detail 2 Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'ValidasiSimpan
                'idbarang(2) As Integer      , idbomout(28) As Integer
                idbarang = dataRowDetail2(2) : idbomout = dataRowDetail2(28)

                'VALIDASI OUTSTANDING -------------------------
                'If idbomout <> 0 Then
                '    '1. CEK DATA EXIST ------------------------
                '    ftExistOutstandingBomOut = IIf(Len(ftExistOutstandingBomOut.ToString) = 0, "", ftExistOutstandingBomOut & " UNION ")
                '    ftExistOutstandingBomOut = String.Concat(ftExistOutstandingBomOut, "SELECT EXISTS(SELECT 1 FROM m6_bom_out JOIN m6_bom ON idbom = bomid WHERE idbomout = '" & idbomout & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomout & "' as idbomout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                '    '2. CEK JML OUTSTANDING
                '    Dim Outstanding As Double = AsDataTableDSum(dtdetail2, "jmlbarang", "idbomout=" & idbomout)
                '    ftOutstandingBomOut = IIf(Len(ftOutstandingBomOut.ToString) = 0, "", ftOutstandingBomOut & " OR ")
                '    ftOutstandingBomOut = String.Concat(ftOutstandingBomOut, " (bomout.idbomout = " & idbomout & " AND " & Outstanding & " > bomout.jmlbarang) ")
                'End If
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------
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
                Dim vModuleId As Integer = 6, vMenuId As Integer = 4
                Select Case drutama("pdrstatus")
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pdrtgl")), AsFormatTanggal(drutama("pdrtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                ''VALIDASI SIMPAN ========================================
                ''ValidasiSimpan
                'If drutama("pdrstatus") = 2 Then
                '    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingBomIn, ftOutstandingBomIn, dtdetail2, ftExistOutstandingBomOut, ftOutstandingBomOut)
                '    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("pdrid")
                    notransaksi = drutama("pdrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(pdrid), pdrnotransaksi FROM M6_pdr WHERE pdrid='" & result(4) & "' AND pdrstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("pdrautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pdrcabang"), drutama("pdrlokasi"), drutama("pdrsumber"), drutama("pdrtgl"))
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
                            Dim vAkunPD As String = "", vAkunSI As String = "", sqlambil As String = ""
                            Dim dt As New DataTable
                            sqlambil = "SELECT IFNULL(c.cnomor,'') as akunPD, IFNULL(c2.cnomor,'') as akunSI FROM m1_location l LEFT JOIN m1_coa c ON l.lalamat2 = c.cnomor LEFT JOIN m1_coa c2 ON l.lkota = c2.cnomor WHERE l.lkode = '" & drutama("pdrlokasi") & "'"
                            dt = AsDataTableAmbilDariDBCon(sqlambil, myConn)
                            If dt.Rows.Count > 0 Then
                                vAkunPD = FxDB(dt.Rows(0)("akunPD"), "")
                                vAkunSI = FxDB(dt.Rows(0)("akunSI"), "")
                            Else
                                result(2) = "Could not find Transaction Code for '" & drutama("pdrlokasi") & "' location." : Trans.Rollback() : GoTo selesai
                            End If

                            'sql = "INSERT INTO `m1_cost_center` (`cckode`, `ccnama`, `ccakun`, `cccatatan`) VALUES ('" & FixQuotes(notransaksi) & "', '" & FixQuotes(notransaksi) & "', '" & FixQuotes(vAkunPD) & "', '" & FixQuotes(vAkunSI) & "') ON DUPLICATE KEY UPDATE cckode = VALUES(cckode);"
                            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            'With objCmd
                            '    .Connection = myConn
                            '    .Transaction = Trans
                            '    .CommandType = CommandType.Text
                            '    .CommandText = sql
                            'End With
                            'objCmd.ExecuteNonQuery()

                        End If

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pdrid) FROM M6_pdr WHERE pdrnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_pdr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Pdr_HistorySimpan("" & paramSplit(0) & "★M6_Pdr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pdrsumber")) & "▼" & FixQuotes(drutama("pdrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Pdr set pdrcabang  = '" & FixQuotes(drutama("pdrcabang")) & "', pdrlokasi  = '" & FixQuotes(drutama("pdrlokasi")) & "', pdrgudangasal  = '" & FixQuotes(drutama("pdrgudangasal")) & "', pdrgudangproduksi  = '" & FixQuotes(drutama("pdrgudangproduksi")) & "', pdrgudangtujuan  = '" & FixQuotes(drutama("pdrgudangtujuan")) & "', pdrsumber  = '" & FixQuotes(drutama("pdrsumber")) & "', pdrjenis  = '" & FixQuotes(drutama("pdrjenis")) & "', pdrautonotransaksi  = " & drutama("pdrautonotransaksi") & ", pdrnotransaksi  = '" & FixQuotes(notransaksi) & "', pdrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("pdrtgl"))) & "', pdrkodepa  = " & drutama("pdrkodepa") & ", pdrdimintaoleh  = " & drutama("pdrdimintaoleh") & ", pdrdimintaolehkontak  = '" & FixQuotes(drutama("pdrdimintaolehkontak")) & "', pdrmintake  = " & drutama("pdrmintake") & ", pdrtgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("pdrtgldipakai"))) & "', pdrestimasikerja  = '" & FixQuotes(drutama("pdrestimasikerja")) & "', pdrmatauang  = '" & FixQuotes(drutama("pdrmatauang")) & "', pdrkurs  = '" & FixDouble(drutama("pdrkurs")) & "', pdrtotalhargain  = '" & FixDouble(drutama("pdrtotalhargain")) & "', pdrtotalhargaout  = '" & FixDouble(drutama("pdrtotalhargaout")) & "', pdrtotalhppin  = '" & FixDouble(drutama("pdrtotalhppin")) & "', pdrtotalhppout  = '" & FixDouble(drutama("pdrtotalhppout")) & "', pdruraian  = '" & FixQuotes(drutama("pdruraian")) & "', pdrcatatan  = '" & FixQuotes(drutama("pdrcatatan")) & "', pdrnoref  = '" & FixQuotes(drutama("pdrnoref")) & "', pdrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pdrtglnoref"))) & "', pdridbom  = " & drutama("pdridbom") & ", pdrstatuswoin  = " & drutama("pdrstatuswoin") & ", pdrstatuswoout  = " & drutama("pdrstatuswoout") & ", pdrstatusmrsin  = " & drutama("pdrstatusmrsin") & ", pdrstatusmrsout  = " & drutama("pdrstatusmrsout") & ", pdrstatusmrnin  = " & drutama("pdrstatusmrnin") & ", pdrstatusmrnout  = " & drutama("pdrstatusmrnout") & ", pdrstatuspdin  = " & drutama("pdrstatuspdin") & ", pdrstatuspdout  = " & drutama("pdrstatuspdout") & ", pdrstatus  = " & drutama("pdrstatus") & ", pdrstatussebelumnya  = " & drutama("pdrstatussebelumnya") & ", pdrjmlrevisi  = pdrjmlrevisi+1, pdrcetakanke  = " & drutama("pdrcetakanke") & ", pdrmodifikasiuser  = " & drutama("pdrmodifikasiuser") & ", pdrmodifikasitgl  = NOW(), pdrcustomtext1  = '" & FixQuotes(drutama("pdrcustomtext1")) & "', pdrcustomtext2  = '" & FixQuotes(drutama("pdrcustomtext2")) & "', pdrcustomtext3  = '" & FixQuotes(drutama("pdrcustomtext3")) & "', pdrcustomtext4  = '" & FixQuotes(drutama("pdrcustomtext4")) & "', pdrcustomtext5  = '" & FixQuotes(drutama("pdrcustomtext5")) & "', pdrcustomint1  = " & drutama("pdrcustomint1") & ", pdrcustomint2  = " & drutama("pdrcustomint2") & ", pdrcustomint3  = " & drutama("pdrcustomint3") & ", pdrcustomdbl1  = '" & FixDouble(drutama("pdrcustomdbl1")) & "', pdrcustomdbl2  = '" & FixDouble(drutama("pdrcustomdbl2")) & "', pdrcustomdbl3  = '" & FixDouble(drutama("pdrcustomdbl3")) & "', pdrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate1"))) & "', pdrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate2"))) & "', pdrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate3"))) & "', pdraktivitas = '" & FixDouble(drutama("pdraktivitas")) & "' where pdrid = '" & drutama("pdrid") & "'"
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

                    If drutama("pdrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pdrcabang"), drutama("pdrlokasi"), drutama("pdrsumber"), drutama("pdrtgl"), drutama("pdrsumber"), 6)
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
                        notransaksi = drutama("pdrnotransaksi")

                        Dim vAkunPD As String = "", vAkunSI As String = "", sqlambil As String = ""
                        Dim dt As New DataTable
                        sqlambil = "SELECT IFNULL(c.cnomor,'') as akunPD, IFNULL(c2.cnomor,'') as akunSI FROM m1_location l LEFT JOIN m1_coa c ON l.lalamat2 = c.cnomor LEFT JOIN m1_coa c2 ON l.lkota = c2.cnomor WHERE l.lkode = '" & drutama("pdrlokasi") & "'"
                        dt = AsDataTableAmbilDariDBCon(sqlambil, myConn)
                        If dt.Rows.Count > 0 Then
                            vAkunPD = FxDB(dt.Rows(0)("akunPD"), "")
                            vAkunSI = FxDB(dt.Rows(0)("akunSI"), "")
                        Else
                            result(2) = "Could not find Transaction Code for '" & drutama("pdrlokasi") & "' location." : Trans.Rollback() : GoTo selesai
                        End If

                        sql = "INSERT INTO `m1_cost_center` (`cckode`, `ccnama`, `ccakun`, `cccatatan`) VALUES ('" & FixQuotes(notransaksi) & "', '" & FixQuotes(notransaksi) & "', '" & FixQuotes(vAkunPD) & "', '" & FixQuotes(vAkunSI) & "') ON DUPLICATE KEY UPDATE cckode = VALUES(cckode);"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pdrid) FROM m6_pdr WHERE pdrnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Pdr (pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, pdrstatuspdout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrisclose, pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, pdrcustomtext5, pdrcustomint1, pdrcustomint2, pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, pdrcustomdate1, pdrcustomdate2, pdrcustomdate3, pdraktivitas) values('" & FixQuotes(drutama("pdrcabang")) & "', '" & FixQuotes(drutama("pdrlokasi")) & "', '" & FixQuotes(drutama("pdrgudangasal")) & "', '" & FixQuotes(drutama("pdrgudangproduksi")) & "', '" & FixQuotes(drutama("pdrgudangtujuan")) & "', '" & FixQuotes(drutama("pdrsumber")) & "', '" & FixQuotes(drutama("pdrjenis")) & "', " & drutama("pdrautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrtgl"))) & "', " & drutama("pdrkodepa") & ", " & drutama("pdrdimintaoleh") & ", '" & FixQuotes(drutama("pdrdimintaolehkontak")) & "', " & drutama("pdrmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("pdrtgldipakai"))) & "', '" & FixQuotes(drutama("pdrestimasikerja")) & "', '" & FixQuotes(drutama("pdrmatauang")) & "', '" & FixDouble(drutama("pdrkurs")) & "', '" & FixDouble(drutama("pdrtotalhargain")) & "', '" & FixDouble(drutama("pdrtotalhargaout")) & "', '" & FixDouble(drutama("pdrtotalhppin")) & "', '" & FixDouble(drutama("pdrtotalhppout")) & "', '" & FixQuotes(drutama("pdruraian")) & "', '" & FixQuotes(drutama("pdrcatatan")) & "', '" & FixQuotes(drutama("pdrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrtglnoref"))) & "', " & drutama("pdridbom") & ", " & drutama("pdrstatuswoin") & ", " & drutama("pdrstatuswoout") & ", " & drutama("pdrstatusmrsin") & ", " & drutama("pdrstatusmrsout") & ", " & drutama("pdrstatusmrnin") & ", " & drutama("pdrstatusmrnout") & ", " & drutama("pdrstatuspdin") & ", " & drutama("pdrstatuspdout") & ", " & drutama("pdrstatus") & ", " & drutama("pdrstatussebelumnya") & ", " & drutama("pdrjmlrevisi") & ", " & drutama("pdrcetakanke") & ", " & drutama("pdrinputuser") & ", NOW(), " & drutama("pdrmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("pdrisclose") & ", '" & FixQuotes(drutama("pdrcustomtext1")) & "', '" & FixQuotes(drutama("pdrcustomtext2")) & "', '" & FixQuotes(drutama("pdrcustomtext3")) & "', '" & FixQuotes(drutama("pdrcustomtext4")) & "', '" & FixQuotes(drutama("pdrcustomtext5")) & "', " & drutama("pdrcustomint1") & ", " & drutama("pdrcustomint2") & ", " & drutama("pdrcustomint3") & ", '" & FixDouble(drutama("pdrcustomdbl1")) & "', '" & FixDouble(drutama("pdrcustomdbl2")) & "', '" & FixDouble(drutama("pdrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate3"))) & "', '" & FixDouble(drutama("pdraktivitas")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select pdrid from M6_pdr where pdrnotransaksi='" & notransaksi & "' AND pdrinputuser= '" & userid & "' order by pdrmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Pdr_In where idpdr = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Dim vCostCenter As String = notransaksi
                Dim vCostCenter As String


                'Proses detail1
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        vCostCenter = FixQuotes(dr1("costcenter"))
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idpdrin") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(vCostCenter) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomin") & ", '" & FixDouble(dr1("jmlwo")) & "', " & dr1("statuswo") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(vCostCenter) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Pdr_In(idpdrin, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M6_Pdr_Out where idpdr = '" & result(4) & "'"
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
						vCostCenter = FixQuotes(dr1("costcenter"))
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idpdrout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(vCostCenter) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomout") & ", '" & FixDouble(dr1("jmlwo")) & "', " & dr1("statuswo") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(vCostCenter) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Pdr_Out(idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PDR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M6_PdrUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "PDR", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber &
                                              "' UNION SELECT Pdrtgl, Pdrnotransaksi, Pdrstatus FROM M6_Pdr WHERE Pdrid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pdrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_pdr_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Pdr_HistorySimpan("" & paramSplit(0) & "★M6_Pdr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m6_pdr_terkait("pdrid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M6_Pdr SET Pdrstatus = " & nilaiStatus & ", Pdrmodifikasiuser='" & userid & "', Pdrmodifikasitgl = NOW(), Pdrposting = 0, Pdrpostingtgl = '1971-01-01 00:00:00', Pdrjmlrevisi = Pdrjmlrevisi + 1 WHERE Pdrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_PdrSearch(PostWsSearch(paramSplit(0), "M6_PdrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_PdrDelete(ByVal param As String) As String

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
            Dim sumber As String = "PDR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pdrid, Pdrnotransaksi FROM M6_Pdr WHERE Pdrid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pdrcabang, pdrlokasi, pdrsumber, pdrautonotransaksi, pdrnotransaksi, pdrtgl"
            sql &= " FROM M6_pdr"
            sql &= " WHERE pdrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pdrcabang")
                lokasi = dtNomorNext.Rows(0)("pdrlokasi")
                sumber = dtNomorNext.Rows(0)("pdrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pdrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pdrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pdrtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_Pdr_In WHERE idpdr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL2
            sql = "DELETE FROM M6_Pdr_Out WHERE idpdr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M6_Pdr WHERE pdrid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_PdrSearch(PostWsSearch(paramSplit(0), "M6_PdrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M6_PdrGetdataById(ByVal param As String) As String
        'M6_PdrGetdataById Utama --------------------------------------------------------
        'pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, 
        'pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, 
        'pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, 
        'pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, 
        'pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, 
        'pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, 
        'pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, 
        'pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, pdrcustomtext5, pdrcustomint1, pdrcustomint2, 
        'pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, pdrcustomdate1, pdrcustomdate2, pdrcustomdate3, 
        'pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, 
        'pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, 
        'pdrinputusernama, pdrmodifikasiusernama, pdraktivitas, pdraktivitaskode, pdraktivitasnama, pdrjeniswajibwo

        'M6_PdrGetdataById In --------------------------------------------------------
        'idpdrin, idpdr, idbarang, namabarang, tipebarang, 
        'jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, 
        'gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idbomin, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, 
        'statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, 
        'divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, 
        'jmlsisamrn, jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan

        'M6_PdrGetdataById Out --------------------------------------------------------
        'idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, 
        'subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, jmlsisamrn, 
        'jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan

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

        Dim NmMemcached As String = "aplikasi1-m6_pl~m6_pl_Detail-" & idtransaksi
        Dim Filter2 As String = ""

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("statusrealisasi", "pdri.statusrealisasi")

            Filter2 = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter2 = Filter2.Replace("statusrealisasi", "pdro.statusrealisasi")
        End If

        'Set filter utama
        If Len(Filter) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pdrid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pdrid = " & idtransaksi & " and " & Filter
        End If

        'Set filter detail 2
        If Len(Filter2) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter2 = "idpdr = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter2 = "idpdr = '" & idtransaksi & "' and " & Filter2
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m6_pdr_getdata")
        sql = "select pdr.pdrid AS pdrid, pdr.pdrcabang AS pdrcabang, pdr.pdrlokasi AS pdrlokasi, pdr.pdrgudangasal AS pdrgudangasal, pdr.pdrgudangproduksi AS pdrgudangproduksi, pdr.pdrgudangtujuan AS pdrgudangtujuan, pdr.pdrsumber AS pdrsumber, pdr.pdrjenis AS pdrjenis, pdr.pdrautonotransaksi AS pdrautonotransaksi, pdr.pdrnotransaksi AS pdrnotransaksi, pdr.pdrtgl AS pdrtgl, pdr.pdrkodepa AS pdrkodepa, pdr.pdrdimintaoleh AS pdrdimintaoleh, pdr.pdrdimintaolehkontak AS pdrdimintaolehkontak, pdr.pdrmintake AS pdrmintake, pdr.pdrtgldipakai AS pdrtgldipakai, pdr.pdrestimasikerja AS pdrestimasikerja, pdr.pdrmatauang AS pdrmatauang, pdr.pdrkurs AS pdrkurs, pdr.pdrtotalhargain AS pdrtotalhargain, pdr.pdrtotalhargaout AS pdrtotalhargaout, pdr.pdrtotalhppin AS pdrtotalhppin, pdr.pdrtotalhppout AS pdrtotalhppout, pdr.pdruraian AS pdruraian, pdr.pdrcatatan AS pdrcatatan, pdr.pdrnoref AS pdrnoref, pdr.pdrtglnoref AS pdrtglnoref, pdr.pdridbom AS pdridbom, pdr.pdrstatuswoin AS pdrstatuswoin, pdr.pdrstatuswoout AS pdrstatuswoout, pdr.pdrstatusmrsin AS pdrstatusmrsin, pdr.pdrstatusmrsout AS pdrstatusmrsout, pdr.pdrstatusmrnin AS pdrstatusmrnin, pdr.pdrstatusmrnout AS pdrstatusmrnout, pdr.pdrstatuspdin AS pdrstatuspdin, pdr.pdrstatuspdout AS pdrstatuspdout, pdr.pdrstatusrealisasiin AS pdrstatusrealisasiin, pdr.pdrstatusrealisasiout AS pdrstatusrealisasiout, pdr.pdrstatus AS pdrstatus, pdr.pdrstatussebelumnya AS pdrstatussebelumnya, pdr.pdrjmlrevisi AS pdrjmlrevisi, pdr.pdrcetakanke AS pdrcetakanke, pdr.pdrinputuser AS pdrinputuser, pdr.pdrinputtgl AS pdrinputtgl, pdr.pdrmodifikasiuser AS pdrmodifikasiuser, pdr.pdrmodifikasitgl AS pdrmodifikasitgl, pdr.pdrposting AS pdrposting, pdr.pdrpostingtgl AS pdrpostingtgl, pdr.pdrisclose AS pdrisclose, pdr.pdrcustomtext1 AS pdrcustomtext1, pdr.pdrcustomtext2 AS pdrcustomtext2, pdr.pdrcustomtext3 AS pdrcustomtext3, pdr.pdrcustomtext4 AS pdrcustomtext4, pdr.pdrcustomtext5 AS pdrcustomtext5, pdr.pdrcustomint1 AS pdrcustomint1, pdr.pdrcustomint2 AS pdrcustomint2, pdr.pdrcustomint3 AS pdrcustomint3, pdr.pdrcustomdbl1 AS pdrcustomdbl1, pdr.pdrcustomdbl2 AS pdrcustomdbl2, pdr.pdrcustomdbl3 AS pdrcustomdbl3, pdr.pdrcustomdate1 AS pdrcustomdate1, pdr.pdrcustomdate2 AS pdrcustomdate2, pdr.pdrcustomdate3 AS pdrcustomdate3, br.bnama AS pdrcabangnama, lc.lnama AS pdrlokasinama, wh1.wnama AS pdrgudangasalnama, wh2.wnama AS pdrgudangproduksinama, wh3.wnama AS pdrgudangtujuannama, pc.pcnama AS pdrjenisnama, c1.kkode AS pdrdimintaolehkode, c1.knama AS pdrdimintaolehnama, c2.kkode AS pdrmintakekode, c2.knama AS pdrmintakenama, we.wenama AS pdrestimasikerjanama, bom.bomnotransaksi AS pdrnotransaksibom, st1.nama AS pdrstatusnama, st2.nama AS pdrstatussebelumnyanama, u1.unama AS pdrinputusernama, u2.unama AS pdrmodifikasiusernama, pdr.pdraktivitas, pa.pakode as pdraktivitaskode, pa.panama as pdraktivitasnama, pc.pcwajibwo AS pdrjeniswajibwo, pdri.idpdrin AS idpdrin, pdri.idpdr AS idpdr, pdri.idbarang AS idbarang, pdri.namabarang AS namabarang, pdri.tipebarang AS tipebarang, pdri.jml AS jml, pdri.satuan AS satuan, pdri.nilaisatuan AS nilaisatuan, pdri.jmlbarang AS jmlbarang, pdri.satuanbarang AS satuanbarang, pdri.matauang AS matauang, pdri.kurs AS kurs, pdri.harga AS harga, pdri.hpppersen AS hpppersen, pdri.hpp AS hpp, i.brekpersediaan AS rekpersediaan, pdri.cabang AS cabang, pdri.lokasi AS lokasi, pdri.gudangasal AS gudangasal, pdri.gudangproduksi AS gudangproduksi, pdri.gudangtujuan AS gudangtujuan, pdri.costcenter AS costcenter, pdri.divisi AS divisi, pdri.subdivisi AS subdivisi, pdri.proyek AS proyek, pdri.catatan AS catatan, pdri.urutan AS urutan, pdri.idbomin AS idbomin, pdri.jmlwo AS jmlwo, pdri.statuswo AS statuswo, pdri.jmlmrs AS jmlmrs, pdri.statusmrs AS statusmrs, pdri.jmlmrn AS jmlmrn, pdri.statusmrn AS statusmrn, pdri.jmlpd AS jmlpd, pdri.statuspd AS statuspd, pdri.jmlrealisasi AS jmlrealisasi, pdri.statusrealisasi AS statusrealisasi, pdri.isclose AS isclose, pdri.customtext1 AS customtext1, pdri.customtext2 AS customtext2, pdri.customtext3 AS customtext3, pdri.customdbl1 AS customdbl1, pdri.customdbl2 AS customdbl2, pdri.customdbl3 AS customdbl3, pdri.customdate1 AS customdate1, pdri.customdate2 AS customdate2, pdri.customdate3 AS customdate3, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, cc.ccnama AS costcenternama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, p.pnama AS proyeknama, pdr.pdrnotransaksi AS notransaksi, bom2.bomnotransaksi AS bomnotransaksi, ((pdri.jmlbarang - pdri.jmlwo) / pdri.nilaisatuan) AS jmlsisawo, ((pdri.jmlbarang - pdri.jmlmrs) / pdri.nilaisatuan) AS jmlsisamrs, ((pdri.jmlbarang - pdri.jmlmrn) / pdri.nilaisatuan) AS jmlsisamrn,((pdri.jmlbarang - pdri.jmlpd) / pdri.nilaisatuan) AS jmlsisapd,((pdri.jmlbarang - pdri.jmlrealisasi) / pdri.nilaisatuan) AS jmlsisarealisasi, i.bjmllapangan,  i.bsatuanlapangan, i.bcustom12, i.bcustom11 from m6_pdr pdr join m6_pdr_in pdri on pdr.pdrid = pdri.idpdr left join m1_branch br on pdr.pdrcabang = br.bkode left join m1_location lc on pdr.pdrlokasi = lc.lkode left join m1_warehouse wh1 on pdr.pdrgudangasal = wh1.wkode left join m1_warehouse wh2 on pdr.pdrgudangproduksi = wh2.wkode left join m1_warehouse wh3 on pdr.pdrgudangtujuan = wh3.wkode left join m1_production_category pc on pdr.pdrjenis = pc.pckode left join m1_contact c1 on pdr.pdrdimintaoleh = c1.kid left join m1_contact c2 on pdr.pdrmintake = c2.kid left join m1_working_estimate we on pdr.pdrestimasikerja = we.wekode left join m6_bom bom on pdr.pdridbom = bom.bomid left join m0_status st1 on pdr.pdrstatus = st1.kode left join m0_status st2 on pdr.pdrstatussebelumnya = st2.kode left join m0_user u1 on pdr.pdrinputuser = u1.userid left join m0_user u2 on pdr.pdrmodifikasiuser = u2.userid left join m1_production_activity pa on pdr.pdraktivitas = pa.paid left join m1_item i on pdri.idbarang = i.bid left join m1_cost_center cc on pdri.costcenter = cc.cckode left join m1_division d on pdri.divisi = d.dkode left join m1_subdivision sd on pdri.subdivisi = sd.sdkode left join m1_project p on pdri.proyek = p.pkode left join m6_bom_in bomi on pdri.idbomin = bomi.idbomin left join m6_bom bom2 on bomi.idbom = bom2.bomid"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pdrid"), 0), sptField,
                     FxDB(drutama("pdrcabang"), ""), sptField,
                     FxDB(drutama("pdrlokasi"), ""), sptField,
                     FxDB(drutama("pdrgudangasal"), ""), sptField,
                     FxDB(drutama("pdrgudangproduksi"), ""), sptField,
                     FxDB(drutama("pdrgudangtujuan"), ""), sptField,
                     FxDB(drutama("pdrsumber"), ""), sptField,
                     FxDB(drutama("pdrjenis"), ""), sptField,
                     FxDB(drutama("pdrautonotransaksi"), 0), sptField,
                     FxDB(drutama("pdrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pdrkodepa"), 0), sptField,
                     FxDB(drutama("pdrdimintaoleh"), 0), sptField,
                     FxDB(drutama("pdrdimintaolehkontak"), ""), sptField,
                     FxDB(drutama("pdrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(drutama("pdrestimasikerja"), ""), sptField,
                     FxDB(drutama("pdrmatauang"), ""), sptField,
                     FxDB(drutama("pdrkurs"), 0), sptField,
                     FxDB(drutama("pdrtotalhargain"), 0), sptField,
                     FxDB(drutama("pdrtotalhargaout"), 0), sptField,
                     FxDB(drutama("pdrtotalhppin"), 0), sptField,
                     FxDB(drutama("pdrtotalhppout"), 0), sptField,
                     FxDB(drutama("pdruraian"), ""), sptField,
                     FxDB(drutama("pdrcatatan"), ""), sptField,
                     FxDB(drutama("pdrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pdridbom"), 0), sptField,
                     FxDB(drutama("pdrstatuswoin"), 0), sptField,
                     FxDB(drutama("pdrstatuswoout"), 0), sptField,
                     FxDB(drutama("pdrstatusmrsin"), 0), sptField,
                     FxDB(drutama("pdrstatusmrsout"), 0), sptField,
                     FxDB(drutama("pdrstatusmrnin"), 0), sptField,
                     FxDB(drutama("pdrstatusmrnout"), 0), sptField,
                     FxDB(drutama("pdrstatuspdin"), 0), sptField,
                     FxDB(drutama("pdrstatuspdout"), 0), sptField,
                     FxDB(drutama("pdrstatusrealisasiin"), 0), sptField,
                     FxDB(drutama("pdrstatusrealisasiout"), 0), sptField,
                     FxDB(drutama("pdrstatus"), 0), sptField,
                     FxDB(drutama("pdrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pdrjmlrevisi"), 0), sptField,
                     FxDB(drutama("pdrcetakanke"), 0), sptField,
                     FxDB(drutama("pdrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pdrisclose"), 0), sptField,
                     FxDB(drutama("pdrcustomtext1"), ""), sptField,
                     FxDB(drutama("pdrcustomtext2"), ""), sptField,
                     FxDB(drutama("pdrcustomtext3"), ""), sptField,
                     FxDB(drutama("pdrcustomtext4"), ""), sptField,
                     FxDB(drutama("pdrcustomtext5"), ""), sptField,
                     FxDB(drutama("pdrcustomint1"), 0), sptField,
                     FxDB(drutama("pdrcustomint2"), 0), sptField,
                     FxDB(drutama("pdrcustomint3"), 0), sptField,
                     FxDB(drutama("pdrcustomdbl1"), 0), sptField,
                     FxDB(drutama("pdrcustomdbl2"), 0), sptField,
                     FxDB(drutama("pdrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pdrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pdrcabangnama"), ""), sptField,
                     FxDB(drutama("pdrlokasinama"), ""), sptField,
                     FxDB(drutama("pdrgudangasalnama"), ""), sptField,
                     FxDB(drutama("pdrgudangproduksinama"), ""), sptField,
                     FxDB(drutama("pdrgudangtujuannama"), ""), sptField,
                     FxDB(drutama("pdrjenisnama"), ""), sptField,
                     FxDB(drutama("pdrdimintaolehkode"), ""), sptField,
                     FxDB(drutama("pdrdimintaolehnama"), ""), sptField,
                     FxDB(drutama("pdrmintakekode"), ""), sptField,
                     FxDB(drutama("pdrmintakenama"), ""), sptField,
                     FxDB(drutama("pdrestimasikerjanama"), ""), sptField,
                     FxDB(drutama("pdrnotransaksibom"), ""), sptField,
                     FxDB(drutama("pdrstatusnama"), ""), sptField,
                     FxDB(drutama("pdrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pdrinputusernama"), ""), sptField,
                     FxDB(drutama("pdrmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("pdraktivitas"), 0), sptField,
                     FxDB(drutama("pdraktivitaskode"), ""), sptField,
                     FxDB(drutama("pdraktivitasnama"), ""), sptField,
                     FxDB(drutama("pdrjeniswajibwo"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpdrin"), 0), sptField,
                     FxDB(dr("idpdr"), 0), sptField,
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
                     FxDB(dr("idbomin"), 0), sptField,
                     FxDB(dr("jmlwo"), 0), sptField,
                     FxDB(dr("statuswo"), 0), sptField,
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
                     FxDB(dr("jmlpd"), 0), sptField,
                     FxDB(dr("statuspd"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
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
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisawo"), 0), sptField,
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("bcustom12"), 0), sptField,
                     FxDB(dr("bcustom11"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA OUT
            Dim querygiro As New m0_query
            'sql = querygiro.PanggilQuery("m6_pdr_getdata_out")
            sql = "select `pdro`.`idpdrout` AS `idpdrout`,`pdro`.`idpdr` AS `idpdr`,`pdro`.`idbarang` AS `idbarang`,`pdro`.`namabarang` AS `namabarang`,`pdro`.`tipebarang` AS `tipebarang`,`pdro`.`jml` AS `jml`,`pdro`.`satuan` AS `satuan`,`pdro`.`nilaisatuan` AS `nilaisatuan`,`pdro`.`jmlbarang` AS `jmlbarang`,`pdro`.`satuanbarang` AS `satuanbarang`,`pdro`.`matauang` AS `matauang`,`pdro`.`kurs` AS `kurs`,`pdro`.`harga` AS `harga`,`pdro`.`hpp` AS `hpp`,`pdro`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`pdro`.`idhppfifomasuk` AS `idhppfifomasuk`,`i`.`brekpersediaan` AS `rekpersediaan`,`pdro`.`cabang` AS `cabang`,`pdro`.`lokasi` AS `lokasi`,`pdro`.`gudangasal` AS `gudangasal`,`pdro`.`gudangproduksi` AS `gudangproduksi`,`pdro`.`gudangtujuan` AS `gudangtujuan`,`pdro`.`costcenter` AS `costcenter`,`pdro`.`divisi` AS `divisi`,`pdro`.`subdivisi` AS `subdivisi`,`pdro`.`proyek` AS `proyek`,`pdro`.`catatan` AS `catatan`,`pdro`.`urutan` AS `urutan`,`pdro`.`idbomout` AS `idbomout`,`pdro`.`jmlwo` AS `jmlwo`,`pdro`.`statuswo` AS `statuswo`,`pdro`.`jmlmrs` AS `jmlmrs`,`pdro`.`statusmrs` AS `statusmrs`,`pdro`.`jmlmrn` AS `jmlmrn`,`pdro`.`statusmrn` AS `statusmrn`,`pdro`.`jmlpd` AS `jmlpd`,`pdro`.`statuspd` AS `statuspd`,`pdro`.`jmlrealisasi` AS `jmlrealisasi`,`pdro`.`statusrealisasi` AS `statusrealisasi`,`pdro`.`isclose` AS `isclose`,`pdro`.`customtext1` AS `customtext1`,`pdro`.`customtext2` AS `customtext2`,`pdro`.`customtext3` AS `customtext3`,`pdro`.`customdbl1` AS `customdbl1`,`pdro`.`customdbl2` AS `customdbl2`,`pdro`.`customdbl3` AS `customdbl3`,`pdro`.`customdate1` AS `customdate1`,`pdro`.`customdate2` AS `customdate2`,`pdro`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pdr`.`pdrnotransaksi` AS `notransaksi`,`bom2`.`bomnotransaksi` AS `bomnotransaksi`,((`pdro`.`jmlbarang` - `pdro`.`jmlwo`) / `pdro`.`nilaisatuan`) AS `jmlsisawo`,((`pdro`.`jmlbarang` - `pdro`.`jmlmrs`) / `pdro`.`nilaisatuan`) AS `jmlsisamrs`,((`pdro`.`jmlbarang` - `pdro`.`jmlmrn`) / `pdro`.`nilaisatuan`) AS `jmlsisamrn`,((`pdro`.`jmlbarang` - `pdro`.`jmlpd`) / `pdro`.`nilaisatuan`) AS `jmlsisapd`,((`pdro`.`jmlbarang` - `pdro`.`jmlrealisasi`) / `pdro`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bjmllapangan, i.bsatuanlapangan, i.bstok, IFNULL(SUM(ib.jmlbooking),0) AS jmlbooking, IFNULL((i.bstok-SUM(ib.jmlbooking)),0) AS stokakhir from (((((((((`m6_pdr_out` `pdro` left join `m6_pdr` `pdr` on((`pdro`.`idpdr` = `pdr`.`pdrid`))) left join `m1_item` `i` on((`pdro`.`idbarang` = `i`.`bid`))) left join `m1_cost_center` `cc` on((`pdro`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`pdro`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`pdro`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`pdro`.`proyek` = `p`.`pkode`))) left join `m6_bom_out` `bomo` on((`pdro`.`idbomout` = `bomo`.`idbomout`))) left join `m6_bom` `bom2` on((`bomo`.`idbom` = `bom2`.`bomid`))) left join `m1_item_booking` `ib` on((`pdro`.`idbarang` = `ib`.`idbarang`)))"

            Dim dtout As New DataTable
            dtout = AmbilData("aplikasi1-M6_Pdr_Pack", Filter2, "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "pdro.idbarang", sql) ' Ambil data ke databases

            For Each dr As DataRow In dtout.Rows
                detailout = String.Concat(detailout,
                     FxDB(dr("idpdrout"), 0), sptField,
                     FxDB(dr("idpdr"), 0), sptField,
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
                     FxDB(dr("idbomout"), 0), sptField,
                     FxDB(dr("jmlwo"), 0), sptField,
                     FxDB(dr("statuswo"), 0), sptField,
                     FxDB(dr("jmlmrs"), 0), sptField,
                     FxDB(dr("statusmrs"), 0), sptField,
                     FxDB(dr("jmlmrn"), 0), sptField,
                     FxDB(dr("statusmrn"), 0), sptField,
                     FxDB(dr("jmlpd"), 0), sptField,
                     FxDB(dr("statuspd"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
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
                     FxDB(dr("bomnotransaksi"), ""), sptField,
                     FxDB(dr("jmlsisawo"), 0), sptField,
                     FxDB(dr("jmlsisamrs"), 0), sptField,
                     FxDB(dr("jmlsisamrn"), 0), sptField,
                     FxDB(dr("jmlsisapd"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("stokakhir"), 0), sptRow)
            Next
            If detailout.Length > 0 Then detailout = detailout.Substring(0, detailout.Length - sptRow.Length)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, pdrcustomtext5, pdrcustomint1, pdrcustomint2, pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, pdrcustomdate1, pdrcustomdate2, pdrcustomdate3, pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, pdrinputusernama, pdrmodifikasiusernama, pdraktivitas, pdraktivitaskode, pdraktivitasnama, pdrjeniswajibwo" & sptSubParam & "idpdrin, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, bcustom12, bcustom11" & sptSubParam & "idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, costcenternama, divisinama, subdivisinama, proyeknama, notransaksi, bomnotransaksi, jmlsisawo, jmlsisamrs, jmlsisamrn, jmlsisapd, jmlsisarealisasi, bjmllapangan, bsatuanlapangan, stokakhir"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdrSearch(ByVal param As String) As String
        'M6_PdrSearch --------------------------------------------------------
        'pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, 
        'pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, 
        'pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, 
        'pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, 
        'pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, 
        'pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, 
        'pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, 
        'pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, 
        'pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, 
        'pdrinputusernama, pdrmodifikasiusernama, pdraktivitas, pdraktivitaskode, pdraktivitasnama, salesid, saleskode, salesnama

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
        'sql = query.PanggilQuery("m6_pdr_v")
        'sql = "select pdr.pdrid AS pdrid, pdr.pdrcabang AS pdrcabang, pdr.pdrlokasi AS pdrlokasi, pdr.pdrgudangasal AS pdrgudangasal, pdr.pdrgudangproduksi AS pdrgudangproduksi, pdr.pdrgudangtujuan AS pdrgudangtujuan, pdr.pdrsumber AS pdrsumber, pdr.pdrjenis AS pdrjenis, pdr.pdrautonotransaksi AS pdrautonotransaksi, pdr.pdrnotransaksi AS pdrnotransaksi, pdr.pdrtgl AS pdrtgl, pdr.pdrkodepa AS pdrkodepa, pdr.pdrdimintaoleh AS pdrdimintaoleh, pdr.pdrdimintaolehkontak AS pdrdimintaolehkontak, pdr.pdrmintake AS pdrmintake, pdr.pdrtgldipakai AS pdrtgldipakai, pdr.pdrestimasikerja AS pdrestimasikerja, pdr.pdrmatauang AS pdrmatauang, pdr.pdrkurs AS pdrkurs, pdr.pdrtotalhargain AS pdrtotalhargain, pdr.pdrtotalhargaout AS pdrtotalhargaout, pdr.pdrtotalhppin AS pdrtotalhppin, pdr.pdrtotalhppout AS pdrtotalhppout, pdr.pdruraian AS pdruraian, pdr.pdrcatatan AS pdrcatatan, pdr.pdrnoref AS pdrnoref, pdr.pdrtglnoref AS pdrtglnoref, pdr.pdridbom AS pdridbom, pdr.pdrstatuswoin AS pdrstatuswoin, pdr.pdrstatuswoout AS pdrstatuswoout, pdr.pdrstatusmrsin AS pdrstatusmrsin, pdr.pdrstatusmrsout AS pdrstatusmrsout, pdr.pdrstatusmrnin AS pdrstatusmrnin, pdr.pdrstatusmrnout AS pdrstatusmrnout, pdr.pdrstatuspdin AS pdrstatuspdin, pdr.pdrstatuspdout AS pdrstatuspdout, pdr.pdrstatusrealisasiin AS pdrstatusrealisasiin, pdr.pdrstatusrealisasiout AS pdrstatusrealisasiout, pdr.pdrstatus AS pdrstatus, pdr.pdrstatussebelumnya AS pdrstatussebelumnya, pdr.pdrjmlrevisi AS pdrjmlrevisi, pdr.pdrcetakanke AS pdrcetakanke, pdr.pdrinputuser AS pdrinputuser, pdr.pdrinputtgl AS pdrinputtgl, pdr.pdrmodifikasiuser AS pdrmodifikasiuser, pdr.pdrmodifikasitgl AS pdrmodifikasitgl, pdr.pdrposting AS pdrposting, pdr.pdrpostingtgl AS pdrpostingtgl, pdr.pdrisclose AS pdrisclose, br.bnama AS pdrcabangnama, lc.lnama AS pdrlokasinama, wh1.wnama AS pdrgudangasalnama, wh2.wnama AS pdrgudangproduksinama, wh3.wnama AS pdrgudangtujuannama, pc.pcnama AS pdrjenisnama, c1.kkode AS pdrdimintaolehkode, c1.knama AS pdrdimintaolehnama, c2.kkode AS pdrmintakekode, c2.knama AS pdrmintakenama, we.wenama AS pdrestimasikerjanama, bom.bomnotransaksi AS pdrnotransaksibom, st1.nama AS pdrstatusnama, st2.nama AS pdrstatussebelumnyanama, u1.unama AS pdrinputusernama, u2.unama AS pdrmodifikasiusernama, pdr.pdraktivitas, pa.pakode as pdraktivitaskode, pa.panama as pdraktivitasnama from m6_pdr pdr left join m1_branch br on pdr.pdrcabang = br.bkode left join m1_location lc on pdr.pdrlokasi = lc.lkode left join m1_warehouse wh1 on pdr.pdrgudangasal = wh1.wkode left join m1_warehouse wh2 on pdr.pdrgudangproduksi = wh2.wkode left join m1_warehouse wh3 on pdr.pdrgudangtujuan = wh3.wkode left join m1_production_category pc on pdr.pdrjenis = pc.pckode left join m1_contact c1 on pdr.pdrdimintaoleh = c1.kid left join m1_contact c2 on pdr.pdrmintake = c2.kid left join m1_working_estimate we on pdr.pdrestimasikerja = we.wekode left join m6_bom bom on pdr.pdridbom = bom.bomid left join m0_status st1 on pdr.pdrstatus = st1.kode left join m0_status st2 on pdr.pdrstatussebelumnya = st2.kode left join m0_user u1 on pdr.pdrinputuser = u1.userid left join m0_user u2 on pdr.pdrmodifikasiuser = u2.userid left join m1_production_activity pa on pdr.pdraktivitas = pa.paid"
        sql = "select pdr.pdrid AS pdrid, pdr.pdrcabang AS pdrcabang, pdr.pdrlokasi AS pdrlokasi, pdr.pdrgudangasal AS pdrgudangasal, pdr.pdrgudangproduksi AS pdrgudangproduksi, pdr.pdrgudangtujuan AS pdrgudangtujuan, pdr.pdrsumber AS pdrsumber, pdr.pdrjenis AS pdrjenis, pdr.pdrautonotransaksi AS pdrautonotransaksi, pdr.pdrnotransaksi AS pdrnotransaksi, pdr.pdrtgl AS pdrtgl, pdr.pdrkodepa AS pdrkodepa, pdr.pdrdimintaoleh AS pdrdimintaoleh, pdr.pdrdimintaolehkontak AS pdrdimintaolehkontak, pdr.pdrmintake AS pdrmintake, pdr.pdrtgldipakai AS pdrtgldipakai, pdr.pdrestimasikerja AS pdrestimasikerja, pdr.pdrmatauang AS pdrmatauang, pdr.pdrkurs AS pdrkurs, pdr.pdrtotalhargain AS pdrtotalhargain, pdr.pdrtotalhargaout AS pdrtotalhargaout, pdr.pdrtotalhppin AS pdrtotalhppin, pdr.pdrtotalhppout AS pdrtotalhppout, pdr.pdruraian AS pdruraian, pdr.pdrcatatan AS pdrcatatan, pdr.pdrnoref AS pdrnoref, pdr.pdrtglnoref AS pdrtglnoref, pdr.pdridbom AS pdridbom, pdr.pdrstatuswoin AS pdrstatuswoin, pdr.pdrstatuswoout AS pdrstatuswoout, pdr.pdrstatusmrsin AS pdrstatusmrsin, pdr.pdrstatusmrsout AS pdrstatusmrsout, pdr.pdrstatusmrnin AS pdrstatusmrnin, pdr.pdrstatusmrnout AS pdrstatusmrnout, pdr.pdrstatuspdin AS pdrstatuspdin, pdr.pdrstatuspdout AS pdrstatuspdout, pdr.pdrstatusrealisasiin AS pdrstatusrealisasiin, pdr.pdrstatusrealisasiout AS pdrstatusrealisasiout, pdr.pdrstatus AS pdrstatus, pdr.pdrstatussebelumnya AS pdrstatussebelumnya, pdr.pdrjmlrevisi AS pdrjmlrevisi, pdr.pdrcetakanke AS pdrcetakanke, pdr.pdrinputuser AS pdrinputuser, pdr.pdrinputtgl AS pdrinputtgl, pdr.pdrmodifikasiuser AS pdrmodifikasiuser, pdr.pdrmodifikasitgl AS pdrmodifikasitgl, pdr.pdrposting AS pdrposting, pdr.pdrpostingtgl AS pdrpostingtgl, pdr.pdrisclose AS pdrisclose, br.bnama AS pdrcabangnama, lc.lnama AS pdrlokasinama, wh1.wnama AS pdrgudangasalnama, wh2.wnama AS pdrgudangproduksinama, wh3.wnama AS pdrgudangtujuannama, pc.pcnama AS pdrjenisnama, c1.kkode AS pdrdimintaolehkode, c1.knama AS pdrdimintaolehnama, c2.kkode AS pdrmintakekode, c2.knama AS pdrmintakenama, we.wenama AS pdrestimasikerjanama, bom.bomnotransaksi AS pdrnotransaksibom, st1.nama AS pdrstatusnama, st2.nama AS pdrstatussebelumnyanama, u1.unama AS pdrinputusernama, u2.unama AS pdrmodifikasiusernama, pdr.pdraktivitas, pa.pakode as pdraktivitaskode, pa.panama as pdraktivitasnama , cs.kid as salesid, cs.kkode as saleskode, cs.knama as salesnama from m6_pdr pdr left join m1_branch br on pdr.pdrcabang = br.bkode left join m1_location lc on pdr.pdrlokasi = lc.lkode left join m1_warehouse wh1 on pdr.pdrgudangasal = wh1.wkode  left join m1_warehouse wh2 on pdr.pdrgudangproduksi = wh2.wkode  left join m1_warehouse wh3 on pdr.pdrgudangtujuan = wh3.wkode  left join m1_production_category pc on pdr.pdrjenis = pc.pckode  left join m1_contact c1 on pdr.pdrdimintaoleh = c1.kid  left join m1_contact c2 on pdr.pdrmintake = c2.kid  left join m1_working_estimate we on pdr.pdrestimasikerja = we.wekode  left join m6_bom bom on pdr.pdridbom = bom.bomid  left join m0_status st1 on pdr.pdrstatus = st1.kode  left join m0_status st2 on pdr.pdrstatussebelumnya = st2.kode  left join m0_user u1 on pdr.pdrinputuser = u1.userid  left join m0_user u2 on pdr.pdrmodifikasiuser = u2.userid  left join m1_production_activity pa on pdr.pdraktivitas = pa.paid left join m6_pdr_in pdri on pdr.pdrid = pdri.idpdr left join m5_so_detail sod on pdri.customtext3 = sod.customtext3 and sod.customtext3 <> '' left join m5_so so on sod.idso = so.soid left join m1_contact cs on so.sobagianpenjualan = cs.kid "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "pdr.pdrid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pdrid"), 0), sptField,
                     FxDB(dr("pdrcabang"), ""), sptField,
                     FxDB(dr("pdrlokasi"), ""), sptField,
                     FxDB(dr("pdrgudangasal"), ""), sptField,
                     FxDB(dr("pdrgudangproduksi"), ""), sptField,
                     FxDB(dr("pdrgudangtujuan"), ""), sptField,
                     FxDB(dr("pdrsumber"), ""), sptField,
                     FxDB(dr("pdrjenis"), ""), sptField,
                     FxDB(dr("pdrautonotransaksi"), 0), sptField,
                     FxDB(dr("pdrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("pdrkodepa"), 0), sptField,
                     FxDB(dr("pdrdimintaoleh"), 0), sptField,
                     FxDB(dr("pdrdimintaolehkontak"), ""), sptField,
                     FxDB(dr("pdrmintake"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrtgldipakai"), ""), formatTgl), sptField,
                     FxDB(dr("pdrestimasikerja"), ""), sptField,
                     FxDB(dr("pdrmatauang"), ""), sptField,
                     FxDB(dr("pdrkurs"), 0), sptField,
                     FxDB(dr("pdrtotalhargain"), 0), sptField,
                     FxDB(dr("pdrtotalhargaout"), 0), sptField,
                     FxDB(dr("pdrtotalhppin"), 0), sptField,
                     FxDB(dr("pdrtotalhppout"), 0), sptField,
                     FxDB(dr("pdruraian"), ""), sptField,
                     FxDB(dr("pdrcatatan"), ""), sptField,
                     FxDB(dr("pdrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pdrtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pdridbom"), 0), sptField,
                     FxDB(dr("pdrstatuswoin"), 0), sptField,
                     FxDB(dr("pdrstatuswoout"), 0), sptField,
                     FxDB(dr("pdrstatusmrsin"), 0), sptField,
                     FxDB(dr("pdrstatusmrsout"), 0), sptField,
                     FxDB(dr("pdrstatusmrnin"), 0), sptField,
                     FxDB(dr("pdrstatusmrnout"), 0), sptField,
                     FxDB(dr("pdrstatuspdin"), 0), sptField,
                     FxDB(dr("pdrstatuspdout"), 0), sptField,
                     FxDB(dr("pdrstatusrealisasiin"), 0), sptField,
                     FxDB(dr("pdrstatusrealisasiout"), 0), sptField,
                     FxDB(dr("pdrstatus"), 0), sptField,
                     FxDB(dr("pdrstatussebelumnya"), 0), sptField,
                     FxDB(dr("pdrjmlrevisi"), 0), sptField,
                     FxDB(dr("pdrcetakanke"), 0), sptField,
                     FxDB(dr("pdrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pdrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pdrisclose"), 0), sptField,
                     FxDB(dr("pdrcabangnama"), ""), sptField,
                     FxDB(dr("pdrlokasinama"), ""), sptField,
                     FxDB(dr("pdrgudangasalnama"), ""), sptField,
                     FxDB(dr("pdrgudangproduksinama"), ""), sptField,
                     FxDB(dr("pdrgudangtujuannama"), ""), sptField,
                     FxDB(dr("pdrjenisnama"), ""), sptField,
                     FxDB(dr("pdrdimintaolehkode"), ""), sptField,
                     FxDB(dr("pdrdimintaolehnama"), ""), sptField,
                     FxDB(dr("pdrmintakekode"), ""), sptField,
                     FxDB(dr("pdrmintakenama"), ""), sptField,
                     FxDB(dr("pdrestimasikerjanama"), ""), sptField,
                     FxDB(dr("pdrnotransaksibom"), ""), sptField,
                     FxDB(dr("pdrstatusnama"), ""), sptField,
                     FxDB(dr("pdrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pdrinputusernama"), ""), sptField,
                     FxDB(dr("pdrmodifikasiusernama"), ""), sptField,
                     FxDB(dr("pdraktivitas"), 0), sptField,
                     FxDB(dr("pdraktivitaskode"), ""), sptField,
                     FxDB(dr("pdraktivitasnama"), ""), sptField,
                     FxDB(dr("salesid"), 0), sptField,
                     FxDB(dr("saleskode"), ""), sptField,
                     FxDB(dr("salesnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, pdrstatuspdout, pdrstatusrealisasiin, pdrstatusrealisasiout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrposting, pdrpostingtgl, pdrisclose, pdrcabangnama, pdrlokasinama, pdrgudangasalnama, pdrgudangproduksinama, pdrgudangtujuannama, pdrjenisnama, pdrdimintaolehkode, pdrdimintaolehnama, pdrmintakekode, pdrmintakenama, pdrestimasikerjanama, pdrnotransaksibom, pdrstatusnama, pdrstatussebelumnyanama, pdrinputusernama, pdrmodifikasiusernama, pdraktivitas, pdraktivitaskode, pdraktivitasnama, salesid, saleskode, salesnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_PdrTerkait(ByVal param As String) As String
        'M6_PdrTerkait --------------------------------------------------------
        'pdrid, pdrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "pdrid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND pdrid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "pdrid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m6_pdr_terkait(Filter)
        sql = m6_pdr_terkait(Filter)


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m5_bom_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each pl As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(pl("pdrid"), 0), sptField,
                     FxDB(pl("pdrnotransaksi"), ""), sptField,
                     FxDB(pl("sumber"), ""), sptField,
                     FxDB(pl("idterkait"), 0), sptField,
                     FxDB(pl("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(pl("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(pl("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(pl("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(pl("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related PDR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pdrid, pdrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetailIn As DataTable, ByVal ftExistOutstandingBomIn As String, ByVal ftOutstandingBomIn As String, ByVal dtdetailOut As DataTable, ByVal ftExistOutstandingBomOut As String, ByVal ftOutstandingBomOut As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING BOM IN --------------------------------
        If Len(ftExistOutstandingBomIn) > 0 Then 'ftExistOutstanding = rowExists, idbomin, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingBomIn)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idbomin=" & dtval.Rows(0)("idbomin")
                dtLookup = AsDataTableFilterLimit(dtdetailIn, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail 1 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in BOM(result)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT bomin.idbomin, (bomin.jmlbarang) as sisapdr, i.bid, i.bkode FROM m6_bom_in AS bomin INNER JOIN m1_item AS i ON bomin.idbarang = i.bid WHERE " & ftOutstandingBomIn
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisapdr")

                filterLookup = "idbomin=" & dtval.Rows(0)("idbomin")
                dtLookup = AsDataTableFilterLimit(dtdetailIn, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail 1 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in BOM(result), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING BOM IN -------------------------


        'VALIDASI OUTSTANDING BOM OUT -------------------------------
        If Len(ftExistOutstandingBomOut) > 0 Then 'ftExistOutstanding = rowExists, idbomout, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingBomOut)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idbomout=" & dtval.Rows(0)("idbomout")
                dtLookup = AsDataTableFilterLimit(dtdetailOut, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Detail 2 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in BOM(material)" : GoTo selesai
            End If

            'CEK JML SISA OUTSTANDING
            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT bomout.idbomout, (bomout.jmlbarang) as sisapdr, i.bid, i.bkode FROM m6_bom_out AS bomout INNER JOIN m1_item AS i ON bomout.idbarang = i.bid WHERE " & ftOutstandingBomOut
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisapdr")

                filterLookup = "idbomout=" & dtval.Rows(0)("idbomout")
                dtLookup = AsDataTableFilterLimit(dtdetailOut, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Detail 2 Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in BOM(material), item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING BOM OUT ------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M6_PdrSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataDetail2(), dataRowDetail2() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'pdrid(0) As Integer, pdrcabang(1) As String, pdrlokasi(2) As String, pdrgudangasal(3) As String, pdrgudangproduksi(4) As String, 
        'pdrgudangtujuan(5) As String, pdrsumber(6) As String, pdrjenis(7) As String, pdrautonotransaksi(8) As Integer, pdrnotransaksi(9) As String, 
        'pdrtgl(10) As Date, pdrkodepa(11) As Integer, pdrdimintaoleh(12) As Integer, pdrdimintaolehkontak(13) As String, pdrmintake(14) As Integer, 
        'pdrtgldipakai(15) As Date, pdrestimasikerja(16) As String, pdrmatauang(17) As String, pdrkurs(18) As Double, pdrtotalhargain(19) As Double, 
        'pdrtotalhargaout(20) As Double, pdrtotalhppin(21) As Double, pdrtotalhppout(22) As Double, pdruraian(23) As String, pdrcatatan(24) As String, 
        'pdrnoref(25) As String, pdrtglnoref(26) As Date, pdridbom(27) As Integer, pdrstatuswoin(28) As Integer, pdrstatuswoout(29) As Integer, 
        'pdrstatusmrsin(30) As Integer, pdrstatusmrsout(31) As Integer, pdrstatusmrnin(32) As Integer, pdrstatusmrnout(33) As Integer, pdrstatuspdin(34) As Integer, 
        'pdrstatuspdout(35) As Integer, pdrstatus(36) As Integer, pdrstatussebelumnya(37) As Integer, pdrjmlrevisi(38) As Integer, pdrcetakanke(39) As Integer, 
        'pdrinputuser(40) As Integer, pdrinputtgl(41) As DateTime, pdrmodifikasiuser(42) As Integer, pdrmodifikasitgl(43) As DateTime, pdrisclose(44) As Integer, 
        'pdrcustomtext1(45) As String, pdrcustomtext2(46) As String, pdrcustomtext3(47) As String, pdrcustomtext4(48) As String, pdrcustomtext5(49) As String, 
        'pdrcustomint1(50) As Integer, pdrcustomint2(51) As Integer, pdrcustomint3(52) As Integer, pdrcustomdbl1(53) As Double, pdrcustomdbl2(54) As Double, 
        'pdrcustomdbl3(55) As Double, pdrcustomdate1(56) As Date, pdrcustomdate2(57) As Date, pdrcustomdate3(58) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pdrid, pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, 
        'pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, 
        'pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, 
        'pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, 
        'pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, 
        'pdrstatuspdout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, 
        'pdrmodifikasiuser, pdrmodifikasitgl, pdrisclose, pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, 
        'pdrcustomtext5, pdrcustomint1, pdrcustomint2, pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, 
        'pdrcustomdate1, pdrcustomdate2, pdrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 59) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pdrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pdrid required numeric." : GoTo selesai
        End If
        'pdrautonotransaksi(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pdrautonotransaksi required numeric." : GoTo selesai
        End If
        'pdrtgl(10) As Date
        If (IsDate(dataUtama(10)) = False) Then
            result(2) = "pdrtgl required date." : GoTo selesai
        End If
        'pdrkodepa(11) As Integer
        If (IsNumeric(dataUtama(11)) = False) Then
            result(2) = "pdrkodepa required numeric." : GoTo selesai
        End If
        'pdrdimintaoleh(12) As Integer
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "pdrdimintaoleh required numeric." : GoTo selesai
        End If
        If (dataUtama(12) < 1) Then
            result(2) = "pdrdimintaoleh can't be empty." : GoTo selesai
        End If
        'pdrmintake(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "pdrmintake required numeric." : GoTo selesai
        End If
        'pdrtgldipakai(15) As Date
        If (IsDate(dataUtama(15)) = False) Then
            result(2) = "pdrtgldipakai required date." : GoTo selesai
        End If
        'pdrkurs(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "pdrkurs required numeric." : GoTo selesai
        End If
        'pdrtotalhargain(19) As Double
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "pdrtotalhargain required numeric." : GoTo selesai
        End If
        'pdrtotalhargaout(20) As Double
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "pdrtotalhargaout required numeric." : GoTo selesai
        End If
        'pdrtotalhppin(21) As Double
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "pdrtotalhppin required numeric." : GoTo selesai
        End If
        'pdrtotalhppout(22) As Double
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pdrtotalhppout required numeric." : GoTo selesai
        End If
        'pdrtglnoref(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "pdrtglnoref required date." : GoTo selesai
        End If
        'pdridbom(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pdridbom required numeric." : GoTo selesai
        End If
        'pdrstatuswoin(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "pdrstatuswoin required numeric." : GoTo selesai
        End If
        'pdrstatuswoout(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "pdrstatuswoout required numeric." : GoTo selesai
        End If
        'pdrstatusmrsin(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "pdrstatusmrsin required numeric." : GoTo selesai
        End If
        'pdrstatusmrsout(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "pdrstatusmrsout required numeric." : GoTo selesai
        End If
        'pdrstatusmrnin(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pdrstatusmrnin required numeric." : GoTo selesai
        End If
        'pdrstatusmrnout(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pdrstatusmrnout required numeric." : GoTo selesai
        End If
        'pdrstatuspdin(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pdrstatuspdin required numeric." : GoTo selesai
        End If
        'pdrstatuspdout(35) As Integer
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "pdrstatuspdout required numeric." : GoTo selesai
        End If
        'pdrstatus(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pdrstatus required numeric." : GoTo selesai
        End If
        'pdrstatussebelumnya(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pdrstatussebelumnya required numeric." : GoTo selesai
        End If
        'pdrjmlrevisi(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pdrjmlrevisi required numeric." : GoTo selesai
        End If
        'pdrcetakanke(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "pdrcetakanke required numeric." : GoTo selesai
        End If
        'pdrinputuser(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pdrinputuser required numeric." : GoTo selesai
        End If
        'pdrinputtgl(41) As DateTime
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "pdrinputtgl required date." : GoTo selesai
        End If
        'pdrmodifikasiuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pdrmodifikasiuser required numeric." : GoTo selesai
        End If
        'pdrmodifikasitgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "pdrmodifikasitgl required date." : GoTo selesai
        End If
        'pdrisclose(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "pdrisclose required numeric." : GoTo selesai
        End If
        'pdrcustomint1(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "pdrcustomint1 required numeric." : GoTo selesai
        End If
        'pdrcustomint2(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pdrcustomint2 required numeric." : GoTo selesai
        End If
        'pdrcustomint3(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pdrcustomint3 required numeric." : GoTo selesai
        End If
        'pdrcustomdbl1(53) As Double
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pdrcustomdbl1 required numeric." : GoTo selesai
        End If
        'pdrcustomdbl2(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pdrcustomdbl2 required numeric." : GoTo selesai
        End If
        'pdrcustomdbl3(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pdrcustomdbl3 required numeric." : GoTo selesai
        End If
        'pdrcustomdate1(56) As Date
        If (IsDate(dataUtama(56)) = False) Then
            result(2) = "pdrcustomdate1 required date." : GoTo selesai
        End If
        'pdrcustomdate2(57) As Date
        If (IsDate(dataUtama(57)) = False) Then
            result(2) = "pdrcustomdate2 required date." : GoTo selesai
        End If
        'pdrcustomdate3(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "pdrcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pdrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pdrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pdrcabang should not be more than 25 character." : GoTo selesai
        End If

        'pdrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pdrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pdrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'pdrgudangasal(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pdrgudangasal can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pdrgudangasal should not be more than 25 character." : GoTo selesai
        End If

        'pdrgudangproduksi(4) As String
        'If Len(dataUtama(4)) = 0 Then
        '    result(2) = "pdrgudangproduksi can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(4)) > 25 Then
            result(2) = "pdrgudangproduksi should not be more than 25 character." : GoTo selesai
        End If

        'pdrgudangtujuan(5) As String
        'If Len(dataUtama(5)) = 0 Then
        '    result(2) = "pdrgudangtujuan can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(5)) > 25 Then
            result(2) = "pdrgudangtujuan should not be more than 25 character." : GoTo selesai
        End If

        'pdrsumber(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "pdrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 10 Then
            result(2) = "pdrsumber should not be more than 10 character." : GoTo selesai
        End If

        'pdrjenis(7) As String
        If Len(dataUtama(7)) = 0 Then
            result(2) = "pdrjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 25 Then
            result(2) = "pdrjenis should not be more than 25 character." : GoTo selesai
        End If

        'pdrnotransaksi(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pdrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 50 Then
            result(2) = "pdrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pdrtgl(10) As Date
        If Len(dataUtama(10)) = 0 Then
            result(2) = "pdrtgl can't be empty" : GoTo selesai
        End If

        'pdrtgldipakai(15) As Date
        If Len(dataUtama(15)) = 0 Then
            result(2) = "pdrtgldipakai can't be empty" : GoTo selesai
        End If

        'pdrmatauang(17) As String
        If Len(dataUtama(17)) = 0 Then
            result(2) = "pdrmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(17)) > 25 Then
            result(2) = "pdrmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pdrkurs(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "pdrkurs can't be empty" : GoTo selesai
        End If

        'pdrtotalhargain(19) As Double
        If Len(dataUtama(19)) = 0 Then
            result(2) = "pdrtotalhargain can't be empty" : GoTo selesai
        End If

        'pdrtotalhargaout(20) As Double
        If Len(dataUtama(20)) = 0 Then
            result(2) = "pdrtotalhargaout can't be empty" : GoTo selesai
        End If

        'pdrtotalhppin(21) As Double
        If Len(dataUtama(21)) = 0 Then
            result(2) = "pdrtotalhppin can't be empty" : GoTo selesai
        End If

        'pdrtotalhppout(22) As Double
        If Len(dataUtama(22)) = 0 Then
            result(2) = "pdrtotalhppout can't be empty" : GoTo selesai
        End If

        'pdrtglnoref(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "pdrtglnoref can't be empty" : GoTo selesai
        End If

        'pdrinputtgl(41) As DateTime
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pdrinputtgl can't be empty" : GoTo selesai
        End If

        'pdrmodifikasitgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "pdrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pdrcustomdbl1(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "pdrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pdrcustomdbl2(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "pdrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pdrcustomdbl3(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "pdrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pdrcustomdate1(56) As Date
        If Len(dataUtama(56)) = 0 Then
            result(2) = "pdrcustomdate1 can't be empty" : GoTo selesai
        End If

        'pdrcustomdate2(57) As Date
        If Len(dataUtama(57)) = 0 Then
            result(2) = "pdrcustomdate2 can't be empty" : GoTo selesai
        End If

        'pdrcustomdate3(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "pdrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pdrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrgudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrgudangproduksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrgudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrdimintaoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrdimintaolehkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrmintake", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrtgldipakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrestimasikerja", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhargain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhargaout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhppin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtotalhppout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdridbom", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuswoin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuswoout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrsin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrsout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrnin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatusmrnout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuspdin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatuspdout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pdrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "pdrid~pdrcabang~pdrlokasi~pdrgudangasal~pdrgudangproduksi~pdrgudangtujuan~pdrsumber~pdrjenis~pdrautonotransaksi~pdrnotransaksi~pdrtgl~pdrkodepa~pdrdimintaoleh~pdrdimintaolehkontak~pdrmintake~pdrtgldipakai~pdrestimasikerja~pdrmatauang~pdrkurs~pdrtotalhargain~pdrtotalhargaout~pdrtotalhppin~pdrtotalhppout~pdruraian~pdrcatatan~pdrnoref~pdrtglnoref~pdridbom~pdrstatuswoin~pdrstatuswoout~pdrstatusmrsin~pdrstatusmrsout~pdrstatusmrnin~pdrstatusmrnout~pdrstatuspdin~pdrstatuspdout~pdrstatus~pdrstatussebelumnya~pdrjmlrevisi~pdrcetakanke~pdrinputuser~pdrinputtgl~pdrmodifikasiuser~pdrmodifikasitgl~pdrisclose~pdrcustomtext1~pdrcustomtext2~pdrcustomtext3~pdrcustomtext4~pdrcustomtext5~pdrcustomint1~pdrcustomint2~pdrcustomint3~pdrcustomdbl1~pdrcustomdbl2~pdrcustomdbl3~pdrcustomdate1~pdrcustomdate2~pdrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL1 -------------------------------------------------------
        'idpdrin(0) As Integer, idpdr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpppersen(13) As Double, hpp(14) As Double, 
        'rekpersediaan(15) As String, cabang(16) As String, lokasi(17) As String, gudangasal(18) As String, gudangproduksi(19) As String, 
        'gudangtujuan(20) As String, costcenter(21) As String, divisi(22) As String, subdivisi(23) As String, proyek(24) As String, 
        'catatan(25) As String, urutan(26) As Integer, idbomin(27) As Integer, jmlwo(28) As Double, statuswo(29) As Integer, 
        'jmlmrs(30) As Double, statusmrs(31) As Integer, jmlmrn(32) As Double, statusmrn(33) As Integer, jmlpd(34) As Double, 
        'statuspd(35) As Integer, isclose(36) As Integer, customtext1(37) As String, customtext2(38) As String, customtext3(39) As String, 
        'customdbl1(40) As Double, customdbl2(41) As Double, customdbl3(42) As Double, customdate1(43) As Date, customdate2(44) As Date, 
        'customdate3(45) As Date

        'MAPPING BUAT FLEX DATA DETAIL1 -----------------------------------------------------
        'idpdrin, idpdr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, 
        'hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, 
        'jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, 
        'statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL1 ======================================================
        'SPLIT PARAMETER DATA DETAIL1
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL1 ===============================================

        'Buat datatable DETAIL1
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpdrin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail, "idbomin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlwo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuswo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspd", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim idbarang As Integer = 0, idbomin As Integer = 0, idbomout As Integer = 0

        Dim ftExistOutstandingBomIn As String = "", ftOutstandingBomIn As String = ""
        Dim ftExistOutstandingBomOut As String = "", ftOutstandingBomOut As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL1 ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL1 -----------------------------------
            'CEK ARRAY DATA DETAIL1
            If (dataRowDetail.Length <> 46) Then
                result(2) = "Detail 1 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL1 ----------------------------

            'VALIDASI TIPE DATA DETAIL1 ------------------------------------------
            'idpdrin(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idpdrin required numeric." : GoTo selesai
            End If
            'idpdr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idpdr required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
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
            'idbomin(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - idbomin required numeric." : GoTo selesai
            End If
            'jmlwo(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlwo required numeric." : GoTo selesai
            End If
            'statuswo(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statuswo required numeric." : GoTo selesai
            End If
            'jmlmrs(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
            End If
            'statusmrs(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrs required numeric." : GoTo selesai
            End If
            'jmlmrn(32) As Double
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
            End If
            'statusmrn(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statusmrn required numeric." : GoTo selesai
            End If
            'jmlpd(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd required numeric." : GoTo selesai
            End If
            'statuspd(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - statuspd required numeric." : GoTo selesai
            End If
            'isclose(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(40) As Double
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(43) As Date
            If (IsDate(dataRowDetail(43)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(44) As Date
            If (IsDate(dataRowDetail(44)) = False) Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(45) As Date
            If (IsDate(dataRowDetail(45)) = False) Then
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
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
            End If
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

            'jmlwo(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlwo can't be empty" : GoTo selesai
            End If

            'jmlmrs(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
            End If

            'jmlmrn(32) As Double
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
            End If

            'jmlpd(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
            End If

            'customdbl1(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(43) As Date
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(44) As Date
            If Len(dataRowDetail(44)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(45) As Date
            If Len(dataRowDetail(45)) = 0 Then
                result(2) = "Detail 1 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL1 --------------------------------

            If AsDataTableTambahData(dtdetail, "idpdrin~idpdr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpppersen~hpp~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomin~jmlwo~statuswo~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45)) = False Then
                result(2) = "Detail 1 Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , idbomin(27) As Integer
            idbarang = dataRowDetail(2) : idbomin = dataRowDetail(27)

            'VALIDASI OUTSTANDING -------------------------
            'If idbomin <> 0 Then
            '    '1. CEK DATA EXIST
            '    ftExistOutstandingBomIn = IIf(Len(ftExistOutstandingBomIn.ToString) = 0, "", ftExistOutstandingBomIn & " UNION ")
            '    ftExistOutstandingBomIn = String.Concat(ftExistOutstandingBomIn, "SELECT EXISTS(SELECT 1 FROM m6_bom_in JOIN m6_bom ON idbom = bomid WHERE idbomin = '" & idbomin & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomin & "' as idbomin, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
            '    '2. CEK JML OUTSTANDING
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbomin=" & idbomin)
            '    ftOutstandingBomIn = IIf(Len(ftOutstandingBomIn.ToString) = 0, "", ftOutstandingBomIn & " OR ")
            '    ftOutstandingBomIn = String.Concat(ftOutstandingBomIn, " (bomin.idbomin = " & idbomin & " AND " & Outstanding & " > bomin.jmlbarang) ")
            'End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL1 ===========================================


        'MAPPING BUAT WS DATA DETAIL2 -------------------------------------------------------
        'idpdrout(0) As Integer, idpdr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, hpp(13) As Double, idhppkhususmasuk(14) As Integer, 
        'idhppfifomasuk(15) As Integer, rekpersediaan(16) As String, cabang(17) As String, lokasi(18) As String, gudangasal(19) As String, 
        'gudangproduksi(20) As String, gudangtujuan(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idbomout(28) As Integer, jmlwo(29) As Double, 
        'statuswo(30) As Integer, jmlmrs(31) As Double, statusmrs(32) As Integer, jmlmrn(33) As Double, statusmrn(34) As Integer, 
        'jmlpd(35) As Double, statuspd(36) As Integer, isclose(37) As Integer, customtext1(38) As String, customtext2(39) As String, 
        'customtext3(40) As String, customdbl1(41) As Double, customdbl2(42) As Double, customdbl3(43) As Double, customdate1(44) As Date, 
        'customdate2(45) As Date, customdate3(46) As Date

        'MAPPING BUAT FLEX DATA DETAIL2 -----------------------------------------------------
        'idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, 
        'idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, 
        'gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, 
        'jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL2 ======================================================
        'SPLIT PARAMETER DATA DETAIL2
        dataDetail2 = dataSplit(2).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL2 ===============================================

        'Buat datatable DETAIL2
        Dim dtdetail2 As New DataTable
        AsDataTableTambahField(dtdetail2, "idpdrout", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "idpdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "jmlbarang", AsEnumTypeData.AsDouble)
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
        AsDataTableTambahField(dtdetail2, "idbomout", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlwo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statuswo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlmrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statusmrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "jmlpd", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail2, "statuspd", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail2, "isclose", AsEnumTypeData.AsInt64)
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
            If dataDetail2(i - 1).Length > 0 Then
                'SPLIT DATA DETAIL
                dataRowDetail2 = dataDetail2(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL2 -----------------------------------
                'CEK ARRAY DATA DETAIL2
                If (dataRowDetail2.Length <> 47) Then
                    result(2) = "Detail 2 Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL2 ----------------------------

                'VALIDASI TIPE DATA DETAIL2 ------------------------------------------
                'idpdrout(0) As Integer
                If (IsNumeric(dataRowDetail2(0)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idpdrout required numeric." : GoTo selesai
                End If
                'idpdr(1) As Integer
                If (IsNumeric(dataRowDetail2(1)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idpdr required numeric." : GoTo selesai
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
                'idbomout(28) As Integer
                If (IsNumeric(dataRowDetail2(28)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - idbomout required numeric." : GoTo selesai
                End If
                'jmlwo(29) As Double
                If (IsNumeric(dataRowDetail2(29)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlwo required numeric." : GoTo selesai
                End If
                'statuswo(30) As Integer
                If (IsNumeric(dataRowDetail2(30)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statuswo required numeric." : GoTo selesai
                End If
                'jmlmrs(31) As Double
                If (IsNumeric(dataRowDetail2(31)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrs required numeric." : GoTo selesai
                End If
                'statusmrs(32) As Integer
                If (IsNumeric(dataRowDetail2(32)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statusmrs required numeric." : GoTo selesai
                End If
                'jmlmrn(33) As Double
                If (IsNumeric(dataRowDetail2(33)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrn required numeric." : GoTo selesai
                End If
                'statusmrn(34) As Integer
                If (IsNumeric(dataRowDetail2(34)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statusmrn required numeric." : GoTo selesai
                End If
                'jmlpd(35) As Double
                If (IsNumeric(dataRowDetail2(35)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - jmlpd required numeric." : GoTo selesai
                End If
                'statuspd(36) As Integer
                If (IsNumeric(dataRowDetail2(36)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - statuspd required numeric." : GoTo selesai
                End If
                'isclose(37) As Integer
                If (IsNumeric(dataRowDetail2(37)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(41) As Double
                If (IsNumeric(dataRowDetail2(41)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(42) As Double
                If (IsNumeric(dataRowDetail2(42)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(43) As Double
                If (IsNumeric(dataRowDetail2(43)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(44) As Date
                If (IsDate(dataRowDetail2(44)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(45) As Date
                If (IsDate(dataRowDetail2(45)) = False) Then
                    result(2) = "Detail 2 Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(46) As Date
                If (IsDate(dataRowDetail2(46)) = False) Then
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
                If Len(dataRowDetail2(16)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - rekpersediaan can't be empty" : GoTo selesai
                End If
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

                'jmlwo(29) As Double
                If Len(dataRowDetail2(29)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlwo can't be empty" : GoTo selesai
                End If

                'jmlmrs(31) As Double
                If Len(dataRowDetail2(31)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrs can't be empty" : GoTo selesai
                End If

                'jmlmrn(33) As Double
                If Len(dataRowDetail2(33)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlmrn can't be empty" : GoTo selesai
                End If

                'jmlpd(35) As Double
                If Len(dataRowDetail2(35)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - jmlpd can't be empty" : GoTo selesai
                End If

                'customdbl1(41) As Double
                If Len(dataRowDetail2(41)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(42) As Double
                If Len(dataRowDetail2(42)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(43) As Double
                If Len(dataRowDetail2(43)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(44) As Date
                If Len(dataRowDetail2(44)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(45) As Date
                If Len(dataRowDetail2(45)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(46) As Date
                If Len(dataRowDetail2(46)) = 0 Then
                    result(2) = "Detail 2 Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA DETAIL2 --------------------------------

                If AsDataTableTambahData(dtdetail2, "idpdrout~idpdr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~idhppkhususmasuk~idhppfifomasuk~rekpersediaan~cabang~lokasi~gudangasal~gudangproduksi~gudangtujuan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idbomout~jmlwo~statuswo~jmlmrs~statusmrs~jmlmrn~statusmrn~jmlpd~statuspd~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail2(0) & "~" & dataRowDetail2(1) & "~" & dataRowDetail2(2) & "~" & dataRowDetail2(3) & "~" & dataRowDetail2(4) & "~" & dataRowDetail2(5) & "~" & dataRowDetail2(6) & "~" & dataRowDetail2(7) & "~" & dataRowDetail2(8) & "~" & dataRowDetail2(9) & "~" & dataRowDetail2(10) & "~" & dataRowDetail2(11) & "~" & dataRowDetail2(12) & "~" & dataRowDetail2(13) & "~" & dataRowDetail2(14) & "~" & dataRowDetail2(15) & "~" & dataRowDetail2(16) & "~" & dataRowDetail2(17) & "~" & dataRowDetail2(18) & "~" & dataRowDetail2(19) & "~" & dataRowDetail2(20) & "~" & dataRowDetail2(21) & "~" & dataRowDetail2(22) & "~" & dataRowDetail2(23) & "~" & dataRowDetail2(24) & "~" & dataRowDetail2(25) & "~" & dataRowDetail2(26) & "~" & dataRowDetail2(27) & "~" & dataRowDetail2(28) & "~" & dataRowDetail2(29) & "~" & dataRowDetail2(30) & "~" & dataRowDetail2(31) & "~" & dataRowDetail2(32) & "~" & dataRowDetail2(33) & "~" & dataRowDetail2(34) & "~" & dataRowDetail2(35) & "~" & dataRowDetail2(36) & "~" & dataRowDetail2(37) & "~" & dataRowDetail2(38) & "~" & dataRowDetail2(39) & "~" & dataRowDetail2(40) & "~" & dataRowDetail2(41) & "~" & dataRowDetail2(42) & "~" & dataRowDetail2(43) & "~" & dataRowDetail2(44) & "~" & dataRowDetail2(45) & "~" & dataRowDetail2(46)) = False Then
                    result(2) = "Detail 2 Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'ValidasiSimpan
                'idbarang(2) As Integer      , idbomout(28) As Integer
                idbarang = dataRowDetail2(2) : idbomout = dataRowDetail2(28)

                'VALIDASI OUTSTANDING -------------------------
                'If idbomout <> 0 Then
                '    '1. CEK DATA EXIST ------------------------
                '    ftExistOutstandingBomOut = IIf(Len(ftExistOutstandingBomOut.ToString) = 0, "", ftExistOutstandingBomOut & " UNION ")
                '    ftExistOutstandingBomOut = String.Concat(ftExistOutstandingBomOut, "SELECT EXISTS(SELECT 1 FROM m6_bom_out JOIN m6_bom ON idbom = bomid WHERE idbomout = '" & idbomout & "' AND (bomstatus = 2 OR bomstatus = 3 OR bomstatus = 4 OR bomstatus = 7) LIMIT 1) as rowExists, '" & idbomout & "' as idbomout, bkode FROM m1_item WHERE bid = '" & idbarang & "'")
                '    '2. CEK JML OUTSTANDING
                '    Dim Outstanding As Double = AsDataTableDSum(dtdetail2, "jmlbarang", "idbomout=" & idbomout)
                '    ftOutstandingBomOut = IIf(Len(ftOutstandingBomOut.ToString) = 0, "", ftOutstandingBomOut & " OR ")
                '    ftOutstandingBomOut = String.Concat(ftOutstandingBomOut, " (bomout.idbomout = " & idbomout & " AND " & Outstanding & " > bomout.jmlbarang) ")
                'End If
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("pdrtgl")), AsFormatTanggal(drutama("pdrtgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                ''VALIDASI SIMPAN ========================================
                ''ValidasiSimpan
                'If drutama("pdrstatus") = 2 Then
                '    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingBomIn, ftOutstandingBomIn, dtdetail2, ftExistOutstandingBomOut, ftOutstandingBomOut)
                '    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF VALIDASI SIMPAN =================================

                If isUpdate Then
                    result(4) = drutama("pdrid")
                    notransaksi = drutama("pdrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(pdrid), pdrnotransaksi FROM M6_pdr WHERE pdrid='" & result(4) & "' AND pdrstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pdrid) FROM M6_pdr WHERE pdrnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m6_pdr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M6_Pdr_HistorySimpan("" & paramSplit(0) & "★M6_Pdr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("pdrsumber")) & "▼" & FixQuotes(drutama("pdrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M6_Pdr set pdrcabang  = '" & FixQuotes(drutama("pdrcabang")) & "', pdrlokasi  = '" & FixQuotes(drutama("pdrlokasi")) & "', pdrgudangasal  = '" & FixQuotes(drutama("pdrgudangasal")) & "', pdrgudangproduksi  = '" & FixQuotes(drutama("pdrgudangproduksi")) & "', pdrgudangtujuan  = '" & FixQuotes(drutama("pdrgudangtujuan")) & "', pdrsumber  = '" & FixQuotes(drutama("pdrsumber")) & "', pdrjenis  = '" & FixQuotes(drutama("pdrjenis")) & "', pdrautonotransaksi  = " & drutama("pdrautonotransaksi") & ", pdrnotransaksi  = '" & FixQuotes(notransaksi) & "', pdrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("pdrtgl"))) & "', pdrkodepa  = " & drutama("pdrkodepa") & ", pdrdimintaoleh  = " & drutama("pdrdimintaoleh") & ", pdrdimintaolehkontak  = '" & FixQuotes(drutama("pdrdimintaolehkontak")) & "', pdrmintake  = " & drutama("pdrmintake") & ", pdrtgldipakai  = '" & FixQuotes(AsFormatTanggal(drutama("pdrtgldipakai"))) & "', pdrestimasikerja  = '" & FixQuotes(drutama("pdrestimasikerja")) & "', pdrmatauang  = '" & FixQuotes(drutama("pdrmatauang")) & "', pdrkurs  = '" & FixDouble(drutama("pdrkurs")) & "', pdrtotalhargain  = '" & FixDouble(drutama("pdrtotalhargain")) & "', pdrtotalhargaout  = '" & FixDouble(drutama("pdrtotalhargaout")) & "', pdrtotalhppin  = '" & FixDouble(drutama("pdrtotalhppin")) & "', pdrtotalhppout  = '" & FixDouble(drutama("pdrtotalhppout")) & "', pdruraian  = '" & FixQuotes(drutama("pdruraian")) & "', pdrcatatan  = '" & FixQuotes(drutama("pdrcatatan")) & "', pdrnoref  = '" & FixQuotes(drutama("pdrnoref")) & "', pdrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pdrtglnoref"))) & "', pdridbom  = " & drutama("pdridbom") & ", pdrstatuswoin  = " & drutama("pdrstatuswoin") & ", pdrstatuswoout  = " & drutama("pdrstatuswoout") & ", pdrstatusmrsin  = " & drutama("pdrstatusmrsin") & ", pdrstatusmrsout  = " & drutama("pdrstatusmrsout") & ", pdrstatusmrnin  = " & drutama("pdrstatusmrnin") & ", pdrstatusmrnout  = " & drutama("pdrstatusmrnout") & ", pdrstatuspdin  = " & drutama("pdrstatuspdin") & ", pdrstatuspdout  = " & drutama("pdrstatuspdout") & ", pdrstatus  = " & drutama("pdrstatus") & ", pdrstatussebelumnya  = " & drutama("pdrstatussebelumnya") & ", pdrjmlrevisi  = pdrjmlrevisi+1, pdrcetakanke  = " & drutama("pdrcetakanke") & ", pdrmodifikasiuser  = " & drutama("pdrmodifikasiuser") & ", pdrmodifikasitgl  = NOW(), pdrcustomtext1  = '" & FixQuotes(drutama("pdrcustomtext1")) & "', pdrcustomtext2  = '" & FixQuotes(drutama("pdrcustomtext2")) & "', pdrcustomtext3  = '" & FixQuotes(drutama("pdrcustomtext3")) & "', pdrcustomtext4  = '" & FixQuotes(drutama("pdrcustomtext4")) & "', pdrcustomtext5  = '" & FixQuotes(drutama("pdrcustomtext5")) & "', pdrcustomint1  = " & drutama("pdrcustomint1") & ", pdrcustomint2  = " & drutama("pdrcustomint2") & ", pdrcustomint3  = " & drutama("pdrcustomint3") & ", pdrcustomdbl1  = '" & FixDouble(drutama("pdrcustomdbl1")) & "', pdrcustomdbl2  = '" & FixDouble(drutama("pdrcustomdbl2")) & "', pdrcustomdbl3  = '" & FixDouble(drutama("pdrcustomdbl3")) & "', pdrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate1"))) & "', pdrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate2"))) & "', pdrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate3"))) & "' where pdrid = '" & drutama("pdrid") & "'"
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

                    If drutama("pdrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pdrcabang"), drutama("pdrlokasi"), drutama("pdrsumber"), drutama("pdrtgl"))
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
                        notransaksi = drutama("pdrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pdrid) FROM m6_pdr WHERE pdrnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M6_Pdr (pdrcabang, pdrlokasi, pdrgudangasal, pdrgudangproduksi, pdrgudangtujuan, pdrsumber, pdrjenis, pdrautonotransaksi, pdrnotransaksi, pdrtgl, pdrkodepa, pdrdimintaoleh, pdrdimintaolehkontak, pdrmintake, pdrtgldipakai, pdrestimasikerja, pdrmatauang, pdrkurs, pdrtotalhargain, pdrtotalhargaout, pdrtotalhppin, pdrtotalhppout, pdruraian, pdrcatatan, pdrnoref, pdrtglnoref, pdridbom, pdrstatuswoin, pdrstatuswoout, pdrstatusmrsin, pdrstatusmrsout, pdrstatusmrnin, pdrstatusmrnout, pdrstatuspdin, pdrstatuspdout, pdrstatus, pdrstatussebelumnya, pdrjmlrevisi, pdrcetakanke, pdrinputuser, pdrinputtgl, pdrmodifikasiuser, pdrmodifikasitgl, pdrisclose, pdrcustomtext1, pdrcustomtext2, pdrcustomtext3, pdrcustomtext4, pdrcustomtext5, pdrcustomint1, pdrcustomint2, pdrcustomint3, pdrcustomdbl1, pdrcustomdbl2, pdrcustomdbl3, pdrcustomdate1, pdrcustomdate2, pdrcustomdate3) values('" & FixQuotes(drutama("pdrcabang")) & "', '" & FixQuotes(drutama("pdrlokasi")) & "', '" & FixQuotes(drutama("pdrgudangasal")) & "', '" & FixQuotes(drutama("pdrgudangproduksi")) & "', '" & FixQuotes(drutama("pdrgudangtujuan")) & "', '" & FixQuotes(drutama("pdrsumber")) & "', '" & FixQuotes(drutama("pdrjenis")) & "', " & drutama("pdrautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrtgl"))) & "', " & drutama("pdrkodepa") & ", " & drutama("pdrdimintaoleh") & ", '" & FixQuotes(drutama("pdrdimintaolehkontak")) & "', " & drutama("pdrmintake") & ", '" & FixQuotes(AsFormatTanggal(drutama("pdrtgldipakai"))) & "', '" & FixQuotes(drutama("pdrestimasikerja")) & "', '" & FixQuotes(drutama("pdrmatauang")) & "', '" & FixDouble(drutama("pdrkurs")) & "', '" & FixDouble(drutama("pdrtotalhargain")) & "', '" & FixDouble(drutama("pdrtotalhargaout")) & "', '" & FixDouble(drutama("pdrtotalhppin")) & "', '" & FixDouble(drutama("pdrtotalhppout")) & "', '" & FixQuotes(drutama("pdruraian")) & "', '" & FixQuotes(drutama("pdrcatatan")) & "', '" & FixQuotes(drutama("pdrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrtglnoref"))) & "', " & drutama("pdridbom") & ", " & drutama("pdrstatuswoin") & ", " & drutama("pdrstatuswoout") & ", " & drutama("pdrstatusmrsin") & ", " & drutama("pdrstatusmrsout") & ", " & drutama("pdrstatusmrnin") & ", " & drutama("pdrstatusmrnout") & ", " & drutama("pdrstatuspdin") & ", " & drutama("pdrstatuspdout") & ", " & drutama("pdrstatus") & ", " & drutama("pdrstatussebelumnya") & ", " & drutama("pdrjmlrevisi") & ", " & drutama("pdrcetakanke") & ", " & drutama("pdrinputuser") & ", NOW(), " & drutama("pdrmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("pdrisclose") & ", '" & FixQuotes(drutama("pdrcustomtext1")) & "', '" & FixQuotes(drutama("pdrcustomtext2")) & "', '" & FixQuotes(drutama("pdrcustomtext3")) & "', '" & FixQuotes(drutama("pdrcustomtext4")) & "', '" & FixQuotes(drutama("pdrcustomtext5")) & "', " & drutama("pdrcustomint1") & ", " & drutama("pdrcustomint2") & ", " & drutama("pdrcustomint3") & ", '" & FixDouble(drutama("pdrcustomdbl1")) & "', '" & FixDouble(drutama("pdrcustomdbl2")) & "', '" & FixDouble(drutama("pdrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select pdrid from M6_pdr where pdrnotransaksi='" & notransaksi & "' AND pdrinputuser= '" & userid & "' order by pdrmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail1 ketika update
                If (isUpdate) Then
                    sql = "Delete from M6_Pdr_In where idpdr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpdrin") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpppersen")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomin") & ", '" & FixDouble(dr1("jmlwo")) & "', " & dr1("statuswo") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Pdr_In(idpdrin, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpppersen, hpp, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomin, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M6_Pdr_Out where idpdr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpdrout") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idbomout") & ", '" & FixDouble(dr1("jmlwo")) & "', " & dr1("statuswo") & ", '" & FixDouble(dr1("jmlmrs")) & "', " & dr1("statusmrs") & ", '" & FixDouble(dr1("jmlmrn")) & "', " & dr1("statusmrn") & ", '" & FixDouble(dr1("jmlpd")) & "', " & dr1("statuspd") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M6_Pdr_Out(idpdrout, idpdr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, idhppkhususmasuk, idhppfifomasuk, rekpersediaan, cabang, lokasi, gudangasal, gudangproduksi, gudangtujuan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idbomout, jmlwo, statuswo, jmlmrs, statusmrs, jmlmrn, statusmrn, jmlpd, statuspd, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PDR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M6_PdrUpdateStatusOld(ByVal param As String) As String

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
            Dim sumber As String = "Pdr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pdrtgl, Pdrnotransaksi, Pdrstatus FROM M6_Pdr WHERE Pdrid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pdrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m6_pdr_history
            Dim rsSimpanHistory As String = SimpanHistory.M6_Pdr_HistorySimpan("" & paramSplit(0) & "★M6_Pdr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m6_mrs_terkait("pdrid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================
            End If

            'update status utama
            sql = "UPDATE M6_Pdr SET Pdrstatus = " & nilaiStatus & ", Pdrmodifikasiuser='" & userid & "', Pdrmodifikasitgl = NOW(), Pdrposting = 0, Pdrpostingtgl = '1971-01-01 00:00:00', Pdrjmlrevisi = Pdrjmlrevisi + 1 WHERE Pdrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_PdrSearch(PostWsSearch(paramSplit(0), "M6_PdrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_PdrDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Pdr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pdrid, Pdrnotransaksi FROM M6_Pdr WHERE Pdrid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pdrcabang, pdrlokasi, pdrsumber, pdrautonotransaksi, pdrnotransaksi, pdrtgl"
            sql &= " FROM M6_pdr"
            sql &= " WHERE pdrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pdrcabang")
                lokasi = dtNomorNext.Rows(0)("pdrlokasi")
                sumber = dtNomorNext.Rows(0)("pdrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pdrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pdrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("pdrtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL1
            sql = "DELETE FROM M6_Pdr_In WHERE idpdr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL2
            sql = "DELETE FROM M6_Pdr_Out WHERE idpdr ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M6_Pdr WHERE pdrid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M6_PdrSearch(PostWsSearch(paramSplit(0), "M6_PdrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function m6_pdr_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = ""
        Dim filter5 As String = "", filter6 As String = "", filter7 As String = "", filter8 As String = "", filter9 As String = ""

        'Replace Filter
        If (strFilter.Length > 0) Then
            'BOM IN
            filter1 = strFilter

            'BOM OUT
            filter2 = strFilter

            'WO IN
            filter3 = strFilter
            filter3 = filter3 & " AND (wo.wostatus IN(2,3,4,7)) "

            'WO OUT
            filter4 = strFilter
            filter4 = filter4 & " AND (wo.wostatus IN(2,3,4,7)) "

            'MRS OUT
            filter5 = strFilter
            filter5 = filter5 & " AND (mrs.mrsstatus IN(2,3,4,7)) "

            'MRN OUT
            filter6 = strFilter
            filter6 = filter6 & " AND (mrn.mrnstatus IN(2,3,4,7)) "

            'PD IN
            filter7 = strFilter
            filter7 = filter7 & " AND (pd.pdstatus IN(2,3,4,7)) "

            'PD OUT
            filter8 = strFilter
            filter8 = filter8 & " AND (pd.pdstatus IN(2,3,4,7)) "

            'JOURNAL
            filter9 = strFilter
        Else
            'WO IN
            filter3 = " (wo.wostatus IN(2,3,4,7)) "

            'WO OUT
            filter4 = " (wo.wostatus IN(2,3,4,7)) "

            'MRS OUT
            filter5 = " (mrs.mrsstatus IN(2,3,4,7)) "

            'MRN OUT
            filter6 = " (mrn.mrnstatus IN(2,3,4,7)) "

            'PD IN
            filter7 = " (pd.pdstatus IN(2,3,4,7)) "

            'PD OUT
            filter8 = " (pd.pdstatus IN(2,3,4,7)) "

        End If

        'BOM IN
        If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        'BOM OUT
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        'WO IN
        If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        'WO OUT
        If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        'MRS OUT
        If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        'MRN OUT
        If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        'PD IN
        If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        'PD OUT
        If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        'JOURNAL
        If Len(filter9) > 0 Then filter9 = " WHERE " & filter9

        'BOM IN
        sql = "  select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, bom.bomsumber AS sumber, bom.bomid AS idterkait, bom.bomnotransaksi AS noterkait, bom.bomtgl AS tglterkait, bom.bominputtgl AS inputtglterkait, bom.bommodifikasitgl AS modifikasitglterkait, 0 as jenisterkait from m6_bom_in bomi join m6_bom bom on bomi.idbom = bom.bomid join m6_pdr_in pdri on bomi.idbomin = pdri.idbomin join m6_pdr pdr ON pdri.idpdr = pdr.pdrid " & filter1 & " group by bom.bomid, pdr.pdrid"
        'BOM OUT
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, bom.bomsumber AS sumber, bom.bomid AS idterkait, bom.bomnotransaksi AS noterkait, bom.bomtgl AS tglterkait, bom.bominputtgl AS inputtglterkait, bom.bommodifikasitgl AS modifikasitglterkait, 0 as jenisterkait from m6_bom_out bomo join m6_bom bom on bomo.idbom = bom.bomid join m6_pdr_out pdro on bomo.idbomout = pdro.idbomout join m6_pdr pdr ON pdro.idpdr = pdr.pdrid " & filter2 & " group by bom.bomid, pdr.pdrid "
        'WO IN
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, wo.wosumber AS sumber, wo.woid AS idterkait, wo.wonotransaksi AS noterkait, wo.wotgl AS tglterkait, wo.woinputtgl AS inputtglterkait, wo.womodifikasitgl AS modifikasitglterkait, 1 as jenisterkait from m6_wo_in woi join m6_wo wo on woi.idwo = wo.woid join m6_pdr_in pdri on woi.idpdrin = pdri.idpdrin join m6_pdr pdr ON pdri.idpdr = pdr.pdrid " & filter3 & " group by wo.woid, pdr.pdrid "
        'WO OUT
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, wo.wosumber AS sumber, wo.woid AS idterkait, wo.wonotransaksi AS noterkait, wo.wotgl AS tglterkait, wo.woinputtgl AS inputtglterkait, wo.womodifikasitgl AS modifikasitglterkait, 1 as jenisterkait from m6_wo_out woo join m6_wo wo on woo.idwo = wo.woid join m6_pdr_out pdro on woo.idpdrout = pdro.idpdrout join m6_pdr pdr ON pdro.idpdr = pdr.pdrid " & filter4 & " group by wo.woid, pdr.pdrid "
        'MRS OUT
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, mrs.mrssumber AS sumber, mrs.mrsid AS idterkait, mrs.mrsnotransaksi AS noterkait, mrs.mrstgl AS tglterkait, mrs.mrsinputtgl AS inputtglterkait, mrs.mrsmodifikasitgl AS modifikasitglterkait, 1 as jenisterkait from m6_mrs_out mrso join m6_mrs mrs on mrso.idmrs = mrs.mrsid join m6_pdr_out pdro on mrso.idpdrout = pdro.idpdrout join m6_pdr pdr ON pdro.idpdr = pdr.pdrid " & filter5 & " group by mrs.mrsid, pdr.pdrid"
        'MRN OUT
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, mrn.mrnsumber AS sumber, mrn.mrnid AS idterkait, mrn.mrnnotransaksi AS noterkait, mrn.mrntgl AS tglterkait, mrn.mrninputtgl AS inputtglterkait, mrn.mrnmodifikasitgl AS modifikasitglterkait, 1 as jenisterkait from m6_mrn_out mrno join m6_mrn mrn on mrno.idmrn = mrn.mrnid join m6_pdr_out pdro on mrno.idpdrout = pdro.idpdrout join m6_pdr pdr ON pdro.idpdr = pdr.pdrid " & filter6 & " group by mrn.mrnid, pdr.pdrid"
        'PD IN
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, pd.pdsumber AS sumber, pd.pdid AS idterkait, pd.pdnotransaksi AS noterkait, pd.pdtgl AS tglterkait, pd.pdinputtgl AS inputtglterkait, pd.pdmodifikasitgl AS modifikasitglterkait, 1 as jenisterkait from m6_pd_in pdi join m6_pd pd on pdi.idpd = pd.pdid join m6_pdr_in pdri on pdi.idpdrin = pdri.idpdrin join m6_pdr pdr ON pdri.idpdr = pdr.pdrid " & filter7 & " group by pd.pdid, pdr.pdrid"
        'PD OUT
        sql &= " UNION ALL "
        sql &= " select pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, pd.pdsumber AS sumber, pd.pdid AS idterkait, pd.pdnotransaksi AS noterkait, pd.pdtgl AS tglterkait, pd.pdinputtgl AS inputtglterkait, pd.pdmodifikasitgl AS modifikasitglterkait, 1 as jenisterkait from m6_pd_out pdo join m6_pd pd on pdo.idpd = pd.pdid join m6_pdr_out pdro on pdo.idpdrout = pdro.idpdrout join m6_pdr pdr ON pdro.idpdr = pdr.pdrid " & filter8 & " group by pd.pdid, pdr.pdrid"
        'JOURNAL
        sql &= " UNION ALL "
        sql &= " SELECT pdr.pdrid AS pdrid, pdr.pdrnotransaksi AS pdrnotransaksi, t.tsumber AS sumber, t.tidtransaksi AS idterkait, t.tnotransaksi AS noterkait, t.ttgl AS tglterkait, t.tinputtgl AS inputtglterkait, t.tmodifikasitgl AS modifikasitglterkait,  1 as jenisterkait FROM m6_pdr pdr JOIN m2_transaction_journal t ON pdr.pdrnotransaksi = t.tcostcenter " & filter9 & " GROUP BY pdr.pdrid, t.tsumber, t.tidtransaksi"

        'GRUPKAN BERDASARKAN ID DAN IDTERKAIT
        sql = " SELECT * FROM ( " & sql & " ) as terkait GROUP BY pdrid, sumber, idterkait "

        Return sql
    End Function

End Class