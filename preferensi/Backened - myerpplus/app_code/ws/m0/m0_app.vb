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
Public Class m0_app
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M0_AppRegister(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = "", AppKey As String = "", AppSecret As String = ""
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
        'appid(0) As Integer, appname(1) As String, appkey(2) As String, appsecret(3) As String, appactive(4) As Integer, 
        'appcreated(5) As DateTime

        'MAPPING BUAT FLEX --------------------------------------------------------
        'appid, appname, appkey, appsecret, appactive, appcreated

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 6) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'appid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "appid required numeric." : GoTo selesai
        End If
        'appactive(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "appactive required numeric." : GoTo selesai
        End If
        'appcreated(5) As DateTime
        If (IsDate(dataUtama(5)) = False) Then
            result(2) = "appcreated required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'appname(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "appname can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "appname should not be more than 25 character." : GoTo selesai
        End If

        'appkey(2) As String
        'If Len(dataUtama(2)) = 0 Then
        '    result(2) = "appkey can't be empty" : GoTo selesai
        'End If
        'If Len(dataUtama(2)) > 50 Then
        '    result(2) = "appkey should not be more than 50 character." : GoTo selesai
        'End If

        'appsecret(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "appsecret can't be empty" : GoTo selesai
        'End If
        'If Len(dataUtama(3)) > 250 Then
        '    result(2) = "appsecret should not be more than 250 character." : GoTo selesai
        'End If

        'appcreated(5) As DateTime
        If Len(dataUtama(5)) = 0 Then
            result(2) = "appcreated can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA ========================================================


        'GENERATE APPKEY DAN APPSECRET ===============================================
        Dim ClsSecurity As New ClsSecurity
        AppKey = ClsSecurity.Generate(20)

        AppSecret = ClsSecurity.MD5CalcString(AppKey & dataUtama(1))
        'END OF GENERATE APPKEY DAN APPSECRET ========================================


        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(appid) FROM M0_App WHERE appid=" & result(4))
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    sql = "Update M0_App set appname  = '" & FixQuotes(dataUtama(1)) & "', appkey  = '" & FixQuotes(AppKey) & "', appsecret  = '" & FixQuotes(AppSecret) & "', appactive  = " & dataUtama(4) & " where appid = " & dataUtama(0) & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else
                sql = "Insert into M0_App (appname, appkey, appsecret, appactive, appcreated) values('" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(AppKey) & "', '" & FixQuotes(AppSecret) & "', " & dataUtama(4) & ", NOW())"
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