Imports System.IO.Compression
Imports System.Web
Imports System.Web.Security

Public Class MdlCompression
    Implements IHttpModule
    Private _isDisposed As Boolean = False

    Public Sub Init(ByVal context As HttpApplication) Implements IHttpModule.Init
        AddHandler context.BeginRequest, New EventHandler(AddressOf context_BeginRequest)
    End Sub

    Private Sub context_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        Dim app As HttpApplication = TryCast(sender, HttpApplication)
        Dim ctx As HttpContext = app.Context

        If Not ctx.Request.Url.PathAndQuery.ToLower().Contains(".asmx") Then
            Return
        End If

        If IsEncodingAccepted("gzip") Then
            app.Request.Filter = New System.IO.Compression.GZipStream(app.Request.Filter, CompressionMode.Decompress)

            app.Response.Filter = New GZipStream(app.Response.Filter, CompressionMode.Compress)
            SetEncoding("gzip")
        ElseIf IsEncodingAccepted("deflate") Then
            app.Response.Filter = New DeflateStream(app.Response.Filter, CompressionMode.Compress)
            SetEncoding("deflate")
        End If
    End Sub
    Private Function IsEncodingAccepted(ByVal encoding As String) As Boolean
        Return HttpContext.Current.Request.Headers("Accept-encoding") IsNot Nothing AndAlso HttpContext.Current.Request.Headers("Accept-encoding").Contains(encoding)
    End Function
    Private Sub SetEncoding(ByVal encoding As String)
        HttpContext.Current.Response.AppendHeader("Content-encoding", encoding)
    End Sub
    Private Sub Dispose(ByVal dispose__1 As Boolean)
        _isDisposed = dispose__1
    End Sub
    Protected Overrides Sub Finalize()
        Try
            Dispose(False)
        Finally
            MyBase.Finalize()
        End Try
    End Sub
    Public Sub Dispose() Implements IHttpModule.Dispose
        Dispose(True)
    End Sub
End Class