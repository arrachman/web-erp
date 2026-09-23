Imports Microsoft.VisualBasic
Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.IO

Public Module ModGlobal

    Public KataKunci As String 'Source code ini punya Alfasoft
    Public ErrNumber As Long = 0, ErrStep As Long = 0, ErrDescription As String = "", ErrSource As String = "", ErrLine As Integer = 0, HasilSQL As String = ""
    Public strCon As String = ""
    Public strConStieReport As String
    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public sptLogin As String = "Θ"
    Public LocationResultReport, LocationMRT, StiOdbcDB As String
    Public IDGenerate, WebAccessKey, Filter, OrderBy, GroupBy, Param1, Title, unama, Sumber, NamaPerusahaan As String
    Public Modul, MenuReport, Item, RQuery, Extension, userid, IDTransaksi, xstep As Integer
    Public LokasiError As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\Log_Error_AgentCogs.txt"
    Public getlokasi As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
    Public dataProses As String = ""
    Public namabackup As String = Now.Year & Now.Month & Now.Day & "_" & Now.Hour & Now.Minute & Now.Second
    Public LokasiHitungUlang As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\LogHitungUlangHPPFIFO.txt"
    Public LokasiHitungUlangTime As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\LogHitungUlangHPPFIFO" & namabackup & ".txt"

    Function readTagXML(ByVal data As String, ByVal tag As String) As String
        Try

            Dim depan, belakang As New Integer
            For i = 0 To data.Length
                If data.Substring(i, tag.Length) = tag Then
                    depan = i + tag.Length + 1
                    For n = depan To data.Length
                        If data.Substring(n, tag.Length) = tag Then
                            belakang = n - 2 - depan
                            Return data.Substring(depan, belakang)
                        End If
                    Next
                End If
            Next
        Catch ex As Exception
            SimpanLogToFile(Err.Description)
        End Try
        Return ""
    End Function

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

    Public Sub Log(ByVal StrVal As String)
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

    Public Sub LogHItungUlang(ByVal StrVal As String)
        If Len(StrVal) > 0 Then
            Try
                If File.Exists(LokasiHitungUlang) = False Then
                    File.Create(LokasiHitungUlang).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(LokasiHitungUlang, True)
                With streamWriter
                    .Write(Now & " : " & StrVal & vbCrLf)
                    .Flush()
                    .Dispose()
                    .Close()
                End With

                If File.Exists(LokasiHitungUlangTime) = False Then
                    File.Create(LokasiHitungUlangTime).Dispose()
                End If

                streamWriter = New StreamWriter(LokasiHitungUlangTime, True)
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

#Region "DataTable"
    '//Datatable - Tipe data untuk tambah field di datatable
    Public Enum AsEnumTypeData
        AsBoolean = 0 : AsByte = 1 : AsChar = 2 : AsDateTime = 3 : AsDecimal = 4 : AsDouble = 5 : AsInt16 = 6 : AsInt32 = 7
        AsInt64 = 8 : AsSByte = 9 : AsSingle = 10 : AsString = 11 : AsTimeSpan = 12 : AsUInt16 = 13 : AsUInt32 = 14 : AsUInt64 = 15
    End Enum

    '//Datatable - Tambah Field/Kolom
    Public Function AsDataTableTambahField(ByVal dt1 As DataTable, ByVal NamaField As String, Optional ByVal TipeData As AsEnumTypeData = AsEnumTypeData.AsString, Optional ByVal ApaFieldAutoIncrement As Boolean = False, Optional ByVal AutoIncrementStart As Integer = 1, Optional ByVal ApaFieldUnik As Boolean = False, Optional ByVal ApaFieldPrimaryKey As Boolean = False, Optional ByVal PrimaryKeyIndex As Integer = 0) As Boolean
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim NamaTipe As String = "System.String"

        xStep = 1 'Set nama tipe
        Select Case TipeData
            Case 0 : NamaTipe = "System.Boolean"
            Case 1 : NamaTipe = "System.Byte"
            Case 2 : NamaTipe = "System.Char"
            Case 3 : NamaTipe = "System.DateTime"
            Case 4 : NamaTipe = "System.Decimal"
            Case 5 : NamaTipe = "System.Double"
            Case 6 : NamaTipe = "System.Int16"
            Case 7 : NamaTipe = "System.Int32"
            Case 8 : NamaTipe = "System.Int64"
            Case 9 : NamaTipe = "System.SByte"
            Case 10 : NamaTipe = "System.Single"
            Case 11 : NamaTipe = "System.String"
            Case 12 : NamaTipe = "System.TimeSpan"
            Case 13 : NamaTipe = "System.UInt16"
            Case 14 : NamaTipe = "System.UInt32"
            Case 15 : NamaTipe = "System.UInt64"
        End Select

        xStep = 2 'Jika blm ada Field yg ingin dibuat, maka proses
        If dt1.Columns.Contains(NamaField) = False Then

            xStep = 3 'Set dataField baru
            Dim dc1 = New DataColumn(NamaField, Type.GetType(NamaTipe))

            xStep = 4 'Jika Fieldnya AutoIncrement
            If ApaFieldAutoIncrement = True Then
                dc1.AutoIncrement = True
                dc1.AutoIncrementSeed = 1
                dc1.ReadOnly = True
            End If

            xStep = 5 'Jika Fieldnya unik
            If ApaFieldUnik = True Then
                dc1.Unique = True
            End If

            xStep = 6 'Tambahkan Fieldnya ke datatable
            dt1.Columns.Add(dc1)

            xStep = 7 'Jika Fieldnya Primary Key
            If ApaFieldPrimaryKey = True Then
                Dim pK As DataColumn() = New DataColumn(0) {}
                pK(PrimaryKeyIndex) = dc1
                dt1.PrimaryKey = pK
            End If

        End If

        Return True

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableTambahField", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return False
    End Function

    '//Datatable - Tambah Data
    Public Function AsDataTableTambahData(ByVal dt1 As DataTable, ByVal ArrayKolom As String, ByVal ArrayNilai As String, Optional ByVal PemisahArray As String = "~") As Boolean
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim row As DataRow = Nothing
        Dim xArrayKolom() As String = Split(ArrayKolom, PemisahArray)
        Dim xArrayNilai() As String = Split(ArrayNilai, PemisahArray)

        xStep = 1 'Proses
        If xArrayKolom.Count = xArrayNilai.Count Then
            xStep = 2 'Tambah baru kosong baru
            row = dt1.NewRow()
            xStep = 3 'Set datanya
            For I As Integer = 0 To xArrayKolom.Count - 1
                row(xArrayKolom(I)) = xArrayNilai(I)
            Next
            xStep = 4 'Tambah row ke datatable
            dt1.Rows.Add(row)
        End If

        Return True

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableTambahData", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return False
    End Function

    '//Datatable - Update Data
    Public Function AsDataTableUpdateData(ByVal dt1 As DataTable, ByVal StrFilter As String, ByVal ArrayKolom As String, ByVal ArrayNilai As String, Optional ByVal PemisahArray As String = "~") As Boolean
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim xArrayKolom() As String = Split(ArrayKolom, PemisahArray)
        Dim xArrayNilai() As String = Split(ArrayNilai, PemisahArray)

        xStep = 1 'Filter dulu
        Dim DrItems() As DataRow = dt1.Select(StrFilter)

        xStep = 2 'Update datanya
        For Each dr1 As DataRow In DrItems
            If xArrayKolom.Count = xArrayNilai.Count Then
                xStep = 3 'Set datanya
                For I As Integer = 0 To xArrayKolom.Count - 1
                    dr1(xArrayKolom(I)) = xArrayNilai(I)
                Next
                xStep = 4 'Update datatable
                dt1.AcceptChanges()
            End If
        Next

        Return True
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableUpdateData", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return False
    End Function

    '//Datatable - Delete Data
    Public Function AsDataTableDeleteData(ByVal dt1 As DataTable, ByVal StrFilter As String) As Boolean
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""

        xStep = 1 'Set datarow yg akan dihapus
        Dim BarisYangDihapus() As DataRow = dt1.Select(StrFilter)
        If Not BarisYangDihapus Is Nothing Then
            If BarisYangDihapus.Length > 0 Then
                xStep = 2 'Proses hapus satu persatu
                For i As Integer = 0 To BarisYangDihapus.Length - 1
                    BarisYangDihapus(i).Delete()
                Next
                xStep = 3 'Update data
                dt1.AcceptChanges()
            End If
        End If

        Return True
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDeleteData", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return False
    End Function

    '//DataTable - Ambil DataTable dari DB berdasarkan SQL yg diberikan. AsAmbilDataTable->AsDataTableAmbilDariDB
    Public Function AsDataTableAmbilDariDB(ByVal StrSQL As String, ByVal strCon As String) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 4 'Set datatable
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        Return dt1
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDataTableAmbilDariDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dt1
    End Function

    '//DataTable - Filter dan sort DataTable untuk menghasilkan DataRow(). StrFilter = "kotaasal = 'Malang'". StrSort = "nama desc". AsFilterDataTable->AsDataTableFilterSort
    Public Function AsDataTableFilterSort(ByVal dt1 As DataTable, ByVal StrFilter As String, Optional ByVal StrSort As String = "", Optional ByVal DataDirandom As Boolean = False) As DataRow()
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim DrItems() As DataRow

        xStep = 1 'Cek apakah ada permintaan utk random data?
        If DataDirandom = True Then
            xStep = 2 'Jika dirandom, maka tambahkan field untuk menyimpan data randomnya
            If dt1.Columns.Contains("RandomNum") = False Then
                dt1.Columns.Add(New DataColumn("RandomNum", Type.GetType("System.Int32")))
            End If

            xStep = 3 'Isikan data randomnya
            Dim random As New Random()
            For J As Integer = 0 To dt1.Rows.Count - 1
                dt1.Rows(J)("RandomNum") = random.Next(10000)
            Next

            xStep = 4 'Filter dan urutkan data dg field random
            DrItems = dt1.Select(StrFilter, "RandomNum")
        Else
            xStep = 5 'Jika tidak dirandom, maka filter dan sort dg parameter yg diminta user
            If Len(StrSort) = 0 Then
                DrItems = dt1.Select(StrFilter)
            Else
                DrItems = dt1.Select(StrFilter, StrSort)
            End If
        End If

        Return DrItems
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableFilterSort", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return DrItems
    End Function

    '//DataTable - Filter dan sort DataTable untuk menghasilkan DataTable baru. StrFilter = "kotaasal = 'Malang'". StrSort = "nama desc". AsFilterDataTableDt->AsDataTableFilterSortDt
    Public Function AsDataTableFilterSortDt(ByVal dt1 As DataTable, ByVal StrFilter As String, Optional ByVal StrSort As String = "", Optional ByVal DataDirandom As Boolean = False) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim DrItems() As DataRow
        Dim dtx As DataTable = New DataTable()

        xStep = 1 'Cek apakah ada permintaan utk random data?
        If DataDirandom = True Then
            xStep = 2 'Jika dirandom, maka tambahkan field untuk menyimpan data randomnya
            If dt1.Columns.Contains("RandomNum") = False Then
                dt1.Columns.Add(New DataColumn("RandomNum", Type.GetType("System.Int32")))
            End If

            xStep = 3 'Isikan data randomnya
            Dim random As New Random()
            For J As Integer = 0 To dt1.Rows.Count - 1
                dt1.Rows(J)("RandomNum") = random.Next(10000)
            Next

            xStep = 4 'Filter dan urutkan data dg field random
            DrItems = dt1.Select(StrFilter, "RandomNum")
            dtx = dt1.Clone
        Else
            xStep = 5 'Jika tidak dirandom, maka filter dan sort dg parameter yg diminta user
            If Len(StrSort) = 0 Then
                DrItems = dt1.Select(StrFilter)
            Else
                DrItems = dt1.Select(StrFilter, StrSort)
            End If
            dtx = dt1.Clone
        End If

        xStep = 6 'Masukkan data datarow hasil filter dan sort ke datatable temporary
        For I = 0 To DrItems.Length - 1
            dtx.Rows.Add(DrItems(I).ItemArray)
        Next

        Return dtx
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableFilterSortDt", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dtx
    End Function

    '//DataTable - Filter dan sort dan limit hasil DataTable untuk menghasilkan DataTable baru. StrFilter = "kotaasal = 'Malang'". StrSort = "nama desc". AsFilterDataTableLimit->AsDataTableFilterLimit
    Public Function AsDataTableFilterLimit(ByVal dt1 As DataTable, ByVal StrFilter As String, Optional ByVal StrSort As String = "", Optional ByVal xStart As Long = 0, Optional ByVal xRowCount As Long = 0, Optional ByVal DataDirandom As Boolean = False) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim DrItems() As DataRow
        Dim xStrSort As String = StrSort
        Dim dtx As DataTable = New DataTable()

        xStep = 1 'Jika dirandom, maka tambahkan field untuk menyimpan data randomnya
        If DataDirandom = True Then
            If dt1.Columns.Contains("RandomNum") = False Then
                dt1.Columns.Add(New DataColumn("RandomNum", Type.GetType("System.Int32")))
            End If

            xStep = 2 'Isi data randomnya
            Dim random As New Random()
            For J As Integer = 0 To dt1.Rows.Count - 1
                dt1.Rows(J)("RandomNum") = random.Next(10000)
            Next
            xStrSort = "RandomNum"
        End If

        xStep = 3 'Buat datatable temporary yg akan diisi hasil penfilteran sesuai limit
        dtx = dt1.Clone

        xStep = 4 'Proses filter dan sort
        If Len(xStrSort) = 0 Then
            DrItems = dt1.Select(StrFilter)
        Else
            DrItems = dt1.Select(StrFilter, xStrSort)
        End If

        xStep = 5 'Ambil sesuai jumlah limit yg diminta
        Dim JmlData As Integer = DrItems.Count
        For I As Integer = xStart To (xStart + xRowCount) - 1
            If I >= JmlData Then
                Exit For
            Else
                dtx.Rows.Add(DrItems(I).ItemArray)
            End If
        Next

        Return dtx
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableFilterLimit", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dtx
    End Function

    '//DataTable - Melimit datatable untuk menghasilkan datatable baru yg sdh terlimit. AsLimitDataTable->AsDataTableLimit
    Public Function AsDataTableLimit(ByVal dt1 As DataTable, Optional ByVal xStart As Long = 0, Optional ByVal xRowCount As Long = 0) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim dtx As DataTable = New DataTable()

        xStep = 1 'Cloning datatable lalu isikan datanya
        dtx = dt1.Clone()
        For i = xStart To xStart + xRowCount
            If i >= dt1.Rows.Count Then
                Exit For
            Else
                dtx.ImportRow(dt1.Rows(i))
            End If
        Next

        Return dtx
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableLimit", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dtx
    End Function

    '//DataTable - Mengambil/mencari suatu nilai pada field tertentu. AsFilterDataTableX->AsDataTableDLookup
    Public Function AsDataTableDLookup(ByVal dt1 As DataTable, ByVal StrField As String, Optional ByVal StrFilter As String = "", Optional ByVal NilaiJikaEOF As String = "") As String
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim DrItems() As DataRow
        Dim FieldIdx As Integer = 0, I As Integer = 0

        xStep = 1 'Tentukan index field yg dipilih
        For Each dc1 As DataColumn In dt1.Columns
            If LCase(dc1.ColumnName) = LCase(StrField) Then
                FieldIdx = I
            End If
            I = I + 1
        Next

        xStep = 2 'Filter datanya dan kembalikan nilai datanya sesuai field yg diminta
        If Len(StrFilter) > 0 Then
            xStep = 3 'Proses dg filter
            DrItems = dt1.Select(StrFilter)
            If DrItems.Count > 0 Then
                Dim dr1 As DataRow = DrItems(0)
                Return dr1(FieldIdx)
            Else
                Return NilaiJikaEOF
            End If
        Else
            xStep = 4 'Proses tanpa filter
            If dt1.Rows.Count > 0 Then
                Dim dr2 As DataRow = dt1.Rows(0)
                Return dr2(FieldIdx)
            Else
                Return NilaiJikaEOF
            End If
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDLookup", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return NilaiJikaEOF
    End Function

    '//DataTable - Menghitung jml record pada suatu datatable. AsFilterDataTableCount->AsDataTableDCount
    Public Function AsDataTableDCount(ByVal dt1 As DataTable, Optional ByVal StrFilter As String = "") As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""

        xStep = 1 'Proses
        If Len(StrFilter) > 0 Then
            xStep = 2 'Proses dg filter
            Dim DrItems() As DataRow
            DrItems = dt1.Select(StrFilter)
            Return DrItems.Count
        Else
            xStep = 3 'Proses tanpa filter
            Return dt1.Rows.Count
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDCount", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function

    '//DataTable - Menghitung jml nilai (sum) pada suatu datatable
    Public Function AsDataTableDSum(ByVal dt1 As DataTable, ByVal StrField As String, Optional ByVal StrFilter As String = "") As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim Jml As Double = 0

        xStep = 1 'Proses
        Dim objSum As Object = dt1.Compute("sum(" & StrField & ")", StrFilter)
        Jml = CDbl(objSum.ToString())

        Return Jml
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDSum", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function

    '//DataTable - Menghitung rata2 jml nilai (avg) pada suatu datatable
    Public Function AsDataTableDAvg(ByVal dt1 As DataTable, ByVal StrField As String, Optional ByVal StrFilter As String = "") As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim Jml As Double = 0, Ke As Integer = 0

        xStep = 1 'Proses
        Dim objSum As Object = dt1.Compute("avg(" & StrField & ")", StrFilter)
        Jml = CDbl(objSum.ToString())

        Return Jml
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDAvg", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function

    '//DataTable - Mengambil nilai terbesar (max) pada suatu field di datatable. Perintah=max, min, avg
    Public Function AsDataTableDMax(ByVal dt1 As DataTable, ByVal StrField As String, Optional ByVal StrFilter As String = "", Optional ByVal Perintah As String = "max") As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim Jml As Double = 0, Ke As Integer = 0

        xStep = 1 'Proses
        Dim objSum As Object = dt1.Compute(Perintah & "(" & StrField & ")", StrFilter)
        Jml = CDbl(objSum.ToString())

        Return Jml
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDMax", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function

    '//DataTable - Ambil data maksimal per grup
    Public Function AsDataTableDMaxPerGrup(ByVal dt1 As DataTable, ByVal StrFieldGrup As String, ByVal StrFieldJml As String, Optional ByVal GrupBerupaNumeric As Boolean = False, Optional ByVal Perintah As String = "max") As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim DaftarGrup As String = ""
        Dim DtNew As DataTable = dt1.Clone

        xStep = 1 'Ambil daftar grupnya
        For Each dr1 As DataRow In dt1.Rows
            Dim GrupX As String = dr1(StrFieldGrup)
            If InStr(DaftarGrup, GrupX) = 0 Then
                DaftarGrup += IIf(Len(DaftarGrup) = 0, GrupX, "~" & GrupX)
            End If
        Next

        xStep = 2 'Ambil max per grup
        Dim DtGrups() As String = Split(DaftarGrup, "~")
        Dim JmlMax As Double = 0
        Dim dr2() As DataRow

        For Each NamaGrup As String In DtGrups

            xStep = 3 'Ambil jml maksimalnya
            If GrupBerupaNumeric = True Then
                JmlMax = AsDataTableDMax(dt1, StrFieldJml, StrFieldGrup & "=" & NamaGrup, Perintah)
                dr2 = dt1.Select(StrFieldGrup & "=" & NamaGrup & " and " & StrFieldJml & "=" & JmlMax)
            Else
                JmlMax = AsDataTableDMax(dt1, StrFieldJml, StrFieldGrup & "='" & NamaGrup & "'", Perintah)
                dr2 = dt1.Select(StrFieldGrup & "='" & NamaGrup & "' and " & StrFieldJml & "=" & JmlMax)
            End If

            xStep = 4 'Filter sesuai jml maksimal, lalu masukkan ke datatable yg baru
            For I = 0 To dr2.Length - 1
                DtNew.Rows.Add(dr2(I).ItemArray)
            Next

        Next

        Return DtNew
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDMaxPerGrup", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return DtNew
    End Function

    '//DataTable - Ambil data maksimal per grup ber tingkat. StrFieldGrups : jenjang~kelas~pelajaran. StrFieldGrupsTipeData : N~N~N.  N=Numeric, T=Text
    Public Function AsDataTableDMaxPerGrupBertingkat(ByVal dt1 As DataTable, ByVal StrFieldGrups As String, ByVal StrFieldGrupsTipeData As String, ByVal StrFieldJml As String) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""

        xStep = 1 'Init
        Dim DtNew As DataTable = dt1.Clone
        Dim DaftarGrup As String = ""
        Dim FieldGrup() As String = Split(StrFieldGrups, "~")
        Dim FieldGrupTipeData() As String = Split(StrFieldGrupsTipeData, "~")
        Dim JmlFieldGrup As Integer = FieldGrup.Count
        Dim Sb As StringBuilder = New StringBuilder

        xStep = 2 'Ambil daftar grupnya
        For Each dr1 As DataRow In dt1.Rows

            Sb.Clear()

            xStep = 3 'Buffer
            For J = 0 To JmlFieldGrup - 1
                Dim GrupX As String = dr1(FieldGrup(J))
                If Sb.Length = 0 Then
                    Sb.Append(GrupX)
                Else
                    Sb.Append("|" & GrupX)
                End If
            Next

            xStep = 4 'Masukkan ke daftar
            Dim KeyGrup As String = Sb.ToString
            If InStr(DaftarGrup, KeyGrup) = 0 Then
                DaftarGrup += IIf(Len(DaftarGrup) = 0, KeyGrup, "~" & KeyGrup)

                Dim JmlMax As Double = 0
                Dim dr2() As DataRow

                xStep = 5 'Set filter grupnya
                Dim NamaGrupX() As String = Split(KeyGrup, "|")
                Dim Flt As String = ""
                Dim FilterGrup As String = ""

                xStep = 6 'Set filter
                For K As Integer = 0 To FieldGrup.Count - 1
                    Select Case FieldGrupTipeData(K)
                        Case "N" 'Grupnya berupa numeric
                            Flt = "(" & FieldGrup(K) & "=" & NamaGrupX(K) & ")"
                            FilterGrup += IIf(Len(FilterGrup) = 0, Flt, " and " & Flt)
                        Case "T" 'Grupnya berupa text
                            Flt = "(" & FieldGrup(K) & "='" & NamaGrupX(K) & "')"
                            FilterGrup += IIf(Len(FilterGrup) = 0, Flt, " and " & Flt)
                    End Select
                Next

                'HttpContext.Current.Response.Write("FilterGrup : " & FilterGrup & "<br>")
                'HttpContext.Current.Response.Flush()

                xStep = 7 'Filter sesuai jml maksimal, lalu masukkan ke datatable yg baru
                JmlMax = AsDataTableDMax(dt1, StrFieldJml, FilterGrup)
                dr2 = dt1.Select(FilterGrup & " and (" & StrFieldJml & "=" & JmlMax & ")")
                For I = 0 To dr2.Length - 1
                    DtNew.Rows.Add(dr2(I).ItemArray)
                Next

            End If
        Next

        Return DtNew
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDMaxPerGrupBertingkat", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return DtNew
    End Function

    '//DataTable - Mengambil nilai terkecil (min) pada suatu field di datatable
    Public Function AsDataTableDMin(ByVal dt1 As DataTable, ByVal StrField As String, Optional ByVal StrFilter As String = "") As Integer
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim Jml As Double = 0, Ke As Integer = 0

        xStep = 1 'Proses
        Dim objSum As Object = dt1.Compute("min(" & StrField & ")", StrFilter)
        Jml = CDbl(objSum.ToString())

        Return Jml
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'AsPesanKesalahan("AsDataTableDMin", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function

    '//DataTable - Simpan datatable ke DB. AsSimpanDataTableKeDB->AsDataTableSimpanKeDB
    Public Function AsDataTableSimpanKeDB(ByVal dt1 As DataTable, ByVal StrSQLSelect As String, ByVal strCon As String) As Boolean
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Cek koneksi ke database
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQLSelect, ConX)
        Dim builder As MySqlCommandBuilder = New MySqlCommandBuilder(da1)
        da1.SelectCommand = New MySqlCommand(StrSQLSelect, ConX)
        da1.Update(dt1)

        'Tutup Koneksi
        ConX.Close()

        Return True
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDataTableSimpanKeDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return False
    End Function

    '//DataTable - Ambil data dari session/DB dan simpan kembali setelah dilakukan update. AsAmbilDataTableDariSession->AsDataTableAmbilDariSession
    '    Public Function AsDataTableAmbilDariSession(ByVal NamaSession As String, ByVal StrSQLJikaDiSessionBlmAda As String, Optional ByVal ConDb123 As Long = 1) As DataTable
    '        '--Saat input, StrSQL adalah SQL untuk ambil data kosong (tidak ada datanya). Misal Select first 1 NIS,NAMA from SISWA where NIS='XXX'. XXX ini datanya tidak ada
    '        '--Saat edit, StrSQL adalah SQL untuk mengedit transaksi tersebut. Misal Select NIS,NAMA from SISWA where NIS='S001'. S001 ini adalah NIS yg datanya akan diedit

    '        On Error GoTo Salah
    '        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
    '        Dim dt1 As DataTable = New DataTable()
    '        dt1 = CType(HttpContext.Current.Session(NamaSession), DataTable)

    '        xStep = 1 'Jika nothing maka proses
    '        If dt1 Is Nothing Then
    '            xStep = 2 'Proses sesuai koneksinya
    '            Select Case ConDb123
    '                Case 1 : dt1 = AsDataTableAmbilDariDB(StrSQLJikaDiSessionBlmAda, 1)
    '                Case 2 : dt1 = AsDataTableAmbilDariDB(StrSQLJikaDiSessionBlmAda, 2)
    '                Case 3 : dt1 = AsDataTableAmbilDariDB(StrSQLJikaDiSessionBlmAda, 3)
    '            End Select
    '        End If

    '        Return dt1
    '        Exit Function
    'Salah:
    '        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
    '        'AsPesanKesalahan("AsDataTableAmbilDariSession", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
    '        Return dt1
    '    End Function

    '//DataTable - Simpan DataTable di session ke database. AsSimpanDataTableDiSessionKeDB->AsDataTableSimpanDiSessionKeDB
    '    Public Function AsDataTableSimpanDiSessionKeDB(ByVal NamaSession As String, ByVal StrSQLSelect As String, Optional ByVal ConDb123 As Long = 1) As Boolean
    '        On Error GoTo Salah
    '        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
    '        Dim ConX As MySqlConnection = New MySqlConnection

    '        xStep = 1 'Cek koneksi ke database
    '        Select Case ConDb123
    '            Case 1 : If AsKoneksiKeDB() = True Then ConX = Con1
    '            Case 2 : If AsKoneksiKeDB2() = True Then ConX = Con2
    '            Case 3 : If AsKoneksiKeDB3() = True Then ConX = Con3
    '        End Select

    '        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQLSelect, ConX)
    '        Dim builder As MySqlCommandBuilder = New MySqlCommandBuilder(da1)
    '        da1.SelectCommand = New MySqlCommand(StrSQLSelect, ConX)
    '        Dim dt1 As DataTable = CType(AsDataTableAmbilDariSession(NamaSession, StrSQLSelect, ConDb123), DataTable)
    '        da1.Update(dt1)

    '        Return True
    '        Exit Function
    'Salah:
    '        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
    '        'AsPesanKesalahan("AsDataTableSimpanDiSessionKeDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
    '        Return False
    '    End Function

    Public Function AsDataTableAmbilDariDBCon(ByVal StrSQL As String, ByVal ConX As MySqlConnection) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        'Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Buat Koneksi
        'ConX.ConnectionString = strCon
        'ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 4 'Set datatable
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        ''Tutup Koneksi
        'ConX.Close()

        Return dt1
        Exit Function
Salah:
        'ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'If ErrNumber = 5 Then
        '    If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
        '        Err.Clear()
        '        ConX = New MySqlConnection(strCon)
        '        ConX.Open()
        '        GoTo Ulang
        '    End If
        'End If

        'AsPesanKesalahan("AsDataTableAmbilDariDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dt1
    End Function

#End Region

#Region "Penanganan data dg DataBase"
    '//Eksekusi SQL untuk insert/update/delete
    Public Function AsEksekusiSQL(ByVal StrSQL As String, ByVal strCon As String, Optional ByVal ConDb123 As Long = 1, Optional ByVal TampilkanPesanJikaErrorDuplicat As Boolean = False) As Boolean
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        xStep = 2 'Proses
        Dim Cmd1 As MySqlCommand = New MySqlCommand(StrSQL, ConX)
        HasilSQL = Cmd1.ExecuteNonQuery()

        'Tutup Koneksi
        ConX.Close()

        Return True
        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "'PRIMARY'") > 0 Then
                If TampilkanPesanJikaErrorDuplicat = False Then
                    Return False
                End If
            ElseIf InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsEksekusiSQL", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return False
    End Function
    '//AsDLookup (mencari data ke tabel)
    Public Function AsDLookup(ByVal NamaField As String, ByVal NamaTabel As String, ByVal Flt As String, ByVal strCon As String, Optional ByVal NilaiJikaEOF As String = "", Optional ByVal ConDb123 As Long = 1) As String
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim StrSQL As String = "", Hasil As String = "", Ix As Integer = 0, JmlField As Integer = 0
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Set SQL
        StrSQL = "select " & NamaField & " from " & NamaTabel & " where " & Flt

        xStep = 2 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 3 'Set datatable lalu diisi datanya
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 4 'Proses dan kembalikan hasilnya
        If dt1.Rows.Count = 0 Then
            Return NilaiJikaEOF
        Else
            JmlField = dt1.Columns.Count

            If JmlField = 1 Then
                Hasil = dt1.Rows(0)(0)
                Return Hasil
            Else
                Hasil = ""
                For Ix = 0 To JmlField - 1
                    If Len(Hasil) = 0 Then
                        Hasil = dt1.Rows(0)(Ix)
                    Else
                        Hasil = Hasil & ", " & dt1.Rows(0)(Ix)
                    End If
                Next
                Return Hasil
            End If
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        ' AsPesanKesalahan("AsDLookup", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return NilaiJikaEOF
    End Function
    '//AsDCount (menghitung jumlah record)
    Public Function AsDCount(ByVal NamaField As String, ByVal NamaTabel As String, ByVal strCon As String, Optional ByVal Flt As String = "", Optional ByVal ConDb123 As Long = 1) As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim StrSQL As String, Hasil As Double = 0
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Set SQL
        If Len(Flt) = 0 Then
            StrSQL = "select Count(" & NamaField & ") from " & NamaTabel
        Else
            StrSQL = "select Count(" & NamaField & ") from " & NamaTabel & " where " & Flt
        End If

        xStep = 2 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 3 'Set datatable lalu isikan datanya
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 4 'Proses dan kembalikan hasilnya
        If dt1.Rows.Count = 0 Then
            Return 0
        Else
            Hasil = dt1.Rows(0)(0)
            Return Hasil
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDCount", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function
    '//AsDSum (menghitung total nilai suatu field) 
    Public Function AsDSum(ByVal NamaField As String, ByVal NamaTabel As String, ByVal strCon As String, Optional ByVal Flt As String = "", Optional ByVal ConDb123 As Long = 1) As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim StrSQL As String, Hasil As Double = 0
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Set SQL
        If Len(Flt) = 0 Then
            StrSQL = "select Sum(" & NamaField & ") from " & NamaTabel
        Else
            StrSQL = "select Sum(" & NamaField & ") from " & NamaTabel & " where " & Flt
        End If

        xStep = 2 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 3 'Set datatable lalu isi datanya
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 4 'Proses dan kembalikan hasilnya
        If dt1.Rows.Count = 0 Then
            Return 0
        Else
            Hasil = dt1.Rows(0)(0)
            Return Hasil
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDSum", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function
    '//AsDAvg (menghitung rata2 nilai suatu field)
    Public Function AsDAvg(ByVal NamaField As String, ByVal NamaTabel As String, ByVal strCon As String, Optional ByVal Flt As String = "", Optional ByVal ConDb123 As Long = 1) As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim StrSQL As String, Hasil As Double = 0
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Set SQL
        If Len(Flt) = 0 Then
            StrSQL = "select Avg(" & NamaField & ") from " & NamaTabel
        Else
            StrSQL = "select Avg(" & NamaField & ") from " & NamaTabel & " where " & Flt
        End If

        xStep = 2 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 3 'Set datatable dan isi datanya
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 4 'Proses dan kembalikan hasilnya
        If dt1.Rows.Count = 0 Then
            Return 0
        Else
            Hasil = dt1.Rows(0)(0)
            Return Hasil
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDAvg", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function
    '//AsDMax (ambil nilai tertinggi pada suatu field)
    Public Function AsDMax(ByVal NamaField As String, ByVal NamaTabel As String, ByVal strCon As String, Optional ByVal Flt As String = "", Optional ByVal ConDb123 As Long = 1) As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim StrSQL As String, Hasil As Double = 0
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Set SQL
        If Len(Flt) = 0 Then
            StrSQL = "select " & NamaField & " from " & NamaTabel & " order by " & NamaField & " desc limit 1"
        Else
            StrSQL = "select " & NamaField & " from " & NamaTabel & " where " & Flt & " order by " & NamaField & " desc limit 1"
        End If

        xStep = 2 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 3 'Set datatable dan isi datanya
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 4 'Proses dan kembalikan hasilnya
        If dt1.Rows.Count = 0 Then
            Return 0
        Else
            Hasil = dt1.Rows(0)(0)
            Return Hasil
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDMax", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function
    '//AsDMin (Ambil nilai terendah pada suatu field)
    Public Function AsDMin(ByVal NamaField As String, ByVal NamaTabel As String, ByVal strCon As String, Optional ByVal Flt As String = "", Optional ByVal ConDb123 As Long = 1) As Double
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim StrSQL As String, Hasil As Double = 0
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Set SQL
        If Len(Flt) = 0 Then
            StrSQL = "select " & NamaField & " from " & NamaTabel & " order by " & NamaField & " asc limit 1"
        Else
            StrSQL = "select " & NamaField & " from " & NamaTabel & " where " & Flt & " order by " & NamaField & " asc limit 1"
        End If

        xStep = 2 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 3 'Set datatable dan isi datanya
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 4 'Proses dan kembalikan hasilnya
        If dt1.Rows.Count = 0 Then
            Return 0
        Else
            Hasil = dt1.Rows(0)(0)
            Return Hasil
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsDMin", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return 0
    End Function
    '//Ambil 1 field data dari DB berdasarkan SQL yg diberikan. AsAmbilData->AsAmbilDataDariDB
    Public Function AsAmbilDataDariDB(ByVal StrSQL As String, ByVal strCon As String, Optional ByVal ConDb123 As Long = 1, Optional ByVal NilaiJikaEOF As String = "")
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        Dim Hasil As String = ""
        Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Buat Koneksi
        ConX.ConnectionString = strCon
        ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)

        xStep = 2 'Set dataset
        Dim ds1 As DataSet = New DataSet()
        da1.Fill(ds1)

        'Tutup Koneksi
        ConX.Close()

        xStep = 3 'Kembalikan hasilnya
        If ds1.Tables(0).Rows.Count > 0 Then
            Return ds1.Tables(0).Rows(0)(0)
        Else
            Return NilaiJikaEOF
        End If

        Exit Function
Salah:
        ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        If ErrNumber = 5 Then
            If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
                Err.Clear()
                ConX = New MySqlConnection(strCon)
                ConX.Open()
                GoTo Ulang
            End If
        End If

        'AsPesanKesalahan("AsAmbilDataDariDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return NilaiJikaEOF
    End Function
#End Region

#Region "Misc"
    '//Mengganti ' jadi '' untuk masukkan data ke database
    Public Function FixQuotes(ByVal text As String) As String
        text = text.Replace("'", "''")
        Return text
    End Function

    '//Mengganti , jadi . untuk masukkan data ke database
    Public Function FixDouble(ByVal text As String) As String
        text = text.Replace(",", ".")   'Replace koma menjadi titik
        Return text
    End Function

    '//Mengganti ' jadi '' untuk masukkan data ke database
    Public Function AsFx(ByVal Dt As String)
        Dim Dx As String = Dt
        Dx = Replace(Dx, "'", "''")
        Return Dx
    End Function

    '//Misc - Ambil data dari array
    Public Function AsAmbilDariArray(ByVal Dt As String, ByVal Pemisah As String, ByVal Indek As Long)
        On Error Resume Next
        Dim A() As String
        A = Split(Dt, Pemisah)
        Return AsNz(Trim(A(Indek)), "")
    End Function

    '//Set URL SEO
    Public Function AsSeoURL(ByVal StrData As String) As String
        On Error Resume Next
        Dim Dt As String = LCase(StrData)

        If Right(Dt, 1) = "?" Then
            Dt = Left(Dt, Len(Dt) - 1)
        End If

        If Right(Dt, 1) = " " Then
            Dt = Left(Dt, Len(Dt) - 1)
        End If

        Dt = Replace(Dt, " - ", "-")
        Dt = Replace(Dt, " / ", "-")
        Dt = Replace(Dt, " \ ", "-")
        Dt = Replace(Dt, " & ", "-")

        Dt = Replace(Dt, " ", "-")
        Dt = Replace(Dt, "'", "-")
        Dt = Replace(Dt, "`", "-")
        Dt = Replace(Dt, "~", "-")
        Dt = Replace(Dt, "!", "-")
        Dt = Replace(Dt, "@", "-")
        Dt = Replace(Dt, "#", "-")
        Dt = Replace(Dt, "$", "-")
        Dt = Replace(Dt, "%", "-")
        Dt = Replace(Dt, "^", "-")
        Dt = Replace(Dt, "&", "-")
        Dt = Replace(Dt, "*", "-")
        Dt = Replace(Dt, "(", "-")
        Dt = Replace(Dt, ")", "-")
        Dt = Replace(Dt, "=", "-")
        Dt = Replace(Dt, "_", "-")
        Dt = Replace(Dt, "+", "-")
        Dt = Replace(Dt, "{", "-")
        Dt = Replace(Dt, "}", "-")
        Dt = Replace(Dt, "[", "-")
        Dt = Replace(Dt, "]", "-")
        Dt = Replace(Dt, "|", "-")
        Dt = Replace(Dt, "\", "-")
        Dt = Replace(Dt, "/", "-")
        Dt = Replace(Dt, ":", "-")
        Dt = Replace(Dt, ";", "-")
        Dt = Replace(Dt, ",", "-")
        Dt = Replace(Dt, ".", "-")
        Dt = Replace(Dt, "?", "-")
        Dt = Replace(Dt, "<", "-")
        Dt = Replace(Dt, ">", "-")
        Dt = Replace(Dt, Chr(34), "-")
        Dt = Replace(Dt, Chr(145), "-")
        Dt = Replace(Dt, Chr(146), "-")
        Dt = Replace(Dt, Chr(147), "-")
        Dt = Replace(Dt, Chr(148), "-")
        Dt = Replace(Dt, "---", "-")
        Dt = Replace(Dt, "--", "-")

        If Right(Dt, 1) = "-" Then
            Dt = Left(Dt, Len(Dt) - 1)
        End If

        Return Dt
    End Function

    '//Set keyword SEO
    Public Function AsSeoKeyword(ByVal StrData As String) As String
        Dim Dt As String = LCase(StrData)

        Dt = AsHilangkanSpasiDpnBlk(Dt)

        If Right(Dt, 1) = "?" Then
            Dt = Left(Dt, Len(Dt) - 1)
        End If

        Dt = Replace(Dt, " - ", ",")
        Dt = Replace(Dt, " / ", ",")
        Dt = Replace(Dt, " \ ", ",")
        Dt = Replace(Dt, " & ", ",")

        Dt = Replace(Dt, " ", ",")
        Dt = Replace(Dt, "'", "")
        Dt = Replace(Dt, "`", "")
        Dt = Replace(Dt, "~", "")
        Dt = Replace(Dt, "!", "")
        Dt = Replace(Dt, "@", "")
        Dt = Replace(Dt, "#", "")
        Dt = Replace(Dt, "$", "")
        Dt = Replace(Dt, "%", "")
        Dt = Replace(Dt, "^", "")
        Dt = Replace(Dt, "&", "")
        Dt = Replace(Dt, "*", "")
        Dt = Replace(Dt, "(", "")
        Dt = Replace(Dt, ")", "")
        Dt = Replace(Dt, "=", "")
        Dt = Replace(Dt, "_", "")
        Dt = Replace(Dt, "+", ",")
        Dt = Replace(Dt, "{", "")
        Dt = Replace(Dt, "}", "")
        Dt = Replace(Dt, "[", "")
        Dt = Replace(Dt, "]", "")
        Dt = Replace(Dt, "|", ",")
        Dt = Replace(Dt, "\", ",")
        Dt = Replace(Dt, "/", ",")
        Dt = Replace(Dt, ":", ",")
        Dt = Replace(Dt, ";", ",")
        Dt = Replace(Dt, ".", "")
        Dt = Replace(Dt, "?", "")
        Dt = Replace(Dt, "<", "")
        Dt = Replace(Dt, ">", "")
        Dt = Replace(Dt, Chr(34), "")
        Dt = Replace(Dt, Chr(145), "")
        Dt = Replace(Dt, Chr(146), "")
        Dt = Replace(Dt, Chr(147), "")
        Dt = Replace(Dt, Chr(148), "")
        Dt = Replace(Dt, ",,,", ",")
        Dt = Replace(Dt, ",,", ",")

        If Right(Dt, 1) = "," Then
            Dt = Left(Dt, Len(Dt) - 1)
        End If

        Return Dt
    End Function

    '//Set description SEO
    Public Function AsSeoDescription(ByVal StrData As String) As String
        Dim Dt As String = StrConv(StrData, vbProperCase)
        Dt = AsHilangkanSpasiDpnBlk(Dt)
        Dt = Replace(Dt, Chr(34), "")
        Dt = Replace(Dt, Chr(145), "")
        Dt = Replace(Dt, Chr(146), "")
        Dt = Replace(Dt, Chr(147), "")
        Dt = Replace(Dt, Chr(148), "")

        Return Dt
    End Function

    '//Set tag jadi link. Misal : malang, surabaya --> <a href=''>malang</a><a href=''>malang</a>
    Public Function AsTagJadiLink(ByVal StrTag As String, ByVal StrLinkDepan As String, Optional ByVal StrLinkBelakang As String = "", Optional ByVal StrNamaClass As String = "", Optional ByVal StrTarget As String = "", Optional ByVal StrOnClick As String = "", Optional ByVal StrPemisah As String = ",", Optional ByVal KecilkanLinknya As Boolean = True, Optional ByVal KecilkanHurufnya As Boolean = False)
        On Error Resume Next

        Dim Sb As StringBuilder = New StringBuilder
        Dim Dt As String = StrTag
        Dim SdhTerisi As Boolean = False
        Dim TgText As String = ""

        Dim Dtx() As String = Split(Dt, StrPemisah)
        Dim NamaClass As String = IIf(Len(StrNamaClass) = 0, "", "class=" & Chr(34) & StrNamaClass & Chr(34) & " ")
        Dim Target As String = IIf(Len(StrTarget) = 0, "", "target=" & Chr(34) & StrTarget & Chr(34) & " ")
        Dim OnClick As String = IIf(Len(StrOnClick) = 0, "", "onclick=" & Chr(34) & StrOnClick & Chr(34) & " ")

        For Each Tg As String In Dtx
            Tg = AsHilangkanSpasiDpnBlk(Tg)
            TgText = Tg

            If KecilkanLinknya = True Then
                Tg = LCase(Tg)
            End If
            If KecilkanHurufnya = True Then
                TgText = LCase(TgText)
            End If

            If SdhTerisi = False Then
                Sb.Append("<a " & NamaClass & Target & OnClick & "href=" & Chr(34) & StrLinkDepan & Tg & StrLinkBelakang & Chr(34) & ">" & TgText & "</a>")
            Else
                Sb.Append(", <a " & NamaClass & Target & OnClick & "href=" & Chr(34) & StrLinkDepan & Tg & StrLinkBelakang & Chr(34) & ">" & TgText & "</a>")
            End If
            SdhTerisi = True
        Next

        Return Sb.ToString
    End Function

    '//Hilangkan spasi didepan dan belakang
    Public Function AsHilangkanSpasiDpnBlk(ByVal StrData As String, Optional ByVal HilangkanSpasiDepan As Boolean = True, Optional ByVal HilangkanSpasiBelakang As Boolean = True) As String
        On Error Resume Next
        Dim Dt As String = StrData
Ulang:
        If HilangkanSpasiDepan = True Then
            If Left(Dt, 1) = " " Then
                Dt = Right(Dt, Len(Dt) - 1)
                GoTo Ulang
            End If
        End If

Ulang2:
        If HilangkanSpasiBelakang = True Then
            If Right(Dt, 1) = " " Then
                Dt = Left(Dt, Len(Dt) - 1)
                GoTo Ulang2
            End If
        End If

        Return Dt
    End Function

    '//Hitungan waktu seperti di Facebook, misal 1 menit yang lalu, 14 jam yang lalu, dsb
    Public Function AsHitungWaktu(ByVal StrTglJamDibuat As String, Optional ByVal StrTambahan As String = " yang lalu") As String

        Try
            Dim CreatedDateTime As DateTime = Convert.ToDateTime(StrTglJamDibuat)
            Dim StrReturn As String = Nothing
            Dim TimeDiff As TimeSpan = DateTime.Now - CreatedDateTime
            Dim MinDiff As Double = Convert.ToDouble(TimeDiff.TotalMinutes.ToString())
            Dim SecDiff As Double = Convert.ToDouble(TimeDiff.TotalSeconds.ToString())

            If MinDiff < 0 Then
                MinDiff = 0
            End If

            If SecDiff >= 0 And SecDiff < 60 Then
                StrReturn = Math.Floor(Convert.ToDecimal(SecDiff)).ToString() & " detik" & StrTambahan
                Return StrReturn
                Exit Function
            End If

            If MinDiff < 60 Then
                StrReturn = Math.Floor(Convert.ToDecimal(MinDiff)).ToString() & " menit" & StrTambahan
            Else

                MinDiff = MinDiff / 60

                If MinDiff < 24 Then
                    StrReturn = Math.Floor(Convert.ToDecimal(MinDiff)).ToString() & " jam" & StrTambahan
                Else
                    MinDiff = MinDiff / 24
                    If MinDiff < 7 Then
                        StrReturn = Math.Floor(Convert.ToDecimal(MinDiff)).ToString() & " hari" & StrTambahan
                    ElseIf MinDiff < 30 Then
                        MinDiff = MinDiff / 7
                        StrReturn = Math.Floor(Convert.ToDecimal(MinDiff)).ToString() & " minggu" & StrTambahan
                    Else
                        MinDiff = MinDiff / 30
                        StrReturn = Math.Floor(Convert.ToDecimal(MinDiff)).ToString() & " bulan" & StrTambahan
                    End If
                End If
            End If

            Return StrReturn

        Catch ex As Exception
            Return "Waktu yang lalu"
        End Try
    End Function

    ''//Tampilkan pesan error
    'Public Sub AsPesanKesalahan(ByVal NamaSumber As String, ByVal ErrNumber As Long, ByVal ErrDescription As String, ByVal ErrSource As String, ByVal ErrLine As Integer, Optional ByVal ErrStep As Long = 0)
    '    On Error Resume Next
    '    If ErrNumber <> 0 Then Exit Sub
    '    If Len(ErrDescription) = 0 Then Exit Sub

    '    Dim Hasil As String = "Ada kesalahan di " & NamaSumber & " :<br>" & _
    '        "Nomor : " & ErrNumber & "<br>" & _
    '        "Uraian : " & ErrDescription & "<br>" & _
    '        "Sumber : " & ErrSource & "<br>" & _
    '        "Baris : " & ErrLine & "<br>" & _
    '        "Step : " & ErrStep & "<br>"
    '    HttpContext.Current.Response.Write(Hasil)
    '    HttpContext.Current.Response.Flush()
    '    'HttpContext.Current.Response.End()
    'End Sub

#End Region

#Region "Konversi Data"
    '//Konversi data - Jika datanya kosong, diganti dg data lainnya
    Public Function AsNz(ByVal zDataNya, ByVal Pengganti)
        On Error Resume Next
        If String.IsNullOrEmpty(zDataNya) Then
            Return Pengganti
        Else
            Return zDataNya
        End If
    End Function

    '//Cari tgl mulai dan terakhir pada minggu ini. OffsetX - Hari dimulainya minggu. 0=Minggu, 1=Senin, ...
    Public Function AsTglInfoMinggu(ByVal TglX As Date, Optional ByVal OffsetX As Long = 0, Optional ByVal StrFormatTgl As String = "dd MMMM yyyy", Optional ByVal StrPemisahTgl As String = " - ") As String
        On Error Resume Next
        Dim TglMulaiX As Date = Date.MinValue
        Dim TglTerakhirX As Date = Date.MinValue
        TglMulaiX = TglX.AddDays(OffsetX - Convert.ToDouble(TglX.DayOfWeek))
        TglTerakhirX = TglX.AddDays((OffsetX + 6) - Convert.ToDouble(TglX.DayOfWeek))

        Dim Hasil As String = AsFormatTanggal(TglMulaiX, StrFormatTgl) & StrPemisahTgl & AsFormatTanggal(TglTerakhirX, StrFormatTgl)
        Return Hasil
    End Function

    '//Konversi data - Menset nilai ketika data benar (sesuai kriteria) dan tidak
    Public Function AsBoolToYaTidak(ByVal Dt As String, ByVal KataTrue As String, ByVal KataFalse As String)
        Return IIf(Dt = 0, KataFalse, KataTrue)
    End Function
#End Region

#Region "Angka dan tanggal"
    '//Penformatan - Format tanggal
    Public Function AsFormatTanggal(ByVal Tanggal As Date, Optional ByVal FormatTanggal As String = "yyyy-MM-dd") As String
        On Error Resume Next
        Dim StrTgl As String = Tanggal.ToString(FormatTanggal)
        Dim DaftarBulan1 As String = "January~February~March~April~May~June~July~August~September~October~November~December~Jan~Feb~Mar~Apr~May~Jun~Jul~Aug~Sep~Oct~Nov~Dec"
        Dim DaftarBulan2 As String = "Januari~Februari~Maret~April~Mei~Juni~Juli~Agustus~September~Oktober~November~Desember~Jan~Feb~Mar~Apr~Mei~Jun~Jul~Agust~Sep~Okt~Nov~Des"
        Dim Dt1() As String = Split(DaftarBulan1, "~")
        Dim Dt2() As String = Split(DaftarBulan2, "~")

        For I As Integer = 0 To 23
            StrTgl = Replace(StrTgl, Dt1(I), Dt2(I))
        Next

        Return StrTgl
    End Function

    '//Penformatan - Format angka
    Public Function AsFormatAngka(ByVal Angka As Double, Optional ByVal FormatAngka As String = "#,##0") As String
        On Error Resume Next
        Dim StrAngka As String = String.Format("{0:" & FormatAngka & "}", Angka)

        StrAngka = Replace(StrAngka, ".", "-")
        StrAngka = Replace(StrAngka, ",", ".")
        StrAngka = Replace(StrAngka, "-", ",")

        Return StrAngka
    End Function

    '//Buat angka acak dari dg nilai range
    Public Function AsAngkaAcakDgRange(ByVal AngkaMinimal As Integer, ByVal AngkaMaksimal As Integer)
        Dim RandomGenerator As Random = New Random()
        Dim intRandomNumber As Integer = RandomGenerator.Next(AngkaMinimal, AngkaMaksimal + 1)
        Return intRandomNumber
    End Function
#End Region

    Public Function FxDB(ByVal Param As Object, ByVal DefaultVal As Object) As String
        If IsDBNull(Param) Then
            Return DefaultVal
        Else
            ''Cek jika Formattgl=true , maka format tgl
            'If FormatTgl Then
            '    Dim formattgl1 As String = "dd/MM/yyyy"
            '    Dim formattgl2 As String = "dd/MM/yyyy hh:mm:ss"

            '    'Jika tipe date, maka format tanggal
            '    If (IsDate(Param)) Then
            '        'Jika panjang param>10 maka ksh formattgl2
            '        Param = AsFormatTanggal(Param, formattgl1)
            '    End If
            'End If

            Return Param
        End If
    End Function

    Public Function M2_Accounting_PeriodeCheck(ByVal tglAwal As String, ByVal tglAkhir As String) As String
        On Error GoTo selesai
        Dim success As Integer = 0, errmessage As String = "", filter As String = ""

        'CEK TIPE DATA =============================================
        If (IsDate(tglAwal) = False) Then
            errmessage = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If
        If (IsDate(tglAkhir) = False) Then
            errmessage = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If
        'END OF CEK TIPE DATA ======================================

        'BUAT FILTER ===============================================
        '   'jika tahun berbeda
        If Not Year(tglAwal).Equals(Year(tglAkhir)) Then
            filter = "((aptahun = '" & Year(tglAwal) & "' AND apbulan >= '" & Month(tglAwal) & "') or (aptahun > '" & Year(tglAwal) & "' AND aptahun < '" & Year(tglAkhir) & "') or (aptahun = '" & Year(tglAkhir) & "' AND apbulan <= '" & Month(tglAkhir) & "'))"
            'jika tahun sama
        ElseIf Year(tglAwal).Equals(Year(tglAkhir)) Then
            '   'jika bulan sama
            If Month(tglAwal).Equals(Month(tglAkhir)) Then
                filter = "((aptahun = '" & Year(tglAwal) & "') AND (apbulan = '" & Month(tglAwal) & "'))"
                'jika bulan beda
            Else
                filter = "((aptahun = '" & Year(tglAwal) & "') AND (apbulan BETWEEN '" & Month(tglAwal) & "' AND '" & Month(tglAkhir) & "'))"
            End If
        End If
        'END OF BUAT FILTER ========================================


        'CEK PERIODE AKUNTANSI SUDAH TUTUP/BELUM
        Dim dt As DataTable = AsDataTableAmbilDariDB("SELECT aptahun, apbulan FROM m2_accounting_period WHERE " & filter & " AND aptutupperiode = '1'", strCon)
        If dt.Rows.Count > 0 Then success = 0 : errmessage = "Accounting Periode : Year = '" & dt.Rows(0)(0) & "', Month = '" & dt.Rows(0)(1) & "' has closed." : GoTo selesai

        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage)
    End Function

    '//FUNGSI UNTUK PEMBULATAN ANGKA DESIMAL
    Public Function F_Round(ByVal Number As Double) As Double

        Dim hasilDesimal As String = ""
        Dim StrNum As String = Number.ToString.Replace(",", ".")
        Dim sptAngka As String() = StrNum.Split(".")

        If sptAngka.Length > 1 Then
            Dim currNum As Integer = 0, prevNum As Integer = 0

            For i = 1 To sptAngka(1).Length
                If i = 1 Then
                    prevNum = 0
                Else
                    prevNum = Val(sptAngka(1).ElementAt(i - 2))
                End If
                currNum = Val(sptAngka(1).ElementAt(i - 1))

                If Val(prevNum) > 0 And Val(currNum) = 0 Then
                    Exit For
                End If

                hasilDesimal = String.Concat(hasilDesimal, currNum)
            Next
        End If

        hasilDesimal = String.Concat(sptAngka(0), ".", hasilDesimal)

        Return Double.Parse(hasilDesimal)
    End Function

    'FUNGSI UNTUK AMBIL SETTING
    Public Function F_getSetting(ByVal sModule As Integer, ByVal sGrup As String, ByVal sKode As String) As String
        Dim sql As String = "", hasil As String = ""

        sql = "SELECT snilai FROM m0_setting WHERE smodule = '" & sModule & "' AND sgrup = '" & sGrup & "' AND skode = '" & sKode & "'"
        Dim dtSetting As DataTable = AsDataTableAmbilDariDB(sql, strCon)
        If dtSetting.Rows.Count > 0 Then
            If Len(FxDB(dtSetting.Rows(0)("snilai"), "")) > 0 Then
                hasil = FxDB(dtSetting.Rows(0)("snilai"), "")
            End If
        End If

        Return hasil
    End Function

End Module

Public Class RsPaging
    Public isPaging, isNext, isPrev As Boolean
    Public curPage, prevPage, nextPage As String
    Public countPage, countRow As Integer
End Class