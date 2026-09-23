Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_coa_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Coa_HistorySimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim idtransaksi As String = ""

        'SET DEFAULT RESULT
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
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

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
        'idbarang(1) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'idbarang


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 1) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================

        'idbarang(0) As Integer
        idtransaksi = dataUtama(0)
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m1_coa_history(SELECT 0, coa.* FROM m1_coa coa WHERE coa.cnomor = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY UTAMA --------------------------------

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'Con2.Close()
        'Con2 = Nothing
        'END OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Coa_HistorySearch(ByVal param As String) As String
        'M1_Coa_HistorySearch --------------------------------------------------------
        'cidhistory, cid, cnomor, ctipe, cdc, curutan, caktif, cnama, 
        'cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, 
        'clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, 
        'ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, 
        'csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, 
        'csaldoakhir, cparentnama, ccabangnama, clokasinama, cdivisinama, cmatauangnama, cnamabank, 
        'ccostcenter, ccustomtext1, ccustomtext2, ccustomtext3, ccustomtext4, ccustomtext5, ccustomtext6, 
        'ccustomtext7, ccustomtext8, ccustomtext9, ccustomtext10, ccustomint1, ccustomint2, ccustomint3, 
        'ccustomint4, ccustomint5, ccustomint6, ccustomint7, ccustomint8, ccustomint9, ccustomint10, 
        'ccustomdbl1, ccustomdbl2, ccustomdbl3, ccustomdbl4, ccustomdbl5, ccustomdbl6, ccustomdbl7, 
        'ccustomdbl8, ccustomdbl9, ccustomdbl10, ccustomdate1, ccustomdate2, ccustomdate3, ccustomdate4, 
        'ccustomdate5, ccustomdate6, ccustomdate7, ccustomdate8, ccustomdate9, ccustomdate10

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
            Filter = Filter.Replace("cid", "c.cid")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        sql = "SELECT c.cidhistory,  c.cid AS cid, c.cnomor AS cnomor, c.ctipe AS ctipe, c.cdc AS cdc, c.curutan AS curutan, c.caktif AS caktif, c.cnama AS cnama, c.cnamaalias1 AS cnamaalias1, c.cnamaalias2 AS cnamaalias2, c.cnamaalias3 AS cnamaalias3, c.cgd AS cgd, c.clevel AS clevel, c.csubdari AS csubdari, c.cparent AS cparent, c.clevel1 AS clevel1, c.clevel2 AS clevel2, c.clevel3 AS clevel3, c.clevel4 AS clevel4, c.clevel5 AS clevel5, c.cjenisaruskas AS cjenisaruskas, c.cbukupembantu AS cbukupembantu, c.ccabang AS ccabang, c.clokasi AS clokasi, c.cdivisi AS cdivisi, c.cmatauang AS cmatauang, c.ckodebank AS ckodebank, c.cnorekbank AS cnorekbank, c.cjenis AS cjenis, c.csaldoawal AS csaldoawal, c.csaldoberjalan AS csaldoberjalan, c.ccatatan AS ccatatan, c.cinputuser AS cinputuser, c.cinputtgl AS cinputtgl, c.cmodifikasiuser AS cmodifikasiuser, c.cmodifikasitgl AS cmodifikasitgl, (c.csaldoawal + c.csaldoberjalan) AS csaldoakhir, c2.cnama AS cparentnama, br.bnama AS ccabangnama, lc.lnama AS clokasinama, d.dnama AS cdivisinama, cr.cnama AS cmatauangnama, bn.bnama AS cnamabank, c.ccostcenter, c.ccustomtext1, c.ccustomtext2, c.ccustomtext3, c.ccustomtext4, c.ccustomtext5, c.ccustomtext6, c.ccustomtext7, c.ccustomtext8, c.ccustomtext9, c.ccustomtext10, c.ccustomint1, c.ccustomint2, c.ccustomint3, c.ccustomint4, c.ccustomint5, c.ccustomint6, c.ccustomint7, c.ccustomint8, c.ccustomint9, c.ccustomint10, c.ccustomdbl1, c.ccustomdbl2, c.ccustomdbl3, c.ccustomdbl4, c.ccustomdbl5, c.ccustomdbl6, c.ccustomdbl7, c.ccustomdbl8, c.ccustomdbl9, c.ccustomdbl10, c.ccustomdate1, c.ccustomdate2, c.ccustomdate3, c.ccustomdate4, c.ccustomdate5, c.ccustomdate6, c.ccustomdate7, c.ccustomdate8, c.ccustomdate9, c.ccustomdate10 from m1_coa_history c  left join m1_coa_history c2 on c.cparent = c2.cnomor left join m1_branch br on c.ccabang = br.bkode left join m1_location lc on c.clokasi = lc.lkode left join m1_division d on c.cdivisi = d.dkode left join m1_bank bn on c.ckodebank = bn.bkode left join m1_currency cr on c.cmatauang = cr.ckode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Coa_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cidhistory"), 0), sptField,
                     FxDB(dr("cid"), ""), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("ctipe"), 0), sptField,
                     FxDB(dr("cdc"), ""), sptField,
                     FxDB(dr("curutan"), 0), sptField,
                     FxDB(dr("caktif"), 0), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cnamaalias1"), ""), sptField,
                     FxDB(dr("cnamaalias2"), ""), sptField,
                     FxDB(dr("cnamaalias3"), ""), sptField,
                     FxDB(dr("cgd"), ""), sptField,
                     FxDB(dr("clevel"), 0), sptField,
                     FxDB(dr("csubdari"), 0), sptField,
                     FxDB(dr("cparent"), ""), sptField,
                     FxDB(dr("clevel1"), ""), sptField,
                     FxDB(dr("clevel2"), ""), sptField,
                     FxDB(dr("clevel3"), ""), sptField,
                     FxDB(dr("clevel4"), ""), sptField,
                     FxDB(dr("clevel5"), ""), sptField,
                     FxDB(dr("cjenisaruskas"), ""), sptField,
                     FxDB(dr("cbukupembantu"), 0), sptField,
                     FxDB(dr("ccabang"), ""), sptField,
                     FxDB(dr("clokasi"), ""), sptField,
                     FxDB(dr("cdivisi"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("ckodebank"), ""), sptField,
                     FxDB(dr("cnorekbank"), ""), sptField,
                     FxDB(dr("cjenis"), ""), sptField,
                     FxDB(dr("csaldoawal"), 0), sptField,
                     FxDB(dr("csaldoberjalan"), 0), sptField,
                     FxDB(dr("ccatatan"), ""), sptField,
                     FxDB(dr("cinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("csaldoakhir"), 0), sptField,
                     FxDB(dr("cparentnama"), ""), sptField,
                     FxDB(dr("ccabangnama"), ""), sptField,
                     FxDB(dr("clokasinama"), ""), sptField,
                     FxDB(dr("cdivisinama"), ""), sptField,
                     FxDB(dr("cmatauangnama"), ""), sptField,
                     FxDB(dr("cnamabank"), ""), sptField,
                     FxDB(dr("ccostcenter"), 0), sptField,
                     FxDB(dr("ccustomtext1"), ""), sptField,
                     FxDB(dr("ccustomtext2"), ""), sptField,
                     FxDB(dr("ccustomtext3"), ""), sptField,
                     FxDB(dr("ccustomtext4"), ""), sptField,
                     FxDB(dr("ccustomtext5"), ""), sptField,
                     FxDB(dr("ccustomtext6"), ""), sptField,
                     FxDB(dr("ccustomtext7"), ""), sptField,
                     FxDB(dr("ccustomtext8"), ""), sptField,
                     FxDB(dr("ccustomtext9"), ""), sptField,
                     FxDB(dr("ccustomtext10"), ""), sptField,
                     FxDB(dr("ccustomint1"), 0), sptField,
                     FxDB(dr("ccustomint2"), 0), sptField,
                     FxDB(dr("ccustomint3"), 0), sptField,
                     FxDB(dr("ccustomint4"), 0), sptField,
                     FxDB(dr("ccustomint5"), 0), sptField,
                     FxDB(dr("ccustomint6"), 0), sptField,
                     FxDB(dr("ccustomint7"), 0), sptField,
                     FxDB(dr("ccustomint8"), 0), sptField,
                     FxDB(dr("ccustomint9"), 0), sptField,
                     FxDB(dr("ccustomint10"), 0), sptField,
                     FxDB(dr("ccustomdbl1"), 0), sptField,
                     FxDB(dr("ccustomdbl2"), 0), sptField,
                     FxDB(dr("ccustomdbl3"), 0), sptField,
                     FxDB(dr("ccustomdbl4"), 0), sptField,
                     FxDB(dr("ccustomdbl5"), 0), sptField,
                     FxDB(dr("ccustomdbl6"), 0), sptField,
                     FxDB(dr("ccustomdbl7"), 0), sptField,
                     FxDB(dr("ccustomdbl8"), 0), sptField,
                     FxDB(dr("ccustomdbl9"), 0), sptField,
                     FxDB(dr("ccustomdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ccustomdate10"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Coa data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cidhistory, cid, cnomor, ctipe, cdc, curutan, caktif, cnama, cnamaalias1, cnamaalias2, cnamaalias3, cgd, clevel, csubdari, cparent, clevel1, clevel2, clevel3, clevel4, clevel5, cjenisaruskas, cbukupembantu, ccabang, clokasi, cdivisi, cmatauang, ckodebank, cnorekbank, cjenis, csaldoawal, csaldoberjalan, ccatatan, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, csaldoakhir, cparentnama, ccabangnama, clokasinama, cdivisinama, cmatauangnama, cnamabank, ccostcenter, ccustomtext1, ccustomtext2, ccustomtext3, ccustomtext4, ccustomtext5, ccustomtext6, ccustomtext7, ccustomtext8, ccustomtext9, ccustomtext10, ccustomint1, ccustomint2, ccustomint3, ccustomint4, ccustomint5, ccustomint6, ccustomint7, ccustomint8, ccustomint9, ccustomint10, ccustomdbl1, ccustomdbl2, ccustomdbl3, ccustomdbl4, ccustomdbl5, ccustomdbl6, ccustomdbl7, ccustomdbl8, ccustomdbl9, ccustomdbl10, ccustomdate1, ccustomdate2, ccustomdate3, ccustomdate4, ccustomdate5, ccustomdate6, ccustomdate7, ccustomdate8, ccustomdate9, ccustomdate10"))

        Return wsResult
    End Function

End Class
