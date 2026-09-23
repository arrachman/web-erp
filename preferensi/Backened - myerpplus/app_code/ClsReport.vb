Imports System.Web
Imports System.Web.SessionState

Public Class ClsReport

    Public Enum AsOutputFormat As Integer
        OutputExcel = 1
        OutputHTML = 2
        OutputPDF = 3
        OutputRTF = 4
        OutputText = 5
        OutputTiff = 6
    End Enum

    'Setting utama
    Public NamaLaporan As String
    Public ConStr As String
    Public SQLStr As String
    Public NamaFileTujuan As String
    Public OutputFormat As AsOutputFormat
    Public TerbilangJml As Double
    Public RMATAUANG As String

    'Judul Laporan
    Public RTITLE As String

    'Perusahaan
    Public PTNAMA As String
    Public PTALAMAT As String
    Public PTKOTA As String
    Public PTPROPINSI As String
    Public PTNOTELP As String
    Public PTNOFAX As String
    Public PTEMAIL As String
    Public PTWEBSITE As String
    Public PTKOTATTD As String

    'Parameter Tambahan
    Public PARAM1 As String
    Public PARAM2 As String
    Public PARAM3 As String
    Public PARAM4 As String
    Public PARAM5 As String

    Public Function CreateReport() As String
        On Error GoTo salah

        Dim Server As HttpServerUtility = HttpContext.Current.Server
        Dim Rpt As Object = Server.CreateObject("AsReportNET.ClsMain")

        With Rpt

            'Setting utama
            .NamaLaporan = NamaLaporan
            .ConStr = ConStr
            .SQLStr = SQLStr
            .NamaFileTujuan = NamaFileTujuan
            .OutputFormat = OutputFormat
            .TerbilangJml = TerbilangJml

            'Judul Laporan
            .RTITLE = RTITLE

            'Parameter tambahan
            .PARAM1 = PARAM1
            .PARAM2 = PARAM2
            .PARAM3 = PARAM3
            .PARAM4 = PARAM4
            .PARAM5 = PARAM5

            'Perusahaan
            .PTNAMA = PTNAMA
            .PTALAMAT = PTALAMAT
            .PTKOTA = PTKOTA
            .PTPROPINSI = PTPROPINSI
            .PTNOTELP = PTNOTELP
            .PTNOFAX = PTNOFAX
            .PTEMAIL = PTEMAIL
            .PTWEBSITE = PTWEBSITE
            .PTKOTATTD = PTKOTATTD

            Return .CreateReport()
        End With

        Rpt = Nothing

        Exit Function
salah:
        Return "Err : " & Err.Number & " / " & Err.Description

    End Function

End Class
