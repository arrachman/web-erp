Public Class ReportOptimasi
    Public Function M0_MutasiStok_Rekap(ByVal param As String) As String
        '//LAPORAN MUTASI STOK REKAP (CABANG. LOKASI, GUDANG, COSTCENTER, DIVISI, SUBDIVISI, PROYEK)

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String,
        'cabangAwal(4) As String, cabangAkhir(5) As String, lokasiAwal(6) As String, lokasiAkhir(7) As String,
        'gudangAwal(8) As String, gudangAkhir(9) As String, costcenterAwal(10) As String, costcenterAkhir(11) As String, 
        'divisiAwal(12) As String, divisiAkhir(13) As String, subdivisiAwal(14) As String, subdivisiAkhir(15) As String, 
        'proyekAwal(16) As String, proyekAkhir(17) As String, orderBy(18) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, barangAwal, barangAkhir,
        'cabangAwal, cabangAkhir, lokasiAwal, lokasiAkhir,
        'gudangAwal, gudangAkhir, costcenterAwal, costcenterAkhir, 
        'divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, 
        'proyekAwal, proyekAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim cabangAwal As String = "", cabangAkhir As String = "", lokasiAwal As String = "", lokasiAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", costcenterAwal As String = "", costcenterAkhir As String = ""
        Dim divisiAwal As String = "", divisiAkhir As String = "", subdivisiAwal As String = "", subdivisiAkhir As String = ""
        Dim proyekAwal As String = "", proyekAkhir As String = "", orderBy As String = ""

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 19) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'barangAwal(2) As String
        barangAwal = dataDetail(2)

        'barangAkhir(3) As String
        barangAkhir = dataDetail(3)

        'cabangAwal(4) As String
        cabangAwal = dataDetail(4)

        'cabangAkhir(5) As String
        cabangAkhir = dataDetail(5)

        'lokasiAwal(6) As String
        lokasiAwal = dataDetail(6)

        'lokasiAkhir(7) As String
        lokasiAkhir = dataDetail(7)

        'gudangAwal(8) As String
        gudangAwal = dataDetail(8)

        'gudangAkhir(9) As String
        gudangAkhir = dataDetail(9)

        'costcenterAwal(10) As String
        costcenterAwal = dataDetail(10)

        'costcenterAkhir(11) As String
        costcenterAkhir = dataDetail(11)

        'divisiAwal(12) As String
        divisiAwal = dataDetail(12)

        'divisiAkhir(13) As String
        divisiAkhir = dataDetail(13)

        'subdivisiAwal(14) As String
        subdivisiAwal = dataDetail(14)

        'subdivisiAkhir(15) As String
        subdivisiAkhir = dataDetail(15)

        'proyekAwal(16) As String
        proyekAwal = dataDetail(16)

        'proyekAkhir(17) As String
        proyekAkhir = dataDetail(17)

        'orderBy(18) As String
        If Len(dataDetail(18)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(18).ToString <> "bkode" And dataDetail(18).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(18)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()


        'HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Mutasi_Stok_Detail
        sql = "DELETE FROM M2r_Mutasi_Stok_Detail WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace Stock Report data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail -----------------------------------



        AsEksekusiSQL("UPDATE m0_msmq SET , pesan = 'Cek Setting' WHERE id = '" & FixQuotes(idMsmq) & "'", strCon)
        'CEK SUDAH INPUT STOCK OPNAME ATAU BELUM, BERDASARKAN SETTING
        'JIKA SETTING CETAK MUTASI STOK REKAP HARUS INPUT STOCK OPNAME MAKA CEK DATA DAHULU
        sql = "  SELECT s.snilai, IFNULL(c.ccabang,'') as ccabang, IFNULL(c.clokasi,'') as clokasi, IFNULL(c.cgudang,'') as cgudang "
        sql &= " FROM m0_setting s "
        sql &= " LEFT JOIN m2r_mutasi_stok_custom c "
        sql &= " ON c.ctgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.ccabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            sql &= " AND c.ccabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            sql &= " AND c.ccabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If
        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.clokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            sql &= " AND c.clokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            sql &= " AND c.clokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If
        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.cgudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            sql &= " AND c.cgudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            sql &= " AND c.cgudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If
        sql &= " WHERE s.smodule = 3 AND s.sgrup = 'stockopname' AND s.skode = 'CetakMutasiStokRekap' "
        sql &= " GROUP BY s.smodule, s.sgrup, s.skode, c.ccabang, c.clokasi, c.cgudang, c.ctgl "

        'result(2) = "sql " & sql : GoTo selesai

        Dim isCustom As String = "", ftCabang As String = "", ftLokasi As String = "", ftGudang As String = ""
        Dim dtCustom As DataTable = AsDataTableAmbilDariDB(sql, strCon) 'snilai, ccabang, clokasi, cgudang
        If dtCustom.Rows.Count > 0 Then
            For Each drCustom In dtCustom.Rows
                'SET SETTING
                isCustom = drCustom("snilai")

                'FILTER CABANG
                ftCabang &= IIf(Len(ftCabang) > 0, ", ", "")
                ftCabang &= "'" & FixQuotes(drCustom("ccabang")) & "'"

                'FILTER LOKASI
                ftLokasi &= IIf(Len(ftLokasi) > 0, ", ", "")
                ftLokasi &= "'" & FixQuotes(drCustom("clokasi")) & "'"

                'FILTER GUDANG
                ftGudang &= IIf(Len(ftGudang) > 0, ", ", "")
                ftGudang &= "'" & FixQuotes(drCustom("cgudang")) & "'"

            Next

        Else
            result(2) = "Setting for Stock Summary permission not found." : GoTo selesai

        End If

        AsEksekusiSQL("UPDATE m0_msmq SET , pesan = 'Proses parameter' WHERE id = '" & FixQuotes(idMsmq) & "'", strCon)
        ' Optimasi mulai disini"

        'AMBIL BARANG SESUAI FILTER --------------------------------------------------
        'sql = "  SELECT i.bid, i.bkode, i.bnama, ifnull(it.cabang,'') as  cabang, ifnull(it.lokasi,'') as lokasi, w.wkode as gudang, ifnull(it.costcenter,'') as costcenter, ifnull(it.divisi,'') as divisi, ifnull(it.subdivisi,'') as subdivisi, ifnull(it.proyek,'') as proyek"
        'sql = "  SELECT inputtgl, tgl, idbarang, gudang, ifnull(it.costcenter,'') as costcenter, ifnull(it.divisi,'') as divisi, ifnull(it.subdivisi,'') as subdivisi, ifnull(it.proyek,'') as proyek, it.id as msid, it.cabang as mscabang, it.lokasi as mslokasi, it.gudang as msgudang, IFNULL(it.costcenter,'') as mscostcenter, IFNULL(it.divisi,'') as msdivisi, IFNULL(it.subdivisi,'') as mssubdivisi, IFNULL(it.proyek,'') as msproyek, it.idbarang as msidbarang, it.satuanbarang as mssatuanbarang, it.tgl as mstgl, it.sumber as mssumber, it.notransaksi as msnotransaksi, it.kontak as mskontak, it.uraian as msuraian, it.catatan as mscatatan, it.catatandetail as mscatatandetail, it.jenismutasi, it.jmlbarang, 0 as msjmlmasuk, 0 as msjmlkeluar, it.inputtgl as msinputtgl, '' as mscustomtext1, it.customtext2 as mscustomtext2, it.customtext3 as mscustomtext3, it.customtext4 as mscustomtext4, it.customtext5 as mscustomtext5, it.customint1 as mscustomint1, it.customint2 as mscustomint2, it.customint3 as mscustomint3, it.customint4 as mscustomint4, it.customint5 as mscustomint5, '0' as mscustomdbl1, 0 as mscustomdbl2, 0 as mscustomdbl3, 0 as mscustomdbl4, it.customdbl5 as mscustomdbl5, it.customdate1 as mscustomdate1, it.customdate2 as mscustomdate2, it.customdate3 as mscustomdate3, it.customdate4 as mscustomdate4, it.customdate5 as mscustomdate5"

        'SELECT idbarang, gudang FROM `m1_item_transaction` GROUP BY idbarang, gudang
        'sql &= " FROM (SELECT idbarang, gudang FROM m1_item_transaction GROUP BY idbarang, gudang) ititem "
        'sql &= " JOIN m1_item i ON ititem.idbarang = i.bid "
        'sql &= " JOIN m1_warehouse w ON ititem.gudang = w.wkode "
        'sql &= " LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND w.wkode = it.gudang "

        'JIKA SETTING CETAK MUTASI STOK REKAP HARUS INPUT STOCK OPNAME MAKA TAMBAHKAN FILTER SESUAI DATA YANG SUDAH DIAMBIL
        If isCustom = 1 Then
            'FILTER CABANG CUSTOM
            If Len(ftCabang) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " cabang IN(" & ftCabang & ")"
            End If

            'FILTER LOKASI CUSTOM
            If Len(ftLokasi) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " lokasi IN(" & ftLokasi & ")"
            End If

            'FILTER GUDANG CUSTOM
            If Len(ftGudang) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " gudang IN(" & ftGudang & ")"
            End If
        End If


        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (cabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "')"
        ElseIf Len(cabangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            Filter &= " cabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            Filter &= " cabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If

        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (lokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "')"
        ElseIf Len(lokasiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            Filter &= " lokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            Filter &= " lokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If

        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (gudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "')"
        ElseIf Len(gudangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            Filter &= " gudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            Filter &= " gudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If

        'FILTER COSTCENTER
        If Len(costcenterAwal) > 0 And Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL DAN COSTCENTER AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (costcenter BETWEEN '" & FixQuotes(costcenterAwal) & "' AND '" & FixQuotes(costcenterAkhir) & "')"
        ElseIf Len(costcenterAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL SAJA YANG DIISI MAKA FILTER >= COSTCENTER AWAL
            Filter &= " costcenter >= '" & FixQuotes(costcenterAwal) & "'"
        ElseIf Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AKHIR SAJA YANG DIISI MAKA FILTER <= COSTCENTER AKHIR
            Filter &= " costcenter <= '" & FixQuotes(costcenterAkhir) & "'"
        End If

        'FILTER DIVISI
        If Len(divisiAwal) > 0 And Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL DAN DIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (divisi BETWEEN '" & FixQuotes(divisiAwal) & "' AND '" & FixQuotes(divisiAkhir) & "')"
        ElseIf Len(divisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL SAJA YANG DIISI MAKA FILTER >= DIVISI AWAL
            'Filter &= " divisi >= '" & FixQuotes(divisiAwal) & "'"
            Filter &= " i.bdivisi = '" & FixQuotes(divisiAwal) & "'"
        ElseIf Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= DIVISI AKHIR
            Filter &= " divisi <= '" & FixQuotes(divisiAkhir) & "'"
        End If

        'FILTER SUBDIVISI
        If Len(subdivisiAwal) > 0 And Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL DAN SUBDIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (subdivisi BETWEEN '" & FixQuotes(subdivisiAwal) & "' AND '" & FixQuotes(subdivisiAkhir) & "')"
        ElseIf Len(subdivisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL SAJA YANG DIISI MAKA FILTER >= SUBDIVISI AWAL
            Filter &= " subdivisi >= '" & FixQuotes(subdivisiAwal) & "'"
        ElseIf Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= SUBDIVISI AKHIR
            Filter &= " subdivisi <= '" & FixQuotes(subdivisiAkhir) & "'"
        End If

        'FILTER PROYEK
        If Len(proyekAwal) > 0 And Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL DAN PROYEK AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " (proyek BETWEEN '" & FixQuotes(proyekAwal) & "' AND '" & FixQuotes(proyekAkhir) & "')"
        ElseIf Len(proyekAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL SAJA YANG DIISI MAKA FILTER >= PROYEK AWAL
            Filter &= " proyek >= '" & FixQuotes(proyekAwal) & "'"
        ElseIf Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AKHIR SAJA YANG DIISI MAKA FILTER <= PROYEK AKHIR
            Filter &= " proyek <= '" & FixQuotes(proyekAkhir) & "'"
        End If


        'Filter BARANG (KODE/NAMA)
        If Len(barangAwal) > 0 And Len(barangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA BARANG AWAL DAN BARANG AKHIR DIISI MAKA Filter BETWEEN
            Filter &= " (i." & FixQuotes(orderBy) & " BETWEEN '" & FixQuotes(barangAwal) & "' AND '" & FixQuotes(barangAkhir) & "')"
        ElseIf Len(barangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA BARANG AWAL SAJA YANG DIISI MAKA Filter >= BARANG AWAL
            Filter &= " i." & FixQuotes(orderBy) & " >= '" & FixQuotes(barangAwal) & "'"
        ElseIf Len(barangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA BARANG AKHIR SAJA YANG DIISI MAKA Filter <= BARANG AKHIR
            Filter &= " i." & FixQuotes(orderBy) & " <= '" & FixQuotes(barangAkhir) & "'"
        End If

        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin,'" & FixQuotes(idMsmq) & "' as idmsmq, 0 msnourut, gudang, wnama msgudangnama, bkode mskodebarang, bnama msnamabarang, bsatuan mssatuanbarang, "
        sql &= " SUM((CASE WHEN it.tgl < '" & tglAwal & "' THEN (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE it.jmlbarang * -1 END) ELSE 0 END)) as saldoawal, "
        sql &= " SUM((CASE WHEN it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' AND jenismutasi = 1 THEN  it.jmlbarang ELSE 0 END)) as masuk, "
        sql &= " SUM((CASE WHEN it.tgl BETWEEN '" & tglAwal & "' AND '" & tglAkhir & "' AND jenismutasi = 0 THEN  it.jmlbarang ELSE 0 END)) as keluar, "
        sql &= " SUM((CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE it.jmlbarang * -1 END)) as saldoakhir"

        sql &= " FROM m1_item_transaction it"
        sql &= " JOIN m1_item i ON it.idbarang = i.bid  AND (it.tgl <= '" & tglAkhir & "')" & IIf(Len(Filter) > 0, String.Concat(" AND", Filter), Filter)
        sql &= " JOIN m1_warehouse w ON it.gudang = w.wkode "


        'GROUP BY IDBARANG
        sql &= " GROUP BY i.bid, w.wkode"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"

        'ORDER BY
        'sql &= " ORDER BY i." & FixQuotes(orderBy) & ", w.wkode"
        'If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        'If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        'If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        'If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"

        sql = "INSERT INTO M2r_Mutasi_Stok_Rekap (" & sql & ")"

        'result(2) = sql : GoTo selesai
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed proccessing Starting Balance Stock Report '" : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = result(4)

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If
        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function
    Public Function M0_MutasiStok_RekapOld20180806(ByVal param As String) As String
        '//LAPORAN MUTASI STOK REKAP (CABANG. LOKASI, GUDANG, COSTCENTER, DIVISI, SUBDIVISI, PROYEK)

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String,
        'cabangAwal(4) As String, cabangAkhir(5) As String, lokasiAwal(6) As String, lokasiAkhir(7) As String,
        'gudangAwal(8) As String, gudangAkhir(9) As String, costcenterAwal(10) As String, costcenterAkhir(11) As String, 
        'divisiAwal(12) As String, divisiAkhir(13) As String, subdivisiAwal(14) As String, subdivisiAkhir(15) As String, 
        'proyekAwal(16) As String, proyekAkhir(17) As String, orderBy(18) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, barangAwal, barangAkhir,
        'cabangAwal, cabangAkhir, lokasiAwal, lokasiAkhir,
        'gudangAwal, gudangAkhir, costcenterAwal, costcenterAkhir, 
        'divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, 
        'proyekAwal, proyekAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim cabangAwal As String = "", cabangAkhir As String = "", lokasiAwal As String = "", lokasiAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", costcenterAwal As String = "", costcenterAkhir As String = ""
        Dim divisiAwal As String = "", divisiAkhir As String = "", subdivisiAwal As String = "", subdivisiAkhir As String = ""
        Dim proyekAwal As String = "", proyekAkhir As String = "", orderBy As String = ""

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA
        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 19) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'barangAwal(2) As String
        barangAwal = dataDetail(2)

        'barangAkhir(3) As String
        barangAkhir = dataDetail(3)

        'cabangAwal(4) As String
        cabangAwal = dataDetail(4)

        'cabangAkhir(5) As String
        cabangAkhir = dataDetail(5)

        'lokasiAwal(6) As String
        lokasiAwal = dataDetail(6)

        'lokasiAkhir(7) As String
        lokasiAkhir = dataDetail(7)

        'gudangAwal(8) As String
        gudangAwal = dataDetail(8)

        'gudangAkhir(9) As String
        gudangAkhir = dataDetail(9)

        'costcenterAwal(10) As String
        costcenterAwal = dataDetail(10)

        'costcenterAkhir(11) As String
        costcenterAkhir = dataDetail(11)

        'divisiAwal(12) As String
        divisiAwal = dataDetail(12)

        'divisiAkhir(13) As String
        divisiAkhir = dataDetail(13)

        'subdivisiAwal(14) As String
        subdivisiAwal = dataDetail(14)

        'subdivisiAkhir(15) As String
        subdivisiAkhir = dataDetail(15)

        'proyekAwal(16) As String
        proyekAwal = dataDetail(16)

        'proyekAkhir(17) As String
        proyekAkhir = dataDetail(17)

        'orderBy(18) As String
        If Len(dataDetail(18)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(18).ToString <> "bkode" And dataDetail(18).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(18)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()


        'HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Mutasi_Stok_Detail
        sql = "DELETE FROM M2r_Mutasi_Stok_Detail WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace Stock Report data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail -----------------------------------

        'CEK SUDAH INPUT STOCK OPNAME ATAU BELUM, BERDASARKAN SETTING
        'JIKA SETTING CETAK MUTASI STOK REKAP HARUS INPUT STOCK OPNAME MAKA CEK DATA DAHULU
        sql = "  SELECT s.snilai, IFNULL(c.ccabang,'') as ccabang, IFNULL(c.clokasi,'') as clokasi, IFNULL(c.cgudang,'') as cgudang "
        sql &= " FROM m0_setting s "
        sql &= " LEFT JOIN m2r_mutasi_stok_custom c "
        sql &= " ON c.ctgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.ccabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            sql &= " AND c.ccabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            sql &= " AND c.ccabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If
        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.clokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            sql &= " AND c.clokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            sql &= " AND c.clokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If
        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.cgudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            sql &= " AND c.cgudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            sql &= " AND c.cgudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If
        sql &= " WHERE s.smodule = 3 AND s.sgrup = 'stockopname' AND s.skode = 'CetakMutasiStokRekap' "
        sql &= " GROUP BY s.smodule, s.sgrup, s.skode, c.ccabang, c.clokasi, c.cgudang, c.ctgl "

        'result(2) = "sql " & sql : GoTo selesai

        Dim isCustom As String = "", ftCabang As String = "", ftLokasi As String = "", ftGudang As String = ""
        Dim dtCustom As DataTable = AsDataTableAmbilDariDB(sql, strCon) 'snilai, ccabang, clokasi, cgudang

        If dtCustom.Rows.Count > 0 Then
            For Each drCustom In dtCustom.Rows
                'SET SETTING
                isCustom = drCustom("snilai")

                'FILTER CABANG
                ftCabang &= IIf(Len(ftCabang) > 0, ", ", "")
                ftCabang &= "'" & FixQuotes(drCustom("ccabang")) & "'"

                'FILTER LOKASI
                ftLokasi &= IIf(Len(ftLokasi) > 0, ", ", "")
                ftLokasi &= "'" & FixQuotes(drCustom("clokasi")) & "'"

                'FILTER GUDANG
                ftGudang &= IIf(Len(ftGudang) > 0, ", ", "")
                ftGudang &= "'" & FixQuotes(drCustom("cgudang")) & "'"

            Next

        Else
            result(2) = "Setting for Stock Summary permission not found." : GoTo selesai

        End If

        ' Optimasi mulai disini
        Dim m1_item As DataTable, getFilter As String = ""

        'AMBIL BARANG SESUAI FILTER --------------------------------------------------
        'sql = "  SELECT i.bid, i.bkode, i.bnama, ifnull(it.cabang,'') as  cabang, ifnull(it.lokasi,'') as lokasi, w.wkode as gudang, ifnull(it.costcenter,'') as costcenter, ifnull(it.divisi,'') as divisi, ifnull(it.subdivisi,'') as subdivisi, ifnull(it.proyek,'') as proyek"
        'sql = "  SELECT inputtgl, tgl, idbarang, gudang, ifnull(it.costcenter,'') as costcenter, ifnull(it.divisi,'') as divisi, ifnull(it.subdivisi,'') as subdivisi, ifnull(it.proyek,'') as proyek, it.id as msid, it.cabang as mscabang, it.lokasi as mslokasi, it.gudang as msgudang, IFNULL(it.costcenter,'') as mscostcenter, IFNULL(it.divisi,'') as msdivisi, IFNULL(it.subdivisi,'') as mssubdivisi, IFNULL(it.proyek,'') as msproyek, it.idbarang as msidbarang, it.satuanbarang as mssatuanbarang, it.tgl as mstgl, it.sumber as mssumber, it.notransaksi as msnotransaksi, it.kontak as mskontak, it.uraian as msuraian, it.catatan as mscatatan, it.catatandetail as mscatatandetail, it.jenismutasi, it.jmlbarang, 0 as msjmlmasuk, 0 as msjmlkeluar, it.inputtgl as msinputtgl, '' as mscustomtext1, it.customtext2 as mscustomtext2, it.customtext3 as mscustomtext3, it.customtext4 as mscustomtext4, it.customtext5 as mscustomtext5, it.customint1 as mscustomint1, it.customint2 as mscustomint2, it.customint3 as mscustomint3, it.customint4 as mscustomint4, it.customint5 as mscustomint5, '0' as mscustomdbl1, 0 as mscustomdbl2, 0 as mscustomdbl3, 0 as mscustomdbl4, it.customdbl5 as mscustomdbl5, it.customdate1 as mscustomdate1, it.customdate2 as mscustomdate2, it.customdate3 as mscustomdate3, it.customdate4 as mscustomdate4, it.customdate5 as mscustomdate5"
        sql = "  SELECT *"
        'SELECT idbarang, gudang FROM `m1_item_transaction` GROUP BY idbarang, gudang
        'sql &= " FROM (SELECT idbarang, gudang FROM m1_item_transaction GROUP BY idbarang, gudang) ititem "
        'sql &= " JOIN m1_item i ON ititem.idbarang = i.bid "
        'sql &= " JOIN m1_warehouse w ON ititem.gudang = w.wkode "
        'sql &= " LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND w.wkode = it.gudang "

        sql &= " FROM m1_item_transaction it "
        'sql &= " JOIN m1_item i ON it.idbarang = i.bid "
        'sql &= " JOIN m1_warehouse w ON it.gudang = w.wkode "

        'JIKA SETTING CETAK MUTASI STOK REKAP HARUS INPUT STOCK OPNAME MAKA TAMBAHKAN FILTER SESUAI DATA YANG SUDAH DIAMBIL
        If isCustom = 1 Then
            'FILTER CABANG CUSTOM
            If Len(ftCabang) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " it.cabang IN(" & ftCabang & ")"
            End If

            'FILTER LOKASI CUSTOM
            If Len(ftLokasi) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " it.lokasi IN(" & ftLokasi & ")"
            End If

            'FILTER GUDANG CUSTOM
            If Len(ftGudang) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " it.gudang IN(" & ftGudang & ")"
            End If
        End If


        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.cabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            Filter &= " it.cabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            Filter &= " it.cabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If

        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.lokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            Filter &= " it.lokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            Filter &= " it.lokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If

        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.gudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            Filter &= " it.gudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            Filter &= " it.gudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If

        'FILTER COSTCENTER
        If Len(costcenterAwal) > 0 And Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL DAN COSTCENTER AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.costcenter BETWEEN '" & FixQuotes(costcenterAwal) & "' AND '" & FixQuotes(costcenterAkhir) & "'"
        ElseIf Len(costcenterAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL SAJA YANG DIISI MAKA FILTER >= COSTCENTER AWAL
            Filter &= " it.costcenter >= '" & FixQuotes(costcenterAwal) & "'"
        ElseIf Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AKHIR SAJA YANG DIISI MAKA FILTER <= COSTCENTER AKHIR
            Filter &= " it.costcenter <= '" & FixQuotes(costcenterAkhir) & "'"
        End If

        'FILTER DIVISI
        If Len(divisiAwal) > 0 And Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL DAN DIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.divisi BETWEEN '" & FixQuotes(divisiAwal) & "' AND '" & FixQuotes(divisiAkhir) & "'"
        ElseIf Len(divisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL SAJA YANG DIISI MAKA FILTER >= DIVISI AWAL
            Filter &= " it.divisi >= '" & FixQuotes(divisiAwal) & "'"
        ElseIf Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= DIVISI AKHIR
            Filter &= " it.divisi <= '" & FixQuotes(divisiAkhir) & "'"
        End If

        'FILTER SUBDIVISI
        If Len(subdivisiAwal) > 0 And Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL DAN SUBDIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.subdivisi BETWEEN '" & FixQuotes(subdivisiAwal) & "' AND '" & FixQuotes(subdivisiAkhir) & "'"
        ElseIf Len(subdivisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL SAJA YANG DIISI MAKA FILTER >= SUBDIVISI AWAL
            Filter &= " it.subdivisi >= '" & FixQuotes(subdivisiAwal) & "'"
        ElseIf Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= SUBDIVISI AKHIR
            Filter &= " it.subdivisi <= '" & FixQuotes(subdivisiAkhir) & "'"
        End If

        'FILTER PROYEK
        If Len(proyekAwal) > 0 And Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL DAN PROYEK AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.proyek BETWEEN '" & FixQuotes(proyekAwal) & "' AND '" & FixQuotes(proyekAkhir) & "'"
        ElseIf Len(proyekAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL SAJA YANG DIISI MAKA FILTER >= PROYEK AWAL
            Filter &= " it.proyek >= '" & FixQuotes(proyekAwal) & "'"
        ElseIf Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AKHIR SAJA YANG DIISI MAKA FILTER <= PROYEK AKHIR
            Filter &= " it.proyek <= '" & FixQuotes(proyekAkhir) & "'"
        End If

        Dim filterKodeNamaBarang As String = ""

        'filterKodeNamaBarang BARANG (KODE/NAMA)
        If Len(barangAwal) > 0 And Len(barangAkhir) > 0 Then
            filterKodeNamaBarang = IIf(Len(filterKodeNamaBarang) > 0, String.Concat(filterKodeNamaBarang, " AND"), filterKodeNamaBarang)
            'JIKA BARANG AWAL DAN BARANG AKHIR DIISI MAKA filterKodeNamaBarang BETWEEN
            filterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " BETWEEN '" & FixQuotes(barangAwal) & "' AND '" & FixQuotes(barangAkhir) & "'"
        ElseIf Len(barangAwal) > 0 Then
            filterKodeNamaBarang = IIf(Len(filterKodeNamaBarang) > 0, String.Concat(filterKodeNamaBarang, " AND"), filterKodeNamaBarang)
            'JIKA BARANG AWAL SAJA YANG DIISI MAKA filterKodeNamaBarang >= BARANG AWAL
            filterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " >= '" & FixQuotes(barangAwal) & "'"
        ElseIf Len(barangAkhir) > 0 Then
            filterKodeNamaBarang = IIf(Len(filterKodeNamaBarang) > 0, String.Concat(filterKodeNamaBarang, " AND"), filterKodeNamaBarang)
            'JIKA BARANG AKHIR SAJA YANG DIISI MAKA filterKodeNamaBarang <= BARANG AKHIR
            filterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " <= '" & FixQuotes(barangAkhir) & "'"
        End If

        'result(2) = "filterKodeNamaBarang : " & filterKodeNamaBarang : GoTo selesai

        If Len(filterKodeNamaBarang) > 0 Then
            m1_item = AsDataTableAmbilDariDB("SELECT * FROM m1_item i WHERE " & filterKodeNamaBarang, strCon)
            'result(2) = "SELECT * FROM m1_item WHERE " & filterKodeNamaBarang : GoTo selesai
            getFilter = ""
            For i = 0 To m1_item.Rows.Count - 1
                getFilter &= m1_item.Rows(i)("bid").ToString & ","
            Next

            If m1_item.Rows.Count > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " it.idbarang IN (" & getFilter.Substring(0, getFilter.Length - 1) & ")"
            End If
        Else
            m1_item = AsDataTableAmbilDariDB("SELECT * FROM m1_item", strCon)
        End If

        'FILTER
        sql &= IIf(Len(Filter) > 0, String.Concat(" WHERE", Filter), Filter)

        'GROUP BY IDBARANG
        'sql &= " GROUP BY i.bid, w.wkode

        'If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        'If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        'If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        'If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"

        'ORDER BY
        'sql &= " ORDER BY i." & FixQuotes(orderBy) & ", w.wkode"
        'If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        'If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        'If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        'If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"



        'sql = " select * from m1_item "

        'AMBIL DATA BARANG

        Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql, strCon)
        'END OF AMBIL BARANG SESUAI FILTER ------------------------------------------

        Dim dtBaranggrp As New DataTable
        Dim drBarangFilter = dtBarang.AsEnumerable().GroupBy(Function(r) New With { _
            Key .f1 = r("idbarang"), _
            Key .f2 = r("gudang"), _
            Key .f3 = IIf(Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0, r("costcenter"), ""),
            Key .f4 = IIf(Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0, r("divisi"), ""),
            Key .f5 = IIf(Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0, r("subdivisi"), ""),
            Key .f6 = IIf(Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0, r("proyek"), "") _
        }).[Select](Function(g) g.OrderBy(Function(r) r("idbarang")).First())

        If drBarangFilter.Count > 0 Then
            dtBaranggrp = drBarangFilter.CopyToDataTable()
        End If

        'ORDER BY
        Dim strOrderBy As String = "idbarang, gudang"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then strOrderBy &= ", costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then strOrderBy &= ", divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then strOrderBy &= ", subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then strOrderBy &= ", proyek"
        dtBaranggrp.DefaultView.Sort = strOrderBy
        dtBaranggrp = dtBaranggrp.DefaultView.ToTable()

        'result(2) = "dtBarang " + dtBarang.Rows.Count.ToString + "sqlku " & sql : GoTo selesai

        Dim dtwarehouse As DataTable = AsDataTableAmbilDariDB("select * from m1_warehouse", strCon)
        'dim dtcost_center as datatable = asdatatableambildaridb("select * from m1_cost_center", strcon)
        'dim dtdivision as datatable = asdatatableambildaridb("select * from m1_division", strcon)
        'dim dtsubdivision as datatable = asdatatableambildaridb("select * from m1_subdivision", strcon)
        'dim dtproject as DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_project", strCon)

        xstep = 10 ' Ambil data branch, lokasi
        'SimpanLogToFile("- xStep : 3")
        'Dim dtbranch As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_branch", strCon)
        'Dim dtlocation As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_location", strCon)


        'PROSES SELECT DATA PERSEDIAAN ----------------------------------------------
        If dtBaranggrp.Rows.Count > 0 Then

            Dim idbarang As String = "", gudang As String = "", costcenter As String = ""
            Dim divisi As String = "", subdivisi As String = "", proyek As String = ""
            Dim dtSA As New DataTable, jmlkeluar As Double = 0, jmlmasuk As Double = 0
            Dim sqlSA As String = "", sqlSM As String = "", sqlSAGabung As String = "", sqlSMGabung As String = ""
            Dim sqlSAJadi As String = "", sqlSMJadi As String = ""

            Dim drX() As DataRow, dtX As New DataTable
            Dim saldoawal As Double = 0, masuk As Double = 0, keluar As Double = 0, saldoakhir As Double = 0
            strValue = New StringBuilder
            Dim drBarang As DataRow
            Dim nourut As Integer = 0
            Dim jmldata As Integer = 0
            'PERULANGAN PROSES MUTASI STOK
            For n = 0 To dtBaranggrp.Rows.Count - 1
                Dim dr1 = dtBaranggrp.Rows(n)
                dtX = New DataTable
                saldoawal = 0 : masuk = 0 : keluar = 0 : saldoakhir = 0
                'SET STEP KE
                stepKe += 1
                nourut += 1
                'SET IDBARANG, GUDANG
                idbarang = dr1("idbarang") : gudang = dr1("gudang")
                costcenter = dr1("costcenter") : divisi = dr1("divisi")
                subdivisi = dr1("subdivisi") : proyek = dr1("proyek")
                SimpanLogToFile(idbarang.ToString & " " & gudang)

                ' Hitung saldoawal
                drX = dtBarang.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl < '" & FixQuotes(tglAwal) & "'", "")
                ''SimpanLogToFile(dr6.Length.ToString & "@")

                If drX.Length > 0 Then
                    drBarang = m1_item.Select("bid = " + idbarang.ToString + "", "").CopyToDataTable()(0)
                    dtX = drX.CopyToDataTable()

                    dtX.DefaultView.Sort = "tgl, inputtgl, id"
                    dtX = dtX.DefaultView.ToTable()

                    For i = 0 To dtX.Rows.Count - 1
                        If dtX.Rows(i)("jenismutasi") = 1 Then
                            saldoawal += dtX.Rows(i)("jmlbarang")
                        Else
                            saldoawal -= dtX.Rows(i)("jmlbarang")
                        End If
                    Next
                End If
                'result(2) = dtX.Rows.Count.ToString : GoTo selesai

                ' Hitung masuk dan keluar
                drX = dtBarang.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl >= '" & FixQuotes(tglAwal) & "' AND tgl <= '" & FixQuotes(tglAkhir) & "'", "")
                'SimpanLogToFile(dr6.Length.ToString & "@")

                If drX.Length > 0 Then
                    drBarang = m1_item.Select("bid = " + idbarang.ToString + "", "").CopyToDataTable()(0)
                    dtX = drX.CopyToDataTable()

                    dtX.DefaultView.Sort = "tgl, inputtgl, id"
                    dtX = dtX.DefaultView.ToTable()

                    For i = 0 To dtX.Rows.Count - 1
                        If dtX.Rows(i)("jenismutasi") = 1 Then
                            masuk += dtX.Rows(i)("jmlbarang")
                        Else
                            keluar += dtX.Rows(i)("jmlbarang")
                        End If
                    Next
                End If

                saldoakhir = saldoawal + masuk - keluar

                Dim gudangnama As String = ""
                Dim costcenternama As String = ""
                Dim divisinama As String = ""
                Dim subdivisinama As String = ""
                Dim proyeknama As String = ""
                Dim cabangnama As String = ""
                Dim lokasinama As String = ""

                'If dtX.Rows.Count > 0 Then
                drAttNama = dtwarehouse.Select("wkode = '" & dr1("gudang") & "'") : If drAttNama.Length > 0 Then gudangnama = drAttNama(0)("wnama")
                'End If

                'drAttNama = dtcost_center.Select("cckode = '" & dtSA.Rows(0)("costcenter") & "'") : If drAttNama.Length > 0 Then costcenternama = drAttNama(0)("ccnama")
                'drAttNama = dtdivision.Select("dkode = '" & dtSA.Rows(0)("divisi") & "'") : If drAttNama.Length > 0 Then divisinama = drAttNama(0)("dnama")
                'drAttNama = dtsubdivision.Select("sdkode = '" & dtSA.Rows(0)("subdivisi") & "'") : If drAttNama.Length > 0 Then subdivisinama = drAttNama(0)("sdnama")
                'drAttNama = dtproject.Select("pkode = '" & dtSA.Rows(0)("proyek") & "'") : If drAttNama.Length > 0 Then proyeknama = drAttNama(0)("pnama")
                'drAttNama = dtbranch.Select("bkode = '" & dtSA.Rows(0)("cabang") & "'") : If drAttNama.Length > 0 Then cabangnama = drAttNama(0)("bnama")
                'drAttNama = dtlocation.Select("lkode = '" & dtSA.Rows(0)("lokasi") & "'") : If drAttNama.Length > 0 Then lokasinama = drAttNama(0)("lnama")

                If dtX.Rows.Count > 0 Then
                    jmldata += 1
                    Dim dr3 = dtX.Rows(0)
                    ''BUAT VALUE SQL INSERT 
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    ' idlogin, 1 as msnourut, ms.msid, ms.mscabang, ms.mscabangnama, ms.mslokasi, ms.mslokasinama, ms.msgudang, ms.msgudangnama, ms.mscostcenter, ms.mscostcenternama, ms.msdivisi, ms.msdivisinama, ms.mssubdivisi, ms.mssubdivisinama, ms.msproyek, ms.msproyeknama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, 0 as msjmlmasuk, 0 as msjmlkeluar, SUM(ms.msjmlmasuk - ms.msjmlkeluar) as mssaldo, ms.msinputtgl, '" & FixQuotes(idMsmq) & "' as idmsmq, '" & FixDouble(userid) & "' as msuserid, ms.mscustomtext1, ms.mscustomtext2, ms.mscustomtext3, ms.mscustomtext4, ms.mscustomtext5, ms.mscustomint1, ms.mscustomint2, ms.mscustomint3, ms.mscustomint4, ms.mscustomint5, ms.mscustomdbl1, ms.mscustomdbl2, ms.mscustomdbl3, ms.mscustomdbl4, ms.mscustomdbl5, ms.mscustomdate1, ms.mscustomdate2, ms.mscustomdate3, ms.mscustomdate4, ms.mscustomdate5
                    strValue.Append("('" & FixQuotes(idLogin) & "', '" & nourut.ToString & "', '0', '" & FixQuotes(dr3("cabang")) & "', '" & _
                                    FixQuotes(cabangnama) & "', '" & FixQuotes(dr3("lokasi")) & "', '" & FixQuotes(lokasinama) & "', '" & FixQuotes(dr3("gudang")) & "', '" & _
                                    FixQuotes(gudangnama) & "', '" & FixQuotes(dr3("costcenter")) & "', '" & FixQuotes(costcenternama) & "', '" & FixQuotes(dr3("divisi")) & "', '" & _
                                    FixQuotes(divisinama) & "', '" & FixQuotes(dr3("subdivisi")) & "', '" & FixQuotes(subdivisinama) & "', '" & FixQuotes(dr3("proyek")) & "', '" & _
                                    FixQuotes(proyeknama) & "', '" & _
                                    FixQuotes(drBarang("bid")) & "', '" & FixQuotes(drBarang("bkode")) & "', '" & FixQuotes(drBarang("bnama")) & "', '" & FixQuotes(drBarang("bnama")) & "', '" & _
                                    FixQuotes(dr3("satuanbarang")) & "', '" & FixQuotes(AsFormatTanggal(dr3("tgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(dr3("sumber")) & "', '" & FixQuotes(dr3("notransaksi")) & "', '" & _
                                    FixQuotes(dr3("kontak")) & "', '', '', '" & FixQuotes(dr3("uraian")) & "', '" & _
                                    FixQuotes(dr3("catatan")) & "', '" & FixQuotes(dr3("catatandetail")) & "', '" & FixQuotes(jmlmasuk.ToString()) & "', '" & FixQuotes(jmlkeluar.ToString()) & "', '" & _
                                    FixQuotes(saldoawal.ToString()) & "', '" & FixQuotes(AsFormatTanggal(dr3("inputtgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(idMsmq) & "', '" & _
                                    FixQuotes(userid.ToString()) & "', '" & FixQuotes(dr3("customtext1")) & "', '" & FixQuotes(dr3("customtext2")) & "', '" & FixQuotes(dr3("customtext3")) & "', '" & _
                                    FixQuotes(dr3("customtext4")) & "', '" & FixQuotes(dr3("customtext5")) & "', '" & FixQuotes(dr3("customint1")) & "', '" & FixQuotes(dr3("customint2")) & "', '" & _
                                    FixQuotes(dr3("customint3")) & "', '" & FixQuotes(dr3("customint4")) & "', '" & FixQuotes(dr3("customint5")) & "', '" & FixQuotes(saldoawal.ToString) & "', '" & _
                                    FixQuotes(masuk.ToString) & "', '" & FixQuotes(keluar.ToString) & "', '" & FixQuotes(saldoakhir.ToString) & "', '" & FixQuotes(dr3("customdbl5")) & "', '" & _
                                    FixQuotes(AsFormatTanggal(dr3("customdate1"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate2"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate3"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate4"), "yyyy-MM-dd")) & "', '" & _
                                    FixQuotes(AsFormatTanggal(dr3("customdate5"), "yyyy-MM-dd")) & "')")


                    'INSERT KE DATABASE =================================
                    'SALDO AWAL

                    ''SALDO MUTASI
                    'sqlSMJadi = " INSERT INTO M2r_Mutasi_Stok_Detail (" & sqlSMGabung & ")"
                    'If AsEksekusiSQL(sqlSMJadi, strCon) = False Then
                    '    result(2) = "Failed proccessing Data Stock Report '" & FixQuotes(gudang) & "' Warehouse : " & IIf(orderBy = "bkode", dr1("bkode"), dr1("bnama")) : GoTo selesai
                    'End If
                    'END OF INSERT KE DATABASE ==========================



                    'result(2) = sql : GoTo selesai

                Else
                    jmldata += 1
                    '    result(2) = "INSERT INTO M2r_Mutasi_Stok_Detail VALUES  " & strValue.ToString & ";" : GoTo selesai
                End If

                'UPDATE PROGRESS ====================================
                'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
                progressPersen = IIf(stepKe = dtBarang.Rows.Count, Prosentase, (Math.Round(Prosentase / dtBarang.Rows.Count, 2)) * stepKe)
                'progressPersen = IIf(stepKe = dtBarang.Rows.Count, Prosentase, (Math.Round(Prosentase / (dtBarang.Rows.Count), 2)) * (stepKe - jmldata))
                'progressPersen = IIf(stepKe = jmldata, Prosentase, (Math.Round(Prosentase / jmldata, 2)) * stepKe)

                'UPDATE PROGRESS REPORT M0_MSMQ
                sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
                If AsEksekusiSQL(sql, strCon) = False Then
                    result(2) = "Failed updating progress Data Stock Report '" & FixQuotes(gudang) & "' Warehouse : " & IIf(orderBy = "bkode", dr1("bkode"), dr1("bnama")) : GoTo selesai
                End If
                'END OF UPDATE PROGRESS =============================
            Next
            'result(2) = saldoakhir.ToString() : GoTo selesai

            'result(2) = "INSERT INTO M2r_Mutasi_Stok_Detail VALUES  " & strValue.ToString & ";" : GoTo selesai
            If AsEksekusiSQL("INSERT INTO M2r_Mutasi_Stok_Detail VALUES  " & strValue.ToString & ";", strCon) = False Then
                result(2) = "Failed proccessing Starting Balance Stock Report '" : GoTo selesai
            End If
        Else

dataKosong:

            'UPDATE PROGRESS ====================================
            'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
            progressPersen = 100

            'UPDATE PROGRESS REPORT M0_MSMQ
            sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
            If AsEksekusiSQL(sql, strCon) = False Then
                result(2) = "Failed updating progress Data Stock Report 'Nothing' Warehouse : " & IIf(orderBy = "bkode", "Nothing", "Nothing") : GoTo selesai
            End If
            'END OF UPDATE PROGRESS =============================

        End If
        'END OF PROSES SELECT DATA PERSEDIAAN ---------------------------------------

        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If
        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_MutasiStok_Detail(ByVal param As String) As String
        '//LAPORAN MUTASI STOK DETAIL (CABANG. LOKASI, GUDANG, COSTCENTER, DIVISI, SUBDIVISI, PROYEK)

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String,
        'cabangAwal(4) As String, cabangAkhir(5) As String, lokasiAwal(6) As String, lokasiAkhir(7) As String,
        'gudangAwal(8) As String, gudangAkhir(9) As String, costcenterAwal(10) As String, costcenterAkhir(11) As String, 
        'divisiAwal(12) As String, divisiAkhir(13) As String, subdivisiAwal(14) As String, subdivisiAkhir(15) As String, 
        'proyekAwal(16) As String, proyekAkhir(17) As String, orderBy(18) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, barangAwal, barangAkhir,
        'cabangAwal, cabangAkhir, lokasiAwal, lokasiAkhir,
        'gudangAwal, gudangAkhir, costcenterAwal, costcenterAkhir, 
        'divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, 
        'proyekAwal, proyekAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim cabangAwal As String = "", cabangAkhir As String = "", lokasiAwal As String = "", lokasiAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", costcenterAwal As String = "", costcenterAkhir As String = ""
        Dim divisiAwal As String = "", divisiAkhir As String = "", subdivisiAwal As String = "", subdivisiAkhir As String = ""
        Dim proyekAwal As String = "", proyekAkhir As String = "", orderBy As String = ""

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 19) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'barangAwal(2) As String
        barangAwal = dataDetail(2)

        'barangAkhir(3) As String
        barangAkhir = dataDetail(3)

        'cabangAwal(4) As String
        cabangAwal = dataDetail(4)

        'cabangAkhir(5) As String
        cabangAkhir = dataDetail(5)

        'lokasiAwal(6) As String
        lokasiAwal = dataDetail(6)

        'lokasiAkhir(7) As String
        lokasiAkhir = dataDetail(7)

        'gudangAwal(8) As String
        gudangAwal = dataDetail(8)

        'gudangAkhir(9) As String
        gudangAkhir = dataDetail(9)

        'costcenterAwal(10) As String
        costcenterAwal = dataDetail(10)

        'costcenterAkhir(11) As String
        costcenterAkhir = dataDetail(11)

        'divisiAwal(12) As String
        divisiAwal = dataDetail(12)

        'divisiAkhir(13) As String
        divisiAkhir = dataDetail(13)

        'subdivisiAwal(14) As String
        subdivisiAwal = dataDetail(14)

        'subdivisiAkhir(15) As String
        subdivisiAkhir = dataDetail(15)

        'proyekAwal(16) As String
        proyekAwal = dataDetail(16)

        'proyekAkhir(17) As String
        proyekAkhir = dataDetail(17)

        'orderBy(18) As String
        If Len(dataDetail(18)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(18).ToString <> "bkode" And dataDetail(18).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(18)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()

        'HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Mutasi_Stok_Detail
        sql = "DELETE FROM M2r_Mutasi_Stok_Detail WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace Stock Report data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail -----------------------------------


        'AMBIL BARANG SESUAI FILTER --------------------------------------------------
        sql = "  SELECT *"
        sql &= " FROM m1_item_transaction it"

        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.cabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            Filter &= " it.cabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            Filter &= " it.cabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If

        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.lokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            Filter &= " it.lokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            Filter &= " it.lokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If

        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.gudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            Filter &= " it.gudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            Filter &= " it.gudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If

        'FILTER COSTCENTER
        If Len(costcenterAwal) > 0 And Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL DAN COSTCENTER AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.costcenter BETWEEN '" & FixQuotes(costcenterAwal) & "' AND '" & FixQuotes(costcenterAkhir) & "'"
        ElseIf Len(costcenterAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL SAJA YANG DIISI MAKA FILTER >= COSTCENTER AWAL
            Filter &= " it.costcenter >= '" & FixQuotes(costcenterAwal) & "'"
        ElseIf Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AKHIR SAJA YANG DIISI MAKA FILTER <= COSTCENTER AKHIR
            Filter &= " it.costcenter <= '" & FixQuotes(costcenterAkhir) & "'"
        End If

        'FILTER DIVISI
        If Len(divisiAwal) > 0 And Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL DAN DIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.divisi BETWEEN '" & FixQuotes(divisiAwal) & "' AND '" & FixQuotes(divisiAkhir) & "'"
        ElseIf Len(divisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL SAJA YANG DIISI MAKA FILTER >= DIVISI AWAL
            Filter &= " it.divisi >= '" & FixQuotes(divisiAwal) & "'"
        ElseIf Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= DIVISI AKHIR
            Filter &= " it.divisi <= '" & FixQuotes(divisiAkhir) & "'"
        End If

        'FILTER SUBDIVISI
        If Len(subdivisiAwal) > 0 And Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL DAN SUBDIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.subdivisi BETWEEN '" & FixQuotes(subdivisiAwal) & "' AND '" & FixQuotes(subdivisiAkhir) & "'"
        ElseIf Len(subdivisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL SAJA YANG DIISI MAKA FILTER >= SUBDIVISI AWAL
            Filter &= " it.subdivisi >= '" & FixQuotes(subdivisiAwal) & "'"
        ElseIf Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= SUBDIVISI AKHIR
            Filter &= " it.subdivisi <= '" & FixQuotes(subdivisiAkhir) & "'"
        End If

        'FILTER PROYEK
        If Len(proyekAwal) > 0 And Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL DAN PROYEK AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.proyek BETWEEN '" & FixQuotes(proyekAwal) & "' AND '" & FixQuotes(proyekAkhir) & "'"
        ElseIf Len(proyekAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL SAJA YANG DIISI MAKA FILTER >= PROYEK AWAL
            Filter &= " it.proyek >= '" & FixQuotes(proyekAwal) & "'"
        ElseIf Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AKHIR SAJA YANG DIISI MAKA FILTER <= PROYEK AKHIR
            Filter &= " it.proyek <= '" & FixQuotes(proyekAkhir) & "'"
        End If

        Dim FilterKodeNamaBarang As String = ""
        'FILTER BARANG (KODE/NAMA)
        If Len(barangAwal) > 0 And Len(barangAkhir) > 0 Then
            FilterKodeNamaBarang = IIf(Len(FilterKodeNamaBarang) > 0, String.Concat(FilterKodeNamaBarang, " AND"), FilterKodeNamaBarang)
            'JIKA BARANG AWAL DAN BARANG AKHIR DIISI MAKA FilterKodeNamaBarang BETWEEN
            FilterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " BETWEEN '" & FixQuotes(barangAwal) & "' AND '" & FixQuotes(barangAkhir) & "'"
        ElseIf Len(barangAwal) > 0 Then
            FilterKodeNamaBarang = IIf(Len(FilterKodeNamaBarang) > 0, String.Concat(FilterKodeNamaBarang, " AND"), FilterKodeNamaBarang)
            'JIKA BARANG AWAL SAJA YANG DIISI MAKA FilterKodeNamaBarang >= BARANG AWAL
            FilterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " >= '" & FixQuotes(barangAwal) & "'"
        ElseIf Len(barangAkhir) > 0 Then
            FilterKodeNamaBarang = IIf(Len(FilterKodeNamaBarang) > 0, String.Concat(FilterKodeNamaBarang, " AND"), FilterKodeNamaBarang)
            'JIKA BARANG AKHIR SAJA YANG DIISI MAKA FilterKodeNamaBarang <= BARANG AKHIR
            FilterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " <= '" & FixQuotes(barangAkhir) & "'"
        End If

        xstep = 1 ' Mulai hitung ulang
        Dim dtBarangID As DataTable
        Dim getIDBarang As String = ""

        xstep = 2 ' Jika ada filter berdasarkan kode barang maka ambil data barang tujuannya untuk gantil filter berdasarkan kode diganti idbarang
        If Len(FilterKodeNamaBarang) > 0 Then

            xstep = 2.1 ' Ambil data barang
            dtBarangID = AsDataTableAmbilDariDB("SELECT i.bid, i.bkode, i.btipe, i.bnama, i.bsatuan FROM m1_item i WHERE" & FilterKodeNamaBarang, strCon)

            xstep = 2.2 ' Buat filter idbarang dari data barang 2.1
            getIDBarang = ""
            For i = 0 To dtBarangID.Rows.Count - 1
                getIDBarang &= dtBarangID.Rows(i)("bid").ToString & ","
            Next

            If dtBarangID.Rows.Count > 0 Then
                If Len(getIDBarang) > 0 Then
                    Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                    Filter &= " it.idbarang IN (" & getIDBarang.Substring(0, getIDBarang.Length - 1) & ")"
                End If
            End If
        End If

        xstep = 3 ' Buat filter dari paramaeter
        sql &= IIf(Len(Filter) > 0, String.Concat(" WHERE", Filter), Filter)

        xstep = 4 ' Buat sorting dari paramaeter khusus ambil transaksi barang
        Dim sqlOrderBY As String = " ORDER BY it.idbarang, it.gudang"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sqlOrderBY &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sqlOrderBY &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sqlOrderBY &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sqlOrderBY &= ", it.proyek"
        sqlOrderBY &= ", it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail"

        xstep = 4 ' Ambil data barang
        Dim dtBarangTransaksi As DataTable = AsDataTableAmbilDariDB(sql & sqlOrderBY, strCon)

        xstep = 5 ' Buat filter kontak, untuk ambil kontak berdasarkan kontak-kontak yang di filter
        getIDBarang = ""
        For i = 0 To dtBarangTransaksi.Rows.Count - 1
            getIDBarang &= dtBarangTransaksi.Rows(i)("kontak").ToString & ","
        Next
        SimpanLogToFile("Mulai")
        xstep = 5.1 ' ambil data kontak
        Dim dtContact As New DataTable
        If Len(getIDBarang) > 0 Then
            dtContact = AsDataTableAmbilDariDB("SELECT * FROM m1_contact WHERE kid IN (" & getIDBarang.Substring(0, getIDBarang.Length - 1) & ")", strCon)
        End If


        xstep = 6 ' Buat group by dari paramaeter
        sql &= " GROUP BY it.idbarang, it.gudang"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"

        xstep = 7 ' Buat order by dari paramaeter
        sql &= " ORDER BY it.idbarang, it.gudang"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"
        sql &= ", it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail"

        xstep = 8 ' Ambil data transaksi barang
        'SimpanLogToFile("- xStep : 1@" & sql)
        Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql, strCon)
        SimpanLogToFile(dtBarang.Rows.Count.ToString & " " & sql)
        xstep = 8.1 ' Jika ada filter berdasarkan kode barang maka ambil data barang tujuannya untuk gantil filter berdasarkan kode diganti idbarang
        If Len(FilterKodeNamaBarang) = 0 Then

            xstep = 8.2 ' Buat filter dari idbarang untuk ganti dari kodebarang
            getIDBarang = ""
            For i = 0 To dtBarang.Rows.Count - 1
                getIDBarang &= dtBarang.Rows(i)("idbarang").ToString & ","
            Next

            xstep = 8.3 ' Ambil data barang
            If dtBarang.Rows.Count > 0 Then
                If Len(getIDBarang) > 0 Then
                    dtBarangID = AsDataTableAmbilDariDB("SELECT i.bid, i.bkode, i.btipe, i.bnama, i.bsatuan FROM m1_item i WHERE bid IN (" & getIDBarang.Substring(0, getIDBarang.Length - 1) & ")", strCon)
                End If
            End If
        End If

        xstep = 9 ' Ambil data gudang, costcenter, divisi, subdivisi, project
        'SimpanLogToFile("- xStep : 2")
        Dim dtwarehouse As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_warehouse", strCon)
        Dim dtcost_center As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_cost_center", strCon)
        Dim dtdivision As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_division", strCon)
        Dim dtsubdivision As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_subdivision", strCon)
        Dim dtproject As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_project", strCon)

        xstep = 10 ' Ambil data branch, lokasi
        'SimpanLogToFile("- xStep : 3")
        Dim dtbranch As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_branch", strCon)
        Dim dtlocation As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_location", strCon)

        xstep = 11 ' jika ada barang per gudang yang di hitung ulang
        Dim dr6(), drAttNama() As DataRow
        'SimpanLogToFile("- xStep : 4 @" & dtBarang.Rows.Count.ToString)
        'PROSES SELECT DATA PERSEDIAAN ----------------------------------------------
        If dtBarang.Rows.Count > 0 Then

            xstep = 11.1 ' persiapan variabel penampung
            Dim idbarang As String = "", gudang As String = "", costcenter As String = ""
            Dim divisi As String = "", subdivisi As String = "", proyek As String = ""
            Dim dtSA As New DataTable, saldoawal As Double = 0
            Dim sqlSA As String = "", sqlSM As String = "", sqlSAGabung As String = "", sqlSMGabung As String = ""
            Dim sqlSAJadi As String = "", sqlSMJadi As String = ""
            strValue = New StringBuilder
            Dim gudangnama As String = ""
            Dim costcenternama As String = ""
            Dim divisinama As String = ""
            Dim subdivisinama As String = ""
            Dim proyeknama As String = ""
            Dim cabangnama As String = ""
            Dim lokasinama As String = ""
            Dim drBarang As DataRow

            xstep = 11.2 ' perulangan tiap barang per gudang yang akan di hitung
            'PERULANGAN PROSES MUTASI STOK
            For Each dr1 As DataRow In dtBarang.Rows

                xstep = 11.3 ' Set variabel dari perulangan ke variabel penampung
                'SimpanLogToFile("xstep 0")
                'SET IDBARANG, GUDANG
                idbarang = dr1("idbarang") : gudang = dr1("gudang")
                costcenter = dr1("costcenter") : divisi = dr1("divisi")
                subdivisi = dr1("subdivisi") : proyek = dr1("proyek")
                saldoawal = 0

                xstep = 11.4 ' Filter data barang berdasarkan barang yang di hitung
                dr6 = dtBarangID.Select("bid = '" & idbarang.ToString & "'", "")

                xstep = 11.5 ' Jika ada barang yang di hitung
                If dr6.Length > 0 Then

                    xstep = 11.6 ' Set datarow barang baris pertama
                    drBarang = dr6(0)

                    'SET STEP KE
                    stepKe += 1

                    xstep = 11.7 ' data transaksi barang di filter berdasarkan barang, gudang dan tgl yg di hitung sekarang
                    'AMBIL SALDO AWAL
                    dr6 = dtBarangTransaksi.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl < '" & FixQuotes(tglAwal) & "'", "")
                    saldoawal = 0
                    'SimpanLogToFile(dr6.Length.ToString & "@")
                    If dr6.Length > 0 Then
                        'SimpanLogToFile("xstep 1")
                        xstep = 11.8 ' perulangan
                        dtSA = dr6.CopyToDataTable

                        For Each dr2 As DataRow In dtSA.Rows
                            'SimpanLogToFile(dr2("jenismutasi").ToString + " @ " + dr2("jmlbarang").ToString)
                            If dr2("jenismutasi") = 1 Then
                                saldoawal = saldoawal + dr2("jmlbarang")
                            Else
                                saldoawal = saldoawal - dr2("jmlbarang")
                            End If
                        Next

                        'SimpanLogToFile("saldoawal : " & saldoawal.ToString)

                        'SALDO AWAL
                        dr = dtSA.Rows(0)
                        'SimpanLogToFile("xstep 2")


                    End If
                    xstep = 11.9 ' ambil nama attribut
                    dr6 = dtwarehouse.Select("wkode = '" & gudang & "'") : If dr6.Length > 0 Then gudangnama = dr6(0)("wnama")
                    dr6 = dtcost_center.Select("cckode = '" & costcenter & "'") : If dr6.Length > 0 Then costcenternama = dr6(0)("ccnama")
                    dr6 = dtdivision.Select("dkode = '" & divisi & "'") : If dr6.Length > 0 Then divisinama = dr6(0)("dnama")
                    dr6 = dtsubdivision.Select("sdkode = '" & subdivisi & "'") : If dr6.Length > 0 Then subdivisinama = dr6(0)("sdnama")
                    dr6 = dtproject.Select("pkode = '" & proyek & "'") : If dr6.Length > 0 Then proyeknama = dr6(0)("pnama")
                    dr6 = dtbranch.Select("bkode = '" & dr1("cabang") & "'") : If dr6.Length > 0 Then cabangnama = dr6(0)("bnama")
                    dr6 = dtlocation.Select("lkode = '" & dr1("lokasi") & "'") : If dr6.Length > 0 Then lokasinama = dr6(0)("lnama")

                    'BUAT VALUE SQL INSERT 
                    xstep = 11.11 ' set ke strvalue
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    ' idlogin, 1 as msnourut, ms.msid, ms.mscabang, ms.mscabangnama, ms.mslokasi, ms.mslokasinama, ms.msgudang, ms.msgudangnama, ms.mscostcenter, ms.mscostcenternama, ms.msdivisi, ms.msdivisinama, ms.mssubdivisi, ms.mssubdivisinama, ms.msproyek, ms.msproyeknama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, 0 as msjmlmasuk, 0 as msjmlkeluar, SUM(ms.msjmlmasuk - ms.msjmlkeluar) as mssaldo, ms.msinputtgl, '" & FixQuotes(idMsmq) & "' as idmsmq, '" & FixDouble(userid) & "' as msuserid, ms.mscustomtext1, ms.mscustomtext2, ms.mscustomtext3, ms.mscustomtext4, ms.mscustomtext5, ms.mscustomint1, ms.mscustomint2, ms.mscustomint3, ms.mscustomint4, ms.mscustomint5, ms.mscustomdbl1, ms.mscustomdbl2, ms.mscustomdbl3, ms.mscustomdbl4, ms.mscustomdbl5, ms.mscustomdate1, ms.mscustomdate2, ms.mscustomdate3, ms.mscustomdate4, ms.mscustomdate5
                    strValue.Append("('" & FixQuotes(idLogin) & "', '" & FixQuotes(1) & "', '10', '', '" & _
                                    FixQuotes(cabangnama) & "', '', '" & FixQuotes(lokasinama) & "', '" & FixQuotes(gudang) _
                                    & "', '" & FixQuotes(gudangnama) & "', '" & FixQuotes(costcenter) & "', '" & FixQuotes(costcenternama) & "', '" & _
                                    FixQuotes(divisi) & "', '" & FixQuotes(divisinama) & "', '" & FixQuotes(subdivisi) & "', '" & _
                                    FixQuotes(subdivisinama) & "', '" & FixQuotes(proyek) & "', '" & FixQuotes(proyeknama) & "', '" & _
                                    FixQuotes(drBarang("bid")) & "', '" & FixQuotes(drBarang("bkode")) & "', '" & FixQuotes(drBarang("btipe")) & "', '" & _
                                    FixQuotes(drBarang("bnama")) & "', '" & FixQuotes(drBarang("bsatuan")) & "', '" & FixQuotes(tglAwal) & "', '" & _
                                    FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes(0) & "', '" & _
                                    FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("Saldo Awal") & "', '" & _
                                    FixQuotes("Saldo Awal") & "', '" & FixQuotes("Saldo Awal") & "', '" & FixQuotes(0) & "', '" & _
                                    FixQuotes(0) & "', '" & FixQuotes(saldoawal.ToString()) & "', '" & FixQuotes(tglAwal) & " 00:00:00" & "', '" & _
                                    FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes("") & "', '" & _
                                    FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & _
                                    FixQuotes("") & "', '" & FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & _
                                    FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & _
                                    FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & _
                                    FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & FixQuotes("1900-01-01") & "', '" & _
                                    FixQuotes("1900-01-01") & "', '" & FixQuotes("1900-01-01") & "', '" & FixQuotes("1900-01-01") & "', '" & _
                                    FixQuotes("1900-01-01") & "')")
                    'SimpanLogToFile("xstep 4")


                    xstep = 11.12 ' Hitung saldo Mutasi
                    'SALDO MUTASI
                    Dim nourut As Integer = 1
                    Dim jmlmasuk, jmlkeluar As Double
                    dr6 = dtBarangTransaksi.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl >= '" & FixDouble(tglAwal) & "' AND tgl <= '" & FixDouble(tglAkhir) & "'", "")

                    'SimpanLogToFile("xstep 5")
                    xstep = 11.13 ' jika ada saldo mutasi
                    If dr6.Length > 0 Then
                        'SimpanLogToFile("go !")
                        dtSA = dr6.CopyToDataTable

                        xstep = 11.131 ' jika ada saldo mutasi
                        drAttNama = dtwarehouse.Select("wkode = '" & dtSA.Rows(0)("gudang") & "'") : If drAttNama.Length > 0 Then gudangnama = drAttNama(0)("wnama")
                        drAttNama = dtcost_center.Select("cckode = '" & dtSA.Rows(0)("costcenter") & "'") : If drAttNama.Length > 0 Then costcenternama = drAttNama(0)("ccnama")
                        drAttNama = dtdivision.Select("dkode = '" & dtSA.Rows(0)("divisi") & "'") : If drAttNama.Length > 0 Then divisinama = drAttNama(0)("dnama")
                        drAttNama = dtsubdivision.Select("sdkode = '" & dtSA.Rows(0)("subdivisi") & "'") : If drAttNama.Length > 0 Then subdivisinama = drAttNama(0)("sdnama")
                        drAttNama = dtproject.Select("pkode = '" & dtSA.Rows(0)("proyek") & "'") : If drAttNama.Length > 0 Then proyeknama = drAttNama(0)("pnama")
                        drAttNama = dtbranch.Select("bkode = '" & dtSA.Rows(0)("cabang") & "'") : If drAttNama.Length > 0 Then cabangnama = drAttNama(0)("bnama")
                        drAttNama = dtlocation.Select("lkode = '" & dtSA.Rows(0)("lokasi") & "'") : If drAttNama.Length > 0 Then lokasinama = drAttNama(0)("lnama")
                        For Each dr3 As DataRow In dtSA.Rows
                            xstep = 11.132 ' jika ada saldo mutasi
                            nourut += 1
                            dr6 = dtContact.Select("kid = '" & dr3("kontak") & "'")

                            'SimpanLogToFile(dr6.Length.ToString & " contact" & dr3("kontak"))
                            jmlmasuk = 0
                            jmlkeluar = 0

                            If dr3("jenismutasi") = 1 Then
                                jmlmasuk = dr3("jmlbarang")
                            Else
                                jmlkeluar = dr3("jmlbarang")
                            End If

                            saldoawal = saldoawal + jmlmasuk - jmlkeluar

                            'BUAT VALUE SQL INSERT 
                            xstep = 11.133 ' jika ada saldo mutasi
                            strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                            ' idlogin, 1 as msnourut, ms.msid, ms.mscabang, ms.mscabangnama, ms.mslokasi, ms.mslokasinama, ms.msgudang, ms.msgudangnama, ms.mscostcenter, ms.mscostcenternama, ms.msdivisi, ms.msdivisinama, ms.mssubdivisi, ms.mssubdivisinama, ms.msproyek, ms.msproyeknama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, 0 as msjmlmasuk, 0 as msjmlkeluar, SUM(ms.msjmlmasuk - ms.msjmlkeluar) as mssaldo, ms.msinputtgl, '" & FixQuotes(idMsmq) & "' as idmsmq, '" & FixDouble(userid) & "' as msuserid, ms.mscustomtext1, ms.mscustomtext2, ms.mscustomtext3, ms.mscustomtext4, ms.mscustomtext5, ms.mscustomint1, ms.mscustomint2, ms.mscustomint3, ms.mscustomint4, ms.mscustomint5, ms.mscustomdbl1, ms.mscustomdbl2, ms.mscustomdbl3, ms.mscustomdbl4, ms.mscustomdbl5, ms.mscustomdate1, ms.mscustomdate2, ms.mscustomdate3, ms.mscustomdate4, ms.mscustomdate5
                            strValue.Append("('" & FixQuotes(idLogin) & "', '" & nourut.ToString & "', '" & FixQuotes(dr3("id")) & "', '" & FixQuotes(dr3("cabang")) & "', '" & _
                                            FixQuotes(cabangnama) & "', '" & FixQuotes(dr3("lokasi")) & "', '" & FixQuotes(lokasinama) & "', '" & FixQuotes(dr3("gudang")) & "', '" & _
                                            FixQuotes(gudangnama) & "', '" & FixQuotes(dr3("costcenter")) & "', '" & FixQuotes(costcenternama) & "', '" & FixQuotes(dr3("divisi")) & "', '" & _
                                            FixQuotes(divisinama) & "', '" & FixQuotes(dr3("subdivisi")) & "', '" & FixQuotes(subdivisinama) & "', '" & FixQuotes(dr3("proyek")) & "', '" & _
                                            FixQuotes(proyeknama) & "', '" & _
                                            FixQuotes(drBarang("bid")) & "', '" & FixQuotes(drBarang("bkode")) & "', '" & FixQuotes(drBarang("bnama")) & "', '" & FixQuotes(drBarang("bnama")) & "', '" & _
                                            FixQuotes(dr3("satuanbarang")) & "', '" & FixQuotes(AsFormatTanggal(dr3("tgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(dr3("sumber")) & "', '" & FixQuotes(dr3("notransaksi")) & "', '" & _
                                            FixQuotes(dr3("kontak")) & "', '" & FixQuotes(dr6(0)("kkode")) & "', '" & FixQuotes(dr6(0)("knama")) & "', '" & FixQuotes(dr3("uraian")) & "', '" & _
                                            FixQuotes(dr3("catatan")) & "', '" & FixQuotes(dr3("catatandetail")) & "', '" & FixQuotes(jmlmasuk.ToString()) & "', '" & FixQuotes(jmlkeluar.ToString()) & "', '" & _
                                            FixQuotes(saldoawal.ToString()) & "', '" & FixQuotes(AsFormatTanggal(dr3("inputtgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(idMsmq) & "', '" & _
                                            FixQuotes(userid.ToString()) & "', '" & FixQuotes(dr3("customtext1")) & "', '" & FixQuotes(dr3("customtext2")) & "', '" & FixQuotes(dr3("customtext3")) & "', '" & _
                                            FixQuotes(dr3("customtext4")) & "', '" & FixQuotes(dr3("customtext5")) & "', '" & FixQuotes(dr3("customint1")) & "', '" & FixQuotes(dr3("customint2")) & "', '" & _
                                            FixQuotes(dr3("customint3")) & "', '" & FixQuotes(dr3("customint4")) & "', '" & FixQuotes(dr3("customint5")) & "', '" & FixQuotes(dr3("customdbl1")) & "', '" & _
                                            FixQuotes(dr3("customdbl2")) & "', '" & FixQuotes(dr3("customdbl3")) & "', '" & FixQuotes(dr3("customdbl4")) & "', '" & FixQuotes(dr3("customdbl5")) & "', '" & _
                                            FixQuotes(AsFormatTanggal(dr3("customdate1"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate2"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate3"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate4"), "yyyy-MM-dd")) & "', '" & _
                                            FixQuotes(AsFormatTanggal(dr3("customdate5"), "yyyy-MM-dd")) & "')")

                            xstep = 11.134 ' jika ada saldo mutasi
                        Next

                        'SimpanLogToFile("barang : " & idbarang.ToString & " gudang : " & gudang & " saldoawal : " & saldoawal.ToString)
                        'SimpanLogToFile("done !")
                    Else
                        'SimpanLogToFile("SKIP !")
                    End If
                Else
                    ' Jika tidak ada barang yang di hitung ulang
                    'SimpanLogToFile("idbarang : " & idbarang.ToString)
                End If
                'UPDATE PROGRESS ====================================
                'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
                progressPersen = IIf(stepKe = dtBarang.Rows.Count, Prosentase, (Math.Round(Prosentase / dtBarang.Rows.Count, 2)) * stepKe)

                Dim sqlInsert As String = ""
                xstep = 11.135 ' jika ada saldo mutasi
                If strValue.Length > 0 Then
                    xstep = 11.136 ' jika ada saldo mutasi
                    sqlInsert = "INSERT INTO M2r_Mutasi_Stok_Detail VALUES  " & strValue.ToString & ";"
                End If

                'UPDATE PROGRESS REPORT M0_MSMQ
                xstep = 11.137 ' jika ada saldo mutasi
                sql = sqlInsert & "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
                result(2) = sql
                SimpanLogToFile(sql)
                If AsEksekusiSQL(sql, strCon) = False Then
                    xstep = 11.138 ' jika ada saldo mutasi
                    result(2) = "Failed updating progress Data Stock Report '" & FixQuotes(gudang) & "' Warehouse : " & dr1("namabarang") : GoTo selesai
                End If

                xstep = 11.139 ' jika ada saldo mutasi
                If strValue.Length > 0 Then
                    strValue = New StringBuilder
                End If
                'END OF UPDATE PROGRESS =============================

            Next


        Else

            'UPDATE PROGRESS ====================================
            'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
            progressPersen = 100

            'UPDATE PROGRESS REPORT M0_MSMQ
            sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
            If AsEksekusiSQL(sql, strCon) = False Then
                result(2) = "Failed updating progress Data Stock Report 'Nothing' Warehouse : " & IIf(orderBy = "bkode", "Nothing", "Nothing") : GoTo selesai
            End If
            'END OF UPDATE PROGRESS =============================

        End If
        'SimpanLogToFile("- xStep : 5")
        'END OF PROSES SELECT DATA PERSEDIAAN ---------------------------------------

        xstep = 11.1301 ' jika ada saldo mutasi
        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_MutasiStok_Detail20241025(ByVal param As String) As String
        '//LAPORAN MUTASI STOK DETAIL (CABANG. LOKASI, GUDANG, COSTCENTER, DIVISI, SUBDIVISI, PROYEK)

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String,
        'cabangAwal(4) As String, cabangAkhir(5) As String, lokasiAwal(6) As String, lokasiAkhir(7) As String,
        'gudangAwal(8) As String, gudangAkhir(9) As String, costcenterAwal(10) As String, costcenterAkhir(11) As String, 
        'divisiAwal(12) As String, divisiAkhir(13) As String, subdivisiAwal(14) As String, subdivisiAkhir(15) As String, 
        'proyekAwal(16) As String, proyekAkhir(17) As String, orderBy(18) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, barangAwal, barangAkhir,
        'cabangAwal, cabangAkhir, lokasiAwal, lokasiAkhir,
        'gudangAwal, gudangAkhir, costcenterAwal, costcenterAkhir, 
        'divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, 
        'proyekAwal, proyekAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim cabangAwal As String = "", cabangAkhir As String = "", lokasiAwal As String = "", lokasiAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", costcenterAwal As String = "", costcenterAkhir As String = ""
        Dim divisiAwal As String = "", divisiAkhir As String = "", subdivisiAwal As String = "", subdivisiAkhir As String = ""
        Dim proyekAwal As String = "", proyekAkhir As String = "", orderBy As String = ""

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 19) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'barangAwal(2) As String
        barangAwal = dataDetail(2)

        'barangAkhir(3) As String
        barangAkhir = dataDetail(3)

        'cabangAwal(4) As String
        cabangAwal = dataDetail(4)

        'cabangAkhir(5) As String
        cabangAkhir = dataDetail(5)

        'lokasiAwal(6) As String
        lokasiAwal = dataDetail(6)

        'lokasiAkhir(7) As String
        lokasiAkhir = dataDetail(7)

        'gudangAwal(8) As String
        gudangAwal = dataDetail(8)

        'gudangAkhir(9) As String
        gudangAkhir = dataDetail(9)

        'costcenterAwal(10) As String
        costcenterAwal = dataDetail(10)

        'costcenterAkhir(11) As String
        costcenterAkhir = dataDetail(11)

        'divisiAwal(12) As String
        divisiAwal = dataDetail(12)

        'divisiAkhir(13) As String
        divisiAkhir = dataDetail(13)

        'subdivisiAwal(14) As String
        subdivisiAwal = dataDetail(14)

        'subdivisiAkhir(15) As String
        subdivisiAkhir = dataDetail(15)

        'proyekAwal(16) As String
        proyekAwal = dataDetail(16)

        'proyekAkhir(17) As String
        proyekAkhir = dataDetail(17)

        'orderBy(18) As String
        If Len(dataDetail(18)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(18).ToString <> "bkode" And dataDetail(18).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(18)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()

        'HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Mutasi_Stok_Detail
        sql = "DELETE FROM M2r_Mutasi_Stok_Detail WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace Stock Report data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail -----------------------------------


        'AMBIL BARANG SESUAI FILTER --------------------------------------------------
        sql = "  SELECT *"
        sql &= " FROM m1_item_transaction it"

        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.cabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            Filter &= " it.cabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            Filter &= " it.cabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If

        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.lokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            Filter &= " it.lokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            Filter &= " it.lokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If

        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.gudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            Filter &= " it.gudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            Filter &= " it.gudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If

        'FILTER COSTCENTER
        If Len(costcenterAwal) > 0 And Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL DAN COSTCENTER AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.costcenter BETWEEN '" & FixQuotes(costcenterAwal) & "' AND '" & FixQuotes(costcenterAkhir) & "'"
        ElseIf Len(costcenterAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL SAJA YANG DIISI MAKA FILTER >= COSTCENTER AWAL
            Filter &= " it.costcenter >= '" & FixQuotes(costcenterAwal) & "'"
        ElseIf Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AKHIR SAJA YANG DIISI MAKA FILTER <= COSTCENTER AKHIR
            Filter &= " it.costcenter <= '" & FixQuotes(costcenterAkhir) & "'"
        End If

        'FILTER DIVISI
        If Len(divisiAwal) > 0 And Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL DAN DIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.divisi BETWEEN '" & FixQuotes(divisiAwal) & "' AND '" & FixQuotes(divisiAkhir) & "'"
        ElseIf Len(divisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL SAJA YANG DIISI MAKA FILTER >= DIVISI AWAL
            Filter &= " it.divisi >= '" & FixQuotes(divisiAwal) & "'"
        ElseIf Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= DIVISI AKHIR
            Filter &= " it.divisi <= '" & FixQuotes(divisiAkhir) & "'"
        End If

        'FILTER SUBDIVISI
        If Len(subdivisiAwal) > 0 And Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL DAN SUBDIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.subdivisi BETWEEN '" & FixQuotes(subdivisiAwal) & "' AND '" & FixQuotes(subdivisiAkhir) & "'"
        ElseIf Len(subdivisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL SAJA YANG DIISI MAKA FILTER >= SUBDIVISI AWAL
            Filter &= " it.subdivisi >= '" & FixQuotes(subdivisiAwal) & "'"
        ElseIf Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= SUBDIVISI AKHIR
            Filter &= " it.subdivisi <= '" & FixQuotes(subdivisiAkhir) & "'"
        End If

        'FILTER PROYEK
        If Len(proyekAwal) > 0 And Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL DAN PROYEK AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " it.proyek BETWEEN '" & FixQuotes(proyekAwal) & "' AND '" & FixQuotes(proyekAkhir) & "'"
        ElseIf Len(proyekAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL SAJA YANG DIISI MAKA FILTER >= PROYEK AWAL
            Filter &= " it.proyek >= '" & FixQuotes(proyekAwal) & "'"
        ElseIf Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AKHIR SAJA YANG DIISI MAKA FILTER <= PROYEK AKHIR
            Filter &= " it.proyek <= '" & FixQuotes(proyekAkhir) & "'"
        End If

        Dim FilterKodeNamaBarang As String = ""
        'FILTER BARANG (KODE/NAMA)
        If Len(barangAwal) > 0 And Len(barangAkhir) > 0 Then
            FilterKodeNamaBarang = IIf(Len(FilterKodeNamaBarang) > 0, String.Concat(FilterKodeNamaBarang, " AND"), FilterKodeNamaBarang)
            'JIKA BARANG AWAL DAN BARANG AKHIR DIISI MAKA FilterKodeNamaBarang BETWEEN
            FilterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " BETWEEN '" & FixQuotes(barangAwal) & "' AND '" & FixQuotes(barangAkhir) & "'"
        ElseIf Len(barangAwal) > 0 Then
            FilterKodeNamaBarang = IIf(Len(FilterKodeNamaBarang) > 0, String.Concat(FilterKodeNamaBarang, " AND"), FilterKodeNamaBarang)
            'JIKA BARANG AWAL SAJA YANG DIISI MAKA FilterKodeNamaBarang >= BARANG AWAL
            FilterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " >= '" & FixQuotes(barangAwal) & "'"
        ElseIf Len(barangAkhir) > 0 Then
            FilterKodeNamaBarang = IIf(Len(FilterKodeNamaBarang) > 0, String.Concat(FilterKodeNamaBarang, " AND"), FilterKodeNamaBarang)
            'JIKA BARANG AKHIR SAJA YANG DIISI MAKA FilterKodeNamaBarang <= BARANG AKHIR
            FilterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " <= '" & FixQuotes(barangAkhir) & "'"
        End If

        xstep = 1 ' Mulai hitung ulang
        Dim dtBarangID As DataTable
        Dim getIDBarang As String = ""

        xstep = 2 ' Jika ada filter berdasarkan kode barang maka ambil data barang tujuannya untuk gantil filter berdasarkan kode diganti idbarang
        If Len(FilterKodeNamaBarang) > 0 Then

            xstep = 2.1 ' Ambil data barang
            dtBarangID = AsDataTableAmbilDariDB("SELECT i.bid, i.bkode, i.btipe, i.bnama, i.bsatuan FROM m1_item i WHERE" & FilterKodeNamaBarang, strCon)

            xstep = 2.2 ' Buat filter idbarang dari data barang 2.1
            getIDBarang = ""
            For i = 0 To dtBarangID.Rows.Count - 1
                getIDBarang &= dtBarangID.Rows(i)("bid").ToString & ","
            Next

            If dtBarangID.Rows.Count > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " it.idbarang IN (" & getIDBarang.Substring(0, getIDBarang.Length - 1) & ")"
            End If
        End If

        xstep = 3 ' Buat filter dari paramaeter
        sql &= IIf(Len(Filter) > 0, String.Concat(" WHERE", Filter), Filter)

        xstep = 4 ' Buat sorting dari paramaeter khusus ambil transaksi barang
        'Dim sqlOrderBY As String = " ORDER BY it.idbarang, it.gudang"
        Dim sqlOrderBY As String = " ORDER BY it.tgl, it.inputtgl"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sqlOrderBY &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sqlOrderBY &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sqlOrderBY &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sqlOrderBY &= ", it.proyek"

        xstep = 4 ' Ambil data barang
        Dim dtBarangTransaksi As DataTable = AsDataTableAmbilDariDB(sql & sqlOrderBY, strCon)

        xstep = 5 ' Buat filter kontak, untuk ambil kontak berdasarkan kontak-kontak yang di filter
        getIDBarang = ""
        If dtBarangTransaksi.Rows.Count > 0 Then
            For i = 0 To dtBarangTransaksi.Rows.Count - 1
                getIDBarang &= dtBarangTransaksi.Rows(i)("kontak").ToString & ","
            Next
        End If



        xstep = 5.1 ' ambil data kontak
        Dim dtContact As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_contact WHERE kid IN (" & getIDBarang.Substring(0, getIDBarang.Length - 1) & ")", strCon)

        xstep = 6 ' Buat group by dari paramaeter
        sql &= " GROUP BY it.idbarang, it.gudang"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"

        xstep = 7 ' Buat order by dari paramaeter
        'sql &= " ORDER BY it.idbarang, it.gudang"
        sql &= " ORDER BY it.tgl, it.inputtgl"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then sql &= ", it.costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then sql &= ", it.divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then sql &= ", it.subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then sql &= ", it.proyek"

        xstep = 8 ' Ambil data transaksi barang
        'SimpanLogToFile("- xStep : 1@" & sql)
        Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql, strCon)

        xstep = 8.1 ' Jika ada filter berdasarkan kode barang maka ambil data barang tujuannya untuk gantil filter berdasarkan kode diganti idbarang
        If Len(FilterKodeNamaBarang) = 0 Then

            xstep = 8.2 ' Buat filter dari idbarang untuk ganti dari kodebarang
            getIDBarang = ""
            For i = 0 To dtBarang.Rows.Count - 1
                getIDBarang &= dtBarang.Rows(i)("idbarang").ToString & ","
            Next

            xstep = 8.3 ' Ambil data barang
            If dtBarang.Rows.Count > 0 Then
                dtBarangID = AsDataTableAmbilDariDB("SELECT i.bid, i.bkode, i.btipe, i.bnama, i.bsatuan FROM m1_item i WHERE bid IN (" & getIDBarang.Substring(0, getIDBarang.Length - 1) & ")", strCon)
            End If
        End If

        xstep = 9 ' Ambil data gudang, costcenter, divisi, subdivisi, project
        'SimpanLogToFile("- xStep : 2")
        Dim dtwarehouse As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_warehouse", strCon)
        Dim dtcost_center As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_cost_center", strCon)
        Dim dtdivision As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_division", strCon)
        Dim dtsubdivision As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_subdivision", strCon)
        Dim dtproject As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_project", strCon)

        xstep = 10 ' Ambil data branch, lokasi
        'SimpanLogToFile("- xStep : 3")
        Dim dtbranch As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_branch", strCon)
        Dim dtlocation As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_location", strCon)

        xstep = 11 ' jika ada barang per gudang yang di hitung ulang
        Dim dr6(), drAttNama() As DataRow
        'SimpanLogToFile("- xStep : 4 @" & dtBarang.Rows.Count.ToString)
        'PROSES SELECT DATA PERSEDIAAN ----------------------------------------------
        If dtBarang.Rows.Count > 0 Then

            xstep = 11.1 ' persiapan variabel penampung
            Dim idbarang As String = "", gudang As String = "", costcenter As String = ""
            Dim divisi As String = "", subdivisi As String = "", proyek As String = ""
            Dim dtSA As New DataTable, saldoawal As Double = 0
            Dim sqlSA As String = "", sqlSM As String = "", sqlSAGabung As String = "", sqlSMGabung As String = ""
            Dim sqlSAJadi As String = "", sqlSMJadi As String = ""
            strValue = New StringBuilder
            Dim gudangnama As String = ""
            Dim costcenternama As String = ""
            Dim divisinama As String = ""
            Dim subdivisinama As String = ""
            Dim proyeknama As String = ""
            Dim cabangnama As String = ""
            Dim lokasinama As String = ""
            Dim drBarang As DataRow

            xstep = 11.2 ' perulangan tiap barang per gudang yang akan di hitung
            'PERULANGAN PROSES MUTASI STOK
            For Each dr1 As DataRow In dtBarang.Rows

                xstep = 11.3 ' Set variabel dari perulangan ke variabel penampung
                'SimpanLogToFile("xstep 0")
                'SET IDBARANG, GUDANG
                idbarang = dr1("idbarang") : gudang = dr1("gudang")
                costcenter = dr1("costcenter") : divisi = dr1("divisi")
                subdivisi = dr1("subdivisi") : proyek = dr1("proyek")
                saldoawal = 0

                xstep = 11.4 ' Filter data barang berdasarkan barang yang di hitung
                dr6 = dtBarangID.Select("bid = '" & idbarang.ToString & "'", "")

                xstep = 11.5 ' Jika ada barang yang di hitung
                If dr6.Length > 0 Then

                    xstep = 11.6 ' Set datarow barang baris pertama
                    drBarang = dr6(0)

                    'SET STEP KE
                    stepKe += 1

                    xstep = 11.7 ' data transaksi barang di filter berdasarkan barang, gudang dan tgl yg di hitung sekarang
                    'AMBIL SALDO AWAL
                    dr6 = dtBarangTransaksi.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl < '" & FixQuotes(tglAwal) & "'", "")
                    'SimpanLogToFile(dr6.Length.ToString & "@")
                    If dr6.Length > 0 Then
                        'SimpanLogToFile("xstep 1")
                        xstep = 11.8 ' perulangan
                        dtSA = dr6.CopyToDataTable

                        For Each dr2 As DataRow In dtSA.Rows
                            'SimpanLogToFile(dr2("jenismutasi").ToString + " @ " + dr2("jmlbarang").ToString)
                            If dr2("jenismutasi") = 1 Then
                                saldoawal = saldoawal + dr2("jmlbarang")
                            Else
                                saldoawal = saldoawal - dr2("jmlbarang")
                            End If
                        Next

                        'SimpanLogToFile("saldoawal : " & saldoawal.ToString)

                        'SALDO AWAL
                        dr = dtSA.Rows(0)
                        'SimpanLogToFile("xstep 2")

                        xstep = 11.9 ' ambil nama attribut
                        dr6 = dtwarehouse.Select("wkode = '" & dr("gudang") & "'") : If dr6.Length > 0 Then gudangnama = dr6(0)("wnama")
                        dr6 = dtcost_center.Select("cckode = '" & dr("costcenter") & "'") : If dr6.Length > 0 Then costcenternama = dr6(0)("ccnama")
                        dr6 = dtdivision.Select("dkode = '" & dr("divisi") & "'") : If dr6.Length > 0 Then divisinama = dr6(0)("dnama")
                        dr6 = dtsubdivision.Select("sdkode = '" & dr("subdivisi") & "'") : If dr6.Length > 0 Then subdivisinama = dr6(0)("sdnama")
                        dr6 = dtproject.Select("pkode = '" & dr("proyek") & "'") : If dr6.Length > 0 Then proyeknama = dr6(0)("pnama")
                        dr6 = dtbranch.Select("bkode = '" & dr("cabang") & "'") : If dr6.Length > 0 Then cabangnama = dr6(0)("bnama")
                        dr6 = dtlocation.Select("lkode = '" & dr("lokasi") & "'") : If dr6.Length > 0 Then lokasinama = dr6(0)("lnama")

                        'BUAT VALUE SQL INSERT 
                        xstep = 11.11 ' set ke strvalue
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        ' idlogin, 1 as msnourut, ms.msid, ms.mscabang, ms.mscabangnama, ms.mslokasi, ms.mslokasinama, ms.msgudang, ms.msgudangnama, ms.mscostcenter, ms.mscostcenternama, ms.msdivisi, ms.msdivisinama, ms.mssubdivisi, ms.mssubdivisinama, ms.msproyek, ms.msproyeknama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, 0 as msjmlmasuk, 0 as msjmlkeluar, SUM(ms.msjmlmasuk - ms.msjmlkeluar) as mssaldo, ms.msinputtgl, '" & FixQuotes(idMsmq) & "' as idmsmq, '" & FixDouble(userid) & "' as msuserid, ms.mscustomtext1, ms.mscustomtext2, ms.mscustomtext3, ms.mscustomtext4, ms.mscustomtext5, ms.mscustomint1, ms.mscustomint2, ms.mscustomint3, ms.mscustomint4, ms.mscustomint5, ms.mscustomdbl1, ms.mscustomdbl2, ms.mscustomdbl3, ms.mscustomdbl4, ms.mscustomdbl5, ms.mscustomdate1, ms.mscustomdate2, ms.mscustomdate3, ms.mscustomdate4, ms.mscustomdate5
                        strValue.Append("('" & FixQuotes(idLogin) & "', '" & FixQuotes(1) & "', '" & FixQuotes(dr("id")) & "', '" & FixQuotes(dr("cabang")) & "', '" & _
                                        FixQuotes(cabangnama) & "', '" & FixQuotes(dr("lokasi")) & "', '" & FixQuotes(lokasinama) & "', '" & FixQuotes(dr("gudang")) _
                                        & "', '" & FixQuotes(gudangnama) & "', '" & FixQuotes(dr("costcenter")) & "', '" & FixQuotes(costcenternama) & "', '" & _
                                        FixQuotes(dr("divisi")) & "', '" & FixQuotes(divisinama) & "', '" & FixQuotes(dr("subdivisi")) & "', '" & _
                                        FixQuotes(subdivisinama) & "', '" & FixQuotes(dr("proyek")) & "', '" & FixQuotes(proyeknama) & "', '" & _
                                        FixQuotes(drBarang("bid")) & "', '" & FixQuotes(drBarang("bkode")) & "', '" & FixQuotes(drBarang("btipe")) & "', '" & _
                                        FixQuotes(drBarang("bnama")) & "', '" & FixQuotes(drBarang("bsatuan")) & "', '" & FixQuotes(tglAwal) & "', '" & _
                                        FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes(0) & "', '" & _
                                        FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("Saldo Awal") & "', '" & _
                                        FixQuotes("Saldo Awal") & "', '" & FixQuotes("Saldo Awal") & "', '" & FixQuotes(0) & "', '" & _
                                        FixQuotes(0) & "', '" & FixQuotes(saldoawal.ToString()) & "', '" & FixQuotes(tglAwal) & " 00:00:00" & "', '" & _
                                        FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes(dr("customtext1")) & "', '" & _
                                        FixQuotes(dr("customtext2")) & "', '" & FixQuotes(dr("customtext3")) & "', '" & FixQuotes(dr("customtext4")) & "', '" & _
                                        FixQuotes(dr("customtext5")) & "', '" & FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & _
                                        FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & _
                                        FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & _
                                        FixQuotes(0) & "', '" & FixQuotes(0) & "', '" & FixQuotes("1900-01-01") & "', '" & _
                                        FixQuotes("1900-01-01") & "', '" & FixQuotes("1900-01-01") & "', '" & FixQuotes("1900-01-01") & "', '" & _
                                        FixQuotes("1900-01-01") & "')")
                        'SimpanLogToFile("xstep 4")


                    End If
                    xstep = 11.12 ' Hitung saldo Mutasi
                    'SALDO MUTASI
                    Dim nourut As Integer = 0
                    Dim jmlmasuk, jmlkeluar As Double
                    dr6 = dtBarangTransaksi.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl >= '" & FixDouble(tglAwal) & "' AND tgl <= '" & FixDouble(tglAkhir) & "'", "")

                    'SimpanLogToFile("xstep 5")
                    xstep = 11.13 ' jika ada saldo mutasi
                    If dr6.Length > 0 Then
                        'SimpanLogToFile("go !")
                        dtSA = dr6.CopyToDataTable

                        drAttNama = dtwarehouse.Select("wkode = '" & dtSA.Rows(0)("gudang") & "'") : If drAttNama.Length > 0 Then gudangnama = drAttNama(0)("wnama")
                        drAttNama = dtcost_center.Select("cckode = '" & dtSA.Rows(0)("costcenter") & "'") : If drAttNama.Length > 0 Then costcenternama = drAttNama(0)("ccnama")
                        drAttNama = dtdivision.Select("dkode = '" & dtSA.Rows(0)("divisi") & "'") : If drAttNama.Length > 0 Then divisinama = drAttNama(0)("dnama")
                        drAttNama = dtsubdivision.Select("sdkode = '" & dtSA.Rows(0)("subdivisi") & "'") : If drAttNama.Length > 0 Then subdivisinama = drAttNama(0)("sdnama")
                        drAttNama = dtproject.Select("pkode = '" & dtSA.Rows(0)("proyek") & "'") : If drAttNama.Length > 0 Then proyeknama = drAttNama(0)("pnama")
                        drAttNama = dtbranch.Select("bkode = '" & dtSA.Rows(0)("cabang") & "'") : If drAttNama.Length > 0 Then cabangnama = drAttNama(0)("bnama")
                        drAttNama = dtlocation.Select("lkode = '" & dtSA.Rows(0)("lokasi") & "'") : If drAttNama.Length > 0 Then lokasinama = drAttNama(0)("lnama")
                        For Each dr3 As DataRow In dtSA.Rows
                            nourut += 1
                            dr6 = dtContact.Select("kid = '" & dr3("kontak") & "'")

                            'SimpanLogToFile(dr6.Length.ToString & " contact" & dr3("kontak"))
                            jmlmasuk = 0
                            jmlkeluar = 0

                            If dr3("jenismutasi") = 1 Then
                                jmlmasuk = dr3("jmlbarang")
                            Else
                                jmlkeluar = dr3("jmlbarang")
                            End If

                            saldoawal = saldoawal + jmlmasuk - jmlkeluar

                            'BUAT VALUE SQL INSERT 
                            strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                            ' idlogin, 1 as msnourut, ms.msid, ms.mscabang, ms.mscabangnama, ms.mslokasi, ms.mslokasinama, ms.msgudang, ms.msgudangnama, ms.mscostcenter, ms.mscostcenternama, ms.msdivisi, ms.msdivisinama, ms.mssubdivisi, ms.mssubdivisinama, ms.msproyek, ms.msproyeknama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, 0 as msjmlmasuk, 0 as msjmlkeluar, SUM(ms.msjmlmasuk - ms.msjmlkeluar) as mssaldo, ms.msinputtgl, '" & FixQuotes(idMsmq) & "' as idmsmq, '" & FixDouble(userid) & "' as msuserid, ms.mscustomtext1, ms.mscustomtext2, ms.mscustomtext3, ms.mscustomtext4, ms.mscustomtext5, ms.mscustomint1, ms.mscustomint2, ms.mscustomint3, ms.mscustomint4, ms.mscustomint5, ms.mscustomdbl1, ms.mscustomdbl2, ms.mscustomdbl3, ms.mscustomdbl4, ms.mscustomdbl5, ms.mscustomdate1, ms.mscustomdate2, ms.mscustomdate3, ms.mscustomdate4, ms.mscustomdate5
                            strValue.Append("('" & FixQuotes(idLogin) & "', '" & nourut.ToString & "', '" & FixQuotes(dr3("id")) & "', '" & FixQuotes(dr3("cabang")) & "', '" & _
                                            FixQuotes(cabangnama) & "', '" & FixQuotes(dr3("lokasi")) & "', '" & FixQuotes(lokasinama) & "', '" & FixQuotes(dr3("gudang")) & "', '" & _
                                            FixQuotes(gudangnama) & "', '" & FixQuotes(dr3("costcenter")) & "', '" & FixQuotes(costcenternama) & "', '" & FixQuotes(dr3("divisi")) & "', '" & _
                                            FixQuotes(divisinama) & "', '" & FixQuotes(dr3("subdivisi")) & "', '" & FixQuotes(subdivisinama) & "', '" & FixQuotes(dr3("proyek")) & "', '" & _
                                            FixQuotes(proyeknama) & "', '" & _
                                            FixQuotes(drBarang("bid")) & "', '" & FixQuotes(drBarang("bkode")) & "', '" & FixQuotes(drBarang("bnama")) & "', '" & FixQuotes(drBarang("bnama")) & "', '" & _
                                            FixQuotes(dr3("satuanbarang")) & "', '" & FixQuotes(AsFormatTanggal(dr3("tgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(dr3("sumber")) & "', '" & FixQuotes(dr3("notransaksi")) & "', '" & _
                                            FixQuotes(dr3("kontak")) & "', '" & FixQuotes(dr6(0)("kkode")) & "', '" & FixQuotes(dr6(0)("knama")) & "', '" & FixQuotes(dr3("uraian")) & "', '" & _
                                            FixQuotes(dr3("catatan")) & "', '" & FixQuotes(dr3("catatandetail")) & "', '" & FixQuotes(jmlmasuk.ToString()) & "', '" & FixQuotes(jmlkeluar.ToString()) & "', '" & _
                                            FixQuotes(saldoawal.ToString()) & "', '" & FixQuotes(AsFormatTanggal(dr3("inputtgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(idMsmq) & "', '" & _
                                            FixQuotes(userid.ToString()) & "', '" & FixQuotes(dr3("customtext1")) & "', '" & FixQuotes(dr3("customtext2")) & "', '" & FixQuotes(dr3("customtext3")) & "', '" & _
                                            FixQuotes(dr3("customtext4")) & "', '" & FixQuotes(dr3("customtext5")) & "', '" & FixQuotes(dr3("customint1")) & "', '" & FixQuotes(dr3("customint2")) & "', '" & _
                                            FixQuotes(dr3("customint3")) & "', '" & FixQuotes(dr3("customint4")) & "', '" & FixQuotes(dr3("customint5")) & "', '" & FixQuotes(dr3("customdbl1")) & "', '" & _
                                            FixQuotes(dr3("customdbl2")) & "', '" & FixQuotes(dr3("customdbl3")) & "', '" & FixQuotes(dr3("customdbl4")) & "', '" & FixQuotes(dr3("customdbl5")) & "', '" & _
                                            FixQuotes(AsFormatTanggal(dr3("customdate1"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate2"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate3"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr3("customdate4"), "yyyy-MM-dd")) & "', '" & _
                                            FixQuotes(AsFormatTanggal(dr3("customdate5"), "yyyy-MM-dd")) & "')")

                        Next

                        'SimpanLogToFile("barang : " & idbarang.ToString & " gudang : " & gudang & " saldoawal : " & saldoawal.ToString)
                        'SimpanLogToFile("done !")
                    Else
                        'SimpanLogToFile("SKIP !")
                    End If
                Else
                    ' Jika tidak ada barang yang di hitung ulang
                    'SimpanLogToFile("idbarang : " & idbarang.ToString)
                End If
                'UPDATE PROGRESS ====================================
                'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
                progressPersen = IIf(stepKe = dtBarang.Rows.Count, Prosentase, (Math.Round(Prosentase / dtBarang.Rows.Count, 2)) * stepKe)

                Dim sqlInsert As String = ""
                If strValue.Length > 0 Then
                    sqlInsert = "INSERT INTO M2r_Mutasi_Stok_Detail VALUES  " & strValue.ToString & ";"
                End If

                'UPDATE PROGRESS REPORT M0_MSMQ
                sql = sqlInsert & "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
                If AsEksekusiSQL(sql, strCon) = False Then
                    result(2) = "Failed updating progress Data Stock Report '" & FixQuotes(gudang) & "' Warehouse : " & IIf(orderBy = "bkode", dr1("bkode"), dr1("bnama")) : GoTo selesai
                End If

                If strValue.Length > 0 Then
                    strValue = New StringBuilder
                End If
                'END OF UPDATE PROGRESS =============================

            Next


        Else

            'UPDATE PROGRESS ====================================
            'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
            progressPersen = 100

            'UPDATE PROGRESS REPORT M0_MSMQ
            sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
            If AsEksekusiSQL(sql, strCon) = False Then
                result(2) = "Failed updating progress Data Stock Report 'Nothing' Warehouse : " & IIf(orderBy = "bkode", "Nothing", "Nothing") : GoTo selesai
            End If
            'END OF UPDATE PROGRESS =============================

        End If
        'SimpanLogToFile("- xStep : 5")
        'END OF PROSES SELECT DATA PERSEDIAAN ---------------------------------------

        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_RekapKartuStok(ByVal param As String) As String
        '//LAPORAN REKAP KARTU STOK

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String,
        'orderBy(4) As String, gudangAwal(5) As String, gudangAkhir(6) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, barangAwal, barangAkhir, 
        'orderBy, gudangAwal, gudangAkhir

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim FilterBarang As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder, strInsert As New StringBuilder
        Dim progressPersen As Double = 0, counter As Double = 0, batasCounter As Double = 1000

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = "", orderBy As String = ""
        'Dim gudangAwal As String = "", gudangAkhir As String = ""

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 7) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'barangAwal(2) As String
        barangAwal = dataDetail(2)

        'barangAkhir(3) As String
        barangAkhir = dataDetail(3)


        'orderBy(4) As String
        If Len(dataDetail(4)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(4).ToString <> "bkode" And dataDetail(4).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(4)
        End If

        ''gudangAwal(5) As String
        'gudangAwal = dataDetail(5)

        ''gudangAkhir(6) As String
        'gudangAkhir = dataDetail(6)
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()

        'HAPUS IDLOGIN PADA M2r_Kartu_Stok ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Kartu_Stok
        sql = "DELETE FROM M2r_Kartu_Stok WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace Stock Card data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Kartu_Stok -----------------------------------


        'FILTER BARANG (KODE/NAMA)
        If Len(barangAwal) > 0 And Len(barangAkhir) > 0 Then
            'JIKA BARANG AWAL DAN BARANG AKHIR DIISI MAKA FILTER BETWEEN
            FilterBarang = IIf(Len(FilterBarang) > 0, String.Concat(FilterBarang, " AND "), FilterBarang)
            FilterBarang &= " i." & FixQuotes(orderBy) & " BETWEEN '" & FixQuotes(barangAwal) & "' AND '" & FixQuotes(barangAkhir) & "'"
        ElseIf Len(barangAwal) > 0 Then
            'JIKA BARANG AWAL SAJA YANG DIISI MAKA FILTER >= BARANG AWAL
            FilterBarang = IIf(Len(FilterBarang) > 0, String.Concat(FilterBarang, " AND "), FilterBarang)
            FilterBarang &= " i." & FixQuotes(orderBy) & " >= '" & FixQuotes(barangAwal) & "'"
        ElseIf Len(barangAkhir) > 0 Then
            'JIKA BARANG AKHIR SAJA YANG DIISI MAKA FILTER <= BARANG AKHIR
            FilterBarang = IIf(Len(FilterBarang) > 0, String.Concat(FilterBarang, " AND "), FilterBarang)
            FilterBarang &= " i." & FixQuotes(orderBy) & " <= '" & FixQuotes(barangAkhir) & "'"
        End If

        If Len(FilterBarang) > 0 Then
            FilterBarang = " AND " & FilterBarang
        End If


        'AMBIL SALDO AWAL HPP
        Dim sqlHppSa As String = "", sqlSAJadi As String = ""
        sqlHppSa = "  SELECT '" & FixQuotes(idMsmq) & "' as idmsmq, i.bid, IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0) as kssaldojml, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) / (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0)) as kssaldohpp, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) as kssaldonilai "
        sqlHppSa &= " FROM m1_item i "
        sqlHppSa &= " JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.tgl < '" & FixQuotes(tglAwal) & "' " & FilterBarang
        sqlHppSa &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
        sqlHppSa &= " GROUP BY i.bid "
        Dim dtHppSa As DataTable = AsDataTableAmbilDariDB(sqlHppSa, strCon)

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 10 : stepKe = 1
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If

        'INSERT DATA SALDO AWAL HPP KE TABEL PEMBANTU
        If dtHppSa.Rows.Count > 0 Then
            Dim strHppSa As New StringBuilder : counter = 0
            For Each dr1 As DataRow In dtHppSa.Rows

                counter += 1
                strHppSa.Append(IIf(Len(strHppSa.ToString) = 0, "", ", "))

                'mapping :                       idmsmq,                                id,                                        jml,                                             hpp,                                             nilai
                strHppSa.Append("('" & FixQuotes(idMsmq) & "', '" & FixDouble(FxDB(dr1("bid"), "")) & "', '" & FixDouble(FxDB(dr1("kssaldojml"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldohpp"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldonilai"), 0)) & "')")

                If Len(strHppSa.ToString) > 0 And (counter >= dtHppSa.Rows.Count Or counter Mod batasCounter = 0) Then
                    sqlSAJadi = " Insert into m2r_hppglobalsa (idmsmq, id, jml, hpp, nilai) values" & strHppSa.ToString & ""
                    If AsEksekusiSQL(sqlSAJadi, strCon) = False Then
                        result(2) = "Failed proccessing Starting Balance COGS " & " - " & sqlSAJadi : GoTo selesai
                    End If
                    strHppSa.Clear()
                End If

            Next
        End If

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 20 : stepKe = 2
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If


        'AMBIL SALDO MASUK HPP
        Dim sqlHppMs As String = "", sqlMsJadi As String = ""
        sqlHppMs = "  SELECT '" & FixQuotes(idMsmq) & "' as idmsmq, i.bid, IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0) as kssaldojml, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) / (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0)) as kssaldohpp, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) as kssaldonilai "
        sqlHppMs &= " FROM m1_item i "
        sqlHppMs &= " JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.jenismutasi = 1 AND it.tgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' " & FilterBarang
        sqlHppMs &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
        sqlHppMs &= " GROUP BY i.bid "
        Dim dtHppMs As DataTable = AsDataTableAmbilDariDB(sqlHppMs, strCon)

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 30 : stepKe = 3
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If

        'INSERT DATA SALDO MASUK HPP KE TABEL PEMBANTU
        If dtHppMs.Rows.Count > 0 Then
            Dim strHppms As New StringBuilder : counter = 0
            For Each dr1 As DataRow In dtHppMs.Rows

                counter += 1
                strHppms.Append(IIf(Len(strHppms.ToString) = 0, "", ", "))

                'mapping :                       idmsmq,                                id,                                        jml,                                             hpp,                                             nilai
                strHppms.Append("('" & FixQuotes(idMsmq) & "', '" & FixDouble(FxDB(dr1("bid"), "")) & "', '" & FixDouble(FxDB(dr1("kssaldojml"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldohpp"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldonilai"), 0)) & "')")

                If Len(strHppms.ToString) > 0 And (counter >= dtHppMs.Rows.Count Or counter Mod batasCounter = 0) Then
                    sqlMsJadi = " Insert into m2r_hppglobalms (idmsmq, id, jml, hpp, nilai) values" & strHppms.ToString & ""
                    If AsEksekusiSQL(sqlMsJadi, strCon) = False Then
                        result(2) = "Failed proccessing COGS In " & " - " & sqlMsJadi : GoTo selesai
                    End If
                    strHppms.Clear()
                End If

            Next
        End If

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 40 : stepKe = 4
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If


        'AMBIL SALDO KELUAR HPP
        Dim sqlHppKl As String = "", sqlKlJadi As String = ""
        sqlHppKl = "  SELECT '" & FixQuotes(idMsmq) & "' as idmsmq, i.bid, IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0) as kssaldojml, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) / (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0)) as kssaldohpp, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) as kssaldonilai "
        sqlHppKl &= " FROM m1_item i "
        sqlHppKl &= " JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.jenismutasi = 0 AND it.tgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' " & FilterBarang
        sqlHppKl &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
        sqlHppKl &= " GROUP BY i.bid "
        Dim dtHppkl As DataTable = AsDataTableAmbilDariDB(sqlHppKl, strCon)

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 50 : stepKe = 5
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If

        'INSERT DATA SALDO KELUAR HPP KE TABEL PEMBANTU
        If dtHppkl.Rows.Count > 0 Then
            Dim strHppkl As New StringBuilder : counter = 0
            For Each dr1 As DataRow In dtHppkl.Rows

                counter += 1
                strHppkl.Append(IIf(Len(strHppkl.ToString) = 0, "", ", "))

                'mapping :                       idmsmq,                                id,                                        jml,                                             hpp,                                             nilai
                strHppkl.Append("('" & FixQuotes(idMsmq) & "', '" & FixDouble(FxDB(dr1("bid"), "")) & "', '" & FixDouble(FxDB(dr1("kssaldojml"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldohpp"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldonilai"), 0)) & "')")

                If Len(strHppkl.ToString) > 0 And (counter >= dtHppkl.Rows.Count Or counter Mod batasCounter = 0) Then
                    sqlKlJadi = " Insert into m2r_hppglobalkl (idmsmq, id, jml, hpp, nilai) values" & strHppkl.ToString & ""
                    If AsEksekusiSQL(sqlKlJadi, strCon) = False Then
                        result(2) = "Failed proccessing COGS Out " & " - " & sqlKlJadi : GoTo selesai
                    End If
                    strHppkl.Clear()
                End If

            Next
        End If

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 60 : stepKe = 6
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If


        'AMBIL SALDO AKHIR HPP
        Dim sqlHppSk As String = "", sqlSkJadi As String = ""
        sqlHppSk = "  SELECT '" & FixQuotes(idMsmq) & "' as idmsmq, i.bid, IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0) as kssaldojml, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) / (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0)) as kssaldohpp, (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) as kssaldonilai "
        sqlHppSk &= " FROM m1_item i "
        sqlHppSk &= " JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.tgl <= '" & FixQuotes(tglAkhir) & "' " & FilterBarang
        sqlHppSk &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 "
        sqlHppSk &= " GROUP BY i.bid "


        Dim dtHppsk As DataTable = AsDataTableAmbilDariDB(sqlHppSk, strCon)

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 70 : stepKe = 7
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If

        'INSERT DATA SALDO AKHIR HPP KE TABEL PEMBANTU
        If dtHppsk.Rows.Count > 0 Then
            Dim strHppsk As New StringBuilder : counter = 0
            For Each dr1 As DataRow In dtHppsk.Rows

                counter += 1
                strHppsk.Append(IIf(Len(strHppsk.ToString) = 0, "", ", "))

                'mapping :                       idmsmq,                                id,                                        jml,                                             hpp,                                             nilai
                strHppsk.Append("('" & FixQuotes(idMsmq) & "', '" & FixDouble(FxDB(dr1("bid"), "")) & "', '" & FixDouble(FxDB(dr1("kssaldojml"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldohpp"), 0)) & "', '" & FixDouble(FxDB(dr1("kssaldonilai"), 0)) & "')")

                If Len(strHppsk.ToString) > 0 And (counter >= dtHppsk.Rows.Count Or counter Mod batasCounter = 0) Then
                    sqlSkJadi = " Insert into m2r_hppglobalsk (idmsmq, id, jml, hpp, nilai) values" & strHppsk.ToString & ""
                    If AsEksekusiSQL(sqlSkJadi, strCon) = False Then
                        result(2) = "Failed proccessing Ending Balance COGS " & " - " & sqlSkJadi : GoTo selesai
                    End If
                    strHppsk.Clear()
                End If

            Next
        End If

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 80 : stepKe = 8
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If


        'AMBIL SALDO AKHIR REPORT
        Dim sqlReport As String = "", sqlReportJadi As String = ""
        sqlReport = "  SELECT '" & FixQuotes(idLogin) & "' as idlogin, sk.id as ksnourut, 0 as ksid, '' as ksgudang, '' as ksgudangnama, i.bkategori as kskategoribarang, ic.icnama as kskategoribarangnama, sk.id as ksidbarang, i.bkode as kskodebarang, i.btipe as kstipebarang, i.bnama as ksnamabarang, i.bsatuan as kssatuanbarang, '" & FixQuotes(tglAkhir) & "' as kstgl, 'KS' as kssumber, 'KS' as ksnotransaksi, 1 as kskontak, 'KS' as kskontakkode, 'KS' as kskontaknama, 'KS' as ksuraian, '' as kscatatan, '' as kscatatandetail, s.snilai as ksmatauang, 1 as kskurs, 0 as ksharga, 0 as ksdiskon, 0 as ksjmldiskon, 1 ksjenismutasi, IFNULL(ms.jml,0) as ksjmlmasuk, IFNULL(ms.hpp,0) as kshargamasuk, IFNULL(ms.nilai,0) as ksnilaimasuk, IFNULL(kl.jml*-1,0) as ksjmlkeluar, IFNULL(kl.hpp,0) as kshargakeluar, IFNULL(kl.nilai*-1,0) as ksnilaikeluar, sk.jml as kssaldojml, sk.hpp as kssaldohpp, sk.nilai as kssaldonilai, '1971-01-01 00:00:00' as kspostingtgl, '1971-01-01 00:00:00' as ksinputtgl, '" & FixQuotes(idMsmq) & "' as idmsmq, '" & FixDouble(userid) & "' as ksuserid, '' as kscustomtext1, '' as kscustomtext2, '' as kscustomtext3, '' as kscustomtext4, '' as kscustomtext5, '0' as kscustomint1, '0' as kscustomint2, '0' as kscustomint3, '0' as kscustomint4, '0' as kscustomint5, IFNULL(sa.jml,0) as kscustomdbl1, IFNULL(sa.hpp,0) as kscustomdbl2, IFNULL(sa.nilai,0) as kscustomdbl3, '0' as kscustomdbl4, '0' as kscustomdbl5, '1900-01-01' as kscustomdate1, '1900-01-01' as kscustomdate2, '1900-01-01' as kscustomdate3, '1900-01-01' as kscustomdate4, '1900-01-01' as kscustomdate5 "
        sqlReport &= " FROM m2r_hppglobalsk sk "
        sqlReport &= " JOIN m1_item i ON sk.id = i.bid AND sk.idmsmq = '" & FixQuotes(idMsmq) & "' "
        sqlReport &= " JOIN m0_setting s ON s.smodule = 0 AND s.sgrup = 'accounting' AND s.skode = 'MataUangFungsional' "
        sqlReport &= " LEFT JOIN m1_item_category ic ON i.bkategori = ic.ickode "
        sqlReport &= " LEFT JOIN m2r_hppglobalsa sa ON sk.id = sa.id AND sk.idmsmq = sa.idmsmq "
        sqlReport &= " LEFT JOIN m2r_hppglobalms ms ON sk.id = ms.id AND sk.idmsmq = ms.idmsmq "
        sqlReport &= " LEFT JOIN m2r_hppglobalkl kl ON sk.id = kl.id AND sk.idmsmq = kl.idmsmq "
        sqlReport &= " WHERE IFNULL(ms.jml,0) <> 0 OR IFNULL(ms.hpp,0) <> 0 OR IFNULL(ms.nilai,0) <> 0 OR IFNULL(kl.jml,0) <> 0 OR IFNULL(kl.hpp,0) <> 0 OR IFNULL(kl.nilai,0) <> 0 OR sk.jml <> 0 OR sk.hpp <> 0 OR sk.nilai <> 0 OR IFNULL(sa.jml,0) <> 0 OR IFNULL(sa.hpp,0) <> 0 OR IFNULL(sa.nilai,0) <> 0 "
        sqlReport &= " ORDER BY i.bkategori ASC, i.bkode ASC "
        Dim dtReport As DataTable = AsDataTableAmbilDariDB(sqlReport, strCon)

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 90 : stepKe = 9
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If

        'INSERT DATA SALDO AKHIR HPP KE TABEL PEMBANTU
        If dtReport.Rows.Count > 0 Then
            Dim strreport As New StringBuilder : counter = 0
            For Each drInsert As DataRow In dtReport.Rows

                counter += 1
                strreport.Append(IIf(Len(strreport.ToString) = 0, "", ", "))

                'mapping :                                       idlogin,                                            ksnourut,                                           ksid,                                           ksgudang,                                           ksgudangnama,                                               kskategoribarang,                                           kskategoribarangnama,                                           ksidbarang,                                             kskodebarang,                                           kstipebarang,                                           ksnamabarang,                                               kssatuanbarang,                                                         kstgl,                                                          kssumber,                                           ksnotransaksi,                                          kskontak,                                           kskontakkode,                                            kskontaknama,                                              ksuraian,                                           kscatatan,                                          kscatatandetail,                                            ksmatauang,                                             kskurs,                                             ksharga,                                         ksdiskon,                                          ksjmldiskon,                                            ksjenismutasi,                                              ksjmlmasuk,                                         kshargamasuk,                                           ksnilaimasuk,                                           ksjmlkeluar,                                            kshargakeluar,                                          ksnilaikeluar,                                          kssaldojml,                                             kssaldohpp,                                         kssaldonilai,                                                           kspostingtgl,                                                                                                        ksinputtgl,                                                                                    idmsmq,                                             ksuserid,                                           kscustomtext1,                                          kscustomtext2,                                              kscustomtext3,                                              kscustomtext4,                                          kscustomtext5,                                           kscustomint1,                                          kscustomint2,                                           kscustomint3,                                           kscustomint4,                                           kscustomint5,                                               kscustomdbl1,                                       kscustomdbl2,                                               kscustomdbl3,                                       kscustomdbl4,                                           kscustomdbl5,                                                           kscustomdate1,                                                                       kscustomdate2,                                                                         kscustomdate3,                                                                      kscustomdate4,                                                                          kscustomdate5
                strreport.Append("('" & FixQuotes(FxDB(drInsert("idlogin"), "")) & "', '" & FixDouble(FxDB(drInsert("ksnourut"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksid"), 0)) & "', '" & FixQuotes(FxDB(drInsert("ksgudang"), "")) & "', '" & FixQuotes(FxDB(drInsert("ksgudangnama"), "")) & "', '" & FixQuotes(FxDB(drInsert("kskategoribarang"), "")) & "', '" & FixQuotes(FxDB(drInsert("kskategoribarangnama"), "")) & "', '" & FixDouble(FxDB(drInsert("ksidbarang"), 0)) & "', '" & FixQuotes(FxDB(drInsert("kskodebarang"), "")) & "', '" & FixQuotes(FxDB(drInsert("kstipebarang"), "")) & "', '" & FixQuotes(FxDB(drInsert("ksnamabarang"), "")) & "', '" & FixQuotes(FxDB(drInsert("kssatuanbarang"), "")) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kstgl"), "1900-01-01"))) & "', '" & FixQuotes(FxDB(drInsert("kssumber"), "")) & "', '" & FixQuotes(FxDB(drInsert("ksnotransaksi"), "")) & "', '" & FixDouble(FxDB(drInsert("kskontak"), 0)) & "', '" & FixQuotes(FxDB(drInsert("kskontakkode"), "")) & "', '" & FixQuotes(FxDB(drInsert("kskontaknama"), "")) & "', '" & FixQuotes(FxDB(drInsert("ksuraian"), "")) & "', '" & FixQuotes(FxDB(drInsert("kscatatan"), "")) & "', '" & FixQuotes(FxDB(drInsert("kscatatandetail"), "")) & "', '" & FixQuotes(FxDB(drInsert("ksmatauang"), "")) & "', '" & FixDouble(FxDB(drInsert("kskurs"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksharga"), 0)) & "', '" & FixQuotes(FxDB(drInsert("ksdiskon"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksjmldiskon"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksjenismutasi"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksjmlmasuk"), 0)) & "', '" & FixDouble(FxDB(drInsert("kshargamasuk"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksnilaimasuk"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksjmlkeluar"), 0)) & "', '" & FixDouble(FxDB(drInsert("kshargakeluar"), 0)) & "', '" & FixDouble(FxDB(drInsert("ksnilaikeluar"), 0)) & "', '" & FixDouble(FxDB(drInsert("kssaldojml"), 0)) & "', '" & FixDouble(FxDB(drInsert("kssaldohpp"), 0)) & "', '" & FixDouble(FxDB(drInsert("kssaldonilai"), 0)) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kspostingtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("ksinputtgl"), "1971-01-01 00:00:00"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(FxDB(drInsert("idmsmq"), "")) & "', '" & FixDouble(FxDB(drInsert("ksuserid"), 0)) & "', '" & FixQuotes(FxDB(drInsert("kscustomtext1"), "")) & "', '" & FixQuotes(FxDB(drInsert("kscustomtext2"), "")) & "', '" & FixQuotes(FxDB(drInsert("kscustomtext3"), "")) & "', '" & FixQuotes(FxDB(drInsert("kscustomtext4"), "")) & "', '" & FixQuotes(FxDB(drInsert("kscustomtext5"), "")) & "', '" & FixDouble(FxDB(drInsert("kscustomint1"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomint2"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomint3"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomint4"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomint5"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomdbl1"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomdbl2"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomdbl3"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomdbl4"), 0)) & "', '" & FixDouble(FxDB(drInsert("kscustomdbl5"), 0)) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kscustomdate1"), "1900-01-01"))) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kscustomdate2"), "1900-01-01"))) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kscustomdate3"), "1900-01-01"))) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kscustomdate4"), "1900-01-01"))) & "', '" & FixQuotes(AsFormatTanggal(FxDB(drInsert("kscustomdate5"), "1900-01-01"))) & "')")

                If Len(strreport.ToString) > 0 And (counter >= dtReport.Rows.Count Or counter Mod batasCounter = 0) Then
                    sqlReportJadi = "  Insert into M2r_Kartu_Stok (idlogin,ksnourut,ksid,ksgudang,ksgudangnama,kskategoribarang,kskategoribarangnama,ksidbarang,kskodebarang,kstipebarang,ksnamabarang,kssatuanbarang,kstgl,kssumber,ksnotransaksi,kskontak,kskontakkode,kskontaknama,ksuraian,kscatatan,kscatatandetail,ksmatauang,kskurs,ksharga,ksdiskon,ksjmldiskon,ksjenismutasi,ksjmlmasuk,kshargamasuk,ksnilaimasuk,ksjmlkeluar,kshargakeluar,ksnilaikeluar,kssaldojml,kssaldohpp,kssaldonilai,kspostingtgl,ksinputtgl,idmsmq,ksuserid,kscustomtext1,kscustomtext2,kscustomtext3,kscustomtext4,kscustomtext5,kscustomint1,kscustomint2,kscustomint3,kscustomint4,kscustomint5,kscustomdbl1,kscustomdbl2,kscustomdbl3,kscustomdbl4,kscustomdbl5,kscustomdate1,kscustomdate2,kscustomdate3,kscustomdate4,kscustomdate5) values" & strreport.ToString & ""
                    If AsEksekusiSQL(sqlReportJadi, strCon) = False Then
                        result(2) = "Failed finishing report " & " - " & sqlReportJadi : GoTo selesai
                    End If
                    strreport.Clear()
                End If

            Next
        End If

        'UPDATE PROGRESS REPORT M0_MSMQ
        progressPersen = 100 : stepKe = 10
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed updating progress Data Stock Card " & stepKe : GoTo selesai
        End If

        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_MutasiStok_RekapNew(ByVal param As String) As String
        '//LAPORAN MUTASI STOK REKAP (CABANG. LOKASI, GUDANG, COSTCENTER, DIVISI, SUBDIVISI, PROYEK)

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String,
        'cabangAwal(4) As String, cabangAkhir(5) As String, lokasiAwal(6) As String, lokasiAkhir(7) As String,
        'gudangAwal(8) As String, gudangAkhir(9) As String, costcenterAwal(10) As String, costcenterAkhir(11) As String, 
        'divisiAwal(12) As String, divisiAkhir(13) As String, subdivisiAwal(14) As String, subdivisiAkhir(15) As String, 
        'proyekAwal(16) As String, proyekAkhir(17) As String, orderBy(18) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, barangAwal, barangAkhir,
        'cabangAwal, cabangAkhir, lokasiAwal, lokasiAkhir,
        'gudangAwal, gudangAkhir, costcenterAwal, costcenterAkhir, 
        'divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, 
        'proyekAwal, proyekAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim cabangAwal As String = "", cabangAkhir As String = "", lokasiAwal As String = "", lokasiAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", costcenterAwal As String = "", costcenterAkhir As String = ""
        Dim divisiAwal As String = "", divisiAkhir As String = "", subdivisiAwal As String = "", subdivisiAkhir As String = ""
        Dim proyekAwal As String = "", proyekAkhir As String = "", orderBy As String = ""

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 19) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'barangAwal(2) As String
        barangAwal = dataDetail(2)

        'barangAkhir(3) As String
        barangAkhir = dataDetail(3)

        'cabangAwal(4) As String
        cabangAwal = dataDetail(4)

        'cabangAkhir(5) As String
        cabangAkhir = dataDetail(5)

        'lokasiAwal(6) As String
        lokasiAwal = dataDetail(6)

        'lokasiAkhir(7) As String
        lokasiAkhir = dataDetail(7)

        'gudangAwal(8) As String
        gudangAwal = dataDetail(8)

        'gudangAkhir(9) As String
        gudangAkhir = dataDetail(9)

        'costcenterAwal(10) As String
        costcenterAwal = dataDetail(10)

        'costcenterAkhir(11) As String
        costcenterAkhir = dataDetail(11)

        'divisiAwal(12) As String
        divisiAwal = dataDetail(12)

        'divisiAkhir(13) As String
        divisiAkhir = dataDetail(13)

        'subdivisiAwal(14) As String
        subdivisiAwal = dataDetail(14)

        'subdivisiAkhir(15) As String
        subdivisiAkhir = dataDetail(15)

        'proyekAwal(16) As String
        proyekAwal = dataDetail(16)

        'proyekAkhir(17) As String
        proyekAkhir = dataDetail(17)

        'orderBy(18) As String
        If Len(dataDetail(18)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(18).ToString <> "bkode" And dataDetail(18).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(18)
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()


        'HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Mutasi_Stok_Detail
        sql = "DELETE FROM M2r_Mutasi_Stok_Detail WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace Stock Report data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Mutasi_Stok_Detail -----------------------------------



        AsEksekusiSQL("UPDATE m0_msmq SET , pesan = 'Cek Setting' WHERE id = '" & FixQuotes(idMsmq) & "'", strCon)
        'CEK SUDAH INPUT STOCK OPNAME ATAU BELUM, BERDASARKAN SETTING
        'JIKA SETTING CETAK MUTASI STOK REKAP HARUS INPUT STOCK OPNAME MAKA CEK DATA DAHULU
        sql = "  SELECT s.snilai, IFNULL(c.ccabang,'') as ccabang, IFNULL(c.clokasi,'') as clokasi, IFNULL(c.cgudang,'') as cgudang "
        sql &= " FROM m0_setting s "
        sql &= " LEFT JOIN m2r_mutasi_stok_custom c "
        sql &= " ON c.ctgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.ccabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            sql &= " AND c.ccabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            sql &= " AND c.ccabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If
        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.clokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            sql &= " AND c.clokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            sql &= " AND c.clokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If
        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            sql &= " AND c.cgudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            sql &= " AND c.cgudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            sql &= " AND c.cgudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If
        sql &= " WHERE s.smodule = 3 AND s.sgrup = 'stockopname' AND s.skode = 'CetakMutasiStokRekap' "
        sql &= " GROUP BY s.smodule, s.sgrup, s.skode, c.ccabang, c.clokasi, c.cgudang, c.ctgl "

        'result(2) = "sql " & sql : GoTo selesai

        Dim isCustom As String = "", ftCabang As String = "", ftLokasi As String = "", ftGudang As String = ""
        Dim dtCustom As DataTable = AsDataTableAmbilDariDB(sql, strCon) 'snilai, ccabang, clokasi, cgudang
        If dtCustom.Rows.Count > 0 Then
            For Each drCustom In dtCustom.Rows
                'SET SETTING
                isCustom = drCustom("snilai")

                'FILTER CABANG
                ftCabang &= IIf(Len(ftCabang) > 0, ", ", "")
                ftCabang &= "'" & FixQuotes(drCustom("ccabang")) & "'"

                'FILTER LOKASI
                ftLokasi &= IIf(Len(ftLokasi) > 0, ", ", "")
                ftLokasi &= "'" & FixQuotes(drCustom("clokasi")) & "'"

                'FILTER GUDANG
                ftGudang &= IIf(Len(ftGudang) > 0, ", ", "")
                ftGudang &= "'" & FixQuotes(drCustom("cgudang")) & "'"

            Next

        Else
            result(2) = "Setting for Stock Summary permission not found." : GoTo selesai

        End If

        AsEksekusiSQL("UPDATE m0_msmq SET , pesan = 'Proses parameter' WHERE id = '" & FixQuotes(idMsmq) & "'", strCon)
        ' Optimasi mulai disini
        Dim m1_item As DataTable, getFilter As String = ""

        'AMBIL BARANG SESUAI FILTER --------------------------------------------------
        'sql = "  SELECT i.bid, i.bkode, i.bnama, ifnull(it.cabang,'') as  cabang, ifnull(it.lokasi,'') as lokasi, w.wkode as gudang, ifnull(it.costcenter,'') as costcenter, ifnull(it.divisi,'') as divisi, ifnull(it.subdivisi,'') as subdivisi, ifnull(it.proyek,'') as proyek"
        'sql = "  SELECT inputtgl, tgl, idbarang, gudang, ifnull(it.costcenter,'') as costcenter, ifnull(it.divisi,'') as divisi, ifnull(it.subdivisi,'') as subdivisi, ifnull(it.proyek,'') as proyek, it.id as msid, it.cabang as mscabang, it.lokasi as mslokasi, it.gudang as msgudang, IFNULL(it.costcenter,'') as mscostcenter, IFNULL(it.divisi,'') as msdivisi, IFNULL(it.subdivisi,'') as mssubdivisi, IFNULL(it.proyek,'') as msproyek, it.idbarang as msidbarang, it.satuanbarang as mssatuanbarang, it.tgl as mstgl, it.sumber as mssumber, it.notransaksi as msnotransaksi, it.kontak as mskontak, it.uraian as msuraian, it.catatan as mscatatan, it.catatandetail as mscatatandetail, it.jenismutasi, it.jmlbarang, 0 as msjmlmasuk, 0 as msjmlkeluar, it.inputtgl as msinputtgl, '' as mscustomtext1, it.customtext2 as mscustomtext2, it.customtext3 as mscustomtext3, it.customtext4 as mscustomtext4, it.customtext5 as mscustomtext5, it.customint1 as mscustomint1, it.customint2 as mscustomint2, it.customint3 as mscustomint3, it.customint4 as mscustomint4, it.customint5 as mscustomint5, '0' as mscustomdbl1, 0 as mscustomdbl2, 0 as mscustomdbl3, 0 as mscustomdbl4, it.customdbl5 as mscustomdbl5, it.customdate1 as mscustomdate1, it.customdate2 as mscustomdate2, it.customdate3 as mscustomdate3, it.customdate4 as mscustomdate4, it.customdate5 as mscustomdate5"
        sql = "  SELECT *"
        'SELECT idbarang, gudang FROM `m1_item_transaction` GROUP BY idbarang, gudang
        'sql &= " FROM (SELECT idbarang, gudang FROM m1_item_transaction GROUP BY idbarang, gudang) ititem "
        'sql &= " JOIN m1_item i ON ititem.idbarang = i.bid "
        'sql &= " JOIN m1_warehouse w ON ititem.gudang = w.wkode "
        'sql &= " LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND w.wkode = it.gudang "

        sql &= " FROM m1_item_transaction "
        'sql &= " JOIN m1_item i ON it.idbarang = i.bid "
        'sql &= " JOIN m1_warehouse w ON it.gudang = w.wkode "

        'JIKA SETTING CETAK MUTASI STOK REKAP HARUS INPUT STOCK OPNAME MAKA TAMBAHKAN FILTER SESUAI DATA YANG SUDAH DIAMBIL
        If isCustom = 1 Then
            'FILTER CABANG CUSTOM
            If Len(ftCabang) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " cabang IN(" & ftCabang & ")"
            End If

            'FILTER LOKASI CUSTOM
            If Len(ftLokasi) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " lokasi IN(" & ftLokasi & ")"
            End If

            'FILTER GUDANG CUSTOM
            If Len(ftGudang) > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " gudang IN(" & ftGudang & ")"
            End If
        End If


        'FILTER CABANG
        If Len(cabangAwal) > 0 And Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL DAN CABANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " cabang BETWEEN '" & FixQuotes(cabangAwal) & "' AND '" & FixQuotes(cabangAkhir) & "'"
        ElseIf Len(cabangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AWAL SAJA YANG DIISI MAKA FILTER >= CABANG AWAL
            Filter &= " cabang >= '" & FixQuotes(cabangAwal) & "'"
        ElseIf Len(cabangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA CABANG AKHIR SAJA YANG DIISI MAKA FILTER <= CABANG AKHIR
            Filter &= " cabang <= '" & FixQuotes(cabangAkhir) & "'"
        End If

        'FILTER LOKASI
        If Len(lokasiAwal) > 0 And Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL DAN LOKASI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " lokasi BETWEEN '" & FixQuotes(lokasiAwal) & "' AND '" & FixQuotes(lokasiAkhir) & "'"
        ElseIf Len(lokasiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AWAL SAJA YANG DIISI MAKA FILTER >= LOKASI AWAL
            Filter &= " lokasi >= '" & FixQuotes(lokasiAwal) & "'"
        ElseIf Len(lokasiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA LOKASI AKHIR SAJA YANG DIISI MAKA FILTER <= LOKASI AKHIR
            Filter &= " lokasi <= '" & FixQuotes(lokasiAkhir) & "'"
        End If

        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL DAN GUDANG AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " gudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "'"
        ElseIf Len(gudangAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AWAL SAJA YANG DIISI MAKA FILTER >= GUDANG AWAL
            Filter &= " gudang >= '" & FixQuotes(gudangAwal) & "'"
        ElseIf Len(gudangAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA GUDANG AKHIR SAJA YANG DIISI MAKA FILTER <= GUDANG AKHIR
            Filter &= " gudang <= '" & FixQuotes(gudangAkhir) & "'"
        End If

        'FILTER COSTCENTER
        If Len(costcenterAwal) > 0 And Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL DAN COSTCENTER AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " costcenter BETWEEN '" & FixQuotes(costcenterAwal) & "' AND '" & FixQuotes(costcenterAkhir) & "'"
        ElseIf Len(costcenterAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AWAL SAJA YANG DIISI MAKA FILTER >= COSTCENTER AWAL
            Filter &= " costcenter >= '" & FixQuotes(costcenterAwal) & "'"
        ElseIf Len(costcenterAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA COSTCENTER AKHIR SAJA YANG DIISI MAKA FILTER <= COSTCENTER AKHIR
            Filter &= " costcenter <= '" & FixQuotes(costcenterAkhir) & "'"
        End If

        'FILTER DIVISI
        If Len(divisiAwal) > 0 And Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL DAN DIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " divisi BETWEEN '" & FixQuotes(divisiAwal) & "' AND '" & FixQuotes(divisiAkhir) & "'"
        ElseIf Len(divisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AWAL SAJA YANG DIISI MAKA FILTER >= DIVISI AWAL
            Filter &= " divisi >= '" & FixQuotes(divisiAwal) & "'"
        ElseIf Len(divisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA DIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= DIVISI AKHIR
            Filter &= " divisi <= '" & FixQuotes(divisiAkhir) & "'"
        End If

        'FILTER SUBDIVISI
        If Len(subdivisiAwal) > 0 And Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL DAN SUBDIVISI AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " subdivisi BETWEEN '" & FixQuotes(subdivisiAwal) & "' AND '" & FixQuotes(subdivisiAkhir) & "'"
        ElseIf Len(subdivisiAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AWAL SAJA YANG DIISI MAKA FILTER >= SUBDIVISI AWAL
            Filter &= " subdivisi >= '" & FixQuotes(subdivisiAwal) & "'"
        ElseIf Len(subdivisiAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA SUBDIVISI AKHIR SAJA YANG DIISI MAKA FILTER <= SUBDIVISI AKHIR
            Filter &= " subdivisi <= '" & FixQuotes(subdivisiAkhir) & "'"
        End If

        'FILTER PROYEK
        If Len(proyekAwal) > 0 And Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL DAN PROYEK AKHIR DIISI MAKA FILTER BETWEEN
            Filter &= " proyek BETWEEN '" & FixQuotes(proyekAwal) & "' AND '" & FixQuotes(proyekAkhir) & "'"
        ElseIf Len(proyekAwal) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AWAL SAJA YANG DIISI MAKA FILTER >= PROYEK AWAL
            Filter &= " proyek >= '" & FixQuotes(proyekAwal) & "'"
        ElseIf Len(proyekAkhir) > 0 Then
            Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
            'JIKA PROYEK AKHIR SAJA YANG DIISI MAKA FILTER <= PROYEK AKHIR
            Filter &= " proyek <= '" & FixQuotes(proyekAkhir) & "'"
        End If

        Dim filterKodeNamaBarang As String = ""

        'filterKodeNamaBarang BARANG (KODE/NAMA)
        If Len(barangAwal) > 0 And Len(barangAkhir) > 0 Then
            filterKodeNamaBarang = IIf(Len(filterKodeNamaBarang) > 0, String.Concat(filterKodeNamaBarang, " AND"), filterKodeNamaBarang)
            'JIKA BARANG AWAL DAN BARANG AKHIR DIISI MAKA filterKodeNamaBarang BETWEEN
            filterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " BETWEEN '" & FixQuotes(barangAwal) & "' AND '" & FixQuotes(barangAkhir) & "'"
        ElseIf Len(barangAwal) > 0 Then
            filterKodeNamaBarang = IIf(Len(filterKodeNamaBarang) > 0, String.Concat(filterKodeNamaBarang, " AND"), filterKodeNamaBarang)
            'JIKA BARANG AWAL SAJA YANG DIISI MAKA filterKodeNamaBarang >= BARANG AWAL
            filterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " >= '" & FixQuotes(barangAwal) & "'"
        ElseIf Len(barangAkhir) > 0 Then
            filterKodeNamaBarang = IIf(Len(filterKodeNamaBarang) > 0, String.Concat(filterKodeNamaBarang, " AND"), filterKodeNamaBarang)
            'JIKA BARANG AKHIR SAJA YANG DIISI MAKA filterKodeNamaBarang <= BARANG AKHIR
            filterKodeNamaBarang &= " i." & FixQuotes(orderBy) & " <= '" & FixQuotes(barangAkhir) & "'"
        End If

        'result(2) = "filterKodeNamaBarang : " & filterKodeNamaBarang : GoTo selesai

        AsEksekusiSQL("UPDATE m0_msmq SET , pesan = 'Get Data Item' WHERE id = '" & FixQuotes(idMsmq) & "'", strCon)
        If Len(filterKodeNamaBarang) > 0 Then
            SimpanLogToFile("SELECT * FROM m1_item i WHERE " & filterKodeNamaBarang)
            m1_item = AsDataTableAmbilDariDB("SELECT bid, bkode FROM m1_item i WHERE " & filterKodeNamaBarang, strCon)
            'result(2) = "SELECT * FROM m1_item WHERE " & filterKodeNamaBarang : GoTo selesai
            getFilter = ""
            For d = 0 To m1_item.Rows.Count - 1
                getFilter &= m1_item.Rows(d)("bid").ToString & ","
            Next

            If m1_item.Rows.Count > 0 Then
                Filter = IIf(Len(Filter) > 0, String.Concat(Filter, " AND"), Filter)
                Filter &= " idbarang IN (" & getFilter.Substring(0, getFilter.Length - 1) & ")"
            End If
        Else
            SimpanLogToFile("SELECT * FROM m1_item")
            m1_item = AsDataTableAmbilDariDB("SELECT * FROM m1_item", strCon)
        End If

        'FILTER
        sql &= IIf(Len(Filter) > 0, String.Concat(" WHERE", Filter), Filter)

        'AMBIL DATA BARANG
        SimpanLogToFile(sql)
        Dim dtBarang As DataTable = AsDataTableAmbilDariDB(sql, strCon)
        'END OF AMBIL BARANG SESUAI FILTER ------------------------------------------
        Dim dtBaranggrp As New DataTable
        Dim drBarangFilter = dtBarang.AsEnumerable().GroupBy(Function(r) New With { _
            Key .f1 = r("idbarang"), _
            Key .f2 = r("gudang"), _
            Key .f3 = IIf(Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0, r("costcenter"), ""),
            Key .f4 = IIf(Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0, r("divisi"), ""),
            Key .f5 = IIf(Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0, r("subdivisi"), ""),
            Key .f6 = IIf(Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0, r("proyek"), "") _
        }).[Select](Function(g) g.OrderBy(Function(r) r("idbarang")).First())

        If drBarangFilter.Count > 0 Then
            dtBaranggrp = drBarangFilter.CopyToDataTable()
        End If

        'ORDER BY
        Dim strOrderBy As String = "idbarang, gudang"
        If Len(costcenterAwal) > 0 Or Len(costcenterAkhir) > 0 Then strOrderBy &= ", costcenter"
        If Len(divisiAwal) > 0 Or Len(divisiAkhir) > 0 Then strOrderBy &= ", divisi"
        If Len(subdivisiAwal) > 0 Or Len(subdivisiAkhir) > 0 Then strOrderBy &= ", subdivisi"
        If Len(proyekAwal) > 0 Or Len(proyekAkhir) > 0 Then strOrderBy &= ", proyek"
        dtBaranggrp.DefaultView.Sort = strOrderBy
        dtBaranggrp = dtBaranggrp.DefaultView.ToTable()

        Dim dtwarehouse As DataTable = AsDataTableAmbilDariDB("select * from m1_warehouse", strCon)

        'PROSES SELECT DATA PERSEDIAAN ----------------------------------------------
        If dtBaranggrp.Rows.Count > 0 Then

            Dim idbarang As String = "", gudang As String = "", costcenter As String = ""
            Dim divisi As String = "", subdivisi As String = "", proyek As String = ""
            Dim dtSA As New DataTable, jmlkeluar As Double = 0, jmlmasuk As Double = 0
            Dim sqlSA As String = "", sqlSM As String = "", sqlSAGabung As String = "", sqlSMGabung As String = ""
            Dim sqlSAJadi As String = "", sqlSMJadi As String = ""

            Dim drX() As DataRow, dtX As New DataTable
            Dim saldoawal As Double = 0, masuk As Double = 0, keluar As Double = 0, saldoakhir As Double = 0
            strValue = New StringBuilder

            ' Hitung Jenis mutasi dan jml barang
            dtBarang.Columns.Add("jm", Type.GetType("System.Double"))
            dtBarang.Columns.Add("jk", Type.GetType("System.Double"))
            For j = 0 To dtBarang.Rows.Count - 1
                If dtBarang.Rows(j)("jenismutasi") = 1 Then
                    dtBarang.Rows(j)("jm") = dtBarang.Rows(j)("jmlbarang")
                Else
                    dtBarang.Rows(j)("jk") = dtBarang.Rows(j)("jmlbarang")
                End If
            Next

            ' Join dg tabel m1_item kode barang
            Dim ods As New DataSet
            ods.Tables.Add(dtBaranggrp)
            ods.Tables.Add(m1_item)
            ods.Relations.Add("TheRelation", m1_item.Columns("bid"), dtBaranggrp.Columns("idbarang"), False)
            dtBaranggrp.Columns.Add("kodebarang", GetType(System.String), "Parent.bkode")
            ods.Relations.RemoveAt(0)
            ods.Tables.RemoveAt(1)
            ods.Tables.Add(dtwarehouse)
            ods.Relations.Add("TheRelation2", dtwarehouse.Columns("wkode"), dtBaranggrp.Columns("gudang"), False)
            dtBaranggrp.Columns.Add("namagudang", GetType(System.String), "Parent.wnama")

            'PERULANGAN PROSES MUTASI STOK
            For n = 0 To dtBaranggrp.Rows.Count - 1
                Dim dr1 = dtBaranggrp.Rows(n)
                saldoawal = 0 : masuk = 0 : keluar = 0 : saldoakhir = 0

                'SET IDBARANG, GUDANG
                idbarang = dr1("idbarang") : gudang = dr1("gudang")

                ' Hitung saldoawal
                drX = dtBarang.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl < '" & FixQuotes(tglAwal) & "'", "")
                If drX.Length > 0 Then
                    dtX = drX.CopyToDataTable()
                    Try : masuk = dtX.Compute("SUM(jm)", "") : Catch ex As Exception : End Try
                    Try : keluar = dtX.Compute("SUM(jk)", "") : Catch ex As Exception : End Try
                    saldoawal = masuk - keluar
                End If

                ' Hitung masuk dan keluar
                drX = dtBarang.Select("idbarang = " + idbarang.ToString + " AND gudang = '" & gudang & "' AND tgl >= '" & FixQuotes(tglAwal) & "' AND tgl <= '" & FixQuotes(tglAkhir) & "'", "")

                If drX.Length > 0 Then
                    dtX = drX.CopyToDataTable()
                    Try : masuk = dtX.Compute("SUM(jm)", "") : Catch ex As Exception : End Try
                    Try : keluar = dtX.Compute("SUM(jk)", "") : Catch ex As Exception : End Try

                End If

                saldoakhir = saldoawal + masuk - keluar

                strValue.Append("('" & FixQuotes(idLogin) & "', '" & n.ToString & "', '0', '" & FixQuotes(dr1("cabang")) & "', '', '" & FixQuotes(dr1("lokasi")) & "', '', '" & FixQuotes(dr1("gudang")) & "', '" & _
                                FixQuotes(dr1("namagudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '', '" & FixQuotes(dr1("divisi")) & "', '', '" & FixQuotes(dr1("subdivisi")) & "', '', '" & FixQuotes(dr1("proyek")) & "', '', '" & _
                                FixQuotes(dr1("idbarang")) & "', '" & FixQuotes(dr1("kodebarang")) & "', '', '" & FixQuotes(dr1("namabarang")) & "', '" & _
                                FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("notransaksi")) & "', '" & _
                                FixQuotes(dr1("kontak")) & "', '', '', '" & FixQuotes(dr1("uraian")) & "', '" & _
                                FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("catatandetail")) & "', '" & FixQuotes(jmlmasuk.ToString()) & "', '" & FixQuotes(jmlkeluar.ToString()) & "', '" & _
                                FixQuotes(saldoawal.ToString()) & "', '" & FixQuotes(AsFormatTanggal(dr1("inputtgl"), "yyyy-MM-dd")) & "', '" & FixQuotes(idMsmq) & "', '" & _
                                FixQuotes(userid.ToString()) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & _
                                FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customint1")) & "', '" & FixQuotes(dr1("customint2")) & "', '" & _
                                FixQuotes(dr1("customint3")) & "', '" & FixQuotes(dr1("customint4")) & "', '" & FixQuotes(dr1("customint5")) & "', '" & FixQuotes(saldoawal.ToString) & "', '" & _
                                FixQuotes(masuk.ToString) & "', '" & FixQuotes(keluar.ToString) & "', '" & FixQuotes(saldoakhir.ToString) & "', '" & FixQuotes(dr1("customdbl5")) & "', '" & _
                                FixQuotes(AsFormatTanggal(dr1("customdate1"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"), "yyyy-MM-dd")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"), "yyyy-MM-dd")) & "', '" & _
                                FixQuotes(AsFormatTanggal(dr1("customdate5"), "yyyy-MM-dd")) & "'),")

                'UPDATE PROGRESS ====================================
                'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
                progressPersen = IIf(stepKe = dtBarang.Rows.Count, Prosentase, (Math.Round(Prosentase / dtBarang.Rows.Count, 2)) * stepKe)

                'UPDATE PROGRESS REPORT M0_MSMQ
                pesan = String.Concat(dr1("kodebarang"), dr1("namabarang"), " - Gudang (", dr1("namagudang"), ")", "\n Hitung Barang per gudang dari ", dtBaranggrp.Rows.Count.ToString, " - ", (n + 1).ToString)
                pesan = FixQuotes(pesan)
                sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "', pesan = '" & pesan & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
                If AsEksekusiSQL(sql, strCon) = False Then
                    result(2) = "Failed updating progress Data Stock Report '" & FixQuotes(gudang) & "' Warehouse : " & IIf(orderBy = "bkode", dr1("bkode"), dr1("bnama")) : GoTo selesai
                End If
                'END OF UPDATE PROGRESS =============================

            Next

            If AsEksekusiSQL("INSERT INTO M2r_Mutasi_Stok_Detail VALUES  " & strValue.ToString.Substring(0, strValue.Length - 1) & ";", strCon) = False Then
                result(2) = "Failed proccessing Starting Balance Stock Report '" : GoTo selesai
            End If
        Else

dataKosong:

            'UPDATE PROGRESS ====================================
            'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML BARANG MAKA PROGRESS = PROSENTASE
            progressPersen = 100

            'UPDATE PROGRESS REPORT M0_MSMQ
            sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
            If AsEksekusiSQL(sql, strCon) = False Then
                result(2) = "Failed updating progress Data Stock Report 'Nothing' Warehouse : " & IIf(orderBy = "bkode", "Nothing", "Nothing") : GoTo selesai
            End If
            'END OF UPDATE PROGRESS =============================
        End If
        'END OF PROSES SELECT DATA PERSEDIAAN ---------------------------------------

        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If
        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    Public Function M0_ARVoucher_PerSalesman(ByVal param As String) As String
        '//LAPORAN VOUCHER PIUTANG (ACCOUNT RECEIVABLES)
        '//SUMBER SI(SALES INVOICE), SR(SALES RETUR)

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tglAwal(0) As String, tglAkhir(1) As String, area(2) As String, customerCategory(3) As String, 
        'customerAwal(4) As String, customerAkhir(5) As String, salesmanCategory(6) As String, salesman(7) As String, matauang(8) As String,
        'orderBy(9) As String, statusJT(10) As Integer, tglJTAwal(11) As String, tglJTAkhir(12) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tglAwal, tglAkhir, area, customerCategory, 
        'customerAwal, customerAkhir, salesmanCategory, salesman, matauang,
        'orderBy, statusJT, tglJTAwal, tglJTAkhir

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tglAwal As String = "", tglAkhir As String = "", tglJTAwal As String = "", tglJTAkhir As String = ""
        Dim area As String = "", customerCat As String = "", customerAwal As String = "", customerAkhir As String = ""
        Dim salesmanCat As String = "", salesman As String = "", matauang As String = "", orderBy As String = "", statusJT As Integer = 0

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
        'validKey = ClsValidKey.ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'SET IDLOGIN = WEBSITE ACCESS KEY
        idLogin = paramSplit(0)

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 16) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ModuleId(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "ModuleId required numeric." : GoTo selesai
        Else
            ModuleId = dataUtama(0)
        End If

        'MenuName(1) As String
        MenuName = dataUtama(1)

        'Query(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "Query can't be empty" : GoTo selesai
        Else
            Query = dataUtama(2)
        End If

        'FileFormat(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "FileFormat required numeric." : GoTo selesai
        Else
            FileFormat = dataUtama(3)
        End If

        'Param1(4) As String
        Param1 = dataUtama(4)

        'Param2(5) As String
        Param2 = dataUtama(5)

        'Param3(6) As String
        Param3 = dataUtama(6)

        'Param4(7) As String
        Param4 = dataUtama(7)

        'Param5(8) As String
        Param5 = dataUtama(8)

        'namaPerusahaan(9) As String 
        namaPerusahaan = dataUtama(9)

        'namaReport(10) As String
        namaReport = dataUtama(10)

        'rp2(11) As String
        rp2 = dataUtama(11)

        'rp3(12) As String
        rp3 = dataUtama(12)

        'rp4(13) As String
        rp4 = dataUtama(13)

        'rp5(14) As String
        rp5 = dataUtama(14)

        'idMsmq(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "idMsmq can't be empty" : GoTo selesai
        Else
            idMsmq = dataUtama(15)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DAN SET DATA DETAIL ======================================================
        dataDetail = dataSplit(1).Split(sptField)    'SPLIT PARAMETER DATA DETAIL

        'CEK ARRAY DATA DETAIL 
        If (dataDetail.Length <> 13) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tglAwal(0) As String
        If Len(dataDetail(0)) > 0 Then
            If (IsDate(dataDetail(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataDetail(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataDetail(1)) > 0 Then
            If (IsDate(dataDetail(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataDetail(1))
            End If
        Else
            tglAkhir = AsFormatTanggal("2100-12-31")
        End If

        'area(2) As String
        area = dataDetail(2)

        'customerCategory(3) As String
        customerCat = dataDetail(3)

        'customerAwal(4) As String
        customerAwal = dataDetail(4)

        'customerAkhir(5) As String
        customerAkhir = dataDetail(5)

        'salesmanCategory(6) As String
        salesmanCat = dataDetail(6)

        'salesman(7) As String
        salesman = dataDetail(7)

        'matauang(8) As String
        matauang = dataDetail(8)

        'orderBy(9) As String
        If Len(dataDetail(9)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataDetail(9).ToString <> "kkode" And dataDetail(9).ToString <> "knama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataDetail(9)
        End If

        'statusJT(10) As Integer
        If (IsNumeric(dataDetail(10)) = False) Then
            result(2) = "statusJT required numeric." : GoTo selesai
        Else
            statusJT = dataDetail(10)
        End If

        'tglJTAwal(11) As String
        If Len(dataDetail(11)) > 0 Then
            If (IsDate(dataDetail(11)) = False) Then
                result(2) = "tglJTAwal required date." : GoTo selesai
            Else
                tglJTAwal = AsFormatTanggal(dataDetail(11))
            End If
        Else
            tglJTAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglJTAkhir(12) As String
        If Len(dataDetail(12)) > 0 Then
            If (IsDate(dataDetail(12)) = False) Then
                result(2) = "tglJTAkhir required date." : GoTo selesai
            Else
                tglJTAkhir = AsFormatTanggal(dataDetail(12))
            End If
        Else
            tglJTAkhir = AsFormatTanggal("2100-12-31")
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(strCon)
        Con1.Open()

        'HAPUS IDLOGIN PADA M2r_Ar_Voucher ------------------------------------------
        'HAPUS DATA BERDASARKAN IDMSMQ DI M2r_Ar_Voucher
        sql = "DELETE FROM m2r_ar_Voucher WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql, strCon) = False Then
            result(2) = "Failed replace AR Voucher data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Ar_Voucher -----------------------------------


        'AMBIL DATA CUSTOMER SESUAI FILTER ---------------------------------------
        'KONTAK KATEGORI CUSTOMER, DAN AKUN SESUAI SETTING Voucher PIUTANG
        ''sql = "SELECT c1.kid, c1.kkode, c1.knama FROM m1_contact c1 JOIN m2_transaction_journal t ON c1.kid = t.tkontak LEFT JOIN m1_contact c2 ON c1.ksalesman = c2.kid "
        'sql = "SELECT c1.kid, c1.kkode, c1.knama FROM m1_contact c1 JOIN m2_transaction_journal t ON c1.kid = t.tkontak "
        'sql += " WHERE (c1.kkategori = 'C') AND t.tstatus IN(2,3,4,7) AND (t.tsumber = 'SI' OR t.tsumber = 'SR') "
        ''TAMBAHKAN FILTER AREA
        'If Len(area) > 0 Then sql += " AND (c1.karea = '" & area & "') "
        ''TAMBAHKAN FILTER KATEGORI CUSTOMER
        'If Len(customerCat) > 0 Then sql += " AND (c1.kkategoricustomer = '" & customerCat & "') "
        ''TAMBAHKAN FILTER KODE/NAMA KONTAK
        'If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
        '    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
        '    sql += " AND (c1." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
        'ElseIf Len(customerAwal) > 0 Then
        '    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
        '    sql += " AND (c1." & orderBy & " >= '" & customerAwal & "') "
        'ElseIf Len(customerAkhir) > 0 Then
        '    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
        '    sql += " AND (c1." & orderBy & " <= '" & customerAkhir & "') "
        'End If
        ' ''TAMBAHKAN FILTER KATEGORI SALESMAN
        ''If Len(salesmanCat) > 0 Then sql += " AND (c2.kkategorisalesman = '" & salesmanCat & "') "
        ' ''TAMBAHKAN FILTER SALESMAN
        ''If Len(salesman) > 0 Then sql += " AND (c2." & orderBy & " = '" & salesman & "') "
        ''TAMBAHKAN GROUPING
        'sql += " GROUP BY c1.kid "
        ''TAMBAHKAN SORTING
        'sql += " ORDER BY c1." & orderBy & ""

        sql = "SELECT c1.kid, c1.kkode, c1.knama FROM m1_contact c1 "
        sql += " WHERE (c1.kkategori = 'M') "
        'TAMBAHKAN FILTER AREA
        If Len(area) > 0 Then sql += " AND (c1.karea = '" & area & "') "
        ''TAMBAHKAN FILTER KATEGORI CUSTOMER
        'If Len(customerCat) > 0 Then sql += " AND (c1.kkategoricustomer = '" & customerCat & "') "
        'TAMBAHKAN FILTER KODE/NAMA KONTAK
        'If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
        '    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
        '    sql += " AND (c1." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
        'ElseIf Len(customerAwal) > 0 Then
        '    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
        '    sql += " AND (c1." & orderBy & " >= '" & customerAwal & "') "
        'ElseIf Len(customerAkhir) > 0 Then
        '    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
        '    sql += " AND (c1." & orderBy & " <= '" & customerAkhir & "') "
        'End If
        'TAMBAHKAN FILTER KATEGORI SALESMAN
        If Len(salesmanCat) > 0 Then sql += " AND (c1.kkategorisalesman = '" & salesmanCat & "') "
        'TAMBAHKAN FILTER SALESMAN
        If Len(salesman) > 0 Then sql += " AND (c1.kkode = '" & salesman & "') "
        ''TAMBAHKAN GROUPING
        'sql += " GROUP BY c1.kid "
        'TAMBAHKAN SORTING
        sql += " ORDER BY c1." & orderBy & ""
        '-- DI BAWAH SINI DI KASIH 
        'result(2) = sql : GoTo selesai


        Dim dtCustomer As DataTable = AsDataTableAmbilDariDB(sql, strCon)
        'END OF AMBIL DATA CUSTOMER SESUAI FILTER --------------------------------


        'PERHITUNGAN Voucher PER KONTAK --------------------------------------------
        If dtCustomer.Rows.Count > 0 Then

            'AMBIL MATAUANG FUNGSIONAL DARI SETTING
            Dim dtMatauang As DataTable = AsDataTableAmbilDariDB("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", strCon)
            Dim uangFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
            If uangFungsional = "Not found" Then
                result(2) = "Setting Functional Currency not found." : GoTo selesai
            End If
            Dim kursFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
            If kursFungsional = "Not found" Then
                result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
            End If

            Dim dtSMutasi As New DataTable, dtSSubtotal As New DataTable, dtSAkhir As New DataTable
            Dim sqlFungsionalSI As String = "", sqlValasSI As String = ""
            Dim sqlFungsionalSR As String = "", sqlValasSR As String = ""
            Dim sqlJadi As String = "", sqlSubtotal As String = ""
            Dim urut As Double = 1, urutSAkhir As Double = 1, idkontak As String = "", kodekontak As String = "", namakontak As String = ""

            sql = "DROP TABLE IF EXISTS m5_pv_tmp_" + idLogin + ";CREATE TABLE m5_pv_tmp" + idLogin + " SELECT pvid FROM m5_pv WHERE pvstatus IN(2,3,4,7) AND pv.pvtgl <= '" & FixQuotes(tglAkhir) & "';ALTER TABLE m5_pv_tmp_" + idLogin + " ADD PRIMARY KEY (pvid);"
            sql &= "DROP TABLE IF EXISTS m5_pv_detail_tmp_" + idLogin + ";CREATE TABLE m5_pv_detail_tmp" + idLogin + " SELECT idpv, sumber, jmlbayar, jmlbayarvalas, idtransaksi FROM m5_pv_tmp_" + idLogin + " JOIN m5_pv_detail ON pvid = idpv;"
            sql &= "DROP TABLE IF EXISTS m5_si_tmp_" + idLogin + ";CREATE TABLE m5_si_tmp_" + idLogin + " SELECTSELECT si.siid, si.sitgl, si.sisumber,si.sinotransaksi,si.sicustomer,si.siuraian,si.sicatatan,si.sitotaltransaksi,si.sikurs, si.simatauang, si.sitgljatuhtempo, si.sistatuslunas,si.sitgllunas, si.siinputtgl FROM m5_si si JOIN m1_contact c ON si.sicustomer = c.kid WHERE si.sistatus IN(2,3,4,7);ALTER TABLE m5_si_tmp_" + idLogin + " ADD PRIMARY KEY (siid);"
            sql &= "DROP TABLE IF EXISTS m1_contact_tmp_" + idLogin + ";CREATE TABLE m1_contact_tmp_" + idLogin + " SELECT kid, c.kkode, c.knama, c.k1alamat1, c.k1alamat2, c.k1alamat3, c.k1alamat4, c.k1alamat5, c.k1notelp1, c.k1notelp2 FROM m1_contact c;ALTER TABLE m1_contact_tmp_" + idLogin + " ADD PRIMARY KEY (kid);"
            If AsEksekusiSQL(sql, strCon) = False Then
                result(2) = "Failed proccessing Tempory Table " : GoTo selesai
            End If

            Dim snilai As String = ""
            Dim dtsetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'akun' AND skode = 'PiutangUsaha'", strCon)
            If dtsetting.Rows.Count > 0 Then
                snilai = dtsetting.Rows(0)(0)
            End If

            'PERULANGAN HITUNG Voucher PER KONTAK ++++++++++++++++++++++++++
            For Each dr As DataRow In dtCustomer.Rows
                'SET IDKONTAK
                idkontak = dr("kid")
                kodekontak = dr("kkode")
                namakontak = dr("knama")

                'RESET QUERY INSERT, SET STEPKE
                strValue.Clear() : stepKe = stepKe + 1

                'SALDO MUTASI =======================================
                'QUERY SALDO MUTASI MATAUANG FUNGSIONAL
                'SI FUNGSIONAL
                sqlFungsionalSI = "  SELECT t.arid, t.artgl, t.arsumber, t.arnotransaksi, t.arkontak, t.arkontakkode, t.arkontaknama, t.aralamat1, t.aralamat2, t.aralamat3, t.aralamat4, t.aralamat5, t.arnotelp1, t.arnotelp2, t.arnorek, t.armatauang, t.arkurs, t.aruraian, t.arcatatan, t.artotal, SUM(t.arbayar) as arbayar, t.artotal - SUM(t.arbayar) as arsisa, t.artgljatuhtempo, t.arstatuslunas, t.artgllunas, t.arinputtgl, t.arisfungsional, t.arissaldoakhir FROM ( "

                sqlFungsionalSI &= " ( "
                sqlFungsionalSI &= " SELECT si.siid as arid, si.sitgl as artgl, si.sisumber as arsumber, si.sinotransaksi as arnotransaksi, si.sicustomer as arkontak, c.kkode as arkontakkode, c.knama as arkontaknama, c.k1alamat1 as aralamat1, c.k1alamat2 as aralamat2, c.k1alamat3 as aralamat3, c.k1alamat4 as aralamat4, c.k1alamat5 as aralamat5, c.k1notelp1 as arnotelp1, c.k1notelp2 as arnotelp2, " + snilai + " as arnorek, '" & FixQuotes(uangFungsional) & "' as armatauang, '" & FixDouble(kursFungsional) & "' as arkurs, si.siuraian as aruraian, si.sicatatan as arcatatan, (si.sitotaltransaksi * si.sikurs) as artotal, SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE IFNULL((pvd.jmlbayar),0) END) ELSE (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE (IFNULL((pvd.jmlbayarvalas),0) * si.sikurs) END) END)) as arbayar, (si.sitotaltransaksi * si.sikurs) - SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE IFNULL((pvd.jmlbayar),0) END) ELSE (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE (IFNULL((pvd.jmlbayarvalas),0) * si.sikurs) END) END)) as arsisa, si.sitgljatuhtempo as artgljatuhtempo, si.sistatuslunas as arstatuslunas, si.sitgllunas as artgllunas, si.siinputtgl as arinputtgl, 1 as arisfungsional, 0 as arissaldoakhir "
                sqlFungsionalSI &= " FROM m5_si_tmp_" + idLogin + " si "
                sqlFungsionalSI &= " LEFT JOIN m1_contact_tmp_" + idLogin + " c ON si.sicustomer = c.kid "
                'sqlFungsionalSI &= " LEFT JOIN m1_contact c2 ON si.sibagianpenjualan = c2.kid "
                'sqlFungsionalSI &= " LEFT JOIN m0_setting s ON smodule = 0 AND sgrup = 'akun' AND skode = 'PiutangUsaha' "
                sqlFungsionalSI &= " LEFT JOIN m5_pv_detail_tmp_" + idLogin + " pvd ON si.sisumber = pvd.sumber AND si.siid = pvd.idtransaksi "
                sqlFungsionalSI &= " LEFT JOIN m5_pv pv_tmp_" + idLogin + " ON pvd.idpv = pv.pvid"
                sqlFungsionalSI &= " WHERE si.sicarabayar = 1 "
                sqlFungsionalSI &= " AND si.sibagianpenjualan = '" & FixDouble(idkontak) & "' "
                sqlFungsionalSI &= " AND si.sitgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
                sqlFungsionalSI &= " AND (si.sistatuslunas <> 2 OR (si.sistatuslunas = 2 AND si.sitgllunas > '" & FixQuotes(tglAkhir) & "')) "
                sqlFungsionalSI &= " AND (CASE LENGTH('" & FixQuotes(matauang) & "') WHEN 0 THEN si.simatauang LIKE '%' ELSE si.simatauang = '" & FixQuotes(matauang) & "' END) "
                'statusJT, 0 = belum, 1 = sudah, 2 = semua
                sqlFungsionalSI &= " AND (CASE '" & FixDouble(statusJT) & "' WHEN 0 THEN si.sitgljatuhtempo > '" & FixQuotes(tglAkhir) & "' WHEN 1 THEN si.sitgljatuhtempo <= '" & FixQuotes(tglAkhir) & "' ELSE si.sitgljatuhtempo LIKE '%' END) "
                sqlFungsionalSI &= " AND si.sitgljatuhtempo BETWEEN '" & FixQuotes(tglJTAwal) & "' AND '" & FixQuotes(tglJTAkhir) & "' "

                'TAMBAHKAN FILTER KATEGORI CUSTOMER
                If Len(customerCat) > 0 Then sqlFungsionalSI += " AND (c.kkategoricustomer = '" & customerCat & "') "
                'TAMBAHKAN FILTER KODE/NAMA KONTAK
                If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sqlFungsionalSI += " AND (c." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
                ElseIf Len(customerAwal) > 0 Then
                    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
                    sqlFungsionalSI += " AND (c." & orderBy & " >= '" & customerAwal & "') "
                ElseIf Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
                    sqlFungsionalSI += " AND (c." & orderBy & " <= '" & customerAkhir & "') "
                End If

                sqlFungsionalSI &= " GROUP BY si.siid "
                sqlFungsionalSI &= " ORDER BY c.kkode, si.sitgl, si.siinputtgl, si.siid "
                sqlFungsionalSI &= " ) "

                sqlFungsionalSI &= " UNION ALL "

                sqlFungsionalSI &= " ("
                sqlFungsionalSI &= " SELECT si.siid as arid, si.sitgl as artgl, si.sisumber as arsumber, si.sinotransaksi as arnotransaksi, si.sicustomer as arkontak, c.kkode as arkontakkode, c.knama as arkontaknama, c.k1alamat1 as aralamat1, c.k1alamat2 as aralamat2, c.k1alamat3 as aralamat3, c.k1alamat4 as aralamat4, c.k1alamat5 as aralamat5, c.k1notelp1 as arnotelp1, c.k1notelp2 as arnotelp2, " + snilai + "  as arnorek, '" & FixQuotes(uangFungsional) & "' as armatauang, '" & FixDouble(kursFungsional) & "' as arkurs, si.siuraian as aruraian, si.sicatatan as arcatatan, (si.sitotaltransaksi * si.sikurs) as artotal, SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE IFNULL((sr.srtotaltransaksi),0) END) ELSE (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE (IFNULL((sr.srtotaltransaksi),0) * si.sikurs) END) END)) as arbayar, (si.sitotaltransaksi * si.sikurs) - SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE IFNULL((sr.srtotaltransaksi),0) END) ELSE (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE (IFNULL((sr.srtotaltransaksi),0) * si.sikurs) END) END)) as arsisa, si.sitgljatuhtempo as artgljatuhtempo, si.sistatuslunas as arstatuslunas, si.sitgllunas as artgllunas, si.siinputtgl as arinputtgl, 1 as arisfungsional, 0 as arissaldoakhir "
                sqlFungsionalSI &= " FROM m5_si_tmp_" + idLogin + " si "
                sqlFungsionalSI &= " LEFT JOIN m1_contact_tmp_" + idLogin + " c ON si.sicustomer = c.kid "
                'sqlFungsionalSI &= " LEFT JOIN m1_contact c2 ON si.sibagianpenjualan = c2.kid "
                sqlFungsionalSI &= " LEFT JOIN m5_sr sr ON si.siid = sr.sridsi AND sr.srstatus IN(2,3,4,7) AND sr.srjenis = 1 AND sr.srtgl <= '" & FixQuotes(tglAkhir) & "' "
                sqlFungsionalSI &= " WHERE si.sicarabayar = 1 "
                sqlFungsionalSI &= " AND si.sibagianpenjualan = '" & FixDouble(idkontak) & "' "
                sqlFungsionalSI &= " AND si.sitgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
                sqlFungsionalSI &= " AND (si.sistatuslunas <> 2 OR (si.sistatuslunas = 2 AND si.sitgllunas > '" & FixQuotes(tglAkhir) & "')) "
                sqlFungsionalSI &= " AND (CASE LENGTH('" & FixQuotes(matauang) & "') WHEN 0 THEN si.simatauang LIKE '%' ELSE si.simatauang = '" & FixQuotes(matauang) & "' END) "
                'statusJT, 0 = belum, 1 = sudah, 2 = semua
                sqlFungsionalSI &= " AND (CASE '" & FixDouble(statusJT) & "' WHEN 0 THEN si.sitgljatuhtempo > '" & FixQuotes(tglAkhir) & "' WHEN 1 THEN si.sitgljatuhtempo <= '" & FixQuotes(tglAkhir) & "' ELSE si.sitgljatuhtempo LIKE '%' END) "
                sqlFungsionalSI &= " AND si.sitgljatuhtempo BETWEEN '" & FixQuotes(tglJTAwal) & "' AND '" & FixQuotes(tglJTAkhir) & "' "

                'TAMBAHKAN FILTER KATEGORI CUSTOMER
                If Len(customerCat) > 0 Then sqlFungsionalSI += " AND (c.kkategoricustomer = '" & customerCat & "') "
                'TAMBAHKAN FILTER KODE/NAMA KONTAK
                If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sqlFungsionalSI += " AND (c." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
                ElseIf Len(customerAwal) > 0 Then
                    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
                    sqlFungsionalSI += " AND (c." & orderBy & " >= '" & customerAwal & "') "
                ElseIf Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
                    sqlFungsionalSI += " AND (c." & orderBy & " <= '" & customerAkhir & "') "
                End If

                sqlFungsionalSI &= " GROUP BY si.siid "
                sqlFungsionalSI &= " ORDER BY c.kkode, si.sitgl, si.siinputtgl, si.siid "
                sqlFungsionalSI &= " ) "

                sqlFungsionalSI &= " ) as t"
                sqlFungsionalSI &= " GROUP BY t.arid "
                sqlFungsionalSI &= " ORDER BY t.arkontakkode, t.artgl, t.arinputtgl, t.arid "


                'SI VALAS
                sqlValasSI = "  SELECT t.arid, t.artgl, t.arsumber, t.arnotransaksi, t.arkontak, t.arkontakkode, t.arkontaknama, t.aralamat1, t.aralamat2, t.aralamat3, t.aralamat4, t.aralamat5, t.arnotelp1, t.arnotelp2, t.arnorek, t.armatauang, t.arkurs, t.aruraian, t.arcatatan, t.artotal, SUM(t.arbayar) as arbayar, t.artotal - SUM(t.arbayar) as arsisa, t.artgljatuhtempo, t.arstatuslunas, t.artgllunas, t.arinputtgl, t.arisfungsional, t.arissaldoakhir FROM ( "

                sqlValasSI &= " ( "
                sqlValasSI &= "  SELECT si.siid as arid, si.sitgl as artgl, si.sisumber as arsumber, si.sinotransaksi as arnotransaksi, si.sicustomer as arkontak, c.kkode as arkontakkode, c.knama as arkontaknama, c.k1alamat1 as aralamat1, c.k1alamat2 as aralamat2, c.k1alamat3 as aralamat3, c.k1alamat4 as aralamat4, c.k1alamat5 as aralamat5, c.k1notelp1 as arnotelp1, c.k1notelp2 as arnotelp2, " + snilai + "  as arnorek, si.simatauang as armatauang, si.sikurs as arkurs, si.siuraian as aruraian, si.sicatatan as arcatatan, si.sitotaltransaksi as artotal, SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE IFNULL((pvd.jmlbayar),0) END) ELSE (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE IFNULL((pvd.jmlbayarvalas),0) END) END)) as arbayar, (si.sitotaltransaksi) - SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE IFNULL((pvd.jmlbayar),0) END) ELSE (CASE IFNULL(pv.pvid,0) WHEN 0 THEN 0 ELSE IFNULL((pvd.jmlbayarvalas),0) END) END)) as arsisa, si.sitgljatuhtempo as artgljatuhtempo, si.sistatuslunas as arstatuslunas, si.sitgllunas as artgllunas, si.siinputtgl as arinputtgl, 0 as arisfungsional, 0 as arissaldoakhir "
                sqlValasSI &= " FROM m5_si si "
                sqlValasSI &= " LEFT JOIN m1_contact_tmp_" + idLogin + " c ON si.sicustomer = c.kid "
                'sqlValasSI &= " LEFT JOIN m1_contact c2 ON si.sibagianpenjualan = c2.kid "
                'sqlValasSI &= " LEFT JOIN m0_setting s ON smodule = 0 AND sgrup = 'akun' AND skode = 'PiutangUsaha' "
                sqlValasSI &= " LEFT JOIN m5_pv_detail_tmp_" + idLogin + " pvd ON si.sisumber = pvd.sumber AND si.siid = pvd.idtransaksi "
                sqlValasSI &= " LEFT JOIN m5_pv_tmp_" + idLogin + " pv ON pvd.idpv = pv.pvid "
                sqlValasSI &= " WHERE si.sicarabayar = 1 "
                sqlValasSI &= " AND si.sibagianpenjualan = '" & FixDouble(idkontak) & "' "
                sqlValasSI &= " AND si.sitgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
                sqlValasSI &= " AND si.simatauang <> '" & FixQuotes(uangFungsional) & "' "
                sqlValasSI &= " AND (si.sistatuslunas <> 2 OR (si.sistatuslunas = 2 AND si.sitgllunas > '" & FixQuotes(tglAkhir) & "')) "
                sqlValasSI &= " AND (CASE LENGTH('" & FixQuotes(matauang) & "') WHEN 0 THEN si.simatauang LIKE '%' ELSE si.simatauang = '" & FixQuotes(matauang) & "' END) "
                'statusJT, 0 = belum, 1 = sudah, 2 = semua
                sqlValasSI &= " AND (CASE '" & FixDouble(statusJT) & "' WHEN 0 THEN si.sitgljatuhtempo > '" & FixQuotes(tglAkhir) & "' WHEN 1 THEN si.sitgljatuhtempo <= '" & FixQuotes(tglAkhir) & "' ELSE si.sitgljatuhtempo LIKE '%' END) "
                sqlValasSI &= " AND si.sitgljatuhtempo BETWEEN '" & FixQuotes(tglJTAwal) & "' AND '" & FixQuotes(tglJTAkhir) & "' "

                'TAMBAHKAN FILTER KATEGORI CUSTOMER
                If Len(customerCat) > 0 Then sqlValasSI += " AND (c.kkategoricustomer = '" & customerCat & "') "
                'TAMBAHKAN FILTER KODE/NAMA KONTAK
                If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sqlValasSI += " AND (c." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
                ElseIf Len(customerAwal) > 0 Then
                    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
                    sqlValasSI += " AND (c." & orderBy & " >= '" & customerAwal & "') "
                ElseIf Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
                    sqlValasSI += " AND (c." & orderBy & " <= '" & customerAkhir & "') "
                End If

                sqlValasSI &= " GROUP BY si.siid "
                sqlValasSI &= " ORDER BY c.kkode, si.sitgl, si.siinputtgl, si.siid "
                sqlValasSI &= " ) "

                sqlValasSI &= " UNION ALL "

                sqlValasSI &= " ( "
                sqlValasSI &= "  SELECT si.siid as arid, si.sitgl as artgl, si.sisumber as arsumber, si.sinotransaksi as arnotransaksi, si.sicustomer as arkontak, c.kkode as arkontakkode, c.knama as arkontaknama, c.k1alamat1 as aralamat1, c.k1alamat2 as aralamat2, c.k1alamat3 as aralamat3, c.k1alamat4 as aralamat4, c.k1alamat5 as aralamat5, c.k1notelp1 as arnotelp1, c.k1notelp2 as arnotelp2, " + snilai + "  as arnorek, si.simatauang as armatauang, si.sikurs as arkurs, si.siuraian as aruraian, si.sicatatan as arcatatan, si.sitotaltransaksi as artotal, SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE IFNULL((sr.srtotaltransaksi),0) END) ELSE (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE IFNULL((sr.srtotaltransaksi),0) END) END)) as arbayar, (si.sitotaltransaksi) - SUM((CASE si.simatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE IFNULL((sr.srtotaltransaksi),0) END) ELSE (CASE IFNULL(sr.srid,0) WHEN 0 THEN 0 ELSE IFNULL((sr.srtotaltransaksi),0) END) END)) as arsisa, si.sitgljatuhtempo as artgljatuhtempo, si.sistatuslunas as arstatuslunas, si.sitgllunas as artgllunas, si.siinputtgl as arinputtgl, 0 as arisfungsional, 0 as arissaldoakhir "
                sqlValasSI &= " FROM m5_si si "
                sqlValasSI &= " LEFT JOIN m1_contact_tmp_" + idLogin + " c ON si.sicustomer = c.kid "
                'sqlValasSI &= " LEFT JOIN m1_contact c2 ON si.sibagianpenjualan = c2.kid "
                'sqlValasSI &= " LEFT JOIN m0_setting s ON smodule = 0 AND sgrup = 'akun' AND skode = 'PiutangUsaha' "
                sqlValasSI &= " LEFT JOIN m5_sr sr ON si.siid = sr.sridsi AND sr.srstatus IN(2,3,4,7) AND sr.srjenis = 1 AND sr.srtgl <= '" & FixQuotes(tglAkhir) & "' "
                sqlValasSI &= " WHERE si.sicarabayar = 1 "
                sqlValasSI &= " AND si.sibagianpenjualan = '" & FixDouble(idkontak) & "' "
                sqlValasSI &= " AND si.sitgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
                sqlValasSI &= " AND si.simatauang <> '" & FixQuotes(uangFungsional) & "' "
                sqlValasSI &= " AND (si.sistatuslunas <> 2 OR (si.sistatuslunas = 2 AND si.sitgllunas > '" & FixQuotes(tglAkhir) & "')) "
                sqlValasSI &= " AND (CASE LENGTH('" & FixQuotes(matauang) & "') WHEN 0 THEN si.simatauang LIKE '%' ELSE si.simatauang = '" & FixQuotes(matauang) & "' END) "
                'statusJT, 0 = belum, 1 = sudah, 2 = semua
                sqlValasSI &= " AND (CASE '" & FixDouble(statusJT) & "' WHEN 0 THEN si.sitgljatuhtempo > '" & FixQuotes(tglAkhir) & "' WHEN 1 THEN si.sitgljatuhtempo <= '" & FixQuotes(tglAkhir) & "' ELSE si.sitgljatuhtempo LIKE '%' END) "
                sqlValasSI &= " AND si.sitgljatuhtempo BETWEEN '" & FixQuotes(tglJTAwal) & "' AND '" & FixQuotes(tglJTAkhir) & "' "

                'TAMBAHKAN FILTER KATEGORI CUSTOMER
                If Len(customerCat) > 0 Then sqlValasSI += " AND (c.kkategoricustomer = '" & customerCat & "') "
                'TAMBAHKAN FILTER KODE/NAMA KONTAK
                If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sqlValasSI += " AND (c." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
                ElseIf Len(customerAwal) > 0 Then
                    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
                    sqlValasSI += " AND (c." & orderBy & " >= '" & customerAwal & "') "
                ElseIf Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
                    sqlValasSI += " AND (c." & orderBy & " <= '" & customerAkhir & "') "
                End If

                sqlValasSI &= " GROUP BY si.siid "
                sqlValasSI &= " ORDER BY c.kkode, si.sitgl, si.siinputtgl, si.siid "
                sqlValasSI &= " ) "

                sqlValasSI &= " ) as t"
                sqlValasSI &= " GROUP BY t.arid "
                sqlValasSI &= " ORDER BY t.arkontakkode, t.artgl, t.arinputtgl, t.arid "



                'SR FUNGSIONAL
                sqlFungsionalSR = " SELECT sr.srid as arid, sr.srtgl as artgl, sr.srsumber as arsumber, sr.srnotransaksi as arnotransaksi, sr.srcustomer as arkontak, c.kkode as arkontakkode, c.knama as arkontaknama, c.k1alamat1 as aralamat1, c.k1alamat2 as aralamat2, c.k1alamat3 as aralamat3, c.k1alamat4 as aralamat4, c.k1alamat5 as aralamat5, c.k1notelp1 as arnotelp1, c.k1notelp2 as arnotelp2, s.snilai as arnorek, '" & FixQuotes(uangFungsional) & "' as armatauang, '" & FixDouble(kursFungsional) & "' as arkurs, sr.sruraian as aruraian, sr.srcatatan as arcatatan, (sr.srtotaltransaksi * sr.srkurs * -1) as artotal, SUM((CASE sr.srmatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN IFNULL((pvd.jmlbayar * -1),0) ELSE IFNULL((pvd.jmlbayarvalas * -1),0) * sr.srkurs END)) as arbayar, (sr.srtotaltransaksi * sr.srkurs * -1) - SUM((CASE sr.srmatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN IFNULL((pvd.jmlbayar * -1),0) ELSE IFNULL((pvd.jmlbayarvalas * -1),0) * sr.srkurs END)) as arsisa, sr.srtgljatuhtempo as artgljatuhtempo, sr.srstatuslunas as arstatuslunas, sr.srtgllunas as artgllunas, sr.srinputtgl as arinputtgl, 1 as arisfungsional, 0 as arissaldoakhir "
                sqlFungsionalSR &= " FROM m5_pv_tmp_" + idLogin + " pv "
                sqlFungsionalSR &= " JOIN m5_pv_detail_tmp_" + idLogin + " pvd ON pv.pvid = pvd.idpv "
                sqlFungsionalSR &= " RIGHT JOIN m5_sr sr ON pvd.idtransaksi = sr.srid AND pvd.sumber = sr.srsumber "
                sqlFungsionalSR &= " JOIN m1_contact c ON sr.srcustomer = c.kid "
                'sqlFungsionalSR &= " JOIN m1_contact c2 ON sr.srbagianpenjualan = c2.kid "
                'sqlFungsionalSR &= " JOIN m0_setting s ON smodule = 0 AND sgrup = 'akun' AND skode = 'PiutangUsaha' "
                sqlFungsionalSR &= " WHERE sr.srstatus IN(2,3,4,7) AND sr.srjenis = 0 "
                sqlFungsionalSR &= " AND sr.srbagianpenjualan = '" & FixDouble(idkontak) & "' "
                sqlFungsionalSR &= " AND sr.srtgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
                sqlFungsionalSR &= " AND (sr.srstatuslunas <> 2 OR (sr.srstatuslunas = 2 AND sr.srtgllunas > '" & FixQuotes(tglAkhir) & "')) "
                sqlFungsionalSR &= " AND (CASE LENGTH('" & FixQuotes(matauang) & "') WHEN 0 THEN sr.srmatauang LIKE '%' ELSE sr.srmatauang = '" & FixQuotes(matauang) & "' END) "
                'statusJT, 0 = belum, 1 = sudah, 2 = semua
                sqlFungsionalSR &= " AND (CASE '" & FixDouble(statusJT) & "' WHEN 0 THEN sr.srtgljatuhtempo > '" & FixQuotes(tglAkhir) & "' WHEN 1 THEN sr.srtgljatuhtempo <= '" & FixQuotes(tglAkhir) & "' ELSE sr.srtgljatuhtempo LIKE '%' END) "
                sqlFungsionalSR &= " AND sr.srtgljatuhtempo BETWEEN '" & FixQuotes(tglJTAwal) & "' AND '" & FixQuotes(tglJTAkhir) & "' "

                'TAMBAHKAN FILTER KATEGORI CUSTOMER
                If Len(customerCat) > 0 Then sqlFungsionalSR += " AND (c.kkategoricustomer = '" & customerCat & "') "
                'TAMBAHKAN FILTER KODE/NAMA KONTAK
                If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sqlFungsionalSR += " AND (c." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
                ElseIf Len(customerAwal) > 0 Then
                    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
                    sqlFungsionalSR += " AND (c." & orderBy & " >= '" & customerAwal & "') "
                ElseIf Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
                    sqlFungsionalSR += " AND (c." & orderBy & " <= '" & customerAkhir & "') "
                End If

                sqlFungsionalSR &= " GROUP BY sr.srid "
                sqlFungsionalSR &= " ORDER BY c.kkode, sr.srtgl, sr.srinputtgl, sr.srid "

                'SR VALAS
                sqlValasSR = " SELECT sr.srid as arid, sr.srtgl as artgl, sr.srsumber as arsumber, sr.srnotransaksi as arnotransaksi, sr.srcustomer as arkontak, c.kkode as arkontakkode, c.knama as arkontaknama, c.k1alamat1 as aralamat1, c.k1alamat2 as aralamat2, c.k1alamat3 as aralamat3, c.k1alamat4 as aralamat4, c.k1alamat5 as aralamat5, c.k1notelp1 as arnotelp1, c.k1notelp2 as arnotelp2, s.snilai as arnorek, sr.srmatauang as armatauang, sr.srkurs as arkurs, sr.sruraian as aruraian, sr.srcatatan as arcatatan, (sr.srtotaltransaksi * -1) as artotal, SUM((CASE sr.srmatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN IFNULL((pvd.jmlbayar * -1),0) ELSE IFNULL((pvd.jmlbayarvalas * -1),0) END)) as arbayar, (sr.srtotaltransaksi * -1) - SUM((CASE sr.srmatauang WHEN '" & FixQuotes(uangFungsional) & "' THEN IFNULL((pvd.jmlbayar * -1),0) ELSE IFNULL((pvd.jmlbayarvalas * -1),0) END)) as arsisa, sr.srtgljatuhtempo as artgljatuhtempo, sr.srstatuslunas as arstatuslunas, sr.srtgllunas as artgllunas, sr.srinputtgl as arinputtgl, 0 as arisfungsional, 0 as arissaldoakhir "
                sqlValasSR &= " FROM m5_pv_tmp_" + idLogin + " pv "
                sqlValasSR &= " JOIN m5_pv_detail_tmp_" + idLogin + " pvd ON pv.pvid = pvd.idpv "
                sqlValasSR &= " RIGHT JOIN m5_sr sr ON pvd.idtransaksi = sr.srid AND pvd.sumber = sr.srsumber "
                sqlValasSR &= " JOIN m1_contact c ON sr.srcustomer = c.kid "
                'sqlValasSR &= " JOIN m1_contact c2 ON sr.srbagianpenjualan = c2.kid "
                'sqlValasSR &= " JOIN m0_setting s ON smodule = 0 AND sgrup = 'akun' AND skode = 'PiutangUsaha' "
                sqlValasSR &= " WHERE sr.srstatus IN(2,3,4,7) AND sr.srjenis = 0 "
                sqlValasSR &= " AND sr.srbagianpenjualan = '" & FixDouble(idkontak) & "' "
                sqlValasSR &= " AND sr.srtgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "' "
                sqlValasSR &= " AND sr.srmatauang <> '" & FixQuotes(uangFungsional) & "' "
                sqlValasSR &= " AND (sr.srstatuslunas <> 2 OR (sr.srstatuslunas = 2 AND sr.srtgllunas > '" & FixQuotes(tglAkhir) & "')) "
                sqlValasSR &= " AND (CASE LENGTH('" & FixQuotes(matauang) & "') WHEN 0 THEN sr.srmatauang LIKE '%' ELSE sr.srmatauang = '" & FixQuotes(matauang) & "' END) "
                'statusJT, 0 = belum, 1 = sudah, 2 = semua
                sqlValasSR &= " AND (CASE '" & FixDouble(statusJT) & "' WHEN 0 THEN sr.srtgljatuhtempo > '" & FixQuotes(tglAkhir) & "' WHEN 1 THEN sr.srtgljatuhtempo <= '" & FixQuotes(tglAkhir) & "' ELSE sr.srtgljatuhtempo LIKE '%' END) "
                sqlValasSR &= " AND sr.srtgljatuhtempo BETWEEN '" & FixQuotes(tglJTAwal) & "' AND '" & FixQuotes(tglJTAkhir) & "' "

                'TAMBAHKAN FILTER KATEGORI CUSTOMER
                If Len(customerCat) > 0 Then sqlValasSR += " AND (c.kkategoricustomer = '" & customerCat & "') "
                'TAMBAHKAN FILTER KODE/NAMA KONTAK
                If Len(customerAwal) > 0 And Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AWAL DAN AKHIR DIISI MAKA FILTER BETWEEN
                    sqlValasSR += " AND (c." & orderBy & " BETWEEN '" & customerAwal & "' AND '" & customerAkhir & "') "
                ElseIf Len(customerAwal) > 0 Then
                    'JIKA CUSTOMER AWAL DIISI MAKA FILTER KONTAK >= CUSTOMER AWAL
                    sqlValasSR += " AND (c." & orderBy & " >= '" & customerAwal & "') "
                ElseIf Len(customerAkhir) > 0 Then
                    'JIKA CUSTOMER AKHIR DIISI MAKA FILTER KONTAK <= CUSTOMER AWAL
                    sqlValasSR += " AND (c." & orderBy & " <= '" & customerAkhir & "') "
                End If

                sqlValasSR &= " GROUP BY sr.srid "
                sqlValasSR &= " ORDER BY c.kkode, sr.srtgl, sr.srinputtgl, sr.srid "

                'GABUNGKAN SQL
                sqlJadi = " SELECT * FROM ( "
                sqlJadi &= " (" & sqlFungsionalSI & ") "
                sqlJadi &= " UNION ALL "
                sqlJadi &= " (" & sqlValasSI & ") "
                sqlJadi &= " UNION ALL "
                sqlJadi &= " (" & sqlFungsionalSR & ") "
                sqlJadi &= " UNION ALL "
                sqlJadi &= " (" & sqlValasSR & ") "
                sqlJadi &= " ) as ar "
                sqlJadi &= " ORDER BY ar.arkontakkode, ar.artgl, ar.arinputtgl, ar.arsumber, ar.arid, ar.arisfungsional DESC, ar.armatauang "

                'AMBIL SALDO MUTASI
                dtSMutasi = AsDataTableAmbilDariDB(sqlJadi, strCon)
                If dtSMutasi.Rows.Count > 0 Then
                    'INSERT SALDO MUTASI KE TABEL PEMBANTU
                    'SORTING DATA TERLEBIH DAHULU
                    dtSMutasi = AsDataTableFilterSortDt(dtSMutasi, "", "arkontakkode, artgl, arinputtgl, arsumber, arid, arisfungsional DESC, armatauang")
                    For Each dr1 As DataRow In dtSMutasi.Rows
                        'BUAT QUERY INSERT KE TABEL PEMBANTU
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'MAPPING :         arnourut,                     idlogin,                           arid,                                             artgl,                              arsumber,                             arnotransaksi,                             arkontak,                             arkontakkode,                             arkontaknama,                             aralamat1,                             aralamat2,                             aralamat3,                             aralamat4,                             aralamat5,                             arnotelp1,                             arnotelp2,                             arnorek,                             armatauang,                             arkurs,                             aruraian,                             arcatatan,                             artotal,                             arbayar,                             arsisa,                                             artgljatuhtempo,                   arstatuslunas,                                           artgllunas,                                              arinputtgl,                                         arisfungsional,                          arissaldoakhir,                       idmsmq,                    aruserid,                      arcustomtext1,                   arcustomtext2,           arcustomtext3,           arcustomtext4,           arcustomtext5,      arcustomint1,arcustomint2,arcustomint3,arcustomint4,arcustomint5, arcustomdbl1,           arcustomdbl2,           arcustomdbl3,           arcustomdbl4,           arcustomdbl5,                                      arcustomdate1,                                      arcustomdate2,                                      arcustomdate3,                                      arcustomdate4,                                      arcustomdate5
                        strValue.Append("('" & urut & "', '" & FixQuotes(idLogin) & "', '" & FixQuotes(dr1("arid")) & "', '" & FixQuotes(AsFormatTanggal(dr1("artgl"))) & "', '" & FixQuotes(dr1("arsumber")) & "', '" & FixQuotes(dr1("arnotransaksi")) & "', '" & FixQuotes(dr1("arkontak")) & "', '" & FixQuotes(dr1("arkontakkode")) & "', '" & FixQuotes(dr1("arkontaknama")) & "', '" & FixQuotes(dr1("aralamat1")) & "', '" & FixQuotes(dr1("aralamat2")) & "', '" & FixQuotes(dr1("aralamat3")) & "', '" & FixQuotes(dr1("aralamat4")) & "', '" & FixQuotes(dr1("aralamat5")) & "', '" & FixQuotes(dr1("arnotelp1")) & "', '" & FixQuotes(dr1("arnotelp2")) & "', '" & FixQuotes(dr1("arnorek")) & "', '" & FixQuotes(dr1("armatauang")) & "', '" & FixQuotes(dr1("arkurs")) & "', '" & FixQuotes(dr1("aruraian")) & "', '" & FixQuotes(dr1("arcatatan")) & "', '" & FixDouble(dr1("artotal")) & "', '" & FixDouble(dr1("arbayar")) & "', '" & FixDouble(dr1("arsisa")) & "', '" & FixQuotes(AsFormatTanggal(dr1("artgljatuhtempo"))) & "', " & dr1("arstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(dr1("artgllunas"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("arinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("arisfungsional") & ", " & FixDouble(dr1("arissaldoakhir")) & ", '" & FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes(kodekontak) & "', '" & FixQuotes(namakontak) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & idkontak & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                        'INCREMENT NOURUT
                        urut += 1
                    Next
                End If
                'END OF SALDO MUTASI ================================


                'INSERT KE DATABASE =================================
                'SALDO MUTASI
                If Len(strValue.ToString) > 0 Then
                    sql = "Insert into M2r_Ar_Voucher(arnourut, idlogin, arid, artgl, arsumber, arnotransaksi, arkontak, arkontakkode, arkontaknama, aralamat1, aralamat2, aralamat3, aralamat4, aralamat5, arnotelp1, arnotelp2, arnorek, armatauang, arkurs, aruraian, arcatatan, artotal, arbayar, arsisa, artgljatuhtempo, arstatuslunas, artgllunas, arinputtgl, arisfungsional, arissaldoakhir, idmsmq, aruserid, arcustomtext1, arcustomtext2, arcustomtext3, arcustomtext4, arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomint4, arcustomint5, arcustomdbl1, arcustomdbl2, arcustomdbl3, arcustomdbl4, arcustomdbl5, arcustomdate1, arcustomdate2, arcustomdate3, arcustomdate4, arcustomdate5) values" & strValue.ToString & ""
                    If AsEksekusiSQL(sql, strCon) = False Then
                        result(2) = "Failed proccessing AR Voucher : " & IIf(orderBy = "kkode", dr("kkode"), dr("knama")) : GoTo selesai
                    End If
                End If
                'END OF INSERT KE DATABASE ==========================


                'AMBIL TOTAL VOUCHER PER KONTAK =====================
                'CLEAR QUERY
                strValue.Clear()

                sqlSubtotal = "SELECT '0' as arid, ar.artgl, ar.arsumber, '' as arnotransaksi, ar.arkontak, ar.arkontakkode, ar.arkontaknama, ar.aralamat1, ar.aralamat2, ar.aralamat3, ar.aralamat4, ar.aralamat5, ar.arnotelp1, ar.arnotelp2, ar.arnorek, ar.armatauang, ar.arkurs, 'Subtotal' as aruraian, 'Subtotal' as arcatatan, SUM(ar.artotal) as artotal, SUM(ar.arbayar) as arbayar, SUM(ar.arsisa) as arsisa, ar.artgljatuhtempo, ar.arstatuslunas, ar.artgllunas, ar.arinputtgl, ar.arisfungsional, ar.arissaldoakhir "
                sqlSubtotal &= " FROM m2r_ar_voucher ar "
                sqlSubtotal &= " WHERE ar.idlogin = '" & FixQuotes(idLogin) & "' "
                sqlSubtotal &= " AND ar.idmsmq = '" & FixQuotes(idMsmq) & "' "
                sqlSubtotal &= " AND ar.arcustomint1 = '" & FixDouble(idkontak) & "' "
                sqlSubtotal &= " GROUP BY ar.armatauang "
                sqlSubtotal &= " ORDER BY ar.arisfungsional DESC, ar.armatauang "

                'AMBIL SALDO SUBTOTAL
                dtSSubtotal = AsDataTableAmbilDariDB(sqlSubtotal, strCon)
                If dtSSubtotal.Rows.Count > 0 Then
                    'URUTAN SALDO AKHIR
                    urutSAkhir = 1

                    'INSERT SALDO SUBTOTAL KE TABEL PEMBANTU
                    'SORTING DATA TERLEBIH DAHULU
                    dtSSubtotal = AsDataTableFilterSortDt(dtSSubtotal, "", "arisfungsional DESC, armatauang")
                    For Each dr1 As DataRow In dtSSubtotal.Rows
                        'BUAT QUERY INSERT KE TABEL PEMBANTU
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'MAPPING :         arnourut,                     idlogin,                           arid,                                             artgl,                              arsumber,                             arnotransaksi,                             arkontak,                             arkontakkode,                             arkontaknama,                             aralamat1,                             aralamat2,                             aralamat3,                             aralamat4,                             aralamat5,                             arnotelp1,                             arnotelp2,                             arnorek,                             armatauang,                             arkurs,                             aruraian,                             arcatatan,                             artotal,                             arbayar,                             arsisa,                                             artgljatuhtempo,                   arstatuslunas,                                           artgllunas,                                              arinputtgl,                                         arisfungsional,                 arissaldoakhir,                     idmsmq,                    aruserid,                     arcustomtext1,                  arcustomtext2,           arcustomtext3,           arcustomtext4,           arcustomtext5,       arcustomint1,arcustomint2,arcustomint3,arcustomint4,arcustomint5, arcustomdbl1,           arcustomdbl2,           arcustomdbl3,           arcustomdbl4,           arcustomdbl5,                                      arcustomdate1,                                      arcustomdate2,                                      arcustomdate3,                                      arcustomdate4,                                      arcustomdate5
                        strValue.Append("('" & urut & "', '" & FixQuotes(idLogin) & "', '" & FixQuotes(dr1("arid")) & "', '" & FixQuotes(AsFormatTanggal(dr1("artgl"))) & "', '" & FixQuotes(dr1("arsumber")) & "', '" & FixQuotes(dr1("arnotransaksi")) & "', '" & FixQuotes(dr1("arkontak")) & "', '" & FixQuotes(dr1("arkontakkode")) & "', '" & FixQuotes(dr1("arkontaknama")) & "', '" & FixQuotes(dr1("aralamat1")) & "', '" & FixQuotes(dr1("aralamat2")) & "', '" & FixQuotes(dr1("aralamat3")) & "', '" & FixQuotes(dr1("aralamat4")) & "', '" & FixQuotes(dr1("aralamat5")) & "', '" & FixQuotes(dr1("arnotelp1")) & "', '" & FixQuotes(dr1("arnotelp2")) & "', '" & FixQuotes(dr1("arnorek")) & "', '" & FixQuotes(dr1("armatauang")) & "', '" & FixQuotes(dr1("arkurs")) & "', '" & FixQuotes(dr1("aruraian")) & "', '" & FixQuotes(dr1("arcatatan")) & "', '" & FixDouble(dr1("artotal")) & "', '" & FixDouble(dr1("arbayar")) & "', '" & FixDouble(dr1("arsisa")) & "', '" & FixQuotes(AsFormatTanggal(dr1("artgljatuhtempo"))) & "', " & dr1("arstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(dr1("artgllunas"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("arinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("arisfungsional") & ", " & FixDouble(urutSAkhir) & ", '" & FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes(kodekontak) & "', '" & FixQuotes(namakontak) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & idkontak & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                        'INCREMENT NOURUT SALDO AKHIR
                        urutSAkhir += 1

                        'INCREMENT NOURUT
                        urut += 1
                    Next
                End If
                'END OF AMBIL TOTAL VOUCHER PER KONTAK ==============


                'INSERT KE DATABASE =================================
                'SALDO SUBTOTAL
                If Len(strValue.ToString) > 0 Then
                    sql = "Insert into M2r_Ar_Voucher(arnourut, idlogin, arid, artgl, arsumber, arnotransaksi, arkontak, arkontakkode, arkontaknama, aralamat1, aralamat2, aralamat3, aralamat4, aralamat5, arnotelp1, arnotelp2, arnorek, armatauang, arkurs, aruraian, arcatatan, artotal, arbayar, arsisa, artgljatuhtempo, arstatuslunas, artgllunas, arinputtgl, arisfungsional, arissaldoakhir, idmsmq, aruserid, arcustomtext1, arcustomtext2, arcustomtext3, arcustomtext4, arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomint4, arcustomint5, arcustomdbl1, arcustomdbl2, arcustomdbl3, arcustomdbl4, arcustomdbl5, arcustomdate1, arcustomdate2, arcustomdate3, arcustomdate4, arcustomdate5) values" & strValue.ToString & ""
                    If AsEksekusiSQL(sql, strCon) = False Then
                        result(2) = "Failed proccessing Subtotal AR Voucher : " & IIf(orderBy = "kkode", dr("kkode"), dr("knama")) : GoTo selesai
                    End If
                End If
                'END OF INSERT KE DATABASE ==========================


                'UPDATE PROGRESS ====================================
                'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                progressPersen = IIf(stepKe = dtCustomer.Rows.Count + 1, Prosentase, (Math.Round(Prosentase / (dtCustomer.Rows.Count + 1), 2)) * stepKe)

                'UPDATE PROGRESS REPORT M0_MSMQ
                sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
                If AsEksekusiSQL(sql, strCon) = False Then
                    result(2) = "Failed updating progress AR Voucher : " & IIf(orderBy = "kkode", dr("kkode"), dr("knama")) : GoTo selesai
                End If
                'END OF UPDATE PROGRESS =============================

            Next
            'END OF PERULANGAN HITUNG Voucher PER KONTAK +++++++++++++++++++


            'HITUNG TOTAL SEMUA KONTAK +++++++++++++++++++++++++++++++++++++
            'TAMBAHKAN STEP
            stepKe += 1

            'AMBIL TOTAL VOUCHER SEMUA KONTAK =====================
            'CLEAR QUERY
            strValue.Clear()

            sqlSubtotal = "SELECT '0' as arid, ar.artgl, ar.arsumber, '' as arnotransaksi, '0' as arkontak, '' as arkontakkode, '' as arkontaknama, '' as aralamat1, '' as aralamat2, '' as aralamat3, '' as aralamat4, '' as aralamat5, '' as arnotelp1, '' as arnotelp2, ar.arnorek, ar.armatauang, ar.arkurs, 'Total' as aruraian, 'Total' as arcatatan, SUM(ar.artotal) as artotal, SUM(ar.arbayar) as arbayar, SUM(ar.arsisa) as arsisa, ar.artgljatuhtempo, ar.arstatuslunas, ar.artgllunas, ar.arinputtgl, ar.arisfungsional, ar.arissaldoakhir "
            sqlSubtotal &= " FROM m2r_ar_voucher ar "
            sqlSubtotal &= " WHERE ar.idlogin = '" & FixQuotes(idLogin) & "' "
            sqlSubtotal &= " AND ar.idmsmq = '" & FixQuotes(idMsmq) & "' "
            sqlSubtotal &= " AND ar.arissaldoakhir = '" & FixDouble(0) & "' "
            sqlSubtotal &= " GROUP BY ar.armatauang "
            sqlSubtotal &= " ORDER BY ar.arisfungsional DESC, ar.armatauang "

            'AMBIL SALDO SUBTOTAL
            dtSSubtotal = AsDataTableAmbilDariDB(sqlSubtotal, strCon)
            If dtSSubtotal.Rows.Count > 0 Then
                'URUTAN SALDO AKHIR
                urutSAkhir = 1

                'INSERT SALDO SUBTOTAL KE TABEL PEMBANTU
                'SORTING DATA TERLEBIH DAHULU
                dtSSubtotal = AsDataTableFilterSortDt(dtSSubtotal, "", "arisfungsional DESC, armatauang")
                For Each dr1 As DataRow In dtSSubtotal.Rows
                    'BUAT QUERY INSERT KE TABEL PEMBANTU
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    'MAPPING :         arnourut,                     idlogin,                           arid,                                             artgl,                              arsumber,                             arnotransaksi,                             arkontak,                             arkontakkode,                             arkontaknama,                             aralamat1,                             aralamat2,                             aralamat3,                             aralamat4,                             aralamat5,                             arnotelp1,                             arnotelp2,                             arnorek,                             armatauang,                             arkurs,                             aruraian,                             arcatatan,                             artotal,                             arbayar,                             arsisa,                                             artgljatuhtempo,                   arstatuslunas,                                           artgllunas,                                              arinputtgl,                                         arisfungsional,                 arissaldoakhir,                     idmsmq,                    aruserid,                 arcustomtext1,           arcustomtext2,           arcustomtext3,           arcustomtext4,           arcustomtext5,arcustomint1,arcustomint2,arcustomint3,arcustomint4,arcustomint5, arcustomdbl1,           arcustomdbl2,           arcustomdbl3,           arcustomdbl4,           arcustomdbl5,                                      arcustomdate1,                                      arcustomdate2,                                      arcustomdate3,                                      arcustomdate4,                                      arcustomdate5
                    strValue.Append("('" & urut & "', '" & FixQuotes(idLogin) & "', '" & FixQuotes(dr1("arid")) & "', '" & FixQuotes(AsFormatTanggal(dr1("artgl"))) & "', '" & FixQuotes(dr1("arsumber")) & "', '" & FixQuotes(dr1("arnotransaksi")) & "', '" & FixQuotes(dr1("arkontak")) & "', '" & FixQuotes(dr1("arkontakkode")) & "', '" & FixQuotes(dr1("arkontaknama")) & "', '" & FixQuotes(dr1("aralamat1")) & "', '" & FixQuotes(dr1("aralamat2")) & "', '" & FixQuotes(dr1("aralamat3")) & "', '" & FixQuotes(dr1("aralamat4")) & "', '" & FixQuotes(dr1("aralamat5")) & "', '" & FixQuotes(dr1("arnotelp1")) & "', '" & FixQuotes(dr1("arnotelp2")) & "', '" & FixQuotes(dr1("arnorek")) & "', '" & FixQuotes(dr1("armatauang")) & "', '" & FixQuotes(dr1("arkurs")) & "', '" & FixQuotes(dr1("aruraian")) & "', '" & FixQuotes(dr1("arcatatan")) & "', '" & FixDouble(dr1("artotal")) & "', '" & FixDouble(dr1("arbayar")) & "', '" & FixDouble(dr1("arsisa")) & "', '" & FixQuotes(AsFormatTanggal(dr1("artgljatuhtempo"))) & "', " & dr1("arstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(dr1("artgllunas"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("arinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("arisfungsional") & ", " & FixDouble(urutSAkhir) & ", '" & FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                    'INCREMENT NOURUT SALDO AKHIR
                    urutSAkhir += 1

                    'INCREMENT NOURUT
                    urut += 1
                Next
            End If
            'END OF AMBIL TOTAL VOUCHER SEMUA KONTAK ==============


            'INSERT KE DATABASE =================================
            'SALDO TOTAL
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Ar_Voucher(arnourut, idlogin, arid, artgl, arsumber, arnotransaksi, arkontak, arkontakkode, arkontaknama, aralamat1, aralamat2, aralamat3, aralamat4, aralamat5, arnotelp1, arnotelp2, arnorek, armatauang, arkurs, aruraian, arcatatan, artotal, arbayar, arsisa, artgljatuhtempo, arstatuslunas, artgllunas, arinputtgl, arisfungsional, arissaldoakhir, idmsmq, aruserid, arcustomtext1, arcustomtext2, arcustomtext3, arcustomtext4, arcustomtext5, arcustomint1, arcustomint2, arcustomint3, arcustomint4, arcustomint5, arcustomdbl1, arcustomdbl2, arcustomdbl3, arcustomdbl4, arcustomdbl5, arcustomdate1, arcustomdate2, arcustomdate3, arcustomdate4, arcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql, strCon) = False Then
                    result(2) = "Failed proccessing Total AR Voucher." : GoTo selesai
                End If
            End If
            'END OF INSERT KE DATABASE ==========================


            'UPDATE PROGRESS ====================================
            'HITUNG PROSENTASE PROGRESS (100/JML DATA CUSTOMER) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
            progressPersen = IIf(stepKe = dtCustomer.Rows.Count + 1, Prosentase, (Math.Round(Prosentase / (dtCustomer.Rows.Count + 1), 2)) * stepKe)

            'UPDATE PROGRESS REPORT M0_MSMQ
            sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
            If AsEksekusiSQL(sql, strCon) = False Then
                result(2) = "Failed updating progress Total AR Voucher." : GoTo selesai
            End If
            'END OF UPDATE PROGRESS =============================
            'END OF HITUNG TOTAL SEMUA KONTAK ++++++++++++++++++++++++++++++


        End If
        'END OF PERHITUNGAN Voucher PER KONTAK -------------------------------------

        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)

        'END OF TRANSAKSI KE DATABASE ======================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function
End Class
