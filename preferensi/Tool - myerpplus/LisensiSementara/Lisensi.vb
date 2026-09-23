Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Messaging
Imports System.Xml
Imports System.Timers
Imports MySql.Data.MySqlClient

Public Class ToolsManager
    Dim xStep As Integer = 0

    Public dr As DataRow
    Public dtlist As New DataTable

    Dim queue As New MessageQueue
    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public sptLogin As String = "Θ"
    Public go As Boolean = True

    Dim tmr As System.Windows.Forms.Timer = New System.Windows.Forms.Timer
    Dim StrCon, app As String
    Dim intervalCheck, CountLogout As Integer
    Dim url, config, PathQueue As String
    Dim AppConfig As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\LisensiSementara.config"
    Dim LokasiError As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\LogLisensi.txt"

    Protected Overrides Sub OnStart(ByVal args() As String)
        If File.Exists(AppConfig) = False Then
            SimpanLogToFile("Error StartConfig : File LisensiSementara.config Not Found")
            Return
        End If
        Dim MyTimer As New System.Timers.Timer()
        AddHandler MyTimer.Elapsed, AddressOf OnTimedEvent

        StartConfig()
        MyTimer.Interval = intervalCheck * 1000
        MyTimer.Enabled = True
        MyTimer.Start()
        ListenQueue()

    End Sub

    Private Sub OnTimedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
        Try
            If (dtlist.Rows.Count > 0) Then
                For i = dtlist.Rows.Count - 1 To 0 Step -1
                    dtlist.Rows(i)("ulaktif") += 1
                    If dtlist.Rows(i)("ulid") = "del" Then
                        dtlist.Rows.RemoveAt(i)
                    ElseIf dtlist.Rows(i)("ulaktif") = CountLogout Then
                        f_splitWs(dtlist.Rows(i)("ulid") + "★M0_Logout★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" + dtlist.Rows(i)("uluser").ToString + "★1★" + app)
                        dtlist.Rows.RemoveAt(i)
                    End If
                Next
            End If
        Catch ex As Exception
            SimpanLogToFile("Error Run Time : " + Err.Description)
        End Try
    End Sub

    Public Sub SimpanLogToFile(ByVal StrVal As String)
        If Len(StrVal) > 0 Then
            Try
                If File.Exists(LokasiError) = False Then
                    File.Create(LokasiError).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(LokasiError, True)
                With streamWriter
                    .Write(Now & " : " & StrVal & vbCrLf)
                    .Flush()
                    .Dispose()
                    .Close()
                End With
            Catch ex As Exception
            End Try
        End If
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
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

            AddHandler queue.ReceiveCompleted, AddressOf MyReceiveCompleted

            queue.BeginReceive()
        Catch ex As Exception
            SimpanLogToFile("Error ListenQueue : " & Err.Description)
            queue.BeginReceive()
        End Try
        Return
    End Sub

    Sub MyReceiveCompleted(ByVal [source] As [Object], ByVal asyncResult As ReceiveCompletedEventArgs)
        Dim mq As MessageQueue = CType([source], MessageQueue)
        Dim m As Message = mq.EndReceive(asyncResult.AsyncResult)
        Dim s, type, idlogin, userid As String

        Try
            s = m.Body
            type = s.Split(sptField)(0)
            idlogin = s.Split(sptField)(1)
            userid = s.Split(sptField)(2)
            If type = "login" Then
                dr = dtlist.NewRow()
                dr(0) = idlogin
                dr(1) = userid
                dr(2) = 1
                dtlist.Rows.Add(dr)
            ElseIf type = "check" Then
                For i = 0 To dtlist.Rows.Count - 1
                    If dtlist.Rows(i)("ulid") = idlogin Then
                        dtlist.Rows(i)("ulaktif") = 0 : GoTo selesai
                    End If
                Next
            ElseIf type = "logout" Then
                For i = 0 To dtlist.Rows.Count - 1
                    If dtlist.Rows(i)("ulid") = idlogin Then
                        dtlist.Rows(i)("ulid") = "del" : GoTo selesai
                    End If
                Next
            End If


        Catch ex As Exception
            SimpanLogToFile("Error MyReceivedCompleted : " & Err.Description)
        End Try
selesai:
        ' Restart the asynchronous Receive operation.
        queue.BeginReceive()
        Return
    End Sub

    Sub StartConfig()
        Try
            config = File.ReadAllText(AppConfig)
            Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                While reader.Read()
                    Select Case reader.NodeType
                        Case XmlNodeType.Element
                            Select Case reader.Name
                                Case "PathQueueUserLogin" : PathQueue = reader.ReadElementContentAsString()
                                Case "StrCon" : StrCon = reader.ReadElementContentAsString()
                                Case "url" : url = reader.ReadElementContentAsString()
                                Case "app" : app = reader.ReadElementContentAsString()
                                Case "intervalCheck" : intervalCheck = reader.ReadElementContentAsString()
                                Case "CountLogout" : CountLogout = reader.ReadElementContentAsString()
                            End Select
                    End Select
                End While
            End Using
            config = File.ReadAllText(app + "\app\app.xml")
            Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                While reader.Read()
                    Select Case reader.NodeType
                        Case XmlNodeType.Element
                            Select Case reader.Name
                                Case "kodeapp" : PathQueue = ".\PRIVATE$\" + reader.ReadElementContentAsString()
                            End Select
                    End Select
                End While
            End Using
            dtlist = AsDataTableAmbilDariDB("SELECT ulid, uluser, ulaktif FROM m0_userlogin", StrCon)

        Catch ex As Exception
            SimpanLogToFile("Error StartConfig : " & Err.Description)
        End Try
    End Sub

    Private Sub f_splitWs(ByVal param As String)
        'Test Load Web Service
        Try
            Dim req As HttpWebRequest
            Dim res As HttpWebResponse
            Dim webStream As Stream
            req = WebRequest.Create(url + "/ws/myerpplus.asmx/Ws?param=" + param)
            req.Method = "GET"
            res = req.GetResponse()
            webStream = res.GetResponseStream()
        Catch ex As Exception
            SimpanLogToFile("Informasi Split WS : " + Err.Description)
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