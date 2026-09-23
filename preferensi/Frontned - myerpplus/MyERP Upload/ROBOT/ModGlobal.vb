Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Management

Module ModGlobal

    Public LokasiLog As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\log.txt"
    Public ErrNumber As Long = 0, ErrStep As Long = 0, ErrDescription As String = "", ErrSource As String = "", ErrLine As Integer = 0, HasilSQL As String = ""
    Public strCon As String = "", strUrlWs As String = ""
    Public userid As String = "", AppPath As String = Replace(Application.StartupPath.ToString, "\report\config", "")

    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public sptLogin As String = "Θ"


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

#Region "FxDB"

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

    'FUNGSI UNTUK AMBIL SETTING
    Public Function F_getSetting(ByVal sModule As Integer, ByVal sGrup As String, ByVal sKode As String, ByVal strCon As String) As String
        Dim sql As String = "", hasil As String = ""

        Try
            sql = "SELECT snilai FROM m0_setting WHERE smodule = '" & sModule & "' AND sgrup = '" & sGrup & "' AND skode = '" & sKode & "'"
            Dim dtSetting As DataTable = AsDataTableAmbilDariDB(sql, strCon)
            If dtSetting.Rows.Count > 0 Then
                If Len(FxDB(dtSetting.Rows(0)("snilai"), "")) > 0 Then
                    hasil = FxDB(dtSetting.Rows(0)("snilai"), "")
                End If
            End If

        Catch ex As Exception
            MsgBox("F_getSetting : " & ex.Message)

        End Try

        Return hasil
    End Function

#End Region

#Region "Database"

    '//FUNGSI UNTUK BACA XML
    Public Function F_BacaXML(ByVal dataXML As String, ByVal tagXML As String) As String()
        Dim hasil(2) As String
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim rsDataXML() As String = dataXML.Replace("<" + tagXML + ">", "★").Split(CChar("★"))
        If rsDataXML.Length <> 2 Then
            hasil(0) = 0
            hasil(1) = "Tag : '" & tagXML & "' does not found in XML file."
            GoTo selesai

        Else
            hasil(0) = 1
            hasil(1) = rsDataXML(1).Replace("</" + tagXML + ">", "★").Split(CChar("★"))(0)
        End If

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK AMBIL NILAI TAG DARI APP.XML
    Public Function F_AppGetValue(ByVal tagXML As String) As String()
        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Try

            Dim myPath As String = AppPath + "\app\app.xml" 'HttpContext.Current.Server.MapPath("~/") + "app\app.xml"
            Dim sr As StreamReader
            Dim contents As String = ""

            sr = File.OpenText(myPath)
            contents = sr.ReadToEnd()
            sr.Close()

            Dim rsBacaXML() As String = F_BacaXML(contents, tagXML)
            hasil(0) = rsBacaXML(0)
            hasil(1) = rsBacaXML(1)

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = ex.Message

        End Try

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK AMBIL NILAI DARI CONSTR DATABASE DI APP.XML
    Public Function F_ConStrGetValue(ByVal ConStrTag As String, ByVal vStrCon As String) As String()
        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim conStr As String = "", currTag As String = ""
        Dim conStrSplit() As String, currTagSplit() As String
        'Dim AppValue() As String = F_AppGetValue("ConStr")
        Dim AppValue() As String = {"1", vStrCon}
        If AppValue(0) = 1 Then
            conStr = AppValue(1)
            conStrSplit = conStr.Split(";")
            For i As Integer = 0 To conStrSplit.Length - 1
                currTag = conStrSplit(i)
                currTagSplit = currTag.Split("=")
                If currTagSplit(0).ToLower.Equals(ConStrTag.ToLower) Then
                    If currTagSplit.Length <> 2 Then
                        hasil(0) = 0
                        hasil(1) = "Tag : '" & ConStrTag & "' does not found in ConStr file."
                        GoTo selesai

                    Else
                        hasil(0) = 1
                        hasil(1) = currTagSplit(1)
                        GoTo selesai

                    End If

                End If
            Next

        Else
            hasil(0) = AppValue(0)
            hasil(1) = AppValue(1)
            GoTo selesai

        End If

        hasil(0) = 0
        hasil(1) = "Tag : '" & ConStrTag & "' does not found in ConStr file."

selesai:
        Return hasil
    End Function

    '//FUNSI UNTUK AMBIL DIREKTORI PATH BERDASARKAN NAMA SERVICE
    Public Function F_GetServicePath(ByVal ServiceName As String) As String()
        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim result As String = ""
        Dim resultSplit As String()

        'AMBIL DIREKTORI SERVICE BERDASARKAN NAMA SERVICE
        Dim query As [String] = [String].Format("SELECT PathName FROM Win32_Service WHERE Name = '{0}'", ServiceName)

        'PERULANGAN SEBANYAK DATA YANG DITEMUKAN
        Using mos As New ManagementObjectSearcher(query)
            For Each mo As ManagementObject In mos.[Get]()
                'SET URL PATH SERVICE
                result = mo("PathName").ToString()
            Next
        End Using

        'PATH DISPLIT DENGAN TANDA PETIK DUA
        If Len(result) > 0 Then
            resultSplit = result.Split(Chr(34))

            'AMBIL DIREKTORI PATH TANPA NAMA FILE SERVICENYA
            If resultSplit.Length > 1 Then
                result = resultSplit(1).Substring(0, resultSplit(1).LastIndexOf("\") + 1)
            Else
                result = resultSplit(0).Substring(0, resultSplit(0).LastIndexOf("\") + 1)
            End If

            hasil(0) = 1
            hasil(1) = result
            GoTo selesai
        End If

        hasil(0) = 0
        hasil(1) = "Service Name : '" & ServiceName & "' does not found in services list."

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK DUMP DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_DumpSQLAsString(ByVal websiteAccessKey As String, ByVal userid As String, ByVal vStrCon As String) As String()
        ' Uses the mysqldump.exe program to make a backup of the database.
        ' This is an in-out stream operation.  The data is piped in via the 
        ' process's standard output and sent to a filestream to be written 
        ' to disk.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim strValue As New StringBuilder

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'DAFTAR TABEL YANG DI DUMP (tabel1 tabel2 tabel3 tabeldst)
            Dim strTable As String = "m0_role m0_role_custom m0_role_menu m0_role_report m0_selling_rate m0_setting m0_setting_location m0_user m0_user_branch m0_user_location m0_user_role m0_user_warehouse m1_area m1_bank m1_branch m1_coa m1_cogs_fifo_in m1_cogs_fifo_out m1_cogs_special_in m1_cogs_special_out m1_contact m1_contact_attention m1_contact_category m1_contact_point m1_cost_center m1_currency m1_customer_category m1_division m1_item m1_item_assembly m1_item_category m1_item_location m1_item_location_warehouse m1_item_stock_warehouse m1_item_type m1_location m1_no_batch_in m1_no_batch_out m1_no_batch_transaction m1_no_serial_in m1_no_serial_out m1_no_serial_transaction m1_project m1_region m1_salesman_category m1_selling_point m1_subdivision m1_supplier_category m1_tax m1_terms m1_unit m1_warehouse m1_type_sa m_12_area m_12_area_category m_12_pos_additional_item m_12_pos_additional_item_detail m_12_pos_bonus_item m_12_pos_bonus_item_detail m_12_pos_category m_12_pos_category_setting m_12_pos_discount_category_item m_12_pos_discount_item m_12_pos_item m_12_pos_point_category_item m_12_pos_point_item m_12_pos_point_transaction m_12_pos_setting m_12_pos_substitution_item m_12_pos_substitution_item_detail m_12_pos_voucher_in m_12_pos_voucher_out m1_item_permission"

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", vStrCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", vStrCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", vStrCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", vStrCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", vStrCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If


            'PROSES DUMP DATABASE
            Dim myProcess As Process = New Process()

            'Dim strOptions As String = String.Format(" --user={0} --password={1} --host={2} --port={3} --add-drop-database --add-drop-table --extended-insert --databases {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)
            Dim strOptions As String = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()

            End With


            Dim sOut As System.IO.StreamReader = myProcess.StandardOutput
            Dim line As String

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            strValue.AppendLine("SET foreign_key_checks = 0;SET UNIQUE_CHECKS = 0;SET AUTOCOMMIT = 0;")

            ' Read and display the lines from the file until the end 
            ' of the file is reached.
            Do
                line = sOut.ReadLine()
                strValue.AppendLine(line)
            Loop Until line Is Nothing

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            strValue.AppendLine("SET foreign_key_checks = 1;SET UNIQUE_CHECKS = 1;SET AUTOCOMMIT = 1;COMMIT;")

            sOut.Close()
            myProcess.Close()

            hasil(0) = 1
            hasil(1) = strValue.ToString

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Dump database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    '//FUNGSI UNTUK EXECUTE DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_ExecuteSQL(ByVal websiteAccessKey As String, ByVal userid As String, ByVal fileName As String, ByVal vStrCon As String) As String()
        ' Uses the mysqlimport.exe program to execute a backup of the database.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim filePath As String = AppPath + "\files\db\" 'HttpContext.Current.Server.MapPath("~/") & "files\db\"

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", vStrCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", vStrCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", vStrCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", vStrCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", vStrCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If

            'CEK FILE EXISTS
            If (Not File.Exists(filePath & fileName)) Then
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai
            End If

            'PROSES EXECUTE SQL
            Dim myProcess As New Process()
            myProcess.StartInfo.FileName = "cmd.exe"
            myProcess.StartInfo.UseShellExecute = False
            myProcess.StartInfo.CreateNoWindow = True
            myProcess.StartInfo.WorkingDirectory = pathServiceDB
            myProcess.StartInfo.RedirectStandardInput = True
            myProcess.StartInfo.RedirectStandardOutput = True
            myProcess.StartInfo.RedirectStandardError = True
            myProcess.Start()

            Dim myStreamWriter As StreamWriter = myProcess.StandardInput
            Dim mystreamreader As StreamReader = myProcess.StandardOutput
            myStreamWriter.WriteLine("mysql -u " & DBUser & " -p" & DBPassword & " " & DBDatabase & " < " & filePath & fileName & " ")
            myStreamWriter.Close()
            myProcess.WaitForExit()
            myProcess.Close()

            hasil(0) = 1
            hasil(1) = ""

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Execute database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

#End Region

End Module
