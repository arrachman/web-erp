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
Public Class m1_patient
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_PatientSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 1) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'pid, pkode, nama, prefix, tgllahir, umur, jeniskelamin, statusperkawinan, agama, ayah, ibu, suamiistri, notelepon, nofax, nohp, email, alamat, kota, provinsi, negara, 
        'kodepos, keluargalain, noteleponlain, catatan paktif, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, ptingkatjual, pkategoripasien, pkategoripasiennama 

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pid, pkode, nama, prefix, tgllahir, umur, jeniskelamin, statusperkawinan, agama, ayah, ibu, suamiistri, notelepon, nofax, nohp, email, alamat, kota, provinsi, negara, 
        'kodepos, keluargalain, noteleponlain, catatan paktif, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, ptingkatjual, pkategoripasien, pkategoripasiennama


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 35) Then
            result(2) = "Invalid main transaction data parameter." + dataUtama.Length.ToString : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kjid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pid required numeric." : GoTo selesai
        End If
        'kjtgl(4) As Date
        If (IsDate(dataUtama(4)) = False) Then
            result(2) = "ptgllahir required date." : GoTo selesai
        End If

        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "pumur required numeric." : GoTo selesai
        End If
        'statusperkawinan(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pstatusperkawinan required numeric." : GoTo selesai
        End If
        'agama(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pagama required numeric." : GoTo selesai
        End If

        'kjinputuser(17) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "pinputuser required numeric." : GoTo selesai
        End If
        'kjinputtgl(18) As DateTime
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "pinputtgl required date." : GoTo selesai
        End If
        'kjmodifikasiuser(19) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "pmodifikasiuser required numeric." : GoTo selesai
        End If
        'kjmodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "pmodifikasitgl required date." : GoTo selesai
        End If
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "ptingkatjual required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================

        If Len(dataUtama(2)) = 0 Then
            result(2) = "pnama can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(2)) > 100 Then
            result(2) = "pnama should not be more than 100 character." : GoTo selesai
        End If

        'jml(5) As Double
        If Len(dataUtama(4)) = 0 Then
            result(2) = "ptgllahir can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "ptgllahir should not be more than 10 character." : GoTo selesai
        End If

        'satuan(6) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "pumur can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 10 Then
            result(2) = "pumur should not be more than 10 character." : GoTo selesai
        End If
        If Len(dataUtama(5)) <= 0 Then
            result(2) = "pumur can't be less than or equal to zero" : GoTo selesai
        End If

        'satuanbarang(9) As String
        'If Len(dataUtama(6)) = 0 Then
        '    result(2) = "pjeniskelamin can't be empty" : GoTo selesai
        'End If
        'If Len(dataUtama(6)) > 10 Then
        '    result(2) = "pjeniskelamin should not be more than 10 character." : GoTo selesai
        'End If

        'kjinputtgl(18) As DateTime
        If Len(dataUtama(26)) = 0 Then
            result(2) = "pinputtgl can't be empty" : GoTo selesai
        End If

        'kjmodifikasitgl(20) As DateTime
        If Len(dataUtama(28)) = 0 Then
            result(2) = "pmodifikasitgl can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(29)) = 0 Then
            result(2) = "ptingkatjual can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(29)) > 10 Then
            result(2) = "ptingkatjual should not be more than 10 character." : GoTo selesai
        End If
        If Len(dataUtama(29)) <= 0 Then
            result(2) = "ptingkatjual can't be less than or equal to zero" : GoTo selesai
        End If

        If Len(dataUtama(30)) = 0 Then
            result(2) = "pkategoripasien can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pprefix", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pumur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pjeniskelamin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pstatusperkawinan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pagama", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "payah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pibu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "psuamiistri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pnotelepon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pnofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pnohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "palamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pkota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pprovinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pnegara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pkodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pkeluargalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pnoteleponlain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "paktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ptingkatjual", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pkategoripasiennama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pdesa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pkecamatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pketumur", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "pid~pkode~pnama~pprefix~ptgllahir~pumur~pjeniskelamin~pstatusperkawinan~pagama~payah~pibu~psuamiistri~pnotelepon~pnofax~pnohp~pemail~palamat~pkota~pprovinsi~pnegara~pkodepos~pkeluargalain~pnoteleponlain~pcatatan~paktif~pinputuser~pinputtgl~pmodifikasiuser~pmodifikasitgl~ptingkatjual~pkategoripasien~pkategoripasiennama~pdesa~pkecamatan~pketumur", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

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
                    result(4) = drutama("pid")
                    notransaksi = drutama("pkode")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(pid), pkode FROM m1_patient WHERE pid='" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        ''If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                        ''    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pid) FROM m1_patient WHERE pkode='" & notransaksi & "'")
                        ''    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                        ''    If cekNo > 0 Then
                        ''        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                        ''    End If
                        ''End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m11_kj_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M11_Kj_HistorySimpan("" & paramSplit(0) & "★M11_Kj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("kjsumber")) & "▼" & FixQuotes(drutama("kjid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update m1_patient set pkode  = '" & FixQuotes(drutama("pkode")) & "', pnama = '" & FixQuotes(drutama("pnama")) & "', pprefix = '" & FixQuotes(drutama("pprefix")) & "', ptgllahir = '" & FixQuotes(AsFormatTanggal(drutama("ptgllahir"))) & "', pumur = " & drutama("pumur") & ", pjeniskelamin = '" & FixQuotes(drutama("pjeniskelamin")) & "', pstatusperkawinan = " & drutama("pstatusperkawinan") & ", pagama = " & drutama("pagama") & ", payah = '" & FixQuotes(drutama("payah")) & "', pibu = '" & FixQuotes(drutama("pibu")) & "', psuamiistri = '" & FixQuotes(drutama("psuamiistri")) & "', pnotelepon = '" & FixQuotes(drutama("pnotelepon")) & "', pnofax = '" & FixQuotes(drutama("pnofax")) & "', pnohp = '" & FixQuotes(drutama("pnohp")) & "', pemail = '" & FixQuotes(drutama("pemail")) & "', palamat = '" & FixQuotes(drutama("palamat")) & "', pkota = '" & FixQuotes(drutama("pkota")) & "', pprovinsi = '" & FixQuotes(drutama("pprovinsi")) & "', pnegara = '" & FixQuotes(drutama("pnegara")) & "', pkodepos = '" & FixQuotes(drutama("pkodepos")) & "', pkeluargalain = '" & FixQuotes(drutama("pkeluargalain")) & "', pnoteleponlain = '" & FixQuotes(drutama("pnoteleponlain")) & "', pcatatan = '" & FixQuotes(drutama("pcatatan")) & "', paktif  = " & drutama("paktif") & ", pmodifikasiuser  = " & drutama("pmodifikasiuser") & ", pmodifikasitgl  = NOW(), ptingkatjual = " & drutama("ptingkatjual") & ", pkategoripasien = '" & FixQuotes(drutama("pkategoripasien")) & "', pkategoripasiennama = '" & FixQuotes(drutama("pkategoripasiennama")) & "', pdesa = '" & FixQuotes(drutama("pdesa")) & "', pkecamatan = '" & FixQuotes(drutama("pkecamatan")) & "', pketumur = " & drutama("pketumur") & " where pid = '" & drutama("pid") & "'"
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

                    'If drutama("kjautonotransaksi") = 1 Then

                    '    'GENERATE NOTRANSAKSI =========================================
                    '    Dim wsM0_Nomor As New m0_nomor
                    '    Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("kjcabang"), drutama("kjlokasi"), drutama("kjsumber"), drutama("kjtgl"))
                    '    Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                    '    arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                    '    'cek success generate notransaksi
                    '    If (arrNotransaksi(0) = 1) Then
                    '        notransaksi = arrNotransaksi(2)
                    '        'tambah query update m0_nomor_next
                    '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '        With objCmd
                    '            .Connection = Con1
                    '            .Transaction = Trans
                    '            .CommandType = CommandType.Text
                    '            .CommandText = arrNotransaksi(3)
                    '        End With
                    '        objCmd.ExecuteNonQuery()
                    '    Else
                    '        result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                    '    End If
                    '    'END OF GENERATE NOTRANSAKSI ==================================

                    'Else
                    '    notransaksi = drutama("kjnotransaksi")
                    'End If
                    'result(2) = notransaksi + " " + userid + " Dtdetail : " + dtdetail.Rows.Count.ToString : GoTo selesai
                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pid) FROM m1_patient WHERE pkode='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into m1_patient (pkode, pnama, pprefix, ptgllahir, pumur, pjeniskelamin, pstatusperkawinan, pagama, payah, pibu, psuamiistri, pnotelepon, pnofax, pnohp, pemail, palamat, pkota, pprovinsi, pnegara, pkodepos, pkeluargalain, pnoteleponlain, pcatatan, paktif, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, ptingkatjual, pkategoripasien, pkategoripasiennama, pdesa, pkecamatan, pketumur) values('" & FixQuotes(drutama("pkode")) & "', '" & FixQuotes(drutama("pnama")) & "', '" & FixQuotes(drutama("pprefix")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ptgllahir"))) & "', " & drutama("pumur") & ", '" & FixQuotes(drutama("pjeniskelamin")) & "', " & drutama("pstatusperkawinan") & ", " & drutama("pagama") & ", '" & FixQuotes(drutama("payah")) & "', '" & FixQuotes(drutama("pibu")) & "', '" & FixQuotes(drutama("psuamiistri")) & "', '" & FixQuotes(drutama("pnotelepon")) & "', '" & FixQuotes(drutama("pnofax")) & "', '" & FixQuotes(drutama("pnohp")) & "', '" & FixQuotes(drutama("pemail")) & "', '" & FixQuotes(drutama("palamat")) & "', '" & FixQuotes(drutama("pkota")) & "', '" & FixQuotes(drutama("pprovinsi")) & "', '" & FixQuotes(drutama("pnegara")) & "', '" & FixQuotes(drutama("pkodepos")) & "', '" & FixQuotes(drutama("pkeluargalain")) & "', '" & FixQuotes(drutama("pnoteleponlain")) & "', '" & FixQuotes(drutama("pcatatan")) & "', " & drutama("paktif") & ", " & drutama("pinputuser") & ", NOW(), " & drutama("pmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("ptingkatjual") & ", '" & FixQuotes(drutama("pkategoripasien")) & "', '" & FixQuotes(drutama("pkategoripasiennama")) & "', '" & FixQuotes(drutama("pdesa")) & "', '" & FixQuotes(drutama("pkecamatan")) & "', " & drutama("pketumur") & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'Dim dt2 As New DataTable
                    ''Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    'dt2 = AsDataTableAmbilDariDB("select pid from m1_patient where pkode ='" & notransaksi & "' AND pinputuser= '" & userid & "' order by pmodifikasitgl desc limit 1")
                    'If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT USER LOG ====================================================================
                'Dim sumber As String = "KJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                ''ambil moduleid dan menuid dari m0_nomor
                'Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'")
                'If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                ''jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                'If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

                'sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                '    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = Con1
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
                'END OF INSERT USER LOG =============================================================

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

                'AMBIL DATA =============================================================
                Dim paramSearch As String = M1_PatientSearch(PostWsSearch(paramSplit(0), "M1_PatientSearch", 1, 20, "pkode = '" & FixQuotes(drutama("pkode")) & "'", Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_PatientDelete(ByVal param As String) As String

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
            result(2) = "pkode can't be empty." : GoTo selesai
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
            Dim paramTerkait As String = M1_PatientTerkait(PostWsTerkait(paramSplit(0), "M1_PatientTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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

            'DELETE
            sql = "DELETE FROM M1_Patient WHERE pkode = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_PatientSearch(PostWsSearch(paramSplit(0), "M1_PatientSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_PatientSearch(ByVal param As String) As String
        'M1_PatientSearch --------------------------------------------------------
        'pkode, pnama, palamat, paktif, pinputuser, pinputtgl, pmodifikasiuser, 
        'pmodifikasitgl

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
            Filter = Filter.Replace("pkode", "p.pkode")
            Filter = Filter.Replace("pnama", "p.pnama")
            Filter = Filter.Replace("palamat", "p.palamat")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'sql = "SELECT p.*, pc.pcawalannotran AS pawalankatpasien FROM m1_patient p LEFT JOIN m1_patient_category pc ON p.pkategoripasien = pc.pckode"
        sql = "SELECT p.*, pc.pcawalannotran AS pawalankatpasien, v.vnama AS pdesanama, sd.sdnama AS pkecamatannama, c1.cnama AS pkotanama, pr.pnama AS pprovinsinama, c2.cnama AS pnegaranama FROM m1_patient p LEFT JOIN m1_patient_category pc ON p.pkategoripasien = pc.pckode LEFT JOIN m1_village v ON p.pdesa = v.vkode LEFT JOIN m1_subdistrict sd ON p.pkecamatan = sd.sdkode LEFT JOIN m1_city c1 ON p.pkota = c1.ckode LEFT JOIN m1_province pr ON p.pprovinsi = pr.pkode LEFT JOIN m1_country c2 ON p.pnegara = c2.ckode"
        dt = AmbilData("aplikasi1-M1_Patient", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("pid"), 0), sptField,
                             FxDB(dr("pkode"), ""), sptField,
                             FxDB(dr("pnama"), ""), sptField,
                             FxDB(dr("pprefix"), ""), sptField,
                             AsFormatTanggal(FxDB(dr("ptgllahir"), ""), formatTgl), sptField,
                             FxDB(dr("pumur"), 0), sptField,
                             FxDB(dr("pjeniskelamin"), ""), sptField,
                             FxDB(dr("pstatusperkawinan"), 0), sptField,
                             FxDB(dr("pagama"), 0), sptField,
                             FxDB(dr("payah"), ""), sptField,
                             FxDB(dr("pibu"), ""), sptField,
                             FxDB(dr("psuamiistri"), ""), sptField,
                             FxDB(dr("pnotelepon"), ""), sptField,
                             FxDB(dr("pnofax"), ""), sptField,
                             FxDB(dr("pnohp"), ""), sptField,
                             FxDB(dr("pemail"), ""), sptField,
                             FxDB(dr("palamat"), ""), sptField,
                             FxDB(dr("pkota"), ""), sptField,
                             FxDB(dr("pprovinsi"), ""), sptField,
                             FxDB(dr("pnegara"), ""), sptField,
                             FxDB(dr("pkodepos"), ""), sptField,
                             FxDB(dr("pkeluargalain"), ""), sptField,
                             FxDB(dr("pnoteleponlain"), ""), sptField,
                             FxDB(dr("pcatatan"), ""), sptField,
                             FxDB(dr("paktif"), 0), sptField,
                             FxDB(dr("pinputuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("pinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("pmodifikasiuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("pmodifikasitgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("ptingkatjual"), 0), sptField,
                             FxDB(dr("pkategoripasien"), ""), sptField,
                             FxDB(dr("pkategoripasiennama"), ""), sptField,
                             FxDB(dr("pawalankatpasien"), ""), sptField,
                             FxDB(dr("pdesa"), ""), sptField,
                             FxDB(dr("pkecamatan"), ""), sptField,
                             FxDB(dr("pdesanama"), ""), sptField,
                             FxDB(dr("pkecamatannama"), ""), sptField,
                             FxDB(dr("pkotanama"), ""), sptField,
                             FxDB(dr("pprovinsinama"), ""), sptField,
                             FxDB(dr("pnegaranama"), ""), sptField,
                             FxDB(dr("pketumur"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Patient data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pid, pkode, pnama, pprefix, ptgllahir, pumur, pjeniskelamin, pstatusperkawinan, pagama, payah, pibu, psuamiistri, pnotelepon, pnofax, pnohp, pemail, palamat, pkota, pprovinsi, pnegara, pkodepos, pkeluargalain, pnoteleponlain, pcatatan, paktif, pinputuser, pinputtgl, pmodifikasiuser, pmodifikasitgl, ptingkatjual, pkategoripasien, pkategoripasiennama, pawalankatpasien, pdesa, pkecamatan, pdesanama, pkecamatannama, pkotanama, pprovinsinama, pnegaranama, pketumur"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_PatientCekId(ByVal param As String) As String

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
            result(2) = "pkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(pkode) FROM m1_patient WHERE pkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column pkode." : GoTo selesai
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
    Public Function M1_PatientTerkait(ByVal param As String) As String
        'M1_AreaTerkait --------------------------------------------------------
        'akode, anama, sumber, idterkait

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
            result(2) = "pkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_patient_terkait")
        sql = sql.Replace("valkode", idtransaksi)
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m1_patient_terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("pkode"), ""), sptField,
                             FxDB(dr("pnama"), ""), sptField,
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
            result(2) = "Related Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akode, anama, sumber, idterkait"))

        Return wsResult
    End Function

End Class