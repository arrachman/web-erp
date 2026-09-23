Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_point_category_item
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Point_Category_ItemSimpan(ByVal param As String) As String
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
        'pcikategori(0) As String, pcikategoribarang(1) As String, pcioperator(2) As String, pcijml1(3) As Double, pcijml2(4) As Double, 
        'pcijmlpoint(5) As Double, pcicustomtext1(6) As String, pcicustomtext2(7) As String, pcicustomtext3(8) As String, pcicustomtext4(9) As String, 
        'pcicustomtext5(10) As String, pcicustomint1(11) As Integer, pcicustomint2(12) As Integer, pcicustomint3(13) As Integer, pcicustomdbl1(14) As Double, 
        'pcicustomdbl2(15) As Double, pcicustomdbl3(16) As Double, pcicustomdate1(17) As Date, pcicustomdate2(18) As Date, pcicustomdate3(19) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, 
        'pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, 
        'pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate3", AsEnumTypeData.AsString)

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
            'pcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pcijml1 required numeric." : GoTo selesai
            End If
            'pcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pcijml2 required numeric." : GoTo selesai
            End If
            'pcijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pcijmlpoint required numeric." : GoTo selesai
            End If
            'pcicustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint1 required numeric." : GoTo selesai
            End If
            'pcicustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint2 required numeric." : GoTo selesai
            End If
            'pcicustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint3 required numeric." : GoTo selesai
            End If
            'pcicustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl1 required numeric." : GoTo selesai
            End If
            'pcicustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl2 required numeric." : GoTo selesai
            End If
            'pcicustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl3 required numeric." : GoTo selesai
            End If
            'pcicustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate1 required date." : GoTo selesai
            End If
            'pcicustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate2 required date." : GoTo selesai
            End If
            'pcicustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pcikategori should not be more than 25 character." : GoTo selesai
            End If

            'pcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'pcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pcioperator should not be more than 25 character." : GoTo selesai
            End If

            'pcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pcijml1 can't be empty" : GoTo selesai
            End If

            'pcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pcijml2 can't be empty" : GoTo selesai
            End If

            'pcijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pcijmlpoint can't be empty" : GoTo selesai
            End If

            'pcicustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'pcicustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate1 can't be empty" : GoTo selesai
            End If

            'pcicustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate2 can't be empty" : GoTo selesai
            End If

            'pcicustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pcikategori~pcikategoribarang~pcioperator~pcijml1~pcijml2~pcijmlpoint~pcicustomtext1~pcicustomtext2~pcicustomtext3~pcicustomtext4~pcicustomtext5~pcicustomint1~pcicustomint2~pcicustomint3~pcicustomdbl1~pcicustomdbl2~pcicustomdbl3~pcicustomdate1~pcicustomdate2~pcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
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
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("pcikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM M_12_Pos_Point_Category_Item WHERE pcikategori = '" & FixQuotes(drutama("pcikategori")) & "' AND pcikategoribarang = '" & FixQuotes(drutama("pcikategoribarang")) & "'"
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
                    sql = "SELECT pci.pcikategori as kategori, pci.pcikategoribarang as kategoribarang, pci.pcioperator as operator, ic.icnama, (CASE pci.pcioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM M_12_Pos_Point_Category_Item pci JOIN m1_item_category ic ON pci.pcikategoribarang = ic.ickode WHERE pci.pcikategori = '" & FxDB(dr1("pcikategori"), "") & "' AND pci.pcikategoribarang = '" & FxDB(dr1("pcikategoribarang"), "") & "' GROUP BY pci.pcioperator ORDER BY pci.pcioperator"
                    dtOperator = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("pcioperator") = 2 Or (vOperator = 1 And dr1("pcioperator") = vOperator) Then
                                        result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("pcikategori")) & "', '" & FixQuotes(dr1("pcikategoribarang")) & "', '" & FixQuotes(dr1("pcioperator")) & "', '" & FixDouble(dr1("pcijml1")) & "', '" & FixDouble(dr1("pcijml2")) & "', '" & FixDouble(dr1("pcijmlpoint")) & "', '" & FixQuotes(dr1("pcicustomtext1")) & "', '" & FixQuotes(dr1("pcicustomtext2")) & "', '" & FixQuotes(dr1("pcicustomtext3")) & "', '" & FixQuotes(dr1("pcicustomtext4")) & "', '" & FixQuotes(dr1("pcicustomtext5")) & "', " & dr1("pcicustomint1") & ", " & dr1("pcicustomint2") & ", " & dr1("pcicustomint3") & ", '" & FixDouble(dr1("pcicustomdbl1")) & "', '" & FixDouble(dr1("pcicustomdbl2")) & "', '" & FixDouble(dr1("pcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate3"))) & "')")

                    sql = "Insert into M_12_Pos_Point_Category_Item(pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_Category_ItemDelete(ByVal param As String) As String

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
        Dim pcikategori As String = "", pcikategoribarang As String = "", pcioperator As String = "", pcijml1 As String = "", pcijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK pcikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pcikategori can't be empty." : GoTo selesai
            Else
                pcikategori = idtrans(0)
            End If
            'CEK pcikategoribarang
            If (Len(idtrans(1)) = 0) Then
                result(2) = "pcikategoribarang can't be empty." : GoTo selesai
            Else
                pcikategoribarang = idtrans(1)
            End If
            'CEK pcioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "pcioperator can't be empty." : GoTo selesai
            Else
                pcioperator = idtrans(2)
            End If
            'CEK pcijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "pcijml1 required numeric." : GoTo selesai
            Else
                pcijml1 = idtrans(3)
            End If
            'CEK pcijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "pcijml2 required numeric." : GoTo selesai
            Else
                pcijml2 = idtrans(4)
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
            sql = "SELECT pcikategori as kategoripos FROM M_12_Pos_Point_Category_Item WHERE pcikategori = '" & pcikategori & "' AND pcikategoribarang = '" & pcikategoribarang & "' AND pcioperator = '" & pcioperator & "' AND pcijml1 = '" & pcijml1 & "' AND pcijml2 = '" & pcijml2 & "' GROUP BY pcikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Point_Category_Item WHERE pcikategori = '" & pcikategori & "' AND pcikategoribarang = '" & pcikategoribarang & "' AND pcioperator = '" & pcioperator & "' AND pcijml1 = '" & pcijml1 & "' AND pcijml2 = '" & pcijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Point_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_Category_ItemImport(ByVal param As String) As String
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
        'pcikategori(0) As String, pcikategoribarang(1) As String, pcioperator(2) As String, pcijml1(3) As Double, pcijml2(4) As Double, 
        'pcijmlpoint(5) As Double, pcicustomtext1(6) As String, pcicustomtext2(7) As String, pcicustomtext3(8) As String, pcicustomtext4(9) As String, 
        'pcicustomtext5(10) As String, pcicustomint1(11) As Integer, pcicustomint2(12) As Integer, pcicustomint3(13) As Integer, pcicustomdbl1(14) As Double, 
        'pcicustomdbl2(15) As Double, pcicustomdbl3(16) As Double, pcicustomdate1(17) As Date, pcicustomdate2(18) As Date, pcicustomdate3(19) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, 
        'pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, 
        'pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate3", AsEnumTypeData.AsString)

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
            'pcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pcijml1 required numeric." : GoTo selesai
            End If
            'pcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pcijml2 required numeric." : GoTo selesai
            End If
            'pcijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pcijmlpoint required numeric." : GoTo selesai
            End If
            'pcicustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint1 required numeric." : GoTo selesai
            End If
            'pcicustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint2 required numeric." : GoTo selesai
            End If
            'pcicustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint3 required numeric." : GoTo selesai
            End If
            'pcicustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl1 required numeric." : GoTo selesai
            End If
            'pcicustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl2 required numeric." : GoTo selesai
            End If
            'pcicustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl3 required numeric." : GoTo selesai
            End If
            'pcicustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate1 required date." : GoTo selesai
            End If
            'pcicustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate2 required date." : GoTo selesai
            End If
            'pcicustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pcikategori should not be more than 25 character." : GoTo selesai
            End If

            'pcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'pcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pcioperator should not be more than 25 character." : GoTo selesai
            End If

            'pcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pcijml1 can't be empty" : GoTo selesai
            End If

            'pcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pcijml2 can't be empty" : GoTo selesai
            End If

            'pcijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pcijmlpoint can't be empty" : GoTo selesai
            End If

            'pcicustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'pcicustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate1 can't be empty" : GoTo selesai
            End If

            'pcicustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate2 can't be empty" : GoTo selesai
            End If

            'pcicustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pcikategori~pcikategoribarang~pcioperator~pcijml1~pcijml2~pcijmlpoint~pcicustomtext1~pcicustomtext2~pcicustomtext3~pcicustomtext4~pcicustomtext5~pcicustomint1~pcicustomint2~pcicustomint3~pcicustomdbl1~pcicustomdbl2~pcicustomdbl3~pcicustomdate1~pcicustomdate2~pcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("pcikategori")) & "', '" & FixQuotes(dr1("pcikategoribarang")) & "', '" & FixQuotes(dr1("pcioperator")) & "', '" & FixDouble(dr1("pcijml1")) & "', '" & FixDouble(dr1("pcijml2")) & "', '" & FixDouble(dr1("pcijmlpoint")) & "', '" & FixQuotes(dr1("pcicustomtext1")) & "', '" & FixQuotes(dr1("pcicustomtext2")) & "', '" & FixQuotes(dr1("pcicustomtext3")) & "', '" & FixQuotes(dr1("pcicustomtext4")) & "', '" & FixQuotes(dr1("pcicustomtext5")) & "', " & dr1("pcicustomint1") & ", " & dr1("pcicustomint2") & ", " & dr1("pcicustomint3") & ", '" & FixDouble(dr1("pcicustomdbl1")) & "', '" & FixDouble(dr1("pcicustomdbl2")) & "', '" & FixDouble(dr1("pcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Point_Category_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Point_Category_Item(pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_Category_ItemSimpanOld(ByVal param As String) As String
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
        'pcikategori(0) As String, pcikategoribarang(1) As String, pcioperator(2) As String, pcijml1(3) As Double, pcijml2(4) As Double, 
        'pcijmlpoint(5) As Double, pcicustomtext1(6) As String, pcicustomtext2(7) As String, pcicustomtext3(8) As String, pcicustomtext4(9) As String, 
        'pcicustomtext5(10) As String, pcicustomint1(11) As Integer, pcicustomint2(12) As Integer, pcicustomint3(13) As Integer, pcicustomdbl1(14) As Double, 
        'pcicustomdbl2(15) As Double, pcicustomdbl3(16) As Double, pcicustomdate1(17) As Date, pcicustomdate2(18) As Date, pcicustomdate3(19) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, 
        'pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, 
        'pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate3", AsEnumTypeData.AsString)

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
            'pcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pcijml1 required numeric." : GoTo selesai
            End If
            'pcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pcijml2 required numeric." : GoTo selesai
            End If
            'pcijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pcijmlpoint required numeric." : GoTo selesai
            End If
            'pcicustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint1 required numeric." : GoTo selesai
            End If
            'pcicustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint2 required numeric." : GoTo selesai
            End If
            'pcicustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint3 required numeric." : GoTo selesai
            End If
            'pcicustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl1 required numeric." : GoTo selesai
            End If
            'pcicustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl2 required numeric." : GoTo selesai
            End If
            'pcicustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl3 required numeric." : GoTo selesai
            End If
            'pcicustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate1 required date." : GoTo selesai
            End If
            'pcicustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate2 required date." : GoTo selesai
            End If
            'pcicustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pcikategori should not be more than 25 character." : GoTo selesai
            End If

            'pcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'pcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pcioperator should not be more than 25 character." : GoTo selesai
            End If

            'pcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pcijml1 can't be empty" : GoTo selesai
            End If

            'pcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pcijml2 can't be empty" : GoTo selesai
            End If

            'pcijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pcijmlpoint can't be empty" : GoTo selesai
            End If

            'pcicustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'pcicustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate1 can't be empty" : GoTo selesai
            End If

            'pcicustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate2 can't be empty" : GoTo selesai
            End If

            'pcicustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pcikategori~pcikategoribarang~pcioperator~pcijml1~pcijml2~pcijmlpoint~pcicustomtext1~pcicustomtext2~pcicustomtext3~pcicustomtext4~pcicustomtext5~pcicustomint1~pcicustomint2~pcicustomint3~pcicustomdbl1~pcicustomdbl2~pcicustomdbl3~pcicustomdate1~pcicustomdate2~pcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
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
                ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drutama("pcikategori")) & "' "

                'HAPUS DATA KATEGORI DAN BARANG YANG SAMA
                sql = "DELETE FROM M_12_Pos_Point_Category_Item WHERE pcikategori = '" & FixQuotes(drutama("pcikategori")) & "' AND pcikategoribarang = '" & FixQuotes(drutama("pcikategoribarang")) & "'"
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
                    sql = "SELECT pci.pcikategori as kategori, pci.pcikategoribarang as kategoribarang, pci.pcioperator as operator, ic.icnama, (CASE pci.pcioperator WHEN 0 THEN 'Between' WHEN 1 THEN '>=' WHEN 2 THEN 'Multiple' ELSE 'Unknown' END) as operatornama FROM M_12_Pos_Point_Category_Item pci JOIN m1_item_category ic ON pci.pcikategoribarang = ic.ickode WHERE pci.pcikategori = '" & FxDB(dr1("pcikategori"), "") & "' AND pci.pcikategoribarang = '" & FxDB(dr1("pcikategoribarang"), "") & "' GROUP BY pci.pcioperator ORDER BY pci.pcioperator"
                    dtOperator = AsDataTableAmbilDariDB(sql)
                    If dtOperator.Rows.Count > 0 Then
                        For Each dr2 As DataRow In dtOperator.Rows
                            vOperator = FxDB(dr2("operator").ToString, "")
                            If Len(vOperator) > 0 Then
                                If vOperator = 2 Then
                                    'JIKA SUDAH TERDAPAT OPERATOR KELIPATAN (2)
                                    result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                Else
                                    'JIKA BELUM TERDAPAT OPERATOR KELIPATAN (2), CEK KONDISI OPERATOR YANG SUDAH DIINPUTKAN
                                    'JIKA OPERATOR YANG DIINPUTKAN ADALAH KELIPATAN (2) MAKA TAMPILKAN ERRMESSAGE
                                    'JIKA SUDAH TERDAPAT OPERATOR >= (1) DAN YANG DIINPUTKAN ADALAH OPERATOR >= (1) LAGI MAKA TAMPILKAN ERRMESSAGE
                                    If dr1("pcioperator") = 2 Or (vOperator = 1 And dr1("pcioperator") = vOperator) Then
                                        result(2) = "Item Category : " & FxDB(dr2("icnama"), "") & " - already has '" & FxDB(dr2("operatornama"), 0) & "' condition." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                            End If
                        Next
                    End If

                    'strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Clear()
                    strValue2.Append("('" & FixQuotes(dr1("pcikategori")) & "', '" & FixQuotes(dr1("pcikategoribarang")) & "', '" & FixQuotes(dr1("pcioperator")) & "', '" & FixDouble(dr1("pcijml1")) & "', '" & FixDouble(dr1("pcijml2")) & "', '" & FixDouble(dr1("pcijmlpoint")) & "', '" & FixQuotes(dr1("pcicustomtext1")) & "', '" & FixQuotes(dr1("pcicustomtext2")) & "', '" & FixQuotes(dr1("pcicustomtext3")) & "', '" & FixQuotes(dr1("pcicustomtext4")) & "', '" & FixQuotes(dr1("pcicustomtext5")) & "', " & dr1("pcicustomint1") & ", " & dr1("pcicustomint2") & ", " & dr1("pcicustomint3") & ", '" & FixDouble(dr1("pcicustomdbl1")) & "', '" & FixDouble(dr1("pcicustomdbl2")) & "', '" & FixDouble(dr1("pcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate3"))) & "')")

                    sql = "Insert into M_12_Pos_Point_Category_Item(pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_Category_ItemDeleteOld(ByVal param As String) As String

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
        Dim pcikategori As String = "", pcikategoribarang As String = "", pcioperator As String = "", pcijml1 As String = "", pcijml2 As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 5) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK pcikategori
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pcikategori can't be empty." : GoTo selesai
            Else
                pcikategori = idtrans(0)
            End If
            'CEK pcikategoribarang
            If (Len(idtrans(1)) = 0) Then
                result(2) = "pcikategoribarang can't be empty." : GoTo selesai
            Else
                pcikategoribarang = idtrans(1)
            End If
            'CEK pcioperator
            If (Len(idtrans(2)) = 0) Then
                result(2) = "pcioperator can't be empty." : GoTo selesai
            Else
                pcioperator = idtrans(2)
            End If
            'CEK pcijml1
            If (IsNumeric(idtrans(3)) = False) Then
                result(2) = "pcijml1 required numeric." : GoTo selesai
            Else
                pcijml1 = idtrans(3)
            End If
            'CEK pcijml2
            If (IsNumeric(idtrans(4)) = False) Then
                result(2) = "pcijml2 required numeric." : GoTo selesai
            Else
                pcijml2 = idtrans(4)
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
            sql = "SELECT pcikategori as kategoripos FROM M_12_Pos_Point_Category_Item WHERE pcikategori = '" & pcikategori & "' AND pcikategoribarang = '" & pcikategoribarang & "' AND pcioperator = '" & pcioperator & "' AND pcijml1 = '" & pcijml1 & "' AND pcijml2 = '" & pcijml2 & "' GROUP BY pcikategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Point_Category_Item WHERE pcikategori = '" & pcikategori & "' AND pcikategoribarang = '" & pcikategoribarang & "' AND pcioperator = '" & pcioperator & "' AND pcijml1 = '" & pcijml1 & "' AND pcijml2 = '" & pcijml2 & "'"
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
            Dim paramSearch As String = M12_Pos_Point_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_Category_ItemImportOld(ByVal param As String) As String
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
        'pcikategori(0) As String, pcikategoribarang(1) As String, pcioperator(2) As String, pcijml1(3) As Double, pcijml2(4) As Double, 
        'pcijmlpoint(5) As Double, pcicustomtext1(6) As String, pcicustomtext2(7) As String, pcicustomtext3(8) As String, pcicustomtext4(9) As String, 
        'pcicustomtext5(10) As String, pcicustomint1(11) As Integer, pcicustomint2(12) As Integer, pcicustomint3(13) As Integer, pcicustomdbl1(14) As Double, 
        'pcicustomdbl2(15) As Double, pcicustomdbl3(16) As Double, pcicustomdate1(17) As Date, pcicustomdate2(18) As Date, pcicustomdate3(19) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, 
        'pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, 
        'pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pcikategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcikategoribarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcioperator", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijml2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcijmlpoint", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcicustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcicustomdate3", AsEnumTypeData.AsString)

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
            'pcijml1(3) As Double
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - pcijml1 required numeric." : GoTo selesai
            End If
            'pcijml2(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - pcijml2 required numeric." : GoTo selesai
            End If
            'pcijmlpoint(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - pcijmlpoint required numeric." : GoTo selesai
            End If
            'pcicustomint1(11) As Integer
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint1 required numeric." : GoTo selesai
            End If
            'pcicustomint2(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint2 required numeric." : GoTo selesai
            End If
            'pcicustomint3(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - pcicustomint3 required numeric." : GoTo selesai
            End If
            'pcicustomdbl1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl1 required numeric." : GoTo selesai
            End If
            'pcicustomdbl2(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl2 required numeric." : GoTo selesai
            End If
            'pcicustomdbl3(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdbl3 required numeric." : GoTo selesai
            End If
            'pcicustomdate1(17) As Date
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate1 required date." : GoTo selesai
            End If
            'pcicustomdate2(18) As Date
            If (IsDate(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate2 required date." : GoTo selesai
            End If
            'pcicustomdate3(19) As Date
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - pcicustomdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pcikategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pcikategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pcikategori should not be more than 25 character." : GoTo selesai
            End If

            'pcikategoribarang(1) As 
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - pcikategoribarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - pcikategoribarang should not be more than 25 character." : GoTo selesai
            End If

            'pcioperator(2) As String
            If IsNumeric(dataRowDetail(2)) = False Then
                result(2) = "Row : " & i & " - pcioperator required numeric" : GoTo selesai
            ElseIf dataRowDetail(2) <> 0 And dataRowDetail(2) <> 1 And dataRowDetail(2) <> 2 Then
                result(2) = "Row : " & i & " - invalid pcioperator value" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - pcioperator should not be more than 25 character." : GoTo selesai
            End If

            'pcijml1(3) As Double
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pcijml1 can't be empty" : GoTo selesai
            End If

            'pcijml2(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pcijml2 can't be empty" : GoTo selesai
            End If

            'pcijmlpoint(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - pcijmlpoint can't be empty" : GoTo selesai
            End If

            'pcicustomdbl1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl1 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl2(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl2 can't be empty" : GoTo selesai
            End If

            'pcicustomdbl3(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdbl3 can't be empty" : GoTo selesai
            End If

            'pcicustomdate1(17) As Date
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate1 can't be empty" : GoTo selesai
            End If

            'pcicustomdate2(18) As Date
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate2 can't be empty" : GoTo selesai
            End If

            'pcicustomdate3(19) As Date
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - pcicustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pcikategori~pcikategoribarang~pcioperator~pcijml1~pcijml2~pcijmlpoint~pcicustomtext1~pcicustomtext2~pcicustomtext3~pcicustomtext4~pcicustomtext5~pcicustomint1~pcicustomint2~pcicustomint3~pcicustomdbl1~pcicustomdbl2~pcicustomdbl3~pcicustomdate1~pcicustomdate2~pcicustomdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("pcikategori")) & "', '" & FixQuotes(dr1("pcikategoribarang")) & "', '" & FixQuotes(dr1("pcioperator")) & "', '" & FixDouble(dr1("pcijml1")) & "', '" & FixDouble(dr1("pcijml2")) & "', '" & FixDouble(dr1("pcijmlpoint")) & "', '" & FixQuotes(dr1("pcicustomtext1")) & "', '" & FixQuotes(dr1("pcicustomtext2")) & "', '" & FixQuotes(dr1("pcicustomtext3")) & "', '" & FixQuotes(dr1("pcicustomtext4")) & "', '" & FixQuotes(dr1("pcicustomtext5")) & "', " & dr1("pcicustomint1") & ", " & dr1("pcicustomint2") & ", " & dr1("pcicustomint3") & ", '" & FixDouble(dr1("pcicustomdbl1")) & "', '" & FixDouble(dr1("pcicustomdbl2")) & "', '" & FixDouble(dr1("pcicustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("pcicustomdate3"))) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Point_Category_Item"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada
                    sql = "Insert into M_12_Pos_Point_Category_Item(pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3) values" & strValue2.ToString & ""
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
            Dim paramSearch As String = M12_Pos_Point_Category_ItemSearch(PostWsSearch(paramSplit(0), "M12_Pos_Point_Category_ItemSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Point_Category_ItemSearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Point_Category_ItemSearch --------------------------------------------------------
        'pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, 
        'pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, 
        'pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3, pcnama, 
        'icnama, pcioperatornama

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
        sql = "select `pci`.`pcikategori` AS `pcikategori`,`pci`.`pcikategoribarang` AS `pcikategoribarang`,`pci`.`pcioperator` AS `pcioperator`,`pci`.`pcijml1` AS `pcijml1`,`pci`.`pcijml2` AS `pcijml2`,`pci`.`pcijmlpoint` AS `pcijmlpoint`,`pci`.`pcicustomtext1` AS `pcicustomtext1`,`pci`.`pcicustomtext2` AS `pcicustomtext2`,`pci`.`pcicustomtext3` AS `pcicustomtext3`,`pci`.`pcicustomtext4` AS `pcicustomtext4`,`pci`.`pcicustomtext5` AS `pcicustomtext5`,`pci`.`pcicustomint1` AS `pcicustomint1`,`pci`.`pcicustomint2` AS `pcicustomint2`,`pci`.`pcicustomint3` AS `pcicustomint3`,`pci`.`pcicustomdbl1` AS `pcicustomdbl1`,`pci`.`pcicustomdbl2` AS `pcicustomdbl2`,`pci`.`pcicustomdbl3` AS `pcicustomdbl3`,`pci`.`pcicustomdate1` AS `pcicustomdate1`,`pci`.`pcicustomdate2` AS `pcicustomdate2`,`pci`.`pcicustomdate3` AS `pcicustomdate3`,`pc`.`pcnama` AS `pcnama`,`ic`.`icnama` AS `icnama`,(case `pci`.`pcioperator` when 0 then 'Between' when 1 then '>=' when 2 then 'Multiple' else 'Unknown' end) AS `pcioperatornama` from ((`M_12_Pos_Point_Category_Item` `pci` join `m_12_pos_category` `pc` on((`pci`.`pcikategori` = `pc`.`pckode`))) join `m1_item_category` `ic` on((`pci`.`pcikategoribarang` = `ic`.`ickode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Point_Category_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pcikategori"), ""), sptField,
                     FxDB(dr("pcikategoribarang"), ""), sptField,
                     FxDB(dr("pcioperator"), ""), sptField,
                     FxDB(dr("pcijml1"), 0), sptField,
                     FxDB(dr("pcijml2"), 0), sptField,
                     FxDB(dr("pcijmlpoint"), 0), sptField,
                     FxDB(dr("pcicustomtext1"), ""), sptField,
                     FxDB(dr("pcicustomtext2"), ""), sptField,
                     FxDB(dr("pcicustomtext3"), ""), sptField,
                     FxDB(dr("pcicustomtext4"), ""), sptField,
                     FxDB(dr("pcicustomtext5"), ""), sptField,
                     FxDB(dr("pcicustomint1"), 0), sptField,
                     FxDB(dr("pcicustomint2"), 0), sptField,
                     FxDB(dr("pcicustomint3"), 0), sptField,
                     FxDB(dr("pcicustomdbl1"), 0), sptField,
                     FxDB(dr("pcicustomdbl2"), 0), sptField,
                     FxDB(dr("pcicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pcicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pcicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pcicustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("icnama"), ""), sptField,
                     FxDB(dr("pcioperatornama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Point Category Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3, pcnama, icnama, pcioperatornama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Point_Category_ItemDownload(ByVal param As String) As String
        'M12_Pos_Point_Category_ItemDownload --------------------------------------------------------
        'pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, 
        'pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, 
        'pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3

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

        dt = AmbilData("aplikasi1-M_12_Pos_Point_Category_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pcikategori"), ""), sptField,
                     FxDB(dr("pcikategoribarang"), ""), sptField,
                     FxDB(dr("pcioperator"), ""), sptField,
                     FxDB(dr("pcijml1"), 0), sptField,
                     FxDB(dr("pcijml2"), 0), sptField,
                     FxDB(dr("pcijmlpoint"), 0), sptField,
                     FxDB(dr("pcicustomtext1"), ""), sptField,
                     FxDB(dr("pcicustomtext2"), ""), sptField,
                     FxDB(dr("pcicustomtext3"), ""), sptField,
                     FxDB(dr("pcicustomtext4"), ""), sptField,
                     FxDB(dr("pcicustomtext5"), ""), sptField,
                     FxDB(dr("pcicustomint1"), 0), sptField,
                     FxDB(dr("pcicustomint2"), 0), sptField,
                     FxDB(dr("pcicustomint3"), 0), sptField,
                     FxDB(dr("pcicustomdbl1"), 0), sptField,
                     FxDB(dr("pcicustomdbl2"), 0), sptField,
                     FxDB(dr("pcicustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pcicustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pcicustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("pcicustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Point Category Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcikategori, pcikategoribarang, pcioperator, pcijml1, pcijml2, pcijmlpoint, pcicustomtext1, pcicustomtext2, pcicustomtext3, pcicustomtext4, pcicustomtext5, pcicustomint1, pcicustomint2, pcicustomint3, pcicustomdbl1, pcicustomdbl2, pcicustomdbl3, pcicustomdate1, pcicustomdate2, pcicustomdate3"))

        Return wsResult
    End Function

End Class
