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
Public Class m1_files
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_FilesSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim isUpdate As Boolean

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
        'fsumber(0) As String, fnamafile(1) As String, fidtransaksi(2) As String, fidtransaksi2(3) As String, fcatatan(4) As String, 
        'fukuranfile(5) As String, ftanggal(6) As Date, finputuser(7) As Integer, finputtgl(8) As DateTime, fdefault(9) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'fsumber, fnamafile, fidtransaksi, fidtransaksi2, fcatatan, fukuranfile, ftanggal, 
        'finputuser, finputtgl, fdefault

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "fsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "fnamafile", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "fidtransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "fidtransaksi2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "fcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "fukuranfile", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "ftanggal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "finputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "finputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "fdefault", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 10) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'ftanggal(6) As Date
            If (IsDate(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - ftanggal required date." : GoTo selesai
            End If
            'finputuser(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - finputuser required numeric." : GoTo selesai
            End If
            'finputtgl(8) As DateTime
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - finputtgl required date." : GoTo selesai
            End If
            'fdefault(9) As Integer
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - fdefault required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'fsumber(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - fsumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(0)) > 15 Then
                result(2) = "Row : " & i & " - fsumber should not be more than 15 character." : GoTo selesai
            End If

            'fnamafile(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - fnamafile can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 100 Then
                result(2) = "Row : " & i & " - fnamafile should not be more than 100 character." : GoTo selesai
            End If

            'fidtransaksi(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - fidtransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 100 Then
                result(2) = "Row : " & i & " - fidtransaksi should not be more than 100 character." : GoTo selesai
            End If

            'fidtransaksi2(3) As String
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - fidtransaksi2 should not be more than 100 character." : GoTo selesai
            End If

            'fukuranfile(5) As String
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - fukuranfile can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(5)) > 25 Then
                result(2) = "Row : " & i & " - fukuranfile should not be more than 25 character." : GoTo selesai
            End If

            'ftanggal(6) As Date
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - ftanggal can't be empty" : GoTo selesai
            End If

            'finputtgl(8) As DateTime
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - finputtgl can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            AsDataTableTambahData(dtdetail, "fsumber~fnamafile~fidtransaksi~fidtransaksi2~fcatatan~fukuranfile~ftanggal~finputuser~finputtgl~fdefault", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9))
        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim sumber As String = "", idtrans As String = "", idtrans2 As String = ""

        Try
            If (dtdetail.Rows.Count > 0) Then
                Dim strValue1 As New StringBuilder, strValue2 As New StringBuilder, strValue3 As New StringBuilder, strValue4 As New StringBuilder
                sumber = dtdetail.Rows(0)("fsumber").ToString
                idtrans = dtdetail.Rows(0)("fidtransaksi").ToString
                idtrans2 = dtdetail.Rows(0)("fidtransaksi2").ToString

                If isUpdate Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'fcatatan
                        strValue1.Append(" WHEN '" & FixQuotes(dr1("fnamafile")) & "' THEN '" & FixQuotes(dr1("fcatatan")) & "' ")
                        'fukuranfile
                        strValue2.Append(" WHEN '" & FixQuotes(dr1("fnamafile")) & "' THEN '" & FixQuotes(dr1("fukuranfile")) & "' ")
                        'ftanggal
                        strValue3.Append(" WHEN '" & FixQuotes(dr1("fnamafile")) & "' THEN '" & FixQuotes(dr1("ftanggal")) & "' ")
                        'fdefault
                        strValue4.Append(" WHEN '" & FixQuotes(dr1("fnamafile")) & "' THEN '" & FixQuotes(dr1("fdefault")) & "' ")
                    Next
                    sql = "UPDATE m1_files SET fcatatan = CASE fnamafile " & strValue1.ToString & " ELSE fcatatan END, fukuranfile = CASE fnamafile " & strValue2.ToString & " ELSE fukuranfile END, ftanggal = CASE fnamafile " & strValue3.ToString & " ELSE ftanggal END, fdefault = CASE fnamafile " & strValue4.ToString & " ELSE fdefault END WHERE fsumber='" & sumber & "' AND fidtransaksi='" & idtrans & "'"
                    If Len(idtrans2) > 0 Then sql = sql & " AND fidtransaksi2='" & idtrans2 & "'"
                    'result(2) = sql : GoTo selesai
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("fsumber")) & "', '" & FixQuotes(dr1("fnamafile")) & "', '" & FixQuotes(dr1("fidtransaksi")) & "', '" & FixQuotes(dr1("fidtransaksi2")) & "', '" & FixQuotes(dr1("fcatatan")) & "', '" & FixQuotes(dr1("fukuranfile")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ftanggal"))) & "', " & dr1("finputuser") & ", NOW(), " & dr1("fdefault") & ")")
                    Next
                    sql = "Insert into M1_Files(fsumber, fnamafile, fidtransaksi, fidtransaksi2, fcatatan, fukuranfile, ftanggal, finputuser, finputtgl, fdefault) values" & strValue2.ToString & ""
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

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_FilesSearch(PostWsSearch(paramSplit(0), "M1_FilesSearch", pagingSplit(0), pagingSplit(1), "fsumber='" & sumber & "' AND fidtransaksi='" & idtrans & "'", pagingSplit(3), formatTgl, formatTglWaktu))

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
    Public Function M1_FilesDelete(ByVal param As String) As String

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
        Dim sumber As String = "", namafile As String = "", idtransaksi2 As String = ""
        Dim idtrans(4) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 4) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK SUMBER
            If (Len(idtrans(0)) = 0) Then
                result(2) = "fsumber can't be empty" : GoTo selesai
            Else
                sumber = idtrans(0)
            End If
            'ID TRANSAKSI
            If (Len(idtrans(1)) = 0) Then
                result(2) = "fidtransaksi can't be empty" : GoTo selesai
            Else
                idtransaksi = idtrans(1)
            End If
            'CEK NAMAFILE
            If (Len(idtrans(2)) = 0) Then
                result(2) = "fnamafile can't be empty" : GoTo selesai
            Else
                namafile = idtrans(2)
            End If
            'ID TRANSAKSI 2
            If (Len(idtrans(3)) > 0) Then
                idtransaksi2 = idtrans(3)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'DELETE
            sql = "DELETE FROM M1_Files WHERE fsumber = '" & sumber & "' AND fnamafile='" & namafile & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            'HAPUS FILE
            If namafile.Length > 0 Then
                Dim mypath = HttpContext.Current.Server.MapPath("~/") & "files\f1\" & sumber & "\" & namafile
                Try
                    System.IO.File.Delete(mypath)
                Catch exc As Exception
                    result(2) = "Error deleting file. Desc : " & exc.Message & "." : GoTo selesai
                End Try
            End If

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            Dim filter As String = IIf(Len(idtransaksi2) > 0, "fsumber='" & sumber & "' AND fidtransaksi='" & idtransaksi & "' AND fidtransaksi2='" & idtransaksi2 & "'", "fsumber='" & sumber & "' AND fidtransaksi='" & idtransaksi & "'")

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_FilesSearch(PostWsSearch(paramSplit(0), "M1_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))

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
    Public Function M1_FilesSearch(ByVal param As String) As String
        'M1_FilesSearch --------------------------------------------------------
        'fsumber, fnamafile, fidtransaksi, fidtransaksi2, fcatatan, fukuranfile, ftanggal, 
        'finputuser, finputtgl, finputusernama, fdefault

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

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_files_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Files", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("fsumber"), ""), sptField,
                     FxDB(dr("fnamafile"), ""), sptField,
                     FxDB(dr("fidtransaksi"), ""), sptField,
                     FxDB(dr("fidtransaksi2"), ""), sptField,
                     FxDB(dr("fcatatan"), ""), sptField,
                     FxDB(dr("fukuranfile"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ftanggal"), ""), formatTgl), sptField,
                     FxDB(dr("finputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("finputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("fdefault"), 0), sptField,
                     FxDB(dr("finputusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("fsumber, fnamafile, fidtransaksi, fidtransaksi2, fcatatan, fukuranfile, ftanggal, finputuser, finputtgl, fdefault, finputusernama"))

        Return wsResult
    End Function

End Class