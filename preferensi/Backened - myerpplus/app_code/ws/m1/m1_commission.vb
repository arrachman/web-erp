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
Public Class m1_commission
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_CommissionSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean
        Dim search As String = ""
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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'kmkode(0) As String, kmnama(1) As String, kmketerangan(2) As String, kmaktif(3) As Integer, kminputuser(4) As Integer, 
        'kminputtgl(5) As DateTime, kmmodifikasiuser(6) As Integer, kmmodifikasitgl(7) As DateTime, kmcustomtext1(8) As String, kmcustomtext2(9) As String, 
        'kmcustomtext3(10) As String, kmcustomtext4(11) As String, kmcustomtext5(12) As String, kmcustomint1(13) As Integer, kmcustomint2(14) As Integer, 
        'kmcustomint3(15) As Integer, kmcustomdbl1(16) As Double, kmcustomdbl2(17) As Double, kmcustomdbl3(18) As Double, kmcustomdate1(19) As Date, 
        'kmcustomdate2(20) As Date, kmcustomdate3(21) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kmkode, kmnama, kmketerangan, kmaktif, kminputuser, 
        'kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmcustomtext1, kmcustomtext2, 
        'kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomint1, kmcustomint2, 
        'kmcustomint3, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdate1, 
        'kmcustomdate2, kmcustomdate3

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA
        If (dataUtama.Length <> 22) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'kminputuser(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "kminputuser required numeric." : GoTo selesai
        End If
        'kminputtgl(5) As DateTime
        If (IsDate(dataUtama(5)) = False) Then
            result(2) = "kmkminputtgl required date." : GoTo selesai
        End If
        'kmmodifikasiuser(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "kmmodifikasiuser required numeric." : GoTo selesai
        End If
        'kmmodifikasitgl(7) As DateTime
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "kmmodifikasitgl required date." : GoTo selesai
        End If
        'kmcustomint1(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "kmcustomint1 required numeric." : GoTo selesai
        End If
        'kmcustomint2(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "kmcustomint2 required numeric." : GoTo selesai
        End If
        'kmcustomint3(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "kmcustomint3 required numeric." : GoTo selesai
        End If
        'kmcustomdbl1(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "kmcustomdbl1 required numeric." : GoTo selesai
        End If
        'kmcustomdbl2(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "kmcustomdbl2 required numeric." : GoTo selesai
        End If
        'kmcustomdbl3(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "kmcustomdbl3 required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'kmkode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "kmkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "kmkode should not be more than 25 character." : GoTo selesai
        End If

        'kmnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kmnama can't be empty" : GoTo selesai
        End If

        'kminputtgl(5) As DateTime
        If Len(dataUtama(5)) = 0 Then
            result(2) = "kminputtgl can't be empty" : GoTo selesai
        End If

        'kmmodifikasitgl(7) As DateTime
        If Len(dataUtama(7)) = 0 Then
            result(2) = "kmmodifikasitgl can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA ========================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kmkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmketerangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomdbl1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "kmcustomdbl2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "kmcustomdbl3", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "kmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "kmkode~kmnama~kmketerangan~kmaktif~kminputuser~kminputtgl~kmmodifikasiuser~kmmodifikasitgl~kmcustomtext1~kmcustomtext2~kmcustomtext3~kmcustomtext4~kmcustomtext5~kmcustomint1~kmcustomint2~kmcustomint3~kmcustomdbl1~kmcustomdbl2~kmcustomdbl3~kmcustomdate1~kmcustomdate2~kmcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'kmdiddetail(0) As Integer, kmdkodekomisi(1) As String, kmdkriteria(2) As Integer, kmdoperator(3) As String, kmdjml1(4) As Double, 
        'kmdjml2(5) As Double, kmdkriterianilai(6) As Double, kmdnilai(7) As Integer, kmdcustomtext1(8) As String, kmdcustomtext2(9) As String, 
        'kmdcustomtext3(10) As String, kmdcustomtext4(11) As String, kmdcustomtext5(12) As String, kmdcustomint1(13) As Integer, kmdcustomint2(14) As Integer, 
        'kmdcustomdbl3(15) As Integer, kmdcustomdbl1(16) As Double, kmdcustomdbl2(17) As Double, kmdcustomdbl3(18) As Double, kmdcustomdate1(19) As Date, 
        'kmdcustomdate2(20) As Date, customdate3(21) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'kmdiddetail, kmdkodekomisi, kmdkriteria, kmdoperator, kmdjml1, 
        'kmdjml2, kmdkriterianilai, kmdnilai, kmdcustomtext1, kmdcustomtext2, 
        'kmdcustomtext3, kmdcustomtext4, kmdcustomtext5, kmdcustomint1, kmdcustomint2, 
        'kmdcustomdbl3, kmdcustomdbl1, kmdcustomdbl2, kmdcustomdbl3, kmdcustomdate1, 
        'kmdcustomdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "kmdiddetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kmdkodekomisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdkriteria", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kmdoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdkriterianilai", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kmdnilai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kmdcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kmdcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kmdcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kmdcustomdate3", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'kmdiddetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - kmdiddetail required numeric." : GoTo selesai
            End If
            'kmdkriteria(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - kmdkriteria required numeric." : GoTo selesai
            End If
            'kmdjml1(4) As Integer
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kmdjml1 required numeric." : GoTo selesai
            End If
            'kmdjml2(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kmdjml2 required numeric." : GoTo selesai
            End If
            'kmdkriterianilai(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - kmdkriterianilai required numeric." : GoTo selesai
            End If
            'kmdnilai(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - kmdnilai required numeric." : GoTo selesai
            End If
            'kmdcustomint1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomint1 required numeric." : GoTo selesai
            End If
            'kmdcustomint2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomint2 required numeric." : GoTo selesai
            End If
            'kmdcustomint3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomint3 required numeric." : GoTo selesai
            End If
            'kmdcustomdbl1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomdbl1 required numeric." : GoTo selesai
            End If
            'kmdcustomdbl2(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomdbl2 required numeric." : GoTo selesai
            End If
            'kmdcustomdbl3(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomdbl3 required numeric." : GoTo selesai
            End If
            'kmdcustomdate1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomdate1 required date." : GoTo selesai
            End If
            'kmdcustomdate2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomdate2 required date." : GoTo selesai
            End If
            'kmdcustomdate3(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - kmdcustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'kmdiddetail(0) As Double
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - kmdiddetail can't be empty" : GoTo selesai
            End If

            'kmdkodekomisi(1) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - kmdkodekomisi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - kmdkodekomisi should not be more than 25 character." : GoTo selesai
            End If

            'kmdkriteria(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kmdkriteria can't be empty" : GoTo selesai
            End If

            'kmdoperator(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - kmdoperator can't be empty" : GoTo selesai
            End If
            If IsNumeric(dataRowDetail(3)) = False Then
                result(2) = "Row : " & i & " - kmdoperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(3) <> 0 And dataRowDetail(3) <> 1 And dataRowDetail(3) <> 2 Then
                result(2) = "Row : " & i & " - invalid kmdoperator value" : GoTo selesai
            End If

            'kmdcustomint1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomint1 can't be empty" : GoTo selesai
            End If

            'kmdcustomint2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomint2 can't be empty" : GoTo selesai
            End If

            'kmdcustomint3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomint3 can't be empty" : GoTo selesai
            End If

            'kmdcustomdbl1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomdbl1 can't be empty" : GoTo selesai
            End If

            'kmdcustomdbl2(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomdbl2 can't be empty" : GoTo selesai
            End If

            'kmdcustomdbl3(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomdbl3 can't be empty" : GoTo selesai
            End If

            'kmdcustomdate1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomdate1 can't be empty" : GoTo selesai
            End If

            'kmdcustomdate2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomdate2 can't be empty" : GoTo selesai
            End If

            'kmdcustomdate3(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - kmdcustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "kmdiddetail~kmdkodekomisi~kmdkriteria~kmdoperator~kmdjml1~kmdjml2~kmdkriterianilai~kmdnilai~kmdcustomtext1~kmdcustomtext2~kmdcustomtext3~kmdcustomtext4~kmdcustomtext5~kmdcustomint1~kmdcustomint2~kmdcustomint3~kmdcustomdbl1~kmdcustomdbl2~kmdcustomdbl3~kmdcustomdate1~kmdcustomdate2~kmdcustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." & dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) : GoTo selesai
            End If

        Next

        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(kmkode) FROM M1_Commission WHERE kmkode ='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_commission_history
                    Dim commissionSimpanHistory As String = SimpanHistory.M1_Commission_HistorySimpan("" & paramSplit(0) & "★M1_Commission_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim commissionSplit() As String = commissionSimpanHistory.Split(sptParam)
                    Dim commissionSplitResult() As String = commissionSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (commissionSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & commissionSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Commission set kmnama  = '" & dataUtama(1) & "', kmketerangan  = '" & dataUtama(2) & "', kmaktif  = " & dataUtama(3) & ", kmmodifikasiuser  = " & dataUtama(6) & ", kmmodifikasitgl  = NOW(), kmcustomtext1  = '" & FixQuotes(dataUtama(8)) & "', kmcustomtext2  = '" & FixQuotes(dataUtama(9)) & "', kmcustomtext3  = '" & FixQuotes(dataUtama(10)) & "', kmcustomtext4  = '" & FixQuotes(dataUtama(11)) & "', kmcustomtext5  = '" & FixQuotes(dataUtama(12)) & "', kmcustomint1  = " & dataUtama(13) & ", kmcustomint2  = " & dataUtama(14) & ", kmcustomint3  = " & dataUtama(15) & ", kmcustomdbl1  = '" & FixDouble(dataUtama(16)) & "', kmcustomdbl2  = '" & FixDouble(dataUtama(17)) & "', kmcustomdbl3  = '" & FixDouble(dataUtama(18)) & "', kmcustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(19))) & "', kmcustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(20))) & "', kmcustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(21))) & "' where kmkode = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    sql = "Delete from M1_Commission_Detail where kmdkodekomisi = '" & dataUtama(0) & "'"
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
                sql = "Insert into M1_Commission (kmkode, kmnama, kmketerangan, kmaktif, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdate1, kmcustomdate2, kmcustomdate3) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', " & dataUtama(3) & ", " & dataUtama(4) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(5), "yyyy-MM-dd H:mm:ss")) & "', " & dataUtama(6) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(7), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(dataUtama(10)) & "', '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', " & dataUtama(13) & ", " & dataUtama(14) & ", " & dataUtama(15) & ", '" & dataUtama(16) & "', '" & dataUtama(17) & "', '" & dataUtama(18) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(19), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(20), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(21), "yyyy-MM-dd")) & "')"
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
                Dim dtOperator As New DataTable
                Dim vOperator As String = ""
                For Each dr1 As DataRow In dtdetail.Rows
                    'CEK OPERATOR :
                    'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                    '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                    'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                    sql = "SELECT kmd.kmdkodekomisi as kategori, kmd.kmdoperator as operator, (CASE kmd.kmdoperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m1_commission_detail kmd WHERE kmd.kmdkodekomisi = '" & dataUtama(0) & "' GROUP BY kmd.kmdoperator ORDER BY kmd.kmdoperator"
                    dtOperator = AsDataTableAmbilDariDB(sql)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Commission : " & dataUtama(0) & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("kmdoperator") = 2 Or (vOperator = 1 And dr1("kmdoperator") = vOperator) Then
                                        result(2) = "Commission : " & dataUtama(0) & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'INSERT DETAIL
                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("(" & dr1("kmdiddetail") & ", '" & dataUtama(0) & "', " & dr1("kmdkriteria") & ", '" & FixQuotes(dr1("kmdoperator")) & "', " & dr1("kmdjml1") & ", " & dr1("kmdjml2") & ", " & dr1("kmdkriterianilai") & ", " & dr1("kmdnilai") & ", '" & FixQuotes(dr1("kmdcustomtext1")) & "', '" & FixQuotes(dr1("kmdcustomtext2")) & "', '" & FixQuotes(dr1("kmdcustomtext3")) & "', '" & FixQuotes(dr1("kmdcustomtext4")) & "', '" & FixQuotes(dr1("kmdcustomtext5")) & "', " & dr1("kmdcustomint1") & ", " & dr1("kmdcustomint2") & ", " & dr1("kmdcustomint3") & ", '" & FixDouble(dr1("kmdcustomdbl1")) & "', '" & FixDouble(dr1("kmdcustomdbl2")) & "', '" & FixDouble(dr1("kmdcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("kmdcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("kmdcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("kmdcustomdate3"))) & "')")
                    sql = "Insert into M1_Commission_Detail(kmdiddetail,kmdkodekomisi,kmdkriteria,kmdoperator,kmdjml1,kmdjml2,kmdkriterianilai,kmdnilai,kmdcustomtext1,kmdcustomtext2,kmdcustomtext3,kmdcustomtext4,kmdcustomtext5,kmdcustomint1,kmdcustomint2,kmdcustomint3,kmdcustomdbl1,kmdcustomdbl2,kmdcustomdbl3,kmdcustomdate1,kmdcustomdate2,kmdcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next

            Else
                result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai

            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_CommissionSearch(PostWsSearch(paramSplit(0), "M1_CommissionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_CommissionDelete(ByVal param As String) As String

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
            result(2) = "kmkode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_CommissionTerkait(PostWsTerkait(paramSplit(0), "M1_ContactTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_commission_history
            Dim commissionSimpanHistory As String = SimpanHistory.M1_Commission_HistorySimpan("" & paramSplit(0) & "★M1_Commission_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim commissionSplit() As String = commissionSimpanHistory.Split(sptParam)
            Dim commissionSplitResult() As String = commissionSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (commissionSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & commissionSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM m1_commission WHERE kmkode = '" & idtransaksi & "'; DELETE FROM m1_commission_detail where kmdkodekomisi = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_CommissionSearch(PostWsSearch(paramSplit(0), "M1_CommissionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_CommissionSearch(ByVal param As String) As String
        'M1_CommissionSearch --------------------------------------------------------
        'kmkode, kmnama, kmketerangan, kmaktif, kminputuser, 
        'kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmcustomtext1, kmcustomtext2, 
        'kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomint1, kmcustomint2, 
        'kmcustomint3, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdate1, 
        'kmcustomdate2, kmcustomdate3, kminputusernama, kmmodifikasiusernama

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

        'BUAT QUERY 
        sql = "SELECT km.kmkode, km.kmnama, km.kmketerangan, km.kmaktif, km.kminputuser, km.kminputtgl, km.kmmodifikasiuser, km.kmmodifikasitgl, km.kmcustomtext1, km.kmcustomtext2, km.kmcustomtext3, km.kmcustomtext4, km.kmcustomtext5, km.kmcustomint1, km.kmcustomint2, km.kmcustomint3, km.kmcustomdbl1, km.kmcustomdbl2, km.kmcustomdbl3, km.kmcustomdate1, km.kmcustomdate2, km.kmcustomdate3, u1.unama as kminputusernama, u2.unama as kmmodifikasiusernama FROM m1_commission km LEFT JOIN m0_user u1 ON km.kminputuser = u1.userid LEFT JOIN m0_user u2 ON km.kmmodifikasiuser = u2.userid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Commission", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kmkode"), ""), sptField,
                     FxDB(dr("kmnama"), ""), sptField,
                     FxDB(dr("kmketerangan"), ""), sptField,
                     FxDB(dr("kmaktif"), 0), sptField,
                     FxDB(dr("kminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmcustomtext1"), ""), sptField,
                     FxDB(dr("kmcustomtext2"), ""), sptField,
                     FxDB(dr("kmcustomtext3"), ""), sptField,
                     FxDB(dr("kmcustomtext4"), ""), sptField,
                     FxDB(dr("kmcustomtext5"), ""), sptField,
                     FxDB(dr("kmcustomint1"), 0), sptField,
                     FxDB(dr("kmcustomint2"), 0), sptField,
                     FxDB(dr("kmcustomint3"), 0), sptField,
                     FxDB(dr("kmcustomdbl1"), 0), sptField,
                     FxDB(dr("kmcustomdbl2"), 0), sptField,
                     FxDB(dr("kmcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kmcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kminputusernama"), ""), sptField,
                     FxDB(dr("kmmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Commission data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmkode, kmnama, kmketerangan, kmaktif, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdate1, kmcustomdate2, kmcustomdate3, kminputusernama, kmmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_CommissionCekId(ByVal param As String) As String

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
            result(2) = "kmkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(kmkode) FROM m1_commission WHERE kmkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column bkode." : GoTo selesai
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
    Public Function M1_CommissionTerkait(ByVal param As String) As String
        'M1_CommissionTerkait --------------------------------------------------------
        'kmkode, kmnama, sumber, idterkait

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
            result(2) = "pkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim query As String = "select `km`.`kmkode` AS `kmkode`,`km`.`kmnama` AS `kmnama`,'CONTACT' AS `sumber`,`c`.`kkode` AS `idterkait` from `m1_contact` `c` join `m1_commission` `km` on `c`.`kkomisikode` = `km`.`kmkode` where km.kmkode='valkode' GROUP BY km.kmkode, c.kid"
        query = query.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Commission", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , query) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("kmkode"), ""), sptField,
                             FxDB(dr("kmnama"), ""), sptField,
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
            result(2) = "Related Commission data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmkode, kmnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_CommissionGetdataById(ByVal param As String) As String

        'M1_CommissionGetdataById Utama --------------------------------------------------------
        'kmkode, kmnama, kmketerangan, kmaktif, kminputuser, 
        'kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmcustomtext1, kmcustomtext2, 
        'kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomint1, kmcustomint2, 
        'kmcustomint3, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdate1, 
        'kmcustomdate2, kmcustomdate3, kminputusernama, kmmodifikasiusernama

        'M3_MrGetdataById Detail -------------------------------------------------------
        'kmdiddetail, kmdkodekomisi, kmdkriteria, kmdoperator, kmdjml1, 
        'kmdjml2, kmdkriterianilai, kmdnilai, kmdcustomtext1, kmdcustomtext2, 
        'kmdcustomtext3, kmdcustomtext4, kmdcustomtext5, kmdcustomint1, kmdcustomint2, 
        'kmdcustomdbl3, kmdcustomdbl1, kmdcustomdbl2, kmdcustomdbl3, kmdcustomdate1, 
        'kmdcustomdate2, customdate3

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
        'If (IsNumeric(paramSplit(3)) = False) Then
        '    result(2) = "idtransaksi required numeric." : GoTo selesai
        'End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M1_Commission~M1_Commission_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "kmkode = " & idtransaksi
        Else ' jika filter diisi
            Filter = "kmkode = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'BUAT QUERY 
        sql = "SELECT km.kmkode, km.kmnama, km.kmketerangan, km.kmaktif, km.kminputuser, km.kminputtgl, km.kmmodifikasiuser, km.kmmodifikasitgl, km.kmcustomtext1, km.kmcustomtext2, km.kmcustomtext3, km.kmcustomtext4, km.kmcustomtext5, km.kmcustomint1, km.kmcustomint2, km.kmcustomint3, km.kmcustomdbl1, km.kmcustomdbl2, km.kmcustomdbl3, km.kmcustomdate1, km.kmcustomdate2, km.kmcustomdate3, u1.unama as kminputusernama, u2.unama as kmmodifikasiusernama,kmd.kmdiddetail,kmd.kmdkodekomisi,kmd.kmdkriteria,kmd.kmdoperator,kmd.kmdjml1,kmd.kmdjml2,kmd.kmdkriterianilai,kmd.kmdnilai, kmd.kmdcustomtext1, kmd.kmdcustomtext2, kmd.kmdcustomtext3, kmd.kmdcustomtext4, kmd.kmdcustomtext5, kmd.kmdcustomint1, kmd.kmdcustomint2, kmd.kmdcustomint3, kmd.kmdcustomdbl1, kmd.kmdcustomdbl2, kmd.kmdcustomdbl3, kmd.kmdcustomdate1, kmd.kmdcustomdate2, kmd.kmdcustomdate3 FROM (((m1_commission km JOIN m1_commission_detail kmd ON((`km`.`kmkode` = `kmd`.`kmdkodekomisi`)))LEFT JOIN m0_user u1 ON km.kminputuser = u1.userid)LEFT JOIN m0_user u2 ON km.kmmodifikasiuser = u2.userid)"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(
                     FxDB(drutama("kmkode"), ""), sptField,
                     FxDB(drutama("kmnama"), ""), sptField,
                     FxDB(drutama("kmketerangan"), ""), sptField,
                     FxDB(drutama("kmaktif"), 0), sptField,
                     FxDB(drutama("kminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kmcustomtext1"), ""), sptField,
                     FxDB(drutama("kmcustomtext2"), ""), sptField,
                     FxDB(drutama("kmcustomtext3"), ""), sptField,
                     FxDB(drutama("kmcustomtext4"), ""), sptField,
                     FxDB(drutama("kmcustomtext5"), ""), sptField,
                     FxDB(drutama("kmcustomint1"), 0), sptField,
                     FxDB(drutama("kmcustomint2"), 0), sptField,
                     FxDB(drutama("kmcustomint3"), 0), sptField,
                     FxDB(drutama("kmcustomdbl1"), 0), sptField,
                     FxDB(drutama("kmcustomdbl2"), 0), sptField,
                     FxDB(drutama("kmcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("kminputusernama"), ""), sptField,
                     FxDB(drutama("kmmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("kmdiddetail"), 0), sptField,
                     FxDB(dr("kmdkodekomisi"), ""), sptField,
                     FxDB(dr("kmdkriteria"), 0), sptField,
                     FxDB(dr("kmdoperator"), ""), sptField,
                     FxDB(dr("kmdjml1"), 0), sptField,
                     FxDB(dr("kmdjml2"), 0), sptField,
                     FxDB(dr("kmdkriterianilai"), 0), sptField,
                     FxDB(dr("kmdnilai"), 0), sptField,
                     FxDB(dr("kmdcustomtext1"), ""), sptField,
                     FxDB(dr("kmdcustomtext2"), ""), sptField,
                     FxDB(dr("kmdcustomtext3"), ""), sptField,
                     FxDB(dr("kmdcustomtext4"), ""), sptField,
                     FxDB(dr("kmdcustomtext5"), ""), sptField,
                     FxDB(dr("kmdcustomint1"), 0), sptField,
                     FxDB(dr("kmdcustomint2"), 0), sptField,
                     FxDB(dr("kmdcustomint3"), 0), sptField,
                     FxDB(dr("kmdcustomdbl1"), 0), sptField,
                     FxDB(dr("kmdcustomdbl2"), 0), sptField,
                     FxDB(dr("kmdcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kmdcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kmdcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kmdcustomdate3"), ""), formatTgl), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmkode, kmnama, kmketerangan, kmaktif, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdate1, kmcustomdate2, kmcustomdate3, kminputusernama, kmmodifikasiusernama" & sptSubParam & "kmdiddetail, kmdkodekomisi, kmdkriteria, kmdoperator, kmdjml1, kmdjml2, kmdkriterianilai, kmdnilai, kmdcustomtext1, kmdcustomtext2, kmdcustomtext3, kmdcustomtext4, kmdcustomtext5, kmdcustomint1, kmdcustomint2, kmdcustomdbl3, kmdcustomdbl1, kmdcustomdbl2, kmdcustomdbl3, kmdcustomdate1, kmdcustomdate2, kmdcustomdate3"))

        Return wsResult
    End Function

End Class