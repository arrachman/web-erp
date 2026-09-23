Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_print
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Print(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)
        Dim dataUtama() As String

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = "", paket As String = "", notransaksi As String = ""

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

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If

        'M2_Print
        'paket, idtransaksi, notransaksi

        'sumber(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "paket can't be empty" : GoTo selesai
        Else
            paket = dataUtama(0)
        End If

        'idtransaksi(1) As Integer
        If (IsNumeric(dataUtama(1)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            idtransaksi = dataUtama(1)
        End If

        'notransaksi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "notransaksi can't be empty" : GoTo selesai
        Else
            notransaksi = dataUtama(2)
        End If
        'END OF VALIDASI DAN SET DATA =====================================================

        'CASE SUMBER UNTUK QUERY ==========================================================
        Select Case paket
            Case "SQ" : sql = "UPDATE M5_SQ SET SQcetakanke=SQcetakanke+1 WHERE SQid='" & idtransaksi & "'"
            Case "SO" : sql = "UPDATE M5_SO SET SOcetakanke=SOcetakanke+1 WHERE SOid='" & idtransaksi & "'"
            Case "AS" : sql = "UPDATE M5_AS SET AScetakanke=AScetakanke+1 WHERE ASid='" & idtransaksi & "'"
            Case "PL" : sql = "UPDATE M5_PL SET PLcetakanke=PLcetakanke+1 WHERE PLid='" & idtransaksi & "'"
            Case "DO" : sql = "UPDATE M5_DO SET DOcetakanke=DOcetakanke+1 WHERE DOid='" & idtransaksi & "'"
            Case "DR" : sql = "UPDATE M5_DR SET DRcetakanke=DRcetakanke+1 WHERE DRid='" & idtransaksi & "'"
            Case "PI" : sql = "UPDATE M5_PI SET PIcetakanke=PIcetakanke+1 WHERE PIid='" & idtransaksi & "'"
            Case "SI" : sql = "UPDATE M5_SI SET SIcetakanke=SIcetakanke+1 WHERE SIid='" & idtransaksi & "'"
            Case "RNR" : sql = "UPDATE M5_RNR SET RNRcetakanke=RNRcetakanke+1 WHERE RNRid='" & idtransaksi & "'"
            Case "SR" : sql = "UPDATE M5_SR SET SRcetakanke=SRcetakanke+1 WHERE SRid='" & idtransaksi & "'"
            Case "IC" : sql = "UPDATE M5_IC SET ICcetakanke=ICcetakanke+1 WHERE ICid='" & idtransaksi & "'"
            Case "PV" : sql = "UPDATE M5_PV SET PVcetakanke=PVcetakanke+1 WHERE PVid='" & idtransaksi & "'"
            Case Else
                result(2) = "Invalid Packet." : GoTo selesai
        End Select
        'END OF CASE SUMBER UNTUK QUERY ===================================================


        'UPDATE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'UPDATE JUMLAH CETAKAN
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
            result(2) = notransaksi
            result(3) = 0
            result(4) = idtransaksi

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi

        End Try

        objCmd = Nothing
        'END OF UPDATE DI DATABASE ==========================================================

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
