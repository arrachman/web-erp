Imports System.Net.Sockets, System.IO

Public Class ChatClient

    Public Event MessageRecieved(ByVal Str As String)
    Public Event ClientExited(ByVal Client As ChatClient)

    Private sWriter As StreamWriter
    Private Client As TcpClient
    Public lokasi As String

    Sub New(ByVal xclient As TcpClient)
        Try
            Client = xclient
            Client.GetStream.BeginRead(New Byte() {0}, 0, 0, AddressOf Read, Nothing)
        Catch ex As Exception
            log("Kesalahan Baca. Ket : " & Err.Description & " | Sumber ChatClient New()")
        End Try
    End Sub

    Private Sub Read()
        Try
            RaiseEvent MessageRecieved(New StreamReader(Client.GetStream).ReadLine)
            Client.GetStream.BeginRead(New Byte() {0}, 0, 0, New AsyncCallback(AddressOf Read), Nothing)
        Catch ex As Exception
            'log("Kesalahan Baca. Ket : " & Err.Description & " | Sumber ChatClient Read()")
            Try
                RaiseEvent ClientExited(Me)
            Catch e As Exception
                log("Error Read2, ClientExited : " + Err.Description)
            End Try
        End Try
    End Sub

    Public Sub Send(ByVal Message As String)
        Try
            sWriter = New StreamWriter(Client.GetStream)
            sWriter.WriteLine(Message)
            sWriter.Flush()
        Catch ex As Exception
            log("Kesalahan Kirim. Ket : " & Err.Description & " | Sumber ChatClient Send()")
        End Try
    End Sub
End Class

Public Module ModGlobal
    Public LokasiError As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\log.txt"

    Public Sub log(ByVal StrVal As String)
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
End Module
