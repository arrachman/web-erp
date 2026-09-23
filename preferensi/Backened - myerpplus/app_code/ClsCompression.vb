Imports System.IO
Imports System.IO.Compression
Imports System.Globalization
Imports System.Web

Public Class ClsCompression
    Implements IHttpModule
    Public Sub New()
    End Sub

    Public Sub Dispose() Implements IHttpModule.Dispose
    End Sub

    Public Sub Init(ByVal app As HttpApplication) Implements IHttpModule.Init
        AddHandler app.PreRequestHandlerExecute, New EventHandler(AddressOf Compress)
    End Sub

    Private Sub Compress(ByVal sender As Object, ByVal e As EventArgs)
        Dim app As HttpApplication = DirectCast(sender, HttpApplication)
        Dim request As HttpRequest = app.Request
        Dim response As HttpResponse = app.Response

        'Ajax Web Service request is always starts with application/json
        If request.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("application/") Or _
            request.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("text/") Then
            'Skip Compression for IE6, User may be using an older version of IE6 which does not support compression
            'If Not ((request.Browser.IsBrowser("IE")) AndAlso (request.Browser.MajorVersion <= 6)) Then
            Dim acceptEncoding As String = request.Headers("Accept-Encoding")

            If Not String.IsNullOrEmpty(acceptEncoding) Then
                acceptEncoding = acceptEncoding.ToLower(CultureInfo.InvariantCulture)

                If acceptEncoding.Contains("gzip") Then
                    response.Filter = New GZipStream(response.Filter, CompressionMode.Compress)
                    response.AddHeader("Content-encoding", "gzip")
                ElseIf acceptEncoding.Contains("deflate") Then
                    response.Filter = New DeflateStream(response.Filter, CompressionMode.Compress)
                    response.AddHeader("Content-encoding", "deflate")
                End If
                'End If
            End If
        End If
    End Sub
End Class