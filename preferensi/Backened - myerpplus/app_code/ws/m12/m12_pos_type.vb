Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m12_pos_type
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M12_Pos_TypeSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim search As String = "", Filter As String = "", Sorting As String = ""

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
        'ptkode(0) As String, ptnama(1) As String, ptcatatan(2) As String, ptaktif(3) As Integer, ptinputuser(4) As Integer, 
        'ptinputtgl(5) As DateTime, ptmodifikasiuser(6) As Integer, ptmodifikasitgl(7) As DateTime, ptcustomtext1(8) As String, ptcustomtext2(9) As String, 
        'ptcustomtext3(10) As String, ptcustomtext4(11) As String, ptcustomtext5(12) As String, ptcustomint1(13) As Integer, ptcustomint2(14) As Integer, 
        'ptcustomint3(15) As Integer, ptcustomdbl1(16) As Double, ptcustomdbl2(17) As Double, ptcustomdbl3(18) As Double, ptcustomdate1(19) As Date, 
        'ptcustomdate2(20) As Date, ptcustomdate3(21) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, 
        'ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, 
        'ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, 
        'ptcustomdate3


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 22) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'ptaktif(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "ptaktif required numeric." : GoTo selesai
        End If
        'ptinputuser(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "ptinputuser required numeric." : GoTo selesai
        End If
        'ptinputtgl(5) As DateTime
        If (IsDate(dataUtama(5)) = False) Then
            result(2) = "ptinputtgl required date." : GoTo selesai
        End If
        'ptmodifikasiuser(6) As Integer
        If (IsNumeric(dataUtama(6)) = False) Then
            result(2) = "ptmodifikasiuser required numeric." : GoTo selesai
        End If
        'ptmodifikasitgl(7) As DateTime
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "ptmodifikasitgl required date." : GoTo selesai
        End If
        'ptcustomint1(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "ptcustomint1 required numeric." : GoTo selesai
        End If
        'ptcustomint2(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "ptcustomint2 required numeric." : GoTo selesai
        End If
        'ptcustomint3(15) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "ptcustomint3 required numeric." : GoTo selesai
        End If
        'ptcustomdbl1(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "ptcustomdbl1 required numeric." : GoTo selesai
        End If
        'ptcustomdbl2(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "ptcustomdbl2 required numeric." : GoTo selesai
        End If
        'ptcustomdbl3(18) As Double
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "ptcustomdbl3 required numeric." : GoTo selesai
        End If
        'ptcustomdate1(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "ptcustomdate1 required date." : GoTo selesai
        End If
        'ptcustomdate2(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "ptcustomdate2 required date." : GoTo selesai
        End If
        'ptcustomdate3(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "ptcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'ptkode(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "ptkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 25 Then
            result(2) = "ptkode should not be more than 25 character." : GoTo selesai
        End If

        'ptnama(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ptnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "ptnama should not be more than 100 character." : GoTo selesai
        End If

        'ptinputtgl(5) As DateTime
        If Len(dataUtama(5)) = 0 Then
            result(2) = "ptinputtgl can't be empty" : GoTo selesai
        End If

        'ptmodifikasitgl(7) As DateTime
        If Len(dataUtama(7)) = 0 Then
            result(2) = "ptmodifikasitgl can't be empty" : GoTo selesai
        End If

        'ptcustomdbl1(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "ptcustomdbl1 can't be empty" : GoTo selesai
        End If

        'ptcustomdbl2(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "ptcustomdbl2 can't be empty" : GoTo selesai
        End If

        'ptcustomdbl3(18) As Double
        If Len(dataUtama(18)) = 0 Then
            result(2) = "ptcustomdbl3 can't be empty" : GoTo selesai
        End If

        'ptcustomdate1(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "ptcustomdate1 can't be empty" : GoTo selesai
        End If

        'ptcustomdate2(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "ptcustomdate2 can't be empty" : GoTo selesai
        End If

        'ptcustomdate3(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "ptcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "ptkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ptcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "ptkode~ptnama~ptcatatan~ptaktif~ptinputuser~ptinputtgl~ptmodifikasiuser~ptmodifikasitgl~ptcustomtext1~ptcustomtext2~ptcustomtext3~ptcustomtext4~ptcustomtext5~ptcustomint1~ptcustomint2~ptcustomint3~ptcustomdbl1~ptcustomdbl2~ptcustomdbl3~ptcustomdate1~ptcustomdate2~ptcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'tipepos(0) As String, kelasproduk(1) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'tipepos, kelasproduk

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "tipepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kelasproduk", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        If Len(dataSplit(1)) > 0 Then
            Dim JmlDtDetail As Integer = dataDetail.Length
            For i = 1 To JmlDtDetail
                'SPLIT DATA DETAIL
                dataRowDetail = dataDetail(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowDetail.Length <> 2) Then
                    result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'tipepos(0) As String
                If Len(dataRowDetail(0)) = 0 Then
                    result(2) = "Row : " & i & " - tipepos can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(0)) > 25 Then
                    result(2) = "Row : " & i & " - tipepos should not be more than 25 character." : GoTo selesai
                End If

                'kelasproduk(1) As String
                If Len(dataRowDetail(1)) = 0 Then
                    result(2) = "Row : " & i & " - kelasproduk can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(1)) > 25 Then
                    result(2) = "Row : " & i & " - kelasproduk should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA DETAIL --------------------------------

                If AsDataTableTambahData(dtdetail, "tipepos~kelasproduk", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
        End If
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)
                If isUpdate Then
                    result(4) = drutama("ptkode")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(ptkode) FROM M_12_Pos_Type WHERE ptkode= '" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m12_pos_type_history
                        Dim contactSimpanHistory As String = SimpanHistory.M12_Pos_TypeHistorySimpan("" & paramSplit(0) & "★M12_Pos_TypeHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                        Dim contactSplit() As String = contactSimpanHistory.Split(sptParam)
                        Dim contactSplitResult() As String = contactSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (contactSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & contactSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_12_Pos_Type set ptnama  = '" & FixQuotes(drutama("ptnama")) & "', ptcatatan  = '" & FixQuotes(drutama("ptcatatan")) & "', ptaktif  = " & drutama("ptaktif") & ", ptmodifikasiuser  = " & drutama("ptmodifikasiuser") & ", ptmodifikasitgl  = '" & FixQuotes(AsFormatTanggal(drutama("ptmodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', ptcustomtext1  = '" & FixQuotes(drutama("ptcustomtext1")) & "', ptcustomtext2  = '" & FixQuotes(drutama("ptcustomtext2")) & "', ptcustomtext3  = '" & FixQuotes(drutama("ptcustomtext3")) & "', ptcustomtext4  = '" & FixQuotes(drutama("ptcustomtext4")) & "', ptcustomtext5  = '" & FixQuotes(drutama("ptcustomtext5")) & "', ptcustomint1  = " & drutama("ptcustomint1") & ", ptcustomint2  = " & drutama("ptcustomint2") & ", ptcustomint3  = " & drutama("ptcustomint3") & ", ptcustomdbl1  = '" & FixDouble(drutama("ptcustomdbl1")) & "', ptcustomdbl2  = '" & FixDouble(drutama("ptcustomdbl2")) & "', ptcustomdbl3  = '" & FixDouble(drutama("ptcustomdbl3")) & "', ptcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ptcustomdate1"))) & "', ptcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ptcustomdate2"))) & "', ptcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ptcustomdate3"))) & "' where ptkode = '" & drutama("ptkode") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If

                Else
                    sql = "Insert into M_12_Pos_Type (ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3) values('" & FixQuotes(drutama("ptkode")) & "', '" & FixQuotes(drutama("ptnama")) & "', '" & FixQuotes(drutama("ptcatatan")) & "', " & drutama("ptaktif") & ", " & drutama("ptinputuser") & ", '" & FixQuotes(AsFormatTanggal(drutama("ptinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', 0, '1971-01-01 00:00:00', '" & FixQuotes(drutama("ptcustomtext1")) & "', '" & FixQuotes(drutama("ptcustomtext2")) & "', '" & FixQuotes(drutama("ptcustomtext3")) & "', '" & FixQuotes(drutama("ptcustomtext4")) & "', '" & FixQuotes(drutama("ptcustomtext5")) & "', " & drutama("ptcustomint1") & ", " & drutama("ptcustomint2") & ", " & drutama("ptcustomint3") & ", '" & FixDouble(drutama("ptcustomdbl1")) & "', '" & FixDouble(drutama("ptcustomdbl2")) & "', '" & FixDouble(drutama("ptcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ptcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ptcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ptcustomdate3"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDB("select ptkode from M_12_Pos_Type where ptkode = '" & FixQuotes(drutama("ptkode")) & "' order by ptmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_12_Pos_Type_Class_Product where tipepos = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & result(4) & "', '" & FixQuotes(dr1("kelasproduk")) & "')")
                    Next
                    sql = "Insert into M_12_Pos_Type_Class_Product(tipepos, kelasproduk) values" & strValue2.ToString & ""
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
                Dim paramSearch As String = M12_Pos_TypeSearch(PostWsSearch(paramSplit(0), "M12_Pos_TypeSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
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
    Public Function M12_Pos_TypeDelete(ByVal param As String) As String

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
        Dim search As String = "", Filter As String = "", Sorting As String = "", formatTgl As String = "", formatTglWaktu As String = ""

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "ptkode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M12_Pos_TypeTerkait(PostWsTerkait(paramSplit(0), "M12_Pos_TypeTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m12_pos_type_history
            Dim contactSimpanHistory As String = SimpanHistory.M12_Pos_TypeHistorySimpan("" & paramSplit(0) & "★M12_Pos_TypeHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim contactSplit() As String = contactSimpanHistory.Split(sptParam)
            Dim contactSplitResult() As String = contactSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (contactSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & contactSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE DETAIL
            sql = "DELETE FROM M_12_Pos_Type_Class_Product WHERE tipepos = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_12_Pos_Type WHERE ptkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M12_Pos_TypeSearch(PostWsSearch(paramSplit(0), "M12_Pos_TypeSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M12_Pos_TypeSearch(ByVal param As String) As String
        'M12_Pos_TypeSearch --------------------------------------------------------
        'ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, 
        'ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, 
        'ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, 
        'ptcustomdate3, ptinputusernama, ptmodifikasiusernama

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
        sql = "select `pt`.`ptkode` AS `ptkode`,`pt`.`ptnama` AS `ptnama`,`pt`.`ptcatatan` AS `ptcatatan`,`pt`.`ptaktif` AS `ptaktif`,`pt`.`ptinputuser` AS `ptinputuser`,`pt`.`ptinputtgl` AS `ptinputtgl`,`pt`.`ptmodifikasiuser` AS `ptmodifikasiuser`,`pt`.`ptmodifikasitgl` AS `ptmodifikasitgl`,`pt`.`ptcustomtext1` AS `ptcustomtext1`,`pt`.`ptcustomtext2` AS `ptcustomtext2`,`pt`.`ptcustomtext3` AS `ptcustomtext3`,`pt`.`ptcustomtext4` AS `ptcustomtext4`,`pt`.`ptcustomtext5` AS `ptcustomtext5`,`pt`.`ptcustomint1` AS `ptcustomint1`,`pt`.`ptcustomint2` AS `ptcustomint2`,`pt`.`ptcustomint3` AS `ptcustomint3`,`pt`.`ptcustomdbl1` AS `ptcustomdbl1`,`pt`.`ptcustomdbl2` AS `ptcustomdbl2`,`pt`.`ptcustomdbl3` AS `ptcustomdbl3`,`pt`.`ptcustomdate1` AS `ptcustomdate1`,`pt`.`ptcustomdate2` AS `ptcustomdate2`,`pt`.`ptcustomdate3` AS `ptcustomdate3`,`u1`.`unama` AS `ptinputusernama`,`u2`.`unama` AS `ptmodifikasiusernama` from ((`m_12_pos_type` `pt` left join `m0_user` `u1` on((`pt`.`ptinputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`pt`.`ptmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Type", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("ptkode"), ""), sptField,
                     FxDB(dr("ptnama"), ""), sptField,
                     FxDB(dr("ptcatatan"), ""), sptField,
                     FxDB(dr("ptaktif"), 0), sptField,
                     FxDB(dr("ptinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ptinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ptmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ptmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ptcustomtext1"), ""), sptField,
                     FxDB(dr("ptcustomtext2"), ""), sptField,
                     FxDB(dr("ptcustomtext3"), ""), sptField,
                     FxDB(dr("ptcustomtext4"), ""), sptField,
                     FxDB(dr("ptcustomtext5"), ""), sptField,
                     FxDB(dr("ptcustomint1"), 0), sptField,
                     FxDB(dr("ptcustomint2"), 0), sptField,
                     FxDB(dr("ptcustomint3"), 0), sptField,
                     FxDB(dr("ptcustomdbl1"), 0), sptField,
                     FxDB(dr("ptcustomdbl2"), 0), sptField,
                     FxDB(dr("ptcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ptcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ptinputusernama"), ""), sptField,
                     FxDB(dr("ptmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "POS Type data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, ptinputusernama, ptmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_TypeGetdataById(ByVal param As String) As String

        'M12_Pos_TypeGetdataById Utama --------------------------------------------------------
        'ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, 
        'ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, 
        'ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, 
        'ptcustomdate3, ptinputusernama, ptmodifikasiusernama

        'M12_Pos_TypeGetdataById Detail -------------------------------------------------------
        'tipepos, kelasproduk, kelasproduknama

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (Len(paramSplit(3)) = 0) Then
            result(2) = "idtransaksi can't be empty." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M2_Aj~M2_Aj_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "ptkode = '" & idtransaksi & "'"
        Else ' jika filter diisi
            Filter = "ptkode = '" & idtransaksi & "' and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pt`.`ptkode` AS `ptkode`,`pt`.`ptnama` AS `ptnama`,`pt`.`ptcatatan` AS `ptcatatan`,`pt`.`ptaktif` AS `ptaktif`,`pt`.`ptinputuser` AS `ptinputuser`,`pt`.`ptinputtgl` AS `ptinputtgl`,`pt`.`ptmodifikasiuser` AS `ptmodifikasiuser`,`pt`.`ptmodifikasitgl` AS `ptmodifikasitgl`,`pt`.`ptcustomtext1` AS `ptcustomtext1`,`pt`.`ptcustomtext2` AS `ptcustomtext2`,`pt`.`ptcustomtext3` AS `ptcustomtext3`,`pt`.`ptcustomtext4` AS `ptcustomtext4`,`pt`.`ptcustomtext5` AS `ptcustomtext5`,`pt`.`ptcustomint1` AS `ptcustomint1`,`pt`.`ptcustomint2` AS `ptcustomint2`,`pt`.`ptcustomint3` AS `ptcustomint3`,`pt`.`ptcustomdbl1` AS `ptcustomdbl1`,`pt`.`ptcustomdbl2` AS `ptcustomdbl2`,`pt`.`ptcustomdbl3` AS `ptcustomdbl3`,`pt`.`ptcustomdate1` AS `ptcustomdate1`,`pt`.`ptcustomdate2` AS `ptcustomdate2`,`pt`.`ptcustomdate3` AS `ptcustomdate3`,`u1`.`unama` AS `ptinputusernama`,`u2`.`unama` AS `ptmodifikasiusernama` from `m_12_pos_type` `pt` left join `m0_user` `u1` on `pt`.`ptinputuser` = `u1`.`userid` left join `m0_user` `u2` on `pt`.`ptmodifikasiuser` = `u2`.`userid`"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)

            Dim tglinput As String = "", tglmodifikasi As String = "", tglcustom1 As String = "", tglcustom2 As String = "", tglcustom3 As String = ""
            'SET FORMAT TGL
            If Len(FxDB(drutama("ptinputtgl"), "")) > 0 Then tglinput = AsFormatTanggal(FxDB(drutama("ptinputtgl"), ""), formatTglWaktu)
            If Len(FxDB(drutama("ptmodifikasitgl"), "")) > 0 Then tglmodifikasi = AsFormatTanggal(FxDB(drutama("ptmodifikasitgl"), ""), formatTglWaktu)
            If Len(FxDB(drutama("ptcustomdate1"), "")) > 0 Then tglcustom1 = AsFormatTanggal(FxDB(drutama("ptcustomdate1"), ""), formatTgl)
            If Len(FxDB(drutama("ptcustomdate2"), "")) > 0 Then tglcustom2 = AsFormatTanggal(FxDB(drutama("ptcustomdate2"), ""), formatTgl)
            If Len(FxDB(drutama("ptcustomdate3"), "")) > 0 Then tglcustom3 = AsFormatTanggal(FxDB(drutama("ptcustomdate3"), ""), formatTgl)

            utama = String.Concat(
                     FxDB(drutama("ptkode"), ""), sptField,
                     FxDB(drutama("ptnama"), ""), sptField,
                     FxDB(drutama("ptcatatan"), ""), sptField,
                     FxDB(drutama("ptaktif"), 0), sptField,
                     FxDB(drutama("ptinputuser"), ""), sptField,
                     tglinput, sptField,
                     FxDB(drutama("ptmodifikasiuser"), ""), sptField,
                     tglmodifikasi, sptField,
                     FxDB(drutama("ptcustomtext1"), ""), sptField,
                     FxDB(drutama("ptcustomtext2"), ""), sptField,
                     FxDB(drutama("ptcustomtext3"), ""), sptField,
                     FxDB(drutama("ptcustomtext4"), ""), sptField,
                     FxDB(drutama("ptcustomtext5"), ""), sptField,
                     FxDB(drutama("ptcustomint1"), 0), sptField,
                     FxDB(drutama("ptcustomint2"), 0), sptField,
                     FxDB(drutama("ptcustomint3"), 0), sptField,
                     FxDB(drutama("ptcustomdbl1"), 0), sptField,
                     FxDB(drutama("ptcustomdbl2"), 0), sptField,
                     FxDB(drutama("ptcustomdbl3"), 0), sptField,
                     tglcustom1, sptField,
                     tglcustom2, sptField,
                     tglcustom3, sptField,
                     FxDB(drutama("ptinputusernama"), ""), sptField,
                     FxDB(drutama("ptmodifikasiusernama"), ""))


            sql = "SELECT ptcp.tipepos as tipepos, cp.cpkode as kelasproduk, cp.cpnama as kelasproduknama FROM m1_class_product cp LEFT JOIN m_12_pos_type_class_product ptcp ON cp.cpkode = ptcp.kelasproduk AND ptcp.tipepos = 'valkode'"
            sql = sql.Replace("valkode", idtransaksi)
            dt = AmbilData(NmMemcached, "", "ptcp.tipepos DESC, cp.cpkode", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail,
                     FxDB(dr("tipepos"), ""), sptField,
                     FxDB(dr("kelasproduk"), ""), sptField,
                     FxDB(dr("kelasproduknama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptkode, ptnama, ptcatatan, ptaktif, ptinputuser, ptinputtgl, ptmodifikasiuser, ptmodifikasitgl, ptcustomtext1, ptcustomtext2, ptcustomtext3, ptcustomtext4, ptcustomtext5, ptcustomint1, ptcustomint2, ptcustomint3, ptcustomdbl1, ptcustomdbl2, ptcustomdbl3, ptcustomdate1, ptcustomdate2, ptcustomdate3, ptinputusernama, ptmodifikasiusernama" & sptSubParam & "tipepos, kelasproduk, kelasproduknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_TypeTerkait(ByVal param As String) As String
        'M12_Pos_TypeTerkait --------------------------------------------------------
        'ptkode, ptnama, sumber, idterkait

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
            result(2) = "ptkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        sql = "SELECT pt.ptkode as ptkode, pt.ptnama as ptnama, 'POS Category' as sumber, pc.pckode as idterkait FROM m_12_pos_category pc JOIN m_12_pos_type pt ON pc.pctipepos = pt.ptkode AND pt.ptkode = 'valkode' GROUP BY pt.ptkode, pc.pckode"
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M_12_Pos_Type", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("ptkode"), ""), sptField,
                             FxDB(dr("ptnama"), ""), sptField,
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
            result(2) = "Related POS Type data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ptkode, ptnama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M12_Pos_TypeCekId(ByVal param As String) As String

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
            result(2) = "ptkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(ptkode) FROM M_12_Pos_Type WHERE ptkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column akode." : GoTo selesai
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

End Class
