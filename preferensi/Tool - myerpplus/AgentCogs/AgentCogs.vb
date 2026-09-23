Imports System.IO
Imports Stimulsoft.Report
Imports Stimulsoft.Report.Dictionary
Imports System.Net
Imports System.Net.Sockets

Public Class AgentCogs
    Dim Client As TcpClient, ErrMessage, sptField As String, ReportConfig As String
    Dim MyTimer As New System.Timers.Timer(), aku As Object

    Sub f_load() Handles Me.Load
        Try
            aku = Me

            'set standart spt
            sptField = "▼"

            'set tcpclient
            Client = New TcpClient("127.0.0.1", 422)
            Client.GetStream.BeginRead(New Byte() {0}, 0, 0, New AsyncCallback(AddressOf Read), Nothing)

            'lokasi report.config
            ReportConfig = getlokasi + "\app.config"

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
                            End Select
                    End Select
                End While
            End Using

            'setting timer close paksa, di setting 5 menit
            AddHandler MyTimer.Elapsed, AddressOf OnTimedEvent
            MyTimer.Interval = 120 * 60 * 1000
            MyTimer.Enabled = True
            MyTimer.Start()
        Catch ex As Exception
            SimpanLogToFile("error f_load : " + Err.Description)
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
        Try
            Dim datasangar As String = New StreamReader(Client.GetStream).ReadLine

            Dim a() As String = datasangar.Split(sptField)

            '// FORMAT MSMQ JURNAL DAN HPP = tipe(J=Jurnal, C=Hpp), idmsmq, sumber, idtransaksi, userid --> dipisah dengan sptField ▼
            '// a = tipe(0), idmsmq(1), sumber(2), idtransaksi(3), userid(4), strConn(5), LocationResultReport(6)
            Dim tipe As String = a(0), idmsmq As String = a(1), sumber As String = a(2), idtransaksi As String = a(3), userid As Integer = a(4)

            'If sumber = "SUMBER" Then
            '    Dim getHitungUlang As New HitungUlang
            '    getHitungUlang.M0_CogsHitungUlang_Fifo()
            '    GoTo selesai
            'End If

            Dim dt As New DataTable
            Dim sql As String = "", idmsmqFailed As String = "", sumberFailed As String = "", idtransaksiFailed As String = "", useridFailed As String = ""

cekMsmqSebelumnya:  '//CEK MSMQ SEBELUMNYA, JIKA ADA YG GAGAL MAKA PROSES YG GAGAL TERLEBIH DAHULU
            sql = "SELECT mc2.mcid, mc2.mcsumber, mc2.mcidtransaksi, mc2.mctglantrian, mc2.mcuserid FROM m0_msmq_cogs mc1 LEFT JOIN m0_msmq_cogs mc2 ON mc1.mctglantrian > mc2.mctglantrian WHERE mc1.mcid = '" & FixQuotes(idmsmq) & "' AND mc2.mcprogress IN(0,1,3) ORDER BY mc2.mctglantrian LIMIT 1"
            dt = AsDataTableAmbilDariDB(sql, strCon)
            If dt.Rows.Count > 0 Then

                idmsmqFailed = dt.Rows(0)("mcid").ToString
                sumberFailed = dt.Rows(0)("mcsumber").ToString
                idtransaksiFailed = dt.Rows(0)("mcidtransaksi").ToString
                useridFailed = dt.Rows(0)("mcuserid").ToString

                If Len(idmsmqFailed) > 0 Then
                    '//PROSES MSMQ YANG GAGAL
                    Log(" - Proses COGS sebelumnya, msmq :" + idmsmqFailed + ", sumber : " + sumberFailed + ", idtransaksi : " + idtransaksiFailed)

                    If sumberFailed <> sumber Or (sumberFailed = sumber And idtransaksiFailed <> idtransaksi) Then
                        If F_Cogs(idmsmqFailed, sumberFailed, idtransaksiFailed, useridFailed) = False Then
                            GoTo selesai
                        End If
                    Else
                        sql = "UPDATE m0_msmq_cogs SET mcprogress = '4', mcpesan = '', mctglselesai = NOW() WHERE mcid = '" & idmsmqFailed & "'"
                        If AsEksekusiSQL(sql, strCon) = False Then
                            Log(" -- Failed progress COGS, msmq :" + idmsmqFailed + ", sumber : " + sumberFailed + ", idtransaksi : " + idtransaksiFailed)
                            GoTo selesai
                        End If
                    End If

                    GoTo cekMsmqSebelumnya
                End If
            End If

            '// PANGGIL PROSES HPP
            If F_Cogs(idmsmq, sumber, idtransaksi, userid) = False Then
                GoTo selesai
            End If

            '            aku.Close()
            '            Client.GetStream.BeginRead(New Byte() {0}, 0, 0, AddressOf Read, Nothing)
            '            Exit Sub
            '        Catch ex As Exception
            '            SimpanLogToFile("[Agent] Error : " + Err.Description + ",param : " + New StreamReader(Client.GetStream).ReadLine + " Read()")
            '        End Try
            'selesai:


        Catch ex As Exception
            'SimpanLogToFile("[Agent] Error : " + Err.Description + ",param : " + New StreamReader(Client.GetStream).ReadLine + " Read()")
            SimpanLogToFile("[Agent] Error : " + Err.Description + " Read()")
        End Try

selesai:
        'Log("aku.Close()")
        aku.Close()
        'Client.GetStream.BeginRead(New Byte() {0}, 0, 0, AddressOf Read, Nothing)
        Exit Sub
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

    '//PROSES HPP TRANSAKSI
    Public Function F_Cogs(ByVal idmsmq As String, ByVal sumber As String, ByVal idtransaksi As String, ByVal userid As Integer) As Boolean

        Try
            '// FORMAT MSMQ JURNAL DAN HPP = tipe(J=Jurnal, C=Hpp), idmsmq, sumber, idtransaksi, userid --> dipisah dengan sptField ▼
            '// a = tipe(0), idmsmq(1), sumber(2), idtransaksi(3), userid(4)

            ''// KIRIM INFORMASI SEDANG PROSES ((J) Jurnal OR (C) HPP, Status, Id Generate, keterangan)
            'Send("C" & sptField & "Process" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". ")

            '// PROSES HPP
            Dim clsCogs As New Cogs
            '// mapping fungsi yg dikirim = idmsmq, sumber, idtransaksi
            Dim strCogs As String = clsCogs.M0_Cogs("WebsiteAccessKey★M0_Cogs★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★" & idmsmq & "△" & sumber & "△" & idtransaksi & "")

            '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
            '// AMBIL KEMBALIAN FUNGSI JURNAL
            Dim rsCogs() As String = strCogs.Split(sptParam)
            '// JIKA KEMBALIAN FUNGSI HPP <> 3 MAKA SALAH
            If rsCogs.Length = 3 Then
                '// AMBIL BAGIAN RESULT DARI FUNGSI HPP
                '// result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
                Dim rsResult() As String = rsCogs(0).Split(sptSubParam)
                '// BAGIAN RESULT DARI FUNGSI HPP <> 5 MAKA SALAH
                If rsResult.Length = 5 Then
                    '// KIRIM INFORMASI PROSES BERDSARKAN RESULT SUCCESS
                    If rsResult(1) <> 1 And rsResult(1) <> 4 Then '// JIKA GAGAL
                        '// KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
                        SimpanLogToFile("C" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". " & rsResult(2) & "")
                        Return False
                    End If
                Else
                    '// KIRIM INFORMASI PROSES SALAH
                    SimpanLogToFile("C" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". Invalid result data #2'")
                    Return False
                End If

            Else
                '// KIRIM INFORMASI PROSES SALAH
                SimpanLogToFile("C" & sptField & "Failed" & sptField & idmsmq & sptField & sumber & " : " & idtransaksi & ". Invalid result data #1'")
                Return False
            End If

        Catch ex As Exception
            SimpanLogToFile("[Agent] Error F_Cogs : IDMSMQ : " + idmsmq + " Desc : " + Err.Description)
            Return False
        End Try

        Return True
    End Function

End Class