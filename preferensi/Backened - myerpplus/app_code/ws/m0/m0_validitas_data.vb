Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports System.Net
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
Imports MySql.Data.MySqlClient

' Catatan :
' Hitung ulang jurnal ppv belum ada jadi hitung ulang ppv di tiadakan juga.
' Modul 11 tidak cek jurnalnya, karena tidak ada hitung ulang jurnalnya

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_validitas_data
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim errstring As String = ""

    <WebMethod()>
    Public Function m0_cek_perpaket(ByVal param As String) As String
        'M0_NotesSearch --------------------------------------------------------
        'nsumber, nidtransaksi, ncatatan
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", searchdata As String = ""

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

        Dim paket, p1, p2 As String
        paket = paramSplit(5).Split(sptSubParam)(0)
        p1 = paramSplit(5).Split(sptSubParam)(1)
        p2 = paramSplit(5).Split(sptSubParam)(2)

        'Using client = New WebClient()
        '    client.DownloadFile("http://converter.telerik.com/App_Themes/images/telerik-code-converter.png", "\\192.168.1.36\d\a.jpg")
        'End Using
        'GoTo selesai

        Dim myConn As New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction
        'Dim da As MySqlDataAdapter
        Dim drJurnal() As DataRow, dr() As DataRow
        Dim nocategory As Integer = 0
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            Dim dtNomor, dtTrans, dtTransDetail, dtTransBarang, dtJurnal As New DataTable

            Dim moduleid As String = "", kodetabel As String = "", namatabel As String = "", utamaid As String = ""
            Dim tidakAdaJurnal As Integer = 0, tidakAdaTransBarang As Integer = 0, JurnalAdaPosting As Integer = 0, TransBarangAdaPosting As Integer = 0


            'AMBIL DATA DARI SETTING -----------------------------
            Dim dtSetting As New DataTable, grupJurnalPos As String = ""
            dtSetting = AsDataTableAmbilDariDB("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'GrupJurnalPOS')")

            'GRUP JURNAL POS
            grupJurnalPos = AsDataTableDLookup(dtSetting, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'GrupJurnalPOS'", "Not found")
            If grupJurnalPos = "Not found" Or Len(grupJurnalPos) = 0 Then
                result(2) = "Setting GrupJurnalPOS not found." : GoTo selesai
            Else
                grupJurnalPos = "simatauang, sikurs, sihargatermasukpajak" & IIf(Len(grupJurnalPos) > 0, ", ", "") & grupJurnalPos
            End If
            'END OF AMBIL DATA DARI SETTING ----------------------


            Select Case paket
                Case "TransaksiBarangDobel"
                    nocategory = 5
                    dt = ambilData("SELECT notransaksi, id, sumber, jenismutasi, idutama, iddetail, bnama FROM m1_item_transaction JOIN m1_item ON bid = idbarang WHERE " + f_TglFilter("tgl", p1, p2) + " GROUP BY sumber, jenismutasi, notransaksi, idutama, iddetail, gudang HAVING COUNT(1) <> 1", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + "|" + dt.Rows(i)(5).ToString + "|" + dt.Rows(i)(6).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "JurnalDobel"
                    nocategory = 6
                    dt = ambilData("SELECT  tnotransaksi, tsumber, tidtransaksi, tnorek, tdebit, tkredit, turaian, tcatatan, turutan, tid FROM `m2_transaction_journal` WHERE " + f_TglFilter("ttgl", p1, p2) + " GROUP BY tsumber, tidtransaksi, tnorek, tdebit, tkredit, turaian, tcatatan, turutan HAVING COUNT(1) <> 1", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + "|" + dt.Rows(i)(5).ToString + "|" + dt.Rows(i)(6).ToString + "|" + dt.Rows(i)(7).ToString + "|" + dt.Rows(i)(8).ToString + "|" + dt.Rows(i)(9).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "SaldoJMLHPPMinus"
                    nocategory = 7
                    dt = ambilData("SELECT id, namabarang, saldohpp, saldojml  FROM m1_item_transaction WHERE (saldojml < 0 OR saldohpp < 0)  AND " + f_TglFilter("tgl", p1, p2) + " GROUP BY idbarang", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "StokTBvsstokpergudang"
                    nocategory = 8
                    dt = ambilData("SELECT idbarang, i.bnama, gudang1, SUM(stok1) stok1, SUM(stok2) stok2 FROM (SELECT b.bid idbarang, tb.gudang gudang1,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stok1, 0 stok2	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang UNION SELECT idbarang, kgudang gudang1, 0 stok, stok stok2 FROM m1_item_stock_warehouse) h JOIN m1_item i ON i.bid = h.idbarang GROUP BY h.idbarang, gudang1 HAVING stok1 <> stok2", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "StokPergudangvsStokGlobal"
                    nocategory = 9
                    dt = ambilData("SELECT idbarang, h.bnama, ROUND(SUM(iswstok), 5) stok1, SUM(bstok) stok2 FROM (SELECT idbarang, '' bnama, SUM(stok) iswstok, 0 bstok FROM m1_item_stock_warehouse GROUP BY idbarang UNION SELECT bid, bnama, 0 iswstok, bstok bstok FROM m1_item) h GROUP BY h.idbarang HAVING stok1 <> stok2", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "StokGlobalMinus"
                    nocategory = 10
                    dt = ambilData("SELECT bid, bnama, bstok FROM m1_item WHERE bstok < 0", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "StokPergudangMinus"
                    nocategory = 11
                    dt = ambilData("SELECT idbarang, bnama, stok FROM m1_item_stock_warehouse JOIN m1_item ON bid = idbarang WHERE stok < 0", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "Akunjurnaltidakadadicoa"
                    nocategory = 12
                    dt = ambilData("SELECT tnorek, tid, tnotransaksi FROM m2_transaction_journal LEFT JOIN m1_coa ON tnorek = cnomor WHERE cid IS NULL AND " + f_TglFilter("ttgl", p1, p2), myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "JurnalTidakImbang"
                    nocategory = 13

                    'BUAT FILTER NOTRANSAKSI JURNAL TRANSAKSI UNTUK TRANSAKSI POS
                    Dim ftGroupJurnal As String = grupJurnalPos
                    Dim fieldGrup As String() = Replace(grupJurnalPos, " ", "").Split(",")
                    For j As Integer = 0 To fieldGrup.Length - 1
                        If fieldGrup(j).ToLower.Equals("sihargatermasukpajak") Then
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tadjustment")
                        ElseIf fieldGrup(j).ToLower.Equals("sicustomer") Then
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tkontak")
                        Else
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "t" & Right(fieldGrup(j), fieldGrup(j).Length - 2))
                        End If
                    Next

                    'sumber, idtransaksi, notransaksi, debit, kredit, selisih
                    dt = ambilData("SELECT tsumber, tidtransaksi, tnotransaksi, ROUND(SUM(tdebit), 2) debit, ROUND(SUM(tkredit), 2) kredit, ROUND(SUM(tdebit - tkredit), 2) selisih, " & ftGroupJurnal & "  FROM m2_transaction_journal WHERE tsaldoawal = 0 AND " + f_TglFilter("ttgl", p1, p2) + " GROUP BY tnotransaksi HAVING selisih > 3 OR selisih < -3", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        If dt.Rows(i)(0) = "POS" Then
                            Dim idTransaksi As String = ""
                            For j As Integer = 0 To fieldGrup.Length - 1
                                idTransaksi &= IIf(Len(idTransaksi) > 0, " AND ", "")
                                If fieldGrup(j).Contains("tgl") Then
                                    idTransaksi &= fieldGrup(j) & " = '" & FixQuotes(AsFormatTanggal(dt.Rows(i)(j + 6))) & "'"
                                Else
                                    idTransaksi &= fieldGrup(j) & " = '" & FixQuotes(dt.Rows(i)(j + 6)) & "'"
                                End If
                            Next
                            searchdata += dt.Rows(i)(0) + "|" + idTransaksi + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + sptLogin
                        Else
                            searchdata += dt.Rows(i)(0) + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + sptLogin
                        End If
                    Next

                    If searchdata.Length > 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "Transaksisisanolkomasekian"
                    nocategory = 14
                    dt = ambilData("SELECT sinotransaksi, siid, sitotal, sijmlbayar FROM m5_si WHERE (sitotal-sijmlbayar) < 1 AND (sitotal-sijmlbayar) <>  0 AND sistatus IN (2, 3, 4, 7) AND " + f_TglFilter("sitgl", p1, p2), myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "POSPenjualanDobel"
                    nocategory = 15
                    dt = ambilData("SELECT sinoref, sinotransaksi, siid from M5_SI WHERE LENGTH(sinoref) <> 0 AND sistatus IN (2, 3, 4, 7) AND " + f_TglFilter("sitgl", p1, p2) + " GROUP BY sinoref HAVING COUNT(1) <> 1", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata = searchdata + dt.Rows(i)(0) + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + sptLogin
                    Next

                    If searchdata.Length > 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "Stokserialvspergudang"
                    nocategory = 16
                    dt = ambilData("SELECT nsiidbarang, bnama, nsigudang, SUM(nsijmlsisa) nsistok, stok FROM `m1_no_serial_in` JOIN m1_item ON bid = nsiidbarang JOIN m1_item_stock_warehouse ON nsiidbarang = idbarang AND nsigudang = kgudang GROUP BY nsiidbarang, nsigudang HAVING nsistok <> stok", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        'nsiidbarang, bnama, nsigudang, nsistok, stok
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "JurnalTidakAdaPosting"
                    nocategory = 1
                    'Ambil nomor 
                    dtNomor = ambilData("SELECT moduleid, kodetabel FROM `m0_nomor` WHERE transaksifa = 1 ORDER BY moduleid, menuid", myConn)

                    'BUAT FILTER NOTRANSAKSI JURNAL TRANSAKSI UNTUK TRANSAKSI POS
                    Dim ftGroupJurnal As String = grupJurnalPos
                    Dim fieldGrup As String() = Replace(grupJurnalPos, " ", "").Split(",")
                    For j As Integer = 0 To fieldGrup.Length - 1
                        If fieldGrup(j).ToLower.Equals("sihargatermasukpajak") Then
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tadjustment")
                        ElseIf fieldGrup(j).ToLower.Equals("sicustomer") Then
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tkontak")
                        Else
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "t" & Right(fieldGrup(j), fieldGrup(j).Length - 2))
                        End If
                    Next

                    'Ambil data jurnal
                    dtJurnal = ambilData("SELECT tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tnorek, tdebit, tkredit, turaian, tcatatan, turutan, '0' tjurnal, " & ftGroupJurnal & " FROM m2_transaction_journal WHERE " + f_TglFilter("ttgl", p1, p2), myConn)

                    'Ambil dari masing-masing transaksi
                    For i = 0 To dtNomor.Rows.Count - 1
                        dtTrans = New DataTable()
                        moduleid = dtNomor(i)("moduleid")
                        kodetabel = dtNomor(i)("kodetabel")

                        If kodetabel = "POS" Then
                            namatabel = "m5_si"
                        ElseIf moduleid = "10" Or moduleid = "11" Or moduleid = "12" Then
                            namatabel = "m_" + moduleid + "_" + kodetabel
                        Else
                            namatabel = "m" + moduleid + "_" + kodetabel
                        End If
                        If kodetabel = "PB" Then
                            kodetabel = "PV"
                        End If

                        'Filter sumber, idtransaksi, notransaksi
                        If kodetabel = "POS" Then
                            dtTrans = ambilData("SELECT sisumber sumber, siid id, sinotransaksi notransaksi, " & grupJurnalPos & " FROM " + namatabel + " WHERE sistatus IN(2,3,4,7) AND sicarabayar = 2 AND " + f_TglFilter("sitgl", p1, p2) & " GROUP BY " & grupJurnalPos, myConn)
                        ElseIf kodetabel = "SI" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "id id, " + kodetabel + "notransaksi notransaksi FROM " + namatabel + " WHERE " + kodetabel + "status IN(2,3,4,7) AND sicarabayar <> 2 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        Else
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "id id, " + kodetabel + "notransaksi notransaksi FROM " + namatabel + " WHERE " + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        End If
                        If dtTrans.Rows.Count > 0 Then
                            For y = 0 To dtTrans.Rows.Count - 1
                                If kodetabel = "POS" Then
                                    Dim fieldGroupJurnal As String() = Replace(ftGroupJurnal, " ", "").Split(",")
                                    Dim ftDtJurnal As String = ""
                                    For j As Integer = 0 To fieldGroupJurnal.Length - 1
                                        ftDtJurnal &= IIf(Len(ftDtJurnal) > 0, " AND ", "")
                                        If fieldGroupJurnal(j).Contains("tgl") Then
                                            ftDtJurnal &= fieldGroupJurnal(j) & " = '" & FixQuotes(AsFormatTanggal(dtTrans.Rows(y)(j + 3))) & "'"
                                        Else
                                            ftDtJurnal &= fieldGroupJurnal(j) & " = '" & FixQuotes(dtTrans.Rows(y)(j + 3)) & "'"
                                        End If
                                    Next
                                    drJurnal = dtJurnal.Select("tsumber = 'POS' AND " & ftDtJurnal)

                                Else
                                    drJurnal = dtJurnal.Select("tsumber = '" + dtTrans.Rows(y)("sumber") + "' AND tidtransaksi = '" + dtTrans.Rows(y)("id").ToString + "' AND tnotransaksi = '" + dtTrans.Rows(y)("notransaksi") + "'")
                                End If

                                For j = 0 To drJurnal.Length - 1
                                    drJurnal(j)("tjurnal") = 1
                                Next
                            Next
                        End If
                    Next

                    drJurnal = dtJurnal.Select("tjurnal = '0'")
                    For y = 0 To drJurnal.Length - 1
                        JurnalAdaPosting += 1
                        'tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tnorek, tdebit, tkredit, turaian, tcatatan, turutan
                        searchdata += drJurnal(y)("tid").ToString + "|" + drJurnal(y)("tsumber").ToString + "|" + drJurnal(y)("tidtransaksi").ToString + "|" + drJurnal(y)("tnotransaksi").ToString + "|" + drJurnal(y)("ttgl").ToString + "|" + drJurnal(y)("tnorek").ToString + "|" + drJurnal(y)("tdebit").ToString + "|" + drJurnal(y)("tkredit").ToString + "|" + drJurnal(y)("turaian").ToString + "|" + drJurnal(y)("tcatatan").ToString + "|" + drJurnal(y)("turutan").ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = (JurnalAdaPosting).ToString()

                Case "TransaksiPostingbelumTerjurnal"
                    nocategory = 3
                    'Ambil nomor 
                    dtNomor = ambilData("SELECT moduleid, kodetabel FROM `m0_nomor` WHERE transaksifa = 1 ORDER BY moduleid, menuid", myConn)

                    'BUAT FILTER NOTRANSAKSI JURNAL TRANSAKSI UNTUK TRANSAKSI POS
                    Dim ftGroupJurnal As String = grupJurnalPos
                    Dim fieldGrup As String() = Replace(grupJurnalPos, " ", "").Split(",")
                    For j As Integer = 0 To fieldGrup.Length - 1
                        If fieldGrup(j).ToLower.Equals("sihargatermasukpajak") Then
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tadjustment")
                        ElseIf fieldGrup(j).ToLower.Equals("sicustomer") Then
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tkontak")
                        Else
                            ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "t" & Right(fieldGrup(j), fieldGrup(j).Length - 2))
                        End If
                    Next
                    Dim fldGrup As String() = Replace(ftGroupJurnal, " ", "").Split(",")

                    'Ambil data jurnal
                    dtJurnal = ambilData("SELECT tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tnorek, tdebit, tkredit, turaian, tcatatan, turutan, '0' tjurnal, " & ftGroupJurnal & " FROM m2_transaction_journal WHERE " + f_TglFilter("ttgl", p1, p2), myConn)

                    'Ambil dari masing-masing transaksi
                    For i = 0 To dtNomor.Rows.Count - 1
                        dtTrans = New DataTable()
                        moduleid = dtNomor(i)("moduleid")
                        kodetabel = dtNomor(i)("kodetabel")

                        If kodetabel = "POS" Then
                            namatabel = "m5_si"
                        ElseIf moduleid = "10" Or moduleid = "11" Or moduleid = "12" Then
                            namatabel = "m_" + moduleid + "_" + kodetabel
                        Else
                            namatabel = "m" + moduleid + "_" + kodetabel
                        End If

                        If moduleid = "6" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_in sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND sid.hpp > 0 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)

                            For y = 0 To dtTrans.Rows.Count - 1
                                drJurnal = dtJurnal.Select("tsumber = '" + dtTrans.Rows(y)("sumber") + "' AND tidtransaksi = '" + dtTrans.Rows(y)("id").ToString + "' AND tnotransaksi = '" + dtTrans.Rows(y)("notransaksi") + "'")

                                If drJurnal.Length = 0 Then
                                    tidakAdaJurnal += 1
                                    searchdata += dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("tgl").ToString + sptLogin
                                End If
                            Next

                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_out sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND sid.hpp > 0 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf moduleid = "3" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND sid.hpp > 0 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf moduleid = "2" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si WHERE " + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "VP" Then
                            dtTrans = ambilData("SELECT * FROM( SELECT vpsumber sumber, vpnotransaksi notransaksi, vpid id, vptgl tgl, vp.vpid, SUM( (CASE WHEN vpd.sumber IN('RI','CA') THEN vpd.jmlbayar ELSE vpd.jmlbayar * - 1 END) ) as total FROM m4_vp vp JOIN m4_vp_detail vpd ON vp.vpid = vpd.idvp AND vp.vpstatus IN (2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2) + " GROUP BY vp.vpid, vpd.rekhutangpiutang HAVING total <> 0 ) as jurnal GROUP BY jurnal.vpid", myConn)
                        ElseIf kodetabel = "PV" Then
                            dtTrans = ambilData("SELECT * FROM(SELECT pvsumber sumber, pvnotransaksi notransaksi, pvid id, pvtgl tgl, pv.pvid, ROUND(SUM((CASE WHEN pvd.sumber IN('SI','OM','CA') THEN pvd.jmlbayar ELSE pvd.jmlbayar * - 1 END) ), 5) as total FROM m5_pv pv JOIN m5_pv_detail pvd ON pv.pvid = pvd.idpv AND pv.pvstatus IN (2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2) + " GROUP BY pv.pvid, pvd.rekhutangpiutang HAVING total <> 0) as jurnal GROUP BY jurnal.pvid", myConn)
                        ElseIf kodetabel = "AP" Or kodetabel = "AS" Or kodetabel = "IP" Or kodetabel = "PP" Or kodetabel = "RP" Or kodetabel = "PP" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si WHERE " + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "DA" Or kodetabel = "PPV" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "RK" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_pay sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "LU" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND sid.harga > 0 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "RI" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND sid.harga > 0 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "SI" Then
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND sicarabayar <> 2 AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        ElseIf kodetabel = "POS" Then
                            dtTrans = ambilData("SELECT sisumber sumber, sinotransaksi notransaksi, siid id, sitgl tgl, " & grupJurnalPos & " FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.idsi = si.siid WHERE sistatus IN(2,3,4,7) AND sicarabayar = 2 AND " + f_TglFilter("sitgl", p1, p2), myConn)
                        Else
                            If kodetabel = "PB" Then
                                kodetabel = "PV"
                            End If
                            dtTrans = ambilData("SELECT " + kodetabel + "sumber sumber, " + kodetabel + "notransaksi notransaksi, " + kodetabel + "id id, " + kodetabel + "tgl tgl FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON sid.id" + kodetabel + " = si." + kodetabel + "id WHERE " + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                        End If


                        For y = 0 To dtTrans.Rows.Count - 1

                            If kodetabel = "POS" Then
                                'BUAT FILTER NOTRANSAKSI JURNAL TRANSAKSI UNTUK TRANSAKSI POS
                                Dim fieldGroupJurnal As String() = Replace(ftGroupJurnal, " ", "").Split(",")
                                Dim ftDtJurnal As String = ""
                                For j As Integer = 0 To fieldGroupJurnal.Length - 1
                                    ftDtJurnal &= IIf(Len(ftDtJurnal) > 0, " AND ", "")
                                    If fieldGroupJurnal(j).Contains("tgl") Then
                                        ftDtJurnal &= fieldGroupJurnal(j) & " = '" & FixQuotes(AsFormatTanggal(dtTrans.Rows(y)(j + 4))) & "'"
                                    Else
                                        ftDtJurnal &= fieldGroupJurnal(j) & " = '" & FixQuotes(dtTrans.Rows(y)(j + 4)) & "'"
                                    End If
                                Next
                                drJurnal = dtJurnal.Select("tsumber = 'POS' AND " & ftDtJurnal)
                            Else
                                drJurnal = dtJurnal.Select("tsumber = '" + dtTrans.Rows(y)("sumber") + "' AND tidtransaksi = '" + dtTrans.Rows(y)("id").ToString + "' AND tnotransaksi = '" + dtTrans.Rows(y)("notransaksi") + "'")
                            End If

                            If drJurnal.Length = 0 Then
                                tidakAdaJurnal += 1
                                'Sumber, id, notransaksi
                                If kodetabel = "POS" Then
                                    Dim idTransaksi As String = ""
                                    For j As Integer = 0 To fieldGrup.Length - 1
                                        idTransaksi &= IIf(Len(idTransaksi) > 0, " AND ", "")
                                        If fieldGrup(j).Contains("tgl") Then
                                            idTransaksi &= fieldGrup(j) & " = '" & FixQuotes(AsFormatTanggal(dtTrans.Rows(y)(j + 4))) & "'"
                                        Else
                                            idTransaksi &= fieldGrup(j) & " = '" & FixQuotes(dtTrans.Rows(y)(j + 4)) & "'"
                                        End If
                                    Next

                                    searchdata += "POS" + "|" + idTransaksi + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("tgl").ToString + sptLogin
                                Else
                                    searchdata += dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("tgl").ToString + sptLogin
                                End If

                            End If
                        Next
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = tidakAdaJurnal.ToString

                Case "TransBarangTidakAdaPosting"
                    nocategory = 2
                    'Ambil nomor 
                    'dtNomor = ambilData("SELECT moduleid, kodetabel FROM `m0_nomor` WHERE transaksibarang = 1 ORDER BY moduleid, menuid", myConn)
                    dtNomor = ambilData("SELECT moduleid, kodetabel FROM `m0_nomor` WHERE transaksibarang = 1 AND kodetabel <> 'POS' ORDER BY moduleid, menuid", myConn)

                    'Ambil data transaksi barang
                    dtTransBarang = ambilData("SELECT id,  sumber, idutama, notransaksi, iddetail, idbarang, '0' tbarang, bnama FROM m1_item_transaction JOIN m1_item ON bid = idbarang WHERE " + f_TglFilter("tgl", p1, p2), myConn)

                    'Ambil dari masing-masing transaksi
                    For i = 0 To dtNomor.Rows.Count - 1
                        moduleid = dtNomor(i)("moduleid")
                        kodetabel = dtNomor(i)("kodetabel")

                        If moduleid = "10" Or moduleid = "11" Or moduleid = "12" Then
                            namatabel = "m_" + moduleid + "_" + kodetabel
                        Else
                            namatabel = "m" + moduleid + "_" + kodetabel
                        End If

                        If kodetabel = "RI" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idbarang FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " WHERE rijenispembeliankategori = 1 AND si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)

                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("notransaksi = '" + dtTrans.Rows(y)("notransaksi") + "' AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                For x = 0 To dr.Length - 1
                                    dr(x)("tbarang") = 1
                                Next
                            Next
                        ElseIf moduleid = "6" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "in iddetail, sid.idbarang FROM " + namatabel + " si JOIN " + namatabel + "_in sid ON si." + kodetabel + "id = sid.id" + kodetabel + " WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)

                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("notransaksi = '" + dtTrans.Rows(y)("notransaksi") + "' AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                For x = 0 To dr.Length - 1
                                    dr(x)("tbarang") = 1
                                Next
                            Next

                            'Try
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "out iddetail, sid.idbarang FROM " + namatabel + " si JOIN " + namatabel + "_out sid ON si." + kodetabel + "id = sid.id" + kodetabel + " WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)

                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("notransaksi = '" + dtTrans.Rows(y)("notransaksi") + "' AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                For x = 0 To dr.Length - 1
                                    dr(x)("tbarang") = 1
                                Next
                            Next
                        ElseIf moduleid = "11" Then
                            If kodetabel = "PB" Then
                                kodetabel = "PV"
                            End If
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idlayanan idbarang FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)

                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("notransaksi = '" + dtTrans.Rows(y)("notransaksi") + "' AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                For x = 0 To dr.Length - 1
                                    dr(x)("tbarang") = 1
                                Next
                            Next
                        Else
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idbarang FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)

                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("notransaksi = '" + dtTrans.Rows(y)("notransaksi") + "' AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                For x = 0 To dr.Length - 1
                                    dr(x)("tbarang") = 1
                                Next
                            Next

                        End If
                    Next

                    dr = dtTransBarang.Select("tbarang = '0'")
                    For y = 0 To dr.Length - 1
                        TransBarangAdaPosting += 1
                        'id,  sumber, idutama, notransaksi, iddetail, idbarang, bnama
                        searchdata += dr(y)(0).ToString + "|" + dr(y)(1).ToString + "|" + dr(y)(2).ToString + "|" + dr(y)(3).ToString + "|" + dr(y)(4).ToString + "|" + dr(y)(5).ToString + "|" + dr(y)(7).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 2)
                    End If
                    search = TransBarangAdaPosting.ToString()

                Case "TransaksiPostingBelumMasukTransaksiBarang"
                    nocategory = 4
                    'Ambil nomor 
                    'dtNomor = ambilData("SELECT moduleid, kodetabel FROM `m0_nomor` WHERE transaksibarang = 1 AND moduleid IN (11) ORDER BY moduleid, menuid", myConn)
                    dtNomor = ambilData("SELECT moduleid, kodetabel FROM `m0_nomor` WHERE transaksibarang = 1 AND kodetabel <> 'POS' ORDER BY moduleid, menuid", myConn)

                    'Ambil data transaksi barang
                    dtTransBarang = ambilData("SELECT id,  sumber, idutama, notransaksi, iddetail, idbarang, '0' tbarang, jenismutasi FROM m1_item_transaction WHERE " + f_TglFilter("tgl", p1, p2), myConn)

                    'Ambil dari masing-masing transaksi
                    'bnama, notransaksi, sumber, id, iddetail
                    For i = 0 To dtNomor.Rows.Count - 1
                        dtTrans = New DataTable()
                        moduleid = dtNomor(i)("moduleid")
                        kodetabel = dtNomor(i)("kodetabel")

                        If moduleid = "10" Or moduleid = "11" Or moduleid = "12" Then
                            namatabel = "m_" + moduleid + "_" + kodetabel
                        Else
                            namatabel = "m" + moduleid + "_" + kodetabel
                        End If



                        If kodetabel = "MRS" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "out iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_out sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("jenismutasi = 1 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|1" + sptLogin
                                End If


                                dr = dtTransBarang.Select("jenismutasi = 0 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If
                            Next

                        ElseIf kodetabel = "MRN" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "out iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_out sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("jenismutasi = 1 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|1" + sptLogin
                                End If


                                dr = dtTransBarang.Select("jenismutasi = 0 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If
                            Next

                        ElseIf kodetabel = "TS" Or kodetabel = "RS" Or kodetabel = "DNR" Or kodetabel = "DO" Or kodetabel = "DR" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("jenismutasi = 0 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If

                                dr = dtTransBarang.Select("jenismutasi = 1 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|1" + sptLogin
                                End If
                            Next
                        ElseIf kodetabel = "SA" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idbarang, bnama, jmlbarangmasuk FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    If Double.Parse(dtTrans.Rows(y)("jmlbarangmasuk")) > 0 Then
                                        searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|1" + sptLogin
                                    Else
                                        searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                    End If
                                End If
                            Next
                        ElseIf kodetabel = "RI" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE rijenispembeliankategori = 1 AND si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If
                            Next
                        ElseIf moduleid = "6" Then
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "in iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_in sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("jenismutasi = 1 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|1" + sptLogin
                                End If
                            Next

                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "out iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_out sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("jenismutasi = 0 AND sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If
                            Next
                        ElseIf moduleid = "11" Then
                            If kodetabel = "PB" Then
                                kodetabel = "PV"
                            End If
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idlayanan idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idlayanan WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If
                            Next
                        Else
                            dtTrans = ambilData("SELECT si." + kodetabel + "notransaksi notransaksi, si." + kodetabel + "sumber sumber, si." + kodetabel + "id id, sid.id" + kodetabel + "detail iddetail, sid.idbarang, bnama FROM " + namatabel + " si JOIN " + namatabel + "_detail sid ON si." + kodetabel + "id = sid.id" + kodetabel + " JOIN m1_item ON bid = sid.idbarang WHERE si." + kodetabel + "status IN(2,3,4,7) AND " + f_TglFilter(kodetabel + "tgl", p1, p2), myConn)
                            For y = 0 To dtTrans.Rows.Count - 1
                                dr = dtTransBarang.Select("sumber = '" + dtTrans.Rows(y)("sumber") + "' AND idutama = '" + dtTrans.Rows(y)("id").ToString + "' AND iddetail = '" + dtTrans.Rows(y)("iddetail").ToString + "' AND idbarang = '" + dtTrans.Rows(y)("idbarang").ToString + "'")
                                If dr.Length = 0 Then
                                    tidakAdaTransBarang += 1
                                    searchdata += dtTrans.Rows(y)("bnama").ToString + "|" + dtTrans.Rows(y)("notransaksi").ToString + "|" + dtTrans.Rows(y)("sumber").ToString + "|" + dtTrans.Rows(y)("id").ToString + "|" + dtTrans.Rows(y)("iddetail").ToString + "|0" + sptLogin
                                End If
                            Next
                        End If
                    Next
                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = tidakAdaTransBarang.ToString

                Case "Stokbatchvspergudang"
                    nocategory = 17
                    dt = ambilData("SELECT bid, bnama, nbigudang, SUM(nbijmlsisa) nbistok, stok FROM `m1_no_batch_in` JOIN m1_item ON bid = nbiidbarang JOIN m1_item_stock_warehouse ON nbiidbarang = idbarang AND nbigudang = kgudang GROUP BY nbiidbarang, nbigudang HAVING nbistok <> stok", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1) + "|" + dt.Rows(i)(2).ToString + "|" + dt.Rows(i)(3).ToString + "|" + dt.Rows(i)(4).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "HitungUlangHPP"
                    nocategory = 18
                    dt = ambilData("SELECT namabarang, idbarang, COUNT(1) no FROM m1_item_transaction it JOIN m0_nomor n ON n.awalan = it.sumber WHERE hppfix = 0 AND n.transaksihpp = 1 GROUP BY idbarang", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + "|" + dt.Rows(i)(1).ToString + "|" + dt.Rows(i)(2).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                    search = dt.Rows.Count.ToString

                Case "JurnalUlang"
                    nocategory = 19
                    dt = ambilData("SELECT snilai FROM `m0_setting` WHERE sgrup = 'validitasdata' AND skode = 'jurnalulang'", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString
                    Next

                    search = dt.Rows.Count.ToString

                Case Else
                    result(2) = "Invalid packet" : GoTo selesai

            End Select

            'sql = "INSERT INTO m0_validitas_data (kode, tahun, bulan, status, keterangan, tgl_cek) VALUES ('" + nocategory.ToString + "', '', '', '', '', 'NOW()')")
            'changeData(sql, myConn, Trans)

            result(1) = 1

            Trans.Commit()  '*** Commit Transaction ***'

        Catch ex As Exception
            If Err.Description = "Unable to cast object of type 'System.String' to type 'System.Data.DataTable'." And errstring <> "" Then
                result(2) = "Paket : " + paket + " - " + errstring
            Else
                result(2) = "Paket : " + paket + " - " + Err.Description
            End If

            Try
                Trans.Rollback() '*** RollBack Transaction ***'  
            Catch ex1 As Exception
                result(2) += " " + Err.Description
            End Try

        End Try

        myConn.Close()

selesai:

        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, searchdata)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmlbelumfix"), sptSubParam, ReplaceMapping("data"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function m0_hitung_perpaket(ByVal param As String) As String
        'M0_NotesSearch --------------------------------------------------------
        'nsumber, nidtransaksi, ncatatan
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "0"

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""
        Dim searchdata As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", xString As String = ""
        Dim dt As New DataTable

        Dim _AccessKey As String = ""
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

        _AccessKey = paramSplit(0)
        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
            result(2) = "Access denied for get data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI PARAMETER PAGING =========================================================
        userid = paramSplit(3)
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

        Dim paket, p1, p2 As String
        paket = paramSplit(5).Split(sptSubParam)(0)
        p1 = paramSplit(5).Split(sptSubParam)(1)
        p2 = paramSplit(5).Split(sptSubParam)(2)
        searchdata = paramSplit(5).Split(sptSubParam)(3)

        Dim myConn As New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction
        'Dim da As MySqlDataAdapter, str As New StringBuilder, dr As DataRow
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            Dim dtNomor, dtTrans, dtTransDetail, dtTransBarang, dtJurnal As New DataTable
            'Dim drJurnal(), drTransBarang() As DataRow
            Dim arr() As String

            Dim moduleid As String = "", kodetabel As String = "", namatabel As String = "", utamaid As String = ""
            Dim tidakAdaJurnal As Integer = 0, tidakAdaTransBarang As Integer = 0, JurnalAdaPosting As Integer = 0, TransBarangAdaPosting As Integer = 0
            Dim data As String = "", indexArray As Integer = 0


            'AMBIL DATA DARI SETTING -----------------------------
            Dim dtSetting As New DataTable, grupJurnalPos As String = ""
            dtSetting = AsDataTableAmbilDariDB("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = 0 AND sgrup = 'accounting' AND skode = 'GrupJurnalPOS')")

            'GRUP JURNAL POS
            grupJurnalPos = AsDataTableDLookup(dtSetting, "snilai", "smodule = 0 AND sgrup = 'accounting' AND skode = 'GrupJurnalPOS'", "Not found")
            If grupJurnalPos = "Not found" Or Len(grupJurnalPos) = 0 Then
                result(2) = "Setting GrupJurnalPOS not found." : GoTo selesai
            Else
                grupJurnalPos = "simatauang, sikurs, sihargatermasukpajak" & IIf(Len(grupJurnalPos) > 0, ", ", "") & grupJurnalPos
            End If
            'END OF AMBIL DATA DARI SETTING ----------------------


            Select Case paket
                Case "TransaksiBarangDobel"
                    'notransaksi, id, sumber, jenismutasi, idutama, iddetail, bnama
                    arr = searchdata.Split(sptLogin)
                    For i = 0 To arr.Length - 1
                        data += arr(i).Split("|")(1) + ","
                    Next

                    data = "(" + data.Substring(0, data.Length - 1) + ")"
                    'result(2) = "DELETE FROM m1_item_transaction WHERE id IN " + data : GoTo selesai
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m1_item_transaction WHERE id IN " + data
                        .ExecuteNonQuery()
                        .Dispose()
                    End With

                    ' update per gudang dan global
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m1_item_stock_warehouse;INSERT INTO m1_item_stock_warehouse (SELECT b.bid, tb.gudang,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stokfix	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang)"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With

                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "UPDATE `m1_item` SET `bstok`='0';UPDATE(SELECT idbarang, round(SUM(stok), 5) stok FROM m1_item_stock_warehouse GROUP BY idbarang) h JOIN m1_item i ON i.bid = h.idbarang SET i.bstok = h.stok"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With
                Case "JurnalDobel"
                    'tnotransaksi, tsumber, tidtransaksi, tnorek, tdebit, tkredit, turaian, tcatatan, turutan, tid
                    arr = searchdata.Split(sptLogin)
                    For i = 0 To arr.Length - 1
                        data += arr(i).Split("|")(9) + ","
                    Next

                    data = "(" + data.Substring(0, data.Length - 1) + ")"

                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m2_transaction_journal WHERE tid IN " + data
                        .ExecuteNonQuery()
                        .Dispose()
                    End With
                Case "SaldoJMLHPPMinus"

                Case "StokTBvsstokpergudang"
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m1_item_stock_warehouse;INSERT INTO m1_item_stock_warehouse (SELECT b.bid, tb.gudang,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stokfix	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang)"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With
                Case "StokPergudangvsStokGlobal"
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "UPDATE `m1_item` SET `bstok`='0';UPDATE(SELECT idbarang, round(SUM(stok), 5) stok FROM m1_item_stock_warehouse GROUP BY idbarang) h JOIN m1_item i ON i.bid = h.idbarang SET i.bstok = h.stok"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With
                Case "StokGlobalMinus"
                Case "StokPergudangMinus"
                Case "Akunjurnaltidakadadicoa"
                Case "JurnalTidakImbang"
                    'tsumber, tidtransaksi, tnotransaksi, tdebit, tkredit
                    arr = searchdata.Split(sptLogin)
                    Dim strJournal, rsJournal(), rsResult() As String
                    Dim sumber As String, idTransaksi As String
                    'M0_JournalUlang
                    Dim wsM0_JournalUlang As New m0_journal
                    searchdata = ""
                    For i = 0 To arr.Length - 1
                        sumber = arr(i).Split("|")(0)
                        idTransaksi = arr(i).Split("|")(1)
                        strJournal = wsM0_JournalUlang.M0_Journal(paramSplit(0) & "★M0_Journal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★M0_Journal△" & sumber & "△" & idTransaksi & "")

                        '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
                        rsJournal = strJournal.Split(sptParam)

                        '// JIKA KEMBALIAN FUNGSI JURNAL <> 3 MAKA SALAH
                        If rsJournal.Length = 3 Then
                            '// AMBIL BAGIAN RESULT DARI FUNGSI JURNAL - result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
                            rsResult = rsJournal(0).Split(sptSubParam)
                            '// JIKA BAGIAN RESULT DARI FUNGSI JURNAL <> 5 MAKA SALAH
                            If rsResult.Length = 5 Then
                                If rsResult(1) <> 1 And rsResult(1) <> 4 And rsResult(2) <> "Invalid Packet." Then '// JIKA GAGAL - KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
                                    result(2) = "Journal proccess failed. " & sumber & " : " & idTransaksi & ". " & rsResult(2) & "" : GoTo selesai
                                End If
                            Else
                                result(2) = "Journal proccess failed. " & sumber & " : " & idTransaksi & ". Invalid result data #2'" : GoTo selesai
                            End If

                        Else
                            result(2) = "Journal proccess failed. " & sumber & " : " & idTransaksi & ". Invalid result data #1'" : GoTo selesai
                        End If

                        If sumber = "POS" Then
                            'BUAT FILTER NOTRANSAKSI JURNAL TRANSAKSI UNTUK TRANSAKSI POS
                            Dim ftGroupJurnal As String = idTransaksi
                            Dim fieldGrup As String() = Replace(grupJurnalPos, " ", "").Split(",")
                            For j As Integer = 0 To fieldGrup.Length - 1
                                If fieldGrup(j).ToLower.Equals("sihargatermasukpajak") Then
                                    ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tadjustment")
                                ElseIf fieldGrup(j).ToLower.Equals("sicustomer") Then
                                    ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "tkontak")
                                Else
                                    ftGroupJurnal = ftGroupJurnal.Replace(fieldGrup(j), "t" & Right(fieldGrup(j), fieldGrup(j).Length - 2))
                                End If
                            Next
                            dt = ambilData("SELECT tsumber, tidtransaksi, tnotransaksi, SUM(tdebit) tdebit, SUM(tkredit) tkredit FROM m2_transaction_journal WHERE tsumber = '" + sumber + "' AND " + ftGroupJurnal + " HAVING ROUND(SUM(tdebit - tkredit) , 5) <> 0", myConn)
                        Else
                            dt = ambilData("SELECT tsumber, tidtransaksi, tnotransaksi, SUM(tdebit) tdebit, SUM(tkredit) tkredit FROM m2_transaction_journal WHERE tsumber = '" + sumber + "' AND tidtransaksi = " + idTransaksi + " HAVING ROUND(SUM(tdebit - tkredit) , 5) <> 0", myConn)
                        End If

                        If dt.Rows.Count <> 0 Then
                            searchdata &= arr(i).ToString + sptLogin
                            'result(2) = "Journal not balanced, check transaction again. " & arr(i).Split("|")(2) & " : " & sumber & " : " & idTransaksi & ". Invalid result data #3'" : GoTo selesai
                        End If
                    Next
                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 1)
                    End If
                Case "Transaksisisanolkomasekian"
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "UPDATE(SELECT siid, sitotal, sijmlbayar FROM m5_si WHERE (sitotal-sijmlbayar) < 1 AND (sitotal-sijmlbayar) <>  0) h JOIN m5_si si ON si.siid = h.siid SET si.sitotal = si.sijmlbayar"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With
                Case "POSPenjualanDobel"
                    Dim strJournal As String
                    'Dim rsJournal(), rsResult() As String
                    arr = searchdata.Split(sptLogin)
                    For i = 0 To arr.Length - 1
                        'sinoref, sinotransaksi, siid
                        Dim wsM5_Si As New m5_si
                        strJournal = wsM5_Si.M5_SiUpdateStatus(paramSplit(0) & "★M5_SiUpdateStatus★1△1△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid.ToString & "★0★" & arr(i).Split("|")(2) & "△0")
                        Return strJournal
                        '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
                        'rsJournal = strJournal.Split(sptParam)
                    Next
                Case "Stokserialvspergudang"
                Case "JurnalTidakAdaPosting"
                    'tid, tsumber, tidtransaksi, tnotransaksi, ttgl, tnorek, tdebit, tkredit, turaian, tcatatan, turutan
                    arr = searchdata.Split(sptLogin)
                    For i = 0 To arr.Length - 1
                        data += arr(i).Split("|")(0) + ","
                    Next

                    data = "(" + data.Substring(0, data.Length - 1) + ")"
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m2_transaction_journal WHERE tid IN " + data
                        .ExecuteNonQuery()
                        .Dispose()
                    End With
                Case "TransaksiPostingbelumTerjurnal"
                    arr = searchdata.Split(sptLogin)
                    Dim strJournal, rsJournal(), rsResult() As String
                    Dim sumber As String, idTransaksi As String
                    'M0_JournalUlang
                    Dim wsM0_JournalUlang As New m0_journal
                    For i = 0 To arr.Length - 1
                        sumber = arr(i).Split("|")(0)
                        idTransaksi = arr(i).Split("|")(1)
                        strJournal = wsM0_JournalUlang.M0_Journal(paramSplit(0) & "★M0_Journal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★1★M0_Journal△" & sumber & "△" & idTransaksi & "")

                        '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
                        rsJournal = strJournal.Split(sptParam)

                        '// JIKA KEMBALIAN FUNGSI JURNAL <> 3 MAKA SALAH
                        If rsJournal.Length = 3 Then
                            '// AMBIL BAGIAN RESULT DARI FUNGSI JURNAL - result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
                            rsResult = rsJournal(0).Split(sptSubParam)
                            '// JIKA BAGIAN RESULT DARI FUNGSI JURNAL <> 5 MAKA SALAH
                            If rsResult.Length = 5 Then
                                If rsResult(1) <> 1 And rsResult(1) <> 4 Then '// JIKA GAGAL - KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
                                    result(2) = "Journal proccess failed. " & sumber & " : " & idTransaksi & ". " & rsResult(2) & "" : GoTo selesai
                                End If
                            Else
                                result(2) = "Journal proccess failed. " & sumber & " : " & idTransaksi & ". Invalid result data #2'" : GoTo selesai
                            End If

                        Else
                            result(2) = "Journal proccess failed. " & sumber & " : " & idTransaksi & ". Invalid result data #1'" : GoTo selesai
                        End If
                    Next
                Case "TransBarangTidakAdaPosting"
                    'id,  sumber, idutama, notransaksi, iddetail, idbarang, bnama
                    arr = searchdata.Split(sptLogin)
                    For i = 0 To arr.Length - 1
                        data += arr(i).Split("|")(0) + ","
                    Next

                    data = "(" + data.Substring(0, data.Length - 1) + ")"
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m1_item_transaction WHERE id IN " + data
                        .ExecuteNonQuery()
                        .Dispose()
                    End With

                Case "TransaksiPostingBelumMasukTransaksiBarang"
                    Dim sumber, idutama, iddetail As String
                    Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0

                    Dim objCmd As New MySql.Data.MySqlClient.MySqlCommand()
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    Dim idbarang As Integer = 0, jmlbarang As Double = 0
                    Dim gudang As String = "", notransaksi As String = ""

                    arr = searchdata.Split(sptLogin)
                    Dim arrHitungUlang(arr.Length - 1) As String
                    Dim dtMatauang As DataTable = ambilData("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", myConn)
                    Dim uangFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                    If uangFungsional = "Not found" Then
                        result(2) = "Setting Functional Currency not found." : GoTo selesai
                    End If
                    Dim kursFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                    If kursFungsional = "Not found" Then
                        result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
                    End If

                    For i = 0 To arr.Length - 1

                        'Set Default
                        hpp = 0 : postinghpp = 0 : bstok = 0
                        jenismutasi = 0 : saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        '//bnama, notransaksi, sumber, id, iddetail
                        sumber = arr(i).Split("|")(2)
                        idutama = arr(i).Split("|")(3)
                        result(4) = idutama
                        iddetail = arr(i).Split("|")(4)
                        jenismutasi = arr(i).Split("|")(5)
                        xString = arr(i)
                        '///CATATAN KHUSUS UNTUK SI TRANSAKSI BARANG ASSEMBLY LANGSUNG (PENYUSUN KELUAR), CUSTOMINT10 DIISI -2, UNTUK URUTAN HITUNG ULANG HPP
                        'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                        Select Case sumber
                            'M3
                            Case "TS"
                                Dim updStokIn As String = "", updStokOut As String = "", gudangIn As String = "", gudangOut As String = ""

                                'INSERT ITEM TRANSACTION ====================================================
                                'AMBIL DATA DETAIL YANG BARU
                                sql = "SELECT tsd.idtsdetail, tsd.idbarang, tsd.namabarang, tsd.tipebarang, tsd.jml, tsd.satuan, tsd.jmlbarang, tsd.satuanbarang, tsd.idhppkhususmasuk, tsd.gudangasal, tsd.gudangtransit, tsd.gudangtujuan, tsd.catatan, tsd.costcenter, tsd.divisi, tsd.subdivisi, tsd.proyek, ts.tsinputtgl, i.bhpp, i.bhppaverage FROM m3_ts_detail tsd JOIN m3_ts ts ON tsd.idts = ts.tsid JOIN m1_item i ON tsd.idbarang = i.bid WHERE tsd.idts = '" & result(4) & "' AND tsd.idtsdetail = '" & iddetail & "' AND tsd.idtsdetail = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)

                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m3_ts WHERE tsid = " & result(4), myConn)

                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("tsnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then

                                    'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                                    dtMatauang = ambilData("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", myConn)
                                    uangFungsional = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                                    If uangFungsional = "Not found" Then
                                        result(2) = "Setting Functional Currency not found." : GoTo selesai
                                    End If
                                    kursFungsional = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                                    If kursFungsional = "Not found" Then
                                        result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
                                    End If

                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET GUDANG MASUK
                                        'JIKA TSJENIS = 0 (MUTASI TIDAK LANGSUNG), MAKA GUDANG MASUK = GUDANGTRANSIT
                                        'JIKA TSJENIS = 1 (MUTASI LANGSUNG), MAKA GUDANG MASUK = GUDANGTUJUAN
                                        If drutama("tsjenis") = 0 Then
                                            gudangIn = dr1("gudangtransit")
                                        Else
                                            gudangIn = dr1("gudangtujuan")
                                        End If

                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("bhppaverage"))

                                        If jenismutasi = 0 Then
                                            'POSTING BARANG KELUAR (gudangasal)
                                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("tscabang")) & "', '" & FixQuotes(drutama("tslokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("tskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("tssumber")) & "', " & result(4) & ", " & dr1("idtsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("tstgl"))) & "', " & drutama("tsbagianmutasi") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("tsuraian")) & "', '" & FixQuotes(drutama("tscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("tsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("tsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        Else
                                            'POSTING BARANG MASUK (gudang masuk)
                                            jenismutasi = 1
                                            'QUERY INSERT TRANSAKSI BARANG MASUK
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                            cabang,                                    lokasi,                        gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("tscabang")) & "', '" & FixQuotes(drutama("tslokasi")) & "', '" & FixQuotes(gudangIn) & "', " & drutama("tskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("tssumber")) & "', " & result(4) & ", " & dr1("idtsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("tstgl"))) & "', " & drutama("tsbagianmutasi") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("tsuraian")) & "', '" & FixQuotes(drutama("tscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("tsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("tsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                                        End If

                                    Next

                                    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                    changeData(sql, myConn, Trans)
                                    strTransaksiBarang.Clear()
                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "RS"
                                'AMBIL DATA DETAIL YANG BARU
                                sql = "SELECT rsd.idrsdetail, rsd.idbarang, rsd.namabarang, rsd.tipebarang, rsd.jml, rsd.satuan, rsd.jmlbarang, rsd.satuanbarang, rsd.gudangasal, rsd.gudangtransit, rsd.gudangtujuan, rsd.catatan, rsd.costcenter, rsd.divisi, rsd.subdivisi, rsd.proyek, rs.rsinputtgl, i.bhpp, i.bhppaverage FROM m3_rs_detail rsd JOIN m3_rs rs ON rsd.idrs = rs.rsid JOIN m1_item i ON rsd.idbarang = i.bid WHERE rsd.idrs = '" & result(4) & "' AND rsd.idrsdetail = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)

                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m3_rs WHERE rsid = " & result(4), myConn)

                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("rsnotransaksi")
                                If dtDetailNew.Rows.Count > 0 Then

                                    'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                                    dtMatauang = ambilData("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", myConn)
                                    uangFungsional = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                                    If uangFungsional = "Not found" Then
                                        result(2) = "Setting Functional Currency not found." : GoTo selesai
                                    End If
                                    kursFungsional = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                                    If kursFungsional = "Not found" Then
                                        result(2) = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
                                    End If

                                    For Each dr1 As DataRow In dtDetailNew.Rows
                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("bhppaverage"))

                                        'POSTING BARANG KELUAR (gudangtransit)
                                        strTransaksiBarang.Clear()
                                        If jenismutasi = 0 Then
                                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,             idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("rskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rssumber")) & "', " & result(4) & ", " & dr1("idrsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rsbagianterima") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("rsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        Else
                                            'POSTING BARANG MASUK (gudangtujuan)
                                            jenismutasi = 1
                                            'QUERY INSERT TRANSAKSI BARANG MASUK
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                            cabang,                                    lokasi,                                   gudang,                        kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                               kurs,                    harga,                 diskon,              jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rscabang")) & "', '" & FixQuotes(drutama("rslokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("rskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rssumber")) & "', " & result(4) & ", " & dr1("idrsdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rstgl"))) & "', " & drutama("rsbagianterima") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(uangFungsional) & "', '" & FixDouble(kursFungsional) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rsuraian")) & "', '" & FixQuotes(drutama("rscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("rsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        End If
                                    Next

                                    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                    changeData(sql, myConn, Trans)
                                Else
                                    ' result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "SA"

                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m3_sa WHERE said = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("sanotransaksi")

                                'JIKA sacustomint1 = 1, MAKA PENYESUAIAN HPP. JIKA sacustomint1 = 0, MAKA PENYESUAIAN STOK
                                If drutama("sacustomint1") = 1 Then
                                    sql = "SELECT sad.idsadetail, sad.idbarang, sad.namabarang, sad.tipebarang, (CASE j.jenismutasi WHEN 1 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok / sad.nilaisatuan ELSE 0 END) as jmlkeluar, sad.satuan, sad.nilaisatuan, (CASE j.jenismutasi WHEN 1 THEN isw.stok ELSE 0 END) as jmlbarangmasuk, (CASE j.jenismutasi WHEN 0 THEN isw.stok ELSE 0 END) as jmlbarangkeluar, sad.satuanbarang, sad.hpp, sad.idhppkhususmasuk, isw.kgudang as gudang, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid JOIN m0_jenismutasi j JOIN m1_item_stock_warehouse isw ON sad.idbarang = isw.idbarang AND isw.stok <> 0 WHERE sad.idsa = '" & result(4) & "' AND sad.idsadetail = '" & iddetail & "' ORDER BY sad.idsadetail, j.jenismutasi, isw.kgudang"
                                Else
                                    sql = "SELECT sad.idsadetail, sad.idbarang, sad.namabarang, sad.tipebarang, sad.jmlmasuk, sad.jmlkeluar, sad.satuan, sad.jmlbarangmasuk, sad.jmlbarangkeluar, sad.satuanbarang, sad.hpp, sad.idhppkhususmasuk, sad.gudang, sad.catatan, sad.costcenter, sad.divisi, sad.subdivisi, sad.customdbl1, sad.proyek, sa.sainputtgl, i.bhpp FROM m3_sa_detail sad JOIN m3_sa sa ON sad.idsa = sa.said JOIN m1_item i ON sad.idbarang = i.bid WHERE sad.idsa = '" & result(4) & "' AND sad.idsadetail = '" & iddetail & "'"
                                End If
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)

                                If dtDetailNew.Rows.Count > 0 Then

                                    'INSERT ITEM TRANSACTION ==================================================
                                    Dim sqlStokGudang As String = "", jmltransaksi As Double = 0

                                    'AMBIL MATAUANG FUNGSIONAL DARI SETTING
                                    dtMatauang = ambilData("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')", myConn)
                                    Dim matauang As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
                                    If matauang = "Not found" Then
                                        result(2) = "Setting Functional Currency not found." : Trans.Rollback() : GoTo selesai
                                    End If
                                    Dim kurs As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
                                    If kurs = "Not found" Then
                                        result(2) = "Setting Exchange Rate Functional Currency not found." : Trans.Rollback() : GoTo selesai
                                    End If

                                    'PERULANGAN DATA DETAIL
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        gudang = dr1("gudang")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'BARANG MASUK ATAU KELUAR
                                            If Double.Parse(dr1("jmlbarangmasuk")) > 0 Then
                                                jmlbarang = Double.Parse(dr1("jmlbarangmasuk"))
                                                jmltransaksi = Double.Parse(dr1("jmlmasuk"))

                                                'jenismutasi dan postinghpp 
                                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                                jenismutasi = 1 : postinghpp = 0

                                                'hitung saldojml = bstok + jmlbarang
                                                saldojml = bstok + jmlbarang

                                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                                'sql stok pergudang
                                                sqlStokGudang = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"

                                            Else
                                                jmlbarang = Double.Parse(dr1("jmlbarangkeluar"))
                                                jmltransaksi = Double.Parse(dr1("jmlkeluar"))

                                                'jenismutasi dan postinghpp 
                                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                                jenismutasi = 0 : postinghpp = 0

                                                'hitung saldojml = bstok - jmlbarang
                                                saldojml = bstok - jmlbarang

                                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                                'sql stok pergudang
                                                sqlStokGudang = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"

                                            End If

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                             cabang,                                   lokasi,                             gudang,                        kodepa,           jenismutasi,                               sumber,              idutama,                 iddetail,                      notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                       jmlbarang,                           satuanbarang,                        matauang,                      kurs,                    harga,                 diskon,              jmldiskon,                        idhppikm,        idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                               inputuser,  postingtgl, updatehpp,     postinghpp,     hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("sacabang")) & "', '" & FixQuotes(drutama("salokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("sakodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("sasumber")) & "', " & result(4) & ", " & dr1("idsadetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("satgl"))) & "', " & drutama("sabagiansa") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmltransaksi) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(matauang) & "', '" & FixDouble(kurs) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("sauraian")) & "', '" & FixQuotes(drutama("sacatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("sainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("sainputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            changeData(sqlStokGudang, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            'TAMBAHKAN KONDISI JIKA CUSTOMDBL <> 0 MAKA UPDATE BHARGABELI
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE " & FixDouble(dr1("customdbl1")) & " WHEN 0 THEN bhargabeli ELSE " & FixDouble(dr1("customdbl1")) & " END) WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)

                                        End If

                                    Next
                                    'END OF INSERT ITEM TRANSACTION ===========================================

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "IB"
                                Dim dtDetailNew As DataTable = ambilData("SELECT ibd.idibdetail, ibd.idbarang, ibd.namabarang, ibd.tipebarang, ibd.jml, ibd.satuan, ibd.jmlbarang, ibd.satuanbarang, ibd.matauang, ibd.kurs, ibd.hpp, ibd.gudang, ibd.catatan, ibd.costcenter, ibd.divisi, ibd.subdivisi, ibd.proyek, ib.ibinputtgl, i.bhpp FROM m3_ib_detail ibd JOIN m3_ib ib ON ibd.idib = ib.ibid JOIN m1_item i ON ibd.idbarang = i.bid WHERE ibd.idib = '" & result(4) & "' AND ibd.idibdetail = '" & iddetail & "' ORDER BY ibd.urutan", myConn)

                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m3_ib WHERE ibid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("ibnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then

                                    'INSERT ITEM TRANSACTION ====================================================
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        gudang = dr1("gudang")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 1 : postinghpp = 0

                                            'hitung saldojml = bstok + jmlbarang
                                            saldojml = bstok + jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                             gudang,                        kodepa,           jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                             jmlbarang,                             satuanbarang,                             matauang,                             kurs,                  harga,            diskon,                 jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("ibcabang")) & "', '" & FixQuotes(drutama("iblokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("ibkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("ibsumber")) & "', " & result(4) & ", " & dr1("idibdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ibtgl"))) & "', " & drutama("ibbagianib") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(hpp) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("iburaian")) & "', '" & FixQuotes(drutama("ibcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("ibinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("ibinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)
                                        End If

                                    Next
                                    'END OF INSERT ITEM TRANSACTION =============================================

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                                'M4
                            Case "GRN"
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m4_grn WHERE grnid = " & result(4), myConn)
                                Dim dtDetailNew As DataTable = ambilData("SELECT grnd.idgrndetail, grnd.idbarang, grnd.namabarang, grnd.tipebarang, grnd.jml, grnd.satuan, grnd.jmlbarang, grnd.satuanbarang, grnd.matauang, grnd.kurs, grnd.harga, grnd.diskon, grnd.jmldiskon, grnd.gudang, grnd.catatan, grnd.costcenter, grnd.divisi, grnd.subdivisi, grnd.proyek, grn.grninputtgl, i.bhpp, grnd.jmlpajak1, grnd.jmlpajak2 FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid JOIN m1_item i ON grnd.idbarang = i.bid WHERE grnd.idgrn = '" & result(4) & "' AND grnd.idgrndetail = '" & iddetail & "' ORDER BY grnd.urutan", myConn)
                                'result(2) = "SELECT * FROM m4_grn WHERE grid = " & result(4) + "@" : GoTo selesai
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("grnnotransaksi")


                                If dtDetailNew.Rows.Count > 0 Then

                                    'INSERT ITEM TRANSACTION ====================================================
                                    For Each dr1 As DataRow In dtDetailNew.Rows
                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        gudang = dr1("gudang")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bid, bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then

                                            Dim isfound As Boolean = False
                                            For y = 0 To arrHitungUlang.Length - 1
                                                If dtSaldo.Rows(0)("bid") = arrHitungUlang(y) Then
                                                    isfound = True
                                                End If
                                            Next

                                            If Not isfound Then
                                                arrHitungUlang(indexArray) = dtSaldo.Rows(0)("bid")
                                                indexArray = indexArray + 1
                                            End If

                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 1 : postinghpp = 0

                                            'hitung saldojml = bstok + jmlbarang
                                            saldojml = bstok + jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0
                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                               gudang,                         kodepa,           jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("grncabang")) & "', '" & FixQuotes(drutama("grnlokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("grnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("grnsumber")) & "', " & result(4) & ", " & dr1("idgrndetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("grntgl"))) & "', " & drutama("grnsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("grnuraian")) & "', '" & FixQuotes(drutama("grncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("grninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("grninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble(Double.Parse(dr1("kurs")) * Double.Parse(dr1("harga"))) & "' WHERE bid = '" & idbarang & "'"
                                            If drutama("grnhargatermasukpajak") = 0 Then
                                                sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                            Else
                                                sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                            End If
                                            changeData(sql, myConn, Trans)

                                        End If

                                    Next
                                    'END OF INSERT ITEM TRANSACTION =============================================

                                Else
                                    'result(2) = "Detail transaction data not found." : GoTo selesai
                                End If

                            Case "RI"
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m4_ri WHERE riid = " & result(4), myConn)
                                Dim dtDetailNew As DataTable = ambilData("SELECT rid.idridetail, rid.idbarang, rid.namabarang, rid.tipebarang, rid.jml, rid.satuan, rid.jmlbarang, rid.satuanbarang, rid.matauang, rid.kurs, rid.harga, rid.diskon, rid.jmldiskon, rid.gudang, rid.catatan, rid.costcenter, rid.divisi, rid.subdivisi, rid.proyek, ri.riinputtgl, i.bhpp, rid.jmlpajak1, rid.jmlpajak2 FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE rid.idri = '" & result(4) & "' AND rid.idridetail = '" & iddetail & "' ORDER BY rid.urutan", myConn)

                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("rinotransaksi")
                                If dtDetailNew.Rows.Count > 0 Then

                                    'INSERT ITEM TRANSACTION ====================================================
                                    For Each dr1 As DataRow In dtDetailNew.Rows
                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        gudang = dr1("gudang")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bid, bkode, bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then

                                            Dim isfound As Boolean = False
                                            For y = 0 To arrHitungUlang.Length - 1
                                                If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                    isfound = True
                                                End If
                                            Next

                                            If Not isfound Then
                                                arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                                indexArray = indexArray + 1
                                            End If

                                            'result(2) = indexArray.ToString + " " + arrHitungUlang(indexArray - 1).ToString + "@" : GoTo selesai
                                            data += arr(i).Split("|")(2) + ","

                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 1 : postinghpp = 0

                                            'hitung saldojml = bstok + jmlbarang
                                            saldojml = bstok + jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                             gudang,                        kodepa,           jenismutasi,                              sumber,              idutama,                  iddetail,                      notransaksi,                                                 tgl,                          kontak,                 idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rikodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("risumber")) & "', " & result(4) & ", " & dr1("idridetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("risupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("riinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("riinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble(Double.Parse(dr1("kurs")) * Double.Parse(dr1("harga"))) & "' WHERE bid = '" & idbarang & "'"
                                            If drutama("rihargatermasukpajak") = 0 Then
                                                sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                            Else
                                                sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                            End If
                                            changeData(sql, myConn, Trans)
                                        End If

                                    Next
                                    'END OF INSERT ITEM TRANSACTION =============================================

                                Else
                                    ' result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "DNR"
                                'AMBIL DATA DETAIL YANG BARU
                                sql = "SELECT dnrd.iddnrdetail, dnrd.idbarang, dnrd.namabarang, dnrd.tipebarang, dnrd.jml, dnrd.satuan, dnrd.jmlbarang, dnrd.satuanbarang, dnrd.matauang, dnrd.kurs, dnrd.harga, dnrd.diskon, dnrd.jmldiskon, dnrd.hpp, dnrd.idhppkhususmasuk, dnrd.gudangasal, dnrd.gudangtransit, dnrd.gudangtujuan, dnrd.catatan, dnrd.costcenter, dnrd.divisi, dnrd.subdivisi, dnrd.proyek, dnr.dnrinputtgl, i.bhpp FROM m4_dnr_detail dnrd JOIN m4_dnr dnr ON dnrd.iddnr = dnr.dnrid JOIN m1_item i ON dnrd.idbarang = i.bid WHERE dnrd.iddnr = '" & result(4) & "' AND  dnrd.iddnrdetail = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m4_dnr WHERE dnrid = " & result(4), myConn)

                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("dnrnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("hpp"))

                                        'POSTING BARANG KELUAR (gudangasal)
                                        If jenismutasi = 0 Then

                                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                              cabang,                                    lokasi,                                 gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("dnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & result(4) & ", " & dr1("iddnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("dnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("dnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        Else
                                            'POSTING BARANG MASUK (gudangtransit)
                                            jenismutasi = 1
                                            'QUERY INSERT TRANSAKSI BARANG MASUK
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                              cabang,                                    lokasi,                                    gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("dnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & result(4) & ", " & dr1("iddnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("dnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("dnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        End If
                                    Next

                                    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                    changeData(sql, myConn, Trans)
                                    strTransaksiBarang.Clear()
                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "PRT"
                                Dim dtDetailNew As DataTable = ambilData("SELECT prtd.idprtdetail, prtd.idbarang, prtd.namabarang, prtd.tipebarang, prtd.jml, prtd.satuan, prtd.jmlbarang, prtd.satuanbarang, prtd.matauang, prtd.kurs, prtd.harga, prtd.diskon, prtd.jmldiskon, prtd.idhppkhususmasuk, prtd.hpp, prtd.gudangasal, prtd.gudangtransit, prtd.gudangtujuan, prtd.catatan, prtd.costcenter, prtd.divisi, prtd.subdivisi, prtd.proyek, prt.prtinputtgl, i.bhpp FROM m4_prt_detail prtd JOIN m4_prt prt ON prtd.idprt = prt.prtid JOIN m1_item i ON prtd.idbarang = i.bid WHERE prtd.idprt = '" & result(4) & "' AND prtd.idprtdetail = '" & iddetail & "'", myConn)

                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m4_prt WHERE prtid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("prtnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then

                                    'INSERT ITEM TRANSACTION ====================================================
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        gudang = dr1("gudangtransit")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 0 : postinghpp = 0

                                            'hitung saldojml = bstok - jmlbarang
                                            saldojml = bstok - jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                                    gudang,                         kodepa,             jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("prtkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("prtsumber")) & "', " & result(4) & ", " & dr1("idprtdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drutama("prtcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("prtinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("prtinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)

                                        End If

                                    Next
                                    'END OF INSERT ITEM TRANSACTION =============================================

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                                'M5
                            Case "DO"
                                'AMBIL DATA DETAIL YANG BARU
                                sql = "SELECT dod.iddodetail, dod.idbarang, dod.namabarang, dod.tipebarang, dod.jml, dod.satuan, dod.jmlbarang, dod.satuanbarang, dod.matauang, dod.kurs, dod.harga, dod.diskon, dod.jmldiskon, dod.hpp, dod.idhppkhususmasuk, dod.gudangasal, dod.gudangtransit, dod.gudangtujuan, dod.catatan, dod.costcenter, dod.divisi, dod.subdivisi, dod.proyek, `do`.doinputtgl, i.bhpp FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = `do`.doid JOIN m1_item i ON dod.idbarang = i.bid WHERE dod.iddo = '" & result(4) & "' AND dod.iddodetail = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m5_do WHERE doid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("donotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("hpp"))
                                        strTransaksiBarang.Clear()
                                        'POSTING BARANG KELUAR (gudangasal)
                                        If jenismutasi = 0 Then

                                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                             cabang,                                   lokasi,                                gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("dokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dosumber")) & "', " & result(4) & ", " & dr1("iddodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("docustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("doinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("doinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        Else
                                            'POSTING BARANG MASUK (gudangtransit)
                                            jenismutasi = 1
                                            'QUERY INSERT TRANSAKSI BARANG MASUK
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                             cabang,                                   lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("docabang")) & "', '" & FixQuotes(drutama("dolokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("dokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dosumber")) & "', " & result(4) & ", " & dr1("iddodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dotgl"))) & "', " & drutama("docustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("douraian")) & "', '" & FixQuotes(drutama("docatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("doinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("doinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        End If
                                    Next

                                    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                    changeData(sql, myConn, Trans)

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "DR"
                                'AMBIL DATA DETAIL YANG BARU
                                sql = "SELECT drd.iddrdetail, drd.idbarang, drd.namabarang, drd.tipebarang, drd.jml, drd.jmlbarang, drd.jmlkembali, drd.jmlbarangkembali, drd.satuan, drd.satuanbarang, drd.matauang, drd.kurs, drd.harga, drd.diskon, drd.jmldiskon, drd.hpp, drd.idhppkhususmasuk, drd.gudangasal, drd.gudangtransit, drd.gudangtujuan, drd.gudangkembali, drd.catatan, drd.costcenter, drd.divisi, drd.subdivisi, drd.proyek, dr.drinputtgl, i.bhpp FROM m5_dr_detail drd JOIN m5_dr dr ON drd.iddr = dr.drid JOIN m1_item i ON drd.idbarang = i.bid WHERE drd.iddr = '" & result(4) & "' AND  drd.iddrdetail = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m5_dr WHERE drid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                Dim jmlbarangkembali As Double = 0
                                notransaksi = drutama("drnotransaksi")

                                Dim jmlTransaksi As Double = 0, jmlTransaksiKembali As Double = 0

                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows
                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'jml
                                        jmlTransaksi = Double.Parse(dr1("jml"))
                                        jmlTransaksiKembali = Double.Parse(dr1("jmlkembali"))

                                        'jmlbarang
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        jmlbarangkembali = Double.Parse(dr1("jmlbarangkembali"))

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("hpp"))

                                        strTransaksiBarang.Clear()
                                        'POSTING BARANG KELUAR (gudangtransit) == jmlbarang + jmlbarangkembali
                                        If jenismutasi = 0 Then
                                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                             cabang,                                   lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                                       satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksi + jmlTransaksiKembali) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang + jmlbarangkembali) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        Else
                                            'POSTING BARANG MASUK (gudangkembali)
                                            If jmlbarangkembali <> 0 Then
                                                jenismutasi = 1
                                                'QUERY INSERT TRANSAKSI BARANG MASUK
                                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                                'mapping                        id,                             cabang,                                   lokasi,                                  gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                         satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangkembali")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksiKembali) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarangkembali) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            End If

                                            'POSTING BARANG MASUK (gudangtujuan)
                                            jenismutasi = 1
                                            'QUERY INSERT TRANSAKSI BARANG MASUK
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                             cabang,                                   lokasi,                                  gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                    notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                                 satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("drcabang")) & "', '" & FixQuotes(drutama("drlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("drkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("drsumber")) & "', " & result(4) & ", " & dr1("iddrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("drtgl"))) & "', " & drutama("drcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(jmlTransaksi) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("druraian")) & "', '" & FixQuotes(drutama("drcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("drinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("drinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                                        End If
                                    Next

                                    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                    changeData(sql, myConn, Trans)

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "SI"
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m5_si WHERE siid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("sinotransaksi")
                                Dim dtDetailNew As DataTable = ambilData("SELECT sid.idsidetail, sid.idbarang, sid.namabarang, sid.tipebarang, sid.jml, sid.satuan, sid.jmlbarang, sid.satuanbarang, sid.matauang, sid.kurs, sid.harga, sid.diskon, sid.jmldiskon, sid.idhppkhususmasuk, sid.hpp, sid.gudangasal, sid.gudangtransit, sid.gudangtujuan, sid.catatan, sid.costcenter, sid.divisi, sid.subdivisi, sid.proyek, si.siinputtgl, i.bhpp FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid WHERE sid.idsi = '" & result(4) & "' AND sid.idsidetail = '" & iddetail & "'", myConn)
                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        gudang = dr1("gudangtujuan")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        'dtSaldo = ambilData(sql, myconn)
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 0 : postinghpp = 0

                                            'hitung saldojml = bstok - jmlbarang
                                            saldojml = bstok - jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("sicabang")) & "', '" & FixQuotes(drutama("silokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("sikodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("sisumber")) & "', " & result(4) & ", " & dr1("idsidetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sitgl"))) & "', " & drutama("sicustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("siuraian")) & "', '" & FixQuotes(drutama("sicatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("siinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("siinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)

                                        End If

                                    Next

                                Else
                                    ' result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "SR"
                                Dim dtDetailNew As DataTable = ambilData("SELECT srd.idsrdetail, srd.idbarang, srd.namabarang, srd.tipebarang, srd.jml, srd.satuan, srd.jmlbarang, srd.satuanbarang, srd.matauang, srd.kurs, srd.harga, srd.diskon, srd.jmldiskon, srd.hpp, srd.idhppkhususkeluar, srd.gudangasal, srd.gudangtransit, srd.gudangtujuan, srd.catatan, srd.costcenter, srd.divisi, srd.subdivisi, srd.proyek, sr.srinputtgl, i.bhpp, IFNULL(sid.hpp,srd.hpp)as hppbaru FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON srd.idsidetail=sid.idsidetail WHERE srd.idsr = '" & result(4) & "' AND srd.idsrdetail = '" & iddetail & "'", myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m5_sr WHERE srid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("srnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then

                                    'INSERT ITEM TRANSACTION ====================================================
                                    For Each dr1 As DataRow In dtDetailNew.Rows 'SET NILAI VARIABEL

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idbarang"))
                                        jmlbarang = Double.Parse(dr1("jmlbarang"))
                                        gudang = dr1("gudangtujuan")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 1 : postinghpp = 0

                                            'hitung saldojml = bstok + jmlbarang
                                            saldojml = bstok + jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                          kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                          kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,                         idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("srkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("srsumber")) & "', " & result(4) & ", " & dr1("idsrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drutama("srcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("srinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("srinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)
                                        End If

                                    Next
                                    'END OF INSERT ITEM TRANSACTION =============================================

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                                'M6
                            Case "MRS"
                                sql = "SELECT mrso.idmrsout, mrso.idbarang, mrso.namabarang, mrso.tipebarang, mrso.jml, mrso.satuan, mrso.jmlbarang, mrso.satuanbarang, mrso.matauang, mrso.kurs, mrso.harga, mrso.hpp, mrso.idhppkhususmasuk, mrso.gudangasal, mrso.gudangproduksi, mrso.gudangtujuan, mrso.catatan, mrso.costcenter, mrso.divisi, mrso.subdivisi, mrso.proyek, mrs.mrsinputtgl, i.bhpp FROM m6_mrs_out mrso JOIN m6_mrs mrs ON mrso.idmrs = mrs.mrsid JOIN m1_item i ON mrso.idbarang = i.bid WHERE mrso.idmrs = '" & result(4) & "' AND mrso.idmrsout = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m6_mrs WHERE mrsid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("mrsnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows
                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("hpp"))
                                        'POSTING BARANG KELUAR (gudangasal)
                                        If jenismutasi = 0 Then

                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                              cabang,                                    lokasi,                                 gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,                idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrscabang")) & "', '" & FixQuotes(drutama("mrslokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("mrskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrssumber")) & "', " & result(4) & ", " & dr1("idmrsout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrstgl"))) & "', " & drutama("mrsbagianmrs") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrsuraian")) & "', '" & FixQuotes(drutama("mrscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)
                                        Else
                                            strTransaksiBarang.Clear()
                                            'POSTING BARANG MASUK (gudangproduksi)
                                            jenismutasi = 1
                                            'mapping                        id,                              cabang,                                    lokasi,                                     gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,                idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrscabang")) & "', '" & FixQuotes(drutama("mrslokasi")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', " & drutama("mrskodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrssumber")) & "', " & result(4) & ", " & dr1("idmrsout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrstgl"))) & "', " & drutama("mrsbagianmrs") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrsuraian")) & "', '" & FixQuotes(drutama("mrscatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrsinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrsinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)
                                        End If
                                    Next


                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "MRN"
                                sql = "SELECT mrno.idmrnout, mrno.idbarang, mrno.namabarang, mrno.tipebarang, mrno.jml, mrno.satuan, mrno.jmlbarang, mrno.satuanbarang, mrno.matauang, mrno.kurs, mrno.harga, mrno.hpp, mrno.idhppkhususkeluar, mrno.gudangasal, mrno.gudangproduksi, mrno.gudangtujuan, mrno.catatan, mrno.costcenter, mrno.divisi, mrno.subdivisi, mrno.proyek, mrn.mrninputtgl, i.bhpp FROM m6_mrn_out mrno JOIN m6_mrn mrn ON mrno.idmrn = mrn.mrnid JOIN m1_item i ON mrno.idbarang = i.bid WHERE mrno.idmrn = '" & result(4) & "' AND mrno.idmrnout = '" & iddetail & "'"
                                Dim dtDetailNew As DataTable = ambilData(sql, myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m6_mrn WHERE mrnid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("mrnnotransaksi")

                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows
                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idbarang")
                                            indexArray = indexArray + 1
                                        End If

                                        'jenismutasi dan postinghpp 
                                        '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                        '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                        '- untuk transaksi mutasi saja maka postinghpp = 0
                                        postinghpp = 0

                                        'hitung hpp = hpp
                                        hpp = Double.Parse(dr1("hpp"))

                                        'POSTING BARANG KELUAR (gudangproduksi)
                                        If jenismutasi = 0 Then

                                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                              cabang,                                    lokasi,                                     gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,        idhppikm,          idhppikk,                               hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(dr1("gudangproduksi")) & "', " & drutama("mrnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrnsumber")) & "', " & result(4) & ", " & dr1("idmrnout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnbagianmrn") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        Else
                                            'POSTING BARANG MASUK (gudangtujuan)
                                            jenismutasi = 1
                                            'QUERY INSERT TRANSAKSI BARANG MASUK
                                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                            'mapping                        id,                              cabang,                                    lokasi,                                   gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,            iddetail,                    notransaksi,                                                  tgl,                             kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                   diskon,              jmldiskon,        idhppikm,          idhppikk,                               hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("mrncabang")) & "', '" & FixQuotes(drutama("mrnlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("mrnkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("mrnsumber")) & "', " & result(4) & ", " & dr1("idmrnout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("mrntgl"))) & "', " & drutama("mrnbagianmrn") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("mrnuraian")) & "', '" & FixQuotes(drutama("mrncatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("mrninputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("mrninputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                        End If
                                    Next

                                    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                    changeData(sql, myConn, Trans)

                                Else
                                    'result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                            Case "PD"

                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m6_pd WHERE pdid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("pdnotransaksi")

                                If jenismutasi = 0 Then

                                    'AMBIL DATA DETAIL BARANG BAHAN YANG BARU +++++++++++++++++++++++++++++++++++++++
                                    Dim dtDetailOut As DataTable = ambilData("SELECT pdo.idpdout, pdo.idbarang, pdo.namabarang, pdo.tipebarang, pdo.jml, pdo.satuan, pdo.jmlbarang, pdo.satuanbarang, pdo.matauang, pdo.kurs, pdo.harga, pdo.hpp, pdo.idhppkhususmasuk, pdo.gudangasal, pdo.gudangproduksi, pdo.gudangtujuan, pdo.catatan, pdo.costcenter, pdo.divisi, pdo.subdivisi, pdo.proyek, pd.pdinputtgl, i.bhpp FROM m6_pd_out pdo JOIN m6_pd pd ON pdo.idpd = pd.pdid JOIN m1_item i ON pdo.idbarang = i.bid WHERE pdo.idpd = '" & result(4) & "' AND  pdo.idpdout = '" & iddetail & "'", myConn)

                                    If dtDetailOut.Rows.Count > 0 Then

                                        'INSERT ITEM TRANSACTION #1 ==================================================
                                        'PERULANGAN DATA DETAIL BARANG BAHAN
                                        For Each dr1 As DataRow In dtDetailOut.Rows
                                            Dim isfound As Boolean = False
                                            For y = 0 To arrHitungUlang.Length - 1
                                                If dtDetailOut.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                    isfound = True
                                                End If
                                            Next

                                            If Not isfound Then
                                                arrHitungUlang(indexArray) = dtDetailOut.Rows(0)("idbarang")
                                                indexArray = indexArray + 1
                                            End If
                                            'SET NILAI VARIABEL
                                            idbarang = Double.Parse(dr1("idbarang"))
                                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                                            gudang = dr1("gudangproduksi")

                                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                            dtSaldo = ambilData(sql, myConn)
                                            If dtSaldo.Rows.Count > 0 Then
                                                'set nilai stok
                                                bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                                'jenismutasi dan postinghpp 
                                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                                jenismutasi = 0 : postinghpp = 0

                                                'hitung saldojml = bstok - jmlbarang
                                                saldojml = bstok - jmlbarang

                                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                                'QUERY INSERT TRANSAKSI BARANG
                                                strTransaksiBarang.Clear()
                                                'mapping                        id,                             cabang,                                   lokasi,                        gudang,                      kodepa,           jenismutasi,                               sumber,              idutama,              iddetail,                      notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                       jmlbarang,                           satuanbarang,                             matauang,                             kurs,                             harga,                 diskon,               jmldiskon,                        idhppikm,         idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                               inputuser,  postingtgl, updatehpp,     postinghpp,     hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("pdcabang")) & "', '" & FixQuotes(drutama("pdlokasi")) & "', '" & FixQuotes(gudang) & "', " & drutama("pdkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("pdsumber")) & "', " & result(4) & ", " & dr1("idpdout") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdtgl"))) & "', " & drutama("pdbagianpd") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("pduraian")) & "', '" & FixQuotes(drutama("pdcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("pdinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("pdinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                                sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                                changeData(sql, myConn, Trans)

                                                'UPDATE STOK PERGUDANG
                                                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                                changeData(sql, myConn, Trans)

                                                'UPDATE STOK GLOBAL
                                                sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                                changeData(sql, myConn, Trans)
                                            End If

                                        Next
                                        'END OF INSERT ITEM TRANSACTION #1 ==========================================

                                    End If
                                Else

                                    'AMBIL DATA DETAIL BARANG HASIL YANG BARU +++++++++++++++++++++++++++++++++++++++
                                    Dim dtDetailIn As DataTable = ambilData("SELECT pdi.idpdin, pdi.idbarang, pdi.namabarang, pdi.tipebarang, pdi.jml, pdi.satuan, pdi.jmlbarang, pdi.satuanbarang, pdi.matauang, pdi.kurs, pdi.harga, pdi.hpp, pdi.gudangasal, pdi.gudangproduksi, pdi.gudangtujuan, pdi.catatan, pdi.costcenter, pdi.divisi, pdi.subdivisi, pdi.proyek, pd.pdinputtgl, i.bhpp FROM m6_pd_in pdi JOIN m6_pd pd ON pdi.idpd = pd.pdid JOIN m1_item i ON pdi.idbarang = i.bid WHERE pdi.idpd = '" & result(4) & "' AND pdi.idpdin = '" & iddetail & "'", myConn)

                                    'INSERT ITEM TRANSACTION #2 =====================================================
                                    If dtDetailIn.Rows.Count > 0 Then
                                        'PERULANGAN DATA DETAIL BARANG HASIL
                                        For Each dr1 As DataRow In dtDetailIn.Rows
                                            Dim isfound As Boolean = False
                                            For y = 0 To arrHitungUlang.Length - 1
                                                If dtDetailIn.Rows(0)("idbarang") = arrHitungUlang(y) Then
                                                    isfound = True
                                                End If
                                            Next

                                            If Not isfound Then
                                                arrHitungUlang(indexArray) = dtDetailIn.Rows(0)("idbarang")
                                                indexArray = indexArray + 1
                                            End If
                                            'SET NILAI VARIABEL
                                            idbarang = Double.Parse(dr1("idbarang"))
                                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                                            gudang = dr1("gudangproduksi")

                                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                            dtSaldo = ambilData(sql, myConn)
                                            If dtSaldo.Rows.Count > 0 Then
                                                'set nilai stok
                                                bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                                'jenismutasi dan postinghpp 
                                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                                jenismutasi = 1 : postinghpp = 0

                                                'hitung saldojml = bstok + jmlbarang
                                                saldojml = bstok + jmlbarang

                                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                                'QUERY INSERT TRANSAKSI BARANG
                                                strTransaksiBarang.Clear()
                                                'mapping                        id,                             cabang,                                   lokasi,                        gudang,                      kodepa,           jenismutasi,                               sumber,              idutama,              iddetail,                      notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                       jmlbarang,                           satuanbarang,                             matauang,                             kurs,                             harga,                 diskon,               jmldiskon,        idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                               inputuser,  postingtgl, updatehpp,     postinghpp,     hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("pdcabang")) & "', '" & FixQuotes(drutama("pdlokasi")) & "', '" & FixQuotes(gudang) & "', " & drutama("pdkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("pdsumber")) & "', " & result(4) & ", " & dr1("idpdin") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("pdtgl"))) & "', " & drutama("pdbagianpd") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(jmlbarang) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(0) & "', '" & FixDouble(0) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("pduraian")) & "', '" & FixQuotes(drutama("pdcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("pdinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("pdinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                                sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                                changeData(sql, myConn, Trans)

                                                'UPDATE STOK PERGUDANG
                                                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                                changeData(sql, myConn, Trans)

                                                'UPDATE STOK GLOBAL
                                                sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                                changeData(sql, myConn, Trans)
                                            End If


                                            'BUAT QUERY UNTUK INSERT TABEL PEMBANDING PRODUKSI SESUAI BOM
                                            'BUAT CASE UNTUK QUERY ----------------------------------------------
                                            'idbarang = Double.Parse(dr1("idbarang"))
                                            'jmlbarang = Double.Parse(dr1("jmlbarang"))

                                            'ftBarangBom = IIf(Len(ftBarangBom.ToString) = 0, "", ftBarangBom & " OR ")
                                            'ftBarangBom = String.Concat(ftBarangBom, " (ibomout.idbaranghasil = '" & FixDouble(idbarang) & "') ")

                                            'strJml += " WHEN ibomout.idbaranghasil = '" & FixDouble(idbarang) & "' THEN ((ibomout.jmlbarang / ibomin.jmlbarang) * " & FixDouble(jmlbarang) & ") "
                                            'strJmlbarang += " WHEN ibomout.idbaranghasil = '" & FixDouble(idbarang) & "' THEN (((ibomout.jmlbarang / ibomin.jmlbarang) * " & FixDouble(jmlbarang) & ") * ibomout.nilaisatuan) "
                                            'END OF BUAT CASE UNTUK QUERY ---------------------------------------

                                        Next

                                    End If
                                End If
                                'M_11

                            Case "AK"

                                Dim dtDetailNew As DataTable = ambilData("SELECT akd.idakdetail, akd.idlayanan, akd.namalayanan, akd.tipebarang, akd.jml, akd.satuan, akd.jmltotal, akd.satuandefault, akd.matauang, akd.kurs, akd.harga, akd.diskon, akd.jmldiskon, akd.idhppkhususmasuk, akd.hpp, akd.gudang, akd.gudangtransit, akd.gudangtujuan, akd.catatan, akd.costcenter, akd.divisi, akd.subdivisi, akd.proyek, ak.akinputtgl, i.bhpp FROM m_11_ak_detail akd JOIN m_11_ak ak ON akd.idak = ak.akid JOIN m1_item i ON akd.idlayanan = i.bid WHERE akd.idak = '" & result(4) & "' AND akd.idakdetail = '" & iddetail & "'", myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m_11_ak WHERE akid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("aknotransaksi")
                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idlayanan") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idlayanan")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idlayanan"))
                                        jmlbarang = Double.Parse(dr1("jmltotal"))
                                        gudang = dr1("gudang")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 0 : postinghpp = 0

                                            'hitung saldojml = bstok - jmlbarang
                                            saldojml = bstok - jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("akcabang")) & "', '" & FixQuotes(drutama("aklokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("akkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("aksumber")) & "', " & result(4) & ", " & dr1("idakdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & drutama("akcustomer") & ", " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', CONCAT('" & FixQuotes(drutama("akuraian")) & "', ' ', '" & FixQuotes(drutama("aknoref")) & "'), '" & FixQuotes(drutama("akcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("akinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("akinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)
                                        End If

                                    Next

                                    'Else
                                    '    result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                                'jenismutasi = 1
                                'dt = ambilData("SELECT jmltotal, satuandefault, aknoref, gudang, aknotransaksi notransaksi, akcustomer, namalayanan, idhppkhususmasuk, akcabang, aklokasi, akgudang, dtl.gudangtransit, akkodepa, aksumber, aknotransaksi, aktgl, idlayanan, tipebarang, jml, satuan, idakdetail, '" & uangFungsional & "' matauang, '" & kursFungsional & "' kurs, bhppaverage harga, 0 diskon, 0 jmldiskon, 0 idhppikm, bhpp, akuraian, akcatatan, catatan, costcenter, divisi, subdivisi, proyek, akinputtgl, akinputuser, bkode FROM m_11_ak utm JOIN m_11_ak_detail dtl ON dtl.idak = utm.akid JOIN m1_item i ON i.bid = dtl.idlayanan WHERE utm.akid = " + idutama + " AND dtl.idakdetail = " + iddetail, myConn)
                                'For x = 0 To dt.Rows.Count - 1
                                '    dr = dt.Rows(x)
                                '    str.Clear()

                                '    'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                '    str.Append("(" & 0 & ",'" & FixQuotes(dr("akcabang")) & "', '" & FixQuotes(dr("aklokasi")) & "', '" & FixQuotes(dr("gudang")) & "', " & dr("akkodepa") & ", " & jenismutasi & ", '" & FixQuotes(dr("aksumber")) & "', " & idutama & ", " & dr("idakdetail") & ", '" & dr("notransaksi") & "', '" & FixQuotes(AsFormatTanggal(dr("aktgl"))) & "', " & dr("akcustomer") & ", " & dr("idlayanan") & ", '" & FixQuotes(dr("namalayanan")) & "', '" & FixQuotes(dr("tipebarang")) & "', '" & FixQuotes(dr("bhpp")) & "', '" & FixDouble(dr("jml")) & "', '" & FixQuotes(dr("satuan")) & "', '" & FixDouble(dr("jmltotal")) & "', '" & FixQuotes(dr("satuandefault")) & "', '" & FixQuotes(dr("matauang")) & "', '" & FixDouble(dr("kurs")) & "', '" & FixDouble(dr("harga")) & "', '" & FixQuotes(dr("diskon")) & "', '" & FixDouble(dr("jmldiskon")) & "', " & dr("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', CONCAT('" & FixQuotes(dr("akuraian")) & "', ' ', '" & FixQuotes(dr("aknoref")) & "'), '" & FixQuotes(dr("akcatatan")) & "', '" & FixQuotes(dr("catatan")) & "', '" & FixQuotes(dr("costcenter")) & "', '" & FixQuotes(dr("divisi")) & "', '" & FixQuotes(dr("subdivisi")) & "', '" & FixQuotes(dr("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr("akinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr("akinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                '    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & str.ToString & ""
                                'Next
                            Case "RO"

                                Dim dtDetailNew As DataTable = ambilData("SELECT rod.idrodetail, rod.idlayanan, rod.namalayanan, rod.tipebarang, rod.jml, rod.satuan, rod.jmltotal, rod.satuandefault, rod.matauang, rod.kurs, rod.harga, rod.diskon, rod.jmldiskon, rod.idhppkhususkeluar, rod.hpp, rod.gudang, rod.gudangtransit, rod.gudangtujuan, rod.catatan, rod.costcenter, rod.divisi, rod.subdivisi, rod.proyek, ro.roinputtgl, i.bhpp FROM m_11_ro_detail rod JOIN m_11_ro ro ON rod.idro = ro.roid JOIN m1_item i ON rod.idlayanan = i.bid WHERE rod.idro = '" & result(4) & "' AND rod.idrodetail = '" & iddetail & "'", myConn)
                                Dim dtUtamalNew As DataTable = ambilData("SELECT * FROM m_11_ro WHERE roid = " & result(4), myConn)
                                Dim drutama As DataRow = dtUtamalNew.Rows(0)
                                notransaksi = drutama("ronotransaksi")
                                If dtDetailNew.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtDetailNew.Rows

                                        Dim isfound As Boolean = False
                                        For y = 0 To arrHitungUlang.Length - 1
                                            If dtDetailNew.Rows(0)("idlayanan") = arrHitungUlang(y) Then
                                                isfound = True
                                            End If
                                        Next

                                        If Not isfound Then
                                            arrHitungUlang(indexArray) = dtDetailNew.Rows(0)("idlayanan")
                                            indexArray = indexArray + 1
                                        End If

                                        'SET NILAI VARIABEL
                                        idbarang = Double.Parse(dr1("idlayanan"))
                                        jmlbarang = Double.Parse(dr1("jmltotal"))
                                        gudang = dr1("gudang")

                                        'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                        sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                                        dtSaldo = ambilData(sql, myConn)
                                        If dtSaldo.Rows.Count > 0 Then
                                            'set nilai stok
                                            bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                            'jenismutasi dan postinghpp 
                                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                            jenismutasi = 1 : postinghpp = 0

                                            'hitung saldojml = bstok - jmlbarang
                                            saldojml = bstok + jmlbarang

                                            'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                            hpp = 0 : saldohpp = 0 : saldonilai = 0

                                            'QUERY INSERT TRANSAKSI BARANG
                                            strTransaksiBarang.Clear()
                                            'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rocabang")) & "', '" & FixQuotes(drutama("rolokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rosumber")) & "', " & result(4) & ", " & dr1("idrodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & drutama("rocustomer") & ", " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rouraian")) & "', '" & FixQuotes(drutama("rocatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("roinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("roinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK PERGUDANG
                                            sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                            changeData(sql, myConn, Trans)

                                            'UPDATE STOK GLOBAL
                                            sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                            changeData(sql, myConn, Trans)

                                        End If

                                    Next

                                    'Else
                                    '    result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                                End If

                                'jenismutasi = 1
                                'dt = ambilData("SELECT idhppkhususmasuk, rocabang, rolokasi, rogudang, dtl.gudangtransit, dtl.gudangasal, rokodepa, rosumber, ronotransaksi, rotgl, rosupplier, idbarang, namabarang, tipebarang, jml, satuan, dtl.jmlbarang, idrodetail, dtl.satuanbarang, '" & uangFungsional & "' matauang, '" & kursFungsional & "' kurs, bhppaverage harga, 0 diskon, 0 jmldiskon, 0 idhppikm, bhpp, rouraian, rocatatan, catatan, costcenter, divisi, subdivisi, proyek, roinputtgl, roinputuser, bkode FROM m_11_ro utm JOIN m_11_ro_detail dtl ON dtl.idro = utm.roid JOIN m1_item i ON i.bid = dtl.idbarang WHERE utm.roid = " + idutama + " AND dtl.idrodetail = " + iddetail, myConn)
                                'For x = 0 To dt.Rows.Count - 1
                                '    dr = dt.Rows(x)
                                '    str.Clear()

                                '    'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                '    str.Append("(" & 0 & ",'" & FixQuotes(dr("rocabang")) & "', '" & FixQuotes(dr("rolokasi")) & "', '" & FixQuotes(dr("gudang")) & "', " & dr("rokodepa") & ", " & jenismutasi & ", '" & FixQuotes(dr("rosumber")) & "', " & idutama & ", " & dr("idrodetail") & ", '" & dr("notransaksi") & "', '" & FixQuotes(AsFormatTanggal(dr("rotgl"))) & "', " & dr("rocustomer") & ", " & dr("idlayanan") & ", '" & FixQuotes(dr("namalayanan")) & "', '" & FixQuotes(dr("tipebarang")) & "', '" & FixQuotes(dr("bhpp")) & "', '" & FixDouble(dr("jml")) & "', '" & FixQuotes(dr("satuan")) & "', '" & FixDouble(dr("jmltotal")) & "', '" & FixQuotes(dr("satuandefault")) & "', '" & FixQuotes(dr("matauang")) & "', '" & FixDouble(dr("kurs")) & "', '" & FixDouble(dr("harga")) & "', '" & FixQuotes(dr("diskon")) & "', '" & FixDouble(dr("jmldiskon")) & "', " & 0 & ", " & dr("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(dr("rouraian")) & "', '" & FixQuotes(dr("rocatatan")) & "', '" & FixQuotes(dr("catatan")) & "', '" & FixQuotes(dr("costcenter")) & "', '" & FixQuotes(dr("divisi")) & "', '" & FixQuotes(dr("subdivisi")) & "', '" & FixQuotes(dr("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr("roinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr("roinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                '    sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & str.ToString & ""
                                'Next
                        End Select

                    Next
                    For i = 0 To arr.Length - 1
                        'Set Default
                        hpp = 0 : postinghpp = 0 : bstok = 0
                        jenismutasi = 0 : saldojml = 0 : saldohpp = 0 : saldonilai = 0

                        '//bnama, notransaksi, sumber, id, iddetail
                        sumber = arr(i).Split("|")(2)
                        idutama = arr(i).Split("|")(3)
                        result(4) = idutama
                        iddetail = arr(i).Split("|")(4)
                        jenismutasi = arr(i).Split("|")(5)

                        Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                        'BUAT ID UNIQUE
                        mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)
                        dt = ambilData("SELECT * FROM M0_Msmq_Cogs WHERE mcid = '" + mjid + "'", myConn)

                        If dt.Rows.Count < 1 Then
                            'MSMQ TABEL
                            sql = "Insert into M0_Msmq_Cogs(mcid, mcsumber, mcidtransaksi, mcprogress, mcpesan, mctglantrian, mctglselesai, mcuserid) values ('" _
                                & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                            changeData(sql, myConn, Trans)

                            'MSMQ ANTRIAN
                            Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                            If ProsesHpp.Equals("0") = False Then
                                hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                                If Len(hasilMsmq) > 0 Then
                                    result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If
                    Next



                    'Dim tgl As String = "", strJournal As String = "", rsJournal() As String, rsResult() As String
                    'dt = ambilData("SELECT CONCAT(aptahun, '-', apbulan, '-01') tgl, CONCAT(aptahun, '-', apbulan, '-31') tglakhir FROM m2_accounting_period WHERE aptutupperiode = 0 ORDER BY aptahun, apbulan", myConn)


                    '                    Trans.Commit()  '*** Commit Transaction ***'

                    '                    tgl = dt.Rows(0)("tgl")
                    '                    For j = 9999 To indexArray - 1
                    '                        For i = 0 To arr.Length - 1

                    'ulangfor:
                    '                            'result(2) = paramSplit(0) & "★M0_CogsHitungUlang_Average★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tgl & "▼" & dt.Rows(dt.Rows.Count - 1)("tglakhir") & "▼" & arrHitungUlang(j) : GoTo selesai
                    '                            Dim M0_CogsHitungUlang As New m0_cogs
                    '                            'strJournal = M0_CogsHitungUlang.M0_CogsHitungUlang_AveragePerBarang(paramSplit(0) & "★M0_CogsHitungUlang_AveragePerBarang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tgl & "▼" & dt.Rows(dt.Rows.Count - 1)("tglakhir") & "▼" & arrHitungUlang(j))
                    '                            'Return strJournal
                    '                            '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
                    '                            rsJournal = strJournal.Split(sptParam)

                    '                            '// JIKA KEMBALIAN FUNGSI JURNAL <> 3 MAKA SALAH
                    '                            If rsJournal.Length = 3 Then
                    '                                '// AMBIL BAGIAN RESULT DARI FUNGSI JURNAL - result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
                    '                                rsResult = rsJournal(0).Split(sptSubParam)
                    '                                '// JIKA BAGIAN RESULT DARI FUNGSI JURNAL <> 5 MAKA SALAH
                    '                                If rsResult.Length = 5 Then
                    '                                    If rsResult(1) <> 1 And rsResult(1) <> 4 Then '// JIKA GAGAL - KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
                    '                                        If rsResult(2).Contains("try restarting transaction") Then
                    '                                            GoTo ulangfor
                    '                                        End If
                    '                                        result(2) = "Cogs proccess failed. " & arrHitungUlang(j) & ". " & rsResult(2) & "" : GoTo selesai
                    '                                    End If
                    '                                Else
                    '                                    result(2) = "Cogs proccess failed. " & arrHitungUlang(j) & ". Invalid result data #2'" : GoTo selesai
                    '                                End If

                    '                            Else
                    '                                result(2) = "Cogs proccess failed. " & arrHitungUlang(j) & ". Invalid result data #1'" : GoTo selesai
                    '                            End If

                    '                        Next
                    '                    Next
                    'data = "(" + data.Substring(0, data.Length - 2) + ")"

                    ' update per gudang dan global
                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "DELETE FROM m1_item_stock_warehouse;INSERT INTO m1_item_stock_warehouse (SELECT b.bid, tb.gudang,	sum(	(CASE tb.jenismutasi	WHEN 1 THEN tb.jmlbarang	ELSE tb.jmlbarang * -1 	END)	) as stokfix	FROM m1_item_transaction tb	JOIN m1_item b ON tb.idbarang = b.bid	WHERE b.bjenis = 'P' GROUP BY tb.idbarang, tb.gudang)"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With

                    With New MySql.Data.MySqlClient.MySqlCommand()
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = "UPDATE `m1_item` SET `bstok`='0';UPDATE(SELECT idbarang, round(SUM(stok), 5) stok FROM m1_item_stock_warehouse GROUP BY idbarang) h JOIN m1_item i ON i.bid = h.idbarang SET i.bstok = h.stok"
                        .ExecuteNonQuery()
                        .Dispose()
                    End With

                Case "Stokbatchvspergudang"


                    'hHitungUlangHPP
                Case "HitungUlangHPP"
                    Dim tglakhir As String = "", tgl As String = "", strJournal As String = "", rsJournal() As String, rsResult() As String, HppGlobal As String
                    dt = ambilData("SELECT snilai FROM `m0_setting` WHERE sgrup = 'company' AND skode = 'HppGlobal'", myConn)
                    HppGlobal = dt.Rows(0)("snilai")
                    dt = ambilData("SELECT CONCAT(aptahun, '-', apbulan, '-01') tgl, LAST_DAY(CONCAT(aptahun, '-', apbulan, '-01')) tglakhir FROM m2_accounting_period WHERE aptutupperiode = 0 ORDER BY aptahun, apbulan", myConn)
                    tgl = dt.Rows(0)("tgl")
                    tglakhir = dt.Rows(dt.Rows.Count - 1)("tglakhir")
                    arr = searchdata.Split(sptLogin)

                    For k = 0 To arr.Length - 1

ulanghitungulanghpp:
                        'result(2) = paramSplit(0) & "★M0_CogsHitungUlang_AveragePerBarang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tgl & "▼" & tglakhir & "▼" & dt.Rows(i)(0) : GoTo selesai
                        Dim M0_CogsHitungUlang As New m0_cogs
                        If HppGlobal = "F" Then
                            strJournal = M0_CogsHitungUlang.M0_CogsHitungUlang_Fifo(paramSplit(0) & "★M0_CogsHitungUlang_Fifo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tgl & "▼" & tglakhir & "▼" & arr(k).ToString.Split("|")(1) & "▼" & arr(k).ToString.Split("|")(1))
                        ElseIf HppGlobal = "R" Then
                            strJournal = M0_CogsHitungUlang.M0_CogsHitungUlang_AveragePerBarang(paramSplit(0) & "★M0_CogsHitungUlang_AveragePerBarang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & tgl & "▼" & tglakhir & "▼" & arr(k).ToString.Split("|")(1))
                        End If
                        'Return strJournal
                        '// FORMAT kembalian fungsi jurnal = result★paging★data, yg diambil bagian result saja. 
                        rsJournal = strJournal.Split(sptParam)

                        '// JIKA KEMBALIAN FUNGSI JURNAL <> 3 MAKA SALAH
                        If rsJournal.Length = 3 Then
                            '// AMBIL BAGIAN RESULT DARI FUNGSI JURNAL - result = target(0)△success(2)△errmessage(2)△errstep(3)△idtransaksi(4)
                            rsResult = rsJournal(0).Split(sptSubParam)
                            '// JIKA BAGIAN RESULT DARI FUNGSI JURNAL <> 5 MAKA SALAH
                            If rsResult.Length = 5 Then
                                If rsResult(1) <> 1 And rsResult(1) <> 4 Then '// JIKA GAGAL - KIRIM INFORMASI PROSES GAGAL, TAMPILKAN ERRMESSAGE
                                    If rsResult(2).Contains("try restarting transaction") Then
                                        GoTo ulanghitungulanghpp
                                    End If
                                    result(2) = "Cogs proccess failed. " & arr(k).ToString.Split("|")(0) & ". " & rsResult(2) & "" : GoTo selesai
                                End If
                            Else
                                result(2) = "Cogs proccess failed. " & arr(k).ToString.Split("|")(0) & ". Invalid result data #2'" : GoTo selesai
                            End If

                        Else
                            result(2) = "Cogs proccess failed. " & arr(k).ToString.Split("|")(0) & ". Invalid result data #1'" : GoTo selesai
                        End If
                    Next

                    'hJurnalUlang
                Case "JurnalUlang"
                    dt = ambilData("SELECT snilai FROM `m0_setting` WHERE sgrup = 'validitasdata' AND skode = 'jurnalulang'", myConn)
                    For i = 0 To dt.Rows.Count - 1
                        searchdata += dt.Rows(i)(0).ToString + sptLogin
                    Next

                    If searchdata.Length <> 0 Then
                        searchdata = searchdata.Substring(0, searchdata.Length - 2)
                    End If
                    search = dt.Rows.Count.ToString

                Case Else
                    result(2) = "Invalid packet" : GoTo selesai
            End Select

            result(1) = 1
            Try

                Trans.Commit()  '*** Commit Transaction ***'
            Catch ex As Exception

            End Try
        Catch ex As Exception
            If Err.Description = "Unable to cast object of type 'System.String' to type 'System.Data.DataTable'." And errstring <> "" Then
                result(2) = "Paket : " + paket + " - " + errstring
            Else
                result(2) = "Paket : " + paket + " - " + Err.Description + " - " + xString
            End If

            Try
                Trans.Rollback() '*** RollBack Transaction ***'  
            Catch ex1 As Exception
                result(2) += " " + Err.Description
            End Try
        End Try

        myConn.Close()
selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, searchdata)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmlbelumfix"), sptSubParam, ReplaceMapping("data"))

        Return wsResult
    End Function

    Function f_TglFilter(ByVal kolom As String, ByVal tgl1 As String, ByVal tgl2 As String) As String
        If tgl1 <> "" And tgl2 <> "" Then
            Return kolom + " >= '" + tgl1 + "' AND " + kolom + " <= '" + tgl2 + "'"
        ElseIf tgl2 <> "" Then
            Return kolom + " <= '" + tgl2 + "'"
        ElseIf tgl1 <> "" Then
            Return kolom + " >= '" + tgl1 + "'"
        Else
            Return kolom + " LIKE '%%'"
        End If
        Return ""
    End Function

    Function ambilData(ByVal query As String, ByVal myConn As MySqlConnection) As Object
        Try
            Dim dt As New DataTable
            Dim obj As MySql.Data.MySqlClient.MySqlCommand = New MySql.Data.MySqlClient.MySqlCommand()
            With obj
                .Connection = myConn
                .CommandType = CommandType.Text
                .CommandText = query
                .CommandTimeout = 0
                .Dispose()
            End With
            Dim obje As Object = obj.ExecuteReader()
            dt.Load(obje)
            obje.Close()
            Return dt
        Catch ex As Exception

            errstring = Err.Description + vbNewLine + query
            Return Err.Description
        End Try
    End Function

    Sub changeData(ByVal query As String, ByVal myConn As MySqlConnection, ByVal Trans As MySqlTransaction)
        Try
            Dim obj As MySql.Data.MySqlClient.MySqlCommand = New MySql.Data.MySqlClient.MySqlCommand()
            With obj
                .Connection = myConn
                .CommandType = CommandType.Text
                .Transaction = Trans
                .CommandText = query
                .CommandTimeout = 0
                .ExecuteNonQuery()
                .Dispose()
            End With
        Catch ex As Exception
            errstring = Err.Description + vbNewLine + query
        End Try
    End Sub
End Class