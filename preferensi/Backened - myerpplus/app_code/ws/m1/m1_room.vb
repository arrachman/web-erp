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
Public Class m1_room
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_RoomSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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
        'rkode(0) As String, rnama(1) As String, rhargajual1(2) As Double, rhargajual2(3) As Double, rhargajual3(4) As Double, 
        'rhargajual4(5) As Double, rhargajual5(6) As Double, rdiskonjual1(7) As String, rdiskonjual2(8) As String, rdiskonjual3(9) As String, 
        'rdiskonjual4(10) As String, rdiskonjual5(11) As String, rjmlkasur(12) As Double, rcatatan(13) As String, rrekpersediaan(14) As String, 
        'rrekhargapokok(15) As String, rrekdiskonpenjualan(16) As String, rrekpenjualan(17) As String, raktif(18) As Integer, risclose(19) As Integer,
        'rinputuser(20) As Integer, rinputtgl(21) As DateTime, rmodifikasiuser(22) As Integer, rmodifikasitgl(23) As DateTime, rcustomtext1(24) As String, 
        'rcustomtext2(25) As String, rcustomtext3(26) As String, rcustomtext4(27) As String, rcustomtext5(28) As String, rcustomint1(29) As String, 
        'rcustomint2(30) As String, rcustomint3(31) As String, rcustomint4(32) As String, rcustomint5(33) As String, rcustomdbl1(34) As String, 
        'rcustomdbl2(35) As String, rcustomdbl3(36) As String, rcustomdbl4(37) As String, rcustomdbl5(38) As String, rcustomdate1(39) As Date, 
        'rcustomdate2(40) As Date, rcustomdate3(41) As Date, rcustomdate4(42) As Date, rcustomdate5(43) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'rkode, rnama, rhargajual1, rhargajual2, rhargajual3, 
        'rhargajual4, rhargajual5, rdiskonjual1, rdiskonjual2, rdiskonjual3, 
        'rdiskonjual4, rdiskonjual5, rjmlkasur, rcatatan, rrekpersediaan, 
        'rrekhargapokok, rrekdiskonpenjualan, rrekpenjualan, raktif, risclose,
        'rinputuser, rinputtgl, rmodifikasiuser, rmodifikasitgl, rcustomtext1, 
        'rcustomtext2, rcustomtext3, rcustomtext4, rcustomtext5, rcustomint1,
        'rcustomint2, rcustomint3, rcustomint4, rcustomint5, rcustomdbl1,
        'rcustomdbl2, rcustomdbl3, rcustomdbl4, rcustomdbl5, rcustomdate1,
        'rcustomdate2, rcustomdate3, rcustomdate4, rcustomdate5

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 45) Then
            result(2) = "Invalid data parameter." & dataUtama.Length : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'rhargajual1(2) As Double
        If (IsNumeric(dataUtama(2)) = False) Then
            result(2) = "rhargajual1 required numeric." : GoTo selesai
        End If
        'rhargajual2(3) As Double
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "rhargajual2 required numeric." : GoTo selesai
        End If
        'rhargajual3(4) As Double
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rhargajual3 required numeric." : GoTo selesai
        End If
        'rhargajual4(5) As Double
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rhargajual4 required numeric." : GoTo selesai
        End If
        'rhargajual5(6) As Double
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "rhargajual5 required numeric." : GoTo selesai
        End If
        'rjmlkasur(12) As Double
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "rjmlkasur required numeric." : GoTo selesai
        End If
        'raktif(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "raktif required numeric." : GoTo selesai
        End If
        'risclose(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "risclose required numeric." : GoTo selesai
        End If
        'rinputuser(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rinputuser required numeric." : GoTo selesai
        End If
        'rinputtgl(21) As DateTime
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "rinputtgl required date." : GoTo selesai
        End If
        'rmodifikasiuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rmodifikasiuser required numeric." : GoTo selesai
        End If
        'rmodifikasitgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "rmodifikasitgl required date." : GoTo selesai
        End If
        'rcustomint1(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "rcustomint1 required numeric." : GoTo selesai
        End If
        'rcustomint2(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "rcustomint2 required numeric." : GoTo selesai
        End If
        'rcustomint3(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rcustomint3 required numeric." : GoTo selesai
        End If
        'rcustomint4(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rcustomint4 required numeric." : GoTo selesai
        End If
        'rcustomint5(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rcustomint5 required numeric." : GoTo selesai
        End If
        'rcustomdbl1(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rcustomdbl1 required numeric." : GoTo selesai
        End If
        'rcustomdbl2(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rcustomdbl2 required numeric." : GoTo selesai
        End If
        'rcustomdbl3(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "rcustomdbl3 required numeric." : GoTo selesai
        End If
        'rcustomdbl4(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rcustomdbl4 required numeric." : GoTo selesai
        End If
        'rcustomdbl5(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rcustomdbl5 required numeric." : GoTo selesai
        End If
        'rcustomdate1(39) As DateTime
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "rcustomdate1 required date." : GoTo selesai
        End If
        'rcustomdate2(40) As DateTime
        If (IsDate(dataUtama(40)) = False) Then
            result(2) = "rcustomdate2 required date." : GoTo selesai
        End If
        'rcustomdate3(41) As DateTime
        If (IsDate(dataUtama(41)) = False) Then
            result(2) = "rcustomdate3 required date." : GoTo selesai
        End If
        'rcustomdate4(42) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "rcustomdate4 required date." : GoTo selesai
        End If
        'rcustomdate5(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "rcustomdate5 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'rkode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "rkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "rkode should not be more than 25 character." : GoTo selesai
        End If

        'rnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 50 Then
            result(2) = "rnama should not be more than 50 character." : GoTo selesai
        End If

        'rinputtgl(21) As DateTime
        If Len(dataUtama(21)) = 0 Then
            result(2) = "rinputtgl can't be empty" : GoTo selesai
        End If

        'rmodifikasitgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "rmodifikasitgl can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA ========================================================

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rkode) FROM M1_Room WHERE rkode='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    'Dim SimpanHistory As New m1_room_history
                    'Dim roomSimpanHistory As String = SimpanHistory.M1_Room_HistorySimpan("" & paramSplit(0) & "★M1_Room_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    'Dim roomSplit() As String = roomSimpanHistory.Split(sptParam)
                    'Dim roomSplitResult() As String = roomSplit(0).Split(sptSubParam)
                    ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    'If (roomSplitResult(1) = 0) Then
                    '    result(2) = "Insert history failed : " & roomSplitResult(2) : Trans.Rollback() : GoTo selesai
                    'End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Room set rnama  = '" & FixQuotes(dataUtama(1)) & "', rhargajual1  = '" & FixDouble(dataUtama(2)) & "', rhargajual2  = '" & FixDouble(dataUtama(3)) & "', rhargajual3  = '" & FixDouble(dataUtama(4)) & "', rhargajual4  = '" & FixDouble(dataUtama(5)) & "', rhargajual5  = '" & FixDouble(dataUtama(6)) & "', rdiskonjual1  = '" & FixQuotes(dataUtama(7)) & "', rdiskonjual2  = '" & FixQuotes(dataUtama(8)) & "', rdiskonjual3  = '" & FixQuotes(dataUtama(9)) & "', rdiskonjual4  = '" & FixQuotes(dataUtama(10)) & "', rdiskonjual5  = '" & FixQuotes(dataUtama(11)) & "', rjmlkasur  = '" & FixDouble(dataUtama(12)) & "', rcatatan  = '" & FixQuotes(dataUtama(13)) & "', rrekpersediaan  = '" & FixQuotes(dataUtama(14)) & "', rrekhargapokok  = '" & FixQuotes(dataUtama(15)) & "', rrekdiskonpenjualan  = '" & FixQuotes(dataUtama(16)) & "', rrekpenjualan  = '" & FixQuotes(dataUtama(17)) & "', raktif  = " & dataUtama(18) & ", risclose  = " & dataUtama(19) & ", rmodifikasiuser  = " & dataUtama(20) & ", rmodifikasitgl  = NOW() , rcustomtext1  = '" & FixQuotes(dataUtama(24)) & "' , rcustomtext2  = '" & FixQuotes(dataUtama(25)) & "', rcustomtext3  = '" & FixQuotes(dataUtama(26)) & "', rcustomtext4  = '" & FixQuotes(dataUtama(27)) & "', rcustomtext5  = '" & FixQuotes(dataUtama(28)) & "', rcustomint1  = " & dataUtama(29) & ", rcustomint2  = " & dataUtama(30) & ", rcustomint3  = " & dataUtama(31) & ", rcustomint4  = " & dataUtama(32) & ", rcustomint5  = " & dataUtama(33) & ", rcustomdbl1  = " & FixDouble(dataUtama(34)) & ", rcustomdbl2  = " & FixDouble(dataUtama(35)) & ", rcustomdbl3  = " & FixDouble(dataUtama(36)) & ", rcustomdbl4  = " & FixDouble(dataUtama(37)) & ", rcustomdbl5  = " & FixDouble(dataUtama(38)) & ", rcustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(39))) & "', rcustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(40))) & "', rcustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(41))) & "', rcustomdate4  = '" & FixQuotes(AsFormatTanggal(dataUtama(42))) & "', rcustomdate5  = '" & FixQuotes(AsFormatTanggal(dataUtama(43))) & "', rkelas  = " & dataUtama(44) & " where rkode = '" & dataUtama(0) & "'"
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
                sql = "Insert into M1_Room (rkode, rnama, rhargajual1, rhargajual2, rhargajual3, rhargajual4, rhargajual5, rdiskonjual1, rdiskonjual2, rdiskonjual3, rdiskonjual4, rdiskonjual5, rjmlkasur, rcatatan, rrekpersediaan, rrekhargapokok, rrekdiskonpenjualan, rrekpenjualan, raktif, risclose, rinputuser, rinputtgl, rmodifikasiuser, rmodifikasitgl, rcustomtext1, rcustomtext2, rcustomtext3, rcustomtext4, rcustomtext5, rcustomint1, rcustomint2, rcustomint3, rcustomint4, rcustomint5, rcustomdbl1, rcustomdbl2, rcustomdbl3, rcustomdbl4, rcustomdbl5, rcustomdate1, rcustomdate2, rcustomdate3, rcustomdate4, rcustomdate5, rkelas) values('" & FixQuotes(dataUtama(0)) & "', '" & FixQuotes(dataUtama(1)) & "', '" & FixDouble(dataUtama(2)) & "', '" & FixDouble(dataUtama(3)) & "', '" & FixDouble(dataUtama(4)) & "', '" & FixDouble(dataUtama(5)) & "', '" & FixDouble(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(dataUtama(10)) & "', '" & FixQuotes(dataUtama(11)) & "', '" & FixDouble(dataUtama(12)) & "', '" & FixQuotes(dataUtama(13)) & "', '" & FixQuotes(dataUtama(14)) & "', '" & FixQuotes(dataUtama(15)) & "', '" & FixQuotes(dataUtama(16)) & "', '" & FixQuotes(dataUtama(17)) & "', " & dataUtama(18) & ", " & dataUtama(19) & ", " & dataUtama(20) & ", NOW(), " & dataUtama(22) & ", '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(24)) & "', '" & FixQuotes(dataUtama(25)) & "', '" & FixQuotes(dataUtama(26)) & "', '" & FixQuotes(dataUtama(27)) & "', '" & FixQuotes(dataUtama(28)) & "', " & dataUtama(29) & ", " & dataUtama(30) & ", " & dataUtama(31) & ", " & dataUtama(32) & ", " & dataUtama(33) & ", " & FixDouble(dataUtama(34)) & ", " & FixDouble(dataUtama(35)) & ", " & FixDouble(dataUtama(36)) & ", " & FixDouble(dataUtama(37)) & ", " & FixDouble(dataUtama(38)) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(39))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(40))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(41))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(42))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(43))) & "', " & dataUtama(44) & ")"
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
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_RoomSearch(PostWsSearch(paramSplit(0), "M1_RoomSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            'result(1) = hasilSearch.success
            'result(2) = hasilSearch.errmessage

            resultPaging(0) = hasilSearch.isPaging
            resultPaging(1) = hasilSearch.isNext
            resultPaging(2) = hasilSearch.isPrevious
            resultPaging(3) = hasilSearch.countPage
            resultPaging(4) = hasilSearch.countRow

            search = hasilSearch.data
            'END OF AMBIL DATA ======================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'Con1.Close()
        'Con1 = Nothing
        'END OF SIMPAN KE DATABASE ==========================================================

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

    <WebMethod()>
    Public Function M1_RoomDelete(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "rkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'CEK TERKAIT =============================================================
            Dim paramTerkait As String = M1_RoomTerkait(PostWsTerkait(paramSplit(0), "M1_RoomTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
            Dim hasilTerkait As New RsHasilWsSearch
            hasilTerkait = GetWsSearch(paramTerkait)
            If hasilTerkait.success = 1 Then
                result(2) = "It has related transactions."

                resultPaging(0) = hasilTerkait.isPaging
                resultPaging(1) = hasilTerkait.isNext
                resultPaging(2) = hasilTerkait.isPrevious
                resultPaging(3) = hasilTerkait.countPage
                resultPaging(4) = hasilTerkait.countRow

                search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
            End If
            'END OF CEK TERKAIT ======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m1_room_history
            Dim roomSimpanHistory As String = SimpanHistory.M1_Room_HistorySimpan("" & paramSplit(0) & "★M1_Room_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim roomSplit() As String = roomSimpanHistory.Split(sptParam)
            Dim roomSplitResult() As String = roomSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (roomSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & roomSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Room WHERE rkode = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_RoomSearch(PostWsSearch(paramSplit(0), "M1_RoomSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            'result(1) = hasilSearch.success
            'result(2) = hasilSearch.errmessage

            resultPaging(0) = hasilSearch.isPaging
            resultPaging(1) = hasilSearch.isNext
            resultPaging(2) = hasilSearch.isPrevious
            resultPaging(3) = hasilSearch.countPage
            resultPaging(4) = hasilSearch.countRow

            search = hasilSearch.data
            'END OF AMBIL DATA ======================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi

        End Try

        objCmd = Nothing
        'Con1.Close()
        'Con1 = Nothing
        'END OF DELETE DI DATABASE ==========================================================

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

    <WebMethod()>
    Public Function M1_RoomSearch(ByVal param As String) As String
        'M1_BedSearch --------------------------------------------------------
        'rkode, rnama, rhargajual1, rhargajual2, rhargajual3, rhargajual4, 
        'rhargajual5, rdiskonjual1, rdiskonjual2, rdiskonjual3, rdiskonjual4, 
        'rdiskonjual5, rjmlkasur, rcatatan, rrekpersediaan, rrekhargapokok, 
        'rrekdiskonpenjualan, rrekpenjualan, raktif, risclose, rinputuser, 
        'rinputtgl, rmodifikasiuser, rmodifikasitgl, rcustomtext1, rcustomtext2, 
        'rcustomtext3, rcustomtext4, rcustomtext5, rcustomint1, rcustomint2, 
        'rcustomint3, rcustomint4, rcustomint5, rcustomdbl1, rcustomdbl2, 
        'rcustomdbl3, rcustomdbl4, rcustomdbl5, rcustomdate1, rcustomdate2, 
        'rcustomdate3, rcustomdate4, rcustomdate5, rrekpersediaannama, rrekhargapokoknama,
        'rrekdiskonpenjualannama, rrekpenjualannama

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
            'Filter = Filter.Replace("rkode", "r.rkode")
            'Filter = Filter.Replace("rrnama", "r.rnama")
            'Filter = Filter.Replace("rcatatan", "r.rcatatan")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'BUAT QUERY
        sql = "select `r`.`rkode` AS `rkode`,`r`.`rnama` AS `rnama`,`r`.`rhargajual1` AS `rhargajual1`,`r`.`rhargajual2` AS `rhargajual2`,`r`.`rhargajual3` AS `rhargajual3`,`r`.`rhargajual4` AS `rhargajual4`,`r`.`rhargajual5` AS `rhargajual5`,`r`.`rdiskonjual1` AS `rdiskonjual1`,`r`.`rdiskonjual2` AS `rdiskonjual2`,`r`.`rdiskonjual3` AS `rdiskonjual3`,`r`.`rdiskonjual4` AS `rdiskonjual4`,`r`.`rdiskonjual5` AS `rdiskonjual5`,`r`.`rjmlkasur` AS `rjmlkasur`,`r`.`rcatatan` AS `rcatatan`,`r`.`rrekpersediaan` AS `rrekpersediaan`,`r`.`rrekhargapokok` AS `rrekhargapokok`,`r`.`rrekdiskonpenjualan` AS `rrekdiskonpenjualan`,`r`.`rrekpenjualan` AS `rrekpenjualan`,`r`.`raktif` AS `raktif`,`r`.`risclose` AS `risclose`,`r`.`rinputuser` AS `rinputuser`,`r`.`rinputtgl` AS `rinputtgl`,`r`.`rmodifikasiuser` AS `rmodifikasiuser`,`r`.`rmodifikasitgl` AS `rmodifikasitgl`,`r`.`rcustomtext1` AS `rcustomtext1`,`r`.`rcustomtext2` AS `rcustomtext2`,`r`.`rcustomtext3` AS `rcustomtext3`,`r`.`rcustomtext4` AS `rcustomtext4`,`r`.`rcustomtext5` AS `rcustomtext5`,`r`.`rcustomint1` AS `rcustomint1`,`r`.`rcustomint2` AS `rcustomint2`,`r`.`rcustomint3` AS `rcustomint3`,`r`.`rcustomint4` AS `rcustomint4`,`r`.`rcustomint5` AS `rcustomint5`,`r`.`rcustomdbl1` AS `rcustomdbl1`,`r`.`rcustomdbl2` AS `rcustomdbl2`,`r`.`rcustomdbl3` AS `rcustomdbl3`,`r`.`rcustomdbl4` AS `rcustomdbl4`,`r`.`rcustomdbl5` AS `rcustomdbl5`,`r`.`rcustomdate1` AS `rcustomdate1`,`r`.`rcustomdate2` AS `rcustomdate2`,`r`.`rcustomdate3` AS `rcustomdate3`,`r`.`rcustomdate4` AS `rcustomdate4`,`r`.`rcustomdate5` AS `rcustomdate5`,`c1`.`cnama` AS `rrekpersediaannama`,`c2`.`cnama` AS `rrekhargapokoknama`,`c3`.`cnama` AS `rrekdiskonpenjualannama`,`c4`.`cnama` AS `rrekpenjualannama`,r.rkelas AS rkelas from ((((`m1_room` `r` left join `m1_coa` `c1` on((`c1`.`cnomor` = `r`.`rrekpersediaan`))) left join `m1_coa` `c2` on((`c2`.`cnomor` = `r`.`rrekhargapokok`)))left join `m1_coa` `c3` on((`c3`.`cnomor` = `r`.`rrekdiskonpenjualan`)))left join `m1_coa` `c4` on((`c4`.`cnomor` = `r`.`rrekpenjualan`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Room", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
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
                     FxDB(dr("rkelas"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkode, rnama, rhargajual1, rhargajual2, rhargajual3, rhargajual4, rhargajual5, rdiskonjual1, rdiskonjual2, rdiskonjual3, rdiskonjual4, rdiskonjual5, rjmlkasur, rcatatan, rrekpersediaan, rrekhargapokok, rrekdiskonpenjualan, rrekpenjualan, raktif, risclose, rinputuser, rinputtgl, rmodifikasiuser, rmodifikasitgl, rcustomtext1, rcustomtext2, rcustomtext3, rcustomtext4, rcustomtext5, rcustomint1, rcustomint2, rcustomint3, rcustomint4, rcustomint5, rcustomdbl1, rcustomdbl2, rcustomdbl3, rcustomdbl4, rcustomdbl5, rcustomdate1, rcustomdate2, rcustomdate3, rcustomdate4, rcustomdate5, rrekpersediaannama, rrekhargapokoknama, rrekdiskonpenjualannama, rrekpenjualannama, rkelas"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_RoomCekId(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

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
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "rkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(rkode) FROM m1_room WHERE rkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column rkode." : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi
        'END OF CEK DI DATABASE ==========================================================


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
    Public Function M1_RoomTerkait(ByVal param As String) As String
        'M1_RoomTerkait --------------------------------------------------------
        'rkode, rnama, sumber, idterkait

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "rkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_bed_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Bed", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("rkode"), ""), sptField,
                             FxDB(dr("rnama"), ""), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idterkait"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related Room data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rkode, rnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class