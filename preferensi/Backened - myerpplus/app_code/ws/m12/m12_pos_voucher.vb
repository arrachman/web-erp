Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_voucher
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_VoucherSimpan(ByVal param As String) As String
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
        'viid(0) As Integer, vikategori(1) As String, vicabang(2) As String, vilokasi(3) As String, vikode(4) As String, 
        'vimatauang(5) As String, vijml(6) As Double, vijmlvalas(7) As Double, vijmlbayar(8) As Double, vijmlbayarvalas(9) As Double, 
        'vitgllunas(10) As Date, viisclose(11) As Integer, vicustomtext1(12) As String, vicustomtext2(13) As String, vicustomtext3(14) As String, 
        'vicustomdbl1(15) As Double, vicustomdbl2(16) As Double, vicustomdbl3(17) As Double, vicustomdate1(18) As Date, vicustomdate2(19) As Date, 
        'vicustomdate3(20) As Date, vitglbuat(21) As Date, vitglexpired(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, 
        'vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "viid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "viisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "vicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglbuat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglexpired", AsEnumTypeData.AsString)


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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'viid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - viid required numeric." : GoTo selesai
            End If
            'vijml(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - vijml required numeric." : GoTo selesai
            End If
            'vijmlvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - vijmlvalas required numeric." : GoTo selesai
            End If
            'vijmlbayar(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayar required numeric." : GoTo selesai
            End If
            'vijmlbayarvalas(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayarvalas required numeric." : GoTo selesai
            End If
            'vitgllunas(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - vitgllunas required date." : GoTo selesai
            End If
            'viisclose(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - viisclose required numeric." : GoTo selesai
            End If
            'vicustomdbl1(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl1 required numeric." : GoTo selesai
            End If
            'vicustomdbl2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl2 required numeric." : GoTo selesai
            End If
            'vicustomdbl3(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl3 required numeric." : GoTo selesai
            End If
            'vicustomdate1(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate1 required date." : GoTo selesai
            End If
            'vicustomdate2(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate2 required date." : GoTo selesai
            End If
            'vicustomdate3(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate3 required date." : GoTo selesai
            End If
            'vitglbuat(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - vitglbuat required date." : GoTo selesai
            End If
            'vitglexpired(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - vitglexpired required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'vikategori(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - vikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - vikategori should not be more than 25 character." : GoTo selesai
            End If

            'vicabang(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - vicabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - vicabang should not be more than 25 character." : GoTo selesai
            End If

            'vilokasi(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - vilokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - vilokasi should not be more than 25 character." : GoTo selesai
            End If

            'vikode(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - vikode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 100 Then
                result(2) = "Row : " & i & " - vikode should not be more than 100 character." : GoTo selesai
            End If

            'vimatauang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - vimatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - vimatauang should not be more than 25 character." : GoTo selesai
            End If

            'vijml(6) As Double
            If Len(dataRowDetail(6)) <= 0 Then
                result(2) = "Row : " & i & " - vijml can't be empty" : GoTo selesai
            End If

            'vijmlvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - vijmlvalas can't be empty" : GoTo selesai
            End If

            'vijmlbayar(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayar can't be empty" : GoTo selesai
            End If

            'vijmlbayarvalas(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayarvalas can't be empty" : GoTo selesai
            End If

            'vitgllunas(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - vitgllunas can't be empty" : GoTo selesai
            End If

            'vicustomdbl1(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl1 can't be empty" : GoTo selesai
            End If

            'vicustomdbl2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl2 can't be empty" : GoTo selesai
            End If

            'vicustomdbl3(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl3 can't be empty" : GoTo selesai
            End If

            'vicustomdate1(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate1 can't be empty" : GoTo selesai
            End If

            'vicustomdate2(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate2 can't be empty" : GoTo selesai
            End If

            'vicustomdate3(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate3 can't be empty" : GoTo selesai
            End If


            'vitglbuat(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - vitglbuat can't be empty" : GoTo selesai
            End If

            'vitglexpired(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - vitglexpired can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "viid~vikategori~vicabang~vilokasi~vikode~vimatauang~vijml~vijmlvalas~vijmlbayar~vijmlbayarvalas~vitgllunas~viisclose~vicustomtext1~vicustomtext2~vicustomtext3~vicustomdbl1~vicustomdbl2~vicustomdbl3~vicustomdate1~vicustomdate2~vicustomdate3~vitglbuat~vitglexpired", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                If dtdetail.Rows.Count > 0 Then

                    'JIKA UPDATE MAKA CEK VOUCHER TSB SUDAH DIBAYAR ATAU BELUM
                    If isUpdate Then
                        Dim dtCek As New DataTable
                        For Each dr1 As DataRow In dtdetail.Rows

                            'CEK JMLBAYAR VOUCHER --------------
                            sql = "SELECT GROUP_CONCAT(si.sinotransaksi SEPARATOR ', ') as sinotransaksi FROM m_12_pos_voucher_out vo JOIN m5_si si ON vo.voidtransaksi = si.siid WHERE vo.voidvi = '" & FixQuotes(dr1("viid")) & "' GROUP BY vo.voidvi ORDER BY si.sitgl, si.sinotransaksi"
                            dtCek = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtCek.Rows.Count > 0 Then
                                If Len(FxDB(dtCek.Rows(0)("sinotransaksi"), "")) > 0 Then
                                    result(2) = "No. Voucher - " & FixQuotes(dr1("vikode")) & " has related transaction on : " & FixQuotes(dtCek.Rows(0)("sinotransaksi")) : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                            'END OF CEK JMLBAYAR VOUCHER -------

                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & FixQuotes(dr1("viid")) & "', '" & FixQuotes(dr1("vikategori")) & "', '" & FixQuotes(dr1("vicabang")) & "', '" & FixQuotes(dr1("vilokasi")) & "', '" & FixQuotes(dr1("vikode")) & "', '" & FixQuotes(dr1("vimatauang")) & "', '" & FixDouble(dr1("vijml")) & "', '" & FixDouble(dr1("vijmlvalas")) & "', '" & FixDouble(dr1("vijmlbayar")) & "', '" & FixDouble(dr1("vijmlbayarvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitgllunas"))) & "', " & dr1("viisclose") & ", '" & FixQuotes(dr1("vicustomtext1")) & "', '" & FixQuotes(dr1("vicustomtext2")) & "', '" & FixQuotes(dr1("vicustomtext3")) & "', '" & FixDouble(dr1("vicustomdbl1")) & "', '" & FixDouble(dr1("vicustomdbl2")) & "', '" & FixDouble(dr1("vicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglbuat"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglexpired"))) & "')")
                        Next

                    Else
                        For Each dr1 As DataRow In dtdetail.Rows
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & FixQuotes(dr1("viid")) & "', '" & FixQuotes(dr1("vikategori")) & "', '" & FixQuotes(dr1("vicabang")) & "', '" & FixQuotes(dr1("vilokasi")) & "', '" & FixQuotes(dr1("vikode")) & "', '" & FixQuotes(dr1("vimatauang")) & "', '" & FixDouble(dr1("vijml")) & "', '" & FixDouble(dr1("vijmlvalas")) & "', '" & FixDouble(dr1("vijmlbayar")) & "', '" & FixDouble(dr1("vijmlbayarvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitgllunas"))) & "', " & dr1("viisclose") & ", '" & FixQuotes(dr1("vicustomtext1")) & "', '" & FixQuotes(dr1("vicustomtext2")) & "', '" & FixQuotes(dr1("vicustomtext3")) & "', '" & FixDouble(dr1("vicustomdbl1")) & "', '" & FixDouble(dr1("vicustomdbl2")) & "', '" & FixDouble(dr1("vicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglbuat"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglexpired"))) & "')")
                        Next

                    End If

                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then
                        sql = "Insert into M_12_Pos_Voucher_In(viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE vikategori = VALUES(vikategori), vicabang = VALUES(vicabang), vilokasi = VALUES(vilokasi), vikode = VALUES(vikode), vimatauang = VALUES(vimatauang), vijml = VALUES(vijml), vijmlvalas = VALUES(vijmlvalas), vijmlbayar = VALUES(vijmlbayar), vijmlbayarvalas = VALUES(vijmlbayarvalas), vitgllunas = VALUES(vitgllunas), viisclose = VALUES(viisclose), vicustomtext1 = VALUES(vicustomtext1), vicustomtext2 = VALUES(vicustomtext2), vicustomtext3 = VALUES(vicustomtext3), vicustomdbl1 = VALUES(vicustomdbl1), vicustomdbl2 = VALUES(vicustomdbl2), vicustomdbl3 = VALUES(vicustomdbl3), vicustomdate1 = VALUES(vicustomdate1), vicustomdate2 = VALUES(vicustomdate2), vicustomdate3 = VALUES(vicustomdate3), vitglbuat = VALUES(vitglbuat), vitglexpired = VALUES(vitglexpired)"
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

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_VoucherSearch(PostWsSearch(paramSplit(0), "M12_Pos_VoucherSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'myconn.Close()
        'myconn = Nothing
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
    Public Function M12_Pos_VoucherDelete(ByVal param As String) As String

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

            'CEK JMLBAYAR VOUCHER --------------
            Dim dtCek As New DataTable
            sql = "SELECT GROUP_CONCAT(si.sinotransaksi SEPARATOR ', ') as sinotransaksi FROM m_12_pos_voucher_out vo JOIN m5_si si ON vo.voidtransaksi = si.siid WHERE vo.voidvi = '" & FixQuotes(idtransaksi) & "' GROUP BY vo.voidvi ORDER BY si.sitgl, si.sinotransaksi"
            dtCek = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtCek.Rows.Count > 0 Then
                If Len(FxDB(dtCek.Rows(0)("sinotransaksi"), "")) > 0 Then
                    result(2) = "This Voucher has related transaction on : " & FixQuotes(dtCek.Rows(0)("sinotransaksi")) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF CEK JMLBAYAR VOUCHER -------

            'DELETE
            sql = "DELETE FROM M_12_Pos_Voucher_In WHERE viid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
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
            Dim paramSearch As String = M12_Pos_VoucherSearch(PostWsSearch(paramSplit(0), "M12_Pos_VoucherSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_VoucherImport(ByVal param As String) As String
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
        'viid(0) As Integer, vikategori(1) As String, vicabang(2) As String, vilokasi(3) As String, vikode(4) As String, 
        'vimatauang(5) As String, vijml(6) As Double, vijmlvalas(7) As Double, vijmlbayar(8) As Double, vijmlbayarvalas(9) As Double, 
        'vitgllunas(10) As Date, viisclose(11) As Integer, vicustomtext1(12) As String, vicustomtext2(13) As String, vicustomtext3(14) As String, 
        'vicustomdbl1(15) As Double, vicustomdbl2(16) As Double, vicustomdbl3(17) As Double, vicustomdate1(18) As Date, vicustomdate2(19) As Date, 
        'vicustomdate3(20) As Date, vitglbuat(21) As Date, vitglexpired(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, 
        'vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "viid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "viisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "vicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglbuat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglexpired", AsEnumTypeData.AsString)


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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'viid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - viid required numeric." : GoTo selesai
            End If
            'vijml(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - vijml required numeric." : GoTo selesai
            End If
            'vijmlvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - vijmlvalas required numeric." : GoTo selesai
            End If
            'vijmlbayar(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayar required numeric." : GoTo selesai
            End If
            'vijmlbayarvalas(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayarvalas required numeric." : GoTo selesai
            End If
            'vitgllunas(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - vitgllunas required date." : GoTo selesai
            End If
            'viisclose(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - viisclose required numeric." : GoTo selesai
            End If
            'vicustomdbl1(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl1 required numeric." : GoTo selesai
            End If
            'vicustomdbl2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl2 required numeric." : GoTo selesai
            End If
            'vicustomdbl3(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl3 required numeric." : GoTo selesai
            End If
            'vicustomdate1(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate1 required date." : GoTo selesai
            End If
            'vicustomdate2(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate2 required date." : GoTo selesai
            End If
            'vicustomdate3(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate3 required date." : GoTo selesai
            End If
            'vitglbuat(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - vitglbuat required date." : GoTo selesai
            End If
            'vitglexpired(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - vitglexpired required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'vikategori(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - vikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - vikategori should not be more than 25 character." : GoTo selesai
            End If

            'vicabang(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - vicabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - vicabang should not be more than 25 character." : GoTo selesai
            End If

            'vilokasi(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - vilokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - vilokasi should not be more than 25 character." : GoTo selesai
            End If

            'vikode(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - vikode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 100 Then
                result(2) = "Row : " & i & " - vikode should not be more than 100 character." : GoTo selesai
            End If

            'vimatauang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - vimatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - vimatauang should not be more than 25 character." : GoTo selesai
            End If

            'vijml(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - vijml can't be empty" : GoTo selesai
            End If

            'vijmlvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - vijmlvalas can't be empty" : GoTo selesai
            End If

            'vijmlbayar(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayar can't be empty" : GoTo selesai
            End If

            'vijmlbayarvalas(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayarvalas can't be empty" : GoTo selesai
            End If

            'vitgllunas(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - vitgllunas can't be empty" : GoTo selesai
            End If

            'vicustomdbl1(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl1 can't be empty" : GoTo selesai
            End If

            'vicustomdbl2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl2 can't be empty" : GoTo selesai
            End If

            'vicustomdbl3(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl3 can't be empty" : GoTo selesai
            End If

            'vicustomdate1(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate1 can't be empty" : GoTo selesai
            End If

            'vicustomdate2(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate2 can't be empty" : GoTo selesai
            End If

            'vicustomdate3(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate3 can't be empty" : GoTo selesai
            End If


            'vitglbuat(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - vitglbuat can't be empty" : GoTo selesai
            End If

            'vitglexpired(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - vitglexpired can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "viid~vikategori~vicabang~vilokasi~vikode~vimatauang~vijml~vijmlvalas~vijmlbayar~vijmlbayarvalas~vitgllunas~viisclose~vicustomtext1~vicustomtext2~vicustomtext3~vicustomdbl1~vicustomdbl2~vicustomdbl3~vicustomdate1~vicustomdate2~vicustomdate3~vitglbuat~vitglexpired", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                If dtdetail.Rows.Count > 0 Then

                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("viid")) & "', '" & FixQuotes(dr1("vikategori")) & "', '" & FixQuotes(dr1("vicabang")) & "', '" & FixQuotes(dr1("vilokasi")) & "', '" & FixQuotes(dr1("vikode")) & "', '" & FixQuotes(dr1("vimatauang")) & "', '" & FixDouble(dr1("vijml")) & "', '" & FixDouble(dr1("vijmlvalas")) & "', '" & FixDouble(dr1("vijmlbayar")) & "', '" & FixDouble(dr1("vijmlbayarvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitgllunas"))) & "', " & dr1("viisclose") & ", '" & FixQuotes(dr1("vicustomtext1")) & "', '" & FixQuotes(dr1("vicustomtext2")) & "', '" & FixQuotes(dr1("vicustomtext3")) & "', '" & FixDouble(dr1("vicustomdbl1")) & "', '" & FixDouble(dr1("vicustomdbl2")) & "', '" & FixDouble(dr1("vicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglbuat"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglexpired"))) & "')")
                    Next

                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then

                        'DELETE
                        sql = "DELETE FROM M_12_Pos_Voucher_In"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'INSERT
                        sql = "Insert into M_12_Pos_Voucher_In(viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE vikategori = VALUES(vikategori), vicabang = VALUES(vicabang), vilokasi = VALUES(vilokasi), vikode = VALUES(vikode), vimatauang = VALUES(vimatauang), vijml = VALUES(vijml), vijmlvalas = VALUES(vijmlvalas), vijmlbayar = VALUES(vijmlbayar), vijmlbayarvalas = VALUES(vijmlbayarvalas), vitgllunas = VALUES(vitgllunas), viisclose = VALUES(viisclose), vicustomtext1 = VALUES(vicustomtext1), vicustomtext2 = VALUES(vicustomtext2), vicustomtext3 = VALUES(vicustomtext3), vicustomdbl1 = VALUES(vicustomdbl1), vicustomdbl2 = VALUES(vicustomdbl2), vicustomdbl3 = VALUES(vicustomdbl3), vicustomdate1 = VALUES(vicustomdate1), vicustomdate2 = VALUES(vicustomdate2), vicustomdate3 = VALUES(vicustomdate3), vitglbuat = VALUES(vitglbuat), vitglexpired = VALUES(vitglexpired)"
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

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_VoucherSearch(PostWsSearch(paramSplit(0), "M12_Pos_VoucherSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'myconn.Close()
        'myconn = Nothing
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
    Public Function M12_Pos_VoucherSimpanOld(ByVal param As String) As String
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
        'viid(0) As Integer, vikategori(1) As String, vicabang(2) As String, vilokasi(3) As String, vikode(4) As String, 
        'vimatauang(5) As String, vijml(6) As Double, vijmlvalas(7) As Double, vijmlbayar(8) As Double, vijmlbayarvalas(9) As Double, 
        'vitgllunas(10) As Date, viisclose(11) As Integer, vicustomtext1(12) As String, vicustomtext2(13) As String, vicustomtext3(14) As String, 
        'vicustomdbl1(15) As Double, vicustomdbl2(16) As Double, vicustomdbl3(17) As Double, vicustomdate1(18) As Date, vicustomdate2(19) As Date, 
        'vicustomdate3(20) As Date, vitglbuat(21) As Date, vitglexpired(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, 
        'vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "viid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "viisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "vicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglbuat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglexpired", AsEnumTypeData.AsString)


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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'viid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - viid required numeric." : GoTo selesai
            End If
            'vijml(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - vijml required numeric." : GoTo selesai
            End If
            'vijmlvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - vijmlvalas required numeric." : GoTo selesai
            End If
            'vijmlbayar(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayar required numeric." : GoTo selesai
            End If
            'vijmlbayarvalas(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayarvalas required numeric." : GoTo selesai
            End If
            'vitgllunas(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - vitgllunas required date." : GoTo selesai
            End If
            'viisclose(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - viisclose required numeric." : GoTo selesai
            End If
            'vicustomdbl1(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl1 required numeric." : GoTo selesai
            End If
            'vicustomdbl2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl2 required numeric." : GoTo selesai
            End If
            'vicustomdbl3(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl3 required numeric." : GoTo selesai
            End If
            'vicustomdate1(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate1 required date." : GoTo selesai
            End If
            'vicustomdate2(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate2 required date." : GoTo selesai
            End If
            'vicustomdate3(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate3 required date." : GoTo selesai
            End If
            'vitglbuat(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - vitglbuat required date." : GoTo selesai
            End If
            'vitglexpired(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - vitglexpired required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'vikategori(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - vikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - vikategori should not be more than 25 character." : GoTo selesai
            End If

            'vicabang(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - vicabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - vicabang should not be more than 25 character." : GoTo selesai
            End If

            'vilokasi(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - vilokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - vilokasi should not be more than 25 character." : GoTo selesai
            End If

            'vikode(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - vikode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 100 Then
                result(2) = "Row : " & i & " - vikode should not be more than 100 character." : GoTo selesai
            End If

            'vimatauang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - vimatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - vimatauang should not be more than 25 character." : GoTo selesai
            End If

            'vijml(6) As Double
            If Len(dataRowDetail(6)) <= 0 Then
                result(2) = "Row : " & i & " - vijml can't be empty" : GoTo selesai
            End If

            'vijmlvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - vijmlvalas can't be empty" : GoTo selesai
            End If

            'vijmlbayar(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayar can't be empty" : GoTo selesai
            End If

            'vijmlbayarvalas(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayarvalas can't be empty" : GoTo selesai
            End If

            'vitgllunas(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - vitgllunas can't be empty" : GoTo selesai
            End If

            'vicustomdbl1(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl1 can't be empty" : GoTo selesai
            End If

            'vicustomdbl2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl2 can't be empty" : GoTo selesai
            End If

            'vicustomdbl3(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl3 can't be empty" : GoTo selesai
            End If

            'vicustomdate1(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate1 can't be empty" : GoTo selesai
            End If

            'vicustomdate2(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate2 can't be empty" : GoTo selesai
            End If

            'vicustomdate3(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate3 can't be empty" : GoTo selesai
            End If


            'vitglbuat(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - vitglbuat can't be empty" : GoTo selesai
            End If

            'vitglexpired(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - vitglexpired can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "viid~vikategori~vicabang~vilokasi~vikode~vimatauang~vijml~vijmlvalas~vijmlbayar~vijmlbayarvalas~vitgllunas~viisclose~vicustomtext1~vicustomtext2~vicustomtext3~vicustomdbl1~vicustomdbl2~vicustomdbl3~vicustomdate1~vicustomdate2~vicustomdate3~vitglbuat~vitglexpired", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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

                    'JIKA UPDATE MAKA CEK VOUCHER TSB SUDAH DIBAYAR ATAU BELUM
                    If isUpdate Then
                        Dim dtCek As New DataTable
                        For Each dr1 As DataRow In dtdetail.Rows

                            'CEK JMLBAYAR VOUCHER --------------
                            sql = "SELECT GROUP_CONCAT(si.sinotransaksi SEPARATOR ', ') as sinotransaksi FROM m_12_pos_voucher_out vo JOIN m5_si si ON vo.voidtransaksi = si.siid WHERE vo.voidvi = '" & FixQuotes(dr1("viid")) & "' GROUP BY vo.voidvi ORDER BY si.sitgl, si.sinotransaksi"
                            dtCek = AsDataTableAmbilDariDB(sql)
                            If dtCek.Rows.Count > 0 Then
                                If Len(FxDB(dtCek.Rows(0)("sinotransaksi"), "")) > 0 Then
                                    result(2) = "No. Voucher - " & FixQuotes(dr1("vikode")) & " has related transaction on : " & FixQuotes(dtCek.Rows(0)("sinotransaksi")) : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                            'END OF CEK JMLBAYAR VOUCHER -------

                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & FixQuotes(dr1("viid")) & "', '" & FixQuotes(dr1("vikategori")) & "', '" & FixQuotes(dr1("vicabang")) & "', '" & FixQuotes(dr1("vilokasi")) & "', '" & FixQuotes(dr1("vikode")) & "', '" & FixQuotes(dr1("vimatauang")) & "', '" & FixDouble(dr1("vijml")) & "', '" & FixDouble(dr1("vijmlvalas")) & "', '" & FixDouble(dr1("vijmlbayar")) & "', '" & FixDouble(dr1("vijmlbayarvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitgllunas"))) & "', " & dr1("viisclose") & ", '" & FixQuotes(dr1("vicustomtext1")) & "', '" & FixQuotes(dr1("vicustomtext2")) & "', '" & FixQuotes(dr1("vicustomtext3")) & "', '" & FixDouble(dr1("vicustomdbl1")) & "', '" & FixDouble(dr1("vicustomdbl2")) & "', '" & FixDouble(dr1("vicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglbuat"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglexpired"))) & "')")
                        Next

                    Else
                        For Each dr1 As DataRow In dtdetail.Rows
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & FixQuotes(dr1("viid")) & "', '" & FixQuotes(dr1("vikategori")) & "', '" & FixQuotes(dr1("vicabang")) & "', '" & FixQuotes(dr1("vilokasi")) & "', '" & FixQuotes(dr1("vikode")) & "', '" & FixQuotes(dr1("vimatauang")) & "', '" & FixDouble(dr1("vijml")) & "', '" & FixDouble(dr1("vijmlvalas")) & "', '" & FixDouble(dr1("vijmlbayar")) & "', '" & FixDouble(dr1("vijmlbayarvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitgllunas"))) & "', " & dr1("viisclose") & ", '" & FixQuotes(dr1("vicustomtext1")) & "', '" & FixQuotes(dr1("vicustomtext2")) & "', '" & FixQuotes(dr1("vicustomtext3")) & "', '" & FixDouble(dr1("vicustomdbl1")) & "', '" & FixDouble(dr1("vicustomdbl2")) & "', '" & FixDouble(dr1("vicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglbuat"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglexpired"))) & "')")
                        Next

                    End If

                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then
                        sql = "Insert into M_12_Pos_Voucher_In(viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE vikategori = VALUES(vikategori), vicabang = VALUES(vicabang), vilokasi = VALUES(vilokasi), vikode = VALUES(vikode), vimatauang = VALUES(vimatauang), vijml = VALUES(vijml), vijmlvalas = VALUES(vijmlvalas), vijmlbayar = VALUES(vijmlbayar), vijmlbayarvalas = VALUES(vijmlbayarvalas), vitgllunas = VALUES(vitgllunas), viisclose = VALUES(viisclose), vicustomtext1 = VALUES(vicustomtext1), vicustomtext2 = VALUES(vicustomtext2), vicustomtext3 = VALUES(vicustomtext3), vicustomdbl1 = VALUES(vicustomdbl1), vicustomdbl2 = VALUES(vicustomdbl2), vicustomdbl3 = VALUES(vicustomdbl3), vicustomdate1 = VALUES(vicustomdate1), vicustomdate2 = VALUES(vicustomdate2), vicustomdate3 = VALUES(vicustomdate3), vitglbuat = VALUES(vitglbuat), vitglexpired = VALUES(vitglexpired)"
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
            Dim paramSearch As String = M12_Pos_VoucherSearch(PostWsSearch(paramSplit(0), "M12_Pos_VoucherSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_VoucherDeleteOld(ByVal param As String) As String

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

            'CEK JMLBAYAR VOUCHER --------------
            Dim dtCek As New DataTable
            sql = "SELECT GROUP_CONCAT(si.sinotransaksi SEPARATOR ', ') as sinotransaksi FROM m_12_pos_voucher_out vo JOIN m5_si si ON vo.voidtransaksi = si.siid WHERE vo.voidvi = '" & FixQuotes(idtransaksi) & "' GROUP BY vo.voidvi ORDER BY si.sitgl, si.sinotransaksi"
            dtCek = AsDataTableAmbilDariDB(sql)
            If dtCek.Rows.Count > 0 Then
                If Len(FxDB(dtCek.Rows(0)("sinotransaksi"), "")) > 0 Then
                    result(2) = "This Voucher has related transaction on : " & FixQuotes(dtCek.Rows(0)("sinotransaksi")) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF CEK JMLBAYAR VOUCHER -------

            'DELETE
            sql = "DELETE FROM M_12_Pos_Voucher_In WHERE viid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Pos_VoucherSearch(PostWsSearch(paramSplit(0), "M12_Pos_VoucherSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_VoucherImportOld(ByVal param As String) As String
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
        'viid(0) As Integer, vikategori(1) As String, vicabang(2) As String, vilokasi(3) As String, vikode(4) As String, 
        'vimatauang(5) As String, vijml(6) As Double, vijmlvalas(7) As Double, vijmlbayar(8) As Double, vijmlbayarvalas(9) As Double, 
        'vitgllunas(10) As Date, viisclose(11) As Integer, vicustomtext1(12) As String, vicustomtext2(13) As String, vicustomtext3(14) As String, 
        'vicustomdbl1(15) As Double, vicustomdbl2(16) As Double, vicustomdbl3(17) As Double, vicustomdate1(18) As Date, vicustomdate2(19) As Date, 
        'vicustomdate3(20) As Date, vitglbuat(21) As Date, vitglexpired(22) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, 
        'vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "viid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vikode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vijmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "viisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "vicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vicustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglbuat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "vitglexpired", AsEnumTypeData.AsString)


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
            If (dataRowDetail.Length <> 23) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'viid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - viid required numeric." : GoTo selesai
            End If
            'vijml(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - vijml required numeric." : GoTo selesai
            End If
            'vijmlvalas(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - vijmlvalas required numeric." : GoTo selesai
            End If
            'vijmlbayar(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayar required numeric." : GoTo selesai
            End If
            'vijmlbayarvalas(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - vijmlbayarvalas required numeric." : GoTo selesai
            End If
            'vitgllunas(10) As Date
            If (IsDate(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - vitgllunas required date." : GoTo selesai
            End If
            'viisclose(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - viisclose required numeric." : GoTo selesai
            End If
            'vicustomdbl1(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl1 required numeric." : GoTo selesai
            End If
            'vicustomdbl2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl2 required numeric." : GoTo selesai
            End If
            'vicustomdbl3(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - vicustomdbl3 required numeric." : GoTo selesai
            End If
            'vicustomdate1(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate1 required date." : GoTo selesai
            End If
            'vicustomdate2(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate2 required date." : GoTo selesai
            End If
            'vicustomdate3(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - vicustomdate3 required date." : GoTo selesai
            End If
            'vitglbuat(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - vitglbuat required date." : GoTo selesai
            End If
            'vitglexpired(22) As Date
            If (IsDate(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - vitglexpired required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'vikategori(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - vikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - vikategori should not be more than 25 character." : GoTo selesai
            End If

            'vicabang(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - vicabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - vicabang should not be more than 25 character." : GoTo selesai
            End If

            'vilokasi(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - vilokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - vilokasi should not be more than 25 character." : GoTo selesai
            End If

            'vikode(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - vikode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 100 Then
                result(2) = "Row : " & i & " - vikode should not be more than 100 character." : GoTo selesai
            End If

            'vimatauang(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - vimatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - vimatauang should not be more than 25 character." : GoTo selesai
            End If

            'vijml(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - vijml can't be empty" : GoTo selesai
            End If

            'vijmlvalas(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - vijmlvalas can't be empty" : GoTo selesai
            End If

            'vijmlbayar(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayar can't be empty" : GoTo selesai
            End If

            'vijmlbayarvalas(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - vijmlbayarvalas can't be empty" : GoTo selesai
            End If

            'vitgllunas(10) As Date
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - vitgllunas can't be empty" : GoTo selesai
            End If

            'vicustomdbl1(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl1 can't be empty" : GoTo selesai
            End If

            'vicustomdbl2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl2 can't be empty" : GoTo selesai
            End If

            'vicustomdbl3(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdbl3 can't be empty" : GoTo selesai
            End If

            'vicustomdate1(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate1 can't be empty" : GoTo selesai
            End If

            'vicustomdate2(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate2 can't be empty" : GoTo selesai
            End If

            'vicustomdate3(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - vicustomdate3 can't be empty" : GoTo selesai
            End If


            'vitglbuat(21) As Date
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - vitglbuat can't be empty" : GoTo selesai
            End If

            'vitglexpired(22) As Date
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - vitglexpired can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "viid~vikategori~vicabang~vilokasi~vikode~vimatauang~vijml~vijmlvalas~vijmlbayar~vijmlbayarvalas~vitgllunas~viisclose~vicustomtext1~vicustomtext2~vicustomtext3~vicustomdbl1~vicustomdbl2~vicustomdbl3~vicustomdate1~vicustomdate2~vicustomdate3~vitglbuat~vitglexpired", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22)) = False Then
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
                        strValue2.Append("('" & FixQuotes(dr1("viid")) & "', '" & FixQuotes(dr1("vikategori")) & "', '" & FixQuotes(dr1("vicabang")) & "', '" & FixQuotes(dr1("vilokasi")) & "', '" & FixQuotes(dr1("vikode")) & "', '" & FixQuotes(dr1("vimatauang")) & "', '" & FixDouble(dr1("vijml")) & "', '" & FixDouble(dr1("vijmlvalas")) & "', '" & FixDouble(dr1("vijmlbayar")) & "', '" & FixDouble(dr1("vijmlbayarvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitgllunas"))) & "', " & dr1("viisclose") & ", '" & FixQuotes(dr1("vicustomtext1")) & "', '" & FixQuotes(dr1("vicustomtext2")) & "', '" & FixQuotes(dr1("vicustomtext3")) & "', '" & FixDouble(dr1("vicustomdbl1")) & "', '" & FixDouble(dr1("vicustomdbl2")) & "', '" & FixDouble(dr1("vicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vicustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglbuat"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("vitglexpired"))) & "')")
                    Next

                    'insert jika data belum ada, dan update jika data sudah ada
                    If Len(strValue2.ToString) > 0 Then

                        'DELETE
                        sql = "DELETE FROM M_12_Pos_Voucher_In"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'INSERT
                        sql = "Insert into M_12_Pos_Voucher_In(viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, vitglbuat, vitglexpired) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE vikategori = VALUES(vikategori), vicabang = VALUES(vicabang), vilokasi = VALUES(vilokasi), vikode = VALUES(vikode), vimatauang = VALUES(vimatauang), vijml = VALUES(vijml), vijmlvalas = VALUES(vijmlvalas), vijmlbayar = VALUES(vijmlbayar), vijmlbayarvalas = VALUES(vijmlbayarvalas), vitgllunas = VALUES(vitgllunas), viisclose = VALUES(viisclose), vicustomtext1 = VALUES(vicustomtext1), vicustomtext2 = VALUES(vicustomtext2), vicustomtext3 = VALUES(vicustomtext3), vicustomdbl1 = VALUES(vicustomdbl1), vicustomdbl2 = VALUES(vicustomdbl2), vicustomdbl3 = VALUES(vicustomdbl3), vicustomdate1 = VALUES(vicustomdate1), vicustomdate2 = VALUES(vicustomdate2), vicustomdate3 = VALUES(vicustomdate3), vitglbuat = VALUES(vitglbuat), vitglexpired = VALUES(vitglexpired)"
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
            Dim paramSearch As String = M12_Pos_VoucherSearch(PostWsSearch(paramSplit(0), "M12_Pos_VoucherSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_VoucherSearch(ByVal param As String) As String
        'M12_Pos_VoucherSearch --------------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, 
        'vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, 
        'pcnama, bnama, lnama, viisclosenama, vitglbuat, vitglexpired, vijmlsisa, vijmlsisavalas

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
            Filter = pagingSplit(2) & " AND (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN lc.lkode LIKE '%' OR lc.lkode IS NULL ELSE lc.lkode = uloc.lokasi END)"
            '#Taruh fungsi replace disini...
        Else
            Filter = " (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN lc.lkode LIKE '%' OR lc.lkode IS NULL ELSE lc.lkode = uloc.lokasi END)"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'sql = "select vi.viid AS viid, vi.vikategori AS vikategori, vi.vicabang AS vicabang, vi.vilokasi AS vilokasi, vi.vikode AS vikode, vi.vimatauang AS vimatauang, vi.vijml AS vijml, vi.vijmlvalas AS vijmlvalas, vi.vijmlbayar AS vijmlbayar, vi.vijmlbayarvalas AS vijmlbayarvalas, vi.vijml - vi.vijmlbayar as vijmlsisa, vi.vijmlvalas - vi.vijmlbayarvalas as vijmlsisavalas, vi.vitgllunas AS vitgllunas, vi.viisclose AS viisclose, vi.vicustomtext1 AS vicustomtext1, vi.vicustomtext2 AS vicustomtext2, vi.vicustomtext3 AS vicustomtext3, vi.vicustomdbl1 AS vicustomdbl1, vi.vicustomdbl2 AS vicustomdbl2, vi.vicustomdbl3 AS vicustomdbl3, vi.vicustomdate1 AS vicustomdate1, vi.vicustomdate2 AS vicustomdate2, vi.vicustomdate3 AS vicustomdate3, pc.pcnama AS pcnama, br.bnama AS bnama, lc.lnama AS lnama, (case vi.viisclose when 1 then 'Close' else 'Available' end) AS viisclosenama, vi.vitglbuat AS vitglbuat, vi.vitglexpired AS vitglexpired from m_12_pos_voucher_in vi join m_12_pos_category pc on vi.vikategori = pc.pckode join m1_branch br on vi.vicabang = br.bkode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_location uloc on ul.uluser = uloc.userid join m1_location lc on uloc.lokasi = lc.lkode and vi.vikategori = lc.lkategoripos"
        sql = "select vi.viid AS viid, vi.vikategori AS vikategori, vi.vicabang AS vicabang, vi.vilokasi AS vilokasi, vi.vikode AS vikode, vi.vimatauang AS vimatauang, vi.vijml AS vijml, vi.vijmlvalas AS vijmlvalas, vi.vijmlbayar AS vijmlbayar, vi.vijmlbayarvalas AS vijmlbayarvalas, vi.vijml - vi.vijmlbayar as vijmlsisa, vi.vijmlvalas - vi.vijmlbayarvalas as vijmlsisavalas, vi.vitgllunas AS vitgllunas, vi.viisclose AS viisclose, vi.vicustomtext1 AS vicustomtext1, vi.vicustomtext2 AS vicustomtext2, vi.vicustomtext3 AS vicustomtext3, vi.vicustomdbl1 AS vicustomdbl1, vi.vicustomdbl2 AS vicustomdbl2, vi.vicustomdbl3 AS vicustomdbl3, vi.vicustomdate1 AS vicustomdate1, vi.vicustomdate2 AS vicustomdate2, vi.vicustomdate3 AS vicustomdate3, pc.pcnama AS pcnama, br.bnama AS bnama, lc.lnama AS lnama, (case vi.viisclose when 1 then 'Close' else 'Available' end) AS viisclosenama, vi.vitglbuat AS vitglbuat, vi.vitglexpired AS vitglexpired from m_12_pos_voucher_in vi join m_12_pos_category pc on vi.vikategori = pc.pckode join m1_branch br on vi.vicabang = br.bkode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' left join m0_user_location uloc on ul.uluser = uloc.userid left join m1_location lc on uloc.lokasi = lc.lkode and vi.vikategori = lc.lkategoripos"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Voucher", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "vi.viid", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("viid"), ""), sptField,
                     FxDB(dr("vikategori"), ""), sptField,
                     FxDB(dr("vicabang"), ""), sptField,
                     FxDB(dr("vilokasi"), ""), sptField,
                     FxDB(dr("vikode"), ""), sptField,
                     FxDB(dr("vimatauang"), ""), sptField,
                     FxDB(dr("vijml"), 0), sptField,
                     FxDB(dr("vijmlvalas"), 0), sptField,
                     FxDB(dr("vijmlbayar"), 0), sptField,
                     FxDB(dr("vijmlbayarvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vitgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("viisclose"), 0), sptField,
                     FxDB(dr("vicustomtext1"), ""), sptField,
                     FxDB(dr("vicustomtext2"), ""), sptField,
                     FxDB(dr("vicustomtext3"), ""), sptField,
                     FxDB(dr("vicustomdbl1"), 0), sptField,
                     FxDB(dr("vicustomdbl2"), 0), sptField,
                     FxDB(dr("vicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("lnama"), ""), sptField,
                     FxDB(dr("viisclosenama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vitglbuat"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vitglexpired"), ""), formatTgl), sptField,
                     FxDB(dr("vijmlsisa"), 0), sptField,
                     FxDB(dr("vijmlsisavalas"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Voucher data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, pcnama, bnama, lnama, viisclosenama, vitglbuat, vitglexpired, vijmlsisa, vijmlsisavalas"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_VoucherSearchOld(ByVal param As String) As String
        'M12_Pos_VoucherSearch --------------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, 
        'vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, 
        'pcnama, bnama, lnama, viisclosenama, vitglbuat, vitglexpired, vijmlsisa, vijmlsisavalas

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
        sql = "select vi.viid AS viid, vi.vikategori AS vikategori, vi.vicabang AS vicabang, vi.vilokasi AS vilokasi, vi.vikode AS vikode, vi.vimatauang AS vimatauang, vi.vijml AS vijml, vi.vijmlvalas AS vijmlvalas, vi.vijmlbayar AS vijmlbayar, vi.vijmlbayarvalas AS vijmlbayarvalas, vi.vijml - vi.vijmlbayar as vijmlsisa, vi.vijmlvalas - vi.vijmlbayarvalas as vijmlsisavalas, vi.vitgllunas AS vitgllunas, vi.viisclose AS viisclose, vi.vicustomtext1 AS vicustomtext1, vi.vicustomtext2 AS vicustomtext2, vi.vicustomtext3 AS vicustomtext3, vi.vicustomdbl1 AS vicustomdbl1, vi.vicustomdbl2 AS vicustomdbl2, vi.vicustomdbl3 AS vicustomdbl3, vi.vicustomdate1 AS vicustomdate1, vi.vicustomdate2 AS vicustomdate2, vi.vicustomdate3 AS vicustomdate3, pc.pcnama AS pcnama, br.bnama AS bnama, lc.lnama AS lnama, (case vi.viisclose when 1 then 'Close' else 'Available' end) AS viisclosenama, vi.vitglbuat AS vitglbuat, vi.vitglexpired AS vitglexpired from m_12_pos_voucher_in vi join m_12_pos_category pc on vi.vikategori = pc.pckode join m1_branch br on vi.vicabang = br.bkode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_location uloc on ul.uluser = uloc.userid join m1_location lc on uloc.lokasi = lc.lkode and vi.vikategori = lc.lkategoripos"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Voucher", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "vi.viid", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("viid"), ""), sptField,
                     FxDB(dr("vikategori"), ""), sptField,
                     FxDB(dr("vicabang"), ""), sptField,
                     FxDB(dr("vilokasi"), ""), sptField,
                     FxDB(dr("vikode"), ""), sptField,
                     FxDB(dr("vimatauang"), ""), sptField,
                     FxDB(dr("vijml"), 0), sptField,
                     FxDB(dr("vijmlvalas"), 0), sptField,
                     FxDB(dr("vijmlbayar"), 0), sptField,
                     FxDB(dr("vijmlbayarvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vitgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("viisclose"), 0), sptField,
                     FxDB(dr("vicustomtext1"), ""), sptField,
                     FxDB(dr("vicustomtext2"), ""), sptField,
                     FxDB(dr("vicustomtext3"), ""), sptField,
                     FxDB(dr("vicustomdbl1"), 0), sptField,
                     FxDB(dr("vicustomdbl2"), 0), sptField,
                     FxDB(dr("vicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("lnama"), ""), sptField,
                     FxDB(dr("viisclosenama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vitglbuat"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vitglexpired"), ""), formatTgl), sptField,
                     FxDB(dr("vijmlsisa"), 0), sptField,
                     FxDB(dr("vijmlsisavalas"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Voucher data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3, pcnama, bnama, lnama, viisclosenama, vitglbuat, vitglexpired, vijmlsisa, vijmlsisavalas"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_VoucherDownload(ByVal param As String) As String
        'M12_Pos_VoucherDownload --------------------------------------------------------
        'viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, 
        'vijmlvalas, vijmlbayar, vijmlbayarvalas, vitglbuat, vitglexpired, vitgllunas, viisclose, 
        'vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, 
        'vicustomdate2, vicustomdate3

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

        dt = AmbilData("aplikasi1-M_12_Pos_Voucher", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("viid"), ""), sptField,
                     FxDB(dr("vikategori"), ""), sptField,
                     FxDB(dr("vicabang"), ""), sptField,
                     FxDB(dr("vilokasi"), ""), sptField,
                     FxDB(dr("vikode"), ""), sptField,
                     FxDB(dr("vimatauang"), ""), sptField,
                     FxDB(dr("vijml"), 0), sptField,
                     FxDB(dr("vijmlvalas"), 0), sptField,
                     FxDB(dr("vijmlbayar"), 0), sptField,
                     FxDB(dr("vijmlbayarvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vitglbuat"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vitglexpired"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vitgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("viisclose"), 0), sptField,
                     FxDB(dr("vicustomtext1"), ""), sptField,
                     FxDB(dr("vicustomtext2"), ""), sptField,
                     FxDB(dr("vicustomtext3"), ""), sptField,
                     FxDB(dr("vicustomdbl1"), 0), sptField,
                     FxDB(dr("vicustomdbl2"), 0), sptField,
                     FxDB(dr("vicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("vicustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Voucher data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("viid, vikategori, vicabang, vilokasi, vikode, vimatauang, vijml, vijmlvalas, vijmlbayar, vijmlbayarvalas, vitglbuat, vitglexpired, vitgllunas, viisclose, vicustomtext1, vicustomtext2, vicustomtext3, vicustomdbl1, vicustomdbl2, vicustomdbl3, vicustomdate1, vicustomdate2, vicustomdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_VoucherCode(ByVal param As String) As String
        'M12_Pos_VoucherCode --------------------------------------------------------
        'kode

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

        If (IsNumeric(pagingSplit(2)) = False) Then
            result(2) = "filter required numeric." : GoTo selesai
        End If

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Dim validchars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim validnumbers As String = "1234567890"
        Dim randomChar As Char = "", hasil As String = ""
        Dim idx As Integer = 0

        Dim sb As New StringBuilder()
        Dim rand As New Random()

        'If dt.Rows.Count > 0 Then
        For i As Integer = 1 To pagingSplit(0)

            sb.Clear()

            For j As Integer = 1 To pagingSplit(1)
                idx = rand.Next(0, validchars.Length)
                randomChar = validchars(idx)
                sb.Append(randomChar)
            Next j

            For k As Integer = 1 To pagingSplit(2)
                idx = rand.Next(0, validnumbers.Length)
                randomChar = validnumbers(idx)
                sb.Append(randomChar)
            Next k

            search = String.Concat(search, sb.ToString, sptRow)
        Next
        search = search.Substring(0, IIf(search.Length > 0, search.Length - sptRow.Length, search.Length))

        result(1) = 1
        resultPaging(0) = Math.Abs(Val(pg1.isPaging))
        resultPaging(1) = Math.Abs(Val(pg1.isNext))
        resultPaging(2) = Math.Abs(Val(pg1.isPrev))
        resultPaging(3) = pg1.countPage
        resultPaging(4) = pg1.countRow
        'Else
        '    result(2) = "Code Voucher data not found."
        'End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kode"))

        Return wsResult
    End Function

End Class
