Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.IO

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_library
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_SetFileLibrary(ByVal param As String) As String
        'M0_SetFileLibrary --------------------------------------------------------
        'namaFile, content
        '===> namaFile : namaFolder/namaFile.extensi
        '===> namaFolder : "grid" atau "report"

        'On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim dataSplit() As String

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim fileName As String = "", folderGlobal As String = ""

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


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid file data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(dataSplit(0)) = 0) Then
            result(2) = "File Name can't be empty." : GoTo selesai
        Else
            'SET NAMAFILE
            fileName = dataSplit(0).ToString

            'CEK NAMA FOLDER, JIKA BUKAN "grid" atau "report" maka tampilkan alert
            Dim nmFolder() As String = fileName.Split("/")
            If (nmFolder.Length < 2) Then
                result(2) = "#1. Invalid folder name." : GoTo selesai
            ElseIf Not (nmFolder(0).ToString.Equals("grid") Or nmFolder(0).ToString.Equals("report") Or nmFolder(0).ToString.Equals("statistic")) Then
                result(2) = "#2. Invalid folder name." : GoTo selesai
            Else
                folderGlobal = nmFolder(0).ToString
            End If

            'CEK EKSTENSI FILE
            Dim fileExt() As String = fileName.Split(".")
            If (fileExt.Length < 2) Then
                result(2) = "#1. Invalid file extentions." : GoTo selesai
            ElseIf (Len(fileExt(fileExt.Length - 1)) = 0) Then
                result(2) = "#2. Invalid file extentions." : GoTo selesai
            End If
        End If

        'SET CONTENTS
        contents = dataSplit(1).ToString
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK FILE EXISTS
        Try
            File.Delete(myPath & fileName)
            File.WriteAllText(myPath & fileName, contents)
            contents = fileName & sptSubParam & contents
        Catch ex As Exception
            result(2) = ex.Message
            contents = "" : GoTo selesai
        End Try

        'GENERATE JSON GLOBAL =============================================================================
        Dim sr As StreamReader
        Dim strJson As String = ""

        'SET Directory
        myPath = HttpContext.Current.Server.MapPath("~/") & "app\libs\" & folderGlobal & "\"

        Try
            Dim myRootFolder As New IO.DirectoryInfo(myPath)
            Dim myFileList As IO.FileInfo()
            Dim myFile As IO.FileInfo

            'list the names of all files in the specified directory
            myFileList = myRootFolder.GetFiles("*.json")

            strJson = "{""form"":["

            Dim i As Integer = 0
            For Each myFile In myFileList
                sr = File.OpenText(String.Concat(myPath, myFile))
                contents = sr.ReadToEnd()
                sr.Close()

                If i = 0 Then
                    strJson = String.Concat(strJson, "{""n"":""", myFile, """, ", contents.Substring(1, contents.Length - 2), "}")
                Else
                    strJson = String.Concat(strJson, ", {""n"":""", myFile, """, ", contents.Substring(1, contents.Length - 2), "}")
                End If
                i = i + 1
            Next

            strJson = strJson & "]}"
            strJson = strJson.Replace(".json", "")

        Catch ex As Exception
            result(2) = ex.Message : GoTo selesai
        End Try

        'CREATE JSON UNTUK SEMUA FORM =====================================
        myPath = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim jsonName As String = folderGlobal & ".json"
        'CEK FILE EXISTS
        Try
            File.Delete(myPath & jsonName)
            File.WriteAllText(myPath & jsonName, strJson)

            result(1) = 1

        Catch ex As Exception
            result(2) = ex.Message
            contents = "" : GoTo selesai
        End Try
        'END OF CREATE JSON UNTUK SEMUA FORM ==============================

        'END OF GENERATE JSON GLOBAL ======================================================================

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_GetFileLibrary(ByVal param As String) As String
        'M0_GetFileLibrary --------------------------------------------------------
        'namaFile, content

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim sr As StreamReader
        Dim fileName As String = ""

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


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "File Name can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            fileName = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK FILE EXISTS
        If (File.Exists(myPath & fileName)) Then
            sr = File.OpenText(myPath & fileName)
            contents = sr.ReadToEnd()
            contents = fileName & sptSubParam & contents
            sr.Close()
        Else
            result(2) = fileName & " File doesn't exists." : GoTo selesai
        End If

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, contents)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_SetLangFileLibrary(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strJson As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim isUpdate As Boolean
        Dim dtTranslate As New DataTable
        Dim namaKolom() As String, arrHasil() As String

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
        'kodebahasa(1) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kodebahasa

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptField)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'GENERATE FILE JSON ================================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        namaKolom = {"s", "t"}
        For i = 1 To JmlDtDetail
            If (Len(dataDetail(i - 1)) = 0) Then
                result(2) = "Row : " & i & " - Language Code can't be empty." : GoTo selesai
            Else
                'AMBIL DATA TRANSLATE BERDASARKAN BAHASA
                dtTranslate = AsDataTableAmbilDariDB("SELECT s.ssentence, t.ttranslate FROM m0_sentence s JOIN m0_translate t ON s.sid=t.tsentence WHERE t.tlanguage='" & dataDetail(i - 1) & "'")
                'BUAT STRUKTUR JSON
                strJson = CreateJson(dataDetail(i - 1), dtTranslate, namaKolom)
                'BUAT FILE JSON
                arrHasil = M0_GenerateFileLibrary(paramSplit(0) & "★M0_GenerateFileLibrary★0△0△△△△★0★1★" & "language\" & dataDetail(i - 1) & ".json" & sptSubParam & strJson).Split(sptParam)
                'CEK HASIL BUAT FILE JSON, JIKA ERROR MAKA GOTO SELESAI
                arrHasil = arrHasil(0).Split(sptSubParam)
                If arrHasil(1) = 0 Then
                    result(2) = "Row : " & i & " - " & arrHasil(2) : GoTo selesai
                End If
            End If
        Next
        'END OF GENERATE FILE JSON =========================================================

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = result(4)

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
    Public Function M0_SetFormFileLibrary(ByVal param As String) As String
        'M0_SetFormFileLibrary --------------------------------------------------------
        'folder, file

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sr As StreamReader
        Dim myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\grid\"
        Dim strJson As String = "", contents As String = ""

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

        'SET Directory
        myPath = String.Concat(myPath, paramSplit(5))

        Try
            Dim myRootFolder As New IO.DirectoryInfo(myPath)
            Dim myFileList As IO.FileInfo()
            Dim myFile As IO.FileInfo

            'list the names of all files in the specified directory
            myFileList = myRootFolder.GetFiles()

            strJson = "{""form"":["

            Dim i As Integer = 0
            For Each myFile In myFileList
                sr = File.OpenText(String.Concat(myPath, myFile))
                contents = sr.ReadToEnd()
                sr.Close()

                If i = 0 Then
                    strJson = String.Concat(strJson, "{""n"":""", myFile, """, ", contents.Substring(1, contents.Length - 2), "}")
                Else
                    strJson = String.Concat(strJson, ", {""n"":""", myFile, """, ", contents.Substring(1, contents.Length - 2), "}")
                End If
                i = i + 1
            Next

            strJson = strJson & "]}"
            strJson = strJson.Replace(".json", "")

        Catch ex As Exception
            result(2) = ex.Message : GoTo selesai
        End Try

        'CREATE JSON UNTUK SEMUA FORM =====================================
        myPath = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim jsonName As String = "grid.json"
        'CEK FILE EXISTS
        Try
            File.Delete(myPath & jsonName)
            File.WriteAllText(myPath & jsonName, strJson)

            result(1) = 1

        Catch ex As Exception
            result(2) = ex.Message
            contents = "" : GoTo selesai
        End Try
        'END OF CREATE JSON UNTUK SEMUA FORM ==============================


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_GenerateFileLibrary(ByVal param As String) As String
        'M0_GenerateFileLibrary --------------------------------------------------------
        'namaFile, content

        'On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim dataSplit() As String

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim fileName As String = ""

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


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid file data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(dataSplit(0)) = 0) Then
            result(2) = "File Name can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            fileName = dataSplit(0).ToString
            Dim fileExt() As String = fileName.Split(".")
            If (fileExt.Length < 2) Then
                result(2) = "#1. Invalid file extentions." : GoTo selesai
            ElseIf (Len(fileExt(fileExt.Length - 1)) = 0) Then
                result(2) = "#2. Invalid file extentions." : GoTo selesai
            End If
        End If

        'SET CONTENTS
        contents = dataSplit(1).ToString
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK FILE EXISTS
        Try
            File.Delete(myPath & fileName)
            File.WriteAllText(myPath & fileName, contents)
            contents = fileName & sptSubParam & contents
        Catch ex As Exception
            result(2) = ex.Message
            contents = "" : GoTo selesai
        End Try

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, contents)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_GetLibraryGrid(ByVal param As String) As String
        On Error GoTo selesai

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim sr As StreamReader
        Dim fileName As String = "grid.json"

        Dim wsResult As String = ""

        'CEK FILE EXISTS
        If (File.Exists(myPath & fileName)) Then
            sr = File.OpenText(myPath & fileName)
            contents = sr.ReadToEnd()
            sr.Close()
        Else
            wsResult = fileName & " File doesn't exists." : GoTo selesai
        End If

        wsResult = contents

selesai:

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_GetLibraryReport(ByVal param As String) As String
        On Error GoTo selesai

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim sr As StreamReader
        Dim fileName As String = "report.json"

        Dim wsResult As String = ""

        'CEK FILE EXISTS
        If (File.Exists(myPath & fileName)) Then
            sr = File.OpenText(myPath & fileName)
            contents = sr.ReadToEnd()
            sr.Close()
        Else
            wsResult = fileName & " File doesn't exists." : GoTo selesai
        End If

        wsResult = contents

selesai:

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_GetLibraryStatistic(ByVal param As String) As String
        On Error GoTo selesai

        Dim contents As String = "", myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\"
        Dim sr As StreamReader
        Dim fileName As String = "statistic.json"

        Dim wsResult As String = ""

        'CEK FILE EXISTS
        If (File.Exists(myPath & fileName)) Then
            sr = File.OpenText(myPath & fileName)
            contents = sr.ReadToEnd()
            sr.Close()
        Else
            wsResult = fileName & " File doesn't exists." : GoTo selesai
        End If

        wsResult = contents

selesai:

        Return wsResult
    End Function

End Class
