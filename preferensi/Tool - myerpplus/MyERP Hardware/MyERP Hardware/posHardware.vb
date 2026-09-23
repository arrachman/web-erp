Imports System.IO
Imports System.Text
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Net

Public Class POS_Hardware
    Dim ctThread As Threading.Thread
    Public Shared Sub Main()
        Application.Run(New POS_Hardware())
    End Sub

    Public Sub New()
        ' The Windows Forms Designer requires the following call.
        InitializeComponent()

    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ntfHardware.MouseDoubleClick
        ntfHardware.Visible = False
        WindowState = FormWindowState.Normal
        ShowInTaskbar = True
    End Sub

    Private Sub Form1_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        If WindowState = FormWindowState.Minimized Then
            ntfHardware.Visible = True
            ShowInTaskbar = False
            ntfHardware.ShowBalloonTip(1000)
        End If
    End Sub

    Private Sub btnconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btstart.Click
        If btstart.Enabled Then
            btstart.Enabled = False

            ctThread = New Threading.Thread(AddressOf petugas_siap)

            ctThread.Start()
            btnstop.Enabled = True
        End If
    End Sub

    Private Sub btndisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnstop.Click
        If btnstop.Enabled Then
            btnstop.Enabled = False
            ctThread.Abort()
            serverSocket.Stop()

            btstart.Enabled = True
        End If
    End Sub

    Private Sub POS_Hardware_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If File.Exists(LokasiAppConfig) = False Then
            File.Create(LokasiAppConfig).Dispose()
            Dim data As String = ""
            data = "<setting>" & vbCrLf
            data += "<port>8484</port>" & vbCrLf
            data += "<portname>COM1</portname>" & vbCrLf
            data += "</setting>"
            File.WriteAllText(LokasiAppConfig, data)
        End If

        dtAntri = New DataTable
        dtAntri.Columns.Add("Tanggal")
        dtAntri.Columns.Add("User")
        dtAntri.Columns.Add("No Transksi")
        dtAntri.Columns.Add("Data")
        dgAntri.DataSource = dtAntri
        dgAntri.Columns(dgAntri.ColumnCount - 2).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dgAntri.ReadOnly = True

        dtPrinted = New DataTable
        dtPrinted.Columns.Add("Tanggal")
        dtPrinted.Columns.Add("User")
        dtPrinted.Columns.Add("Data")
        dgPrinted.DataSource = dtPrinted
        dgAntri.Columns(dgAntri.ColumnCount - 2).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dgPrinted.ReadOnly = True

        btstart.Enabled = False
        Dim streamToPrint As StreamReader = New StreamReader("app.config")
        Dim line As String
        While 0 < 1
            line = streamToPrint.ReadLine()
            If line Is Nothing Then
                Exit While
            End If
            If line.Contains("<port>") Then
                txtport.Text = line.Replace("<port>", "").Replace("</port>", "")
            ElseIf line.Contains("portname") Then
                portname = line.Replace("<portname>", "").Replace("</portname>", "")
            End If


        End While
        streamToPrint.Dispose()
        streamToPrint.Close()
        kebijakanKu = "<?xml version=""1.0""?>"
        kebijakanKu += "<cross-domain-policy xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"""
        kebijakanKu += "xsi:noNamespaceSchemaLocation=""http://www.adobe.com/xml/schemas/PolicyFileSocket.xsd"">"
        kebijakanKu += "<site-control permitted-cross-domain-policies=""all"" />"
        kebijakanKu += "<allow-access-from domain=""*"" to-ports=""*"" secure=""false"" />"
        kebijakanKu += "</cross-domain-policy>"

        kebijakanKu = String.Format("{0}" & vbNullChar, kebijakanKu)

        ctThread = New Threading.Thread(AddressOf petugas_siap)
        ctThread.Start()
        btnstop.Enabled = True
        'btndisconnect.Enabled = False
    End Sub

    Private Sub POS_Hardware_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        serverSocket.Stop()
    End Sub

    Public Sub petugas_siap()
        Dim infiniteCounter As Integer = 0, bytesFrom(10024) As Byte, dataDariUser As String, networkStream As NetworkStream
        serverSocket = New TcpListener(IPAddress.Any, txtport.Text)
        serverSocket.Start()
        log("> MyERP Hardware Started ....")
        Try
            For infiniteCounter = 1 To 2
                infiniteCounter = 1
                clientSocket = serverSocket.AcceptTcpClient()
                networkStream = clientSocket.GetStream()
                networkStream.Read(bytesFrom, 0, CInt(clientSocket.ReceiveBufferSize))
                dataDariUser = System.Text.Encoding.ASCII.GetString(bytesFrom)

                If dataDariUser.Contains("<policy-file-request/>") Then
                    mintaKebijakan()
                ElseIf dataDariUser.Contains("$") = False Then
                    log("<  Data baru NULL")
                ElseIf dataDariUser.Split("$").Length = 1 Then
                    log("<  Data baru kosong")
                Else
                    dataDariUser = dataDariUser.Substring(0, dataDariUser.IndexOf("$"))

                    If dataDariUser = "getInfo" Then
                        getInfo()
                    Else
                        Dim arr As String() = dataDariUser.Replace("@p1@", sptParam).Split(sptParam)
                        If arr(0) = "cetakLangsung" Then
                            'Dim cl As New CetakLangsung
                            'cl.Page_Load(arr(1), arr(2))
                        Else
                            perangkatKerasLakukan(dataDariUser)
                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            If Err.Description = "Thread was being aborted." Then
                log("< MyERP Hardware Stopped ....")
            Else
                log("< petugas_siap, Err : " + Err.Description)
            End If
        End Try
    End Sub

    Private Sub mintaKebijakan()
        Dim broadcastStream As Net.Sockets.NetworkStream = clientSocket.GetStream()
        Dim broadcastBytes As [Byte]() = Encoding.ASCII.GetBytes(kebijakanKu)
        broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length)
        broadcastStream.Flush()
    End Sub

    Private Sub getInfo()
        Dim kirim As String = ""

        For Each printer In System.Drawing.Printing.PrinterSettings.InstalledPrinters
            If kirim.Length > 0 Then
                kirim += "@p2@"
            End If
            kirim += printer
        Next

        Dim broadcastStream As NetworkStream = clientSocket.GetStream()
        Dim broadcastBytes As [Byte]() = Encoding.ASCII.GetBytes(kirim)
        broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length)
        broadcastStream.Flush()
        clientSocket.Close()
        log(">  Minta Info")
    End Sub

    Private Sub perangkatKerasLakukan(ByVal user As String)
        user1 = user
        Try
            CType(ListUser(user1), TcpClient).Close()
            log("<  Current user " + user1 + " clossed")
            ListUser(user1) = clientSocket
            log(">  User " + user1 + " listed")
        Catch ex As Exception
            ListUser(user1) = clientSocket
            log(">  User " + user1 + " listed")
        End Try

        Dim broadcastStream As Net.Sockets.NetworkStream = ListUser(user1).GetStream()
        Dim broadcastBytes As [Byte]() = Encoding.ASCII.GetBytes("sign_in")
        broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length)
        broadcastStream.Flush()

        no_urut = no_urut + 1
        Dim terima As New mintaUser
        terima.userMulai(ListUser(user1), user1 + "_" + (no_urut).ToString)
    End Sub

    Private Sub btnsave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnsave.Click
        If txtport.Text.Length = 0 Then
            MsgBox("Isi port dulu")
            txtport.Focus()
            Return
        End If

        Dim data As String = ""
        data = "<setting>" & vbCrLf
        data += "<port>" + txtport.Text + "</port>" & vbCrLf
        data += "</setting>"
        File.WriteAllText(LokasiAppConfig, data)
    End Sub
End Class
