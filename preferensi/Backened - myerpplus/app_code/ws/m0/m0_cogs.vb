Imports System.Data
Imports AsModuleMySQL.CommonFunction

Public Class m0_cogs
    Inherits System.Web.Services.WebService

    Dim userid As String = ""   'User Id diisi dengan user yang melakukan proses transaksi
    Dim formatTglDB As String = "yyyy-MM-dd"
    Dim formatTglWaktuDB As String = "yyyy-MM-dd H:mm:ss"

	
    Public Function M0_CogsHitungUlang_FifoOptimasi(ByVal param As String) As String
      

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim isUpdate As Boolean, sql As String = ""
        Dim tglAwal As String = "", tglAkhir As String = ""
        Dim kodeBarangAwal As String = "", kodeBarangAkhir As String = ""
        Dim hitungPerBarang As Boolean = False, idbarang As Integer = 0

        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        Dim ProsesHpp As String = F_getSetting(0, "hppulang", "status")
        If ProsesHpp = "1" Then

            result(2) = "On Progress cogs" : GoTo selesai
        End If
        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, kodeBarangAwal(2) As String, kodeBarangAkhir(3) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, kodeBarangAwal, kodeBarangAkhir


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 4) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'tglAwal(0) As Date
        If (IsDate(dataUtama(0)) = False) Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(dataUtama(0))
        End If

        ''tglAkhir(1) As Date
        'If (IsDate(dataUtama(1)) = False) Then
        '    result(2) = "tglAkhir required date." : GoTo selesai
        'Else
        '    tglAkhir = AsFormatTanggal(dataUtama(1))
        'End If

        'kodeBarangAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            kodeBarangAwal = dataUtama(2)
        End If

        'kodeBarangAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            kodeBarangAkhir = dataUtama(3)
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'CEK HITUNG ULANG PERBARANG ATAU BUKAN -----------------------------
        'JIKA KODEBARANG AWAL ATAU KODEBARANG AKHIR DIISI MAKA HITUNG ULANG PERBARANG
        '-- ARITNYA, HITUNG ULANG PERBARANG MASIH BELUM BENAR, HPP BELUM FIX
        If Len(kodeBarangAwal) > 0 Or Len(kodeBarangAkhir) > 0 Then
            hitungPerBarang = True
        Else
            hitungPerBarang = False
        End If
        'END OF CEK HITUNG ULANG PERBARANG ATAU BUKAN ----------------------


        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            ''VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            'Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.id LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.id LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "  SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            sql &= " ORDER BY it.id LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "  SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            sql &= " ORDER BY it.id LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '    tglAwal = tgl
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '    tglAwal = tglHistory
            'End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            ''CEK PERIODE AKUNTANSI ---------------------------------------------
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAwal))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI --------------------------------------


            ''CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR ---------------------------
            ''-- TIDAK BOLEH LEBIH DARI BATAS TGL
            'Dim batasTgl As Double = 120
            'Dim jarakTgl As Double = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > batasTgl Then
            'result(2) = "Difference between Start Date and End Date should not be more than " & batasTgl & " days. (" & jarakTgl & " days)" : Trans.Rollback() : GoTo selesai
            'End If
            ''END OF CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR --------------------


            ''CEK TRANSAKSI SEBELUM TGL AWAL ------------------------------------ 
            ''-- APAKAH MASIH ADA TRANSAKSI YANG HARUS DIHITUNG ULANG
            ''BUAT QUERY
            'sql = "  SELECT DATE(postingtgl) as postingtgl"
            'sql &= " FROM M1_Item_Transaction"
            ''sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bhpp = 'F'"
            'sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bjenis <> 'V' AND bhpp = 'F'"
            'sql &= " JOIN M0_Nomor ON sumber = kodetabel AND transaksihpp = 1"
            'sql &= " WHERE hppfix = '0'"
            'sql &= " AND DATE(postingtgl) < '" & FixQuotes(tglAwal) & "'"
            ''FILTER KODEBARANG
            'If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
            '    sql &= " AND bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            'ElseIf Len(kodeBarangAwal) > 0 Then
            '    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
            '    sql &= " AND bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            'ElseIf Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
            '    sql &= " AND bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            'End If
            ''ORDER BY
            ''sql &= " ORDER BY postingtgl ASC, jenismutasi ASC, id ASC"
            'sql &= " ORDER BY id ASC"
            ''LIMIT
            'sql &= " LIMIT 1"

            'Dim dtCekTgl As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtCekTgl.Rows.Count > 0 Then
            '    result(2) = "Date " & AsFormatTanggal(dtCekTgl.Rows(0)("postingtgl")) & " must be recalculated first." : GoTo selesai
            'End If
            ''END OF CEK TRANSAKSI SEBELUM TGL AWAL -----------------------------


            ''UPDATE JMLKELUAR PADA HPP FIFO MASUK ------------------------------
            ''MENGURANGI JMLKELUAR PADA HPP FIFO MASUK SESUAI JMLKELUAR DARI HPP FIFO KELUAR
            ''DIMANA TGLINPUT HPP FIFO KELUAR >= TGLAWAL

            'sql = "  UPDATE m1_cogs_fifo_in cfi"
            'sql &= " JOIN"
            'sql &= " ("
            'sql &= " SELECT cfo.cfoidcfi, SUM(cfo.cfojmlkeluar) as jmlkeluar"
            'sql &= " FROM m1_cogs_fifo_out cfo"
            ''sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
            'sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
            'sql &= " WHERE DATE(cfo.cfoinputtgl) >= '" & FixQuotes(tglAwal) & "'"
            ''FILTER KODEBARANG
            'If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
            '    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            'ElseIf Len(kodeBarangAwal) > 0 Then
            '    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
            '    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            'ElseIf Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
            '    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            'End If
            'sql &= " GROUP BY cfo.cfoidcfi"
            'sql &= " ) as fifoOut"
            'sql &= " ON cfi.cfiid = fifoOut.cfoidcfi"
            'sql &= " SET cfi.cfijmlkeluar = cfi.cfijmlkeluar - fifoOut.jmlkeluar"

            ''TAMBAHKAN QUERY UPDATE HPP FIFO MASUK
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = Con1
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()
            ''END OF UPDATE JMLKELUAR PADA HPP FIFO MASUK -----------------------

            Dim hasilMsmq As String = SendMsmq(dirMsmq, "C", 0, "SUMBER", 0, userid)
            If Len(hasilMsmq) > 0 Then
                result(2) = hasilMsmq : GoTo selesai
            End If

            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = Con1
                .CommandType = CommandType.Text
                .CommandText = "UPDATE m0_setting SET snilai = 1 WHERE smodule = 0 AND sgrup = 'hppulang' AND skode = 'status'"
                .ExecuteNonQuery()
            End With

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------


        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try


        'END OF PROSES JURNAL ULANG ==================================================


selesai:


        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_FifoCek(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim isUpdate As Boolean, sql As String = ""
        Dim tglAwal As String = "", tglAkhir As String = ""
        Dim kodeBarangAwal As String = "", kodeBarangAkhir As String = ""
        Dim hitungPerBarang As Boolean = False, idbarang As Integer = 0

        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0


        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        Try
            Dim ProsesHpp As String = F_getSetting(0, "hppulang", "status")
                Dim mypath = HttpContext.Current.Server.MapPath("~/") & "report\config\LogHitungUlangHPPFIFO.txt"
            If ProsesHpp = "1" Then
                Try
                    If System.IO.File.Exists(mypath) Then
                        strResultData = System.IO.File.ReadAllText(mypath)
                    Else
                        strResultData = ""
                    End If
                Catch exc As Exception
                    result(2) = "Error read file. Desc : " & exc.Message & "." : GoTo selesai
                End Try
            ElseIf ProsesHpp = "3" Then
                Try
                    If System.IO.File.Exists(mypath) Then
                        strResultData = System.IO.File.ReadAllText(mypath)
                    Else
                        strResultData = ""
                    End If
                Catch exc As Exception
                    result(2) = "Informasi Hitung Ulang HPP." : GoTo selesai
                End Try
            Else 
			If System.IO.File.Exists(mypath) Then
				strResultData = System.IO.File.ReadAllText(mypath)
			Else
				strResultData = ""
			End If
                result(2) = "Hitung Ulang selesai" : GoTo selesai
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------


        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try


        'END OF PROSES JURNAL ULANG ==================================================


selesai:


        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function
	
    Public Function M0_CogsHitungUlang_FifoOld(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim isUpdate As Boolean, sql As String = ""
        Dim tglAwal As String = "", tglAkhir As String = ""
        Dim kodeBarangAwal As String = "", kodeBarangAkhir As String = ""
        Dim hitungPerBarang As Boolean = False, idbarang As Integer = 0

        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0


        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, kodeBarangAwal(2) As String, kodeBarangAkhir(3) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, kodeBarangAwal, kodeBarangAkhir


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 4) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'tglAwal(0) As Date
        If (IsDate(dataUtama(0)) = False) Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(dataUtama(0))
        End If

        ''tglAkhir(1) As Date
        'If (IsDate(dataUtama(1)) = False) Then
        '    result(2) = "tglAkhir required date." : GoTo selesai
        'Else
        '    tglAkhir = AsFormatTanggal(dataUtama(1))
        'End If

        'kodeBarangAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            kodeBarangAwal = dataUtama(2)
        End If

        'kodeBarangAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            kodeBarangAkhir = dataUtama(3)
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'CEK HITUNG ULANG PERBARANG ATAU BUKAN -----------------------------
        'JIKA KODEBARANG AWAL ATAU KODEBARANG AKHIR DIISI MAKA HITUNG ULANG PERBARANG
        '-- ARITNYA, HITUNG ULANG PERBARANG MASIH BELUM BENAR, HPP BELUM FIX
        If Len(kodeBarangAwal) > 0 Or Len(kodeBarangAkhir) > 0 Then
            hitungPerBarang = True
        Else
            hitungPerBarang = False
        End If
        'END OF CEK HITUNG ULANG PERBARANG ATAU BUKAN ----------------------


        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            ''VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            'Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.id LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.id LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "  SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            sql &= " ORDER BY it.id LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "  SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            sql &= " ORDER BY it.id LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '    tglAwal = tgl
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '    tglAwal = tglHistory
            'End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAwal))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            ''CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR ---------------------------
            ''-- TIDAK BOLEH LEBIH DARI BATAS TGL
            'Dim batasTgl As Double = 120
            'Dim jarakTgl As Double = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > batasTgl Then
            'result(2) = "Difference between Start Date and End Date should not be more than " & batasTgl & " days. (" & jarakTgl & " days)" : Trans.Rollback() : GoTo selesai
            'End If
            ''END OF CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR --------------------


            ''CEK TRANSAKSI SEBELUM TGL AWAL ------------------------------------ 
            ''-- APAKAH MASIH ADA TRANSAKSI YANG HARUS DIHITUNG ULANG
            ''BUAT QUERY
            'sql = "  SELECT DATE(postingtgl) as postingtgl"
            'sql &= " FROM M1_Item_Transaction"
            ''sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bhpp = 'F'"
            'sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bjenis <> 'V' AND bhpp = 'F'"
            'sql &= " JOIN M0_Nomor ON sumber = kodetabel AND transaksihpp = 1"
            'sql &= " WHERE hppfix = '0'"
            'sql &= " AND DATE(postingtgl) < '" & FixQuotes(tglAwal) & "'"
            ''FILTER KODEBARANG
            'If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
            '    sql &= " AND bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            'ElseIf Len(kodeBarangAwal) > 0 Then
            '    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
            '    sql &= " AND bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            'ElseIf Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
            '    sql &= " AND bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            'End If
            ''ORDER BY
            ''sql &= " ORDER BY postingtgl ASC, jenismutasi ASC, id ASC"
            'sql &= " ORDER BY id ASC"
            ''LIMIT
            'sql &= " LIMIT 1"

            'Dim dtCekTgl As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtCekTgl.Rows.Count > 0 Then
            '    result(2) = "Date " & AsFormatTanggal(dtCekTgl.Rows(0)("postingtgl")) & " must be recalculated first." : GoTo selesai
            'End If
            ''END OF CEK TRANSAKSI SEBELUM TGL AWAL -----------------------------


            ''UPDATE JMLKELUAR PADA HPP FIFO MASUK ------------------------------
            ''MENGURANGI JMLKELUAR PADA HPP FIFO MASUK SESUAI JMLKELUAR DARI HPP FIFO KELUAR
            ''DIMANA TGLINPUT HPP FIFO KELUAR >= TGLAWAL

            'sql = "  UPDATE m1_cogs_fifo_in cfi"
            'sql &= " JOIN"
            'sql &= " ("
            'sql &= " SELECT cfo.cfoidcfi, SUM(cfo.cfojmlkeluar) as jmlkeluar"
            'sql &= " FROM m1_cogs_fifo_out cfo"
            ''sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
            'sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
            'sql &= " WHERE DATE(cfo.cfoinputtgl) >= '" & FixQuotes(tglAwal) & "'"
            ''FILTER KODEBARANG
            'If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
            '    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            'ElseIf Len(kodeBarangAwal) > 0 Then
            '    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
            '    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            'ElseIf Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
            '    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            'End If
            'sql &= " GROUP BY cfo.cfoidcfi"
            'sql &= " ) as fifoOut"
            'sql &= " ON cfi.cfiid = fifoOut.cfoidcfi"
            'sql &= " SET cfi.cfijmlkeluar = cfi.cfijmlkeluar - fifoOut.jmlkeluar"

            ''TAMBAHKAN QUERY UPDATE HPP FIFO MASUK
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = Con1
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()
            ''END OF UPDATE JMLKELUAR PADA HPP FIFO MASUK -----------------------

            Dim myConn1 As MySql.Data.MySqlClient.MySqlConnection
            Dim Trans1 As MySql.Data.MySqlClient.MySqlTransaction
            Dim objCmd1 As MySql.Data.MySqlClient.MySqlCommand

            '*** Open Connection ***'  
            myConn1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
            myConn1.Open()

            '*** Start Transaction ***'  
            Trans1 = myConn1.BeginTransaction(IsolationLevel.ReadCommitted)

            Try

                'DELETE HPP FIFO KELUAR --------------------------------------------
                sql = "  DELETE cfo"
                sql &= " FROM m1_cogs_fifo_out cfo"
                'sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
                sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
                sql &= " WHERE DATE(cfo.cfoinputtgl) >= '" & FixQuotes(tglAwal) & "'"
                'FILTER KODEBARANG
                If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
                ElseIf Len(kodeBarangAwal) > 0 Then
                    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
                ElseIf Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
                End If
                'TAMBAHKAN QUERY DELETE HPP FIFO KELUAR
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()

                'INSERT KE TABEL LOG SUKSES
                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(0) & "', 'stepke : " & FixDouble(0) & ", delete fifo out', 2)"
                'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                'If AsEksekusiSQL(sql) = False Then
                '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                'End If
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()
                'END OF DELETE HPP FIFO KELUAR -------------------------------------


                'DELETE HPP FIFO MASUK ---------------------------------------------
                sql = "  DELETE cfi"
                sql &= " FROM m1_cogs_fifo_in cfi"
                'sql &= " JOIN M1_Item i ON cfi.cfiidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
                sql &= " JOIN M1_Item i ON cfi.cfiidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
                sql &= " WHERE DATE(cfi.cfiinputtgl) >= '" & FixQuotes(tglAwal) & "'"
                'FILTER KODEBARANG
                If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
                ElseIf Len(kodeBarangAwal) > 0 Then
                    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
                ElseIf Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
                End If
                'TAMBAHKAN QUERY DELETE HPP FIFO MASUK
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()

                'INSERT KE TABEL LOG SUKSES
                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(0) & "', 'stepke : " & FixDouble(0) & ", delete fifo in', 2)"
                'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                'If AsEksekusiSQL(sql) = False Then
                '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                'End If
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()
                'END OF DELETE HPP FIFO MASUK --------------------------------------

                Trans1.Commit()  '*** Commit Transaction ***'

            Catch ex As Exception

                Trans1.Rollback() '*** RollBack Transaction ***'  
                result(1) = 0
                result(2) = ex.Message
                result(3) = 0
                result(4) = result(4)
                GoTo selesai

            Finally
                myConn1.Close()

            End Try


            'HITUNG ULANG TRANSAKSI BARANG -------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode, it.tipebarang, it.namabarang, it.satuanbarang, it.saldojml, it.saldohpp, it.saldonilai, it.postingtgl "
            sql &= " FROM M1_Item_Transaction it"
            'sql &= " JOIN M1_Item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
            sql &= " JOIN M1_Item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
            sql &= " JOIN M0_Nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
            sql &= " WHERE it.tgl >= '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            'ORDER BY
            'sql &= " ORDER BY it.postingtgl ASC, it.jenismutasi ASC, it.id ASC"
            'sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            sql &= " ORDER BY it.id "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                Dim strFifo As New StringBuilder, strIdHppFifo As New StringBuilder

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = "", jmlbarang As Double = 0
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0, postingtgl As String = ""
                Dim satuanbarang As String = "", tipebarang As String = "", namabarang As String = ""

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable, dtCekFifo As New DataTable, sisa As Double = 0, dtFifo As New DataTable

                Dim myConn2 As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                Dim drBarang As DataRow

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    'If i = 532 Then result(2) = "xx" : GoTo selesai

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn2.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn2.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))
                        postingtgl = AsFormatTanggal(FxDB(drBarang("postingtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        jmlbarang = Double.Parse(FxDB(drBarang("jmlbarang"), 0))
                        satuanbarang = FxDB(drBarang("satuanbarang"), "")
                        tipebarang = FxDB(drBarang("tipebarang"), "")
                        namabarang = FxDB(drBarang("namabarang"), "")

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                        sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.id <= '" & FixQuotes(id) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        'sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        sqlSAwal &= " ORDER BY it.id "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        If jenismutasi = 1 And sumber = "PD" Then
                            'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                            sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                            sql &= " FROM m6_pd_in pdi "
                            sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                            sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                            sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                            sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY pdi.idpdin "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SI" Then
                            'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                            sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                            sql &= " FROM m5_si_detail sid "
                            sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                            sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                            sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY sid.idsidetail "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SR" Then
                            'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_sr_detail srd "
                            sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                            sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                            sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK

                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                   cfitipebarang,                      cfisatuan,                      cfijmlmasuk, cfijmlkeluar,              cfisisa,            cfiharga,  cfiisclose,               cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hppmasuk & "', " & 0 & ", '" & FixQuotes(postingtgl) & "')")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn2
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd2.ExecuteNonQuery()

                            'saldonilai = (saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            'JIKA BARANG KELUAR

                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dtCekFifo = AsDataTableAmbilDariDB("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'")
                            If dtCekFifo.Rows.Count > 0 Then
                                sisa = Double.Parse(dtCekFifo(0)("cfisisa"))
                                If jmlbarang > sisa Then
                                    result(2) = "Row : " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa & " " & satuanbarang : Trans2.Rollback() : GoTo selesai
                                End If
                            Else
                                result(2) = "Row :" & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans2.Rollback() : GoTo selesai
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDB("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0")
                            dtFifo = AsDataTableAmbilDariDB("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")")
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hppkeluar = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                          cfoharga,    cfoisclose,            cfoidcfi,                    cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", '" & FixQuotes(postingtgl) & "')")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn2
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn2
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()
                                Next

                            Else
                                result(2) = "Row : " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans2.Rollback() : GoTo selesai
                            End If

                            'saldonilai = (saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                            HppTrans = hppmasuk

                        Else
                            'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                            HppTrans = hppkeluar

                        End If

                        'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        Select Case sumber.ToUpper
                            Case "SA"
                                sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "IB"
                                sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "GRN"
                                sql = ""

                            Case "RI"
                                sql = ""

                            Case "PRT"
                                sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SI"
                                'SI ADA BARANG ASSEMBLY LANGSUNG
                                If jenismutasi = 0 And customint10 = -2 Then
                                    'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 1 And customint10 = -1 Then
                                    'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 0 And customint10 = 0 Then
                                    'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'sql = ""
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "SR"
                                sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "PD"
                                'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                If jenismutasi = 1 Then
                                    'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                    sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                    sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "LU"
                                sql = ""

                            Case "LB"
                                sql = ""

                            Case "AK"
                                sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case "RO"
                                sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case Else
                                sql = ""
                        End Select

                        'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        If Len(sql) > 0 Then
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn2
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd2.ExecuteNonQuery()
                        End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        sql &= " , it.jurnalfix = '0' "
                        sql &= " , it.updatehpp = '1' "
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                            sql &= " , it.hppfix = '1' "
                        End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ''AMBILSALDO AKHIR
                        'sql = "  SELECT it.id "
                        'sql &= " FROM m1_item_transaction it "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        'sql &= " LIMIT 1"
                        'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        'If dtSaldoAkhir.Rows.Count > 0 Then
                        '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 10

                        'UPDATE HISTORI TRANSAKSI BARANG
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                            'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                            If tglBefore <> tgl Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            ElseIf stepKe >= dtBarang.Rows.Count Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            Else
                                sql = ""
                            End If
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn2
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If

                            'PERBARUI TGL BEFORE
                            tglBefore = tgl
                        End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPAVERAGE
                        sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', '" & i & " from " & dtBarang.Rows.Count - 1 & " stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        'result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn2.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------


        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try


        'END OF PROSES JURNAL ULANG ==================================================


selesai:


        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_Fifo(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim isUpdate As Boolean, sql As String = ""
        Dim tglAwal As String = "", tglAkhir As String = ""
        Dim kodeBarangAwal As String = "", kodeBarangAkhir As String = ""
        Dim hitungPerBarang As Boolean = False, idbarang As Integer = 0

        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0


        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, kodeBarangAwal(2) As String, kodeBarangAkhir(3) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, kodeBarangAwal, kodeBarangAkhir


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 4) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'tglAwal(0) As Date
        If (IsDate(dataUtama(0)) = False) Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(dataUtama(0))
        End If

        ''tglAkhir(1) As Date
        'If (IsDate(dataUtama(1)) = False) Then
        '    result(2) = "tglAkhir required date." : GoTo selesai
        'Else
        '    tglAkhir = AsFormatTanggal(dataUtama(1))
        'End If

        'kodeBarangAwal(2) As String
        If Len(dataUtama(2)) > 0 Then
            kodeBarangAwal = dataUtama(2)
        End If

        'kodeBarangAkhir(3) As String
        If Len(dataUtama(3)) > 0 Then
            kodeBarangAkhir = dataUtama(3)
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'CEK HITUNG ULANG PERBARANG ATAU BUKAN -----------------------------
        'JIKA KODEBARANG AWAL ATAU KODEBARANG AKHIR DIISI MAKA HITUNG ULANG PERBARANG
        '-- ARITNYA, HITUNG ULANG PERBARANG MASIH BELUM BENAR, HPP BELUM FIX
        If Len(kodeBarangAwal) > 0 Or Len(kodeBarangAkhir) > 0 Then
            hitungPerBarang = True
        Else
            hitungPerBarang = False
        End If
        'END OF CEK HITUNG ULANG PERBARANG ATAU BUKAN ----------------------


        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            ''VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            'Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.id LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.id LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "  SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            sql &= " ORDER BY it.id LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "  SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            sql &= " ORDER BY it.id LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '    tglAwal = tgl
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '    tglAwal = tglHistory
            'End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            ''CEK PERIODE AKUNTANSI ---------------------------------------------
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAwal))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI --------------------------------------


            ''CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR ---------------------------
            ''-- TIDAK BOLEH LEBIH DARI BATAS TGL
            'Dim batasTgl As Double = 120
            'Dim jarakTgl As Double = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > batasTgl Then
            'result(2) = "Difference between Start Date and End Date should not be more than " & batasTgl & " days. (" & jarakTgl & " days)" : Trans.Rollback() : GoTo selesai
            'End If
            ''END OF CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR --------------------


            ''CEK TRANSAKSI SEBELUM TGL AWAL ------------------------------------ 
            ''-- APAKAH MASIH ADA TRANSAKSI YANG HARUS DIHITUNG ULANG
            ''BUAT QUERY
            'sql = "  SELECT DATE(postingtgl) as postingtgl"
            'sql &= " FROM M1_Item_Transaction"
            ''sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bhpp = 'F'"
            'sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bjenis <> 'V' AND bhpp = 'F'"
            'sql &= " JOIN M0_Nomor ON sumber = kodetabel AND transaksihpp = 1"
            'sql &= " WHERE hppfix = '0'"
            'sql &= " AND DATE(postingtgl) < '" & FixQuotes(tglAwal) & "'"
            ''FILTER KODEBARANG
            'If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
            '    sql &= " AND bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            'ElseIf Len(kodeBarangAwal) > 0 Then
            '    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
            '    sql &= " AND bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            'ElseIf Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
            '    sql &= " AND bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            'End If
            ''ORDER BY
            ''sql &= " ORDER BY postingtgl ASC, jenismutasi ASC, id ASC"
            'sql &= " ORDER BY id ASC"
            ''LIMIT
            'sql &= " LIMIT 1"

            'Dim dtCekTgl As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtCekTgl.Rows.Count > 0 Then
            '    result(2) = "Date " & AsFormatTanggal(dtCekTgl.Rows(0)("postingtgl")) & " must be recalculated first." : GoTo selesai
            'End If
            ''END OF CEK TRANSAKSI SEBELUM TGL AWAL -----------------------------


            ''UPDATE JMLKELUAR PADA HPP FIFO MASUK ------------------------------
            ''MENGURANGI JMLKELUAR PADA HPP FIFO MASUK SESUAI JMLKELUAR DARI HPP FIFO KELUAR
            ''DIMANA TGLINPUT HPP FIFO KELUAR >= TGLAWAL

            'sql = "  UPDATE m1_cogs_fifo_in cfi"
            'sql &= " JOIN"
            'sql &= " ("
            'sql &= " SELECT cfo.cfoidcfi, SUM(cfo.cfojmlkeluar) as jmlkeluar"
            'sql &= " FROM m1_cogs_fifo_out cfo"
            ''sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
            'sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
            'sql &= " WHERE DATE(cfo.cfoinputtgl) >= '" & FixQuotes(tglAwal) & "'"
            ''FILTER KODEBARANG
            'If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
            '    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            'ElseIf Len(kodeBarangAwal) > 0 Then
            '    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
            '    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            'ElseIf Len(kodeBarangAkhir) > 0 Then
            '    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
            '    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            'End If
            'sql &= " GROUP BY cfo.cfoidcfi"
            'sql &= " ) as fifoOut"
            'sql &= " ON cfi.cfiid = fifoOut.cfoidcfi"
            'sql &= " SET cfi.cfijmlkeluar = cfi.cfijmlkeluar - fifoOut.jmlkeluar"

            ''TAMBAHKAN QUERY UPDATE HPP FIFO MASUK
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = Con1
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sql
            'End With
            'objCmd.ExecuteNonQuery()
            ''END OF UPDATE JMLKELUAR PADA HPP FIFO MASUK -----------------------

            Dim myConn1 As MySql.Data.MySqlClient.MySqlConnection
            Dim Trans1 As MySql.Data.MySqlClient.MySqlTransaction
            Dim objCmd1 As MySql.Data.MySqlClient.MySqlCommand

            '*** Open Connection ***'  
            myConn1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
            myConn1.Open()

            '*** Start Transaction ***'  
            Trans1 = myConn1.BeginTransaction(IsolationLevel.ReadCommitted)

            Try

                'DELETE HPP FIFO KELUAR --------------------------------------------
                sql = "  DELETE cfo"
                sql &= " FROM m1_cogs_fifo_out cfo"
                'sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
                sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
                'sql &= " WHERE DATE(cfo.cfoinputtgl) >= '" & FixQuotes(tglAwal) & "'"
                sql &= " WHERE DATE(cfo.cfoinputtgl) LIKE '%' "
                'FILTER KODEBARANG
                If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
                ElseIf Len(kodeBarangAwal) > 0 Then
                    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
                ElseIf Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
                End If
                'TAMBAHKAN QUERY DELETE HPP FIFO KELUAR
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()

                'INSERT KE TABEL LOG SUKSES
                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(0) & "', 'stepke : " & FixDouble(0) & ", delete fifo out', 2)"
                'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                'If AsEksekusiSQL(sql) = False Then
                '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                'End If
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()
                'END OF DELETE HPP FIFO KELUAR -------------------------------------


                'DELETE HPP FIFO MASUK ---------------------------------------------
                sql = "  DELETE cfi"
                sql &= " FROM m1_cogs_fifo_in cfi"
                'sql &= " JOIN M1_Item i ON cfi.cfiidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
                sql &= " JOIN M1_Item i ON cfi.cfiidbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
                'sql &= " WHERE DATE(cfi.cfiinputtgl) >= '" & FixQuotes(tglAwal) & "'"
                sql &= " WHERE DATE(cfi.cfiinputtgl) LIKE '%'"
                'FILTER KODEBARANG
                If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
                ElseIf Len(kodeBarangAwal) > 0 Then
                    'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                    sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
                ElseIf Len(kodeBarangAkhir) > 0 Then
                    'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                    sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
                End If
                'TAMBAHKAN QUERY DELETE HPP FIFO MASUK
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()

                'INSERT KE TABEL LOG SUKSES
                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(0) & "', 'stepke : " & FixDouble(0) & ", delete fifo in', 2)"
                'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                'If AsEksekusiSQL(sql) = False Then
                '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                'End If
                objCmd1 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd1
                    .Connection = myConn1
                    .Transaction = Trans1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd1.ExecuteNonQuery()
                'END OF DELETE HPP FIFO MASUK --------------------------------------

                Trans1.Commit()  '*** Commit Transaction ***'

            Catch ex As Exception

                Trans1.Rollback() '*** RollBack Transaction ***'  
                result(1) = 0
                result(2) = ex.Message
                result(3) = 0
                result(4) = result(4)
                GoTo selesai

            Finally
                myConn1.Close()

            End Try


            'HITUNG ULANG TRANSAKSI BARANG -------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode, it.tipebarang, it.namabarang, it.satuanbarang, it.saldojml, it.saldohpp, it.saldonilai, it.postingtgl "
            sql &= " FROM M1_Item_Transaction it"
            'sql &= " JOIN M1_Item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
            sql &= " JOIN M1_Item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'F'"
            sql &= " JOIN M0_Nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
            'sql &= " WHERE it.tgl >= '" & tglAwal & "' "
            sql &= " WHERE it.tgl LIKE '%' "
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND i.bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND i.bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND i.bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            'ORDER BY
            'sql &= " ORDER BY it.postingtgl ASC, it.jenismutasi ASC, it.id ASC"
            'sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            sql &= " ORDER BY it.id "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                Dim strFifo As New StringBuilder, strIdHppFifo As New StringBuilder

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = "", jmlbarang As Double = 0
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0, postingtgl As String = ""
                Dim satuanbarang As String = "", tipebarang As String = "", namabarang As String = ""

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable, dtCekFifo As New DataTable, sisa As Double = 0, dtFifo As New DataTable

                Dim myConn2 As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                Dim drBarang As DataRow

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    'If i = 532 Then result(2) = "xx" : GoTo selesai

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn2.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn2.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))
                        postingtgl = AsFormatTanggal(FxDB(drBarang("postingtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        jmlbarang = Double.Parse(FxDB(drBarang("jmlbarang"), 0))
                        satuanbarang = FxDB(drBarang("satuanbarang"), "")
                        tipebarang = FxDB(drBarang("tipebarang"), "")
                        namabarang = FxDB(drBarang("namabarang"), "")

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                        sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.id <= '" & FixQuotes(id) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        'sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        sqlSAwal &= " ORDER BY it.id "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        If jenismutasi = 1 And sumber = "PD" Then
                            'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                            sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                            sql &= " FROM m6_pd_in pdi "
                            sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                            sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                            sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                            sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY pdi.idpdin "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SI" Then
                            'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                            sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                            sql &= " FROM m5_si_detail sid "
                            sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                            sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                            sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY sid.idsidetail "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SR" Then
                            'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_sr_detail srd "
                            sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                            sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                            sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "RNR" Then
                            'JIKA rnr AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_rnr_detail rnrd "
                            sql &= " JOIN m5_si_detail sid ON rnrd.idsidetail = sid.idsidetail "
                            sql &= " AND rnrd.idrnr = '" & FixDouble(idutama) & "'"
                            sql &= " AND rnrd.idrnrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND rnrd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK

                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                   cfitipebarang,                      cfisatuan,                      cfijmlmasuk, cfijmlkeluar,              cfisisa,            cfiharga,  cfiisclose,               cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hppmasuk & "', " & 0 & ", '" & FixQuotes(postingtgl) & "')")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn2
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd2.ExecuteNonQuery()

                            'saldonilai = (saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            'JIKA BARANG KELUAR

                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dtCekFifo = AsDataTableAmbilDariDB("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'")
                            If dtCekFifo.Rows.Count > 0 Then
                                sisa = Double.Parse(dtCekFifo(0)("cfisisa"))
                                If jmlbarang > sisa Then
                                    result(2) = "Row : " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa & " " & satuanbarang : Trans2.Rollback() : GoTo selesai
                                End If
                            Else
                                result(2) = "Row :" & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans2.Rollback() : GoTo selesai
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDB("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0")
                            dtFifo = AsDataTableAmbilDariDB("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")")
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hppkeluar = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                          cfoharga,    cfoisclose,            cfoidcfi,                    cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", '" & FixQuotes(postingtgl) & "')")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn2
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "' WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn2
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()
                                Next

                            Else
                                result(2) = "Row : " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans2.Rollback() : GoTo selesai
                            End If

                            'saldonilai = (saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                            HppTrans = hppmasuk

                        Else
                            'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                            HppTrans = hppkeluar

                        End If

                        'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        If tgl >= tglAwal Then
                            Select Case sumber.ToUpper
                                Case "SA"
                                    sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Case "IB"
                                    sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Case "GRN"
                                    sql = ""

                                Case "RI"
                                    sql = ""

                                Case "PRT"
                                    sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Case "SI"
                                    'SI ADA BARANG ASSEMBLY LANGSUNG
                                    If jenismutasi = 0 And customint10 = -2 Then
                                        'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                        sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    ElseIf jenismutasi = 1 And customint10 = -1 Then
                                        'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                        sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    ElseIf jenismutasi = 0 And customint10 = 0 Then
                                        'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                        sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Else
                                        'sql = ""
                                        sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    End If

                                Case "RNR"
                                    sql = "UPDATE m5_rnr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idrnr = '" & FixDouble(idutama) & "' AND idrnrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Case "SR"
                                    sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Case "PD"
                                    'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                    If jenismutasi = 1 Then
                                        'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                        sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Else
                                        'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                        sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    End If

                                Case "LU"
                                    sql = ""

                                Case "LB"
                                    sql = ""

                                Case "AK"
                                    sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                                Case "RO"
                                    sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                                Case Else
                                    sql = ""
                            End Select

                            'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn2
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If
                        End If


                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        If tgl >= tglAwal Then
                            sql = "  UPDATE m1_item_transaction it "
                            sql &= " SET "
                            sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                            sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                            sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                            sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                            sql &= " , it.jurnalfix = '0' "
                            sql &= " , it.updatehpp = '1' "
                            If hitungPerBarang = False Then
                                'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                                sql &= " , it.hppfix = '1' "
                            End If
                            sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn2
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd2.ExecuteNonQuery()
                        End If


                        'STEP DETAIL
                        stepDetail = 9

                        ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ''AMBILSALDO AKHIR
                        'sql = "  SELECT it.id "
                        'sql &= " FROM m1_item_transaction it "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        'sql &= " LIMIT 1"
                        'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        'If dtSaldoAkhir.Rows.Count > 0 Then
                        '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 10

                        'UPDATE HISTORI TRANSAKSI BARANG
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                            'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                            If tglBefore <> tgl Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            ElseIf stepKe >= dtBarang.Rows.Count Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            Else
                                sql = ""
                            End If
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn2
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If

                            'PERBARUI TGL BEFORE
                            tglBefore = tgl
                        End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPAVERAGE
                        sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', '" & i & " from " & dtBarang.Rows.Count - 1 & " stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        'result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn2.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------


        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try


        'END OF PROSES JURNAL ULANG ==================================================


selesai:


        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_Average1Commit(ByVal param As String) As String

        Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        idbarang = dataUtama(2)
        If (IsNumeric(idbarang) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            If jarakTgl > 120 Then
                result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                    tglAwal = tgl
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                    tglAwal = tglHistory
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                tglAwal = tgl
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                tglAwal = tglHistory
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAwal))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= "AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AWAL
                Dim dtSaldoAkhir As New DataTable

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For Each drBarang As DataRow In dtBarang.Rows

                    'STEPKE
                    stepKe = stepKe + 1

                    'RESET NILAI VARIABEL SALDO HASIL HITUNG
                    saldojml = 0 : saldohpp = 0 : saldonilai = 0

                    'SET DATA BARANG
                    id = Integer.Parse(FxDB(drBarang("id"), 0))
                    idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                    kodebarang = FxDB(drBarang("bkode"), "")
                    jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                    tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                    inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                    sumber = FxDB(drBarang("sumber"), "")
                    notransaksi = FxDB(drBarang("notransaksi"), "")
                    idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                    iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                    customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                    'SET SALDO YANG DIHITUNG
                    If jenismutasi = 1 Then
                        'JIKA BARANG MASUK
                        jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                        hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                        nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0

                    Else
                        'JIKA BARANG KELUAR
                        jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                        hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                        nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0

                    End If

                    'AMBIL SALDO AWAL
                    sql = "  SELECT it.id, it.saldojml, it.saldohpp, it.saldonilai"
                    sql &= " FROM m1_item_transaction it "
                    'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                    sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                    sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                    sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                    sql &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                    sql &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                    sql &= " AND (CASE "
                    sql &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                    sql &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                    sql &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                    sql &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                    sql &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                    sql &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                    sql &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                    sql &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                    sql &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                    sql &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                    sql &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                    sql &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                    sql &= " ELSE it.id LIKE '%' "
                    sql &= " END) "
                    sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                    sql &= " LIMIT 1"

                    dtSaldo = AsDataTableAmbilDariDB(sql)
                    If dtSaldo.Rows.Count > 0 Then
                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If Len(FxDB(dtSaldo.Rows(0)("id"), "")) > 0 Then
                            saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(0)("saldojml"), 0)), 2)
                            saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(0)("saldohpp"), 0)), 2)
                            saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(0)("saldonilai"), 0)), 2)

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                            GoTo setSaldoAwalNol

                        End If

                    Else
                        'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                        saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                    End If

                    'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                    'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                    If jenismutasi = 1 And sumber = "PD" Then
                        'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                        sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                        sql &= " FROM m6_pd_in pdi "
                        sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                        sql &= " WHERE pdi.idpdin = '" & FixDouble(iddetail) & "'"
                        sql &= " GROUP BY pdi.idpdin "
                        dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        If dtHppMasukSpesial.Rows.Count > 0 Then
                            If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                            Else
                                hppmasuk = 0
                            End If
                        End If

                    ElseIf jenismutasi = 1 And sumber = "SI" Then
                        'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                        sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                        sql &= " FROM m5_si_detail sid "
                        sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                        sql &= " WHERE sid.idsidetail = '" & FixDouble(iddetail) & "'"
                        sql &= " GROUP BY sid.idsidetail "
                        dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        If dtHppMasukSpesial.Rows.Count > 0 Then
                            If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                            Else
                                hppmasuk = 0
                            End If
                        End If

                    ElseIf jenismutasi = 1 And sumber = "SR" Then
                        'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                        sql = "  SELECT sid.hpp as hpp "
                        sql &= " FROM m5_sr_detail srd "
                        sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                        sql &= " WHERE srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                        dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        If dtHppMasukSpesial.Rows.Count > 0 Then
                            If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                            Else
                                hppmasuk = 0
                            End If
                        End If

                    End If


                    'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                    If jenismutasi = 1 Then
                        'JIKA BARANG MASUK
                        'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                        saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)

                    Else
                        'JIKA BARANG KELUAR
                        If sumber <> "PRT" Then
                            'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                            hppkeluar = Math.Round(saldoawalhpp, 2)

                        End If

                        'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                        saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)

                    End If

                    'HITUNG SALDOJML
                    saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)

                    'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                    If saldojml <> 0 Then
                        saldonilai = Math.Round(saldonilai, 2)
                        saldohpp = Math.Round(saldonilai / saldojml, 2)

                    Else
                        saldonilai = 0
                        saldohpp = 0

                    End If

                    'PEMBULATAN HPP
                    hppmasuk = Math.Round(hppmasuk, 2)
                    hppkeluar = Math.Round(hppkeluar, 2)


                    'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                    'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                    'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                    If jenismutasi = 1 Then
                        'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                        HppTrans = hppmasuk

                    Else
                        'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                        HppTrans = hppkeluar

                    End If

                    'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                    Select Case sumber.ToUpper
                        Case "SA"
                            sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsadetail = '" & FixDouble(iddetail) & "' "

                        Case "IB"
                            sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idibdetail = '" & FixDouble(iddetail) & "' "

                        Case "GRN"
                            sql = ""

                        Case "RI"
                            sql = ""

                        Case "PRT"
                            sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprtdetail = '" & FixDouble(iddetail) & "' "

                        Case "SI"
                            'SI ADA BARANG ASSEMBLY LANGSUNG
                            If jenismutasi = 0 And customint10 = -2 Then
                                'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsimaterial = '" & FixDouble(iddetail) & "' "

                            ElseIf jenismutasi = 1 And customint10 = -1 Then
                                'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "' "

                            ElseIf jenismutasi = 0 And customint10 = 0 Then
                                'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "' "

                            Else
                                sql = ""

                            End If

                        Case "SR"
                            sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsrdetail = '" & FixDouble(iddetail) & "' "

                        Case "PD"
                            'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                            If jenismutasi = 1 Then
                                'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpdin = '" & FixDouble(iddetail) & "' "

                            Else
                                'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpdout = '" & FixDouble(iddetail) & "' "

                            End If

                        Case "LU"
                            sql = ""

                        Case "LB"
                            sql = ""

                        Case "AK"
                            sql = ""

                        Case "RO"
                            sql = ""

                        Case Else
                            sql = ""
                    End Select

                    'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                    If Len(sql) > 0 Then
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If


                    'UPDATE TRANSAKSI BARANG
                    sql = "  UPDATE m1_item_transaction it "
                    sql &= " SET "
                    sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                    sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                    sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                    sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                    If hitungPerBarang = False Then
                        'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                        sql &= " , it.hppfix = '1' "
                    End If
                    sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                    'AMBILSALDO AKHIR
                    sql = "  SELECT it.id "
                    sql &= " FROM m1_item_transaction it "
                    'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                    sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                    sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                    sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                    sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                    sql &= " LIMIT 1"
                    dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                    If dtSaldoAkhir.Rows.Count > 0 Then
                        If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                            'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                            If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                                sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If
                        End If

                    End If

                    'UPDATE HISTORI TRANSAKSI BARANG
                    If hitungPerBarang = False Then
                        'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                        'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                        If tglBefore <> tgl Then
                            'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        ElseIf stepKe >= dtBarang.Rows.Count Then
                            'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        Else
                            sql = ""
                        End If
                        If Len(sql) > 0 Then
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If

                        'PERBARUI TGL BEFORE
                        tglBefore = tgl
                    End If


                    'INSERT KE TABEL LOG SUKSES
                    Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    Con2.Open()

                    '*** Start Transaction ***'  
                    Trans2 = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = Con2
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        Trans2.Commit()  '*** Commit Transaction ***'

                    Catch ex As Exception
                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(2) = ex.Message : GoTo selesai

                    End Try

                Next
            End If
            'END OF PROSES HITUNG ULANG ----------------------------------------


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "

            Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
            Con2.Open()

            '*** Start Transaction ***'  
            Trans2 = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

            Try

                'INSERT KE TABEL LOG GAGAL
                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". " & FixQuotes(result(2)) & "', 3)"
                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd2
                    .Connection = Con2
                    .Transaction = Trans2
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd2.ExecuteNonQuery()

                Trans2.Commit()  '*** Commit Transaction ***'

            Catch ex As Exception
                Trans2.Rollback() '*** RollBack Transaction ***'  
                result(2) = ex.Message

            End Try

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_AverageOld(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            If jarakTgl > 120 Then
                result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                    tglAwal = tgl
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                    tglAwal = tglHistory
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                tglAwal = tgl
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                tglAwal = tglHistory
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For Each drBarang As DataRow In dtBarang.Rows

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                        sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        If jenismutasi = 1 And sumber = "PD" Then
                            'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                            sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                            sql &= " FROM m6_pd_in pdi "
                            sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                            sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                            sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                            sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY pdi.idpdin "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SI" Then
                            'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                            sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                            sql &= " FROM m5_si_detail sid "
                            sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                            sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                            sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY sid.idsidetail "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SR" Then
                            'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_sr_detail srd "
                            sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                            sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                            sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)

                        Else
                            'JIKA BARANG KELUAR
                            If sumber <> "PRT" Then
                                'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                                hppkeluar = Math.Round(saldoawalhpp, 2)

                            End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            saldonilai = Math.Round(saldonilai, 2)
                            saldohpp = Math.Round(saldonilai / saldojml, 2)

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        hppmasuk = Math.Round(hppmasuk, 2)
                        hppkeluar = Math.Round(hppkeluar, 2)

                        'STEP DETAIL
                        stepDetail = 7

                        'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                            HppTrans = hppmasuk

                        Else
                            'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                            HppTrans = hppkeluar

                        End If

                        'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        Select Case sumber.ToUpper
                            Case "SA"
                                sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "IB"
                                sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "GRN"
                                sql = ""

                            Case "RI"
                                sql = ""

                            Case "PRT"
                                sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SI"
                                'SI ADA BARANG ASSEMBLY LANGSUNG
                                If jenismutasi = 0 And customint10 = -2 Then
                                    'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 1 And customint10 = -1 Then
                                    'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 0 And customint10 = 0 Then
                                    'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'sql = ""
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "SR"
                                sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "PD"
                                'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                If jenismutasi = 1 Then
                                    'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                    sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                    sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "LU"
                                sql = ""

                            Case "LB"
                                sql = ""

                            Case "AK"
                                sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case "RO"
                                sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case Else
                                sql = ""
                        End Select

                        'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        If Len(sql) > 0 Then
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd2.ExecuteNonQuery()
                        End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        sql &= " , it.jurnalfix = '0' "
                        sql &= " , it.updatehpp = '1' "
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                            sql &= " , it.hppfix = '1' "
                        End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ''AMBILSALDO AKHIR
                        'sql = "  SELECT it.id "
                        'sql &= " FROM m1_item_transaction it "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        'sql &= " LIMIT 1"
                        'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        'If dtSaldoAkhir.Rows.Count > 0 Then
                        '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 10

                        'UPDATE HISTORI TRANSAKSI BARANG
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                            'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                            If tglBefore <> tgl Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            ElseIf stepKe >= dtBarang.Rows.Count Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            Else
                                sql = ""
                            End If
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If

                            'PERBARUI TGL BEFORE
                            tglBefore = tgl
                        End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPAVERAGE
                        sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        If AsEksekusiSQL(sql) = False Then
                            result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        End If
                        'objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        'With objCmd2
                        '    .Connection = myConn
                        '    .Transaction = Trans2
                        '    .CommandType = CommandType.Text
                        '    .CommandText = sql
                        'End With
                        'objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_Average(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                    tglAwal = tgl
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                    tglAwal = tglHistory
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                tglAwal = tgl
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                tglAwal = tglHistory
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------

            If idbarang = 0 Then
                sql = "TRUNCATE TABLE m0_hitungulang_log; "
                sql &= "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & tglAwal & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & tglAwal & "'; "
                sql &= "UPDATE m1_item_transaction_history SET hppfix = 1; "
                AsEksekusiSQL(sql)
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed persiapan hitung ulang." : GoTo selesai
                End If
            Else
                sql = "TRUNCATE TABLE m0_hitungulang_log; "
                sql &= "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "' and idbarang = '" & idbarang & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & tglAwal & "' and idbarang = '" & idbarang & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & tglAwal & "' and idbarang = '" & idbarang & "'; "
                sql &= "UPDATE m1_item_transaction_history SET hppfix = 1; "
                AsEksekusiSQL(sql)
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed persiapan hitung ulang." : GoTo selesai
                End If
            End If


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction
                Dim vsaldo As Double = 0
                Dim drBarang As DataRow

                If Not hitungPerBarang Then
                    sql = "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "';"
                    If AsEksekusiSQL(sql) = False Then
                        result(2) = "failed : " & FixQuotes(sql) : GoTo selesai
                    End If
                End If

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            'hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            'nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            'jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            'hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            'nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai, it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail  "
                        sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                'it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail
                                If (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") > tgl) Then
                                    vsaldo = 1
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And Date.Parse(AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")) > Date.Parse(inputtgl)) Then
                                    vsaldo = 1.1
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") > customint10) Then
                                    vsaldo = 1.2
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") > jenismutasi) Then
                                    vsaldo = 1.3
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") > idutama) Then
                                    vsaldo = 1.4
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") = idutama And dtSaldo.Rows(0)("iddetail") > iddetail) Then
                                    vsaldo = 1.5
                                    GoTo setSaldoAwalNol

                                Else
                                    vsaldo = 2
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                End If

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    vsaldo = 3
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    vsaldo = 4
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                vsaldo = 5
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                            vsaldo = 6
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        If jenismutasi = 1 And sumber = "PD" Then
                            If kodebarang.ToUpper.StartsWith("BM") Or kodebarang.ToUpper.StartsWith("BR1.BM") Then
                                hppmasuk = 0
                            Else
                                'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                                sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                                sql &= " FROM m6_pd_in pdi "
                                sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                                sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                                sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                                sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                                sql &= " GROUP BY pdi.idpdin "
                                dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                If dtHppMasukSpesial.Rows.Count > 0 Then
                                    If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                        'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                        hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                    Else
                                        hppmasuk = 0
                                    End If
                                End If
                            End If
                            

                        ElseIf jenismutasi = 1 And sumber = "SI" Then
                            'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                            sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                            sql &= " FROM m5_si_detail sid "
                            sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                            sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                            sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY sid.idsidetail "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SR" Then
                            'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_sr_detail srd "
                            sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                            sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                            sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "RNR" Then
                            'JIKA rnr AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_rnr_detail rnrd "
                            sql &= " JOIN m5_si_detail sid ON rnrd.idsidetail = sid.idsidetail "
                            sql &= " AND rnrd.idrnr = '" & FixDouble(idutama) & "'"
                            sql &= " AND rnrd.idrnrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND rnrd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            'saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            'JIKA BARANG KELUAR
                            If sumber <> "PRT" Then
                                'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                                'hppkeluar = Math.Round(saldoawalhpp, 2)
                                hppkeluar = saldoawalhpp

                            End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            'saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        'saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            'saldonilai = Math.Round(saldonilai, 2)
                            'saldohpp = Math.Round(saldonilai / saldojml, 2)
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        'hppmasuk = Math.Round(hppmasuk, 2)
                        'hppkeluar = Math.Round(hppkeluar, 2)
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                            HppTrans = hppmasuk

                        Else
                            'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                            HppTrans = hppkeluar

                        End If

                        'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        Select Case sumber.ToUpper
                            Case "SA"
                                sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "IB"
                                sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "GRN"
                                sql = ""

                            Case "RI"
                                sql = ""

                            Case "PRT"
                                sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SI"
                                'SI ADA BARANG ASSEMBLY LANGSUNG
                                If jenismutasi = 0 And customint10 = -2 Then
                                    'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 1 And customint10 = -1 Then
                                    'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 0 And customint10 = 0 Then
                                    'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'sql = ""
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "RNR"
                                sql = "UPDATE m5_rnr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idrnr = '" & FixDouble(idutama) & "' AND idrnrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SR"
                                sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "PD"
                                'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                If jenismutasi = 1 Then
                                    'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                    sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                    sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "LU"
                                sql = ""

                            Case "LB"
                                sql = ""

                            Case "AK"
                                sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case "RO"
                                sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case Else
                                sql = ""
                        End Select

                        'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        If Len(sql) > 0 Then
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                                .CommandTimeout = 200000
                            End With
                            objCmd2.ExecuteNonQuery()
                        End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        sql &= " , it.jurnalfix = '0' "
                        sql &= " , it.updatehpp = '1' "
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                            sql &= " , it.hppfix = '1' "
                        End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ''AMBILSALDO AKHIR
                        'sql = "  SELECT it.id "
                        'sql &= " FROM m1_item_transaction it "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        'sql &= " LIMIT 1"
                        'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        'If dtSaldoAkhir.Rows.Count > 0 Then
                        '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 10

                        'UPDATE HISTORI TRANSAKSI BARANG
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                            'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                            If tglBefore <> tgl Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            ElseIf stepKe >= dtBarang.Rows.Count Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            Else
                                sql = ""
                            End If
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                    .CommandTimeout = 200000
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If

                            'PERBARUI TGL BEFORE
                            tglBefore = tgl
                        End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPAVERAGE
                        sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixDouble(vsaldo) & ", " & FixDouble(dtSaldo.Rows.Count) & "," & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ================================================== 

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_Average20210114(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                    tglAwal = tgl
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                    tglAwal = tglHistory
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                tglAwal = tgl
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                tglAwal = tglHistory
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------

            If idbarang = 0 Then
                sql = "TRUNCATE TABLE m0_hitungulang_log; "
                sql &= "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & tglAwal & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & tglAwal & "'; "
                sql &= "UPDATE m1_item_transaction_history SET hppfix = 1; "
                AsEksekusiSQL(sql)
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed persiapan hitung ulang." : GoTo selesai
                End If
            Else
                sql = "TRUNCATE TABLE m0_hitungulang_log; "
                sql &= "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "' and idbarang = '" & idbarang & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 1 WHERE tgl < '" & tglAwal & "' and idbarang = '" & idbarang & "'; "
                sql &= "UPDATE m1_item_transaction SET hppfix = 0 WHERE tgl >= '" & tglAwal & "' and idbarang = '" & idbarang & "'; "
                sql &= "UPDATE m1_item_transaction_history SET hppfix = 1; "
                AsEksekusiSQL(sql)
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed persiapan hitung ulang." : GoTo selesai
                End If
            End If


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction
                Dim vsaldo As Double = 0
                Dim drBarang As DataRow

                If Not hitungPerBarang Then
                    sql = "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "';"
                    If AsEksekusiSQL(sql) = False Then
                        result(2) = "failed : " & FixQuotes(sql) : GoTo selesai
                    End If
                End If

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            'hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            'nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            'jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            'hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            'nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai, it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail  "
                        sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                'it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail
                                If (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") > tgl) Then
                                    vsaldo = 1
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And Date.Parse(AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")) > Date.Parse(inputtgl)) Then
                                    vsaldo = 1.1
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") > customint10) Then
                                    vsaldo = 1.2
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") > jenismutasi) Then
                                    vsaldo = 1.3
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") > idutama) Then
                                    vsaldo = 1.4
                                    GoTo setSaldoAwalNol

                                ElseIf (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") = idutama And dtSaldo.Rows(0)("iddetail") > iddetail) Then
                                    vsaldo = 1.5
                                    GoTo setSaldoAwalNol

                                Else
                                    vsaldo = 2
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                End If

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    vsaldo = 3
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    vsaldo = 4
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                vsaldo = 5
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                            vsaldo = 6
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        If jenismutasi = 1 And sumber = "PD" Then
                            'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                            sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                            sql &= " FROM m6_pd_in pdi "
                            sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                            sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                            sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                            sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY pdi.idpdin "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SI" Then
                            'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                            sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                            sql &= " FROM m5_si_detail sid "
                            sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                            sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                            sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY sid.idsidetail "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SR" Then
                            'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_sr_detail srd "
                            sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                            sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                            sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "RNR" Then
                            'JIKA rnr AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_rnr_detail rnrd "
                            sql &= " JOIN m5_si_detail sid ON rnrd.idsidetail = sid.idsidetail "
                            sql &= " AND rnrd.idrnr = '" & FixDouble(idutama) & "'"
                            sql &= " AND rnrd.idrnrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND rnrd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            'saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            'JIKA BARANG KELUAR
                            If sumber <> "PRT" Then
                                'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                                'hppkeluar = Math.Round(saldoawalhpp, 2)
                                hppkeluar = saldoawalhpp

                            End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            'saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        'saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            'saldonilai = Math.Round(saldonilai, 2)
                            'saldohpp = Math.Round(saldonilai / saldojml, 2)
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        'hppmasuk = Math.Round(hppmasuk, 2)
                        'hppkeluar = Math.Round(hppkeluar, 2)
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                            HppTrans = hppmasuk

                        Else
                            'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                            HppTrans = hppkeluar

                        End If

                        'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        Select Case sumber.ToUpper
                            Case "SA"
                                sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "IB"
                                sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "GRN"
                                sql = ""

                            Case "RI"
                                sql = ""

                            Case "PRT"
                                sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SI"
                                'SI ADA BARANG ASSEMBLY LANGSUNG
                                If jenismutasi = 0 And customint10 = -2 Then
                                    'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 1 And customint10 = -1 Then
                                    'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 0 And customint10 = 0 Then
                                    'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'sql = ""
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "RNR"
                                sql = "UPDATE m5_rnr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idrnr = '" & FixDouble(idutama) & "' AND idrnrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SR"
                                sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "PD"
                                'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                If jenismutasi = 1 Then
                                    'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                    sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                    sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "LU"
                                sql = ""

                            Case "LB"
                                sql = ""

                            Case "AK"
                                sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case "RO"
                                sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case Else
                                sql = ""
                        End Select

                        'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        If Len(sql) > 0 Then
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                                .CommandTimeout = 200000
                            End With
                            objCmd2.ExecuteNonQuery()
                        End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        sql &= " , it.jurnalfix = '0' "
                        sql &= " , it.updatehpp = '1' "
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                            sql &= " , it.hppfix = '1' "
                        End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ''AMBILSALDO AKHIR
                        'sql = "  SELECT it.id "
                        'sql &= " FROM m1_item_transaction it "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        'sql &= " LIMIT 1"
                        'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        'If dtSaldoAkhir.Rows.Count > 0 Then
                        '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 10

                        'UPDATE HISTORI TRANSAKSI BARANG
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                            'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                            If tglBefore <> tgl Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            ElseIf stepKe >= dtBarang.Rows.Count Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            Else
                                sql = ""
                            End If
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                    .CommandTimeout = 200000
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If

                            'PERBARUI TGL BEFORE
                            tglBefore = tgl
                        End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPAVERAGE
                        sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixDouble(vsaldo) & ", " & FixDouble(dtSaldo.Rows.Count) & "," & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                            .CommandTimeout = 200000
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ================================================== 

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_MasukAverage(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            If jarakTgl > 120 Then
                result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                    tglAwal = tgl
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                    tglAwal = tglHistory
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
                tglAwal = tgl
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
                tglAwal = tglHistory
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                Dim drBarang As DataRow

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            'hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            'nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            'jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            'hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            'nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai, it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail  "
                        sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                'it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail
                                If (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") > tgl) _
                                    Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") > inputtgl) _
                                    Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") > customint10) _
                                    Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") > jenismutasi) _
                                    Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") > idutama) _
                                    Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") = idutama And dtSaldo.Rows(0)("iddetail") > iddetail) _
                                    Then
                                    GoTo setSaldoAwalNol

                                Else
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                End If

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        If jenismutasi = 1 And sumber = "PD" Then
                            'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                            sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                            sql &= " FROM m6_pd_in pdi "
                            sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                            sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                            sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                            sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY pdi.idpdin "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SI" Then
                            'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                            sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                            sql &= " FROM m5_si_detail sid "
                            sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                            sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                            sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                            sql &= " GROUP BY sid.idsidetail "
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    hppmasuk = 0
                                End If
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SR" Then
                            'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_sr_detail srd "
                            sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                            sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                            sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    'hppmasuk = 0
                                    hppmasuk = saldoawalhpp
                                End If
                            Else
                                hppmasuk = saldoawalhpp
                            End If

                        ElseIf jenismutasi = 1 And sumber = "RNR" Then
                            'JIKA rnr AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                            sql = "  SELECT sid.hpp as hpp "
                            sql &= " FROM m5_rnr_detail rnrd "
                            sql &= " JOIN m5_si_detail sid ON rnrd.idsidetail = sid.idsidetail "
                            sql &= " AND rnrd.idrnr = '" & FixDouble(idutama) & "'"
                            sql &= " AND rnrd.idrnrdetail = '" & FixDouble(iddetail) & "'"
                            sql &= " AND rnrd.idbarang = '" & FixDouble(idbarang) & "'"
                            dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                            If dtHppMasukSpesial.Rows.Count > 0 Then
                                If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                    hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                Else
                                    'hppmasuk = 0
                                    hppmasuk = saldoawalhpp
                                End If
                            Else
                                hppmasuk = saldoawalhpp
                            End If

                        ElseIf jenismutasi = 1 And sumber = "SA" Then
                            'JIKA SA MASUK MAKA HPP MASUK BERDASARKAN HPP AVERAGE
                            hppmasuk = saldoawalhpp
                        End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            'saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            'JIKA BARANG KELUAR
                            If sumber <> "PRT" Then
                                'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                                'hppkeluar = Math.Round(saldoawalhpp, 2)
                                hppkeluar = saldoawalhpp

                            End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            'saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        'saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            'saldonilai = Math.Round(saldonilai, 2)
                            'saldohpp = Math.Round(saldonilai / saldojml, 2)
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        'hppmasuk = Math.Round(hppmasuk, 2)
                        'hppkeluar = Math.Round(hppkeluar, 2)
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                            HppTrans = hppmasuk

                        Else
                            'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                            HppTrans = hppkeluar

                        End If

                        'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        Select Case sumber.ToUpper
                            Case "SA"
                                sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "IB"
                                sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "GRN"
                                sql = ""

                            Case "RI"
                                sql = ""

                            Case "PRT"
                                sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SI"
                                'SI ADA BARANG ASSEMBLY LANGSUNG
                                If jenismutasi = 0 And customint10 = -2 Then
                                    'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 1 And customint10 = -1 Then
                                    'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                ElseIf jenismutasi = 0 And customint10 = 0 Then
                                    'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'sql = ""
                                    sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "RNR"
                                sql = "UPDATE m5_rnr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idrnr = '" & FixDouble(idutama) & "' AND idrnrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "SR"
                                sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                            Case "PD"
                                'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                If jenismutasi = 1 Then
                                    'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                    sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                Else
                                    'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                    sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                End If

                            Case "LU"
                                sql = ""

                            Case "LB"
                                sql = ""

                            Case "AK"
                                sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case "RO"
                                sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                            Case Else
                                sql = ""
                        End Select

                        'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        If Len(sql) > 0 Then
                            objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd2
                                .Connection = myConn
                                .Transaction = Trans2
                                .CommandType = CommandType.Text
                                .CommandText = sql
                                .CommandTimeout = 200000
                            End With
                            objCmd2.ExecuteNonQuery()
                        End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        sql &= " , it.jurnalfix = '0' "
                        sql &= " , it.updatehpp = '1' "
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                            sql &= " , it.hppfix = '1' "
                        End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ''AMBILSALDO AKHIR
                        'sql = "  SELECT it.id "
                        'sql &= " FROM m1_item_transaction it "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        'sql &= " LIMIT 1"
                        'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        'If dtSaldoAkhir.Rows.Count > 0 Then
                        '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 10

                        'UPDATE HISTORI TRANSAKSI BARANG
                        If hitungPerBarang = False Then
                            'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                            'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                            If tglBefore <> tgl Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                            ElseIf stepKe >= dtBarang.Rows.Count Then
                                'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                            Else
                                sql = ""
                            End If
                            If Len(sql) > 0 Then
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                            End If

                            'PERBARUI TGL BEFORE
                            tglBefore = tgl
                        End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPAVERAGE
                        sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ================================================== 

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_AveragePerBarangOld1(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            If jarakTgl > 120 Then
                result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '    tglAwal = tgl
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '    tglAwal = tglHistory
            'End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            ''CEK PERIODE AKUNTANSI ---------------------------------------------
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtTransBarangAll As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG GROUP BY BARANG --------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " GROUP BY i.bid "
            sql &= " ORDER BY i.bid "
            Dim dtListBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG GROUP BY BARANG --------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtListBarang.Rows.Count > 0 Then

                'DATATABLE TRANSAKSI BARANG PER BARANG
                Dim dtTransBarang As New DataTable

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                Dim drBarang As DataRow

                'PERULANGAN HITUNG ULANG PER BARANG
                For i As Double = 0 To dtListBarang.Rows.Count - 1

                    'RESET NILAI VARIABEL SALDO HASIL HITUNG
                    saldojml = 0 : saldohpp = 0 : saldonilai = 0

                    'AMBIL BARANG
                    idbarang = Double.Parse(FxDB(dtListBarang.Rows(i)("idbarang"), 0))

                    'AMBIL SALDO AWAL
                    sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                    sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                    sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                    sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tglAwal) & "' "
                    sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                    dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                    'AMBIL DATA TRANSAKSI BARANG PERBARANG
                    dtTransBarang = AsDataTableFilterSortDt(dtTransBarangAll, "idbarang = '" & idbarang & "'", "tgl, inputtgl, customint10, jenismutasi, idutama, iddetail")
                    If dtTransBarang.Rows.Count > 0 Then

                        'PERULANGAN HITUNG ULANG PER BARANG PER ROW TRANSAKSI BARANG
                        For j As Double = 0 To dtTransBarang.Rows.Count - 1

                            drBarang = dtTransBarang.Rows(j)

                            '*** Open Connection ***'  
                            myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                            myConn.Open()

                            '*** Start Transaction ***'  
                            Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                            Try

                                'STEPKE
                                stepKe = stepKe + 1

                                'STEP DETAIL
                                stepDetail = 1

                                'SET DATA BARANG
                                id = Integer.Parse(FxDB(drBarang("id"), 0))
                                'idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                                kodebarang = FxDB(drBarang("bkode"), "")
                                jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                                tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                                inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                                sumber = FxDB(drBarang("sumber"), "")
                                notransaksi = FxDB(drBarang("notransaksi"), "")
                                idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                                iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                                customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                                'STEP DETAIL
                                stepDetail = 2

                                'SET SALDO YANG DIHITUNG
                                If jenismutasi = 1 Then
                                    'JIKA BARANG MASUK
                                    jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                                    hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                                    nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0

                                Else
                                    'JIKA BARANG KELUAR
                                    jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                                    hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                                    nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0

                                End If

                                'STEP DETAIL
                                stepDetail = 3

                                'JIKA PERULANGAN PERTAMA PER BARANG MAKA AMBIL SALDO AWAL DARI DATA TABLE
                                If j = 0 Then

                                    'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                    If dtSaldo.Rows.Count > 0 Then
                                        'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                                        currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                                        'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                                        If currUrutan = 0 Then
                                            saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                            saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                            saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)

                                            'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                                        ElseIf currUrutan > 1 Then
                                            'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                            dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                            'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                            If dtCurrSaldo.Rows.Count > 0 Then
                                                saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                                saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                                saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)

                                            Else
                                                'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                                GoTo setSaldoAwalNol
                                            End If

                                            'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                                        Else
                                            'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                            GoTo setSaldoAwalNol

                                        End If

                                    Else
                                        'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                                        saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                                    End If

                                Else
                                    'JIKA BUKAN PERULANGAN PERTAMA PER BARANG MAKA AMBIL SALDO AWAL DARI SALDO PERULANGAN SEBELUMNYA
                                    saldoawaljml = saldojml : saldoawalhpp = saldohpp : saldoawalnilai = saldonilai

                                End If


                                'STEP DETAIL
                                stepDetail = 4

                                'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                                'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                                If jenismutasi = 1 And sumber = "PD" Then
                                    'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                                    sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                                    sql &= " FROM m6_pd_in pdi "
                                    sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                                    sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                                    sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                                    sql &= " GROUP BY pdi.idpdin "
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                ElseIf jenismutasi = 1 And sumber = "SI" Then
                                    'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                                    sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                                    sql &= " FROM m5_si_detail sid "
                                    sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                                    sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                                    sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                                    sql &= " GROUP BY sid.idsidetail "
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                ElseIf jenismutasi = 1 And sumber = "SR" Then
                                    'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                                    sql = "  SELECT sid.hpp as hpp "
                                    sql &= " FROM m5_sr_detail srd "
                                    sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                                    sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                                    sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                End If

                                'STEP DETAIL
                                stepDetail = 5

                                'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                                If jenismutasi = 1 Then
                                    'JIKA BARANG MASUK
                                    'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                                    saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)

                                Else
                                    'JIKA BARANG KELUAR
                                    'If sumber <> "PRT" Then
                                    'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                                    hppkeluar = Math.Round(saldoawalhpp, 2)
                                    'End If

                                    'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                                    saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)

                                End If

                                'STEP DETAIL
                                stepDetail = 6

                                'HITUNG SALDOJML
                                saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)

                                'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                                If saldojml <> 0 Then
                                    saldonilai = Math.Round(saldonilai, 2)
                                    saldohpp = Math.Round(saldonilai / saldojml, 2)

                                Else
                                    saldonilai = 0
                                    saldohpp = 0

                                End If

                                'PEMBULATAN HPP
                                hppmasuk = Math.Round(hppmasuk, 2)
                                hppkeluar = Math.Round(hppkeluar, 2)

                                'STEP DETAIL
                                stepDetail = 7

                                'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                                'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                                'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                                If jenismutasi = 1 Then
                                    'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                                    HppTrans = hppmasuk

                                Else
                                    'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                                    HppTrans = hppkeluar

                                End If

                                'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                                Select Case sumber.ToUpper
                                    Case "SA"
                                        sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "IB"
                                        sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "GRN"
                                        sql = ""

                                    Case "RI"
                                        sql = ""

                                    Case "PRT"
                                        sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "SI"
                                        'SI ADA BARANG ASSEMBLY LANGSUNG
                                        If jenismutasi = 0 And customint10 = -2 Then
                                            'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                            sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        ElseIf jenismutasi = 1 And customint10 = -1 Then
                                            'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        ElseIf jenismutasi = 0 And customint10 = 0 Then
                                            'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        Else
                                            'sql = ""
                                            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        End If

                                    Case "SR"
                                        sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "PD"
                                        'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                        If jenismutasi = 1 Then
                                            'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                            sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        Else
                                            'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                            sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        End If

                                    Case "LU"
                                        sql = ""

                                    Case "LB"
                                        sql = ""

                                    Case "AK"
                                        sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                                    Case "RO"
                                        sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                                    Case Else
                                        sql = ""
                                End Select

                                'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                                If Len(sql) > 0 Then
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()
                                End If

                                'STEP DETAIL
                                stepDetail = 8

                                'UPDATE TRANSAKSI BARANG
                                sql = "  UPDATE m1_item_transaction it "
                                sql &= " SET "
                                sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                                sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                                sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                                sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                                sql &= " , it.jurnalfix = '0' "
                                sql &= " , it.updatehpp = '1' "
                                'If hitungPerBarang = False Then
                                'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                                sql &= " , it.hppfix = '1' "
                                'End If
                                sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()

                                'STEP DETAIL
                                stepDetail = 9

                                ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                                ''AMBILSALDO AKHIR
                                'sql = "  SELECT it.id "
                                'sql &= " FROM m1_item_transaction it "
                                ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                                'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                                'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                                'sql &= " LIMIT 1"
                                'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                                'If dtSaldoAkhir.Rows.Count > 0 Then
                                '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                                '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                                '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                                sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                                '        End If
                                '    End If

                                'End If

                                'STEP DETAIL
                                stepDetail = 10

                                'UPDATE HISTORI TRANSAKSI BARANG
                                If hitungPerBarang = False Then
                                    'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                                    'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                                    If tglBefore <> tgl Then
                                        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "' AND it.idbarang = '" & FixDouble(idbarang) & "'"
                                    ElseIf stepKe >= dtTransBarangAll.Rows.Count Then
                                        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "' AND it.idbarang = '" & FixDouble(idbarang) & "'"
                                    Else
                                        sql = ""
                                    End If
                                    If Len(sql) > 0 Then
                                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd2
                                            .Connection = myConn
                                            .Transaction = Trans2
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd2.ExecuteNonQuery()
                                    End If

                                    'PERBARUI TGL BEFORE
                                    tglBefore = tgl
                                End If

                                'STEP DETAIL
                                stepDetail = 11

                                'INSERT KE M0_HPPAVERAGE
                                sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                                sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                                sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                                'If AsEksekusiSQL(sql) = False Then
                                '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                                'End If
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()

                                'STEP DETAIL
                                stepDetail = 12

                                'INSERT KE TABEL LOG SUKSES
                                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                                'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                                'If AsEksekusiSQL(sql) = False Then
                                '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                                'End If
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()


                                Trans2.Commit()  '*** Commit Transaction ***'
                                result(1) = 1
                                result(2) = ""
                                result(3) = stepKe
                                result(4) = result(4)

                            Catch ex As Exception


                                Trans2.Rollback() '*** RollBack Transaction ***'  
                                result(1) = 0
                                result(2) = ex.Message
                                result(3) = 0
                                result(4) = result(4)
                                GoTo selesai

                            Finally
                                myConn.Close()

                            End Try

                        Next
                    End If

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_AveragePerBarang(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            'VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            'JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            'BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        Else
            'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            Dim tgl As String = "", tglHistory As String = ""

            '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTgl.Rows.Count > 0 Then
                tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            End If

            '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            If dtTglHistory.Rows.Count > 0 Then
                tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '    tglAwal = tgl
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '    tglAwal = tglHistory
            'End If

            'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            If Len(tgl) > 0 And Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
                If Date.Parse(tgl) < Date.Parse(tglHistory) Then
                    'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
                Else
                    'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
                End If
            ElseIf Len(tgl) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
                result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            ElseIf Len(tglHistory) > 0 Then
                'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
                result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtTransBarangAll As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG GROUP BY BARANG --------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            If Not hitungPerBarang Then
                sql &= " AND it.hppfix = 0 "
            End If
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " GROUP BY i.bid "
            sql &= " ORDER BY i.bid "
            Dim dtListBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG GROUP BY BARANG --------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtListBarang.Rows.Count > 0 Then

                'DATATABLE TRANSAKSI BARANG PER BARANG
                Dim dtTransBarang As New DataTable

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                Dim drBarang As DataRow

                If Not hitungPerBarang Then
                    sql = "DELETE FROM m0_hppaverage WHERE tgl >= '" & tglAwal & "';"
                    If AsEksekusiSQL(sql) = False Then
                        result(2) = "failed : " & FixQuotes(sql) : GoTo selesai
                    End If
                End If

                'PERULANGAN HITUNG ULANG PER BARANG
                For i As Double = 0 To dtListBarang.Rows.Count - 1

                    tglBefore = tglAwal

                    'RESET NILAI VARIABEL SALDO HASIL HITUNG
                    saldojml = 0 : saldohpp = 0 : saldonilai = 0

                    'AMBIL BARANG
                    idbarang = Double.Parse(FxDB(dtListBarang.Rows(i)("idbarang"), 0))

                    'AMBIL SALDO AWAL
                    sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai, it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                    sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                    sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                    sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tglAwal) & "' "
                    sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                    dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                    'AMBIL DATA TRANSAKSI BARANG PERBARANG
                    dtTransBarang = AsDataTableFilterSortDt(dtTransBarangAll, "idbarang = '" & idbarang & "'", "tgl, inputtgl, customint10, jenismutasi, idutama, iddetail")
                    If dtTransBarang.Rows.Count > 0 Then

                        'PERULANGAN HITUNG ULANG PER BARANG PER ROW TRANSAKSI BARANG
                        For j As Double = 0 To dtTransBarang.Rows.Count - 1

                            drBarang = dtTransBarang.Rows(j)

                            '*** Open Connection ***'  
                            myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                            myConn.Open()

                            '*** Start Transaction ***'  
                            Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                            Try

                                'STEPKE
                                stepKe = stepKe + 1

                                'STEP DETAIL
                                stepDetail = 1

                                'SET DATA BARANG
                                id = Integer.Parse(FxDB(drBarang("id"), 0))
                                'idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                                kodebarang = FxDB(drBarang("bkode"), "")
                                jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                                tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                                inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                                sumber = FxDB(drBarang("sumber"), "")
                                notransaksi = FxDB(drBarang("notransaksi"), "")
                                idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                                iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                                customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                                'STEP DETAIL
                                stepDetail = 2

                                'SET SALDO YANG DIHITUNG
                                If jenismutasi = 1 Then
                                    'JIKA BARANG MASUK
                                    'jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                                    'hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                                    'nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0
                                    jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                                    hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                                    nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                                Else
                                    'JIKA BARANG KELUAR
                                    'jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                                    'hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                                    'nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0
                                    jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                                    hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                                    nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                                End If

                                'STEP DETAIL
                                stepDetail = 3

                                'JIKA PERULANGAN PERTAMA PER BARANG MAKA AMBIL SALDO AWAL DARI DATA TABLE
                                If j = 0 Then

                                    'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                    If dtSaldo.Rows.Count > 0 Then
                                        'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                                        currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                                        'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                                        If currUrutan = 0 Then

                                            'it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail
                                            If (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") > tgl) _
                                                Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") > inputtgl) _
                                                Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") > customint10) _
                                                Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") > jenismutasi) _
                                                Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") > idutama) _
                                                Or (AsFormatTanggal(FxDB(dtSaldo.Rows(0)("tgl"), "1900-01-01"), "yyyy-MM-dd") = tgl And AsFormatTanggal(FxDB(dtSaldo.Rows(0)("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss") = inputtgl And dtSaldo.Rows(0)("customint10") = customint10 And dtSaldo.Rows(0)("jenismutasi") = jenismutasi And dtSaldo.Rows(0)("idutama") = idutama And dtSaldo.Rows(0)("iddetail") > iddetail) _
                                                Then
                                                GoTo setSaldoAwalNol

                                            Else
                                                'saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                                'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                                'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)
                                                saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                                saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                                saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                            End If

                                            'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                                        ElseIf currUrutan > 1 Then
                                            'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                            dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                            'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                            If dtCurrSaldo.Rows.Count > 0 Then
                                                'saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                                'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                                'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)
                                                saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                                saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                                saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                            Else
                                                'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                                GoTo setSaldoAwalNol
                                            End If

                                            'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                                        Else
                                            'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                            GoTo setSaldoAwalNol

                                        End If

                                    Else
                                        'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                                        saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                                    End If

                                Else
                                    'JIKA BUKAN PERULANGAN PERTAMA PER BARANG MAKA AMBIL SALDO AWAL DARI SALDO PERULANGAN SEBELUMNYA
                                    saldoawaljml = saldojml : saldoawalhpp = saldohpp : saldoawalnilai = saldonilai

                                End If


                                'STEP DETAIL
                                stepDetail = 4

                                'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                                'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                                If jenismutasi = 1 And sumber = "PD" Then
                                    'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                                    sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                                    sql &= " FROM m6_pd_in pdi "
                                    sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                                    sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                                    sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                                    sql &= " GROUP BY pdi.idpdin "
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                            hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                ElseIf jenismutasi = 1 And sumber = "SI" Then
                                    'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                                    sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                                    sql &= " FROM m5_si_detail sid "
                                    sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                                    sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                                    sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                                    sql &= " GROUP BY sid.idsidetail "
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                            hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                ElseIf jenismutasi = 1 And sumber = "SR" Then
                                    'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                                    sql = "  SELECT sid.hpp as hpp "
                                    sql &= " FROM m5_sr_detail srd "
                                    sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                                    sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                                    sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                            hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                ElseIf jenismutasi = 1 And sumber = "RNR" Then
                                    'JIKA rnr AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                                    sql = "  SELECT sid.hpp as hpp "
                                    sql &= " FROM m5_rnr_detail rnrd "
                                    sql &= " JOIN m5_si_detail sid ON rnrd.idsidetail = sid.idsidetail "
                                    sql &= " AND rnrd.idrnr = '" & FixDouble(idutama) & "'"
                                    sql &= " AND rnrd.idrnrdetail = '" & FixDouble(iddetail) & "'"
                                    sql &= " AND rnrd.idbarang = '" & FixDouble(idbarang) & "'"
                                    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                                    If dtHppMasukSpesial.Rows.Count > 0 Then
                                        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                                            'hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                                            hppmasuk = Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0))
                                        Else
                                            hppmasuk = 0
                                        End If
                                    End If

                                End If

                                'STEP DETAIL
                                stepDetail = 5

                                'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                                If jenismutasi = 1 Then
                                    'JIKA BARANG MASUK
                                    'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                                    'saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)
                                    saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                                Else
                                    'JIKA BARANG KELUAR
                                    If sumber <> "PRT" Then
                                        'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                                        'hppkeluar = Math.Round(saldoawalhpp, 2)
                                        hppkeluar = saldoawalhpp
                                    End If

                                    'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                                    'saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)
                                    saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                                End If

                                'STEP DETAIL
                                stepDetail = 6

                                'HITUNG SALDOJML
                                'saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)
                                saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                                'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                                If saldojml <> 0 Then
                                    'saldonilai = Math.Round(saldonilai, 2)
                                    'saldohpp = Math.Round(saldonilai / saldojml, 2)
                                    saldonilai = saldonilai
                                    saldohpp = saldonilai / saldojml

                                Else
                                    saldonilai = 0
                                    saldohpp = 0

                                End If

                                'PEMBULATAN HPP
                                'hppmasuk = Math.Round(hppmasuk, 2)
                                'hppkeluar = Math.Round(hppkeluar, 2)
                                hppmasuk = hppmasuk
                                hppkeluar = hppkeluar

                                'STEP DETAIL
                                stepDetail = 7

                                'UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                                'SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                                'SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                                If jenismutasi = 1 Then
                                    'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                                    HppTrans = hppmasuk

                                Else
                                    'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                                    HppTrans = hppkeluar

                                End If

                                'UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                                Select Case sumber.ToUpper
                                    Case "SA"
                                        sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "IB"
                                        sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "GRN"
                                        sql = ""

                                    Case "RI"
                                        sql = ""

                                    Case "PRT"
                                        sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "SI"
                                        'SI ADA BARANG ASSEMBLY LANGSUNG
                                        If jenismutasi = 0 And customint10 = -2 Then
                                            'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                            sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        ElseIf jenismutasi = 1 And customint10 = -1 Then
                                            'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                                            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        ElseIf jenismutasi = 0 And customint10 = 0 Then
                                            'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                                            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        Else
                                            'sql = ""
                                            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        End If

                                    Case "RNR"
                                        sql = "UPDATE m5_rnr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idrnr = '" & FixDouble(idutama) & "' AND idrnrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "SR"
                                        sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                    Case "PD"
                                        'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                        If jenismutasi = 1 Then
                                            'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                                            sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        Else
                                            'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                            sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                                        End If

                                    Case "LU"
                                        sql = ""

                                    Case "LB"
                                        sql = ""

                                    Case "AK"
                                        sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                                    Case "RO"
                                        sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                                    Case Else
                                        sql = ""
                                End Select

                                'EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                                If Len(sql) > 0 Then
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()
                                End If

                                'STEP DETAIL
                                stepDetail = 8

                                'UPDATE TRANSAKSI BARANG
                                sql = "  UPDATE m1_item_transaction it "
                                sql &= " SET "
                                sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                                sql &= " , it.saldojml = '" & FixDouble(saldojml) & "' "
                                sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                                sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                                sql &= " , it.jurnalfix = '0' "
                                sql &= " , it.updatehpp = '1' "
                                'If hitungPerBarang = False Then
                                'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                                sql &= " , it.hppfix = '1' "
                                'End If
                                sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()

                                'STEP DETAIL
                                stepDetail = 9

                                ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                                ''AMBILSALDO AKHIR
                                'sql = "  SELECT it.id "
                                'sql &= " FROM m1_item_transaction it "
                                ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                                'sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                                'sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                                'sql &= " LIMIT 1"
                                'dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                                'If dtSaldoAkhir.Rows.Count > 0 Then
                                '    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                                '        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                                '        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                                sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()
                                '        End If
                                '    End If

                                'End If

                                'STEP DETAIL
                                stepDetail = 10

                                'UPDATE HISTORI TRANSAKSI BARANG
                                'If hitungPerBarang = False Then
                                'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                                'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                                If tglBefore <> tgl Then
                                    'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                                    sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "' AND it.idbarang = '" & FixDouble(idbarang) & "'"
                                    'ElseIf stepKe >= dtTransBarangAll.Rows.Count Then
                                ElseIf j + 1 >= dtTransBarang.Rows.Count Then
                                    'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                                    sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "' AND it.idbarang = '" & FixDouble(idbarang) & "'"
                                Else
                                    sql = ""
                                End If
                                If Len(sql) > 0 Then
                                    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd2
                                        .Connection = myConn
                                        .Transaction = Trans2
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd2.ExecuteNonQuery()
                                End If

                                'PERBARUI TGL BEFORE
                                tglBefore = tgl
                                'End If

                                'STEP DETAIL
                                stepDetail = 11

                                'INSERT KE M0_HPPAVERAGE
                                sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                                sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                                sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                                'If AsEksekusiSQL(sql) = False Then
                                '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                                'End If
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()

                                'STEP DETAIL
                                stepDetail = 12

                                'INSERT KE TABEL LOG SUKSES
                                sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                                sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                                'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                                'If AsEksekusiSQL(sql) = False Then
                                '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                                'End If
                                objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd2
                                    .Connection = myConn
                                    .Transaction = Trans2
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd2.ExecuteNonQuery()


                                Trans2.Commit()  '*** Commit Transaction ***'
                                result(1) = 1
                                result(2) = ""
                                result(3) = stepKe
                                result(4) = result(4)

                            Catch ex As Exception


                                Trans2.Rollback() '*** RollBack Transaction ***'  
                                result(1) = 0
                                result(2) = ex.Message
                                result(3) = 0
                                result(4) = result(4)
                                GoTo selesai

                            Finally
                                myConn.Close()

                            End Try

                        Next
                    End If

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_SaldoOld1(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            If jarakTgl > 120 Then
                result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            End If

            ''VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            ''JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            ''BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            ''DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            ''DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            'Dim tgl As String = "", tglHistory As String = ""

            ''1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            ''sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtTgl.Rows.Count > 0 Then
            '    tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            'End If

            ''2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            ''sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtTglHistory.Rows.Count > 0 Then
            '    tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            'End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
            '        result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
            '        result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
            '    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
            '    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            'End If

            'Else
            '    'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            '    'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            '    'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            '    'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            '    Dim tgl As String = "", tglHistory As String = ""

            '    '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            '    'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            '    If dtTgl.Rows.Count > 0 Then
            '        tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            '    End If

            '    '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            '    'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            '    If dtTglHistory.Rows.Count > 0 Then
            '        tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            '    End If

            '    'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            '    If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '        If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '            'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '            tglAwal = tgl
            '        Else
            '            'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '            tglAwal = tglHistory
            '        End If
            '    ElseIf Len(tgl) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    ElseIf Len(tglHistory) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For Each drBarang As DataRow In dtBarang.Rows

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                        'sqlSAwal &= " FROM m0_hppaverage it, (SELECT @zurut := 0) AS VariableInit "
                        sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid "
                        sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1, (SELECT @zurut := 0) AS VariableInit "
                        sqlSAwal &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        ''AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        ''PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        'If jenismutasi = 1 And sumber = "PD" Then
                        '    'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                        '    sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                        '    sql &= " FROM m6_pd_in pdi "
                        '    sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                        '    sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                        '    sql &= " GROUP BY pdi.idpdin "
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'ElseIf jenismutasi = 1 And sumber = "SI" Then
                        '    'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                        '    sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                        '    sql &= " FROM m5_si_detail sid "
                        '    sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                        '    sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                        '    sql &= " GROUP BY sid.idsidetail "
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'ElseIf jenismutasi = 1 And sumber = "SR" Then
                        '    'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                        '    sql = "  SELECT sid.hpp as hpp "
                        '    sql &= " FROM m5_sr_detail srd "
                        '    sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                        '    sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)

                        Else
                            ''JIKA BARANG KELUAR
                            'If sumber <> "PRT" Then
                            '    'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                            '    hppkeluar = Math.Round(saldoawalhpp, 2)

                            'End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            saldonilai = Math.Round(saldonilai, 2)
                            saldohpp = Math.Round(saldonilai / saldojml, 2)

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        hppmasuk = Math.Round(hppmasuk, 2)
                        hppkeluar = Math.Round(hppkeluar, 2)

                        'STEP DETAIL
                        stepDetail = 7

                        ''UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        ''SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        ''SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        'If jenismutasi = 1 Then
                        '    'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                        '    HppTrans = hppmasuk

                        'Else
                        '    'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                        '    HppTrans = hppkeluar

                        'End If

                        ''UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        'Select Case sumber.ToUpper
                        '    Case "SA"
                        '        sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "IB"
                        '        sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "GRN"
                        '        sql = ""

                        '    Case "RI"
                        '        sql = ""

                        '    Case "PRT"
                        '        sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "SI"
                        '        'SI ADA BARANG ASSEMBLY LANGSUNG
                        '        If jenismutasi = 0 And customint10 = -2 Then
                        '            'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                        '            sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        ElseIf jenismutasi = 1 And customint10 = -1 Then
                        '            'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        ElseIf jenismutasi = 0 And customint10 = 0 Then
                        '            'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        Else
                        '            'sql = ""
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        End If

                        '    Case "SR"
                        '        sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "PD"
                        '        'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                        '        If jenismutasi = 1 Then
                        '            'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                        '            sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        Else
                        '            'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                        '            sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        End If

                        '    Case "LU"
                        '        sql = ""

                        '    Case "LB"
                        '        sql = ""

                        '    Case "AK"
                        '        sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                        '    Case "RO"
                        '        sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                        '    Case Else
                        '        sql = ""
                        'End Select

                        ''EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        'If Len(sql) > 0 Then
                        '    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        '    With objCmd2
                        '        .Connection = myConn
                        '        .Transaction = Trans2
                        '        .CommandType = CommandType.Text
                        '        .CommandText = sql
                        '    End With
                        '    objCmd2.ExecuteNonQuery()
                        'End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        'sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= "  it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        'sql &= " , it.jurnalfix = '0' "
                        'sql &= " , it.updatehpp = '1' "
                        'If hitungPerBarang = False Then
                        '    'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                        '    sql &= " , it.hppfix = '1' "
                        'End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ' ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ' ''AMBILSALDO AKHIR
                        ''sql = "  SELECT it.id "
                        ''sql &= " FROM m1_item_transaction it "
                        ' ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        ''sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        ''sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        ''sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        ''sql &= " LIMIT 1"
                        ''dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        ''If dtSaldoAkhir.Rows.Count > 0 Then
                        ''    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        ''        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        ''        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        'sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        'objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        'With objCmd2
                        '    .Connection = myConn
                        '    .Transaction = Trans2
                        '    .CommandType = CommandType.Text
                        '    .CommandText = sql
                        'End With
                        'objCmd2.ExecuteNonQuery()
                        ''        End If
                        ''    End If

                        ''End If

                        'STEP DETAIL
                        stepDetail = 10

                        ''UPDATE HISTORI TRANSAKSI BARANG
                        'If hitungPerBarang = False Then
                        '    'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                        '    'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                        '    If tglBefore <> tgl Then
                        '        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        '        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        '    ElseIf stepKe >= dtBarang.Rows.Count Then
                        '        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        '        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        '    Else
                        '        sql = ""
                        '    End If
                        '    If Len(sql) > 0 Then
                        '        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        '        With objCmd2
                        '            .Connection = myConn
                        '            .Transaction = Trans2
                        '            .CommandType = CommandType.Text
                        '            .CommandText = sql
                        '        End With
                        '        objCmd2.ExecuteNonQuery()
                        '    End If

                        '    'PERBARUI TGL BEFORE
                        '    tglBefore = tgl
                        'End If

                        'STEP DETAIL
                        stepDetail = 11

                        ''INSERT KE M0_HPPAVERAGE
                        'sql = "  INSERT INTO m0_hppaverage(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        'sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        'sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        ''If AsEksekusiSQL(sql) = False Then
                        ''    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        ''End If
                        'objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        'With objCmd2
                        '    .Connection = myConn
                        '    .Transaction = Trans2
                        '    .CommandType = CommandType.Text
                        '    .CommandText = sql
                        'End With
                        'objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 2, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        If AsEksekusiSQL(sql) = False Then
                            result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        End If
                        'objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        'With objCmd2
                        '    .Connection = myConn
                        '    .Transaction = Trans2
                        '    .CommandType = CommandType.Text
                        '    .CommandText = sql
                        'End With
                        'objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 2, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_Saldo(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            'VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            If jarakTgl > 120 Then
                result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            End If

            ''VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            ''JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            ''BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            ''DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            ''DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            'Dim tgl As String = "", tglHistory As String = ""

            ''1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            ''sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtTgl.Rows.Count > 0 Then
            '    tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            'End If

            ''2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            ''sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtTglHistory.Rows.Count > 0 Then
            '    tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            'End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
            '        result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
            '        result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
            '    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
            '    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            'End If

            'Else
            '    'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            '    'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            '    'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            '    'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            '    Dim tgl As String = "", tglHistory As String = ""

            '    '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            '    'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            '    If dtTgl.Rows.Count > 0 Then
            '        tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            '    End If

            '    '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            '    'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            '    If dtTglHistory.Rows.Count > 0 Then
            '        tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            '    End If

            '    'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            '    If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '        If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '            'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '            tglAwal = tgl
            '        Else
            '            'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '            tglAwal = tglHistory
            '        End If
            '    ElseIf Len(tgl) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    ElseIf Len(tglHistory) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction
                Dim drBarang As DataRow

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            'hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            'nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            'jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            'hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            'nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                        sqlSAwal &= " FROM m0_hppsaldo it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.tgl <= '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                'saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)
                                saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        ''AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        ''PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        'If jenismutasi = 1 And sumber = "PD" Then
                        '    'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                        '    sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                        '    sql &= " FROM m6_pd_in pdi "
                        '    sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                        '    sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                        '    sql &= " GROUP BY pdi.idpdin "
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'ElseIf jenismutasi = 1 And sumber = "SI" Then
                        '    'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                        '    sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                        '    sql &= " FROM m5_si_detail sid "
                        '    sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                        '    sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                        '    sql &= " GROUP BY sid.idsidetail "
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'ElseIf jenismutasi = 1 And sumber = "SR" Then
                        '    'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                        '    sql = "  SELECT sid.hpp as hpp "
                        '    sql &= " FROM m5_sr_detail srd "
                        '    sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                        '    sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            'saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            ''JIKA BARANG KELUAR
                            'If sumber <> "PRT" Then
                            '    'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                            '    hppkeluar = Math.Round(saldoawalhpp, 2)

                            'End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            'saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        'saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            'saldonilai = Math.Round(saldonilai, 2)
                            'saldohpp = Math.Round(saldonilai / saldojml, 2)
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        'hppmasuk = Math.Round(hppmasuk, 2)
                        'hppkeluar = Math.Round(hppkeluar, 2)
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        ''UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        ''SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        ''SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        'If jenismutasi = 1 Then
                        '    'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                        '    HppTrans = hppmasuk

                        'Else
                        '    'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                        '    HppTrans = hppkeluar

                        'End If

                        ''UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        'Select Case sumber.ToUpper
                        '    Case "SA"
                        '        sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "IB"
                        '        sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "GRN"
                        '        sql = ""

                        '    Case "RI"
                        '        sql = ""

                        '    Case "PRT"
                        '        sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "SI"
                        '        'SI ADA BARANG ASSEMBLY LANGSUNG
                        '        If jenismutasi = 0 And customint10 = -2 Then
                        '            'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                        '            sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        ElseIf jenismutasi = 1 And customint10 = -1 Then
                        '            'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        ElseIf jenismutasi = 0 And customint10 = 0 Then
                        '            'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        Else
                        '            'sql = ""
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        End If

                        '    Case "SR"
                        '        sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "PD"
                        '        'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                        '        If jenismutasi = 1 Then
                        '            'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                        '            sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        Else
                        '            'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                        '            sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        End If

                        '    Case "LU"
                        '        sql = ""

                        '    Case "LB"
                        '        sql = ""

                        '    Case "AK"
                        '        sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                        '    Case "RO"
                        '        sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                        '    Case Else
                        '        sql = ""
                        'End Select

                        ''EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        'If Len(sql) > 0 Then
                        '    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        '    With objCmd2
                        '        .Connection = myConn
                        '        .Transaction = Trans2
                        '        .CommandType = CommandType.Text
                        '        .CommandText = sql
                        '    End With
                        '    objCmd2.ExecuteNonQuery()
                        'End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        'sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= "  it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        'sql &= " , it.jurnalfix = '0' "
                        'sql &= " , it.updatehpp = '1' "
                        'If hitungPerBarang = False Then
                        '    'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                        '    sql &= " , it.hppfix = '1' "
                        'End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ' ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ' ''AMBILSALDO AKHIR
                        ''sql = "  SELECT it.id "
                        ''sql &= " FROM m1_item_transaction it "
                        ' ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        ''sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        ''sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        ''sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        ''sql &= " LIMIT 1"
                        ''dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        ''If dtSaldoAkhir.Rows.Count > 0 Then
                        ''    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        ''        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        ''        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        'sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        'objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        'With objCmd2
                        '    .Connection = myConn
                        '    .Transaction = Trans2
                        '    .CommandType = CommandType.Text
                        '    .CommandText = sql
                        'End With
                        'objCmd2.ExecuteNonQuery()
                        ''        End If
                        ''    End If

                        ''End If

                        'STEP DETAIL
                        stepDetail = 10

                        ''UPDATE HISTORI TRANSAKSI BARANG
                        'If hitungPerBarang = False Then
                        '    'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                        '    'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                        '    If tglBefore <> tgl Then
                        '        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        '        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        '    ElseIf stepKe >= dtBarang.Rows.Count Then
                        '        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        '        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        '    Else
                        '        sql = ""
                        '    End If
                        '    If Len(sql) > 0 Then
                        '        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        '        With objCmd2
                        '            .Connection = myConn
                        '            .Transaction = Trans2
                        '            .CommandType = CommandType.Text
                        '            .CommandText = sql
                        '        End With
                        '        objCmd2.ExecuteNonQuery()
                        '    End If

                        '    'PERBARUI TGL BEFORE
                        '    tglBefore = tgl
                        'End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPSALDO
                        sql = "  INSERT INTO m0_hppsaldo(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 2, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 2, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_SaldoFifo(ByVal param As String) As String

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim tglAwal As String = "", tglAkhir As String = "", idbarang As Integer = 0, hitungPerBarang As Boolean = True
        Dim id As Integer = 0
        Dim notransaksi As String = "", kodebarang As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'tglAwal(0) As Date, tglAkhir(1) As Date, idbarang(2) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tglAwal, tglAkhir, idbarang

        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 3) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'VALIDASI TIPE DATA ================================================================
        'tglAwal(0) As Date
        tglAwal = dataUtama(0)
        If (IsDate(tglAwal) = False Or tglAwal = "0000-00-00") Then
            result(2) = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If

        'tglAkhir(1) As Date
        tglAkhir = dataUtama(1)
        If (IsDate(tglAkhir) = False Or tglAkhir = "0000-00-00") Then
            result(2) = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If

        'idbarang(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "idbarang required numeric." : GoTo selesai
        Else
            idbarang = dataUtama(2)
            'set hitungPerBarang, jika idbarang <> 0 maka true, jika idbarang = 0 maka false
            hitungPerBarang = IIf(idbarang <> 0, True, False)
        End If
        'END OF VALIDASI TIPE DATA =========================================================


        'TRANSAKSI KE DATABASE =============================================================
        'myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        'myConn.Open()

        'SET TGLAWAL ---------------------------------------------------------------
        If Not hitungPerBarang Then
            'JIKA HITUNG ULANG SEMUA BARANG MAKA VALIDASI TGLAWAL

            ''VALIDASI JARAK TGLAWAL DAN TGLAKHIR, MAKSIMAL 120 HARI
            'Dim jarakTgl As Long = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > 120 Then
            '    result(2) = "Difference between Start Date and End Date should not be more than 120 days. Difference between Start Date and End Date that you fill is " & jarakTgl & " days." : GoTo selesai
            'End If

            ''VALIDASI SEBELUM TGLAWAL YG DIINPUT MASIH ADA BARANG YANG HARUS DIHITUNG ULANG ATAU TIDAK
            ''JIKA MASIH ADA YG HARUS DIHITUNG ULANG MAKA ADA PERINGATAN
            ''BARANG HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/IB/GRN/RI/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            ''DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            ''DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            'Dim tgl As String = "", tglHistory As String = ""

            ''1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            ''sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtTgl.Rows.Count > 0 Then
            '    tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            'End If

            ''2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            ''sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            'Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            'If dtTglHistory.Rows.Count > 0 Then
            '    tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            'End If

            ''MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            'If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '    If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '        'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
            '        result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            '    Else
            '        'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
            '        result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            '    End If
            'ElseIf Len(tgl) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION
            '    result(2) = "Date " & AsFormatTanggal(tgl) & " must be recalculated." : GoTo selesai
            'ElseIf Len(tglHistory) > 0 Then
            '    'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA HARUS HITUNG ULANG MULAI TGL M1_ITEM_TRANSACTION_HISTORY
            '    result(2) = "Date " & AsFormatTanggal(tglHistory) & " must be recalculated." : GoTo selesai
            'End If

            'Else
            '    'JIKA HITUNG ULANG PERBARANG MAKA AMBIL TGLAWAL BERDASARKAN :

            '    'BARANG SESUAI FILTER DAN HPP AVERAGE DAN BUKAN JASA, HPPFIX = 0 AND (SUMBER = SA/GRN/PRT/SI/SR/PD) AND TGL < TGLAWAL LIMIT 1
            '    'DATA DIAMBILKAN DARI M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            '    'DIAMBIL TANGGAL YANG TERKECIL DARI KEDUA TABEL TERSEBUT
            '    Dim tgl As String = "", tglHistory As String = ""

            '    '1. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION
            '    'sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    sql = "SELECT it.tgl FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    Dim dtTgl As DataTable = AsDataTableAmbilDariDB(sql)
            '    If dtTgl.Rows.Count > 0 Then
            '        tgl = AsFormatTanggal(dtTgl.Rows(0)(0))
            '    End If

            '    '2. AMBIL TANGGAL DARI M1_ITEM_TRANSACTION_HISTORY
            '    'sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    sql = "SELECT it.tgl FROM m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 WHERE it.hppfix = 0 AND it.tgl < '" & tglAwal & "' AND it.idbarang = '" & idbarang & "' ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail LIMIT 1"
            '    Dim dtTglHistory As DataTable = AsDataTableAmbilDariDB(sql)
            '    If dtTglHistory.Rows.Count > 0 Then
            '        tglHistory = AsFormatTanggal(dtTglHistory.Rows(0)(0))
            '    End If

            '    'MEBANDINGKAN TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY
            '    If Len(tgl) > 0 And Len(tglHistory) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION DAN M1_ITEM_TRANSACTION_HISTORY MAKA CEK TGL TERKECIL
            '        If Date.Parse(tgl) < Date.Parse(tglHistory) Then
            '            'JIKA TGL M1_ITEM_TRANSACTION < M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '            tglAwal = tgl
            '        Else
            '            'JIKA TGL M1_ITEM_TRANSACTION >= M1_ITEM_TRANSACTION_HISTORY MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '            tglAwal = tglHistory
            '        End If
            '    ElseIf Len(tgl) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION
            '        tglAwal = tgl
            '    ElseIf Len(tglHistory) > 0 Then
            '        'JIKA TERDAPAT TGL M1_ITEM_TRANSACTION_HISTORY SAJA MAKA TGLAWAL = TGL M1_ITEM_TRANSACTION_HISTORY
            '        tglAwal = tglHistory
            '    End If

        End If
        'END OF SET TGLAWAL --------------------------------------------------------


        'PROSES HITUNG ULANG =========================================================

        Try

            ''CEK PERIODE AKUNTANSI ---------------------------------------------
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAkhir))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI --------------------------------------


            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, i.bkode "
            'sql &= " FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
            sql &= " FROM m1_item_transaction it "
            'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
            sql &= " JOIN m1_item i ON it.idbarang = i.bid "
            sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
            sql &= " WHERE it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' "
            If hitungPerBarang Then
                sql &= " AND it.idbarang = '" & idbarang & "' "
            End If
            sql &= " ORDER BY it.id "
            Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql)
            'AMBIL DATA BARANG HITUNG ULANG ------------------------------------


            'PROSES HITUNG ULANG -----------------------------------------------
            If dtBarang.Rows.Count > 0 Then

                'DATATABLE SALDO AWAL
                Dim dtSaldo As New DataTable, dtCurrSaldo As New DataTable, currUrutan As Double = 0, saUrutan As Double = 0
                Dim sqlSAwal As String = ""

                'DATATABLE BARANG MASUK SPESIAL (PD, SI Assembly Langsung, SR Ambil SI)
                Dim dtHppMasukSpesial As New DataTable

                'VARIABEL TANGGAL SEBELUMNYA
                Dim tglBefore As String = tglAwal

                'VARIABEL DATA BARANG
                Dim jenismutasi As Integer = 0, tgl As String = "", inputtgl As String = "", sumber As String = ""
                Dim idutama As Integer = 0, iddetail As Integer = 0, customint10 As Integer = 0

                'VARIABEL SALDO AWAL
                Dim saldoawaljml As Double = 0, saldoawalhpp As Double = 0, saldoawalnilai As Double = 0

                'VARIABEL SALDO YANG DIHITUNG
                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0
                Dim hppmasuk As Double = 0, hppkeluar As Double = 0, nilaimasuk As Double = 0, nilaikeluar As Double = 0

                'VARIABEL SALDO HASIL HITUNG
                Dim saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                'VARIABEL UPDATE KE TABEL TRANSAKSI MASING-MASING
                Dim HppTrans As Double = 0

                'DATATABLE SALDO AKHIR
                Dim dtSaldoAkhir As New DataTable

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                Dim objCmd2 As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans2 As MySql.Data.MySqlClient.MySqlTransaction
                Dim drBarang As DataRow

                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    drBarang = dtBarang.Rows(i)

                    '*** Open Connection ***'  
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                    myConn.Open()

                    '*** Start Transaction ***'  
                    Trans2 = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try

                        'STEPKE
                        stepKe = stepKe + 1

                        'STEP DETAIL
                        stepDetail = 1

                        'RESET NILAI VARIABEL SALDO HASIL HITUNG
                        saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        'SET DATA BARANG
                        id = Integer.Parse(FxDB(drBarang("id"), 0))
                        idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
                        kodebarang = FxDB(drBarang("bkode"), "")
                        jenismutasi = Integer.Parse(FxDB(drBarang("jenismutasi"), 0))
                        tgl = AsFormatTanggal(FxDB(drBarang("tgl"), "1900-01-01"), "yyyy-MM-dd")
                        inputtgl = AsFormatTanggal(FxDB(drBarang("inputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")
                        sumber = FxDB(drBarang("sumber"), "")
                        notransaksi = FxDB(drBarang("notransaksi"), "")
                        idutama = Integer.Parse(FxDB(drBarang("idutama"), 0))
                        iddetail = Integer.Parse(FxDB(drBarang("iddetail"), 0))
                        customint10 = Integer.Parse(FxDB(drBarang("customint10"), 0))

                        'STEP DETAIL
                        stepDetail = 2

                        'SET SALDO YANG DIHITUNG
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'jmlmasuk = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlkeluar = 0
                            'hppmasuk = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppkeluar = 0
                            'nilaimasuk = Math.Round(jmlmasuk * hppmasuk, 2) : nilaikeluar = 0
                            jmlmasuk = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlkeluar = 0
                            hppmasuk = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppkeluar = 0
                            nilaimasuk = jmlmasuk * hppmasuk : nilaikeluar = 0

                        Else
                            'JIKA BARANG KELUAR
                            'jmlkeluar = Math.Round(Double.Parse(FxDB(drBarang("jmlbarang"), 0)), 2) : jmlmasuk = 0
                            'hppkeluar = Math.Round(Double.Parse(FxDB(drBarang("hpp"), 0)), 2) : hppmasuk = 0
                            'nilaikeluar = Math.Round(jmlkeluar * hppkeluar, 2) : nilaimasuk = 0
                            jmlkeluar = Double.Parse(FxDB(drBarang("jmlbarang"), 0)) : jmlmasuk = 0
                            hppkeluar = Double.Parse(FxDB(drBarang("hpp"), 0)) : hppmasuk = 0
                            nilaikeluar = jmlkeluar * hppkeluar : nilaimasuk = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 3

                        'AMBIL SALDO AWAL
                        'AMBIL SALDO AWAL
                        sqlSAwal = "  SELECT @zurut := @zurut + 1 as idurut, it.id, it.saldojml, it.saldohpp, it.saldonilai "
                        sqlSAwal &= " FROM m0_hppsaldo it, (SELECT @zurut := 0) AS VariableInit "
                        'sqlSAwal &= " FROM m1_item_transaction it "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        'sqlSAwal &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        sqlSAwal &= " WHERE it.isclose = 0  AND it.idbarang = '" & FixDouble(idbarang) & "' "
                        sqlSAwal &= " AND it.id < '" & FixQuotes(id) & "' "
                        'sqlSAwal &= " AND it.inputtgl <= '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND (CASE "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail < '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.sumber = '" & FixQuotes(sumber) & "' "
                        'sqlSAwal &= " AND it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " AND it.jenismutasi <> '" & FixDouble(jenismutasi) & "' "
                        'sqlSAwal &= " AND it.idutama = '" & FixDouble(idutama) & "' "
                        'sqlSAwal &= " THEN it.iddetail > '" & FixDouble(iddetail) & "' "
                        'sqlSAwal &= " WHEN it.tgl = '" & FixQuotes(tgl) & "' "
                        'sqlSAwal &= " AND it.inputtgl = '" & FixQuotes(inputtgl) & "' "
                        'sqlSAwal &= " THEN it.id < '" & FixDouble(id) & "' "
                        'sqlSAwal &= " ELSE it.id LIKE '%' "
                        'sqlSAwal &= " END) "
                        'sqlSAwal &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC "
                        sqlSAwal &= " ORDER BY it.id "
                        'sqlSAwal &= " LIMIT 1"
                        dtSaldo = AsDataTableAmbilDariDB(sqlSAwal)

                        'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                        If dtSaldo.Rows.Count > 0 Then
                            'AMBIL NO URUT SESUAI ID TRANSAKSI BARANG YANG SEDANG DIHITUNG
                            currUrutan = AsDataTableDLookup(dtSaldo, "idurut", "id = '" & id & "'", 0)

                            'JIKA NO URUT = 0 MAKA SALDO AWAL = URUTAN TERBESAR DARI DATA SALDO AWAL
                            If currUrutan = 0 Then
                                'saldoawaljml = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0)), 2)
                                'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0)), 2)
                                'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0)), 2)
                                saldoawaljml = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldojml"), 0))
                                saldoawalhpp = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldohpp"), 0))
                                saldoawalnilai = Double.Parse(FxDB(dtSaldo.Rows(dtSaldo.Rows.Count - 1)("saldonilai"), 0))

                                'JIKA NO URUT > 1 MAKA SALDO AWAL = NO URUT - 1
                            ElseIf currUrutan > 1 Then
                                'AMBIL DATA SALDO AWAL SESUAI URUTAN SALDO AWAL
                                dtCurrSaldo = AsDataTableFilterSortDt(dtSaldo, "idurut = '" & currUrutan - 1 & "'")

                                'JIKA TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL
                                If dtCurrSaldo.Rows.Count > 0 Then
                                    'saldoawaljml = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0)), 2)
                                    'saldoawalhpp = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0)), 2)
                                    'saldoawalnilai = Math.Round(Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0)), 2)
                                    saldoawaljml = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldojml"), 0))
                                    saldoawalhpp = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldohpp"), 0))
                                    saldoawalnilai = Double.Parse(FxDB(dtCurrSaldo.Rows(0)("saldonilai"), 0))

                                Else
                                    'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
                                    GoTo setSaldoAwalNol
                                End If

                                'JIKA NO URUT = 1 MAKA SALDO AWAL = 0
                            Else
                                'JIKA URUTAN SALDO AWAL <= 0 MAKA SET NILAI SALDO AWAL NOL
                                GoTo setSaldoAwalNol

                            End If

                        Else
                            'JIKA TIDAK TERDAPAT SALDO AWAL MAKA SET NILAI SALDO AWAL NOL
setSaldoAwalNol:
                            saldoawaljml = 0 : saldoawalhpp = 0 : saldoawalnilai = 0

                        End If

                        'STEP DETAIL
                        stepDetail = 4

                        ''AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                        ''PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                        'If jenismutasi = 1 And sumber = "PD" Then
                        '    'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                        '    sql = "  SELECT ((pdi.hpppersen / 100) * IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0)) / pdi.jmlbarang as hpp "
                        '    sql &= " FROM m6_pd_in pdi "
                        '    sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd "
                        '    sql &= " AND pdi.idpd = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND pdi.idbarang = '" & FixDouble(idbarang) & "'"
                        '    sql &= " GROUP BY pdi.idpdin "
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'ElseIf jenismutasi = 1 And sumber = "SI" Then
                        '    'JIKA TRANSAKSI SI ASSEMBLY LANGSUNG, MAKA HITUNG HPP MASUK BERDASARKAN HPP BARANG PENYUSUN
                        '    sql = "  SELECT IFNULL(SUM(sim.jmlbarang * sim.hpp),0) / sid.jmlbarang as hpp "
                        '    sql &= " FROM m5_si_detail sid "
                        '    sql &= " JOIN m5_si_material sim ON sid.idsidetail = sim.idsidetail "
                        '    sql &= " AND sid.idsi = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND sid.idsidetail = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND sid.idbarang = '" & FixDouble(idbarang) & "'"
                        '    sql &= " GROUP BY sid.idsidetail "
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'ElseIf jenismutasi = 1 And sumber = "SR" Then
                        '    'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                        '    sql = "  SELECT sid.hpp as hpp "
                        '    sql &= " FROM m5_sr_detail srd "
                        '    sql &= " JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail "
                        '    sql &= " AND srd.idsr = '" & FixDouble(idutama) & "'"
                        '    sql &= " AND srd.idsrdetail = '" & FixDouble(iddetail) & "'"
                        '    sql &= " AND srd.idbarang = '" & FixDouble(idbarang) & "'"
                        '    dtHppMasukSpesial = AsDataTableAmbilDariDB(sql)
                        '    If dtHppMasukSpesial.Rows.Count > 0 Then
                        '        If Len(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)) > 0 Then
                        '            hppmasuk = Math.Round(Double.Parse(FxDB(dtHppMasukSpesial.Rows(0)("hpp"), 0)), 2)
                        '        Else
                        '            hppmasuk = 0
                        '        End If
                        '    End If

                        'End If

                        'STEP DETAIL
                        stepDetail = 5

                        'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                        If jenismutasi = 1 Then
                            'JIKA BARANG MASUK
                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) + (hppmasuk * jmlmasuk), 2)
                            'saldonilai = Math.Round((saldoawalnilai) + (hppmasuk * jmlmasuk), 2)
                            saldonilai = (saldoawalnilai) + (hppmasuk * jmlmasuk)

                        Else
                            ''JIKA BARANG KELUAR
                            'If sumber <> "PRT" Then
                            '    'SELAIN PRT, HPP AMBIL DARI HPP TERAKHIR
                            '    hppkeluar = Math.Round(saldoawalhpp, 2)

                            'End If

                            'saldonilai = Math.Round((saldoawalhpp * saldoawaljml) - (hppkeluar * jmlkeluar), 2)
                            'saldonilai = Math.Round((saldoawalnilai) - (hppkeluar * jmlkeluar), 2)
                            saldonilai = (saldoawalnilai) - (hppkeluar * jmlkeluar)

                        End If

                        'STEP DETAIL
                        stepDetail = 6

                        'HITUNG SALDOJML
                        'saldojml = Math.Round(saldoawaljml + (jmlmasuk - jmlkeluar), 2)
                        saldojml = saldoawaljml + (jmlmasuk - jmlkeluar)

                        'PEMBULATAN HASIL PERHITUNGAN SALDONILAI DAN HITUNG SALDOHPP
                        If saldojml <> 0 Then
                            'saldonilai = Math.Round(saldonilai, 2)
                            'saldohpp = Math.Round(saldonilai / saldojml, 2)
                            saldonilai = saldonilai
                            saldohpp = saldonilai / saldojml

                        Else
                            saldonilai = 0
                            saldohpp = 0

                        End If

                        'PEMBULATAN HPP
                        'hppmasuk = Math.Round(hppmasuk, 2)
                        'hppkeluar = Math.Round(hppkeluar, 2)
                        hppmasuk = hppmasuk
                        hppkeluar = hppkeluar

                        'STEP DETAIL
                        stepDetail = 7

                        ''UPDATE HPP KE TABEL TRANSAKSI MASING-MASING
                        ''SA/IB/GRN/RI/PRT/SI/SR/PD/LU/LB/AK/RO
                        ''SET HPP UNTUK TABEL TRANSAKSI MASING-MASING
                        'If jenismutasi = 1 Then
                        '    'JIKA BARANG MASUK MAKA AMBIL HPPMASUK
                        '    HppTrans = hppmasuk

                        'Else
                        '    'JIKA BARANG KELUAR MAKA AMBIL HPPKELUAR
                        '    HppTrans = hppkeluar

                        'End If

                        ''UPDATE KE TABEL TRANSAKSI BERDASARKAN SUMBER TRANSAKSI
                        'Select Case sumber.ToUpper
                        '    Case "SA"
                        '        sql = "UPDATE m3_sa_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsa = '" & FixDouble(idutama) & "' AND idsadetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "IB"
                        '        sql = "UPDATE m3_ib_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idib = '" & FixDouble(idutama) & "' AND idibdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "GRN"
                        '        sql = ""

                        '    Case "RI"
                        '        sql = ""

                        '    Case "PRT"
                        '        sql = "UPDATE m4_prt_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idprt = '" & FixDouble(idutama) & "' AND idprtdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "SI"
                        '        'SI ADA BARANG ASSEMBLY LANGSUNG
                        '        If jenismutasi = 0 And customint10 = -2 Then
                        '            'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                        '            sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsimaterial = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        ElseIf jenismutasi = 1 And customint10 = -1 Then
                        '            'SI BARANG HASIL MASUK      (customint10 = -1), UPDATE KE TABEL M5_SI_DETAIL
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        ElseIf jenismutasi = 0 And customint10 = 0 Then
                        '            'SI BARANG HASIL KELUAR     (customint10 =  0), UPDATE KE TABEL M5_SI_DETAIL
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        Else
                        '            'sql = ""
                        '            sql = "UPDATE m5_si_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsi = '" & FixDouble(idutama) & "' AND idsidetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        End If

                        '    Case "SR"
                        '        sql = "UPDATE m5_sr_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idsr = '" & FixDouble(idutama) & "' AND idsrdetail = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '    Case "PD"
                        '        'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                        '        If jenismutasi = 1 Then
                        '            'JIKA MASUK MAKA UPDATE TABEL M6_PD_IN
                        '            sql = "UPDATE m6_pd_in SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdin = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        Else
                        '            'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                        '            sql = "UPDATE m6_pd_out SET hpp = '" & FixDouble(HppTrans) & "' WHERE idpd = '" & FixDouble(idutama) & "' AND idpdout = '" & FixDouble(iddetail) & "' AND idbarang = '" & FixDouble(idbarang) & "' "

                        '        End If

                        '    Case "LU"
                        '        sql = ""

                        '    Case "LB"
                        '        sql = ""

                        '    Case "AK"
                        '        sql = "UPDATE m_11_ak_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idak = '" & FixDouble(idutama) & "' AND idakdetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                        '    Case "RO"
                        '        sql = "UPDATE m_11_ro_detail SET hpp = '" & FixDouble(HppTrans) & "' WHERE idro = '" & FixDouble(idutama) & "' AND idrodetail = '" & FixDouble(iddetail) & "' AND idlayanan = '" & FixDouble(idbarang) & "' "

                        '    Case Else
                        '        sql = ""
                        'End Select

                        ''EKSEKUSI SQL UPDATE TABEL TRANSAKSI MASING-MASING
                        'If Len(sql) > 0 Then
                        '    objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        '    With objCmd2
                        '        .Connection = myConn
                        '        .Transaction = Trans2
                        '        .CommandType = CommandType.Text
                        '        .CommandText = sql
                        '    End With
                        '    objCmd2.ExecuteNonQuery()
                        'End If

                        'STEP DETAIL
                        stepDetail = 8

                        'UPDATE TRANSAKSI BARANG
                        sql = "  UPDATE m1_item_transaction it "
                        sql &= " SET "
                        'sql &= " it.hpp = '" & FixDouble(HppTrans) & "' "
                        sql &= "  it.saldojml = '" & FixDouble(saldojml) & "' "
                        sql &= " , it.saldohpp = '" & FixDouble(saldohpp) & "' "
                        sql &= " , it.saldonilai = '" & FixDouble(saldonilai) & "' "
                        'sql &= " , it.jurnalfix = '0' "
                        'sql &= " , it.updatehpp = '1' "
                        'If hitungPerBarang = False Then
                        '    'JIKA HITUNG ULANG SEMUA BARANG MAKA SET HPPFIX = 1
                        '    sql &= " , it.hppfix = '1' "
                        'End If
                        sql &= " WHERE it.id = '" & FixDouble(id) & "' "
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 9

                        ' ''UPDATE KE MASTER BARANG JIKA BARIS INI MERUPAKAN BARIS TERAKHIR PADA TRANSAKSI BARANG
                        ' ''AMBILSALDO AKHIR
                        ''sql = "  SELECT it.id "
                        ''sql &= " FROM m1_item_transaction it "
                        ' ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' "
                        ''sql &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' "
                        ''sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
                        ''sql &= " WHERE it.idbarang = '" & FixDouble(idbarang) & "' "
                        ''sql &= " ORDER BY it.tgl DESC, it.inputtgl DESC, it.customint10 DESC, it.jenismutasi DESC, it.idutama DESC, it.iddetail DESC"
                        ''sql &= " LIMIT 1"
                        ''dtSaldoAkhir = AsDataTableAmbilDariDB(sql)
                        ''If dtSaldoAkhir.Rows.Count > 0 Then
                        ''    If Len(FxDB(dtSaldoAkhir.Rows(0)("id"), 0)) > 0 Then
                        ''        'JIKA ID TRANSAKSI BARANG YG SEDANG DIPROSES = ID TRANSAKSI BARANG TERAKHIR MAKA UPDATE MASTER BARANG
                        ''        If FxDB(dtSaldoAkhir.Rows(0)("id"), 0) = id Then
                        'sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bedithpp = 0 WHERE bid = '" & FixDouble(idbarang) & "'"
                        'objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        'With objCmd2
                        '    .Connection = myConn
                        '    .Transaction = Trans2
                        '    .CommandType = CommandType.Text
                        '    .CommandText = sql
                        'End With
                        'objCmd2.ExecuteNonQuery()
                        ''        End If
                        ''    End If

                        ''End If

                        'STEP DETAIL
                        stepDetail = 10

                        ''UPDATE HISTORI TRANSAKSI BARANG
                        'If hitungPerBarang = False Then
                        '    'JIKA HITUNG ULANG SEMUA BARANG DAN TANGGAL SEBELUMNYA <> TANGGAL TRANSAKSI YG SEDANG DIHITUNG
                        '    'MAKA UPDATE HPPFIX = 0 PADA HISTORI TRANSAKSI BARANG
                        '    If tglBefore <> tgl Then
                        '        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        '        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tglBefore) & "'"
                        '    ElseIf stepKe >= dtBarang.Rows.Count Then
                        '        'sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        '        sql = "UPDATE m1_item_transaction_history it JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' SET it.hppfix = '1' WHERE it.tgl <= '" & FixQuotes(tgl) & "'"
                        '    Else
                        '        sql = ""
                        '    End If
                        '    If Len(sql) > 0 Then
                        '        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        '        With objCmd2
                        '            .Connection = myConn
                        '            .Transaction = Trans2
                        '            .CommandType = CommandType.Text
                        '            .CommandText = sql
                        '        End With
                        '        objCmd2.ExecuteNonQuery()
                        '    End If

                        '    'PERBARUI TGL BEFORE
                        '    tglBefore = tgl
                        'End If

                        'STEP DETAIL
                        stepDetail = 11

                        'INSERT KE M0_HPPSALDO
                        sql = "  INSERT INTO m0_hppsaldo(id, jenismutasi, sumber, idutama, iddetail, tgl, idbarang, saldojml, saldohpp, saldonilai, inputtgl, customint10) "
                        sql &= " VALUES('" & FixDouble(id) & "', '" & FixDouble(jenismutasi) & "', '" & FixQuotes(sumber) & "', '" & FixDouble(idutama) & "', '" & FixDouble(iddetail) & "', '" & FixQuotes(AsFormatTanggal(tgl)) & "', '" & FixDouble(idbarang) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixDouble(AsFormatTanggal(inputtgl, "yyyy-MM-dd H:mm:ss")) & "', '" & FixDouble(customint10) & "')"
                        sql &= " ON DUPLICATE KEY UPDATE id = VALUES(id), jenismutasi = VALUES(jenismutasi), sumber = VALUES(sumber), idutama = VALUES(idutama), iddetail = VALUES(iddetail), tgl = VALUES(tgl), idbarang = VALUES(idbarang), saldojml = VALUES(saldojml), saldohpp = VALUES(saldohpp), saldonilai = VALUES(saldonilai), inputtgl = VALUES(inputtgl), customint10 = VALUES(customint10) "
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert COGS Average." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()

                        'STEP DETAIL
                        stepDetail = 12

                        'INSERT KE TABEL LOG SUKSES
                        sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
                        sql &= " VALUES(0, 2, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & "', 2)"
                        'sql &= " VALUES(0, 0, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & " - " & FixQuotes(sqlSAwal) & "', 2)"
                        'If AsEksekusiSQL(sql) = False Then
                        '    result(2) = "Failed insert log #1." : Trans2.Rollback() : GoTo selesai
                        'End If
                        objCmd2 = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd2
                            .Connection = myConn
                            .Transaction = Trans2
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd2.ExecuteNonQuery()


                        Trans2.Commit()  '*** Commit Transaction ***'
                        result(1) = 1
                        result(2) = ""
                        result(3) = stepKe
                        result(4) = result(4)

                    Catch ex As Exception

                        Trans2.Rollback() '*** RollBack Transaction ***'  
                        result(1) = 0
                        result(2) = ex.Message
                        result(3) = 0
                        result(4) = result(4)
                        GoTo selesai

                    Finally
                        myConn.Close()

                    End Try

                Next
            End If

            result(1) = 1
            result(2) = ""
            result(3) = stepKe
            result(4) = result(4)
            'END OF PROSES HITUNG ULANG ----------------------------------------

        Catch ex As Exception

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try
        'END OF PROSES JURNAL ULANG ==================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "stepke : " & FixDouble(stepKe) & ", Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". step detail : " & FixDouble(stepDetail) & "."

            'INSERT KE TABEL LOG GAGAL
            sql = "  INSERT INTO m0_hitungulang_log (huid, hujenis, huinputtgl, huke, huidtransaksibarang, hucatatan, hustatus) "
            sql &= " VALUES(0, 2, NOW(), 1, '" & FixDouble(id) & "', 'stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", " & FixQuotes(result(2)) & "', 3)"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "stepke : " & FixDouble(stepKe) & ", " & FixQuotes(notransaksi) & " - " & FixQuotes(kodebarang) & ". step detail : " & FixDouble(stepDetail) & ", Failed insert log #2."
            End If

        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

End Class
