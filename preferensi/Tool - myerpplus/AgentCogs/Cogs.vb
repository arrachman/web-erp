Public Class Cogs

    Dim userid As String = ""   'User Id diisi dengan user yang melakukan proses transaksi
    Dim formatTglDB = "yyyy-MM-dd"
    Dim formatTglWaktuDB = "yyyy-MM-dd H:mm:ss"

    Public Function M0_Cogs(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging

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

        ''VALIDASI WEBSITEACCESSKEY =========================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
        '    result(2) = "Access denied for delete data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDMSMQ, SUMBER DAN IDTRANSAKSI
        Dim msmqId As String = "", sumber As String = "", idtransaksi As String = ""
        Dim idtrans(3) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 3) Then
            result(2) = "Invalid key parameter." : GoTo selesai
        Else
            'CEK IDMSMQ
            If (Len(idtrans(0)) = 0) Then
                result(2) = "MSMQ ID can't be empty." : GoTo selesai
            Else
                msmqId = idtrans(0)
            End If
            'CEK SUMBER
            If (Len(idtrans(1)) = 0) Then
                result(2) = "Sumber can't be empty." : GoTo selesai
            Else
                sumber = idtrans(1)
            End If
            'CEK IDTRANSAKSI
            If (IsNumeric(idtrans(2)) = False) Then
                result(2) = "ID Transaksi required numeric." : GoTo selesai
            Else
                idtransaksi = idtrans(2)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'SIMPAN KE DATABASE ================================================================
        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        Dim rsCogs As String = "" 'progress?errMessage?sqlhpp?sqlupdateposting
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""

        'PANGGIL HPP BERDASARKAN SUMBER
        Select Case sumber.ToUpper

            '************ M3 ****************
            Case "SA" : rsCogs = M3_Sa(idtransaksi)
            Case "IB" : rsCogs = M3_Ib(idtransaksi)

                '************ M4 ****************
            Case "GRN" : rsCogs = M4_Grn(idtransaksi)
            Case "RI" : rsCogs = M4_Ri(idtransaksi)
            Case "PRT" : rsCogs = M4_Prt(idtransaksi)

                '************ M5 ****************
            Case "SI" : rsCogs = M5_Si(idtransaksi)
            Case "RNR" : rsCogs = M5_Rnr(idtransaksi)
            Case "SR" : rsCogs = M5_Sr(idtransaksi)

                '************ M6 ****************
            Case "PD" : rsCogs = M6_Pd(idtransaksi)

                '************ M11 ***************
            Case "AK" : rsCogs = M11_Ak(idtransaksi)
            Case "RO" : rsCogs = M11_Ro(idtransaksi)

            Case Else
                result(2) = "Invalid Packet. (" & sumber & ")" : GoTo selesai
        End Select

        'SPLIT HASIL HPP (progress, errmessage, sql)
        Dim sptRsCogs() As String = rsCogs.Split(sptSubParam)
        If sptRsCogs.Length <> 3 Then
            result(2) = "Invalid cogs result." : GoTo selesai
        Else
            rsProgress = sptRsCogs(0)
            rsErrMessage = sptRsCogs(1)
            rsSql = sptRsCogs(2)
        End If

        'JIKA HASIL HPP TIDAK ERROR MAKA EKSEKUSI SQL
        'If rsProgress = 1 Then

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'EKSEKUSI SQL SESUAI BANYAKNYA QUERY HASIL HPP
            Dim sptRsSql() As String = rsSql.Split(sptRow)
            For i As Integer = 1 To sptRsSql.Length
                sql = sptRsSql(i - 1)
                'JIKA SQL TERISI MAKA DI EKSEKUSI
                If Len(sql) > 0 Then
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'Else
                    '    result(2) = "Invalid cogs : " & i - 1 & "." : Trans.Rollback() : GoTo selesai
                End If
            Next

            'JIKA HASIL HPP TIDAK ERROR MAKA UPDATE STATUS TABEL MSQMQ HPP = SUKSES(2), ELSE = GAGAL(3)
            Dim mcprogress As Integer = 0
            If rsProgress = 1 Then
                'JIKA 1 MAKA SUKSES(2)
                mcprogress = 2
            ElseIf rsProgress = 4 Then
                'JIKA 4 MAKA TRANSAKSI TDK APPROVED(4)
                mcprogress = 4
            Else
                'SELAIN 1 DAN 4 MAKA GAGAL(3)
                mcprogress = 3
            End If

            'UPDATE STATUS TABEL MSQMQ HPP
            'sql = "UPDATE m0_msmq_cogs SET mcprogress = '" & mcprogress & "', mcpesan = '" & FixQuotes(rsErrMessage) & "', mctglselesai = NOW() WHERE mcid = '" & msmqId & "'"
            sql = "UPDATE m0_msmq_cogs SET mcprogress = (CASE mcid WHEN '" & msmqId & "' THEN '" & mcprogress & "' ELSE 4 END), mcpesan = (CASE mcid WHEN '" & msmqId & "' THEN '" & FixQuotes(rsErrMessage) & "' ELSE '' END), mctglselesai = NOW() WHERE mcsumber = '" & sumber & "' AND mcidtransaksi = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'UPDATE STATUS TABEL MSQMQ HPP
            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = rsProgress
            result(2) = rsErrMessage
            result(3) = 0
            result(4) = result(4)

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        myConn.Close()
        'END OF SIMPAN KE DATABASE =========================================================

        'Else
        'result(2) = rsErrMessage : GoTo selesai
        'End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam)

        Return wsResult
    End Function

    Public Function M0_CogsHitungUlang_Fifo(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim isUpdate As Boolean, sql As String = "", stepKe As Double = 0
        Dim tglAwal As String = "", tglAkhir As String = ""
        Dim kodeBarangAwal As String = "", kodeBarangAkhir As String = ""
        Dim hitungPerBarang As Boolean = False

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


        ''VALIDASI WEBSITEACCESSKEY =========================================================
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ==================================================


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


        'PROSES HITUNG ULANG =========================================================
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'CEK PERIODE AKUNTANSI ---------------------------------------------
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglAwal), AsFormatTanggal(tglAwal))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI --------------------------------------


            ''CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR ---------------------------
            ''-- TIDAK BOLEH LEBIH DARI BATAS TGL
            'Dim batasTgl As Double = 120
            'Dim jarakTgl As Double = DateDiff(DateInterval.Day, Date.Parse(tglAwal), Date.Parse(tglAkhir))
            'If jarakTgl > batasTgl Then
            'result(2) = "Difference between Start Date and End Date should not be more than " & batasTgl & " days. (" & jarakTgl & " days)" : Trans.Rollback() : GoTo selesai
            'End If
            ''END OF CEK JARAK ANTARA TGL AWAL DAN TGL AKHIR --------------------


            'CEK TRANSAKSI SEBELUM TGL AWAL ------------------------------------ 
            '-- APAKAH MASIH ADA TRANSAKSI YANG HARUS DIHITUNG ULANG
            'BUAT QUERY
            sql = "  SELECT DATE(postingtgl) as postingtgl"
            sql &= " FROM M1_Item_Transaction"
            sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bhpp = 'F'"
            sql &= " JOIN M0_Nomor ON sumber = kodetabel AND transaksihpp = 1"
            sql &= " WHERE hppfix = '0'"
            sql &= " AND DATE(postingtgl) < '" & FixQuotes(tglAwal) & "'"
            'FILTER KODEBARANG
            If Len(kodeBarangAwal) > 0 And Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                sql &= " AND bkode BETWEEN '" & FixQuotes(kodeBarangAwal) & "' AND '" & FixQuotes(kodeBarangAkhir) & "'"
            ElseIf Len(kodeBarangAwal) > 0 Then
                'JIKA KODE BARANG AWAL SAJA YANG DIISI MAKA FILTER >= KODE BARANG AWAL
                sql &= " AND bkode >= '" & FixQuotes(kodeBarangAwal) & "'"
            ElseIf Len(kodeBarangAkhir) > 0 Then
                'JIKA KODE BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= KODE BARANG AKHIR
                sql &= " AND bkode <= '" & FixQuotes(kodeBarangAkhir) & "'"
            End If
            'ORDER BY
            'sql &= " ORDER BY postingtgl ASC, jenismutasi ASC, id ASC"
            sql &= " ORDER BY id ASC"
            'LIMIT
            sql &= " LIMIT 1"

            Dim dtCekTgl As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtCekTgl.Rows.Count > 0 Then
                result(2) = "Date " & AsFormatTanggal(dtCekTgl.Rows(0)("postingtgl")) & " must be recalculated first." : GoTo selesai
            End If
            'END OF CEK TRANSAKSI SEBELUM TGL AWAL -----------------------------


            'CEK HITUNG ULANG PERBARANG ATAU BUKAN -----------------------------
            'JIKA KODEBARANG AWAL ATAU KODEBARANG AKHIR DIISI MAKA HITUNG ULANG PERBARANG
            '-- ARITNYA, HITUNG ULANG PERBARANG MASIH BELUM BENAR, HPP BELUM FIX
            If Len(kodeBarangAwal) > 0 Or Len(kodeBarangAkhir) > 0 Then
                hitungPerBarang = True
            Else
                hitungPerBarang = False
            End If
            'END OF CEK HITUNG ULANG PERBARANG ATAU BUKAN ----------------------


            'UPDATE JMLKELUAR PADA HPP FIFO MASUK ------------------------------
            'MENGURANGI JMLKELUAR PADA HPP FIFO MASUK SESUAI JMLKELUAR DARI HPP FIFO KELUAR
            'DIMANA TGLINPUT HPP FIFO KELUAR >= TGLAWAL
            sql = "  UPDATE m1_cogs_fifo_in cfi"
            sql &= " JOIN"
            sql &= " ("
            sql &= " SELECT cfo.cfoidcfi, SUM(cfo.cfojmlkeluar) as jmlkeluar"
            sql &= " FROM m1_cogs_fifo_out cfo"
            sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
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
            sql &= " GROUP BY cfo.cfoidcfi"
            sql &= " ) as fifoOut"
            sql &= " ON cfi.cfiid = fifoOut.cfoidcfi"
            sql &= " SET cfi.cfijmlkeluar = cfi.cfijmlkeluar - fifoOut.jmlkeluar"

            'TAMBAHKAN QUERY UPDATE HPP FIFO MASUK
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF UPDATE JMLKELUAR PADA HPP FIFO MASUK -----------------------


            'DELETE HPP FIFO KELUAR --------------------------------------------
            sql = "  DELETE cfo"
            sql &= " FROM m1_cogs_fifo_out cfo"
            sql &= " JOIN M1_Item i ON cfo.cfoidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
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
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF DELETE HPP FIFO KELUAR -------------------------------------


            'DELETE HPP FIFO MASUK ---------------------------------------------
            sql = "  DELETE cfi"
            sql &= " FROM m1_cogs_fifo_in cfi"
            sql &= " JOIN M1_Item i ON cfi.cfiidbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
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
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF DELETE HPP FIFO MASUK --------------------------------------


            'HITUNG ULANG TRANSAKSI BARANG -------------------------------------
            'BUAT QUERY
            sql = "  SELECT it.id, it.sumber, it.idutama, it.iddetail, it.jenismutasi, it.idbarang, i.bkode as kodebarang, it.tipebarang, it.namabarang, it.jmlbarang, it.satuanbarang, it.hpp, it.saldojml, it.saldohpp, it.saldonilai, it.postingtgl "
            sql &= " FROM M1_Item_Transaction it"
            sql &= " JOIN M1_Item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp = 'F'"
            sql &= " JOIN M0_Nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
            sql &= " WHERE DATE(it.postingtgl) >= '" & FixQuotes(tglAwal) & "'"
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
            sql &= " ORDER BY it.id ASC"

            Dim dtTrans As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtTrans.Rows.Count > 0 Then
                Dim currRow As Double = 0

                Dim dtSA As New DataTable
                Dim SAJml As Double = 0, SAHpp As Double = 0, SANilai As Double = 0

                Dim dtPD As New DataTable, totalHpp As Double = 0, hppPersen As Double = 0

                Dim id As Double = 0, sumber As String = "", idutama As Double = 0, iddetail As Double = 0, postingtgl As String = ""
                Dim jenismutasi As Double = 0, idbarang As Double = 0, jmlbarang As Double = 0
                Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuanbarang As String = ""
                Dim hpp As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0, sisa As Double = 0

                Dim dtFifo As New DataTable, dtCekFifo As New DataTable
                Dim strFifo As New StringBuilder, strIdHppFifo As New StringBuilder

                For Each dr1 As DataRow In dtTrans.Rows
                    'SET CURRENT ROW
                    currRow += 1
                    'SET STEP KE = (CURRENT ROW/JML ROW) * 100
                    stepKe = Math.Round((currRow / dtTrans.Rows.Count) * 100, 2)

                    'SET NILAI VARIABEL
                    id = dr1("id") : sumber = dr1("sumber") : idutama = dr1("idutama") : iddetail = dr1("iddetail")
                    postingtgl = AsFormatTanggal(dr1("postingtgl"), formatTglWaktuDB) : jenismutasi = dr1("jenismutasi")
                    idbarang = dr1("idbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : satuanbarang = dr1("satuanbarang")
                    kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    saldojml = Double.Parse(dr1("saldojml")) : saldohpp = Double.Parse(dr1("saldojml")) : saldonilai = Double.Parse(dr1("saldonilai"))

                    'AMBIL SALDO AWAL
                    'QUERY
                    sql = "  SELECT IFNULL(saldojml, 0) as saldojml, IFNULL(saldohpp, 0) as saldohpp, IFNULL(saldonilai, 0) as saldonilai"
                    sql &= " FROM M1_Item_Transaction"
                    sql &= " JOIN M1_Item ON idbarang = bid AND bjenis <> 'J' AND bhpp = 'F'"
                    sql &= " JOIN M0_Nomor ON sumber = kodetabel AND transaksihpp = 1"
                    sql &= " WHERE postingtgl <= '" & FixQuotes(postingtgl) & "'"
                    sql &= " AND id < '" & FixDouble(id) & "'"
                    sql &= " AND idbarang = '" & FixDouble(idbarang) & "'"
                    'ORDER BY
                    'sql &= " ORDER BY postingtgl DESC, jenismutasi DESC, id DESC"
                    sql &= " ORDER BY id DESC"
                    'LIMIT
                    sql &= " LIMIT 1"

                    dtSA = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSA.Rows.Count > 0 Then
                        SAJml = Double.Parse(dtSA.Rows(0)("saldojml"))
                        SAHpp = Double.Parse(dtSA.Rows(0)("saldohpp"))
                        SANilai = Double.Parse(dtSA.Rows(0)("saldonilai"))
                    Else
                        SAJml = 0 : SAHpp = 0 : SANilai = 0
                    End If

                    'PROSES BERDASARKAN JENIS MUTASI
                    If jenismutasi = 1 Then
                        'JENISMUTASI = 1, BARANG MASUK
                        If sumber = "PD" Then

                            'JIKA SUMBER PD (PRODUKSI), HPP BARANG MASUK DIHITUNG DARI PROSENTASE TOTAL HPP BARANG KELUAR
                            'AMBIL PROSENTASE HPP DARI PD IN DAN AMBIL TOTAL HPP DARI PD OUT
                            sql = "  SELECT pdi.idpdin, pdi.hpppersen, IFNULL(SUM(pdo.jmlbarang * pdo.hpp),0) as totalhpp"
                            sql &= " FROM m6_pd_in pdi"
                            sql &= " JOIN m6_pd_out pdo ON pdi.idpd = pdo.idpd"
                            sql &= " WHERE pdi.idpd = '" & FixDouble(idutama) & "'"
                            sql &= " AND pdi.idpdin = '" & FixDouble(iddetail) & "'"
                            sql &= " GROUP BY pdi.idpdin"

                            dtPD = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtPD.Rows.Count > 0 Then
                                hppPersen = Double.Parse(dtPD.Rows(0)("hpppersen"))
                                totalHpp = Double.Parse(dtPD.Rows(0)("totalhpp"))
                            Else
                                hppPersen = 0 : totalHpp = 0
                            End If

                            'PERHITUNGAN HPP = ((HPPPERSEN / 100) * TOTAL HPP BARANG KELUAR) / JMLBARANG
                            hpp = ((hppPersen / 100) * totalHpp) / jmlbarang

                        Else

                            'SELAIN SUMBER PD (PRODUKSI), HPP BARANG MASUK DIHITUNG DARI TRANSAKSI
                            hpp = Double.Parse(dr1("hpp"))

                        End If

                        'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                        strFifo.Clear()
                        'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                   cfitipebarang,                      cfisatuan,                      cfijmlmasuk, cfijmlkeluar,              cfisisa,            cfiharga,  cfiisclose,               cfiinputtgl
                        strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", '" & FixQuotes(postingtgl) & "')")
                        sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                        'SALDONILAI = ((HPPLAMA * JMLLAMA)) + (HPPBARU * JMLBARANG))
                        If saldojml <> 0 Then
                            saldonilai = (SAHpp * SAJml) + (hpp * jmlbarang)
                            saldohpp = saldonilai / saldojml
                        Else
                            saldonilai = 0
                            saldohpp = 0
                        End If

                    Else
                        'JENISMUTASI <> 1, BARANG KELUAR

                        'RESET strIdHppFifo
                        strIdHppFifo.Clear()

                        'CEK JML HPP FIFO YANG TERSEDIA
                        dtCekFifo = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                        If dtCekFifo.Rows.Count > 0 Then
                            sisa = Double.Parse(dtCekFifo(0)("cfisisa"))
                            If jmlbarang > sisa Then
                                result(2) = "Row : " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa & " " & satuanbarang : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            result(2) = "Row :" & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                        End If

                        'AMBIL DATA HPP FIFO MASUK
                        'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                        'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                        dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                        If dtFifo.Rows.Count > 0 Then

                            'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                            hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

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
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            Next

                        Else
                            result(2) = "Row : " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                        End If

                        'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                        'SALDONILAI = ((HPPLAMA * JMLLAMA)) - (HPPBARU * JMLBARANG))
                        If saldojml <> 0 Then
                            saldonilai = (SAHpp * SAJml) - (hpp * jmlbarang)
                            saldohpp = saldonilai / saldojml
                        Else
                            saldonilai = 0
                            saldohpp = 0
                        End If

                    End If


                    'UPDATE KE TRANSAKSI MASING-MASING BERDASARKAN SUMBER
                    Select Case sumber.ToUpper
                        Case "SA" : sql = "UPDATE M3_Sa_Detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsadetail = '" & FixDouble(iddetail) & "'"
                        Case "IB" : sql = "UPDATE M3_Ib_Detail SET hpp = '" & FixDouble(hpp) & "' WHERE idibdetail = '" & FixDouble(iddetail) & "'"
                        Case "GRN" : sql = ""
                        Case "PRT" : sql = "UPDATE M4_prt_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idprtdetail = '" & FixDouble(iddetail) & "'"
                        Case "SI" : sql = "UPDATE M5_si_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "'"
                        Case "SR" : sql = "UPDATE M5_sr_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsrdetail = '" & FixDouble(iddetail) & "'"
                        Case "PD"
                            'JIKA PRODUKSI MAKA, BARANG MASUK UPDATE TABEL IN, BARANG KELUAR UPDATE TABEL OUT
                            If jenismutasi = 1 Then
                                sql = "UPDATE M6_pd_out SET hpp = '" & FixDouble(hpp) & "' WHERE idpdout = '" & FixDouble(iddetail) & "'"
                            Else
                                sql = "UPDATE M6_pd_in SET hpp = '" & FixDouble(hpp) & "' WHERE idpdin = '" & FixDouble(iddetail) & "'"
                            End If
                        Case Else : sql = ""
                    End Select
                    If Len(sql) > 0 Then
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    If hitungPerBarang Then
                        'TIDAK UPDATE HPPFIX
                        sql = "UPDATE M1_Item_Transaction SET idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1 WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    Else
                        'UPDATE HPPFIX
                        sql = "UPDATE M1_Item_Transaction SET idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = 1 WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    End If
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
            End If
            'END OF HITUNG ULANG TRANSAKSI BARANG ------------------------------


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

        'SET ERR STEPKE
        result(3) = stepKe

        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    '********************************************** M3 **********************************************

#Region "M3_Sa"
    '20150919
    Public Function M3_SaTanpaJurnal(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'SA MASUK :
        'PERSEDIAAN (D)
        '         AKUN LAWAN (K)
        'SA KELUAR :
        'AKUN LAWAN (D)
        '         PERSEDIAAN (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SA' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT sa.* FROM m3_sa sa WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "'", myConn)
            'END OF AMBIL DATA ---------------------------------------


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then

                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("said")
                sumber = drutama("sasumber")
                noTransaksi = drutama("sanotransaksi")
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
            If drutama("sacustomint1") = 1 Then
                'DETAIL
                'dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idbarang, i.bkode as kodebarang, sad.namabarang, sad.tipebarang, sad.satuan, sad.nilaisatuan, sad.satuanbarang, (CASE j.jenismutasi WHEN 1 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlkeluar, (CASE j.jenismutasi WHEN 1 THEN isw.stok ELSE 0 END) as jmlbarangmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok ELSE 0 END) as jmlbarangkeluar, sad.hpp, sad.idhppkhususmasuk, isw.kgudang as gudang, sad.urutan, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp, i.bjenis FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid JOIN m0_jenismutasi j JOIN m1_item_stock_warehouse isw ON sad.idbarang = isw.idbarang AND isw.stok <> 0 WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "' ORDER BY sad.urutan, j.jenismutasi, isw.kgudang, sad.idsadetail", myConn)
                dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idbarang, i.bkode as kodebarang, sad.namabarang, sad.tipebarang, sad.satuan, sad.nilaisatuan, sad.satuanbarang, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as jmlbarangmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as jmlbarangkeluar, sad.hpp, sad.idhppkhususmasuk, it.gudang, sad.urutan, i.bhpp, i.bjenis FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND it.sumber = 'SA' AND it.idutama = '" & idtransaksi & "' JOIN m3_sa_detail sad ON it.iddetail = sad.idsadetail ORDER BY sad.urutan, it.jenismutasi, it.gudang, sad.idsadetail", myConn)
            Else
                'DETAIL
                dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idbarang, i.bkode as kodebarang, sad.namabarang, sad.tipebarang, sad.satuan, sad.nilaisatuan, sad.satuanbarang, sad.jmlbarangmasuk, sad.jmlbarangkeluar, sad.hpp, sad.idhppkhususmasuk, sad.gudang, sad.urutan, i.bhpp, i.bjenis FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "' ORDER BY sad.urutan", myConn)
            End If

            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder


            'PROSES SET HPP ====================================================
            'JIKA TERDAPAT DATA MAKA SET HPP DAN BUAT JURNAL
            If dtDetail.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                For Each dr1 As DataRow In dtDetail.Rows
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsadetail")) : bjenis = dr1("bjenis")
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : gudang = dr1("gudang") : bhpp = dr1("bhpp")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan"))

                    'SET JENIS MUTASI DAN JMLBARANG
                    'BERDASARKAN TRANSAKSI MASUK ATAU KELUAR
                    If Double.Parse(dr1("jmlbarangmasuk")) <> 0 Then

                        'TRANSAKSI MASUK ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        jenismutasi = 1 : jmlbarang = Double.Parse(dr1("jmlbarangmasuk"))

                        'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                        sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                        dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtSaldo.Rows.Count > 0 Then
                            hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                            saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                        Else
                            hpplama = 0 : saldojml = 0
                        End If

                        'SET HPP (JIKA BARANG MASUK MAKA DIAMBILKAN DARI HPP TRANSAKSI YG DIINPUT)
                        hpp = Double.Parse(dr1("hpp"))

                        If bjenis <> "J" Then
                            'BARANG PERSEDIAAN ################################

                            'INSERT HPP MASUK (FIFO DAN KHUSUS)
                            If bhpp = "F" Then
                                'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                                'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                                strFifo.Clear()
                                'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                                strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                                sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            ElseIf bhpp = "I" Then
                                'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                                'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                                strKhusus.Clear()
                                'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            End If

                        End If


                        'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                        'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                        If saldojml <> 0 Then
                            saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                            saldohpp = saldonilai / saldojml
                        Else
                            saldonilai = 0
                            saldohpp = 0
                        End If

                        'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                        sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'  AND gudang = '" & gudang & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    Else


                        'TRANSAKSI KELUAR ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        jenismutasi = 0 : jmlbarang = Double.Parse(dr1("jmlbarangkeluar"))

                        'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                        sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                        dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtSaldo.Rows.Count > 0 Then
                            hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                            saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                        Else
                            hpplama = 0 : saldojml = 0
                        End If


                        If bjenis <> "J" Then
                            'BARANG PERSEDIAAN ################################

                            'AMBIL HPP SESUAI TIPE HPP
                            If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                                'AMBIL HPP DARI BARANG
                                sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                                If dtSaldo.Rows.Count > 0 Then
                                    hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                                Else
                                    hpp = 0
                                End If

                            ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                                'AMBIL HPP DARI HPP KHUSUS MASUK
                                sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                                dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                                If dtSaldo.Rows.Count > 0 Then
                                    hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                    'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                    strKhusus.Clear()
                                    'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                    strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                    sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                    sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                Else
                                    hpp = 0
                                End If


                            ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                                'RESET strIdHppFifo
                                strIdHppFifo.Clear()

                                'CEK JML HPP FIFO YANG TERSEDIA
                                dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                                If dt.Rows.Count > 0 Then
                                    sisa = Double.Parse(dt(0)(0))
                                    If jmlbarang > sisa Then
                                        rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                    End If
                                Else
                                    If jmlbarang > 0 Then
                                        rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                    End If
                                End If

                                'AMBIL DATA HPP FIFO MASUK
                                'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                                'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                                dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                                If dtFifo.Rows.Count > 0 Then

                                    'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                    hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                    'PERULANGAN DATA HPP FIFO
                                    For Each dr2 As DataRow In dtFifo.Rows
                                        ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                        ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                        'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                        'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                        'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                        strFifo.Clear()
                                        'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                        strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                        sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = myConn
                                            .Transaction = Trans
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd.ExecuteNonQuery()

                                        'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                        sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = myConn
                                            .Transaction = Trans
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd.ExecuteNonQuery()
                                    Next

                                Else
                                    If jmlbarang > 0 Then
                                        rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                    Else
                                        hpp = 0
                                    End If
                                End If

                            End If

                        Else

                            'BARANG JASA ######################################
                            'hitung hpp = hpp transaksi
                            hpp = Double.Parse(dr1("hpp"))

                        End If


                        'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                        'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                        If saldojml <> 0 Then
                            saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                            saldohpp = saldonilai / saldojml
                        Else
                            saldonilai = 0
                            saldohpp = 0
                        End If


                        'UPDATE HPP PADA TRANSAKSI
                        sql = "UPDATE M3_sa_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsadetail = '" & FixDouble(iddetail) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                        sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "' AND gudang = '" & gudang & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If

                Next
                'END OF PROSES SET HPP -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES SET HPP =============================================


            'BUAT JURNAL HPP PERSEDIAAN ========================================
            'AMBIL DATA DETAIL YANG BARU UNTUK UPDATE TRANSAKSI BARANG DAN JURNAL
            'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
            'If drutama("sacustomint1") = 1 Then
            '    dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idsa, sad.idbarang, sad.namabarang, sad.tipebarang, (CASE it.jenismutasi WHEN 1 THEN it.jml ELSE 0 END) as jmlmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jml ELSE 0 END) as jmlkeluar, sad.satuan, sad.nilaisatuan, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as jmlbarangmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as jmlbarangkeluar, sad.satuanbarang, it.idhppikm as idhppkhususmasuk, sad.hpplama, it.hpp, sad.rekpersediaan, sad.reklawan, sad.idspdetail, sad.cabang, sad.lokasi, sad.gudang, sad.costcenter, sad.divisi, sad.subdivisi, sad.proyek, sad.catatan, sad.urutan, sad.isclose, sad.customtext1, sad.customtext2, sad.customtext3, sad.customdbl1, sad.customdbl2, sad.customdbl3, sad.customdate1, sad.customdate2, sad.customdate3 FROM m1_item_transaction it JOIN m3_sa_detail sad ON it.iddetail = sad.idsadetail AND it.sumber = 'SA' AND it.idutama = '" & idtransaksi & "' ORDER BY sad.urutan, it.jenismutasi, it.gudang, sad.idsadetail", myConn)
            'Else
            '    dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.* FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "'", myConn)
            'End If


            'If dtDetail.Rows.Count > 0 Then

            '    'AKUN DEBIT & KREDIT ---------------------------------
            '    'DATA DIAMBILKAN DARI TRANSAKSI DETAIL
            '    For Each drdetail As DataRow In dtDetail.Rows

            '        'JIKA jmlmasuk > 0 MAKA JURNAL SA MASUK
            '        If Double.Parse(drdetail("jmlbarangmasuk")) > 0 Then
            '            'AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
            '            debitkredit = 0
            '            'GROUPING AKUN DEBIT (rekpersediaan)
            '            'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
            '            filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
            '            'NOMINAL = HPP * JML
            '            nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangmasuk"))
            '            If AsDataTableDCount(dtjurnal, filter) > 0 Then
            '                nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
            '                If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
            '                    rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            Else
            '                If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
            '                                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
            '                    rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            End If
            '            'END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~

            '            'AKUN KREDIT ~~~~~~~~~~~~~~~~~~~~~~~~
            '            debitkredit = 1
            '            'GROUPING AKUN KREDIT (reklawan)
            '            'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "'"
            '            filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "' AND costcenter = '' AND divisi = '' AND subdivisi = '' AND proyek = ''"
            '            'NOMINAL = HPP * JML
            '            nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangmasuk"))
            '            If AsDataTableDCount(dtjurnal, filter) > 0 Then
            '                nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
            '                If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
            '                    rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            Else
            '                'If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
            '                '                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
            '                '    rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                'End If
            '                If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
            '                                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, "", "", "", "", urutan)) = False Then
            '                    rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If

            '            End If
            '            'END OF AKUN KREDIT ~~~~~~~~~~~~~~~~~


            '            'JIKA jmlkeluar > 0 MAKA JURNAL SA KELUAR
            '        Else
            '            'AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
            '            debitkredit = 0
            '            'GROUPING AKUN DEBIT (reklawan)
            '            'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "'"
            '            filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "' AND costcenter = '' AND divisi = '' AND subdivisi = '' AND proyek = ''"
            '            'NOMINAL = HPP * JML
            '            nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangkeluar"))
            '            If AsDataTableDCount(dtjurnal, filter) > 0 Then
            '                nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
            '                If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
            '                    rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            Else
            '                'If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
            '                '                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
            '                '    rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                'End If
            '                If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
            '                                       String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, "", "", "", "", urutan)) = False Then
            '                    rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            End If
            '            'END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~

            '            'AKUN KREDIT ~~~~~~~~~~~~~~~~~~~~~~~~
            '            debitkredit = 1
            '            'GROUPING AKUN KREDIT (rekpersediaan)
            '            'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
            '            filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
            '            'NOMINAL = HPP * JML
            '            nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangkeluar"))
            '            If AsDataTableDCount(dtjurnal, filter) > 0 Then
            '                nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
            '                If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
            '                    rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            Else
            '                If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
            '                                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
            '                    rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
            '                End If
            '            End If
            '            'END OF AKUN KREDIT ~~~~~~~~~~~~~~~~~

            '        End If
            '    Next
            '    'END OF AKUN DEBIT & KREDIT --------------------------

            'Else
            '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
            '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            'End If
            'END OF BUAT JURNAL HPP PERSEDIAAN =================================


            'BUAT SQL ==========================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            'dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            ''DELETE JURNAL JIKA NOMINAL = 0
            'AsDataTableDeleteData(dtjurnal, "nominal = 0")

            'For Each drjurnal As DataRow In dtjurnal.Rows
            '    urutan = urutan + 1
            '    'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
            '    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
            '    'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
            '    If drjurnal("debitkredit") = 0 Then
            '        'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,             tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo,   ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
            '        strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(drutama("sasumber")) & "', " & 0 & ", " & drutama("said") & ", '" & FixQuotes(drutama("sanotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sakodepa") & ", " & drutama("sabagiansa") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(Double.Parse(kurs)) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', 0, '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("sastatus") & ", 1, NOW(), " & drutama("sajmlrevisi") & ", " & drutama("sacetakanke") & ", " & drutama("sainputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("sainputtgl"), formatTglWaktuDB)) & "', " & drutama("samodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("samodifikasitgl"), formatTglWaktuDB)) & "')")
            '    Else
            '        'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,              tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang, ttgljatuhtempo,  ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
            '        strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(drutama("sasumber")) & "', " & 0 & ", " & drutama("said") & ", '" & FixQuotes(drutama("sanotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sakodepa") & ", " & drutama("sabagiansa") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(Double.Parse(kurs)) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', 0, '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("sastatus") & ", 1, NOW(), " & drutama("sajmlrevisi") & ", " & drutama("sacetakanke") & ", " & drutama("sainputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("sainputtgl"), formatTglWaktuDB)) & "', " & drutama("samodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("samodifikasitgl"), formatTglWaktuDB)) & "')")
            '    End If
            'Next

            ''BUAT SQL JURNAL
            'If Len(strValue.ToString) > 0 Then
            '    sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myConn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sqlJurnal
            '    End With
            '    objCmd.ExecuteNonQuery()
            'End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SA
            sqlPosting = "UPDATE m3_sa SET saposting = 1, sapostingtgl = NOW() WHERE said = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ===================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        'END OF EKSEKUSI QUERY HPP ===========================================

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function

    Public Function M3_Sa(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'SA MASUK :
        'PERSEDIAAN (D)
        '         AKUN LAWAN (K)
        'SA KELUAR :
        'AKUN LAWAN (D)
        '         PERSEDIAAN (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SA' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT sa.* FROM m3_sa sa WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "'", myConn)
            'END OF AMBIL DATA ---------------------------------------


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then

                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("said")
                sumber = drutama("sasumber")
                noTransaksi = drutama("sanotransaksi")
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
            If drutama("sacustomint1") = 1 Then
                'DETAIL
                'dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idbarang, i.bkode as kodebarang, sad.namabarang, sad.tipebarang, sad.satuan, sad.nilaisatuan, sad.satuanbarang, (CASE j.jenismutasi WHEN 1 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlkeluar, (CASE j.jenismutasi WHEN 1 THEN isw.stok ELSE 0 END) as jmlbarangmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok ELSE 0 END) as jmlbarangkeluar, sad.hpp, sad.idhppkhususmasuk, isw.kgudang as gudang, sad.urutan, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp, i.bjenis FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid JOIN m0_jenismutasi j JOIN m1_item_stock_warehouse isw ON sad.idbarang = isw.idbarang AND isw.stok <> 0 WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "' ORDER BY sad.urutan, j.jenismutasi, isw.kgudang, sad.idsadetail", myConn)
                dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idbarang, i.bkode as kodebarang, sad.namabarang, sad.tipebarang, sad.satuan, sad.nilaisatuan, sad.satuanbarang, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as jmlbarangmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as jmlbarangkeluar, sad.hpp, sad.idhppkhususmasuk, it.gudang, sad.urutan, i.bhpp, i.bjenis FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND it.sumber = 'SA' AND it.idutama = '" & idtransaksi & "' JOIN m3_sa_detail sad ON it.iddetail = sad.idsadetail ORDER BY sad.urutan, it.jenismutasi, it.gudang, sad.idsadetail", myConn)
            Else
                'DETAIL
                dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idbarang, i.bkode as kodebarang, sad.namabarang, sad.tipebarang, sad.satuan, sad.nilaisatuan, sad.satuanbarang, sad.jmlbarangmasuk, sad.jmlbarangkeluar, sad.hpp, sad.idhppkhususmasuk, sad.gudang, sad.urutan, i.bhpp, i.bjenis FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "' ORDER BY sad.urutan", myConn)
            End If

            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder


            'PROSES SET HPP ====================================================
            'JIKA TERDAPAT DATA MAKA SET HPP DAN BUAT JURNAL
            If dtDetail.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                For Each dr1 As DataRow In dtDetail.Rows
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsadetail")) : bjenis = dr1("bjenis")
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : gudang = dr1("gudang") : bhpp = dr1("bhpp")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan"))

                    'SET JENIS MUTASI DAN JMLBARANG
                    'BERDASARKAN TRANSAKSI MASUK ATAU KELUAR
                    If Double.Parse(dr1("jmlbarangmasuk")) <> 0 Then

                        'TRANSAKSI MASUK ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        jenismutasi = 1 : jmlbarang = Double.Parse(dr1("jmlbarangmasuk"))

                        'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                        sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                        dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtSaldo.Rows.Count > 0 Then
                            hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                            saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                        Else
                            hpplama = 0 : saldojml = 0
                        End If

                        'SET HPP (JIKA BARANG MASUK MAKA DIAMBILKAN DARI HPP TRANSAKSI YG DIINPUT)
                        hpp = Double.Parse(dr1("hpp"))

                        If bjenis <> "J" Then
                            'BARANG PERSEDIAAN ################################

                            'INSERT HPP MASUK (FIFO DAN KHUSUS)
                            If bhpp = "F" Then
                                'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                                'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                                strFifo.Clear()
                                'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                                strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                                sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            ElseIf bhpp = "I" Then
                                'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                                'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                                strKhusus.Clear()
                                'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            End If

                        End If


                        'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                        'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                        If saldojml <> 0 Then
                            saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                            saldohpp = saldonilai / saldojml
                        Else
                            saldonilai = 0
                            saldohpp = 0
                        End If

                        'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                        sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'  AND gudang = '" & gudang & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                        sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    Else


                        'TRANSAKSI KELUAR ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        jenismutasi = 0 : jmlbarang = Double.Parse(dr1("jmlbarangkeluar"))

                        'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                        sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                        dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtSaldo.Rows.Count > 0 Then
                            hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                            saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                        Else
                            hpplama = 0 : saldojml = 0
                        End If


                        If bjenis <> "J" Then
                            'BARANG PERSEDIAAN ################################

                            'AMBIL HPP SESUAI TIPE HPP
                            If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                                'AMBIL HPP DARI BARANG
                                sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                                If dtSaldo.Rows.Count > 0 Then
                                    hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                                Else
                                    hpp = 0
                                End If

                            ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                                'AMBIL HPP DARI HPP KHUSUS MASUK
                                sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                                dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                                If dtSaldo.Rows.Count > 0 Then
                                    hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                    'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                    strKhusus.Clear()
                                    'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                    strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                    sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                    sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                Else
                                    hpp = 0
                                End If


                            ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                                'RESET strIdHppFifo
                                strIdHppFifo.Clear()

                                'CEK JML HPP FIFO YANG TERSEDIA
                                dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                                If dt.Rows.Count > 0 Then
                                    sisa = Double.Parse(dt(0)(0))
                                    If jmlbarang > sisa Then
                                        rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                    End If
                                Else
                                    If jmlbarang > 0 Then
                                        rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                    End If
                                End If

                                'AMBIL DATA HPP FIFO MASUK
                                'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                                'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                                dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                                If dtFifo.Rows.Count > 0 Then

                                    'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                    hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                    'PERULANGAN DATA HPP FIFO
                                    For Each dr2 As DataRow In dtFifo.Rows
                                        ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                        ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                        'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                        'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                        'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                        strFifo.Clear()
                                        'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                        strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                        sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = myConn
                                            .Transaction = Trans
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd.ExecuteNonQuery()

                                        'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                        sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = myConn
                                            .Transaction = Trans
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd.ExecuteNonQuery()
                                    Next

                                Else
                                    If jmlbarang > 0 Then
                                        rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                    Else
                                        hpp = 0
                                    End If
                                End If

                            End If

                        Else

                            'BARANG JASA ######################################
                            'hitung hpp = hpp transaksi
                            hpp = Double.Parse(dr1("hpp"))

                        End If


                        'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                        'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                        If saldojml <> 0 Then
                            saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                            saldohpp = saldonilai / saldojml
                        Else
                            saldonilai = 0
                            saldohpp = 0
                        End If


                        'UPDATE HPP PADA TRANSAKSI
                        sql = "UPDATE M3_sa_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsadetail = '" & FixDouble(iddetail) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                        'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                        sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "' AND gudang = '" & gudang & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If

                Next
                'END OF PROSES SET HPP -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES SET HPP =============================================


            'BUAT JURNAL HPP PERSEDIAAN ========================================
            'AMBIL DATA DETAIL YANG BARU UNTUK UPDATE TRANSAKSI BARANG DAN JURNAL
            'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
            If drutama("sacustomint1") = 1 Then
                dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.idsadetail, sad.idsa, sad.idbarang, sad.namabarang, sad.tipebarang, (CASE it.jenismutasi WHEN 1 THEN it.jml ELSE 0 END) as jmlmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jml ELSE 0 END) as jmlkeluar, sad.satuan, sad.nilaisatuan, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as jmlbarangmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as jmlbarangkeluar, sad.satuanbarang, it.idhppikm as idhppkhususmasuk, sad.hpplama, it.hpp, sad.rekpersediaan, sad.reklawan, sad.idspdetail, sad.cabang, sad.lokasi, sad.gudang, sad.costcenter, sad.divisi, sad.subdivisi, sad.proyek, sad.catatan, sad.urutan, sad.isclose, sad.customtext1, sad.customtext2, sad.customtext3, sad.customdbl1, sad.customdbl2, sad.customdbl3, sad.customdate1, sad.customdate2, sad.customdate3 FROM m1_item_transaction it JOIN m3_sa_detail sad ON it.iddetail = sad.idsadetail AND it.sumber = 'SA' AND it.idutama = '" & idtransaksi & "' ORDER BY sad.urutan, it.jenismutasi, it.gudang, sad.idsadetail", myConn)
            Else
                dtDetail = AsDataTableAmbilDariDBCon("SELECT sad.* FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said WHERE (sa.sastatus = 2 OR sa.sastatus = 3 OR sa.sastatus = 4 OR sa.sastatus = 7) AND sa.said = '" & idtransaksi & "'", myConn)
            End If


            If dtDetail.Rows.Count > 0 Then

                'AKUN DEBIT & KREDIT ---------------------------------
                'DATA DIAMBILKAN DARI TRANSAKSI DETAIL
                For Each drdetail As DataRow In dtDetail.Rows

                    'JIKA jmlmasuk > 0 MAKA JURNAL SA MASUK
                    If Double.Parse(drdetail("jmlbarangmasuk")) > 0 Then
                        'AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        'NOMINAL = HPP * JML
                        nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangmasuk"))
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
                                rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~

                        'AKUN KREDIT ~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'GROUPING AKUN KREDIT (reklawan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "' AND costcenter = '' AND divisi = '' AND subdivisi = '' AND proyek = ''"
                        'NOMINAL = HPP * JML
                        nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangmasuk"))
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
                                rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            'If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                            '                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                            '    rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            'End If
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, "", "", "", "", urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If

                        End If
                        'END OF AKUN KREDIT ~~~~~~~~~~~~~~~~~


                        'JIKA jmlkeluar > 0 MAKA JURNAL SA KELUAR
                    Else
                        'AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'GROUPING AKUN DEBIT (reklawan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("reklawan").ToString & "' AND costcenter = '' AND divisi = '' AND subdivisi = '' AND proyek = ''"
                        'NOMINAL = HPP * JML
                        nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangkeluar"))
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
                                rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            'If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                            '                         String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                            '    rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            'End If
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                   String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("reklawan").ToString, "AKUN LAWAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, "", "", "", "", urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~

                        'AKUN KREDIT ~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'GROUPING AKUN KREDIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        'NOMINAL = HPP * JML
                        nominal = Double.Parse(drdetail("hpp")) * Double.Parse(drdetail("jmlbarangkeluar"))
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal", nominal) = False Then
                                rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sacatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN KREDIT ~~~~~~~~~~~~~~~~~

                    End If
                Next
                'END OF AKUN DEBIT & KREDIT --------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN =================================


            'BUAT SQL ==========================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMINAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,             tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo,   ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(drutama("sasumber")) & "', " & 0 & ", " & drutama("said") & ", '" & FixQuotes(drutama("sanotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sakodepa") & ", " & drutama("sabagiansa") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(Double.Parse(kurs)) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', 0, '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("sastatus") & ", 1, NOW(), " & drutama("sajmlrevisi") & ", " & drutama("sacetakanke") & ", " & drutama("sainputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("sainputtgl"), formatTglWaktuDB)) & "', " & drutama("samodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("samodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,              tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang, ttgljatuhtempo,  ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(drutama("sasumber")) & "', " & 0 & ", " & drutama("said") & ", '" & FixQuotes(drutama("sanotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sakodepa") & ", " & drutama("sabagiansa") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(Double.Parse(kurs)) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', 0, '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("sastatus") & ", 1, NOW(), " & drutama("sajmlrevisi") & ", " & drutama("sacetakanke") & ", " & drutama("sainputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("sainputtgl"), formatTglWaktuDB)) & "', " & drutama("samodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("samodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next

            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SA
            sqlPosting = "UPDATE m3_sa SET saposting = 1, sapostingtgl = NOW() WHERE said = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ===================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        'END OF EKSEKUSI QUERY HPP ===========================================

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function

#End Region

#Region "M3_Ib"
    '20150919
    Public Function M3_Ib(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'PERSEDIAAN (D)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'IB' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT Ib.* FROM M3_Ib Ib WHERE (Ib.Ibstatus = 2 OR Ib.Ibstatus = 3 OR Ib.Ibstatus = 4 OR Ib.Ibstatus = 7) AND Ib.Ibid = '" & idtransaksi & "'", myConn)
            'DETAIL
            dtDetail = AsDataTableAmbilDariDBCon("SELECT Ibd.*, i.bhpp FROM M3_Ib_detail Ibd JOIN M3_Ib Ib ON Ibd.idIb = Ib.Ibid JOIN m1_item i ON Ibd.idbarang = i.bid WHERE (Ib.Ibstatus = 2 OR Ib.Ibstatus = 3 OR Ib.Ibstatus = 4 OR Ib.Ibstatus = 7) AND Ib.Ibid = '" & idtransaksi & "' ORDER BY ibd.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("Ibid")
                sumber = drutama("Ibsumber")
                noTransaksi = drutama("Ibnotransaksi")
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder


            'PROSES HPP BARANG MASUK ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetail.Rows.Count > 0 Then

                For Each dr1 As DataRow In dtDetail.Rows

                    'PROSES SET HPP --------------------------------------
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idIbdetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    satuanbarang = dr1("satuanbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : gudang = dr1("gudang")
                    bhpp = dr1("bhpp")
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'hitung hpp = hpp
                    hpp = Double.Parse(dr1("hpp"))

                    'INSERT HPP MASUK (FIFO DAN KHUSUS)
                    If bhpp = "F" Then
                        'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                        'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                        strFifo.Clear()
                        'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                        strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                        sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    ElseIf bhpp = "I" Then
                        'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                        'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                        strKhusus.Clear()
                        'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                        strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                        sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If

                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If

                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------


                    'PROSES JURNAL ---------------------------------------
                    'AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
                    debitkredit = 0

                    'NOMINAL = jmlbarang * hpp * kurs
                    nominal = Double.Parse(dr1("jmlbarang")) * Double.Parse(dr1("hpp")) * Double.Parse(dr1("kurs"))
                    'NOMINAL VALAS JIKA MENGGUNAKAN MATAUANG VALAS
                    If dr1("matauang").ToString <> matauang Then
                        'NOMINAL VALAS = jmlbarang * hpp
                        nominalvalas = Double.Parse(dr1("jmlbarang")) * Double.Parse(dr1("hpp"))
                    Else
                        nominalvalas = 0
                    End If

                    'GROUPING AKUN DEBIT (rekpersediaan)
                    'filter = "debitkredit = " & debitkredit & " AND norek = '" & dr1("rekpersediaan").ToString & "'"
                    filter = "debitkredit = " & debitkredit & " AND norek = '" & dr1("rekpersediaan").ToString & "' AND costcenter = '" & dr1("costcenter").ToString & "' AND divisi = '" & dr1("divisi").ToString & "' AND subdivisi = '" & dr1("subdivisi").ToString & "' AND proyek = '" & dr1("proyek").ToString & "'"
                    If AsDataTableDCount(dtjurnal, filter) > 0 Then
                        nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                        nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                            rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    Else
                        If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                 String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", dr1("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("Ibcatatan").ToString, dr1("costcenter").ToString, dr1("divisi").ToString, dr1("subdivisi").ToString, dr1("proyek").ToString, urutan)) = False Then
                            rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                    'END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~
                    'END OF PROSES JURNAL --------------------------------

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK ====================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("Ibcabang")) & "', '" & FixQuotes(drutama("Iblokasi")) & "', '" & FixQuotes(drutama("Ibsumber")) & "', " & 0 & ", " & drutama("Ibid") & ", '" & FixQuotes(drutama("Ibnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("Ibtgl"))) & "', " & drutama("Ibkodepa") & ", " & drutama("Ibbagianib") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("Iburaian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("Ibmatauang")) & "', '" & FixDouble(drutama("Ibkurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("Ibstatus") & ", 1, NOW(), " & drutama("Ibjmlrevisi") & ", " & drutama("Ibcetakanke") & ", " & drutama("Ibinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("Ibinputtgl"), formatTglWaktuDB)) & "', " & drutama("Ibmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("Ibmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("Ibcabang")) & "', '" & FixQuotes(drutama("Iblokasi")) & "', '" & FixQuotes(drutama("Ibsumber")) & "', " & 0 & ", " & drutama("Ibid") & ", '" & FixQuotes(drutama("Ibnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("Ibtgl"))) & "', " & drutama("Ibkodepa") & ", " & drutama("Ibbagianib") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("Iburaian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("Ibmatauang")) & "', '" & FixDouble(drutama("Ibkurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("Ibstatus") & ", 1, NOW(), " & drutama("Ibjmlrevisi") & ", " & drutama("Ibcetakanke") & ", " & drutama("Ibinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("Ibinputtgl"), formatTglWaktuDB)) & "', " & drutama("Ibmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("Ibmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING
            sqlPosting = "UPDATE M3_Ib SET Ibposting = 1, Ibpostingtgl = NOW() WHERE Ibid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

    '********************************************** M4 **********************************************

#Region "M4_Grn"
    '20150919
    Public Function M4_Grn(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'PERSEDIAAN (D)
        '         HUTANG SEMENTARA (K)
        '         AKUN KREDIT BIAYA (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable, dtCost As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim totalTransaksiFungsional As Double = 0, totalBiayaFungsional As Double = 0, debitkreditgroup As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtjurnal, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "kurs", AsEnumTypeData.AsDouble)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'GRN' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT grn.* FROM m4_grn grn WHERE (grn.grnstatus = 2 OR grn.grnstatus = 3 OR grn.grnstatus = 4 OR grn.grnstatus = 7) AND grn.grnid = '" & idtransaksi & "'", myConn)
            'DETAIL
            dtDetail = AsDataTableAmbilDariDBCon("SELECT grnd.*, i.bhpp FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid JOIN m1_item i ON grnd.idbarang = i.bid WHERE (grn.grnstatus = 2 OR grn.grnstatus = 3 OR grn.grnstatus = 4 OR grn.grnstatus = 7) AND grn.grnid = '" & idtransaksi & "' ORDER BY grnd.urutan", myConn)
            'COST
            dtCost = AsDataTableAmbilDariDBCon("SELECT grnc.* FROM m4_grn_cost grnc JOIN m4_grn grn ON grnc.idgrn = grn.grnid WHERE (grn.grnstatus = 2 OR grn.grnstatus = 3 OR grn.grnstatus = 4 OR grn.grnstatus = 7) AND grn.grnid = '" & idtransaksi & "' ORDER BY grnc.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("grnid")
                sumber = drutama("grnsumber")
                noTransaksi = drutama("grnnotransaksi")
                termasukPajak = Integer.Parse(drutama("grnhargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'TAMBAHKAN FIELD TOTALFUNGSIONAL PADA DT DETAIL  
            AsDataTableTambahField(dtDetail, "totalfungsional", AsEnumTypeData.AsDouble)

            'PERHITUNGAN TOTALFUNGSIONAL BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
            If termasukPajak Then
                ''TOTALFUNGSIONAL = (((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs)
                'dtDetail.Columns("totalfungsional").Expression = "(((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs)"

                'TOTALFUNGSIONAL = (((jml * harga) - jmldiskon - jmlpajak1) * kurs)
                dtDetail.Columns("totalfungsional").Expression = "(((jml * harga) - jmldiskon - jmlpajak1) * kurs)"

            Else
                'TOTALFUNGSIONAL = (((jml * harga) - jmldiskon) * kurs)
                dtDetail.Columns("totalfungsional").Expression = "(((jml * harga) - jmldiskon) * kurs)"

            End If

            'AMBIL TOTALTRANSAKSI, SUM TOTALFUNGSIONAL DT DETAIL
            totalTransaksiFungsional = AsDataTableDSum(dtDetail, "totalfungsional")


            'AMBIL BIAYA =======================================================
            'JIKA TRANSAKSI MEMILIKI BIAYA YANG TERMASUK HPP MAKA HITUNG HPP DENGAN PENAMBAHAN BIAYA TERSEBUT
            If dtCost.Rows.Count > 0 Then
                'TAMBAHKAN FIELD JUMLAHFUNGSIONAL PADA DT COST
                'JUMLAHFUNGSIONAL = (jumlah * kurs)
                AsDataTableTambahField(dtCost, "jumlahfungsional", AsEnumTypeData.AsDouble)
                dtCost.Columns("jumlahfungsional").Expression = "(jumlah * kurs)"

                'AMBIL TOTAL BIAYA (FUNGSIONAL) TERMASUK HPP
                totalBiayaFungsional = AsDataTableDSum(dtCost, "jumlahfungsional", "termasukhpp = 1")
            End If
            'END OF AMBIL BIAYA ================================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0, prosentaseHpp As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder


            'PROSES HPP BARANG MASUK ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetail.Rows.Count > 0 Then

                For Each dr1 As DataRow In dtDetail.Rows

                    'PROSES SET HPP --------------------------------------
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idgrndetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    satuanbarang = dr1("satuanbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : gudang = dr1("gudang")
                    bhpp = dr1("bhpp")
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'PERHITUNGAN PROSENTASE BIAYA YANG MASUK HPP
                    'PROSENTASE BIAYA = (TOTAL PERBARANG FUNGSIONAL / TOTAL TRANSAKSI FUNGSIONAL) 
                    'BIAYA MASUK HPP = PROSENTASE BIAYA * TOTAL BIAYA FUNGSIONAL
                    If totalTransaksiFungsional <> 0 Then
                        prosentaseHpp = (Double.Parse(dr1("totalfungsional")) / totalTransaksiFungsional)
                    Else
                        prosentaseHpp = 0
                    End If


                    'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                    If termasukPajak Then
                        ''hitung hpp = (((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs) / jmlbarang
                        'hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                        ''hitung hpp = ((((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs) / jmlbarang) + ((prosentaseHpp * totalBiayaFungsional) / jmlbarang)
                        'hpp = ((((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))) + ((prosentaseHpp * totalBiayaFungsional) / Double.Parse(dr1("jmlbarang")))

                        'hitung hpp = ((((jml * harga) - jmldiskon - jmlpajak1) * kurs) / jmlbarang) + ((prosentaseHpp * totalBiayaFungsional) / jmlbarang)
                        hpp = ((((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))) + ((prosentaseHpp * totalBiayaFungsional) / Double.Parse(dr1("jmlbarang")))

                    Else
                        ''hitung hpp = (((jml * harga) - jmldiskon) * kurs) / jmlbarang
                        'hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                        'hitung hpp = ((((jml * harga) - jmldiskon) * kurs) / jmlbarang) + ((prosentaseHpp * totalBiayaFungsional) / jmlbarang)
                        hpp = ((((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))) + ((prosentaseHpp * totalBiayaFungsional) / Double.Parse(dr1("jmlbarang")))
                    End If

                    'INSERT HPP MASUK (FIFO DAN KHUSUS)
                    If bhpp = "F" Then
                        'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                        'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                        strFifo.Clear()
                        'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                        strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                        sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    ElseIf bhpp = "I" Then
                        'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                        'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                        strKhusus.Clear()
                        'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                        strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                        sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If

                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If

                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    'sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bhargabeli = '" & FixDouble(hpp) & "' WHERE bid = '" & idbarang & "'"
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------


                    'UPDATE HARGAJUAL ==============================================
                    'JIKA TERDAPAT SETTING OTOMATIS UPDATE HARGA JUAL MAKA UPDATE HARGA JUAL BARANG SESUAI RUMUS
                    'RUMUS HARGA JUAL TERDAPAT DI SETTING JUGA
                    'TINGKAT HARGA JUAL YANG YANG DIUPDATE SESUAI DENGAN SETTING (1 - 5)
                    Dim settingHargaJual As String = "", rumusHargaJual As String = ""

                    'HARGAJUAL 1
                    '0  options PembelianUpdateHargaJual1
                    '0  options PembelianUpdateHargaJual1Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 1
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual1")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 1
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual1Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual1 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 2
                    '0  options PembelianUpdateHargaJual2
                    '0  options PembelianUpdateHargaJual2Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 2
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual2")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 2
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual2Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual2 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 3
                    '0  options PembelianUpdateHargaJual3
                    '0  options PembelianUpdateHargaJual3Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 3
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual3")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 3
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual3Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual3 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 4
                    '0  options PembelianUpdateHargaJual4
                    '0  options PembelianUpdateHargaJual4Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 4
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual4")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 4
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual4Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual4 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 5
                    '0  options PembelianUpdateHargaJual5
                    '0  options PembelianUpdateHargaJual5Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 5
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual5")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 5
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual5Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual5 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If
                    'END OF UPDATE HARGAJUAL =======================================


                    'PROSES JURNAL ---------------------------------------
                    'AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
                    'AKUN DEBIT DITAMBAHKAN DENGAN BIAYA
                    debitkredit = 0

                    'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                    If termasukPajak Then
                        ''NOMINAL = ((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs
                        'nominal = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))

                        ''NOMINAL = (((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs) + (prosentaseHpp * totalBiayaFungsional)
                        'nominal = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))) + (prosentaseHpp * totalBiayaFungsional)

                        'NOMINAL = (((jml * harga) - jmldiskon - jmlpajak1) * kurs) + (prosentaseHpp * totalBiayaFungsional)
                        nominal = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1"))) * Double.Parse(dr1("kurs"))) + (prosentaseHpp * totalBiayaFungsional)
                    Else
                        ''NOMINAL = ((jml * harga) - jmldiskon) * kurs
                        'nominal = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))

                        'NOMINAL = (((jml * harga) - jmldiskon) * kurs) + (prosentaseHpp * totalBiayaFungsional)
                        nominal = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))) + (prosentaseHpp * totalBiayaFungsional)
                    End If

                    'NOMINAL VALAS JIKA MENGGUNAKAN MATAUANG VALAS
                    If dr1("matauang").ToString <> matauang Then
                        'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                        If termasukPajak Then
                            'NOMINAL VALAS = ((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2)
                            'nominalvalas = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2")))
                            nominalvalas = nominal / Double.Parse(drutama("grnkurs"))

                        Else
                            'NOMINAL VALAS = ((jml * harga) - jmldiskon)
                            'nominalvalas = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")))
                            nominalvalas = nominal / Double.Parse(drutama("grnkurs"))

                        End If

                    Else
                        nominalvalas = 0

                    End If

                    'GROUPING AKUN DEBIT (rekpersediaan)
                    'filter = "debitkredit = " & debitkredit & " AND norek = '" & dr1("rekpersediaan").ToString & "'"
                    filter = "debitkredit = " & debitkredit & " AND norek = '" & dr1("rekpersediaan").ToString & "' AND costcenter = '" & dr1("costcenter").ToString & "' AND divisi = '" & dr1("divisi").ToString & "' AND subdivisi = '" & dr1("subdivisi").ToString & "' AND proyek = '" & dr1("proyek").ToString & "'"
                    If AsDataTableDCount(dtjurnal, filter) > 0 Then
                        nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                        nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                            rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    Else
                        If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan~matauang~kurs", _
                                                 String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}", dr1("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("grncatatan").ToString, dr1("costcenter").ToString, dr1("divisi").ToString, dr1("subdivisi").ToString, dr1("proyek").ToString, urutan, drutama("grnmatauang").ToString, drutama("grnkurs").ToString)) = False Then
                            rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                    'END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~


                    'AKUN KREDIT ~~~~~~~~~~~~~~~~~~~~~~~~
                    debitkredit = 1

                    'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                    If termasukPajak Then
                        ''NOMINAL = ((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs
                        'nominal = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))

                        'NOMINAL = ((jml * harga) - jmldiskon - jmlpajak1) * kurs
                        nominal = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1"))) * Double.Parse(dr1("kurs"))

                    Else
                        'NOMINAL = ((jml * harga) - jmldiskon) * kurs
                        nominal = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))

                    End If

                    'NOMINAL VALAS JIKA MENGGUNAKAN MATAUANG VALAS
                    If dr1("matauang").ToString <> matauang Then
                        'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                        If termasukPajak Then
                            'NOMINAL VALAS = ((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2)
                            'nominalvalas = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2")))
                            nominalvalas = nominal / Double.Parse(drutama("grnkurs"))

                        Else
                            'NOMINAL VALAS = ((jml * harga) - jmldiskon)
                            'nominalvalas = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")))
                            nominalvalas = nominal / Double.Parse(drutama("grnkurs"))

                        End If

                    Else
                        nominalvalas = 0

                    End If

                    'GROUPING AKUN KREDIT (rekhutangsementara)
                    'filter = "debitkredit = " & debitkredit & " AND norek = '" & dr1("rekhutangsementara").ToString & "'"
                    filter = "debitkredit = " & debitkredit & " AND norek = '" & dr1("rekhutangsementara").ToString & "' AND costcenter = '' AND divisi = '' AND subdivisi = '' AND proyek = ''"
                    If AsDataTableDCount(dtjurnal, filter) > 0 Then
                        nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                        nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                            rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    Else
                        'If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan~matauang~kurs", _
                        '                        String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}", dr1("rekhutangsementara").ToString, "HUTANG SEMENTARA", nominal, nominalvalas, debitkredit, drutama("grncatatan").ToString, dr1("costcenter").ToString, dr1("divisi").ToString, dr1("subdivisi").ToString, dr1("proyek").ToString, urutan, drutama("grnmatauang").ToString, drutama("grnkurs").ToString)) = False Then
                        '    rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        'End If
                        If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan~matauang~kurs", _
                                                 String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}", dr1("rekhutangsementara").ToString, "HUTANG SEMENTARA", nominal, nominalvalas, debitkredit, drutama("grncatatan").ToString, "", "", "", "", urutan, drutama("grnmatauang").ToString, drutama("grnkurs").ToString)) = False Then
                            rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                    'END OF AKUN KREDIT ~~~~~~~~~~~~~~~~~
                    'END OF PROSES JURNAL --------------------------------

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK ====================================


            'PROSES BIAYA ========================================
            If dtCost.Rows.Count > 0 Then
                For Each drcost As DataRow In dtCost.Rows
                    If drcost("matauang").ToString <> matauang Then
                        'JIKA MATA UANG ASING
                        'NOMINAL = jumlah * kurs
                        nominal = Double.Parse(drcost("jumlah")) * Double.Parse(drcost("kurs"))
                        'NOMINALVALAS = jumlah
                        nominalvalas = Double.Parse(drcost("jumlah"))

                    Else
                        'JIKA MATA UANG FUNGSIONAL
                        'NOMINAL = jumlah
                        nominal = Double.Parse(drcost("jumlah"))
                        'NOMINALVALAS = 0
                        nominalvalas = 0
                    End If


                    'JURNAL SISI DEBIT
                    If Not drcost("termasukhpp").ToString.Equals("1") Then
                        'JIKA TIDAK TERMASUK HPP MAKA TAMBAHKAN JURNAL BIAYA PADA SISI DEBIT
                        'JIKA TERMASUK HPP TIDAK MENJURNAL PADA SISI DEBIT KARENA NOMINAL DEBIT SUDAH MASUK KE PERSEDIAAN BARANG
                        debitkredit = 0

                        'GROUPING AKUN DEBIT BIAYA
                        'filter = "norek = '" & drcost("rekdebit").ToString & "' AND matauang='" & drcost("matauang").ToString & "' AND kurs='" & FixDouble(drcost("kurs")) & "'"
                        filter = "norek = '" & drcost("rekdebit").ToString & "' AND matauang='" & drcost("matauang").ToString & "' AND kurs='" & FixDouble(drcost("kurs")) & "' AND costcenter = '" & drcost("costcenter").ToString & "' AND divisi = '" & drcost("divisi").ToString & "' AND subdivisi = '" & drcost("subdivisi").ToString & "' AND proyek = '" & drcost("proyek").ToString & "'"
                        'CEK AKUN SUDAH ADA ATAU BELUM PADA DT JURNAL
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            'JIKA ADA, CEK AKUN TERSEBUT DEBIT / KREDIT
                            debitkreditgroup = Double.Parse(AsDataTableDLookup(dtjurnal, "debitkredit", filter))
                            If debitkreditgroup = debitkredit Then
                                'JIKA AKUN SAMA-SAMA DEBIT/SAMA-SAMA KREDIT MAKA TAMBAHKAN NOMINALNYA SAJA
                                nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                                nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                                'UPDATE NOMINAL AKUN PADA DT JURNAL
                                If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                    rsErrMessage = "Failed update grouping datatable journal transaction #1 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                End If

                            Else
                                'JIKA AKUN BERBEDA DEBIT KREDITNYA MAKA NOMINAL BARU = NOMINAL AKUN PADA DT JURNAL - NOMINAL AKUN
                                nominal = Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter)) - nominal
                                nominalvalas = Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter)) - nominalvalas
                                'CEK NOMINAL BARU, JIKA NOMINAL BARU < 1 MAKA PINDAH SISI DEBIT DAN KREDITNYA
                                If nominal < 0 Then
                                    'ABSOLUTKAN NILAI NOMINAL
                                    nominal = Math.Abs(nominal)
                                    nominalvalas = Math.Abs(nominalvalas)
                                    'UPDATE NOMINAL DAN DEBITKREDIT DT JURNAL
                                    If debitkreditgroup = 0 Then
                                        'JIKA AKUN PADA DT JURNAL DEBIT MAKA DIPINDAH KE KREDIT
                                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas~debitkredit", nominal & "~" & nominalvalas & "~" & 1) = False Then
                                            rsErrMessage = "Failed update grouping datatable journal transaction #2 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                        End If
                                    Else
                                        'JIKA AKUN PADA DT JURNAL KREDIT MAKA DIPINDAH KE DEBIT
                                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas~debitkredit", nominal & "~" & nominalvalas & "~" & 0) = False Then
                                            rsErrMessage = "Failed update grouping datatable journal transaction #3 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                        End If
                                    End If

                                    'JIKA NOMINAL BARU >= 0 MAKA UPDATE NOMINALNYA SAJA
                                Else
                                    'UPDATE NOMINAL AKUN PADA DT JURNAL
                                    If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                        rsErrMessage = "Failed update grouping datatable journal transaction #4 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                    End If
                                End If
                            End If

                            'JIKA TIDAK ADA, TAMBAHKAN AKUN PADA DT JURNAL
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan~matauang~kurs", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}", drcost("rekdebit").ToString, "BIAYA", nominal, nominalvalas, debitkredit, drutama("grncatatan").ToString, "", "", "", "", urutan, drcost("matauang").ToString, FixDouble(drcost("kurs")))) = False Then
                                rsErrMessage = "Failed insert datatable journal transaction " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                            End If
                        End If

                    End If

                    'JURNAL SISI KREDIT
                    debitkredit = 1

                    'GROUPING AKUN KREDIT BIAYA
                    'filter = "norek = '" & drcost("rekkredit").ToString & "' AND matauang='" & drcost("matauang").ToString & "' AND kurs='" & FixDouble(drcost("kurs")) & "'"
                    filter = "norek = '" & drcost("rekkredit").ToString & "' AND matauang='" & drcost("matauang").ToString & "' AND kurs='" & FixDouble(drcost("kurs")) & "' AND costcenter = '" & drcost("costcenter").ToString & "' AND divisi = '" & drcost("divisi").ToString & "' AND subdivisi = '" & drcost("subdivisi").ToString & "' AND proyek = '" & drcost("proyek").ToString & "'"
                    'CEK AKUN SUDAH ADA ATAU BELUM PADA DT JURNAL
                    If AsDataTableDCount(dtjurnal, filter) > 0 Then
                        'JIKA ADA, CEK AKUN TERSEBUT DEBIT / KREDIT
                        debitkreditgroup = Double.Parse(AsDataTableDLookup(dtjurnal, "debitkredit", filter))
                        If debitkreditgroup = debitkredit Then
                            'JIKA AKUN SAMA-SAMA DEBIT/SAMA-SAMA KREDIT MAKA TAMBAHKAN NOMINALNYA SAJA
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            'UPDATE NOMINAL AKUN PADA DT JURNAL
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal transaction #1 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                            End If

                        Else
                            'JIKA AKUN BERBEDA DEBIT KREDITNYA MAKA NOMINAL BARU = NOMINAL AKUN PADA DT JURNAL - NOMINAL AKUN
                            nominal = Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter)) - nominal
                            nominalvalas = Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter)) - nominalvalas
                            'CEK NOMINAL BARU, JIKA NOMINAL BARU < 1 MAKA PINDAH SISI DEBIT DAN KREDITNYA
                            If nominal < 0 Then
                                'ABSOLUTKAN NILAI NOMINAL
                                nominal = Math.Abs(nominal)
                                nominalvalas = Math.Abs(nominalvalas)
                                'UPDATE NOMINAL DAN DEBITKREDIT DT JURNAL
                                If debitkreditgroup = 0 Then
                                    'JIKA AKUN PADA DT JURNAL DEBIT MAKA DIPINDAH KE KREDIT
                                    If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas~debitkredit", nominal & "~" & nominalvalas & "~" & 1) = False Then
                                        rsErrMessage = "Failed update grouping datatable journal transaction #2 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                    End If
                                Else
                                    'JIKA AKUN PADA DT JURNAL KREDIT MAKA DIPINDAH KE DEBIT
                                    If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas~debitkredit", nominal & "~" & nominalvalas & "~" & 0) = False Then
                                        rsErrMessage = "Failed update grouping datatable journal transaction #3 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                    End If
                                End If

                                'JIKA NOMINAL BARU >= 0 MAKA UPDATE NOMINALNYA SAJA
                            Else
                                'UPDATE NOMINAL AKUN PADA DT JURNAL
                                If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                    rsErrMessage = "Failed update grouping datatable journal transaction #4 " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                                End If
                            End If
                        End If

                        'JIKA TIDAK ADA, TAMBAHKAN AKUN PADA DT JURNAL
                    Else
                        If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan~matauang~kurs", _
                                                 String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}", drcost("rekkredit").ToString, "BIAYA", nominal, nominalvalas, debitkredit, drutama("grncatatan").ToString, "", "", "", "", urutan, drcost("matauang").ToString, FixDouble(drcost("kurs")))) = False Then
                            rsErrMessage = "Failed insert datatable journal transaction " & sumber & " (" & noTransaksi & ") : " & urutan & "." : GoTo selesai
                        End If
                    End If

                Next
            End If
            'END OF PROSES BIAYA =================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                         tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(drutama("grnsumber")) & "', " & 0 & ", " & drutama("grnid") & ", '" & FixQuotes(drutama("grnnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnkodepa") & ", " & drutama("grnsupplier") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drjurnal("matauang")) & "', '" & FixDouble(drjurnal("kurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("grnstatus") & ", 1, NOW(), " & drutama("grnjmlrevisi") & ", " & drutama("grncetakanke") & ", " & drutama("grninputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("grninputtgl"), formatTglWaktuDB)) & "', " & drutama("grnmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("grnmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                         tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(drutama("grnsumber")) & "', " & 0 & ", " & drutama("grnid") & ", '" & FixQuotes(drutama("grnnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnkodepa") & ", " & drutama("grnsupplier") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drjurnal("matauang")) & "', '" & FixDouble(drjurnal("kurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("grnstatus") & ", 1, NOW(), " & drutama("grnjmlrevisi") & ", " & drutama("grncetakanke") & ", " & drutama("grninputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("grninputtgl"), formatTglWaktuDB)) & "', " & drutama("grnmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("grnmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING
            sqlPosting = "UPDATE M4_Grn SET grnposting = 1, grnpostingtgl = NOW() WHERE grnid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

#Region "M4_Ri"
    Public Function M4_Ri(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'PERSEDIAAN (D)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable, dtCost As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim totalTransaksiFungsional As Double = 0, totalBiayaFungsional As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            ''HAPUS JURNAL LAMA
            'Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RI' AND tidtransaksi = '" & FixDouble(idtransaksi) & "' AND tgrup = 0"
            'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            'With objCmd
            '    .Connection = myConn
            '    .Transaction = Trans
            '    .CommandType = CommandType.Text
            '    .CommandText = sqlHapus
            'End With
            'objCmd.ExecuteNonQuery()


            ''AMBIL DATA DARI SETTING ---------------------------------
            'Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            ''MATAUANG
            'Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            'If matauang = "Not found" Then
            '    rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            'End If
            ''KURS
            'Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            'If kurs = "Not found" Then
            '    rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            'End If
            ''AKUN HUTANG USAHA
            'Dim rekHutangUsaha As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'akun' AND skode = 'HutangUsaha'", "Not found")
            'If rekHutangUsaha = "Not found" Then
            '    rsErrMessage = "Setting Account Payable CoA not found." : GoTo selesai
            'End If
            ''END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT ri.* FROM m4_ri ri WHERE (ri.ristatus = 2 OR ri.ristatus = 3 OR ri.ristatus = 4 OR ri.ristatus = 7) AND ri.riid = '" & idtransaksi & "'", myConn)
            'DETAIL
            'dtDetail = AsDataTableAmbilDariDBCon("SELECT rid.*, i.bhpp FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE (ri.ristatus = 2 OR ri.ristatus = 3 OR ri.ristatus = 4 OR ri.ristatus = 7) AND ri.riid = '" & idtransaksi & "' ORDER BY rid.urutan", myConn)
            dtDetail = AsDataTableAmbilDariDBCon("SELECT rid.*, i.bhpp FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE (ri.ristatus = 2 OR ri.ristatus = 3 OR ri.ristatus = 4 OR ri.ristatus = 7) AND ri.riid = '" & idtransaksi & "' AND i.bjenis = 'P' ORDER BY rid.urutan", myConn)
            'COST
            dtCost = AsDataTableAmbilDariDBCon("SELECT ric.* FROM m4_ri_cost ric JOIN m4_ri ri ON ric.idri = ri.riid WHERE (ri.ristatus = 2 OR ri.ristatus = 3 OR ri.ristatus = 4 OR ri.ristatus = 7) AND ri.riid = '" & idtransaksi & "' AND ric.termasukhpp = 1 ORDER BY ric.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("riid")
                sumber = drutama("risumber")
                noTransaksi = drutama("rinotransaksi")
                termasukPajak = Integer.Parse(drutama("rihargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'TAMBAHKAN FIELD TOTALFUNGSIONAL PADA DT DETAIL  
            AsDataTableTambahField(dtDetail, "totalfungsional", AsEnumTypeData.AsDouble)

            'PERHITUNGAN TOTALFUNGSIONAL BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
            If termasukPajak Then
                ''TOTALFUNGSIONAL = (((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs)
                'dtDetail.Columns("totalfungsional").Expression = "(((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs)"

                'TOTALFUNGSIONAL = (((jml * harga) - jmldiskon - jmlpajak1) * kurs)
                dtDetail.Columns("totalfungsional").Expression = "(((jml * harga) - jmldiskon - jmlpajak1) * kurs)"

            Else
                'TOTALFUNGSIONAL = (((jml * harga) - jmldiskon) * kurs)
                dtDetail.Columns("totalfungsional").Expression = "(((jml * harga) - jmldiskon) * kurs)"

            End If

            'AMBIL TOTALTRANSAKSI, SUM TOTALFUNGSIONAL DT DETAIL
            totalTransaksiFungsional = AsDataTableDSum(dtDetail, "totalfungsional")


            'AMBIL BIAYA =======================================================
            'JIKA TRANSAKSI MEMILIKI BIAYA YANG TERMASUK HPP MAKA HITUNG HPP DENGAN PENAMBAHAN BIAYA TERSEBUT
            If dtCost.Rows.Count > 0 Then
                'TAMBAHKAN FIELD JUMLAHFUNGSIONAL PADA DT COST
                'JUMLAHFUNGSIONAL = (jumlah * kurs)
                AsDataTableTambahField(dtCost, "jumlahfungsional", AsEnumTypeData.AsDouble)
                dtCost.Columns("jumlahfungsional").Expression = "(jumlah * kurs)"

                'AMBIL TOTAL BIAYA (FUNGSIONAL)
                'totalBiayaFungsional = AsDataTableDSum(dtCost, "jumlahfungsional")
                totalBiayaFungsional = AsDataTableDSum(dtCost, "jumlahfungsional", "termasukhpp = 1")
            Else
                totalBiayaFungsional = 0
            End If
            'END OF AMBIL BIAYA ================================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0, prosentaseHpp As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder


            'PROSES HPP BARANG MASUK ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetail.Rows.Count > 0 Then

                For Each dr1 As DataRow In dtDetail.Rows

                    'PROSES SET HPP --------------------------------------
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idridetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    satuanbarang = dr1("satuanbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : gudang = dr1("gudang")
                    bhpp = dr1("bhpp")
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'PERHITUNGAN PROSENTASE BIAYA YANG MASUK HPP
                    'PROSENTASE BIAYA = (TOTAL PERBARANG FUNGSIONAL / TOTAL TRANSAKSI FUNGSIONAL) 
                    'BIAYA MASUK HPP = PROSENTASE BIAYA * TOTAL BIAYA FUNGSIONAL
                    If totalTransaksiFungsional <> 0 Then
                        prosentaseHpp = (Double.Parse(dr1("totalfungsional")) / totalTransaksiFungsional)
                    Else
                        prosentaseHpp = 0
                    End If

                    'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                    If termasukPajak Then
                        ''hitung hpp = ((((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs) / jmlbarang) + ((prosentaseHpp * totalBiayaFungsional) / jmlbarang)
                        'hpp = ((((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))) + ((prosentaseHpp * totalBiayaFungsional) / Double.Parse(dr1("jmlbarang")))

                        'hitung hpp = ((((jml * harga) - jmldiskon - jmlpajak1) * kurs) / jmlbarang) + ((prosentaseHpp * totalBiayaFungsional) / jmlbarang)
                        hpp = ((((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))) + ((prosentaseHpp * totalBiayaFungsional) / Double.Parse(dr1("jmlbarang")))

                    Else
                        'hitung hpp = ((((jml * harga) - jmldiskon) * kurs) / jmlbarang) + ((prosentaseHpp * totalBiayaFungsional) / jmlbarang)
                        hpp = ((((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))) + ((prosentaseHpp * totalBiayaFungsional) / Double.Parse(dr1("jmlbarang")))

                    End If

                    'INSERT HPP MASUK (FIFO DAN KHUSUS)
                    If bhpp = "F" Then
                        'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                        'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                        strFifo.Clear()
                        'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                        strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                        sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()


                    ElseIf bhpp = "I" Then
                        'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                        'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                        strKhusus.Clear()
                        'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                        strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                        sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    End If

                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If

                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    'sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "', bhargabeli = '" & FixDouble(hpp) & "' WHERE bid = '" & idbarang & "'"
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------


                    'UPDATE HARGAJUAL ==============================================
                    'JIKA TERDAPAT SETTING OTOMATIS UPDATE HARGA JUAL MAKA UPDATE HARGA JUAL BARANG SESUAI RUMUS
                    'RUMUS HARGA JUAL TERDAPAT DI SETTING JUGA
                    'TINGKAT HARGA JUAL YANG YANG DIUPDATE SESUAI DENGAN SETTING (1 - 5)
                    Dim settingHargaJual As String = "", rumusHargaJual As String = ""

                    'HARGAJUAL 1
                    '0  options PembelianUpdateHargaJual1
                    '0  options PembelianUpdateHargaJual1Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 1
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual1")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 1
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual1Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual1 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 2
                    '0  options PembelianUpdateHargaJual2
                    '0  options PembelianUpdateHargaJual2Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 2
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual2")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 2
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual2Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual2 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 3
                    '0  options PembelianUpdateHargaJual3
                    '0  options PembelianUpdateHargaJual3Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 3
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual3")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 3
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual3Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual3 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 4
                    '0  options PembelianUpdateHargaJual4
                    '0  options PembelianUpdateHargaJual4Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 4
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual4")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 4
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual4Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual4 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'HARGAJUAL 5
                    '0  options PembelianUpdateHargaJual5
                    '0  options PembelianUpdateHargaJual5Rumus
                    'AMBIL SETTING UPDATE HARGA JUAL 5
                    settingHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual5")
                    If settingHargaJual.Equals("1") Then
                        'AMBIL RUMUS HARGA JUAL 5
                        rumusHargaJual = F_getSetting(0, "options", "PembelianUpdateHargaJual5Rumus")
                        If Len(rumusHargaJual) > 0 Then
                            sql = "UPDATE m1_item SET bhargajual5 = " & FixQuotes(rumusHargaJual) & " WHERE bid = '" & idbarang & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If
                    'END OF UPDATE HARGAJUAL =======================================


                    ''PROSES JURNAL ---------------------------------------
                    ''AKUN DEBIT ~~~~~~~~~~~~~~~~~~~~~~~~~
                    'debitkredit = 0

                    ''NOMINAL = (((jml * harga) - jmldiskon) * kurs
                    'nominal = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))
                    ''NOMINAL VALAS JIKA MENGGUNAKAN MATAUANG VALAS
                    'If dr1("matauang").ToString <> matauang Then
                    '    'NOMINAL VALAS = (((jml * harga) - jmldiskon)
                    '    nominalvalas = ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")))
                    'Else
                    '    nominalvalas = 0
                    'End If

                    ''GROUPING AKUN DEBIT (rekpersediaan)
                    'filter = "debitkredit=" & debitkredit & " AND norek='" & dr1("rekpersediaan").ToString & "'"
                    'If AsDataTableDCount(dtjurnal, filter) > 0 Then
                    '    nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                    '    nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                    '    If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                    '        rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                    '    End If
                    'Else
                    '    If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                    '                             String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", dr1("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("ricatatan").ToString, dr1("costcenter").ToString, dr1("divisi").ToString, dr1("subdivisi").ToString, dr1("proyek").ToString, urutan)) = False Then
                    '        rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                    '    End If
                    'End If
                    ''END OF AKUN DEBIT ~~~~~~~~~~~~~~~~~~
                    ''END OF PROSES JURNAL --------------------------------

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK ====================================


            'BUAT SQL ============================================================
            'Dim strValue As New StringBuilder

            ''URUTKAN JURNAL
            'dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            ''DELETE JURNAL JIKA NOMIAL = 0
            'AsDataTableDeleteData(dtjurnal, "nominal = 0")

            'For Each drjurnal As DataRow In dtjurnal.Rows
            '    urutan = urutan + 1
            '    'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
            '    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
            '    'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
            '    If drjurnal("debitkredit") = 0 Then
            '        'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
            '        strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(drutama("risumber")) & "', " & 0 & ", " & drutama("riid") & ", '" & FixQuotes(drutama("rinotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("rikodepa") & ", " & drutama("risupplier") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("rimatauang")) & "', '" & FixDouble(drutama("rikurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 0, 0, 'O', '0', 0, " & drutama("ristatus") & ", 1, NOW(), " & drutama("rijmlrevisi") & ", " & drutama("ricetakanke") & ", " & drutama("riinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("riinputtgl"), formatTglWaktuDB)) & "', " & drutama("rimodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("rimodifikasitgl"), formatTglWaktuDB)) & "')")
            '    Else
            '        'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
            '        strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(drutama("risumber")) & "', " & 0 & ", " & drutama("riid") & ", '" & FixQuotes(drutama("rinotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("rikodepa") & ", " & drutama("risupplier") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("rimatauang")) & "', '" & FixDouble(drutama("rikurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 0, 0, 'O', '0', 0, " & drutama("ristatus") & ", 1, NOW(), " & drutama("rijmlrevisi") & ", " & drutama("ricetakanke") & ", " & drutama("riinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("riinputtgl"), formatTglWaktuDB)) & "', " & drutama("rimodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("rimodifikasitgl"), formatTglWaktuDB)) & "')")
            '    End If
            'Next


            ''BUAT SQL JURNAL
            'If Len(strValue.ToString) > 0 Then
            '    sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myConn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sqlJurnal
            '    End With
            '    objCmd.ExecuteNonQuery()
            'End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING
            sqlPosting = "UPDATE M4_Ri SET riposting = 1, ripostingtgl = NOW() WHERE riid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

#Region "M4_Prt"
    Public Function M4_Prt(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'GROUP 1 : =====================
        'HUTANG USAHA   (-D)
        'DISKON         (-D)
        '           PPN MASUKAN1    (-K)
        '           PPN MASUKAN2    (-K)
        '           BIAYA LAIN      (-K)
        '           RETUR PEMBELIAN (+K)

        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 SUDAH DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'HPP            (+D)
        '           PERSEDIAAN      (-K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PRT' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT prt.* FROM m4_prt prt WHERE (prt.prtstatus = 2 OR prt.prtstatus = 3 OR prt.prtstatus = 4 OR prt.prtstatus = 7) AND prt.prtid = '" & idtransaksi & "'", myConn)
            'DETAIL
            dtDetail = AsDataTableAmbilDariDBCon("SELECT prtd.*, i.bhpp, i.bkode as kodebarang, i.bjenis FROM m4_prt_detail prtd JOIN m4_prt prt ON prtd.idprt = prt.prtid JOIN m1_item i ON prtd.idbarang = i.bid WHERE (prt.prtstatus = 2 OR prt.prtstatus = 3 OR prt.prtstatus = 4 OR prt.prtstatus = 7) AND prt.prtid = '" & idtransaksi & "' ORDER BY prtd.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("prtid")
                sumber = drutama("prtsumber")
                noTransaksi = drutama("prtnotransaksi")
                termasukPajak = Integer.Parse(drutama("prthargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder


            'PROSES HPP BARANG KELUAR ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG KELUAR
            If dtDetail.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                For Each dr1 As DataRow In dtDetail.Rows
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idprtdetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtransit") : bhpp = dr1("bhpp") : idhppikm = Double.Parse(dr1("idhppkhususmasuk"))
                    urutanDetail = Double.Parse(dr1("urutan")) : bjenis = dr1("bjenis") : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI TRANSAKSI
                            'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                            If termasukPajak Then
                                ''hitung hpp = (((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs) / jmlbarang
                                'hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                                'hitung hpp = (((jml * harga) - jmldiskon - jmlpajak1) * kurs) / jmlbarang
                                hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                            Else
                                'hitung hpp = (((jml * harga) - jmldiskon) * kurs) / jmlbarang
                                hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                            End If

                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    Else
                        'BARANG JASA ######################################

                        'AMBIL HPP DARI TRANSAKSI
                        'PERHITUNGAN BERDASARKAN HARGA TERMASUK PAJAK ATAU TIDAK
                        If termasukPajak Then
                            ''hitung hpp = (((jml * harga) - jmldiskon - jmlpajak1 - jmlpajak2) * kurs) / jmlbarang
                            'hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1")) - Double.Parse(dr1("jmlpajak2"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                            'hitung hpp = (((jml * harga) - jmldiskon - jmlpajak1) * kurs) / jmlbarang
                            hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")) - Double.Parse(dr1("jmlpajak1"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                        Else
                            'hitung hpp = (((jml * harga) - jmldiskon) * kurs) / jmlbarang
                            hpp = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) * Double.Parse(dr1("kurs"))) / Double.Parse(dr1("jmlbarang"))

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M4_prt_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idprtdetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG KELUAR ====================================


            'BUAT JURNAL HPP PERSEDIAAN =========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT prtd.*, i.bjenis FROM M4_prt_detail prtd JOIN M4_prt prt ON prtd.idprt = prt.prtid JOIN m1_item i ON prtd.idbarang = i.bid WHERE (prt.prtstatus = 2 OR prt.prtstatus = 3 OR prt.prtstatus = 4 OR prt.prtstatus = 7) AND prt.prtid = '" & idtransaksi & "'", myConn)
            If dtDetail.Rows.Count > 0 Then
                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("prtmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("prtcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("prtmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek  ='" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("prtcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next
            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder
            Dim jGrup As Integer = 2
            'JURNAL GRUP 2 -------------------------------------------

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                              tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,              tsaldoawal, tadjustment,                          tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(drutama("prtsumber")) & "', " & 0 & ", " & drutama("prtid") & ", '" & FixQuotes(drutama("prtnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtkodepa") & ", " & drutama("prtsupplier") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("prtmatauang")) & "', '" & FixDouble(drutama("prtkurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', " & FixDouble(drutama("prtstatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("prtsaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', " & FixDouble(jGrup) & ", 0, 'O', '0', 0, " & drutama("prtstatus") & ", 1, NOW(), " & drutama("prtjmlrevisi") & ", " & drutama("prtcetakanke") & ", " & drutama("prtinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("prtinputtgl"), formatTglWaktuDB)) & "', " & drutama("prtmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("prtmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                              tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,              tsaldoawal, tadjustment,                          tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(drutama("prtsumber")) & "', " & 0 & ", " & drutama("prtid") & ", '" & FixQuotes(drutama("prtnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtkodepa") & ", " & drutama("prtsupplier") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("prtmatauang")) & "', '" & FixDouble(drutama("prtkurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', " & FixDouble(drutama("prtstatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("prtsaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', " & FixDouble(jGrup) & ", 0, 'O', '0', 0, " & drutama("prtstatus") & ", 1, NOW(), " & drutama("prtjmlrevisi") & ", " & drutama("prtcetakanke") & ", " & drutama("prtinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("prtinputtgl"), formatTglWaktuDB)) & "', " & drutama("prtmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("prtmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next
            'END OF JURNAL GRUP 2 ------------------------------------


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING PRT
            sqlPosting = "UPDATE M4_prt SET prtposting = 1, prtpostingtgl = NOW() WHERE prtid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL =====================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

    '********************************************** M5 **********************************************

#Region "M5_Si"
    Public Function M5_SiOld(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'GROUP 1 : =====================
        'PIUTANG USAHA (D)
        'DISKON        (D)
        '           PPN KELUARAN1     (K)
        '           PPN KELUARAN2     (K)
        '           PEND. PENJUALAN   (K)
        '           PEND. LAIN        (K)

        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 SUDAH DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'HPP           (D)
        '           PERSEDIAAN        (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable, dtDetailIn As New DataTable, dtMaterial As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SI' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT si.* FROM m5_si si WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "'", myConn)
            'MATERIAL
            dtMaterial = AsDataTableAmbilDariDBCon("SELECT sim.idsimaterial, sim.idbarang, i.bkode as kodebarang, sim.namabarang, sim.tipebarang, sim.satuan, sim.nilaisatuan, sim.satuanbarang, sim.jmlbarang, sim.hpp, sim.gudangtujuan, sim.urutan, sim.idhppkhususmasuk, i.bhpp, i.bjenis FROM m5_si_material sim JOIN m5_si si ON sim.idsi = si.siid JOIN m1_item i ON sim.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sim.urutan", myConn)
            'DETAIL MASUK
            dtDetailIn = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, i.bkode as kodebarang, sid.namabarang, sid.tipebarang, sid.satuan, sid.nilaisatuan, sid.satuanbarang, sid.jmlbarang, sid.hpp, sid.gudangtujuan, sid.urutan, sid.idhppkhususmasuk, i.bhpp, i.bjenis FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly = 1 WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sid.urutan", myConn)
            'DETAIL KELUAR
            dtDetail = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, i.bkode as kodebarang, sid.namabarang, sid.tipebarang, sid.satuan, sid.nilaisatuan, sid.satuanbarang, sid.jmlbarang, sid.hpp, sid.gudangtujuan, sid.urutan, sid.idhppkhususmasuk, i.bhpp, i.bjenis FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sid.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("siid")
                sumber = drutama("sisumber")
                noTransaksi = drutama("sinotransaksi")
                termasukPajak = Integer.Parse(drutama("sihargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'CEK DATA DETAIL DAN MATERIAL ======================================
            If dtDetail.Rows.Count < 1 And dtMaterial.Rows.Count < 1 Then
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'CEK DATA DETAIL DAN MATERIAL ======================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable, strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder


            '1. DATA MATERIAL KELUAR
            'PROSES HPP BARANG KELUAR ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG KELUAR
            If dtMaterial.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtMaterial.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsimaterial"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI BARANG
                            sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Material Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Material Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Material Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    Else
                        'BARANG JASA ######################################
                        'hitung hpp = hpp transaksi
                        hpp = Double.Parse(dr1("hpp"))

                    End If


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(hpp) & "' WHERE idsimaterial = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG KELUAR ====================================


            '2. DATA DETAIL MASUK
            'PROSES HPP BARANG MASUK ============================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetailIn.Rows.Count > 0 Then
                Dim dtHppMasuk As New DataTable

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtDetailIn.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsidetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'AMBIL HPP DARI BARANG PENYUSUN - DETAIL MATERIAL
                    dtHppMasuk = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(jmlbarang * hpp),0) as hpp FROM m5_si_material WHERE idsidetail = '" & iddetail & "'", myConn)
                    If dtHppMasuk.Rows.Count > 0 Then
                        hpp = Double.Parse(dtHppMasuk.Rows(0)("hpp")) / jmlbarang
                    Else
                        hpp = 0
                    End If


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'INSERT HPP MASUK (FIFO DAN KHUSUS)
                        If bhpp = "F" Then
                            'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                        ElseIf bhpp = "I" Then
                            'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                            'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                            strKhusus.Clear()
                            'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                            strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                            sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M5_si_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK =====================================


            '3. DATA DETAIL KELUAR
            'PROSES HPP BARANG KELUAR ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG KELUAR
            If dtDetail.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtDetail.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsidetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI BARANG
                            sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    Else
                        'BARANG JASA ######################################
                        'hitung hpp = hpp transaksi
                        hpp = Double.Parse(dr1("hpp"))

                    End If


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M5_si_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG KELUAR ====================================





            'BUAT JURNAL HPP PERSEDIAAN ==========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT sid.*, i.bjenis FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly <> 1 WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.sicarabayar <> 2 AND si.siid = '" & idtransaksi & "'", myConn)
            'AMBIL DATA MATERIAL YANG BARU
            dtMaterial = AsDataTableAmbilDariDBCon("SELECT sim.*, i.bjenis FROM m5_si_material sim JOIN m5_si si ON sim.idsi = si.siid JOIN m1_item i ON sim.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.sicarabayar <> 2 AND si.siid = '" & idtransaksi & "'", myConn)

            If dtDetail.Rows.Count < 1 And dtMaterial.Rows.Count < 1 Then
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If

            'PROSES DETAIL
            If dtDetail.Rows.Count > 0 Then
                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Detail : Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Detail : Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit  =" & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Detail : Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Detail : Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If

            'PROSES MATERIAL
            If dtMaterial.Rows.Count > 0 Then
                For Each drdetail As DataRow In dtMaterial.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Material : Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Material : Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Material : Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Material : Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,          tsaldoawal, tadjustment,                         tcostcenter,                                 tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sicabang")) & "', '" & FixQuotes(drutama("silokasi")) & "', '" & FixQuotes(drutama("sisumber")) & "', " & 0 & ", " & drutama("siid") & ", '" & FixQuotes(drutama("sinotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl"))) & "', " & drutama("sikodepa") & ", " & drutama("sicustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("siuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("simatauang")) & "', '" & FixDouble(drutama("sikurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("sitgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgllunas"))) & "', " & FixDouble(drutama("sistatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("sisaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("sistatus") & ", 1, NOW(), " & drutama("sijmlrevisi") & ", " & drutama("sicetakanke") & ", " & drutama("siinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("siinputtgl"), formatTglWaktuDB)) & "', " & drutama("simodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("simodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,          tsaldoawal, tadjustment,                         tcostcenter,                                 tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sicabang")) & "', '" & FixQuotes(drutama("silokasi")) & "', '" & FixQuotes(drutama("sisumber")) & "', " & 0 & ", " & drutama("siid") & ", '" & FixQuotes(drutama("sinotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl"))) & "', " & drutama("sikodepa") & ", " & drutama("sicustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("siuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("simatauang")) & "', '" & FixDouble(drutama("sikurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("sitgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgllunas"))) & "', " & FixDouble(drutama("sistatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("sisaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("sistatus") & ", 1, NOW(), " & drutama("sijmlrevisi") & ", " & drutama("sicetakanke") & ", " & drutama("siinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("siinputtgl"), formatTglWaktuDB)) & "', " & drutama("simodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("simodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next

            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If

            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SI
            sqlPosting = "UPDATE m5_si SET siposting = 1, sipostingtgl = NOW() WHERE siid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL =====================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function

    '20150919
    Public Function M5_Si(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'GROUP 1 : =====================
        'PIUTANG USAHA (D)
        'DISKON        (D)
        '           PPN KELUARAN1     (K)
        '           PPN KELUARAN2     (K)
        '           PEND. PENJUALAN   (K)
        '           PEND. LAIN        (K)

        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 SUDAH DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'HPP           (D)
        '           PERSEDIAAN        (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable, dtDetailIn As New DataTable, dtMaterial As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SI' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT si.* FROM m5_si si WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "'", myConn)
            'MATERIAL
            dtMaterial = AsDataTableAmbilDariDBCon("SELECT sim.idsimaterial, sim.idbarang, i.bkode as kodebarang, sim.namabarang, sim.tipebarang, sim.satuan, sim.nilaisatuan, sim.satuanbarang, sim.jmlbarang, sim.hpp, sim.gudangtujuan, sim.urutan, sim.idhppkhususmasuk, i.bhpp, i.bjenis, i.bhargabeli FROM m5_si_material sim JOIN m5_si si ON sim.idsi = si.siid JOIN m1_item i ON sim.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sim.urutan", myConn)
            'DETAIL MASUK
            dtDetailIn = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, i.bkode as kodebarang, sid.namabarang, sid.tipebarang, sid.satuan, sid.nilaisatuan, sid.satuanbarang, sid.jmlbarang, sid.hpp, sid.gudangtujuan, sid.urutan, sid.idhppkhususmasuk, i.bhpp, i.bjenis, i.bhargabeli FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly = 1 WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sid.urutan", myConn)
            'DETAIL KELUAR
            dtDetail = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, i.bkode as kodebarang, sid.namabarang, sid.tipebarang, sid.satuan, sid.nilaisatuan, sid.satuanbarang, sid.jmlbarang, sid.hpp, sid.gudangtujuan, sid.urutan, sid.idhppkhususmasuk, i.bhpp, i.bjenis, i.bhargabeli FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sid.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("siid")
                sumber = drutama("sisumber")
                noTransaksi = drutama("sinotransaksi")
                termasukPajak = Integer.Parse(drutama("sihargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'CEK DATA DETAIL DAN MATERIAL ======================================
            If dtDetail.Rows.Count < 1 And dtMaterial.Rows.Count < 1 Then
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'CEK DATA DETAIL DAN MATERIAL ======================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable, strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder


            '1. DATA MATERIAL KELUAR
            'PROSES HPP BARANG KELUAR ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG KELUAR
            If dtMaterial.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtMaterial.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsimaterial"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis = "P" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI BARANG
                            sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Material Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Material Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Material Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    ElseIf bjenis = "J" Then
                        'BARANG JASA ######################################
                        'hitung hpp = hpp transaksi
                        hpp = Double.Parse(dr1("hpp"))

                    ElseIf bjenis = "V" Then
                        'BARANG VOUCHER ######################################
                        'hitung hpp = bhargabeli
                        hpp = Double.Parse(dr1("bhargabeli"))

                    End If


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE m5_si_material SET hpp = '" & FixDouble(hpp) & "' WHERE idsimaterial = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG KELUAR ====================================


            '2. DATA DETAIL MASUK
            'PROSES HPP BARANG MASUK ============================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetailIn.Rows.Count > 0 Then
                Dim dtHppMasuk As New DataTable

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtDetailIn.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsidetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'AMBIL HPP DARI BARANG PENYUSUN - DETAIL MATERIAL
                    dtHppMasuk = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(jmlbarang * hpp),0) as hpp FROM m5_si_material WHERE idsidetail = '" & iddetail & "'", myConn)
                    If dtHppMasuk.Rows.Count > 0 Then
                        hpp = Double.Parse(dtHppMasuk.Rows(0)("hpp")) / jmlbarang
                    Else
                        hpp = 0
                    End If


                    If bjenis = "P" Then
                        'BARANG PERSEDIAAN ################################

                        'INSERT HPP MASUK (FIFO DAN KHUSUS)
                        If bhpp = "F" Then
                            'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                        ElseIf bhpp = "I" Then
                            'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                            'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                            strKhusus.Clear()
                            'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                            strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                            sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M5_si_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK =====================================


            '3. DATA DETAIL KELUAR
            'PROSES HPP BARANG KELUAR ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG KELUAR
            If dtDetail.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtDetail.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idsidetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis = "P" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI BARANG
                            sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    ElseIf bjenis = "J" Then
                        'BARANG JASA ######################################
                        'hitung hpp = hpp transaksi
                        hpp = Double.Parse(dr1("hpp"))

                    ElseIf bjenis = "V" Then
                        'BARANG VOUCHER ######################################
                        'hitung hpp = bhargabeli
                        hpp = Double.Parse(dr1("bhargabeli"))

                    End If


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M5_si_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsidetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG KELUAR ====================================





            'BUAT JURNAL HPP PERSEDIAAN ==========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT sid.*, i.bjenis FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly <> 1 WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.sicarabayar <> 2 AND si.siid = '" & idtransaksi & "'", myConn)
            'AMBIL DATA MATERIAL YANG BARU
            dtMaterial = AsDataTableAmbilDariDBCon("SELECT sim.*, i.bjenis FROM m5_si_material sim JOIN m5_si si ON sim.idsi = si.siid JOIN m1_item i ON sim.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.sicarabayar <> 2 AND si.siid = '" & idtransaksi & "'", myConn)

            If dtDetail.Rows.Count < 1 And dtMaterial.Rows.Count < 1 Then
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If

            'PROSES DETAIL
            If dtDetail.Rows.Count > 0 Then
                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Detail : Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Detail : Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit  =" & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Detail : Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Detail : Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If

            'PROSES MATERIAL
            If dtMaterial.Rows.Count > 0 Then
                For Each drdetail As DataRow In dtMaterial.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Material : Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Material : Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("simatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Material : Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("sicatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Material : Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,          tsaldoawal, tadjustment,                         tcostcenter,                                 tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sicabang")) & "', '" & FixQuotes(drutama("silokasi")) & "', '" & FixQuotes(drutama("sisumber")) & "', " & 0 & ", " & drutama("siid") & ", '" & FixQuotes(drutama("sinotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl"))) & "', " & drutama("sikodepa") & ", " & drutama("sicustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("siuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("simatauang")) & "', '" & FixDouble(drutama("sikurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("sitgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgllunas"))) & "', " & FixDouble(drutama("sistatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("sisaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("sistatus") & ", 1, NOW(), " & drutama("sijmlrevisi") & ", " & drutama("sicetakanke") & ", " & drutama("siinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("siinputtgl"), formatTglWaktuDB)) & "', " & drutama("simodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("simodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,          tsaldoawal, tadjustment,                         tcostcenter,                                 tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("sicabang")) & "', '" & FixQuotes(drutama("silokasi")) & "', '" & FixQuotes(drutama("sisumber")) & "', " & 0 & ", " & drutama("siid") & ", '" & FixQuotes(drutama("sinotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl"))) & "', " & drutama("sikodepa") & ", " & drutama("sicustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("siuraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("simatauang")) & "', '" & FixDouble(drutama("sikurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("sitgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgllunas"))) & "', " & FixDouble(drutama("sistatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("sisaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("sistatus") & ", 1, NOW(), " & drutama("sijmlrevisi") & ", " & drutama("sicetakanke") & ", " & drutama("siinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("siinputtgl"), formatTglWaktuDB)) & "', " & drutama("simodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("simodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next

            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If

            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SI
            sqlPosting = "UPDATE m5_si SET siposting = 1, sipostingtgl = NOW() WHERE siid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL =====================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function

#End Region

#Region "M5_Sr"
    '20150919
    Public Function M5_Sr(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'GROUP 1 : =====================
        'RETUR PENJUALAN  (D)
        'PPN KELUARAN1    (D)
        'PPN KELUARAN2    (D)
        'PEND. LAIN       (D)
        '       DISKON         (K)
        '       PIUTANG USAHA  (K)

        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'PERSEDIAAN       (D)
        '           HPP        (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'SR' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT sr.* FROM m5_sr sr WHERE (sr.srstatus = 2 OR sr.srstatus = 3 OR sr.srstatus = 4 OR sr.srstatus = 7) AND sr.srid = '" & idtransaksi & "'", myConn)
            'DETAIL (JIKA AMBIL DARI SI MAKA HPP DIAMBILKAN DARI SI DETAIL, JIKA TIDAK AMBIL SI MAKA HPP AMBIL DARI SR DETAIL)
            dtDetail = AsDataTableAmbilDariDBCon("SELECT srd.idsrdetail, srd.idbarang, i.bkode as kodebarang, srd.namabarang, srd.tipebarang, srd.satuan, srd.nilaisatuan, srd.satuanbarang, srd.jmlbarang, srd.gudangtujuan, srd.urutan, srd.idsidetail, i.bhpp, IFNULL(sid.hpp, srd.hpp) as hpp, i.bjenis FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail WHERE (sr.srstatus = 2 OR sr.srstatus = 3 OR sr.srstatus = 4 OR sr.srstatus = 7) AND sr.srid = '" & idtransaksi & "' ORDER BY srd.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("srid")
                sumber = drutama("srsumber")
                noTransaksi = drutama("srnotransaksi")
                termasukPajak = Integer.Parse(drutama("srhargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = "", urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder


            'PROSES HPP BARANG MASUK ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetail.Rows.Count > 0 Then

                For Each dr1 As DataRow In dtDetail.Rows

                    'PROSES SET HPP --------------------------------------
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idsrdetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    satuanbarang = dr1("satuanbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : gudang = dr1("gudangtujuan")
                    bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'hitung hpp = hpp
                    hpp = Double.Parse(dr1("hpp"))

                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'INSERT HPP MASUK (FIFO DAN KHUSUS)
                        If bhpp = "F" Then
                            'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                        ElseIf bhpp = "I" Then
                            'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                            'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                            strKhusus.Clear()
                            'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                            strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                            sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M5_sr_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idsrdetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK ====================================


            'BUAT JURNAL HPP PERSEDIAAN ==========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT srd.*, i.bjenis FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid WHERE (sr.srstatus = 2 OR sr.srstatus = 3 OR sr.srstatus = 4 OR sr.srstatus = 7) AND sr.srid = '" & idtransaksi & "'", myConn)

            If dtDetail.Rows.Count > 0 Then

                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")
                    hpp = Double.Parse(drdetail("hpp"))

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("srmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("srcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("srmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("srcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder
            Dim jGrup As Integer = 2

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,             tsaldoawal, tadjustment,                       tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(drutama("srsumber")) & "', " & 0 & ", " & drutama("srid") & ", '" & FixQuotes(drutama("srnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srkodepa") & ", " & drutama("srcustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("srmatauang")) & "', '" & FixDouble(drutama("srkurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', " & FixDouble(drutama("srstatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("srsaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', " & FixDouble(jGrup) & ", 0, 'O', '0', 0, " & drutama("srstatus") & ", 1, NOW(), " & drutama("srjmlrevisi") & ", " & drutama("srcetakanke") & ", " & drutama("srinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("srinputtgl"), formatTglWaktuDB)) & "', " & drutama("srmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("srmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,             tsaldoawal, tadjustment,                       tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(drutama("srsumber")) & "', " & 0 & ", " & drutama("srid") & ", '" & FixQuotes(drutama("srnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srkodepa") & ", " & drutama("srcustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("srmatauang")) & "', '" & FixDouble(drutama("srkurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', " & FixDouble(drutama("srstatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(drutama("srsaldoawal")) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', " & FixDouble(jGrup) & ", 0, 'O', '0', 0, " & drutama("srstatus") & ", 1, NOW(), " & drutama("srjmlrevisi") & ", " & drutama("srcetakanke") & ", " & drutama("srinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("srinputtgl"), formatTglWaktuDB)) & "', " & drutama("srmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("srmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SR
            sqlPosting = "UPDATE M5_sr SET srposting = 1, srpostingtgl = NOW() WHERE srid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================

            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

#Region "M5_Rnr"
    '20150919
    Public Function M5_Rnr(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'PERSEDIAAN       (D)
        '           HPP        (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        Dim termasukPajak As Integer = 0 '0 = tidak termasuk pajak, 1 = termasuk pajak

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RNR' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT rnr.* FROM m5_rnr rnr WHERE (rnr.rnrstatus = 2 OR rnr.rnrstatus = 3 OR rnr.rnrstatus = 4 OR rnr.rnrstatus = 7) AND rnr.rnrid = '" & idtransaksi & "'", myConn)
            'DETAIL (JIKA AMBIL DARI SI MAKA HPP DIAMBILKAN DARI SI DETAIL, JIKA TIDAK AMBIL SI MAKA HPP AMBIL DARI RNR DETAIL)
            dtDetail = AsDataTableAmbilDariDBCon("SELECT rnrd.idrnrdetail, rnrd.idbarang, i.bkode as kodebarang, rnrd.namabarang, rnrd.tipebarang, rnrd.satuan, rnrd.nilaisatuan, rnrd.satuanbarang, rnrd.jmlbarang, rnrd.gudangtujuan, rnrd.urutan, rnrd.idsidetail, i.bhpp, IFNULL(sid.hpp, rnrd.hpp) as hpp, i.bjenis FROM m5_rnr_detail rnrd JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid JOIN m1_item i ON rnrd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON rnrd.idsidetail = sid.idsidetail WHERE (rnr.rnrstatus = 2 OR rnr.rnrstatus = 3 OR rnr.rnrstatus = 4 OR rnr.rnrstatus = 7) AND rnr.rnrid = '" & idtransaksi & "' ORDER BY rnrd.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("rnrid")
                sumber = drutama("rnrsumber")
                noTransaksi = drutama("rnrnotransaksi")
                termasukPajak = Integer.Parse(drutama("rnrhargatermasukpajak"))
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = "", urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder


            'PROSES HPP BARANG MASUK ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetail.Rows.Count > 0 Then

                For Each dr1 As DataRow In dtDetail.Rows

                    'PROSES SET HPP --------------------------------------
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idrnrdetail"))
                    idbarang = Double.Parse(dr1("idbarang")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    satuanbarang = dr1("satuanbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : gudang = dr1("gudangtujuan")
                    bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'hitung hpp = hpp
                    hpp = Double.Parse(dr1("hpp"))

                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'INSERT HPP MASUK (FIFO DAN KHUSUS)
                        If bhpp = "F" Then
                            'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                        ElseIf bhpp = "I" Then
                            'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                            'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                            strKhusus.Clear()
                            'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                            strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                            sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M5_rnr_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idrnrdetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK ====================================


            'BUAT JURNAL HPP PERSEDIAAN ==========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT rnrd.*, i.bjenis FROM m5_rnr_detail rnrd JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid JOIN m1_item i ON rnrd.idbarang = i.bid WHERE (rnr.rnrstatus = 2 OR rnr.rnrstatus = 3 OR rnr.rnrstatus = 4 OR rnr.rnrstatus = 7) AND rnr.rnrid = '" & idtransaksi & "'", myConn)

            If dtDetail.Rows.Count > 0 Then

                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")
                    hpp = Double.Parse(drdetail("hpp"))

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("rnrmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekhargapokok").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("rnrcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                        If drutama("rnrmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                        filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("rnrcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder
            Dim jGrup As Integer = 2

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,             tsaldoawal, tadjustment,                       tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("rnrcabang")) & "', '" & FixQuotes(drutama("rnrlokasi")) & "', '" & FixQuotes(drutama("rnrsumber")) & "', " & 0 & ", " & drutama("rnrid") & ", '" & FixQuotes(drutama("rnrnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', " & drutama("rnrkodepa") & ", " & drutama("rnrcustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("rnruraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("rnrmatauang")) & "', '" & FixDouble(drutama("rnrkurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrtgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgllunas"))) & "', " & FixDouble(drutama("rnrstatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(0) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', " & FixDouble(jGrup) & ", 0, 'O', '0', 0, " & drutama("rnrstatus") & ", 1, NOW(), " & drutama("rnrjmlrevisi") & ", " & drutama("rnrcetakanke") & ", " & drutama("rnrinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrinputtgl"), formatTglWaktuDB)) & "', " & drutama("rnrmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,             tsaldoawal, tadjustment,                       tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("rnrcabang")) & "', '" & FixQuotes(drutama("rnrlokasi")) & "', '" & FixQuotes(drutama("rnrsumber")) & "', " & 0 & ", " & drutama("rnrid") & ", '" & FixQuotes(drutama("rnrnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', " & drutama("rnrkodepa") & ", " & drutama("rnrcustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("rnruraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("rnrmatauang")) & "', '" & FixDouble(drutama("rnrkurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrtgljatuhtempo"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgllunas"))) & "', " & FixDouble(drutama("rnrstatuslunas")) & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & FixDouble(0) & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', " & FixDouble(jGrup) & ", 0, 'O', '0', 0, " & drutama("rnrstatus") & ", 1, NOW(), " & drutama("rnrjmlrevisi") & ", " & drutama("rnrcetakanke") & ", " & drutama("rnrinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrinputtgl"), formatTglWaktuDB)) & "', " & drutama("rnrmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING RNR
            sqlPosting = "UPDATE M5_rnr SET rnrposting = 1, rnrpostingtgl = NOW() WHERE rnrid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================

            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

    '********************************************** M6 **********************************************

#Region "M6_Pd"
    Public Function M6_Pd(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'PERSEDIAAN BARANG HASIL (D)
        '           PERSEDIAAN BARANG BAHAN     (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetailIn As New DataTable, dtDetailOut As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PD' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT pd.* FROM M6_pd pd WHERE (pd.pdstatus = 2 OR pd.pdstatus = 3 OR pd.pdstatus = 4 OR pd.pdstatus = 7) AND pd.pdid = '" & idtransaksi & "'", myConn)
            'DETAIL OUT
            dtDetailOut = AsDataTableAmbilDariDBCon("SELECT pdo.idpdout, pdo.idbarang, i.bkode as kodebarang, pdo.namabarang, pdo.tipebarang, pdo.satuanbarang, pdo.satuan, pdo.nilaisatuan, pdo.jmlbarang, pdo.harga, pdo.gudangproduksi, pdo.idhppkhususmasuk, pdo.urutan, i.bhpp, i.bjenis FROM M6_pd_out pdo JOIN M6_pd pd ON pdo.idpd = pd.pdid JOIN m1_item i ON pdo.idbarang = i.bid WHERE (pd.pdstatus = 2 OR pd.pdstatus = 3 OR pd.pdstatus = 4 OR pd.pdstatus = 7) AND pd.pdid = '" & idtransaksi & "' ORDER BY pdo.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("pdid")
                sumber = drutama("pdsumber")
                noTransaksi = drutama("pdnotransaksi")
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable, strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder
            Dim totalHpp As Double = 0, hpppersen As Double = 0


            'PROSES HPP BARANG BAHAN ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG BAHAN - DETAIL OUT
            If dtDetailOut.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                For Each dr1 As DataRow In dtDetailOut.Rows
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idpdout")) : bjenis = dr1("bjenis")
                    idbarang = Double.Parse(dr1("idbarang")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namabarang") : satuanbarang = dr1("satuanbarang") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmlbarang"))
                    gudang = dr1("gudangproduksi") : bhpp = dr1("bhpp") : idhppikm = Double.Parse(dr1("idhppkhususmasuk"))
                    urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI BARANG
                            sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    Else

                        'BARANG JASA ######################################
                        'hitung hpp = harga transaksi
                        hpp = Double.Parse(dr1("harga"))

                    End If


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M6_pd_out SET hpp = '" & FixDouble(hpp) & "' WHERE idpdout = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG BAHAN ====================================


            'AMBIL DATA ----------------------------------------------
            'DETAIL IN
            dtDetailIn = AsDataTableAmbilDariDBCon("SELECT pdi.idpdin, pdi.idbarang, pdi.namabarang, pdi.tipebarang, pdi.satuanbarang, pdi.jmlbarang, pdi.hpppersen, pdi.gudangproduksi, i.bhpp, i.bjenis FROM M6_pd_in pdi JOIN M6_pd pd ON pdi.idpd = pd.pdid JOIN m1_item i ON pdi.idbarang = i.bid  WHERE (pd.pdstatus = 2 OR pd.pdstatus = 3 OR pd.pdstatus = 4 OR pd.pdstatus = 7) AND pd.pdid = '" & idtransaksi & "' ORDER BY pdi.urutan", myConn)


            'PROSES HPP BARANG HASIL ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG BAHAN - DETAIL OUT
            If dtDetailIn.Rows.Count > 0 Then

                'AMBIL TOTAL HPP DARI BARANG BAHAN - DETAIL OUT
                dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(jmlbarang * hpp),0) as totalhpp FROM m6_pd_out WHERE idpd = '" & idutama & "'", myConn)
                If dt.Rows.Count > 0 Then
                    totalHpp = Double.Parse(dt.Rows(0)(0))
                Else
                    totalHpp = 0
                End If

                'PROSES SET HPP --------------------------------------
                For Each dr1 As DataRow In dtDetailIn.Rows
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idpdin")) : bjenis = dr1("bjenis")
                    idbarang = Double.Parse(dr1("idbarang")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namabarang")
                    satuanbarang = dr1("satuanbarang") : jmlbarang = Double.Parse(dr1("jmlbarang")) : gudang = dr1("gudangproduksi")
                    bhpp = dr1("bhpp") : hpppersen = Double.Parse(dr1("hpppersen"))
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'PERHITUNGAN HPP = ((HPPPERSEN / 100) * TOTAL HPP BARANG KELUAR) / JMLBARANG
                    hpp = ((hpppersen / 100) * totalHpp) / jmlbarang


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'INSERT HPP MASUK (FIFO DAN KHUSUS)
                        If bhpp = "F" Then
                            'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                        ElseIf bhpp = "I" Then
                            'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                            'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                            strKhusus.Clear()
                            'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                            strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                            sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M6_pd_in SET hpp = '" & FixDouble(hpp) & "' WHERE idpdin = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Next
                'END OF PROSES SET HPP -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG HASIL ====================================


            'BUAT JURNAL HPP PERSEDIAAN ========================================
            'DETAIL IN
            dtDetailIn = AsDataTableAmbilDariDBCon("SELECT pdi.* FROM M6_pd_in pdi JOIN M6_pd pd ON pdi.idpd = pd.pdid JOIN m1_item i ON pdi.idbarang = i.bid  WHERE (pd.pdstatus = 2 OR pd.pdstatus = 3 OR pd.pdstatus = 4 OR pd.pdstatus = 7) AND pd.pdid = '" & idtransaksi & "'", myConn)
            'DETAIL OUT
            dtDetailOut = AsDataTableAmbilDariDBCon("SELECT pdo.* FROM M6_pd_out pdo JOIN M6_pd pd ON pdo.idpd = pd.pdid JOIN m1_item i ON pdo.idbarang = i.bid  WHERE (pd.pdstatus = 2 OR pd.pdstatus = 3 OR pd.pdstatus = 4 OR pd.pdstatus = 7) AND pd.pdid = '" & idtransaksi & "'", myConn)

            If dtutama.Rows.Count > 0 And dtDetailIn.Rows.Count > 0 And dtDetailOut.Rows.Count > 0 Then

                'AKUN DEBIT ------------------------------------------
                'BARANG HASIL
                For Each drdetail As DataRow In dtDetailIn.Rows

                    debitkredit = 0
                    'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                    'NOMINAL = jmlbarang * hpp
                    nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                    If drutama("pdmatauang").ToString <> matauang Then
                        'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                        nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                        'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                    Else
                        'NOMINAL VALAS = 0 
                        nominalvalas = 0
                    End If

                    'GROUPING AKUN DEBIT (rekpersediaan)
                    'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                    filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                    If AsDataTableDCount(dtjurnal, filter) > 0 Then
                        nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                        nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                            rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    Else
                        If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                 String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN MASUK", nominal, nominalvalas, debitkredit, drutama("pdcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                            rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                Next
                'END OF AKUN DEBIT -----------------------------------


                'AKUN KREDIT -----------------------------------------
                'BARANG BAHAN
                For Each drdetail As DataRow In dtDetailOut.Rows

                    debitkredit = 1
                    'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                    'NOMINAL = jmlbarang * hpp
                    nominal = Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))
                    If drutama("pdmatauang").ToString <> matauang Then
                        'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                        nominalvalas = (Double.Parse(drdetail("jmlbarang")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                        'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                    Else
                        'NOMINAL VALAS = 0 
                        nominalvalas = 0
                    End If

                    'GROUPING AKUN DEBIT (rekpersediaan)
                    'filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "'"
                    filter = "debitkredit = " & debitkredit & " AND norek = '" & drdetail("rekpersediaan").ToString & "' AND costcenter = '" & drdetail("costcenter").ToString & "' AND divisi = '" & drdetail("divisi").ToString & "' AND subdivisi = '" & drdetail("subdivisi").ToString & "' AND proyek = '" & drdetail("proyek").ToString & "'"
                    If AsDataTableDCount(dtjurnal, filter) > 0 Then
                        nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                        nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                        If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                            rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    Else
                        If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                 String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN KELUAR", nominal, nominalvalas, debitkredit, drutama("pdcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                            rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                Next
                'END OF AKUN KREDIT ----------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("pdcabang")) & "', '" & FixQuotes(drutama("pdlokasi")) & "', '" & FixQuotes(drutama("pdsumber")) & "', " & 0 & ", " & drutama("pdid") & ", '" & FixQuotes(drutama("pdnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdtgl"))) & "', " & drutama("pdkodepa") & ", " & drutama("pdbagianpd") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("pduraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("pdmatauang")) & "', '" & FixDouble(drutama("pdkurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("pdstatus") & ", 1, NOW(), " & drutama("pdjmlrevisi") & ", " & drutama("pdcetakanke") & ", " & drutama("pdinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("pdinputtgl"), formatTglWaktuDB)) & "', " & drutama("pdmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("pdmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,ttgljatuhtempo,ttgllunas,tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter,           tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("pdcabang")) & "', '" & FixQuotes(drutama("pdlokasi")) & "', '" & FixQuotes(drutama("pdsumber")) & "', " & 0 & ", " & drutama("pdid") & ", '" & FixQuotes(drutama("pdnotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdtgl"))) & "', " & drutama("pdkodepa") & ", " & drutama("pdbagianpd") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("pduraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("pdmatauang")) & "', '" & FixDouble(drutama("pdkurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '1900-01-01', '1900-01-01', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', 0, 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 1, 0, 'O', '0', 0, " & drutama("pdstatus") & ", 1, NOW(), " & drutama("pdjmlrevisi") & ", " & drutama("pdcetakanke") & ", " & drutama("pdinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("pdinputtgl"), formatTglWaktuDB)) & "', " & drutama("pdmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("pdmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING PD
            sqlPosting = "UPDATE M6_pd SET pdposting = 1, pdpostingtgl = NOW() WHERE pdid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

    '********************************************** M11 *********************************************

#Region "M11_Ak"
    Public Function M11_Ak(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'GROUP 1 : =====================
        'PIUTANG SEMENTARA (D)
        '           PEND. APOTEK (K)

        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 SUDAH DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'HPP           (D)
        '           PERSEDIAAN        (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable, dtDetailIn As New DataTable, dtMaterial As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'AK' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT ak.* FROM m_11_ak ak WHERE (ak.akstatus = 2 OR ak.akstatus = 3 OR ak.akstatus = 4 OR ak.akstatus = 7) AND ak.akid = '" & idtransaksi & "'", myConn)
            'MATERIAL
            'dtMaterial = AsDataTableAmbilDariDBCon("SELECT sim.idsimaterial, sim.idbarang, i.bkode as kodebarang, sim.namabarang, sim.tipebarang, sim.satuan, sim.nilaisatuan, sim.satuanbarang, sim.jmlbarang, sim.hpp, sim.gudangtujuan, sim.urutan, sim.idhppkhususmasuk, i.bhpp, i.bjenis FROM m5_si_material sim JOIN m5_si si ON sim.idsi = si.siid JOIN m1_item i ON sim.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sim.urutan", myConn)
            'DETAIL MASUK
            'dtDetailIn = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, i.bkode as kodebarang, sid.namabarang, sid.tipebarang, sid.satuan, sid.nilaisatuan, sid.satuanbarang, sid.jmlbarang, sid.hpp, sid.gudangtujuan, sid.urutan, sid.idhppkhususmasuk, i.bhpp, i.bjenis FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly = 1 WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "' ORDER BY sid.urutan", myConn)
            'DETAIL KELUAR
            dtDetail = AsDataTableAmbilDariDBCon("SELECT akd.idakdetail, akd.idlayanan, i.bkode as kodebarang, akd.namalayanan, akd.tipebarang, akd.satuan, akd.nilaisatuan, akd.satuandefault, akd.jmltotal, akd.hpp, akd.gudangtujuan, akd.urutan, akd.idhppkhususmasuk, i.bhpp, i.bjenis FROM m_11_ak_detail akd JOIN m_11_ak ak ON akd.idak = ak.akid JOIN m1_item i ON akd.idlayanan = i.bid WHERE (ak.akstatus = 2 OR ak.akstatus = 3 OR ak.akstatus = 4 OR ak.akstatus = 7) AND ak.akid = '" & idtransaksi & "' ORDER BY akd.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("akid")
                sumber = drutama("aksumber")
                noTransaksi = drutama("aknotransaksi")
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            'CEK DATA DETAIL DAN MATERIAL ======================================
            If dtDetail.Rows.Count < 1 Then
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'CEK DATA DETAIL DAN MATERIAL ======================================

            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = ""
            Dim idhppikm As Double = 0, urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable, strKhusus As New StringBuilder, strIdHppFifo As New StringBuilder

            '3. DATA DETAIL KELUAR
            'PROSES HPP BARANG KELUAR ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG KELUAR
            If dtDetail.Rows.Count > 0 Then

                'PROSES SET HPP --------------------------------------
                Dim i As Integer = 0
                For Each dr1 As DataRow In dtDetail.Rows
                    i += 1
                    'SET NILAI VARIABEL
                    iddetail = Double.Parse(dr1("idakdetail"))
                    idbarang = Double.Parse(dr1("idlayanan")) : kodebarang = dr1("kodebarang") : tipebarang = dr1("tipebarang")
                    namabarang = dr1("namalayanan") : satuanbarang = dr1("satuandefault") : satuan = dr1("satuan")
                    nilaisatuan = Double.Parse(dr1("nilaisatuan")) : jmlbarang = Double.Parse(dr1("jmltotal"))
                    gudang = dr1("gudangtujuan") : bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    idhppikm = Double.Parse(dr1("idhppkhususmasuk")) : urutanDetail = Double.Parse(dr1("urutan")) : jenismutasi = 0


                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If


                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'AMBIL HPP SESUAI TIPE HPP
                        If bhpp = "R" Then '//AVERAGE +++++++++++++++++++++
                            'AMBIL HPP DARI BARANG
                            sql = "SELECT bhppaverage as hpp FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))
                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "I" Then '//KHUSUS ++++++++++++++++++
                            'AMBIL HPP DARI HPP KHUSUS MASUK
                            sql = "SELECT harga as hpp FROM m1_cogs_special_in WHERE idhppikm = '" & FixDouble(idhppikm) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                hpp = Double.Parse(dtSaldo.Rows(0)("hpp"))

                                'BUAT QUERY UNTUK INSERT HPP KHUSUS OUT (m1_cogs_special_out)
                                strKhusus.Clear()
                                'mapping           idhppikk,      idbarang,                    sumber,           idtransaksi,      idhppikm,                    satuan,                            jmlkeluar,                      gudang,          isclose
                                strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", " & idhppikm & ", '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                                sql = "Insert into M1_Cogs_Special_Out(idhppikk, idbarang, sumber, idtransaksi, idhppikm, satuan, jmlkeluar, gudang, isclose) values" & strKhusus.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE HPP FIFO IN (m1_cogs_special_in)
                                sql = "UPDATE m1_cogs_special_in SET jmlkeluar = ROUND(jmlkeluar + '" & FixDouble(jmlbarang) & "', 5) WHERE (idhppikm = '" & idhppikm & "')"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                hpp = 0
                            End If


                        ElseIf bhpp = "F" Then '//FIFO ++++++++++++++++++++
                            'RESET strIdHppFifo
                            strIdHppFifo.Clear()

                            'CEK JML HPP FIFO YANG TERSEDIA
                            dt = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(cfisisa),0) as cfisisa FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & idbarang & "'", myConn)
                            If dt.Rows.Count > 0 Then
                                sisa = Double.Parse(dt(0)(0))
                                If jmlbarang > sisa Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : Trans.Rollback() : GoTo selesai
                                End If
                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #1" : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                            'AMBIL DATA HPP FIFO MASUK
                            'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                            'dtFifo = AsDataTableAmbilDariDBCon("SELECT * FROM ( SELECT CAST(@saldobutuh as UNSIGNED) as saldobutuh, cfi.cfisisa as saldotersedia, (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as saldodipakai, cfi.cfiharga as harga, cfi.cfiharga * (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as subtotal, cfi.cfisisa - (CASE WHEN cfi.cfisisa <= CAST(@saldobutuh as UNSIGNED) THEN cfi.cfisisa ELSE CAST(@saldobutuh as UNSIGNED) END) as sisasaldo, (CASE WHEN CAST(@saldobutuh as UNSIGNED) - cfi.cfisisa < 0 THEN @saldobutuh := 0 ELSE @saldobutuh := @saldobutuh - cfi.cfisisa END) as sisabutuh, cfi.cfiid, cfi.cfisatuan FROM m1_cogs_fifo_in cfi, (SELECT @saldobutuh := " & FixDouble(jmlbarang) & ") AS variableInit1 WHERE cfi.cfiisclose = 0 AND cfi.cfiidbarang = " & FixDouble(idbarang) & " ORDER BY cfi.cfiinputtgl ASC ) as hppFifo WHERE saldodipakai > 0", myConn)
                            dtFifo = AsDataTableAmbilDariDBCon("CALL f_cogs_fifo(" & FixDouble(idbarang) & ", " & FixDouble(jmlbarang) & ")", myConn)
                            If dtFifo.Rows.Count > 0 Then

                                'SET NILAI HPP BARU SUM(subtotal) / SUM(saldodipakai)
                                hpp = Double.Parse(AsDataTableDSum(dtFifo, "subtotal")) / Double.Parse(AsDataTableDSum(dtFifo, "saldodipakai"))

                                'PERULANGAN DATA HPP FIFO
                                For Each dr2 As DataRow In dtFifo.Rows
                                    ''BUAT strIdHppFifo UNTUK idhppfifo PADA m1_item_transaction
                                    ''FORMAT idhppfifomasuk,jml,harga|idhppfifomasuk,jml,harga|dst..
                                    'strIdHppFifo.Append(IIf(Len(strIdHppFifo.ToString) > 0, "|", ""))
                                    'strIdHppFifo.Append(dr2("cfiid") & "," & dr2("saldodipakai") & "," & dr2("harga"))

                                    'BUAT QUERY UNTUK INSERT HPP FIFO OUT (m1_cogs_fifo_out)
                                    strFifo.Clear()
                                    'mapping             cfoid,  cfoidbarang,                 cfosumber,         cfoidtransaksi,                     cfosatuan,                             cfojmlkeluar,                     cfoharga,    cfoisclose,          cfoidcfi, cfoinputtgl
                                    strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(dr2("cfisatuan")) & "', '" & FixDouble(dr2("saldodipakai")) & "', '" & FixDouble(dr2("harga")) & "', " & 0 & ", " & dr2("cfiid") & ", NOW())")
                                    sql = "Insert into M1_Cogs_Fifo_Out(cfoid, cfoidbarang, cfosumber, cfoidtransaksi, cfosatuan, cfojmlkeluar, cfoharga, cfoisclose, cfoidcfi, cfoinputtgl) values" & strFifo.ToString & ""
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE HPP FIFO IN (m1_cogs_fifo_in)
                                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = ROUND(cfijmlkeluar + '" & FixDouble(dr2("saldodipakai")) & "', 5) WHERE (cfiid = '" & dr2("cfiid") & "')"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                Next

                            Else
                                If jmlbarang > 0 Then
                                    rsErrMessage = "Detail Row : " & urutanDetail & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list. #2" : Trans.Rollback() : GoTo selesai
                                Else
                                    hpp = 0
                                End If
                            End If

                        End If

                    Else
                        'BARANG JASA ######################################
                        'hitung hpp = hpp transaksi
                        hpp = Double.Parse(dr1("hpp"))

                    End If


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG KELUAR (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML + JMLBARANG)) - (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml + jmlbarang)) - (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M_11_ak_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idakdetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM) DAN HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item JOIN M1_Item_Transaction ON bid = idbarang SET bhppaverage = '" & FixDouble(saldohpp) & "', idhppfifo = '" & FixQuotes(strIdHppFifo.ToString) & "', hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE bid = '" & idbarang & "' AND sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Next
                'END OF PROSES SET HPP -------------------------------

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG KELUAR ====================================

            'BUAT JURNAL HPP PERSEDIAAN ==========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT akd.*, i.bjenis FROM m_11_ak_detail akd JOIN m_11_ak ak ON akd.idak = ak.akid JOIN m1_item i ON akd.idlayanan = i.bid AND i.bassembly <> 1 WHERE (ak.akstatus = 2 OR ak.akstatus = 3 OR ak.akstatus = 4 OR ak.akstatus = 7) AND ak.akid = '" & idtransaksi & "'", myConn)
            'AMBIL DATA MATERIAL YANG BARU
            'dtMaterial = AsDataTableAmbilDariDBCon("SELECT sim.*, i.bjenis FROM m5_si_material sim JOIN m5_si si ON sim.idsi = si.siid JOIN m1_item i ON sim.idbarang = i.bid WHERE (si.sistatus = 2 OR si.sistatus = 3 OR si.sistatus = 4 OR si.sistatus = 7) AND si.siid = '" & idtransaksi & "'", myConn)

            If dtDetail.Rows.Count < 1 Then
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If

            'PROSES DETAIL
            If dtDetail.Rows.Count > 0 Then
                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))
                        If drutama("akmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        filter = "debitkredit=" & debitkredit & " AND norek='" & drdetail("rekhargapokok").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Detail : Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("akcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Detail : Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))
                        If drutama("akmatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        filter = "debitkredit=" & debitkredit & " AND norek='" & drdetail("rekpersediaan").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Detail : Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("akcatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Detail : Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

                'Else
                '    'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                '    rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If

            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,          tsaldoawal, tadjustment,                         tcostcenter,                                 tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("akcabang")) & "', '" & FixQuotes(drutama("aklokasi")) & "', '" & FixQuotes(drutama("aksumber")) & "', " & 0 & ", " & drutama("akid") & ", '" & FixQuotes(drutama("aknotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & drutama("akkodepa") & ", " & drutama("akcustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', CONCAT('" & FixQuotes(drutama("akuraian")) & "', ' ', '" & FixQuotes(drutama("aknoref")) & "'), '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("akmatauang")) & "', '" & FixDouble(drutama("akkurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & 0 & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("akstatus") & ", 1, NOW(), " & drutama("akjmlrevisi") & ", " & drutama("akcetakanke") & ", " & drutama("akinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("akinputtgl"), formatTglWaktuDB)) & "', " & drutama("akmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("akmodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,          tsaldoawal, tadjustment,                         tcostcenter,                                 tdivisi,                                 tsubdivisi,                                tproyek,     tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("akcabang")) & "', '" & FixQuotes(drutama("aklokasi")) & "', '" & FixQuotes(drutama("aksumber")) & "', " & 0 & ", " & drutama("akid") & ", '" & FixQuotes(drutama("aknotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & drutama("akkodepa") & ", " & drutama("akcustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', CONCAT('" & FixQuotes(drutama("akuraian")) & "', ' ', '" & FixQuotes(drutama("aknoref")) & "'), '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("akmatauang")) & "', '" & FixDouble(drutama("akkurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & 0 & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("akstatus") & ", 1, NOW(), " & drutama("akjmlrevisi") & ", " & drutama("akcetakanke") & ", " & drutama("akinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("akinputtgl"), formatTglWaktuDB)) & "', " & drutama("akmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("akmodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next

            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If

            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SI
            sqlPosting = "UPDATE m_11_ak SET akposting = 1, aktglposting = NOW() WHERE akid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL =====================================================


            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

#Region "M11_Ro"
    Public Function M11_Ro(ByVal idtransaksi As Integer) As String 'progress?errMessage?sqljurnal?sqlupdateposting
        'GROUP 1 : =====================
        'RETUR PENJUALAN  (D)
        '       PIUTANG SEMENTARA  (K)

        'DISINI HANYA MEMBUAT GROUP 2, GROUP 1 DIPROSES PADA TOOLS JOURNAL
        'GROUP 2 : =====================
        'PERSEDIAAN       (D)
        '           HPP        (K)

        Dim wsResult As String = ""
        Dim rsProgress As Integer = 0, rsErrMessage As String = "", rsSql As String = ""
        Dim sumber As String = "", noTransaksi As String = "", filter As String = ""
        Dim sql As String = "", idutama As Double = 0, iddetail As Double = 0

        Dim dtutama As New DataTable, dtDetail As New DataTable
        Dim drutama As DataRow, dtSaldo As New DataTable, dt As New DataTable
        Dim debitkredit As Integer = 0, urutan As Integer = 0, nominal As Double = 0, nominalvalas As Double = 0
        Dim sqlPosting As String = "", sqlJurnal As String = ""

        'BUAT DT JURNAL
        Dim dtjurnal As New DataTable
        AsDataTableTambahField(dtjurnal, "norek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsInt64) '0=debit,1=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "urutan", AsEnumTypeData.AsInt64)


        '//TRANSAKSI KE DATABASE ****************************************************************
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '******* Start Transaction ******'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'HAPUS JURNAL LAMA
            Dim sqlHapus As String = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RO' AND tgrup = '2' AND tidtransaksi = '" & FixDouble(idtransaksi) & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlHapus
            End With
            objCmd.ExecuteNonQuery()


            'AMBIL DATA DARI SETTING ---------------------------------
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')", myConn)
            'MATAUANG
            Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
            If matauang = "Not found" Then
                rsErrMessage = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'KURS
            Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
            If kurs = "Not found" Then
                rsErrMessage = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF AMBIL DATA DARI SETTING --------------------------


            'AMBIL DATA ----------------------------------------------
            'UTAMA
            dtutama = AsDataTableAmbilDariDBCon("SELECT ro.* FROM m_11_ro ro WHERE (ro.rostatus = 2 OR ro.rostatus = 3 OR ro.rostatus = 4 OR ro.rostatus = 7) AND ro.roid = '" & idtransaksi & "'", myConn)
            'DETAIL (JIKA AMBIL DARI SI MAKA HPP DIAMBILKAN DARI SI DETAIL, JIKA TIDAK AMBIL SI MAKA HPP AMBIL DARI SR DETAIL)
            dtDetail = AsDataTableAmbilDariDBCon("SELECT rod.idrodetail, rod.idlayanan, i.bkode as kodebarang, rod.namalayanan, rod.tipebarang, rod.satuan, rod.nilaisatuan, rod.satuandefault, rod.jmltotal, rod.gudangtujuan, rod.urutan, rod.hpp, i.bhpp, i.bjenis FROM m_11_ro_detail rod JOIN m_11_ro ro ON rod.idro = ro.roid JOIN m1_item i ON rod.idlayanan = i.bid WHERE (ro.rostatus = 2 OR ro.rostatus = 3 OR ro.rostatus = 4 OR ro.rostatus = 7) AND ro.roid = '" & idtransaksi & "' ORDER BY rod.urutan", myConn)


            'SET DATA UTAMA ====================================================
            If dtutama.Rows.Count > 0 Then
                'SET DATA UTAMA --------------------------------------
                drutama = dtutama.Rows(0)

                'SET SUMBER DAN NOTRANSAKSI
                idutama = drutama("roid")
                sumber = drutama("rosumber")
                noTransaksi = drutama("ronotransaksi")
                'END OF SET DATA UTAMA -------------------------------

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF SET DATA UTAMA =============================================


            Dim idbarang As Double = 0, kodebarang As String = "", tipebarang As String = "", namabarang As String = ""
            Dim satuanbarang As String = "", satuan As String = "", nilaisatuan As Double = 0, jmlbarang As Double = 0, gudang As String = ""
            Dim bhpp As String = "", bjenis As String = "", urutanDetail As Double = 0, hpp As Double = 0
            Dim jenismutasi As Double = 0, postinghpp As Double = 0, hpplama As Double = 0
            Dim saldohpp As Double = 0, saldonilai As Double = 0, saldojml As Double = 0, sisa As Double = 0
            Dim strFifo As New StringBuilder, dtFifo As New DataTable
            Dim strKhusus As New StringBuilder


            'PROSES HPP BARANG MASUK ===========================================
            'JIKA TERDAPAT DATA TRANSAKSI MAKA SET HPP BARANG MASUK
            If dtDetail.Rows.Count > 0 Then

                For Each dr1 As DataRow In dtDetail.Rows

                    'PROSES SET HPP --------------------------------------
                    'SET VARIABEL
                    iddetail = Double.Parse(dr1("idrodetail"))
                    idbarang = Double.Parse(dr1("idlayanan")) : tipebarang = dr1("tipebarang") : namabarang = dr1("namalayanan")
                    satuanbarang = dr1("satuandefault") : jmlbarang = Double.Parse(dr1("jmltotal")) : gudang = dr1("gudangtujuan")
                    bhpp = dr1("bhpp") : bjenis = dr1("bjenis")
                    jenismutasi = 1

                    'AMBIL HPP DARI BARANG, SALDOJML DARI TRANSAKSI BARANG
                    sql = "SELECT i.bhppaverage as hpplama, it.saldojml FROM m1_item i LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.sumber = '" & FixQuotes(sumber) & "' AND it.idutama = '" & FixDouble(idutama) & "' AND it.iddetail = '" & FixDouble(iddetail) & "' AND it.jenismutasi = '" & FixDouble(jenismutasi) & "' WHERE i.bid = '" & FixDouble(idbarang) & "'"
                    dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSaldo.Rows.Count > 0 Then
                        hpplama = Double.Parse(dtSaldo.Rows(0)("hpplama"))
                        saldojml = Double.Parse(dtSaldo.Rows(0)("saldojml"))
                    Else
                        hpplama = 0 : saldojml = 0
                    End If

                    'hitung hpp = hpp
                    hpp = Double.Parse(dr1("hpp"))

                    If bjenis <> "J" Then
                        'BARANG PERSEDIAAN ################################

                        'INSERT HPP MASUK (FIFO DAN KHUSUS)
                        If bhpp = "F" Then
                            'JIKA HPP FIFO MAKA INSERT HPP FIFO MASUK
                            'BUAT QUERY UNTUK INSERT HPP FIFO IN (m1_cogs_fifo_in)
                            strFifo.Clear()
                            'mapping           cfiid,    cfiidbarang,                 cfisumber,         cfiidtransaksi,             cfinamabarang,                  cfitipebarang,                    cfisatuan,                         cfijmlmasuk, cfijmlkeluar,              cfisisa,              cfiharga,cfiisclose,cfiinputtgl
                            strFifo.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "', '" & hpp & "', " & 0 & ", NOW())")
                            sql = "Insert into M1_Cogs_Fifo_In(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) values" & strFifo.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()


                        ElseIf bhpp = "I" Then
                            'JIKA HPP KHUSUS MAKA INSERT HPP KHUSUS MASUK
                            'BUAT QUERY UNTUK INSERT HPP KHUSUS IN (m1_cogs_special_in)
                            strKhusus.Clear()
                            'mapping        idhppikm,         idbarang,                    sumber,            idtransaksi,                namabarang,                      tipebarang,                      satuan,                            jmlmasuk,  jmlkeluar,                  sisa,              hpp                      gudang,        isclose
                            strKhusus.Append("(" & 0 & ", " & idbarang & ", '" & FixQuotes(sumber) & "', " & iddetail & ", '" & FixQuotes(namabarang) & "', '" & FixQuotes(tipebarang) & "', '" & FixQuotes(satuanbarang) & "', '" & FixDouble(jmlbarang) & "', '0', '" & FixDouble(jmlbarang) & "','" & hpp & "', '" & FixQuotes(gudang) & "', " & 0 & ")")
                            sql = "Insert into M1_Cogs_Special_In(idhppikm, idbarang, sumber, idtransaksi, namabarang, tipebarang, satuan, jmlmasuk, jmlkeluar, sisa, harga, gudang, isclose) values" & strKhusus.ToString & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                        End If

                    End If


                    'UPDATE HPP PADA TRANSAKSI
                    sql = "UPDATE M_11_ro_detail SET hpp = '" & FixDouble(hpp) & "' WHERE idrodetail = '" & FixDouble(iddetail) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'PERHITUNGAN SALDOHPP DAN SALDONILAI BARANG MASUK (M1_ITEM_TRANSACTION)
                    'SALDONILAI = ((HPPLAMA * (SALDOJML - JMLBARANG)) + (HPPBARU * JMLBARANG))
                    If saldojml <> 0 Then
                        saldonilai = (hpplama * (saldojml - jmlbarang)) + (hpp * jmlbarang)
                        saldohpp = saldonilai / saldojml
                    Else
                        saldonilai = 0
                        saldohpp = 0
                    End If


                    'UPDATE HPP, SALDOHPP, SALDONILAI PADA TRANSAKSI BARANG
                    sql = "UPDATE M1_Item_Transaction SET hpp = '" & FixDouble(hpp) & "', saldohpp = '" & FixDouble(saldohpp) & "', saldonilai = '" & FixDouble(saldonilai) & "', postinghpp = 1, hppfix = (CASE tipehpp WHEN 'R' THEN 0 ELSE 1 END), postingtgl = NOW() WHERE sumber = '" & sumber & "' AND jenismutasi = '" & jenismutasi & "' AND idutama = '" & idutama & "' AND iddetail = '" & iddetail & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    'UPDATE HPPAVERAGE GLOBAL (M1_ITEM)
                    sql = "UPDATE m1_item SET bhppaverage = '" & FixDouble(saldohpp) & "' WHERE bid = '" & idbarang & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF PROSES SET HPP -------------------------------

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF PROSES HPP BARANG MASUK ====================================


            'BUAT JURNAL HPP PERSEDIAAN ==========================================
            'AMBIL DATA DETAIL YANG BARU
            dtDetail = AsDataTableAmbilDariDBCon("SELECT rod.*, i.bjenis FROM m_11_ro_detail rod JOIN m_11_ro ro ON rod.idro = ro.roid JOIN m1_item i ON rod.idlayanan = i.bid WHERE (ro.rostatus = 2 OR ro.rostatus = 3 OR ro.rostatus = 4 OR ro.rostatus = 7) AND ro.roid = '" & idtransaksi & "'", myConn)

            If dtDetail.Rows.Count > 0 Then

                For Each drdetail As DataRow In dtDetail.Rows
                    'SET VARIABLE
                    bjenis = drdetail("bjenis")
                    hpp = Double.Parse(drdetail("hpp"))

                    'JIKA BUKAN BARANG JASA MAKA BUAT JURNAL
                    If bjenis <> "J" Then

                        'AKUN DEBIT ------------------------------------------
                        'AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 1
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))
                        If drutama("romatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekhargapokok)
                        filter = "debitkredit=" & debitkredit & " AND norek='" & drdetail("rekhargapokok").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekhargapokok").ToString, "HPP", nominal, nominalvalas, debitkredit, drutama("rocatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal debit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN HPP ~~~~~~~~~~~~~~~~~~~~~~~~~
                        'END OF AKUN DEBIT -----------------------------------


                        'AKUN KREDIT -----------------------------------------
                        'AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~~~~~~~~
                        debitkredit = 0
                        'JIKA MENGGUNAKAN MATAUANG VALAS MAKA PERHITUNGAN VALAS, ELSE MAKA PERHITUNGAN BIASA
                        'NOMINAL = jmlbarang * hpp
                        nominal = Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))
                        If drutama("romatauang").ToString <> matauang Then
                            'NOMINAL VALAS = (jmlbarang * hpp) / kurs
                            nominalvalas = (Double.Parse(drdetail("jmltotal")) * Double.Parse(drdetail("hpp"))) / Double.Parse(drdetail("kurs"))

                            'JIKA MENGGUNAKAN MATAUANG FUNGSIONAL MAKA PERHITUNGAN BIASA
                        Else
                            'NOMINAL VALAS = 0 
                            nominalvalas = 0
                        End If

                        'GROUPING AKUN DEBIT (rekpersediaan)
                        filter = "debitkredit=" & debitkredit & " AND norek='" & drdetail("rekpersediaan").ToString & "'"
                        If AsDataTableDCount(dtjurnal, filter) > 0 Then
                            nominal += Double.Parse(AsDataTableDLookup(dtjurnal, "nominal", filter))
                            nominalvalas += Double.Parse(AsDataTableDLookup(dtjurnal, "nominalvalas", filter))
                            If AsDataTableUpdateData(dtjurnal, filter, "nominal~nominalvalas", nominal & "~" & nominalvalas) = False Then
                                rsErrMessage = "Failed update grouping datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        Else
                            If AsDataTableTambahData(dtjurnal, "norek~namaakun~nominal~nominalvalas~debitkredit~catatan~costcenter~divisi~subdivisi~proyek~urutan", _
                                                     String.Format("{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", drdetail("rekpersediaan").ToString, "PERSEDIAAN", nominal, nominalvalas, debitkredit, drutama("rocatatan").ToString, drdetail("costcenter").ToString, drdetail("divisi").ToString, drdetail("subdivisi").ToString, drdetail("proyek").ToString, urutan)) = False Then
                                rsErrMessage = "Failed insert datatable journal kredit " & sumber & " (" & noTransaksi & ") : " & urutan & "." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF AKUN PERSEDIAAN ~~~~~~~~~~~~~~~~~~
                        'END OF AKUN KREDIT ----------------------------------

                    End If

                Next

            Else
                'JIKA TIDAK ADA DATA MAKA UPDATE STATUS MSMQ MENJADI 4(TRANSAKSI TDK APPROVED)
                rsProgress = 4 : Trans.Rollback() : GoTo selesai
            End If
            'END OF BUAT JURNAL HPP PERSEDIAAN ===================================


            'BUAT SQL ============================================================
            Dim strValue As New StringBuilder
            Dim jGrup As Integer = 2

            'URUTKAN JURNAL
            dtjurnal = AsDataTableFilterSortDt(dtjurnal, "", "debitkredit ASC, norek ASC")

            'DELETE JURNAL JIKA NOMIAL = 0
            AsDataTableDeleteData(dtjurnal, "nominal = 0")

            For Each drjurnal As DataRow In dtjurnal.Rows
                urutan = urutan + 1
                'BUAT VALUE SQL INSERT KE M2_TRANSACTION_JOURNAL
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'JIKA debitkredit = 0 maka DEBIT, ELSE KREDIT
                If drjurnal("debitkredit") = 0 Then
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,                          tdebit,              tkredit,                          tdebitvalas,         tkreditvalas, tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,             tsaldoawal, tadjustment,                       tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("rocabang")) & "', '" & FixQuotes(drutama("rolokasi")) & "', '" & FixQuotes(drutama("rosumber")) & "', " & 0 & ", " & drutama("roid") & ", '" & FixQuotes(drutama("ronotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & drutama("rokodepa") & ", " & drutama("rocustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("rouraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("romatauang")) & "', '" & FixDouble(drutama("rokurs")) & "', '', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', '" & 0 & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & 0 & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("rostatus") & ", 1, NOW(), " & drutama("rojmlrevisi") & ", " & drutama("rocetakanke") & ", " & drutama("roinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("roinputtgl"), formatTglWaktuDB)) & "', " & drutama("romodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("romodifikasitgl"), formatTglWaktuDB)) & "')")
                Else
                    'mapping            tid,                             tcabang,                                  tlokasi,                                  tsumber,    tkodetabelangka,       tidtransaksi,                              tnotransaksi,                                                  ttgl,                        tkodepa,                     tkontak,                                tnorek,                                 turaian,                                  tcatatan,                                  tmatauang,                                   tkurs,      tnobon,   tdebit,                             tkredit,         tdebitvalas,                          tkreditvalas,         tcarabayar, thutangpiutang,                                    ttgljatuhtempo,                                                  ttgllunas,                                  tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang,             tsaldoawal, tadjustment,                       tcostcenter,                                tdivisi,                                 tsubdivisi,                                tproyek,                                tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke,                     tinputuser,                                           tinputtgl,                                              tmodifikasiuser,                                                tmodifikasitgl
                    strValue.Append("(" & 0 & ", '" & FixQuotes(drutama("rocabang")) & "', '" & FixQuotes(drutama("rolokasi")) & "', '" & FixQuotes(drutama("rosumber")) & "', " & 0 & ", " & drutama("roid") & ", '" & FixQuotes(drutama("ronotransaksi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & drutama("rokodepa") & ", " & drutama("rocustomer") & ", '" & FixQuotes(drjurnal("norek")) & "', '" & FixQuotes(drutama("rouraian")) & "', '" & FixQuotes(drjurnal("catatan")) & "', '" & FixQuotes(drutama("romatauang")) & "', '" & FixDouble(drutama("rokurs")) & "', '', '" & 0 & "', '" & FixDouble(drjurnal("nominal")) & "', '" & 0 & "', '" & FixDouble(drjurnal("nominalvalas")) & "', " & 0 & ", " & 0 & ", '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & 0 & ", '1900-01-01', " & 0 & ", " & urutan & ", '', " & 0 & ", 0, '" & FixQuotes(drjurnal("costcenter")) & "', '" & FixQuotes(drjurnal("divisi")) & "', '" & FixQuotes(drjurnal("subdivisi")) & "', '" & FixQuotes(drjurnal("proyek")) & "', 2, 0, 'O', '0', 0, " & drutama("rostatus") & ", 1, NOW(), " & drutama("rojmlrevisi") & ", " & drutama("rocetakanke") & ", " & drutama("roinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("roinputtgl"), formatTglWaktuDB)) & "', " & drutama("romodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("romodifikasitgl"), formatTglWaktuDB)) & "')")
                End If
            Next


            'BUAT SQL JURNAL
            If Len(strValue.ToString) > 0 Then
                sqlJurnal = "Insert into M2_Transaction_Journal(tid, tcabang, tlokasi, tsumber, tkodetabelangka, tidtransaksi, tnotransaksi, ttgl, tkodepa, tkontak, tnorek, turaian, tcatatan, tmatauang, tkurs, tnobon, tdebit, tkredit, tdebitvalas, tkreditvalas, tcarabayar, thutangpiutang, ttgljatuhtempo, ttgllunas, tstatuslunas, ttglrekonsiliasi, tsudahrekonsiliasi, turutan, tnohutangpiutang, tsaldoawal, tadjustment, tcostcenter, tdivisi, tsubdivisi, tproyek, tgrup, tretail, tjenisaruskas, tjmlrealisasium, tstatusrealisasi, tstatus, tposting, tpostingtgl, tjmlrevisi, tcetakanke, tinputuser, tinputtgl, tmodifikasiuser, tmodifikasitgl) values" & strValue.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sqlJurnal
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE STATUS POSTING JURNAL TRANSAKSI BARANG
            sql = "UPDATE M1_Item_Transaction SET postingjurnal = 1 WHERE sumber = '" & sumber & "' AND idutama = '" & idutama & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'TAMBAHKAN SQL UNTUK UPDATE TGLPOSTING SR
            sqlPosting = "UPDATE M_11_ro SET roposting = 1, rotglposting = NOW() WHERE roid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sqlPosting
            End With
            objCmd.ExecuteNonQuery()


            'GABUNGKAN VALUE SQL JURNAL HPP , SQL POSTING 
            rsSql = ""
            'END OF BUAT SQL ================================================

            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            rsProgress = 0
            rsErrMessage = "Transaction Rollback : " & ex.Message
            GoTo selesai

        End Try

        objCmd = Nothing
        myConn.Close()
        '//END OF TRANSAKSI KE DATABASE *********************************************************

        rsProgress = 1

selesai:
        wsResult = String.Concat(rsProgress, sptSubParam, rsErrMessage, sptSubParam, rsSql)
        Return wsResult
    End Function
#End Region

End Class
