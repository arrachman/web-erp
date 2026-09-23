Imports System.IO
Imports Stimulsoft.Report
Imports Stimulsoft.Report.Dictionary
Imports System.Net
Imports System.Net.Sockets

Public Class Agent
    Dim Client As TcpClient, ErrMessage, sptField As String, first As Boolean = True, ReportConfig As String
    Dim MyTimer As New System.Timers.Timer(), aku As Object

    Sub f_load() Handles Me.Load
        Try
            'SimpanLogToFile(Directory.GetDirectories(StartPath))
            aku = Me

            'set standart spt
            sptField = "▼"

            'lokasi report.config
            ReportConfig = getlokasi + "\app.config"
            LocationResultReport = getlokasi.Replace("\config", "") + "\temp\"
            LocationMRT = getlokasi.Replace("\config", "") + "\mrt\"
            LokasiError = getlokasi + "\Log_Error_Agent.txt"

            'Cek report.config
            If File.Exists(ReportConfig) = False Then
                SimpanLogToFile("[Agent] Error StartConfig : File agent.config Not Found")
                aku.Close()
            End If

            'download report.config
            config = File.ReadAllText(ReportConfig)
            'config = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(config)).Replace("UUhKeU5HTm9iV0Z1TURnd01qa3p3aXphcmQ5Mw==", "")))
            config = Encoding.UTF8.GetString(Convert.FromBase64String(config))

            'Set Koneksi, lokasi hasil report dan lokasi file mrt dari report.config
            Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                While reader.Read()
                    Select Case reader.NodeType
                        Case XmlNodeType.Element
                            Select Case reader.Name
                                Case "ConStr" : strCon = reader.ReadElementContentAsString()
                                Case "ReportConStr" : strConStieReport = reader.ReadElementContentAsString()
                            End Select
                    End Select
                End While
            End Using

            'setting timer close paksa, di setting 20 menit
            AddHandler MyTimer.Elapsed, AddressOf OnTimedEvent
            MyTimer.Interval = 120 * 60 * 1000
            MyTimer.Enabled = True
            MyTimer.Start()

            'set tcpclient
            Client = New TcpClient("127.0.0.1", 421)
            Client.GetStream.BeginRead(New Byte() {0}, 0, 0, New AsyncCallback(AddressOf Read), Nothing)

        Catch ex As Exception
            SimpanLogToFile("Error Me.Load : " + Err.Description)
        End Try
    End Sub

    Private Sub OnTimedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
        Try
            SimpanLogToFile("Tutup paksa 120 menit, dataProses : " + dataProses)
            MyTimer.stop()

            aku.Close()
            SimpanLogToFile("aku.Close() sudah di lewati")
        Catch ex As Exception
            SimpanLogToFile("Error OnTimedEvent : " + Err.Description)
        End Try
    End Sub

    Sub Read(ByVal ar As IAsyncResult)
        Dim a() As String, data As String

        If first = True Then

            Try

                first = False
                dataProses = New StreamReader(Client.GetStream).ReadLine
                a = dataProses.Split(sptField)


                If a(0) = "J" Or a(0) = "C" Then
                    'SimpanLogToFile("jurnal : ")
                    '// FORMAT MSMQ JURNAL DAN HPP = tipe(J=Jurnal, C=Hpp), idmsmq, sumber, idtransaksi, userid --> dipisah dengan sptField ▼
                    '// a = tipe(0), idmsmq(1), sumber(2), idtransaksi(3), userid(4), strConn(5), LocationResultReport(6)
                    Dim tipe As String = a(0), idmsmq As String = a(1), sumber As String = a(2), idtransaksi As String = a(3), userid As Integer = a(4)

                    '// JIKA tipe = J maka proses jurnal, JIKA tipe = C maka proses hpp
                    If tipe = "J" Then
                        'SimpanLogToFile("jurnal1 : ")
                        Dim dt As New DataTable
                        Dim sql As String = "", idmsmqFailed As String = "", sumberFailed As String = "", idtransaksiFailed As String = "", useridFailed As String = ""

cekMsmqSebelumnya:      '//CEK MSMQ SEBELUMNYA, JIKA ADA YG GAGAL MAKA PROSES YG GAGAL TERLEBIH DAHULU
                        'sql = "SELECT mj2.mjid, mj2.mjsumber, mj2.mjidtransaksi, mj2.mjtglantrian, mj2.mjuserid FROM m0_msmq_journal mj1 LEFT JOIN m0_msmq_journal mj2 ON mj1.mjtglantrian > mj2.mjtglantrian WHERE mj1.mjid = '" & FixQuotes(idmsmq) & "' AND mj2.mjprogress IN(0,3) ORDER BY mj2.mjtglantrian LIMIT 1"
                        sql = "SELECT mj2.mjid, mj2.mjsumber, mj2.mjidtransaksi, mj2.mjtglantrian, mj2.mjuserid FROM m0_msmq_journal mj1 LEFT JOIN m0_msmq_journal mj2 ON mj1.mjtglantrian > mj2.mjtglantrian WHERE mj1.mjid = '" & FixQuotes(idmsmq) & "' AND mj2.mjprogress IN(0,1,3) ORDER BY mj2.mjtglantrian LIMIT 1"
                        dt = AsDataTableAmbilDariDB(sql, strCon)
                        If dt.Rows.Count > 0 Then

                            idmsmqFailed = dt.Rows(0)("mjid").ToString
                            sumberFailed = dt.Rows(0)("mjsumber").ToString
                            idtransaksiFailed = dt.Rows(0)("mjidtransaksi").ToString
                            useridFailed = dt.Rows(0)("mjuserid").ToString

                            If Len(idmsmqFailed) > 0 Then
                                If sumberFailed <> sumber Or (sumberFailed = sumber And idtransaksiFailed <> idtransaksi) Then
                                    '//PROSES MSMQ YANG GAGAL
                                    'sql = "UPDATE m0_msmq_journal SET mjprogress = '1', mjpesan = 'on process' WHERE mjid = '" & idmsmqFailed & "'"
                                    'If AsEksekusiSQL(sql, strCon) = False Then
                                    '    SimpanLogToFile(" -- Failed progress Journal, msmq :" + idmsmqFailed + ", sumber : " + sumberFailed + ", idtransaksi : " + idtransaksiFailed)
                                    '    GoTo selesai
                                    'End If
                                    If F_Jurnal(idmsmqFailed, sumberFailed, idtransaksiFailed, useridFailed) = False Then
                                        GoTo selesai
                                    End If
                                Else
                                    sql = "UPDATE m0_msmq_journal SET mjprogress = '4', mjpesan = '', mjtglselesai = NOW() WHERE mjid = '" & idmsmqFailed & "'"
                                    If AsEksekusiSQL(sql, strCon) = False Then
                                        SimpanLogToFile(" -- Failed progress Journal, msmq :" + idmsmqFailed + ", sumber : " + sumberFailed + ", idtransaksi : " + idtransaksiFailed)
                                        GoTo selesai
                                    End If
                                End If

                                GoTo cekMsmqSebelumnya
                            End If

                        End If

                        If F_Jurnal(idmsmq, sumber, idtransaksi, userid) = False Then
                            GoTo selesai
                        End If

                        'ElseIf tipe = "C" Then
                        '    '// PANGGIL PROSES HPP
                        '    F_Cogs(idmsmq, sumber, idtransaksi, userid)
                        'SimpanLogToFile("jurnal2 : ")
                    End If

                ElseIf a(0) = "JurnalUlang" Then
                    F_JurnalUlang(dataProses)
                Else
                    'SimpanLogToFile("report : ")
                    Dim clsReport As New Report
                    clsReport.generator(a)
                End If

            Catch ex As Exception
                SimpanLogToFile("[Agent] Error : " + Err.Description + " Read()")
            End Try

selesai:
            'tutup agent di akhir proses
            aku.Close()
            'Client.GetStream.BeginRead(New Byte() {0}, 0, 0, AddressOf Read, Nothing)
            'Catch ex As Exception
            'MsgBox("Cannot read from report manager : " + ex.Message)
            'End Try
            Exit Sub

        End If

    End Sub

    Public Sub Send(ByVal Str As String)
        Try
            If Str.Split(sptField)(1) = "Failed" Then
                SimpanLogToFile(Str)
            End If
            If Str.Split(sptField)(1) = "Done" Or Str.Split(sptField)(1) = "Failed" Then
                aku.Close()
            End If
        Catch ex As Exception
            SimpanLogToFile("Cannot send to report manager : " + ex.Message)
        End Try
    End Sub

    '//PROSES JURNAL TRANSAKSI
    Public Function F_Jurnal(ByVal idmsmq As String, ByVal sumber As String, ByVal idtransaksi As String, ByVal userid As Integer) As Boolean

        Try
            '// FORMAT MSMQ JURNAL DAN HPP = tipe(J=Jurnal, C=Hpp), idmsmq, sumber, idtransaksi, userid --> dipisah dengan sptField ▼
            '// a = tipe(0), idmsmq(1), sumber(2), idtransaksi(3), userid(4)

            ''// KIRIM INFORMASI SEDANG PROSES ((J) Jurnal OR (C) HPP, Status, Id Generate, keterangan)
            'Send("J" & sptField & "Process" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". ")

            '// PROSES JURNAL
            Dim clsJournal As New Journal
            '// mapping fungsi yg dikirim = idmsmq, sumber, idtransaksi
            Dim strJournal As String = clsJournal.M0_Journal("WebsiteAccessKey★M0_Journal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★" & idmsmq & "△" & sumber & "△" & idtransaksi & "")

            '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
            '// AMBIL KEMBALIAN FUNGSI JURNAL
            Dim rsJournal() As String = strJournal.Split(sptParam)
            '// JIKA KEMBALIAN FUNGSI JURNAL <> 3 MAKA SALAH
            If rsJournal.Length = 3 Then
                '// AMBIL BAGIAN RESULT DARI FUNGSI JURNAL
                '// result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
                Dim rsResult() As String = rsJournal(0).Split(sptSubParam)
                '// BAGIAN RESULT DARI FUNGSI JURNAL <> 5 MAKA SALAH
                If rsResult.Length = 5 Then
                    '// KIRIM INFORMASI PROSES BERDSARKAN RESULT SUCCESS
                    If rsResult(1) <> 1 And rsResult(1) <> 4 Then '// JIKA GAGAL
                        '// KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
                        SimpanLogToFile("J" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". " & rsResult(2) & "")
                        Return False
                    End If
                Else
                    '// KIRIM INFORMASI PROSES SALAH
                    SimpanLogToFile("J" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". Invalid result data #2'")
                    Return False
                End If

            Else
                '// KIRIM INFORMASI PROSES SALAH
                SimpanLogToFile("J" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". Invalid result data #1'")
                Return False
            End If

        Catch ex As Exception
            SimpanLogToFile("[Agent] Error F_Jurnal : IDMSMQ : " + idmsmq + " Desc : " + Err.Description)
            Return False
        End Try

        Return True
    End Function

    ''//PROSES HPP TRANSAKSI
    'Public Sub F_Cogs(ByVal idmsmq As String, ByVal sumber As String, ByVal idtransaksi As String, ByVal userid As Integer)

    '    Try
    '        '// FORMAT MSMQ JURNAL DAN HPP = tipe(J=Jurnal, C=Hpp), idmsmq, sumber, idtransaksi, userid --> dipisah dengan sptField ▼
    '        '// a = tipe(0), idmsmq(1), sumber(2), idtransaksi(3), userid(4)

    '        ''// KIRIM INFORMASI SEDANG PROSES ((J) Jurnal OR (C) HPP, Status, Id Generate, keterangan)
    '        'Send("C" & sptField & "Process" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". ")

    '        '// PROSES HPP
    '        Dim clsCogs As New Cogs
    '        '// mapping fungsi yg dikirim = idmsmq, sumber, idtransaksi
    '        Dim strCogs As String = clsCogs.M0_Cogs("WebsiteAccessKey★M0_Cogs★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★" & idmsmq & "△" & sumber & "△" & idtransaksi & "")

    '        '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
    '        '// AMBIL KEMBALIAN FUNGSI JURNAL
    '        Dim rsCogs() As String = strCogs.Split(sptParam)
    '        '// JIKA KEMBALIAN FUNGSI HPP <> 3 MAKA SALAH
    '        If rsCogs.Length = 3 Then
    '            '// AMBIL BAGIAN RESULT DARI FUNGSI HPP
    '            '// result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
    '            Dim rsResult() As String = rsCogs(0).Split(sptSubParam)
    '            '// BAGIAN RESULT DARI FUNGSI HPP <> 5 MAKA SALAH
    '            If rsResult.Length = 5 Then
    '                '// KIRIM INFORMASI PROSES BERDSARKAN RESULT SUCCESS
    '                If rsResult(1) <> 1 Then '// JIKA GAGAL
    '                    '// KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
    '                    SimpanLogToFile("C" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". " & rsResult(2) & "")
    '                End If
    '            Else
    '                '// KIRIM INFORMASI PROSES SALAH
    '                SimpanLogToFile("C" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". Invalid result data #2'")
    '            End If

    '        Else
    '            '// KIRIM INFORMASI PROSES SALAH
    '            SimpanLogToFile("C" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". Invalid result data #1'")
    '        End If

    '    Catch ex As Exception
    '        SimpanLogToFile("[Agent] Error F_Cogs : IDMSMQ : " + idmsmq + " Desc : " + Err.Description)
    '    End Try

    'End Sub

    'Private Sub f_load(sender As Object, e As EventArgs) Handles MyBase.Load

    'End Sub

    Public Sub F_JurnalUlang(ByVal data As String)
        Dim clsJournal As New JournalUlangOptimasi
        clsJournal.M0_JournalUlangOptimasi(data)
    End Sub
End Class