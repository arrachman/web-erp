Public Class m0_nomor
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    Public Function M0_Notransaksi(ByVal cabang As String, ByVal lokasi As String, ByVal kodetabel As String, ByVal tgl As String) As String
        On Error GoTo selesai

        Dim dt As DataTable
        Dim notransaksi As String = ""
        Dim awalan As String = ""
        Dim sqlambil As String = "", sql As String = ""
        Dim success As Integer = 0, jmldigit As Integer = 0, noberikutnya As Integer = 0
        Dim errmessage As String = ""

        'SET TAHUN
        Dim thn As String = Year(tgl).ToString.Substring(2, 2)
        'SET BULAN
        Dim bln As String = Month(tgl)

        'AMBIL KODE TRANSAKSI LOKASI
        sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
        dt = AsDataTableAmbilDariDB(sqlambil, strCon)
        If dt.Rows.Count > 0 Then
            lokasi = dt.Rows(0)("lkodetransaksi")
        Else
            errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
        End If

        'AMBIL AWALAN, JMLDIGIT, NOBERIKUTNYA BERDASARKAN KODETABEL, CABANG, LOKASI, TAHUN, BULAN
        sqlambil = "SELECT n.awalan, n.jmldigit, nb.noberikutnya FROM m0_nomor n JOIN m0_nomor_next nb ON n.kodetabel=nb.kodetabel WHERE n.kodetabel='" & kodetabel & "' AND nb.cabang='" & cabang & "' AND nb.lokasi='" & lokasi & "' AND nb.tahun='" & thn & "' AND nb.bulan='" & bln & "'"
        dt = AsDataTableAmbilDariDB(sqlambil, strCon)
        If (dt.Rows.Count > 0) Then
            awalan = dt.Rows(0)(0)
            jmldigit = Val(dt.Rows(0)(1))
            noberikutnya = Val(dt.Rows(0)(2))

            'SET SQL
            sql = "UPDATE M0_Nomor_Next SET noberikutnya = '" & noberikutnya + 1 & "' WHERE cabang='" & cabang & "' AND lokasi='" & lokasi & "' AND kodetabel='" & kodetabel & "' AND tahun='" & Val(thn) & "' AND bulan='" & Val(bln) & "'"

        Else
            'AMBIL AWALAN, JMLDIGIT BERDASARKAN KODETABEL
            sqlambil = "SELECT awalan, jmldigit FROM m0_nomor WHERE kodetabel='" & kodetabel & "'"
            dt = AsDataTableAmbilDariDB(sqlambil, strCon)
            If (dt.Rows.Count > 0) Then
                awalan = dt.Rows(0)(0)
                jmldigit = Val(dt.Rows(0)(1))
                noberikutnya = 1

                'SET SQL
                sql = "Insert into M0_Nomor_Next (cabang, lokasi, kodetabel, tahun, bulan, noberikutnya) values('" & cabang & "', '" & lokasi & "', '" & kodetabel & "', " & Val(thn) & ", " & Val(bln) & ", '" & 2 & "')"
            Else
                errmessage = "Could not find '" & kodetabel & "' in m0_nomor." : GoTo selesai
            End If
        End If

        'SET NOTRANSAKSI
        notransaksi = String.Concat(cabang, lokasi, awalan, thn)
        'SET BULAN NOTRANSAKSI
        If (bln.Length > 1) Then
            notransaksi = String.Concat(notransaksi, bln)
        Else
            notransaksi = String.Concat(notransaksi, "0", bln)
        End If

        'SET DIGIT NOTRANSAKSI
        Dim digit As String = noberikutnya.ToString
        For i As Integer = digit.Length + 1 To jmldigit
            digit = "0" & digit
        Next

        notransaksi = String.Concat(notransaksi, digit)
        'notransaksi = String.Concat(notransaksi, "-", digit)

        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, notransaksi, sptSubParam, sql)
    End Function

End Class