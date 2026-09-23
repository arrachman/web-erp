Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Messaging
Imports System.Xml
Imports System.Timers
Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.Net.Mail

Public Class ToolsManager
    Dim xStep As Integer = 0

    Public ErrNumber As Long = 0, ErrStep As Long = 0, ErrDescription As String = "", ErrSource As String = "", ErrLine As Integer = 0, HasilSQL As String = ""

    ' Variable Split Ws
    Public ob_resultWs, vResult(), vPaging(), vData(), vArrResult(), wsTarget, wsErrmessage, wsErrstep, wsIdtransaksi, vNamaKolom() As String
    Public wsSuccess, wsIspaging, wsIsNext, wsIsPrev As Boolean
    Public wsCurPage, wsCountRow As Integer
    Public wsArrUtama, wsArrDetail As DataTable
    Public row As DataRow, idx As Integer

    'IpAddress
    Dim Listener As TcpListener

    'IpAddress
    Dim ListenerCOGS As TcpListener

    Dim url As String
    Dim currUrl As String, currParam As String, currSql As String, currKoneksi As String

    Dim queue As New MessageQueue, QUser As New MessageQueue, cogs As Boolean = True

    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public sptLogin As String = "Θ"
    Public go As Boolean = True

    Public dr As DataRow
    Public dt, dtCOGS, dtObj, dtUser, dtConfig, dtTemp As New DataTable
    Dim JmlAgent As Integer
    Dim LokasiAgent As String
    Dim waktuHitungUlang As String = ""
    Dim hariHitungUlang As String = ""
    Dim tglHitungUlang As Integer = 0
    Dim jamHitungUlang As Integer = 0
    Dim menitHitungUlang As Integer = 0

    Dim waktuReminder As String = "", hariReminder As String = "", tglReminder As Integer = 0, jamReminder As Integer = 0, menitReminder As Integer = 0
    Dim intervalReminder As Integer = 0
    Dim CountLogout As Integer
    Dim config, PathQueue, PQUser As String
    Dim AppConfig As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\app.config"
    Dim lokasi As String, ar() As String, num As Integer, urutan As Integer = 0
    Dim vThread As Threading.Thread, vThreadCOGS As Threading.Thread
    Dim MyTimer As New System.Timers.Timer(), MyTimerUser As New System.Timers.Timer()
    Dim Client As ChatClient, ls As Object = {}

    'Protected Overrides Sub OnStart20250519(ByVal args() As String)
    '    Try
    '        log("Mulai MyERP Queue")

    '        For Each process2 As Process In Process.GetProcesses
    '            If process2.ProcessName.Contains("Agent") Then
    '                process2.Kill()
    '            End If
    '        Next
    '        If File.Exists(AppConfig) = False Then
    '            log("Error StartConfig : File app.config Not Found")
    '            Return
    '        End If
    '        StartConfig()
    '        ListenQueue()
    '        ListenQueueUser()
    '    Catch ex As Exception
    '        log("Error OnStart : " & Err.Description)
    '    End Try
    'End Sub

    Protected Overrides Sub OnStart(ByVal args() As String)
        Try
            log("Mulai MyERP Queue")

            'For Each process2 As Process In Process.GetProcesses
            '    If process2.ProcessName.Contains("Agent") Then
            '        process2.Kill()
            '    End If
            'Next
            If File.Exists(AppConfig) = False Then
                log("Error StartConfig : File app.config Not Found")
                Return
            End If
            StartConfig()
            ListenQueue()
            ListenQueueUser()
        Catch ex As Exception
            log("Error OnStart : " & Err.Description)
        End Try
    End Sub

    'Protected Overrides Sub OnStop20250519()
    '    Try
    '        For Each process2 As Process In Process.GetProcesses
    '            If process2.ProcessName.Contains("Agent") Then
    '                process2.Kill()
    '            End If
    '        Next
    '        log("Keluar MyERP Queue")
    '    Catch ex As Exception
    '        log("Error OnStop : " & Err.Description)
    '    End Try
    'End Sub

    Protected Overrides Sub OnStop()
        Try
            'For Each process2 As Process In Process.GetProcesses
            '    If process2.ProcessName.Contains("Agent") Then
            '        process2.Kill()
            '    End If
            'Next
            log("Keluar MyERP Queue")
        Catch ex As Exception
            log("Error OnStop : " & Err.Description)
        End Try
    End Sub

    Sub StartConfig()
        Try
            idx = 0
            'setting delay timer for print
            AddHandler MyTimer.Elapsed, AddressOf OnTimedEvent
            MyTimer.Interval = 50
            MyTimer.Enabled = True
            idx = 1
            'ulid, uluser, ulaktif
            dtUser = New DataTable
            With dtUser
                .Clear()
                .Columns.Add("ulid")
                .Columns.Add("uluser")
                .Columns.Add("ulaktif")
                .Columns.Add("ullokasi")
                .Columns.Add("ulurl")
            End With

            dtConfig = New DataTable
            With dtConfig
                .Clear()
                .Columns.Add("lokasi")
                .Columns.Add("url")
                .Columns.Add("koneksi")
                .Columns.Add("HitungUlang")
                .Columns.Add("AwalHitungUlang")
                .Columns.Add("AkhirHitungUlang")
                .Columns.Add("Reminder")
            End With

            idx = 2
            config = File.ReadAllText(AppConfig)
            Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                While reader.Read()
                    Select Case reader.NodeType
                        Case XmlNodeType.Element
                            Select Case reader.Name
                                Case "PathQueue" : PathQueue = reader.ReadElementContentAsString()
                                Case "PathQueueUser" : PQUser = reader.ReadElementContentAsString()
                                Case "CountLogout" : CountLogout = reader.ReadElementContentAsString()
                                Case "JumlahAgent" : JmlAgent = reader.ReadElementContentAsString()
                                Case "waktuHitungUlang"
                                    waktuHitungUlang = reader.ReadElementContentAsString()
                                    If waktuHitungUlang.Split(".").Length = 2 Then
                                        jamHItungUlang = waktuHitungUlang.Split(".")(0)
                                        menitHitungUlang = waktuHitungUlang.Split(".")(1)
                                    ElseIf waktuHitungUlang.Split(".").Length = 3 Then
                                        If IsNumeric(waktuHitungUlang.Split(".")(0)) Then
                                            tglHItungUlang = waktuHitungUlang.Split(".")(0)
                                        Else
                                            hariHitungUlang = waktuHitungUlang.Split(".")(0)
                                        End If
                                        jamHItungUlang = waktuHitungUlang.Split(".")(1)
                                        menitHitungUlang = waktuHitungUlang.Split(".")(2)
                                    End If
                                Case "app"
                                    dr = dtConfig.NewRow()
                                    dr(0) = reader.ReadElementContentAsString()
                                    dr(3) = True
                                    dr(6) = True
                                    dtConfig.Rows.Add(dr)
                                Case "waktuReminder"
                                    waktuReminder = reader.ReadElementContentAsString()
                                    If waktuReminder.Split(".").Length = 2 Then
                                        jamReminder = waktuReminder.Split(".")(0)
                                        menitReminder = waktuReminder.Split(".")(1)
                                    ElseIf waktuReminder.Split(".").Length = 3 Then
                                        If IsNumeric(waktuReminder.Split(".")(0)) Then
                                            tglReminder = waktuReminder.Split(".")(0)
                                        Else
                                            hariReminder = waktuReminder.Split(".")(0)
                                        End If
                                        jamReminder = waktuReminder.Split(".")(1)
                                        menitReminder = waktuReminder.Split(".")(2)
                                    End If
                                Case "intervalReminder" : intervalReminder = reader.ReadElementContentAsString()
                            End Select
                    End Select
                End While
            End Using

            idx = 3
            'setting delay timer for print
            AddHandler MyTimerUser.Elapsed, AddressOf OnTimedEventUser
            MyTimerUser.Interval = 1 * 1000
            MyTimerUser.Enabled = True

            Listener = New TcpListener(IPAddress.Any, 421)
            Listener.Start()
            Listener.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClient), Listener)

            ListenerCOGS = New TcpListener(IPAddress.Any, 422)
            ListenerCOGS.Start()
            ListenerCOGS.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClientCOGS), ListenerCOGS)
            With dt
                .Clear()
                .Columns.Clear()
                .Columns.Add("id")
                .Columns.Add("msmq")
            End With
            With dtCOGS
                .Clear()
                .Columns.Clear()
                .Columns.Add("msmq")
            End With
            dtObj.Rows.Add(dtObj.NewRow)
            For i = 0 To dtConfig.Rows.Count - 1
                AppConfig = dtConfig.Rows(i)("lokasi") + "\app\app.xml"
                If My.Computer.FileSystem.FileExists(AppConfig) Then
                    config = File.ReadAllText(AppConfig)
                    idx = 4.12
                    Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                        While reader.Read()
                            Select Case reader.NodeType
                                Case XmlNodeType.Element
                                    Select Case reader.Name
                                        Case "url" : dtConfig.Rows(i)("url") = reader.ReadElementContentAsString()
                                    End Select
                            End Select
                        End While
                    End Using
                Else
                    log("Directory App Not Exist : " + AppConfig)
                End If

                AppConfig = dtConfig.Rows(i)("lokasi") + "\report\config\app.config"
                If My.Computer.FileSystem.FileExists(AppConfig) Then
                    Dim koneksi As String
                    koneksi = File.ReadAllText(AppConfig)
                    koneksi = Encoding.UTF8.GetString(Convert.FromBase64String(koneksi))
                    Using reader As XmlReader = XmlReader.Create(New StringReader(koneksi))
                        While reader.Read()
                            Select Case reader.NodeType
                                Case XmlNodeType.Element
                                    Select Case reader.Name
                                        Case "ConStr"
                                            dtConfig.Rows(i)("koneksi") = reader.ReadElementContentAsString()
                                            dtTemp = AsDataTableAmbilDariDB("SELECT ulid, uluser, ulaktif FROM m0_userlogin", dtConfig.Rows(i)("koneksi"))
                                            For x = 0 To dtTemp.Rows.Count - 1
                                                dr = dtUser.NewRow()
                                                dr(0) = dtTemp.Rows(x)("ulid")
                                                dr(1) = dtTemp.Rows(x)("uluser")
                                                dr(2) = dtTemp.Rows(x)("ulaktif")
                                                dr(3) = dtConfig.Rows(i)("lokasi")
                                                dr(4) = dtConfig.Rows(i)("url")
                                                dtUser.Rows.Add(dr)
                                            Next
                                    End Select
                            End Select
                        End While
                    End Using

                Else
                    log("Directory report config not exist : " + dt.Rows(i)("lokasi"))
                End If
            Next
        Catch ex As Exception
            log("Error StartConfig : " & Err.Description + ", index : " + idx.ToString)
        End Try
    End Sub

    Sub ListenQueue()
    Try
            'Cek apakah nama antriannya sudah ada di msmq pc/belum.
            If MessageQueue.Exists(PathQueue) = False Then
                queue = MessageQueue.Create(PathQueue)
                queue.SetPermissions("Administrator", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow)
                queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow)
            End If

            queue.Path = PathQueue
            queue.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})

            AddHandler queue.ReceiveCompleted, AddressOf GetMessageQueue

            queue.BeginReceive()
        Catch ex As Exception
            log("Error ListenQueue1 : " & Err.Description)
            Try
                queue.BeginReceive()
            Catch e As Exception
                log("Error ListenQueue2 : " & Err.Description)
            End Try
        End Try
        Return
    End Sub

    Sub ListenQueueUser()
        Try
            'Cek apakah nama antriannya sudah ada di msmq pc/belum.
            If MessageQueue.Exists(PQUser) = False Then
                QUser = MessageQueue.Create(PQUser)
                QUser.SetPermissions("Administrator", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow)
                QUser.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow)
            End If

            QUser.Path = PQUser
            QUser.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})

            AddHandler QUser.ReceiveCompleted, AddressOf GetMessageQueueUser
            MyTimerUser.Enabled = True
            MyTimerUser.Start()
            QUser.BeginReceive()
        Catch ex As Exception
            log("Error ListenQueueUser1 : " & Err.Description)
            Try
                QUser.BeginReceive()
            Catch e As Exception
                log("Error ListenQueueUser2 : " & Err.Description)
            End Try
        End Try
        Return
    End Sub

    Public Sub GetMessageQueue(ByVal [source] As [Object], ByVal asyncResult As ReceiveCompletedEventArgs)
        Dim s As String = "", arr() As String
        Try
            Dim mq As MessageQueue = CType([source], MessageQueue)
            Dim m As Message = mq.EndReceive(asyncResult.AsyncResult)

            s = m.Body
            If s.Length = 0 Then
                log("Error GetMessageQueue : MessageQueue Empty")
            Else
                urutan = urutan + 1
                log("GetMessageQueue(" + (urutan).ToString + ") : " + s)
                arr = s.Split(sptField)

                If arr.Length = 1 Then
                    log("Error GetMessageQueue : format data salah, data : " + s)
                ElseIf arr(0) = "C" Then
                    '' jika dapat request HPP

                    'Add MessageQueue in Datatable 'dtCOGS'
                    dr = dtCOGS.NewRow()
                    dr(0) = s
                    dtCOGS.Rows.Add(dr)

                    'Request AgentCOGS After get MessageQueue
                    requestAgentCOGS()
                Else
                    '' jika dapat request Cetak atau Jurnal

                    'Add MessageQueue in Datatable 'dt'
                    dr = dt.NewRow()
                    dr(0) = s.Split(sptField)(0)
                    dr(1) = s
                    dt.Rows.Add(dr)

                    'Request Agent After get MessageQueue
                    requestAgent(True)
                End If

            End If

            ' Restart the asynchronous Receive operation.
            queue.BeginReceive()
            Return
        Catch ex As Exception
            ' Restart the asynchronous Receive operation.
            queue.BeginReceive()
            Try
                log("Error GetMessageQueue1 : " & Err.Description & " Data : " & s)
            Catch e As Exception
                log("Error GetMessageQueue2 : " & Err.Description & " Data : " & s)
            End Try
        End Try
    End Sub

    Sub GetMessageQueueUser(ByVal [source] As [Object], ByVal asyncResult As ReceiveCompletedEventArgs)
        Dim mq As MessageQueue = CType([source], MessageQueue)
        Dim m As Message = mq.EndReceive(asyncResult.AsyncResult)
        Dim s, type, idlogin, userid, lokasi As String
        s = ""
        Try
            s = m.Body
            type = s.Split(sptField)(0)
            idlogin = s.Split(sptField)(1)
            userid = s.Split(sptField)(2)
            lokasi = s.Split(sptField)(3)
            If type = "login" Then
                dr = dtUser.NewRow()
                dr(0) = idlogin
                dr(1) = userid
                dr(2) = 1
                dr(3) = lokasi
                dr(4) = ""
                For d = 0 To dtConfig.Rows.Count - 1
                    If dtConfig.Rows(d)("lokasi") = lokasi Then
                        dr(4) = dtConfig.Rows(d)("url") : GoTo cariselesai
                    End If
                Next

                AppConfig = lokasi + "\app\app.xml"
                If My.Computer.FileSystem.FileExists(AppConfig) Then
                    config = File.ReadAllText(AppConfig)
                    Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                        While reader.Read()
                            Select Case reader.NodeType
                                Case XmlNodeType.Element
                                    Select Case reader.Name
                                        Case "url" : dr(4) = reader.ReadElementContentAsString()
                                    End Select
                            End Select
                        End While
                    End Using
                Else
                    log("Directory New App Not Exist : " + lokasi)
                End If
cariselesai:
                dtUser.Rows.Add(dr)
            ElseIf type = "check" Then
                For i = 0 To dtUser.Rows.Count - 1
                    If dtUser.Rows(i)("ulid") = idlogin And dtUser.Rows(i)("ullokasi") = lokasi Then
                        dtUser.Rows(i)("ulaktif") = 0 : GoTo selesai
                    End If
                Next

                dr = dtUser.NewRow()
                dr(0) = idlogin
                dr(1) = userid
                dr(2) = 1
                dr(3) = lokasi
                dr(4) = ""
                For d = 0 To dtConfig.Rows.Count - 1
                    If dtConfig.Rows(d)("lokasi") = lokasi Then
                        dr(4) = dtConfig.Rows(d)("url") : GoTo cariselesai2
                    End If
                Next

                AppConfig = lokasi + "\app\app.xml"
                If My.Computer.FileSystem.FileExists(AppConfig) Then
                    config = File.ReadAllText(AppConfig)
                    Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                        While reader.Read()
                            Select Case reader.NodeType
                                Case XmlNodeType.Element
                                    Select Case reader.Name
                                        Case "url" : dr(4) = reader.ReadElementContentAsString()
                                    End Select
                            End Select
                        End While
                    End Using
                Else
                    log("Directory New App Not Exist : " + lokasi)
                End If
cariselesai2:
                dtUser.Rows.Add(dr)
            ElseIf type = "logout" Then
                For i = 0 To dtUser.Rows.Count - 1
                    If dtUser.Rows(i)("ulid") = idlogin And dtUser.Rows(i)("ullokasi") = lokasi Then
                        dtUser.Rows(i)("ulid") = "del" : GoTo selesai
                    End If
                Next
            End If
        Catch ex As Exception
            ' Restart the asynchronous Receive operation.
            QUser.BeginReceive()
            Try
                log("Error GetMessageQueue1 : " & Err.Description & " Data : " & s)
            Catch e As Exception
                log("Error GetMessageQueue2 : " & Err.Description & " Data : " & s)
            End Try
        End Try
selesai:
        ' Restart the asynchronous Receive operation.
        QUser.BeginReceive()
    End Sub

    Public Sub requestAgent(ByVal validasi As Boolean)
        Try
            If validasi Then
                If Int(JmlAgent) > 0 And dt.Rows.Count > 0 And go = True Then
                    go = False
                    JmlAgent -= 1

                    vThread = New Threading.Thread(AddressOf runAgent)
                    vThread.Start()
                End If
            Else
                If Int(JmlAgent) > 0 And dt.Rows.Count > 0 Then
                    JmlAgent -= 1

                    vThread = New Threading.Thread(AddressOf runAgent)
                    vThread.Start()
                Else
                    go = True
                End If
            End If
        Catch ex As Exception
            log("Error requestAgent : " & Err.Description)
        End Try
    End Sub

    Public Sub requestAgentCOGS()
        Try
            If cogs And dtCOGS.Rows.Count > 0 Then
                cogs = False
                num = 0
                requestAgentCOGS_()
            End If
        Catch ex As Exception
            log("Error requestAgentCOGS : " & Err.Description)
        End Try
    End Sub

    Public Sub requestAgentCOGS_()
        Try
            ar = dtCOGS.Rows(num)(0).ToString.Split(sptField)
            lokasi = ar(ar.Length - 1)

            If dtObj.Columns.Contains(lokasi) = False Then
                dtObj.Columns.Add(lokasi)
                dtObj(0)(lokasi) = True
            End If

            If dtObj(0)(lokasi) Then
                dtObj(0)(lokasi) = False

                vThreadCOGS = New Threading.Thread(AddressOf runAgentCOGS)  'Buat thread baru
                vThreadCOGS.Start()    'Jalankan thread
            Else
                num = num + 1
                If dtCOGS.Rows.Count > num Then
                    requestAgentCOGS_()
                Else
                    cogs = True
                End If
            End If
        Catch ex As Exception
            log("Error requestAgentCOGS_ : " & Err.Description)
        End Try
    End Sub

    Sub runAgent()
        Dim ar() As String = dt.Rows(0)(1).ToString.Split(sptField)
        Try
            log("Get AGENT : " + ar(ar.Length - 1) + "report\config\Agent.exe")
            Dim startInfo As New ProcessStartInfo(ar(ar.Length - 1) + "report\config\Agent.exe")
            startInfo.WindowStyle = ProcessWindowStyle.Minimized
            startInfo.WindowStyle = ProcessWindowStyle.Hidden
            startInfo.CreateNoWindow = False
            startInfo.UseShellExecute = False
            Process.Start(startInfo)

        Catch ex As Exception
            Try
                go = True
                JmlAgent += 1
                log("Error runAgent1 : " & Err.Description & ", Lokasi Agent : " + ar(ar.Length - 1) + "report\config\Agent.exe")
            Catch e As Exception
                log("Error runAgent2 : " & Err.Description)
            End Try
        End Try
    End Sub

    Sub runAgentCOGS()
        Try
            log("Get AGENT COGS : " + lokasi + "report\config\AgentCogs.exe")
            Dim startInfo As New ProcessStartInfo(lokasi + "report\config\AgentCogs.exe")
            startInfo.WindowStyle = ProcessWindowStyle.Minimized
            startInfo.WindowStyle = ProcessWindowStyle.Hidden
            startInfo.CreateNoWindow = False
            startInfo.UseShellExecute = False
            Process.Start(startInfo)
        Catch ex As Exception
            Try
                cogs = True
                dtObj(0)(lokasi) = True
                log("Error runAgentCOGS1 : " & Err.Description & ", Lokasi Agent : " + lokasi + "report\config\AgentCogs.exe")
            Catch e As Exception
                log("Error runAgentCOGS2 : " & Err.Description)
            End Try
        End Try
    End Sub

    Sub AcceptClient(ByVal ar As IAsyncResult)
        Try
            Client = New ChatClient(Listener.EndAcceptTcpClient(ar))
            AddHandler (Client.ClientExited), AddressOf AgentClosed
            MyTimer.Start()

        Catch ex As Exception
            log("Error AcceptClient : " & Err.Description)
        End Try
    End Sub

    Private Sub OnTimedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
        Try
            xStep = 0
            MyTimer.Stop()
            xStep = 1
            If dt.Rows.Count > 0 Then
                xStep = 2
                Client.Send(dt.Rows(0)(1))
                xStep = 3
                dt.Rows.RemoveAt(0)
                xStep = 4
            End If
            xStep = 5
            requestAgent(False)
            xStep = 6
            Listener.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClient), Listener)
            xStep = 7
        Catch ex As Exception
            log("Error OnTimedEvent : " + Err.Description + " xStep : " + xStep.ToString)
        End Try
    End Sub

    Sub AcceptClientCOGS(ByVal ar As IAsyncResult)
        Try
            Dim Client As ChatClient = New ChatClient(ListenerCOGS.EndAcceptTcpClient(ar))
            AddHandler (Client.ClientExited), AddressOf ClientExitedCOGS
            Client.lokasi = lokasi
            Client.Send(dtCOGS.Rows(num)("msmq"))
            dtCOGS.Rows.RemoveAt(num)
            cogs = True
            requestAgentCOGS()
            ListenerCOGS.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClientCOGS), ListenerCOGS)
        Catch ex As Exception
            log("Error AcceptClientCOGS : " & Err.Description)
        End Try
    End Sub

    Sub AgentClosed(ByVal Client As ChatClient)
        Try
            JmlAgent += 1
            requestAgent(True)
        Catch ex As Exception
            log("Error AgentClosed : " & Err.Description)
        End Try
    End Sub

    Sub ClientExitedCOGS(ByVal Client As ChatClient)
        Try
            dtObj(0)(Client.lokasi) = True
            requestAgentCOGS()
        Catch ex As Exception
            log("Error ClientExitedCOGS : " & Err.Description)
        End Try
    End Sub

    Function WS_Request(ByVal URL As String, ByVal POSTdata As String, Optional ByVal method As String = "POST") As String
        Dim responseData As String = ""

        'TAMBAHKAN PARAM
        'POSTdata = System.Web.HttpUtility.UrlEncode(POSTdata)

        Try

            Dim cookieJar As New Net.CookieContainer()
            Dim hwrequest As Net.HttpWebRequest = Net.WebRequest.Create(URL)

            hwrequest.CookieContainer = cookieJar
            hwrequest.Accept = "*/*"
            hwrequest.AllowAutoRedirect = True
            hwrequest.UserAgent = "http_requester/0.1"
            hwrequest.Timeout = 999999999
            hwrequest.ReadWriteTimeout = 999999999
            hwrequest.Method = method
            hwrequest.KeepAlive = False
            hwrequest.ProtocolVersion = HttpVersion.Version10

            If hwrequest.Method = "POST" Then

                hwrequest.ContentType = "application/x-www-form-urlencoded"

                Dim encoding As New UTF8Encoding() 'Use UTF8Encoding for XML requests
                Dim postByteArray() As Byte = encoding.GetBytes(POSTdata)

                hwrequest.ContentLength = postByteArray.Length

                Dim postStream As IO.Stream = hwrequest.GetRequestStream()

                postStream.Write(postByteArray, 0, postByteArray.Length)
                postStream.Close()

            End If

            Dim hwresponse As Net.HttpWebResponse = hwrequest.GetResponse()

            If hwresponse.StatusCode = Net.HttpStatusCode.OK Then

                Dim responseStream As IO.StreamReader = _
                  New IO.StreamReader(hwresponse.GetResponseStream())
                responseData = responseStream.ReadToEnd()

            End If

            hwresponse.Close()

        Catch e As Exception
            responseData = "An error occurred: " & e.Message
            log(responseData)

        End Try

        Return responseData

    End Function

    Private Sub f_splitWs(ByVal url As String, ByVal param As String)
        'Test Load Web Service

        Dim stepke As Double = 0

        Try

            stepke = 1

            Dim WsReturn As String = "", sql As String = ""
            Dim datasptParam As String
            Dim datasptRow As String()
            Dim datasptField As String()
            Dim setDtConfig As Integer = 1
            WsReturn = WS_Request(url + "/ws/myerpplus.asmx/Ws", "param=" + param)
            'log(url + "/ws/myerpplus.asmx/Ws" + "param=" + param)
            'log(WsReturn)

            If WsReturn.Contains("M2_Accounting_PeriodSearch") Then

                stepke = 2

                datasptParam = WsReturn.Split(sptParam)(2)
                datasptRow = datasptParam.Split(sptRow)
                'log("datasptParam : " + datasptParam)
                'log("datasptRow : " + datasptRow.Length.ToString)
                For d = 0 To dtConfig.Rows.Count - 1
                    If dtConfig.Rows(d)("url") = url Then
                        setDtConfig = d
                        Exit For
                    End If
                Next

                Dim AkhirHitungUlang As String = ""

                stepke = 3

                For i = 0 To datasptRow.Length - 1
                    datasptField = datasptRow(i).Split(sptField)
                    If i = 0 Then
                        dtConfig.Rows(setDtConfig)("AwalHitungUlang") = datasptField(1) + "-" + datasptField(2) + "-01"
                    End If

                    Dim firstDayOfMonth = New DateTime(datasptField(1), datasptField(2), 1)
                    Dim lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1)

                    'If i <= 2 Then
                    AkhirHitungUlang = datasptField(1) + "-" + datasptField(2) + "-" + lastDayOfMonth.Day.ToString
                    'End If
                    dtConfig.Rows(setDtConfig)("AkhirHitungUlang") = datasptField(1) + "-" + datasptField(2) + "-" + lastDayOfMonth.Day.ToString
                    'log(datasptField(1) + "-" + datasptField(2) + "-31")
                Next

                stepke = 4

                Dim wsHPP As String = "M0_CogsHitungUlang_Fifo"
                Dim dtSetting = AsDataTableAmbilDariDB("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'HppGlobal'", dtConfig.Rows(setDtConfig)("koneksi"))
                For x = 0 To dtSetting.Rows.Count - 1

                    For d = 0 To dtConfig.Rows.Count - 1

                        stepke = 5

                        If dtConfig.Rows(d)("url") = url Then
                            setDtConfig = d

                            'sql = "ALTER EVENT `Booking PO` DISABLE; ALTER EVENT `Booking SO` DISABLE; ALTER EVENT `Stok Global` DISABLE; ALTER EVENT `Stok Pergudang` DISABLE; TRUNCATE TABLE m0_hitungulang_log; DELETE FROM m0_hppaverage WHERE tgl >= '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction_history SET hppfix = 1;"
                            'sql = "TRUNCATE TABLE m0_hitungulang_log; DELETE FROM m0_hppaverage WHERE tgl >= '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction_history SET hppfix = 1;"
                            sql = "TRUNCATE TABLE m0_userlogin; UPDATE m0_user SET uaktif = 0; TRUNCATE TABLE m0_hitungulang_log; UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "'; UPDATE m1_item_transaction_history SET hppfix = 1;"
                            log(dtConfig.Rows(setDtConfig)("url") & " - Update Hpp Fix - " & sql)
                            currSql = sql
                            currKoneksi = dtConfig.Rows(d)("koneksi")

                            Dim ConX As MySqlConnection = New MySqlConnection
                            ConX.ConnectionString = dtConfig.Rows(d)("koneksi")
                            ConX.Open()
                            With New MySql.Data.MySqlClient.MySqlCommand()
                                .Connection = ConX
                                .CommandType = CommandType.Text
                                .CommandText = sql
                                .CommandTimeout = 999999999
                                .ExecuteNonQuery()
                                .Dispose()
                            End With
                            Exit For
                        End If
                    Next

                    If dtSetting.Rows(x)("snilai") = "R" Then

                        stepke = 6

                        Dim dtSetting2 = AsDataTableAmbilDariDB("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'WsHitungUlangHPP'", dtConfig.Rows(setDtConfig)("koneksi"))
                        For x2 = 0 To dtSetting2.Rows.Count - 1
                            wsHPP = dtSetting2.Rows(x2)("snilai")
                        Next

                        param = "1★" & wsHPP & "★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & AkhirHitungUlang & "▼0"

                        currUrl = url
                        currParam = param

                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang HPP " & wsHPP & " tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & AkhirHitungUlang & " - " & param)
                        f_splitWs(url, param)

                    ElseIf dtSetting.Rows(x)("snilai") = "F" Then

                        stepke = 7

                        wsHPP = "M0_CogsHitungUlang_Fifo"

                        param = "1★" & wsHPP & "★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & AkhirHitungUlang & "▼▼"

                        currUrl = url
                        currParam = param

                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang HPP " & wsHPP & " tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & AkhirHitungUlang & " - " & param)
                        f_splitWs(url, param)

                    End If
                Next

                'JIKA HITUNG ULANG ERROR TIMEOUT ATAU CONNECTION NOT VALID
            ElseIf WsReturn.Contains("M0_CogsHitungUlang") And _
                (WsReturn.Contains("Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.") Or _
                 WsReturn.Contains("Connection must be valid and open to rollback transaction") Or _
                 WsReturn.Contains("Deadlock found when trying to get lock; try restarting transaction") Or _
                 WsReturn.Contains("failed")) Then

                stepke = 8

                param = currParam
                url = currUrl
                sql = currSql

                log(url & " - Ulangi Proses " & " - Update Hpp Fix - " & sql)
                Dim ConX As MySqlConnection = New MySqlConnection
                ConX.ConnectionString = currKoneksi
                ConX.Open()
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = ConX
                    .CommandType = CommandType.Text
                    .CommandText = sql
                    .CommandTimeout = 999999999
                    .ExecuteNonQuery()
                    .Dispose()
                End With

                log(url & " - Ulangi Proses - " & param & " - " & WsReturn)
                f_splitWs(url, param)

                'JIKA HITUNG ULANG SUKSES
            ElseIf WsReturn.Contains("M0_CogsHitungUlang") Then

                stepke = 9

                For d = 0 To dtConfig.Rows.Count - 1

                    stepke = 10

                    If dtConfig.Rows(d)("url") = url Then
                        setDtConfig = d

                        'sql = "ALTER EVENT `Booking PO` ENABLE; ALTER EVENT `Booking SO` ENABLE; ALTER EVENT `Stok Global` ENABLE; ALTER EVENT `Stok Pergudang` ENABLE;"
                        'sql = "ALTER EVENT `Booking PO` ENABLE; ALTER EVENT `Booking SO` ENABLE; ALTER EVENT `Stok Global` ENABLE;"
                        sql = "UPDATE m0_user SET uaktif = 1; "

                        'log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang Stok per gudang dan global")
                        log(dtConfig.Rows(setDtConfig)("url") & " - Enable event - " & sql)
                        Dim ConX As MySqlConnection = New MySqlConnection
                        ConX.ConnectionString = dtConfig.Rows(d)("koneksi")
                        ConX.Open()
                        With New MySql.Data.MySqlClient.MySqlCommand()
                            .Connection = ConX
                            .CommandType = CommandType.Text
                            '.CommandText = "DELETE FROM m1_item_stock_warehouse;INSERT INTO m1_item_stock_warehouse (SELECT b.bid, tb.gudang,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stokfix	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang);UPDATE `m1_item` SET `bstok`='0';UPDATE(SELECT idbarang, round(SUM(stok), 5) stok FROM m1_item_stock_warehouse GROUP BY idbarang) h JOIN m1_item i ON i.bid = h.idbarang SET i.bstok = h.stok"
                            .CommandText = sql
                            .CommandTimeout = 999999999
                            .ExecuteNonQuery()
                            .Dispose()
                        End With
                        Exit For
                    End If
                Next

                stepke = 11

                param = "1★M0_ReqJournalUlang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & dtConfig.Rows(setDtConfig)("AkhirHitungUlang") & "▼▼▼▼0▼1▼0"
                log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang M0_JournalUlang tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & dtConfig.Rows(setDtConfig)("AkhirHitungUlang") & " - " & param)

                'f_splitWs(url, "1★M0_ReqJournalUlang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & dtConfig.Rows(setDtConfig)("AkhirHitungUlang") & "▼▼▼▼0▼0▼0")
                f_splitWs(url, param)

                log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang selesai")

            End If

        Catch ex As Exception
            log("Step : " & stepke & " - Informasi Split WS : " + Err.Description + " URL : " + url + "/ws/myerpplus.asmx/Ws?param=" + param)

        End Try

    End Sub

    Private Sub f_splitWs20190806(ByVal url As String, ByVal param As String)
        'Test Load Web Service
        Try

            Dim WsReturn As String = ""
            Dim datasptParam As String
            Dim datasptRow As String()
            Dim datasptField As String()
            Dim setDtConfig As Integer = 1
            WsReturn = WS_Request(url + "/ws/myerpplus.asmx/Ws", "param=" + param)
            'log(url + "/ws/myerpplus.asmx/Ws" + "param=" + param)
            'log(WsReturn)
            If WsReturn.Contains("M2_Accounting_PeriodSearch") Then
                datasptParam = WsReturn.Split(sptParam)(2)
                datasptRow = datasptParam.Split(sptRow)
                'log("datasptParam : " + datasptParam)
                'log("datasptRow : " + datasptRow.Length.ToString)
                For d = 0 To dtConfig.Rows.Count - 1
                    If dtConfig.Rows(d)("url") = url Then
                        setDtConfig = d
                        Exit For
                    End If
                Next

                Dim AkhirHitungUlang As String = ""

                For i = 0 To datasptRow.Length - 1
                    datasptField = datasptRow(i).Split(sptField)
                    If i = 0 Then
                        dtConfig.Rows(setDtConfig)("AwalHitungUlang") = datasptField(1) + "-" + datasptField(2) + "-01"
                    End If

                    Dim firstDayOfMonth = New DateTime(datasptField(1), datasptField(2), 1)
                    Dim lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1)

                    If i <= 2 Then
                        AkhirHitungUlang = datasptField(1) + "-" + datasptField(2) + "-" + lastDayOfMonth.Day.ToString
                    End If
                    dtConfig.Rows(setDtConfig)("AkhirHitungUlang") = datasptField(1) + "-" + datasptField(2) + "-" + lastDayOfMonth.Day.ToString
                    'log(datasptField(1) + "-" + datasptField(2) + "-31")
                Next

                Dim wsHPP As String = "M0_CogsHitungUlang_Fifo"
                Dim dtSetting = AsDataTableAmbilDariDB("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'HppGlobal'", dtConfig.Rows(setDtConfig)("koneksi"))
                For x = 0 To dtSetting.Rows.Count - 1

                    For d = 0 To dtConfig.Rows.Count - 1
                        If dtConfig.Rows(d)("url") = url Then
                            setDtConfig = d
                            log(dtConfig.Rows(setDtConfig)("url") & " - Update Hpp Fix - " & "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl BETWEEN '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "' AND '" & AkhirHitungUlang & "';")
                            Dim ConX As MySqlConnection = New MySqlConnection
                            ConX.ConnectionString = dtConfig.Rows(d)("koneksi")
                            ConX.Open()
                            With New MySql.Data.MySqlClient.MySqlCommand()
                                .Connection = ConX
                                .CommandType = CommandType.Text
                                .CommandText = "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl BETWEEN '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "' AND '" & AkhirHitungUlang & "';"
                                .ExecuteNonQuery()
                                .Dispose()
                            End With
                            Exit For
                        End If
                    Next

                    If dtSetting.Rows(x)("snilai") = "R" Then
                        Dim dtSetting2 = AsDataTableAmbilDariDB("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'WsHitungUlangHPP'", dtConfig.Rows(setDtConfig)("koneksi"))
                        For x2 = 0 To dtSetting2.Rows.Count - 1
                            wsHPP = dtSetting2.Rows(x2)("snilai")
                        Next

                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang HPP " & wsHPP & " tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & AkhirHitungUlang)
                        f_splitWs(url, "1★" & wsHPP & "★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & AkhirHitungUlang & "▼0")

                    ElseIf dtSetting.Rows(x)("snilai") = "F" Then
                        wsHPP = "M0_CogsHitungUlang_Fifo"

                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang HPP " & wsHPP & " tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & AkhirHitungUlang)
                        f_splitWs(url, "1★" & wsHPP & "★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & AkhirHitungUlang & "▼▼")

                    End If
                Next

            ElseIf WsReturn.Contains("M0_CogsHitungUlang") Then
                For d = 0 To dtConfig.Rows.Count - 1
                    If dtConfig.Rows(d)("url") = url Then
                        setDtConfig = d
                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang Stok per gudang dan global")
                        Dim ConX As MySqlConnection = New MySqlConnection
                        ConX.ConnectionString = dtConfig.Rows(d)("koneksi")
                        ConX.Open()
                        With New MySql.Data.MySqlClient.MySqlCommand()
                            .Connection = ConX
                            .CommandType = CommandType.Text
                            .CommandText = "DELETE FROM m1_item_stock_warehouse;INSERT INTO m1_item_stock_warehouse (SELECT b.bid, tb.gudang,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stokfix	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang);UPDATE `m1_item` SET `bstok`='0';UPDATE(SELECT idbarang, round(SUM(stok), 5) stok FROM m1_item_stock_warehouse GROUP BY idbarang) h JOIN m1_item i ON i.bid = h.idbarang SET i.bstok = h.stok"
                            .ExecuteNonQuery()
                            .Dispose()
                        End With
                        Exit For
                    End If
                Next
                log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang M0_JournalUlang tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & dtConfig.Rows(setDtConfig)("AkhirHitungUlang"))
                f_splitWs(url, "1★M0_ReqJournalUlang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & dtConfig.Rows(setDtConfig)("AkhirHitungUlang") & "▼▼▼▼0▼0▼0")
                log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang selesai")
            End If
        Catch ex As Exception
            log("Informasi Split WS : " + Err.Description + " URL : " + url + "/ws/myerpplus.asmx/Ws?param=" + param)
        End Try
    End Sub

    Public Function AsDataTableAmbilDariDB(ByVal StrSQL As String, ByVal strCon As String) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)
        da1.SelectCommand.CommandTimeout = 31536000

        xStep = 4 'Set datatable
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        Return dt1
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDataTableAmbilDariDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dt1
    End Function

    Public Function AsDataTableAmbilDariDB20250729(ByVal StrSQL As String, ByVal strCon As String) As DataTable
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

    'Private Sub f_splitWs(ByVal url As String, ByVal param As String)
    '    'Test Load Web Service
    '    Try
    '        Dim WsReturn As String = ""
    '        Dim datasptParam As String
    '        Dim datasptRow As String()
    '        Dim datasptField As String()
    '        Dim setDtConfig As Integer = 1
    '        Dim req As HttpWebRequest
    '        Dim res As HttpWebResponse
    '        Dim webStream As Stream
    '        req = WebRequest.Create(url + "/ws/myerpplus.asmx/Ws?param=" + param)
    '        req.Method = "GET"
    '        res = req.GetResponse()
    '        webStream = res.GetResponseStream()
    '        Using reader = New StreamReader(res.GetResponseStream())
    '            WsReturn = reader.ReadToEnd()
    '            If WsReturn.Contains("M2_Accounting_PeriodSearch") Then
    '                datasptParam = WsReturn.Split(sptParam)(2)
    '                datasptRow = datasptParam.Split(sptRow)
    '                'log("datasptParam : " + datasptParam)
    '                'log("datasptRow : " + datasptRow.Length.ToString)
    '                For d = 0 To dtConfig.Rows.Count - 1
    '                    If dtConfig.Rows(d)("url") = url Then
    '                        setDtConfig = d
    '                        Exit For
    '                    End If
    '                Next

    '                Dim AkhirHitungUlang As String = ""

    '                For i = 0 To datasptRow.Length - 1
    '                    datasptField = datasptRow(i).Split(sptField)
    '                    If i = 0 Then
    '                        dtConfig.Rows(setDtConfig)("AwalHitungUlang") = datasptField(1) + "-" + datasptField(2) + "-01"
    '                    End If

    '                    Dim firstDayOfMonth = New DateTime(datasptField(1), datasptField(2), 1)
    '                    Dim lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1)

    '                    If i <= 2 Then
    '                        AkhirHitungUlang = datasptField(1) + "-" + datasptField(2) + "-" + lastDayOfMonth.Day.ToString
    '                    End If
    '                    dtConfig.Rows(setDtConfig)("AkhirHitungUlang") = datasptField(1) + "-" + datasptField(2) + "-" + lastDayOfMonth.Day.ToString
    '                    'log(datasptField(1) + "-" + datasptField(2) + "-31")
    '                Next

    '                Dim wsHPP As String = "M0_CogsHitungUlang_Fifo"
    '                Dim dtSetting = AsDataTableAmbilDariDB("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'HppGlobal'", dtConfig.Rows(setDtConfig)("koneksi"))
    '                For x = 0 To dtSetting.Rows.Count - 1

    '                    For d = 0 To dtConfig.Rows.Count - 1
    '                        If dtConfig.Rows(d)("url") = url Then
    '                            setDtConfig = d
    '                            log(dtConfig.Rows(setDtConfig)("url") & " - Update Hpp Fix - " & "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl BETWEEN '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "' AND '" & AkhirHitungUlang & "';")
    '                            Dim ConX As MySqlConnection = New MySqlConnection
    '                            ConX.ConnectionString = dtConfig.Rows(d)("koneksi")
    '                            ConX.Open()
    '                            With New MySql.Data.MySqlClient.MySqlCommand()
    '                                .Connection = ConX
    '                                .CommandType = CommandType.Text
    '                                .CommandText = "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl BETWEEN '" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "' AND '" & AkhirHitungUlang & "';"
    '                                .ExecuteNonQuery()
    '                                .Dispose()
    '                            End With
    '                            Exit For
    '                        End If
    '                    Next

    '                    If dtSetting.Rows(x)("snilai") = "R" Then
    '                        Dim dtSetting2 = AsDataTableAmbilDariDB("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'WsHitungUlangHPP'", dtConfig.Rows(setDtConfig)("koneksi"))
    '                        For x2 = 0 To dtSetting2.Rows.Count - 1
    '                            wsHPP = dtSetting2.Rows(x2)("snilai")
    '                        Next

    '                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang HPP " & wsHPP & " tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & AkhirHitungUlang)
    '                        f_splitWs(url, "1★" & wsHPP & "★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & AkhirHitungUlang & "▼0")

    '                    ElseIf dtSetting.Rows(x)("snilai") = "F" Then
    '                        wsHPP = "M0_CogsHitungUlang_Fifo"

    '                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang HPP " & wsHPP & " tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & AkhirHitungUlang)
    '                        f_splitWs(url, "1★" & wsHPP & "★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & AkhirHitungUlang & "▼▼")

    '                    End If
    '                Next

    '            ElseIf WsReturn.Contains("M0_CogsHitungUlang") Then
    '                For d = 0 To dtConfig.Rows.Count - 1
    '                    If dtConfig.Rows(d)("url") = url Then
    '                        setDtConfig = d
    '                        log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang Stok per gudang dan global")
    '                        Dim ConX As MySqlConnection = New MySqlConnection
    '                        ConX.ConnectionString = dtConfig.Rows(d)("koneksi")
    '                        ConX.Open()
    '                        With New MySql.Data.MySqlClient.MySqlCommand()
    '                            .Connection = ConX
    '                            .CommandType = CommandType.Text
    '                            .CommandText = "DELETE FROM m1_item_stock_warehouse;INSERT INTO m1_item_stock_warehouse (SELECT b.bid, tb.gudang,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stokfix	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang);UPDATE m1_item SET bstok = 0; UPDATE m1_item i JOIN (SELECT isw.idbarang, ROUND(SUM(isw.stok),5) as totalstok FROM m1_item_stock_warehouse isw GROUP BY isw.idbarang ) as sp ON i.bid = sp.idbarang SET i.bstok = sp.totalstok WHERE i.bstok <> sp.totalstok;"
    '                            .ExecuteNonQuery()
    '                            .Dispose()
    '                        End With
    '                        Exit For
    '                    End If
    '                Next
    '                log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang M0_JournalUlang tanggal " & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & " sd " & dtConfig.Rows(setDtConfig)("AkhirHitungUlang"))
    '                f_splitWs(url, "1★M0_ReqJournalUlang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★1★0★" & dtConfig.Rows(setDtConfig)("AwalHitungUlang") & "▼" & dtConfig.Rows(setDtConfig)("AkhirHitungUlang") & "▼▼▼▼0▼0▼0")
    '                log(dtConfig.Rows(setDtConfig)("url") & " - Hitung ulang selesai")
    '            End If
    '        End Using
    '    Catch ex As Exception
    '        log("Informasi Split WS : " + Err.Description + " URL : " + url + "/ws/myerpplus.asmx/Ws?param=" + param)
    '    End Try
    'End Sub

    'Public Function AsDataTableAmbilDariDB(ByVal StrSQL As String, ByVal strCon As String) As DataTable
    '    Dim ConX As MySqlConnection = New MySqlConnection

    '    ConX.ConnectionString = strCon
    '    ConX.Open()

    '    Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

    '    Dim dt1 As DataTable = New DataTable()
    '    da1.Fill(dt1)

    '    'Tutup Koneksi
    '    ConX.Close()

    '    Return dt1
    'End Function

    Private Sub OnTimedEventUser20190806(ByVal source As Object, ByVal e As ElapsedEventArgs)
        Try
            If (dtUser.Rows.Count > 0) Then
                For i = dtUser.Rows.Count - 1 To 0 Step -1
                    dtUser.Rows(i)("ulaktif") += 1
                    If dtUser.Rows(i)("ulid") = "del" Then
                        dtUser.Rows.RemoveAt(i)
                    ElseIf dtUser.Rows(i)("ulaktif") >= CountLogout Then
                        If dtUser.Rows(i)("ulurl") = "" Then
                            log("URL tidak ada untuk lokasi : " + dtUser.Rows(i)("lokasi"))
                        Else
                            f_splitWs(dtUser.Rows(i)("ulurl"), dtUser.Rows(i)("ulid") + "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" + dtUser.Rows(i)("uluser").ToString + "★1★" + dtUser.Rows(i)("ullokasi"))
                            dtUser.Rows.RemoveAt(i)
                        End If

                    End If
                Next
            End If

            If waktuHitungUlang <> "" Then
                If tglHItungUlang <> 0 Then

                    If tglHItungUlang = Date.Now().Day And jamHItungUlang = Now.Hour And menitHitungUlang <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") Then
                                dtConfig.Rows(d)("HitungUlang") = False
                                log(dtConfig.Rows(d)("url") & " - Hitung ulang di mulai dan Cek Periode Akuntansi")
                                f_splitWs(dtConfig.Rows(d)("url"), "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                            End If
                        Next
                    End If
                    If tglHItungUlang <> Date.Now().Day And jamHItungUlang <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") = False Then
                                dtConfig.Rows(d)("HitungUlang") = True
                            End If
                        Next
                    End If

                ElseIf Len(hariHitungUlang) <> 0 Then

                    If hariHitungUlang.ToLower = WeekdayName(Weekday(Date.Now())).ToLower And jamHItungUlang = Now.Hour And menitHitungUlang <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") Then
                                dtConfig.Rows(d)("HitungUlang") = False
                                log(dtConfig.Rows(d)("url") & " - Hitung ulang di mulai dan Cek Periode Akuntansi")
                                f_splitWs(dtConfig.Rows(d)("url"), "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                            End If
                        Next
                    End If
                    If hariHitungUlang.ToLower <> WeekdayName(Weekday(Date.Now())).ToLower And jamHItungUlang <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") = False Then
                                dtConfig.Rows(d)("HitungUlang") = True
                            End If
                        Next
                    End If

                Else

                    If jamHItungUlang = Now.Hour And menitHitungUlang <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") Then
                                dtConfig.Rows(d)("HitungUlang") = False
                                log(dtConfig.Rows(d)("url") & " - Hitung ulang di mulai dan Cek Periode Akuntansi")
                                f_splitWs(dtConfig.Rows(d)("url"), "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                            End If
                        Next
                    End If
                    If jamHItungUlang <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") = False Then
                                dtConfig.Rows(d)("HitungUlang") = True
                            End If
                        Next
                    End If

                End If
                
            End If

        Catch ex As Exception
            log("Error Run Time User : " + Err.Description)
        End Try
    End Sub

    Private Sub OnTimedEventUser(ByVal source As Object, ByVal e As ElapsedEventArgs)
        Try
            If (dtUser.Rows.Count > 0) Then
                For i = dtUser.Rows.Count - 1 To 0 Step -1
                    dtUser.Rows(i)("ulaktif") += 1
                    If dtUser.Rows(i)("ulid") = "del" Then
                        dtUser.Rows.RemoveAt(i)
                    ElseIf dtUser.Rows(i)("ulaktif") >= CountLogout Then
                        If dtUser.Rows(i)("ulurl") = "" Then
                            log("URL tidak ada untuk lokasi : " + dtUser.Rows(i)("lokasi"))
                        Else
                            f_splitWs(dtUser.Rows(i)("ulurl"), dtUser.Rows(i)("ulid") + "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" + dtUser.Rows(i)("uluser").ToString + "★1★" + dtUser.Rows(i)("ullokasi"))
                            dtUser.Rows.RemoveAt(i)
                        End If

                    End If
                Next
            End If

            If waktuHitungUlang <> "" Then

                If tglHItungUlang <> 0 Then

                    If tglHItungUlang = Date.Now().Day And jamHItungUlang = Now.Hour And menitHitungUlang <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") Then
                                dtConfig.Rows(d)("HitungUlang") = False
                                log(dtConfig.Rows(d)("url") & " - Hitung ulang di mulai dan Cek Periode Akuntansi - " & "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                                f_splitWs(dtConfig.Rows(d)("url"), "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                            End If
                        Next
                    End If
                    If tglHItungUlang <> Date.Now().Day And jamHItungUlang <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") = False Then
                                dtConfig.Rows(d)("HitungUlang") = True
                            End If
                        Next
                    End If

                ElseIf Len(hariHitungUlang) <> 0 Then

                    If hariHitungUlang.ToLower = WeekdayName(Weekday(Date.Now())).ToLower And jamHItungUlang = Now.Hour And menitHitungUlang <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") Then
                                dtConfig.Rows(d)("HitungUlang") = False
                                log(dtConfig.Rows(d)("url") & " - Hitung ulang di mulai dan Cek Periode Akuntansi - " & "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                                f_splitWs(dtConfig.Rows(d)("url"), "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                            End If
                        Next
                    End If
                    If hariHitungUlang.ToLower <> WeekdayName(Weekday(Date.Now())).ToLower And jamHItungUlang <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") = False Then
                                dtConfig.Rows(d)("HitungUlang") = True
                            End If
                        Next
                    End If

                Else

                    If jamHItungUlang = Now.Hour And menitHitungUlang <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") Then
                                dtConfig.Rows(d)("HitungUlang") = False
                                log(dtConfig.Rows(d)("url") & " - Hitung ulang di mulai dan Cek Periode Akuntansi - " & "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                                f_splitWs(dtConfig.Rows(d)("url"), "1★M2_Accounting_PeriodSearch★0△0△aptutupperiode = 0△aptahun, apbulan△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★")
                            End If
                        Next
                    End If
                    If jamHItungUlang <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("HitungUlang") = False Then
                                dtConfig.Rows(d)("HitungUlang") = True
                            End If
                        Next
                    End If

                End If

            End If

            If waktuReminder <> "" Then

                If tglReminder <> 0 Then

                    If tglReminder = Date.Now().Day And jamReminder = Now.Hour And menitReminder <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("Reminder") Then
                                dtConfig.Rows(d)("Reminder") = False
                                log(dtConfig.Rows(d)("url") & " - Email Reminder Process")
                                f_emailReminder(dtConfig.Rows(d)("url"), dtConfig.Rows(d)("koneksi"))
                            End If
                        Next
                    End If
                    If tglReminder <> Date.Now().Day And jamReminder <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("Reminder") = False Then
                                dtConfig.Rows(d)("Reminder") = True
                            End If
                        Next
                    End If

                ElseIf Len(hariReminder) <> 0 Then

                    If hariReminder.ToLower = WeekdayName(Weekday(Date.Now())).ToLower And jamReminder = Now.Hour And menitReminder <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("Reminder") Then
                                dtConfig.Rows(d)("Reminder") = False
                                log(dtConfig.Rows(d)("url") & " - Email Reminder Process")
                                f_emailReminder(dtConfig.Rows(d)("url"), dtConfig.Rows(d)("koneksi"))
                            End If
                        Next
                    End If
                    If hariReminder.ToLower <> WeekdayName(Weekday(Date.Now())).ToLower And jamReminder <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("Reminder") = False Then
                                dtConfig.Rows(d)("Reminder") = True
                            End If
                        Next
                    End If

                Else

                    If jamReminder = Now.Hour And menitReminder <= Now.Minute Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("Reminder") Then
                                dtConfig.Rows(d)("Reminder") = False
                                log(dtConfig.Rows(d)("url") & " - Email Reminder Process")
                                f_emailReminder(dtConfig.Rows(d)("url"), dtConfig.Rows(d)("koneksi"))
                            End If
                        Next
                    End If
                    If jamReminder <> Now.Hour Then
                        For d = 0 To dtConfig.Rows.Count - 1
                            If dtConfig.Rows(d)("Reminder") = False Then
                                dtConfig.Rows(d)("Reminder") = True
                            End If
                        Next
                    End If

                End If

            End If

        Catch ex As Exception
            log("Error Run Time User : " + Err.Description)
        End Try
    End Sub

    Private Sub f_emailReminder(ByVal url As String, ByVal strCon As String)
        Try

            ' Perintah kirim email
            log(url & " f_emailReminder : " + "Start process email reminder.")
            Dim sendEmail As Boolean = True, sql As String = ""
            Dim sumber As String = "", ModuleId As Integer = 0, MenuReport As Integer = 0, vUrl As String = ""
            If sendEmail Then
                sql = "SELECT nm.kodetabel, n.moduleid, n.menuid, n.namamenu, n.statusnotifikasi, s.nama as stnama, n.statusupdate, n.userid, n.email, n.email2, n.emailpengirim, n.passwordpengirim, n.useridmail, n.divisi FROM m0_notifikasi_email n JOIN m0_nomor nm ON n.moduleid = nm.moduleid AND n.menuid = nm.menuid LEFT JOIN m0_status s ON n.statusnotifikasi = s.kode ORDER BY n.divisi, n.moduleid, n.menuid, n.statusnotifikasi, n.statusupdate, n.userid"
                Dim dtNotifSet As DataTable = AsDataTableAmbilDariDB(sql, strCon)
                log(url & " f_emailReminder : " + "Get data setting email notification. rows : " & dtNotifSet.Rows.Count)
                If dtNotifSet.Rows.Count > 0 Then

                    Dim dtDashboard As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE sgrup = 'dashboard' AND skode = 'url'", strCon)
                    If dtDashboard.Rows.Count > 0 Then
                        If Len(FxDB(dtDashboard.Rows(0)(0), "")) > 0 Then
                            vUrl = FxDB(dtDashboard.Rows(0)(0), "")
                        Else
                            log(url & " Error f_emailReminder : " + "Dashboard url is empty in Setting Data.") : GoTo selesai
                        End If
                    Else
                        log(url & " Error f_emailReminder : " + "Dashboard url not found in Setting Data.") : GoTo selesai
                    End If

                    Dim dtTrans As New DataTable
                    Dim vStUpdBefore As Double = 0, vUseridBefore As Double = 0

                    For Each drns As DataRow In dtNotifSet.Rows
                        sumber = drns("kodetabel") : ModuleId = drns("moduleid") : MenuReport = drns("menuid")

                        If ModuleId = 4 And MenuReport = 3 Then 'PR
                            sql = "  SELECT prid as idtrans, prnotransaksi as notrans, prtgl as tgltrans, DATEDIFF(NOW(),prtgl) as tglselisih, prmatauang as matauang, prkurs as kurs, prtotaltransaksi as totaltransaksi, prkurs * prtotaltransaksi as totalfungsional "
                            sql &= " FROM m4_pr "
                            'sql &= " JOIN m4_pr_detail ON prid = idpr JOIN m1_item ON idbarang = bid "
                            sql &= " WHERE prstatus = " & drns("statusnotifikasi") & " "
                            sql &= " AND prdimintaoleh = " & drns("userid") & " "
                            sql &= " AND DATEDIFF(NOW(),prtgl) >= " & intervalReminder & " "
                            sql &= " AND prlokasi = " & drns("divisi") & " "
                            'sql &= " GROUP BY prtgl, prnotransaksi, prinputtgl, prid "
                            sql &= " ORDER BY prtgl, prnotransaksi, prinputtgl, prid "
                            dtTrans = AsDataTableAmbilDariDB(sql, strCon)
                            log(url & " f_emailReminder : " + "Get data trans email notification " & sumber & " -- " & drns("namamenu") & " - stnotif " & drns("statusnotifikasi") & " - stupdate " & drns("statusupdate") & " - userid " & drns("userid") & " rows : " & dtTrans.Rows.Count)

                        ElseIf ModuleId = 4 And MenuReport = 7 Then 'PO

                            If vStUpdBefore <> drns("statusupdate") Then
                                vUseridBefore = 0
                            End If

                            sql = "  SELECT poid as idtrans, ponotransaksi as notrans, potgl as tgltrans, DATEDIFF(NOW(),potgl) as tglselisih, pomatauang as matauang, pokurs as kurs, pototaltransaksi as totaltransaksi, pokurs * pototaltransaksi as totalfungsional "
                            sql &= " FROM m4_po "
                            'sql &= " JOIN m4_po_detail ON poid = idpo JOIN m1_item ON idbarang = bid "
                            sql &= " WHERE postatus = " & drns("statusnotifikasi") & " "
                            sql &= " AND pojenispembeliankategori = " & drns("statusupdate") & " "
                            sql &= " AND DATEDIFF(NOW(),potgl) >= " & intervalReminder & " "
                            sql &= " AND pototaltransaksi * pokurs <= " & drns("userid") & " "
                            If vUseridBefore <> 0 Then
                                sql &= " AND pototaltransaksi * pokurs > " & vUseridBefore & " "
                            End If
                            sql &= " AND polokasi = " & drns("divisi") & " "
                            'sql &= " GROUP BY potgl, ponotransaksi, poinputtgl, poid "
                            sql &= " ORDER BY potgl, ponotransaksi, poinputtgl, poid "
                            dtTrans = AsDataTableAmbilDariDB(sql, strCon)
                            log(url & " f_emailReminder : " + "Get data trans email notification " & sumber & " -- " & drns("namamenu") & " - stnotif " & drns("statusnotifikasi") & " - stupdate " & drns("statusupdate") & " - userid " & drns("userid") & " rows : " & dtTrans.Rows.Count)

                            vStUpdBefore = drns("statusupdate")
                            vUseridBefore = drns("userid")
                        End If

                        If dtTrans.Rows.Count > 0 Then

                            Dim fromAddress = New MailAddress(drns("emailpengirim").ToString, "Reminder " & sumber)
                            Dim toAddress = New MailAddress(drns("email").ToString)
                            Dim fromPassword As String = drns("passwordpengirim").ToString
                            Dim subject As String = "MyERPPlus - Reminder " & sumber & " " & drns("stnama") & " - " & DateTime.Now
                            Dim body As String = "", vUrut As Double = 0
                            Dim strValue As New StringBuilder

                            Dim smtp = New SmtpClient()
                            smtp.Host = "smtp.gmail.com"
                            smtp.Port = 587
                            smtp.EnableSsl = True
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network
                            'smtp.UseDefaultCredentials = True
                            smtp.UseDefaultCredentials = False
                            smtp.Credentials = New System.Net.NetworkCredential(fromAddress.Address, fromPassword)

                            Using message = New MailMessage(fromAddress, toAddress)
                                If Len(drns("email2").ToString) > 0 Then
                                    message.CC.Add(New MailAddress(drns("email2").ToString))
                                End If

                                message.Subject = subject
                                message.IsBodyHtml = True
                                body = "Reminder regarding the pending approval for the transaction number mentioned below."
                                body &= "<br>"
                                body &= "Kindly process the approval at your earliest convenience by clicking the link provided."
                                body &= "<br><br>"
                                body &= drns("namamenu")
                                body &= "<br><br>"
                                body &= "<table border=1 cellpadding=5>"
                                body &= "<tr>"
                                body &= "<th>No </th>"
                                body &= "<th> Trans No </th>"
                                body &= "<th>Date</th>"
                                body &= "<th>Curr</th>"
                                body &= "<th>Total</th>"
                                body &= "</tr>"


                                For Each drt As DataRow In dtTrans.Rows
                                    vUrut += 1

                                    body &= "<tr>"
                                    body &= "<td>" & vUrut & "</td>"
                                    body &= "<td><a href='" & vUrl & "/app/autologin.aspx?iduser=" & drns("useridmail") & "&sumber=" & sumber & "&idtransaksi=" & drt("idtrans") & "'>" & drt("notrans") & "</a></td>"
                                    body &= "<td>" & drt("tgltrans") & "</td>"
                                    body &= "<td>" & drt("matauang") & "</td>"
                                    body &= "<td style='text-align:right;'>" & FormatNumber(drt("totaltransaksi")) & "</td>"
                                    body &= "</tr>"

                                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                                    strValue.Append("(0, '" & vUrl & "', 'm" & ModuleId.ToString + "_" + sumber & "', '" & sumber & "', " & drt("idtrans") & ", " & drns("useridmail") & ", 'reminder', 0, NOW(), NOW())")

                                Next

                                body &= "</table>"

                                'INSERT LOG MAIL
                                Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
                                Dim myConn As New MySql.Data.MySqlClient.MySqlConnection(strCon)
                                myConn.Open()
                                '*** Start Transaction ***'  
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .CommandType = CommandType.Text
                                    .CommandText = "Insert into m0_dashboardmail(did, durl, dtable, dsumber, didtransaksi, diduser, dketerangan, dview, dinputtgl, dmodiftgl) values " & strValue.ToString & ""
                                End With
                                objCmd.ExecuteNonQuery()


                                message.Body = body

                                smtp.Send(message)
                            End Using

                        End If
                    Next
                End If

            End If

            log(url & " f_emailReminder : " + "Finish process email reminder.")
selesai:

        Catch ex As Exception
            log(url & " Error f_emailReminder : " + Err.Description)

        End Try

    End Sub

    Public Function FxDB(ByVal Param As Object, ByVal DefaultVal As Object) As String
        If IsDBNull(Param) Then
            Return DefaultVal
        Else
            ''Cek jika Formattgl=true , maka format tgl
            'If FormatTgl Then
            '    Dim formattgl1 As String = "dd/MM/yyyy"
            '    Dim formattgl2 As String = "dd/MM/yyyy hh:mm:ss"

            '    'Jika tipe date, maka format tanggal
            '    If (IsDate(Param)) Then
            '        'Jika panjang param>10 maka ksh formattgl2
            '        Param = AsFormatTanggal(Param, formattgl1)
            '    End If
            'End If

            Return Param
        End If
    End Function

End Class