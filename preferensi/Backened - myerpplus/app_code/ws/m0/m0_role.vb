Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_role
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_RoleSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataMenu(), dataRowMenu(), dataReport(), dataRowReport(), dataCustom(), dataRowCustom(), dataItemCategory(), dataRowItemCategory() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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
        If (dataSplit.Length <> 4 And dataSplit.Length <> 5) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'rkode(0) As String, rnama(1) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rkode, rnama

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 2) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rkode(0) As String
        If (Len(dataUtama(0)) = 0) Then
            result(2) = "rkode can't be empty." : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "rkode should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA ===============================================================
        'rnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rnama can't be empty." : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rnama should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ========================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnama", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rkode~rnama", dataUtama(0) & "~" & dataUtama(1)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA MENU -------------------------------------------------------
        'rmmoduleid(0) As Integer, rmmenuid(1) As Integer, rmrole(2) As String, rmakses(3) As String, rmfavourite(4) As Integer

        'MAPPING BUAT FLEX DATA MENU -----------------------------------------------------
        'rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite

        'Buat datatable menu
        Dim dtmenu As New DataTable
        AsDataTableTambahField(dtmenu, "rmmoduleid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmenu, "rmmenuid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmenu, "rmrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmenu, "rmakses", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmenu, "rmfavourite", AsEnumTypeData.AsInt64)

        If (Len(dataSplit(1)) > 0) Then

            'SPLIT PARAMETER DATA MENU
            dataMenu = dataSplit(1).Split(sptRow)

            'VALIDASI DAN SET DATA ROW MENU ==================================================
            Dim JmlDtMenu As Integer = dataMenu.Length
            For i = 1 To JmlDtMenu
                'SPLIT DATA MENU
                dataRowMenu = dataMenu(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA MENU -----------------------------------
                'CEK ARRAY DATA MENU
                If (dataRowMenu.Length <> 5) Then
                    result(2) = "Menu Row : " & i & " - Invalid detail data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW MENU ----------------------------

                'VALIDASI TIPE DATA MENU ------------------------------------------
                'rmmoduleid(0) As Integer
                If (IsNumeric(dataRowMenu(0)) = False) Then
                    result(2) = "Menu Row : " & i & " - rmmoduleid required numeric." : GoTo selesai
                End If
                'rmmenuid(1) As Integer
                If (IsNumeric(dataRowMenu(1)) = False) Then
                    result(2) = "Menu Row : " & i & " - rmmenuid required numeric." : GoTo selesai
                End If
                ''rmrole(2) As String
                'If (IsNumeric(dataRowMenu(2)) = False) Then
                '    result(2) = "Menu Row : " & i & " - rmrole required numeric." : GoTo selesai
                'End If
                'rmfavourite(4) As Integer
                If (IsNumeric(dataRowMenu(4)) = False) Then
                    result(2) = "Menu Row : " & i & " - rmfavourite required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA MENU -----------------------------------

                'VALIDASI DATA MENU ---------------------------------------
                'rmrole(2) As String
                If Len(dataRowMenu(2)) = 0 Then
                    result(2) = "Menu Row : " & i & " - rmrole can't be empty" : GoTo selesai
                End If
                If (Len(dataRowMenu(2)) > 25) Then
                    result(2) = "Menu Row : " & i & " - rmrole should not be more than 25 character." : GoTo selesai
                End If

                'rmakses(3) As String
                If Len(dataRowMenu(3)) = 0 Then
                    result(2) = "Menu Row : " & i & " - rmakses can't be empty" : GoTo selesai
                End If
                If Len(dataRowMenu(3)) > 25 Then
                    result(2) = "Menu Row : " & i & " - rmakses should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA MENU --------------------------------

                If AsDataTableTambahData(dtmenu, "rmmoduleid~rmmenuid~rmrole~rmakses~rmfavourite", dataRowMenu(0) & "~" & dataRowMenu(1) & "~" & dataRowMenu(2) & "~" & dataRowMenu(3) & "~" & dataRowMenu(4)) = False Then
                    result(2) = "Menu Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA MENU ===========================================

        End If

        'MAPPING BUAT WS DATA REPORT -------------------------------------------------------
        'rrmoduleid(0) As Integer, rrmenuid(1) As Integer, rritem(2) As Integer, rrrole(3) As String, rrakses(4) As String

        'MAPPING BUAT FLEX DATA REPORT -----------------------------------------------------
        'rrmoduleid, rrmenuid, rritem, rrrole, rrakses

        'Buat datatable report
        Dim dtreport As New DataTable
        AsDataTableTambahField(dtreport, "rrmoduleid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtreport, "rrmenuid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtreport, "rritem", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtreport, "rrrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtreport, "rrakses", AsEnumTypeData.AsString)

        If (Len(dataSplit(2)) > 0) Then

            'SPLIT PARAMETER DATA REPORT
            dataReport = dataSplit(2).Split(sptRow)

            'VALIDASI DAN SET DATA ROW REPORT ==================================================
            Dim JmlDtReport As Integer = dataReport.Length
            For i = 1 To JmlDtReport
                'SPLIT DATA REPORT
                dataRowReport = dataReport(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA REPORT -----------------------------------
                'CEK ARRAY DATA REPORT
                If (dataRowReport.Length <> 5) Then
                    result(2) = "Report Row : " & i & " - Invalid report data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW REPORT ----------------------------

                'VALIDASI TIPE DATA REPORT ------------------------------------------
                'rrmoduleid(0) As Integer
                If (IsNumeric(dataRowReport(0)) = False) Then
                    result(2) = "Report Row : " & i & " - rrmoduleid required numeric." : GoTo selesai
                End If
                'rrmenuid(1) As Integer
                If (IsNumeric(dataRowReport(1)) = False) Then
                    result(2) = "Report Row : " & i & " - rrmenuid required numeric." : GoTo selesai
                End If
                'rritem(2) As Integer
                If (IsNumeric(dataRowReport(2)) = False) Then
                    result(2) = "Report Row : " & i & " - rritem required numeric." : GoTo selesai
                End If
                ''rrrole(3) As Integer
                'If (IsNumeric(dataRowReport(3)) = False) Then
                '    result(2) = "Report Row : " & i & " - rrrole required numeric." : GoTo selesai
                'End If
                'END OF VALIDASI TIPE DATA REPORT -----------------------------------

                'VALIDASI DATA REPORT ---------------------------------------
                'rrrole(3) As String
                If Len(dataRowReport(3)) = 0 Then
                    result(2) = "Report Row : " & i & " - rrrole can't be empty" : GoTo selesai
                End If
                If Len(dataRowReport(3)) > 25 Then
                    result(2) = "Report Row : " & i & " - rrrole should not be more than 25 character." : GoTo selesai
                End If

                'rrakses(4) As String
                If Len(dataRowReport(4)) = 0 Then
                    result(2) = "Report Row : " & i & " - rrakses can't be empty" : GoTo selesai
                End If
                If Len(dataRowReport(4)) > 25 Then
                    result(2) = "Report Row : " & i & " - rrakses should not be more than 25 character." : GoTo selesai
                End If

                'END OF VALIDASI DATA REPORT --------------------------------

                If AsDataTableTambahData(dtreport, "rrmoduleid~rrmenuid~rritem~rrrole~rrakses", dataRowReport(0) & "~" & dataRowReport(1) & "~" & dataRowReport(2) & "~" & dataRowReport(3) & "~" & dataRowReport(4)) = False Then
                    result(2) = "Report Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA REPORT ===========================================

        End If

        'MAPPING BUAT WS DATA CUSTOM -------------------------------------------------------
        'rcmoduleid(0) As Integer, rcidpc(1) As Integer, rcrole(2) As Integer, rcakses(3) As String

        'MAPPING BUAT FLEX DATA CUSTOM -----------------------------------------------------
        'rcmoduleid, rcidpc, rcrole, rcakses

        'Buat datatable custom
        Dim dtcustom As New DataTable
        AsDataTableTambahField(dtcustom, "rcmoduleid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcustom, "rcidpc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcustom, "rcrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcustom, "rcakses", AsEnumTypeData.AsString)

        If (Len(dataSplit(3)) > 0) Then

            'SPLIT PARAMETER DATA CUSTOM
            dataCustom = dataSplit(3).Split(sptRow)

            'VALIDASI DAN SET DATA ROW CUSTOM ==================================================
            Dim JmlDtCustom As Integer = dataCustom.Length
            For i = 1 To JmlDtCustom
                'SPLIT DATA CUSTOM
                dataRowCustom = dataCustom(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA CUSTOM -----------------------------------
                'CEK ARRAY DATA CUSTOM
                If (dataRowCustom.Length <> 4) Then
                    result(2) = "Custom Row : " & i & " - Invalid custom data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW CUSTOM ----------------------------

                'VALIDASI TIPE DATA CUSTOM ------------------------------------------
                'rcmoduleid(0) As Integer
                If (IsNumeric(dataRowCustom(0)) = False) Then
                    result(2) = "Custom Row : " & i & " - rcmoduleid required numeric." : GoTo selesai
                End If
                'rcidpc(1) As Integer
                If (IsNumeric(dataRowCustom(1)) = False) Then
                    result(2) = "Custom Row : " & i & " - rcidpc required numeric." : GoTo selesai
                End If
                ''rcrole(2) As Integer
                'If (IsNumeric(dataRowCustom(2)) = False) Then
                '    result(2) = "Custom Row : " & i & " - rcrole required numeric." : GoTo selesai
                'End If
                'END OF VALIDASI TIPE DATA CUSTOM -----------------------------------

                'VALIDASI DATA CUSTOM ---------------------------------------
                'rcrole(2) As String
                If Len(dataRowCustom(2)) = 0 Then
                    result(2) = "Custom Row : " & i & " - rcrole required numeric." : GoTo selesai
                End If
                If Len(dataRowCustom(2)) > 25 Then
                    result(2) = "Custom Row : " & i & " - rcrole should not be more than 25 character." : GoTo selesai
                End If

                'rcakses(3) As String
                If Len(dataRowCustom(3)) = 0 Then
                    result(2) = "Custom Row : " & i & " - rcakses can't be empty" : GoTo selesai
                End If
                If Len(dataRowCustom(3)) > 25 Then
                    result(2) = "Custom Row : " & i & " - rcakses should not be more than 25 character." : GoTo selesai
                End If

                'END OF VALIDASI DATA CUSTOM --------------------------------

                If AsDataTableTambahData(dtcustom, "rcmoduleid~rcidpc~rcrole~rcakses", dataRowCustom(0) & "~" & dataRowCustom(1) & "~" & dataRowCustom(2) & "~" & dataRowCustom(3)) = False Then
                    result(2) = "Custom Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA CUSTOM ===========================================

        End If


        'MAPPING BUAT WS DATA ITEM CATEGORY -------------------------------------------------------
        'ricrole(0) As String, rickategoribarang(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'ricrole, rickategoribarang

        'Buat datatable item category
        Dim dtItemCategory As New DataTable
        AsDataTableTambahField(dtItemCategory, "ricrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtItemCategory, "rickategoribarang", AsEnumTypeData.AsString)

        'CEK PARAMETER DATA ASSET
        If dataSplit.Length > 4 Then
            If dataSplit(4).Length > 0 Then

                'VALIDASI DAN SET DATA ItemCategory ======================================================
                'SPLIT PARAMETER DATA ItemCategory
                dataItemCategory = dataSplit(4).Split(sptRow)
                'END OF VALIDASI DAN SET DATA ItemCategory ===============================================

                'VALIDASI DAN SET DATA ROW ItemCategory ==================================================
                Dim JmlDtItemCategory As Integer = dataItemCategory.Length
                For i = 1 To JmlDtItemCategory
                    'SPLIT DATA ItemCategory
                    dataRowItemCategory = dataItemCategory(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA ItemCategory -----------------------------------
                    'CEK ARRAY DATA ItemCategory
                    If (dataRowItemCategory.Length <> 2) Then
                        result(2) = "Item Category Row : " & i & " - Invalid ItemCategory transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW ItemCategory ----------------------------

                    'VALIDASI TIPE DATA ItemCategory ------------------------------------------
                    'END OF VALIDASI TIPE DATA ItemCategory -----------------------------------

                    'VALIDASI DATA ItemCategory ---------------------------------------
                    'ricrole(0) As String
                    If Len(dataRowItemCategory(0)) = 0 Then
                        result(2) = "Item Category Row : " & i & " - ricrole can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowItemCategory(0)) > 25 Then
                        result(2) = "Item Category Row : " & i & " - ricrole should not be more than 25 character." : GoTo selesai
                    End If

                    'rickategoribarang(1) As String
                    If Len(dataRowItemCategory(1)) = 0 Then
                        result(2) = "Item Category Row : " & i & " - rickategoribarang can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowItemCategory(1)) > 50 Then
                        result(2) = "Item Category Row : " & i & " - rickategoribarang should not be more than 50 character." : GoTo selesai
                    End If

                    'END OF VALIDASI DATA ItemCategory --------------------------------

                    If AsDataTableTambahData(dtItemCategory, "ricrole~rickategoribarang", dataRowItemCategory(0) & "~" & dataRowItemCategory(1)) = False Then
                        result(2) = "Item Category Row : " & i & " - insert into datatable failed." : GoTo selesai
                    End If

                Next
                'END OF VALIDASI DAN SET ROW DATA ItemCategory ===========================================

            End If
        End If

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
                Dim dr1 As DataRow = dtutama.Rows(0)
                If isUpdate Then
                    result(4) = dr1("rkode")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rkode) FROM M0_Role WHERE rkode ='" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        sql = "Update M0_Role set rnama  = '" & FixQuotes(dr1("rnama")) & "' WHERE rkode ='" & result(4) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                Else
                    result(4) = dr1("rkode")
                    sql = "Insert into M0_Role (rkode, rnama) values('" & dr1("rkode") & "', '" & FixQuotes(dr1("rnama")) & "')"
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
                result(2) = "Main Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            'Hapus menu ketika update
            If (isUpdate) Then
                sql = "Delete from M0_Role_Menu where rmrole = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Hapus report ketika update
            If (isUpdate) Then
                sql = "Delete from M0_Role_Report where rrrole = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Hapus custom ketika update
            If (isUpdate) Then
                sql = "Delete from M0_Role_Custom where rcrole = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Hapus itemcategory ketika update
            If (isUpdate) Then
                sql = "Delete from M0_Role_Item_category where ricrole = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses menu
            If (dtmenu.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtmenu.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & dr1("rmmoduleid") & ", " & dr1("rmmenuid") & ", '" & result(4) & "', '" & FixQuotes(dr1("rmakses")) & "', " & dr1("rmfavourite") & ")")
                Next
                sql = "Insert into M0_Role_Menu(rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite) values" & strValue2.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses report
            If (dtreport.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtreport.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & dr1("rrmoduleid") & ", " & dr1("rrmenuid") & ", " & dr1("rritem") & ", '" & result(4) & "', '" & FixQuotes(dr1("rrakses")) & "')")
                Next
                sql = "Insert into M0_Role_Report(rrmoduleid, rrmenuid, rritem, rrrole, rrakses) values" & strValue2.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses custom
            If (dtcustom.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtcustom.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & dr1("rcmoduleid") & ", " & dr1("rcidpc") & ", '" & result(4) & "', '" & FixQuotes(dr1("rcakses")) & "')")
                Next
                sql = "Insert into M0_Role_Custom(rcmoduleid, rcidpc, rcrole, rcakses) values" & strValue2.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses Item Category
            If (dtItemCategory.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtItemCategory.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("('" & result(4) & "', '" & FixQuotes(dr1("rickategoribarang")) & "')")
                Next
                sql = "Insert into M0_Role_Item_Category(ricrole, rickategoribarang) values" & strValue2.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M0_RoleSearch(PostWsSearch(paramSplit(0), "M0_RoleSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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
    Public Function M0_RoleDelete(ByVal param As String) As String

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
        Dim Filter As String = "", Sorting As String = ""
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim dt As New DataTable

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
            result(2) = "rkode can't be empty." : GoTo selesai
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

            'CEK ROLE SUDAH TERKAIT DENGAN USER ATAU BELUM
            sql = "SELECT GROUP_CONCAT(CONCAT(' ' ,u.unama)) as terkait FROM m0_user_role ur JOIN m0_user u ON ur.userid = u.userid WHERE ur.role = '" & idtransaksi & "' GROUP BY ur.role"
            Dim dtCek As DataTable = AsDataTableAmbilDariDB(sql)
            If dtCek.Rows.Count > 0 Then
                If Len(dtCek.Rows(0)(0)) > 0 Then
                    result(2) = "Can't delete Role " & idtransaksi & ", it has been used by the following users : " & dtCek.Rows(0)(0) & "" : Trans.Rollback() : GoTo selesai
                End If
            End If

            'DELETE ITEM CATEGORY
            sql = "DELETE FROM M0_Role_Item_Category WHERE ricrole = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE CUSTOM
            sql = "DELETE FROM M0_Role_Custom WHERE rcrole = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE REPORT
            sql = "DELETE FROM M0_Role_Report WHERE rrrole = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE MENU
            sql = "DELETE FROM M0_Role_Menu WHERE rmrole = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M0_Role WHERE rkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M0_RoleSearch(PostWsSearch(paramSplit(0), "M0_RoleSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M0_RoleSearch(ByVal param As String) As String
        'M0_RoleSearch --------------------------------------------------------
        'rkode, rnama

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

        dt = AmbilData("aplikasi1-M0_Role", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rkode"), 0), sptField,
                     FxDB(dr("rnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Role data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkode, rnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_RoleGetdataById(ByVal param As String) As String

        'M0_RoleGetdataById Utama --------------------------------------------------------
        'rkode, rnama

        'M0_RoleGetdataById Menu ---------------------------------------------------------
        'rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite

        'M0_RoleGetdataById Report ---------------------------------------------------------
        'rrmoduleid, rrmenuid, rritem, rrrole, rrakses

        'M0_RoleGetdataById Custom ---------------------------------------------------------
        'rcmoduleid, rcidpc, rcrole, rcakses

        'M0_RoleGetdataById Item Category ---------------------------------------------------------
        'ricrole, rickategoribarang, icnama

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
        Dim Filter As String = "", Sorting As String = "", idtransaksi As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", menu As String = "", report As String = "", custom As String = "", itemcategory As String = ""

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
        If (Len(paramSplit(3)) = 0) Then
            result(2) = "rkode can't be empty." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M0_Role~M0_Role_Menu-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rkode = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter = "rkode = '" & idtransaksi & "' and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        dt = AmbilData(NmMemcached, Filter, "rmmoduleid ASC, rmmenuid ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , "SELECT * FROM M0_Role r JOIN M0_Role_Menu rm ON r.rkode=rm.rmrole")

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rkode"), 0), sptField,
                            FxDB(drutama("rnama"), ""))

            For Each dr As DataRow In dt.Rows
                menu = String.Concat(menu, FxDB(dr("rmmoduleid"), 0), sptField,
                                FxDB(dr("rmmenuid"), 0), sptField,
                                FxDB(dr("rmrole"), 0), sptField,
                                FxDB(dr("rmakses"), ""), sptField,
                                FxDB(dr("rmfavourite"), 0), sptRow)
            Next
            If menu.Length > sptRow.Length Then menu = menu.Substring(0, menu.Length - sptRow.Length)

            'AMBIL DATA REPORT
            Dim dtreport As New DataTable
            dtreport = AmbilData("aplikasi1-M0_Role_Report", "rrrole='" & idtransaksi & "'", "rrmoduleid ASC, rrmenuid ASC, rritem ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtreport.Rows
                report = String.Concat(report,
                     FxDB(dr("rrmoduleid"), 0), sptField,
                     FxDB(dr("rrmenuid"), 0), sptField,
                     FxDB(dr("rritem"), 0), sptField,
                     FxDB(dr("rrrole"), 0), sptField,
                     FxDB(dr("rrakses"), ""), sptRow)
            Next
            If report.Length > sptRow.Length Then report = report.Substring(0, report.Length - sptRow.Length)

            'AMBIL DATA CUSTOM
            Dim dtcustom As New DataTable
            dtcustom = AmbilData("aplikasi1-M0_Role_Custom", "rcrole='" & idtransaksi & "'", "rcmoduleid ASC, rcidpc ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcustom.Rows
                custom = String.Concat(custom,
                     FxDB(dr("rcmoduleid"), 0), sptField,
                     FxDB(dr("rcidpc"), 0), sptField,
                     FxDB(dr("rcrole"), 0), sptField,
                     FxDB(dr("rcakses"), ""), sptRow)
            Next
            If custom.Length > sptRow.Length Then custom = custom.Substring(0, custom.Length - sptRow.Length)

            'AMBIL DATA ITEM CATEGORY
            Dim dtitemcategory As New DataTable
            sql = "SELECT ric.ricrole, ric.rickategoribarang, ic.icnama FROM m0_role_item_category ric JOIN m1_item_category ic ON ric.rickategoribarang = ic.ickode"
            dtitemcategory = AmbilData("aplikasi1-M0_Role_Item_Category", "ricrole='" & idtransaksi & "'", "ricrole ASC, rickategoribarang ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtitemcategory.Rows
                itemcategory = String.Concat(itemcategory,
                     FxDB(dr("ricrole"), ""), sptField,
                     FxDB(dr("rickategoribarang"), ""), sptField,
                     FxDB(dr("icnama"), ""), sptRow)
            Next
            If itemcategory.Length > sptRow.Length Then itemcategory = itemcategory.Substring(0, itemcategory.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, menu, sptSubParam, report, sptSubParam, custom, sptSubParam, itemcategory)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkode, rnama"), sptSubParam, ReplaceMapping("rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite"), sptSubParam, ReplaceMapping("rrmoduleid, rrmenuid, rritem, rrrole, rrakses"), sptSubParam, ReplaceMapping("rcmoduleid, rcidpc, rcrole, rcakses"), sptSubParam, ReplaceMapping("ricrole, rickategoribarang, icnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_RoleGetSetting(ByVal param As String) As String
        'M0_RoleGetSetting ---------------------------------
        'M0_MenuByLanguage, M0_Report_VSearch, M0_Permissions_CustomSearch

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim Sorting As String = "", Bahasa As String = "", strMenuLang As String = "", strReportV As String = "", strPermCustom As String = ""

        Dim srtSplit As String = ""
        Dim arrMenuLang() As String = srtSplit.Split(""), arrReportV() As String = srtSplit.Split(""), arrpPermCustom() As String = srtSplit.Split("")

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

        'SET BAHASA
        If (pagingSplit(2).Length > 0) Then
            Bahasa = pagingSplit(2)
            '#Taruh fungsi replace disini...
        Else
            result(2) = "Language can't be empty." : GoTo selesai
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'AMBIL M0_MenuByLanguage =======================================================
        Dim wsM0_Menu_Lang As New wsM0_Menu
        arrMenuLang = wsM0_Menu_Lang.M0_MenuByLanguage(paramSplit(0) & "★M0_MenuByLanguage★0△0△" & Bahasa & "△mnmoduleid, mnurutan△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strMenuLang = arrMenuLang(2)
        'END OF AMBIL M0_MenuByLanguage ================================================

        'AMBIL M0_Report_VSearch =======================================================
        Dim wsM0_Report As New m0_report
        arrReportV = wsM0_Report.M0_Report_VSearch(paramSplit(0) & "★M0_Report_VSearch★0△0△" & Bahasa & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strReportV = arrReportV(2)
        'END OF AMBIL M0_Report_VSearch ================================================

        'AMBIL M0_MenuByLanguage =======================================================
        Dim wsM0_Permissions_Custom As New m0_permissions_custom
        arrpPermCustom = wsM0_Permissions_Custom.M0_Permissions_CustomSearch(paramSplit(0) & "★M0_Permissions_CustomSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strPermCustom = arrpPermCustom(2)
        'END OF AMBIL M0_MenuByLanguage ================================================

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strMenuLang, sptLogin, strReportV, sptLogin, strPermCustom)

        If result(1) = 1 Then
            If arrMenuLang.Length > 2 And arrReportV.Length > 2 And arrpPermCustom.Length > 2 Then wsResult = String.Concat(wsResult, sptParam, arrMenuLang(3), sptLogin, arrReportV(3), sptLogin, arrpPermCustom(3))
        End If

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_RoleDownload(ByVal param As String) As String
        'M0_RoleDownload --------------------------------------------------------
        'Utama
        'rkode, rnama

        'Menu
        'rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite

        'Report
        'rrmoduleid, rrmenuid, rritem, rrrole, rrakses

        'Custom
        'rcmoduleid, rcidpc, rcrole, rcakses

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim menu As String = "", report As String = "", custom As String = ""

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

        'AMBIL DATA ROLE
        sql = "SELECT r.rkode, r.rnama FROM m0_role r JOIN m0_user_role ur ON r.rkode = ur.role JOIN m0_user u ON ur.userid = u.userid"
        dt = AmbilData("aplikasi1-M0_Role", Filter, "r.rkode", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "r.rkode", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rkode"), ""), sptField,
                     FxDB(dr("rnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)


            'AMBIL DATA MENU
            sql = "SELECT rm.rmmoduleid, rm.rmmenuid, rm.rmrole, rm.rmakses, rm.rmfavourite FROM m0_role r JOIN m0_user_role ur ON r.rkode = ur.role JOIN m0_user u ON ur.userid = u.userid JOIN m0_role_menu rm ON r.rkode = rm.rmrole"
            Dim dtmenu As New DataTable
            dtmenu = AmbilData("aplikasi1-M0_Role_Menu", Filter, "rm.rmrole, rm.rmmoduleid, rm.rmmenuid", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "rm.rmrole, rm.rmmoduleid, rm.rmmenuid", sql) ' Ambil data ke databases
            For Each dr As DataRow In dtmenu.Rows
                menu = String.Concat(menu,
                     FxDB(dr("rmmoduleid"), ""), sptField,
                     FxDB(dr("rmmenuid"), ""), sptField,
                     FxDB(dr("rmrole"), ""), sptField,
                     FxDB(dr("rmakses"), ""), sptField,
                     FxDB(dr("rmfavourite"), 0), sptRow)
            Next
            If menu.Length > 0 Then menu = menu.Substring(0, menu.Length - sptRow.Length) Else menu = menu


            'AMBIL DATA REPORT
            sql = "SELECT rr.rrmoduleid, rr.rrmenuid, rr.rritem, rr.rrrole, rr.rrakses FROM m0_role r JOIN m0_user_role ur ON r.rkode = ur.role JOIN m0_user u ON ur.userid = u.userid JOIN m0_role_report rr ON r.rkode = rr.rrrole"
            Dim dtreport As New DataTable
            dtreport = AmbilData("aplikasi1-M0_Role_Report", Filter, "rr.rrrole, rr.rrmoduleid, rr.rrmenuid, rr.rritem", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "rr.rrrole, rr.rrmoduleid, rr.rrmenuid, rr.rritem", sql) ' Ambil data ke databases
            For Each dr As DataRow In dtreport.Rows
                report = String.Concat(report,
                     FxDB(dr("rrmoduleid"), ""), sptField,
                     FxDB(dr("rrmenuid"), ""), sptField,
                     FxDB(dr("rritem"), 0), sptField,
                     FxDB(dr("rrrole"), ""), sptField,
                     FxDB(dr("rrakses"), ""), sptRow)
            Next
            If report.Length > 0 Then report = report.Substring(0, report.Length - sptRow.Length) Else report = report


            'AMBIL DATA CUSTOM
            sql = "SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_role r JOIN m0_user_role ur ON r.rkode = ur.role JOIN m0_user u ON ur.userid = u.userid JOIN m0_role_custom rc ON r.rkode = rc.rcrole"
            Dim dtcustom As New DataTable
            dtcustom = AmbilData("aplikasi1-M0_Role_Custom", Filter, "rc.rcrole, rc.rcmoduleid, rc.rcidpc", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "rc.rcrole, rc.rcmoduleid, rc.rcidpc", sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcustom.Rows
                custom = String.Concat(custom,
                     FxDB(dr("rcmoduleid"), ""), sptField,
                     FxDB(dr("rcidpc"), ""), sptField,
                     FxDB(dr("rcrole"), ""), sptField,
                     FxDB(dr("rcakses"), ""), sptRow)
            Next
            If custom.Length > 0 Then custom = custom.Substring(0, custom.Length - sptRow.Length) Else custom = custom


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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, menu, sptSubParam, report, sptSubParam, custom)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkode, rnama" & sptSubParam & "rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite" & sptSubParam & "rrmoduleid, rrmenuid, rritem, rrrole, rrakses" & sptSubParam & "rcmoduleid, rcidpc, rcrole, rcakses"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_RoleImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataRowUtama(), dataMenu(), dataRowMenu(), dataReport(), dataRowReport(), dataCustom(), dataRowCustom() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim dt As New DataTable

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
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'rkode(0) As String, rnama(1) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rkode, rnama

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnama", AsEnumTypeData.AsString)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 2) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'rkode(0) As String
            If (Len(dataRowUtama(0)) = 0) Then
                result(2) = "Main Row : " & i & " - rkode can't be empty." : GoTo selesai
            End If
            If Len(dataRowUtama(0)) > 25 Then
                result(2) = "Main Row : " & i & " - rkode should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA ===============================================================
            'rnama(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - rnama can't be empty." : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - rnama should not be more than 25 character." : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ========================================================


            If AsDataTableTambahData(dtutama, "rkode~rnama", dataRowUtama(0) & "~" & dataRowUtama(1)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA MENU -------------------------------------------------------
        'rmmoduleid(0) As Integer, rmmenuid(1) As Integer, rmrole(2) As String, rmakses(3) As String, rmfavourite(4) As Integer

        'MAPPING BUAT FLEX DATA MENU -----------------------------------------------------
        'rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite

        'Buat datatable menu
        Dim dtmenu As New DataTable
        AsDataTableTambahField(dtmenu, "rmmoduleid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmenu, "rmmenuid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtmenu, "rmrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmenu, "rmakses", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtmenu, "rmfavourite", AsEnumTypeData.AsInt64)

        If (Len(dataSplit(1)) > 0) Then

            'SPLIT PARAMETER DATA MENU
            dataMenu = dataSplit(1).Split(sptRow)

            'VALIDASI DAN SET DATA ROW MENU ==================================================
            Dim JmlDtMenu As Integer = dataMenu.Length
            For i = 1 To JmlDtMenu
                'SPLIT DATA MENU
                dataRowMenu = dataMenu(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA MENU -----------------------------------
                'CEK ARRAY DATA MENU
                If (dataRowMenu.Length <> 5) Then
                    result(2) = "Menu Row : " & i & " - Invalid detail data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW MENU ----------------------------

                'VALIDASI TIPE DATA MENU ------------------------------------------
                'rmmoduleid(0) As Integer
                If (IsNumeric(dataRowMenu(0)) = False) Then
                    result(2) = "Menu Row : " & i & " - rmmoduleid required numeric." : GoTo selesai
                End If
                'rmmenuid(1) As Integer
                If (IsNumeric(dataRowMenu(1)) = False) Then
                    result(2) = "Menu Row : " & i & " - rmmenuid required numeric." : GoTo selesai
                End If
                ''rmrole(2) As String
                'If (IsNumeric(dataRowMenu(2)) = False) Then
                '    result(2) = "Menu Row : " & i & " - rmrole required numeric." : GoTo selesai
                'End If
                'rmfavourite(4) As Integer
                If (IsNumeric(dataRowMenu(4)) = False) Then
                    result(2) = "Menu Row : " & i & " - rmfavourite required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA MENU -----------------------------------

                'VALIDASI DATA MENU ---------------------------------------
                'rmrole(2) As String
                If Len(dataRowMenu(2)) = 0 Then
                    result(2) = "Menu Row : " & i & " - rmrole can't be empty" : GoTo selesai
                End If
                If (Len(dataRowMenu(2)) > 25) Then
                    result(2) = "Menu Row : " & i & " - rmrole should not be more than 25 character." : GoTo selesai
                End If

                'rmakses(3) As String
                If Len(dataRowMenu(3)) = 0 Then
                    result(2) = "Menu Row : " & i & " - rmakses can't be empty" : GoTo selesai
                End If
                If Len(dataRowMenu(3)) > 25 Then
                    result(2) = "Menu Row : " & i & " - rmakses should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA MENU --------------------------------

                If AsDataTableTambahData(dtmenu, "rmmoduleid~rmmenuid~rmrole~rmakses~rmfavourite", dataRowMenu(0) & "~" & dataRowMenu(1) & "~" & dataRowMenu(2) & "~" & dataRowMenu(3) & "~" & dataRowMenu(4)) = False Then
                    result(2) = "Menu Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA MENU ===========================================

        End If

        'MAPPING BUAT WS DATA REPORT -------------------------------------------------------
        'rrmoduleid(0) As Integer, rrmenuid(1) As Integer, rritem(2) As Integer, rrrole(3) As String, rrakses(4) As String

        'MAPPING BUAT FLEX DATA REPORT -----------------------------------------------------
        'rrmoduleid, rrmenuid, rritem, rrrole, rrakses

        'Buat datatable report
        Dim dtreport As New DataTable
        AsDataTableTambahField(dtreport, "rrmoduleid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtreport, "rrmenuid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtreport, "rritem", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtreport, "rrrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtreport, "rrakses", AsEnumTypeData.AsString)

        If (Len(dataSplit(2)) > 0) Then

            'SPLIT PARAMETER DATA REPORT
            dataReport = dataSplit(2).Split(sptRow)

            'VALIDASI DAN SET DATA ROW REPORT ==================================================
            Dim JmlDtReport As Integer = dataReport.Length
            For i = 1 To JmlDtReport
                'SPLIT DATA REPORT
                dataRowReport = dataReport(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA REPORT -----------------------------------
                'CEK ARRAY DATA REPORT
                If (dataRowReport.Length <> 5) Then
                    result(2) = "Report Row : " & i & " - Invalid report data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW REPORT ----------------------------

                'VALIDASI TIPE DATA REPORT ------------------------------------------
                'rrmoduleid(0) As Integer
                If (IsNumeric(dataRowReport(0)) = False) Then
                    result(2) = "Report Row : " & i & " - rrmoduleid required numeric." : GoTo selesai
                End If
                'rrmenuid(1) As Integer
                If (IsNumeric(dataRowReport(1)) = False) Then
                    result(2) = "Report Row : " & i & " - rrmenuid required numeric." : GoTo selesai
                End If
                'rritem(2) As Integer
                If (IsNumeric(dataRowReport(2)) = False) Then
                    result(2) = "Report Row : " & i & " - rritem required numeric." : GoTo selesai
                End If
                ''rrrole(3) As Integer
                'If (IsNumeric(dataRowReport(3)) = False) Then
                '    result(2) = "Report Row : " & i & " - rrrole required numeric." : GoTo selesai
                'End If
                'END OF VALIDASI TIPE DATA REPORT -----------------------------------

                'VALIDASI DATA REPORT ---------------------------------------
                'rrrole(3) As String
                If Len(dataRowReport(3)) = 0 Then
                    result(2) = "Report Row : " & i & " - rrrole can't be empty" : GoTo selesai
                End If
                If Len(dataRowReport(3)) > 25 Then
                    result(2) = "Report Row : " & i & " - rrrole should not be more than 25 character." : GoTo selesai
                End If

                'rrakses(4) As String
                If Len(dataRowReport(4)) = 0 Then
                    result(2) = "Report Row : " & i & " - rrakses can't be empty" : GoTo selesai
                End If
                If Len(dataRowReport(4)) > 25 Then
                    result(2) = "Report Row : " & i & " - rrakses should not be more than 25 character." : GoTo selesai
                End If

                'END OF VALIDASI DATA REPORT --------------------------------

                If AsDataTableTambahData(dtreport, "rrmoduleid~rrmenuid~rritem~rrrole~rrakses", dataRowReport(0) & "~" & dataRowReport(1) & "~" & dataRowReport(2) & "~" & dataRowReport(3) & "~" & dataRowReport(4)) = False Then
                    result(2) = "Report Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA REPORT ===========================================

        End If

        'MAPPING BUAT WS DATA CUSTOM -------------------------------------------------------
        'rcmoduleid(0) As Integer, rcidpc(1) As Integer, rcrole(2) As Integer, rcakses(3) As String

        'MAPPING BUAT FLEX DATA CUSTOM -----------------------------------------------------
        'rcmoduleid, rcidpc, rcrole, rcakses

        'Buat datatable custom
        Dim dtcustom As New DataTable
        AsDataTableTambahField(dtcustom, "rcmoduleid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcustom, "rcidpc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcustom, "rcrole", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcustom, "rcakses", AsEnumTypeData.AsString)

        If (Len(dataSplit(3)) > 0) Then

            'SPLIT PARAMETER DATA CUSTOM
            dataCustom = dataSplit(3).Split(sptRow)

            'VALIDASI DAN SET DATA ROW CUSTOM ==================================================
            Dim JmlDtCustom As Integer = dataCustom.Length
            For i = 1 To JmlDtCustom
                'SPLIT DATA CUSTOM
                dataRowCustom = dataCustom(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA CUSTOM -----------------------------------
                'CEK ARRAY DATA CUSTOM
                If (dataRowCustom.Length <> 4) Then
                    result(2) = "Custom Row : " & i & " - Invalid custom data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW CUSTOM ----------------------------

                'VALIDASI TIPE DATA CUSTOM ------------------------------------------
                'rcmoduleid(0) As Integer
                If (IsNumeric(dataRowCustom(0)) = False) Then
                    result(2) = "Custom Row : " & i & " - rcmoduleid required numeric." : GoTo selesai
                End If
                'rcidpc(1) As Integer
                If (IsNumeric(dataRowCustom(1)) = False) Then
                    result(2) = "Custom Row : " & i & " - rcidpc required numeric." : GoTo selesai
                End If
                ''rcrole(2) As Integer
                'If (IsNumeric(dataRowCustom(2)) = False) Then
                '    result(2) = "Custom Row : " & i & " - rcrole required numeric." : GoTo selesai
                'End If
                'END OF VALIDASI TIPE DATA CUSTOM -----------------------------------

                'VALIDASI DATA CUSTOM ---------------------------------------
                'rcrole(2) As String
                If Len(dataRowCustom(2)) = 0 Then
                    result(2) = "Custom Row : " & i & " - rcrole required numeric." : GoTo selesai
                End If
                If Len(dataRowCustom(2)) > 25 Then
                    result(2) = "Custom Row : " & i & " - rcrole should not be more than 25 character." : GoTo selesai
                End If

                'rcakses(3) As String
                If Len(dataRowCustom(3)) = 0 Then
                    result(2) = "Custom Row : " & i & " - rcakses can't be empty" : GoTo selesai
                End If
                If Len(dataRowCustom(3)) > 25 Then
                    result(2) = "Custom Row : " & i & " - rcakses should not be more than 25 character." : GoTo selesai
                End If

                'END OF VALIDASI DATA CUSTOM --------------------------------

                If AsDataTableTambahData(dtcustom, "rcmoduleid~rcidpc~rcrole~rcakses", dataRowCustom(0) & "~" & dataRowCustom(1) & "~" & dataRowCustom(2) & "~" & dataRowCustom(3)) = False Then
                    result(2) = "Custom Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA CUSTOM ===========================================

        End If



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

                'Hapus role
                sql = "Delete from M0_Role"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Hapus menu
                sql = "Delete from M0_Role_Menu"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Hapus report
                sql = "Delete from M0_Role_Report"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Hapus custom
                sql = "Delete from M0_Role_Custom"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("rkode")) & "', '" & FixQuotes(dr1("rnama")) & "')")
                Next
                sql = "Insert into M0_Role(rkode, rnama) values" & strValue1.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses menu
                If (dtmenu.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtmenu.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("rmmoduleid") & ", " & dr1("rmmenuid") & ", '" & FixQuotes(dr1("rmrole")) & "', '" & FixQuotes(dr1("rmakses")) & "', " & dr1("rmfavourite") & ")")
                    Next
                    sql = "Insert into M0_Role_Menu(rmmoduleid, rmmenuid, rmrole, rmakses, rmfavourite) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Proses report
                If (dtreport.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtreport.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("rrmoduleid") & ", " & dr1("rrmenuid") & ", " & dr1("rritem") & ", '" & FixQuotes(dr1("rrrole")) & "', '" & FixQuotes(dr1("rrakses")) & "')")
                    Next
                    sql = "Insert into M0_Role_Report(rrmoduleid, rrmenuid, rritem, rrrole, rrakses) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Proses custom
                If (dtcustom.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtcustom.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("rcmoduleid") & ", " & dr1("rcidpc") & ", '" & FixQuotes(dr1("rcrole")) & "', '" & FixQuotes(dr1("rcakses")) & "')")
                    Next
                    sql = "Insert into M0_Role_Custom(rcmoduleid, rcidpc, rcrole, rcakses) values" & strValue2.ToString & ""
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
                result(2) = "Main Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M0_RoleSearch(PostWsSearch(paramSplit(0), "M0_RoleSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            result(1) = hasilSearch.success
            result(2) = hasilSearch.errmessage

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