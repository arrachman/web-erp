Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_room_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M1_Room_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m1_room_history(SELECT 0, room.* FROM m1_room room WHERE room.rkode = '" & idtransaksi & "')"
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
    Public Function M1_Room_HistorySearch(ByVal param As String) As String
        'M1_Bed_HistorySearch --------------------------------------------------------
        'ridhistory, rkode, rnama, rhargajual1, rhargajual2, 
        'rhargajual3, rhargajual4, rhargajual5, rdiskonjual1, rdiskonjual2, 
        'rdiskonjual3, rdiskonjual4, rdiskonjual5, rjmlkasur, rcatatan, 
        'rrekpersediaan, rrekhargapokok, rrekdiskonpenjualan, rrekpenjualan, raktif, 
        'risclose, rinputuser, rinputtgl, rmodifikasiuser, rmodifikasitgl, 
        'rcustomtext1, rcustomtext2, rcustomtext3, rcustomtext4, rcustomtext5, 
        'rcustomint1, rcustomint2, rcustomint3, rcustomint4, rcustomint5, 
        'rcustomdbl1, rcustomdbl2, rcustomdbl3, rcustomdbl4, rcustomdbl5, 
        'rcustomdate1, rcustomdate2, rcustomdate3, rcustomdate4, rcustomdate5, 
        'rrekpersediaannama, rrekhargapokoknama, rrekdiskonpenjualannama, rrekpenjualannama, rinputusernama,
        'rmodifikasiusernama

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
        sql = "select `r`.`ridhistory` AS `ridhistory`,`r`.`rkode` AS `rkode`,`r`.`rnama` AS `rnama`,`r`.`rhargajual1` AS `rhargajual1`,`r`.`rhargajual2` AS `rhargajual2`,`r`.`rhargajual3` AS `rhargajual3`,`r`.`rhargajual4` AS `rhargajual4`,`r`.`rhargajual5` AS `rhargajual5`,`r`.`rdiskonjual1` AS `rdiskonjual1`,`r`.`rdiskonjual2` AS `rdiskonjual2`,`r`.`rdiskonjual3` AS `rdiskonjual3`,`r`.`rdiskonjual4` AS `rdiskonjual4`,`r`.`rdiskonjual5` AS `rdiskonjual5`,`r`.`rjmlkasur` AS `rjmlkasur`,`r`.`rcatatan` AS `rcatatan`,`r`.`rrekpersediaan` AS `rrekpersediaan`,`r`.`rrekhargapokok` AS `rrekhargapokok`,`r`.`rrekdiskonpenjualan` AS `rrekdiskonpenjualan`,`r`.`rrekpenjualan` AS `rrekpenjualan`,`r`.`raktif` AS `raktif`,`r`.`risclose` AS `risclose`,`r`.`rinputuser` AS `rinputuser`,`r`.`rinputtgl` AS `rinputtgl`,`r`.`rmodifikasiuser` AS `rmodifikasiuser`,`r`.`rmodifikasitgl` AS `rmodifikasitgl`,`r`.`rcustomtext1` AS `rcustomtext1`,`r`.`rcustomtext2` AS `rcustomtext2`,`r`.`rcustomtext3` AS `rcustomtext3`,`r`.`rcustomtext4` AS `rcustomtext4`,`r`.`rcustomtext5` AS `rcustomtext5`,`r`.`rcustomint1` AS `rcustomint1`,`r`.`rcustomint2` AS `rcustomint2`,`r`.`rcustomint3` AS `rcustomint3`,`r`.`rcustomint4` AS `rcustomint4`,`r`.`rcustomint5` AS `rcustomint5`,`r`.`rcustomdbl1` AS `rcustomdbl1`,`r`.`rcustomdbl2` AS `rcustomdbl2`,`r`.`rcustomdbl3` AS `rcustomdbl3`,`r`.`rcustomdbl4` AS `rcustomdbl4`,`r`.`rcustomdbl5` AS `rcustomdbl5`,`r`.`rcustomdate1` AS `rcustomdate1`,`r`.`rcustomdate2` AS `rcustomdate2`,`r`.`rcustomdate3` AS `rcustomdate3`,`r`.`rcustomdate4` AS `rcustomdate4`,`r`.`rcustomdate5` AS `rcustomdate5`,`c1`.`cnama` AS `rrekpersediaannama`,`c2`.`cnama` AS `rrekhargapokoknama`,`c3`.`cnama` AS `rrekdiskonpenjualannama`,`c4`.`cnama` AS `rrekpenjualannama`,`ui`.`unama` AS `rinputusernama`,`um`.`unama` AS `rmodifikasiusernama` from ((((((`m1_room_history` `r` left join `m1_coa` `c1` on((`c1`.`cnomor` = `r`.`rrekpersediaan`))) left join `m1_coa` `c2` on((`c2`.`cnomor` = `r`.`rrekhargapokok`)))left join `m1_coa` `c3` on((`c3`.`cnomor` = `r`.`rrekdiskonpenjualan`)))left join `m1_coa` `c4` on((`c4`.`cnomor` = `r`.`rrekpenjualan`)))LEFT JOIN `m0_user` `ui` ON ((`r`.`rinputuser` = `ui`.`userid`))) LEFT JOIN `m0_user` `um` ON ((`r`.`rmodifikasiuser` = `um`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Room_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("ridhistory"), ""), sptField,
                             FxDB(dr("rkode"), ""), sptField,
                             FxDB(dr("rnama"), ""), sptField,
                             FxDB(dr("rhargajual1"), 0), sptField,
                             FxDB(dr("rhargajual2"), 0), sptField,
                             FxDB(dr("rhargajual3"), 0), sptField,
                             FxDB(dr("rhargajual4"), 0), sptField,
                             FxDB(dr("rhargajual5"), 0), sptField,
                             FxDB(dr("rdiskonjual1"), 0), sptField,
                             FxDB(dr("rdiskonjual2"), 0), sptField,
                             FxDB(dr("rdiskonjual3"), 0), sptField,
                             FxDB(dr("rdiskonjual4"), 0), sptField,
                             FxDB(dr("rdiskonjual5"), 0), sptField,
                             FxDB(dr("rjmlkasur"), 0), sptField,
                             FxDB(dr("rcatatan"), ""), sptField,
                             FxDB(dr("rrekpersediaan"), ""), sptField,
                             FxDB(dr("rrekhargapokok"), ""), sptField,
                             FxDB(dr("rrekdiskonpenjualan"), ""), sptField,
                             FxDB(dr("rrekpenjualan"), ""), sptField,
                             FxDB(dr("raktif"), 0), sptField,
                             FxDB(dr("risclose"), 0), sptField,
                             FxDB(dr("rinputuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("rinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("rmodifikasiuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("rmodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("rcustomtext1"), ""), sptField,
                             FxDB(dr("rcustomtext2"), ""), sptField,
                             FxDB(dr("rcustomtext3"), ""), sptField,
                             FxDB(dr("rcustomtext4"), ""), sptField,
                             FxDB(dr("rcustomtext5"), ""), sptField,
                             FxDB(dr("rcustomint1"), 0), sptField,
                             FxDB(dr("rcustomint2"), 0), sptField,
                             FxDB(dr("rcustomint3"), 0), sptField,
                             FxDB(dr("rcustomint4"), 0), sptField,
                             FxDB(dr("rcustomint5"), 0), sptField,
                             FxDB(dr("rcustomdbl1"), 0), sptField,
                             FxDB(dr("rcustomdbl2"), 0), sptField,
                             FxDB(dr("rcustomdbl3"), 0), sptField,
                             FxDB(dr("rcustomdbl4"), 0), sptField,
                             FxDB(dr("rcustomdbl5"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("rcustomdate1"), ""), formatTglWaktu), sptField,
                             AsFormatTanggal(FxDB(dr("rcustomdate2"), ""), formatTglWaktu), sptField,
                             AsFormatTanggal(FxDB(dr("rcustomdate3"), ""), formatTglWaktu), sptField,
                             AsFormatTanggal(FxDB(dr("rcustomdate4"), ""), formatTglWaktu), sptField,
                             AsFormatTanggal(FxDB(dr("rcustomdate5"), ""), formatTglWaktu), sptField,
                             FxDB(dr("rrekpersediaannama"), ""), sptField,
                             FxDB(dr("rrekhargapokoknama"), ""), sptField,
                             FxDB(dr("rrekdiskonpenjualannama"), ""), sptField,
                             FxDB(dr("rrekpenjualannama"), ""), sptField,
                             FxDB(dr("rinputusernama"), ""), sptField,
                             FxDB(dr("rmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Room data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ridhistory, rkode, rnama, rhargajual1, rhargajual2, rhargajual3, rhargajual4, rhargajual5, rdiskonjual1, rdiskonjual2, rdiskonjual3, rdiskonjual4, rdiskonjual5, rjmlkasur, rcatatan, rrekpersediaan, rrekhargapokok, rrekdiskonpenjualan, rrekpenjualan, raktif, risclose, rinputuser, rinputtgl, rmodifikasiuser, rmodifikasitgl, rcustomtext1, rcustomtext2, rcustomtext3, rcustomtext4, rcustomtext5, rcustomint1, rcustomint2, rcustomint3, rcustomint4, rcustomint5, rcustomdbl1, rcustomdbl2, rcustomdbl3, rcustomdbl4, rcustomdbl5, rcustomdate1, rcustomdate2, rcustomdate3, rcustomdate4, rcustomdate5, rrekpersediaannama, rrekhargapokoknama, rrekdiskonpenjualannama, rrekpenjualannama, rinputusernama, rmodifikasiusernama"))

        Return wsResult
    End Function
End Class
