Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m6_machine_plotting
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M6_Machine_PlottingSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = ""
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


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'mesin(0) As String, tgl(1) As Date, kelas(2) As String, subkelas(3) As String, jml(4) As Double, 
        'satuan(5) As String, nilaisatuan(6) As Double, jmlbarang(7) As Double, satuanbarang(8) As String, matauang(9) As String, 
        'kurs(10) As Double, harga(11) As Double, hpp(12) As Double, cabang(13) As String, lokasi(14) As String, 
        'gudang(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'catatan(20) As String, urutan(21) As Integer, jmlrealisasi(22) As Double, statusrealisasi(23) As Integer, insertby(24) As Integer, 
        'isclose(25) As Integer, inputuser(26) As Bigint, inputtgl(27) As DateTime, modifikasiuser(28) As Bigint, modifikasitgl(29) As DateTime, 
        'customtext1(30) As String, customtext2(31) As String, customtext3(32) As String, customtext4(33) As String, customtext5(34) As String, 
        'customtext6(35) As String, customtext7(36) As String, customtext8(37) As String, customtext9(38) As String, customtext10(39) As String, 
        'customint1(40) As Integer, customint2(41) As Integer, customint3(42) As Integer, customint4(43) As Integer, customint5(44) As Integer, 
        'customint6(45) As Integer, customint7(46) As Integer, customint8(47) As Integer, customint9(48) As Integer, customint10(49) As Integer, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdbl4(53) As Double, customdbl5(54) As Double, 
        'customdbl6(55) As Double, customdbl7(56) As Double, customdbl8(57) As Double, customdbl9(58) As Double, customdbl10(59) As Double, 
        'customdate1(60) As Date, customdate2(61) As Date, customdate3(62) As Date, customdate4(63) As Date, customdate5(64) As Date, 
        'customdate6(65) As Date, customdate7(66) As Date, customdate8(67) As Date, customdate9(68) As Date, customdate10(69) As Date


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "mesin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "insertby", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "inputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "modifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "modifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 70) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'tgl(1) As Date
            If (IsDate(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - tgl required date." : GoTo selesai
            End If
            'jml(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hpp(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'urutan(21) As Integer
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'jmlrealisasi(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlrealisasi required numeric." : GoTo selesai
            End If
            'statusrealisasi(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - statusrealisasi required numeric." : GoTo selesai
            End If
            'insertby(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - insertby required numeric." : GoTo selesai
            End If
            'isclose(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'inputtgl(27) As DateTime
            If (IsDate(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - inputtgl required date." : GoTo selesai
            End If
            'modifikasitgl(29) As DateTime
            If (IsDate(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - modifikasitgl required date." : GoTo selesai
            End If
            'customint1(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - customint1 required numeric." : GoTo selesai
            End If
            'customint2(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - customint2 required numeric." : GoTo selesai
            End If
            'customint3(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - customint3 required numeric." : GoTo selesai
            End If
            'customint4(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - customint4 required numeric." : GoTo selesai
            End If
            'customint5(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - customint5 required numeric." : GoTo selesai
            End If
            'customint6(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - customint6 required numeric." : GoTo selesai
            End If
            'customint7(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - customint7 required numeric." : GoTo selesai
            End If
            'customint8(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customint8 required numeric." : GoTo selesai
            End If
            'customint9(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customint9 required numeric." : GoTo selesai
            End If
            'customint10(49) As Integer
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customint10 required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdbl4(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl4 required numeric." : GoTo selesai
            End If
            'customdbl5(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl5 required numeric." : GoTo selesai
            End If
            'customdbl6(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdbl6 required numeric." : GoTo selesai
            End If
            'customdbl7(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdbl7 required numeric." : GoTo selesai
            End If
            'customdbl8(57) As Double
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - customdbl8 required numeric." : GoTo selesai
            End If
            'customdbl9(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdbl9 required numeric." : GoTo selesai
            End If
            'customdbl10(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - customdbl10 required numeric." : GoTo selesai
            End If
            'customdate1(60) As Date
            If (IsDate(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(61) As Date
            If (IsDate(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(62) As Date
            If (IsDate(dataRowDetail(62)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'customdate4(63) As Date
            If (IsDate(dataRowDetail(63)) = False) Then
                result(2) = "Row : " & i & " - customdate4 required date." : GoTo selesai
            End If
            'customdate5(64) As Date
            If (IsDate(dataRowDetail(64)) = False) Then
                result(2) = "Row : " & i & " - customdate5 required date." : GoTo selesai
            End If
            'customdate6(65) As Date
            If (IsDate(dataRowDetail(65)) = False) Then
                result(2) = "Row : " & i & " - customdate6 required date." : GoTo selesai
            End If
            'customdate7(66) As Date
            If (IsDate(dataRowDetail(66)) = False) Then
                result(2) = "Row : " & i & " - customdate7 required date." : GoTo selesai
            End If
            'customdate8(67) As Date
            If (IsDate(dataRowDetail(67)) = False) Then
                result(2) = "Row : " & i & " - customdate8 required date." : GoTo selesai
            End If
            'customdate9(68) As Date
            If (IsDate(dataRowDetail(68)) = False) Then
                result(2) = "Row : " & i & " - customdate9 required date." : GoTo selesai
            End If
            'customdate10(69) As Date
            If (IsDate(dataRowDetail(69)) = False) Then
                result(2) = "Row : " & i & " - customdate10 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'mesin(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - mesin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 500 Then
                result(2) = "Row : " & i & " - mesin should not be more than 500 character." : GoTo selesai
            End If

            'tgl(1) As Date
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - tgl can't be empty" : GoTo selesai
            End If

            'kelas(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kelas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 500 Then
                result(2) = "Row : " & i & " - kelas should not be more than 500 character." : GoTo selesai
            End If

            'subkelas(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - subkelas can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 500 Then
                result(2) = "Row : " & i & " - subkelas should not be more than 500 character." : GoTo selesai
            End If

            'jml(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If

            'satuan(5) As String
            'If Len(dataRowDetail(5)) = 0 Then
            '    result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(5)) > 500 Then
                result(2) = "Row : " & i & " - satuan should not be more than 500 character." : GoTo selesai
            End If

            'nilaisatuan(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If

            'satuanbarang(8) As String
            'If Len(dataRowDetail(8)) = 0 Then
            '    result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(8)) > 500 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 500 character." : GoTo selesai
            End If

            'matauang(9) As String
            'If Len(dataRowDetail(9)) = 0 Then
            '    result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(9)) > 500 Then
                result(2) = "Row : " & i & " - matauang should not be more than 500 character." : GoTo selesai
            End If

            'kurs(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hpp(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'jmlrealisasi(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlrealisasi can't be empty" : GoTo selesai
            End If

            'inputuser(26) As 
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - inputuser can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 20 Then
                result(2) = "Row : " & i & " - inputuser should not be more than 20 character." : GoTo selesai
            End If

            'inputtgl(27) As DateTime
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - inputtgl can't be empty" : GoTo selesai
            End If

            'modifikasiuser(28) As 
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - modifikasiuser can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(28)) > 20 Then
                result(2) = "Row : " & i & " - modifikasiuser should not be more than 20 character." : GoTo selesai
            End If

            'modifikasitgl(29) As DateTime
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - modifikasitgl can't be empty" : GoTo selesai
            End If

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdbl4(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
            End If

            'customdbl5(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
            End If

            'customdbl6(55) As Double
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdbl6 can't be empty" : GoTo selesai
            End If

            'customdbl7(56) As Double
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdbl7 can't be empty" : GoTo selesai
            End If

            'customdbl8(57) As Double
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdbl8 can't be empty" : GoTo selesai
            End If

            'customdbl9(58) As Double
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdbl9 can't be empty" : GoTo selesai
            End If

            'customdbl10(59) As Double
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdbl10 can't be empty" : GoTo selesai
            End If

            'customdate1(60) As Date
            If Len(dataRowDetail(60)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(61) As Date
            If Len(dataRowDetail(61)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(62) As Date
            If Len(dataRowDetail(62)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'customdate4(63) As Date
            If Len(dataRowDetail(63)) = 0 Then
                result(2) = "Row : " & i & " - customdate4 can't be empty" : GoTo selesai
            End If

            'customdate5(64) As Date
            If Len(dataRowDetail(64)) = 0 Then
                result(2) = "Row : " & i & " - customdate5 can't be empty" : GoTo selesai
            End If

            'customdate6(65) As Date
            If Len(dataRowDetail(65)) = 0 Then
                result(2) = "Row : " & i & " - customdate6 can't be empty" : GoTo selesai
            End If

            'customdate7(66) As Date
            If Len(dataRowDetail(66)) = 0 Then
                result(2) = "Row : " & i & " - customdate7 can't be empty" : GoTo selesai
            End If

            'customdate8(67) As Date
            If Len(dataRowDetail(67)) = 0 Then
                result(2) = "Row : " & i & " - customdate8 can't be empty" : GoTo selesai
            End If

            'customdate9(68) As Date
            If Len(dataRowDetail(68)) = 0 Then
                result(2) = "Row : " & i & " - customdate9 can't be empty" : GoTo selesai
            End If

            'customdate10(69) As Date
            If Len(dataRowDetail(69)) = 0 Then
                result(2) = "Row : " & i & " - customdate10 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "mesin~tgl~kelas~subkelas~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~hpp~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~jmlrealisasi~statusrealisasi~insertby~isclose~inputuser~inputtgl~modifikasiuser~modifikasitgl~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customint1~customint2~customint3~customint4~customint5~customint6~customint7~customint8~customint9~customint10~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("mesin")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl"))) & "', '" & FixQuotes(dr1("kelas")) & "', '" & FixQuotes(dr1("subkelas")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("insertby") & ", " & dr1("isclose") & ", '" & FixQuotes(userid) & "', NOW(), '" & FixQuotes(0) & "', '" & FixQuotes(AsFormatTanggal("1971-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', " & dr1("customint1") & ", " & dr1("customint2") & ", " & dr1("customint3") & ", " & dr1("customint4") & ", " & dr1("customint5") & ", " & dr1("customint6") & ", " & dr1("customint7") & ", " & dr1("customint8") & ", " & dr1("customint9") & ", " & dr1("customint10") & ", '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "')")
                Next
                sql = "Insert into M6_Machine_Plotting(mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values " & strValue2.ToString & " "
                sql &= " On Duplicate Key Update mesin = values (mesin), tgl = values (tgl), kelas = values (kelas), subkelas = values (subkelas), jml = values (jml), satuan = values (satuan), nilaisatuan = values (nilaisatuan), jmlbarang = values (jmlbarang), satuanbarang = values (satuanbarang), matauang = values (matauang), kurs = values (kurs), harga = values (harga), hpp = values (hpp), cabang = values (cabang), lokasi = values (lokasi), gudang = values (gudang), costcenter = values (costcenter), divisi = values (divisi), subdivisi = values (subdivisi), proyek = values (proyek), catatan = values (catatan), urutan = values (urutan), jmlrealisasi = values (jmlrealisasi), statusrealisasi = values (statusrealisasi), insertby = values (insertby), isclose = values (isclose), modifikasiuser = '" & FixQuotes(userid) & "', modifikasitgl = NOW(), customtext1 = values (customtext1), customtext2 = values (customtext2), customtext3 = values (customtext3), customtext4 = values (customtext4), customtext5 = values (customtext5), customtext6 = values (customtext6), customtext7 = values (customtext7), customtext8 = values (customtext8), customtext9 = values (customtext9), customtext10 = values (customtext10), customint1 = values (customint1), customint2 = values (customint2), customint3 = values (customint3), customint4 = values (customint4), customint5 = values (customint5), customint6 = values (customint6), customint7 = values (customint7), customint8 = values (customint8), customint9 = values (customint9), customint10 = values (customint10), customdbl1 = values (customdbl1), customdbl2 = values (customdbl2), customdbl3 = values (customdbl3), customdbl4 = values (customdbl4), customdbl5 = values (customdbl5), customdbl6 = values (customdbl6), customdbl7 = values (customdbl7), customdbl8 = values (customdbl8), customdbl9 = values (customdbl9), customdbl10 = values (customdbl10), customdate1 = values (customdate1), customdate2 = values (customdate2), customdate3 = values (customdate3), customdate4 = values (customdate4), customdate5 = values (customdate5), customdate6 = values (customdate6), customdate7 = values (customdate7), customdate8 = values (customdate8), customdate9 = values (customdate9), customdate10 = values (customdate10) "
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

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
    Public Function M6_Machine_PlottingSearch(ByVal param As String) As String
        'M6_Machine_PlottingSearch --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama

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
        sql = "select `mp`.`mesin` AS `mesin`,`mp`.`tgl` AS `tgl`,`mp`.`kelas` AS `kelas`,`mp`.`subkelas` AS `subkelas`,`mp`.`jml` AS `jml`,`mp`.`satuan` AS `satuan`,`mp`.`nilaisatuan` AS `nilaisatuan`,`mp`.`jmlbarang` AS `jmlbarang`,`mp`.`satuanbarang` AS `satuanbarang`,`mp`.`matauang` AS `matauang`,`mp`.`kurs` AS `kurs`,`mp`.`harga` AS `harga`,`mp`.`hpp` AS `hpp`,`mp`.`cabang` AS `cabang`,`mp`.`lokasi` AS `lokasi`,`mp`.`gudang` AS `gudang`,`mp`.`costcenter` AS `costcenter`,`mp`.`divisi` AS `divisi`,`mp`.`subdivisi` AS `subdivisi`,`mp`.`proyek` AS `proyek`,`mp`.`catatan` AS `catatan`,`mp`.`urutan` AS `urutan`,`mp`.`jmlrealisasi` AS `jmlrealisasi`,`mp`.`statusrealisasi` AS `statusrealisasi`,`mp`.`insertby` AS `insertby`,`mp`.`isclose` AS `isclose`,`mp`.`inputuser` AS `inputuser`,`mp`.`inputtgl` AS `inputtgl`,`mp`.`modifikasiuser` AS `modifikasiuser`,`mp`.`modifikasitgl` AS `modifikasitgl`,`mp`.`customtext1` AS `customtext1`,`mp`.`customtext2` AS `customtext2`,`mp`.`customtext3` AS `customtext3`,`mp`.`customtext4` AS `customtext4`,`mp`.`customtext5` AS `customtext5`,`mp`.`customtext6` AS `customtext6`,`mp`.`customtext7` AS `customtext7`,`mp`.`customtext8` AS `customtext8`,`mp`.`customtext9` AS `customtext9`,`mp`.`customtext10` AS `customtext10`,`mp`.`customint1` AS `customint1`,`mp`.`customint2` AS `customint2`,`mp`.`customint3` AS `customint3`,`mp`.`customint4` AS `customint4`,`mp`.`customint5` AS `customint5`,`mp`.`customint6` AS `customint6`,`mp`.`customint7` AS `customint7`,`mp`.`customint8` AS `customint8`,`mp`.`customint9` AS `customint9`,`mp`.`customint10` AS `customint10`,`mp`.`customdbl1` AS `customdbl1`,`mp`.`customdbl2` AS `customdbl2`,`mp`.`customdbl3` AS `customdbl3`,`mp`.`customdbl4` AS `customdbl4`,`mp`.`customdbl5` AS `customdbl5`,`mp`.`customdbl6` AS `customdbl6`,`mp`.`customdbl7` AS `customdbl7`,`mp`.`customdbl8` AS `customdbl8`,`mp`.`customdbl9` AS `customdbl9`,`mp`.`customdbl10` AS `customdbl10`,`mp`.`customdate1` AS `customdate1`,`mp`.`customdate2` AS `customdate2`,`mp`.`customdate3` AS `customdate3`,`mp`.`customdate4` AS `customdate4`,`mp`.`customdate5` AS `customdate5`,`mp`.`customdate6` AS `customdate6`,`mp`.`customdate7` AS `customdate7`,`mp`.`customdate8` AS `customdate8`,`mp`.`customdate9` AS `customdate9`,`mp`.`customdate10` AS `customdate10`,`m`.`mnama` AS `mesinnama`,`cl`.`cnama` AS `kelasnama`,`sb`.`scnama` AS `subkelasnama`,`u1`.`unama` AS `inputusernama`,`u2`.`unama` AS `modifikasiusernama` from (((((`m6_machine_plotting` `mp` left join `m1_machine` `m` on((`mp`.`mesin` = `m`.`mkode`))) left join `m1_class` `cl` on((`mp`.`kelas` = `cl`.`ckode`))) left join `m1_subclass` `sb` on((`mp`.`subkelas` = `sb`.`sckode`))) left join `m0_user` `u1` on((`mp`.`inputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`mp`.`modifikasiuser` = `u2`.`userid`)))"

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_Machine_PlottingGenerate(ByVal param As String) As String
        'M6_Machine_PlottingGenerate --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

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


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
        Else
            result(2) = "Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
        Else
            result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            If kategoriAwal <> kategoriAkhir Then
                result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
        Else
            result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            If kelompokAwal <> kelompokAkhir Then
                result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            End If
        Else
            kelompokAkhir = kelompokAwal
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'GENERATE TANGGAL
        sql = "CALL p_plotting_machine_tgl(" & FixDouble(userid) & ",'" & FixQuotes(AsFormatTanggal(tglAwal)) & "','" & FixQuotes(AsFormatTanggal(tglAkhir)) & "') "
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed generate date data." : GoTo selesai
        End If


        'PANGGIL QUERY
        sql = "  SELECT m.mkode as mesin, pl.tgl as tgl, IFNULL(mp.kelas,cl.ckode) as kelas, IFNULL(mp.subkelas,sb.sckode) as subkelas, IFNULL(mp.jml,0) as jml, IFNULL(mp.satuan,'') as satuan, IFNULL(mp.nilaisatuan,0) as nilaisatuan, IFNULL(mp.jmlbarang,0) as jmlbarang, IFNULL(mp.satuanbarang,'') as satuanbarang, IFNULL(mp.matauang,'') as matauang, IFNULL(mp.kurs,0) as kurs, IFNULL(mp.harga,0) as harga, IFNULL(mp.hpp,0) as hpp, IFNULL(mp.cabang,'') as cabang, IFNULL(mp.lokasi,'') as lokasi, IFNULL(mp.gudang,'') as gudang, IFNULL(mp.costcenter,'') as costcenter, IFNULL(mp.divisi,'') as divisi, IFNULL(mp.subdivisi,'') as subdivisi, IFNULL(mp.proyek,'') as proyek, IFNULL(mp.catatan,'') as catatan, IFNULL(mp.urutan,0) as urutan, IFNULL(mp.jmlrealisasi,0) as jmlrealisasi, IFNULL(mp.statusrealisasi,0) as statusrealisasi, IFNULL(mp.insertby,0) as insertby, IFNULL(mp.isclose,0) as isclose, IFNULL(mp.inputuser,0) as inputuser, IFNULL(mp.inputtgl,'1971-01-01 00:00:00') as inputtgl, IFNULL(mp.modifikasiuser,0) as modifikasiuser, IFNULL(mp.modifikasitgl,'1971-01-01 00:00:00') as modifikasitgl, IFNULL(mp.customtext1,'') as customtext1, IFNULL(mp.customtext2,'') as customtext2, IFNULL(mp.customtext3,'') as customtext3, IFNULL(mp.customtext4,'') as customtext4, IFNULL(mp.customtext5,'') as customtext5, IFNULL(mp.customtext6,'') as customtext6, IFNULL(mp.customtext7,'') as customtext7, IFNULL(mp.customtext8,'') as customtext8,IFNULL(mp.customtext9,'') as customtext9, IFNULL(mp.customtext10,'') as customtext10, IFNULL(mp.customint1,0) as customint1, IFNULL(mp.customint2,0) as customint2, IFNULL(mp.customint3,0) as customint3, IFNULL(mp.customint4,0) as customint4, IFNULL(mp.customint5,0) as customint5, IFNULL(mp.customint6,0) as customint6, IFNULL(mp.customint7,0) as customint7, IFNULL(mp.customint8,0) as customint8, IFNULL(mp.customint9,0) as customint9, IFNULL(mp.customint10,0) as customint10, IFNULL(mp.customdbl1,0) as customdbl1, IFNULL(mp.customdbl2,0) as customdbl2, IFNULL(mp.customdbl3,0) as customdbl3, IFNULL(mp.customdbl4,0) as customdbl4, IFNULL(mp.customdbl5,0) as customdbl5, IFNULL(mp.customdbl6,0) as customdbl6, IFNULL(mp.customdbl7,0) as customdbl7, IFNULL(mp.customdbl8,0) as customdbl8, IFNULL(mp.customdbl9,0) as customdbl9, IFNULL(mp.customdbl10,0) as customdbl10, IFNULL(mp.customdate1,'1900-01-01') as customdate1, IFNULL(mp.customdate2,'1900-01-01') as customdate2, IFNULL(mp.customdate3,'1900-01-01') as customdate3, IFNULL(mp.customdate4,'1900-01-01') as customdate4, IFNULL(mp.customdate5,'1900-01-01') as customdate5, IFNULL(mp.customdate6,'1900-01-01') as customdate6, IFNULL(mp.customdate7,'1900-01-01') as customdate7, IFNULL(mp.customdate8,'1900-01-01') as customdate8, IFNULL(mp.customdate9,'1900-01-01') as customdate9, IFNULL(mp.customdate10,'1900-01-01') as customdate10, m.mnama as mesinnama, IFNULL(cl2.cnama,cl.cnama) as kelasnama, IFNULL(sb2.scnama,sb.scnama) as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama "
        sql &= " FROM m2r_plotting_machine_tgl pl "
        sql &= " JOIN m1_machine m ON m.mkode BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "' "
        sql &= " JOIN m1_class cl ON cl.ckode BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "' "
        sql &= " JOIN m1_subclass sb ON sb.sckode BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "' "
        sql &= " AND pl.userid = '" & FixDouble(userid) & "' "
        sql &= " LEFT JOIN m6_machine_plotting mp ON m.mkode = mp.mesin AND pl.tgl = mp.tgl AND (mp.insertby = 1 OR mp.isclose = 1) "
        sql &= " LEFT JOIN m0_user u1 ON mp.inputuser = u1.userid "
        sql &= " LEFT JOIN m0_user u2 ON mp.modifikasiuser = u2.userid "
        sql &= " LEFT JOIN m1_class cl2 ON mp.kelas = cl2.ckode "
        sql &= " LEFT JOIN m1_subclass sb2 ON mp.subkelas = sb2.sckode "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_Machine_PlottingGenerateMulti(ByVal param As String) As String
        'M6_Machine_PlottingGenerate --------------------------------------------------------
        'mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, 
        'modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, 
        'customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, 
        'customint3, customint4, customint5, customint6, customint7, customint8, customint9, 
        'customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, 
        'customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, 
        'mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

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


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            Dim vMesin As String() = dataUtama(2).Split(sptRow)
            Dim JmlDtMesin As Integer = vMesin.Length
            For i = 1 To JmlDtMesin
                If Len(vMesin(i - 1)) > 0 Then
                    mesinAwal = IIf(Len(mesinAwal.ToString) = 0, "", mesinAwal & " , ")
                    mesinAwal = String.Concat(mesinAwal, "'" & vMesin(i - 1) & "'")
                Else
                    result(2) = "1. Machine can't be empty." : GoTo selesai
                End If
            Next

            If Len(mesinAwal) = 0 Then
                result(2) = "2. Machine can't be empty." : GoTo selesai
            End If

        Else
            result(2) = "3. Machine can't be empty." : GoTo selesai
        End If

        'mesinAkhir(3) As String
        'If Len(dataUtama(3)) > 0 Then
        '    mesinAkhir = dataUtama(3)
        'Else
        '    mesinAkhir = mesinAwal
        'End If
        mesinAkhir = ""

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
        Else
            result(2) = "Class can't be empty." : GoTo selesai
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
            If kategoriAwal <> kategoriAkhir Then
                result(2) = "Class 2 can't be different with Class 1." : GoTo selesai
            End If
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
        Else
            result(2) = "Sub Class can't be empty." : GoTo selesai
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
            If kelompokAwal <> kelompokAkhir Then
                result(2) = "Sub Class 2 can't be different with Sub Class 1." : GoTo selesai
            End If
        Else
            kelompokAkhir = kelompokAwal
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'GENERATE TANGGAL
        sql = "CALL p_plotting_machine_tgl(" & FixDouble(userid) & ",'" & FixQuotes(AsFormatTanggal(tglAwal)) & "','" & FixQuotes(AsFormatTanggal(tglAkhir)) & "') "
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed generate date data." : GoTo selesai
        End If


        'PANGGIL QUERY
        sql = "  SELECT m.mkode as mesin, pl.tgl as tgl, IFNULL(mp.kelas,cl.ckode) as kelas, IFNULL(mp.subkelas,sb.sckode) as subkelas, IFNULL(mp.jml,0) as jml, IFNULL(mp.satuan,'') as satuan, IFNULL(mp.nilaisatuan,0) as nilaisatuan, IFNULL(mp.jmlbarang,0) as jmlbarang, IFNULL(mp.satuanbarang,'') as satuanbarang, IFNULL(mp.matauang,'') as matauang, IFNULL(mp.kurs,0) as kurs, IFNULL(mp.harga,0) as harga, IFNULL(mp.hpp,0) as hpp, IFNULL(mp.cabang,'') as cabang, IFNULL(mp.lokasi,'') as lokasi, IFNULL(mp.gudang,'') as gudang, IFNULL(mp.costcenter,'') as costcenter, IFNULL(mp.divisi,'') as divisi, IFNULL(mp.subdivisi,'') as subdivisi, IFNULL(mp.proyek,'') as proyek, IFNULL(mp.catatan,'') as catatan, IFNULL(mp.urutan,0) as urutan, IFNULL(mp.jmlrealisasi,0) as jmlrealisasi, IFNULL(mp.statusrealisasi,0) as statusrealisasi, IFNULL(mp.insertby,0) as insertby, IFNULL(mp.isclose,0) as isclose, IFNULL(mp.inputuser,0) as inputuser, IFNULL(mp.inputtgl,'1971-01-01 00:00:00') as inputtgl, IFNULL(mp.modifikasiuser,0) as modifikasiuser, IFNULL(mp.modifikasitgl,'1971-01-01 00:00:00') as modifikasitgl, IFNULL(mp.customtext1,'') as customtext1, IFNULL(mp.customtext2,'') as customtext2, IFNULL(mp.customtext3,'') as customtext3, IFNULL(mp.customtext4,'') as customtext4, IFNULL(mp.customtext5,'') as customtext5, IFNULL(mp.customtext6,'') as customtext6, IFNULL(mp.customtext7,'') as customtext7, IFNULL(mp.customtext8,'') as customtext8,IFNULL(mp.customtext9,'') as customtext9, IFNULL(mp.customtext10,'') as customtext10, IFNULL(mp.customint1,0) as customint1, IFNULL(mp.customint2,0) as customint2, IFNULL(mp.customint3,0) as customint3, IFNULL(mp.customint4,0) as customint4, IFNULL(mp.customint5,0) as customint5, IFNULL(mp.customint6,0) as customint6, IFNULL(mp.customint7,0) as customint7, IFNULL(mp.customint8,0) as customint8, IFNULL(mp.customint9,0) as customint9, IFNULL(mp.customint10,0) as customint10, IFNULL(mp.customdbl1,0) as customdbl1, IFNULL(mp.customdbl2,0) as customdbl2, IFNULL(mp.customdbl3,0) as customdbl3, IFNULL(mp.customdbl4,0) as customdbl4, IFNULL(mp.customdbl5,0) as customdbl5, IFNULL(mp.customdbl6,0) as customdbl6, IFNULL(mp.customdbl7,0) as customdbl7, IFNULL(mp.customdbl8,0) as customdbl8, IFNULL(mp.customdbl9,0) as customdbl9, IFNULL(mp.customdbl10,0) as customdbl10, IFNULL(mp.customdate1,'1900-01-01') as customdate1, IFNULL(mp.customdate2,'1900-01-01') as customdate2, IFNULL(mp.customdate3,'1900-01-01') as customdate3, IFNULL(mp.customdate4,'1900-01-01') as customdate4, IFNULL(mp.customdate5,'1900-01-01') as customdate5, IFNULL(mp.customdate6,'1900-01-01') as customdate6, IFNULL(mp.customdate7,'1900-01-01') as customdate7, IFNULL(mp.customdate8,'1900-01-01') as customdate8, IFNULL(mp.customdate9,'1900-01-01') as customdate9, IFNULL(mp.customdate10,'1900-01-01') as customdate10, m.mnama as mesinnama, IFNULL(cl2.cnama,cl.cnama) as kelasnama, IFNULL(sb2.scnama,sb.scnama) as subkelasnama, IFNULL(u1.unama,'') as inputusernama, IFNULL(u2.unama,'') as modifikasiusernama "
        sql &= " FROM m2r_plotting_machine_tgl pl "
        sql &= " JOIN m1_machine m ON m.mkode IN ( " & (mesinAwal) & " ) "
        sql &= " JOIN m1_class cl ON cl.ckode BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "' "
        sql &= " JOIN m1_subclass sb ON sb.sckode BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "' "
        sql &= " AND pl.userid = '" & FixDouble(userid) & "' "
        sql &= " LEFT JOIN m6_machine_plotting mp ON m.mkode = mp.mesin AND pl.tgl = mp.tgl AND (mp.insertby = 1 OR mp.isclose = 1) "
        sql &= " LEFT JOIN m0_user u1 ON mp.inputuser = u1.userid "
        sql &= " LEFT JOIN m0_user u2 ON mp.modifikasiuser = u2.userid "
        sql &= " LEFT JOIN m1_class cl2 ON mp.kelas = cl2.ckode "
        sql &= " LEFT JOIN m1_subclass sb2 ON mp.subkelas = sb2.sckode "

        dt = AmbilData("aplikasi1-m6_pl_v", Filter, sorting, True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("mesin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kelas"), ""), sptField,
                     FxDB(dr("subkelas"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("insertby"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("inputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("modifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customint1"), 0), sptField,
                     FxDB(dr("customint2"), 0), sptField,
                     FxDB(dr("customint3"), 0), sptField,
                     FxDB(dr("customint4"), 0), sptField,
                     FxDB(dr("customint5"), 0), sptField,
                     FxDB(dr("customint6"), 0), sptField,
                     FxDB(dr("customint7"), 0), sptField,
                     FxDB(dr("customint8"), 0), sptField,
                     FxDB(dr("customint9"), 0), sptField,
                     FxDB(dr("customint10"), 0), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     FxDB(dr("mesinnama"), ""), sptField,
                     FxDB(dr("kelasnama"), ""), sptField,
                     FxDB(dr("subkelasnama"), ""), sptField,
                     FxDB(dr("inputusernama"), ""), sptField,
                     FxDB(dr("modifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("mesin, tgl, kelas, subkelas, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, hpp, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, jmlrealisasi, statusrealisasi, insertby, isclose, inputuser, inputtgl, modifikasiuser, modifikasitgl, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, mesinnama, kelasnama, subkelasnama, inputusernama, modifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M6_Machine_PlottingClose(ByVal param As String) As String
        'On Error GoTo selesai

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String, strResultData As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = "", notransaksi As String = ""
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


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 9) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String, isClose(8) As Integer

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = "", isClose As Integer = 0


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
        Else
            kelompokAkhir = kelompokAwal
        End If

        'isClose(8) As String
        If (dataUtama(8) <> 0 And dataUtama(8) <> 1) Then
            result(2) = "isClose required numeric. (0/1)" : GoTo selesai
        Else
            isClose = dataUtama(8)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'UPDATE CLOSE/UNCLOSE
            sql = "  UPDATE m6_machine_plotting SET isclose = " & FixDouble(isClose) & " "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            If Len(mesinAwal) > 0 Then sql &= " AND mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "' "
            If Len(kategoriAwal) > 0 Then sql &= " AND kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "' "
            If Len(kelompokAwal) > 0 Then sql &= " AND subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "' "
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim f As String
            f = " tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            If Len(mesinAwal) > 0 Then f &= " AND mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "' "
            If Len(kategoriAwal) > 0 Then f &= " AND kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "' "
            If Len(kelompokAwal) > 0 Then f &= " AND subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "' "

            Dim paramSearch As String = M6_Machine_PlottingSearch(PostWsSearch(paramSplit(0), "M6_Machine_PlottingSearch", pagingSplit(0), pagingSplit(1), f, sorting, formatTgl, formatTglWaktu))

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
    Public Function M6_Machine_PlottingDelete(ByVal param As String) As String
        'On Error GoTo selesai

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strplrt(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String, strResultData As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", sorting As String = "", notransaksi As String = ""
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


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Or paramSplit(3) <= 0 Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 8) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As String, tglAkhir(1) As String, mesinAwal(2) As String, mesinAkhir(3) As String, 
        'kategoriAwal(4) As String, kategoriAkhir(5) As String, kelompokAwal(6) As String, kelompokAkhir(7) As String

        Dim tglAwal As String = "", tglAkhir As String = "", mesinAwal As String = "", mesinAkhir As String = ""
        Dim kategoriAwal As String = "", kategoriAkhir As String = "", kelompokAwal As String = "", kelompokAkhir As String = ""


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "Start date required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            result(2) = "Start date can't be empty." : GoTo selesai
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "End date required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            result(2) = "End date can't be empty." : GoTo selesai
        End If

        'mesinAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            mesinAwal = dataUtama(2)
        End If

        'mesinAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            mesinAkhir = dataUtama(3)
        Else
            mesinAkhir = mesinAwal
        End If

        'kategoriAwal(4) As String
        If Len(dataUtama(4)) > 0 Then
            kategoriAwal = dataUtama(4)
        End If

        'kategoriAkhir(5) As String
        If Len(dataUtama(5)) > 0 Then
            kategoriAkhir = dataUtama(5)
        Else
            kategoriAkhir = kategoriAwal
        End If

        'kelompokAwal(6) As String
        If Len(dataUtama(6)) > 0 Then
            kelompokAwal = dataUtama(6)
        End If

        'kelompokAkhir(7) As String
        If Len(dataUtama(7)) > 0 Then
            kelompokAkhir = dataUtama(7)
        Else
            kelompokAkhir = kelompokAwal
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'VALIDASI DATA CLOSE
            sql = "  SELECT mesin, tgl, kelas, subkelas FROM m6_machine_plotting "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            If Len(mesinAwal) > 0 Then sql &= " AND mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "' "
            If Len(kategoriAwal) > 0 Then sql &= " AND kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "' "
            If Len(kelompokAwal) > 0 Then sql &= " AND subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "' "
            sql &= " AND isclose = 1"
            Dim dtVal As DataTable = AsDataTableAmbilDariDB(sql)
            If dtVal.Rows.Count > 0 Then
                result(2) = "Can't delete data, Plotting Machine " & dtVal.Rows(0)("mesin") & " - " & AsFormatTanggal(dtVal.Rows(0)("tgl"), formatTgl) & " has been close" : Trans.Rollback() : GoTo selesai
            End If


            'DELETE DATA
            sql = "  DELETE FROM m6_machine_plotting "
            sql &= " WHERE tgl BETWEEN '" & FixQuotes(AsFormatTanggal(tglAwal)) & "' AND '" & FixQuotes(AsFormatTanggal(tglAkhir)) & "' "
            If Len(mesinAwal) > 0 Then sql &= " AND mesin BETWEEN '" & FixQuotes(mesinAwal) & "' AND '" & FixQuotes(mesinAkhir) & "' "
            If Len(kategoriAwal) > 0 Then sql &= " AND kelas BETWEEN '" & FixQuotes(kategoriAwal) & "' AND '" & FixQuotes(kategoriAkhir) & "' "
            If Len(kelompokAwal) > 0 Then sql &= " AND subkelas BETWEEN '" & FixQuotes(kelompokAwal) & "' AND '" & FixQuotes(kelompokAkhir) & "' "
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult

    End Function

End Class