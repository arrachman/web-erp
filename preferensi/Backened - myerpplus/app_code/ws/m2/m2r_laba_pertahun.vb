Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2r_laba_pertahun
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi


    Public Function m2r_laba_pertahun(ByVal param As String) As String
        '//LAPORAN LABA RUGI

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'ModuleId(0) As Integer, MenuName(1) As String, Query(2) As String, FileFormat(3) As Integer, Param1(4) As String, 
        'Param2(5) As String, Param3(6) As String, Param4(7) As String, Param5(8) As String, namaPerusahaan(9) As String, 
        'namaReport(10) As String, rp2(11) As String, rp3(12) As String, rp4(13) As String, rp5(14) As String, idMsmq(15) As String

        'Detail
        'tahun(0) As Integer, bulan(1) As Integer, level(2) As Integer, saldoNol(3) As Integer, pembagiNominal(4) As Integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'ModuleId, MenuName, Query, FileFormat, Param1, 
        'Param2, Param3, Param4, Param5, namaPerusahaan, 
        'namaReport, rp2, rp3, rp4, rp5, idMsmq

        'Detail
        'tahun, bulan, level, saldoNol, pembagiNominal

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

        'VARIABEL TOTAL STEP REPORT
        Dim totalStep As Double = 9

        'VARIABLE FUNGSI REPORT
        Dim idLogin As String = ""
        Dim ModuleId As Integer = 0, MenuName As String = "", Query As String = "", FileFormat As Integer = 0, Param1 As String = ""
        Dim Param2 As String = "", Param3 As String = "", Param4 As String = "", Param5 As String = "", namaPerusahaan As String = ""
        Dim namaReport As String = "", rp2 As String = "", rp3 As String = "", rp4 As String = "", rp5 As String = "", idMsmq As String = ""
        Dim tahun As Integer = 0, bulan As Integer = 0, level As Integer = 0, saldoNol As Integer = 0, pembagiNominal As Integer = 0, tahunLalu As Integer = 0, bulanLalu As Integer = 0
        Dim januari As Integer = 0, februari As Integer = 0, maret As Integer = 0, april As Integer = 0, mei As Integer = 0, juni As Integer = 0, juli As Integer = 0, agustus As Integer = 0, september As Integer = 0, oktober As Integer = 0, november As Integer = 0, desember As Integer = 0

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
            result(2) = "Invalid transaction data parameter. " + dataSplit.Length.ToString : GoTo selesai
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
        If (dataDetail.Length <> 5) Then
            result(2) = "Invalid detail transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'VALIDASI TIPE DATA DETAIL =========================================================
        'tahun(0) As Integer
        If (IsNumeric(dataDetail(0)) = False) Then
            result(2) = "tahun required numeric." : GoTo selesai
        Else
            tahun = dataDetail(0)
        End If

        'bulan(1) As Integer
        If (IsNumeric(dataDetail(1)) = False) Then
            result(2) = "bulan required numeric." : GoTo selesai
        Else
            bulan = dataDetail(1)
        End If

        'level(2) As Integer
        If (IsNumeric(dataDetail(2)) = False) Then
            result(2) = "level required numeric." : GoTo selesai
        Else
            level = dataDetail(2)
        End If

        'saldoNol(3) As Integer
        If (IsNumeric(dataDetail(3)) = False) Then
            result(2) = "saldoNol required numeric." : GoTo selesai
        Else
            saldoNol = dataDetail(3)
        End If

        'pembagiNominal(4) As Integer
        If (IsNumeric(dataDetail(4)) = False) Then
            result(2) = "pembagiNominal required numeric." : GoTo selesai
        Else
            pembagiNominal = dataDetail(4)
        End If

        'SET tahunLalu DAN bulanLalu
        'JIKA BULAN = 1 MAKA TAHUNLALU = TAHUN - 1 DAN BULANLALU = 12
        'JIKA BULAN <> 1 MAKA TAHUNLALU = TAHUN DAN BULANLALU = BULAN - 1
        If bulan = 1 Then
            tahunLalu = tahun - 1 : bulanLalu = 12
        Else
            tahunLalu = tahun : bulanLalu = bulan - 1
        End If
        'END OF VALIDASI TIPE DATA DETAIL ==================================================

        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'HAPUS IDLOGIN PADA M2r_Posisi_Keuangan ------------------------------------
        'HAPUS DATA BERDASRAKAN IDMSMSQ DI M2r_Posisi_Keuangan
        sql = "DELETE FROM M2r_Posisi_Keuangan WHERE idmsmq = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed replace trial balance data." : GoTo selesai
        End If
        'END OF HAPUS IDLOGIN PADA M2r_Posisi_Keuangan -----------------------------


        'HITUNG REALISASI BULAN INI DAN BULAN LALU ---------------------------------
        'HITUNG BULAN LALU
        stepKe = 1
        Dim rsHitung As String = M0_HitungRealisasilabapertahun(idLogin & "★M0_HitungRealisasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tahun & sptField)
        Dim rsHitungSplit() As String = rsHitung.Split(sptSubParam)
        'FORMAT KEMBALIAN = NamaFungsi△isSuccess△errMessage△errStep△idtransaksi★0△0△0△0△0★ ----> M0_HitungRealisasi△1△△0△0★0△0△0△0△0★
        If rsHitungSplit(1) <> 1 Then
            'JIKA ISSUCCESS <> 1 MAKA KIRIM INFORMASI PROSES GAGAL
            result(2) = rsHitungSplit(2) & " : " & tahun : GoTo selesai
        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        Dim progressPersen As Double = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If

        'HITUNG BULAN INI
        ' HITUNG BULAN JANUARI
        stepKe = 2
        rsHitung = M0_HitungRealisasilabapertahun(idLogin & "★M0_HitungRealisasi_laba_pertahun★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tahun & sptField & bulan)
        rsHitungSplit = rsHitung.Split(sptSubParam)
        'FORMAT KEMBALIAN = NamaFungsi△isSuccess△errMessage△errStep△idtransaksi★0△0△0△0△0★ ----> M0_HitungRealisasi△1△△0△0★0△0△0△0△0★
        If rsHitungSplit(1) <> 1 Then
            'JIKA ISSUCCESS <> 1 MAKA KIRIM INFORMASI PROSES GAGAL
            result(2) = rsHitungSplit(2) & " : " & tahun & " - " & bulan : GoTo selesai
        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF HITUNG REALISASI BULAN INI DAN BULAN LALU --------------------------

        Dim urut As Double = 1, labaKotor(2, 3) As Double
        'SET DEFAULT NILAI PENDAPATAN DAN HPP
        labaKotor(0, 0) = 0 : labaKotor(0, 1) = 0 : labaKotor(0, 2) = 0 'PENDAPATAN
        labaKotor(1, 0) = 0 : labaKotor(1, 1) = 0 : labaKotor(1, 2) = 0 'HPP

        'PENDAPATAN ----------------------------------------------------------------
        stepKe = 3
        strValue.Clear()

        'BUAT SQL AMBIL DATA BULAN INI DAN BULAN LALU
        Dim sqlBulanIni As String = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' AND r.rbulan = '" & FixDouble(bulan) & "') WHERE (c.ctipe = '11') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        Dim sqlBulanLalu As String = "SELECT c.cnomor as pknorek, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebitlalu, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkreditlalu, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahunLalu) & "' AND r.rbulan = '" & FixDouble(bulanLalu) & "') WHERE (c.ctipe = '11') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        Dim sqlBulanJanuari As String = "SELECT '" & FixDouble(tahun) & "'as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as plnorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cdg as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '11') AND r.rbulan = '1' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor "
        Dim sqlBulanFebruari As String = "SELECT '" & FixDouble(tahun) & "'as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as plnorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cdg as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '11') AND r.rbulan = '2' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor "

        ''QUERY AMBIL DATA

        'sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        'sql += " (" & sqlBulanIni & ") as s "
        'sql += " JOIN "
        'sql += " (" & sqlBulanLalu & ") as k "
        'sql += " ON s.pknorek = k.pknorek "
        'sql += " ORDER BY s.pknorek ASC "
        '-------------------
        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        sql += " (" & sqlBulanJanuari & ") as s "
        sql += " JOIN "
        sql += " (" & sqlBulanFebruari & ") as k "
        sql += " ON s.pknorek = k.pknorek "
        sql += " ORDER BY s.pknorek ASC "


        'AMBIL DATA KE DATABASE
        Dim dt As DataTable = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then

            'DEKLARASI UNTUK SIMPAN LEVEL INDUK
            Dim strInduk(level) As String
            Dim currLevel As Integer = 0, prevLevel As Integer = 0

            'CEK TAMPILKAN SALDO NOL
            'JIKA TAMPILKAN SALDO NOL MAKA TAMPILKAN SEMUA DATA
            'JIKA TIDAK TAMPILKAN SALDO NOL MAKA DATA DIFILTER LAGI YANG SALDO NYA > 0 SAJA
            If saldoNol <> 1 Then
                dt = AsDataTableFilterSortDt(dt, "pksaldo <> '0' OR pksaldolalu <> '0'")
            End If

            'PERULANGAN BUAT QUERY INSERT KE TABEL PEMBANTU (M2r_Posisi_Keuangan)
            dt = AsDataTableFilterSortDt(dt, "", "pknorek")
            For Each dr1 As DataRow In dt.Rows
                'SET CURRENT LEVEL
                currLevel = Integer.Parse(dr1("pklevel"))

                'CEK PERVIOUS LEVEL UNTUK MENAMPILKAN SUBTOTAL
                If prevLevel > currLevel Then
                    'JIKA PREVIOUS LEVEL > CURRENT LEVEL MAKA PERULANGAN MEMBUAT ROW SUBTOTAL
                    For i = 1 To prevLevel - currLevel
                        'BUAT QUERY INSERT SUBTOTAL 
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'ditambahkan urutan terlebih dahulu
                        strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                        'increament urutan
                        urut += 1
                    Next
                End If

                'JIKA LEVEL AKUN < LEVEL PARAMETER, MAKA SIMPAN AKUN UNTUK DITAMPILKAN SEBAGAI SUBTOTAL
                If currLevel < level Then
                    'tanda kurung diawal sengaja dikosongi, untuk mengisi urutan saat insert ke tabel
                    'mapping :                                 idlogin,                             pktahun,                             pkbulan,                             pknorek,                                                      pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                           pkdebit,                             pkkredit,                             pksaldo,                             pkdebitlalu,                             pkkreditlalu,                             pksaldolalu,                             pkdebitvariasi,                             pkkreditvariasi,                             pksaldovariasi,                             idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                    strInduk(currLevel) = "'" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & String.Concat("Total ", FixQuotes(dr1("pknoreknama"))) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & FixDouble(dr1("pkdebit")) & "', '" & FixDouble(dr1("pkkredit")) & "', '" & FixDouble(dr1("pksaldo")) & "', '" & FixDouble(dr1("pkdebitlalu")) & "', '" & FixDouble(dr1("pkkreditlalu")) & "', '" & FixDouble(dr1("pksaldolalu")) & "', '" & FixDouble(dr1("pkdebitvariasi")) & "', '" & FixDouble(dr1("pkkreditvariasi")) & "', '" & FixDouble(dr1("pksaldovariasi")) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')"
                End If

                'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                                                  pkdebit,                                                        pkkredit,                                                        pksaldo,                                                        pkdebitlalu,                                                        pkkreditlalu,                                                        pksaldolalu,                                                        pkdebitvariasi,                                                        pkkreditvariasi,                                                        pksaldovariasi,                                 idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & IIf(currLevel = level, FixDouble(dr1("pkdebit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkredit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldo")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldolalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldovariasi")), 0) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                'JIKA LEVEL = 1 MAKA AMBIL SALDO, SALDOLALU, SALDOVARIASI UNTUK HITUNG LABA KOTOR
                If currLevel = 1 Then
                    labaKotor(0, 0) = Double.Parse(dr1("pksaldo")) : labaKotor(0, 1) = Double.Parse(dr1("pksaldolalu")) : labaKotor(0, 2) = Double.Parse(dr1("pksaldovariasi"))
                End If

                'increament urutan
                urut += 1

                'SET PREVIOUS LEVEL
                prevLevel = Integer.Parse(dr1("pklevel"))
            Next

            'INSERT AKUN INDUK YANG TERSISA
            For i = 1 To level
                If Len(strInduk(level - i)) > 0 Then
                    'BUAT QUERY INSERT SUBTOTAL 
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    'ditambahkan urutan terlebih dahulu
                    strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                    'increament urutan
                    urut += 1
                End If
            Next

            'SIMPAN KE M2r_Posisi_Keuangan
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed processing balance sheet report." : GoTo selesai
                End If
            End If

        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF PENDAPATAN ---------------------------------------------------------


        'HPP -----------------------------------------------------------------------
        stepKe = 4
        strValue.Clear()

        'AMBIL LEVEL TERKECIL DARI AKUN HPP
        Dim dtHpp As DataTable = AsDataTableAmbilDariDB("SELECT MIN(clevel), cparent FROM m1_coa WHERE ctipe = 12")
        Dim levelInduk As Integer = 0, parentHpp As String = ""
        If dtHpp.Rows.Count > 0 Then
            levelInduk = Integer.Parse(dtHpp.Rows(0)(0)) : parentHpp = dtHpp.Rows(0)(1)
        Else
            result(2) = "CoA for COGS not found." : GoTo selesai
        End If

        'BUAT SQL AMBIL DATA BULAN INI DAN BULAN LALU
        'JIKA LEVEL TERKECIL DARI AKUN HPP > 1 DAN LEVEL < LEVEL INDUK, MAKA FILTER LEVEL -->  c.clevel <= levelInduk
        sqlBulanIni = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' AND r.rbulan = '" & FixDouble(bulan) & "') WHERE (c.ctipe = '12') AND c.clevel <= '" & IIf(levelInduk > 1 And level < levelInduk, FixDouble(levelInduk), FixDouble(level)) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanJanuari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '12') AND r.rbulan = '1' AND c.clevel <= '" & IIf(levelInduk > 1 And level < levelInduk, FixDouble(levelInduk), FixDouble(level)) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanFebruari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '12') AND r.rbulan = '2' AND c.clevel <= '" & IIf(levelInduk > 1 And level < levelInduk, FixDouble(levelInduk), FixDouble(level)) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanLalu = "SELECT c.cnomor as pknorek, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebitlalu, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkreditlalu, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahunLalu) & "' AND r.rbulan = '" & FixDouble(bulanLalu) & "') WHERE (c.ctipe = '12') AND c.clevel <= '" & IIf(levelInduk > 1 And level < levelInduk, FixDouble(levelInduk), FixDouble(level)) & "' GROUP BY c.cnomor ORDER BY c.cnomor"

        ''QUERY AMBIL DATA
        'sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        'sql += " (" & sqlBulanIni & ") as s "
        'sql += " JOIN "
        'sql += " (" & sqlBulanLalu & ") as k "
        'sql += " ON s.pknorek = k.pknorek "
        ' sql += " ORDER BY s.pknorek ASC "
        '======
        'QUERY AMBIL DATA
        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        sql += " (" & sqlBulanJanuari & ") as s "
        sql += " JOIN "
        sql += " (" & sqlBulanFebruari & ") as k "
        sql += " ON s.pknorek = k.pknorek "
        sql += " ORDER BY s.pknorek ASC "

        'AMBIL DATA KE DATABASE
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then

            'DEKLARASI UNTUK SIMPAN LEVEL INDUK
            Dim strInduk(level) As String
            Dim currLevel As Integer = 0, prevLevel As Integer = 0

            'JIKA LEVEL TERKECIL DARI AKUN HPP > 1 DAN LEVEL > 1 MAKA AKUN INDUK DIBUATKAN SENDIRI
            If levelInduk > 1 And level > 1 Then
                Dim dtInduk As DataTable = AsDataTableFilterLimit(dt, "pklevel = '" & levelInduk & "'", "pknorek", , 1)
                If dtInduk.Rows.Count > 0 Then
                    Dim dr1 As DataRow = dtInduk.Rows(0)
                    For i = 1 To levelInduk - 1
                        'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                        'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,           pklevel,                     pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                 pkgddata,                  pkleveldata,           pkdebit,    pkkredit,     pksaldo, pkdebitlalu,pkkreditlalu, pksaldolalu,pkdebitvariasi,pkkreditvariasi,pksaldovariasi,              idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                        strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & i & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes("G") & "', " & dr1("pkleveldata") & ", '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & 0 & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                        'increament urutan
                        urut += 1
                    Next
                End If
            End If

            'CEK TAMPILKAN SALDO NOL
            'JIKA TAMPILKAN SALDO NOL MAKA TAMPILKAN SEMUA DATA
            'JIKA TIDAK TAMPILKAN SALDO NOL MAKA DATA DIFILTER LAGI YANG SALDO NYA > 0 SAJA
            If saldoNol <> 1 Then
                dt = AsDataTableFilterSortDt(dt, "pksaldo <> '0' OR pksaldolalu <> '0'")
            End If

            'PERULANGAN BUAT QUERY INSERT KE TABEL PEMBANTU (M2r_Posisi_Keuangan)
            dt = AsDataTableFilterSortDt(dt, "", "pknorek")
            For Each dr1 As DataRow In dt.Rows
                'SET CURRENT LEVEL
                currLevel = IIf(level = 1, 1, Integer.Parse(dr1("pklevel")))

                'CEK PERVIOUS LEVEL UNTUK MENAMPILKAN SUBTOTAL
                If prevLevel > currLevel Then
                    'JIKA PREVIOUS LEVEL > CURRENT LEVEL MAKA PERULANGAN MEMBUAT ROW SUBTOTAL
                    For i = 1 To prevLevel - currLevel
                        'BUAT QUERY INSERT SUBTOTAL 
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'ditambahkan urutan terlebih dahulu
                        strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                        'increament urutan
                        urut += 1
                    Next
                End If

                'JIKA LEVEL AKUN < LEVEL PARAMETER, MAKA SIMPAN AKUN UNTUK DITAMPILKAN SEBAGAI SUBTOTAL
                If currLevel < level Then
                    'tanda kurung diawal sengaja dikosongi, untuk mengisi urutan saat insert ke tabel
                    'mapping :                                 idlogin,                             pktahun,                             pkbulan,                             pknorek,                                                      pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                           pkdebit,                             pkkredit,                             pksaldo,                             pkdebitlalu,                             pkkreditlalu,                             pksaldolalu,                             pkdebitvariasi,                             pkkreditvariasi,                             pksaldovariasi,                             idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                    strInduk(currLevel) = "'" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & String.Concat("Total ", FixQuotes(dr1("pknoreknama"))) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & FixDouble(dr1("pkdebit")) & "', '" & FixDouble(dr1("pkkredit")) & "', '" & FixDouble(dr1("pksaldo")) & "', '" & FixDouble(dr1("pkdebitlalu")) & "', '" & FixDouble(dr1("pkkreditlalu")) & "', '" & FixDouble(dr1("pksaldolalu")) & "', '" & FixDouble(dr1("pkdebitvariasi")) & "', '" & FixDouble(dr1("pkkreditvariasi")) & "', '" & FixDouble(dr1("pksaldovariasi")) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')"
                End If

                'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                                    pklevel,                            pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                                                 pkgddata,                   pkleveldata,                                                  pkdebit,                                                        pkkredit,                                                        pksaldo,                                                        pkdebitlalu,                                                        pkkreditlalu,                                                        pksaldolalu,                                                        pkdebitvariasi,                                                        pkkreditvariasi,                                                        pksaldovariasi,                                 idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & IIf(level = 1, 1, dr1("pklevel")) & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & IIf(level = 1, "D", FixQuotes(dr1("pkgddata"))) & "', " & dr1("pkleveldata") & ", '" & IIf(currLevel = level, FixDouble(dr1("pkdebit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkredit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldo")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldolalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldovariasi")), 0) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                'JIKA LEVEL = LEVEL INDUK MAKA AMBIL SALDO DAN SALDOLALU UNTUK HITUNG LABA KOTOR
                'If currLevel = level Then
                If currLevel = levelInduk Or currLevel = 1 Then
                    labaKotor(1, 0) = Double.Parse(dr1("pksaldo")) : labaKotor(1, 1) = Double.Parse(dr1("pksaldolalu")) : labaKotor(1, 2) = Double.Parse(dr1("pksaldovariasi"))
                End If

                'increament urutan
                urut += 1

                'SET PREVIOUS LEVEL
                prevLevel = Integer.Parse(dr1("pklevel"))
            Next

            'INSERT AKUN INDUK YANG TERSISA
            For i = 1 To level
                If Len(strInduk(level - i)) > 0 Then
                    'BUAT QUERY INSERT SUBTOTAL 
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    'ditambahkan urutan terlebih dahulu
                    strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                    'increament urutan
                    urut += 1
                End If
            Next

            'JIKA LEVEL TERKECIL DARI AKUN HPP > 1 DAN LEVEL > 1 MAKA SUBTOTAL AKUN INDUK DIBUATKAN SENDIRI
            If levelInduk > 1 And level > 1 Then
                Dim dtInduk As DataTable = AsDataTableFilterLimit(dt, "pklevel = '" & levelInduk & "'", "pknorek", , 1)
                If dtInduk.Rows.Count > 0 Then
                    Dim dr1 As DataRow = dtInduk.Rows(0)
                    For i = 1 To levelInduk - 1
                        'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                        'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                                                     pknoreknama,                   pktipe,                           pkgd,                             pkjenis,           pklevel,                     pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                 pkgddata,                  pkleveldata,                           pkdebit,                             pkkredit,                             pksaldo,                             pkdebitlalu,                             pkkreditlalu,                             pksaldolalu,                             pkdebitvariasi,                             pkkreditvariasi,                             pksaldovariasi,                             idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                        strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & String.Concat("Total ", FixQuotes(dr1("pknoreknama"))) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & i & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes("G") & "', " & dr1("pkleveldata") & ", '" & FixDouble(dr1("pkdebit")) & "', '" & FixDouble(dr1("pkkredit")) & "', '" & FixDouble(dr1("pksaldo")) & "', '" & FixDouble(dr1("pkdebitlalu")) & "', '" & FixDouble(dr1("pkkreditlalu")) & "', '" & FixDouble(dr1("pksaldolalu")) & "', '" & FixDouble(dr1("pkdebitvariasi")) & "', '" & FixDouble(dr1("pkkreditvariasi")) & "', '" & FixDouble(dr1("pksaldovariasi")) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                        'increament urutan
                        urut += 1
                    Next
                End If
            End If

            'SIMPAN KE M2r_Posisi_Keuangan
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed processing balance sheet report." : GoTo selesai
                End If
            End If

        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF HPP ----------------------------------------------------------------


        'LABA KOTOR ----------------------------------------------------------------
        stepKe = 5
        strValue.Clear()

        'BUAT QUERY INSERT LABA KOTOR
        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
        'mapping :                                                 idlogin,                      pktahun,                    pkbulan,               pknorek,                      pknoreknama,           pktipe,                pkgd,                   pkjenis,      pklevel,            pklevel1,               pklevel2,                pklevel3,                pklevel4,                pklevel5,               pkgddata,           pkleveldata,     pkdebit,    pkkredit,                  pksaldo,                               pkdebitlalu,pkkreditlalu,                 pksaldolalu,                         pkdebitvariasi,pkkreditvariasi,                      pksaldovariasi,                                idmsmq,                    pkuserid,                 pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
        strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(idLogin) & "', '" & FixQuotes(tahun) & "', '" & FixQuotes(bulan) & "', '" & FixQuotes("") & "', '" & FixQuotes("Laba Bruto") & "', " & 0 & ", '" & FixQuotes("G") & "', '" & FixQuotes("M") & "', " & 1 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("G") & "', " & level & ", '" & 0 & "', '" & 0 & "', '" & FixDouble(labaKotor(0, 0) - labaKotor(1, 0)) & "', '" & 0 & "', '" & 0 & "', '" & FixDouble(labaKotor(0, 1) - labaKotor(1, 1)) & "', '" & 0 & "', '" & 0 & "', '" & FixDouble(labaKotor(0, 2) - labaKotor(1, 2)) & "', '" & FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

        'SIMPAN KE M2r_Posisi_Keuangan
        If Len(strValue.ToString) > 0 Then
            sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
            If AsEksekusiSQL(sql) = False Then
                result(2) = "Failed processing balance sheet report." : GoTo selesai
            End If
        End If

        'increament urutan
        urut += 1

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF LABA KOTOR ---------------------------------------------------------


        'PENDAPATAN LAIN -----------------------------------------------------------
        stepKe = 6
        strValue.Clear()

        'BUAT SQL AMBIL DATA BULAN INI DAN BULAN LALU
        sqlBulanIni = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' AND r.rbulan = '" & FixDouble(bulan) & "') WHERE (c.ctipe = '14') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanLalu = "SELECT c.cnomor as pknorek, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebitlalu, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkreditlalu, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahunLalu) & "' AND r.rbulan = '" & FixDouble(bulanLalu) & "') WHERE (c.ctipe = '14') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanJanuari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '14') AND r.rbulan = '1' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanFebruari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '14') AND r.rbulan = '2' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"

        'QUERY AMBIL DATA
        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        sql += " (" & sqlBulanJanuari & ") as s "
        sql += " JOIN "
        sql += " (" & sqlBulanFebruari & ") as k "
        sql += " ON s.pknorek = k.pknorek "
        sql += " ORDER BY s.pknorek ASC "

        'AMBIL DATA KE DATABASE
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then

            'DEKLARASI UNTUK SIMPAN LEVEL INDUK
            Dim strInduk(level) As String
            Dim currLevel As Integer = 0, prevLevel As Integer = 0

            'CEK TAMPILKAN SALDO NOL
            'JIKA TAMPILKAN SALDO NOL MAKA TAMPILKAN SEMUA DATA
            'JIKA TIDAK TAMPILKAN SALDO NOL MAKA DATA DIFILTER LAGI YANG SALDO NYA > 0 SAJA
            If saldoNol <> 1 Then
                dt = AsDataTableFilterSortDt(dt, "pksaldo <> '0' OR pksaldolalu <> '0'")
            End If

            'PERULANGAN BUAT QUERY INSERT KE TABEL PEMBANTU (M2r_Posisi_Keuangan)
            dt = AsDataTableFilterSortDt(dt, "", "pknorek")
            For Each dr1 As DataRow In dt.Rows
                'SET CURRENT LEVEL
                currLevel = Integer.Parse(dr1("pklevel"))

                'CEK PERVIOUS LEVEL UNTUK MENAMPILKAN SUBTOTAL
                If prevLevel > currLevel Then
                    'JIKA PREVIOUS LEVEL > CURRENT LEVEL MAKA PERULANGAN MEMBUAT ROW SUBTOTAL
                    For i = 1 To prevLevel - currLevel
                        'BUAT QUERY INSERT SUBTOTAL 
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'ditambahkan urutan terlebih dahulu
                        strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                        'increament urutan
                        urut += 1
                    Next
                End If

                'JIKA LEVEL AKUN < LEVEL PARAMETER, MAKA SIMPAN AKUN UNTUK DITAMPILKAN SEBAGAI SUBTOTAL
                If currLevel < level Then
                    'tanda kurung diawal sengaja dikosongi, untuk mengisi urutan saat insert ke tabel
                    'mapping :                                 idlogin,                             pktahun,                             pkbulan,                             pknorek,                                                      pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                           pkdebit,                             pkkredit,                             pksaldo,                             pkdebitlalu,                             pkkreditlalu,                             pksaldolalu,                             pkdebitvariasi,                             pkkreditvariasi,                             pksaldovariasi,                             idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                    strInduk(currLevel) = "'" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & String.Concat("Total ", FixQuotes(dr1("pknoreknama"))) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & FixDouble(dr1("pkdebit")) & "', '" & FixDouble(dr1("pkkredit")) & "', '" & FixDouble(dr1("pksaldo")) & "', '" & FixDouble(dr1("pkdebitlalu")) & "', '" & FixDouble(dr1("pkkreditlalu")) & "', '" & FixDouble(dr1("pksaldolalu")) & "', '" & FixDouble(dr1("pkdebitvariasi")) & "', '" & FixDouble(dr1("pkkreditvariasi")) & "', '" & FixDouble(dr1("pksaldovariasi")) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')"
                End If

                'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                                                  pkdebit,                                                        pkkredit,                                                        pksaldo,                                                        pkdebitlalu,                                                        pkkreditlalu,                                                        pksaldolalu,                                                        pkdebitvariasi,                                                        pkkreditvariasi,                                                        pksaldovariasi,                                 idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & IIf(currLevel = level, FixDouble(dr1("pkdebit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkredit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldo")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldolalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldovariasi")), 0) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                'increament urutan
                urut += 1

                'SET PREVIOUS LEVEL
                prevLevel = Integer.Parse(dr1("pklevel"))
            Next

            'INSERT AKUN INDUK YANG TERSISA
            For i = 1 To level
                If Len(strInduk(level - i)) > 0 Then
                    'BUAT QUERY INSERT SUBTOTAL 
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    'ditambahkan urutan terlebih dahulu
                    strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                    'increament urutan
                    urut += 1
                End If
            Next

            'SIMPAN KE M2r_Posisi_Keuangan
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed processing balance sheet report." : GoTo selesai
                End If
            End If

        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF PENDAPATAN LAIN ----------------------------------------------------


        'BIAYA ---------------------------------------------------------------------
        stepKe = 7
        strValue.Clear()

        'BUAT SQL AMBIL DATA BULAN INI DAN BULAN LALU
        sqlBulanIni = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' AND r.rbulan = '" & FixDouble(bulan) & "') WHERE (c.ctipe = '13') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanLalu = "SELECT c.cnomor as pknorek, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebitlalu, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkreditlalu, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahunLalu) & "' AND r.rbulan = '" & FixDouble(bulanLalu) & "') WHERE (c.ctipe = '13') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanJanuari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "') WHERE (c.ctipe = '13') AND r.rbulan = '1' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanFebruari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "') WHERE (c.ctipe = '13') AND r.rbulan = '2' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"

        'QUERY AMBIL DATA
        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        sql += " (" & sqlBulanJanuari & ") as s "
        sql += " JOIN "
        sql += " (" & sqlBulanFebruari & ") as k "
        sql += " ON s.pknorek = k.pknorek "
        sql += " ORDER BY s.pknorek ASC "

        'AMBIL DATA KE DATABASE
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then

            'DEKLARASI UNTUK SIMPAN LEVEL INDUK
            Dim strInduk(level) As String
            Dim currLevel As Integer = 0, prevLevel As Integer = 0

            'CEK TAMPILKAN SALDO NOL
            'JIKA TAMPILKAN SALDO NOL MAKA TAMPILKAN SEMUA DATA
            'JIKA TIDAK TAMPILKAN SALDO NOL MAKA DATA DIFILTER LAGI YANG SALDO NYA > 0 SAJA
            If saldoNol <> 1 Then
                dt = AsDataTableFilterSortDt(dt, "pksaldo <> '0' OR pksaldolalu <> '0'")
            End If

            'PERULANGAN BUAT QUERY INSERT KE TABEL PEMBANTU (M2r_Posisi_Keuangan)
            dt = AsDataTableFilterSortDt(dt, "", "pknorek")
            For Each dr1 As DataRow In dt.Rows
                'SET CURRENT LEVEL
                currLevel = Integer.Parse(dr1("pklevel"))

                'CEK PREVIOUS LEVEL UNTUK MENAMPILKAN SUBTOTAL
                If prevLevel > currLevel Then
                    'JIKA PREVIOUS LEVEL > CURRENT LEVEL MAKA PERULANGAN MEMBUAT ROW SUBTOTAL
                    For i = 1 To prevLevel - currLevel
                        'BUAT QUERY INSERT SUBTOTAL 
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'ditambahkan urutan terlebih dahulu
                        strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                        'increament urutan
                        urut += 1
                    Next
                End If

                'JIKA LEVEL AKUN < LEVEL PARAMETER, MAKA SIMPAN AKUN UNTUK DITAMPILKAN SEBAGAI SUBTOTAL
                If currLevel < level Then
                    'tanda kurung diawal sengaja dikosongi, untuk mengisi urutan saat insert ke tabel
                    'mapping :                                 idlogin,                             pktahun,                             pkbulan,                             pknorek,                                                     pknoreknama,                   pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                           pkdebit,                             pkkredit,                                                                                pksaldo,                                                                            pkdebitlalu,                             pkkreditlalu,                                                                                pksaldolalu,                                                                                pkdebitvariasi,                             pkkreditvariasi,                                                                                pksaldovariasi,                                                                                   idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                    strInduk(currLevel) = "'" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & String.Concat("Total ", FixQuotes(dr1("pknoreknama"))) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & FixDouble(dr1("pkdebit")) & "', '" & FixDouble(dr1("pkkredit")) & "', '" & IIf(currLevel = 1 And levelInduk > 1, Double.Parse(FixDouble(dr1("pksaldo"))) - labaKotor(1, 0), FixDouble(dr1("pksaldo"))) & "', '" & FixDouble(dr1("pkdebitlalu")) & "', '" & FixDouble(dr1("pkkreditlalu")) & "', '" & IIf(currLevel = 1 And levelInduk > 1, Double.Parse(FixDouble(dr1("pksaldolalu"))) - labaKotor(1, 1), FixDouble(dr1("pksaldolalu"))) & "', '" & FixDouble(dr1("pkdebitvariasi")) & "', '" & FixDouble(dr1("pkkreditvariasi")) & "', '" & IIf(currLevel = 1 And levelInduk > 1, Double.Parse(FixDouble(dr1("pksaldovariasi"))) - labaKotor(1, 2), FixDouble(dr1("pksaldovariasi"))) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')"
                End If

                'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                ''mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                                                  pkdebit,                                                        pkkredit,                                                        pksaldo,                                                        pkdebitlalu,                                                        pkkreditlalu,                                                        pksaldolalu,                                                        pkdebitvariasi,                                                        pkkreditvariasi,                                                        pksaldovariasi,                                 idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                'strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & IIf(currLevel = level, FixDouble(dr1("pkdebit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkredit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldo")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldolalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldovariasi")), 0) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                                                  pkdebit,                                                        pkkredit,                                                                                                           pksaldo,                                                                                                       pkdebitlalu,                                                        pkkreditlalu,                                                                                                           pksaldolalu,                                                                                                           pkdebitvariasi,                                                        pkkreditvariasi,                                                                                                           pksaldovariasi,                                                                                       idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & IIf(currLevel = level, FixDouble(dr1("pkdebit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkredit")), 0) & "', '" & IIf(currLevel = level, IIf(currLevel = 1 And levelInduk > 1, Double.Parse(FixDouble(dr1("pksaldo"))) - labaKotor(1, 0), FixDouble(dr1("pksaldo"))), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditlalu")), 0) & "', '" & IIf(currLevel = level, IIf(currLevel = 1 And levelInduk > 1, Double.Parse(FixDouble(dr1("pksaldolalu"))) - labaKotor(1, 1), FixDouble(dr1("pksaldolalu"))), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditvariasi")), 0) & "', '" & IIf(currLevel = level, IIf(currLevel = 1 And levelInduk > 1, Double.Parse(FixDouble(dr1("pksaldovariasi"))) - labaKotor(1, 2), FixDouble(dr1("pksaldovariasi"))), 0) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                'increament urutan
                urut += 1

                'SET PREVIOUS LEVEL
                prevLevel = Integer.Parse(dr1("pklevel"))
            Next

            'INSERT AKUN INDUK YANG TERSISA
            For i = 1 To level
                If Len(strInduk(level - i)) > 0 Then
                    'BUAT QUERY INSERT SUBTOTAL 
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    'ditambahkan urutan terlebih dahulu
                    strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                    'increament urutan
                    urut += 1
                End If
            Next

            'SIMPAN KE M2r_Posisi_Keuangan
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed processing balance sheet report." : GoTo selesai
                End If
            End If

        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF BIAYA --------------------------------------------------------------


        'BIAYA LAIN ----------------------------------------------------------------
        stepKe = 8
        strValue.Clear()

        'BUAT SQL AMBIL DATA BULAN INI DAN BULAN LALU
        sqlBulanIni = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' AND r.rbulan = '" & FixDouble(bulan) & "') WHERE (c.ctipe = '15') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanLalu = "SELECT c.cnomor as pknorek, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebitlalu, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkreditlalu, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahunLalu) & "' AND r.rbulan = '" & FixDouble(bulanLalu) & "') WHERE (c.ctipe = '15') AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanJanuari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "') WHERE (c.ctipe = '15') AND r.rbulan = '1' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"
        sqlBulanFebruari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "') WHERE (c.ctipe = '15') AND r.rbulan = '2' AND c.clevel <= '" & FixDouble(level) & "' GROUP BY c.cnomor ORDER BY c.cnomor"

        'QUERY AMBIL DATA
        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        sql += " (" & sqlBulanJanuari & ") as s "
        sql += " JOIN "
        sql += " (" & sqlBulanFebruari & ") as k "
        sql += " ON s.pknorek = k.pknorek "
        sql += " ORDER BY s.pknorek ASC "

        'AMBIL DATA KE DATABASE
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then

            'DEKLARASI UNTUK SIMPAN LEVEL INDUK
            Dim strInduk(level) As String
            Dim currLevel As Integer = 0, prevLevel As Integer = 0

            'CEK TAMPILKAN SALDO NOL
            'JIKA TAMPILKAN SALDO NOL MAKA TAMPILKAN SEMUA DATA
            'JIKA TIDAK TAMPILKAN SALDO NOL MAKA DATA DIFILTER LAGI YANG SALDO NYA > 0 SAJA
            If saldoNol <> 1 Then
                dt = AsDataTableFilterSortDt(dt, "pksaldo <> '0' OR pksaldolalu <> '0'")
            End If

            'PERULANGAN BUAT QUERY INSERT KE TABEL PEMBANTU (M2r_Posisi_Keuangan)
            dt = AsDataTableFilterSortDt(dt, "", "pknorek")
            For Each dr1 As DataRow In dt.Rows
                'SET CURRENT LEVEL
                currLevel = Integer.Parse(dr1("pklevel"))

                'CEK PERVIOUS LEVEL UNTUK MENAMPILKAN SUBTOTAL
                If prevLevel > currLevel Then
                    'JIKA PREVIOUS LEVEL > CURRENT LEVEL MAKA PERULANGAN MEMBUAT ROW SUBTOTAL
                    For i = 1 To prevLevel - currLevel
                        'BUAT QUERY INSERT SUBTOTAL 
                        strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                        'ditambahkan urutan terlebih dahulu
                        strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                        'increament urutan
                        urut += 1
                    Next
                End If

                'JIKA LEVEL AKUN < LEVEL PARAMETER, MAKA SIMPAN AKUN UNTUK DITAMPILKAN SEBAGAI SUBTOTAL
                If currLevel < level Then
                    'tanda kurung diawal sengaja dikosongi, untuk mengisi urutan saat insert ke tabel
                    'mapping :                                 idlogin,                             pktahun,                             pkbulan,                             pknorek,                                                      pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                           pkdebit,                             pkkredit,                             pksaldo,                             pkdebitlalu,                             pkkreditlalu,                             pksaldolalu,                             pkdebitvariasi,                             pkkreditvariasi,                             pksaldovariasi,                             idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                    strInduk(currLevel) = "'" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & String.Concat("Total ", FixQuotes(dr1("pknoreknama"))) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & FixDouble(dr1("pkdebit")) & "', '" & FixDouble(dr1("pkkredit")) & "', '" & FixDouble(dr1("pksaldo")) & "', '" & FixDouble(dr1("pkdebitlalu")) & "', '" & FixDouble(dr1("pkkreditlalu")) & "', '" & FixDouble(dr1("pksaldolalu")) & "', '" & FixDouble(dr1("pkdebitvariasi")) & "', '" & FixDouble(dr1("pkkreditvariasi")) & "', '" & FixDouble(dr1("pksaldovariasi")) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')"
                End If

                'BUAT QUERY INSERT TRANSAKSI MUTASI NOREK
                strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                'jika currLevel = level, maka saldo ditampilkan. jika currlevel <> level maka saldo diisi 0
                'mapping :                                                      idlogin,                             pktahun,                             pkbulan,                             pknorek,                             pknoreknama,                  pktipe,                           pkgd,                             pkjenis,                  pklevel,                           pklevel1,                             pklevel2,                             pklevel3,                             pklevel4,                             pklevel5,                             pkgddata,                  pkleveldata,                                                  pkdebit,                                                        pkkredit,                                                        pksaldo,                                                        pkdebitlalu,                                                        pkkreditlalu,                                                        pksaldolalu,                                                        pkdebitvariasi,                                                        pkkreditvariasi,                                                        pksaldovariasi,                                 idmsmq,                             pkuserid,                   pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
                strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(dr1("idlogin")) & "', '" & FixQuotes(dr1("pktahun")) & "', '" & FixQuotes(dr1("pkbulan")) & "', '" & FixQuotes(dr1("pknorek")) & "', '" & FixQuotes(dr1("pknoreknama")) & "', " & dr1("pktipe") & ", '" & FixQuotes(dr1("pkgd")) & "', '" & FixQuotes(dr1("pkjenis")) & "', " & dr1("pklevel") & ", '" & FixQuotes(dr1("pklevel1")) & "', '" & FixQuotes(dr1("pklevel2")) & "', '" & FixQuotes(dr1("pklevel3")) & "', '" & FixQuotes(dr1("pklevel4")) & "', '" & FixQuotes(dr1("pklevel5")) & "', '" & FixQuotes(dr1("pkgddata")) & "', " & dr1("pkleveldata") & ", '" & IIf(currLevel = level, FixDouble(dr1("pkdebit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkredit")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldo")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditlalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldolalu")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkdebitvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pkkreditvariasi")), 0) & "', '" & IIf(currLevel = level, FixDouble(dr1("pksaldovariasi")), 0) & "', '" & FixQuotes(dr1("idmsmq")) & "', '" & FixQuotes(dr1("pkuserid")) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                'increament urutan
                urut += 1

                'SET PREVIOUS LEVEL
                prevLevel = Integer.Parse(dr1("pklevel"))
            Next

            'INSERT AKUN INDUK YANG TERSISA
            For i = 1 To level
                If Len(strInduk(level - i)) > 0 Then
                    'BUAT QUERY INSERT SUBTOTAL 
                    strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
                    'ditambahkan urutan terlebih dahulu
                    strValue.Append("('" & FixDouble(urut) & "'," & strInduk(level - i))
                    'increament urutan
                    urut += 1
                End If
            Next

            'SIMPAN KE M2r_Posisi_Keuangan
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed processing balance sheet report." : GoTo selesai
                End If
            End If

        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF BIAYA LAIN ---------------------------------------------------------


        'LABA BERSIH ---------------------------------------------------------------
        stepKe = 9
        strValue.Clear()

        Dim labaBersih(2, 3) As Double
        'SET DEFAULT NILAI PENDAPATAN DAN BIAYA
        labaBersih(0, 0) = 0 : labaBersih(0, 1) = 0 : labaBersih(0, 2) = 0 'PENDAPATAN
        labaBersih(1, 0) = 0 : labaBersih(1, 1) = 0 : labaBersih(1, 2) = 0 'BIAYA

        'AMBIL AKUN LEVEL 1 DARI PENDAPATAN + PENDAPATAN LAIN DAN BIAYA + BIAYA LAIN
        'BUAT SQL AMBIL DATA BULAN INI DAN BULAN LALU
        sqlBulanIni = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' AND r.rbulan = '" & FixDouble(bulan) & "') WHERE (c.ctipe = '11' OR c.ctipe = '12' OR c.ctipe = '13' OR c.ctipe = '14' OR c.ctipe = '15') AND c.clevel = '" & FixDouble(1) & "' GROUP BY c.cjenis ORDER BY c.cnomor"
        sqlBulanLalu = "SELECT c.cnomor as pknorek, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebitlalu, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkreditlalu, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahunLalu) & "' AND r.rbulan = '" & FixDouble(bulanLalu) & "') WHERE (c.ctipe = '11' OR c.ctipe = '12' OR c.ctipe = '13' OR c.ctipe = '14' OR c.ctipe = '15') AND c.clevel = '" & FixDouble(1) & "' GROUP BY c.cjenis ORDER BY c.cnomor"
        sqlBulanJanuari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldo FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '11' OR c.ctipe = '12' OR c.ctipe = '13' OR c.ctipe = '14' OR c.ctipe = '15') AND r.rbulan = '1' AND c.clevel = '" & FixDouble(1) & "' GROUP BY c.cjenis ORDER BY c.cnomor"
        sqlBulanFebruari = "SELECT '" & FixDouble(tahun) & "' as pktahun, '" & FixDouble(bulan) & "' as pkbulan, c.cnomor as pknorek, c.cnama as pknoreknama, c.ctipe as pktipe, c.cgd as pkgd, c.cjenis as pkjenis, c.clevel as pklevel, c.clevel1 as pklevel1, c.clevel2 as pklevel2, c.clevel3 as pklevel3, c.clevel4 as pklevel4, c.clevel5 as pklevel5, (CASE c.clevel WHEN '" & FixDouble(level) & "' THEN 'D' ELSE 'G' END) AS pkgddata, '" & FixDouble(level) & "' as pkleveldata, (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") AS pkdebit, (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") AS pkkredit, (CASE c.cdc WHEN 'D' THEN (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") ELSE (IFNULL(SUM(r.rjmlkredit),0) / " & FixDouble(pembagiNominal) & ") - (IFNULL(SUM(r.rjmldebit),0) / " & FixDouble(pembagiNominal) & ") END) AS pksaldolalu FROM m1_coa c LEFT JOIN m2_realization r ON c.cnomor = r.rnorek AND (r.rtahun = '" & FixDouble(tahun) & "' ) WHERE (c.ctipe = '11' OR c.ctipe = '12' OR c.ctipe = '13' OR c.ctipe = '14' OR c.ctipe = '15') AND r.rbulan = '2' AND c.clevel = '" & FixDouble(1) & "' GROUP BY c.cjenis ORDER BY c.cnomor"

        'QUERY AMBIL DATA
        sql = "SELECT '" & FixQuotes(idLogin) & "' as idlogin, s.pktahun, s.pkbulan, s.pknorek, s.pknoreknama, s.pktipe, s.pkgd, s.pkjenis, s.pklevel, s.pklevel1, s.pklevel2, s.pklevel3, s.pklevel4, s.pklevel5, s.pkgddata, s.pkleveldata, s.pkdebit, s.pkkredit, s.pksaldo, k.pkdebitlalu, k.pkkreditlalu, k.pksaldolalu, s.pkdebit - k.pkdebitlalu as pkdebitvariasi, s.pkkredit - k.pkkreditlalu as pkkreditvariasi, s.pksaldo - k.pksaldolalu as pksaldovariasi, '" & FixQuotes(idMsmq) & "' as  idmsmq, '" & FixDouble(userid) & "' as pkuserid FROM "
        sql += " (" & sqlBulanJanuari & ") as s "
        sql += " JOIN "
        sql += " (" & sqlBulanFebruari & ") as k "
        sql += " ON s.pknorek = k.pknorek "
        sql += " ORDER BY s.pknorek ASC "

        'AMBIL DATA KE DATABASE
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then

            'PENDAPATAN
            Dim dtPend As DataTable = AsDataTableFilterSortDt(dt, "pkjenis = 'P'")
            If dtPend.Rows.Count > 0 Then
                For Each dr1 In dtPend.Rows
                    'SET SALDO PENDAPATAN
                    labaBersih(0, 0) = Double.Parse(dr1("pksaldo")) : labaBersih(0, 1) = Double.Parse(dr1("pksaldolalu")) : labaBersih(0, 2) = Double.Parse(dr1("pksaldovariasi"))
                Next
            End If

            'BIAYA
            Dim dtBiaya As DataTable = AsDataTableFilterSortDt(dt, "pkjenis = 'B'")
            If dtBiaya.Rows.Count > 0 Then
                For Each dr1 In dtBiaya.Rows
                    'SET SALDO BIAYA
                    labaBersih(1, 0) = Double.Parse(dr1("pksaldo")) : labaBersih(1, 1) = Double.Parse(dr1("pksaldolalu")) : labaBersih(1, 2) = Double.Parse(dr1("pksaldovariasi"))
                Next
            End If

            'BUAT QUERY INSERT LABA KOTOR
            strValue.Append(IIf(Len(strValue.ToString) = 0, "", ", "))
            'mapping :                                                 idlogin,                      pktahun,                    pkbulan,               pknorek,                              pknoreknama,           pktipe,                pkgd,                   pkjenis,      pklevel,            pklevel1,               pklevel2,                pklevel3,                pklevel4,                pklevel5,               pkgddata,           pkleveldata,     pkdebit,    pkkredit,                  pksaldo,                                 pkdebitlalu,pkkreditlalu,                 pksaldolalu,                           pkdebitvariasi,pkkreditvariasi,                      pksaldovariasi,                                  idmsmq,                    pkuserid,                 pkcustomtext1,           pkcustomtext2,           pkcustomtext3,           pkcustomtext4,           pkcustomtext5,pkcustomint1,pkcustomint2,pkcustomint3,pkcustomint4,pkcustomint5, pkcustomdbl1,           pkcustomdbl2,           pkcustomdbl3,           pkcustomdbl4,           pkcustomdbl5,                                 pkcustomdate1,                                      pkcustomdate2,                                      pkcustomdate3,                                      pkcustomdate4,                                      pkcustomdate5
            strValue.Append("('" & FixDouble(urut) & "','" & FixQuotes(idLogin) & "', '" & FixQuotes(tahun) & "', '" & FixQuotes(bulan) & "', '" & FixQuotes("") & "', '" & FixQuotes("Laba Sebelum Pajak") & "', " & 0 & ", '" & FixQuotes("G") & "', '" & FixQuotes("M") & "', " & 1 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("G") & "', " & level & ", '" & 0 & "', '" & 0 & "', '" & FixDouble(labaBersih(0, 0) - labaBersih(1, 0)) & "', '" & 0 & "', '" & 0 & "', '" & FixDouble(labaBersih(0, 1) - labaBersih(1, 1)) & "', '" & 0 & "', '" & 0 & "', '" & FixDouble(labaBersih(0, 2) - labaBersih(1, 2)) & "', '" & FixQuotes(idMsmq) & "', '" & FixQuotes(userid) & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

            'SIMPAN KE M2r_Posisi_Keuangan
            If Len(strValue.ToString) > 0 Then
                sql = "Insert into M2r_Posisi_Keuangan(pkurut, idlogin, pktahun, pkbulan, pknorek, pknoreknama, pktipe, pkgd, pkjenis, pklevel, pklevel1, pklevel2, pklevel3, pklevel4, pklevel5, pkgddata, pkleveldata, pkdebit, pkkredit, pksaldo, pkdebitlalu, pkkreditlalu, pksaldolalu, pkdebitvariasi, pkkreditvariasi, pksaldovariasi, idmsmq, pkuserid, pkcustomtext1, pkcustomtext2, pkcustomtext3, pkcustomtext4, pkcustomtext5, pkcustomint1, pkcustomint2, pkcustomint3, pkcustomint4, pkcustomint5, pkcustomdbl1, pkcustomdbl2, pkcustomdbl3, pkcustomdbl4, pkcustomdbl5, pkcustomdate1, pkcustomdate2, pkcustomdate3, pkcustomdate4, pkcustomdate5) values" & strValue.ToString & ""
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed processing balance sheet report." : GoTo selesai
                End If
            End If

            'increament urutan
            urut += 1

        End If

        'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
        progressPersen = IIf(stepKe = totalStep, Prosentase, Math.Round(Prosentase / totalStep, 2) * stepKe)

        'UPDATE PROGRESS REPORT M0_MSMQ
        sql = "UPDATE m0_msmq SET progress = '4', progresspersen = '" & FixDouble(progressPersen) & "' WHERE id = '" & FixQuotes(idMsmq) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed updating progress balance sheet. #" & stepKe : GoTo selesai
        End If
        'END OF LABA BERSIH --------------------------------------------------------

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

    Public Function M0_HitungRealisasilabapertahun(ByVal param As String) As String
        '//HITUNG REALISASI NOAKUN (M2_REALIZATION)

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim norekLR As String = "", strUpdate As String = ""

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim dt As New DataTable, vtahun As Integer = 0, vbulan As Integer = 0, vkodepa As Integer = 0, isActive As Integer = 0


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


        'MAPPING BUAT WS ----------------------------------------------------------
        'tahun(0) As integer, bulan(1) As integer

        'MAPPING BUAT FLEX --------------------------------------------------------
        'tahun, bulan


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 2) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'tahun(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "tahun required numeric." : GoTo selesai
        Else
            vtahun = Integer.Parse(dataUtama(0))
        End If

        'bulan(1) As Integer
        If (IsNumeric(dataUtama(1)) = False) Then
            result(2) = "bulan required numeric." : GoTo selesai
        Else
            vbulan = Integer.Parse(dataUtama(1))
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'TRANSAKSI KE DATABASE =======================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'AMBIL KODE PA DARI M2_ACCOUNTING_PERIOD
        sql = "SELECT apkode, aptutupperiode FROM m2_accounting_period WHERE aptahun = '" & vtahun & "' AND apbulan = '" & vbulan & "'"
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            vkodepa = Integer.Parse(dt.Rows(0)("apkode"))
            isActive = Integer.Parse(dt.Rows(0)("aptutupperiode"))
        Else
            result(2) = "Accounting Period not found." : GoTo selesai
        End If


        'JIKA PERIODE AKUNTANSI BLM TUTUP PERIODE MAKA DIHITUNG
        If isActive = 0 Then
            'AMBIL NOREK BULAN BERJALAN DARI SETTING
            Dim dtLR As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'akun' AND skode= 'LabaRugiBerjalan'")
            If dtLR.Rows.Count > 0 Then
                norekLR = dtLR.Rows(0)(0)
            Else
                result(2) = "Setting for Monthly Profit/Loss CoA not found." : GoTo selesai
            End If

            'UPDATE REALISASI PADA M2_REALIZATION MENJADI 0
            sql = "UPDATE m2_realization SET rjmldebit = 0, rjmlkredit = 0 WHERE rkodepa = '" & vkodepa & "'"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "Failed Updating realization data." : GoTo selesai
            End If

            'VARIABEL DATATABLE UNTUK NOAKUN LEVEL 1 SD 5
            Dim dt1 As New DataTable, dt2 As New DataTable, dt3 As New DataTable, dt4 As New DataTable, dt5 As New DataTable

            'REALISASI AKUN LEVEL 5
            'AMBIL NOAKUN LEVEL 5 DARI M2_TRANSACTION_JOURNAL SESUAI KODEPA
            sql = "SELECT tnorek as norek, SUM(tdebit) as debit, SUM(tkredit) as kredit FROM m2_transaction_journal WHERE tstatus IN(2, 3, 4, 7) AND tkodepa = '" & vkodepa & "' GROUP BY tnorek"
            dt5 = AsDataTableAmbilDariDB(sql)
            If dt5.Rows.Count > 0 Then

                'UPDATE REALISASI AKUN LEVEL 5
                For Each dr5 As DataRow In dt5.Rows
                    strUpdate = IIf(Len(strUpdate.ToString) = 0, "", strUpdate & ", ")
                    'mapping :                                            rtahun,                      rbulan,                           rnorek,                            rjmldebit,                         rjmlkredit, ranggaran,            rkodepa
                    strUpdate = String.Concat(strUpdate, "('" & FixDouble(vtahun) & "', '" & FixDouble(vbulan) & "', '" & FixQuotes(dr5("norek")) & "', '" & FixDouble(dr5("debit")) & "', '" & FixDouble(dr5("kredit")) & "', 0, '" & FixDouble(vkodepa) & "')")
                Next
                If Len(strUpdate) > 0 Then
                    sql = "INSERT INTO m2_realization (rtahun, rbulan, rnorek, rjmldebit, rjmlkredit, ranggaran, rkodepa) VALUES " & strUpdate & " ON DUPLICATE KEY UPDATE rjmldebit = VALUES(rjmldebit), rjmlkredit = VALUES(rjmlkredit)"
                    If AsEksekusiSQL(sql) = False Then
                        result(2) = "Failed Updating realization data level 5." : GoTo selesai
                    End If
                End If

                'RESET VARIABLE UPDATE
                strUpdate = ""

                'UPDATE REALISASI AKUN LEVEL 5 UNTUK AKUN LABARUGI BULAN BERJALAN
                'HITUNG LABARUGI PERBULAN
                'TOTAL PENDAPATAN - TOTAL BIAYA = (PENDAPATAN + PENDAPATAN LAIN) - (BIAYA + BIAYA LAIN)
                sql = "SELECT IFNULL(SUM(tkredit) - SUM(tdebit),0) as saldo FROM m2_transaction_journal JOIN m1_coa ON tnorek = cnomor WHERE tstatus IN(2, 3, 4, 7) AND (ctipe = 11 OR ctipe = 14) AND tkodepa = '" & vkodepa & "'"
                Dim dtPend As DataTable = AsDataTableAmbilDariDB(sql)
                sql = "SELECT IFNULL(SUM(tdebit) - SUM(tkredit),0) as saldo FROM m2_transaction_journal JOIN m1_coa ON tnorek = cnomor WHERE tstatus IN(2, 3, 4, 7) AND (ctipe = 12 OR ctipe = 13 OR ctipe = 15) AND tkodepa = '" & vkodepa & "'"
                Dim dtBiaya As DataTable = AsDataTableAmbilDariDB(sql)

                'SET PENDAPATAN DAN BIAYA
                Dim pend As Double = 0, biaya As Double = 0
                If dtPend.Rows.Count > 0 Then
                    pend = Double.Parse(dtPend.Rows(0)(0))
                End If
                If dtBiaya.Rows.Count > 0 Then
                    biaya = Double.Parse(dtBiaya.Rows(0)(0))
                End If

                'BUAT QUERY HITUNG LABARUGI PERBULAN
                strUpdate = IIf(Len(strUpdate.ToString) = 0, "", strUpdate & ", ")
                'mapping :                                            rtahun,                      rbulan,                           rnorek,                              rjmldebit,                                                              rjmlkredit,                             ranggaran,            rkodepa
                strUpdate = String.Concat(strUpdate, "('" & FixDouble(vtahun) & "', '" & FixDouble(vbulan) & "', '" & FixQuotes(norekLR) & "', '" & IIf(pend - biaya < 0, FixDouble(Math.Abs(pend - biaya)), 0) & "', '" & IIf(pend - biaya >= 0, FixDouble(Math.Abs(pend - biaya)), 0) & "', 0, '" & FixDouble(vkodepa) & "')")

                'UPDATE KE DATABASE AKUN LABARUGI PERBULAN
                If Len(strUpdate) > 0 Then
                    sql = "INSERT INTO m2_realization (rtahun, rbulan, rnorek, rjmldebit, rjmlkredit, ranggaran, rkodepa) VALUES " & strUpdate & " ON DUPLICATE KEY UPDATE rjmldebit = rjmldebit + VALUES(rjmldebit), rjmlkredit = rjmlkredit + VALUES(rjmlkredit)"
                    If AsEksekusiSQL(sql) = False Then
                        result(2) = "Failed Updating realization data Monthly Profit/Loss." : GoTo selesai
                    End If
                End If

                'RESET VARIABLE UPDATE
                strUpdate = ""

                'REALISASI AKUN LEVEL 4
                'AMBIL NOAKUN INDUK LEVEL 4 DARI M2_REALIZATION SESUAI KODEPA
                sql = "SELECT c.clevel4 as norek, SUM(r.rjmldebit) as debit, SUM(r.rjmlkredit) as kredit FROM m2_realization r JOIN m1_coa c ON r.rnorek = c.cnomor WHERE r.rkodepa = '" & vkodepa & "' AND c.clevel = 5 GROUP BY c.clevel4"
                dt4 = AsDataTableAmbilDariDB(sql)
                If dt4.Rows.Count > 0 Then

                    'UPDATE REALISASI AKUN LEVEL 4
                    For Each dr4 As DataRow In dt4.Rows
                        strUpdate = IIf(Len(strUpdate.ToString) = 0, "", strUpdate & ", ")
                        'mapping :                                            rtahun,                      rbulan,                           rnorek,                            rjmldebit,                         rjmlkredit, ranggaran,            rkodepa
                        strUpdate = String.Concat(strUpdate, "('" & FixDouble(vtahun) & "', '" & FixDouble(vbulan) & "', '" & FixQuotes(dr4("norek")) & "', '" & FixDouble(dr4("debit")) & "', '" & FixDouble(dr4("kredit")) & "', 0, '" & FixDouble(vkodepa) & "')")
                    Next
                    If Len(strUpdate) > 0 Then
                        sql = "INSERT INTO m2_realization (rtahun, rbulan, rnorek, rjmldebit, rjmlkredit, ranggaran, rkodepa) VALUES " & strUpdate & " ON DUPLICATE KEY UPDATE rjmldebit = VALUES(rjmldebit), rjmlkredit = VALUES(rjmlkredit)"
                        If AsEksekusiSQL(sql) = False Then
                            result(2) = "Failed Updating realization data level 4." : GoTo selesai
                        End If
                    End If

                    'RESET VARIABLE UPDATE
                    strUpdate = ""

                    'REALISASI AKUN LEVEL 3
                    'AMBIL NOAKUN INDUK LEVEL 3 DARI M2_REALIZATION SESUAI KODEPA
                    sql = "SELECT c.clevel3 as norek, SUM(r.rjmldebit) as debit, SUM(r.rjmlkredit) as kredit FROM m2_realization r JOIN m1_coa c ON r.rnorek = c.cnomor WHERE r.rkodepa = '" & vkodepa & "' AND c.clevel = 4 GROUP BY c.clevel3"
                    dt3 = AsDataTableAmbilDariDB(sql)
                    If dt3.Rows.Count > 0 Then

                        'UPDATE REALISASI AKUN LEVEL 3
                        For Each dr3 As DataRow In dt3.Rows
                            strUpdate = IIf(Len(strUpdate.ToString) = 0, "", strUpdate & ", ")
                            'mapping :                                            rtahun,                      rbulan,                           rnorek,                            rjmldebit,                         rjmlkredit, ranggaran,            rkodepa
                            strUpdate = String.Concat(strUpdate, "('" & FixDouble(vtahun) & "', '" & FixDouble(vbulan) & "', '" & FixQuotes(dr3("norek")) & "', '" & FixDouble(dr3("debit")) & "', '" & FixDouble(dr3("kredit")) & "', 0, '" & FixDouble(vkodepa) & "')")
                        Next
                        If Len(strUpdate) > 0 Then
                            sql = "INSERT INTO m2_realization (rtahun, rbulan, rnorek, rjmldebit, rjmlkredit, ranggaran, rkodepa) VALUES " & strUpdate & " ON DUPLICATE KEY UPDATE rjmldebit = VALUES(rjmldebit), rjmlkredit = VALUES(rjmlkredit)"
                            If AsEksekusiSQL(sql) = False Then
                                result(2) = "Failed Updating realization data level 3." : GoTo selesai
                            End If
                        End If

                        'RESET VARIABLE UPDATE
                        strUpdate = ""

                        'REALISASI AKUN LEVEL 2
                        'AMBIL NOAKUN INDUK LEVEL 2 DARI M2_REALIZATION SESUAI KODEPA
                        sql = "SELECT c.clevel2 as norek, SUM(r.rjmldebit) as debit, SUM(r.rjmlkredit) as kredit FROM m2_realization r JOIN m1_coa c ON r.rnorek = c.cnomor WHERE r.rkodepa = '" & vkodepa & "' AND c.clevel = 3 GROUP BY c.clevel2"
                        dt2 = AsDataTableAmbilDariDB(sql)
                        If dt2.Rows.Count > 0 Then

                            'UPDATE REALISASI AKUN LEVEL 2
                            For Each dr2 As DataRow In dt2.Rows
                                strUpdate = IIf(Len(strUpdate.ToString) = 0, "", strUpdate & ", ")
                                'mapping :                                            rtahun,                      rbulan,                           rnorek,                            rjmldebit,                         rjmlkredit, ranggaran,            rkodepa
                                strUpdate = String.Concat(strUpdate, "('" & FixDouble(vtahun) & "', '" & FixDouble(vbulan) & "', '" & FixQuotes(dr2("norek")) & "', '" & FixDouble(dr2("debit")) & "', '" & FixDouble(dr2("kredit")) & "', 0, '" & FixDouble(vkodepa) & "')")
                            Next
                            If Len(strUpdate) > 0 Then
                                sql = "INSERT INTO m2_realization (rtahun, rbulan, rnorek, rjmldebit, rjmlkredit, ranggaran, rkodepa) VALUES " & strUpdate & " ON DUPLICATE KEY UPDATE rjmldebit = VALUES(rjmldebit), rjmlkredit = VALUES(rjmlkredit)"
                                If AsEksekusiSQL(sql) = False Then
                                    result(2) = "Failed Updating realization data level 2." : GoTo selesai
                                End If
                            End If

                            'RESET VARIABLE UPDATE
                            strUpdate = ""

                            'REALISASI AKUN LEVEL 1
                            'AMBIL NOAKUN INDUK LEVEL 1 DARI M2_REALIZATION SESUAI KODEPA
                            sql = "SELECT c.clevel1 as norek, SUM(r.rjmldebit) as debit, SUM(r.rjmlkredit) as kredit FROM m2_realization r JOIN m1_coa c ON r.rnorek = c.cnomor WHERE r.rkodepa = '" & vkodepa & "' AND c.clevel = 2 GROUP BY c.clevel1"
                            dt1 = AsDataTableAmbilDariDB(sql)
                            If dt2.Rows.Count > 0 Then

                                'UPDATE REALISASI AKUN LEVEL 1
                                For Each dr1 As DataRow In dt1.Rows
                                    strUpdate = IIf(Len(strUpdate.ToString) = 0, "", strUpdate & ", ")
                                    'mapping :                                            rtahun,                      rbulan,                           rnorek,                            rjmldebit,                         rjmlkredit, ranggaran,            rkodepa
                                    strUpdate = String.Concat(strUpdate, "('" & FixDouble(vtahun) & "', '" & FixDouble(vbulan) & "', '" & FixQuotes(dr1("norek")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("kredit")) & "', 0, '" & FixDouble(vkodepa) & "')")
                                Next
                                If Len(strUpdate) > 0 Then
                                    sql = "INSERT INTO m2_realization (rtahun, rbulan, rnorek, rjmldebit, rjmlkredit, ranggaran, rkodepa) VALUES " & strUpdate & " ON DUPLICATE KEY UPDATE rjmldebit = VALUES(rjmldebit), rjmlkredit = VALUES(rjmlkredit)"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed Updating realization data level 1." : GoTo selesai
                                    End If
                                End If

                                'RESET VARIABLE UPDATE
                                strUpdate = ""

                            End If

                        End If

                    End If

                End If

            End If
        End If
        'END OF TRANSAKSI KE DATABASE ================================================

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
End Class
