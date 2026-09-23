Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_category_setting
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_Category_SettingSimpan(ByVal param As String) As String
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
        'pcskategori(0) As String, pcsmodule(1) As , pcsgrup(2) As String, pcskode(3) As String, pcsnilai(4) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pcskategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcsmodule", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcsgrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcskode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcsnilai", AsEnumTypeData.AsString)


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
            If (dataRowDetail.Length <> 5) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'pcsmodule(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - pcsmodule required numeric." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pcskategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pcskategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pcskategori should not be more than 25 character." : GoTo selesai
            End If

            'pcsgrup(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - pcsgrup can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 30 Then
                result(2) = "Row : " & i & " - pcsgrup should not be more than 30 character." : GoTo selesai
            End If

            'pcskode(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pcskode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 50 Then
                result(2) = "Row : " & i & " - pcskode should not be more than 50 character." : GoTo selesai
            End If

            'pcsnilai(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pcsnilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 500 Then
                result(2) = "Row : " & i & " - pcsnilai should not be more than 500 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pcskategori~pcsmodule~pcsgrup~pcskode~pcsnilai", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4)) = False Then
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
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(dr1("pcskategori")) & "' "

                    'BUAT QUERY
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & FixQuotes(dr1("pcskategori")) & "','" & FixQuotes(dr1("pcsmodule")) & "', '" & FixQuotes(dr1("pcsgrup")) & "', '" & FixQuotes(dr1("pcskode")) & "', '" & FixQuotes(dr1("pcsnilai")) & "')")
                Next
                'insert jika data belum ada, dan update jika data sudah ada                                                                                             pcsnilai
                sql = "Insert into M_12_Pos_Category_Setting(pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pcsnilai = VALUES(pcsnilai)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M12_Pos_Category_SettingSearch(PostWsSearch(paramSplit(0), "M12_Pos_Category_SettingSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_Category_SettingDelete(ByVal param As String) As String

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
        Dim kategori As String = "", modul As String = "", grup As String = ""
        Dim idtrans(4) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 4) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK PCSKATEGORI
            If (Len(idtrans(0)) = 0) Then
                result(2) = "pcskategori can't be empty." : GoTo selesai
            Else
                kategori = idtrans(0)
            End If
            'CEK PCSMODULE
            If (IsNumeric(idtrans(1)) = False) Then
                result(2) = "pcsmodule required numeric." : GoTo selesai
            Else
                modul = idtrans(1)
            End If
            'CEK PCSGRUP
            If (Len(idtrans(2)) = 0) Then
                result(2) = "pcsgrup can't be empty." : GoTo selesai
            Else
                grup = idtrans(2)
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
            sql = "SELECT pcskategori as kategoripos FROM M_12_Pos_Category_Setting WHERE pcskategori = '" & kategori & "' AND pcsmodule = '" & modul & "' AND pcsgrup = '" & grup & "' GROUP BY pcskategori"
            Dim dtKategoriPOS As DataTable = AsDataTableAmbilDariDB(sql)
            If dtKategoriPOS.Rows.Count > 0 Then
                For Each drKategoriPOS As DataRow In dtKategoriPOS.Rows
                    'BUAT FILTER KATEGORI POS UNTUK USER LOGIN
                    ftKategoriPOS = IIf(ftKategoriPOS.Length > 0, ftKategoriPOS & " OR ", "")
                    ftKategoriPOS &= " l.lkategoripos = '" & FixQuotes(drKategoriPOS("kategoripos")) & "' "
                Next
            End If

            'DELETE
            sql = "DELETE FROM M_12_Pos_Category_Setting WHERE pcskategori = '" & kategori & "' AND pcsmodule = '" & modul & "' AND pcsgrup = '" & grup & "'"
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
            Dim paramSearch As String = M12_Pos_Category_SettingSearch(PostWsSearch(paramSplit(0), "M12_Pos_Category_SettingSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu, kategori))
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
    Public Function M12_Pos_Category_SettingSearch(ByVal param As String) As String
        'M12_Pos_Category_SettingSearch --------------------------------------------------------
        'pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai, snama, suraian, 
        'surutan, stipedata, sjenisinputan, scombodata, pcnama, pccatatan, modulename

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
            Filter = pagingSplit(2) & " AND (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN l.lkode LIKE '%' OR l.lkode IS NULL ELSE l.lkode = uloc.lokasi END)"
            '#Taruh fungsi replace disini...
        Else
            Filter = " (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN l.lkode LIKE '%' OR l.lkode IS NULL ELSE l.lkode = uloc.lokasi END)"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''VALIDASI DAN SET IDTRANSAKSI ======================================================
        ''CEK IDTRANSAKSI
        'Dim idtransaksi As String = ""
        'If (Len(paramSplit(3)) = 0) Then
        '    result(2) = "pckode can't be empty." : GoTo selesai
        'Else
        '    'SET IDTRANSAKSI
        '    idtransaksi = paramSplit(3)
        'End If
        ''END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'PANGGIL QUERY
        'sql = "select pcs.pcskategori AS pcskategori, ifnull(pcs.pcsmodule, ps.smodule) AS pcsmodule, ifnull(pcs.pcsgrup, ps.sgrup) AS pcsgrup, ifnull(pcs.pcskode, ps.skode) AS pcskode, ifnull(pcs.pcsnilai,ps.snilai) AS pcsnilai, ps.snama AS snama, ps.suraian AS suraian, ps.surutan AS surutan, ps.stipedata AS stipedata, ps.sjenisinputan AS sjenisinputan, ps.scombodata AS scombodata, pc.pcnama AS pcnama, pc.pccatatan AS pccatatan, m.mname AS modulename from m_12_pos_setting ps join m0_module m on ps.smodule = m.mid join m_12_pos_category_setting pcs on ps.smodule = pcs.pcsmodule and ps.sgrup = pcs.pcsgrup and ps.skode = pcs.pcskode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_location uloc on ul.uluser = uloc.userid join m1_location l on uloc.lokasi = l.lkode and pcs.pcskategori = l.lkategoripos left join m_12_pos_category pc on pcs.pcskategori = pc.pckode"
        sql = "select pcs.pcskategori AS pcskategori, ifnull(pcs.pcsmodule, ps.smodule) AS pcsmodule, ifnull(pcs.pcsgrup, ps.sgrup) AS pcsgrup, ifnull(pcs.pcskode, ps.skode) AS pcskode, ifnull(pcs.pcsnilai,ps.snilai) AS pcsnilai, ps.snama AS snama, ps.suraian AS suraian, ps.surutan AS surutan, ps.stipedata AS stipedata, ps.sjenisinputan AS sjenisinputan, ps.scombodata AS scombodata, pc.pcnama AS pcnama, pc.pccatatan AS pccatatan, m.mname AS modulename from m_12_pos_setting ps join m0_module m on ps.smodule = m.mid join m_12_pos_category_setting pcs on ps.smodule = pcs.pcsmodule and ps.sgrup = pcs.pcsgrup and ps.skode = pcs.pcskode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' left join m0_user_location uloc on ul.uluser = uloc.userid left join m1_location l on uloc.lokasi = l.lkode and pcs.pcskategori = l.lkategoripos left join m_12_pos_category pc on pcs.pcskategori = pc.pckode"
        'sql = sql.Replace("valkategoripos", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Category_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "pc.pckode", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pcskategori"), ""), sptField,
                     FxDB(dr("pcsmodule"), ""), sptField,
                     FxDB(dr("pcsgrup"), ""), sptField,
                     FxDB(dr("pcskode"), ""), sptField,
                     FxDB(dr("pcsnilai"), ""), sptField,
                     FxDB(dr("snama"), ""), sptField,
                     FxDB(dr("suraian"), ""), sptField,
                     FxDB(dr("surutan"), 0), sptField,
                     FxDB(dr("stipedata"), ""), sptField,
                     FxDB(dr("sjenisinputan"), ""), sptField,
                     FxDB(dr("scombodata"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("pccatatan"), ""), sptField,
                     FxDB(dr("modulename"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Setting data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai, snama, suraian, surutan, stipedata, sjenisinputan, scombodata, pcnama, pccatatan, modulename"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Category_SettingSearchOld(ByVal param As String) As String
        'M12_Pos_Category_SettingSearch --------------------------------------------------------
        'pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai, snama, suraian, 
        'surutan, stipedata, sjenisinputan, scombodata, pcnama, pccatatan, modulename

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

        ''VALIDASI DAN SET IDTRANSAKSI ======================================================
        ''CEK IDTRANSAKSI
        'Dim idtransaksi As String = ""
        'If (Len(paramSplit(3)) = 0) Then
        '    result(2) = "pckode can't be empty." : GoTo selesai
        'Else
        '    'SET IDTRANSAKSI
        '    idtransaksi = paramSplit(3)
        'End If
        ''END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'PANGGIL QUERY
        sql = "select pcs.pcskategori AS pcskategori, ifnull(pcs.pcsmodule, ps.smodule) AS pcsmodule, ifnull(pcs.pcsgrup, ps.sgrup) AS pcsgrup, ifnull(pcs.pcskode, ps.skode) AS pcskode, ifnull(pcs.pcsnilai,ps.snilai) AS pcsnilai, ps.snama AS snama, ps.suraian AS suraian, ps.surutan AS surutan, ps.stipedata AS stipedata, ps.sjenisinputan AS sjenisinputan, ps.scombodata AS scombodata, pc.pcnama AS pcnama, pc.pccatatan AS pccatatan, m.mname AS modulename from m_12_pos_setting ps join m0_module m on ps.smodule = m.mid join m_12_pos_category_setting pcs on ps.smodule = pcs.pcsmodule and ps.sgrup = pcs.pcsgrup and ps.skode = pcs.pcskode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_location uloc on ul.uluser = uloc.userid join m1_location l on uloc.lokasi = l.lkode and pcs.pcskategori = l.lkategoripos left join m_12_pos_category pc on pcs.pcskategori = pc.pckode"
        'sql = sql.Replace("valkategoripos", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Category_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "pc.pckode", sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pcskategori"), ""), sptField,
                     FxDB(dr("pcsmodule"), ""), sptField,
                     FxDB(dr("pcsgrup"), ""), sptField,
                     FxDB(dr("pcskode"), ""), sptField,
                     FxDB(dr("pcsnilai"), ""), sptField,
                     FxDB(dr("snama"), ""), sptField,
                     FxDB(dr("suraian"), ""), sptField,
                     FxDB(dr("surutan"), 0), sptField,
                     FxDB(dr("stipedata"), ""), sptField,
                     FxDB(dr("sjenisinputan"), ""), sptField,
                     FxDB(dr("scombodata"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("pccatatan"), ""), sptField,
                     FxDB(dr("modulename"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Setting data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai, snama, suraian, surutan, stipedata, sjenisinputan, scombodata, pcnama, pccatatan, modulename"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Category_SettingGetdataById(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M12_Pos_Category_SettingGetdataById --------------------------------------------------------
        'pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai, snama, suraian, 
        'surutan, stipedata, sjenisinputan, scombodata, pcnama, modulename

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (Len(paramSplit(3)) = 0) Then
            result(2) = "pckode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(3)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'PANGGIL QUERY
        sql = "select `pcs`.`pcskategori` AS `pcskategori`,ifnull(`pcs`.`pcsmodule`,`ps`.`smodule`) AS `pcsmodule`,ifnull(`pcs`.`pcsgrup`,`ps`.`sgrup`) AS `pcsgrup`,ifnull(`pcs`.`pcskode`,`ps`.`skode`) AS `pcskode`,ifnull(`pcs`.`pcsnilai`,`ps`.`snilai`) AS `pcsnilai`,`ps`.`snama` AS `snama`,`ps`.`suraian` AS `suraian`,`ps`.`surutan` AS `surutan`,`ps`.`stipedata` AS `stipedata`,`ps`.`sjenisinputan` AS `sjenisinputan`,`ps`.`scombodata` AS `scombodata`,`pc`.`pcnama` AS `pcnama`,`m`.`mname` AS `modulename` from (((`m_12_pos_setting` `ps` join `m0_module` `m` on((`ps`.`smodule` = `m`.`mid`))) left join `m_12_pos_category_setting` `pcs` on(((`ps`.`smodule` = `pcs`.`pcsmodule`) and (`ps`.`sgrup` = `pcs`.`pcsgrup`) and (`ps`.`skode` = `pcs`.`pcskode`) and (`pcs`.`pcskategori` = 'valkategoripos')))) left join `m_12_pos_category` `pc` on((`pcs`.`pcskategori` = `pc`.`pckode`)))"
        sql = sql.Replace("valkategoripos", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Category_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pcskategori"), ""), sptField,
                     FxDB(dr("pcsmodule"), ""), sptField,
                     FxDB(dr("pcsgrup"), ""), sptField,
                     FxDB(dr("pcskode"), ""), sptField,
                     FxDB(dr("pcsnilai"), ""), sptField,
                     FxDB(dr("snama"), ""), sptField,
                     FxDB(dr("suraian"), ""), sptField,
                     FxDB(dr("surutan"), 0), sptField,
                     FxDB(dr("stipedata"), ""), sptField,
                     FxDB(dr("sjenisinputan"), ""), sptField,
                     FxDB(dr("scombodata"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptField,
                     FxDB(dr("modulename"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Setting data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai, snama, suraian, surutan, stipedata, sjenisinputan, scombodata, pcnama, modulename"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Category_LocationSearch(ByVal param As String) As String
        'M12_Pos_Category_LocationSearch --------------------------------------------------------
        'lkode, lnama, lkodetransaksi, lcabang, laktif, lalamat1, lalamat2, 
        'lkota, lkodepos, lnotelp, lnofax, lcatatan, linputuser, linputtgl, 
        'lmodifikasiuser, lmodifikasitanggal, lcabangnama, lkategoripos, pcnama

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
            Filter = pagingSplit(2) & " AND (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN l.lkode LIKE '%' OR l.lkode IS NULL ELSE l.lkode = uloc.lokasi END)"
            '#Taruh fungsi replace disini...
        Else
            Filter = " (CASE LENGTH(IFNULL(uloc.lokasi,'')) WHEN 0 THEN l.lkode LIKE '%' OR l.lkode IS NULL ELSE l.lkode = uloc.lokasi END)"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'BUAT QUERY
        'sql = "select l.lkode AS lkode, l.lnama AS lnama, l.lkodetransaksi AS lkodetransaksi, l.lcabang AS lcabang, l.laktif AS laktif, l.lalamat1 AS lalamat1, l.lalamat2 AS lalamat2, l.lkota AS lkota, l.lkodepos AS lkodepos, l.lnotelp AS lnotelp, l.lnofax AS lnofax, l.lcatatan AS lcatatan, l.linputuser AS linputuser, l.linputtgl AS linputtgl, l.lmodifikasiuser AS lmodifikasiuser, l.lmodifikasitanggal AS lmodifikasitanggal, b.bnama AS lcabangnama, l.lkategoripos AS lkategoripos, pc.pcnama AS pcnama from m1_location l join m_12_pos_category pc on l.lkategoripos = pc.pckode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_location uloc on ul.uluser = uloc.userid and l.lkode = uloc.lokasi left join m1_branch b on l.lcabang = b.bkode"
        sql = "select l.lkode AS lkode, l.lnama AS lnama, l.lkodetransaksi AS lkodetransaksi, l.lcabang AS lcabang, l.laktif AS laktif, l.lalamat1 AS lalamat1, l.lalamat2 AS lalamat2, l.lkota AS lkota, l.lkodepos AS lkodepos, l.lnotelp AS lnotelp, l.lnofax AS lnofax, l.lcatatan AS lcatatan, l.linputuser AS linputuser, l.linputtgl AS linputtgl, l.lmodifikasiuser AS lmodifikasiuser, l.lmodifikasitanggal AS lmodifikasitanggal, b.bnama AS lcabangnama, l.lkategoripos AS lkategoripos, pc.pcnama AS pcnama from m1_location l join m_12_pos_category pc on l.lkategoripos = pc.pckode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' LEFT JOIN m0_user_location uloc ON ul.uluser = uloc.userid left join m1_branch b on l.lcabang = b.bkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Location", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "l.lkode", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("lkode"), ""), sptField,
                     FxDB(dr("lnama"), ""), sptField,
                     FxDB(dr("lkodetransaksi"), ""), sptField,
                     FxDB(dr("lcabang"), ""), sptField,
                     FxDB(dr("laktif"), 0), sptField,
                     FxDB(dr("lalamat1"), ""), sptField,
                     FxDB(dr("lalamat2"), ""), sptField,
                     FxDB(dr("lkota"), ""), sptField,
                     FxDB(dr("lkodepos"), ""), sptField,
                     FxDB(dr("lnotelp"), ""), sptField,
                     FxDB(dr("lnofax"), ""), sptField,
                     FxDB(dr("lcatatan"), ""), sptField,
                     FxDB(dr("linputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("linputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lmodifikasitanggal"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lcabangnama"), ""), sptField,
                     FxDB(dr("lkategoripos"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Location data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("lkode, lnama, lkodetransaksi, lcabang, laktif, lalamat1, lalamat2, lkota, lkodepos, lnotelp, lnofax, lcatatan, linputuser, linputtgl, lmodifikasiuser, lmodifikasitanggal, lcabangnama, lkategoripos, pcnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Category_LocationSearchOld(ByVal param As String) As String
        'M12_Pos_Category_LocationSearch --------------------------------------------------------
        'lkode, lnama, lkodetransaksi, lcabang, laktif, lalamat1, lalamat2, 
        'lkota, lkodepos, lnotelp, lnofax, lcatatan, linputuser, linputtgl, 
        'lmodifikasiuser, lmodifikasitanggal, lcabangnama, lkategoripos, pcnama

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
        sql = "select l.lkode AS lkode, l.lnama AS lnama, l.lkodetransaksi AS lkodetransaksi, l.lcabang AS lcabang, l.laktif AS laktif, l.lalamat1 AS lalamat1, l.lalamat2 AS lalamat2, l.lkota AS lkota, l.lkodepos AS lkodepos, l.lnotelp AS lnotelp, l.lnofax AS lnofax, l.lcatatan AS lcatatan, l.linputuser AS linputuser, l.linputtgl AS linputtgl, l.lmodifikasiuser AS lmodifikasiuser, l.lmodifikasitanggal AS lmodifikasitanggal, b.bnama AS lcabangnama, l.lkategoripos AS lkategoripos, pc.pcnama AS pcnama from m1_location l join m_12_pos_category pc on l.lkategoripos = pc.pckode join m0_userlogin ul on ul.ulid = '" & FixQuotes(paramSplit(0)) & "' join m0_user_location uloc on ul.uluser = uloc.userid and l.lkode = uloc.lokasi left join m1_branch b on l.lcabang = b.bkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Location", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "l.lkode", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("lkode"), ""), sptField,
                     FxDB(dr("lnama"), ""), sptField,
                     FxDB(dr("lkodetransaksi"), ""), sptField,
                     FxDB(dr("lcabang"), ""), sptField,
                     FxDB(dr("laktif"), 0), sptField,
                     FxDB(dr("lalamat1"), ""), sptField,
                     FxDB(dr("lalamat2"), ""), sptField,
                     FxDB(dr("lkota"), ""), sptField,
                     FxDB(dr("lkodepos"), ""), sptField,
                     FxDB(dr("lnotelp"), ""), sptField,
                     FxDB(dr("lnofax"), ""), sptField,
                     FxDB(dr("lcatatan"), ""), sptField,
                     FxDB(dr("linputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("linputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lmodifikasitanggal"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lcabangnama"), ""), sptField,
                     FxDB(dr("lkategoripos"), ""), sptField,
                     FxDB(dr("pcnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Location data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("lkode, lnama, lkodetransaksi, lcabang, laktif, lalamat1, lalamat2, lkota, lkodepos, lnotelp, lnofax, lcatatan, linputuser, linputtgl, lmodifikasiuser, lmodifikasitanggal, lcabangnama, lkategoripos, pcnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Category_SettingDownload(ByVal param As String) As String
        'M12_Pos_Category_SettingDownload --------------------------------------------------------
        'pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai

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

        ''VALIDASI DAN SET IDTRANSAKSI ======================================================
        ''CEK IDTRANSAKSI
        'Dim idtransaksi As String = ""
        'If (Len(paramSplit(3)) = 0) Then
        '    result(2) = "pckode can't be empty." : GoTo selesai
        'Else
        '    'SET IDTRANSAKSI
        '    idtransaksi = paramSplit(3)
        'End If
        ''END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Category_Setting", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pcskategori"), ""), sptField,
                     FxDB(dr("pcsmodule"), ""), sptField,
                     FxDB(dr("pcsgrup"), ""), sptField,
                     FxDB(dr("pcskode"), ""), sptField,
                     FxDB(dr("pcsnilai"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Setting data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_Category_SettingImport(ByVal param As String) As String
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
        'pcskategori(0) As String, pcsmodule(1) As , pcsgrup(2) As String, pcskode(3) As String, pcsnilai(4) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "pcskategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcsmodule", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "pcsgrup", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcskode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "pcsnilai", AsEnumTypeData.AsString)


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
            If (dataRowDetail.Length <> 5) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'pcsmodule(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - pcsmodule required numeric." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'pcskategori(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - pcskategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 25 Then
                result(2) = "Row : " & i & " - pcskategori should not be more than 25 character." : GoTo selesai
            End If

            'pcsgrup(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - pcsgrup can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 30 Then
                result(2) = "Row : " & i & " - pcsgrup should not be more than 30 character." : GoTo selesai
            End If

            'pcskode(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - pcskode can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 50 Then
                result(2) = "Row : " & i & " - pcskode should not be more than 50 character." : GoTo selesai
            End If

            'pcsnilai(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - pcsnilai can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 500 Then
                result(2) = "Row : " & i & " - pcsnilai should not be more than 500 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "pcskategori~pcsmodule~pcsgrup~pcskode~pcsnilai", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4)) = False Then
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
                    strValue2.Append("('" & FixQuotes(dr1("pcskategori")) & "','" & FixQuotes(dr1("pcsmodule")) & "', '" & FixQuotes(dr1("pcsgrup")) & "', '" & FixQuotes(dr1("pcskode")) & "', '" & FixQuotes(dr1("pcsnilai")) & "')")
                Next

                If Len(strValue2.ToString) > 0 Then
                    'DELETE
                    sql = "DELETE FROM M_12_Pos_Category_Setting"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'insert jika data belum ada, dan update jika data sudah ada                                                                                             pcsnilai
                    sql = "Insert into M_12_Pos_Category_Setting(pcskategori, pcsmodule, pcsgrup, pcskode, pcsnilai) values" & strValue2.ToString & " ON DUPLICATE KEY UPDATE pcsnilai = VALUES(pcsnilai)"
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
            Dim paramSearch As String = M12_Pos_Category_SettingSearch(PostWsSearch(paramSplit(0), "M12_Pos_Category_SettingSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

End Class
