Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.IO.Ports

Public Class mintaUser
    Dim clientSocket As TcpClient
    Dim ctThread As Threading.Thread, user_ As String

    Public Sub userMulai(ByVal inClientSocket As TcpClient, ByVal user As String)
        user_ = user
        Me.clientSocket = inClientSocket
        ctThread = New Threading.Thread(AddressOf petugas)

        ctThread.Start()
    End Sub

    Private Sub petugas()
        Dim dataDariUser As String = ""
        Dim infiniteCounter As Integer
        Dim bytesFrom(10024) As Byte
        Dim no_kirim As Integer = -1, last_no_kirim As Integer = -1, jml_data As Integer = 0

        ' Try
        For infiniteCounter = 1 To 2
            infiniteCounter = 1
            jml_data = jml_data + 1
            Dim networkStream As NetworkStream = clientSocket.GetStream()
            If clientSocket.Connected = False Then
                log("<  Data " + user_ + " Disconnect")
                clientSocket.Close()
                Return
            End If

            Try
                networkStream.Read(bytesFrom, 0, CInt(clientSocket.ReceiveBufferSize))
            Catch ex As Exception
                log("<< a - " + Err.Description)
                Return
            End Try

            dataDariUser = System.Text.Encoding.ASCII.GetString(bytesFrom)

            If dataDariUser.Contains("<policy-file-request/>") Then
                mintaKebijakan()
            ElseIf dataDariUser.Contains("$") = False Then
                log("<  Data " + user_ + " NULL")
                clientSocket.Close()
                Return
            ElseIf dataDariUser.Split("$").Length = 1 Then
                log("<  Data " + user_ + " kosong")
                clientSocket.Close()
                Return
            Else
                'dataDariUser di split $ untuk mengahapus NULL sebnyak bytes < 10024 yang tidak terpakai
                dataDariUser = dataDariUser.Substring(0, dataDariUser.IndexOf("$"))
                log("dataDariUser : " + dataDariUser)
                If dataDariUser.Length > 0 Then
                    no_kirim = dataDariUser.Replace("@p1@", sptParam).Split(sptParam)(2)

                    If last_no_kirim = no_kirim Then
                        'data sama berindikasi error, maka di hentikan threadnya
                        log("<< User " + user_ + " Not Connect in MyERP Hardware, last_no_kirim = " + last_no_kirim.ToString)
                        clientSocket.Close()
                        Return
                    Else
                        ''data benar maka di proses

                        'catat no kirim
                        last_no_kirim = no_kirim

                        'log dan list job myhardware
                        log(">> Request From " + user_)
                        dapatTugas(user_, dataDariUser)

                        Try
                            Dim broadcastStream As Net.Sockets.NetworkStream = clientSocket.GetStream()
                            Dim broadcastBytes As [Byte]() = Encoding.ASCII.GetBytes("get_stream")
                            broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length)
                            broadcastStream.Flush()
                        Catch ex As Exception
                            log("<< Broadcast gagal - " + Err.Description)
                        End Try
                    End If
                End If
            End If
        Next
        'Catch ex As Exception
        '    log("<< End - " + user_)
        '    logError(Err.Description)
        '    clientSocket.Close()
        'End Try
    End Sub

    Protected Sub dapatTugas(ByVal user As String, ByVal data As String)
        'namaPerusahaan + param + user + param + pelanggan + param +  detail + param + bayar + param + diskon
        ' Try
        If data.Length > 0 Then
            dr = dtAntri.NewRow()
            dr(0) = Now
            dr(1) = user_
            dr(2) = ""
            dr(3) = data
            dtAntri.Rows.Add(dr)

            action()
        End If
        ' Catch ex As Exception
        'log("Ada error masih di cari tau , harap tenang")
        'logError("action : " + Err.Description)
        'End Try
    End Sub

    Protected Sub action()
        Dim splitData() As String, ngapain As String, data As String, user As String
        'namaPerusahaan + param + user + param + pelanggan + param +  detail + param + bayar + param + diskon
        'Try
        If go And dtAntri.Rows.Count > 0 Then
            go = False
            data = dtAntri.Rows(0)(3).ToString
            user = dtAntri.Rows(0)(1).ToString
            dr = dtPrinted.NewRow
            dr(0) = dtAntri(0)(0)
            dr(1) = dtAntri(0)(1)
            dr(2) = dtAntri(0)(2)
            dtPrinted.Rows.Add(dr)
            dtAntri.Rows.RemoveAt(0)
            splitData = data.Replace("@p1@", sptParam).Split(sptParam)
            ngapain = splitData(0)
            log(ngapain)
            data = splitData(1)

            If ngapain = "poleDisplay" Then
                log("User " + user + ", tampilkan data pole diplay, data : " + data)
                tampilkanDataPoleDisplay(data)
            End If

            If ngapain = "cetak" Then
                log("User " + user + ", lakukan Cetak data : " + data)
                lakukanCetak(data)
            ElseIf ngapain = "cashDrawer" Then
                log("User " + user + ", buka cash drawer data : " + data)
                bukaCashDrawer(data)

            End If

            If dtAntri.Rows.Count > 0 Then
                log("jumlah antrian tersisa = " + dtAntri.Rows.Count.ToString)
                action()
            Else
                go = True
            End If
        End If
    End Sub

    Private Sub mintaKebijakan()
        Dim broadcastStream As Net.Sockets.NetworkStream = clientSocket.GetStream()
        Dim broadcastBytes As [Byte]() = Encoding.ASCII.GetBytes(kebijakanKu)
        broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length)
        broadcastStream.Flush()
    End Sub

    Private Sub lakukanCetak(ByVal data As String)
        Try
            bukaCashDrawer(" ")
            Dim splitData() As String, field() As String, row() As String, strVal As String = ""
            Dim cetak As New ClsCetak
            Dim namaToko As String, user As String, pelanggan As String, diskon As Double, bayar As Double, bayartunai As Double, kredit As Double, debit As Double, totaltransaksi As Double, detail As String
            Dim namaPrint As String, jarak As Integer, kode As String, nama As String, jml As Double, harga As Double, jmldiskon As Double, subtotal As Double
            Dim Alamat1 As String, Alamat2 As String, notransaksi As String, struk As Integer, NoSlip As String, Kupon As Double, SHU As Double, Sukarela As Double
            Dim NoKKredit As String, NoKDebit As String, custom1 As String
            Dim customtext1 As String, customtext2 As String, customtext3 As String, customtext4 As String, customtext5 As String
            Dim customint1 As Integer, customint2 As Integer, customint3 As Integer, customint4 As Integer, customint5 As Integer
            Dim customdbl1 As Double, customdbl2 As Double, customdbl3 As Double, customdbl4 As Double, customdbl5 As Double
            Dim LokasiCetak As String

            'namaPrint, struk, jarak, namaToko, user, pelanggan, detail, bayar, diskon, alamat1, alamat2, notransaksi
            splitData = data.Replace("@p2@", sptParam).Split(sptParam)

            'set variable
            namaPrint = splitData(0)
            struk = splitData(1)
            jarak = splitData(2)
            namaToko = splitData(3)
            user = splitData(4)
            pelanggan = splitData(5)
            detail = splitData(6)
            bayar = Double.Parse(splitData(7))
            diskon = Double.Parse(splitData(8))
            Alamat1 = splitData(9)
            Alamat2 = splitData(10)
            notransaksi = splitData(11)
            kredit = splitData(12)
            debit = splitData(13)
            totaltransaksi = splitData(14)
            NoSlip = splitData(15)
            Kupon = splitData(16)
            SHU = splitData(17)
            Sukarela = splitData(18)
            NoKDebit = splitData(19)
            NoKKredit = splitData(20)
            custom1 = splitData(21)
            bayartunai = splitData(22)
            customtext1 = splitData(23)
            customtext2 = splitData(24)
            customtext3 = splitData(25)
            customtext4 = splitData(26)
            customtext5 = splitData(27)
            customint1 = splitData(28) 'sisa poin
            customint2 = splitData(29)
            customint3 = splitData(30)
            customint4 = splitData(31)
            customint5 = splitData(32)
            customdbl1 = splitData(33)
            customdbl2 = splitData(34)
            customdbl3 = splitData(35)
            customdbl4 = splitData(36)
            customdbl5 = splitData(37)

            'header
            cetak.Header.NamaToko = namaToko
            'cetak.Header.User = user
            'cetak.Header.pelanggan = pelanggan
            'cetak.Header.notransaksi = notransaksi
            cetak.Header.Alamat1 = Alamat1
            cetak.Header.Alamat2 = Alamat2

            'content
            cetak.Content.User = user
            cetak.Content.pelanggan = pelanggan
            cetak.Content.notransaksi = notransaksi
            cetak.Content.NoSlip = NoSlip
            row = detail.Split("~")
            For i = 0 To row.Length - 1
                field = row(i).Split("|")
                kode = field(0)
                'If Len(field(1)) > 16 Then
                '    nama = field(1).Substring(0, 16)
                'Else
                nama = field(1)
                'End If

                jml = Double.Parse(field(2))
                harga = Double.Parse(field(3))

                jmldiskon = Double.Parse(field(4))
                If jmldiskon > 0 Then
                    'If Len(field(1)) > 16 Then
                    '    nama = nama.Substring(0, 15)
                    '    nama &= "*"
                    'Else
                    nama &= "*"
                    'End If


                End If
                subtotal = Double.Parse(field(5))

                cetak.Content.isi(jmldiskon, kode, nama, jml, harga, subtotal)
            Next

            'footer
            cetak.Content.diskon = diskon
            cetak.Content.bayartunai = bayartunai
            cetak.Content.bayar = bayar
            cetak.Content.kredit = kredit
            cetak.Content.debit = debit
            cetak.Content.Kupon = Kupon
            cetak.Content.Shu = SHU
            cetak.Content.Sukarela = Sukarela
            cetak.Content.NoKDebit = NoKDebit
            cetak.Content.NoKKredit = NoKKredit
            cetak.Content.custom1 = custom1
            cetak.Content.customtext1 = customtext1
            cetak.Content.customtext2 = customtext2
            cetak.Content.customtext3 = customtext3
            cetak.Content.customtext4 = customtext4
            cetak.Content.customtext5 = customtext5
            cetak.Content.customint1 = customint1 'sisa poin
            cetak.Content.customint2 = customint2 'jumlah poin
            cetak.Content.customint3 = customint3 'jumlah poin cc
            cetak.Content.customint4 = customint4
            cetak.Content.customint5 = customint5
            cetak.Content.customdbl1 = customdbl1 'bayarpoin
            cetak.Content.customdbl2 = customdbl2 'bayarpoincc
            cetak.Content.customdbl3 = customdbl3
            cetak.Content.customdbl4 = customdbl4
            cetak.Content.customdbl5 = customdbl5
            'cetak.Content.total = totaltransaksi

            'generate cetak
            If struk = 1 Then
                Try
                    strVal = cetak.generate
                Catch ex As Exception
                    log(Err.Description)
                End Try


            ElseIf struk = 2 Then

            ElseIf struk = 3 Then

            ElseIf struk = 4 Then

            ElseIf struk = 5 Then

            ElseIf struk = 6 Then

            ElseIf struk = 7 Then

            ElseIf struk = 8 Then

            ElseIf struk = 9 Then

            ElseIf struk = 10 Then

            End If

            'Minta Cetak
            RawPrinterHelper.SendStringToPrinter(namaPrint, strVal)

            'ndek kene ganok bos sekan
            '# Cutting Paper
            '@#[Global,ESC/POS Compatible]
            '- <29><86><66><0>
            '@#Epson
            '- <70> (Full)
            '- <80> (Partial)
            '- <27><105>
            '- <27><109>
            '- <27><121>
            '- <27><112><0><5><250>
            '- <27><112><0><75><250>
            RawPrinterHelper.SendStringToPrinter(namaPrint, Chr(27) & Chr(105))

            '# Buka Cash Drawer
            '@#Global,ESC/POS Compatible
            '- <27><112><0><64><240> 
            '- <27><112><48><40><40>
            '@#Epson
            '- <27><112><0><48>
            '- <27><112><48><55><121>
            '- <27><112><0><25><250>     
            '- <27><70><0><50><50>     
            '- <27><112><32><25>     
            '- <27><112><0><64><240>     
            '- <27><112>
            'RawPrinterHelper.SendStringToPrinter(namaPrint, Chr(27) & Chr(112) & Chr(0) & Chr(75) & Chr(250))


            For i = 0 To jarak
                'RawPrinterHelper.SendStringToPrinter(namaPrint, vbNewLine)
            Next

            ''Buat file cetak
            LokasiCetak = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\cetak_" + f_Random(4) + "_" + Now.ToString.Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt"
            File.Create(LokasiCetak).Dispose()
            File.WriteAllText(LokasiCetak, strVal)
        Catch ex As Exception
            log(Err.Description)
        End Try


    End Sub

    Public Declare Function OpenUSBcr Lib "usbcr.dll" () As Long
    Public Declare Function DrawerOpen Lib "usbcr.dll" (ByVal ID As Long) As Long
    Public Declare Function CloseUSBcr Lib "usbcr.dll" () As Long

    Private Sub bukaCashDrawer(ByVal data As String)
        OpenUSBcr()
        Try
            DrawerOpen(1)
            DrawerOpen(2)
            DrawerOpen(3)
            DrawerOpen(4)
            DrawerOpen(5)
            DrawerOpen(6)
            DrawerOpen(7)
            DrawerOpen(8)
            DrawerOpen(9)
            DrawerOpen(10)
        Catch ex As Exception

        End Try
        CloseUSBcr()
    End Sub

    Private Sub tampilkanDataPoleDisplay(ByVal data As String)
        Try
            If data.Length > 40 Then
                data = data.Substring(0, 40)
            ElseIf data.Length < 40 Then
                For i = data.Length To 40 - 1
                    data += " "
                Next
            End If
            If poleData = data Then
                Return
            End If
            Dim sp As New SerialPort()
            sp.PortName = "COM7"
            sp.BaudRate = 9600
            sp.Parity = Parity.None
            sp.DataBits = 8
            sp.StopBits = StopBits.One
            Try
                sp.Open()
            Catch ex As Exception
                log(Err.Description)
            End Try
            poleData = data
            sp.Write(data)
            sp.Close()
            sp.Dispose()
            sp = Nothing

        Catch ex As Exception

        End Try

    End Sub
End Class
