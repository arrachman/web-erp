Imports System.Timers
Imports MySql.Data.MySqlClient
Imports System.Net.Mail

Public Class ToolsManager
    Dim xStep As Integer = 0

    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public sptLogin As String = "Θ"
    Dim MyTimer As New System.Timers.Timer()

    Protected Overrides Sub OnStart(ByVal args() As String)
        Try
            log("Mulai Cek Replikasi")
            AddHandler MyTimer.Elapsed, AddressOf OnTimedEvent
            MyTimer.Interval = 5 * 1000
            MyTimer.Enabled = True
        Catch ex As Exception
            log("Error OnStart : " & Err.Description)
        End Try
    End Sub

    Public validasiPerulangan As Boolean = True
    Private Sub OnTimedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
        Try
            If validasiPerulangan Then
                validasiPerulangan = False
                f_action()
            End If
        Catch ex As Exception
            log("Error OnTimedEvent : " + Err.Description)
        End Try
    End Sub

    Public Sub f_action()
        Try
            Dim dt As DataTable
            dt = AsDataTableAmbilDariDB("SHOW SLAVE STATUS", "Server=127.0.0.1;Database=myerpplus-vmart;Uid=myerpplus;Pwd=myerpplus")
            For x = 0 To dt.Rows.Count - 1
                If dt.Rows(x)("Slave_IO_Running") <> "Yes" Or dt.Rows(x)("Slave_SQL_Running") <> "Yes" Then
                    'Last_SQL_Error
                    If dt.Rows(x)("Last_SQL_Error").ToString() <> "" Then
                        AsDataTableAmbilDariDB("STOP SLAVE; SET GLOBAL sql_slave_skip_counter = 1; START SLAVE;", "Server=127.0.0.1;Database=myerpplus-vmart;Uid=myerpplus;Pwd=myerpplus")

                        log("Sent Email")
                        Dim Smtp_Server As New SmtpClient
                        Dim e_mail As New MailMessage()
                        Smtp_Server.UseDefaultCredentials = False
                        Smtp_Server.Credentials = New Net.NetworkCredential("iklanmyerpplus2012@gmail.com", "lalabumbum")
                        Smtp_Server.Port = 587
                        Smtp_Server.EnableSsl = True
                        Smtp_Server.Host = "smtp.gmail.com"

                        e_mail = New MailMessage()
                        e_mail.From = New MailAddress("arrachm4n@gmail.com")
                        e_mail.To.Add("alfa.wawan@gmail.com")
                        e_mail.To.Add("adhieprasetiyo@gmail.com")
                        e_mail.To.Add("ranggabudipangestu@gmail.com")
                        e_mail.To.Add("arrachm4n@gmail.com")
                        e_mail.Subject = "Replikasi KWSG"
                        e_mail.IsBodyHtml = False
                        e_mail.Body = "Last_SQL_Error : " + dt.Rows(x)("Last_SQL_Error").ToString()
                        log("Last_SQL_Error : " + dt.Rows(x)("Last_SQL_Error").ToString())
                        Smtp_Server.Send(e_mail)


                    End If
                End If
            Next
            'STOP SLAVE; SET GLOBAL sql_slave_skip_counter = 1; START SLAVE;
            validasiPerulangan = True

        Catch ex As Exception
            log("Info f_action : " + Err.Description)
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        Try
            log("Keluar Cek Replikasi")
        Catch ex As Exception
            log("Error OnStop : " & Err.Description)
        End Try
    End Sub


    Public Function AsDataTableAmbilDariDB(ByVal StrSQL As String, ByVal strCon As String) As DataTable
        Dim ConX As MySqlConnection = New MySqlConnection

        ConX.ConnectionString = strCon
        ConX.Open()
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        Return dt1
    End Function

End Class