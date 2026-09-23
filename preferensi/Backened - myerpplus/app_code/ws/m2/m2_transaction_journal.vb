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
Public Class m2_transaction_journal
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M2_Transaction_Journal_VoucherSearch(ByVal param As String) As String
        'M2_Transaction_Journal_VoucherSearch --------------------------------------------------------
        'tid, tnotransaksi, tnorek, tnoreknama, tmatauang, tkurs, tdebit, 
        'tkredit, tdebitvalas, tkreditvalas, tkontak, tkontakkode, tkontaknama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
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
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
            result(2) = "Access denied for get data"
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

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m2_transaction_journal_voucher")
        sql = "select t.tid AS tid, t.tnotransaksi AS tnotransaksi, t.tnorek AS tnorek,c.cnama AS tnoreknama,t.tmatauang AS tmatauang,t.tkurs AS tkurs,t.tdebit AS tdebit,t.tkredit AS tkredit,t.tdebitvalas AS tdebitvalas, t.tkreditvalas AS tkreditvalas, t.tkontak as tkontak, k.kkode as tkontakkode, k.knama as tkontaknama from m2_transaction_journal t left join m1_coa c on t.tnorek = c.cnomor left join m1_contact k on t.tkontak = k.kid"

        dt = AmbilData("aplikasi1-M2_Transaction_Journal_Voucher", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("tid"), 0), sptField,
                             FxDB(dr("tnotransaksi"), ""), sptField,
                             FxDB(dr("tnorek"), ""), sptField,
                             FxDB(dr("tnoreknama"), ""), sptField,
                             FxDB(dr("tmatauang"), ""), sptField,
                             FxDB(dr("tkurs"), 0), sptField,
                             FxDB(dr("tdebit"), 0), sptField,
                             FxDB(dr("tkredit"), 0), sptField,
                             FxDB(dr("tdebitvalas"), 0), sptField,
                             FxDB(dr("tkreditvalas"), 0), sptField,
                             FxDB(dr("tkontak"), 0), sptField,
                             FxDB(dr("tkontakkode"), ""), sptField,
                             FxDB(dr("tkontaknama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Journal Voucher data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tid, tnotransaksi, tnorek, tnoreknama, tmatauang, tkurs, tdebit, tkredit, tdebitvalas, tkreditvalas, tkontak, tkontakkode, tkontaknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_GeneralLedgerOld(ByVal param As String) As String
        'M2_GeneralLedger -----------------------------------------------------------
        'tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tkontak, tkontakkode, tkontaknama, 
        'tnorek, tnoreknama, tmatauang, tkurs, tdebit, tkredit, tdebitvalas, tkreditvalas, 
        'turaian, tcatatan, tsaldo, tsaldoawal, tsaldodebit, tsaldokredit, tsaldoakhir, tissaldoawal

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'tglAwal(0) As String, tglAkhir(1) As String, norekAwal(2) As String, norekAkhir(3) As String
        'kontakAwal(4) As String, kontakAkhir(5) As String, orderBy(6) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'tglAwal, tglAkhir, norekAwal, norekAkhir, 
        'kontakAwal, kontakAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim dt As New DataTable
        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI
        Dim tglAwal As String = "", tglAkhir As String = "", norekAwal As String = "", norekAkhir As String = ""
        Dim kontakAwal As String = "", kontakAkhir As String = "", orderBy As String = ""

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


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 7) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            tglAkhir = AsFormatTanggal(Now)
        End If

        'norekAwal(2) As String
        norekAwal = dataUtama(2)

        'norekAkhir(3) As String
        norekAkhir = dataUtama(3)

        'kontakAwal(4) As String
        kontakAwal = dataUtama(4)

        'kontakAkhir(5) As String
        kontakAkhir = dataUtama(5)

        'orderBy(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataUtama(6).ToString <> "cnomor" And dataUtama(6).ToString <> "cnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataUtama(6)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ==================================================


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

        'TRANSAKSI KE DATABASE
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        'AMBIL DATA DARI SETTING -----------------------------
        Dim matauang As String = "", kurs As String = ""
        Dim dtMatauang As DataTable = AsDataTableAmbilDariDB("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')")
        'MATAUANG
        matauang = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
        If matauang = "Not found" Then
            result(2) = "Setting Functional Currency not found." : GoTo selesai
        End If
        'KURS
        kurs = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
        If kurs = "Not found" Then
            result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
        End If
        'END OF AMBIL DATA DARI SETTING ----------------------


        Dim dtSA As New DataTable, saldoawal As Double = 0
        Dim sqlSA As String = "", sqlSM As String = "", sqlSMGabung As String = ""
        Dim sqlSAJadi As String = "", sqlSMJadi As String = ""


        'QUERY SALDO AWAL ###########################
        '                                    tid,                 tsumber,                             tidtransaksi,                 tnotransaksi,                                 ttgl,      tkontak,       tkontakkode,       tkontaknama,             tnorek,            tnoreknama,                                  tmatauang,                              tkurs,                            tdebit,                             tkredit,                                 tdebitvalas,                                  tkreditvalas,                 turaian,                 tcatatan,                                                                                                                                              tsaldo,      tsaldoawal,      tsaldodebit,      tsaldokredit,      tsaldoakhir,      tissaldoawal
        sqlSA = "  SELECT IFNULL(t.tid,0) as tid, 'Saldo Awal' as tsumber, IFNULL(t.tidtransaksi,0) as tidtransaksi, 'Saldo Awal' as tnotransaksi, '" & FixQuotes(tglAwal) & "' as ttgl, 0 as tkontak, '' as tkontakkode, '' as tkontaknama, c.cnomor as tnorek, c.cnama as tnoreknama, '" & FixQuotes(matauang) & "' as tmatauang, '" & FixDouble(kurs) & "' as tkurs, IFNULL(SUM(t.tdebit),0) as tdebit, IFNULL(SUM(t.tkredit),0) as tkredit, IFNULL(SUM(t.tdebitvalas),0) as tdebitvalas, IFNULL(SUM(t.tkreditvalas),0) as tkreditvalas, 'Saldo Awal' as turaian, 'Saldo Awal' as tcatatan, (CASE c.cdc WHEN 'D' THEN IFNULL(SUM(t.tdebit),0) - IFNULL(SUM(t.tkredit),0) ELSE IFNULL(SUM(t.tkredit),0) - IFNULL(SUM(t.tdebit),0) END) as tsaldo, 0 as tsaldoawal, 0 as tsaldodebit, 0 as tsaldokredit, 0 as tsaldoakhir, 0 as tissaldoawal"
        sqlSA &= " FROM m1_coa c"
        sqlSA &= " LEFT JOIN m2_transaction_journal t ON c.cnomor = t.tnorek AND t.tstatus IN(2,3,4,7) AND t.ttgl < '" & FixQuotes(tglAwal) & "'"
        sqlSA &= " LEFT JOIN m1_contact k ON t.tkontak = k.kid"
        sqlSA &= " WHERE c.cnomor = '" & FixQuotes(norekAwal) & "'"
        'FILTER CONTACT
        If Len(kontakAwal) > 0 And Len(kontakAkhir) > 0 Then
            sqlSA &= " AND (k.kkode BETWEEN '" & FixQuotes(kontakAwal) & "' AND '" & FixQuotes(kontakAkhir) & "')"
        ElseIf Len(kontakAwal) > 0 Then
            sqlSA &= " AND (k.kkode >= '" & FixQuotes(kontakAwal) & "')"
        ElseIf Len(kontakAkhir) > 0 Then
            sqlSA &= " AND (k.kkode <= '" & FixQuotes(kontakAkhir) & "')"
        End If

        'AMBIL SALDO AWAL
        dtSA = AsDataTableAmbilDariDB(sqlSA)
        If dtSA.Rows.Count > 0 Then
            saldoawal = Double.Parse(dtSA.Rows(0)("tsaldo"))
        Else
            saldoawal = 0
        End If

        'QUERY SALDO MUTASI #########################
        '                   tid,                     tsumber,                   tidtransaksi,                   tnotransaksi,           ttgl,              tkontak,            tkontakkode,            tkontaknama,             tnorek,            tnoreknama,                tmatauang,            tkurs,             tdebit,              tkredit,                  tdebitvalas,                   tkreditvalas,              turaian,               tcatatan,                                                                                                                                                                tsaldo,      tsaldoawal,      tsaldodebit,      tsaldokredit,      tsaldoakhir,                 tissaldoawal
        sqlSM = "  SELECT t.tid as tid, t.tsumber as tsumber, t.tidtransaksi as tidtransaksi, t.tnotransaksi as tnotransaksi, t.ttgl as ttgl, t.tkontak as tkontak, k.kkode as tkontakkode, k.knama as tkontaknama, c.cnomor as tnorek, c.cnama as tnoreknama, t.tmatauang as tmatauang, t.tkurs as tkurs, t.tdebit as tdebit, t.tkredit as tkredit, t.tdebitvalas as tdebitvalas, t.tkreditvalas as tkreditvalas, t.turaian as turaian, t.tcatatan as tcatatan, (CASE c.cdc WHEN 'D' THEN @saldo := @saldo + IFNULL(t.tdebit,0) - IFNULL(t.tkredit,0) ELSE @saldo := @saldo + IFNULL(t.tkredit,0) - IFNULL(t.tdebit,0) END) as tsaldo, 0 as tsaldoawal, 0 as tsaldodebit, 0 as tsaldokredit, 0 as tsaldoakhir, t.tsaldoawal as tissaldoawal"
        sqlSM &= " FROM m2_transaction_journal t"
        sqlSM &= " JOIN m1_coa c ON t.tnorek = c.cnomor"
        sqlSM &= " JOIN m1_contact k ON t.tkontak = k.kid"
        sqlSM &= " , (SELECT @saldo := " & FixDouble(saldoawal) & ") AS variableInit1"
        sqlSM &= " WHERE t.tstatus IN(2,3,4,7) AND t.tnorek = '" & FixQuotes(norekAwal) & "'"
        sqlSM &= " AND t.ttgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "'"
        'FILTER CONTACT
        If Len(kontakAwal) > 0 And Len(kontakAkhir) > 0 Then
            sqlSM &= " AND (k.kkode BETWEEN '" & FixQuotes(kontakAwal) & "' AND '" & FixQuotes(kontakAkhir) & "')"
        ElseIf Len(kontakAwal) > 0 Then
            sqlSM &= " AND (k.kkode >= '" & FixQuotes(kontakAwal) & "')"
        ElseIf Len(kontakAkhir) > 0 Then
            sqlSM &= " AND (k.kkode <= '" & FixQuotes(kontakAkhir) & "')"
        End If
        sqlSM &= " ORDER BY t.ttgl, t.tinputtgl, t.tid"


        'AMBIL DATA SALDO AWAL DAN SALDO MUTASI ######
        sqlSMJadi = "(" & sqlSA & ") UNION (" & sqlSM & ")"
        dt = AsDataTableAmbilDariDB(sqlSMJadi)
        If dt.Rows.Count > 0 Then

            'AMBIL SALDO MASUK, SALDO KELUAR, SALDO AKHIR
            Dim saldomasuk As Double = 0, saldokeluar As Double = 0, saldoakhir As Double = 0
            saldomasuk = AsDataTableDSum(dt, "tdebit")
            saldokeluar = AsDataTableDSum(dt, "tkredit")
            saldoakhir = Double.Parse(dt.Rows(dt.Rows.Count - 1)("tsaldo"))

            'SET PAGING
            If pagingSplit(0) > 0 Or pagingSplit(0) = -1 Then pg1.isPaging = True Else pg1.isPaging = False
            Dim rowStart As Integer = 0, dtJadi As New DataTable

            If pg1.isPaging Then
                'LIMIT LAST PAGE
                If pagingSplit(0) = -1 Then
                    'HITUNG PAGE NUMBER = jmldata/itemlimit
                    pagingSplit(0) = Math.Ceiling((dt.Rows.Count) / pagingSplit(1))
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)

                    'LIMIT SESUAI PAGENUMBER
                ElseIf pagingSplit(0) > 0 Then
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)
                End If
                dtJadi = AsDataTableFilterLimit(dt, "", "", rowStart, pagingSplit(1))

            Else
                dtJadi = dt 'AsDataTableFilterLimit(dt, "", "")
            End If

            If dtJadi.Rows.Count > 0 Then
                For Each dr As DataRow In dtJadi.Rows
                    search = String.Concat(search,
                                 FxDB(dr("tid"), 0), sptField,
                                 FxDB(dr("tsumber"), ""), sptField,
                                 FxDB(dr("tidtransaksi"), 0), sptField,
                                 FxDB(dr("tnotransaksi"), ""), sptField,
                                 AsFormatTanggal(FxDB(dr("ttgl"), ""), formatTgl), sptField,
                                 FxDB(dr("tkontak"), 0), sptField,
                                 FxDB(dr("tkontakkode"), ""), sptField,
                                 FxDB(dr("tkontaknama"), ""), sptField,
                                 FxDB(dr("tnorek"), ""), sptField,
                                 FxDB(dr("tnoreknama"), ""), sptField,
                                 FxDB(dr("tmatauang"), ""), sptField,
                                 FxDB(dr("tkurs"), 0), sptField,
                                 FxDB(dr("tdebit"), ""), sptField,
                                 FxDB(dr("tkredit"), ""), sptField,
                                 FxDB(dr("tdebitvalas"), ""), sptField,
                                 FxDB(dr("tkreditvalas"), ""), sptField,
                                 FxDB(dr("turaian"), ""), sptField,
                                 FxDB(dr("tcatatan"), 0), sptField,
                                 FxDB(dr("tsaldo"), 0), sptField,
                                 FxDB(saldoawal, 0), sptField,
                                 FxDB(saldomasuk, 0), sptField,
                                 FxDB(saldokeluar, 0), sptField,
                                 FxDB(saldoakhir, 0), sptField,
                                 FxDB(dr("tissaldoawal"), 0), sptRow)
                Next
                search = search.Substring(0, search.Length - sptRow.Length)

                result(1) = 1

                If pg1.isPaging Then
                    pg1.isPrev = pagingSplit(0) > 1
                    pg1.isNext = dt.Rows.Count > pagingSplit(0) * pagingSplit(1)

                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(pg1.isNext))
                    resultPaging(2) = Math.Abs(Val(pg1.isPrev))
                    resultPaging(3) = pagingSplit(0)
                    resultPaging(4) = pg1.countRow
                Else
                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(False))
                    resultPaging(2) = Math.Abs(Val(False))
                    resultPaging(3) = 0
                    resultPaging(4) = 0
                End If


            Else
                result(2) = "General Ledger data not found. #2" : GoTo selesai
            End If

        Else
            result(2) = "General Ledger data not found. #1" : GoTo selesai
        End If


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tkontak, tkontakkode, tkontaknama, tnorek, tnoreknama, tmatauang, tkurs, tdebit, tkredit, tdebitvalas, tkreditvalas, turaian, tcatatan, tsaldo, tsaldoawal, tsaldodebit, tsaldokredit, tsaldoakhir, tissaldoawal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_GeneralLedger(ByVal param As String) As String
        'M2_GeneralLedger -----------------------------------------------------------
        'tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tkontak, tkontakkode, tkontaknama, 
        'tnorek, tnoreknama, tmatauang, tkurs, tdebit, tkredit, tdebitvalas, tkreditvalas, 
        'turaian, tcatatan, tsaldo, tsaldoawal, tsaldodebit, tsaldokredit, tsaldoakhir, tissaldoawal

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'tglAwal(0) As String, tglAkhir(1) As String, norekAwal(2) As String, norekAkhir(3) As String
        'kontakAwal(4) As String, kontakAkhir(5) As String, orderBy(6) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'tglAwal, tglAkhir, norekAwal, norekAkhir, 
        'kontakAwal, kontakAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim dt As New DataTable
        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI
        Dim tglAwal As String = "", tglAkhir As String = "", norekAwal As String = "", norekAkhir As String = ""
        Dim kontakAwal As String = "", kontakAkhir As String = "", orderBy As String = ""

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


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 7) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            tglAkhir = AsFormatTanggal(Now)
        End If

        'norekAwal(2) As String
        norekAwal = dataUtama(2)

        'norekAkhir(3) As String
        norekAkhir = dataUtama(3)

        'kontakAwal(4) As String
        kontakAwal = dataUtama(4)

        'kontakAkhir(5) As String
        kontakAkhir = dataUtama(5)

        'orderBy(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataUtama(6).ToString <> "cnomor" And dataUtama(6).ToString <> "cnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataUtama(6)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ==================================================


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

        'TRANSAKSI KE DATABASE
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        'AMBIL DATA DARI SETTING -----------------------------
        Dim matauang As String = "", kurs As String = ""
        Dim dtMatauang As DataTable = AsDataTableAmbilDariDB("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional') OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs')")
        'MATAUANG
        matauang = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'MataUangFungsional'", "Not found")
        If matauang = "Not found" Then
            result(2) = "Setting Functional Currency not found." : GoTo selesai
        End If
        'KURS
        kurs = AsDataTableDLookup(dtMatauang, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'Kurs'", "Not found")
        If kurs = "Not found" Then
            result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
        End If
        'END OF AMBIL DATA DARI SETTING ----------------------


        Dim dtSA As New DataTable, saldoawal As Double = 0
        Dim sqlSA As String = "", sqlSM As String = "", sqlSMGabung As String = ""
        Dim sqlSAJadi As String = "", sqlSMJadi As String = ""


        'QUERY SALDO AWAL ###########################
        '                                    tid,                 tsumber,                             tidtransaksi,                 tnotransaksi,                                 ttgl,      tkontak,       tkontakkode,       tkontaknama,             tnorek,            tnoreknama,                                  tmatauang,                              tkurs,                            tdebit,                             tkredit,                                 tdebitvalas,                                  tkreditvalas,                 turaian,                 tcatatan,                                                                                                                                              tsaldo,      tsaldoawal,      tsaldodebit,      tsaldokredit,      tsaldoakhir,      tissaldoawal
        sqlSA = "  SELECT IFNULL(t.tid,0) as tid, 'Saldo Awal' as tsumber, IFNULL(t.tidtransaksi,0) as tidtransaksi, 'Saldo Awal' as tnotransaksi, '" & FixQuotes(tglAwal) & "' as ttgl, 0 as tkontak, '' as tkontakkode, '' as tkontaknama, c.cnomor as tnorek, c.cnama as tnoreknama, '" & FixQuotes(matauang) & "' as tmatauang, '" & FixDouble(kurs) & "' as tkurs, IFNULL(SUM(t.tdebit),0) as tdebit, IFNULL(SUM(t.tkredit),0) as tkredit, IFNULL(SUM(t.tdebitvalas),0) as tdebitvalas, IFNULL(SUM(t.tkreditvalas),0) as tkreditvalas, 'Saldo Awal' as turaian, 'Saldo Awal' as tcatatan, (CASE c.cdc WHEN 'D' THEN IFNULL(SUM(t.tdebit),0) - IFNULL(SUM(t.tkredit),0) ELSE IFNULL(SUM(t.tkredit),0) - IFNULL(SUM(t.tdebit),0) END) as tsaldo, 0 as tsaldoawal, 0 as tsaldodebit, 0 as tsaldokredit, 0 as tsaldoakhir, 0 as tissaldoawal"
        sqlSA &= " FROM m1_coa c"
        sqlSA &= " LEFT JOIN m2_transaction_journal t ON c.cnomor = t.tnorek AND t.tstatus IN(2,3,4,7) AND t.ttgl < '" & FixQuotes(tglAwal) & "'"
        sqlSA &= " LEFT JOIN m1_contact k ON t.tkontak = k.kid"
        sqlSA &= " WHERE c.cnomor = '" & FixQuotes(norekAwal) & "'"
        'FILTER CONTACT
        If Len(kontakAwal) > 0 And Len(kontakAkhir) > 0 Then
            sqlSA &= " AND (k.kkode BETWEEN '" & FixQuotes(kontakAwal) & "' AND '" & FixQuotes(kontakAkhir) & "')"
        ElseIf Len(kontakAwal) > 0 Then
            sqlSA &= " AND (k.kkode >= '" & FixQuotes(kontakAwal) & "')"
        ElseIf Len(kontakAkhir) > 0 Then
            sqlSA &= " AND (k.kkode <= '" & FixQuotes(kontakAkhir) & "')"
        End If

        'AMBIL SALDO AWAL
        dtSA = AsDataTableAmbilDariDB(sqlSA)
        If dtSA.Rows.Count > 0 Then
            saldoawal = Double.Parse(dtSA.Rows(0)("tsaldo"))
        Else
            saldoawal = 0
        End If

        ''QUERY SALDO MUTASI #########################
        ''                   tid,                     tsumber,                   tidtransaksi,                   tnotransaksi,           ttgl,              tkontak,            tkontakkode,            tkontaknama,             tnorek,            tnoreknama,                tmatauang,            tkurs,             tdebit,              tkredit,                  tdebitvalas,                   tkreditvalas,              turaian,               tcatatan,                                                                                                                                                                tsaldo,      tsaldoawal,      tsaldodebit,      tsaldokredit,      tsaldoakhir,                 tissaldoawal
        'sqlSM = "  SELECT t.tid as tid, t.tsumber as tsumber, t.tidtransaksi as tidtransaksi, t.tnotransaksi as tnotransaksi, t.ttgl as ttgl, t.tkontak as tkontak, k.kkode as tkontakkode, k.knama as tkontaknama, c.cnomor as tnorek, c.cnama as tnoreknama, t.tmatauang as tmatauang, t.tkurs as tkurs, t.tdebit as tdebit, t.tkredit as tkredit, t.tdebitvalas as tdebitvalas, t.tkreditvalas as tkreditvalas, t.turaian as turaian, t.tcatatan as tcatatan, (CASE c.cdc WHEN 'D' THEN @saldo := @saldo + IFNULL(t.tdebit,0) - IFNULL(t.tkredit,0) ELSE @saldo := @saldo + IFNULL(t.tkredit,0) - IFNULL(t.tdebit,0) END) as tsaldo, 0 as tsaldoawal, 0 as tsaldodebit, 0 as tsaldokredit, 0 as tsaldoakhir, t.tsaldoawal as tissaldoawal"
        'sqlSM &= " FROM m2_transaction_journal t"
        'sqlSM &= " JOIN m1_coa c ON t.tnorek = c.cnomor"
        'sqlSM &= " JOIN m1_contact k ON t.tkontak = k.kid"
        'sqlSM &= " , (SELECT @saldo := " & FixDouble(saldoawal) & ") AS variableInit1"
        'sqlSM &= " WHERE t.tstatus IN(2,3,4,7) AND t.tnorek = '" & FixQuotes(norekAwal) & "'"
        'sqlSM &= " AND t.ttgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "'"
        ''FILTER CONTACT
        'If Len(kontakAwal) > 0 And Len(kontakAkhir) > 0 Then
        '    sqlSM &= " AND (k.kkode BETWEEN '" & FixQuotes(kontakAwal) & "' AND '" & FixQuotes(kontakAkhir) & "')"
        'ElseIf Len(kontakAwal) > 0 Then
        '    sqlSM &= " AND (k.kkode >= '" & FixQuotes(kontakAwal) & "')"
        'ElseIf Len(kontakAkhir) > 0 Then
        '    sqlSM &= " AND (k.kkode <= '" & FixQuotes(kontakAkhir) & "')"
        'End If
        'sqlSM &= " ORDER BY t.ttgl, t.tinputtgl, t.tid"

        'QUERY SALDO MUTASI #########################
        sqlSM = "SELECT bb.tid as tid, bb.tsumber as tsumber, bb.tidtransaksi as tidtransaksi, bb.tnotransaksi as tnotransaksi, bb.ttgl as ttgl, bb.tkontak as tkontak, bb.tkontakkode as tkontakkode, bb.tkontaknama as tkontaknama, bb.tnorek as tnorek, bb.tnoreknama as tnoreknama, bb.tmatauang as tmatauang, bb.tkurs as tkurs, bb.tdebit as tdebit, bb.tkredit as tkredit, bb.tdebitvalas as tdebitvalas, bb.tkreditvalas as tkreditvalas, bb.turaian as turaian, bb.tcatatan as tcatatan, (CASE bb.cdc WHEN 'D' THEN @saldo := @saldo + IFNULL(bb.tdebit,0) - IFNULL(bb.tkredit,0) ELSE @saldo := @saldo + IFNULL(bb.tkredit,0) - IFNULL(bb.tdebit,0) END) as tsaldo, 0 as tsaldoawal, 0 as tsaldodebit, 0 as tsaldokredit, 0 as tsaldoakhir, bb.tsaldoawal as tissaldoawal"
        sqlSM &= " FROM ("
        '                   tid,                     tsumber,                   tidtransaksi,                   tnotransaksi,           ttgl,              tkontak,            tkontakkode,            tkontaknama,             tnorek,            tnoreknama,                tmatauang,            tkurs,             tdebit,              tkredit,                  tdebitvalas,                   tkreditvalas,              turaian,               tcatatan,   cdc,                                                                                                                                                               tsaldo,      tsaldoawal,      tsaldodebit,      tsaldokredit,      tsaldoakhir,                 tissaldoawal
        sqlSM &= " SELECT t.tid as tid, t.tsumber as tsumber, t.tidtransaksi as tidtransaksi, t.tnotransaksi as tnotransaksi, t.ttgl as ttgl, t.tkontak as tkontak, k.kkode as tkontakkode, k.knama as tkontaknama, c.cnomor as tnorek, c.cnama as tnoreknama, t.tmatauang as tmatauang, t.tkurs as tkurs, t.tdebit as tdebit, t.tkredit as tkredit, t.tdebitvalas as tdebitvalas, t.tkreditvalas as tkreditvalas, t.turaian as turaian, t.tcatatan as tcatatan, c.cdc, (CASE c.cdc WHEN 'D' THEN @saldo := @saldo + IFNULL(t.tdebit,0) - IFNULL(t.tkredit,0) ELSE @saldo := @saldo + IFNULL(t.tkredit,0) - IFNULL(t.tdebit,0) END) as tsaldo, 0 as tsaldoawal, 0 as tsaldodebit, 0 as tsaldokredit, 0 as tsaldoakhir, t.tsaldoawal as tissaldoawal"
        sqlSM &= " FROM m2_transaction_journal t"
        sqlSM &= " JOIN m1_coa c ON t.tnorek = c.cnomor"
        sqlSM &= " JOIN m1_contact k ON t.tkontak = k.kid"
        sqlSM &= " WHERE t.tstatus IN(2,3,4,7) AND t.tnorek = '" & FixQuotes(norekAwal) & "'"
        sqlSM &= " AND t.ttgl BETWEEN '" & FixQuotes(tglAwal) & "' AND '" & FixQuotes(tglAkhir) & "'"
        'FILTER CONTACT
        If Len(kontakAwal) > 0 And Len(kontakAkhir) > 0 Then
            sqlSM &= " AND (k.kkode BETWEEN '" & FixQuotes(kontakAwal) & "' AND '" & FixQuotes(kontakAkhir) & "')"
        ElseIf Len(kontakAwal) > 0 Then
            sqlSM &= " AND (k.kkode >= '" & FixQuotes(kontakAwal) & "')"
        ElseIf Len(kontakAkhir) > 0 Then
            sqlSM &= " AND (k.kkode <= '" & FixQuotes(kontakAkhir) & "')"
        End If
        sqlSM &= " ORDER BY t.ttgl, t.tinputtgl, t.tid"
        sqlSM &= " ) as bb"
        sqlSM &= " , (SELECT @saldo := " & FixDouble(saldoawal) & ") AS variableInit1"

        'AMBIL DATA SALDO AWAL DAN SALDO MUTASI ######
        sqlSMJadi = "(" & sqlSA & ") UNION (" & sqlSM & ")"
        dt = AsDataTableAmbilDariDB(sqlSMJadi)
        If dt.Rows.Count > 0 Then

            'AMBIL SALDO MASUK, SALDO KELUAR, SALDO AKHIR
            Dim saldomasuk As Double = 0, saldokeluar As Double = 0, saldoakhir As Double = 0
            saldomasuk = AsDataTableDSum(dt, "tdebit")
            saldokeluar = AsDataTableDSum(dt, "tkredit")
            saldoakhir = Double.Parse(dt.Rows(dt.Rows.Count - 1)("tsaldo"))

            'SET PAGING
            If pagingSplit(0) > 0 Or pagingSplit(0) = -1 Then pg1.isPaging = True Else pg1.isPaging = False
            Dim rowStart As Integer = 0, dtJadi As New DataTable

            If pg1.isPaging Then
                'LIMIT LAST PAGE
                If pagingSplit(0) = -1 Then
                    'HITUNG PAGE NUMBER = jmldata/itemlimit
                    pagingSplit(0) = Math.Ceiling((dt.Rows.Count) / pagingSplit(1))
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)

                    'LIMIT SESUAI PAGENUMBER
                ElseIf pagingSplit(0) > 0 Then
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)
                End If
                dtJadi = AsDataTableFilterLimit(dt, "", "", rowStart, pagingSplit(1))

            Else
                dtJadi = dt 'AsDataTableFilterLimit(dt, "", "")
            End If

            If dtJadi.Rows.Count > 0 Then
                For Each dr As DataRow In dtJadi.Rows
                    search = String.Concat(search,
                                 FxDB(dr("tid"), 0), sptField,
                                 FxDB(dr("tsumber"), ""), sptField,
                                 FxDB(dr("tidtransaksi"), 0), sptField,
                                 FxDB(dr("tnotransaksi"), ""), sptField,
                                 AsFormatTanggal(FxDB(dr("ttgl"), ""), formatTgl), sptField,
                                 FxDB(dr("tkontak"), 0), sptField,
                                 FxDB(dr("tkontakkode"), ""), sptField,
                                 FxDB(dr("tkontaknama"), ""), sptField,
                                 FxDB(dr("tnorek"), ""), sptField,
                                 FxDB(dr("tnoreknama"), ""), sptField,
                                 FxDB(dr("tmatauang"), ""), sptField,
                                 FxDB(dr("tkurs"), 0), sptField,
                                 FxDB(dr("tdebit"), ""), sptField,
                                 FxDB(dr("tkredit"), ""), sptField,
                                 FxDB(dr("tdebitvalas"), ""), sptField,
                                 FxDB(dr("tkreditvalas"), ""), sptField,
                                 FxDB(dr("turaian"), ""), sptField,
                                 FxDB(dr("tcatatan"), 0), sptField,
                                 FxDB(dr("tsaldo"), 0), sptField,
                                 FxDB(saldoawal, 0), sptField,
                                 FxDB(saldomasuk, 0), sptField,
                                 FxDB(saldokeluar, 0), sptField,
                                 FxDB(saldoakhir, 0), sptField,
                                 FxDB(dr("tissaldoawal"), 0), sptRow)
                Next
                search = search.Substring(0, search.Length - sptRow.Length)

                result(1) = 1

                If pg1.isPaging Then
                    pg1.isPrev = pagingSplit(0) > 1
                    pg1.isNext = dt.Rows.Count > pagingSplit(0) * pagingSplit(1)

                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(pg1.isNext))
                    resultPaging(2) = Math.Abs(Val(pg1.isPrev))
                    resultPaging(3) = pagingSplit(0)
                    resultPaging(4) = pg1.countRow
                Else
                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(False))
                    resultPaging(2) = Math.Abs(Val(False))
                    resultPaging(3) = 0
                    resultPaging(4) = 0
                End If


            Else
                result(2) = "General Ledger data not found. #2" : GoTo selesai
            End If

        Else
            result(2) = "General Ledger data not found. #1" : GoTo selesai
        End If


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tkontak, tkontakkode, tkontaknama, tnorek, tnoreknama, tmatauang, tkurs, tdebit, tkredit, tdebitvalas, tkreditvalas, turaian, tcatatan, tsaldo, tsaldoawal, tsaldodebit, tsaldokredit, tsaldoakhir, tissaldoawal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_Data_Journal_VoucherSearch(ByVal param As String) As String
        'M2_Data_Journal_VoucherSearch --------------------------------------------------------
        'tidtransaksi, tsumber, tnotransaksi, ttgl, turaian, tdebit, tkontak, 
        'tkontakkode, tkontaknama, tmatauang, tkurs, tinputtgl, tinputuser, tinputuserkode, tinputusernama, 
        'tstatus, tstatusnama, tmodifikasitgl, tmodifikasiuser, tmodifikasiuserkode, tmodifikasiusernama, tsaldoawal

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
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
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
            result(2) = "Access denied for get data"
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
            Filter = Filter.Replace("tkontakkode", "c.kkode")
            Filter = Filter.Replace("tinputuserkode", "ui.ukode")
            Filter = Filter.Replace("tstatusnama", "s.nama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        sql = "SELECT `tj`.`tidtransaksi` AS `tidtransaksi`,`tj`.`tsumber` AS `tsumber`,`tj`.`tnotransaksi` AS `tnotransaksi`,`tj`.`ttgl` AS `ttgl`,`tj`.`turaian` AS `turaian`,SUM(`tj`.`tdebit`) AS `tdebit`,`tj`.`tkontak` AS `tkontak`,`c`.`kkode` AS `tkontakkode`,`c`.`knama` AS `tkontaknama`,`tj`.`tmatauang` AS `tmatauang`,`tj`.`tkurs` AS `tkurs`,`tj`.`tinputtgl` AS `tinputtgl`,`tj`.`tinputuser` AS `tinputuser`,`ui`.`ukode` AS `tinputuserkode`,`ui`.`unama` AS `tinputusernama`,`tj`.`tstatus` AS `tstatus`,`s`.`nama` AS `tstatusnama`,`tj`.`tmodifikasitgl` AS `tmodifikasitgl`,`tj`.`tmodifikasiuser` AS `tmodifikasiuser`,`um`.`ukode` AS `tmodifikasiuserkode`,`um`.`unama` AS `tmodifikasiusernama`, tj.tsaldoawal as tsaldoawal FROM `m2_transaction_journal` `tj` LEFT JOIN `m1_contact` `c` ON `c`.`kid` = `tj`.`tkontak` LEFT JOIN `m0_user` `ui` ON `ui`.`userid` = `tj`.`tinputuser` LEFT JOIN `m0_user` `um` ON `um`.`userid` = `tj`.`tmodifikasiuser` LEFT JOIN `m0_status` `s` ON `s`.`kode` = `tj`.`tstatus`"

        dt = AmbilData("aplikasi1-M2_Data_Journal_Voucher", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "tsumber, tidtransaksi", sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("tidtransaksi"), 0), sptField,
                             FxDB(dr("tsumber"), ""), sptField,
                             FxDB(dr("tnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("ttgl"), ""), formatTgl), sptField,
                             FxDB(dr("turaian"), ""), sptField,
                             FxDB(dr("tdebit"), ""), sptField,
                             FxDB(dr("tkontak"), 0), sptField,
                             FxDB(dr("tkontakkode"), ""), sptField,
                             FxDB(dr("tkontaknama"), ""), sptField,
                             FxDB(dr("tmatauang"), ""), sptField,
                             FxDB(dr("tkurs"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("tinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("tinputuser"), ""), sptField,
                             FxDB(dr("tinputuserkode"), ""), sptField,
                             FxDB(dr("tinputusernama"), ""), sptField,
                             FxDB(dr("tstatus"), 0), sptField,
                             FxDB(dr("tstatusnama"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("tmodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("tmodifikasiuser"), ""), sptField,
                             FxDB(dr("tmodifikasiuserkode"), ""), sptField,
                             FxDB(dr("tmodifikasiusernama"), ""), sptField,
                             FxDB(dr("tsaldoawal"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Data Journal data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tidtransaksi, tsumber, tnotransaksi, ttgl, turaian, tdebit, tkontak, tkontakkode, tkontaknama, tmatauang, tkurs, tinputtgl, tinputuser, tinputuserkode, tinputusernama, tstatus, tstatusnama, tmodifikasitgl, tmodifikasiuser, tmodifikasiuserkode, tmodifikasiusernama, tsaldoawal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_Data_Item_TransactionSearch(ByVal param As String) As String
        'M2_Data_Item_TransactionSearch --------------------------------------------------------
        'itid, itcabang, itcabangnama, itlokasi, itlokasinama, itgudang, itgudangnama,
        'itjenismutasi, itjenismutasinama, itsumber, itidutama, itiddetail, itnotransaksi, ittgl,
        'itkontak, itkontakkode, itkontaknama, itidbarang, itkodebarang, itnamabarang, ittipebarang,
        'ittipehpp, itjmlbarang, itsatuanbarang, itmatauang, itkurs, itharga, itdiskon,
        'itjmldiskon, itidhppikm, itidhppikk, itidhppfifo, ithpp, ituraian, itcatatan,
        'itcatatandetail, itinputtgl, itinputuser, itinputuserkode, itinputusernama, ittotal

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
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
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
            result(2) = "Access denied for get data"
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
            Filter = Filter.Replace("itnotransaksi", "it.notransaksi")
            Filter = Filter.Replace("ittgl", "it.tgl")
            Filter = Filter.Replace("itkodebarang", "i.bkode")
            Filter = Filter.Replace("ituraian", "it.uraian")
            Filter = Filter.Replace("itkontakkode", "c.kkode")
            Filter = Filter.Replace("itgudang", "it.gudang")
            Filter = Filter.Replace("itsatuanbarang", "it.satuanbarang")
            Filter = Filter.Replace("itsumber", "it.sumber")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
            Sorting = Sorting.Replace("itinputtgl", "it.inputtgl")
        End If

        'PANGGIL QUERY
        sql = "SELECT `it`.`id` AS `itid`,`it`.`cabang` AS `itcabang`,`b`.`bnama` AS `itcabangnama`,`it`.`lokasi` AS `itlokasi`,`l`.`lnama` AS `itlokasinama`,`it`.`gudang` AS `itgudang`,`w`.`wnama` AS `itgudangnama`,`it`.`jenismutasi` AS `itjenismutasi`,(CASE `jenismutasi` WHEN 1 THEN 'Masuk' ELSE 'Keluar' END) AS `itjenismutasinama`,`it`.`sumber` AS `itsumber`,`it`.`idutama` AS `itidutama`,`it`.`iddetail` AS `itiddetail`,`it`.`notransaksi` AS `itnotransaksi`,`it`.`tgl` AS `ittgl`,`it`.`kontak` AS `itkontak`,`c`.`kkode` AS `itkontakkode`,`c`.`knama` AS `itkontaknama`,`it`.`idbarang` AS `itidbarang`,`i`.`bkode` AS `itkodebarang`,`it`.`namabarang` AS `itnamabarang`,`it`.`tipebarang` AS `ittipebarang`,`it`.`tipehpp` AS `ittipehpp`,`it`.`jmlbarang` AS `itjmlbarang`,`it`.`satuanbarang` AS `itsatuanbarang`,`it`.`matauang` AS `itmatauang`,`it`.`kurs` AS `itkurs`,`it`.`harga` AS `itharga`,`it`.`diskon` AS `itdiskon`,`it`.`jmldiskon` AS `itjmldiskon`,`it`.`idhppikm` AS `itidhppikm`,`it`.`idhppikk` AS `itidhppikk`,`it`.`idhppfifo` AS `itidhppfifo`,`it`.`hpp` AS `ithpp`,`it`.`uraian` AS `ituraian`,`it`.`catatan` AS `itcatatan`,`it`.`catatandetail` AS `itcatatandetail`,`it`.`inputtgl` AS `itinputtgl`,`it`.`inputuser` AS `itinputuser`,`u`.`ukode` AS `itinputuserkode`,`u`.`unama` AS `itinputusernama`,((`it`.`jmlbarang` * `it`.`harga`) - `it`.`jmldiskon`) * `it`.`kurs` AS `ittotal`FROM ((((((`m1_item_transaction` `it` LEFT JOIN `m1_branch` `b` ON `b`.`bkode` = `it`.`cabang`) LEFT JOIN `m1_location` `l` ON `l`.`lkode` = `it`.`lokasi`) LEFT JOIN `m1_warehouse` `w` ON `w`.`wkode` = `it`.`gudang`) LEFT JOIN `m1_contact` `c` ON `c`.`kid` = `it`.`kontak`) LEFT JOIN `m0_user` `u` ON `u`.`userid` = `it`.`inputuser`)LEFT JOIN `m1_item` `i` ON `i`.`bid` = `it`.`idbarang`)"

        dt = AmbilData("aplikasi1-M2_Data_Journal_Voucher", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("itid"), 0), sptField,
                             FxDB(dr("itcabang"), ""), sptField,
                             FxDB(dr("itcabangnama"), ""), sptField,
                             FxDB(dr("itlokasi"), ""), sptField,
                             FxDB(dr("itlokasinama"), ""), sptField,
                             FxDB(dr("itgudang"), ""), sptField,
                             FxDB(dr("itgudangnama"), ""), sptField,
                             FxDB(dr("itjenismutasi"), 0), sptField,
                             FxDB(dr("itjenismutasinama"), ""), sptField,
                             FxDB(dr("itsumber"), ""), sptField,
                             FxDB(dr("itidutama"), 0), sptField,
                             FxDB(dr("itiddetail"), 0), sptField,
                             FxDB(dr("itnotransaksi"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("ittgl"), ""), formatTgl), sptField,
                             FxDB(dr("itkontak"), 0), sptField,
                             FxDB(dr("itkontakkode"), ""), sptField,
                             FxDB(dr("itkontaknama"), ""), sptField,
                             FxDB(dr("itidbarang"), 0), sptField,
                             FxDB(dr("itkodebarang"), ""), sptField,
                             FxDB(dr("itnamabarang"), ""), sptField,
                             FxDB(dr("ittipebarang"), ""), sptField,
                             FxDB(dr("ittipehpp"), 0), sptField,
                             FxDB(dr("itjmlbarang"), 0), sptField,
                             FxDB(dr("itsatuanbarang"), ""), sptField,
                             FxDB(dr("itmatauang"), ""), sptField,
                             FxDB(dr("itkurs"), 0), sptField,
                             FxDB(dr("itharga"), 0), sptField,
                             FxDB(dr("itdiskon"), 0), sptField,
                             FxDB(dr("itjmldiskon"), 0), sptField,
                             FxDB(dr("itidhppikm"), 0), sptField,
                             FxDB(dr("itidhppikk"), 0), sptField,
                             FxDB(dr("itidhppfifo"), 0), sptField,
                             FxDB(dr("ithpp"), 0), sptField,
                             FxDB(dr("ituraian"), ""), sptField,
                             FxDB(dr("itcatatan"), ""), sptField,
                             FxDB(dr("itcatatandetail"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("itinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("itinputuser"), 0), sptField,
                             FxDB(dr("itinputuserkode"), ""), sptField,
                             FxDB(dr("itinputusernama"), ""), sptField,
                             FxDB(dr("ittotal"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Data Item Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("itid,itcabang,itcabangnama,itlokasi,itlokasinama,itgudang,itgudangnama,itjenismutasi,itjenismutasinama,itsumber,itidutama,itiddetail,itnotransaksi,ittgl,itkontak,itkontakkode,itkontaknama,itidbarang,itkodebarang,itnamabarang,ittipebarang,ittipehpp,itjmlbarang,itsatuanbarang,itmatauang,itkurs,itharga,itdiskon,itjmldiskon,itidhppikm,itidhppikk,itidhppfifo,ithpp,ituraian,itcatatan,itcatatandetail,itinputtgl,itinputuser,itinputuserkode,itinputusernama,ittotal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_KartuSerial(ByVal param As String) As String
        'M2_KartuSerial -----------------------------------------------------------
        'sumber, idutama, gudang, bkode, jenismutasi, tgl, notransaksi, kodepa, namabarang, 
        'jmlmasuk, jmlkeluar, uraian, nstkode, totalmasuk, totalkeluar

        'MAPPING BUAT FLEX --------------------------------------------------------
        'gudangAwal, gudangAkhir, kodeAwal, kodeAkhir, serialAwal, serialAkhir

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim dt As New DataTable
        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI
        Dim gudangAwal As String = "", gudangAkhir As String = ""
        Dim kodeAwal As String = "", kodeAkhir As String = ""
        Dim serialAwal As String = "", serialAkhir As String = ""

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


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 6) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA =========================================================
        'gudangAwal
        gudangAwal = dataUtama(0)

        'gudangAkhir
        gudangAkhir = dataUtama(1)

        'kodeAwal
        kodeAwal = dataUtama(2)

        'kodeAkhir
        kodeAkhir = dataUtama(3)

        'serialAwal
        serialAwal = dataUtama(4)

        'serialAkhir
        serialAkhir = dataUtama(5)
        'END OF VALIDASI TIPE DATA UTAMA ==================================================


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

        'TRANSAKSI KE DATABASE
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim sqlSM As String = "", sqlfilter As String = ""
        Dim totalmasuk As Double = 0, totalkeluar As Double = 0

        'FILTER #####################################
        'FILTER GUDANG
        If Len(gudangAwal) > 0 And Len(gudangAkhir) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (it.gudang BETWEEN '" & FixQuotes(gudangAwal) & "' AND '" & FixQuotes(gudangAkhir) & "')"
        ElseIf Len(gudangAwal) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (it.gudang >= '" & FixQuotes(gudangAwal) & "')"
        ElseIf Len(gudangAkhir) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (it.gudang <= '" & FixQuotes(gudangAkhir) & "')"
        End If

        'FILTER KODEBARANG
        If Len(kodeAwal) > 0 And Len(kodeAkhir) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (i.bkode BETWEEN '" & FixQuotes(kodeAwal) & "' AND '" & FixQuotes(kodeAkhir) & "')"
        ElseIf Len(kodeAwal) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (i.bkode >= '" & FixQuotes(kodeAwal) & "')"
        ElseIf Len(kodeAkhir) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (i.bkode <= '" & FixQuotes(kodeAkhir) & "')"
        End If

        'FILTER NO SERIAL
        If Len(serialAwal) > 0 And Len(serialAkhir) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (nst.nstkode BETWEEN '" & FixQuotes(serialAwal) & "' AND '" & FixQuotes(serialAkhir) & "')"
        ElseIf Len(serialAwal) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (nst.nstkode >= '" & FixQuotes(serialAwal) & "')"
        ElseIf Len(serialAkhir) > 0 Then
            If Len(sqlfilter) > 0 Then sqlfilter &= " AND "
            sqlfilter &= " (nst.nstkode <= '" & FixQuotes(serialAkhir) & "')"
        End If

        'QUERY SALDO MUTASI #########################
        sqlSM = "  SELECT it.sumber, it.idutama, it.gudang, i.bkode, it.jenismutasi, it.tgl, it.notransaksi, it.kodepa, it.namabarang, (CASE it.jenismutasi WHEN 1 THEN nst.nstjml WHEN 0 then 0 END) AS jmlmasuk, (CASE it.jenismutasi WHEN 1 THEN 0 WHEN 0 THEN nst.nstjml END) AS jmlkeluar, it.uraian, nst.nstkode"
        sqlSM &= " FROM m1_item_transaction it"
        sqlSM &= " JOIN m1_item i ON it.idbarang = i.bid AND i.bserial = 1"
        sqlSM &= " JOIN m1_no_serial_transaction nst ON it.sumber = nst.nstsumber"
        sqlSM &= " AND it.idutama = nst.nstidtransaksi"
        sqlSM &= " AND it.idbarang = nst.nstidbarang "
        sqlSM &= " AND it.jenismutasi = nst.nstjenismutasi"
        If Len(sqlfilter) > 0 Then
            sqlSM &= " WHERE " & sqlfilter
        End If
        sqlSM &= " ORDER BY it.gudang ASC, i.bkode ASC, it.tgl ASC, it.jenismutasi DESC, it.sumber ASC, it.id ASC, nst.nstid ASC"


        'AMBIL DATA SALDO AWAL DAN SALDO MUTASI ######
        dt = AsDataTableAmbilDariDB(sqlSM)
        If dt.Rows.Count > 0 Then

            'SET TOTAL
            totalmasuk = AsDataTableDSum(dt, "jmlmasuk")
            totalkeluar = AsDataTableDSum(dt, "jmlkeluar")

            'SET PAGING
            If pagingSplit(0) > 0 Or pagingSplit(0) = -1 Then pg1.isPaging = True Else pg1.isPaging = False
            Dim rowStart As Integer = 0, dtJadi As New DataTable

            If pg1.isPaging Then
                'LIMIT LAST PAGE
                If pagingSplit(0) = -1 Then
                    'HITUNG PAGE NUMBER = jmldata/itemlimit
                    pagingSplit(0) = Math.Ceiling((dt.Rows.Count) / pagingSplit(1))
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)

                    'LIMIT SESUAI PAGENUMBER
                ElseIf pagingSplit(0) > 0 Then
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)
                End If
                dtJadi = AsDataTableFilterLimit(dt, "", "", rowStart, pagingSplit(1))

            Else
                dtJadi = dt 'AsDataTableFilterLimit(dt, "", "")
            End If

            If dtJadi.Rows.Count > 0 Then
                For Each dr As DataRow In dtJadi.Rows
                    search = String.Concat(search,
                                 FxDB(dr("sumber"), ""), sptField,
                                 FxDB(dr("idutama"), 0), sptField,
                                 FxDB(dr("gudang"), ""), sptField,
                                 FxDB(dr("bkode"), ""), sptField,
                                 FxDB(dr("jenismutasi"), 0), sptField,
                                 AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                                 FxDB(dr("notransaksi"), ""), sptField,
                                 FxDB(dr("kodepa"), 0), sptField,
                                 FxDB(dr("namabarang"), ""), sptField,
                                 FxDB(dr("jmlmasuk"), 0), sptField,
                                 FxDB(dr("jmlkeluar"), 0), sptField,
                                 FxDB(dr("uraian"), ""), sptField,
                                 FxDB(dr("nstkode"), ""), sptField,
                                 FxDB(totalmasuk, 0), sptField,
                                 FxDB(totalkeluar, 0), sptRow)
                Next
                search = search.Substring(0, search.Length - sptRow.Length)

                result(1) = 1

                If pg1.isPaging Then
                    pg1.isPrev = pagingSplit(0) > 1
                    pg1.isNext = dt.Rows.Count > pagingSplit(0) * pagingSplit(1)

                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(pg1.isNext))
                    resultPaging(2) = Math.Abs(Val(pg1.isPrev))
                    resultPaging(3) = pagingSplit(0)
                    resultPaging(4) = pg1.countRow
                Else
                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(False))
                    resultPaging(2) = Math.Abs(Val(False))
                    resultPaging(3) = 0
                    resultPaging(4) = 0
                End If


            Else
                result(2) = "Serial Card data not found. #2" : GoTo selesai
            End If

        Else
            result(2) = "Serial Card data not found. #1" : GoTo selesai
        End If


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sumber, idutama, gudang, bkode, jenismutasi, tgl, notransaksi, kodepa, namabarang, jmlmasuk, jmlkeluar, uraian, nstkode, totalmasuk, totalkeluar"))

        Return wsResult
    End Function

End Class