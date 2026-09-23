'Imports Newtonsoft.Json
'Imports Newtonsoft.Json.Linq
Imports System.IO

Public Class Libs
    Public data As String, getlokasi As String
    'Public lang As JObject
    Public dt As DataTable = New DataTable, dr As DataRow
    Private Sub Libs_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        getlokasi = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
        loadlibs()
    End Sub

    Sub loadlibs()
        With dt
            .Clear()
            .Columns.Clear()
            .Columns.Add("No")
            .Columns.Add("ID")
            .Columns.Add("Translate")
        End With
        With dg
            .DataSource = dt
            .Columns(0).Width = 30
            .Columns(1).Width = 200
            .Columns(2).Width = 200
        End With
        dg.Columns(dg.ColumnCount - 1).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        data = File.ReadAllText(getlokasi + "\ENG.json")
        'lang = JObject.Parse(data)
        'For i = 0 To lang("libs").Count - 1
        '    dr = dt.NewRow()
        '    dr(0) = dg.RowCount
        '    dr(1) = lang("libs")(i)("l")
        '    dr(2) = lang("libs")(i)("t")
        '    dt.Rows.Add(dr)
        'Next
    End Sub

    Private Sub btnload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnload.Click
        loadlibs()
    End Sub

    Private Sub btnsimpan_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnsimpan.Click
        Dim simpan As String = "{""libs"":["
        For i = 0 To dg.RowCount - 2
            simpan += "{""l"":""" + dt.Rows(i)(1) + """, ""t"":""" + dt.Rows(i)(2) + """}"
            If i < dg.RowCount - 2 Then
                simpan += ", "
            End If
        Next
            simpan += "]}"

        File.Delete(getlokasi + "\ENG.json")
        File.Create(getlokasi + "\ENG.json").Dispose()

        Dim streamWriter As StreamWriter = New StreamWriter(getlokasi + "\ENG.json", True)
        With streamWriter
            .Write(simpan)
            .Flush()
            .Dispose()
            .Close()
        End With
    End Sub

    Private Sub btndelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btndelete.Click
        dg.Rows.Remove(dg.CurrentRow)
    End Sub
End Class
