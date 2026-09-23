Public Class ClsCetak
    Public Header As New ClsCetakHeader
    Public Content As New ClsCetakContent
    Public Footer As New ClsCetakFooter


    Public Function generate() As String
        Dim hasil As String = ""

        hasil &= Content.generate
        hasil &= Header.generate

        Return hasil
    End Function
End Class

Public Class ClsCetakHeader
    Public width As Integer
    Public NamaToko As String = ""
    Public Alamat1 As String = "Jl. Kendalsari Barat Ruko Kav.3"
    Public Alamat2 As String = "Sukarno Hatta - Malang"


    Public Function generate() As String
        Dim hasil As String = ""
        'hasil &= vbCrLf
        ' Header (Nama Toko dan Alamat)
        'hasil &= NamaToko + vbCrLf
        hasil &= center(Alamat1) + vbCrLf
        hasil &= center(Alamat2) + vbCrLf
        'hasil &= vbCrLf
        Return hasil
    End Function

End Class

Public Class ClsCetakContent
    Private hasil_isi = "", jml_item As Double = 0, hasil As String = "", kembali As Double
    Public diskon As Double = 0, bayartunai As Double, bayar As Double = 0, kredit As Double = 0, debit As Double = 0, total As Double = 0, diskonperbarang As Double = 0, totalsebelumdiskon As Double = 0
    Public pelanggan As String = ""

    Dim time As DateTime = DateTime.Now
    Dim format As String = "d-MMM-yyyy HH:mm"
    'May Tue 18 16:46 2010
    Public tgl As String = time.ToString(Format)
    Public notransaksi As String = "IV140800004"
    Public User As String = ""
    Public transaksi_ke As String = "1"
    Public NoSlip As String = ""
    Public Kupon As Double = 0, Shu As Double = 0, Sukarela As Double = 0
    Public SaldoKupon As Double = 0, SaldoShu As Double = 0, SaldoSukarela As Double = 0
    Public NoKKredit As String = "", NoKDebit As String = "", custom1 As String = ""
    Public customtext1 As String = "", customtext2 As String = "", customtext3 As String = "", customtext4 As String = "", customtext5 As String = ""
    Public customint1 As Integer = 0, customint2 As Integer = 0, customint3 As Integer = 0, customint4 As Integer = 0, customint5 As Integer = 0
    Public customdbl1 As Double = 0, customdbl2 As Double = 0, customdbl3 As Double = 0, customdbl4 As Double = 0, customdbl5 As Double = 0


    Dim kolom_header(4) As String

    Public Sub isi(ByVal jmldiskon As Double, ByVal kode As String, ByVal nama As String, ByVal jml As Double, ByVal harga As Double, ByVal subtotal As Double)
        jml_item += jml
        total += subtotal
        diskonperbarang += jmldiskon
        Dim barang As String
        barang = kode & " - " & nama
        If barang.Length > 40 Then
            barang = barang.Substring(0, 39)
        End If
        hasil_isi &= mode_kolom_content(1, {barang}) + vbCrLf
        hasil_isi &= mode_kolom_content(4, {jml, uang(harga), uang(jmldiskon), uang(subtotal)}) + vbCrLf
    End Sub

    Public Function generate() As String
        Dim hasil As String = ""

        'Nomor Telepon
        'hasil &= center("(0356-711296)") + vbCrLf
        'hasil &= center("(031-7887771)") + vbCrLf

        hasil &= garis() + vbCrLf



        ' TGL dan Nomor Transaksi
        hasil &= left_rigth(tgl, notransaksi) + vbCrLf
        ' cetak ulang
        Dim copy As String = ""
        If NoSlip = 1 Then
            copy = "(COPY)"
        End If
        hasil &= left_rigth("Tanggal   : " + User, "") + vbCrLf
        hasil &= left_rigth("Kasir     : " + User, "" + copy) + vbCrLf

        ' Pelanggan
        Dim splitpelanggan As Array
        splitpelanggan = pelanggan.Split(" - ")
        hasil &= "Pelanggan : " + splitpelanggan(1) + vbCrLf
        hasil &= "No Member : " + splitpelanggan(0) + vbCrLf
        'point
        Dim totalpoint As Integer = customint1 + customint4 - customint2
        hasil &= "Point     : " + totalpoint + vbCrLf

        


        ' header barang
        Dim spasi As String = ""
        'Dim indexpertama As String
        'Dim namajml As String = "Brg"
        ''indexpertama = (21 - namajml.Length) / 2

        'For i = 0 To 9 - 1
        '    spasi &= " "
        'Next
        'namajml = "Brg" & spasi & "Qty"

        hasil &= garis() + vbCrLf
        hasil &= mode_kolom(4, {"Qty", "Harga", "Diskon", "Total"}) + vbCrLf
        hasil &= garis() + vbCrLf

        ' Detail barang
        hasil &= hasil_isi
        hasil &= garis() + vbCrLf

        ' Total Gross
        hasil &= mode_kolom_total(4, {" ", jml_item.ToString, "Item", uang(total)}) + vbCrLf

        'If diskonperbarang > 0 Then
        '    total = totalsebelumdiskon - diskonperbarang
        '    hasil &= mode_kolom_total(4, {" ", "", "Diskon", uang(diskonperbarang.ToString)}) + vbCrLf
        'End If
        ' Total Gross
        'hasil &= mode_kolom_total(4, {"", jml_item.ToString, "Item", uang(total)}) + vbCrLf

        If diskon > 0 Then
            total -= diskon
            hasil &= mode_kolom_total(4, {"", "", "Diskon", uang(diskon.ToString)}) + vbCrLf
        End If




        kembali = bayar - total

        hasil &= garis2() + vbCrLf
        hasil &= mode_kolom_total(4, {"", "", "Total", uang(total)}) + vbCrLf
        hasil &= vbCrLf
        hasil &= mode_kolom_total(4, {"", "", "Tunai", uang(bayartunai)}) + vbCrLf

        If kredit > 0 Then
            Dim detailkredit As Array = NoKKredit.Split("-")
            hasil &= mode_kolom_total(4, {detailkredit(1), "-", "Kredit", uang(kredit)}) + vbCrLf
            hasil &= mode_kolom_total(4, {detailkredit(0), "", "", ""}) + vbCrLf
        End If

        If debit > 0 Then
            'hasil &= mode_kolom_total(4, {NoKDebit, "-", "Debit", uang(debit)}) + vbCrLf
            Dim detaildebit As Array = NoKDebit.Split("-")
            hasil &= mode_kolom_total(4, {detaildebit(1), "-", "Debit", uang(debit)}) + vbCrLf
            hasil &= mode_kolom_total(4, {detaildebit(0), "", "", ""}) + vbCrLf
        End If

        If customint2 > 0 Then
            hasil &= mode_kolom_total(4, {"", "", "Point (" & customint2 & ")", uang(customdbl1)}) + vbCrLf
        End If

        If customint3 > 0 Then
            hasil &= mode_kolom_total(4, {"", "", "Point CC (" & customint3 & ")", uang(customdbl2)}) + vbCrLf
        End If

        'If NoSlip <> "" Then
        '    'If Kupon > 0 Then
        '    hasil &= mode_kolom_total(4, {"", "", "Kupon", uang(Kupon)}) + vbCrLf
        '    'End If

        '    'If Shu > 0 Then
        '    hasil &= mode_kolom_total(4, {"", "", "SHU", uang(Shu)}) + vbCrLf
        '    'End If


        '    'If Sukarela > 0 Then
        '    'hasil &= mode_kolom_total(4, {"", "", "Sukarela", uang(Sukarela)}) + vbCrLf
        '    'End If
        'End If

        hasil &= garis2() + vbCrLf

        hasil &= mode_kolom_total(4, {"", "", "Bayar", uang(bayar)}) + vbCrLf

        hasil &= mode_kolom_total(4, {"", "", "Kembali", uang(kembali)}) + vbCrLf
        hasil &= vbCrLf
        'sisa poin
        hasil &= mode_kolom_total(4, {"", "", "Point yg didapat", customint4}) + vbCrLf
        hasil &= mode_kolom_total(4, {"", "", "Point Sebelumnya", customint1}) + vbCrLf
        If (customint2 > 0) Then
            hasil &= mode_kolom_total(4, {"", "", "Pembayaran Point", customint2}) + vbCrLf
        End If

        hasil &= garis2() + vbCrLf
        hasil &= mode_kolom_total(4, {"", "", "Point terakhir  ", totalpoint}) + vbCrLf

        'If NoSlip <> "" Then
        '    hasil &= garis2() + vbCrLf
        '    'If SaldoKupon > 0 Then
        '    hasil &= mode_kolom_total(4, {"", "", "Sld Kupon", uang(SaldoKupon)}) + vbCrLf
        '    'End If

        '    'If SaldoShu > 0 Then
        '    hasil &= mode_kolom_total(4, {"", "", "Sld SHU", uang(SaldoShu)}) + vbCrLf
        '    'End If

        '    'If SaldoSukarela > 0 Then
        '    'hasil &= mode_kolom_total(4, {"", "", "Sld Sukarela", uang(SaldoSukarela)}) + vbCrLf
        '    'End If
        'End If


        'PPN
        hasil &= vbCrLf
        'hasil &= center("DPP : " & uang(Math.Round((total / 11) * 10)) & "   " & "PPn : " & uang(Math.Round(total / 11))) + vbCrLf

        'KATA PENUTUP
        'hasil &= center("Harga BKP sudah termasuk Pajak") + vbCrLf
        'hasil &= center("Barang tidak dapat dikembalikan/ditukar") + vbCrLf
        'hasil &= center("Terima Kasih atas kunjungannya") + vbCrLf
        hasil &= center("Terima kasih") + vbCrLf
        hasil &= center("Selamat datang kembali") + vbCrLf
        hasil &= center("CS : 081 210 888 115") + vbCrLf
        hasil &= center("www.truwear.id") + vbCrLf + vbCrLf + vbCrLf + vbCrLf + vbCrLf

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
    Dim width As Integer = 40
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

        hitungwidth = (width - panjangdata) / banyak_kolom - 1

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

    Function mode_kolom_content(ByVal banyak_kolom As Integer, ByVal data() As String)
        Dim hasil As String = "", spasi As String = "", hitungwidth As String
        Dim panjangdata As Integer = 0

        If pertama Then
            pertama = False
            mode_kolom(4, {"Nama Brg", "Jml", "Harga", "Total"})
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
        hasil = hasil
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
