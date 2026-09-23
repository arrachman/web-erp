Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_po
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_PoSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataCost(), dataRowCost(), dataTrans(), dataRowTrans() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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
        If (dataSplit.Length <> 3 And dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'poid(0) As Integer, pocabang(1) As String, polokasi(2) As String, pogudang(3) As String, poasalbarang(4) As String, 
        'poasalbarangkategori(5) As Integer, pojenispembelian(6) As String, pojenispembeliankategori(7) As Integer, pocarabayar(8) As Integer, posumber(9) As String, 
        'poautonotransaksi(10) As Integer, ponotransaksi(11) As String, potgl(12) As Date, pokodepa(13) As Integer, posupplier(14) As Integer, 
        'posupplierkontak(15) As String, po1alamat1(16) As String, po1alamat2(17) As String, po1alamat3(18) As String, po2alamat1(19) As String, 
        'po2alamat2(20) As String, po2alamat3(21) As String, pobagianpembelian(22) As Integer, potgldipenuhi(23) As Date, potermin(24) As String, 
        'potgljatuhtempo(25) As Date, pouraian(26) As String, pocatatan(27) As String, ponoref(28) As String, potglnoref(29) As Date, 
        'potglpenutupan(30) As Date, pomatauang(31) As String, pokurs(32) As Double, pohargatermasukpajak(33) As Integer, pototal(34) As Double, 
        'podiskonpersen(35) As String, pojmldiskon(36) As Double, pototalpajak1detail(37) As Double, pototalpajak2detail(38) As Double, pobiayalainpersen(39) As String, 
        'pobiayalain(40) As Double, pototaltransaksi(41) As Double, pojmlbayar(42) As Double, porekdiskon(43) As String, porekpajak1(44) As String, 
        'porekpajak2(45) As String, porekbiayalain(46) As String, porekbayar(47) As String, poidpr(48) As Integer, poidcs(49) As Integer, 
        'poidrq(50) As Integer, poidbs(51) As Integer, postatusipc(52) As Integer, postatusgrn(53) As Integer, postatusri(54) As Integer, 
        'postatusdnr(55) As Integer, postatusprt(56) As Integer, postatus(57) As Integer, postatussebelumnya(58) As Integer, pojmlrevisi(59) As Integer, 
        'pocetakanke(60) As Integer, poinputuser(61) As Integer, poinputtgl(62) As DateTime, pomodifikasiuser(63) As Integer, pomodifikasitgl(64) As DateTime, 
        'poisclose(65) As Integer, pocustomtext1(66) As String, pocustomtext2(67) As String, pocustomtext3(68) As String, pocustomtext4(69) As String, 
        'pocustomtext5(70) As String, pocustomint1(71) As Integer, pocustomint2(72) As Integer, pocustomint3(73) As Integer, pocustomdbl1(74) As Double, 
        'pocustomdbl2(75) As Double, pocustomdbl3(76) As Double, pocustomdate1(77) As Date, pocustomdate2(78) As Date, pocustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, 
        'pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, 
        'posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, 
        'po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, 
        'ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, 
        'podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, 
        'pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, 
        'poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, 
        'postatusprt, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, 
        'pomodifikasiuser, pomodifikasitgl, poisclose, pocustomtext1, pocustomtext2, pocustomtext3, pocustomtext4, 
        'pocustomtext5, pocustomint1, pocustomint2, pocustomint3, pocustomdbl1, pocustomdbl2, pocustomdbl3, 
        'pocustomdate1, pocustomdate2, pocustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." & dataUtama.Length : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'poid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "poid required numeric." : GoTo selesai
        End If
        'poasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "poasalbarangkategori required numeric." : GoTo selesai
        End If
        'pojenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pojenispembeliankategori required numeric." : GoTo selesai
        End If
        'pocarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pocarabayar required numeric." : GoTo selesai
        End If
        'poautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "poautonotransaksi required numeric." : GoTo selesai
        End If
        'potgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "potgl required date." : GoTo selesai
        End If
        'pokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pokodepa required numeric." : GoTo selesai
        End If
        'posupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "posupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "posupplier can't be empty." : GoTo selesai
        End If
        'pobagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pobagianpembelian required numeric." : GoTo selesai
        End If
        'potgldipenuhi(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "potgldipenuhi required date." : GoTo selesai
        End If
        'potgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "potgljatuhtempo required date." : GoTo selesai
        End If
        'potglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "potglnoref required date." : GoTo selesai
        End If
        'potglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "potglpenutupan required date." : GoTo selesai
        End If
        'pokurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pokurs required numeric." : GoTo selesai
        End If
        'pohargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pohargatermasukpajak required numeric." : GoTo selesai
        End If
        'pototal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pototal required numeric." : GoTo selesai
        End If
        'pojmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pojmldiskon required numeric." : GoTo selesai
        End If
        'pototalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pototalpajak1detail required numeric." : GoTo selesai
        End If
        'pototalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pototalpajak2detail required numeric." : GoTo selesai
        End If
        'pobiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pobiayalain required numeric." : GoTo selesai
        End If
        'pototaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pototaltransaksi required numeric." : GoTo selesai
        End If
        'pojmlbayar(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pojmlbayar required numeric." : GoTo selesai
        End If
        'poidpr(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "poidpr required numeric." : GoTo selesai
        End If
        'poidcs(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "poidcs required numeric." : GoTo selesai
        End If
        'poidrq(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "poidrq required numeric." : GoTo selesai
        End If
        'poidbs(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "poidbs required numeric." : GoTo selesai
        End If
        'postatusipc(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "postatusipc required numeric." : GoTo selesai
        End If
        'postatusgrn(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "postatusgrn required numeric." : GoTo selesai
        End If
        'postatusri(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "postatusri required numeric." : GoTo selesai
        End If
        'postatusdnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "postatusdnr required numeric." : GoTo selesai
        End If
        'postatusprt(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "postatusprt required numeric." : GoTo selesai
        End If
        'postatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "postatus required numeric." : GoTo selesai
        End If
        'postatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "postatussebelumnya required numeric." : GoTo selesai
        End If
        'pojmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "pojmlrevisi required numeric." : GoTo selesai
        End If
        'pocetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "pocetakanke required numeric." : GoTo selesai
        End If
        'poinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "poinputuser required numeric." : GoTo selesai
        End If
        'poinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "poinputtgl required date." : GoTo selesai
        End If
        'pomodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "pomodifikasiuser required numeric." : GoTo selesai
        End If
        'pomodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "pomodifikasitgl required date." : GoTo selesai
        End If
        'poisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "poisclose required numeric." : GoTo selesai
        End If
        'pocustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "pocustomint1 required numeric." : GoTo selesai
        End If
        'pocustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "pocustomint2 required numeric." : GoTo selesai
        End If
        'pocustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "pocustomint3 required numeric." : GoTo selesai
        End If
        'pocustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "pocustomdbl1 required numeric." : GoTo selesai
        End If
        'pocustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "pocustomdbl2 required numeric." : GoTo selesai
        End If
        'pocustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "pocustomdbl3 required numeric." : GoTo selesai
        End If
        'pocustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "pocustomdate1 required date." : GoTo selesai
        End If
        'pocustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "pocustomdate2 required date." : GoTo selesai
        End If
        'pocustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "pocustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pocabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pocabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pocabang should not be more than 25 character." : GoTo selesai
        End If

        'polokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "polokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "polokasi should not be more than 25 character." : GoTo selesai
        End If

        'pogudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pogudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pogudang should not be more than 25 character." : GoTo selesai
        End If

        'posumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "posumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "posumber should not be more than 10 character." : GoTo selesai
        End If

        'ponotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "ponotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "ponotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'potgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "potgl can't be empty" : GoTo selesai
        End If

        'potgldipenuhi(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "potgldipenuhi can't be empty" : GoTo selesai
        End If

        'potgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "potgljatuhtempo can't be empty" : GoTo selesai
        End If

        'potglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "potglnoref can't be empty" : GoTo selesai
        End If

        'potglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "potglpenutupan can't be empty" : GoTo selesai
        End If

        'pomatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pomatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "pomatauang should not be more than 25 character." : GoTo selesai
        End If

        'pokurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pokurs can't be empty" : GoTo selesai
        End If

        'pototal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "pototal can't be empty" : GoTo selesai
        End If

        'podiskonpersen(35) As String
        If Len(dataUtama(35)) = 0 Then
            result(2) = "podiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "podiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pojmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pojmldiskon can't be empty" : GoTo selesai
        End If

        'pototalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pototalpajak1detail can't be empty" : GoTo selesai
        End If

        'pototalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pototalpajak2detail can't be empty" : GoTo selesai
        End If

        'pobiayalainpersen(39) As String
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "pobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'pobiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pobiayalain can't be empty" : GoTo selesai
        End If

        'pototaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pototaltransaksi can't be empty" : GoTo selesai
        End If

        'pojmlbayar(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pojmlbayar can't be empty" : GoTo selesai
        End If

        'poinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "poinputtgl can't be empty" : GoTo selesai
        End If

        'pomodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "pomodifikasitgl can't be empty" : GoTo selesai
        End If

        'pocustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "pocustomdbl1 can't be empty" : GoTo selesai
        End If

        'pocustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "pocustomdbl2 can't be empty" : GoTo selesai
        End If

        'pocustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "pocustomdbl3 can't be empty" : GoTo selesai
        End If

        'pocustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "pocustomdate1 can't be empty" : GoTo selesai
        End If

        'pocustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "pocustomdate2 can't be empty" : GoTo selesai
        End If

        'pocustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "pocustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "poid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "polokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pojenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pojenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "posumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ponotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "posupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "posupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pobagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "potgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pouraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ponoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pomatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "podiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pomodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pomodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "poid~pocabang~polokasi~pogudang~poasalbarang~poasalbarangkategori~pojenispembelian~pojenispembeliankategori~pocarabayar~posumber~poautonotransaksi~ponotransaksi~potgl~pokodepa~posupplier~posupplierkontak~po1alamat1~po1alamat2~po1alamat3~po2alamat1~po2alamat2~po2alamat3~pobagianpembelian~potgldipenuhi~potermin~potgljatuhtempo~pouraian~pocatatan~ponoref~potglnoref~potglpenutupan~pomatauang~pokurs~pohargatermasukpajak~pototal~podiskonpersen~pojmldiskon~pototalpajak1detail~pototalpajak2detail~pobiayalainpersen~pobiayalain~pototaltransaksi~pojmlbayar~porekdiskon~porekpajak1~porekpajak2~porekbiayalain~porekbayar~poidpr~poidcs~poidrq~poidbs~postatusipc~postatusgrn~postatusri~postatusdnr~postatusprt~postatus~postatussebelumnya~pojmlrevisi~pocetakanke~poinputuser~poinputtgl~pomodifikasiuser~pomodifikasitgl~poisclose~pocustomtext1~pocustomtext2~pocustomtext3~pocustomtext4~pocustomtext5~pocustomint1~pocustomint2~pocustomint3~pocustomdbl1~pocustomdbl2~pocustomdbl3~pocustomdate1~pocustomdate2~pocustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpodetail(0) As Integer, idpo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, costcenter(23) As String, divisi(24) As String, 
        'subdivisi(25) As String, proyek(26) As String, catatan(27) As String, urutan(28) As Integer, idprdetail(29) As Integer, 
        'idcsdetail(30) As Integer, idrqdetail(31) As Integer, idbsdetail(32) As Integer, jmlipc(33) As Double, statusipc(34) As Integer, 
        'jmlgrn(35) As Double, statusgrn(36) As Integer, jmlri(37) As Double, statusri(38) As Integer, jmldnr(39) As Double, 
        'statusdnr(40) As Integer, jmlprt(41) As Double, statusprt(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargafix", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlipc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlgrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlprt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)

        'AMBIL SETTING WAJIB PR ATAU TIDAK
        Dim wajibPR As Integer = 0
        sql = "SELECT snilai FROM m0_setting WHERE smodule = 4 AND sgrup = 'options' AND skode = 'POWajibPR'"
        Dim dtWajibPR As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
        If dtWajibPR.Rows.Count > 0 Then
            wajibPR = FxDB(FixDouble(dtWajibPR(0)(0)), 0)
        Else
            result(2) = "Setting PO required PR not found." : GoTo selesai
        End If

        'Variabel ValidasiSimpan
        Dim ftBarang As String = ""
        Dim ftExistOutstandingPR As String = "", ftOutstandingPR As String = "", updNilaiPR As String = "", updFilterPR As String = ""
        Dim ftExistOutstandingRQ As String = "", ftOutstandingRQ As String = "", updNilaiRQ As String = "", updFilterRQ As String = ""
        Dim updStokBooking As String = "", gudang As String = ""
        Dim idbarang As Integer = 0, idprdetail As Integer = 0, idrqdetail As Integer = 0, jmlbarang As Double = 0

        Dim ftExistOutstandingPro As String = "", ftOutstandingPro As String = ""
        Dim updNilaiPro As String = "", updFilterPro As String = "", proyek As String = ""
        Dim cekProyek As Boolean = False, cekItemProyek As String = "", dtItemProyek As New DataTable

        'CEK ITEM PR WAJIB PROYEK
        Dim dtSett As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE smodule = 4 AND sgrup = 'options' AND skode = 'PoRequiredProject'", myConn)
        If dtSett.Rows.Count > 0 Then
            If Len(FxDB(dtSett.Rows(0)("snilai"), "")) > 0 Then
                dtItemProyek = AsDataTableAmbilDariDBCon("SELECT bid, bkode, bnama FROM m1_item WHERE " & FxDB(dtSett.Rows(0)("snilai"), "") & "", myConn)
                If dtItemProyek.Rows.Count > 0 Then
                    cekProyek = True
                End If
            End If
        End If


        'FILTER RQ, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftRQ As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 53) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idpo(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpo required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargafix(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargafix required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'jmlipc(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - jmlipc required numeric." : GoTo selesai
            End If
            'statusipc(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - statusipc required numeric." : GoTo selesai
            End If
            'jmlgrn(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - jmlgrn required numeric." : GoTo selesai
            End If
            'statusgrn(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - statusgrn required numeric." : GoTo selesai
            End If
            'jmlri(37) As Double
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(5) <= 0 Then
            If dataRowDetail(5) < 0 Then
                result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(8) <= 0 Then
            If dataRowDetail(8) < 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'proyek(26) As String
            If cekProyek Then
                'idbarang(2) As Integer
                cekItemProyek = AsDataTableDLookup(dtItemProyek, "bkode", "bid = " & dataRowDetail(2), "xxxEOFxxx")
                If cekItemProyek <> "xxxEOFxxx" Then
                    If Len(dataRowDetail(26)) = 0 Then
                        result(2) = "Row : " & i & " - project can't be empty" : GoTo selesai
                    End If
                End If
            End If

            'jmlipc(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - jmlipc can't be empty" : GoTo selesai
            End If

            'jmlgrn(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - jmlgrn can't be empty" : GoTo selesai
            End If

            'jmlri(37) As Double
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpodetail~idpo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~subtotal", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & ((dataRowDetail(5) * dataRowDetail(13)) - dataRowDetail(15)) * dataRowDetail(11)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String       , idprdetail(29) As Integer      , idrqdetail(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idprdetail = dataRowDetail(29) : idrqdetail = dataRowDetail(31)
            'proyek(26) As String
            proyek = dataRowDetail(26)

            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            If proyek <> "" Then
                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPro = IIf(Len(ftExistOutstandingPro.ToString) = 0, "", ftExistOutstandingPro & " UNION ")
                ftExistOutstandingPro = String.Concat(ftExistOutstandingPro, "SELECT EXISTS(SELECT 1 FROM m1_project WHERE pkode = '" & proyek & "' LIMIT 1) as rowExists, '" & proyek & "' as proyek")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "subtotal", "proyek = '" & proyek & "'")
                ftOutstandingPro = IIf(Len(ftOutstandingPro.ToString) = 0, "", ftOutstandingPro & " OR ")
                ftOutstandingPro = String.Concat(ftOutstandingPro, " (pkode = '" & proyek & "' AND " & Outstanding & " > (pnilaikontrak - prealisasikontrak)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPro = String.Concat("WHEN '" & proyek & "' THEN ROUND(prealisasikontrak + " & Outstanding & ", 5) ", updNilaiPro)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPro = IIf(Len(updFilterPro.ToString) = 0, "", updFilterPro & " OR ")
                updFilterPro = String.Concat(updFilterPro, "(pkode = '" & proyek & "')")
            End If

            'VALIDASI OUTSTANDING -------------------------
            If idprdetail <> 0 Then 'PR
                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPR = IIf(Len(ftExistOutstandingPR.ToString) = 0, "", ftExistOutstandingPR & " UNION ")
                ftExistOutstandingPR = String.Concat(ftExistOutstandingPR, "SELECT EXISTS(SELECT 1 FROM m4_pr_detail JOIN m4_pr ON idpr = prid WHERE idprdetail = '" & idprdetail & "' AND (prstatus = 2 OR prstatus = 3 OR prstatus = 4 OR prstatus = 7) LIMIT 1) as rowExists, '" & idprdetail & "' as idprdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                ftOutstandingPR = IIf(Len(ftOutstandingPR.ToString) = 0, "", ftOutstandingPR & " OR ")
                ftOutstandingPR = String.Concat(ftOutstandingPR, " (prd.idprdetail = " & idprdetail & " AND " & Outstanding & " > (prd.jmlbarang - prd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPR = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPR)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                updFilterPR = String.Concat(updFilterPR, "(idprdetail = '" & idprdetail & "')")

            Else
                'CEK WAJIB PR ATAU TIDAK
                If wajibPR = 1 Then
                    result(2) = "Row : " & i & " - PO required to retrieve data from PR." : GoTo selesai
                End If

            End If

            If idrqdetail <> 0 Then 'RQ
                'CEK RQ YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftRQ = IIf(Len(ftRQ.ToString) = 0, "", ftRQ & " OR ")
                ftRQ = String.Concat(ftRQ, " (rqd.idrqdetail = " & idrqdetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingRQ = IIf(Len(ftExistOutstandingRQ.ToString) = 0, "", ftExistOutstandingRQ & " UNION ")
                ftExistOutstandingRQ = String.Concat(ftExistOutstandingRQ, "SELECT EXISTS(SELECT 1 FROM m4_rq_detail JOIN m4_rq ON idrq = rqid WHERE idrqdetail = '" & idrqdetail & "' AND (rqstatus = 2 OR rqstatus = 3 OR rqstatus = 4 OR rqstatus = 7) LIMIT 1) as rowExists, '" & idrqdetail & "' as idrqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrqdetail=" & idrqdetail)
                ftOutstandingRQ = IIf(Len(ftOutstandingRQ.ToString) = 0, "", ftOutstandingRQ & " OR ")
                ftOutstandingRQ = String.Concat(ftOutstandingRQ, " (rqd.idrqdetail = " & idrqdetail & " AND " & Outstanding & " > (rqd.jmlbarang - rqd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiRQ = String.Concat("WHEN '" & idrqdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiRQ)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                updFilterRQ = String.Concat(updFilterRQ, "(idrqdetail = '" & idrqdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            '5. SET NILAI UPDATE STOK BOOKING
            updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA COST -------------------------------------------------------
        'idpocost(0) As Integer, idpo(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, rekdebit(6) As String, rekkredit(7) As String, kontak(8) As Integer, termasukhpp(9) As Integer, 
        'catatan(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'urutan(15) As Integer, idprcost(16) As Integer, idcscost(17) As Integer, idrqcost(18) As Integer, idbscost(19) As Integer, 
        'jumlahipc(20) As Double, statusipc(21) As Integer, jumlahgrn(22) As Double, statusgrn(23) As Integer, jumlahri(24) As Double, 
        'statusri(25) As Integer, jumlahbayar(26) As Double, statusbayar(27) As Integer, isclose(28) As Integer, customtext1(29) As String, 
        'customtext2(30) As String, customtext3(31) As String, customdbl1(32) As Double, customdbl2(33) As Double, customdbl3(34) As Double, 
        'customdate1(35) As Date, customdate2(36) As Date, customdate3(37) As Date

        'MAPPING BUAT FLEX DATA COST -----------------------------------------------------
        'idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idpocost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "kodecost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "jumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "termasukhpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idprcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idcscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idrqcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idbscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahipc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahgrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate3", AsEnumTypeData.AsString)

        'CEK PARAMETER DATA COST
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA COST ======================================================
            'SPLIT PARAMETER DATA COST
            dataCost = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA COST ===============================================

            'VALIDASI DAN SET DATA ROW Cost ==================================================
            Dim JmlDtCost As Integer = dataCost.Length
            For i = 1 To JmlDtCost
                'SPLIT DATA Cost
                dataRowCost = dataCost(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Cost -----------------------------------
                'CEK ARRAY DATA Cost
                If (dataRowCost.Length <> 38) Then
                    result(2) = "Cost Row : " & i & " - Invalid Cost transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Cost ----------------------------

                'VALIDASI TIPE DATA Cost ------------------------------------------
                'idpocost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                End If
                'idpo(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpo required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowCost(4)) = False) Then
                    result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowCost(5)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'kontak(8) As Integer
                If (IsNumeric(dataRowCost(8)) = False) Then
                    result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                End If
                'termasukhpp(9) As Integer
                If (IsNumeric(dataRowCost(9)) = False) Then
                    result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                End If
                'urutan(15) As Integer
                If (IsNumeric(dataRowCost(15)) = False) Then
                    result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idprcost(16) As Integer
                If (IsNumeric(dataRowCost(16)) = False) Then
                    result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                End If
                'idcscost(17) As Integer
                If (IsNumeric(dataRowCost(17)) = False) Then
                    result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                End If
                'idrqcost(18) As Integer
                If (IsNumeric(dataRowCost(18)) = False) Then
                    result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                End If
                'idbscost(19) As Integer
                If (IsNumeric(dataRowCost(19)) = False) Then
                    result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                End If
                'jumlahipc(20) As Double
                If (IsNumeric(dataRowCost(20)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahipc required numeric." : GoTo selesai
                End If
                'statusipc(21) As Integer
                If (IsNumeric(dataRowCost(21)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusipc required numeric." : GoTo selesai
                End If
                'jumlahgrn(22) As Double
                If (IsNumeric(dataRowCost(22)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahgrn required numeric." : GoTo selesai
                End If
                'statusgrn(23) As Integer
                If (IsNumeric(dataRowCost(23)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusgrn required numeric." : GoTo selesai
                End If
                'jumlahri(24) As Double
                If (IsNumeric(dataRowCost(24)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahri required numeric." : GoTo selesai
                End If
                'statusri(25) As Integer
                If (IsNumeric(dataRowCost(25)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusri required numeric." : GoTo selesai
                End If
                'jumlahbayar(26) As Double
                If (IsNumeric(dataRowCost(26)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                End If
                'statusbayar(27) As Integer
                If (IsNumeric(dataRowCost(27)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                End If
                'isclose(28) As Integer
                If (IsNumeric(dataRowCost(28)) = False) Then
                    result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(32) As Double
                If (IsNumeric(dataRowCost(32)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(33) As Double
                If (IsNumeric(dataRowCost(33)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(34) As Double
                If (IsNumeric(dataRowCost(34)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(35) As Date
                If (IsDate(dataRowCost(35)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(36) As Date
                If (IsDate(dataRowCost(36)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(37) As Date
                If (IsDate(dataRowCost(37)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA Cost -----------------------------------

                'VALIDASI DATA Cost ---------------------------------------
                'kodecost(2) As String
                If Len(dataRowCost(2)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(2)) > 25 Then
                    result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                End If

                'matauang(3) As String
                If Len(dataRowCost(3)) = 0 Then
                    result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(3)) > 25 Then
                    result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowCost(4)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowCost(5)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If

                'rekdebit(6) As String
                If dataRowCost(9) = 0 Then
                    If Len(dataRowCost(6)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                    End If
                End If
                If Len(dataRowCost(6)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                End If

                'rekkredit(7) As String
                If Len(dataRowCost(7)) = 0 Then
                    result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(7)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                End If

                'jumlahipc(20) As Double
                If Len(dataRowCost(20)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahipc can't be empty" : GoTo selesai
                End If

                'jumlahgrn(22) As Double
                If Len(dataRowCost(22)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahgrn can't be empty" : GoTo selesai
                End If

                'jumlahri(24) As Double
                If Len(dataRowCost(24)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahri can't be empty" : GoTo selesai
                End If

                'jumlahbayar(26) As Double
                If Len(dataRowCost(26)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                End If

                'customdbl1(32) As Double
                If Len(dataRowCost(32)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(33) As Double
                If Len(dataRowCost(33)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(34) As Double
                If Len(dataRowCost(34)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(35) As Date
                If Len(dataRowCost(35)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(36) As Date
                If Len(dataRowCost(36)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(37) As Date
                If Len(dataRowCost(37)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA Cost --------------------------------

                If AsDataTableTambahData(dtcost, "idpocost~idpo~kodecost~matauang~kurs~jumlah~rekdebit~rekkredit~kontak~termasukhpp~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~jumlahipc~statusipc~jumlahgrn~statusgrn~jumlahri~statusri~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34) & "~" & dataRowCost(35) & "~" & dataRowCost(36) & "~" & dataRowCost(37)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================

        End If


        'MAPPING BUAT WS DATA TRANS -------------------------------------------------------
        'idpotrans(0) As Integer, idpo(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customdbl1(12) As Double, customdbl2(13) As Double, customdbl3(14) As Double, 
        'customdbl4(15) As Double, customdbl5(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, 
        'customdate4(20) As Date, customdate5(21) As Date

        'MAPPING BUAT FLEX DATA TRANS -----------------------------------------------------
        'idpotrans, idpo, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, 
        'customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, 
        'customdate5

        'Buat datatable trans
        Dim dttrans As New DataTable
        AsDataTableTambahField(dttrans, "idpotrans", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "idpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dttrans, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "customdate5", AsEnumTypeData.AsString)

        'CEK PARAMETER DATA TRANS
        If dataSplit.Length > 3 Then
            If dataSplit(3).Length > 0 Then

                'VALIDASI DAN SET DATA TRANS ======================================================
                'SPLIT PARAMETER DATA TRANS
                dataTrans = dataSplit(3).Split(sptRow)
                'END OF VALIDASI DAN SET DATA TRANS ===============================================

                'VALIDASI DAN SET DATA ROW TRANS ==================================================
                Dim JmlDtTrans As Integer = dataTrans.Length
                For i = 1 To JmlDtTrans
                    'SPLIT DATA TRANS
                    dataRowTrans = dataTrans(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA TRANS -----------------------------------
                    'CEK ARRAY DATA TRANS
                    If (dataRowTrans.Length <> 22) Then
                        result(2) = "Trans Row : " & i & " - Invalid trans transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW TRANS ----------------------------

                    'VALIDASI TIPE DATA TRANS ------------------------------------------
                    'urutan(5) As Integer
                    If (IsNumeric(dataRowTrans(5)) = False) Then
                        result(2) = "Trans Row : " & i & "urutan required numeric." : GoTo selesai
                    End If
                    'isclose(6) As Integer
                    If (IsNumeric(dataRowTrans(6)) = False) Then
                        result(2) = "Trans Row : " & i & "isclose required numeric." : GoTo selesai
                    End If
                    'customdbl1(12) As Double
                    If (IsNumeric(dataRowTrans(12)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl1 required numeric." : GoTo selesai
                    End If
                    'customdbl2(13) As Double
                    If (IsNumeric(dataRowTrans(13)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl2 required numeric." : GoTo selesai
                    End If
                    'customdbl3(14) As Double
                    If (IsNumeric(dataRowTrans(14)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl3 required numeric." : GoTo selesai
                    End If
                    'customdbl4(15) As Double
                    If (IsNumeric(dataRowTrans(15)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl4 required numeric." : GoTo selesai
                    End If
                    'customdbl5(16) As Double
                    If (IsNumeric(dataRowTrans(16)) = False) Then
                        result(2) = "Trans Row : " & i & "customdbl5 required numeric." : GoTo selesai
                    End If
                    'customdate1(17) As Date
                    If (IsDate(dataRowTrans(17)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate1 required date." : GoTo selesai
                    End If
                    'customdate2(18) As Date
                    If (IsDate(dataRowTrans(18)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate2 required date." : GoTo selesai
                    End If
                    'customdate3(19) As Date
                    If (IsDate(dataRowTrans(19)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate3 required date." : GoTo selesai
                    End If
                    'customdate4(20) As Date
                    If (IsDate(dataRowTrans(20)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate4 required date." : GoTo selesai
                    End If
                    'customdate5(21) As Date
                    If (IsDate(dataRowTrans(21)) = False) Then
                        result(2) = "Trans Row : " & i & "customdate5 required date." : GoTo selesai
                    End If
                    'END OF VALIDASI TIPE DATA TRANS -----------------------------------

                    'VALIDASI DATA TRANS ---------------------------------------
                    'idpotrans(0) As Integer
                    If Len(dataRowTrans(0)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idpotrans can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(0)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idpotrans should not be more than 20 character." : GoTo selesai
                    End If

                    'idpo(1) As Integer
                    If Len(dataRowTrans(1)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idpo can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(1)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idpo should not be more than 20 character." : GoTo selesai
                    End If

                    'sumber(2) As String
                    If Len(dataRowTrans(2)) = 0 Then
                        result(2) = "Trans Row : " & i & " - sumber can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(2)) > 10 Then
                        result(2) = "Trans Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
                    End If

                    'idtransaksi(3) As Integer
                    If Len(dataRowTrans(3)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idtransaksi can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(3)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idtransaksi should not be more than 20 character." : GoTo selesai
                    End If

                    'customdbl1(12) As Double
                    If Len(dataRowTrans(12)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                    End If

                    'customdbl2(13) As Double
                    If Len(dataRowTrans(13)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                    End If

                    'customdbl3(14) As Double
                    If Len(dataRowTrans(14)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                    End If

                    'customdbl4(15) As Double
                    If Len(dataRowTrans(15)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
                    End If

                    'customdbl5(16) As Double
                    If Len(dataRowTrans(16)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
                    End If

                    'customdate1(17) As Date
                    If Len(dataRowTrans(17)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                    End If

                    'customdate2(18) As Date
                    If Len(dataRowTrans(18)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                    End If

                    'customdate3(19) As Date
                    If Len(dataRowTrans(19)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                    End If

                    'customdate4(20) As Date
                    If Len(dataRowTrans(20)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate4 can't be empty" : GoTo selesai
                    End If

                    'customdate5(21) As Date
                    If Len(dataRowTrans(21)) = 0 Then
                        result(2) = "Trans Row : " & i & " - customdate5 can't be empty" : GoTo selesai
                    End If
                    'END OF VALIDASI DATA TRANS --------------------------------

                    If AsDataTableTambahData(dttrans, "idpotrans~idpo~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customtext4~customtext5~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdate1~customdate2~customdate3~customdate4~customdate5", dataRowTrans(0) & "~" & dataRowTrans(1) & "~" & dataRowTrans(2) & "~" & dataRowTrans(3) & "~" & dataRowTrans(4) & "~" & dataRowTrans(5) & "~" & dataRowTrans(6) & "~" & dataRowTrans(7) & "~" & dataRowTrans(8) & "~" & dataRowTrans(9) & "~" & dataRowTrans(10) & "~" & dataRowTrans(11) & "~" & dataRowTrans(12) & "~" & dataRowTrans(13) & "~" & dataRowTrans(14) & "~" & dataRowTrans(15) & "~" & dataRowTrans(16) & "~" & dataRowTrans(17) & "~" & dataRowTrans(18) & "~" & dataRowTrans(19) & "~" & dataRowTrans(20) & "~" & dataRowTrans(21)) = False Then
                        result(2) = "Trans Row : " & i & " - insert into datatable failed." : GoTo selesai
                    End If

                Next
                'END OF VALIDASI DAN SET ROW DATA TRANS ===========================================

            End If
        End If


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 7
                Select Case drutama("postatus")
                    Case 0 : vAkses = 0
                    Case 1 : vAkses = 0
                    Case 2 : vAkses = 8
                    Case 3 : vAkses = 0
                    Case 4 : vAkses = 0
                    Case 5 : vAkses = 0
                    Case 6 : vAkses = 0
                    Case 7 : vAkses = 0
                    Case 8 : vAkses = 4
                    Case 9 : vAkses = 5
                    Case 10 : vAkses = 6
                    Case 11 : vAkses = 7
                    Case 12 : vAkses = 0
                End Select
                msgAkses = HakAkses(vModuleId, vMenuId, vAkses, userid)
                If Len(msgAkses) > 0 Then
                    result(2) = msgAkses : Trans.Rollback() : GoTo selesai
                End If
                'END OF CEK HAK AKSES STATUS =====================


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("potgl")), AsFormatTanggal(drutama("potgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================

                'CEK MATA UANG PO
                If drutama("pomatauang").ToString.ToUpper.Equals("IDR") Then
                    'JIKA IDR
                    'CEK DALAM 1 PO TIDAK BISA BERCAMPUR ANTARA ITEM CONSUMABLE DAN NON CONSUMABLE
                    sql = "SELECT GROUP_CONCAT(DISTINCT LEFT(bkode,2) ORDER BY bkode) as jenis FROM m1_item WHERE " & ftBarang & " GROUP BY LEFT(bkode,2) ASC "
                    Dim dtJenisPO As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim vJenis As Double = AsDataTableDCount(dtJenisPO, "jenis = 'W-'")
                    If vJenis > 0 Then drutama("pojenispembeliankategori") = 0 Else drutama("pojenispembeliankategori") = 1
                    If drutama("pojenispembeliankategori") = 0 Then
                        vJenis = AsDataTableDCount(dtJenisPO, "jenis <> 'W-'")
                        If vJenis > 0 Then
                            result(2) = "Can't mixed consumable and non consumable item in single PO transaction." : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                Else
                    'JIKA VALAS
                    drutama("pojenispembeliankategori") = 2
                End If
                

                'VALIDASI SIMPAN ========================================
                If drutama("postatus") = 2 Or drutama("postatus") = 1 Or drutama("postatus") = 8 Or drutama("postatus") = 9 Or drutama("postatus") = 10 Or drutama("postatus") = 11 Then
                    'CEK HAK AKSES
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                    'Dim rsCekHakAkses As String = HakAkses(4, 7, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                    'If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingPR, ftOutstandingPR, ftExistOutstandingRQ, ftOutstandingRQ, ftRQ, drutama("pohargatermasukpajak"), ftExistOutstandingPro, ftOutstandingPro)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    'VALIDASI HARGA BELI
                    If drutama("postatus") = 2 Then
                        Dim dtHACustom As DataTable = AsDataTableAmbilDariDBCon("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 4 AND pc.pcid = 11 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1", myConn)
                        If dtHACustom.Rows.Count > 0 Then
                            If dtHACustom.Rows(0)("rcakses") = 0 Then
                                GoTo validasihargabeli
                            End If

                        Else
validasihargabeli:
                            sql = "SELECT i.bid, '" & FixQuotes(drutama("pomatauang")) & "' as matauang, IFNULL(ip.khhargabeli,0) as hargabeli FROM m1_item i LEFT JOIN m1_item_price ip ON i.bid = ip.khidbarang AND ip.khmatauang = '" & FixQuotes(drutama("pomatauang")) & "' WHERE " & ftBarang
                            Dim dtHargaBeli As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtHargaBeli.Rows.Count > 0 Then
                                Dim dtval As New DataTable
                                For Each dr1 As DataRow In dtHargaBeli.Rows
                                    dtval = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr1("bid") & "' AND harga > " & dr1("hargabeli") & "", , , 1)
                                    If dtval.Rows.Count > 0 Then
                                        result(2) = "Row : " & dtval(0)("urutan") & " - " & dtval(0)("namabarang") & " price is greater then Item's Purchase Price (" & FormatNumber(dtval(0)("harga")) & " < " & FormatNumber(dr1("hargabeli")) & "). This role doesn't have permission to Approved this transaction." : Trans.Rollback() : GoTo selesai
                                    End If
                                Next
                            End If
                        End If

                    End If
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("potermin").ToString, AsFormatTanggal(drutama("potgl")), "potgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("potgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("pototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("pototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("pototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("pohargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("pototaltransaksi") = Double.Parse(drutama("pototal")) - Double.Parse(drutama("pojmldiskon")) + Double.Parse(drutama("pototalpajak1detail")) + Double.Parse(drutama("pototalpajak2detail")) + Double.Parse(drutama("pobiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("pototaltransaksi") = Double.Parse(drutama("pototal")) - Double.Parse(drutama("pojmldiskon")) + Double.Parse(drutama("pototalpajak2detail")) + Double.Parse(drutama("pobiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("poid")
                    notransaksi = drutama("ponotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(poid), ponotransaksi FROM M4_po WHERE poid='" & result(4) & "' AND postatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("poautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pocabang"), drutama("polokasi"), drutama("posumber"), drutama("potgl"), drutama("posumber"), 4)
                            Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                            arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                            'cek success generate notransaksi
                            If (arrNotransaksi(0) = 1) Then
                                notransaksi = arrNotransaksi(2)
                                'tambah query update m0_nomor_next
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = arrNotransaksi(3)
                                End With
                                objCmd.ExecuteNonQuery()
                            Else
                                result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF GENERATE NOTRANSAKSI ==================================

                        End If

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(poid) FROM m4_po WHERE ponotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_po_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Po_HistorySimpan("" & paramSplit(0) & "★M4_Po_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("posumber")) & "▼" & FixQuotes(drutama("poid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Po set pocabang  = '" & FixQuotes(drutama("pocabang")) & "', polokasi  = '" & FixQuotes(drutama("polokasi")) & "', pogudang  = '" & FixQuotes(drutama("pogudang")) & "', poasalbarang  = '" & FixQuotes(drutama("poasalbarang")) & "', poasalbarangkategori  = " & drutama("poasalbarangkategori") & ", pojenispembelian  = '" & FixQuotes(drutama("pojenispembelian")) & "', pojenispembeliankategori  = " & drutama("pojenispembeliankategori") & ", pocarabayar  = " & drutama("pocarabayar") & ", posumber  = '" & FixQuotes(drutama("posumber")) & "', poautonotransaksi  = " & drutama("poautonotransaksi") & ", ponotransaksi  = '" & notransaksi & "', potgl  = '" & FixQuotes(AsFormatTanggal(drutama("potgl"))) & "', pokodepa  = " & drutama("pokodepa") & ", posupplier  = " & drutama("posupplier") & ", posupplierkontak  = '" & FixQuotes(drutama("posupplierkontak")) & "', po1alamat1  = '" & FixQuotes(drutama("po1alamat1")) & "', po1alamat2  = '" & FixQuotes(drutama("po1alamat2")) & "', po1alamat3  = '" & FixQuotes(drutama("po1alamat3")) & "', po2alamat1  = '" & FixQuotes(drutama("po2alamat1")) & "', po2alamat2  = '" & FixQuotes(drutama("po2alamat2")) & "', po2alamat3  = '" & FixQuotes(drutama("po2alamat3")) & "', pobagianpembelian  = " & drutama("pobagianpembelian") & ", potgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("potgldipenuhi"))) & "', potermin  = '" & FixQuotes(drutama("potermin")) & "', potgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("potgljatuhtempo"))) & "', pouraian  = '" & FixQuotes(drutama("pouraian")) & "', pocatatan  = '" & FixQuotes(drutama("pocatatan")) & "', ponoref  = '" & FixQuotes(drutama("ponoref")) & "', potglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("potglnoref"))) & "', potglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("potglpenutupan"))) & "', pomatauang  = '" & FixQuotes(drutama("pomatauang")) & "', pokurs  = '" & FixDouble(drutama("pokurs")) & "', pohargatermasukpajak  = " & drutama("pohargatermasukpajak") & ", pototal  = '" & FixDouble(drutama("pototal")) & "', podiskonpersen  = '" & FixQuotes(drutama("podiskonpersen")) & "', pojmldiskon  = '" & FixDouble(drutama("pojmldiskon")) & "', pototalpajak1detail  = '" & FixDouble(drutama("pototalpajak1detail")) & "', pototalpajak2detail  = '" & FixDouble(drutama("pototalpajak2detail")) & "', pobiayalainpersen  = '" & FixQuotes(drutama("pobiayalainpersen")) & "', pobiayalain  = '" & FixDouble(drutama("pobiayalain")) & "', pototaltransaksi  = '" & FixDouble(drutama("pototaltransaksi")) & "', pojmlbayar  = '" & FixDouble(drutama("pojmlbayar")) & "', porekdiskon  = '" & FixQuotes(drutama("porekdiskon")) & "', porekpajak1  = '" & FixQuotes(drutama("porekpajak1")) & "', porekpajak2  = '" & FixQuotes(drutama("porekpajak2")) & "', porekbiayalain  = '" & FixQuotes(drutama("porekbiayalain")) & "', porekbayar  = '" & FixQuotes(drutama("porekbayar")) & "', poidpr  = " & drutama("poidpr") & ", poidcs  = " & drutama("poidcs") & ", poidrq  = " & drutama("poidrq") & ", poidbs  = " & drutama("poidbs") & ", postatusipc  = " & drutama("postatusipc") & ", postatusgrn  = " & drutama("postatusgrn") & ", postatusri  = " & drutama("postatusri") & ", postatusdnr  = " & drutama("postatusdnr") & ", postatusprt  = " & drutama("postatusprt") & ", postatus  = " & drutama("postatus") & ", postatussebelumnya  = " & drutama("postatussebelumnya") & ", pojmlrevisi  = pojmlrevisi+1, pocetakanke  = " & drutama("pocetakanke") & ", pomodifikasiuser  = " & userid & ", pomodifikasitgl  = NOW(), pocustomtext1  = '" & FixQuotes(drutama("pocustomtext1")) & "', pocustomtext2  = '" & FixQuotes(drutama("pocustomtext2")) & "', pocustomtext3  = '" & FixQuotes(drutama("pocustomtext3")) & "', pocustomtext4  = '" & FixQuotes(drutama("pocustomtext4")) & "', pocustomtext5  = '" & FixQuotes(drutama("pocustomtext5")) & "', pocustomint1  = " & drutama("pocustomint1") & ", pocustomint2  = " & drutama("pocustomint2") & ", pocustomint3  = " & drutama("pocustomint3") & ", pocustomdbl1  = '" & FixDouble(drutama("pocustomdbl1")) & "', pocustomdbl2  = '" & FixDouble(drutama("pocustomdbl2")) & "', pocustomdbl3  = '" & FixDouble(drutama("pocustomdbl3")) & "', pocustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate1"))) & "', pocustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate2"))) & "', pocustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate3"))) & "' where poid = '" & drutama("poid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("poautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pocabang"), drutama("polokasi"), drutama("posumber"), drutama("potgl"), drutama("posumber"), 4)
                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksi(0) = 1) Then
                            notransaksi = arrNotransaksi(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNotransaksi(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOTRANSAKSI ==================================

                    Else
                        notransaksi = drutama("ponotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(poid) FROM m4_po WHERE ponotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Po (pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, postatusprt, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, pomodifikasiuser, pomodifikasitgl, poisclose, pocustomtext1, pocustomtext2, pocustomtext3, pocustomtext4, pocustomtext5, pocustomint1, pocustomint2, pocustomint3, pocustomdbl1, pocustomdbl2, pocustomdbl3, pocustomdate1, pocustomdate2, pocustomdate3) values('" & FixQuotes(drutama("pocabang")) & "', '" & FixQuotes(drutama("polokasi")) & "', '" & FixQuotes(drutama("pogudang")) & "', '" & FixQuotes(drutama("poasalbarang")) & "', " & drutama("poasalbarangkategori") & ", '" & FixQuotes(drutama("pojenispembelian")) & "', " & drutama("pojenispembeliankategori") & ", " & drutama("pocarabayar") & ", '" & FixQuotes(drutama("posumber")) & "', " & drutama("poautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("potgl"))) & "', " & drutama("pokodepa") & ", " & drutama("posupplier") & ", '" & FixQuotes(drutama("posupplierkontak")) & "', '" & FixQuotes(drutama("po1alamat1")) & "', '" & FixQuotes(drutama("po1alamat2")) & "', '" & FixQuotes(drutama("po1alamat3")) & "', '" & FixQuotes(drutama("po2alamat1")) & "', '" & FixQuotes(drutama("po2alamat2")) & "', '" & FixQuotes(drutama("po2alamat3")) & "', " & drutama("pobagianpembelian") & ", '" & FixQuotes(AsFormatTanggal(drutama("potgldipenuhi"))) & "', '" & FixQuotes(drutama("potermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("potgljatuhtempo"))) & "', '" & FixQuotes(drutama("pouraian")) & "', '" & FixQuotes(drutama("pocatatan")) & "', '" & FixQuotes(drutama("ponoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("potglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("potglpenutupan"))) & "', '" & FixQuotes(drutama("pomatauang")) & "', '" & FixDouble(drutama("pokurs")) & "', " & drutama("pohargatermasukpajak") & ", '" & FixDouble(drutama("pototal")) & "', '" & FixQuotes(drutama("podiskonpersen")) & "', '" & FixDouble(drutama("pojmldiskon")) & "', '" & FixDouble(drutama("pototalpajak1detail")) & "', '" & FixDouble(drutama("pototalpajak2detail")) & "', '" & FixQuotes(drutama("pobiayalainpersen")) & "', '" & FixDouble(drutama("pobiayalain")) & "', '" & FixDouble(drutama("pototaltransaksi")) & "', '" & FixDouble(drutama("pojmlbayar")) & "', '" & FixQuotes(drutama("porekdiskon")) & "', '" & FixQuotes(drutama("porekpajak1")) & "', '" & FixQuotes(drutama("porekpajak2")) & "', '" & FixQuotes(drutama("porekbiayalain")) & "', '" & FixQuotes(drutama("porekbayar")) & "', " & drutama("poidpr") & ", " & drutama("poidcs") & ", " & drutama("poidrq") & ", " & drutama("poidbs") & ", " & drutama("postatusipc") & ", " & drutama("postatusgrn") & ", " & drutama("postatusri") & ", " & drutama("postatusdnr") & ", " & drutama("postatusprt") & ", " & drutama("postatus") & ", " & drutama("postatussebelumnya") & ", " & drutama("pojmlrevisi") & ", " & drutama("pocetakanke") & ", " & drutama("poinputuser") & ", NOW(), " & drutama("pomodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("poisclose") & ", '" & FixQuotes(drutama("pocustomtext1")) & "', '" & FixQuotes(drutama("pocustomtext2")) & "', '" & FixQuotes(drutama("pocustomtext3")) & "', '" & FixQuotes(drutama("pocustomtext4")) & "', '" & FixQuotes(drutama("pocustomtext5")) & "', " & drutama("pocustomint1") & ", " & drutama("pocustomint2") & ", " & drutama("pocustomint3") & ", '" & FixDouble(drutama("pocustomdbl1")) & "', '" & FixDouble(drutama("pocustomdbl2")) & "', '" & FixDouble(drutama("pocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate3"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDBCon("select poid from M4_po where ponotransaksi='" & notransaksi & "' AND poinputuser= '" & userid & "' order by pomodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Po_Detail where idpo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
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
                        strValue2.Append("(" & dr1("idpodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Po_Detail(idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Po_Cost where idpo = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses cost
                If (dtcost.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtcost.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idpocost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', " & dr1("kontak") & ", " & dr1("termasukhpp") & ", '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", '" & FixDouble(dr1("jumlahipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jumlahgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jumlahri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Po_Cost(idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus trans ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_po_Trans where idpo = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses trans
                If (dttrans.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dttrans.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("idpotrans")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "')")
                    Next
                    sql = "Insert into M4_po_Trans(idpotrans, idpo, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("postatus") = 2 Then

                    If Len(updNilaiPro) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m1_project SET prealisasikontrak = (CASE pkode " & updNilaiPro & " ELSE prealisasikontrak END) WHERE " & updFilterPro
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                        'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    End If

                    If Len(updNilaiPR) > 0 Then 'PR
                        'UPDATE DETAIL
                        sql = "UPDATE m4_pr_detail SET jmlrealisasi = (CASE idprdetail " & updNilaiPR & " ELSE jmlrealisasi END) WHERE " & updFilterPR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpr FROM M4_pr_detail WHERE " & updFilterPR & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPR = "" : updFilterPR = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPR = String.Concat(updNilaiPR, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                                updFilterPR = String.Concat(updFilterPR, "(prid = '" & dr1("idpr") & "')")
                            Next

                            sql = "UPDATE m4_pr SET prstatusrealisasi = (CASE prid " & updNilaiPR & " ELSE prstatusrealisasi END) WHERE " & updFilterPR
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    If Len(updNilaiRQ) > 0 Then 'RQ
                        'UPDATE DETAIL
                        sql = "UPDATE m4_rq_detail SET jmlrealisasi = (CASE idrqdetail " & updNilaiRQ & " ELSE jmlrealisasi END) WHERE " & updFilterRQ
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idrq FROM m4_rq_detail WHERE " & updFilterRQ & " GROUP BY idrq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idrq = '" & dr1("idrq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idrq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_rq_detail WHERE " & ftDetail & " GROUP BY idrq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiRQ = "" : updFilterRQ = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiRQ = String.Concat(updNilaiRQ, "WHEN '" & dr1("idrq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                                updFilterRQ = String.Concat(updFilterRQ, "(rqid = '" & dr1("idrq") & "')")
                            Next

                            sql = "UPDATE m4_rq SET rqstatusrealisasi = (CASE rqid " & updNilaiRQ & " ELSE rqstatusrealisasi END) WHERE " & updFilterRQ
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'UPDATE STOK BOOKING ================================================================
                    If Len(updStokBooking) > 0 Then
                        sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK BOOKING =========================================================

                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'ambil moduleid dan menuid dari m0_nomor
                Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'", myConn)
                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

                sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF INSERT USER LOG =============================================================

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

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
        'myconn.Close()
        'myconn = Nothing
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
    Public Function M4_PoUpdateStatus(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", nilaiStatus As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"
        Dim idtransaksi As String = "", idtransaksih As String = ""
        Dim dtdetail As DataTable
        Dim isDelete As Boolean = False

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""

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
            Filter = Filter.Replace("posupplierkode", "c1.kkode")
            Filter = Filter.Replace("posuppliernama", "c1.knama")
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

        'VALIDASI DAN SET ISDELETE =========================================================
        'CEK ISDELETE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isdelete required numeric." : GoTo selesai
        Else
            'SET ISDELETE
            If (Val(paramSplit(4)) = 1) Then
                isDelete = True
            Else
                isDelete = False
            End If
        End If
        'END OF VALIDASI DAN SET ISDELETE ==================================================

        'VALIDASI DAN SET NILAISTATUS ======================================================
        'SPILIT PARAMETER NILAISTATUS
        nilaiSplit = paramSplit(5).Split(sptSubParam)

        'CEK ARRAY NILAISTATUS
        If (nilaiSplit.Length <> 2) Then
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If

        'CEK IDTRANSAKSI
        If (IsNumeric(nilaiSplit(0)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = nilaiSplit(0)

        'SET NILAI STATUS
        If (Len(nilaiSplit(1)) > 0) Then
            'JIKA NUMERIC MAKA NILAISTATUS = PARAM NILAI STATUS YG DIINPUT
            'JIKA TIDAK MAKA NILAISTATUS = UNCLOSE
            If (IsNumeric(nilaiSplit(1)) = True) Then
                nilaiStatus = nilaiSplit(1)
                'JIKA NILAI STATUS < 0 ATAU NILAI STATUS > 12 MAKA NILAISTATUS TIDAK VALID
                If (nilaiStatus < 0 Or nilaiStatus > 12) Then
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            Else
                If (nilaiSplit(1).ToString.ToLower = "unclose") Then
                    nilaiStatus = "unclose"
                Else
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            End If
        Else
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET NILAISTATUS ================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Po", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Potgl, Ponotransaksi, Postatus FROM M4_Po WHERE Poid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Postatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m4_po_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Po_HistorySimpan("" & paramSplit(0) & "★M4_Po_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0, idrqdetail As Integer = 0
            Dim updNilaiPR As String = "", updFilterPR As String = "", updNilaiRQ As String = "", updFilterRQ As String = ""
            Dim gudang As String = "", updStokBooking As String = ""
            Dim ftOutstandingPro As String = "", updNilaiPro As String = "", updFilterPro As String = "", proyek As String = ""

            If jnsaktivitas = 7 Then
                'JIKA CLOSE MAKA KURANGI BOOKING
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, jmlbarang - jmlrealisasi as jmlupdate, gudang FROM m4_po_detail WHERE idpo = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlupdate") : gudang = dr1("gudang")
                        updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok
                    Next
                End If

                'UPDATE STOK BOOKING ================================
                If Len(updStokBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =========================

            ElseIf jnsaktivitas = 17 Then
                'JIKA UNCLOSE MAKA TAMBAH BOOKING
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, jmlbarang - jmlrealisasi as jmlupdate, gudang FROM m4_po_detail WHERE idpo = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlupdate") : gudang = dr1("gudang")
                        updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok
                    Next
                End If

                'UPDATE STOK BOOKING ================================
                If Len(updStokBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =========================

            End If


            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m4_po_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idprdetail, idrqdetail, urutan, proyek, ((jml * harga) - jmldiskon) * kurs as subtotal FROM m4_po_detail WHERE idpo = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idprdetail = dr1("idprdetail") : idrqdetail = dr1("idrqdetail")
                        proyek = dr1("proyek")

                        If proyek <> "" Then
                            '1. SET NILAI UPDATE OUTSTANDING ----------
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "subtotal", "proyek='" & proyek & "'")
                            updNilaiPro = String.Concat("WHEN '" & proyek & "' THEN ROUND(prealisasikontrak - " & Outstanding & ", 5) ", updNilaiPro)
                            '2. SET FILTERUPDATE OUTSTANDING ----------
                            updFilterPro = IIf(Len(updFilterPro.ToString) = 0, "", updFilterPro & " OR ")
                            updFilterPro = String.Concat(updFilterPro, "(pkode = '" & proyek & "')")
                        End If

                        'UPDATE OUTSTANDING ---------------------------
                        If idprdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                            updNilaiPR = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPR)
                            '2. SET FILTERUPDATE OUTSTANDING PR
                            updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                            updFilterPR = String.Concat(updFilterPR, "(idprdetail = '" & idprdetail & "')")
                        End If

                        If idrqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING RQ
                            Dim OutstandingRQ As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrqdetail=" & idrqdetail)
                            updNilaiRQ = String.Concat("WHEN '" & idrqdetail & "' THEN ROUND(jmlrealisasi - '" & OutstandingRQ & "', 5) ", updNilaiRQ)
                            '2. SET FILTERUPDATE OUTSTANDING RQ
                            updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                            updFilterRQ = String.Concat(updFilterRQ, "(idrqdetail = '" & idrqdetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                        '3. SET NILAI UPDATE STOK BOOKING KELUAR -------------
                        updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilterPro) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m1_project SET prealisasikontrak = (CASE pkode " & updNilaiPro & " ELSE prealisasikontrak END) WHERE " & updFilterPro
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE OUTSTANDING DETAIL ---------------
                End If

                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterPR) > 0 Then 'PR
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_pr_detail SET jmlrealisasi = (CASE idprdetail " & updNilaiPR & " ELSE jmlrealisasi END) WHERE " & updFilterPR
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpr FROM M4_pr_detail WHERE " & updFilterPR & " GROUP BY idpr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPR = "" : updFilterPR = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiPR = String.Concat(updNilaiPR, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                            updFilterPR = String.Concat(updFilterPR, "(prid = '" & dr1("idpr") & "')")
                        Next

                        sql = "UPDATE m4_pr SET prstatusrealisasi = (CASE prid " & updNilaiPR & " ELSE prstatusrealisasi END) WHERE " & updFilterPR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If

                If Len(updFilterRQ) > 0 Then 'RQ
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m4_rq_detail SET jmlrealisasi = (CASE idrqdetail " & updNilaiRQ & " ELSE jmlrealisasi END) WHERE " & updFilterRQ
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA --------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idrq FROM m4_rq_detail WHERE " & updFilterRQ & " GROUP BY idrq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idrq = '" & dr1("idrq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idrq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_rq_detail WHERE " & ftDetail & " GROUP BY idrq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiRQ = "" : updFilterRQ = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiRQ = String.Concat(updNilaiRQ, "WHEN '" & dr1("idrq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                            updFilterRQ = String.Concat(updFilterRQ, "(rqid = '" & dr1("idrq") & "')")
                        Next

                        sql = "UPDATE m4_rq SET rqstatusrealisasi = (CASE rqid " & updNilaiRQ & " ELSE rqstatusrealisasi END) WHERE " & updFilterRQ
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================

                'UPDATE STOK BOOKING ================================
                If Len(updStokBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =========================

            End If

            'update status utama
            sql = "UPDATE M4_Po SET Postatus = " & nilaiStatus & ", Pomodifikasiuser='" & userid & "', Pomodifikasitgl = NOW(), Poposting = 0, Popostingtgl = '1971-01-01 00:00:00', Pojmlrevisi = Pojmlrevisi + 1 WHERE Poid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M4_PoSearch(PostWsSearch(paramSplit(0), "M4_PoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'myconn.Close()
        'myconn = Nothing
        'UPDATE OF SIMPAN KE DATABASE ==========================================================

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
    Public Function M4_PoDelete(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

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
            Filter = Filter.Replace("posupplierkode", "c1.kkode")
            Filter = Filter.Replace("posuppliernama", "c1.knama")
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
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Po", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Poid, Ponotransaksi FROM M4_Po WHERE Poid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pocabang, polokasi, posumber, poautonotransaksi, ponotransaksi, potgl"
            sql &= " FROM M4_po"
            sql &= " WHERE poid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pocabang")
                lokasi = dtNomorNext.Rows(0)("polokasi")
                sumber = dtNomorNext.Rows(0)("posumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("poautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ponotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("potgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE TRANS
            sql = "DELETE FROM M4_po_Trans WHERE idpo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE COST
            sql = "DELETE FROM M4_po_Cost WHERE idpo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Po_Detail WHERE idpo = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Po WHERE poid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'UPDATE NOMOR BERIKUTNYA ============================================================
            'JIKA AUTO NO. TRANSAKSI
            If autonotransaksi = 1 Then
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 4)
                Dim arrNomorNext(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                arrNomorNext = rsNomorNext.Split(sptSubParam)
                'Cek success M0_DeleteNotransaksi
                If (arrNomorNext(0) = 1) Then
                    sql = arrNomorNext(3)
                    'Tambah query update m0_nomor_next
                    If Len(sql) > 0 Then
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                Else
                    result(2) = arrNomorNext(1) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF UPDATE NOMOR BERIKUTNYA =====================================================


            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M4_PoSearch(PostWsSearch(paramSplit(0), "M4_PoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'myconn.Close()
        'myconn = Nothing
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
    Public Function M4_PoGetdataById(ByVal param As String) As String

        'M4_PoGetdataById Utama --------------------------------------------------------
        'poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, 
        'pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, 
        'posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, 
        'po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, 
        'ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, 
        'podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, 
        'pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, 
        'poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, 
        'postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, 
        'poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocustomtext1, 
        'pocustomtext2, pocustomtext3, pocustomtext4, pocustomtext5, pocustomint1, pocustomint2, pocustomint3, 
        'pocustomdbl1, pocustomdbl2, pocustomdbl3, pocustomdate1, pocustomdate2, pocustomdate3, pocabangnama, 
        'polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, poterminnama, 
        'poterminharijatuhtempo, porekdiskonnama, porekpajak1nama, porekpajak2nama, porekbiayalainnama, porekbayarnama, ponotransaksipr, 
        'ponotransaksics, ponotransaksirq, ponotransaksibs, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama , kpkp

        'M4_PoGetdataById Detail -------------------------------------------------------
        'idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, rqnotransaksi, 
        'bsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_PoGetdataById Cost -------------------------------------------------------
        'idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, 
        'kontaknama, costcenternama, divisinama, subdivisinama

        'M4_PoGetdataById Trans -------------------------------------------------------
        'idpotrans, idpo, sumber, idtransaksi, catatan, urutan, isclose, customtext1, 
        'customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, 
        'customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5,
        'notransaksi, tgltransaksi, kontak, kontakkode, kontaknama

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

        Dim utama As String = "", detail As String = "", cost As String = "", trans As String = "", idtransaksi As String = ""

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
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M4_Po~M4_Po_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "poid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "poid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_po_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("poid"), 0), sptField,
                     FxDB(drutama("pocabang"), ""), sptField,
                     FxDB(drutama("polokasi"), ""), sptField,
                     FxDB(drutama("pogudang"), ""), sptField,
                     FxDB(drutama("poasalbarang"), ""), sptField,
                     FxDB(drutama("poasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pojenispembelian"), ""), sptField,
                     FxDB(drutama("pojenispembeliankategori"), 0), sptField,
                     FxDB(drutama("pocarabayar"), 0), sptField,
                     FxDB(drutama("posumber"), ""), sptField,
                     FxDB(drutama("poautonotransaksi"), 0), sptField,
                     FxDB(drutama("ponotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("potgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pokodepa"), 0), sptField,
                     FxDB(drutama("posupplier"), 0), sptField,
                     FxDB(drutama("posupplierkontak"), ""), sptField,
                     FxDB(drutama("po1alamat1"), ""), sptField,
                     FxDB(drutama("po1alamat2"), ""), sptField,
                     FxDB(drutama("po1alamat3"), ""), sptField,
                     FxDB(drutama("po2alamat1"), ""), sptField,
                     FxDB(drutama("po2alamat2"), ""), sptField,
                     FxDB(drutama("po2alamat3"), ""), sptField,
                     FxDB(drutama("pobagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("potgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("potermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("potgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pouraian"), ""), sptField,
                     FxDB(drutama("pocatatan"), ""), sptField,
                     FxDB(drutama("ponoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("potglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("potglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("pomatauang"), ""), sptField,
                     FxDB(drutama("pokurs"), 0), sptField,
                     FxDB(drutama("pohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pototal"), 0), sptField,
                     FxDB(drutama("podiskonpersen"), ""), sptField,
                     FxDB(drutama("pojmldiskon"), 0), sptField,
                     FxDB(drutama("pototalpajak1detail"), 0), sptField,
                     FxDB(drutama("pototalpajak2detail"), 0), sptField,
                     FxDB(drutama("pobiayalainpersen"), ""), sptField,
                     FxDB(drutama("pobiayalain"), 0), sptField,
                     FxDB(drutama("pototaltransaksi"), 0), sptField,
                     FxDB(drutama("pojmlbayar"), 0), sptField,
                     FxDB(drutama("porekdiskon"), ""), sptField,
                     FxDB(drutama("porekpajak1"), ""), sptField,
                     FxDB(drutama("porekpajak2"), ""), sptField,
                     FxDB(drutama("porekbiayalain"), ""), sptField,
                     FxDB(drutama("porekbayar"), ""), sptField,
                     FxDB(drutama("poidpr"), 0), sptField,
                     FxDB(drutama("poidcs"), 0), sptField,
                     FxDB(drutama("poidrq"), 0), sptField,
                     FxDB(drutama("poidbs"), 0), sptField,
                     FxDB(drutama("postatusipc"), 0), sptField,
                     FxDB(drutama("postatusgrn"), 0), sptField,
                     FxDB(drutama("postatusri"), 0), sptField,
                     FxDB(drutama("postatusdnr"), 0), sptField,
                     FxDB(drutama("postatusprt"), 0), sptField,
                     FxDB(drutama("postatusrealisasi"), 0), sptField,
                     FxDB(drutama("postatus"), 0), sptField,
                     FxDB(drutama("postatussebelumnya"), 0), sptField,
                     FxDB(drutama("pojmlrevisi"), 0), sptField,
                     FxDB(drutama("pocetakanke"), 0), sptField,
                     FxDB(drutama("poinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("poinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pomodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pomodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("poposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("popostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("poisclose"), 0), sptField,
                     FxDB(drutama("pocustomtext1"), ""), sptField,
                     FxDB(drutama("pocustomtext2"), ""), sptField,
                     FxDB(drutama("pocustomtext3"), ""), sptField,
                     FxDB(drutama("pocustomtext4"), ""), sptField,
                     FxDB(drutama("pocustomtext5"), ""), sptField,
                     FxDB(drutama("pocustomint1"), 0), sptField,
                     FxDB(drutama("pocustomint2"), 0), sptField,
                     FxDB(drutama("pocustomint3"), 0), sptField,
                     FxDB(drutama("pocustomdbl1"), 0), sptField,
                     FxDB(drutama("pocustomdbl2"), 0), sptField,
                     FxDB(drutama("pocustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pocustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pocabangnama"), ""), sptField,
                     FxDB(drutama("polokasinama"), ""), sptField,
                     FxDB(drutama("pogudangnama"), ""), sptField,
                     FxDB(drutama("posupplierkode"), ""), sptField,
                     FxDB(drutama("posuppliernama"), ""), sptField,
                     FxDB(drutama("pobagianpembeliankode"), ""), sptField,
                     FxDB(drutama("pobagianpembeliannama"), ""), sptField,
                     FxDB(drutama("poterminnama"), ""), sptField,
                     FxDB(drutama("poterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("porekdiskonnama"), ""), sptField,
                     FxDB(drutama("porekpajak1nama"), ""), sptField,
                     FxDB(drutama("porekpajak2nama"), ""), sptField,
                     FxDB(drutama("porekbiayalainnama"), ""), sptField,
                     FxDB(drutama("porekbayarnama"), ""), sptField,
                     FxDB(drutama("ponotransaksipr"), ""), sptField,
                     FxDB(drutama("ponotransaksics"), ""), sptField,
                     FxDB(drutama("ponotransaksirq"), ""), sptField,
                     FxDB(drutama("ponotransaksibs"), ""), sptField,
                     FxDB(drutama("postatusnama"), ""), sptField,
                     FxDB(drutama("postatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("poinputusernama"), ""), sptField,
                     FxDB(drutama("pomodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idpo"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargafix"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)


            'AMBIL DATA COST
            sql = "SELECT poc.idpocost, poc.idpo, poc.kodecost, poc.matauang, poc.kurs, poc.jumlah, poc.rekdebit, poc.rekkredit, poc.kontak, poc.termasukhpp, poc.catatan, poc.costcenter, poc.divisi, poc.subdivisi, poc.proyek, poc.urutan, poc.idprcost, poc.idcscost, poc.idrqcost, poc.idbscost, poc.jumlahipc, poc.statusipc, poc.jumlahgrn, poc.statusgrn, poc.jumlahri, poc.statusri, poc.jumlahbayar, poc.statusbayar, poc.isclose, poc.customtext1, poc.customtext2, poc.customtext3, poc.customdbl1, poc.customdbl2, poc.customdbl3, poc.customdate1, poc.customdate2, poc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama,  c.kkode as kontakkode, c.knama as kontaknama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sddivisi as subdivisinama FROM m4_po_cost poc JOIN m4_po po ON poc.idpo = po.poid LEFT JOIN m1_other_cost oc ON poc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON poc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON poc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON poc.kontak = c.kid LEFT JOIN m1_cost_center cc ON poc.costcenter = cc.cckode LEFT JOIN m1_division d ON poc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON poc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_po_cost", Filter, "poc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idpocost"), ""), sptField,
                     FxDB(dr("idpo"), ""), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("termasukhpp"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), ""), sptField,
                     FxDB(dr("idcscost"), ""), sptField,
                     FxDB(dr("idrqcost"), ""), sptField,
                     FxDB(dr("idbscost"), ""), sptField,
                     FxDB(dr("jumlahipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jumlahgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jumlahri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jumlahbayar"), 0), sptField,
                     FxDB(dr("statusbayar"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodecostnama"), ""), sptField,
                     FxDB(dr("rekdebitnama"), ""), sptField,
                     FxDB(dr("rekkreditnama"), ""), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost

            'AMBIL DATA TRANS
            sql = "SELECT potrans.idpotrans, potrans.idpo, potrans.sumber, potrans.idtransaksi, potrans.catatan, potrans.urutan, potrans.isclose, potrans.customtext1, potrans.customtext2, potrans.customtext3, potrans.customtext4, potrans.customtext5, potrans.customdbl1, potrans.customdbl2, potrans.customdbl3, potrans.customdbl4, potrans.customdbl5, potrans.customdate1, potrans.customdate2, potrans.customdate3, potrans.customdate4, potrans.customdate5, m5si.sinotransaksi as notransaksi, m5si.sitgl as tgltransaksi, m5si.sicustomer as kontak, c.kkode as kontakkode,  c.knama as kontaknama FROM m4_po_trans potrans LEFT JOIN m5_si m5si  ON potrans.sumber = m5si.sisumber AND potrans.idtransaksi = m5si.siid LEFT JOIN m1_contact c ON m5si.sicustomer = c.kid"
            Dim dttrans As New DataTable
            dttrans = AmbilData("aplikasi1-m1_no_trans_out", "potrans.idpo = '" & idtransaksi & "'", "potrans.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dttrans.Rows
                trans = String.Concat(trans,
                     FxDB(dr("idpotrans"), 0), sptField,
                     FxDB(dr("idpo"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), "1900-01-01"), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgltransaksi"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptRow)
            Next
            If trans.Length > 0 Then trans = trans.Substring(0, trans.Length - sptRow.Length) Else trans = trans

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = " transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, cost, sptSubParam, trans)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocustomtext1, pocustomtext2, pocustomtext3, pocustomtext4, pocustomtext5, pocustomint1, pocustomint2, pocustomint3, pocustomdbl1, pocustomdbl2, pocustomdbl3, pocustomdate1, pocustomdate2, pocustomdate3, pocabangnama, polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, poterminnama, poterminharijatuhtempo, porekdiskonnama, porekpajak1nama, porekpajak2nama, porekbiayalainnama, porekbayarnama, ponotransaksipr, ponotransaksics, ponotransaksirq, ponotransaksibs, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama, kpkp" & sptSubParam & "idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama" & sptSubParam & "idpotrans, idpo, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5, notransaksi, tgltransaksi, kontak, kontakkode, kontaknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PoSearch(ByVal param As String) As String
        'M4_PoSearch --------------------------------------------------------
        'poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, 
        'pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, 
        'posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, 
        'po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, 
        'ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, 
        'podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, 
        'pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, 
        'poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, 
        'postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, 
        'poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocabangnama, 
        'polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, prnotransaksi, 
        'csnotransaksi, rqnotransaksi, bsnotransaksi, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama

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
            Filter = Filter.Replace("posupplierkode", "c1.kkode")
            Filter = Filter.Replace("posuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_po_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Po", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("poid"), 0), sptField,
                     FxDB(dr("pocabang"), ""), sptField,
                     FxDB(dr("polokasi"), ""), sptField,
                     FxDB(dr("pogudang"), ""), sptField,
                     FxDB(dr("poasalbarang"), ""), sptField,
                     FxDB(dr("poasalbarangkategori"), 0), sptField,
                     FxDB(dr("pojenispembelian"), ""), sptField,
                     FxDB(dr("pojenispembeliankategori"), 0), sptField,
                     FxDB(dr("pocarabayar"), 0), sptField,
                     FxDB(dr("posumber"), ""), sptField,
                     FxDB(dr("poautonotransaksi"), 0), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgl"), ""), formatTgl), sptField,
                     FxDB(dr("pokodepa"), 0), sptField,
                     FxDB(dr("posupplier"), 0), sptField,
                     FxDB(dr("posupplierkontak"), ""), sptField,
                     FxDB(dr("po1alamat1"), ""), sptField,
                     FxDB(dr("po1alamat2"), ""), sptField,
                     FxDB(dr("po1alamat3"), ""), sptField,
                     FxDB(dr("po2alamat1"), ""), sptField,
                     FxDB(dr("po2alamat2"), ""), sptField,
                     FxDB(dr("po2alamat3"), ""), sptField,
                     FxDB(dr("pobagianpembelian"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("potgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("potermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pouraian"), ""), sptField,
                     FxDB(dr("pocatatan"), ""), sptField,
                     FxDB(dr("ponoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("potglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("pomatauang"), ""), sptField,
                     FxDB(dr("pokurs"), 0), sptField,
                     FxDB(dr("pohargatermasukpajak"), 0), sptField,
                     FxDB(dr("pototal"), 0), sptField,
                     FxDB(dr("podiskonpersen"), ""), sptField,
                     FxDB(dr("pojmldiskon"), 0), sptField,
                     FxDB(dr("pototalpajak1detail"), 0), sptField,
                     FxDB(dr("pototalpajak2detail"), 0), sptField,
                     FxDB(dr("pobiayalainpersen"), ""), sptField,
                     FxDB(dr("pobiayalain"), 0), sptField,
                     FxDB(dr("pototaltransaksi"), 0), sptField,
                     FxDB(dr("pojmlbayar"), 0), sptField,
                     FxDB(dr("porekdiskon"), ""), sptField,
                     FxDB(dr("porekpajak1"), ""), sptField,
                     FxDB(dr("porekpajak2"), ""), sptField,
                     FxDB(dr("porekbiayalain"), ""), sptField,
                     FxDB(dr("porekbayar"), ""), sptField,
                     FxDB(dr("poidpr"), 0), sptField,
                     FxDB(dr("poidcs"), 0), sptField,
                     FxDB(dr("poidrq"), 0), sptField,
                     FxDB(dr("poidbs"), 0), sptField,
                     FxDB(dr("postatusipc"), 0), sptField,
                     FxDB(dr("postatusgrn"), 0), sptField,
                     FxDB(dr("postatusri"), 0), sptField,
                     FxDB(dr("postatusdnr"), 0), sptField,
                     FxDB(dr("postatusprt"), 0), sptField,
                     FxDB(dr("postatusrealisasi"), 0), sptField,
                     FxDB(dr("postatus"), 0), sptField,
                     FxDB(dr("postatussebelumnya"), 0), sptField,
                     FxDB(dr("pojmlrevisi"), 0), sptField,
                     FxDB(dr("pocetakanke"), 0), sptField,
                     FxDB(dr("poinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("poinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pomodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pomodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("poposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("popostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("poisclose"), 0), sptField,
                     FxDB(dr("pocabangnama"), ""), sptField,
                     FxDB(dr("polokasinama"), ""), sptField,
                     FxDB(dr("pogudangnama"), ""), sptField,
                     FxDB(dr("posupplierkode"), ""), sptField,
                     FxDB(dr("posuppliernama"), ""), sptField,
                     FxDB(dr("pobagianpembeliankode"), ""), sptField,
                     FxDB(dr("pobagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("postatusnama"), ""), sptField,
                     FxDB(dr("postatussebelumnyanama"), ""), sptField,
                     FxDB(dr("poinputusernama"), ""), sptField,
                     FxDB(dr("pomodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, postatusprt, postatusrealisasi, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, pomodifikasiuser, pomodifikasitgl, poposting, popostingtgl, poisclose, pocabangnama, polokasinama, pogudangnama, posupplierkode, posuppliernama, pobagianpembeliankode, pobagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, postatusnama, postatussebelumnyanama, poinputusernama, pomodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Po_Detail_VSearch(ByVal param As String) As String
        'M4_Po_Detail_VSearch --------------------------------------------------------
        'idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, ponotransaksi, 
        'pouraian, pocatatan, ponoref, potgl, potglnoref, posupplierkontak, po1alamat1, po1alamat2, 
        'po1alamat3, po2alamat1, po2alamat2, po2alamat3, potermin, poterminnama, poterminharijatuhtempo, 
        'pobagianpembelian, pobagianpembeliankode, pobagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, 
        'brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, posupplier, posupplierkode, posuppliernama, 
        'bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pohargatermasukpajak, pocustomtext1, pocustomtext2,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama

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
            Filter = Filter.Replace("idbarang", "pod.idbarang")
            Filter = Filter.Replace("statusrealisasi", "pod.statusrealisasi")
            Filter = Filter.Replace("isclose", "pod.isclose")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_po_detail_v")
        'sql = "select `pod`.`idpodetail` AS `idpodetail`,`pod`.`idpo` AS `idpo`,`pod`.`idbarang` AS `idbarang`,`pod`.`namabarang` AS `namabarang`,`pod`.`tipebarang` AS `tipebarang`,`pod`.`jml` AS `jml`,`pod`.`satuan` AS `satuan`,`pod`.`nilaisatuan` AS `nilaisatuan`,`pod`.`jmlbarang` AS `jmlbarang`,`pod`.`satuanbarang` AS `satuanbarang`,`pod`.`matauang` AS `matauang`,`pod`.`kurs` AS `kurs`,`pod`.`hargafix` AS `hargafix`,`pod`.`harga` AS `harga`,`pod`.`diskon` AS `diskon`,`pod`.`jmldiskon` AS `jmldiskon`,`pod`.`pajak1` AS `pajak1`,`pod`.`jmlpajak1` AS `jmlpajak1`,`pod`.`pajak2` AS `pajak2`,`pod`.`jmlpajak2` AS `jmlpajak2`,`pod`.`cabang` AS `cabang`,`pod`.`lokasi` AS `lokasi`,`pod`.`gudang` AS `gudang`,`pod`.`costcenter` AS `costcenter`,`pod`.`divisi` AS `divisi`,`pod`.`subdivisi` AS `subdivisi`,`pod`.`proyek` AS `proyek`,`pod`.`catatan` AS `catatan`,`pod`.`urutan` AS `urutan`,`pod`.`idprdetail` AS `idprdetail`,`pod`.`idcsdetail` AS `idcsdetail`,`pod`.`idrqdetail` AS `idrqdetail`,`pod`.`idbsdetail` AS `idbsdetail`,`pod`.`jmlipc` AS `jmlipc`,`pod`.`statusipc` AS `statusipc`,`pod`.`jmlgrn` AS `jmlgrn`,`pod`.`statusgrn` AS `statusgrn`,`pod`.`jmlri` AS `jmlri`,`pod`.`statusri` AS `statusri`,`pod`.`jmldnr` AS `jmldnr`,`pod`.`statusdnr` AS `statusdnr`,`pod`.`jmlprt` AS `jmlprt`,`pod`.`statusrealisasi` AS `statusrealisasi`,`pod`.`jmlrealisasi` AS `jmlrealisasi`,`pod`.`statusprt` AS `statusprt`,`pod`.`isclose` AS `isclose`,`pod`.`customtext1` AS `customtext1`,`pod`.`customtext2` AS `customtext2`,`pod`.`customtext3` AS `customtext3`,`pod`.`customdbl1` AS `customdbl1`,`pod`.`customdbl2` AS `customdbl2`,`pod`.`customdbl3` AS `customdbl3`,`pod`.`customdate1` AS `customdate1`,`pod`.`customdate2` AS `customdate2`,`pod`.`customdate3` AS `customdate3`,`po`.`ponotransaksi` AS `ponotransaksi`,`po`.`pouraian` AS `pouraian`,`po`.`pocatatan` AS `pocatatan`,`po`.`ponoref` AS `ponoref`,`po`.`potgl` AS `potgl`,`po`.`potglnoref` AS `potglnoref`,`po`.`posupplierkontak` AS `posupplierkontak`,`po`.`po1alamat1` AS `po1alamat1`,`po`.`po1alamat2` AS `po1alamat2`,`po`.`po1alamat3` AS `po1alamat3`,`po`.`po2alamat1` AS `po2alamat1`,`po`.`po2alamat2` AS `po2alamat2`,`po`.`po2alamat3` AS `po2alamat3`,`po`.`potermin` AS `potermin`,`tr`.`trnama` AS `poterminnama`,`tr`.`trharijatuhtempo` AS `poterminharijatuhtempo`,`po`.`pobagianpembelian` AS `pobagianpembelian`,`c1`.`kkode` AS `pobagianpembeliankode`,`c1`.`knama` AS `pobagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pod`.`jmlbarang` - `pod`.`jmlipc`) / `pod`.`nilaisatuan`) AS `jmlsisaipc`,((`pod`.`jmlbarang` - `pod`.`jmlgrn`) / `pod`.`nilaisatuan`) AS `jmlsisagrn`,((`pod`.`jmlbarang` - `pod`.`jmlri`) / `pod`.`nilaisatuan`) AS `jmlsisari`,((`pod`.`jmlbarang` - `pod`.`jmlrealisasi`) / `pod`.`nilaisatuan`) AS `jmlsisarealisasi`,`po`.`posupplier` AS `posupplier`,`c`.`kkode` AS `posupplierkode`,`c`.`knama` AS `posuppliernama`, i.bjmllapangan, i.bsatuanlapangan, po.pohargatermasukpajak, po.pocustomtext1, po.pocustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from (((((((`m4_po_detail` `pod` join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) left join `m1_terms` `tr` on((`po`.`potermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`po`.`pobagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`pod`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`pod`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pod`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`po`.`posupplier` = `c`.`kid`)))"
        sql = "select `pod`.`idpodetail` AS `idpodetail`,`pod`.`idpo` AS `idpo`,`pod`.`idbarang` AS `idbarang`,`pod`.`namabarang` AS `namabarang`,`pod`.`tipebarang` AS `tipebarang`,`pod`.`jml` AS `jml`,`pod`.`satuan` AS `satuan`,`pod`.`nilaisatuan` AS `nilaisatuan`,`pod`.`jmlbarang` AS `jmlbarang`,`pod`.`satuanbarang` AS `satuanbarang`,`pod`.`matauang` AS `matauang`,`pod`.`kurs` AS `kurs`,`pod`.`hargafix` AS `hargafix`,`pod`.`harga` AS `harga`,`pod`.`diskon` AS `diskon`,`pod`.`jmldiskon` AS `jmldiskon`,`pod`.`pajak1` AS `pajak1`,`pod`.`jmlpajak1` AS `jmlpajak1`,`pod`.`pajak2` AS `pajak2`,`pod`.`jmlpajak2` AS `jmlpajak2`,`pod`.`cabang` AS `cabang`,`pod`.`lokasi` AS `lokasi`,`pod`.`gudang` AS `gudang`,`pod`.`costcenter` AS `costcenter`,`pod`.`divisi` AS `divisi`,`pod`.`subdivisi` AS `subdivisi`,`pod`.`proyek` AS `proyek`,`pod`.`catatan` AS `catatan`,`pod`.`urutan` AS `urutan`,`pod`.`idprdetail` AS `idprdetail`,`pod`.`idcsdetail` AS `idcsdetail`,`pod`.`idrqdetail` AS `idrqdetail`,`pod`.`idbsdetail` AS `idbsdetail`,`pod`.`jmlipc` AS `jmlipc`,`pod`.`statusipc` AS `statusipc`,`pod`.`jmlgrn` AS `jmlgrn`,`pod`.`statusgrn` AS `statusgrn`,`pod`.`jmlri` AS `jmlri`,`pod`.`statusri` AS `statusri`,`pod`.`jmldnr` AS `jmldnr`,`pod`.`statusdnr` AS `statusdnr`,`pod`.`jmlprt` AS `jmlprt`,`pod`.`statusrealisasi` AS `statusrealisasi`,`pod`.`jmlrealisasi` AS `jmlrealisasi`,`pod`.`statusprt` AS `statusprt`,`pod`.`isclose` AS `isclose`,`pod`.`customtext1` AS `customtext1`,`pod`.`customtext2` AS `customtext2`,`pod`.`customtext3` AS `customtext3`,`pod`.`customdbl1` AS `customdbl1`,`pod`.`customdbl2` AS `customdbl2`,`pod`.`customdbl3` AS `customdbl3`,`pod`.`customdate1` AS `customdate1`,`pod`.`customdate2` AS `customdate2`,`pod`.`customdate3` AS `customdate3`,`po`.`ponotransaksi` AS `ponotransaksi`,`po`.`pouraian` AS `pouraian`,`po`.`pocatatan` AS `pocatatan`,`po`.`ponoref` AS `ponoref`,`po`.`potgl` AS `potgl`,`po`.`potglnoref` AS `potglnoref`,`po`.`posupplierkontak` AS `posupplierkontak`,`po`.`po1alamat1` AS `po1alamat1`,`po`.`po1alamat2` AS `po1alamat2`,`po`.`po1alamat3` AS `po1alamat3`,`po`.`po2alamat1` AS `po2alamat1`,`po`.`po2alamat2` AS `po2alamat2`,`po`.`po2alamat3` AS `po2alamat3`,`po`.`potermin` AS `potermin`,`tr`.`trnama` AS `poterminnama`,`tr`.`trharijatuhtempo` AS `poterminharijatuhtempo`,`po`.`pobagianpembelian` AS `pobagianpembelian`,`c1`.`kkode` AS `pobagianpembeliankode`,`c1`.`knama` AS `pobagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pod`.`jmlbarang` - `pod`.`jmlipc`) / `pod`.`nilaisatuan`) AS `jmlsisaipc`,((`pod`.`jmlbarang` - `pod`.`jmlgrn`) / `pod`.`nilaisatuan`) AS `jmlsisagrn`,((`pod`.`jmlbarang` - `pod`.`jmlri`) / `pod`.`nilaisatuan`) AS `jmlsisari`,((`pod`.`jmlbarang` - `pod`.`jmlrealisasi`) / `pod`.`nilaisatuan`) AS `jmlsisarealisasi`,`po`.`posupplier` AS `posupplier`,`c`.`kkode` AS `posupplierkode`,`c`.`knama` AS `posuppliernama`, i.bjmllapangan, i.bsatuanlapangan, po.pohargatermasukpajak, po.pocustomtext1, po.pocustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama , d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama from `m4_po_detail` `pod` join `m4_po` `po` on `pod`.`idpo` = `po`.`poid` left join `m1_terms` `tr` on `po`.`potermin` = `tr`.`trkode` left join `m1_contact` `c1` on `po`.`pobagianpembelian` = `c1`.`kid` left join `m1_item` `i` on `pod`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `pod`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `pod`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c` on `po`.`posupplier` = `c`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = pod.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = pod.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = pod.costcenter LEFT JOIN m1_project p ON p.pkode = pod.proyek"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idpo"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargafix"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("pouraian"), ""), sptField,
                     FxDB(dr("pocatatan"), ""), sptField,
                     FxDB(dr("ponoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("potglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("posupplierkontak"), ""), sptField,
                     FxDB(dr("po1alamat1"), ""), sptField,
                     FxDB(dr("po1alamat2"), ""), sptField,
                     FxDB(dr("po1alamat3"), ""), sptField,
                     FxDB(dr("po2alamat1"), ""), sptField,
                     FxDB(dr("po2alamat2"), ""), sptField,
                     FxDB(dr("po2alamat3"), ""), sptField,
                     FxDB(dr("potermin"), ""), sptField,
                     FxDB(dr("poterminnama"), ""), sptField,
                     FxDB(dr("poterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("pobagianpembelian"), 0), sptField,
                     FxDB(dr("pobagianpembeliankode"), ""), sptField,
                     FxDB(dr("pobagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekdiskonpembelian"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisaipc"), 0), sptField,
                     FxDB(dr("jmlsisagrn"), 0), sptField,
                     FxDB(dr("jmlsisari"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("posupplier"), ""), sptField,
                     FxDB(dr("posupplierkode"), ""), sptField,
                     FxDB(dr("posuppliernama"), ""), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("pohargatermasukpajak"), 0), sptField,
                     FxDB(dr("pocustomtext1"), ""), sptField,
                     FxDB(dr("pocustomtext2"), ""), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, ponotransaksi, pouraian, pocatatan, ponoref, potgl, potglnoref, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, potermin, poterminnama, poterminharijatuhtempo, pobagianpembelian, pobagianpembeliankode, pobagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, posupplier, posupplierkode, posuppliernama, bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pohargatermasukpajak, pocustomtext1, pocustomtext2, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, divisinama, subdivisinama, costcenternama, proyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Po_Detail_Cost(ByVal param As String) As String
        'M4_Po_Detail_Cost --------------------------------------------------------
        'Detail
        'idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, ponotransaksi, 
        'pouraian, pocatatan, ponoref, potgl, potglnoref, posupplierkontak, po1alamat1, po1alamat2, 
        'po1alamat3, po2alamat1, po2alamat2, po2alamat3, potermin, poterminnama, poterminharijatuhtempo, 
        'pobagianpembelian, pobagianpembeliankode, pobagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, 
        'brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, posupplier, posupplierkode, posuppliernama, 
        'bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pohargatermasukpajak, pocustomtext1, pocustomtext2,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama

        'Cost
        'idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, 
        'kontaknama, costcenternama, divisinama, subdivisinama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", cost As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter1 As String = "", Sorting1 As String = "", Filter2 As String = "", Sorting2 As String = ""
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

        'FILTER DIBAGI MENJADI 2, DETAIL DAN COST
        'VALIDASI PAGING KHUSUS, FILTER DAN SORTING UNTUK 2 TABEL
        Dim filterSplit(2) As String, sortingSplit(2) As String

        filterSplit = pagingSplit(2).Split(sptRow)
        If (filterSplit.Length <> 2) Then
            result(2) = "Invalid filter parameter." : GoTo selesai
        End If
        'Replace disesuaikan dengan kebutuhan
        If (filterSplit(0).Length > 0) Then
            Filter1 = filterSplit(0)
            '#Taruh fungsi replace disini...
            Filter1 = Filter1.Replace("idbarang", "pod.idbarang")
            Filter1 = Filter1.Replace("statusrealisasi", "pod.statusrealisasi")
            Filter1 = Filter1.Replace("isclose", "pod.isclose")
        End If
        If (filterSplit(1).Length > 0) Then
            Filter2 = filterSplit(1)
            '#Taruh fungsi replace disini...
        End If

        sortingSplit = pagingSplit(3).Split(sptRow)
        If (sortingSplit.Length <> 2) Then
            result(2) = "Invalid sorting parameter." : GoTo selesai
        End If
        If (sortingSplit(0).Length > 0) Then
            Sorting1 = sortingSplit(0)
            '#Taruh fungsi replace disini...
        End If
        If (sortingSplit(1).Length > 0) Then
            Sorting2 = sortingSplit(1)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = "select `pod`.`idpodetail` AS `idpodetail`,`pod`.`idpo` AS `idpo`,`pod`.`idbarang` AS `idbarang`,`pod`.`namabarang` AS `namabarang`,`pod`.`tipebarang` AS `tipebarang`,`pod`.`jml` AS `jml`,`pod`.`satuan` AS `satuan`,`pod`.`nilaisatuan` AS `nilaisatuan`,CONCAT(`pod`.`jmlbarang`, '#$') AS `jmlbarang`,`pod`.`satuanbarang` AS `satuanbarang`,`pod`.`matauang` AS `matauang`,`pod`.`kurs` AS `kurs`,`pod`.`hargafix` AS `hargafix`,`pod`.`harga` AS `harga`,`pod`.`diskon` AS `diskon`,`pod`.`jmldiskon` AS `jmldiskon`,`pod`.`pajak1` AS `pajak1`,`pod`.`jmlpajak1` AS `jmlpajak1`,`pod`.`pajak2` AS `pajak2`,`pod`.`jmlpajak2` AS `jmlpajak2`,`pod`.`cabang` AS `cabang`,`pod`.`lokasi` AS `lokasi`,`pod`.`gudang` AS `gudang`,`pod`.`costcenter` AS `costcenter`,`pod`.`divisi` AS `divisi`,`pod`.`subdivisi` AS `subdivisi`,`pod`.`proyek` AS `proyek`,`pod`.`catatan` AS `catatan`,`pod`.`urutan` AS `urutan`,`pod`.`idprdetail` AS `idprdetail`,`pod`.`idcsdetail` AS `idcsdetail`,`pod`.`idrqdetail` AS `idrqdetail`,`pod`.`idbsdetail` AS `idbsdetail`,`pod`.`jmlipc` AS `jmlipc`,`pod`.`statusipc` AS `statusipc`,`pod`.`jmlgrn` AS `jmlgrn`,`pod`.`statusgrn` AS `statusgrn`,`pod`.`jmlri` AS `jmlri`,`pod`.`statusri` AS `statusri`,`pod`.`jmldnr` AS `jmldnr`,`pod`.`statusdnr` AS `statusdnr`,`pod`.`jmlprt` AS `jmlprt`,`pod`.`statusrealisasi` AS `statusrealisasi`,`pod`.`jmlrealisasi` AS `jmlrealisasi`,`pod`.`statusprt` AS `statusprt`,`pod`.`isclose` AS `isclose`,`pod`.`customtext1` AS `customtext1`,`pod`.`customtext2` AS `customtext2`,`pod`.`customtext3` AS `customtext3`,`pod`.`customdbl1` AS `customdbl1`,`pod`.`customdbl2` AS `customdbl2`,`pod`.`customdbl3` AS `customdbl3`,`pod`.`customdate1` AS `customdate1`,`pod`.`customdate2` AS `customdate2`,`pod`.`customdate3` AS `customdate3`,`po`.`ponotransaksi` AS `ponotransaksi`,`po`.`pouraian` AS `pouraian`,`po`.`pocatatan` AS `pocatatan`,`po`.`ponoref` AS `ponoref`,`po`.`potgl` AS `potgl`,`po`.`potglnoref` AS `potglnoref`,`po`.`posupplierkontak` AS `posupplierkontak`,`po`.`po1alamat1` AS `po1alamat1`,`po`.`po1alamat2` AS `po1alamat2`,`po`.`po1alamat3` AS `po1alamat3`,`po`.`po2alamat1` AS `po2alamat1`,`po`.`po2alamat2` AS `po2alamat2`,`po`.`po2alamat3` AS `po2alamat3`,`po`.`potermin` AS `potermin`,`tr`.`trnama` AS `poterminnama`,`tr`.`trharijatuhtempo` AS `poterminharijatuhtempo`,`po`.`pobagianpembelian` AS `pobagianpembelian`,`c1`.`kkode` AS `pobagianpembeliankode`,`c1`.`knama` AS `pobagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pod`.`jmlbarang` - `pod`.`jmlipc`) / `pod`.`nilaisatuan`) AS `jmlsisaipc`,((`pod`.`jmlbarang` - `pod`.`jmlgrn`) / `pod`.`nilaisatuan`) AS `jmlsisagrn`,((`pod`.`jmlbarang` - `pod`.`jmlri`) / `pod`.`nilaisatuan`) AS `jmlsisari`, CONCAT(((`pod`.`jmlbarang` - `pod`.`jmlrealisasi`) / `pod`.`nilaisatuan`), '#$') AS `jmlsisarealisasi`,`po`.`posupplier` AS `posupplier`,`c`.`kkode` AS `posupplierkode`,`c`.`knama` AS `posuppliernama`, i.bjmllapangan, i.bsatuanlapangan from (((((((`m4_po_detail` `pod` left join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) left join `m1_terms` `tr` on((`po`.`potermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`po`.`pobagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`pod`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`pod`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pod`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`po`.`posupplier` = `c`.`kid`)))"
        'sql = " select `pod`.`idpodetail` AS `idpodetail`,`pod`.`idpo` AS `idpo`,`pod`.`idbarang` AS `idbarang`,`pod`.`namabarang` AS `namabarang`,`pod`.`tipebarang` AS `tipebarang`,`pod`.`jml` AS `jml`,`pod`.`satuan` AS `satuan`,`pod`.`nilaisatuan` AS `nilaisatuan`,`pod`.`jmlbarang` AS `jmlbarang`,`pod`.`satuanbarang` AS `satuanbarang`,`pod`.`matauang` AS `matauang`,`pod`.`kurs` AS `kurs`,`pod`.`hargafix` AS `hargafix`,`pod`.`harga` AS `harga`,`pod`.`diskon` AS `diskon`,`pod`.`jmldiskon` AS `jmldiskon`,`pod`.`pajak1` AS `pajak1`,`pod`.`jmlpajak1` AS `jmlpajak1`,`pod`.`pajak2` AS `pajak2`,`pod`.`jmlpajak2` AS `jmlpajak2`,`pod`.`cabang` AS `cabang`,`pod`.`lokasi` AS `lokasi`,`pod`.`gudang` AS `gudang`,`pod`.`costcenter` AS `costcenter`,`pod`.`divisi` AS `divisi`,`pod`.`subdivisi` AS `subdivisi`,`pod`.`proyek` AS `proyek`,`pod`.`catatan` AS `catatan`,`pod`.`urutan` AS `urutan`,`pod`.`idprdetail` AS `idprdetail`,`pod`.`idcsdetail` AS `idcsdetail`,`pod`.`idrqdetail` AS `idrqdetail`,`pod`.`idbsdetail` AS `idbsdetail`,`pod`.`jmlipc` AS `jmlipc`,`pod`.`statusipc` AS `statusipc`,`pod`.`jmlgrn` AS `jmlgrn`,`pod`.`statusgrn` AS `statusgrn`,`pod`.`jmlri` AS `jmlri`,`pod`.`statusri` AS `statusri`,`pod`.`jmldnr` AS `jmldnr`,`pod`.`statusdnr` AS `statusdnr`,`pod`.`jmlprt` AS `jmlprt`,`pod`.`statusrealisasi` AS `statusrealisasi`,`pod`.`jmlrealisasi` AS `jmlrealisasi`,`pod`.`statusprt` AS `statusprt`,`pod`.`isclose` AS `isclose`,`pod`.`customtext1` AS `customtext1`,`pod`.`customtext2` AS `customtext2`,`pod`.`customtext3` AS `customtext3`,`pod`.`customdbl1` AS `customdbl1`,`pod`.`customdbl2` AS `customdbl2`,`pod`.`customdbl3` AS `customdbl3`,`pod`.`customdate1` AS `customdate1`,`pod`.`customdate2` AS `customdate2`,`pod`.`customdate3` AS `customdate3`,`po`.`ponotransaksi` AS `ponotransaksi`,`po`.`pouraian` AS `pouraian`,`po`.`pocatatan` AS `pocatatan`,`po`.`ponoref` AS `ponoref`,`po`.`potgl` AS `potgl`,`po`.`potglnoref` AS `potglnoref`,`po`.`posupplierkontak` AS `posupplierkontak`,`po`.`po1alamat1` AS `po1alamat1`,`po`.`po1alamat2` AS `po1alamat2`,`po`.`po1alamat3` AS `po1alamat3`,`po`.`po2alamat1` AS `po2alamat1`,`po`.`po2alamat2` AS `po2alamat2`,`po`.`po2alamat3` AS `po2alamat3`,`po`.`potermin` AS `potermin`,`tr`.`trnama` AS `poterminnama`,`tr`.`trharijatuhtempo` AS `poterminharijatuhtempo`,`po`.`pobagianpembelian` AS `pobagianpembelian`,`c1`.`kkode` AS `pobagianpembeliankode`,`c1`.`knama` AS `pobagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pod`.`jmlbarang` - `pod`.`jmlipc`) / `pod`.`nilaisatuan`) AS `jmlsisaipc`,((`pod`.`jmlbarang` - `pod`.`jmlgrn`) / `pod`.`nilaisatuan`) AS `jmlsisagrn`,((`pod`.`jmlbarang` - `pod`.`jmlri`) / `pod`.`nilaisatuan`) AS `jmlsisari`,((`pod`.`jmlbarang` - `pod`.`jmlrealisasi`) / `pod`.`nilaisatuan`) AS `jmlsisarealisasi`,`po`.`posupplier` AS `posupplier`,`c`.`kkode` AS `posupplierkode`,`c`.`knama` AS `posuppliernama`, i.bjmllapangan, i.bsatuanlapangan, po.pohargatermasukpajak, po.pocustomtext1, po.pocustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from (((((((`m4_po_detail` `pod` join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) left join `m1_terms` `tr` on((`po`.`potermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`po`.`pobagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`pod`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`pod`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pod`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`po`.`posupplier` = `c`.`kid`)))"
        sql = "select `pod`.`idpodetail` AS `idpodetail`,`pod`.`idpo` AS `idpo`,`pod`.`idbarang` AS `idbarang`,`pod`.`namabarang` AS `namabarang`,`pod`.`tipebarang` AS `tipebarang`,`pod`.`jml` AS `jml`,`pod`.`satuan` AS `satuan`,`pod`.`nilaisatuan` AS `nilaisatuan`,`pod`.`jmlbarang` AS `jmlbarang`,`pod`.`satuanbarang` AS `satuanbarang`,`pod`.`matauang` AS `matauang`,`pod`.`kurs` AS `kurs`,`pod`.`hargafix` AS `hargafix`,`pod`.`harga` AS `harga`,`pod`.`diskon` AS `diskon`,`pod`.`jmldiskon` AS `jmldiskon`,`pod`.`pajak1` AS `pajak1`,`pod`.`jmlpajak1` AS `jmlpajak1`,`pod`.`pajak2` AS `pajak2`,`pod`.`jmlpajak2` AS `jmlpajak2`,`pod`.`cabang` AS `cabang`,`pod`.`lokasi` AS `lokasi`,`pod`.`gudang` AS `gudang`,`pod`.`costcenter` AS `costcenter`,`pod`.`divisi` AS `divisi`,`pod`.`subdivisi` AS `subdivisi`,`pod`.`proyek` AS `proyek`,`pod`.`catatan` AS `catatan`,`pod`.`urutan` AS `urutan`,`pod`.`idprdetail` AS `idprdetail`,`pod`.`idcsdetail` AS `idcsdetail`,`pod`.`idrqdetail` AS `idrqdetail`,`pod`.`idbsdetail` AS `idbsdetail`,`pod`.`jmlipc` AS `jmlipc`,`pod`.`statusipc` AS `statusipc`,`pod`.`jmlgrn` AS `jmlgrn`,`pod`.`statusgrn` AS `statusgrn`,`pod`.`jmlri` AS `jmlri`,`pod`.`statusri` AS `statusri`,`pod`.`jmldnr` AS `jmldnr`,`pod`.`statusdnr` AS `statusdnr`,`pod`.`jmlprt` AS `jmlprt`,`pod`.`statusrealisasi` AS `statusrealisasi`,`pod`.`jmlrealisasi` AS `jmlrealisasi`,`pod`.`statusprt` AS `statusprt`,`pod`.`isclose` AS `isclose`,`pod`.`customtext1` AS `customtext1`,`pod`.`customtext2` AS `customtext2`,`pod`.`customtext3` AS `customtext3`,`pod`.`customdbl1` AS `customdbl1`,`pod`.`customdbl2` AS `customdbl2`,`pod`.`customdbl3` AS `customdbl3`,`pod`.`customdate1` AS `customdate1`,`pod`.`customdate2` AS `customdate2`,`pod`.`customdate3` AS `customdate3`,`po`.`ponotransaksi` AS `ponotransaksi`,`po`.`pouraian` AS `pouraian`,`po`.`pocatatan` AS `pocatatan`,`po`.`ponoref` AS `ponoref`,`po`.`potgl` AS `potgl`,`po`.`potglnoref` AS `potglnoref`,`po`.`posupplierkontak` AS `posupplierkontak`,`po`.`po1alamat1` AS `po1alamat1`,`po`.`po1alamat2` AS `po1alamat2`,`po`.`po1alamat3` AS `po1alamat3`,`po`.`po2alamat1` AS `po2alamat1`,`po`.`po2alamat2` AS `po2alamat2`,`po`.`po2alamat3` AS `po2alamat3`,`po`.`potermin` AS `potermin`,`tr`.`trnama` AS `poterminnama`,`tr`.`trharijatuhtempo` AS `poterminharijatuhtempo`,`po`.`pobagianpembelian` AS `pobagianpembelian`,`c1`.`kkode` AS `pobagianpembeliankode`,`c1`.`knama` AS `pobagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pod`.`jmlbarang` - `pod`.`jmlipc`) / `pod`.`nilaisatuan`) AS `jmlsisaipc`,((`pod`.`jmlbarang` - `pod`.`jmlgrn`) / `pod`.`nilaisatuan`) AS `jmlsisagrn`,((`pod`.`jmlbarang` - `pod`.`jmlri`) / `pod`.`nilaisatuan`) AS `jmlsisari`,((`pod`.`jmlbarang` - `pod`.`jmlrealisasi`) / `pod`.`nilaisatuan`) AS `jmlsisarealisasi`,`po`.`posupplier` AS `posupplier`,`c`.`kkode` AS `posupplierkode`,`c`.`knama` AS `posuppliernama`, i.bjmllapangan, i.bsatuanlapangan, po.pohargatermasukpajak, po.pocustomtext1, po.pocustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from `m4_po_detail` `pod` join `m4_po` `po` on `pod`.`idpo` = `po`.`poid` left join `m1_terms` `tr` on `po`.`potermin` = `tr`.`trkode` left join `m1_contact` `c1` on `po`.`pobagianpembelian` = `c1`.`kid` left join `m1_item` `i` on `pod`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `pod`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `pod`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c` on `po`.`posupplier` = `c`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Po_Detail", Filter1, Sorting1, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idpo"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargafix"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("jmlipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jmlgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jmlri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("pouraian"), ""), sptField,
                     FxDB(dr("pocatatan"), ""), sptField,
                     FxDB(dr("ponoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("potglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("posupplierkontak"), ""), sptField,
                     FxDB(dr("po1alamat1"), ""), sptField,
                     FxDB(dr("po1alamat2"), ""), sptField,
                     FxDB(dr("po1alamat3"), ""), sptField,
                     FxDB(dr("po2alamat1"), ""), sptField,
                     FxDB(dr("po2alamat2"), ""), sptField,
                     FxDB(dr("po2alamat3"), ""), sptField,
                     FxDB(dr("potermin"), ""), sptField,
                     FxDB(dr("poterminnama"), ""), sptField,
                     FxDB(dr("poterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("pobagianpembelian"), 0), sptField,
                     FxDB(dr("pobagianpembeliankode"), ""), sptField,
                     FxDB(dr("pobagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekdiskonpembelian"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisaipc"), 0), sptField,
                     FxDB(dr("jmlsisagrn"), 0), sptField,
                     FxDB(dr("jmlsisari"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("posupplier"), ""), sptField,
                     FxDB(dr("posupplierkode"), ""), sptField,
                     FxDB(dr("posuppliernama"), ""), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("pohargatermasukpajak"), 0), sptField,
                     FxDB(dr("pocustomtext1"), ""), sptField,
                     FxDB(dr("pocustomtext2"), ""), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            'AMBIL DATA COST
            sql = "SELECT poc.idpocost, poc.idpo, poc.kodecost, poc.matauang, poc.kurs, poc.jumlah, poc.rekdebit, poc.rekkredit, poc.kontak, poc.termasukhpp, poc.catatan, poc.costcenter, poc.divisi, poc.subdivisi, poc.proyek, poc.urutan, poc.idprcost, poc.idcscost, poc.idrqcost, poc.idbscost, poc.jumlahipc, poc.statusipc, poc.jumlahgrn, poc.statusgrn, poc.jumlahri, poc.statusri, poc.jumlahbayar, poc.statusbayar, poc.isclose, poc.customtext1, poc.customtext2, poc.customtext3, poc.customdbl1, poc.customdbl2, poc.customdbl3, poc.customdate1, poc.customdate2, poc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama,  c.kkode as kontakkode, c.knama as kontaknama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sddivisi as subdivisinama FROM m4_po_cost poc JOIN m4_po po ON poc.idpo = po.poid LEFT JOIN m1_other_cost oc ON poc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON poc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON poc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON poc.kontak = c.kid LEFT JOIN m1_cost_center cc ON poc.costcenter = cc.cckode LEFT JOIN m1_division d ON poc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON poc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_po_cost", Filter2, Sorting2, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idpocost"), ""), sptField,
                     FxDB(dr("idpo"), ""), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("kontak"), ""), sptField,
                     FxDB(dr("termasukhpp"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), ""), sptField,
                     FxDB(dr("idcscost"), ""), sptField,
                     FxDB(dr("idrqcost"), ""), sptField,
                     FxDB(dr("idbscost"), ""), sptField,
                     FxDB(dr("jumlahipc"), 0), sptField,
                     FxDB(dr("statusipc"), 0), sptField,
                     FxDB(dr("jumlahgrn"), 0), sptField,
                     FxDB(dr("statusgrn"), 0), sptField,
                     FxDB(dr("jumlahri"), 0), sptField,
                     FxDB(dr("statusri"), 0), sptField,
                     FxDB(dr("jumlahbayar"), 0), sptField,
                     FxDB(dr("statusbayar"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodecostnama"), ""), sptField,
                     FxDB(dr("rekdebitnama"), ""), sptField,
                     FxDB(dr("rekkreditnama"), ""), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost

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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, cost)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, ponotransaksi, pouraian, pocatatan, ponoref, potgl, potglnoref, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, potermin, poterminnama, poterminharijatuhtempo, pobagianpembelian, pobagianpembeliankode, pobagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, posupplier, posupplierkode, posuppliernama, bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pohargatermasukpajak, pocustomtext1, pocustomtext2, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama" & sptSubParam & "idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PoTerkait(ByVal param As String) As String
        'M4_PoTerkait --------------------------------------------------------
        'poid, ponotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

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
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "poid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_po_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("poid"), 0), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idterkait"), 0), sptField,
                     FxDB(dr("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related PO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("poid, ponotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingPR As String, ByVal ftOutstandingPR As String, ByVal ftExistOutstandingRQ As String, ByVal ftOutstandingRQ As String, ByVal ftRQ As String, ByVal termasukPajak As String, ByVal ftExistOutstandingPro As String, ByVal ftOutstandingPro As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = ""

        If Len(ftExistOutstandingPro) > 0 Then 'ftExistOutstanding = rowExists, proyek
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPro)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("proyek")

                filterLookup = "proyek='" & dtval.Rows(0)("proyek") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " doesn't exists in Project Data" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT p.pkode as proyek, (p.pnilaikontrak - p.prealisasikontrak) as sisapr FROM m1_project p WHERE pnilaikontrak > 0 AND (" & ftOutstandingPro & ")"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("proyek")
                sisa = dtval.Rows(0)("sisapr")

                filterLookup = "proyek='" & dtval.Rows(0)("proyek") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("matauang")
                    nilaiSatuan = dtLookup.Rows(0)("kurs")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " exceeds the amount of Project, outstanding available " & FormatNumber(sisa / nilaiSatuan) & " " & satuan : GoTo selesai
            End If
        End If

        'VALIDASI OUTSTANDING ---------------------------------------
        'PR
        If Len(ftExistOutstandingPR) > 0 Then 'ftExistOutstanding = rowExists, idprdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPR)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idprdetail=" & dtval.Rows(0)("idprdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PR" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT prd.idprdetail, (prd.jmlbarang - prd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m4_pr_detail AS prd INNER JOIN m1_item AS i ON prd.idbarang = i.bid WHERE " & ftOutstandingPR
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then

                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idprdetail=" & dtval.Rows(0)("idprdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PR, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If

        'RQ
        If Len(ftExistOutstandingRQ) > 0 Then 'ftExistOutstanding = rowExists, idrqdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingRQ)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idrqdetail=" & dtval.Rows(0)("idrqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in RQ" : GoTo selesai
            End If

            'CEK RQ YANG DIAMBIL
            'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            If Len(ftRQ) > 0 Then
                sql = "SELECT rq.rqnotransaksi as notransaksi, (CASE rq.rqhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_rq_detail rqd JOIN m4_rq rq ON rqd.idrq = rq.rqid WHERE " & ftRQ & " GROUP BY rq.rqhargatermasukpajak"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 1 Then
                    errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                    For Each dr1 As DataRow In dtval.Rows
                        errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajak")
                    Next
                    GoTo selesai
                End If

                'CEK TRANSAKSI HARGA TERMASUK PAJAK TIDAK BOLEH AMBIL TRANSAKSI HARGA TIDAK TERMASUK PAJAK, DAN SEBALIKNYA
                If Len(termasukPajak) > 0 Then
                    sql = "SELECT i.bkode, rqd.idrqdetail, rq.rqnotransaksi as notransaksi, (CASE rq.rqhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_rq_detail rqd JOIN m4_rq rq ON rqd.idrq = rq.rqid JOIN m1_item i ON rqd.idbarang = i.bid WHERE (" & ftRQ & ") AND rq.rqhargatermasukpajak <> " & termasukPajak & " ORDER BY rqd.urutan"
                    dtval = AsDataTableAmbilDariDB(sql)
                    If dtval.Rows.Count > 0 Then
                        'Ambil informasi utk errmessage
                        kodebarang = dtval.Rows(0)("bkode")

                        filterLookup = "idrqdetail = " & dtval.Rows(0)("idrqdetail")
                        dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                        If dtLookup.Rows.Count > 0 Then
                            tipebarang = dtLookup.Rows(0)("tipebarang")
                            namabarang = dtLookup.Rows(0)("namabarang")
                            urutan = dtLookup.Rows(0)("urutan")
                        End If
                        errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & ". " & dtval.Rows(0)("notransaksi") & " " & dtval.Rows(0)("termasukpajak") : GoTo selesai
                    End If
                End If

            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT rqd.idrqdetail, (rqd.jmlbarang - rqd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m4_rq_detail AS rqd INNER JOIN m1_item AS i ON rqd.idbarang = i.bid WHERE " & ftOutstandingRQ
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idrqdetail=" & dtval.Rows(0)("idrqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in RQ, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_PoSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataCost(), dataRowCost() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'poid(0) As Integer, pocabang(1) As String, polokasi(2) As String, pogudang(3) As String, poasalbarang(4) As String, 
        'poasalbarangkategori(5) As Integer, pojenispembelian(6) As String, pojenispembeliankategori(7) As Integer, pocarabayar(8) As Integer, posumber(9) As String, 
        'poautonotransaksi(10) As Integer, ponotransaksi(11) As String, potgl(12) As Date, pokodepa(13) As Integer, posupplier(14) As Integer, 
        'posupplierkontak(15) As String, po1alamat1(16) As String, po1alamat2(17) As String, po1alamat3(18) As String, po2alamat1(19) As String, 
        'po2alamat2(20) As String, po2alamat3(21) As String, pobagianpembelian(22) As Integer, potgldipenuhi(23) As Date, potermin(24) As String, 
        'potgljatuhtempo(25) As Date, pouraian(26) As String, pocatatan(27) As String, ponoref(28) As String, potglnoref(29) As Date, 
        'potglpenutupan(30) As Date, pomatauang(31) As String, pokurs(32) As Double, pohargatermasukpajak(33) As Integer, pototal(34) As Double, 
        'podiskonpersen(35) As String, pojmldiskon(36) As Double, pototalpajak1detail(37) As Double, pototalpajak2detail(38) As Double, pobiayalainpersen(39) As String, 
        'pobiayalain(40) As Double, pototaltransaksi(41) As Double, pojmlbayar(42) As Double, porekdiskon(43) As String, porekpajak1(44) As String, 
        'porekpajak2(45) As String, porekbiayalain(46) As String, porekbayar(47) As String, poidpr(48) As Integer, poidcs(49) As Integer, 
        'poidrq(50) As Integer, poidbs(51) As Integer, postatusipc(52) As Integer, postatusgrn(53) As Integer, postatusri(54) As Integer, 
        'postatusdnr(55) As Integer, postatusprt(56) As Integer, postatus(57) As Integer, postatussebelumnya(58) As Integer, pojmlrevisi(59) As Integer, 
        'pocetakanke(60) As Integer, poinputuser(61) As Integer, poinputtgl(62) As DateTime, pomodifikasiuser(63) As Integer, pomodifikasitgl(64) As DateTime, 
        'poisclose(65) As Integer, pocustomtext1(66) As String, pocustomtext2(67) As String, pocustomtext3(68) As String, pocustomtext4(69) As String, 
        'pocustomtext5(70) As String, pocustomint1(71) As Integer, pocustomint2(72) As Integer, pocustomint3(73) As Integer, pocustomdbl1(74) As Double, 
        'pocustomdbl2(75) As Double, pocustomdbl3(76) As Double, pocustomdate1(77) As Date, pocustomdate2(78) As Date, pocustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'poid, pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, 
        'pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, 
        'posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, 
        'po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, 
        'ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, 
        'podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, 
        'pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, 
        'poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, 
        'postatusprt, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, 
        'pomodifikasiuser, pomodifikasitgl, poisclose, pocustomtext1, pocustomtext2, pocustomtext3, pocustomtext4, 
        'pocustomtext5, pocustomint1, pocustomint2, pocustomint3, pocustomdbl1, pocustomdbl2, pocustomdbl3, 
        'pocustomdate1, pocustomdate2, pocustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'poid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "poid required numeric." : GoTo selesai
        End If
        'poasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "poasalbarangkategori required numeric." : GoTo selesai
        End If
        'pojenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pojenispembeliankategori required numeric." : GoTo selesai
        End If
        'pocarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pocarabayar required numeric." : GoTo selesai
        End If
        'poautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "poautonotransaksi required numeric." : GoTo selesai
        End If
        'potgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "potgl required date." : GoTo selesai
        End If
        'pokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pokodepa required numeric." : GoTo selesai
        End If
        'posupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "posupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "posupplier can't be empty." : GoTo selesai
        End If
        'pobagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pobagianpembelian required numeric." : GoTo selesai
        End If
        'potgldipenuhi(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "potgldipenuhi required date." : GoTo selesai
        End If
        'potgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "potgljatuhtempo required date." : GoTo selesai
        End If
        'potglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "potglnoref required date." : GoTo selesai
        End If
        'potglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "potglpenutupan required date." : GoTo selesai
        End If
        'pokurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pokurs required numeric." : GoTo selesai
        End If
        'pohargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pohargatermasukpajak required numeric." : GoTo selesai
        End If
        'pototal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pototal required numeric." : GoTo selesai
        End If
        'pojmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pojmldiskon required numeric." : GoTo selesai
        End If
        'pototalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pototalpajak1detail required numeric." : GoTo selesai
        End If
        'pototalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pototalpajak2detail required numeric." : GoTo selesai
        End If
        'pobiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pobiayalain required numeric." : GoTo selesai
        End If
        'pototaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pototaltransaksi required numeric." : GoTo selesai
        End If
        'pojmlbayar(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pojmlbayar required numeric." : GoTo selesai
        End If
        'poidpr(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "poidpr required numeric." : GoTo selesai
        End If
        'poidcs(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "poidcs required numeric." : GoTo selesai
        End If
        'poidrq(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "poidrq required numeric." : GoTo selesai
        End If
        'poidbs(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "poidbs required numeric." : GoTo selesai
        End If
        'postatusipc(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "postatusipc required numeric." : GoTo selesai
        End If
        'postatusgrn(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "postatusgrn required numeric." : GoTo selesai
        End If
        'postatusri(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "postatusri required numeric." : GoTo selesai
        End If
        'postatusdnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "postatusdnr required numeric." : GoTo selesai
        End If
        'postatusprt(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "postatusprt required numeric." : GoTo selesai
        End If
        'postatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "postatus required numeric." : GoTo selesai
        End If
        'postatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "postatussebelumnya required numeric." : GoTo selesai
        End If
        'pojmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "pojmlrevisi required numeric." : GoTo selesai
        End If
        'pocetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "pocetakanke required numeric." : GoTo selesai
        End If
        'poinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "poinputuser required numeric." : GoTo selesai
        End If
        'poinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "poinputtgl required date." : GoTo selesai
        End If
        'pomodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "pomodifikasiuser required numeric." : GoTo selesai
        End If
        'pomodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "pomodifikasitgl required date." : GoTo selesai
        End If
        'poisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "poisclose required numeric." : GoTo selesai
        End If
        'pocustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "pocustomint1 required numeric." : GoTo selesai
        End If
        'pocustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "pocustomint2 required numeric." : GoTo selesai
        End If
        'pocustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "pocustomint3 required numeric." : GoTo selesai
        End If
        'pocustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "pocustomdbl1 required numeric." : GoTo selesai
        End If
        'pocustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "pocustomdbl2 required numeric." : GoTo selesai
        End If
        'pocustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "pocustomdbl3 required numeric." : GoTo selesai
        End If
        'pocustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "pocustomdate1 required date." : GoTo selesai
        End If
        'pocustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "pocustomdate2 required date." : GoTo selesai
        End If
        'pocustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "pocustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pocabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pocabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pocabang should not be more than 25 character." : GoTo selesai
        End If

        'polokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "polokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "polokasi should not be more than 25 character." : GoTo selesai
        End If

        'pogudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pogudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pogudang should not be more than 25 character." : GoTo selesai
        End If

        'posumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "posumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "posumber should not be more than 10 character." : GoTo selesai
        End If

        'ponotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "ponotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "ponotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'potgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "potgl can't be empty" : GoTo selesai
        End If

        'potgldipenuhi(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "potgldipenuhi can't be empty" : GoTo selesai
        End If

        'potgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "potgljatuhtempo can't be empty" : GoTo selesai
        End If

        'potglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "potglnoref can't be empty" : GoTo selesai
        End If

        'potglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "potglpenutupan can't be empty" : GoTo selesai
        End If

        'pomatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pomatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "pomatauang should not be more than 25 character." : GoTo selesai
        End If

        'pokurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pokurs can't be empty" : GoTo selesai
        End If

        'pototal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "pototal can't be empty" : GoTo selesai
        End If

        'podiskonpersen(35) As String
        If Len(dataUtama(35)) = 0 Then
            result(2) = "podiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "podiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pojmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pojmldiskon can't be empty" : GoTo selesai
        End If

        'pototalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pototalpajak1detail can't be empty" : GoTo selesai
        End If

        'pototalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pototalpajak2detail can't be empty" : GoTo selesai
        End If

        'pobiayalainpersen(39) As String
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "pobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'pobiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pobiayalain can't be empty" : GoTo selesai
        End If

        'pototaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pototaltransaksi can't be empty" : GoTo selesai
        End If

        'pojmlbayar(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pojmlbayar can't be empty" : GoTo selesai
        End If

        'poinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "poinputtgl can't be empty" : GoTo selesai
        End If

        'pomodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "pomodifikasitgl can't be empty" : GoTo selesai
        End If

        'pocustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "pocustomdbl1 can't be empty" : GoTo selesai
        End If

        'pocustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "pocustomdbl2 can't be empty" : GoTo selesai
        End If

        'pocustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "pocustomdbl3 can't be empty" : GoTo selesai
        End If

        'pocustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "pocustomdate1 can't be empty" : GoTo selesai
        End If

        'pocustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "pocustomdate2 can't be empty" : GoTo selesai
        End If

        'pocustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "pocustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "poid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "polokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pojenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pojenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "posumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ponotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "posupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "posupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "po2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pobagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "potgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pouraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ponoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "potglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pomatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "podiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "porekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "postatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "poinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pomodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pomodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "poisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pocustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "poid~pocabang~polokasi~pogudang~poasalbarang~poasalbarangkategori~pojenispembelian~pojenispembeliankategori~pocarabayar~posumber~poautonotransaksi~ponotransaksi~potgl~pokodepa~posupplier~posupplierkontak~po1alamat1~po1alamat2~po1alamat3~po2alamat1~po2alamat2~po2alamat3~pobagianpembelian~potgldipenuhi~potermin~potgljatuhtempo~pouraian~pocatatan~ponoref~potglnoref~potglpenutupan~pomatauang~pokurs~pohargatermasukpajak~pototal~podiskonpersen~pojmldiskon~pototalpajak1detail~pototalpajak2detail~pobiayalainpersen~pobiayalain~pototaltransaksi~pojmlbayar~porekdiskon~porekpajak1~porekpajak2~porekbiayalain~porekbayar~poidpr~poidcs~poidrq~poidbs~postatusipc~postatusgrn~postatusri~postatusdnr~postatusprt~postatus~postatussebelumnya~pojmlrevisi~pocetakanke~poinputuser~poinputtgl~pomodifikasiuser~pomodifikasitgl~poisclose~pocustomtext1~pocustomtext2~pocustomtext3~pocustomtext4~pocustomtext5~pocustomint1~pocustomint2~pocustomint3~pocustomdbl1~pocustomdbl2~pocustomdbl3~pocustomdate1~pocustomdate2~pocustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpodetail(0) As Integer, idpo(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, costcenter(23) As String, divisi(24) As String, 
        'subdivisi(25) As String, proyek(26) As String, catatan(27) As String, urutan(28) As Integer, idprdetail(29) As Integer, 
        'idcsdetail(30) As Integer, idrqdetail(31) As Integer, idbsdetail(32) As Integer, jmlipc(33) As Double, statusipc(34) As Integer, 
        'jmlgrn(35) As Double, statusgrn(36) As Integer, jmlri(37) As Double, statusri(38) As Integer, jmldnr(39) As Double, 
        'statusdnr(40) As Integer, jmlprt(41) As Double, statusprt(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargafix", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlipc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlgrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlprt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingPR As String = "", ftOutstandingPR As String = "", updNilaiPR As String = "", updFilterPR As String = ""
        Dim ftExistOutstandingRQ As String = "", ftOutstandingRQ As String = "", updNilaiRQ As String = "", updFilterRQ As String = ""
        Dim updStokBooking As String = "", gudang As String = ""
        Dim idbarang As Integer = 0, idprdetail As Integer = 0, idrqdetail As Integer = 0, jmlbarang As Double = 0

        'FILTER RQ, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftRQ As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 53) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idpo(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpo required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargafix(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargafix required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'jmlipc(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - jmlipc required numeric." : GoTo selesai
            End If
            'statusipc(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - statusipc required numeric." : GoTo selesai
            End If
            'jmlgrn(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - jmlgrn required numeric." : GoTo selesai
            End If
            'statusgrn(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - statusgrn required numeric." : GoTo selesai
            End If
            'jmlri(37) As Double
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - jmlri required numeric." : GoTo selesai
            End If
            'statusri(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - statusri required numeric." : GoTo selesai
            End If
            'jmldnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlipc(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - jmlipc can't be empty" : GoTo selesai
            End If

            'jmlgrn(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - jmlgrn can't be empty" : GoTo selesai
            End If

            'jmlri(37) As Double
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - jmlri can't be empty" : GoTo selesai
            End If

            'jmldnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpodetail~idpo~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String       , idprdetail(29) As Integer      , idrqdetail(31) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idprdetail = dataRowDetail(29) : idrqdetail = dataRowDetail(31)

            'VALIDASI OUTSTANDING -------------------------
            If idprdetail <> 0 Then 'PR
                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPR = IIf(Len(ftExistOutstandingPR.ToString) = 0, "", ftExistOutstandingPR & " UNION ")
                ftExistOutstandingPR = String.Concat(ftExistOutstandingPR, "SELECT EXISTS(SELECT 1 FROM m4_pr_detail JOIN m4_pr ON idpr = prid WHERE idprdetail = '" & idprdetail & "' AND (prstatus = 2 OR prstatus = 3 OR prstatus = 4 OR prstatus = 7) LIMIT 1) as rowExists, '" & idprdetail & "' as idprdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                ftOutstandingPR = IIf(Len(ftOutstandingPR.ToString) = 0, "", ftOutstandingPR & " OR ")
                ftOutstandingPR = String.Concat(ftOutstandingPR, " (prd.idprdetail = " & idprdetail & " AND " & Outstanding & " > (prd.jmlbarang - prd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPR = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPR)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                updFilterPR = String.Concat(updFilterPR, "(idprdetail = '" & idprdetail & "')")
            End If

            If idrqdetail <> 0 Then 'RQ
                'CEK RQ YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftRQ = IIf(Len(ftRQ.ToString) = 0, "", ftRQ & " OR ")
                ftRQ = String.Concat(ftRQ, " (rqd.idrqdetail = " & idrqdetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingRQ = IIf(Len(ftExistOutstandingRQ.ToString) = 0, "", ftExistOutstandingRQ & " UNION ")
                ftExistOutstandingRQ = String.Concat(ftExistOutstandingRQ, "SELECT EXISTS(SELECT 1 FROM m4_rq_detail JOIN m4_rq ON idrq = rqid WHERE idrqdetail = '" & idrqdetail & "' AND (rqstatus = 2 OR rqstatus = 3 OR rqstatus = 4 OR rqstatus = 7) LIMIT 1) as rowExists, '" & idrqdetail & "' as idrqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrqdetail=" & idrqdetail)
                ftOutstandingRQ = IIf(Len(ftOutstandingRQ.ToString) = 0, "", ftOutstandingRQ & " OR ")
                ftOutstandingRQ = String.Concat(ftOutstandingRQ, " (rqd.idrqdetail = " & idrqdetail & " AND " & Outstanding & " > (rqd.jmlbarang - rqd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiRQ = String.Concat("WHEN '" & idrqdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiRQ)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                updFilterRQ = String.Concat(updFilterRQ, "(idrqdetail = '" & idrqdetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            '5. SET NILAI UPDATE STOK BOOKING
            updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA COST -------------------------------------------------------
        'idpocost(0) As Integer, idpo(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, rekdebit(6) As String, rekkredit(7) As String, kontak(8) As Integer, termasukhpp(9) As Integer, 
        'catatan(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'urutan(15) As Integer, idprcost(16) As Integer, idcscost(17) As Integer, idrqcost(18) As Integer, idbscost(19) As Integer, 
        'jumlahipc(20) As Double, statusipc(21) As Integer, jumlahgrn(22) As Double, statusgrn(23) As Integer, jumlahri(24) As Double, 
        'statusri(25) As Integer, jumlahbayar(26) As Double, statusbayar(27) As Integer, isclose(28) As Integer, customtext1(29) As String, 
        'customtext2(30) As String, customtext3(31) As String, customdbl1(32) As Double, customdbl2(33) As Double, customdbl3(34) As Double, 
        'customdate1(35) As Date, customdate2(36) As Date, customdate3(37) As Date

        'MAPPING BUAT FLEX DATA COST -----------------------------------------------------
        'idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idpocost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "kodecost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "jumlah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "termasukhpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idprcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idcscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idrqcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idbscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahipc", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahgrn", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate3", AsEnumTypeData.AsString)

        'CEK PARAMETER DATA COST
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA COST ======================================================
            'SPLIT PARAMETER DATA COST
            dataCost = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA COST ===============================================

            'VALIDASI DAN SET DATA ROW Cost ==================================================
            Dim JmlDtCost As Integer = dataCost.Length
            For i = 1 To JmlDtCost
                'SPLIT DATA Cost
                dataRowCost = dataCost(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Cost -----------------------------------
                'CEK ARRAY DATA Cost
                If (dataRowCost.Length <> 38) Then
                    result(2) = "Cost Row : " & i & " - Invalid Cost transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Cost ----------------------------

                'VALIDASI TIPE DATA Cost ------------------------------------------
                'idpocost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                End If
                'idpo(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpo required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowCost(4)) = False) Then
                    result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowCost(5)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'kontak(8) As Integer
                If (IsNumeric(dataRowCost(8)) = False) Then
                    result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                End If
                'termasukhpp(9) As Integer
                If (IsNumeric(dataRowCost(9)) = False) Then
                    result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                End If
                'urutan(15) As Integer
                If (IsNumeric(dataRowCost(15)) = False) Then
                    result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idprcost(16) As Integer
                If (IsNumeric(dataRowCost(16)) = False) Then
                    result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                End If
                'idcscost(17) As Integer
                If (IsNumeric(dataRowCost(17)) = False) Then
                    result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                End If
                'idrqcost(18) As Integer
                If (IsNumeric(dataRowCost(18)) = False) Then
                    result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                End If
                'idbscost(19) As Integer
                If (IsNumeric(dataRowCost(19)) = False) Then
                    result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                End If
                'jumlahipc(20) As Double
                If (IsNumeric(dataRowCost(20)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahipc required numeric." : GoTo selesai
                End If
                'statusipc(21) As Integer
                If (IsNumeric(dataRowCost(21)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusipc required numeric." : GoTo selesai
                End If
                'jumlahgrn(22) As Double
                If (IsNumeric(dataRowCost(22)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahgrn required numeric." : GoTo selesai
                End If
                'statusgrn(23) As Integer
                If (IsNumeric(dataRowCost(23)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusgrn required numeric." : GoTo selesai
                End If
                'jumlahri(24) As Double
                If (IsNumeric(dataRowCost(24)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahri required numeric." : GoTo selesai
                End If
                'statusri(25) As Integer
                If (IsNumeric(dataRowCost(25)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusri required numeric." : GoTo selesai
                End If
                'jumlahbayar(26) As Double
                If (IsNumeric(dataRowCost(26)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                End If
                'statusbayar(27) As Integer
                If (IsNumeric(dataRowCost(27)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                End If
                'isclose(28) As Integer
                If (IsNumeric(dataRowCost(28)) = False) Then
                    result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(32) As Double
                If (IsNumeric(dataRowCost(32)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(33) As Double
                If (IsNumeric(dataRowCost(33)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(34) As Double
                If (IsNumeric(dataRowCost(34)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(35) As Date
                If (IsDate(dataRowCost(35)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(36) As Date
                If (IsDate(dataRowCost(36)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(37) As Date
                If (IsDate(dataRowCost(37)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA Cost -----------------------------------

                'VALIDASI DATA Cost ---------------------------------------
                'kodecost(2) As String
                If Len(dataRowCost(2)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(2)) > 25 Then
                    result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                End If

                'matauang(3) As String
                If Len(dataRowCost(3)) = 0 Then
                    result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(3)) > 25 Then
                    result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowCost(4)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowCost(5)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If

                'rekdebit(6) As String
                If dataRowCost(9) = 0 Then
                    If Len(dataRowCost(6)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                    End If
                End If
                If Len(dataRowCost(6)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                End If

                'rekkredit(7) As String
                If Len(dataRowCost(7)) = 0 Then
                    result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(7)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                End If

                'jumlahipc(20) As Double
                If Len(dataRowCost(20)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahipc can't be empty" : GoTo selesai
                End If

                'jumlahgrn(22) As Double
                If Len(dataRowCost(22)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahgrn can't be empty" : GoTo selesai
                End If

                'jumlahri(24) As Double
                If Len(dataRowCost(24)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahri can't be empty" : GoTo selesai
                End If

                'jumlahbayar(26) As Double
                If Len(dataRowCost(26)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                End If

                'customdbl1(32) As Double
                If Len(dataRowCost(32)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(33) As Double
                If Len(dataRowCost(33)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(34) As Double
                If Len(dataRowCost(34)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(35) As Date
                If Len(dataRowCost(35)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(36) As Date
                If Len(dataRowCost(36)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(37) As Date
                If Len(dataRowCost(37)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA Cost --------------------------------

                If AsDataTableTambahData(dtcost, "idpocost~idpo~kodecost~matauang~kurs~jumlah~rekdebit~rekkredit~kontak~termasukhpp~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~jumlahipc~statusipc~jumlahgrn~statusgrn~jumlahri~statusri~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34) & "~" & dataRowCost(35) & "~" & dataRowCost(36) & "~" & dataRowCost(37)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================

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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("potgl")), AsFormatTanggal(drutama("potgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("postatus") = 2 Then
                    'CEK HAK AKSES
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                    Dim rsCekHakAkses As String = HakAkses(4, 7, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                    If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingPR, ftOutstandingPR, ftExistOutstandingRQ, ftOutstandingRQ, ftRQ, drutama("pohargatermasukpajak"), "", "")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("potermin").ToString, AsFormatTanggal(drutama("potgl")), "potgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("potgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("pototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("pototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("pototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("pohargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("pototaltransaksi") = Double.Parse(drutama("pototal")) - Double.Parse(drutama("pojmldiskon")) + Double.Parse(drutama("pototalpajak1detail")) + Double.Parse(drutama("pototalpajak2detail")) + Double.Parse(drutama("pobiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("pototaltransaksi") = Double.Parse(drutama("pototal")) - Double.Parse(drutama("pojmldiskon")) + Double.Parse(drutama("pobiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("poid")
                    notransaksi = drutama("ponotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(poid), ponotransaksi FROM M4_po WHERE poid='" & result(4) & "' AND postatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(poid) FROM m4_po WHERE ponotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_po_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Po_HistorySimpan("" & paramSplit(0) & "★M4_Po_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("posumber")) & "▼" & FixQuotes(drutama("poid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Po set pocabang  = '" & FixQuotes(drutama("pocabang")) & "', polokasi  = '" & FixQuotes(drutama("polokasi")) & "', pogudang  = '" & FixQuotes(drutama("pogudang")) & "', poasalbarang  = '" & FixQuotes(drutama("poasalbarang")) & "', poasalbarangkategori  = " & drutama("poasalbarangkategori") & ", pojenispembelian  = '" & FixQuotes(drutama("pojenispembelian")) & "', pojenispembeliankategori  = " & drutama("pojenispembeliankategori") & ", pocarabayar  = " & drutama("pocarabayar") & ", posumber  = '" & FixQuotes(drutama("posumber")) & "', poautonotransaksi  = " & drutama("poautonotransaksi") & ", ponotransaksi  = '" & notransaksi & "', potgl  = '" & FixQuotes(AsFormatTanggal(drutama("potgl"))) & "', pokodepa  = " & drutama("pokodepa") & ", posupplier  = " & drutama("posupplier") & ", posupplierkontak  = '" & FixQuotes(drutama("posupplierkontak")) & "', po1alamat1  = '" & FixQuotes(drutama("po1alamat1")) & "', po1alamat2  = '" & FixQuotes(drutama("po1alamat2")) & "', po1alamat3  = '" & FixQuotes(drutama("po1alamat3")) & "', po2alamat1  = '" & FixQuotes(drutama("po2alamat1")) & "', po2alamat2  = '" & FixQuotes(drutama("po2alamat2")) & "', po2alamat3  = '" & FixQuotes(drutama("po2alamat3")) & "', pobagianpembelian  = " & drutama("pobagianpembelian") & ", potgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("potgldipenuhi"))) & "', potermin  = '" & FixQuotes(drutama("potermin")) & "', potgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("potgljatuhtempo"))) & "', pouraian  = '" & FixQuotes(drutama("pouraian")) & "', pocatatan  = '" & FixQuotes(drutama("pocatatan")) & "', ponoref  = '" & FixQuotes(drutama("ponoref")) & "', potglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("potglnoref"))) & "', potglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("potglpenutupan"))) & "', pomatauang  = '" & FixQuotes(drutama("pomatauang")) & "', pokurs  = '" & FixDouble(drutama("pokurs")) & "', pohargatermasukpajak  = " & drutama("pohargatermasukpajak") & ", pototal  = '" & FixDouble(drutama("pototal")) & "', podiskonpersen  = '" & FixQuotes(drutama("podiskonpersen")) & "', pojmldiskon  = '" & FixDouble(drutama("pojmldiskon")) & "', pototalpajak1detail  = '" & FixDouble(drutama("pototalpajak1detail")) & "', pototalpajak2detail  = '" & FixDouble(drutama("pototalpajak2detail")) & "', pobiayalainpersen  = '" & FixQuotes(drutama("pobiayalainpersen")) & "', pobiayalain  = '" & FixDouble(drutama("pobiayalain")) & "', pototaltransaksi  = '" & FixDouble(drutama("pototaltransaksi")) & "', pojmlbayar  = '" & FixDouble(drutama("pojmlbayar")) & "', porekdiskon  = '" & FixQuotes(drutama("porekdiskon")) & "', porekpajak1  = '" & FixQuotes(drutama("porekpajak1")) & "', porekpajak2  = '" & FixQuotes(drutama("porekpajak2")) & "', porekbiayalain  = '" & FixQuotes(drutama("porekbiayalain")) & "', porekbayar  = '" & FixQuotes(drutama("porekbayar")) & "', poidpr  = " & drutama("poidpr") & ", poidcs  = " & drutama("poidcs") & ", poidrq  = " & drutama("poidrq") & ", poidbs  = " & drutama("poidbs") & ", postatusipc  = " & drutama("postatusipc") & ", postatusgrn  = " & drutama("postatusgrn") & ", postatusri  = " & drutama("postatusri") & ", postatusdnr  = " & drutama("postatusdnr") & ", postatusprt  = " & drutama("postatusprt") & ", postatus  = " & drutama("postatus") & ", postatussebelumnya  = " & drutama("postatussebelumnya") & ", pojmlrevisi  = pojmlrevisi+1, pocetakanke  = " & drutama("pocetakanke") & ", pomodifikasiuser  = " & drutama("pomodifikasiuser") & ", pomodifikasitgl  = NOW(), pocustomtext1  = '" & FixQuotes(drutama("pocustomtext1")) & "', pocustomtext2  = '" & FixQuotes(drutama("pocustomtext2")) & "', pocustomtext3  = '" & FixQuotes(drutama("pocustomtext3")) & "', pocustomtext4  = '" & FixQuotes(drutama("pocustomtext4")) & "', pocustomtext5  = '" & FixQuotes(drutama("pocustomtext5")) & "', pocustomint1  = " & drutama("pocustomint1") & ", pocustomint2  = " & drutama("pocustomint2") & ", pocustomint3  = " & drutama("pocustomint3") & ", pocustomdbl1  = '" & FixDouble(drutama("pocustomdbl1")) & "', pocustomdbl2  = '" & FixDouble(drutama("pocustomdbl2")) & "', pocustomdbl3  = '" & FixDouble(drutama("pocustomdbl3")) & "', pocustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate1"))) & "', pocustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate2"))) & "', pocustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate3"))) & "' where poid = '" & drutama("poid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("poautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pocabang"), drutama("polokasi"), drutama("posumber"), drutama("potgl"))
                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksi(0) = 1) Then
                            notransaksi = arrNotransaksi(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNotransaksi(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOTRANSAKSI ==================================

                    Else
                        notransaksi = drutama("ponotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(poid) FROM m4_po WHERE ponotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Po (pocabang, polokasi, pogudang, poasalbarang, poasalbarangkategori, pojenispembelian, pojenispembeliankategori, pocarabayar, posumber, poautonotransaksi, ponotransaksi, potgl, pokodepa, posupplier, posupplierkontak, po1alamat1, po1alamat2, po1alamat3, po2alamat1, po2alamat2, po2alamat3, pobagianpembelian, potgldipenuhi, potermin, potgljatuhtempo, pouraian, pocatatan, ponoref, potglnoref, potglpenutupan, pomatauang, pokurs, pohargatermasukpajak, pototal, podiskonpersen, pojmldiskon, pototalpajak1detail, pototalpajak2detail, pobiayalainpersen, pobiayalain, pototaltransaksi, pojmlbayar, porekdiskon, porekpajak1, porekpajak2, porekbiayalain, porekbayar, poidpr, poidcs, poidrq, poidbs, postatusipc, postatusgrn, postatusri, postatusdnr, postatusprt, postatus, postatussebelumnya, pojmlrevisi, pocetakanke, poinputuser, poinputtgl, pomodifikasiuser, pomodifikasitgl, poisclose, pocustomtext1, pocustomtext2, pocustomtext3, pocustomtext4, pocustomtext5, pocustomint1, pocustomint2, pocustomint3, pocustomdbl1, pocustomdbl2, pocustomdbl3, pocustomdate1, pocustomdate2, pocustomdate3) values('" & FixQuotes(drutama("pocabang")) & "', '" & FixQuotes(drutama("polokasi")) & "', '" & FixQuotes(drutama("pogudang")) & "', '" & FixQuotes(drutama("poasalbarang")) & "', " & drutama("poasalbarangkategori") & ", '" & FixQuotes(drutama("pojenispembelian")) & "', " & drutama("pojenispembeliankategori") & ", " & drutama("pocarabayar") & ", '" & FixQuotes(drutama("posumber")) & "', " & drutama("poautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("potgl"))) & "', " & drutama("pokodepa") & ", " & drutama("posupplier") & ", '" & FixQuotes(drutama("posupplierkontak")) & "', '" & FixQuotes(drutama("po1alamat1")) & "', '" & FixQuotes(drutama("po1alamat2")) & "', '" & FixQuotes(drutama("po1alamat3")) & "', '" & FixQuotes(drutama("po2alamat1")) & "', '" & FixQuotes(drutama("po2alamat2")) & "', '" & FixQuotes(drutama("po2alamat3")) & "', " & drutama("pobagianpembelian") & ", '" & FixQuotes(AsFormatTanggal(drutama("potgldipenuhi"))) & "', '" & FixQuotes(drutama("potermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("potgljatuhtempo"))) & "', '" & FixQuotes(drutama("pouraian")) & "', '" & FixQuotes(drutama("pocatatan")) & "', '" & FixQuotes(drutama("ponoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("potglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("potglpenutupan"))) & "', '" & FixQuotes(drutama("pomatauang")) & "', '" & FixDouble(drutama("pokurs")) & "', " & drutama("pohargatermasukpajak") & ", '" & FixDouble(drutama("pototal")) & "', '" & FixQuotes(drutama("podiskonpersen")) & "', '" & FixDouble(drutama("pojmldiskon")) & "', '" & FixDouble(drutama("pototalpajak1detail")) & "', '" & FixDouble(drutama("pototalpajak2detail")) & "', '" & FixQuotes(drutama("pobiayalainpersen")) & "', '" & FixDouble(drutama("pobiayalain")) & "', '" & FixDouble(drutama("pototaltransaksi")) & "', '" & FixDouble(drutama("pojmlbayar")) & "', '" & FixQuotes(drutama("porekdiskon")) & "', '" & FixQuotes(drutama("porekpajak1")) & "', '" & FixQuotes(drutama("porekpajak2")) & "', '" & FixQuotes(drutama("porekbiayalain")) & "', '" & FixQuotes(drutama("porekbayar")) & "', " & drutama("poidpr") & ", " & drutama("poidcs") & ", " & drutama("poidrq") & ", " & drutama("poidbs") & ", " & drutama("postatusipc") & ", " & drutama("postatusgrn") & ", " & drutama("postatusri") & ", " & drutama("postatusdnr") & ", " & drutama("postatusprt") & ", " & drutama("postatus") & ", " & drutama("postatussebelumnya") & ", " & drutama("pojmlrevisi") & ", " & drutama("pocetakanke") & ", " & drutama("poinputuser") & ", NOW(), " & drutama("pomodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("poisclose") & ", '" & FixQuotes(drutama("pocustomtext1")) & "', '" & FixQuotes(drutama("pocustomtext2")) & "', '" & FixQuotes(drutama("pocustomtext3")) & "', '" & FixQuotes(drutama("pocustomtext4")) & "', '" & FixQuotes(drutama("pocustomtext5")) & "', " & drutama("pocustomint1") & ", " & drutama("pocustomint2") & ", " & drutama("pocustomint3") & ", '" & FixDouble(drutama("pocustomdbl1")) & "', '" & FixDouble(drutama("pocustomdbl2")) & "', '" & FixDouble(drutama("pocustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("pocustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select poid from M4_po where ponotransaksi='" & notransaksi & "' AND poinputuser= '" & userid & "' order by pomodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Po_Detail where idpo = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Po_Detail(idpodetail, idpo, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Po_Cost where idpo = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses cost
                If (dtcost.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtcost.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idpocost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', " & dr1("kontak") & ", " & dr1("termasukhpp") & ", '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", '" & FixDouble(dr1("jumlahipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jumlahgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jumlahri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Po_Cost(idpocost, idpo, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("postatus") = 2 Then
                    If Len(updNilaiPR) > 0 Then 'PR
                        'UPDATE DETAIL
                        sql = "UPDATE m4_pr_detail SET jmlrealisasi = (CASE idprdetail " & updNilaiPR & " ELSE jmlrealisasi END) WHERE " & updFilterPR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpr FROM M4_pr_detail WHERE " & updFilterPR & " GROUP BY idpr")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPR = "" : updFilterPR = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPR = String.Concat(updNilaiPR, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                                updFilterPR = String.Concat(updFilterPR, "(prid = '" & dr1("idpr") & "')")
                            Next

                            sql = "UPDATE m4_pr SET prstatusrealisasi = (CASE prid " & updNilaiPR & " ELSE prstatusrealisasi END) WHERE " & updFilterPR
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    If Len(updNilaiRQ) > 0 Then 'RQ
                        'UPDATE DETAIL
                        sql = "UPDATE m4_rq_detail SET jmlrealisasi = (CASE idrqdetail " & updNilaiRQ & " ELSE jmlrealisasi END) WHERE " & updFilterRQ
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idrq FROM m4_rq_detail WHERE " & updFilterRQ & " GROUP BY idrq")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idrq = '" & dr1("idrq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idrq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_rq_detail WHERE " & ftDetail & " GROUP BY idrq")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiRQ = "" : updFilterRQ = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiRQ = String.Concat(updNilaiRQ, "WHEN '" & dr1("idrq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                                updFilterRQ = String.Concat(updFilterRQ, "(rqid = '" & dr1("idrq") & "')")
                            Next

                            sql = "UPDATE m4_rq SET rqstatusrealisasi = (CASE rqid " & updNilaiRQ & " ELSE rqstatusrealisasi END) WHERE " & updFilterRQ
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    'UPDATE STOK BOOKING ================================================================
                    If Len(updStokBooking) > 0 Then
                        sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK BOOKING =========================================================

                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                'ambil moduleid dan menuid dari m0_nomor
                Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'")
                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

                sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF INSERT USER LOG =============================================================

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PoUpdateStatusOld(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", nilaiStatus As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"
        Dim idtransaksi As String = "", idtransaksih As String = ""
        Dim dtdetail As DataTable
        Dim isDelete As Boolean = False

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""

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
            Filter = Filter.Replace("posupplierkode", "c1.kkode")
            Filter = Filter.Replace("posuppliernama", "c1.knama")
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

        'VALIDASI DAN SET ISDELETE =========================================================
        'CEK ISDELETE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isdelete required numeric." : GoTo selesai
        Else
            'SET ISDELETE
            If (Val(paramSplit(4)) = 1) Then
                isDelete = True
            Else
                isDelete = False
            End If
        End If
        'END OF VALIDASI DAN SET ISDELETE ==================================================

        'VALIDASI DAN SET NILAISTATUS ======================================================
        'SPILIT PARAMETER NILAISTATUS
        nilaiSplit = paramSplit(5).Split(sptSubParam)

        'CEK ARRAY NILAISTATUS
        If (nilaiSplit.Length <> 2) Then
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If

        'CEK IDTRANSAKSI
        If (IsNumeric(nilaiSplit(0)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = nilaiSplit(0)

        'SET NILAI STATUS
        If (Len(nilaiSplit(1)) > 0) Then
            'JIKA NUMERIC MAKA NILAISTATUS = PARAM NILAI STATUS YG DIINPUT
            'JIKA TIDAK MAKA NILAISTATUS = UNCLOSE
            If (IsNumeric(nilaiSplit(1)) = True) Then
                nilaiStatus = nilaiSplit(1)
                'JIKA NILAI STATUS < 0 ATAU NILAI STATUS > 12 MAKA NILAISTATUS TIDAK VALID
                If (nilaiStatus < 0 Or nilaiStatus > 12) Then
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            Else
                If (nilaiSplit(1).ToString.ToLower = "unclose") Then
                    nilaiStatus = "unclose"
                Else
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            End If
        Else
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET NILAISTATUS ================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Po", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Potgl, Ponotransaksi, Postatus FROM M4_Po WHERE Poid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Postatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m4_po_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Po_HistorySimpan("" & paramSplit(0) & "★M4_Po_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m4_po_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0, idrqdetail As Integer = 0
                Dim updNilaiPR As String = "", updFilterPR As String = "", updNilaiRQ As String = "", updFilterRQ As String = ""
                Dim gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idprdetail, idrqdetail, urutan FROM m4_po_detail WHERE idpo = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idprdetail = dr1("idprdetail") : idrqdetail = dr1("idrqdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idprdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idprdetail=" & idprdetail)
                            updNilaiPR = String.Concat("WHEN '" & idprdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPR)
                            '2. SET FILTERUPDATE OUTSTANDING PR
                            updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                            updFilterPR = String.Concat(updFilterPR, "(idprdetail = '" & idprdetail & "')")
                        End If

                        If idrqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING RQ
                            Dim OutstandingRQ As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrqdetail=" & idrqdetail)
                            updNilaiRQ = String.Concat("WHEN '" & idrqdetail & "' THEN ROUND(jmlrealisasi - '" & OutstandingRQ & "', 5) ", updNilaiRQ)
                            '2. SET FILTERUPDATE OUTSTANDING RQ
                            updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                            updFilterRQ = String.Concat(updFilterRQ, "(idrqdetail = '" & idrqdetail & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                        '3. SET NILAI UPDATE STOK BOOKING KELUAR -------------
                        updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterPR) > 0 Then 'PR
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_pr_detail SET jmlrealisasi = (CASE idprdetail " & updNilaiPR & " ELSE jmlrealisasi END) WHERE " & updFilterPR
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpr FROM M4_pr_detail WHERE " & updFilterPR & " GROUP BY idpr")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpr = '" & dr1("idpr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idpr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_pr_detail WHERE " & ftDetail & " GROUP BY idpr")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPR = "" : updFilterPR = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiPR = String.Concat(updNilaiPR, "WHEN '" & dr1("idpr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPR = IIf(Len(updFilterPR.ToString) = 0, "", updFilterPR & " OR ")
                            updFilterPR = String.Concat(updFilterPR, "(prid = '" & dr1("idpr") & "')")
                        Next

                        sql = "UPDATE m4_pr SET prstatusrealisasi = (CASE prid " & updNilaiPR & " ELSE prstatusrealisasi END) WHERE " & updFilterPR
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If

                If Len(updFilterRQ) > 0 Then 'RQ
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m4_rq_detail SET jmlrealisasi = (CASE idrqdetail " & updNilaiRQ & " ELSE jmlrealisasi END) WHERE " & updFilterRQ
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA --------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idrq FROM m4_rq_detail WHERE " & updFilterRQ & " GROUP BY idrq")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idrq = '" & dr1("idrq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idrq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_rq_detail WHERE " & ftDetail & " GROUP BY idrq")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiRQ = "" : updFilterRQ = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiRQ = String.Concat(updNilaiRQ, "WHEN '" & dr1("idrq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterRQ = IIf(Len(updFilterRQ.ToString) = 0, "", updFilterRQ & " OR ")
                            updFilterRQ = String.Concat(updFilterRQ, "(rqid = '" & dr1("idrq") & "')")
                        Next

                        sql = "UPDATE m4_rq SET rqstatusrealisasi = (CASE rqid " & updNilaiRQ & " ELSE rqstatusrealisasi END) WHERE " & updFilterRQ
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================

                'UPDATE STOK BOOKING ================================
                If Len(updStokBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =========================

            End If

            'update status utama
            sql = "UPDATE M4_Po SET Postatus = " & nilaiStatus & ", Pomodifikasiuser='" & userid & "', Pomodifikasitgl = NOW(), Poposting = 0, Popostingtgl = '1971-01-01 00:00:00', Pojmlrevisi = Pojmlrevisi + 1 WHERE Poid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M4_PoSearch(PostWsSearch(paramSplit(0), "M4_PoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'UPDATE OF SIMPAN KE DATABASE ==========================================================

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
    Public Function M4_PoDeleteOld(ByVal param As String) As String

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

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

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
            Filter = Filter.Replace("posupplierkode", "c1.kkode")
            Filter = Filter.Replace("posuppliernama", "c1.knama")
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
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
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
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Po", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Poid, Ponotransaksi FROM M4_Po WHERE Poid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pocabang, polokasi, posumber, poautonotransaksi, ponotransaksi, potgl"
            sql &= " FROM M4_po"
            sql &= " WHERE poid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pocabang")
                lokasi = dtNomorNext.Rows(0)("polokasi")
                sumber = dtNomorNext.Rows(0)("posumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("poautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ponotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("potgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE COST
            sql = "DELETE FROM M4_po_Cost WHERE idpo ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Po_Detail WHERE idpo = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Po WHERE poid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'UPDATE NOMOR BERIKUTNYA ============================================================
            'JIKA AUTO NO. TRANSAKSI
            If autonotransaksi = 1 Then
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi)
                Dim arrNomorNext(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                arrNomorNext = rsNomorNext.Split(sptSubParam)
                'Cek success M0_DeleteNotransaksi
                If (arrNomorNext(0) = 1) Then
                    sql = arrNomorNext(3)
                    'Tambah query update m0_nomor_next
                    If Len(sql) > 0 Then
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                Else
                    result(2) = arrNomorNext(1) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF UPDATE NOMOR BERIKUTNYA =====================================================


            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M4_PoSearch(PostWsSearch(paramSplit(0), "M4_PoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

End Class