Imports System.Data
Imports System.Text
Imports System.IO

Public Class HitungUlang
    Dim userid As String = ""   'User Id diisi dengan user yang melakukan proses transaksi
    Dim formatTglDB As String = "yyyy-MM-dd"
    Dim formatTglWaktuDB As String = "yyyy-MM-dd H:mm:ss"
    Dim ObjId(999) As Object
    Dim Obj(999) As Object

    Public Function M0_CogsHitungUlang_Fifo() As String
        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim pg1 As New RsPaging
        Dim search As String = "", stepKe As Double = 0, stepDetail As Double = 0
        Dim Filter As String = "", Sorting As String = ""

        Dim sql As String = ""
        Dim tglAwal As String = "", tglAkhir As String = ""
        Dim kodeBarangAwal As String = "", kodeBarangAkhir As String = ""
        Dim hitungPerBarang As Boolean = False, idbarang As Integer = 0

        Dim strFifoIN As New StringBuilder, strFifoOUT As String = ""

        Dim cfiid As Integer = 0

        Dim id As Integer = 0
        Dim notransaksi As String = ""

        Dim myConn1 As MySql.Data.MySqlClient.MySqlConnection
        Dim TimedOutMySQL As Integer = 180
        Dim dbconn As String = strCon

        If File.Exists(LokasiHitungUlang) Then File.Delete(LokasiHitungUlang)
        LogHItungUlang("-- Start")

        '*** Open Connection ***'  
        myConn1 = New MySql.Data.MySqlClient.MySqlConnection(dbconn)
        myConn1.Open()

        Try
            LogHItungUlang("-- Create table temporary")
            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = myConn1
                .CommandType = CommandType.Text
                .CommandText &= "DROP TABLE IF EXISTS m0_nomor_tmp;CREATE TABLE m0_nomor_tmp SELECT kodetabel FROM m0_nomor WHERE transaksihpp = 1;"
                .CommandText &= "DROP TABLE IF EXISTS m1_item_tmp;CREATE TABLE m1_item_tmp SELECT bid FROM m1_item WHERE bjenis <> 'J' AND bjenis <> 'V' AND bhpp = 'F';"
                .CommandText &= "DROP TABLE IF EXISTS m2_ap_tmp;CREATE TABLE m2_ap_tmp SELECT apkode FROM m2_accounting_period WHERE aptutupperiode = 0;"
                .CommandText &= "ALTER TABLE m2_ap_tmp ADD PRIMARY KEY (apkode);"
                .CommandText &= "ALTER TABLE m1_item_tmp ADD PRIMARY KEY (bid);"
                .CommandText &= "ALTER TABLE m0_nomor_tmp ADD PRIMARY KEY (kodetabel);"
                .CommandTimeout = TimedOutMySQL
                .ExecuteNonQuery()
            End With

            LogHItungUlang("-- Create table m1_item_transaction_before_" & namabackup & "")
            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = myConn1
                .CommandType = CommandType.Text
                .CommandText &= "CREATE TABLE m1_item_transaction_before_" & namabackup & " SELECT id, idbarang, jmlbarang, hpp, jenismutasi, saldojml, saldohpp, saldonilai FROM m1_item_transaction JOIN m1_item_tmp i ON idbarang = i.bid JOIN m0_nomor_tmp n ON sumber = n.kodetabel"
                .CommandTimeout = TimedOutMySQL
                .ExecuteNonQuery()
            End With

            ' Get data PD
            LogHItungUlang("-- Get Resources (SI, SR AND PD)")
            SetObj("dt_m5_sr_detail", AsDataTableAmbilDariDBCon("SELECT idsrdetail, idsr, idbarang, idsidetail, hpp FROM m5_sr_detail", myConn1))
            SetObj("dt_m5_si_detail", AsDataTableAmbilDariDBCon("SELECT idsidetail, idsi, idbarang, hpp FROM m5_si_detail", myConn1))
            SetObj("dt_m6_pd_in", AsDataTableAmbilDariDBCon("SELECT idpdin, idpd, idbarang, hpppersen, jmlbarang FROM m6_pd_in", myConn1))
            SetObj("dt_m6_pd_out", AsDataTableAmbilDariDBCon("SELECT idpdout, idpd, idbarang, jmlbarang, hpp FROM m6_pd_out", myConn1))

            LogHItungUlang("-- Get item transaction data")
            'HITUNG ULANG TRANSAKSI BARANG -------------------------------------
            sql = "  SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.customint10, it.notransaksi, it.saldojml, it.saldohpp, it.saldonilai, it.postingtgl, it.kodepa "
            sql &= " FROM m1_item_transaction it"
            sql &= " JOIN m1_item_tmp i ON it.idbarang = i.bid"
            sql &= " JOIN m0_nomor_tmp n ON it.sumber = n.kodetabel"
            'sql &= " WHERE it.idbarang = 179"
            sql &= " ORDER BY it.id"

            'sql &= "  LIMIT 0, 700"

            Dim dtBarang As DataTable = AsDataTableAmbilDariDBCon(sql, myConn1)

            'PROSES HITUNG ULANG -----------------------------------------------
            LogHItungUlang("-- Total item transaction data : " & (dtBarang.Rows.Count - 1).ToString)
            If dtBarang.Rows.Count > 0 Then

                Dim dtSaldoFix As New DataTable
                AsDataTableTambahField(dtSaldoFix, "id", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtSaldoFix, "idbarang", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtSaldoFix, "idutama", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtSaldoFix, "iddetail", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtSaldoFix, "jenismutasi", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtSaldoFix, "customint10", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtSaldoFix, "jmlbarang", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(dtSaldoFix, "hpp", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(dtSaldoFix, "sumber", AsEnumTypeData.AsString)
                AsDataTableTambahField(dtSaldoFix, "kodepa", AsEnumTypeData.AsInt64)

                Dim dtM1_Cogs_Fifo_In As New DataTable
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfiid", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfiidbarang", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfisumber", AsEnumTypeData.AsString)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfiidtransaksi", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfisatuan", AsEnumTypeData.AsString)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfijmlmasuk", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfijmlkeluar", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfisisa", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfiharga", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfiisclose", AsEnumTypeData.AsString)
                AsDataTableTambahField(dtM1_Cogs_Fifo_In, "cfiinputtgl", AsEnumTypeData.AsString)

                Dim dtM1_Cogs_Fifo_Out As New DataTable
                AsDataTableTambahField(dtM1_Cogs_Fifo_Out, "data", AsEnumTypeData.AsString)

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

                Dim drBarang As DataRow
                Dim drfilter() As DataRow, dr As DataRow, dr2 As DataRow

                Dim saldobutuh As Double = 0
                Dim subtotal As Double = 0
                Dim saldodipakai As Double = 0
                Dim sisasaldo As Double = 0
                Dim nilaihpppd As Double = 0
                Dim sdurut As Integer = 0


                'PERULANGAN HITUNG ULANG PER ROW TRANSAKSI BARANG
                'logHitungUlang("-- 5 For i As Double = 0 To dtBarang.Rows.Count - 1")
                For i As Double = 0 To dtBarang.Rows.Count - 1

                    drBarang = dtBarang.Rows(i)

                    'STEPKE
                    stepKe = stepKe + 1

                    'STEP DETAIL
                    stepDetail = 1

                    'RESET NILAI VARIABEL SALDO HASIL HITUNG
                    saldojml = 0 : saldohpp = 0 : saldonilai = 0
                    If i Mod 10000 = 0 Then
                        If dtBarang.Rows.Count > (i + 50000) Then
                            sd = i + 50000
                        Else
                            sd = dtBarang.Rows.Count - 1
                        End If
                        LogHItungUlang("-- Calculate item transaction from " & i.ToString & " to " & sd.ToString())
                    End If
                    'SET DATA BARANG
                    id = Integer.Parse(FxDB(drBarang("id"), 0))
                    idbarang = Integer.Parse(FxDB(drBarang("idbarang"), 0))
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
                    stepDetail = 4

                    'logHitungUlang("-- 10 AMBIL HPP BARANG UNTUK KONDISI KHUSUS")
                    'AMBIL HPP BARANG UNTUK KONDISI KHUSUS 
                    'PRODUKSI MASUK, SI ASSEMBLY LANGSUNG MASUK, SR MASUK AMBIL SI
                    If jenismutasi = 1 And sumber = "PD" Then

                        'JIKA TRANSAKSI PRODUKSI, MAKA HITUNG HPP MASUK BERDASARKAN PROSENTASE HPP BARANG PENYUSUN
                        drfilter = GetObj("dt_m6_pd_in").select("idpdin = " & iddetail)
                        If drfilter.Length > 0 Then
                            dr = drfilter(0)

                            drfilter = GetObj("dt_m6_pd_out").select("idpd = " & dr("idpd"))
                            If drfilter.Length > 0 Then
                                nilaihpppd = 0
                                For d = 0 To drfilter.Length - 1
                                    nilaihpppd += drfilter(d)("jmlbarang") * drfilter(d)("hpp")
                                Next
                                hppmasuk = FixDouble(((dr("hpppersen") / 100) * nilaihpppd) / dr("jmlbarang"))
                            End If
                        End If

                    ElseIf jenismutasi = 1 And sumber = "SI" Then



                    ElseIf jenismutasi = 1 And sumber = "SR" Then

                        'JIKA SR AMBIL SI, MAKA HPP MASUK BERDASARKAN HPP KELUAR PADA SI
                        drfilter = GetObj("dt_m5_sr_detail").select("idsrdetail = " & iddetail)
                        If drfilter.Length > 0 Then
                            drfilter = GetObj("dt_m5_si_detail").select("idsidetail = " & drfilter(0)("idsidetail"))
                            If drfilter.Length > 0 Then hppmasuk = drfilter(0)("hpp")
                        End If

                    End If

                    'STEP DETAIL
                    stepDetail = 5

                    'logHitungUlang(" -- 13")
                    'PROSES HITUNG HPP, SALDOJML, SALDOHPP DAN SALDONILAI
                    If jenismutasi = 1 Then
                        'JIKA BARANG MASUK
                        dr = GetDTObj("dt_" & idbarang).NewRow
                        dr("cfiid") = dtM1_Cogs_Fifo_In.Rows.Count
                        dr("cfiidbarang") = idbarang
                        dr("cfisumber") = sumber
                        dr("cfiidtransaksi") = iddetail
                        dr("cfisatuan") = FixQuotes("satuanbarang")
                        dr("cfijmlmasuk") = FixDouble(jmlbarang)
                        dr("cfijmlkeluar") = 0
                        dr("cfisisa") = FixDouble(jmlbarang)
                        dr("cfiharga") = hppmasuk
                        dr("cfiisclose") = 0
                        dr("cfiinputtgl") = FixQuotes(postingtgl)
                        SetDTObj("dt_" & idbarang, dr)
                    Else
                        'JIKA BARANG KELUAR

                        'AMBIL DATA HPP FIFO MASUK
                        'MAPPING FIELDNYA : saldobutuh, saldotersedia, saldodipakai, harga, subtotal, sisasaldo, sisabutuh, cfiid, cfisatuan 
                        drfilter = GetDTObj("dt_" & idbarang.ToString).Select("cfisisa > 0")
                        If drfilter.Length > 0 Then

                            saldobutuh = FixDouble(jmlbarang)
                            subtotal = 0
                            saldodipakai = 0
                            sisasaldo = 0

                            For x = 0 To drfilter.Length - 1
                                dr = drfilter(x)

                                If dr("cfisisa") <= saldobutuh Then
                                    saldodipakai += dr("cfisisa")
                                    subtotal += dr("cfisisa") * dr("cfiharga")
                                Else
                                    saldodipakai += saldobutuh
                                    subtotal += saldobutuh * dr("cfiharga")
                                End If

                                dr("cfisisa") -= saldodipakai
                                dr("cfijmlkeluar") += saldodipakai
                                saldobutuh -= dr("cfisisa")

                                dr2 = dtM1_Cogs_Fifo_Out.NewRow
                                dr2(0) = "(" & 0 & ", " & idbarang & ", '" & sumber & "', " & iddetail & ", '', '" & FixDouble(saldodipakai) & "', '" & FixDouble(dr("cfiharga")) & "', '0', '" & dr("cfiid") & "', '" & FixQuotes(postingtgl) & "'), "
                                dtM1_Cogs_Fifo_Out.Rows.Add(dr2)

                                If saldobutuh <= 0 Then
                                    Exit For
                                End If
                            Next

                        End If
                    End If

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
                            Case "SI"
                                'SI ADA BARANG ASSEMBLY LANGSUNG
                                If jenismutasi = 0 And customint10 = -2 Then
                                    'SI BARANG PENYUSUN KELUAR  (customint10 = -2), UPDATE KE TABEL M5_SI_MATERIAL
                                    ' drfilter = GetDTObj("dt_m5_si_material").select("idsimaterial = " & FixDouble(iddetail))
                                    'If drfilter.Length > 0 Then drfilter.CopyToDataTable()(0)("hpp") = FixDouble(HppTrans)

                                Else
                                    drfilter = GetDTObj("dt_m5_si_detail").select("idsidetail = " & FixDouble(iddetail))
                                    If drfilter.Length > 0 Then drfilter.CopyToDataTable()(0)("hpp") = FixDouble(HppTrans)

                                End If

                            Case "PD"
                                'PRODUKSI DIBAGI 2, BAHAN (KELUAR) DAN HASIL (MASUK)
                                If jenismutasi = 1 Then
                                    'drfilter = GetDTObj("dt_m6_pd_in").select("idpdin = " & FixDouble(iddetail))
                                    'If drfilter.Length > 0 Then drfilter.CopyToDataTable()(0)("hpp") = FixDouble(HppTrans)

                                Else
                                    'JIKA KELUAR MAKA UPDATE TABEL M6_PD_OUT
                                    drfilter = GetDTObj("dt_m6_pd_out").select("idpdout = " & FixDouble(iddetail))
                                    If drfilter.Length > 0 Then drfilter.CopyToDataTable()(0)("hpp") = FixDouble(HppTrans)

                                End If

                            Case Else
                                sql = ""
                        End Select

                    End If


                    'STEP DETAIL
                    stepDetail = 8

                    'UPDATE TRANSAKSI BARANG
                    If tgl >= tglAwal Then
                        dr = dtSaldoFix.NewRow
                        dr("id") = id
                        dr("idbarang") = idbarang
                        dr("idutama") = idutama
                        dr("iddetail") = iddetail
                        dr("iddetail") = iddetail
                        dr("sumber") = sumber
                        dr("jenismutasi") = jenismutasi
                        dr("customint10") = customint10
                        dr("jmlbarang") = jmlbarang
                        dr("kodepa") = Integer.Parse(FxDB(drBarang("kodepa"), 0))
                        dr("hpp") = FixDouble(HppTrans)
                        dtSaldoFix.Rows.Add(dr)


                    End If

                    'result(1) = 1
                    result(2) = ""
                    result(3) = stepKe
                    result(4) = result(4)

                Next


                LogHItungUlang("-- Create table m1_item_transaction_after_" & namabackup)

                Dim datake As Integer = 0
                Dim limit As Integer = 50000
                Dim perulangan As Integer = 0
                Dim zidbarang As Integer = 0
                saldojml = 0 : saldonilai = 0

                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "CREATE TABLE m1_item_transaction_after_" & namabackup & " (id integer PRIMARY KEY, idbarang integer, idutama integer, iddetail integer, sumber varchar(10), jmlbarang double, hpp double, jenismutasi integer, customint10 integer, saldojml double, saldonilai double, saldohpp double, kodepa integer); "
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                dtSaldoFix.DefaultView.Sort = "idbarang, id"
                dtSaldoFix = dtSaldoFix.DefaultView.ToTable
ulangexecute:
                sql = "INSERT INTO m1_item_transaction_after_" & namabackup & " VALUES "

                If (datake + limit) < dtSaldoFix.Rows.Count Then
                    perulangan = (datake + limit)
                Else
                    perulangan = dtSaldoFix.Rows.Count
                End If

                LogHItungUlang("-- Insert into table m1_item_transaction_after_" & namabackup & " from " & datake.ToString & " to " & perulangan.ToString)

                For i = datake To perulangan - 1
                    dr = dtSaldoFix.Rows(i)

                    sql &= "(" & dr("id") & ", " & dr("idbarang") & ", " & dr("idutama") & ", " & dr("iddetail") & ", '" & dr("sumber") & "', " & dr("jmlbarang") & ", " & dr("hpp") & ", " & dr("jenismutasi") & ", " & dr("customint10") & ", 0, 0, 0, " & dr("kodepa") & "), "
                Next

                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = sql.Substring(0, sql.Length - 2) & ";"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                datake += limit

                If datake < dtSaldoFix.Rows.Count Then GoTo ulangexecute

                LogHItungUlang("-- Calculate the balance of amount, value and hpp")
                sql = "SELECT * FROM (SELECT id, kodepa, @zsaldo := (CASE WHEN @zidbarang = idbarang OR @zsaldo = 0 THEN ROUND((CASE it.jenismutasi WHEN 1 THEN @zsaldo + it.jmlbarang ELSE @zsaldo - it.jmlbarang END), 5) ELSE ROUND((CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END),5) END) as saldojml, @znilai := (CASE WHEN @zsaldo = 0 THEN 0 ELSE (CASE WHEN @zidbarang = idbarang OR @znilai = 0 THEN ROUND((CASE it.jenismutasi WHEN 1 THEN ROUND(@znilai + (it.jmlbarang * it.hpp), 5) ELSE @znilai - (it.jmlbarang * it.hpp) END),5) ELSE ROUND((CASE it.jenismutasi WHEN 1 THEN (it.jmlbarang * it.hpp) ELSE -(it.jmlbarang * it.hpp) END),5) END)END) as saldonilai, IFNULL(@znilai/@zsaldo, 0) as saldohpp, @zidbarang := idbarang FROM m1_item_transaction_after_" & namabackup & " it  JOIN m0_nomor_tmp n ON n.kodetabel = it.sumber JOIN m1_item_tmp i ON it.idbarang = i.bid , (SELECT @zsaldo := 0) as a1, (SELECT @zidbarang := 0) as a2, (SELECT @znilai := 0) as a3 ORDER BY it.idbarang, it.id) it JOIN m2_ap_tmp ap ON it.kodepa = ap.apkode"
                sql = "UPDATE (" & sql & ") h JOIN m1_item_transaction_after_" & namabackup & " it ON it.id = h.id SET it.saldojml = h.saldojml, it.saldonilai = h.saldonilai, it.saldohpp = h.saldohpp"

                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                sql = "DELETE m1_item_transaction_after_" & namabackup & " FROM m1_item_transaction_after_" & namabackup & " LEFT JOIN m2_ap_tmp ON kodepa = apkode WHERE apkode IS NULL"

                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = sql
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                datake = 0
                perulangan = 0
ulangexecute2:
                'Update Transaksi Barang
                If (datake + limit) < dtSaldoFix.Rows.Count Then
                    perulangan = (datake + limit)
                Else
                    perulangan = dtSaldoFix.Rows.Count
                End If

                LogHItungUlang("-- Update item transaction from " & datake.ToString & " to " & perulangan.ToString)

                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h JOIN m1_item_transaction it ON it.id = h.id SET it.hpp = h.hpp,  it.saldojml = h.saldojml,  it.saldonilai = h.saldonilai,  it.saldohpp = h.saldohpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                datake += limit
                If datake < dtSaldoFix.Rows.Count Then GoTo ulangexecute2

                datake = 0
                perulangan = 0
                limit = 200000
ulangexecute3:

                'Update Transaksi Barang
                If (datake + limit) < dtSaldoFix.Rows.Count Then
                    perulangan = (datake + limit)
                Else
                    perulangan = dtSaldoFix.Rows.Count
                End If

                LogHItungUlang("-- Update resources (SA, IB, PRT, SI, RNR, SR, PD) from " & datake.ToString & " to " & perulangan.ToString)

                ' SA
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'SA' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m3_sa_detail h ON h1.idutama = h.idsa AND h1.iddetail = h.idsadetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' IB
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'IB' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m3_ib_detail h ON h1.idutama = h.idib AND h1.iddetail = h.idibdetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' PRT
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'PRT' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m4_prt_detail h ON h1.idutama = h.idprt AND h1.iddetail = h.idprtdetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' SI
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE customint10 <> -2 AND sumber = 'SI' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m5_si_detail h ON h1.idutama = h.idsi AND h1.iddetail = h.idsidetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' SI MATERIAL
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE jenismutasi = 0 AND customint10 = -2 AND sumber = 'SI' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m5_si_detail h ON h1.idutama = h.idsi AND h1.iddetail = h.idsidetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' RNR
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'RNR' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m5_rnr_detail h ON h1.idutama = h.idrnr AND h1.iddetail = h.idrnrdetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' SR
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'SR' LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m5_sr_detail h ON h1.idutama = h.idsr AND h1.iddetail = h.idsrdetail AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' PD IN
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'PD' AND jenismutasi = 1 LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m6_pd_in h ON h1.idutama = h.idpd AND h1.iddetail = h.idpdin AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' PD IN
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'PD' AND jenismutasi = 0 LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m6_pd_out h ON h1.idutama = h.idpd AND h1.iddetail = h.idpdout AND h1.idbarang = h.idbarang SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' AK
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'PD' AND jenismutasi = 0 LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m_11_ak_detail h ON h1.idutama = h.idak AND h1.iddetail = h.idakdetail AND h1.idbarang = h.idlayanan SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                ' RO
                With New MySql.Data.MySqlClient.MySqlCommand()
                    .Connection = myConn1
                    .CommandType = CommandType.Text
                    .CommandText = "UPDATE(SELECT * FROM m1_item_transaction_after_" & namabackup & " WHERE sumber = 'PD' AND jenismutasi = 0 LIMIT " & datake.ToString & ", " & (perulangan - 1).ToString & ") h1 JOIN m_11_ro_detail h ON h1.idutama = h.idro AND h1.iddetail = h.idrodetail AND h1.idbarang = h.idlayanan SET h.hpp = h1.hpp;"
                    .CommandTimeout = TimedOutMySQL
                    .ExecuteNonQuery()
                End With

                datake += limit
                If datake < dtSaldoFix.Rows.Count Then GoTo ulangexecute3


                LogHItungUlang("-- Create query fifo in")

                sql = "SELECT idbarang FROM m1_item_transaction it JOIN m1_item_tmp i ON it.idbarang = i.bid JOIN m0_nomor_tmp n ON it.sumber = n.kodetabel WHERE jenismutasi = 1 GROUP BY idbarang"

                ' FIFO IN
                Dim dtBarangMasuk As DataTable = AsDataTableAmbilDariDBCon(sql, myConn1)

                Dim str As String = ""
                For i = 0 To dtBarangMasuk.Rows.Count - 1
                    drfilter = GetDTObj("dt_" & dtBarangMasuk.Rows(i)("idbarang")).Select("cfisisa > 0")
                    For x = 0 To drfilter.Length - 1
                        dr = drfilter(x)
                        str &= "(" & dr("cfiid") & ", " & dr("cfiidbarang") & ", '" & dr("cfisumber") & "', " & dr("cfiidtransaksi") & ", '', '', '', '" & dr("cfijmlmasuk") & "', '" & dr("cfijmlkeluar") & "', '" & dr("cfisisa") & "', '" & dr("cfiharga") & "', " & 0 & ", '" & dr("cfiinputtgl") & "'), "
                    Next
                Next

                If Len(str) > 0 Then
                    LogHItungUlang("-- Insert to table fifo in")
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn1
                        .CommandType = CommandType.Text
                        .CommandText = "TRUNCATE m1_cogs_fifo_in; INSERT INTO m1_cogs_fifo_in(cfiid, cfiidbarang, cfisumber, cfiidtransaksi, cfinamabarang, cfitipebarang, cfisatuan, cfijmlmasuk, cfijmlkeluar, cfisisa, cfiharga, cfiisclose, cfiinputtgl) VALUES " & str.Substring(0, str.Length - 2)
                        .CommandTimeout = TimedOutMySQL
                        .ExecuteNonQuery()
                    End With
                    str = ""
                End If

            End If


            LogHItungUlang("-- Update status is end")
            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = myConn1
                .CommandType = CommandType.Text
                .CommandText = "UPDATE m0_setting SET snilai = 0 WHERE smodule = 0 AND sgrup = 'hppulang' AND skode = 'status'"
                .CommandTimeout = TimedOutMySQL
                .ExecuteNonQuery()
            End With


        Catch ex As Exception
            LogHItungUlang("-- Update status is end")
            LogHItungUlang("-- Info : " & Err.Description)
            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = myConn1
                .CommandType = CommandType.Text
                .CommandText = "UPDATE m0_setting SET snilai = 3 WHERE smodule = 0 AND sgrup = 'hppulang' AND skode = 'status'"
                .CommandTimeout = TimedOutMySQL
                .ExecuteNonQuery()
            End With
        End Try
        result(1) = 1
        result(2) = ""
        result(3) = stepKe
        result(4) = result(4)
        'END OF PROSES HITUNG ULANG ----------------------------------------

selesai:
        LogHItungUlang("-- End")

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Function GetObj(nama As String) As Object
        For i = 0 To ObjId.Length - 1
            If ObjId(i) = nama Then
                Return Obj(i)
            End If
        Next

        SetObj(nama, New Object)
        Return New Object
    End Function

    Sub SetObj(nama As String, value As Object)
        For i = 0 To ObjId.Length - 1
            If IsNothing(ObjId(i)) Then
                ObjId(i) = nama : Obj(i) = value : Exit For
            ElseIf ObjId(i) = nama Then
                Obj(i) = value : Exit For
            End If
        Next
    End Sub

    Function GetDTObj(nama As String) As Object
        For i = 0 To ObjId.Length - 1
            If IsNothing(ObjId(i)) Then
                ObjId(i) = nama
                Obj(i) = New DataTable
                AsDataTableTambahField(Obj(i), "cfiid", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(Obj(i), "cfiidbarang", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(Obj(i), "cfisumber", AsEnumTypeData.AsString)
                AsDataTableTambahField(Obj(i), "cfiidtransaksi", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(Obj(i), "cfisatuan", AsEnumTypeData.AsString)
                AsDataTableTambahField(Obj(i), "cfijmlmasuk", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfijmlkeluar", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfisisa", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfiharga", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfiisclose", AsEnumTypeData.AsString)
                AsDataTableTambahField(Obj(i), "cfiinputtgl", AsEnumTypeData.AsString)
                Return Obj(i)
            ElseIf ObjId(i) = nama Then
                Return Obj(i)
            End If
        Next

        Return New Object
    End Function

    Sub SetDTObj(nama As String, value As Object)
        For i = 0 To ObjId.Length - 1

            If IsNothing(ObjId(i)) Then
                ObjId(i) = nama
                Obj(i) = New DataTable
                AsDataTableTambahField(Obj(i), "cfiid", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(Obj(i), "cfiidbarang", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(Obj(i), "cfisumber", AsEnumTypeData.AsString)
                AsDataTableTambahField(Obj(i), "cfiidtransaksi", AsEnumTypeData.AsInt64)
                AsDataTableTambahField(Obj(i), "cfisatuan", AsEnumTypeData.AsString)
                AsDataTableTambahField(Obj(i), "cfijmlmasuk", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfijmlkeluar", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfisisa", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfiharga", AsEnumTypeData.AsDouble)
                AsDataTableTambahField(Obj(i), "cfiisclose", AsEnumTypeData.AsString)
                AsDataTableTambahField(Obj(i), "cfiinputtgl", AsEnumTypeData.AsString)
                Obj(i).Rows.Add(value)
                Exit For
            ElseIf ObjId(i) = nama Then
                Obj(i).Rows.Add(value) : Exit For
            End If
        Next

    End Sub

End Class
