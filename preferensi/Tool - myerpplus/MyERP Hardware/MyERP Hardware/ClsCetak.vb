Public Class ClsCetak
    Public Header As New ClsCetakHeader
    Public Content As New ClsCetakContent
    Public Footer As New ClsCetakFooter


    Public Function generate() As String
        Dim hasil As String = ""

        hasil &= Header.generate
        hasil &= Content.generate

        Return hasil
    End Function
End Class

Public Class ClsCetakHeader
    Public width As Integer
    Public NamaToko As String = "BRO BARBERSHOP - CAB. GADING"
    Public Alamat1 As String = "Ruko Pasar Modern Paramount"
    Public Alamat2 As String = "Blok A.26, Gading Serpong - Tangerang"


    Public Function generate() As String
        Dim hasil As String = ""
        'hasil &= vbCrLf
        ' Header (Nama Toko dan Alamat)
        ' hasil &= center(NamaToko) + vbCrLf
        hasil &= Alamat1 + vbCrLf + vbCrLf
        hasil &= Alamat2
        'hasil &= vbCrLf
        Return hasil
    End Function

End Class

Public Class ClsCetakContent
    Private hasil_isi = "", jml_item As Integer = 0, hasil As String = "", kembali As Double
    Public diskon As Double = 0, bayar As Double = 0, kredit As Double = 0, debit As Double = 0, total As Double = 0
    Public kupon As Double = 0, shu As Double = 0, sukarela As Double = 0
    Public pelanggan As String = ""

    Dim time As DateTime = DateTime.Now
    Dim format As String = "d-MMM-yyyy HH:mm"
    'May Tue 18 16:46 2010
    Public tgl As String = time.ToString(Format)
    Public notransaksi As String = "IV140800004"
    Public noslip As String = ""
    Public User As String = ""
    Public transaksi_ke As String = "1"
    Dim kolom_header(4) As String

    Public Sub isi(ByVal jmldiskon As Double, ByVal nama As String, ByVal jml As Integer, ByVal harga As Double, ByVal subtotal As Double)
        jml_item += jml
        total += subtotal
        hasil_isi &= mode_kolom_content(4, {jml, uang(harga), uang(jmldiskon), uang(subtotal)}, nama) + vbCrLf
    End Sub

    Public Function generate() As String
        Dim hasil As String = ""

        'Nomor Telepon
        'hasil &= center("(031-7882944)") + vbCrLf
        hasil &= center("") + vbCrLf

        hasil &= garis() + vbCrLf

        ' TGL
        hasil &= left_rigth(tgl, "") + vbCrLf

        ' notransaksi
        hasil &= left_rigth(notransaksi, "") + vbCrLf

        ' noslip
        hasil &= left_rigth(noslip, "") + vbCrLf

        'hasil &= left_rigth("User     : " + User, "#" + transaksi_ke) + vbCrLf
        hasil &= left_rigth("User     : " + User, "") + vbCrLf

        ' Pelanggan
        hasil &= "Pelanggan: " + pelanggan + vbCrLf

        hasil &= "No Slip: " + noslip + vbCrLf

        ' header barang
        hasil &= garis() + vbCrLf
        hasil &= mode_kolom(4, {"Brg/Jml", "Harga", "Diskon", "SubTotal"}) + vbCrLf
        hasil &= garis() + vbCrLf

        ' Detail barang
        hasil &= hasil_isi
        hasil &= garis() + vbCrLf

        ' Total
        hasil &= mode_kolom_total(4, {"", jml_item.ToString, "Item", uang(total)}) + vbCrLf

        If diskon > 0 Then
            total -= diskon
            hasil &= mode_kolom_total(4, {"", "", "Diskon", uang(diskon.ToString)}) + vbCrLf
        End If
        kembali = bayar - total

        hasil &= garis2() + vbCrLf
        hasil &= mode_kolom_total(4, {"", "", "Total", uang(total)}) + vbCrLf
        hasil &= vbCrLf
        hasil &= mode_kolom_total(4, {"", "", "Bayar", uang(bayar)}) + vbCrLf

        'update kupon, shu, sukarela
        'hasil &= mode_kolom_total(4, {"", "", "Kupon", uang(kupon)}) + vbCrLf
        'hasil &= mode_kolom_total(4, {"", "", "SHU", uang(shu)}) + vbCrLf
        'hasil &= mode_kolom_total(4, {"", "", "Sukarela", uang(sukarela)}) + vbCrLf


        If kredit > 0 Then
            hasil &= mode_kolom_total(4, {"", "", "Kredit", uang(kredit)}) + vbCrLf
        End If

        If debit > 0 Then
            hasil &= mode_kolom_total(4, {"", "", "Debit", uang(debit)}) + vbCrLf
        End If
        hasil &= mode_kolom_total(4, {"", "", "Kembali", uang(kembali)}) + vbCrLf

        'PPN
        hasil &= vbCrLf
        'hasil &= center("DPP : " & uang(Math.Round((total / 11) * 10)) & "   " & "PPn : " & uang(Math.Round(total / 11))) + vbCrLf
        'hasil &= center("") + vbCrLf

        'KATA PENUTUP
        hasil &= center("Dapur Kunang Kunang") + vbCrLf
        hasil &= center("Lebih unik lebih asyik") + vbCrLf
        hasil &= center("Terima kasih atas kunjungannya") + vbCrLf + vbCrLf
        hasil &= center("Dapur Kunang Kunang") + vbCrLf
        hasil &= center("Jl. Jaksa Agung Suprapto 1, Tulungagung") + vbCrLf
        hasil &= center("(Timur Alun-Alun T. Agung)") + vbCrLf

        Return hasil
    End Function

End Class


Public Class ClsCetakFooter
    Public Pesan1 As String = ""
    Public Pesan2 As String = ""
    Public Pesan3 As String = ""
End Class

Public Class jenis_cetak

End Class

Module fungsi_global

    Dim pertama As Boolean = True
    Dim width As Integer = 30
    Dim kolom_header(4) As String

    Function left_rigth(ByVal left_data As String, ByVal right_data As String)
        Dim spasi As String = "", hitungwidth As String
        hitungwidth = width - (left_data.Length + right_data.Length)
        For i = 0 To hitungwidth - 1
            spasi &= " "
        Next
        Return left_data + spasi + right_data
    End Function

    Function center(ByVal data As String)
        Dim spasi As String = "", hitungwidth As String
        hitungwidth = width - data.Length
        hitungwidth = hitungwidth / 2
        For i = 0 To hitungwidth - 1
            spasi &= " "
        Next
        Return spasi + data
    End Function

    Function mode_kolom(ByVal banyak_kolom As Integer, ByVal data() As String)
        Dim hasil As String = "", spasi As String = "", hitungwidth As String
        Dim panjangdata As Integer = 0

        If (data.Length <> banyak_kolom) Then
            MsgBox("banyak kolom dengan datanya tidak sama") : GoTo selesai
        End If

        For i = 0 To banyak_kolom - 1
            panjangdata += data(i).Length
        Next

        hitungwidth = (width - panjangdata) / banyak_kolom

        For i = 0 To hitungwidth - 1
            spasi &= " "
        Next

        For i = 0 To banyak_kolom - 1
            hasil += data(i)
            kolom_header(i) = data(i).Length.ToString
            If i < banyak_kolom - 1 Then
                hasil += spasi
            End If
        Next

        If hasil.Length < width Then
            panjangdata = width - hasil.Length
            panjangdata = panjangdata / (banyak_kolom - 1)

            For n = 0 To panjangdata - 1
                spasi += " "
            Next

            hasil = ""
            For i = 0 To banyak_kolom - 1
                hasil += data(i)
                kolom_header(i) = hasil.Length
                If i < banyak_kolom - 1 Then
                    hasil += spasi
                End If

            Next

        End If
selesai:
        Return hasil
    End Function

    Function mode_kolom_content(ByVal banyak_kolom As Integer, ByVal data() As String, ByVal nama As String)
        Dim hasil As String = "", spasi As String = "", hitungwidth As String
        Dim panjangdata As Integer = 0

        If pertama Then
            pertama = False
            mode_kolom(4, {"Kode Brg", "Jml", "Harga", "Sub Total"})
        End If

        If (data.Length <> banyak_kolom) Then
            MsgBox("banyak kolom dengan datanya tidak sama") : GoTo selesai
        End If

        For i = 0 To banyak_kolom - 1
            panjangdata += data(i).Length
        Next

        hitungwidth = (width - panjangdata) / banyak_kolom

        For i = 0 To hitungwidth - 1
            spasi &= " "
        Next

        For i = banyak_kolom - 1 To 0 Step -1
            hasil = data(i) + hasil

            If i > 0 Then
                If i = 1 Then
                    For n = 0 To width - hasil.Length - data(0).Length - 1
                        hasil = " " + hasil
                    Next
                Else
                    For n = 0 To (kolom_header(i) - kolom_header(i - 1)) - data(i).Length - 1
                        hasil = " " + hasil
                    Next
                End If
            End If
        Next
        If nama.Length > 30 Then
            hasil = nama.Substring(0, 29) + vbCrLf + hasil
            hasil = nama.Substring(30, nama.Length - 1) + vbCrLf + hasil
        Else

            hasil = nama + vbCrLf + hasil
        End If
selesai:
        Return hasil
    End Function

    Function mode_kolom_total(ByVal banyak_kolom As Integer, ByVal data() As String)
        Dim hasil As String = "", spasi As String = "", hitungwidth As String
        Dim panjangdata As Integer = 0

        If (data.Length <> banyak_kolom) Then
            MsgBox("banyak kolom dengan datanya tidak sama") : GoTo selesai
        End If

        For i = 0 To banyak_kolom - 1
            panjangdata += data(i).Length
        Next

        hitungwidth = (width - panjangdata) / banyak_kolom

        For i = 0 To hitungwidth - 1
            spasi &= " "
        Next

        For i = banyak_kolom - 1 To 0 Step -1
            If i <> 2 Then
                hasil = data(i) + hasil
            End If

            If i > 0 Then
                If i = 1 Then
                    For n = 0 To width - hasil.Length - data(0).Length - 1
                        hasil = " " + hasil
                    Next
                Else
                    For n = 0 To (kolom_header(i) - kolom_header(i - 1)) - data(i).Length - 1
                        hasil = " " + hasil
                    Next
                End If
            End If

            If i = 2 Then
                hasil = " " + data(i) + hasil.Substring(1, hasil.Length - 1)
            End If
        Next

selesai:
        Return hasil
    End Function

    Function garis2()
        Dim spasi As String = ""
        For i = 0 To width - 1
            If i < 6 Then
                spasi &= " "
            Else
                spasi &= "-"
            End If
        Next
        Return spasi
    End Function

    Function uang(ByVal data As Double)
        If data = 0 Then
            Return 0
        End If
        Return data.ToString("##,##,###")
    End Function

    Public Function garis()
        Dim spasi As String = ""
        For i = 0 To width - 1
            spasi &= "-"
        Next
        Return spasi
    End Function

End Module
