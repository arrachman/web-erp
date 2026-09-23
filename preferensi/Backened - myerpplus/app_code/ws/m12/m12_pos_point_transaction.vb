Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_point_transaction
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Point_TransactionSimpan(ByVal param As String) As String
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

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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
        'ptkategori(0) As String, ptoperator(1) As String, ptjml1(2) As Double, ptjml2(3) As Double, ptjmlpoint(4) As Double, 
        'ptcustomtext1(5) As String, ptcustomtext2(6) As String, ptcustomtext3(7) As String, ptcustomtext4(8) As String, ptcustomtext5(9) As String, 
        'ptcustomint1(10) As Integer, ptcustomint2(11) As Integer, ptcustomint3(12) As Integer, ptcustomdbl1(13) As Double, ptcustomdbl2(14) As Double, 
        'ptcustomdbl3(15) As Double, ptcustomdate1(16) As Date, ptcustomdate2(17) As Date, ptcustomdate3(18) As Date
        'pttgl1(19) As Date, pttgl2(20) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, 
        'ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, 
        'ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, pttgl1, pttgl2

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ptkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pttgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pttgl2", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 21) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'ptjml1(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - ptjml1 required numeric." : GoTo selesai
            End If
            'ptjml2(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - ptjml2 required numeric." : GoTo selesai
            End If
            'ptjmlpoint(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - ptjmlpoint required numeric." : GoTo selesai
            End If
            'ptcustomint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint1 required numeric." : GoTo selesai
            End If
            'ptcustomint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint2 required numeric." : GoTo selesai
            End If
            'ptcustomint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint3 required numeric." : GoTo selesai
            End If
            'ptcustomdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl1 required numeric." : GoTo selesai
            End If
            'ptcustomdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl2 required numeric." : GoTo selesai
            End If
            'ptcustomdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl3 required numeric." : GoTo selesai
            End If
            'ptcustomdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate1 required date." : GoTo selesai
            End If
            'ptcustomdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate2 required date." : GoTo selesai
            End If
            'ptcustomdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate3 required date." : GoTo selesai
            End If
            'pttgl1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pttgl1 required date." : GoTo selesai
            End If
            'pttgl2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - pttgl2 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ptkategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ptkategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ptkategori should not be more than 25 character." : GoTo selesai
            End If

            'ptoperator(1) As String
            If IsNumeric(dataRowDetail(1)) = False Then
                result(2) = "Row : " & i & " - ptoperator can't be empty" : GoTo selesai
            ElseIf dataRowDetail(1) <> 0 And dataRowDetail(1) <> 1 And dataRowDetail(1) <> 2 Then
                result(2) = "Row : " & i & " - invalid ptoperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - ptoperator should not be more than 25 character." : GoTo selesai
            End If

            'ptjml1(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - ptjml1 can't be empty" : GoTo selesai
            End If

            'ptjml2(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - ptjml2 can't be empty" : GoTo selesai
            End If

            'ptjmlpoint(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - ptjmlpoint can't be empty" : GoTo selesai
            End If

            'ptcustomdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl1 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl2 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl3 can't be empty" : GoTo selesai
            End If

            'ptcustomdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate1 can't be empty" : GoTo selesai
            End If

            'ptcustomdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate2 can't be empty" : GoTo selesai
            End If

            'ptcustomdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate3 can't be empty" : GoTo selesai
            End If

            'pttgl1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pttgl1 can't be empty" : GoTo selesai
            End If

            'pttgl2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pttgl2 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ptkategori~ptoperator~ptjml1~ptjml2~ptjmlpoint~ptcustomtext1~ptcustomtext2~ptcustomtext3~ptcustomtext4~ptcustomtext5~ptcustomint1~ptcustomint2~ptcustomint3~ptcustomdbl1~ptcustomdbl2~ptcustomdbl3~ptcustomdate1~ptcustomdate2~ptcustomdate3~pttgl1~pttgl2", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim drutama As DataRow = dtdetail.Rows(0)

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("ptkategori")) & "' "

                'HAPUS DATA KATEGORI YANG SAMA
                sql = "DELETE FROM m_12_pos_point_transaction WHERE ptkategori = '" & FixQuotes(drutama("ptkategori")) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'INSERT
                Dim strValue2 As New StringBuilder
                Dim dtOperator As New DataTable
                Dim vOperator As String = ""
                For Each dr1 As DataRow In dtdetail.Rows
                    'CEK OPERATOR :
                    'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                    '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                    'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                    sql = "SELECT pt.ptkategori as kategori, pt.ptoperator as operator, (CASE pt.ptoperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_point_transaction pt WHERE pt.ptkategori = '" & FxDB(dr1("ptkategori"), "") & "' GROUP BY pt.ptoperator ORDER BY pt.ptoperator"
                    dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "POS Category : " & FxDB(dr2("kategori"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("ptoperator") = 2 Or (vOperator = 1 And dr1("ptoperator") = vOperator) Then
                                        result(2) = "POS Category : " & FxDB(dr2("kategori"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("ptkategori")) & "', '" & FixQuotes(dr1("ptoperator")) & "', '" & FixDouble(dr1("ptjml1")) & "', '" & FixDouble(dr1("ptjml2")) & "', '" & FixDouble(dr1("ptjmlpoint")) & "', '" & FixQuotes(dr1("ptcustomtext1")) & "', '" & FixQuotes(dr1("ptcustomtext2")) & "', '" & FixQuotes(dr1("ptcustomtext3")) & "', '" & FixQuotes(dr1("ptcustomtext4")) & "', '" & FixQuotes(dr1("ptcustomtext5")) & "', " & dr1("ptcustomint1") & ", " & dr1("ptcustomint2") & ", " & dr1("ptcustomint3") & ", '" & FixDouble(dr1("ptcustomdbl1")) & "', '" & FixDouble(dr1("ptcustomdbl2")) & "', '" & FixDouble(dr1("ptcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pttgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pttgl2"))) & "')")

                    sql = "Insert into M_12_Pos_Point_Transaction(ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, pttgl1, pttgl2) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next

            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_Point_TransactionSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_TransactionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
    Public Function M12_Pos_Point_TransactionDelete(ByVal param As String) As String

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

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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
        Dim ptkategori As String = "", ptoperator As String = "", ptjml1 As String = "", ptjml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 4) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK ptkategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "ptkategori can't be empty." : GoTo selesai
            Else
                ptkategori = idtrans(0)
            End If
            'CEK ptoperator
            If (Len(idtrans(1)) = 0) Then
                result(2) = "ptoperator can't be empty." : GoTo selesai
            Else
                ptoperator = idtrans(1)
            End If
            'CEK ptjml1
            If (IsNumeric(idtrans(2)) = False) Then
                result(2) = "ptjml1 required numeric." : GoTo selesai
            Else
                ptjml1 = idtrans(2)
            End If
            'CEK ptjml2
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "ptjml2 required numeric." : GoTo selesai
            Else
                ptjml2 = idtrans(3)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
            sql = "SELECT ptkategori as kategoripos FROM M_12_Pos_Point_Transaction WHERE ptkategori = '" & ptkategori & "' AND ptoperator = '" & ptoperator & "' AND ptjml1 = '" & ptjml1 & "' AND ptjml2 = '" & ptjml2 & "' GROUP BY ptkategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Point_Transaction WHERE ptkategori = '" & ptkategori & "' AND ptoperator = '" & ptoperator & "' AND ptjml1 = '" & ptjml1 & "' AND ptjml2 = '" & ptjml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Point_TransactionSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_TransactionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
    Public Function M12_Pos_Point_TransactionImport(ByVal param As String) As String
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
        'ptkategori(0) As String, ptoperator(1) As String, ptjml1(2) As Double, ptjml2(3) As Double, ptjmlpoint(4) As Double, 
        'ptcustomtext1(5) As String, ptcustomtext2(6) As String, ptcustomtext3(7) As String, ptcustomtext4(8) As String, ptcustomtext5(9) As String, 
        'ptcustomint1(10) As Integer, ptcustomint2(11) As Integer, ptcustomint3(12) As Integer, ptcustomdbl1(13) As Double, ptcustomdbl2(14) As Double, 
        'ptcustomdbl3(15) As Double, ptcustomdate1(16) As Date, ptcustomdate2(17) As Date, ptcustomdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, 
        'ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, 
        'ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ptkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'ptjml1(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - ptjml1 required numeric." : GoTo selesai
            End If
            'ptjml2(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - ptjml2 required numeric." : GoTo selesai
            End If
            'ptjmlpoint(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - ptjmlpoint required numeric." : GoTo selesai
            End If
            'ptcustomint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint1 required numeric." : GoTo selesai
            End If
            'ptcustomint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint2 required numeric." : GoTo selesai
            End If
            'ptcustomint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint3 required numeric." : GoTo selesai
            End If
            'ptcustomdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl1 required numeric." : GoTo selesai
            End If
            'ptcustomdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl2 required numeric." : GoTo selesai
            End If
            'ptcustomdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl3 required numeric." : GoTo selesai
            End If
            'ptcustomdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate1 required date." : GoTo selesai
            End If
            'ptcustomdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate2 required date." : GoTo selesai
            End If
            'ptcustomdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ptkategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ptkategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ptkategori should not be more than 25 character." : GoTo selesai
            End If

            'ptoperator(1) As String
            If IsNumeric(dataRowDetail(1)) = False Then
                result(2) = "Row : " & i & " - ptoperator can't be empty" : GoTo selesai
            ElseIf dataRowDetail(1) <> 0 And dataRowDetail(1) <> 1 And dataRowDetail(1) <> 2 Then
                result(2) = "Row : " & i & " - invalid ptoperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - ptoperator should not be more than 25 character." : GoTo selesai
            End If

            'ptjml1(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - ptjml1 can't be empty" : GoTo selesai
            End If

            'ptjml2(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - ptjml2 can't be empty" : GoTo selesai
            End If

            'ptjmlpoint(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - ptjmlpoint can't be empty" : GoTo selesai
            End If

            'ptcustomdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl1 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl2 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl3 can't be empty" : GoTo selesai
            End If

            'ptcustomdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate1 can't be empty" : GoTo selesai
            End If

            'ptcustomdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate2 can't be empty" : GoTo selesai
            End If

            'ptcustomdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ptkategori~ptoperator~ptjml1~ptjml2~ptjmlpoint~ptcustomtext1~ptcustomtext2~ptcustomtext3~ptcustomtext4~ptcustomtext5~ptcustomint1~ptcustomint2~ptcustomint3~ptcustomdbl1~ptcustomdbl2~ptcustomdbl3~ptcustomdate1~ptcustomdate2~ptcustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
            'Proses detail
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtdetail.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("ptkategori")) & "', '" & FixQuotes(dr1("ptoperator")) & "', '" & FixDouble(dr1("ptjml1")) & "', '" & FixDouble(dr1("ptjml2")) & "', '" & FixDouble(dr1("ptjmlpoint")) & "', '" & FixQuotes(dr1("ptcustomtext1")) & "', '" & FixQuotes(dr1("ptcustomtext2")) & "', '" & FixQuotes(dr1("ptcustomtext3")) & "', '" & FixQuotes(dr1("ptcustomtext4")) & "', '" & FixQuotes(dr1("ptcustomtext5")) & "', " & dr1("ptcustomint1") & ", " & dr1("ptcustomint2") & ", " & dr1("ptcustomint3") & ", '" & FixDouble(dr1("ptcustomdbl1")) & "', '" & FixDouble(dr1("ptcustomdbl2")) & "', '" & FixDouble(dr1("ptcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Point_Transaction"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Point_Transaction(ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3) values" & strValue2.ToString & ""
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
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_Point_TransactionSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_TransactionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_TransactionSimpanOld(ByVal param As String) As String
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

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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
        'ptkategori(0) As String, ptoperator(1) As String, ptjml1(2) As Double, ptjml2(3) As Double, ptjmlpoint(4) As Double, 
        'ptcustomtext1(5) As String, ptcustomtext2(6) As String, ptcustomtext3(7) As String, ptcustomtext4(8) As String, ptcustomtext5(9) As String, 
        'ptcustomint1(10) As Integer, ptcustomint2(11) As Integer, ptcustomint3(12) As Integer, ptcustomdbl1(13) As Double, ptcustomdbl2(14) As Double, 
        'ptcustomdbl3(15) As Double, ptcustomdate1(16) As Date, ptcustomdate2(17) As Date, ptcustomdate3(18) As Date
        'pttgl1(19) As Date, pttgl2(20) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, 
        'ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, 
        'ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, pttgl1, pttgl2

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ptkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pttgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pttgl2", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 21) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'ptjml1(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - ptjml1 required numeric." : GoTo selesai
            End If
            'ptjml2(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - ptjml2 required numeric." : GoTo selesai
            End If
            'ptjmlpoint(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - ptjmlpoint required numeric." : GoTo selesai
            End If
            'ptcustomint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint1 required numeric." : GoTo selesai
            End If
            'ptcustomint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint2 required numeric." : GoTo selesai
            End If
            'ptcustomint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint3 required numeric." : GoTo selesai
            End If
            'ptcustomdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl1 required numeric." : GoTo selesai
            End If
            'ptcustomdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl2 required numeric." : GoTo selesai
            End If
            'ptcustomdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl3 required numeric." : GoTo selesai
            End If
            'ptcustomdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate1 required date." : GoTo selesai
            End If
            'ptcustomdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate2 required date." : GoTo selesai
            End If
            'ptcustomdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate3 required date." : GoTo selesai
            End If
            'pttgl1(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pttgl1 required date." : GoTo selesai
            End If
            'pttgl2(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - pttgl2 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ptkategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ptkategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ptkategori should not be more than 25 character." : GoTo selesai
            End If

            'ptoperator(1) As String
            If IsNumeric(dataRowDetail(1)) = False Then
                result(2) = "Row : " & i & " - ptoperator can't be empty" : GoTo selesai
            ElseIf dataRowDetail(1) <> 0 And dataRowDetail(1) <> 1 And dataRowDetail(1) <> 2 Then
                result(2) = "Row : " & i & " - invalid ptoperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - ptoperator should not be more than 25 character." : GoTo selesai
            End If

            'ptjml1(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - ptjml1 can't be empty" : GoTo selesai
            End If

            'ptjml2(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - ptjml2 can't be empty" : GoTo selesai
            End If

            'ptjmlpoint(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - ptjmlpoint can't be empty" : GoTo selesai
            End If

            'ptcustomdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl1 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl2 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl3 can't be empty" : GoTo selesai
            End If

            'ptcustomdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate1 can't be empty" : GoTo selesai
            End If

            'ptcustomdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate2 can't be empty" : GoTo selesai
            End If

            'ptcustomdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate3 can't be empty" : GoTo selesai
            End If

            'pttgl1(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pttgl1 can't be empty" : GoTo selesai
            End If

            'pttgl2(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pttgl2 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ptkategori~ptoperator~ptjml1~ptjml2~ptjmlpoint~ptcustomtext1~ptcustomtext2~ptcustomtext3~ptcustomtext4~ptcustomtext5~ptcustomint1~ptcustomint2~ptcustomint3~ptcustomdbl1~ptcustomdbl2~ptcustomdbl3~ptcustomdate1~ptcustomdate2~ptcustomdate3~pttgl1~pttgl2", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20)) = False Then
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
                Dim drutama As DataRow = dtdetail.Rows(0)

                'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("ptkategori")) & "' "

                'HAPUS DATA KATEGORI YANG SAMA
                sql = "DELETE FROM m_12_pos_point_transaction WHERE ptkategori = '" & FixQuotes(drutama("ptkategori")) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'INSERT
                Dim strValue2 As New StringBuilder
                Dim dtOperator As New DataTable
                Dim vOperator As String = ""
                For Each dr1 As DataRow In dtdetail.Rows
                    'CEK OPERATOR :
                    'JIKA BETWEEN (0) ATAU >= (1) MAKA BOLEH LEBIH DARI SATU KONDISI 
                    '=> BEBERAPA KONDISI BETWEEN DAN SATU KONDISI >= (1)
                    'JIKA KELIPATAN (2) MAKA HANYA BOLEH SATU KONDISI
                    sql = "SELECT pt.ptkategori as kategori, pt.ptoperator as operator, (CASE pt.ptoperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_point_transaction pt WHERE pt.ptkategori = '" & FxDB(dr1("ptkategori"), "") & "' GROUP BY pt.ptoperator ORDER BY pt.ptoperator"
                    dtOperator = AsDataTableAmbilDariDB(sql)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "POS Category : " & FxDB(dr2("kategori"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("ptoperator") = 2 Or (vOperator = 1 And dr1("ptoperator") = vOperator) Then
                                        result(2) = "POS Category : " & FxDB(dr2("kategori"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("ptkategori")) & "', '" & FixQuotes(dr1("ptoperator")) & "', '" & FixDouble(dr1("ptjml1")) & "', '" & FixDouble(dr1("ptjml2")) & "', '" & FixDouble(dr1("ptjmlpoint")) & "', '" & FixQuotes(dr1("ptcustomtext1")) & "', '" & FixQuotes(dr1("ptcustomtext2")) & "', '" & FixQuotes(dr1("ptcustomtext3")) & "', '" & FixQuotes(dr1("ptcustomtext4")) & "', '" & FixQuotes(dr1("ptcustomtext5")) & "', " & dr1("ptcustomint1") & ", " & dr1("ptcustomint2") & ", " & dr1("ptcustomint3") & ", '" & FixDouble(dr1("ptcustomdbl1")) & "', '" & FixDouble(dr1("ptcustomdbl2")) & "', '" & FixDouble(dr1("ptcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pttgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pttgl2"))) & "')")

                    sql = "Insert into M_12_Pos_Point_Transaction(ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, pttgl1, pttgl2) values" & strValue2.ToString & ""
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
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_Point_TransactionSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_TransactionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDB(sql)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
    Public Function M12_Pos_Point_TransactionDeleteOld(ByVal param As String) As String

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

        '//FILTER KATEGORI POS UNTUK LOGOUT USER KATEGORI TERSEBUT, AGAR LOAD SETTING POS YG TERBARU
        Dim ftKategoriPOS As String = ""

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
        Dim ptkategori As String = "", ptoperator As String = "", ptjml1 As String = "", ptjml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 4) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK ptkategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "ptkategori can't be empty." : GoTo selesai
            Else
                ptkategori = idtrans(0)
            End If
            'CEK ptoperator
            If (Len(idtrans(1)) = 0) Then
                result(2) = "ptoperator can't be empty." : GoTo selesai
            Else
                ptoperator = idtrans(1)
            End If
            'CEK ptjml1
            If (IsNumeric(idtrans(2)) = False) Then
                result(2) = "ptjml1 required numeric." : GoTo selesai
            Else
                ptjml1 = idtrans(2)
            End If
            'CEK ptjml2
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "ptjml2 required numeric." : GoTo selesai
            Else
                ptjml2 = idtrans(3)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'AMBIL KATEGORI POS TRANSAKSI UNTUK FILTER USER LOGIN
            sql = "SELECT ptkategori as kategoripos FROM M_12_Pos_Point_Transaction WHERE ptkategori = '" & ptkategori & "' AND ptoperator = '" & ptoperator & "' AND ptjml1 = '" & ptjml1 & "' AND ptjml2 = '" & ptjml2 & "' GROUP BY ptkategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Point_Transaction WHERE ptkategori = '" & ptkategori & "' AND ptoperator = '" & ptoperator & "' AND ptjml1 = '" & ptjml1 & "' AND ptjml2 = '" & ptjml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Point_TransactionSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_TransactionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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


            'PROSES LOGOUT USER =====================================================
            If Len(ftKategoriPOS) > 0 Then
                'USER YG LOGIN DILOGOUT AGAR SETTING POS TERLOAD ULANG
                sql = "SELECT ul.ulid, u.userid FROM m0_userlogin ul JOIN m0_user u ON ul.uluser = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE (" & ftKategoriPOS & ")"
                Dim dtUser As DataTable = AsDataTableAmbilDariDB(sql)
                If dtUser.Rows.Count > 0 Then
                    Dim WsLogout As New m0_login
                    Dim rsLogout As String = ""
                    For Each drUser As DataRow In dtUser.Rows
                        'LOGOUT USER SESUAI KATEGORI POS YG DISETTING
                        rsLogout = WsLogout.M0_Logout(drUser("ulid") & "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & drUser("userid") & "★0★" & Application("AppCode") & "")
                    Next
                End If

            End If
            'END OF PROSES LOGOUT USER ==============================================


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
    Public Function M12_Pos_Point_TransactionImportOld(ByVal param As String) As String
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
        'ptkategori(0) As String, ptoperator(1) As String, ptjml1(2) As Double, ptjml2(3) As Double, ptjmlpoint(4) As Double, 
        'ptcustomtext1(5) As String, ptcustomtext2(6) As String, ptcustomtext3(7) As String, ptcustomtext4(8) As String, ptcustomtext5(9) As String, 
        'ptcustomint1(10) As Integer, ptcustomint2(11) As Integer, ptcustomint3(12) As Integer, ptcustomdbl1(13) As Double, ptcustomdbl2(14) As Double, 
        'ptcustomdbl3(15) As Double, ptcustomdate1(16) As Date, ptcustomdate2(17) As Date, ptcustomdate3(18) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, 
        'ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, 
        'ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "ptkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptoperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptjmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "ptcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ptcustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 19) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'ptjml1(2) As Double
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - ptjml1 required numeric." : GoTo selesai
            End If
            'ptjml2(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - ptjml2 required numeric." : GoTo selesai
            End If
            'ptjmlpoint(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - ptjmlpoint required numeric." : GoTo selesai
            End If
            'ptcustomint1(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint1 required numeric." : GoTo selesai
            End If
            'ptcustomint2(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint2 required numeric." : GoTo selesai
            End If
            'ptcustomint3(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - ptcustomint3 required numeric." : GoTo selesai
            End If
            'ptcustomdbl1(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl1 required numeric." : GoTo selesai
            End If
            'ptcustomdbl2(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl2 required numeric." : GoTo selesai
            End If
            'ptcustomdbl3(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdbl3 required numeric." : GoTo selesai
            End If
            'ptcustomdate1(16) As Date
            If (IsDate(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate1 required date." : GoTo selesai
            End If
            'ptcustomdate2(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate2 required date." : GoTo selesai
            End If
            'ptcustomdate3(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - ptcustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'ptkategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - ptkategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - ptkategori should not be more than 25 character." : GoTo selesai
            End If

            'ptoperator(1) As String
            If IsNumeric(dataRowDetail(1)) = False Then
                result(2) = "Row : " & i & " - ptoperator can't be empty" : GoTo selesai
            ElseIf dataRowDetail(1) <> 0 And dataRowDetail(1) <> 1 And dataRowDetail(1) <> 2 Then
                result(2) = "Row : " & i & " - invalid ptoperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - ptoperator should not be more than 25 character." : GoTo selesai
            End If

            'ptjml1(2) As Double
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - ptjml1 can't be empty" : GoTo selesai
            End If

            'ptjml2(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - ptjml2 can't be empty" : GoTo selesai
            End If

            'ptjmlpoint(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - ptjmlpoint can't be empty" : GoTo selesai
            End If

            'ptcustomdbl1(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl1 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl2(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl2 can't be empty" : GoTo selesai
            End If

            'ptcustomdbl3(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdbl3 can't be empty" : GoTo selesai
            End If

            'ptcustomdate1(16) As Date
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate1 can't be empty" : GoTo selesai
            End If

            'ptcustomdate2(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate2 can't be empty" : GoTo selesai
            End If

            'ptcustomdate3(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - ptcustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "ptkategori~ptoperator~ptjml1~ptjml2~ptjmlpoint~ptcustomtext1~ptcustomtext2~ptcustomtext3~ptcustomtext4~ptcustomtext5~ptcustomint1~ptcustomint2~ptcustomint3~ptcustomdbl1~ptcustomdbl2~ptcustomdbl3~ptcustomdate1~ptcustomdate2~ptcustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("ptkategori")) & "', '" & FixQuotes(dr1("ptoperator")) & "', '" & FixDouble(dr1("ptjml1")) & "', '" & FixDouble(dr1("ptjml2")) & "', '" & FixDouble(dr1("ptjmlpoint")) & "', '" & FixQuotes(dr1("ptcustomtext1")) & "', '" & FixQuotes(dr1("ptcustomtext2")) & "', '" & FixQuotes(dr1("ptcustomtext3")) & "', '" & FixQuotes(dr1("ptcustomtext4")) & "', '" & FixQuotes(dr1("ptcustomtext5")) & "', " & dr1("ptcustomint1") & ", " & dr1("ptcustomint2") & ", " & dr1("ptcustomint3") & ", '" & FixDouble(dr1("ptcustomdbl1")) & "', '" & FixDouble(dr1("ptcustomdbl2")) & "', '" & FixDouble(dr1("ptcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ptcustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Point_Transaction"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Point_Transaction(ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_TransactionSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_TransactionSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_TransactionSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Point_TransactionSearch --------------------------------------------------------
        'ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, 
        'ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, 
        'ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, pcnama, ptoperatornama, pttgl1, pttgl2

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", SFilterSplit() As String = {}, SFilter As String = ""

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
        sql = "select `pt`.`ptkategori` AS `ptkategori`,`pt`.`ptoperator` AS `ptoperator`,`pt`.`ptjml1` AS `ptjml1`,`pt`.`ptjml2` AS `ptjml2`,`pt`.`ptjmlpoint` AS `ptjmlpoint`,`pt`.`ptcustomtext1` AS `ptcustomtext1`,`pt`.`ptcustomtext2` AS `ptcustomtext2`,`pt`.`ptcustomtext3` AS `ptcustomtext3`,`pt`.`ptcustomtext4` AS `ptcustomtext4`,`pt`.`ptcustomtext5` AS `ptcustomtext5`,`pt`.`ptcustomint1` AS `ptcustomint1`,`pt`.`ptcustomint2` AS `ptcustomint2`,`pt`.`ptcustomint3` AS `ptcustomint3`,`pt`.`ptcustomdbl1` AS `ptcustomdbl1`,`pt`.`ptcustomdbl2` AS `ptcustomdbl2`,`pt`.`ptcustomdbl3` AS `ptcustomdbl3`,`pt`.`ptcustomdate1` AS `ptcustomdate1`,`pt`.`ptcustomdate2` AS `ptcustomdate2`,`pt`.`ptcustomdate3` AS `ptcustomdate3`,`pc`.`pcnama` AS `pcnama`,(case `pt`.`ptoperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `ptoperatornama`, `pt`.`pttgl1` AS `pttgl1`, `pt`.`pttgl2` AS `pttgl2` from (`m_12_pos_point_transaction` `pt` join `m_12_pos_category` `pc` on((`pt`.`ptkategori` = `pc`.`pckode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Point_Transaction", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ptkategori"), ""), sptField,
                     FxDB(dr("ptoperator"), ""), sptField,
                     FxDB(dr("ptjml1"), 0), sptField,
                     FxDB(dr("ptjml2"), 0), sptField,
                     FxDB(dr("ptjmlpoint"), 0), sptField,
                     FxDB(dr("ptcustomtext1"), ""), sptField,
                     FxDB(dr("ptcustomtext2"), ""), sptField,
                     FxDB(dr("ptcustomtext3"), ""), sptField,
                     FxDB(dr("ptcustomtext4"), ""), sptField,
                     FxDB(dr("ptcustomtext5"), ""), sptField,
                     FxDB(dr("ptcustomint1"), 0), sptField,
                     FxDB(dr("ptcustomint2"), 0), sptField,
                     FxDB(dr("ptcustomint3"), 0), sptField,
                     FxDB(dr("ptcustomdbl1"), 0), sptField,
                     FxDB(dr("ptcustomdbl2"), 0), sptField,
                     FxDB(dr("ptcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("ptoperatornama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pttgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pttgl2"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Point Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, pcnama, ptoperatornama, pttgl1, pttgl2"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Point_TransactionDownload(ByVal param As String) As String
        'M12_Pos_Point_TransactionDownload --------------------------------------------------------
        'ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, 
        'ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, 
        'ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", SFilterSplit() As String = {}, SFilter As String = ""

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

        dt = AmbilData("aplikasi1-M_12_Pos_Point_Transaction", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ptkategori"), ""), sptField,
                     FxDB(dr("ptoperator"), ""), sptField,
                     FxDB(dr("ptjml1"), 0), sptField,
                     FxDB(dr("ptjml2"), 0), sptField,
                     FxDB(dr("ptjmlpoint"), 0), sptField,
                     FxDB(dr("ptcustomtext1"), ""), sptField,
                     FxDB(dr("ptcustomtext2"), ""), sptField,
                     FxDB(dr("ptcustomtext3"), ""), sptField,
                     FxDB(dr("ptcustomtext4"), ""), sptField,
                     FxDB(dr("ptcustomtext5"), ""), sptField,
                     FxDB(dr("ptcustomint1"), 0), sptField,
                     FxDB(dr("ptcustomint2"), 0), sptField,
                     FxDB(dr("ptcustomint3"), 0), sptField,
                     FxDB(dr("ptcustomdbl1"), 0), sptField,
                     FxDB(dr("ptcustomdbl2"), 0), sptField,
                     FxDB(dr("ptcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Point Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptkategori, ptoperator, ptjml1, ptjml2, ptjmlpoint, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3"))

        Return wsResult
    End Function

End Class
