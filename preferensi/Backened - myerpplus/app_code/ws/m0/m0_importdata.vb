Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.Data.OleDb
Imports System.Globalization

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_importdata

    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_ImportdataSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", stepSheet As Double = 0, stepKe As Double = 0, Prosentase As Double = 100
        Dim progress As Double = 0, progressPersen As Double = 0, pesan As String = "", tglselesai As String = "'1971-01-01 00:00:00'"
        Dim miid As String = ""

        Dim myPath As String = ""
        Dim sPath As String = "", rsReadExcel As String = ""
        Dim sumber As String = ""
        Dim filepaket As String = "", filenama As String = ""
        Dim filesheet As String = ""

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


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'miid(0) As String, misumber(1) As String, miprogresspersen(2) As Integer, miprogress(3) As Integer, mipesan(4) As String, 
        'mitglantrian(5) As DateTime, mitglselesai(6) As DateTime, miuserid(7) As Integer, mipaket(8) As String, minamafile(9) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'miid, misumber, miprogresspersen, miprogress, mipesan, mitglantrian, mitglselesai, 
        'miuserid, mipaket, minamafile


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = dataSplit(0).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 10) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'miprogresspersen(2) As Integer
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "miprogresspersen required numeric." : GoTo selesai
        End If
        'miprogress(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "miprogress required numeric." : GoTo selesai
        End If
        'mitglantrian(5) As DateTime
        If (IsDate(dataUtama(5)) = False) Then
            result(2) = "mitglantrian required date." : GoTo selesai
        End If
        'mitglselesai(6) As DateTime
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "mitglselesai required date." : GoTo selesai
        End If
        'miuserid(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "miuserid required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'VALIDASI DATA ===============================================================
        'miid(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "miid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 50 Then
            result(2) = "miid should not be more than 50 character." : GoTo selesai
        End If

        'misumber(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "misumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 10 Then
            result(2) = "misumber should not be more than 10 character." : GoTo selesai
        End If
        If dataUtama(1).ToLower <> "val" And dataUtama(1).ToLower <> "imp" Then
            result(2) = "invalid packet form misumber. (val = validation, imp = import)" : GoTo selesai
        End If

        'mitglantrian(5) As DateTime
        If Len(dataUtama(5)) = 0 Then
            result(2) = "mitglantrian can't be empty" : GoTo selesai
        End If

        'mitglselesai(6) As DateTime
        If Len(dataUtama(6)) = 0 Then
            result(2) = "mitglselesai can't be empty" : GoTo selesai
        End If

        'mipaket(8) As String
        If Len(dataUtama(8)) = 0 Then
            result(2) = "mipaket can't be empty" : GoTo selesai
        End If

        'minamafile(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "minamafile can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA ========================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'namasheet(0) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'namasheet


        'VALIDASI DAN SET DATA DETAIL ================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL =========================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "namasheet", AsEnumTypeData.AsString)


        'VALIDASI DAN SET DATA ROW DETAIL ============================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL ---------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 1) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL --------------------


            'VALIDASI DATA DETAIL ---------------------------------------
            'namasheet(0) As String
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - namasheet can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "namasheet", dataRowDetail(0)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL =====================================


        'SIMPAN KE DATABASE ==========================================================
        Dim Con1 As MySql.Data.MySqlClient.MySqlConnection
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                rowUpdate = 1
                If (rowUpdate > 0) Then
                    sql = "Update M0_Msmq_Importdata set miprogress  = " & dataUtama(3) & ", mipesan  = '" & FixQuotes(dataUtama(4)) & "', mitglselesai  = NOW(), miprogresspersen = " & dataUtama(2) & " where miid  = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else

                'GENERATE MIID
                Dim Security As New ClsSecurity
                dataUtama(0) = Security.MD5CalcString(userid & dataUtama(1) & dataUtama(9) & Now)

                'sql = "Insert into M0_Msmq_Importdata (miid, misumber, miprogresspersen, miprogress, mipesan, mitglantrian, mitglselesai, miuserid, mipaket, minamafile) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixDouble(dataUtama(2)) & "', " & dataUtama(3) & ", '" & FixQuotes(dataUtama(4)) & "', NOW(), '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "')"
                sql = "Insert into M0_Msmq_Importdata (miid, misumber, miprogresspersen, miprogress, mipesan, mitglantrian, mitglselesai, miuserid, mipaket, minamafile) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixDouble(dataUtama(2)) & "', " & dataUtama(3) & ", '', NOW(), '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

            End If

            Trans.Commit()  '*** Commit Transaction ***'
            'result(1) = 1
            'result(2) = notransaksi
            'result(3) = 0
            'result(4) = result(4)

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
            result(3) = 0
            result(4) = result(4)
            GoTo selesai

        End Try

        objCmd = Nothing
        'Con1.Close()
        'END OF SIMPAN KE DATABASE ==========================================================


        'PROSES IMPORT DATA =================================================================
        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'DataTable untuk menampung data dari excel
            Dim dtExcelData As New DataTable

            'Url Path
            myPath = HttpContext.Current.Server.MapPath("~/") & "importdata\files\"

            'misumber(1) As String ==> val : validasi, imp : import
            sumber = dataUtama(1)
            'mipaket(8) As String                 , minamafile(9) As String
            filepaket = dataUtama(8) : filenama = dataUtama(9)
            miid = dataUtama(0)

            'Proses sheet excel
            If (dtdetail.Rows.Count > 0) Then

                Dim dtTableData As DataTable
                Dim strImport As String = "", strField As String = "", strValues As String = ""
                Dim rowStart As Double = 0, drExcel As DataRow, rsWSTransaksi(3) As String, rsWSResult(4) As String

                'AMBIL STRUKTUR TABEL TUJUAN IMPORT
                dtTableData = AsDataTableAmbilDariDB("SHOW COLUMNS FROM " & filepaket) 'Field, Type, Null, Key, Default, Extra
                'BUAT STRUKTUR NAMA FIELD QUERY INSERT
                If dtTableData.Rows.Count > 0 Then
                    For Each dr As DataRow In dtTableData.Rows
                        strField = IIf(Len(strField.ToString) = 0, "", strField & ", ")
                        strField = String.Concat(strField, dr("Field"))
                    Next
                    If Len(strField) > 0 Then strField = "(" & strField & ")"

                    'JIKA PAKET CB MAKA TAMBAH KOLOM
                    If filepaket.ToLower.Equals("m2_cb_detail") Then
                        'DATA TABLE DITAMBAHKAN KOLOM UNTUK KEPERLUAN DATA UTAMA
                        'cbtgl, cbkontak, cburaian, cbcatatan
                        If AsDataTableTambahData(dtTableData, "Field~Type~Null~Key~Default~Extra", "cbtgl~date~NO~~~") = False Then result(2) = "m2_cb_detail : cbtgl - insert into datatable failed." : Trans.Rollback() : GoTo selesai
                        If AsDataTableTambahData(dtTableData, "Field~Type~Null~Key~Default~Extra", "cbkontak~bigint(20)~NO~~~") = False Then result(2) = "m2_cb_detail : cbkontak - insert into datatable failed." : Trans.Rollback() : GoTo selesai
                        If AsDataTableTambahData(dtTableData, "Field~Type~Null~Key~Default~Extra", "cburaian~varchar(250)~YES~~~") = False Then result(2) = "m2_cb_detail : cburaian - insert into datatable failed." : Trans.Rollback() : GoTo selesai
                        If AsDataTableTambahData(dtTableData, "Field~Type~Null~Key~Default~Extra", "cbcatatan~varchar(250)~YES~~~") = False Then result(2) = "m2_cb_detail : cbcatatan - insert into datatable failed." : Trans.Rollback() : GoTo selesai
                    End If

                Else
                    result(2) = "Table name '" & filepaket & "' doesn't exist in database." : Trans.Rollback() : GoTo selesai

                End If

                'Perulangan sebanyak sheet excel yang dipilih
                For Each dr1 As DataRow In dtdetail.Rows
                    'SET STEP SHEET KE
                    stepSheet = stepSheet + 1
                    stepKe = 0

                    'namasheet(0) As String
                    filesheet = dr1("namasheet").ToString

                    'Set folder dan nama file excel
                    sPath = myPath + filenama

                    'PROSES READ FILE EXCEL -------------------------
                    'Panggil fungsi ReadExcelFile untuk membaca file excel dan ditampung pada datatable
                    rsReadExcel = "" : dtExcelData.Clear() : dtExcelData.Reset() : dtExcelData.Dispose()
                    rsReadExcel = ReadExcelFile(sPath, filesheet, dtExcelData)
                    If Len(rsReadExcel) > 0 Then
                        result(2) = rsReadExcel : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF PROSES READ FILE EXCEL ------------------

                    'result(2) = dtExcelData.Rows.Count : Trans.Rollback() : GoTo selesai
                    'PROSES IMPORT KE TABEL -------------------------
                    'AMBIL VALUES DARI DATATABLE DATA EXCEL YANG AKAN DIIMPORT
                    If dtExcelData.Rows.Count > 0 Then
                        Dim sptDataTipe() As String, sptDataLength() As String
                        Dim namaField As String = "", dataTipe As String = "", dataLength As String = ""
                        Dim AllowNull As String = "", dataDefault As String = ""
                        Dim dtUser As New DataTable, cabang As String = "", lokasi As String = ""


                        'PROSES IMPORT DIBEDAKAN BERDASARKAN PAKET
                        Select Case filepaket.ToLower

                            '    IMPORT PENYESUAIAN STOK ==========================================================
                            Case "m3_sa_detail"

                                Dim WsSA As New wsm3_sa
                                'Dim drNext As DataRow

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> 7 Then
                                    result(2) = filesheet & " - Column count doesn't match with Stock Adjusment Template." : Trans.Rollback() : GoTo selesai
                                End If

                                'AMBIL DATA UNTUK UTAMA, DARI PESAN (mipesan(4) As String)
                                'tanggal(0), kontak(1), jenispenyesuaian(2), uraian(3), catatan(4), gudang(5) -> displit dengan karakter '|'

                                Dim mipesan As String = dataUtama(4)
                                'If mipesan.Contains("|") Then result(2) = "mipesan can't contains '|' characters." : Trans.Rollback() : GoTo selesai

                                Dim splitPesan As String() = mipesan.Split("|")
                                If splitPesan.Length <> 6 Then
                                    result(2) = "Invalid mipesan parameter." : Trans.Rollback() : GoTo selesai
                                End If

                                Dim tanggal As String = splitPesan(0)
                                Dim kontak As Integer = 0
                                If IsNumeric(splitPesan(1)) = False Then
                                    result(2) = "Kontak required numeric." : Trans.Rollback() : GoTo selesai
                                Else
                                    kontak = splitPesan(1)
                                End If

                                Dim jenispenyesuaian As String = splitPesan(2), uraian As String = splitPesan(3)
                                Dim catatan As String = splitPesan(4), gudang As String = splitPesan(5), reklawan As String = ""

                                'AMBIL JENIS PENYESUAIAN DARI MASTER m1_type_sa
                                sql = "SELECT tsakode, tsarek FROM m1_type_sa WHERE tsakode = '" & FixQuotes(jenispenyesuaian) & "'"
                                Dim dtTypeSA As DataTable = AsDataTableAmbilDariDB(sql)
                                If dtTypeSA.Rows.Count > 0 Then
                                    If Len(FxDB(dtTypeSA.Rows(0)("tsarek").ToString, "")) > 0 Then
                                        reklawan = FxDB(dtTypeSA.Rows(0)("tsarek").ToString, "")
                                    Else
                                        result(2) = "CoA for '" & jenispenyesuaian & "' does not found in Stock Adjustment Type data." : Trans.Rollback() : GoTo selesai
                                    End If
                                Else
                                    result(2) = "'" & jenispenyesuaian & "' does not found in Stock Adjustment Type data." : Trans.Rollback() : GoTo selesai

                                End If

                                'AMBIL CABANG DAN LOKASI USER
                                dtUser = AsDataTableAmbilDariDB("SELECT unama, ucabang, ulokasi FROM m0_user WHERE userid = '" & userid & "'")
                                If dtUser.Rows.Count > 0 Then
                                    cabang = dtUser.Rows(0)("ucabang")
                                    lokasi = dtUser.Rows(0)("ulokasi")

                                Else
                                    result(2) = filesheet & " - User data not found." : Trans.Rollback() : GoTo selesai

                                End If

                                Dim dtGudang As New DataTable, dtBarang As New DataTable
                                Dim bid As Integer = 0, btipe As String = "", bsatuan As String = "", bhppaverage As Double = 0, brekpersediaan As String = "", bnama As String = ""
                                Dim jmlmasuk As Double = 0, jmlkeluar As Double = 0

                                'PROSES BUAT PARAMETER TRANSAKSI UTAMA -------------------------------------
                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    'PROSES POSTING TRANSAKSI JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then
                                        drExcel = dtExcelData.Rows(iRow)

                                        'AMBIL GUDANG DARI MASTER m1_warehouse
                                        sql = "SELECT wkode FROM m1_warehouse WHERE wkode = '" & FixQuotes(gudang) & "'"
                                        dtGudang = AsDataTableAmbilDariDB(sql)
                                        If dtGudang.Rows.Count > 0 Then
                                            If Len(FxDB(dtGudang.Rows(0)("wkode").ToString, "")) > 0 Then
                                                gudang = FxDB(dtGudang.Rows(0)("wkode").ToString, "")
                                            Else
                                                result(2) = "'" & gudang & "' does not found in Warehouse data." : Trans.Rollback() : GoTo selesai
                                            End If
                                        Else
                                            result(2) = "'" & gudang & "' does not found in Warehouse data." : Trans.Rollback() : GoTo selesai

                                        End If

                                        'AMBIL BARANG DARI MASTER m1_item
                                        sql = "SELECT bid, bnama, btipe, bsatuan, bhppaverage, brekpersediaan FROM m1_item WHERE bkode = '" & FixQuotes(FxDB(drExcel("kodebarang"), "")) & "'"
                                        dtBarang = AsDataTableAmbilDariDB(sql)
                                        If dtBarang.Rows.Count > 0 Then
                                            If Len(FxDB(dtBarang.Rows(0)("bid"), "")) > 0 Then
                                                bid = FxDB(dtBarang.Rows(0)("bid"), 0)
                                                btipe = FxDB(dtBarang.Rows(0)("btipe"), "")
                                                bsatuan = FxDB(dtBarang.Rows(0)("bsatuan"), "")
                                                bhppaverage = FxDB(dtBarang.Rows(0)("bhppaverage"), 0)
                                                brekpersediaan = FxDB(dtBarang.Rows(0)("brekpersediaan"), "")
                                                bnama = FxDB(dtBarang.Rows(0)("bnama"), "")
                                            End If

                                        Else
                                            result(2) = "'" & FxDB(drExcel("kodebarang"), "") & "' does not found in Item data." : Trans.Rollback() : GoTo selesai

                                        End If

                                        'MAPPING EXCEL
                                        'no, kodebarang, namabarang, gudang, jmlmasuk, jmlkeluar, hpp, 
                                        'hargabeli, akunlawan, namaakunlawan, catatan

                                        'BUAT DATA MENJADI PARAMETER WS TRANSAKSI DETAIL
                                        'MAPPING :
                                        'idsadetail, idsa, idbarang, namabarang, tipebarang, jmlmasuk, jmlkeluar, 
                                        'satuan, nilaisatuan, jmlbarangmasuk, jmlbarangkeluar, satuanbarang, idhppkhususmasuk, hpplama, 
                                        'hpp, rekpersediaan, reklawan, idspdetail, cabang, lokasi, gudang, 
                                        'costcenter, divisi, subdivisi, proyek, catatan, urutan, isclose, 
                                        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
                                        'customdate2, customdate3

                                        'SET JMLMASUK DAN JMLKELUAR
                                        If FixDouble(FxDB(drExcel("jmlmasuk"), 0)) >= 0 Then
                                            jmlmasuk = Double.Parse(FixDouble(FxDB(drExcel("jmlmasuk"), 0))) : jmlkeluar = 0
                                        Else
                                            jmlkeluar = Math.Abs(Double.Parse(FixDouble(FxDB(drExcel("jmlmasuk"), 0)))) : jmlmasuk = 0
                                        End If

                                        'PARAMETER DATA
                                        strValues &= IIf(Len(strValues) > 0, sptRow, "")
                                        '    idsadetail,          idsa,                 idbarang,                   namabarang,                     tipebarang,                               jmlmasuk,                    jmlkeluar, 
                                        strValues &= 0 & sptField & 0 & sptField & FixDouble(bid) & sptField & FixQuotes(bnama) & sptField & FixQuotes(btipe) & sptField & FixDouble(FxDB(jmlmasuk, 0)) & sptField & FixDouble(FxDB(jmlkeluar, 0)) & sptField
                                        '                       satuan,     nilaisatuan,                           jmlbarangmasuk, jmlbarangkeluar,                   satuanbarang, idhppkhususmasuk,                     hpplama, 
                                        strValues &= FixQuotes(bsatuan) & sptField & 1 & sptField & FixDouble(FxDB(jmlmasuk, 0)) & sptField & FixDouble(FxDB(jmlkeluar, 0)) & sptField & FixQuotes(bsatuan) & sptField & 0 & sptField & FixDouble(bhppaverage) & sptField
                                        '                               hpp,                           rekpersediaan,                        reklawan,      idspdetail,            cabang,             lokasi,                       gudang, 
                                        strValues &= FixDouble(FxDB(drExcel("hpp"), 0)) & sptField & FixQuotes(brekpersediaan) & sptField & FixQuotes(reklawan) & sptField & 0 & sptField & cabang & sptField & lokasi & sptField & FixQuotes(gudang) & sptField
                                        '     costcenter,         divisi,      subdivisi,         proyek,                               catatan,                                   urutan,       isclose, 
                                        strValues &= "" & sptField & "" & sptField & "" & sptField & "" & sptField & FixQuotes(FxDB(drExcel("catatan"), "")) & sptField & FixDouble(FxDB(drExcel("no"), 0)) & sptField & 0 & sptField
                                        '    customtext1,    customtext2,    customtext3,                              customdbl1,        customdbl2,    customdbl3,             customdate1, 
                                        strValues &= "" & sptField & "" & sptField & "" & sptField & FixDouble(FxDB(drExcel("hargabeli"), 0)) & sptField & 0 & sptField & 0 & sptField & "1900-01-01" & sptField
                                        '             customdate2,             customdate3
                                        strValues &= "1900-01-01" & sptField & "1900-01-01"

                                        'JIKA ROW TERAKHIR MAKA BUAT PARAM UTAMA, DAN SIMPAN TRANSAKSI
                                        If iRow = dtExcelData.Rows.Count - 1 Then

                                            'BUAT PARAMETER WS TRANSAKSI UTAMA
                                            'MAPPING :
                                            'said, sacabang, salokasi, sagudang, sasumber, sajenis, saautonotransaksi, 
                                            'sanotransaksi, satgl, sakodepa, sabagiansa, sabagiansakontak, sauraian, sacatatan, 
                                            'sanoref, satglnoref, saidsp, sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, 
                                            'sainputuser, sainputtgl, samodifikasiuser, samodifikasitgl, saposting, satutupperiode, saisclose, 
                                            'sacustomtext1, sacustomtext2, sacustomtext3, sacustomtext4, sacustomtext5, sacustomint1, sacustomint2, 
                                            'sacustomint3, sacustomdbl1, sacustomdbl2, sacustomdbl3, sacustomdate1, sacustomdate2, sacustomdate3

                                            '        said,                    sacabang,                      salokasi,                      sagudang,             sasumber,                      sajenis,         saautonotransaksi, 
                                            strField = 0 & sptField & FixQuotes(cabang) & sptField & FixQuotes(lokasi) & sptField & FixQuotes(gudang) & sptField & "SA" & sptField & FixQuotes(jenispenyesuaian) & sptField & 1 & sptField
                                            '     sanotransaksi,                        satgl,        sakodepa,                  sabagiansa, sabagiansakontak,                    sauraian,                      sacatatan, 
                                            strField &= "Auto" & sptField & FixQuotes(tanggal) & sptField & 0 & sptField & FixDouble(kontak) & sptField & "" & sptField & FixQuotes(uraian) & sptField & FixQuotes(catatan) & sptField
                                            '       sanoref,             satglnoref,          saidsp,      sastatus, sastatussebelumnya, sajmlrevisi, sacetakanke, 
                                            strField &= "" & sptField & "1900-01-01" & sptField & 0 & sptField & 2 & sptField & 0 & sptField & 0 & sptField & 0 & sptField
                                            '                sainputuser,                        sainputtgl, samodifikasiuser,                samodifikasitgl,       saposting, satutupperiode,    saisclose, 
                                            strField &= FixDouble(userid) & sptField & "1971-01-01 00:00:00" & sptField & 0 & sptField & "1971-01-01 00:00:00" & sptField & 0 & sptField & 0 & sptField & 0 & sptField
                                            ' sacustomtext1,  sacustomtext2,  sacustomtext3,  sacustomtext4,  sacustomtext5,  sacustomint1,  sacustomint2, 
                                            strField &= "" & sptField & "" & sptField & "" & sptField & "" & sptField & "" & sptField & 0 & sptField & 0 & sptField
                                            ' sacustomint3,  sacustomdbl1,  sacustomdbl2,  sacustomdbl3,          sacustomdate1,            sacustomdate2,            sacustomdate3
                                            strField &= 0 & sptField & 0 & sptField & 0 & sptField & 0 & sptField & "1900-01-01" & sptField & "1900-01-01" & sptField & "1900-01-01"


                                            'PARAMETER WS TRANSAKSI
                                            strImport = paramSplit(0) & "★M3_SaSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strField & sptSubParam & strValues & sptSubParam & sptSubParam
                                            strField = "" : strValues = ""

                                            'POSTING PARAMETER DATA KE WS TRANSAKSI
                                            rsWSTransaksi = WsSA.M3_SaSimpan(strImport).Split(sptParam)
                                            rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                            If rsWSResult(1) = 0 Then
                                                result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                            End If

                                        End If

                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next
                                'END OF PROSES BUAT PARAMETER TRANSAKSI UTAMA ------------------------------


                                '    IMPORT SALDO COA ==========================================================
                            Case "m2_cb_detail"

                                Dim WsCB As New m2_cb
                                Dim drNext As DataRow

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> dtTableData.Rows.Count Then
                                    result(2) = filesheet & " - Column count doesn't match with '" & filepaket & "' table." : Trans.Rollback() : GoTo selesai
                                End If

                                'AMBIL CABANG DAN LOKASI USER
                                dtUser = AsDataTableAmbilDariDB("SELECT unama, ucabang, ulokasi FROM m0_user WHERE userid = '" & userid & "'")
                                If dtUser.Rows.Count > 0 Then
                                    cabang = dtUser.Rows(0)("ucabang")
                                    lokasi = dtUser.Rows(0)("ulokasi")

                                Else
                                    result(2) = filesheet & " - User data not found." : Trans.Rollback() : GoTo selesai

                                End If


                                'PROSES BUAT PARAMETER TRANSAKSI UTAMA -------------------------------------
                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
                                    For iField = 0 To dtTableData.Rows.Count - 1

                                        'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
                                        namaField = dtTableData.Rows(iField)("Field").ToString
                                        AllowNull = dtTableData.Rows(iField)("Null").ToString
                                        dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

                                        'AMBIL TIPEDATA DAN LENGTH VALUE
                                        sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
                                        If sptDataTipe.Length > 1 Then
                                            sptDataLength = sptDataTipe(1).Split(")")
                                        Else
                                            sptDataLength = "".Split("")
                                        End If
                                        dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

                                        'SET DEFAULT VALUE
                                        If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            If Len(dataDefault) > 0 Then
                                                dtExcelData.Rows(iRow)(iField) = dataDefault

                                            Else
                                                '    NUMERIC
                                                If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                                   dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                                   dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                                   dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
                                                    dtExcelData.Rows(iRow)(iField) = 0

                                                    'YEAR
                                                ElseIf dataTipe.Equals("year") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900"

                                                    'DATE
                                                ElseIf dataTipe.Equals("date") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900-01-01"

                                                    'TIME
                                                ElseIf dataTipe.Equals("time") Then
                                                    dtExcelData.Rows(iRow)(iField) = "00:00:00"

                                                    'DATETIME
                                                ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

                                                End If

                                            End If

                                        End If

                                        'CEK ALLOWNULL
                                        If AllowNull.Equals("NO") And Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            result(2) = filesheet & " - Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                        End If

                                        'VALIDASI TIPEDATA DAN LENGTH VALUE
                                        'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
                                        'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
                                        'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

                                        '    NUMERIC
                                        If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                           dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                           dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                           dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
                                           dataTipe.Equals("year") Then
                                            If IsNumeric(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If

                                            'DATE
                                        ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
                                           dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                            If IsDate(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If
                                            'FORMATTING TANGGAL
                                            If dataTipe.Equals("date") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd")
                                            ElseIf dataTipe.Equals("time") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "H:mm:ss")
                                            ElseIf dataTipe.Equals("timestamp") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            ElseIf dataTipe.Equals("datetime") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            End If

                                            'SELAIN NUMERIC DAN TANGGAL
                                        Else
                                            'CEK LENGTH DATA
                                            If Len(dataLength) > 0 Then
                                                If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) > Double.Parse(dataLength) Then
                                                    result(2) = filesheet & " - Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If

                                        End If

                                    Next


                                    'PROSES POSTING TRANSAKSI JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then

                                        'BUAT DATA MENJADI PARAMETER WS TRANSAKSI DETAIL
                                        'MAPPING :
                                        'idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, 
                                        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
                                        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
                                        'customdbl3, customdate1, customdate2, customdate3

                                        drExcel = dtExcelData.Rows(iRow)
                                        'PARAMETER DATA
                                        strValues &= IIf(Len(strValues) > 0, sptRow, "")
                                        strValues &= drExcel("idcbdetail") & sptField & drExcel("idcb") & sptField & drExcel("norek") & sptField & drExcel("matauang") & sptField
                                        strValues &= drExcel("kurs") & sptField & drExcel("debit") & sptField & drExcel("debitvalas") & sptField
                                        strValues &= drExcel("kredit") & sptField & drExcel("kreditvalas") & sptField & drExcel("catatan") & sptField & drExcel("costcenter") & sptField
                                        strValues &= drExcel("divisi") & sptField & drExcel("subdivisi") & sptField & drExcel("proyek") & sptField
                                        strValues &= drExcel("urutan") & sptField & drExcel("isclose") & sptField & drExcel("customtext1") & sptField & drExcel("customtext2") & sptField
                                        strValues &= drExcel("customtext3") & sptField & drExcel("customdbl1") & sptField & drExcel("customdbl2") & sptField
                                        strValues &= drExcel("customdbl3") & sptField & drExcel("customdate1") & sptField & drExcel("customdate2") & sptField & drExcel("customdate3")

                                        'JIKA BUKAN ROW TERAKHIR MAKA CEK DATA SELANJUTNYA (IROW + 1)
                                        If iRow <> dtExcelData.Rows.Count - 1 Then
                                            'AMBIL ROW SELANJUTNYA
                                            drNext = dtExcelData.Rows(iRow + 1)

                                            'BANDINGKAN CURRENT ROW DAN NEXT ROW
                                            'JIKA TGL/KONTAK/MATAUANG/KURS TIDAK SAMA MAKA NOMOR TRANSAKSI DIBEDAKAN
                                            If FxDB(drExcel("cbtgl"), "1900-01-01") <> FxDB(drNext("cbtgl"), "1900-01-01") Or _
                                               FxDB(drExcel("cbkontak"), "0") <> FxDB(drNext("cbkontak"), "0") Or _
                                               FxDB(drExcel("matauang"), "") <> FxDB(drNext("matauang"), "") Or _
                                               FxDB(drExcel("kurs"), "0") <> FxDB(drNext("kurs"), "0") Then

                                                'BUAT PARAMETER WS TRANSAKSI UTAMA
                                                'MAPPING :
                                                'cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
                                                'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
                                                'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
                                                'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
                                                'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbcustomtext1, cbcustomtext2, cbcustomtext3, 
                                                'cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, 
                                                'cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3

                                                strField = 0 & sptField & FixQuotes(cabang) & sptField & FixQuotes(lokasi) & sptField & "CB" & sptField
                                                strField &= 1 & sptField & "Auto" & sptField & drExcel("cbtgl") & sptField
                                                strField &= 0 & sptField & drExcel("cbkontak") & sptField & "" & sptField & drExcel("cburaian") & sptField
                                                strField &= drExcel("cbcatatan") & sptField & drExcel("matauang") & sptField & drExcel("kurs") & sptField
                                                strField &= 0 & sptField & 0 & sptField & 0 & sptField & 0 & sptField
                                                strField &= 0 & sptField & 0 & sptField & 0 & sptField
                                                strField &= "1900-01-01" & sptField & 2 & sptField & 0 & sptField & 0 & sptField
                                                strField &= 0 & sptField & 0 & sptField & userid & sptField
                                                strField &= drExcel("cbtgl") & sptField & 0 & sptField & drExcel("cbtgl") & sptField & 0 & sptField
                                                strField &= "" & sptField & "" & sptField & "" & sptField
                                                strField &= "" & sptField & "" & sptField & 0 & sptField & 0 & sptField
                                                strField &= 0 & sptField & 0 & sptField & 0 & sptField
                                                strField &= 0 & sptField & "1900-01-01" & sptField & "1900-01-01" & sptField & "1900-01-01"

                                                'PARAMETER WS TRANSAKSI
                                                strImport = paramSplit(0) & "★M2_CbSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strField & sptSubParam & strValues & sptSubParam
                                                strField = "" : strValues = ""

                                                'POSTING PARAMETER DATA KE WS TRANSAKSI
                                                rsWSTransaksi = WsCB.M2_CbSimpan(strImport).Split(sptParam)
                                                rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                                If rsWSResult(1) = 0 Then
                                                    result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                                End If

                                            End If

                                        Else

                                            'BUAT PARAMETER WS TRANSAKSI UTAMA
                                            'MAPPING :
                                            'cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
                                            'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
                                            'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
                                            'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
                                            'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbcustomtext1, cbcustomtext2, cbcustomtext3, 
                                            'cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, 
                                            'cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3

                                            strField = 0 & sptField & FixQuotes(cabang) & sptField & FixQuotes(lokasi) & sptField & "CB" & sptField
                                            strField &= 1 & sptField & "Auto" & sptField & drExcel("cbtgl") & sptField
                                            strField &= 0 & sptField & drExcel("cbkontak") & sptField & "" & sptField & drExcel("cburaian") & sptField
                                            strField &= drExcel("cbcatatan") & sptField & drExcel("matauang") & sptField & drExcel("kurs") & sptField
                                            strField &= 0 & sptField & 0 & sptField & 0 & sptField & 0 & sptField
                                            strField &= 0 & sptField & 0 & sptField & 0 & sptField
                                            strField &= "1900-01-01" & sptField & 2 & sptField & 0 & sptField & 0 & sptField
                                            strField &= 0 & sptField & 0 & sptField & userid & sptField
                                            strField &= drExcel("cbtgl") & sptField & 0 & sptField & drExcel("cbtgl") & sptField & 0 & sptField
                                            strField &= "" & sptField & "" & sptField & "" & sptField
                                            strField &= "" & sptField & "" & sptField & 0 & sptField & 0 & sptField
                                            strField &= 0 & sptField & 0 & sptField & 0 & sptField
                                            strField &= 0 & sptField & "1900-01-01" & sptField & "1900-01-01" & sptField & "1900-01-01"

                                            'PARAMETER WS TRANSAKSI
                                            strImport = paramSplit(0) & "★M2_CbSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strField & sptSubParam & strValues & sptSubParam
                                            strField = "" : strValues = ""

                                            'POSTING PARAMETER DATA KE WS TRANSAKSI
                                            rsWSTransaksi = WsCB.M2_CbSimpan(strImport).Split(sptParam)
                                            rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                            If rsWSResult(1) = 0 Then
                                                result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                            End If

                                        End If

                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next
                                'END OF PROSES BUAT PARAMETER TRANSAKSI UTAMA ------------------------------



                                'IMPORT SALDO BARANG =======================================================
                            Case "m3_ib_detail"



                                'IMPORT SALDO HUTANG (INVOICE PEMBELIAN) ===================================
                            Case "m4_ri"

                                Dim WsRI As New m4_ri

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> dtTableData.Rows.Count Then
                                    result(2) = filesheet & " - Column count doesn't match with '" & filepaket & "' table." : Trans.Rollback() : GoTo selesai
                                End If

                                'PROSES BUAT PARAMETER TRANSAKSI UTAMA -------------------------------------
                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
                                    For iField = 0 To dtTableData.Rows.Count - 1

                                        'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
                                        namaField = dtTableData.Rows(iField)("Field").ToString
                                        AllowNull = dtTableData.Rows(iField)("Null").ToString
                                        dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

                                        'AMBIL TIPEDATA DAN LENGTH VALUE
                                        sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
                                        If sptDataTipe.Length > 1 Then
                                            sptDataLength = sptDataTipe(1).Split(")")
                                        Else
                                            sptDataLength = "".Split("")
                                        End If
                                        dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

                                        'SET DEFAULT VALUE
                                        If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            If Len(dataDefault) > 0 Then
                                                dtExcelData.Rows(iRow)(iField) = dataDefault

                                            Else
                                                '    NUMERIC
                                                If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                                   dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                                   dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                                   dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
                                                    dtExcelData.Rows(iRow)(iField) = 0

                                                    'YEAR
                                                ElseIf dataTipe.Equals("year") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900"

                                                    'DATE
                                                ElseIf dataTipe.Equals("date") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900-01-01"

                                                    'TIME
                                                ElseIf dataTipe.Equals("time") Then
                                                    dtExcelData.Rows(iRow)(iField) = "00:00:00"

                                                    'DATETIME
                                                ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

                                                End If

                                            End If

                                        End If

                                        'CEK ALLOWNULL
                                        If AllowNull.Equals("NO") And Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            result(2) = filesheet & " - Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                        End If

                                        'VALIDASI TIPEDATA DAN LENGTH VALUE
                                        'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
                                        'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
                                        'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

                                        '    NUMERIC
                                        If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                           dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                           dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                           dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
                                           dataTipe.Equals("year") Then
                                            If IsNumeric(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If

                                            'DATE
                                        ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
                                           dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                            If IsDate(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If
                                            'FORMATTING TANGGAL
                                            If dataTipe.Equals("date") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd")
                                            ElseIf dataTipe.Equals("time") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "H:mm:ss")
                                            ElseIf dataTipe.Equals("timestamp") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            ElseIf dataTipe.Equals("datetime") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            End If

                                            'SELAIN NUMERIC DAN TANGGAL
                                        Else
                                            'CEK LENGTH DATA
                                            If Len(dataLength) > 0 Then
                                                If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) > Double.Parse(dataLength) Then
                                                    result(2) = filesheet & " - Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If

                                        End If

                                    Next


                                    'PROSES POSTING TRANSAKSI JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then

                                        'BUAT DATA MENJADI PARAMETER WS TRANSAKSI
                                        'MAPPING :
                                        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian,
                                        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa,
                                        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2,
                                        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref,
                                        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen,
                                        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar,
                                        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1,
                                        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs,
                                        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatus, ristatussebelumnya,
                                        'rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting,
                                        'ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5,
                                        'ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1,
                                        'ricustomdate2, ricustomdate3

                                        drExcel = dtExcelData.Rows(iRow)
                                        'PARAMETER DATA
                                        strValues = drExcel("riid") & sptField & drExcel("ricabang") & sptField & drExcel("rilokasi") & sptField & drExcel("rigudang") & sptField
                                        strValues &= drExcel("riasalbarang") & sptField & drExcel("riasalbarangkategori") & sptField & drExcel("rijenispembelian") & sptField
                                        strValues &= drExcel("rijenispembeliankategori") & sptField & drExcel("ricarabayar") & sptField & drExcel("risumber") & sptField & drExcel("riautonotransaksi") & sptField
                                        strValues &= drExcel("rinotransaksi") & sptField & drExcel("ritgl") & sptField & drExcel("rikodepa") & sptField
                                        strValues &= drExcel("risupplier") & sptField & drExcel("risupplierkontak") & sptField & drExcel("ri1alamat1") & sptField & drExcel("ri1alamat2") & sptField
                                        strValues &= drExcel("ri1alamat3") & sptField & drExcel("ri2alamat1") & sptField & drExcel("ri2alamat2") & sptField
                                        strValues &= drExcel("ri2alamat3") & sptField & drExcel("ribagianpembelian") & sptField & drExcel("ritermin") & sptField & drExcel("ritgljatuhtempo") & sptField
                                        strValues &= drExcel("riuraian") & sptField & drExcel("ricatatan") & sptField & drExcel("rinoref") & sptField
                                        strValues &= drExcel("ritglnoref") & sptField & drExcel("ritglpenutupan") & sptField & drExcel("rimatauang") & sptField & drExcel("rikurs") & sptField
                                        strValues &= drExcel("rihargatermasukpajak") & sptField & drExcel("ritotal") & sptField & drExcel("ridiskonpersen") & sptField
                                        strValues &= drExcel("rijmldiskon") & sptField & drExcel("ritotalpajak1detail") & sptField & drExcel("ritotalpajak2detail") & sptField & drExcel("ribiayalainpersen") & sptField
                                        strValues &= drExcel("ribiayalain") & sptField & drExcel("ritotaltransaksi") & sptField & drExcel("rijmlbayar") & sptField
                                        strValues &= drExcel("ristatuslunas") & sptField & drExcel("ritgllunas") & sptField & drExcel("rinofakturpajak") & sptField & drExcel("risdhbayarpajak") & sptField
                                        strValues &= drExcel("ritglbayarpajak") & sptField & drExcel("rirekdiskon") & sptField & drExcel("rirekpajak1") & sptField
                                        strValues &= drExcel("rirekpajak2") & sptField & drExcel("rirekbiayalain") & sptField & drExcel("rirekbayar") & sptField & drExcel("riidpr") & sptField
                                        strValues &= drExcel("riidcs") & sptField & drExcel("riidrq") & sptField & drExcel("riidbs") & sptField
                                        strValues &= drExcel("riidpo") & sptField & drExcel("riidipc") & sptField & drExcel("riidgrn") & sptField & drExcel("ristatusdnr") & sptField
                                        strValues &= drExcel("ristatusprt") & sptField & drExcel("ristatus") & sptField & drExcel("ristatussebelumnya") & sptField
                                        strValues &= drExcel("rijmlrevisi") & sptField & drExcel("ricetakanke") & sptField & userid & sptField & drExcel("riinputtgl") & sptField
                                        strValues &= drExcel("rimodifikasiuser") & sptField & drExcel("rimodifikasitgl") & sptField & drExcel("riposting") & sptField
                                        strValues &= drExcel("ritutupperiode") & sptField & drExcel("riisclose") & sptField & drExcel("ricustomtext1") & sptField & drExcel("ricustomtext2") & sptField
                                        strValues &= drExcel("ricustomtext3") & sptField & drExcel("ricustomtext4") & sptField & drExcel("ricustomtext5") & sptField
                                        strValues &= drExcel("ricustomint1") & sptField & drExcel("ricustomint2") & sptField & drExcel("ricustomint3") & sptField & drExcel("ricustomdbl1") & sptField
                                        strValues &= drExcel("ricustomdbl2") & sptField & drExcel("ricustomdbl3") & sptField & drExcel("ricustomdate1") & sptField
                                        strValues &= drExcel("ricustomdate2") & sptField & drExcel("ricustomdate3")

                                        'PARAMETER WS TRANSAKSI
                                        strImport = paramSplit(0) & "★M4_RiBalance★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strValues

                                        'POSTING PARAMETER DATA KE WS TRANSAKSI
                                        rsWSTransaksi = WsRI.M4_RiBalance(strImport).Split(sptParam)
                                        rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                        If rsWSResult(1) = 0 Then
                                            result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                        End If

                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next
                                'END OF PROSES BUAT PARAMETER TRANSAKSI UTAMA ------------------------------



                                'IMPORT SALDO HUTANG (RETUR PEMBELIAN) =====================================
                            Case "m4_prt"

                                Dim WsPRT As New m4_prt

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> dtTableData.Rows.Count Then
                                    result(2) = filesheet & " - Column count doesn't match with '" & filepaket & "' table." : Trans.Rollback() : GoTo selesai
                                End If

                                'PROSES BUAT PARAMETER TRANSAKSI UTAMA -------------------------------------
                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
                                    For iField = 0 To dtTableData.Rows.Count - 1

                                        'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
                                        namaField = dtTableData.Rows(iField)("Field").ToString
                                        AllowNull = dtTableData.Rows(iField)("Null").ToString
                                        dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

                                        'AMBIL TIPEDATA DAN LENGTH VALUE
                                        sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
                                        If sptDataTipe.Length > 1 Then
                                            sptDataLength = sptDataTipe(1).Split(")")
                                        Else
                                            sptDataLength = "".Split("")
                                        End If
                                        dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

                                        'SET DEFAULT VALUE
                                        If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            If Len(dataDefault) > 0 Then
                                                dtExcelData.Rows(iRow)(iField) = dataDefault

                                            Else
                                                '    NUMERIC
                                                If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                                   dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                                   dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                                   dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
                                                    dtExcelData.Rows(iRow)(iField) = 0

                                                    'YEAR
                                                ElseIf dataTipe.Equals("year") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900"

                                                    'DATE
                                                ElseIf dataTipe.Equals("date") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900-01-01"

                                                    'TIME
                                                ElseIf dataTipe.Equals("time") Then
                                                    dtExcelData.Rows(iRow)(iField) = "00:00:00"

                                                    'DATETIME
                                                ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

                                                End If

                                            End If

                                        End If

                                        'CEK ALLOWNULL
                                        If AllowNull.Equals("NO") And Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            result(2) = filesheet & " - Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                        End If

                                        'VALIDASI TIPEDATA DAN LENGTH VALUE
                                        'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
                                        'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
                                        'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

                                        '    NUMERIC
                                        If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                           dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                           dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                           dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
                                           dataTipe.Equals("year") Then
                                            If IsNumeric(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If

                                            'DATE
                                        ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
                                           dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                            If IsDate(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If
                                            'FORMATTING TANGGAL
                                            If dataTipe.Equals("date") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd")
                                            ElseIf dataTipe.Equals("time") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "H:mm:ss")
                                            ElseIf dataTipe.Equals("timestamp") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            ElseIf dataTipe.Equals("datetime") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            End If

                                            'SELAIN NUMERIC DAN TANGGAL
                                        Else
                                            'CEK LENGTH DATA
                                            If Len(dataLength) > 0 Then
                                                If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) > Double.Parse(dataLength) Then
                                                    result(2) = filesheet & " - Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If

                                        End If

                                    Next


                                    'PROSES POSTING TRANSAKSI JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then

                                        'BUAT DATA MENJADI PARAMETER WS TRANSAKSI
                                        'MAPPING :
                                        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
                                        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
                                        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
                                        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
                                        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
                                        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
                                        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
                                        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
                                        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
                                        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
                                        'prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, 
                                        'prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, 
                                        'prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3

                                        drExcel = dtExcelData.Rows(iRow)
                                        'PARAMETER DATA
                                        strValues = drExcel("prtid") & sptField & drExcel("prtcabang") & sptField & drExcel("prtlokasi") & sptField & drExcel("prtgudang") & sptField
                                        strValues &= drExcel("prtasalbarang") & sptField & drExcel("prtasalbarangkategori") & sptField & drExcel("prtjenispembelian") & sptField
                                        strValues &= drExcel("prtjenispembeliankategori") & sptField & drExcel("prtcarabayar") & sptField & drExcel("prtsumber") & sptField & drExcel("prtautonotransaksi") & sptField
                                        strValues &= drExcel("prtnotransaksi") & sptField & drExcel("prttgl") & sptField & drExcel("prtkodepa") & sptField
                                        strValues &= drExcel("prtsupplier") & sptField & drExcel("prtsupplierkontak") & sptField & drExcel("prt1alamat1") & sptField & drExcel("prt1alamat2") & sptField
                                        strValues &= drExcel("prt1alamat3") & sptField & drExcel("prt2alamat1") & sptField & drExcel("prt2alamat2") & sptField
                                        strValues &= drExcel("prt2alamat3") & sptField & drExcel("prtbagianpembelian") & sptField & drExcel("prttermin") & sptField & drExcel("prttgljatuhtempo") & sptField
                                        strValues &= drExcel("prturaian") & sptField & drExcel("prtcatatan") & sptField & drExcel("prtnoref") & sptField
                                        strValues &= drExcel("prttglnoref") & sptField & drExcel("prttglpenutupan") & sptField & drExcel("prtmatauang") & sptField & drExcel("prtkurs") & sptField
                                        strValues &= drExcel("prthargatermasukpajak") & sptField & drExcel("prttotal") & sptField & drExcel("prtdiskonpersen") & sptField
                                        strValues &= drExcel("prtjmldiskon") & sptField & drExcel("prttotalpajak1detail") & sptField & drExcel("prttotalpajak2detail") & sptField & drExcel("prtbiayalainpersen") & sptField
                                        strValues &= drExcel("prtbiayalain") & sptField & drExcel("prttotaltransaksi") & sptField & drExcel("prtsisatransaksi") & sptField
                                        strValues &= drExcel("prtjmlbayar") & sptField & drExcel("prtstatuslunas") & sptField & drExcel("prttgllunas") & sptField & drExcel("prtnofakturpajak") & sptField
                                        strValues &= drExcel("prtsdhbayarpajak") & sptField & drExcel("prttglbayarpajak") & sptField & drExcel("prtrekdiskon") & sptField
                                        strValues &= drExcel("prtrekpajak1") & sptField & drExcel("prtrekpajak2") & sptField & drExcel("prtrekbiayalain") & sptField & drExcel("prtrekbayar") & sptField
                                        strValues &= drExcel("prtreksisa") & sptField & drExcel("prtidpr") & sptField & drExcel("prtidcs") & sptField
                                        strValues &= drExcel("prtidrq") & sptField & drExcel("prtidbs") & sptField & drExcel("prtidpo") & sptField & drExcel("prtidipc") & sptField
                                        strValues &= drExcel("prtidgrn") & sptField & drExcel("prtidri") & sptField & drExcel("prtiddnr") & sptField
                                        strValues &= drExcel("prtstatus") & sptField & drExcel("prtstatussebelumnya") & sptField & drExcel("prtjmlrevisi") & sptField & drExcel("prtcetakanke") & sptField
                                        strValues &= userid & sptField & drExcel("prtinputtgl") & sptField & drExcel("prtmodifikasiuser") & sptField
                                        strValues &= drExcel("prtmodifikasitgl") & sptField & drExcel("prtposting") & sptField & drExcel("prttutupperiode") & sptField & drExcel("prtisclose") & sptField
                                        strValues &= drExcel("prtcustomtext1") & sptField & drExcel("prtcustomtext2") & sptField & drExcel("prtcustomtext3") & sptField
                                        strValues &= drExcel("prtcustomtext4") & sptField & drExcel("prtcustomtext5") & sptField & drExcel("prtcustomint1") & sptField & drExcel("prtcustomint2") & sptField
                                        strValues &= drExcel("prtcustomint3") & sptField & drExcel("prtcustomdbl1") & sptField & drExcel("prtcustomdbl2") & sptField
                                        strValues &= drExcel("prtcustomdbl3") & sptField & drExcel("prtcustomdate1") & sptField & drExcel("prtcustomdate2") & sptField & drExcel("prtcustomdate3")

                                        'PARAMETER WS TRANSAKSI
                                        strImport = paramSplit(0) & "★M4_PrtBalance★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strValues

                                        'POSTING PARAMETER DATA KE WS TRANSAKSI
                                        rsWSTransaksi = WsPRT.M4_PrtBalance(strImport).Split(sptParam)
                                        rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                        If rsWSResult(1) = 0 Then
                                            result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                        End If

                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next
                                'END OF PROSES BUAT PARAMETER TRANSAKSI UTAMA ------------------------------



                                'IMPORT SALDO PIUTANG (INVOICE PENJUALAN) ==================================
                            Case "m5_si"

                                Dim WsSI As New m5_si

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> dtTableData.Rows.Count Then
                                    result(2) = filesheet & " - Column count doesn't match with '" & filepaket & "' table." : Trans.Rollback() : GoTo selesai
                                End If

                                'PROSES BUAT PARAMETER TRANSAKSI UTAMA -------------------------------------
                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
                                    For iField = 0 To dtTableData.Rows.Count - 1

                                        'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
                                        namaField = dtTableData.Rows(iField)("Field").ToString
                                        AllowNull = dtTableData.Rows(iField)("Null").ToString
                                        dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

                                        'AMBIL TIPEDATA DAN LENGTH VALUE
                                        sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
                                        If sptDataTipe.Length > 1 Then
                                            sptDataLength = sptDataTipe(1).Split(")")
                                        Else
                                            sptDataLength = "".Split("")
                                        End If
                                        dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

                                        'SET DEFAULT VALUE
                                        If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            If Len(dataDefault) > 0 Then
                                                dtExcelData.Rows(iRow)(iField) = dataDefault

                                            Else
                                                '    NUMERIC
                                                If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                                   dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                                   dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                                   dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
                                                    dtExcelData.Rows(iRow)(iField) = 0

                                                    'YEAR
                                                ElseIf dataTipe.Equals("year") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900"

                                                    'DATE
                                                ElseIf dataTipe.Equals("date") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900-01-01"

                                                    'TIME
                                                ElseIf dataTipe.Equals("time") Then
                                                    dtExcelData.Rows(iRow)(iField) = "00:00:00"

                                                    'DATETIME
                                                ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

                                                End If

                                            End If

                                        End If

                                        'CEK ALLOWNULL
                                        If AllowNull.Equals("NO") And Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            result(2) = filesheet & " - Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                        End If

                                        'VALIDASI TIPEDATA DAN LENGTH VALUE
                                        'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
                                        'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
                                        'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

                                        '    NUMERIC
                                        If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                           dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                           dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                           dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
                                           dataTipe.Equals("year") Then
                                            If IsNumeric(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If

                                            'DATE
                                        ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
                                           dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                            If IsDate(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If
                                            'FORMATTING TANGGAL
                                            If dataTipe.Equals("date") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd")
                                            ElseIf dataTipe.Equals("time") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "H:mm:ss")
                                            ElseIf dataTipe.Equals("timestamp") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            ElseIf dataTipe.Equals("datetime") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            End If

                                            'SELAIN NUMERIC DAN TANGGAL
                                        Else
                                            'CEK LENGTH DATA
                                            If Len(dataLength) > 0 Then
                                                If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) > Double.Parse(dataLength) Then
                                                    result(2) = filesheet & " - Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If

                                        End If

                                    Next


                                    'PROSES POSTING TRANSAKSI JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then

                                        'BUAT DATA MENJADI PARAMETER WS TRANSAKSI
                                        'MAPPING :
                                        'siid, sicabang, silokasi, sigudang, siasalbarang, siasalbarangkategori, sijenispenjualan, 
                                        'sijenispenjualankategori, sicarabayar, sisumber, siautonotransaksi, sinotransaksi, sitgl, sikodepa, 
                                        'sicustomer, sicustomerkontak, si1alamat1, si1alamat2, si1alamat3, si2alamat1, si2alamat2, 
                                        'si2alamat3, sibagianpenjualan, siekspedisi, sitglkirim, sitermin, sitgljatuhtempo, siuraian, 
                                        'sicatatan, sinoref, sitglnoref, sitglpenutupan, simatauang, sikurs, sihargatermasukpajak, 
                                        'sitotal, sidiskonpersen, sijmldiskon, sitotalpajak1detail, sitotalpajak2detail, sibiayalainpersen, sibiayalain, 
                                        'sitotaltransaksi, sijmlbayar, sistatuslunas, sitgllunas, sinofakturpajak, sisdhbayarpajak, sitglbayarpajak, 
                                        'sirekdiskon, sirekpajak1, sirekpajak2, sirekbiayalain, sirekbayar, siidsq, siidso, 
                                        'siidpl, siiddo, siiddr, siidpi, sistatusrnr, sistatussr, sistatus, 
                                        'sistatussebelumnya, sijmlrevisi, sicetakanke, siinputuser, siinputtgl, simodifikasiuser, simodifikasitgl, 
                                        'siposting, situtupperiode, siisclose, sicustomtext1, sicustomtext2, sicustomtext3, sicustomtext4, 
                                        'sicustomtext5, sicustomint1, sicustomint2, sicustomint3, sicustomdbl1, sicustomdbl2, sicustomdbl3, 
                                        'sicustomdate1, sicustomdate2, sicustomdate3, sijmluangmuka, sirekuangmuka, siidas

                                        drExcel = dtExcelData.Rows(iRow)
                                        'PARAMETER DATA
                                        strValues = drExcel("siid") & sptField & drExcel("sicabang") & sptField & drExcel("silokasi") & sptField & drExcel("sigudang") & sptField
                                        strValues &= drExcel("siasalbarang") & sptField & drExcel("siasalbarangkategori") & sptField & drExcel("sijenispenjualan") & sptField
                                        strValues &= drExcel("sijenispenjualankategori") & sptField & drExcel("sicarabayar") & sptField & drExcel("sisumber") & sptField & drExcel("siautonotransaksi") & sptField
                                        strValues &= drExcel("sinotransaksi") & sptField & drExcel("sitgl") & sptField & drExcel("sikodepa") & sptField
                                        strValues &= drExcel("sicustomer") & sptField & drExcel("sicustomerkontak") & sptField & drExcel("si1alamat1") & sptField & drExcel("si1alamat2") & sptField
                                        strValues &= drExcel("si1alamat3") & sptField & drExcel("si2alamat1") & sptField & drExcel("si2alamat2") & sptField
                                        strValues &= drExcel("si2alamat3") & sptField & drExcel("sibagianpenjualan") & sptField & drExcel("siekspedisi") & sptField & drExcel("sitglkirim") & sptField
                                        strValues &= drExcel("sitermin") & sptField & drExcel("sitgljatuhtempo") & sptField & drExcel("siuraian") & sptField
                                        strValues &= drExcel("sicatatan") & sptField & drExcel("sinoref") & sptField & drExcel("sitglnoref") & sptField & drExcel("sitglpenutupan") & sptField
                                        strValues &= drExcel("simatauang") & sptField & drExcel("sikurs") & sptField & drExcel("sihargatermasukpajak") & sptField
                                        strValues &= drExcel("sitotal") & sptField & drExcel("sidiskonpersen") & sptField & drExcel("sijmldiskon") & sptField & drExcel("sitotalpajak1detail") & sptField
                                        strValues &= drExcel("sitotalpajak2detail") & sptField & drExcel("sibiayalainpersen") & sptField & drExcel("sibiayalain") & sptField
                                        strValues &= drExcel("sitotaltransaksi") & sptField & drExcel("sijmlbayar") & sptField & drExcel("sistatuslunas") & sptField & drExcel("sitgllunas") & sptField
                                        strValues &= drExcel("sinofakturpajak") & sptField & drExcel("sisdhbayarpajak") & sptField & drExcel("sitglbayarpajak") & sptField
                                        strValues &= drExcel("sirekdiskon") & sptField & drExcel("sirekpajak1") & sptField & drExcel("sirekpajak2") & sptField & drExcel("sirekbiayalain") & sptField
                                        strValues &= drExcel("sirekbayar") & sptField & drExcel("siidsq") & sptField & drExcel("siidso") & sptField
                                        strValues &= drExcel("siidpl") & sptField & drExcel("siiddo") & sptField & drExcel("siiddr") & sptField & drExcel("siidpi") & sptField
                                        strValues &= drExcel("sistatusrnr") & sptField & drExcel("sistatussr") & sptField & drExcel("sistatus") & sptField
                                        strValues &= drExcel("sistatussebelumnya") & sptField & drExcel("sijmlrevisi") & sptField & drExcel("sicetakanke") & sptField & userid & sptField
                                        strValues &= drExcel("siinputtgl") & sptField & drExcel("simodifikasiuser") & sptField & drExcel("simodifikasitgl") & sptField
                                        strValues &= drExcel("siposting") & sptField & drExcel("situtupperiode") & sptField & drExcel("siisclose") & sptField & drExcel("sicustomtext1") & sptField
                                        strValues &= drExcel("sicustomtext2") & sptField & drExcel("sicustomtext3") & sptField & drExcel("sicustomtext4") & sptField
                                        strValues &= drExcel("sicustomtext5") & sptField & drExcel("sicustomint1") & sptField & drExcel("sicustomint2") & sptField & drExcel("sicustomint3") & sptField
                                        strValues &= drExcel("sicustomdbl1") & sptField & drExcel("sicustomdbl2") & sptField & drExcel("sicustomdbl3") & sptField
                                        strValues &= drExcel("sicustomdate1") & sptField & drExcel("sicustomdate2") & sptField & drExcel("sicustomdate3") & sptField & drExcel("sijmluangmuka") & sptField
                                        strValues &= drExcel("sirekuangmuka") & sptField & drExcel("siidas")

                                        'PARAMETER WS TRANSAKSI
                                        strImport = paramSplit(0) & "★M5_SiBalance★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strValues

                                        'POSTING PARAMETER DATA KE WS TRANSAKSI
                                        rsWSTransaksi = WsSI.M5_SiBalance(strImport).Split(sptParam)
                                        rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                        If rsWSResult(1) = 0 Then
                                            result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                        End If

                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next
                                'END OF PROSES BUAT PARAMETER TRANSAKSI UTAMA ------------------------------



                                'IMPORT SALDO PIUTANG (RETUR PENJUALAN) ====================================
                            Case "m5_sr"

                                Dim WsSR As New m5_sr

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> dtTableData.Rows.Count Then
                                    result(2) = filesheet & " - Column count doesn't match with '" & filepaket & "' table." : Trans.Rollback() : GoTo selesai
                                End If

                                'PROSES BUAT PARAMETER TRANSAKSI UTAMA -------------------------------------
                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
                                    For iField = 0 To dtTableData.Rows.Count - 1

                                        'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
                                        namaField = dtTableData.Rows(iField)("Field").ToString
                                        AllowNull = dtTableData.Rows(iField)("Null").ToString
                                        dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

                                        'AMBIL TIPEDATA DAN LENGTH VALUE
                                        sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
                                        If sptDataTipe.Length > 1 Then
                                            sptDataLength = sptDataTipe(1).Split(")")
                                        Else
                                            sptDataLength = "".Split("")
                                        End If
                                        dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

                                        'SET DEFAULT VALUE
                                        If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            If Len(dataDefault) > 0 Then
                                                dtExcelData.Rows(iRow)(iField) = dataDefault

                                            Else
                                                '    NUMERIC
                                                If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                                   dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                                   dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                                   dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
                                                    dtExcelData.Rows(iRow)(iField) = 0

                                                    'YEAR
                                                ElseIf dataTipe.Equals("year") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900"

                                                    'DATE
                                                ElseIf dataTipe.Equals("date") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900-01-01"

                                                    'TIME
                                                ElseIf dataTipe.Equals("time") Then
                                                    dtExcelData.Rows(iRow)(iField) = "00:00:00"

                                                    'DATETIME
                                                ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

                                                End If

                                            End If

                                        End If

                                        'CEK ALLOWNULL
                                        If AllowNull.Equals("NO") And Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            result(2) = filesheet & " - Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                        End If

                                        'VALIDASI TIPEDATA DAN LENGTH VALUE
                                        'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
                                        'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
                                        'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

                                        '    NUMERIC
                                        If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                           dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                           dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                           dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
                                           dataTipe.Equals("year") Then
                                            If IsNumeric(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If

                                            'DATE
                                        ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
                                           dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                            If IsDate(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If
                                            'FORMATTING TANGGAL
                                            If dataTipe.Equals("date") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd")
                                            ElseIf dataTipe.Equals("time") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "H:mm:ss")
                                            ElseIf dataTipe.Equals("timestamp") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            ElseIf dataTipe.Equals("datetime") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            End If

                                            'SELAIN NUMERIC DAN TANGGAL
                                        Else
                                            'CEK LENGTH DATA
                                            If Len(dataLength) > 0 Then
                                                If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) > Double.Parse(dataLength) Then
                                                    result(2) = filesheet & " - Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If

                                        End If

                                    Next


                                    'PROSES POSTING TRANSAKSI JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then

                                        'BUAT DATA MENJADI PARAMETER WS TRANSAKSI
                                        'MAPPING :
                                        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
                                        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
                                        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
                                        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
                                        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
                                        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
                                        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
                                        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
                                        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
                                        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
                                        'srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, 
                                        'srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, 
                                        'srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3

                                        drExcel = dtExcelData.Rows(iRow)
                                        'PARAMETER DATA
                                        strValues = drExcel("srid") & sptField & drExcel("srcabang") & sptField & drExcel("srlokasi") & sptField & drExcel("srgudang") & sptField
                                        strValues &= drExcel("srasalbarang") & sptField & drExcel("srasalbarangkategori") & sptField & drExcel("srjenispenjulan") & sptField
                                        strValues &= drExcel("srjenispenjualankategori") & sptField & drExcel("srcarabayar") & sptField & drExcel("srsumber") & sptField & drExcel("srautonotransaksi") & sptField
                                        strValues &= drExcel("srnotransaksi") & sptField & drExcel("srtgl") & sptField & drExcel("srkodepa") & sptField
                                        strValues &= drExcel("srcustomer") & sptField & drExcel("srcustomerkontak") & sptField & drExcel("sr1alamat1") & sptField & drExcel("sr1alamat2") & sptField
                                        strValues &= drExcel("sr1alamat3") & sptField & drExcel("sr2alamat1") & sptField & drExcel("sr2alamat2") & sptField
                                        strValues &= drExcel("sr2alamat3") & sptField & drExcel("srbagianpenjualan") & sptField & drExcel("srekspedisi") & sptField & drExcel("srtglkirim") & sptField
                                        strValues &= drExcel("srtermin") & sptField & drExcel("srtgljatuhtempo") & sptField & drExcel("sruraian") & sptField
                                        strValues &= drExcel("srcatatan") & sptField & drExcel("srnoref") & sptField & drExcel("srtglnoref") & sptField & drExcel("srtglpenutupan") & sptField
                                        strValues &= drExcel("srmatauang") & sptField & drExcel("srkurs") & sptField & drExcel("srhargatermasukpajak") & sptField
                                        strValues &= drExcel("srtotal") & sptField & drExcel("srdiskonpersen") & sptField & drExcel("srjmldiskon") & sptField & drExcel("srtotalpajak1detail") & sptField
                                        strValues &= drExcel("srtotalpajak2detail") & sptField & drExcel("srbiayalainpersen") & sptField & drExcel("srbiayalain") & sptField
                                        strValues &= drExcel("srtotaltransaksi") & sptField & drExcel("srsisatransaksi") & sptField & drExcel("srjmlbayar") & sptField & drExcel("srstatuslunas") & sptField
                                        strValues &= drExcel("srtgllunas") & sptField & drExcel("srnofakturpajak") & sptField & drExcel("srsdhbayarpajak") & sptField
                                        strValues &= drExcel("srtglbayarpajak") & sptField & drExcel("srrekdiskon") & sptField & drExcel("srrekpajak1") & sptField & drExcel("srrekpajak2") & sptField
                                        strValues &= drExcel("srrekbiayalain") & sptField & drExcel("srreksisa") & sptField & drExcel("srrekbayar") & sptField
                                        strValues &= drExcel("sridsq") & sptField & drExcel("sridso") & sptField & drExcel("sridpl") & sptField & drExcel("sriddo") & sptField
                                        strValues &= drExcel("sriddr") & sptField & drExcel("sridpi") & sptField & drExcel("sridsi") & sptField
                                        strValues &= drExcel("sridrnr") & sptField & drExcel("srstatus") & sptField & drExcel("srstatussebelumnya") & sptField & drExcel("srjmlrevisi") & sptField
                                        strValues &= drExcel("srcetakanke") & sptField & userid & sptField & drExcel("srinputtgl") & sptField
                                        strValues &= drExcel("srmodifikasiuser") & sptField & drExcel("srmodifikasitgl") & sptField & drExcel("srposting") & sptField & drExcel("srtutupperiode") & sptField
                                        strValues &= drExcel("srisclose") & sptField & drExcel("srcustomtext1") & sptField & drExcel("srcustomtext2") & sptField
                                        strValues &= drExcel("srcustomtext3") & sptField & drExcel("srcustomtext4") & sptField & drExcel("srcustomtext5") & sptField & drExcel("srcustomint1") & sptField
                                        strValues &= drExcel("srcustomint2") & sptField & drExcel("srcustomint3") & sptField & drExcel("srcustomdbl1") & sptField
                                        strValues &= drExcel("srcustomdbl2") & sptField & drExcel("srcustomdbl3") & sptField & drExcel("srcustomdate1") & sptField & drExcel("srcustomdate2") & sptField
                                        strValues &= drExcel("srcustomdate3")

                                        'PARAMETER WS TRANSAKSI
                                        strImport = paramSplit(0) & "★M5_SrBalance★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userid & "★0★" & strValues

                                        'POSTING PARAMETER DATA KE WS TRANSAKSI
                                        rsWSTransaksi = WsSR.M5_SrBalance(strImport).Split(sptParam)
                                        rsWSResult = rsWSTransaksi(0).Split(sptSubParam) 'paket, isSuccess, message, idTransaksi

                                        If rsWSResult(1) = 0 Then
                                            result(2) = filesheet & " - Row " & iRow + 1 & " : " & rsWSResult(2) : Trans.Rollback() : GoTo selesai
                                        End If

                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next
                                'END OF PROSES BUAT PARAMETER TRANSAKSI UTAMA ------------------------------



                                'IMPORT MASTER DATA ========================================================
                            Case Else

                                'CEK JML KOLOM EXCEL VS DATABASE
                                If dtExcelData.Columns.Count <> dtTableData.Rows.Count Then
                                    result(2) = filesheet & " - Column count doesn't match with '" & filepaket & "' table." : Trans.Rollback() : GoTo selesai
                                End If

                                'PERULANGAN SEBANYAK ROW DATA EXCEL
                                For iRow = rowStart To dtExcelData.Rows.Count - 1
                                    'SET STEPKE
                                    stepKe = stepKe + 1

                                    strValues = String.Concat("(")

                                    'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
                                    For iField = 0 To dtTableData.Rows.Count - 1

                                        'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
                                        namaField = dtTableData.Rows(iField)("Field").ToString
                                        AllowNull = dtTableData.Rows(iField)("Null").ToString
                                        dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

                                        'AMBIL TIPEDATA DAN LENGTH VALUE
                                        sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
                                        If sptDataTipe.Length > 1 Then
                                            sptDataLength = sptDataTipe(1).Split(")")
                                        Else
                                            sptDataLength = "".Split("")
                                        End If
                                        dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

                                        'SET DEFAULT VALUE
                                        If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            If Len(dataDefault) > 0 Then
                                                dtExcelData.Rows(iRow)(iField) = dataDefault

                                            Else
                                                '    NUMERIC
                                                If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                                   dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                                   dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                                   dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
                                                    dtExcelData.Rows(iRow)(iField) = 0

                                                    'YEAR
                                                ElseIf dataTipe.Equals("year") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900"

                                                    'DATE
                                                ElseIf dataTipe.Equals("date") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1900-01-01"

                                                    'TIME
                                                ElseIf dataTipe.Equals("time") Then
                                                    dtExcelData.Rows(iRow)(iField) = "00:00:00"

                                                    'DATETIME
                                                ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                                    dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

                                                End If

                                            End If

                                        End If

                                        'CEK ALLOWNULL
                                        If AllowNull.Equals("NO") And Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) = 0 Then
                                            result(2) = filesheet & " - Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                        End If

                                        'VALIDASI TIPEDATA DAN LENGTH VALUE
                                        'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
                                        'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
                                        'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

                                        '    NUMERIC
                                        If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
                                           dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
                                           dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
                                           dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
                                           dataTipe.Equals("year") Then
                                            If IsNumeric(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If

                                            'DATE
                                        ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
                                           dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
                                            If IsDate(FxDB(dtExcelData.Rows(iRow)(iField), "")) = False Then
                                                result(2) = filesheet & " - Incorrect " & dataTipe & " value : '" & FxDB(dtExcelData.Rows(iRow)(iField), "") & "' for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                            End If
                                            'FORMATTING TANGGAL
                                            If dataTipe.Equals("date") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd")
                                            ElseIf dataTipe.Equals("time") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "H:mm:ss")
                                            ElseIf dataTipe.Equals("timestamp") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            ElseIf dataTipe.Equals("datetime") Then
                                                dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(FxDB(dtExcelData.Rows(iRow)(iField), ""), "yyyy-MM-dd H:mm:ss")
                                            End If

                                            'SELAIN NUMERIC DAN TANGGAL
                                        Else
                                            'CEK LENGTH DATA
                                            If Len(dataLength) > 0 Then
                                                If Len(FxDB(dtExcelData.Rows(iRow)(iField), "")) > Double.Parse(dataLength) Then
                                                    result(2) = filesheet & " - Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : Trans.Rollback() : GoTo selesai
                                                End If
                                            End If

                                        End If

                                        'TAMBAHKAN VALUES QUERY SQL INSERT
                                        strValues = IIf(iField = 0, strValues, strValues & ", ")
                                        strValues = String.Concat(strValues, "'" & FixQuotes(FxDB(dtExcelData.Rows(iRow)(iField), "")) & "'")

                                    Next

                                    strValues = String.Concat(strValues, ")")

                                    'PROSES BUAT QUERY SQL JIKA PROSES IMPORT (SUMBER = IMP)
                                    If sumber.ToLower.Equals("imp") Then
                                        sql = "INSERT INTO " & filepaket & " " & strField & " VALUES " & strValues
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = Con1
                                            .Transaction = Trans
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd.ExecuteNonQuery()
                                    End If

                                    'HITUNG PROSENTASE PROGRESS (100/JML DATA NOREK) * stepKe, JIKA STEP = JML NOREK MAKA PROGRESS = PROSENTASE
                                    progressPersen = IIf(stepKe = dtExcelData.Rows.Count - rowStart, Prosentase, Math.Round(Prosentase / dtExcelData.Rows.Count - rowStart, 2) * stepKe)

                                    'JIKA STEP SHEET = JML SHEET MAKA PROGRES = SELESAI (2), JIKA BELUM MAKA PROGRES = PROSES PROGRES (4)
                                    progress = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, 2, 4)

                                    'JIKA PROSES MAKA PESAN = NAMASHEET, JIKA SELESAI MAKA PESAN = KOSONG
                                    pesan = IIf(stepSheet = dtdetail.Rows.Count And progressPersen = 100, "", filesheet & " - Processing row " & stepKe & " from " & dtExcelData.Rows.Count - rowStart & " rows. ")

                                    'JIKA SELESAI MAKA UPDATE TGLSELESAI
                                    tglselesai = IIf(progress = 2, "NOW()", "'1971-01-01 00:00:00'")

                                    'UPDATE PROGRESS REPORT M0_MSMQ
                                    sql = "UPDATE m0_msmq_importdata SET miprogress = '" & progress & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(pesan) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
                                    If AsEksekusiSQL(sql) = False Then
                                        result(2) = "Failed updating progress '" & filesheet & "'." & sql : Trans.Rollback() : GoTo selesai
                                    End If

                                Next

                        End Select

                    Else
                        result(2) = filesheet & " - No data were found to be imported." : Trans.Rollback() : GoTo selesai

                    End If
                    'END OF PROSES IMPORT KE TABEL ------------------

                Next
            End If

            If sumber.ToLower.Equals("imp") Then
                Trans.Commit()  '*** Commit Transaction ***'
            End If


        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = filesheet & " - Transaction Rollback : " & ex.Message & " " & pesan
            result(3) = 0
            result(4) = result(4)
            GoTo selesai

        End Try
        'END OF PROSES IMPORT DATA ==========================================================


        'SET RESULT
        result(1) = 1
        result(2) = notransaksi
        result(3) = 0
        result(4) = result(4)

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "

            'UPDATE PROGRESS REPORT M0_MSMQ
            sql = "UPDATE m0_msmq_importdata SET miprogress = '" & 3 & "', miprogresspersen = '" & FixDouble(progressPersen) & "', mipesan = '" & FixDouble(FixQuotes(result(2))) & "', mitglselesai = " & (tglselesai) & " WHERE miid = '" & FixQuotes(miid) & "'"
            If AsEksekusiSQL(sql) = False Then
                result(2) = filesheet & " - Failed updating progress."
            End If
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_ImportdataGetdataById(ByVal param As String) As String

        'M0_ImportdataGetdataById --------------------------------------------------------
        'miprogress, mipesan, miprogresspersen

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", idtransaksi As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0
        result(2) = ""
        result(3) = 0
        result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0
        resultPaging(1) = 0
        resultPaging(2) = 0
        resultPaging(3) = 0
        resultPaging(4) = 0


        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
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
        'If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
        '    result(2) = "Access denied for get data"
        'End If
        ''END OF VALIDASI WEBSITEACCESSKEY ==================================================


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


        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(3)) = 0) Then
            result(2) = "ID MSMQ can't be empty." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M0_Msmq_Importdata", "miid='" & idtransaksi & "'", Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , )

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("miprogress"), 0), sptField,
                     FxDB(drutama("mipesan"), ""), sptField,
                     FxDB(drutama("miprogresspersen"), ""))

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            utama = String.Concat(0, sptField,
                               0, sptField,
                               0)
            result(1) = 1
            result(2) = "Import file data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("miprogress, mipesan, miprogresspersen"))

        Return wsResult
    End Function

    '    <WebMethod()>
    '    Public Function ImportData(ByVal param As String) As String
    '        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
    '        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

    '        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
    '        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
    '        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

    '        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
    '        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

    '        Dim wsResult As String = ""
    '        Dim strResult, strResultPaging, strResultData As String
    '        Dim kolomname As String = ""

    '        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
    '        Dim dtdb As DataTable, data As String = "", arr1() As String, arr2() As String
    '        'SET DEFAULT RESULT
    '        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
    '        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

    '        'SET DEFAULT PAGING
    '        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

    '        'VALIDASI PARAMETER GLOBAL =========================================================
    '        'SPLIT PARAM
    '        paramSplit = param.Split(sptParam)

    '        'CEK ARRAY PARAM
    '        If (paramSplit.Length <> 6) Then
    '            result(2) = "Invalid parameter." : GoTo selesai
    '        End If
    '        'END OF VALIDASI PARAMETER GLOBAL ==================================================

    '        'VALIDASI WEBSITEACCESSKEY =========================================================
    '        If Len(paramSplit(0)) = 0 Then
    '            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
    '        End If

    '        'Cek apakah WebsiteAccessKey valid
    '        Dim ClsValidKey As New ClsSecurity
    '        Dim validKey As RsValidKey
    '        validKey = ValidateKey(paramSplit(0))
    '        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

    '        '///Validasi Hak akses. Cek ModuleID dan MenuID
    '        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
    '        '    result(2) = "Access denied for insert/update data"
    '        'End If
    '        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

    '        'VALIDASI DAN SET USERID ===========================================================
    '        'CEK USERID
    '        If (IsNumeric(paramSplit(3)) = False) Then
    '            result(2) = "userid required numeric." : GoTo selesai
    '        End If

    '        'SET USERID
    '        userid = paramSplit(3)
    '        'END OF VALIDASI DAN SET USERID ====================================================

    '        'VALIDASI DAN SET ISUPDATE =========================================================
    '        'CEK ISUPDATE
    '        If (IsNumeric(paramSplit(4)) = False) Then
    '            result(2) = "isupdate required numeric." : GoTo selesai
    '        Else
    '            'SET ISUPDATE
    '            If (Val(paramSplit(4)) = 1) Then
    '                isUpdate = True
    '            Else
    '                isUpdate = False
    '            End If
    '        End If
    '        'END OF VALIDASI DAN SET USERID ====================================================

    '        'VALIDASI DAN SET DATA =============================================================
    '        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

    '        'CEK ARRAY DATA
    '        If (dataSplit.Length <> 2) Then
    '            result(2) = "Invalid transaction data parameter." : GoTo selesai
    '        End If

    '        'CEK ARRAY DATA 
    '        If (dataSplit(1).Length = 0) Then
    '            result(2) = "Data Detail (Sumber) can't be empty ." : GoTo selesai
    '        End If
    '        'END OF VALIDASI DAN SET DATA ======================================================


    '        'VALIDASI DAN SET DATA UTAMA =======================================================
    '        'dataUtama =      'SPLIT PARAMETER DATA UTAMA

    '        'GET STRUKTURE TABEL
    '        dtdb = AsDataTableAmbilDariDB("SHOW COLUMNS FROM m1_" + dataSplit(1))

    '        'CEK ARRAY DATA UTAMA
    '        If (dataSplit(0).Split(sptRow)(0).Split(sptField).Length <> dtdb.Rows.Count) Then
    '            result(2) = "Invalid main transaction data parameter." + dataSplit(0).Split(sptRow)(0).Split(sptField).Length.ToString + " " + dtdb.Rows.Count.ToString : GoTo selesai
    '        End If
    '        'END OF VALIDASI DAN SET DATA UTAMA ================================================

    '        'VALIDASI DATA ==========================================================
    '        arr1 = dataSplit(0).Split(sptRow)
    '        For i = 0 To arr1.Length - 1
    '            arr2 = arr1(i).Split(sptField)
    '            For j = 0 To dtdb.Rows.Count - 1

    '                'Cek Type
    '                If dtdb.Rows(j)("Type").ToString.Replace("varchar", sptField).Split(sptField).Length > 1 Then
    '                    arr2(j) = FixQuotes(arr2(j))
    '                ElseIf dtdb.Rows(j)("Type").ToString.Replace("bigint", sptField).Split(sptField).Length > 1 Then
    '                    If (IsNumeric(arr2(j)) = False) Then
    '                        result(2) = "line to " + (i + 1).ToString + " : " + dtdb.Rows(j)("Field") + " required numeric." : GoTo selesai
    '                    End If
    '                ElseIf dtdb.Rows(j)("Type").ToString.Replace("tinyint", sptField).Split(sptField).Length > 1 Then
    '                    If (IsNumeric(arr2(j)) = False) Then
    '                        result(2) = "line to " + (i + 1).ToString + " : " + dtdb.Rows(j)("Field") + " required numeric. " : GoTo selesai
    '                    End If
    '                ElseIf dtdb.Rows(j)("Type").ToString.Replace("timestamp", sptField).Split(sptField).Length > 1 Then
    '                    If (IsDate(arr2(j)) = False) Then
    '                        result(2) = "line to " + (i + 1).ToString + " : " + dtdb.Rows(j)("Field") + " required date." : GoTo selesai
    '                    End If
    '                End If


    '                'cek jika wajib di isi
    '                If dtdb.Rows(j)("Null") = "NO" Then
    '                    If Len(arr2(j)) = 0 Then
    '                        result(2) = "line to " + (i + 1).ToString + " : " + dtdb.Rows(j)("Field") + " can't be empty" : GoTo selesai
    '                    End If
    '                End If
    '            Next
    '        Next

    '        'END OF VALIDASI TIPE DATA UTAMA ===================================================
    '        Dim isi As String = ""
    '        For i = 0 To arr1.Length - 1
    '            isi += IIf(Len(isi) = 0, "(", ",(")
    '            arr2 = arr1(i).Split(sptField)
    '            For j = 0 To dtdb.Rows.Count - 1
    '                kolomname += IIf(i = 0, IIf(Len(kolomname) > 0, ",", "") + dtdb.Rows(j)("Field"), "")
    '                isi += IIf(j = 0, "", ",") + """" + arr2(j) + """"
    '            Next
    '            isi += ")"
    '        Next
    '        'result(2) = kolomname + " " + isi : GoTo selesai
    '        'SIMPAN KE DATABASE =================================================================
    '        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
    '        Con1.Open()

    '        '*** Start Transaction ***'  
    '        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
    '        sql = "Insert into m1_" + dataSplit(1) + "(" + kolomname + ") values" + isi
    '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '        With objCmd
    '            .Connection = Con1
    '            .Transaction = Trans
    '            .CommandType = CommandType.Text
    '            .CommandText = sql
    '        End With
    '        objCmd.ExecuteNonQuery()

    '        result(2) = " ok" : GoTo selesai
    '        'Buat datatable dtutama
    '        Dim dtutama As New DataTable
    '        AsDataTableTambahField(dtutama, "ajid", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcabang", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajlokasi", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajsumber", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajautonotransaksi", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajnotransaksi", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajtgl", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajkodepa", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajkontak", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajkontakperson", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajuraian", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcatatan", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajmatauang", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajkurs", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajdebit", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtutama, "ajdebitvalas", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtutama, "ajkredit", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtutama, "ajkreditvalas", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtutama, "ajjumlahbayar", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajjumlahbayarvalas", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajstatusbayar", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajtgllunas", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajstatus", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajstatussebelumnya", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajjmlrevisi", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajcetakanke", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajisclose", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajinputuser", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajinputtgl", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajmodifikasiuser", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajmodifikasitgl", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajposting", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajcustomtext1", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomtext2", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomtext3", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomtext4", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomtext5", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomint1", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajcustomint2", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajcustomint3", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtutama, "ajcustomdbl1", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomdbl2", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomdbl3", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomdate1", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomdate2", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtutama, "ajcustomdate3", AsEnumTypeData.AsString)
    '        AsDataTableTambahData(dtutama, "ajid~ajcabang~ajlokasi~ajsumber~ajautonotransaksi~ajnotransaksi~ajtgl~ajkodepa~ajkontak~ajkontakperson~ajuraian~ajcatatan~ajmatauang~ajkurs~ajdebit~ajdebitvalas~ajkredit~ajkreditvalas~ajjumlahbayar~ajjumlahbayarvalas~ajstatusbayar~ajtgllunas~ajstatus~ajstatussebelumnya~ajjmlrevisi~ajcetakanke~ajisclose~ajinputuser~ajinputtgl~ajmodifikasiuser~ajmodifikasitgl~ajposting~ajcustomtext1~ajcustomtext2~ajcustomtext3~ajcustomtext4~ajcustomtext5~ajcustomint1~ajcustomint2~ajcustomint3~ajcustomdbl1~ajcustomdbl2~ajcustomdbl3~ajcustomdate1~ajcustomdate2~ajcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45))

    '        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
    '        'idajdetail(0) As Integer, idaj(1) As Integer, norek(2) As String, matauang(3) As String, kurs(4) As Double, 
    '        'debit(5) As Double, debitvalas(6) As Double, kredit(7) As Double, kreditvalas(8) As Double, catatan(9) As String, 
    '        'costcenter(10) As String, divisi(11) As String, subdivisi(12) As String, proyek(13) As String, urutan(14) As Integer, 
    '        'isclose(15) As Integer, customtext1(16) As String, customtext2(17) As String, customtext3(18) As String, customdbl1(19) As Double, 
    '        'customdbl2(20) As Double, customdbl3(21) As Double, customdate1(22) As Date, customdate2(23) As Date, customdate3(24) As Date

    '        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
    '        'idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, 
    '        'kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, 
    '        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
    '        'customdbl3, customdate1, customdate2, customdate3

    '        'VALIDASI DAN SET DATA DETAIL ======================================================
    '        'SPLIT PARAMETER DATA DETAIL
    '        dataDetail = dataSplit(1).Split(sptRow)
    '        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

    '        'Buat datatable detail
    '        Dim dtdetail As New DataTable
    '        AsDataTableTambahField(dtdetail, "idajdetail", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "idaj", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtdetail, "norek", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "debit", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtdetail, "debitvalas", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtdetail, "kredit", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtdetail, "kreditvalas", AsEnumTypeData.AsDouble)
    '        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
    '        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
    '        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

    '        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
    '        Dim JmlDtDetail As Integer = dataDetail.Length
    '        For i = 1 To JmlDtDetail
    '            'SPLIT DATA DETAIL
    '            dataRowDetail = dataDetail(i - 1).Split(sptField)

    '            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
    '            'CEK ARRAY DATA DETAIL
    '            If (dataRowDetail.Length <> 25) Then
    '                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
    '            End If
    '            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

    '            'VALIDASI TIPE DATA DETAIL ------------------------------------------
    '            'idajdetail(0) As Integer
    '            If (IsNumeric(dataRowDetail(0)) = False) Then
    '                result(2) = "Row : " & i & " - idajdetail required numeric." : GoTo selesai
    '            End If
    '            'idaj(1) As Integer
    '            If (IsNumeric(dataRowDetail(1)) = False) Then
    '                result(2) = "Row : " & i & " - idaj required numeric." : GoTo selesai
    '            End If
    '            'kurs(4) As Double
    '            If (IsNumeric(dataRowDetail(4)) = False) Then
    '                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
    '            End If
    '            'debit(5) As Double
    '            If (IsNumeric(dataRowDetail(5)) = False) Then
    '                result(2) = "Row : " & i & " - debit required numeric." : GoTo selesai
    '            End If
    '            'debitvalas(6) As Double
    '            If (IsNumeric(dataRowDetail(6)) = False) Then
    '                result(2) = "Row : " & i & " - debitvalas required numeric." : GoTo selesai
    '            End If
    '            'kredit(7) As Double
    '            If (IsNumeric(dataRowDetail(7)) = False) Then
    '                result(2) = "Row : " & i & " - kredit required numeric." : GoTo selesai
    '            End If
    '            'kreditvalas(8) As Double
    '            If (IsNumeric(dataRowDetail(8)) = False) Then
    '                result(2) = "Row : " & i & " - kreditvalas required numeric." : GoTo selesai
    '            End If
    '            'urutan(14) As Integer
    '            If (IsNumeric(dataRowDetail(14)) = False) Then
    '                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
    '            End If
    '            'isclose(15) As Integer
    '            If (IsNumeric(dataRowDetail(15)) = False) Then
    '                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
    '            End If
    '            'customdbl1(19) As Double
    '            If (IsNumeric(dataRowDetail(19)) = False) Then
    '                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
    '            End If
    '            'customdbl2(20) As Double
    '            If (IsNumeric(dataRowDetail(20)) = False) Then
    '                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
    '            End If
    '            'customdbl3(21) As Double
    '            If (IsNumeric(dataRowDetail(21)) = False) Then
    '                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
    '            End If
    '            'customdate1(22) As Date
    '            If (IsDate(dataRowDetail(22)) = False) Then
    '                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
    '            End If
    '            'customdate2(23) As Date
    '            If (IsDate(dataRowDetail(23)) = False) Then
    '                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
    '            End If
    '            'customdate3(24) As Date
    '            If (IsDate(dataRowDetail(24)) = False) Then
    '                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
    '            End If
    '            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

    '            'VALIDASI DATA DETAIL ---------------------------------------
    '            'norek(2) As String
    '            If Len(dataRowDetail(2)) = 0 Then
    '                result(2) = "Row : " & i & " - norek can't be empty" : GoTo selesai
    '            End If
    '            If Len(dataRowDetail(2)) > 25 Then
    '                result(2) = "Row : " & i & " - norek should not be more than 25 character." : GoTo selesai
    '            End If

    '            'matauang(3) As String
    '            If Len(dataRowDetail(3)) = 0 Then
    '                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
    '            End If
    '            If Len(dataRowDetail(3)) > 25 Then
    '                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
    '            End If

    '            'kurs(4) As Double
    '            If Len(dataRowDetail(4)) = 0 Then
    '                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
    '            End If

    '            'debit(5) As Double
    '            If Len(dataRowDetail(5)) = 0 Then
    '                result(2) = "Row : " & i & " - debit can't be empty" : GoTo selesai
    '            End If

    '            'debitvalas(6) As Double
    '            If Len(dataRowDetail(6)) = 0 Then
    '                result(2) = "Row : " & i & " - debitvalas can't be empty" : GoTo selesai
    '            End If

    '            'kredit(7) As Double
    '            If Len(dataRowDetail(7)) = 0 Then
    '                result(2) = "Row : " & i & " - kredit can't be empty" : GoTo selesai
    '            End If

    '            'kreditvalas(8) As Double
    '            If Len(dataRowDetail(8)) = 0 Then
    '                result(2) = "Row : " & i & " - kreditvalas can't be empty" : GoTo selesai
    '            End If

    '            'validasi jumlah debit dan kredit tidak boleh diisi keduanya
    '            If dataRowDetail(5) = 0 And dataRowDetail(7) = 0 Then
    '                result(2) = "Row : " & i & " - debits and credits can't be zero" : GoTo selesai
    '            End If
    '            If dataRowDetail(5) <> 0 And dataRowDetail(7) <> 0 Then
    '                result(2) = "Row : " & i & " - debits and credits can't be filled in both" : GoTo selesai
    '            End If
    '            If dataRowDetail(6) <> 0 And dataRowDetail(8) <> 0 Then
    '                result(2) = "Row : " & i & " - foreign debits and credits can't be filled in both" : GoTo selesai
    '            End If

    '            'customdbl1(19) As Double
    '            If Len(dataRowDetail(19)) = 0 Then
    '                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
    '            End If

    '            'customdbl2(20) As Double
    '            If Len(dataRowDetail(20)) = 0 Then
    '                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
    '            End If

    '            'customdbl3(21) As Double
    '            If Len(dataRowDetail(21)) = 0 Then
    '                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
    '            End If

    '            'customdate1(22) As Date
    '            If Len(dataRowDetail(22)) = 0 Then
    '                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
    '            End If

    '            'customdate2(23) As Date
    '            If Len(dataRowDetail(23)) = 0 Then
    '                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
    '            End If

    '            'customdate3(24) As Date
    '            If Len(dataRowDetail(24)) = 0 Then
    '                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
    '            End If

    '            'END OF VALIDASI DATA DETAIL --------------------------------

    '            AsDataTableTambahData(dtdetail, "idajdetail~idaj~norek~matauang~kurs~debit~debitvalas~kredit~kreditvalas~catatan~costcenter~divisi~subdivisi~proyek~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24))

    '        Next

    '        'validasi jumlah debit dan kredit harus seimbang
    '        Dim debit As Double = 0, kredit As Double = 0, debitvalas As Double = 0, kreditvalas As Double = 0
    '        debit = AsDataTableDSum(dtdetail, "debit")
    '        debitvalas = AsDataTableDSum(dtdetail, "debitvalas")
    '        kredit = AsDataTableDSum(dtdetail, "kredit")
    '        kreditvalas = AsDataTableDSum(dtdetail, "kreditvalas")
    '        If debit <> kredit Then
    '            result(2) = "Total debits and credits are not balanced" : GoTo selesai
    '        End If
    '        If debitvalas <> kreditvalas Then
    '            result(2) = "Total foreign debits and credits are not balanced" : GoTo selesai
    '        End If
    '        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


    '        'SIMPAN KE DATABASE =================================================================
    '        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
    '        Con1.Open()

    '        '*** Start Transaction ***'  
    '        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

    '        Dim dtupdate As New DataTable
    '        Dim rowUpdate As Integer = 0

    '        Try
    '            'Proses utama
    '            If (dtutama.Rows.Count > 0) Then
    '                Dim drutama As DataRow = dtutama.Rows(0)

    '                'CEK TOTAL UTAMA DAN DETAIL =============================
    '                If drutama("ajdebit") <> debit Then
    '                    result(2) = "Total debits main and detail are not balanced" : Trans.Rollback() : GoTo selesai
    '                ElseIf drutama("ajdebitvalas") <> debitvalas Then
    '                    result(2) = "Total foreign debits main and detail are not balanced" : Trans.Rollback() : GoTo selesai
    '                ElseIf drutama("ajkredit") <> kredit Then
    '                    result(2) = "Total credits main and detail are not balanced" : Trans.Rollback() : GoTo selesai
    '                ElseIf drutama("ajkreditvalas") <> kreditvalas Then
    '                    result(2) = "Total foreign credits main and detail are not balanced" : Trans.Rollback() : GoTo selesai
    '                End If
    '                'END OF CEK TOTAL UTAMA DAN DETAIL ======================

    '                'CEK PERIODE AKUNTANSI ==================================
    '                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
    '                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ajtgl")), AsFormatTanggal(drutama("ajtgl")))
    '                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
    '                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
    '                'END OF CEK PERIODE AKUNTANSI ===========================

    '                'CEK MATAUANG COA =======================================
    '                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "ajmatauang", "", dtdetail, "norek")
    '                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
    '                'END OF CEK MATAUANG COA ================================

    '                If isUpdate Then
    '                    result(4) = drutama("ajid")
    '                    notransaksi = drutama("ajnotransaksi")
    '                    'JIKA UPDATE CEK JML ROW PADA DATABASE
    '                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ajid), ajnotransaksi FROM M2_aj WHERE ajid='" & result(4) & "' AND ajstatus NOT IN(2,3,4,7)")
    '                    rowUpdate = dtupdate.Rows(0)(0)

    '                    If (rowUpdate > 0) Then

    '                        'CEK NO TRANSAKSI ======================
    '                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
    '                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ajid) FROM m2_aj WHERE ajnotransaksi='" & notransaksi & "'")
    '                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
    '                            If cekNo > 0 Then
    '                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
    '                            End If
    '                        End If
    '                        'END OF CEK NO TRANSAKSI ===============

    '                        sql = "Update M2_Aj set ajcabang  = '" & FixQuotes(drutama("ajcabang")) & "', ajlokasi  = '" & FixQuotes(drutama("ajlokasi")) & "', ajsumber  = '" & FixQuotes(drutama("ajsumber")) & "', ajautonotransaksi  = " & drutama("ajautonotransaksi") & ", ajnotransaksi  = '" & notransaksi & "', ajtgl  = '" & FixQuotes(AsFormatTanggal(drutama("ajtgl"))) & "', ajkodepa  = " & drutama("ajkodepa") & ", ajkontak  = " & drutama("ajkontak") & ", ajkontakperson  = '" & FixQuotes(drutama("ajkontakperson")) & "', ajuraian  = '" & FixQuotes(drutama("ajuraian")) & "', ajcatatan  = '" & FixQuotes(drutama("ajcatatan")) & "', ajmatauang  = '" & FixQuotes(drutama("ajmatauang")) & "', ajkurs  = '" & FixDouble(drutama("ajkurs")) & "', ajdebit  = '" & FixDouble(drutama("ajdebit")) & "', ajdebitvalas  = '" & FixDouble(drutama("ajdebitvalas")) & "', ajkredit  = '" & FixDouble(drutama("ajkredit")) & "', ajkreditvalas  = '" & FixDouble(drutama("ajkreditvalas")) & "', ajjumlahbayar  = '" & FixDouble(drutama("ajjumlahbayar")) & "', ajjumlahbayarvalas  = '" & FixDouble(drutama("ajjumlahbayarvalas")) & "', ajstatusbayar  = " & drutama("ajstatusbayar") & ", ajtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ajtgllunas"))) & "', ajstatus  = " & drutama("ajstatus") & ", ajstatussebelumnya  = " & drutama("ajstatussebelumnya") & ", ajjmlrevisi  = ajjmlrevisi + 1, ajcetakanke  = " & drutama("ajcetakanke") & ", ajisclose  = " & drutama("ajisclose") & ", ajmodifikasiuser  = " & drutama("ajmodifikasiuser") & ", ajmodifikasitgl  = NOW(), ajposting  = 0, ajcustomtext1  = '" & FixQuotes(drutama("ajcustomtext1")) & "', ajcustomtext2  = '" & FixQuotes(drutama("ajcustomtext2")) & "', ajcustomtext3  = '" & FixQuotes(drutama("ajcustomtext3")) & "', ajcustomtext4  = '" & FixQuotes(drutama("ajcustomtext4")) & "', ajcustomtext5  = '" & FixQuotes(drutama("ajcustomtext5")) & "', ajcustomint1  = " & drutama("ajcustomint1") & ", ajcustomint2  = " & drutama("ajcustomint2") & ", ajcustomint3  = " & drutama("ajcustomint3") & ", ajcustomdbl1  = '" & FixDouble(drutama("ajcustomdbl1")) & "', ajcustomdbl2  = '" & FixDouble(drutama("ajcustomdbl2")) & "', ajcustomdbl3  = '" & FixDouble(drutama("ajcustomdbl3")) & "', ajcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate1"))) & "', ajcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate2"))) & "', ajcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate3"))) & "' where ajid = '" & drutama("ajid") & "'"
    '                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                        With objCmd
    '                            .Connection = Con1
    '                            .Transaction = Trans
    '                            .CommandType = CommandType.Text
    '                            .CommandText = sql
    '                        End With
    '                        objCmd.ExecuteNonQuery()
    '                    Else
    '                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
    '                    End If
    '                Else

    '                    If drutama("ajautonotransaksi") = 1 Then

    '                        'GENERATE NOTRANSAKSI =========================================
    '                        Dim wsM0_Nomor As New m0_nomor
    '                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ajcabang"), drutama("ajlokasi"), drutama("ajsumber"), drutama("ajtgl"))
    '                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
    '                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
    '                        'cek success generate notransaksi
    '                        If (arrNotransaksi(0) = 1) Then
    '                            notransaksi = arrNotransaksi(2)
    '                            'tambah query update m0_nomor_next
    '                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                            With objCmd
    '                                .Connection = Con1
    '                                .Transaction = Trans
    '                                .CommandType = CommandType.Text
    '                                .CommandText = arrNotransaksi(3)
    '                            End With
    '                            objCmd.ExecuteNonQuery()
    '                        Else
    '                            result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
    '                        End If
    '                        'END OF GENERATE NOTRANSAKSI ==================================

    '                    Else
    '                        notransaksi = drutama("ajnotransaksi")
    '                    End If

    '                    'CEK NO TRANSAKSI ======================
    '                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(ajid) FROM m2_aj WHERE ajnotransaksi='" & notransaksi & "'")
    '                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
    '                    If cekNo > 0 Then
    '                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
    '                    End If
    '                    'END OF CEK NO TRANSAKSI ===============

    '                    sql = "Insert into M2_Aj (ajcabang, ajlokasi, ajsumber, ajautonotransaksi, ajnotransaksi, ajtgl, ajkodepa, ajkontak, ajkontakperson, ajuraian, ajcatatan, ajmatauang, ajkurs, ajdebit, ajdebitvalas, ajkredit, ajkreditvalas, ajjumlahbayar, ajjumlahbayarvalas, ajstatusbayar, ajtgllunas, ajstatus, ajstatussebelumnya, ajjmlrevisi, ajcetakanke, ajisclose, ajinputuser, ajinputtgl, ajmodifikasiuser, ajmodifikasitgl, ajposting, ajcustomtext1, ajcustomtext2, ajcustomtext3, ajcustomtext4, ajcustomtext5, ajcustomint1, ajcustomint2, ajcustomint3, ajcustomdbl1, ajcustomdbl2, ajcustomdbl3, ajcustomdate1, ajcustomdate2, ajcustomdate3) values('" & FixQuotes(drutama("ajcabang")) & "', '" & FixQuotes(drutama("ajlokasi")) & "', '" & FixQuotes(drutama("ajsumber")) & "', " & drutama("ajautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("ajtgl"))) & "', " & drutama("ajkodepa") & ", " & drutama("ajkontak") & ", '" & FixQuotes(drutama("ajkontakperson")) & "', '" & FixQuotes(drutama("ajuraian")) & "', '" & FixQuotes(drutama("ajcatatan")) & "', '" & FixQuotes(drutama("ajmatauang")) & "', '" & FixDouble(drutama("ajkurs")) & "', '" & FixDouble(drutama("ajdebit")) & "', '" & FixDouble(drutama("ajdebitvalas")) & "', '" & FixDouble(drutama("ajkredit")) & "', '" & FixDouble(drutama("ajkreditvalas")) & "', '" & FixDouble(drutama("ajjumlahbayar")) & "', '" & FixDouble(drutama("ajjumlahbayarvalas")) & "', " & drutama("ajstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("ajtgllunas"))) & "', " & drutama("ajstatus") & ", " & drutama("ajstatussebelumnya") & ", " & drutama("ajjmlrevisi") & ", " & drutama("ajcetakanke") & ", " & drutama("ajisclose") & ", " & drutama("ajinputuser") & ", NOW(), " & drutama("ajmodifikasiuser") & ", '1971-01-01 00:00:00', 0, '" & FixQuotes(drutama("ajcustomtext1")) & "', '" & FixQuotes(drutama("ajcustomtext2")) & "', '" & FixQuotes(drutama("ajcustomtext3")) & "', '" & FixQuotes(drutama("ajcustomtext4")) & "', '" & FixQuotes(drutama("ajcustomtext5")) & "', " & drutama("ajcustomint1") & ", " & drutama("ajcustomint2") & ", " & drutama("ajcustomint3") & ", '" & FixDouble(drutama("ajcustomdbl1")) & "', '" & FixDouble(drutama("ajcustomdbl2")) & "', '" & FixDouble(drutama("ajcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ajcustomdate3"))) & "')"
    '                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                    With objCmd
    '                        .Connection = Con1
    '                        .Transaction = Trans
    '                        .CommandType = CommandType.Text
    '                        .CommandText = sql
    '                    End With
    '                    objCmd.ExecuteNonQuery()

    '                    Dim dt2 As New DataTable
    '                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
    '                    dt2 = AsDataTableAmbilDariDB("select ajid from M2_aj where ajnotransaksi='" & notransaksi & "' AND ajinputuser= '" & userid & "' order by ajmodifikasitgl desc limit 1")
    '                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
    '                End If

    '                'Hapus detail ketika update
    '                If (isUpdate) Then
    '                    sql = "Delete from M2_Aj_Detail where idaj = '" & result(4) & "'"
    '                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                    With objCmd
    '                        .Connection = Con1
    '                        .Transaction = Trans
    '                        .CommandType = CommandType.Text
    '                        .CommandText = sql
    '                    End With
    '                    objCmd.ExecuteNonQuery()
    '                End If

    '                'Proses detail
    '                If (dtdetail.Rows.Count > 0) Then
    '                    Dim strValue2 As New StringBuilder
    '                    For Each dr1 As DataRow In dtdetail.Rows
    '                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
    '                        strValue2.Append("(" & dr1("idajdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("norek")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("debit")) & "', '" & FixDouble(dr1("debitvalas")) & "', '" & FixDouble(dr1("kredit")) & "', '" & FixDouble(dr1("kreditvalas")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
    '                    Next
    '                    sql = "Insert into M2_Aj_Detail(idajdetail, idaj, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
    '                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                    With objCmd
    '                        .Connection = Con1
    '                        .Transaction = Trans
    '                        .CommandType = CommandType.Text
    '                        .CommandText = sql
    '                    End With
    '                    objCmd.ExecuteNonQuery()
    '                End If

    '                'INSERT MSMQ JURNAL =================================================================
    '                Dim sumber As String = "AJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
    '                If drutama("ajstatus") = 2 Then
    '                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
    '                    'BUAT ID UNIQUE
    '                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

    '                    'MSMQ TABEL
    '                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
    '                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
    '                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                    With objCmd
    '                        .Connection = Con1
    '                        .Transaction = Trans
    '                        .CommandType = CommandType.Text
    '                        .CommandText = sql
    '                    End With
    '                    objCmd.ExecuteNonQuery()

    '                    'MSMQ ANTRIAN
    '                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
    '                    If Len(hasilMsmq) > 0 Then
    '                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
    '                    End If

    '                End If
    '                'END OF INSERT MSMQ JURNAL ==========================================================

    '                'INSERT USER LOG ====================================================================
    '                'ambil moduleid dan menuid dari m0_nomor
    '                Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'")
    '                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
    '                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
    '                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

    '                sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
    '                    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
    '                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
    '                With objCmd
    '                    .Connection = Con1
    '                    .Transaction = Trans
    '                    .CommandType = CommandType.Text
    '                    .CommandText = sql
    '                End With
    '                objCmd.ExecuteNonQuery()
    '                'END OF INSERT USER LOG =============================================================

    '                Trans.Commit()  '*** Commit Transaction ***'
    '                result(1) = 1
    '                result(2) = notransaksi
    '                result(3) = 0
    '                result(4) = result(4)

    '            Else
    '                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
    '            End If

    '        Catch ex As Exception
    '            Trans.Rollback() '*** RollBack Transaction ***'  
    '            result(1) = 0
    '            result(2) = ex.Message
    '            result(3) = 0
    '            result(4) = result(4)

    '        End Try

    '        objCmd = Nothing
    '        'Con1.Close()
    '        'Con1 = Nothing
    '        'END OF SIMPAN KE DATABASE ==========================================================

    'selesai:
    '        'If result(1) = 0 Then
    '        '    If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
    '        'End If

    '        strResult = String.Join(sptSubParam, result)
    '        strResultPaging = String.Join(sptSubParam, resultPaging)
    '        strResultData = ""
    '        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
    '        Return wsResult
    '    End Function

End Class
