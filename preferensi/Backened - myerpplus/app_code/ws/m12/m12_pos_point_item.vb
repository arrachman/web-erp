Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_point_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Point_ItemSimpan(ByVal param As String) As String
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
        'pikategori(0) As String, piidbarang(1) As Double, pioperator(2) As String, pijml1(3) As Double, pijml2(4) As Double, 
        'pijmlpoint(5) As Double, picustomtext1(6) As String, picustomtext2(7) As String, picustomtext3(8) As String, picustomtext4(9) As String, 
        'picustomtext5(10) As String, picustomint1(11) As Integer, picustomint2(12) As Integer, picustomint3(13) As Integer, picustomdbl1(14) As Double, 
        'picustomdbl2(15) As Double, picustomdbl3(16) As Double, picustomdate1(17) As Date, picustomdate2(18) As Date, picustomdate3(19) As Date
        'pitgl1(20) As Date, pitgl2(21) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, 
        'picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, 
        'picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3
        'pitgl1, pitgl2

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pitgl2", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pijml1 required numeric." : GoTo selesai
            End If
            'pijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pijml2 required numeric." : GoTo selesai
            End If
            'pijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pijmlpoint required numeric." : GoTo selesai
            End If
            'picustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'pitgl1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - pitgl1 required date." : GoTo selesai
            End If
            'pitgl2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - pitgl2 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pioperator should not be more than 25 character." : GoTo selesai
            End If

            'pijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pijml1 can't be empty" : GoTo selesai
            End If

            'pijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pijml2 can't be empty" : GoTo selesai
            End If

            'pijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pijmlpoint can't be empty" : GoTo selesai
            End If

            'picustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If

            'pitgl1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pitgl1 can't be empty" : GoTo selesai
            End If

            'pitgl2(21) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pitgl2 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pioperator~pijml1~pijml2~pijmlpoint~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~pitgl1~pitgl2", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
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
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("pikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM m_12_pos_point_item WHERE pikategori = '" & FixQuotes(drutama("pikategori")) & "' AND piidbarang = '" & FixQuotes(drutama("piidbarang")) & "'"
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
                    sql = "SELECT pi.pikategori as kategori, pi.piidbarang as idbarang, pi.pioperator as operator, i.bkode, (CASE pi.pioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_point_item pi JOIN m1_item i ON pi.piidbarang = i.bid WHERE pi.pikategori = '" & FxDB(dr1("pikategori"), "") & "' AND pi.piidbarang = '" & FxDB(dr1("piidbarang"), "") & "' GROUP BY pi.pioperator ORDER BY pi.pioperator"
                    dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("pioperator") = 2 Or (vOperator = 1 And dr1("pioperator") = vOperator) Then
                                        result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixQuotes(dr1("pioperator")) & "', '" & FixDouble(dr1("pijml1")) & "', '" & FixDouble(dr1("pijml2")) & "', '" & FixDouble(dr1("pijmlpoint")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pitgl2"))) & "')")

                    sql = "Insert into M_12_Pos_Point_Item(pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pitgl1, pitgl2) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_ItemDelete(ByVal param As String) As String

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
        Dim pikategori As String = "", piidbarang As String = "", pioperator As String = "", pijml1 As String = "", pijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK pikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pikategori can't be empty." : GoTo selesai
            Else
                pikategori = idtrans(0)
            End If
            'CEK piidbarang
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "piidbarang required numeric." : GoTo selesai
            Else
                piidbarang = idtrans(1)
            End If
            'CEK pioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "pioperator can't be empty." : GoTo selesai
            Else
                pioperator = idtrans(2)
            End If
            'CEK pijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "pijml1 required numeric." : GoTo selesai
            Else
                pijml1 = idtrans(3)
            End If
            'CEK pijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "pijml2 required numeric." : GoTo selesai
            Else
                pijml2 = idtrans(4)
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
            sql = "SELECT pikategori as kategoripos FROM M_12_Pos_Point_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "' AND pioperator = '" & pioperator & "' AND pijml1 = '" & pijml1 & "' AND pijml2 = '" & pijml2 & "' GROUP BY pikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Point_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "' AND pioperator = '" & pioperator & "' AND pijml1 = '" & pijml1 & "' AND pijml2 = '" & pijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Point_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_ItemImport(ByVal param As String) As String
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
        'pikategori(0) As String, piidbarang(1) As Double, pioperator(2) As String, pijml1(3) As Double, pijml2(4) As Double, 
        'pijmlpoint(5) As Double, picustomtext1(6) As String, picustomtext2(7) As String, picustomtext3(8) As String, picustomtext4(9) As String, 
        'picustomtext5(10) As String, picustomint1(11) As Integer, picustomint2(12) As Integer, picustomint3(13) As Integer, picustomdbl1(14) As Double, 
        'picustomdbl2(15) As Double, picustomdbl3(16) As Double, picustomdate1(17) As Date, picustomdate2(18) As Date, picustomdate3(19) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, 
        'picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, 
        'picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 20) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pijml1 required numeric." : GoTo selesai
            End If
            'pijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pijml2 required numeric." : GoTo selesai
            End If
            'pijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pijmlpoint required numeric." : GoTo selesai
            End If
            'picustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pioperator should not be more than 25 character." : GoTo selesai
            End If

            'pijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pijml1 can't be empty" : GoTo selesai
            End If

            'pijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pijml2 can't be empty" : GoTo selesai
            End If

            'pijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pijmlpoint can't be empty" : GoTo selesai
            End If

            'picustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pioperator~pijml1~pijml2~pijmlpoint~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixQuotes(dr1("pioperator")) & "', '" & FixDouble(dr1("pijml1")) & "', '" & FixDouble(dr1("pijml2")) & "', '" & FixDouble(dr1("pijmlpoint")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Point_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Point_Item(pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_ItemSimpanOld(ByVal param As String) As String
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
        'pikategori(0) As String, piidbarang(1) As Double, pioperator(2) As String, pijml1(3) As Double, pijml2(4) As Double, 
        'pijmlpoint(5) As Double, picustomtext1(6) As String, picustomtext2(7) As String, picustomtext3(8) As String, picustomtext4(9) As String, 
        'picustomtext5(10) As String, picustomint1(11) As Integer, picustomint2(12) As Integer, picustomint3(13) As Integer, picustomdbl1(14) As Double, 
        'picustomdbl2(15) As Double, picustomdbl3(16) As Double, picustomdate1(17) As Date, picustomdate2(18) As Date, picustomdate3(19) As Date
        'pitgl1(20) As Date, pitgl2(21) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, 
        'picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, 
        'picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3
        'pitgl1, pitgl2

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pitgl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pitgl2", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 22) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pijml1 required numeric." : GoTo selesai
            End If
            'pijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pijml2 required numeric." : GoTo selesai
            End If
            'pijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pijmlpoint required numeric." : GoTo selesai
            End If
            'picustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'pitgl1(20) As Date
            If (IsDate(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - pitgl1 required date." : GoTo selesai
            End If
            'pitgl2(21) As Date
            If (IsDate(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - pitgl2 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pioperator should not be more than 25 character." : GoTo selesai
            End If

            'pijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pijml1 can't be empty" : GoTo selesai
            End If

            'pijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pijml2 can't be empty" : GoTo selesai
            End If

            'pijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pijmlpoint can't be empty" : GoTo selesai
            End If

            'picustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If

            'pitgl1(20) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pitgl1 can't be empty" : GoTo selesai
            End If

            'pitgl2(21) As Date
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - pitgl2 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pioperator~pijml1~pijml2~pijmlpoint~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3~pitgl1~pitgl2", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21)) = False Then
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
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("pikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM m_12_pos_point_item WHERE pikategori = '" & FixQuotes(drutama("pikategori")) & "' AND piidbarang = '" & FixQuotes(drutama("piidbarang")) & "'"
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
                    sql = "SELECT pi.pikategori as kategori, pi.piidbarang as idbarang, pi.pioperator as operator, i.bkode, (CASE pi.pioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM m_12_pos_point_item pi JOIN m1_item i ON pi.piidbarang = i.bid WHERE pi.pikategori = '" & FxDB(dr1("pikategori"), "") & "' AND pi.piidbarang = '" & FxDB(dr1("piidbarang"), "") & "' GROUP BY pi.pioperator ORDER BY pi.pioperator"
                    dtOperator = AsDataTableAmbilDariDB(sql)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("pioperator") = 2 Or (vOperator = 1 And dr1("pioperator") = vOperator) Then
                                        result(2) = "Item : " & FxDB(dr2("bkode"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixQuotes(dr1("pioperator")) & "', '" & FixDouble(dr1("pijml1")) & "', '" & FixDouble(dr1("pijml2")) & "', '" & FixDouble(dr1("pijmlpoint")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pitgl1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pitgl2"))) & "')")

                    sql = "Insert into M_12_Pos_Point_Item(pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pitgl1, pitgl2) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_ItemDeleteOld(ByVal param As String) As String

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
        Dim pikategori As String = "", piidbarang As String = "", pioperator As String = "", pijml1 As String = "", pijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK pikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pikategori can't be empty." : GoTo selesai
            Else
                pikategori = idtrans(0)
            End If
            'CEK piidbarang
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "piidbarang required numeric." : GoTo selesai
            Else
                piidbarang = idtrans(1)
            End If
            'CEK pioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "pioperator can't be empty." : GoTo selesai
            Else
                pioperator = idtrans(2)
            End If
            'CEK pijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "pijml1 required numeric." : GoTo selesai
            Else
                pijml1 = idtrans(3)
            End If
            'CEK pijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "pijml2 required numeric." : GoTo selesai
            Else
                pijml2 = idtrans(4)
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
            sql = "SELECT pikategori as kategoripos FROM M_12_Pos_Point_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "' AND pioperator = '" & pioperator & "' AND pijml1 = '" & pijml1 & "' AND pijml2 = '" & pijml2 & "' GROUP BY pikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Point_Item WHERE pikategori = '" & pikategori & "' AND piidbarang = '" & piidbarang & "' AND pioperator = '" & pioperator & "' AND pijml1 = '" & pijml1 & "' AND pijml2 = '" & pijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Point_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_ItemImportOld(ByVal param As String) As String
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
        'pikategori(0) As String, piidbarang(1) As Double, pioperator(2) As String, pijml1(3) As Double, pijml2(4) As Double, 
        'pijmlpoint(5) As Double, picustomtext1(6) As String, picustomtext2(7) As String, picustomtext3(8) As String, picustomtext4(9) As String, 
        'picustomtext5(10) As String, picustomint1(11) As Integer, picustomint2(12) As Integer, picustomint3(13) As Integer, picustomdbl1(14) As Double, 
        'picustomdbl2(15) As Double, picustomdbl3(16) As Double, picustomdate1(17) As Date, picustomdate2(18) As Date, picustomdate3(19) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, 
        'picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, 
        'picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "piidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "picustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "picustomdate3", AsEnumTypeData.AsString)

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
            If (dataRowDetail.Length <> 20) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'piidbarang(1) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - piidbarang required numeric." : GoTo selesai
            End If
            'pijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pijml1 required numeric." : GoTo selesai
            End If
            'pijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pijml2 required numeric." : GoTo selesai
            End If
            'pijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pijmlpoint required numeric." : GoTo selesai
            End If
            'picustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - picustomint1 required numeric." : GoTo selesai
            End If
            'picustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - picustomint2 required numeric." : GoTo selesai
            End If
            'picustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - picustomint3 required numeric." : GoTo selesai
            End If
            'picustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl1 required numeric." : GoTo selesai
            End If
            'picustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl2 required numeric." : GoTo selesai
            End If
            'picustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - picustomdbl3 required numeric." : GoTo selesai
            End If
            'picustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - picustomdate1 required date." : GoTo selesai
            End If
            'picustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - picustomdate2 required date." : GoTo selesai
            End If
            'picustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - picustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pikategori should not be more than 25 character." : GoTo selesai
            End If

            'piidbarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - piidbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 20 Then
                result(2) = "Row : " & i & " - piidbarang should not be more than 20 character." : GoTo selesai
            End If

            'pioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pioperator should not be more than 25 character." : GoTo selesai
            End If

            'pijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pijml1 can't be empty" : GoTo selesai
            End If

            'pijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pijml2 can't be empty" : GoTo selesai
            End If

            'pijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pijmlpoint can't be empty" : GoTo selesai
            End If

            'picustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl1 can't be empty" : GoTo selesai
            End If

            'picustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl2 can't be empty" : GoTo selesai
            End If

            'picustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - picustomdbl3 can't be empty" : GoTo selesai
            End If

            'picustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate1 can't be empty" : GoTo selesai
            End If

            'picustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate2 can't be empty" : GoTo selesai
            End If

            'picustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - picustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pikategori~piidbarang~pioperator~pijml1~pijml2~pijmlpoint~picustomtext1~picustomtext2~picustomtext3~picustomtext4~picustomtext5~picustomint1~picustomint2~picustomint3~picustomdbl1~picustomdbl2~picustomdbl3~picustomdate1~picustomdate2~picustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("pikategori")) & "', '" & FixQuotes(dr1("piidbarang")) & "', '" & FixQuotes(dr1("pioperator")) & "', '" & FixDouble(dr1("pijml1")) & "', '" & FixDouble(dr1("pijml2")) & "', '" & FixDouble(dr1("pijmlpoint")) & "', '" & FixQuotes(dr1("picustomtext1")) & "', '" & FixQuotes(dr1("picustomtext2")) & "', '" & FixQuotes(dr1("picustomtext3")) & "', '" & FixQuotes(dr1("picustomtext4")) & "', '" & FixQuotes(dr1("picustomtext5")) & "', " & dr1("picustomint1") & ", " & dr1("picustomint2") & ", " & dr1("picustomint3") & ", '" & FixDouble(dr1("picustomdbl1")) & "', '" & FixDouble(dr1("picustomdbl2")) & "', '" & FixDouble(dr1("picustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("picustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Point_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Point_Item(pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_ItemSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Point_ItemSearch --------------------------------------------------------
        'pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, 
        'picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, 
        'picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pcnama, 
        'bkode, bnama, btipe, bsatuan, pioperatornama, pitgl1, pitgl2

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
        sql = "select `pi`.`pikategori` AS `pikategori`,`pi`.`piidbarang` AS `piidbarang`,`pi`.`pioperator` AS `pioperator`,`pi`.`pijml1` AS `pijml1`,`pi`.`pijml2` AS `pijml2`,`pi`.`pijmlpoint` AS `pijmlpoint`,`pi`.`picustomtext1` AS `picustomtext1`,`pi`.`picustomtext2` AS `picustomtext2`,`pi`.`picustomtext3` AS `picustomtext3`,`pi`.`picustomtext4` AS `picustomtext4`,`pi`.`picustomtext5` AS `picustomtext5`,`pi`.`picustomint1` AS `picustomint1`,`pi`.`picustomint2` AS `picustomint2`,`pi`.`picustomint3` AS `picustomint3`,`pi`.`picustomdbl1` AS `picustomdbl1`,`pi`.`picustomdbl2` AS `picustomdbl2`,`pi`.`picustomdbl3` AS `picustomdbl3`,`pi`.`picustomdate1` AS `picustomdate1`,`pi`.`picustomdate2` AS `picustomdate2`,`pi`.`picustomdate3` AS `picustomdate3`,`pc`.`pcnama` AS `pcnama`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bsatuan` AS `bsatuan`,(case `pi`.`pioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `pioperatornama`, `pitgl1` AS `pitgl1`, `pitgl2` AS `pitgl2` from ((`m_12_pos_point_item` `pi` join `m_12_pos_category` `pc` on((`pi`.`pikategori` = `pc`.`pckode`))) join `m1_item` `i` on((`pi`.`piidbarang` = `i`.`bid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Point_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pikategori"), ""), sptField,
                     FxDB(dr("piidbarang"), ""), sptField,
                     FxDB(dr("pioperator"), ""), sptField,
                     FxDB(dr("pijml1"), 0), sptField,
                     FxDB(dr("pijml2"), 0), sptField,
                     FxDB(dr("pijmlpoint"), 0), sptField,
                     FxDB(dr("picustomtext1"), ""), sptField,
                     FxDB(dr("picustomtext2"), ""), sptField,
                     FxDB(dr("picustomtext3"), ""), sptField,
                     FxDB(dr("picustomtext4"), ""), sptField,
                     FxDB(dr("picustomtext5"), ""), sptField,
                     FxDB(dr("picustomint1"), 0), sptField,
                     FxDB(dr("picustomint2"), 0), sptField,
                     FxDB(dr("picustomint3"), 0), sptField,
                     FxDB(dr("picustomdbl1"), 0), sptField,
                     FxDB(dr("picustomdbl2"), 0), sptField,
                     FxDB(dr("picustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("pioperatornama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pitgl1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pitgl2"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Point Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3, pcnama, bkode, bnama, btipe, bsatuan, pioperatornama, pitgl1, pitgl2"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Point_ItemDownload(ByVal param As String) As String
        'M12_Pos_Point_ItemDownload --------------------------------------------------------
        'pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, 
        'picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, 
        'picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3

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

        dt = AmbilData("aplikasi1-M_12_Pos_Point_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pikategori"), ""), sptField,
                     FxDB(dr("piidbarang"), ""), sptField,
                     FxDB(dr("pioperator"), ""), sptField,
                     FxDB(dr("pijml1"), 0), sptField,
                     FxDB(dr("pijml2"), 0), sptField,
                     FxDB(dr("pijmlpoint"), 0), sptField,
                     FxDB(dr("picustomtext1"), ""), sptField,
                     FxDB(dr("picustomtext2"), ""), sptField,
                     FxDB(dr("picustomtext3"), ""), sptField,
                     FxDB(dr("picustomtext4"), ""), sptField,
                     FxDB(dr("picustomtext5"), ""), sptField,
                     FxDB(dr("picustomint1"), 0), sptField,
                     FxDB(dr("picustomint2"), 0), sptField,
                     FxDB(dr("picustomint3"), 0), sptField,
                     FxDB(dr("picustomdbl1"), 0), sptField,
                     FxDB(dr("picustomdbl2"), 0), sptField,
                     FxDB(dr("picustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("picustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Point Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pikategori, piidbarang, pioperator, pijml1, pijml2, pijmlpoint, picustomtext1, picustomtext2, picustomtext3, picustomtext4, picustomtext5, picustomint1, picustomint2, picustomint3, picustomdbl1, picustomdbl2, picustomdbl3, picustomdate1, picustomdate2, picustomdate3"))

        Return wsResult
    End Function

End Class
