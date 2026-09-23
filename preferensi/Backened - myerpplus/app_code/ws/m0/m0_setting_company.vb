Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.IO
Imports System.Xml
Imports System.Net

Imports System.Globalization
Imports System.Diagnostics

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_setting_company
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_GetProfilCompany(ByVal param As String) As String
        'M0_SetFileLibrary --------------------------------------------------------
        'namaFile, content
        '===> namaFile : namaFolder/namaFile.extensi
        '===> namaFolder : "grid" atau "report"

        'On Error GoTo selesai
        Dim searchmap As String = "", paramSearch As String = "", hasilSearch As New RsHasilWsSearch
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", dataSetting As String = "", dataPelanggan As String = ""
        Dim settingmap As String = ""
        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim Filter As String = "", Sorting As String = "", wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim dataSplit() As String

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
        pagingSplit = paramSplit(2).Split(sptSubParam)
        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
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


        'GET DATA PELANGGAN ======================================================
        'Dim dtPelanggan As DataTable = AsDataTableAmbilDariDB("SELECT * FROM pelanggan")

        'dataPelanggan = String.Concat(dataPelanggan, GetDataPelanggan(AppCode, "nama"), sptField)
        'dataPelanggan = String.Concat(dataPelanggan, GetDataPelanggan(AppCode, "logocompany"), sptField)
        'dataPelanggan = String.Concat(dataPelanggan, GetDataPelanggan(AppCode, "logoparent"), sptField)
        'dataPelanggan = String.Concat(dataPelanggan, GetDataPelanggan(AppCode, "backgroundimage"))

        ' set app
        Dim namapt As String = ""
        Using reader As XmlReader = XmlReader.Create(New StringReader(File.ReadAllText(HttpContext.Current.Server.MapPath("~/") & "app\app.xml")))
            While reader.Read()
                Select Case reader.NodeType
                    Case XmlNodeType.Element
                        Select Case reader.Name
                            Case "namapt" : namapt = reader.ReadElementContentAsString()
                        End Select
                End Select
            End While
        End Using

        'END OF GET DATA PELANGGAN ===============================================
        dataPelanggan = String.Concat(dataPelanggan, namapt, sptField, sptField, sptField)
        'GET DATA PROFIL COMPANY ======================================================
        Using wsm As New m0_setting
            paramSearch = wsm.M0_SettingSearch(PostWsSearch(paramSplit(0), "M0_SettingSearch", 0, 0, Filter, Sorting, formatTgl, formatTglWaktu))
        End Using
        hasilSearch = GetWsSearch(paramSearch)
        If hasilSearch.success = 0 Then
            result(2) = hasilSearch.errmessage : GoTo selesai
        End If
        dataSetting = hasilSearch.data.Split(sptParam)(0)
        settingmap = hasilSearch.data.Split(sptParam)(1)
        'END OF GET DATA PROFIL COMPANY ===============================================

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, dataSetting, sptSubParam, dataPelanggan)
        wsResult = String.Concat(wsResult, sptParam, settingmap, sptSubParam, ReplaceMapping("nama, logocompany, logoparent, backgroundimage"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_SimpanProfilCompany(ByVal param As String) As String

        'On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim paramSearch As String = "", hasilSearch As New RsHasilWsSearch
        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim dataSplit() As String

        Dim dt As New DataTable

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)
        pagingSplit = paramSplit(2).Split(sptSubParam)
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

        Dim namapt As String = paramSplit(5).Split(sptSubParam)(1).ToString.Split(sptField)(0).ToString

        'Dim processList() As Process
        'processList = Process.GetProcessesByName(ListBox1.Items(ListBox1.SelectedIndex).ToString)

        'For Each proc As Process In processList
        '    If MsgBox("Terminate " & proc.ProcessName & "?", MsgBoxStyle.YesNo, "Terminate?") = MsgBoxResult.Yes Then
        '        Try
        '            proc.Kill()
        '        Catch ex As Exception
        '            MessageBox.Show(ex.Message)
        '        End Try
        '    End If
        'Next

        ' set app
        Dim pathappxml As String = HttpContext.Current.Server.MapPath("~/") & "app\app.xml"
        'result(2) = pathappxml
        'File.Delete(pathappxml)
        'result(2) = "a" : GoTo selesai
        Dim namaptbaru As String, appxml As String = File.ReadAllText(pathappxml)
        Using reader As XmlReader = XmlReader.Create(New StringReader(appxml))
            While reader.Read()
                Select Case reader.NodeType
                    Case XmlNodeType.Element
                        Select Case reader.Name
                            Case "namapt"
                                namaptbaru = reader.ReadElementContentAsString()
                                If namapt <> namaptbaru Then
                                    File.Delete(pathappxml)
                                    File.WriteAllText(pathappxml, appxml.Replace(namapt, namaptbaru))
                                End If
                        End Select
                End Select
            End While
        End Using

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)(1).Split(sptField)    'SPLIT PARAMETER DATA
        param = param.Replace(sptSubParam + paramSplit(5).Split(sptSubParam)(1), "")
        'CEK ARRAY DATA
        'If (dataSplit.Length <> 6) Then
        '    result(2) = "Invalid file data parameter." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================



        'SIPMAN KE DATA PELANGGAN ======================================================
        'Dim Sql As String
        'Dim ConStr As String = Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(HttpContext.Current.Server.MapPath("~/") + "\report\config\config")))
        'Dim myConn2 = New MySql.Data.MySqlClient.MySqlConnection(ConStr)
        'myConn2.Open()
        'Sql = "Update pelanggan set nama = '" & FixQuotes(dataSplit(0)) & "', logocompany = '" & FixQuotes(dataSplit(1)) & "', logoparent = '" & FixQuotes(dataSplit(2)) & "', backgroundimage = '" & FixQuotes(dataSplit(3)) & "' WHERE kode = '" & AppCode & "'"
        'Try
        '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
        '    With objCmd
        '        .Connection = myConn2
        '        .CommandType = CommandType.Text
        '        .CommandText = Sql
        '    End With
        '    objCmd.ExecuteNonQuery()

        'Catch ex As Exception
        '    result(2) = Err.Description : GoTo selesai
        'End Try
        'myConn2.Close()
        'END OF SIPMAN KE DATA PELANGGAN ===============================================

        'SIPMAN KE TABEL SETTING ======================================================
        Using wsm As New m0_setting
            paramSearch = wsm.M0_SettingSimpan(param)
        End Using
        'END OF SIPMAN KE TABEL SETTING ===============================================
        hasilSearch = GetWsSearch(paramSearch)

        If hasilSearch.success = 0 Then
            result(2) = hasilSearch.errmessage : GoTo selesai
        End If

        result(1) = 1
        param = param.Replace(pagingSplit(2).Split(sptField)(0) + sptField, "")
        Return M0_GetProfilCompany(param)
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
    Public Function M0_SetAplikasiSearch(ByVal param As String) As String
        'M0_SetFileLibrary --------------------------------------------------------
        'namaFile, content
        '===> namaFile : namaFolder/namaFile.extensi
        '===> namaFolder : "grid" atau "report"

        'On Error GoTo selesai
        Dim searchmap As String = "", paramSearch As String, hasilSearch As New RsHasilWsSearch
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim dataSplit() As String

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
        pagingSplit = paramSplit(2).Split(sptSubParam)
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        Dim Filter As String = "", Sorting As String = ""
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'GET DATA PELANGGAN ======================================================

        Dim paramSettingGrup As New m0_caridata
        paramSearch = paramSettingGrup.CdM0_Realization_Category(PostWsSearch(paramSplit(0), "CdM0_SettingGrup", 0, 0, "", "", formatTgl, formatTglWaktu))

        hasilSearch = GetWsSearch(paramSearch)

        Dim hasilGrup As String = hasilSearch.data.Split(sptParam)(0)
        For Each dr As String In hasilGrup.Split(sptRow)
            search2 = String.Concat(search2,
                    FxDB(dr, ""), sptField,
                    FxDB(dr, ""), sptRow)
        Next
        search2 = search2.Substring(0, search2.Length - sptRow.Length)


        Dim paramSetting As New m0_setting
        If Filter.Length = 0 Then
            Filter = "sgrup = '" & hasilGrup.Split(sptRow)(0) & "'"
        End If
        paramSearch = paramSetting.M0_SettingSearch(PostWsSearch(paramSplit(0), "CdM0_SettingGrup", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

        hasilSearch = GetWsSearch(paramSearch)

        'result(1) = hasilSearch.success
        'result(2) = hasilSearch.errmessage

        resultPaging(0) = hasilSearch.isPaging
        resultPaging(1) = hasilSearch.isNext
        resultPaging(2) = hasilSearch.isPrevious
        resultPaging(3) = hasilSearch.countPage
        resultPaging(4) = hasilSearch.countRow
        search = hasilSearch.data.Split(sptParam)(0)
        searchmap = hasilSearch.data.Split(sptParam)(1)

        'END OF GET DATA PROFIL COMPANY ===============================================
        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, search2)
        wsResult = String.Concat(wsResult, sptParam, searchmap, sptSubParam, ReplaceMapping("l, v"))

        Return wsResult
    End Function


End Class