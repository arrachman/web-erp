Imports System.IO
Imports System.Drawing.Printing
Imports System.Net.Sockets

Module ModGlobal
    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼", user1 As String = "", no_urut As Integer = 0, poleData As String = ""
    Public LokasiLog As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\Log_MyERP_Hardware.txt"
    Public LokasiLogError As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\Log_Error.txt"
    Public printFont As Font
    Public streamToPrint As StreamReader
    Public dtAntri As New DataTable, dtPrinted As New DataTable
    Public dr As DataRow, go As Boolean = True
    Public serverSocket As TcpListener
    Public clientSocket As TcpClient, portname As String = ""
    Public LokasiAppConfig As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\app.config"
    Public ListUser As New Hashtable
    Public myHardware As Threading.Thread
    Private mstrOpenDrawerCode = Chr(27) & Chr(112) & Chr(48) & Chr(55) & Chr(121)
    Private mstrPartialCutCode = Chr(27) & Chr(105)
    Private mstrFullCutCode = Chr(27) & Chr(109)
    Private mstrStringToPrint As String
    Public kebijakanKu As String = ""

    Public Sub log(ByVal StrVal As String)
        If Len(StrVal) > 0 Then
            Try
                If File.Exists(LokasiLog) = False Then
                    File.Create(LokasiLog).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(LokasiLog, True)
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

    Public Sub logError(ByVal StrVal As String)
        If Len(StrVal) > 0 Then
            Try
                If File.Exists(LokasiLogError) = False Then
                    File.Create(LokasiLogError).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(LokasiLogError, True)
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

    Public Sub openReport(ByVal StrVal As String)
        Dim LokasiCetak As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\cetak_" + Now.ToString.Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt"
        If Len(StrVal) > 0 Then
            Try
                If File.Exists(LokasiCetak) = False Then
                    File.Create(LokasiCetak).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(LokasiCetak, True)
                With streamWriter
                    .Write(StrVal)
                    .Flush()
                    .Dispose()
                    .Close()
                End With
                cetak_boy(LokasiCetak)
            Catch ex As Exception
            End Try
        End If
    End Sub

    Public Sub cetak_boy(ByVal lokasiCetak As String)
        Dim line As String = Nothing
        Try
            streamToPrint = New StreamReader(lokasiCetak)
            While 0 < 1
                line = streamToPrint.ReadLine()
                If line Is Nothing Then
                    Exit While
                End If
                line = line + vbNewLine

                'RawPrinterHelper.SendStringToPrinter(print, line)
            End While
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)
            'RawPrinterHelper.SendStringToPrinter(print, vbNewLine)

            'Dim pd As New PrintDocument()
            'AddHandler pd.PrintPage, AddressOf pd_PrintPage
            'pd.Print()
            streamToPrint.Dispose()
            streamToPrint.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Function f_Random(ByVal size As Integer) As String
        Dim nilai As Char() = New Char(size - 1) {}
        Dim _rng As Random = New Random()
        Dim _chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        For i As Integer = 0 To size - 1
            nilai(i) = _chars(_rng.[Next](_chars.Length))
        Next
        Return New String(nilai)
    End Function
End Module
